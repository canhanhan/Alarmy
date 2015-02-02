using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alarmy.Tests.Utils
{
    internal abstract class FakeAlarmService : IAlarmService
    {
        private readonly List<IAlarm> alarms = new List<IAlarm>();

        public event EventHandler<AlarmEventArgs> OnAlarmStatusChange;

        public event EventHandler<AlarmEventArgs> OnAlarmAdd;

        public event EventHandler<AlarmEventArgs> OnAlarmRemoval;

        public event EventHandler<AlarmEventArgs> OnAlarmUpdate;

        public double RepositoryRefreshInterval { get; set; }

        public abstract void Start();

        public abstract void Stop();

        public IDictionary<Guid, IAlarm> Cache { get; set; }

        public void Add(IAlarm alarm)
        {
            this.alarms.Add(alarm);
            this.OnAlarmAdd.Invoke(e: new AlarmEventArgs(alarm));
        }

        public void Remove(IAlarm alarm)
        {
            this.alarms.Remove(alarm);
            this.OnAlarmRemoval.Invoke(e: new AlarmEventArgs(alarm));
        }

        public void Update(IAlarm alarm) {
            this.OnAlarmUpdate.Invoke(e: new AlarmEventArgs(alarm));
        }

        public void AddStorage(IAlarm alarm)
        {
            this.alarms.Add(alarm);
        }

        public void TriggerAlarmStatusChange(IAlarm alarm)
        {
            if (!this.alarms.Contains(alarm))
                this.alarms.Add(alarm);

            this.OnAlarmStatusChange.Invoke(e: new AlarmEventArgs(alarm));
        }
        
        public IEnumerable<IAlarm> List()
        {
            return this.alarms.AsReadOnly();
        }

        public void Dispose() {}
    }
}
