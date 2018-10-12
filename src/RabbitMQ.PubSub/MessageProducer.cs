using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MessagePack;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace RabbitMQ.PubSub
{
    public class MessageProducer : IMessageProducer
    {
        private readonly ConfigRabbitMQ _config;
        private readonly IBasicProperties _defaultProperties;

        private readonly IConnection _connection;
        private readonly IModel _model;

        private readonly ActionBlock<PublishCommand> _publishBlock;

        public MessageProducer(IOptions<ConfigRabbitMQ> config, IConnectionHelper connectionFactory)
        {
            _config = config.Value;
            _connection = connectionFactory.TryCreateConnection(_config.Host);
            _model = _connection.CreateModel();

            _defaultProperties = _model.CreateBasicProperties();
            _defaultProperties.Persistent = _config.PersistentDelivery;

            var blockOptions = new ExecutionDataflowBlockOptions
            {
                SingleProducerConstrained = true
            };

            Action<PublishCommand> publishAction = (command) =>
            {
                _model.BasicPublish(
                    exchange: command.Exchange,
                    routingKey: command.RoutingKey,
                    basicProperties: command.Properties,
                    body: command.Body);
            };

            _publishBlock = new ActionBlock<PublishCommand>(publishAction, blockOptions);
        }

        public void Dispose()
        {
            _publishBlock.Complete();
            _model.Dispose();
            _connection.Dispose();
        }

        public Task Publish<T>(T obj, string routingKey = null, string exchange = null)
        {
            return _publishBlock.SendAsync(new PublishCommand
            {
                Body = MessagePackSerializer.Serialize(obj),
                Exchange = exchange ?? _config.DefaultExchange,
                Properties = _defaultProperties,
                RoutingKey = routingKey ?? string.Empty
            });
        }
    }
}
