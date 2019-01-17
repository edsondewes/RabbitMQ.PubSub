using RabbitMQ.PubSub;

namespace SignalrNotification.Report
{
    public class RabbitMQProgress<T> : IReportProgress<T>
    {
        private readonly IMessageProducer _producer;
        private readonly string _routingKey;

        public RabbitMQProgress(IMessageProducer producer)
        {
            _producer = producer;
            _routingKey = $"report_{typeof(T).FullName}";
        }

        public void Report(T obj, string reportId)
        {
            var options = PublishOptions
                .RoutingTo(_routingKey)
                .WithReportId(reportId);

            _producer.Publish(obj, options);
        }
    }
}
