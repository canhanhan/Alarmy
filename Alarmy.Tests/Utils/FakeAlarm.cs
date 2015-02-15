using Alarmy.Common;
using NSubstitute;
using System;

namespace Alarmy.Tests.Utils
{
    internal abstract class FakeAlarm : IAlarm
    {
        public static FakeAlarm GetAlarm(AlarmStatus status = default(AlarmStatus), Guid id = default(Guid))
        {
            var alarm = Substitute.For<FakeAlarm>();
            alarm.Id = id == default(Guid) ? Guid.NewGuid() : id;
            alarm.Status = status;
            return alarm;
        }

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
        public abstract bool CheckStatusChange();
        public void Import(IAlarm alarm) { }
    }
}
