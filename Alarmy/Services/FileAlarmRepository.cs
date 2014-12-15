using Alarmy.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Alarmy
{
    public class FileAlarmRepository : IAlarmRepository
    {
        private readonly string path;
        private string lastHash;
        private List<IAlarm> alarmsCache;

        private readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public FileAlarmRepository(string path)
        {
            this.path = path;

            if (!File.Exists(path))
                using (File.Create(path))
                    Console.WriteLine("Creating the file " + path);
        }

        public IEnumerable<IAlarm> List()
        {
            lock (this.path)
            {
                using (var file = File.Open(this.path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var md5 = MD5.Create())
                {
                    var hash = BitConverter.ToString(md5.ComputeHash(file));
                    if (lastHash == null || hash != lastHash)
                    {
                        file.Position = 0;
                        using (var streamReader = new StreamReader(file))
                        using (var reader = new JsonTextReader(streamReader))
                        {
                            var serializer = JsonSerializer.Create(this.settings);
                            alarmsCache = serializer.Deserialize<List<IAlarm>>(reader) ?? new List<IAlarm>();
                            lastHash = hash;
                        }
                    }

                    return alarmsCache;
                }
            }
        }

        public void Add(IAlarm alarm)
        {
            lock (this.path)
            {
                List<IAlarm> list = this.List() as List<IAlarm>;
                list.Add(alarm);

                Write(list);
            }
        }

        public void Remove(IAlarm alarm)
        {
            lock (this.path)
            {
                List<IAlarm> list = this.List() as List<IAlarm>;
                list.Remove(alarm);

                Write(list);
            }
        }

        private void Write(List<IAlarm> list)
        {
            var serializer = JsonSerializer.Create(this.settings);
            using (var file = File.Open(this.path, FileMode.Open, FileAccess.Write, FileShare.Read))
            {
                using (var streamWriter = new StreamWriter(file))
                using (var writer = new JsonTextWriter(streamWriter))
                {
                    serializer.Serialize(writer, list);
                }
            }
        }
    }
}
