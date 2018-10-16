using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly ActionBlock<PublishCommand[]> _publishBlock;

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

            Action<PublishCommand[]> publishAction = (commands) =>
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
            };

            _publishBlock = new ActionBlock<PublishCommand[]>(publishAction, blockOptions);
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

        public Task Publish<T>(T obj, string routingKey = null, string exchange = null)
        {
            return _publishBlock.SendAsync(new[] {
                new PublishCommand
                {
                    Body = MessagePackSerializer.Serialize(obj),
                    Exchange = exchange ?? _config.DefaultExchange,
                    Properties = _defaultProperties,
                    RoutingKey = routingKey ?? string.Empty
                }
            });
        }

        public Task Publish<T>(IEnumerable<T> obj, string routingKey = null, string exchange = null)
        {
            var commands = obj.Select(c => new PublishCommand
            {
                Body = MessagePackSerializer.Serialize(c),
                Exchange = exchange ?? _config.DefaultExchange,
                Properties = _defaultProperties,
                RoutingKey = routingKey ?? string.Empty
            }).ToArray();

            return _publishBlock.SendAsync(commands);
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
