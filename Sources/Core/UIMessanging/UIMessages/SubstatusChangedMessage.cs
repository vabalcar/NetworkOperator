namespace NetworkOperator.Core.UIMessanging.UIMessages
{
    public class SubstatusChangedMessage : IUIMessage
    {
        public string NewSubstatus { get; private set; }
        public SubstatusChangedMessage(string newSubstatus)
        {
            NewSubstatus = newSubstatus;
        }
    }
}
