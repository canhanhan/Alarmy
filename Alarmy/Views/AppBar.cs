using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Alarmy.Views
{
    // Modified version of: http://www.codeproject.com/Articles/6741/AppBar-using-C
    internal class AppBar : Form
    {
        private class NativeMethods
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

            [DllImport("user32", CharSet = CharSet.Unicode)]
            public static extern int RegisterWindowMessage(string message);

            [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall, PreserveSig = false)]
            public static extern void SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

            [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, [MarshalAs(UnmanagedType.Bool)]bool repaint);

            private NativeMethods() { }
        }

        private enum ABEdge : int
        {
            ABE_LEFT = 0,
            ABE_TOP,
            ABE_RIGHT,
            ABE_BOTTOM
        }

        private enum ABMsg : int
        {
            ABM_ACTIVATE = 6,
            ABM_GETAUTOHIDEBAR = 7,
            ABM_GETSTATE = 4,
            ABM_GETTASKBARPOS = 5,
            ABM_NEW = 0,
            ABM_QUERYPOS = 2,
            ABM_REMOVE = 1,
            ABM_SETAUTOHIDEBAR = 8,
            ABM_SETPOS = 3,
            ABM_SETSTATE = 10,
            ABM_WINDOWPOSCHANGED = 9
        }

        private enum ABNotify : int
        {
            ABN_STATECHANGE = 0,
            ABN_POSCHANGED,
            ABN_FULLSCREENAPP,
            ABN_WINDOWARRANGE
        }

        private readonly int callBackId;        

        protected bool IsRegistering { get; private set; }
        protected bool IsRegistered { get; private set; }
        protected bool IsRepositioning { get; private set; }

        public AppBar()
        {
            this.callBackId = NativeMethods.RegisterWindowMessage(Guid.NewGuid().ToString());
        }

        private void RegisterOrUnregisterBar(bool register)
        {
            if (this.IsRegistering)
                return;

            try
            {
                this.IsRegistering = true;

                var abd = new NativeMethods.APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = Handle;

                if (register == this.IsRegistered)
                {
                    return;
                }
                else if (register)
                {
                    abd.uCallbackMessage = this.callBackId;
                    NativeMethods.SHAppBarMessage((int)ABMsg.ABM_NEW, ref abd);
                    this.IsRegistered = true;

                    this.Reposition();
                }
                else
                {
                    NativeMethods.SHAppBarMessage((int)ABMsg.ABM_REMOVE, ref abd);
                    this.IsRegistered = false;
                }
            }
            finally
            {
                this.IsRegistering = false;
            }            
        }

        protected void RegisterBar()
        {
            this.RegisterOrUnregisterBar(register: true);
        }

        protected void UnRegisterBar()
        {
            this.RegisterOrUnregisterBar(register: false);
        }

        protected void Reposition(int width=0)
        {
            if (this.IsRepositioning || !this.IsRegistered)
                return;

            try
            {
                this.IsRepositioning = true;

                if (width == 0)
                    width = this.Width;

                var workingArea = Screen.FromControl(this).Bounds;
                var edge = (this.Location.X < workingArea.Width / 2 ? ABEdge.ABE_LEFT : ABEdge.ABE_RIGHT);
                var abd = new NativeMethods.APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = Handle;
                abd.uEdge = (int)edge;
                abd.rc.top = workingArea.Top;
                abd.rc.bottom = workingArea.Height;

                if (edge == ABEdge.ABE_LEFT)
                {
                    abd.rc.left = workingArea.Left;
                    abd.rc.right = workingArea.Left + width;
                }
                else
                {
                    abd.rc.right = workingArea.Right;
                    abd.rc.left = workingArea.Right - width;
                }

                NativeMethods.SHAppBarMessage((int)ABMsg.ABM_QUERYPOS, ref abd);
                if (edge == ABEdge.ABE_LEFT)
                {
                    abd.rc.right = abd.rc.left + width;
                }
                NativeMethods.SHAppBarMessage((int)ABMsg.ABM_SETPOS, ref abd);
                this.Bounds = new Rectangle(abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top);
            }
            finally
            {
                this.IsRepositioning = false;
            }            
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == callBackId)
            {
                switch (m.WParam.ToInt32())                
                {                    
                    case (int)ABNotify.ABN_POSCHANGED:
                        this.Reposition();
                        break;
                }
            }

            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style &= (~0x00C00000);
                cp.Style &= (~0x00800000);
                cp.ExStyle = 0x00000080;
                return cp;
            }
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            if (this.IsRepositioning || this.IsRegistering || !this.Visible)
                return;

            var workingArea = Screen.FromControl(this).WorkingArea;
            if (workingArea.Right <= Location.X + Width || workingArea.Left >= Location.X)
            {
                if (!this.IsRegistered)
                {
                    this.RegisterBar();
                }
                else
                {
                    this.Reposition();
                }
            }
            else if (this.IsRegistered)
            {
                this.UnRegisterBar();
            }
        }
    }
}
