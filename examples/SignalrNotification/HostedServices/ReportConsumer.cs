using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.PubSub;
using SignalrNotification.Report;

namespace SignalrNotification.HostedServices
{
    public class ReportConsumer<T> : IBackgroundConsumer<T>
    {
        private readonly IReportListener<T> _listener;

        public ReportConsumer(IReportListener<T> listener)
        {
            _listener = listener;
        }

        public Task Consume(T obj, MessageContext context, CancellationToken cancellationToken)
        {
            var reportId = context.ExtractReportId();
            return _listener.Receive(obj, reportId, cancellationToken);
        }
    }
}
