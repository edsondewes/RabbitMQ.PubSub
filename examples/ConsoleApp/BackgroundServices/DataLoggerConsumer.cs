﻿using System.Threading;
using System.Threading.Tasks;
using ConsoleApp.Services;
using RabbitMQ.PubSub;

namespace ConsoleApp.BackgroundServices
{
    public class DataLoggerConsumer : IBackgroundConsumer<SomeData>
    {
        private readonly DelayedLogger _delayedLogger;

        public DataLoggerConsumer(DelayedLogger delayedLogger)
        {
            _delayedLogger = delayedLogger;
        }

        public async Task Consume(SomeData obj, MessageContext context, CancellationToken cancellationToken)
        {
            await _delayedLogger.Log("Data received: {id} - {name}", new object[] { obj.Id, obj.Name }, cancellationToken);
            context.Ack();
        }
    }
}
