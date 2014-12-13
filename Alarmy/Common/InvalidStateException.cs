using System;

namespace Alarmy.Common
{
    public abstract class InvalidStateException : ApplicationException { }

    public class CancelledInvalidStateException : InvalidStateException
    {
        internal protected const string STR_CancelledStatusError = "Alarm status cannot be changed when cancelled";

        public override string ToString()
        {
            return STR_CancelledStatusError;
        }
    }

    public class CompletedInvalidStateException : InvalidStateException
    {
        internal protected const string STR_CompletedStatusError = "Alarm status cannot be changed when completed";

        public override string ToString()
        {
            return STR_CompletedStatusError;
        }
    }

    public class SnoozedInvalidStateException : InvalidStateException
    {
        internal protected const string STR_SnoozedStatusError = "Snoozed alarm can only be cancelled, completed.";

        public override string ToString()
        {
            return STR_SnoozedStatusError;
        }
    }

    public class RingingInvalidStateException : InvalidStateException
    {
        internal protected const string STR_RingingStatusError = "Ringing alarm can only be cancelled, completed.";

        public override string ToString()
        {
            return STR_RingingStatusError;
        }
    }

    public class SetInvalidStateException : InvalidStateException
    {
        internal protected const string STR_SetStatusError = "Set alarm can only be cancelled or ringing.";

        public override string ToString()
        {
            return STR_SetStatusError;
        }
    }

    public class MissedInvalidStateException : InvalidStateException
    {
        internal protected const string STR_MissedStatusError = "Missed alarm can only be cancelled or ringing.";

        public override string ToString()
        {
            return STR_MissedStatusError;
        }
    }
}
