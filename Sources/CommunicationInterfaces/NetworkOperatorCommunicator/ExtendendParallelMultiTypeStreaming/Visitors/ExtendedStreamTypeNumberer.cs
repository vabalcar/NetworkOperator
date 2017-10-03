using NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming.ParallelStreams;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitors;

namespace NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming.Visitors
{
    class ExtendedStreamTypeNumberer : StreamTypeNumberer, IExtendedStreamVisitor<short>
    {
        public short Visit(ParallelActionRequestStream stream) => GetId(stream);
        public short Visit(ParallelOperandSelectorMessageStream stream) => GetId(stream);
    }
}
