using System;
using System.Runtime.InteropServices;

namespace Alarmy
{
    internal class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        public const int HWND_BROADCAST = 0xffff;       
        
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

        [DllImport("user32")]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        
        [DllImport("user32", CharSet = CharSet.Unicode)]
        public static extern int RegisterWindowMessage(string message);

        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall, PreserveSig=false)]
        public static extern void SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

        [DllImport("USER32")]
        public static extern int GetSystemMetrics(int Index);

        [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, [MarshalAs(UnmanagedType.Bool)]bool repaint);
    }
}
