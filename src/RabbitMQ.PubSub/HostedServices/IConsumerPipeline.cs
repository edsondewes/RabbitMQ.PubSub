using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub
{
    public interface IConsumerPipeline<T>
    {
        Task Handle(T obj, MessageContext context, CancellationToken cancellationToken, Func<Task> next);
    }
}
