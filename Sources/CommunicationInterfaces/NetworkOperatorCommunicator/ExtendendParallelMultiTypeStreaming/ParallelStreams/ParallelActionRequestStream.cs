using System;
using NetworkOperator.Core.OperationDescription;

namespace NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming.ParallelStreams
{
    public class ParallelActionRequestStream : ExtendedParallelStream<ActionRequest>
    {
        public override R Accept<R>(IExtendedTypedStreamVisitor<R> visitor, ActionRequest data)
        {
            return visitor.Visit(this, data);
        }

        public override R Accept<R>(IExtendedStreamVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public override ActionRequest FromBytes(byte[] data)
        {
            byte[] content = new byte[data.Length - 2];
            Array.Copy(data, 2, content, 0, content.Length);
            return new ActionRequest()
            {
                ParentOperation = BitConverter.ToInt16(data, 0),
                Content = content
            };
        }

        public override byte[] GetBytes(ActionRequest actionRequest)
        {
            byte[] serializedParentOperation = BitConverter.GetBytes(actionRequest.ParentOperation);
            byte[] serializedActionRequest = new byte[serializedParentOperation.Length + actionRequest.Content.Length];
            serializedParentOperation.CopyTo(serializedActionRequest, 0);
            actionRequest.Content.CopyTo(serializedActionRequest, serializedParentOperation.Length);
            return serializedActionRequest;
        }
    }
}
