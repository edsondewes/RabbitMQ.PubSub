namespace RabbitMQ.PubSub
{
    public interface IConsumerStrategy
    {
        void Consume(byte[] body);
    }
}
