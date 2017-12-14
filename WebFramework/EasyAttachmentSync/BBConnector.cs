using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using System.IO;
using Microarea.TaskBuilderNet.Core.Generic;
using System.ServiceModel;
using Microarea.TaskBuilderNet.Core;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.NotificationManager;
using System.Data;
using System.Data.SqlClient;
using Microarea.TaskBuilderNet.Core.NotificationService;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.WebServices.EasyAttachmentSync
{
	internal class BBConnector
	{

		private enum ApprovalStatus { NotApproved=0, Approved, Rejected, UnDefined };

		private static string BrainBusinessServiceUrl;

		//-------------------identificativo del connettore per EasyAttachments---------------------
		private static readonly Guid ApplicationProviderId = new System.Guid("123ED732-D3ED-4328-8E1B-93334E8422B3");

		//-------identificativi degli eventi esposti dal connettore per EasyAttachments----da tenere allineati con il connettore--------

		//NewAttachmentByIdEvent - parametri in ingresso: AttachmentId int, CompanyId int, workerId int, comments string
		private static readonly Guid NewAttachmentEvent = new System.Guid("123B495A-0F7D-41A6-A696-B99AC94576DF");

		//NewAttachmentByDescriptionEvent - parametri in ingresso: Description string, CompanyId int
		private static readonly Guid DeleteUserEvent = new System.Guid("012B495A-0F7D-41A6-A696-B99AC94576DF");

		//Add User - parametri in ingresso: UserName string, Password string, FirstName string
		private static readonly Guid AddUserEvent = new System.Guid("789B495A-0F7D-41A6-A696-B99AC94576DF");

		//TestProcess - parametri in ingresso: UserName string
		private static readonly Guid TestProcessEvent = new System.Guid("234B495A-0F7D-41A6-A696-B99AC94576DF");

		//TestProcess - parametri in ingresso: UserName string
		private static readonly Guid DeleteFormInstanceEvent = new System.Guid("345B495A-0F7D-41A6-A696-B99AC94576DF");

		//------------identificativi dei processi creati con Brain Business Studio------------------
		private static readonly Guid EaAddUserNew = new System.Guid("ce76c5ad-d324-450c-8557-5631529a0f70");
		private static readonly Guid EaSimpleApprovation = new System.Guid("13bf50b0-1dd2-4ae3-a1b6-7720626d81fb");
		private static readonly Guid FieldsTestDefinition = new System.Guid("0131829f-8885-4c22-a097-41093b2090bb");
		private static readonly Guid TestDiProcessoDefinition = new System.Guid("e0a74a4b-fc35-4511-a725-01569848d9a8");
		private static readonly Guid DeleteFormInstanceDefinition = new System.Guid("0d8eb9f9-5e4a-4622-927b-b471ba24c053");

		//todo: da sostituire utilizzando il metodo del basepathfinder
		private static readonly string NotificationServiceUrl =
			//Per Sviluppo:
			BasePathFinder.BasePathFinderInstance.NotificationServiceUrl
			//Per Test:
			//"http://localhost/MicroService/Service1.svc"
			;

		/// <summary>
		/// Aggiorna il valore della variabile omonima richiedendolo al Notification Service, il quale interrogherà a sua volta il db todo: interrogare direttamente il db da qui?
		/// </summary>
		/// <returns></returns>
		internal static bool UpdateBBServiceUrl() 
		{
			try
			{
				BrainBusinessServiceUrl = NotificationServiceUtility.GetNotificationServiceClient(BasePathFinder.BasePathFinderInstance.NotificationServiceUrl).GetBrainBusinessServiceUrl();
				return true;
			}
			catch 
			{
				return false;
			}
		}

		/// <summary>
		/// restituisce l'indirizzo a cui contattare il servizio di Brain Business
		/// </summary>
		/// <returns></returns>
		internal static string GetBBServiceUrl()
		{
			if(string.IsNullOrEmpty(BrainBusinessServiceUrl))
			{
				try
				{
					if(!UpdateBBServiceUrl())
						throw new Exception();
				}
				catch(Exception e)
				{
					//default url
					BrainBusinessServiceUrl = "http://localhost:8081/APMServiceclient";
					EASyncEngine.WriteMessageInEventLog("BBConnector, GetBBServiceUrl() - errore in fase di connessione di recupero dell'indirizzo di brain business dal database di sistema\n Verrà impostato il valore di default: "  +  BrainBusinessServiceUrl+ "\n"+ e.Message);
				}
			}

			return BrainBusinessServiceUrl;
		}

		/// <summary>
		/// verifica la connessione con il servizio di Brain
		/// </summary>
		/// <returns></returns>
        internal static bool BBIsAlive()
		{
			try
			{
				var serviceRef = BBServiceUtility.GetBBServiceClient(GetBBServiceUrl());
				bool connected = serviceRef.Ping();
				return connected;
			}
			catch 
			{
				return false;
			}
		}
                      
		/// <summary>
		/// Gira l'evento a un particolare processo di BrainBusiness
		/// </summary>
		/// <param name="attachmentId"></param>
		/// <param name="companyId"></param>
		/// <param name="workerId"></param>
		/// <param name="comments"></param>
		/// <returns>il guid del processo</returns>
		internal static bool NewAttachmentById(int attachmentId, int companyId, int workerId, string comments)
		{
			if(comments==null)
				EASyncEngine.WriteMessageInEventLog("BBConnector, NewAttachmentById(...) - comments = null");
			
			var ProcessGuid=new System.Guid();
			//-----------------------------------------------------------------------
			var RequesterUserName = BuildBrainBusinessUserName(companyId, workerId);
			//-----------------------------------------------------------------------
			var approverId = 1;
			//-----------------------------------------------------------------------
			var ApprovalUserName = BuildBrainBusinessUserName(companyId, approverId);
			//-----------------------------------------------------------------------
			//nome della company = application code di BrainBusiness

			DmsDatabaseInfo dmsInfo = EASyncEngine.GetDmsDatabaseFromCompanyId(companyId);
			string companyName = dmsInfo.Company;

			//inizializzo il workflowContext con gli attributi necessari per il metodo DispatchEvent
			var myWorkFlowContext = new BBServiceReference.WorkflowContext
			{
				ApplicationConnectionCode = companyName,
				ApplicationProviderEventID = NewAttachmentEvent,
				ApplicationProviderID = ApplicationProviderId
			};
			//aggiungo i parametri al contesto.
			//Questi parametri sono quelli attesi dallo specifico evento che vogliamo chiamare
			myWorkFlowContext.Parameters = new Dictionary<string, object>();
			myWorkFlowContext.Parameters.Add("AttachmentId", attachmentId);
			myWorkFlowContext.Parameters.Add("RequesterUserName", RequesterUserName);
			myWorkFlowContext.Parameters.Add("Comments", comments);
			myWorkFlowContext.Parameters.Add("ApprovalUserName", ApprovalUserName);

			//faccio lo stesso con l'applicationContext
			var myApplicationContext = new BBServiceReference.ApplicationContext
			{
				ObjectID = attachmentId.ToString(),
				ObjectType = "fakeAttachment",
				//todo: vedere se bisogna sostituire il worker con il brainBusinessUserName
				Username = RequesterUserName
			};

			try
			{
				//rappresenta il webService a cui invieremo gli eventi che verranno gestiti dai processi
				var bbServiceRef = BBServiceUtility.GetBBServiceClient(GetBBServiceUrl());
				//faccio partire un particolare processo, in seguito ad un particolare evento

				//old style->   //var guid= bbServiceRef.DispatchEventForDefinitionByIdentifier(myWorkFlowContext, EADefinitionComplessa);
				ProcessGuid = bbServiceRef.CreateWorkflow(myWorkFlowContext, EaSimpleApprovation, myApplicationContext);
				//per test->	bbServiceRef.CreateWorkflow(myWorkFlowContext, FieldsTestDefinition, myApplicationContext);
				Insert(attachmentId, workerId, comments, companyId, approverId);

				//sottoscrivo il web service delle notifiche agli aggiornamenti riguardanti questo particolare processo
				bbServiceRef.Subscribe(ProcessGuid, RequesterUserName, NotificationServiceUrl);
				
				return true;

			}
			catch(ServerTooBusyException e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, NewAttachmentById(...) - " + 
					string.Format(EASyncStrings.BBSendEventError, 1) + 
					"\n" + e.Message + "\n\n" + EASyncStrings.BBCheckBBServices);
			}
			catch(EndpointNotFoundException e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, NewAttachmentById(...) - " +
					string.Format(EASyncStrings.BBSendEventError, 2) +
					"\n" + e.Message + "\n\n" + EASyncStrings.BBCheckBBServices);
			}
			catch(FaultException e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, NewAttachmentById(...) - " +
					string.Format(EASyncStrings.BBSendEventError, 3) +
					"\n" + e.Message + "\n\n" + 
					string.Format(EASyncStrings.BBCheckCompanyConnection, companyName));
			}
			catch(Exception e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, NewAttachmentById(...) - " +
					string.Format(EASyncStrings.BBSendEventError, 4) +
					"\n" + e.Message);
			}
			return false;
		}

		/// <summary>
		/// Metodo utilizzato per inserire una richiesta di approvazione di un allegato 
		/// all'interno della tabella DMS_WFAttachments
		/// </summary>
		/// <param name="attachmentId"></param>
		/// <param name="workerId"></param>
		/// <param name="comments"></param>
		/// <param name="companyId"></param>
		private static void Insert(int attachmentId, int workerId, string comments, int companyId, int approverId)
		{
			DmsDatabaseInfo dmsInfo = EASyncEngine.GetDmsDatabaseFromCompanyId(companyId);
			if(dmsInfo == null)
				EASyncEngine.WriteMessageInEventLog("BBConnector, Insert(...) - " +
					string.Format(EASyncStrings.BBDmsInfoRetrievalError, 1));
			if(string.IsNullOrWhiteSpace(comments))
				comments = "no comments";

			TBConnection dmsConnection = null;
			TBCommand tbCommand = null;


			try
			{
				dmsConnection = new TBConnection(dmsInfo.DMSConnectionString, DBMSType.SQLSERVER);
				dmsConnection.Open();

				string query = "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; " +
							"BEGIN TRANSACTION; " +
							"UPDATE [dbo].[DMS_WFAttachments] " +
							"SET RequestComments = @comment "+
							"WHERE AttachmentID = @attachmentId " +
							"AND WorkerID= @workerId; " +
							"IF @@ROWCOUNT = 0 BEGIN " +
							"INSERT INTO [dbo].[DMS_WFAttachments] " +
							"([AttachmentID], [WorkerID], [RequestComments], [ApproverID]) VALUES "+
							"(@attachmentId, @workerId, @comment, @approverId) " +
							"END COMMIT TRANSACTION;";
				tbCommand = new TBCommand(query, dmsConnection);
				//imposto i parametri
				tbCommand.Parameters.Add("@comment", comments.Left(1024));
				tbCommand.Parameters.Add("@attachmentId", attachmentId);
				tbCommand.Parameters.Add("@workerId", workerId);
				tbCommand.Parameters.Add("@approverId", approverId);
					
				//eseguo la query
				tbCommand.ExecuteNonQuery();
			}
			catch(SqlException sqlExc)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, Insert(...) - errore in fase di connessione al Db: \n" + sqlExc.Message);
			}
			catch(Exception e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, Insert(...) - errore generico durante l'inserimento: \n" + e.Message);
			}
			finally
			{
				if(tbCommand != null)
					tbCommand.Dispose();
				
				if(dmsConnection != null && dmsConnection.State != ConnectionState.Closed)
				{
					dmsConnection.Close();
					dmsConnection.Dispose();
				}
			}
		}

		/// <summary>
		/// Metodo utilizzato per l'approvazione di un allegato da parte del EAConnector utilizzato da Brain Business
		/// </summary>
		/// <param name="attachmentId"></param>
		/// <param name="requesterUserName"></param>
		/// <param name="approvalUserName"></param>
		/// <param name="approvalStatus"></param>
		/// <param name="approvalComments"></param>
		internal static void Approve(int attachmentId, string requesterUserName, string approvalUserName, int approvalStatus, string approvalComments)
		{
			int workerId = GetWorkerIdFromBbUserName(requesterUserName);
			int approverId = GetWorkerIdFromBbUserName(approvalUserName);
			int companyId = GetCompanyIdFromBbUserName(requesterUserName);
			if(GetCompanyIdFromBbUserName(approvalUserName) != companyId)
				EASyncEngine.WriteMessageInEventLog("BBConnector, Approve(...) - errore nei parametri: richiedente e approvatore appartengono a due diverse company");

			DmsDatabaseInfo dmsInfo = EASyncEngine.GetDmsDatabaseFromCompanyId(companyId);
			if(dmsInfo == null)
				EASyncEngine.WriteMessageInEventLog("BBConnector, Approve(...) - errore in fase di recupero dmsInfo dal companyId");
			if(string.IsNullOrWhiteSpace(approvalComments))
				approvalComments = "no comments";

			TBConnection dmsConnection = null;
			TBCommand tbCommand = null;

			try
			{
				dmsConnection = new TBConnection(dmsInfo.DMSConnectionString, DBMSType.SQLSERVER);
				dmsConnection.Open();

				string query = "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; " +
                            "BEGIN TRANSACTION; " +
                            "UPDATE [dbo].[DMS_WFAttachments] " +
                            "SET ApprovalStatus = @approvalStatus " +
                            ", ApproverID = @approverId " +
                            ", ApprovalDate = getdate() " +
                            ", ApprovalComments = @approvalComments " +
                            "WHERE AttachmentID = @attachmentId " +
                            "AND WorkerID= @workerId; " +
                            "IF @@ROWCOUNT = 0 BEGIN " +
                            "INSERT INTO [dbo].[DMS_WFAttachments] " +
                            "([AttachmentID], [WorkerID], [ApprovalStatus], [ApproverID], [ApprovalDate], [ApprovalComments]) VALUES " +
                            "(@attachmentId, @workerId, @approvalStatus, @approverId, getdate(), @approvalComments) " +
                            "END COMMIT TRANSACTION;";
				tbCommand = new TBCommand(query, dmsConnection);
				//imposto i parametri
				tbCommand.Parameters.Add("@approvalStatus", approvalStatus);
				tbCommand.Parameters.Add("@approverId", approverId);
				tbCommand.Parameters.Add("@approvalComments", approvalComments.Left(1024));
				tbCommand.Parameters.Add("@attachmentId", attachmentId);
				tbCommand.Parameters.Add("@workerId", workerId);
				//eseguo la query
				tbCommand.ExecuteNonQuery();
			}
			catch(SqlException sqlExc)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, Approve(...) - errore in fase di connessione al Db : \n" + sqlExc.Message);
			}
			catch(Exception e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, Approve(...) - errore durante l'approvazione dell'allegato: \n" + e.Message);
			}
			finally
			{
				if(tbCommand != null)
					tbCommand.Dispose();

				if(dmsConnection != null && dmsConnection.State != ConnectionState.Closed)
				{
					dmsConnection.Close();
					dmsConnection.Dispose();
				}
			}
		}

		/// <summary>
		/// Metodo utilizzato per recuperare il contenuto del campo description a partire dall'id e dalla company di un allegato
		/// </summary>
		/// <param name="attachmentId"></param>
		/// <param name="requesterUserName"></param>
		/// <returns></returns>
		internal static string GetDescription(int attachmentId, string requesterUserName)
		{
			string description = "Errore nel recupero della descrizione";
			
			if(string.IsNullOrEmpty(requesterUserName))
				EASyncEngine.WriteMessageInEventLog("BBConnector, GetDescription(...) - errore input argument requesterUserName");
			
			int workerId = GetWorkerIdFromBbUserName(requesterUserName);
			int companyId = GetCompanyIdFromBbUserName(requesterUserName);
			DmsDatabaseInfo dmsInfo = EASyncEngine.GetDmsDatabaseFromCompanyId(companyId);
			if(dmsInfo == null)
				EASyncEngine.WriteMessageInEventLog("BBConnector, GetDescription(...) - errore in fase di recupero dmsInfo dal companyId");

			TBConnection dmsConnection = null;
			TBCommand tbCommand = null;

			try
			{
				dmsConnection = new TBConnection(dmsInfo.DMSConnectionString, DBMSType.SQLSERVER);
				
				dmsConnection.Open();

				string queryString = "SELECT [Description] " +
                                    "FROM [dbo].[DMS_Attachment] " +
                                    "WHERE AttachmentID= @attachmentId;";
				tbCommand = new TBCommand(queryString, dmsConnection);
				//imposto i parametri
				tbCommand.Parameters.Add("@attachmentId", attachmentId);
				//eseguo la query
				var returnedDescription = tbCommand.ExecuteScalar();
				description = (string)returnedDescription;
			}
			catch(SqlException sqlEx)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, GetDescription(...) - errore in fase di esecuzione della query: \n" + sqlEx.Message);
			}
			catch(Exception e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, GetDescription(...) - errore in fase di recuperi della descrizione di una richiesta di approvazione: \n" + e.Message);
			}
			finally
			{
				if(tbCommand != null)
					tbCommand.Dispose();

				if(dmsConnection != null && dmsConnection.State != ConnectionState.Closed)
				{
					dmsConnection.Close();
					dmsConnection.Dispose();
				}
			}
			return description;
		}

		/// <summary>
		/// Metodo utilizzato per recuperare aggiungere un worker di mago tra
		/// gli utenti di Brain Business
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="workerId"></param>
		/// <param name="firstName"></param>
		/// <param name="lastName"></param>
		/// <param name="password"></param>
		internal static bool AddUser(int companyId, int workerId, string firstName = null, string lastName = null, string password = null)
		{

			/////////////////////////////////
			DmsDatabaseInfo dmsInfo = EASyncEngine.GetDmsDatabaseFromCompanyId(companyId);
			if(dmsInfo == null)
				EASyncEngine.WriteMessageInEventLog("BBConnector, Approve(...) - errore in fase di recupero dmsInfo dal companyId");
			string companyName = dmsInfo.Company;
			/////////////////////////////////

			string userName = BuildBrainBusinessUserName(companyId, workerId);
			if(password == null)
				password = "BrainBusiness";
			if(string.IsNullOrWhiteSpace(firstName))
				firstName = userName;
			if(string.IsNullOrWhiteSpace(lastName))
				lastName = userName;

			//inizializzo il workflowContext con gli attributi necessari per il metodo DispatchEvent
			var myWorkFlowContext = new BBServiceReference.WorkflowContext
			{
				ApplicationConnectionCode = companyName,
				ApplicationProviderEventID = AddUserEvent,
				ApplicationProviderID = ApplicationProviderId
			};
			//aggiungo i parametri al contesto.
			//Questi parametri saranno quelli attesi dallo specifico evento che vogliamo chiamare
			myWorkFlowContext.Parameters = new Dictionary<string, object>();
			myWorkFlowContext.Parameters.Add("Username", userName);
			myWorkFlowContext.Parameters.Add("FirstName", firstName);
			myWorkFlowContext.Parameters.Add("LastName", lastName);
			myWorkFlowContext.Parameters.Add("CompanyName", companyName);
			myWorkFlowContext.Parameters.Add("Password", password);
			//faccio lo stesso con l'applicationContext
			var myApplicationContext = new BBServiceReference.ApplicationContext
			{
				ObjectID = userName,
				ObjectType = "Sent User",
				//todo: vedere se bisogna sostituire il worker con il brainBusinessUserName
				Username = workerId.ToString()
			};

			try
			{
				//var bbServiceRef = new BBServiceReference.WorkflowServiceClient();
				var bbServiceRef = BBServiceUtility.GetBBServiceClient(GetBBServiceUrl());
				var ProcessGuid = bbServiceRef.CreateWorkflow(myWorkFlowContext, EaAddUserNew, myApplicationContext);
				//sottoscrivo il web service delle notifiche agli aggiornamenti riguardanti questo particolare processo
				var registered = bbServiceRef.Subscribe(ProcessGuid, userName, NotificationServiceUrl);
				return true;

			}
			catch(EndpointNotFoundException e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, AddUser(...) - errore durante l'invio dell'evento: \n" + 
					e.Message + "\n\nControllare che Brain Business sia installato e tutti i servizi siano avviati");
			}
			catch(ServerTooBusyException e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, AddUser(...) - errore durante l'invio dell'evento: \n" + 
					e.Message + "\n\nControllare che Brain Business sia installato e tutti i servizi siano avviati");
			}
			catch(Exception e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, AddUser(...) - errore generico durante l'invio dell'evento: \n" + e.Message);
			}
			return false;

		}

		/// <summary>
		/// Metodo utilizzato per far partire un processo di test su BB studio.
		/// NB: Assicurarsi di aver aggiunto la definizione del processo alla connessione con 
		/// la specifica company da cui viene chiamato
		/// </summary>
		/// <param name="companyId">l'id della company</param>
		/// <param name="workerId">l'id del mandante</param>
		/// <returns></returns>
		internal static bool LaunchBBProcessTest(int companyId, int workerId)
		{
			var ProcessGuid = new System.Guid();
			//-----------------------------------------------------------------------
			var RequesterUserName = BuildBrainBusinessUserName(companyId, workerId);
			//-----------------------------------------------------------------------

			DmsDatabaseInfo dmsInfo = EASyncEngine.GetDmsDatabaseFromCompanyId(companyId);
			string companyName = dmsInfo.Company;

			//inizializzo il workflowContext con gli attributi necessari per il metodo DispatchEvent
			var myWorkFlowContext = new BBServiceReference.WorkflowContext
			{
				ApplicationConnectionCode = companyName,
				ApplicationProviderEventID = TestProcessEvent,
				ApplicationProviderID = ApplicationProviderId
			};
			//aggiungo i parametri al contesto.
			//Questi parametri sono quelli attesi dallo specifico evento che vogliamo chiamare
			myWorkFlowContext.Parameters = new Dictionary<string, object>();
			myWorkFlowContext.Parameters.Add("RequesterUserName", RequesterUserName);

			//faccio lo stesso con l'applicationContext
			var myApplicationContext = new BBServiceReference.ApplicationContext
			{
				ObjectType = "testDiProcesso",
				//todo: vedere se bisogna sostituire il worker con il brainBusinessUserName
				Username = RequesterUserName
			};

			try
			{
				//rappresenta il webService a cui invieremo gli eventi che verranno gestiti dai processi
				var bbServiceRef = BBServiceUtility.GetBBServiceClient(GetBBServiceUrl());
				//faccio partire un particolare processo, in seguito ad un particolare evento

				ProcessGuid = bbServiceRef.CreateWorkflow(myWorkFlowContext, TestDiProcessoDefinition, myApplicationContext);
				
				//sottoscrivo il web service delle notifiche agli aggiornamenti riguardanti questo particolare processo
				bbServiceRef.Subscribe(ProcessGuid, RequesterUserName, NotificationServiceUrl);

				return true;

			}
			catch(ServerTooBusyException e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, LaunchBBProcessTest(...) - errore 1 durante l'invio dell'evento: \n" +
					e.Message + "\n\nControllare che Brain Business sia installato e tutti i servizia siano avviati");
			}
			catch(EndpointNotFoundException e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, LaunchBBProcessTest(...) - errore 2 durante l'invio dell'evento: \n" +
					e.Message + "\n\nControllare che Brain Business sia installato e tutti i servizia siano avviati");
			}
			catch(FaultException e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, LaunchBBProcessTest(...) - errore 3 durante l'invio dell'evento: \n" + e.Message +
					"\n\nControlla di aver registrato la connessione su BrainBusiness Studio per questa company (" + companyName + ")");
			}
			catch(Exception e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, LaunchBBProcessTest(...) - errore generico durante l'invio dell'evento: \n" + e.Message);
			}
			return false;
		}

		/// <summary>
		/// Metodo utilizzato per far eliminare una FormInstance
		/// </summary>
		/// <param name="companyId">l'id della company</param>
		/// <param name="workerId">l'id del mandante</param>
		/// <returns></returns>
		internal static bool DeleteFormInstance(int companyId, int workerId, int formInstanceId)
		{
			var ProcessGuid = new System.Guid();
			//-----------------------------------------------------------------------
			var RequesterUserName = BuildBrainBusinessUserName(companyId, workerId);
			//-----------------------------------------------------------------------

			DmsDatabaseInfo dmsInfo = EASyncEngine.GetDmsDatabaseFromCompanyId(companyId);
			string companyName = dmsInfo.Company;

			//inizializzo il workflowContext con gli attributi necessari per il metodo DispatchEvent
			var myWorkFlowContext = new BBServiceReference.WorkflowContext
			{
				ApplicationConnectionCode = companyName,
				ApplicationProviderEventID = DeleteFormInstanceEvent,
				ApplicationProviderID = ApplicationProviderId
			};
			//aggiungo i parametri al contesto.
			//Questi parametri sono quelli attesi dallo specifico evento che vogliamo chiamare
			myWorkFlowContext.Parameters = new Dictionary<string, object>();
			myWorkFlowContext.Parameters.Add("RequesterUserName", RequesterUserName);
			myWorkFlowContext.Parameters.Add("FormInstanceId", formInstanceId);

			//faccio lo stesso con l'applicationContext
			var myApplicationContext = new BBServiceReference.ApplicationContext
			{
				ObjectType = "DeleteFormInstance",
				//todo: vedere se bisogna sostituire il worker con il brainBusinessUserName
				Username = RequesterUserName
			};

			try
			{
				//rappresenta il webService a cui invieremo gli eventi che verranno gestiti dai processi
				var bbServiceRef = BBServiceUtility.GetBBServiceClient(GetBBServiceUrl());

				//faccio partire un particolare processo, in seguito ad un particolare evento
				ProcessGuid = bbServiceRef.CreateWorkflow(myWorkFlowContext, DeleteFormInstanceDefinition, myApplicationContext);

				//sottoscrivo il web service delle notifiche agli aggiornamenti riguardanti questo particolare processo
				bbServiceRef.Subscribe(ProcessGuid, RequesterUserName, NotificationServiceUrl);

				return true;

			}
			catch(ServerTooBusyException e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, DeleteFormInstance(...) - errore 1 durante l'invio dell'evento: \n" +
					e.Message + "\n\nControllare che Brain Business sia installato e tutti i servizia siano avviati");
			}
			catch(EndpointNotFoundException e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, DeleteFormInstance(...) - errore 2 durante l'invio dell'evento: \n" +
					e.Message + "\n\nControllare che Brain Business sia installato e tutti i servizia siano avviati");
			}
			catch(FaultException e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, DeleteFormInstance(...) - errore 3 durante l'invio dell'evento: \n" + e.Message +
					"\n\nControlla di aver registrato la connessione su BrainBusiness Studio per questa company (" + companyName + ")");
			}
			catch(Exception e)
			{
				EASyncEngine.WriteMessageInEventLog("BBConnector, DeleteFormInstance(...) - errore generico durante l'invio dell'evento: \n" + e.Message);
			}
			return false;
		}

		/// <summary>
		/// Partendo dal workerId e dal nome della company costruisce lo userName (univoco)
		/// per BrainBusiness
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="workerId"></param>
		/// <returns></returns>
		internal static string BuildBrainBusinessUserName(int companyId, int workerId)
		{
			return "Company:"+ companyId.ToString() + "/Worker:" + workerId.ToString();
		}

		/// <summary>
		/// Partendo dal BrainBusiness UserName, recupero il nome della company
		/// </summary>
		/// <param name="BBUserName"></param>
		/// <returns></returns>
		internal static int GetCompanyIdFromBbUserName(string BBUserName)
		{
			int firstColonIndex = BBUserName.IndexOf(':');
			int slashIndex = BBUserName.IndexOf('/');
			int startPosition= firstColonIndex + 1;
			var substr = BBUserName.Substring(startPosition, slashIndex-startPosition);
			return Int32.Parse(substr);
		}

		/// <summary>
		/// Partendo dal BrainBusiness UserName, recupero il WorkerId
		/// </summary>
		/// <param name="BBUserName"></param>
		/// <returns></returns>
		internal static int GetWorkerIdFromBbUserName(string BBUserName)
		{
			int lastColonIndex = BBUserName.LastIndexOf(':');
			var subrstr = BBUserName.Substring(lastColonIndex + 1);
			return Int32.Parse(subrstr);
		}
	}

	public static class BBServiceUtility
	{
		public static BBServiceReference.WorkflowServiceClient GetBBServiceClient(string siteUrl)
		{
			Uri serviceUri = new Uri(siteUrl);
			EndpointAddress endpointAddress = new EndpointAddress(serviceUri);

			//Create the binding here
			System.ServiceModel.Channels.Binding binding = BindingFactory.CreateInstance();

			BBServiceReference.WorkflowServiceClient client = new BBServiceReference.WorkflowServiceClient(binding, endpointAddress);
			return client;
		}
	}

	public static class NotificationServiceUtility
	{
		/// <summary>
		/// restituisce il client a partire dall'url
		/// </summary>
		/// <param name="siteUrl"></param>
		/// <returns></returns>
		public static NotificationServiceClient GetNotificationServiceClient(string siteUrl)
		{
			Uri serviceUri = new Uri(siteUrl);
			EndpointAddress endpointAddress = new EndpointAddress(serviceUri);

			//Create the binding here
			System.ServiceModel.Channels.Binding binding = BindingFactory.CreateInstance();

			NotificationServiceClient client = new NotificationServiceClient(binding, endpointAddress);
			return client;
		}
	}
}
