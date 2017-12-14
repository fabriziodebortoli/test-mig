using System;
using System.Web.Mail;
using System.Configuration;
using System.Diagnostics;


namespace Tester
{
	/// <summary>
	/// Gestisce l'eventlog per registrare i malfunzionamenti verificatisi
	/// durante il funzionamento del web-service.
	/// </summary>
	/// <remarks>
	/// Quest'oggetto é utilizzato da un web service che, durante la sua
	/// esecuzione, impersona l'account di macchina "ASPNET" e gira pertanto 
	/// con i privilegi di tale utente.
	/// In particolare l'utente ASPNET non ha i diritti per creare
	/// l'event-source nel registro di sistema ma solamente per scrivervi
	/// per cui al momento dell'installazione del web service é necessario che
	/// l'event source sia giá presente sulla macchina.
	/// Gli event logs hanno corrispettivo nel registro di sistema in:
	/// HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\EventLog
	/// </remarks>
	//=========================================================================
	public class WSEventLogWriter
	{
		private		EventLog	WSLog;
		private		int			logId		=	0;
		private		string		sourceName	=	"Crypter";

		private		string		mailTo;
		private		string		smtpServer;

		/// <summary>
		/// Ritorna un identificativo univoco per l'errore.
		/// </summary>
		/// <remarks>
		/// L'id e' il numero estratto dal registro degli
		/// eventi che identifica l'errore verificatosi.
		/// </remarks>
		//---------------------------------------------------------------------
		public		int		LogId	{ get { return logId; } }

		//---------------------------------------------------------------------
		private void SendMail(string subject, string message)
		{
			MailMessage Message =	new MailMessage();
			Message.To			=	mailTo;
			Message.From		=	sourceName;
			Message.Subject		=	subject;
			Message.Body		=	message;

			try
			{
				SmtpMail.SmtpServer = smtpServer;
				SmtpMail.Send(Message);
			}
			catch
			{
				WSLog.WriteEntry("Error sending error message.",
											EventLogEntryType.Error, logId);
			}
		}
		
		/// <exception cref="Exception">
		/// Lanciata se non esiste l'event source su cui registrare i logs.
		/// </exception>
		//---------------------------------------------------------------------
		public WSEventLogWriter()
		{
			try
			{
				if( !EventLog.SourceExists(sourceName) )
					throw new Exception("EventLog non accessibile.");
			}
			catch (Exception exc)
			{
				throw exc;
			}
			WSLog			= new EventLog();
			WSLog.Source	= sourceName;

			logId = (WSLog.Entries.Count > 0) ?
				( (WSLog.Entries[WSLog.Entries.Count - 1].EventID) + 1 ) : 0;

			mailTo		= ConfigurationSettings.AppSettings["mailTo"];
			smtpServer	= ConfigurationSettings.AppSettings["smtpServer"];

			if (mailTo == null)
				mailTo = "ilaria.manzoni@microarea.it";
			if (smtpServer == null)
				smtpServer = "mail.microarea.it";
		}

		/// <summary>
		/// Aggiunge una nuova riga nel log relativo al
		/// web-service per segnalare un errore.
		/// </summary>
		/// <param name="errorMessage">
		/// messaggio da inserire nel log.
		/// </param>
		/// <param name="senderType">
		/// tipo dell'oggetto che segnala l'errore.
		/// </param>
		/// <param name="errorType">
		/// tipo di errore verificatosi.
		/// </param>
		/// <param name="methodName">
		/// nome del metodo durante l'esecuzione del quale si e' verificato
		/// l'errore.
		/// </param>
		//---------------------------------------------------------------------
		public void WriteError(string methodName,string errorMessage, string errorDetails)
										
		{
			errorMessage = String.Format("METODO:{0} - ERRORE:{1} - DETTAGLI:{2}",
										methodName, errorMessage, errorDetails );
			WSLog.WriteEntry(errorMessage, EventLogEntryType.Error, logId);

			// Invia la mail di segnalazione
			SendMail(("Error id " + logId.ToString()), errorMessage);
		}

		/// <summary>
		/// Aggiunge una nuova riga nel log relativo al
		/// web-service per segnalare il successo di un evento.
		/// </summary>
		/// <param name="message">messaggio da inserire nel log.</param>
		//---------------------------------------------------------------------
		public void WriteSuccessfulEvent(string message)
		{
			WSLog.WriteEntry(message, EventLogEntryType.SuccessAudit, logId);
		}

		/// <summary>
		/// Aggiunge una nuova riga nel log relativo al
		/// web-service per segnalare un'informazione.
		/// </summary>
		/// <param name="message">messaggio da inserire nel log.</param>
		//---------------------------------------------------------------------
		public void WriteInformation(string message)
		{
			// Invia la mail di segnalazione
			SendMail("Notification", message);
		}
	}
}
