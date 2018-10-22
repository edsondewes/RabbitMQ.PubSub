using System;
using System.Collections.Generic;
using System.Linq;

namespace RabbitMQ.PubSub
{
    public interface ISerializationManager
    {
        ISerializer GetSerializer(string mimeType = null);
    }

    internal class SerializationManagerImpl : ISerializationManager
    {
        private readonly Dictionary<string, ISerializer> _dic;
        private readonly ISerializer _default;

        public SerializationManagerImpl(IEnumerable<ISerializer> serializers)
        {
            if (!serializers.Any())
                throw new ArgumentException("It's necessary to register at least one serializer", nameof(serializers));

            _dic = serializers.ToDictionary(key => key.MimeType);
            _default = serializers.First();
        }

        public ISerializer GetSerializer(string mimeType = null)
        {
            if (string.IsNullOrEmpty(mimeType))
                return _default;

            return _dic[mimeType];
        }
    }
}
