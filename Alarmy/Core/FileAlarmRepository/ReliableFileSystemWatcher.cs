using Alarmy.Common;
using System;
using System.IO;

namespace Alarmy.Core.FileAlarmRepository
{
    internal class ReliableFileSystemWatcher : IFileWatcher
    {
        private FileSystemWatcher fileSystemWatcher;
        private ITimer timer;

        public event EventHandler FileChanged;

        public ReliableFileSystemWatcher(ITimer timer, string path)
        {
            var parentPath = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);

            this.fileSystemWatcher = new FileSystemWatcher(parentPath, file);
            this.fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            this.fileSystemWatcher.Changed += fileSystemWatcher_Changed;

            this.timer = timer;
            this.timer.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;
            this.timer.Elapsed += timer_Elapsed;
        }

        public void Start()
        {
            this.timer.Start();
            this.fileSystemWatcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            this.timer.Stop();
            this.fileSystemWatcher.EnableRaisingEvents = false;
        }

        public void Dispose()
        {
            if (this.fileSystemWatcher != null)
            {
                this.fileSystemWatcher.Dispose();
                this.fileSystemWatcher = null;
            }

            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
        }

        private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.timer.Reset();
            this.InvokeFileChanged();
        }

        private void timer_Elapsed(object sender, EventArgs e)
        {
            this.InvokeFileChanged();
        }

        private void InvokeFileChanged()
        {
            if (this.FileChanged != null)
                this.FileChanged.Invoke(null, null);
        }
    }
}
