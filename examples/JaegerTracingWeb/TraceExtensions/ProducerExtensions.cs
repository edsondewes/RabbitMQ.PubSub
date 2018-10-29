using System;
using System.Collections.Generic;
using JaegerTracingWeb.TraceExtensions;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;

namespace RabbitMQ.PubSub
{
    public static class ProducerExtensions
    {
        public static PublishOptions WithTraceContext(this PublishOptions options)
        {
            if (!GlobalTracer.IsRegistered())
                throw new Exception("Global tracer is not registered");

            return WithTraceContext(options, GlobalTracer.Instance);
        }

        public static PublishOptions WithTraceContext(this PublishOptions options, ITracer tracer)
        {
            var headers = new Dictionary<string, object>();
            tracer.Inject(tracer.ActiveSpan.Context, BuiltinFormats.TextMap, new RabbitHeaderAdapter(headers));

            return options.WithHeaders(headers);
        }
    }
}
