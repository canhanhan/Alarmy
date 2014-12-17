using Alarmy.Common;
using System;

namespace Alarmy.Infrastructure
{
    public class Timer : ITimer
    {
        public event EventHandler Elapsed;
        private readonly System.Timers.Timer _Timer;

        public double Interval
        {
            get
            {
                return _Timer.Interval;
            }
            set
            {
                _Timer.Interval = value;
            }
        }

        public Timer()
        {
            _Timer = new System.Timers.Timer() { AutoReset = true };
            _Timer.Elapsed += _Timer_Elapsed;
        }

        public void Start()
        {
            _Timer.Start();
        }

        public void Stop()
        {
            _Timer.Stop();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Timer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_Timer != null)
                {
                    _Timer.Dispose();
                }
            }
        }

        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Elapsed != null)
            {
                Elapsed.Invoke(this, null);
            }
        }
    }
}
