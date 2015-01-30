using Alarmy.Common;
using System;

namespace Alarmy.Core.FileAlarmRepository
{
    internal class FreshnessRepositoryFilter : IRepositoryFilter
    {
        private readonly int freshnessInMinutes;
        private readonly IDateTimeProvider dateTimeProvider;

        public FreshnessRepositoryFilter(IDateTimeProvider dateTimeProvider, int freshnessInMinutes)
        {
            if (dateTimeProvider == null)
                throw new ArgumentNullException("dateTimeProvider");

            this.dateTimeProvider = dateTimeProvider;
            this.freshnessInMinutes = freshnessInMinutes;
        }

        public bool Match(IAlarm alarm)
        {
            if (alarm == null)
                throw new ArgumentNullException("alarm");

            return !((alarm.Status == AlarmStatus.Canceled || alarm.Status == AlarmStatus.Completed) && alarm.Time < GetTime().AddMinutes(-this.freshnessInMinutes));
        }

        private DateTime GetTime()
        {
            return this.dateTimeProvider.Now.RoundToMinute();
        }
    }
}
