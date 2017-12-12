using System;
using System.Collections.Generic;
using System.Linq;
using NotificationService.BBServiceReference;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Core.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.ServiceModel;
using Microarea.TaskBuilderNet.Core.NotificationManager;
using Microarea.TaskBuilderNet.Interfaces;
using PAT.Workflow.Runtime.BBNotificationListener;
using System.Xml.Serialization;


namespace NotificationService
{
	internal class NotificationCore
	{
		private static EventLog eventLog = new EventLog("MA Server", ".", "NotificationService");

		//todo andrea: prendere da un file di configurazioni
		//private static string BrainBusinessServiceUrl = "http://localhost:8081/APMServiceclient";
		private static string BrainBusinessServiceUrl = string.Empty;

		private static NotificationSqlConnection NotificationSqlConnection = null;

		static TraceSwitch WebSvcTraceSwitch = new TraceSwitch("WebSvcTraceSwitch", "Web Service Trace");
		static TextWriterTraceListener TestTracer = new TextWriterTraceListener("C:\\traceout.txt");
		// Note that by default the local ASPNET account(IIS 5.x) or the 
		// local NETWORK SERVICE account(IIS 6.0) needs write access to
		// this directory so that the instance of the 
		// TextWriterTraceListener can write to the trace output file.

		/// <summary>
		/// tapullo per BrainBusiness: ogni progresso su un particolare processo, produce una notifica,
		/// che però contiene, form già processate e milestone già notificate. quindi il tapullo consiste
		/// nell'aggiornare i seguenti valori ad ogni notifica al client in modo da non replicare mai le notifiche
		/// todoandrea- questa distinzione andrebbe fata per i singoli client registrati???
		/// </summary>
		private int LastMileStoneReceived;
		private int LastFormReceived;

		/// <summary>
		/// Si occupa di far partire il motore di XSocket
		/// </summary>
		internal NotificationCore()
		{
			//XSocketConnector.Start();
			LastMileStoneReceived = 0;
			LastFormReceived = 0;

			NotificationSqlConnection = new NotificationService.NotificationSqlConnection();

			WriteMessageInEventLog(NSStrings.NoficationCoreSuccesfullyStarted, EventLogEntryType.SuccessAudit);
		}

		/// <summary>
		/// Restituisce l'url a cui contattare il servizio di Brain Business
		/// </summary>
		/// <returns></returns>
		public static string GetBrainBusinessServiceUrl()
		{
			//return BrainBusinessServiceUrl??string.Empty;
			if(string.IsNullOrEmpty(BrainBusinessServiceUrl))
			{
				try
				{
					BrainBusinessServiceUrl = NotificationSqlConnection.GetBrainBusinessServiceUrl();
				}
				catch(Exception)
				{
					//default url
					BrainBusinessServiceUrl = "http://localhost:8081/APMServiceclient";

					bool inserted = NotificationSqlConnection.UpdateOrInsertBrainBusinessServiceUrl(BrainBusinessServiceUrl);
					
					if(inserted)
						WriteMessageInEventLog(NSStrings.BBServiceUrlSuccesfullyInserted + BrainBusinessServiceUrl, EventLogEntryType.Information, true);
					else
						WriteMessageInEventLog(NSStrings.BBServiceUrlError + BrainBusinessServiceUrl, EventLogEntryType.Error, true);
				}
			}

			return BrainBusinessServiceUrl;
		}

		/// <summary>
		/// aggiorna l'indirizzo a cui viene contattato il servizio di brain business
		/// </summary>
		/// <param name="newServiceUrl"></param>
		/// <returns></returns>
		public static bool UpdateOrInsertBrainBusinessServiceUrl(string newServiceUrl) 
		{
			if(string.IsNullOrWhiteSpace(newServiceUrl))
				newServiceUrl = "http://localhost:8081/APMServiceclient";

			bool inserted = NotificationSqlConnection.UpdateOrInsertBrainBusinessServiceUrl(newServiceUrl);
			if(inserted)
				BrainBusinessServiceUrl = newServiceUrl;
			return inserted;
		}

