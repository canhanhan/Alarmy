using Alarmy.Common;
using Alarmy.Core;
using Alarmy.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Linq;
using System.Reflection;

namespace Alarmy.Tests
{
    [TestClass]
    public class AlarmServiceTests
    {
        private InMemoryAlarmRepository repository;
        private ITimer timer;
        private AlarmService service;

        private static FakeAlarm GetAlarm(AlarmStatus status = default(AlarmStatus), Guid id = default(Guid))
        {
            var alarm = Substitute.For<FakeAlarm>();
            alarm.Id = id == default(Guid) ? Guid.NewGuid() : id;
            alarm.Status = status;
            return alarm;
        }

        private void TickTimer()
        {
            this.timer.Elapsed += Raise.Event();
        }

        [TestInitialize]
        public void Setup()
        {
            this.repository = Substitute.For<InMemoryAlarmRepository>();
            this.timer = Substitute.For<ITimer>();
            this.service = new AlarmService(this.repository, timer);
            this.service.Start();
        }

        [TestMethod]
        public void AlarmService_Start_StartsRepository()
        {
            this.service.Start();

            ((ISupportsStartStop)this.repository).Received().Start();
        }

        [TestMethod]
        public void AlarmService_Start_ReadsRepository()
        {
            var alarm = GetAlarm();
            this.repository.Add(alarm);

            this.service.Start();

            Assert.AreSame(alarm, this.service.List().Single());
        }

        [TestMethod]
        public void AlarmService_Stop_StopsRepository()
        {
            this.service.Stop();

            ((ISupportsStartStop)this.repository).Received().Stop();
        }

        [TestMethod]
        public void AlarmService_Add_AddsToRepository()
        {
            var alarm = GetAlarm();

            this.service.Add(alarm);

            Assert.AreSame(alarm, this.repository.Load().First());
        }

        [TestMethod]
        public void AlarmService_Add_TriggersEvent()
        {
            var alarm = GetAlarm();
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.OnAlarmAdd += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            this.service.Add(alarm);

            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm, args.Alarm);
        }

        [TestMethod]
        public void AlarmService_Add_TriggersAlarmStatusCheck()
        {
            var alarm = GetAlarm();

            this.service.Add(alarm);

            alarm.Received().CheckStatusChange();
        }

        [TestMethod]
        public void AlarmService_Remove_RemovesAlarm()
        {
            var alarm1 = GetAlarm();
            var alarm2 = GetAlarm();
            this.service.Add(alarm1);
            this.service.Add(alarm2);

            this.service.Remove(alarm2);
            
            Assert.AreSame(alarm1, this.repository.Load().First());
            Assert.AreEqual(1, this.repository.Load().Count());
        }

