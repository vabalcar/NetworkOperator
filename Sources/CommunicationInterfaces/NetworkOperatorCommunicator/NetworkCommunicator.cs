using NetworkOperator.Core.CommunicationInterfaces;
using NetworkOperator.Core.OperationDescription;
using NetworkOperator.CommunicationInterfaces.Connection.ConnectionEstablishmentStrategies.BroadcastConnectionEstablishmentStrategies;
using NetworkOperator.CommunicationInterfaces.ExtendendParallelMultiTypeStreaming;
using System;
using System.Collections.Generic;
using NetworkOperator.Core.DataStructures;
using NetworkOperator.Core;

namespace NetworkOperator.CommunicationInterfaces
{
    public class NetworkCommunicator : ParallelNetworkStream, ICommunicationInterface
    {
        short IRegistrable.ID { get; set; }
        public IEnumerable<string> AvailableOperands => ClientRequests.ConnectableClients.Keys;
        public int AvailableOperandsCount => ClientRequests.ConnectableClients.Count;
        public event Action<string> OnOperandAdded
        {
            add
            {
                ClientRequests.OnClientConnected += value;
            }
            remove
            {
                ClientRequests.OnClientConnected -= value;
            }
        }
        public event Action<string> OnOperandRemoved
        {
            add
            {
                ClientRequests.OnClientDisconnected += value;
            }
            remove
            {
                ClientRequests.OnClientDisconnected -= value;
            }
        }
        public event Action<OperandSelectorMessage> OnOperandSelectorMessageReceived
        {
            add
            {
                AddReceivedDataHandler(value);
            }
            remove
            {
                RemoveReceivedDataHandler(value);
            }
        }
        public event Action<ActionRequest> OnActionRequestReceived
        {
            add
            {
                AddReceivedDataHandler(value);
            }
            remove
            {
                RemoveReceivedDataHandler(value);
            }
        }
        public event Action OnCommunicationInterfaceDown
        {
            add
            {
                OnConnectionEnded += value;
            }
            remove
            {
                OnConnectionEnded -= value;
            }
        }

        protected new ExtendedParallelMultiTypeStream ParallelStream { get; private set; }

        public NetworkCommunicator() : this(new ExtendedParallelMultiTypeStream())
        {
        }
        protected NetworkCommunicator(ExtendedParallelMultiTypeStream parallelStream) 
            : base(parallelStream, new HighPerformanceServerStrategy(5000))
        {
            ParallelStream = parallelStream;
        }

        public void Connect()
        {
            Open();
            if (IsOpened)
            {
                ClientRequests.SetBroadcastedType<OperandSelectorMessage>();
            }
        }
        public void Disconnect() => Dispose();
        public void ConnectWith(string operand, bool reliabilityRequired)
            => ClientRequests.ConnectWith(operand, reliabilityRequired? Reliability.Reliable : Reliability.Unreliable);
        public void Send(ActionRequest actionRequest) => Write(actionRequest, ParallelStream.Write);
        public void Send(OperandSelectorMessage operandSelectorMessage) => Write(operandSelectorMessage, ParallelStream.Write);
    }
}
