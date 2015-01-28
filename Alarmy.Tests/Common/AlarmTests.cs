using Alarmy.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;

namespace Alarmy.Tests
{
    [TestClass]
    public class AlarmTests
    {
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
            alarm.Status = AlarmStatus.Set;
            
            alarm.Cancel();

            Assert.AreEqual(AlarmStatus.Canceled, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenSet_CanBeRinging()
        {
            alarm.Status = AlarmStatus.Set;

            alarm.SetStatus(AlarmStatus.Ringing);

            Assert.AreEqual(AlarmStatus.Ringing, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenSet_CannotBeCompleted()
        {
            alarm.Status = AlarmStatus.Set;

            alarm.SetStatus(AlarmStatus.Completed);
        }

        [TestMethod]
        public void Alarm_WhenSet_CanBeMissed()
        {
            alarm.Status = AlarmStatus.Set;

            alarm.SetStatus(AlarmStatus.Missed);
            Assert.AreEqual(AlarmStatus.Missed, alarm.Status);
        }        
        #endregion

        #region Ringing status tests
        [TestMethod]
        public void Alarm_WhenRinging_CanBeCompleted()
        {
            alarm.Status = AlarmStatus.Ringing;

            alarm.SetStatus(AlarmStatus.Completed);
            Assert.AreEqual(AlarmStatus.Completed, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenRinging_CanBeMissed()
        {
            alarm.Status = AlarmStatus.Ringing;

            alarm.SetStatus(AlarmStatus.Missed);
            Assert.AreEqual(AlarmStatus.Missed, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenRinging_CanBeCancelled()
        {
            alarm.Status = AlarmStatus.Ringing;

            alarm.SetStatus(AlarmStatus.Canceled);
            Assert.AreEqual(AlarmStatus.Canceled, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenRinging_CanBeSet()
        {
            alarm.Status = AlarmStatus.Ringing;

            alarm.SetStatus(AlarmStatus.Set);
            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }
        #endregion

        #region Cancelled state tests
        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCancelled_CannotBeCompleted()
        {
            alarm.Status = AlarmStatus.Canceled;

            alarm.SetStatus(AlarmStatus.Completed);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCancelled_CannotBeRinging()
        {
            alarm.Status = AlarmStatus.Canceled;

            alarm.SetStatus(AlarmStatus.Ringing);
        }

        [TestMethod]
        public void Alarm_WhenCancelled_CanBeSet()
        {
            alarm.Status = AlarmStatus.Canceled;

            alarm.SetStatus(AlarmStatus.Set);

            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCancelled_CannotBeMissed()
        {
            alarm.Status = AlarmStatus.Canceled;

            alarm.SetStatus(AlarmStatus.Missed);
        }
        #endregion

        #region Completed state tests
        [TestMethod]
        public void Alarm_WhenCompleted_CanBeSet()
        {
            alarm.Status = AlarmStatus.Completed;

            alarm.SetStatus(AlarmStatus.Set);

            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCompleted_CannotBeRinging()
        {
            alarm.Status = AlarmStatus.Completed;

            alarm.SetStatus(AlarmStatus.Ringing);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCompleted_CannotBeCancelled()
        {
            alarm.Status = AlarmStatus.Completed;

            alarm.SetStatus(AlarmStatus.Canceled);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCompleted_CannotBeMissed()
        {
            alarm.Status = AlarmStatus.Completed;

            alarm.SetStatus(AlarmStatus.Missed);
        }
        #endregion

        #region Missed status tests
        [TestMethod]
        public void Alarm_WhenMissed_CanBeCompleted()
        {
            alarm.Status = AlarmStatus.Missed;

            alarm.SetStatus(AlarmStatus.Completed);
            Assert.AreEqual(AlarmStatus.Completed, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenMissed_CanBeCancelled()
        {
            alarm.Status = AlarmStatus.Missed;

            alarm.SetStatus(AlarmStatus.Canceled);
            Assert.AreEqual(AlarmStatus.Canceled, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenMissed_CanBeSet()
        {
            alarm.Status = AlarmStatus.Missed;

            alarm.SetStatus(AlarmStatus.Set);
            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenMissed_CannotBeRinging()
        {
            alarm.Status = AlarmStatus.Missed;

            alarm.SetStatus(AlarmStatus.Ringing);
        }        
        #endregion

        #region TimeTests
        [TestMethod]
        public void Time_WhenSet_ChangesRingTime()
        {
            this.alarm.SetTime(SAMPLE_DATETIME);

            Assert.AreEqual(SAMPLE_DATETIME, this.alarm.Time);
        }

        [TestMethod]
        public void Time_WhenSet_ChangesAlarmStatus()
        {
            this.alarm.Status = AlarmStatus.Ringing;
            this.alarm.SetTime(SAMPLE_DATETIME);

            Assert.AreEqual(AlarmStatus.Set, this.alarm.Status);
        }

        [TestMethod]
        public void Time_WhenSet_ChangesHush()
        {
            this.alarm.IsHushed = true; ;
            this.alarm.SetTime(SAMPLE_DATETIME);

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
            this.alarm.SetTime(SAMPLE_DATETIME);
            this.dateTimeProvider.Now.Returns(SAMPLE_DATETIME.AddMinutes(-5));

            this.alarm.CheckStatusChange();

            Assert.AreEqual(AlarmStatus.Set, this.alarm.Status);
        }

        [TestMethod]
        public void Check_DuringAlarm_SetsStatusToRinging()
        {
            this.alarm.SetTime(SAMPLE_DATETIME);
            this.dateTimeProvider.Now.Returns(SAMPLE_DATETIME);

            this.alarm.CheckStatusChange();

            Assert.AreEqual(AlarmStatus.Ringing, this.alarm.Status);
        }

        [TestMethod]
        public void Check_AfterAlarm_SetsStatusToMissed()
        {
            this.alarm.SetTime(SAMPLE_DATETIME);
            this.dateTimeProvider.Now.Returns(SAMPLE_DATETIME.AddMinutes(5));

            this.alarm.CheckStatusChange();

            Assert.AreEqual(AlarmStatus.Missed, this.alarm.Status);
        }

        [TestMethod]
        public void Check_WhenCompleted_DoesNothing()
        {
            this.alarm.SetTime(SAMPLE_DATETIME);
            this.dateTimeProvider.Now.Returns(SAMPLE_DATETIME.AddMinutes(-5));
            this.alarm.Status = AlarmStatus.Completed;

            this.alarm.CheckStatusChange();

            Assert.AreEqual(AlarmStatus.Completed, this.alarm.Status);
        }
        #endregion
    }
}
