using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Common
{
    public interface IAlarmRepository
    {
        IEnumerable<IAlarm> List();
        void Add(IAlarm alarm);
        void Remove(IAlarm alarm);
    }
}
