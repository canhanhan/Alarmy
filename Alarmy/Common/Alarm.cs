using Alarmy.Infrastructure;
using Castle.Core.Logging;
using System;
using System.Globalization;
using System.Linq;

namespace Alarmy.Common
{
    internal class Alarm : IAlarm
    {
        private static readonly AlarmStatus[] AlarmStatusesToCheck = new[] { AlarmStatus.Ringing, AlarmStatus.Set };
      
        private readonly IDateTimeProvider _DateTimeProvider;
        private Guid _Id = Guid.NewGuid();

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

        public Alarm() 
        {
            _DateTimeProvider = new DateTimeProvider();
        }

        public Alarm(IDateTimeProvider dateTimeProvider)
        {
            _DateTimeProvider = dateTimeProvider;
        }

        public void SetTime(DateTime time)
        {
            Time = time;
            IsHushed = false;
            SetStatus(AlarmStatus.Set);
        }

        public void Cancel()
        {
            IsHushed = false;
            SetStatus(AlarmStatus.Canceled);
        }

        public void Complete()
        {
            IsHushed = false;
            SetStatus(AlarmStatus.Completed);
        }

        public bool CanBeCanceled
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

        public bool IsRinging
        {
            get
            {
                return Status == AlarmStatus.Ringing && !IsHushed;
            }
        }

        public bool CheckStatusChange()
        {
            if (!AlarmStatusesToCheck.Any(status => status == this.Status))
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

        public void Import(IAlarm alarm)
        {
            if (alarm == null)
                throw new ArgumentNullException("alarm");

            this.Status = alarm.Status;
            this.Time = alarm.Time;
            this.Title = alarm.Title;
            this.IsHushed = alarm.IsHushed;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} ({1} - {2})", this.Title, this.Time.ToShortDateString(), this.Id.ToString("B"));
        }

        internal void SetStatus(AlarmStatus status)
        {
            if (status == Status)
            {
                return;
            }
            switch (status)
            {
                case AlarmStatus.Canceled:
                    if (!CanBeCanceled)
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
            return _DateTimeProvider.Now.RoundToMinute();
        }
    }
}
