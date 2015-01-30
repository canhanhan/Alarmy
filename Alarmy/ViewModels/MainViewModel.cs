using Alarmy.Common;
using Alarmy.Infrastructure;
using Castle.Core.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Alarmy.ViewModels
{
    internal class MainViewModel :IDisposable
    {
        private readonly IAlarmManager alarmManager;
        private readonly SoundPlayer soundPlayer;
        private readonly IAlarmService alarmService;
        private readonly IAlarmFactory alarmFactory;
        private readonly IMainView view;
        private readonly Settings settings;

        private bool smartAlarmEnabled;
        private bool smartAlarmMute;
        private bool soundEnabled;
        private bool popupOnAlarmEnabled;

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

        public MainViewModel(IMainView view, IAlarmService alarmService, IAlarmManager alarmManager, IAlarmFactory alarmFactory, Settings settings)        
        {
            this.alarmFactory = alarmFactory;
            this.settings = settings;

            this.popupOnAlarmEnabled = this.settings.PopupOnAlarm;
            this.soundEnabled = this.settings.EnableSound;
            this.smartAlarmEnabled = this.settings.SmartAlarm;

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

            this.alarmService = alarmService;
            this.alarmService.AlarmAdded += alarmService_AlarmAdded;
            this.alarmService.AlarmRemoved += alarmService_AlarmRemoved;
            this.alarmService.AlarmUpdated += alarmService_AlarmUpdated;
            this.alarmService.AlarmStatusChanged += alarmService_AlarmStatusChanged;

            soundPlayer = new SoundPlayer(settings.AlarmSoundFile);

            this.alarmManager = alarmManager;
            this.alarmManager.OnSleep += alarmManager_OnSleep;
            this.alarmManager.OnWakeup += alarmManager_OnWakeup;
            this.alarmManager.OnSoundOn += alarmManager_OnSoundOn;
            this.alarmManager.OnSoundOff += alarmManager_OnSoundOff;
        }

        public void Dispose()
        {
            if (this.soundPlayer != null)
                this.soundPlayer.Dispose();
        }

        private void CheckForAlarmSound()
        {
            if (this.IsAlarmAllowed() && this.AnyRingingAlarms())
            {
                if (!this.soundPlayer.IsPlaying)
                {
                    Logger.Info("Alarm is ringing...");
                    this.soundPlayer.Play();
                }                    
            }
            else if (this.soundPlayer.IsPlaying)
            {
                    Logger.Info("Alarm is not ringing...");
                    this.soundPlayer.Stop();
            }
        }

        private bool IsAlarmAllowed()
        {
            return this.soundEnabled && !(this.smartAlarmEnabled && this.smartAlarmMute);
        }

        private bool AnyRingingAlarms()
        {
            return this.alarmService.List().Any(alarm => alarm.IsRinging);
        }

        #region View Events
        private void view_OnSmartAlarmChange(object sender, EventArgs e)
        {
            this.smartAlarmEnabled = this.view.SmartAlarm;
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
            this.popupOnAlarmEnabled = this.view.PopupOnAlarm;
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
            this.soundEnabled = this.view.EnableSound;
            if (this.view.EnableSound)
            {
                Logger.Info("Sound is enabled.");
            }
            else
            {
                Logger.Info("Sound is disabled.");
            }

            this.CheckForAlarmSound();
        }

        private void view_OnNewRequest(object sender, EventArgs e)
        {
            var metadata = this.view.AskAlarmMetadata();
            if (metadata == null)
            {
                return;
            }
            var alarm = this.alarmFactory.Create();
            alarm.Title = metadata.Title;
            alarm.SetTime(metadata.Time);

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
            Logger.InfoFormat(CultureInfo.InvariantCulture, "{0} is changed. New time: {1}, New title: {2}", alarm.ToString(), metadata.Title, metadata.Time);
            alarm.Title = metadata.Title;
            alarm.SetTime(metadata.Time);
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
            Logger.InfoFormat(CultureInfo.InvariantCulture, "{0} is cancelled. Reason: {1}", alarm.ToString(), reason);
            alarm.Cancel();
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
                Logger.Info("List is hidden.");
                this.view.Hide();
            }
            else
            {
                Logger.Info("List is visible.");
                this.view.Show();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Threading.Timer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void view_Load(object sender, EventArgs e)
        {
            this.view.PopupOnAlarm = this.popupOnAlarmEnabled;
            this.view.EnableSound = this.soundEnabled;
            this.view.SmartAlarm = this.smartAlarmEnabled;

            this.alarmService.Start();

            if (this.settings.StartHidden)
            {
                new System.Threading.Timer((_) => this.view_OnHideRequest(null, null), null, 100, System.Threading.Timeout.Infinite);
            }
        }

        private void view_Closing(object sender, EventArgs e)
        {
            Logger.Info("Closing...");
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

        #region AlarmManager Events
        private void alarmManager_OnSoundOn(object sender, EventArgs e)
        {
            this.smartAlarmMute = false;
            this.CheckForAlarmSound();
        }

        private void alarmManager_OnSoundOff(object sender, EventArgs e)
        {
            this.smartAlarmMute = true;
            this.CheckForAlarmSound();
        }

        private void alarmManager_OnWakeup(object sender, EventArgs e)
        {
            Logger.Info("Smart Alarm sent wakeup");
            this.alarmService.Start();            
        }

        private void alarmManager_OnSleep(object sender, EventArgs e)
        {
            Logger.Info("Smart Alarm sent sleep");
            this.alarmService.Stop();
        }
        #endregion
    }
}
