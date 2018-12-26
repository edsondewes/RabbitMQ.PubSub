using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub
{
    public interface IBackgroundConsumer<T>
    {
        Task Consume(T obj, MessageContext context, CancellationToken cancellationToken);
    }
}
