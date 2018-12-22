using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.PubSub
{
    public class MessageConsumer : IMessageConsumer
    {
        private readonly ConfigRabbitMQ _config;
        private readonly IConnection _connection;
        private readonly ILogger<MessageConsumer> _logger;
        private readonly IModel _model;
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

            _model = _connection.CreateModel();
            _model.CallbackException += CallbackException;
        }

        public void Dispose()
        {
            _model.Dispose();
            _connection.Dispose();
        }

        public ISubscription Subscribe<T>(Action<T, MessageContext> callback, SubscriptionOptions options = null)
        {
            var exchange = options?.Exchange ?? _config.DefaultExchange;

            EnsureExchangeCreated(exchange, options.ExchangeType());
            var queueName = EnsureQueueCreated(options?.RoutingKeys, exchange, options?.Queue);

            var consumer = new EventingBasicConsumer(_model);
            var consumerTag = _model.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            consumer.Received += (_, eventArgs) =>
            {
                var serializer = _serialization.GetSerializer(eventArgs.BasicProperties.ContentType);
                var obj = serializer.Deserialize<T>(eventArgs.Body);
                var context = new MessageContext(eventArgs.BasicProperties);

                callback(obj, context);
            };

            return new SubscriptionImpl(_model, consumerTag);
        }

        private void CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, "A message could not be processed");
        }

        private void EnsureExchangeCreated(string name, string type)
        {
            _model.ExchangeDeclare(name, type, durable: true, autoDelete: false);
        }

        private string EnsureQueueCreated(IEnumerable<string> routingKeys, string exchange, string queue)
        {
            var queueName = _model.QueueDeclare(
                queue: queue ?? string.Empty,
                autoDelete: !_config.DurableQueues || string.IsNullOrEmpty(queue),
                durable: _config.DurableQueues,
                exclusive: false).QueueName;

            if (routingKeys == null || !routingKeys.Any())
                routingKeys = new[] { string.Empty };

            foreach (var routingKey in routingKeys)
            {
                _model.QueueBind(
                    queue: queueName,
                    exchange: exchange,
                    routingKey: routingKey);
            }

            return queueName;
        }
    }
}
