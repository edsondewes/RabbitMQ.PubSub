namespace RabbitMQ.PubSub
{
    public interface IConsumerStrategy<T>
    {
        void Consume(T message);
    }
}
