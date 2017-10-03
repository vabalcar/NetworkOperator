using NetworkOperator.DataStructures.ThreadSafe;
using System;
using System.Threading;

namespace NetworkOperator.CommunicationInterfaces.VariableWatching
{
    public class VariableWatcher
    {
        public WatchedVariableFactory Factory { get; private set; }
        private int timeoutTickLength;
        public int Timeout { get; set; } = 30;/*seconds*/
        private bool stop = false;

        private IDRegister<SemaphoreSlim> semaphors = new IDRegister<SemaphoreSlim>();
        public VariableWatcher(int timeoutTickLength/*ms*/)
        {
            this.timeoutTickLength = timeoutTickLength;
            Factory = new WatchedVariableFactory(this);
        }
        public void WaitForChange<T>(WatchedVariable<T> variable, T originalValue, Action timeoutAction) where T : IEquatable<T>
        {
            int semaphorId;
            semaphorId = semaphors.Add(new SemaphoreSlim(0));
            new Thread(() => 
            {
                for (int i = 0; i < Timeout; i++)
                {
                    Thread.Sleep(timeoutTickLength);
                    if (!variable.Value.Equals(originalValue) || stop)
                    {
                        semaphors[semaphorId]?.Release();
                        return;
                    }
                }
                timeoutAction();
            }).Start();
            while (variable.Value.Equals(originalValue) && !stop)
            {
                semaphors[semaphorId]?.Wait();
            }
            semaphors.Remove(semaphorId);
        }
        public void Watch()
        {
            foreach (var semaphor in semaphors)
            {
                semaphor.Release();
            }
        }
        public void StopAllWaitings()
        {
            stop = true;
        }
    }
}

