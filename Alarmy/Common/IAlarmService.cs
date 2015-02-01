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

        double Interval { get; set; }
        void Start();
        void Stop();

        void Add(IAlarm alarm);
        void Remove(IAlarm alarm);
        void Update(IAlarm item);
        IEnumerable<IAlarm> List();
    }
}
