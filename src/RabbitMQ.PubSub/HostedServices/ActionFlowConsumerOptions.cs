namespace RabbitMQ.PubSub.HostedServices
{
    public class ActionFlowConsumerOptions
    {
        public string Exchange { get; set; }
        public int MaxDegreeOfParallelism { get; set; } = 1;
        public string QueueName { get; set; }
        public string[] RoutingKeys { get; set; }
    }

    public interface IActionFlowConsumerOptionsBuilder
    {
        IActionFlowConsumerOptionsBuilder ForExchange(string exchange);
        IActionFlowConsumerOptionsBuilder ForRoutingKeys(params string[] routingKeys);
        IActionFlowConsumerOptionsBuilder WithMaxDegreeOfParallelism(int maxDegreeOfParallelism);
        IActionFlowConsumerOptionsBuilder WithQueueName(string queueName);
    }

    internal class ActionFlowConsumerOptionsBuilder : IActionFlowConsumerOptionsBuilder
    {
        internal ActionFlowConsumerOptions Options { get; }

        public ActionFlowConsumerOptionsBuilder()
        {
            Options = new ActionFlowConsumerOptions();
        }

        public IActionFlowConsumerOptionsBuilder ForExchange(string exchange)
        {
            Options.Exchange = exchange;
            return this;
        }

        public IActionFlowConsumerOptionsBuilder ForRoutingKeys(params string[] routingKeys)
        {
            Options.RoutingKeys = routingKeys;
            return this;
        }

        public IActionFlowConsumerOptionsBuilder WithMaxDegreeOfParallelism(int maxDegreeOfParallelism)
        {
            Options.MaxDegreeOfParallelism = maxDegreeOfParallelism;
            return this;
        }

        public IActionFlowConsumerOptionsBuilder WithQueueName(string queueName)
        {
            Options.QueueName = queueName;
            return this;
        }
    }
}
