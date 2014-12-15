using Alarmy.Common;
using System;

namespace Alarmy
{
    internal class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }
        public DateTime NowRoundedToCurrentMinute
        {
            get
            {
                var currentTime = this.Now;
                return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);
            }
        }
    }
}
