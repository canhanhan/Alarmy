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

        public event EventHandler<AlarmEventArgs> AlarmAdded;
        public event EventHandler<AlarmEventArgs> AlarmRemoved;
        public event EventHandler<AlarmEventArgs> AlarmUpdated;
        public event EventHandler<AlarmEventArgs> AlarmStatusChanged;

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

            if (this.AlarmAdded != null)
                this.AlarmAdded.Invoke(this, new AlarmEventArgs(alarm));
        }

        public void Remove(IAlarm alarm)
        {
            if (alarm == null)
                throw new ArgumentNullException("alarm");

            this.cache.Remove(alarm.Id);
            this.repository.Remove(alarm);

            if (this.AlarmRemoved != null)
                this.AlarmRemoved.Invoke(this, new AlarmEventArgs(alarm));
        }

        public void Update(IAlarm alarm)
        {
            if (alarm == null)
                throw new ArgumentNullException("alarm");

            this.cache[alarm.Id] = alarm;
            this.repository.Update(alarm);

            if (this.AlarmUpdated != null)
                this.AlarmUpdated.Invoke(this, new AlarmEventArgs(alarm));
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
            var alarms = this.repository.List().ToDictionary(x => x.Id);

            var deletedAlarmKeys = this.cache.Keys.Where(x => !alarms.ContainsKey(x)).ToArray();
            foreach (var key in deletedAlarmKeys)
            {
                if (this.AlarmRemoved != null)
                    this.AlarmRemoved.Invoke(this, new AlarmEventArgs(this.cache[key]));

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

                        if (this.AlarmUpdated != null)
                            this.AlarmUpdated.Invoke(this, new AlarmEventArgs(cachedAlarm));
                    }
                }
                else
                {
                    cachedAlarm = alarm;
                    this.cache.Add(alarm.Id, cachedAlarm);
                    if (this.AlarmAdded != null)
                        this.AlarmAdded.Invoke(this, new AlarmEventArgs(cachedAlarm));
                }

                if (cachedAlarm.CheckStatusChange())
                {
                    if (this.AlarmStatusChanged != null)
                        this.AlarmStatusChanged.Invoke(this, new AlarmEventArgs(cachedAlarm));
                }
            }
        }
    }
}
