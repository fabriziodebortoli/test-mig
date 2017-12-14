using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotificationService.Exceptions
{
    public class NotificationConnectionFailedException : Exception
    {
        public NotificationConnectionFailedException() { }
        public NotificationConnectionFailedException(string message) : base(message) { }
        public NotificationConnectionFailedException(string message, Exception inner) : base(message, inner) { }
    }
}