using System;
using System.Collections.Generic;

namespace Alarmy.Common
{
    internal interface IAlarmService : IDisposable
    {
        event EventHandler<AlarmEventArgs> AlarmStatusChanged;
        event EventHandler<AlarmEventArgs> AlarmAdded;
        event EventHandler<AlarmEventArgs> AlarmRemoved;
        event EventHandler<AlarmEventArgs> AlarmUpdated;

        double Interval { get; set; }
        void Start();
        void Stop();

        void Add(IAlarm alarm);
        void Remove(IAlarm alarm);
        void Update(IAlarm item);
        IEnumerable<IAlarm> List();
    }
}
