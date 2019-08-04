using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace JaegerTracingWeb.Diagnostics
{
    public class DiagnosticsHostedService : IHostedService, IObserver<DiagnosticListener>
    {
        private readonly IEnumerable<ITracingObserver> _observers;
        private IDisposable _listenerSubscription;
        private Dictionary<ITracingObserver, IDisposable> _subscriptions;

        public DiagnosticsHostedService(IEnumerable<ITracingObserver> observers)
        {
            _observers = observers;
            _subscriptions = new Dictionary<ITracingObserver, IDisposable>();
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        public void OnNext(DiagnosticListener listener)
        {
            var observers = _observers.Where(o => o.ListenerName == listener.Name).ToList();
            if (observers.Count == 0)
                return;

            lock (_subscriptions)
            {
                foreach (var item in observers)
                {
                    if (_subscriptions.ContainsKey(item))
                    {
                        _subscriptions.Remove(item, out var subscription);
                        subscription.Dispose();
                    }

                    var newSubscription = listener.Subscribe(item, item.IsEnabled);
                    _subscriptions.Add(item, newSubscription);
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _listenerSubscription = DiagnosticListener.AllListeners.Subscribe(this);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _listenerSubscription?.Dispose();
            foreach (var subscription in _subscriptions.Values)
            {
                subscription.Dispose();
            }

            _subscriptions.Clear();
            _subscriptions = null;
            _listenerSubscription = null;

            return Task.CompletedTask;
        }
    }
}
