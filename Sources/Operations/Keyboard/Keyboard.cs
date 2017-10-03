using System;
using System.Collections.Generic;

namespace NetworkOperator.IO
{
    public class Keyboard : IDisposable
    {
        private Action<KeyboardEventArgs> onKeyStateChanged;
        public event Action<KeyboardEventArgs> OnKeyStateChanged
        {
            add
            {
                onKeyStateChanged += value;
                RegisterEventHandler(e =>
                {
                    if (e.Key.IsMouseKey)
                    {
                        return;
                    }
                    e.Cancel = cancelEventsByDefault;
                    onKeyStateChanged(e);
                }, value);
            }
            remove
            {
                onKeyStateChanged -= value;
                UnregisterEventHandler(value);
            }
        }

        private KeyboardEventHandlerRegister eventHandlerRegister = new KeyboardEventHandlerRegister();
        private Dictionary<Delegate, IntPtr> eventHandlerHookIds = new Dictionary<Delegate, IntPtr>();
        private KeyboardEventSimulator simulator = new KeyboardEventSimulator();
        private bool cancelEventsByDefault = false;
        public bool Blocked
        {
            get => cancelEventsByDefault;
            set => cancelEventsByDefault = value;
        }

        private void RegisterEventHandler(KeyboardEventHandler handler, Delegate userHandler)
            => eventHandlerHookIds.Add(userHandler, eventHandlerRegister.RegisterHandler(handler));
        private void UnregisterEventHandler(Delegate handler)
        {
            if (eventHandlerHookIds.TryGetValue(handler, out IntPtr handlerId))
            {
                eventHandlerRegister.UnregisterHandler(handlerId);
                eventHandlerHookIds.Remove(handler);
            }
        }
        public void Simulate(KeyboardEventArgs keyboardEventArgs)
            => Simulate(new KeyboardEventArgs[] { keyboardEventArgs });
        public void Simulate(KeyboardEventArgs[] multipleKeyboradEventArgs) 
            => simulator.Simulate(multipleKeyboradEventArgs);
        public void Dispose()
        {
            eventHandlerRegister.Dispose();
        }
    }

    public enum KeyStatus : byte
    {
        Pressed, Released
    }

    public enum KeyType : byte
    {
        Normal, System
    }
}
