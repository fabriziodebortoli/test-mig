using System.Runtime.Serialization;
using System.ServiceModel;
using System;
using Microarea.TaskBuilderNet.Interfaces;
using System.Collections.Generic;

namespace NotificationService
{	
	//[KnownType(typeof(GenericNotify))]
	//[KnownType(typeof(Child))]
	
    [ServiceContract]
	[ServiceKnownType(typeof(Child))]
	[ServiceKnownType(typeof(GenericNotify))]
    public interface INotificationService
    {
		[OperationContract]
		bool IsAlive();

		[OperationContract]
        string GetXSocketControllerUrl();

		[OperationContract]
		void SendMessage(string message, int workerId, int companyId);

		[OperationContract]
		void SendIGenericNotify(GenericNotify notify, bool StoreOnDb);

		[OperationContract]
		GenericNotify[] GetAllIGenericNotify(int workerId, int companyId, bool includeProcessed);

		[OperationContract]
		bool SetNotificationAsRead(int NotificationId);

		[OperationContract]
		Father TestInheritance(Father father);


		//-----------------------------Chat module---------------------------------------------------

		[OperationContract]
		void SendChatMessage(GenericNotify notify);

		//-----------------------------BB module-----------------------------------------------------
		[OperationContract]
		string GetBrainBusinessServiceUrl();

		[OperationContract]
		bool UpdateBrainBusinessServiceUrl(string url);

        [OperationContract]
        MyBBFormSchema GetBBForm(int formInstanceId);
        
        [OperationContract]
        bool SetBBForm(MyBBFormSchema myBBForm);

		[OperationContract]
		MyBBFormInstance[] GetAllBBFormInstances(int workerId, int companyId, bool includeProcessed);
    }

	[DataContract]
    public class MyBBFormSchema
    {
        [DataMember]
        public string			xmlSchema			{ get; set; }
        [DataMember]
		public MyBBFormInstance myBBFormInstance	{ get; set; }
    }
    
    [DataContract]
    public class MyBBFormInstance
    {
        [DataMember]
        public DateTime DateProcessed		{ get; set; }
        [DataMember]
        public DateTime DateSubmitted		{ get; set; }
        [DataMember]
        public int		FormInstanceId		{ get; set; }
        [DataMember]
        public bool		IsNotificationOnly	{ get; set; }
        [DataMember]
        public bool		Processed			{ get; set; }
        [DataMember]
        public string	Title				{ get; set; }
        [DataMember]
        public Guid		WorkFlowId			{ get; set; }
        [DataMember]
        public string	UserName			{ get; set; } //utile?
    }

	///// <summary>
	///// Classe generica di notifica implementata dalle specifiche notifiche
	///// </summary>
	//[DataContract]
	//public abstract class IGenericNotify
	//{
	//	[DataMember]
	//	public int WorkerId { get; set; }

	//	[DataMember]
	//	public int CompanyId { get; set; }

	//	[DataMember]
	//	public string Title { get; set; }
		
	//	[DataMember]
	//	public string Description { get; set; }
		
	//	//NotificationType Type { get; }
		
	//	[DataMember]
	//	public DateTime Date { get; set; }	// data di ricezione utile per eventuali ordinamenti
		
	//	[OperationContract]
	//	public abstract void OnClickAction();
	//}
}
