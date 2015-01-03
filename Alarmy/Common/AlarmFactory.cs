namespace Alarmy.Common
{
    internal class AlarmFactory : Alarmy.Common.IAlarmFactory
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
