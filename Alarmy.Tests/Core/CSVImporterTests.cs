using Alarmy.Common;
using Alarmy.Core;
using Alarmy.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Alarmy.Tests.Core
{
    [TestClass]
    public class CSVImporterTests
    {
        private FakeAlarmService service;
        private CSVImporter importer;
        private ImportContext importContext;

        [TestInitialize]
        public void Setup()
        {
            this.service = Substitute.For<FakeAlarmService>();
            this.importer = new CSVImporter(this.service);
            this.importContext = new ImportContext {
                CaptionFormat = "{0}",
                CaptionPatterns = new [] { "\\W+" },
                DateFormat = "dd.MM.yyyy HH:mm"
            };
        }

        private void Import(string content)
        {
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            streamWriter.Write(content);
            streamWriter.Flush();
            stream.Position = 0;
            var streamReader = new StreamReader(stream);
            importer.Import(importContext, streamReader);
        }

        [TestMethod]
        public void CSVImporter_Import_WhenNoAlarmsInFile_DoesNotAddAnyAlarms()
        {
            IList<IAlarm> alarms = null;
            service.WhenForAnyArgs(x => x.Import(null, false)).Do(x => alarms = ((IEnumerable<IAlarm>)x.Args()[0]).ToList());

            this.Import("Caption\tDate\r\nFooter");

            Assert.AreEqual(0, alarms.Count);
        }

        [TestMethod]
        public void CSVImporter_Import_WhenOneAlarmInFile_AddsAlarm()
        {
            IList<IAlarm> alarms = null;
            service.WhenForAnyArgs(x => x.Import(null, false)).Do(x => alarms = ((IEnumerable<IAlarm>)x.Args()[0]).ToList());

            this.Import("Caption\tDate\r\nTest Caption\t10.10.1987 10:00\r\nFooter");

            Assert.AreEqual(1, alarms.Count);
            Assert.AreEqual("Test Caption", alarms[0].Title);
            Assert.AreEqual(new DateTime(1987, 10, 10, 10, 0, 0), alarms[0].Time);
        }

        [TestMethod]
        public void CSVImporter_Import_WhenDateTimeSeperated_MergesDateAndTime()
        {
            IList<IAlarm> alarms = null;
            service.WhenForAnyArgs(x => x.Import(null, false)).Do(x => alarms = ((IEnumerable<IAlarm>)x.Args()[0]).ToList());

            this.Import("Caption\tDate\tTime\r\nTest Caption\t10.10.1987\t10:00\r\nFooter");

            Assert.AreEqual(1, alarms.Count);
            Assert.AreEqual("Test Caption", alarms[0].Title);
            Assert.AreEqual(new DateTime(1987, 10, 10, 10, 0, 0), alarms[0].Time);
        }

        [TestMethod]
        public void CSVImporter_Import_WhenTimeIsBeforeDate_MergesDateAndTime()
        {
            IList<IAlarm> alarms = null;
            service.WhenForAnyArgs(x => x.Import(null, false)).Do(x => alarms = ((IEnumerable<IAlarm>)x.Args()[0]).ToList());

            this.Import("Caption\tTime\tDate\r\nTest Caption\t10:00\t10.10.1987\r\nFooter");

            Assert.AreEqual(1, alarms.Count);
            Assert.AreEqual("Test Caption", alarms[0].Title);
            Assert.AreEqual(new DateTime(1987, 10, 10, 10, 0, 0), alarms[0].Time);
        }

        [TestMethod]
        public void CSVImporter_Import_WhenMultipleAlarms_AddsAll()
        {
            IList<IAlarm> alarms = null;
            service.WhenForAnyArgs(x => x.Import(null, false)).Do(x => alarms = ((IEnumerable<IAlarm>)x.Args()[0]).ToList());

            this.Import("Caption\tTime\tDate\r\nTest Caption\t10:00\t10.10.1987\r\nTest Caption 2\t11:00\t11.10.1987\r\nFooter");

            Assert.AreEqual(2, alarms.Count);
            Assert.AreEqual("Test Caption", alarms[0].Title);
            Assert.AreEqual(new DateTime(1987, 10, 10, 10, 0, 0), alarms[0].Time);
            Assert.AreEqual("Test Caption 2", alarms[1].Title);
            Assert.AreEqual(new DateTime(1987, 10, 11, 11, 0, 0), alarms[1].Time);
        }

        [TestMethod]
        public void CSVImporter_Import_WhenMultipleCaptionFields_MergesAll()
        {
            importContext.CaptionFormat = "{0} - {1}";
            importContext.CaptionPatterns = new[] { "\\w+", "^\\w{4}$" };
            IList<IAlarm> alarms = null;
            service.WhenForAnyArgs(x => x.Import(null, false)).Do(x => alarms = ((IEnumerable<IAlarm>)x.Args()[0]).ToList());

            this.Import("Caption\tCaption2\tTime\tDate\r\nTest Caption\tTest\t10:00\t10.10.1987\r\nFooter");

            Assert.AreEqual(1, alarms.Count);
            Assert.AreEqual("Test Caption - Test", alarms[0].Title);
            Assert.AreEqual(new DateTime(1987, 10, 10, 10, 0, 0), alarms[0].Time);
        }
    }
}
