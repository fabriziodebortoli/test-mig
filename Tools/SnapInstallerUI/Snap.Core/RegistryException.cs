using System;
using System.Runtime.Serialization;

namespace Microarea.Snap.Core
{
    [Serializable]
    public class RegistryException : Exception
    {
        public RegistryException()
        {
        }

        public RegistryException(string message) : base(message)
        {
        }

        public RegistryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RegistryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}