using Alarmy.Common;
using System.IO;

namespace Alarmy.Core.FileAlarmRepository
{
    internal class SharedFileFactory : ISharedFileFactory
    {
        public ISharedFile Read(string path)
        {
            return new SharedFile(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
        }

        public ISharedFile Write(string path)
        {
            return new SharedFile(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
    }
}
