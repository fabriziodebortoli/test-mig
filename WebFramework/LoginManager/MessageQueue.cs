using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using System.Collections.Generic;

namespace Microarea.WebServices.LoginManager
{
	//=========================================================================
	[Serializable]
	public class MessageQueue : ISerializable, IDisposable
	{
		//Coda dei messaggi, 
		//la chiave è lo userName e il valore è l'arrayList di messaggi
		private IDictionary messageQueue;
		private IDictionary notReadMessages;

		public delegate void MessageEventHandler (object sender, MessagesQueueEventArgs args);
		public event MessageEventHandler MessageAdded;
		public event MessageEventHandler MessageConsumed;
		public event MessageEventHandler MessageRead;
        public event MessageEventHandler MessageDeleted;
        //oggetto statico che serve com placeHolder nella hastable valore della notreadmessages	
        private static Object dummy = new object();
		private System.Timers.Timer timer;
      
		//---------------------------------------------------------------------
		public MessageQueue()
		{
			messageQueue	= Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
			notReadMessages = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
		}

		//---------------------------------------------------------------------
		protected MessageQueue(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				messageQueue = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
				notReadMessages = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
			}
			else
			{
				try
				{
					this.messageQueue = Hashtable.Synchronized(
						(Hashtable)info.GetValue("messageQueue", typeof(Hashtable))
						);
				}
				catch(SerializationException)
				{
					messageQueue = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
				}
				try
				{
					this.notReadMessages = Hashtable.Synchronized(
						(Hashtable)info.GetValue("notReadMessages", typeof(Hashtable))
						);
				}
				catch(SerializationException)
				{
					notReadMessages = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
				}
               

			}
		}

		//---------------------------------------------------------------------
		protected virtual void OnMessageAdded(MessagesQueueEventArgs args)
		{
			if (MessageAdded != null)
				MessageAdded(this, args);
		}

		//---------------------------------------------------------------------
		protected virtual void OnMessageConsumed(MessagesQueueEventArgs args)
		{
			if (MessageConsumed != null)
				MessageConsumed(this, args);
		}

		//---------------------------------------------------------------------
		protected virtual void OnMessageRead(MessagesQueueEventArgs args)
		{
			if (MessageRead != null)
				MessageRead(this, args);
		}

        //---------------------------------------------------------------------
        protected virtual void OnMessageDeleted(MessagesQueueEventArgs args)
        {
            MessageDeleted?.Invoke(this, args);
        }

        //---------------------------------------------------------------------
        public void PumpMessageToQueue(IList advertisements)
		{
			if (advertisements == null || advertisements.Count == 0)
				return;

			foreach (Advertisement message in advertisements)
				PumpMessageToQueue(message);

		}
		//---------------------------------------------------------------------
		public void PumpMessageToQueue(Advertisement message)
		{
			if (
				message == null ||
				message.Type == MessageType.None ||
				message.Expired 
				)
				return;

			//se arriva un messaggio con lo stesso id non aggiorno, 
			//perchè è già presente e l'id è unico
			if (!messageQueue.Contains(message.ID))
			{
				messageQueue.Add(message.ID, message);
				//aggiungiamo nei messaggi non letti per ogni utente conosciuto
				foreach (string userName in notReadMessages.Keys)
					((Hashtable)notReadMessages[userName]).Add(message.ID, dummy);

				OnMessageAdded(new MessagesQueueEventArgs(this));
			}

			PurgeExpired();
		}

