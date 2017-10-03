using NetworkOperator.CommunicationInterfaces.Connection;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreams;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor
{
    public interface ITypedStreamVisitor<R>
    {
        R Visit<T>(ParallelStream<T> stream, T data);
        R Visit(ParallelBinaryStream stream, byte[] data);
        R Visit(ParallelStringStream stream, string data);
        R Visit(ParallelInternalMessageStream stream, InternalMessage message);
    }
}
