using NetworkOperator.Core.UIMessanging.UIMessages;
using System.Windows;

namespace NetworkOperator.UserInterfaces.InformedControls
{
    public class MessageReceivedEventArgs : RoutedEventArgs
    {
        public IUIMessage Message { get; set; }
        public MessageReceivedEventArgs(RoutedEvent e, IUIMessage message) : base(e)
        {
            Message = message;
        }
    }
}
