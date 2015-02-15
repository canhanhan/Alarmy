using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Core
{
    internal abstract class AlarmReminderManager : IAlarmReminderManager
    {
        protected readonly IAlarmService alarmService;
        protected readonly ITimer timer;

        public event EventHandler<AlarmReminderEventArgs> OnRequestNotification;

        public AlarmReminderManager(IAlarmService alarmService, ITimer reminderTimer)
        {
            if (alarmService == null)
                throw new ArgumentNullException("alarmService");

            if (reminderTimer == null)
                throw new ArgumentNullException("reminderTimer");

            this.alarmService = alarmService;
            this.timer = reminderTimer;
            reminderTimer.Elapsed += timer_Elapsed;
        }

        public void Start()
        {
            this.timer.Start();
        }

        public void Stop()
        {
            this.timer.Stop();
        }

        protected abstract string PrepareMessage();

        private void timer_Elapsed(object source, EventArgs args)
        {
            var message = this.PrepareMessage();
            if (!string.IsNullOrEmpty(message))
                this.OnRequestNotification.Invoke(e: new AlarmReminderEventArgs("Alarm Reminder", message));
        }

        protected static string PrepareMessage(string task, IEnumerable<IAlarm> alarms)
        {
            return "\r\n" + string.Join("\r\n", alarms.Select(x => string.Format("{0}: {1} - {2}", task, x.Time, x.Title)));
        }
    }
}
