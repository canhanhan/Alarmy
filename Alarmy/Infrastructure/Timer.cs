using Alarmy.Common;
using System;

namespace Alarmy.Infrastructure
{
    internal class Timer : ITimer, IDisposable
    {
        public event EventHandler Elapsed;
        private readonly System.Timers.Timer timer;

        public double Interval
        {
            get
            {
                return timer.Interval;
            }
        }

        public Timer(int interval)
        {
            this.timer = new System.Timers.Timer() { AutoReset = true };
            this.timer.Elapsed += this.Timer_Elapsed;
            this.timer.Interval = TimeSpan.FromSeconds(interval).TotalMilliseconds;
        }

        public void Start()
        {
            this.timer.Start();
        }

        public void Stop()
        {
            this.timer.Stop();
        }

        public void Reset()
        {
            this.timer.Stop();
            this.timer.Start();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Timer()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.timer != null)
                {
                    this.timer.Dispose();
                }
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.Elapsed != null)
            {
                this.Elapsed.Invoke(this, null);
            }
        }
    }
}
