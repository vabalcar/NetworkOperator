namespace NetworkOperator.Core.UIMessanging.UIMessages
{
    public class ProgressChangedMessage : IUIMessage
    {
        public ProgressChangeType ChangeType { get; private set; }
        public int Percentage { get; private set; }
        public ProgressChangedMessage(int percentage)
        {
            ChangeType = ProgressChangeType.PercentageUpdate;
            Percentage = percentage;
        }
        public ProgressChangedMessage(ProgressChangeType changeType)
        {
            ChangeType = changeType;
        }
    }
    public enum ProgressChangeType : byte
    {
        PercentageUpdate, ProcessIsIndeterminate, ProcessCompleted
    }
}
