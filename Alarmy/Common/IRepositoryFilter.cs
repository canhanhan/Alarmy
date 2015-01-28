namespace Alarmy.Common
{
    interface IRepositoryFilter
    {
        bool Match(IAlarm alarm);
    }
}
