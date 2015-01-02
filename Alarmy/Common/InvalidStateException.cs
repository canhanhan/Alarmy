using System;
using System.Runtime.Serialization;

namespace Alarmy.Common
{
    [Serializable]
    public class InvalidStateException : Exception
    {
      public InvalidStateException() : base() { }
      public InvalidStateException(string message): base(message) {}
      public InvalidStateException(string message, Exception innerException): base (message, innerException) {}
      protected InvalidStateException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}
