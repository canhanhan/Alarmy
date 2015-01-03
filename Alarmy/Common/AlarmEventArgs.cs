using System;

namespace Alarmy.Common
{
    internal class AlarmEventArgs : EventArgs
    {
        public IAlarm Alarm { get; private set; }

        public AlarmEventArgs(IAlarm alarm)
        {
            Alarm = alarm;
        }
    }
}
