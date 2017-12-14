using System;
using System.Linq;
using Microarea.TaskBuilderNet.Core.NotificationService;
using System.ServiceModel;
using Microarea.TaskBuilderNet.Core.NameSolver;
using System.Threading;
using System.Threading.Tasks;
using Microarea.TaskBuilderNet.Core.NotificationManager;
using Microarea.TaskBuilderNet.Interfaces;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using System.IO;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	public enum ConnectionStatus { Disconnected=0, Connected=1, IsConnecting=2 };
	#region Events
	/// <summary>
	/// è il "parametro" della notifica che passo al notificationManager quando 
	/// ricevo una form dal Notification Service
	/// </summary>
	public class FormNotifyEventArgs: System.EventArgs
	{
		public int FormInstanceId { get; set; }
	}

	/// <summary>
	/// è il "parametro" della notifica che passo al notificationManager quando 
	/// ricevo una mileStone dal Notification Service
	/// </summary>
	public class MileStoneNotifyEventArgs: System.EventArgs
	{
		public string Title { get; set; }
	}

	/// <summary>
	/// lo status della connessione con XSocket
	/// </summary>
	public class ConnectionWithXSocketEventArgs: System.EventArgs
	{
		public ConnectionStatus status { get; set; }
	}

	/// <summary>
	/// messaggio di una notifica generica
	/// </summary>
	public class NotificationMessageEventArgs:System.EventArgs
	{
		public string Message { get; set; }
	}

	/// <summary>
	/// Event Args per le notifiche interne generiche
	/// </summary>
	public class IGenericNotifyEventArgs:System.EventArgs
	{
		public GenericNotify GenericNotify { get; set; }
	}

	/// <summary>
	/// Event Args per le i messaggi della chat
	/// </summary>
	public class ChatNotifyEventArgs: System.EventArgs
	{
		public GenericNotify GenericNotify { get; set; }
	}

	/// <summary>
	/// Generica notifica di messaggio client to client
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void MessageNotifyEventHandler(object sender, NotificationMessageEventArgs e);

	/// <summary>
	/// delegate per sparare lo status della connessione con XSocket, l'ho spostato qui dal manager per poter 
	/// sparare anche lo status IsConnecting durante la fase di SubScribe 
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void ConnectionWithXSocketEventHandler(object sender, ConnectionWithXSocketEventArgs e);
	
	/// <summary>
	/// delegate per le notifiche delle form
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e">FormNotifyEventArgs contiene il formInstanceId</param>
	public delegate void FormNotifyEventHandler(object sender, FormNotifyEventArgs e);

	/// <summary>
	/// delegate per le notifiche delle milestone
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"> mileStoneNotifyEventArgs contiene il titolo della milestone</param>
	public delegate void MileStoneNotifyEventHandler(object sender, MileStoneNotifyEventArgs e);

	/// <summary>
	/// delegate per le notifiche interne che implementano l'interfaccia IGenericNotify
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void IGenericNotifyEventHandler(object sender, IGenericNotifyEventArgs e);

	/// <summary>
	/// delegate per le notifiche interne che implementano l'interfaccia IGenericNotify
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void ChatNotifyEventHandler(object sender, ChatNotifyEventArgs e);

	/// <summary>
	/// Classe che incapsula exthension methods per la gestione degli eventi e per spararli su thread differenti
	/// </summary>
	public static class NotificationWrapperEventExthensionMethods
	{
		/// <summary>
		/// oggetto statico che uso per lanciare gli eventi in maniera thread Safe
		/// </summary>
		static object SpinLock = new object();

		/// <summary>
		/// Per EventHandler: Controlla se c'è qualcuno registrato a ricevere gli eventi, se sì lo spara
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void Raise(this EventHandler handler, object sender, EventArgs e)
		{
			EventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				eh(sender, e);
		}
		/// <summary>
		/// Per EventHandler: Controlla se c'è qualcuno registrato a ricevere gli eventi, se sì lo spara
		/// su un nuovo thread
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void RaiseOnDifferentThread(this EventHandler handler, object sender, EventArgs e)
		{
			EventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				Task.Factory.StartNewOnDifferentThread(() => eh.Raise(sender, e));
		}

		/// <summary>
		/// exthension per i task
		/// The cancellationToken is necessary to guarantee StartNew() actually uses a different thread,
		/// </summary>
		/// <param name="taskFactory"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static Task StartNewOnDifferentThread(this TaskFactory taskFactory, Action action)
		{
			return taskFactory.StartNew(action : action, cancellationToken : new CancellationToken());
		}

		/// <summary>
		/// Per FormNotifyEventHandler: Controlla se c'è qualcuno registrato a ricevere gli eventi, se sì lo spara
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void Raise(this FormNotifyEventHandler handler, object sender, FormNotifyEventArgs e)
		{
			FormNotifyEventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				eh(sender, e);
		}

		/// <summary>
		/// Per FormNotifyEventHandler: Controlla se c'è qualcuno registrato a ricevere gli eventi, se sì lo spara
		/// su un nuovo thread
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void RaiseOnDifferentThread(this FormNotifyEventHandler handler, object sender, FormNotifyEventArgs e)
		{
			FormNotifyEventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				Task.Factory.StartNewOnDifferentThread(() => eh.Raise(sender, e));
		}

		/// <summary>
		/// Per MileStoneNotifyEventHandler: Controlla se c'è qualcuno registrato a ricevere gli eventi, se sì lo spara
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void Raise(this MileStoneNotifyEventHandler handler, object sender, MileStoneNotifyEventArgs e)
		{
			MileStoneNotifyEventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				eh(sender, e);
		}
		/// <summary>
		/// Per MileStoneNotifyEventHandler: Controlla se c'è qualcuno registrato a ricevere gli eventi, se sì lo spara
		/// su un nuovo thread
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void RaiseOnDifferentThread(this MileStoneNotifyEventHandler handler, object sender, MileStoneNotifyEventArgs e)
		{
			MileStoneNotifyEventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				Task.Factory.StartNewOnDifferentThread(() => eh.Raise(sender, e));
		}

		/// <summary>
		/// Per ConnectionWithXSocketEventHandler: Controlla se sono connesso ad XSocket
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void Raise(this ConnectionWithXSocketEventHandler handler, object sender, ConnectionWithXSocketEventArgs e)
		{
			ConnectionWithXSocketEventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				eh(sender, e);
		}
		/// <summary>
		/// Per ConnectionWithXSocketEventHandler: Controlla se sono connesso ad XSocket
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void RaiseOnDifferentThread(this ConnectionWithXSocketEventHandler handler, object sender, ConnectionWithXSocketEventArgs e)
		{
			ConnectionWithXSocketEventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				Task.Factory.StartNewOnDifferentThread(() => eh.Raise(sender, e));
		}

		/// <summary>
		/// Per NotificationMessageNotifyEventHandler: notifica di messaggio da client to client
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void Raise(this MessageNotifyEventHandler handler, object sender, NotificationMessageEventArgs e)
		{
			MessageNotifyEventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				eh(sender, e);
		}
		/// <summary>
		/// Per NotificationMessageNotifyEventHandler: nnotifica di messaggio da client to client
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void RaiseOnDifferentThread(this MessageNotifyEventHandler handler, object sender, NotificationMessageEventArgs e)
		{
			MessageNotifyEventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				Task.Factory.StartNewOnDifferentThread(() => eh.Raise(sender, e));
		}

		/// <summary>
		/// Per IGenericNotifyEventHandler: notifica di messaggio da client to client
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void Raise(this IGenericNotifyEventHandler handler, object sender, IGenericNotifyEventArgs e)
		{
			IGenericNotifyEventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				eh(sender, e);
		}
		/// <summary>
		/// Per IGenericNotifyEventHandler: nnotifica di messaggio da client to client
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void RaiseOnDifferentThread(this IGenericNotifyEventHandler handler, object sender, IGenericNotifyEventArgs e)
		{
			IGenericNotifyEventHandler eh;
			
			lock(SpinLock) { eh = handler; }
			
			if(eh != null)
				Task.Factory.StartNewOnDifferentThread(() => eh.Raise(sender, e));
		}

		/// <summary>
		/// Per ChatNotifyEventHandler: notifica di messaggio chat da client to client
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void Raise(this ChatNotifyEventHandler handler, object sender, ChatNotifyEventArgs e)
		{
			ChatNotifyEventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				eh(sender, e);
		}
		/// <summary>
		/// Per ChatNotifyEventHandler: notifica di messaggio chat da client to client
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void RaiseOnDifferentThread(this ChatNotifyEventHandler handler, object sender, ChatNotifyEventArgs e)
		{
			ChatNotifyEventHandler eh;

			lock(SpinLock) { eh = handler; }

			if(eh != null)
				Task.Factory.StartNewOnDifferentThread(() => eh.Raise(sender, e));
		}
	}

	#endregion

	/// <summary>
	/// La classe che fa da ponte tra il NotificationManager e il NotificationService
	/// Istanzia un XSocketClient, con cui riceve le notifiche dal NotificationService
	/// e poi spara gli eventi al NotificationManager
	/// </summary>
	public class NotificationServiceWrapper
	{
		internal int CompanyId { get; set; }
		
		internal int WorkerId { get; set; }

		private NotificationServiceClient notificationService;

		/// <summary>
		/// Riferimento al client di Xsocket, utile se vogliamo farci la close
		/// </summary>
		//private XSocketClient xSocketClient;
		/// <summary>
		/// mi serve per sapere se voglio chiudere la connessione o se è stata chiusa per problemi di connessione
		/// </summary>
		//private bool IWannaCloseXSocket;
		/// <summary>
		/// Notifica riguardante lo stato della connessione al motore XSocket
		/// </summary>
		public event ConnectionWithXSocketEventHandler ConnectionNotify;
		/// <summary>
		/// Notifica generica client to client
		/// </summary>
		//public event MessageNotifyEventHandler MessageNotify;
		/// <summary>
		/// Evento di ricezione notifica generica
		/// </summary>
		//public event ChatNotifyEventHandler ChatNotify;
		/// <summary>
		/// Evento di ricezione notifica generica
		/// </summary>
		//public event IGenericNotifyEventHandler IGenericNotify;
		/// <summary>
		/// Notifica sparata quando ricevo una milestone
		/// </summary>
		//public event MileStoneNotifyEventHandler BBMileStoneNotify;
		/// <summary>
		/// Notifica sparata quando ricevo una form
		/// </summary>
		//public event FormNotifyEventHandler BBFormNotify;

		//Concorrenza tra thread nella SmartSubscribe... complessità forse esagerata?
		private Object _lock = new Object();
		private bool isRefreshing = false;
		TimerState timerState= new TimerState { counter = 0 };

		////Parte di diagnostica
		//string fileName = string.Format("{0}-{1}.xml", , DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"));
		//string filePath = Path.Combine(path, fileName);

		/// <summary>
		/// Istanzia il NotificationService
		/// </summary>
		public NotificationServiceWrapper(int workerId, int companyId)
		{
			WorkerId = workerId;
			CompanyId = companyId;

			notificationService = NotificationServiceUtility.GetNotificationServiceClient(BasePathFinder.BasePathFinderInstance.NotificationServiceUrl);
		}

		/// <summary>
		/// Istanzia il NotificationService
		/// </summary>
		/// <param name="url"></param>
		public NotificationServiceWrapper(string url, int workerId, int companyId)
		{
			WorkerId = workerId;
			CompanyId = companyId;

			notificationService = NotificationServiceUtility.GetNotificationServiceClient(url);
		}

		/// <summary>
		/// invia una notifica semplice contenente un semplice messaggio al worker indicato nei parametri
		/// </summary>
		/// <param name="message"></param>
		/// <param name="username"></param>
		public void SendMessage(string message, int ToWorkerId, int ToCompanyId)
		{ 
			notificationService.SendMessage(message, ToWorkerId, ToCompanyId);
		}

		/// <summary>
		/// invia una notifica generica ad un altro utente
		/// </summary>
		/// <param name="message"></param>
		/// <param name="username"></param>
		public void SendIGenericNotify(GenericNotify notify, bool storeOnDb)
		{
			try
			{
				notificationService.SendIGenericNotify(notify, storeOnDb);
			}
			catch(Exception e)
			{
				throw new Exception("Errore durante l'invio della notifica al notification Service", e);
			}
		}

		public List<Microarea.TaskBuilderNet.Interfaces.GenericNotify> GetAllIGenericNotify(int workerId, int companyId, bool includeProcessed)
		{
			try
			{
				return notificationService.GetAllIGenericNotify(workerId, companyId, includeProcessed) != null ?
						notificationService.GetAllIGenericNotify(workerId, companyId, includeProcessed).ToList<GenericNotify>() :
						new List<GenericNotify>();
			}
			catch(Exception e )
			{
				NotificationManagerUtility.SetErrorMessage("errore nella richiesta delle notifiche generiche al NotificationService", e.StackTrace, "Errore nel caricamento delle notifiche generiche");
				return new List<GenericNotify>();
			}
		}

		public bool SetNotificationAsRead(int NotificationId)
		{
			return notificationService.SetNotificationAsRead(NotificationId);
		}

		/// <summary>
		/// test
		/// </summary>
		/// <param name="message"></param>
		/// <param name="username"></param>
		public Father TestInheritance(Father father)
		{
			return notificationService.TestInheritance(father);
		}

		/// <summary>
		/// classico metodo per testare la connessione con il WS
		/// verificare se conviene tenere traccia dell'errore di connessione
		/// </summary>
		/// <returns></returns>
		public bool IsAlive()
		{
			//todoandrea: verificare il controllo seguente
			return notificationService.State == System.ServiceModel.CommunicationState.Opened && notificationService.IsAlive();
		}

		/// <summary>
		/// Metodo per richiedere una specifica form, identificata da un identificatore intero incrementale
		/// </summary>
		/// <param name="formInstanceId">identificatore della form (lato Brain)</param>
		/// <returns>una MyBBForm, ovvero una struttura contenente lo schema xml della forma e 
		/// l'identificatore della form (per rispondere alla form)</returns>
		public NotificationService.MyBBFormSchema GetForm(int formInstanceId)
		{
			return notificationService.GetBBForm(formInstanceId);
		}

		/// <summary>
		/// Metodo per inviare una form compilata a BrainBusiness
		/// </summary>
		/// <param name="compiledForm">la struttura contenente l'xml rappresentante la form (compilata)
		/// e l'identificatore univoco della form</param>
		/// <returns>true se la form è stata inviata correttamente a BrainBusiness</returns>
		public bool SetFormSchema(MyBBFormSchema myBBForm)
		{
			return notificationService.SetBBForm(myBBForm);
		}

		/// <summary>
		/// Richiede le informazioni base delle form relative ad un worker
		/// </summary>
		/// <param name="workerId"></param>
		/// <param name="companyId"></param>
		/// <param name="includeProcessed"></param>
		/// <returns>un array di MyBBFormInstances</returns>
		public MyBBFormInstanceBase[] GetAllFormInstances(bool includeProcessed)
		{
			var allFormInstances = notificationService.GetAllBBFormInstances(WorkerId, CompanyId, includeProcessed);
			return allFormInstances==null? 
				null 
				: 
				allFormInstances.Select
				(
					brainFormInstance => new MyBBFormInstanceBase
					{
						Title			=	brainFormInstance.Title,
						FormInstanceId	=	brainFormInstance.FormInstanceId, 
						Processed		=	brainFormInstance.Processed, 
						DateProcessed	=	brainFormInstance.DateProcessed,
						DateSubmitted	=	brainFormInstance.DateSubmitted,
						UserName		=	brainFormInstance.UserName
					}
				).ToArray();
		}

		/// <summary>
		/// Metodo utilizzato per registrarsi alle notifiche:
		/// viene istanziato un XSocketClient, sempre in ascolto di nuovi eventi lanciati dal NotificationService
		/// </summary>
		/// <param name="workerId"></param>
		/// <param name="companyId"></param>
		public void SubScribe()
		{
			//try
			//{
			//	//recupero l'indirizzo del controller a cui "registrarmi"
			//	var controllerUrl = notificationService.GetXSocketControllerUrl();

			//	xSocketClient = new XSockets.Client40.XSocketClient(controllerUrl, "*");
			//	xSocketClient.OnOpen  += client_OnOpen;
			//	xSocketClient.OnError += client_OnError;
			//	xSocketClient.OnClose += client_OnClose;

			//	//aggiunti per scoprire qualcosa relativamente alla misteriosa disconnessione con XSocket 
			//	// -> quindi sono da rimuovere quando è risolta
			//	xSocketClient.OnPing += xSocketClient_OnPing;
			//	xSocketClient.OnPong += xSocketClient_OnPong;

			//	//mi registro alle notifiche di Brain
			//	xSocketClient.Bind("FormNotify",		RaiseBBFormNotify);

			//	xSocketClient.Bind("MileStoneNotify",	RaiseBBMileStoneNotify);

			//	xSocketClient.Bind("Message",			RaiseMessageNotify);

			//	xSocketClient.Bind("IGenericNotify",	RaiseIGenericNotify);

			//	xSocketClient.Bind("ChatNotify",		RaiseChatNotify);

			//	xSocketClient.Open();

			//	IWannaCloseXSocket = false;
			//	//WaitForConnection(xSocketClient); -> serviva per aspettare di essere connesso prima di settare il mio nome identificativo ->spostato il set nella OnOpen
			//}
			////catchare l'eccezione con il notification service?
			//catch(Exception e)
			//{
			//	throw new CommunicationException("Errore di connessione con XSocket", e);
			//}
		}

		//public void RaiseBBFormNotify(XSockets.Client40.Common.Event.Interface.ITextArgs textArgs)
		//{
		//	try
		//	{
		//		var data = textArgs.data;
		//		BBFormNotify.RaiseOnDifferentThread
		//		(
		//			this,
		//			new FormNotifyEventArgs { FormInstanceId = Int32.Parse(data) }
		//		);
		//	}
		//	catch(Exception e)
		//	{
		//		NotificationManagerUtility.SetErrorMessage("errore con il lancio dell'evento FormNotify", e.StackTrace, "errore");
		//	}
		//}

		//public void RaiseMessageNotify(XSockets.Client40.Common.Event.Interface.ITextArgs textArgs)
		//{
		//	try
		//	{
		//		var data = textArgs.data;
		//		MessageNotify.RaiseOnDifferentThread
		//		(
		//			this,
		//			new NotificationMessageEventArgs { Message = data }
		//		);
		//	}
		//	catch(Exception e)
		//	{
		//		NotificationManagerUtility.SetErrorMessage("errore con il lancio dell'evento MessageNotify", e.StackTrace, "errore");
		//	}
		//}

		//public void RaiseBBMileStoneNotify(XSockets.Client40.Common.Event.Interface.ITextArgs textArgs)
		//{
		//	try
		//	{
		//		var data = textArgs.data;
		//		BBMileStoneNotify.RaiseOnDifferentThread
		//		(
		//			this,
		//			new MileStoneNotifyEventArgs { Title = data }
		//		);
		//	}
		//	catch(Exception e)
		//	{
		//		NotificationManagerUtility.SetErrorMessage("errore con il lancio dell'evento BBMileStoneNotify", e.StackTrace, "errore");
		//	}
		//}

		//public void RaiseIGenericNotify(XSockets.Client40.Common.Event.Interface.ITextArgs textArgs)
		//{
		//	try
		//	{
		//		var data = textArgs.data;
		//		var notify = data.Deserialize<GenericNotify>();
		//		IGenericNotify.RaiseOnDifferentThread
		//			(
		//				this,
		//				new IGenericNotifyEventArgs { GenericNotify = notify }
		//			);
		//	}
		//	catch(Exception e) 
		//	{
		//		NotificationManagerUtility.SetErrorMessage("errore con il lancio dell'evento IGenericNotify", e.StackTrace, "errore");
		//	}
		//}

		//public void RaiseChatNotify(XSockets.Client40.Common.Event.Interface.ITextArgs textArgs)
		//{
		//	try
		//	{
		//		var data = textArgs.data;
		//		var notify = data.Deserialize<GenericNotify>();
		//		ChatNotify.RaiseOnDifferentThread
		//			(
		//				this,
		//				new ChatNotifyEventArgs { GenericNotify = notify }
		//			);
		//	}
		//	catch(Exception e)
		//	{
		//		NotificationManagerUtility.SetErrorMessage("errore con il lancio dell'evento IGenericNotify", e.StackTrace, "errore");
		//	}
		//}

		///// <summary>
		///// metodo implementato per test
		///// </summary>
		///// <param name="sender"></param>
		///// <param name="e"></param>
		//void xSocketClient_OnPong(object sender, XSockets.Client40.Common.Event.Arguments.BinaryArgs e)
		//{
		//	MessageNotify.RaiseOnDifferentThread(this, new NotificationMessageEventArgs { Message = "Pong" });
		//}

		///// <summary>
		///// metodo implementato per test
		///// </summary>
		///// <param name="sender"></param>
		///// <param name="e"></param>
		//void xSocketClient_OnPing(object sender, XSockets.Client40.Common.Event.Arguments.BinaryArgs e)
		//{
		//	MessageNotify.RaiseOnDifferentThread(this, new NotificationMessageEventArgs { Message = "Ping" });
		//}

		///// <summary>
		///// Metodo per aspettare che terminino le operazioni per creare il canale di comunicazione con il controller sul NotificationService
		///// (Handshake)
		///// </summary>
		///// <param name="client"></param>
		///// <param name="timeout"></param>
		///// <returns></returns>
		//private static bool WaitForConnection(XSocketClient client, int timeout = -1)
		//{
		//	return SpinWait.SpinUntil(() => client.IsConnected, timeout);
		//}

		/// <summary>
		/// Evento generato quando viene a mancare la connessione col webService delle notifiche
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void client_OnClose(object sender, EventArgs e)
		{
			ConnectionNotify.Raise(null, new ConnectionWithXSocketEventArgs { status=ConnectionStatus.Disconnected });

			//if(!IWannaCloseXSocket) 
			//{
			//	//MessageNotify.Raise(this, new NotificationMessageEventArgs { Message = "Sei stato disconnesso dal servizio di notifiche real time" });
			//	SmartSubScribe();
			//}
		}

		public void SmartSubScribe()
		{
			//-------------------------versione ignorante------------------------------------------------

			//// il tempo che separa i 3 tentativi di riconnessione
			//var timeOuts = new int[] { 5000, 25000, 50000 };
			//for(int i = 0; !IsConnectedWithXSocket() && i < 3; i++) {Thread.Sleep(timeOuts[i]); SubScribe();}


			//-------------------------versione "ottimizzata"--------------------------------------------
			//-------System.Threading.Timer (not thread safe)
			//-------usato Timer.Timer (thread safe)

			//parte per la concorrenza:
			//mi sto già registrando con questo metodo asincrono, non faccio partire altri thread
			//mettere il timerstate globale e valutare quello( e resettarlo) piuttosto che il semplice booleano
			bool haveToReturn;

			lock(_lock)
			{
				haveToReturn = isRefreshing;

				if(!isRefreshing)
					isRefreshing = true;
			}

			if(haveToReturn)
			{
				lock(_lock)
				{
					timerState.Reset();
				}
				return;
			}
			//il task da eseguire
			System.Threading.TimerCallback TimerDelegate = new System.Threading.TimerCallback(SubscriptionTask);
			//inizializzo una variabile che uso per tenere i parametri che servono al TimerTask
			//var timerState = new TimerState { counter = 0 };
			//faccio partire il Timer
			var timer = new Timer(TimerDelegate, timerState, TimerState.DueTime, TimerState.DueTime);
			//aggiungo il riferimento al timer allo stateObj in modo da poterne fare la dispose
			timerState.TimerReference = timer;
		}

		/// <summary>
		/// metodo richiamato dal timer ad ogni suo risveglio
		/// </summary>
		/// <param name="state"></param>
		private void SubscriptionTask(object state)
		{
			////recuperp l'oggetto contenente i parametri necessari
			//var State = (TimerState)state;

			//// Use the interlocked class to increment the counter variable.
			//System.Threading.Interlocked.Increment(ref State.counter);

			////incremento l'intervallo ad ogni iterazione fino ad un massimo di 30 min (= 1800000 ms)
			//int nextPeriod = Math.Min(State.period * 2, 1800000);
			//System.Threading.Interlocked.Exchange(ref State.period, nextPeriod);

			////setto il nuovo intervallo (in realtà verrà usato solo il primo parametro, ovvero il DueTime
			////come tempo di intermezzo tra questa e la prossima chiamata, e lo stesso la volta successiva)
			//State.TimerReference.Change(State.period, State.period);

			//// controllo se sono già connesso o ho fatto più di tre iterazioni
			//if(IsConnectedWithXSocket() || IWannaCloseXSocket) 
			//{
			//	//Rilascio le risorse del timer ed esco
			//	State.TimerReference.Dispose();
			//	return;
			//}
			//try
			//{
			//	//provo a registrarmi
			//	SubScribe();
			//}
			//catch(Exception)
			//{
			//	////non faccio niente e ci riprovo alla prossima iterazione, se è già il #>1 tentativo, notifico l'utente con un messaggio
			//	//if(State.counter > 1 && State.counter % 5 == 0)
			//	//{
			//	//	//MessageNotify.RaiseOnDifferentThread(this, new NotificationMessageEventArgs
			//	//	//{
			//	//	//	Message = "Tentativo di riconnessione al servizio real time delle notifiche numero " + State.counter +
			//	//	//				" fallito, \n prossimo tentativo tra " + State.GetReadablePeriodTime()
			//	//	//});
			//	//	notificationService = NotificationServiceUtility.GetNotificationServiceClient(BasePathFinder.BasePathFinderInstance.NotificationServiceUrl);
			//	////	//vedere se non serve tirarne altre
			//	////	//DiagnosticView diagnosticView = new DiagnosticView();
			//	////	//diagnosticView.WriteXmlFile(filePath, LogType.Application);
			//	////	//diagnosticView.Close();
			//	////	//NotificationManagerUtility.SetMessage("Errore in fase di subscription ad XSocket", e.StackTrace, "Impossibile connettersi al sistema di notifiche real time");
			//	//}
			//}
		}

		/// <summary>
		/// classe contenente i parametri necessari al metodo di callback del timer
		/// </summary>
		private class TimerState
		{
			/// <summary>
			/// il tempo che aspetto prima di fare partire il task
			/// </summary>
			static public readonly int DueTime = 5000;
			/// <summary>
			/// tempo che intercorre tra una esecuzione e un altra del task
			/// </summary>
			public int period = 5000;
			/// <summary>
			/// numero di volte che il task è stato eseguito
			/// </summary>
			public int counter;
			/// <summary>
			/// referenza al timer in modo da farne la dispose
			/// </summary>
			public System.Threading.Timer TimerReference;
			/// <summary>
			/// restituisce una rappresentazione comprensibile dei millisecondi, trovata online 
			/// -> si potrebbe eliminare la parte dei giorni e delle ore poichè al massimo aspetto 30 min
			/// </summary>
			/// <returns></returns>
			public string GetReadablePeriodTime() 
			{
				var ts = TimeSpan.FromMilliseconds(period);
				var parts = string
				.Format("{0:D2}d:{1:D2}h:{2:D2}m:{3:D2}s",
					ts.Days, ts.Hours, ts.Minutes, ts.Seconds)
				.Split(':')
				.SkipWhile(s => Regex.Match(s, @"00\w").Success) // skip zero-valued components
				.ToArray();
				var result = string.Join(" ", parts); // combine the result

				return result;
			}

			public void Reset() 
			{
				this.counter = 0;
				this.period = 5000;
			}
		}

		/// <summary>
		/// Evento generato quando viene generato un errore con il canale di comunicazione di Xsocket
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//void client_OnError(object sender, XSockets.Client40.Common.Event.Arguments.TextArgs e)
		//{
		//	MessageNotify.RaiseOnDifferentThread(this, new NotificationMessageEventArgs { Message = "Errore con il controller delle notifiche: " + e.data.ToString() });
		//}

		/// <summary>
		/// Evento generato quando viene aperta una connessione con XSocket
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//void client_OnOpen(object sender, EventArgs e)
		//{
		//	ConnectionNotify.Raise(null, new ConnectionWithXSocketEventArgs {status=ConnectionStatus.Connected });
			
		//	//parte concorrenza thread e registrazioni "smart"
		//	lock(_lock)
		//	{
		//		isRefreshing = false;
		//	}

		//	try
		//	{
		//		//Una volta aperta la connesione, imposto il mio username, 
		//		//che viene utilizzato come identificatore per ricevere soltanto le notifiche indirizzate a me
		//		xSocketClient.Send(new { value = NotificationManagerUtility.BuildBrainBusinessUserName(CompanyId, WorkerId) }, "set_BBUserName");
		//	}
		//	catch(Exception exc)
		//	{
		//		throw new CommunicationException("Errore di connessione con XSocket, fase di impostazione nome utente", exc);
		//		//vedere se conviene fare la unsubscribe (magari col flag wanna close xsocket false...)
		//	}
		//}

		/// <summary>
		/// mi serve nella inizializzazione della notification toolbar
		/// </summary>
		/// <returns></returns>
		//public bool IsConnectedWithXSocket()
		//{
		//	return xSocketClient == null ? false : xSocketClient.IsConnected;
		//}

		///// <summary>
		///// evento utilizzato per chiudere la connessione con Xsocket: 
		///// </summary>
		///// <param name="workerId"></param>
		///// <param name="companyId"></param>
		//public void UnSubScribe()
		//{
		//	if(xSocketClient != null && xSocketClient.IsConnected)
		//	{
		//		IWannaCloseXSocket = true;
		//		xSocketClient.Close();
		//	}
		//}
		
		/// <summary>
		/// Restituisce l'url a cui contattare il servizio di Brain Business o string.Empty in caso di string nulla O.o
		/// </summary>
		/// <returns></returns>
		public string GetBrainBusinessServiceUrl()
		{
			return notificationService.GetBrainBusinessServiceUrl();
		}

		/// <summary>
		/// aggiorna il valore corrispondente sul database
		/// </summary>
		/// <param name="url"></param>
		/// <returns>se l'aggiornamento è andato a buon fine</returns>
		public bool UpdateBrainBusinessServiceUrl(string url)
		{
			return notificationService.UpdateBrainBusinessServiceUrl(url);
		}

	}
}
