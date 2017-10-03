using System;

namespace NetworkOperator.Core
{
    class OperandSelectorMessageProcessor
    {
        private static OperandSelectorMessageProcessor current;
        public static OperandSelectorMessageProcessor Current
        {
            get
            {
                if (current == null)
                {
                    current = new OperandSelectorMessageProcessor();
                }
                return current;
            }
        }
        private NetworkOperator networkOperator;
        public NetworkOperator NetworkOperator
        {
            get
            {
                if (networkOperator == null)
                {
                    throw new NullReferenceException($"{nameof(NetworkOperator)} hasn't been set");
                }
                return networkOperator;
            }
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
        public void ProcessUnparsedMessage(OperandSelectorMessage unparsedMessage)
            => ProcessParsedMessage(OperandSelectorMessageParser.Current.Parse(unparsedMessage));
        public void ProcessParsedMessage(OperandSelectorMessage parsedMessage)
        {
            switch (parsedMessage.Type)
            {
                case OperandSelectorMessageType.NotParsed:
                    throw new ArgumentException($"{nameof(OperandSelectorMessage)} must be parsed before being processed");
                case OperandSelectorMessageType.OperandAvailable:
                    NetworkOperator.Registers.Get<OperandSelector>(parsedMessage.SenderId).RegisterOperand(parsedMessage.Operand);
                    break;
                case OperandSelectorMessageType.OperandUnavailable:
                    NetworkOperator.Registers.Get<OperandSelector>(parsedMessage.SenderId).UnregisterOperand(parsedMessage.Operand);
                    break;
                case OperandSelectorMessageType.SetOperand:
                    NetworkOperator.Registers.Get<OperandSelector>(parsedMessage.SenderId).SelectOperand(parsedMessage.Operand);
                    break;
                default:
                    break;
            }
        }
    }
}
