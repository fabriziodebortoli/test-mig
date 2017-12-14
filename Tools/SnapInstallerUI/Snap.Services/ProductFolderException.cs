using System;
using System.Runtime.Serialization;

namespace Microarea.Snap.Services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    [Serializable]
    internal class ProductFolderException : Exception
    {
        public ProductFolderException()
        {
        }

        public ProductFolderException(string message) : base(message)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public ProductFolderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ProductFolderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}