using System.Collections.Generic;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.PubSub.Diagnostics;

namespace RabbitMQ.PubSub
{
    public class MessageProducer : IMessageProducer
    {
        private readonly IConnection _connection;
        private readonly ConfigRabbitMQ _config;
        private readonly IModel _model;
        private readonly ISerializationManager _serialization;

        public MessageProducer(IOptions<ConfigRabbitMQ> config, IConnectionHelper connectionFactory, ISerializationManager serialization)
        {
            _config = config.Value;
            _connection = connectionFactory.CreateConnection(_config);
            _model = _connection.CreateModel();
            _serialization = serialization;
        }

        public void Dispose()
        {
            _model.Dispose();
            _connection.Dispose();
        }

        public void Publish<T>(T obj, PublishOptions options)
        {
            var activity = MessageDiagnostics.StartMessageOut(options);
            try
            {
                var context = CreateContext<T>(options);

                _model.BasicPublish(
                    basicProperties: context.Properties,
                    body: _serialization.Serialize(obj, context.Properties.ContentType),
                    exchange: context.Exchange,
                    routingKey: context.RoutingKey
                    );
            }
            finally
            {
                MessageDiagnostics.StopMessageOut(activity, options);
            }
        }

        public void PublishBatch<T>(IEnumerable<T> enumerable, PublishOptions options)
        {
            var activity = MessageDiagnostics.StartMessageOut(options);
            try
            {
                var context = CreateContext<T>(options);

                var batch = _model.CreateBasicPublishBatch();
                var messages = _serialization.SerializeBatch(enumerable, context.Properties.ContentType);
                foreach (var obj in messages)
                {
                    batch.Add(
                        exchange: context.Exchange,
                        body: obj,
                        mandatory: false,
                        properties: context.Properties,
                        routingKey: context.RoutingKey
                        );
                }

                batch.Publish();
            }
            finally
            {
                MessageDiagnostics.StopMessageOut(activity, options);
            }
        }

        private PublishContext<T> CreateContext<T>(PublishOptions options)
        {
            var properties = _model.CreateBasicProperties();
            properties.Persistent = _config.PersistentDelivery;
            properties.ContentType = options?.MimeType ?? _serialization.DefaultMimeType;
            properties.Headers = options?.Headers;

            var exchange = options?.Exchange ?? _config.DefaultExchange;
            var routingKey = options?.RoutingKey ?? string.Empty;

            return new PublishContext<T>(
                exchange: exchange,
                properties: properties,
                routingKey: routingKey
                );
        }

        private readonly struct PublishContext<T>
        {
            public string Exchange { get; }
            public IBasicProperties Properties { get; }
            public string RoutingKey { get; }

            public PublishContext(IBasicProperties properties, string exchange, string routingKey)
            {
                Properties = properties;
                Exchange = exchange;
                RoutingKey = routingKey;
            }
        }
    }
}
