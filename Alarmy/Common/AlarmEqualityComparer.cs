using System.Collections.Generic;

namespace Alarmy.Common
{
    internal class AlarmEqualityComparer : IEqualityComparer<IAlarm>
    {
        public bool Equals(IAlarm x, IAlarm y)
        {
            return x != null && y != null &&
                    object.Equals(x.Status, y.Status) &&
                    object.Equals(x.Time, y.Time) &&
                    object.Equals(x.Title, y.Title) &&
                    object.Equals(x.IsHushed, y.IsHushed);
        }

        public int GetHashCode(IAlarm obj)
        {
            return obj.Status.GetHashCode() ^ obj.Time.GetHashCode() ^ obj.Title.GetHashCode() ^ obj.IsHushed.GetHashCode();
        }
    }
}
