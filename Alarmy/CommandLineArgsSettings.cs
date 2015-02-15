using Alarmy.Common;
using Alarmy.Infrastructure;
using NDesk.Options;
using System;
using System.Globalization;
using System.Linq;

namespace Alarmy
{
    internal class CommandLineArgsSettings : Settings
    {
        public CommandLineArgsSettings() : base()
        {
            new OptionSet()
            {
                { "a|alarmSound=", x => this.AlarmSoundFile =  Environment.ExpandEnvironmentVariables(x) },
                { "c|checkInterval=", x => this.CheckInterval = int.Parse(x, CultureInfo.InvariantCulture)},
                { "m|mute", _ => this.EnableSound = false },
                { "np|dontPopup", _ => this.PopupOnAlarm = false },
                { "ns|noSmartAlarm", _ => this.SmartAlarm = false },
                { "h|hidden", _ => this.StartHidden = true },
                { "g|alarmListGroupInterval=", x => this.AlarmListGroupInterval = int.Parse(x, CultureInfo.InvariantCulture)},
                { "db|database=", x => this.AlarmDatabasePath =  Environment.ExpandEnvironmentVariables(x) },
                { "f|freshness=", x => this.FreshnessInMinutes = int.Parse(x, CultureInfo.InvariantCulture)},
                { "ra|remindAll", _ => this.RemindAllAlarms = true },
                { "ri|reminderInterval=", x => this.ReminderInterval = int.Parse(x, CultureInfo.InvariantCulture)},
                { "rr|repositoryInterval=", x => this.RepositoryInterval = int.Parse(x, CultureInfo.InvariantCulture)}
            }.Parse(Environment.GetCommandLineArgs().Skip(1));
        }
    }
}
