using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.PubSub.Diagnostics;

namespace RabbitMQ.PubSub
{
    public class MessageConsumer : IMessageConsumer
    {
        private readonly ConfigRabbitMQ _config;
        private readonly IConnection _connection;
        private readonly ILogger<MessageConsumer> _logger;
        private readonly ISerializationManager _serialization;

        public MessageConsumer(IOptions<ConfigRabbitMQ> config,
            IConnectionHelper connectionFactory,
            ISerializationManager serialization,
            ILogger<MessageConsumer> logger)
        {
            _config = config.Value;
            _connection = connectionFactory.CreateConnection(_config);
            _serialization = serialization;
            _logger = logger;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public ISubscription Subscribe<T>(Func<T, MessageContext, Task> callback, SubscriptionOptions options = null)
        {
            var autoAck = options?.AutoAck ?? true;
            var exchange = options?.Exchange ?? _config.DefaultExchange;

            var model = CreateModel();
            EnsureExchangeCreated(model, exchange, options.ExchangeType());
            var queueName = EnsureQueueCreated(model, options?.RoutingKeys, exchange, options?.Queue);

            var consumer = new AsyncEventingBasicConsumer(model);
            var consumerTag = model.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
            consumer.Received += async (_, eventArgs) =>
            {
                var context = new MessageContext(eventArgs, autoAck, model);
                var activity = MessageDiagnostics.StartMessageIn(context);

                try
                {
                    var obj = _serialization.Deserialize<T>(eventArgs.Body, eventArgs.BasicProperties.ContentType);
                    await callback(obj, context);
                }
                finally
                {
                    MessageDiagnostics.StopMessageIn(activity, context);
                }
            };

            return new SubscriptionImpl(model, consumerTag);
        }

        private void CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, "A message could not be processed");
        }

        private IModel CreateModel()
        {
            var model = _connection.CreateModel();
            model.CallbackException += CallbackException;

            if (_config.PrefetchCount.HasValue)
                model.BasicQos(0, _config.PrefetchCount.Value, global: false);

            return model;
        }

        private void EnsureExchangeCreated(IModel model, string name, string type)
        {
            model.ExchangeDeclare(name, type, durable: true, autoDelete: false);
        }

        private string EnsureQueueCreated(IModel model, IEnumerable<string> routingKeys, string exchange, string queue)
        {
            var args = _config.LazyQueues
                ? new Dictionary<string, object> { { "x-queue-mode", "lazy" } }
                : null;

            var queueName = model.QueueDeclare(
                queue: queue ?? string.Empty,
                autoDelete: !_config.DurableQueues || string.IsNullOrEmpty(queue),
                durable: _config.DurableQueues,
                exclusive: false,
                arguments: args).QueueName;

            if (routingKeys == null || !routingKeys.Any())
                routingKeys = new[] { string.Empty };

            foreach (var routingKey in routingKeys)
            {
                model.QueueBind(
                    queue: queueName,
                    exchange: exchange,
                    routingKey: routingKey);
            }

            return queueName;
        }
    }
}
