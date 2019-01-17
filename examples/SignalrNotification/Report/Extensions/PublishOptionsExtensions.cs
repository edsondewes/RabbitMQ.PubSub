using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using static SignalrNotification.Report.ReportHeaderConstants;

namespace RabbitMQ.PubSub
{
    public static class PublishOptionsExtensions
    {
        public static PublishOptions WithReportId(this PublishOptions options, MessageContext context)
        {
            if (context.Headers != null && context.Headers.ContainsKey(ReportHeaderName))
            {
                var reportId = context.Headers[ReportHeaderName];
                options.WithReportId(reportId);
            }

            return options;
        }

        public static PublishOptions WithReportId(this PublishOptions options, HttpRequest request)
        {
            if (request.Headers.ContainsKey(ReportHeaderName))
            {
                var reportId = request.Headers[ReportHeaderName][0];
                options.WithReportId(reportId);
            }

            return options;
        }

        public static PublishOptions WithReportId(this PublishOptions options, object value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options.Headers is null)
            {
                options.Headers = new Dictionary<string, object>();
            }

            options.Headers.Add(ReportHeaderName, value);
            return options;
        }
    }
}
