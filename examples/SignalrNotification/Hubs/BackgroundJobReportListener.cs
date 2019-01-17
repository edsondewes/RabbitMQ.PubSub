using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalrNotification.Model;
using SignalrNotification.Report;

namespace SignalrNotification.Hubs
{
    public class BackgroundJobReportListener : IReportListener<BackgroundJobReport>
    {
        private readonly IHubContext<NotificationHub> _hub;

        public BackgroundJobReportListener(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task Receive(BackgroundJobReport obj, string reportId, CancellationToken cancellationToken)
        {
            await _hub.Clients.Group(reportId).SendAsync("ReceiveBackgroundJobReport", obj, cancellationToken);
        }
    }
}
