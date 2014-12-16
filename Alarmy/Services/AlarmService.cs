using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Services
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
            this.Interval = TimeSpan.FromSeconds(30).TotalMilliseconds;
        }

        public void Start()
        {
            this._Timer.Start();
        }
        public void Stop()
        {
            this._Timer.Stop();
        }

        public void Add(IAlarm alarm)
        {
            this._Repository.Add(alarm);
        }

        public void Remove(IAlarm alarm)
        {
            this._Repository.Remove(alarm);
        }

        public void Update(IAlarm alarm)
        {
            this._Repository.Update(alarm);
        }

        public IEnumerable<IAlarm> List()
        {
            return this._Repository.List();
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

        private static bool ShouldCheck(IAlarm alarm)
        {
            return ALARM_STATUSES_TO_CHECK.Any(status => status == alarm.Status);
        }
    }
}
