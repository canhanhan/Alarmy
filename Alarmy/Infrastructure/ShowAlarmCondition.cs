using Alarmy.Common;
using System;

namespace Alarmy.Infrastructure
{
    internal class ShowAlarmFreshnessCondition : IShowAlarmCondition
    {
        private readonly int freshness;
        private readonly IDateTimeProvider dateTimeProvider;

        public ShowAlarmFreshnessCondition(IDateTimeProvider dateTimeProvider, Settings settings)
        {
            this.dateTimeProvider = dateTimeProvider;
            this.freshness = settings.Freshness;
        }

        public bool Match(IAlarm alarm)
        {
            if (alarm == null)
                throw new ArgumentNullException("alarm");

            return !((alarm.Status == AlarmStatus.Canceled || alarm.Status == AlarmStatus.Completed) && alarm.Time < GetTime().AddMinutes(-this.freshness));
        }

        private DateTime GetTime()
        {
            return this.dateTimeProvider.Now.RoundToMinute();
        }
    }
}
