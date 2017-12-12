using System;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.SQLDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// AddLoginToCompany
	/// </summary>
	//=========================================================================
	public partial class AddLoginToCompany : PlugInsForm
	{

		#region Events and Delegates
		public delegate void	ModifyTreeOfCompanies(object sender, string nodeType,string companyId);
		public event			ModifyTreeOfCompanies OnModifyTreeOfCompanies;

		public delegate void	SendDiagnostic(object sender, Diagnostic diagnostic);
		public event			SendDiagnostic OnSendDiagnostic;

		public delegate bool	IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event			IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;

		public delegate void	AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event			AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;

		public delegate string	GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event			GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;

		public delegate void    CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event			CallHelpFromPopUp OnCallHelpFromPopUp;
		#endregion

		#region Private Variables
		private SqlConnection currentConnection;

		private string	connectionString;
		private string	companyId;
		private string	dbOwnerLogin	= string.Empty;
		private string	dbOwnerPassword = string.Empty;
		private bool	dbOwnerWinAuth  = false;
		private string	dbOwnerDomain   = string.Empty;
		private string	dbOwnerPrimary  = string.Empty;
		private string	dbOwnerIstance  = string.Empty;
		private string	dbCompanyName   = string.Empty;
		private string	dbCompanyServer = string.Empty;
		
		Diagnostic diagnostic = new Diagnostic("SystemAdminPluIn.AddLoginToCompany");
		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public AddLoginToCompany()
		{
			InitializeComponent();
			State = StateEnums.View;
		}

		/// <summary>
		/// Costruttore (con parametri)
		/// </summary>
		//---------------------------------------------------------------------
		public AddLoginToCompany(string	connectionString, SqlConnection	connection, string companyId)
		{
			InitializeComponent();

			this.companyId			= companyId;
			this.currentConnection	= connection;
			this.connectionString	= connectionString;
			State = StateEnums.View;
		}
		#endregion

		#region LoadCompanyData
		/// <summary>
		/// LoadCompanyData
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadCompanyData()
		{
			ArrayList companyData = new ArrayList();
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;

			if (companyDb.GetAllCompanyFieldsById(out companyData, this.companyId))
			{
				CompanyItem companyItem = (CompanyItem)companyData[0];
				LabelTitle.Text			= string.Format(Strings.TitleAddLoginToCompany, companyItem.DbServer);
			}
			else
			{
				diagnostic.Set(companyDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}
		}
		#endregion

		#region CheckData - Verifica l'input dell'utente
		/// <summary>
		/// CheckData
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool CheckData()
		{
			bool correct = true;
		
			if (TextLoginName.Text.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.NoEmptyValue);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				correct = false;
			}

			//verifico la pwd
			ConfirmPassword confirmPassword = new ConfirmPassword();
			confirmPassword.OldPassword		= TextLoginPassword.Text;
			confirmPassword.Focus();
			confirmPassword.ShowDialog();
			if (confirmPassword.DialogResult == DialogResult.OK)
			{
				if (confirmPassword.Diagnostic.Error || confirmPassword.Diagnostic.Warning || confirmPassword.Diagnostic.Information)
				{
					diagnostic.Set(confirmPassword.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					correct = false;
				}
			}
			else
				correct = false;

			return correct;
		}
		#endregion

		#region BuildListView
		/// <summary>
		/// BuildListView
		/// Costruisco il layout della ListView
		/// </summary>
		//----------------------------------------------------------------------
		private void BuildListView()
		{
			//preparo la lista 
			ListViewLogins.Clear();
			ListViewLogins.View					= View.Details;
			ListViewLogins.CheckBoxes			= false;
			ListViewLogins.AllowColumnReorder	= true;
			ListViewLogins.Activation			= ItemActivation.OneClick;
			ListViewLogins.Columns.Add(Strings.User,				-2,	HorizontalAlignment.Left);
			ListViewLogins.Columns.Add(Strings.DBAuthentication,	-2, HorizontalAlignment.Left);
		}
		#endregion

		#region LoadLogins - Carica nella ListView le logins esistenti
		/// <summary>
		/// LoadLogins
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadLogins()
		{
			//carico le login del server
			string buildedStringConnection = string.Empty;
			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole		+= new TransactSQLAccess.AddUserAuthenticatedFromConsole	(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole	+= new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole	(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole		+= new TransactSQLAccess.IsUserAuthenticatedFromConsole		(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp						+= new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
			buildedStringConnection = SettingStringConnection(this.companyId);
			if (buildedStringConnection.Length == 0)
			{
				//non riesco a connettermi all'azienda.
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				State = StateEnums.Editing;
				return;
			}

			connSqlTransact.CurrentStringConnection = buildedStringConnection;
			//eventualmente eseguo l'impersonificazione
			dataToConnectionServer = connSqlTransact.LoginImpersonification
				(
				this.dbOwnerLogin,
				this.dbOwnerPassword,
				this.dbOwnerDomain,
				this.dbOwnerWinAuth,
				this.dbOwnerPrimary,
				this.dbOwnerIstance,
				false
				);
			if (dataToConnectionServer == null) 
			{
				Cursor.Current = Cursors.Default;
				State = StateEnums.Editing;
				return;
			}
			//sono sull'azienda, leggo le login
			ArrayList loginsOfCompany = new ArrayList();
			if (!connSqlTransact.GetLogins(out loginsOfCompany))
			{
				//TODO GESTIRE
				diagnostic.Set(DiagnosticType.Error, Strings.NotExistLogins);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				State = StateEnums.Editing;
				Cursor.Current = Cursors.Default;
				if (dataToConnectionServer != null)
					dataToConnectionServer.Undo();
				return;
			}
			if (loginsOfCompany.Count > 0)
			{
				for (int i=0; i < loginsOfCompany.Count; i++)
				{
					ListViewItem item = new ListViewItem();
					CompanyLogin  companyLogin = (CompanyLogin)loginsOfCompany[i];
					//se è un gruppo NT, un utente NT locale o l'utente sa lo skippo
					if (companyLogin.IsNTGroup || companyLogin.IsLocalNTUser || companyLogin.IsSaUser)
						continue;
					item.Text = companyLogin.Login;
					item.SubItems.Add((companyLogin.IsNTUser) ? Strings.Yes : Strings.No);
					ListViewLogins.Items.Add(item);
				}
			}
		}
		#endregion

		/// <summary>
		/// AddLogin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		public bool AddLogin(object sender, EventArgs e)
		{
			string loginName = TextLoginName.Text;
			string loginPassword = string.Empty, buildedStringConnection = string.Empty;
			bool   result = false;

			if (CheckData())
			{
				TransactSQLAccess connSqlTransact = new TransactSQLAccess();
				connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
				connSqlTransact.OnAddUserAuthenticatedFromConsole		+= new TransactSQLAccess.AddUserAuthenticatedFromConsole	(AddUserAuthentication);
				connSqlTransact.OnGetUserAuthenticatedPwdFromConsole	+= new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole	(GetUserAuthenticatedPwd);
				connSqlTransact.OnIsUserAuthenticatedFromConsole		+= new TransactSQLAccess.IsUserAuthenticatedFromConsole		(IsUserAuthenticated);
				connSqlTransact.OnCallHelpFromPopUp						+= new TransactSQLAccess.CallHelpFromPopUp(CallHelp);
			
				UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
				loginPassword = TextLoginPassword.Text;

				//mi connetto all'azienda
				buildedStringConnection = SettingStringConnection(this.companyId);
				if (buildedStringConnection.Length == 0)
				{
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					//diagnostic.Clear();
					return false;
				}

				connSqlTransact.CurrentStringConnection  = buildedStringConnection;
				//eventualmente eseguo l'impersonificazione
				dataToConnectionServer = connSqlTransact.LoginImpersonification
					(
					this.dbOwnerLogin,
					this.dbOwnerPassword,
					this.dbOwnerDomain,
					this.dbOwnerWinAuth,
					this.dbOwnerPrimary,
					this.dbOwnerIstance,
					false
					);
				
				if (dataToConnectionServer == null) 
				{
					Cursor.Current = Cursors.Default;
					State = StateEnums.Editing;
					return false;
				}

				//se la login non esiste la devo prima creare
				if (!connSqlTransact.ExistLogin(loginName))
				{
					//Gestione errore
					if (!connSqlTransact.SPAddLogin(loginName, loginPassword, DatabaseLayerConsts.MasterDatabase))
					{
						if (dataToConnectionServer != null)
							dataToConnectionServer.Undo();
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotAddLogin, loginName));
						diagnostic.Set(connSqlTransact.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						State = StateEnums.Editing;
						return false;
					}
					else
					{
						//se è andato tutto bene
						if (OnModifyTreeOfCompanies != null)
							OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
						State = StateEnums.View;
					}
				}
				else
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.LoginAlreadyExist, loginName));
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					State = StateEnums.Editing;
				}
			}

			return result;
		}

		#region BtnSave_Click - Premuto il Bottone Aggiungi
		/// <summary>
		/// BtnSave_Click
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnSave_Click(object sender, EventArgs e)
		{
			AddLogin(sender, e);
		}
		#endregion

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

		#region SettingStringConnection
		/// <summary>
		/// SettingStringConnection
		/// Imposta la stringa di connessione
		/// </summary>
		//---------------------------------------------------------------------
		private string SettingStringConnection(string companyId)
		{
			string connectionToCompanyServer = string.Empty, dbOwnerId = string.Empty;
		
			//leggo i dati dall'azienda
			CompanyDb companyDb				= new CompanyDb();
			companyDb.ConnectionString		= this.connectionString;
			companyDb.CurrentSqlConnection  = this.currentConnection;
			
			ArrayList companyData = new ArrayList();
			if (!companyDb.GetAllCompanyFieldsById(out companyData, companyId))
			{
				diagnostic.Set(companyDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				companyData.Clear();
			}

			if (companyData.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)companyData[0];
				this.dbCompanyServer    = companyItem.DbServer;
				this.dbCompanyName      = companyItem.DbServer;
				dbOwnerId               = companyItem.DbOwner;
				
				//setto le info generali sul dbName (Istanza primaria o secondaria)
				string[] serverDbInformation = companyItem.DbServer.Split(Path.DirectorySeparatorChar);
				this.dbOwnerPrimary = serverDbInformation[0];
				if (serverDbInformation.Length > 1)
					this.dbOwnerIstance = serverDbInformation[1];
				this.dbCompanyName = companyItem.DbName;

				//Ora leggo le credenziali del dbo dal MSD_CompanyLogins
				CompanyUserDb companyUser = new CompanyUserDb();
				companyUser.ConnectionString = this.connectionString;
				companyUser.CurrentSqlConnection = this.currentConnection;
				ArrayList userDboData = new ArrayList();
				companyUser.GetUserCompany(out userDboData, dbOwnerId, companyId);
				if (userDboData.Count > 0)
				{
					CompanyUser companyDbo	= (CompanyUser)userDboData[0];
					this.dbOwnerLogin		= companyDbo.DBDefaultUser;
					this.dbOwnerPassword	= companyDbo.DBDefaultPassword;
					this.dbOwnerWinAuth     = companyDbo.DBWindowsAuthentication;

					//ora compongo la stringa di connessione
					if (this.dbOwnerWinAuth)
					{
						connectionToCompanyServer = string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, this.dbCompanyServer, DatabaseLayerConsts.MasterDatabase);
						this.dbOwnerDomain = Path.GetDirectoryName(this.dbOwnerLogin);
					}
					else
						connectionToCompanyServer = string.Format(NameSolverDatabaseStrings.SQLConnection, this.dbCompanyServer, DatabaseLayerConsts.MasterDatabase, this.dbOwnerLogin, this.dbOwnerPassword);
				}
			}
			return connectionToCompanyServer;
		}
		#endregion

		//---------------------------------------------------------------------
		public void LoadData()
		{
			LoadCompanyData();
			BuildListView();
			LoadLogins();
		}

		/// <summary>
		/// AddLoginToCompany_Closing
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void AddLoginToCompany_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// AddLoginToCompany_Deactivate
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void AddLoginToCompany_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// AddLoginToCompany_VisibleChanged
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void AddLoginToCompany_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}

		//---------------------------------------------------------------------
		private void CallHelp(object sender, string nameSpace, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, nameSpace, searchParameter);
		}

		#region Funzioni che mandano in Editing la Form
		//---------------------------------------------------------------------
		private void TextLoginName_TextChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void TextLoginPassword_TextChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}
		#endregion
	}
}