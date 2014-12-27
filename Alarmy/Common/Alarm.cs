using Castle.Core.Logging;
using System;
using System.Linq;

namespace Alarmy.Common
{
    public class Alarm : IAlarm
    {
        public const int ISWORTHSHOWING_FRESNESS = 1;
        private static readonly AlarmStatus[] ALARM_STATUSES_TO_CHECK = new[] { AlarmStatus.Ringing, AlarmStatus.Set };


        private Guid _Id = Guid.NewGuid();
        private readonly IDateTimeProvider _DateTimeProvider;

        public Guid Id
        {
            get
            {
                return _Id;
            }
            set
            {
                _Id = value;
            }
        }

        public string Title { get; set; }

        public AlarmStatus Status { get; set; }

        public DateTime Time { get; set; }

        public bool IsHushed { get; set; }

        public bool IsWorthShowing
        {
            get
            {
                return !((Status == AlarmStatus.Cancelled || Status == AlarmStatus.Completed) && Time < GetTime().AddMinutes(-ISWORTHSHOWING_FRESNESS));
            }
        }

        public Alarm()
        {
        }

        public Alarm(IDateTimeProvider dateTimeProvider)
        {
            _DateTimeProvider = dateTimeProvider;
        }

        public void Set(DateTime time)
        {
            Time = time;
            IsHushed = false;
            SetStatus(AlarmStatus.Set);
        }

#if DEBUG
        public void SetStatusTest(AlarmStatus status)
        {
            Status = status;
        }
#endif

        public void Cancel()
        {
            IsHushed = false;
            SetStatus(AlarmStatus.Cancelled);
        }

        public void Complete()
        {
            IsHushed = false;
            Status = AlarmStatus.Completed;
        }

        public bool CanBeCancelled
        {
            get
            {
                return Status == AlarmStatus.Set || Status == AlarmStatus.Missed || Status == AlarmStatus.Ringing;
            }
        }

        public bool CanBeCompleted
        {
            get
            {
                return Status == AlarmStatus.Ringing || Status == AlarmStatus.Missed;
            }
        }

        public bool CanBeRinging
        {
            get
            {
                return Status == AlarmStatus.Set;
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
                return Status == AlarmStatus.Ringing || Status == AlarmStatus.Set;
            }
        }

        public bool CheckStatusChange()
        {
            if (!ALARM_STATUSES_TO_CHECK.Any(status => status == this.Status))
                return false;

            var time = GetTime();
            if (Status != AlarmStatus.Ringing && Time >= time && Time < time.AddMinutes(1))
            {
                SetStatus(AlarmStatus.Ringing);
                return true;
            }
            else if (Time < time)
            {
                SetStatus(AlarmStatus.Missed);
                return true;
            }

            return false;
        }

        public bool Equals(IAlarm alarm, bool compareOnlyMetadata)
        {
            if (compareOnlyMetadata)
                return DateTime.Equals(this.Time, alarm.Time)
                        && string.Equals(this.Title, alarm.Title)
                        && bool.Equals(this.IsHushed, alarm.IsHushed);
            else
                return this.Equals(alarm);
        }

        public void Import(IAlarm alarm)
        {
            this.Status = alarm.Status;
            this.Time = alarm.Time;
            this.Title = alarm.Title;
            this.IsHushed = alarm.IsHushed;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1} - {2})", this.Title, this.Time.ToShortDateString(), this.Id.ToString("B"));
        }

        internal void SetStatus(AlarmStatus status)
        {
            if (status == Status)
            {
                return;
            }
            switch (status)
            {
                case AlarmStatus.Cancelled:
                    if (!CanBeCancelled)
                    {
                        throw new InvalidStateException();
                    }
                    break;
                case AlarmStatus.Completed:
                    if (!CanBeCompleted)
                    {
                        throw new InvalidStateException();
                    }
                    break;
                case AlarmStatus.Ringing:
                    if (!CanBeRinging)
                    {
                        throw new InvalidStateException();
                    }
                    break;
                case AlarmStatus.Set:
                    if (!CanBeSet)
                    {
                        throw new InvalidStateException();
                    }
                    break;
                case AlarmStatus.Missed:
                    if (!CanBeMissed)
                    {
                        throw new InvalidStateException();
                    }
                    break;
                default:
                    throw new InvalidProgramException("Unknown state: " + Status);
            }

            Status = status;
        }

        private DateTime GetTime()
        {
            return (_DateTimeProvider == null ? DateTime.Now : _DateTimeProvider.Now).RoundToMinute();
        }
    }
}
