using JaegerTracingWeb.BackgroundServices;
using JaegerTracingWeb.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddTracing();

            services.Configure<ConfigRabbitMQ>(Configuration.GetSection("RabbitMQ"));
            services.AddRabbitPubSub();
            services.AddAsyncConsumer<SomeData, SomeDataBackgroundConsumer>(builder => builder.ForRoutingKeys("test"));
            services.AddAsyncConsumer<OtherData, OtherDataBackgroundConsumer>(builder => builder.ForRoutingKeys("test2"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
