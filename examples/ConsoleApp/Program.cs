using System.IO;
using System.Threading.Tasks;
using ConsoleApp.BackgroundServices;
using ConsoleApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.PubSub;

namespace ConsoleApp
{
    internal class Program
    {
        public static async Task Main()
        {
            var builder = CreateHostBuilder();
            await builder.RunConsoleAsync();
        }

        public static IHostBuilder CreateHostBuilder() => new HostBuilder()
            .UseEnvironment("Development")
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<DelayedLogger>();

                services.Configure<ConfigRabbitMQ>(hostContext.Configuration.GetSection("RabbitMQ"));
                services.AddRabbitPubSub();

                services.AddActionFlowConsumer<SomeData, DataLoggerConsumer>(builder => builder.ForRoutingKeys("test"));

                services.AddHostedService<RandomDataProducer>();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
            });
    }
}
