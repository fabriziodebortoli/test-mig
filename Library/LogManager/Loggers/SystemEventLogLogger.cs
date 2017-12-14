using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;


namespace Microarea.Library.LogManager.Loggers
{
	/// <summary>
	/// EventLogManager.
	/// </summary>
	//=========================================================================
	internal class EventLogManager : IDisposable
	{
		private static readonly object staticLockTicket = new object();
		private static EventLogManager eventLogManager;

		private readonly object instanceLockTicket = new object();
		private EventLog eventLog;

		//---------------------------------------------------------------------
		protected EventLogManager(EventLog eventLog)
		{
			this.eventLog = eventLog;
		}

		//---------------------------------------------------------------------
		public virtual void WriteEntry(
			string message,
			EventTypes eventType,
			int logId
			)
		{
			lock (instanceLockTicket)
			{
				EventLogEntryType eventLogEntryType = EventLogEntryType.Error;
				switch (eventType)
				{
					case EventTypes.Information:
						eventLogEntryType = EventLogEntryType.Information;
						break;
					case EventTypes.Success:
						eventLogEntryType = EventLogEntryType.SuccessAudit;
						break;
					case EventTypes.Warning:
						eventLogEntryType = EventLogEntryType.Warning;
						break;
				}
				// TEMP: Errore se logId > 65536!
				//eventLog.WriteEntry(message, eventLogEntryType, logId);
				eventLog.WriteEntry(message, eventLogEntryType);
			}
		}

		//---------------------------------------------------------------------
		public virtual int EntriesCount
		{
			get
			{
				lock (instanceLockTicket)
				{
					return eventLog.Entries.Count;
				}
			}
		}

		//---------------------------------------------------------------------
		[SecurityPermission(SecurityAction.Demand)]
		public static EventLogManager GetEventLogManager(
			string logName,
			string sourceName
			)
		{
			lock (staticLockTicket)
			{
				if (eventLogManager == null)
				{
					eventLogManager = new EventLogManager(GetEventLog(logName, sourceName));
					AppDomain.CurrentDomain.DomainUnload += new EventHandler(DomainUnload);
				}

				return eventLogManager;
			}
		}

		//---------------------------------------------------------------------
		[SecurityPermission(SecurityAction.LinkDemand)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000", Justification="eventLog will be disposed by the calling object")]
		private static EventLog GetEventLog(
			string logName,
			string sourceName
			)
		{
			bool ok = EventLog.SourceExists(sourceName);

			if (!ok)
			{
				try
				{
					EventLog.CreateEventSource(sourceName, logName);
				}
				catch (Exception exc)
				{
					throw new NotExistingSourceException(
						sourceName,
						String.Concat(
							"'",
							sourceName,
							"' does not exist in the system log."
							),
							exc
						);
				}
			}

			EventLog eventLog = new EventLog();
			eventLog.Log = logName;
			eventLog.Source = sourceName;

			return eventLog;
		}

		//---------------------------------------------------------------------
		private static void DomainUnload(object sender, EventArgs e)
		{
			if (eventLogManager != null)
			{
				AppDomain.CurrentDomain.DomainUnload -= new EventHandler(DomainUnload);
				eventLogManager.Dispose();
				eventLogManager = null;
			}
		}

		#region IDisposable Members

		//---------------------------------------------------------------------
		public void Dispose()
		{
			lock (instanceLockTicket)
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

		//---------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				eventLog.Dispose();
				eventLog = null;
			}
		}

		#endregion
	}

	/// <summary>
	/// SystemEventLogLogger.
	/// </summary>
	//=========================================================================
	public class SystemEventLogLogger : BaseLogger
	{
		private readonly object instanceLockTicket = new object();
		private EventLogManager eventLogManager;

		/// <remarks>
		/// Lancia eccezioni del tipo:
		/// <code>NotExistingSourceException</code> se il source per l'event
		/// logging non esiste;
		/// Tutte le eccezioni lanciate dal metodo <code>EventLog.SourceExists</code>;
		/// Tutte le eccezioni lanciate dai costruttori della classe <code>EventLog</code>
		/// </remarks>
		//---------------------------------------------------------------------
		public SystemEventLogLogger(
			string logName,
			string sourceName
			)
			: this(logName, sourceName, (EventTypes.Error | EventTypes.Information | EventTypes.Success | EventTypes.Warning))
		{}

		/// <remarks>
		/// Lancia eccezioni del tipo:
		/// <code>NotExistingSourceException</code> se il source per l'event
		/// logging non esiste;
		/// Tutte le eccezioni lanciate dal metodo <code>EventLog.SourceExists</code>;
		/// Tutte le eccezioni lanciate dai costruttori della classe <code>EventLog</code>
		/// </remarks>
		//---------------------------------------------------------------------
		public SystemEventLogLogger(
			string logName,
			string sourceName,
			EventTypes eventsFilter
			)
		{
			EventsFilter	= eventsFilter;
			eventLogManager = EventLogManager.GetEventLogManager(logName, sourceName);
		}

		#region ILogger Members

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public override void Log(ILoggableEventDescriptor eventDescriptor)
		{
			lock (instanceLockTicket)
			{
				if (Object.ReferenceEquals(eventDescriptor, null))
					throw new ArgumentNullException("eventDescriptor", "'eventDescriptor' cannot be null");

				base.Log(eventDescriptor);

				if (!MatchMyEventsFilter(eventDescriptor.EventType))
					return;

				eventLogManager.WriteEntry(
					eventDescriptor.Serialize(MessageFormatter),
					eventDescriptor.EventType,
					eventDescriptor.LoggableEventInfo.EventId
					);
			}
		}

		#endregion
	}

	//=========================================================================
	[Serializable]
	public class NotExistingSourceException : Exception
	{
		private const string sourceNameKey = "sourceName";
		private string sourceName;

		//---------------------------------------------------------------------
		public string SourceName
		{
			get
			{
				return sourceName;
			}
		}

		//---------------------------------------------------------------------
		public NotExistingSourceException()
			: this (string.Empty, string.Empty, null)
		{}

		//---------------------------------------------------------------------
		public NotExistingSourceException(string message)
			: this (string.Empty, message, null)
		{}

		//---------------------------------------------------------------------
		public NotExistingSourceException(string message, Exception innerException)
			: this (string.Empty, message, innerException)
		{}

		//---------------------------------------------------------------------
		public NotExistingSourceException(string sourceName, string message)
			: this (sourceName, message, null)
		{}

		//---------------------------------------------------------------------
		public NotExistingSourceException(
			string sourceName,
			string message,
			Exception innerException
			)
			: base (message, innerException)
		{
			this.sourceName = sourceName;
		}

		// Needed for xml serialization.
		//---------------------------------------------------------------------
		protected NotExistingSourceException(
			SerializationInfo info,
			StreamingContext context
			)
			: base (info, context)
		{
			if (!Object.ReferenceEquals(info, null))
				this.sourceName = info.GetString(sourceNameKey);
		}

		//---------------------------------------------------------------------
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public override void GetObjectData(
			SerializationInfo info,
			StreamingContext context
			)
		{
			if (Object.ReferenceEquals(info, null))
				throw new ArgumentNullException("info", "'info' cannot be null");

			base.GetObjectData (info, context);
            info.AddValue(sourceNameKey, this.sourceName);			
		}
	}
}
