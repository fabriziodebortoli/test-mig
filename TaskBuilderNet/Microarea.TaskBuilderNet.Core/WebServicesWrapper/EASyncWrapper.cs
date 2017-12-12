using System;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Core.eaSync;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	/// <summary>
	/// Wrapper per l'utilizzo di EasyAttachmentSync
	/// </summary>
	//============================================================================
	public class EasyAttachmentSync
	{
		private MicroareaEasyAttachmentSync easyAttachmentSync = new MicroareaEasyAttachmentSync();

		// evento da intercettare esternamente quando si utilizzano le chiamate ai metodi asincroni
		public event EventHandler<UpdateSOSDocumentsStatusCompletedEventArgs> UpdateSOSDocumentsStatusCompleted;

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="eaSyncUrl">Indirizzo di EasyAttachmentSync</param>
		//---------------------------------------------------------------------------
		public EasyAttachmentSync(string eaSyncUrl)
		{
			easyAttachmentSync.Url = eaSyncUrl;
            easyAttachmentSync.Timeout = -1; 
			easyAttachmentSync.UpdateSOSDocumentsStatusCompleted += new UpdateSOSDocumentsStatusCompletedEventHandler(EASync_UpdateSOSDocumentsStatusCompleted);
		}

		/// <summary>
		/// Reinizializza EasyAttachmentSync
		/// </summary>
		//-----------------------------------------------------------------------
		public bool Init(string authenticationToken)
		{
			string validToken = "{2E8164FA-7A8B-4352-B0DC-479984070507}";

			if (authenticationToken == validToken)
				return easyAttachmentSync.Init(validToken);

			return false;
		}

		/// <summary>
		/// Verifica se il web service risponde
		/// </summary>
		//-----------------------------------------------------------------------
		public bool IsAlive()
		{
			return easyAttachmentSync.IsAlive();
		}

		/// <summary>
		/// Pulisce le informazioni dei database di EA tenute in cache dal WS
		/// </summary>
		//-----------------------------------------------------------------------
		public bool Clear()
		{
			return easyAttachmentSync.Clear();
		}

		/// <summary>
		/// Dato un companyId ritorna se il database di EA associato ha il componente 
		/// FullText Search abilitato
		/// </summary>
		//-----------------------------------------------------------------------
		public bool IsFTSEnabled(int companyId)
		{
			return easyAttachmentSync.IsFTSEnabled(companyId);
		}

		/// <summary>
		/// Abilita/disabilita gli indici FullText search sulle tabelle di EA del database identificato dal companyId
		/// </summary>
		//-----------------------------------------------------------------------
		public bool SuspendFTS(bool suspend, int companyId)
		{
			return easyAttachmentSync.SuspendFTS(suspend, companyId);
		}

		/// <summary>
		/// Stoppa il servizio
		/// </summary>
		//-----------------------------------------------------------------------
		public bool Stop()
		{
			return easyAttachmentSync.Stop();
		}

		/// <summary>
		/// Dato un companyId esegue l'aggiornamento on demand degli indici di default
		/// </summary>
		//-----------------------------------------------------------------------
		public bool UpdateDefaultSearchIndexes(int companyId)
		{
			return easyAttachmentSync.UpdateDefaultSearchIndexes(companyId);
		}

		/// <summary>
		///  Aggiunge alla coda un set di selezioni di attachment da inviare in SOS
		/// </summary>
		//-----------------------------------------------------------------------
		public void EnqueueAttachmentsToSend(int companyId, List<int> attachmentIds, int loginId)
		{
			easyAttachmentSync.EnqueueAttachmentsToSend(companyId, attachmentIds.ToArray(), loginId);
		}

		/// <summary>
		///  Richiama l'aggiornamento dello stato dei documenti inviati in SOS
		/// </summary>
		//-----------------------------------------------------------------------
		public bool UpdateSOSDocumentsStatus(int companyId, out string message)
		{
			return easyAttachmentSync.UpdateSOSDocumentsStatus(companyId, out message);
		}

		/// <summary>
		/// Richiama l'aggiornamento dello stato dei documenti inviati in SOS in modalita' asincrona
		/// </summary>
		//-----------------------------------------------------------------------
		public void UpdateSOSDocumentsStatusAsync(int companyId)
		{
			// richiamo il webmethod asicrono creando tutte le volte un nuovo guid (passato come II param)
			// in modo da evitare chiamate multiple duplicate che genererebbero eccezioni
			easyAttachmentSync.UpdateSOSDocumentsStatusAsync(companyId, Guid.NewGuid());
		}

		///<summary>
		/// Rimpallo l'evento UpdateSOSDocumentsStatusCompletedEventArgs del metodo asincrono
		///</summary>
		//-----------------------------------------------------------------------
		public void EASync_UpdateSOSDocumentsStatusCompleted(object sender, UpdateSOSDocumentsStatusCompletedEventArgs e)
		{
			if (UpdateSOSDocumentsStatusCompleted != null)
				UpdateSOSDocumentsStatusCompleted(sender, e);
		}

		// Per Brain Business

		//verifica la connessione con il servizio di Brain
		public bool BBIsAlive()
		{
			return easyAttachmentSync.BBIsAlive();
		}

		/// <summary>
		/// Lancia l'evento omonimo a BrainBusiness
		/// </summary>
		/// <param name="attachmentId"></param>
		/// <param name="companyId"></param>
		/// <param name="workerId"></param>
		/// <param name="comments">i commenti del richiedente dell'approvazione (resi disponibili all'approvatore)</param>
		/// <returns></returns>
		public bool NewAttachmentById(int attachmentId, int companyId, int workerId = 999999, string comments = "no Comments")
		{
			return easyAttachmentSync.NewAttachmentById(attachmentId, companyId, workerId, comments);
		}

		/// <summary>
		/// Consente di aggiungere l'utente su BrainBusiness usando uno userName
		/// di questo tipo: Company:#companyId/Worker:#workerId
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="workerId"></param>
		/// <param name="firstName"></param>
		/// <param name="lastName"></param>
		/// <param name="password"></param>
		public bool AddUser(int companyId, int workerId, string firstName = null, string lastName = null, string password = null)
		{
			return easyAttachmentSync.AddUser(companyId, workerId, firstName, lastName, password);
		}

		/// <summary>
		/// Processo di test per verficare la connessione con Brain e l'esecuzione di un processo
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="workerId"></param>
		/// <returns></returns>
		public bool LaunchBBProcessTest(int companyId, int workerId)
		{
			return easyAttachmentSync.LaunchBBProcessTest(companyId, workerId);
		}

		/// <summary>
		/// Processo di test per elimninare una FormInstance
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="workerId"></param>
		/// <returns></returns>
		public bool DeleteFormInstance(int companyId, int workerId, int formInstanceId)
		{
			return easyAttachmentSync.DeleteFormInstance(companyId, workerId, formInstanceId);
		}

		/// <summary>
		/// Costruisce lo username "univoco" per Brain Business, a partire dal companyId e workerId
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="workerId"></param>
		/// <returns>"Company:#/Worker:#"</returns>
		public string BuildBrainBusinessUserName(int companyId, int workerId)
		{
			return easyAttachmentSync.BuildBrainBusinessUserName(companyId, workerId);
		}

		/// <summary>
		/// Recupera il companyId a partire dallo username per BrainBusiness
		/// </summary>
		/// <param name="bbUserName"></param>
		/// <returns>companyId</returns>
		public int GetCompanyIdFromBBUserName(string bbUserName)
		{
			return easyAttachmentSync.GetCompanyIdFromBbUserName(bbUserName);
		}

		/// <summary>
		/// Recupera il workerId a partire dallo username per BrainBusiness
		/// </summary>
		/// <param name="bbUserName"></param>
		/// <returns>workerId</returns>
		public int GetWorkerIdFromBBUserName(string bbUserName)
		{
			return easyAttachmentSync.GetWorkerIdFromBbUserName(bbUserName);
		}

		/// <summary>
		/// Aggiorna il valore dell'indirizzo del servizio di Brain Business usato per inviargli le richieste
		/// </summary>
		/// <param name="bbUserName"></param>
		/// <returns>workerId</returns>
		public bool UpdateBBServiceUrl()
		{
			return easyAttachmentSync.UpdateBBServiceUrl();
		}
	}
}
