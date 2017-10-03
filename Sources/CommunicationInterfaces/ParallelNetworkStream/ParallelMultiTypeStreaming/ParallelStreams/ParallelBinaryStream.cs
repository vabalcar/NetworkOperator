using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreams
{
    public class ParallelBinaryStream : ParallelStream<byte[]>
    {
        public override R Accept<R>(ITypedStreamVisitor<R> visitor, byte[] data)
        {
            return visitor.Visit(this, data);
        }
        public override R Accept<R>(IStreamVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public override byte[] FromBytes(byte[] data) => data;

        public override byte[] GetBytes(byte[] data) => data;
    }
}
