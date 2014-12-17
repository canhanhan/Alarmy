using Alarmy.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;

namespace Alarmy.Tests
{
    [TestClass]
    public class AlarmTests
    {
        public const string ARBITRARY_TEST_REASON = "TEST";
        private readonly static DateTime SAMPLE_DATETIME = new DateTime(2014, 10, 10, 13, 05, 0);
        private IDateTimeProvider dateTimeProvider;
        private Alarm alarm;

        [TestInitialize]
        public void Setup()
        {
            this.dateTimeProvider = Substitute.For<IDateTimeProvider>();
            this.alarm = new Alarm(this.dateTimeProvider);
        }

        #region Set status tests
        [TestMethod]
        public void Alarm_WhenSet_CanBeCancelled()
        {
            alarm.SetStatusTest(AlarmStatus.Set);
            
            alarm.Cancel(ARBITRARY_TEST_REASON);

            Assert.AreEqual(AlarmStatus.Cancelled, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenSet_CanBeRinging()
        {
            alarm.SetStatusTest(AlarmStatus.Set);

            alarm.SetStatus(AlarmStatus.Ringing);

            Assert.AreEqual(AlarmStatus.Ringing, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenSet_CannotBeCompleted()
        {
            alarm.SetStatusTest(AlarmStatus.Set);

            alarm.SetStatus(AlarmStatus.Completed);
        }

        [TestMethod]
        public void Alarm_WhenSet_CanBeMissed()
        {
            alarm.SetStatusTest(AlarmStatus.Set);

            alarm.SetStatus(AlarmStatus.Missed);
            Assert.AreEqual(AlarmStatus.Missed, alarm.Status);
        }        
        #endregion

        #region Ringing status tests
        [TestMethod]
        public void Alarm_WhenRinging_CanBeCompleted()
        {
            alarm.SetStatusTest(AlarmStatus.Ringing);

            alarm.SetStatus(AlarmStatus.Completed);
            Assert.AreEqual(AlarmStatus.Completed, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenRinging_CanBeMissed()
        {
            alarm.SetStatusTest(AlarmStatus.Ringing);

            alarm.SetStatus(AlarmStatus.Missed);
            Assert.AreEqual(AlarmStatus.Missed, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenRinging_CanBeCancelled()
        {
            alarm.SetStatusTest(AlarmStatus.Ringing);

            alarm.SetStatus(AlarmStatus.Cancelled);
            Assert.AreEqual(AlarmStatus.Cancelled, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenRinging_CanBeSet()
        {
            alarm.SetStatusTest(AlarmStatus.Ringing);

            alarm.SetStatus(AlarmStatus.Set);
            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }
        #endregion

        #region Cancelled state tests
        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCancelled_CannotBeCompleted()
        {
            alarm.SetStatusTest(AlarmStatus.Cancelled);

            alarm.SetStatus(AlarmStatus.Completed);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCancelled_CannotBeRinging()
        {
            alarm.SetStatusTest(AlarmStatus.Cancelled);

            alarm.SetStatus(AlarmStatus.Ringing);
        }

        [TestMethod]
        public void Alarm_WhenCancelled_CanBeSet()
        {
            alarm.SetStatusTest(AlarmStatus.Cancelled);

            alarm.SetStatus(AlarmStatus.Set);

            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCancelled_CannotBeMissed()
        {
            alarm.SetStatusTest(AlarmStatus.Cancelled);

            alarm.SetStatus(AlarmStatus.Missed);
        }
        #endregion

        #region Completed state tests
        [TestMethod]
        public void Alarm_WhenCompleted_CanBeSet()
        {
            alarm.SetStatusTest(AlarmStatus.Completed);

            alarm.SetStatus(AlarmStatus.Set);

            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCompleted_CannotBeRinging()
        {
            alarm.SetStatusTest(AlarmStatus.Completed);

            alarm.SetStatus(AlarmStatus.Ringing);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCompleted_CannotBeCancelled()
        {
            alarm.SetStatusTest(AlarmStatus.Completed);

            alarm.SetStatus(AlarmStatus.Cancelled);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCompleted_CannotBeMissed()
        {
            alarm.SetStatusTest(AlarmStatus.Completed);

            alarm.SetStatus(AlarmStatus.Missed);
        }
        #endregion

        #region Missed status tests
        [TestMethod]
        public void Alarm_WhenMissed_CanBeCompleted()
        {
            alarm.SetStatusTest(AlarmStatus.Missed);

            alarm.SetStatus(AlarmStatus.Completed);
            Assert.AreEqual(AlarmStatus.Completed, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenMissed_CanBeCancelled()
        {
            alarm.SetStatusTest(AlarmStatus.Missed);

            alarm.SetStatus(AlarmStatus.Cancelled);
            Assert.AreEqual(AlarmStatus.Cancelled, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenMissed_CanBeSet()
        {
            alarm.SetStatusTest(AlarmStatus.Missed);

            alarm.SetStatus(AlarmStatus.Set);
            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenMissed_CannotBeRinging()
        {
            alarm.SetStatusTest(AlarmStatus.Missed);

            alarm.SetStatus(AlarmStatus.Ringing);
        }        
        #endregion

        #region TimeTests
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
        public void Time_WhenSet_ChangesHush()
        {
            this.alarm.IsHushed = true; ;
            this.alarm.Set(SAMPLE_DATETIME);

            Assert.IsFalse(this.alarm.IsHushed);
        }

        #endregion

        #region Snooze tests
        //[TestMethod]
        //public void Snooze_WhenCalled_IncreasesRingTime()
        //{
        //    this.alarm.Set(SAMPLE_DATETIME);
        //    this.alarm.Snooze();

        //    Assert.AreEqual(SAMPLE_DATETIME.AddMinutes(TimeAlarm.SNOOZE_INTERVAL), this.alarm.Time);
        //}

        //[TestMethod]
        //public void Snooze_WhenCalled_ChangesAlarmStatus()
        //{
        //    this.alarm.Set(SAMPLE_DATETIME);
        //    this.alarm.Status = AlarmStatus.Ringing;
        //    this.alarm.Snooze();

        //    Assert.AreEqual(AlarmStatus.Set, this.alarm.Status);
        //}
        #endregion

        #region Check tests
        [TestMethod]
        public void Check_BeforeAlarm_DoesNothing()
        {
            this.alarm.Set(SAMPLE_DATETIME);
            this.dateTimeProvider.Now.Returns(SAMPLE_DATETIME.AddMinutes(-5));

            this.alarm.Check();

            Assert.AreEqual(AlarmStatus.Set, this.alarm.Status);
        }

        [TestMethod]
        public void Check_DuringAlarm_SetsStatusToRinging()
        {
            this.alarm.Set(SAMPLE_DATETIME);
            this.dateTimeProvider.Now.Returns(SAMPLE_DATETIME);

            this.alarm.Check();

            Assert.AreEqual(AlarmStatus.Ringing, this.alarm.Status);
        }

        [TestMethod]
        public void Check_AfterAlarm_SetsStatusToMissed()
        {
            this.alarm.Set(SAMPLE_DATETIME);
            this.dateTimeProvider.Now.Returns(SAMPLE_DATETIME.AddMinutes(5));

            this.alarm.Check();

            Assert.AreEqual(AlarmStatus.Missed, this.alarm.Status);
        }

        [TestMethod]
        public void Check_WhenCompleted_DoesNothing()
        {
            this.alarm.Set(SAMPLE_DATETIME);
            this.dateTimeProvider.Now.Returns(SAMPLE_DATETIME.AddMinutes(-5));
            this.alarm.Status = AlarmStatus.Completed;

            this.alarm.Check();

            Assert.AreEqual(AlarmStatus.Completed, this.alarm.Status);
        }
        #endregion

        #region IsWorthShowingTests
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
            this.TestIsWorthShowing(AlarmStatus.Cancelled, isFresh: false, expectSuccess: false);
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
            this.TestIsWorthShowing(AlarmStatus.Cancelled, isFresh: true, expectSuccess: true);
        }
        #endregion

        private void TestIsWorthShowing(AlarmStatus status, bool isFresh, bool expectSuccess) 
        {
            dateTimeProvider.Now.Returns(SAMPLE_DATETIME);
            alarm.Set(isFresh ? SAMPLE_DATETIME : SAMPLE_DATETIME.AddMinutes(-(Alarm.ISWORTHSHOWING_FRESNESS+1)));
            alarm.SetStatusTest(status);

            Assert.AreEqual(expectSuccess, alarm.IsWorthShowing);
        }
    }
}
