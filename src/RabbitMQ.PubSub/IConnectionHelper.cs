using RabbitMQ.Client;

namespace RabbitMQ.PubSub
{
    public interface IConnectionHelper
    {
        IConnection CreateConnection(string host);
    }
}
