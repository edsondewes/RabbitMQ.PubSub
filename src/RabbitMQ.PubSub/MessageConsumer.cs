using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

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

        public IMessageSubscription Subscribe<T>(
            IEnumerable<string> routingKeys,
            Func<T, Task> callback,
            string exchange = null,
            string queue = null,
            int maxDegreeOfParallelism = 1)
        {
            if (exchange == null)
                exchange = _config.DefaultExchange;

            EnsureExchangeCreated(exchange);

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

            return new DataFlowSubscription<T>(_model, queueName, callback, maxDegreeOfParallelism);
        }

        private void EnsureExchangeCreated(string name)
        {
            _model.ExchangeDeclare(name, ExchangeType.Topic, durable: true, autoDelete: false);
        }
    }
}
