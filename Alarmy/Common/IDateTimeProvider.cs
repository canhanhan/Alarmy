using System;

namespace Alarmy.Common
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}
