using System;
using System.Net.Mail;
using System.Configuration;
using System.Diagnostics;
using System.Collections;

namespace Microarea.Library.WSLogger
{
	
	//=========================================================================
	public class WSCryptLogWriter : BaseLogger
	{
		private ArrayList Errors = null;
		private string serverName = null; 

		//---------------------------------------------------------------------
		public WSCryptLogWriter(string login, string product, string srvName, string mailto) 
		{
			LoginID		= login;
			ProductName = product;
			serverName	= srvName;
			
			smtpServer	= ConfigurationManager.AppSettings["smtpServer"];
			if (smtpServer == null)
				smtpServer = "mail.microarea.it";
			
			GetAddress(mailto);
			sourceName = mailto;

            //dal 23/05/2012 non si logga più sull event viewer
			/*string logSourceName = "Crypter";
            
			try
			{
				if (!EventLog.SourceExists(logSourceName))
				{
					string user		= Environment.UserName;
					string machine	= Environment.MachineName;
					throw new Exception(String.Format("Error during the existing test of the Source: {0}; User: {1}; Machine: {2}", logSourceName, user, machine));
				}
			}
			catch (Exception exc)
			{
				string s = "ServerName: " + srvName;
				string s1 = "Login: " + login;
				string s2 = "Product: " + product;
				SendMail(s +Environment.NewLine +s1 +Environment.NewLine +s2 +Environment.NewLine + "EventLog not accessible", exc.Message);
			}
			WSLog			= new EventLog();
			WSLog.Source = logSourceName;

			logId =	(WSLog.Entries.Count > 0)							?
				Convert.ToInt32((WSLog.Entries[WSLog.Entries.Count - 1].InstanceId) + 1 ) : 
				0;			*/
		} 

		//---------------------------------------------------------------------
		private void GetAddress(string mailto) 
		{
			mailTo = mailto	;
			if (mailTo == null)
				mailTo = ConfigurationManager.AppSettings["mailTo"];
			if (mailTo == null)
				mailTo = "ilaria.manzoni@microarea.it";
		}

		//---------------------------------------------------------------------
		private void AddError(string method, string errorCode, string errorId, string details)
		{
			if (Errors == null)
				Errors = new ArrayList();
			Errors.Add(new ErrorInfo(errorCode, errorId, method, details));
		}

		//---------------------------------------------------------------------
		public override void WriteAndSendError(string methodName, string errorCode, string errorDetails)								
		{	
			string message = String.Format("Method: {0}{1}Error: {2}{3}Details: {4}",
				methodName, Environment.NewLine, errorCode, Environment.NewLine, errorDetails );
			AddError(methodName, errorCode, logId.ToString(), errorDetails);

            if (WSLog == null) 
				return;
            WSLog.WriteEntry(message, EventLogEntryType.Error, logId);
		}

		//---------------------------------------------------------------------
		public void SendMail(string count)
		{
			string name = (RagSociale == null || RagSociale.Length == 0) ? LoginID : RagSociale;
		
			SendMail(String.Format("{0}, {1}", name, ProductName), WriteHtmlMail(count));
		}

