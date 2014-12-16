using Alarmy.Common;
using System;
using System.Collections.Generic;

namespace Alarmy.Services
{
    public class InMemoryAlarmRepository : IAlarmRepository
    {
        private readonly Dictionary<Guid, IAlarm> alarms;

        public InMemoryAlarmRepository()
        {
            this.alarms = new Dictionary<Guid, IAlarm>();
        }

        public IEnumerable<IAlarm> List()
        {
            return this.alarms.Values;
        }

        public void Add(IAlarm alarm)
        {
            this.alarms[alarm.Id] = alarm;
        }

        public void Remove(IAlarm alarm)
        {
            this.alarms.Remove(alarm.Id);
        }

        public void Update(IAlarm alarm)
        {
            this.Add(alarm);
        }
    }
}