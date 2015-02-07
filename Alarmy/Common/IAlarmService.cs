using System;
using System.Collections.Generic;

namespace Alarmy.Common
{
    internal interface IAlarmService : IDisposable, ISupportsStartStop
    {
        event EventHandler<AlarmEventArgs> OnAlarmStatusChange;
        event EventHandler<AlarmEventArgs> OnAlarmAdd;
        event EventHandler<AlarmEventArgs> OnAlarmRemoval;
        event EventHandler<AlarmEventArgs> OnAlarmUpdate;

        void Add(IAlarm alarm);
        void Remove(IAlarm alarm);
        void Update(IAlarm item);
        void Import(IEnumerable<IAlarm> alarms, bool deleteExisting);

        IEnumerable<IAlarm> List();
    }
}
