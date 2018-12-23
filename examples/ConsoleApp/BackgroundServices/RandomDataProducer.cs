using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.PubSub;

namespace ConsoleApp.BackgroundServices
{
    public class RandomDataProducer : BackgroundService
    {
        private readonly IMessageProducer _producer;
        private readonly Random _random;

        public RandomDataProducer(IMessageProducer producer)
        {
            _producer = producer;
            _random = new Random();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const string RountingKey = "test";

            while (!stoppingToken.IsCancellationRequested)
            {
                var number = _random.Next();
                var data = new SomeData
                {
                    Id = number,
                    Name = $"Some Name {number}"
                };

                _producer.Publish(data, PublishOptions.RoutingTo(RountingKey));
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
