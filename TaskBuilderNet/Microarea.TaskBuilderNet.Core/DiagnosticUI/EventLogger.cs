using System;
using System.Diagnostics;
using System.Globalization;

namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	/// <summary>
	/// Wrap-class per gestire gli accessi all'EventLog.
	/// </summary>
	/// <remarks>
	/// ATTENZIONE: solo i primi 8 caratteri di un folder di log custom
	/// sono significativi!
	/// 
	/// Come sorgente consiglio usare il nome del componente che effettua
	/// la scrittura su log, recuperabile dal nome dell'executing assembly.
	/// </remarks>
	public class EventLogger : IDisposable
	{
		private string source;
		private string logName;
		private EventLog log;

		/// <summary>
		/// Crea un EventLogger per gestire messaggi di una sorgente (applicazione)
		/// che scrive in un folder di log omonimo.
		/// </summary>
		/// <param name="source">nome della sorgente/folder di log</param>
		//---------------------------------------------------------------------
		public EventLogger(string source) : this(source, source)
		{
		}
		
		/// <summary>
		/// Crea un EventLogger per gestire messaggi di una sorgente (applicazione)
		/// che scrive in un folder di log.
		/// </summary>
		/// <remarks>
		/// Se la sorgente non esiste la crea, se esiste ma spara su un altro log
		/// la distrugge e poi la ricrea.
		/// </remarks>
		/// <param name="source">sorgente, di solito il nome dell'applicazione</param>
		/// <param name="logName">nome del folder di log</param>
		//---------------------------------------------------------------------
		public EventLogger(string source, string logName)
		{
			this.source = source;
			this.logName = logName;

			try
			{
				if (!EventLog.SourceExists(source))
					EventLog.CreateEventSource(source, logName);
				else
				{
					string actualLog = EventLog.LogNameFromSourceName(source, ".");
					if (string.Compare(logName, actualLog, true, CultureInfo.InvariantCulture) != 0)
					{
						//elimina il vecchio
						EventLog.DeleteEventSource(source);
						// crea il nuovo
						EventLog.CreateEventSource(source, logName);
					}
				}
			                
				// Create an EventLog instance and assign its source.
				log = new EventLog();
				log.Source = source;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				Debug.Fail(ex.Message);
			}
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			if (log != null)
			{
				log.Dispose();
				log = null;
				//GC.SuppressFinalize(this);
			}
		}

		/*
		//---------------------------------------------------------------------
		~Log()
		{
			Dispose();
		}
		*/

		/// <summary>
		/// Scrive una entry nel folder di log
		/// </summary>
		/// <param name="message">messaggio testuale</param>
		/// <param name="type">tipo di messaggio</param>
		//---------------------------------------------------------------------
		public void LogItem(string message, EventLogEntryType type)
		{
			try
			{
				log.WriteEntry(message, type);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				Debug.Fail(ex.Message);
			}
		}

		//---------------------------------------------------------------------
	}
}
