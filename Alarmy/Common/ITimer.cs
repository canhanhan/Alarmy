using System;

namespace Alarmy.Common
{
    internal interface ITimer : IDisposable
    {
        event EventHandler Elapsed;
        double Interval { get; set; }
        void StartTimer();
        void StopTimer();
    }
}
