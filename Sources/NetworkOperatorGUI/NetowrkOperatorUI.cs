using NetworkOperator.Core;
using NetworkOperator.Core.CommunicationInterfaces;
using NetworkOperator.Core.DataStructures;
using NetworkOperator.Core.OperationDescription;
using NetworkOperator.Core.UIMessanging.UIMessages;
using NetworkOperator.UserInterfaces.InformedControls;

namespace NetworkOperator.UserInterfaces
{
    class NetowrkOperatorInterface : IUserInterface, IUserInterfaceController
    {
        short IRegistrable.ID { get; set; }
        private Core.NetworkOperator networkOperator = new Core.NetworkOperator();
        private IUserInterfaceController Controller { get; set; }
        private static NetowrkOperatorInterface current;
        public static NetowrkOperatorInterface Current
        {
            get
            {
                if (current == null) current = new NetowrkOperatorInterface();
                return current;
            }
        }
        public SessionMode SessionMode
        {
            get => Controller.SessionMode;
            set => Controller.SessionMode = value;
        }
        public IReadOnlyRegisterAccessor<Operation> Operations => Controller.Operations;

        private NetowrkOperatorInterface() => Controller = networkOperator.RegisterUI(this);
        public void ProcessMessage(IUIMessage message)
            => InformedControlsDeliverySystem.Current.DeliverMessage(message);
        public void Load() => Controller.Load();
        public void End() => Controller.End();
    }
}
