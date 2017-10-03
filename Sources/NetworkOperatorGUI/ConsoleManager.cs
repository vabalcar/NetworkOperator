using System.Runtime.InteropServices;

namespace NetworkOperator.UserInterfaces
{
    static class ConsoleManager
    {
        private const string KERNEL_DLL = "Kernel32.dll";
        [DllImport(KERNEL_DLL, EntryPoint = "AllocConsole", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenConsole();

        [DllImport(KERNEL_DLL, EntryPoint = "FreeConsole", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseConsole();
    }
}
