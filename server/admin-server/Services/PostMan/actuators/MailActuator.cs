using System;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace Microarea.AdminServer.Services.PostMan.actuators
{
	//================================================================================
	public class MailActuator : IPostManActuator
	{
		string smtpAddress;

		//--------------------------------------------------------------------------------
		public MailActuator(string smtpAddress)
		{
			this.smtpAddress = smtpAddress;
		}

		//--------------------------------------------------------------------------------
		public OperationResult Send(string destination, string subject, string body)
		{
			OperationResult opRes = new OperationResult(true, "Operation commpleted");

			try
			{
				using (SmtpClient client = new SmtpClient())
				{
					client.Connect(this.smtpAddress);
					MimeMessage mailMessage = new MimeMessage();
					mailMessage.From.Add(new MailboxAddress("m4Provisioning@m4.com"));
					mailMessage.To.Add(new MailboxAddress(destination));
					mailMessage.Subject = subject;
					mailMessage.Body = new TextPart("plain") { Text = body };
					client.Send(mailMessage);
					client.Disconnect(true);
				}
			}
			catch (Exception e)
			{
				opRes.Result = false;
				opRes.Message = String.Format("An error occurred while sending email to {0} ({1})", subject, e.Message);
			}

			return opRes;
		}
	}
}
