using NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming.ParallelStreams;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor;

namespace NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming
{
    public interface IExtendedStreamVisitor<R> : IStreamVisitor<R>
    {
        R Visit(ParallelActionRequestStream stream);
        R Visit(ParallelOperandSelectorMessageStream stream);
    }
}
