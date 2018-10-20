using System.Collections.Generic;

namespace RabbitMQ.PubSub
{
    public class SubscriptionOptions
    {
        public string Exchange { get; set; }
        public string Queue { get; set; }
        public IEnumerable<string> RoutingKeys { get; set; }
    }
}
