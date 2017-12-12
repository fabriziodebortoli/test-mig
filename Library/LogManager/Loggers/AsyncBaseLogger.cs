using System;
using System.Threading;


namespace Microarea.Library.LogManager.Loggers
{
	/// <summary>
	/// AsyncBaseLogger.
	/// </summary>
	//=========================================================================
	public class AsyncBaseLogger : BaseLogger
	{
		private readonly object instanceLockTicket = new object();

		//---------------------------------------------------------------------
		public AsyncBaseLogger()
		{}

		//---------------------------------------------------------------------
		public override void Log(ILoggableEventDescriptor eventDescriptor)
		{
			lock (instanceLockTicket)
			{
				if (Object.ReferenceEquals(eventDescriptor, null))
					throw new ArgumentNullException("eventDescriptor", "'eventDescriptor' cannot be null");

				ILogger bubblingLogger = null;
				if (eventDescriptor.LoggableEventInfo != null)
					bubblingLogger = eventDescriptor.LoggableEventInfo.BubblingLogger;

				if (!PropagateEventToChildren || Loggers == null || Loggers.Count == 0)
					return;

				foreach (ILogger logWriter in Loggers)
				{
					if (
						Object.ReferenceEquals(bubblingLogger, null) ||
						!Object.ReferenceEquals(bubblingLogger, logWriter)
						)
					{
						new Thread(
							new ThreadStart(
								() => { logWriter.Log(eventDescriptor); }
								)
							).Start();
					}
				}
			}
		}
	}
}
