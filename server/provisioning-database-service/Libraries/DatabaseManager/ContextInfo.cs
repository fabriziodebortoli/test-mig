using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Microarea.Common.DiagnosticManager;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;
using Microarea.ProvisioningDatabase.Infrastructure.Model.Interfaces;

namespace Microarea.ProvisioningDatabase.Libraries.DatabaseManager
{
	/// <summary>
	/// ContextInfo (classe che contiene funzioni e informazioni comuni, in modo 
	/// da poterle utilizzare agevolmente dai vari componenti esterni)
	/// </summary>
	//============================================================================
	public class ContextInfo
	{
		# region Struttura SystemDBConnectionInfo (con le info per la connessione al db sistema)
		/// struttura che contiene le info per la connessione al database di sistema
		//---------------------------------------------------------------------
		public struct SystemDBConnectionInfo
		{
			public string DBName;
			public string ServerName;
			public string UserId;
			public string Password;
			public string Instance;
		}
		# endregion

		#region Variables
		public SystemDBConnectionInfo SysDBConnectionInfo = new SystemDBConnectionInfo();

		private DBMSType dbType = DBMSType.SQLSERVER;
		private string schemaOwner = string.Empty;
		private string companyId = string.Empty;
		private string connectSysDB = string.Empty;
		private string connectAzDB = string.Empty;
		private PathFinder pathFinder = null;

		private string companyDBServer = string.Empty;
		private string companyDBName = string.Empty;
		private string companyName = string.Empty;
		private bool useDbSlave = false;
		private int port = 0;
		private string provider = string.Empty; // visibilità del provider

		private string dbUser = string.Empty;
		private string dbPw = string.Empty;
		private bool dbWinAut = false;
		private bool useAuditing = false;
		private bool askCredential = true;

		// per tenere traccia della DatabaseCulture impostata sulla company
		private int databaseCulture = 0;
		private bool supportColumnCollation = true;

		private Diagnostic diagnostic = new Diagnostic("ContextInfo");
		public TBConnection Connection = null;

		public int SqlErrorCode = 0;			// codice di errore di TBException
		public string SqlMessage = string.Empty;// Message di tipo TBException

		public string IsoState = string.Empty;
		public bool UseUnicode = false;

		public DBNetworkType DBNetworkType = DBNetworkType.Small;
		// gestione RowSecurityLayer
		private bool isRowSecurityActivated = false; // pilotato dal plugin chiamante, dice se il modulo e' attivato
		private bool useRowSecurity = false; // se l'azienda correntemente connessa utilizza il RowSecurityLayer

		// variabili di appoggio per il controllo preventivo sul database sulla congruenza dell'Unicode e della DBCulture
		public bool InconsistentSetting = false; // variabile da testare dall'esterno per proporre un msg di errore
		private bool supportColsCollationFromDB = false;
		private int dbCultureFromDB = 0;
		private bool dbUnicodeFromDB = false;
		private string messageInconsistentSetting = string.Empty; // composizione messaggio di errore
		//

		//------ gestione database documentale 
		private bool hasSlaves = false; // identifica se esiste sul db di sistema uno slave dell'azienda
		private bool isDMSActivated = false; // pilotato dal plugin chiamante, il check del db documentale si fa SOLO se il modulo e' attivato

		public TBConnection DmsConnection = null;
		private string connectDmsDB = string.Empty;
		private string dmsSlaveId = string.Empty;
		private string dmsSignature = string.Empty;
		private string dmsServerName = string.Empty;
		private string dmsDatabaseName = string.Empty;

		private string dmsSlaveDBUser = string.Empty;
		private string dmsSlaveDBPw = string.Empty;
		private bool dmsSlaveDBWinAuth = false;
		//------
		#endregion

		#region Properties
		//---------------------------------------------------------------------
		public string CompanyId { get { return companyId; } }
		public string CompanyDBServer { get { return companyDBServer; } }
		public string CompanyDBName { get { return companyDBName; } }
		public string CompanyName { get { return companyName; } }
		public string Provider { get { return provider; } }

		public int DatabaseCulture { get { return databaseCulture; } }

		public string SchemaOwner { get { return schemaOwner; } }
		public string ConnectSysDB { get { return connectSysDB; } }
		public string ConnectAzDB { get { return connectAzDB; } }

		public PathFinder PathFinder { get { return pathFinder; } }
		public DBMSType DbType { get { return dbType; } }
		public Diagnostic Diagnostic { get { return diagnostic; } }

		public string ConnectDmsDB { get { return connectDmsDB; } }