		/// <summary>
		/// Restituisce l'url su cui gira XSocket, il "motore" delle notifiche. L'url restituito deve poi
		/// essere utilizzato per creare gli XSocketClient (lato client e server)
		/// </summary>
		/// <returns>l'url di riferimento per gli XSocketClient</returns>
		public string GetControllerUrl()
		{
			return ""/*XSocketConnector.ControllerUrl*/;
		}

		/// <summary>
		/// Invia al client, specificato dallo username, la lista di milestone che lo riguardano 
		/// (idealmente sarà una sola milestone per volta) 
		/// </summary>
		/// <param name="workflowID">l'identificatore del processo a cui appartiene la mileStone</param>
		/// <param name="username">l'identificatore del client nella forma "Company:#/Worker:#"</param>
		/// <param name="mileStonesInfoArray">la lista delle mileStone</param>
		private void SendMilestones(Guid workflowID, string username, MilestoneInfo[] mileStonesInfoArray)
		{
			foreach(var mile in mileStonesInfoArray) {
				Trace.WriteLine(string.Format(NSStrings.MilestoneNotification, mile.MilestoneID ,username));
				//XSocketConnector.SendBBMileStoneNotify(mile.Name, mile.Description);
			}
		}

		/// <summary>
		/// Invia al client, specificato dallo username, la lista di form ancora non processate 
		/// che lo riguardano (idealmente sarà una sola form per volta) 
		/// </summary>
		/// <param name="workflowID">l'identificatore del processo a cui appartiene la mileStone</param>
		/// <param name="username">l'identificatore del client nella forma "Company:#/Worker:#"</param>
		/// <param name="unprocessedFormsArray">la lista delle form</param>
		private void SendUnprocessedForms(Guid workflowID, string username, FormInstanceInfo[] unprocessedFormsArray)
		{
			foreach(var form in unprocessedFormsArray) {
				if(username == "") 
				{
					Trace.WriteLine(string.Format(NSStrings.NotificationFormForGroup, form.WorkFlowID));
					return;
				}
				Trace.WriteLine(string.Format(NSStrings.FormNotification, form.FormInstanceID, username));
				//XSocketConnector.SendBBFormNotify(form.UserName, form.FormInstanceID);
			}

		}

		/// <summary>
		/// Metodo utilizzato solo per loggare le notifiche dal webService:
		/// stampa i valori dei controlli presenti nella form
		/// </summary>
		/// <param name="baseAbstractControl"></param>
		/// <returns>il valore (in stringa) dello specifico controllo della form</returns>
		private static string PrintControlValue(BaseAbstractControl baseAbstractControl)
		{
			var value = new object();
			if(baseAbstractControl is Label)
				value = null;
			else if(baseAbstractControl is Text)
				value = ((Text)baseAbstractControl).Value;
			else if(baseAbstractControl is DropDown)
				value = ((DropDown)baseAbstractControl).Value; //todo: iterare gli elementi presenti nella lista
			else if(baseAbstractControl is DateEdit)
				value = ((DateEdit)baseAbstractControl).Value;
			return value == null ? string.Empty : value.ToString();
		}

		/// <summary>
		/// Metodo utilizzato solo per loggare le notifiche dal webService:
		/// stampa la label dei controlli presenti nella form
		/// </summary>
		/// <param name="baseAbstractControl">il baseabstract control (tipo di Pat), simile a System.Form.Control</param>
		/// <returns>la label del controllo</returns>
		private static string PrintControlLabel(BaseAbstractControl baseAbstractControl)
		{
			if(baseAbstractControl is Label)
				return ((Label)baseAbstractControl).LabelMember;
			else if(baseAbstractControl is Text)
				return ((Text)baseAbstractControl).Label;
			else if(baseAbstractControl is DropDown)
				return ((DropDown)baseAbstractControl).Label;
			else if(baseAbstractControl is DateEdit)
				return ((DateEdit)baseAbstractControl).Label;
			//should not be possible
			return string.Empty;
		}

