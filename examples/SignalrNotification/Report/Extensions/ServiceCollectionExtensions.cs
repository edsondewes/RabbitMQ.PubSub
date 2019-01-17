using SignalrNotification.HostedServices;
using SignalrNotification.Report;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReportListener<TObj, TService>(this IServiceCollection services)
            where TService : class, IReportListener<TObj>
        {
            var routingKey = $"report_{typeof(TObj).FullName}";

            services.AddTransient<IReportListener<TObj>, TService>();

            services.AddAsyncConsumer<TObj, ReportConsumer<TObj>>(builder => builder
                .ForRoutingKeys(routingKey)
                );

            return services;
        }
    }
}
