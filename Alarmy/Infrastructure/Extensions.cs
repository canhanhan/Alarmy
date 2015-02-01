using System;
using System.Reflection;
using System.Windows.Forms;

namespace Alarmy
{
    internal static class Extensions
    {
        public static DateTime RoundToMinute(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }

        // http://stackoverflow.com/a/7029464
        public static DateTime Roundup(this DateTime dateTime, TimeSpan roundingValue)
        {
            return new DateTime(((dateTime.Ticks + roundingValue.Ticks) / roundingValue.Ticks) * roundingValue.Ticks);
        }

        // http://stackoverflow.com/a/15268338
        public static void DoubleBuffered(this Control control, bool enable)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            var doubleBufferPropertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            doubleBufferPropertyInfo.SetValue(control, enable, null);
        }

        public static void Invoke<T>(this EventHandler<T> handler, object sender = null, T e = null) where T: EventArgs
        {
            if (handler != null)
                handler.Invoke(sender, e);
        }

        public static void Invoke(this EventHandler handler, object sender = null, EventArgs e = null) 
        {
            if (handler != null)
                handler.Invoke(sender, e);
        }
    }
}
