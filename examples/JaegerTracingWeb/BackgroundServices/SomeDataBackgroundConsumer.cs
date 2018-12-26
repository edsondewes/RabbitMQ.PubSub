using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.PubSub;

namespace JaegerTracingWeb.BackgroundServices
{
    public class SomeDataBackgroundConsumer : IBackgroundConsumer<SomeData>
    {
        private readonly IMessageProducer _producer;
        private readonly ILogger<SomeDataBackgroundConsumer> _logger;
        private readonly Random _random;

        public SomeDataBackgroundConsumer(IMessageProducer producer, ILogger<SomeDataBackgroundConsumer> logger)
        {
            _producer = producer;
            _logger = logger;
            _random = new Random();
        }

        public async Task Consume(SomeData obj, MessageContext context, CancellationToken cancellationToken)
        {
            var randomDelay = _random.Next(50);
            await Task.Delay(randomDelay, cancellationToken);
            _logger.LogInformation("Received message {text} at {date}", obj.Text, obj.Date);

            var otherData = new OtherData { Date = DateTime.Now };
            _producer.Publish(otherData, PublishOptions
                .RoutingTo("test2")
                .WithTraceContext());
        }
    }
}
