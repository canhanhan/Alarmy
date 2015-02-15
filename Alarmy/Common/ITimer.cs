using System;

namespace Alarmy.Common
{
    internal interface ITimer : IDisposable
    {
        event EventHandler Elapsed;
        double Interval { get; }
        void Start();
        void Stop();
        void Reset();
    }
}