		/// <summary>
		/// Metodo per loggare le notifiche ricevute dal web service di brain Business
		/// </summary>
		/// <param name="workflowID">l'id del processo, ricevuto dalla chiamata al metodo notify</param>
		/// <param name="username">lo username anch'esso ricevuto dal web service di brain</param>
		internal void LogNofitications(Guid workflowID, string username)
		{
			WriteMessageInEventLog(string.Format(NSStrings.NewNotifyLog, workflowID.ToString(), username), EventLogEntryType.Information, true);
			//--------------------------------------------------------------------
			//var serviceref = new BBServiceReference.WorkflowServiceClient();
			var serviceref = BBServiceUtility.GetBBServiceClient(GetBrainBusinessServiceUrl());
			BBServiceReference.WorkFlowInfo wfInfo = serviceref.GetWorkFlowInfo(workflowID);
			if(wfInfo == null)
			{
				Trace.WriteLine("wf info == null");
				return;
			}
			//--------------------------------------------------------------------
			Trace.WriteLine(DateTime.Now.ToString() + " -----------------------------start " +workflowID.ToString() + "----------------------------");
			PrintForms(serviceref, wfInfo);

			PrintMileStones(wfInfo);
			//--------------------------------------------------------------------
			Trace.WriteLine(DateTime.Now.ToString() + " -----------------------------end " + workflowID.ToString() + "------------------------------");
			TestTracer.Dispose();
		}

		/// <summary>
		/// Metodo importante: invia le form e le mileStone a chi di competenza
		/// </summary>
		/// <param name="workflowID">l'identificatore del processo</param>
		/// <param name="username"></param>
		internal void DispatchBBNotifications(Guid workflowID, string username)
		{
			//var serviceref = new BBServiceReference.WorkflowServiceClient();
			var serviceref = BBServiceUtility.GetBBServiceClient(GetBrainBusinessServiceUrl());
			//-----------------
			BBServiceReference.WorkFlowInfo wfInfo = serviceref.GetWorkFlowInfo(workflowID);
			if(wfInfo == null)
			{
				Trace.WriteLine("wf info == null");
				return;
			}
			//--------------------------------------------------------------------
			//notifico le form al destinatario
			var userToNotifyTheForms = new List<string>();

			foreach(var formInstance in wfInfo.FormInstances.Where(x => x.FormInstanceID > LastFormReceived))
				if(!userToNotifyTheForms.Contains<string>(formInstance.UserName))
					userToNotifyTheForms.Add(formInstance.UserName);
			foreach(var user in userToNotifyTheForms)
				SendUnprocessedForms(workflowID, user, wfInfo.FormInstances.Where(x => x.UserName==user && x.Processed==false).ToArray<FormInstanceInfo>());

			//aggiorno il valore attuale del campo LastFormReceived, con l'identificatore più alto tra l'attuale 
			//e quello delle ultime form ricevute
			LastFormReceived = new int[] { LastFormReceived }.Concat(wfInfo.FormInstances.Select(x => x.FormInstanceID)).Max();
			
			//--------------------------------------------------------------------
			//notifico le milestones al destinatario
			var userToNotifyTheMileStones = new List<string>();

			foreach(var mileStone in wfInfo.Milestones.Where(x => x.MilestoneID>LastMileStoneReceived))
				if(!userToNotifyTheMileStones.Contains<string>(mileStone.Name))
					userToNotifyTheMileStones.Add(mileStone.Name);
			foreach(var user in userToNotifyTheMileStones)
				SendMilestones(workflowID, user, wfInfo.Milestones.Where(x => x.Name == user).ToArray<MilestoneInfo>());

			//aggiorno il valore attuale del campo lastMileStoneReceived, con l'identificatore più alto tra l'attuale 
			//e quello delle ultime mileStone ricevute
			LastMileStoneReceived = new int[] { LastMileStoneReceived }.Concat(wfInfo.Milestones.Select(x => x.MilestoneID)).Max();

		}

