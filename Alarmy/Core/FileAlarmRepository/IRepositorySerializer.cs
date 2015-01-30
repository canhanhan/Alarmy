using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Alarmy.Core.FileAlarmRepository
{
    interface IRepositorySerializer
    {
        IDictionary<Guid, IAlarm> Deserialize(Stream stream);
        void Serialize(IDictionary<Guid, IAlarm> alarms, Stream stream);
    }
}
