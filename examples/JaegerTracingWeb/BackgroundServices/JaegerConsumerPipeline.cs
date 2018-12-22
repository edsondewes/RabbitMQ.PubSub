using System;
using System.Threading;
using System.Threading.Tasks;
using OpenTracing;
using RabbitMQ.PubSub;
using RabbitMQ.PubSub.HostedServices;

namespace JaegerTracingWeb.BackgroundServices
{
    public class JaegerConsumerPipeline<T> : IConsumerPipeline<T>
    {
        private readonly ITracer _tracer;

        public JaegerConsumerPipeline(ITracer tracer)
        {
            _tracer = tracer;
        }

        public async Task Handle(T obj, MessageContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            using (IScope scope = _tracer.BuildSpan($"IBackgroundConsumer<{typeof(T).Name}>")
                  .AsChildOf(_tracer.ExtractFromRabbitMQ(context.Headers))
                  .StartActive(finishSpanOnDispose: true))
            {
                await next();
            }
        }
    }
}
