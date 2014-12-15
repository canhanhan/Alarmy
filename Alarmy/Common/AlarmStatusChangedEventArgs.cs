using System;

namespace Alarmy.Common
{
    public class AlarmStatusChangedEventArgs : EventArgs
    {
        public Alarm Alarm { get; private set; }
        public AlarmStatus OldStatus { get; private set; }

        public AlarmStatusChangedEventArgs(Alarm alarm, AlarmStatus oldStatus)
        {
            OldStatus = oldStatus;
            Alarm = alarm;
        }
    }
}
