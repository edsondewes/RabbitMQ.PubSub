using System;

namespace RabbitMQ.PubSub
{
    public interface IMessageConsumer : IDisposable
    {
        ISubscription Subscribe(Action<byte[]> callback, SubscriptionOptions options = null);
        ISubscription Subscribe(IConsumerStrategy strategy, SubscriptionOptions options = null);
    }
}
