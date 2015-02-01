using Alarmy.Common;
using System;

namespace Alarmy.Tests.Utils
{
    internal class FakeAlarm : IAlarm
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public AlarmStatus Status { get; set; }
        public DateTime Time { get; set; }
        public bool IsHushed { get; set; }

        public bool CanBeCanceled { get; set; }
        public bool CanBeCompleted { get; set; }
        public bool CanBeRinging { get; set; }
        public bool CanBeSet { get; set; }
        public bool CanBeMissed { get; set; }
        public bool IsRinging { get; set; }

        public FakeAlarm()
        {
            this.Id = Guid.NewGuid();
        }

        public void SetTime(DateTime time) { }
        public void Cancel() { }
        public void Complete() { }
        public bool CheckStatusChange() { return true; }
        public bool Equals(IAlarm alarm, bool compareOnlyMetadata) { return true; }
        public void Import(IAlarm alarm) { }
    }
}
