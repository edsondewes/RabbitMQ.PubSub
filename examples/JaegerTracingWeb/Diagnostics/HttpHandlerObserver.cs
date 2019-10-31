using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using JaegerTracingWeb.Diagnostics.Adapters;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace JaegerTracingWeb.Diagnostics
{
    public class HttpHandlerObserver : ITracingObserver
    {
        private const string ComponentName = "JaegerTracing.HttpOut";
        private readonly ITracer _tracer;
        private const string PropertiesKey = "ot-Span";

        public string ListenerName => "HttpHandlerDiagnosticListener";

        public Predicate<string> IsEnabled => (eventName) =>
        {
            switch (eventName)
            {
                case "System.Net.Http.Exception":
                case "System.Net.Http.HttpRequestOut":
                case "System.Net.Http.HttpRequestOut.Start":
                case "System.Net.Http.HttpRequestOut.Stop":
                    return true;
                default:
                    return false;
            }
        };

        public HttpHandlerObserver(ITracer tracer)
        {
            _tracer = tracer;
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            var eventName = value.Key;
            var eventData = value.Value;

            switch (eventName)
            {
                case "System.Net.Http.Exception":
                    {
                        var request = eventData.GetProperty("Request") as HttpRequestMessage;

                        if (request.Properties.TryGetValue(PropertiesKey, out object objSpan) && objSpan is ISpan span)
                        {
                            var exception = eventData.GetProperty("Exception") as Exception;
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
                case "System.Net.Http.HttpRequestOut.Start":
                    {
                        var request = eventData.GetProperty("Request") as HttpRequestMessage;

                        var span = _tracer.BuildSpan(request.Method.Method)
                            .WithTag(Tags.SpanKind, Tags.SpanKindClient)
                            .WithTag(Tags.Component, ComponentName)
                            .WithTag(Tags.HttpMethod, request.Method.Method)
                            .WithTag(Tags.HttpUrl, request.RequestUri.ToString())
                            .WithTag(Tags.PeerHostname, request.RequestUri.Host)
                            .WithTag(Tags.PeerPort, request.RequestUri.Port)
                            .Start();

                        _tracer.Inject(span.Context, BuiltinFormats.HttpHeaders, new HttpHeadersInjectAdapter(request.Headers));
                        request.Properties.Add(PropertiesKey, span);

                        break;
                    }
                case "System.Net.Http.HttpRequestOut.Stop":
                    {
                        var request = eventData.GetProperty("Request") as HttpRequestMessage;
                        if (request.Properties.TryGetValue(PropertiesKey, out object objSpan) && objSpan is ISpan span)
                        {
                            if (eventData.GetProperty("Response") is HttpResponseMessage response)
                            {
                                span.SetTag(Tags.HttpStatus, (int)response.StatusCode);
                            }

                            var requestTaskStatus = (TaskStatus)eventData.GetProperty("RequestTaskStatus");
                            if (requestTaskStatus == TaskStatus.Canceled || requestTaskStatus == TaskStatus.Faulted)
                            {
                                span.SetTag(Tags.Error, true);
                            }

                            span.Finish();

                            request.Properties[PropertiesKey] = null;
                        }

                        break;
                    }
            }
        }

        public void OnCompleted() { }
        public void OnError(Exception error) { }
    }
}
