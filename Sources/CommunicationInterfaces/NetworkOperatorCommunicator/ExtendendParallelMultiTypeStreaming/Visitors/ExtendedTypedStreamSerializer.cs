using NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming.ParallelStreams;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitors;
using NetworkOperator.Core;
using NetworkOperator.Core.OperationDescription;

namespace NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming.Visitors
{
    class ExtendedTypedStreamSerializer : TypedStreamSerializer, IExtendedTypedStreamVisitor<byte[]>
    {
        public byte[] Visit(ParallelActionRequestStream stream, ActionRequest actionRequest) => stream.GetBytes(actionRequest);
        public byte[] Visit(ParallelOperandSelectorMessageStream stream, OperandSelectorMessage message) => stream.GetBytes(message);
    }
}
