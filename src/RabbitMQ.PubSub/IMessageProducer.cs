using System;
using System.Collections.Generic;

namespace RabbitMQ.PubSub
{
    public interface IMessageProducer : IDisposable
    {
        void Publish<T>(T obj, PublishOptions options);
        void PublishBatch<T>(IEnumerable<T> obj, PublishOptions options);
    }
}
