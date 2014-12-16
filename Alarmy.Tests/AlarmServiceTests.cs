using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alarmy.Common;
using NSubstitute;
using System.Reflection;
using Alarmy.Services;

namespace Alarmy.Tests
{
    [TestClass]
    public class AlarmServiceTests
    {
        private IAlarmRepository repository;
        private ITimerService timer;
        private AlarmService service;

        private static IAlarm GetAlarm(AlarmStatus status)
        {
            var alarm = Substitute.For<IAlarm>();
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
            this.repository = Substitute.For<IAlarmRepository>();
            this.timer = Substitute.For<ITimerService>();
            this.service = new AlarmService(this.repository, timer);
            this.service.Start();
        }

        [TestMethod]
        private void Check_WhenTimerElapse_ShouldCallCheckOnSetAndRingingAlarms()
        {
            var alarm1 = GetAlarm(AlarmStatus.Set);
            var alarm2 = GetAlarm(AlarmStatus.Ringing);
            this.repository.List().Returns(new[] { alarm1, alarm2 });

            TickTimer();

            alarm1.Received().Check();
            alarm2.Received().Check();
        }

        [TestMethod]
        public void Check_WhenTimerElapse_ShouldNotCallCheckOnCompletedCancelledAndMissed()
        {
            var alarm1 = GetAlarm(AlarmStatus.Completed);
            var alarm2 = GetAlarm(AlarmStatus.Cancelled);
            var alarm3 = GetAlarm(AlarmStatus.Missed);
            this.repository.List().Returns(new[] { alarm1, alarm2, alarm3 });

            TickTimer();

            alarm1.DidNotReceive().Check();
            alarm2.DidNotReceive().Check();
            alarm3.DidNotReceive().Check();
        }

        [TestMethod]
        public void Check_WhenTimerElapse_TriggersEventOnChangedAlarms()
        {
            var alarm1 = GetAlarm(AlarmStatus.Set);
            alarm1.When(x => x.Check()).Do(x => alarm1.Status.Returns(AlarmStatus.Ringing));
            var alarm2 = GetAlarm(AlarmStatus.Set);    
            this.repository.List().Returns(new[] { alarm1, alarm2});           
            AlarmStatus oldStatus = AlarmStatus.Completed;
            IAlarm alarm = null;
            bool hasCalled = false;
            this.service.AlarmStatusChanged += (_, args) =>
            {
                hasCalled = true;
                oldStatus = args.OldStatus;
                alarm = args.Alarm;
            };

            TickTimer();

            Assert.IsTrue(hasCalled, "Event was not triggered");
            Assert.AreSame(alarm1, alarm);
            Assert.AreEqual(oldStatus, AlarmStatus.Set);
        }
    }
}
