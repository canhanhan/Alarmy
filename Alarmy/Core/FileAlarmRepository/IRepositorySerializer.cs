using Alarmy.Common;
using System.Collections.Generic;
using System.IO;

namespace Alarmy.Core.FileAlarmRepository
{
    interface IRepositorySerializer
    {
        IEnumerable<IAlarm> Deserialize(Stream stream);
        void Serialize(IEnumerable<IAlarm> alarms, Stream stream);
    }
}
