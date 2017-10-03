using NetworkOperator.Core.CommunicationInterfaces;
using NetworkOperator.Core.DataStructures;
using System;

namespace NetworkOperator.Core.OperationDescription
{
    public abstract class Operation : IRegistrable, IDisposable
    {
        short IRegistrable.ID { get; set; }
        public abstract ActionRequestCreator ActionRequestCreator { get; }
        public abstract ActionRequestProcessor ActionRequestProcessor { get; }
        public abstract Type OperandSelectorType { get; }
        public abstract string OperandSelectorFieldName { get; }
        public NetworkOperator NetworkOperator { get; set; }
        public OperationAction LastPerformedAction { get; private set; } = OperationAction.Stop;
        public OperationStatus Status => IsRunning ? OperationStatus.Running : OperationStatus.Stopped;
        public bool IsRunning => LastPerformedAction != OperationAction.Stop && LastPerformedAction != OperationAction.Pause;
        public abstract OperationInfo Info { get; }
        public abstract Configuration Configuration { get; }
        public Operation(NetworkOperator networkOperator)
        {
            NetworkOperator = networkOperator;
        }
        public void PerformAction(OperationAction action)
        {
            LastPerformedAction = action;
            switch (action)
            {
                case OperationAction.Start:
                    ActionRequestCreator.Start();
                    ActionRequestProcessor.Start();
                    InformAboutCurrentStatus(OperandSelectorMessageType.OperandAvailable);
                    break;
                case OperationAction.Pause:
                    InformAboutCurrentStatus(OperandSelectorMessageType.OperandUnavailable);
                    ActionRequestCreator.Pause();
                    ActionRequestProcessor.Pause();
                    break;
                case OperationAction.Resume:
                    InformAboutCurrentStatus(OperandSelectorMessageType.OperandAvailable);
                    ActionRequestCreator.Resume();
                    ActionRequestProcessor.Resume();
                    break;
                case OperationAction.Stop:
                    InformAboutCurrentStatus(OperandSelectorMessageType.OperandUnavailable);
                    ActionRequestCreator.Stop();
                    ActionRequestProcessor.Stop();
                    break;
                default:
                    break;
            }
        }
        private void InformAboutCurrentStatus(OperandSelectorMessageType type)
        {
            string localMachineName = NetworkOperator.Registers.Get<OperandSelector>(OperandSelectorType.FullName).LocalMachineName;
            NetworkOperator.Registers.ForEach<ICommunicationInterface>(ci => ci.Send(OperandSelectorMessageFactory.Current.Create(OperandSelectorType, type, localMachineName)));
        }
        public void ToNextStatus()
        {
            switch (Status)
            {
                case OperationStatus.Running:
                    PerformAction(OperationAction.Pause);
                    break;
                case OperationStatus.Stopped:
                    if (LastPerformedAction == OperationAction.Pause)
                    {
                        PerformAction(OperationAction.Resume);
                    }
                    else
                    {
                        PerformAction(OperationAction.Start);
                    }
                    break;
                default:
                    break;
            }
        }
        public virtual void Dispose()
        {
            if (IsRunning)
            {
                PerformAction(OperationAction.Stop);
            }
        }
    } 
    
    public enum OperationAction : byte
    {
        Start, Stop, Pause, Resume
    }
    public enum OperationStatus : byte
    {
        Running, Stopped
    }
}