        //---------------------------------------------------------------------
        private void PurgeNotStorifiedMessages()
        {
            IList /*advertisement*/ list = new ArrayList();
            Hashtable tempHash = null;
            foreach (DictionaryEntry e in messageQueue)
            {
                foreach (DictionaryEntry usermessages in notReadMessages)
                {
                    string userName = usermessages.Key as string;
                    tempHash = notReadMessages[userName] as Hashtable;
                    if (tempHash != null)
                    //solo se l'utente è contenuto nella lista può avere dei messaggi letti , altrimenti sono tutti da leggere
                    {
                        if (tempHash[e.Key] == null)//se non è nella lista di non letti vuol dire che è stato letto quindi è vecchio.
                        {
                            Advertisement x = e.Value as Advertisement;
                            if (x != null && !x.Historicize)
                                x.ExpiryDate = DateTime.Now;//così al prossimo purge lo cancella.
                        }
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        internal void PurgeMessageByTag(string tag, string user = null)
        {
            Hashtable tempHash = null;
            if (user != null)
                tempHash = notReadMessages[user] as Hashtable;
            foreach (DictionaryEntry e in messageQueue)
            {

                if (tempHash == null || tempHash[e.Key] != null)
                {
                    Advertisement x = e.Value as Advertisement;
                    if (x != null && string.Compare(tag, x.Tag, StringComparison.InvariantCultureIgnoreCase) == 0 )
                        x.ExpiryDate = DateTime.Now;//così al prossimo purge lo cancella.
                }
            }
            PurgeExpired();
        }

          

		//---------------------------------------------------------------------
		private void PurgeExpired()
		{
			if (timer == null)
			{
				timer = new System.Timers.Timer();
				timer.Interval = 900000;
				timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);	
				timer.AutoReset = false;
			}
			if (timer.Enabled)
				timer.Stop();
			timer.Start();
		}

		//---------------------------------------------------------------------
		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
            PurgeNotStorifiedMessages();//

			ArrayList expiredList = new ArrayList();

			foreach (Advertisement adv in messageQueue.Values)
				if (adv.Expired)
					expiredList.Add(adv.ID);

			RemoveMessages(expiredList);
			System.Diagnostics.Debug.WriteLine("RemoveMessages finished");
			
		}
		//---------------------------------------------------------------------
		public ArrayList GetOldMessages(string userName)
		{
			if (userName == null)
				throw new ArgumentNullException("userName", "Unable to add 'userName' because is null");

			if (userName.Trim().Length == 0)
				throw new ArgumentException("Unable to add 'userName' because is empty", "userName");

            ArrayList list = new ArrayList();
			Hashtable tempHash = null;
			foreach (DictionaryEntry e in messageQueue)
			{
				tempHash = notReadMessages[userName] as Hashtable;
				if (tempHash != null)
				//solo se l'utente è contenuto nella lista può avere dei messaggi letti , altrimenti sono tutti da leggere
				{
					if (tempHash[e.Key] == null)//se non è nella lista di non letti vuol dire che è stato letto quindi è vecchio.
                    {
                        Advertisement x = e.Value as Advertisement;
                        if (x != null && x.Historicize)
                            list.Add(x);
                    }
				}
			}
            list.Sort(new AdvertisementOrdinatore());
			return list;
		}

		//---------------------------------------------------------------------
        public ArrayList ConsumeMessageFromQueue(string userName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName", "Unable to add 'userName' because is null");

            if (userName.Trim().Length == 0)
                throw new ArgumentException("Unable to add 'userName' because is empty", "userName");

            ArrayList list = new ArrayList();

            if (!notReadMessages.Contains(userName))
            {
                //non contiene lo username, vuol dire che questo utente deve ancora leggerli tutti
                //quindi li aggiungo tutti  e aggiungo l'utente alla lista
                Hashtable myMessages = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
                foreach (DictionaryEntry e in messageQueue)
                {
                    myMessages.Add(e.Key, dummy);
                    list.Add(messageQueue[e.Key] as Advertisement);
                }
                notReadMessages.Add(userName, myMessages);
            }

            else
            {// contiene lo username, vuol dire che questo utente deve ancora leggere solo quelli contenuti nella notreadmessages
                foreach (DictionaryEntry e in ((Hashtable)notReadMessages[userName]))
                    list.Add(messageQueue[e.Key] as Advertisement);
            }

            if (list.Count > 0)
                OnMessageConsumed(new MessagesQueueEventArgs(this));
            list.Sort(new AdvertisementOrdinatore());
            return list;
        }

        //---------------------------------------------------------------------
        public class AdvertisementOrdinatore : IComparer
        {
            //---------------------------------------------------------------------
            public AdvertisementOrdinatore() { }

            //---------------------------------------------------------------------
            public int CompareAdv(Advertisement objA, Advertisement objB)
            {
                if (objA == null && objB == null) return 0;
                if (objA == null) return 1;
                if (objB == null) return -1;
                return objA.CreationDate.CompareTo(objB.CreationDate)*-1;
            }

            public int Compare(object x, object y)
            {
                return CompareAdv(x as Advertisement, y as Advertisement);
            }

        }

		//---------------------------------------------------------------------
		private void RemoveMessages(IList messagesID)
		{
			if (messagesID == null || messagesID.Count == 0)
				return;
            foreach (string id in messagesID)
                RemoveMessage(id);
		}

        //---------------------------------------------------------------------
        internal void RemoveMessage(string messageID)
        {
            if (messageID == null || messageID.Trim().Length == 0)
                    return;

            messageQueue.Remove(messageID);
                Hashtable tempHash = null;
                foreach (string userName in notReadMessages.Keys)
                {
                    tempHash = notReadMessages[userName] as Hashtable;
                    if (tempHash != null)
                        tempHash.Remove(messageID);
                }
            OnMessageDeleted(new MessagesQueueEventArgs(this));

        }
		//---------------------------------------------------------------------
		public void SetMessageRead(string userName, string messageID)
		{
			if (userName == null)
				throw new ArgumentNullException("userName", "Unable to add 'userName' because is null");

			if (userName.Trim().Length == 0)
				throw new ArgumentException("Unable to add 'userName' because is empty", "userName");

			if (messageID == null)
				throw new ArgumentNullException("messageID", "Unable to add 'messageID' because is null");

			if (messageID.Trim().Length == 0)
				throw new ArgumentException("Unable to add 'messageID' because is empty", "messageID");

			// Verificare se dare eccezione nel caso in cui venga messo a 'letto' un messaggio che non era da leggere
			Hashtable tempHash = notReadMessages[userName] as Hashtable;
			if (tempHash != null)
			{
				tempHash.Remove(messageID);
				OnMessageRead(new MessagesQueueEventArgs(this));
			}
		}

		[
		SecurityPermissionAttribute(
			 SecurityAction.Demand,
			 SerializationFormatter=true
			 )
		]
		//---------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException("info", "value cannot be null");

			info.AddValue("messageQueue", messageQueue);
			info.AddValue("notReadMessages", notReadMessages);
		}

		#region IDisposable Members

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
            GC.SuppressFinalize(this);
		}

		#endregion

		//---------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (timer != null)
				{
					timer.Dispose();
					timer = null;
				}
			}
		}

        //---------------------------------------------------------------------
        internal void PurgeOnRestart()
        {
            if (messageQueue == null || messageQueue.Values == null || messageQueue.Values.Count ==0 ) return;

            ArrayList l = new ArrayList();
            foreach (Advertisement a in (messageQueue.Values))
                if (a != null && a.ExpireWithRestart)
                    l.Add(a.ID);

            RemoveMessages(l);
        }

       
    }

	//=========================================================================
	public class MessagesQueueEventArgs : EventArgs
	{
		private MessageQueue messagesQueue;

		public MessageQueue MessagesQueue { get{return messagesQueue;}}

		//---------------------------------------------------------------------
		public MessagesQueueEventArgs(MessageQueue messagesQueue)
		{
			this.messagesQueue = messagesQueue;
		}
	}
}
