using System.Threading;
using System.Threading.Tasks;

namespace SignalrNotification.Report
{
    public interface IReportListener<T>
    {
        Task Receive(T obj, string reportId, CancellationToken cancellationToken);
    }
}
