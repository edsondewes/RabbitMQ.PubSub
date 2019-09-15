using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace RabbitMQ.PubSub
{
    public class AsyncConsumerService<TObj, TService> : IHostedService
        where TService : IBackgroundConsumer<TObj>
    {
        private readonly IMessageConsumer _consumer;
        private readonly TService _service;
        private readonly AsyncConsumerOptions<TObj> _options;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        private ISubscription _subscription;

        public AsyncConsumerService(IMessageConsumer consumer, TService service, AsyncConsumerOptions<TObj> options)
        {
            _consumer = consumer;
            _service = service;
            _options = options;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var manager = new PipelineManager<TObj>(_service, _options.Pipelines, _stoppingCts.Token);

            // Since each message is executed sequencially, we can use a shared manager
            _subscription = _consumer.Subscribe<TObj>(manager.Run, new SubscriptionOptions
            {
                AutoAck = _options.AutoAck,
                Exchange = _options.Exchange,
                Queue = _options.QueueName,
                RoutingKeys = _options.RoutingKeys
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription.Dispose();
            _stoppingCts.Cancel();

            return Task.CompletedTask;
        }
    }
}
