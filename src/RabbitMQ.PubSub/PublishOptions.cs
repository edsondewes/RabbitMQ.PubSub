using System.Collections.Generic;

namespace RabbitMQ.PubSub
{
    public class PublishOptions
    {   
        public string Exchange { get; set; }
        public IDictionary<string, object> Headers { get; set; }
        public string MimeType { get; set; }
        public string RoutingKey { get; set; }

        public PublishOptions ToExchange(string exchange)
        {
            Exchange = exchange;
            return this;
        }

        public PublishOptions ToRoutingKey(string routingKey)
        {
            RoutingKey = routingKey;
            return this;
        }

        public PublishOptions WithHeaders(IDictionary<string, object> headers)
        {
            Headers = headers;
            return this;
        }

        public PublishOptions WithMimeType(string mimeType)
        {
            MimeType = mimeType;
            return this;
        }

        public static PublishOptions RoutingTo(string routingKey) => new PublishOptions { RoutingKey = routingKey };
    }
}
