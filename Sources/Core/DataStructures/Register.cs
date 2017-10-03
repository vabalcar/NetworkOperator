using NetworkOperator.Core.CommunicationInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NetworkOperator.Core.DataStructures
{
    public class Register<T> : IRegistrable, IRegisterAccessor<T> where T : IRegistrable
    {
        public short ID { get; set; }
        public int Count => types.Count;

        private Dictionary<Type, T> types = new Dictionary<Type, T>();
        private Dictionary<string, T> names = new Dictionary<string, T>();
        private Dictionary<short, T> ids = new Dictionary<short, T>();
        private short nextId = 0;

        public void Add(T item)
        {
            types.Add(item.GetType(), item);
            names.Add(item.GetType().FullName, item);
            item.ID = nextId;
            ids.Add(nextId, item);
            ++nextId;
        }
        public T this[Type type]
        {
            get => types[type];
        }
        public T this[string name]
        {
            get => names[name];
        }
        public T this[short id]
        {
            get => ids[id];
        }
        public IEnumerator<T> GetEnumerator() => types.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
