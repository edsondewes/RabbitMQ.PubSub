using RabbitMQ.Client;

namespace RabbitMQ.PubSub
{
    public interface IConnectionHelper
    {
        IConnection TryCreateConnection(string host);
    }
}
