using System;
using System.Collections.Generic;

namespace NetworkOperator.Core.CommunicationInterfaces
{
    public interface IRegisterAccessor<T> : IReadOnlyRegisterAccessor<T>
    {
        void Add(T item);
    }
    public interface IReadOnlyRegisterAccessor<out T> : IEnumerable<T>
    {
        int Count { get; }
        T this[Type type] { get; }
        T this[string name] { get; }
        T this[short id] { get; }
    }
}
