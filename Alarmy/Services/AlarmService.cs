using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Services
{
    internal class AlarmService : IAlarmService, IDisposable
    {
        private const int DEFAULT_INTERVAL = 30;
        private static readonly AlarmStatus[] ALARM_STATUSES_TO_CHECK = new [] { AlarmStatus.Ringing, AlarmStatus.Set };
        private readonly ITimer _Timer;
        private readonly IAlarmRepository _Repository;

        public event EventHandler<AlarmStatusChangedEventArgs> AlarmStatusChanged;
        private AlarmStatus[] statusCache;

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

        public AlarmService(IAlarmRepository repository, ITimer timer)
        {
            _Repository = repository;
            _Timer = timer;
            _Timer.Elapsed += _Timer_Elapsed;
            Interval = TimeSpan.FromSeconds(DEFAULT_INTERVAL).TotalMilliseconds;
        }

        public void Start()
        {
            _Timer.Start();
        }
        public void Stop()
        {
            _Timer.Stop();
        }

        public void Add(IAlarm alarm)
        {
            _Repository.Add(alarm);
        }

        public void Remove(IAlarm alarm)
        {
            _Repository.Remove(alarm);
        }

        public void Update(IAlarm alarm)
        {
            _Repository.Update(alarm);
        }

        public IEnumerable<IAlarm> List()
        {
            return _Repository.List();
        }

        public void Dispose()
        {
            if (_Timer != null)
            {
                _Timer.Dispose();
            }
        }

        private void _Timer_Elapsed(object sender, EventArgs e)
        {
            var alarms = _Repository.List().Where(ShouldCheck).ToList();
            if (this.statusCache == null)
                this.statusCache = alarms.Select(x => x.Status).ToArray();

            alarms.ForEach(alarm => alarm.Check());
                
            for (var i = 0; i < alarms.Count && i < this.statusCache.Length; i++)
            {
                var alarm = alarms[i];
                var status = this.statusCache[i];
                if (alarm.Status != status)
                {
                    if (AlarmStatusChanged != null)
                    {
                        AlarmStatusChanged.Invoke(this, new AlarmStatusChangedEventArgs(alarm, status));
                    }
                }
            }

            this.statusCache = alarms.Select(x => x.Status).ToArray();
        }

        private static bool ShouldCheck(IAlarm alarm)
        {
            return ALARM_STATUSES_TO_CHECK.Any(status => status == alarm.Status);
        }
    }
}
