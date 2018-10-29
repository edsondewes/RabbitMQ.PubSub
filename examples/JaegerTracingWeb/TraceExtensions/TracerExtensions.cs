using System.Collections.Generic;
using JaegerTracingWeb.TraceExtensions;
using OpenTracing.Propagation;

namespace OpenTracing
{
    public static class TracerExtensions
    {
        public static ISpanContext ExtractFromRabbitMQ(this ITracer tracer, IDictionary<string, object> headers)
        {
            var context = tracer.Extract(BuiltinFormats.TextMap, new RabbitHeaderAdapter(headers));
            return context;
        }
    }
}
