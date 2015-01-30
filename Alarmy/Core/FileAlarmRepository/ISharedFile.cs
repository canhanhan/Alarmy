using System;
using System.IO;

namespace Alarmy.Core.FileAlarmRepository
{
    interface ISharedFile : IDisposable
    {
        Stream Stream { get; }
    }
}
