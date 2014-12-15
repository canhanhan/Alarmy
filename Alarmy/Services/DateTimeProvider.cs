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
    }
}
