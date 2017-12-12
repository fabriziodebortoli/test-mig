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
	//=========================================================================
	public partial class LoginAdministrator : PlugInsForm
	{
		#region Varibili private
		private string connectionString;
		private SqlConnection currentConnection;
		private string companyId;
		private string providerId;
		private string dbOwnerLogin = string.Empty;
		private string dbOwnerPassword = string.Empty;
		private bool dbOwnerWinAuth = false;
		private string dbOwnerDomain = string.Empty;
		private string dbOwnerPrimary = string.Empty;
		private string dbOwnerIstance = string.Empty;
		private string dbCompanyName = string.Empty;
		private string dbCompanyServer = string.Empty;

		private Diagnostic diagnostic = new Diagnostic("SystemAdminPluIn.LoginAdministrator");
		#endregion

		#region Eventi e Delegati
		public delegate void ModifyTreeOfCompanies(object sender, string nodeType, string companyId);
		public event ModifyTreeOfCompanies OnModifyTreeOfCompanies;

		public delegate void ModifyTree(object sender, string nodeType);
		public event ModifyTree OnModifyTree;

		public delegate bool DeleteAssociation(object sender, int loginId, int companyId);
		public event DeleteAssociation OnDeleteAssociation;

		public delegate bool UnLockAllForUser(object sender, string userName);
		public event UnLockAllForUser OnUnLockAllForUser;

		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;

		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;

		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;

		public delegate void CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event CallHelpFromPopUp OnCallHelpFromPopUp;

		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;
		#endregion

		//---------------------------------------------------------------------
		public LoginAdministrator(string connectionString, SqlConnection connection, string companyId)
		{
			InitializeComponent();

			this.companyId = companyId;
			this.currentConnection = connection;
			this.connectionString = connectionString;

			ReadCompanyData();

			ComboExistLogins.Enabled = true;
			LblPassword.Enabled = false;
			TbPassword.Enabled = false;
			RbChangePassword.Enabled = false;
			TbNewPassword.Enabled = false;
			RbDeleteLogin.Enabled = false;
			BtnSave.Enabled = false;
		}

		#region Private methods
		/// <summary>
		/// Legge il nome dell'azienda e il server su cui risiede il suo db.
		/// </summary>
		//---------------------------------------------------------------------
		private void ReadCompanyData()
		{
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;
			ArrayList companyData = new ArrayList();

			bool result = companyDb.GetAllCompanyFieldsById(out companyData, this.companyId);
			if (!result)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				DiagnosticViewer.ShowDiagnostic(diagnostic);

				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}

				companyData.Clear();
			}

			string companyName = string.Empty, companyServer = string.Empty;

			if (companyData.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)companyData[0];
				companyName = companyItem.Company;
				companyServer = companyItem.DbServer;
			}

			LblExplication.Text = string.Format(LblExplication.Text, companyServer, companyName);
		}

		//---------------------------------------------------------------------
		private void ComboExistLogins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (ComboExistLogins.SelectedItem != null)
			{
				//se ho selezionato l'utente sa non posso cancellare la sua login
				//ma solo modificare la sua pwd
				LoginItem companyLogin = (LoginItem)ComboExistLogins.SelectedItem;
				if (string.Compare(companyLogin.LoginName, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
					RbDeleteLogin.Enabled = false;
				else
				{
					if (string.Compare(companyLogin.LoginName, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0 ||
						string.Compare(companyLogin.LoginName, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						RbChangePassword.Enabled = false;
						RbChangePassword.Checked = false;
						RbDeleteLogin.Enabled = true;
						RbDeleteLogin.Checked = true;
					}
					else
					{
						TbPassword.Text = string.Empty;
						TbNewPassword.Text = string.Empty;
						bool enabledPwd = !companyLogin.IsNTUser;
						TbPassword.Enabled = enabledPwd;
						LblPassword.Enabled = enabledPwd;
						RbChangePassword.Enabled = enabledPwd;
						TbNewPassword.Enabled = enabledPwd;
						RbChangePassword.Checked = enabledPwd;
						RbDeleteLogin.Checked = !enabledPwd;
						RbDeleteLogin.Enabled = true;
						BtnSave.Enabled = true;
					}
				}
			}
		}

		//---------------------------------------------------------------------
		private void RbChangePassword_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbChangePassword.Checked)
			{
				TbNewPassword.Enabled = true;
				TbNewPassword.Text = string.Empty;
			}
		}

		//---------------------------------------------------------------------
		private void RbDeleteLogin_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbDeleteLogin.Checked)
				TbNewPassword.Enabled = false;
		}

		//---------------------------------------------------------------------
		private void ComboExistLogins_DropDown(object sender, System.EventArgs e)
		{
			if (ComboExistLogins.Items.Count == 0)
				LoadExistLogins();
		}

		//---------------------------------------------------------------------
		private bool IsDbowner(string loginName)
		{
			//se è un dbowner di qualche azienda lo devo segare
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;

			return companyDb.IsDbOwner(loginName, this.providerId);
		}

		/// <summary>
		/// Carica le logins SQL del server su cui risiede l'azienda.
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadExistLogins()
		{
			//carico le login del server
			ComboExistLogins.Items.Clear();
			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

			string buildedStringConnection = BuildConnection(this.companyId);
			if (buildedStringConnection.Length == 0)
			{
				//non riesco a connettermi all'azienda.
				this.ComboExistLogins.Items.Clear();
				this.ComboExistLogins.Enabled = false;
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				return;
			}

			connSqlTransact.CurrentStringConnection = buildedStringConnection;

			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();

			//eventualmente eseguo l'impersonificazione
			dataToConnectionServer =
				connSqlTransact.LoginImpersonification
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
				//TO DO GESTIRE
				Cursor.Current = Cursors.Default;
				return;
			}

			//sono sull'azienda, leggo le login
			ArrayList loginsOfCompany = new ArrayList();
			bool success = connSqlTransact.GetLogins(out loginsOfCompany);
			if (!success)
			{
				//TO DO GESTIRE
				diagnostic.Set(DiagnosticType.Error, Strings.NotExistLogins);
				Cursor.Current = Cursors.Default;

				if (dataToConnectionServer != null)
					dataToConnectionServer.Undo();

				return;
			}

			if (loginsOfCompany.Count > 0)
			{
				foreach (CompanyLogin companyLogin in loginsOfCompany)
				{
					//se è un gruppo NT, un utente NT locale 
					// l'utente sa non lo skippo, perchè permetto di cambiargli la pwd
					// ma naturalmente non di cancellarlo
					if (companyLogin.IsNTGroup || companyLogin.IsLocalNTUser)
						continue;

					if (string.Compare(companyLogin.Login, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0 ||
						string.Compare(companyLogin.Login, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;

					//se è un dbowner di qualche azienda lo devo segare
					if (!IsDbowner(companyLogin.Login))
						ComboExistLogins.Items.Add(new LoginItem(companyLogin.Login, companyLogin.IsNTUser));
				}

				ComboExistLogins.DisplayMember = "loginName";
				ComboExistLogins.ValueMember = "isNTUser";

				//seleziono il primo
				if (ComboExistLogins.Items.Count > 0)
					ComboExistLogins.SelectedIndex = 0;
			}
		}

		/// <summary>
		/// Costruisco stringa di connessione.
		/// </summary>
		//---------------------------------------------------------------------
		private string BuildConnection(string companyId)
		{
			string connectionToCompanyServer = string.Empty;
			string dbOwnerId = string.Empty;

			//leggo i dati dall'azienda
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;
			ArrayList companyData = new ArrayList();

			bool result = companyDb.GetAllCompanyFieldsById(out companyData, companyId);
			if (!result)
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
				this.dbCompanyServer = companyItem.DbServer;
				this.dbCompanyName = companyItem.DbServer;
				dbOwnerId = companyItem.DbOwner;

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
					CompanyUser companyDbo = (CompanyUser)userDboData[0];
					this.dbOwnerLogin = companyDbo.DBDefaultUser;
					this.dbOwnerPassword = companyDbo.DBDefaultPassword;
					this.dbOwnerWinAuth = companyDbo.DBWindowsAuthentication;

					//ora compongo la stringa di connessione
					//ora compongo la stringa
					if (this.dbOwnerWinAuth)
					{
						connectionToCompanyServer =
							string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, this.dbCompanyServer, this.dbCompanyName);

						this.dbOwnerDomain = Path.GetDirectoryName(this.dbOwnerLogin);
					}
					else
						connectionToCompanyServer =
							string.Format(NameSolverDatabaseStrings.SQLConnection, this.dbCompanyServer, this.dbCompanyName, this.dbOwnerLogin, this.dbOwnerPassword);
				}
			}

			return connectionToCompanyServer;
		}

		///<summary>
		/// AddUserAuthenticated
		/// Aggiunge l'utente specificato alla lista degli utenti autenticati della Console
		///</summary>
		//---------------------------------------------------------------------
		private void AddUserAuthentication(string login, string password, string serverName, DBMSType dbType)
		{
			if (OnAddUserAuthenticatedFromConsole != null)
				OnAddUserAuthenticatedFromConsole(login, password, serverName, dbType);
		}

		///<summary>
		/// GetUserAuthenticatedPwd
		/// Richiede alla Console la pwd dell'utente già autenticato
		///</summary>
		//---------------------------------------------------------------------
		private string GetUserAuthenticatedPwd(string login, string serverName)
		{
			string pwd = string.Empty;

			if (OnGetUserAuthenticatedPwdFromConsole != null)
				pwd = OnGetUserAuthenticatedPwdFromConsole(login, serverName);

			return pwd;
		}

		///<summary>
		/// IsUserAuthenticated
		/// Richiede alla Console se l'utente specificato è stato già autenticato
		///</summary>
		//---------------------------------------------------------------------
		private bool IsUserAuthenticated(string login, string password, string serverName)
		{
			bool result = false;
			if (OnIsUserAuthenticatedFromConsole != null)
				result = OnIsUserAuthenticatedFromConsole(login, password, serverName);
			return result;
		}

		//---------------------------------------------------------------------
		private bool CheckData()
		{
			bool isOk = true;

			if (RbChangePassword.Checked)
			{
				if (ComboExistLogins.SelectedItem == null)
				{
					diagnostic.Set(DiagnosticType.Warning, Strings.NotSelectedLogins);
					isOk = false;
				}
			}
			else
			{
				if (RbDeleteLogin.Checked)
				{
					if (ComboExistLogins.SelectedItem == null)
					{
						diagnostic.Set(DiagnosticType.Warning, Strings.NotSelectedLogins);
						isOk = false;
					}
				}
			}

			if (!isOk)
				DiagnosticViewer.ShowDiagnostic(diagnostic);

			return isOk;
		}

		//---------------------------------------------------------------------
		private bool TryToConnect(bool isLoginNT, string login, string password)
		{
			bool successConnection = false;
			TransactSQLAccess tentativeConnSql = new TransactSQLAccess();

			tentativeConnSql.CurrentStringConnection =
				(isLoginNT)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, this.dbCompanyServer, this.dbCompanyName)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, this.dbCompanyServer, this.dbCompanyName, login, password);

			if (tentativeConnSql.TryToConnect())
				successConnection = true;

			tentativeConnSql.CurrentStringConnection = string.Empty;

			return successConnection;
		}

		//---------------------------------------------------------------------
		private bool TryToConnectToMaster(bool isLoginNT, string login, string password)
		{
			bool successConnection = false;
			TransactSQLAccess tentativeConnMasterSql = new TransactSQLAccess();

			tentativeConnMasterSql.CurrentStringConnection =
				(isLoginNT)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, this.dbCompanyServer, DatabaseLayerConsts.MasterDatabase)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, this.dbCompanyServer, DatabaseLayerConsts.MasterDatabase, login, password);

			if (tentativeConnMasterSql.TryToConnect())
				successConnection = true;

			tentativeConnMasterSql.CurrentStringConnection = string.Empty;

			return successConnection;
		}

		//---------------------------------------------------------------------
		private bool ChangeLoginPassword()
		{
			bool successChanged = true;
			string loginName = ((LoginItem)ComboExistLogins.SelectedItem).LoginName;
			string oldPassword = string.Empty;
			string newPassword = string.Empty;
			bool isNTUser = ((LoginItem)ComboExistLogins.SelectedItem).IsNTUser;

			if (!isNTUser)
			{
				oldPassword = TbPassword.Text;
				newPassword = TbNewPassword.Text;

				if (string.Compare(oldPassword, newPassword, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					diagnostic.Set(DiagnosticType.Warning, Strings.PasswordsAreTheSame);
					return false;
				}

				ConfirmPassword confirmPwd = new ConfirmPassword();
				confirmPwd.OldPassword = newPassword;
				confirmPwd.Focus();
				confirmPwd.ShowDialog();

				if (confirmPwd.DialogResult != DialogResult.OK)
					return false;

				if (confirmPwd.Diagnostic.Error || confirmPwd.Diagnostic.Warning || confirmPwd.Diagnostic.Information)
				{
					diagnostic.Set(confirmPwd.Diagnostic);
					return false;
				}
			}

			DialogResult askToDo =
				MessageBox.Show(this, string.Format(Strings.ExplanationForChangePassword, this.dbCompanyServer), Strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (askToDo == DialogResult.Yes)
			{
				if (!TryToConnect(isNTUser, loginName, oldPassword))
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotChangePwdOfLogin, loginName));
					successChanged = false;
					return successChanged;
				}

				TransactSQLAccess connSqlTransact = new TransactSQLAccess();
				connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
				connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
				connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
				connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
				connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

				string buildedStringConnection = BuildConnection(this.companyId);

				if (buildedStringConnection.Length == 0)
				{
					//non riesco a connettermi all'azienda.
					this.ComboExistLogins.Items.Clear();
					this.ComboExistLogins.Enabled = false;
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
					return false;
				}

				connSqlTransact.CurrentStringConnection = buildedStringConnection;

				UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
				//eventualmente eseguo l'impersonificazione
				dataToConnectionServer =
					connSqlTransact.LoginImpersonification
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
					//TO DO GESTIRE
					Cursor.Current = Cursors.Default;
					return false;
				}

				//se la login esiste
				if (connSqlTransact.ExistLogin(loginName))
				{
					//se il cambio password è avvenuto con successo
					if (connSqlTransact.SPChangePassword(loginName, oldPassword, newPassword, this.dbCompanyName))
						ModifyPwdCompaniesUsers(loginName, oldPassword, newPassword);
					else
					{
						diagnostic.Set(DiagnosticType.Error, Strings.CannotChangePwdOfLoginMessage);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						successChanged = false;
					}
				}
				else
				{
					diagnostic.Set(DiagnosticType.Error, Strings.NotExistLogin);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					successChanged = false;
				}
			}

			return successChanged;
		}

		/// <summary>
		/// Cambia la password per tutti gli utenti che soddisfano i requisiti.
		/// </summary>
		//---------------------------------------------------------------------
		private bool ModifyPwdCompaniesUsers(string loginName, string oldPwd, string newPwd)
		{
			bool modifyAll = true;
			ArrayList companiesOnSameServer = new ArrayList();

			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;

			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = this.connectionString;
			companyUserDb.CurrentSqlConnection = this.currentConnection;

			if (!companyDb.SelectCompaniesSameServer(out companiesOnSameServer, this.dbCompanyServer))
			{
				diagnostic.Set(companyDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);

				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}

				companiesOnSameServer.Clear();
				modifyAll = false;
			}

			//per ogni azienda il cui db risiede nel  medesimo server
			for (int i = 0; i < companiesOnSameServer.Count; i++)
			{
				CompanyItem companyItem = (CompanyItem)companiesOnSameServer[i];
				ArrayList usersOfCompany = new ArrayList();

				if (!companyUserDb.SelectAll(out usersOfCompany, companyItem.CompanyId))
				{
					diagnostic.Set(companyUserDb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);

					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}

					usersOfCompany.Clear();
				}

				for (int j = 0; j < usersOfCompany.Count; j++)
				{
					CompanyUser usersOfCompanyItems = (CompanyUser)usersOfCompany[j];

					// controllo solo il nome della login, che deve essere univoca
					if ((string.Compare(usersOfCompanyItems.DBDefaultUser, loginName, StringComparison.InvariantCultureIgnoreCase) == 0))
					{
						UserListItem userItem = new UserListItem();
						userItem.LoginId = usersOfCompanyItems.LoginId;
						userItem.CompanyId = usersOfCompanyItems.CompanyId;
						userItem.DbUser = usersOfCompanyItems.DBDefaultUser;
						userItem.DbPassword = newPwd;
						userItem.DbWindowsAuthentication = usersOfCompanyItems.DBWindowsAuthentication;
						userItem.Disabled = usersOfCompanyItems.Disabled;
						userItem.IsAdmin = usersOfCompanyItems.Admin;
						userItem.EasyBuilderDeveloper = usersOfCompanyItems.EasyBuilderDeveloper;
						userItem.Login = usersOfCompanyItems.Login;

						if (!companyUserDb.Modify(userItem))
						{
							diagnostic.Set(companyUserDb.Diagnostic);
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							if (OnSendDiagnostic != null)
							{
								OnSendDiagnostic(this, diagnostic);
								diagnostic.Clear();
							}
						}
					}
				}
			}

			return modifyAll;
		}

		//---------------------------------------------------------------------
		private bool DisableCompaniesUsers(string loginName, string loginPassword)
		{
			bool disableAll = true;
			ArrayList companiesOnSameServer = new ArrayList();

			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;

			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = this.connectionString;
			companyUserDb.CurrentSqlConnection = this.currentConnection;

			bool result = companyDb.SelectCompaniesSameServer(out companiesOnSameServer, this.dbCompanyServer);
			if (!result)
			{
				diagnostic.Set(companyDb.Diagnostic);
				companiesOnSameServer.Clear();
				disableAll = false;
				return disableAll;
			}

			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();

			//per ogni azienda il cui db risiede nel  medesimo server
			for (int i = 0; i < companiesOnSameServer.Count; i++)
			{
				CompanyItem companyItem = (CompanyItem)companiesOnSameServer[i];
				string buildedStringConnection = BuildConnection(companyItem.CompanyId);

				if (buildedStringConnection.Length == 0)
				{
					//non riesco a connettermi all'azienda.
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
					return false;
				}

				connSqlTransact.CurrentStringConnection = buildedStringConnection;
				//eventualmente eseguo l'impersonificazione
				dataToConnectionServer =
					connSqlTransact.LoginImpersonification
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
					//TO DO GESTIRE
					Cursor.Current = Cursors.Default;
					return false;
				}

				//se la login esiste in master
				if (connSqlTransact.ExistLogin(loginName))
				{
					//se la login ha un accesso al master lo tolgo
					if (connSqlTransact.ExistLoginIntoDb(loginName, DatabaseLayerConsts.MasterDatabase))
					{
						bool resultDrops = connSqlTransact.SPDropUserFromMasterDb(loginName);

						if (!resultDrops)
						{
							if (connSqlTransact.Diagnostic.Warning | connSqlTransact.Diagnostic.Error)
								diagnostic.Set(connSqlTransact.Diagnostic);
							diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotDeleteLogin, loginName));
							if (dataToConnectionServer != null)
								dataToConnectionServer.Undo();
							disableAll = false;
							return disableAll;
						}
					}

					if (connSqlTransact.ExistLoginIntoDb(loginName, this.dbCompanyName))
					{
						bool resultDrops = connSqlTransact.SPDropUser(loginName);

						if (!resultDrops)
						{
							if (connSqlTransact.Diagnostic.Warning | connSqlTransact.Diagnostic.Error)
								diagnostic.Set(connSqlTransact.Diagnostic);
							diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotDeleteLogin, loginName));
							if (dataToConnectionServer != null)
								dataToConnectionServer.Undo();
							disableAll = false;
							return disableAll;
						}
					}

					if (!companyUserDb.ExistLogin(companyItem.CompanyId, loginName, loginPassword))
						continue;

					//Se invece ci sono utenti con questa login va avanti
					ArrayList usersOfCompany = new ArrayList();
					result = companyUserDb.SelectAll(out usersOfCompany, companyItem.CompanyId);

					if (!result)
					{
						if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
							diagnostic.Set(companyUserDb.Diagnostic);

						usersOfCompany.Clear();
					}

					for (int j = 0; j < usersOfCompany.Count; j++)
					{
						CompanyUser usersOfCompanyItems = (CompanyUser)usersOfCompany[j];

						if ((string.Compare(usersOfCompanyItems.DBDefaultUser, loginName, StringComparison.InvariantCultureIgnoreCase) == 0) &&
							(string.Compare(usersOfCompanyItems.DBDefaultPassword, loginPassword, StringComparison.InvariantCultureIgnoreCase) == 0))
						{
							UserListItem userItem = new UserListItem();
							userItem.LoginId = usersOfCompanyItems.LoginId;
							userItem.CompanyId = usersOfCompanyItems.CompanyId;
							userItem.DbUser = string.Empty;
							userItem.DbPassword = string.Empty;
							userItem.DbWindowsAuthentication = usersOfCompanyItems.DBWindowsAuthentication;
							userItem.Disabled = true;
							userItem.IsAdmin = usersOfCompanyItems.Admin;
							userItem.EasyBuilderDeveloper = usersOfCompanyItems.EasyBuilderDeveloper;
							userItem.Login = usersOfCompanyItems.Login;

							bool canDelete = false;
							// chiedo al LoginManager l'autenticazione per procedere con la cancellazione dell'associazione Utente-Azienda
							if (OnDeleteAssociation != null)
								canDelete = OnDeleteAssociation(this, Convert.ToInt32(usersOfCompanyItems.LoginId), Convert.ToInt32(usersOfCompanyItems.CompanyId));

							if (!canDelete)
							{
								// se non è stata fornita un'autenticazione valida visualizzo un msg e non procedo con l'elaborazione
								diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDeleteCompanyUser, loginName) + "\r\n" + Strings.AuthenticationTokenNotValid);
								continue;
							}

							result = companyUserDb.Modify(userItem);

							if (!result)
							{
								if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information || companyUserDb.Diagnostic.Warning)
									diagnostic.Set(companyUserDb.Diagnostic);
								diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.UserModify, usersOfCompanyItems.Login));
							}
							else
							{
								diagnostic.Set(DiagnosticType.Information, string.Format(Strings.AfterDisabledUser, usersOfCompanyItems.Login, companyItem.DbName));

								//Dico al loginManager che ho cancellato l'associazione
								if (OnDeleteAssociation != null)
									OnDeleteAssociation(this, Convert.ToInt32(usersOfCompanyItems.LoginId), Convert.ToInt32(usersOfCompanyItems.CompanyId));

								//informo il lockManager della cancellazione
								if (OnUnLockAllForUser != null)
									OnUnLockAllForUser(this, usersOfCompanyItems.Login);
							}
						}
					}
				}
				else
					disableAll = false;

				if (OnModifyTreeOfCompanies != null)
					OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, companyItem.CompanyId);
			}

			//Ho finito tutto ora devo cancellare la logins
			if (connSqlTransact.CurrentConnection != null)
				connSqlTransact.CurrentConnection.Close();

			result = connSqlTransact.SPDropLogin(loginName);

			if (!result)
			{
				if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning || connSqlTransact.Diagnostic.Information)
					diagnostic.Set(connSqlTransact.Diagnostic);

				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.CannotDeleteLoginDbo, loginName));
				disableAll = false;
			}
			else
				diagnostic.Set(DiagnosticType.Information, string.Format(Strings.AfterDeleteLogin, loginName));

			return disableAll;
		}

		//---------------------------------------------------------------------
		private bool DeleteCompaniesUsers(string loginName, string loginPassword)
		{
			bool deleteAll = true;

			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

			ArrayList companiesOnSameServer = new ArrayList();

			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;

			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = this.connectionString;
			companyUserDb.CurrentSqlConnection = this.currentConnection;

			bool result = companyDb.SelectCompaniesSameServer(out companiesOnSameServer, this.dbCompanyServer);
			if (!result)
			{
				if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
				{
					diagnostic.Set(companyDb.Diagnostic);
					companyDb.Diagnostic.Clear();
				}

				deleteAll = false;
				return deleteAll;
			}

			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();

			//per ogni azienda il cui db risiede nel medesimo server
			for (int i = 0; i < companiesOnSameServer.Count; i++)
			{
				CompanyItem companyItem = (CompanyItem)companiesOnSameServer[i];
				string buildedStringConnection = BuildConnection(companyItem.CompanyId);
				if (buildedStringConnection.Length == 0)
				{
					//non riesco a connettermi all'azienda.
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
					return false;
				}

				connSqlTransact.CurrentStringConnection = buildedStringConnection;

				if (!companyUserDb.ExistLogin(companyItem.CompanyId, loginName, loginPassword))
				{
					//Non ci sono utenti associati con questa login
					if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
					{
						diagnostic.Set(companyUserDb.Diagnostic);
						companyUserDb.Diagnostic.Clear();
					}

					continue;
				}

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
					//TO DO GESTIRE
					Cursor.Current = Cursors.Default;
					return false;
				}

				//se la login esiste in master
				if (connSqlTransact.ExistLogin(loginName))
				{
					//se la login ha un accesso al master lo tolgo
					if (connSqlTransact.ExistLoginIntoDb(loginName, DatabaseLayerConsts.MasterDatabase))
					{
						bool resultDrops = connSqlTransact.SPDropUserFromMasterDb(loginName);

						if (!resultDrops)
						{
							if (connSqlTransact.Diagnostic.Warning | connSqlTransact.Diagnostic.Error)
								diagnostic.Set(connSqlTransact.Diagnostic);

							diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotDeleteLogin, loginName));

							if (dataToConnectionServer != null)
								dataToConnectionServer.Undo();

							return false;
						}
					}
				}

				//se la login esiste
				if (connSqlTransact.ExistLogin(loginName))
				{
					if (connSqlTransact.SPDropUser(loginName))
					{
						ArrayList usersOfCompany = new ArrayList();
						result = companyUserDb.SelectAll(out usersOfCompany, companyItem.CompanyId);

						if (!result)
						{
							if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
							{
								diagnostic.Set(companyUserDb.Diagnostic);
								companyUserDb.Diagnostic.Clear();
							}

							continue;
						}

						for (int j = 0; j < usersOfCompany.Count; j++)
						{
							CompanyUser usersOfCompanyItems = (CompanyUser)usersOfCompany[j];

							if ((string.Compare(usersOfCompanyItems.DBDefaultUser, loginName, StringComparison.InvariantCultureIgnoreCase) == 0) &&
								(string.Compare(usersOfCompanyItems.DBDefaultPassword, loginPassword, StringComparison.InvariantCultureIgnoreCase) == 0))
							{
								bool canDelete = false;
								// chiedo al LoginManager l'autenticazione per procedere con la cancellazione dell'associazione Utente-Azienda
								if (OnDeleteAssociation != null)
									canDelete = OnDeleteAssociation(this, Convert.ToInt32(usersOfCompanyItems.LoginId), Convert.ToInt32(usersOfCompanyItems.CompanyId));

								if (!canDelete)
								{
									// se non è stata fornita un'autenticazione valida visualizzo un msg e non procedo con l'elaborazione
									diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDeleteCompanyUser, loginName) + ". " + Strings.AuthenticationTokenNotValid);
									continue;
								}

								result = companyUserDb.Delete(usersOfCompanyItems.LoginId, usersOfCompanyItems.CompanyId);

								if (!result)
								{
									if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
									{
										diagnostic.Set(companyUserDb.Diagnostic);
										companyUserDb.Diagnostic.Clear();
									}
								}
								else
								{
									diagnostic.Set(DiagnosticType.Information, string.Format(Strings.AfterDeleteUser, usersOfCompanyItems.Login, companyItem.DbName));

									//Dico al loginManager che ho cancellato l'associazione
									if (OnDeleteAssociation != null)
										OnDeleteAssociation(this, Convert.ToInt32(usersOfCompanyItems.LoginId), Convert.ToInt32(usersOfCompanyItems.CompanyId));

									//informo il lockManager della cancellazione
									if (OnUnLockAllForUser != null)
										OnUnLockAllForUser(this, usersOfCompanyItems.Login);
								}
							}
						}
					}
					else
						deleteAll = false;
				}
				else
					deleteAll = false;
			}

			//ora eseguo il drop della login
			if (connSqlTransact.CurrentConnection != null)
				connSqlTransact.CurrentConnection.Close();

			result = connSqlTransact.SPDropLogin(loginName);

			if (!result)
			{
				if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning || connSqlTransact.Diagnostic.Information)
				{
					diagnostic.Set(connSqlTransact.Diagnostic);
					connSqlTransact.Diagnostic.Clear();
				}

				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.CannotDeleteLoginDbo, loginName));
				deleteAll = false;
			}
			else
				diagnostic.Set(DiagnosticType.Information, string.Format(Strings.AfterDeleteLogin, loginName));

			return deleteAll;
		}

		//---------------------------------------------------------------------
		private bool DeleteLogin()
		{
			bool successDelete = true;
			string loginName = ((LoginItem)ComboExistLogins.SelectedItem).LoginName;
			string loginPassword = string.Empty;
			bool isNTUser = ((LoginItem)ComboExistLogins.SelectedItem).IsNTUser;

			if (!isNTUser)
				loginPassword = TbPassword.Text;

			//do un msg
			DialogResult askToDo =
				MessageBox.Show(this, string.Format(Strings.AskBeforeDeleteLogin, this.dbCompanyServer), Strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (askToDo == DialogResult.Yes)
			{
				if (!TryToConnect(isNTUser, loginName, loginPassword) && !TryToConnectToMaster(isNTUser, loginName, loginPassword))
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotChangePwdOfLogin, loginName));
					DiagnosticViewer.ShowDiagnostic(diagnostic);

					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}

					return false;
				}

				askToDo = MessageBox.Show(this, Strings.AskIfDisableUsers, Strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (askToDo == DialogResult.Yes)
				{
					//cancella la login dal db
					//disabilita tutti gli utenti che su quel server si connettevano
					//con la login loginName,loginPassword
					if (!DisableCompaniesUsers(loginName, loginPassword))
					{
						diagnostic.Set(DiagnosticType.Error, Strings.CannotDisableAndDeleteLogin);
						successDelete = false;
					}
				}
				else if (askToDo == DialogResult.No)
				{
					//cancella tutti gli utenti che su quel server si connettevano
					//con la login loginName,loginPassword
					if (!DeleteCompaniesUsers(loginName, loginPassword))
					{
						diagnostic.Set(DiagnosticType.Error, Strings.CannotDisableAndDeleteLogin);
						successDelete = false;
					}
				}
			}

			if (diagnostic.Error || diagnostic.Information || diagnostic.Warning)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);

				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}

			return successDelete;
		}

		//---------------------------------------------------------------------
		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			Save();
		}

		//---------------------------------------------------------------------
		public bool Save()
		{
			bool isSaved = false;

			if (CheckData())
			{
				if (RbChangePassword.Checked)
					isSaved = ChangeLoginPassword();
				else if (RbDeleteLogin.Checked)
					isSaved = DeleteLogin();
			}

			if (isSaved)
			{
				if (OnModifyTree != null)
					OnModifyTree(this, ConstString.containerCompanies);
			}

			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
				DiagnosticViewer.ShowDiagnostic(diagnostic);

			return isSaved;
		}

		//---------------------------------------------------------------------
		private void LoginAdministrator_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		//---------------------------------------------------------------------
		private void LoginAdministrator_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		//---------------------------------------------------------------------
		private void LoginAdministrator_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}

		//---------------------------------------------------------------------
		private void LoginAdministrator_Load(object sender, System.EventArgs e)
		{
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;
			ArrayList companyData = new ArrayList();
			companyDb.GetAllCompanyFieldsById(out companyData, this.companyId);

			if (companyData.Count > 0)
			{
				CompanyItem companyDataItem = (CompanyItem)companyData[0];
				providerId = companyDataItem.ProviderId;
			}
		}

		//---------------------------------------------------------------------
		private void CallHelp(object sender, string nameSpace, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, nameSpace, searchParameter);
		}
		#endregion
	}
}