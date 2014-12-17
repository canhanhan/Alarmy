using System;

namespace Alarmy.Common
{
    public interface ITimer : IDisposable
    {
        event EventHandler Elapsed;
        double Interval { get; set; }
        void Start();
        void Stop();
    }
}
