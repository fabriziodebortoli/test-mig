using System;

namespace Microarea.Snap.Services
{
    public class ErrorOccurredEventArgs : NotificationEventArgs
    {
        public Exception Exception { get; set; }
    }
}