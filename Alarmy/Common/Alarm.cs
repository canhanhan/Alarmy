using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alarmy.Common
{
    public class Alarm : IAlarm
    {
        public const int SNOOZE_INTERVAL = 5;
        private readonly IDateTimeProvider _DateTimeProvider;

        public string Title { get; set; }

        public AlarmStatus Status { get; set; }
        
        public string CancelReason { get; set; }

        public DateTime Time { get; set; }

        public Alarm() { }

        public Alarm(IDateTimeProvider dateTimeProvider)
        {
            this._DateTimeProvider = dateTimeProvider;                
        }

        public void Set(DateTime time)
        {
            this.Time = time;
            this.SetStatus(AlarmStatus.Set);
        }

#if DEBUG
        public void SetStatusTest(AlarmStatus status)
        {
            this.Status = status;
        }
#endif

        public virtual void Cancel(string reason)
        {
            this.CancelReason = reason;
            this.SetStatus(AlarmStatus.Cancelled);
        }

        public virtual void Complete()
        {
            this.Status = AlarmStatus.Completed;
        }

        public bool CanBeCancelled
        {
            get
            {
                return this.Status == AlarmStatus.Set || this.Status == AlarmStatus.Missed || this.Status == AlarmStatus.Ringing;
            }
        }

        public bool CanBeCompleted
        {
            get
            {
                return this.Status == AlarmStatus.Ringing || this.Status == AlarmStatus.Missed;
            }
        }

        public bool CanBeRinging
        {
            get
            {
                return this.Status == AlarmStatus.Set;
            }
        }

        public bool CanBeSet
        {
            get
            {
                return true;
            }
        }

        public bool CanBeMissed
        {
            get
            {
                return this.Status == AlarmStatus.Ringing || this.Status == AlarmStatus.Set;
            }
        }

        public void Check()
        {
            if (this.Status != AlarmStatus.Set && this.Status != AlarmStatus.Ringing)
                return;

            var time = (this._DateTimeProvider == null ? DateTime.Now : this._DateTimeProvider.Now).RoundToMinute();
            if (this.Status != AlarmStatus.Ringing && this.Time >= time && this.Time < time.AddMinutes(1))
            {
                this.SetStatus(AlarmStatus.Ringing);
            }
            else if (this.Time < time)
            {
                this.SetStatus(AlarmStatus.Missed);
            }
        }

        protected void SetStatus(AlarmStatus status)
        {
            if (status == this.Status)
                return;

            switch (status)
            {
                case AlarmStatus.Cancelled:
                    if (!this.CanBeCancelled)
                        throw new InvalidStateException();
                    break;
                case AlarmStatus.Completed:
                    if (!this.CanBeCompleted)
                        throw new InvalidStateException();
                    break;
                case AlarmStatus.Ringing:
                    if (!this.CanBeRinging)
                        throw new InvalidStateException();
                    break;
                case AlarmStatus.Set:
                    if (!this.CanBeSet)
                        throw new InvalidStateException();
                    break;
                case AlarmStatus.Missed:
                    if (!this.CanBeMissed)
                        throw new InvalidStateException();
                    break;
                default:
                    throw new InvalidProgramException("Unknown state: " + this.Status);
            }

            this.Status = status;
        }
    }
}