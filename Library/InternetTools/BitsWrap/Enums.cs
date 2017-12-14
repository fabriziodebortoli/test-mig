using System;

namespace Microarea.Library.Internet.BitsWrap
{
	public enum JobType : int
	{
        CurrentUser = 0,
        AllUsers = 1
    }

    [Flags()]
	public enum NotificationTypes : int
	{
        JobTransferred = 1,
        JobError = 2,
        JobModification = 8
    }

    public enum JobPriority : int
	{
        Foreground = 0,
        High = 1,
        Normal = 2,
        Low = 3
    }

    public enum JobState : int
	{
        Queued = 0,
        Connecting = 1,
        Transferring = 2,
        Suspended = 3,
        Errors = 4,
        TransientError = 5,
        Transferred = 6,
        Acknowledged = 7,
        Cancelled = 8
    }

    public enum ProxyUsage : int
	{
        NoProxy = 1,
        Override = 2,
        Preconfig = 0
    }

}
