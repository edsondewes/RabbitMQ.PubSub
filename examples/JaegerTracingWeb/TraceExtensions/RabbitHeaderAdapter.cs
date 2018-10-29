using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTracing.Propagation;

namespace JaegerTracingWeb.TraceExtensions
{
    public sealed class RabbitHeaderAdapter : ITextMap
    {
        private readonly IDictionary<string, object> _dictionary;

        public RabbitHeaderAdapter(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _dictionary
                .Select(d => new KeyValuePair<string, string>(d.Key, Encoding.UTF8.GetString(d.Value as byte[])))
                .GetEnumerator();
        }

        public void Set(string key, string value)
        {
            _dictionary[key] = value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
