using System;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;
using RabbitMQ.PubSub;

namespace JaegerTracingWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageProducer _producer;
        private readonly ITracer _tracer;

        public MessagesController(IMessageProducer producer, ITracer tracer)
        {
            _producer = producer;
            _tracer = tracer;
        }

        [HttpGet]
        public ActionResult<SomeData> Get(string text)
        {
            if (string.IsNullOrEmpty(text))
                text = "empty text";

            var message = new SomeData
            {
                Date = DateTime.Now,
                Text = text
            };

            _producer.Publish(message, PublishOptions
                .RoutingTo("test")
                .WithTraceContext(_tracer));

            return message;
        }
    }
}
