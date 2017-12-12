using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using System.IO;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.WebServices.EasyAttachmentSync {
	internal class BBConnector {

		private enum Status { NotApproved=0, Approved, Rejected, UnDefined};
		
		//-------------------identificativo del connettore per EasyAttachments---------------------
		private static readonly Guid ApplicationProviderId = new System.Guid("123ED732-D3ED-4328-8E1B-93334E8422B3");
		
		//-------identificativi degli eventi esposti dal connettore per EasyAttachments------------
		
		//NewAttachmentByIdEvent - parametri in ingresso: AttachmentId int, CompanyId int, workerId int, comments string
		private static readonly Guid NewAttachmentByIdEvent = new System.Guid("123B495A-0F7D-41A6-A696-B99AC94576DF");

		//NewAttachmentByDescriptionEvent - parametri in ingresso: Description string, CompanyId int
		private static readonly Guid DeleteUser = new System.Guid("012B495A-0F7D-41A6-A696-B99AC94576DF");

        //Add User - parametri in ingresso: UserName string, Password string, FirstName string
        private static readonly Guid AddUserEvent = new System.Guid("789B495A-0F7D-41A6-A696-B99AC94576DF");
		
		//------------identificativi dei processi creati con Brain Business Studio------------------
		private static readonly Guid EaAddUserNew = new System.Guid("ce76c5ad-d324-450c-8557-5631529a0f70");
        private static readonly Guid EaSimpleApprovation = new System.Guid("13bf50b0-1dd2-4ae3-a1b6-7720626d81fb");
        private static readonly Guid FieldsTestDefinition = new System.Guid("0131829f-8885-4c22-a097-41093b2090bb");


        private static readonly string NotificationServiceUrl =
                                                    //Per Test:
                                                    //"http://localhost/MicroService/Service1.svc"
                                                    //Per Sviluppo:
                                                    "http://localhost/DEVELOPMENT_3X_EA/NotificationService/NotificationService.svc"
                                                    ;
        /// <summary>
        /// Gira l'evento a un particolare processo di BrainBusiness
        /// </summary>
        /// <param name="attachmentId"></param>
        /// <param name="companyId"></param>
        /// <param name="workerId"></param>
        /// <param name="comments"></param>
        /// <returns>il guid del processo</returns>
        internal static System.Guid NewAttachmentById(int attachmentId, int companyId, int workerId, string comments) {
			if(comments==null)
				EASyncEngine.WriteMessageInEventLog("BBConnector, NewAttachmentById(...) - comments = null");
			var ProcessGuid=new System.Guid();
			
            //nome della company = application code di BrainBusiness
            DmsDatabaseInfo dmsInfo = EASyncEngine.GetDmsDatabaseFromCompanyId(companyId);
            string companyName = dmsInfo.Company;
            
			//inizializzo il workflowContext con gli attributi necessari per il metodo DispatchEvent
			var myWorkFlowContext = new BBServiceReference.WorkflowContext {
				ApplicationConnectionCode = companyName,
				ApplicationProviderEventID = NewAttachmentByIdEvent,
				ApplicationProviderID = ApplicationProviderId
			};
            //aggiungo i parametri al contesto.
			//Questi parametri saranno quelli attesi dallo specifico evento che vogliamo chiamare
			myWorkFlowContext.Parameters = new Dictionary<string, object>();
			myWorkFlowContext.Parameters.Add("AttachmentId", attachmentId);
            myWorkFlowContext.Parameters.Add("RequesterUserName", BuildBrainBusinessUserName(companyId,workerId));
			myWorkFlowContext.Parameters.Add("Comments", comments);
            myWorkFlowContext.Parameters.Add("ApprovalUserName", BuildBrainBusinessUserName(companyId, 1));

            //faccio lo stesso con l'applicationContext
            var myApplicationContext = new BBServiceReference.ApplicationContext
            {
                ObjectID = attachmentId.ToString(),
                ObjectType = "fakeAttachment",
                //todo: vedere se bisogna sostituire il worker con il brainBusinessUserName
                Username = BuildBrainBusinessUserName(companyId, workerId)
            };
            
            try {
				//rappresenta il webService a cui invieremo gli eventi che verranno gestiti dai processi
				var bbServiceRef = new BBServiceReference.WorkflowServiceClient();
                //faccio partire un particolare processo, in seguito ad un particolare evento
				
//old style->   //var guid= bbServiceRef.DispatchEventForDefinitionByIdentifier(myWorkFlowContext, EADefinitionComplessa);
                ProcessGuid = bbServiceRef.CreateWorkflow(myWorkFlowContext, EaSimpleApprovation, myApplicationContext);
//per test->    bbServiceRef.CreateWorkflow(myWorkFlowContext, FieldsTestDefinition, myApplicationContext);
				Insert(attachmentId, workerId, comments, companyId);

                //sottoscrivo il web service delle notifiche agli aggiornamenti riguardanti questo particolare processo
                var registered = bbServiceRef.Subscribe(ProcessGuid, BuildBrainBusinessUserName(companyId,workerId), NotificationServiceUrl);

            } catch (Exception e) {
				EASyncEngine.WriteMessageInEventLog("BBConnector, NewAttachmentById(...) - errore durante l'invio dell'evento: \n" + e.Message);
			}
            return ProcessGuid;
		}

		/// <summary>
        /// Metodo utilizzato per inserire una richiesta di approvazione di un allegato 
        /// all'interno della tabella DMS_WFAttachment
		/// </summary>
		/// <param name="attachmentId"></param>
		/// <param name="workerId"></param>
		/// <param name="comments"></param>
		/// <param name="companyId"></param>
        private static void Insert(int attachmentId, int workerId, string comments, int companyId) {
            DmsDatabaseInfo dmsInfo = EASyncEngine.GetDmsDatabaseFromCompanyId(companyId);
			if (dmsInfo == null)
				EASyncEngine.WriteMessageInEventLog("BBConnector, Insert(...) - errore in fase di recupero dmsInfo dal companyId");
            if (comments == null)
                comments = "no comments";
            try {
				using (var dmsConnection = new TBConnection(dmsInfo.DMSConnectionString, DBMSType.SQLSERVER)) {
					dmsConnection.Open();

					string query = "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; " +
								"BEGIN TRANSACTION; " +
								"UPDATE [dbo].[DMS_WFAttachment] " +
								"SET RequestComments = @comment "+
								"WHERE AttachmentID = @attachmentId " +
								"AND WorkerID= @workerId; " +
								"IF @@ROWCOUNT = 0 BEGIN " +
								"INSERT INTO [dbo].[DMS_WFAttachment] " +
								"([AttachmentID], [WorkerID], [RequestComments]) VALUES "+ 
                                "(@attachmentId, @workerId, @comment) " +
								"END COMMIT TRANSACTION;";
					var tbCommand = new TBCommand(query, dmsConnection);
					//imposto i parametri
                    tbCommand.Parameters.Add("@comment", comments);
                    tbCommand.Parameters.Add("@attachmentId", attachmentId);
                    tbCommand.Parameters.Add("@workerId", workerId);
                    //eseguo la query
                    tbCommand.ExecuteNonQuery();

					dmsConnection.Close();
				}
			} catch (Exception e) {
				EASyncEngine.WriteMessageInEventLog("BBConnector, Insert(...) - errore in fase di connessione al Db : \n" + e.Message);
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
            if (dmsInfo == null)
                EASyncEngine.WriteMessageInEventLog("BBConnector, Approve(...) - errore in fase di recupero dmsInfo dal companyId");
            if (approvalComments == null)
                approvalComments = "no comments";
            try
            {
                using (var dmsConnection = new TBConnection(dmsInfo.DMSConnectionString, DBMSType.SQLSERVER))
                {
                    dmsConnection.Open();

                    string query = "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; " +
                                "BEGIN TRANSACTION; " +
                                "UPDATE [dbo].[DMS_WFAttachment] " +
                                "SET ApprovalStatus = @approvalStatus " +
                                ", ApproverID = @approverId " +
                                ", ApprovalDate = getdate() " +
                                ", ApprovalComments = @approvalComments " +
                                "WHERE AttachmentID = @attachmentId " +
                                "AND WorkerID= @workerId; " +
                                "IF @@ROWCOUNT = 0 BEGIN " +
                                "INSERT INTO [dbo].[DMS_WFAttachment] " +
                                "([AttachmentID], [WorkerID], [ApprovalStatus], [ApproverID], [ApprovalDate], [ApprovalComments]) VALUES " +
                                "(@attachmentId, @workerId, @approvalStatus, @approverId, getdate(), @approvalComments) " +
                                "END COMMIT TRANSACTION;";
                    var tbCommand = new TBCommand(query, dmsConnection);
                    //imposto i parametri
                    tbCommand.Parameters.Add("@approvalStatus", approvalStatus);
                    tbCommand.Parameters.Add("@approverId", approverId);
                    tbCommand.Parameters.Add("@approvalComments", approvalComments);
                    tbCommand.Parameters.Add("@attachmentId", attachmentId);
                    tbCommand.Parameters.Add("@workerId", workerId);
                    //eseguo la query
                    tbCommand.ExecuteNonQuery();

                    dmsConnection.Close();
                }
            }
            catch (Exception e)
            {
                EASyncEngine.WriteMessageInEventLog("BBConnector, Approve(...) - errore in fase di connessione al Db : \n" + e.Message);
            }
        }
		
		/// <summary>
        /// Metodo utilizzato per recuperare il contenuto del campo description a partire dall'id e dalla company di un allegato
		/// </summary>
		/// <param name="attachmentId"></param>
		/// <param name="requesterUserName"></param>
		/// <returns></returns>
		internal static string GetDescription(int attachmentId, string requesterUserName) {
			string description = "Errore nel recupero della descrizione";
            if(requesterUserName==null)
                EASyncEngine.WriteMessageInEventLog("BBConnector, GetDescription(...) - errore input argument requesterUserName");
            int workerId = GetWorkerIdFromBbUserName(requesterUserName);
            int companyId = GetCompanyIdFromBbUserName(requesterUserName);
            DmsDatabaseInfo dmsInfo = EASyncEngine.GetDmsDatabaseFromCompanyId(companyId);
			if (dmsInfo == null){
				EASyncEngine.WriteMessageInEventLog("BBConnector, GetDescription(...) - errore in fase di recupero dmsInfo dal companyId");
			}try {
				using (var dmsConnection = new TBConnection(dmsInfo.DMSConnectionString, DBMSType.SQLSERVER)) {
					dmsConnection.Open();

                    string queryString = "SELECT [Description] " +
                                        "FROM [dbo].[DMS_Attachment] " +
                                        "WHERE AttachmentID= @attachmentId;";
					var tbCommand = new TBCommand(queryString, dmsConnection);
                    tbCommand.Parameters.Add("@attachmentId", attachmentId);
                    var returnedDescription = tbCommand.ExecuteScalar();
					description = (string)returnedDescription;
					dmsConnection.Close();
				}
			} catch (Exception e) {
				EASyncEngine.WriteMessageInEventLog("BBConnector, GetDescription(...) - errore in fase di esecuzione della query: \n" + e.Message);
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
        internal static void AddUser(int companyId, int workerId, string firstName = null, string lastName = null, string password = null)
        {
            
            /////////////////////////////////
            DmsDatabaseInfo dmsInfo = EASyncEngine.GetDmsDatabaseFromCompanyId(companyId);
            string companyName = dmsInfo.Company;
            /////////////////////////////////
            
            string userName = BuildBrainBusinessUserName(companyId, workerId);
            if (password == null)
                password = "BrainBusiness";
            if (firstName == null)
                firstName = userName;
            if (lastName == null)
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
                var bbServiceRef = new BBServiceReference.WorkflowServiceClient();
                var ProcessGuid = bbServiceRef.CreateWorkflow(myWorkFlowContext, EaAddUserNew, myApplicationContext);
                //sottoscrivo il web service delle notifiche agli aggiornamenti riguardanti questo particolare processo
                var registered = bbServiceRef.Subscribe(ProcessGuid, userName, "http://localhost/NotificationService/Service1.svc");

            }
            catch (Exception e)
            {
                EASyncEngine.WriteMessageInEventLog("BBConnector, AddUser(...) - errore durante l'invio dell'evento: \n" + e.Message);
            }

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
}
