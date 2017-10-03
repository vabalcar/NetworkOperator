using System.Collections;
using System.Collections.Generic;

namespace NetworkOperator.DataStructures.ThreadSafe
{
    public class IDRegister<T> : IEnumerable<T>
    {
        private class Identifier
        {
            private int highestAssignedId = 0;
            private Queue<int> unasignedUsedIds = new Queue<int>();
            public int GetId()
            {
                if (unasignedUsedIds.Count == 0)
                {
                    return highestAssignedId++;
                }
                else
                {
                    return unasignedUsedIds.Dequeue();
                }
            }
            public void UnassigneId(int id)
            {
                unasignedUsedIds.Enqueue(id);
            }
        }
        private Dictionary<int, T> register = new Dictionary<int, T>();
        private Identifier identifier = new Identifier();
        private object internalLock = new object();
        public T this[int id]
        {
            get
            {
                lock(internalLock)
                {
                    if (register.TryGetValue(id, out T value))
                    {
                        return value;
                    }
                    return default(T);
                }
            }
        }
        public int Add(T item)
        {
            lock(internalLock)
            {
                int id = identifier.GetId();
                register[id] = item;
                return id;
            }
        }
        public void Remove(int id)
        {
            lock(internalLock)
            {
                register.Remove(id);
                identifier.UnassigneId(id);
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            List<T> values = new List<T>();
            lock(internalLock)
            {
                foreach (var item in register.Values)
                {
                    values.Add(item);
                }
            }
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
