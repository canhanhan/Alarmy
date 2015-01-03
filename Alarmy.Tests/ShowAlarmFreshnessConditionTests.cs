using Alarmy.Common;
using Alarmy.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alarmy.Tests
{
    [TestClass]
    public class ShowAlarmFreshnessConditionTests
    {
        private const int SAMPLE_FRESHNESS = 1;
        private readonly static DateTime SAMPLE_DATETIME = new DateTime(2014, 10, 10, 13, 05, 0);

        private class TestSettings : Settings
        {
            public TestSettings()
            {
                this.Freshness = SAMPLE_FRESHNESS;
            }
        }

        private IDateTimeProvider dateTimeProvider;
        private Alarm alarm;
        private ShowAlarmFreshnessCondition condition;

        [TestInitialize]
        public void Setup()
        {
            var settings = new TestSettings();

            this.dateTimeProvider = Substitute.For<IDateTimeProvider>();
            this.condition = new ShowAlarmFreshnessCondition(this.dateTimeProvider, settings);
            this.alarm = new Alarm(this.dateTimeProvider);
        }

        [TestMethod]
        public void IsWorthShowing_WhenSetAndNotFresh_ReturnsTrue()
        {
            this.TestIsWorthShowing(AlarmStatus.Set, isFresh: false, expectSuccess: true);
        }

        [TestMethod]
        public void IsWorthShowing_WhenMissedAndNotFresh_ReturnsTrue()
        {
            this.TestIsWorthShowing(AlarmStatus.Missed, isFresh: false, expectSuccess: true);
        }

        [TestMethod]
        public void IsWorthShowing_WhenRingingAndNotFresh_ReturnsTrue()
        {
            this.TestIsWorthShowing(AlarmStatus.Ringing, isFresh: false, expectSuccess: true);
        }

        [TestMethod]
        public void IsWorthShowing_WhenCompletedAndNotFresh_ReturnsFalse()
        {
            this.TestIsWorthShowing(AlarmStatus.Completed, isFresh: false, expectSuccess: false);
        }

        [TestMethod]
        public void IsWorthShowing_WhenCancelledAndNotFresh_ReturnsFalse()
        {
            this.TestIsWorthShowing(AlarmStatus.Canceled, isFresh: false, expectSuccess: false);
        }

        [TestMethod]
        public void IsWorthShowing_WhenSetAndFresh_ReturnsTrue()
        {
            this.TestIsWorthShowing(AlarmStatus.Set, isFresh: true, expectSuccess: true);
        }

        [TestMethod]
        public void IsWorthShowing_WhenMissedAndFresh_ReturnsTrue()
        {
            this.TestIsWorthShowing(AlarmStatus.Missed, isFresh: true, expectSuccess: true);
        }

        [TestMethod]
        public void IsWorthShowing_WhenRingingAndFresh_ReturnsTrue()
        {
            this.TestIsWorthShowing(AlarmStatus.Ringing, isFresh: true, expectSuccess: true);
        }

        [TestMethod]
        public void IsWorthShowing_WhenCompletedAndFresh_ReturnsTrue()
        {
            this.TestIsWorthShowing(AlarmStatus.Completed, isFresh: true, expectSuccess: true);
        }

        [TestMethod]
        public void IsWorthShowing_WhenCancelledAndFresh_ReturnsTrue()
        {
            this.TestIsWorthShowing(AlarmStatus.Canceled, isFresh: true, expectSuccess: true);
        }

        private void TestIsWorthShowing(AlarmStatus status, bool isFresh, bool expectSuccess)
        {
            dateTimeProvider.Now.Returns(SAMPLE_DATETIME);
            alarm.SetTime(isFresh ? SAMPLE_DATETIME : SAMPLE_DATETIME.AddMinutes(-(SAMPLE_FRESHNESS + 1)));
            alarm.SetStatusTest(status);

            Assert.AreEqual(expectSuccess, this.condition.Match(alarm));
        }
    }
}
