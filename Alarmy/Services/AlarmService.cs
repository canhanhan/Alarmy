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
        private readonly Dictionary<Guid, IAlarm> _Cache;

        public event EventHandler<AlarmEventArgs> AlarmAdded;
        public event EventHandler<AlarmEventArgs> AlarmRemoved;
        public event EventHandler<AlarmEventArgs> AlarmUpdated;
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

        public AlarmService(IAlarmRepository repository, ITimer timer)
        {
            _Repository = repository;
            _Cache = new Dictionary<Guid, IAlarm>();
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
            RefreshCache();
        }

        public void Remove(IAlarm alarm)
        {
            _Repository.Remove(alarm);
            RefreshCache();
        }

        public void Update(IAlarm alarm)
        {
            _Repository.Update(alarm);
            RefreshCache();
        }

        public IEnumerable<IAlarm> List()
        {
            return _Cache.Values;
        }

        public void Dispose()
        {
            if (_Timer != null)
            {
                _Timer.Dispose();
            }
        }

        private void RefreshCache()
        {
            _Timer_Elapsed(null, null);
        }

        private void _Timer_Elapsed(object sender, EventArgs e)
        {
            var alarms = _Repository.List().ToDictionary(x => x.Id);
            alarms.Values.Where(ShouldCheck).ToList().ForEach(x => x.Check());

            var deletedAlarmKeys = this._Cache.Keys.Where(x => !alarms.ContainsKey(x)).ToArray();
            foreach (var key in deletedAlarmKeys)
            {
                if (this.AlarmRemoved != null)
                    this.AlarmRemoved.Invoke(this, new AlarmEventArgs(_Cache[key]));

                _Cache.Remove(key);
            }

            foreach (var alarm in alarms.Values)
            {
                IAlarm cachedAlarm = null;
                if (_Cache.TryGetValue(alarm.Id, out cachedAlarm))
                {
                    var statusChanged = cachedAlarm.Status != alarm.Status;
                    var metadataChanged = !cachedAlarm.Equals(alarm, true);
                    AlarmStatus status = default(AlarmStatus);

                    if (statusChanged || metadataChanged)
                    {
                        status = cachedAlarm.Status;
                        cachedAlarm.Import(alarm);

                        if (statusChanged && this.AlarmStatusChanged != null)
                            this.AlarmStatusChanged.Invoke(this, new AlarmStatusChangedEventArgs(cachedAlarm, status));

                        if (metadataChanged && this.AlarmUpdated != null)
                            this.AlarmUpdated.Invoke(this, new AlarmEventArgs(cachedAlarm));
                    }
                }
                else
                {
                    this._Cache.Add(alarm.Id, alarm);
                    if (this.AlarmAdded != null)
                        this.AlarmAdded.Invoke(this, new AlarmEventArgs(alarm));
                }
            }           
        }

        private static bool ShouldCheck(IAlarm alarm)
        {
            return ALARM_STATUSES_TO_CHECK.Any(status => status == alarm.Status);
        }
    }
}