        [TestMethod]
        public void AlarmService_Remove_TriggersEvent()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.OnAlarmRemoval += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            this.service.Remove(alarm);

            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm, args.Alarm);
        }

        [TestMethod]
        public void AlarmService_Update_UpdatesRepository()
        {
            var alarm1 = GetAlarm();
            alarm1.Title = "Title";
            this.service.Add(alarm1);
            alarm1.Title = "New Title";

            this.service.Update(alarm1);

            Assert.AreSame(alarm1, this.repository.Load().First());
            Assert.AreEqual("New Title", this.repository.Load().First().Title);
        }

        [TestMethod]
        public void AlarmService_Update_TriggersEvent()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.OnAlarmUpdate += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            this.service.Update(alarm);

            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm, args.Alarm);
        }

        [TestMethod]
        public void AlarmService_Update_TriggersAlarmStatusCheck()
        {
            var alarm = GetAlarm();

            this.service.Update(alarm);

            alarm.Received().CheckStatusChange();
        }


        [TestMethod]
        public void AlarmService_Timer_TriggersEvent_WhenAlarmDeletedFromRepository()
        {
            var alarm1 = GetAlarm();
            var alarm2 = GetAlarm();
            this.service.Add(alarm1);
            this.service.Add(alarm2);
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.OnAlarmRemoval += (_, e) =>
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
        public void AlarmService_Timer_TriggersEvent_WhenAlarmUpdatedInRepository()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var newAlarm = GetAlarm(id: alarm.Id);
            newAlarm.Title = "New Title";
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.OnAlarmUpdate += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            this.repository.Update(newAlarm);
            this.TickTimer();
            
            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm, args.Alarm);
        }

        [TestMethod]
        public void AlarmService_Timer_TriggersEvent_WhenAlarmStatusUpdatedInRepository()
        {
            var alarm = GetAlarm(AlarmStatus.Ringing);
            this.service.Add(alarm);
            var newAlarm = GetAlarm(AlarmStatus.Completed, alarm.Id);
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.OnAlarmUpdate += (_, e) =>
            {
                triggerCount++;
                args = e;
            };

            this.repository.Update(newAlarm);
            this.TickTimer();

            Assert.AreEqual(1, triggerCount);
            Assert.AreSame(alarm, args.Alarm);
        }        

        [TestMethod]
        public void AlarmService_Timer_TriggersEvent_WhenAlarmAddedToRepository()
        {
            var alarm = GetAlarm();
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.OnAlarmAdd += (_, e) =>
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
        public void AlarmService_Timer_TriggersEvent_WhenAlarmStatusChangedInRepository()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var triggerCount = 0;
            AlarmEventArgs args = null;
            this.service.OnAlarmStatusChange += (_, e) =>
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
        public void AlarmService_Timer_TriggersOnlyOneEvent_WhenAlarmStatusChangedInRepository()
        {
            var alarm = GetAlarm(AlarmStatus.Ringing);
            this.service.Add(alarm);
            var newAlarm = GetAlarm(AlarmStatus.Completed, id: alarm.Id);
            var alarmAdded = false;
            var alarmUpdated = false;
            var alarmRemoved = false;
            var alarmStatusChanged = false;

            this.service.OnAlarmAdd += (_, e) => alarmAdded = true;
            this.service.OnAlarmUpdate += (_, e) => alarmUpdated = true;
            this.service.OnAlarmRemoval += (_, e) => alarmRemoved = true;
            this.service.OnAlarmStatusChange += (_, e) => alarmStatusChanged = true;

            this.repository.Update(newAlarm);
            this.TickTimer();

            Assert.IsFalse(alarmAdded, "OnAlarmAdd was called");
            Assert.IsTrue(alarmUpdated, "OnAlarmUpdate was not called");
            Assert.IsFalse(alarmRemoved, "OnAlarmRemoval was called");
            Assert.IsFalse(alarmStatusChanged, "OnAlarmStatusChange was called");
        }

        [TestMethod]
        public void AlarmService_Timer_TriggersOnlyOneEvent_WhenAlarmAddedToRepository()
        {
            var alarm = GetAlarm();
            var alarmAdded = false;
            var alarmUpdated = false;
            var alarmRemoved = false;
            var alarmStatusChanged = false;

            this.service.OnAlarmAdd += (_, e) => alarmAdded = true;
            this.service.OnAlarmUpdate += (_, e) => alarmUpdated = true;
            this.service.OnAlarmRemoval += (_, e) => alarmRemoved = true;
            this.service.OnAlarmStatusChange += (_, e) => alarmStatusChanged = true;

            this.repository.Add(alarm);
            this.TickTimer();

            Assert.IsTrue(alarmAdded, "OnAlarmAdd was not called");
            Assert.IsFalse(alarmUpdated, "OnAlarmUpdate was called");
            Assert.IsFalse(alarmRemoved, "OnAlarmRemove was called");
            Assert.IsFalse(alarmStatusChanged, "OnAlarmStatusChange was called");
        }

        [TestMethod]
        public void AlarmService_Timer_TriggersOnlyOneEvent_WhenAlarmRemovedFromRepository()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var alarmAdded = false;
            var alarmUpdated = false;
            var alarmRemoved = false;
            var alarmStatusChanged = false;

            this.service.OnAlarmAdd += (_, e) => alarmAdded = true;
            this.service.OnAlarmUpdate += (_, e) => alarmUpdated = true;
            this.service.OnAlarmRemoval += (_, e) => alarmRemoved = true;
            this.service.OnAlarmStatusChange += (_, e) => alarmStatusChanged = true;

            this.repository.Remove(alarm);
            this.TickTimer();

            Assert.IsFalse(alarmAdded, "OnAlarmAdd was called");
            Assert.IsFalse(alarmUpdated, "OnAlarmUpdate was called");
            Assert.IsTrue(alarmRemoved, "OnAlarmRemoval was not called");
            Assert.IsFalse(alarmStatusChanged, "OnAlarmStatusChange was called");
        }

        [TestMethod]
        public void AlarmService_Timer_TriggersOnlyOneEvent_WhenAlarmUpdatedInRepository()
        {
            var alarm = GetAlarm();
            this.service.Add(alarm);
            var newAlarm = GetAlarm(id: alarm.Id);
            newAlarm.Title = "Test";

            var alarmAdded = false;
            var alarmUpdated = false;
            var alarmRemoved = false;
            var alarmStatusChanged = false;

            this.service.OnAlarmAdd += (_, e) => alarmAdded = true;
            this.service.OnAlarmUpdate += (_, e) => alarmUpdated = true;
            this.service.OnAlarmRemoval += (_, e) => alarmRemoved = true;
            this.service.OnAlarmStatusChange += (_, e) => alarmStatusChanged = true;

            this.repository.Update(newAlarm);
            this.TickTimer();

            Assert.IsFalse(alarmAdded, "OnAlarmAdd was called");
            Assert.IsTrue(alarmUpdated, "OnAlarmUpdate was not called");
            Assert.IsFalse(alarmRemoved, "OnAlarmRemove was called");
            Assert.IsFalse(alarmStatusChanged, "OnAlarmStatusChange was called");

        }
    }
}
