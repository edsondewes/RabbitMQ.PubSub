using RabbitMQ.Client;

namespace RabbitMQ.PubSub
{
    internal class PublishCommand
    {
        public byte[] Body { get; set; }
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
        public IBasicProperties Properties { get; set; }
    }
}
