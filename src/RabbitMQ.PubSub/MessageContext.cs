using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.PubSub.Exceptions;

namespace RabbitMQ.PubSub
{
    public class MessageContext
    {
        private readonly BasicDeliverEventArgs _args;
        private readonly bool _autoAck;
        private readonly IModel _model;

        private bool _ackFired = false;

        public IDictionary<string, object> Headers => _args.BasicProperties.Headers;

        internal MessageContext(BasicDeliverEventArgs args, bool autoAck, IModel model)
        {
            _args = args;
            _autoAck = autoAck;
            _model = model;
        }

        public void Ack()
        {
            EnsureCanAck();
            _model.BasicAck(_args.DeliveryTag, false);
        }

        public void Nack(bool requeue = false)
        {
            EnsureCanAck();
            _model.BasicNack(_args.DeliveryTag, false, requeue);
        }

        public void Reject(bool requeue = false)
        {
            EnsureCanAck();
            _model.BasicReject(_args.DeliveryTag, requeue);
        }

        private void EnsureCanAck()
        {
            if (_autoAck)
            {
                throw new MessageAckAlreadyFiredException("Automatic ack is enabled for this consumer. Cannot ack twice.");
            }

            if (_ackFired)
            {
                throw new MessageAckAlreadyFiredException();
            }

            _ackFired = true;
        }
    }
}
