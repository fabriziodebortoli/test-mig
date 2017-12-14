using System;
using System.Runtime.Serialization;

namespace Microarea.Snap.Services
{
    [Serializable]
    public class LaunchProcessException : Exception
    {
        public LaunchProcessException()
        {
        }

        public LaunchProcessException(string message) : base(message)
        {
        }

        public LaunchProcessException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LaunchProcessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}