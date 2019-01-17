namespace SignalrNotification.Report
{
    public interface IReportProgress<T>
    {
        void Report(T value, string reportId);
    }
}
