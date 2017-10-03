using System;
using NetworkOperator.CommunicationInterfaces.Connection;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreams;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor
{
    public abstract class TypedStreamVisitor<R> : ITypedStreamVisitor<R>
    {
        public R Visit<T>(ParallelStream<T> stream, T data)
        {
            return stream.Accept(this, data);
        }
        public abstract R Visit(ParallelBinaryStream stream, byte[] data);
        public abstract R Visit(ParallelStringStream stream, string data);
        public abstract R Visit(ParallelInternalMessageStream stream, InternalMessage message);
    }
}
