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
            alarms = new Dictionary<Guid, IAlarm>();
        }

        public IEnumerable<IAlarm> List()
        {
            return alarms.Values;
        }

        public void Add(IAlarm alarm)
        {
            alarms[alarm.Id] = alarm;
        }

        public void Remove(IAlarm alarm)
        {
            alarms.Remove(alarm.Id);
        }

        public void Update(IAlarm alarm)
        {
            Add(alarm);
        }
    }
}
