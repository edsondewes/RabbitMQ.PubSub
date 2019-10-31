using JaegerTracingWeb.BackgroundServices;
using JaegerTracingWeb.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.PubSub;

namespace JaegerTracingWeb
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddTracing();

            services.Configure<ConfigRabbitMQ>(Configuration.GetSection("RabbitMQ"));
            services.AddRabbitPubSub();
            services.AddAsyncConsumer<SomeData, SomeDataBackgroundConsumer>(builder => builder.ForRoutingKeys("test"));
            services.AddAsyncConsumer<OtherData, OtherDataBackgroundConsumer>(builder => builder.ForRoutingKeys("test2"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
