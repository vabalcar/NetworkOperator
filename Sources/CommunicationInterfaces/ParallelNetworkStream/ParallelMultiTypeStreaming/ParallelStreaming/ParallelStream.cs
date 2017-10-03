using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming
{
    public abstract class ParallelStream<T> : InterleavingStream
    {

        private Action<T> onDataReceived;

        public event Action<T> OnDataReceived
        {
            add
            {
                onDataReceived += value;
            }
            remove
            {
                onDataReceived -= value;
            }
        }

        public abstract R Accept<R>(ITypedStreamVisitor<R> visitor, T data);
        public abstract override R Accept<R>(IStreamVisitor<R> visitor);

        public abstract byte[] GetBytes(T data);
        public abstract T FromBytes(byte[] data);

        public void Write(T data) => Write(GetBytes(data));
        public void Write(T data, bool wait)
        {
            if (wait)
            {
                Write(data);
            }
            else
            {
                Task.Factory.StartNew(() => Write(data));
            }
        }
        public void WriteParallely(IEnumerable<T> dataSet) => Parallel.ForEach(dataSet, Write);
        public void WriteParallely(IEnumerable<T> dataSet, bool wait)
        {
            if (wait)
            {
                WriteParallely(dataSet);
            }
            else
            {
                Task.Factory.StartNew(() => WriteParallely(dataSet));
            }
        }

        public void ReadNextData()
        {
            byte[] readData = Read();
            if (readData == null)
            {
                return;
            }
            Process(readData);
        }

        public override void Process(byte[] data)
        {
            if (onDataReceived == null || data == null)
            {
                return;
            }
            Task.Factory.StartNew(() => onDataReceived(FromBytes(data)));
        }
    }
}