		public void DispatchBBEvent(Event eventdata)
		{
			if(eventdata is UserEvent) 
			{
				var userEvent = eventdata as UserEvent;
				if(userEvent is UserWorkflowEvent)
				{
					var userWorkflowEvent = userEvent as UserWorkflowEvent;
					DispatchBBNotifications(userWorkflowEvent.WorkflowID, userWorkflowEvent.UserName);
#if DEBUG
					LogNofitications(userWorkflowEvent.WorkflowID, userWorkflowEvent.UserName);
#endif
					return;
				}
				else if(userEvent is NewWorkflowAssignedEvent)
				{
					var newWorkflowAssignedEvent = userEvent as NewWorkflowAssignedEvent;

					return;
				}
			}

			return;
		}

		/// <summary>
		/// Metodo per girare a XSocket la notifica contenente un messaggio e il destinatario
		/// </summary>
		/// <param name="message"></param>
		/// <param name="username"></param>
		public static void DispatchMessage(string message, int workerId, int companyId)
		{
			//XSocketConnector.SendMessage(BuildBrainBusinessUserName(companyId, workerId), message);
		}

		/// <summary>
		/// Stampa le mileStone per il log
		/// </summary>
		/// <param name="wfInfo">le info riguardanti il processo in questione</param>
		private static void PrintMileStones(BBServiceReference.WorkFlowInfo wfInfo)
		{
			Trace.WriteLine(NSStrings.MilestonesList);
			foreach(var mileStoneInfo in wfInfo.Milestones)
			{
				Trace.WriteLine("   id: " + mileStoneInfo.MilestoneID + ", description= " + mileStoneInfo.Description + ", name= " + mileStoneInfo.Name);
			}
			Trace.WriteLine("/" + NSStrings.MilestonesList);
		}

		/// <summary>
		/// Stampa il contenuto delle form per il log
		/// </summary>
		/// <param name="serviceref">il reference al web service di brain</param>
		/// <param name="wfInfo">le info riguardanti il processo in questione</param>
		private static void PrintForms(WorkflowServiceClient serviceref, BBServiceReference.WorkFlowInfo wfInfo)
		{
			Trace.WriteLine(NSStrings.FormsDescriptionList);

			foreach(var formInstanceInfo in wfInfo.FormInstances)
			{
				var formId = formInstanceInfo.FormInstanceID;
				Trace.WriteLine("   id: " + formId + " processed= " + formInstanceInfo.Processed.ToString());
				var root = serviceref.GetFormSchema(formId).Root;
				Trace.WriteLine("   - root ");
				Trace.WriteLine("       - items ");
				foreach(var item in root.Items)
				{
					item.ShowText = true;
					var baseAbstractControl = ((LayoutItemField)item).Field;
					Trace.WriteLine("           " + item.FieldName.ToString() +
                                    "\n           - field: " + ((LayoutItemField)item).Field +
                                    "\n           - label: " + PrintControlLabel(baseAbstractControl) +
                                    "\n           - value: " + PrintControlValue(baseAbstractControl));
				}
				Trace.WriteLine("       - layoutType: " + root.LayoutType.ToString());
			}
			Trace.WriteLine("/" + NSStrings.FormsDescriptionList);
		}

