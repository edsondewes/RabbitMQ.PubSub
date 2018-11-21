using RabbitMQ.Client;

namespace RabbitMQ.PubSub
{
    public class ConnectionHelper : IConnectionHelper
    {
        public IConnection CreateConnection(IConnectionOptions options)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                Password = options.Password ?? ConnectionFactory.DefaultPass,
                UserName = options.User ?? ConnectionFactory.DefaultUser,
                VirtualHost = options.VirtualHost ?? ConnectionFactory.DefaultVHost
            };
            
            return factory.CreateConnection();
        }
    }
}
