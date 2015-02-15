using System;

namespace Alarmy.Common
{
    interface IAlarmReminderManager : ISupportsStartStop
    {
        event EventHandler<AlarmReminderEventArgs> OnRequestNotification;
    }
}
