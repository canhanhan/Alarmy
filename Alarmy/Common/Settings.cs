using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Alarmy.Common
{
    internal class Settings
    {
        public string AlarmSoundFile { get; protected set; }
        public int CheckInterval { get; protected set; }
        public bool EnableSound { get; protected set; }
        public bool PopupOnAlarm { get; protected set; }
        public bool SmartAlarm { get; protected set; }
        public bool StartHidden { get; protected set; }
        public int AlarmListGroupInterval { get; protected set; }
        public string AlarmDatabasePath { get; protected set; }
        public int FreshnessInMinutes { get; protected set; }
        public bool RemindAllAlarms { get; protected set; }
        public int ReminderInterval { get; protected set; }
        public int RepositoryInterval { get; protected set; }
        public string DatePickerFormat { get; protected set; }
        public string ImportDateFormat { get; protected set; }
        public string ImportCaptionFormat { get; protected set; }
        public string[] ImportCaptionPatterns { get; protected set; }
        
        public Settings()
        {
            this.AlarmSoundFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Properties.Settings.Default.AlarmSoundFile);
            this.CheckInterval = Properties.Settings.Default.CheckInterval;
            this.EnableSound = Properties.Settings.Default.EnableSound;
            this.PopupOnAlarm = Properties.Settings.Default.PopupOnAlarm;
            this.SmartAlarm = Properties.Settings.Default.SmartAlarm;
            this.StartHidden = Properties.Settings.Default.StartHidden;
            this.AlarmListGroupInterval = Properties.Settings.Default.AlarmListGroupInterval;
            this.AlarmDatabasePath = Environment.ExpandEnvironmentVariables(Properties.Settings.Default.AlarmDatabasePath);
            this.FreshnessInMinutes = Properties.Settings.Default.FressnessInMinutes;
            this.RemindAllAlarms = Properties.Settings.Default.RemindAllAlarms;
            this.ReminderInterval = Properties.Settings.Default.ReminderInterval;
            this.RepositoryInterval = Properties.Settings.Default.RepositoryInterval;
            this.DatePickerFormat = Properties.Settings.Default.DatePickerFormat;
            this.ImportDateFormat = Properties.Settings.Default.ImportDateFormat;
            this.ImportCaptionFormat = Properties.Settings.Default.ImportCaptionFormat;
            this.ImportCaptionPatterns = Properties.Settings.Default.ImportCaptionPatterns.Cast<string>().ToArray();
        }

        public override string ToString()
        {
            return string.Join(";", this.GetType().GetProperties(System.Reflection.BindingFlags.Public & System.Reflection.BindingFlags.Instance).Select(x => x.Name + "=" + x.GetValue(this, null)));
        }        
    }
}