		public bool HasSlaves { get { return hasSlaves; } }

		public bool UseRowSecurity { get { return useRowSecurity; } }
		public bool IsRowSecurityActivated { get { return isRowSecurityActivated; } }
		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="pathFinder">puntatore al PathFinder</param>
		/// <param name="dbNetworkType">scelta in fase di attivazione: Small or Large</param>
		/// <param name="isoState">country code di attivazione</param>
		//---------------------------------------------------------------------
        public ContextInfo(PathFinder pathFinder, DBNetworkType dbNetworkType, string isoState)
		{
			this.pathFinder = pathFinder;
			DBNetworkType = dbNetworkType;
			IsoState = isoState;
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="pathFinder">puntatore al PathFinder</param>
		/// <param name="dbNetworkType">scelta in fase di attivazione: Small or Large</param>
		/// <param name="isoState">country code di attivazione</param>
		/// <param name="askCredential">se proporre o meno la form di autenticazione all'utente(false per esecuzione in modalita' silente)</param>
		//---------------------------------------------------------------------
        public ContextInfo(PathFinder pathFinder, DBNetworkType dbNetworkType, string isoState, bool askCredential)
			: this(pathFinder, dbNetworkType, isoState)
		{
			this.askCredential = askCredential;
		}
		#endregion

		# region CloseConnection
		/// <summary>
		/// funzione esposta per forzare la chiusura della connessione aperta o appesa
		/// </summary>
		//---------------------------------------------------------------------
		public void CloseConnection()
		{
			if (Connection.State == ConnectionState.Open || Connection.State == ConnectionState.Broken)
			{
				Connection.Close();
                if (!Connection.IsPostgreConnection())
				    Connection.Dispose();
			}

			// nel dubbio chiudo anche la connessione al database documentale
			CloseDmsConnection();
		}
		# endregion

		# region Composizione stringa di connessione db sistema e relative Open
		/// <summary>
		/// compone la stringa di connessione al database di sistema, 
		/// sulla base dei parametri passati dalla Console
		/// </summary>
		/// <param name="param">struttura con i parametri di connessione al DB di sistema</param>
		//---------------------------------------------------------------------------
		public void ComposeSystemDBConnectionString()
		{
			connectSysDB = string.IsNullOrEmpty(SysDBConnectionInfo.Instance)
							? string.Format
								(
								NameSolverDatabaseStrings.SQLConnection,
								SysDBConnectionInfo.ServerName,
								SysDBConnectionInfo.DBName,
								SysDBConnectionInfo.UserId,
								SysDBConnectionInfo.Password
								)
							: string.Format
								(
								NameSolverDatabaseStrings.SQLConnection,
								Path.Combine(SysDBConnectionInfo.ServerName, SysDBConnectionInfo.Instance),
								SysDBConnectionInfo.DBName,
								SysDBConnectionInfo.UserId,
								SysDBConnectionInfo.Password
								);

			// se userId è vuoto vuol dire che sono in sicurezza integrata 
			// (impossibile perche' la connessione al db di sistema non puo' essere in win authentication
			if (string.IsNullOrEmpty(SysDBConnectionInfo.UserId))
			{
				connectSysDB = string.IsNullOrEmpty(SysDBConnectionInfo.Instance)
					? string.Format
						(
						NameSolverDatabaseStrings.SQLWinNtConnection,
						SysDBConnectionInfo.ServerName,
						SysDBConnectionInfo.DBName
						)
					: string.Format
						(
						NameSolverDatabaseStrings.SQLWinNtConnection,
						Path.Combine(SysDBConnectionInfo.ServerName, SysDBConnectionInfo.Instance),
						SysDBConnectionInfo.DBName
						);
			}
		}

		/// <summary>
		/// apre la connessione al database di sistema
		/// </summary>
		/// <returns>se la connessione è avvenuta con successo</returns>
		//---------------------------------------------------------------------------
		public bool OpenSysDBConnection()
		{
			bool resultSysConnect = false;

			if (string.IsNullOrEmpty(connectSysDB))
				return resultSysConnect;

			try
			{
				if (Connection != null && Connection.State == ConnectionState.Open)
					Connection.Close();

				// apro la connessione al db di sistema
				Connection = new TBConnection(connectSysDB, DBMSType.SQLSERVER);
				Connection.Open();
				resultSysConnect = true;
			}
			catch (TBException e)
			{
				resultSysConnect = false;
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "OpenSysDBConnection");
				extendedInfo.Add(DatabaseManagerStrings.Library, "TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrQuerySystemDB, extendedInfo);
				SqlErrorCode = e.Number;
				SqlMessage = e.Message;
			}

			return resultSysConnect;
		}

		/// <summary>
		/// apre la connessione al database di sistema
		/// </summary>
		/// <returns>se la connessione è avvenuta con successo</returns>
		//---------------------------------------------------------------------------
		public bool OpenSysDBConnectionFromString(string connectStr, DBMSType dbmsType)
		{
			if (!string.IsNullOrEmpty(connectStr))
				connectSysDB = connectStr;

			dbType = dbmsType;

			return OpenSysDBConnection();
		}
		# endregion

		# region Composizione stringhe di connessione ai database
		/// <summary>
		/// Join sulle tabelle del database di sistema e compongo
		/// la stringa di connessione al database aziendale
		/// </summary>
		/// <returns>se la query è andata a buon fine</returns>
		//---------------------------------------------------------------------------
		private bool CreateCompanyConnectionString()
		{
			bool result = true;

			if (Connection == null || Connection.State != ConnectionState.Open)
				return result;

			try
			{
				// faccio la join tra le tabelle del db di sistema per avere poi a disposizione
				// i valori per comporre la stringa di connessione al database aziendale
				// su cui devo andare a controllare ed eventualmente effettuare gli aggiornamenti
				string query = @"	SELECT MSD_Companies.Company, MSD_Companies.CompanyDBServer, MSD_Companies.Port,
									MSD_Companies.CompanyDBName, MSD_Companies.UseUnicode, MSD_Companies.UseAuditing,
									MSD_Companies.UseRowSecurity,
									MSD_Companies.DatabaseCulture, MSD_Companies.SupportColumnCollation, MSD_Companies.UseDBSlave,
									MSD_Providers.Provider,
									MSD_CompanyLogins.DBUser, MSD_CompanyLogins.DBPassword, MSD_CompanyLogins.DBWindowsAuthentication
							FROM	MSD_Companies, MSD_Logins, MSD_CompanyLogins, MSD_Providers
							WHERE
									MSD_Companies.CompanyId		 = @CompanyId AND
									MSD_Companies.CompanyId		 = MSD_CompanyLogins.CompanyId AND
									MSD_Companies.CompanyDBOwner = MSD_CompanyLogins.LoginId AND
									MSD_Companies.ProviderId	 = MSD_Providers.ProviderId AND
									MSD_Companies.CompanyDBOwner = MSD_Logins.LoginId";

				using (TBCommand command = new TBCommand(query, Connection))
				{
					command.Parameters.Add("@CompanyId", Convert.ToInt32(companyId));
					using (IDataReader dataReader = command.ExecuteReader())
					{
						while (dataReader.Read())
						{
							companyName = dataReader["Company"].ToString();
							companyDBServer = dataReader["CompanyDBServer"].ToString();
							companyDBName = dataReader["CompanyDBName"].ToString();
							provider = dataReader["Provider"].ToString();
							dbUser = dataReader["DBUser"].ToString();
							dbPw = "14";//Crypto.Decrypt(dataReader["DBPassword"].ToString());
							dbWinAut = (bool)dataReader["DBWindowsAuthentication"];
							UseUnicode = (bool)dataReader["UseUnicode"];
							useAuditing = (bool)dataReader["UseAuditing"];
							schemaOwner = dataReader["DBUser"].ToString();
							databaseCulture = Convert.ToInt32(dataReader["DatabaseCulture"]);
							supportColumnCollation = (bool)dataReader["SupportColumnCollation"];
							useDbSlave = (bool)dataReader["UseDBSlave"];
							port = Convert.ToInt32(dataReader["Port"]);
							useRowSecurity = (bool)dataReader["UseRowSecurity"];
						}

						if (Connection.IsPostgreConnection() && port == 0)
							port = DatabaseLayerConsts.postgreDefaultPort;
					}
				}

				dbType = TBDatabaseType.GetDBMSType(Provider);

				string pw = dbPw;

				connectAzDB = TBDatabaseType.GetConnectionString
								(companyDBServer, dbUser, pw, CompanyDBName, Provider, dbWinAut, false, port);

				// compongo la stringa di connessione al database documentale, sempre
				// che ci sia uno slave agganciato all'azienda
				if (useDbSlave)
					CreateDmsConnectionString();

				result = true;
			}
			catch (TBException e)
			{
				CloseConnection();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "CreateCompanyConnectionString");
				extendedInfo.Add(DatabaseManagerStrings.Library, "TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrQuerySystemDB, extendedInfo);
				result = false;
			}

			return result;
		}

		//---------------------------------------------------------------------------
		private bool CreateDmsConnectionString()
		{
			// controllo se l'azienda ha degli slave associati (per il db documentale)
			// solo se e' stato specificato esplicitamente il check (dipende se il modulo e' stato attivato)
			hasSlaves = isDMSActivated && ExistDmsSlaveForCompany();

			if (!hasSlaves)
				return true;

			if (QuerySysDatabaseForDms() && !string.IsNullOrWhiteSpace(connectDmsDB))
				return true;

			return false;
		}
		# endregion

		# region Apertura connessione al db aziendale (funzione standard e funzione per altri plugins)
		/// <summary>
		/// Effettua la connessione al SOLO database aziendale
		/// (utilizzata da Auditing, DataManager, MigrationKit, che ignorano l'eventuale db documentale)
		/// </summary>
		/// <param name="companyIdNode">id company a cui mi devo connettere</param>
		/// <returns>se la connessione è avvenuta con successo</returns>
		//---------------------------------------------------------------------------
		public bool MakeCompanyConnection(string companyIdNode)
		{
			return MakeCompanyConnection(companyIdNode, false, false);
		}

		/// <summary>
		/// Effettua la connessione al database aziendale (e a quello documentale se richiesto)
		/// </summary>
		/// <param name="companyIdNode">id company a cui mi devo connettere</param>
		/// <param name="isDMSActivated">se devo effettuare il check anche dello slave (solo se il modulo e' attivato)</param>
		/// <param name="isRowSecurityActivated">se il modulo RowSecurityLayer e' attivato</param>
		/// <returns>se la connessione è avvenuta con successo</returns>
		//---------------------------------------------------------------------------
		public bool MakeCompanyConnection(string companyIdNode, bool isDMSActivated, bool isRowSecurityActivated)
		{
			ComposeSystemDBConnectionString();

			this.companyId = companyIdNode;
			this.isDMSActivated = isDMSActivated;
			this.isRowSecurityActivated = isRowSecurityActivated;

			// mi connetto al database di sistema
			// compongo la stringa di connessione al database aziendale e a quello documentale
			if (!(OpenSysDBConnection() && CreateCompanyConnectionString())
				|| string.IsNullOrEmpty(connectAzDB))
			{
				CloseConnection();
				return false;
			}

			try
			{
				// apro la connessione al db aziendale
				Connection = new TBConnection(connectAzDB, dbType, schemaOwner);
				Connection.Open();

				// qui apro la connessione al db documentale (se esiste)
				if (hasSlaves)
				{
					DmsConnection = new TBConnection(connectDmsDB, DBMSType.SQLSERVER, dmsSlaveDBUser);
					DmsConnection.Open();
				}
			}
			catch (TBException e)
			{
				CloseConnection();
				SqlErrorCode = e.Number;
				SqlMessage = e.Message;
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "MakeCompanyConnection");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectCompanyDB, extendedInfo);
				return false;
			}

