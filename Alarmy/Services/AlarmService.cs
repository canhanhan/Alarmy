using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Services
{
    internal class AlarmService : IAlarmService, IDisposable
    {
        private static readonly AlarmStatus[] ALARM_STATUSES_TO_CHECK = new [] { AlarmStatus.Ringing, AlarmStatus.Set };
        private readonly ITimerService _Timer;
        private readonly IAlarmRepository _Repository;

        public event EventHandler<AlarmStatusChangedEventArgs> AlarmStatusChanged;

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

        public AlarmService(IAlarmRepository repository, ITimerService timer)
        {
            _Repository = repository;
            _Timer = timer;
            _Timer.Elapsed += _Timer_Elapsed;
            Interval = TimeSpan.FromSeconds(30).TotalMilliseconds;
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
            var statusCache = alarms.Select(x => x.Status).ToArray();
            alarms.ForEach(alarm => alarm.Check());

            for (var i = 0; i < alarms.Count; i++)
            {
                var alarm = alarms[i];
                var status = statusCache[i];
                if (alarm.Status != status)
                {
                    if (AlarmStatusChanged != null)
                    {
                        AlarmStatusChanged.Invoke(this, new AlarmStatusChangedEventArgs(alarm, status));
                    }
                }
            }
        }

        private static bool ShouldCheck(IAlarm alarm)
        {
            return ALARM_STATUSES_TO_CHECK.Any(status => status == alarm.Status);
        }
    }
}
