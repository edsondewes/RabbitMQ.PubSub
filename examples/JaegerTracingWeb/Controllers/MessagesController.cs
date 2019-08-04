using System;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.PubSub;

namespace JaegerTracingWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageProducer _producer;

        public MessagesController(IMessageProducer producer)
        {
            _producer = producer;
        }

        [HttpGet]
        public ActionResult<SomeData> Get(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = "empty text";
            }

            var message = new SomeData
            {
                Date = DateTime.Now,
                Text = text
            };

            _producer.Publish(message, PublishOptions.RoutingTo("test"));

            return message;
        }
    }
}
