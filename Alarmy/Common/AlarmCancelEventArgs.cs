using System;

namespace Alarmy.Common
{
    public class AlarmCancelEventArgs : EventArgs
    {
        public IAlarm Alarm { get; private set; }
        public string Reason { get; private set; }

        public AlarmCancelEventArgs(IAlarm alarm, string reason)
        {
            this.Reason = reason;
            this.Alarm = alarm;
        }
    }
}
