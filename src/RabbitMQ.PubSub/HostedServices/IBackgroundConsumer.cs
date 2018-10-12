using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub.HostedServices
{
    public interface IBackgroundConsumer<T>
    {
        string Exchange { get; }
        int MaxDegreeOfParallelism { get; }
        string QueueName { get; }
        IEnumerable<string> RoutingKeys { get; }
        Task Consume(T obj);
    }
}
