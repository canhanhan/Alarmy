using NDesk.Options;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Alarmy.Infrastructure
{
    internal class CommandLineArgsSettings : Settings
    {
        public CommandLineArgsSettings() : base()
        {
            new OptionSet()
            {
                { "a|alarmSound=", x => this.AlarmSoundFile = x },
                { "c|checkInterval=", x => this.CheckInterval = double.Parse(x, CultureInfo.InvariantCulture)},
                { "m|mute", x => this.EnableSound = false },
                { "np|dontPopup", x => this.PopupOnAlarm = false },
                { "ns|noSmartAlarm", x => this.SmartAlarm = false },
                { "h|hidden", x => this.StartHidden = true },
                { "g|alarmListGroupInterval=", x => this.AlarmListGroupInterval = int.Parse(x, CultureInfo.InvariantCulture)},
                { "db|database=", x => this.AlarmDatabasePath = x },
                { "f|freshness=", x => this.Freshness = int.Parse(x, CultureInfo.InvariantCulture)} 
            }.Parse(Environment.GetCommandLineArgs().Skip(1));
        }
    }

    internal class Settings
    {
        public string AlarmSoundFile { get; protected set; }
        public double CheckInterval { get; protected set; }
        public bool EnableSound { get; protected set; }
        public bool PopupOnAlarm { get; protected set; }
        public bool SmartAlarm { get; protected set; }
        public bool StartHidden { get; protected set; }
        public int AlarmListGroupInterval { get; protected set; }
        public string AlarmDatabasePath { get; protected set; }
        public int Freshness { get; protected set; }

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
            this.Freshness = 120;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "alarmSoundFile={0};checkInterval={1};enableSound={2};popupOnAlarm={3};smartAlarm={4};alarmListGroupInterval={5};alarmDatabasePath={6}",
                this.AlarmSoundFile, this.CheckInterval, this.EnableSound, this.PopupOnAlarm, this.SmartAlarm, this.AlarmListGroupInterval, this.AlarmDatabasePath);
        }        
    }
}
