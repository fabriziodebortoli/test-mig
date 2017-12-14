using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SerializableTypes;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;
using Microarea.TaskBuilderNet.Data.SQLDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	/// <summary>
	/// Classe richiamabile esternamente che consente di configurare un'azienda di Mago,
	/// con relativo database, utenti e logins, senza alcuna interfaccia grafica.
	/// </summary>
	/// //========================================================================
	public class DatabaseEngine
	{
		// valori passati da fuori con le info di attivazione
		private IPathFinder pathFinder = null;
		private DBNetworkType dbNetworkType = DBNetworkType.Undefined;
		private BrandLoader brandLoader = null;
		private string isoCountryCode = string.Empty;
		private bool forProvisioningEnvironment = false; // se richiamato dal configuratore del sistema di provisioning
		//
		// credenziali di amministrazione del server, utente, password per connettersi al server
		private string inputServer = string.Empty;
		private string inputUser = string.Empty;
		private string inputPassword = string.Empty;
		
		private string inputConnectionString = string.Empty; // stringa di connessione al database master con le credenziali di amministrazione
		//
		private const string internationalSetDataCode = "INTL";
		private const string itCountryCode = "IT";

		// Utente applicativo di esempio da creare e relativa login in SQL
		private const string userAppUserName = "User";
		private string userAppUserPw = string.Empty; // pw iniziale vuota, intanto deve essere subito cambiata

		private string automaticSqlLoginName = string.Empty; // nome login di SQL generata in automatico
		private string automaticSqlLoginPassword = string.Empty; // password per la login di SQL generata in automatico
		//
		private Diagnostic dbEngineDiagnostic = new Diagnostic("DatabaseEngine");
		private TBConnection sysDBConnection = null;
		private DatabaseManager dbManager = null;
		private TransactSQLAccess connSqlTransact = new TransactSQLAccess();

		// variabili per tenere traccia degli inserimenti nel database di sistema
		private string adminId = string.Empty; // loginId inserito nella MSD_Logins
		private string userId = string.Empty; // loginId inserito nella MSD_Logins
		private string lastCompanyId = string.Empty; // companyId inserito nella MSD_Companies
		private ProviderItem sqlProviderItem = null; // informazioni del provider di SQL Server

		// gestione database culture
		private int companyLcid = 1040;
		private bool supportColsCollation = false;
		private string isoStateForSampleData = string.Empty;
		// 
		
		// gestione nomi database e company
		private string companyDbName = string.Empty;
		private string systemDbName = string.Empty;
		private string dmsDbName = string.Empty;
        private string companyName = string.Empty;
		private string sysDBAdminConnectionString = string.Empty;
        private bool loadSampleData = true; // dati di esempio
		private string sysDBProvisioningConnectionString = string.Empty;
		//

		// gestione logins (admin e user) per il sistema di provisioning
		private string adminLoginName = string.Empty;
		private string adminLoginPassword = string.Empty;
		private string userLoginName = string.Empty;
		private string userLoginPassword = string.Empty;
		//

		// variabili di appoggio
		private string myModule;
		private ModuleInfo myModuleInfo;
		private bool isFirstImportFile = true;
		private bool isFirstMandatoryColumn = true;

		//---------------------------------------------------------------------
		public event EventHandler<DBEngineEventArgs> GenerationEvent;
		public event EventHandler<DBEngineEventArgs> GenerationMainEvent;

		///<summary>
		/// Nome del database di sistema
		///</summary>
		//---------------------------------------------------------------------
		public string SystemDbName
		{
			get
			{
				// se non ho inizializzato da fuori il nome del database lo creo con il nome deciso da noi
				if (string.IsNullOrEmpty(systemDbName))
					systemDbName = DatabaseLayerConsts.MagoNetSystemDBName(brandLoader.GetBrandedStringBySourceString("DBPrefix")); 

				// se l'edizione e' Standard devo impostare il nome fisso StandardSystemDb
				if (string.Compare(pathFinder.Edition, NameSolverStrings.StandardEdition, StringComparison.InvariantCultureIgnoreCase) == 0)
					systemDbName = DatabaseLayerConsts.StandardSystemDb; 

				return systemDbName;
			}
			set { systemDbName = value; }
		}

		///<summary>
		/// Nome del database aziendale
		///</summary>
		//---------------------------------------------------------------------
		public string CompanyDbName
		{
			get
			{
				// se non ho inizializzato da fuori il nome del database lo creo con il nome deciso da noi
				if (string.IsNullOrEmpty(companyDbName))
                    companyDbName = DatabaseLayerConsts.MagoNetCompanyDBName(brandLoader.GetBrandedStringBySourceString("DBPrefix")); 
				return companyDbName;
			}
			set { companyDbName = value; }
		}
		
		///<summary>
		/// Nome del database del DMS
		///</summary>
		//---------------------------------------------------------------------
		public string DMSDbName
		{
			get
			{
				// se non ho inizializzato da fuori il nome del database lo creo con il nome deciso da noi
				if (string.IsNullOrEmpty(dmsDbName))
					dmsDbName = DatabaseLayerConsts.MagoNetDMSDBName(brandLoader.GetBrandedStringBySourceString("DBPrefix"));
				return dmsDbName;
			}
			set { dmsDbName = value; }
		}

		///<summary>
		/// Nome dell'azienda
		///</summary>
		//---------------------------------------------------------------------
		public string CompanyName
		{
			get
			{
				// se non ho inizializzato da fuori il nome del database lo creo con il nome deciso da noi
				if (string.IsNullOrEmpty(companyName))
					CompanyName = DatabaseLayerConsts.MagoNetCompanyName(brandLoader.GetBrandedStringBySourceString("DBPrefix"));
                return companyName;
			}
			set { companyName = value; }
		}
       

        //---------------------------------------------------------------------
        public bool LoadSampleData { get { return loadSampleData; } set { loadSampleData = value; } }

		// credenziali di amministrazione per la connessione a SQL Server
		//---------------------------------------------------------------------
		public string Server { get { return inputServer; } set { inputServer = value; } }
		public string User { get { return inputUser; } set { inputUser = value; } }
		public string Password { get { return inputPassword; } set { inputPassword = value; } }

		// login e utenti da creare per il servizio di provisioning
		//---------------------------------------------------------------------
		public string AdminLoginName { get { return adminLoginName; } set { adminLoginName = value; } }
		public string AdminLoginPassword { get { return adminLoginPassword; } set { adminLoginPassword = value; } }
		public string UserLoginName { get { return userLoginName; } set { userLoginName = value; } }
		public string UserLoginPassword { get { return userLoginPassword; } set { userLoginPassword = value; } }

		public Diagnostic DbEngineDiagnostic { get { return dbEngineDiagnostic; } }

		///<summary>
		/// Costruttore
		/// Il parametro companyLcid se diverso da -1 significa che la classe e' richiamata da EasyBuilder
		///</summary>
		//---------------------------------------------------------------------
		public DatabaseEngine(IPathFinder pf, DBNetworkType networkType, string isoCountry, BrandLoader bLoader, int companyLcid = -1, bool forProvisioningEnv = false)
		{
			pathFinder = pf;
			dbNetworkType = networkType;
			isoCountryCode = isoCountry;
			brandLoader = bLoader;
            
			// se l'isostato e' Italia allora considero IT, altrimenti carico i dati INTL
			// (i sample data infatti esistono solo per IT e INTL)
			isoStateForSampleData = (string.Compare(isoCountryCode, itCountryCode, StringComparison.InvariantCultureIgnoreCase) == 0) 
									? itCountryCode 
									: internationalSetDataCode;

			// deduco la database culture da impostare per il database aziendale
			if (companyLcid == -1)
				PurposeDatabaseCultureForCompanyDb();
		}

		///<summary>
		/// Entry-point per l'esecuzione in cascata delle varie operazioni...
		/// FLUSSO COMPLETO:
		/// 1. con le credenziali di amministrazione mi connetto al master e inserisco MagoNetUserAuto01 e pw reverse stringa+.
		/// se esiste gia' ne creo una nuova e aggiungo 02 
		/// 2. con le credenziali di amministrazione creo il database di sistema e la sua struttura dati
		/// 3. salvo la stringa di connessione al database di sistema da salvare nel ServerConnection.config:
		///    - con le credenziali di amministrazione fornite (sa o simili)
		/// 4. creo due utenti applicativi: uno e' l'amministratore fornito all'inizio e l'altro e' User.
		/// 5. creo i providers
		/// 6. creo l'azienda (il dbowner e' l'amministratore)
		/// 7. associo gli utenti all'azienda:
		///    - l'amministratore diventa dbowner e associato 1 a 1
		///    - User associato a MagoNetUserAuto01 (o 02 etc.)
		/// 8. creo il database aziendale e la sua struttura dati + importo i dati di esempio
		/// 9. lancio Mago.Net con Admin
		///	P.S. volendo aggiornare un database aziendale al volo, devo controllare che nessun altro utente sia collegato all'azienda.
		///</summary>
		//---------------------------------------------------------------------
		public bool Execute()
		{
			ComposeConnectionStrings();

			bool result = false;

			//------------------------------------------------------------
			// creo la login di esempio sul server SQL
			//------------------------------------------------------------
			result = CreateMagoNetUserLogin();
			if (!result)
				return false;

			//------------------------------------------------------------
			// creo il contenitore del database di sistema
			//------------------------------------------------------------
			OnDBEngineMainEvent(EventType.Info, MainEventType.CreateSystemDB);
			result = CreateDatabaseContainer(KindOfDatabase.System);
			OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateSystemDB);
			if (!result)
				return false;

			//------------------------------------------------------------
			// creo le tabelle nel database di sistema
			//------------------------------------------------------------
			OnDBEngineMainEvent(EventType.Info, MainEventType.CreateSystemDBStructure);
			result = CreateUpgradeSystemDatabase();
			OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateSystemDBStructure);
			if (!result)
				return false;

			//------------------------------------------------------------
			// salvo il ServerConnection.config, inserisco le informazioni nel db di sistema (application users, company, providers)
			//------------------------------------------------------------
			OnDBEngineMainEvent(EventType.Info, MainEventType.CreateUserAndCompany);

			SaveServerConnectionConfigFile();
			
			result = ConfigureInfoInSystemDB();
			
			OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateUserAndCompany);
			if (!result)
				return false;

			//------------------------------------------------------------
			// creo il contenitore del database aziendale
			//------------------------------------------------------------
			OnDBEngineMainEvent(EventType.Info, MainEventType.CreateCompanyDB);
			result = CreateDatabaseContainer(KindOfDatabase.Company);
			OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateCompanyDB);
			if (!result)
				return false;

			//------------------------------------------------------------
			// creo le tabelle nel database aziendale
			//------------------------------------------------------------
			OnDBEngineMainEvent(EventType.Info, MainEventType.CreateCompanyDBStructure);
			result = CreateUpgradeCompanyDatabase();
			OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateCompanyDBStructure);

			//------------------------------------------------------------
			// associo la login generata automaticamente al database aziendale appena creato
			//------------------------------------------------------------
			GrantLogin();

			//------------------------------------------------------------
			// creo il file LoggedUser.xml, cosi' da proporre subito da MenuManager l'utente Admin per la connessione
			//------------------------------------------------------------
			CreateLoggedUserFile(inputUser);

			OnDBEngineEvent(EventType.Success, DatabaseLayerStrings.ElaborationCompleted);

			return result;
		}

		///<summary>
		/// Entry-point per l'esecuzione in cascata delle varie operazioni per configurare l'ambiente di provisioning
		/// 1. creo le logins in SQL Server
		/// 2. creo il database di sistema e relative tabelle
		/// 3. salvo le info nel ServerConnection.config
		/// 4. configuro nel database di sistema le info relative ai provider, agli utenti applicativi e all'azienda
		/// 5. creo solo il contenitore del database aziendale
		/// 6. creo solo il contenitore del database DMS
		/// 7. assegno le logins con i ruoli necessari ai database (devo farlo prima senno' non riesco a connettermi perche' nel db di sistema ho gli altri utenti)
		/// 8. creo le tabelle del database aziendale
		/// (se dovessi creare anche le tabelle dovrei popolare le tabelle con i dati slave e poi eliminare tutto alla fine perche' qui non so se il DMS e' attivato)
		///</summary>
		//---------------------------------------------------------------------
		public bool ConfigureProvisioningEnvironment(bool lite = false)
		{
			ComposeConnectionStrings();

			bool result = false;

			if (!lite)
			{
				//------------------------------------------------------------
				// creo la login sul server SQL
				//------------------------------------------------------------
				result = CreateLoginsForProvisioningEnv();
				if (!result)
					return false;

				//------------------------------------------------------------
				// creo il contenitore del database di sistema
				//------------------------------------------------------------
				OnDBEngineMainEvent(EventType.Info, MainEventType.CreateSystemDB);
				result = CreateDatabaseContainer(KindOfDatabase.System);
				OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateSystemDB);
				if (!result)
					return false;
			}

			//------------------------------------------------------------
			// creo le tabelle nel database di sistema
			//------------------------------------------------------------
			OnDBEngineMainEvent(EventType.Info, MainEventType.CreateSystemDBStructure);
			result = CreateUpgradeSystemDatabase();
			OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateSystemDBStructure);
			if (!result)
				return false;

			//------------------------------------------------------------
			// salvo il ServerConnection.config, inserisco le informazioni nel db di sistema (application users, company, providers)
			//------------------------------------------------------------
			OnDBEngineMainEvent(EventType.Info, MainEventType.CreateUserAndCompany);

			SaveServerConnectionConfigFile();

			result = ConfigureSystemDBForProvisioningEnv(lite: lite);

			OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateUserAndCompany);
			if (!result)
				return false;

			if (!lite)
			{
				//------------------------------------------------------------
				// creo il contenitore del database aziendale
				//------------------------------------------------------------
				OnDBEngineMainEvent(EventType.Info, MainEventType.CreateCompanyDB);
				result = CreateDatabaseContainer(KindOfDatabase.Company);
				OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateCompanyDB);
				if (!result)
					return false;

				//------------------------------------------------------------
				// creo il contenitore del database del DMS
				//------------------------------------------------------------
				OnDBEngineMainEvent(EventType.Info, MainEventType.CreateDMSDB);
				result = CreateDatabaseContainer(KindOfDatabase.Dms);
				OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateDMSDB);
				if (!result)
					return false;

				//------------------------------------------------------------
				// associo le logins ai database appena creati con gli opportuni ruoli
				// lo devo fare prima della creazione delle tabelle del db aziendale perche' utilizzando le credenziali
				// salvate nel db di sistema non riesco a connettermi
				//------------------------------------------------------------
				result = GrantLoginsForProvisioningEnv();
				if (!result)
					return false;
			}

			//------------------------------------------------------------
			// creo le tabelle nel database aziendale alla fine
			//------------------------------------------------------------
			OnDBEngineMainEvent(EventType.Info, MainEventType.CreateCompanyDBStructure);
			result = CreateUpgradeCompanyDatabase();
			OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateCompanyDBStructure);

			OnDBEngineEvent(EventType.Success, DatabaseLayerStrings.ElaborationCompleted);

			return result;
		}

		//---------------------------------------------------------------------
		public bool ConfigureProvisioningEnvironmentLITE()
		{
			return ConfigureProvisioningEnvironment(true);
		}

		///<summary>
		/// ComposeConnectionStrings
		/// Compongo le stringhe di connessione al database di sistema e al master, visto che poi le utilizzero' 
		/// in piu' punti (la connessione avviene sempre in SQL Authentication)
		///</summary>
		//---------------------------------------------------------------------
		private void ComposeConnectionStrings()
		{
			// compongo la stringa di connessione database master del server con le credenziali di amministrazione
			inputConnectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, inputServer, DatabaseLayerConsts.MasterDatabase, inputUser, inputPassword);

			// compongo la stringa di connessione al database di sistema con le credenziali di amministrazione
			sysDBAdminConnectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, inputServer, SystemDbName, inputUser, inputPassword);

			// compongo la stringa di connessione al database di sistema con le credenziali dell'utente candidato ad essere amministratore
			// che andro' poi a salvare nel ServerConnection.config in modo da proporlo 
			if (forProvisioningEnvironment)
				sysDBProvisioningConnectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, inputServer, SystemDbName, adminLoginName, adminLoginPassword);
		}

		///<summary>
		/// Creazione contenitore database su SQL Server
		///</summary>
		///<param name="kindOfDb">tipo database da creare</param>
		//---------------------------------------------------------------------
		private bool CreateDatabaseContainer(KindOfDatabase kindOfDb)
		{
			string databaseName = string.Empty;

			switch (kindOfDb)
			{ 
				case KindOfDatabase.System:
					databaseName = SystemDbName;
					break;
				case KindOfDatabase.Company:
					databaseName = CompanyDbName;
					break;
				case KindOfDatabase.Dms:
					databaseName = DMSDbName;
					break;
			}

			OnDBEngineEvent(EventType.Success, string.Format(DatabaseLayerStrings.StartCreateDB, databaseName));

			bool result = false;

			DatabaseTask dbTask = new DatabaseTask();
			dbTask.CurrentStringConnection = inputConnectionString;

			SQLCreateDBParameters createParams = new SQLCreateDBParameters();
			createParams.DatabaseName = databaseName;

			try
			{
				result = dbTask.CreateSQLDatabase(createParams);
			}
			catch (TBException e)
			{
				dbEngineDiagnostic.SetError(e.Message);
				OnDBEngineEvent(EventType.Error, string.Format(DatabaseLayerStrings.ErrorDuringDBCreation, databaseName) + "\r\n" + e.Message);
				return result;
			}

			// se si e' verificato un errore in creazione database mostro i vari errori
			if (!result && dbTask.ErrorsList != null && dbTask.ErrorsList.Count > 0)
			{
				dbEngineDiagnostic.Set(dbTask.Diagnostic);
				foreach (string error in dbTask.ErrorsList)
					OnDBEngineEvent(EventType.Error, error);
			}

			OnDBEngineEvent
				(
				result ? EventType.Success : EventType.Error,
				string.Format(DatabaseLayerStrings.EndCreateDB, databaseName)
				);

			return result;
		}

		///<summary>
		/// Creazione/aggiornamento database di sistema
		///</summary>
		//---------------------------------------------------------------------
		private bool CreateUpgradeSystemDatabase()
		{
			bool result = false;

			// creazione tabelle per il database sistema
			dbManager = new DatabaseManager
				(
				(PathFinder)pathFinder,
				sysDBAdminConnectionString,
                DBMSType.SQLSERVER,
				IsoHelper.IsoExpectUnicodeCharSet(isoCountryCode),
				dbEngineDiagnostic,
				brandLoader,
				dbNetworkType,
				isoCountryCode 
				);

			if (!dbManager.Valid)
				return false;

			result = dbManager.CheckDBStructure(KindOfDatabase.System) && dbManager.DatabaseManagement();

			return result;
		}

		///<summary>
		/// Creazione/aggiornamento database aziendale
		/// Il parametro companyId se diverso da null significa che la classe e' richiamata da EasyBuilder
		///</summary>
		//---------------------------------------------------------------------
		public bool CreateUpgradeCompanyDatabase(string companyId = null)
		{
			bool result = false;
			
			//se non ho una connection string valida, prendo quella del server connection info
			if (string.IsNullOrEmpty(sysDBAdminConnectionString))
				sysDBAdminConnectionString = InstallationData.ServerConnectionInfo.SysDBConnectionString;
			
			if (!string.IsNullOrEmpty(companyId))
				lastCompanyId = companyId;

			// data la stringa di connessione estrapolo le informazioni per inizializzare la struttura del ContextInfo
			SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder(sysDBAdminConnectionString);

			ContextInfo.SystemDBConnectionInfo cp = new ContextInfo.SystemDBConnectionInfo();
			string serverName = string.Empty;
			string instanceName = string.Empty;

			// estrapolo il nome dell'istanza dal nome del server
			int pos = scsb.DataSource.IndexOf(Path.DirectorySeparatorChar);
			serverName = (pos > 0) ? scsb.DataSource.Substring(0, pos) : scsb.DataSource;
			instanceName = (pos > 0) ? scsb.DataSource.Substring(pos + 1) : string.Empty;

			cp.DBName = scsb.InitialCatalog;
			cp.ServerName = serverName;
			cp.Instance = instanceName;
			cp.UserId = scsb.UserID;
			cp.Password = scsb.Password;

			// creazione tabelle per il database aziendale
			dbManager = new DatabaseManager
				(
				(PathFinder)pathFinder,
				dbEngineDiagnostic,
				brandLoader,
				cp,
				dbNetworkType,
				isoCountryCode,
				false // no ask credential
				);

			dbManager.OnUpdateModuleCounter += new DatabaseManager.UpdateModuleCounter(PostToModuleCounter);
			dbManager.OnUpdateMandatoryCols += new DatabaseManager.UpdateMandatoryCols(PostToTableAndMandatoryCols);
			dbManager.OnElaborationProgressMessage += new DatabaseManager.ElaborationProgressMessage(PostElaborationProgressMessage);

			// mi connetto alla company appena creata (con l'Id appena inserito nella MSD_Companies)
			if (dbManager.ConnectAndCheckDBStructure(lastCompanyId))
			{
				// se sto configurando il sistema di provisioning NON carico ne' dati default ne' di esempio
				if (forProvisioningEnvironment)
					dbManager.ImportDefaultData = dbManager.ImportSampleData = false;
				else
				{
					// importazione dati di default 
					dbManager.ImportDefaultData = !loadSampleData;
					// importazione i dati di esempio (impostato d'ufficio a true)
					dbManager.ImportSampleData = loadSampleData;

					// imposto le configurazioni per i dati di esempio e i relativi eventi
					if (dbManager.ImpExpManager != null)
					{
						dbManager.ImpExpManager.DBDiagnostic.OnUpdateImportFileCounter += new DatabaseDiagnostic.UpdateImportFileCounter(PostToModuleCounterForSample);

						// se l'edizione e' Enterprise carico la configurazione ManufacturingAdvanced
						if (string.Compare(pathFinder.Edition, NameSolverStrings.EnterpriseEdition, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							if (dbManager.ImportDefaultData)
								dbManager.ImpExpManager.SetDefaultDataConfiguration(DataManagerConsts.ManufacturingAdvanced);
							else
								dbManager.ImpExpManager.SetSampleDataConfiguration(DataManagerConsts.ManufacturingAdvanced, isoStateForSampleData);
						}
						else
						{
							// se l'edizione e' Standard o Professional carico la configurazione Basic
							if (dbManager.ImportDefaultData)
								dbManager.ImpExpManager.SetDefaultDataConfiguration(DataManagerConsts.Basic);
							else
								dbManager.ImpExpManager.SetSampleDataConfiguration(DataManagerConsts.Basic, isoStateForSampleData);
						}
					}
				}

				result = dbManager.DatabaseManagement(false) && !dbManager.ErrorInRunSqlScript; // passo il parametro cosi' salvo il log
				if (!forProvisioningEnvironment)
					OnDBEngineMainEvent(EventType.Success, MainEventType.ImportSampleData);
				else
					OnDBEngineMainEvent(EventType.Success, MainEventType.MandatoryColumnsCreation);
			}

			return result;
		}

		///<summary>
		/// Salvataggio informazioni nel file ServerConnection.config
		///</summary>
		//---------------------------------------------------------------------
		private void SaveServerConnectionConfigFile()
		{
			// scrivo la stringa di connessione al database di sistema
			InstallationData.ServerConnectionInfo.SysDBConnectionString = (forProvisioningEnvironment) ? sysDBProvisioningConnectionString : sysDBAdminConnectionString;

			// fix anomalia 18985: non devo sovrascrivere le due languages, che sono state gia' inserite dal setup
			//InstallationData.ServerConnectionInfo.PreferredLanguage = initializeDBCulture;
			//InstallationData.ServerConnectionInfo.ApplicationLanguage = initializeDBCulture;

			InstallationData.ServerConnectionInfo.MinPasswordLength = Convert.ToInt32(0);
			InstallationData.ServerConnectionInfo.WebServicesTimeOut = ServerConnectionInfo.DefaultWebServicesTimeOut;
			// salvo il contenuto del file
			InstallationData.ServerConnectionInfo.UnParse(this.pathFinder.ServerConnectionFile);
		}

		///<summary>
		/// CreateMagoNetUserLogin
		/// Con le credenziali di amministrazione mi connetto al database master e creo
		/// la login di esempio in SQL Server. Il prefisso e' MagoNetUserAuto, seguito da un contatore numerico
		/// La password e' complessa, facciamo il reverse della stringa e aggiungiamo un punto in fondo.
		///</summary>
		//---------------------------------------------------------------------
		private bool CreateMagoNetUserLogin()
		{
			bool result = false;

			try
			{
				connSqlTransact.CurrentStringConnection = inputConnectionString;

				for (int i = 1; i < 10000; i++)
				{
					// compongo il nome della login con in coda il nome del contatore
                    string userlogin = string.Concat( brandLoader.GetBrandedStringBySourceString("DBPrefix"),DatabaseLayerConsts.SqlLoginPrefix);
                    automaticSqlLoginName = string.Concat(userlogin, i.ToString());

					// se la login esiste gia' sul server procedo a generarne un'altra
					if (!connSqlTransact.ExistLogin(automaticSqlLoginName))
					{
						// la password e' generata automaticamente facendo il reverse della login e aggiungendo un punto in fondo
						automaticSqlLoginPassword = string.Concat(Reverse(automaticSqlLoginName), ".");
						// aggiungo la login al Server SQL
						connSqlTransact.SPAddLogin(automaticSqlLoginName, automaticSqlLoginPassword, DatabaseLayerConsts.MasterDatabase);
						result = true;
						break;
					}
				}
			}
			catch(TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "CreateMagoNetUserLogin");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, string.Format(DatabaseLayerStrings.ErrAddLogin, automaticSqlLoginName), ei);
				return result;
			}

			if (!result && connSqlTransact.Diagnostic.Error)
				dbEngineDiagnostic.Set(connSqlTransact.Diagnostic);

			return result;
		}

		//---------------------------------------------------------------------
		private string Reverse(string stringToReverse)
		{
			char[] charArray = stringToReverse.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}

		/// <summary>
		/// Creo se necessario le login sul server SQL
		/// </summary>
		//---------------------------------------------------------------------
		private bool CreateLoginsForProvisioningEnv()
		{
			bool result = false;

			try
			{
				connSqlTransact.CurrentStringConnection = inputConnectionString;

				// se la login esiste gia' non faccio niente, tanto ho gia' controllato a monte che sia valida la pw
				if (!connSqlTransact.ExistLogin(adminLoginName))
					result = connSqlTransact.SPAddLogin(adminLoginName, adminLoginPassword, DatabaseLayerConsts.MasterDatabase);
				else
					result = true;

				if (result)
				{
					// se la login esiste gia' non faccio niente, tanto ho gia' controllato a monte che sia valida la pw
					if (!connSqlTransact.ExistLogin(userLoginName))
						result = connSqlTransact.SPAddLogin(userLoginName, userLoginPassword, DatabaseLayerConsts.MasterDatabase);
				}
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "CreateLoginsForProvisioningEnv");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrAddLoginsForProvisioning, ei);
				return result;
			}

			if (!result && connSqlTransact.Diagnostic.Error)
				dbEngineDiagnostic.Set(connSqlTransact.Diagnostic);

			return result;
		}

		///<summary>
		/// Configura le informazioni predefinite nel database di sistema:
		/// 1. inserisco i provider per SQL e Oracle e si tiene da parti le info di SQL
		/// 2. creo i due utenti applicativi di prova (Admin e User) (uno amministratore e uno no), 
		/// impostando la lingua preferenziale e forzando il cambio password alla prima login (userMustChangePassword = true)
		/// 3. inserisco l'azienda nella tabella MSD_Companies
		/// 4. associare gli utenti applicativi appena creati all'azienda
		///</summary>
		//---------------------------------------------------------------------
		private bool ConfigureInfoInSystemDB()
		{
			bool result = false;

			sysDBConnection = new TBConnection(sysDBAdminConnectionString, DBMSType.SQLSERVER);

			try
			{
				// apro al volo una connessione al database di sistema in modo di inserire i dati che mi servono
				sysDBConnection.Open();
				
				result = CreateProviders() && CreateApplicationUsers() && CreateCompany();
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "ConfigureInfoInSystemDB");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrSysDBConfiguration, ei);
				return false;
			}
			finally
			{
				// chiudo la connessione
				if (sysDBConnection != null && sysDBConnection.State != ConnectionState.Closed)
				{
					sysDBConnection.Close();
					sysDBConnection.Dispose();
				}
			}

			return result;
		}

		///<summary>
		/// CreateApplicationUsers
		/// Creazione utenti applicativi di esempio (Admin e User)
		/// </summary>
		//---------------------------------------------------------------------
		private bool CreateApplicationUsers()
		{
			try
			{
				UserDb userdb = new UserDb();
				userdb.ConnectionString = sysDBAdminConnectionString;
				userdb.CurrentSqlConnection = (SqlConnection)sysDBConnection.DbConnect;

				//Se l'utente amministratore non è presente nel database MSD_Logins lo aggiungo
				if (!userdb.ExistLoginAlsoDisabled(inputUser))
					userdb.Add
						(
						false, /*windowsAuthentication*/
						inputUser, /*login*/
						inputPassword, /*password*/
						inputUser, /*description*/
						DateTime.Today.ToShortDateString(), /*expirationDate*/
						false, /*disabled*/
						false /*userMustChangePassword*/, 
						false, /*userCannotChangePassword*/
						false, /*expiredDateCannotChange*/
						true, /*passwordNeverExpired*/
						string.Empty /*preferredLanguage*/,
						string.Empty /*applicationLanguage*/, 
						string.Empty, false, true, false, false, false, "0");

				if (userdb.Diagnostic.Error)
					dbEngineDiagnostic.Set(userdb.Diagnostic);

				// mi salvo l'Id della login Admin appena inserita
				adminId = userdb.LastLoginId().ToString();

				//Se l'utente User non è presente nel database MSD_Logins lo aggiungo
				if (!userdb.ExistLoginAlsoDisabled(userAppUserName))
					userdb.Add
						(
						false, /*windowsAuthentication*/
						userAppUserName, /*login*/
						userAppUserPw, /*password*/
						userAppUserName, /*description*/
						DateTime.Today.ToShortDateString(), /*expirationDate*/
						false, /*disabled*/
						true /*userMustChangePassword*/,
						false, /*userCannotChangePassword*/
						false, /*expiredDateCannotChange*/
						false, /*passwordNeverExpired*/
						string.Empty /*preferredLanguage*/,
						string.Empty /*applicationLanguage*/,
                        string.Empty, false, true, false, false, false, "0");

				if (userdb.Diagnostic.Error)
					dbEngineDiagnostic.Set(userdb.Diagnostic);

				// mi salvo l'Id della login User appena inserita
				userId = userdb.LastLoginId().ToString();
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "CreateApplicationUsers");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrApplicationUsersCreation, ei);
				return false;
			}
			
			return !dbEngineDiagnostic.Error;
		}

		///<summary>
		/// Inserisco i providers nella tabella MSD_Providers (se non esistono) e 
		/// tengo da parte le informazioni per il provider di SQL Server
		///</summary>
		//---------------------------------------------------------------------
		private bool CreateProviders()
		{
			try
			{
				// Inserisco nella tabella MSD_Providers i vari providers di database
				ProviderDb providerDb = new ProviderDb();
				providerDb.ConnectionString = sysDBAdminConnectionString;
				providerDb.CurrentSqlConnection = (SqlConnection)sysDBConnection.DbConnect;

				//inserisco i provider MS Sql e Oracle
                if (!providerDb.ExistProvider(NameSolverDatabaseStrings.SQLOLEDBProvider))
					providerDb.Add(DBMSType.SQLSERVER);
				if (!providerDb.ExistProvider(NameSolverDatabaseStrings.OraOLEDBProvider))
					providerDb.Add(DBMSType.ORACLE);

				// memorizzo le info del provider sql
				sqlProviderItem = providerDb.SelectSqlProvider();

				if (providerDb.Diagnostic.Error)
					dbEngineDiagnostic.Set(providerDb.Diagnostic);
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "CreateProviders");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrProvidersCreation, ei);
				return false;
			}

			return !dbEngineDiagnostic.Error;
		}

		///<summary>
		/// GrantLogin
		/// Effettuo il grant della login generata automaticamente al database aziendale
		///</summary>
		//---------------------------------------------------------------------
		private bool GrantLogin()
		{
			bool result = false;

			try
			{
				string companyDBconnectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, inputServer, CompanyDbName, inputUser, inputPassword);
				connSqlTransact.CurrentStringConnection = companyDBconnectionString;

				// aggiungo la login generata automaticamente su SQL al database aziendale
				result = (connSqlTransact.SPGrantDbAccess(automaticSqlLoginName, automaticSqlLoginName, CompanyDbName) &&
						connSqlTransact.SPAddRoleMember(automaticSqlLoginName, DatabaseLayerConsts.RoleDataWriter, CompanyDbName) &&
						connSqlTransact.SPAddRoleMember(automaticSqlLoginName, DatabaseLayerConsts.RoleDataReader, CompanyDbName) &&
						connSqlTransact.SPAddRoleMember(automaticSqlLoginName, DatabaseLayerConsts.RoleDbOwner, CompanyDbName));

				if (!result)
					dbEngineDiagnostic.Set(connSqlTransact.Diagnostic);
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "GrantLogin");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, string.Format(DatabaseLayerStrings.ErrGrantLoginOnDB, automaticSqlLoginName, CompanyDbName), ei);
				return false;
			}

			return result;
		}

		///<summary>
		/// CreateCompany
		/// Creazione azienda e relativa associazione utenti applicativi
		/// </summary>
		//---------------------------------------------------------------------
		private bool CreateCompany()
		{
			try
			{
				// aggiungo la nuova azienda
				CompanyDb companyDb = new CompanyDb();
				companyDb.ConnectionString = sysDBAdminConnectionString;
				companyDb.CurrentSqlConnection = (SqlConnection)sysDBConnection.DbConnect;

				companyDb.Add
					(
					CompanyName,  //DatabaseLayerConsts.MagoNetCompanyName, //company (nome azienda),
					DatabaseLayerStrings.MagoNetSampleCompany,	//description, 
					sqlProviderItem.ProviderId,	//providerId, (metto il provider SQL)
					inputServer,		   		//companyDbServer, 
					CompanyDbName, 				//companyDbName, 
					string.Empty,   			//defaultUser,
					string.Empty,   			//defaultPassword, 
					adminId,					//companyDbOwner, il dbo e' l'utente amministratore
					false,		   				//security,
					false,		   				//activity, 
					true,		   				//useTransaction, 
					true,		   				//useKeyedUpdate,
					false,		   				//companyDbWindowsAuthentication,
					string.Empty,   			//preferredLanguage,
					string.Empty,   			//applicationLanguage,
					false,		   				//disabled,
					IsoHelper.IsoExpectUnicodeCharSet(isoCountryCode), //useUnicode,
					true,		   				//isValid,
					companyLcid,   				//dbCultureLCID,
					supportColsCollation,		//supportColsCollation,
					0,							//port
					false,						//useDBSlave
					false,                      //useRowSecurity
					false						//useDataSynchronizer
					);

				if (companyDb.Diagnostic.Error)
					dbEngineDiagnostic.Set(companyDb.Diagnostic);

				// salvo l'Id dell'azienda appena generato
				lastCompanyId = companyDb.LastCompanyId().ToString();

				// associo le login di amministrazione e User all'azienda appena creata
				CompanyUserDb companyUserDb = new CompanyUserDb();
				companyUserDb.ConnectionString = sysDBAdminConnectionString;
				companyUserDb.CurrentSqlConnection = (SqlConnection)sysDBConnection.DbConnect;

				// l'utente dbo e' associato 1 a 1 alla login con le credenziali di amministrazione
				companyUserDb.Add(lastCompanyId, adminId, true, false, inputUser, inputPassword, false);
				// l'utente User e' associato alla login generata automaticamente
				companyUserDb.Add(lastCompanyId, userId, false, false, automaticSqlLoginName, automaticSqlLoginPassword, false);

				if (companyUserDb.Diagnostic.Error)
					dbEngineDiagnostic.Set(companyUserDb.Diagnostic);
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "CreateCompany");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrCompanyCreation, ei);
			}

			return true;
		}
		
		///<summary>
		/// PurposeDatabaseCultureForCompanyDb
		/// Sulla base del country code di installazione estrapolo la cultura di database da impostare
		/// sull'azienda (e pilotare l'applicazione della collate sulle colonne di database)
		///</summary>
		//---------------------------------------------------------------------
		private void PurposeDatabaseCultureForCompanyDb()
		{
			// sulla base del countrycode di installazione estrapolo una cultura per il database
			// okkio che qui va a leggere le informazioni nel ServerConnection.config, facendo la scaletta
			// tra ApplicationLanguage, PreferredLanguage, e generica lingua
			string initializeDBCulture = DBGenericFunctions.PurposeDatabaseCultureForDBCreation(isoCountryCode);

			// nel caso in cui la cultura sia vuota metto en (non dovrebbe mai accadere)
			if (string.IsNullOrEmpty(initializeDBCulture))
				initializeDBCulture = "en";

			// mi serve per assegnare il valore dell'LCID nell'anagrafica azienda
			// comanda sempre quella impostata sul database
			CultureInfo ci = new CultureInfo(initializeDBCulture);
			companyLcid = ci.LCID;

			supportColsCollation =
				(
				(string.Compare
				(CultureHelper.GetWindowsCollation(ci.LCID),
				NameSolverDatabaseStrings.SQLLatinCollation,
				StringComparison.InvariantCultureIgnoreCase) != 0)
				&&
				!CultureHelper.IsCollationCompatibleWithCulture(companyLcid, NameSolverDatabaseStrings.SQLLatinCollation)
				);
		}
		
		///<summary>
		/// Configura le informazioni predefinite nel database di sistema:
		/// 1. inserisco i provider per SQL e Oracle e si tiene da parti le info di SQL
		/// 2. creo i due utenti applicativi di prova (Admin e User) (uno amministratore e uno no), 
		/// impostando la lingua preferenziale e forzando il cambio password alla prima login (userMustChangePassword = true)
		/// 3. inserisco l'azienda nella tabella MSD_Companies
		/// 4. associare gli utenti applicativi appena creati all'azienda
		///</summary>
		//---------------------------------------------------------------------
		private bool ConfigureSystemDBForProvisioningEnv(bool addCompanyMode = false, bool lite = false)
		{
			bool result = false;

			sysDBConnection = new TBConnection(sysDBAdminConnectionString, DBMSType.SQLSERVER);

			try
			{
				// apro al volo una connessione al database di sistema in modo di inserire i dati che mi servono
				sysDBConnection.Open();

				result = CreateProviders() &&
						(addCompanyMode ? GetApplicationUsersInfoForProvisioningEnvInAddCompanyMode() : CreateApplicationUsersForProvisioningEnv(lite)) &&
						CreateCompanyForProvisioningEnv();
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "ConfigureSystemDBForProvisioningEnv");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrSysDBConfiguration, ei);
				return false;
			}
			finally
			{
				// chiudo la connessione
				if (sysDBConnection != null && sysDBConnection.State != ConnectionState.Closed)
				{
					sysDBConnection.Close();
					sysDBConnection.Dispose();
				}
			}

			return result;
		}

		///<summary>
		/// Creazione utenti applicativi predefiniti dal sistema di provisioning
		/// </summary>
		//---------------------------------------------------------------------
		private bool CreateApplicationUsersForProvisioningEnv(bool lite)
		{
			try
			{
				UserDb userdb = new UserDb();
				userdb.ConnectionString = sysDBAdminConnectionString;
				userdb.CurrentSqlConnection = (SqlConnection)sysDBConnection.DbConnect;

				//Se l'utente amministratore non è presente nel database MSD_Logins lo aggiungo
				if (!userdb.ExistLoginAlsoDisabled(adminLoginName))
				{
					userdb.Add
						(
						false, /*windowsAuthentication*/
						adminLoginName, /*login*/
						adminLoginPassword, /*password*/
						"Administrator application user", /*description*/
						DateTime.Today.ToShortDateString(), /*expirationDate*/
						false, /*disabled*/
						false /*userMustChangePassword*/,
						false, /*userCannotChangePassword*/
						false, /*expiredDateCannotChange*/
						true, /*passwordNeverExpired*/
						string.Empty /*preferredLanguage*/,
						string.Empty /*applicationLanguage*/,
						string.Empty, false, true, false, false, false, "0");

					if (userdb.Diagnostic.Error)
						dbEngineDiagnostic.Set(userdb.Diagnostic);

					// mi salvo l'Id della login Admin appena inserita
					adminId = userdb.LastLoginId().ToString();
				}
				else // altrimenti mi tengo da parte il suo LoginId
					adminId = userdb.GetIdFromLoginName(adminLoginName).ToString();

				//Se l'utente User non è presente nel database MSD_Logins lo aggiungo
				if (!userdb.ExistLoginAlsoDisabled(userLoginName))
				{
					userdb.Add
						(
						false, /*windowsAuthentication*/
						userLoginName, /*login*/
						userLoginPassword, /*password*/
						"", /*description*/
						DateTime.Today.ToShortDateString(), /*expirationDate*/
						false, /*disabled*/
						false /*userMustChangePassword*/,
						false, /*userCannotChangePassword*/
						false, /*expiredDateCannotChange*/
						true, /*passwordNeverExpired*/
						string.Empty /*preferredLanguage*/,
						string.Empty /*applicationLanguage*/,
						string.Empty, false, true, false, false, false, "0");

					if (userdb.Diagnostic.Error)
						dbEngineDiagnostic.Set(userdb.Diagnostic);

					// mi salvo l'Id della login User appena inserita
					userId = userdb.LastLoginId().ToString();
				}
				else // altrimenti mi tengo da parte il suo LoginId
					userId = userdb.GetIdFromLoginName(userLoginName).ToString();
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "CreateApplicationUsersForProvisioningEnv");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrApplicationUsersCreation, ei);
				return false;
			}

			return !dbEngineDiagnostic.Error;
		}

		///<summary>
		/// Lettura id degli utenti applicativi predefiniti dal sistema di provisioning
		/// per l'aggiunta di una nuova azienda
		/// </summary>
		//---------------------------------------------------------------------
		private bool GetApplicationUsersInfoForProvisioningEnvInAddCompanyMode()
		{
			try
			{
				UserDb userdb = new UserDb();
				userdb.ConnectionString = sysDBAdminConnectionString;
				userdb.CurrentSqlConnection = (SqlConnection)sysDBConnection.DbConnect;

				// mi faccio ritornare l'id dell'utente amministratore
				int loginId = userdb.GetIdFromLoginName(adminLoginName);
				if (loginId > 0)
					adminId = loginId.ToString(); // se lo trovo lo assegno alla variabile globale
				else
				{
					// se non lo trovo lo aggiungo?
					userdb.Add
						(
						false, /*windowsAuthentication*/
						adminLoginName, /*login*/
						adminLoginPassword, /*password*/
						"Administrator application user", /*description*/
						DateTime.Today.ToShortDateString(), /*expirationDate*/
						false, /*disabled*/
						false /*userMustChangePassword*/,
						false, /*userCannotChangePassword*/
						false, /*expiredDateCannotChange*/
						true, /*passwordNeverExpired*/
						string.Empty /*preferredLanguage*/,
						string.Empty /*applicationLanguage*/,
						string.Empty, false, true, false, false, false, "0");

					if (userdb.Diagnostic.Error)
						dbEngineDiagnostic.Set(userdb.Diagnostic);

					// mi salvo l'Id della login Admin appena inserita
					adminId = userdb.LastLoginId().ToString();
				}

				// mi faccio ritornare l'id dell'utente User
				loginId = userdb.GetIdFromLoginName(userLoginName);
				if (loginId > 0)
					userId = loginId.ToString(); // se lo trovo lo assegno alla variabile globale
				else
				{
					// se non lo trovo lo aggiungo?
					userdb.Add
						(
						false, /*windowsAuthentication*/
						userLoginName, /*login*/
						userLoginPassword, /*password*/
						"", /*description*/
						DateTime.Today.ToShortDateString(), /*expirationDate*/
						false, /*disabled*/
						false /*userMustChangePassword*/,
						false, /*userCannotChangePassword*/
						false, /*expiredDateCannotChange*/
						true, /*passwordNeverExpired*/
						string.Empty /*preferredLanguage*/,
						string.Empty /*applicationLanguage*/,
						string.Empty, false, true, false, false, false, "0");

					if (userdb.Diagnostic.Error)
						dbEngineDiagnostic.Set(userdb.Diagnostic);

					// mi salvo l'Id della login User appena inserita
					userId = userdb.LastLoginId().ToString();
				}
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "GetApplicationUsersInfoInAddCompanyMode");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrApplicationUsersCreation, ei);
				return false;
			}

			return !dbEngineDiagnostic.Error;
		}

		///<summary>
		/// Creazione azienda e relativa associazione utenti applicativi
		/// </summary>
		//---------------------------------------------------------------------
		private bool CreateCompanyForProvisioningEnv()
		{
			try
			{
				// aggiungo la nuova azienda
				CompanyDb companyDb = new CompanyDb();
				companyDb.ConnectionString = sysDBAdminConnectionString;
				companyDb.CurrentSqlConnection = (SqlConnection)sysDBConnection.DbConnect;

				companyDb.Add
					(
					CompanyName,                //company (nome azienda),
					DatabaseLayerStrings.MagoNetSampleCompany,  //description, 
					sqlProviderItem.ProviderId, //providerId, (metto il provider SQL)
					inputServer,                //companyDbServer, 
					CompanyDbName,              //companyDbName, 
					string.Empty,               //defaultUser,
					string.Empty,               //defaultPassword, 
					adminId,                    //companyDbOwner, il dbo e' l'utente amministratore
					false,                      //security,
					false,                      //activity, 
					true,                       //useTransaction, 
					true,                       //useKeyedUpdate,
					false,                      //companyDbWindowsAuthentication,
					string.Empty,               //preferredLanguage,
					string.Empty,               //applicationLanguage,
					false,                      //disabled,
					true,                       //IsoHelper.IsoExpectUnicodeCharSet(isoCountryCode), //useUnicode, // database sempre in unicode!
					true,                       //isValid,
					companyLcid,                //dbCultureLCID,
					supportColsCollation,       //supportColsCollation,
					0,                          //port
					false,                      //useDBSlave
					false,                      //useRowSecurity
					false                       //useDataSynchro
					);

				if (companyDb.Diagnostic.Error)
					dbEngineDiagnostic.Set(companyDb.Diagnostic);

				// salvo l'Id dell'azienda appena generato
				lastCompanyId = companyDb.LastCompanyId().ToString();

				// associo le login di amministrazione e User all'azienda appena creata
				CompanyUserDb companyUserDb = new CompanyUserDb();
				companyUserDb.ConnectionString = sysDBAdminConnectionString;
				companyUserDb.CurrentSqlConnection = (SqlConnection)sysDBConnection.DbConnect;

				// l'utente dbo e' associato 1 a 1 alla login con le credenziali di amministrazione
				companyUserDb.Add(lastCompanyId, adminId, true, false, adminLoginName, adminLoginPassword, false);
				// l'utente User e' associato alla login generata automaticamente
				companyUserDb.Add(lastCompanyId, userId, false, false, userLoginName, userLoginPassword, false);

				if (companyUserDb.Diagnostic.Error)
					dbEngineDiagnostic.Set(companyUserDb.Diagnostic);
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "CreateCompanyForProvisioningEnv");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrCompanyCreation, ei);
			}

			return true;
		}

		///<summary>
		/// Effettuo i grant delle logins su tutti e tre i database:
		/// - per l'amministratore il ruolo di db_owner
		/// - per l'utente semplice i ruoli di db_datareader e db_writer
		///</summary>
		//---------------------------------------------------------------------
		private bool GrantLoginsForProvisioningEnv()
		{
			bool result = false;

			try
			{
				connSqlTransact.CurrentStringConnection = sysDBAdminConnectionString;

				// aggiungo le varie login su SQL (tanto viene fatto il change database dentro i metodi)
				result = GrantAdminForProvisioningEnv();

				// assegno i ruoli db_datareader e db_datawriter all'utente semplice per tutti i database
				if (result)
					result = GrantUserForProvisioningEnv();

				if (!result)
					dbEngineDiagnostic.Set(connSqlTransact.Diagnostic);
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "GrantLoginsForProvisioningEnv");
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				dbEngineDiagnostic.Set(DiagnosticType.Error, string.Format(DatabaseLayerStrings.ErrGrantLoginOnDB, automaticSqlLoginName, CompanyDbName), ei);
				return false;
			}

			return result;
		}

		//---------------------------------------------------------------------
		private bool GrantAdminForProvisioningEnv()
		{
			bool result = false;

			if (!IsDbo(adminLoginName, SystemDbName))
				result = connSqlTransact.SPGrantDbAccess(adminLoginName, adminLoginName, SystemDbName) &&
						connSqlTransact.SPAddRoleMember(adminLoginName, DatabaseLayerConsts.RoleDbOwner, SystemDbName);
			else
				result = true;
			if (result)
			{
				if (!IsDbo(adminLoginName, CompanyDbName))
					result = connSqlTransact.SPGrantDbAccess(adminLoginName, adminLoginName, CompanyDbName) &&
							connSqlTransact.SPAddRoleMember(adminLoginName, DatabaseLayerConsts.RoleDbOwner, CompanyDbName);
				else
					result = true;
			}
			if (result)
			{
				if (!IsDbo(adminLoginName, DMSDbName))
					result = connSqlTransact.SPGrantDbAccess(adminLoginName, adminLoginName, DMSDbName) &&
							connSqlTransact.SPAddRoleMember(adminLoginName, DatabaseLayerConsts.RoleDbOwner, DMSDbName);
				else
					result = true;
			}

			return result;
		}

		//---------------------------------------------------------------------
		private bool GrantUserForProvisioningEnv()
		{
			bool result = false;

			if (!IsDbo(userLoginName, SystemDbName))
				result = connSqlTransact.SPGrantDbAccess(userLoginName, userLoginName, SystemDbName) &&
					connSqlTransact.SPAddRoleMember(userLoginName, DatabaseLayerConsts.RoleDataWriter, SystemDbName) &&
					connSqlTransact.SPAddRoleMember(userLoginName, DatabaseLayerConsts.RoleDataReader, SystemDbName);
			else
				result = true;
			if (result)
			{
				if (!IsDbo(userLoginName, CompanyDbName))
					result = connSqlTransact.SPGrantDbAccess(userLoginName, userLoginName, CompanyDbName) &&
					connSqlTransact.SPAddRoleMember(userLoginName, DatabaseLayerConsts.RoleDataWriter, CompanyDbName) &&
					connSqlTransact.SPAddRoleMember(userLoginName, DatabaseLayerConsts.RoleDataReader, CompanyDbName);
				else
					result = true;
			}
			if (result)
			{
				if (!IsDbo(userLoginName, DMSDbName))
					result = connSqlTransact.SPGrantDbAccess(userLoginName, userLoginName, DMSDbName) &&
					connSqlTransact.SPAddRoleMember(userLoginName, DatabaseLayerConsts.RoleDataWriter, DMSDbName) &&
					connSqlTransact.SPAddRoleMember(userLoginName, DatabaseLayerConsts.RoleDataReader, DMSDbName);
				else
					result = true;
			}

			return result;
		}

		//---------------------------------------------------------------------
		private bool IsDbo(string login, string database)
		{
			bool isDbo = false;

			string dboOfDataBase = string.Empty;
			if (connSqlTransact.CurrentDbo(login, database, out dboOfDataBase))
				isDbo = string.Compare(login, dboOfDataBase, StringComparison.InvariantCultureIgnoreCase) == 0;

			return isDbo;
		}

		//---------------------------------------------------------------------
		public bool ConfigureProvisioningEnvironmentInAddCompanyModeLITE()
		{
			return ConfigureProvisioningEnvironmentInAddCompanyMode(true);
		}

		/// <summary>
		/// Entry-point per la creazione di una company aggiuntiva in un ambiente gia' configurato
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public bool ConfigureProvisioningEnvironmentInAddCompanyMode(bool lite = false)
		{
			ComposeConnectionStrings();

			bool result = false;

			if (!lite)
			{
				//------------------------------------------------------------
				// creo la login sul server SQL (se non esiste)
				//------------------------------------------------------------
				result = CreateLoginsForProvisioningEnv();
				if (!result)
					return false;
			}

			//------------------------------------------------------------
			// inserisco le informazioni nel db di sistema (application users, company, providers)
			//------------------------------------------------------------
			OnDBEngineMainEvent(EventType.Info, MainEventType.CreateUserAndCompany);

			result = ConfigureSystemDBForProvisioningEnv(true, lite);

			OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateUserAndCompany);
			if (!result)
				return false;

			if (!lite)
			{
				//------------------------------------------------------------
				// creo il contenitore del database aziendale
				//------------------------------------------------------------
				OnDBEngineMainEvent(EventType.Info, MainEventType.CreateCompanyDB);
				result = CreateDatabaseContainer(KindOfDatabase.Company);
				OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateCompanyDB);
				if (!result)
					return false;

				//------------------------------------------------------------
				// creo il contenitore del database del DMS
				//------------------------------------------------------------
				OnDBEngineMainEvent(EventType.Info, MainEventType.CreateDMSDB);
				result = CreateDatabaseContainer(KindOfDatabase.Dms);
				OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateDMSDB);
				if (!result)
					return false;

				//------------------------------------------------------------
				// associo le logins ai database appena creati con gli opportuni ruoli
				// lo devo fare prima della creazione delle tabelle del db aziendale perche' utilizzando le credenziali
				// salvate nel db di sistema non riesco a connettermi
				//------------------------------------------------------------
				result = GrantLoginsForProvisioningEnv();
				if (!result)
					return false;
			}

			//------------------------------------------------------------
			// creo le tabelle nel database aziendale alla fine
			//------------------------------------------------------------
			OnDBEngineMainEvent(EventType.Info, MainEventType.CreateCompanyDBStructure);
			result = CreateUpgradeCompanyDatabase();
			OnDBEngineMainEvent(result ? EventType.Success : EventType.Error, MainEventType.CreateCompanyDBStructure);

			OnDBEngineEvent(EventType.Success, DatabaseLayerStrings.ElaborationCompleted);

			return result;
		}

		#region Metodi ed eventi per la visualizzazione della messaggistica
		/// <summary>
		/// inserisce nella text-box il nome del modulo durante l'elaborazione degli script
		/// "Elaborazione ... in corso"
		/// </summary>
		//---------------------------------------------------------------------
		private void PostToModuleCounter(object sender)
		{
			OnDBEngineEvent(EventType.Success, string.Format(DatabaseLayerStrings.FormModuleCounter, ((ModuleDBInfo)sender).Title));
		}

		/// <summary>
		/// inserisce nella text-box il nome della tabella a cui si stanno aggiungendo le colonne obbligatorie
		/// "Aggiunta colonne obbligatorie alla tabella {0} in corso..."
		/// </summary>
		//---------------------------------------------------------------------
		private void PostToTableAndMandatoryCols(string table)
		{
			if (isFirstMandatoryColumn)
			{
				OnDBEngineMainEvent(EventType.Success, MainEventType.CreateCompanyDBStructure);
				OnDBEngineMainEvent(EventType.Info, MainEventType.MandatoryColumnsCreation);
				isFirstMandatoryColumn = false;
			}

			OnDBEngineEvent(EventType.Success, string.Format(DatabaseLayerStrings.FormTableWithMandatoryColsCounter, table));
		}

		/// <summary>
		/// inserisce nella label il nome del modulo durante l'importazione dei file contenenti i dati di esempio
		/// </summary>
		//---------------------------------------------------------------------
		private void PostToModuleCounterForSample(string fileName, string appName, string moduleName)
		{
			if (isFirstImportFile)
			{
				OnDBEngineMainEvent(EventType.Success, MainEventType.MandatoryColumnsCreation);
				if (!forProvisioningEnvironment)
				OnDBEngineMainEvent(EventType.Info, MainEventType.ImportSampleData);
				isFirstImportFile = false;
			}

			// trucco per visualizzare il nome del modulo brandizzato
			if (string.Compare(myModule, moduleName, StringComparison.InvariantCultureIgnoreCase) != 0)
			{
				myModule = moduleName;
				myModuleInfo = (ModuleInfo)pathFinder.GetModuleInfoByName(appName, moduleName);
			}

			OnDBEngineEvent
				(
				EventType.Success,
				string.Format(DatabaseLayerStrings.FormDefaultFileCounter, fileName, (myModuleInfo != null) ? myModuleInfo.Title : moduleName)
				);
		}

		///<summary>
		/// Salva nel diagnostico principale le informazioni di elaborazione, in modo da serializzarle
		/// in un file di log.
		///</summary>
		//---------------------------------------------------------------------
		private void PostElaborationProgressMessage(object sender, bool ok, string script, string step, string detail, string fullPath, ExtendedInfo ei)
		{
			string app = (sender != null) ? ((ModuleDBInfo)sender).ApplicationBrand : string.Empty;	// visualizzo il Name dell'Applicazione
			string mod = (sender != null) ? ((ModuleDBInfo)sender).Title : string.Empty; // visualizzo il Title del Modulo

			System.Collections.Specialized.StringCollection myStrings = new System.Collections.Specialized.StringCollection();
			myStrings.Add(string.Format(DatabaseLayerStrings.ScriptName, script));
			myStrings.Add(string.Format(DatabaseLayerStrings.StepNumber, step));
			myStrings.Add(string.Format(DatabaseLayerStrings.DirectoryName, fullPath));
			myStrings.Add(string.Format(DatabaseLayerStrings.ApplicationName, app));
			myStrings.Add(string.Format(DatabaseLayerStrings.ModuleName, mod));

			myStrings.Add((!ok)
				? string.Format(DatabaseLayerStrings.ErrorOperation, detail)
				: (string.Compare(detail, DatabaseLayerConsts.OK, StringComparison.InvariantCultureIgnoreCase) == 0)
					? DatabaseLayerStrings.SuccessOperation
					: detail
				 );

			Diagnostic diagnostic = new Diagnostic("DatabaseEngineLog");
			diagnostic.Set((ok) ? DiagnosticType.Information : DiagnosticType.Error, myStrings);

			// aggiungo le informazioni nel diagnostico, così lo salvo nel file di log (escluse le colonne TBCreated e TBModified)
			if (string.Compare(detail, DatabaseLayerStrings.CreatedMandatoryColumns, StringComparison.InvariantCultureIgnoreCase) != 0)
				dbManager.DBManagerDiagnostic.Set(diagnostic);
		}

		//--------------------------------------------------------------------------------
		private void OnDBEngineEvent(EventType eType, string eMessage)
		{
			GenerationEvent?.Invoke(this, new DBEngineEventArgs(eType, eMessage));
		}

		//--------------------------------------------------------------------------------
		private void OnDBEngineMainEvent(EventType eType, MainEventType mainEventType)
		{
			GenerationMainEvent?.Invoke(this, new DBEngineEventArgs(eType, mainEventType));
		}
		# endregion

		///<summary>
		/// Creazione del file LoggedUser.xml, da salvare in Apps\MagoNet\release, in modo da proporre
		/// il corretto utente per la connessione a Mago.Net
		///</summary>
		//---------------------------------------------------------------------
		private void CreateLoggedUserFile(string user)
		{
			LoggedUser loggedUser = new LoggedUser(user, CompanyName);
			loggedUser.Save();
		}
	}

	///<summary>
	/// Classe per la gestione degli eventi e per rendere visibile all'esterno
	/// la messaggistica rimpallata dall'elaborazione o lo stato dell'evento
	///</summary>
	//=========================================================================
	public class DBEngineEventArgs : EventArgs
	{
		private EventType eventType;
		private MainEventType mainEventType;
		private string eventMessage;

		//---------------------------------------------------------------------
		public EventType EventType { get { return eventType; } set { eventType = value; } }
		public MainEventType MainEventType { get { return mainEventType; } set { mainEventType = value; } }
		public string EventMessage { get { return eventMessage; } set { eventMessage = value; } }

		//---------------------------------------------------------------------
		public DBEngineEventArgs(EventType eventType, string eventMessage)
		{
			this.eventType = eventType;
			this.eventMessage = eventMessage;
		}

		//---------------------------------------------------------------------
		public DBEngineEventArgs(EventType eventType, MainEventType mainEventType)
		{
			this.eventType = eventType;
			this.mainEventType = mainEventType;
		}
	}

	///<summary>
	/// MainEventType enum
	/// Identifica i tipi di eventi principali eseguiti in questa classe
	///</summary>
	//=========================================================================
	public enum MainEventType
	{
		CreateSystemDB,
		CreateSystemDBStructure,
		CreateUserAndCompany,
		CreateCompanyDB,
		CreateCompanyDBStructure,
		ImportSampleData,
		MandatoryColumnsCreation,
		CreateDMSDB
	}

	///<summary>
	/// EventType enum
	/// Identifica lo stato dell'evento
	///</summary>
	//=========================================================================
	public enum EventType
	{
		Info,
		Success,
		Error
	}
}
