using System;
using System.Collections.Generic;
using JaegerTracingWeb.Diagnostics.Adapters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace JaegerTracingWeb.Diagnostics
{
    public class AspNetEventObserver : ITracingObserver
    {
        private const string ComponentName = "JaegerTracing.HttpIn";
        private readonly ITracer _tracer;

        public string ListenerName => "Microsoft.AspNetCore";

        public Predicate<string> IsEnabled => (eventName) =>
        {
            switch (eventName)
            {
                case "Microsoft.AspNetCore.Hosting.HttpRequestIn":
                case "Microsoft.AspNetCore.Hosting.HttpRequestIn.Start":
                case "Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop":
                case "Microsoft.AspNetCore.Hosting.UnhandledException":
                    return true;
                default:
                    return false;
            }
        };

        public AspNetEventObserver(ITracer tracer)
        {
            _tracer = tracer;
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            var eventName = value.Key;
            var eventData = value.Value;

            switch (eventName)
            {
                case "Microsoft.AspNetCore.Hosting.HttpRequestIn.Start":
                    {
                        var httpContext = eventData.GetProperty("HttpContext") as HttpContext;
                        var request = httpContext.Request;

                        var headerContext = _tracer.Extract(BuiltinFormats.HttpHeaders, new RequestHeadersExtractAdapter(request.Headers));

                        _tracer.BuildSpan(request.Method)
                            .AsChildOf(headerContext)
                            .WithTag(Tags.Component, ComponentName)
                            .WithTag(Tags.SpanKind, Tags.SpanKindServer)
                            .WithTag(Tags.HttpMethod, request.Method)
                            .WithTag(Tags.HttpUrl, request.GetDisplayUrl())
                            .StartActive();

                        break;
                    }
                case "Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop":
                    {
                        var scope = _tracer.ScopeManager.Active;
                        if (scope != null)
                        {
                            var httpContext = eventData.GetProperty("HttpContext") as HttpContext;

                            scope.Span.SetTag(Tags.HttpStatus, httpContext.Response.StatusCode);
                            scope.Dispose();
                        }

                        break;
                    }
                case "Microsoft.AspNetCore.Hosting.UnhandledException":
                    {
                        var span = _tracer.ActiveSpan;
                        if (span != null)
                        {
                            var exception = eventData.GetProperty("exception") as Exception;
                            span.SetTag(Tags.Error, true);
                            span.Log(new Dictionary<string, object>(3)
                            {
                                { LogFields.Event, Tags.Error.Key },
                                { LogFields.ErrorKind, exception.GetType().Name },
                                { LogFields.ErrorObject, exception },
                            });
                        }

                        break;
                    }
                default:
                    break;
            }
        }

        public void OnCompleted() { }
        public void OnError(Exception error) { }

        
    }
}
