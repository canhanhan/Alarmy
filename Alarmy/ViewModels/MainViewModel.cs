using Alarmy.Common;
using Alarmy.Infrastructure;
using Castle.Core.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Alarmy.ViewModels
{
    internal class MainViewModel
    {
        private readonly IAlarmManager alarmManager;        
        private readonly IAlarmService alarmService;
        private readonly IAlarmFactory alarmFactory;
        private readonly IAlarmReminderManager alarmReminderManager;
        private readonly IImporter importer;
        private readonly IMainView view;
        private readonly Settings settings;

        private bool isAlarmRinging;
        private bool smartAlarmEnabled;
        private bool soundEnabled;
        private bool popupOnAlarmEnabled;
        private bool isSleeping;

        private bool SmartAlarmEnabled
        {
            get { return this.smartAlarmEnabled; }
            set 
            {
                this.smartAlarmEnabled = value;
                this.view.SmartAlarm = value;
            }
        }
        private bool SoundEnabled
        {
            get { return this.soundEnabled; }
            set
            {
                this.soundEnabled = value;
                this.view.IsMute = value;
            }
        }        
        private bool PopupOnAlarmEnabled
        {
            get { return this.popupOnAlarmEnabled; }
            set
            {
                this.popupOnAlarmEnabled = value;
                this.view.PopupOnAlarm = value;
            }
        }
                
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

        public MainViewModel(IMainView view, IAlarmService alarmService, IAlarmManager alarmManager, IAlarmReminderManager alarmReminderManager,
                             IAlarmFactory alarmFactory, IImporter importer, Settings settings)        
        {
            if (view == null)
                throw new ArgumentNullException("view");

            if (alarmService == null)
                throw new ArgumentNullException("alarmService");

            if (alarmManager == null)
                throw new ArgumentNullException("alarmManager");

            if (alarmFactory == null)
                throw new ArgumentNullException("alarmFactory");

            if (importer == null)
                throw new ArgumentNullException("importer");

            if (settings == null)
                throw new ArgumentNullException("settings");

            if (alarmReminderManager == null)
                throw new ArgumentNullException("alarmReminderManager");

            this.alarmFactory = alarmFactory;
            this.settings = settings;

            this.view = view;
            this.view.OnLoad += view_OnLoad;
            this.view.OnClosing += view_OnClosing;
            this.view.OnHideRequest += view_OnHideRequest;
            this.view.OnShowRequest += view_OnShowRequest;
            this.view.OnExitRequest += view_OnExitRequest;
            this.view.OnHushRequest += view_OnHushRequest;
            this.view.OnUnhushRequest += view_OnUnhushRequest;
            this.view.OnCompleteRequest += view_OnCompleteRequest;
            this.view.OnCancelRequest += view_OnCancelRequest;
            this.view.OnChangeRequest += view_OnChangeRequest;
            this.view.OnNewRequest += view_OnNewRequest;
            this.view.OnImportRequest += view_OnImportRequest;
            this.view.OnEnableSoundRequest += view_OnEnableSound;
            this.view.OnDisableSoundRequest += view_OnDisableSound;
            this.view.OnPopupOnAlarmOn += view_OnPopupOnAlarmOn;
            this.view.OnPopupOnAlarmOff += view_OnPopupOnAlarmOff;
            this.view.OnSmartAlarmOn += view_OnSmartAlarmOn;
            this.view.OnSmartAlarmOff += view_OnSmartAlarmOff;

            this.alarmService = alarmService;
            this.alarmService.OnAlarmAdd += alarmService_OnAlarmAdd;
            this.alarmService.OnAlarmRemoval += alarmService_OnAlarmRemoval;
            this.alarmService.OnAlarmUpdate += alarmService_OnAlarmUpdate;
            this.alarmService.OnAlarmStatusChange += alarmService_OnAlarmStatusChange;           

            this.alarmManager = alarmManager;
            this.alarmManager.OnSleep += alarmManager_OnSleep;
            this.alarmManager.OnWakeup += alarmManager_OnWakeup;

            this.alarmReminderManager = alarmReminderManager;
            this.alarmReminderManager.OnRequestNotification += alarmReminderManager_OnRequestNotification;

            this.importer = importer;

            this.PopupOnAlarmEnabled = this.settings.PopupOnAlarm;
            this.SoundEnabled = this.settings.EnableSound;
            this.SmartAlarmEnabled = this.settings.SmartAlarm;
        }

        private void CheckForAlarms()
        {
            if (!this.isSleeping && !this.view.Visible && this.AnyRingingAlarms(includeMissed: true))
            {
                Logger.Info("View is shown...");
                this.view.Show();
            }

            if (!this.isSleeping && this.SoundEnabled && this.AnyRingingAlarms())
            {               
                if (!this.isAlarmRinging)
                {                    
                    Logger.Info("Alarm is ringing...");
                    this.isAlarmRinging = true;
                    this.view.PlayAlarm();
                }                    
            }
            else if (this.isAlarmRinging)
            {
                Logger.Info("Alarm is not ringing...");
                this.isAlarmRinging = false;
                this.view.StopAlarm();
            }
        }

        private bool AnyRingingAlarms(bool includeMissed=false)
        {
            return this.alarmService.List().Any(alarm => alarm.IsRinging || (includeMissed && alarm.Status == AlarmStatus.Missed));
        }

        private void Wakeup()
        {
            this.isSleeping = false;
            this.alarmService.Start();
            this.alarmReminderManager.Start();
            this.CheckForAlarms();
        }

        private void Sleep()
        {
            this.isSleeping = true;
            this.CheckForAlarms();
            this.alarmService.Stop();
            this.alarmReminderManager.Stop();            
        }

        private void alarmReminderManager_OnRequestNotification(object sender, AlarmReminderEventArgs e)
        {
            this.view.ShowReminder(e.Caption, e.Message);
        }

        #region View Events
        private void view_OnSmartAlarmOff(object sender, EventArgs e)
        {
            Logger.Info("Smart alarm is disabled.");
            this.SmartAlarmEnabled = false;
            this.soundEnabled = this.view.SoundEnabled;
            this.Wakeup();
        }

        private void view_OnSmartAlarmOn(object sender, EventArgs e)
        {
            Logger.Info("Smart alarm is enabled.");
            this.SmartAlarmEnabled = true;
        }

        private void view_OnPopupOnAlarmOff(object sender, EventArgs e)
        {
            Logger.Info("List is set not to popup on alarm");
            this.PopupOnAlarmEnabled = false;
        }

        private void view_OnPopupOnAlarmOn(object sender, EventArgs e)
        {
            Logger.Info("List is set to popup on alarm");
            this.PopupOnAlarmEnabled = true;
        }

        private void view_OnDisableSound(object sender, EventArgs e)
        {
            Logger.Info("Sound is disabled.");
            this.SoundEnabled = false;

            this.CheckForAlarms();
        }

        private void view_OnEnableSound(object sender, EventArgs e)
        {
            Logger.Info("Sound is enabled.");
            this.SoundEnabled = true;

            this.CheckForAlarms();
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

        private void view_OnImportRequest(object sender, EventArgs e)
        {
            var context = this.view.AskImport();
            if (context == null)
            {
                return;
            }

            context.DateFormat = settings.ImportDateFormat;
            context.CaptionFormat = settings.ImportCaptionFormat;
            context.CaptionPatterns = settings.ImportCaptionPatterns;

            Logger.Info("Starting import");
            this.importer.Import(context);            
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
            Logger.Info(alarm + " is hushed.");
            alarm.IsHushed = true;
            this.alarmService.Update(alarm);
        }

        private void view_OnUnhushRequest(object sender, AlarmEventArgs e)
        {
            var alarm = e.Alarm;
            Logger.Info(alarm + " is un-hushed.");
            alarm.IsHushed = false;
            this.alarmService.Update(alarm);
        }

        private void view_OnExitRequest(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void view_OnShowRequest(object sender, EventArgs e)
        {
            Logger.Info("List is visible.");
            this.view.Show();

        }

        private void view_OnHideRequest(object sender, EventArgs e)
        {
            Logger.Info("List is hidden.");
            this.view.Hide();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Threading.Timer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void view_OnLoad(object sender, EventArgs e)
        {
            this.view.PopupOnAlarm = this.PopupOnAlarmEnabled;
            this.view.SoundEnabled = this.SoundEnabled;
            this.view.SmartAlarm = this.SmartAlarmEnabled;

            this.Wakeup();

            if (this.settings.StartHidden)
            {
                this.view_OnHideRequest(null, null);                
            }
        }

        private void view_OnClosing(object sender, EventArgs e)
        {
            Logger.Info("Closing...");
            this.Sleep();
        }
        #endregion

        #region Alarm service Events
        private void alarmService_OnAlarmUpdate(object sender, AlarmEventArgs e)
        {
            this.view.UpdateAlarm(e.Alarm);
            this.CheckForAlarms();
        }

        private void alarmService_OnAlarmRemoval(object sender, AlarmEventArgs e)
        {
            this.view.RemoveAlarm(e.Alarm);
            this.CheckForAlarms();
        }

        private void alarmService_OnAlarmAdd(object sender, AlarmEventArgs e)
        {
            this.view.AddAlarm(e.Alarm);
            this.CheckForAlarms();
        }

        private void alarmService_OnAlarmStatusChange(object sender, AlarmEventArgs e)        
        {
            this.view.UpdateAlarm(e.Alarm);
            this.CheckForAlarms();
        }
        #endregion

        #region AlarmManager Events

        private void alarmManager_OnWakeup(object sender, EventArgs e)
        {
            if (!this.smartAlarmEnabled)
                return; 

            Logger.Info("Smart Alarm sent wakeup");
            this.Wakeup();
        }

        private void alarmManager_OnSleep(object sender, EventArgs e)
        {
            if (!this.smartAlarmEnabled)
                return; 

            Logger.Info("Smart Alarm sent sleep");
            this.Sleep();
            
        }
        #endregion
    }
}
