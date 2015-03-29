using System.Collections.Generic;

namespace Alarmy.Common
{
    internal class AlarmMetadataEqualityComparer : IEqualityComparer<IAlarm>
    {
        public bool Equals(IAlarm x, IAlarm y)
        {
            return x != null && y != null &&
                    object.Equals(x.Time, y.Time) &&
                    object.Equals(x.Title, y.Title);
        }

        public int GetHashCode(IAlarm obj)
        {
            return obj.Time.GetHashCode() ^ obj.Title.GetHashCode();
        }
    }
}
