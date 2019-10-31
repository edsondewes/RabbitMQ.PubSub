using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.PubSub;
using SignalrNotification.Model;
using SignalrNotification.Report;

namespace SignalrNotification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IReportProgress<ActionReport> _progress;
        private readonly IMessageProducer _producer;

        public JobsController(IReportProgress<ActionReport> progress, IMessageProducer producer)
        {
            _progress = progress;
            _producer = producer;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var reportId = Request.ExtractReportId();

            _progress.Report(new ActionReport { Text = "Starting the request..." }, reportId);
            await Task.Delay(1000);

            _progress.Report(new ActionReport { Text = "First step ok!" }, reportId);
            await Task.Delay(1000);

            _progress.Report(new ActionReport { Text = "Request done!" }, reportId);

            return Ok();
        }

        [HttpPost]
        public ActionResult Post()
        {
            var data = new BackgroundJobData
            {
                Id = 1,
                Name = Guid.NewGuid().ToString()
            };

            _producer.Publish(data, PublishOptions.RoutingTo("BackgroundJob").WithReportId(Request));
            return Ok();
        }
    }
}
