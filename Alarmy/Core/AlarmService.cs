using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Core
{
    internal class AlarmService : IAlarmService, IDisposable
    {
        private readonly ITimer timer;
        private readonly IAlarmRepository repository;
        private readonly Dictionary<Guid, IAlarm> cache;
        private readonly bool isStoppableRepository;

        public event EventHandler<AlarmEventArgs> OnAlarmAdd;
        public event EventHandler<AlarmEventArgs> OnAlarmRemoval;
        public event EventHandler<AlarmEventArgs> OnAlarmUpdate;
        public event EventHandler<AlarmEventArgs> OnAlarmStatusChange;

        public double Interval
        {
            get
            {
                return this.timer.Interval;
            }
            set
            {
                this.timer.Interval = value;
            }
        }

        public AlarmService(IAlarmRepository repository, ITimer timer, int repositoryRefreshIntervalInSeconds)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            if (timer == null)
                throw new ArgumentNullException("timer");

            this.repository = repository;
            this.isStoppableRepository = typeof(ISupportsStartStop).IsAssignableFrom(repository.GetType());
            this.cache = new Dictionary<Guid, IAlarm>();
            this.timer = timer;
            this.timer.Elapsed += _Timer_Elapsed;

            this.Interval = TimeSpan.FromSeconds(repositoryRefreshIntervalInSeconds).TotalMilliseconds;
        }

        public void Start()
        {
            this.timer.Start();
            if (this.isStoppableRepository)
                ((ISupportsStartStop)this.repository).Start();

            this.Refresh();
        }
        public void Stop()
        {
            this.timer.Stop();
            if (this.isStoppableRepository)
                ((ISupportsStartStop)this.repository).Stop();
        }

        public void Add(IAlarm alarm)
        {
            if (alarm == null)
                throw new ArgumentNullException("alarm");

            this.cache.Add(alarm.Id, alarm);
            this.repository.Add(alarm);

            if (this.OnAlarmAdd != null)
                this.OnAlarmAdd.Invoke(this, new AlarmEventArgs(alarm));

            this.CheckAlarmStatus(alarm);
        }

        public void Remove(IAlarm alarm)
        {
            if (alarm == null)
                throw new ArgumentNullException("alarm");

            this.cache.Remove(alarm.Id);
            this.repository.Remove(alarm);

            if (this.OnAlarmRemoval != null)
                this.OnAlarmRemoval.Invoke(this, new AlarmEventArgs(alarm));
        }

        public void Update(IAlarm alarm)
        {
            if (alarm == null)
                throw new ArgumentNullException("alarm");

            this.cache[alarm.Id] = alarm;
            this.repository.Update(alarm);

            if (this.OnAlarmUpdate != null)
                this.OnAlarmUpdate.Invoke(this, new AlarmEventArgs(alarm));

            this.CheckAlarmStatus(alarm);
        }

        public IEnumerable<IAlarm> List()
        {
            return this.cache.Values;
        }

        public void Dispose()
        {
            if (this.timer != null)
                this.timer.Dispose();
        }

        private void _Timer_Elapsed(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            var alarms = this.repository.List().ToDictionary(x => x.Id);

            var deletedAlarmKeys = this.cache.Keys.Where(x => !alarms.ContainsKey(x)).ToArray();
            foreach (var key in deletedAlarmKeys)
            {
                if (this.OnAlarmRemoval != null)
                    this.OnAlarmRemoval.Invoke(this, new AlarmEventArgs(this.cache[key]));

                this.cache.Remove(key);
            }

            foreach (var alarm in alarms.Values)
            {
                IAlarm cachedAlarm = null;
                if (this.cache.TryGetValue(alarm.Id, out cachedAlarm))
                {
                    if (!cachedAlarm.Equals(alarm, true))
                    {
                        cachedAlarm.Import(alarm);

                        if (this.OnAlarmUpdate != null)
                            this.OnAlarmUpdate.Invoke(this, new AlarmEventArgs(cachedAlarm));
                    }
                }
                else
                {
                    cachedAlarm = alarm;
                    this.cache.Add(alarm.Id, cachedAlarm);
                    if (this.OnAlarmAdd != null)
                        this.OnAlarmAdd.Invoke(this, new AlarmEventArgs(cachedAlarm));
                }

                this.CheckAlarmStatus(cachedAlarm);
            }
        }

        private void CheckAlarmStatus(IAlarm cachedAlarm)
        {
            if (cachedAlarm.CheckStatusChange())
            {
                if (this.OnAlarmStatusChange != null)
                    this.OnAlarmStatusChange.Invoke(this, new AlarmEventArgs(cachedAlarm));
            }
        }
    }
}
