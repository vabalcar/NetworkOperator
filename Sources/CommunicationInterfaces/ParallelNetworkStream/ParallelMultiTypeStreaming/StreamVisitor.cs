using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreams;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor
{
    public abstract class StreamVisitor<R> : IStreamVisitor<R>
    {
        public R Visit(VisitableStream stream)
        {
            return stream.Accept(this);
        }
        public abstract R Visit(ParallelBinaryStream stream);
        public abstract R Visit(ParallelStringStream stream);
        public abstract R Visit(ParallelInternalMessageStream stream);
    }
}
