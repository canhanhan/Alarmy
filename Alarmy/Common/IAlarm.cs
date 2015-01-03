using System;

namespace Alarmy.Common
{
    internal interface IAlarm
    {
        Guid Id { get; set; }
        string Title { get; set; }
        AlarmStatus Status { get; }
        DateTime Time { get; set; }
        bool IsHushed { get; set; }
        void SetTime(DateTime time);
        void Cancel();
        void Complete();
        bool CanBeCanceled { get; }
        bool CanBeCompleted { get; }
        bool CanBeRinging { get; }
        bool CanBeSet { get; }
        bool CanBeMissed { get; }
        bool CheckStatusChange();

        bool Equals(IAlarm alarm, bool compareOnlyMetadata);
        void Import(IAlarm alarm);
#if DEBUG
        void SetStatusTest(AlarmStatus status);
#endif
    }
}
