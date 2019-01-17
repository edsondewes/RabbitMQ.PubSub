using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.PubSub;
using SignalrNotification.Model;
using SignalrNotification.Report;

namespace SignalrNotification.HostedServices
{
    public class BackgroundJobConsumer : IBackgroundConsumer<BackgroundJobData>
    {
        private readonly IReportProgress<BackgroundJobReport> _progress;
        private readonly ILogger<BackgroundJobConsumer> _logger;

        public BackgroundJobConsumer(IReportProgress<BackgroundJobReport> progress, ILogger<BackgroundJobConsumer> logger)
        {
            _progress = progress;
            _logger = logger;
        }

        public async Task Consume(BackgroundJobData obj, MessageContext context, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Starting background job...");

            var reportId = context.ExtractReportId();
            const int Count = 10;
            for (var i = 1; i <= Count; i++)
            {
                var report = new BackgroundJobReport
                {
                    Count = Count,
                    Current = i
                };

                _progress.Report(report, reportId);
                await Task.Delay(500);
            }

            _logger.LogWarning("Finished background job!");
        }
    }
}
