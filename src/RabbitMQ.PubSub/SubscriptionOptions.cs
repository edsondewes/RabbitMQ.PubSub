using System.Collections.Generic;
using System.Linq;

namespace RabbitMQ.PubSub
{
    public class SubscriptionOptions
    {
        public bool AutoAck { get; set; } = true;
        public string Exchange { get; set; }
        public string Queue { get; set; }
        public IEnumerable<string> RoutingKeys { get; set; }
    }

    public static class SubscriptionOptionsExtensions
    {
        public static string ExchangeType(this SubscriptionOptions options)
        {
            if (options != null && options.RoutingKeys != null && options.RoutingKeys.Any())
                return Client.ExchangeType.Topic;

            return Client.ExchangeType.Fanout;
        }
    }
}
