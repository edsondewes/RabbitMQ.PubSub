using System.Threading;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace RabbitMQ.PubSub
{
    public class ConnectionHelper : IConnectionHelper
    {
        private readonly ILogger<ConnectionHelper> _logger;

        public ConnectionHelper(ILogger<ConnectionHelper> logger)
        {
            _logger = logger;
        }

        public IConnection TryCreateConnection(string host)
        {
            var factory = new ConnectionFactory { HostName = host };
            IConnection connection = null;
            do
            {
                try
                {
                    connection = factory.CreateConnection();
                }
                catch (BrokerUnreachableException)
                {
                    _logger.LogWarning("Cannot connect to rabbitmq. Retrying in 5 seconds");
                    Thread.Sleep(5000);
                }
            } while (connection == null);

            return connection;
        }
    }
}
