using System.Windows;
using System.Windows.Controls;

namespace NetworkOperator.UserInterfaces.InformedControls
{
    public class InformedProgressBar : ProgressBar, IInformedControl
    {
        public static readonly RoutedEvent MessageReceivedEvent = MessageReceivedEventRegistrator.RegisterMessageReceivedEvent<InformedProgressBar>();
        public RoutedEvent RegisteredMessageReceivedEvent
        {
            get
            {
                return MessageReceivedEvent;
            }
        }
        public event MessageReceivedEventHandler MessageReceived
        {
            add
            {
                AddHandler(MessageReceivedEvent, value);
            }
            remove
            {
                RemoveHandler(MessageReceivedEvent, value);
            }
        }
        public InformedProgressBar()
        {
            InformedControlsDeliverySystem.Current.Register(this);
        }
    }
}
