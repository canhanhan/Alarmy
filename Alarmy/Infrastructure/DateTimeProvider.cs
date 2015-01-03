using Alarmy.Common;
using System;

namespace Alarmy.Infrastructure
{
    internal class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
    }
}
