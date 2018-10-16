using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace RabbitMQ.PubSub.HostedServices
{
    public class BackgroundConsumerService<TObj, TService> : IHostedService, IDisposable
        where TService : IBackgroundConsumer<TObj>
    {
        private readonly IMessageConsumer _consumer;
        private readonly TService _service;

        private IMessageSubscription _subscription;

        public BackgroundConsumerService(IMessageConsumer consumer, TService service)
        {
            _consumer = consumer;
            _service = service;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscription = _consumer.Subscribe<TObj>(
                callback: _service.Consume,
                exchange: _service.Exchange,
                maxDegreeOfParallelism: _service.MaxDegreeOfParallelism,
                queue: _service.QueueName,
                routingKeys: _service.RoutingKeys
                );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _subscription.Complete();
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}
