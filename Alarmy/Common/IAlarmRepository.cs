using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Common
{
    public interface IAlarmRepository
    {
        IEnumerable<Alarm> List();
        void Add(Alarm alarm);
        void Remove(Alarm alarm);
    }
}
