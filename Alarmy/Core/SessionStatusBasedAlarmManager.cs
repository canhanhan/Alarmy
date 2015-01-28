using Alarmy.Common;
using Alarmy.Infrastructure;
using System;

namespace Alarmy.Core
{
    internal class SessionStateBasedAlarmManager : IAlarmManager
    {
        private readonly SessionStateProvider sessionStateProvider;

        public event EventHandler OnSoundOff;
        public event EventHandler OnSoundOn;
        public event EventHandler OnSleep;
        public event EventHandler OnWakeup;
                
        public SessionStateBasedAlarmManager(SessionStateProvider sessionInformation)
        {
            if (sessionInformation == null)
                throw new ArgumentNullException("sessionInformation");

            this.sessionStateProvider = sessionInformation;
            this.sessionStateProvider.SessionLocked += sessionInformation_SessionLocked;
            this.sessionStateProvider.SessionUnlocked += sessionInformation_SessionUnlocked;
            this.sessionStateProvider.SessionActivated += sessionInformation_SessionActivated;
            this.sessionStateProvider.SessionDeactivated += sessionInformation_SessionDeactivated;
        }

        private void sessionInformation_SessionDeactivated(object sender, EventArgs e)
        {
            if (OnSleep != null)
            {
                OnSleep.Invoke(this, null);
            }
        }

        private void sessionInformation_SessionActivated(object sender, EventArgs e)
        {
            if (OnWakeup != null)
            {
                OnWakeup.Invoke(this, null);
            }
        }

        private void sessionInformation_SessionUnlocked(object sender, EventArgs e)
        {
            if (OnSoundOn != null)
            {
                OnSoundOn.Invoke(this, null);
            }
        }

        private void sessionInformation_SessionLocked(object sender, EventArgs e)
        {
            if (OnSoundOff != null)
            {
                OnSoundOff.Invoke(this, null);
            }
        }
    }
}
