using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreams;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor
{
    public interface IStreamVisitor<R>
    {
        R Visit(VisitableStream stream);
        R Visit(ParallelBinaryStream stream);
        R Visit(ParallelStringStream stream);
        R Visit(ParallelInternalMessageStream stream);
    }
}
