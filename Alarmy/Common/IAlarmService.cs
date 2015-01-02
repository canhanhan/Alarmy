using System;
using System.Collections.Generic;

namespace Alarmy.Common
{
    public interface IAlarmService : IDisposable
    {
        event EventHandler<AlarmEventArgs> AlarmStatusChanged;
        event EventHandler<AlarmEventArgs> AlarmAdded;
        event EventHandler<AlarmEventArgs> AlarmRemoved;
        event EventHandler<AlarmEventArgs> AlarmUpdated;

        double Interval { get; set; }
        void StartTimer();
        void StopTimer();

        void Add(IAlarm alarm);
        void Remove(IAlarm alarm);
        void Update(IAlarm item);
        IEnumerable<IAlarm> List();
    }
}
