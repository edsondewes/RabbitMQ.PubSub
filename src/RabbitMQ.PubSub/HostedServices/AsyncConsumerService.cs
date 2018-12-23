using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RabbitMQ.PubSub.HostedServices
{
    public class AsyncConsumerService<TObj, TService> : IHostedService
        where TService : IBackgroundConsumer<TObj>
    {
        private readonly IMessageConsumer _consumer;
        private readonly TService _service;
        private readonly AsyncConsumerOptions<TObj> _options;
        private readonly ILogger<AsyncConsumerService<TObj, TService>> _logger;

        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        private ISubscription _subscription;

        public AsyncConsumerService(
            IMessageConsumer consumer,
            TService service,
            AsyncConsumerOptions<TObj> options,
            ILogger<AsyncConsumerService<TObj, TService>> logger
            )
        {
            _consumer = consumer;
            _service = service;
            _options = options;
            _logger = logger;

            options.Pipelines.Reverse();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var callback = _options.Pipelines.Any()
                ? (Func<TObj, MessageContext, Task>)ConsumeWithPipeline
                : ConsumeWithToken;

            _subscription = _consumer.Subscribe(callback, new SubscriptionOptions
            {
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

        private Task ConsumeWithToken(TObj obj, MessageContext context)
        {
            return _service.Consume(obj, context, _stoppingCts.Token);
        }

        private Task ConsumeWithPipeline(TObj obj, MessageContext context)
        {
            Task consumerHandler() => _service.Consume(obj, context, _stoppingCts.Token);

            var pipelineHandler = _options.Pipelines.Aggregate(
                (Func<Task>)consumerHandler,
                (next, pipe) => () => pipe.Handle(obj, context, _stoppingCts.Token, next)
                );

            return pipelineHandler();
        }
    }
}
