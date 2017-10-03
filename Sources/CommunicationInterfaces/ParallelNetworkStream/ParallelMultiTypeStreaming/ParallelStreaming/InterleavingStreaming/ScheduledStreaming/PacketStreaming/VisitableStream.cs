using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming
{
    public abstract class VisitableStream
    {
        public abstract R Accept<R>(IStreamVisitor<R> visitor);
    }
}
