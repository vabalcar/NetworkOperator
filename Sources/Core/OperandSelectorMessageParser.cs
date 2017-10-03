using System;

namespace NetworkOperator.Core
{
    class OperandSelectorMessageParser
    {
        private static OperandSelectorMessageParser current;
        public static OperandSelectorMessageParser Current
        {
            get
            {
                if (current == null)
                {
                    current = new OperandSelectorMessageParser();
                }
                return current;
            }
        }
        private OperandSelectorMessageParser()
        {
        }
        public OperandSelectorMessage Parse(OperandSelectorMessage unparsedMessage)
        {
            if (unparsedMessage.IsParsed)
            {
                throw new ArgumentException($"{nameof(OperandSelectorMessage)} has been already parsed");
            }
            string[] tokens = unparsedMessage.Content.Split(OperandSelectorMessage.CONTENT_DELIMITER);
            if (tokens.Length == 3 
                && short.TryParse(tokens[0], out short senderId)
                && tokens[1].Length == 1)
            {
                unparsedMessage.SenderId = senderId;
                unparsedMessage.Type = (OperandSelectorMessageType)tokens[1][0];
                unparsedMessage.Operand = tokens[2];
                unparsedMessage.Content = string.Empty;
            }
            else
            {
                throw new ArgumentException($"{nameof(OperandSelectorMessage)} is in invalid format");
            }
            return unparsedMessage;
        }
    }
}
