namespace RabbitMQ.PubSub
{
    public interface IConnectionOptions
    {
        string Host { get; }
        string User { get; }
        string Password { get; }
        string VirtualHost { get; }
    }
}
