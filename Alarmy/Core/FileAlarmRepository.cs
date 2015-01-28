using Alarmy.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Alarmy.Core
{
    internal class FileAlarmRepository : IAlarmRepository
    {
        private readonly string path;
        private readonly JsonSerializer serializer;
        private readonly IRepositoryFilter[] filters;
        private readonly ISharedFileFactory sharedFileFactory;

        private string lastHash;
        private Dictionary<Guid, IAlarm> alarmsCache;
        
        public FileAlarmRepository(ISharedFileFactory sharedFileFactory, IRepositoryFilter[] filters, string path)
        {
            if (sharedFileFactory == null)
                throw new ArgumentNullException("sharedFileFactory");

            if (filters == null)
                throw new ArgumentNullException("filters");

            this.sharedFileFactory = sharedFileFactory;
            this.filters = filters;
            this.path = path;
            this.serializer = JsonSerializer.Create();
            this.serializer.TypeNameHandling = TypeNameHandling.All;
        }

        public IEnumerable<IAlarm> List()
        {
            using (var sharedFile = this.sharedFileFactory.Read(this.path))
            {
                return this.GetAlarms(sharedFile.Stream).Values;
            }
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

        private Dictionary<Guid, IAlarm> GetAlarms(Stream file)
        {
            if (FileChanged(file))
            {
                using (var reader = new JsonTextReader(new StreamReader(file)))
                {
                    alarmsCache = this.Filter(serializer.Deserialize<Dictionary<Guid, IAlarm>>(reader));
                }
            }
             
            return alarmsCache;            
        }

        private bool FileChanged(Stream file)
        {   
            using (var md5 = MD5.Create())
            {
                var hash = BitConverter.ToString(md5.ComputeHash(file));
                var result = this.lastHash == null || hash != this.lastHash;
                this.lastHash = hash;

                file.Position = 0;
                return result;
            }
        }

        private void Modify(Action<Dictionary<Guid, IAlarm>> operation)
        {
            using (var file = this.sharedFileFactory.Write(path))
            {
                GetAlarms(file.Stream);
                file.Stream.Position = 0;
                operation.Invoke(alarmsCache);
                Write(file.Stream);
            }
        }

        private void Write(Stream file)
        {
            using (var writer = new JsonTextWriter(new StreamWriter(file)))
            {
                var alarmsToWrite = alarmsCache.Values.Where(this.PassesAllFilters).ToDictionary(x => x.Id);
                serializer.Serialize(writer, alarmsToWrite);
            }
        }

        private Dictionary<Guid, IAlarm> Filter(Dictionary<Guid, IAlarm> alarms)
        {
            if (alarms == null)
                return new Dictionary<Guid, IAlarm>();

            return alarms.Values.Where(this.PassesAllFilters).ToDictionary(x => x.Id);
        }

        private bool PassesAllFilters(IAlarm alarm)
        {
            return this.filters.All(x => x.Match(alarm));
        }
    }
}
