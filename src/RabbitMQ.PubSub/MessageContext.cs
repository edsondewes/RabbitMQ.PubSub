using System.Collections.Generic;
using RabbitMQ.Client;

namespace RabbitMQ.PubSub
{
    public class MessageContext
    {
        public IDictionary<string, object> Headers { get; }

        internal MessageContext(IBasicProperties properties)
        {
            Headers = properties.Headers;
        }
    }
}
