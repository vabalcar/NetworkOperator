using System;
using System.Runtime.InteropServices;

namespace NetworkOperator.IO
{
    class KeyboardEventSimulator
    {
        [DllImport(WindowsAPILibraries.USER32_DLL,
            CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] inputs, int inputSize);

        public static UIntPtr SIMULATED_KEYBOARD_EVENT_TAG = new UIntPtr(1);

        private readonly int INPUT_SIZE = Marshal.SizeOf<INPUT>();

        public void Simulate(KeyboardEventArgs[] multipleKeyboardEventArgs)
        {
            INPUT[] inputs = new INPUT[multipleKeyboardEventArgs.Length];
            int i = 0;
            foreach (var keyboardEventArgs in multipleKeyboardEventArgs)
            {
                INPUT input = new INPUT()
                {
                    type = INPUT_.KEYBOARD,
                    inputData = new InputUnion()
                    {
                        ki = GetKeyboardInput(keyboardEventArgs)
                    }
                };
                inputs[i++] = input;
            }
            SendInput((uint)inputs.Length, inputs, INPUT_SIZE);
        }
        private KEYBDINPUT GetKeyboardInput(KeyboardEventArgs keyboardEventArgs)
        {
            KEYBDINPUT input = new KEYBDINPUT()
            {
                time = 0,
                dwExtraInfo = SIMULATED_KEYBOARD_EVENT_TAG,
                wVk = (ushort)keyboardEventArgs.Key.VirtualKey
            };
            if (keyboardEventArgs.KeyStatus == KeyStatus.Released)
            {
                input.dwFlags = KEYEVENTF_.KEYUP;
            }
            return input;
        }
    }
}
