using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub.HostedServices
{
    public interface IConsumerPipeline<T>
    {
        Task Handle(T obj, IDictionary<string, object> header, CancellationToken cancellationToken, Func<Task> next);
    }
}
