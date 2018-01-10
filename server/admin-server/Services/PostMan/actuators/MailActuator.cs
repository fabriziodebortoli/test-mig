using System;
using System.Net.Mail;

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
				using (SmtpClient client = new SmtpClient(this.smtpAddress))
				{
					client.UseDefaultCredentials = true;

					MailMessage mailMessage = new MailMessage();
					mailMessage.From = new MailAddress("m4Provisioning@m4.com");
					mailMessage.To.Add(destination);
					mailMessage.Body = body;
					mailMessage.Subject = subject;
					client.Send(mailMessage);
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
