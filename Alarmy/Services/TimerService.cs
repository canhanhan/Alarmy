using Alarmy.Common;
using System;
using System.Timers;

namespace Alarmy.Services
{
    public class TimerService : ITimerService
    {
        public event EventHandler Elapsed;
        private readonly Timer _Timer;

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

        public TimerService()
        {
            _Timer = new Timer();
            _Timer.AutoReset = true;
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
            if (_Timer != null)
            {
                _Timer.Dispose();
            }
        }

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Elapsed != null)
            {
                Elapsed.Invoke(this, null);
            }
        }
    }
}
