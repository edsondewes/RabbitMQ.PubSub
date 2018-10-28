using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMQ.PubSub.HostedServices
{
    public class ActionFlowConsumerOptions<TObj>
    {
        public string Exchange { get; set; }
        public int MaxDegreeOfParallelism { get; set; } = 1;
        public List<IConsumerPipeline<TObj>> Pipelines { get; set; }
        public string QueueName { get; set; }
        public string[] RoutingKeys { get; set; }

        public ActionFlowConsumerOptions()
        {
            Pipelines = new List<IConsumerPipeline<TObj>>();
        }
    }

    public interface IActionFlowConsumerOptionsBuilder<TObj>
    {
        IActionFlowConsumerOptionsBuilder<TObj> ForExchange(string exchange);
        IActionFlowConsumerOptionsBuilder<TObj> ForRoutingKeys(params string[] routingKeys);
        IActionFlowConsumerOptionsBuilder<TObj> WithMaxDegreeOfParallelism(int maxDegreeOfParallelism);
        IActionFlowConsumerOptionsBuilder<TObj> WithPipeline<TPipe>() where TPipe : IConsumerPipeline<TObj>;
        IActionFlowConsumerOptionsBuilder<TObj> WithQueueName(string queueName);
    }

    internal class ActionFlowConsumerOptionsBuilder<TObj> : IActionFlowConsumerOptionsBuilder<TObj>
    {
        internal ActionFlowConsumerOptions<TObj> Options { get; }
        private readonly IServiceProvider _serviceProvider;

        public ActionFlowConsumerOptionsBuilder(IServiceProvider serviceProvider)
        {
            Options = new ActionFlowConsumerOptions<TObj>();
            _serviceProvider = serviceProvider;
        }

        public IActionFlowConsumerOptionsBuilder<TObj> ForExchange(string exchange)
        {
            Options.Exchange = exchange;
            return this;
        }

        public IActionFlowConsumerOptionsBuilder<TObj> ForRoutingKeys(params string[] routingKeys)
        {
            Options.RoutingKeys = routingKeys;
            return this;
        }

        public IActionFlowConsumerOptionsBuilder<TObj> WithMaxDegreeOfParallelism(int maxDegreeOfParallelism)
        {
            Options.MaxDegreeOfParallelism = maxDegreeOfParallelism;
            return this;
        }

        public IActionFlowConsumerOptionsBuilder<TObj> WithPipeline<TPipe>()
            where TPipe : IConsumerPipeline<TObj>
        {
            Options.Pipelines.Add(ActivatorUtilities.GetServiceOrCreateInstance<TPipe>(_serviceProvider));
            return this;
        }

        public IActionFlowConsumerOptionsBuilder<TObj> WithQueueName(string queueName)
        {
            Options.QueueName = queueName;
            return this;
        }
    }
}
