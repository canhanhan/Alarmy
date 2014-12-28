using NLog;
using NLog.LayoutRenderers;
using System.IO;
using System.Text;

namespace Alarmy.Infrastructure
{
    [LayoutRenderer("databasePath")]
    class DatabasePathLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(Path.GetDirectoryName(Program.Container.Resolve<Settings>().AlarmDatabasePath));
        }
    }
}
