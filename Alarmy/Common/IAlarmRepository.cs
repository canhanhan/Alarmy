using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Common
{
    internal interface IAlarmRepository
    {
        bool IsDirty { get; }
        IEnumerable<IAlarm> List();
        void Add(IAlarm alarm);
        void Remove(IAlarm alarm);
        void Update(IAlarm alarm);
    }
}
