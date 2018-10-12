using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApp.Services;
using RabbitMQ.PubSub.HostedServices;

namespace ConsoleApp.BackgroundServices
{
    public class DataLoggerConsumer : DefaultBackgroundConsumer<SomeData>
    {
        public override IEnumerable<string> RoutingKeys => new[] { "test" };
        private readonly DelayedLogger _delayedLogger;

        public DataLoggerConsumer(DelayedLogger delayedLogger)
        {
            _delayedLogger = delayedLogger;
        }

        public override async Task Consume(SomeData obj)
        {
            await _delayedLogger.Log("Data received: {id} - {name}", obj.Id, obj.Name);
        }
    }
}
