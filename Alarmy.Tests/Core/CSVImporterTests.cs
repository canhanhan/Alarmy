using Alarmy.Common;
using Alarmy.Core;
using Alarmy.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Tests.Core
{
    [TestClass]
    public class CSVImporterTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var service = Substitute.For<FakeAlarmService>();
            var importer = new CSVImporter(service);

            importer.Import(new ImportContext
            {
                Path = @"C:\temp\wake_up_calls21772188.txt",
                DeleteExisting = true
            });

            IList<IAlarm> alarms = null;
            service.WhenForAnyArgs(x => x.Import(null, false)).Do(x => alarms = (IList<IAlarm>)x.Args()[0]);
            Assert.AreEqual(21, alarms.Count);
        }
    }
}
