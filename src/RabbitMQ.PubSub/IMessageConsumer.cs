using System;
using System.Collections.Generic;

namespace RabbitMQ.PubSub
{
    public interface IMessageConsumer : IDisposable
    {
        ISubscription Subscribe<T>(Action<T, IDictionary<string, object>> callback, SubscriptionOptions options = null);
    }
}