		/// <summary>
		/// Metodo per richiedere tutte le form per un particolare utente, considerare di fare un metodo più efficiente per
		/// recuperare solo le form ancora da processare o solamente quelle già processate
		/// </summary>
		/// <param name="workerId"></param>
		/// <param name="companyId"></param>
		/// <returns></returns>
		internal static MyBBFormInstance[] GetAllFormInstances(int workerId, int companyId, bool includeProcessed)
		{
			string userName = BuildBrainBusinessUserName(companyId, workerId);
			
			try
			{
				var serviceRef = BBServiceUtility.GetBBServiceClient(GetBrainBusinessServiceUrl());
				//-----------------
				var brainFormInstancesInfo = serviceRef.GetFormInstances(userName, includeProcessed);
				return brainFormInstancesInfo.Select(brainFormInstance => BrainFormInstanceToMyBBFormInstance(brainFormInstance)).ToArray();
			}
			catch(EndpointNotFoundException e)
			{
				WriteMessageInEventLog(string.Format(NSStrings.ErrorRetrievingForm, 1) + "\n" +
					e.Message + "\n" + NSStrings.CheckBBServices,
					EventLogEntryType.Error);
			}
			catch(ServerTooBusyException e)
			{
				WriteMessageInEventLog(string.Format(NSStrings.ErrorRetrievingForm, 2) + "\n" + 
					e.Message + "\n" + NSStrings.CheckBBServices,
					EventLogEntryType.Error);
			}
			catch(NullReferenceException e)
			{
				WriteMessageInEventLog(string.Format(NSStrings.ErrorRetrievingForm, 3) + "\n" + 
					e.Message, EventLogEntryType.Error);
			}
			catch(Exception e)
			{
				WriteMessageInEventLog(string.Format(NSStrings.ErrorRetrievingForm, 4) + "\n" + 
					e.Message, EventLogEntryType.Error);
			}
			return null;
		}

		//il codice nella seguente regione è stato commentato caso mai dovesse servire, ma va inteso come da eliminare
		#region GetAllFormInstancesNumber
		///// <summary>
		///// Restituisce il numero delle form non ancora processate di un utente
		///// </summary>
		///// <param name="userName"> nella forma compresa da BrainBusiness Company:#/Worker:# </param>
		///// <returns></returns>
		//internal static int GetAllFormInstancesNumber(string userName)
		//{
		//	//le due righe sottostanti fanno restituiscono il numero delle form non processate, ma in maniera meno efficiente

		//	//var formInstancesArray = GetAllFormInstances(workerId, companyId, false);
		//	//return formInstancesArray==null? 0: formInstancesArray.Count();
			
		//	var serviceRef = new BBServiceReference.WorkflowServiceClient();
		//	try
		//	{
		//		var brainFormInstancesInfo = serviceRef.GetFormInstances(userName, false);
		//		return brainFormInstancesInfo==null? 0: brainFormInstancesInfo.Count();
		//	}
		//	catch(EndpointNotFoundException e)
		//	{
		//		WriteMessageInEventLog("Errore 1 in fase di recupero delle form non processate da BrainBusiness:\n" + 
		//			e.Message + "\n\nControllare che Brain Business sia installato e tutti i servizi siano avviati",
		//			EventLogEntryType.Error);
		//	}
		//	catch(ServerTooBusyException e)
		//	{
		//		WriteMessageInEventLog("Errore 2 in fase di recupero delle form non processate, da BrainBusiness: " + 
		//			e.Message + "\n\nControllare che Brain Business sia installato e tutti i servizi siano avviati",
		//			EventLogEntryType.Error);
		//	}
		//	catch(NullReferenceException e)
		//	{
		//		WriteMessageInEventLog("Errore 3 in fase di recupero delle form non processate: " + 
		//			e.Message, EventLogEntryType.Error);
		//	}
		//	catch(Exception e)
		//	{
		//		WriteMessageInEventLog("Errore generico in fase di recupero delle form non processate: " + 
		//			e.Message, EventLogEntryType.Error);
		//	}
		//	return 0;
		//}

		///// <summary>
		///// Restituisce il numero delle form non ancora processate di un utente
		///// </summary>
		///// <param name="workerId"></param>
		///// <param name="companyId"></param>
		///// <returns>il numero delle form (rivolte a lui) non ancora processate </returns>
		//internal static int GetAllFormInstancesNumber(int workerId, int companyId)
		//{
		//	string userName = BuildBrainBusinessUserName(companyId, workerId);
		//	return GetAllFormInstancesNumber(userName);
		//}
		#endregion

