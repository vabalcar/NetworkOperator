using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor;

namespace NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming
{
    public abstract class ExtendedParallelStream<T> : ParallelStream<T>
    {
        public override R Accept<R>(ITypedStreamVisitor<R> visitor, T data)
        {
            if (visitor is IExtendedTypedStreamVisitor<R> extendedVisitor)
            {
                return Accept(extendedVisitor, data);
            }
            return visitor.Visit(this, data);
        }

        public abstract R Accept<R>(IExtendedTypedStreamVisitor<R> visitor, T data);

        public override R Accept<R>(IStreamVisitor<R> visitor)
        {
            if (visitor is IExtendedStreamVisitor<R> extendedVisitor)
            {
                return Accept(extendedVisitor);
            }
            return visitor.Visit(this);
        }

        public abstract R Accept<R>(IExtendedStreamVisitor<R> visitor);
    }
}
