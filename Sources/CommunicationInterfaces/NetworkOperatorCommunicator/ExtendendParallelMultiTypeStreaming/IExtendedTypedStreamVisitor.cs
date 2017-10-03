using NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming.ParallelStreams;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor;
using NetworkOperator.Core;
using NetworkOperator.Core.OperationDescription;

namespace NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming
{
    public interface IExtendedTypedStreamVisitor<R> : ITypedStreamVisitor<R>
    {
        R Visit(ParallelActionRequestStream stream, ActionRequest actionRequest);
        R Visit(ParallelOperandSelectorMessageStream stream, OperandSelectorMessage message);
    }
}
