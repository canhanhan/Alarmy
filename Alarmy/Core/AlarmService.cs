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
        private readonly bool isStoppableRepository;
        private readonly IDictionary<Guid, IAlarm> cache;

        public event EventHandler<AlarmEventArgs> OnAlarmAdd;
        public event EventHandler<AlarmEventArgs> OnAlarmRemoval;
        public event EventHandler<AlarmEventArgs> OnAlarmUpdate;
        public event EventHandler<AlarmEventArgs> OnAlarmStatusChange;
      
        public AlarmService(IAlarmRepository repository, ITimer timer, int checkInterval)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            if (timer == null)
                throw new ArgumentNullException("timer");

            this.repository = repository;
            this.isStoppableRepository = typeof(ISupportsStartStop).IsAssignableFrom(repository.GetType());
            this.cache = new Dictionary<Guid, IAlarm>();
            this.timer = timer;
            this.timer.Elapsed += timer_Elapsed;
            this.timer.Interval = TimeSpan.FromSeconds(checkInterval).TotalMilliseconds;
        }

        public void Start()
        {
            this.timer.Start();

            if (this.isStoppableRepository)
                ((ISupportsStartStop)this.repository).Start();

            this.RefreshRepository();
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

            lock (this.cache)
            {
                this.cache.Add(alarm.Id, alarm);
                this.repository.Add(alarm);

                if (this.OnAlarmAdd != null)
                    this.OnAlarmAdd.Invoke(this, new AlarmEventArgs(alarm));

                this.CheckAlarmStatus(alarm);
            }
        }

        public void Remove(IAlarm alarm)
        {
            if (alarm == null)
                throw new ArgumentNullException("alarm");

            lock (this.cache)
            {
                this.cache.Remove(alarm.Id);
                this.repository.Remove(alarm);

                if (this.OnAlarmRemoval != null)
                    this.OnAlarmRemoval.Invoke(this, new AlarmEventArgs(alarm));
            }
        }

        public void Update(IAlarm alarm)
        {
            if (alarm == null)
                throw new ArgumentNullException("alarm");

            lock (this.cache)
            {
                this.cache[alarm.Id] = alarm;
                this.repository.Update(alarm);

                if (this.OnAlarmUpdate != null)
                    this.OnAlarmUpdate.Invoke(this, new AlarmEventArgs(alarm));

                this.CheckAlarmStatus(alarm);
            }
        }

        public IEnumerable<IAlarm> List()
        {
            return this.cache.Values;
        }
     
        public void Import(IEnumerable<IAlarm> alarms, bool deleteExisting)
        {
            lock (this.cache)
            {
                if (deleteExisting)
                {
                    foreach (var alarm in this.cache.Values.ToArray())
                    {
                        this.Remove(alarm);
                    }
                }

                foreach(var alarm in alarms)
                {
                    this.Add(alarm);
                }
            }
        }
        public void Dispose()
        {
            if (this.timer != null)
                this.timer.Dispose();
        }

        private void timer_Elapsed(object sender, EventArgs e)
        {
            lock (this.cache)
            {
                if (this.repository.IsDirty)
                {
                    this.RefreshRepository();
                }
                else
                {
                    foreach (var alarm in this.cache.Values)
                    {
                        this.CheckAlarmStatus(alarm);
                    }
                }
            }
        }

        private void RefreshRepository()
        {
            lock (this.cache)
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
                        cachedAlarm.Import(alarm);

                        if (this.OnAlarmUpdate != null)
                            this.OnAlarmUpdate.Invoke(this, new AlarmEventArgs(cachedAlarm));
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
