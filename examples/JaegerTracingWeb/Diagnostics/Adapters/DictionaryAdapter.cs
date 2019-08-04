using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using OpenTracing.Propagation;

namespace JaegerTracingWeb.Diagnostics.Adapters
{
    internal sealed class DictionaryAdapter : ITextMap
    {
        private readonly IDictionary<string, object> _dictionary;

        public DictionaryAdapter(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        }

        public void Set(string key, string value)
        {
            if (_dictionary.ContainsKey(key))
            {
                _dictionary.Remove(key);
            }

            _dictionary.Add(key, value);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var kvp in _dictionary)
            {
                yield return new KeyValuePair<string, string>(kvp.Key, Encoding.UTF8.GetString(kvp.Value as byte[]));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
