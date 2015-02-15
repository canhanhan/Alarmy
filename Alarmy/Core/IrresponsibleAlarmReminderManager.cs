using Alarmy.Common;
using System;
using System.Linq;

namespace Alarmy.Core
{
    internal class IrresponsibleAlarmReminderManager : MissedAlarmReminderManager
    {
        public IrresponsibleAlarmReminderManager(IAlarmService alarmService, ITimer reminderTimer) : base(alarmService, reminderTimer) {}

        protected override string PrepareMessage()
        {
            var message = base.PrepareMessage();
            var freshlyCompletedAlarms = this.alarmService.List().Where(
                x => x.Status == AlarmStatus.Completed && (
                    x.Time > DateTime.Now.AddMilliseconds(-(this.timer.Interval * 2))
                    && x.Time <= DateTime.Now.AddMilliseconds(-this.timer.Interval)
                )).ToArray();

            if (freshlyCompletedAlarms.Length > 0)
            {
                if (!string.IsNullOrEmpty(message))
                    message += "\r\n\r\n";

                message += "Following alarms are completed recently. Verify if these are really completed:" + PrepareMessage("Verify", freshlyCompletedAlarms);
            }

            return message;
        }
    }
}
