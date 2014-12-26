using Alarmy.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace Alarmy.Services
{
    public class FileAlarmRepository : IAlarmRepository
    {
        private class SharedFile : IDisposable
        {
            public static SharedFile Read(string path)
            {
                return new SharedFile(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            public static SharedFile Write(string path)
            {
                return new SharedFile(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }

            public static implicit operator FileStream(SharedFile file)
            {
                return file.File;
            }

            private readonly Random random = new Random();      

            private FileStream file;

            public FileStream File
            {
                get
                {
                    return this.file;
                }
            }

            private SharedFile(string path, FileMode mode, FileAccess access, FileShare share)
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

        private readonly string path;
        private string lastHash;
        private Dictionary<Guid, IAlarm> alarmsCache;
        private readonly JsonSerializerSettings settings;
        private readonly JsonSerializer serializer;

        public FileAlarmRepository(string path)
        {
            this.path = path;
            settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            serializer = JsonSerializer.Create(settings);

            if (!File.Exists(path))
            {
                using (File.Create(path))
                {
                    Console.WriteLine("Creating the file " + path);
                }
            }
        }

        public IEnumerable<IAlarm> List()
        {
            using (var sharedFile = SharedFile.Read(this.path))
            {
                return GetAlarms(sharedFile).Values;
            }
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
            Add(alarm);
        }

        private Dictionary<Guid, IAlarm> GetAlarms(FileStream file)
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
                        alarmsCache = GetWorthShowing(serializer.Deserialize<Dictionary<Guid, IAlarm>>(reader) ?? new Dictionary<Guid, IAlarm>());
                        lastHash = hash;
                    }
                }
            }               
            return alarmsCache;            
        }

        private void Modify(Action<Dictionary<Guid, IAlarm>> operation)
        {
            using (var file = SharedFile.Write(path))
            {
                GetAlarms(file);
                file.File.Position = 0;
                operation.Invoke(alarmsCache);
                Write(file);
            }
        }

        private void Write(FileStream file)
        {
            using (var streamWriter = new StreamWriter(file))
            {
                using (var writer = new JsonTextWriter(streamWriter))
                {
                    var alarmsToWrite = alarmsCache.Values.Where(x => x.IsWorthShowing).ToDictionary(x => x.Id);
                    serializer.Serialize(writer, alarmsToWrite);
                }
            }
        }

        private static Dictionary<Guid, IAlarm> GetWorthShowing(Dictionary<Guid, IAlarm> alarms)
        {
            return alarms.Values.Where(x => x.IsWorthShowing).ToDictionary(x => x.Id);
        }
    }
}
