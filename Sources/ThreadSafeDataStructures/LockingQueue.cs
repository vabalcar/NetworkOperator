using System;
using System.Collections.Generic;

namespace NetworkOperator.DataStructures.ThreadSafe
{
    public class LockingQueue<T>
    {
        private Action onQueueYetNotEmptyCallback;
        public event Action OnQueueYetNotEmpty
        {
            add
            {
                onQueueYetNotEmptyCallback += value;
            }
            remove
            {
                onQueueYetNotEmptyCallback -= value;
            }
        }
        private Queue<T> internalQueue = new Queue<T>();
        private object internalLock = new object();
        public void Enqueue(T item)
        {
            lock(internalLock)
            {
                internalQueue.Enqueue(item);
                if (internalQueue.Count == 1 && onQueueYetNotEmptyCallback != null) onQueueYetNotEmptyCallback();
            }
        }
        public T Dequeue()
        {
            lock(internalLock)
            {
                return internalQueue.Dequeue();
            }
        }
        public bool IsEmpty()
        {
            return internalQueue.Count == 0;
        }
    }
}
