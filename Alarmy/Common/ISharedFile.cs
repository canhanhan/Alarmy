using System;
using System.IO;

namespace Alarmy.Common
{
    interface ISharedFile : IDisposable
    {
        Stream Stream { get; }
    }
}
