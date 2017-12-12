using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotificationService.Exceptions
{
    public class NotificationNullException : Exception
    {
        public NotificationNullException() { }
        public NotificationNullException(string message) : base(message) { }
        public NotificationNullException(string message, Exception inner) : base(message, inner) { }
    }
}