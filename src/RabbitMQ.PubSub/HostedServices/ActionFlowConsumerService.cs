using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.PubSub.Subscriptions;

namespace RabbitMQ.PubSub.HostedServices
{
    public class ActionFlowConsumerService<TObj, TService> : IHostedService, IDisposable
        where TService : IBackgroundConsumer<TObj>
    {
        private readonly IMessageConsumer _consumer;
        private readonly TService _service;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        private ActionFlowStrategy<TObj> _strategy;
        private ISubscription _subscription;

        public ActionFlowConsumerService(IMessageConsumer consumer, TService service)
        {
            _consumer = consumer;
            _service = service;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _strategy = new ActionFlowStrategy<TObj>(_service.Consume, new ActionFlowStrategyOptions
            {
                CancellationToken = _stoppingCts.Token,
                MaxDegreeOfParallelism = _service.MaxDegreeOfParallelism
            });

            _subscription = _consumer.Subscribe(_strategy, new SubscriptionOptions
            {
                Exchange = _service.Exchange,
                Queue = _service.QueueName,
                RoutingKeys = _service.RoutingKeys
            });

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_strategy.Complete(), Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public void Dispose()
        {
            _stoppingCts.Cancel();
            _subscription.Dispose();
        }
    }
}
