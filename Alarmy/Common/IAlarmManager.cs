using System;

namespace Alarmy.Common
{
    internal interface IAlarmManager
    {
        event EventHandler OnSleep;
        event EventHandler OnWakeup;
    }
}
