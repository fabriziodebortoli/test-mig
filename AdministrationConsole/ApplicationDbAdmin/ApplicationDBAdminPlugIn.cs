using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using Microarea.Console.Core.DataManager;
using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;
using Microarea.Console.Core.RegressionTestLibrary;
using Microarea.Console.Plugin.ApplicationDBAdmin.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.ApplicationDBAdmin
{
	/// <summary>
	/// PlugIn di amministrazione del database.
	/// </summary>
	//=========================================================================
	public class ApplicationDBAdmin : PlugIn
	{
		# region Events e Delegates
		public delegate void PreRenderContextMenu(object sender, DynamicEventsArgs e);
		public event PreRenderContextMenu	OnPreRenderContextMenu;
		public event ChangeStatusBar		OnChangeStatusBarHandle; 

		# region Eventi intercettati dalla Console per avere delle info esternamente al plug-in
		// comunico alla Console lo stato della creazione del db di sistema
		public delegate void StatusCreationSystemDB(object sender, bool success, string message, Diagnostic dbAdminDiagnostic);
		public event StatusCreationSystemDB OnStatusCreationSystemDB;
		
		// tramite la Console chiedo delle info al LoginManager
		public delegate bool GetCompanyDBIsInFreeState(string companyId);
		public event GetCompanyDBIsInFreeState OnGetCompanyDBIsInFreeState;
		
		// stato della console		
		public delegate StatusType GetConsoleStatus();
		public event GetConsoleStatus OnGetConsoleStatus;

		// evento per chiedere alla Console se è attivata la funzionalità
		public delegate bool IsActivated(string application, string functionality);
		public event IsActivated OnIsActivated;

		public delegate bool CanMigrate();
		public event CanMigrate OnCanMigrate;
		#endregion

		#region Eventi intercettati dal SysAdmin		
		// chiedo al SysAdmin di abilitare/disabilitare il suo menu di popup
		public delegate void EnableSysAdminMenuItem();
		public event EnableSysAdminMenuItem OnEnableSysAdminMenuItem;
		
		public delegate void DisableSysAdminMenuItem();
		public event DisableSysAdminMenuItem OnDisableSysAdminMenuItem;

		public delegate bool GetConnectionInfo(string companyId, CompanyItem companyItem,/*SqlOwnerUserInfo userInfo, DBStructureInfo databaseStructureInfo,*/ Diagnostic diagnostic);
		public event GetConnectionInfo OnGetConnectionInfo;

		public delegate void ModifyCompanyParameters(string companyId, bool useUnicode, bool italianTable, string companyConnString, out bool modifyDBCulture);
		public event ModifyCompanyParameters OnModifyCompanyParameters;
        #endregion

        #region Evento intercettato dal SecurityAdmin
        public delegate void AfterUpgradeCompanyDB(string connectionString);
        public event AfterUpgradeCompanyDB OnAfterUpgradeCompanyDB;
        #endregion

        // comunico al TestPlanAdminPlugIn se si è verificato un errore durante l'aggiornamento del database di test
        public delegate void NotifyUpgradeErrorForTestPlan(bool ok, string script, string step, string detail, string fullPathScript, string application, string module);
		public event NotifyUpgradeErrorForTestPlan OnNotifyUpgradeErrorForTestPlan;

		# region Eventi per abilitare e disabilitare pulsanti della toolbar della Console
		// abilita/disabilita il pulsante Disconnect
		public event System.EventHandler  OnEnableDisconnectToolBarButton;
		public event System.EventHandler OnDisableDisconnectToolBarButton;

		// abilita/disabilita il pulsante Connect
		public event System.EventHandler OnEnableConnectToolBarButton;
		public event System.EventHandler OnDisableConnectToolBarButton;
		
		// abilita/disabilita il pulsante Apri
		public event System.EventHandler OnEnableOpenToolBarButton;
		public event System.EventHandler OnDisableOpenToolBarButton;

		// abilita/disabilita il pulsante Cancella
		public event System.EventHandler OnEnableDeleteToolBarButton;
		public event System.EventHandler OnDisableDeleteToolBarButton;

		// abilita/disabilita il pulsante Refresh
		public event System.EventHandler OnEnableRefreshToolBarButton;
		public event System.EventHandler OnDisableRefreshToolBarButton;
		# endregion

		# endregion

		# region Variabili
		private const int SQL_ERROR_CODE = 4060;

        private MenuStrip consoleMenu;
        private PlugInsTreeView consoleTree;
        private Panel workingAreaConsole;
		
		private DatabaseManager		dbManager			= null;
		private ElaborationForm		elaborationForm		= null;
		private DBForm				dbform				= null;
		private StartingWizardForm	startingWizForm		= null;

		// variabili di comodo per avere il Title dei moduli e il BrandTitle delle applicazioni
		// da visualizzare in fase di caricamento silente dati di default
		private string				myModule		= string.Empty; 
		private ModuleInfo			myModuleInfo	= null;

		// istanzio un oggetto per avere accesso alle varie funzionalità del Context
		private ContextInfo contextInfo = null;

		private string		sysDBConnectionString	= string.Empty;
		private PathFinder	consolePathFinder		= null;

		// oggetto di tipo Diagnostic
		public Diagnostic DBAdminDiagnostic = new Diagnostic("ApplicationDBAdmin");

		// info di ambiente di console
		private ConsoleEnvironmentInfo consoleEnvironmentInfo;
		// info relative alla licenza dell'installazione
		private LicenceInfo	licenceInfo;
		// info relative al Branding dalla Console
		private	BrandLoader	brandLoader;

		// per la gestione creazione db aziendale interattiva, dopo la creazione dell'azienda dal sysadmin
		private bool	interactiveCreation		= false;
		private string	interactiveCompanyId	= string.Empty;
		//
		# endregion

		# region Costruttore (vuoto)
		//---------------------------------------------------------------------
		public ApplicationDBAdmin()	{}
		# endregion

		# region Aggancio eventi al DatabaseManager per comunicare con il ContextInfo
		/// <summary>
		/// funzione centralizzata per agganciare i vari eventi di collegamento al ContextInfo
		/// </summary>
		//---------------------------------------------------------------------
		private void AddEventsInDatabaseManager()
		{		
			if (dbManager != null)
			{
				// aggancio eventi da rimandare al ContextInfo
				dbManager.OnIsUserAuthenticatedFromConsole		+= new DatabaseManager.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				dbManager.OnAddUserAuthenticatedFromConsole		+= new DatabaseManager.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				dbManager.OnGetUserAuthenticatedPwdFromConsole	+= new DatabaseManager.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			}
		}
		# endregion

		# region Load dell'ApplicationDBAdminPlugIn (quando la Console carica i vari PlugIn)
		/// <summary>
		/// Load ci deve essere sempre perchè la usa la console quando la tira su.
		/// Può anche essere vuota, l'importante è che valorizzi dentro a 3 variabili
		/// locali: il menù, il Tree e la Working Area in modo da poterle utilizzare
		/// anche in un secondo tempo
		/// </summary>
		//---------------------------------------------------------------------
		public override void Load
			(
			ConsoleGUIObjects		consoleGUIObjects, 
			ConsoleEnvironmentInfo	consoleEnvironmentInfo,
			LicenceInfo				licenceInfo
			)
		{
			this.consoleMenu			= consoleGUIObjects.MenuConsole; 
			this.consoleTree			= consoleGUIObjects.TreeConsole; 
			this.workingAreaConsole		= consoleGUIObjects.WkgAreaConsole; 
			this.consoleEnvironmentInfo = consoleEnvironmentInfo;
			this.licenceInfo			= licenceInfo;
		}
		# endregion

		# region Eventi per comunicare con il SysAdminPlugIn
		
		# region OnAfterModifyCompanyTree - Dopo la modifica del tree delle aziende
		/// <summary>
		/// Evento sparato dal SysAdmin dopo aver modificato il tree delle aziende
		/// </summary>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterModifyCompanyTree")]
		//---------------------------------------------------------------------
		public void OnAfterModifyCompanyTree(object sender, System.EventArgs e)
		{
			this.MenuRightClick((PlugInTreeNode)sender);
		}
		# endregion

		# region OnSendUpdateCompanyDatabase - Creazione interattiva delle tabelle del db aziendale
		/// <summary>
		/// Evento sparato dal SysAdmin per creare la struttura del database aziendale 
		/// interattivamente dopo aver creato l'azienda.
		/// </summary>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnSendUpdateCompanyDatabase")]
		//---------------------------------------------------------------------
		public void OnSendUpdateCompanyDatabase(object sender, string companyId)
		{
			interactiveCreation = true;
			interactiveCompanyId = companyId;

			EnableProgressBarFromPlugIn(sender);
			
			// richiamo il metodo dove viene scatenata tutta la procedura di controllo dello stato del database
			CheckCompanyDatabase(sender, interactiveCompanyId);
			
			DisableProgressBarFromPlugIn(sender);
		}
		# endregion

		# region OnAfterLogOnDB - Dopo aver effettuato la login al SysAdmin
		/// <summary>
		/// Evento sparato dal SysAdmin dopo aver effettuato la login
		/// </summary>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterLogOn")]
		//---------------------------------------------------------------------
		public void OnAfterLogOnDB(object sender, DynamicEventsArgs e)
		{
            //utilizzo la classe di info ConsoleEnvironment
            this.consoleEnvironmentInfo.ConsoleUserInfo.UserName   = e.Get("DbDefaultUser").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd    = e.Get("DbDefaultPassword").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth  = Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity"));

			if (e.Get("DbServerIstance").ToString().Length == 0)
				this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = e.Get("DbServer").ToString();
			else
				this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = Path.Combine(e.Get("DbServer").ToString(), e.Get("DbServerIstance").ToString());

			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName = e.Get("DbDataSource").ToString();

			if (OnGetConsoleStatus != null)
				this.consoleEnvironmentInfo.ConsoleStatus = OnGetConsoleStatus();

			// Valorizzo la struttura che conterrà i parametri che mi ha passato il SysAdmin per la connessione.
			contextInfo.SysDBConnectionInfo.DBName		= e.Get("DbDataSource").ToString();
			contextInfo.SysDBConnectionInfo.ServerName	= e.Get("DbServer").ToString();
			contextInfo.SysDBConnectionInfo.UserId		= e.Get("DbDefaultUser").ToString();
			contextInfo.SysDBConnectionInfo.Password	= e.Get("DbDefaultPassword").ToString();
			contextInfo.SysDBConnectionInfo.Instance	= e.Get("DbServerIstance").ToString();

			// devo comporre di nuovo la stringa di connessione perche' se ho aggiornato il db di sistema la connessione sottostante e' stata chiusa
			contextInfo.ComposeSystemDBConnectionString();

			//da linea di comando posso lanciare in automatico l'aggiornamento del database aziendale alla connessione
			CommandLineParam[] cmds = CommandLineParam.FromCommandLine();
			foreach (CommandLineParam cmd in cmds)
            {
                bool upgradeAllCompanies = string.Compare(cmd.Name, "UpgradeAllCompanies", StringComparison.InvariantCultureIgnoreCase) == 0;
                bool upgradeAllCompaniesAndExit = string.Compare(cmd.Name, "UpgradeAllCompaniesAndExit", StringComparison.InvariantCultureIgnoreCase) == 0;
                bool upgradeCompany = string.Compare(cmd.Name, "UpgradeCompany", StringComparison.InvariantCultureIgnoreCase) == 0;
                bool upgradeCompanyAndExit = string.Compare(cmd.Name, "UpgradeCompanyAndExit", StringComparison.InvariantCultureIgnoreCase) == 0;
                bool upgradeDefaultRoles = string.Compare(cmd.Name, "UpgradeDefaultRolesAndExit", StringComparison.InvariantCultureIgnoreCase) == 0;

                if (upgradeAllCompanies || upgradeAllCompaniesAndExit)
				{
					// procedo con l'aggiornamento del db aziendale SOLO SE la connessione e aggiornamento del db di sistema e' andato a buon fine
					if (!dbManager.ErrorInRunSqlScript)
                    {
						// se non riesco ad aprire la connessione al db di sistema esco subito
						if (!contextInfo.OpenSysDBConnection())
							Application.Exit();

						CompanyDb companyDb = new CompanyDb();
						companyDb.CurrentSqlConnection = (SqlConnection)contextInfo.Connection.DbConnect;
                        List<string> companiesIdsToUpgrade = companyDb.GetAllCompaniesIds(true);

						contextInfo.CloseConnection();

                        foreach (string companyId in companiesIdsToUpgrade)
						{
                            interactiveCreation = true;
							interactiveCompanyId = companyId;

							CheckCompanyDatabase(sender, interactiveCompanyId, false); // non faccio comparire la form con la richiesta delle credenziali del dbo

                            if (dbManager != null)
                            {
                                dbManager.ImportDefaultData = false;
								dbManager.ImportSampleData = false;
								DisableProgressBarFromPlugIn(sender);
								LoadDetailsForm(sender, e, false);
							}
						}
					}

					if (upgradeAllCompaniesAndExit)
						Application.Exit();
				}

				if (upgradeCompany || upgradeCompanyAndExit)
				{
					// procedo con l'aggiornamento del db aziendale SOLO SE la connessione e aggiornamento del db di sistema e' andato a buon fine
					if (!dbManager.ErrorInRunSqlScript)
					{
						interactiveCreation = true;
						interactiveCompanyId = cmd.Value;

						CheckCompanyDatabase(sender, interactiveCompanyId);
						if (dbManager != null)
						{
							dbManager.ImportDefaultData = false;
							dbManager.ImportSampleData = false;
							DisableProgressBarFromPlugIn(sender);
							LoadDetailsForm(sender, e, false);
						}
					}
				}

                if (upgradeDefaultRoles)
                {
                    string connectionString = String.Empty;
                    string databaseName = e.Get("DbDataSource").ToString();
                    if (databaseName != null && databaseName != String.Empty)
                    {
                        string serverName = e.Get("DbServer").ToString();
                        if (e.Get("DbServerIstance") != null && e.Get("DbServerIstance").ToString() != String.Empty)
                            serverName += Path.DirectorySeparatorChar + e.Get("DbServerIstance").ToString();

                        if (e.Get("IsWindowsIntegratedSecurity") != null && Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity")))
                            connectionString = String.Format("Data Source={0};Integrated Security=SSPI;Initial Catalog={1};Pooling=false;",
                                serverName, databaseName);
                        else
                            connectionString = String.Format("Data Source={0};User ID={1};Password={2};Initial Catalog={3};Pooling=false;",
                                serverName, e.Get("DbDefaultUser").ToString(), e.Get("DbDefaultPassword").ToString(), databaseName);
                    }

                    if (OnAfterUpgradeCompanyDB != null)
                    {
                        OnAfterUpgradeCompanyDB(connectionString);
                        Application.Exit();
                    }
                }

				if (upgradeCompanyAndExit)
					Application.Exit();
			}
		}
		# endregion

		# region OnBeforeConnectionSystemDB (controlli effettuati prima di connettermi al sysdb)
		/// <summary>
		/// Evento sparato dal SysAdmin prima dell'effettiva connessione al db di sistema
		/// (devo controllarne la struttura ed effettuare gli eventuali upgrade)
		/// </summary>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnBeforeConnectionSystemDB")]
		//---------------------------------------------------------------------
		public void OnBeforeConnectionSystemDB
			(
			object	sender, 
			string	connectionString, 
			bool	isNewDB, 
			bool	useUnicode, 
			string	userForConnection, 
			string	databaseForConnection
			)
		{
			// Metodo che esegue la creazione/aggiornamento struttura del database di sistema 
			// in modalita' silente
			sysDBConnectionString = connectionString;

			// non specifico la configurazione
			dbManager = new DatabaseManager
				(
				consolePathFinder,
				connectionString,
				DBMSType.SQLSERVER,
				useUnicode,
				DBAdminDiagnostic,
				brandLoader,
				licenceInfo.DBNetworkType,
                licenceInfo.IsoState
				);
			AddEventsInDatabaseManager();

			// se non sono riuscita a connettermi al db di sistema non vado avanti
			if (!dbManager.Valid)
			{
				if (OnStatusCreationSystemDB != null)
					if (contextInfo.SqlErrorCode == SQL_ERROR_CODE)
						OnStatusCreationSystemDB(this, false, Strings.MsgErrorDBNotExist, null);
					else
						OnStatusCreationSystemDB
							(
							this,
							false,
							string.Format(Strings.MsgErrorConnectionSysDB, databaseForConnection, userForConnection),
							DBAdminDiagnostic
							);
				return;
			}

			// fa il check della struttura del database di sistema
			if (!dbManager.CheckDBStructure(KindOfDatabase.System))
			{
				if (OnStatusCreationSystemDB != null)
					OnStatusCreationSystemDB(this, false, Strings.ErrCheckingSystemDB, null);
				return;
			}

			dbManager.DatabaseManagement();

			if (OnStatusCreationSystemDB != null)
				OnStatusCreationSystemDB
				(
					this,
					!dbManager.ErrorInRunSqlScript, // (è inteso come success, per quello che è negato)
					(dbManager.ErrorInRunSqlScript) ? Strings.MsgUpdateSysDBCompleteWithErrors : Strings.MsgUpdateSysDBCompleteWithSuccess,
					DBAdminDiagnostic
				);
		}
		# endregion

		# region OnCheckDBRequirements (check se il db aziendale è in unicode e se le tabelle sono in italiano)
		/// <summary>
		/// Evento sparato dal SysAdmin per sapere se un database aziendale è di tipo unicode o meno
		/// e se si tratta di un db con le tabelle in italiano
		/// </summary>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnCheckDBRequirements")]
		//---------------------------------------------------------------------
		public bool OnCheckDBRequirements
			(
			string		connectionString, 
			DBMSType	dbmstype, 
			bool		candidateUnicode, 
			out bool	isUnicode, 
			out bool	italianTableName
			)
		{
			TBConnection myConnection = null;
			TBDatabaseSchema mySchema = null;
			bool useUnicode = false, isItalianVersion = false;

			try
			{
				// istanzio una TBConnection e la apro
				myConnection = new TBConnection(connectionString, dbmstype);
				myConnection.Open();

				// istanzio TBDatabaseSchema sulla connessione
				mySchema = new TBDatabaseSchema(myConnection);
				// se la tabella di riferimento TB_DBMark non esiste, restituisco il valore unicode impostato dall'utente
				// altrimenti procedo con il controllo sulla tabella....
				if (!mySchema.ExistTable(DatabaseLayerConsts.TB_DBMark))
				{
					isUnicode = candidateUnicode;
					italianTableName = isItalianVersion;
					return false;
				}
				else
				{
					// analizzo lo schema della tabella e verifico il tipo della colonna Application
					DataTable cols = mySchema.GetTableSchema(DatabaseLayerConsts.TB_DBMark, false);

					foreach (DataRow col in cols.Rows)
					{
						// devo controllare la colonna Status perchè non segue le regole latine
						if (string.Compare(col["ColumnName"].ToString(), "Status", true, CultureInfo.InvariantCulture) == 0)
						{
							TBType providerType = TBDatabaseType.GetTBType(dbmstype, (int)col["ProviderType"]);

							useUnicode = string.Compare
								(col["DataTypeName"].ToString(),//(TBDatabaseType.GetDBDataType(providerType, dbmstype),
								"NChar", // in Oracle e in SQL i tipi hanno lo stesso nome
								true,
								CultureInfo.InvariantCulture) == 0;
							break;
						}
					}
				}

				// già che ho una connessione attiva... verifico se esiste un record che contiene 
				// la riga MagoNet+Azienda (in questo modo capisco se il db è nella versione italiana) 
				// e se esiste la tabella MN_Azienda
				string query = "SELECT COUNT(*) FROM TB_DBMARK WHERE Application = @app AND AddOnModule = @module";
				if (dbmstype == DBMSType.ORACLE)
					query = "SELECT COUNT(*) FROM TB_DBMARK WHERE APPLICATION = @app AND ADDONMODULE = @module";

				TBCommand myCommand = new TBCommand(query, myConnection);
				myCommand.Parameters.Add("@app", "MAGONET");
				myCommand.Parameters.Add("@module", "Azienda");

				if (dbmstype == DBMSType.SQLSERVER)
					isItalianVersion = (myCommand.ExecuteTBScalar() > 0) && mySchema.ExistTable("MN_Azienda");

				if (dbmstype == DBMSType.ORACLE)
					isItalianVersion = (myCommand.ExecuteTBScalar() > 0) && mySchema.ExistTable("MN_AZIENDA");

				myCommand.Dispose();
			}
			catch
			{
			}
			finally
			{
				if (myConnection.State == ConnectionState.Open || myConnection.State == ConnectionState.Broken)
				{
					myConnection.Close(); 
					myConnection.Dispose();
				}
			}
			
			isUnicode			= useUnicode;
			italianTableName	= isItalianVersion;

			return true;
		}
		# endregion

		# region Gestione utente Guest
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterAddGuestUser")]
		//---------------------------------------------------------------------
		public void AfterAddGuestUser (string guestUserName, string guestUserPwd)
		{
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.Exist    = true;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserName = guestUserName;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserPwd  = guestUserPwd;
		}

		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterDeleteGuestUser")]
		//---------------------------------------------------------------------
		public void AfterDeleteGuestUser (object sender, System.EventArgs e)
		{
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.Exist    = false;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserPwd  = string.Empty;
		}
		# endregion

		#region Evento di OnAfterClickF5Button sui nodi del SysAdmin
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterClickF5Button")]
		//---------------------------------------------------------------------
		public void OnAfterClickF5Key(object sender, System.EventArgs e)
		{
			// solo per i nodi di tipo Azienda ri-disegno il menu di contesto
			if (this.consoleTree.SelectedNode != null &&
				(string.Compare(((PlugInTreeNode)this.consoleTree.SelectedNode).Type, DBConstStrings.NodeTypeCompany, StringComparison.InvariantCultureIgnoreCase) == 0))
				this.MenuRightClick((PlugInTreeNode)this.consoleTree.SelectedNode);
		}
		#endregion

		# endregion

		# region Eventi per da e per il TestPlanAdminPlugIn
		/// <summary>
		/// Evento inviato dal TestPlanAdminPlugIn dopo aver effettuato il restore del database per il piano di test
		/// Viene controllato se nel database appena ripristinato:
		/// - esiste la tabella TB_DBMark
		/// - se esistono le tabelle in italiano (vecchia rel. 1.2.5)
		/// - se supporta i caratteri Unicode
		/// Nel caso in cui il db contenga la TB_DBMark viene inviato un evento al SysAdmin [OnModifyCompanyParameters]
		/// che si occupa di aggiornare l'anagrafica azienda associata al db (UseUnicode e DatabaseCulture).
		/// </summary>
		/// <param name="companyConnString">stringa di connessione al db aziendale</param>
		/// <param name="candidateUnicode">valore colonna UseUnicode</param>
		/// <param name="isUnicode">effettivo valore letto dalla tabella TB_DBMark</param>
		/// <param name="italianTableName">se le tabelle sono in italiano</param>
		/// <param name="companyId">id della company</param>
		/// <param name="messages">array di messaggi</param>
		/// <returns>esistenza della tabella TB_DBMark</returns>
		[AssemblyEvent("Microarea.Console.Plugin.TestPlanAdmin.TestPlanAdmin", "OnAfterRestoreDatabaseForTestPlan")]
		//---------------------------------------------------------------------
		public bool AfterRestoreDatabaseForTestPlan(string companyConnString, bool candidateUnicode, out bool isUnicode, out bool italianTableName, string companyId)
		{
			isUnicode  = false;
			italianTableName = false;

			bool existTBDBMark = 
				OnCheckDBRequirements(companyConnString, DBMSType.SQLSERVER, candidateUnicode, out isUnicode, out italianTableName);

			// se esiste la TB_DBMark allora sparo un evento al SysAdmin che effettua:
			// l'update sulla tabella MSD_Companies (per i flag UseUnicode e IsValid e la colonna DatabaseCulture)
			// l'update del tree della console
			if (existTBDBMark)
			{
				bool modifyDBCulture = false;
				if (OnModifyCompanyParameters != null)
					OnModifyCompanyParameters(companyId, isUnicode, italianTableName, companyConnString, out modifyDBCulture);
			}

			return existTBDBMark;
		}

		/// <summary>
		/// OnUpgradeDatabaseForTestPlan
		/// Evento inviato dal TestPlanAdminPlugIn per effettuare l'aggiornamento database
		/// </summary>
		/// <param name="companyId">id della company</param>
		/// <returns></returns>
		[AssemblyEvent("Microarea.Console.Plugin.TestPlanAdmin.TestPlanAdmin", "OnUpgradeDatabaseForTestPlan")]
		//---------------------------------------------------------------------
		public bool OnUpgradeDatabaseForTestPlan(string companyId, ContextInfo context)
		{
			// se ho eseguito un aggiornamento database dall'anagrafica azienda, questo puntatore rimane valorizzato
			// e provoca un errore di cast quando effettuo la comunicazione della messaggistica durante l'elaborazione
			// del TestPlanAdminPlugIn
			if (elaborationForm != null)
				elaborationForm = null;

			// uso il costruttore che accetta il contextinfo già utilizzato in precedenza per effettuare
			// la connessione all'azienda senza esplicita richiesta di autenticazione
			dbManager = new DatabaseManager
				(
				context, 
				DBAdminDiagnostic, 
				brandLoader,
				contextInfo.SysDBConnectionInfo, 
				licenceInfo.DBNetworkType, 
				licenceInfo.IsoState
				);

			// sfrutto l'evento usato per memorizzare le informazioni nell'ElaborationForm per rimpallare 
			// l'evento al TestPlanAdminPlugIn e scriverle nella form di Elaborazione (solo gli errori)
			dbManager.OnElaborationProgressMessage += new DatabaseManager.ElaborationProgressMessage(PostToSqlProgressListView);

			dbManager.ImportDefaultData	= false; // NON DEVO PRECARICARE I DATI DI DEFAULT!!!!

			bool success = dbManager.ConnectAndCheckDBStructureForTestPlan();

			if (success)
				dbManager.DatabaseManagement();

			dbManager.ContextInfo.CloseConnection();

			return (success) ? dbManager.ErrorInRunSqlScript : success;
		}
		# endregion

		# region Eventi per comunicare con la MicroareaConsole

		#region Evento di OnRefreshItem dalla toolbar di MConsole
		[AssemblyEvent("Microarea.Console.MicroareaConsole","OnRefreshItem")]
		//---------------------------------------------------------------------
		public void OnAfterClickRefreshButton(object sender, System.EventArgs e)
		{
			// solo per i nodi di tipo Azienda ri-disegno il menu di contesto
			if (this.consoleTree.SelectedNode != null &&
				(string.Compare(((PlugInTreeNode)this.consoleTree.SelectedNode).Type, DBConstStrings.NodeTypeCompany, StringComparison.InvariantCultureIgnoreCase) == 0))
				this.MenuRightClick((PlugInTreeNode)this.consoleTree.SelectedNode);
		}
		#endregion

		#region Evento di OnNotifyEndedTask dalla MConsole
		/// <summary>
		/// Evento sparato alla fine del task di backup o restore di database, pertanto si tratta solo di SQL Server
		/// </summary>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnNotifyEndedTask")]
		//---------------------------------------------------------------------
		public void OnNotifyEndedTask(int companyId, int loginId, out StringCollection messages)
		{
			bool candidateUnicode = false;
			string connCompanyDBString = 
				contextInfo.ComposeCompanyConnectionStringForRestore(companyId.ToString(), loginId.ToString(), out candidateUnicode);

			bool localUnicode = false, localItalian = false, existTBDBMark = false;

			if (connCompanyDBString.Length > 0)
			{
				// controllo che l'azienda contenga la tabella TB_DBMark, se è in Unicode o in versione italiana
				existTBDBMark = 
					OnCheckDBRequirements(connCompanyDBString, DBMSType.SQLSERVER, candidateUnicode, out localUnicode, out localItalian);
			}

			messages = new StringCollection();
			// se esiste la TB_DBMark allora sparo un evento al SysAdmin che effettua:
			// l'update sulla tabella MSD_Companies (per i flag UseUnicode e IsValid e la colonna DatabaseCulture)
			// l'update del tree della console
			if (existTBDBMark)
			{
				bool modifyDBCulture = false;
				if (OnModifyCompanyParameters != null)
					OnModifyCompanyParameters(companyId.ToString(), localUnicode, localItalian, connCompanyDBString, out modifyDBCulture);

				messages.Add((localItalian) ? DatabaseLayerStrings.DbItalianVersion : DatabaseLayerStrings.DbIsToUpdate);

				if (modifyDBCulture)
					messages.Add(DatabaseLayerStrings.ModifyDBCultureValue);

				if (localUnicode != candidateUnicode)
					messages.Add(DatabaseLayerStrings.ModifyUnicodeValue);
			}
			else
				messages.Add(string.Format(DatabaseLayerStrings.DbNoCompatible, string.Empty));
		}
		#endregion

		# region OnInitPathFinder
		/// <summary>
		/// la Console inizializza il PathFinder (con un PathFinder a seconda che runni lato server o 
		/// lato client) e mi passa l'oggetto giusto da utilizzare per i miei scopi di ricerca.
		/// </summary>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnInitPathFinder")]
		//---------------------------------------------------------------------
		public void OnInitPathFinder(PathFinder pathFinder)
		{
			consolePathFinder = pathFinder;

			// inizializzo il contextInfo con l'istanza corretta del pathfinder della console
			contextInfo = new ContextInfo(pathFinder, licenceInfo.DBNetworkType, licenceInfo.IsoState);
			contextInfo.OnAddUserAuthenticatedFromConsole		+= new ContextInfo.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			contextInfo.OnGetUserAuthenticatedPwdFromConsole	+= new ContextInfo.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			contextInfo.OnIsUserAuthenticatedFromConsole		+= new ContextInfo.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
		}
		# endregion
		
		# region OnInitBrandLoader - riceve il BrandLoader inizializzato
		/// <summary>
		/// la Console mi passa il BrandLoader inizializzato correttamente, in modo da propagarlo poi
		/// dove mi serve
		/// </summary>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnInitBrandLoader")]
		//-------------------------------------------------------------------
		public void OnInitBrandLoader(BrandLoader aBrandLoader)
		{
			brandLoader = aBrandLoader;
		}
		# endregion

		# region ShutDownFromPlugIn
		/// <summary>
		/// La Console mi comunica che che è stato cliccato sulla X o sul menu "Esci".
		/// Se il PlugIn deve effettuare una o più operazioni può farle eseguire prima di uscire.
		/// </summary>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnShutDownConsole")]
		//---------------------------------------------------------------------
		public override bool ShutDownFromPlugIn()
		{
			if (elaborationForm == null)
				return true;
			
			// se il PlugIn sta ancora runnando non permetto di chiudere la Console e dò un messaggio
			if (elaborationForm.State == StateEnums.Processing)
			{
				DiagnosticViewer.ShowCustomizeIconMessage(Strings.MsgNotStopElaboration, string.Empty, MessageBoxIcon.Information);
				return false;
			}

			return true;
		}
		# endregion

		# endregion

		# region Evento click tasto dx per aggiungere le voci al menu di contesto sul nodo Company
		/// <summary>
		/// aggiungo voci al ContextMenu, visualizzate sul click del tasto dx su un nodo di tipo Company
		/// 1. per la creazione/aggiornamento del database
		/// 2. per la gestione dei dati aziendali (import/export, dati di esempio e di default)
		/// 3. RegressionTest (se licenziato)
		/// </summary>
		//---------------------------------------------------------------------
		private void MenuRightClick(PlugInTreeNode companyNode)
		{
			// aggiungo le varie voci al menu di contesto
			if (this.OnPreRenderContextMenu != null)
			{
				ContextMenu newCMenu = new ContextMenu();

				// Gestione Rewind database solo se sono in una versione Development
				// (intesa NON come attivazione ma in base alla presenza dell'attributo development nel DatabaseObjects.xml)
				if (this.contextInfo.IsDevelopmentVersion)
					newCMenu.MenuItems.Add(Strings.RewindDatabaseContextMenu, new EventHandler(this.OnClickRewindDatabase));

				// Creazione/Aggiornamento Database Aziendale
				newCMenu.MenuItems.Add(Strings.DatabaseManagerMenu,	new EventHandler(this.OnClickUpdateDatabase));

				// aggiungo la voce della gestione dati aziendali solo se la Console non è con il web stoppato
				if ((this.consoleEnvironmentInfo.ConsoleStatus & StatusType.RemoteServerError) != StatusType.RemoteServerError)
					newCMenu.MenuItems.Add(Strings.CompanyDataMenu,	new EventHandler(this.OnClickStartingWizard));

				// solo se il provider e' Oracle aggiungo la voce per la gestione delle viste materializzate
				if (TBDatabaseType.GetDBMSType(companyNode.Provider) == DBMSType.ORACLE)
				{
					newCMenu.MenuItems.Add("-"); // aggiungo un separator
					newCMenu.MenuItems.Add(Strings.MViewContextMenu, new System.EventHandler(OnClickManageMView));
				}

				// aggiungo la voce RegressionTest nel context-menu solo se è licenziata la funzionalità "RegressionTestNet"
				if (TBDatabaseType.GetDBMSType(companyNode.Provider) == DBMSType.SQLSERVER &&
					OnIsActivated != null && OnIsActivated(DatabaseLayerConsts.MicroareaConsole, DatabaseLayerConsts.RegressionTestNet))
				{
					newCMenu.MenuItems.Add("-"); // aggiungo un separator
					newCMenu.MenuItems.Add("Gestione Regression Test", new EventHandler(this.OnClickStartingRegressionTestWizard));
				}

				PreRenderEventArgs args = new PreRenderEventArgs("Azienda", newCMenu);
				DynamicEventsArgs newContextMenu = new DynamicEventsArgs(args);
				this.OnPreRenderContextMenu(companyNode, newContextMenu);
			}
		}
		# endregion

		# region Evento click "Creazione o Aggiornamento Database Aziendale"	
		/// <summary>
		/// evento sul click (nel context menu) della voce di aggiornamento database
		/// </summary>
		//---------------------------------------------------------------------
		private void OnClickUpdateDatabase(object sender, System.EventArgs e)
		{
			interactiveCreation = false;
			PlugInTreeNode companyNode = (PlugInTreeNode)this.consoleTree.SelectedNode;

			// richiamo il metodo dove viene scatenata tutta la procedura per la creazione del database
			CheckCompanyDatabase(sender, companyNode.Id);

			DisableProgressBarFromPlugIn(sender);
		}
		#endregion

		#region Evento click "Rewind versione database"	
		/// <summary>
		/// evento sul click (nel context menu) della voce di rewind della versione del database
		/// </summary>
		//---------------------------------------------------------------------
		private void OnClickRewindDatabase(object sender, System.EventArgs e)
		{
			interactiveCreation = false;

			// verifico che all'azienda non sia collegato qualche utente lato Mago
			if (!GetCompanyDBIsFree())
			{
				DiagnosticViewer.ShowCustomizeIconMessage(Strings.ErrCompanyDBIsNotFree, string.Empty, MessageBoxIcon.Exclamation);
				return;
			}


			if (DiagnosticViewer.ShowQuestion(Strings.MsgStartRewindDatabase, String.Empty) == DialogResult.No)
				return;

			EnableProgressBarFromPlugIn(sender);

			List<DevelopmentModuleRelease> modules = GetModulesWithDevelopmentRelease();

			// utilizzo il costruttore con l'indicazione dell'id della company e del tipo di configurazione da caricare
			dbManager = new DatabaseManager
				(
				consolePathFinder,
				DBAdminDiagnostic,
				brandLoader,
				contextInfo.SysDBConnectionInfo,
				licenceInfo.DBNetworkType,
				licenceInfo.IsoState,
				true
				);
			AddEventsInDatabaseManager();

			PlugInTreeNode companyNode = (PlugInTreeNode)this.consoleTree.SelectedNode;

			// connessione al database aziendale e check della sua struttura
			if (!dbManager.ConnectAndCheckDBStructure(companyNode.Id))
			{
				if (DBAdminDiagnostic.Error || DBAdminDiagnostic.Warning || DBAdminDiagnostic.Information)
					DiagnosticViewer.ShowDiagnostic(DBAdminDiagnostic);
				// chiudo la connessione
				dbManager.CloseConnection();
				return;
			}

			string message = string.Empty;

			if (!dbManager.RewindDatabaseRelease(modules, out message))
			{
				DisableProgressBarFromPlugIn(sender);
				MessageBox.Show(message);
				return;
			}

			// richiamo il metodo dove viene scatenata tutta la procedura per l'aggiornamento del database
			CheckCompanyDatabase(sender, companyNode.Id);
			DisableProgressBarFromPlugIn(sender);
		}

		//---------------------------------------------------------------------
		private List<DevelopmentModuleRelease> GetModulesWithDevelopmentRelease()
		{
			List<DevelopmentModuleRelease> modules = new List<DevelopmentModuleRelease>();
			foreach (ApplicationInfo ai in consolePathFinder.ApplicationInfos)
				foreach (ModuleInfo mi in ai.Modules)
					if (mi.DatabaseObjectsInfo.IsDevelopmentVersion)
						modules.Add(new DevelopmentModuleRelease(ai.Name, mi.DatabaseObjectsInfo.Signature, mi.DatabaseObjectsInfo.Release));

			return modules;
		}
		#endregion

		#region Apertura della StartingWizardForm (da cui posso avviare i vari wizard di Import/Export))
		/// <summary>
		/// evento sul click (nel context menu) della voce "Gestione dati aziendali" per i dati di esempio e default
		/// </summary>
		//---------------------------------------------------------------------
		private void OnClickStartingWizard(object sender, System.EventArgs e)
		{
			if (this.OnChangeStatusBarHandle != null)
			{
				DynamicEventsArgs statusBar = new DynamicEventsArgs();
				statusBar.Add(Strings.MsgStatusBarStartWizard);
				this.OnChangeStatusBarHandle(sender, statusBar);
			}

            PlugInTreeNode companyNode = (PlugInTreeNode)this.consoleTree.SelectedNode;

			// provo a connettermi e a fare il check della struttura del db aziendale
			if (ConnectAndCheckCompanyDatabase(companyNode.Id))
			{
				// se il db risulta vuoto non procedo, chiudo la connessione e dò un opportuno messaggio
				if (dbManager.StatusDB == DatabaseStatus.EMPTY)
				{
					DiagnosticViewer.ShowCustomizeIconMessage(Strings.ErrStopWizard, Strings.LblAttention, MessageBoxIcon.Exclamation);
					dbManager.ContextInfo.CloseConnection();  
					dbManager.ContextInfo.UndoImpersonification();
					return;
				}
			}
			else
			{
				// se non riesco a connettermi chiudo la connessione ed esco
				dbManager.ContextInfo.CloseConnection();
				dbManager.ContextInfo.UndoImpersonification();
				return;
			}

			// dopo i controlli sul db aziendale chiudo la connessione.
			dbManager.ContextInfo.CloseConnection();
			dbManager.ContextInfo.UndoImpersonification();

			if (workingAreaConsole.Controls != null && workingAreaConsole.Controls.Count >= 0)
				workingAreaConsole.Controls.Clear();

			startingWizForm = new StartingWizardForm(companyNode.Id, contextInfo, brandLoader, companyNode.Text);
			startingWizForm.OnBeforeStartingWizard += new StartingWizardForm.BeforeStartingWizard(GetCompanyDBIsFree);
			startingWizForm.TopLevel = false;
			startingWizForm.Dock = DockStyle.Fill;
			OnBeforeAddFormFromPlugIn(sender, startingWizForm.Width, startingWizForm.Height); // resize Console
			workingAreaConsole.Controls.Add(startingWizForm);
			startingWizForm.Show();

			if (this.OnChangeStatusBarHandle != null)
			{
				DynamicEventsArgs statusBar = new DynamicEventsArgs();
				statusBar.Add(Strings.MsgStatusBarEndWizard);
				this.OnChangeStatusBarHandle(sender, statusBar);
			}
		}
		# endregion

		# region Apertura della form per la gestione delle viste materializzate (solo per ORACLE)
		///<summary>
		/// Gestione viste materializzate (create/drop), abilitato sul menu di contesto solo per le aziende Oracle 
		///</summary>
		//---------------------------------------------------------------------
		private void OnClickManageMView(object sender, System.EventArgs e)
		{
			PlugInTreeNode companyNode = (PlugInTreeNode)this.consoleTree.SelectedNode;
			if (companyNode == null)
				return;

			OpenMatViewsForm(companyNode.Id);
		}

		/// <summary>
		/// Evento sparato dal plugin dell'Auditing per aggiornare le viste materializzate
		/// di Oracle qualora si ritenga che sia cambiata la struttura delle tabelle sotto tracciatura
		/// (il metodo viene richiamato anche internamente dal menu di contesto)
		/// </summary>
		/// <param name="companyId">id company</param>
		/// <param name="fromAuditingPlugIn">se true disabilita i pulsanti di chiusura della form</param>
		[AssemblyEvent("Microarea.Console.Plugin.AuditingAdmin.AuditingAdmin", "LoadMatViewsForm")]
		//---------------------------------------------------------------------
		public void OpenMatViewsForm(string companyId, bool fromAuditingPlugIn = false)
		{
			if (string.IsNullOrWhiteSpace(companyId))
				return;

			// provo a connettermi e a fare il check della struttura del db aziendale
			if (ConnectAndCheckCompanyDatabase(companyId))
			{
				// se il db risulta vuoto non procedo, chiudo la connessione e dò un opportuno messaggio
				if (dbManager.StatusDB == DatabaseStatus.EMPTY)
				{
					DiagnosticViewer.ShowCustomizeIconMessage(Strings.ErrStopWizard, Strings.LblAttention, MessageBoxIcon.Exclamation);
					dbManager.ContextInfo.CloseConnection();
					dbManager.ContextInfo.UndoImpersonification();
					return;
				}

				// se arrivo dal plugin dell'Auditing eseguo un ulteriore controllo sulla presenza di viste materializzate nello schema
				if (fromAuditingPlugIn)
				{
					bool userHasPrivileges;
					List<string> viewsList;
					dbManager.CheckOracleMatViewsStatus(out userHasPrivileges, out viewsList);
					// se non ci sono viste materializzate ritorno, altrimenti mostro un messaggio e poi apro la form
					if (viewsList.Count == 0)
					{
						// chiudo la connessione ed esco
						dbManager.ContextInfo.CloseConnection();
						dbManager.ContextInfo.UndoImpersonification();
						return;
					}
					else
						DiagnosticViewer.ShowWarning(Strings.UpdateOracleMatViews, Strings.LblAttention);
				}
			}
			else
			{
				// se non riesco a connettermi chiudo la connessione ed esco
				dbManager.ContextInfo.CloseConnection();
				dbManager.ContextInfo.UndoImpersonification();
				return;
			}

			MatViews matViewsForm = new MatViews(dbManager);
			// se arrivo dal plugin dell'Auditing disabilito alcuni controlli
			// in modo da obbligare l'utente ad eseguire la procedura
			if (fromAuditingPlugIn)
				matViewsForm.DisableControls();
			matViewsForm.ShowDialog();

			// chiudo la finestra e relativa connessione al db
			dbManager.ContextInfo.CloseConnection();
			dbManager.ContextInfo.UndoImpersonification();
		}
		#endregion

		# region Apertura del wizard per la gestione dei test di non regressione
		//---------------------------------------------------------------------
		private void OnClickStartingRegressionTestWizard(object sender, System.EventArgs e)
		{
			if (this.OnChangeStatusBarHandle != null)
			{
				DynamicEventsArgs statusBar = new DynamicEventsArgs();
				statusBar.Add(Strings.MsgStatusBarStartWizard);
				this.OnChangeStatusBarHandle(sender, statusBar);
			}

			// richiamo il wizard del RegressionTest
			RegressionTestManager regressionTestMng = new RegressionTestManager(contextInfo);

			RegressionTestSelections selezioni = regressionTestMng.RunRegressionTestWizard();

			if (!selezioni.IsOk)
				return;

			// richiamo l'engine...
			RegressionTestEngine regressionEngine = new RegressionTestEngine
				(
				consolePathFinder, 
				DBAdminDiagnostic,
				contextInfo.SysDBConnectionInfo, 
				((PlugInTreeNode)this.consoleTree.SelectedNode).Id, 
				contextInfo, 
				brandLoader, 
				sysDBConnectionString,
				licenceInfo.DBNetworkType,
                licenceInfo.IsoState
				);

			regressionEngine.OnIsUserAuthenticated		+= new RegressionTestEngine.IsUserAuthenticated(IsUserAuthenticatedFromConsole);
			regressionEngine.OnAddUserAuthenticated		+= new RegressionTestEngine.AddUserAuthenticated(AddUserAuthenticatedFromConsole);
			regressionEngine.OnGetUserAuthenticatedPwd	+= new RegressionTestEngine.GetUserAuthenticatedPwd(GetUserAuthenticatedPwdFromConsole);
			regressionEngine.OnGetConnectionInfo		+= new RegressionTestEngine.GetConnectionInfo(GetConnectionInfoFromSysAdmin);
			
			regressionEngine.Go(selezioni);
		}

		//---------------------------------------------------------------------
		bool GetConnectionInfoFromSysAdmin(string companyId, CompanyItem companyItem, /*SqlOwnerUserInfo userInfo, DBStructureInfo databaseStructureInfo,*/ Diagnostic diagnostic)
		{
			if (OnGetConnectionInfo != null)
				return OnGetConnectionInfo(companyId, companyItem,/*userInfo, databaseStructureInfo,*/ diagnostic);
			
			return false;
		}
		# endregion

		# region Evento sul click del bottone Aggiorna (dalla DBForm)
		/// <summary>
		/// dopo il click sul pulsante "Aggiorna", sulla base dello stato del database e delle operazioni
		/// da effettuare, visualizzo l'elenco degli script (e dei file xml) durante la loro esecuzione
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadDetailsForm(object sender, System.EventArgs e, bool showDiagnostic = true)
		{
            if (this.OnChangeStatusBarHandle != null)
			{
				DynamicEventsArgs statusBar = new DynamicEventsArgs();
				statusBar.Add(Strings.MsgStatusBarStartUpdateDB);
				this.OnChangeStatusBarHandle(sender, statusBar);
			}

			// verifico che all'azienda non sia collegato qualche utente lato MagoNet
			if (!GetCompanyDBIsFree())
            {
                DiagnosticViewer.ShowCustomizeIconMessage(Strings.ErrCompanyDBIsNotFree, string.Empty, MessageBoxIcon.Exclamation);
				return;
			}

			if (workingAreaConsole.Controls != null && workingAreaConsole.Controls.Count >= 0)
				workingAreaConsole.Controls.Clear();

			// disabilito pulsanti della toolbar e menu durante l'elaborazione
			SetStatusForToolbarButtonsAndMenuItemsInConsole(false, sender, e);

			DBAdminDiagnostic.WriteLogOnFile = true;

			elaborationForm = new ElaborationForm(dbManager);
			elaborationForm.TopLevel	= false;
			elaborationForm.Dock		= DockStyle.Fill;
			elaborationForm.State		= StateEnums.Processing;
			elaborationForm.PopolateText();
			OnBeforeAddFormFromPlugIn(sender, elaborationForm.Width, elaborationForm.Height); // resize Console
			workingAreaConsole.Controls.Add(elaborationForm);
            elaborationForm.Show();
			consoleTree.Enabled = false;

			// demando al DatabaseManager le operazioni da effettuare sul database
			dbManager.DatabaseManagement(false); // passo false e significa che può creare il file di log

			elaborationForm.InsertStringInLabel(Strings.MsgStatusBarEnd);
			elaborationForm.State = StateEnums.View;
			consoleTree.Enabled = true;

			DisableProgressBarFromPlugIn(sender);
			if (dbManager.ErrorInRunSqlScript)
			{
				DiagnosticViewer.ShowDiagnostic(dbManager.DBManagerDiagnostic, DiagnosticType.Error | DiagnosticType.Warning, elaborationForm);
				DBAdminDiagnostic.Set(DiagnosticType.LogOnFile, Strings.MsgUpdateDBCompleteWithErrors);
			}
			//Strings.MsgUpdateDBCompleteWithErrors, string.Empty);
			else
			{
				if (showDiagnostic)
					DiagnosticViewer.ShowInformation(Strings.MsgUpdateDBCompleteWithSuccess, string.Empty, elaborationForm);
				DBAdminDiagnostic.Set(DiagnosticType.LogOnFile, Strings.MsgUpdateDBCompleteWithSuccess);
			}

			DBAdminDiagnostic.WriteLogOnFile = false;

			// abilito i pulsanti della toolbar e i menu terminata l'elaborazione
			SetStatusForToolbarButtonsAndMenuItemsInConsole(true, sender, e);

			// aggiorno la colonna DatabaseCulture (solo in SQL)
			dbManager.ContextInfo.SetCompanyDatabaseCulture();

			if (this.OnChangeStatusBarHandle != null)
			{
				DynamicEventsArgs statusBar = new DynamicEventsArgs();
				statusBar.Add(Strings.MsgStatusBarEnd);
				this.OnChangeStatusBarHandle(sender, statusBar);
			}
		}
		# endregion

		# region Creazione delle tabelle del database aziendale (in interattivo dal SysAdmin o dal context-menu)
		/// <summary>
		/// Metodo che richiama tutta la sequenza di operazioni per la creazione delle tabelle del 
		/// database aziendale (richiamata sia da interattivo tramite il SysAdmin che dall'apposito context-menu)
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckCompanyDatabase(object sender, string companyId, bool askCredential = true)
		{
			DBAdminDiagnostic.WriteLogOnFile = true;
			DBAdminDiagnostic.LogFileFullPath = GetPathLog(companyId);

			// utilizzo il costruttore con l'indicazione dell'id della company e del tipo di configurazione da caricare
			dbManager = new DatabaseManager
				(
				consolePathFinder, 
				DBAdminDiagnostic, 
				brandLoader,
				contextInfo.SysDBConnectionInfo, 
				licenceInfo.DBNetworkType, 
				licenceInfo.IsoState,
				askCredential
				);
			AddEventsInDatabaseManager();

			dbManager.OnCanMigrateCompanyDatabase += new DatabaseManager.CanMigrateCompanyDatabase(AskCanMigrateDatabase);

			// eventi per la ElaborationForm
			dbManager.OnElaborationProgressMessage	+= new DatabaseManager.ElaborationProgressMessage(PostToSqlProgressListView);
			dbManager.OnElaborationProgressBar		+= new DatabaseManager.ElaborationProgressBar(PostToProgressBar);
			dbManager.OnUpdateModuleCounter			+= new DatabaseManager.UpdateModuleCounter(PostToModuleCounter);
			dbManager.OnUpdateMandatoryCols			+= new DatabaseManager.UpdateMandatoryCols(PostToTableAndMandatoryCols);
			dbManager.OnInsertMessageInListView		+= new DatabaseManager.InsertMessageInListView(PostMessageInListView);
			dbManager.OnUpdateMViewCounter			+= new DatabaseManager.UpdateMViewCounter(PostToMViewCounter);
			
			// se non sono riuscita a connettermi al db di sistema non vado avanti
			if (!dbManager.Valid)
				return; 
						
			if (this.OnChangeStatusBarHandle != null)
			{
				DynamicEventsArgs statusBar = new DynamicEventsArgs();
				statusBar.Add(Strings.MsgStatusBarStartCheckDB);
				this.OnChangeStatusBarHandle(sender, statusBar);
			}

			// verifico se il database documentale e' attivato
			bool isDMSActivated = false;
			if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				isDMSActivated = true;

			// verifico se il RowSecurityLayer e' attivato
			bool isRowSecurityActivated = false;
			if (OnIsActivated != null && OnIsActivated(DatabaseLayerConsts.MicroareaConsole, DatabaseLayerConsts.RowSecurityToolKit))
				isRowSecurityActivated = true;

			// connessione al database aziendale e check della sua struttura
			// nonchè controllo dell'eventuale incongruenza tra i parametri memorizzati sulla mds_companies
			// e quelli realmente applicati sul database (nello specifico Collate e Unicode)
			// N.B. effettuo il controllo dell'eventuale db documentale solo se il modulo e' attivato!
			if (!dbManager.ConnectAndCheckDBStructure(companyId, isDMSActivated, isRowSecurityActivated))
			{
                if (DBAdminDiagnostic.Error || DBAdminDiagnostic.Warning || DBAdminDiagnostic.Information)
					DiagnosticViewer.ShowDiagnostic(DBAdminDiagnostic);
				// chiudo la connessione
				dbManager.CloseConnection();
				return;
			}

			// se ho verificato che ci sono delle incongruenze nei setting dell'azienda e quelli effettivi sul db
			// visualizzo il messaggio all'utente
			if (dbManager.ContextInfo.InconsistentSetting)
			{
				// deciso con Fabio di effettuare il riallinemento dei valori senza chiedere conferma all'utente
/*				DialogResult res = MessageBox.Show
					(dbManager.ContextInfo.MessageInconsistentSetting,
					Strings.LblAttention, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);

				if (res == DialogResult.Cancel)
				{
					dbManager.CloseConnection();
					if (workingAreaConsole.Controls != null && workingAreaConsole.Controls.Count >= 0)
						workingAreaConsole.Controls.Clear();
					return;
				}
*/
				// effettuo il riallineamento dei valori sulla tabella msd_companies
				if (!dbManager.ContextInfo.LineUpCompanyParametersSetting())
                {
                    MessageBox.Show(Strings.ErrUpdatingSystemDB, Strings.LblAttention, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}

				dbManager.ContextInfo.InconsistentSetting = false;
			}

			// aggancio eventi gestiti a livello di DIAGNOSTIC
			dbManager.ImpExpManager.DBDiagnostic.OnElaborationMessage += new DatabaseDiagnostic.ElaborationMessage(PostToXmlProgressListView);
			dbManager.ImpExpManager.DBDiagnostic.OnUpdateImportFileCounter += new DatabaseDiagnostic.UpdateImportFileCounter(PostToModuleCounterForDefault);
			dbManager.ImpExpManager.DBDiagnostic.OnSetFinishElaboration += new DatabaseDiagnostic.SetFinishElaboration(FinishElaboration);

			if (workingAreaConsole.Controls != null && workingAreaConsole.Controls.Count >= 0)
				workingAreaConsole.Controls.Clear();

			dbform = new DBForm(dbManager, this.consoleEnvironmentInfo.ConsoleStatus, AskCanMigrateDatabase());
			dbform.OnAfterUpdateConfirm += new DBForm.AfterUpdateConfirm(LoadDetailsForm);
			dbform.TopLevel = false;
			dbform.Dock	= DockStyle.Fill;
			dbform.State = StateEnums.View;
			OnBeforeAddFormFromPlugIn(sender, dbform.Width, dbform.Height); // resize Console
			workingAreaConsole.Controls.Add(dbform);
            dbform.Show();

            // la connessione la chiudo ora xchè mi serve a livello di DBForm per caricare le configurazioni dei dati di default
            dbManager.CloseConnection();
			DBAdminDiagnostic.WriteLogOnFile = false;

			if (this.OnChangeStatusBarHandle != null)
			{
				DynamicEventsArgs statusBar = new DynamicEventsArgs();
				statusBar.Add(Strings.MsgStatusBarEndCheckDB);
				this.OnChangeStatusBarHandle(sender, statusBar);
			}
		}

		//---------------------------------------------------------------------
		private string GetPathLog(string companyId)
		{
			string companyName = string.Empty;

			using (SqlConnection myConnection = new SqlConnection())
			{
				myConnection.ConnectionString = this.sysDBConnectionString;
				myConnection.Open();

				CompanyDb companyDb = new CompanyDb();
				companyDb.CurrentSqlConnection = myConnection;
				companyName = companyDb.GetCompanyName(companyId);
			}

			//creo un file di log in xml contenente le informazioni delle aziende e degli errori
			string path = string.IsNullOrWhiteSpace(companyName) 
							? string.Empty 
							: dbManager.ContextInfo.PathFinder.GetCustomCompanyLogPath(companyName, NameSolverStrings.AllUsers);

			if (string.IsNullOrEmpty(path))
				path = dbManager.ContextInfo.PathFinder.GetCustomPath();
			else
			{
				try
				{
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);
					// se incontro problemi di accesso per la creazione della cartella creo il file di log
					// nella Custom, in modo da non perdere le informazioni
				}
				catch (IOException)
				{
					path = dbManager.ContextInfo.PathFinder.GetCustomPath();
				}
				catch (UnauthorizedAccessException)
				{
					path = dbManager.ContextInfo.PathFinder.GetCustomPath();
				}
				catch (Exception)
				{
					path = dbManager.ContextInfo.PathFinder.GetCustomPath();
				}
			}

			return Path.Combine(path, string.Format("{0}-{1}-DumpLog.txt", companyName, DateTime.Now.ToString("yyyy-MM-dd")));
		}
		# endregion

		# region Fire di un evento alla Console per sapere se al database è connesso qualche utente (lato Mago)
		/// <summary>
		/// sparo un evento alla Console e chiedo se il database aziendale è libero 
		/// (ossia non ci sono utenti collegati).
		/// La Console si occupa di chiedere al Login Manager tutti gli utenti collegati 
		/// alla company selezionata
		/// </summary>
		/// <returns>true: il db è free e posso continuare nelle operazioni</returns>
		//---------------------------------------------------------------------
		private bool GetCompanyDBIsFree()
		{
			PlugInTreeNode companyNode = null;
			if (!interactiveCreation)
				companyNode = (PlugInTreeNode)this.consoleTree.SelectedNode;
			
			if (OnGetCompanyDBIsInFreeState != null)
				return OnGetCompanyDBIsInFreeState((interactiveCreation) ? interactiveCompanyId : companyNode.Id);

			return false;
		}
		#endregion

		//---------------------------------------------------------------------
		private bool AskCanMigrateDatabase()
		{
			if (OnCanMigrate != null)
				return OnCanMigrate();

			return false;
		}

		#region Funzioni per comunicare con la ElaborationForm
		/// <summary>
		/// inserisce una riga nella listview con l'elenco delle operazioni effettuate.
		/// </summary>
		//---------------------------------------------------------------------
		private void PostToSqlProgressListView
			(
			object	sender, 
			bool	ok, 
			string	script, 
			string	step, 
			string	detail,
			string	fullPathScript,
			ExtendedInfo ei
			)
		{
			string application = string.Empty, module = string.Empty;
			if (sender != null)
			{
				if (sender is ModuleDBInfo)
				{
					application = ((ModuleDBInfo)sender).ApplicationMember;
					module = ((ModuleDBInfo)sender).ModuleName;
				}
				else if (sender is string)
				{
					string[] str = ((string)sender).Split(new Char[] { '#' });
					application = str[0].ToString();
					if (str.Length > 1)
						module = str[1].ToString();
				}
			}

			if (elaborationForm == null)
			{
				// collegamento con il TestPlanAdminPlugIn
				// se l'operazione è quella di un errore "invio" il messaggio al plugin di Test che lo visualizza
				if (OnNotifyUpgradeErrorForTestPlan != null && !ok)
					OnNotifyUpgradeErrorForTestPlan
						(
						ok, 
						script, 
						step, 
						detail, 
						fullPathScript,
						application,
						module
						);
				return;
			}

			try
			{
				elaborationForm = (ElaborationForm)(workingAreaConsole.Controls[GetIndexFormInPanel(elaborationForm)]);

				elaborationForm.InsertInSqlProgressListView
					(
					ok,
					application, // visualizzo il Name dell'Applicazione
					module, // visualizzo il Title del Modulo
					script,
					step,
					detail,
					fullPathScript,
					ei
					);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("ApplicationDBAdmin::PostToSqlProgressListView" + ex.Message);
			}
		}

		/// <summary>
		/// inserisce nella text-box il nome del modulo durante l'elaborazione degli script
		/// "Elaborazione ... in corso"
		/// </summary>
		//---------------------------------------------------------------------
		private void PostToModuleCounter(object sender)
		{
			if (elaborationForm == null)
				return; 

			elaborationForm = (ElaborationForm)(workingAreaConsole.Controls[GetIndexFormInPanel(elaborationForm)]);
			elaborationForm.InsertModuleNameInLabel(((ModuleDBInfo)sender).Title);
		}

		/// <summary>
		/// inserisce nella text-box il nome della tabella a cui si stanno aggiungendo le colonne obbligatorie
		/// "Aggiunta colonne obbligatorie alla tabella {0} in corso..."
		/// </summary>
		//---------------------------------------------------------------------
		private void PostToTableAndMandatoryCols(string table)
		{
			if (elaborationForm == null)
				return; 

			elaborationForm = (ElaborationForm)(workingAreaConsole.Controls[GetIndexFormInPanel(elaborationForm)]);
			elaborationForm.InsertTableNameWithMandatoryColsInLabel(table);
		}

		/// <summary>
		/// inserisce una stringa contenente "Creazione vista materializzata {0} in corso"
		/// nella text box di elaborazione
		/// </summary>
		//---------------------------------------------------------------------
		private void PostToMViewCounter(string mView)
		{
			if (elaborationForm == null)
				return; 

			elaborationForm = (ElaborationForm)(workingAreaConsole.Controls[GetIndexFormInPanel(elaborationForm)]);
			elaborationForm.InsertMViewInLabel(mView);
		}
		
		/// <summary>
		/// inserisce una riga nella listview con l'elenco delle operazioni effettuate
		/// (per il caricamento silente dei dati di default)
		/// </summary>
		//---------------------------------------------------------------------
		private void PostToXmlProgressListView
			(
			bool	ok, 
			string	application,
			string	module,
			string	script, 
			string	detail,
			string	fullPathScript
			)
		{
			if (elaborationForm == null)
				return; 

			elaborationForm = (ElaborationForm)(workingAreaConsole.Controls[GetIndexFormInPanel(elaborationForm)]);

			// check per individuare il title localizzato del modulo e il brand dell'applicazione
			if (string.Compare(myModule, module, true, CultureInfo.InvariantCulture) != 0)
			{
				myModule = module;
				myModuleInfo = (ModuleInfo)contextInfo.PathFinder.GetModuleInfoByName(application, module);
			}

            string brandMenuTitle = this.brandLoader.GetApplicationBrandMenuTitle(application);
            elaborationForm.InsertInXmlProgressListView
				(
				ok, 
				(brandMenuTitle.IsNullOrEmpty()) ? application : brandMenuTitle, // visualizzo il Brand dell'applicazione
				(myModuleInfo != null) ? myModuleInfo.Title : module,		// visualizzo il Title del Modulo
				script, 
				detail,
				fullPathScript
				);	
		}

		/// <summary>
		/// inserisce nella text-box il nome del modulo durante l'importazione dei file
		/// contenenti i dati di default
		/// </summary>
		//---------------------------------------------------------------------
		private void PostToModuleCounterForDefault(string fileName, string appName, string moduleName)
		{
			if (elaborationForm == null)
				return; 

			elaborationForm = (ElaborationForm)(workingAreaConsole.Controls[GetIndexFormInPanel(elaborationForm)]);

			if (string.Compare(myModule, moduleName, true, CultureInfo.InvariantCulture) != 0)
			{
				myModule = moduleName;
				myModuleInfo = (ModuleInfo)contextInfo.PathFinder.GetModuleInfoByName(appName, moduleName);
			}

			elaborationForm.InsertFileNameInLabelForDefault(fileName, (myModuleInfo != null) ? myModuleInfo.Title : moduleName);
		}

		/// <summary>
		/// per comunicare la fine dell'elaborazione
		/// </summary>
		//---------------------------------------------------------------------
		private void FinishElaboration(string message)
		{
			if (elaborationForm == null)
				return; 

			elaborationForm = (ElaborationForm)(workingAreaConsole.Controls[GetIndexFormInPanel(elaborationForm)]);
			elaborationForm.InsertStringInLabel(Strings.MsgUpdateDBComplete);
		}

		/// <summary>
		/// scrivo un msg di errore nella list view qualora non esistano i file con i dati di default da importare
		/// </summary>
		//---------------------------------------------------------------------
		private void PostMessageInListView()
		{
			if (elaborationForm == null)
				return; 

			elaborationForm = (ElaborationForm)(workingAreaConsole.Controls[GetIndexFormInPanel(elaborationForm)]);
			elaborationForm.RedefineXmlProgressListView();
		}

		# endregion

		# region Abilitazione ProgressBar della Console
		/// <summary>
		/// inserisce uno step nella progress bar della console
		/// </summary>
		//---------------------------------------------------------------------
		private void PostToProgressBar(object sender)
		{
			EnableProgressBarFromPlugIn(sender);
		}
		# endregion

		# region Funzioni di utilità generale richiamate in più punti
		/// <summary>
		/// funzione che restituisce l'indice della form nell'array dei controls agganciati al panel
		/// </summary>
		//---------------------------------------------------------------------
		private int GetIndexFormInPanel(System.Windows.Forms.Form winform)
		{
			if (workingAreaConsole.Controls != null && workingAreaConsole.Controls.Count >= 0)
				return 0;

			foreach (Control ctrl in workingAreaConsole.Controls)
			{
				if (string.Compare(ctrl.Name, winform.Name, true, CultureInfo.InvariantCulture) == 0)
					return workingAreaConsole.Controls.IndexOf(ctrl);
				else
					continue;
			}

			return 0;
		}

		/// <summary>
		/// funzione richiamata in fase di lancio dei wizard di import/export, per provare la connessione 
		/// al database aziendale associato alla company selezionata e il check della sua struttura.
		/// se tutto va a buon fine procedo nel lancio del wizard, altrimenti dò un opportuno messaggio.
		/// </summary>
		//---------------------------------------------------------------------
		private bool ConnectAndCheckCompanyDatabase(string companyId)
		{
			dbManager = new DatabaseManager
				(
				consolePathFinder, 
				DBAdminDiagnostic, 
				brandLoader,
				contextInfo.SysDBConnectionInfo, 
				licenceInfo.DBNetworkType, 
				licenceInfo.IsoState,
				true
				);
			AddEventsInDatabaseManager();

			// connessione al database aziendale e check della sua struttura
			if (!dbManager.ConnectAndCheckDBStructure(companyId))
			{
				if (DBAdminDiagnostic.Error || DBAdminDiagnostic.Warning || DBAdminDiagnostic.Information)
					DiagnosticViewer.ShowDiagnostic(DBAdminDiagnostic);
				// chiudo la connessione
				dbManager.CloseConnection();
				return false;
			}

			return true;
		}
		# endregion

		# region Fire di un gruppo di eventi verso la Console per abilitare/disabilitare toolbar e popup
		/// <summary>
		/// funzione che si occupa di abilitare(enable=true)/disabilitare(enable=false) i pulsanti della toolbar
		/// ed spara un evento al SysAdmin per disabilitare il suo menu popup (&Aziende e Utenti)
		/// </summary>
		//---------------------------------------------------------------------
		private void SetStatusForToolbarButtonsAndMenuItemsInConsole(bool enable, object sender, System.EventArgs e)
		{
			if (enable)
			{
				if (OnEnableConnectToolBarButton != null) OnEnableConnectToolBarButton(sender, e);
				if (OnEnableDisconnectToolBarButton != null) OnEnableDisconnectToolBarButton(sender, e);
				if (OnEnableOpenToolBarButton != null) OnEnableOpenToolBarButton(sender, e);
				if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, e);
				if (OnEnableRefreshToolBarButton != null) OnEnableRefreshToolBarButton(sender, e);
				if (OnEnableSysAdminMenuItem != null) OnEnableSysAdminMenuItem(); // al SysAdmin
			}
			else
			{
				if (OnDisableConnectToolBarButton != null) OnDisableConnectToolBarButton(sender, e);
				if (OnDisableDisconnectToolBarButton != null) OnDisableDisconnectToolBarButton(sender, e);
				if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
				if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
				if (OnDisableRefreshToolBarButton != null) OnDisableRefreshToolBarButton(sender, e);
				if (OnDisableSysAdminMenuItem != null) OnDisableSysAdminMenuItem(); // al SysAdmin
			}
		}
		# endregion

		# region Funzione richiamata dal SysAdmin quando viene effettuata la verifica dell'integrità dei dati
		/// <summary>
		/// Evento "sparato" dal SysAdmin in fase di verifica dell'integrità dei dati di una specifica azienda
		/// </summary>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnCheckStructureCompanyDatabase")]
		//---------------------------------------------------------------------
		public void CheckStructureCompanyDatabase(object sender, string companyId, ref Diagnostic messages)
		{
			// utilizzo il costruttore con l'indicazione dell'id della company e del tipo di configurazione da caricare
			dbManager = new DatabaseManager
				(
				consolePathFinder, 
				DBAdminDiagnostic, 
				brandLoader,
				contextInfo.SysDBConnectionInfo, 
				licenceInfo.DBNetworkType, 
				licenceInfo.IsoState,
				true
				);
			AddEventsInDatabaseManager();

			// se non sono riuscita a connettermi al db di sistema non vado avanti
			if (!dbManager.Valid)
				return; 
						
			if (this.OnChangeStatusBarHandle != null)
			{
				DynamicEventsArgs statusBar = new DynamicEventsArgs();
				statusBar.Add(Strings.MsgStatusBarStartCheckDB);
				this.OnChangeStatusBarHandle(sender, statusBar);
			}

			bool isDMSActivated = false;
			if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				isDMSActivated = true;

			// connessione al database aziendale e check della sua struttura (se attivato anche per il database di EA)
			if (!dbManager.ConnectAndCheckDBStructure(companyId, isDMSActivated, false))
			{
				messages.Set(DBAdminDiagnostic);
				messages.Set(DiagnosticType.Error, Strings.ErrCheckingCompanyDB);
				// chiudo la connessione
				dbManager.CloseConnection();
				return;
			}

			// chiudo la connessione
			dbManager.CloseConnection();

			DiagnosticItems diagnItems = DBAdminDiagnostic.AllMessages() as DiagnosticItems;
			if (diagnItems != null)
			{
				diagnItems.Reverse();
				foreach (DiagnosticItem item in diagnItems)
					messages.Set(item.Type, item.FullExplain);
			}
		}
		# endregion

		#region
		/// <summary>
		/// Evento sparato dal plugin RowSecurityLayer per controllare la struttura 
		/// del SOLO database aziendale, identificato dal companyId
		/// </summary>
		/// <param name="companyId">id company</param>
		[AssemblyEvent("Microarea.Console.Plugin.RowSecurityToolKit.RowSecurityLayer", "CheckCompanyDBForRSL")]
		//---------------------------------------------------------------------
		public DatabaseStatus CheckDBForRowSecurityLayerPlugin(string companyId)
		{
			if (string.IsNullOrWhiteSpace(companyId))
				return DatabaseStatus.EMPTY;

			// provo a connettermi e a fare il check della struttura del db aziendale
			DatabaseStatus dbStatus = ConnectAndCheckCompanyDatabase(companyId) ? dbManager.StatusDB : DatabaseStatus.EMPTY;

			// chiudo la finestra e relativa connessione al db
			dbManager.ContextInfo.CloseConnection();
			dbManager.ContextInfo.UndoImpersonification();

			return dbStatus;
		}
		#endregion
	}

	# region classe PreRenderEventArgs
	//=========================================================================
	public class PreRenderEventArgs
	{
		private string nodeType;
		private ContextMenu newContext;
		
		public string NodeType { get { return this.nodeType; } set { this.nodeType = value; } }

		public ContextMenu NewContext { get { return this.newContext; } set { this.newContext = value; } }

		//---------------------------------------------------------------------
		public PreRenderEventArgs()
		{
			this.nodeType = null;
			this.newContext = null;
		}

		//---------------------------------------------------------------------
		public PreRenderEventArgs(string nodeType, ContextMenu context)
		{
			this.nodeType = nodeType;
			this.newContext = context;
		}
	}
	# endregion
}

/* Ordine di chiamata delle funzioni quando la Console viene caricata:
 * --- Caricamento MicroareaConsole
 * 1. Costruttore ApplicationDbAdmin
 * 2. Load 
 * 3. OnInitPathFinder
 * --- Premendo il pulsante Connect
 * 4. OnBeforeConnectionSystemDB
 * 5. OnAfterLogOnDB
 * --- Uscita dalla Console
 * 6. ShutDownFromPlugIn
*/
