namespace NetworkOperator.Core.OperationDescription
{
    public abstract class ActionRequestProcessor : OperationComponent
    {
        public ActionRequestProcessor(NetworkOperator networkOperator, Operation parent) : base(networkOperator, parent)
        {
        }

        public abstract void ProcessActionRequest(ActionRequest actionRequest);
    }
}
