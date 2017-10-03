using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace NetworkOperator.IO
{
    public class MouseEventSimulator
    {
        [DllImport(WindowsAPILibraries.USER32_DLL,
            CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] inputs, int inputSize);

        public static UIntPtr SIMULATED_MOUSE_EVENT_TAG = new UIntPtr(1);

        private readonly int INPUT_SIZE = Marshal.SizeOf<INPUT>();
        private Size screenSize;

        public MouseEventSimulator(Size screenSize)
        {
            this.screenSize = screenSize;
        }

        public void Simulate(MouseEventArgs[] multipleMouseEventArgs)
        {
            INPUT[] inputs = new INPUT[multipleMouseEventArgs.Length];
            int i = 0;
            foreach (var mouseEventArgs in multipleMouseEventArgs)
            {
                INPUT input = new INPUT()
                {
                    type = INPUT_.MOUSE,
                    inputData = new InputUnion()
                    {
                        mi = GetMouseInput(mouseEventArgs)
                    }
                };
                if (input.inputData.mi.dwFlags == MOUSEEVENTF_.None)
                {
                    continue;
                }
                inputs[i++] = input;
            }
            SendInput((uint)inputs.Length, inputs, INPUT_SIZE);
        }
        private MOUSEINPUT GetMouseInput(MouseEventArgs mouseEventArgs)
        {
            MOUSEINPUT mouseInput = new MOUSEINPUT()
            {
                dwFlags = GetMouseEventFlags(mouseEventArgs),
                time = 0, //let Windows add its own timestamp
                dwExtraInfo = SIMULATED_MOUSE_EVENT_TAG
            };
            if (mouseInput.dwFlags == MOUSEEVENTF_.XDOWN || mouseInput.dwFlags == MOUSEEVENTF_.XUP)
            {
                mouseInput.mouseData = GetXButtonData(mouseEventArgs);
            }
            if (mouseInput.dwFlags == MOUSEEVENTF_.WHEEL || mouseInput.dwFlags == MOUSEEVENTF_.HWHEEL)
            {
                mouseInput.mouseData = GetWheelRotationData(mouseEventArgs);
            }
            if (mouseEventArgs.EventType == MouseEventType.Movement)
            {
                Position normalizedPosition = GetNormalizedPosition(mouseEventArgs.Position);
                mouseInput.dx = normalizedPosition.X;
                mouseInput.dy = normalizedPosition.Y;
            }
            return mouseInput;
        }
        private MOUSEEVENTF_ GetMouseEventFlags(MouseEventArgs mouseEventArgs)
        {
            switch (mouseEventArgs.EventType)
            {
                case MouseEventType.Movement:
                    return MOUSEEVENTF_.MOVE | MOUSEEVENTF_.ABSOLUTE;
                case MouseEventType.ButtonStateChanged:
                    switch (mouseEventArgs.Button)
                    {
                        case MouseButton.Left:
                            switch (mouseEventArgs.ButtonState)
                            {
                                case MouseButtonState.Pressed:
                                    return MOUSEEVENTF_.LEFTDOWN;
                                case MouseButtonState.Released:
                                    return MOUSEEVENTF_.LEFTUP;
                                case MouseButtonState.None:
                                default:
                                    return MOUSEEVENTF_.None;
                            }
                        case MouseButton.Right:
                            switch (mouseEventArgs.ButtonState)
                            {
                                case MouseButtonState.Pressed:
                                    return MOUSEEVENTF_.RIGHTDOWN;
                                case MouseButtonState.Released:
                                    return MOUSEEVENTF_.RIGHTUP;
                                case MouseButtonState.None:
                                default:
                                    return MOUSEEVENTF_.None;
                            }
                        case MouseButton.Middle:
                            switch (mouseEventArgs.ButtonState)
                            {
                                case MouseButtonState.Pressed:
                                    return MOUSEEVENTF_.MIDDLEDOWN;
                                case MouseButtonState.Released:
                                    return MOUSEEVENTF_.MIDDLEUP;
                                case MouseButtonState.None:
                                default:
                                    return MOUSEEVENTF_.None;
                            }
                        case MouseButton.X1:
                        case MouseButton.X2:
                            switch (mouseEventArgs.ButtonState)
                            {
                                case MouseButtonState.Pressed:
                                    return MOUSEEVENTF_.XDOWN;
                                case MouseButtonState.Released:
                                    return MOUSEEVENTF_.XUP;
                                case MouseButtonState.None:
                                default:
                                    return MOUSEEVENTF_.None;
                            }
                        case MouseButton.None:
                        default:
                            return MOUSEEVENTF_.None;
                    }
                case MouseEventType.WheelRotated:
                    switch (mouseEventArgs.Wheel)
                    {
                        case MouseWheel.Vertical:
                            return MOUSEEVENTF_.WHEEL;
                        case MouseWheel.Horizontal:
                            return MOUSEEVENTF_.HWHEEL;
                        case MouseWheel.None:
                        default:
                            return MOUSEEVENTF_.None;
                    }
                default:
                    return MOUSEEVENTF_.None;
            }
        }
        private uint GetXButtonData(MouseEventArgs mouseEventArgs)
        {
            switch (mouseEventArgs.Button)
            {
                case MouseButton.X1:
                    return ToSignedDword((short)XBUTTON._1);
                case MouseButton.X2:
                    return ToSignedDword((short)XBUTTON._2);
                default:
                    return 0;
            }
        }
        private uint GetWheelRotationData(MouseEventArgs mouseEventArgs)
        {
            short wheelRotationInfo = 0;
            switch (mouseEventArgs.WheelRotationDirection)
            {
                case MouseWheelRotationDirection.Forward:
                    wheelRotationInfo = 1;
                    break;
                case MouseWheelRotationDirection.Backward:
                    wheelRotationInfo = -1;
                    break;
                case MouseWheelRotationDirection.None:
                default:
                    return 0;
            }
            wheelRotationInfo *= (short)(mouseEventArgs.WheelRotationLength * (short)WHEEL_.DELTA);
            return ToSignedDword(wheelRotationInfo);
        }
        private uint ToSignedDword(short signedWord) => (uint)((0xFFFFFFFF & signedWord));
        private Position GetNormalizedPosition(Position position)
        {
            if (position.X < 0)
            {
                position.X = 0;
            }
            if (position.Y < 0)
            {
                position.Y = 0;
            }
            position.X = (int)(position.X * (0xFFFF / (double)screenSize.Width) + 1);
            position.Y = (int)(position.Y * (0xFFFF / (double)screenSize.Height) + 1);
            return position;
        }
    }
}
