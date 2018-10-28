using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub
{
    public interface IMessageProducer : IDisposable
    {
        Task Complete();
        Task Publish<T>(T obj, PublishOptions options);
        Task Publish<T>(IEnumerable<T> obj, PublishOptions options);
    }
}
