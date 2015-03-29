using Alarmy.Common;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Alarmy.Core
{
    internal class OperaWakeupReportMap : CsvClassMap<Alarm>
    {
        public OperaWakeupReportMap(string dateTimeFormat, string captionFormat, string[] captionPatterns)
        {
            Map(x => x.Title).ConvertUsing(row => string.Format(captionFormat, captionPatterns.Select(pattern => GetMatchingString(row.CurrentRecord, pattern)).ToArray()));
            Map(x => x.Time).ConvertUsing(row => GetDateTimeFromStrings(row.CurrentRecord, dateTimeFormat));            
        }
        
        private static string GetMatchingString(string[] values, string pattern)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            if (pattern == null)
                throw new ArgumentNullException("pattern");

            var regex = new Regex(pattern);
            return values.Where(x => !string.IsNullOrWhiteSpace(x)).FirstOrDefault(x => regex.IsMatch(x));
        }

        private static DateTime GetDateTimeFromStrings(string[] values, string format) 
        {
            if (values == null)
                throw new ArgumentNullException("values");

            if (format == null)
                throw new ArgumentNullException("format");

           var date = GetMatchingDate(values, format); //Try to get date in one-shot
           if (string.IsNullOrEmpty(date))  // If not, then try to split the format
           {
               var lastSpace = format.LastIndexOf(' ');
               var datePart = format.Substring(0, lastSpace);
               var timePart = format.Substring(lastSpace+1);

               date =string.Format("{0} {1}", GetMatchingDate(values, datePart), GetMatchingDate(values, timePart));               
           }
            
            return string.IsNullOrWhiteSpace(date) ? DateTime.MinValue : DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
        }

        private static string GetMatchingDate(string[] values, string format)
        {
            DateTime date;
            foreach (var value in values)
            {
                if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal & DateTimeStyles.AllowWhiteSpaces, out date))
                {
                    return value;
                }
            }

            return null;
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
            {
                this.Import(context, textReader);
            }
        }

        internal void Import(ImportContext context, TextReader textReader)
        {
            using (var csvReader = new CsvReader(textReader))
            {
                var map = new OperaWakeupReportMap(context.DateFormat, context.CaptionFormat, context.CaptionPatterns);
                csvReader.Configuration.RegisterClassMap(map);
                csvReader.Configuration.Delimiter = context.Delimiter;
                csvReader.Configuration.HasHeaderRecord = context.HasHeaders;

                this.alarmService.Import(csvReader.GetRecords<Alarm>().Cast<IAlarm>().Where(x => x.Time != DateTime.MinValue), context.DeleteExisting);
            }
        }
    }
}
