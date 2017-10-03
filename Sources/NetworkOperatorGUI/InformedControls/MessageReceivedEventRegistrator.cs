using System.Windows;

namespace NetworkOperator.UserInterfaces.InformedControls
{
    static class MessageReceivedEventRegistrator
    {
        public static RoutedEvent RegisterMessageReceivedEvent<UIElem>() where UIElem : UIElement, IInformedControl
        {
            return EventManager.RegisterRoutedEvent("MessageReceived", RoutingStrategy.Direct, typeof(MessageReceivedEventHandler), typeof(UIElem));
        }
    }
}
