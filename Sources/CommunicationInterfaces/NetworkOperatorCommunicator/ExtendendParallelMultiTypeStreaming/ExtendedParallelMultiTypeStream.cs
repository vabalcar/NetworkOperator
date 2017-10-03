using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming;
using NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming.Visitors;
using NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming.ParallelStreams;
using NetworkOperator.Core.OperationDescription;
using NetworkOperator.Core;

namespace NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming
{
    public class ExtendedParallelMultiTypeStream : ParallelMultiTypeStream
    {
        private ParallelActionRequestStream actionRequestStream = new ParallelActionRequestStream();
        private ParallelOperandSelectorMessageStream operandSelectorMessageStream = new ParallelOperandSelectorMessageStream();

        public ExtendedParallelMultiTypeStream() : base(new ExtendedStreamTypeNumberer(), new ExtendedTypedStreamSerializer())
        {
            RegisterSubstream(actionRequestStream);
            RegisterSubstream(operandSelectorMessageStream);
        }

        public void Write(ActionRequest actionRequest) => Write(actionRequestStream, actionRequest);
        public void Write(OperandSelectorMessage message) => Write(operandSelectorMessageStream, message);
    }
}
