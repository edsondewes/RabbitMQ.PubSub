using System;
using System.Threading.Tasks;

namespace RabbitMQ.PubSub
{
    public interface IMessageProducer : IDisposable
    {
        Task Publish<T>(T obj, string routingKey = null, string exchange = null);
    }
}
