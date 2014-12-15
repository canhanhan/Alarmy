using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alarmy.Common
{
    public interface IAlarm
    {
        string Title { get; set; }
        AlarmStatus Status { get; }
        string CancelReason { get; set; }
        DateTime Time { get; set; }
        void Set(DateTime time);
        void Cancel(string reason);
        void Complete();
        bool CanBeCancelled { get; }
        bool CanBeCompleted { get; }
        bool CanBeRinging { get; }
        bool CanBeSet { get; }
        bool CanBeMissed { get; }
        void Check();

        #if DEBUG
        void SetStatusTest(AlarmStatus status);
        #endif
    }
}
