using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.PubSub
{
    public class MessageConsumer : IMessageConsumer
    {
        private readonly ConfigRabbitMQ _config;
        private readonly IConnection _connection;
        private readonly IModel _model;

        public MessageConsumer(IOptions<ConfigRabbitMQ> config, IConnectionHelper connectionFactory)
        {
            _config = config.Value;
            _connection = connectionFactory.TryCreateConnection(_config.Host);
            _model = _connection.CreateModel();
        }

        public void Dispose()
        {
            _model.Dispose();
            _connection.Dispose();
        }

        public ISubscription Subscribe(Action<byte[]> callback, SubscriptionOptions options = null)
        {
            var exchange = options?.Exchange ?? _config.DefaultExchange;

            EnsureExchangeCreated(exchange);
            var queueName = EnsureQueueCreated(options?.RoutingKeys, exchange, options?.Queue);

            var consumer = new EventingBasicConsumer(_model);
            var consumerTag = _model.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            consumer.Received += (_, eventArgs) => callback(eventArgs.Body);

            return new BasicSubscription(_model, consumerTag);
        }

        public ISubscription Subscribe(IConsumerStrategy strategy, SubscriptionOptions options = null)
        {
            return Subscribe(strategy.Consume, options);
        }

        private void EnsureExchangeCreated(string name)
        {
            _model.ExchangeDeclare(name, ExchangeType.Topic, durable: true, autoDelete: false);
        }

        private string EnsureQueueCreated(IEnumerable<string> routingKeys, string exchange, string queue)
        {
            var queueName = _model.QueueDeclare(
                queue: queue ?? string.Empty,
                autoDelete: !_config.DurableQueues || string.IsNullOrEmpty(queue),
                durable: _config.DurableQueues,
                exclusive: false).QueueName;

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
