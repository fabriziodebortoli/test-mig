using System;
using System.IO;
using System.Security.Permissions;


namespace Microarea.Library.LogManager.Loggers
{
	/// <summary>
	/// StreamManager.
	/// </summary>
	//=========================================================================
	internal class StreamManager : IDisposable
	{
		private static readonly object staticLockTicket = new object();
		private static StreamManager streamManager;

		private readonly object instanceLockTicket = new object();
		private StreamWriter streamWriter;

		//---------------------------------------------------------------------
		protected StreamManager(StreamWriter streamWriter)
		{
			this.streamWriter = streamWriter;
		}

		//---------------------------------------------------------------------
		public virtual void WriteLine(string line)
		{
			lock (instanceLockTicket)
			{
				if (streamWriter != null)//è null se è stato disposato
				{
					streamWriter.WriteLine(line);
					streamWriter.Flush();
				}
			}
		}

		//---------------------------------------------------------------------
		[SecurityPermission(SecurityAction.Demand)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "streamManager will be disposed by the calling object")]
		public static StreamManager GetStreamManager(string path)
		{
			lock (staticLockTicket)
			{
				if (streamManager == null)
				{
					streamManager = new StreamManager(new StreamWriter(path, true));
					AppDomain.CurrentDomain.DomainUnload += new EventHandler(DomainUnload);
				}

				return streamManager;
			}
		}

		//---------------------------------------------------------------------
		private static void DomainUnload(object sender, EventArgs e)
		{
			if (streamManager != null)
			{
				AppDomain.CurrentDomain.DomainUnload -= new EventHandler(DomainUnload);
				streamManager.Dispose();
				streamManager = null;
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
				streamWriter.Close();
				streamWriter = null;
			}
		}

		#endregion
	}
	/// <summary>
	/// TextFileLogger.
	/// </summary>
	//=========================================================================
	public class TextFileLogger : BaseLogger
	{
		private StreamManager streamManager;
		private readonly object instanceLockTicket = new object();

		//---------------------------------------------------------------------
		public TextFileLogger(string path)
			:this(path, (EventTypes.Error | EventTypes.Information | EventTypes.Success | EventTypes.Warning))
		{}

		//---------------------------------------------------------------------
		public TextFileLogger(string path, EventTypes eventsFilter)
		{
			EventsFilter	= eventsFilter;
			streamManager	= StreamManager.GetStreamManager(path);
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

				streamManager.WriteLine(eventDescriptor.Serialize(MessageFormatter));
			}
		}

		#endregion
	}
}
