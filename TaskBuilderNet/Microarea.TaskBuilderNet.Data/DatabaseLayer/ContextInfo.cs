using System;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.OracleDataAccess;
using Microarea.TaskBuilderNet.Data.PostgreDataAccess;
using Microarea.TaskBuilderNet.Data.SQLDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
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
		private UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
		private TransactSQLAccess transactSQLAccess = null;
		public SystemDBConnectionInfo SysDBConnectionInfo = new SystemDBConnectionInfo();

		private OracleAccess oracleAccess = null;
		private OracleUserImpersonatedData oracleDataToConnectionServer = new OracleUserImpersonatedData();
		private OracleUserImpersonatedData impersonateOracleUser = null;
		private OracleUserImpersonatedData oracleAdmin = null;

        private PostgreAccess postgreAccess = null;
        private UserImpersonatedData postgreDataToConnectionServer = new UserImpersonatedData();

		private DBMSType dbType = DBMSType.SQLSERVER;
		private string schemaOwner = string.Empty;
		private string companyId = string.Empty;

		private string connectSysDB = string.Empty;
		private string connectAzDB = string.Empty;

		private PathFinder pathFinder = null;

		private bool isDevelopmentVersion = false; // gestione rewind

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
		public bool IsDevelopmentVersion { get { return isDevelopmentVersion; } }

		#endregion

		#region Delegates and Events
		//---------------------------------------------------------------------
		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;
		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;
		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;
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

			// devo controllare se si tratta di un'installazione di tipo Development 
			CheckDevelopmentVersion();
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
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "OpenSysDBConnection");
				extendedInfo.Add(DatabaseLayerStrings.Library, "TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrQuerySystemDB, extendedInfo);
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
				string query = @"	SELECT  MSD_Companies.Company, MSD_Companies.CompanyDBServer, MSD_Companies.Port,
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
							dbPw = Crypto.Decrypt(dataReader["DBPassword"].ToString());
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

				// se sono in modalita' silente (ovvero sto creando un'azienda demo) non chiedo le credenziali
				// di verifica all'utente e considero la password salvata su database
				if (askCredential)
				{
					// devo comporre la stringa di connessione sulla base della password che ha
					// inserito l'utente al momento della richiesta delle credenziali
					switch (dbType)
					{
						case DBMSType.SQLSERVER:
							pw = UserImpersonification();
							break;

						case DBMSType.POSTGRE:
							pw = UserImpersonificationPostgre();
							break;

						case DBMSType.ORACLE:
							pw = OracleUserImpersonification();
							break;
					}

					if (string.Compare(pw, "-1", StringComparison.InvariantCultureIgnoreCase) == 0)
						return false;
				}

				connectAzDB = TBDatabaseType.GetConnectionString(companyDBServer, dbUser, pw, CompanyDBName, Provider, dbWinAut, false, port);

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
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CreateCompanyConnectionString");
				extendedInfo.Add(DatabaseLayerStrings.Library, "TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrQuerySystemDB, extendedInfo);
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
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "MakeCompanyConnection");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectCompanyDB, extendedInfo);
				return false;
			}

			//****** CONTROLLI SMALLNETWORK: versione db e sua dimensione
			// se è la licenza è SmallNetwork il db deve essere MSDE o SqlExpress
			// e se sto associando un db esistente devo controllare che non ecceda la dimensione di 2GB
			// (controllo solo per SQL Server)
			if (DBNetworkType == DBNetworkType.Small && Connection.IsSqlConnection())
			{
			/*	 if (TBCheckDatabase.GetDatabaseVersion(Connection) != DatabaseVersion.MSDE)
				{
					CloseConnection();
					diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.WrongSQLDatabaseVersion);
					return false;
				}*/
                // 6176 Non PIU 04/02/2016 solo check dimensione e non db

				if (InstallationData.CheckDBSize && Functions.IsDBSizeOverMaxLimit(Connection.SqlConnect))
				{
					CloseConnection();
					diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.DBSizeError);
					return false;
				}
			}

			// se la DatabaseCulture dell'azienda è uguale a zero significa che non è stata ancora impostata
			// pertanto cerco di trovare la prima compatibile
			if (DatabaseCulture == 0)
				AssignDatabaseCultureValue();

			return true;
		}
		# endregion

		# region Composizione stringa connessione db aziendale (richiamata per il restore db dallo Scheduler e TestManagerAdminPlugIn)
		/// <summary>
		/// Dato il CompanyId e la LoginId del task dello Scheduler che effettua il Restore del database
		/// estrapolo le informazioni dal db di sistema per comporre al volo la stringa di connessione
		/// al db aziendale comprensiva della password non criptata.
		/// E' stato fatto un metodo apposito per evitare di chiedere all'utente la password dell'utente.
		/// </summary>
		//---------------------------------------------------------------------------
		public string ComposeCompanyConnectionStringForRestore(string companyId, string loginId, out bool companyUseUnicode)
		{
			return ComposeCompanyConnectionStringForRestore(companyId, loginId, out companyUseUnicode, false);
		}

		/// <summary>
		/// ComposeCompanyConnectionStringForRestore
		/// Overload utilizzata espressamente dal TestManagerAdminPlugIn (al suo interno valorizza le variabili di
		/// ContextInfo che mi servono successivamente)
		/// </summary>
		//---------------------------------------------------------------------------
		public string ComposeCompanyConnectionStringForRestore
			(
			string companyId,
			string loginId,
			out bool companyUseUnicode,
			bool fromTestPlanAdmin
			)
		{
			string companyDBName = string.Empty;
			string companyDBServer = string.Empty;
			int companyDBOwner = -1;
			string companyDBOwnerLogin = string.Empty;
			string companyDBOwnerPwd = string.Empty;
			bool companyDBOwnerIsNT = false;

			string companyName = string.Empty;
			bool companyUseAuditing = false;
			int companyDBCulture = 0;
			bool companySupportColCollation = false;

			companyUseUnicode = false;

			ComposeSystemDBConnectionString();

			if (string.IsNullOrEmpty(connectSysDB))
				return string.Empty;

			SqlConnection connection = null;
			SqlCommand selectCompanyDataCommand = null;
			SqlDataReader companyDataReader = null;
			SqlCommand selectCompanyLoginDataCommand = null;
			SqlDataReader companyLoginDataReader = null;

			try
			{
				connection = new SqlConnection(connectSysDB);
				connection.Open();

				string selectQuery = "SELECT ";
				selectQuery += " MSD_Companies.CompanyDBName,";
				selectQuery += " MSD_Companies.CompanyDBServer,";
				selectQuery += " MSD_Companies.CompanyDBOwner,";
				selectQuery += " MSD_Companies.Company,";
				selectQuery += " MSD_Companies.UseAuditing,";
				selectQuery += " MSD_Companies.DatabaseCulture,";
				selectQuery += " MSD_Companies.SupportColumnCollation,";
				selectQuery += " MSD_Companies.UseUnicode";
				selectQuery += " FROM MSD_Companies WHERE MSD_Companies.CompanyId =" + companyId;

				selectCompanyDataCommand = new SqlCommand(selectQuery, connection);
				companyDataReader = selectCompanyDataCommand.ExecuteReader();

				if (companyDataReader != null && companyDataReader.Read())
				{
					companyDBName = (companyDataReader["CompanyDBName"] != System.DBNull.Value)
										? (string)companyDataReader["CompanyDBName"]
										: string.Empty;
					companyDBServer = (companyDataReader["CompanyDBServer"] != System.DBNull.Value)
										? (string)companyDataReader["CompanyDBServer"]
										: string.Empty;
					companyDBOwner = (companyDataReader["CompanyDBOwner"] != System.DBNull.Value)
										? (int)companyDataReader["CompanyDBOwner"]
										: -1;
					companyName = (companyDataReader["Company"] != System.DBNull.Value)
										? (string)companyDataReader["Company"]
										: string.Empty;

					companyUseAuditing = (companyDataReader["UseAuditing"] != System.DBNull.Value)
										? (bool)companyDataReader["UseAuditing"]
										: false;
					companyDBCulture = (companyDataReader["DatabaseCulture"] != System.DBNull.Value)
										? Convert.ToInt32(companyDataReader["DatabaseCulture"].ToString())
										: 0;
					companySupportColCollation = (companyDataReader["SupportColumnCollation"] != System.DBNull.Value)
												? (bool)companyDataReader["SupportColumnCollation"]
												: false;
					companyUseUnicode = (companyDataReader["UseUnicode"] != System.DBNull.Value)
										? (bool)companyDataReader["UseUnicode"]
										: false;
				}

				companyDataReader.Close();

				if (companyDBOwner >= 0)
				{
					selectQuery = "SELECT ";
					selectQuery += " MSD_CompanyLogins.DBUser,";
					selectQuery += " MSD_CompanyLogins.DBPassword,";
					selectQuery += " MSD_CompanyLogins.DBWindowsAuthentication";
					selectQuery += " FROM MSD_CompanyLogins WHERE MSD_CompanyLogins.CompanyId =" + companyId;
					selectQuery += " AND MSD_CompanyLogins.LoginId =" + companyDBOwner.ToString();

					selectCompanyLoginDataCommand = new SqlCommand(selectQuery, connection);
					companyLoginDataReader = selectCompanyLoginDataCommand.ExecuteReader();

					if (companyLoginDataReader != null && companyLoginDataReader.Read())
					{
						companyDBOwnerLogin = (companyLoginDataReader["DBUser"] != System.DBNull.Value)
												? (string)companyLoginDataReader["DBUser"]
												: string.Empty;
						companyDBOwnerPwd = (companyLoginDataReader["DBPassword"] != System.DBNull.Value)
												? Crypto.Decrypt((string)companyLoginDataReader["DBPassword"])
												: string.Empty;
						companyDBOwnerIsNT = (companyLoginDataReader["DBWindowsAuthentication"] != System.DBNull.Value)
												? (bool)companyLoginDataReader["DBWindowsAuthentication"]
												: false;
					}

					companyLoginDataReader.Close();
				}
			}
			catch (SqlException exception)
			{
				Debug.WriteLine(exception.Message);
				return string.Empty;
			}
			finally
			{
				if (companyDataReader != null && !companyDataReader.IsClosed)
					companyDataReader.Close();

				if (selectCompanyDataCommand != null)
					selectCompanyDataCommand.Dispose();

				if (companyLoginDataReader != null && !companyLoginDataReader.IsClosed)
					companyLoginDataReader.Close();

				if (selectCompanyLoginDataCommand != null)
					selectCompanyLoginDataCommand.Dispose();

				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}

			string connectToCompanyDB =
				(companyDBOwnerIsNT)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, companyDBServer, companyDBName)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, companyDBServer, companyDBName, companyDBOwnerLogin, companyDBOwnerPwd);

			// se la funzione è richiamata dal TestPlanAdminPlugIn allora devo valorizzare 'a mano' i datamember di
			// classe, per evitare di chiedere le credenziali all'utente
			if (fromTestPlanAdmin)
			{
				this.companyName = companyName;
				this.companyDBServer = companyDBServer;
				this.companyDBName = companyDBName;
				dbUser = companyDBOwnerLogin;
				dbPw = companyDBOwnerPwd;
				dbWinAut = companyDBOwnerIsNT;
				UseUnicode = companyUseUnicode;
				useAuditing = companyUseAuditing;
				schemaOwner = companyDBOwnerLogin;
				databaseCulture = companyDBCulture;
				supportColumnCollation = companySupportColCollation;
			}

			return connectToCompanyDB;
		}
		# endregion

		# region Eventi gestione Impersonificazione utenti alla Console
		/// <summary>
		/// AddUserAuthentication
		/// Evento verso ApplicationDbAdmin per dire alla Console di aggiungere l'utente 
		/// alla lista degli utenti autenticati
		/// </summary>
		//---------------------------------------------------------------------
		private void AddUserAuthentication(string login, string password, string serverName, DBMSType dbType)
		{
			if (OnAddUserAuthenticatedFromConsole != null)
				OnAddUserAuthenticatedFromConsole(login, password, serverName, dbType);
		}

		/// <summary>
		/// GetUserAuthenticatedPwd
		/// Evento verso ApplicationDbAdmin per chiedere alla console la pwd dell'utente 
		/// precedentemente autenticato
		/// </summary>
		//---------------------------------------------------------------------
		private string GetUserAuthenticatedPwd(string login, string serverName)
		{
			string pwd = string.Empty;
			if (OnGetUserAuthenticatedPwdFromConsole != null)
				pwd = OnGetUserAuthenticatedPwdFromConsole(login, serverName);
			return pwd;
		}

		/// <summary>
		/// IsUserAuthenticated
		/// Evento verso ApplicationDbAdmin per chiedere alla Console se l'utente è autenticato oppure no
		/// </summary>
		//---------------------------------------------------------------------
		private bool IsUserAuthenticated(string login, string password, string serverName)
		{
			bool result = false;
			if (OnIsUserAuthenticatedFromConsole != null)
				result = OnIsUserAuthenticatedFromConsole(login, password, serverName);
			return result;
		}
		# endregion

		# region Metodi Impersonificazione (SqlServer - Oracle)
		/// <summary>
		/// prima di effettuare le operazioni sul database oracle devo richiedere le credenziali del dbo del database
		/// </summary>
		//---------------------------------------------------------------------------
		private string OracleUserImpersonification()
		{
			if (oracleAccess == null)
			{
				oracleAccess = new OracleAccess();

				oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
				oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
				oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			}

			//carico i dati dell'utente che devo impersonificare
			oracleDataToConnectionServer.Domain = Path.GetDirectoryName(dbUser);
			oracleDataToConnectionServer.Login = Path.GetFileName(dbUser);
			oracleDataToConnectionServer.Password = dbPw;
			oracleDataToConnectionServer.OracleService = companyDBServer;
			oracleDataToConnectionServer.WindowsAuthentication = dbWinAut;

			//eseguo l'impersonificazione (la 1^ volta chiede le credenziali e cmq il currentuser NT è escluso)
			impersonateOracleUser = oracleAccess.UserImpersonification(oracleDataToConnectionServer, true);
			oracleDataToConnectionServer.Territory = (impersonateOracleUser != null) ? impersonateOracleUser.Territory : string.Empty;
			oracleDataToConnectionServer.Language = (impersonateOracleUser != null) ? impersonateOracleUser.Language : string.Empty;

			if (impersonateOracleUser != null)
				return impersonateOracleUser.Password;
			else
				return "-1";
		}

