using NetworkOperator.Core.DataStructures;
using NetworkOperator.Core.OperationDescription;
using System;
using System.Collections.Generic;

namespace NetworkOperator.Core.CommunicationInterfaces
{
    public interface ICommunicationInterface : IRegistrable
    {
        IEnumerable<string> AvailableOperands { get; }
        int AvailableOperandsCount { get; }
        event Action<string> OnOperandAdded;
        event Action<string> OnOperandRemoved;
        event Action<OperandSelectorMessage> OnOperandSelectorMessageReceived;
        event Action<ActionRequest> OnActionRequestReceived;
        event Action OnCommunicationInterfaceDown;
        void Connect();
        void Disconnect();
        void ConnectWith(string operand, bool reliabilityRequired);
        void Send(OperandSelectorMessage operandSelectorMessage);
        void Send(ActionRequest actionRequest);
    }
}
