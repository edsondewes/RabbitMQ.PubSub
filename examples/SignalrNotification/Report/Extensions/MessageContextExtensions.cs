using System.Text;
using static SignalrNotification.Report.ReportHeaderConstants;

namespace RabbitMQ.PubSub
{
    public static class MessageContextExtensions
    {
        public static string ExtractReportId(this MessageContext context)
        {
            return Encoding.UTF8.GetString(context.Headers[ReportHeaderName] as byte[]);
        }
    }
}
