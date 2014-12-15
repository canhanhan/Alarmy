using Alarmy.Common;
using System.Collections.Generic;

namespace Alarmy
{
    public class AlarmRepository : IAlarmRepository
    {
        private readonly List<Alarm> alarms;

        public AlarmRepository()
        {
            this.alarms = new List<Alarm>();
        }

        public IEnumerable<Alarm> List()
        {
            return this.alarms.AsReadOnly();
        }

        public void Add(Alarm alarm)
        {
            this.alarms.Add(alarm);
        }

        public void Remove(Alarm alarm)
        {
            this.alarms.Remove(alarm);
        }
    }
}
