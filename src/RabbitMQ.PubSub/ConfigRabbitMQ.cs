namespace RabbitMQ.PubSub
{
    public class ConfigRabbitMQ : IConnectionOptions
    {
        public string Host { get; set; }
        public string Password { get; set; }
        public string User { get; set; }
        public string VirtualHost { get; set; }

        public string DefaultExchange { get; set; } = null;
        public bool DurableQueues { get; set; } = false;
        public bool LazyQueues { get; set; } = false;
        public bool PersistentDelivery { get; set; } = false;
    }
}
