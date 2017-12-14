using System;
using System.Collections;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.OracleDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// ModifyCompanyUsersToOracleLogin.
	/// Modifiche delle associazioni Utenti Applicativi - Azienda Oracle
	/// </summary>
	// ========================================================================
	public partial class ModifyCompanyUsersToOracleLogin : PlugInsForm
	{
		#region Varibili private
		private string	oracleServiceName	= string.Empty;
		private string	companyName			= string.Empty;
		private string	companyDbName		= string.Empty;
		private bool	isNTUser			= false;

		//dati per l'owner
		private string	companyDbOnwer	= string.Empty;
		private bool	ownerIsAdmin	= false;
		private string	ownerDbUser		= string.Empty;
		private string	ownerDbPwd		= string.Empty;
		private bool	ownerDbWinAuth	= false;

		private string			connectionString;
		private string			companyId;
		private SqlConnection	currentConnection;

		private Diagnostic diagnostic	= new Diagnostic("SysAdmin.ModifyCompanyUsersToOracleLogin");
		#endregion

		#region Eventi e Delegati
		public delegate void ModifyTreeOfCompanies(object sender, string nodeType,string companyId);
		public event ModifyTreeOfCompanies OnModifyTreeOfCompanies;

		public delegate void ModifyTree(object sender, string nodeType);
		public event ModifyTree OnModifyTree;
		
		public delegate void SaveUsers(object sender, string id, string companyId);
		public event SaveUsers OnSaveUsers;
		
		public delegate bool AfterDisableUser(object sender, int loginId, int companyId);
		public event AfterDisableUser OnAfterDisableUser;
		
		public delegate bool DeleteAssociation(object sender, int loginId, int companyId);
		public event DeleteAssociation OnDeleteAssociation;
		
		public delegate bool UnLockAllForUser(object sender, string userName);
		public event UnLockAllForUser OnUnLockAllForUser;
		
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
		
		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;
		
		public delegate void CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event CallHelpFromPopUp OnCallHelpFromPopUp;
		
		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;
		
		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;
		
		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;
		#endregion

		/// <summary>
		/// Costruttore (con parametri)
		/// </summary>
		//---------------------------------------------------------------------
		public ModifyCompanyUsersToOracleLogin(string connectionString, SqlConnection connection, string companyId)
		{
			InitializeComponent();
			this.connectionString	= connectionString;
			this.currentConnection	= connection;
			this.companyId			= companyId;
			LabelTitle.Text = Strings.TitleModifyCompanyUsersToLogin;
			BuildListView();
			
			//disabilito tutte le checkbox fino a che non ho degli elementi checked nella listview
			RbDeleteAll.Enabled			= false;
			RbDisableAll.Enabled		= false;
			RbEnableAll.Enabled			= false;
			RbModifyLogin.Enabled		= false;
			GrBoxOracleConnData.Enabled = false;
			isNTUser = false;
		}

		#region BuildListView - Costruisco il layout della griglia
		/// <summary>
		/// BuildListView
		/// Costuisco il layout della ListView
		/// </summary>
		//----------------------------------------------------------------------
		private void BuildListView()
		{
			ListViewUsersCompany.Clear();
			ListViewUsersCompany.View				= View.Details;
			ListViewUsersCompany.CheckBoxes			= true;
			ListViewUsersCompany.AllowColumnReorder = true;
			ListViewUsersCompany.Activation			= ItemActivation.OneClick;
			ListViewUsersCompany.Columns.Add(Strings.User, 170,HorizontalAlignment.Left);
			ListViewUsersCompany.Columns.Add(Strings.Description, 200,HorizontalAlignment.Left);
		}
		#endregion

		//---------------------------------------------------------------------
		private void LoadOracleService()
		{
			CompanyDb companyDb				= new CompanyDb();
			ArrayList companyData			= new ArrayList();
			companyDb.ConnectionString		= this.connectionString;
			companyDb.CurrentSqlConnection	= this.currentConnection;
			companyDb.GetAllCompanyFieldsById(out companyData, this.companyId);

			if (companyData.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)companyData[0];
				oracleServiceName = companyItem.DbServer;
				companyName		  = companyItem.Company;
				companyDbName     = companyItem.DbName;
				companyDbOnwer    = companyItem.DbOwner;
				if (companyDbOnwer.Length > 0)
					LoadDataDbOwner();
			}
		}

		//---------------------------------------------------------------------
		private void LoadDataDbOwner()
		{
			CompanyUserDb companyDbowner = new CompanyUserDb();
			companyDbowner.CurrentSqlConnection	= this.currentConnection;
			companyDbowner.ConnectionString		= this.connectionString;
			bool disable = false;
			companyDbowner.SelectDataForUserCompany
				(
				companyDbOnwer, 
				companyId, 
				out ownerIsAdmin, 
				out ownerDbUser, 
				out ownerDbPwd, 
				out ownerDbWinAuth, 
				out disable
				);
		}

		//---------------------------------------------------------------------
		public bool Save()
		{
			bool isSaved = false;
			if (CheckData())
			{	
				if (RbDeleteAll.Checked)
					isSaved = DeleteAllLogins();
				else 
					if (RbDisableAll.Checked)
						isSaved = DisableAllLogins();
					else 
						if (RbEnableAll.Checked)
							isSaved = EnableAllLogins();
						else 
							if (RbModifyLogin.Checked)
								isSaved = ModifyAllLogins();
			}

			return isSaved;
		}

		//---------------------------------------------------------------------
		private bool DeleteAllLogins()
		{
			// Ricavo il corrente cursore della form della console e lo salvo
			// per poterlo poi riassegnare in seguito, una volta terminata l'elaborazione
			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;
			bool deleateAll = true;
			CompanyUserDb companyUserDb			= new CompanyUserDb();
			companyUserDb.ConnectionString		= connectionString;
			companyUserDb.CurrentSqlConnection	= currentConnection;
			
			//chiedo conferma
			DialogResult askToDo = 
				MessageBox.Show(this, Strings.AskBeforeDeleteOracleLogin, Strings.Warning, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			
			if (askToDo == DialogResult.OK)
			{
				currentConsoleFormCursor = this.TopLevelControl.Cursor;
				//setto il cursore
				this.TopLevelControl.Cursor = Cursors.WaitCursor;
				Cursor.Current = Cursors.WaitCursor;
				//Setto il testo nello statusPanel
				SetConsoleProgressBarText(this, Strings.ProgressWaiting);

				for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
				{
					UserListItem currentItemSelected = (UserListItem)ListViewUsersCompany.CheckedItems[i];
					
					bool canDelete = false;
					// chiedo al LoginManager l'autenticazione per procedere alla disabilitazione dell'utente
					if (OnDeleteAssociation != null)
						canDelete = OnDeleteAssociation(this, Convert.ToInt32(currentItemSelected.LoginId), Convert.ToInt32(currentItemSelected.CompanyId));

					if (!canDelete)
					{
						// se non è stata fornita un'autenticazione valida visualizzo un msg e non procedo con l'elaborazione
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDeleteCompanyUser, currentItemSelected.Login) + ". " + Strings.AuthenticationTokenNotValid);
						continue;
					}

					string loginId = currentItemSelected.LoginId;
					if (companyUserDb.ExistUser(loginId, this.companyId) != 0)
					{
						//se posso cancellare la login, nel senso che non è una login di "gruppo"
						if (companyUserDb.CanDropLogin(currentItemSelected.DbUser, this.companyId))
							DropSynonyms(this.companyDbName, currentItemSelected.DbUser, currentItemSelected.DbPassword, currentItemSelected.DbWindowsAuthentication);
						
						//per quanto riguarda Oracle, un utente con quella login può essere associato solo a una azienda Oracle
						bool result = companyUserDb.Delete(loginId, this.companyId);
						if (result)
						{
							//informo il lockManager della cancellazione
							if (OnUnLockAllForUser != null)
								OnUnLockAllForUser(this, currentItemSelected.Login);
						}
						else
						{
							deleateAll = false;
							//non sono riuscita a cancellare
							if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
								diagnostic.Set(companyUserDb.Diagnostic);
							else
								diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDeleteCompanyUser, currentItemSelected.Login));
						}
					}
				}

				//Pulisco il testo del Msg nllo statusPanel
				SetConsoleProgressBarText(this, string.Empty);

				//rimetto a posto il cursore
				this.TopLevelControl.Cursor = currentConsoleFormCursor;
				Cursor.Current = currentConsoleFormCursor;

				if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
				{
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
						OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				if (OnModifyTree != null) 
					OnModifyTree(this, ConstString.containerCompanies);
			}

			return deleateAll;
		}

		//---------------------------------------------------------------------
		private bool DisableAllLogins()
		{
			// Ricavo il corrente cursore della form della console e lo salvo
			// per poterlo poi riassegnare in seguito, una volta terminata l'elaborazione
			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;
			bool disableAll = true;
			CompanyUserDb companyUserDb			= new CompanyUserDb();
			companyUserDb.ConnectionString		= connectionString;
			companyUserDb.CurrentSqlConnection	= currentConnection;

			currentConsoleFormCursor = this.TopLevelControl.Cursor;
			//setto il cursore
			this.TopLevelControl.Cursor = Cursors.WaitCursor;
			Cursor.Current = Cursors.WaitCursor;
			//Setto il testo nello statusPanel
			SetConsoleProgressBarText(this, Strings.ProgressWaiting);

			for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
			{
				UserListItem currentItemSelected = (UserListItem)ListViewUsersCompany.CheckedItems[i];
				if (!LoginIsDisabled(currentItemSelected.LoginId))
				{
					string loginId = currentItemSelected.LoginId;
					currentItemSelected.Disabled = true;
					
					bool canDisable = false;
					// chiedo al LoginManager l'autenticazione per procedere alla disabilitazione dell'utente
					if (OnAfterDisableUser != null)
						canDisable = OnAfterDisableUser(this, Convert.ToInt32(currentItemSelected.LoginId), Convert.ToInt32(currentItemSelected.CompanyId));

					if (!canDisable)
					{
						// se non è stata fornita un'autenticazione valida visualizzo un msg e non procedo con l'elaborazione
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDisableLogin, currentItemSelected.Login) + ". " + Strings.AuthenticationTokenNotValid);
						continue;
					}

					bool result = false;
					
					if (companyUserDb.ExistUser(loginId, this.companyId) != 0)
					{
						result = companyUserDb.Modify(currentItemSelected);
						if (result)
						{
							//devo comunicare al lockManager che ho disabilitato l'utente
							if (OnUnLockAllForUser != null)
								OnUnLockAllForUser(this, currentItemSelected.Login);
						}
						else
						{
							if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information || companyUserDb.Diagnostic.Warning)
								diagnostic.Set(companyUserDb.Diagnostic);
							else	
								diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDisableLogin, currentItemSelected.Login));
							disableAll = false;
						}
					}
					else
					{
						if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information || companyUserDb.Diagnostic.Warning)
							diagnostic.Set(companyUserDb.Diagnostic);
						else
							diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDisableLogin, currentItemSelected.Login));
						disableAll = false;
					}
				}
			}

			//Pulisco il testo del Msg nllo statusPanel
			SetConsoleProgressBarText(this, string.Empty);

			//rimetto a posto il cursore
			this.TopLevelControl.Cursor = currentConsoleFormCursor;
			Cursor.Current = currentConsoleFormCursor;

			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(this, diagnostic);
				diagnostic.Clear();
			}

			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);

			return disableAll;
		}

		//---------------------------------------------------------------------
		private bool EnableAllLogins()
		{
			// Ricavo il corrente cursore della form della console e lo salvo
			// per poterlo poi riassegnare in seguito, una volta terminata l'elaborazione
			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;
			bool enabledAll = true;
			CompanyUserDb companyUserDb			= new CompanyUserDb();
			companyUserDb.ConnectionString		= connectionString;
			companyUserDb.CurrentSqlConnection	= currentConnection;

			currentConsoleFormCursor = this.TopLevelControl.Cursor;
			//setto il cursore
			this.TopLevelControl.Cursor = Cursors.WaitCursor;
			Cursor.Current = Cursors.WaitCursor;
			//Setto il testo nello statusPanel
			SetConsoleProgressBarText(this, Strings.ProgressWaiting);

			for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
			{
				UserListItem currentItemSelected = (UserListItem)ListViewUsersCompany.CheckedItems[i];

				string loginId = currentItemSelected.LoginId;
				if (!LoginIsDisabled(loginId))
				{
					currentItemSelected.Disabled = false;
					if (companyUserDb.ExistUser(loginId, this.companyId) != 0)
					{
						if (!companyUserDb.Modify(currentItemSelected))
						{
							if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information || companyUserDb.Diagnostic.Warning)
								diagnostic.Set(companyUserDb.Diagnostic);
							else
								diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotEnableLogin, currentItemSelected.Login));
						}
					}
				}
				else
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.ApplicationUserIsDisabled, currentItemSelected.Login));
			}

			//Pulisco il testo del Msg nllo statusPanel
			SetConsoleProgressBarText(this, string.Empty);

			//rimetto a posto il cursore
			this.TopLevelControl.Cursor = currentConsoleFormCursor;
			Cursor.Current = currentConsoleFormCursor;

			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(this, diagnostic);
				diagnostic.Clear();
			}
			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);

			return enabledAll;
		}

		//---------------------------------------------------------------------
		private bool ModifyAllLogins()
		{
			// Ricavo il corrente cursore della form della console e lo salvo
			// per poterlo poi riassegnare in seguito, una volta terminata l'elaborazione
			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;
			bool modifyAll = true, insertUser = true;
			string selectedUser			= string.Empty;
			string selectedUserPwd		= string.Empty;
			bool selectedUserIsWinAuth	= false;

			currentConsoleFormCursor = this.TopLevelControl.Cursor;
			//setto il cursore
			this.TopLevelControl.Cursor = Cursors.WaitCursor;
			Cursor.Current = Cursors.WaitCursor;
			//Setto il testo nello statusPanel
			SetConsoleProgressBarText(this, Strings.ProgressWaiting);

			if (rbSelectExistedOracleUser.Checked)
			{
				OracleUser userSelected = (OracleUser)ComboOracleLogins.SelectedItem;
				selectedUser = userSelected.OracleUserId;
				selectedUserPwd = (userSelected.OracleUserOSAuthent) ? string.Empty : TextBoxOracleUserPwd.Text;
				selectedUserIsWinAuth = userSelected.OracleUserOSAuthent;
				
			}
			else if (rbNewOracleUser.Checked)
			{
				selectedUser			= txtNewOracleUser.Text;
				selectedUserPwd			=  (CbNTSecurity.Checked) ? string.Empty: TextBoxOracleUserPwd.Text;
				selectedUserIsWinAuth	= CbNTSecurity.Checked;
				insertUser				= AddNewOracleUser(this.oracleServiceName, selectedUser, selectedUserPwd, selectedUserIsWinAuth);
			}

			if (CheckOracleConnection(true))
			{
				CompanyUserDb companyUserDb			= new CompanyUserDb();
				companyUserDb.ConnectionString		= connectionString;
				companyUserDb.CurrentSqlConnection	= currentConnection;
				
				for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
				{
					UserListItem currentItemSelected			= (UserListItem)ListViewUsersCompany.CheckedItems[i];
					currentItemSelected.DbUser					= selectedUser;
					currentItemSelected.DbPassword				= selectedUserPwd;
					currentItemSelected.DbWindowsAuthentication = selectedUserIsWinAuth;
					
					if (companyUserDb.ExistUser(currentItemSelected.LoginId, this.companyId) != 0)
					{
						//prelevo le info dell'utente prima della modifica e vedo se posso cancelare il suo vecchio sinonimo
						ArrayList userOfCompany = new ArrayList();
						bool readingOldData = companyUserDb.GetUserCompany(out userOfCompany, currentItemSelected.LoginId, currentItemSelected.CompanyId);
						if (readingOldData)
						{
							CompanyUser currentOldUser = (CompanyUser)userOfCompany[0];

							if (companyUserDb.CanDropLogin(currentOldUser.DBDefaultUser, this.companyId))
								DropSynonyms(this.companyDbName, currentOldUser.DBDefaultUser, currentOldUser.DBDefaultPassword, currentOldUser.DBWindowsAuthentication);
						}
						
						//se non esiste , creo il sinonimo per l'utente con le nuove informazioni
						if (CreateSynonyms(companyDbName, currentItemSelected.DbUser, currentItemSelected.DbPassword, currentItemSelected.DbWindowsAuthentication))
						{
							if (!companyUserDb.Modify(currentItemSelected))
							{
								modifyAll = false;
								if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information || companyUserDb.Diagnostic.Warning)
									diagnostic.Set(companyUserDb.Diagnostic);
								else
									diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotModifyLogin, currentItemSelected.Login));
							}
						}
						else
							modifyAll = false;
					}
				}
			}
			else
				modifyAll = false;

			//Pulisco il testo del Msg nllo statusPanel
			SetConsoleProgressBarText(this, string.Empty);

			//rimetto a posto il cursore
			this.TopLevelControl.Cursor = currentConsoleFormCursor;
			Cursor.Current = currentConsoleFormCursor;

			if (diagnostic.Error || diagnostic.Warning)
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(this, diagnostic);
				diagnostic.Clear();
			}
			
			if (modifyAll)
			{
				if (OnModifyTreeOfCompanies != null)
					OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
				if (OnModifyTreeOfCompanies != null)
					OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);
			}

			return modifyAll;
		}

		//---------------------------------------------------------------------
		private bool DropSynonyms(string schemaName, string userName, string userPassword, bool userOSAuthent)
		{
			bool result = false;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateAdmin = oracleAccess.LoadSystemData(oracleServiceName);
			OracleUserImpersonatedData oracleAdmin = oracleAccess.AdminImpersonification(candidateAdmin);

			if (oracleAdmin == null)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.WrongUserCredential, candidateAdmin.Login));
				return result;
			}
			else
			{
				try
				{
					oracleAccess.LoadUserData(oracleAdmin.OracleService, schemaName, oracleAdmin.Login, oracleAdmin.Password, oracleAdmin.WindowsAuthentication);
					oracleAccess.OpenConnection();
					result = oracleAccess.DropSynonym(schemaName, userName);
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UnableToCreateSynonyms, userName, schemaName, userName));
				}
				finally
				{
					oracleAccess.CloseConnection();
				}
			}

			if (oracleAdmin != null)
				oracleAdmin.Undo();
			return result;
		}

		//---------------------------------------------------------------------
		private bool AddNewOracleUser(string oracleService, string schemaOwner, string schemaOwnerPwd, bool schemaOwnerIsWinNt)
		{
			bool result = false;
			
			//mi connetto (se non l'ho già fatto) con il system di oracle
			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			
			OracleUserImpersonatedData candidateAdmin = oracleAccess.LoadSystemData(oracleService);
			OracleUserImpersonatedData oracleAdmin = oracleAccess.AdminImpersonification(candidateAdmin); 
			if (oracleAdmin == null)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.WrongUserCredential, candidateAdmin.Login));
				return result;
			}
			else
			{
				//aggiungo la login a oracle
				oracleAccess.LoadUserData(oracleAdmin.OracleService, oracleAdmin.Login, oracleAdmin.Password, oracleAdmin.WindowsAuthentication);

				try
				{
					oracleAccess.OpenConnection();
					if (schemaOwnerIsWinNt)
						oracleAccess.CreateNTAssociatedUser(schemaOwner, schemaOwnerPwd);
					else
						oracleAccess.CreateAssociatedUser(schemaOwner, schemaOwnerPwd);
					result = true;
				}
				catch(TBException tbExc)
				{
					Debug.Fail(tbExc.Message);
					diagnostic.Set(DiagnosticType.Error, tbExc.Message);
					result = false;
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
					diagnostic.Set(DiagnosticType.Error, exc.Message);
					result = false;
				}
			}

			return result;
		}

		//---------------------------------------------------------------------
		private bool CreateSynonyms(string schemaName, string userName, string userPassword, bool userOSAuthent)
		{
			bool result = false;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateAdmin = oracleAccess.LoadSystemData(oracleServiceName);
			OracleUserImpersonatedData oracleAdmin = oracleAccess.AdminImpersonification(candidateAdmin);

			if (oracleAdmin == null)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.WrongUserCredential, candidateAdmin.Login));
				return result;
			}
			else
			{
				try
				{
					oracleAccess.LoadUserData(oracleAdmin.OracleService, schemaName, oracleAdmin.Login, oracleAdmin.Password, oracleAdmin.WindowsAuthentication);
					oracleAccess.OpenConnection();
					result = oracleAccess.CreateSynonyms(schemaName, userName);
					if (!result)
					{
						if (oracleAccess.Diagnostic.Error || oracleAccess.Diagnostic.Warning || oracleAccess.Diagnostic.Information)
							diagnostic.Set(oracleAccess.Diagnostic);
						else
							diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UnableToCreateSynonyms, userName, schemaName, userName));
					}
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UnableToCreateSynonyms, userName, schemaName, userName));
				}
				finally
				{
					oracleAccess.CloseConnection();
				}
			}

			if (oracleAdmin != null)
				oracleAdmin.Undo();
			return result;
		}

		//---------------------------------------------------------------------
		private void RbDisableAll_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbDisableAll.Checked)
			{
				RbDeleteAll.Checked			= false;
				RbEnableAll.Checked			= false;
				RbModifyLogin.Checked		= false;
				GrBoxOracleConnData.Enabled = false;
			}
		}

		//---------------------------------------------------------------------
		private void RbModifyLogin_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbModifyLogin.Checked)
			{
				RbDisableAll.Checked		= false;
				RbEnableAll.Checked			= false;
				RbDeleteAll.Checked			= false;
				GrBoxOracleConnData.Enabled = true;
			}
		}

		//---------------------------------------------------------------------
		private void RbDeleteAll_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbDeleteAll.Checked)
			{
				RbDisableAll.Checked		= false;
				RbEnableAll.Checked			= false;
				RbModifyLogin.Checked		= false;
				GrBoxOracleConnData.Enabled = false;
			}
		}

		//---------------------------------------------------------------------
		private void RbEnableAll_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbEnableAll.Checked)
			{
				RbDeleteAll.Checked			= false;
				RbDisableAll.Checked		= false;
				RbModifyLogin.Checked		= false;
				GrBoxOracleConnData.Enabled = false;
			}
		}

		#region LoadOracleDbUsers - Carica tutti gli utenti assegnati a una azienda con DbUser = LoginName selezionata
		/// <summary>
		/// LoadOracleDbUsers
		/// </summary>
		//----------------------------------------------------------------------
		private void LoadOracleDbUsers(string loginName)
		{
			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = connectionString;
			companyUserDb.CurrentSqlConnection = currentConnection;
			ArrayList usersOfCompany = new ArrayList();
			bool existDisabledUsers = false, existEnabledUsers = false;
			
			bool result = companyUserDb.SelectAll(out usersOfCompany, this.companyId);
			if (!result)
			{
				if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information || companyUserDb.Diagnostic.Warning)
					diagnostic.Set(companyUserDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(this, diagnostic);
				diagnostic.Clear();
				usersOfCompany.Clear();
			}
			
			for (int i = 0; i < usersOfCompany.Count; i++)
			{
				CompanyUser itemCompanyUser	 = (CompanyUser)usersOfCompany[i];
				if (loginName.Length > 0)
				{
					if (string.Compare(itemCompanyUser.DBDefaultUser, loginName, StringComparison.InvariantCultureIgnoreCase) != 0)
						continue;
				}
				
				UserListItem listItemUser	= new UserListItem();
				listItemUser.IsModified		= false;
				listItemUser.CompanyId		= this.companyId;
				listItemUser.LoginId		= itemCompanyUser.LoginId;
				listItemUser.Login			= itemCompanyUser.Login;
				listItemUser.Description	= itemCompanyUser.Description.Replace("\r\n", " ");
				listItemUser.DbPassword     = itemCompanyUser.DBDefaultPassword;
				listItemUser.DbUser         = itemCompanyUser.DBDefaultUser;
				listItemUser.DbWindowsAuthentication	= itemCompanyUser.DBWindowsAuthentication;
				listItemUser.LoginWindowsAuthentication = itemCompanyUser.WindowsAuthentication;
				
				listItemUser.ImageIndex =	(itemCompanyUser.WindowsAuthentication)
                                            ? PlugInTreeNode.GetLoginsDefaultImageIndex
                                            : PlugInTreeNode.GetUserDefaultImageIndex;
				listItemUser.Text = itemCompanyUser.Login;
				
				if (itemCompanyUser.Disabled)
				{
					listItemUser.Disabled = true;
					listItemUser.ForeColor = Color.Red;
					existDisabledUsers = !(LoginIsDisabled(listItemUser.LoginId));
				}
				else
				{
					listItemUser.Disabled = false;
					existEnabledUsers = true;
				}

				listItemUser.SubItems.Add(listItemUser.Description.Replace("\r\n", " "));
				
				//se è il dbowner non lo inserisco
				if (!companyUserDb.IsDbo(itemCompanyUser.LoginId, this.companyId))
					ListViewUsersCompany.Items.Add(listItemUser);
			}

			RbEnableAll.Enabled = existDisabledUsers;
			RbDisableAll.Enabled = existEnabledUsers;
		}
		#endregion

		/// <summary>
		/// LoginIsDisabled
		/// True se la login è disabilitata, false altrimenti
		/// </summary>
		//---------------------------------------------------------------------
		private bool LoginIsDisabled(string loginId)
		{
			UserDb loginDetailDb = new UserDb();
			loginDetailDb.ConnectionString = connectionString;
			loginDetailDb.CurrentSqlConnection = currentConnection;
			ArrayList loginData = new ArrayList();
			return loginDetailDb.IsDisabled(loginId);
		}

		//---------------------------------------------------------------------
		private void CbOracleUsers_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (CbOracleUsers.SelectedItem != null)
			{
				// Quando Anna fa il metodo che ritorna tutti gli utenti di uno schema (sinonimi + utente dbowner) 
				string userName = ((OracleUser)CbOracleUsers.SelectedItem).OracleUserId;
				if (userName.Length != 0)
				{
					ListViewUsersCompany.Items.Clear();
					LoadOracleDbUsers(userName);
					if (ListViewUsersCompany.Items.Count > 0)
					{
						RbDeleteAll.Enabled			= true;
						RbModifyLogin.Enabled		= true;
						GrBoxOracleConnData.Enabled = true;
					}
					else
					{
						RbDeleteAll.Enabled			= false;
						RbModifyLogin.Enabled		= false;
						GrBoxOracleConnData.Enabled = false;
					}
				}
			}
		}

		/// <summary>
		/// LoadOracleUser
		/// Carico tutte le logins Oracle (andando a leggere per quella azienda
		/// Oracle tutti i cambi DBUser in MSD_CompanyLogins )
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadOracleUser()
		{
			CbOracleUsers.Items.Clear();
			// Quando Anna fa il metodo che ritorna tutti gli utenti di uno schema (sinonimi + utente dbowner) 
			ArrayList oracleUsers				= new ArrayList();
			CompanyUserDb oracleUserDb			= new CompanyUserDb();
			oracleUserDb.ConnectionString		= this.connectionString;
			oracleUserDb.CurrentSqlConnection	= this.currentConnection;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			
			OracleUserImpersonatedData candidateAdmin = oracleAccess.LoadSystemData(oracleServiceName);
			OracleUserImpersonatedData oracleAdmin = oracleAccess.AdminImpersonification(candidateAdmin); 
			if (oracleAdmin == null)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.WrongUserCredential, candidateAdmin.Login));
				return;
			}
			else
			{ 
				try
				{
					oracleUsers = oracleUserDb.LoadUsersFromOracleSchema(oracleServiceName, companyDbName, oracleAdmin, oracleAccess);
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
				}
				finally
				{
					oracleAccess.CloseConnection();
				}
				
				if (oracleUsers.Count > 0)
				{
					for (int i = 0; i < oracleUsers.Count; i++)
						CbOracleUsers.Items.Add(oracleUsers[i]);
				}
			}

			CbOracleUsers.DisplayMember = "OracleUserId";
			CbOracleUsers.ValueMember	= "OracleUserOSAuthent";
			CbOracleUsers.SelectedIndex = (CbOracleUsers.Items.Count > 0) ? 0 : -1;

			if (oracleAdmin != null)
				oracleAdmin.Undo();
		}

		//---------------------------------------------------------------------
		private void CbOracleUsers_DropDown(object sender, System.EventArgs e)
		{
			if (CbOracleUsers.Items.Count <= 1) 
				LoadOracleUser();
		}

		//---------------------------------------------------------------------
		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			if (OnSaveUsers != null)
				OnSaveUsers(sender, string.Empty, this.companyId);
		}

		#region CheckData - Verifica dei dati introdotti dall'utente
		/// <summary>
		/// CheckData
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckData()
		{
			bool result = true;

			if (ListViewUsersCompany.CheckedItems.Count == 0)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.NotSelectedCompanyUsersToModify);
				result = false;
			}
			
			//ho selezionato modifica
			if (RbModifyLogin.Checked)
			{
				if (rbSelectExistedOracleUser.Checked)
				{
					if ( !((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent && (TextBoxOracleUserPwd.Text.Length == 0))
					{
						diagnostic.Set(DiagnosticType.Error, Strings.EmptyOracleOwnerPwd);
						result = false;
					}
				}
				else if (rbNewOracleUser.Checked)
				{
					if (txtNewOracleUser.Text.Length == 0)
					{
						diagnostic.Set(DiagnosticType.Error, Strings.EmptyOracleOwnerName);
						result = false;
					}
					else if (txtNewOracleUser.Text.Length != 0)
					{
						if (!CbNTSecurity.Checked)
						{
							if (txtNewOracleUser.Text.CountChar(Path.DirectorySeparatorChar) != 0)
							{
								diagnostic.Set(DiagnosticType.Error, Strings.ForbiddedNtUser);
								result = false;
							}
							if (TextBoxOracleUserPwd.Text.Length == 0)
							{
								diagnostic.Set(DiagnosticType.Warning, Strings.EmptyOracleOwnerPwd);
								result = false;
							}
						}

						//TODO UNICODE
						//if (txtNewOracleUser.Text.Trim().IndexOfAny(new char[] {'?', '*', '"', '£', '$', '&', '/', '(', ')', '=', '[', '#', ']', '<', '>', ':', '!', '|'}) != -1)
						if (txtNewOracleUser.Text.Trim().IndexOfAny(new char[] {'?', '*', '"', '$', '&', '/', '(', ')', '=', '[', '#', ']', '<', '>', ':', '!', '|'}) != -1)
						{
							diagnostic.Set(DiagnosticType.Warning, Strings.WrongCharactersInOracleOwnerName);
							result = false;
						}
					}
				}
			}

			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(this, diagnostic);
				diagnostic.Clear();
			}
			return result;
		}
		#endregion

		//---------------------------------------------------------------------
		private void BtnOracleConnectionCheck_Click(object sender, System.EventArgs e)
		{
			if (CheckData())
				CheckOracleConnection(false);
		}

		//---------------------------------------------------------------------
		private bool CheckOracleConnection(bool isSilent)
		{
			OracleUserImpersonatedData	oracleUserData = null;
			bool successConnection	= false, oracleUserIsNt	= false;
			string oracleUser = string.Empty;

			this.diagnostic.Clear();

			if (rbSelectExistedOracleUser.Checked)
			{
				oracleUserIsNt	= ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent;
				oracleUser		= ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId;
			}
			else if (rbNewOracleUser.Checked)
			{
				oracleUser		= txtNewOracleUser.Text;
				oracleUserIsNt	= CbNTSecurity.Checked;
			}

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			
			OracleUserImpersonatedData candidateUser = new OracleUserImpersonatedData();
			candidateUser.Login					= oracleUser;
			candidateUser.WindowsAuthentication	= oracleUserIsNt;
			candidateUser.OracleService			= oracleServiceName;
			candidateUser.IsDba					= false;

			if (oracleUserIsNt)
			{
				candidateUser.Password	= string.Empty;
				candidateUser.Domain	= oracleUser.Split(Path.DirectorySeparatorChar)[0];
				//non può cambiare le informazioni in finestra
				oracleUserData = oracleAccess.UserImpersonification(candidateUser, true, false); 
			}
			else
			{
				candidateUser.Password = TextBoxOracleUserPwd.Text;
				candidateUser.Domain	= string.Empty;
				//silente
				oracleUserData = oracleAccess.UserImpersonification(candidateUser); 
			}

			if (oracleUser == null)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.WrongUserCredential, candidateUser.Login));
				successConnection = false;
				if (!isSilent)
				{
					if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
					{
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						diagnostic.Clear();
					}
				}
				return successConnection;
			}

			if (oracleAccess.Diagnostic.Error || oracleAccess.Diagnostic.Warning || oracleAccess.Diagnostic.Information)
				diagnostic.Set(oracleAccess.Diagnostic);
			successConnection = !oracleAccess.Diagnostic.Error;
			
			if (!isSilent)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				diagnostic.Clear();
			}	

			if (oracleUserIsNt)
			{
				if (oracleUserData != null)
					oracleUserData.Undo(); 
			}

			return successConnection;
		}

		//---------------------------------------------------------------------
		private void TexBoxOracleUserName_TextChanged(object sender, System.EventArgs e)
		{
			if (((TextBox)sender).Text.Split(Path.DirectorySeparatorChar).Length > 1)
			{
				//Se è un gruppo di utenti e sta scrivendo una login NT errore
				if (ListViewUsersCompany.CheckedItems.Count > 1) 
				{
					diagnostic.Set(DiagnosticType.Error, Strings.ForbiddedNtUser);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
						OnSendDiagnostic(sender, diagnostic);
					diagnostic.Clear();
				}
					//se ha selezionato un solo utente (non NT e sta scrivendo una login NT) errore
				else 
				{
					if ((ListViewUsersCompany.CheckedItems.Count == 1) && 
						(!((UserListItem)ListViewUsersCompany.CheckedItems[0]).DbWindowsAuthentication))
					{
						diagnostic.Set(DiagnosticType.Error, Strings.ForbiddedNtUser);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (OnSendDiagnostic != null)
							OnSendDiagnostic(sender, diagnostic);
						diagnostic.Clear();
					}
					else
					{
						isNTUser = true;
						TextBoxOracleUserPwd.Enabled = isNTUser;
					}
				}
			}
		}

		//---------------------------------------------------------------------
		private void ModifyCompanyUsersToOracleLogin_Load(object sender, System.EventArgs e)
		{
			LoadOracleService();
			LoadOracleUser();
			if (RbModifyLogin.Checked)
			{
				LoadAssignedAndFreeOracleUsers();
				rbSelectExistedOracleUser.Checked	= true;
				rbNewOracleUser.Checked				= false;
				ComboOracleLogins.Enabled			= true;
				txtNewOracleUser.Enabled			= false;
				CbNTSecurity.Enabled				= false;
				CbNTSecurity.Checked				= false;
				BtnOracleConnectionCheck.Enabled	= true;
			}
		}
		
		//--------------------------------------------------------------------
		private void LoadAssignedAndFreeOracleUsers()
		{
			ComboOracleLogins.DataSource = null;
			ComboOracleLogins.Items.Clear();
			CompanyUserDb companyUsers			= new CompanyUserDb();
			companyUsers.ConnectionString		= this.connectionString;
			companyUsers.CurrentSqlConnection	= this.currentConnection;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateAdmin = oracleAccess.LoadSystemData(oracleServiceName);
			OracleUserImpersonatedData oracleAdmin = oracleAccess.AdminImpersonification(candidateAdmin); 
			//oracleAccess.AdminImpersonification(oracleService);
			if (oracleAdmin == null)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.WrongUserCredential, candidateAdmin.Login));
				return;
			}
			else
			{
				ArrayList freeUsers		  = companyUsers.LoadFreeOracleUsersForAssociation(oracleServiceName, this.companyDbName, oracleAdmin);
				ArrayList usersFromSchema = companyUsers.LoadUsersFromOracleSchema(oracleServiceName, companyDbName, oracleAdmin, oracleAccess);

				for (int i = 0; i < usersFromSchema.Count; i++)
				{
					OracleUser tempUser = (OracleUser)usersFromSchema[i];
					int pos = freeUsers.BinarySearch(tempUser);
					if (pos < 0)
						freeUsers.Add(tempUser);	
				}
				
				if (freeUsers.Count > 0)
				{
					ComboOracleLogins.DataSource	= freeUsers;
					ComboOracleLogins.DisplayMember = "OracleUserId";
					ComboOracleLogins.ValueMember	= "OracleUserOSAuthent";
					ComboOracleLogins.SelectedIndex = 0;
				}
				else
					ComboOracleLogins.SelectedIndex = -1;
			}

			if (oracleAdmin != null)
				oracleAdmin.Undo();
		}

		#region Funzioni per il Send di Diagnostica a MicroareaConsole
		//---------------------------------------------------------------------
		private void ModifyCompanyUsersToOracleLogin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		//---------------------------------------------------------------------
		private void ModifyCompanyUsersToOracleLogin_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		//---------------------------------------------------------------------
		private void ModifyCompanyUsersToOracleLogin_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
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

		//---------------------------------------------------------------------
		private void ComboOracleLogins_DropDown(object sender, System.EventArgs e)
		{
			if (ComboOracleLogins.Items.Count == 0)
				LoadAssignedAndFreeOracleUsers();
		}

		/// <summary>
		/// ListViewUsersCompany_ItemCheck
		/// abilito/disabilito CbNTSecurity
		/// </summary>
		//---------------------------------------------------------------------
		private void ListViewUsersCompany_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			if (ListViewUsersCompany.CheckedItems.Count > 0 && e.NewValue == CheckState.Checked)
			{
				UserListItem currentUserSelect	= (UserListItem)ListViewUsersCompany.Items[e.Index];
				if (currentUserSelect.Tag != null && 
					string.Compare(currentUserSelect.Tag.ToString(), ConstString.LoginNotExist, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.LoginNotExistInDatabase, currentUserSelect.Login, this.companyDbName));
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					diagnostic.Clear();
					e.NewValue = e.CurrentValue;
				}
			}
			else if (ListViewUsersCompany.CheckedItems.Count == 0 && e.NewValue == CheckState.Checked)
			{
				if (ListViewUsersCompany.Items[e.Index] is UserListItem)
				{
					UserListItem currentUserSelect	= (UserListItem)ListViewUsersCompany.Items[e.Index];
					if (currentUserSelect.Tag != null &&
						string.Compare(currentUserSelect.Tag.ToString(), ConstString.LoginNotExist, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.LoginNotExistInDatabase, currentUserSelect.Login, this.companyDbName));
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						diagnostic.Clear();
						e.NewValue = e.CurrentValue;
					}
				}
			}
			else if (ListViewUsersCompany.CheckedItems.Count > 0 && e.NewValue == CheckState.Unchecked)
			{
				//avrò un utente selezionato al termine dell'evento - il check è abilitato se l'utente 
				//è in sicurezza integrata
				if (ListViewUsersCompany.CheckedIndices.Count == 2)
				{
					int selectedUserPos = 0;
					for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count - 1; i++)
					{
						//scarto il valore di cui sto facendo uncheck
						if (i == e.Index) 
							continue;
						//prendo il restante valore
						selectedUserPos = i;
					}
				}
			}
				
			//ora valuto se abilitare/disabilitare i radio button Enable/disable
			bool canEnable = false;
			bool canDisable = false;
			//Enable - se c'è qualche login di quelle selezionate disabilitate e se queste login  non hanno MSD_Logins.Disable = true
			//Disable - se c'è qualche login enable
			if (e.NewValue == CheckState.Checked)
			{
				//tutto ciò che ho nella CheckItems + Items[e.Index]
				for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
				{
					UserListItem currentUserSelect	= (UserListItem)ListViewUsersCompany.CheckedItems[i];
					if (currentUserSelect != null && currentUserSelect.Disabled)
					{
						if (!LoginIsDisabled(currentUserSelect.LoginId))
							canEnable = true;
					}
					else
						canDisable = true;
				}
				//ora controllo il next valore
				UserListItem nextUserSelect	= (UserListItem)ListViewUsersCompany.Items[e.Index];
				if (nextUserSelect != null && nextUserSelect.Disabled)
				{
					if (!LoginIsDisabled(nextUserSelect.LoginId))
						canEnable = true;
				}
				else
					canDisable = true;
			}
			else if (e.NewValue == CheckState.Unchecked)
			{
				//tutto ciò che ho nella Checkitems - CheckItems[e.Index]
				for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
				{
					if (i == e.Index) 
						continue;
					UserListItem currentUserSelect = (UserListItem)ListViewUsersCompany.CheckedItems[i];
					if (currentUserSelect != null && currentUserSelect.Disabled)
					{
						if (!LoginIsDisabled(currentUserSelect.LoginId))
							canEnable = true;
					}
					else
						canDisable = true;
				}
			}

			RbDisableAll.Enabled = canDisable;
			RbEnableAll.Enabled = canEnable;
		}

		//---------------------------------------------------------------------
		private void CbNTSecurity_CheckStateChanged(object sender, System.EventArgs e)
		{
			ComboOracleLogins.Enabled = RbModifyLogin.Checked;
		}

		//---------------------------------------------------------------------
		private void SendHelp(object sender, string nameSpace, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, nameSpace, searchParameter);
		}

		//---------------------------------------------------------------------
		private void ComboOracleLogins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (ComboOracleLogins.SelectedItem != null)
			{
				OracleUser current = (OracleUser)ComboOracleLogins.SelectedItem;
				if (current.OracleUserOSAuthent)
				{
					//se ho selezionato un utente NT non devo poter digitare la pwd
					TextBoxOracleUserPwd.Text	= string.Empty;
					TextBoxOracleUserPwd.Enabled	= false;
				}
				else
					TextBoxOracleUserPwd.Enabled	= true;
			}
		}

		#region Funzioni relative alla ProgressBar

		#region EnableConsoleProgressBar - Dice alla MC di abilitare la ProgressBar
		/// <summary>
		/// EnableConsoleProgressBar
		/// Dice alla MC di abilitare la ProgressBar
		/// </summary>
		//---------------------------------------------------------------------------
		private void EnableConsoleProgressBar(object sender)
		{
			if (OnEnableProgressBar != null)
				OnEnableProgressBar(sender);
		}
		#endregion

		#region DisableConsoleProgressBar - Dice alla MC di disabilitare la ProgressBar
		/// <summary>
		/// DisableConsoleProgressBar
		/// Dice alla MC di disabilitare la ProgressBar
		/// </summary>
		//---------------------------------------------------------------------------
		private void DisableConsoleProgressBar(object sender)
		{
			if (OnDisableProgressBar != null)
				OnDisableProgressBar(sender);
		}
		#endregion

		#region SetConsoleProgressBarStep - Dice alla MC di impostare lo Step della ProgressBar
		/// <summary>
		/// SetConsoleProgressBarStep
		/// Dice alla MC di impostare lo Step della ProgressBar
		/// al valore di step
		/// </summary>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarStep(object sender, int step)
		{
			if (OnSetProgressBarStep != null)
				OnSetProgressBarStep(sender, step);
		}
		#endregion

		#region SetConsoleProgressBarValue - Dice alla MC di impostare il value della ProgressBar
		/// <summary>
		/// SetConsoleProgressBarValue
		/// Dice alla MC di impostare il value della ProgressBar pari a currentValue
		/// </summary>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarValue(object sender, int currentValue)
		{
			if (OnSetProgressBarValue != null)
				OnSetProgressBarValue(sender, currentValue);
		}
		#endregion

		#region SetConsoleProgressBarText - Dice alla MC quale deve essere il testo da visualizzare accanto alla progressBar 
		/// <summary>
		/// SetConsoleProgressBarText
		/// Dice alla MC quale deve essere il testo da visualizzare accanto alla progressBar (pari a message)
		/// </summary>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarText(object sender, string message)
		{
			if (OnSetProgressBarText != null)
				OnSetProgressBarText(sender, message);
		}
		#endregion

		#endregion
		//---------------------------------------------------------------------
		private void CbNTSecurity_CheckedChanged(object sender, System.EventArgs e)
		{
			if (CbNTSecurity.Checked)
			{
				txtNewOracleUser.Text			= WindowsIdentity.GetCurrent().Name.ToUpper(CultureInfo.InvariantCulture);
				TextBoxOracleUserPwd.Text		= string.Empty;
				TextBoxOracleUserPwd.Enabled	= false;
				txtNewOracleUser.Enabled		= false;
			}
			else
			{
				txtNewOracleUser.Text			= string.Empty;
				TextBoxOracleUserPwd.Text		= string.Empty;
				TextBoxOracleUserPwd.Enabled	= true;
				txtNewOracleUser.Enabled		= true;
			}
		}

		//---------------------------------------------------------------------
		private void txtNewOracleUser_Leave(object sender, System.EventArgs e)
		{
			txtNewOracleUser.Text = txtNewOracleUser.Text.ToUpper(CultureInfo.InvariantCulture);
		}

		//---------------------------------------------------------------------
		private void rbSelectExistedOracleUser_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rbSelectExistedOracleUser.Checked)
			{
				ComboOracleLogins.Enabled = true;
				txtNewOracleUser.Enabled = false;
				TextBoxOracleUserPwd.Text = string.Empty;
				//abilito il bottone di verifica della connessione
				BtnOracleConnectionCheck.Enabled = true;
				txtNewOracleUser.Text = string.Empty;
				CbNTSecurity.Enabled = false;
				CbNTSecurity.Checked = false;
			}
		}

		//---------------------------------------------------------------------
		private void rbNewOracleUser_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rbNewOracleUser.Checked)
			{
				ComboOracleLogins.Enabled			= false;
				txtNewOracleUser.Enabled			= true;
				CbNTSecurity.Enabled				= !ExistNTCurrentUser();
				TextBoxOracleUserPwd.Text			= string.Empty;
				//disabilito il bottone di check
				BtnOracleConnectionCheck.Enabled	= false;
			}
		}

		//---------------------------------------------------------------------
		private bool ExistNTCurrentUser()
		{
			bool existUser = false;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateAdmin = new OracleUserImpersonatedData();
			candidateAdmin.Login = "system";
			candidateAdmin.Password = "";
			candidateAdmin.OracleService = oracleServiceName;
			candidateAdmin.IsDba = true;
			candidateAdmin.Domain = "";

			OracleUserImpersonatedData oracleAdmin = oracleAccess.AdminImpersonification(candidateAdmin); 
			if (oracleAdmin == null)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.WrongUserCredential, candidateAdmin.Login));
				return existUser;
			}
			else
			{
				try
				{
					oracleAccess.LoadUserData(oracleAdmin.OracleService, oracleAdmin.Login, oracleAdmin.Password, oracleAdmin.WindowsAuthentication);
					oracleAccess.OpenConnection();
					existUser  = oracleAccess.ExistUserIntoDatabase(WindowsIdentity.GetCurrent().Name.ToUpper(CultureInfo.InvariantCulture), false);
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
				}
				finally
				{
					oracleAccess.CloseConnection();
				}
			}

			if (oracleAdmin != null)
				oracleAdmin.Undo();

			return existUser;
		}
	}
}