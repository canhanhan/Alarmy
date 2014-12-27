using Alarmy.Common;
using Alarmy.Infrastructure;
using Castle.Core.Logging;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Alarmy.Controllers
{
    internal class MainViewController
    {
        private readonly ISmartAlarmController smartAlarmController;
        private readonly SoundPlayer soundPlayer;
        private readonly IAlarmService alarmService;
        private readonly IMainView view;
        private readonly Settings settings;

        private ILogger logger = NullLogger.Instance;

        public ILogger Logger
        {
            get
            {
                return this.logger;
            }
            set
            {
                this.logger = value;
            }
        }

        public MainViewController(IMainView view, IAlarmService alarmService, ISmartAlarmController smartAlarmController, Settings settings)
        {
            this.view = view;
            this.view.OnLoad += view_Load;
            this.view.OnClosing += view_Closing;
            this.view.OnHideRequest += view_OnHideRequest;
            this.view.OnExitRequest += view_OnExitRequest;
            this.view.OnHushRequest += view_OnHushRequest;
            this.view.OnCompleteRequest += view_OnCompleteRequest;
            this.view.OnCancelRequest += view_OnCancelRequest;
            this.view.OnChangeRequest += view_OnChangeRequest;
            this.view.OnNewRequest += view_OnNewRequest;
            this.view.OnEnableSoundChange += view_OnEnableSoundChange;
            this.view.OnPopupOnAlarmChange += view_OnPopupOnAlarmChange;
            this.view.OnSmartAlarmChange += view_OnSmartAlarmChange;

            this.settings = settings;
 
            this.alarmService = alarmService;
            this.alarmService.Interval = settings.CheckInterval;
            this.alarmService.AlarmAdded += alarmService_AlarmAdded;
            this.alarmService.AlarmRemoved += alarmService_AlarmRemoved;
            this.alarmService.AlarmUpdated += alarmService_AlarmUpdated;
            this.alarmService.AlarmStatusChanged += alarmService_AlarmStatusChanged;

            soundPlayer = new SoundPlayer(settings.AlarmSoundFile);

            this.smartAlarmController = smartAlarmController;
            this.smartAlarmController.OnSleep += smartAlarmController_OnSleep;
            this.smartAlarmController.OnWakeup += smartAlarmController_OnWakeup;
            this.smartAlarmController.OnSoundOn += smartAlarmController_OnSound;
            this.smartAlarmController.OnSoundOff += smartAlarmController_OnSound;
        }

        public void Start()
        {
            this.view.Show();
        }

        private void CheckForAlarmSound()
        {
            if (this.view.EnableSound && (!this.view.SmartAlarm || !this.smartAlarmController.IsSilent) && this.AnyRingingAlarms())
            {
                if (!this.soundPlayer.IsPlaying)
                    this.soundPlayer.Play();
            }
            else
            {
                if (this.soundPlayer.IsPlaying)
                    this.soundPlayer.Stop();
            }
        }

        private bool AnyRingingAlarms()
        {
            var alarms = this.alarmService.List();
            return alarms.Any(x => x.Status == AlarmStatus.Ringing && !x.IsHushed);
        }

        #region View Events
        private void view_OnSmartAlarmChange(object sender, EventArgs e)
        {
            if (this.view.SmartAlarm)
            {
                Logger.Info("Smart alarm is enabled.");
            }
            else
            {
                Logger.Info("Smart alarm is disabled");
            }
        }

        private void view_OnPopupOnAlarmChange(object sender, EventArgs e)
        {
            if (this.view.PopupOnAlarm)
            {
                Logger.Info("List is set to popup on alarm");
            }
            else
            {
                Logger.Info("List is set not to popup on alarm");
            }
        }

        private void view_OnEnableSoundChange(object sender, EventArgs e)
        {
            if (this.view.EnableSound)
            {
                Logger.Info("Sound is enabled.");
            }
            else
            {
                Logger.Info("Sound is disabled.");
                if (this.soundPlayer.IsPlaying)
                {
                    this.soundPlayer.Stop();
                }
            }
        }

        private void view_OnNewRequest(object sender, EventArgs e)
        {
            var metadata = this.view.AskAlarmMetadata();
            if (metadata == null)
            {
                return;
            }
            var alarm = new Alarm() { Title = metadata.Title };
            alarm.Set(metadata.Time);

            Logger.Info(alarm.ToString() + " is created");
            this.alarmService.Add(alarm);
        }

        private void view_OnChangeRequest(object sender, AlarmEventArgs e)
        {
            var alarm = e.Alarm;
            var metadata = this.view.AskAlarmMetadata(alarm);
            if (metadata == null)
            {
                return;
            }
            Logger.InfoFormat("{0} is changed. New time: {1}, New title: {2}", alarm.ToString(), metadata.Title, metadata.Time);
            alarm.Title = metadata.Title;
            alarm.Set(metadata.Time);
            this.alarmService.Update(alarm);
        }

        private void view_OnCancelRequest(object sender, AlarmEventArgs e)
        {
            var alarm = e.Alarm;
            var reason = this.view.AskCancelReason(alarm);
            if (reason == null)
            {
                return;
            }
            Logger.InfoFormat("{0} is cancelled. Reason: {1}", alarm.ToString(), reason);
            alarm.Cancel(reason);
            this.alarmService.Update(alarm);
        }

        private void view_OnCompleteRequest(object sender, AlarmEventArgs e)
        {
            var alarm = e.Alarm;
            Logger.Info(alarm + " is completed.");
            alarm.Complete();

            this.alarmService.Update(alarm);
        }

        private void view_OnHushRequest(object sender, AlarmEventArgs e)
        {
            var alarm = e.Alarm;
            if (alarm.IsHushed)
            {
                Logger.Info(alarm + " is un-hushed.");
                alarm.IsHushed = false;
            }
            else
            {
                Logger.Info(alarm + " is hushed.");
                alarm.IsHushed = true;
            }

            this.alarmService.Update(alarm);
        }

        private void view_OnExitRequest(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void view_OnHideRequest(object sender, EventArgs e)
        {
            if (this.view.Visible)
            {
                Logger.Info("List is hid.");
                this.view.Hide();
            }
            else
            {
                Logger.Info("List is visible.");
                this.view.Show();
            }
        }

        private void view_Load(object sender, EventArgs e)
        {
            this.view.PopupOnAlarm = this.settings.PopupOnAlarm;
            this.view.EnableSound = this.settings.EnableSound;
            this.view.SmartAlarm = this.settings.SmartAlarm;
             
            if (this.settings.StartHidden)
            {
                this.view.Hide();
            }

            this.alarmService.Start();
        }

        private void view_Closing(object sender, EventArgs e)
        {
            this.alarmService.Stop();
        }
        #endregion

        #region Alarm service Events
        private void alarmService_AlarmUpdated(object sender, AlarmEventArgs e)
        {
            this.view.UpdateAlarm(e.Alarm);
            this.CheckForAlarmSound();
        }

        private void alarmService_AlarmRemoved(object sender, AlarmEventArgs e)
        {
            this.view.RemoveAlarm(e.Alarm);
            this.CheckForAlarmSound();
        }

        private void alarmService_AlarmAdded(object sender, AlarmEventArgs e)
        {
            this.view.AddAlarm(e.Alarm);
            this.CheckForAlarmSound();
        }

        private void alarmService_AlarmStatusChanged(object sender, AlarmEventArgs e)
        {
            if ((e.Alarm.Status == AlarmStatus.Ringing || e.Alarm.Status == AlarmStatus.Missed) && !this.view.Visible)
            {
                this.view.Show();
            }

            this.alarmService_AlarmUpdated(this, new AlarmEventArgs(e.Alarm));
        }
        #endregion

        #region SmartAlarm Events
        private void smartAlarmController_OnSound(object sender, EventArgs e)
        {
            this.CheckForAlarmSound();
        }

        private void smartAlarmController_OnWakeup(object sender, EventArgs e)
        {
            this.alarmService.Start();
        }

        private void smartAlarmController_OnSleep(object sender, EventArgs e)
        {
            this.alarmService.Stop();
        }
        #endregion


    }
}
