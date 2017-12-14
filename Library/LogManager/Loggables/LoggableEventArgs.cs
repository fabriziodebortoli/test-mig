using System;


namespace Microarea.Library.LogManager.Loggables
{
	/// <summary>
	/// LoggableEvent.
	/// </summary>
	//=========================================================================
	public class LoggableEventArgs :
		System.EventArgs,
		ILoggableEventDescriptor,
		ICloneable
	{
		private const string EventTag = "Event";

		private int eventCode = -1;
		private EventTypes eventType = EventTypes.None;
		private string message = string.Empty;
		private DateTime eventTimestamp = DateTime.Now;
		private ILoggableEventInfo loggableEventInfo;

		//---------------------------------------------------------------------
		public int EventCode
		{
			get
			{
				return this.eventCode;
			}
		}

		//---------------------------------------------------------------------
		public EventTypes EventType
		{
			get
			{
				return this.eventType;
			}
		}
		
		//---------------------------------------------------------------------
		public string Message
		{
			get
			{
				return this.message;
			}
		}

		//---------------------------------------------------------------------
		public DateTime EventTimestamp
		{
			get
			{
				return this.eventTimestamp;
			}
		}

		//---------------------------------------------------------------------
		public ILoggableEventInfo LoggableEventInfo
		{
			get
			{
				return this.loggableEventInfo;
			}
			set
			{
				this.loggableEventInfo = value;
			}
		}

		//---------------------------------------------------------------------
		public LoggableEventArgs()
			:this(-1, EventTypes.None, string.Empty)
		{}

		//---------------------------------------------------------------------
		public LoggableEventArgs(
			int eventCode,
			EventTypes eventType,
			string message
			)
		{
			this.eventCode	= eventCode;
			this.eventType	= eventType;
			this.message	= message;
			this.eventTimestamp		= DateTime.Now;
			this.loggableEventInfo = new LoggableEventInfo();
		}

		//---------------------------------------------------------------------
		protected LoggableEventArgs(LoggableEventArgs le)
		{
			if (le != null)
			{
				this.eventCode = le.eventCode;
				this.eventType = le.eventType;
				this.message = le.message;
				this.eventTimestamp = le.eventTimestamp;

				if (le.loggableEventInfo != null)
					this.loggableEventInfo = le.loggableEventInfo.Clone() as ILoggableEventInfo;
			}
		}

		//---------------------------------------------------------------------
		public virtual object Clone()
		{
			return new LoggableEventArgs(this);
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public virtual string Serialize(IMessageFormatter formatter)
		{
			if (formatter == null)
				throw new ArgumentNullException("formatter", "Unable to serialize if 'formatter' is null");

			formatter.OpenRoot(EventTag);
			formatter.AddElementWithValue(
				"Timestamp",
				eventTimestamp,
				"dd/MM/yyyy HH:mm:ss.fff"
				);

			if (loggableEventInfo != null && loggableEventInfo.EventId > -1)
				formatter.AddElementWithValue(
					"id",
					loggableEventInfo.EventId
					);

			if (eventCode > -1)
				formatter.AddElementWithValue(
					"code",
					eventCode
					);

			formatter.AddElementWithValue(
					"type",
					EventType
					);

			formatter.AddElementWithValue(
					"message",
					message
					);

			formatter.CloseRoot();

			return formatter.ToString();
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			LoggableEventArgs e = obj as LoggableEventArgs;

			if (e == null)
				return false;

			return
				eventCode == e.eventCode &&
				eventType == e.eventType &&
				String.Compare(message, e.message, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				eventTimestamp == e.eventTimestamp &&
				loggableEventInfo == e.loggableEventInfo;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return
				eventCode.GetHashCode() +
				eventType.GetHashCode() +
				((message != null) ? message.GetHashCode() : 0) +
				eventTimestamp.GetHashCode() +
				((loggableEventInfo != null) ? loggableEventInfo.GetHashCode() : 0);
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return message ?? string.Empty;
		}
	}
}
