using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreams;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor;
using System.Collections.Generic;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitors
{
    public abstract class StreamTypeIdentifier<T> : StreamVisitor<T>
    {
        private Dictionary<VisitableStream, T> ids = new Dictionary<VisitableStream, T>();
        public override T Visit(ParallelBinaryStream stream) => GetId(stream);
        public override T Visit(ParallelStringStream stream) => GetId(stream);
        public override T Visit(ParallelInternalMessageStream stream) => GetId(stream);

        protected T GetId(VisitableStream stream)
        {
            if (ids.TryGetValue(stream, out T value))
            {
                return value;
            }
            else
            {
                T newId = GenerateNewId();
                ids.Add(stream, newId);
                return newId;
            }
        }
        protected abstract T GenerateNewId();
    }
}
