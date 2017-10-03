using NetworkOperator.Core.CommunicationInterfaces;
using NetworkOperator.Core.UIMessanging.UIMessages;

namespace NetworkOperator.Core.UIMessanging
{
    public class UIMessenger
    {
        NetworkOperator networkOperator;
        public UIMessenger(NetworkOperator networkOperator)
        {
            this.networkOperator = networkOperator;
        }
        public void SendMessage(IUIMessage message)
            => networkOperator.Registers.ForEach<IUserInterface>(ui => ui.ProcessMessage(message));
    }
}
