using Jaeger;
using Jaeger.Samplers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OpenTracing;
using OpenTracing.Util;

namespace JaegerTracingWeb.Diagnostics
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTracing(this IServiceCollection services)
        {
            return services
                .AddSingleton<ITracer>(serviceProvider =>
                {
                    var serviceName = serviceProvider.GetRequiredService<IWebHostEnvironment>().ApplicationName;
                    var tracer = new Tracer.Builder(serviceName)
                        .WithSampler(new ConstSampler(true))
                        .Build();

                    GlobalTracer.Register(tracer);

                    return tracer;
                })
                .AddSingleton<ITracingObserver, AspNetEventObserver>()
                .AddSingleton<ITracingObserver, HttpHandlerObserver>()
                .AddSingleton<ITracingObserver, RabbitMQObserver>()
                .AddHostedService<DiagnosticsHostedService>();
        }
    }
}
