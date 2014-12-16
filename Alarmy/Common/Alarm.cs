﻿using System;

namespace Alarmy.Common
{
    public class Alarm : IAlarm
    {
        private Guid _Id = Guid.NewGuid();

        public const int ISWORTHSHOWING_FRESNESS = 1;
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

        public string CancelReason { get; set; }

        public DateTime Time { get; set; }

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
            SetStatus(AlarmStatus.Set);
        }

#if DEBUG
        public void SetStatusTest(AlarmStatus status)
        {
            Status = status;
        }
#endif

        public virtual void Cancel(string reason)
        {
            CancelReason = reason;
            SetStatus(AlarmStatus.Cancelled);
        }

        public virtual void Complete()
        {
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

        public void Check()
        {
            if (Status != AlarmStatus.Set && Status != AlarmStatus.Ringing)
            {
                return;
            }
            var time = GetTime();
            if (Status != AlarmStatus.Ringing && Time >= time && Time < time.AddMinutes(1))
            {
                SetStatus(AlarmStatus.Ringing);
            }
            else
            {
                if (Time < time)
                {
                    SetStatus(AlarmStatus.Missed);
                }
            }
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
