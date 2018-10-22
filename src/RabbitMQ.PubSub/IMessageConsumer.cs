using System;

namespace RabbitMQ.PubSub
{
    public interface IMessageConsumer : IDisposable
    {
        ISubscription Subscribe<T>(Action<T> callback, SubscriptionOptions options = null);
        ISubscription Subscribe<T>(IConsumerStrategy<T> strategy, SubscriptionOptions options = null);
    }
}
