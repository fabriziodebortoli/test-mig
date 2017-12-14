using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotificationService.Exceptions
{
    public class NotificationProcessException : Exception
    {
        public NotificationProcessException() { }
        public NotificationProcessException(string message) : base(message) { }
        public NotificationProcessException(string message, Exception inner) : base(message, inner) { }
    }
}