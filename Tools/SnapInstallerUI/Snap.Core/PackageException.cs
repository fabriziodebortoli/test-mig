using System;
using System.Runtime.Serialization;

namespace Microarea.Snap.Core
{
    [Serializable]
    public class PackageException : Exception, ISerializable
    {
        public PackageException()
        {
        }

        public PackageException(string message) : base(message)
        {
        }

        public PackageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PackageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}