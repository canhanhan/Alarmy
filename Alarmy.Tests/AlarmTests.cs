﻿using Alarmy.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Alarmy.Tests
{
    [TestClass]
    public class AlarmTests
    {
        public const string ARBITRARY_TEST_REASON = "TEST";
        private class TestAlarm : Alarm
        {
            public TestAlarm() : base(null) {}

            public new void SetStatus(AlarmStatus status)
            {
                base.SetStatus(status);
            }
        }

        #region Set status tests
        [TestMethod]
        public void Alarm_WhenSet_CanBeCancelled()
        {
            var alarm = new TestAlarm();            
            alarm.SetStatus(AlarmStatus.Set);
            alarm.Cancel(ARBITRARY_TEST_REASON);

            Assert.AreEqual(AlarmStatus.Cancelled, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenSet_CanBeRinging()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Set);

            alarm.SetStatus(AlarmStatus.Ringing);

            Assert.AreEqual(AlarmStatus.Ringing, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenSet_CannotBeCompleted()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Set);

            alarm.SetStatus(AlarmStatus.Completed);
        }

        [TestMethod]
        public void Alarm_WhenSet_CanBeMissed()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Set);

            alarm.SetStatus(AlarmStatus.Missed);
            Assert.AreEqual(AlarmStatus.Missed, alarm.Status);
        }        
        #endregion

        #region Ringing status tests
        [TestMethod]
        public void Alarm_WhenRinging_CanBeCompleted()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);

            alarm.SetStatus(AlarmStatus.Completed);
            Assert.AreEqual(AlarmStatus.Completed, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenRinging_CanBeMissed()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);

            alarm.SetStatus(AlarmStatus.Missed);
            Assert.AreEqual(AlarmStatus.Missed, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenRinging_CanBeCancelled()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);

            alarm.SetStatus(AlarmStatus.Cancelled);
            Assert.AreEqual(AlarmStatus.Cancelled, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenRinging_CanBeSet()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);

            alarm.SetStatus(AlarmStatus.Set);
            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }
        #endregion

        #region Cancelled state tests
        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCancelled_CannotBeCompleted()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Cancelled);

            alarm.SetStatus(AlarmStatus.Completed);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCancelled_CannotBeRinging()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Cancelled);

            alarm.SetStatus(AlarmStatus.Ringing);
        }

        [TestMethod]
        public void Alarm_WhenCancelled_CanBeSet()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Cancelled);

            alarm.SetStatus(AlarmStatus.Set);

            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCancelled_CannotBeMissed()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Cancelled);

            alarm.SetStatus(AlarmStatus.Missed);
        }
        #endregion

        #region Completed state tests
        [TestMethod]
        public void Alarm_WhenCompleted_CanBeSet()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);
            alarm.SetStatus(AlarmStatus.Completed);

            alarm.SetStatus(AlarmStatus.Set);

            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCompleted_CannotBeRinging()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);
            alarm.SetStatus(AlarmStatus.Completed);

            alarm.SetStatus(AlarmStatus.Ringing);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCompleted_CannotBeCancelled()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);
            alarm.SetStatus(AlarmStatus.Completed);

            alarm.SetStatus(AlarmStatus.Cancelled);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenCompleted_CannotBeMissed()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);
            alarm.SetStatus(AlarmStatus.Completed);

            alarm.SetStatus(AlarmStatus.Missed);
        }
        #endregion

        #region Missed status tests
        [TestMethod]
        public void Alarm_WhenMissed_CanBeCompleted()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);
            alarm.SetStatus(AlarmStatus.Missed);

            alarm.SetStatus(AlarmStatus.Completed);
            Assert.AreEqual(AlarmStatus.Completed, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenMissed_CanBeCancelled()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);
            alarm.SetStatus(AlarmStatus.Missed);

            alarm.SetStatus(AlarmStatus.Cancelled);
            Assert.AreEqual(AlarmStatus.Cancelled, alarm.Status);
        }

        [TestMethod]
        public void Alarm_WhenMissed_CanBeSet()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);
            alarm.SetStatus(AlarmStatus.Missed);

            alarm.SetStatus(AlarmStatus.Set);
            Assert.AreEqual(AlarmStatus.Set, alarm.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidStateException))]
        public void Alarm_WhenMissed_CannotBeRinging()
        {
            var alarm = new TestAlarm();
            alarm.SetStatus(AlarmStatus.Ringing);
            alarm.SetStatus(AlarmStatus.Missed);

            alarm.SetStatus(AlarmStatus.Ringing);
        }        
        #endregion
    }
}
