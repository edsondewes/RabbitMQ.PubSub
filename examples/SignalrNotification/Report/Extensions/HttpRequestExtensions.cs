using System;
using static SignalrNotification.Report.ReportHeaderConstants;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpRequestExtensions
    {
        public static string ExtractReportId(this HttpRequest request)
        {
            if (!request.Headers.ContainsKey(ReportHeaderName))
                throw new Exception();

            var reportId = request.Headers[ReportHeaderName][0];
            return reportId;
        }
    }
}
