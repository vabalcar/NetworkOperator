using NetworkOperator.Core.DataStructures;
using System;
using System.Collections.Generic;

namespace NetworkOperator.Core
{
    public abstract class OperandSelector<T> : OperandSelector
    {
        public abstract void SelectOperand(T operand, Action<string> onSuccess);
    }
    public abstract class OperandSelector : IRegistrable
    {
        short IRegistrable.ID { get; set; }
        public abstract event Action<string> OnOperandChanged;
        public abstract bool IsReliableTransferRequired { get; protected set; }
        public abstract bool AutoInformOthersAboutNewOperand { get; protected set; }
        public abstract bool IsLocalMachineSelected { get; protected set; }
        public abstract string LocalMachineName { get; protected set; }
        public abstract string SelectedOperand { get; protected set; }
        public abstract void SelectOperand(string operand);
        public abstract bool IsOperandAvailable(string operand);
        public abstract void SetOperandsAvailability(IEnumerable<string> availableOperands, int availableOperandsCount);
        public abstract void RegisterOperand(string operand);
        public abstract void UnregisterOperand(string operand);
        public abstract void UnregisterAllOperands();
    }
}
