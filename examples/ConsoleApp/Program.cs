using ConsoleApp.BackgroundServices;
using ConsoleApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.PubSub;

namespace ConsoleApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<DelayedLogger>();

                    services.Configure<ConfigRabbitMQ>(hostContext.Configuration.GetSection("RabbitMQ"));
                    services.AddRabbitPubSub();

                    services.AddAsyncConsumer<SomeData, DataLoggerConsumer>(builder => builder
                        .ForRoutingKeys("test")
                        .WithManualAck()
                    );

                    services.AddHostedService<RandomDataProducer>();
                });
    }
}
