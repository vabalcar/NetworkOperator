namespace NetworkOperator.Core.UIMessanging.UIMessages
{
    public class ErrorMessage : IUIMessage
    {
        public string Message { get; private set; }
        public ErrorMessage(string message)
        {
            Message = message;
        }
    }
}
