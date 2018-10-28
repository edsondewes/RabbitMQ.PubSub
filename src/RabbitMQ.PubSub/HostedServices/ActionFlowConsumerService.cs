using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly TService _service;
        private readonly ActionFlowConsumerOptions<TObj> _options;
        private readonly ILogger<ActionFlowConsumerService<TObj, TService>> _logger;

        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        private ActionBlock<MessageContent> _actionBlock;
        private ISubscription _subscription;

        public ActionFlowConsumerService(
            IMessageConsumer consumer,
            TService service,
            ActionFlowConsumerOptions<TObj> options,
            ILogger<ActionFlowConsumerService<TObj, TService>> logger
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
                ? (Func<MessageContent, Task>)ConsumeWithPipeline
                : ConsumeWithToken;

            _actionBlock = new ActionBlock<MessageContent>(callback, new ExecutionDataflowBlockOptions
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

        private Task ConsumeWithToken(MessageContent message)
        {
            return _service.Consume(message.Obj, _stoppingCts.Token);
        }

        private Task ConsumeWithPipeline(MessageContent message)
        {
            Task consumerHandler() => _service.Consume(message.Obj, _stoppingCts.Token);

            var pipelineHandler = _options.Pipelines.Aggregate(
                (Func<Task>)consumerHandler,
                (next, pipe) => () => pipe.Handle(message.Obj, message.Header, _stoppingCts.Token, next)
                );

            return pipelineHandler();
        }

        private void PostMessage(TObj message, IDictionary<string, object> header)
        {
            var content = new MessageContent(message, header);
            var posted = _actionBlock.Post(content);
            if (!posted)
            {
                _logger.LogError("Could not post message: {message}", message);
            }
        }

        private class MessageContent
        {
            public TObj Obj { get; }
            public IDictionary<string, object> Header { get; }

            public MessageContent(TObj obj, IDictionary<string, object> header)
            {
                Obj = obj;
                Header = header;
            }
        }
    }
}
