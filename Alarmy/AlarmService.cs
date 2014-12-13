using Alarmy.Common;
using System;
using System.Linq;

namespace Alarmy
{
    internal class AlarmService : IAlarmService, IDisposable
    {
        private static readonly AlarmStatus[] ALARM_STATUSES_TO_CHECK = new[] { AlarmStatus.Ringing, AlarmStatus.Set };
        private readonly ITimerService _Timer;
        private readonly IAlarmRepository _Repository;

        public event EventHandler<AlarmStatusChangedEventArgs> AlarmStatusChanged;

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

        public AlarmService(IAlarmRepository repository, ITimerService timer)
        {
            this._Repository = repository;
            this._Timer = timer;
            this._Timer.Elapsed +=_Timer_Elapsed;
            this.Interval = TimeSpan.FromSeconds(30).Milliseconds;
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
            if (_Timer != null)
                _Timer.Dispose();
        }

        private void _Timer_Elapsed(object sender, EventArgs e)
        {
            var alarms = this._Repository.List().Where(ShouldCheck).ToList();
            var statusCache = alarms.Select(x => x.Status).ToArray();
            alarms.ForEach(alarm => alarm.Check());

            for (var i = 0; i < alarms.Count; i++)
            {
                var alarm = alarms[i];
                var status = statusCache[i];      
                if (alarm.Status != status)
                {
                    if (this.AlarmStatusChanged != null)
                        this.AlarmStatusChanged.Invoke(this, new AlarmStatusChangedEventArgs(alarm, status));
                }
            }
        }

        private static bool ShouldCheck(Alarm alarm)
        {
            return ALARM_STATUSES_TO_CHECK.Any(status => status == alarm.Status);
        }
    }
}
