using System;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub
{
    public interface IMessageConsumer : IDisposable
    {
        ISubscription Subscribe<T>(Func<T, MessageContext, Task> callback, SubscriptionOptions options = null);
    }
}
