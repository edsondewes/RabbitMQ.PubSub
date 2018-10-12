using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub
{
    public interface IMessageConsumer : IDisposable
    {
        IMessageSubscription Subscribe<T>(
            IEnumerable<string> routingKeys,
            Func<T, Task> callback,
            string exchange = null,
            string queue = null,
            int maxDegreeOfParallelism = 1);
    }
}
