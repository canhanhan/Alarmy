using Alarmy.Common;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Alarmy.Infrastructure
{
    internal class SessionStateProvider : ISessionStateProvider
    {
        private class NativeMethods
        {
            public const int WTS_CURRENT_SESSION = -1;

            public enum WTS_INFO_CLASS
            {
                WTSInitialProgram = 0,
                WTSApplicationName = 1,
                WTSWorkingDirectory = 2,
                WTSOEMId = 3,
                WTSSessionId = 4,
                WTSUserName = 5,
                WTSWinStationName = 6,
                WTSDomainName = 7,
                WTSConnectState = 8,
                WTSClientBuildNumber = 9,
                WTSClientName = 10,
                WTSClientDirectory = 11,
                WTSClientProductId = 12,
                WTSClientHardwareId = 13,
                WTSClientAddress = 14,
                WTSClientDisplay = 15,
                WTSClientProtocolType = 16,
                WTSIdleTime = 17,
                WTSLogonTime = 18,
                WTSIncomingBytes = 19,
                WTSOutgoingBytes = 20,
                WTSIncomingFrames = 21,
                WTSOutgoingFrames = 22,
                WTSClientInfo = 23,
                WTSSessionInfo = 24,
                WTSSessionInfoEx = 25,
                WTSConfigInfo = 26,
                WTSValidationInfo = 27,
                WTSSessionAddressV4 = 28,
                WTSIsRemoteSession = 29
            }

            public enum WTS_CONNECTSTATE_CLASS
            {
                WTSActive,
                WTSConnected,
                WTSConnectQuery,
                WTSShadow,
                WTSDisconnected,
                WTSIdle,
                WTSListen,
                WTSReset,
                WTSDown,
                WTSInit
            }

            [DllImport("kernel32.dll")]
            public static extern uint WTSGetActiveConsoleSessionId();

            [DllImport("Wtsapi32.dll")]
            [return:MarshalAs(UnmanagedType.Bool)]
            public static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out uint pBytesReturned);

            [DllImport("Wtsapi32.dll")]
            public static extern void WTSFreeMemory(IntPtr pointer);

            private NativeMethods()
            {
            }
        }

        private readonly int sessionId;
        private bool isActive;
        private bool isLocked;

        public event EventHandler SessionLocked;
        public event EventHandler SessionUnlocked;
        public event EventHandler SessionActivated;
        public event EventHandler SessionDeactivated;

        public SessionStateProvider()
        {
            this.sessionId = Process.GetCurrentProcess().SessionId;
            this.isActive = true;
            this.isLocked = false;

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.ConsoleDisconnect:
                case SessionSwitchReason.RemoteDisconnect:                
                case SessionSwitchReason.ConsoleConnect:
                case SessionSwitchReason.RemoteConnect:
                    this.CheckIfCurrentSessionActive();
                    break;
                case SessionSwitchReason.SessionLock:
                    this.Locked();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    this.Unlocked();
                    break;
            }
        }

        private void Unlocked()
        {
            if (this.isLocked) {
                this.isLocked = false;
                if (this.SessionUnlocked != null)
                    this.SessionUnlocked.Invoke(null, null);
           }
        }

        private void Locked()
        {
            if (!this.isLocked)
            {
                this.isLocked = true;
                if (this.SessionLocked != null)
                    this.SessionLocked.Invoke(null, null);
            }
        }

        private void CheckIfCurrentSessionActive()
        {
            var state = this.IsActiveLocalSession() || this.IsActiveRemoteSession();

            if (state && !isActive)
            {
                this.isActive = true;
                if (this.SessionActivated != null)
                    this.SessionActivated.Invoke(null, null);
            }
            else if (!state && isActive)
            {
                this.isActive = false;
                if (this.SessionDeactivated != null)
                    this.SessionDeactivated.Invoke(null, null);
            }
        }

        private bool IsActiveLocalSession()
        {
            return this.sessionId == NativeMethods.WTSGetActiveConsoleSessionId();
        }

        private bool IsActiveRemoteSession()
        {
            IntPtr buffer;
            uint length;
            if (NativeMethods.WTSQuerySessionInformation(IntPtr.Zero, this.sessionId, NativeMethods.WTS_INFO_CLASS.WTSConnectState, out buffer, out length) && length > 0)
            {
                var state = (NativeMethods.WTS_CONNECTSTATE_CLASS)Marshal.ReadIntPtr(buffer);             
                NativeMethods.WTSFreeMemory(buffer);

                return state == NativeMethods.WTS_CONNECTSTATE_CLASS.WTSActive;
            }

            return false;
        }
    }
}
