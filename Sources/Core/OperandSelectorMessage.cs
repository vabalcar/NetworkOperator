namespace NetworkOperator.Core
{
    public class OperandSelectorMessage
    {
        public static char CONTENT_DELIMITER = ';';
        public bool IsParsed => Content.Length == 0;
        public short SenderId { get; internal set; }
        public OperandSelectorMessageType Type { get; internal set; }
        public string Operand { get; internal set; }
        public string Content { get; set; }
        public OperandSelectorMessage(string content)
        {
            Content = content;
        }
    }
}
