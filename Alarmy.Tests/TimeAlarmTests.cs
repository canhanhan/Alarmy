using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alarmy.Common;
using NSubstitute;

namespace Alarmy.Tests
{
    [TestClass]
    public class TimeAlarmTests
    {
        private readonly static DateTime SAMPLE_DATETIME = new DateTime(2014, 10, 10, 13, 05, 0);
        private IDateTimeProvider dateTimeProvider;
        private TimeAlarm alarm;

        [TestInitialize]
        public void Setup()
        {
            this.dateTimeProvider = Substitute.For<IDateTimeProvider>();
            this.alarm = new TimeAlarm(this.dateTimeProvider);
        }

        [TestMethod]
        public void Time_WhenSet_ChangesRingTime()
        {
            this.alarm.Set(SAMPLE_DATETIME);

            Assert.AreEqual(SAMPLE_DATETIME, this.alarm.Time);
        }

        [TestMethod]
        public void Time_WhenSet_ChangesAlarmStatus()
        {
            this.alarm.Status = AlarmStatus.Ringing;
            this.alarm.Set(SAMPLE_DATETIME);

            Assert.AreEqual(AlarmStatus.Set, this.alarm.Status);
        }

        [TestMethod]
        public void Snooze_WhenCalled_IncreasesRingTime()
        {
            this.alarm.Set(SAMPLE_DATETIME);
            this.alarm.Snooze();

            Assert.AreEqual(SAMPLE_DATETIME.AddMinutes(TimeAlarm.SNOOZE_INTERVAL), this.alarm.Time);
        }

        [TestMethod]
        public void Snooze_WhenCalled_ChangesAlarmStatus()
        {
            this.alarm.Set(SAMPLE_DATETIME);
            this.alarm.Status = AlarmStatus.Ringing;
            this.alarm.Snooze();

            Assert.AreEqual(AlarmStatus.Set, this.alarm.Status);
        }

        [TestMethod]
        public void Check_BeforeAlarm_DoesNothing()
        {
            this.alarm.Set(SAMPLE_DATETIME);
            this.dateTimeProvider.NowRoundedToCurrentMinute.Returns(SAMPLE_DATETIME.AddMinutes(5));

            this.alarm.Check();

            Assert.AreEqual(AlarmStatus.Set, this.alarm.Status);
        }

        [TestMethod]
        public void Check_DuringAlarm_SetsStatusToRinging()
        {
            this.alarm.Set(SAMPLE_DATETIME);
            this.dateTimeProvider.NowRoundedToCurrentMinute.Returns(SAMPLE_DATETIME);

            this.alarm.Check();

            Assert.AreEqual(AlarmStatus.Ringing, this.alarm.Status);
        }

        [TestMethod]
        public void Check_AfterAlarm_SetsStatusToMissed()
        {
            this.alarm.Set(SAMPLE_DATETIME);
            this.dateTimeProvider.NowRoundedToCurrentMinute.Returns(SAMPLE_DATETIME.AddMinutes(-5));

            this.alarm.Check();

            Assert.AreEqual(AlarmStatus.Missed, this.alarm.Status);
        }

        [TestMethod]
        public void Check_WhenCompleted_DoesNothing()
        {
            this.alarm.Set(SAMPLE_DATETIME);
            this.dateTimeProvider.NowRoundedToCurrentMinute.Returns(SAMPLE_DATETIME.AddMinutes(-5));
            this.alarm.Status = AlarmStatus.Completed;
            
            this.alarm.Check();

            Assert.AreEqual(AlarmStatus.Completed, this.alarm.Status);
        }
    }
}
