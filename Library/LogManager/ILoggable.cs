using System;


namespace Microarea.Library.LogManager
{
	/// <summary>
	/// ILoggable.
	/// </summary>
	//=========================================================================
	public interface ILoggable
	{
		event EventHandler<Loggables.LoggableEventArgs> Event;
	}

	[Flags]
	//=========================================================================
	public enum EventTypes
	{
		None		=	0x0,
		Error		=	0x1,
		Information	=	0x2,
		Success		=	0x4,
		Warning		=	0x8
	}
}
