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
    public delegate void MouseEventHandler(MouseEventArgs mouseEventArgs);

    public class MouseEventHandlerRegister : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr LowLevelMouseProc(int nCode, UIntPtr mouseMessageIdentifier, IntPtr mouseLowLevelHook);

        private class LowLevelMouseProcProcessor
        {
            public MouseEventArgs Process(UIntPtr mouseMessageIdentifier, IntPtr mouseLowLevelHook)
            {
                MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(mouseLowLevelHook);
                var eventArgs = new MouseEventArgs()
                {
                    IsSimulated = (hookStruct.extraInfo == MouseEventSimulator.SIMULATED_MOUSE_EVENT_TAG),
                    Timestamp = hookStruct.time,
                    Position = new Position()
                    {
                        X = hookStruct.point.x,
                        Y = hookStruct.point.y
                    }
                };
                switch ((WM_)mouseMessageIdentifier)
                {
                    case WM_.MOUSEMOVE:
                        eventArgs.EventType = MouseEventType.Movement;
                        break;
                    case WM_.LBUTTONDOWN:
                        eventArgs.EventType = MouseEventType.ButtonStateChanged;
                        eventArgs.Button = MouseButton.Left;
                        eventArgs.ButtonState = MouseButtonState.Pressed;
                        break;
                    case WM_.LBUTTONUP:
                        eventArgs.EventType = MouseEventType.ButtonStateChanged;
                        eventArgs.Button = MouseButton.Left;
                        eventArgs.ButtonState = MouseButtonState.Released;
                        break;
                    case WM_.RBUTTONDOWN:
                        eventArgs.EventType = MouseEventType.ButtonStateChanged;
                        eventArgs.Button = MouseButton.Right;
                        eventArgs.ButtonState = MouseButtonState.Pressed;
                        break;
                    case WM_.RBUTTONUP:
                        eventArgs.EventType = MouseEventType.ButtonStateChanged;
                        eventArgs.Button = MouseButton.Right;
                        eventArgs.ButtonState = MouseButtonState.Released;
                        break;
                    case WM_.MBUTTONDOWN:
                        eventArgs.EventType = MouseEventType.ButtonStateChanged;
                        eventArgs.Button = MouseButton.Middle;
                        eventArgs.ButtonState = MouseButtonState.Pressed;
                        break;
                    case WM_.MBUTTONUP:
                        eventArgs.EventType = MouseEventType.ButtonStateChanged;
                        eventArgs.Button = MouseButton.Middle;
                        eventArgs.ButtonState = MouseButtonState.Released;
                        break;
                    case WM_.XBUTTONDOWN:
                        eventArgs.EventType = MouseEventType.ButtonStateChanged;
                        eventArgs.Button = GetXButton(hookStruct.mouseData);
                        eventArgs.ButtonState = MouseButtonState.Pressed;
                        break;
                    case WM_.XBUTTONUP:
                        eventArgs.EventType = MouseEventType.ButtonStateChanged;
                        eventArgs.Button = GetXButton(hookStruct.mouseData);
                        eventArgs.ButtonState = MouseButtonState.Released;
                        break;
                    case WM_.MOUSEWHEEL:
                        eventArgs.EventType = MouseEventType.WheelRotated;
                        eventArgs.Wheel = MouseWheel.Vertical;
                        GetMouseWheelRotation(eventArgs, hookStruct.mouseData);
                        break;
                    case WM_.MOUSEHWHEEL:
                        eventArgs.EventType = MouseEventType.WheelRotated;
                        eventArgs.Wheel = MouseWheel.Horizontal;
                        GetMouseWheelRotation(eventArgs, hookStruct.mouseData);
                        break;
                    default:
                        return null;
                }
                return eventArgs;
            }
            private MouseButton GetXButton(uint mouseData)
            {
                switch ((XBUTTON)GetSignedHighOrderWord(mouseData))
                {
                    case XBUTTON._1:
                        return MouseButton.X1;
                    case XBUTTON._2:
                        return MouseButton.X2;
                    default:
                        return MouseButton.None;
                }
            }
            private void GetMouseWheelRotation(MouseEventArgs eventArgs, uint mouseData)
            {
                short rotationData = GetSignedHighOrderWord(mouseData);
                eventArgs.WheelRotationDirection = (rotationData < 0) ?
                    MouseWheelRotationDirection.Backward :
                    MouseWheelRotationDirection.Forward;
                eventArgs.WheelRotationLength = (short)(Math.Abs(rotationData) / (short)WHEEL_.DELTA);
            }
            private short GetSignedHighOrderWord(uint doubleWord) => (short)((doubleWord & 0xFFFF0000) >> 16);
        }

        private class MouseEventHandlerWrapper
        {
            [DllImport(WindowsAPILibraries.USER32_DLL,
                CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, UIntPtr wParam, IntPtr lParam);

            private MouseEventHandler wrappedDelegate;
            private LowLevelMouseProcProcessor lowLevelMouseProcProcessor;

            public IntPtr HookId { get; set; }

            public MouseEventHandlerWrapper(MouseEventHandler delegateToWrap,
                LowLevelMouseProcProcessor lowLevelMouseProcProcessor)
            {
                wrappedDelegate = delegateToWrap;
                this.lowLevelMouseProcProcessor = lowLevelMouseProcProcessor;
            }
            public IntPtr LowLevelMouseProc(int nCode, UIntPtr mouseMessageIdentifier, IntPtr mouseLowLevelHook)
            {
                if (nCode < 0)
                {
                    return CallNextHookEx(HookId, nCode, mouseMessageIdentifier, mouseLowLevelHook);
                }
                var mouseEventArgs = lowLevelMouseProcProcessor.Process(mouseMessageIdentifier, mouseLowLevelHook);
                if (mouseEventArgs == null)
                {
                    return CallNextHookEx(HookId, nCode, mouseMessageIdentifier, mouseLowLevelHook);
                }
                wrappedDelegate(mouseEventArgs);
                if (mouseEventArgs.Cancel)
                {
                    return new IntPtr(1);//true in C languages
                }
                else
                {
                    return CallNextHookEx(HookId, nCode, mouseMessageIdentifier, mouseLowLevelHook);
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
        private LowLevelMouseProcProcessor lowLevelMouseProcProcessor = new LowLevelMouseProcProcessor();
        private Dispatcher messageLoopDispatcher;

        public MouseEventHandlerRegister()
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
        public IntPtr RegisterHandler(MouseEventHandler handler)
        {
            try
            {
                return messageLoopDispatcher.Invoke(() => RegisterHook(new MouseEventHandlerWrapper(handler, lowLevelMouseProcProcessor)));
            }
            catch (TaskCanceledException)
            {
                return NULL;
            }
        }
        private IntPtr RegisterHook(MouseEventHandlerWrapper mouseEventHandlerWrapper)
        {
            IntPtr hookPtr;
            if ((hookPtr = SetWindowsHookEx(WH_.MOUSE_LL, 
                marshaler.GetPointer<LowLevelMouseProc>(mouseEventHandlerWrapper.LowLevelMouseProc),
                NULL, GLOBAL_SCOPE)) == NULL)
            {
                throw new SystemException($"Registration of {WH_.MOUSE_LL} was unsuccesful.");
            }
            mouseEventHandlerWrapper.HookId = hookPtr;
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
