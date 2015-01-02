using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alarmy.Common;
using NSubstitute;
using System.Reflection;
using Alarmy.Services;
using System.Linq;
using System.Linq.Expressions;

namespace Alarmy.Tests
{
    [TestClass]
    public class AlarmServiceTests
    {
        private IAlarmRepository repository;
        private ITimer timer;
        private AlarmService service;

        private static IAlarm GetAlarm(AlarmStatus status = default(AlarmStatus))
        {
            var alarm = Substitute.For<IAlarm>();
            alarm.Id = Guid.NewGuid();
            alarm.Status.Returns(status);
            return alarm;
        }

        private void TickTimer()
        {
            var type = this.service.GetType();
            type.GetMethod("_Timer_Elapsed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this.service, new object[] { null, null });
        }

        [TestInitialize]
        public void Setup()
        {
            this.repository = new InMemoryAlarmRepository();
            this.timer = Substitute.For<ITimer>();
            this.service = new AlarmService(this.repository, timer);
            this.service.StartTimer();
        }

        [TestMethod]
        public void Add_AddsToRepository()
        {
            var alarm = GetAlarm();

            this.service.Add(alarm);

            Assert.AreSame(alarm, this.repository.List().First());
        }

        [TestMethod]
        public void Add_TriggersEvent()
        {
            var alarm = GetAlarm();
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.AlarmAdded += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            this.service.Add(alarm);

            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm, args.Alarm);
        }

        [TestMethod]
        public void Remove_RemovesAlarm()
        {
            var alarm1 = GetAlarm();
            var alarm2 = GetAlarm();
            this.service.Add(alarm1);
            this.service.Add(alarm2);

            this.service.Remove(alarm2);
            
            Assert.AreSame(alarm1, this.repository.List().First());
            Assert.AreEqual(1, this.repository.List().Count());
        }

        [TestMethod]
        public void Remove_TriggersEvent()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.AlarmRemoved += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            this.service.Remove(alarm);

            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm, args.Alarm);
        }

        [TestMethod]
        public void Update_UpdatesRepository()
        {
            var alarm1 = GetAlarm();
            alarm1.Title = "Title";
            this.service.Add(alarm1);
            alarm1.Title = "New Title";

            this.service.Update(alarm1);

            Assert.AreSame(alarm1, this.repository.List().First());
            Assert.AreEqual("New Title", this.repository.List().First().Title);
        }

        [TestMethod]
        public void Update_TriggersEvent()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.AlarmUpdated += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            this.service.Update(alarm);

            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm, args.Alarm);
        }


        [TestMethod]
        public void Timer_WhenAlarmDeletedFromRepository_TriggersEvent()
        {
            var alarm1 = GetAlarm();
            var alarm2 = GetAlarm();
            this.service.Add(alarm1);
            this.service.Add(alarm2);
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.AlarmRemoved += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            this.repository.Remove(alarm2);
            this.TickTimer();

            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm2, args.Alarm);
        }

        [TestMethod]
        public void Timer_WhenAlarmUpdatedInRepository_TriggersEvent()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            alarm.Title = "New Title";
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.AlarmUpdated += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            this.repository.Update(alarm);
            this.TickTimer();

            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm, args.Alarm);
        }

        [TestMethod]
        public void Timer_WhenAlarmAddedToRepository_TriggersEvent()
        {
            var alarm = GetAlarm();
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.AlarmAdded += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            this.repository.Add(alarm);
            this.TickTimer();

            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm, args.Alarm);
        }

        [TestMethod]
        public void Timer_WhenAlarmStatusChangedInRepository_TriggersEvent()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.AlarmStatusChanged += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            alarm.CheckStatusChange().Returns(true);
            this.repository.Update(alarm);
            this.TickTimer();

            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm, args.Alarm);
        }

        [TestMethod]
        public void Timer_WhenAlarmStatusChangedInRepository_TriggersOnlyOneEvent()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var alarmAdded = false;
            var alarmUpdated = false;
            var alarmRemoved = false;
            var alarmStatusChanged = false;

            this.service.AlarmAdded += (_, e) => alarmAdded = true;
            this.service.AlarmUpdated += (_, e) => alarmUpdated = true;
            this.service.AlarmRemoved += (_, e) => alarmRemoved = true;
            this.service.AlarmStatusChanged += (_, e) => alarmStatusChanged = true;

            alarm.Equals(alarm, true).Returns(true);
            alarm.CheckStatusChange().Returns(true);
            this.repository.Update(alarm);
            this.TickTimer();

            Assert.IsFalse(alarmAdded);
            Assert.IsFalse(alarmUpdated);
            Assert.IsFalse(alarmRemoved);
            Assert.IsTrue(alarmStatusChanged);
        }

        [TestMethod]
        public void Timer_WhenAlarmAddedToRepository_TriggersOnlyOneEvent()
        {
            var alarm = GetAlarm();
            var alarmAdded = false;
            var alarmUpdated = false;
            var alarmRemoved = false;
            var alarmStatusChanged = false;

            this.service.AlarmAdded += (_, e) => alarmAdded = true;
            this.service.AlarmUpdated += (_, e) => alarmUpdated = true;
            this.service.AlarmRemoved += (_, e) => alarmRemoved = true;
            this.service.AlarmStatusChanged += (_, e) => alarmStatusChanged = true;

            this.repository.Add(alarm);
            this.TickTimer();

            Assert.IsTrue(alarmAdded);
            Assert.IsFalse(alarmUpdated);
            Assert.IsFalse(alarmRemoved);
            Assert.IsFalse(alarmStatusChanged);
        }

        [TestMethod]
        public void Timer_WhenAlarmRemovedFromRepository_TriggersOnlyOneEvent()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var alarmAdded = false;
            var alarmUpdated = false;
            var alarmRemoved = false;
            var alarmStatusChanged = false;

            this.service.AlarmAdded += (_, e) => alarmAdded = true;
            this.service.AlarmUpdated += (_, e) => alarmUpdated = true;
            this.service.AlarmRemoved += (_, e) => alarmRemoved = true;
            this.service.AlarmStatusChanged += (_, e) => alarmStatusChanged = true;

            this.repository.Remove(alarm);
            this.TickTimer();

            Assert.IsFalse(alarmAdded);
            Assert.IsFalse(alarmUpdated);
            Assert.IsTrue(alarmRemoved);
            Assert.IsFalse(alarmStatusChanged);
        }

        [TestMethod]
        public void Timer_WhenAlarmUpdatedInRepository_TriggersOnlyOneEvent()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var alarmAdded = false;
            var alarmUpdated = false;
            var alarmRemoved = false;
            var alarmStatusChanged = false;

            this.service.AlarmAdded += (_, e) => alarmAdded = true;
            this.service.AlarmUpdated += (_, e) => alarmUpdated = true;
            this.service.AlarmRemoved += (_, e) => alarmRemoved = true;
            this.service.AlarmStatusChanged += (_, e) => alarmStatusChanged = true;

            alarm.Equals(alarm, true).Returns(false);
            this.TickTimer();

            Assert.IsFalse(alarmAdded);
            Assert.IsTrue(alarmUpdated);
            Assert.IsFalse(alarmRemoved);
            Assert.IsFalse(alarmStatusChanged);
        }
    }
}
