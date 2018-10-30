using RabbitMQ.Client;

namespace RabbitMQ.PubSub
{
    public class ConnectionHelper : IConnectionHelper
    {
        public IConnection CreateConnection(string host)
        {
            var factory = new ConnectionFactory { HostName = host };
            return factory.CreateConnection();
        }
    }
}
