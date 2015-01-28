using Alarmy.Common;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Alarmy.Infrastructure
{
    internal class SharedFile : ISharedFile
    {
        private readonly Random random = new Random();

        private Stream file;

        public Stream Stream
        {
            get
            {
                return this.file;
            }
        }

        internal SharedFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            this.file = WaitAndOpenFile(path, mode, access, share);
        }

        public void Dispose()
        {
            if (this.file != null)
            {
                this.file.Dispose();
                this.file = null;
            }
        }

        private FileStream WaitAndOpenFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            while (true)
            {
                try
                {
                    return System.IO.File.Open(path, mode, access, share);
                }
                catch (IOException ex)
                {
                    //If not file sharing exception then rethrow
                    if (Marshal.GetHRForException(ex) != -2147024864)
                    {
                        throw;
                    }

                    Thread.Sleep(400 + (random.Next(10) * 10));
                }
            }
        }
    }
}
