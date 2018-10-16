using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MessagePack;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.PubSub
{
    public class DataFlowSubscription<T> : IMessageSubscription
    {
        private readonly string _consumerTag;
        private readonly IModel _model;

        private readonly ITargetBlock<T> _callbackBlock;
        private readonly ITargetBlock<byte[]> _serializeBlock;

        public DataFlowSubscription(IModel model, string queueName, Func<T, Task> callback, int maxDegreeOfParallelism)
        {
            var consumer = new EventingBasicConsumer(model);
            _consumerTag = model.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            consumer.Received += (_, eventArgs) => _serializeBlock.Post(eventArgs.Body);

            (_serializeBlock, _callbackBlock) = CreateFlow(callback, maxDegreeOfParallelism);
            _model = model;
        }

        public Task Complete()
        {
            _serializeBlock.Complete();
            return _callbackBlock.Completion;
        }

        public void Dispose()
        {
            _model.BasicCancel(_consumerTag);
        }

        private static (ITargetBlock<byte[]> serializeBlock, ITargetBlock<T> callbackBlock)
            CreateFlow(Func<T, Task> callback, int maxDegreeOfParallelism)
        {
            var executionOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                SingleProducerConstrained = true,
            };

            var serializeBlock = new TransformBlock<byte[], T>(body => MessagePackSerializer.Deserialize<T>(body), executionOptions);
            var callbackBlock = new ActionBlock<T>(callback, executionOptions);

            var flowOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true,
            };

            serializeBlock.LinkTo(callbackBlock, flowOptions);

            return (serializeBlock, callbackBlock);
        }
    }
}
