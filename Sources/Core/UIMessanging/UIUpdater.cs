using NetworkOperator.Core.UIMessanging.UIMessages;

namespace NetworkOperator.Core.UIMessanging
{
    public class UIUpdater
    {
        private static UIUpdater current;
        public static UIUpdater Current
        {
            get
            {
                if (current == null)
                {
                    current = new UIUpdater();
                }
                return current;
            }
        }
        public static UIMessenger Messenger { get; set; }
        public static void ChangeProgress(int percentage) 
            => Messenger.SendMessage(new ProgressChangedMessage(percentage));
        public static void ChangeProgress(ProgressChangeType changeType)
            => Messenger.SendMessage(new ProgressChangedMessage(changeType));
        public static void RaiseError(string description) 
            => Messenger.SendMessage(new ErrorMessage(description));
        public static void ChangeStatus(string newStatus)
            => Messenger.SendMessage(new StatusChangedMessage(newStatus));
        public static void ChangeSubstatus(string newSubstatus)
            => Messenger.SendMessage(new SubstatusChangedMessage(newSubstatus));
    }
}
