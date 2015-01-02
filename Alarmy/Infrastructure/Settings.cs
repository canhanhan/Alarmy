using NDesk.Options;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Alarmy.Infrastructure
{
    class Settings
    {
        public string AlarmSoundFile { get; private set; }
        public double CheckInterval { get; private set; }
        public bool EnableSound { get; private set; }
        public bool PopupOnAlarm { get; private set; }
        public bool SmartAlarm { get; private set; }
        public bool StartHidden { get; private set; }
        public int AlarmListGroupInterval { get; set; }
        public string AlarmDatabasePath { get; private set; }

        public Settings()
        {
            this.AlarmSoundFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "alarm.wav");
            this.CheckInterval = 1000;
            this.EnableSound = true;
            this.PopupOnAlarm = true;
            this.SmartAlarm = true;
            this.StartHidden = false;
            this.AlarmListGroupInterval = 15;
            this.AlarmDatabasePath = Environment.ExpandEnvironmentVariables("%TEMP%\\alarms.db");

            new OptionSet()
            {
                { "a|alarmSound=", x => this.AlarmSoundFile = x },
                { "c|checkInterval=", x => this.CheckInterval = double.Parse(x, CultureInfo.InvariantCulture)},
                { "m|mute", x => this.EnableSound = false },
                { "np|dontPopup", x => this.PopupOnAlarm = false },
                { "ns|noSmartAlarm", x => this.SmartAlarm = false },
                { "h|hidden", x => this.StartHidden = true },
                { "g|alarmListGroupInterval=", x => this.AlarmListGroupInterval = int.Parse(x, CultureInfo.InvariantCulture)},
                { "db|database=", x => this.AlarmDatabasePath = x }
            }.Parse(Environment.GetCommandLineArgs().Skip(1));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "alarmSoundFile={0};checkInterval={1};enableSound={2};popupOnAlarm={3};smartAlarm={4};alarmListGroupInterval={5};alarmDatabasePath={6}",
                this.AlarmSoundFile, this.CheckInterval, this.EnableSound, this.PopupOnAlarm, this.SmartAlarm, this.AlarmListGroupInterval, this.AlarmDatabasePath);
        }
    }
}
