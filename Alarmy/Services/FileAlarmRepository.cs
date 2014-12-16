using Alarmy.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace Alarmy
{
    public class FileAlarmRepository : IAlarmRepository
    {
        private readonly string path;
        private readonly Random random = new Random();
        private readonly JsonSerializerSettings settings;
        private readonly JsonSerializer serializer;

        private string lastHash;
        private Dictionary<Guid, IAlarm> alarmsCache;

        public FileAlarmRepository(string path)
        {
            this.path = path;
            this.settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            this.serializer = JsonSerializer.Create(this.settings);

            if (!File.Exists(path))
                using (File.Create(path))
                    Console.WriteLine("Creating the file " + path);
        }

        public IEnumerable<IAlarm> List()
        {
            return GetAlarms().Values;
        }

        public void Add(IAlarm alarm)
        {
            Modify(x => x[alarm.Id] = alarm);
        }

        public void Remove(IAlarm alarm)
        {
            Modify(x => x.Remove(alarm.Id));
        }

        public void Update(IAlarm alarm)
        {
            this.Add(alarm);
        }

        private Dictionary<Guid, IAlarm> GetAlarms()
        {
            lock (this.path)
            {
                this.WaitAndOpenFile(this.path, FileMode.Open, FileAccess.Read, FileShare.Read, file =>
                {
                    using (var md5 = MD5.Create())
                    {
                        var hash = BitConverter.ToString(md5.ComputeHash(file));
                        if (lastHash == null || hash != lastHash)
                        {
                            file.Position = 0;
                            using (var streamReader = new StreamReader(file))
                            using (var reader = new JsonTextReader(streamReader))
                            {
                                alarmsCache = this.serializer.Deserialize<Dictionary<Guid, IAlarm>>(reader) ?? new Dictionary<Guid, IAlarm>();
                                lastHash = hash;
                            }
                        }
                    }
                });

                return alarmsCache;
            }
        }

        private void Modify(Action<Dictionary<Guid, IAlarm>> operation)
        {
            lock (this.path)
            {
                GetAlarms();
                operation.Invoke(alarmsCache);
                Write();
            }

        }

        private void Write()
        {
            this.WaitAndOpenFile(this.path, FileMode.Open, FileAccess.Write, FileShare.Read, file =>
            {
                using (var streamWriter = new StreamWriter(file))
                using (var writer = new JsonTextWriter(streamWriter))
                {
                    var alarmsToWrite = alarmsCache.Values.Where(x => x.IsWorthShowing).ToDictionary(x => x.Id);
                    this.serializer.Serialize(writer, alarmsToWrite);
                }
            });
        }
    
        private void WaitAndOpenFile(string path, FileMode mode, FileAccess access, FileShare share, Action<FileStream> operation)
        {
            while (true)
            {
                try
                {
                    using (var file = File.Open(path, mode, access, share)) 
                    {    
                        operation.Invoke(file);
                        return;                            
                    }
                }
                catch (IOException ex)
                {
                    if (Marshal.GetHRForException(ex) != -2147024864)
                        throw;

                    // Sleep randomly between 400ms to 500ms
                    Thread.Sleep(400 + (random.Next(10)*10));
                }
            }
        }
    }
}
