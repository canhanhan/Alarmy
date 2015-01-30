using Alarmy.Common;

namespace Alarmy.Core.FileAlarmRepository
{
    interface IRepositoryFilter
    {
        bool Match(IAlarm alarm);
    }
}