#pragma warning disable 0618
		// disabilito temporaneamente warning CS0618: 'System.Data.OracleClient.OracleConnection' is obsolete: 
		// 'OracleConnection has been deprecated. http://go.microsoft.com/fwlink/?LinkID=144260'

		/// <summary>
		/// restituisce una connessione al database oracle utilizzando l'utente administrator
		/// </summary>
		//---------------------------------------------------------------------------
		public OracleConnection GetOracleDBAdminConnection()
		{
			if (oracleAccess == null)
			{
				oracleAccess = new OracleAccess();
				oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
				oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
				oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			}

			OracleUserImpersonatedData candidateAdmin = oracleAccess.LoadSystemData(companyDBServer);
			oracleAdmin = oracleAccess.AdminImpersonification(candidateAdmin);

			if (oracleAdmin == null)
				return null;

			try
			{
				oracleAccess.LoadUserData(oracleAdmin.OracleService, oracleAdmin.Login, oracleAdmin.Password, oracleAdmin.WindowsAuthentication);
				oracleAccess.OpenConnection();
			}
			catch (TBException)
			{
				if (oracleAdmin != null)
					oracleAdmin.Undo();
				return null;
			}

			return oracleAccess.CurrentConnection;
		}

		//---------------------------------------------------------------------------
		public void CloseOracleDbAdminConnection()
		{
			if (oracleAccess != null)
				oracleAccess.CloseConnection();

			if (oracleAdmin != null)
				oracleAdmin.Undo();
		}

		//--------------------------------------------------------------------------
		public void CreateSynonymOnTable(string auditTableName, DataTable usersTable)
		{
			OracleConnection oraAdminConnection = null;

			try
			{
				// dal contesto mi faccio restiture una connessione al database oracle come utente dbadmin per poter
				// effettuare la creazione dei sinonimi per tutti gli utenti utilizzatori dello schema
				oraAdminConnection = GetOracleDBAdminConnection();

				if (oraAdminConnection == null)
					return;

				TBOracleAdminFunction oraAdminFunct = new TBOracleAdminFunction(oraAdminConnection);

				foreach (DataRow row in usersTable.Rows)
					oraAdminFunct.CreateSynonymOnTable(SchemaOwner, auditTableName, DBObjectTypes.TABLE, row["DBUser"].ToString());
			}
			catch (TBException)
			{
				CloseOracleDbAdminConnection();
				throw;
			}

			CloseOracleDbAdminConnection();
		}

		//--------------------------------------------------------------------------
		public void DropSynonymOnTable(string auditTableName, DataTable usersTable)
		{
			OracleConnection oraAdminConnection = null;

			try
			{
				// dal contesto mi faccio restiture una connessione al database oracle come utente dbadmin per poter
				// effettuare la creazione dei sinonimi per tutti gli utenti utilizzatori dello schema
				oraAdminConnection = GetOracleDBAdminConnection();

				if (oraAdminConnection == null)
					return;

				TBOracleAdminFunction oraAdminFunct = new TBOracleAdminFunction(oraAdminConnection);

				foreach (DataRow row in usersTable.Rows)
					oraAdminFunct.DropSynonymOnTable(SchemaOwner, auditTableName, row["DBUser"].ToString());
			}
			catch (TBException)
			{
				CloseOracleDbAdminConnection();
				throw;
			}

			CloseOracleDbAdminConnection();
		}

