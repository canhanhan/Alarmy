using System;
using System.Collections.Generic;

namespace Alarmy.Common
{
    public interface IAlarmService : IDisposable
    {
        event EventHandler<AlarmStatusChangedEventArgs> AlarmStatusChanged;

        double Interval { get; set; }
        void Start();
        void Stop();

        void Add(IAlarm alarm);
        void Remove(IAlarm alarm);
        IEnumerable<IAlarm> List();
    }
}
