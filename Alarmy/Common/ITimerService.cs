using System;

namespace Alarmy.Common
{
    public interface ITimerService : IDisposable
    {
        event EventHandler Elapsed;
        double Interval { get; set; }
        void Start();
        void Stop();
    }
}
