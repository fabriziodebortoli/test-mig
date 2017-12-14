using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Microarea.Console.Core.DBLibrary;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.SQLDataAccess;
using Microarea.TaskBuilderNet.Interfaces;
using System;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// CheckCompany.
	/// Data una company, esegue una verifica dei dati, in particolare esistenza database di una azienda, 
	/// dbowner e UserName associati (ci deve essere congruenza con ciò che è stato impostato nelle
	/// tabelle del SYSTEMDB dell'AdministrationConsole)
	/// </summary>
	//=========================================================================
	public partial class CheckCompany : PlugInsForm
	{
		#region Private variables
		private SqlConnection		sysDBConnection	= null;
		private TransactSQLAccess	sqlAccess		= new TransactSQLAccess();
		private string				sysDBConnectionString = string.Empty;

		private TBConnection companyConnection	= null;
		private string companyId				= string.Empty;
		private string companyName				= string.Empty;
		private string companyConnectionString	= string.Empty;

		private	CompanyItem	companyItem			= new CompanyItem();
		private CompanyUser dboItem             = new CompanyUser();
		private ArrayList	companyUsersItem	= new ArrayList();
		private Diagnostic  diagnostic          = new Diagnostic("SysAdminPlugIn.CheckCompany");
		private string		isoState			= string.Empty;

		private ListViewItem selItem = null;
		private int	count = -1;
		#endregion

		#region Events and Delegates
		public delegate void EnableProgressBar(object sender);
		public event EnableProgressBar OnEnableProgressBar;
		public delegate void DisableProgressBar(object sender);
		public event DisableProgressBar OnDisableProgressBar;
		public delegate void SetProgressBarStep(object sender, int step);
		public event SetProgressBarStep OnSetProgressBarStep;
		public delegate void SetProgressBarValue(object sender, int currentValue);
		public event SetProgressBarValue OnSetProgressBarValue;
		public delegate void SetProgressBarText(object sender, string message);
		public event SetProgressBarText OnSetProgressBarText;
		public delegate void EnableConsoleTreeView(bool enable);
		public event EnableConsoleTreeView OnEnableConsoleTreeView;

		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;

		// gestione autenticazione utente
		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;
		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;
		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;

		// evento al SysAdmin che rimbalza all'ApplicationDBAdmin per il controllo della struttura del db
		public delegate Diagnostic CheckDatabaseStructure(string companyId);
		public event CheckDatabaseStructure OnCheckDatabaseStructure;
		#endregion

		#region Constructor
		/// <summary>
		/// CheckCompany
		/// Inizializza la listview, setta le immagini e alcuni default sulla form (TabIndex, connessione, ..)
		/// </summary>
		//---------------------------------------------------------------------
		public CheckCompany
			(
			string connectionStr,
			SqlConnection connection, 
			string companyId, 
			string companyName,
			ImageList stateImageList,
			string isoState
			)
		{
			InitializeComponent();
			
			this.companyId = companyId;
			this.companyName = companyName;
			sysDBConnectionString	= connectionStr;
			sysDBConnection			= connection;

			this.isoState	= isoState;
			this.TabIndex	= 3;
			this.TabStop	= true;
			InitProgressCheck();
			ProgressCheck.LargeImageList = ProgressCheck.SmallImageList = stateImageList;

			// eseguo l'elaborazione subito, senza cliccare sul pulsante
			StartCheckProcess();
		}
		#endregion

		#region InitProgressCheck - Inizializza la finestra per l'avanzamento del check
		/// <summary>
		/// InitProgressCheck
		/// Inizializza la finestra per l'avanzamento del check
		/// </summary>
		//---------------------------------------------------------------------
		private void InitProgressCheck()
		{
			ProgressCheck.TabIndex	= 3;
			ProgressCheck.TabStop	= false;
			ProgressCheck.View		= View.Details;
			// La prima colonna è vuota, ci metterò l'immagine
			ProgressCheck.Columns.Add(string.Empty,	20, HorizontalAlignment.Left);
			ProgressCheck.Columns.Add(Strings.ActionCheck, 670, HorizontalAlignment.Left);
		}
		#endregion

		#region LoadInformationFromSysDB - Carica i dati relativi all'azienda, al dbo e utenti dal db di sistema
		/// <summary>
		/// LoadInformationFromSysDB - Carica i dati relativi alla company, al dbowner e ai suoi utenti
		/// </summary>
		//---------------------------------------------------------------------
		private bool LoadInformationFromSysDB()
		{
            AddItemInListView(Strings.LoadingInfoFromSysDB, PlugInTreeNode.GetInformationStateImageIndex);

			// leggo i dati relativi alla company
			CompanyDb companyDb				= new CompanyDb();
			companyDb.ConnectionString		= sysDBConnectionString;
			companyDb.CurrentSqlConnection	= sysDBConnection;
			ArrayList company				= new ArrayList();

			if (!companyDb.GetAllCompanyFieldsById(out company, companyId))
			{
				if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
					diagnostic.Set(companyDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				company.Clear();
				return false;
			}

			if (company.Count == 0)
				return false;

			companyItem = (CompanyItem)company[0];
			if (companyItem == null)
				return false;

			// leggo i dati relativi all'utente dbo del database della company (dalla MSD_CompanyLogins)
			CompanyUserDb companyUsersDb = new CompanyUserDb();
			companyUsersDb.ConnectionString = sysDBConnectionString;
			companyUsersDb.CurrentSqlConnection = sysDBConnection;
			
			ArrayList userDbo = new ArrayList();
			companyUsersDb.GetUserCompany(out userDbo, companyItem.DbOwner, companyItem.CompanyId);
			if (userDbo.Count > 0)
				dboItem = (CompanyUser)userDbo[0];
			
			// Leggo le logins abilitate a connettersi al database
			ArrayList companyUsers = new ArrayList();
			if (!companyUsersDb.SelectAll(out companyUsers, companyId))
			{
				if (companyUsersDb.Diagnostic.Error || companyUsersDb.Diagnostic.Warning || companyUsersDb.Diagnostic.Information)
					diagnostic.Set(companyUsersDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading);

				DiagnosticViewer.ShowDiagnostic(diagnostic);
				companyUsers.Clear();
			}

			if (companyUsers.Count > 0)
			{
				for (int i = 0; i < companyUsers.Count; i++)
					companyUsersItem.Add((CompanyUser)companyUsers[i]);
			}

			if (companyItem == null || dboItem == null)
				return false;

			// compongo la stringa di connessione per connettermi al database aziendale
			if (dboItem.DBWindowsAuthentication)
				companyConnectionString = 
					string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, companyItem.DbServer, companyItem.DbName);
			else
				companyConnectionString = 
					string.Format(NameSolverDatabaseStrings.SQLConnection, companyItem.DbServer, companyItem.DbName, dboItem.DBDefaultUser, dboItem.DBDefaultPassword);

			return ConnectToMasterDB();
		}
		#endregion

		# region ConnectToMasterDB - apre la connessione al db master
		/// <summary>
		/// ConnectToMasterDB
		/// valorizza il puntatore alla classe TransactSQLAccess con tutti i valori e apre una connessione
		/// al database master, dal quale verranno estrapolate varie informazioni
		/// </summary>
		/// <returns>true se la connessione è aperta</returns>
		//---------------------------------------------------------------------
		private bool ConnectToMasterDB()
		{
            AddItemInListView(string.Format(Strings.ConnectToMasterDB, companyItem.DbServer), PlugInTreeNode.GetInformationStateImageIndex);

			sqlAccess.OnAddUserAuthenticatedFromConsole	 += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			sqlAccess.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			sqlAccess.OnIsUserAuthenticatedFromConsole	 += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			
			sqlAccess.CurrentStringConnection = 				
				(dboItem.DBWindowsAuthentication)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, companyItem.DbServer, companyItem.DbName /*DatabaseLayerConsts.MasterDatabase*/)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, companyItem.DbServer, companyItem.DbName, /*DatabaseLayerConsts.MasterDatabase,*/ dboItem.DBDefaultUser, dboItem.DBDefaultPassword);

			bool result = sqlAccess.TryToConnect();

			if (!result)
			{
				if (sqlAccess.Diagnostic.Error || sqlAccess.Diagnostic.Warning || sqlAccess.Diagnostic.Information)
				{
					diagnostic.Set(sqlAccess.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
				}
			}

			return result;
		}
		# endregion

		#region CheckData - esecuzione dei vari controlli sul server SQL e database
		/// <summary>
		/// CheckData
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckData()
		{
			// leggo alcune proprietà del server SQL:
			// 1. versione
			// 2. collation di default
			// 3. edition
			ReadSQLServerProperties();

			// controllo prima se il database associato all'azienda esiste sul server
			if (CheckIfDbExist())
			{
				// leggo alcune proprietà del database:
				// 1. collation di default
				// 2. status
				ReadDatabaseProperties();

				// controllo la congruenza del flag Unicode e dei valori della DatabaseCulture
				CheckCompanyAndDBValues();

				// visualizzo se l'azienda risulta in Updating
				if (companyItem.Updating)
					AddItemInListView(Strings.CompanyHasSetUpdating, PlugInTreeNode.GetResultRedStateImageIndex, Color.Red);

				// visualizzo se l'azienda usa EasyAttachment
				if (companyItem.UseDBSlave)
					AddItemInListView(Strings.CompanyUseEasyAttachment, PlugInTreeNode.GetResultGreenStateImageIndex);

				// effettuo controlli sugli utenti e le login
				CheckDbOwner();
				CheckDbUsers();

				// controllo la struttura del database e faccio la lista delle operazioni da effettuare
				// tramite un evento chiamo l'ApplicationDBAdmin
				if (OnCheckDatabaseStructure != null)
				{
                    AddItemInListView(Strings.CheckDatabaseStructure, PlugInTreeNode.GetInformationStateImageIndex);

					Diagnostic myDiagnostic = OnCheckDatabaseStructure(companyId);
					if (myDiagnostic != null)
						ShowMessagesInListView(myDiagnostic);
				}
			}

            AddItemInListView(Strings.EndingCompanyDbCheck, PlugInTreeNode.GetArrivalFlagStateImageIndex);
		}
		#endregion

		#region Inizio elaborazione su thread separato
		//---------------------------------------------------------------------
		private void OKButton_Click(object sender, System.EventArgs e)
		{
			StartCheckProcess();
		}

		///<summary>
		/// Lancio l'elaborazione su un thread separato
		///</summary>
		//---------------------------------------------------------------------
		public void StartCheckProcess()
		{
			Thread myThread = new Thread(new ThreadStart(InternalCheckProcess));

			// serve per evitare l'errore "DragDrop registration did not succeed" riscontrato richiamando
			// la ShowDiagnostic statica per visualizzare i messaggi di un altro thread
			myThread.SetApartmentState(ApartmentState.STA);

			// quando si istanzia un nuovo Thread bisogna assegnargli le CurrentCulture, altrimenti le
			// traduzioni in lingue differenti da quelle del sistema operativo non funzionano!!!
			myThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			myThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			myThread.Start();
		}

		//---------------------------------------------------------------------
		private void InternalCheckProcess()
		{
			count = -1;
			ClearItems();

			Cursor.Current = Cursors.WaitCursor;
			EnableCheckButton(false);
			SetConsoleTreeViewEnabled(false);

            AddItemInListView(string.Format(Strings.StartingCompanyDbCheck, companyName), PlugInTreeNode.GetInformationStateImageIndex);

			if (!LoadInformationFromSysDB())
				DiagnosticViewer.ShowCustomizeIconMessage(Strings.CannotPerformChecks, Strings.Error, MessageBoxIcon.Error);
			else
				CheckData();

			Cursor.Current = Cursors.Default; 
			EnableCheckButton(true);
			SetConsoleTreeViewEnabled(true);

			//sendo la diagnostica
			if (OnSendDiagnostic != null)
			{
				OnSendDiagnostic(this, diagnostic);
				diagnostic.Clear();
			}
		}
		#endregion

		# region ReadSQLServerProperties
		/// <summary>
		/// leggo alcune proprietà del server SQL:
		/// 1. versione (SELECT @@version)
		/// 2. collation di default (SELECT SERVERPROPERTY('Collation'))
		/// 3. edition (SELECT SERVERPROPERTY('Edition'))
		/// </summary>
		//---------------------------------------------------------------------
		private void ReadSQLServerProperties()
		{
			string property = string.Empty;

            AddItemInListView(string.Format(Strings.LoadPropertiesForServer, companyItem.DbServer), PlugInTreeNode.GetInformationStateImageIndex);

			using (SqlConnection sqlConnection = new SqlConnection(sqlAccess.CurrentStringConnection))
			{
				sqlConnection.Open();

				try
				{
					using (SqlCommand command = new SqlCommand("SELECT @@version", sqlConnection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
							if (reader.Read())
								property = (reader[""] != System.DBNull.Value) ? (string)reader[""] : string.Empty;

						property = property.Substring(0, 26);
						AddItemInListView(string.Format(Strings.Version, property), PlugInTreeNode.GetResultGreenStateImageIndex);
					}
				}
				catch (SqlException e)
				{
					AddItemInListView(Strings.ErrorDuringReadingServerVersion + e.Message, PlugInTreeNode.GetResultRedStateImageIndex);
					return;
				}

				try
				{
					using (SqlCommand command = new SqlCommand("SELECT SERVERPROPERTY('Edition')", sqlConnection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
							if (reader.Read())
								property = (reader[""] != System.DBNull.Value) ? (string)reader[""] : string.Empty;

						AddItemInListView(string.Format(Strings.EditionOfSqlServerInstance, property), PlugInTreeNode.GetResultGreenStateImageIndex);
					}

					using (SqlCommand command = new SqlCommand("SELECT SERVERPROPERTY('Collation')", sqlConnection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						if (reader.Read())
							property = (reader[""] != System.DBNull.Value) ? (string)reader[""] : string.Empty;

						// segnalo che la collation è Case Sensitive - per ovviare a problemi di query con parametri diversi
						if (property.IndexOf("_CS") > 0)
							AddItemInListView(string.Format(Strings.DefaultCollation, property) + Strings.ServerCollationCaseSensitive, PlugInTreeNode.GetDummyStateImageIndex, Color.DarkGoldenrod);
						else
							AddItemInListView(string.Format(Strings.DefaultCollation, property), PlugInTreeNode.GetResultGreenStateImageIndex);
					}
				}
				catch (SqlException e)
				{
					AddItemInListView(Strings.ErrorDuringReadingServerProperties + e.Message, PlugInTreeNode.GetResultRedStateImageIndex);
				}
			}
		}
		# endregion

		# region ReadDatabaseProperties
		/// <summary>
		/// leggo alcune proprietà del database:
		/// 1. collation di default (SELECT DATABASEPROPERTYEX('nome db', 'Collation'))
		/// 2. status (SELECT DATABASEPROPERTYEX('nome db', 'Status'))
		/// </summary>
		//---------------------------------------------------------------------
		private void ReadDatabaseProperties()
		{
			string property = string.Empty;

            AddItemInListView(string.Format(Strings.LoadPropertiesForDatabase, companyItem.DbName), PlugInTreeNode.GetInformationStateImageIndex);

			try
			{
				using (SqlConnection sqlConnection = new SqlConnection(sqlAccess.CurrentStringConnection))
				{
					sqlConnection.Open();

					using (SqlCommand command = new SqlCommand(string.Format("SELECT DATABASEPROPERTYEX('{0}', 'Collation')", companyItem.DbName), sqlConnection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
							if (reader.Read())
								property = (reader[""] != System.DBNull.Value) ? (string)reader[""] : string.Empty;
					}

					// segnalo che la collation è Case Sensitive - per ovviare a problemi di query con parametri diversi
					if (property.IndexOf("_CS") > 0)
						AddItemInListView
							(
							string.Format(Strings.DefaultCollation,
							(property.Length == 0) ? Strings.NoInformationAvailable : (property + Strings.ServerCollationCaseSensitive)),
							PlugInTreeNode.GetDummyStateImageIndex,
							Color.DarkGoldenrod);
					else
						AddItemInListView
							(
							string.Format(Strings.DefaultCollation,
							(property.Length == 0) ? Strings.NoInformationAvailable : property),
							PlugInTreeNode.GetResultGreenStateImageIndex
							);

					using (SqlCommand command = new SqlCommand(string.Format("SELECT DATABASEPROPERTYEX('{0}', 'Status')", companyItem.DbName), sqlConnection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
							if (reader.Read())
								property = (reader[""] != System.DBNull.Value) ? (string)reader[""] : string.Empty;

						AddItemInListView(string.Format(Strings.DatabaseStatus, property), PlugInTreeNode.GetResultGreenStateImageIndex);
					}
				}
			}
			catch (SqlException e)
			{
				AddItemInListView(Strings.ErrorDuringReadingDbProperties + e.Message, PlugInTreeNode.GetResultRedStateImageIndex);
			}
		}
		# endregion

		#region CheckIfDbExist - Step 1 - Verifico l'esistenza del db
		/// <summary>
		/// CheckIfDbExist
		/// Step1 - Verifico l'esistenza del db
		/// Se fallisco, non vado avanti
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckIfDbExist()
		{
            AddItemInListView(Strings.CheckCompanyDb, PlugInTreeNode.GetInformationStateImageIndex);

			SetConsoleProgressBarValue(this, 1); //Abilito la progressBar
			SetConsoleProgressBarText(this, Strings.ProgressDuringCheck);
			SetConsoleProgressBarStep(this, 5);
			EnableConsoleProgressBar(this);

			bool successCheck = false;

			if (sqlAccess.ExistDataBase(companyItem.DbName))
			{
				// se il database aziendale esiste apro una connessione al volo
				try
				{
					companyConnection = new TBConnection(companyConnectionString, DBMSType.SQLSERVER);
					companyConnection.Open();
					successCheck = true;
				}
				catch (TBException e)
				{
					diagnostic.SetError(e.Message);
					DiagnosticViewer.ShowDiagnostic(diagnostic);

					if (companyConnection != null && companyConnection.State != ConnectionState.Closed)
					{
						companyConnection.Close();
						companyConnection.Dispose();
					}
					successCheck = false;
				}
			}
			else
			{
				if (sqlAccess.Diagnostic.Error || sqlAccess.Diagnostic.Warning || sqlAccess.Diagnostic.Information)
				{
					diagnostic.Set(sqlAccess.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
				}
			}

			AddItemInListView
				(
				successCheck 
				? string.Format(Strings.DatabaseExistsOnServer, companyItem.DbName, companyItem.DbServer)
				: string.Format(Strings.NotExistCompanyDbDuringCheck, companyItem.DbName, companyItem.DbServer),
				successCheck ? PlugInTreeNode.GetResultGreenStateImageIndex : PlugInTreeNode.GetResultRedStateImageIndex,
				successCheck ? Color.Black : Color.Red
				);
			
			//Disabilito la progressBar
			DisableConsoleProgressBar(this);
			return successCheck;
		}
		#endregion

		#region CheckDbOwner - Step 2 - Verifico l'esistenza e congruenza del dbowner
		/// <summary>
		/// CheckDbOwner
		/// Step2 - Verifica dell'esistenza / correttezza del dbowner
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckDbOwner()
		{
            AddItemInListView(Strings.CheckDbownerCompany, PlugInTreeNode.GetInformationStateImageIndex);

			bool successCheck = false;
			string dboOfCompanyDatabase = string.Empty;

			EnableConsoleProgressBar(this);

			//-- verifico se il dbowner impostato è quello corretto
			if (sqlAccess.LoginIsDBOwnerRole(dboItem.Login))
				successCheck = true;
			else
				if (sqlAccess.CurrentDbo(dboItem.Login, companyItem.DbName, out dboOfCompanyDatabase))
				successCheck = true;

			AddItemInListView
				(
				successCheck 
				? string.Format(Strings.CompanyDboExists, dboItem.Login, companyItem.DbName)
				: string.Format(Strings.NotExistCompanyDboDuringCheck, dboOfCompanyDatabase, dboItem.Login),
				successCheck ? PlugInTreeNode.GetResultGreenStateImageIndex : PlugInTreeNode.GetResultRedStateImageIndex,
				successCheck ? Color.Black : Color.Red
				);

			DisableConsoleProgressBar(this);
		}
		#endregion

		#region CheckDbUsers - Step 3 - Verifico l'esistenza e congruenza degli Utenti associati all'Azienda
		/// <summary>
		/// CheckDbUsers
		/// Verifica l'esistenza e la congruenza degli Utenti associati al db aziendale
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckDbUsers()
		{
            AddItemInListView(Strings.CheckJoinedCompanyUsers, PlugInTreeNode.GetInformationStateImageIndex);

			string dbowner = string.Empty;
			
			EnableConsoleProgressBar(this);
			
			if ((companyUsersItem.Count == 1) && (sqlAccess.LoginIsDBOwnerRole(((CompanyUser)companyUsersItem[0]).Login)))
			{
				//non ci sono utenti assegnati
				AddItemInListView(Strings.NotExistCompanyUsersDuringCheck, PlugInTreeNode.GetResultGreenStateImageIndex);
			}
			else
			{
				for (int i = 0; i < companyUsersItem.Count; i++)
				{
					CompanyUser joinUser = (CompanyUser)companyUsersItem[i];

					if (!sqlAccess.LoginIsDBOwnerRole(joinUser.Login))
					{
						if (!sqlAccess.ExistUserIntoDb(joinUser.DBDefaultUser, companyItem.DbName))
							AddItemInListView(string.Format(Strings.NotExistCompanyUserDuringCheck, joinUser.DBDefaultUser), PlugInTreeNode.GetResultRedStateImageIndex, Color.Red);
						else
							AddItemInListView(string.Format(Strings.CompanyUserExists, joinUser.Login, joinUser.DBDefaultUser), PlugInTreeNode.GetResultGreenStateImageIndex);
					}
				}
			}

			SetConsoleProgressBarValue(this, 100);
			SetConsoleProgressBarText (this, string.Empty);
			DisableConsoleProgressBar (this);
		}
		#endregion

		# region CheckCompanyAndDBValues (controllo parametri azienda e settings su db)
		/// <summary>
		/// CheckCompanyAndDBValues
		/// Devo controllare i valori memorizzati sulla tabella MSD_Companies con quelli effettivamente
		/// presenti sul database. Ovvero:
		/// - flag Unicode
		/// - collation applicata sulle colonne
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckCompanyAndDBValues()
		{
			if (companyConnection == null || companyConnection.State != ConnectionState.Open)
				return;

			try
			{
				//-------------------------------
				// CONTROLLO FLAG UNICODE
				//-------------------------------
				bool dbUseUnicode = false;
				// istanzio TBDatabaseSchema sulla connessione
				TBDatabaseSchema mySchema = new TBDatabaseSchema(companyConnection);
				// se la tabella di riferimento TB_DBMark esiste, restituisco il valore unicode impostato dall'utente
				// altrimenti procedo con il controllo sulla tabella....
				if (mySchema.ExistTable(DatabaseLayerConsts.TB_DBMark))
				{
					// analizzo lo schema della tabella e verifico il tipo della colonna Status
					DataTable cols = mySchema.GetTableSchema(DatabaseLayerConsts.TB_DBMark, false);

					foreach (DataRow col in cols.Rows)
					{
						if (string.Compare(col["ColumnName"].ToString(), "Status", true, CultureInfo.InvariantCulture) == 0)
						{
							TBType providerType = (TBType)((SqlDbType)col["ProviderType"]);
							dbUseUnicode = 
								//string.Compare(TBDatabaseType.GetDBDataType(providerType, DBMSType.SQLSERVER), "NChar", true, CultureInfo.InvariantCulture) == 0;
								string.Compare(col["DataTypeName"].ToString(), "NChar", true, CultureInfo.InvariantCulture) == 0;								
							break;
						}
					}
				}

				if (dbUseUnicode != companyItem.UseUnicode)
				{
					AddItemInListView
						(
						string.Format(Strings.FlagUnicodeInconsistent,
						dbUseUnicode ? Strings.Uses : Strings.DoesNotUse),
						PlugInsTreeView.GetUncheckStateImageIndex, Color.Red
						);
				}

				//-------------------------------
				// CONTROLLO DATABASE CULTURE
				//-------------------------------
				int dbCultureValue = 0;
				// se la DatabaseCulture è  uguale a zero viene gestito in fase di aggiornamento
				if (companyItem.DatabaseCulture != 0)
				{
					// devo controllare la compatibilità tra la collate presente sul db e quella memorizzata sulla
					// MSD_Companies. Se diverse devo impostare quella del database, e visualizzando un opportuno messaggio
					dbCultureValue = DBGenericFunctions.AssignDatabaseCultureValue(this.isoState, companyConnectionString, DBMSType.SQLSERVER, dbUseUnicode);

					// se l'LCID letto estrapolato dall'isostato e' 0  nel dubbio gli assegno quello letto dal db
					CultureInfo ciDB = new CultureInfo(dbCultureValue != 0 ? dbCultureValue : companyItem.DatabaseCulture);
					CultureInfo ciSysDB = new CultureInfo(companyItem.DatabaseCulture);

					if (dbCultureValue != companyItem.DatabaseCulture)
						AddItemInListView
							(
							string.Format(Strings.DBCultureInconsistent, ciDB.Name, ciSysDB.Name),
							PlugInsTreeView.GetUncheckStateImageIndex, Color.Red
							);
				}

				// devo calcolare se supporta quella letta direttamente dal db aziendale e poi la confronto con quelle memorizzata nel sysdb
				bool supportColumnCollation = 
					DBGenericFunctions.CalculateSupportColumnCollation(companyConnectionString, dbCultureValue, DBMSType.SQLSERVER, dbUseUnicode);

				if (supportColumnCollation != companyItem.SupportColumnCollation)
					AddItemInListView(Strings.SupportColCollationInconsistent, PlugInsTreeView.GetUncheckStateImageIndex, Color.Red);
			}
			catch (TBException)
			{ }
			finally
			{
				if (companyConnection != null || companyConnection.State != ConnectionState.Closed)
				{
					companyConnection.Close();
					companyConnection.Dispose();
				}
			}
		}
		# endregion

		# region Metodi per la gestione cross-thread dei control grafici
		//---------------------------------------------------------------------
		private void EnableCheckButton(bool enable)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { EnableCheckButton(enable); });
				return;
			}

			OKButton.Enabled = enable;
			OKButton.Visible = enable; // il pulsante e' visibile solo se e' abilitato
		}

		//---------------------------------------------------------------------
		private void ClearItems()
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { ClearItems(); });
				return;
			}

			ProgressCheck.Items.Clear();
		}

		//---------------------------------------------------------------------
		private void AddItemInListView(string message, int imageIdx)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { AddItemInListView(message, imageIdx); });
				return;
			}

			ListViewItem item = new ListViewItem();
			count++;
			item.ImageIndex = imageIdx;
			item.SubItems.Add(message);
			ProgressCheck.Items.Add(item);
			ProgressCheck.EnsureVisible(count);
		}

		//---------------------------------------------------------------------
		private void AddItemInListView(string message, int imageIdx, Color clr)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { AddItemInListView(message, imageIdx, clr); });
				return;
			}

			ListViewItem item = new ListViewItem();
			count++;
			item.ImageIndex = imageIdx;
			item.ForeColor = clr;
			item.SubItems.Add(message);
			ProgressCheck.Items.Add(item);
			ProgressCheck.EnsureVisible(count);
		}
		# endregion

		# region Metodo per visualizzare il Diagnostico con le info dell'ApplicationDBAdminPlugIn
		/// <summary>
		/// inserisce nella list view i messaggi ritornati dall'ApplicationDBAdmin dopo aver effettuato il check
		/// della struttura del database
		/// </summary>
		//---------------------------------------------------------------------
		private void ShowMessagesInListView(Diagnostic messages)
		{
			ListViewItem listItem = null;

			IDiagnosticItems allItems = messages.AllMessages(DiagnosticType.All);

			foreach (DiagnosticItem item in allItems)
			{
				if (item.Type == DiagnosticType.LogOnFile)
					continue;

				listItem = new ListViewItem();

				if (item.Type == DiagnosticType.Error || item.Type == DiagnosticType.Warning)
                    AddItemInListView(item.FullExplain, PlugInTreeNode.GetDummyStateImageIndex);
				else
					AddItemInListView(item.FullExplain, PlugInTreeNode.GetResultGreenStateImageIndex);
			}
		}
		# endregion

		#region Eventi per Abilitare e Impostare la ProgressBar

		#region EnableConsoleProgressBar
		/// <summary>
		/// EnableConsoleProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		private void EnableConsoleProgressBar(object sender)
		{
			if (OnEnableProgressBar != null)
				OnEnableProgressBar(sender);
		}
		#endregion

		#region DisableConsoleProgressBar 
		/// <summary>
		/// DisableConsoleProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		private void DisableConsoleProgressBar(object sender)
		{
			if (OnDisableProgressBar != null)
				OnDisableProgressBar(sender);
		}
		#endregion

		#region SetConsoleProgressBarStep 
		/// <summary>
		/// SetConsoleProgressBarStep
		/// </summary>
		//---------------------------------------------------------------------
		private void SetConsoleProgressBarStep(object sender, int step)
		{
			if (OnSetProgressBarStep != null)
				OnSetProgressBarStep(sender, step);
		}
		#endregion

		#region SetConsoleProgressBarValue
		/// <summary>
		/// SetConsoleProgressBarValue
		/// </summary>
		//---------------------------------------------------------------------
		private void SetConsoleProgressBarValue(object sender, int currentValue)
		{
			if (OnSetProgressBarValue != null)
				OnSetProgressBarValue(sender, currentValue);
		}
		#endregion

		#region SetConsoleProgressBarText
		/// <summary>
		/// SetConsoleProgressBarText
		/// </summary>
		//---------------------------------------------------------------------
		private void SetConsoleProgressBarText(object sender, string message)
		{
			if (OnSetProgressBarText != null)
				OnSetProgressBarText(sender, message);
		}
		#endregion

		///<summary>
		/// Abilita / Disabilita il tree della Console
		///</summary>
		//---------------------------------------------------------------------------
		private void SetConsoleTreeViewEnabled(bool enable)
		{
			if (OnEnableConsoleTreeView != null)
				OnEnableConsoleTreeView(enable);
		}
		#endregion

		# region Eventi sulla form
		/// <summary>
		/// Inizializzazioni durante la Load
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckCompany_Load(object sender, System.EventArgs e)
		{
			LabelTitle.Text = string.Format(LabelTitle.Text, companyName);
		}

		/// <summary>
		/// CheckCompany_Closing
		/// Invio la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckCompany_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// CheckCompany_Deactivate
		/// Invio la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckCompany_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// CheckCompany_VisibleChanged
		/// Invio la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckCompany_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}

		/// <summary>
		/// ProgressCheck_MouseDown
		/// sul click del tasto destro sulla listview visualizzo un context menu x copiare il testo della colonna
		/// </summary>
		//---------------------------------------------------------------------
		private void ProgressCheck_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ListViewContextMenu.MenuItems.Clear();

			if (ProgressCheck.SelectedItems.Count == 1)
			{
				selItem = ProgressCheck.GetItemAt(e.X, e.Y);

				if (selItem != null)
				{
					switch (e.Button)
					{
						case MouseButtons.Right:// col tasto dx faccio vedere il context menu
						{
							ListViewContextMenu.MenuItems.Add
								(
								new MenuItem(Strings.CopyAction, 
								new System.EventHandler(OnCopyAction))
								);
							break;
						}

						case MouseButtons.Left:
						case MouseButtons.Middle:
						case MouseButtons.None:
							break;
						default: 
							break;
					}
				}
			}
		}

		/// <summary>
		/// copio nella clipboard il testo del secondo subitem della listview
		/// </summary>
		//---------------------------------------------------------------------------
		private void OnCopyAction(object sender, System.EventArgs e)
		{
			Clipboard.SetDataObject(selItem.SubItems[1].Text);
		}
		# endregion

		#region Eventi per gestire l'autenticazione ed interrogare la console
		/// <summary>
		/// AddUserAuthentication
		/// Aggiunge l'utente specificato alla lista degli utenti autenticati della Console
		/// </summary>
		//---------------------------------------------------------------------
		private void AddUserAuthentication(string login, string password, string serverName, DBMSType dbType)
		{
			if (OnAddUserAuthenticatedFromConsole != null)
				OnAddUserAuthenticatedFromConsole(login, password, serverName, dbType);
		}

		/// <summary>
		/// GetUserAuthenticatedPwd
		/// Richiede alla Console la pwd dell'utente già autenticato
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
		/// Richiede alla Console se l'utente specificato è stato già autenticato
		/// </summary>
		//---------------------------------------------------------------------
		private bool IsUserAuthenticated(string login, string password, string serverName)
		{
			bool result = false;
			if (OnIsUserAuthenticatedFromConsole != null)
				result = OnIsUserAuthenticatedFromConsole(login, password, serverName);
			return result;
		}
		#endregion
	}
}