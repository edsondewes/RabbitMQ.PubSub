using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMQ.PubSub.HostedServices
{
    public class AsyncConsumerOptions<TObj>
    {
        public bool AutoAck { get; set; } = true;
        public string Exchange { get; set; }
        public List<IConsumerPipeline<TObj>> Pipelines { get; set; }
        public string QueueName { get; set; }
        public string[] RoutingKeys { get; set; }

        public AsyncConsumerOptions()
        {
            Pipelines = new List<IConsumerPipeline<TObj>>();
        }
    }

    public interface IAsyncConsumerOptionsBuilder<TObj>
    {
        IAsyncConsumerOptionsBuilder<TObj> ForExchange(string exchange);
        IAsyncConsumerOptionsBuilder<TObj> ForRoutingKeys(params string[] routingKeys);
        IAsyncConsumerOptionsBuilder<TObj> WithManualAck();
        IAsyncConsumerOptionsBuilder<TObj> WithPipeline<TPipe>() where TPipe : IConsumerPipeline<TObj>;
        IAsyncConsumerOptionsBuilder<TObj> WithQueueName(string queueName);
    }

    internal class AsyncConsumerOptionsBuilder<TObj> : IAsyncConsumerOptionsBuilder<TObj>
    {
        internal AsyncConsumerOptions<TObj> Options { get; }
        private readonly IServiceProvider _serviceProvider;

        public AsyncConsumerOptionsBuilder(IServiceProvider serviceProvider)
        {
            Options = new AsyncConsumerOptions<TObj>();
            _serviceProvider = serviceProvider;

            var globalPipelines = serviceProvider.GetServices<IConsumerPipeline<TObj>>();
            Options.Pipelines.AddRange(globalPipelines);
        }

        public IAsyncConsumerOptionsBuilder<TObj> ForExchange(string exchange)
        {
            Options.Exchange = exchange;
            return this;
        }

        public IAsyncConsumerOptionsBuilder<TObj> ForRoutingKeys(params string[] routingKeys)
        {
            Options.RoutingKeys = routingKeys;
            return this;
        }

        public IAsyncConsumerOptionsBuilder<TObj> WithManualAck()
        {
            Options.AutoAck = false;
            return this;
        }

        public IAsyncConsumerOptionsBuilder<TObj> WithPipeline<TPipe>()
            where TPipe : IConsumerPipeline<TObj>
        {
            Options.Pipelines.Add(ActivatorUtilities.GetServiceOrCreateInstance<TPipe>(_serviceProvider));
            return this;
        }

        public IAsyncConsumerOptionsBuilder<TObj> WithQueueName(string queueName)
        {
            Options.QueueName = queueName;
            return this;
        }
    }
}
