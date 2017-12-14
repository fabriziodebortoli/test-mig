using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

using Microarea.EasyAttachment.Core;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.WebServices.EasyAttachmentSync
{
	//================================================================================
	public class EASyncEngine
	{
		private static EventLog eventLog = new EventLog("MA Server", ".", "EasyAttachmentSync");

		private static Timer threadSyncTimer;
		private static Timer mainSOSUpdateTimer;

		private static List<DmsDatabaseInfo> dmsDbInfoList;

		internal static SOSDocumentSender sosSender;

		private static int syncIndexesTickMinutes = 5;
		private static int sosUpdateDocumentsTickHours = 8;
		private static int sosUpdateDocumentsOnDemandTickHours = 4;

		private static bool syncThreadIsRunning = false;

		// property da controllare per sapere se c'e' qualche processo in esecuzione
		// sia di sincronizzazione che di invio al SOS (sono due processi separati)
		//---------------------------------------------------------------------
		internal static bool IsSynchronizing
		{
			get 
			{ 
				if (sosSender != null)
					return syncThreadIsRunning || sosSender.SendToSOSThreadIsStarted;
				return syncThreadIsRunning;
			}
		}
		
		//---------------------------------------------------------------------
		public static string GetTempFolderForEasyAttachmentSync()
		{
			string temp = Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppDataPath(true), "EasyAttachmentSyncTemp");
			if (!Directory.Exists(temp))
				Directory.CreateDirectory(temp);
			return temp;
		}

		//---------------------------------------------------------------------------
		internal static void SetCulture()
		{
			try
			{
				string cui = InstallationData.ServerConnectionInfo.PreferredLanguage;
				string c = InstallationData.ServerConnectionInfo.ApplicationLanguage;

				if (!String.IsNullOrEmpty(c))
					Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(c);

				if (!String.IsNullOrEmpty(cui))
					Thread.CurrentThread.CurrentUICulture = new CultureInfo(cui);
			}
			catch
			{
			}
		}

		///<summary>
		/// Richiamata dal web method Init
		/// Si occupa di istanziare un timer, che ogni 5 minuti richiama la funzione di sincronizzazione
		/// degli indici di ricerca e del text content per il FullText Search
		///</summary>
		//--------------------------------------------------------------------------------
		internal static void InitTimer()
		{
			SetCulture();

			lock (typeof(EASyncEngine))
			{
				ReadAppSettings();

				// quando faccio la Init forzo la Clear della struttura 
				// (perche' devo ri-eseguire gli aggiornamenti per il Full-Text Search) 
				ClearDmsList();

				// la funzione viene richiamata ogni tot minuti (letti dal web.config)
				threadSyncTimer = new Timer
					(
					new TimerCallback(SynchronizeCallBack), 
					null, 
					new TimeSpan(), 
					new TimeSpan(hours: 0, minutes: syncIndexesTickMinutes, seconds: 0)
					);

				// la funzione viene richiamata ogni tot ore (letti dal web.config), ma la prima volta parte 7 minuti dopo la sincronizzazione
				// (cosi' non coincide con i 5 minuti della sicronizzazione degli indici)
				mainSOSUpdateTimer = new Timer
					(
					new TimerCallback(MainTimerCallBack), 
					null, 
					new TimeSpan(hours: 0, minutes: 7, seconds: 0), 
					new TimeSpan(hours: sosUpdateDocumentsTickHours, minutes: 0, seconds: 0)
					);

				WriteMessageInEventLog(EASyncStrings.EASyncInit, EventLogEntryType.Information, true);
			}
		}

		///<summary>
		/// Leggo tutti i parametri definiti nel web.config
		///</summary>
		//--------------------------------------------------------------------------------
		private static void ReadAppSettings()
		{
			//*************
			// legge il tempo in minuti dal parametro SyncIndexesTickMinutes nel web.config
			//*************
			int tickMinutesMinimum = 1; int tickMinutesMaximum = 60; int tickMinutesDefault = 5;
			syncIndexesTickMinutes = tickMinutesDefault;
			string tickMinutesStr = ConfigurationManager.AppSettings["SyncIndexesTickMinutes"];

			if (string.IsNullOrWhiteSpace(tickMinutesStr))
			{
				if (!Int32.TryParse(tickMinutesStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out syncIndexesTickMinutes))
					syncIndexesTickMinutes = tickMinutesDefault;
			}

			if (syncIndexesTickMinutes < tickMinutesMinimum)
				syncIndexesTickMinutes = tickMinutesMinimum;
			else if (syncIndexesTickMinutes > tickMinutesMaximum)
				syncIndexesTickMinutes = tickMinutesMaximum;

			//*************
			// legge il tempo in ore dal parametro SOSUpdateDocumentsTickHours nel web.config
			//*************
			int sosUpdateDocumentsTickHoursMinimum = 8; int sosUpdateDocumentsTickHoursMaximum = 24; int sosUpdateDocumentsTickHoursDefault = 8;
			sosUpdateDocumentsTickHours = sosUpdateDocumentsTickHoursDefault;
			string sosUpdateDocumentsTickHoursStr = ConfigurationManager.AppSettings["SOSUpdateDocumentsTickHours"];

			if (string.IsNullOrWhiteSpace(sosUpdateDocumentsTickHoursStr))
			{
				if (!Int32.TryParse(sosUpdateDocumentsTickHoursStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out sosUpdateDocumentsTickHours))
					sosUpdateDocumentsTickHours = sosUpdateDocumentsTickHoursDefault;
			}

			if (sosUpdateDocumentsTickHours < sosUpdateDocumentsTickHoursMinimum)
				sosUpdateDocumentsTickHours = sosUpdateDocumentsTickHoursMinimum;
			else if (sosUpdateDocumentsTickHours > sosUpdateDocumentsTickHoursMaximum)
				sosUpdateDocumentsTickHours = sosUpdateDocumentsTickHoursMaximum;

			//*************
			// legge il tempo in ore dal parametro SOSUpdateDocumentsOnDemandTickHours nel web.config
			//*************
			int sosUpdateDocumentsOnDemandTickHoursMinimum = 1; int sosUpdateDocumentsOnDemandTickHoursMaximum = 7; int sosUpdateDocumentsOnDemandTickHoursDefault = 4;
			sosUpdateDocumentsOnDemandTickHours = sosUpdateDocumentsOnDemandTickHoursDefault;
			string sosUpdateDocumentsOnDemandTickHoursStr = ConfigurationManager.AppSettings["SOSUpdateDocumentsOnDemandTickHours"];

			if (string.IsNullOrWhiteSpace(sosUpdateDocumentsOnDemandTickHoursStr))
			{
				if (!Int32.TryParse(sosUpdateDocumentsOnDemandTickHoursStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out sosUpdateDocumentsOnDemandTickHours))
					sosUpdateDocumentsOnDemandTickHours = sosUpdateDocumentsOnDemandTickHoursDefault;
			}

			if (sosUpdateDocumentsOnDemandTickHours < sosUpdateDocumentsOnDemandTickHoursMinimum)
				sosUpdateDocumentsOnDemandTickHours = sosUpdateDocumentsOnDemandTickHoursMinimum;
			else if (sosUpdateDocumentsOnDemandTickHours > sosUpdateDocumentsOnDemandTickHoursMaximum)
				sosUpdateDocumentsOnDemandTickHours = sosUpdateDocumentsOnDemandTickHoursMaximum;

			// per velocizzare le chiamate all'UpdateStatus dei documenti inviati in SOS (nel caso di test/demo)
			// in modo da ignorare il parametro nel Web.config e impostare ogni 4 minuti
			// scommentare il codice sottostante 
			// N.B. (e' necessario anche modificare nel metodo UpdateSOSDocumentsStatus la chiamata di AddHours in AddMinutes)
			// sosUpdateDocumentsOnDemandTickHours = 4;
		}

		///<summary>
		/// Richiamato dal web method Stop
		/// Si occupa di effettuare la Dispose del timer che viene eseguito ogni 5 minuti
		///</summary>
		//--------------------------------------------------------------------------------
		internal static bool StopTimer()
		{
			if (IsSynchronizing)
			{
				WriteMessageInEventLog(EASyncStrings.UnableToStopEASync, EventLogEntryType.Warning);
				return false;
			}

			if (threadSyncTimer != null)
			{
				threadSyncTimer.Dispose();
				WriteMessageInEventLog(EASyncStrings.StoppingEASync, EventLogEntryType.Information, true);
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		public static void WriteLog(Exception ex)
		{
			//todo verificare se thread safe
			lock (eventLog)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:WriteLog", ex.ToString()));
			}
		}

		///<summary>
		/// Si occupa di fare la clear della lista delle info (solo se non sto eseguendo la sincronizzazione)
		///</summary>
		//-----------------------------------------------------------------------
		internal static bool ClearDmsList()
		{
			if (IsSynchronizing)
			{
				WriteMessageInEventLog(EASyncStrings.FTSClearDmsListWithError, EventLogEntryType.Warning);
				return false;
			}

			if (dmsDbInfoList != null)
			{
				dmsDbInfoList.Clear();
				dmsDbInfoList = null;
				WriteMessageInEventLog(EASyncStrings.FTSClearDmsListWithSuccess, EventLogEntryType.Information);
			}

			return true;
		}

		///<summary>
		/// Dato un companyId ritorna se l'azienda corrispondente ha il db di EA con il componente FullText Search abilitato
		///</summary>
		//-----------------------------------------------------------------------
		internal static bool IsFTSEnabled(int companyId)
		{
			DmsDatabaseInfo dmsInfo = GetDmsDatabaseFromCompanyId(companyId);

			return (dmsInfo != null) ? dmsInfo.IsFTSEnabled : false;
		}

		///<summary>
		/// Dato un companyId se l'azienda corrispondente ha il db di EA con il componente FullText Search abilitato
		/// Abilita/disabilita gli indici FullText Search sulle specifiche tabelle
		/// Viene richiamato in fase di salvataggio dei parametri di EA
		///</summary>
		//-----------------------------------------------------------------------
		internal static bool SuspendFTS(bool suspend, int companyId)
		{
			bool result = false;
			// se sto eseguendo la sincronizzazione non procedo
			if (IsSynchronizing)
				return result;

			DmsDatabaseInfo dmsInfo = GetDmsDatabaseFromCompanyId(companyId);

			// se ho trovato le info della company 
			if (dmsInfo != null)
			{
				// valorizzo il flag di Uso del FullText nell'azienda
				dmsInfo.UseFTS = !suspend;

				// se il componente FullText e' installato su SQL vado ad abilitare/disabilitare gli indici fulltext
				if (dmsInfo.IsFTSEnabled)
					result = AlterFullTextIndexes(dmsInfo, suspend);
			}
			return result;
		}

		///<summary>
		/// Abilita/disabilita gli indici FullText Search sulle specifiche tabelle
		///</summary>
		//-----------------------------------------------------------------------
		private static bool AlterFullTextIndexes(DmsDatabaseInfo dmsInfo, bool suspend)
		{
			try
			{
				using (TBConnection dmsConnection = new TBConnection(dmsInfo.DMSConnectionString, DBMSType.SQLSERVER))
				{
					dmsConnection.Open();

					TBCommand myCommand = new TBCommand(dmsConnection);

					// abilito/disabilito l'indice sulla DMS_ArchivedDocContent
					myCommand.CommandText =
						suspend
						? string.Format(EASyncConsts.DisableFullTextIndex, EASyncConsts.DMS_ArchivedDocContent)
						: string.Format(EASyncConsts.EnableFullTextIndex, EASyncConsts.DMS_ArchivedDocContent);
					myCommand.ExecuteNonQuery();

					// abilito/disabilito l'indice sulla DMS_ArchivedDocTextContent
					myCommand.CommandText =
						suspend
						? string.Format(EASyncConsts.DisableFullTextIndex, EASyncConsts.DMS_ArchivedDocTextContent)
						: string.Format(EASyncConsts.EnableFullTextIndex, EASyncConsts.DMS_ArchivedDocTextContent);
					myCommand.ExecuteNonQuery();
				}
			}
			catch(TBException tbEx)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:AlterFullTextIndexes", tbEx.ToString()));
				return false;
			}
			catch (Exception e)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:AlterFullTextIndexes", e.ToString()));
				return false;
			}

			return true;
		}

		///<summary>
		/// Cerca una specifica company nella lista e ritorna le sue informazioni
		///</summary>
		//-----------------------------------------------------------------------
		internal static DmsDatabaseInfo GetDmsDatabaseFromCompanyId(int companyId)
		{
			DmsDatabaseInfo dmsInfo = null;

			// se la lista e' valida oppure sono riuscito a caricare con successo le info 
			// cerco le info per il companyId richiesto
			if (dmsDbInfoList != null || InitStructures())
			{
				foreach (DmsDatabaseInfo ddi in dmsDbInfoList)
				{
					if (ddi.CompanyId == companyId)
					{
						dmsInfo = ddi;
						break;
					}
				}
			}

			return dmsInfo;
		}

		///<summary>
		/// Istanzia la classe SOSSender, se necessario, ed eventualmente le informazioni
		/// dei singoli database di EA. Aggiungo alla coda gli attachment da inviare
		///</summary>
		//-----------------------------------------------------------------------
		internal static void EnqueueAttachments(int companyId, List<int> attachmentIds, int loginId)
		{ 
			// istanzio la classe SOSDocumentSender, se necessario
			if (sosSender == null)
				sosSender = new SOSDocumentSender();

			if (InitStructures())
			{ 
				DmsDatabaseInfo dmsInfo = GetDmsDatabaseFromCompanyId(companyId);
				if (dmsInfo == null)
					return;

				// accodo l'elemento alla queue
				sosSender.EnqueueElement(dmsInfo, attachmentIds, loginId);
			}
		}

		///<summary>
		/// Metodo richiamato dalla CallBack del Timer
		/// Viene eseguito ogni 5 minuti
		///</summary>
		//-----------------------------------------------------------------------
		private static void SynchronizeCallBack(object o)
		{
			//sto già sincronizzando
			if (IsSynchronizing)
				return;

			syncThreadIsRunning = true;

			SetCulture();

			if (InitStructures())
			{
				// eseguo la sincronizzazione degli indici di ricerca (e del textcontent per il FullText Search se necessario)
				if (!SynchronizeIndexes())
					WriteMessageInEventLog(string.Format(EASyncStrings.MethodEndedWithError, "EasyAttachmentSync:SynchronizeIndexes"));
			}

			syncThreadIsRunning = false;
		}

		///<summary>
		/// Istanzio LM e mi faccio ritornare l'elenco delle info delle aziende che usano EA
		///</summary>
		//-----------------------------------------------------------------------
		private static bool InitStructures()
		{
			// prima carico dal database di sistema le informazioni relative alle aziende che usano EA
			if (!LoadDmsStructure())
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodEndedWithError, "EasyAttachmentSync:LoadDmsStructure"));
				return false;
			}

			if (dmsDbInfoList == null || dmsDbInfoList.Count == 0)
				return false;

			// richiamo la funzione di configurazione degli oggetti per l'FTS ed altre funzioni
			InitConfigure(); 

			return true;
		}

		///<summary>
		/// Istanzio il LM per poter poi richiamare il web method che mi serve
		///</summary>
		//--------------------------------------------------------------------------------
		private static Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager GetLoginManager()
		{
			try
			{
				Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager loginManager =
					new Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager();

				return loginManager;
			}
			catch (Exception e)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, EASyncStrings.LMInit, e.ToString()), EventLogEntryType.Error, true);
				return null;
			}
		}

		///<summary>
		/// Istanzio LM e mi faccio ritornare l'elenco delle info delle aziende che usano EA
		///</summary>
		//-----------------------------------------------------------------------
		private static bool LoadDmsStructure()
		{
			try
			{
				// se LM e' null e non riesco ad inizializzarlo non procedo
				Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager loginManager = GetLoginManager();
				if (loginManager == null)
					return false;

				// chiedo al LM l'elenco delle stringhe di connessione per le aziende che usano EA
				List<DmsDatabaseInfo> dmsList = loginManager.GetDMSDatabasesInfo("{2E8164FA-7A8B-4352-B0DC-479984070507}");

				// se la lista e' vuota non procedo
				if (dmsList == null || dmsList.Count == 0)
					return true;

				// se la lista principale e' vuota la valorizzo subito
				if (dmsDbInfoList == null || dmsDbInfoList.Count == 0)
					dmsDbInfoList = dmsList;
				else
				{
					// se la lista principale contiene degli elementi, devo aggiungere i soli elementi nuovi e contrassegnare come gia'
					// analizzati quelli vecchi
					foreach (DmsDatabaseInfo recent in dmsList)
					{
						bool found = false;

						foreach (DmsDatabaseInfo old in dmsDbInfoList)
						{
							if (
								string.Compare(recent.DMSConnectionString, old.DMSConnectionString, StringComparison.InvariantCultureIgnoreCase) == 0 &&
								recent.CompanyId == old.CompanyId // fix anomalia 21005 
								)
							{
								old.LCID = recent.LCID;
								found = true;
								break;
							}
						}

						if (!found) // se non ho trovato il db lo aggiungo alla lista
							dmsDbInfoList.Add(recent);
					}
				}
			}
			catch (Exception e)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:LoadDmsStructure", e.ToString()));
				return false;
			}
			return true;
		}

		///<summary>
		/// Procedura di sincronizzazione degli indici di ricerca
		/// Per ogni database di EasyAttachment eseguo la connessione e richiamo il processo
		/// che mette a posto i valori degli indici
		///</summary>
		//-----------------------------------------------------------------------
		private static bool SynchronizeIndexes()
		{
			try
			{
				// se LM non ritorna stringhe di connessione ai database non procedo
				if (dmsDbInfoList == null || dmsDbInfoList.Count == 0)
					return true;
				
				// se la lista contiene almeno un elemento richiamo il processo di sincronizzazione per ogni database
				// al momento non creo un nuovo task perche' non vorrei che ci fossero piu' aziende collegate allo stesso database dms
				foreach (DmsDatabaseInfo info in dmsDbInfoList)
				{
					if (string.IsNullOrWhiteSpace(info.DMSConnectionString))
						continue;

					RunSynchronizeIndexesForDB(info);
				}
			}
			catch (Exception e)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:SynchronizeIndexes", e.ToString()));
				return false;
			}
			return true;
		}

		///<summary>
		/// Esecuzione processo di sincronizzazione degli indici di ricerca per un database
		/// e aggiornamento del text content solo se il FullText Search e' abilitato
		///</summary>
		//-----------------------------------------------------------------------
		private static void RunSynchronizeIndexesForDB(DmsDatabaseInfo info)
		{
			// compongo un messaggio da aggiungere in coda a quello di errore
			string message = string.Format(" - Server '{0} Database '{1}' - ", info.Server, info.Database); 

			// istanzio il model con la stringa di connessione
			DMSModelDataContext dmsModel = new DMSModelDataContext(info.DMSConnectionString);
			
			try
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.RunSynchronizeDB, dmsModel.Connection.Database, dmsModel.Connection.DataSource), EventLogEntryType.Information);
				
				// controllo che il database a cui vorrei connettermi esista
				// ma SOLO SE la connessione non e' in winauth (altrimenti non mi entra nella gestione
				// delle eccezioni e vanifico il messaggio successivo)
				if (info.DMSConnectionString.IndexOf(EASyncConsts.SSPI) == -1 && !dmsModel.DatabaseExists())
				{
					WriteMessageInEventLog(string.Format(EASyncStrings.CheckDBExist, dmsModel.Connection.Database, dmsModel.Connection.DataSource), EventLogEntryType.Warning);
					return;
				}

			    // indipendentemente dalle operazioni precedenti cancello gli eventuali SearchFieldIndex non più utilizzati 
				// (ovvero non presenti in DMS_AttachmentSearchFieldIndexes e DMS_ArchivedDocSearchIndexes)
				GarbageSearchFieldIndexes(dmsModel, info);

				// se il componente FullText e' installato su SQLServer e la ricerca e' abilitata sull'azienda procedo all'aggiornamento dei contenuti
				if (info.IsFTSEnabled && info.UseFTS)
					UpdateFTSContent(dmsModel, info);
			}
			catch (SqlException sqlExc)
			{
				// nel caso in cui la stringa di connessione al database sia con utente in WinAuth
				// l'eccezione potrebbe essere stata sollevata dall'impossibilita' di effettuare l'impersonificazione dell'utente ASP.NET di IIS
				// per la connessione a SQL Server
				// La soluzione a questo problema (che pertanto si verifica SOLO in caso di dbowner del database in WinAuth) si ottiene come segue:
				// 1. in IISManager bisogna identificare l'applicationpool collegato al webservice EasyAttachmentSync
				// 2. andare in Advanced Settings, selezionare il parametro 'Identity' nella sezione 'Process Model' e scegliere l'opzione
				// Custom Account, specificando il nome dell'utente di dominio <domain>\<user> e la relativa password
				if (info.DMSConnectionString.IndexOf(EASyncConsts.SSPI) > 0)
				{
					WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "01 EasyAttachmentSync:RunSynchronizeIndexesForDB" + message, sqlExc.Message));
					if (sqlExc.Number == 18456)
						WriteMessageInEventLog(string.Format(EASyncStrings.ChangeIdentityAppPool, dmsModel.Connection.Database));
					return;
				}

				// l'errore 208 viene scatenato quando la tabella coinvolta nella query non esiste sul database
				if (sqlExc.Number == 208)
				{
					WriteMessageInEventLog(string.Format(EASyncStrings.CheckTablesExist, dmsModel.Connection.Database, dmsModel.Connection.DataSource), EventLogEntryType.Warning);
					return;
				}

				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "02 EasyAttachmentSync:RunSynchronizeIndexesForDB" + message, sqlExc.ToString()));
			}
			catch (Exception e)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "03 EasyAttachmentSync:RunSynchronizeIndexesForDB" + message, e.ToString()));
			}
			finally
			{
 				if (dmsModel != null)
					dmsModel.Dispose();
			}
		}

		///<summary>
		/// Metodo che si occupa di fare pulizia nella tabella DMS_SearchFieldIndexes
		/// eliminando le righe i cui indici non sono piu' utilizzati negli allegati o nei documenti archiviati
		/// SENZA L'UTILIZZO DI LINQ (introdotto per la risoluzione dell'anomalia 20494)
		///</summary>
		//----------------------------------------------------------------
		private static void GarbageSearchFieldIndexes(DMSModelDataContext dmsModel, DmsDatabaseInfo dmsInfo)
		{
			List<string> idxsToDelete = new List<string>();

			TBConnection dmsConnection = null;
			TBCommand tbCommand = null;
			IDataReader tbDataReader = null;

			try
			{
				// creo ed apro la connessione
				dmsConnection = new TBConnection(dmsInfo.DMSConnectionString, DBMSType.SQLSERVER);
				dmsConnection.Open();

				tbCommand = new TBCommand("SELECT * FROM [udf_GetUnusedFieldIndexes]()", dmsConnection);
				tbDataReader = tbCommand.ExecuteReader();

				while (tbDataReader.Read())
					idxsToDelete.Add(tbDataReader[0].ToString()); // mi tengo da parte gli indici da eliminare ritornati dall'udf

				if (tbDataReader != null && !tbDataReader.IsClosed)
				{
					tbDataReader.Close();
					tbDataReader.Dispose();
				}
				
				// se ho trovato degli indici inutilizzati procedo alla loro cancellazione
				if (idxsToDelete.Count != 0)
				{
					tbCommand.CommandText = "DELETE FROM DMS_SearchFieldIndexes WHERE SearchIndexID = @paramIdx";
					tbCommand.Parameters.Add("@paramIdx", SqlDbType.VarChar);

					foreach (var idx in idxsToDelete)
					{
						((IDataParameter)tbCommand.Parameters.GetParameterAt(0)).Value = idx;
						tbCommand.ExecuteNonQuery();
					}
				}
			}
			catch (SqlException sqlExc)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "01 EasyAttachmentSync:GarbageSearchFieldIndexes", sqlExc.ToString()));
			}
			catch (Exception e)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "02 EasyAttachmentSync:GarbageSearchFieldIndexes", e.ToString()));
			}
			finally
			{
				if (tbCommand != null) 
					tbCommand.Dispose();
				if (tbDataReader != null && !tbDataReader.IsClosed)
				{
					tbDataReader.Close();
					tbDataReader.Dispose();
				}
				if (dmsConnection != null && dmsConnection.State != ConnectionState.Closed)
				{
					dmsConnection.Close();
					dmsConnection.Dispose();
				}
			}
		}
		
		///<summary>
		/// Istanzio un FullTextManager ed eseguo l'update del text content per il FullText Search
		///</summary>
		//-----------------------------------------------------------------------
		private static void UpdateFTSContent(DMSModelDataContext dmsModel, DmsDatabaseInfo dmsInfo)
		{
			string eaSyncTempPath = string.Empty;
			string message = string.Empty; // only for log

			// First delete the old files
			try
			{
				eaSyncTempPath = GetTempFolderForEasyAttachmentSync();
				if (string.IsNullOrWhiteSpace(eaSyncTempPath))
					return;

				// N.B. il terzo parametro e' a false: non vado ricorsivamente perche' altrimenti 
				// elimino anche l'eventuale sottodirectorySOSConnectorTemp
				Functions.DeleteFiles(eaSyncTempPath, "*", false); 
			}
			catch (IOException ioExc)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "01 EasyAttachmentSync:UpdateFTSContent", message + ioExc.ToString()));
				return;
			}
			catch (Exception e)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "02 EasyAttachmentSync:UpdateFTSContent", message + e.ToString()));
				return;
			}

			try
			{
				FullTextManager ftm = new FullTextManager(dmsModel, eaSyncTempPath, dmsInfo.LCID);
				ftm.UpdateTextContentCompleted += new EventHandler<FullTextInfoEventArgs>(ftm_UpdateTextContentCompleted);
				ftm.CreateDocumentsContent(dmsInfo.DMSConnectionString, string.Compare(dmsInfo.ExtensionTypeCollate, dmsInfo.FulltextDocumentTypesCollate, true) == 0 ? string.Empty : dmsInfo.ExtensionTypeCollate);
			}
			catch(Exception e)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "03 EasyAttachmentSync:UpdateFTSContent", e.ToString()));
			}
		}

		///<summary>
		/// Scrivo nell'EventViewer i messaggi inviati dal FullTextManager
		///</summary>
		//-----------------------------------------------------------------------
		private static void ftm_UpdateTextContentCompleted(object sender, FullTextInfoEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.Message))
				WriteMessageInEventLog(e.Message, e.ProcessResult ? EventLogEntryType.Information : EventLogEntryType.Error);
		}
	
		///<summary>
		/// Inizializzo la struttura con le informazioni dei database di EA
		///</summary>
		//--------------------------------------------------------------------------------
		internal static void InitConfigure()
		{
			// se la lista contiene almeno un elemento richiamo il processo di check delle opzioni di FullText Search
			// solo per gli elementi che non sono stati analizzati
			foreach (DmsDatabaseInfo info in dmsDbInfoList)
			{
				if (!info.IsAlreadyAnalyzed)
					ConfigureSingleDatabase(info);
			}
		}

		///<summary>
		/// Per ogni database di EA:
		/// 1. apro la connessione
		/// 2. controllo che il componente full-text search sia installato in SQL
		/// 3. controllo che il database sia stato abilitato all'utilizzo del full-text search
		/// 4. creo il catalog fulltext se non esiste
		/// 5. creo l'indice fulltext sulla tabella DMS_ArchivedDocContent e DMS_ArchivedDocTextContent (impostando il LANGUAGE corretto ove possibile)
		/// 6. ri-creo le due user defined function (udf_FTS_SearchAttachments e udf_FTS_SearchArchivedDoc)
		/// 7. carico i tipi di estensioni per la COLLATE specificata
		/// 8. abilito il caricamento degli IFilter del sistema operativo
		///</summary>
		//--------------------------------------------------------------------------------
		private static void ConfigureSingleDatabase(DmsDatabaseInfo info)
		{
			// compongo un messaggio da aggiungere in coda a quello di errore
			string message = string.Format(" - Server '{0} Database '{1}' - ", info.Server, info.Database); 

			TBConnection dmsConnection = null;
			TBCommand myCommand = null;

			try
			{
				// creo ed apro la connessione
				dmsConnection = new TBConnection(info.DMSConnectionString, DBMSType.SQLSERVER);
				dmsConnection.Open();
				myCommand = new TBCommand(dmsConnection);

				//prima elimino e ri-creo la funzione [udf_GetUnusedFieldIndexes]				
				myCommand.CommandText = string.Format(EASyncConsts.DropUDF, "udf_GetUnusedFieldIndexes");
				myCommand.ExecuteNonQuery();
				// carico il file contenente le istruzioni per creare la user defined query udf_FTS_GetUnusedFieldIndexes dalle risorse 
				// (occhio che deve contenere il solo testo della funzione!)
				myCommand.CommandText = Properties.Resources.udf_GetUnusedFieldIndexes;
				myCommand.ExecuteNonQuery();

				TBDatabaseSchema tbSchema = new TBDatabaseSchema(dmsConnection);
				if (tbSchema.ExistTable(EASyncConsts.DMS_Settings))
				{	
					// eseguo una query sulla tabella DMS_Settings e controllo se la ricerca full-text e' in uso nell'azienda (parametro per tutti i worker)
					myCommand.CommandText = @"SELECT Settings.query('/SettingState/Options/FTSOptions/EnableFTS/text()') 
										FROM [DMS_Settings] WHERE WorkerID = -1 AND SettingType = 2";
					using (XmlReader reader = myCommand.ExecuteXmlReader())
					{
						if (reader != null && reader.Read())
							info.UseFTS = string.Compare(reader.Value, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0;
						else
							info.UseFTS = false;
					}
				}

				// la verifica la faccio solo se esistono le tabelle su cui verranno creati gli indici di ricerca. Se queste non sono presenti
				// (ad esempio perchè deve essere fatto ancora il passaggio di release dalla 1 alla 2 allora non proseguo con il controllo
				// e lascio info.IsAlreadyAnalyzed == false in modo da poter rifare il controllo al prossimo ciclo, sperando che l'utente abbia fatto
				// nel mentre lo scatto di release del DB di EasyAttachment
				if (!tbSchema.ExistTable(EASyncConsts.DMS_ArchivedDocContent) || !tbSchema.ExistTable(EASyncConsts.DMS_ArchivedDocTextContent))
				{
					dmsConnection.Close();
					dmsConnection.Dispose();
					return;
				}

				WriteMessageInEventLog(string.Format(EASyncStrings.FTSChecking, dmsConnection.Database, dmsConnection.DataSource), EventLogEntryType.Information);

				info.IsFTSEnabled = false;
				info.IsAlreadyAnalyzed = true;

				// controllo che il server abbia il componente Full-Text Search installato
				if (!TBCheckDatabase.IsFullTextSearchInstalled(dmsConnection))
				{
					WriteMessageInEventLog(string.Format(EASyncStrings.FTSNotInstalled, dmsConnection.Database, dmsConnection.DataSource), EventLogEntryType.Information);
					return;
				}

				// controllo che il server abbia il componente Full-Text Search abilitato e provo ad abilitarlo (solo per i server versione 2005)
				if (!TBCheckDatabase.CheckFullTextSearchEnabled(dmsConnection))
				{
					WriteMessageInEventLog(string.Format(EASyncStrings.FTSEnableError, dmsConnection.Database, dmsConnection.DataSource), EventLogEntryType.Information);
					return;
				}

				// creazione catalog
				myCommand.CommandText = string.Format(EASyncConsts.CreateFTSCatalog, dmsConnection.Database);
				myCommand.ExecuteNonQuery();
				
				// verifico se la cultura del database e' compatibile con i fulltext_languages previsti da SQL
				bool isLanguageSupported = TBCheckDatabase.IsSupportedLanguageForFullTextSearch(dmsConnection, info.LCID);

				// creazione indice fulltext sulla tabella DMS_ArchivedDocContent
				myCommand.CommandText = string.Format
					(
					EASyncConsts.CreateFTIndexOnBinary,
					isLanguageSupported ? string.Concat("LANGUAGE ", info.LCID.ToString()) : string.Empty, // imposto o meno esplicitamente il LANGUAGE
					dmsConnection.Database
					);
				myCommand.ExecuteNonQuery();

				// creazione indice fulltext sulla tabella DMS_ArchivedDocTextContent
				myCommand.CommandText = string.Format
					(
					EASyncConsts.CreateFTIndexOnText,
					isLanguageSupported ? string.Concat("LANGUAGE ", info.LCID.ToString()) : string.Empty, // imposto o meno esplicitamente il LANGUAGE
					dmsConnection.Database
					);
				myCommand.ExecuteNonQuery();

				// elimino la user defined query udf_FTS_SearchAttachments
				myCommand.CommandText = string.Format(EASyncConsts.DropUDF, "udf_FTS_SearchAttachments");
				myCommand.ExecuteNonQuery();
				// carico il file contenente le istruzioni per creare la user defined query udf_FTS_SearchAttachments dalle risorse 
				// (occhio che deve contenere il solo testo della funzione!)
				myCommand.CommandText = Properties.Resources.udf_FTS_SearchAttachments;
				myCommand.ExecuteNonQuery();

				// elimino la user defined query udf_FTS_SearchArchivedDoc
				myCommand.CommandText = string.Format(EASyncConsts.DropUDF, "udf_FTS_SearchArchivedDoc");
				myCommand.ExecuteNonQuery();
				// carico il file contenente le istruzioni per creare la user defined query udf_FTS_SearchArchivedDoc dalle risorse 
				// (occhio che deve contenere il solo testo della funzione!)
				myCommand.CommandText = Properties.Resources.udf_FTS_SearchArchivedDoc;
				myCommand.ExecuteNonQuery();

				myCommand.CommandText = @"select COLLATION_NAME from INFORMATION_SCHEMA.COLUMNS 
										where TABLE_NAME = 'DMS_ArchivedDocContent' and COLUMN_NAME = 'ExtensionType'";
				object objColl = myCommand.ExecuteScalar();
				if (objColl != null)
					info.ExtensionTypeCollate = (string)objColl;
				info.FulltextDocumentTypesCollate = TBCheckDatabase.GetServerCollation(dmsConnection);

				// abilito il caricamento degli IFilter del sistema operativo
				myCommand.CommandText = EASyncConsts.LoadOSResources;
				myCommand.ExecuteNonQuery();

				// imposto la ricerca FullText come abilitata
				info.IsFTSEnabled = true;
				
				WriteMessageInEventLog(string.Format(EASyncStrings.FTSUpdating, dmsConnection.Database, dmsConnection.DataSource), EventLogEntryType.Information);
			}
			catch (TBException e)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "01 EasyAttachmentSync:ConfigureSingleDatabase" + message, e.ToString()));
			}
			catch (Exception e)
			{
				WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "02 EasyAttachmentSync:ConfigureSingleDatabase" + message, e.ToString()));
			}
			finally
			{
				if (myCommand != null)
					myCommand.Dispose();
				if (dmsConnection != null && dmsConnection.State != ConnectionState.Closed)
				{
					dmsConnection.Close();
					dmsConnection.Dispose();
				}
			}
		}

		///<summary>
		/// Metodo richiamato dalla CallBack del MainTimer
		/// Viene eseguito ogni 8 ore
		///</summary>
		//-----------------------------------------------------------------------
		private static void MainTimerCallBack(object o)
		{
			if (InitStructures())
			{
				//sto già sincronizzando quindi non procedo
				if (IsSynchronizing)
					return;

				foreach (DmsDatabaseInfo dmsInfo in dmsDbInfoList)
				{
					// procedo solo se il SOSConnector e' attivato
					if (dmsInfo == null || !dmsInfo.IsSOSActivated)
						continue;

					SetCulture();

					WriteMessageInEventLog(string.Format(EASyncStrings.SOSRunUpdateDocumentStatus, dmsInfo.Database, dmsInfo.Server), EventLogEntryType.Information);

					if (!InternalUpdateSOSDocumentsStatus(dmsInfo))
						WriteMessageInEventLog(string.Format(EASyncStrings.SOSRunUpdateDocumentStatusEndedWithError, dmsInfo.Database, dmsInfo.Server), EventLogEntryType.Error);
					//else WriteMessageInEventLog(string.Format(EASyncStrings.SOSRunUpdateDocumentStatusSuccessfullyCompleted, dmsInfo.Database, dmsInfo.Server), EventLogEntryType.Information);
				}
			}
		}

		///<summary>
		/// Metodo richiamato dal webmethod omonimo, utilizzato dalla form Monitor dentro EasyAttachment,
		/// che si occupa di aggiornare lo stato dei documenti in SOS
		///</summary>
		//----------------------------------------------------------------
		internal static bool UpdateSOSDocumentsStatus(int companyId, out string message)
		{
			message = string.Empty;
			bool result = true;

			SetCulture();

			DmsDatabaseInfo dmsInfo = GetDmsDatabaseFromCompanyId(companyId);
			
			// solo se il SOSConnector e' attivato allora procedo
			if (dmsInfo != null && dmsInfo.IsSOSActivated)
			{
				// procedo all'aggiornamento solo se sono trascorse almeno tot ore dall'ultima sincronizzazione
				if (DateTime.Now >= dmsInfo.LastSOSUpdateDateTime.AddHours(sosUpdateDocumentsOnDemandTickHours))
					result = InternalUpdateSOSDocumentsStatus(dmsInfo);
			}

			message = result 
					? string.Format(EASyncStrings.SOSLastUpdateDocRequested, dmsInfo.LastSOSUpdateDateTime.ToString("g", CultureInfo.CreateSpecificCulture("it-IT")))
					: EASyncStrings.SOSUpdateDocWithError;

			if (!result)
				WriteMessageInEventLog(string.Format(EASyncStrings.SOSRunUpdateDocumentStatusEndedWithError, dmsInfo.Database, dmsInfo.Server), EventLogEntryType.Error);
				//WriteMessageInEventLog(string.Format(EASyncStrings.SOSRunUpdateDocumentStatus, dmsInfo.Database, dmsInfo.Server), EventLogEntryType.Information);

			return result;
		}

		///<summary>
		/// Metodo interno che si occupa di eseguire l'aggiornamento dello stato dei documenti SOS
		///</summary>
		//----------------------------------------------------------------
		private static bool InternalUpdateSOSDocumentsStatus(DmsDatabaseInfo dmsInfo)
		{
			if (string.IsNullOrWhiteSpace(dmsInfo.DMSConnectionString))
				return false;

			lock (dmsInfo)
			{
				DMSModelDataContext dmsModel = new DMSModelDataContext(dmsInfo.DMSConnectionString);

				try
				{
					// x ora commento questa query
					// considero le sole envelope la cui SynchronizedDate e' trascorsa da almeno 2 ore 
					/*external
					? from docs in dmsModel.DMS_SOSEnvelopes where ((TimeSpan)(DateTime.Now - docs.SynchronizedDate)).TotalHours >= 2 select docs
					: from docs in dmsModel.DMS_SOSEnvelopes where ((TimeSpan)(DateTime.Now - docs.DispatchDate)).TotalHours >= 2 &&
					  ((TimeSpan)(DateTime.Now - docs.SynchronizedDate)).TotalHours <= 24 select docs;*/

					var docToSync = from docs in dmsModel.DMS_SOSEnvelopes select docs;
					if (docToSync == null || !docToSync.Any())
					{
						// se non ho trovato record cmq aggiorno la data
						dmsInfo.LastSOSUpdateDateTime = DateTime.Now;
						return true;
					}

					// carico le credenziali dalla tabella DMS_SOSConfiguration
					DMS_SOSConfiguration sosConf = (from sc in dmsModel.DMS_SOSConfigurations select sc).Single();
					if (sosConf == null || string.IsNullOrWhiteSpace(sosConf.KeeperCode) || string.IsNullOrWhiteSpace(sosConf.SubjectCode))
						return false;

					// Istanzio un proxy temporaneo
					SOSProxyWrapper sosProxy = new SOSProxyWrapper(BasePathFinder.BasePathFinderInstance.SOSProxyUrl);
					sosProxy.Init(sosConf.KeeperCode, sosConf.SubjectCode, sosConf.MySOSUser, Crypto.Decrypt(sosConf.MySOSPassword), sosConf.SOSWebServiceUrl);

					SOSLogWriter.WriteLogEntry(dmsInfo.Company, "STARTING InternalUpdateSOSDocumentsStatus");

					foreach (DMS_SOSEnvelope envelope in docToSync)
					{
						if (envelope.SendingType == (int)SendingType.FTP)
						{
							// stabilisco il prossimo giorno della settimana (escluso oggi) 
							DateTime nextSynchroDay = GetNextWeekday(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59, 999).AddDays(1), (DayOfWeek)sosConf.FTPUpdateDayOfWeek);
							DateTime creationDate = (envelope.CreationDate == null) ? DateTime.MinValue : (DateTime)envelope.CreationDate;
							if ((nextSynchroDay - creationDate).TotalDays < 7)
							{
								SOSLogWriter.WriteLogEntry(dmsInfo.Company, string.Format("Update SOSEnvelope with EnvelopeID = {0} has been skipped because its CreationDate is too recent", envelope.EnvelopeID.ToString()));
								continue;
							}
						}

						SOSLogWriter.WriteLogEntry(dmsInfo.Company, string.Format("SOSEnvelope with EnvelopeID = {0} has DispatchStatus = {1}", envelope.EnvelopeID.ToString(), envelope.DispatchStatus.ToString()));

						if (envelope.DispatchStatus == (int)StatoSpedizioneEnum.SPDSUBUNAUTH)	// il Soggetto in questione non è autorizzato a recuperare i dati
							continue;

						// questi stati di envelope sono tali per cui devo liberare i sosdoc correlati
						if (
							envelope.DispatchStatus == (int)StatoSpedizioneEnum.SPDCONUNEX	||	// il Conservatore non è definito a Sistema
							envelope.DispatchStatus == (int)StatoSpedizioneEnum.SPDCUSTUNEX	||	// il Cliente del Conservatore non è definito a Sistema
							envelope.DispatchStatus == (int)StatoSpedizioneEnum.SPDNEX		||	// la spedizione indicata non esiste
							envelope.DispatchStatus == (int)StatoSpedizioneEnum.SPDEXIST	||	// il file è una copia di un archivio già spedito
							envelope.DispatchStatus == (int)StatoSpedizioneEnum.EMPTY			// uso interno Microarea in caso di chiamata ws errata	
							)
						{
							// ma devo "sganciare" gli eventuali documenti ad essa correlati, in modo che possano essere rispediti con un'altra spedizione
							foreach (DMS_SOSDocument sosDoc in envelope.DMS_SOSDocuments)
							{
								sosDoc.DocumentStatus = (int)StatoDocumento.TORESEND; // assegno lo stato TORESEND
								// faccio la Clear dei vecchi riferimenti
								// sosDoc.LotID = string.Empty;
								// sosDoc.AbsoluteCode = string.Empty;
								// sosDoc.EnvelopeID = 0; // non imposto a zero l'EnvelopeID perche' rischio di avere dei SOSDoc orfani (vedi Erbolario con il down dei server Zucchetti)
								sosDoc.ArchivedDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
								dmsModel.SubmitChanges();

								SOSLogWriter.AppendText(dmsInfo.Company, string.Format("SOSDocument with AttachmentID = {0} SET DocumentStatus to RESEND.", sosDoc.AttachmentID));
							}

							continue;
						}

						// faccio una query secca per ottenere da parte il nome della classe documentale
						var collection = from c in dmsModel.DMS_Collections where c.CollectionID == envelope.CollectionID select c.SosDocClass;
						string docClassName = (collection != null) ? collection.Single() : string.Empty;

						// rimangono fuori questi stati: 
						// SPDOP: preso in carico
						// SPDNOP: non preso in carico, l'archivio dovrà essere rispedito (per aggiornare gli envelope/documenti che sono stati rispediti)
						// SPDALLOK: il contenuto della spedizione è stato acquisito
						// SPDALLKO: il contenuto della spedizione non è stato acquisito, deve essere rivisto e spedito nuovamente (dettaglio con esitospedizione)
						// SPDOKWERR: il contenuto della spedizione è stato acquisito parzialmente, alcuni doc presentano errori, è necessario correggere i doc errati e rispedirli (dettaglio con esitospedizione)
						// chiedo lo Stato della spedizione per aggiornare l'eventuale stato spedizione
						StatoSpedizione statoSped;
						if (!sosProxy.StatoSpedizione(envelope.DispatchCode, out statoSped))
						{
							SOSLogWriter.WriteLogEntry(dmsInfo.Company, string.Format("Error during '{0}' web-method. Please check MA Server in EventViewer", "StatoSpedizione"), "InternalUpdateSOSDocumentsStatus");
							continue;
						}

						SOSLogWriter.AppendText(dmsInfo.Company, string.Format("Calling {0} for Envelope with EnvelopeId = {1} (DispatchCode = {2}) and DocClass = {3})", "StatoSpedizione",
							envelope.EnvelopeID.ToString(), envelope.DispatchCode, docClassName));
	
						// vado ad aggiornare le righe nell'envelope, con l'ultimo stato ritornato
						envelope.DispatchStatus = (int)statoSped.StatoSpedizione;
						envelope.SynchronizedDate = DateTime.Now;
						dmsModel.SubmitChanges();

						SOSLogWriter.AppendText(dmsInfo.Company, string.Format("Updating DispatchStatus to {0} (= {1}) for SOSEnvelope with EnvelopeID = {2} ",
							statoSped.StatoSpedizione.ToString(), envelope.DispatchStatus.ToString(), envelope.EnvelopeID.ToString()));

						// aggiorno il LastSOSUpdateDateTime
						dmsInfo.LastSOSUpdateDateTime = DateTime.Now;

						// se la classe documentale e' vuota non procedo
						if (string.IsNullOrWhiteSpace(docClassName))
							continue;

						// se lo stato continua ad essere uguale a SPDOP/SPDOK (nel caso di frammentata corposa) 
						// significa che l'envelope e' ancora in attesa di essere lavorata
						// quindi non ha senso procedere alla chiamata dell'esito spedizione, visto che deve ancora essere presa in carico dal sistema
						if (envelope.DispatchStatus == (int)StatoSpedizioneEnum.SPDOP || envelope.DispatchStatus == (int)StatoSpedizioneEnum.SPDOK)
							continue;

						// prima di richiamare l'EsitoSped, calcolo il nr di sosdoc appartenenti all'Envelope che sto analizzando che hanno uno stato 
						// inferiore al DOCSIGN (ovvero lo stato che assumono dopo che il lotto e' stato chiuso)
						int numSOSDocToUpdate = envelope.DMS_SOSDocuments.Count(doc => doc.DocumentStatus < (int)StatoDocumento.DOCSIGN);
						if (numSOSDocToUpdate == 0)
						{
							SOSLogWriter.AppendText(dmsInfo.Company, string.Format("SOSEnvelope with EnvelopeID = {0} has no documents to update, because their status is >= DOCSIGN (= 9).", envelope.EnvelopeID.ToString()));
							continue;
						}

						SOSLogWriter.AppendText(dmsInfo.Company, string.Format("Calling {0} for Envelope with EnvelopeId = {1} (DispatchCode = {2}) and DocClass = {3})", "EsitoSpedizione",
							envelope.EnvelopeID.ToString(), envelope.DispatchCode, docClassName));

						EsitoSpedizione esitoSped;
						if (sosProxy.EsitoSpedizione(envelope.DispatchCode, docClassName, out esitoSped) && esitoSped != null)
						{
							// aggiorno lo stato della spedizione (cosi la volta successiva l'envelope viene scartata grazie all'if sopra)
							envelope.DispatchStatus = (int)esitoSped.StatoSpedizione;
							envelope.DispatchDate = esitoSped.DataPresaInCarico;
							dmsModel.SubmitChanges();

							SOSLogWriter.AppendText(dmsInfo.Company, string.Format("Updating DispatchStatus to {0} (= {1}) for SOSEnvelope with EnvelopeID = {2} ",
								esitoSped.StatoSpedizione.ToString(), envelope.DispatchStatus.ToString(), envelope.EnvelopeID.ToString()));


							foreach (DMS_SOSDocument sosDoc in envelope.DMS_SOSDocuments)
							{
								// Attenzione che l'esito torna i soli documenti che non sono stati scartati!
								Documento doc = esitoSped.GetDocumentByAttachmentId(sosDoc.AttachmentID);

								if (doc == null)
								{
									// ai documenti scartati (ovvero non ritornati) assegno lo stato TORESEND
									sosDoc.DocumentStatus = (int)StatoDocumento.TORESEND;
									// e faccio la Clear dei vecchi riferimenti
									// sosDoc.LotID = string.Empty;
									// sosDoc.AbsoluteCode = string.Empty;
									// sosDoc.EnvelopeID = 0; // non imposto a zero l'EnvelopeID perche' rischio di avere dei SOSDoc orfani (vedi Erbolario con il down dei server Zucchetti)
									sosDoc.ArchivedDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;

									SOSLogWriter.AppendText(dmsInfo.Company, string.Format("SOSDocument with AttachmentID = {0} does not exist in SOS. DocumentStatus set to RESEND.", sosDoc.AttachmentID));
								}
								else
								{
									sosDoc.AbsoluteCode = doc.CodiceAssoluto;
									sosDoc.ArchivedDate = doc.DataArchiviazioneDocumentale;
									sosDoc.DocumentStatus = (int)doc.StatoDocumento;
									sosDoc.LotID = doc.LottoId;
									sosDoc.RegistrationDate = doc.DataConservazioneSostitutiva;

									SOSLogWriter.AppendText(dmsInfo.Company, string.Format("SOSDocument with AttachmentID = {0} exists in SOS. DocumentStatus set to {1}. (AbsoluteCode = {2} - LotID = {3})",
															sosDoc.AttachmentID, doc.StatoDocumento.ToString(), doc.CodiceAssoluto, doc.LottoId));

									// se lo stato documento e' maggiore o uguale a DOCTEMP elimino il binario del PDF/A (cosi alleggerisco il database)
									if (doc.StatoDocumento >= StatoDocumento.DOCTEMP)
									{
										using (SqlConnection sqlConnection = new SqlConnection(dmsInfo.DMSConnectionString))
										{
											sqlConnection.Open();
											using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
											{
												sqlCommand.CommandText = string.Format
													(
														@"UPDATE [DMS_SOSDocument] SET [PdfABinary] = NULL WHERE [AttachmentID] = {0}",
														sosDoc.AttachmentID.ToString()
													);
												sqlCommand.ExecuteNonQuery();
												SOSLogWriter.AppendText(dmsInfo.Company, string.Format("Updating PdfABinary content in SOSDocument with attachmentId = {0}", sosDoc.AttachmentID.ToString()));
											}
										}
									}

									// se lo stato documento e' uguale a DOCSIGN significa 
									// che il doc e' stato inviato in conservazione e quindi non e' piu' modificabile
									// percio' vado ad aggiornare le colonne con i dati del SOS nella DMS_Attachment correlata
									// (avendo copiato i riferimenti a questo punto il sosdoc si potrebbe eliminare dal db)
									if (doc.StatoDocumento == StatoDocumento.DOCSIGN)
									{
										sosDoc.DMS_Attachment.AbsoluteCode = doc.CodiceAssoluto;
										sosDoc.DMS_Attachment.LotID = doc.LottoId;
										sosDoc.DMS_Attachment.RegistrationDate = doc.DataConservazioneSostitutiva;
									}
								}
								dmsModel.SubmitChanges();
							}
						}

						// aggiorno la data di sincronizzazione
						envelope.SynchronizedDate = DateTime.Now;
						dmsModel.SubmitChanges();

						// aggiorno il LastSOSUpdateDateTime
						dmsInfo.LastSOSUpdateDateTime = DateTime.Now;
					}
				}
				catch (Exception e)
				{
					Debug.Fail(e.ToString());
					WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, "EasyAttachmentSync:InternalUpdateSOSDocumentsStatus" +
						string.Format(" - Server '{0} Database '{1}' - ", dmsInfo.Server, dmsInfo.Database), e.ToString()));
					SOSLogWriter.WriteLogEntry(dmsInfo.Company, "InternalUpdateSOSDocumentsStatus ended with errors");
					return false;
				}
				finally
				{
					if (dmsModel != null)
						dmsModel.Dispose();
				}
			}

			SOSLogWriter.WriteLogEntry(dmsInfo.Company, "InternalUpdateSOSDocumentsStatus successfully completed");

			return true;
		}

		//----------------------------------------------------------------
		public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
		{
			// The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
			int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
			return start.AddDays(daysToAdd);
		}

		///<summary>
		/// Metodo generico per scrivere il messaggio nell'eventlog
		///</summary>
		//----------------------------------------------------------------
		internal static void WriteMessageInEventLog(string message, EventLogEntryType entryType = EventLogEntryType.Error, bool alwaysWrite = false)
		{
			// scrivo nell'EventLog se:
			// 1. ho forzato la scrittura, oppure
			// 2. ho abilitato il log verboso, oppure
			// 3. non ho abilitato il log verboso e ho un messaggio diverso da Information (quindi scrivo solo gli errori e i warning)
			if (
				alwaysWrite ||
				InstallationData.ServerConnectionInfo.EnableEAVerboseLog ||
				(!InstallationData.ServerConnectionInfo.EnableEAVerboseLog && entryType != EventLogEntryType.Information)
				)
			{
				string logEntry = string.Format("{0}: {1}", InstallationData.InstallationName, message);
				try
				{
					eventLog.WriteEntry(logEntry, entryType);
				}
				catch (Exception)
				{
					Debug.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToString(), logEntry));
				}
			}
		}
	}
}

//----------------------------------------------------------------
// PROVE DI APPLICAZIONE DEI TASK IN MULTI-THREADING
// al momento non in uso perche' rischiamo di avere piu' aziende collegate allo stesso database DMS
//----------------------------------------------------------------
/*
	List<Task> tasks = new List<Task>();

	// se la lista contiene almeno un elemento richiamo il processo di sincronizzazione per ogni database
	// al momento non creo un nuovo task perche' non vorrei che ci fossero piu' aziende collegate allo stesso database DMS
	foreach (string dmsConnString in dmsConnectionStringList)
	{
		if (string.IsNullOrWhiteSpace(dmsConnString))
			continue;

		Action<object> action = (conn) => { RunSynchronizeProcess((string)conn); };
		tasks.Add(Task.Factory.StartNew(action, dmsConnString));
	}

	// aspetto che tutti i task siano terminati
	Task.WaitAll(tasks.ToArray());

	for (int i = 0; i < tasks.Count; i++)
	{
		// qui si puo' testare se il singolo task e' fallito
		if (tasks[i].IsFaulted)
			eventLog.WriteEntry(string.Format("EASyncEngine:SynchronizeIndexes() error: {0}", e.ToString()), EventLogEntryType.Error);
	}
*/

