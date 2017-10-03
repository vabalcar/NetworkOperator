using NetworkOperator.Core.CommunicationInterfaces;

namespace NetworkOperator.Core.OperationDescription
{
    public abstract class ActionRequestCreator : OperationComponent
    {
        public ActionRequestCreator(NetworkOperator networkOperator, Operation parent) : base(networkOperator, parent)
        {
        }

        public void Send(ActionRequest actionRequest)
        {
            actionRequest.ParentOperation = networkOperator.Registers.GetId<Operation>(parent.GetType());
            networkOperator.Registers.ForEach<ICommunicationInterface>(ci => ci.Send(actionRequest));
        }
    }
}
