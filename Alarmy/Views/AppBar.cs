﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Alarmy.Views
{
    internal class AppBar : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
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

        private enum ABEdge : int
        {
            ABE_LEFT = 0,
            ABE_TOP,
            ABE_RIGHT,
            ABE_BOTTOM
        }

        private bool fBarRegistered = false;
        private bool fBarIsRegistering = false;

        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        private static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);
        [DllImport("USER32")]
        private static extern int GetSystemMetrics(int Index);
        [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern int RegisterWindowMessage(string msg);
        private int uCallBack;
        private bool fBarIsRepositioning;

        protected bool IsRegistering
        {
            get
            {
                return fBarIsRegistering;
            }
        }
        protected bool IsRegistered
        {
            get
            {
                return fBarRegistered;
            }
        }

        protected void RegisterBar()
        {
            if (fBarIsRegistering)
            {
                return;
            }
            fBarIsRegistering = true;

            var abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = Handle;
            if (!fBarRegistered)
            {
                uCallBack = RegisterWindowMessage("AppBarMessage");
                abd.uCallbackMessage = uCallBack;

                SHAppBarMessage((int)ABMsg.ABM_NEW, ref abd);
                fBarRegistered = true;

                ABSetPos();
            }
            else
            {
                SHAppBarMessage((int)ABMsg.ABM_REMOVE, ref abd);
                fBarRegistered = false;
            }

            fBarIsRegistering = false;
        }

        private void ABSetPos()
        {
            if (fBarIsRepositioning)
            {
                return;
            }
            fBarIsRepositioning = true;

            var abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = Handle;
            abd.uEdge = (int)(Location.X < Screen.GetWorkingArea(this).Width / 2 ? ABEdge.ABE_LEFT : ABEdge.ABE_RIGHT);

            if (abd.uEdge == (int)ABEdge.ABE_LEFT || abd.uEdge == (int)ABEdge.ABE_RIGHT)
            {
                abd.rc.top = 0;
                abd.rc.bottom = SystemInformation.PrimaryMonitorSize.Height;
                if (abd.uEdge == (int)ABEdge.ABE_LEFT)
                {
                    abd.rc.left = 0;
                    abd.rc.right = Size.Width;
                }
                else
                {
                    abd.rc.right = SystemInformation.PrimaryMonitorSize.Width;
                    abd.rc.left = abd.rc.right - Size.Width;
                }
            }
            else
            {
                abd.rc.left = 0;
                abd.rc.right = SystemInformation.PrimaryMonitorSize.Width;
                if (abd.uEdge == (int)ABEdge.ABE_TOP)
                {
                    abd.rc.top = 0;
                    abd.rc.bottom = Size.Height;
                }
                else
                {
                    abd.rc.bottom = SystemInformation.PrimaryMonitorSize.Height;
                    abd.rc.top = abd.rc.bottom - Size.Height;
                }
            }


            SHAppBarMessage((int)ABMsg.ABM_QUERYPOS, ref abd);



            switch (abd.uEdge)
            {
                case (int)ABEdge.ABE_LEFT:
                    abd.rc.right = abd.rc.left + Size.Width;
                    break;
                case (int)ABEdge.ABE_RIGHT:
                    abd.rc.left = abd.rc.right - Size.Width;
                    break;
                case (int)ABEdge.ABE_TOP:
                    abd.rc.bottom = abd.rc.top + Size.Height;
                    break;
                case (int)ABEdge.ABE_BOTTOM:
                    abd.rc.top = abd.rc.bottom - Size.Height;
                    break;
            }


            SHAppBarMessage((int)ABMsg.ABM_SETPOS, ref abd);



            MoveWindow(abd.hWnd, abd.rc.left, abd.rc.top,
                abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, true);

            fBarIsRepositioning = false;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == uCallBack)
            {
                switch (m.WParam.ToInt32())
                {
                    case (int)ABNotify.ABN_POSCHANGED:
                        ABSetPos();
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

            if (fBarIsRegistering)
            {
                return;
            }
            var workingArea = Screen.FromControl(this).WorkingArea;
            if (workingArea.Right <= Location.X + Width || workingArea.Left >= Location.X)
            {
                if (!fBarRegistered)
                {
                    Console.WriteLine("Register");
                    RegisterBar();
                }
                else
                {
                    Console.WriteLine("Repos");
                    ABSetPos();
                }
            }
            else
            {
                if (fBarRegistered)
                {
                    Console.WriteLine("Unregister");
                    RegisterBar();
                }
            }
        }
    }
}