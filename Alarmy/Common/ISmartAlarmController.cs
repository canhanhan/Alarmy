using System;

namespace Alarmy.Common
{
    internal interface ISmartAlarmController
    {
        bool IsActive { get; }
        bool IsSilent { get; }

        event EventHandler OnSleep;
        event EventHandler OnSoundOff;
        event EventHandler OnSoundOn;
        event EventHandler OnWakeup;
    }
}
