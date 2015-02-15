using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Common
{
    internal interface IAlarmRepository
    {
        IEnumerable<IAlarm> Load();
        void Save(IEnumerable<IAlarm> alarms);
    }
}
