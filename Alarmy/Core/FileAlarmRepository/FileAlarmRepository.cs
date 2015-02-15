using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Core.FileAlarmRepository
{
    internal class FileAlarmRepository : IAlarmRepository, ISupportsStartStop
    {
        private readonly string path;
        private readonly IRepositoryFilter[] filters;
        private readonly ISharedFileFactory sharedFileFactory;
        private readonly IFileWatcher watcher;
        private readonly IRepositorySerializer serializer;
        
        private bool isChanged;

        public FileAlarmRepository(IFileWatcher watcher, ISharedFileFactory sharedFileFactory, IRepositorySerializer serializer, IRepositoryFilter[] filters, string path)
        {
            if (watcher == null)
                throw new ArgumentNullException("watcher");

            if (sharedFileFactory == null)
                throw new ArgumentNullException("sharedFileFactory");

            if (filters == null)
                throw new ArgumentNullException("filters");

            if (serializer == null)
                throw new ArgumentNullException("serializer");

            this.serializer = serializer;

            this.sharedFileFactory = sharedFileFactory;
            this.filters = filters;
            this.path = path;

            this.watcher = watcher;
            this.watcher.FileChanged += watcher_FileChanged;           
        }

        public void Start()
        {
            this.watcher.Start();
            this.watcher_FileChanged(null, null);
        }

        public void Stop()
        {
            this.watcher.Stop();
        }

        public IEnumerable<IAlarm> Load()
        {
            if (!this.isChanged)
                return null;

            using (var sharedFile = this.sharedFileFactory.Read(this.path))
            {
                this.isChanged = false;
                return this.Filter(this.serializer.Deserialize(sharedFile.Stream));
            }            
        }

        public void Save(IEnumerable<IAlarm> alarms)
        {
            using (var file = this.sharedFileFactory.Write(path))
            {
                this.serializer.Serialize(alarms.ToArray(), file.Stream);
            }
        }
                
        private void watcher_FileChanged(object sender, EventArgs e)
        {
            this.isChanged = true;
        }

        private IEnumerable<IAlarm> Filter(IEnumerable<IAlarm> alarms)
        {
            if (alarms == null)
                return new IAlarm[0];

            return alarms.Where(alarm => this.filters.All(filter => filter.Match(alarm)));
        }
    }
}
