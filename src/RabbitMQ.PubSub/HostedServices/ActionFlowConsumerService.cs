using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RabbitMQ.PubSub.HostedServices
{
    public class ActionFlowConsumerService<TObj, TService> : IHostedService, IDisposable
        where TService : IBackgroundConsumer<TObj>
    {
        private readonly IMessageConsumer _consumer;
        private readonly ILogger<ActionFlowConsumerService<TObj, TService>> _logger;
        private readonly ActionFlowConsumerOptions _options;
        private readonly TService _service;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        private ActionBlock<TObj> _actionBlock;
        private ISubscription _subscription;

        public ActionFlowConsumerService(
            IMessageConsumer consumer,
            TService service,
            ActionFlowConsumerOptions options,
            ILogger<ActionFlowConsumerService<TObj, TService>> logger
            )
        {
            _consumer = consumer;
            _service = service;
            _options = options;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _actionBlock = new ActionBlock<TObj>(ConsumeWithToken, new ExecutionDataflowBlockOptions
            {
                CancellationToken = _stoppingCts.Token,
                MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism,
                SingleProducerConstrained = true,
            });

            _subscription = _consumer.Subscribe<TObj>(PostMessage, new SubscriptionOptions
            {
                Exchange = _options.Exchange,
                Queue = _options.QueueName,
                RoutingKeys = _options.RoutingKeys,
            });

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _subscription.Dispose();
                _stoppingCts.Cancel();
            }
            finally
            {
                var stoppingTask = await Task.WhenAny(_actionBlock.Completion, Task.Delay(Timeout.Infinite, cancellationToken));
                if (stoppingTask.IsFaulted)
                    _logger.LogError(stoppingTask.Exception.InnerException, "An error ocurred when processing a message");
            }
        }

        public void Dispose()
        {
            _subscription.Dispose();
            _stoppingCts.Cancel();
        }

        private Task ConsumeWithToken(TObj obj)
        {
            return _service.Consume(obj, _stoppingCts.Token);
        }

        private void PostMessage(TObj message)
        {
            var posted = _actionBlock.Post(message);
            if (!posted)
            {
                _logger.LogError("Could not post message: {message}", message);
            }
        }
    }
}
