using Alarmy.Common;
using System.Linq;

namespace Alarmy.Core
{
    internal class MissedAlarmReminderManager : AlarmReminderManager
    {
        public MissedAlarmReminderManager(IAlarmService alarmService, ITimer reminderTimer) : base(alarmService, reminderTimer) {}

        protected override string PrepareMessage()
        {
            var missedAlarms = this.alarmService.List().Where(x => x.Status == AlarmStatus.Missed).ToArray();
            if (missedAlarms.Length > 0)                
            {
                return "Following alarms are missed. Complete or cancel following alarms:" + PrepareMessage("Missed", missedAlarms);
            }

            return null;
        }
    }
}
