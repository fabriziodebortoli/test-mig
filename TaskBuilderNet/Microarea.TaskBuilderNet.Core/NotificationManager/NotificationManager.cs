using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using SailorsPromises;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Microarea.TaskBuilderNet.Core.NotificationManager
{
	
	public class NotificationManager
	{
		private bool IsViewer;

		/// <summary>
		/// Il Wrapper che fa da ponte con il web service delle notifiche
		/// </summary>
		public NotificationServiceWrapper notificationServiceWrapper { get; private set; }

		public int WorkerId { get; private set; }
		public int CompanyId { get; private set; }

		private IList<BaseNotificationModule> BaseNotificationModules { get; set; }

		//Events
		//public event NewFormNotifyEventHandler NewFormNotify;
		
		public event ConnectionWithXSocketEventHandler ConnectionWithXSocketNotify;

		public event MessageNotifyEventHandler MessageNotify;

		public event EventHandler Notify;

		public event IGenericNotifyEventHandler IGenericNotify;

		/// <summary>
		/// Costruttore del Manager, a sua volta, questo, instanzia un Service Wrapper che funge da client XSocket
		/// e che fa da ponte per il NotificationService, inoltre contiene una lista di moduli di notifica a cui è possibile richiedere
		/// le notifiche tramite il metodo GetAllNotifications.
		/// I parametri workerId e companyId servono per registrarsi alle notifiche
		/// </summary>
		/// <param name="workerId"></param>
		/// <param name="companyId"></param>
		/// <param name="isViewer">consente di distinguere il comportamento dipendemente se si tratta 
		/// del notification manager del menu (attivo) o utilizzato altrove (passivo)</param>
		/// <param name="parent">la form in cui andare a sovrapporre le form di notifica</param>
		public NotificationManager(int workerId, int companyId, bool isViewer)
		{
			IsViewer = isViewer;
			
			NotificationManagerUtility.CheckNotificationServiceEventLog();

			WorkerId									= workerId;
			CompanyId									= companyId;

			notificationServiceWrapper					= new NotificationServiceWrapper(WorkerId, CompanyId);

			notificationServiceWrapper.ConnectionNotify += notificationServiceWrapper_ConnectionStatus;
			//notificationServiceWrapper.MessageNotify	+= notificationServiceWrapper_MessageNotify;
			//notificationServiceWrapper.IGenericNotify	+= notificationServiceWrapper_IGenericNotify;

			BaseNotificationModules = new List<BaseNotificationModule>();

			BaseNotificationModules.Add(new BBNotificationModule(notificationServiceWrapper, isViewer));
			BaseNotificationModules.Add(new GenericNotificationModule(notificationServiceWrapper, isViewer));
			BaseNotificationModules.Add(new ChatNotificationModule(notificationServiceWrapper, isViewer));

			//sottoscrizione sottostante commentata per avere un feedback, ovvero: 
			//prima creo il manager, 
			//mi registro alle notifiche 
			//e poi sottoscrivo

			//SubscribeToTheNotificationService(); 
			RegisterOnBaseModulesEventHandler();
		}

		private void notificationServiceWrapper_IGenericNotify(object sender, IGenericNotifyEventArgs e)
		{
			IGenericNotify.RaiseOnDifferentThread(sender, e);
		}

		/// <summary>
		/// Mi registro a tutti gli eventi base di notifica dei moduli
		/// </summary>
		private void RegisterOnBaseModulesEventHandler()
		{
			foreach(var module in BaseNotificationModules)
				module.BaseModuleEventHandler += module_BaseModuleEventHandler;
		}

		/// <summary>
		/// giro gli eventi base di notifica dei moduli al notification button o viewer che si registra all'evento notify
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void module_BaseModuleEventHandler(object sender, EventArgs e)
		{
			Notify.Raise(sender, e);
			//MessageNotify.RaiseOnDifferentThread(this, new NotificationMessageEventArgs { Message= "notifica da brain" });
		}

		/// <summary>
		/// Metodo per recuperare singolarmente i moduli
		/// </summary>
		/// <returns></returns>
		public T GetModule<T>() where T : BaseNotificationModule
		{
			foreach(var module in BaseNotificationModules)
				if(module is T)
					return module as T;
			return null;
		}

		/// <summary>
		/// Metodo sollevato quando ricevo una notifica generica client to client
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void notificationServiceWrapper_MessageNotify(object sender, NotificationMessageEventArgs e)
		{
			MessageNotify.RaiseOnDifferentThread(this, e);
		}

		/// <summary>
		/// Evento ricevuto dal wrapper e sparato alla toolbar in caso di cambio  dello status della
		/// connessione con XSocket
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void notificationServiceWrapper_ConnectionStatus(object sender, ConnectionWithXSocketEventArgs e)
		{
			ConnectionWithXSocketNotify.Raise(this, e);
		}

		/// <summary>
		/// Consente di inviare un messaggio ad un worker qualsiasi di una company qualsiasi (che abbia instanziato un notificationManager)
		/// </summary>
		/// <param name="message"></param>
		/// <param name="ToWorkerId"></param>
		/// <param name="ToCompanyId"></param>
		public void SendMessage(string message, int ToWorkerId, int ToCompanyId)
		{
			notificationServiceWrapper.SendMessage(message, ToWorkerId, ToCompanyId);
		}

		/// <summary>
		/// Consente di inviare una notifica generica al ws che la rigira al giusto worker
		/// </summary>
		/// <param name="notify"> la notifica generica da inviare
		/// il campo FromUsername non è obbligatorio in quanto di default viene settato all'utente corrente</param>
		public void SendIGenericNotify(GenericNotify notify, bool storeOnDb)
		{
			if(string.IsNullOrWhiteSpace(notify.FromUserName))
				notify.FromUserName = NotificationManagerUtility.BuildBrainBusinessUserName(CompanyId, WorkerId);
			try
			{
				notificationServiceWrapper.SendIGenericNotify(notify, storeOnDb);
			}
			catch(Exception e )
			{
				notificationServiceWrapper_IGenericNotify(this, new IGenericNotifyEventArgs
				{
					GenericNotify = new GenericNotify
						{
							ToCompanyId = CompanyId,
							ToWorkerId = WorkerId,
							Title = NotificationManagerStrings.ErrorSendingGenericNotification,
							Description = NotificationManagerStrings.ErrorSendingGenericNotifyDesc + " , " + NotificationManagerStrings.Error + ": \n" + e.Message,
							Date = DateTime.Now
						}
				});
			}
		}

		/// <summary>
		/// Mi serve per verificare la connesione con XSocket
		/// </summary>
		/// <returns></returns>
		public bool IsConnectedWithXSocket()
		{
            //return notificationServiceWrapper.IsConnectedWithXSocket();
            return false;
		}

		/// <summary>
		/// mi serve per verificare la connessione con il Notification Service
		/// </summary>
		/// <returns></returns>
		public bool IsConnectedWithNotificationService()
		{
			return notificationServiceWrapper.IsAlive();
		}

		/// <summary>
		/// la registrazione viene effettuata in maniera asincrona
		/// </summary>
		public void SubscribeToTheNotificationServiceAsync()
		{
			A.Sailor()
			.When(() =>
			{
				ConnectionWithXSocketNotify.Raise(null, new ConnectionWithXSocketEventArgs 
				{ 
					status=ConnectionStatus.IsConnecting 
				});
				notificationServiceWrapper.SmartSubScribe();
			})
			.OnError((ex) =>
			{
				ConnectionWithXSocketNotify.Raise(null, new ConnectionWithXSocketEventArgs
				{
					status = ConnectionStatus.Disconnected
				});
			});
		}

		/// <summary>
		/// la registrazione viene effettuata in maniera sincrona (per i test di connessione nel BB Connector Settings)
		/// </summary>
		public void SubscribeToTheNotificationServiceSync()
		{
			notificationServiceWrapper.SubScribe();
		}

		/// <summary>
		/// Metodo per richiedere tutte le notifiche a tutti i provider
		/// </summary>
		/// <returns></returns>
		public IList<IGenericNotify> GetAllNotifications()
		{
			var allNotifications = new List<IGenericNotify>();

			try
			{
				foreach(var module in BaseNotificationModules)
					allNotifications.AddRange(module.GetNotifications());
				//allNotifications.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
			}
			catch(Exception e )
			{
				NotificationManagerUtility.SetErrorMessage(NotificationManagerStrings.ErrorLoadingNotifications, e.StackTrace, NotificationManagerStrings.Error);
			}
			return allNotifications;
		}

		/// <summary>
		/// Metodo da chiamare per chiudere la connessione con XSocket
		/// </summary>
		public void UnSubScribe()
		{
			//notificationServiceWrapper.UnSubScribe();
		}

		/// <summary>
		/// test
		/// </summary>
		/// <param name="message"></param>
		/// <param name="username"></param>
		public Father TestInheritance(Father father)
		{
			return notificationServiceWrapper.TestInheritance(father);
		}
	}
}

