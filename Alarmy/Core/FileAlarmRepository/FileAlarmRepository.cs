using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.IO;
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

        private IDictionary<Guid, IAlarm> alarmsCache;

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

        public IEnumerable<IAlarm> List()
        {
            return this.alarmsCache.Values;
        }

        public void Add(IAlarm alarm)
        {
            this.Modify(x => x[alarm.Id] = alarm);
        }

        public void Remove(IAlarm alarm)
        {
            this.Modify(x => x.Remove(alarm.Id));
        }

        public void Update(IAlarm alarm)
        {
            this.Add(alarm);
        }

        private void GetAlarms(Stream stream, bool filter = true)
        {
            this.alarmsCache = this.serializer.Deserialize(stream) ?? new Dictionary<Guid, IAlarm>();

            if (filter)
                this.FilterAlarms();
        }

        private void Modify(Action<IDictionary<Guid, IAlarm>> operation)
        {
            using (var file = this.sharedFileFactory.Write(path))
            {
                this.GetAlarms(file.Stream, filter: false);
                file.Stream.Position = 0;

                operation.Invoke(this.alarmsCache);

                this.serializer.Serialize(this.alarmsCache, file.Stream);
            }
        }

        private void FilterAlarms()
        {
            foreach(var keyValuePair in this.alarmsCache.Where(x => !this.PassesAllFilters(x.Value)).ToArray())
            {
                this.alarmsCache.Remove(keyValuePair.Key);
            }
        }

        private bool PassesAllFilters(IAlarm alarm)
        {
            return this.filters.All(x => x.Match(alarm));
        }

        private void watcher_FileChanged(object sender, EventArgs e)
        {
            using (var sharedFile = this.sharedFileFactory.Read(this.path))
            {
                this.GetAlarms(sharedFile.Stream);
            }
        }
    }
}
