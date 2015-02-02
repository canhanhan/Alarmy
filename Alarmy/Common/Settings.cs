using System;
using System.Globalization;
using System.IO;

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

        public Settings()
        {
            this.AlarmSoundFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "alarm.wav");
            this.CheckInterval = 1;
            this.EnableSound = true;
            this.PopupOnAlarm = true;
            this.SmartAlarm = true;
            this.StartHidden = false;
            this.AlarmListGroupInterval = 15;
            this.AlarmDatabasePath = Environment.ExpandEnvironmentVariables("%TEMP%\\alarms.db");
            this.FreshnessInMinutes = 120;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "alarmSoundFile={0};checkInterval={1};enableSound={2};popupOnAlarm={3};smartAlarm={4};alarmListGroupInterval={5};alarmDatabasePath={6};freshnessInMinutes={7}",
                this.AlarmSoundFile, this.CheckInterval, this.EnableSound, this.PopupOnAlarm, this.SmartAlarm, this.AlarmListGroupInterval, this.AlarmDatabasePath, this.FreshnessInMinutes);
        }        
    }
}
