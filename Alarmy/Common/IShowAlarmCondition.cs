namespace Alarmy.Common
{
    interface IShowAlarmCondition
    {
        bool Match(IAlarm alarm);
    }
}
