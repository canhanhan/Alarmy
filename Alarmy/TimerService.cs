using Alarmy.Common;
using System;
using System.Timers;

namespace Alarmy
{
    public class TimerService : ITimerService
    {
        public event EventHandler Elapsed;
        private readonly Timer _Timer;

        public double Interval
        {
            get
            {
                return this._Timer.Interval;
            }
            set
            {
                this._Timer.Interval = value;
            }
        }

        public TimerService()
        {
            this._Timer = new Timer();
            this._Timer.AutoReset = true;
            this._Timer.Elapsed += _Timer_Elapsed;
        }

        public void Start()
        {
            this._Timer.Start();
        }

        public void Stop()
        {
            this._Timer.Stop();
        }

        public void Dispose()
        {
            if (this._Timer != null)
                this._Timer.Dispose();
        }

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.Elapsed != null)
                this.Elapsed.Invoke(this, null);
        }
    }
}
