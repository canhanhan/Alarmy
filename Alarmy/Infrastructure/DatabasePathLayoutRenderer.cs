using NLog;
using NLog.LayoutRenderers;
using System.Text;

namespace Alarmy.Infrastructure
{
    [LayoutRenderer("databasePath")]
    class DatabasePathLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(Program.Container.Resolve<Settings>().AlarmDatabasePath);
        }
    }
}
