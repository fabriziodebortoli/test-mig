using System;
using System.IO;
using System.Runtime.Serialization;


namespace Microarea.WebServices.LoginManager
{
	//=========================================================================
	public class MessagesPersister : IDisposable
	{
		private System.Runtime.Serialization.IFormatter bf;
		private string messagesFilePath;
		private object lockTicket = new object();
		private System.Timers.Timer timer;
		private MessageQueue currentMessagesQueue;

		//---------------------------------------------------------------------
		public MessagesPersister(string messagesFilePath, System.Runtime.Serialization.IFormatter bf)
		{
			if (messagesFilePath == null)
				throw new ArgumentNullException("messagesFilePath");
			if (messagesFilePath.Trim().Length == 0)
				throw new ArgumentException("invalid path", "messagesFilePath");

			if (bf == null)
				throw new ArgumentNullException("bf");

			this.messagesFilePath = messagesFilePath;
			this.bf = bf;
		}

		//---------------------------------------------------------------------
		public void Save(ISerializable messagesQueue)
		{
			if (messagesQueue == null)
				return;

			using (Stream output = File.Create(messagesFilePath))
			{
				bf.Serialize(output, messagesQueue);
			}
		}

		//---------------------------------------------------------------------
		public ISerializable Load()
		{
			if (!File.Exists(messagesFilePath))
				return null;

			using (Stream input = File.OpenRead(messagesFilePath))
			{
				return bf.Deserialize(input) as ISerializable;
			}
		}

		//---------------------------------------------------------------------
		public void ListenTo(MessageQueue messageQueue)
		{
			if (messageQueue == null)
				throw new ArgumentNullException("messageQueue");

			messageQueue.MessageAdded		+= new MessageQueue.MessageEventHandler(SaveMessages);
			messageQueue.MessageRead		+= new MessageQueue.MessageEventHandler(SaveMessages);
            messageQueue.MessageDeleted     += new MessageQueue.MessageEventHandler(SaveMessages);
        }

		//---------------------------------------------------------------------
		public void StopListeningTo(MessageQueue messageQueue)
		{
			if (messageQueue == null)
				throw new ArgumentNullException("messageQueue");

			messageQueue.MessageAdded	-= new MessageQueue.MessageEventHandler(SaveMessages);
            messageQueue.MessageRead    -= new MessageQueue.MessageEventHandler(SaveMessages);
            messageQueue.MessageDeleted -= new MessageQueue.MessageEventHandler(SaveMessages);
        }
		//---------------------------------------------------------------------
		private void SaveMessages(object sender, MessagesQueueEventArgs args)
		{
			lock(lockTicket)
			{
				if (args == null || args.MessagesQueue == null)
					return;
				currentMessagesQueue = args.MessagesQueue;
		
				//Il save è legato a molti eventi e per non eseguire troppi salvataggi inutili 
				//si aspetta mezzo secondo prima di salvare, 
				//in attesa di altri eventi che richiedano il salvataggio
				if (timer == null)
				{
					timer = new System.Timers.Timer();
					timer.Interval = 5000;
					timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
					timer.AutoReset = false;
				}

				if (timer.Enabled)
					timer.Stop();

				timer.Start();
			}
		}

		//---------------------------------------------------------------------
		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			lock(lockTicket)
			{
				try
				{
					if (currentMessagesQueue != null)
						Save(currentMessagesQueue);
				}
				catch {}
			}
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
	}
}
