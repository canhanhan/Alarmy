using System;

namespace Alarmy.Common
{
    interface ISharedFileFactory
    {
        ISharedFile Read(string path);
        ISharedFile Write(string path);
    }
}
