using System;

namespace Alarmy.Common
{
    interface IAlarmService : IDisposable
    {
        event EventHandler<AlarmStatusChangedEventArgs> AlarmStatusChanged;

        double Interval { get; set; }
        void Start();
        void Stop();
    }
}
