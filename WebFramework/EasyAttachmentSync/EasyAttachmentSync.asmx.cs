using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Services;

namespace Microarea.WebServices.EasyAttachmentSync
{
	///<summary>
	/// WebService che si occupa della sincronizzazione degli indici di ricerca in EasyAttachment
	///</summary>
	//================================================================================
	[WebService(Namespace = "http://microarea.it/EasyAttachmentSync/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	//================================================================================
	public class MicroareaEasyAttachmentSync : System.Web.Services.WebService
	{
		[WebMethod]
		//-----------------------------------------------------------------------
		public bool Init(string authenticationToken)
		{
			// se il token e' diverso da quello previsto non procedo
			if (authenticationToken != "{2E8164FA-7A8B-4352-B0DC-479984070507}")
			{
				Debug.WriteLine("EasyAttachmentSync:Init(): authenticationToken not valid!");
				return false;
			}

			// se sto eseguendo la sincronizzazione non procedo
			if (EASyncEngine.IsSynchronizing)
				return false;

			try
			{
				// inizializzo il timer
				EASyncEngine.InitTimer();
			}
			catch (Exception ex)
			{
				EASyncEngine.WriteLog(ex);
				Debug.WriteLine(string.Format("An error occurred during EasyAttachmentSync:Init: {0}", ex.ToString()));
				throw ex;
			}

			return true;
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool IsAlive()
		{
			return true;
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool Clear()
		{
			return EASyncEngine.ClearDmsList();
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool IsFTSEnabled(int companyId)
		{
			return EASyncEngine.IsFTSEnabled(companyId);
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool Stop()
		{
			bool result = false;

			try
			{
				// fermo il timer
				result =  EASyncEngine.StopTimer();
			}
			catch (Exception ex)
			{
				EASyncEngine.WriteLog(ex);
				Debug.WriteLine(string.Format("An error occurred during EasyAttachmentSync:Stop: {0}", ex.ToString()));
				throw ex;
			}
			
			return result;
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool SuspendFTS(bool suspend, int companyId)
		{
			return EASyncEngine.SuspendFTS(suspend, companyId);
		}


		[WebMethod]
		//-----------------------------------------------------------------------
		public void EnqueueAttachmentsToSend(int companyId, List<int> attachmentIds, int loginId)
		{
			EASyncEngine.EnqueueAttachments(companyId, attachmentIds, loginId);
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool UpdateSOSDocumentsStatus(int companyId, out string message)
		{
			return EASyncEngine.UpdateSOSDocumentsStatus(companyId, out message);
		}


        //---------------------------------------------------------------------
        //----------------------BBConnector WebMethods-------------------------
        //---------------------------------------------------------------------

		//verifica la connessione con il servizio di Brain
		[WebMethod]
		//-----------------------------------------------------------------------
		public bool BBIsAlive()
		{
			return BBConnector.BBIsAlive();
		}


        //fa scattare l'evento NewAttachmentById a Brain Business
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool NewAttachmentById(int attachmentId, int companyId, int workerId, string comments)
        {
            return BBConnector.NewAttachmentById(attachmentId, companyId, workerId, comments);
        }

        //metodo richiamato da Brain Business per l'approvazione degli allegati
        [WebMethod]
        //-----------------------------------------------------------------------
        public void Approve(int attachmentId, string requesterUserName, string approvalUserName, int approvalStatus, string approvalComments)
        {
            BBConnector.Approve(attachmentId, requesterUserName, approvalUserName, approvalStatus, approvalComments);
        }

        //metodo richiamato da Brain Business per richiedere la descrizione dell'allegato
        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetDescription(int attachmentId, string requesterUserName)
        {
            return BBConnector.GetDescription(attachmentId, requesterUserName);
        }

        //metodo utilizzato per inserire gli utenti di mago in Brain Business
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool AddUser(int companyId, int workerId, string firstName = null, string lastName = null, string password = null)
        {
            return BBConnector.AddUser(companyId, workerId, firstName, lastName, password);
        }

		//metodo utilizzato per testare la connessione con BB
        [WebMethod]
        //-----------------------------------------------------------------------
		public bool LaunchBBProcessTest(int companyId, int workerId)
        {
			return BBConnector.LaunchBBProcessTest(companyId, workerId);
        }

		//metodo utilizzato per cancellare una formInstance
		[WebMethod]
		//-----------------------------------------------------------------------
		public bool DeleteFormInstance(int companyId, int workerId, int formInstanceId)
		{
			return BBConnector.DeleteFormInstance(companyId, workerId, formInstanceId);
		}

        //metodo utilizzato per recuperare il bb username da company e worker Ids
        [WebMethod]
        //-----------------------------------------------------------------------
        public string BuildBrainBusinessUserName(int companyId, int workerId)
        {
            return BBConnector.BuildBrainBusinessUserName(companyId, workerId);
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public int GetCompanyIdFromBbUserName(string bbUserName)
        {
            return BBConnector.GetCompanyIdFromBbUserName(bbUserName);
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public int GetWorkerIdFromBbUserName(string bbUserName)
        {
            return BBConnector.GetWorkerIdFromBbUserName(bbUserName);
        }

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool UpdateBBServiceUrl()
		{
			return BBConnector.UpdateBBServiceUrl();
		}


		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
		}
	}
}