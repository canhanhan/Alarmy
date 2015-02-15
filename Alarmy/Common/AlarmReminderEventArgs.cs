using System;

namespace Alarmy.Common
{
    internal class AlarmReminderEventArgs : EventArgs
    {
        public string Caption { get; private set; }
        public string Message { get; private set; }

        public AlarmReminderEventArgs(string caption, string message)
        {
            if (caption == null)
                throw new ArgumentNullException("caption");

            if (message == null)
                throw new ArgumentNullException("message");

            this.Caption = caption;
            this.Message = message;
        }
    }
}
