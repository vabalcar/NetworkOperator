using System.Windows;

namespace NetworkOperator.UserInterfaces.InformedControls
{
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

    interface IInformedControl
    {
        event MessageReceivedEventHandler MessageReceived;
        RoutedEvent RegisteredMessageReceivedEvent { get; }
        void RaiseEvent(RoutedEventArgs e);
    }
}
