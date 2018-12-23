using System.Collections.Generic;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

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
            var context = CreateContext<T>(options);
            
            _model.BasicPublish(
                basicProperties: context.Properties,
                body: context.Serialize(obj),
                exchange: context.Exchange,
                routingKey: context.RoutingKey
                );
        }

        public void Publish<T>(IEnumerable<T> enumerable, PublishOptions options)
        {
            var context = CreateContext<T>(options);

            var batch = _model.CreateBasicPublishBatch();
            foreach (var obj in enumerable)
            {
                var body = context.Serialize(obj);

                batch.Add(exchange: context.Exchange,
                    body: body,
                    mandatory: false,
                    properties: context.Properties,
                    routingKey: context.RoutingKey
                    );
            }

            batch.Publish();
        }

        private PublishContext<T> CreateContext<T>(PublishOptions options)
        {
            var serializer = _serialization.GetSerializer(options?.MimeType);
            var properties = _model.CreateBasicProperties();
            properties.Persistent = _config.PersistentDelivery;
            properties.ContentType = serializer.MimeType;
            properties.Headers = options?.Headers;

            var exchange = options?.Exchange ?? _config.DefaultExchange;
            var routingKey = options?.RoutingKey ?? string.Empty;

            return new PublishContext<T>(
                exchange: exchange,
                properties: properties,
                routingKey: routingKey,
                serializer: serializer
                );
        }
        
        private readonly struct PublishContext<T>
        {
            private readonly ISerializer _serializer;

            public string Exchange { get; }
            public IBasicProperties Properties { get; }
            public string RoutingKey { get; }

            public PublishContext(IBasicProperties properties, ISerializer serializer, string exchange, string routingKey)
            {
                Properties = properties;
                Exchange = exchange;
                RoutingKey = routingKey;

                _serializer = serializer;
            }

            public byte[] Serialize(T obj) => _serializer.Serialize(obj);
        }
    }
}
