using System.Threading;
using System.Threading.Tasks;
using ConsoleApp.Services;
using RabbitMQ.PubSub.HostedServices;

namespace ConsoleApp.BackgroundServices
{
    public class DataLoggerConsumer : IBackgroundConsumer<SomeData>
    {
        private readonly DelayedLogger _delayedLogger;
        
        public DataLoggerConsumer(DelayedLogger delayedLogger)
        {
            _delayedLogger = delayedLogger;
        }

        public async Task Consume(SomeData obj, CancellationToken cancellationToken)
        {
            await _delayedLogger.Log("Data received: {id} - {name}", new object[] { obj.Id, obj.Name }, cancellationToken);
        }
    }
}
