using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub
{
    public interface IMessageProducer : IDisposable
    {
        Task Complete();
        Task Publish<T>(T obj, string routingKey = null, string exchange = null, string mimeType = null);
        Task Publish<T>(IEnumerable<T> obj, string routingKey = null, string exchange = null, string mimeType = null);
    }
}
