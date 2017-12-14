using System;

namespace Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects
{
    //============================================================================================
    public class WTETaskNotificationRecipient
	{

		//-------------------------------------------------------------------------------------------
		public WTETaskNotificationRecipient()
		{
			IsToNotifyOnFailure = IsToNotifyOnSuccess = true;
		}	

		//-------------------------------------------------------------------------------------------
		public WTETaskNotificationRecipient(WTETaskNotificationRecipient aRecipient)
		{
			Recipient = aRecipient.Recipient;
			IsToNotifyOnSuccess = aRecipient.IsToNotifyOnSuccess;
			IsToNotifyOnFailure = aRecipient.IsToNotifyOnFailure;
		}

		//-------------------------------------------------------------------------------------------
		public WTETaskNotificationRecipient(string aRecipientName, int sendcondition)
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
			using (WTESimpleMAPIWrapper simpleMAPI = new WTESimpleMAPIWrapper())
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
                    //string.Format(TaskSchedulerWindowsControlsStrings.InvalidWTEEmailAddressErrMsg, Recipient)
                    String.Empty
					);
			/* Siccome non so se sul database pregresso c'� un indirizzo o un nome, non controllo
			 * la validit� della mail (prima si faceva la ResolveName con la MAPI che adesso non funziona pi�)
			 * magari la mail non parte, ma almeno nel dataentry ho la possibilit� di modificarla
			try
			{
				MailAddress address = new MailAddress(Recipient);
			}
			catch (Exception ex)
			{
				if (!ResolveName())
					throw new ApplicationException
						(
						string.Format(TaskSchedulerWindowsControlsStrings.InvalidWTEEmailAddressErrMsg, Recipient),
						ex
						);
			}*/
		}

		//-------------------------------------------------------------------------------------------
		public bool IsValid()
		{
			try { ValidateAddress(); return true; }
			catch { return false; }
		}
	}
	
	//============================================================================
	public class WTETaskNotificationRecipientsCollection : System.Collections.CollectionBase
	{
		// Restricts to WTETaskNotificationRecipient types, items that can be added to the collection
		//-------------------------------------------------------------------------------------------
		public void Add(WTETaskNotificationRecipient aRecipient)
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
		public WTETaskNotificationRecipient Item(int Index)
		{
			// The appropriate item is retrieved from the List object and explicitly cast 
			// to the WTETaskNotificationRecipient type, then returned to the caller.
			return (WTETaskNotificationRecipient) List[Index];
		}	
	}
}