#pragma warning restore 0618

		/// <summary>
		/// prima di effettuare le operazioni sul database devo richiedere le credenziali del dbo del database
		/// </summary>
		//---------------------------------------------------------------------------
		private string UserImpersonification()
		{
			string ownerDomain = Path.GetDirectoryName(dbUser);
			string ownerLogin = Path.GetFileName(dbUser);

			string[] str = companyDBServer.Split(new Char[] { Path.DirectorySeparatorChar });
			string tempDbServer = str[0].ToString();

			string tempDbIstance = string.Empty;
			if (str.Length > 1)
				tempDbIstance = str[1].ToString();

			transactSQLAccess = new TransactSQLAccess();
			transactSQLAccess.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			transactSQLAccess.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			transactSQLAccess.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);

			dataToConnectionServer = transactSQLAccess.LoginImpersonification
				(
				ownerLogin,
				//password, // password della msd_logins
				dbPw,
				ownerDomain,
				dbWinAut,
				tempDbServer,
				tempDbIstance,
				false // enableChangeCredential, x disabilitare il cambio delle credenziali
				);

			return (dataToConnectionServer != null) ? dataToConnectionServer.Password : "-1";
		}

        /// <summary>
        /// prima di effettuare le operazioni sul database devo richiedere le credenziali del dbo del database
        /// </summary>
        //---------------------------------------------------------------------------
        private string UserImpersonificationPostgre()
       {
            string ownerDomain = Path.GetDirectoryName(dbUser);
            string ownerLogin = Path.GetFileName(dbUser);

            string[] str = companyDBServer.Split(new Char[] { Path.DirectorySeparatorChar });
            string tempDbServer = str[0].ToString();

            string tempDbIstance = string.Empty;
            if (str.Length > 1)
                tempDbIstance = str[1].ToString();

            postgreAccess = new PostgreAccess();
            postgreAccess.OnAddUserAuthenticatedFromConsole += new PostgreAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
            postgreAccess.OnIsUserAuthenticatedFromConsole += new PostgreAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
            postgreAccess.OnGetUserAuthenticatedPwdFromConsole += new PostgreAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);

            postgreDataToConnectionServer = postgreAccess.LoginImpersonification
                (
                ownerLogin,
                //password, // password della msd_logins
                dbPw,
                ownerDomain,
                dbWinAut,
                tempDbServer,
                tempDbIstance,
                false, // enableChangeCredential, x disabilitare il cambio delle credenziali
                port
                );

            return (postgreDataToConnectionServer != null) ? postgreDataToConnectionServer.Password : "-1";
        }



		/// <summary>
		/// metodo visibile esternamente per effettuare l'undo della Impersonification
		/// </summary>
		//---------------------------------------------------------------------------
		public void UndoImpersonification()
		{
			if (dbType == DBMSType.SQLSERVER)
			{
				if (dataToConnectionServer != null)
					dataToConnectionServer.Undo();
			}

			if (dbType == DBMSType.ORACLE)
			{
				if (impersonateOracleUser != null)
					impersonateOracleUser.Undo();
			}

            if (dbType == DBMSType.POSTGRE)
            {
                if (impersonateOracleUser != null)
                    postgreDataToConnectionServer.Undo();
            }
		}
		# endregion

		# region SetUpdatingFlag (per impostare il flag Updating sulla MSD_Companies)
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

			SqlConnection mySysDBConnection = null;

			try
			{
				// apro al volo una connessione al db di sistema
				mySysDBConnection = new SqlConnection(connectSysDB);
				mySysDBConnection.Open();

				string query = "UPDATE MSD_Companies SET Updating = @update WHERE CompanyId = @companyId";

				SqlCommand sqlCommand = new SqlCommand(query, mySysDBConnection);
				sqlCommand.Parameters.AddWithValue("@update", isUpdating);
				sqlCommand.Parameters.AddWithValue("@companyId", Convert.ToInt32(companyId));
				sqlCommand.ExecuteNonQuery();

				// sto impostando a false il flag 'Updating' e significa che ho ultimato le modifiche di aggiornamento
				// se il database è Oracle faccio un ultimo aggiornamento della colonna SupportColumnCollation, sulla base
				// di ciò che in realtà è stato calcolato
				if (dbType == DBMSType.ORACLE && !isUpdating)
				{
					bool supportColumn =
						DBGenericFunctions.CalculateSupportColumnCollation(this.connectAzDB, DatabaseCulture, dbType, this.UseUnicode);
					query = "UPDATE MSD_Companies SET SupportColumnCollation = @support WHERE CompanyId = @idCompany";

					sqlCommand.CommandText = query;
					sqlCommand.Parameters.AddWithValue("@support", supportColumn);
					sqlCommand.Parameters.AddWithValue("@idCompany", Convert.ToInt32(companyId));
					sqlCommand.ExecuteNonQuery();
				}
			}
			catch (SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SetUpdatingFlag");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrorUpdatingSystemDB, extendedInfo);
			}
			finally
			{
				if (mySysDBConnection.State == ConnectionState.Open)
				{
					mySysDBConnection.Close();
					mySysDBConnection.Dispose();
				}
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
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SetArithAbortDbOption");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrorUpdatingSystemDB, extendedInfo);
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

			SqlConnection mySysDBConnection = null;

			try
			{
				// apro al volo una connessione al db di sistema
				mySysDBConnection = new SqlConnection(connectSysDB);
				mySysDBConnection.Open();

				string query = @"UPDATE MSD_Companies SET DatabaseCulture = @dbculture 
								WHERE CompanyId = @companyId AND DatabaseCulture = 0";

				SqlCommand sqlCommand = new SqlCommand(query, mySysDBConnection);
				sqlCommand.Parameters.AddWithValue("@dbculture", DatabaseCulture);
				sqlCommand.Parameters.AddWithValue("@companyId", Convert.ToInt32(companyId));
				sqlCommand.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SetCompanyDatabaseCulture");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrorUpdatingSystemDB, extendedInfo);
			}
			finally
			{
				if (mySysDBConnection.State == ConnectionState.Open)
				{
					mySysDBConnection.Close();
					mySysDBConnection.Dispose();
				}
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
			int[] lcids = null;
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
				case DBMSType.ORACLE:
					{
						if (impersonateOracleUser != null)
							lcids = CultureHelper.GetCompatibleLocaleIDsWithOracleLanguageAndTerritory(impersonateOracleUser.Language, impersonateOracleUser.Territory);
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
								if (string.Compare(cults[i].TwoLetterISOLanguageName, this.IsoState, StringComparison.InvariantCultureIgnoreCase) == 0)
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
						if (string.Compare(cults[i].TwoLetterISOLanguageName, this.IsoState, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							if (!cults[i].IsNeutralCulture)
							{
								databaseCulture = cults[i].LCID;
								break;
							}
						}
					}
				}
			}
		}
		#endregion

		#region Check congruenza e allineamento parametri company / database
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
                if (dbType == DBMSType.POSTGRE) return;
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
					DataTable cols = mySchema.GetTableSchema(DatabaseLayerConsts.TB_DBMark, false);

					foreach (DataRow col in cols.Rows)
					{
						// devo controllare la colonna Status perchè non segue le regole latine
						if (string.Compare(col["ColumnName"].ToString(), "Status", StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							TBType providerType = TBDatabaseType.GetTBType(dbType, (int)col["ProviderType"]);

							dbUnicodeFromDB = string.Compare
								//(col["DataTypeName"].ToString(),
                                (TBDatabaseType.GetDBDataType(providerType, dbType),
								"NChar", // in Oracle e in SQL i tipi hanno lo stesso nome
								StringComparison.InvariantCultureIgnoreCase) == 0;
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
				if (DatabaseCulture != 0)
				{
					// devo controllare la compatibilità tra la collate presente sul db e quella memorizzata sulla
					// MSD_Companies. Se diverse devo impostare quella del database, e visualizzando un opportuno messaggio
					if (dbType != DBMSType.ORACLE)
						dbCultureFromDB = DBGenericFunctions.AssignDatabaseCultureValue(this.IsoState, this.connectAzDB, dbType, dbUnicodeFromDB);
					else
						dbCultureFromDB = DBGenericFunctions.AssignOracleDatabaseCultureValue
							(
							this.pathFinder, this.IsoState,
							impersonateOracleUser.Language, impersonateOracleUser.Territory
							);

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
					InconsistentSetting = true;
			}
			catch (TBException)
			{ }

			// se uno dei controlli ha rilevato qualche errore compongo la stringa di errore da visualizzare all'utente
			if (InconsistentSetting)
			{
				messageInconsistentSetting = DatabaseLayerStrings.InconsistentSettings1 + "\r\n";
				messageInconsistentSetting += DatabaseLayerStrings.InconsistentSettings2 + "\r\n";
				messageInconsistentSetting += (dbType != DBMSType.ORACLE)
						? DatabaseLayerStrings.InconsistentSettings3 : DatabaseLayerStrings.InconsistentSettings4;
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

			SqlConnection mySysDBConnection = null;

			try
			{
				// apro al volo una connessione al db di sistema
				mySysDBConnection = new SqlConnection(connectSysDB);
				mySysDBConnection.Open();

				string query = @"UPDATE MSD_Companies SET UseUnicode = @unicode, DatabaseCulture = @dbculture, 
								SupportColumnCollation = @supportCollation WHERE CompanyId = @companyId";

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
			catch (SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "LineUpCompanyParametersSetting");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrorUpdatingSystemDB, extendedInfo);
			}
			finally
			{
				if (mySysDBConnection != null && mySysDBConnection.State == ConnectionState.Open)
				{
					mySysDBConnection.Close();
					mySysDBConnection.Dispose();
				}
			}

			return success;
		}
		#endregion

		#region Check Development version
		/// <summary>
		/// Ricerca almeno un file DatabaseObjects.xml che abbia l'attributo
		/// development="true" nel nodo Release (gestione Rewind)
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckDevelopmentVersion()
		{
			foreach (ApplicationInfo ai in this.pathFinder.ApplicationInfos)
				foreach (ModuleInfo mi in ai.Modules)
					if (mi.DatabaseObjectsInfo.IsDevelopmentVersion)
					{
						// me ne basta trovare uno e ritorno
						isDevelopmentVersion = true;
						return;
					}
		}
		# endregion

		#region GESTIONE DATABASE DOCUMENTALE
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
				string query = @"	SELECT	MSD_CompanyDBSlaves.SlaveId, MSD_CompanyDBSlaves.ServerName,
									MSD_CompanyDBSlaves.DatabaseName, MSD_CompanyDBSlaves.Signature,
									MSD_SlaveLogins.SlaveDBUser, MSD_SlaveLogins.SlaveDBPassword,
									MSD_SlaveLogins.SlaveDBWindowsAuthentication,
									MSD_Companies.UseUnicode, MSD_Companies.DatabaseCulture, MSD_Companies.SupportColumnCollation
							FROM	MSD_CompanyDBSlaves, MSD_SlaveLogins, MSD_Companies
							WHERE	MSD_CompanyDBSlaves.CompanyId = @CompanyId AND
									MSD_CompanyDBSlaves.SlaveDBOwner = MSD_SlaveLogins.LoginId AND
									MSD_CompanyDBSlaves.SlaveId = MSD_SlaveLogins.SlaveId AND
									MSD_Companies.CompanyId = MSD_CompanyDBSlaves.CompanyId";

				using (TBCommand command = new TBCommand(query, Connection))
				{
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
				}

				// per ora non chiedo la password e me la leggo da database
/*				string pw = dmsSlaveDBPw;

				// se sono in modalita' silente (ovvero sto creando un'azienda demo) non chiedo le credenziali
				// di verifica all'utente e considero la password salvata su database
				if (askCredential)
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
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "QuerySysDatabaseForDms");
				extendedInfo.Add(DatabaseLayerStrings.Library, "TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrQuerySystemDB, extendedInfo);
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
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "ExistDmsSlaveForCompany");
				extendedInfo.Add(DatabaseLayerStrings.Library, "TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrQuerySystemDB, extendedInfo);
				exist = false;
			}
			return exist;
		}
		# endregion
	}
}
