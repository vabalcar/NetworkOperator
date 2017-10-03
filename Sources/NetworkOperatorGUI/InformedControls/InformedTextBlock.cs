using System.Windows;
using System.Windows.Controls;

namespace NetworkOperator.UserInterfaces.InformedControls
{
    public class InformedTextBlock : TextBlock, IInformedControl
    {
        public static readonly RoutedEvent MessageReceivedEvent = MessageReceivedEventRegistrator.RegisterMessageReceivedEvent<InformedTextBlock>();
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
        public InformedTextBlock()
        {
            InformedControlsDeliverySystem.Current.Register(this);
        }
    }
}
