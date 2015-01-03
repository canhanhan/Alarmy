using NLog;
using NLog.LayoutRenderers;
using System.IO;
using System.Text;

namespace Alarmy.Infrastructure
{
    [LayoutRenderer("databasePath")]
    internal class DatabasePathLayoutRenderer : LayoutRenderer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(Path.GetDirectoryName(Program.Container.Resolve<Settings>().AlarmDatabasePath));
        }
    }
}
