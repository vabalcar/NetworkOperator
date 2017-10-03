using NetworkOperator.Core.DataStructures;
using NetworkOperator.Core.UIMessanging.UIMessages;

namespace NetworkOperator.Core.CommunicationInterfaces
{
    public interface IUserInterface : IRegistrable
    {
        void ProcessMessage(IUIMessage message);
    }
}
