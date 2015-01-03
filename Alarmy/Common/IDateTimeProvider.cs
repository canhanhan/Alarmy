using System;

namespace Alarmy.Common
{
    internal interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}
