
namespace Microarea.Library.LogManager.Loggables
{
	/// <summary>
	/// LoggableEventInfo.
	/// </summary>
	//=========================================================================
	internal class LoggableEventInfo : ILoggableEventInfo
	{
		private static readonly object staticLockTicket = new object();
		private static int eventIdCounter = 0;
		//---------------------------------------------------------------------
		private static int GetEventId()
		{
			lock (staticLockTicket)
			{
				return (++eventIdCounter);
			}
		}

		private	ILogger	bubblingLogger;
		private	int		eventId;

		//---------------------------------------------------------------------
		public LoggableEventInfo()
			: this (null)
		{}

		//---------------------------------------------------------------------
		public LoggableEventInfo(ILogger bubblingLogger)
		{
			this.bubblingLogger	= bubblingLogger;
			this.eventId		= GetEventId();
		}

		#region ILoggableEventInfo Members

		//---------------------------------------------------------------------
		public ILogger BubblingLogger
		{
			get
			{
				return this.bubblingLogger;
			}
			set
			{
				this.bubblingLogger = value;
			}
		}

		//---------------------------------------------------------------------
		public int EventId
		{
			get
			{
				return this.eventId;
			}
		}

		#endregion

		#region ICloneable Members

		//---------------------------------------------------------------------
		public object Clone()
		{
			LoggableEventInfo lei = new LoggableEventInfo(this.bubblingLogger);
			lei.eventId = this.eventId;

			return lei;
		}

		#endregion
	}
}
