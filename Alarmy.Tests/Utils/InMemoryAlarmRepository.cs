using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Tests
{
    internal abstract class InMemoryAlarmRepository : IAlarmRepository, ISupportsStartStop
    {
        public Dictionary<Guid, IAlarm> Alarms;

        public InMemoryAlarmRepository()
        {
            this.Alarms = new Dictionary<Guid, IAlarm>();            
        }

        public IEnumerable<IAlarm> Load()
        {
            return this.Alarms.Values;
        }

        public void Add(IAlarm alarm)
        {
            this.Alarms.Add(alarm.Id, alarm);
        }

        public void Update(IAlarm alarm)
        {
            this.Alarms[alarm.Id] = alarm;
        }

        public void Remove(IAlarm alarm)
        {
            this.Alarms.Remove(alarm.Id);
        }

        public void Save(IEnumerable<IAlarm> alarms)
        {
            this.Alarms = alarms.ToDictionary(x => x.Id);
        }

        public abstract void Start();
        public abstract void Stop();       
    }
}
