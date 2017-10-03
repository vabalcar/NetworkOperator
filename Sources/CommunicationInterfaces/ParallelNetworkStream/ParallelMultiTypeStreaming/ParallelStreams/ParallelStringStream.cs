using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor;
using System.Text;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreams
{
    public class ParallelStringStream : ParallelStream<string>
    {
        public override R Accept<R>(ITypedStreamVisitor<R> visitor, string data)
        {
            return visitor.Visit(this, data);
        }

        public override R Accept<R>(IStreamVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public override string FromBytes(byte[] data) => Encoding.Unicode.GetString(data);

        public override byte[] GetBytes(string data) => Encoding.Unicode.GetBytes(data);
    }
}
