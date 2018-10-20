using RabbitMQ.Client;

namespace RabbitMQ.PubSub
{
    internal class BasicSubscription : ISubscription
    {
        private readonly string _consumerTag;
        private readonly IModel _model;

        public BasicSubscription(IModel model, string consumerTag)
        {
            _consumerTag = consumerTag;
            _model = model;
        }

        public void Dispose()
        {
            _model.BasicCancel(_consumerTag);
        }
    }
}
