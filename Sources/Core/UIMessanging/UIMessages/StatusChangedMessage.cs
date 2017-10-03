namespace NetworkOperator.Core.UIMessanging.UIMessages
{
    public class StatusChangedMessage : IUIMessage
    {
        public string NewStatus { get; private set; }
        public StatusChangedMessage(string newStatus)
        {
            NewStatus = newStatus;
        }
    }
}
