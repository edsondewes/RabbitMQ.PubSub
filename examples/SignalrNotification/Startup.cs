using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.PubSub;
using SignalrNotification.HostedServices;
using SignalrNotification.Hubs;
using SignalrNotification.Model;
using SignalrNotification.Report;

namespace SignalrNotification
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSignalR();

            services.Configure<ConfigRabbitMQ>(Configuration.GetSection("RabbitMQ"));
            services.AddRabbitPubSub();

            services.AddSingleton(typeof(IReportProgress<>), typeof(RabbitMQProgress<>));
            services.AddAsyncConsumer<BackgroundJobData, BackgroundJobConsumer>(builder => builder.ForRoutingKeys("BackgroundJob"));
            services.AddReportListener<BackgroundJobReport, BackgroundJobReportListener>();
            services.AddReportListener<ActionReport, ActionReportListener>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSignalR(routes =>
            {
                routes.MapHub<NotificationHub>("/notification");
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
