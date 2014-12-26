using System;

namespace Alarmy.Common
{
    public class AlarmEventArgs : EventArgs
    {
        public IAlarm Alarm { get; private set; }

        public AlarmEventArgs(IAlarm alarm)
        {
            Alarm = alarm;
        }
    }
}
