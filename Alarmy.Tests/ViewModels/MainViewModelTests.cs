using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alarmy.ViewModels;
using Alarmy.Common;
using NSubstitute;
using Alarmy.Tests.Utils;

namespace Alarmy.Tests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        private FakeMainView view;
        private FakeAlarmService alarmService;
        private IAlarmManager alarmManager;
        private IAlarmFactory alarmFactory;
        private Settings settings;
        private MainViewModel viewModel;

        [TestInitialize]
        public void Setup()
        {
            this.view = Substitute.For<FakeMainView>();
            this.alarmService = Substitute.For<FakeAlarmService>();
            this.alarmManager = Substitute.For<IAlarmManager>();
            this.alarmFactory = Substitute.For<IAlarmFactory>();
            this.settings = new Settings();
            this.viewModel = new MainViewModel(this.view, this.alarmService, this.alarmManager, this.alarmFactory, this.settings);
        }

        [TestMethod]
        public void MainViewModel_AlarmStatusChange_PlaysAlarm_WhenSoundEnabled()
        {
            this.view.SetSound(true);
            var alarm = new FakeAlarm { IsRinging = true };
            
            this.alarmService.TriggerAlarmStatusChange(alarm);

            this.view.Received().PlayAlarm();
        }

        [TestMethod]
        public void MainViewModel_AlarmStatusChange_DoesNotPlayAlarm_WhenSoundDisabled()
        {
            this.view.SetSound(false);
            var alarm = new FakeAlarm { IsRinging = true };
                        
            this.alarmService.TriggerAlarmStatusChange(alarm);

            this.view.DidNotReceive().PlayAlarm();
        }

        [TestMethod]
        public void MainViewModel_SoundChange_PlaysAlarm_WhenSoundEnabled()
        {
            this.view.SetSound(false);
            var alarm = new FakeAlarm { IsRinging = true };            
            this.alarmService.AddStorage(alarm);

            this.view.SetSound(true);

            this.view.Received().PlayAlarm();
        }

        [TestMethod]
        public void MainViewModel_SoundChange_StopsAlarm_WhenSoundDisabled()
        {
            this.view.SetSound(true);
            var alarm = new FakeAlarm { IsRinging = true };            
            this.alarmService.Add(alarm);

            this.view.SetSound(false);

            this.view.Received().StopAlarm();
        }
        
        [TestMethod]
        public void MainViewModel_SoundChange_DoesNotPlayAlarm_WhenNoRingingAlarms()
        {
            this.view.SetSound(false);
            var alarm = new FakeAlarm { IsRinging = false };            
            this.alarmService.AddStorage(alarm);

            this.view.SetSound(true);

            this.view.DidNotReceive().PlayAlarm();
        }

        [TestMethod]
        public void MainViewModel_AlarmManagerSleep_StopsService_WhenSmartAlarmOn()        
        {
            this.view.SetSmartAlarm(true);

            this.alarmManager.OnSleep += Raise.Event();

            this.alarmService.Received().Stop();
        }

        [TestMethod]
        public void MainViewModel_AlarmManagerSleep_DoesNotStopService_WhenSmartAlarmOff()
        {
            this.view.SetSmartAlarm(false);

            this.alarmManager.OnSleep += Raise.Event();

            this.alarmService.DidNotReceive().Stop();
        }

        [TestMethod]
        public void MainViewModel_AlarmManagerWakeup_StartsService_WhenSmartAlarmOn()
        {
            this.view.SetSmartAlarm(true);
            
            this.alarmManager.OnWakeup += Raise.Event();

            this.alarmService.Received().Start();
        }

        [TestMethod]
        public void MainViewModel_AlarmManagerWakeup_RingsAlarm_WhenAnyRingingAlarm()
        {
            this.view.SetSmartAlarm(true);
            this.view.SetSound(true);
            var alarm = new FakeAlarm { IsRinging = true };
            this.alarmService.AddStorage(alarm);
            
            this.alarmManager.OnWakeup += Raise.Event();

            this.view.Received().PlayAlarm();
        }

        [TestMethod]
        public void MainViewModel_AlarmManagerSoundOn_RingsAlarm_WhenMuteOff()
        {
            this.view.SetSmartAlarm(true);
            this.view.SetSound(true);
            var alarm = new FakeAlarm { IsRinging = true };
            this.alarmService.AddStorage(alarm);

            this.alarmManager.OnSoundOn += Raise.Event();

            this.view.Received().PlayAlarm();
        }

        [TestMethod]
        public void MainViewModel_AlarmManagerSoundOn_DoesNotRingAlarm_WhenMuteOn()
        {
            this.view.SetSmartAlarm(true);
            this.view.SetSound(false);
            var alarm = new FakeAlarm { IsRinging = true };
            this.alarmService.AddStorage(alarm);

            this.alarmManager.OnSoundOn += Raise.Event();

            this.view.DidNotReceive().PlayAlarm();
        }

        [TestMethod]
        public void MainViewModel_AlarmManagerSoundOn_DoesNotRingAlarm_WhenNoRingingAlarms()
        {
            this.view.SetSmartAlarm(true);
            this.view.SetSound(true);
            var alarm = new FakeAlarm { IsRinging = false };
            this.alarmService.AddStorage(alarm);

            this.alarmManager.OnSoundOn += Raise.Event();

            this.view.DidNotReceive().PlayAlarm();
        }

        [TestMethod]
        public void MainViewModel_AlarmManagerSoundOff_StopsAlarm_WhenAnyRingingAlarms()
        {
            this.view.SetSmartAlarm(true);
            this.view.SetSound(true);
            var alarm = new FakeAlarm { IsRinging = true };
            this.alarmService.Add(alarm);

            this.alarmManager.OnSoundOff += Raise.Event();

            this.view.Received().StopAlarm();
        }

        [TestMethod]
        public void MainViewModel_AlarmServiceAdd_AddsAlarmToView()
        {
            var alarm = new FakeAlarm();

            this.alarmService.Add(alarm);

            this.view.Received().AddAlarm(alarm);
        }

        [TestMethod]
        public void MainViewModel_AlarmServiceAdd_StartsAlarm_WhenRingingAlarmAdded()
        {
            var alarm = new FakeAlarm { IsRinging = true };

            this.alarmService.Add(alarm);

            this.view.Received().PlayAlarm();
        }

        [TestMethod]
        public void MainViewModel_AlarmServiceRemove_RemovesAlarmFromView()
        {
            var alarm = new FakeAlarm();

            this.alarmService.Remove(alarm);

            this.view.Received().RemoveAlarm(alarm);
        }

        [TestMethod]
        public void MainViewModel_AlarmServiceRemove_StopsAlarm_WhenRingingAlarmRemoved()
        {
            var alarm = new FakeAlarm { IsRinging = true };
            this.alarmService.Add(alarm);

            this.alarmService.Remove(alarm);

            this.view.Received().StopAlarm();
        }

        [TestMethod]
        public void MainViewModel_ViewSmartAlarmDisable_StartsService_WhenSmartAlarmStoppedService()
        {
            this.view.SetSmartAlarm(true);
            this.alarmManager.OnSleep += Raise.Event();

            this.view.SetSmartAlarm(false);

            this.alarmService.Received().Start();
        }

        [TestMethod]
        public void MainViewModel_ViewSmartAlarmDisable_EnablesSound_WhenViewIsNotMuted()
        {
            this.view.SetSmartAlarm(true);
            this.view.SetSound(true);
            var alarm = new FakeAlarm { IsRinging = true };
            this.alarmService.AddStorage(alarm);
            this.alarmManager.OnSoundOff += Raise.Event();

            this.view.SetSmartAlarm(false);

            this.view.Received().PlayAlarm();
        }

        [TestMethod]
        public void MainViewModel_AlarmStatusChange_PopsupView_WhenEnabledAndViewHidden()
        {
            this.view.SetPopupOnAlarm(true);
            this.view.Visible = false;
            var alarm = new FakeAlarm { IsRinging = true };
            
            this.alarmService.TriggerAlarmStatusChange(alarm);

            this.view.Received().Show();
        }

        [TestMethod]
        public void MainViewModel_AlarmStatusChange_PopsupView_ForMissedAlarms()
        {
            this.view.SetPopupOnAlarm(true);
            this.view.Visible = false;
            var alarm = new FakeAlarm { Status = AlarmStatus.Missed };

            this.alarmService.TriggerAlarmStatusChange(alarm);

            this.view.Received().Show();
        }

        [TestMethod]
        public void MainViewModel_AlarmStatusChange_DoesNotPopupView_WhenAlreadyVisible()
        {
            this.view.SetPopupOnAlarm(true);
            this.view.Visible = true;
            var alarm = new FakeAlarm { IsRinging = true };

            this.alarmService.TriggerAlarmStatusChange(alarm);

            this.view.DidNotReceive().Show();
        }

        [TestMethod]
        public void MainViewModel_AlarmManagerWakeup_PopupView_WhenAnyRingingAlarm()
        {
            this.view.SetSmartAlarm(true);
            this.view.SetPopupOnAlarm(true);
            this.view.Visible = false;
            var alarm = new FakeAlarm { IsRinging = true };
            this.alarmService.AddStorage(alarm);

            this.alarmManager.OnWakeup += Raise.Event();

            this.view.Received().Show();
        }
    }
}
