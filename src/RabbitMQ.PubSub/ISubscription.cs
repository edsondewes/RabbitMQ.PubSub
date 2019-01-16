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
        private bool _disposed = false;

        public SubscriptionImpl(IModel model, string consumerTag)
        {
            _consumerTag = consumerTag;
            _model = model;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _model.BasicCancel(_consumerTag);
                _model.Dispose();
                _disposed = true;
            }
        }
    }
}
