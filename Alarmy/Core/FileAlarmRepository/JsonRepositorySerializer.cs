using Alarmy.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Alarmy.Core.FileAlarmRepository
{
    internal class JsonRepositorySerializer : IRepositorySerializer
    {
        private readonly JsonSerializer serializer;

        public JsonRepositorySerializer()
        {
            this.serializer = JsonSerializer.Create();
            this.serializer.TypeNameHandling = TypeNameHandling.Objects;
        }

        public IEnumerable<IAlarm> Deserialize(Stream stream)
        {
            using (var reader = new JsonTextReader(new StreamReader(stream)))
            {
                reader.CloseInput = false;
                return serializer.Deserialize<IEnumerable<IAlarm>>(reader);
            }
        }

        public void Serialize(IEnumerable<IAlarm> alarms, Stream stream)
        {
            var streamWriter = new StreamWriter(stream);
            using (var writer = new JsonTextWriter(streamWriter))
            {
                writer.CloseOutput = false;
                serializer.Serialize(writer, alarms);
                streamWriter.Flush();
            }
        }
    }
}
