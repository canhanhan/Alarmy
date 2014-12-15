using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alarmy.Common
{
    public abstract class Alarm
    {
        public string Title { get; set; }
#if DEBUG
        public AlarmStatus Status { get; internal set; }
#else
        public AlarmStatus Status { get; private set; }
#endif
        public string CancelReason { get; private set; }
        public abstract bool IsDelayable { get; }

        public virtual void Cancel(string reason)
        {
            this.CancelReason = reason;
            this.SetStatus(AlarmStatus.Cancelled);
        }

        public virtual void Complete()
        {
            this.Status = AlarmStatus.Completed;
        }

        public abstract void Check();

        protected void SetStatus(AlarmStatus status)
        {
            if (status == this.Status)
                return;

            switch (this.Status)
            {
                case AlarmStatus.Cancelled:
                    throw new CancelledInvalidStateException();
                case AlarmStatus.Completed:
                    throw new CompletedInvalidStateException();
                case AlarmStatus.Ringing:
                    if (status != AlarmStatus.Missed && status != AlarmStatus.Completed && status != AlarmStatus.Set)
                        throw new RingingInvalidStateException();
                    break;
                case AlarmStatus.Set:
                    if (status != AlarmStatus.Cancelled && status != AlarmStatus.Ringing && status != AlarmStatus.Missed)
                        throw new SetInvalidStateException();
                    break;
                case AlarmStatus.Missed:
                    if (status != AlarmStatus.Completed && status != AlarmStatus.Cancelled)
                        throw new MissedInvalidStateException();
                    break;
                default:
                    throw new InvalidProgramException("Unknown state: " + this.Status);
            }

            this.Status = status;
        }
    }
}