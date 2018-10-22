namespace RabbitMQ.PubSub
{
    public interface ISerializer
    {
        string MimeType { get; }
        T Deserialize<T>(byte[] body);
        byte[] Serialize<T>(T obj);
    }
}
