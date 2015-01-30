using System;

namespace Alarmy.Core.FileAlarmRepository
{
    interface ISharedFileFactory
    {
        ISharedFile Read(string path);
        ISharedFile Write(string path);
    }
}
