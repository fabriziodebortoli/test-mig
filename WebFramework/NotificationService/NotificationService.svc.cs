using System;
using PAT.Workflow.Runtime.BBNotificationListener;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Interfaces;
using System.Collections.Generic;

namespace NotificationService
{
    public class NoficationService : INotificationService, IBrainBusinessNotificationListener
    {
        private static NotificationCore notificationCore = null;
        /// <summary>
        /// Referenza all'applicazione, la prima volta che la si utilizza chiama in automatico il costruttore statico
        /// </summary>
        internal static NotificationCore NotificationCore { get { return notificationCore; } }

        static NoficationService() {
            notificationCore = new NotificationCore();
        }

		/// <summary>
		/// classico metodo per testare la connessione con il WS
		/// </summary>
		/// <returns></returns>
		public bool IsAlive()
		{
			return true;
		}

		/// <summary>
		/// Metodo esposto da IBrainBusinessNotificationListener e
		/// richiamato da Brain Business per notificare aggiornamenti sui processi
		/// </summary>
		/// <param name="workflowID"></param>
		/// <param name="username"></param>
        public void Notify(Guid workflowID, string username)
        {
            NotificationCore.DispatchBBNotifications(workflowID, username);
			
			//UnComment follow lines to log notifications in the console
#if DEBUG
			NotificationCore.LogNofitications(workflowID, username);
#endif
        }

		void IBrainBusinessNotificationListener.ReceiveEvent(Event eventData)
		{
			NotificationCore.DispatchBBEvent(eventData);
		}

		/// <summary>
		/// Prova funzionamento
		/// </summary>
		/// <param name="message"></param>
		/// <param name="username"></param>
		public void SendMessage(string message, int workerId, int companyId)
		{
			NotificationCore.DispatchMessage(message, workerId, companyId);
		}

		/// <summary>
		/// Prova funzionamento
		/// </summary>
		/// <param name="message"></param>
		/// <param name="username"></param>
		public void SendIGenericNotify(GenericNotify notify, bool StoreOnDb)
		{
			NotificationCore.SendGenericNotify(notify, StoreOnDb);
		}

		/// <summary>
		/// Restituisce un array con tutte le notifiche richieste, ovvero riguardanti il worker
		/// selezionato e se devono essere o meno incluse quelle processate
		/// </summary>
		/// <param name="workerId"></param>
		/// <param name="companyId"></param>
		/// <param name="includeProcessed"></param>
		/// <returns></returns>
		public GenericNotify[] GetAllIGenericNotify(int workerId, int companyId, bool includeProcessed)
		{
			return notificationCore.GetAllIGenericNotify(workerId, companyId, includeProcessed); 
		}

		/// <summary>
		/// Fa un update sul db settando a now(momento dlla query) il campo readDate
		/// </summary>
		/// <param name="NotificationId"></param>
		/// <returns></returns>
		public bool SetNotificationAsRead(int NotificationId)
		{
			return notificationCore.SetNotificationAsRead(NotificationId);
		}

        /// <summary>
        /// Metodo per richiedere una specifica form, identificata da un identificatore intero incrementale
        /// </summary>
        /// <param name="formInstanceId">identificatore della form (lato Brain)</param>
        /// <returns>una MyBBForm, ovvero una struttura contenente lo schema xml della forma e 
        /// l'identificatore della form (per rispondere alla form)</returns>
        public MyBBFormSchema GetBBForm(int formInstanceId) 
        {
            return NotificationCore.GetForm(formInstanceId);
        }

        /// <summary>
        /// Metodo per inviare una form compilata a BrainBusiness
        /// </summary>
        /// <param name="compiledForm">la struttura contenente l'xml rappresentante la form (compilata)
        /// e l'identificatore univoco della form</param>
        /// <returns>true se la form è stata inviata correttamente a BrainBusiness</returns>
        public bool SetBBForm(MyBBFormSchema myBBForm) 
        {
			//qui posso notificare il "sender" di aggiornarsi, questo perchè se rispondo a una form da notificaiton manager1 (viewer),
			//non riesco ad aggiornare il notification manager2(traylet) -> se lo faccio, sarebbe bene usare un nuovo tipo di notifica con enum tipo
            return NotificationCore.SetForm(myBBForm);
        }

        /// <summary>
        /// Restituisce l'url su cui gira XSocket, il "motore" delle notifiche. L'url restituito deve poi
        /// essere utilizzato per creare gli XSocketClient (lato client e server)
        /// </summary>
        /// <returns>l'url di riferimento per gli XSocketClient</returns>
        public string GetXSocketControllerUrl()
        {
            return notificationCore.GetControllerUrl();
        }

		public string GetBrainBusinessServiceUrl()
		{
			return NotificationCore.GetBrainBusinessServiceUrl();
		}

		public bool UpdateBrainBusinessServiceUrl(string url)
		{
			return NotificationCore.UpdateOrInsertBrainBusinessServiceUrl(url);
		}

		/// <summary>
		/// Metodo per richiedere tutte le form per un particolare utente, considerare di fare un metodo più efficiente per
		/// recuperare solo le form ancora da processare o solamente quelle già processate
		/// </summary>
		/// <param name="workerId"></param>
		/// <param name="companyId"></param>
		/// <returns></returns>
		public MyBBFormInstance[] GetAllBBFormInstances(int workerId, int companyId, bool includeProcessed)
		{
			return NotificationCore.GetAllFormInstances(workerId, companyId, includeProcessed);
		}

		public Father TestInheritance(Father father)
		{
			return father;
		}

		//------------------------chat methods-------------------------------

		public void SendChatMessage(GenericNotify notify)
		{
			throw new NotImplementedException();
		}
	}
}
