using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RabbitMQ.PubSub.Subscriptions
{
    public class ActionFlowStrategy<T> : IConsumerStrategy<T>
    {
        private readonly ITargetBlock<T> _callbackBlock;

        public ActionFlowStrategy(Func<T, Task> callback, ActionFlowStrategyOptions dataFlowOptions = null)
        {
            var executionOptions = new ExecutionDataflowBlockOptions
            {
                CancellationToken = dataFlowOptions?.CancellationToken ?? default,
                MaxDegreeOfParallelism = dataFlowOptions?.MaxDegreeOfParallelism ?? 1,
                SingleProducerConstrained = true,
            };

            _callbackBlock = new ActionBlock<T>(callback, executionOptions);
        }

        public Task Complete()
        {
            _callbackBlock.Complete();
            return _callbackBlock.Completion;
        }

        public void Consume(T message)
        {
            _callbackBlock.Post(message);
        }
    }
}
