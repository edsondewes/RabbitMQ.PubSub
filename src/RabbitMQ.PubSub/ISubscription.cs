using System;
using RabbitMQ.Client;

namespace RabbitMQ.PubSub
{
    public interface ISubscription : IDisposable
    {
    }

    internal class SubscriptionImpl : ISubscription
    {
        private readonly string _consumerTag;
        private readonly IModel _model;

        public SubscriptionImpl(IModel model, string consumerTag)
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
