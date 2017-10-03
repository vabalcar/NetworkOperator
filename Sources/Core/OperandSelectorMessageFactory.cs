using System;

namespace NetworkOperator.Core
{
    class OperandSelectorMessageFactory
    {
        private static OperandSelectorMessageFactory current;
        public static OperandSelectorMessageFactory Current
        {
            get
            {
                if (current == null)
                {
                    current = new OperandSelectorMessageFactory();
                }
                return current;
            }
        }
        private NetworkOperator networkOperator;
        public NetworkOperator NetworkOperator
        {
            set
            {
                if (networkOperator == null)
                {
                    networkOperator = value;
                }
                else
                {
                    throw new InvalidOperationException($"{nameof(NetworkOperator)} has been already set.");
                }
            }
        }
        private OperandSelectorMessageFactory()
        {
        }
        public OperandSelectorMessage Create<OperandSelectorType>(OperandSelectorMessageType messageType, string operand) where OperandSelectorType : OperandSelector
        {
            short operandSelectorId = networkOperator.Registers.GetId<OperandSelector, OperandSelectorType>();
            return Create(operandSelectorId, messageType, operand);
        }
        public OperandSelectorMessage Create(Type targetOperandSelectorType, OperandSelectorMessageType messageType, string operand)
        {
            //TODO: Check that targetOperandSelectorType inherits from OperandSelector
            short operandSelectorId = networkOperator.Registers.GetId<OperandSelector>(targetOperandSelectorType);
            return Create(operandSelectorId, messageType, operand);
        }
        public OperandSelectorMessage Create(short operandSelectorId, OperandSelectorMessageType messageType, string operand)
        {
            char messageTypeChar = (char)messageType;
            return new OperandSelectorMessage($"{operandSelectorId}{OperandSelectorMessage.CONTENT_DELIMITER}{messageTypeChar}{OperandSelectorMessage.CONTENT_DELIMITER}{operand}");
        }
    }

    public enum OperandSelectorMessageType
    {
        NotParsed = 0, OperandAvailable = 'A', OperandUnavailable = 'U', SetOperand = 'S'
    }
}