		//---------------------------------------------------------------------
		private string WriteHtmlMail(string count)
		{
			System.Text.StringBuilder MailMessage = new System.Text.StringBuilder();
			string empty = "---";
			string header = @"<html>
						<head>
						<title>" + "Crypter" + @"</title>
						<meta http-equiv=""Content-Type"" content=""text/html; charset=iso-8859-1"">
						<style>
						h3						{font-family: Verdana,Arial,Helvetia,sans-serif; color: #000080; font-size: 14px;}
						th						{font-family: Verdana,Arial,Helvetia,sans-serif; color: #FFF0F5; font-size: 11px;}
						p						{font-family: Verdana,Arial,Helvetia,sans-serif; font-size: 10px; color: #000080; text-align: justify}
						td						{font-family: Verdana,Arial,Helvetia,sans-serif; font-size: 10px; color: #000080;}
						li						{font-family: Verdana,Arial,Helvetia,sans-serif; font-size: 10px; color: #000080; text-align: justify}
						body					{background: #FFFFFF; font-family: Verdana,Arial,Helvetia,sans-serif; font-size: 10px; color: #000080; text-align: justify}
						.ReportHeader			{font-family: Verdana,Arial,Helvetia,sans-serif; font-weight:bold; font-size: 11px; color: #FFFFFF; background: #DA3287; border-top:1px solid #003399; border-left:1px solid #003399; border-bottom:1px solid #003399; border-right:1px solid #003399;}   
						.borderBottom			{background: #F0F0F0; border-top:0px solid #003399;border-left:0px solid #003399; border-bottom:1px solid #003399; border-right:0px solid #003399;}										
						.border1				{background: #F0F0F0; border-top:1px solid #003399; border-left:1px solid #003399; border-bottom:1px solid #003399; border-right:1px solid #003399;}
						.borderBottomBold		{background: #F0F0F0; border-top:0px solid #003399;border-left:0px solid #003399; border-bottom:2px solid #003399; border-right:0px solid #003399;}										
						.borderBottomErrorItem	{background: #F0F0F0; border-top:0px solid #003399;border-left:0px solid #003399; border-bottom:1px solid #003399; border-right:0px solid #003399;color: #FFF0F5;}
						.ErrorItem				{color: #FFF0F5;}
						</style>
						</head>
						<body>";
			MailMessage.Append(header);
			MailMessage.Append("<table class='border1' border='0' width='600' cellspacing='0' cellpadding='5'>");
			string arg, message;
			//cella
			arg = "Crypting required on: " + serverName + " - v.4.20160307";
			MailMessage.Append("<tr><td class='ReportHeader' colspan='2'>" + arg + "</th></tr>");
			//fine cella
			//cella
			arg = "Crypting start:";
			message = ProcStarting.ToString("yyyy-MM-dd HH.mm") ;
			if (message == null || message.Length == 0) message = empty;
			MailMessage.Append("<tr><td class='BorderBottom' width='35%'><b>" + arg + "</b></td>");
			MailMessage.Append("<td class='BorderBottom'>" + message + "</td></tr>");
			//fine cella
			//cella
			arg = "Company:";
			message = RagSociale;
			if (message == null || message.Length == 0) message = empty;
			MailMessage.Append("<tr><td class='BorderBottom'><b>" + arg + "</b></td>");
			MailMessage.Append("<td class='BorderBottom'>" + message + "</td></tr>");
			//fine cella
			//cella
			arg = "Company code:";
			message = CodAzienda;
			if (message == null || message.Length == 0) message = empty;
			MailMessage.Append("<tr><td class='BorderBottom'><b>" + arg + "</b></td>");
			MailMessage.Append("<td class='BorderBottom'>" + message + "</td></tr>");
			//fine cella
			//cella
			arg = "Login:";
			message = LoginID;
			if (message == null || message.Length == 0) message = empty;
			MailMessage.Append("<tr><td class='BorderBottom'><b>" + arg + "</b></td>");
			MailMessage.Append("<td class='BorderBottom'>" + message + "</td></tr>");
			//fine cella
			//cella
			arg = "Product:";
			message = ProductName;
			if (message == null || message.Length == 0) message = empty;
			MailMessage.Append("<tr><td class='BorderBottom'><b>" + arg + "</b></td>");
			MailMessage.Append("<td class='BorderBottom'>" + message + "</td></tr>");
			//fine cella
			//cella
			arg = "File received number:";
			message = count;
			if (message == null || message.Length == 0) message = empty;
			MailMessage.Append("<tr><td class='BorderBottom'><b>" + arg + "</b></td>");
			MailMessage.Append("<td class='BorderBottom'>" + message + "</td></tr>");
			//fine cella	
			//ERRORI
			bool hasError = (Errors != null && Errors.Count > 0);
			if (hasError)
			{
				//cella
				arg = "Errors";
				MailMessage.Append("<tr><td class='ReportHeader' colspan='2'>" + arg + "</th></tr>");
				//fine cella
					MailMessage.Append("<tr><td colspan='2'><table class='border1' border='0' width='100%' cellspacing='0' cellpadding='5'>");
				for (int i = 0; i < Errors.Count; i++)//(ErrorInfo ei in Errors)
				{
					ErrorInfo ei = (ErrorInfo)Errors[i];
					//cella
					arg = "Error code:";
					message = ei.Code;
					if (message == null || message.Length == 0) message = empty;
					int index = i+1;
					MailMessage.Append("<tr><td class='ErrorItem' width='5%'><b>" + index.ToString() + "</b></td>");
					MailMessage.Append("<td class='BorderBottom' width='35%'><b>" + arg + "</b></td>");
					MailMessage.Append("<td class='BorderBottom'>" + message + "</td></tr>");
					//fine cella
					//cella
					arg = "Error id:";
					message = ei.ID;
					if (message == null || message.Length == 0) message = empty;
					MailMessage.Append("<tr><td>"+" "+"</td>");
					MailMessage.Append("<td class='BorderBottom'><b>" + arg + "</b></td>");
					MailMessage.Append("<td class='BorderBottom'>" + message + "</td></tr>");
					//fine cella
					//cella
					arg = "Method:";
					message = ei.Method;
					if (message == null || message.Length == 0) message = empty;
					MailMessage.Append("<tr><td>"+" "+"</td>");
					MailMessage.Append("<td class='BorderBottom'><b>" + arg + "</b></td>");
					MailMessage.Append("<td class='BorderBottom'>" + message + "</td></tr>");
					//fine cella
					//cella
					arg = "Message:";
					message = ei.Details;
					if (message == null || message.Length == 0) message = empty;
					MailMessage.Append("<tr><td class='borderBottomBold'>&nbsp;</td>");
					MailMessage.Append("<td class='borderBottomBold'><b>" + arg + "</b></td>");
					MailMessage.Append("<td class='borderBottomBold'>" + message + "</td></tr>");
					//fine cella
				}
			}
			MailMessage.Append("</table></td></tr>");
					
			MailMessage.Append("</table><br /><br />");
			MailMessage.Append("<table border='0' width='600' cellspacing='0' cellpadding='0'>");
			string footer = hasError	?	
							"Crypting procedure stopped with errors."			:
							"Crypting procedure finished successfully.";	
			MailMessage.Append("<tr><td colspan='2'>" + footer + "</td></tr>");
			MailMessage.Append("</table>");
			MailMessage.Append("</body></html>");

			return MailMessage.ToString();
		}

	}
	/// <summary>
	/// 
	/// </summary>
	//=========================================================================
	public class ErrorInfo
	{
		public string Code;
		public string ID;
		public string Method;
		public string Details;

		public ErrorInfo( string code, string id, string method, string details)
		{
			Code	= code;
			ID		= id;
			Method	= method;
			Details	= details;
			
		}
	}
}
