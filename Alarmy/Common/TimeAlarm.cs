using System;

namespace Alarmy.Common
{
    public class TimeAlarm : Alarm
    {
        public const int SNOOZE_INTERVAL = 5;
        private readonly IDateTimeProvider _DateTimeProvider;
        
        public DateTime Time { get; private set; }

        public override bool IsDelayable { get { return true; } }

        public TimeAlarm(IDateTimeProvider dateTimeProvider)
        {
            this._DateTimeProvider = dateTimeProvider;
        }

        public void Set(DateTime time)
        {
            this.Time = time;
            this.SetStatus(AlarmStatus.Set);
        }

        public void Snooze()
        {
            var newTime = this.Time.AddMinutes(SNOOZE_INTERVAL);
            this.Set(newTime);
        }

        public override void Check()
        {
            if (this.Status != AlarmStatus.Set && this.Status != AlarmStatus.Ringing)
                return;

            var time = this._DateTimeProvider.NowRoundedToCurrentMinute;
            if (this.Status != AlarmStatus.Ringing && this.Time >= time && this.Time < time.AddMinutes(1))
            {
                this.SetStatus(AlarmStatus.Ringing);
            }
            else if (this.Time > time.AddMinutes(1))
            {
                this.SetStatus(AlarmStatus.Missed);
            } 
        }
    }
}
