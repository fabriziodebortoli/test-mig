using System;
using System.Net.Mail;
using System.Configuration;
using System.Diagnostics;

namespace Microarea.Library.WSLogger
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
	public abstract class BaseLogger
	{
		protected		EventLog	WSLog		= null;
		protected		int			logId		=	0;
		protected		string		sourceName	= null;
		protected		string		mailTo		= null;
		protected		string		smtpServer	= null;
		public			string		ProductName	= null;
		public			string		LoginID		= null;
		public			string		RagSociale	= null;
		public			string		CodAzienda	= null;
		public			DateTime	ProcStarting;

		public BaseLogger(){}
		public BaseLogger(string loginid, string productname){}

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
		public virtual void WriteSuccessfulEvent(string message)
		{
			if (WSLog == null) 
				return;
			WSLog.WriteEntry(message, EventLogEntryType.SuccessAudit, logId);
		}

		//---------------------------------------------------------------------
		public virtual void WriteAndSendError(string methodName,string errorMessage)								
		{
			WriteAndSendError(methodName, errorMessage, "---");		
		}

		//---------------------------------------------------------------------
		public virtual void WriteAndSendError(string methodName, string errorMessage, string errorDetails)								
		{
			if (WSLog == null) 
				return;
			string message = String.Format("Method: {0}{1}Error: {2}{3}Details: {4}",
				methodName, Environment.NewLine, errorMessage, Environment.NewLine, errorDetails );
			WSLog.WriteEntry(message, EventLogEntryType.Error, logId);			
			// Invia la mail di segnalazione
			SendMail(("Error id " + logId.ToString()), message);
		}

		//---------------------------------------------------------------------
		public virtual void SendMail(string subject, string message)
		{
			if (mailTo == null || smtpServer == null || sourceName == null)
				return;
			MailMessage Message =	new MailMessage();
            string[] destinationMailAddresses = mailTo.Split(';');
            foreach (string mailAddress in destinationMailAddresses)
            {
                Message.To.Add(mailAddress);
            }
			Message.From		=	new MailAddress(sourceName);
			Message.Subject		=	subject;
			Message.Body		=	message;
			Message.IsBodyHtml	=	true;
			try
			{
				SmtpClient aSmtpClient = new SmtpClient(smtpServer);
                aSmtpClient.Send(Message);
			}
			catch
			{}
		}
	}
}
