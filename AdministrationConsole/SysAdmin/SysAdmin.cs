 using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;
using Microarea.Console.Plugin.SysAdmin.Form;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.SQLDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin
{
	/// <summary>
	/// CustomContextMenu
	/// Per i contexMenu che necessita di campi chiave
	/// </summary>
	//=========================================================================
	public class CustomContextMenu : System.Windows.Forms.ContextMenu
	{
		#region Variabili Private
		private string roleId = string.Empty;
		private string companyId = string.Empty;
		private string loginId = string.Empty;
		private DBMSType companyProvider = DBMSType.UNKNOWN;
		#endregion

		#region Proprietà
		public string CompanyId { get { return companyId; } set { companyId = value; } }
		public string RoleId { get { return roleId; } set { roleId = value; } }
		public string LoginId { get { return loginId; } set { loginId = value; } }
		public DBMSType CompanyProvider { get { return companyProvider; } set { companyProvider = value; } }
		#endregion

		#region Costruttori
		//---------------------------------------------------------------------
		public CustomContextMenu(string companyId, string roleId, string loginId, string companyProvider)
		{
			MenuItems.Clear();
			CompanyId = companyId;
			RoleId = roleId;
			LoginId = loginId;
			CompanyProvider = TBDatabaseType.GetDBMSType(companyProvider);
		}

		//---------------------------------------------------------------------
		public CustomContextMenu(string companyId, string roleId, string loginId, DBMSType companyProvider)
		{
			MenuItems.Clear();
			CompanyId = companyId;
			RoleId = roleId;
			LoginId = loginId;
			CompanyProvider = companyProvider;
		}

		//---------------------------------------------------------------------
		public CustomContextMenu(string companyId)
			: this(companyId, string.Empty, string.Empty, DBMSType.SQLSERVER)
		{ }

		//---------------------------------------------------------------------
		public CustomContextMenu(string companyId, DBMSType providerType)
			: this(companyId, string.Empty, string.Empty, providerType)
		{ }

		//---------------------------------------------------------------------
		public CustomContextMenu(string companyId, string roleId)
			: this(companyId, roleId, string.Empty, DBMSType.SQLSERVER)
		{ }

		//---------------------------------------------------------------------
		public CustomContextMenu(string companyId, string roleId, DBMSType providerType)
			: this(companyId, roleId, string.Empty, providerType)
		{ }

		//---------------------------------------------------------------------
		public CustomContextMenu(string companyId, string roleId, string loginId)
			: this(companyId, roleId, loginId, DBMSType.SQLSERVER)
		{ }
		#endregion
	}

	/// <summary>
	/// SysAdmin
	/// PlugIn di amministrazione di OSL
	/// </summary>
	//========================================================================= 
	public class SysAdmin : PlugIn
	{
		#region Variabili Private
		private ContextMenu context;
		private CustomContextMenu customContext;
		private MenuStrip consoleMenu;
		private PlugInsTreeView consoleTree;
		private Panel workingAreaConsole;
		private Panel bottomWorkingAreaConsole;
		private ImageList imageListTree;
		private SysAdminStatus currentStatus;
		private bool runningAtServer = false;
		private bool ErrorsFromCheckDb = false;
		private string oldConnStringSysDB = string.Empty;

		private PathFinder pathFinder = null;
		private BrandLoader brandLoader = null;
		private DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
		private Diagnostic diagnostic = new Diagnostic("SysAdmin");

		private ConsoleEnvironmentInfo consoleEnvironmentInfo;
		private LicenceInfo licenceInfo;
		#endregion

		#region Proprietà e Membri
		private DynamicEventsArgs settingsForConnection;
		public DynamicEventsArgs SettingsForConnection { get { return settingsForConnection; } set { settingsForConnection = value; } }
		#endregion

		#region Delegati ed Eventi
		//---------------------------------------------------------------------
		public delegate bool AfterCreateServerConnection(object sender);
		public event AfterCreateServerConnection OnAfterCreateServerConnection;

		public delegate void DeleteCompanyFromSysAdmin(object sender, string companyId);
		public event DeleteCompanyFromSysAdmin OnDeleteCompanyFromSysAdmin;

		public delegate void AfterAddSystemsUser(string userName, string userPwd);
		public event AfterAddSystemsUser OnAfterAddGuestUser;
		public event System.EventHandler OnAfterDeleteGuestUser;

		public delegate void RefreshConsoleStatus(string fullServerName);
		public event RefreshConsoleStatus OnRefreshConsoleStatus;

		public delegate StatusType GetConsoleStatus();
		public event GetConsoleStatus OnGetConsoleStatus;

		// tramite la Console chiedo delle info al LoginManager
		public delegate bool GetCompanyDBIsInFreeState(string companyId);
		public event GetCompanyDBIsInFreeState OnGetCompanyDBIsInFreeState;

		public event System.EventHandler OnAfterClickF5Button;

		public event DeleteCompanyFromSysAdmin OnSendUpdateCompanyDatabase;
		public event System.EventHandler OnDisableShowAllGrantsToolBarButtonPushed;

		public delegate void SaveConfigFile(object sender);
		public event SaveConfigFile OnSaveConfigFile;

		public delegate bool ShowLoginAdvancedOptions();
		public event ShowLoginAdvancedOptions OnShowLoginAdvancedOptions;

		#region Delegati ed eventi per la comunicazione con il LockManager e LoginManager
		//---------------------------------------------------------------------
		//LockManager
		public delegate bool UnlockAllForUser(object sender, string loginName);
		public event UnlockAllForUser OnUnlockAllForUser;

		public delegate bool UnlockForCompanyDBName(object sender, string companyDBName);
		public event UnlockForCompanyDBName OnUnlockForCompanyDBName;

		//LoginManager
		public delegate bool DeleteAssociationToLoginManager(object sender, int loginId, int companyId);
		public event DeleteAssociationToLoginManager OnDeleteAssociationToLoginManager;

		public delegate bool DeleteUserToLoginManager(object sender, int loginId);
		public event DeleteUserToLoginManager OnDeleteUserToLoginManager;

		public delegate bool DeleteCompanyToLoginManager(object sender, int companyId);
		public event DeleteCompanyToLoginManager OnDeleteCompanyToLoginManager;

		public delegate void ModifyCulture(object sender, string cultureUI, string culture);
		public event ModifyCulture OnModifyCulture;
		#endregion

		#region Delegati ed Eventi specifici per la MConsole
		// evento inviato dopo il log on / log off
		//---------------------------------------------------------------------
		public delegate void EnableMenuAfterLogOperation(bool enable);
		public event EnableMenuAfterLogOperation OnEnableMenuAfterLogOperation;

		// event to the Console, that has to show the about box if product is not registered
		//---------------------------------------------------------------------
		public event EventHandler ConnectedToSysDB;
		protected virtual void OnConnectedToSysDB(EventArgs e)
		{
			if (ConnectedToSysDB != null)
				ConnectedToSysDB(this, e);
		}
		# endregion

		#region Delegati ed Eventi per il Login/LogOff del SysAdmin
		//---------------------------------------------------------------------
		public delegate void AfterLogOn(object sender, DynamicEventsArgs e);
		public event AfterLogOn OnAfterLogOn;

		public delegate void AfterLogOff(object sender, System.EventArgs e);
		public event AfterLogOff OnAfterLogOff;
		#endregion

		#region Delegati ed Eventi per le azioni sui Providers
		//---------------------------------------------------------------------
		public delegate void ModifyProvider(object sender, string id);
		public event ModifyProvider OnModifyProvider;

		public delegate void SaveProvider(object sender, string id);
		public event SaveProvider OnSaveProvider;
		#endregion

		#region Delegati ed Eventi per le azioni sugli Utenti
		//---------------------------------------------------------------------
		public delegate void NewGuestUser(object sender, System.EventArgs e);
		public delegate void NewUser(object sender, System.EventArgs e);
		public delegate void OpenUser(object sender, string id);
		public delegate void ModifyUtente(object sender, string id);
		public delegate void DeleteUser(object sender, string id);
		public delegate void UpdateUser(object sender);
		public delegate void SaveUser(object sender, string id);
		public delegate void SaveNewUser(object sender, string id);
		public delegate void AfterChangedDisabledLogin(object sender, string loginId, bool disable);
		public event NewUser OnNewUser;
		//public event NewGuestUser OnNewGuestUser;
		public event OpenUser OnOpenUser;
		public event ModifyUtente OnModifyUtente;
		public event UpdateUser OnUpdateUser;
		public event DeleteUser OnDeleteUser;
		public event SaveUser OnSaveUser;
		public event SaveNewUser OnSaveNewUser;
		public event AfterChangedDisabledLogin OnAfterChangedDisabledLogin;
		public event DeleteUser OnDeleteUserToPlugIns;
		#endregion

		#region Delegati ed Eventi per le azioni sulle Aziende
		//---------------------------------------------------------------------
		public delegate void NewCompany(object sender, System.EventArgs e);
		public delegate void OpenCompany(object sender, string id);
		public delegate void ModifyCompany(object sender, string id);
		public delegate void UpdateCompanies(object sender);
		public delegate void UpdateCompany(object sender, string id);
		public delegate void DeleteCompany(object sender, string id, DBMSType providerType);
		public delegate void SaveCompany(object sender, string id);
		public delegate void AfterSavedCompany(object sender, string id);
		public delegate void NewCloneCompany(object sender, EventArgs e);
		public delegate void BackupDbCompany(object sender, EventArgs e);
		public delegate void RestoreDbCompany(object sender, EventArgs e);
		public delegate void AfterClonedCompany(string companyId);
		public delegate void AfterChangedCompanyAuditing(object sender, string companyId, bool activity);
		public delegate void AfterChangedCompanyOSLSecurity(object sender, string companyId, bool security);
		public delegate void AfterChangedCompanyDisable(object sender, string companyId, bool isDisable);
		public delegate void BeforeConnectionSystemDB(object sender, string connectionString, bool isNewDatabase, bool useUnicode, string userForConnection, string databaseForConnection);
		public delegate void CheckStructureCompanyDatabase(object sender, string companyId, ref Diagnostic messages);
		public delegate bool CheckDBRequirements(string connectionString, DBMSType dbType, bool candidateUnicode, out bool isUnicode, out bool italianTableName);
		public delegate void AfterSaveNewCompany(string companyId, bool isDisabled);

		public event NewCompany OnNewCompany;
		public event OpenCompany OnOpenCompany;
		public event ModifyCompany OnModifyCompany;
		public event UpdateCompanies OnUpdateCompanies;
		public event UpdateCompany OnUpdateCompany;
		public event DeleteCompany OnDeleteCompany;
		public event SaveCompany OnSaveCompany;
		public event AfterSavedCompany OnAfterSavedCompany;
		public event NewCloneCompany OnNewCloneCompany;
		public event BackupDbCompany OnBackupDbCompany;
		public event RestoreDbCompany OnRestoreDbCompany;
		public event AfterClonedCompany OnAfterClonedCompany;
		public event AfterChangedCompanyAuditing OnAfterChangedCompanyAuditing;
		public event AfterChangedCompanyOSLSecurity OnAfterChangedCompanyOSLSecurity;
		public event AfterChangedCompanyDisable OnAfterChangedCompanyDisable;
		public event BeforeConnectionSystemDB OnBeforeConnectionSystemDB;
		public event CheckStructureCompanyDatabase OnCheckStructureCompanyDatabase;
		public event CheckDBRequirements OnCheckDBRequirements;
		public event AfterSaveNewCompany OnAfterSaveNewCompany;
		#endregion

		#region Delegati ed eventi per le azioni sui Ruoli di una Azienda
		//---------------------------------------------------------------------
		public delegate void NewRole(object sender, System.EventArgs e);
		public delegate void OpenRole(object sender, string id, string companyId);
		public delegate void ModifyRuolo(object sender, string id, string companyId);
		public delegate void DeleteRole(object sender, string id, string companyId);
		public delegate void UpdateRole(object sender, string companyId);
		public delegate void SaveRole(object sender, string id, string companyId);
		public delegate void SaveNewRole(object sender, string id, string companyId);
		public delegate void AfterClonedRole(string companyId);
		public delegate void AfterClickDisabledRole(object sender, string roleId, bool disabled);

		public event NewRole OnNewRole;
		public event OpenRole OnOpenRole;
		public event ModifyRuolo OnModifyRuolo;
		public event DeleteRole OnDeleteRole;
		public event UpdateRole OnUpdateRole;
		public event SaveRole OnSaveRole;
		public event SaveNewRole OnSaveNewRole;
		public event AfterClonedRole OnAfterClonedRole;
		public event AfterClickDisabledRole OnAfterClickDisabledRole;
		#endregion

		#region Delegati ed Eventi per le azioni sugli utenti assegnati a una Azienda
		//---------------------------------------------------------------------
		public delegate void OpenCompanyUser(object sender, string id, string companyId);
		public delegate void ModifyCompanyUser(object sender, string id, string companyId);
		public delegate void SaveCompanyUser(object sender, string id, string companyId);
		public delegate void SaveCompanyUserToLogin(object sender, string companyId);
		public delegate void UpdateCompanyUser(object sender, string companyId);
		public delegate void DeleteCompanyUser(object sender, string id, string companyId, DBMSType typeOfProvider);
		public delegate void AfterDeleteCompanyUser(object sender, string id, string companyId);
		public delegate void AfterClonedUserCompany(string companyId);
		public delegate void AfterChangedDisabledCompanyLogin(object sender, string companyId, string loginId, bool disable);

		//public event OpenCompanyUser OnOpenCompanyUser;
		public event ModifyCompanyUser OnModifyCompanyUser;
		public event SaveCompanyUser OnSaveCompanyUser;
		public event SaveCompanyUserToLogin OnSaveCompanyUserToLogin;
		public event UpdateCompanyUser OnUpdateCompanyUser;
		public event DeleteCompanyUser OnDeleteCompanyUser;
		public event AfterDeleteCompanyUser OnAfterDeleteCompanyUser;
		public event AfterClonedUserCompany OnAfterClonedUserCompany;
		public event AfterChangedDisabledCompanyLogin OnAfterChangedDisabledCompanyLogin;
		#endregion

		#region Delegati ed Eventi per azioni sugli utenti assegnati al Ruolo di una Azienda
		//---------------------------------------------------------------------
		public delegate void ModifyCompanyUserRole(object sender, string id, string companyId, string roleId);
		public delegate void SaveCompanyUserRole(object sender, string id, string companyId, string roleId);
		public delegate void UpdateCompanyUserRole(object sender, string companyId, string roleId);
		public delegate void DeleteCompanyUserRole(object sender, string id, string companyId, string roleId);

		public event ModifyCompanyUserRole OnModifyCompanyUserRole;
		public event SaveCompanyUserRole OnSaveCompanyUserRole;
		public event UpdateCompanyUserRole OnUpdateCompanyUserRole;
		public event DeleteCompanyUserRole OnDeleteCompanyUserRole;
		#endregion

		#region Delegati ed Eventi per la modifica del tree della Microarea Console
		//---------------------------------------------------------------------
		public delegate void AfterModifyCompanyTree(object sender, System.EventArgs e);
		public event AfterModifyCompanyTree OnAfterModifyCompanyTree;
		#endregion

		#region Eventi che abilitano la toolbar della Microarea Console
		//---------------------------------------------------------------------
		public event System.EventHandler OnEnableNewToolBarButton;
		public event System.EventHandler OnEnableOpenToolBarButton;
		public event System.EventHandler OnEnableSaveToolBarButton;
		public event System.EventHandler OnEnableDeleteToolBarButton;
		public event System.EventHandler OnEnableConnectToolBarButton;
		#endregion

		#region Eventi che disabilitano la toolbar della Microarea Console
		//---------------------------------------------------------------------
		public event System.EventHandler OnDisableNewToolBarButton;
		public event System.EventHandler OnDisableOpenToolBarButton;
		public event System.EventHandler OnDisableSaveToolBarButton;
		public event System.EventHandler OnDisableDeleteToolBarButton;
		public event System.EventHandler OnDisableConnectToolBarButton;
		public event System.EventHandler OnDisableExplorerToolBarButton;
		public event System.EventHandler OnDisableOtherObjectsToolBarButton;
		public event System.EventHandler OnDisableShowSecurityIconsToolBarButton;
		public event System.EventHandler OnDisableFindSecurityObjectsToolBarButton;
		public event System.EventHandler OnDisableApplySecurityFilterToolBarButton;
		#endregion

		#region Eventi generici comuni ai PlugIns
		// Eventi da plugIn (in parte ancora non utilizzati)
		//---------------------------------------------------------------------
		public event PlugInLoad OnPlugInLoadHandle;
		public event PlugInUnLoad OnPlugInUnLoadHandle;
		public event ChangeStatusBar OnChangeStatusBarHandle;
		// event to the Console, to force re-initializing LoginManager after changes in connnection string
		public event ChangeConnectionString OnConnectionStringChanged;

		# region Abilita/Disabilita menu del SysAdmin in Console
		// abilita/disabilita l'intero menu a tendina (e quindi anche i suoi sottomenu)
		//---------------------------------------------------------------------
		public delegate void DisableGroupPlugInMenuHandle(string textMenu, System.EventArgs e);
		public event DisableGroupPlugInMenuHandle OnDisableGroupPlugInMenuHandle;
		public delegate void EnableGroupPlugInMenuHandle(string textMenu, System.EventArgs e);
		public event EnableGroupPlugInMenuHandle OnEnableGroupPlugInMenuHandle;
		#endregion

		#endregion

		#region Eventi verso il LoginManager
		public delegate bool GetLoggedUsers(object sender);
		public event GetLoggedUsers OnGetLoggedUsers;

		public delegate bool IsActivated(string application, string functionality);
		public event IsActivated OnIsActivated;

		public delegate bool HasUserAlreadyChangedPasswordToday(string user);
		public event HasUserAlreadyChangedPasswordToday OnHasUserAlreadyChangedPasswordToday;

		public delegate void TraceAction(string company, string login, TraceActionType type, string processName);
		public event TraceAction OnTraceAction;
		#endregion

		#endregion

		#region Costruttore
		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public SysAdmin()
		{
			this.imageListTree = new ImageList();

			// edit server connection config
			this.OnSaveConfigFile += new SysAdmin.SaveConfigFile(this.editConfigFile_OnSave);

			//providers
			this.OnModifyProvider += new SysAdmin.ModifyProvider(this.provider_OnModify);
			this.OnSaveProvider += new SysAdmin.SaveProvider(this.provider_OnSave);
			//users
			this.OnNewUser += new SysAdmin.NewUser(this.user_OnNew);
			this.OnOpenUser += new SysAdmin.OpenUser(this.user_OnOpen);
			this.OnModifyUtente += new SysAdmin.ModifyUtente(this.user_OnModify);
			this.OnSaveUser += new SysAdmin.SaveUser(this.user_OnSave);
			this.OnSaveNewUser += new SysAdmin.SaveNewUser(this.user_OnSaveNew);
			this.OnDeleteUser += new SysAdmin.DeleteUser(this.user_OnDelete);
			this.OnUpdateUser += new SysAdmin.UpdateUser(this.user_OnUpdate);
			//company
			this.OnNewCompany += new SysAdmin.NewCompany(this.company_OnNew);
			this.OnOpenCompany += new SysAdmin.OpenCompany(this.company_OnOpen);
			this.OnModifyCompany += new SysAdmin.ModifyCompany(this.company_OnModify);
			this.OnSaveCompany += new SysAdmin.SaveCompany(this.company_OnSave);
			this.OnDeleteCompany += new SysAdmin.DeleteCompany(this.company_OnDelete);
			this.OnNewCloneCompany += new SysAdmin.NewCloneCompany(this.OnCloneCompanyHandler);
			this.OnBackupDbCompany += new SysAdmin.BackupDbCompany(this.company_OnBackupDb);
			this.OnRestoreDbCompany += new SysAdmin.RestoreDbCompany(this.company_OnRestoreDb);
			this.OnUpdateCompany += new SysAdmin.UpdateCompany(this.company_OnUpdate);
			this.OnUpdateCompanies += new SysAdmin.UpdateCompanies(this.companies_OnUpdate);
			//roles
			this.OnNewRole += new SysAdmin.NewRole(this.role_OnNew);
			this.OnOpenRole += new SysAdmin.OpenRole(this.role_OnOpen);
			this.OnModifyRuolo += new SysAdmin.ModifyRuolo(this.role_OnModify);
			this.OnSaveRole += new SysAdmin.SaveRole(this.role_OnSave);
			this.OnSaveNewRole += new SysAdmin.SaveNewRole(this.role_OnSaveNew);
			this.OnDeleteRole += new SysAdmin.DeleteRole(this.role_OnDelete);
			this.OnUpdateRole += new SysAdmin.UpdateRole(this.role_OnUpdate);
			//companyusers
			//this.OnOpenCompanyUser += new SysAdmin.OpenCompanyUser(this.company_OnModifyCompanyUser);
			this.OnModifyCompanyUser += new SysAdmin.ModifyCompanyUser(this.company_OnModifyCompanyUser);
			this.OnSaveCompanyUser += new SysAdmin.SaveCompanyUser(this.company_OnSaveCompanyUser);
			this.OnSaveCompanyUserToLogin += new SysAdmin.SaveCompanyUserToLogin(this.company_OnSaveCompanyUserToLogin);
			this.OnDeleteCompanyUser += new SysAdmin.DeleteCompanyUser(this.company_OnDeleteCompanyUser);
			this.OnUpdateCompanyUser += new SysAdmin.UpdateCompanyUser(this.company_OnUpdateCompanyUser);
			//company users roles
			this.OnModifyCompanyUserRole += new SysAdmin.ModifyCompanyUserRole(this.company_OnModifyCompanyUserRole);
			this.OnSaveCompanyUserRole += new SysAdmin.SaveCompanyUserRole(this.company_OnSaveCompanyUserRole);
			this.OnDeleteCompanyUserRole += new SysAdmin.DeleteCompanyUserRole(this.company_OnDeleteCompanyUserRole);
			this.OnUpdateCompanyUserRole += new SysAdmin.UpdateCompanyUserRole(this.company_OnUpdateCompanyUserRole);
			ErrorsFromCheckDb = false;
		}
		#endregion

		#region Funzione Load comune a tutti i PlugIn
		/// <summary>
		/// Load
		/// La funzione è definita nella classe PlugIns
		/// TUTTI I PLUGINs DEVONO AVERNE UNA,la console richiama questa per caricare il plugin
		/// </summary>
		/// <param name="mnuConsole"></param>
		/// <param name="trvConsole"></param>
		/// <param name="wkAreaConsole"></param>
		//---------------------------------------------------------------------
		public override void Load
			(
			ConsoleGUIObjects consoleGUIObjects,
			ConsoleEnvironmentInfo consoleEnvironmentInfo,
			LicenceInfo licenceInfo
			)
		{
			consoleTree = consoleGUIObjects.TreeConsole;
			workingAreaConsole = consoleGUIObjects.WkgAreaConsole;
			bottomWorkingAreaConsole = consoleGUIObjects.BottomWkgAreaConsole;
			bottomWorkingAreaConsole.Enabled = false;
			bottomWorkingAreaConsole.Visible = false;
			consoleMenu = consoleGUIObjects.MenuConsole;
			runningAtServer = consoleEnvironmentInfo.RunningFromServer;

			this.consoleEnvironmentInfo = consoleEnvironmentInfo;
			this.licenceInfo = licenceInfo;
			UpdateConsoleMenu(consoleMenu);

			if (OnPlugInLoadHandle != null)
			{
				string message = String.Format(Strings.PlugInLoaded, "System Administrator");
				OnPlugInLoadHandle(this, new DynamicEventsArgs(message));
			}
		}
		#endregion

		#region ReceiveDiagnostic - Le info delle form vengono aggiunte alla diagnostica del SysAdmin
		/// <summary>
		/// ReceiveDiagnostic
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="diagnostic"></param>
		//---------------------------------------------------------------------
		private void ReceiveDiagnostic(object sender, Diagnostic diagnostic)
		{
			if (diagnostic.Error | diagnostic.Warning | diagnostic.Information)
				this.diagnostic.Set(diagnostic);
		}
		#endregion

		#region LogOn - Procedure di Login del SysAdmin

		#region OnClickLogOn - Premuto il pulsante di LogOn dalla maschera di Login
		/// <summary>
		/// OnClickLogOn
		/// </summary>
		//---------------------------------------------------------------------
		private void OnClickLogOn(object sender, System.EventArgs e)
		{
			// se l'utente ha cliccato sul pulsante di Connect della toolbar principale della Console
			// ignoro le impostazioni per l'autologin
			bool enableAutoLoginFromCmdLine = !(sender is ToolStripButton) && !(sender is ToolStripMenuItem);

			DialogResult result = DialogResult.None;
			if (consoleEnvironmentInfo.IsLiteConsole)
			{
				LoginLite loginL = new LoginLite(brandLoader, licenceInfo.Edition, consoleEnvironmentInfo.ConsoleStatus, enableAutoLoginFromCmdLine);
				loginL.HasErrors = false;
				loginL.OnSendDiagnosticLite += new LoginLite.SendDiagnosticLite(this.ReceiveDiagnostic);
				loginL.OnSuccessLogOnLite += new LoginLite.SuccessLogOnLite(this.OnSuccessLogOn);
				loginL.OnUnSuccessLogOnLite += new LoginLite.UnSuccessLogOnLite(this.OnUnSuccessLogOn);
				loginL.OnModifiedServerConnectionConfigLite += new LoginLite.ModifiedServerConnectionConfigLite(OnModifiedServerConnectionConfig);
				loginL.OnLogPlugInsLite += new LoginLite.LogPlugInsLite(LogPlugIns);
				result = loginL.ShowDialog(this.workingAreaConsole.FindForm());
			}
			else
			{
				Login login = new Login(brandLoader, licenceInfo, consoleEnvironmentInfo.ConsoleStatus, enableAutoLoginFromCmdLine);
				login.HasErrors = false;
				login.OnSendDiagnostic += new Login.SendDiagnostic(this.ReceiveDiagnostic);
				login.OnSuccessLogOn += new Login.SuccessLogOn(this.OnSuccessLogOn);
				login.OnUnSuccessLogOn += new Login.UnSuccessLogOn(this.OnUnSuccessLogOn);
				login.OnModifiedServerConnectionConfig += new Login.ModifiedServerConnectionConfig(OnModifiedServerConnectionConfig);
				login.OnLogPlugIns += new Login.LogPlugIns(LogPlugIns);
				result = login.ShowDialog(this.workingAreaConsole.FindForm());
			}

			if (result == DialogResult.OK)
				OnConnectedToSysDB(new EventArgs()); // event to Console, that has to show the AboutBox if product is not registered
		}
		#endregion

		#region CheckServerVersion - controllo preventivo sull'edizione di SQL a cui mi sto connettendo
		//---------------------------------------------------------------------
		private bool CheckServerVersion(object sender)
		{
			//imposto il nome del server e se si tratta di una istanza, completo il nome del server	
			string serverName = currentStatus.ServerName;
			if (!string.IsNullOrWhiteSpace(currentStatus.ServerIstanceName))
				serverName += Path.DirectorySeparatorChar + currentStatus.ServerIstanceName;

			// compongo la stringa di connessione al master con le credenziali impostate alla login
			string connectionString = (licenceInfo.IsAzureSQLDatabase)
				? string.Format(NameSolverDatabaseStrings.SQLAzureConnection, serverName, DatabaseLayerConsts.MasterDatabase, currentStatus.OwnerDbName, currentStatus.OwnerDbPassword)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase, currentStatus.OwnerDbName, currentStatus.OwnerDbPassword);

			TransactSQLAccess sqlAccess = new TransactSQLAccess(licenceInfo.IsAzureSQLDatabase);
			sqlAccess.CurrentStringConnection = connectionString;

			bool result = sqlAccess.TryToConnect();
			if (!result)
				diagnostic.Set(sqlAccess.Diagnostic);

			if (result)
			{
				// controllo che l'edizione del server a cui mi sto connettendo sia compatibile con quella licenziata
				SQLServerEdition sqlEdition = SQLServerEdition.Undefined;
				using (SqlConnection myConnection = new SqlConnection(connectionString))
				{
					myConnection.Open();
					sqlEdition = TBCheckDatabase.GetSQLServerEdition(myConnection);

					if (
						(licenceInfo.IsAzureSQLDatabase && (sqlEdition != SQLServerEdition.SqlAzureV12)) ||
						(!licenceInfo.IsAzureSQLDatabase && (sqlEdition == SQLServerEdition.SqlAzureV12))
						)
					{
						diagnostic.Set(DiagnosticType.Warning, 
							string.Format("The server {0} you are trying to connect is not a {1} version, as expected by your licence. Please choose another server!", serverName,
							licenceInfo.IsAzureSQLDatabase ? "SQL Azure" : "SQL Server Desktop"));
						result = false;
					}
				}
			}

			if (!result)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				ErrorsFromCheckDb = true;
				((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
				return false;
			}

			return result;
		}
		#endregion

		#region OnSuccessLogOn - LogOn effettuato con successo
		/// <summary>
		/// OnSuccessLogOn
		/// </summary>
		//---------------------------------------------------------------------
		private void OnSuccessLogOn(object sender, DynamicEventsArgs e)
		{
			//pulisco la diagnostica
			diagnostic.Clear();
			SetProgressBarMaxValueFromPlugIn(sender, 100);

			string domainUser = string.Empty;
			string serverName = string.Empty;

			SettingsForConnection = e;
			currentStatus = new SysAdminStatus();
			currentStatus.ServerName = e.Get("DbServer").ToString();
			currentStatus.ServerIstanceName = e.Get("DbServerIstance").ToString();
			currentStatus.DataSource = e.Get("DbDataSource").ToString();
			currentStatus.User = e.Get("DbDefaultUser").ToString();
			currentStatus.IntegratedConnection = Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity"));
			currentStatus.Password = e.Get("DbDefaultPassword").ToString();

			bool isNewDatabase = Boolean.Parse(e.Get("IsNewDataBase").ToString());

			//imposto il nome del server e se si tratta di una istanza, completo il nome del server	
			serverName = currentStatus.ServerName;
			if (!string.IsNullOrWhiteSpace(currentStatus.ServerIstanceName))
				serverName += Path.DirectorySeparatorChar + currentStatus.ServerIstanceName;

			currentStatus.OwnerDbName = currentStatus.User;
			currentStatus.OwnerDbPassword = currentStatus.Password;

			// prima di procedere devo controllare che il SQL Server a cui mi sto connettendo sia compatibile
			// con la licenza (se ho una licenza per Azure NON posso connettermi ad un server locale e viceversa)
			if (!CheckServerVersion(sender))
				return;

			if (licenceInfo.IsAzureSQLDatabase)
			{
				currentStatus.ConnectionString =
					string.Format(NameSolverDatabaseStrings.SQLAzureConnection, serverName, currentStatus.DataSource, currentStatus.OwnerDbName, currentStatus.OwnerDbPassword);

				ConnectToAzureSystemDB(isNewDatabase, sender);
				return;
			}

			currentStatus.ConnectionString =
				string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, currentStatus.DataSource, currentStatus.OwnerDbName, currentStatus.OwnerDbPassword);

			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();

			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(HelpFromPopUp);

			if (isNewDatabase)
			{
				// tento di connettermi al server e successivamente creo un nuovo database di sistema
				ConnectToNewSystemDB(dataToConnectionServer, connSqlTransact, sender);
			}
			else
			{
				currentStatus.User = currentStatus.OwnerDbName;
				currentStatus.Password = currentStatus.OwnerDbPassword;
				string serverComplete = String.IsNullOrWhiteSpace(currentStatus.ServerIstanceName)
					? currentStatus.ServerName
					: Path.Combine(currentStatus.ServerName, currentStatus.ServerIstanceName);

				// mi connetto al master per testare l'esistenza della login
				connSqlTransact.CurrentStringConnection =
					string.Format(NameSolverDatabaseStrings.SQLConnection, serverComplete, DatabaseLayerConsts.MasterDatabase, currentStatus.OwnerDbName, currentStatus.OwnerDbPassword);

				if (connSqlTransact.ExistLogin(currentStatus.OwnerDbName))
				{
					if (!consoleEnvironmentInfo.IsLiteConsole)
					{
						// ha i ruoli server corretti?
						if (!connSqlTransact.LoginIsSystemAdminRole(currentStatus.OwnerDbName, DatabaseLayerConsts.RoleSysAdmin))
						{
							diagnosticViewer.Message = string.Format(Strings.NoPermissionUser, currentStatus.OwnerDbName);
							diagnosticViewer.Title = Strings.Error;
							diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
							diagnosticViewer.ShowIcon = MessageBoxIcon.Error;
							DialogResult responseForRoleSQLUser = diagnosticViewer.Show();
							if (responseForRoleSQLUser == DialogResult.No)
							{
								if (dataToConnectionServer != null)
									dataToConnectionServer.Undo();
								ErrorsFromCheckDb = true;
								((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
								return;
							}
							else
							{
								bool toGrant = false;
								if (!connSqlTransact.ExistLoginIntoDb(currentStatus.OwnerDbName, DatabaseLayerConsts.MasterDatabase))
									toGrant = true;
								string oldString = connSqlTransact.CurrentStringConnection;
								string loginToRolePermission = currentStatus.OwnerDbName;
								dataToConnectionServer =
									connSqlTransact.LoginImpersonification
									(
									DatabaseLayerConsts.LoginSa,
									string.Empty,
									string.Empty,
									currentStatus.IntegratedConnection,
									currentStatus.ServerName,
									currentStatus.ServerIstanceName,
									false
									);
								if (dataToConnectionServer == null)
								{
									ErrorsFromCheckDb = true;
									((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
									return;
								}
								//setto la stringa di connessione con le nuove credenziali
								connSqlTransact.CurrentStringConnection =
									string.Format(NameSolverDatabaseStrings.SQLConnection, serverComplete, DatabaseLayerConsts.MasterDatabase, dataToConnectionServer.Login, dataToConnectionServer.Password);

								//se è standard edition devo controllare che il db sia MSDE
								if (!CheckIfEditionAndDBNetworkAreCorrect(connSqlTransact.CurrentStringConnection, serverComplete))
								{
									ErrorsFromCheckDb = true;
									((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
									return;
								}

								bool addPermission = false;
								if (toGrant)
									addPermission =
										connSqlTransact.SPGrantDbAccess(loginToRolePermission, loginToRolePermission, DatabaseLayerConsts.MasterDatabase) &&
										connSqlTransact.SPAddSrvRoleMember(loginToRolePermission, DatabaseLayerConsts.RoleSysAdmin, string.Empty);
								else
									addPermission = connSqlTransact.SPAddSrvRoleMember(loginToRolePermission, DatabaseLayerConsts.RoleSysAdmin, string.Empty);

								if (!addPermission)
								{
									if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning || connSqlTransact.Diagnostic.Information)
									{
										diagnostic.Set(connSqlTransact.Diagnostic);
										DiagnosticViewer.ShowDiagnostic(diagnostic);
									}
									ErrorsFromCheckDb = true;
									((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
									if (dataToConnectionServer != null)
										dataToConnectionServer.Undo();
									return;
								}
								connSqlTransact.CurrentStringConnection = oldString;
							}
						}
					}
				}
				else
				{
					// devo ri-assegnare la stringa di connessione specificando il nome del database di sistema
					connSqlTransact.CurrentStringConnection =
						string.Format(NameSolverDatabaseStrings.SQLConnection, serverComplete, currentStatus.DataSource, currentStatus.OwnerDbName, currentStatus.OwnerDbPassword);

					if (!connSqlTransact.TryToConnect())
					{
						DiagnosticViewer.ShowDiagnosticAndClear(connSqlTransact.Diagnostic);
						ErrorsFromCheckDb = true;
						((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
						return;
					}

					if (!connSqlTransact.LoginIsDBOwnerRole(currentStatus.OwnerDbName))
					{
						diagnosticViewer.Message = string.Format(Strings.NoPermissionUserLite, currentStatus.OwnerDbName);
						diagnosticViewer.Title = Strings.Error;
						diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
						diagnosticViewer.ShowIcon = MessageBoxIcon.Error;
						diagnosticViewer.Show();
						ErrorsFromCheckDb = true;
						((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
						return;
					}

					// se è standard edition devo controllare che il db sia MSDE
					if (!CheckIfEditionAndDBNetworkAreCorrect(connSqlTransact.CurrentStringConnection, serverComplete))
					{
						ErrorsFromCheckDb = true;
						((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
						return;
					}
				}

				// controllo che il database al quale mi sto connettendo non contenga la tabella TB_DBMark
				// e visualizzo un msg
				TBConnection myConnection = null;
				TBDatabaseSchema mySchema = null;
				try
				{
					// istanzio una TBConnection e la apro
					myConnection = new TBConnection(currentStatus.ConnectionString, DBMSType.SQLSERVER);
					myConnection.Open();

					// faccio un controllo sul server per capire se e' in Mixed Mode (altrimenti visualizzo un msg e non faccio procedere)
					if (!TBCheckDatabase.GetSQLServerIsInMixedMode(myConnection))
					{
						diagnosticViewer.Message = string.Format(Strings.IntegratedSecurityModeOnly, serverComplete);
						diagnosticViewer.Title = Strings.Error;
						diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
						diagnosticViewer.ShowIcon = MessageBoxIcon.Error;
						diagnosticViewer.Show();
						if (dataToConnectionServer != null)
							dataToConnectionServer.Undo();
						ErrorsFromCheckDb = true;
						((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
						return;
					}

					// istanzio TBDatabaseSchema sulla connessione
					mySchema = new TBDatabaseSchema(myConnection);
					// se la tabella di riferimento TB_DBMark esiste, visualizzo un msg all'utente
					if (mySchema.ExistTable(DatabaseLayerConsts.TB_DBMark))
					{
						diagnosticViewer.Message = string.Format(Strings.WrongLoginOnCompanyDB, currentStatus.DataSource);
						diagnosticViewer.ShowIcon = MessageBoxIcon.Question;
						diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
						diagnosticViewer.DefaultButton = MessageBoxDefaultButton.Button2;
						DialogResult result = diagnosticViewer.Show();

						if (result == DialogResult.No)
						{
							ErrorsFromCheckDb = true;
							if (dataToConnectionServer != null)
								dataToConnectionServer.Undo();
							((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
							return;
						}
					}
				}
				catch
				{ }
				finally
				{
					if (myConnection != null && myConnection.State != ConnectionState.Closed)
					{
						myConnection.Close();
						myConnection.Dispose();
					}
				}

				// passo useUnicode a false. E' l'ApplicationDBAdmin che fa poi il controllo sulla tabella dbmark
				// per capire se il db e' unicode o meno.
				bool useUnicode = false;
				OnBeforeConnectionSystemDB?.Invoke(this, currentStatus.ConnectionString, false, useUnicode, currentStatus.User, currentStatus.DataSource);
				((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
			}
		}

		///<summary>
		/// Connessione ad un nuovo database di sistema
		///</summary>
		//---------------------------------------------------------------------
		private bool ConnectToAzureSystemDB(bool isNewDatabase, object sender)
		{
			//imposto il nome del server e se si tratta di una istanza, completo il nome del server	
			string serverName = currentStatus.ServerName;
			if (!string.IsNullOrWhiteSpace(currentStatus.ServerIstanceName))
				serverName += Path.DirectorySeparatorChar + currentStatus.ServerIstanceName;

			// se si tratta di un nuovo db mi connetto al master, altrimenti vado sul nome db scelto nella combo
			currentStatus.ConnectionString =
				string.Format(NameSolverDatabaseStrings.SQLAzureConnection, serverName, (isNewDatabase) ? DatabaseLayerConsts.MasterDatabase : currentStatus.DataSource, currentStatus.OwnerDbName, currentStatus.OwnerDbPassword);

			TransactSQLAccess sqlAccess = new TransactSQLAccess(true);
			sqlAccess.CurrentStringConnection = currentStatus.ConnectionString;

			// provo a connettermi con le credenziali fornite
			if (!sqlAccess.TryToConnect())
			{
				diagnostic.Set(sqlAccess.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				ErrorsFromCheckDb = true;
				((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
				return false;
			}

			// controllo che l'edizione del server a cui mi sto connettendo sia compatibile con quella licenziata
			SQLServerEdition sqlEdition = SQLServerEdition.Undefined;
			using (SqlConnection myConnection = new SqlConnection(sqlAccess.CurrentStringConnection))
			{
				myConnection.Open();
				sqlEdition = TBCheckDatabase.GetSQLServerEdition(myConnection);
				if (sqlEdition != SQLServerEdition.SqlAzureV12)
				{
					diagnostic.Set(DiagnosticType.Warning, string.Format("The server {0} you are trying to connect is not a SQL Azure version, as expected by your licence. Please choose another one!", serverName));
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					ErrorsFromCheckDb = true;
					((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
					return false;
				}
			}

			// si tratta di un nuovo database
			if (isNewDatabase)
			{
				// imposto gia' la stringa di connessione con il nome del nuovo database 
				currentStatus.ConnectionString =
					string.Format(NameSolverDatabaseStrings.SQLAzureConnection, serverName, currentStatus.DataSource, currentStatus.OwnerDbName, currentStatus.OwnerDbPassword);

				// ora chiamo il metodo che effettivamente crea il database di sistema, con la form con i parametri
				if (!CreateSystemDB(sender))
					return false;

				//currentStatus.CurrentConnection = azureAccess.OpenConnection(); // serve?
			}
			else
			{
				// passo useUnicode a false. E' l'ApplicationDBAdmin che fa poi il controllo sulla tabella dbmark
				// per capire se il db e' unicode o meno.
				bool useUnicode = false;
				OnBeforeConnectionSystemDB?.Invoke(this, currentStatus.ConnectionString, false, useUnicode, currentStatus.User, currentStatus.DataSource);
			}

			((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;

			return true;
		}

		///<summary>
		/// Connessione ad un nuovo database di sistema
		///</summary>
		//---------------------------------------------------------------------
		private void ConnectToNewSystemDB(UserImpersonatedData dataToConnectionServer, TransactSQLAccess connSqlTransact, object sender)
		{
			currentStatus.User = currentStatus.OwnerDbName;
			currentStatus.Password = currentStatus.OwnerDbPassword;

			string serverComplete = String.IsNullOrWhiteSpace(currentStatus.ServerIstanceName)
				? currentStatus.ServerName
				: Path.Combine(currentStatus.ServerName, currentStatus.ServerIstanceName);

			connSqlTransact.CurrentStringConnection = currentStatus.IntegratedConnection
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverComplete, DatabaseLayerConsts.MasterDatabase)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, serverComplete, DatabaseLayerConsts.MasterDatabase, currentStatus.OwnerDbName, currentStatus.OwnerDbPassword);

			if (!connSqlTransact.ExistLogin(currentStatus.OwnerDbName))
			{
				//se è l'utente sa non posso inserirlo!
				if (string.Compare(currentStatus.OwnerDbName, DatabaseLayerConsts.LoginSa, true, CultureInfo.InvariantCulture) == 0)
				{
					if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning)
						diagnostic.Set(connSqlTransact.Diagnostic);
					else
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.WrongSaPassword, serverComplete, currentStatus.OwnerDbName));

					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (dataToConnectionServer != null)
						dataToConnectionServer.Undo();
					ErrorsFromCheckDb = true;
					((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
					return;
				}

				diagnosticViewer.Message = string.Format(Strings.WrongUser, serverComplete, currentStatus.OwnerDbName);
				diagnosticViewer.Title = Strings.Error;
				diagnosticViewer.ShowIcon = MessageBoxIcon.Warning;
				diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
				DialogResult responseForUserSQLInsert = diagnosticViewer.Show();

				if (responseForUserSQLInsert == DialogResult.No)
				{
					if (dataToConnectionServer != null)
						dataToConnectionServer.Undo();
					ErrorsFromCheckDb = true;
					((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
					return;
				}

				//procedo a inserire l'utente. Mi devo però loginare come "almeno" sa
				//che spero possa inserire l'utente (sono in modalita SQL)
				string newSqlLogin = currentStatus.OwnerDbName;
				string newSqlPassword = currentStatus.OwnerDbPassword;
				string oldStringConn = connSqlTransact.CurrentStringConnection;

				//Rieseguo la login
				dataToConnectionServer = connSqlTransact.LoginImpersonification
					(
					DatabaseLayerConsts.LoginSa,
					string.Empty,
					string.Empty,
					currentStatus.IntegratedConnection,
					currentStatus.ServerName,
					currentStatus.ServerIstanceName,
					false
					);

				if (dataToConnectionServer == null)
				{
					ErrorsFromCheckDb = true;
					((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
					return;
				}

				// se la connessione è aperta, la chiudo
				// setto la nuova stringa di connessione con sa e la sua password
				connSqlTransact.CurrentStringConnection =
					string.Format(NameSolverDatabaseStrings.SQLConnection, serverComplete, DatabaseLayerConsts.MasterDatabase, dataToConnectionServer.Login, dataToConnectionServer.Password);

				if (!CheckIfEditionAndDBNetworkAreCorrect(connSqlTransact.CurrentStringConnection, serverComplete))
				{
					ErrorsFromCheckDb = true;
					((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
					return;
				}

				bool loginInserted =
					connSqlTransact.SPAddLogin(newSqlLogin, newSqlPassword, DatabaseLayerConsts.MasterDatabase) &&
					connSqlTransact.SPGrantDbAccess(newSqlLogin, newSqlLogin, DatabaseLayerConsts.MasterDatabase) &&
					connSqlTransact.SPAddSrvRoleMember(newSqlLogin, DatabaseLayerConsts.RoleSysAdmin, string.Empty);

				if (!loginInserted)
				{
					//impossibile inserire, interrompo
					if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning || connSqlTransact.Diagnostic.Information)
					{
						diagnostic.Set(connSqlTransact.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					}
					if (dataToConnectionServer != null)
						dataToConnectionServer.Undo();
					ErrorsFromCheckDb = true;
					((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
					return;
				}

				connSqlTransact.CurrentStringConnection = oldStringConn;
				//setto di nuovo l'utente come era prima
				//ho inserito l'utente
				dataToConnectionServer.Login = newSqlLogin;
				dataToConnectionServer.Password = newSqlPassword;
			}
			else // la login esiste ma non ha il ruolo SysAdmin ?
				if (!connSqlTransact.LoginIsSystemAdminRole(currentStatus.OwnerDbName, DatabaseLayerConsts.RoleSysAdmin))
				{
					//se è standard edition devo controllare che il db sia MSDE
					if (!CheckIfEditionAndDBNetworkAreCorrect(connSqlTransact.CurrentStringConnection, serverComplete))
					{
						ErrorsFromCheckDb = true;
						((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
						return;
					}

					diagnosticViewer.Message = string.Format(Strings.NoPermissionUser, currentStatus.OwnerDbName);
					diagnosticViewer.Title = Strings.Error;
					diagnosticViewer.ShowButtons = MessageBoxButtons.OKCancel;
					diagnosticViewer.ShowIcon = MessageBoxIcon.Error;

					DialogResult responseForRoleSQLUser = diagnosticViewer.Show();
					if (responseForRoleSQLUser == DialogResult.Cancel)
					{
						if (dataToConnectionServer != null)
							dataToConnectionServer.Undo();
						ErrorsFromCheckDb = true;
						((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
						return;
					}
					else
					{
						//attribuisco il ruolo sysAdmin alla logins selezionata
						bool toGrant = false;
						if (!connSqlTransact.ExistLoginIntoDb(currentStatus.OwnerDbName, DatabaseLayerConsts.MasterDatabase))
							toGrant = true;

						string oldString = connSqlTransact.CurrentStringConnection;
						string loginToRolePermission = currentStatus.OwnerDbName;
						dataToConnectionServer = connSqlTransact.LoginImpersonification
							(
							DatabaseLayerConsts.LoginSa,
							string.Empty,
							string.Empty,
							currentStatus.IntegratedConnection,
							currentStatus.ServerName,
							currentStatus.ServerIstanceName,
							false
							);

						if (dataToConnectionServer == null)
						{
							ErrorsFromCheckDb = true;
							((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
							return;
						}
						//setto la stringa di connessione con le nuove credenziali
						connSqlTransact.CurrentStringConnection =
							string.Format(NameSolverDatabaseStrings.SQLConnection, serverComplete, DatabaseLayerConsts.MasterDatabase, dataToConnectionServer.Login, dataToConnectionServer.Password);

						bool addPermission = false;
						if (toGrant)
							addPermission =
								connSqlTransact.SPGrantDbAccess(loginToRolePermission, loginToRolePermission, DatabaseLayerConsts.MasterDatabase) &&
								connSqlTransact.SPAddSrvRoleMember(loginToRolePermission, DatabaseLayerConsts.RoleSysAdmin, string.Empty);
						else
							addPermission = connSqlTransact.SPAddSrvRoleMember(loginToRolePermission, DatabaseLayerConsts.RoleSysAdmin, string.Empty);

						if (!addPermission)
						{
							if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning || connSqlTransact.Diagnostic.Information)
							{
								diagnostic.Set(connSqlTransact.Diagnostic);
								DiagnosticViewer.ShowDiagnostic(diagnostic);
							}
							ErrorsFromCheckDb = true;
							((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
							if (dataToConnectionServer != null)
								dataToConnectionServer.Undo();
							return;
						}
						connSqlTransact.CurrentStringConnection = oldString;
					}
				}
				else
				{
					// se è standard edition devo controllare che il db sia MSDE
					if (!CheckIfEditionAndDBNetworkAreCorrect(connSqlTransact.CurrentStringConnection, serverComplete))
					{
						ErrorsFromCheckDb = true;
						((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
						return;
					}
				}

			Cursor.Current = Cursors.WaitCursor;

			// se sono arrivata a questo punto significa che le credenziali inserite sono corrette e valide
			// pertanto chiamo il metodo che effettivamente crea il database di sistema, con la form con i parametri
			if (!CreateSystemDB(sender))
			{
				if (dataToConnectionServer != null)
					dataToConnectionServer.Undo();
				return;
			}

			// setto la stringa di connessione
			connSqlTransact.CurrentStringConnection = currentStatus.ConnectionString;
			//currentStatus.CurrentConnection = connSqlTransact.OpenConnection(); // secondo me questo non serve

			((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
		}

		//---------------------------------------------------------------------
		private bool CreateSystemDB(object sender)
		{
			// mostro la nuova form con i parametri per la creazione del database
			CreateDBForm createDBForm = new CreateDBForm
			(
				false,								// isCompanyDB
				currentStatus.ServerName,			// serverName
				currentStatus.ServerIstanceName,	// istanceName
				currentStatus.DataSource,			// databaseName
				currentStatus.OwnerDbName,			// loginName
				currentStatus.OwnerDbPassword,		// password
				currentStatus.IntegratedConnection,	// isWinAuth
				true,								// showUnicodeCheck,
				true,								// enableUnicodeCheck,
				this.licenceInfo.UseUnicodeSet(),	// initUnicodeCheckValue
				this.licenceInfo.DBNetworkType,		// dbNetworkType
				string.Empty,						// initDBCulture (only company db)
				true,                               // disableDBCultureComboBox (superfluo, intanto la combobox non e' visibile per il sysdb)
				isAzureDb: this.licenceInfo.IsAzureSQLDatabase
			);

			// se l'utente ha cliccato su Cancel non procedo
			bool resultCreationDB = (createDBForm.ShowDialog() == DialogResult.Cancel) ? false : createDBForm.Result;

			Cursor.Current = Cursors.Default;

			if (resultCreationDB)
			{
				diagnostic.Set(DiagnosticType.Information, string.Format(Strings.DatabaseCreated, currentStatus.DataSource));
				OnBeforeConnectionSystemDB?.Invoke(this, currentStatus.ConnectionString, resultCreationDB, createDBForm.UseUnicode, currentStatus.OwnerDbName, currentStatus.DataSource);
				((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
			}
			else
			{
				ErrorsFromCheckDb = true;
				((ILoginFormHasError)sender).HasErrors = ErrorsFromCheckDb;
				if (createDBForm.CreateDBDiagnostic != null &&
					(createDBForm.CreateDBDiagnostic.Error || createDBForm.CreateDBDiagnostic.Warning || createDBForm.CreateDBDiagnostic.Information))
					diagnostic.Set(createDBForm.CreateDBDiagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, Strings.AbortingDBCreation);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			}

			return resultCreationDB;
		}
		#endregion

		#region OnAfterSuccessTableCreation - Dopo che ApplicationDBAdmin ha terminato la creazione delle tabelle
		/// <summary>
		/// OnAfterSuccessTableCreation
		/// Da collegare all'evento di creazione tabelle mandatomi da ApplicationDBAdmin
		/// </summary>
		[AssemblyEvent("Microarea.Console.Plugin.ApplicationDBAdmin.ApplicationDBAdmin", "OnStatusCreationSystemDB")]
		//-------------------------------------------------------------------------------------------
		public void OnAfterSuccessCheckDataBase(object sender, bool success, string messageAfterCheckDataBase, Diagnostic dbAdminDiagnostic)
		{
			TransactSQLAccess connSqlTransact = new TransactSQLAccess();

			string strServerName =
					(string.IsNullOrEmpty(currentStatus.ServerIstanceName))
					? currentStatus.ServerName
					: Path.Combine(currentStatus.ServerName, currentStatus.ServerIstanceName);

			//se è andato tutto bene mostro la message box
			bool silent = !success;

			// ApplicationDBAdmin ha creato le tabelle nel db e se non ci sono errori mi loggo
			if (success)
			{
				if (!CheckIfEditionAndDBNetworkAreCorrect(currentStatus.ConnectionString, strServerName))
				{
					ErrorsFromCheckDb = true;
					return;
				}

				this.consoleEnvironmentInfo.ConsoleUserInfo.UserName = currentStatus.OwnerDbName;
				this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd = currentStatus.OwnerDbPassword;
				this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth = currentStatus.IntegratedConnection;
				this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName =
					string.IsNullOrEmpty(currentStatus.ServerIstanceName)
					? currentStatus.ServerName
					: Path.Combine(currentStatus.ServerName, currentStatus.ServerIstanceName);
				this.consoleEnvironmentInfo.ConsoleUserInfo.DbName = currentStatus.DataSource;

				OnRefreshConsoleStatus?.Invoke(this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName);

				Cursor.Current = Cursors.WaitCursor;

				// se scelgo la modalità non silente mostro il messaggio di avvenuta creazione del db
				if (!silent)
				{
					Cursor.Current = Cursors.Default;
					if (messageAfterCheckDataBase.Length > 0)
					{
						ExtendedInfo extendedInfo = null;
						if (dbAdminDiagnostic != null)
						{
							if (dbAdminDiagnostic.Error)
							{
								extendedInfo = new ExtendedInfo();
								IDiagnosticItems items = dbAdminDiagnostic.AllMessages();
								foreach (DiagnosticItem item in items)
									extendedInfo.Add(Strings.Error, item.FullExplain);
							}
						}
						diagnostic.Set(DiagnosticType.Information, messageAfterCheckDataBase, extendedInfo);
					}
					Cursor.Current = Cursors.WaitCursor;
				}

				//Ora devo aggiungere alla tabella MSD_Logins il dbo del database di sistema creato
				connSqlTransact.CurrentStringConnection = currentStatus.ConnectionString;

				// Controllo che il database contenga le tabelle principali (per sicurezza)
				if (!connSqlTransact.IsSysAdminDataBase(currentStatus.DataSource))
				{
					Cursor.Current = Cursors.Default;
					if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning || connSqlTransact.Diagnostic.Information)
						diagnostic.Set(connSqlTransact.Diagnostic);
					else
						diagnostic.Set(DiagnosticType.Error, Microarea.TaskBuilderNet.Data.SQLDataAccess.SQLDataAccessStrings.ErrorDataBaseSysAdminConnection);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					return;
				}

				// mi connetto 
				currentStatus.CurrentConnection = new SqlConnection(currentStatus.ConnectionString);
				currentStatus.CurrentConnection.Open();

				//Se l'utente dbo del db non è presente nel database MSD_Logins lo aggiungo
				UserDb userdb = new UserDb();
				userdb.ConnectionString = currentStatus.ConnectionString;
				userdb.CurrentSqlConnection = currentStatus.CurrentConnection;

				AddUserAuthenticatedFromConsole(currentStatus.User, currentStatus.Password, strServerName, DBMSType.SQLSERVER);

				if (!userdb.ExistLoginAlsoDisabled(currentStatus.User))
				{
					userdb.Add
						(
						false,
						currentStatus.User,
						currentStatus.Password,
						currentStatus.User,
						DateTime.Today.ToShortDateString(),
						false,
						false,
						false,
						false,
						true, /* la pwd non scade mai */
						string.Empty,
						string.Empty,
						string.Empty,
						false,
						true,
						false,
						false,
						false,   //amministratore area riservata sul sito web
						"0"
						);

					if (string.Compare(currentStatus.User, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) != 0)
					{
						// se NON si tratta di Console Lite e la licenza non e' SQL Azure non procedo all'aggiunta del ruolo di sysadmin
						if (!consoleEnvironmentInfo.IsLiteConsole && !licenceInfo.IsAzureSQLDatabase)
						{
							if (!connSqlTransact.SPAddSrvRoleMember(currentStatus.User, DatabaseLayerConsts.RoleSysAdmin, currentStatus.DataSource))
							{
								//non sono riuscita a settare la role ma il db è ok segnalo il problema ma vado avanti
								diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.CannotSettingRoleSysAdmin, currentStatus.OwnerDbName, currentStatus.DataSource));
								DiagnosticViewer.ShowDiagnostic(diagnostic);
							}
						}
					}
				}

				//*******************************************************************************************************
				// Eliminazione utente applicativo EasyLookSystem dal database di sistema in modalità silente (M. 3786)	*
				// [dopo la connessione al db di sistema elimino l'utente e tutte le sue associazioni alle aziende]		*
				//*******************************************************************************************************
				ArrayList easyLookUser;
				if (userdb.LoadFromLogin(out easyLookUser, NameSolverStrings.EasyLookSystemLogin))
				{
					if (easyLookUser != null && easyLookUser.Count > 0)
					{
						UserItem el = (UserItem)easyLookUser[0];
						if (el != null && !userdb.Delete(el.LoginId))
						{
							diagnostic.Set(userdb.Diagnostic);
							diagnostic.Set(DiagnosticType.Warning, Strings.ErrorDeletingEasyLookSystem);
						}
					}
				}
				else
					diagnostic.Set(userdb.Diagnostic);

				// Inserisco nella tabella MSD_Providers i vari providers di database
				ProviderDb providerDb = new ProviderDb();
				providerDb.ConnectionString = currentStatus.ConnectionString;
				providerDb.CurrentSqlConnection = currentStatus.CurrentConnection;

				ArrayList providers = new ArrayList();
				providerDb.SelectAllProviders(out providers);

				//inserisco il provider MS Sql
				if (!providerDb.ExistProvider(NameSolverDatabaseStrings.SQLOLEDBProvider))
				{
					if (!providerDb.Add(DBMSType.SQLSERVER))
					{
						if (providerDb.Diagnostic.Error || providerDb.Diagnostic.Warning || providerDb.Diagnostic.Information)
							diagnostic.Set(providerDb.Diagnostic);
						else
						{
							diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ProviderAdding, DatabaseLayerConsts.SqlOleProviderDescription));
							success = false;
						}
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					}
				}

				/* if (!providerDb.ExistProvider(NameSolverDatabaseStrings.PostgreOdbcProvider))
                {
                    if (!providerDb.Add(DBMSType.POSTGRE,true))
                    {
                        if (providerDb.Diagnostic.Error || providerDb.Diagnostic.Warning || providerDb.Diagnostic.Information)
                            diagnostic.Set(providerDb.Diagnostic);
                        else
                        {
                            diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ProviderAdding, DatabaseLayerConsts.PostgreProviderDescription));
                            success = false;
                        }
                        DiagnosticViewer.ShowDiagnostic(diagnostic);
                    }
                }*/

				if (!providerDb.ExistProvider(NameSolverDatabaseStrings.OraOLEDBProvider))
				{
					if (!providerDb.Add(DBMSType.ORACLE))
					{
						if (providerDb.Diagnostic.Error || providerDb.Diagnostic.Warning || providerDb.Diagnostic.Information)
							diagnostic.Set(providerDb.Diagnostic);
						else
						{
							diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ProviderAdding, DatabaseLayerConsts.OracleProviderDescription));
							success = false;
						}
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					}
				}

				// se la DBNetwork è Undefined dò un msg di errore
				if (licenceInfo.DBNetworkType == DBNetworkType.Undefined)
				{
					diagnostic.Set(DiagnosticType.Error, Strings.UnableToReadSystemDatabaseVersion);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					success = false;
				}
			}
			else
			{
				// l'ApplicationDBAdmin ha ottenuto degli errori in fase di creazione/agg. database di sistema
				if (!string.IsNullOrEmpty(messageAfterCheckDataBase))
				{
					IExtendedInfo extendedInfo = null;
					if (dbAdminDiagnostic != null)
					{
						if (dbAdminDiagnostic.Error)
						{
							extendedInfo = new ExtendedInfo();
							IDiagnosticItems items = dbAdminDiagnostic.AllMessages();
							foreach (DiagnosticItem item in items)
								if (item.ExtendedInfo != null && item.ExtendedInfo.Count > 0)
									extendedInfo = item.ExtendedInfo;
								else
									extendedInfo.Add(Strings.Error, item.FullExplain);
						}
					}
					diagnostic.Set(DiagnosticType.Error, messageAfterCheckDataBase, extendedInfo);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
				}
			}

			ErrorsFromCheckDb = !success;
		}
		#endregion

		#region LogPlugIns - Effettua il caricamento dei PlugIns
		/// <summary>
		/// LogPlugIns
		/// Effettua il caricamento dei PlugIns
		/// </summary>
		//---------------------------------------------------------------------
		private void LogPlugIns(object sender)
		{
			//Disabilito il bottone di connessione e abilito quello di disconnessione
			if (OnDisableConnectToolBarButton != null)
				OnDisableConnectToolBarButton(sender, new System.EventArgs());
			
			//Aggiorno l'albero della console con i dati prelevati dalle tabelle
			UpdateConsoleTree(this.consoleTree);
			//Aggiorno i menu della Console, il SysAdmin aggiunge un MenuItem
			UpdateMainMenu(this.consoleMenu, true);

			Cursor.Current = Cursors.Default;
			Cursor.Show();
			workingAreaConsole.Controls.Clear(); //pulisco la working area

			//Aggiungo l'utente corrente alla lista autenticata degli utenti della console
			string serverComplete = (currentStatus.ServerIstanceName.Length == 0)
				? currentStatus.ServerName
				: currentStatus.ServerName + Path.DirectorySeparatorChar + currentStatus.ServerIstanceName;

			AddUserAuthenticatedFromConsole(currentStatus.User, currentStatus.Password, serverComplete, DBMSType.SQLSERVER);

			//verifico lo stato di console (ed eventualmente mostro i panel)
			if (OnRefreshConsoleStatus != null)
				OnRefreshConsoleStatus(serverComplete);

			//prendo lo stato e lo memorizzo
			if (OnGetConsoleStatus != null)
				consoleEnvironmentInfo.ConsoleStatus = OnGetConsoleStatus();

			//Dico ai Plug-Ins senza autenticazione che il SysAdmin si è loggato
			if (OnAfterLogOn != null)
				OnAfterLogOn(sender, SettingsForConnection);

			// mando un evento alla form della MConsole per abilitare il menu "Il mio Mago.net"
			if (OnEnableMenuAfterLogOperation != null)
				OnEnableMenuAfterLogOperation(true);
		}
		#endregion

		#region OnUnSuccessLogOn - LogOn effettuato senza successo
		/// <summary>
		/// OnUnSuccessLogOn
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnUnSuccessLogOn(object sender, System.EventArgs e)
		{
			ErrorsFromCheckDb = false;
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth = false;
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName = string.Empty;
			UpdateMainMenu(consoleMenu, false);
		}
		#endregion

		#region OnModifiedServerConnectionConfig - Ho modificato la stringa di connessione e salvo il serverConnection.config
		/// <summary>
		/// OnModifiedServerConnectionConfig
		/// </summary>
		//---------------------------------------------------------------------
		private bool OnModifiedServerConnectionConfig(object sender, bool created, string connectionString)
		{
			bool saveConfigFile = true;

			oldConnStringSysDB = InstallationData.ServerConnectionInfo.SysDBConnectionString;

			//sto creando il file 
			if (created)
			{
				//salvo il file
				SaveServerConnectionConfigFile(connectionString);
				//Ora dico alla Console che deve ri-caricare il LoginManager
				OnConnectionStringChanged(sender, new DynamicEventsArgs(oldConnStringSysDB));
				//Ora dico alla console di re-inizializzarsi
				bool waiting = OnAfterCreateServerConnection(sender);
				if (!waiting)
					return saveConfigFile;
			}
			else //sto modificando il serverConnection.config
			{
				//Dico alla Console di contattare il LoginManager e veriricare se ci sono utenti connessi
				if (GetLoggedUsersFromLoginManager(sender))
				{
					//chiedo conferma prima di Init
					diagnosticViewer.Message = Strings.ExistLoggedUsers;
					diagnosticViewer.Title = Strings.InfoConnectionNotUpdated;
					diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
					diagnosticViewer.ShowIcon = MessageBoxIcon.Information;
					DialogResult executeInit = diagnosticViewer.Show();
					if (executeInit == DialogResult.Yes)
					{
						SaveServerConnectionConfigFile(connectionString);
						OnConnectionStringChanged(sender, new DynamicEventsArgs(oldConnStringSysDB));
						//Ora dico alla console di re-inizializzarsi
						if (!OnAfterCreateServerConnection(sender))
							return saveConfigFile;
					}
					else
					{
						//Messaggio di Impossibilità a continuare la connessione alla MC
						diagnostic.Set(DiagnosticType.Error, Strings.CannotSaveConnectionInfo);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						saveConfigFile = false;
					}
				}
				else
				{
					SaveServerConnectionConfigFile(connectionString);
					OnConnectionStringChanged(sender, new DynamicEventsArgs(oldConnStringSysDB));
					//Ora dico alla console di re-inizializzarsi
					if (!OnAfterCreateServerConnection(sender))
						return saveConfigFile;
				}
			}
			//Dico ai Plug-Ins interessati che il SysAdmin si è loggato
			//if (OnAfterLogOn != null) OnAfterLogOn(this, SettingsForConnection);
			return saveConfigFile;
		}

		/// <summary>
		/// SaveServerConnectionConfigFile
		/// </summary>
		/// <param name="connectionString"></param>
		//---------------------------------------------------------------------
		private void SaveServerConnectionConfigFile(string connectionString)
		{
			InstallationData.ServerConnectionInfo.SysDBConnectionString = connectionString;
			InstallationData.ServerConnectionInfo.MinPasswordLength = Convert.ToInt32(ConstString.minPasswordLength);
			InstallationData.ServerConnectionInfo.WebServicesTimeOut = Convert.ToInt32(ServerConnectionInfo.DefaultWebServicesTimeOut);
			InstallationData.ServerConnectionInfo.UnParse(this.pathFinder.ServerConnectionFile);
			diagnostic.Set(DiagnosticType.Information, Strings.ConnectionUpdated);
			DiagnosticViewer.ShowDiagnostic(diagnostic);
		}
		#endregion

		#endregion

		#region LogOff - Disconnessione del SysAdmin
		/// <summary>
		/// OnClickLogOff
		/// </summary>
		//---------------------------------------------------------------------
		private void OnClickLogOff(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			currentStatus.Clear();
			currentStatus = null;
			ErrorsFromCheckDb = false;

			if (OnAfterLogOff != null) 
				OnAfterLogOff(sender, e);

			// mando un evento alla form della MConsole per disabilitare il menu "Il mio Mago.net"
			if (OnEnableMenuAfterLogOperation != null)
				OnEnableMenuAfterLogOperation(false);

			UpdateMainMenu(consoleMenu, false);
			
			DeleteNodesFromTree(consoleTree);
			consoleTree.SelectedNode = consoleTree.Nodes[0];

			if (OnChangeStatusBarHandle != null)
			{
				DynamicEventsArgs statusBar = new DynamicEventsArgs();
				statusBar.Add(string.Empty);
				OnChangeStatusBarHandle(sender, statusBar);
			}
			if (OnPlugInUnLoadHandle != null)
			{
				string message = String.Format(Strings.PlugInUnLoaded, "System Administrator ");
				OnPlugInUnLoadHandle(sender, new DynamicEventsArgs(message));
			}
			if (OnEnableConnectToolBarButton != null) OnEnableConnectToolBarButton(sender, e);
			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
			if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
			if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
			if (OnDisableExplorerToolBarButton != null) OnDisableExplorerToolBarButton(sender, e);
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
			if (OnDisableShowAllGrantsToolBarButtonPushed != null)
				OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
		}
		#endregion

		#region Operazioni sul Menù della Microarea Console

		#region UpdateConsoleMenu - Esegue l'update delle voci di menù della console
		/// <summary>
		/// UpdateConsoleMenu
		/// </summary>
		/// <param name="mnuConsole"></param>
		//---------------------------------------------------------------------
		private void UpdateConsoleMenu(MenuStrip mnuConsole)
		{
			ToolStripMenuItem mnuPopUp = new ToolStripMenuItem(Strings.Tools);
			int lastIndex = mnuConsole.Items.Count;
			ToolStripItem lastHelpMenu = mnuConsole.Items[lastIndex - 1];

			mnuConsole.Items.Insert(lastIndex - 1, mnuPopUp);
			mnuConsole.Items.Add(lastHelpMenu);

			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Plugin.SysAdmin.Images.ConnectMenu.bmp");
			Image itemImage = (imageStream != null) ? Image.FromStream(imageStream) : null;
			ToolStripMenuItem mnuChildPopUp = new ToolStripMenuItem(Strings.LogOn, itemImage, new System.EventHandler(this.OnClickLogOn));
			mnuChildPopUp.ImageTransparentColor = Color.Magenta;
			mnuPopUp.DropDownItems.Add(mnuChildPopUp);

			mnuPopUp.DropDownItems.Add(new ToolStripSeparator());

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Plugin.SysAdmin.Images.DisconnectMenu.bmp");
			itemImage = (imageStream != null) ? Image.FromStream(imageStream) : null;
			mnuChildPopUp = new ToolStripMenuItem(Strings.LogOff, itemImage, new System.EventHandler(this.OnClickLogOff));
			mnuChildPopUp.ImageTransparentColor = Color.Magenta;
			mnuChildPopUp.Enabled = false;
			mnuPopUp.DropDownItems.Add(mnuChildPopUp);

			if (!consoleEnvironmentInfo.IsLiteConsole && !licenceInfo.IsAzureSQLDatabase)
			{
				mnuPopUp.DropDownItems.Add(new ToolStripSeparator());
				ToolStripMenuItem mnuBackupRestoreSysAdmin = new ToolStripMenuItem(Strings.BackupSystemDatabase, null, new System.EventHandler(SysAdminBackup));
				mnuPopUp.DropDownItems.Add(mnuBackupRestoreSysAdmin);
			}
		}
		#endregion

		#region UpdateMainMenu - se il logon è andato a buon fine, aggiorna il menù
		/// <summary>
		/// UpdateMainMenu
		/// </summary>
		//---------------------------------------------------------------------
		private void UpdateMainMenu(MenuStrip mnuConsole, bool successLogOn)
		{
			if (successLogOn)
			{
				//devo abilitare il logOff e disabilitare il logOn
				foreach (ToolStripMenuItem itemMenu in mnuConsole.Items)
				{
					if (itemMenu.Text != Strings.Tools) continue;
					itemMenu.DropDownItems[0].Enabled = false;
					itemMenu.DropDownItems[2].Enabled = true;
					if (!consoleEnvironmentInfo.IsLiteConsole && !licenceInfo.IsAzureSQLDatabase)
						itemMenu.DropDownItems[4].Enabled = true;
					break;
				}
			}
			else
			{
				foreach (ToolStripMenuItem itemMenu in mnuConsole.Items)
				{
					if (itemMenu.Text != Strings.Tools) continue;
					itemMenu.DropDownItems[0].Enabled = true;
					itemMenu.DropDownItems[2].Enabled = false;
					if (!consoleEnvironmentInfo.IsLiteConsole && !licenceInfo.IsAzureSQLDatabase)
						itemMenu.DropDownItems[4].Enabled = false;
					break;
				}
			}
		}
		#endregion

		#endregion

		#region Operazioni sul Tree di Microarea Console

		#region DeleteNodesFromTree - Cancella i nodi del sysAdmin dal tree di console
		/// <summary>
		/// DeleteNodesFromTree
		/// Cancella dal tree tutti i nodi del SysAdmin (per esempio, quando faccio il logoff)
		/// </summary>
		/// <param name="trvConsole"></param>
		//---------------------------------------------------------------------
		private void DeleteNodesFromTree(System.Windows.Forms.TreeView trvConsole)
		{
			string plugInName = Assembly.GetExecutingAssembly().GetName(true).Name;
			TreeNodeCollection nodeCollection = trvConsole.Nodes[0].Nodes;
			foreach (PlugInTreeNode node in nodeCollection)
			{
				PlugInTreeNode customNode = (PlugInTreeNode)node;
				if (customNode == null)
					continue;
				if (string.Compare(customNode.AssemblyName, plugInName, true, CultureInfo.InvariantCulture) == 0)
					node.Remove();
			}
		}
		#endregion

		#region UpdateConsoleTree - Aggiorna il tree di Microarea Console con gli oggetti del SysAdmin
		/// <summary>
		/// UpdateConsoleTree
		/// </summary>
		/// <param name="trvConsole"></param>
		//---------------------------------------------------------------------
		private void UpdateConsoleTree(TreeView trvConsole)
		{
			//root
			PlugInTreeNode lastNodeTree = (PlugInTreeNode)trvConsole.Nodes[trvConsole.Nodes.Count - 1];
			PlugInTreeNode rootPlugInNode = new PlugInTreeNode(Strings.NodeRoot);
			rootPlugInNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			rootPlugInNode.AssemblyType = typeof(SysAdmin);
			rootPlugInNode.ToolTipText = Strings.NodeRootToolTip;
			rootPlugInNode.ImageIndex = rootPlugInNode.SelectedImageIndex = PlugInTreeNode.GetSqlServerGroupDefaultImageIndex;
			rootPlugInNode.Type = ConstString.SysAdminPlugInRoot;

			int rootPosition = lastNodeTree.Nodes.Add(rootPlugInNode);
			//aggiungo i figli
			//Aziende
			PlugInTreeNode groupPlugInNode = new PlugInTreeNode(Strings.NodeCompanies);
			groupPlugInNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			groupPlugInNode.AssemblyType = typeof(SysAdmin);
			groupPlugInNode.Type = ConstString.containerCompanies;
			groupPlugInNode.Checked = false;
			groupPlugInNode.ImageIndex = PlugInTreeNode.GetCompaniesDefaultImageIndex;
			groupPlugInNode.SelectedImageIndex = PlugInTreeNode.GetCompaniesDefaultImageIndex;

            //posso inserire una nuova azienda se non siamo in Error
            if ((consoleEnvironmentInfo.ConsoleStatus & StatusType.RemoteServerError) != StatusType.RemoteServerError)
			{
				ContextMenuCompanies();
				groupPlugInNode.ContextMenu = this.context;
			}
			else
			{
				if (groupPlugInNode.ContextMenu != null && groupPlugInNode.ContextMenu.MenuItems != null)
					groupPlugInNode.ContextMenu.MenuItems.Clear();
			}

			int posCompanyNodes = lastNodeTree.Nodes[rootPosition].Nodes.Add(groupPlugInNode);
			LoadAllCompanies(lastNodeTree.Nodes[rootPosition].Nodes[posCompanyNodes] as PlugInTreeNode);

			//Utenti
			PlugInTreeNode usersPlugInNode = new PlugInTreeNode(Strings.NodeUsers);
			usersPlugInNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			usersPlugInNode.AssemblyType = typeof(SysAdmin);
			usersPlugInNode.Type = ConstString.containerUsers;
			usersPlugInNode.Checked = false;
			usersPlugInNode.ImageIndex = PlugInTreeNode.GetUsersGroupDefaultImageIndex;
			usersPlugInNode.SelectedImageIndex = PlugInTreeNode.GetUsersGroupDefaultImageIndex; ;
			ContextMenuUsers();
			usersPlugInNode.ContextMenu = this.context;
			int usersPosition = lastNodeTree.Nodes[rootPosition].Nodes.Add(usersPlugInNode);
			LoadAllUsers(lastNodeTree.Nodes[rootPosition].Nodes[usersPosition] as PlugInTreeNode);

			//Provider
			PlugInTreeNode providersPlugInNode = new PlugInTreeNode(Strings.NodeProviders);
			providersPlugInNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			providersPlugInNode.AssemblyType = typeof(SysAdmin);
			providersPlugInNode.Type = ConstString.containerProviders;
			providersPlugInNode.Checked = false;
			providersPlugInNode.ImageIndex = PlugInTreeNode.GetConfigSettingsDefaultImageIndex;
			providersPlugInNode.SelectedImageIndex = PlugInTreeNode.GetConfigSettingsDefaultImageIndex;
			LoadAllProviders(providersPlugInNode);
			lastNodeTree.Nodes[rootPosition].Nodes.Add(providersPlugInNode);
			//aggiungo i nodi così costituiti al tree
			lastNodeTree.Expand();
			consoleTree.Focus();

			//Esiste l'utente Guest - Se si sparo l'evento ai PlugIn
			if (ExistGuestUser())
			{
				this.consoleEnvironmentInfo.ConsoleUserGuestInfo.Exist = true;
				this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserName = NameSolverStrings.GuestLogin;
				this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserPwd = NameSolverStrings.GuestPwd;
				if (OnAfterAddGuestUser != null)
					OnAfterAddGuestUser(NameSolverStrings.GuestLogin, NameSolverStrings.GuestPwd);
			}
		}
		#endregion

		#region LoadAllProviders - Carica nel tree i nodi dei providers
		/// <summary>
		/// LoadAllProviders
		/// Carica nel tree tutti i providers
		/// </summary>
		/// <param name="providersNode"></param>
		//---------------------------------------------------------------------
		private void LoadAllProviders(PlugInTreeNode providersNode)
		{
			ArrayList providers = new ArrayList();
			ProviderDb providerDb = new ProviderDb();
			providerDb.ConnectionString = currentStatus.ConnectionString;
			providerDb.CurrentSqlConnection = currentStatus.CurrentConnection;
			bool result = providerDb.SelectAllProviders(out providers);
			if (!result)
			{
				diagnostic.Set(providerDb.Diagnostic);
				providers.Clear();
			}
			for (int i = 0; i < providers.Count; i++)
			{
				PlugInTreeNode currentNode = new PlugInTreeNode();
				ProviderItem providerItem = (ProviderItem)providers[i];
				currentNode.ImageIndex = currentNode.SelectedImageIndex = PlugInTreeNode.GetConfigSettingsDefaultImageIndex;
				currentNode.Text = providerItem.Description;
				currentNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
				currentNode.AssemblyType = typeof(SysAdmin);
				currentNode.Type = ConstString.itemProvider;
				currentNode.Id = providerItem.ProviderId;
				ContextMenuProvider();
				currentNode.ContextMenu = this.context;
				providersNode.Nodes.Add(currentNode);
			}
		}
		#endregion

		#region LoadAllUsers - Carica nel tree i nodi degli Utenti Applicativi
		/// <summary>
		/// LoadAllUsers
		/// Carica nel tree tutti gli utenti
		/// </summary>
		/// <param name="usersNode"></param>
		//---------------------------------------------------------------------
		private void LoadAllUsers(PlugInTreeNode usersNode)
		{
			if (usersNode == null)
				return;

			ArrayList users = new ArrayList();
			UserDb userDb = new UserDb();
			userDb.ConnectionString = currentStatus.ConnectionString;
			userDb.CurrentSqlConnection = currentStatus.CurrentConnection;
			bool result = userDb.SelectAllUsers(out users, true);
			if (!result)
			{
				if (userDb.Diagnostic.Error || userDb.Diagnostic.Warning || userDb.Diagnostic.Information)
					diagnostic.Set(userDb.Diagnostic);
				users.Clear();
			}
			for (int i = 0; i < users.Count; i++)
			{
				PlugInTreeNode currentNode = new PlugInTreeNode();
				UserItem userItem = (UserItem)users[i];
				currentNode.ImageIndex = PlugInTreeNode.GetLoginsDefaultImageIndex;
				currentNode.SelectedImageIndex = PlugInTreeNode.GetLoginsDefaultImageIndex;
				currentNode.Text = userItem.Login;
				currentNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
				currentNode.AssemblyType = typeof(SysAdmin);
				currentNode.Type = ConstString.itemUser;
				currentNode.Id = userItem.LoginId;
				ContextMenuUser();
				currentNode.ContextMenu = this.context;
				int position = usersNode.Nodes.Add(currentNode);
				if (userItem.Disabled)
				{
					usersNode.Nodes[position].ForeColor = Color.Red;
					usersNode.Nodes[position].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
				}
				if (userItem.Locked)
				{
					usersNode.Nodes[position].ForeColor = Color.Gray;
					usersNode.Nodes[position].StateImageIndex = PlugInTreeNode.GetRedFlagStateImageIndex;
				}
				if (!userItem.Disabled && !userItem.Locked)
				{
					if (userItem.WindowsAuthentication)
						usersNode.Nodes[position].ImageIndex = usersNode.Nodes[position].SelectedImageIndex = PlugInTreeNode.GetLoginsDefaultImageIndex;
					else
						usersNode.Nodes[position].ImageIndex = usersNode.Nodes[position].SelectedImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
				}
			}
		}
		#endregion

		#region LoadAllCompanies - Carica nel tree i nodi delle Aziende (con utenti e ruoli)
		/// <summary>
		/// LoadAllCompanies
		/// Carica nel Tree tutte le company con i relativi dati associati (ruoli e utenti)
		/// </summary>
		/// <param name="companyNode"></param>
		//---------------------------------------------------------------------
		private void LoadAllCompanies(PlugInTreeNode companyNode)
		{
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = currentStatus.ConnectionString;
			companyDb.CurrentSqlConnection = currentStatus.CurrentConnection;
			ArrayList companies = new ArrayList();

			bool result = companyDb.SelectAllCompanies(out companies);
			if (!result)
			{
				diagnostic.Set(companyDb.Diagnostic);
				companies.Clear();
			}

			// se sono nella Standard Edition considero solo le prime 2 companies caricate,
			// altrimenti considero tutte quelle presenti l'array delle companies
			bool isStandardEdition = string.Compare(licenceInfo.Edition, NameSolverStrings.StandardEdition, true, CultureInfo.InvariantCulture) == 0;
			int companiesCounter = (isStandardEdition && companies.Count > 2) ? 2 : companies.Count;

			for (int i = 0; i < companiesCounter; i++)
			{
				PlugInTreeNode currentNode = new PlugInTreeNode();
				CompanyItem companyItem = (CompanyItem)companies[i];
				currentNode.SelectedImageIndex = currentNode.ImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
				currentNode.Text = companyItem.Company;
				currentNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
				currentNode.AssemblyType = typeof(SysAdmin);
				currentNode.Type = ConstString.itemCompany;
				currentNode.Id = companyItem.CompanyId;
				currentNode.Provider = companyItem.Provider;
				currentNode.IsValid = companyItem.IsValid;
				currentNode.UseEasyAttachment = companyItem.UseDBSlave;

				((StringCollection)(settingsForConnection.Get("CompaniesIdAdmitted"))).Add(companyItem.CompanyId);

				if (companyItem.Disabled)
					currentNode.ForeColor = Color.Red;

				ContextMenuCompany(currentNode);
				if (OnAfterModifyCompanyTree != null)
					OnAfterModifyCompanyTree(currentNode, new System.EventArgs());

				currentNode.ContextMenu = this.context;
				int posCompanyNode = companyNode.Nodes.Add(currentNode);

				if (companyItem.UseSecurity && IsFunctionalityActivated(DatabaseLayerConsts.MicroareaConsole, DatabaseLayerConsts.SecurityAdmin))
				{
					PlugInTreeNode roleNode = new PlugInTreeNode();
					roleNode.Text = Strings.NodeRoles;
					//roleNode.NodeToolTip = string.Format(Strings.NodeRolesToolTip, companyItem.Company);
					roleNode.ImageIndex = PlugInTreeNode.GetRolesDefaultImageIndex;
					roleNode.SelectedImageIndex = PlugInTreeNode.GetRolesDefaultImageIndex;
					roleNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
					roleNode.AssemblyType = typeof(SysAdmin);
					roleNode.CompanyId = companyItem.CompanyId;
					roleNode.Type = ConstString.containerCompanyRoles;
					roleNode.Provider = companyItem.Provider;
					ContextMenuRoles(companyItem.CompanyId, TBDatabaseType.GetDBMSType(companyItem.Provider));
					roleNode.ContextMenu = this.customContext;
					// ora leggo per ogni company i ruoli ad essa associati
					int posCompanyRoles = currentNode.Nodes.Add(roleNode);
					companyNode.Nodes[posCompanyNode].StateImageIndex = PlugInTreeNode.GetLockStateImageIndex;
					LoadAllRolesOfCompany(((PlugInTreeNode)companyNode.Nodes[posCompanyNode].Nodes[posCompanyRoles]).CompanyId, companyNode.Nodes[posCompanyNode].Nodes[posCompanyRoles] as PlugInTreeNode);
				}

				if (companyItem.Disabled)
					companyNode.Nodes[posCompanyNode].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;

				if (!companyItem.IsValid)
				{
					currentNode.ForeColor = Color.Red;
					companyNode.Nodes[posCompanyNode].StateImageIndex = PlugInTreeNode.GetCompaniesToMigrateImageIndex;
				}

				// ora devo caricare gli utenti associati alla company
				PlugInTreeNode usersNode = new PlugInTreeNode();
				usersNode.Text = Strings.NodeCompanyUsers;
				usersNode.ImageIndex = usersNode.SelectedImageIndex = PlugInTreeNode.GetUsersDefaultImageIndex;
				usersNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
				usersNode.AssemblyType = typeof(SysAdmin);
				usersNode.CompanyId = companyItem.CompanyId;
				usersNode.Provider = companyItem.Provider;
				usersNode.Type = ConstString.containerCompanyUsers;
				usersNode.ContextMenu = ContextMenuUsersToCompany(companyItem.CompanyId, companyItem.DbServer, TBDatabaseType.GetDBMSType(companyItem.Provider));

				// ora leggo per ogni company tutti gli utenti associati
				int posUsersNode = companyNode.Nodes[posCompanyNode].Nodes.Add(usersNode);
				LoadAllUsersOfCompany(((PlugInTreeNode)companyNode.Nodes[posCompanyNode].Nodes[posUsersNode]).CompanyId, companyNode.Nodes[posCompanyNode].Nodes[posUsersNode] as PlugInTreeNode);
			}
		}
		#endregion

		#region LoadCompany - Carica nel tree l'azienda specificata nella posizione specificata

		/// <summary>
		/// LoadCompany
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="companyNode"></param>
		//---------------------------------------------------------------------
		private void LoadCompany(string companyId, PlugInTreeNode companyNode)
		{
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = currentStatus.ConnectionString;
			companyDb.CurrentSqlConnection = currentStatus.CurrentConnection;
			ArrayList company = new ArrayList();
			bool result = companyDb.GetAllCompanyFieldsById(out company, companyId);
			if (!result)
			{
				diagnostic.Set(companyDb.Diagnostic);
				company.Clear();
			}
			if (company.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)company[0];
				companyNode.SelectedImageIndex = companyNode.ImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
				companyNode.Text = companyItem.Company;
				companyNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
				companyNode.AssemblyType = typeof(SysAdmin);
				companyNode.Type = ConstString.itemCompany;
				companyNode.Id = companyItem.CompanyId;
				companyNode.Provider = companyItem.Provider;
				companyNode.IsValid = companyItem.IsValid;
				companyNode.UseEasyAttachment = companyItem.UseDBSlave;

				if (companyItem.Disabled)
					companyNode.ForeColor = Color.Red;

				ContextMenuCompany(companyNode);
				companyNode.ContextMenu = this.context;

				if (companyItem.UseSecurity && IsFunctionalityActivated(DatabaseLayerConsts.MicroareaConsole, DatabaseLayerConsts.SecurityAdmin))
				{
					PlugInTreeNode roleNode = new PlugInTreeNode();
					roleNode.Text = Strings.NodeRoles;
					roleNode.ImageIndex = PlugInTreeNode.GetRolesDefaultImageIndex;
					roleNode.SelectedImageIndex = PlugInTreeNode.GetRolesDefaultImageIndex;
					roleNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
					roleNode.AssemblyType = typeof(SysAdmin);
					roleNode.CompanyId = companyItem.CompanyId;
					roleNode.Type = ConstString.containerCompanyRoles;
					roleNode.Provider = companyItem.Provider;
					ContextMenuRoles(companyItem.CompanyId, TBDatabaseType.GetDBMSType(companyItem.Provider));
					roleNode.ContextMenu = this.customContext;
					// ora leggo per ogni company i ruoli ad essa associati
					int posCompanyRoles = companyNode.Nodes.Add(roleNode);
					this.role_OnUpdate(companyNode.Nodes[posCompanyRoles], companyNode.Id);
					if (companyItem.UseSecurity)
						companyNode.StateImageIndex = PlugInTreeNode.GetLockStateImageIndex;
				}

				if (companyItem.Disabled)
					companyNode.StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;

				if (!companyItem.IsValid)
				{
					companyNode.ForeColor = Color.Red;
					companyNode.StateImageIndex = PlugInTreeNode.GetCompaniesToMigrateImageIndex;
				}

				//ora devo caricare gli utenti associati alla company
				PlugInTreeNode usersNode = new PlugInTreeNode();
				usersNode.Text = Strings.NodeCompanyUsers;
				usersNode.ImageIndex = usersNode.SelectedImageIndex = PlugInTreeNode.GetUsersDefaultImageIndex;
				usersNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
				usersNode.AssemblyType = typeof(SysAdmin);
				usersNode.CompanyId = companyItem.CompanyId;
				usersNode.Provider = companyItem.Provider;
				usersNode.Type = ConstString.containerCompanyUsers;
				usersNode.ContextMenu = ContextMenuUsersToCompany(companyItem.CompanyId, companyItem.DbServer, TBDatabaseType.GetDBMSType(companyItem.Provider));

				//ora leggo per ogni company tutti gli utenti associati
				int posUsersNode = companyNode.Nodes.Add(usersNode);
				this.company_OnUpdateCompanyUser(companyNode.Nodes[posUsersNode], companyNode.Id);
			}
		}
		#endregion

		#region LoadAllRolesOfCompany - Carica nel tree tutti i Ruoli di una Azienda
		/// <summary>
		/// LoadAllRolesOfCompany
		/// Carico nel tree tutti i ruoli di una company con i relativi utenti della company associati a quei ruoli (se esistono)
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="roleNode"></param>
		//---------------------------------------------------------------------
		private void LoadAllRolesOfCompany(string companyId, PlugInTreeNode roleNode)
		{
			if (roleNode == null)
				return;

			RoleDb roleDb = new RoleDb();
			roleDb.ConnectionString = currentStatus.ConnectionString;
			roleDb.CurrentSqlConnection = currentStatus.CurrentConnection;
			ArrayList roles = new ArrayList();
			if (!roleDb.SelectAllRolesOfCompany(out roles, companyId))
			{
				diagnostic.Set(roleDb.Diagnostic);
				roles.Clear();
			}
			for (int i = 0; i < roles.Count; i++)
			{
                RoleItem roleItem = (RoleItem)roles[i];
                PlugInTreeNode currentNode = new PlugInTreeNode();
				currentNode.ImageIndex = PlugInTreeNode.GetRoleDefaultImageIndex;
				currentNode.SelectedImageIndex = PlugInTreeNode.GetRoleDefaultImageIndex;
				currentNode.Text = roleItem.Role;
				currentNode.Id = roleItem.RoleId;
                currentNode.ReadOnly = roleItem.ReadOnly;
				currentNode.CompanyId = roleItem.CompanyId;
				currentNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
				currentNode.AssemblyType = typeof(SysAdmin);
				currentNode.Type = ConstString.itemRole;
				ContextMenuRoleUserCompany(roleItem.CompanyId, roleItem.RoleId, roleItem.ReadOnly);
				currentNode.ContextMenu = this.customContext;
				//ora carico gli utenti della company associati al ruolo
				PlugInTreeNode usersNode = new PlugInTreeNode();
				int posUsersRole = roleNode.Nodes.Add(currentNode);
				if (roleItem.Disabled)
				{
					roleNode.Nodes[posUsersRole].ForeColor = Color.Red;
					roleNode.Nodes[posUsersRole].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
				}

				LoadAllUsersRolesOfCompany(roleItem.CompanyId, roleItem.RoleId, roleNode.Nodes[posUsersRole] as PlugInTreeNode);
			}
		}
		#endregion

		#region LoadAllUsersOfCompany - Carica nel tree tutti gli Utenti associati a una Azienda
		/// <summary>
		/// LoadAllUsersOfCompany
		/// Carico tutti gli utenti di una company
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="usersCompanyNode"></param>
		//---------------------------------------------------------------------
		private void LoadAllUsersOfCompany(string companyId, PlugInTreeNode usersCompanyNode)
		{
			if (usersCompanyNode == null)
				return;

			ArrayList usersOfCompany = new ArrayList();
			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = currentStatus.ConnectionString;
			companyUserDb.CurrentSqlConnection = currentStatus.CurrentConnection;
			
			if (!companyUserDb.SelectAll(out usersOfCompany, companyId))
			{
				diagnostic.Set(companyUserDb.Diagnostic);
				usersOfCompany.Clear();
			}

			for (int i = 0; i < usersOfCompany.Count; i++)
			{
				PlugInTreeNode currentNode = new PlugInTreeNode();
				CompanyUser userCompanyItem = (CompanyUser)usersOfCompany[i];
				currentNode.ImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
				currentNode.SelectedImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
				currentNode.Text = userCompanyItem.Login;
				currentNode.Id = userCompanyItem.LoginId;
				currentNode.CompanyId = companyId;
				currentNode.Provider = usersCompanyNode.Provider;
				currentNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
				currentNode.AssemblyType = typeof(SysAdmin);
				currentNode.Type = ConstString.itemCompanyUser;
				ContextMenuUserCompany(companyId, userCompanyItem.LoginId, TBDatabaseType.GetDBMSType(usersCompanyNode.Provider));
				currentNode.ContextMenu = this.customContext;
				int posCompanyUser = usersCompanyNode.Nodes.Add(currentNode);
				if (userCompanyItem.Disabled)
				{
					usersCompanyNode.Nodes[posCompanyUser].ForeColor = Color.Red;
					usersCompanyNode.Nodes[posCompanyUser].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
				}
				if (companyUserDb.IsDbo(userCompanyItem.LoginId, userCompanyItem.CompanyId))
					usersCompanyNode.Nodes[posCompanyUser].StateImageIndex = PlugInTreeNode.GetKeyStateImageIndex;
				//In funzione della tipologia dell'utente, setto l'icona
				if (userCompanyItem.WindowsAuthentication)
					usersCompanyNode.Nodes[posCompanyUser].ImageIndex = usersCompanyNode.Nodes[posCompanyUser].SelectedImageIndex = PlugInTreeNode.GetLoginsDefaultImageIndex;
				else
					usersCompanyNode.Nodes[posCompanyUser].ImageIndex = usersCompanyNode.Nodes[posCompanyUser].SelectedImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
			}
		}
		#endregion

		#region LoadAllUsersRolesOfCompany - Carica nel tree tutti gli Utenti associati ad un Ruolo di una Azienda
		/// <summary>
		/// LoadAllUsersRolesOfCompany
		/// Carico nel Tree tutti gli utenti di una company associati alla role specificata
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="roleId"></param>
		/// <param name="usersRolesCompanyNode"></param>
		//---------------------------------------------------------------------
		private void LoadAllUsersRolesOfCompany(string companyId, string roleId, PlugInTreeNode usersRolesCompanyNode)
		{
			if (usersRolesCompanyNode == null)
				return;

			CompanyRoleLoginDb companyRoleLoginDb = new CompanyRoleLoginDb();
			ArrayList usersRolesOfCompany = new ArrayList();
			companyRoleLoginDb.ConnectionString = currentStatus.ConnectionString;
			companyRoleLoginDb.CurrentSqlConnection = currentStatus.CurrentConnection;
			if (!companyRoleLoginDb.SelectAll(out usersRolesOfCompany, companyId, roleId))
			{
				diagnostic.Set(companyRoleLoginDb.Diagnostic);
				usersRolesOfCompany.Clear();
			}

			for (int i = 0; i < usersRolesOfCompany.Count; i++)
			{
				PlugInTreeNode currentNode = new PlugInTreeNode();
				CompanyUser userCompanyItem = (CompanyUser)usersRolesOfCompany[i];
				currentNode.Text = userCompanyItem.Login;
				currentNode.ImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
				currentNode.SelectedImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
				currentNode.Id = userCompanyItem.LoginId;
				currentNode.RoleId = roleId;
				currentNode.CompanyId = companyId;
				currentNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
				currentNode.AssemblyType = typeof(SysAdmin);
				currentNode.Type = ConstString.itemRoleCompanyUser;
				ContextMenuUsersRolesOfCompany(companyId, roleId, userCompanyItem.LoginId);
				currentNode.ContextMenu = this.customContext;
				int posUserCompanyRole = usersRolesCompanyNode.Nodes.Add(currentNode);
				if (userCompanyItem.Disabled)
				{
					usersRolesCompanyNode.Nodes[posUserCompanyRole].ForeColor = Color.Red;
					usersRolesCompanyNode.Nodes[posUserCompanyRole].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
				}
				if (userCompanyItem.WindowsAuthentication)
					usersRolesCompanyNode.Nodes[posUserCompanyRole].ImageIndex = usersRolesCompanyNode.Nodes[posUserCompanyRole].SelectedImageIndex = PlugInTreeNode.GetLoginsDefaultImageIndex;
				else
					usersRolesCompanyNode.Nodes[posUserCompanyRole].ImageIndex = usersRolesCompanyNode.Nodes[posUserCompanyRole].SelectedImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
			}
		}
		#endregion

		#region FindNodeTipology - ricerca nel tree tutti i nodi di una certa tipologia
		/// <summary>
		/// FindNodeTipology
		/// Cerco nel tree i nodi corripondenti alla tipologia - se id="ALL" cerca su tutti, se id è specificato
		/// cerca il ndodo la cui CompanyId = companyid
		/// </summary>
		//---------------------------------------------------------------------
		private PlugInTreeNode FindNodeTipology
			(
			TreeNodeCollection nodes,
			string plugInName,
			string tipology,
			string companyId
			)
		{
			PlugInTreeNode nodeOfTypology = null;
			for (int i = 0; nodeOfTypology == null; i++)
			{
				try
				{
					if (i >= nodes.Count)
						return nodeOfTypology;

					if ((((PlugInTreeNode)nodes[i]).Type == tipology) &&
						(((PlugInTreeNode)nodes[i]).AssemblyName == plugInName))
					{
						nodeOfTypology = (PlugInTreeNode)nodes[i];
						if (companyId != "ALL")
						{
							if (nodeOfTypology.CompanyId == companyId)
								return nodeOfTypology;
							else
							{
								if (nodes[i].Parent.NextNode != null && nodes[i].Parent.NextNode.Nodes.Count > 0)
								{
									PlugInTreeNode nextNode = (PlugInTreeNode)nodes[i].Parent.NextNode;
									nodeOfTypology = FindNodeTipology(((TreeNode)nextNode).Nodes, plugInName, tipology, companyId);
								}
								else
									//sono arrivata in fondo, non ho trovato nulla
									return null;
							}
						}
						else
							return nodeOfTypology;
					}
					else
					{
						if (nodes[i].Nodes.Count > 0)
							nodeOfTypology = FindNodeTipology(nodes[i].Nodes, plugInName, tipology, companyId);
					}
				}
				catch (System.NullReferenceException exc)
				{
					Debug.Fail(exc.Message);

				}
			}
			return nodeOfTypology;
		}
		#endregion

		#region consoleTree_OnModifyProvider - Aggiornamento dei nodi Providers dopo una modifica
		/// <summary>
		/// consoleTree_OnModifyProvider
		/// Il tree fa un reload dei nodi di una certo tipo (specificato da nodeType) che sono stati modificati
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="nodeType"></param>
		//---------------------------------------------------------------------
		private void consoleTree_OnModifyProvider(object sender, string nodeType)
		{
			PlugInTreeNode node = FindNodeTipology(consoleTree.Nodes, Assembly.GetExecutingAssembly().GetName().Name, nodeType, "ALL");
			PlugInTreeNode selectedNode = (PlugInTreeNode)sender;

			if (node != null)
			{
				bool isExpanded = node.IsExpanded;
				int position = node.Nodes.IndexOf(selectedNode);
				if (position != -1)
				{
					node.Nodes.RemoveAt(position);
					node.Nodes.Insert(position, (PlugInTreeNode)sender);
					consoleTree.SelectedNode = (PlugInTreeNode)sender;
				}
				else
				{
					node.Nodes.Clear();
					LoadAllProviders(node);
				}

				if (isExpanded)
					node.Expand();

				consoleTree.Focus();
				if (consoleTree.SelectedNode != null)
					OnAfterSelectConsoleTree(sender, new TreeViewEventArgs(consoleTree.SelectedNode));
			}
		}
		#endregion

		#region consoleTree_OnModifyUtente - Aggiornamento dei Nodi Utente dopo una modifica
		/// <summary>
		/// consoleTree_OnModifyUtente
		/// Il tree fa un reload dei nodi di una certo tipo (specificato da nodeType) che sono stati modificati
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="nodeType"></param>
		//---------------------------------------------------------------------
		private void consoleTree_OnModifyUtente(object sender, string nodeType)
		{
			PlugInTreeNode node =
				FindNodeTipology(consoleTree.Nodes, Assembly.GetExecutingAssembly().GetName().Name, nodeType, "ALL");

			if (node != null)
			{
				bool isExpanded = node.IsExpanded;
				node.Nodes.Clear();

				if (string.Compare(nodeType, ConstString.containerUsers, true, CultureInfo.InvariantCulture) == 0)
					LoadAllUsers(node);
				else if (string.Compare(nodeType, ConstString.containerCompanies, true, CultureInfo.InvariantCulture) == 0)
				{
					LoadAllCompanies(node);
					node = FindNodeTipology(consoleTree.Nodes, Assembly.GetExecutingAssembly().GetName().Name, ConstString.containerUsers, "ALL");
					node.Nodes.Clear();
					LoadAllUsers(node);
				}

				if (isExpanded)
					node.Expand();
				consoleTree.Focus();
				if (consoleTree.SelectedNode != null)
					OnAfterSelectConsoleTree(sender, new TreeViewEventArgs(consoleTree.SelectedNode));
			}
		}
		#endregion

		#region consoleTree_onModifyAzienda - Aggiornamento dei nodi Azienda dopo una modifica
		/// <summary>
		/// consoleTree_onModifyAzienda
		/// Il tree fa un reload dei nodi di una certo tipo (specificato da nodeType) che sono stati modificati
		/// </summary>
		//---------------------------------------------------------------------
		private void consoleTree_onModifyAzienda(object sender, string nodeType)
		{
			PlugInTreeNode node = FindNodeTipology(consoleTree.Nodes, Assembly.GetExecutingAssembly().GetName().Name, nodeType, "ALL");
			if (node != null)
			{
				bool isExpanded = node.IsExpanded;
				node.Nodes.Clear();
				LoadAllCompanies(node);
				if (isExpanded)
					node.Expand();
			}
			consoleTree.Focus();

			OnAfterSelectConsoleTree
				(sender,
				(consoleTree.SelectedNode != null) ? new TreeViewEventArgs(consoleTree.SelectedNode) : new TreeViewEventArgs(node));
		}

		//---------------------------------------------------------------------
		private void EditConfigFile_OnModifyCulture(object sender, string cultureUI, string culture)
		{
			if (OnModifyCulture != null)
				OnModifyCulture(sender, cultureUI, culture);
		}
		#endregion

		#region consoleTree_OnDeleteCompany - Aggiornamento dei nodi Azienda dopo una cancellazione
		/// <summary>
		/// consoleTree_OnDeleteCompany
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="nodeType"></param>
		//---------------------------------------------------------------------
		private void consoleTree_OnDeleteCompany(object sender, string nodeType)
		{
			PlugInTreeNode node =
				FindNodeTipology(consoleTree.Nodes, Assembly.GetExecutingAssembly().GetName().Name, nodeType, "ALL");

			if (node != null)
			{
				bool isExpanded = node.IsExpanded;
				node.Nodes.Clear();
				LoadAllCompanies(node);
				if (isExpanded)
					node.Expand();
				consoleTree.Focus();
				if (consoleTree.SelectedNode != null)
					OnAfterSelectConsoleTree(sender, new TreeViewEventArgs(consoleTree.SelectedNode));
				else
					OnAfterSelectConsoleTree(sender, new TreeViewEventArgs(node));
			}
		}
		#endregion

		#region consoleTree_onModifyRole - Aggiornamento dei nodi Ruolo dopo una modifica
		/// <summary>
		/// consoleTree_onModifyRole
		/// Il tree fa un reload dei nodi della company (specificati dal companyId) che sono stati modificati
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="nodeType"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------------
		private void consoleTree_onModifyRole(object sender, string nodeType, string companyId)
		{
			PlugInTreeNode node =
				FindNodeTipology(consoleTree.Nodes, Assembly.GetExecutingAssembly().GetName().Name, nodeType, companyId);

			if (node != null)
			{
				bool isExpanded = node.IsExpanded;
				node.Nodes.Clear();
				LoadAllRolesOfCompany(node.CompanyId, node);
				if (isExpanded)
					node.Expand();
				consoleTree.Focus();
				if (consoleTree.SelectedNode != null)
					OnAfterSelectConsoleTree(sender, new TreeViewEventArgs(consoleTree.SelectedNode));
			}
		}
		#endregion

		#region consoleTree_OnModifyLoginCompany
		/// <summary>
		/// consoleTree_OnModifyLoginCompany
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="nodeType"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void consoleTree_OnModifyLoginCompany(object sender, string nodeType, string companyId)
		{
			PlugInTreeNode node =
				FindNodeTipology(consoleTree.Nodes, Assembly.GetExecutingAssembly().GetName().Name, nodeType, companyId);

			if (node != null)
			{
				bool isExpanded = node.IsExpanded;
				node.Nodes.Clear();
				if (string.Compare(nodeType, ConstString.containerCompanyUsers, true, CultureInfo.InvariantCulture) == 0)
					this.LoadAllUsersOfCompany(node.CompanyId, node);
				if (string.Compare(nodeType, ConstString.containerCompanyRoles, true, CultureInfo.InvariantCulture) == 0)
					LoadAllRolesOfCompany(node.CompanyId, node);
				if (isExpanded)
					node.Expand();
				consoleTree.Focus();
				if (consoleTree.SelectedNode != null)
					OnAfterSelectConsoleTree(sender, new TreeViewEventArgs(consoleTree.SelectedNode));
			}
		}
		#endregion

		#region consoleTree_OnModifyUserCompany - Aggiornamento dei nodi Utente di una azienda dopo una modifica
		/// <summary>
		/// consoleTree_OnModifyUserCompany
		/// Il tree fa un reload dei nodi della company (specificati dal companyId) che sono stati modificati
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="nodeType"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void consoleTree_OnModifyUserCompany(object sender, string nodeType, string companyId)
		{
			PlugInTreeNode node =
				FindNodeTipology(consoleTree.Nodes, Assembly.GetExecutingAssembly().GetName().Name, nodeType, companyId);

			if (node != null)
			{
				bool isExpanded = node.IsExpanded;
				node.Nodes.Clear();
				if (string.Compare(nodeType, ConstString.containerCompanyUsers, true, CultureInfo.InvariantCulture) == 0)
					LoadAllUsersOfCompany(node.CompanyId, node);
				if (string.Compare(nodeType, ConstString.containerCompanyRoles, true, CultureInfo.InvariantCulture) == 0)
					LoadAllRolesOfCompany(node.CompanyId, node);
				if (isExpanded)
					node.Expand();
				consoleTree.Focus();
				if (consoleTree.SelectedNode != null)
					OnAfterSelectConsoleTree(sender, new TreeViewEventArgs(consoleTree.SelectedNode));
			}
		}
		#endregion
		#endregion

		#region ViewInfoSysAdminConnection - Visualizza le info sulla connessione
		/// <summary>
		/// ViewInfoConnection
		/// </summary>
		//---------------------------------------------------------------------
		private void ViewInfoSysAdminConnection(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			ViewInfoConnection viewInfoConn = new ViewInfoConnection(currentStatus, brandLoader);
			viewInfoConn.OnSendDiagnostic += new ViewInfoConnection.SendDiagnostic(ReceiveDiagnostic);
			viewInfoConn.TopLevel = false;
			viewInfoConn.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, viewInfoConn.ClientSize.Width, viewInfoConn.ClientSize.Height);
			workingAreaConsole.Controls.Add(viewInfoConn);
			viewInfoConn.Enabled = true;
			viewInfoConn.Show();
		}
		#endregion

		#region EditConfigFile - Visualizza/Modifica il ServerConnection.config
		/// <summary>
		/// EditConfigFile
		/// </summary>
		//---------------------------------------------------------------------
		private void EditConfigFile(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();

			EditConfigFile manageConfigFile = new EditConfigFile(pathFinder, this.licenceInfo.DBNetworkType);
			manageConfigFile.OnModifyCulture += new EditConfigFile.ModifyCulture(EditConfigFile_OnModifyCulture);
			manageConfigFile.OnModifyTree += new EditConfigFile.ModifyTree(consoleTree_onModifyAzienda);
			manageConfigFile.OnSendDiagnostic += new EditConfigFile.SendDiagnostic(ReceiveDiagnostic);
			manageConfigFile.OnAfterCreateServerConnection += new EditConfigFile.AfterCreateServerConnection(OnAfterCreateServerConnection);
			manageConfigFile.OnEnableSaveButton += new EventHandler(OnEnableSaveToolBarButton);
			manageConfigFile.TopLevel = false;
			manageConfigFile.Dock = DockStyle.Fill;

			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, manageConfigFile.ClientSize.Width, manageConfigFile.ClientSize.Height);
			workingAreaConsole.Controls.Add(manageConfigFile);
			manageConfigFile.Enabled = true;
			manageConfigFile.Show();

			if (OnEnableSaveToolBarButton != null)
				OnEnableSaveToolBarButton(sender, new System.EventArgs());
		}
		#endregion

		#region SysAdminBackup - Backup del db di sistema
		/// <summary>
		/// SysAdminBackup
		/// Richiama la form per effettuare il baclup del database di sistema
		/// </summary>
		//---------------------------------------------------------------------
		private void SysAdminBackup(object sender, System.EventArgs e)
		{
			BackupDatabase backupDb =
				new BackupDatabase(false, string.Empty, currentStatus, consoleEnvironmentInfo.RunningFromServer);

			backupDb.OnEnableProgressBar += new BackupDatabase.EnableProgressBar(EnableProgressBarFromPlugIn);
			backupDb.OnDisableProgressBar += new BackupDatabase.DisableProgressBar(DisableProgressBarFromPlugIn);
			backupDb.OnSetProgressBarStep += new BackupDatabase.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			backupDb.OnSetProgressBarText += new BackupDatabase.SetProgressBarText(SetProgressBarTextFromPlugIn);
			backupDb.OnSetProgressBarValue += new BackupDatabase.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			backupDb.OnIsActivated += new BackupDatabase.IsActivated(IsFunctionalityActivated);

			workingAreaConsole.Controls.Clear();
			if (backupDb.BackupDBDiagnostic.Error)
			{
				DiagnosticViewer.ShowDiagnostic(backupDb.BackupDBDiagnostic);
				return;
			}

			backupDb.TopLevel = false;
			backupDb.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, backupDb.Width, backupDb.Height);
			workingAreaConsole.Controls.Add(backupDb);
			backupDb.Show();
		}
		#endregion

		#region ContextMenu

		#region ContextMenuCompanies - ContexMenu sul nodo contenitore di  Aziende
		/// <summary>
		/// ContextMenuCompanies
		/// Context menu che appare sul nodo tree contenitore delle company
		/// </summary>
		//---------------------------------------------------------------------
		private void ContextMenuCompanies()
		{
			context = new ContextMenu();
			context.MenuItems.Clear();
			context.MenuItems.Add(Strings.NewCompany, new System.EventHandler(OnNewCompany));
		}

		# region CompaniesNumber - ritorna il numero delle aziende censite nel corrente database di sistema
		//---------------------------------------------------------------------
		private int CompaniesNumber()
		{
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.currentStatus.ConnectionString;
			companyDb.CurrentSqlConnection = this.currentStatus.CurrentConnection;
			return companyDb.CompaniesNumber();
		}
		# endregion

		#region ContextMenuCompany - ContextMenu su un nodo Azienda
		/// <summary>
		/// ContextMenuCompany
		/// </summary>
		//---------------------------------------------------------------------
		private void ContextMenuCompany(PlugInTreeNode companyNode)
		{
			DBMSType providerType = TBDatabaseType.GetDBMSType(companyNode.Provider);

			context = new ContextMenu();
			context.MenuItems.Clear();
			context.MenuItems.Add(Strings.Properties, new System.EventHandler(OnViewCompany));
			context.MenuItems.Add("-");

			//Menu Cancellazioni
            if (providerType == DBMSType.SQLSERVER)
			{
				MenuItem menuDeleteCompany = new MenuItem(Strings.DeleteTitleMenu);
				menuDeleteCompany.MenuItems.Add(Strings.DeleteCompany, new System.EventHandler(OnDeleteCompanyHandler));
				menuDeleteCompany.MenuItems.Add(Strings.DeleteDataBaseObject, new System.EventHandler(OnDeleteCompanyObjectHandler));
				context.MenuItems.Add(menuDeleteCompany);
			}
			else
				context.MenuItems.Add(Strings.DeleteCompany, new System.EventHandler(OnDeleteCompanyHandler));

			// menu visualizzati solo se il provider è SQL Server			
			if (providerType == DBMSType.SQLSERVER)
			{
				//Menu Verifica dati
				context.MenuItems.Add("-");
				context.MenuItems.Add(Strings.CheckIntegrity, new System.EventHandler(OnCheckIntegrityCompany));

				if (companyNode.IsValid && !consoleEnvironmentInfo.IsLiteConsole && !licenceInfo.IsAzureSQLDatabase)
				{
					//posso clonare una nuova azienda se non siamo in errore
					if ((consoleEnvironmentInfo.ConsoleStatus & StatusType.RemoteServerError) != StatusType.RemoteServerError)
					{
						context.MenuItems.Add("-");
						context.MenuItems.Add(Strings.CloneCompany, new System.EventHandler(OnNewCloneCompany));
					}
				}

				if (!consoleEnvironmentInfo.IsLiteConsole && !licenceInfo.IsAzureSQLDatabase)
				{
					//Menu Backup/Restore
					MenuItem menuBackupRestoreCompany = new MenuItem(Strings.BackupRestoreTitleMenu);
					menuBackupRestoreCompany.MenuItems.Add(Strings.BackupDatabase, new System.EventHandler(OnBackupDbCompany));
					menuBackupRestoreCompany.MenuItems.Add(Strings.RestoreDataBase, new System.EventHandler(OnRestoreDbCompany));
					context.MenuItems.Add(menuBackupRestoreCompany);
				}
			}
			
			// il menu Backup/Restore e' visualizzato per Oracle solo se ho dei db slave e i relativi moduli sono attivati
			if (providerType == DBMSType.ORACLE)
			{
				if (IsFunctionalityActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment) && companyNode.UseEasyAttachment && !licenceInfo.IsAzureSQLDatabase)
				{
					MenuItem menuBackupRestoreCompany = new MenuItem(Strings.BackupRestoreTitleMenu);
					menuBackupRestoreCompany.MenuItems.Add(Strings.BackupDatabase, new System.EventHandler(OnBackupDbCompany));
					menuBackupRestoreCompany.MenuItems.Add(Strings.RestoreDataBase, new System.EventHandler(OnRestoreDbCompany));
					context.MenuItems.Add(menuBackupRestoreCompany);
				}
			}
		}
		#endregion

		#region Handles per le voci del ContextMenu

		#region company_OnNew - Crea una nuova Azienda
		/// <summary>
		/// company_OnNew
		/// </summary>
		//---------------------------------------------------------------------
		private void company_OnNew(object sender, System.EventArgs e)
		{
			if (consoleEnvironmentInfo.IsLiteConsole)
			{
				//Gestione della consoleLite
				OnNewCompanyLite(sender, e);
				return;
			}

			workingAreaConsole.Controls.Clear();
			SetProgressBarMaxValueFromPlugIn(sender, 100);

			Company newCompany = new Company
				(
				currentStatus.OwnerDbName,
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				pathFinder,
				licenceInfo
				);

			newCompany.UserConnected = currentStatus.User;
			newCompany.UserPwdConnected = currentStatus.Password;
			newCompany.DataSourceSysAdmin = currentStatus.DataSource;
			newCompany.ServerNameSystemDb = currentStatus.ServerName;
			newCompany.ServerIstanceSystemDb = currentStatus.ServerIstanceName;

			//quando creo una nuova azienda devo settare il cursor del tree in modo che
			//Se l'utente da ok e poi mi clicca sul tree il processo non venga interrotto
			newCompany.TreeConsole = this.consoleTree;
			newCompany.OnCallHelp += new Company.CallHelp(HelpFromPopUp);
			newCompany.OnModifyTree += new Company.ModifyTree(consoleTree_onModifyAzienda);
			newCompany.OnModifyTree += new Company.ModifyTree(consoleTree_OnModifyUtente);
			newCompany.OnModifyTreeOfCompanies += new Company.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
			newCompany.OnAfterChangedAuditing += new Company.AfterChangedAuditing(AfterChangedAuditingCompany);
			newCompany.OnAfterModifyCompany += new Company.AfterModifyCompany(AfterModifyCompany);
			newCompany.OnAfterDeleteCompany += new Company.AfterDeleteCompany(AfterDeleteCompany);
			newCompany.OnAfterChangedCompanyDisable += new Company.AfterChangedCompanyDisable(AfterChangedCompanyDisabled);
			newCompany.OnEnableProgressBar += new Company.EnableProgressBar(EnableProgressBarFromPlugIn);
			newCompany.OnDisableProgressBar += new Company.DisableProgressBar(DisableProgressBarFromPlugIn);
			newCompany.OnSetProgressBarStep += new Company.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			newCompany.OnSetProgressBarText += new Company.SetProgressBarText(SetProgressBarTextFromPlugIn);
			newCompany.OnSetProgressBarValue += new Company.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			newCompany.OnGetUserAuthenticatedPwdFromConsole += new Company.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			newCompany.OnIsUserAuthenticatedFromConsole += new Company.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			newCompany.OnAddUserAuthenticatedFromConsole += new Company.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			newCompany.OnDisableSaveButton += new EventHandler(OnDisableSaveToolBarButton);
			newCompany.OnEnableSaveButton += new EventHandler(OnEnableSaveToolBarButton);
			newCompany.OnCreateDBStructure += new Company.CreateDBStructure(OnCreateCompanyDBStructure);
			newCompany.OnCheckDBRequirementsUsed += new Company.CheckDBRequirementsUsed(OnCheckRequirements);
			newCompany.OnAfterSaveNewCompany += new Company.AfterSaveNewCompany(newCompany_OnAfterSaveNewCompany);
			newCompany.OnIsActivated += new Company.IsActivated(IsFunctionalityActivated);

			newCompany.TopLevel = false;
			newCompany.Dock = DockStyle.Fill;
			//prima devo settare le proprietà poi posso caricare i dati di default (utenti, provider, server, ecc...)
			newCompany.LoadDefaultData();
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, newCompany.ClientSize.Width, newCompany.ClientSize.Height);
			this.workingAreaConsole.Controls.Add(newCompany);
			newCompany.Visible = true;
			newCompany.Enabled = true;
			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null)
				OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
		}

		//---------------------------------------------------------------------
		private void OnNewCompanyLite(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			SetProgressBarMaxValueFromPlugIn(sender, 100);

			CompanyLite newCompanyLite = new CompanyLite
				(
				currentStatus.OwnerDbName,
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				pathFinder,
				licenceInfo
				);

			newCompanyLite.UserConnected = currentStatus.User;
			newCompanyLite.UserPwdConnected = currentStatus.Password;
			newCompanyLite.DataSourceSysAdmin = currentStatus.DataSource;
			newCompanyLite.ServerNameSystemDb = currentStatus.ServerName;
			newCompanyLite.ServerIstanceSystemDb = currentStatus.ServerIstanceName;

			//quando creo una nuova azienda devo settare il cursor del tree in modo che
			//Se l'utente da ok e poi mi clicca sul tree il processo non venga interrotto
			newCompanyLite.TreeConsole = this.consoleTree;
			newCompanyLite.OnCallHelp += new CompanyLite.CallHelp(HelpFromPopUp);
			newCompanyLite.OnModifyTree += new CompanyLite.ModifyTree(consoleTree_onModifyAzienda);
			newCompanyLite.OnModifyTree += new CompanyLite.ModifyTree(consoleTree_OnModifyUtente);
			newCompanyLite.OnModifyTreeOfCompanies += new CompanyLite.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
			newCompanyLite.OnAfterChangedAuditing += new CompanyLite.AfterChangedAuditing(AfterChangedAuditingCompany);
			newCompanyLite.OnAfterModifyCompany += new CompanyLite.AfterModifyCompany(AfterModifyCompany);
			newCompanyLite.OnAfterDeleteCompany += new CompanyLite.AfterDeleteCompany(AfterDeleteCompany);
			newCompanyLite.OnAfterChangedCompanyDisable += new CompanyLite.AfterChangedCompanyDisable(AfterChangedCompanyDisabled);
			newCompanyLite.OnEnableProgressBar += new CompanyLite.EnableProgressBar(EnableProgressBarFromPlugIn);
			newCompanyLite.OnDisableProgressBar += new CompanyLite.DisableProgressBar(DisableProgressBarFromPlugIn);
			newCompanyLite.OnSetProgressBarStep += new CompanyLite.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			newCompanyLite.OnSetProgressBarText += new CompanyLite.SetProgressBarText(SetProgressBarTextFromPlugIn);
			newCompanyLite.OnSetProgressBarValue += new CompanyLite.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			newCompanyLite.OnGetUserAuthenticatedPwdFromConsole += new CompanyLite.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			newCompanyLite.OnIsUserAuthenticatedFromConsole += new CompanyLite.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			newCompanyLite.OnAddUserAuthenticatedFromConsole += new CompanyLite.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			newCompanyLite.OnEnableSaveButton += new EventHandler(OnEnableSaveToolBarButton);
			newCompanyLite.OnCreateDBStructure += new CompanyLite.CreateDBStructure(OnCreateCompanyDBStructure);
			newCompanyLite.OnCheckDBRequirementsUsed += new CompanyLite.CheckDBRequirementsUsed(OnCheckRequirements);
			newCompanyLite.OnAfterSaveNewCompany += new CompanyLite.AfterSaveNewCompany(newCompany_OnAfterSaveNewCompany);
			newCompanyLite.OnIsActivated += new CompanyLite.IsActivated(IsFunctionalityActivated);

			newCompanyLite.TopLevel = false;
			newCompanyLite.Dock = DockStyle.Fill;
			//prima devo settare le proprietà poi posso caricare i dati di default (utenti, provider, server, ecc...)
			newCompanyLite.LoadDefaultData();
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, newCompanyLite.ClientSize.Width, newCompanyLite.ClientSize.Height);
			this.workingAreaConsole.Controls.Add(newCompanyLite);
			newCompanyLite.Visible = true;
			newCompanyLite.Enabled = true;
			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
		}
		#endregion

		#region company_OnView - Visualizza i dati di una Azienda
		/// <summary>
		/// company_OnView
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnViewCompany(object sender, System.EventArgs e)
		{
			try
			{
				PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
				workingAreaConsole.Controls.Clear();
				OnModifyCompany(sender, selectedNode.Id);
			}
			catch (System.Exception err)
			{
				Debug.Fail(err.Message);
			}
		}
		#endregion

		#region OnCloneCompanyHandler - Clona una azienda
		/// <summary>
		/// OnCloneCompanyHandler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnCloneCompanyHandler(object sender, System.EventArgs e)
		{
			SetProgressBarMaxValueFromPlugIn(sender, 100);
			workingAreaConsole.Controls.Clear();
			CloneCompany cloneCompany = new CloneCompany
				(
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				licenceInfo,
				((PlugInTreeNode)consoleTree.SelectedNode).Id
				);

			cloneCompany.OnCallHelpFromPopUp += new CloneCompany.CallHelpFromPopUp(HelpFromPopUp);
			cloneCompany.OnSendDiagnostic += new CloneCompany.SendDiagnostic(ReceiveDiagnostic);
			cloneCompany.OnModifyTree += new CloneCompany.ModifyTree(consoleTree_onModifyAzienda);
			cloneCompany.OnAfterClonedCompany += new CloneCompany.AfterClonedCompany(cloneCompany_OnAfterClonedCompany);
			cloneCompany.OnEnableProgressBar += new CloneCompany.EnableProgressBar(EnableProgressBarFromPlugIn);
			cloneCompany.OnDisableProgressBar += new CloneCompany.DisableProgressBar(DisableProgressBarFromPlugIn);
			//cloneCompany.OnSetProgressBarStep += new CloneCompany.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			cloneCompany.OnSetProgressBarText += new CloneCompany.SetProgressBarText(SetProgressBarTextFromPlugIn);
			//cloneCompany.OnSetProgressBarValue += new CloneCompany.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			cloneCompany.OnSetCyclicStepProgressBar += new CloneCompany.SetCyclicStepProgressBar(SetCyclicStepProgressBarFromPlugIn);
			cloneCompany.OnAddUserAuthenticatedFromConsole += new CloneCompany.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			cloneCompany.OnGetUserAuthenticatedPwdFromConsole += new CloneCompany.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			cloneCompany.OnIsUserAuthenticatedFromConsole += new CloneCompany.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);

			if (!cloneCompany.Diagnostic.Error)
			{
				cloneCompany.TopLevel = false;
				cloneCompany.Dock = DockStyle.Fill;
				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, cloneCompany.ClientSize.Width, cloneCompany.ClientSize.Height);
				workingAreaConsole.Controls.Add(cloneCompany);
				cloneCompany.Show();
			}
		}
		#endregion

		#region OnDeleteCompanyHandler - Cancella un'Azienda
		/// <summary>
		/// Dal nodo Azienda scelgo di eliminare la sua anagrafica ed eventualmente
		/// il database (piu' quello di EasyAttachment)
		/// </summary>
		//---------------------------------------------------------------------
		private void OnDeleteCompanyHandler(object sender, System.EventArgs e)
		{
			// verifico che all'azienda non sia collegato qualche utente lato MagoNet
			if (OnGetCompanyDBIsInFreeState != null)
			{
				if (!OnGetCompanyDBIsInFreeState(((PlugInTreeNode)consoleTree.SelectedNode).Id))
				{
					DiagnosticViewer.ShowCustomizeIconMessage(Strings.ErrCompanyDBIsNotFree, string.Empty, MessageBoxIcon.Exclamation);
					return;
				}
			}

			DBMSType providerType = TBDatabaseType.GetDBMSType(((PlugInTreeNode)consoleTree.SelectedNode).Provider);
			OnDeleteCompany((PlugInTreeNode)consoleTree.SelectedNode, ((PlugInTreeNode)consoleTree.SelectedNode).Id, providerType);
		}
		#endregion

		#region OnCheckIntegrityCompany - Verifica integrità dati di un'Azienda (solo per SQLServer)
		/// <summary>
		/// OnCheckIntegrityCompany
		/// </summary>
		//---------------------------------------------------------------------
		private void OnCheckIntegrityCompany(object sender, System.EventArgs e)
		{
			SetProgressBarMaxValueFromPlugIn(sender, 100);
			workingAreaConsole.Controls.Clear();

			CheckCompany checkCompany = new CheckCompany
				(
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				((PlugInTreeNode)consoleTree.SelectedNode).Id,
				((PlugInTreeNode)consoleTree.SelectedNode).Text,
				consoleTree.StateImageList,
				this.licenceInfo.IsoState
				);

			checkCompany.OnSendDiagnostic += new CheckCompany.SendDiagnostic(ReceiveDiagnostic);
			checkCompany.OnDisableProgressBar += new CheckCompany.DisableProgressBar(DisableProgressBarFromPlugIn);
			checkCompany.OnEnableProgressBar += new CheckCompany.EnableProgressBar(EnableProgressBarFromPlugIn);
			checkCompany.OnSetProgressBarStep += new CheckCompany.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			checkCompany.OnSetProgressBarText += new CheckCompany.SetProgressBarText(SetProgressBarTextFromPlugIn);
			checkCompany.OnSetProgressBarValue += new CheckCompany.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			checkCompany.OnAddUserAuthenticatedFromConsole += new CheckCompany.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			checkCompany.OnGetUserAuthenticatedPwdFromConsole += new CheckCompany.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			checkCompany.OnIsUserAuthenticatedFromConsole += new CheckCompany.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			checkCompany.OnCheckDatabaseStructure += new CheckCompany.CheckDatabaseStructure(checkCompany_OnCheckDatabaseStructure);
			checkCompany.OnEnableConsoleTreeView += new CheckCompany.EnableConsoleTreeView(SetConsoleTreeViewEnabledFromPlugIn);

			checkCompany.TopLevel = false;
			checkCompany.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, checkCompany.ClientSize.Width, checkCompany.ClientSize.Height);
			workingAreaConsole.Controls.Add(checkCompany);
			checkCompany.Show();
		}
		#endregion

		#region OnDeleteCompanyObjectHandler - Svuota un db aziendale
		/// <summary>
		/// Dal nodo Azienda scelgo di svuotare il database aziendale di tutti i suoi oggetti
		/// </summary>
		//---------------------------------------------------------------------
		private void OnDeleteCompanyObjectHandler(object sender, System.EventArgs e)
		{
			// verifico che all'azienda non sia collegato qualche utente lato MagoNet
			if (OnGetCompanyDBIsInFreeState != null)
			{
				if (!OnGetCompanyDBIsInFreeState(((PlugInTreeNode)consoleTree.SelectedNode).Id))
				{
					DiagnosticViewer.ShowCustomizeIconMessage(Strings.ErrCompanyDBIsNotFree, string.Empty, MessageBoxIcon.Exclamation);
					return;
				}
			}

			if (consoleEnvironmentInfo.IsLiteConsole)
			{
				CompanyLite deleteCompanyLite = new CompanyLite
				(
				currentStatus.OwnerDbName,
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				((PlugInTreeNode)consoleTree.SelectedNode).Id,
				pathFinder,
				licenceInfo
				);

				deleteCompanyLite.OnCallHelp += new CompanyLite.CallHelp(HelpFromPopUp);
				deleteCompanyLite.OnGetUserAuthenticatedPwdFromConsole += new CompanyLite.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				deleteCompanyLite.OnIsUserAuthenticatedFromConsole += new CompanyLite.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				deleteCompanyLite.OnAddUserAuthenticatedFromConsole += new CompanyLite.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				deleteCompanyLite.OnSetCyclicStepProgressBar += new CompanyLite.SetCyclicStepProgressBar(SetCyclicStepProgressBarFromPlugIn);
				deleteCompanyLite.OnEnableProgressBar += new CompanyLite.EnableProgressBar(EnableProgressBarFromPlugIn);
				deleteCompanyLite.OnDisableProgressBar += new CompanyLite.DisableProgressBar(DisableProgressBarFromPlugIn);
				deleteCompanyLite.OnSetProgressBarValue += new CompanyLite.SetProgressBarValue(SetProgressBarValueFromPlugIn);
				deleteCompanyLite.OnSetProgressBarStep += new CompanyLite.SetProgressBarStep(SetProgressBarStepFromPlugIn);
				deleteCompanyLite.OnSetProgressBarText += new CompanyLite.SetProgressBarText(SetProgressBarTextFromPlugIn);
				deleteCompanyLite.OnIsActivated += new CompanyLite.IsActivated(IsFunctionalityActivated);

				consoleTree.Enabled = false;
				workingAreaConsole.Enabled = false;

				DBMSType providerTypeLite = TBDatabaseType.GetDBMSType(((PlugInTreeNode)consoleTree.SelectedNode).Provider);
				if (providerTypeLite == DBMSType.SQLSERVER)
					deleteCompanyLite.DeleteAllSqlServerObjects(sender, e);

				consoleTree.Enabled = true;
				workingAreaConsole.Enabled = true;
			}
			else
			{
			Company deleteCompany = new Company
				(
				currentStatus.OwnerDbName,
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				((PlugInTreeNode)consoleTree.SelectedNode).Id,
				pathFinder,
				licenceInfo
				);

			deleteCompany.OnCallHelp += new Company.CallHelp(HelpFromPopUp);
			deleteCompany.OnGetUserAuthenticatedPwdFromConsole += new Company.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			deleteCompany.OnIsUserAuthenticatedFromConsole += new Company.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			deleteCompany.OnAddUserAuthenticatedFromConsole += new Company.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			deleteCompany.OnSetCyclicStepProgressBar += new Company.SetCyclicStepProgressBar(SetCyclicStepProgressBarFromPlugIn);
			deleteCompany.OnEnableProgressBar += new Company.EnableProgressBar(EnableProgressBarFromPlugIn);
			deleteCompany.OnDisableProgressBar += new Company.DisableProgressBar(DisableProgressBarFromPlugIn);
			deleteCompany.OnSetProgressBarValue += new Company.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			deleteCompany.OnSetProgressBarStep += new Company.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			deleteCompany.OnSetProgressBarText += new Company.SetProgressBarText(SetProgressBarTextFromPlugIn);
			deleteCompany.OnIsActivated += new Company.IsActivated(IsFunctionalityActivated);

			consoleTree.Enabled = false;
			workingAreaConsole.Enabled = false;

			DBMSType providerType = TBDatabaseType.GetDBMSType(((PlugInTreeNode)consoleTree.SelectedNode).Provider);
			if (providerType == DBMSType.SQLSERVER)
				deleteCompany.DeleteAllSqlServerObjects(sender, e);
            

			consoleTree.Enabled = true;
			workingAreaConsole.Enabled = true;
		}
		}
		#endregion

		#region company_OnBackupDb - Backup del database di un'azienda
		/// <summary>
		/// company_OnBackupDb
		/// </summary>
		//---------------------------------------------------------------------
		private void company_OnBackupDb(object sender, EventArgs e)
		{
			BackupDatabase backupDb = new BackupDatabase
				(
				true,
				((PlugInTreeNode)consoleTree.SelectedNode).Id,
				currentStatus,
				consoleEnvironmentInfo.RunningFromServer
				);

			backupDb.OnEnableProgressBar += new BackupDatabase.EnableProgressBar(EnableProgressBarFromPlugIn);
			backupDb.OnDisableProgressBar += new BackupDatabase.DisableProgressBar(DisableProgressBarFromPlugIn);
			backupDb.OnSetProgressBarStep += new BackupDatabase.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			backupDb.OnSetProgressBarText += new BackupDatabase.SetProgressBarText(SetProgressBarTextFromPlugIn);
			backupDb.OnSetProgressBarValue += new BackupDatabase.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			backupDb.OnSetCyclicStepProgressBar += new BackupDatabase.SetCyclicStepProgressBar(SetCyclicStepProgressBarFromPlugIn);
			backupDb.OnEnableConsoleTreeView += new BackupDatabase.EnableConsoleTreeView(SetConsoleTreeViewEnabledFromPlugIn);
			backupDb.OnIsActivated += new BackupDatabase.IsActivated(IsFunctionalityActivated);

			workingAreaConsole.Controls.Clear();
			if (backupDb.BackupDBDiagnostic.Error)
			{
				DiagnosticViewer.ShowDiagnostic(backupDb.BackupDBDiagnostic);
				return;
			}

			backupDb.TopLevel = false;
			backupDb.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, backupDb.Width, backupDb.Height);
			workingAreaConsole.Controls.Add(backupDb);
			backupDb.Show();
		}

		#endregion

		#region company_OnRestoreDb - Restore del database di una Azienda
		/// <summary>
		/// company_OnRestoreDb
		/// </summary>
		//---------------------------------------------------------------------
		private void company_OnRestoreDb(object sender, EventArgs e)
		{
			RestoreDatabase restoreDb = new RestoreDatabase(((PlugInTreeNode)consoleTree.SelectedNode).Id, currentStatus, licenceInfo.IsoState);
			restoreDb.OnEnableProgressBar += new RestoreDatabase.EnableProgressBar(EnableProgressBarFromPlugIn);
			restoreDb.OnDisableProgressBar += new RestoreDatabase.DisableProgressBar(DisableProgressBarFromPlugIn);
			restoreDb.OnSetProgressBarStep += new RestoreDatabase.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			restoreDb.OnSetProgressBarText += new RestoreDatabase.SetProgressBarText(SetProgressBarTextFromPlugIn);
			restoreDb.OnSetProgressBarValue += new RestoreDatabase.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			restoreDb.OnCheckRestoredDatabase += new RestoreDatabase.CheckRestoredDatabase(OnCheckRequirements);
			restoreDb.OnModifyTree += new RestoreDatabase.ModifyTree(consoleTree_onModifyAzienda);
			restoreDb.OnModifyTreeOfCompanies += new RestoreDatabase.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
			restoreDb.OnAfterModifyCompany += new RestoreDatabase.AfterModifyCompany(AfterModifyCompany);
			restoreDb.OnSetCyclicStepProgressBar += new RestoreDatabase.SetCyclicStepProgressBar(SetCyclicStepProgressBarFromPlugIn);
			restoreDb.OnEnableConsoleTreeView += new RestoreDatabase.EnableConsoleTreeView(SetConsoleTreeViewEnabledFromPlugIn);
			restoreDb.OnIsActivated += new RestoreDatabase.IsActivated(IsFunctionalityActivated);

			workingAreaConsole.Controls.Clear();
			if (restoreDb.RestoreDBDiagnostic.Error)
			{
				DiagnosticViewer.ShowDiagnostic(restoreDb.RestoreDBDiagnostic);
				return;
			}

			restoreDb.TopLevel = false;
			restoreDb.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, restoreDb.Width, restoreDb.Height);
			workingAreaConsole.Controls.Add(restoreDb);
			restoreDb.Show();
		}
		#endregion
		#endregion

		#endregion

		#region ContextMenuRoles - ContextMenu sul nodo contenitore dei Ruoli di una Azienda
		/// <summary>
		/// ContextMenuRoles
		/// Context menu che appare sul nodo tree contenitore dei ruoli per una specificata company
		/// E' un context menu castom, in cui riporto l'id della company e del ruolo (quando serve)
		/// </summary>
		//---------------------------------------------------------------------
		private void ContextMenuRoles(string companyId, DBMSType providerCompany)
		{
			customContext = new CustomContextMenu(companyId, providerCompany);
			customContext.MenuItems.Clear();
			customContext.MenuItems.Add(Strings.NewRole, new System.EventHandler(role_OnNew));
		}

		#region Handles per le voci del ContextMenu

		#region role_OnNew - Creazione di un Ruolo di una Azienda
		/// <summary>
		/// role_OnNew
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void role_OnNew(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();
			Role newRole = new Role(currentStatus.ConnectionString, currentStatus.CurrentConnection, context.CompanyId);
			newRole.OnSendDiagnostic += new Role.SendDiagnostic(ReceiveDiagnostic);
			newRole.OnModifyTreeOfCompanies += new Role.ModifyTreeOfCompanies(consoleTree_onModifyRole);
			newRole.OnAfterChangedDisabled += new Role.AfterChangedDisabled(AfterDisabledRole);
			newRole.TopLevel = false;
			newRole.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, newRole.ClientSize.Width, newRole.ClientSize.Height);
			workingAreaConsole.Controls.Add(newRole);
			newRole.Visible = true;
			newRole.Enabled = true;
			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null)
				OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
		}
		#endregion
		#endregion
		#endregion

		#region ContextMenuUsersToCompany - ContextMenu sul nodo contenitore degli Utenti di una Azienda
		/// <summary>
		/// ContextMenuUsersToCompany
		/// Context menu che appare sul nodo tree contenitore degli utenti di una company
		/// </summary>
		//---------------------------------------------------------------------
		private CustomContextMenu ContextMenuUsersToCompany(string CompanyId, string companyServer, DBMSType providerCompany)
		{
			customContext = new CustomContextMenu(CompanyId, providerCompany);
			customContext.MenuItems.Clear();

			switch (providerCompany)
			{
				case DBMSType.SQLSERVER:
					{
						if (!consoleEnvironmentInfo.IsLiteConsole)
						customContext.MenuItems.Add(string.Format(Strings.AddUsersToLogin, companyServer), new System.EventHandler(OnAddMicroareaLoginsToCompanyDb));
						customContext.MenuItems.Add(string.Format(Strings.AddUsersToLogins, companyServer), new System.EventHandler(OnJoinCompanyUsers));

						// le opzioni avanzate le mostro solo se l'utente lo ha scelto nella finestra Customize
						if (OnShowLoginAdvancedOptions != null && OnShowLoginAdvancedOptions())
						{
							//Menu opzioni avanzate
							customContext.MenuItems.Add("-");
							MenuItem menuAdvancedOptions = new MenuItem(Strings.LoginAdvancedOptions);
							menuAdvancedOptions.MenuItems.Add(Strings.ModifyUserJoin, new System.EventHandler(OnModifyJoinCompanyUsers));
							menuAdvancedOptions.MenuItems.Add(Strings.AddNewLogin, new System.EventHandler(OnAddLoginToCompanyDb));
							menuAdvancedOptions.MenuItems.Add(Strings.LoginAdministration, new System.EventHandler(OnManagerDatabaseLogin));
							customContext.MenuItems.Add(menuAdvancedOptions);
						}
						break;
					}

				case DBMSType.ORACLE:
					{
						customContext.MenuItems.Add(string.Format(Strings.AddOracleUsersToLogins, companyServer), new System.EventHandler(OnJoinCompanyUsers));
							
						// l'opzione "Change associations" la mostro solo se l'utente lo ha scelto nella finestra Customize
						if (OnShowLoginAdvancedOptions != null && OnShowLoginAdvancedOptions())
						{
							customContext.MenuItems.Add("-");
							customContext.MenuItems.Add(Strings.ModifyUserJoin, new System.EventHandler(OnModifyJoinCompanyUsers));
						}
						break;
					}

                case DBMSType.POSTGRE:
                    {
                        customContext.MenuItems.Add(string.Format(Strings.AddUsersToLoginPostgre, companyServer), new System.EventHandler(OnAddMicroareaLoginsToCompanyDb));
                        customContext.MenuItems.Add(string.Format(Strings.AddUsersToLoginsPostgre, companyServer), new System.EventHandler(OnJoinCompanyUsers));

                        // le opzioni avanzate le mostro solo se l'utente lo ha scelto nella finestra Customize
                        if (OnShowLoginAdvancedOptions != null && OnShowLoginAdvancedOptions())
                        {
                            //Menu opzioni avanzate
                            customContext.MenuItems.Add("-");
                            MenuItem menuAdvancedOptions = new MenuItem(Strings.LoginAdvancedOptions);
                            menuAdvancedOptions.MenuItems.Add(Strings.ModifyUserJoin, new System.EventHandler(OnModifyJoinCompanyUsers));
                            menuAdvancedOptions.MenuItems.Add(Strings.AddNewLogin, new System.EventHandler(OnAddLoginToCompanyDb));
                            menuAdvancedOptions.MenuItems.Add(Strings.LoginAdministration, new System.EventHandler(OnManagerDatabaseLogin));
                            customContext.MenuItems.Add(menuAdvancedOptions);
                        }
                        break;
                    }
			}

			return customContext;
		}

		#region Handles per le voci del ContextMenu

		#region OnAddMicroareaLoginsToCompanyDb - Aggiunge MicroareaLogin come login sql
		/// <summary>
		/// OnAddMicroareaLoginsToCompanyDb
		/// Associazione utenti applicativi all'azienda 1 a 1 (solo per aziende su SQL Server e Postgre)
		/// </summary>
		//---------------------------------------------------------------------
		private void OnAddMicroareaLoginsToCompanyDb(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			workingAreaConsole.Visible = true;
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();

            if (context.CompanyProvider == DBMSType.SQLSERVER)
            {
                AddCompanyUserToLogin addCompanyUserToLogin = new AddCompanyUserToLogin
			    (
			    currentStatus.ConnectionString,
			    currentStatus.CurrentConnection,
			    context.CompanyId,
			    context.CompanyProvider,
			    brandLoader
			    );
                addCompanyUserToLogin.OnCallHelpFromPopUp += new AddCompanyUserToLogin.CallHelpFromPopUp(HelpFromPopUp);
                addCompanyUserToLogin.OnSaveUsers += new AddCompanyUserToLogin.SaveUsers(OnSaveCompanyUser);
                addCompanyUserToLogin.OnSendDiagnostic += new AddCompanyUserToLogin.SendDiagnostic(ReceiveDiagnostic);
                addCompanyUserToLogin.OnModifyTreeOfCompanies += new AddCompanyUserToLogin.ModifyTreeOfCompanies(consoleTree_OnModifyLoginCompany);
                addCompanyUserToLogin.OnAddUserAuthenticatedFromConsole += new AddCompanyUserToLogin.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
                addCompanyUserToLogin.OnGetUserAuthenticatedPwdFromConsole += new AddCompanyUserToLogin.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
                addCompanyUserToLogin.OnIsUserAuthenticatedFromConsole += new AddCompanyUserToLogin.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
                addCompanyUserToLogin.OnIsActivated += new AddCompanyUserToLogin.IsActivated(IsFunctionalityActivated);

                addCompanyUserToLogin.TopLevel = false;
                addCompanyUserToLogin.Dock = DockStyle.Fill;
                //eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
                OnBeforeAddFormFromPlugIn(sender, addCompanyUserToLogin.ClientSize.Width, addCompanyUserToLogin.ClientSize.Height);
                workingAreaConsole.Controls.Add(addCompanyUserToLogin);
                addCompanyUserToLogin.Show();
            }
            else if (context.CompanyProvider == DBMSType.POSTGRE)
            {
                AddCompanyUserToLoginPostgre addCompanyUserToLogin = new AddCompanyUserToLoginPostgre
                (
                currentStatus.ConnectionString,
                currentStatus.CurrentConnection,
                context.CompanyId,
                context.CompanyProvider,
                brandLoader
                );
                addCompanyUserToLogin.OnCallHelpFromPopUp += new AddCompanyUserToLoginPostgre.CallHelpFromPopUp(HelpFromPopUp);
                addCompanyUserToLogin.OnSaveUsers += new AddCompanyUserToLoginPostgre.SaveUsers(OnSaveCompanyUser);
                addCompanyUserToLogin.OnSendDiagnostic += new AddCompanyUserToLoginPostgre.SendDiagnostic(ReceiveDiagnostic);
                addCompanyUserToLogin.OnModifyTreeOfCompanies += new AddCompanyUserToLoginPostgre.ModifyTreeOfCompanies(consoleTree_OnModifyLoginCompany);
                addCompanyUserToLogin.OnAddUserAuthenticatedFromConsole += new AddCompanyUserToLoginPostgre.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
                addCompanyUserToLogin.OnGetUserAuthenticatedPwdFromConsole += new AddCompanyUserToLoginPostgre.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
                addCompanyUserToLogin.OnIsUserAuthenticatedFromConsole += new AddCompanyUserToLoginPostgre.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
                //addCompanyUserToLogin.OnIsActivated += new AddCompanyUserToLoginPostgre.IsActivated(IsFunctionalityActivated);

                addCompanyUserToLogin.TopLevel = false;
                addCompanyUserToLogin.Dock = DockStyle.Fill;
                //eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
                OnBeforeAddFormFromPlugIn(sender, addCompanyUserToLogin.ClientSize.Width, addCompanyUserToLogin.ClientSize.Height);
                workingAreaConsole.Controls.Add(addCompanyUserToLogin);
                addCompanyUserToLogin.Show();
            }

			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
		}
		#endregion

		#region OnJoinCompanyUsers - Associa uno o più utenti applicativi
		/// <summary>
		/// OnJoinCompanyUsers
		/// Associazione utenti applicativi all'azienda 1 a n
		/// </summary>
		//---------------------------------------------------------------------
		private void OnJoinCompanyUsers(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			workingAreaConsole.Visible = true;
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();

			if (context.CompanyProvider == DBMSType.SQLSERVER)
			{
				if (consoleEnvironmentInfo.IsLiteConsole)
				{
					AddCompanyUsersToLoginLite addCompanyUsersToLoginLite = new AddCompanyUsersToLoginLite
						(
						currentStatus.ConnectionString,
						currentStatus.CurrentConnection,
						context.CompanyId,
						brandLoader
						);
					addCompanyUsersToLoginLite.OnCallHelpFromPopUp += new AddCompanyUsersToLoginLite.CallHelpFromPopUp(HelpFromPopUp);
					addCompanyUsersToLoginLite.OnSaveUsers += new AddCompanyUsersToLoginLite.SaveUsers(OnSaveCompanyUser);
					addCompanyUsersToLoginLite.OnSendDiagnostic += new AddCompanyUsersToLoginLite.SendDiagnostic(ReceiveDiagnostic);
					addCompanyUsersToLoginLite.OnModifyTreeOfCompanies += new AddCompanyUsersToLoginLite.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
					addCompanyUsersToLoginLite.OnAddUserAuthenticatedFromConsole += new AddCompanyUsersToLoginLite.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
					addCompanyUsersToLoginLite.OnGetUserAuthenticatedPwdFromConsole += new AddCompanyUsersToLoginLite.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
					addCompanyUsersToLoginLite.OnIsUserAuthenticatedFromConsole += new AddCompanyUsersToLoginLite.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
					addCompanyUsersToLoginLite.OnIsActivated += new AddCompanyUsersToLoginLite.IsActivated(IsFunctionalityActivated);

					addCompanyUsersToLoginLite.TopLevel = false;
					addCompanyUsersToLoginLite.Dock = DockStyle.Fill;
					//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
					OnBeforeAddFormFromPlugIn(sender, addCompanyUsersToLoginLite.ClientSize.Width, addCompanyUsersToLoginLite.ClientSize.Height);
					workingAreaConsole.Controls.Add(addCompanyUsersToLoginLite);
					addCompanyUsersToLoginLite.Show();
				}
				else
				{
				AddCompanyUsersToLogin addCompanyUsersToLogin = new AddCompanyUsersToLogin
					(
					currentStatus.ConnectionString,
					currentStatus.CurrentConnection,
					context.CompanyId,
					brandLoader
					);
				addCompanyUsersToLogin.OnCallHelpFromPopUp += new AddCompanyUsersToLogin.CallHelpFromPopUp(HelpFromPopUp);
				addCompanyUsersToLogin.OnSaveUsers += new AddCompanyUsersToLogin.SaveUsers(OnSaveCompanyUser);
				addCompanyUsersToLogin.OnSendDiagnostic += new AddCompanyUsersToLogin.SendDiagnostic(ReceiveDiagnostic);
				addCompanyUsersToLogin.OnModifyTreeOfCompanies += new AddCompanyUsersToLogin.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
				addCompanyUsersToLogin.OnAddUserAuthenticatedFromConsole += new AddCompanyUsersToLogin.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				addCompanyUsersToLogin.OnGetUserAuthenticatedPwdFromConsole += new AddCompanyUsersToLogin.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				addCompanyUsersToLogin.OnIsUserAuthenticatedFromConsole += new AddCompanyUsersToLogin.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				addCompanyUsersToLogin.OnIsActivated += new AddCompanyUsersToLogin.IsActivated(IsFunctionalityActivated);

				addCompanyUsersToLogin.TopLevel = false;
				addCompanyUsersToLogin.Dock = DockStyle.Fill;
				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, addCompanyUsersToLogin.ClientSize.Width, addCompanyUsersToLogin.ClientSize.Height);
				workingAreaConsole.Controls.Add(addCompanyUsersToLogin);
				addCompanyUsersToLogin.Show();
			}
			}
			else if (context.CompanyProvider == DBMSType.ORACLE)
			{
				AddCompanyUserToOracleLogin addOracleLogins = new AddCompanyUserToOracleLogin(currentStatus.ConnectionString, currentStatus.CurrentConnection, context.CompanyId);
				addOracleLogins.OnCallHelpFromPopUp += new AddCompanyUserToOracleLogin.CallHelpFromPopUp(HelpFromPopUp);
				addOracleLogins.OnSaveUsers += new AddCompanyUserToOracleLogin.SaveUsers(OnSaveCompanyUser);
				addOracleLogins.OnSendDiagnostic += new AddCompanyUserToOracleLogin.SendDiagnostic(ReceiveDiagnostic);
				addOracleLogins.OnModifyTreeOfCompanies += new AddCompanyUserToOracleLogin.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
				addOracleLogins.OnAddUserAuthenticatedFromConsole += new AddCompanyUserToOracleLogin.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				addOracleLogins.OnGetUserAuthenticatedPwdFromConsole += new AddCompanyUserToOracleLogin.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				addOracleLogins.OnIsUserAuthenticatedFromConsole += new AddCompanyUserToOracleLogin.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				addOracleLogins.OnEnableProgressBar += new AddCompanyUserToOracleLogin.EnableProgressBar(EnableProgressBarFromPlugIn);
				addOracleLogins.OnDisableProgressBar += new AddCompanyUserToOracleLogin.DisableProgressBar(DisableProgressBarFromPlugIn);
				addOracleLogins.OnSetProgressBarStep += new AddCompanyUserToOracleLogin.SetProgressBarStep(SetProgressBarStepFromPlugIn);
				addOracleLogins.OnSetProgressBarText += new AddCompanyUserToOracleLogin.SetProgressBarText(SetProgressBarTextFromPlugIn);
				addOracleLogins.OnSetProgressBarValue += new AddCompanyUserToOracleLogin.SetProgressBarValue(SetProgressBarValueFromPlugIn);
				addOracleLogins.OnIsActivated += new AddCompanyUserToOracleLogin.IsActivated(IsFunctionalityActivated);

				addOracleLogins.TopLevel = false;
				addOracleLogins.Dock = DockStyle.Fill;
				OnBeforeAddFormFromPlugIn(sender, addOracleLogins.ClientSize.Width, addOracleLogins.ClientSize.Height);
				workingAreaConsole.Controls.Add(addOracleLogins);
				addOracleLogins.Show();
			}

            else if (context.CompanyProvider == DBMSType.POSTGRE)
            {
                AddCompanyUsersToLoginPostgre addCompanyUsersToLoginPostgre = new AddCompanyUsersToLoginPostgre
                    (
                    currentStatus.ConnectionString,
                    currentStatus.CurrentConnection,
                    context.CompanyId,
                    brandLoader
                    );
                addCompanyUsersToLoginPostgre.OnCallHelpFromPopUp += new AddCompanyUsersToLoginPostgre.CallHelpFromPopUp(HelpFromPopUp);
                addCompanyUsersToLoginPostgre.OnSaveUsers += new AddCompanyUsersToLoginPostgre.SaveUsers(OnSaveCompanyUser);
                addCompanyUsersToLoginPostgre.OnSendDiagnostic += new AddCompanyUsersToLoginPostgre.SendDiagnostic(ReceiveDiagnostic);
                addCompanyUsersToLoginPostgre.OnModifyTreeOfCompanies += new AddCompanyUsersToLoginPostgre.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
                addCompanyUsersToLoginPostgre.OnAddUserAuthenticatedFromConsole += new AddCompanyUsersToLoginPostgre.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
                addCompanyUsersToLoginPostgre.OnGetUserAuthenticatedPwdFromConsole += new AddCompanyUsersToLoginPostgre.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
                addCompanyUsersToLoginPostgre.OnIsUserAuthenticatedFromConsole += new AddCompanyUsersToLoginPostgre.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
                //addCompanyUsersToLoginPostgre.OnIsActivated += new AddCompanyUsersToLoginPostgre.IsActivated(IsFunctionalityActivated);

                addCompanyUsersToLoginPostgre.TopLevel = false;
                addCompanyUsersToLoginPostgre.Dock = DockStyle.Fill;
                //eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
                OnBeforeAddFormFromPlugIn(sender, addCompanyUsersToLoginPostgre.ClientSize.Width, addCompanyUsersToLoginPostgre.ClientSize.Height);
                workingAreaConsole.Controls.Add(addCompanyUsersToLoginPostgre);
                addCompanyUsersToLoginPostgre.Show();
            }
		}
		#endregion

		#region OnModifyJoinCompanyUsers - Modifica l'associazione di una login a uno o più utenti applicativi
		/// <summary>
		/// OnModifyJoinCompanyUsers
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnModifyJoinCompanyUsers(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			workingAreaConsole.Visible = true;
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();

			if (context.CompanyProvider == DBMSType.SQLSERVER)
			{
				ModifyCompanyUsersToLogin modifyCompanyUsersToLogin = new ModifyCompanyUsersToLogin(currentStatus.ConnectionString, currentStatus.CurrentConnection, context.CompanyId);
				modifyCompanyUsersToLogin.OnCallHelpFromPopUp += new ModifyCompanyUsersToLogin.CallHelpFromPopUp(HelpFromPopUp);
				modifyCompanyUsersToLogin.OnAfterDisableUser += new ModifyCompanyUsersToLogin.AfterDisableUser(OnDeleteAssociationToLoginManager);
				modifyCompanyUsersToLogin.OnDeleteAssociation += new ModifyCompanyUsersToLogin.DeleteAssociation(OnDeleteAssociationToLoginManager);
				modifyCompanyUsersToLogin.OnUnLockAllForUser += new ModifyCompanyUsersToLogin.UnLockAllForUser(OnUnlockAllForUser);
				modifyCompanyUsersToLogin.OnSaveUsers += new ModifyCompanyUsersToLogin.SaveUsers(OnSaveCompanyUser);
				modifyCompanyUsersToLogin.OnSendDiagnostic += new ModifyCompanyUsersToLogin.SendDiagnostic(ReceiveDiagnostic);
				modifyCompanyUsersToLogin.OnModifyTree += new ModifyCompanyUsersToLogin.ModifyTree(consoleTree_onModifyAzienda);
				modifyCompanyUsersToLogin.OnModifyTreeOfCompanies += new ModifyCompanyUsersToLogin.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
				modifyCompanyUsersToLogin.OnAddUserAuthenticatedFromConsole += new ModifyCompanyUsersToLogin.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				modifyCompanyUsersToLogin.OnGetUserAuthenticatedPwdFromConsole += new ModifyCompanyUsersToLogin.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				modifyCompanyUsersToLogin.OnIsUserAuthenticatedFromConsole += new ModifyCompanyUsersToLogin.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				modifyCompanyUsersToLogin.TopLevel = false;
				modifyCompanyUsersToLogin.Dock = DockStyle.Fill;
				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, modifyCompanyUsersToLogin.ClientSize.Width, modifyCompanyUsersToLogin.ClientSize.Height);
				workingAreaConsole.Controls.Add(modifyCompanyUsersToLogin);
				modifyCompanyUsersToLogin.Show();
			}
			else if (context.CompanyProvider == DBMSType.ORACLE)
			{
				ModifyCompanyUsersToOracleLogin modifyOracleLogin = new ModifyCompanyUsersToOracleLogin(currentStatus.ConnectionString, currentStatus.CurrentConnection, context.CompanyId);
				modifyOracleLogin.OnCallHelpFromPopUp += new ModifyCompanyUsersToOracleLogin.CallHelpFromPopUp(HelpFromPopUp);
				modifyOracleLogin.OnAfterDisableUser += new ModifyCompanyUsersToOracleLogin.AfterDisableUser(OnDeleteAssociationToLoginManager);
				modifyOracleLogin.OnDeleteAssociation += new ModifyCompanyUsersToOracleLogin.DeleteAssociation(OnDeleteAssociationToLoginManager);
				modifyOracleLogin.OnUnLockAllForUser += new ModifyCompanyUsersToOracleLogin.UnLockAllForUser(OnUnlockAllForUser);
				modifyOracleLogin.OnSaveUsers += new ModifyCompanyUsersToOracleLogin.SaveUsers(OnSaveCompanyUser);
				modifyOracleLogin.OnSendDiagnostic += new ModifyCompanyUsersToOracleLogin.SendDiagnostic(ReceiveDiagnostic);
				modifyOracleLogin.OnModifyTree += new ModifyCompanyUsersToOracleLogin.ModifyTree(consoleTree_onModifyAzienda);
				modifyOracleLogin.OnModifyTreeOfCompanies += new ModifyCompanyUsersToOracleLogin.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
				modifyOracleLogin.OnAddUserAuthenticatedFromConsole += new ModifyCompanyUsersToOracleLogin.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				modifyOracleLogin.OnGetUserAuthenticatedPwdFromConsole += new ModifyCompanyUsersToOracleLogin.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				modifyOracleLogin.OnIsUserAuthenticatedFromConsole += new ModifyCompanyUsersToOracleLogin.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				modifyOracleLogin.OnEnableProgressBar += new ModifyCompanyUsersToOracleLogin.EnableProgressBar(EnableProgressBarFromPlugIn);
				modifyOracleLogin.OnDisableProgressBar += new ModifyCompanyUsersToOracleLogin.DisableProgressBar(DisableProgressBarFromPlugIn);
				modifyOracleLogin.OnSetProgressBarStep += new ModifyCompanyUsersToOracleLogin.SetProgressBarStep(SetProgressBarStepFromPlugIn);
				modifyOracleLogin.OnSetProgressBarText += new ModifyCompanyUsersToOracleLogin.SetProgressBarText(SetProgressBarTextFromPlugIn);
				modifyOracleLogin.OnSetProgressBarValue += new ModifyCompanyUsersToOracleLogin.SetProgressBarValue(SetProgressBarValueFromPlugIn);
				modifyOracleLogin.TopLevel = false;
				modifyOracleLogin.Dock = DockStyle.Fill;
				OnBeforeAddFormFromPlugIn(sender, modifyOracleLogin.ClientSize.Width, modifyOracleLogin.ClientSize.Height);
				workingAreaConsole.Controls.Add(modifyOracleLogin);
				modifyOracleLogin.Show();
			}
            else if (context.CompanyProvider == DBMSType.POSTGRE)
            {
                ModifyCompanyUsersToLoginPostgre modifyCompanyUsersToLoginPostgre = new ModifyCompanyUsersToLoginPostgre(currentStatus.ConnectionString, currentStatus.CurrentConnection, context.CompanyId);
                modifyCompanyUsersToLoginPostgre.OnCallHelpFromPopUp += new ModifyCompanyUsersToLoginPostgre.CallHelpFromPopUp(HelpFromPopUp);
                modifyCompanyUsersToLoginPostgre.OnAfterDisableUser += new ModifyCompanyUsersToLoginPostgre.AfterDisableUser(OnDeleteAssociationToLoginManager);
                modifyCompanyUsersToLoginPostgre.OnDeleteAssociation += new ModifyCompanyUsersToLoginPostgre.DeleteAssociation(OnDeleteAssociationToLoginManager);
                modifyCompanyUsersToLoginPostgre.OnUnLockAllForUser += new ModifyCompanyUsersToLoginPostgre.UnLockAllForUser(OnUnlockAllForUser);
                modifyCompanyUsersToLoginPostgre.OnSaveUsers += new ModifyCompanyUsersToLoginPostgre.SaveUsers(OnSaveCompanyUser);
                modifyCompanyUsersToLoginPostgre.OnSendDiagnostic += new ModifyCompanyUsersToLoginPostgre.SendDiagnostic(ReceiveDiagnostic);
                modifyCompanyUsersToLoginPostgre.OnModifyTree += new ModifyCompanyUsersToLoginPostgre.ModifyTree(consoleTree_onModifyAzienda);
                modifyCompanyUsersToLoginPostgre.OnModifyTreeOfCompanies += new ModifyCompanyUsersToLoginPostgre.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
                modifyCompanyUsersToLoginPostgre.OnAddUserAuthenticatedFromConsole += new ModifyCompanyUsersToLoginPostgre.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
                modifyCompanyUsersToLoginPostgre.OnGetUserAuthenticatedPwdFromConsole += new ModifyCompanyUsersToLoginPostgre.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
                modifyCompanyUsersToLoginPostgre.OnIsUserAuthenticatedFromConsole += new ModifyCompanyUsersToLoginPostgre.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
                modifyCompanyUsersToLoginPostgre.TopLevel = false;
                modifyCompanyUsersToLoginPostgre.Dock = DockStyle.Fill;
                //eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
                OnBeforeAddFormFromPlugIn(sender, modifyCompanyUsersToLoginPostgre.ClientSize.Width, modifyCompanyUsersToLoginPostgre.ClientSize.Height);
                workingAreaConsole.Controls.Add(modifyCompanyUsersToLoginPostgre);
                modifyCompanyUsersToLoginPostgre.Show();
            }
		}
		#endregion

		#region OnAddLoginToCompanyDb - Aggiunge una login su sql dove installato il db di sistema
		/// <summary>
		/// OnAddLoginToCompanyDb
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnAddLoginToCompanyDb(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			workingAreaConsole.Visible = true;
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();
			AddLoginToCompany addLoginToCompany = new AddLoginToCompany(currentStatus.ConnectionString, currentStatus.CurrentConnection, context.CompanyId);
			addLoginToCompany.OnCallHelpFromPopUp += new AddLoginToCompany.CallHelpFromPopUp(HelpFromPopUp);
			addLoginToCompany.OnSendDiagnostic += new AddLoginToCompany.SendDiagnostic(ReceiveDiagnostic);
			addLoginToCompany.OnModifyTreeOfCompanies += new AddLoginToCompany.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
			addLoginToCompany.OnAddUserAuthenticatedFromConsole += new AddLoginToCompany.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			addLoginToCompany.OnGetUserAuthenticatedPwdFromConsole += new AddLoginToCompany.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			addLoginToCompany.OnIsUserAuthenticatedFromConsole += new AddLoginToCompany.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			addLoginToCompany.TopLevel = false;
			addLoginToCompany.Dock = DockStyle.Fill;

			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null)
				OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, addLoginToCompany.ClientSize.Width, addLoginToCompany.ClientSize.Height);
			workingAreaConsole.Controls.Add(addLoginToCompany);
			addLoginToCompany.LoadData();
			addLoginToCompany.Show();
		}
		#endregion

		#region OnManagerDatabaseLogin - Manutenzione delle Logins di database
		/// <summary>
		/// OnManagerDatabaseLogin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnManagerDatabaseLogin(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			workingAreaConsole.Visible = true;
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();
			LoginAdministrator loginAdministrator = new LoginAdministrator(currentStatus.ConnectionString, currentStatus.CurrentConnection, context.CompanyId);
			loginAdministrator.OnCallHelpFromPopUp += new LoginAdministrator.CallHelpFromPopUp(HelpFromPopUp);
			loginAdministrator.OnDeleteAssociation += new LoginAdministrator.DeleteAssociation(OnDeleteAssociationToLoginManager);
			loginAdministrator.OnUnLockAllForUser += new LoginAdministrator.UnLockAllForUser(OnUnlockAllForUser);
			loginAdministrator.OnSendDiagnostic += new LoginAdministrator.SendDiagnostic(ReceiveDiagnostic);
			loginAdministrator.OnModifyTreeOfCompanies += new LoginAdministrator.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
			loginAdministrator.OnModifyTree += new LoginAdministrator.ModifyTree(consoleTree_onModifyAzienda);
			loginAdministrator.OnAddUserAuthenticatedFromConsole += new LoginAdministrator.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			loginAdministrator.OnGetUserAuthenticatedPwdFromConsole += new LoginAdministrator.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			loginAdministrator.OnIsUserAuthenticatedFromConsole += new LoginAdministrator.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			loginAdministrator.TopLevel = false;
			loginAdministrator.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, loginAdministrator.ClientSize.Width, loginAdministrator.ClientSize.Height);
			workingAreaConsole.Controls.Add(loginAdministrator);
			loginAdministrator.Show();
		}
		#endregion
		#endregion
		#endregion

		#region ContextMenuRoleUserCompany - ContextMenu sul nodo contenitore degli Utenti di un Ruolo di una Azienda
		/// <summary>
		/// ContextMenuRoleUserCompany
		/// Context menu che appare sul nodo tree contenitore degli utenti associati a una role di una company
		/// </summary>
		/// <param name="CompanyId"></param>
		/// <param name="RoleId"></param>
		//---------------------------------------------------------------------
		private void ContextMenuRoleUserCompany(string CompanyId, string RoleId, bool readOnly)
		{
			customContext = new CustomContextMenu(CompanyId, RoleId);
			customContext.MenuItems.Clear();
            if (readOnly)
            {
                customContext.MenuItems.Add(Strings.UserJoin, new System.EventHandler(OnNewRoleUserCompany));
                return;
            }

			customContext.MenuItems.Add(Strings.Properties, new System.EventHandler(OnViewRoleCompany));
			customContext.MenuItems.Add("-");
			customContext.MenuItems.Add(Strings.UserJoin, new System.EventHandler(OnNewRoleUserCompany));
			customContext.MenuItems.Add("-");
			customContext.MenuItems.Add(Strings.CloneRole, new System.EventHandler(OnCloneRoleCompany));
			customContext.MenuItems.Add(Strings.DeleteRole, new System.EventHandler(OnDeleteRoleCompany));
		}

		#region Handles per le voci del ContextMenu

		#region OnViewRoleCompany - Visualizza il ruolo di una azienda
		/// <summary>
		/// OnViewRoleCompany
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnViewRoleCompany(object sender, System.EventArgs e)
		{
			try
			{
				workingAreaConsole.Controls.Clear();
				PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
				OnModifyRuolo(sender, selectedNode.Id, selectedNode.CompanyId);
			}
			catch (System.Exception err)
			{
				Debug.Fail(err.Message);
			}
		}
		#endregion

		#region OnCloneRoleCompany - Clonazione del Ruolo di una Azienda
		/// <summary>
		/// OnCloneRoleCompany
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnCloneRoleCompany(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();
			CloneCompanyRole cloneCompanyRole = new CloneCompanyRole
				(
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				context.CompanyId,
				context.RoleId
				);
			cloneCompanyRole.OnSendDiagnostic += new CloneCompanyRole.SendDiagnostic(ReceiveDiagnostic);
			cloneCompanyRole.OnModifyTreeOfCompanies += new CloneCompanyRole.ModifyTreeOfCompanies(consoleTree_onModifyRole);
			cloneCompanyRole.OnAfterClonedRole += new CloneCompanyRole.AfterClonedRole(cloneRole_OnAfterClonedCompany);
			cloneCompanyRole.TopLevel = false;
			cloneCompanyRole.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, cloneCompanyRole.ClientSize.Width, cloneCompanyRole.ClientSize.Height);
			workingAreaConsole.Controls.Add(cloneCompanyRole);
			cloneCompanyRole.Show();
		}
		#endregion

		#region OnDeleteRoleCompany - Cancellazione del Ruolo di una Azienda
		/// <summary>
		/// OnDeleteRoleCompany
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnDeleteRoleCompany(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();
			OnDeleteRole(sender, context.RoleId, context.CompanyId);
		}
		#endregion

		#region OnNewRoleUserCompany - Associo un Utente di una Azienda a un Ruolo della stessa Azienda
		/// <summary>
		/// OnNewRoleUserCompany
		/// inserisco una nuova associazione ruolo (di una company ) - utente (di una company)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnNewRoleUserCompany(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();
			JoinUserRole newJoinUserRole = new JoinUserRole
				(
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				context.CompanyId,
				context.RoleId,
				consoleTree.ImageList
				);
			newJoinUserRole.OnSendDiagnostic += new JoinUserRole.SendDiagnostic(ReceiveDiagnostic);
			newJoinUserRole.OnModifyTreeOfCompanies += new JoinUserRole.ModifyTreeOfCompanies(consoleTree_onModifyRole);
			newJoinUserRole.TopLevel = false;
			newJoinUserRole.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, newJoinUserRole.ClientSize.Width, newJoinUserRole.ClientSize.Height);
			workingAreaConsole.Controls.Add(newJoinUserRole);
			newJoinUserRole.Show();
			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null)
				OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
		}
		#endregion
		#endregion
		#endregion

		#region ContextMenuUserCompany - ContextMenu sul nodo di un Utente di una Azienda
		/// <summary>
		/// ContextMenuUserCompany
		/// </summary>
		//---------------------------------------------------------------------
		private void ContextMenuUserCompany(string companyId, string loginId, DBMSType providerOfCompany)
		{
			customContext = new CustomContextMenu(companyId, null, loginId, providerOfCompany);
			customContext.MenuItems.Clear();
			customContext.MenuItems.Add(Strings.Properties, new System.EventHandler(OnViewUserCompany));
			customContext.MenuItems.Add("-");

			if (IsSecurityCompany(companyId) && !IsGuestUser(loginId) && !IsEasyLookSystem(loginId))
			{
				customContext.MenuItems.Add(Strings.JoinRole, new System.EventHandler(OnJoinRoleUser));
				customContext.MenuItems.Add("-");
			}

			if (providerOfCompany == DBMSType.SQLSERVER)
			{
				CompanyUserDb companyUserDb = new CompanyUserDb();
				companyUserDb.ConnectionString = currentStatus.ConnectionString;
				companyUserDb.CurrentSqlConnection = currentStatus.CurrentConnection;

				// aggiungo il menu di contesto per la clonazione utente solo se:
				// 1. non si tratta dell'utente Anonymous
				// 2. non si tratta dell'utente EasyLookSystem
				// 3. non si tratta dell'utente dbowner del database
				// 4. non sono in versione Lite
				if (!IsGuestUser(loginId) && !IsEasyLookSystem(loginId) && !companyUserDb.IsDbo(loginId, companyId) && !consoleEnvironmentInfo.IsLiteConsole)
					customContext.MenuItems.Add(Strings.CloneUser, new System.EventHandler(OnCloneUser));
			}

			customContext.MenuItems.Add(Strings.DeleteUser, new System.EventHandler(OnDeleteUserCompany));
		}

		#region IsGuestUser - True se l'utente identificato da loginid è Guest
		/// <summary>
		/// IsGuestUser
		/// </summary>
		//---------------------------------------------------------------------
		private bool IsGuestUser(string loginId)
		{
			UserDb userDb = new UserDb();
			userDb.ConnectionString = currentStatus.ConnectionString;
			userDb.CurrentSqlConnection = currentStatus.CurrentConnection;
			return userDb.IsGuestUser(loginId);
		}
		#endregion

		# region IsEasyLookSystem - True se l'utente identificato da loginId è EasyLookSystem
		//---------------------------------------------------------------------
		private bool IsEasyLookSystem(string loginId)
		{
			UserDb userDb = new UserDb();
			userDb.ConnectionString = currentStatus.ConnectionString;
			userDb.CurrentSqlConnection = currentStatus.CurrentConnection;
			return userDb.IsEasyLookSystemUser(loginId);
		}
		# endregion

		#region IsSecurityCompany - True se l'azienda è soggetta a Security (e quindi costruisco un context menu appropriato)
		/// <summary>
		/// IsSecurityCompany
		/// </summary>
		//---------------------------------------------------------------------
		private bool IsSecurityCompany(string companyId)
		{
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = currentStatus.ConnectionString;
			companyDb.CurrentSqlConnection = currentStatus.CurrentConnection;
			return companyDb.IsSecurityCompany(companyId);
		}
		#endregion

		#region Handles per le voci del ContextMenu

		#region OnViewUserCompany - Visualizza l'utente associato all'azienda
		/// <summary>
		/// OnViewUserCompany
		/// </summary>
		//---------------------------------------------------------------------
		private void OnViewUserCompany(object sender, System.EventArgs e)
		{
			try
			{
				workingAreaConsole.Controls.Clear();
				PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
				OnModifyCompanyUser(sender, selectedNode.Id, selectedNode.CompanyId);
			}
			catch (System.Exception err)
			{
				Debug.Fail(err.Message);
			}
		}
		#endregion

		#region OnCloneUser - Nuova Clonazione di un Utente associato a un'Azienda
		/// <summary>
		/// OnCloneUser
		/// </summary>
		//---------------------------------------------------------------------
		private void OnCloneUser(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();

			CloneUser cloneUser =
				new CloneUser(currentStatus.ConnectionString, currentStatus.CurrentConnection, context.CompanyId, context.LoginId, pathFinder);

			cloneUser.OnSendDiagnostic += new CloneUser.SendDiagnostic(ReceiveDiagnostic);
			cloneUser.OnModifyTreeOfCompanies += new CloneUser.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
			cloneUser.OnModifyTree += new CloneUser.ModifyTree(consoleTree_OnModifyUtente);
			cloneUser.OnAfterClonedUserCompany += new CloneUser.AfterClonedUserCompany(cloneUserCompany_OnAfterClonedUserCompany);
			cloneUser.OnAddUserAuthenticatedFromConsole += new CloneUser.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			cloneUser.OnGetUserAuthenticatedPwdFromConsole += new CloneUser.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			cloneUser.OnIsUserAuthenticatedFromConsole += new CloneUser.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			cloneUser.OnCallHelpFromPopUp += new CloneUser.CallHelpFromPopUp(HelpFromPopUp);
			cloneUser.OnIsActivated += new CloneUser.IsActivated(IsFunctionalityActivated);

			cloneUser.TopLevel = false;
			cloneUser.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, cloneUser.ClientSize.Width, cloneUser.ClientSize.Height);
			workingAreaConsole.Controls.Add(cloneUser);
			cloneUser.Show();
		}
		#endregion

		#region OnDeleteUserCompany - Cancellazione di un Utente associato ad un'Azienda
		/// <summary>
		/// OnDeleteUserCompany
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnDeleteUserCompany(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();
			if (context.CompanyId.Length > 0 && context.LoginId.Length > 0)
				OnDeleteCompanyUser(sender, context.LoginId, context.CompanyId, context.CompanyProvider);
		}
		#endregion

		#region OnJoinRoleUser - Associazione di un Utente a una Azienda
		/// <summary>
		/// OnJoinRoleUser
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnJoinRoleUser(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();
			JoinRoleUser joinRoleUser = new JoinRoleUser
				(
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				context.CompanyId,
				context.LoginId,
				consoleTree.ImageList
				);
			joinRoleUser.OnSendDiagnostic += new JoinRoleUser.SendDiagnostic(ReceiveDiagnostic);
			joinRoleUser.OnModifyTreeOfCompanies += new JoinRoleUser.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
			joinRoleUser.TopLevel = false;
			joinRoleUser.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, joinRoleUser.ClientSize.Width, joinRoleUser.ClientSize.Height);
			workingAreaConsole.Controls.Add(joinRoleUser);
			joinRoleUser.Show();
			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null)
				OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
		}
		#endregion

		#endregion
		#endregion

		#region ContextMenuUsersRolesOfCompany - ContextMenu sul nodo di un Utente di un Ruolo di un'Azienda
		/// <summary>
		/// ContextMenuUsersRolesOfCompany
		/// </summary>
		/// <param name="CompanyId"></param>
		/// <param name="RoleId"></param>
		/// <param name="LoginId"></param>
		//---------------------------------------------------------------------
		private void ContextMenuUsersRolesOfCompany(string CompanyId, string RoleId, string LoginId)
		{
			customContext = new CustomContextMenu(CompanyId, RoleId, LoginId);
			customContext.MenuItems.Clear();
			customContext.MenuItems.Add(Strings.Properties, new System.EventHandler(OnViewUserRoleCompany));
			customContext.MenuItems.Add("-");
			customContext.MenuItems.Add(Strings.DeleteUser, new System.EventHandler(OnDeleteUserRoleCompany));
		}

		#region Handles per le voci del ContextMenu

		#region OnViewUserRoleCompany - Visualizza
		/// <summary>
		/// OnViewUserRoleCompany
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnViewUserRoleCompany(object sender, System.EventArgs e)
		{
			try
			{
				workingAreaConsole.Controls.Clear();
				PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
				OnModifyCompanyUserRole(sender, selectedNode.Id, selectedNode.CompanyId, selectedNode.RoleId);
			}
			catch (System.Exception err)
			{
				Debug.Fail(err.Message);
			}
		}
		#endregion

		#region OnDeleteUserRoleCompany - Cancello l'associazione di un Utente a un Ruolo di una Azienda
		/// <summary>
		/// OnDeleteUserRoleCompany
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnDeleteUserRoleCompany(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			MenuItem itemMenu = (MenuItem)sender;
			CustomContextMenu context = (CustomContextMenu)itemMenu.GetContextMenu();
			CompanyRoleLoginDb companyRoleLoginDb = new CompanyRoleLoginDb();
			companyRoleLoginDb.ConnectionString = currentStatus.ConnectionString;
			companyRoleLoginDb.CurrentSqlConnection = currentStatus.CurrentConnection;

			if (!companyRoleLoginDb.Delete(context.LoginId, context.CompanyId, context.RoleId))
			{
				diagnostic.Set(companyRoleLoginDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			}
			else
				consoleTree_onModifyRole(sender, ConstString.containerCompanyRoles, context.CompanyId);
		}
		#endregion
		#endregion
		#endregion

		#region ContextMenuUsers - ContextMenu sul nodo contenitore degli Utenti Applicativi
		/// <summary>
		/// ContextMenuUsers
		/// Context menu che appare sul nodo tree contenitore degli utenti
		/// </summary>
		//---------------------------------------------------------------------
		private void ContextMenuUsers()
		{
			context = new ContextMenu();
			context.MenuItems.Clear();
			context.MenuItems.Add(Strings.NewUser, new System.EventHandler(user_OnNew));
			if (!ExistGuestUser() && IsFunctionalityActivated(DatabaseLayerConsts.WebFramework, DatabaseLayerConsts.EasyLook))
			{
				context.MenuItems.Add("-");
				context.MenuItems.Add(Strings.NewGuestUser, new System.EventHandler(guestUser_OnNew));
			}
		}

		//---------------------------------------------------------------------
		private bool ExistGuestUser()
		{
			UserDb guestUser = new UserDb();
			guestUser.ConnectionString = this.currentStatus.ConnectionString;
			guestUser.CurrentSqlConnection = this.currentStatus.CurrentConnection;
			return guestUser.ExistLogin(NameSolverStrings.GuestLogin);
		}

		#region Handles per le voci del ContextMenu

		#region user_OnNew - Inserisco un nuovo Utente Applicativo
		/// <summary>
		/// user_OnNew
		/// </summary>
		//---------------------------------------------------------------------
		private void user_OnNew(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();

			if (consoleEnvironmentInfo.IsLiteConsole || licenceInfo.IsAzureSQLDatabase)
			{
				UserLite newUserLite = new UserLite(currentStatus, licenceInfo.IsAzureSQLDatabase, pathFinder);
				newUserLite.OnAddGuestUser += new EventHandler(AddGuestUser);
				newUserLite.OnDeleteGuestUser += new EventHandler(DeleteGuestUser);
				newUserLite.OnSendDiagnostic += new UserLite.SendDiagnostic(ReceiveDiagnostic);
				newUserLite.OnModifyTree += new UserLite.ModifyTree(consoleTree_OnModifyUtente);
				newUserLite.OnAfterDisabledCheckedChanged += new UserLite.AfterDisabledCheckedChanged(AfterDisabledLogin);
				newUserLite.OnTraceAction += new UserLite.TraceAction(ExecuteTraceAction);
				newUserLite.TopLevel = false;
				newUserLite.Dock = DockStyle.Fill;
				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, newUserLite.ClientSize.Width, newUserLite.ClientSize.Height);
				workingAreaConsole.Controls.Add(newUserLite);
				newUserLite.Enabled = true;
				newUserLite.Visible = true;
			}
			else
			{
				User newUser = new User(currentStatus, licenceInfo.IsAzureSQLDatabase, pathFinder, brandLoader);
				newUser.OnAddGuestUser += new EventHandler(AddGuestUser);
				newUser.OnDeleteGuestUser += new EventHandler(DeleteGuestUser);
				newUser.OnSendDiagnostic += new User.SendDiagnostic(ReceiveDiagnostic);
				newUser.OnModifyTree += new User.ModifyTree(consoleTree_OnModifyUtente);
				newUser.OnAfterDisabledCheckedChanged += new User.AfterDisabledCheckedChanged(AfterDisabledLogin);
				newUser.OnTraceAction += new User.TraceAction(ExecuteTraceAction);
				newUser.TopLevel = false;
				newUser.Dock = DockStyle.Fill;
				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, newUser.ClientSize.Width, newUser.ClientSize.Height);
				workingAreaConsole.Controls.Add(newUser);
				newUser.Enabled = true;
				newUser.Visible = true;
			}

			OnDisableNewToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableOpenToolBarButton?.Invoke(sender, new EventArgs());
			OnEnableSaveToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableFindSecurityObjectsToolBarButton?.Invoke(sender, e);
			OnDisableShowSecurityIconsToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableApplySecurityFilterToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableOtherObjectsToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableShowAllGrantsToolBarButtonPushed?.Invoke(sender, e);
		}
		#endregion

		#region guestUser_OnNew - Inserisco l'utente Guest
		//---------------------------------------------------------------------
		private void guestUser_OnNew(object sender, System.EventArgs e)
		{
			User guestNewUser =	new User(currentStatus, pathFinder, brandLoader, NameSolverStrings.GuestLogin);
			guestNewUser.OnAddGuestUser += new EventHandler(AddGuestUser);
			guestNewUser.OnDeleteGuestUser += new EventHandler(DeleteGuestUser);
			guestNewUser.OnSendDiagnostic += new User.SendDiagnostic(ReceiveDiagnostic);
			guestNewUser.OnModifyTree += new User.ModifyTree(consoleTree_OnModifyUtente);
			guestNewUser.OnAfterDisabledCheckedChanged += new User.AfterDisabledCheckedChanged(AfterDisabledLogin);
			guestNewUser.Save(sender, e);
		}
		#endregion

		# region AddGuestUser e DeleteGuestUser
		//---------------------------------------------------------------------
		private void AddGuestUser(object sender, System.EventArgs e)
		{
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.Exist = true;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserName = NameSolverStrings.GuestLogin;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserPwd = NameSolverStrings.GuestPwd;
			OnAfterAddGuestUser?.Invoke(NameSolverStrings.GuestLogin, NameSolverStrings.GuestPwd);
		}

		//---------------------------------------------------------------------
		private void DeleteGuestUser(object sender, System.EventArgs e)
		{
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.Exist = false;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserPwd = string.Empty;
			OnAfterDeleteGuestUser?.Invoke(sender, e);
		}
		# endregion

		#endregion

		#endregion

		#region ContextMenuUser - ContextMenu sul nodo di un Utente Applicativo
		/// <summary>
		/// ContextMenuUser
		/// </summary>
		//---------------------------------------------------------------------
		private void ContextMenuUser()
		{
			context = new ContextMenu();
			context.MenuItems.Clear();
			context.MenuItems.Add(Strings.Properties, new System.EventHandler(OnViewUser));
			context.MenuItems.Add("-");
			context.MenuItems.Add(Strings.DeleteUser, new System.EventHandler(OnDeleteUserHandler));
		}

		#region Handles per le voci del ContextMenu

		#region OnViewUser - Visualizza anagrafica utente applicativo
		/// <summary>
		/// OnViewUser
		/// </summary>
		//---------------------------------------------------------------------
		private void OnViewUser(object sender, System.EventArgs e)
		{
			try
			{
				workingAreaConsole.Controls.Clear();
				PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
				this.OnModifyUtente(sender, selectedNode.Id);
			}
			catch (System.Exception err)
			{
				Debug.Fail(err.Message);
			}
		}
		#endregion

		#region OnDeleteUserHandler - Cancella un Utente Applicativo
		/// <summary>
		/// OnDeleteUserHandler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnDeleteUserHandler(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			if (OnDeleteUser != null)
				OnDeleteUser(sender, ((PlugInTreeNode)consoleTree.SelectedNode).Id);
		}
		#endregion

		#endregion

		#endregion

		#region ContextMenuProvider - ContextMenu sul nodo di un Provider
		/// <summary>
		/// ContextMenuProvider
		/// </summary>
		//---------------------------------------------------------------------
		private void ContextMenuProvider()
		{
			context = new ContextMenu();
			context.MenuItems.Clear();
			context.MenuItems.Add(Strings.Properties, new System.EventHandler(OnViewProvider));
		}

		#region Handles per le voci del ContextMenu

		#region OnViewProvider - Visualizza
		/// <summary>
		/// OnViewProvider
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void OnViewProvider(object sender, System.EventArgs e)
		{
			try
			{
				workingAreaConsole.Controls.Clear();
				PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
				this.OnModifyProvider(sender, selectedNode.Id);
			}
			catch (System.Exception err)
			{
				Debug.Fail(err.Message);
			}
		}
		#endregion
		#endregion
		#endregion

		#endregion

		#region Funzioni di Gestione del Tree della MicroareaConsole da parte del SysAdmin

		#region OnAfterKeyDownConsoleTree - Intercetta un tasto premuto sul tree
		/// <summary>
		/// OnAfterKeyDownConsoleTree
		/// Funzione che intercetta un tasto prenuto sul tree
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		public void OnAfterKeyDownConsoleTree(object sender, KeyEventArgs e)
		{
			bottomWorkingAreaConsole.Enabled = false;
			bottomWorkingAreaConsole.Visible = false;
			workingAreaConsole.Visible = true;
			TreeView selectedTree = (TreeView)sender;
			PlugInTreeNode selectedNode = (PlugInTreeNode)(selectedTree.SelectedNode);
			DBMSType providerType = TBDatabaseType.GetDBMSType(selectedNode.Provider);

			if (selectedNode.Type.Length > 0)
			{
				if (e.KeyData == Keys.Delete)
				{
					workingAreaConsole.Controls.Clear();
					switch (selectedNode.Type)
					{
						case ConstString.itemUser:
							OnDeleteUser(selectedNode, selectedNode.Id);
							break;
						case ConstString.itemCompany:
							OnDeleteCompany(selectedNode, selectedNode.Id, providerType);
							break;
						case ConstString.itemRole:
							OnDeleteRole(selectedNode, selectedNode.Id, selectedNode.CompanyId);
							break;
						case ConstString.itemCompanyUser:
							OnDeleteCompanyUser(selectedNode, selectedNode.Id, selectedNode.CompanyId, TBDatabaseType.GetDBMSType(selectedNode.Provider));
							break;
						case ConstString.itemRoleCompanyUser:
							OnDeleteCompanyUserRole(selectedNode, selectedNode.Id, selectedNode.CompanyId, selectedNode.RoleId);
							break;
						default:
							break;
					}
				}
				else if (e.KeyData == Keys.F5)
				{
					OnAfterClickRefreshButton(sender, e);
					if (OnAfterClickF5Button != null)
						OnAfterClickF5Button(sender, e);
				}
			}
		}
		#endregion

		#region OnAfterUpdateTreeContextMenu - scelta della visualizzazione delle liste
		/// <summary>
		/// OnAfterUpdateTreeContextMenu
		/// Intercetto l'event OnUpdateTreeContextMenu sparato dalla console
		/// e aggiunge/toglie i MenuItem dal menu di contesto degli utenti associati all'azienda
		/// </summary>
		//-------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnUpdateTreeContextMenu")]
		public void OnAfterUpdateTreeContextMenu(object sender)
		{
			// Vado ad aggiornare il context menu degli utenti associati all'azienda 
			consoleTree_onModifyAzienda(this, ConstString.containerCompanies);
		}
		#endregion

		#region OnAfterClickRefreshButton - Premuto il bottone di Refresh
		/// <summary>
		/// OnAfterClickRefreshButton
		/// Ho premuto il bottone di Refresh
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnRefreshItem")]
		public void OnAfterClickRefreshButton(object sender, System.EventArgs e)
		{
			consoleTree.Focus();

			PlugInTreeNode selectedNode = (PlugInTreeNode)(this.consoleTree.SelectedNode);
			if (selectedNode.Type.Length > 0 &&
				(((PlugInTreeNode)consoleTree.SelectedNode).AssemblyName == Assembly.GetExecutingAssembly().GetName().Name))
			{
				bottomWorkingAreaConsole.Enabled = false;
				bottomWorkingAreaConsole.Visible = false;
				workingAreaConsole.Visible = true;
				SetProgressBarTextFromPlugIn(sender, Strings.ProgressDuringCheck);
				SetProgressBarValueFromPlugIn(sender, 3);
				SetProgressBarStepFromPlugIn(sender, 3);
				SetProgressBarMaxValueFromPlugIn(sender, 100);
				EnableProgressBarFromPlugIn(sender);
				Application.DoEvents();

				switch (selectedNode.Type)
				{
					case ConstString.itemUser:
					case ConstString.containerUsers:
						workingAreaConsole.Controls.Clear();
						OnUpdateUser(selectedNode);
						break;
					case ConstString.itemCompany:
						workingAreaConsole.Controls.Clear();
						OnUpdateCompany(selectedNode, selectedNode.Id);
						break;
					case ConstString.containerCompanies:
						workingAreaConsole.Controls.Clear();
						OnUpdateCompanies(selectedNode);
						break;
					case ConstString.itemRole:
					case ConstString.containerCompanyRoles:
						workingAreaConsole.Controls.Clear();
						OnUpdateRole(selectedNode, selectedNode.CompanyId);
						break;
					case ConstString.itemCompanyUser:
					case ConstString.containerCompanyUsers:
						workingAreaConsole.Controls.Clear();
						OnUpdateCompanyUser(selectedNode, selectedNode.CompanyId);
						break;
					case ConstString.itemRoleCompanyUser:
						workingAreaConsole.Controls.Clear();
						OnUpdateCompanyUserRole(selectedNode, selectedNode.CompanyId, selectedNode.RoleId);
						break;
					default: break;
				}
				//disabilito la progressbar
				SetProgressBarValueFromPlugIn(sender, 100);
				Application.DoEvents();
				SetProgressBarTextFromPlugIn(sender, string.Empty);
				DisableProgressBarFromPlugIn(sender);
				if (consoleTree.SelectedNode != null)
					OnAfterSelectConsoleTree(sender, new TreeViewEventArgs(this.consoleTree.SelectedNode));
			}
		}

		#region Funzioni per Update dei singoli nodi

		#region role_OnUpdate - Update dei Ruoli
		/// <summary>
		/// role_OnUpdate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void role_OnUpdate(object sender, string companyId)
		{
			PlugInTreeNode containerOfRoles = (PlugInTreeNode)sender;
			if (string.Compare(containerOfRoles.Type, ConstString.itemRole, true, CultureInfo.InvariantCulture) == 0)
				containerOfRoles = ((PlugInTreeNode)sender).Parent;
			containerOfRoles.Nodes.Clear();
			LoadAllRolesOfCompany(companyId, containerOfRoles);
		}
		#endregion

		#region user_OnUpdate - Update degli Utenti Applicativi
		/// <summary>
		/// user_OnUpdate
		/// </summary>
		//---------------------------------------------------------------------
		private void user_OnUpdate(object sender)
		{
			PlugInTreeNode containerOfUsers = (PlugInTreeNode)sender;
			if (string.Compare(containerOfUsers.Type, ConstString.itemUser, true, CultureInfo.InvariantCulture) == 0)
				containerOfUsers = ((PlugInTreeNode)sender).Parent;
			containerOfUsers.Nodes.Clear();
			LoadAllUsers(containerOfUsers);
		}
		#endregion

		#region company_OnUpdateCompanyUser - Update degli Utenti associati a un'Azienda
		/// <summary>
		/// company_OnUpdateCompanyUser
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void company_OnUpdateCompanyUser(object sender, string companyId)
		{
			PlugInTreeNode containerOfCompanyUsers = (PlugInTreeNode)sender;
			if (string.Compare(containerOfCompanyUsers.Type, ConstString.itemCompanyUser, true, CultureInfo.InvariantCulture) == 0)
				containerOfCompanyUsers = ((PlugInTreeNode)sender).Parent;
			containerOfCompanyUsers.Nodes.Clear();
			LoadAllUsersOfCompany(companyId, containerOfCompanyUsers);
		}
		#endregion

		#region company_OnUpdate - Update di un'Azienda
		/// <summary>
		/// company_OnUpdate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void company_OnUpdate(object sender, string companyId)
		{
			PlugInTreeNode containerOfCompany = (PlugInTreeNode)sender;
			containerOfCompany.Nodes.Clear();
			LoadCompany(companyId, containerOfCompany);
		}
		#endregion

		#region companies_OnUpdate - Update delle Aziende
		/// <summary>
		/// companies_OnUpdate
		/// </summary>
		/// <param name="sender"></param>
		//---------------------------------------------------------------------
		private void companies_OnUpdate(object sender)
		{
			PlugInTreeNode containerOfCompanies = (PlugInTreeNode)sender;
			containerOfCompanies.Nodes.Clear();
			LoadAllCompanies(containerOfCompanies);
		}
		#endregion

		#region company_OnUpdateCompanyUserRole - Update di un Utente di un'Azienda associato a un Ruolo
		/// <summary>
		/// company_OnUpdateCompanyUserRole
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		/// <param name="roleId"></param>
		//---------------------------------------------------------------------
		private void company_OnUpdateCompanyUserRole(object sender, string companyId, string roleId)
		{
			PlugInTreeNode containerOfUsersRole = ((PlugInTreeNode)sender).Parent;
			containerOfUsersRole.Nodes.Clear();
			LoadAllUsersRolesOfCompany(companyId, roleId, containerOfUsersRole);
		}
		#endregion
		#endregion
		#endregion

		#region OnAfterDoubleClickConsoleTree - Doppio click sul tree
		/// <summary>
		/// OnAfterDoubleClickConsoleTree
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		public void OnAfterDoubleClickConsoleTree(object sender, System.EventArgs e)
		{
			bottomWorkingAreaConsole.Enabled = false;
			bottomWorkingAreaConsole.Visible = false;
			workingAreaConsole.Visible = true;
			TreeView selectedTree = (TreeView)sender;

			PlugInTreeNode selectedNode = (PlugInTreeNode)(selectedTree.SelectedNode);
			if (selectedNode.Type.Length > 0)
			{
				//abilito la progressBar
				SetProgressBarTextFromPlugIn(sender, Strings.ProgressWaiting);
				SetProgressBarMaxValueFromPlugIn(sender, 100);
				SetProgressBarValueFromPlugIn(sender, 3);
				SetProgressBarStepFromPlugIn(sender, 3);
				EnableProgressBarFromPlugIn(sender);
				Application.DoEvents();

				switch (selectedNode.Type)
				{
					case ConstString.containerProviders:
						if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
						if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
						if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
						if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
						if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
						if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
						if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
						if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
						if (OnDisableShowAllGrantsToolBarButtonPushed != null)
							OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
						break;

					case ConstString.containerCompanies:
						//posso inserire una nuova azienda se non siamo in error
						if ((consoleEnvironmentInfo.ConsoleStatus & StatusType.RemoteServerError) != StatusType.RemoteServerError)
						{
							ContextMenuCompanies();
							selectedNode.ContextMenu = this.context;
							if (OnEnableNewToolBarButton != null) OnEnableNewToolBarButton(sender, e);
						}
						else
						{
							if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
							if (selectedNode.ContextMenu != null && selectedNode.ContextMenu.MenuItems != null)
								selectedNode.ContextMenu.MenuItems.Clear();
						}

						if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
						if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
						if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
						if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
						if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
						if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
						if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
						if (OnDisableShowAllGrantsToolBarButtonPushed != null)
							OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
						break;

					case ConstString.containerUsers:
					case ConstString.containerCompanyRoles:
					case ConstString.containerLoginsUsers:
						if (OnEnableNewToolBarButton != null) OnEnableNewToolBarButton(sender, e);
						if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
						if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
						if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
						if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
						if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
						if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
						if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
						if (OnDisableShowAllGrantsToolBarButtonPushed != null)
							OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
						break;

					case ConstString.containerCompanyUsers:
						if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
						if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
						if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
						if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
						if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
						if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
						if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
						if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
						if (OnDisableShowAllGrantsToolBarButtonPushed != null)
							OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
						break;

					case ConstString.itemCompany:
						workingAreaConsole.Controls.Clear();
						if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
						if (OnEnableOpenToolBarButton != null) OnEnableOpenToolBarButton(sender, e);
						if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
						if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, e);
						if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
						if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
						if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
						if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
						if (OnDisableShowAllGrantsToolBarButtonPushed != null)
							OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
						OnModifyCompany(sender, selectedNode.Id);
						break;

					case ConstString.itemUser:
						workingAreaConsole.Controls.Clear();
						if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
						if (OnEnableOpenToolBarButton != null) OnEnableOpenToolBarButton(sender, e);
						if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
						if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
						if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
						if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
						if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
						if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
						if (OnDisableShowAllGrantsToolBarButtonPushed != null)
							OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
						OnModifyUtente(sender, selectedNode.Id);
						break;

					case ConstString.itemProvider:
						workingAreaConsole.Controls.Clear();
						if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
						if (OnEnableOpenToolBarButton != null) OnEnableOpenToolBarButton(sender, e);
						if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
						if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, e);
						if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
						if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
						if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
						if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
						if (OnDisableShowAllGrantsToolBarButtonPushed != null)
							OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
						OnModifyProvider(sender, selectedNode.Id);
						break;

					case ConstString.itemCompanyUser:
						workingAreaConsole.Controls.Clear();
						if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
						if (OnEnableOpenToolBarButton != null) OnEnableOpenToolBarButton(sender, e);
						if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
						if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
						if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
						if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
						if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
						if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
						if (OnDisableShowAllGrantsToolBarButtonPushed != null)
							OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
						OnModifyCompanyUser(sender, selectedNode.Id, selectedNode.CompanyId);
						break;

					case ConstString.itemRoleCompanyUser:
						workingAreaConsole.Controls.Clear();
						if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
						if (OnEnableOpenToolBarButton != null) OnEnableOpenToolBarButton(sender, e);
						if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
						if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
						if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
						if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
						if (OnDisableShowAllGrantsToolBarButtonPushed != null)
							OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
						if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
						OnModifyCompanyUserRole(sender, selectedNode.Id, selectedNode.CompanyId, selectedNode.RoleId);
						break;

					case ConstString.itemRole:
						workingAreaConsole.Controls.Clear();
                        if (selectedNode.ReadOnly)
                            break;

						if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
						if (OnEnableOpenToolBarButton != null) OnEnableOpenToolBarButton(sender, e);
						if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
						if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
						if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
						if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
						if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
						if (OnDisableShowAllGrantsToolBarButtonPushed != null)
							OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
						if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
						OnModifyRuolo(sender, selectedNode.Id, selectedNode.CompanyId);
						break;
					//@@ LARA PER DEMO
					case ConstString.configParameters:
						workingAreaConsole.Controls.Clear();
						if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
						if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
						if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
						if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
						if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
						if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
						if (OnDisableShowAllGrantsToolBarButtonPushed != null)
							OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
						if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
						EditConfigFile(sender, e);
						break;
				}
				//disabilito la progressBar
				SetProgressBarValueFromPlugIn(sender, 100);
				Application.DoEvents();
				SetProgressBarTextFromPlugIn(sender, string.Empty);
				DisableProgressBarFromPlugIn(sender);
			}
		}

		#region Azioni di Modifica sugli oggetti del SysAdmin

		#region company_OnModify - Modifica di un'Azienda
		/// <summary>
		/// company_OnModify
		/// modifica di una company esistente
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		//---------------------------------------------------------------------
		private void company_OnModify(object sender, string id)
		{
			SetProgressBarMaxValueFromPlugIn(sender, 100);
			workingAreaConsole.Controls.Clear();
			
			if (consoleEnvironmentInfo.IsLiteConsole)
			{
				CompanyLite modCompanyLite = new CompanyLite
				 (
				 currentStatus.OwnerDbName,
				 currentStatus.ConnectionString,
				 currentStatus.CurrentConnection,
				 id,
				 pathFinder,
				 licenceInfo
				 );
				modCompanyLite.UserConnected = currentStatus.User;
				modCompanyLite.UserPwdConnected = currentStatus.Password;
				modCompanyLite.DataSourceSysAdmin = currentStatus.DataSource;
				modCompanyLite.TreeConsole = this.consoleTree;

				modCompanyLite.OnCallHelp += new CompanyLite.CallHelp(HelpFromPopUp);
				modCompanyLite.OnModifyTree += new CompanyLite.ModifyTree(consoleTree_onModifyAzienda);
				modCompanyLite.OnModifyTreeOfCompanies += new CompanyLite.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
				modCompanyLite.OnAfterChangedAuditing += new CompanyLite.AfterChangedAuditing(AfterChangedAuditingCompany);
				modCompanyLite.OnAfterChangedOSLSecurity += new CompanyLite.AfterChangedOSLSecurity(AfterChangedOSLSecurityCompany);
				modCompanyLite.OnAfterModifyCompany += new CompanyLite.AfterModifyCompany(AfterModifyCompany);
				modCompanyLite.OnAfterDeleteCompany += new CompanyLite.AfterDeleteCompany(AfterDeleteCompany);
				modCompanyLite.OnAfterChangedCompanyDisable += new CompanyLite.AfterChangedCompanyDisable(AfterChangedCompanyDisabled);
				modCompanyLite.OnCreateDBStructure += new CompanyLite.CreateDBStructure(OnCreateCompanyDBStructure);
				modCompanyLite.OnCheckDBRequirementsUsed += new CompanyLite.CheckDBRequirementsUsed(OnCheckRequirements);
				modCompanyLite.OnEnableProgressBar += new CompanyLite.EnableProgressBar(EnableProgressBarFromPlugIn);
				modCompanyLite.OnDisableProgressBar += new CompanyLite.DisableProgressBar(DisableProgressBarFromPlugIn);
				modCompanyLite.OnSetProgressBarValue += new CompanyLite.SetProgressBarValue(SetProgressBarValueFromPlugIn);
				modCompanyLite.OnSetProgressBarStep += new CompanyLite.SetProgressBarStep(SetProgressBarStepFromPlugIn);
				modCompanyLite.OnSetProgressBarText += new CompanyLite.SetProgressBarText(SetProgressBarTextFromPlugIn);
				modCompanyLite.OnEnableSaveButton += new EventHandler(OnEnableSaveToolBarButton);
				modCompanyLite.OnGetUserAuthenticatedPwdFromConsole += new CompanyLite.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				modCompanyLite.OnIsUserAuthenticatedFromConsole += new CompanyLite.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				modCompanyLite.OnAddUserAuthenticatedFromConsole += new CompanyLite.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				modCompanyLite.OnBeforeDeleteCompany += new CompanyLite.BeforeDeleteCompany(OnDeleteCompanyToLoginManager);
				modCompanyLite.OnIsActivated += new CompanyLite.IsActivated(IsFunctionalityActivated);

				if (!modCompanyLite.Diagnostic.Error)
				{
					modCompanyLite.TopLevel = false;
					modCompanyLite.Dock = DockStyle.Fill;
					//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
					OnBeforeAddFormFromPlugIn(sender, modCompanyLite.ClientSize.Width, modCompanyLite.ClientSize.Height);
					workingAreaConsole.Controls.Add(modCompanyLite);
					modCompanyLite.Show();
					if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
					if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
					if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
					if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, new System.EventArgs());
					if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, new System.EventArgs());
					if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
					if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
					if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, new System.EventArgs());
					if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
				}
			}
			else
			{
			Company modCompany = new Company
				(
				currentStatus.OwnerDbName,
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				id,
				pathFinder,
				licenceInfo
				);
			modCompany.UserConnected = currentStatus.User;
			modCompany.UserPwdConnected = currentStatus.Password;
			modCompany.DataSourceSysAdmin = currentStatus.DataSource;
			modCompany.TreeConsole = this.consoleTree;

			modCompany.OnCallHelp += new Company.CallHelp(HelpFromPopUp);
			modCompany.OnModifyTree += new Company.ModifyTree(consoleTree_onModifyAzienda);
			modCompany.OnModifyTreeOfCompanies += new Company.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
			modCompany.OnAfterChangedAuditing += new Company.AfterChangedAuditing(AfterChangedAuditingCompany);
			modCompany.OnAfterChangedOSLSecurity += new Company.AfterChangedOSLSecurity(AfterChangedOSLSecurityCompany);
			modCompany.OnAfterModifyCompany += new Company.AfterModifyCompany(AfterModifyCompany);
			modCompany.OnAfterDeleteCompany += new Company.AfterDeleteCompany(AfterDeleteCompany);
			modCompany.OnAfterChangedCompanyDisable += new Company.AfterChangedCompanyDisable(AfterChangedCompanyDisabled);
			modCompany.OnCreateDBStructure += new Company.CreateDBStructure(OnCreateCompanyDBStructure);
			modCompany.OnCheckDBRequirementsUsed += new Company.CheckDBRequirementsUsed(OnCheckRequirements);
			modCompany.OnEnableProgressBar += new Company.EnableProgressBar(EnableProgressBarFromPlugIn);
			modCompany.OnDisableProgressBar += new Company.DisableProgressBar(DisableProgressBarFromPlugIn);
			modCompany.OnSetProgressBarValue += new Company.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			modCompany.OnSetProgressBarStep += new Company.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			modCompany.OnSetProgressBarText += new Company.SetProgressBarText(SetProgressBarTextFromPlugIn);
			modCompany.OnDisableSaveButton += new EventHandler(OnDisableSaveToolBarButton);
			modCompany.OnEnableSaveButton += new EventHandler(OnEnableSaveToolBarButton);
			modCompany.OnGetUserAuthenticatedPwdFromConsole += new Company.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			modCompany.OnIsUserAuthenticatedFromConsole += new Company.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			modCompany.OnAddUserAuthenticatedFromConsole += new Company.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			modCompany.OnBeforeDeleteCompany += new Company.BeforeDeleteCompany(OnDeleteCompanyToLoginManager);
			modCompany.OnIsActivated += new Company.IsActivated(IsFunctionalityActivated);
			
			if (!modCompany.Diagnostic.Error)
			{
				modCompany.TopLevel = false;
				modCompany.Dock = DockStyle.Fill;
				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, modCompany.ClientSize.Width, modCompany.ClientSize.Height);
				workingAreaConsole.Controls.Add(modCompany);
				modCompany.Show();
				if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
				if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
				if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
				if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, new System.EventArgs());
				if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, new System.EventArgs());
				if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
				if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
					if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, new System.EventArgs());
				if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
			}
		}
		}
		#endregion

		#region user_OnModify - Modifica di un Utente Applicativo
		/// <summary>
		/// user_OnModify
		/// modifica di un utente esistente
		/// </summary>
		//---------------------------------------------------------------------
		private void user_OnModify(object sender, string id)
		{
			workingAreaConsole.Controls.Clear();

			if (consoleEnvironmentInfo.IsLiteConsole || licenceInfo.IsAzureSQLDatabase)
			{
				UserLite modUserLite = new UserLite(currentStatus, licenceInfo.IsAzureSQLDatabase, id, pathFinder);
				modUserLite.OnAddGuestUser += new EventHandler(AddGuestUser);
				modUserLite.OnDeleteGuestUser += new EventHandler(DeleteGuestUser);
				modUserLite.OnSendDiagnostic += new UserLite.SendDiagnostic(ReceiveDiagnostic);
				modUserLite.OnModifyTree += new UserLite.ModifyTree(consoleTree_OnModifyUtente);
				modUserLite.OnAfterDisabledCheckedChanged += new UserLite.AfterDisabledCheckedChanged(AfterDisabledLogin);
				modUserLite.OnTraceAction += new UserLite.TraceAction(ExecuteTraceAction);
				if (!modUserLite.Diagnostic.Error)
				{
					modUserLite.TopLevel = false;
					modUserLite.Dock = DockStyle.Fill;
					//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
					OnBeforeAddFormFromPlugIn(sender, modUserLite.ClientSize.Width, modUserLite.ClientSize.Height);
					workingAreaConsole.Controls.Add(modUserLite);
					modUserLite.Show();
				}
			}
			else
			{
				User modUser = new User(currentStatus, licenceInfo.IsAzureSQLDatabase, id, pathFinder, brandLoader);
				modUser.OnAddGuestUser += new EventHandler(AddGuestUser);
				modUser.OnDeleteGuestUser += new EventHandler(DeleteGuestUser);
				modUser.OnSendDiagnostic += new User.SendDiagnostic(ReceiveDiagnostic);
				modUser.OnModifyTree += new User.ModifyTree(consoleTree_OnModifyUtente);
				modUser.OnAfterDisabledCheckedChanged += new User.AfterDisabledCheckedChanged(AfterDisabledLogin);
				modUser.OnTraceAction += new User.TraceAction(ExecuteTraceAction);
				if (!modUser.Diagnostic.Error)
				{
					modUser.TopLevel = false;
					modUser.Dock = DockStyle.Fill;
					//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
					OnBeforeAddFormFromPlugIn(sender, modUser.ClientSize.Width, modUser.ClientSize.Height);
					workingAreaConsole.Controls.Add(modUser);
					modUser.Show();
				}
			}

			OnDisableNewToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableOpenToolBarButton?.Invoke(sender, new EventArgs());
			OnEnableSaveToolBarButton?.Invoke(sender, new EventArgs());
			OnEnableDeleteToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableFindSecurityObjectsToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableShowSecurityIconsToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableApplySecurityFilterToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableShowAllGrantsToolBarButtonPushed?.Invoke(sender, new EventArgs());
			OnDisableOtherObjectsToolBarButton?.Invoke(sender, new EventArgs());
		}
		#endregion

		#region provider_OnModify - Modifica di un Provider
		/// <summary>
		/// provider_OnModify
		/// modifica di un provider esistente
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		//---------------------------------------------------------------------
		private void provider_OnModify(object sender, string id)
		{
			workingAreaConsole.Controls.Clear();
			Provider modProvider = new Provider(currentStatus.ConnectionString, currentStatus.CurrentConnection, id);//, licenceInfo.DBNetworkType);
			modProvider.OnModifyTree += new Provider.ModifyTree(consoleTree_OnModifyProvider);
			modProvider.TopLevel = false;
			modProvider.Dock = DockStyle.Fill;

			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, modProvider.ClientSize.Width, modProvider.ClientSize.Height);
			workingAreaConsole.Controls.Add(modProvider);
			modProvider.Show();

			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null)
				OnDisableShowAllGrantsToolBarButtonPushed(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
		}
		#endregion

		#region company_OnModifyCompanyUser [e listViewDetail_OnModifyCompanyUser] - Modifica di un Utente associato a un'Azienda
		/// <summary>
		/// company_OnModifyCompanyUser
		/// modifica di un utente associato a una company
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		/// <param name="companyId"></param>
		//----------------------------------------------------------------------
		private void company_OnModifyCompanyUser(object sender, string id, string companyId)
		{
			workingAreaConsole.Controls.Clear();
			string userName = ((PlugInTreeNode)consoleTree.SelectedNode).Text;
			DBMSType providerUser = TBDatabaseType.GetDBMSType(((PlugInTreeNode)consoleTree.SelectedNode).Provider);

			if (providerUser == DBMSType.SQLSERVER)
			{
				if (!consoleEnvironmentInfo.IsLiteConsole)
				{
				DetailCompanyUser detailCompanyUser = new DetailCompanyUser(currentStatus.ConnectionString, currentStatus.CurrentConnection, companyId, id, userName);
				detailCompanyUser.OnCallHelpFromPopUp += new DetailCompanyUser.CallHelpFromPopUp(HelpFromPopUp);
				detailCompanyUser.OnSendDiagnostic += new DetailCompanyUser.SendDiagnostic(ReceiveDiagnostic);
				detailCompanyUser.OnAfterChangeDisabledCheckBox += new DetailCompanyUser.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
				detailCompanyUser.OnModifyTreeOfCompanies += new DetailCompanyUser.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
				detailCompanyUser.OnAddUserAuthenticatedFromConsole += new DetailCompanyUser.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				detailCompanyUser.OnGetUserAuthenticatedPwdFromConsole += new DetailCompanyUser.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				detailCompanyUser.OnIsUserAuthenticatedFromConsole += new DetailCompanyUser.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				detailCompanyUser.OnIsActivated += new DetailCompanyUser.IsActivated(IsFunctionalityActivated);

				detailCompanyUser.TopLevel = false;
				detailCompanyUser.Dock = DockStyle.Fill;
				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, detailCompanyUser.ClientSize.Width, detailCompanyUser.ClientSize.Height);
				workingAreaConsole.Controls.Add(detailCompanyUser);
				detailCompanyUser.Show();
			}
				else
				{
					DetailCompanyUserLite detailCompanyUserLite = new DetailCompanyUserLite(currentStatus.ConnectionString, currentStatus.CurrentConnection, companyId, id, userName);
					detailCompanyUserLite.OnCallHelpFromPopUp += new DetailCompanyUserLite.CallHelpFromPopUp(HelpFromPopUp);
					detailCompanyUserLite.OnSendDiagnostic += new DetailCompanyUserLite.SendDiagnostic(ReceiveDiagnostic);
					detailCompanyUserLite.OnAfterChangeDisabledCheckBox += new DetailCompanyUserLite.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
					detailCompanyUserLite.OnModifyTreeOfCompanies += new DetailCompanyUserLite.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
					detailCompanyUserLite.OnAddUserAuthenticatedFromConsole += new DetailCompanyUserLite.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
					detailCompanyUserLite.OnGetUserAuthenticatedPwdFromConsole += new DetailCompanyUserLite.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
					detailCompanyUserLite.OnIsUserAuthenticatedFromConsole += new DetailCompanyUserLite.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
					detailCompanyUserLite.OnIsActivated += new DetailCompanyUserLite.IsActivated(IsFunctionalityActivated);
					detailCompanyUserLite.TopLevel = false;
					detailCompanyUserLite.Dock = DockStyle.Fill;
					//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
					OnBeforeAddFormFromPlugIn(sender, detailCompanyUserLite.ClientSize.Width, detailCompanyUserLite.ClientSize.Height);
					workingAreaConsole.Controls.Add(detailCompanyUserLite);
					detailCompanyUserLite.Show();
				}
			}
			else if (providerUser == DBMSType.ORACLE)
			{
				//disabilito la progress bar
				SetProgressBarValueFromPlugIn(sender, 100);
				SetProgressBarTextFromPlugIn(sender, string.Empty);
				DisableProgressBarFromPlugIn(sender);
				DetailOracleCompanyUser detailOracleCompanyUser = new DetailOracleCompanyUser(currentStatus.ConnectionString, currentStatus.CurrentConnection, companyId, id, userName);
				detailOracleCompanyUser.OnCallHelpFromPopUp += new DetailOracleCompanyUser.CallHelpFromPopUp(HelpFromPopUp);
				detailOracleCompanyUser.OnSendDiagnostic += new DetailOracleCompanyUser.SendDiagnostic(ReceiveDiagnostic);
				detailOracleCompanyUser.OnAfterChangeDisabledCheckBox += new DetailOracleCompanyUser.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
				detailOracleCompanyUser.OnModifyTreeOfCompanies += new DetailOracleCompanyUser.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
				detailOracleCompanyUser.OnAddUserAuthenticatedFromConsole += new DetailOracleCompanyUser.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				detailOracleCompanyUser.OnGetUserAuthenticatedPwdFromConsole += new DetailOracleCompanyUser.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				detailOracleCompanyUser.OnIsUserAuthenticatedFromConsole += new DetailOracleCompanyUser.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				detailOracleCompanyUser.OnIsActivated += new DetailOracleCompanyUser.IsActivated(IsFunctionalityActivated);
				detailOracleCompanyUser.OnEnableProgressBar += new DetailOracleCompanyUser.EnableProgressBar(EnableProgressBarFromPlugIn);
				detailOracleCompanyUser.OnDisableProgressBar += new DetailOracleCompanyUser.DisableProgressBar(DisableProgressBarFromPlugIn);
				detailOracleCompanyUser.OnSetProgressBarStep += new DetailOracleCompanyUser.SetProgressBarStep(SetProgressBarStepFromPlugIn);
				detailOracleCompanyUser.OnSetProgressBarText += new DetailOracleCompanyUser.SetProgressBarText(SetProgressBarTextFromPlugIn);
				detailOracleCompanyUser.OnSetProgressBarValue += new DetailOracleCompanyUser.SetProgressBarValue(SetProgressBarValueFromPlugIn);
				detailOracleCompanyUser.TopLevel = false;
				detailOracleCompanyUser.Dock = DockStyle.Fill;
				OnBeforeAddFormFromPlugIn(sender, detailOracleCompanyUser.ClientSize.Width, detailOracleCompanyUser.ClientSize.Height);
				workingAreaConsole.Controls.Add(detailOracleCompanyUser);
				detailOracleCompanyUser.Show();
			}
            else if (providerUser == DBMSType.POSTGRE)
            {
                DetailCompanyUserPostgre detailCompanyUser = new DetailCompanyUserPostgre(currentStatus.ConnectionString, currentStatus.CurrentConnection, companyId, id, userName);
                detailCompanyUser.OnCallHelpFromPopUp += new DetailCompanyUserPostgre.CallHelpFromPopUp(HelpFromPopUp);
                detailCompanyUser.OnSendDiagnostic += new DetailCompanyUserPostgre.SendDiagnostic(ReceiveDiagnostic);
                detailCompanyUser.OnAfterChangeDisabledCheckBox += new DetailCompanyUserPostgre.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
                detailCompanyUser.OnModifyTreeOfCompanies += new DetailCompanyUserPostgre.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
                detailCompanyUser.OnAddUserAuthenticatedFromConsole += new DetailCompanyUserPostgre.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
                detailCompanyUser.OnGetUserAuthenticatedPwdFromConsole += new DetailCompanyUserPostgre.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
                detailCompanyUser.OnIsUserAuthenticatedFromConsole += new DetailCompanyUserPostgre.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
                detailCompanyUser.OnIsActivated += new DetailCompanyUserPostgre.IsActivated(IsFunctionalityActivated);
                detailCompanyUser.TopLevel = false;
                detailCompanyUser.Dock = DockStyle.Fill;
                //eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
                OnBeforeAddFormFromPlugIn(sender, detailCompanyUser.ClientSize.Width, detailCompanyUser.ClientSize.Height);
                workingAreaConsole.Controls.Add(detailCompanyUser);
                detailCompanyUser.Show();
            }

			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
		}

		///<summary>
		/// listViewDetail_OnModifyCompanyUser
		/// Per visualizzare la form di dettaglio dell'utente associato all'azienda
		/// Viene chiamato solo dalla ListView presente nella workingArea della Console
		///</summary>
		//----------------------------------------------------------------------
		private void listViewDetail_OnModifyCompanyUser(object sender, CompanyUser cUser)
		{
			workingAreaConsole.Controls.Clear();
			DBMSType providerUser = TBDatabaseType.GetDBMSType(((PlugInTreeNode)consoleTree.SelectedNode.Parent).Provider);

			if (providerUser == DBMSType.SQLSERVER)
			{
				if (!consoleEnvironmentInfo.IsLiteConsole)
				{
				DetailCompanyUser detailCompanyUser = new DetailCompanyUser(currentStatus.ConnectionString, currentStatus.CurrentConnection, cUser.CompanyId, cUser.LoginId, cUser.Login);
				detailCompanyUser.OnCallHelpFromPopUp += new DetailCompanyUser.CallHelpFromPopUp(HelpFromPopUp);
				detailCompanyUser.OnSendDiagnostic += new DetailCompanyUser.SendDiagnostic(ReceiveDiagnostic);
				detailCompanyUser.OnAfterChangeDisabledCheckBox += new DetailCompanyUser.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
				detailCompanyUser.OnModifyTreeOfCompanies += new DetailCompanyUser.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
				detailCompanyUser.OnAddUserAuthenticatedFromConsole += new DetailCompanyUser.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				detailCompanyUser.OnGetUserAuthenticatedPwdFromConsole += new DetailCompanyUser.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				detailCompanyUser.OnIsUserAuthenticatedFromConsole += new DetailCompanyUser.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				detailCompanyUser.OnIsActivated += new DetailCompanyUser.IsActivated(IsFunctionalityActivated);
				detailCompanyUser.TopLevel = false;
				detailCompanyUser.Dock = DockStyle.Fill;
				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, detailCompanyUser.ClientSize.Width, detailCompanyUser.ClientSize.Height);
				workingAreaConsole.Controls.Add(detailCompanyUser);
				detailCompanyUser.Show();
			}
				else
				{
					DetailCompanyUserLite detailCompanyUserLite = new DetailCompanyUserLite(currentStatus.ConnectionString, currentStatus.CurrentConnection, cUser.CompanyId, cUser.LoginId, cUser.Login);
					detailCompanyUserLite.OnCallHelpFromPopUp += new DetailCompanyUserLite.CallHelpFromPopUp(HelpFromPopUp);
					detailCompanyUserLite.OnSendDiagnostic += new DetailCompanyUserLite.SendDiagnostic(ReceiveDiagnostic);
					detailCompanyUserLite.OnAfterChangeDisabledCheckBox += new DetailCompanyUserLite.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
					detailCompanyUserLite.OnModifyTreeOfCompanies += new DetailCompanyUserLite.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
					detailCompanyUserLite.OnAddUserAuthenticatedFromConsole += new DetailCompanyUserLite.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
					detailCompanyUserLite.OnGetUserAuthenticatedPwdFromConsole += new DetailCompanyUserLite.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
					detailCompanyUserLite.OnIsUserAuthenticatedFromConsole += new DetailCompanyUserLite.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
					detailCompanyUserLite.OnIsActivated += new DetailCompanyUserLite.IsActivated(IsFunctionalityActivated);
					detailCompanyUserLite.TopLevel = false;
					detailCompanyUserLite.Dock = DockStyle.Fill;
					//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
					OnBeforeAddFormFromPlugIn(sender, detailCompanyUserLite.ClientSize.Width, detailCompanyUserLite.ClientSize.Height);
					workingAreaConsole.Controls.Add(detailCompanyUserLite);
					detailCompanyUserLite.Show();
				}
			}
			else if (providerUser == DBMSType.ORACLE)
			{
				//disabilito la progress bar
				SetProgressBarValueFromPlugIn(sender, 100);
				SetProgressBarTextFromPlugIn(sender, string.Empty);
				DisableProgressBarFromPlugIn(sender);
				DetailOracleCompanyUser detailOracleCompanyUser = new DetailOracleCompanyUser(currentStatus.ConnectionString, currentStatus.CurrentConnection, cUser.CompanyId, cUser.LoginId, cUser.Login);
				detailOracleCompanyUser.OnCallHelpFromPopUp += new DetailOracleCompanyUser.CallHelpFromPopUp(HelpFromPopUp);
				detailOracleCompanyUser.OnSendDiagnostic += new DetailOracleCompanyUser.SendDiagnostic(ReceiveDiagnostic);
				detailOracleCompanyUser.OnAfterChangeDisabledCheckBox += new DetailOracleCompanyUser.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
				detailOracleCompanyUser.OnModifyTreeOfCompanies += new DetailOracleCompanyUser.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
				detailOracleCompanyUser.OnAddUserAuthenticatedFromConsole += new DetailOracleCompanyUser.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				detailOracleCompanyUser.OnGetUserAuthenticatedPwdFromConsole += new DetailOracleCompanyUser.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				detailOracleCompanyUser.OnIsUserAuthenticatedFromConsole += new DetailOracleCompanyUser.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				detailOracleCompanyUser.OnIsActivated += new DetailOracleCompanyUser.IsActivated(IsFunctionalityActivated);
				detailOracleCompanyUser.OnEnableProgressBar += new DetailOracleCompanyUser.EnableProgressBar(EnableProgressBarFromPlugIn);
				detailOracleCompanyUser.OnDisableProgressBar += new DetailOracleCompanyUser.DisableProgressBar(DisableProgressBarFromPlugIn);
				detailOracleCompanyUser.OnSetProgressBarStep += new DetailOracleCompanyUser.SetProgressBarStep(SetProgressBarStepFromPlugIn);
				detailOracleCompanyUser.OnSetProgressBarText += new DetailOracleCompanyUser.SetProgressBarText(SetProgressBarTextFromPlugIn);
				detailOracleCompanyUser.OnSetProgressBarValue += new DetailOracleCompanyUser.SetProgressBarValue(SetProgressBarValueFromPlugIn);
				detailOracleCompanyUser.TopLevel = false;
				detailOracleCompanyUser.Dock = DockStyle.Fill;
				OnBeforeAddFormFromPlugIn(sender, detailOracleCompanyUser.ClientSize.Width, detailOracleCompanyUser.ClientSize.Height);
				workingAreaConsole.Controls.Add(detailOracleCompanyUser);
				detailOracleCompanyUser.Show();
			}
            else if (providerUser == DBMSType.POSTGRE)
            {
                DetailCompanyUserPostgre detailCompanyUser = new DetailCompanyUserPostgre(currentStatus.ConnectionString, currentStatus.CurrentConnection, cUser.CompanyId, cUser.LoginId, cUser.Login);
                detailCompanyUser.OnCallHelpFromPopUp += new DetailCompanyUserPostgre.CallHelpFromPopUp(HelpFromPopUp);
                detailCompanyUser.OnSendDiagnostic += new DetailCompanyUserPostgre.SendDiagnostic(ReceiveDiagnostic);
                detailCompanyUser.OnAfterChangeDisabledCheckBox += new DetailCompanyUserPostgre.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
                detailCompanyUser.OnModifyTreeOfCompanies += new DetailCompanyUserPostgre.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
                detailCompanyUser.OnAddUserAuthenticatedFromConsole += new DetailCompanyUserPostgre.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
                detailCompanyUser.OnGetUserAuthenticatedPwdFromConsole += new DetailCompanyUserPostgre.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
                detailCompanyUser.OnIsUserAuthenticatedFromConsole += new DetailCompanyUserPostgre.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
                detailCompanyUser.OnIsActivated += new DetailCompanyUserPostgre.IsActivated(IsFunctionalityActivated);
                detailCompanyUser.TopLevel = false;
                detailCompanyUser.Dock = DockStyle.Fill;
                //eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
                OnBeforeAddFormFromPlugIn(sender, detailCompanyUser.ClientSize.Width, detailCompanyUser.ClientSize.Height);
                workingAreaConsole.Controls.Add(detailCompanyUser);
                detailCompanyUser.Show();
            }

			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
		}
		#endregion

		#region role_OnModify - Modifica di un Ruolo di una Azienda
		/// <summary>
		/// role_OnModify
		/// Modifica di un ruolo esistente
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void role_OnModify(object sender, string id, string companyId)
		{
			workingAreaConsole.Controls.Clear();
			Role modRole = new Role(currentStatus.ConnectionString, currentStatus.CurrentConnection, id, companyId);
			modRole.OnSendDiagnostic += new Role.SendDiagnostic(ReceiveDiagnostic);
			modRole.OnModifyTreeOfCompanies += new Role.ModifyTreeOfCompanies(consoleTree_onModifyRole);
			modRole.OnAfterChangedDisabled += new Role.AfterChangedDisabled(AfterDisabledRole);
			modRole.TopLevel = false;
			modRole.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, modRole.ClientSize.Width, modRole.ClientSize.Height);
			workingAreaConsole.Controls.Add(modRole);
			modRole.Show();
			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null)
				OnDisableShowAllGrantsToolBarButtonPushed(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
		}
		#endregion
		#endregion

		#endregion

		#region OnAfterSelectConsoleTree - Ho selezionato un nodo sul tree
		/// <summary>
		/// OnAfterSelectConsoleTree
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		public void OnAfterSelectConsoleTree(object sender, TreeViewEventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)e.Node;
			if (selectedNode == null)
				return;

			if (string.Compare(selectedNode.AssemblyName, Assembly.GetExecutingAssembly().GetName().Name, true, CultureInfo.InvariantCulture) == 0)
			{
				bottomWorkingAreaConsole.Enabled = false;
				bottomWorkingAreaConsole.Visible = false;
				workingAreaConsole.Visible = true;

				if (workingAreaConsole != null &&
					selectedNode.Type.Length > 0 &&
					string.Compare(selectedNode.Type, ConstString.SysAdminPlugInRoot, true, CultureInfo.InvariantCulture) != 0 &&
					string.Compare(selectedNode.Type, ConstString.configParameters, true, CultureInfo.InvariantCulture) != 0)
				{
					switch (selectedNode.Type)
					{
						case ConstString.containerProviders:
							{
								if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
								if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
								if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
								if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
								if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
								if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
								if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
								if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
								if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
								break;
							}
						case ConstString.containerCompanies:
							{
								//posso inserire una nuova azienda se non siamo in Error
								if (((consoleEnvironmentInfo.ConsoleStatus & StatusType.RemoteServerError) != StatusType.RemoteServerError) )
								{
									ContextMenuCompanies();
									selectedNode.ContextMenu = this.context;
									if (OnEnableNewToolBarButton != null) OnEnableNewToolBarButton(sender, e);
								}
								else
								{
									if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
									if (selectedNode.ContextMenu != null && selectedNode.ContextMenu.MenuItems != null)
										selectedNode.ContextMenu.MenuItems.Clear();
								}

								if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
								if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
								if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
								if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
								if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
								if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
								if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
								if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
								break;
							}
						case ConstString.containerCompanyRoles:
							{
								if (OnEnableNewToolBarButton != null) OnEnableNewToolBarButton(sender, e);
								if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
								if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
								if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
								if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
								if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
								if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
								if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
								if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
								break;
							}
						case ConstString.containerUsers:
							{
								ContextMenuUsers();
								selectedNode.ContextMenu = this.context;
								if (OnEnableNewToolBarButton != null) OnEnableNewToolBarButton(sender, e);
								if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
								if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
								if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
								if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
								if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
								if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
								if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
								if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
								break;
							}
						case ConstString.containerCompanyUsers:
							{
								if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
								if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
								if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
								if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
								if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
								if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
								if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
								if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
								if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
								break;
							}
						case ConstString.itemCompany:
							{
								workingAreaConsole.Controls.Clear();
								if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
								if (OnEnableOpenToolBarButton != null) OnEnableOpenToolBarButton(sender, e);
								if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
								if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, e);
								if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
								if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
								if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
								if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
								if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
								break;
							}
						case ConstString.itemRole:
						case ConstString.itemUser:
						case ConstString.itemCompanyUser:
							{
								workingAreaConsole.Controls.Clear();
								if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
								if (OnEnableOpenToolBarButton != null) OnEnableOpenToolBarButton(sender, e);
								if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
								if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, e);
								if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
								if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
								if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
								if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
								if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
								break;
							}
						case ConstString.itemRoleCompanyUser:
							{
								workingAreaConsole.Controls.Clear();
								if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
								if (OnEnableOpenToolBarButton != null) OnEnableOpenToolBarButton(sender, e);
								if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
								if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
								if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
								if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
								if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
								if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
								if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
								break;
							}
						case ConstString.itemProvider:
							{
								workingAreaConsole.Controls.Clear();
								if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
								if (OnEnableOpenToolBarButton != null) OnEnableOpenToolBarButton(sender, e);
								if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
								if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, e);
								if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
								if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
								if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
								if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
								if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
								if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
								break;
							}
					}

					workingAreaConsole.Controls.Clear();
					ListViewDetail listViewDetail = new ListViewDetail
						(
						currentStatus.TypeOfView,
						consoleTree.ImageList,
						string.Compare(this.licenceInfo.Edition, NameSolverStrings.StandardEdition, true, CultureInfo.InvariantCulture) == 0
						);

					listViewDetail.OnSendDiagnostic += new ListViewDetail.SendDiagnostic(ReceiveDiagnostic);
					listViewDetail.OnModifyProvider += new ListViewDetail.ModifyProvider(provider_OnModify);
					listViewDetail.OnModifyUser += new ListViewDetail.ModifyUser(user_OnModify);
					listViewDetail.OnModifyCompanyUser += new ListViewDetail.ModifyCompanyUser(company_OnModifyCompanyUser);
					listViewDetail.OnModifyContainerCompanyUser += new ListViewDetail.ModifyContainerCompanyUser(listViewDetail_OnModifyCompanyUser);
					listViewDetail.OnModifyRole += new ListViewDetail.ModifyRole(role_OnModify);
					listViewDetail.OnModifyCompanyUserRole += new ListViewDetail.ModifyCompanyUserRole(company_OnModifyCompanyUserRole);
					listViewDetail.OnModifyCompany += new ListViewDetail.ModifyCompany(company_OnModify);
					listViewDetail.OnIsActivated += new ListViewDetail.IsActivated(IsFunctionalityActivated);

					listViewDetail.ObjectId = selectedNode.Id;
					listViewDetail.RoleId = selectedNode.RoleId;
					listViewDetail.ParentId = selectedNode.CompanyId;
					listViewDetail.ConnectionString = currentStatus.ConnectionString;
					listViewDetail.CurrentConnection = currentStatus.CurrentConnection;
					listViewDetail.TopLevel = false;
					listViewDetail.Dock = DockStyle.Fill;
					listViewDetail.SettingListView(selectedNode.Type, consoleTree.ImageList);

					//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
					OnBeforeAddFormFromPlugIn(sender, listViewDetail.ClientSize.Width, listViewDetail.ClientSize.Height);
					workingAreaConsole.Controls.Add(listViewDetail);
					listViewDetail.Show();
				}
				else if (string.Compare(selectedNode.Type, ConstString.SysAdminPlugInRoot, true, CultureInfo.InvariantCulture) == 0)
				{
					ViewInfoSysAdminConnection(sender, e);
					if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
					if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
					if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
					if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
					if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
					if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
					if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
					if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
					if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
				}
				else if (string.Compare(selectedNode.Type, ConstString.configParameters, true, CultureInfo.InvariantCulture) == 0)
				{
					EditConfigFile(sender, e);
					if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
					if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
					if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
					if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
					if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
					if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
					if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
					if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
				}
				else
				{
					if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
					if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, e);
					if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
					if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
					if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
					if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
					if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
					if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
					if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
				}
			}
		}
		#endregion

		#endregion

		#region Eventi del SysAdmin verso altri PlugIns e/o Microarea Console

		#region cloneCompany_OnAfterClonedCompany - Intercettato dal SecurityAdmin
		/// <summary>
		/// cloneCompany_OnAfterClonedCompany
		/// </summary>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void cloneCompany_OnAfterClonedCompany(string companyId)
		{
			if (OnAfterClonedCompany != null)
				OnAfterClonedCompany(companyId);
		}
		#endregion

		#region cloneUserCompany_OnAfterClonedUserCompany - Intercettato del SecurityAdmin
		/// <summary>
		/// cloneUserCompany_OnAfterClonedUserCompany
		/// Lancio evento ad altri plugIns dopo la clonazione della company
		/// </summary>
		/// <param name="companyId"></param>
		//--------------------------------------------------------------------
		private void cloneUserCompany_OnAfterClonedUserCompany(string companyId)
		{
			if (OnAfterClonedUserCompany != null)
				OnAfterClonedUserCompany(companyId);
		}
		#endregion

		#region cloneRole_OnAfterClonedCompany - Intercettato dal SecurityAdmin
		/// <summary>
		/// cloneRole_OnAfterClonedCompany
		/// Lancio evento di clonazione di un ruolo ad altri PlugIns
		/// </summary>
		/// <param name="companyId"></param>
		//-----------------------------------------------------------------------
		private void cloneRole_OnAfterClonedCompany(string companyId)
		{
			if (OnAfterClonedRole != null)
				OnAfterClonedRole(companyId);
		}
		#endregion

		#region AfterModifyCompany
		/// <summary>
		/// AfterModifyCompany
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void AfterModifyCompany(object sender, string companyId)
		{
			if (OnAfterSavedCompany != null)
				OnAfterSavedCompany(sender, companyId);
		}
		#endregion

		#region AfterDeleteCompany - Intercettato dal SecurityAdmin
		/// <summary>
		/// AfterDeleteCompany
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void AfterDeleteCompany(object sender, string companyId)
		{
			if (OnDeleteCompanyFromSysAdmin != null)
				OnDeleteCompanyFromSysAdmin(sender, companyId);
		}
		#endregion

		#region AfterChangedAuditingCompany - Intercettato dall'AuditingAdmin
		/// <summary>
		/// AfterChangedAuditingCompany
		/// </summary>		
		//---------------------------------------------------------------------
		private void AfterChangedAuditingCompany(object sender, string companyId, bool activity)
		{
			if (OnAfterChangedCompanyAuditing != null)
				OnAfterChangedCompanyAuditing(sender, companyId, activity);
		}
		#endregion

		#region AfterChangedOSLSecurityCompany - Intercettato dal SecurityAdmin
		/// <summary>
		/// AfterChangedOSLSecurityCompany
		/// </summary>
		//---------------------------------------------------------------------
		private void AfterChangedOSLSecurityCompany(object sender, string companyId, bool security)
		{
			if (OnAfterChangedCompanyOSLSecurity != null)
				OnAfterChangedCompanyOSLSecurity(sender, companyId, security);
		}
		#endregion

		#region AfterChangedCompanyDisabled - Intercettato dal SecurityAdmin
		/// <summary>
		/// AfterChangedCompanyDisable
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		/// <param name="isDisabled"></param>
		//---------------------------------------------------------------------
		private void AfterChangedCompanyDisabled(object sender, string companyId, bool isDisabled)
		{
			if (OnAfterChangedCompanyDisable != null)
				OnAfterChangedCompanyDisable(sender, companyId, isDisabled);
		}
		#endregion

		#region AfterDisabledLogin - Intercettato dal SecurityAdmin
		/// <summary>
		/// AfterDisabledLogin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="loginId"></param>
		/// <param name="disabled"></param>
		//---------------------------------------------------------------------
		private void AfterDisabledLogin(object sender, string loginId, bool disabled)
		{
			if (OnAfterChangedDisabledLogin != null)
				OnAfterChangedDisabledLogin(sender, loginId, disabled);
		}
		#endregion

		#region company_OnAfterDisabledCompanyLogin - Intercettato dal SecurityAdmin
		/// <summary>
		/// company_OnAfterDisabledCompanyLogin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		/// <param name="loginId"></param>
		/// <param name="disable"></param>
		//---------------------------------------------------------------------
		private void company_OnAfterDisabledCompanyLogin(object sender, string companyId, string loginId, bool disable)
		{
			if (OnAfterChangedDisabledCompanyLogin != null)
				OnAfterChangedDisabledCompanyLogin(sender, companyId, loginId, disable);

			if (disable)
			{
				if (consoleTree.SelectedNode != null)
				{
					bool canDelete = false;
					// chiedo al LoginManager l'autenticazione per procedere con la cancellazione dell'associazione Utente-Azienda
					if (OnDeleteAssociationToLoginManager != null)
						canDelete = OnDeleteAssociationToLoginManager(sender, Convert.ToInt32(loginId), Convert.ToInt32(companyId));

					if (!canDelete)
					{
						// se non è stata fornita un'autenticazione valida visualizzo un msg e non procedo con l'elaborazione
						diagnostic.Set(DiagnosticType.Error, Strings.AuthenticationTokenNotValid);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						return;
					}

					//devo avvisare il lockManager
					if (OnUnlockAllForUser != null)
						OnUnlockAllForUser(sender, consoleTree.SelectedNode.Text);
				}
			}
		}
		#endregion

		#region AfterDisabledRole - Intercettato dal SecurityAdmin
		/// <summary>
		/// AfterDisabledRole
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="roleId"></param>
		/// <param name="disabled"></param>
		//---------------------------------------------------------------------
		private void AfterDisabledRole(object sender, string roleId, bool disabled)
		{
			if (OnAfterClickDisabledRole != null)
				OnAfterClickDisabledRole(sender, roleId, disabled);
		}
		#endregion

		#region GetLoggedUsersFromLoginManager - chiede al LoginManager se ci sono utenti connessi
		/// <summary>
		/// GetLoggedUsersFromLoginManager
		/// </summary>
		/// <param name="sender"></param>
		//---------------------------------------------------------------------
		private bool GetLoggedUsersFromLoginManager(object sender)
		{
			bool existLoggedUsers = false;
			if (OnGetLoggedUsers != null)
				existLoggedUsers = OnGetLoggedUsers(sender);
			return existLoggedUsers;
		}
		#endregion

		#region IsFunctionalityActivated - chiede al LoginManager se una funzionalita' e' attivata
		/// <summary>
		/// IsFunctionalityActivated
		/// Rimpalla l'evento all'OnIsActivated della Microarea Console, che lo chiede a LM
		/// </summary>
		//---------------------------------------------------------------------
		private bool IsFunctionalityActivated(string application, string functionality)
		{
			if (OnIsActivated != null)
				return OnIsActivated(application, functionality);
			else
				return false;
		}
		# endregion

		#region IfHasUserAlreadyChangedPasswordToday - True se l'utente ha già cambiato la pwd
		/// <summary>
		/// HasUserAlreadyChangedPasswordToday
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool IfHasUserAlreadyChangedPasswordToday(string user)
		{
			if (OnHasUserAlreadyChangedPasswordToday != null)
				return OnHasUserAlreadyChangedPasswordToday(user);
			else
				return false;
		}
		#endregion

		#region ExecuteTraceAction - Tracciatura di alcune operazioni su utenti/aziende (tabella MSD_Trace)
		/// <summary>
		/// ExecuteTraceAction
		/// Tracciatura di alcune operazioni su utenti/aziende (tabella MSD_Trace)
		/// </summary>
		/// <param name="company"></param>
		/// <param name="login"></param>
		/// <param name="type"></param>
		/// <param name="processName"></param>
		//---------------------------------------------------------------------
		private void ExecuteTraceAction(string company, string login, TraceActionType type, string processName)
		{
			if (OnTraceAction != null)
				OnTraceAction(company, login, type, processName);
		}
		#endregion

		#region ShutDownFromPlugIn - Evento di ShutDown di Console intercettato dal PlugIn e sparato dalla Console
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnShutDownConsole")]
		//---------------------------------------------------------------------
		public override bool ShutDownFromPlugIn()
		{
			return base.ShutDownFromPlugIn();
		}
		#endregion

		# region OnCreateCompanyDBStructure - Intercettato dall'ApplicationDBAdmin per creare le tabelle dell'azienda
		/// <summary>
		/// Richiama la funzione OnClickUpdateDatabase del plugin ApplicationDBAdmin
		/// Dopo aver creato l'azienda visualizzo direttamente la form per la creazione del database,
		/// senza visualizzare alcun messaggio aggiuntivo e relativa message box.
		/// </summary>
		//---------------------------------------------------------------------
		private void OnCreateCompanyDBStructure(object sender, string companyId)
		{
			// su richiesta di Fabio ho tolto il messaggio di avvertimento se si vuole procedere
			// a creare il database dopo l'azienda
			/*DialogResult askIfContinue = DiagnosticViewer.ShowQuestion(Strings.CreateCompanyDBStructure, Strings.NewDataBase);

			if (askIfContinue == DialogResult.Yes)
			{
				if (OnSendUpdateCompanyDatabase != null)
					OnSendUpdateCompanyDatabase(sender, companyId);
			}*/

			if (OnSendUpdateCompanyDatabase != null)
				OnSendUpdateCompanyDatabase(sender, companyId);
		}
		# endregion

		#region OnCheckRequirements - Intercettato dall'ApplicationDBAdmin testa se il db è unicode e se ha le tabelle in italiano
		/// <summary>
		/// OnCheckRequirements
		/// evento per l'ApplicationDBAdmin: viene controllato se il db usa il set di caratteri Unicode e
		/// se contiene le tabelle di Mago in versione italiana o inglese
		/// </summary>
		//---------------------------------------------------------------------
		private bool OnCheckRequirements(string connection, DBMSType dbType, bool candidateUnicode, out bool isUnicode, out bool italianTableName)
		{
			bool localUnicode = false, localItalian = false, existTBDBMark = true;

			if (OnCheckDBRequirements != null)
				existTBDBMark = OnCheckDBRequirements(connection, dbType, candidateUnicode, out localUnicode, out localItalian);

			isUnicode = localUnicode;
			italianTableName = localItalian;
			return existTBDBMark;
		}
		#endregion

		# region OnModifyCompanyParameters - Per l'ApplicationDBAdmin per modificare nella company il flag UseUnicode e IsValid e la DatabaseCulture
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.ApplicationDBAdmin.ApplicationDBAdmin", "OnModifyCompanyParameters")]
		public void OnModifyCompanyParameters(string companyId, bool isUnicode, bool isItalianVersionDB, string companyConnString, out bool modifyDBCulture)
		{
			modifyDBCulture = false;
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = currentStatus.ConnectionString;
			companyDb.CurrentSqlConnection = currentStatus.CurrentConnection;

			ArrayList company = new ArrayList();
			if (!companyDb.GetAllCompanyFieldsById(out company, companyId))
				return;

			if (company.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)company[0];
				DBMSType dbType = TBDatabaseType.GetDBMSType(companyItem.Provider);

				// se la DatabaseCulture è  uguale a zero si tratta di un restore effettuato su un'azienda
				// prima della 2.5, pertanto devo andare a valorizzare la colonna leggendo direttamente dal database
				if (companyItem.DatabaseCulture == 0)
				{
					companyItem.DatabaseCulture =
						DBGenericFunctions.AssignDatabaseCultureValue(this.licenceInfo.IsoState, companyConnString, dbType, isUnicode);
				}
				else
				{
					// devo controllare la compatibilità tra la collate presente sul db e quella memorizzata sulla
					// MSD_Companies. Se diverse devo impostare quella del database, e visualizzando un opportuno messaggio
					int dbCultureValue =
						DBGenericFunctions.AssignDatabaseCultureValue(this.licenceInfo.IsoState, companyConnString, dbType, isUnicode);

					if (dbCultureValue != companyItem.DatabaseCulture)
					{
						modifyDBCulture = true;
						companyItem.DatabaseCulture = dbCultureValue;
					}
				}

				bool supportColumnCollation =
					DBGenericFunctions.CalculateSupportColumnCollation(companyConnString, companyItem.DatabaseCulture, dbType, isUnicode);

				companyDb.Modify
					(companyId, companyItem.Company, companyItem.Description,
					companyItem.ProviderId, companyItem.DbServer, companyItem.DbName, companyItem.DbDefaultUser,
					companyItem.DbDefaultPassword, companyItem.DbOwner, companyItem.UseSecurity, companyItem.UseAuditing,
					companyItem.UseTransaction, companyItem.UseKeyedUpdate, companyItem.DBAuthenticationWindows,
					companyItem.PreferredLanguage, companyItem.ApplicationLanguage, companyItem.Disabled,
					isUnicode,
					!isItalianVersionDB,
					companyItem.DatabaseCulture,
					supportColumnCollation,
					companyItem.Port, 
					companyItem.UseDBSlave,
					companyItem.UseRowSecurity,
					companyItem.UseDataSynchro
					);

				// forzo l'update del tree della console in modo da visualizzare un eventuale cambio di stato dei nodi
				consoleTree_OnModifyUserCompany(this, ConstString.containerCompanies, companyId);
				OnAfterClickRefreshButton(this, new System.EventArgs());
			}
		}
		#endregion

		#region checkCompany_OnCheckDatabaseStructure - Per l'ApplicationDBAdmin con il controllo della struttura del database
		/// <summary>
		/// OnCheckRequirements
		/// evento per l'ApplicationDBAdmin: dato un companyId controllo la struttura del database e 
		/// ritorno un Diagnostic che contiene l'elenco dei messaggi da visualizzare
		/// </summary>
		//---------------------------------------------------------------------
		private Diagnostic checkCompany_OnCheckDatabaseStructure(string companyId)
		{
			if (OnCheckStructureCompanyDatabase != null)
			{
				Diagnostic messages = new Diagnostic("OnCheckStructureCompanyDatabase");
				OnCheckStructureCompanyDatabase(this, companyId, ref messages);
				return messages;
			}

			return null;
		}
		#endregion

		#endregion

		#region Eventi intercettati dal SysAdmin che provengono da uno o più PlugIns

		#region OnPreRenderContextMenu - Appiccico il ContextMenu inviatomi dagli altri plugins ai nodi del SysAdmin
		/// <summary>
		/// OnPreRenderContextMenu
		/// Intercetto da tutti i plugIn l'evento OnPreRenderContextMenu e aggiungo le eventuali voci aggiunte
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnPreRenderContextMenu")]
		public void OnPreRenderContextMenu(object sender, DynamicEventsArgs e)
		{
			string nodeType = (string)e.Get("NodeType");
			ContextMenu newContext = (ContextMenu)e.Get("NewContext");

			PlugInTreeNode treeNode = (PlugInTreeNode)sender;
			if (treeNode != null)
			{
				ContextMenu oldContextMenu = treeNode.ContextMenu;

				if (string.Compare(nodeType, ConstString.containerCompanies, true, CultureInfo.InvariantCulture) == 0)
					ContextMenuCompanies();
				if (string.Compare(nodeType, ConstString.itemCompany, true, CultureInfo.InvariantCulture) == 0 && treeNode.IsValid)
					ContextMenuCompany(treeNode);
				if (string.Compare(nodeType, ConstString.containerCompanyRoles, true, CultureInfo.InvariantCulture) == 0)
					this.ContextMenuRoles(treeNode.CompanyId, TBDatabaseType.GetDBMSType(treeNode.Provider));
				if (string.Compare(nodeType, ConstString.containerCompanyUsersRoles, true, CultureInfo.InvariantCulture) == 0)
					this.ContextMenuUsersRolesOfCompany(treeNode.CompanyId, treeNode.RoleId, treeNode.Id);
				if (string.Compare(nodeType, ConstString.containerCompanyUsers, true, CultureInfo.InvariantCulture) == 0)
					this.ContextMenuUserCompany(treeNode.CompanyId, treeNode.Id, TBDatabaseType.GetDBMSType(treeNode.Provider));
				if (string.Compare(nodeType, ConstString.itemCompanyUser, true, CultureInfo.InvariantCulture) == 0)
					this.ContextMenuUserCompany(treeNode.CompanyId, treeNode.Id, TBDatabaseType.GetDBMSType(treeNode.Provider));
				if (string.Compare(nodeType, ConstString.containerUsers, true, CultureInfo.InvariantCulture) == 0)
					this.ContextMenuUsers();
				if (string.Compare(nodeType, ConstString.itemUser, true, CultureInfo.InvariantCulture) == 0)
					this.ContextMenuUser();

				if ((treeNode.IsValid && string.Compare(nodeType, ConstString.itemCompany, true, CultureInfo.InvariantCulture) == 0) ||
					(string.Compare(nodeType, ConstString.itemCompany, true, CultureInfo.InvariantCulture) != 0))
				{
					oldContextMenu = context;
					MenuItem separator = new MenuItem("-");
					oldContextMenu.MenuItems.Add(separator);
					if (oldContextMenu != null)
						oldContextMenu.MergeMenu(newContext);
					else
					{
						oldContextMenu = new ContextMenu();
						oldContextMenu = newContext;
					}

					treeNode.ContextMenu = oldContextMenu;
				}
			}
		}
		#endregion

		#region OnRefreshRolesFromImport - Evento dal SecurityAdminPlugIns (dopo che sono stati importati i ruoli dell'azienda)
		/// <summary>
		/// OnRefreshRolesFromImport
		/// Evento dal SecurityAdminPlugIns (dopo che sono stati importati i ruoli dell'azienda)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SecurityAdmin.SecurityAdmin", "OnRefreshRolesFromImport")]
		public void OnRefreshRolesFromImport(object sender, int companyId)
		{
			consoleTree_onModifyRole(sender, ConstString.containerCompanyRoles, companyId.ToString());
		}
		#endregion

		#region newCompany_OnAfterSaveNewCompany - Evento per il SecurityAdminPlugIn (dopo che è stata creata una nuova azienda)
		/// <summary>
		/// OnAfterSaveNewCompany
		/// Evento per il SecurityAdminPlugIn (dopo che è stata creata una nuova azienda)
		/// </summary>
		//---------------------------------------------------------------------
		public void newCompany_OnAfterSaveNewCompany(string companyId, bool isDisabled)
		{
			if (OnAfterSaveNewCompany != null)
				OnAfterSaveNewCompany(companyId, isDisabled);
		}
		#endregion

		# region OnEnableSysAdminMenuItem e OnDisableSysAdminMenuItem
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.ApplicationDBAdmin.ApplicationDBAdmin", "OnEnableSysAdminMenuItem")]
		public void OnEnableSysAdminMenuItem()
		{
			if (OnEnableGroupPlugInMenuHandle != null)
				OnEnableGroupPlugInMenuHandle(Strings.Tools, new System.EventArgs());
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.ApplicationDBAdmin.ApplicationDBAdmin", "OnDisableSysAdminMenuItem")]
		public void OnDisableSysAdminMenuItem()
		{
			if (OnDisableGroupPlugInMenuHandle != null)
				OnDisableGroupPlugInMenuHandle(Strings.Tools, new System.EventArgs());
		}
		# endregion

		# region OnGetRecordsTraced e OnDeleteRecordsTraced
		[AssemblyEvent("All", "OnGetRecordsTraced")]
		//---------------------------------------------------------------------
		public SqlDataReader OnGetRecordsTraced(string allString, string company, string user, TraceActionType operationType, DateTime fromDate, DateTime toDate)
		{
			bool result = false;
			SqlDataReader readerRecordsTraced = null;

			OperationTracedDb operationTracedDb = new OperationTracedDb();
			operationTracedDb.ConnectionString = currentStatus.ConnectionString;
			operationTracedDb.CurrentSqlConnection = currentStatus.CurrentConnection;

			result = operationTracedDb.GetRecordsTraced(out readerRecordsTraced, allString, company, user, operationType, fromDate, toDate);
			return (result) ? readerRecordsTraced : null;
		}

		[AssemblyEvent("All", "OnDeleteRecordsTraced")]
		//---------------------------------------------------------------------
		public bool OnDeleteRecordsTraced(DateTime toDate)
		{
			bool result = false;
			OperationTracedDb operationTracedDb = new OperationTracedDb();

			operationTracedDb.ConnectionString = currentStatus.ConnectionString;
			operationTracedDb.CurrentSqlConnection = currentStatus.CurrentConnection;
			result = operationTracedDb.DeleteToDateRecordsTraces(toDate);

			if (!result)
			{
				if (operationTracedDb.Diagnostic.Error || operationTracedDb.Diagnostic.Warning || operationTracedDb.Diagnostic.Information)
					diagnostic.Set(operationTracedDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.OperationTracedDeleting);
			}
			else
				diagnostic.Set(operationTracedDb.Diagnostic);

			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
				DiagnosticViewer.ShowDiagnostic(diagnostic);

			return result;
		}
		# endregion

		#region OnGetCompanies - Restituisce le aziende presenti
		[AssemblyEvent("All", "OnGetCompanies")]
		//---------------------------------------------------------------------
		public SqlDataReader OnGetCompanies()
		{
			bool result = false;
			SqlDataReader readerCompanies = null;

			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = currentStatus.ConnectionString;
			companyDb.CurrentSqlConnection = currentStatus.CurrentConnection;

			result = companyDb.GetAllCompanies(out readerCompanies);
			return (result) ? readerCompanies : null;
		}

		#endregion

		#region OnGetCompanyUsers - Restituisce gli utenti associati a una azienda
		[AssemblyEvent("All", "OnGetCompanyUsers")]
		//---------------------------------------------------------------------
		public SqlDataReader OnGetCompanyUsers(string companyId)
		{
			bool result = false;
			SqlDataReader readerAllCompanyUsers = null;

			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = currentStatus.ConnectionString;
			companyUserDb.CurrentSqlConnection = currentStatus.CurrentConnection;

			result = companyUserDb.GetAll(out readerAllCompanyUsers, companyId);
			return (result) ? readerAllCompanyUsers : null;
		}
		#endregion

		#region OnGetApplicationUsers - Restituisce gli utenti applicativi presenti
		[AssemblyEvent("All", "OnGetApplicationUsers")]
		//---------------------------------------------------------------------
		public SqlDataReader OnGetApplicationUsers(bool localServer, string serverName)
		{
			bool result = false;
			bool selectedUserDisabled = false;
			UserDb userDb = new UserDb();
			userDb.ConnectionString = currentStatus.ConnectionString;
			userDb.CurrentSqlConnection = currentStatus.CurrentConnection;

			SqlDataReader readerApplicationUser = null;

			//posso mostrare tutti gli utenti applicativi
			if (localServer)
				result = userDb.GetAllUsers(out readerApplicationUser, selectedUserDisabled);
			//non posso mostrare gli utenti NT locali
			else
			{
				if (serverName.Length > 0)
					result = userDb.GetAllUsersExceptLocals(out readerApplicationUser, serverName);
			}
			return (result) ? readerApplicationUser : null;
		}
		#endregion

		# region Chiamato dall'ApplicationDbAmin per RegressionTest per ottenere i dati del dbo
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.ApplicationDBAdmin.ApplicationDBAdmin", "OnGetConnectionInfo")]
		public bool OnGetConnectionInfo(string companyId, CompanyItem companyItem,/*SqlOwnerUserInfo dbUserOwnerInfo, DBStructureInfo dbStructureInfo,*/ Diagnostic messages)
		{
			bool successCheck = false;
			bool successCheckDbOwner = false;

			// qui mi connetto come utente di console (accedo al sysadmin)
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = currentStatus.ConnectionString;
			companyDb.CurrentSqlConnection = this.currentStatus.CurrentConnection;

			ArrayList companyData = new ArrayList();
			successCheck = companyDb.GetAllCompanyFieldsById(out companyData, companyId);

			if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
				messages.Set(companyDb.Diagnostic);

			if (successCheck)
			{
				CompanyItem ci = (CompanyItem)companyData[0];
				companyItem.DbServer = ci.DbServer;
				companyItem.UseUnicode = ci.UseUnicode;
				companyItem.DbName = ci.DbName;

				//leggo le info per l'utente
				CompanyUserDb companyUserDb = new CompanyUserDb();
				companyUserDb.ConnectionString = currentStatus.ConnectionString;
				companyUserDb.CurrentSqlConnection = currentStatus.CurrentConnection;

				ArrayList companyUserData = new ArrayList();
				successCheckDbOwner = companyUserDb.GetUserCompany(out companyUserData, ci.DbOwner, companyId);

				if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
					messages.Set(companyDb.Diagnostic);

				if (successCheckDbOwner)
				{
					CompanyUser companyUserItem = (CompanyUser)companyUserData[0];
					companyItem.DbDefaultUser = companyUserItem.DBDefaultUser;
					companyItem.DbDefaultPassword = companyUserItem.DBDefaultPassword;
					companyItem .DBAuthenticationWindows = companyUserItem.DBWindowsAuthentication;
				}
			}
			return (successCheck && successCheckDbOwner);
		}
		#endregion

		# region OnAddRootPlugInTreeNode - Chiamato dal ServicesAdminPlugIn
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.ServicesAdmin.ServicesAdmin", "OnAddRootPlugInTreeNode")]
		public bool AddConfigParametersTreNode(PlugInTreeNode rootNode)
		{
			if (rootNode == null)
				return false;

			PlugInTreeNode configParametersPlugInNode = new PlugInTreeNode(Strings.ModifyConfigFile);
			configParametersPlugInNode.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			configParametersPlugInNode.AssemblyType = typeof(SysAdmin);
			configParametersPlugInNode.Type = ConstString.configParameters;
			configParametersPlugInNode.Checked = false;
			configParametersPlugInNode.ImageIndex = PlugInTreeNode.GetToolsDefaultImageIndex;
			configParametersPlugInNode.SelectedImageIndex = PlugInTreeNode.GetToolsDefaultImageIndex;
			rootNode.Nodes.Add(configParametersPlugInNode);

			return true;
		}
		#endregion

		#endregion

		#region Eventi intercettati dal SysAdmin che provengono dalla Microarea Console

		#region OnAfterInitPathFinder - riceve il PathFinder inizializzato
		/// <summary>
		/// OnAfterInitPathFinder
		/// </summary>
		//-------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnInitPathFinder")]
		public void OnAfterInitPathFinder(PathFinder pathFinder)
		{
			this.pathFinder = pathFinder;
		}
		#endregion

		#region OnInitBrandLoader - riceve il BrandLoader inizializzato
		//-------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnInitBrandLoader")]
		public void OnInitBrandLoader(BrandLoader aBrandLoader)
		{
			brandLoader = aBrandLoader;
		}
		#endregion

		#region OnAfterChangedView - scelta della visualizzazione delle liste
		/// <summary>
		/// OnAfterChangedView
		/// Intercetto l'event OnChangedView sparato dalla console (ho selezionato una diversa visualizzazione) 
		/// e poi ridisegno la lista nella working area (panel)
		/// </summary>
		//-------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnChangedView")]
		public void OnAfterChangedView(object sender, View typeOfView)
		{
			if (currentStatus != null)
			{
				currentStatus.TypeOfView = typeOfView;
				if (consoleTree.SelectedNode != null) OnAfterSelectConsoleTree(sender, new TreeViewEventArgs(consoleTree.SelectedNode));
			}
		}
		#endregion

		#region OnAfterClickNewButton - Premuto il Bottone New dalla Microarea Console
		/// <summary>
		/// OnAfterClickNewButton
		/// Intercetta la pressione del bottone New dalla console
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnNewItem")]
		public void OnAfterClickNewButton(object sender, System.EventArgs e)
		{
			consoleTree.Focus();
			if ((consoleTree.SelectedNode != null) &&
				(((PlugInTreeNode)consoleTree.SelectedNode).AssemblyName == Assembly.GetExecutingAssembly().GetName().Name))
			{
				string typeOfObject = ((PlugInTreeNode)consoleTree.SelectedNode).Type;
				PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
				switch (typeOfObject)
				{
					case ConstString.containerCompanies:
						OnNewCompany(sender, e);
						break;
					case ConstString.containerUsers:
						OnNewUser(sender, e);
						break;
					case ConstString.containerCompanyRoles:
						OnNewRole(selectedNode.ContextMenu.MenuItems[0], e);
						break;
					default:
						break;
				}
			}
		}
		#endregion

		#region OnAfterClickOpenButton - Premuto il Bottone Open dalla Microarea Console
		/// <summary>
		/// OnAfterClickOpenButton
		/// Intercetta la pressione del bottone Open dalla Console
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnOpenItem")]
		public void OnAfterClickOpenButton(object sender, System.EventArgs e)
		{
			consoleTree.Focus();
			if ((consoleTree.SelectedNode != null) &&
				(((PlugInTreeNode)consoleTree.SelectedNode).AssemblyName == Assembly.GetExecutingAssembly().GetName().Name))
			{
				//abilito la progressBar
				SetProgressBarTextFromPlugIn(sender, Strings.ProgressWaiting);
				SetProgressBarValueFromPlugIn(sender, 1);
				SetProgressBarStepFromPlugIn(sender, 3);
				SetProgressBarMaxValueFromPlugIn(sender, 100);
				EnableProgressBarFromPlugIn(sender);
				Application.DoEvents();

				string typeOfObject = ((PlugInTreeNode)consoleTree.SelectedNode).Type;
				PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;

				switch (typeOfObject)
				{
					case ConstString.itemCompany:
						OnOpenCompany(selectedNode, selectedNode.Id);
						break;
					case ConstString.itemRole:
						OnOpenRole(selectedNode, selectedNode.Id, selectedNode.CompanyId);
						break;
					case ConstString.itemCompanyUser:
						OnModifyCompanyUser(selectedNode, selectedNode.Id, selectedNode.CompanyId);
						break;
					case ConstString.itemRoleCompanyUser:
						OnModifyCompanyUserRole(selectedNode, selectedNode.Id, selectedNode.CompanyId, selectedNode.RoleId);
						break;
					case ConstString.itemUser:
						OnOpenUser(selectedNode, selectedNode.Id);
						break;
					default:
						break;
				}
				//disabilito la progressBar
				SetProgressBarValueFromPlugIn(sender, 100);
				SetProgressBarTextFromPlugIn(sender, string.Empty);
				DisableProgressBarFromPlugIn(sender);
			}
		}

		#region Operazioni di Open dei singoli oggetti nel SysAdmin

		#region company_OnOpen - Visualizzazione di una Azienda precedentemente inserita
		/// <summary>
		/// company_OnOpen
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		//---------------------------------------------------------------------
		private void company_OnOpen(object sender, string id)
		{
			SetProgressBarMaxValueFromPlugIn(sender, 100);
			workingAreaConsole.Controls.Clear();
			
			if (consoleEnvironmentInfo.IsLiteConsole)
			{
				CompanyLite modCompanyLite = new CompanyLite
					(
					currentStatus.OwnerDbName,
					currentStatus.ConnectionString,
					currentStatus.CurrentConnection,
					id,
					pathFinder,
					licenceInfo
					);
				modCompanyLite.UserConnected = currentStatus.User;
				modCompanyLite.UserPwdConnected = currentStatus.Password;
				modCompanyLite.DataSourceSysAdmin = currentStatus.DataSource;
				modCompanyLite.OnCallHelp += new CompanyLite.CallHelp(HelpFromPopUp);
				modCompanyLite.OnModifyTree += new CompanyLite.ModifyTree(consoleTree_onModifyAzienda);
				modCompanyLite.OnCheckDBRequirementsUsed += new CompanyLite.CheckDBRequirementsUsed(OnCheckRequirements);
				modCompanyLite.OnAfterChangedAuditing += new CompanyLite.AfterChangedAuditing(AfterChangedAuditingCompany);
				modCompanyLite.OnAfterChangedOSLSecurity += new CompanyLite.AfterChangedOSLSecurity(AfterChangedOSLSecurityCompany);
				modCompanyLite.OnEnableProgressBar += new CompanyLite.EnableProgressBar(EnableProgressBarFromPlugIn);
				modCompanyLite.OnDisableProgressBar += new CompanyLite.DisableProgressBar(DisableProgressBarFromPlugIn);
				modCompanyLite.OnSetProgressBarStep += new CompanyLite.SetProgressBarStep(SetProgressBarStepFromPlugIn);
				modCompanyLite.OnSetProgressBarText += new CompanyLite.SetProgressBarText(SetProgressBarTextFromPlugIn);
				modCompanyLite.OnSetProgressBarValue += new CompanyLite.SetProgressBarValue(SetProgressBarValueFromPlugIn);
				modCompanyLite.OnGetUserAuthenticatedPwdFromConsole += new CompanyLite.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				modCompanyLite.OnIsUserAuthenticatedFromConsole += new CompanyLite.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				modCompanyLite.OnAddUserAuthenticatedFromConsole += new CompanyLite.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				modCompanyLite.OnEnableSaveButton += new EventHandler(OnEnableSaveToolBarButton);
				modCompanyLite.OnIsActivated += new CompanyLite.IsActivated(IsFunctionalityActivated);
				modCompanyLite.TopLevel = false;
				modCompanyLite.Dock = DockStyle.Fill;

				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, modCompanyLite.ClientSize.Width, modCompanyLite.ClientSize.Height);
				workingAreaConsole.Controls.Add(modCompanyLite);
				modCompanyLite.Show();
			}
			else
			{
			Company modCompany = new Company
				(
				currentStatus.OwnerDbName,
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				id,
				pathFinder,
				licenceInfo
				);
			modCompany.UserConnected = currentStatus.User;
			modCompany.UserPwdConnected = currentStatus.Password;
			modCompany.DataSourceSysAdmin = currentStatus.DataSource;
			modCompany.OnCallHelp += new Company.CallHelp(HelpFromPopUp);
			modCompany.OnModifyTree += new Company.ModifyTree(consoleTree_onModifyAzienda);
			modCompany.OnCheckDBRequirementsUsed += new Company.CheckDBRequirementsUsed(OnCheckRequirements);
			modCompany.OnAfterChangedAuditing += new Company.AfterChangedAuditing(AfterChangedAuditingCompany);
			modCompany.OnAfterChangedOSLSecurity += new Company.AfterChangedOSLSecurity(AfterChangedOSLSecurityCompany);
			modCompany.OnEnableProgressBar += new Company.EnableProgressBar(EnableProgressBarFromPlugIn);
			modCompany.OnDisableProgressBar += new Company.DisableProgressBar(DisableProgressBarFromPlugIn);
			modCompany.OnSetProgressBarStep += new Company.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			modCompany.OnSetProgressBarText += new Company.SetProgressBarText(SetProgressBarTextFromPlugIn);
			modCompany.OnSetProgressBarValue += new Company.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			modCompany.OnGetUserAuthenticatedPwdFromConsole += new Company.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			modCompany.OnIsUserAuthenticatedFromConsole += new Company.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			modCompany.OnAddUserAuthenticatedFromConsole += new Company.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			modCompany.OnDisableSaveButton += new EventHandler(OnDisableSaveToolBarButton);
			modCompany.OnEnableSaveButton += new EventHandler(OnEnableSaveToolBarButton);
			modCompany.OnIsActivated += new Company.IsActivated(IsFunctionalityActivated);
			modCompany.TopLevel = false;
			modCompany.Dock = DockStyle.Fill;

			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, modCompany.ClientSize.Width, modCompany.ClientSize.Height);
			workingAreaConsole.Controls.Add(modCompany);
			modCompany.Show();
			}

			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null) OnDisableShowAllGrantsToolBarButtonPushed(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
		}
		#endregion

		#region role_OnOpen - Visualizzazione di un Ruolo di una Azienda precedentemente inserito
		/// <summary>
		/// role_OnOpen
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void role_OnOpen(object sender, string id, string companyId)
		{
			workingAreaConsole.Controls.Clear();
			Role modRole = new Role(currentStatus.ConnectionString, currentStatus.CurrentConnection, id, companyId);
			modRole.OnModifyTreeOfCompanies += new Role.ModifyTreeOfCompanies(consoleTree_onModifyRole);
			modRole.OnSendDiagnostic += new Role.SendDiagnostic(ReceiveDiagnostic);
			modRole.TopLevel = false;
			modRole.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, modRole.ClientSize.Width, modRole.ClientSize.Height);
			workingAreaConsole.Controls.Add(modRole);
			modRole.Show();
			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null)
				OnDisableShowAllGrantsToolBarButtonPushed(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
		}
		#endregion

		#region company_OnModifyCompanyUserRole - Visualizzazione di un Utente associato al Ruolo di una Azienda
		/// <summary>
		/// company_OnModifyCompanyUserRole
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="roleId"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void company_OnModifyCompanyUserRole(object sender, string loginId, string companyId, string roleId)
		{
			workingAreaConsole.Controls.Clear();
			DetailUserRole modUserCompanyRole = new DetailUserRole(currentStatus.ConnectionString, currentStatus.CurrentConnection, companyId, loginId, roleId);
			modUserCompanyRole.OnSendDiagnostic += new DetailUserRole.SendDiagnostic(ReceiveDiagnostic);
			modUserCompanyRole.OnModifyTreeOfCompanies += new DetailUserRole.ModifyTreeOfCompanies(consoleTree_onModifyRole);
			modUserCompanyRole.TopLevel = false;
			modUserCompanyRole.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(sender, modUserCompanyRole.ClientSize.Width, modUserCompanyRole.ClientSize.Height);
			workingAreaConsole.Controls.Add(modUserCompanyRole);
			modUserCompanyRole.Show();
			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, new System.EventArgs());
			if (OnDisableOpenToolBarButton != null) OnDisableOpenToolBarButton(sender, new System.EventArgs());
			if (OnEnableSaveToolBarButton != null) OnEnableSaveToolBarButton(sender, new System.EventArgs());
			if (OnEnableDeleteToolBarButton != null) OnEnableDeleteToolBarButton(sender, new System.EventArgs());
			if (OnDisableFindSecurityObjectsToolBarButton != null) OnDisableFindSecurityObjectsToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, new System.EventArgs());
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, new System.EventArgs());
			if (OnDisableShowAllGrantsToolBarButtonPushed != null)
				OnDisableShowAllGrantsToolBarButtonPushed(sender, new System.EventArgs());
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, new System.EventArgs());
		}

		#endregion

		#region user_OnOpen - Visualizzazione di un Utente Applicativo precedentemente inserito
		/// <summary>
		/// user_OnOpen 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		//---------------------------------------------------------------------
		private void user_OnOpen(object sender, string id)
		{
			workingAreaConsole.Controls.Clear();
			//abilito la progressBar
			SetProgressBarMaxValueFromPlugIn(sender, 100);
			SetProgressBarTextFromPlugIn(sender, Strings.ProgressWaiting);
			SetProgressBarValueFromPlugIn(sender, 1);
			SetProgressBarStepFromPlugIn(sender, 3);
			EnableProgressBarFromPlugIn(sender);

			if (consoleEnvironmentInfo.IsLiteConsole || licenceInfo.IsAzureSQLDatabase)
			{
				UserLite modUserLite = new UserLite(currentStatus, licenceInfo.IsAzureSQLDatabase, id, pathFinder);
				modUserLite.OnAddGuestUser += new EventHandler(AddGuestUser);
				modUserLite.OnDeleteGuestUser += new EventHandler(DeleteGuestUser);
				modUserLite.OnSendDiagnostic += new UserLite.SendDiagnostic(ReceiveDiagnostic);
				modUserLite.OnModifyTree += new UserLite.ModifyTree(consoleTree_OnModifyUtente);
				modUserLite.OnAfterDisabledCheckedChanged += new UserLite.AfterDisabledCheckedChanged(AfterDisabledLogin);
				modUserLite.OnTraceAction += new UserLite.TraceAction(ExecuteTraceAction);
				modUserLite.TopLevel = false;
				modUserLite.Dock = DockStyle.Fill;
				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, modUserLite.ClientSize.Width, modUserLite.ClientSize.Height);
				workingAreaConsole.Controls.Add(modUserLite);
				//disabilito la progressBar
				SetProgressBarValueFromPlugIn(sender, 100);
				SetProgressBarTextFromPlugIn(sender, string.Empty);
				DisableProgressBarFromPlugIn(sender);
				modUserLite.Show();
			}
			else
			{
				User modUser = new User(currentStatus, licenceInfo.IsAzureSQLDatabase, id, pathFinder, brandLoader);
				modUser.OnAddGuestUser += new EventHandler(AddGuestUser);
				modUser.OnDeleteGuestUser += new EventHandler(DeleteGuestUser);
				modUser.OnSendDiagnostic += new User.SendDiagnostic(ReceiveDiagnostic);
				modUser.OnModifyTree += new User.ModifyTree(consoleTree_OnModifyUtente);
				modUser.OnAfterDisabledCheckedChanged += new User.AfterDisabledCheckedChanged(AfterDisabledLogin);
				modUser.OnTraceAction += new User.TraceAction(ExecuteTraceAction);
				modUser.TopLevel = false;
				modUser.Dock = DockStyle.Fill;
				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, modUser.ClientSize.Width, modUser.ClientSize.Height);
				workingAreaConsole.Controls.Add(modUser);
				//disabilito la progressBar
				SetProgressBarValueFromPlugIn(sender, 100);
				SetProgressBarTextFromPlugIn(sender, string.Empty);
				DisableProgressBarFromPlugIn(sender);
				modUser.Show();
			}

			OnDisableNewToolBarButton?.Invoke(sender, new EventArgs()); 
			OnDisableOpenToolBarButton?.Invoke(sender, new EventArgs());
			OnEnableSaveToolBarButton?.Invoke(sender, new EventArgs());
			OnEnableDeleteToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableShowSecurityIconsToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableFindSecurityObjectsToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableApplySecurityFilterToolBarButton?.Invoke(sender, new EventArgs());
			OnDisableShowAllGrantsToolBarButtonPushed?.Invoke(sender, new EventArgs());
			OnDisableOtherObjectsToolBarButton?.Invoke(sender, new EventArgs());
		}
		#endregion
		#endregion
		#endregion

		#region OnAfterClickSaveButton - Premuto il Bottone di Save dalla Microarea Console
		/// <summary>
		/// OnAfterClickSaveButton
		/// Intercetta la pressione del bottone Save dalla Console
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnSaveItem")]
		public void OnAfterClickSaveButton(object sender, System.EventArgs e)
		{
			consoleTree.Focus();

			if ((consoleTree.SelectedNode != null) &&
				(((PlugInTreeNode)consoleTree.SelectedNode).AssemblyName == Assembly.GetExecutingAssembly().GetName().Name))
			{
				string typeOfObject = ((PlugInTreeNode)consoleTree.SelectedNode).Type;
				PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
				consoleTree.SuspendLayout();

				switch (typeOfObject)
				{
					case ConstString.itemProvider:
						OnSaveProvider(selectedNode, selectedNode.Id);
						break;
					case ConstString.containerUsers:
						OnSaveNewUser(selectedNode, selectedNode.Id);
						break;
					case ConstString.itemUser:
						OnSaveUser(selectedNode, selectedNode.Id);
						break;
					case ConstString.containerCompanies:
						OnSaveCompany(selectedNode, selectedNode.Id);
						break;
					case ConstString.itemCompany:
						OnSaveCompany(selectedNode, selectedNode.Id);
						break;
					case ConstString.containerCompanyRoles:
						OnSaveNewRole(selectedNode, selectedNode.Id, selectedNode.CompanyId);
						break;
					case ConstString.itemRole:
						OnSaveRole(selectedNode, selectedNode.Id, selectedNode.CompanyId);
						break;
					case ConstString.itemCompanyUser:
						OnSaveCompanyUser(selectedNode, selectedNode.Id, selectedNode.CompanyId);
						break;
					case ConstString.containerCompanyUsers:
						OnSaveCompanyUser(selectedNode, selectedNode.Id, selectedNode.CompanyId);
						break;
					case ConstString.itemRoleCompanyUser:
						OnSaveCompanyUserRole(selectedNode, selectedNode.Id, selectedNode.CompanyId, selectedNode.RoleId);
						break;
					case ConstString.containerLoginsUsers:
						OnSaveCompanyUserToLogin(selectedNode, selectedNode.CompanyId);
						break;
					case ConstString.configParameters:
						OnSaveConfigFile(selectedNode);
						break;
					default:
						break;
				}
				consoleTree.ResumeLayout();
			}
		}

		#region Operazioni di Save sui singoli oggetti del SysAdmin

		#region editConfigFile_OnSave - Salvataggio del file ServerConnection.config
		/// <summary>
		/// editConfigFile_OnSave
		/// </summary>
		/// <param name="sender"></param>
		//---------------------------------------------------------------------
		private void editConfigFile_OnSave(object sender)
		{
			if (workingAreaConsole.Controls.Count >= 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
				if (string.Compare(o.GetType().Name, ConstString.configParameters, true, CultureInfo.InvariantCulture) == 0)
				{
					EditConfigFile editConfigFile = (EditConfigFile)workingAreaConsole.Controls[0];
					editConfigFile.Save(sender);
				}
			}
		}
		# endregion

		#region provider_OnSave - Salvataggio di un Provider
		/// <summary>
		/// provider_OnSave
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		//---------------------------------------------------------------------
		private void provider_OnSave(object sender, string id)
		{
			if (workingAreaConsole.Controls.Count >= 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
				if (string.Compare(o.GetType().Name, ConstString.itemProvider, true, CultureInfo.InvariantCulture) == 0)
				{
					Provider saveProvider = (Provider)workingAreaConsole.Controls[0];
					saveProvider.Save(sender, new System.EventArgs());
				}
			}
		}
		#endregion

		#region user_OnSaveNew - Salvataggio di un nuovo Utente Applicativo
		/// <summary>
		/// user_OnSaveNew
		/// </summary>
		//---------------------------------------------------------------------
		private void user_OnSaveNew(object sender, string id)
		{
			if (workingAreaConsole.Controls.Count >= 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
				if (string.Compare(o.GetType().Name, "User", true, CultureInfo.InvariantCulture) == 0)
				{
					User saveUser = (User)workingAreaConsole.Controls[0];
					saveUser.Save(sender, new System.EventArgs());
				}
				if (string.Compare(o.GetType().Name, "UserLite", true, CultureInfo.InvariantCulture) == 0)
				{
					UserLite saveUserLite = (UserLite)workingAreaConsole.Controls[0];
					saveUserLite.Save(sender, new System.EventArgs());
				}
			}
		}
		#endregion

		#region user_OnSave - Salvataggio di un Utente Applicativo
		/// <summary>
		/// user_OnSave
		/// </summary>
		//---------------------------------------------------------------------
		private void user_OnSave(object sender, string id)
		{
			if (workingAreaConsole.Controls.Count >= 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
				if (string.Compare(o.GetType().Name, "User", true, CultureInfo.InvariantCulture) == 0)
				{
					User saveUser = (User)workingAreaConsole.Controls[0];
					saveUser.Save(sender, new System.EventArgs());
				}
				if (string.Compare(o.GetType().Name, "UserLite", true, CultureInfo.InvariantCulture) == 0)
				{
					UserLite saveUserLite = (UserLite)workingAreaConsole.Controls[0];
					saveUserLite.Save(sender, new System.EventArgs());
				}
			}
		}
		#endregion

		#region company_OnSave - Salvataggio di una Azienda
		/// <summary>
		/// company_OnSave
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		//---------------------------------------------------------------------
		private void company_OnSave(object sender, string id)
		{
			if (workingAreaConsole.Controls.Count >= 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
				if (string.Compare(o.GetType().Name, "Company", true, CultureInfo.InvariantCulture) == 0)
				{
					Company saveCompany = (Company)workingAreaConsole.Controls[0];
					saveCompany.UserConnected = currentStatus.User;
					saveCompany.UserPwdConnected = currentStatus.Password;
					saveCompany.DataSourceSysAdmin = currentStatus.DataSource;
					saveCompany.IntegratedConnection = currentStatus.IntegratedConnection;
					this.consoleTree.Cursor = Cursors.WaitCursor;
					bool isCompanySaved = saveCompany.SaveCompany();
					this.consoleTree.Cursor = Cursors.Default;
					if ((isCompanySaved) && (OnAfterSavedCompany != null))
						OnAfterSavedCompany(sender, id);
				}
				if (string.Compare(o.GetType().Name, "CompanyLite", true, CultureInfo.InvariantCulture) == 0)
				{
					CompanyLite saveCompanyLite = (CompanyLite)workingAreaConsole.Controls[0];
					saveCompanyLite.UserConnected = currentStatus.User;
					saveCompanyLite.UserPwdConnected = currentStatus.Password;
					saveCompanyLite.DataSourceSysAdmin = currentStatus.DataSource;
					saveCompanyLite.IntegratedConnection = currentStatus.IntegratedConnection;
					this.consoleTree.Cursor = Cursors.WaitCursor;
					bool isCompanySaved = saveCompanyLite.SaveCompany();
					this.consoleTree.Cursor = Cursors.Default;
					if ((isCompanySaved) && (OnAfterSavedCompany != null))
						OnAfterSavedCompany(sender, id);
				}
			}
		}
		#endregion

		#region role_OnSaveNew - Salvataggio di un nuovo Ruolo di Azienda
		/// <summary>
		/// role_OnSaveNew
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void role_OnSaveNew(object sender, string id, string companyId)
		{
			if (workingAreaConsole.Controls.Count >= 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
				if (string.Compare(o.GetType().Name, "Role", true, CultureInfo.InvariantCulture) == 0)
				{
					Role saveRole = (Role)workingAreaConsole.Controls[0];
					saveRole.SaveNew(sender, new System.EventArgs());
				}
			}
		}
		#endregion

		#region role_OnSave - Salvataggio di un Ruolo di una Azienda
		/// <summary>
		/// role_OnSave
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void role_OnSave(object sender, string id, string companyId)
		{
			if (workingAreaConsole.Controls.Count >= 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
				if (string.Compare(o.GetType().Name, "Role", true, CultureInfo.InvariantCulture) == 0)
				{
					Role saveRole = (Role)workingAreaConsole.Controls[0];
					saveRole.Save(sender, new System.EventArgs());
				}
				if (string.Compare(o.GetType().Name, "JoinUserRole", true, CultureInfo.InvariantCulture) == 0)
				{
					JoinUserRole saveJoinUserRole = (JoinUserRole)workingAreaConsole.Controls[0];
					saveJoinUserRole.Save(sender, new System.EventArgs());
				}
			}
		}
		#endregion

		#region company_OnSaveCompanyUserToLogin - Salvataggio delle Logins di una Azienda
		/// <summary>
		/// company_OnSaveCompanyUserToLogin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void company_OnSaveCompanyUserToLogin(object sender, string companyId)
		{
			if (workingAreaConsole.Controls.Count >= 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
				if (string.Compare(o.GetType().Name, "AddCompanyUsersToLogin", true, CultureInfo.InvariantCulture) == 0)
				{
					AddCompanyUserToLogin addCompanyUserToLogin = (AddCompanyUserToLogin)o;
					addCompanyUserToLogin.Save(sender, new System.EventArgs());
				}
				if (string.Compare(o.GetType().Name, "AddCompanyUsersToLoginLite", true, CultureInfo.InvariantCulture) == 0)
				{
					AddCompanyUsersToLoginLite addCompanyUsersToLoginLite = (AddCompanyUsersToLoginLite)o;
					addCompanyUsersToLoginLite.Save();
				}
			}
		}
		#endregion

		#region company_OnSaveCompanyUser - Salvataggio di un Utente di una Azienda
		/// <summary>
		/// company_OnSaveCompanyUser
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void company_OnSaveCompanyUser(object sender, string id, string companyId)
		{
			if (workingAreaConsole.Controls.Count == 0)
				return;

			Control o = workingAreaConsole.Controls[0];
			if (o == null)
				return;

			// TODO - cleaned-up and fixed, but still shitty: better get rid of hard-coded namespaces and use an enum instead
			string typeName = o.GetType().Name;

			if (string.Compare(typeName, "AddCompanyUsersToLogin", true, CultureInfo.InvariantCulture) == 0)
			{
				AddCompanyUsersToLogin addCompanyUsersToLogin = (AddCompanyUsersToLogin)o;
				addCompanyUsersToLogin.NewSave();
			}
			else if (string.Compare(typeName, "AddCompanyUsersToLoginLite", true, CultureInfo.InvariantCulture) == 0)
			{
				AddCompanyUsersToLoginLite addCompanyUsersToLoginLite = (AddCompanyUsersToLoginLite)o;
				addCompanyUsersToLoginLite.Save();
			}
			else if (string.Compare(typeName, "AddCompanyUsersToLoginPostgre", true, CultureInfo.InvariantCulture) == 0)
            {
                AddCompanyUsersToLoginPostgre addCompanyUsersToLoginPostgre = (AddCompanyUsersToLoginPostgre)o;
                addCompanyUsersToLoginPostgre.NewSave();
            }
			else if (string.Compare(typeName, "AddCompanyUserToLogin", true, CultureInfo.InvariantCulture) == 0)
			{
				AddCompanyUserToLogin addCompanyUserToLogin = (AddCompanyUserToLogin)o;
				addCompanyUserToLogin.Save(sender, new System.EventArgs());
			}
            else if (string.Compare(typeName, "AddCompanyUserToLoginPostgre", true, CultureInfo.InvariantCulture) == 0)
            {
                AddCompanyUserToLoginPostgre addCompanyUserToLoginPostgre = (AddCompanyUserToLoginPostgre)o;
                addCompanyUserToLoginPostgre.Save(sender, new System.EventArgs());
            }
			else if (string.Compare(typeName, "AddCompanyUserToOracleLogin", true, CultureInfo.InvariantCulture) == 0)
			{
				AddCompanyUserToOracleLogin addOracleLogin = (AddCompanyUserToOracleLogin)o;
				addOracleLogin.Save(sender, new System.EventArgs()); 
			}
			else if (string.Compare(typeName, "ModifyCompanyUsersToLogin", true, CultureInfo.InvariantCulture) == 0)
			{
				ModifyCompanyUsersToLogin modifyCompanyUsersToLogin = (ModifyCompanyUsersToLogin)o;
				modifyCompanyUsersToLogin.Save();
			}
            else if (string.Compare(typeName, "ModifyCompanyUsersToLoginPostgre", true, CultureInfo.InvariantCulture) == 0)
            {
                ModifyCompanyUsersToLoginPostgre modifyCompanyUsersToLoginPostgre = (ModifyCompanyUsersToLoginPostgre)o;
                modifyCompanyUsersToLoginPostgre.Save();
            }
			else if (string.Compare(typeName, "ModifyCompanyUsersToOracleLogin", true, CultureInfo.InvariantCulture) == 0)
			{
				ModifyCompanyUsersToOracleLogin modifyOracleLogin = (ModifyCompanyUsersToOracleLogin)o;
				modifyOracleLogin.Save();
			}
			else if (string.Compare(typeName, "AddLoginToCompany", true, CultureInfo.InvariantCulture) == 0)
			{
				AddLoginToCompany addLoginToCompany = (AddLoginToCompany)o;
				addLoginToCompany.AddLogin(sender, new System.EventArgs());
			}
			else if (string.Compare(typeName, "LoginAdministrator", true, CultureInfo.InvariantCulture) == 0)
			{
				LoginAdministrator loginAdministrator = (LoginAdministrator)o;
				loginAdministrator.Save();
			}
			else if (string.Compare(typeName, "DetailCompanyUser", true, CultureInfo.InvariantCulture) == 0)
			{
				DetailCompanyUser saveCompanyUser = (DetailCompanyUser)o;
				saveCompanyUser.OnAfterChangeDisabledCheckBox += new DetailCompanyUser.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
				saveCompanyUser.SaveCompanyUser();
			}
			else if (string.Compare(typeName, "DetailCompanyUserLite", true, CultureInfo.InvariantCulture) == 0)
			{
				DetailCompanyUserLite saveCompanyUserLite = (DetailCompanyUserLite)o;
				saveCompanyUserLite.OnAfterChangeDisabledCheckBox += new DetailCompanyUserLite.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
				saveCompanyUserLite.SaveCompanyUser();
			}
            else if (string.Compare(typeName, "DetailCompanyUserPostgre", true, CultureInfo.InvariantCulture) == 0)
            {
                DetailCompanyUserPostgre saveCompanyUserPostgre = (DetailCompanyUserPostgre)o;
                saveCompanyUserPostgre.OnAfterChangeDisabledCheckBox += new DetailCompanyUserPostgre.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
                saveCompanyUserPostgre.SaveCompanyUser();
            }
			else if (string.Compare(typeName, "DetailOracleCompanyUser", true, CultureInfo.InvariantCulture) == 0)
			{
				DetailOracleCompanyUser saveOracleCompanyUser = (DetailOracleCompanyUser)o;
				saveOracleCompanyUser.OnAfterChangeDisabledCheckBox += new DetailOracleCompanyUser.AfterChangeDisabledCheckBox(company_OnAfterDisabledCompanyLogin);
				saveOracleCompanyUser.ModifyCompanyUser();
			}
			else if (string.Compare(typeName, "JoinRoleUser", true, CultureInfo.InvariantCulture) == 0)
			{
				JoinRoleUser saveJoinRoleUser = (JoinRoleUser)o;
				saveJoinRoleUser.Save(sender, new System.EventArgs());
			}
		}
		#endregion

		#region company_OnSaveCompanyUserRole - Salvataggio di un Utente associato al Ruolo di una Azienda
		/// <summary>
		/// company_OnSaveCompanyUserRole
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="loginId"></param>
		/// <param name="companyId"></param>
		/// <param name="roleId"></param>
		//---------------------------------------------------------------------
		private void company_OnSaveCompanyUserRole(object sender, string loginId, string companyId, string roleId)
		{
			if (workingAreaConsole.Controls.Count >= 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
				if (string.Compare(o.GetType().Name, "JoinUserRole", true, CultureInfo.InvariantCulture) == 0)
				{
					JoinUserRole saveCompanyUserRole = (JoinUserRole)workingAreaConsole.Controls[0];
					saveCompanyUserRole.Save(sender, new System.EventArgs());
				}
				if (string.Compare(o.GetType().Name, "DetailUserRole", true, CultureInfo.InvariantCulture) == 0)
				{
					DetailUserRole saveCompanyUserRole = (DetailUserRole)workingAreaConsole.Controls[0];
					saveCompanyUserRole.Save(sender, new System.EventArgs());
				}
			}
		}
		#endregion
		#endregion
		#endregion

		#region OnAfterClickDeleteButton - Premuto il bottone di Delete della Microarea Console
		/// <summary>
		/// OnAfterClickDeleteButton
		/// Intercetta la pressione del bottone Delete dalla Console
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnDeleteItem")]
		public void OnAfterClickDeleteButton(object sender, System.EventArgs e)
		{
			this.consoleTree.Focus();
			if ((consoleTree.SelectedNode != null) &&
				(((PlugInTreeNode)consoleTree.SelectedNode).AssemblyName == Assembly.GetExecutingAssembly().GetName().Name))
			{
				string typeOfObject = ((PlugInTreeNode)consoleTree.SelectedNode).Type;
				PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;

				DBMSType providerType = TBDatabaseType.GetDBMSType(selectedNode.Provider);
				switch (typeOfObject)
				{
					case ConstString.itemUser:
						OnDeleteUser(selectedNode, selectedNode.Id);
						break;
					case ConstString.itemCompany:
						OnDeleteCompany(selectedNode, selectedNode.Id, providerType);
						break;
					case ConstString.itemRole:
						OnDeleteRole(selectedNode, selectedNode.Id, selectedNode.CompanyId);
						break;
					case ConstString.itemCompanyUser:
						OnDeleteCompanyUser(selectedNode, selectedNode.Id, selectedNode.CompanyId, TBDatabaseType.GetDBMSType(selectedNode.Provider));
						break;
					case ConstString.itemRoleCompanyUser:
						OnDeleteCompanyUserRole(selectedNode, selectedNode.Id, selectedNode.CompanyId, selectedNode.RoleId);
						break;
					default:
						break;
				}
			}
		}

		#region Azioni di Delete sugli oggetti del SysAdmin

		#region user_OnDelete - Cancellazione di un Utente Applicativo
		/// <summary>
		/// user_OnDelete
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		//---------------------------------------------------------------------
		private void user_OnDelete(object sender, string id)
		{
			//metto la form in view in modo che non mi dia fastidio
			if (workingAreaConsole.Controls.Count != 0 && workingAreaConsole.Controls[0] is PlugInsForm)
				((PlugInsForm)workingAreaConsole.Controls[0]).State = StateEnums.View;

			Cursor currentConsoleFormCursor = consoleTree.TopLevelControl.Cursor;

			DialogResult askIfContinue = DiagnosticViewer.ShowQuestion(Strings.AskIfDeleteUser, Strings.Delete);

			if (askIfContinue == DialogResult.Yes)
			{
				if (consoleTree.SelectedNode != null)
				{
					bool canDelete = false;
					// chiedo al LoginManager l'autenticazione per procedere con la cancellazione dell'utente applicativo
					if (OnDeleteUserToLoginManager != null)
						canDelete = OnDeleteUserToLoginManager(sender, Convert.ToInt32(id));

					if (!canDelete)
					{
						// se non è stata fornita un'autenticazione valida visualizzo un msg e non procedo con l'elaborazione
						consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
						diagnostic.Set(DiagnosticType.Error, Strings.AuthenticationTokenNotValid);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						return;
					}

					string userName = consoleTree.SelectedNode.Text;
					UserDb dbUser = new UserDb();
					dbUser.ConnectionString = currentStatus.ConnectionString;
					dbUser.CurrentSqlConnection = currentStatus.CurrentConnection;

					bool isGuest = IsGuestUser(id);
					bool result = dbUser.Delete(id);

					if (!result)
					{
						if (dbUser.Diagnostic.Error || dbUser.Diagnostic.Information || dbUser.Diagnostic.Warning)
						{
							diagnostic.Set(dbUser.Diagnostic);
							DiagnosticViewer.ShowDiagnostic(diagnostic);
						}
					}
					else
					{
						if (isGuest)
							DeleteGuestUser(sender, new EventArgs());

						consoleTree_OnModifyUtente(sender, ConstString.containerUsers);
						consoleTree_onModifyAzienda(sender, ConstString.containerCompanies);

						//comunico al LoginManager la cancellazione
						if (OnDeleteUserToLoginManager != null)
							OnDeleteUserToLoginManager(sender, Convert.ToInt32(id));
						if (OnDeleteUserToPlugIns != null)
							OnDeleteUserToPlugIns(sender, id);

						//comunico al LockManager la cancellazione
						if ((OnUnlockAllForUser != null) && (userName.Length > 0))
							OnUnlockAllForUser(sender, userName);
						if (this.OnTraceAction != null)
							OnTraceAction(string.Empty, userName, TraceActionType.DeleteUser, DatabaseLayerConsts.MicroareaConsole);
					}
				}
			}
		}
		#endregion

		#region company_OnDelete - Cancellazione di una Azienda
		/// <summary>
		/// company_OnDelete
		/// </summary>
		//---------------------------------------------------------------------
		private void company_OnDelete(object sender, string id, DBMSType providerType)
		{
			//metto la form in view in modo che non mi dia fastidio
			if (workingAreaConsole.Controls.Count != 0 && workingAreaConsole.Controls[0] is PlugInsForm)
				((PlugInsForm)workingAreaConsole.Controls[0]).State = StateEnums.View;

			Cursor currentConsoleFormCursor = consoleTree.TopLevelControl.Cursor;
			SetProgressBarMaxValueFromPlugIn(sender, 100);

			bool canDelete = false;
			// chiedo al LoginManager l'autenticazione per procedere con la cancellazione dell'Azienda
			if (OnDeleteCompanyToLoginManager != null)
				canDelete = OnDeleteCompanyToLoginManager(sender, Convert.ToInt32(id));

			if (!canDelete)
			{
				// se non è stata fornita un'autenticazione valida visualizzo un msg e non procedo con l'elaborazione
				consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
				diagnostic.Set(DiagnosticType.Error, Strings.AuthenticationTokenNotValid);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				return;
			}

			if (consoleEnvironmentInfo.IsLiteConsole)
			{
				CompanyLite deleteCompanyLite = new CompanyLite
				   (
				   currentStatus.OwnerDbName,
				   currentStatus.ConnectionString,
				   currentStatus.CurrentConnection,
				   id,
				   pathFinder,
				   licenceInfo
				   );
				deleteCompanyLite.OnCallHelp += new CompanyLite.CallHelp(HelpFromPopUp);
				deleteCompanyLite.OnModifyTree += new CompanyLite.ModifyTree(consoleTree_OnDeleteCompany);
				deleteCompanyLite.OnEnableProgressBar += new CompanyLite.EnableProgressBar(EnableProgressBarFromPlugIn);
				deleteCompanyLite.OnDisableProgressBar += new CompanyLite.DisableProgressBar(DisableProgressBarFromPlugIn);
				deleteCompanyLite.OnSetProgressBarStep += new CompanyLite.SetProgressBarStep(SetProgressBarStepFromPlugIn);
				deleteCompanyLite.OnSetProgressBarText += new CompanyLite.SetProgressBarText(SetProgressBarTextFromPlugIn);
				deleteCompanyLite.OnSetProgressBarValue += new CompanyLite.SetProgressBarValue(SetProgressBarValueFromPlugIn);
				deleteCompanyLite.OnGetUserAuthenticatedPwdFromConsole += new CompanyLite.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
				deleteCompanyLite.OnIsUserAuthenticatedFromConsole += new CompanyLite.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
				deleteCompanyLite.OnAddUserAuthenticatedFromConsole += new CompanyLite.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
				deleteCompanyLite.OnAfterDeleteCompany += new CompanyLite.AfterDeleteCompany(AfterDeleteCompany);
				deleteCompanyLite.OnEnableSaveButton += new EventHandler(OnEnableSaveToolBarButton);
				deleteCompanyLite.OnIsActivated += new CompanyLite.IsActivated(IsFunctionalityActivated);

				if (!deleteCompanyLite.Diagnostic.Error)
				{
					bool deleted = false;

					switch (providerType)
					{
						case DBMSType.SQLSERVER:
							deleted = deleteCompanyLite.DeleteSqlData(sender, new System.EventArgs());
							break;
					}

					if (deleted)
					{
						//informo il login manager della cancellazione
						if (OnDeleteCompanyToLoginManager != null)
							OnDeleteCompanyToLoginManager(sender, Convert.ToInt32(id));
						if (OnUnlockForCompanyDBName != null && (deleteCompanyLite.NameOfCompanyDb.Length > 0))
							OnUnlockForCompanyDBName(sender, deleteCompanyLite.NameOfCompanyDb);
						if (OnTraceAction != null)
							OnTraceAction(deleteCompanyLite.NameOfCompany, string.Empty, TraceActionType.DeleteCompany, DatabaseLayerConsts.MicroareaConsole);
						consoleTree_OnDeleteCompany(sender, ConstString.containerCompanies);
					}
				}
				else
				{
					diagnostic.Set(DiagnosticType.Error, Strings.CannotDeleteCompany);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
				}
			}

			else
			{
			Company deleteCompany = new Company
				(
				currentStatus.OwnerDbName,
				currentStatus.ConnectionString,
				currentStatus.CurrentConnection,
				id,
				pathFinder,
				licenceInfo
				);
			deleteCompany.OnCallHelp += new Company.CallHelp(HelpFromPopUp);
			deleteCompany.OnModifyTree += new Company.ModifyTree(consoleTree_OnDeleteCompany);
			deleteCompany.OnEnableProgressBar += new Company.EnableProgressBar(EnableProgressBarFromPlugIn);
			deleteCompany.OnDisableProgressBar += new Company.DisableProgressBar(DisableProgressBarFromPlugIn);
			deleteCompany.OnSetProgressBarStep += new Company.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			deleteCompany.OnSetProgressBarText += new Company.SetProgressBarText(SetProgressBarTextFromPlugIn);
			deleteCompany.OnSetProgressBarValue += new Company.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			deleteCompany.OnGetUserAuthenticatedPwdFromConsole += new Company.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			deleteCompany.OnIsUserAuthenticatedFromConsole += new Company.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			deleteCompany.OnAddUserAuthenticatedFromConsole += new Company.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			deleteCompany.OnAfterDeleteCompany += new Company.AfterDeleteCompany(AfterDeleteCompany);
			deleteCompany.OnDisableSaveButton += new EventHandler(OnDisableSaveToolBarButton);
			deleteCompany.OnEnableSaveButton += new EventHandler(OnEnableSaveToolBarButton);
			deleteCompany.OnIsActivated += new Company.IsActivated(IsFunctionalityActivated);

			if (!deleteCompany.Diagnostic.Error)
			{
				bool deleted = false;

				switch (providerType)
				{
					case DBMSType.SQLSERVER:
						deleted = deleteCompany.DeleteSqlData(sender, new System.EventArgs());
						break;
					case DBMSType.ORACLE:
						deleted = deleteCompany.DeleteOracleData(sender, new System.EventArgs());
						break;
                    case DBMSType.POSTGRE:
                        deleted = deleteCompany.DeletePostgreData(sender, new System.EventArgs());
                        break;
				}

				if (deleted)
				{
					//informo il login manager della cancellazione
					if (OnDeleteCompanyToLoginManager != null)
						OnDeleteCompanyToLoginManager(sender, Convert.ToInt32(id));
					if (OnUnlockForCompanyDBName != null && (deleteCompany.NameOfCompanyDb.Length > 0))
						OnUnlockForCompanyDBName(sender, deleteCompany.NameOfCompanyDb);
					if (OnTraceAction != null)
						OnTraceAction(deleteCompany.NameOfCompany, string.Empty, TraceActionType.DeleteCompany, DatabaseLayerConsts.MicroareaConsole);
					consoleTree_OnDeleteCompany(sender, ConstString.containerCompanies);
				}
			}
			else
			{
				diagnostic.Set(DiagnosticType.Error, Strings.CannotDeleteCompany);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			}
		}
		}
		#endregion

		#region role_OnDelete - Cancellazione di un Ruolo di una Azienda
		/// <summary>
		/// role_OnDelete
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void role_OnDelete(object sender, string id, string companyId)
		{
            PlugInTreeNode node = null;
            if (sender is PlugInTreeNode)
                node = (PlugInTreeNode)sender;

            //metto la form in view in modo che non mi dia fastidio
            if (workingAreaConsole.Controls.Count != 0 && workingAreaConsole.Controls[0] is PlugInsForm)
				((PlugInsForm)workingAreaConsole.Controls[0]).State = StateEnums.View;


            if (workingAreaConsole.Controls.Count > 0 && workingAreaConsole.Controls[0] != null)
            {
                object o = workingAreaConsole.Controls[0];
                if (string.Compare(o.GetType().Name, "Role", true, CultureInfo.InvariantCulture) == 0)
                {
                    Role deleteRole = (Role)workingAreaConsole.Controls[0];

                    if (node != null && (deleteRole.Name == NameSolverStrings.EasyStudioDeveloperRole || deleteRole.Name == NameSolverStrings.ReportEditorRole))
                        return;

                    DialogResult askIfContinue = DiagnosticViewer.ShowQuestion(Strings.AskIfDeleteRole, Strings.Delete);
                    if (askIfContinue == DialogResult.Yes)
                        deleteRole.Delete(sender, new System.EventArgs());
                }
                else
                {
                    if (node != null && (node.Text == NameSolverStrings.EasyStudioDeveloperRole || node.Text == NameSolverStrings.ReportEditorRole))
                        return;

                    DialogResult askIfContinue = DiagnosticViewer.ShowQuestion(Strings.AskIfDeleteRole, Strings.Delete);
                    if (askIfContinue == DialogResult.No)
                        return;

                    RoleDb dbRole = new RoleDb();
                    dbRole.ConnectionString = currentStatus.ConnectionString;
                    dbRole.CurrentSqlConnection = currentStatus.CurrentConnection;
                    if (!dbRole.Delete(id, companyId))
                    {
                        if (dbRole.Diagnostic.Error || dbRole.Diagnostic.Information || dbRole.Diagnostic.Warning)
                        {
                            diagnostic.Set(dbRole.Diagnostic);
                            DiagnosticViewer.ShowDiagnostic(diagnostic);
                        }
                    }
                    else
                        consoleTree_onModifyRole(sender, ConstString.containerCompanyRoles, companyId);
                }
            }
            else
            {
                if (node != null && (node.Text == NameSolverStrings.EasyStudioDeveloperRole || node.Text == NameSolverStrings.ReportEditorRole))
                    return;

                DialogResult askIfContinue = DiagnosticViewer.ShowQuestion(Strings.AskIfDeleteRole, Strings.Delete);
                if (askIfContinue == DialogResult.No)
                    return;

                RoleDb dbRole = new RoleDb();
                dbRole.ConnectionString = currentStatus.ConnectionString;
                dbRole.CurrentSqlConnection = currentStatus.CurrentConnection;
                if (!dbRole.Delete(id, companyId))
                {
                    if (dbRole.Diagnostic.Error || dbRole.Diagnostic.Information || dbRole.Diagnostic.Warning)
                    {
                        diagnostic.Set(dbRole.Diagnostic);
                        DiagnosticViewer.ShowDiagnostic(diagnostic);
                    }
                }
                else
                    consoleTree_onModifyRole(sender, ConstString.containerCompanyRoles, companyId);
            }

		}
		#endregion

		#region company_OnDeleteCompanyUser - Cancellazione di un Utente di un'Azienda
		/// <summary>
		/// company_OnDeleteCompanyUser
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		/// <param name="companyId"></param>
		//---------------------------------------------------------------------
		private void company_OnDeleteCompanyUser(object sender, string id, string companyId, DBMSType providerType)
		{
			Cursor currentConsoleFormCursor = consoleTree.TopLevelControl.Cursor;
			//metto la form in view in modo che non mi dia fastidio
			if (workingAreaConsole.Controls.Count != 0 && workingAreaConsole.Controls[0] is PlugInsForm)
				((PlugInsForm)workingAreaConsole.Controls[0]).State = StateEnums.View;

			if (DiagnosticViewer.ShowQuestion(Strings.AskIfDeleteCompanyUser, Strings.Delete) == DialogResult.No)
			{
				consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
				return;
			}

			// chiedo al LoginManager l'autenticazione per procedere con la cancellazione dell'associazione Utente-Azienda
				bool canDelete = false;
				if (OnDeleteAssociationToLoginManager != null)
					canDelete = OnDeleteAssociationToLoginManager(sender, Convert.ToInt32(id), Convert.ToInt32(companyId));
				if (!canDelete)
				{
					// se non è stata fornita un'autenticazione valida visualizzo un msg e non procedo con l'elaborazione
					consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
					diagnostic.Set(DiagnosticType.Error, Strings.AuthenticationTokenNotValid);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					return;
				}
			//

				CompanyUserDb dbCompanyUser = new CompanyUserDb();
				dbCompanyUser.ConnectionString = currentStatus.ConnectionString;
				dbCompanyUser.CurrentSqlConnection = currentStatus.CurrentConnection;

			// se l'utente applicativo e' il dbowner non lo posso eliminare
			if (dbCompanyUser.IsDbo(id, companyId))
				{
				consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
				diagnostic.Set(DiagnosticType.Warning, DatabaseItemsStrings.CannotDeleteDbo);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				return;
			}

					ArrayList userCompany = new ArrayList();
			if (dbCompanyUser.GetUserCompany(out userCompany, id, companyId) && userCompany.Count > 0)
						{
				

				string userName = ((CompanyUser)userCompany[0]).Login;
							string dbUser = ((CompanyUser)userCompany[0]).DBDefaultUser;
							string dbPassword = ((CompanyUser)userCompany[0]).DBDefaultPassword;
							bool dbWinAut = ((CompanyUser)userCompany[0]).DBWindowsAuthentication;

							CompanyDb companyDb = new CompanyDb();
							companyDb.ConnectionString = currentStatus.ConnectionString;
							companyDb.CurrentSqlConnection = currentStatus.CurrentConnection;

							ArrayList companyData = new ArrayList();
							companyDb.GetAllCompanyFieldsById(out companyData, companyId);

				if (companyData.Count > 0)
				{
					CompanyItem companyItem = (CompanyItem)companyData[0];
					string companyName = companyItem.Company;

					//-----------------------------------------------------------------------------
					// solo se NON sono in versione Lite provo anche ad eliminare la login da SQL -
					//-----------------------------------------------------------------------------
					if (!consoleEnvironmentInfo.IsLiteConsole)
					{
							switch (providerType)
							{
								case (DBMSType.ORACLE):
									{
									string oracleService = companyItem.DbServer;
									string oracleDbName = companyItem.DbName;
									string oracleCompanyName = companyItem.Company;
											companyName = oracleCompanyName;

										//cancello i sinonimi
										if (!string.IsNullOrWhiteSpace(oracleService) && !string.IsNullOrWhiteSpace(oracleDbName))
										{
											if (dbCompanyUser.CanDropLogin(dbUser, companyId))
											{
											DetailOracleCompanyUser detailOracleCompanyUser = new DetailOracleCompanyUser(currentStatus.ConnectionString, currentStatus.CurrentConnection, companyId, id, userName);
												detailOracleCompanyUser.OnCallHelpFromPopUp += new DetailOracleCompanyUser.CallHelpFromPopUp(HelpFromPopUp);
												detailOracleCompanyUser.OnModifyTreeOfCompanies += new DetailOracleCompanyUser.ModifyTreeOfCompanies(consoleTree_OnModifyUserCompany);
												detailOracleCompanyUser.OnAddUserAuthenticatedFromConsole += new DetailOracleCompanyUser.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
												detailOracleCompanyUser.OnGetUserAuthenticatedPwdFromConsole += new DetailOracleCompanyUser.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
												detailOracleCompanyUser.OnIsUserAuthenticatedFromConsole += new DetailOracleCompanyUser.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
												detailOracleCompanyUser.OnEnableProgressBar += new DetailOracleCompanyUser.EnableProgressBar(EnableProgressBarFromPlugIn);
												detailOracleCompanyUser.OnDisableProgressBar += new DetailOracleCompanyUser.DisableProgressBar(DisableProgressBarFromPlugIn);
												detailOracleCompanyUser.OnSetProgressBarStep += new DetailOracleCompanyUser.SetProgressBarStep(SetProgressBarStepFromPlugIn);
												detailOracleCompanyUser.OnSetProgressBarText += new DetailOracleCompanyUser.SetProgressBarText(SetProgressBarTextFromPlugIn);
												detailOracleCompanyUser.OnSetProgressBarValue += new DetailOracleCompanyUser.SetProgressBarValue(SetProgressBarValueFromPlugIn);

												if (!detailOracleCompanyUser.DropSynonyms(oracleDbName, oracleService, dbUser, dbPassword, dbWinAut))
												{
													consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
													diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UnableToDropSynonyms, dbUser, oracleCompanyName, oracleDbName));
													DiagnosticViewer.ShowDiagnostic(diagnostic);
												}
											}
										}
										else
										{
											consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
											diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UnableToDropSynonyms, dbUser, oracleCompanyName, oracleDbName));
											DiagnosticViewer.ShowDiagnostic(diagnostic);
										}
										break;
									}
								case DBMSType.SQLSERVER:
									{
									bool removeSlaveLogin = false;
									string sqlDbName = companyItem.DbName;
									string sqlCompanyName = companyItem.Company;
											companyName = sqlCompanyName;
									bool useDbSlave = companyItem.UseDBSlave;

										// se l'azienda usa gli slave devo rimuovere eventualmente la login
										if (useDbSlave)
										{
											SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
											slaveLoginDb.ConnectionString = currentStatus.ConnectionString;
											slaveLoginDb.CurrentSqlConnection = currentStatus.CurrentConnection;
											removeSlaveLogin = slaveLoginDb.CanDropLogin(dbUser, companyId);
										}

									// Cancellazione login associata al db aziendale su SQL Server (ed eventualmente sul DMS)
										if (dbCompanyUser.CanDropLogin(dbUser, companyId))
										{
										DetailCompanyUser detailSqlCompanyUser = new DetailCompanyUser(currentStatus.ConnectionString, currentStatus.CurrentConnection, companyId, id, userName);
											detailSqlCompanyUser.OnCallHelpFromPopUp += new DetailCompanyUser.CallHelpFromPopUp(HelpFromPopUp);
											detailSqlCompanyUser.OnAddUserAuthenticatedFromConsole += new DetailCompanyUser.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
											detailSqlCompanyUser.OnGetUserAuthenticatedPwdFromConsole += new DetailCompanyUser.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
											detailSqlCompanyUser.OnIsUserAuthenticatedFromConsole += new DetailCompanyUser.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
											detailSqlCompanyUser.OnIsActivated += new DetailCompanyUser.IsActivated(IsFunctionalityActivated);

											if (!detailSqlCompanyUser.RevokeCompanyLogin(dbUser, companyId))
											{
												consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
												diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UnableToDropSqlLogin, dbUser, sqlDbName, sqlCompanyName));
												DiagnosticViewer.ShowDiagnostic(diagnostic);
											}

											// rimuovo anche la login dal database dms
											if (useDbSlave && removeSlaveLogin)
											{
												if (!detailSqlCompanyUser.RevokeDmsLogin(dbUser, companyId))
												{
													consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
													diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UnableToDropSqlLogin, dbUser, sqlDbName, sqlCompanyName));
													DiagnosticViewer.ShowDiagnostic(diagnostic);
												}
											}
										}
										break;
									}
                                case DBMSType.POSTGRE:
                                    {
									bool removeSlaveLogin = false;
									string sqlDbName = companyItem.DbName;
									string sqlCompanyName = companyItem.Company;
                                            companyName = sqlCompanyName;
									bool useDbSlave = companyItem.UseDBSlave;

                                        // se l'azienda usa gli slave devo rimuovere eventualmente la login
                                        if (useDbSlave)
                                        {
                                            SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
                                            slaveLoginDb.ConnectionString = currentStatus.ConnectionString;
                                            slaveLoginDb.CurrentSqlConnection = currentStatus.CurrentConnection;
                                            removeSlaveLogin = slaveLoginDb.CanDropLogin(dbUser, companyId);
                                        }

                                        // Cancellazione login associata al db aziendale su SQL Server (ed eventualmente sul Easy Attachment)
                                        if (dbCompanyUser.CanDropLogin(dbUser, companyId))
                                        {
										DetailCompanyUserPostgre detailSqlCompanyUser = new DetailCompanyUserPostgre(currentStatus.ConnectionString, currentStatus.CurrentConnection, companyId, id, userName);
                                            detailSqlCompanyUser.OnCallHelpFromPopUp += new DetailCompanyUserPostgre.CallHelpFromPopUp(HelpFromPopUp);
                                            detailSqlCompanyUser.OnAddUserAuthenticatedFromConsole += new DetailCompanyUserPostgre.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
                                            detailSqlCompanyUser.OnGetUserAuthenticatedPwdFromConsole += new DetailCompanyUserPostgre.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
                                            detailSqlCompanyUser.OnIsUserAuthenticatedFromConsole += new DetailCompanyUserPostgre.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
                                            detailSqlCompanyUser.OnIsActivated += new DetailCompanyUserPostgre.IsActivated(IsFunctionalityActivated);

                                            if (!detailSqlCompanyUser.RevokeCompanyLogin(dbUser, companyId))
                                            {
                                                consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
                                                diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UnableToDropSqlLogin, dbUser, sqlDbName, sqlCompanyName));
                                                DiagnosticViewer.ShowDiagnostic(diagnostic);
                                            }

                                            // rimuovo anche la login dal database dms
                                            //if (useDbSlave && removeSlaveLogin)
                                            //{
                                            //    if (!detailSqlCompanyUser.RevokeDmsLogin(dbUser, companyId))
                                            //    {
                                            //        consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
                                            //        diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UnableToDropSqlLogin, dbUser, sqlDbName, sqlCompanyName));
                                            //        DiagnosticViewer.ShowDiagnostic(diagnostic);
                                            //    }
                                            //}
                                        }
                                        break;
                                    }
							}		
					}
							
					// eliminazione anagrafica utente azienda/DMS
					if (dbCompanyUser.Delete(id, companyId))
							{
								//comunico al LoginManager che ho cancellato l'associazione Utente-Azienda
								if (OnDeleteAssociationToLoginManager != null)
									OnDeleteAssociationToLoginManager(sender, Convert.ToInt32(id), Convert.ToInt32(companyId));
								//comunico al LockManager che ho cancellato l'associazione Utente-Azienda
								if (OnUnlockAllForUser != null)
									OnUnlockAllForUser(sender, userName);
								if (OnTraceAction != null)
									OnTraceAction(companyName, userName, TraceActionType.DeleteCompanyUser, DatabaseLayerConsts.MicroareaConsole);

								consoleTree_OnModifyUserCompany(sender, ConstString.containerCompanyUsers, companyId);
								consoleTree_onModifyRole(sender, ConstString.containerCompanyRoles, companyId);

								if (OnAfterDeleteCompanyUser != null)
									OnAfterDeleteCompanyUser(sender, id, companyId);
							}
				}
				}

				consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
			}
		#endregion

		#region company_OnDeleteCompanyUserRole - Cancellazione di un Utente di un Ruolo di una Azienda
		/// <summary>
		/// company_OnDeleteCompanyUserRole
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="loginId"></param>
		/// <param name="companyId"></param>
		/// <param name="roleId"></param>
		//---------------------------------------------------------------------
		private void company_OnDeleteCompanyUserRole(object sender, string loginId, string companyId, string roleId)
		{
			//metto la form in view in modo che non mi dia fastidio
			if (workingAreaConsole.Controls.Count != 0 && workingAreaConsole.Controls[0] is PlugInsForm)
				((PlugInsForm)workingAreaConsole.Controls[0]).State = StateEnums.View;

			DialogResult askIfContinue =
				DiagnosticViewer.ShowQuestion(Strings.AskIfDeleteCompanyUserRole, Strings.Delete);

			if (askIfContinue == DialogResult.Yes)
			{
				if (workingAreaConsole.Controls.Count > 0 && workingAreaConsole.Controls[0] != null)
				{
					object o = workingAreaConsole.Controls[0];
					if (string.Compare(o.GetType().Name, "DetailUserRole", true, CultureInfo.InvariantCulture) == 0)
					{
						DetailUserRole deleteCompanyUserRole = (DetailUserRole)workingAreaConsole.Controls[0];
						deleteCompanyUserRole.Delete(sender, new System.EventArgs());
					}
				}
				else
				{
					CompanyRoleLoginDb dbCompanyRoleLogin = new CompanyRoleLoginDb();
					dbCompanyRoleLogin.ConnectionString = currentStatus.ConnectionString;
					dbCompanyRoleLogin.CurrentSqlConnection = currentStatus.CurrentConnection;
					dbCompanyRoleLogin.Delete(loginId, companyId, roleId);
					consoleTree_OnModifyUserCompany(sender, ConstString.containerCompanyRoles, companyId);
				}
			}
		}
		#endregion

		#endregion
		#endregion

		#region OnAfterConnectButton - Premuto il bottone Connect della Microarea Console
		/// <summary>
		/// Intercetta la pressione del bottone Connect dalla Console
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnConnect")]
		public void OnAfterConnectButton(object sender, System.EventArgs e)
		{
			OnClickLogOn(sender, e);
		}
		#endregion

		#region OnAfterDisconnectButton - Premuto il bottone Disconnect della Microarea Console
		/// <summary>
		/// OnAfterDisconnectButton
		/// Intercetta la pressione del bottone Disconnect dalla Console
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnDisconnect")]
		public void OnAfterDisconnectButton(object sender, System.EventArgs e)
		{
			if (workingAreaConsole.Controls.Count > 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
				if (o is PlugInsForm)
				{
					if (((PlugInsForm)o).State != StateEnums.View && ((PlugInsForm)o).State != StateEnums.None)
					{
						diagnosticViewer.Message = string.Format(Strings.ConfirmExit, Strings.NodeRoot);
						diagnosticViewer.Title = Strings.Disconnect;
						diagnosticViewer.ShowButtons = MessageBoxButtons.OKCancel;
						diagnosticViewer.ShowIcon = MessageBoxIcon.Information;
						diagnosticViewer.DefaultButton = MessageBoxDefaultButton.Button2;
						DialogResult exitResult = diagnosticViewer.Show();
						if (exitResult == DialogResult.OK)
						{
							OnClickLogOff(sender, e);
							if (OnEnableConnectToolBarButton != null) OnEnableConnectToolBarButton(sender, e);
						}
					}
					else
					{
						OnClickLogOff(sender, e);
						if (OnEnableConnectToolBarButton != null) OnEnableConnectToolBarButton(sender, e);
					}
				}
				else
				{
					OnClickLogOff(sender, e);
					if (OnEnableConnectToolBarButton != null) OnEnableConnectToolBarButton(sender, e);
				}
			}
			else
			{
				OnClickLogOff(sender, e);
				if (OnEnableConnectToolBarButton != null) OnEnableConnectToolBarButton(sender, e);
			}

			SetProgressBarTextFromPlugIn(sender, string.Empty);
			DisableProgressBarFromPlugIn(sender);
		}
		#endregion

		#region OnAfterClickMessagesButton
		/// <summary>
		/// OnAfterClickMessagesButtons
		/// Risponde all'evento lanciato dalla console OnGetPlugInMessages
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnGetPlugInMessages")]
		public void OnAfterClickMessagesButton(object sender, Diagnostic diagnosticFromConsole)
		{
			if (diagnostic.Warning || diagnostic.Error || diagnostic.Information)
				diagnosticFromConsole.Set(diagnostic);
		}
		#endregion

		#region CheckIfEditionAndDBNetworkAreCorrect
		/// <summary>
		/// CheckIfEditionAndDBNetworkAreCorrect
		/// se la versione del server è diversa da MSDE devo effettuare ulteriori controlli:
		/// - se si tratta di Standard Edition
		/// - se la licenza è Small Network
		/// </summary>
		//---------------------------------------------------------------------
		public bool CheckIfEditionAndDBNetworkAreCorrect(string connectionString, string serverName)
		{
            return true;/* 6176 dal 04022016 non si vuole più obbligare a sqlexpress o msde in caso di small, ma si bloccherà solo la dimensione a 2 giga.
			bool res = false;

			// se la versione del db è diversa da MSDE
			if (TBCheckDatabase.GetDatabaseVersion(connectionString, NameSolverDatabaseStrings.SQLOLEDBProvider)
				!= DatabaseVersion.MSDE)
			{
				// se l'edizione è standard o se la licenza è SmallNetwork
				if (string.Compare(this.licenceInfo.Edition, NameSolverStrings.StandardEdition, true, CultureInfo.InvariantCulture) == 0 ||
					(licenceInfo.DBNetworkType == DBNetworkType.Small))
				{
					res = false;
					diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.WrongSQLDatabaseVersion);
				}
				else
					res = true;

				// visualizzo i msg solo se i controlli non sono andati a buon fine
				if (!res)
					DiagnosticViewer.ShowDiagnostic(diagnostic);
			}
			else
				res = true;

			return res;*/
		}
		#endregion

		#endregion
	}
}
