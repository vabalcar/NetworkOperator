using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NetworkOperator.DataStructures.ThreadSafe
{
    public class ShareableImmutableDictionary<TKey, TValue> : IEnumerable<TValue>
    {
        private IImmutableDictionary<TKey, TValue> innerDictionary = ImmutableDictionary.Create<TKey, TValue>();

        private object innerLock = new object();
        public TValue this [TKey key]
        {
            get => innerDictionary[key];
        }
        public bool TryGetValue(TKey key, out TValue value) => innerDictionary.TryGetValue(key, out value);
        public void Add(TKey key, TValue value)
        {
            lock(innerLock)
            {
                innerDictionary = innerDictionary.Add(key, value);
            }
        }
        public IEnumerator<TValue> GetEnumerator() => innerDictionary.Values.GetEnumerator();
        public void Remove(TKey key)
        {
            lock(innerLock)
            {
                innerDictionary = innerDictionary.Remove(key);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
