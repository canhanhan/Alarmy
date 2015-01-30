using Alarmy.Common;
using System;

namespace Alarmy.Core.FileAlarmRepository
{
    internal interface IFileWatcher : IDisposable, ISupportsStartStop
    {
        event EventHandler FileChanged;        
    }
}
