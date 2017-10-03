using NetworkOperator.Core;
using System.Text;

namespace NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming.ParallelStreams
{
    public class ParallelOperandSelectorMessageStream : ExtendedParallelStream<OperandSelectorMessage>
    {
        public override R Accept<R>(IExtendedTypedStreamVisitor<R> visitor, OperandSelectorMessage data)
        {
            return visitor.Visit(this, data);
        }
        public override R Accept<R>(IExtendedStreamVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public override OperandSelectorMessage FromBytes(byte[] data) => new OperandSelectorMessage(Encoding.Unicode.GetString(data));
        public override byte[] GetBytes(OperandSelectorMessage message) => Encoding.Unicode.GetBytes(message.Content);
    }
}
