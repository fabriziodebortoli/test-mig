using System;

using Microarea.Console.Core.TaskSchedulerObjects.TaskSchedulerMail;
using System.Net.Mail;
using Microarea.Console.Core.TaskSchedulerWindowsControls;

namespace Microarea.Console.Core.TaskSchedulerObjects
{
	//============================================================================================
	public class TaskNotificationRecipient
	{
		public const string SchedulerMailNotificationsTableName	= "MSD_SchedulerMailNotifications";
		public const string TaskIdColumnName					= "TaskId";
		public const string RecipientNameColumnName				= "RecipientName";
		public const string SendConditionColumnName				= "SendCondition";

		public static string[] SchedulerMailNotificationsTableColumns = new string[3]{
																						 TaskIdColumnName,
																						 RecipientNameColumnName,
																						 SendConditionColumnName
																					 };


		//-------------------------------------------------------------------------------------------
		public TaskNotificationRecipient()
		{
			IsToNotifyOnFailure = IsToNotifyOnSuccess = true;
		}	

		//-------------------------------------------------------------------------------------------
		public TaskNotificationRecipient(TaskNotificationRecipient aRecipient)
		{
			Recipient = aRecipient.Recipient;
			IsToNotifyOnSuccess = aRecipient.IsToNotifyOnSuccess;
			IsToNotifyOnFailure = aRecipient.IsToNotifyOnFailure;
		}

		//-------------------------------------------------------------------------------------------
		public TaskNotificationRecipient(string aRecipientName, int sendcondition)
		{
			Recipient = aRecipientName;

			if ((sendcondition & 0x0001) == 0x0001)
				IsToNotifyOnSuccess = true;
			if ((sendcondition & 0x0002) == 0x0002)
				IsToNotifyOnFailure = true;
		}

		//-------------------------------------------------------------------------------------------
		private bool ResolveName()
		{
			bool resolved = false;
			using (SimpleMAPIWrapper simpleMAPI = new SimpleMAPIWrapper())
			{
				if (simpleMAPI.Logon(IntPtr.Zero, false))
				{
					string recipientAddress;
					if (simpleMAPI.GetAddressByDisplayName(Recipient, out recipientAddress))
					{
						Recipient = recipientAddress;
						resolved = true;
					}
					simpleMAPI.Logoff();
				}
			}
			return resolved;
		}

		//-----------------------------------------------------------------------------
		public override string ToString()
		{ 
			return Recipient;
		}

		//-------------------------------------------------------------------------------------------
		public bool IsToNotifyOnSuccess
		{
			get;
			set;
		}

		//-------------------------------------------------------------------------------------------
		public bool IsToNotifyOnFailure
		{
			get;
			set;
		}

		//-------------------------------------------------------------------------------------------
		private string recipient = "";
		public string Recipient
		{
			get { return recipient; }
			set 
			{
				recipient = value;
				ValidateAddress();
			}
		}

		//-------------------------------------------------------------------------------------------
		private void ValidateAddress()
		{
			if (string.IsNullOrEmpty(Recipient))
				throw new ApplicationException
					(
					string.Format(TaskSchedulerWindowsControlsStrings.InvalidEmailAddressErrMsg, Recipient)
					);
			/* Siccome non so se sul database pregresso c'è un indirizzo o un nome, non controllo
			 * la validità della mail (prima si faceva la ResolveName con la MAPI che adesso non funziona più)
			 * magari la mail non parte, ma almeno nel dataentry ho la possibilità di modificarla
			try
			{
				MailAddress address = new MailAddress(Recipient);
			}
			catch (Exception ex)
			{
				if (!ResolveName())
					throw new ApplicationException
						(
						string.Format(TaskSchedulerWindowsControlsStrings.InvalidEmailAddressErrMsg, Recipient),
						ex
						);
			}*/
		}

		//-------------------------------------------------------------------------------------------
		internal bool IsValid()
		{
			try { ValidateAddress(); return true; }
			catch { return false; }
		}
	}
	
	//============================================================================
	public class TaskNotificationRecipientsCollection : System.Collections.CollectionBase
	{
		// Restricts to TaskNotificationRecipient types, items that can be added to the collection
		//-------------------------------------------------------------------------------------------
		public void Add(TaskNotificationRecipient aRecipient)
		{
			List.Add(aRecipient);
		}

		//-------------------------------------------------------------------------------------------
		public void Remove(int index)
		{
			// Check to see if there is a recipient at the supplied index.
			if (index > Count - 1 || index < 0)
			{
				throw new IndexOutOfRangeException("The supplied index is out of range");
			}
			else
			{
				List.RemoveAt(index); 
			}
		}

		//-------------------------------------------------------------------------------------------
		public TaskNotificationRecipient Item(int Index)
		{
			// The appropriate item is retrieved from the List object and explicitly cast 
			// to the TaskNotificationRecipient type, then returned to the caller.
			return (TaskNotificationRecipient) List[Index];
		}	
	}
}