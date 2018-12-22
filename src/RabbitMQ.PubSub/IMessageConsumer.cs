using System;

namespace RabbitMQ.PubSub
{
    public interface IMessageConsumer : IDisposable
    {
        ISubscription Subscribe<T>(Action<T, MessageContext> callback, SubscriptionOptions options = null);
    }
}
