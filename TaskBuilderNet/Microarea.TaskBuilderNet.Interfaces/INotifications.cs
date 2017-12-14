using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Microarea.TaskBuilderNet.Interfaces
{
	[Serializable]
	public enum NotificationType { Generic = 0, BrainBusiness = 1, Chat=2 };

	/// <summary>
	/// Classe generica di notifica implementata dalle specifiche notifiche
	/// </summary>
	public interface IGenericNotify : ISerializable
	{
		// destinatario
		int ToCompanyId { get; }
		int ToWorkerId { get; }
		// mittente
		string FromUserName { get; }
		//int FromCompanyId { get; }
		//int FromWorkerId { get; }

		string Title { get; }

		string Description { get; }

		NotificationType NotificationType { get; }

		DateTime Date { get; }	// data di invio-ricezione utile per eventuali ordinamenti
		DateTime ReadDate { get; }	// data di lettura utile per eventuali ordinamenti

		void OnClickAction();
	}

	[Serializable]
	[KnownType(typeof(NotificationType))]
	public class GenericNotify : IGenericNotify, ISerializable
	{
		// destinatario
		public int ToCompanyId { get; set; }
		public int ToWorkerId { get; set; }

		// mittente
		public string FromUserName { get; set; }
		public int FromCompanyId { get; set; }
		public int FromWorkerId { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

		public NotificationType NotificationType { get; set; }
		//{
		//	get { return NotificationType.Generic; }
		//	set { NotificationType = NotificationType.Generic; }
		//}

		public DateTime Date { get; set; } // data di invio-ricezione utile per eventuali ordinamenti
		public DateTime ReadDate { get; set; }// data di lettura utile per eventuali ordinamenti

		public bool StoredOnDb { get; set; }
		public int NotificationId { get; set; }

		public virtual void OnClickAction() { }

		public bool IsRead()
		{
			//return ReadDate.HasValue;
			return !(this.ReadDate == DateTime.MinValue || this.ReadDate==default(DateTime));
		}

		public GenericNotify() { }

		public GenericNotify(SerializationInfo info, StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException("info", "value cannot be null");

			ToCompanyId		= (int)		info.GetValue("ToCompanyId",	typeof(int));
			ToWorkerId		= (int)		info.GetValue("ToWorkerId",		typeof(int));
			FromUserName	= (string)	info.GetValue("FromUserName",	typeof(string));
			FromCompanyId	= (int)		info.GetValue("FromCompanyId",	typeof(int));
			FromWorkerId	= (int)		info.GetValue("FromWorkerId",	typeof(int));
			Title			= (string)	info.GetValue("Title",			typeof(string));
			Description		= (string)	info.GetValue("Description",	typeof(string));
			Date			= (DateTime)info.GetValue("Date",			typeof(DateTime));
			ReadDate		= (DateTime)info.GetValue("ReadDate",		typeof(DateTime));
			NotificationId	= (int)		info.GetValue("NotificationId", typeof(int));
			StoredOnDb		= (bool)	info.GetValue("StoredOnDb",		typeof(bool));
		}

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException("info", "value cannot be null");

			info.AddValue("ToCompanyId", ToCompanyId);
			info.AddValue("ToWorkerId", ToWorkerId);
			info.AddValue("FromUserName", FromUserName);
			info.AddValue("FromCompanyId", FromCompanyId);
			info.AddValue("FromWorkerId", FromWorkerId);
			info.AddValue("Title", Title);
			info.AddValue("Description", Description);
			info.AddValue("NotificationType", NotificationType);
			info.AddValue("Date", Date);
			info.AddValue("ReadDate", ReadDate.ToUniversalTime());
			info.AddValue("NotificationId", NotificationId);
			info.AddValue("StoredOnDb", StoredOnDb);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException("info");

			GetObjectData(info, context);
		}
	}

	[Serializable]
	public class Father: ISerializable
	{
		public int Intero { get; set; }

		public Father(int intero) 
		{
			Intero = intero;
		}

		protected Father(SerializationInfo info, StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException("info", "value cannot be null");

			Intero = (int)info.GetValue("intero", typeof(int));
		}
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException("info", "value cannot be null");

			info.AddValue("intero", Intero);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException("info");

			GetObjectData(info, context);
		}
	}

	[Serializable]
	public class Child : Father
	{
		public int AltroIntero { get; set; }

		public Child(int intero, int altrointero) : base(intero) { AltroIntero = altrointero; }

		protected Child(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			AltroIntero = (int)info.GetValue("altrointero", typeof(int));
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("altrointero", AltroIntero);
		}

	}

}
