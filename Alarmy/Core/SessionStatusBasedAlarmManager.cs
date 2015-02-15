using Alarmy.Common;
using Alarmy.Infrastructure;
using System;
using System.Diagnostics;

namespace Alarmy.Core
{
    internal class SessionStateBasedAlarmManager : IAlarmManager
    {
        private readonly ISessionStateProvider sessionStateProvider;

        public event EventHandler OnSleep;
        public event EventHandler OnWakeup;

        public SessionStateBasedAlarmManager(ISessionStateProvider sessionInformation)
        {
            if (sessionInformation == null)
                throw new ArgumentNullException("sessionInformation");

            this.sessionStateProvider = sessionInformation;
            this.sessionStateProvider.SessionLocked += sessionInformation_SessionDeactivated;
            this.sessionStateProvider.SessionUnlocked += sessionInformation_SessionActivated;
            this.sessionStateProvider.SessionActivated += sessionInformation_SessionActivated;
            this.sessionStateProvider.SessionDeactivated += sessionInformation_SessionDeactivated;
        }

        private void sessionInformation_SessionDeactivated(object sender, EventArgs e)
        {
            Trace.WriteLine("sessionInformation_SessionDeactivated");

            if (OnSleep != null)
            {
                OnSleep.Invoke(this, null);
            }
        }

        private void sessionInformation_SessionActivated(object sender, EventArgs e)
        {
            Trace.WriteLine("sessionInformation_SessionActivated");

            if (OnWakeup != null)
            {
                OnWakeup.Invoke(this, null);
            }
        }
    }
}