		/// <summary>
		/// Converte la struttura delle formInstance di brain nella nostra
		/// </summary>
		/// <param name="brainFormInstance"></param>
		/// <returns></returns>
		private static MyBBFormInstance BrainFormInstanceToMyBBFormInstance(FormInstanceInfo brainFormInstance)
		{
			return new MyBBFormInstance()
			{
				DateProcessed = brainFormInstance.DateProcessed,
				DateSubmitted = brainFormInstance.DateSubmitted,
				FormInstanceId = brainFormInstance.FormInstanceID,
				IsNotificationOnly = brainFormInstance.NotificationOnly,
				Processed = brainFormInstance.Processed,
				Title = brainFormInstance.Title,
				WorkFlowId = brainFormInstance.WorkFlowID,
				UserName = brainFormInstance.UserName
			};
		}

		/// <summary>
		/// Metodo per richiedere una specifica form, identificata da un identificatore intero incrementale
		/// </summary>
		/// <param name="formInstanceId">identificatore della form (lato Brain)</param>
		/// <returns>una MyBBForm, ovvero una struttura contenente lo schema xml della forma e 
		/// l'identificatore della form (per rispondere alla form)</returns>
		internal static MyBBFormSchema GetForm(int formInstanceId)
		{
			try
			{
				var serviceRef = BBServiceUtility.GetBBServiceClient(GetBrainBusinessServiceUrl());
				//-----------------
				var schema = serviceRef.GetFormSchema(formInstanceId);
				//-----------------
				var myBBformInstance = BrainFormInstanceToMyBBFormInstance(serviceRef.GetFormInstance(formInstanceId));
				//-----------------
				var xmlSchema = DataContractSerializeObject<SchemaMetadata>(schema);
				//-----------------
				return new MyBBFormSchema { xmlSchema=xmlSchema, myBBFormInstance=myBBformInstance};
			}
			catch(ServerTooBusyException e)
			{
				WriteMessageInEventLog(string.Format(NSStrings.ErrorRetrievingForm, 1) + e.Message, EventLogEntryType.Error);
			}
			catch(NullReferenceException e)
			{
				WriteMessageInEventLog(string.Format(NSStrings.ErrorRetrievingForm, 2) + e.Message, EventLogEntryType.Error);
			}
			catch(Exception e)
			{
				WriteMessageInEventLog(string.Format(NSStrings.ErrorRetrievingForm, 3) + e.Message, EventLogEntryType.Error);
			}
			return null;
		}

		/// <summary>
		/// Metodo per inviare una form compilata a BrainBusiness
		/// </summary>
		/// <param name="compiledForm">la struttura contenente l'xml rappresentante la form (compilata)
		/// e l'identificatore univoco della form</param>
		/// <returns>true se la form è stata inviata correttamente a BrainBusiness</returns>
		internal static bool SetForm(MyBBFormSchema compiledForm)
		{
			try
			{
				var serviceRef = BBServiceUtility.GetBBServiceClient(GetBrainBusinessServiceUrl());
				//-----------------
				var compiledSchema = Deserialize<SchemaMetadata>(compiledForm.xmlSchema);
				return serviceRef.SetFormSchema(compiledSchema, compiledForm.myBBFormInstance.FormInstanceId);
			}
			catch(ServerTooBusyException e)
			{
				WriteMessageInEventLog(string.Format(NSStrings.ErrorSendingForm, 1) + e.Message, EventLogEntryType.Error);
			}
			catch(NullReferenceException e)
			{
				WriteMessageInEventLog(string.Format(NSStrings.ErrorSendingForm, 2) + e.Message, EventLogEntryType.Error);
			}
			catch(Exception e)
			{
				WriteMessageInEventLog(string.Format(NSStrings.ErrorSendingForm, 3) + e.Message, EventLogEntryType.Error);
			}
			return false;
		}

		/// <summary>
		/// Metodo per convertire lo SchemaMetaData della form in un xml comprensibile lato client
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objectToSerialize"></param>
		/// <returns></returns>
		private static string DataContractSerializeObject<T>(T objectToSerialize)
		{
			using(var output = new StringWriter())
			using(var writer = new XmlTextWriter(output) { Formatting = Formatting.Indented })
			{
				new DataContractSerializer(typeof(T)).WriteObject(writer, objectToSerialize);
				return output.GetStringBuilder().ToString();
			}
		}

