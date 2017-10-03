using NetworkOperator.Core.UIMessanging.UIMessages;
using System.Collections.Generic;
using System.Windows;

namespace NetworkOperator.UserInterfaces.InformedControls
{
    class InformedControlsDeliverySystem
    {
        private List<IInformedControl> registeredInformedControls = new List<IInformedControl>();
        private Window currentWindow;
        private static InformedControlsDeliverySystem instance;
        public static InformedControlsDeliverySystem Current
        {
            get
            {
                if (instance == null)
                {
                    instance = new InformedControlsDeliverySystem();
                }
                return instance;
            }
        }
        public Window CurrentWindow
        {
            get
            {
                return currentWindow;
            }
            set
            {
                registeredInformedControls.Clear();
                currentWindow = value;
            }
        }
        private InformedControlsDeliverySystem() { }
        public void Register(IInformedControl control)
        {
            if (CurrentWindow != Application.Current.MainWindow)
            {
                CurrentWindow = Application.Current.MainWindow;
            }
            registeredInformedControls.Add(control);
        }
        public void DeliverMessage(IUIMessage message)
        {
            if (Application.Current == null)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(() => {
                foreach (var informedControl in registeredInformedControls)
                {
                    informedControl.RaiseEvent(new MessageReceivedEventArgs(informedControl.RegisteredMessageReceivedEvent, message));
                }
            });
        }
    }
}
