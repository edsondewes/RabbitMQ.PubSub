using System;
using System.Collections.Generic;
using JaegerTracingWeb.Diagnostics.Adapters;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using RabbitMQ.PubSub;

namespace JaegerTracingWeb.Diagnostics
{
    public class RabbitMQObserver : ITracingObserver
    {
        private const string ComponentName = "JaegerTracing.Background";
        private readonly ITracer _tracer;

        public string ListenerName => "RabbitMQ.PubSub";

        public Predicate<string> IsEnabled => null;

        public RabbitMQObserver(ITracer tracer)
        {
            _tracer = tracer;
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            var eventName = value.Key;
            var eventData = value.Value;

            switch (eventName)
            {
                case "RabbitMQ.PubSub.MessageIn.Start":
                    {
                        var context = eventData.GetProperty("Context") as MessageContext;
                        var headerContext = _tracer.Extract(BuiltinFormats.HttpHeaders, new DictionaryAdapter(context.Headers));

                        _tracer.BuildSpan("rabbit.consume")
                            .AsChildOf(headerContext)
                            .WithTag(Tags.Component, ComponentName)
                            .WithTag(Tags.SpanKind, Tags.SpanKindConsumer)
                            .StartActive();

                        break;
                    }
                case "RabbitMQ.PubSub.MessageIn.Stop":
                    {
                        DisposeActiveScope();
                        break;
                    }
                case "RabbitMQ.PubSub.MessageOut.Start":
                    {
                        var options = eventData.GetProperty("Options") as PublishOptions;

                        var scope = _tracer.BuildSpan("rabbit.publish")
                            .WithTag(Tags.Component, ComponentName)
                            .WithTag(Tags.SpanKind, Tags.SpanKindProducer)
                            .WithTag("exchange", options.Exchange)
                            .WithTag("mime.type", options.MimeType)
                            .WithTag("routing.key", options.RoutingKey)
                            .StartActive();

                        if (options.Headers is null)
                        {
                            options.Headers = new Dictionary<string, object>();
                        }

                        _tracer.Inject(scope.Span.Context, BuiltinFormats.HttpHeaders, new DictionaryAdapter(options.Headers));

                        break;
                    }
                case "RabbitMQ.PubSub.MessageOut.Stop":
                    {
                        DisposeActiveScope();
                        break;
                    }
                case "RabbitMQ.PubSub.Deserialize.Start":
                    {
                        _tracer.BuildSpan("rabbit.deserialize")
                            .WithTag(Tags.Component, ComponentName)
                            .WithTag(Tags.SpanKind, Tags.SpanKindConsumer)
                            .StartActive();

                        break;
                    }
                case "RabbitMQ.PubSub.Deserialize.Stop":
                    {
                        DisposeActiveScope();
                        break;
                    }
                case "RabbitMQ.PubSub.Serialize.Start":
                    {
                        _tracer.BuildSpan("rabbit.serialize")
                            .WithTag(Tags.Component, ComponentName)
                            .WithTag(Tags.SpanKind, Tags.SpanKindConsumer)
                            .StartActive();

                        break;
                    }
                case "RabbitMQ.PubSub.Serialize.Stop":
                    {
                        DisposeActiveScope();
                        break;
                    }

                default:
                    break;
            }
        }

        public void OnCompleted() { }
        public void OnError(Exception error) { }

        private void DisposeActiveScope()
        {
            var scope = _tracer.ScopeManager.Active;
            if (scope != null)
            {
                scope.Dispose();
            }
        }
    }
}
