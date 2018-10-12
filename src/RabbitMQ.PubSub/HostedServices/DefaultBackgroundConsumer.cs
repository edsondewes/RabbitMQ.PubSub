using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub.HostedServices
{
    public abstract class DefaultBackgroundConsumer<T> : IBackgroundConsumer<T>
    {
        public virtual string Exchange => null;
        public virtual string QueueName => null;
        public virtual int MaxDegreeOfParallelism => 1;
        public abstract IEnumerable<string> RoutingKeys { get; }
        public abstract Task Consume(T obj);
    }
}
