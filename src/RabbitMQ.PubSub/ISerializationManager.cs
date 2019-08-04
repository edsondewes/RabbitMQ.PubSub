using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.PubSub.Diagnostics;

namespace RabbitMQ.PubSub
{
    public interface ISerializationManager
    {
        string DefaultMimeType { get; }
        T Deserialize<T>(byte[] body, string mimeType = null);
        byte[] Serialize<T>(T obj, string mimeType = null);
        List<byte[]> SerializeBatch<T>(IEnumerable<T> enumerable, string mimeType = null);
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

        public string DefaultMimeType => _default.MimeType;

        public T Deserialize<T>(byte[] body, string mimeType = null)
        {
            var activity = MessageDiagnostics.StartMessageDeserialize();

            var serializer = GetSerializer(mimeType);
            var obj = serializer.Deserialize<T>(body);

            MessageDiagnostics.StopMessageDeserialize(activity);

            return obj;
        }

        public byte[] Serialize<T>(T obj, string mimeType = null)
        {
            var activity = MessageDiagnostics.StartMessageSerialize();

            var serializer = GetSerializer(mimeType);
            var result = serializer.Serialize(obj);

            MessageDiagnostics.StopMessageSerialize(activity);

            return result;
        }

        public List<byte[]> SerializeBatch<T>(IEnumerable<T> enumerable, string mimeType = null)
        {
            var activity = MessageDiagnostics.StartMessageSerialize();

            var serializer = GetSerializer(mimeType);
            var list = new List<byte[]>();
            foreach (var item in enumerable)
            {
                list.Add(serializer.Serialize(item));
            }

            MessageDiagnostics.StopMessageSerialize(activity);

            return list;
        }

        private ISerializer GetSerializer(string mimeType = null)
        {
            if (mimeType is null)
                return _default;

            return _dic[mimeType];
        }
    }
}
