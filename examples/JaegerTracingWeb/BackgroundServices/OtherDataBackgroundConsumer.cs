using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.PubSub.HostedServices;

namespace JaegerTracingWeb.BackgroundServices
{
    public class OtherDataBackgroundConsumer : IBackgroundConsumer<OtherData>
    {
        private readonly ILogger<OtherDataBackgroundConsumer> _logger;
        private readonly Random _random;

        public OtherDataBackgroundConsumer(ILogger<OtherDataBackgroundConsumer> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task Consume(OtherData obj, CancellationToken cancellationToken)
        {
            var randomDelay = _random.Next(50);
            await Task.Delay(randomDelay, cancellationToken);
            _logger.LogInformation("Received other data at {date}", obj.Date);
        }
    }
}
