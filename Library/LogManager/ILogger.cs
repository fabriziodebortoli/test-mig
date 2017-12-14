using System;


namespace Microarea.Library.LogManager
{
	/// <summary>
	/// ILogger.
	/// </summary>
	//=========================================================================
	public interface ILogger
	{
		event EventHandler<Loggables.LoggableEventArgs> BubbleLoggableEvent;

		//---------------------------------------------------------------------
		void Log(ILoggableEventDescriptor eventDescriptor);

		//---------------------------------------------------------------------
		ILogger ListenTo(ILoggable loggable);

		//---------------------------------------------------------------------
		ILogger StopListeningTo(ILoggable loggable);

		//---------------------------------------------------------------------
		ILogger AddLogWriter(ILogger logger);

		//---------------------------------------------------------------------
		ILogger RemoveLogWriter(ILogger logger);
	}
}
