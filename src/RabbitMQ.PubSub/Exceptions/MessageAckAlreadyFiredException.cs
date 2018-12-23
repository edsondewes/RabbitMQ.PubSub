using System;

namespace RabbitMQ.PubSub.Exceptions
{
    public class MessageAckAlreadyFiredException : Exception
    {
        public MessageAckAlreadyFiredException()
        {
        }

        public MessageAckAlreadyFiredException(string message) : base(message)
        {
        }
    }
}
