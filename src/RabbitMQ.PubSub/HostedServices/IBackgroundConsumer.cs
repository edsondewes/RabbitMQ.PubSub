using System.Threading.Tasks;

namespace RabbitMQ.PubSub.HostedServices
{
    public interface IBackgroundConsumer<T>
    {
        Task Consume(T obj);
    }
}
