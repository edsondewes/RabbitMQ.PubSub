using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MessagePack;

namespace RabbitMQ.PubSub.Subscriptions
{
    public class ActionFlowStrategy<T> : IConsumerStrategy
    {
        private readonly ITargetBlock<T> _callbackBlock;
        private readonly IPropagatorBlock<byte[], T> _serializeBlock;

        public ActionFlowStrategy(Func<T, Task> callback, ActionFlowStrategyOptions dataFlowOptions = null)
        {
            var executionOptions = new ExecutionDataflowBlockOptions
            {
                CancellationToken = dataFlowOptions?.CancellationToken ?? default(CancellationToken),
                MaxDegreeOfParallelism = dataFlowOptions?.MaxDegreeOfParallelism ?? 1,
                SingleProducerConstrained = true,
            };

            _serializeBlock = new TransformBlock<byte[], T>(body => MessagePackSerializer.Deserialize<T>(body), executionOptions);
            _callbackBlock = new ActionBlock<T>(callback, executionOptions);

            var flowOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true,
            };

            _serializeBlock.LinkTo(_callbackBlock, flowOptions);
        }

        public Task Complete()
        {
            _serializeBlock.Complete();
            return _callbackBlock.Completion;
        }

        public void Consume(byte[] body)
        {
            _serializeBlock.Post(body);
        }
    }
}
