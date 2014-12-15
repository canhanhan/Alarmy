using System;

namespace Alarmy.Common
{
    public class AlarmStatusChangedEventArgs : EventArgs
    {
        public IAlarm Alarm { get; private set; }
        public AlarmStatus OldStatus { get; private set; }

        public AlarmStatusChangedEventArgs(IAlarm alarm, AlarmStatus oldStatus)
        {
            OldStatus = oldStatus;
            Alarm = alarm;
        }
    }
}
