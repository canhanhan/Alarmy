using Alarmy.Common;

namespace Alarmy.Core
{
    internal class AlarmFactory : IAlarmFactory
    {
        private readonly IDateTimeProvider dateTimeProvider;

        public AlarmFactory(IDateTimeProvider dateTimeProvider)
        {
            this.dateTimeProvider = dateTimeProvider;
        }

        public IAlarm Create()
        {
            return new Alarm(this.dateTimeProvider);
        }
    }
}
