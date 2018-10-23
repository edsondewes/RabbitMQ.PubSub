using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace RabbitMQ.PubSub
{
    public class MessageProducer : IMessageProducer
    {
        private readonly IConnection _connection;
        private readonly ConfigRabbitMQ _config;
        private readonly IModel _model;
        private readonly ActionBlock<PublishCommand[]> _publishBlock;
        private readonly ISerializationManager _serialization;

        public MessageProducer(IOptions<ConfigRabbitMQ> config, IConnectionHelper connectionFactory, ISerializationManager serialization)
        {
            _config = config.Value;
            _connection = connectionFactory.TryCreateConnection(_config.Host);
            _model = _connection.CreateModel();
            _serialization = serialization;
            
            _publishBlock = new ActionBlock<PublishCommand[]>(Send, new ExecutionDataflowBlockOptions
            {
                SingleProducerConstrained = true
            });
        }

        public Task Complete()
        {
            _publishBlock.Complete();
            return _publishBlock.Completion;
        }

        public void Dispose()
        {
            _model.Dispose();
            _connection.Dispose();
        }

        public Task Publish<T>(T obj, string routingKey = null, string exchange = null, string mimeType = null)
        {
            return Publish<T>(new[] { obj }, routingKey, exchange, mimeType);
        }

        public Task Publish<T>(IEnumerable<T> obj, string routingKey = null, string exchange = null, string mimeType = null)
        {
            var serializer = _serialization.GetSerializer(mimeType);
            var properties = _model.CreateBasicProperties();
            properties.Persistent = _config.PersistentDelivery;
            properties.ContentType = serializer.MimeType;

            var commands = obj.Select(message => new PublishCommand
            {
                Body = serializer.Serialize(message),
                Exchange = exchange ?? _config.DefaultExchange,
                Properties = properties,
                RoutingKey = routingKey ?? string.Empty
            }).ToArray();

            return _publishBlock.SendAsync(commands);
        }

        private void Send(PublishCommand[] commands)
        {
            switch (commands.Length)
            {
                case 0:
                    return;
                case 1:
                    SendSingle(commands[0]);
                    return;
                default:
                    SendBatch(commands);
                    return;
            }
        }

        private void SendSingle(PublishCommand command)
        {
            _model.BasicPublish(
                basicProperties: command.Properties,
                body: command.Body,
                exchange: command.Exchange,
                routingKey: command.RoutingKey
                );
        }

        private void SendBatch(PublishCommand[] commands)
        {
            var batch = _model.CreateBasicPublishBatch();
            foreach (var command in commands)
            {
                batch.Add(exchange: command.Exchange,
                    body: command.Body,
                    mandatory: false,
                    properties: command.Properties,
                    routingKey: command.RoutingKey
                    );
            }

            batch.Publish();
        }
    }
}
