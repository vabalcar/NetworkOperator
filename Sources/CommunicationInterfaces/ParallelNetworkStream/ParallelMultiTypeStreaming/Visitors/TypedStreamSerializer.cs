using NetworkOperator.CommunicationInterfaces.Connection;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreams;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitors
{
    public class TypedStreamSerializer : TypedStreamVisitor<byte[]>
    {
        public override byte[] Visit(ParallelBinaryStream stream, byte[] data) => stream.GetBytes(data);
        public override byte[] Visit(ParallelStringStream stream, string data) => stream.GetBytes(data);
        public override byte[] Visit(ParallelInternalMessageStream stream, InternalMessage message) => stream.GetBytes(message);
    }
}
