using Alarmy.Common;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Alarmy.Core
{
    internal class OperaWakeupReportMap : CsvClassMap<Alarm>
    {
        public OperaWakeupReportMap(string format)
        {            
            Map(x => x.Title).ConvertUsing(row => string.Format("{1} (#{0})", row.GetField(0), row.GetField(1)));
            Map(x => x.Time).ConvertUsing(row => DateTime.ParseExact(string.Format("{0} {1}", row.GetField(2), row.GetField(3)), format, CultureInfo.InvariantCulture));
        }
    }

    internal class CSVImporter : IImporter
    {
        private readonly IAlarmService alarmService;

        public CSVImporter(IAlarmService alarmService)
        {
            if (alarmService == null)
                throw new ArgumentNullException("alarmService");

            this.alarmService = alarmService;
        }

        public void Import(ImportContext context)        
        {
            using (var textReader = new StreamReader(File.OpenRead(context.Path)))
            using (var csvReader = new CsvReader(textReader))
            {
                var map = new OperaWakeupReportMap(context.DateFormat);
                csvReader.Configuration.RegisterClassMap(map);
                csvReader.Configuration.Delimiter = "\t";
                csvReader.Configuration.HasHeaderRecord = false;

                this.alarmService.Import(csvReader.GetRecords<Alarm>(), context.DeleteExisting);
            }
        }
    }
}