		/// <summary>
		/// Metodo per convertire l'xml rappresentante la form in uno SchemaMetaData comprensibile lato brainBusiness
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rawXml"></param>
		/// <returns></returns>
		private static T Deserialize<T>(string rawXml)
		{
			using(XmlReader reader = XmlReader.Create(new StringReader(rawXml)))
			{
				DataContractSerializer formatter0 =
                    new DataContractSerializer(typeof(T));
				return (T)formatter0.ReadObject(reader);
			}
		}

		/// <summary>
		/// Partendo dal workerId e dal nome della company costruisce lo userName (univoco)
		/// per BrainBusiness
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="workerId"></param>
		/// <returns>il BBuserName nel formato: "Company:#/Worker:#"</returns>
		internal static string BuildBrainBusinessUserName(int companyId, int workerId)
		{
			return "Company:" + companyId.ToString() + "/Worker:" + workerId.ToString();
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
			int startPosition = firstColonIndex + 1;
			var substr = BBUserName.Substring(startPosition, slashIndex - startPosition);
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

		/// <summary>
		/// Metodo generico per scrivere il messaggio nell'eventlog -- copiato da EasyAttachmentSync
		/// </summary>
		/// <param name="message"></param>
		/// <param name="entryType"></param>
		/// <param name="alwaysWrite"></param>
		internal static void WriteMessageInEventLog(string message, EventLogEntryType entryType = EventLogEntryType.Error, bool alwaysWrite = false)
		{
			if  (alwaysWrite ||
				InstallationData.ServerConnectionInfo.EnableEAVerboseLog ||
				(!InstallationData.ServerConnectionInfo.EnableEAVerboseLog && entryType != EventLogEntryType.Information))
			{
				string logEntry = string.Format("{0}: {1}", InstallationData.InstallationName, message);
				try
				{
					eventLog.WriteEntry(logEntry, entryType);
				}
				catch(Exception)
				{
					Debug.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToString(), logEntry));
				}
			}
		}

		/// <summary>
		/// Inserisce una Notifica sul database, da rivedere il tipo di ritorno in base all'avvenuto corretto inserimento
		/// </summary>
		/// <param name="message"></param>
		/// <param name="username"></param>
		public static void SendGenericNotify(GenericNotify notify, bool StoreOnDb)
		{
			if(StoreOnDb)
			{
				try
				{
					notify.NotificationId = NotificationSqlConnection.InsertGenericNotifications(notify);
					notify.StoredOnDb = true;
				}
				catch(Exception e)
				{
					WriteMessageInEventLog(NSStrings.ErrorWhileInsertInDB+ e.Message, EventLogEntryType.Error);
				}
			}
			try
			{
				string bbUserName = BuildBrainBusinessUserName(notify.ToCompanyId, notify.ToWorkerId);
				//XSocketConnector.SendIGenericNotify(bbUserName, notify);
			}
			catch(Exception e)
			{
				WriteMessageInEventLog(NSStrings.ErrorWhileSendingWithXSocket + e.Message, EventLogEntryType.Error);
			}

			//bool insertResult = NotificationSqlConnection.InsertGenericNotifications(notify);
			//string bbUserName = BuildBrainBusinessUserName(notify.ToCompanyId, notify.ToWorkerId);
			//XSocketConnector.SendIGenericNotify(bbUserName, notify);
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
			return NotificationSqlConnection.GetAllNotifications(companyId, workerId, includeProcessed);
			//return null;
		}

