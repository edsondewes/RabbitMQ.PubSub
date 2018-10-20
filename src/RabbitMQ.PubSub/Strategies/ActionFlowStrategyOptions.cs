using System.Threading;

namespace RabbitMQ.PubSub.Subscriptions
{
    public class ActionFlowStrategyOptions
    {
        public CancellationToken CancellationToken { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
    }
}
