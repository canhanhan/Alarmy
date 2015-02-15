using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Core
{
    internal class AlarmService : IAlarmService, IDisposable
    {
        private readonly IEqualityComparer<IAlarm> valueComparer;
        private readonly ITimer timer;
        private readonly IAlarmRepository repository;
        private readonly bool isStoppableRepository;
        private readonly IDictionary<Guid, IAlarm> cache;

        private bool pauseRepositoryUpdates;

        public event EventHandler<AlarmEventArgs> OnAlarmAdd;
        public event EventHandler<AlarmEventArgs> OnAlarmRemoval;
        public event EventHandler<AlarmEventArgs> OnAlarmUpdate;
        public event EventHandler<AlarmEventArgs> OnAlarmStatusChange;
      
        public AlarmService(IAlarmRepository repository, ITimer checkTimer)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            if (checkTimer == null)
                throw new ArgumentNullException("checkTimer");

            this.cache = new Dictionary<Guid, IAlarm>();
            this.valueComparer = new AlarmEqualityComparer();
            this.repository = repository;
            this.isStoppableRepository = typeof(ISupportsStartStop).IsAssignableFrom(repository.GetType());
            this.timer = checkTimer;
            this.timer.Elapsed += timer_Elapsed;
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
                this.UpdateRepository();

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
                this.UpdateRepository();

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
                this.UpdateRepository();

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
            try
            {
                this.pauseRepositoryUpdates = true;
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
            finally
            {
                this.pauseRepositoryUpdates = false;
                this.UpdateRepository();
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
                if (!this.RefreshRepository())
                foreach (var alarm in this.cache.Values)
                {
                    this.CheckAlarmStatus(alarm);
                }
            }
        }

        private bool RefreshRepository()
        {
            lock (this.cache)
            {                
                var alarmList = this.repository.Load();
                if (alarmList == null)
                    return false;

                var alarms = alarmList.ToDictionary(x => x.Id);
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
                        if (!this.valueComparer.Equals(cachedAlarm, alarm))
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

                return true;
            }
        }

        private void UpdateRepository()
        {
            if (this.pauseRepositoryUpdates)
                return;

            this.repository.Save(this.cache.Values);
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
