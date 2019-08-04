using System;
using System.Collections.Generic;

namespace JaegerTracingWeb.Diagnostics
{
    public interface ITracingObserver : IObserver<KeyValuePair<string, object>>
    {
        string ListenerName { get; }
        Predicate<string> IsEnabled { get; }
    }
}