		/// <summary>
		/// Fa un update sul db settando a now(momento dlla query) il campo readDate
		/// </summary>
		/// <param name="NotificationId"></param>
		/// <returns></returns>
		public bool SetNotificationAsRead(int NotificationId)
		{
			return NotificationSqlConnection.SetNotificationAsRead(NotificationId);
			//return false;
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

}
//public static class NotificationCoreExtensions
//{
//	/// <summary>
//	/// Writes the given object instance to an XML file.
//	/// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
//	/// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
//	/// <para>Object type must have a parameterless constructor.</para>
//	/// </summary>
//	/// <typeparam name="T">The type of object being written to the file.</typeparam>
//	/// <param name="filePath">The file path to write the object instance to.</param>
//	/// <param name="objectToWrite">The object instance to write to the file.</param>
//	/// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
//	public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
//	{
//		TextWriter writer = null;
//		try
//		{
//			var serializer = new XmlSerializer(typeof(T));
//			writer = new StreamWriter(filePath, append);
//			serializer.Serialize(writer, objectToWrite);
//		}
//		finally
//		{
//			if(writer != null)
//				writer.Close();
//		}
//	}

//	/// <summary>
//	/// Reads an object instance from an XML file.
//	/// <para>Object type must have a parameterless constructor.</para>
//	/// </summary>
//	/// <typeparam name="T">The type of object to read from the file.</typeparam>
//	/// <param name="filePath">The file path to read the object instance from.</param>
//	/// <returns>Returns a new instance of the object read from the XML file.</returns>
//	public static T ReadFromXmlFile<T>(string filePath) where T : new()
//	{
//		TextReader reader = null;
//		try
//		{
//			var serializer = new XmlSerializer(typeof(T));
//			reader = new StreamReader(filePath);
//			return (T)serializer.Deserialize(reader);
//		}
//		finally
//		{
//			if(reader != null)
//				reader.Close();
//		}
//	}

//	public class BrainBusinessService 
//	{
//		public string BrainBusinessServiceUrl { get; set; }
//	}
//}
/*
        ///<summary>
        /// Metodo per recuperare la prima form non processata come XML
        ///</summary>
        //----------------------------------------------------------------
        internal static string GetFirstUnProcessedFormToString(int workerId, int companyId)
        {
            try
            {
                var schema = GetFirstUnProcessedForm(workerId, companyId);
                return DataContractSerializeObject<SchemaMetadata>(schema);
           }
            catch (Exception e)
            {
                return e.ToString();
            }

    //To Json using JavascriptSerializer
            //try
            //{
            //    var schema = GetFirstUnProcessedForm(workerId, companyId);
            //    var json = new JavaScriptSerializer().Serialize(schema);
            //    return json;
            //}
            //catch (Exception e)
            //{
            //    return e.ToString();
            //}
            
    //To Json using DataContractSerializer (no System.Web.Xyz dependencies)
            //var json = new DataContractJsonSerializer(schema.GetType());
            //MemoryStream ms = new MemoryStream();
            //XmlDictionaryWriter writer = JsonReaderWriterFactory.CreateJsonWriter(ms);
            //json.WriteObject(ms, schema);
            //writer.Flush();
            //var jsonString = Encoding.Default.GetString(ms.GetBuffer());
            //return jsonString;

    //To XML
            //System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(schema.GetType());
            //var stringWriter= new StringWriter();
            //using (var xmlWriter = System.Xml.XmlWriter.Create(stringWriter))
            //{
            //  xmlSerializer.Serialize(xmlWriter, schema);
            //}
            //return stringWriter.ToString();


            //DataContractSerializer dcs = new DataContractSerializer(typeof(SchemaMetadata));
            //FileStream fs = new FileStream();
            //XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(fs);
            //dcs.WriteObject(writer, schema);
            //return dcs.ToString();
        }*/

//lista dei client connessi-------nella forma bbUserName-companyLogin--
//todo: vedere se ha senso un dictionary: non possono esserci più worker connessi alla stessa login contemporaneamente
//private Dictionary<string, string> workerLoginDictionary;

//internal void SubScribe(int workerId, int companyId, string companyLogin)
//{
//    var bbUserName = BuildBrainBusinessUserName(workerId, companyId);
//    if(!workerLoginDictionary.ContainsKey(bbUserName))
//        workerLoginDictionary.Add(bbUserName, companyLogin); 
//}

//internal void UnSubScribe(int workerId, int companyId)
//{
//    workerLoginDictionary.Remove(BuildBrainBusinessUserName(workerId, companyId));
//}