using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalrNotification.Model;
using SignalrNotification.Report;

namespace SignalrNotification.Hubs
{
    public class ActionReportListener : IReportListener<ActionReport>
    {
        private readonly IHubContext<NotificationHub> _hub;

        public ActionReportListener(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task Receive(ActionReport obj, string reportId, CancellationToken cancellationToken)
        {
            await _hub.Clients.Group(reportId).SendAsync("ReceiveActionReport", obj, cancellationToken);
        }
    }
}
