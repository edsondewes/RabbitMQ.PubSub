using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub.HostedServices
{
    public interface IConsumerPipeline<T>
    {
        Task Handle(T obj, MessageContext context, CancellationToken cancellationToken, Func<Task> next);
    }
}
