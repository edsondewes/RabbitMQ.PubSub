using System;

namespace RabbitMQ.PubSub.Exceptions
{
    public class InvalidExchangeException : Exception
    {
        public InvalidExchangeException()
            : base("You must specify the exchange using the DefaultExchange config or the options argument")
        {
        }
    }
}
