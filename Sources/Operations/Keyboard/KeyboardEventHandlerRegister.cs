using NetworkOperator.InteropServices;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace NetworkOperator.IO
{
    public delegate void KeyboardEventHandler(KeyboardEventArgs keyboardEventArgs);

    public class KeyboardEventHandlerRegister : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr LowLevelKeyboardProc(int nCode, UIntPtr keyboardMessageIdentifier, IntPtr keyboardLowLevelHook);

        private class LowLevelKeyboardProcProcessor
        {
            public KeyboardEventArgs Process(UIntPtr keyboardMessageIdentifier, IntPtr keyboardLowLevelHook)
            {
                KBDLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(keyboardLowLevelHook);
                var eventArgs = new KeyboardEventArgs()
                {
                    IsSimulated = (hookStruct.extraInfo == KeyboardEventSimulator.SIMULATED_KEYBOARD_EVENT_TAG),
                    Timestamp = hookStruct.time,
                    Key = new Key() { VirtualKey = hookStruct.vkCode }
                };
                switch ((WM_)keyboardMessageIdentifier)
                {
                    case WM_.KEYDOWN:
                        eventArgs.KeyType = KeyType.Normal;
                        eventArgs.KeyStatus = KeyStatus.Pressed;
                        break;
                    case WM_.KEYUP:
                        eventArgs.KeyType = KeyType.Normal;
                        eventArgs.KeyStatus = KeyStatus.Released;
                        break;
                    case WM_.SYSKEYDOWN:
                        eventArgs.KeyType = KeyType.System;
                        eventArgs.KeyStatus = KeyStatus.Pressed;
                        break;
                    case WM_.SYSKEYUP:
                        eventArgs.KeyType = KeyType.System;
                        eventArgs.KeyStatus = KeyStatus.Released;
                        break;
                    default:
                        break;
                }
                return eventArgs;
            }
        }

        private class KeyboardEventHandlerWrapper
        {
            [DllImport(WindowsAPILibraries.USER32_DLL,
                CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, UIntPtr wParam, IntPtr lParam);

            private KeyboardEventHandler wrappedDelegate;
            private LowLevelKeyboardProcProcessor lowLevelKeyboardProcProcessor;


            public IntPtr HookId { get; set; }

            public KeyboardEventHandlerWrapper(KeyboardEventHandler delegateToWrap,
                LowLevelKeyboardProcProcessor lowLevelKeyboardProcProcessor)
            {
                wrappedDelegate = delegateToWrap;
                this.lowLevelKeyboardProcProcessor = lowLevelKeyboardProcProcessor;
            }
            public IntPtr LowLevelKeyboardProc(int nCode, UIntPtr keyboardMessageIdentifier, IntPtr keyboardLowLevelHook)
            {
                if (nCode < 0)
                {
                    return CallNextHookEx(HookId, nCode, keyboardMessageIdentifier, keyboardLowLevelHook);
                }
                var keyboardEventArgs = lowLevelKeyboardProcProcessor.Process(keyboardMessageIdentifier, keyboardLowLevelHook);
                if (keyboardEventArgs == null)
                {
                    return CallNextHookEx(HookId, nCode, keyboardMessageIdentifier, keyboardLowLevelHook);
                }
                wrappedDelegate(keyboardEventArgs);
                if (keyboardEventArgs.Cancel)
                {
                    return new IntPtr(1);//true in C languages
                }
                else
                {
                    return CallNextHookEx(HookId, nCode, keyboardMessageIdentifier, keyboardLowLevelHook);
                }
            }
        }

        /// <returns>Pointer to registered hook if trgistration was successful, null pointer otherwise.</returns>
        [DllImport(WindowsAPILibraries.USER32_DLL,
            CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(WH_ hookType, IntPtr callbackPtr, IntPtr dllHandle, uint ThreadId);

        [DllImport(WindowsAPILibraries.USER32_DLL,
            CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hookPtr);

        private IntPtr NULL = new IntPtr(0);
        private const uint GLOBAL_SCOPE = 0;

        private List<IntPtr> registeredHooks = new List<IntPtr>();
        private FunctionMarshaler marshaler = new FunctionMarshaler();
        private LowLevelKeyboardProcProcessor lowLevelKeyboardProcProcessor = new LowLevelKeyboardProcProcessor();
        private Dispatcher messageLoopDispatcher;

        public KeyboardEventHandlerRegister()
        {
            RunMessageLoop();
        }
        private void RunMessageLoop()
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            new Thread(() => 
            {
                messageLoopDispatcher = Dispatcher.CurrentDispatcher;
                semaphore.Release();
                Application.Run();
            }).Start();
            semaphore.Wait();
            semaphore.Dispose();
        }
        public IntPtr RegisterHandler(KeyboardEventHandler handler) 
            => messageLoopDispatcher.Invoke(() => RegisterHook(new KeyboardEventHandlerWrapper(handler, 
                lowLevelKeyboardProcProcessor)));
        private IntPtr RegisterHook(KeyboardEventHandlerWrapper keyboardEventHandlerWrapper)
        {
            IntPtr hookPtr;
            if ((hookPtr = SetWindowsHookEx(WH_.KEYBOARD_LL, 
                marshaler.GetPointer<LowLevelKeyboardProc>(keyboardEventHandlerWrapper.LowLevelKeyboardProc),
                NULL, GLOBAL_SCOPE)) == NULL)
            {
                throw new SystemException($"Registration of {WH_.KEYBOARD_LL} was unsuccesful.");
            }
            keyboardEventHandlerWrapper.HookId = hookPtr;
            registeredHooks.Add(hookPtr);
            return hookPtr;
        }
        public void UnregisterHandler(IntPtr handlerHookId)
        {
            UnhookWindowsHookEx(handlerHookId);
            marshaler.DisposeDelegate(handlerHookId);
            registeredHooks.Remove(handlerHookId);
        }
        public void Dispose()
        {
            foreach (var hookPtr in registeredHooks)
            {
                UnhookWindowsHookEx(hookPtr);
            }
            registeredHooks.Clear();
            marshaler.Dispose();
            try
            {
                messageLoopDispatcher.Invoke(Application.ExitThread);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
