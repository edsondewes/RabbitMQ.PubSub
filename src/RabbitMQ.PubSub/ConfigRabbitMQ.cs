namespace RabbitMQ.PubSub
{
    public class ConfigRabbitMQ
    {
        public string Host { get; set; }

        public string DefaultExchange { get; set; } = null;
        public bool DurableQueues { get; set; } = false;
        public bool PersistentDelivery { get; set; } = false;
    }
}
