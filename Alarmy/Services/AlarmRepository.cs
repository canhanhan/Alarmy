using Alarmy.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Alarmy
{
    public class InMemoryAlarmRepository : IAlarmRepository
    {
        private readonly List<IAlarm> alarms;

        public InMemoryAlarmRepository()
        {
            this.alarms = new List<IAlarm>();
        }

        public IEnumerable<IAlarm> List()
        {
            return this.alarms.AsReadOnly();
        }

        public void Add(IAlarm alarm)
        {
            this.alarms.Add(alarm);
        }

        public void Remove(IAlarm alarm)
        {
            this.alarms.Remove(alarm);
        }
    }
}