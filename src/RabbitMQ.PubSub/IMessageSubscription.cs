using System;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub
{
    public interface IMessageSubscription : IDisposable
    {
        Task Complete();
    }
}