			//****** CONTROLLI SMALLNETWORK: versione db e sua dimensione
			// se è la licenza è SmallNetwork il db deve essere MSDE o SqlExpress
			// e se sto associando un db esistente devo controllare che non ecceda la dimensione di 2GB
			// (controllo solo per SQL Server)
			if (DBNetworkType == DBNetworkType.Small && Connection.IsSqlConnection())
			{
				if (/*InstallationData.CheckDBSize &&*/ TBCheckDatabase.IsDBSizeOverMaxLimit(Connection))
				{
					CloseConnection();
					diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.DBSizeError);
					return false;
				}
			}

			// se la DatabaseCulture dell'azienda è uguale a zero significa che non è stata ancora impostata
			// pertanto cerco di trovare la prima compatibile
			if (DatabaseCulture == 0)
				AssignDatabaseCultureValue();

			return true;
		}

		//---------------------------------------------------------------------------
		public bool MakeSubscriptionDatabaseConnection(ISubscriptionDatabase subDatabase)
		{
			connectAzDB = string.Format(NameSolverDatabaseStrings.SQLConnection, subDatabase.DBServer, subDatabase.DBName, subDatabase.DBOwner, subDatabase.DBPassword);

			try
			{
				// apro la connessione al db aziendale
				Connection = new TBConnection(connectAzDB, dbType);
				Connection.Open();

				// qui apro la connessione al db documentale (se esiste)
				if (subDatabase.UseDMS)
				{
					hasSlaves = true;
					connectDmsDB = string.Format(NameSolverDatabaseStrings.SQLConnection, subDatabase.DMSDBServer, subDatabase.DMSDBName, subDatabase.DMSDBOwner, subDatabase.DMSDBPassword);

					DmsConnection = new TBConnection(connectDmsDB, DBMSType.SQLSERVER);
					DmsConnection.Open();
				}
			}
			catch (TBException e)
			{
				CloseConnection();
				SqlErrorCode = e.Number;
				SqlMessage = e.Message;
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "MakeCompanyConnection");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectCompanyDB, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region SetUpdatingFlag (per impostare il flag Updating sulla MSD_Companies)
			/// <summary>
			/// Imposta il valore corretto nel flag Updating sulla tabella MSD_Companies
			/// (a seconda del parametro passato)
			/// Se è a true significa che la company è in fase di aggiornamento.
			/// Nel caso in cui un utente lato TB provi ad accedere a questa company, il
			/// programma bloccherà l'accesso.
			/// </summary>
			//---------------------------------------------------------------------------
			public void SetUpdatingFlag(bool isUpdating)
		{
			if (string.IsNullOrEmpty(connectSysDB))
				return;

			try
			{
				string query = "UPDATE MSD_Companies SET Updating = @update WHERE CompanyId = @companyId";

				// apro al volo una connessione al db di sistema
				using (SqlConnection mySysDBConnection = new SqlConnection(connectSysDB))
				{
					mySysDBConnection.Open();
					using (SqlCommand sqlCommand = new SqlCommand(query, mySysDBConnection))
					{
						sqlCommand.Parameters.AddWithValue("@update", isUpdating);
						sqlCommand.Parameters.AddWithValue("@companyId", Convert.ToInt32(companyId));
						sqlCommand.ExecuteNonQuery();
					}
				}
			}
			catch (SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "SetUpdatingFlag");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorUpdatingSystemDB, extendedInfo);
			}
		}
		# endregion

		# region SET ARITHABORT ON/OFF su database aziendale (solo SQL)
		/// <summary>
		/// Imposta il valore del parametro ARITHABORT sul database aziendale a ON/OFF
		/// per la creazione/aggiornamento di database con view 
		/// </summary>
		//---------------------------------------------------------------------------
		public void SetArithAbortDbOption(bool setOn)
		{
			if (Connection == null || Connection.State != ConnectionState.Open || !Connection.IsSqlConnection())
				return;

			try
			{
				string query = string.Format("ALTER DATABASE [{0}] SET ARITHABORT {1}", Connection.Database, setOn ? "ON" : "OFF");

				TBCommand tbCommand = new TBCommand(query, Connection);
				tbCommand.ExecuteNonQuery();
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "SetArithAbortDbOption");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorUpdatingSystemDB, extendedInfo);
			}
		}
		# endregion

		# region SetCompanyDatabaseCulture (per impostare l'LCID relativo alla DatabaseCulture della company)
		/// <summary>
		/// Imposta il valore corretto della DatabaseCulture sulla tabella MSD_Companies
		/// </summary>
		//---------------------------------------------------------------------------
		public void SetCompanyDatabaseCulture()
		{
			if (string.IsNullOrEmpty(connectSysDB))
				return;
			
			try
			{
				string query = @"UPDATE MSD_Companies SET DatabaseCulture = @dbculture 
								WHERE CompanyId = @companyId AND DatabaseCulture = 0";

				// apro al volo una connessione al db di sistema
				using (SqlConnection mySysDBConnection = new SqlConnection(connectSysDB))
				{
					mySysDBConnection.Open();

					using (SqlCommand sqlCommand = new SqlCommand(query, mySysDBConnection))
					{
						sqlCommand.Parameters.AddWithValue("@dbculture", DatabaseCulture);
						sqlCommand.Parameters.AddWithValue("@companyId", Convert.ToInt32(companyId));
						sqlCommand.ExecuteNonQuery();
					}
				}
			}
			catch (SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "SetCompanyDatabaseCulture");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorUpdatingSystemDB, extendedInfo);
			}
		}
		# endregion

		# region AssignDatabaseCultureValue
		/// <summary>
		/// Se la colonna DatabaseCulture sulla tabella MSD_Companies è uguale a zero,
		/// si tratta di un vecchio database. Pertanto devo leggere la Collate direttamente dal db
		/// e cercare un LCID valido da assegnare.
		/// Prima guardo sul ServerConnection.config il valore dell'ApplicationLanguage,
		/// poi salgo alla PreferredLanguage (se non neutral) e poi a quella più vicina all'IsoStato di installazione
		/// </summary>
		//---------------------------------------------------------------------------
		private void AssignDatabaseCultureValue()
		{
			/*int[] lcids = null;
			string validCollate = string.Empty;

			switch (dbType)
			{
				case DBMSType.SQLSERVER:
					{
						validCollate = TBCheckDatabase.GetValidCollationPropertyForDB(Connection);
						// richiamare la funzione di Carlotta per avere un elenco di LCID validi e poi vado in scaletta
						lcids = CultureHelper.GetCompatibleLocaleIDs(validCollate);
						break;
					}
			}

			// se per la collate c'è un solo LCID prendo quello (situazione ottimale)
			if (lcids != null && lcids.Length == 1)
				databaseCulture = lcids[0];
			else
			{
				int lcidToFind = 0;
				CultureInfo ci = null;
				CultureInfo[] cults = null;

				if (InstallationData.ServerConnectionInfo.ApplicationLanguage.Length > 0)
				{
					ci = new CultureInfo(InstallationData.ServerConnectionInfo.ApplicationLanguage);
					lcidToFind = ci.LCID;
				}
				else
				{
					if (InstallationData.ServerConnectionInfo.PreferredLanguage.Length > 0)
					{
						ci = new CultureInfo(InstallationData.ServerConnectionInfo.PreferredLanguage);
						if (!ci.IsNeutralCulture)
							lcidToFind = ci.LCID;
						else
						{
							cults = CultureInfo.GetCultures(CultureTypes.AllCultures);
							for (int i = 0; i < cults.Length; i++)
							{
								if (string.Compare(cults[i].TwoLetterISOLanguageName, this.IsoState, StringComparison.OrdinalIgnoreCase) == 0)
								{
									if (!cults[i].IsNeutralCulture)
									{
										lcidToFind = cults[i].LCID;
										break;
									}
								}
							}
						}
					}
				}

				if (lcids != null)
				{
					// cerco nell'array di possibili LCID compatibili con quello uguale al mio
					for (int i = 0; i < lcids.Length; i++)
					{
						if (lcidToFind == lcids[i])
						{
							databaseCulture = lcids[i];
							break;
						}
					}
				}

				// se la DatabaseCulture è sempre uguale a zero allora considero la prima culture non neutral
				// che si avvicina di più all'iso stato.
				if (DatabaseCulture == 0)
				{
					if (cults == null)
						cults = CultureInfo.GetCultures(CultureTypes.AllCultures);

					for (int i = 0; i < cults.Length; i++)
					{
						if (string.Compare(cults[i].TwoLetterISOLanguageName, this.IsoState, StringComparison.OrdinalIgnoreCase) == 0)
						{
							if (!cults[i].IsNeutralCulture)
							{
								databaseCulture = cults[i].LCID;
								break;
							}
						}
					}
				}
			}*/
		}
		# endregion

		/// <summary>
		/// Se esiste la tabella TB_DBMark controlla se i valori :
		/// - se è applicato il charset Unicode sulla colonna Status e il corrispondente flag sulla MSD_Companies
		/// - se la collation del database corrisponde a quella memorizzata sulla MSD_Companies
		/// - se il flag SupportColumnCollation corrisponde a quello memorizzato sulla MSD_Companies
		/// </summary>
		//---------------------------------------------------------------------------
		public void CheckCompanyParameterSetting()
		{
			try
			{
                if (dbType == DBMSType.POSTGRE)
					return;

				//-------------------------------
				// CONTROLLO FLAG UNICODE
				//-------------------------------
				// istanzio TBDatabaseSchema sulla connessione
				TBDatabaseSchema mySchema = new TBDatabaseSchema(this.Connection);
				// se la tabella di riferimento TB_DBMark esiste, restituisco il valore unicode impostato dall'utente
				// altrimenti procedo con il controllo sulla tabella....
				if (mySchema.ExistTable(DatabaseLayerConsts.TB_DBMark))
				{
					// analizzo lo schema della tabella e verifico il tipo della colonna Application
					TBTable cols = mySchema.GetTableSchema(DatabaseLayerConsts.TB_DBMark, false);

					foreach (TBColumn col in cols.Columns)
					{
						// devo controllare la colonna Status perchè non segue le regole latine
						if (string.Compare(col.Name, "Status", StringComparison.OrdinalIgnoreCase) == 0)
						{
							TBType providerType = TBDatabaseType.GetTBType(dbType, col.DataType);
							dbUnicodeFromDB = string.Compare(TBDatabaseType.GetDBDataType(providerType, dbType), "NChar", StringComparison.OrdinalIgnoreCase) == 0;
							break;
						}
					}
				}

				if (dbUnicodeFromDB != UseUnicode)
					InconsistentSetting = true;

				//-------------------------------
				// CONTROLLO DATABASE CULTURE
				//-------------------------------
				// se la DatabaseCulture è uguale a zero viene gestito in fase di aggiornamento
				/*if (DatabaseCulture != 0)
				{
					// devo controllare la compatibilità tra la collate presente sul db e quella memorizzata sulla
					// MSD_Companies. Se diverse devo impostare quella del database, e visualizzando un opportuno messaggio
					dbCultureFromDB = DBGenericFunctions.AssignDatabaseCultureValue(this.IsoState, this.connectAzDB, dbType, dbUnicodeFromDB);

					// se l'LCID letto estrapolato dall'isostato e' 0  nel dubbio gli assegno quello letto dal db
					if (dbCultureFromDB == 0)
						dbCultureFromDB = DatabaseCulture;
					else if (dbCultureFromDB != DatabaseCulture)
						InconsistentSetting = true;
				}

				// devo calcolare se supporta quella letta direttamente dal db aziendale e poi la confronto con quelle memorizzata nel sysdb
				supportColsCollationFromDB =
					DBGenericFunctions.CalculateSupportColumnCollation(this.connectAzDB, dbCultureFromDB, dbType, dbUnicodeFromDB);

				if (supportColsCollationFromDB != supportColumnCollation)
					InconsistentSetting = true;*/
			}
			catch (TBException)
			{ }

			// se uno dei controlli ha rilevato qualche errore compongo la stringa di errore da visualizzare all'utente
			if (InconsistentSetting)
			{
				messageInconsistentSetting = DatabaseManagerStrings.InconsistentSettings1 + "\r\n";
				messageInconsistentSetting += DatabaseManagerStrings.InconsistentSettings2 + "\r\n";
				messageInconsistentSetting += (dbType != DBMSType.ORACLE)
						? DatabaseManagerStrings.InconsistentSettings3 : DatabaseManagerStrings.InconsistentSettings4;
			}
		}

		/// <summary>
		/// Metodo che esegue il riallineamento dei parametri della msd_companies, qualora essi siano discordanti
		/// con quelli letti dalle impostazioni del database (nello specifico si tratta dei valori unicode e il tipo
		/// di collate applicata alle colonne)
		/// </summary>
		/// <returns>successo dell'operazione</returns>
		//---------------------------------------------------------------------------
		public bool LineUpCompanyParametersSetting()
		{
			bool success = false;

			if (string.IsNullOrEmpty(connectSysDB))
				return success;

			try
			{
				string query = @"UPDATE MSD_Companies SET UseUnicode = @unicode, DatabaseCulture = @dbculture, 
								SupportColumnCollation = @supportCollation WHERE CompanyId = @companyId";

				// apro al volo una connessione al db di sistema
				using (SqlConnection mySysDBConnection = new SqlConnection(connectSysDB))
				{
					mySysDBConnection.Open();

					SqlCommand sqlCommand = new SqlCommand(query, mySysDBConnection);
					sqlCommand.Parameters.AddWithValue("@unicode", dbUnicodeFromDB);
					sqlCommand.Parameters.AddWithValue("@dbculture", dbCultureFromDB);
					sqlCommand.Parameters.AddWithValue("@supportCollation", supportColsCollationFromDB);
					sqlCommand.Parameters.AddWithValue("@companyId", Convert.ToInt32(companyId));
					sqlCommand.ExecuteNonQuery();
					success = true;

					// se l'assegnazione è andata a buon fine devo assegnare i nuovi valori alle variabili del ContextInfo
					// visto che vengono poi utilizzate nelle funzioni dell'aggiornamento db
					this.UseUnicode = dbUnicodeFromDB;
					this.databaseCulture = dbCultureFromDB;
					this.supportColumnCollation = supportColsCollationFromDB;
				}
			}
			catch (SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "LineUpCompanyParametersSetting");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorUpdatingSystemDB, extendedInfo);
			}

			return success;
		}

		# region GESTIONE DATABASE DOCUMENTALE
		/// <summary>
		/// Join sulle tabelle del database di sistema per avere le info per comporre 
		/// la stringa di connessione al database documentale
		/// </summary>
		/// <returns>se la query è andata a buon fine</returns>
		//---------------------------------------------------------------------------
		private bool QuerySysDatabaseForDms()
		{
			bool result = true;

			if (Connection == null || Connection.State != ConnectionState.Open)
				return result;

			try
			{
				// faccio la join tra le tabelle del db di sistema per avere poi a disposizione
				// i valori per comporre la stringa di connessione al database documentale
				string query = @"	SELECT MSD_CompanyDBSlaves.SlaveId, MSD_CompanyDBSlaves.ServerName,
									MSD_CompanyDBSlaves.DatabaseName, MSD_CompanyDBSlaves.Signature,
									MSD_SlaveLogins.SlaveDBUser, MSD_SlaveLogins.SlaveDBPassword,
									MSD_SlaveLogins.SlaveDBWindowsAuthentication,
									MSD_Companies.UseUnicode, MSD_Companies.DatabaseCulture, MSD_Companies.SupportColumnCollation
							FROM	MSD_CompanyDBSlaves, MSD_SlaveLogins, MSD_Companies
							WHERE	MSD_CompanyDBSlaves.CompanyId = @CompanyId AND
									MSD_CompanyDBSlaves.SlaveDBOwner = MSD_SlaveLogins.LoginId AND
									MSD_CompanyDBSlaves.SlaveId = MSD_SlaveLogins.SlaveId AND
									MSD_Companies.CompanyId = MSD_CompanyDBSlaves.CompanyId";

				TBCommand command = new TBCommand(query, Connection);
				command.Parameters.Add("@CompanyId", Convert.ToInt32(companyId));

				using (IDataReader dataReader = command.ExecuteReader())
				{
					while (dataReader.Read())
					{
						dmsSlaveId = dataReader["SlaveId"].ToString();
						dmsServerName = dataReader["ServerName"].ToString();
						dmsDatabaseName = dataReader["DatabaseName"].ToString();
						dmsSignature = dataReader["Signature"].ToString();
						dmsSlaveDBUser = dataReader["SlaveDBUser"].ToString();
						dmsSlaveDBPw = Crypto.Decrypt(dataReader["SlaveDBPassword"].ToString());
						dmsSlaveDBWinAuth = (bool)dataReader["SlaveDBWindowsAuthentication"];

						UseUnicode = (bool)dataReader["UseUnicode"]; // da togliere?
						databaseCulture = Convert.ToInt32(dataReader["DatabaseCulture"]); // da togliere?
						supportColumnCollation = (bool)dataReader["SupportColumnCollation"]; // da togliere?
					}
				}

				// per ora non chiedo la password e me la leggo da database
				string pw = dmsSlaveDBPw;

				// se sono in modalita' silente (ovvero sto creando un'azienda demo) non chiedo le credenziali
				// di verifica all'utente e considero la password salvata su database
/*				if (askCredential)
				{
					pw = UserImpersonification();

					if (string.Compare(pw, "-1", StringComparison.InvariantCultureIgnoreCase) == 0)
						return false;
				}
*/
				connectDmsDB = TBDatabaseType.GetConnectionString
						(dmsServerName, dmsSlaveDBUser, dmsSlaveDBPw, dmsDatabaseName, DBMSType.SQLSERVER, dmsSlaveDBWinAuth, 0);

				result = true;
			}
			catch (TBException e)
			{
				CloseConnection();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "QuerySysDatabaseForDms");
				extendedInfo.Add(DatabaseManagerStrings.Library, "TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrQuerySystemDB, extendedInfo);
				result = false;
			}

			return result;
		}

		/// <summary>
		/// funzione esposta per forzare la chiusura della connessione aperta o appesa
		/// </summary>
		//---------------------------------------------------------------------
		public void CloseDmsConnection()
		{
			if (DmsConnection != null &&
				(DmsConnection.State == ConnectionState.Open || DmsConnection.State == ConnectionState.Broken))
			{
				DmsConnection.Close();
				DmsConnection.Dispose();
			}
		}

		///<summary>
		/// Per sapere se esiste uno slave con Signature="DMS" associato al companyId
		///</summary>
		//---------------------------------------------------------------------------
		private bool ExistDmsSlaveForCompany()
		{
			bool exist = false;

			if (Connection == null || Connection.State != ConnectionState.Open)
				return exist;

			try
			{
				string query = "SELECT COUNT(*) FROM MSD_CompanyDBSlaves WHERE CompanyId = @CompanyId AND Signature = 'DMS'";

				TBCommand command = new TBCommand(query, Connection);
				command.Parameters.Add("@CompanyId", Convert.ToInt32(companyId));
				exist = command.ExecuteTBScalar() > 0;
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "ExistDmsSlaveForCompany");
				extendedInfo.Add(DatabaseManagerStrings.Library, "TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrQuerySystemDB, extendedInfo);
				exist = false;
			}
			return exist;
		}
		# endregion
	}
}
