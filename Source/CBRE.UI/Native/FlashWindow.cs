using System;
using System.Runtime.InteropServices;

namespace CBRE.UI.Native
{
    public class FlashWindow
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [Flags]
        public enum FLASHFLAGS : uint
        {
            FLASHW_STOP = 0x00000000,
            FLASHW_CAPTION = 0x00000001,
            FLASHW_TRAY = 0x00000002,
            FLASHW_ALL = 0x00000003,
            FLASHW_TIMER = 0x00000004,
            FLASHW_TIMERNOFG = 0x0000000C
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public FLASHFLAGS dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }
    }
}
