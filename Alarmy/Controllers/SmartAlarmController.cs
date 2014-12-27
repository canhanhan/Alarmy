using Alarmy.Common;
using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace Alarmy.Controllers
{
    internal class SmartAlarmController : ISmartAlarmController
    {
        public event EventHandler OnSoundOff;
        public event EventHandler OnSoundOn;
        public event EventHandler OnSleep;
        public event EventHandler OnWakeup;

        public SmartAlarmController()
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        public bool IsActive { get; private set; }
        public bool IsSilent { get; private set; }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.ConsoleDisconnect:
                case SessionSwitchReason.ConsoleConnect:
                    CheckIfCurrentSessionActive();
                    break;
                case SessionSwitchReason.SessionLock:
                    Mute();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    Unmute();
                    break;
            }
        }

        private void Unmute()
        {
            IsSilent = false;

            if (OnSoundOn != null)
            {
                OnSoundOn.Invoke(this, null);
            }
        }

        private void Mute()
        {
            IsSilent = true;

            if (OnSoundOff != null)
            {
                OnSoundOff.Invoke(this, null);
            }
        }

        private void CheckIfCurrentSessionActive()
        {
            var currentSession = NativeMethods.WTSGetActiveConsoleSessionId();
            if (currentSession == Process.GetCurrentProcess().SessionId)
            {
                IsActive = true;

                if (OnWakeup != null)
                {
                    OnWakeup.Invoke(this, null);
                }
            }
            else
            {
                IsActive = false;

                if (OnSleep != null)
                {
                    OnSleep.Invoke(this, null);
                }
            }
        }
    }
}
