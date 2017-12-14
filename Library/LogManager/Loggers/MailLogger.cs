using System;
using System.Net.Mail;


namespace Microarea.Library.LogManager.Loggers
{
	/// <summary>
	/// Summary description for MailLogger.
	/// </summary>
	//=========================================================================
	public class MailLogger : BaseLogger
	{
		private	string smtpServerAddress;
		private	MailAddress senderEmailAddress;
		private	MailAddressCollection receiverEmailAddresses;
		private readonly object instanceLockTicket = new object();

		//---------------------------------------------------------------------
		public MailLogger(
			string smtpServer,
			string sender,
			string receiver
			)
			:
			this(
				smtpServer,
				sender,
				receiver,
				(EventTypes.Error | EventTypes.Information | EventTypes.Success | EventTypes.Warning)
			)
		{}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public MailLogger(
			string smtpServer,
			string sender,
			string receiver,
			EventTypes eventsFilter
			)
		{
			this.smtpServerAddress		= smtpServer;
			this.senderEmailAddress		= new MailAddress(sender);

			if (receiver == null)
				throw new ArgumentNullException(
					"receiver",
					"Unable to send mails if 'receiver' is null"
					);

			receiverEmailAddresses = new MailAddressCollection();
			string[] tempAddresses = receiver.Split(',');
			foreach (string address in tempAddresses)
				receiverEmailAddresses.Add(new MailAddress(address));

			if (receiverEmailAddresses.Count == 0)
				throw new ArgumentException("'receiver' contains only ','", "receiver");

			EventsFilter = eventsFilter;
		}

		#region ILogger Members

		//---------------------------------------------------------------------
		public override void Log(ILoggableEventDescriptor eventDescriptor)
		{
			lock (instanceLockTicket)
			{
				if (Object.ReferenceEquals(eventDescriptor, null))
					throw new ArgumentNullException("eventDescriptor", "'eventDescriptor' cannot be null");

				base.Log(eventDescriptor);

				if (!MatchMyEventsFilter(eventDescriptor.EventType))
					return;

				using (MailMessage aMessage = new MailMessage())
				{
					foreach (MailAddress address in receiverEmailAddresses)
						aMessage.To.Add(address);

					aMessage.From = this.senderEmailAddress;

					aMessage.Subject = eventDescriptor.LoggableEventInfo.EventId.ToString(
						System.Globalization.CultureInfo.InvariantCulture
						);

					aMessage.Body = eventDescriptor.Serialize(MessageFormatter);
					aMessage.IsBodyHtml = false;

					aMessage.BodyEncoding = System.Text.Encoding.UTF8;

					using (SmtpClient smtpClient = new SmtpClient())
					{
						smtpClient.Host = this.smtpServerAddress;
						try
						{
							smtpClient.Send(aMessage);
						}
						catch (ArgumentNullException anExc)
						{
							System.Diagnostics.Debug.WriteLine(anExc.ToString());
						}
						catch (ArgumentOutOfRangeException aoorExc)
						{
							System.Diagnostics.Debug.WriteLine(aoorExc.ToString());
						}
						catch (ObjectDisposedException odExc)
						{
							System.Diagnostics.Debug.WriteLine(odExc.ToString());
						}
						catch (InvalidOperationException invOpExc)
						{
							System.Diagnostics.Debug.WriteLine(invOpExc.ToString());
						}
						catch (SmtpFailedRecipientsException sfrExc)
						{
							System.Diagnostics.Debug.WriteLine(sfrExc.ToString());
						}
						catch (SmtpException smtpExc)
						{
							System.Diagnostics.Debug.WriteLine(smtpExc.ToString());
						}
					}
				}
			}
		}

		#endregion
	}
}
