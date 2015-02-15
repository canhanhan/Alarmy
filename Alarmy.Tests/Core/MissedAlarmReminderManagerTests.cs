using Alarmy.Common;
using Alarmy.Core;
using Alarmy.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Alarmy.Tests.Core
{
    [TestClass]
    public class MissedAlarmReminderManagerTests
    {
        private ITimer timer;
        private FakeAlarmService service;
        private MissedAlarmReminderManager manager;

        [TestInitialize]
        public void Setup()
        {
            this.timer = Substitute.For<ITimer>();
            this.service = Substitute.For<FakeAlarmService>();
            this.manager = new MissedAlarmReminderManager(service, timer);
        }

        [TestMethod]
        public void AlarmReminderManager_TimerTrigger_TriggersOnRequestNotification_WhenMissingAlarmsExist()
        {
            var hasReceivedEvent = false;
            service.Add(FakeAlarm.GetAlarm(AlarmStatus.Missed));
            manager.OnRequestNotification += (_, __) => hasReceivedEvent = true;

            timer.Elapsed += Raise.Event();

            Assert.IsTrue(hasReceivedEvent);            
        }

        [TestMethod]
        public void AlarmReminderManager_TimerTrigger_DoesNotTriggerOnRequestNotification_WhenMissingAlarmsExist()
        {
            var hasReceivedEvent = false;
            service.Add(FakeAlarm.GetAlarm(AlarmStatus.Completed));
            manager.OnRequestNotification += (_, __) => hasReceivedEvent = true;

            timer.Elapsed += Raise.Event();

            Assert.IsFalse(hasReceivedEvent);
        }
    }
}
