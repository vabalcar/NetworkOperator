using System;
using System.Runtime.InteropServices;

namespace NetworkOperator.IO
{
    internal enum WH_ : int//int in C++
    {
        CALLWNDPROC = 4,
        CALLWNDPROCRET = 12,
        CBT = 5,
        DEBUG = 9,
        FOREGROUNDIDLE = 11,
        GETMESSAGE = 3,
        JOURNALPLAYBACK = 1,
        JOURNALRECORD = 0,
        KEYBOARD = 2,
        KEYBOARD_LL = 13,
        MOUSE = 7,
        MOUSE_LL = 14,
        MSGFILTER = -1,
        SHELL = 10,
        SYSMSGFILTER = 6
    }

    internal enum WM_ : uint//WPARAM in C++
    {
        MOUSEMOVE = 0x0200,
        LBUTTONDOWN = 0x0201,
        LBUTTONUP = 0x0202,
        RBUTTONDOWN = 0x0204,
        RBUTTONUP = 0x0205,
        MBUTTONDOWN = 0x0207,
        MBUTTONUP = 0x0208,
        MOUSEWHEEL = 0x020A,
        XBUTTONDOWN = 0x020B,
        XBUTTONUP = 0x020C,
        MOUSEHWHEEL = 0x020E
    }

    internal enum XBUTTON : ushort//WORD in C++
    {
        _1 = 0x0001,
        _2 = 0x0002
    }

    internal enum WHEEL_ : byte//not specified in C++
    {
        DELTA = 120
    }

    internal enum INPUT_ : uint//DWORD in C++
    {
        MOUSE = 0,
        KEYBOARD = 1,
        HARDWARE = 2
    }

    [Flags]
    internal enum MOUSEEVENTF_ : uint//DWORD in C++
    {
        ABSOLUTE = 0x8000,
        HWHEEL = 0x01000,
        MOVE = 0x0001,
        MOVE_NOCOALESCE = 0x2000,
        LEFTDOWN = 0x0002,
        LEFTUP = 0x0004,
        RIGHTDOWN = 0x0008,
        RIGHTUP = 0x0010,
        MIDDLEDOWN = 0x0020,
        MIDDLEUP = 0x0040,
        VIRTUALDESK = 0x4000,
        WHEEL = 0x0800,
        XDOWN = 0x0080,
        XUP = 0x0100,
        None = 0 //my own abstract fake event, it is equivalent to parse error
    }

    internal static class WindowsAPILibraries
    {
        public const string USER32_DLL = "user32.dll";
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MSLLHOOKSTRUCT
    {
        public POINT point;
        public uint mouseData;
        public WM_ flags;
        public uint time;
        public UIntPtr extraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT
    {
        public INPUT_ type;
        public InputUnion inputData;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;
        [FieldOffset(0)]
        public KEYBDINPUT ki;
        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public MOUSEEVENTF_ dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }
}
