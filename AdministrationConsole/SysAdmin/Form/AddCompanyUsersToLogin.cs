using System;
using System.Collections;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.SQLDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// AddCompanyUsersToLogin - Associa più utenti ad una login
	/// </summary>
	//=========================================================================
	public partial class AddCompanyUsersToLogin : PlugInsForm
	{
		#region Events and Delegates
		public delegate void ModifyTreeOfCompanies (object sender, string nodeType,string companyId);
		public event ModifyTreeOfCompanies OnModifyTreeOfCompanies;
		
		public delegate void SaveUsers(object sender, string id, string companyId);
		public event SaveUsers OnSaveUsers;
		
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

		public delegate bool IsActivated(string application, string functionality);
		public event IsActivated OnIsActivated;
		#endregion

		#region Private Variables
		Diagnostic diagnostic = new Diagnostic("SysAdmin.AddCompanyUsersToLogin"); 
		
		private SqlConnection sysDbConnection;
		private string sysDbConnString = string.Empty;
		private string companyId		= string.Empty;

		// dati della login che si vuole aggiungere
		private bool isLoginNT = false;
		private string loginSelected = string.Empty;
		private string loginPassword = string.Empty;

		// variabili di appoggio per il database aziendale
		private TransactSQLAccess companyConnSqlTransact = new TransactSQLAccess();
		private UserImpersonatedData companyImpersonated = new UserImpersonatedData();

		private string dbOwnerLogin		= string.Empty;
		private string dbOwnerPassword  = string.Empty;
		private bool   dbOwnerWinAuth   = false;
		private string dbOwnerDomain    = string.Empty;
		private string dbOwnerPrimary   = string.Empty;
		private string dbOwnerInstance   = string.Empty;
		private string dbCompanyName    = string.Empty;
		private string dbCompanyServer  = string.Empty;
		private bool companyUseSlave = false;
		
		private string companyConnectionString = string.Empty;

		// variabili di appoggio per il database documentale
		private bool isDMSActivated = false;

		private TransactSQLAccess dmsConnSqlTransact = new TransactSQLAccess();
		private UserImpersonatedData dmsImpersonated = new UserImpersonatedData();

		private string dmsDbOwnerLogin = string.Empty;
		private string dmsDbOwnerPassword = string.Empty;
		private bool dmsDbOwnerWinAuth = false;
		private string dmsDbOwnerDomain = string.Empty;
		private string dmsDbOwnerPrimary = string.Empty;
		private string dmsDbOwnerInstance = string.Empty;
		private string dmsDatabaseName = string.Empty;
		private string dmsServerName = string.Empty;

		private string dmsConnectionString = string.Empty;
		//
		#endregion

		/// <summary>
		/// Costruttore con parametri
		/// </summary>
		//---------------------------------------------------------------------
		public AddCompanyUsersToLogin(string connectionStr, SqlConnection connection, string companyId, BrandLoader brandLoader)
		{
			InitializeComponent();
			this.companyId			= companyId;
			this.sysDbConnection	= connection;
			this.sysDbConnString	= connectionStr;
			State = StateEnums.View;
			
			if (brandLoader != null)
			{
				string brandedCompany = brandLoader.GetCompanyName();
				if (brandedCompany != null && brandedCompany.Length > 0)
					LblExplication.Text = LblExplication.Text.Replace(NameSolverStrings.Microarea, brandedCompany);
			}

			companyConnectionString = BuildConnection(this.companyId);
		}

		#region BuildListView
		/// <summary>
		/// BuildListView
		/// Costruisco il layout della ListView
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

		#region LoadAllMicroareaUsers
		/// <summary>
		/// LoadAllMicroareaUsers
		/// Carico tutti gli Utenti Applicativi per i quali NON esiste una Login in SQL pari al loro LogiName 
		/// (non è necessario controllare la password poichè non possono esistere due login, anche con pwd diverse, 
		/// nello stesso server SQL)
		/// </summary>
		//----------------------------------------------------------------------
		private void LoadAllMicroareaUsers()
		{
			UserDb userDb				= new UserDb();
			userDb.ConnectionString		= sysDbConnString;
			userDb.CurrentSqlConnection = sysDbConnection;
			ArrayList users				= new ArrayList();

			if (!userDb.SelectAllUsersExceptSa(out users, false))
				SendToCallerDiagnostic(userDb.Diagnostic);

			for (int i = 0; i < users.Count; i++)
			{
				UserItem itemUser = (UserItem)users[i];
				//se esiste già la login (in MDS_CompanyLogins DBUser = itemUser.Login) non lo mostro
				CompanyUserDb companyUserDb = new CompanyUserDb();
				companyUserDb.ConnectionString = sysDbConnString;
				companyUserDb.CurrentSqlConnection = sysDbConnection;
				
				//Skippa tutti quelli già assegnati su quella azienda
				if (companyUserDb.ExistUser(itemUser.LoginId, companyId) > 0)
				{
					//esiste già una riga in MSD_CompanyLogins: la devo considerare se disable=true e se DBUser="" e DBPassword =""
					bool isAdmin = false, dbWinAuth = false, disabled = false;
					string dbUser = string.Empty, dbPwd = string.Empty;

					if (!companyUserDb.SelectDataForUserCompany(itemUser.LoginId, companyId, out isAdmin, out dbUser, out dbPwd, out dbWinAuth, out disabled))
					{
						SendToCallerDiagnostic(companyUserDb.Diagnostic);
						continue;
					}

					if (!(disabled && string.IsNullOrEmpty(dbUser) && string.IsNullOrEmpty(dbPwd)))
						continue;
				}

				// se è l'utente Guest o EasyLookSystem lo skippo
				if (string.Compare(itemUser.Login, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0 ||
					string.Compare(itemUser.Login, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
				    continue;

				UserListItem listItemUser	= new UserListItem();
				listItemUser.IsModified		= false;
				listItemUser.CompanyId		= companyId;
				listItemUser.LoginId		= itemUser.LoginId;
				listItemUser.Login			= itemUser.Login;
				listItemUser.Description	= itemUser.Description.Replace("\r\n", " ");
				listItemUser.DbPassword     = itemUser.Password;
				listItemUser.DbWindowsAuthentication = itemUser.WindowsAuthentication;
				
				listItemUser.ImageIndex = (itemUser.WindowsAuthentication)
                        ? PlugInTreeNode.GetLoginsDefaultImageIndex
                        : PlugInTreeNode.GetUserDefaultImageIndex;
				
				listItemUser.Text = itemUser.Login;
				
				if (itemUser.Disabled)
				{
					listItemUser.Disabled = true;
					listItemUser.ForeColor = Color.Red;
				}
				else
					listItemUser.Disabled = false;
				
				listItemUser.SubItems.Add(listItemUser.Description.Replace("\r\n", " "));
				if (!companyUserDb.IsDbo(itemUser.LoginId, this.companyId))
					ListViewUsersCompany.Items.Add(listItemUser);
			}

			if (ListViewUsersCompany.Items.Count == 0)
			{
				LblAllUsersJoined.Visible	= true;
				BtnSelectAll.Enabled		= false;
				BtnUnselectAll.Enabled		= false;
				BtnSave.Enabled				= false;
			}
		}
		#endregion

		#region SelectAll e UnselectAll
		/// <summary>
		/// BtnSelectAll_Click
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnSelectAll_Click(object sender, System.EventArgs e)
		{
			for (int i = 0; i < ListViewUsersCompany.Items.Count; i++)
			{
				if (!ListViewUsersCompany.Items[i].Checked)
					ListViewUsersCompany.Items[i].Checked = true;
			}
			
			BtnSelectAll.Enabled = false;
			BtnUnselectAll.Enabled = true;
		}

		/// <summary>
		/// BtnUnselectAll_Click
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnUnselectAll_Click(object sender, System.EventArgs e)
		{
			for (int i = 0; i < ListViewUsersCompany.Items.Count; i++)
			{
				if (ListViewUsersCompany.Items[i].Checked)
					ListViewUsersCompany.Items[i].Checked = false;
			}
			
			BtnSelectAll.Enabled = true;
			BtnUnselectAll.Enabled = false;
		}
		#endregion

		#region Events on form controls
		/// <summary>
		/// ListViewUsersCompany_ItemCheck
		/// </summary>
		//---------------------------------------------------------------------
		private void ListViewUsersCompany_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			//Di default entrambi i bottoni sono abilitati
			BtnSelectAll.Enabled	= true;
			BtnUnselectAll.Enabled	= true;

			//tutti gli oggetti sono selezionati? Disabilito il bottone di SelectAll
			if (ListViewUsersCompany.CheckedItems.Count == ListViewUsersCompany.Items.Count) 
			{ 
				if (e.CurrentValue == CheckState.Unchecked && e.NewValue != CheckState.Unchecked)
					BtnSelectAll.Enabled = false;
			}
			//nessun oggetto è selezionato? Disabilito il bottone di UnSelectAll
			else if (ListViewUsersCompany.CheckedItems.Count == 0)
			{
				if (e.CurrentValue == CheckState.Unchecked && e.NewValue != CheckState.Checked)
					BtnUnselectAll.Enabled = false;
			}

			State = StateEnums.Editing;
		}
		/// <summary>
		/// RadioNewLogin_CheckedChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void RadioNewLogin_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;

			CbLogins.Enabled	= !(((RadioButton)sender).Checked);
			TbLoginName.Enabled = (((RadioButton)sender).Checked);
			TbLoginName.Text	= string.Empty;
			TbLoginPassword.Text= string.Empty;
		}

		/// <summary>
		/// CbLogins_DropDown
		/// </summary>
		//---------------------------------------------------------------------
		private void CbLogins_DropDown(object sender, System.EventArgs e)
		{
			if (CbLogins.Items.Count == 0) 
				LoadLogins();
		}

		/// <summary>
		/// AddCompanyUsersToLogin_Load
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyUsersToLogin_Load(object sender, System.EventArgs e)
		{
			BuildListView();

			LoadAllMicroareaUsers();

			RadioNewLogin.Checked = true;

			if (ListViewUsersCompany.CheckedItems.Count == 0)
			{
				BtnSelectAll.Enabled = true;
				BtnUnselectAll.Enabled = false;
			}
			else
			{
				BtnSelectAll.Enabled = true;
				BtnUnselectAll.Enabled = true;
			}

			State = StateEnums.View;

			// l'evento posso spararlo solo nella Load, perche' nel costruttore non e' ancora stato 
			// agganciato e valorizzato!
			if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				isDMSActivated = true;
		}

		/// <summary>
		/// AddCompanyUsersToLogin_Closing
		/// Invio la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyUsersToLogin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// AddCompanyUsersToLogin_Deactivate
		/// Invio la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyUsersToLogin_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// AddCompanyUsersToLogin_VisibleChanged
		/// Invio la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyUsersToLogin_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}

		/// <summary>
		/// TbLoginName_TextChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void TbLoginName_TextChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		/// <summary>
		/// CbLogins_SelectedIndexChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void CbLogins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
			if (this.CbLogins.SelectedItem == null)
				return;

			string loginName = ((LoginItem)CbLogins.SelectedItem).LoginName;
			if (!string.IsNullOrEmpty(loginName))
			{
				if (string.Compare(loginName, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					LblGuestUser.Visible	= true;
					BtnSelectAll.Enabled	= false;
					BtnUnselectAll.Enabled	= false;
					BtnSave.Enabled			= false;
					groupPassword.Enabled	= false;
					RadioNewLogin.Enabled	= false;
				}
				else
				{
					LblGuestUser.Visible	= false;
					BtnSave.Enabled			= true;
					groupPassword.Enabled	= true;
					RadioNewLogin.Enabled	= true;
				}
			}
		}

		//---------------------------------------------------------------------
		private void CbLogins_EnabledChanged(object sender, System.EventArgs e)
		{
			if (CbLogins.Enabled && CbLogins.Items.Count == 0)
				LoadLogins();
		}
		#endregion

		#region LoadLogins - Carica nella combo le Logins presenti sul server SQL
		/// <summary>
		/// LoadLogins
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadLogins()
		{
			//carico le login del server
			this.CbLogins.Items.Clear();
			Cursor.Current = Cursors.WaitCursor;
			
			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole		+= new TransactSQLAccess.AddUserAuthenticatedFromConsole	(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole	+= new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole	(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole		+= new TransactSQLAccess.IsUserAuthenticatedFromConsole		(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp						+= new TransactSQLAccess.CallHelpFromPopUp(CallHelp);
			
			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();

			if (string.IsNullOrEmpty(companyConnectionString)) //non riesco a connettermi all'azienda.
			{
				this.RadioNewLogin.Checked = true;
				this.CbLogins.Items.Clear();
				this.CbLogins.Enabled = false;
				Cursor.Current = Cursors.Default;
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				return;
			}

			connSqlTransact.CurrentStringConnection = companyConnectionString;
			dataToConnectionServer = connSqlTransact.LoginImpersonification
				(
				this.dbOwnerLogin,
				this.dbOwnerPassword,
				this.dbOwnerDomain,
				this.dbOwnerWinAuth,
				this.dbOwnerPrimary,
				this.dbOwnerInstance,
				false
				);
			
			if (dataToConnectionServer == null) 
			{
				Cursor.Current = Cursors.Default;
				return;
			}
			
			ArrayList loginsOfCompany = new ArrayList(); 
			if (!connSqlTransact.GetLogins(out loginsOfCompany)) // leggo le logins
			{
				//TO DO GESTIRE
				diagnostic.Set(connSqlTransact.Diagnostic);
				diagnostic.Set(DiagnosticType.Error, Strings.NotExistLogins);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				Cursor.Current = Cursors.Default;
				if (dataToConnectionServer != null)
					dataToConnectionServer.Undo();
				return;
			}

			if (loginsOfCompany.Count > 0)
			{
				for (int i = 0; i < loginsOfCompany.Count; i++)
				{
					CompanyLogin companyLogin = (CompanyLogin)loginsOfCompany[i];
					//se è un gruppo NT, un utente NT (locale o no) l'utente sa lo skippo
					if (companyLogin.IsNTGroup || companyLogin.IsLocalNTUser || companyLogin.IsSaUser || companyLogin.IsNTUser)
						continue;
					// se è l'utente Guest o EasyLookSystem lo skippo
					if (string.Compare(companyLogin.Login, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0 ||
						string.Compare(companyLogin.Login, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
					    continue;

					CbLogins.Items.Add(new LoginItem(companyLogin.Login, companyLogin.IsNTUser));
				}
				
				CbLogins.DisplayMember = "loginName";
				CbLogins.ValueMember   = "isNTUser";
				//seleziono il primo
				if (CbLogins.Items.Count > 0)
					CbLogins.SelectedIndex = 0;
			}

			Cursor.Current = Cursors.Default;
		}
		#endregion

		# region CheckData
		/// <summary>
		/// CheckData
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckData()
		{
			bool checkSuccess = true;
			if (RadioSelectLogin.Checked)
			{
				if (this.CbLogins.SelectedItem == null)
				{
					diagnostic.Set(DiagnosticType.Information, Strings.NotSelectedLogins);
					checkSuccess = false;
				}
			}
			else if (RadioNewLogin.Checked)
			{
				if (string.IsNullOrEmpty(TbLoginName.Text))
				{
					diagnostic.Set(DiagnosticType.Information, Strings.NotSelectedLogins);
					checkSuccess = false;
				}
			}
			if (this.ListViewUsersCompany.CheckedItems.Count == 0)
			{
				diagnostic.Set(DiagnosticType.Information, Strings.NotSelectedCompanyUsers);
				checkSuccess = false;
			}

			if (!checkSuccess)
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			return checkSuccess;
		}
		# endregion

		# region Save
		/// <summary>
		/// Save
		/// </summary>
		//---------------------------------------------------------------------
		public bool Save()
		{
			bool result = false;

			if (CheckData())
			{
				//Costruisco la stringa di connessione all'azienda selezionata (eventualmente
				//impersonificando il dbowner dell'azienda in questione
				TransactSQLAccess connSqlTransact = new TransactSQLAccess();
				connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
				connSqlTransact.OnAddUserAuthenticatedFromConsole		+= new TransactSQLAccess.AddUserAuthenticatedFromConsole	(AddUserAuthentication);
				connSqlTransact.OnGetUserAuthenticatedPwdFromConsole	+= new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole	(GetUserAuthenticatedPwd);
				connSqlTransact.OnIsUserAuthenticatedFromConsole		+= new TransactSQLAccess.IsUserAuthenticatedFromConsole		(IsUserAuthenticated);
				connSqlTransact.OnCallHelpFromPopUp						+= new TransactSQLAccess.CallHelpFromPopUp(CallHelp);
			
				UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
				if (string.IsNullOrEmpty(companyConnectionString))
				{
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					State = StateEnums.Editing;
					return false;
				}

				connSqlTransact.CurrentStringConnection = companyConnectionString;
				//eventualmente eseguo l'impersonificazione
				dataToConnectionServer = connSqlTransact.LoginImpersonification
					(
					this.dbOwnerLogin,
					this.dbOwnerPassword,
					this.dbOwnerDomain,
					this.dbOwnerWinAuth,
					this.dbOwnerPrimary,
					this.dbOwnerInstance,
					false
					);
				
				if (dataToConnectionServer == null) 
				{
					Cursor.Current = Cursors.Default;
					State = StateEnums.Editing;
					return result;
				}
				//Step 1a. Se la login è nuova, la inserisco, controllando che non esista
				//        nel caso dò un messaggio di errore e mi fermo
				if (RadioNewLogin.Checked)
				{
					loginSelected = TbLoginName.Text;
					loginPassword = TbLoginPassword.Text;
					isLoginNT = false;
					
					if (connSqlTransact.ExistLogin(loginSelected))
					{
						diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.LoginAlreadyExist, loginSelected));
						diagnostic.Set(connSqlTransact.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						State = StateEnums.Editing;
						return result;
					}
					if (!AddLoginToCompany(isLoginNT, loginSelected, loginPassword, connSqlTransact))
					{
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotAddLogin, loginSelected));
						diagnostic.Set(connSqlTransact.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						State = StateEnums.Editing;
						return result;
					}
				}
				//Step 1b.  Se ho scelto una login esistente, provo a connettermi se non ci
				//        riesco, dò un messaggio di errore e mi fermo
				else if (RadioSelectLogin.Checked)
				{
					if (this.CbLogins.SelectedItem == null)
					{
						State = StateEnums.Editing;
						return result;
					}

					loginSelected = ((LoginItem)CbLogins.SelectedItem).LoginName;
					loginPassword = TbLoginPassword.Text;
					isLoginNT     = ((LoginItem)CbLogins.SelectedItem).IsNTUser;
					
					if (connSqlTransact.ExistLogin(loginSelected))
					{
						bool resultGrant = false;
						if (!connSqlTransact.ExistUserIntoDb(loginSelected, this.dbCompanyName))
							resultGrant = GrantLoginToCompany(isLoginNT, loginSelected, loginPassword, connSqlTransact) &
										  TryToConnect(isLoginNT, loginSelected, loginPassword);
						else
							resultGrant = TryToConnect(isLoginNT, loginSelected, loginPassword);
						
						if (!resultGrant)
						{
							diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CannotConnectWithLogin);
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							State = StateEnums.Editing;
							return result;
						}
						else
							State = StateEnums.View;
					}
				}

				//Step 2. Aggiorno la MSD_CompanyLogin aggiungendo /modificando le righe e
				//        impostando la DBUser=LoginName e DBPassword=Password
				if (UpdatedCompanyLogins(isLoginNT, loginSelected, loginPassword, connSqlTransact ))
				{
					State = StateEnums.View;
					if (OnModifyTreeOfCompanies != null)
						OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
					if (OnModifyTreeOfCompanies != null)
						OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);
					result = true;
				}
				else
					State = StateEnums.Editing;
			}

			return result;
		}
		# endregion

		/// <summary>
		/// Nuovo metodo di salvataggio (con il database documentale)
		/// </summary>
		//---------------------------------------------------------------------
		public bool NewSave()
		{
			// pulisco il diagnostico, in modo da non visualizzare vecchi messaggi
			diagnostic.Clear();

			bool result = false;

            //@@TODO: decidere se mettere il controllo dell'attivazione del modulo Easy Attachment
			bool dmsToManage = false;

			if (CheckData())
			{
				// memorizzo la login e la password da aggiungere
				if (RadioNewLogin.Checked)
				{
					loginSelected = TbLoginName.Text;
					loginPassword = TbLoginPassword.Text;
					isLoginNT = false;
				}
				if (RadioSelectLogin.Checked)
				{
					if (this.CbLogins.SelectedItem == null)
					{
						State = StateEnums.Editing;
						return result;
					}

					loginSelected = ((LoginItem)CbLogins.SelectedItem).LoginName;
					loginPassword = TbLoginPassword.Text;
					isLoginNT = ((LoginItem)CbLogins.SelectedItem).IsNTUser;
				}
				//

				// mi connetto al database aziendale, con le credenziali del dbowner
				result = ConnectToCompanyDatabase();

				// se non sono riuscita a connettermi al database aziendale non procedo
				if (!result)
					return result; // messaggio di errore

				// se il modulo dms e' attivato e l'azienda ha uno slave associato procedo con i controlli sul database
				if (isDMSActivated && companyUseSlave)
				{
					result = ConnectToDmsDatabase();

					// se non sono riuscita a connettermi al database documentale non procedo
					if (!result)
						return result; // messaggio di errore

					dmsToManage = true;
				}

				// se devo gestire il database dms eseguo dei controlli aggiuntivi
				if (dmsToManage)
				{
					// confronto i nomi dei server dei due database
					bool sameServer = string.Compare(dbCompanyServer, dmsServerName, StringComparison.InvariantCultureIgnoreCase) == 0;

					// a seconda del fatto di essere sullo stesso server o meno richiamo due procedure differenti
					// perche' sono diversi i controlli da effettuare
					result = (sameServer) ? AddUsersOnSameServer(dmsToManage) : AddUsersOnDifferentServer(dmsToManage);
				}
				else
					result = AddUsersOnSameServer(dmsToManage);

				if (result)
				{
					//Step 2. Aggiorno la MSD_CompanyLogins (e l'eventuale MSD_SlaveLogins) 
					//		aggiungendo/modificando le righe ed impostando la DBUser=LoginName e DBPassword=Password
					if (UpdateSystemDBTables(isLoginNT, loginSelected, loginPassword, dmsToManage))
					{
						State = StateEnums.View;
						if (OnModifyTreeOfCompanies != null)
							OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
						if (OnModifyTreeOfCompanies != null)
							OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);
						result = true;
					}
					else
						State = StateEnums.Editing;
				}

				if (companyImpersonated != null)
					companyImpersonated.Undo();
				if (dmsImpersonated != null)
					dmsImpersonated.Undo();
			}

			return result;
		}

		///<summary>
		/// AddUsersOnSameServer
		/// Richiamata se i due database sono sul medesimo server (oppure devo gestire solo il database aziendale)
		///</summary>
		//---------------------------------------------------------------------
		private bool AddUsersOnSameServer(bool dmsToManage)
		{
			bool result = false;

			//Step 1. Se la login è nuova, la inserisco, controllando che non esista
			//        nel caso dò un messaggio di errore e mi fermo
			if (RadioNewLogin.Checked)
			{
				// se la login esiste sul server del db aziendale non procedo perche' non posso inserirla
				if (companyConnSqlTransact.ExistLogin(loginSelected))
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.LoginAlreadyExist, loginSelected));
					diagnostic.Set(companyConnSqlTransact.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					State = StateEnums.Editing;
					return result;
				}

				result = (dmsToManage) ? CheckTwoDatabases(false) : CheckOneDatabase(false);
			}

			//Step 1b.  Se ho scelto una login esistente, provo a connettermi se non ci
			//        riesco, dò un messaggio di errore e mi fermo
			if (RadioSelectLogin.Checked)
			{
				if (companyConnSqlTransact.ExistLogin(loginSelected))
				{
					result = (dmsToManage) ? CheckTwoDatabases(true) : CheckOneDatabase(true);

					if (!result)
					{
						diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CannotConnectWithLogin);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						State = StateEnums.Editing;
						return result;
					}
				}
			}

			State = StateEnums.View;
			return result;
		}

		//---------------------------------------------------------------------
		private bool CheckOneDatabase(bool existLoginOnCompanyServer)
		{
			bool result = false;

			// se la login non esiste sul server la aggiungo
			if (!existLoginOnCompanyServer)
			{
				if (!companyConnSqlTransact.SPAddLogin(loginSelected, loginPassword, DatabaseLayerConsts.MasterDatabase))
				{
					if (companyConnSqlTransact.Diagnostic.Error)
						diagnostic.Set(companyConnSqlTransact.Diagnostic);
					return result;
				}
			}
			else //altrimenti provo a connettermi con le credenziali indicate
				if (!NewTryToConnectSpecificDB(isLoginNT, loginSelected, loginPassword, DatabaseLayerConsts.MasterDatabase, false))
				{
					// se non riesco visualizzo un msg e non procedo
					if (diagnostic.Error)
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					return result;
				}


			bool existLoginOnCompanyDb = companyConnSqlTransact.ExistUserIntoDb(loginSelected, this.dbCompanyName);

			// se la login esiste sul database aziendale provo a connettermi con la pw indicata
			if (existLoginOnCompanyDb)
				result = NewTryToConnect(isLoginNT, loginSelected, loginPassword, false);
			else
				// se la login non esiste devo aggiugerla e grantarla
				result = NewGrantLogin(isLoginNT, loginSelected, loginPassword, false);

			return result;
		}

		//---------------------------------------------------------------------
		private bool CheckTwoDatabases(bool existLoginOnCompanyServer)
		{
			bool result = false;

			// se la login non esiste sul server la aggiungo
			if (!existLoginOnCompanyServer)
			{
				if (!companyConnSqlTransact.SPAddLogin(loginSelected, loginPassword, DatabaseLayerConsts.MasterDatabase))
				{
					if (companyConnSqlTransact.Diagnostic.Error)
						diagnostic.Set(companyConnSqlTransact.Diagnostic);
					return result;
				}
			}
			else //altrimenti provo a connettermi con le credenziali indicate
				if (!NewTryToConnectSpecificDB(isLoginNT, loginSelected, loginPassword, DatabaseLayerConsts.MasterDatabase, false))
				{
					// se non riesco visualizzo un msg e non procedo
					if (diagnostic.Error)
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					return result;
				}

			bool existLoginOnCompanyDb = companyConnSqlTransact.ExistUserIntoDb(loginSelected, this.dbCompanyName);
			bool existLoginOnDmsDb = dmsConnSqlTransact.ExistUserIntoDb(loginSelected, this.dmsDatabaseName);

			// se la login non esiste in entrambi i database la granto subito su tutti e 2
			if (!existLoginOnCompanyDb && !existLoginOnDmsDb)
			{
				// se la login esiste sul server devo fare due grant su entrambi i database
				result = NewGrantLogin(isLoginNT, loginSelected, loginPassword, false) &&
							NewGrantLogin(isLoginNT, loginSelected, loginPassword, true);
			}

			// se la login esiste sul database aziendale e NON esiste su quello documentale (e viceversa)
			// prima provo a connettermi e poi effettuo il grant della login
			if (existLoginOnCompanyDb && !existLoginOnDmsDb)
			{
				if (NewTryToConnect(isLoginNT, loginSelected, loginPassword, false))
					result = NewAddLogin(isLoginNT, loginSelected, loginPassword, true);
			}
			if (!existLoginOnCompanyDb && existLoginOnDmsDb)
			{
				if (NewTryToConnect(isLoginNT, loginSelected, loginPassword, true))
					result = NewAddLogin(isLoginNT, loginSelected, loginPassword, false);
			}

			// se la login esiste in entrambi i server devo provare a connettermi per individuare 
			// se la login inserita e' corretta
			if (existLoginOnCompanyDb && existLoginOnDmsDb)
			{
				result = NewTryToConnect(isLoginNT, loginSelected, loginPassword, false) &&
							NewTryToConnect(isLoginNT, loginSelected, loginPassword, true);
			}

			return result;
		}

		///<summary>
		/// AddUsersOnDifferentServer
		/// Richiamata se i due database sono su due server differenti
		///</summary>
		//---------------------------------------------------------------------
		private bool AddUsersOnDifferentServer(bool dmsToManage)
		{
			bool result = false;

			//Step 1. Se la login è nuova, la inserisco, controllando che non esista
			//        nel caso dò un messaggio di errore e mi fermo
			if (RadioNewLogin.Checked)
			{
				// se la login esiste sul server non procedo perche' non posso inserirla
				if (companyConnSqlTransact.ExistLogin(loginSelected))
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.LoginAlreadyExist, loginSelected));
					diagnostic.Set(companyConnSqlTransact.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					State = StateEnums.Editing;
					return result;
				}

				// aggiungo la login sul database aziendale
				if (!NewAddLogin(isLoginNT, loginSelected, loginPassword, false))
				{
					if (diagnostic.Error)
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					State = StateEnums.Editing;
					return result;
				}

				// se e' andato tutto a buon fine procedo ad aggiungere la login sul database documentale (se presente)
				if (dmsToManage)
				{
					//result = NewAddLogin(isLoginNT, loginSelected, loginPassword, true);
					result = CheckAndSetDmsLoginAccess(isLoginNT, loginSelected, loginPassword);
					
					if (!result)
					{
						if (diagnostic.Error)
							DiagnosticViewer.ShowDiagnostic(diagnostic);
						State = StateEnums.Editing;
						return result;
					}
				}
			}
			
			//Step 1b.  Se ho scelto una login esistente, provo a connettermi.
			//        Se non ci riesco, dò un messaggio di errore e mi fermo
			if (RadioSelectLogin.Checked)
			{
				// se la login selezionata esiste sul server del database aziendale (dovrebbe essere sempre vero)
				if (companyConnSqlTransact.ExistLogin(loginSelected))
				{
					// verifico che la login sia stata assegnata sul database aziendale
					bool existLoginOnCompanyDb = companyConnSqlTransact.ExistUserIntoDb(loginSelected, this.dbCompanyName);

					// se esiste provo a connettermi con le credenziali inserite
					if (existLoginOnCompanyDb)
					{
						if (NewTryToConnect(isLoginNT, loginSelected, loginPassword, false))
						{
                            // se riesco a connettermi devo effettuare dei controlli preventivi sull'eventuale Easy Attachment (se necessario)
							if (dmsToManage)
								result = CheckAndSetDmsLoginAccess(isLoginNT, loginSelected, loginPassword);
						}
						else
						{
							// non riesco a connettermi con le credenziali fornite sul database aziendale
							// visualizzo un msg e non procedo
							if (diagnostic.Error)
								DiagnosticViewer.ShowDiagnostic(diagnostic);
							return false;
						}
					}
					else
					{ 
						// non esiste la login sul database aziendale, quindi devo grantarla
						// prima pero' devo fare i controlli sull'eventuale server e database documentale (se esiste)
						if (dmsToManage)
						{
							result = CheckAndSetDmsLoginAccess(isLoginNT, loginSelected, loginPassword);

							// se sono riuscita ad impostare le login sul database dms allora continuo con
							// il database aziendale
							if (result)
							{
								result = NewGrantLogin(isLoginNT, loginSelected, loginPassword, false);
								if (!result)
								{
									if (diagnostic.Error)
										DiagnosticViewer.ShowDiagnostic(diagnostic);
									return result;
								}
							}
						}
						else
						{
							// eseguo direttamente il grant della login sul database aziendale
							result = NewGrantLogin(isLoginNT, loginSelected, loginPassword, false);
							if (!result)
							{
								if (diagnostic.Error)
									DiagnosticViewer.ShowDiagnostic(diagnostic);
								return result;
							}
						}
					}
				}
				else 
				{
					// la login non esiste neppure sul server del database aziendale quindi non procedo
					// visualizzo un msg e non procedo
					if (diagnostic.Error)
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					return false;
				}
			}		
			
			State = StateEnums.View;
			return result;
		}

		///<summary>
		/// CheckDmsLoginAccess
		/// Effettua controlli di presenza di login sul server/database dms e li aggiunge di conseguenza
		///</summary>
		//---------------------------------------------------------------------
		private bool CheckAndSetDmsLoginAccess(bool isLoginNT, string loginName, string loginPassword)
		{
			bool result = false;

            // controllo se la login esiste sul server del Easy Attachment
			if (dmsConnSqlTransact.ExistLogin(loginName))
			{
                // verifico che la login sia stata assegnata al Easy Attachment
				bool existLoginOnDmsDb = dmsConnSqlTransact.ExistUserIntoDb(loginName, this.dmsDatabaseName);

				// se esiste provo a connettermi con le credenziali inserite
				if (existLoginOnDmsDb)
				{
					result = NewTryToConnect(isLoginNT, loginName, loginPassword, true);

					// se non riesco a connettermi, significa che la password non va bene
					if (!result)
					{
						// se non riesco visualizzo un msg e non procedo
						if (diagnostic.Error)
							DiagnosticViewer.ShowDiagnostic(diagnostic);
						return result;
					}
				}
				else
				{
					// se non esiste effettuo il grant sul database 
					result = NewGrantLogin(isLoginNT, loginName, loginPassword, true);

					if (!result)
					{
						// se non riesco visualizzo un msg e non procedo
						if (diagnostic.Error)
							DiagnosticViewer.ShowDiagnostic(diagnostic);
						return result;
					}
				}
			}
			else
			{
                // la login NON esiste sul server del Easy Attachment, quindi la aggiungo
				result = NewAddLogin(isLoginNT, loginName, loginPassword, true);
				if (!result)
				{
					// se non riesco visualizzo un msg e non procedo
					if (diagnostic.Error)
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					return result;
				}
			}

			return result;
		}

		#region GrantLoginToCompany and AddLoginToCompany
		/// <summary>
		/// GrantLoginToCompany
		/// </summary>
		//---------------------------------------------------------------------
		private bool GrantLoginToCompany(bool isLoginNT, string loginName, string loginPassword, TransactSQLAccess connSqlTransact)
		{
			if (isLoginNT)
			{
				return (connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName));
			}
			else
			{
				return (connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName));
			}
		}

		/// <summary>
		/// AddLoginToCompany
		/// </summary>
		//---------------------------------------------------------------------
		private bool AddLoginToCompany(bool isLoginNT, string loginName, string loginPassword, TransactSQLAccess connSqlTransact)
		{
			if (isLoginNT)
			{
				return (connSqlTransact.SPGrantLogin(loginName) &&
						connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName));
			}
			else
			{
				return (connSqlTransact.SPAddLogin(loginName, loginPassword, DatabaseLayerConsts.MasterDatabase) &&
						connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName));
			}
		}

		/// <summary>
		/// NewAddLogin
		/// </summary>
		//---------------------------------------------------------------------
		private bool NewAddLogin(bool isLoginNT, string loginName, string loginPassword, bool isDmsManage)
		{
			string dbName = isDmsManage ? this.dmsDatabaseName : this.dbCompanyName;
			TransactSQLAccess connSqlTransact = isDmsManage ? dmsConnSqlTransact : companyConnSqlTransact;

			bool result = false;

			if (isLoginNT)
			{
				result = (connSqlTransact.SPGrantLogin(loginName) &&
						connSqlTransact.SPGrantDbAccess(loginName, loginName, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, dbName));
			}
			else
			{
				result = (connSqlTransact.SPAddLogin(loginName, loginPassword, DatabaseLayerConsts.MasterDatabase) &&
						connSqlTransact.SPGrantDbAccess(loginName, loginName, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, dbName));
			}

			if (!result)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotAddLogin, loginName));
				diagnostic.Set(connSqlTransact.Diagnostic);
			}

			return result;
		}

		/// <summary>
		/// NewGrantLogin
		/// </summary>
		//---------------------------------------------------------------------
		private bool NewGrantLogin(bool isLoginNT, string loginName, string loginPassword, bool isDmsManage)
		{
			string dbName = isDmsManage ? this.dmsDatabaseName : this.dbCompanyName;
			TransactSQLAccess connSqlTransact = isDmsManage ? dmsConnSqlTransact : companyConnSqlTransact;

			bool result = false;

			if (isLoginNT)
			{
				result = (connSqlTransact.SPGrantDbAccess(loginName, loginName, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, dbName));
			}
			else
			{
				result = (connSqlTransact.SPGrantDbAccess(loginName, loginName, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, dbName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, dbName));
			}

			if (!result)
			{
				diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CannotConnectWithLogin);
				diagnostic.Set(connSqlTransact.Diagnostic);
			}

			return result;
		}

		#endregion

		#region TryToConnect
		/// <summary>
		/// TryToConnect
		/// </summary>
		//---------------------------------------------------------------------
		private bool TryToConnect(bool isLoginNT, string loginName, string loginPassword)
		{
			TransactSQLAccess tentativeConnSql = new TransactSQLAccess();

			tentativeConnSql.CurrentStringConnection = (isLoginNT)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, this.dbCompanyServer,  this.dbCompanyName)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, this.dbCompanyServer, this.dbCompanyName, loginName, loginPassword);

			 return tentativeConnSql.TryToConnect();
		}

		/// <summary>
		/// NewTryToConnect
		/// </summary>
		//---------------------------------------------------------------------
		private bool NewTryToConnect(bool isLoginNT, string loginName, string loginPassword, bool isDmsManage)
		{
			TransactSQLAccess tentativeConnSql = new TransactSQLAccess();

			string serverName = isDmsManage ? dmsServerName : dbCompanyServer;
			string dbName = isDmsManage ? dmsDatabaseName : dbCompanyName;

			tentativeConnSql.CurrentStringConnection = (isLoginNT)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, dbName)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, dbName, loginName, loginPassword);

			bool result = tentativeConnSql.TryToConnect();
			
			if (!result)
			{
				if (tentativeConnSql.Diagnostic.Error)
					diagnostic.Set(tentativeConnSql.Diagnostic);
			}

			return result;
		}

		/// <summary>
		/// NewTryToConnect
		/// </summary>
		//---------------------------------------------------------------------
		private bool NewTryToConnectSpecificDB(bool isLoginNT, string loginName, string loginPassword, string database, bool isDmsManage)
		{
			TransactSQLAccess tentativeConnSql = new TransactSQLAccess();

			string serverName = isDmsManage ? dmsServerName : dbCompanyServer;

			tentativeConnSql.CurrentStringConnection = (isLoginNT)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, database)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, database, loginName, loginPassword);

			bool result = tentativeConnSql.TryToConnect();

			if (!result)
			{
				if (tentativeConnSql.Diagnostic.Error)
					diagnostic.Set(tentativeConnSql.Diagnostic);
			}

			return result;
		}

		#endregion

		/// <summary>
		/// UpdateSystemDBTables
		/// esegue l'aggiornamento delle login sulle tabelle del database di sistema
		/// (MSD_CompanyLogins e MSD_SlaveLogins)
		/// </summary>
		//---------------------------------------------------------------------
		private bool UpdateSystemDBTables(bool isLoginNT, string loginName, string loginPassword, bool isDmsManage)
		{
			bool success = false;

			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = this.sysDbConnString;
			companyUserDb.CurrentSqlConnection = this.sysDbConnection;

			CompanyDBSlave companyDBSlave = new CompanyDBSlave();
			companyDBSlave.ConnectionString = this.sysDbConnString;
			companyDBSlave.CurrentSqlConnection = this.sysDbConnection;

			// scorro tutti gli utenti applicativi selezionati nella griglia
			for (int i = 0; i < this.ListViewUsersCompany.CheckedItems.Count; i++)
			{
				UserListItem currentItemSelected = (UserListItem)ListViewUsersCompany.CheckedItems[i];
				currentItemSelected.DbUser = loginName;

				currentItemSelected.IsAdmin =
					(string.Compare(currentItemSelected.Login, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0);

				currentItemSelected.DbPassword = loginPassword;
				currentItemSelected.DbWindowsAuthentication = isLoginNT;
				currentItemSelected.Disabled = false;

				//se l'utente esiste già associato lo cambio
				bool result = (companyUserDb.ExistUser(currentItemSelected.LoginId, currentItemSelected.CompanyId) == 1)
								? companyUserDb.Modify(currentItemSelected)
								: companyUserDb.Add(currentItemSelected);

				if (!result)
				{
					SendToCallerDiagnostic(companyUserDb.Diagnostic);
					success = false;
				}
				else
					success = true;

				// se sono riuscita ad aggiornare la tabella MSD_CompanyLogins
				// procedo ad aggiornare anche la MSD_SlaveLogins
				if (success && isDmsManage)
				{
					// leggo il record associato alla company nella tabella MSD_CompanyDBSlaves, per avere lo slaveId
					CompanyDBSlaveItem dbSlaveItem;
					if (companyDBSlave.SelectSlaveForCompanyIdAndSignature(currentItemSelected.CompanyId, DatabaseLayerConsts.DMSSignature, out dbSlaveItem))
					{
						SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
						slaveLoginDb.ConnectionString = this.sysDbConnString;
						slaveLoginDb.CurrentSqlConnection = this.sysDbConnection;

						if (!slaveLoginDb.ExistLoginForSlaveId(currentItemSelected.LoginId, dbSlaveItem.SlaveId))
							slaveLoginDb.Add(currentItemSelected, dbSlaveItem.SlaveId);
						else
							slaveLoginDb.Modify(currentItemSelected, dbSlaveItem.SlaveId);
					}
				}
			}

			return success;
		}

		#region UpdatedCompanyLogins
		/// <summary>
		/// UpdatedCompanyLogins
		/// </summary>
		//---------------------------------------------------------------------
		private bool UpdatedCompanyLogins(bool isLoginNT, string loginName, string loginPassword, TransactSQLAccess connSqlTransact)
		{
			bool updated = false;

			CompanyUserDb companyUserDb			= new CompanyUserDb();
			companyUserDb.ConnectionString		= this.sysDbConnString;
			companyUserDb.CurrentSqlConnection	= this.sysDbConnection;
			
			for (int i = 0; i < this.ListViewUsersCompany.CheckedItems.Count; i++)
			{
				UserListItem currentItemSelected = (UserListItem)ListViewUsersCompany.CheckedItems[i];
				currentItemSelected.DbUser = loginName;

				currentItemSelected.IsAdmin = 
					(string.Compare(currentItemSelected.Login, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0);

				currentItemSelected.DbPassword = loginPassword;
				currentItemSelected.DbWindowsAuthentication = isLoginNT;
				currentItemSelected.Disabled = false;
				
				//se l'utente esiste già associato lo cambio
				bool result = 
					(companyUserDb.ExistUser(currentItemSelected.LoginId, currentItemSelected.CompanyId) == 1)
					? companyUserDb.Modify(currentItemSelected)
					: companyUserDb.Add(currentItemSelected);

				if (!result)
				{
					SendToCallerDiagnostic(companyUserDb.Diagnostic);
					updated = false;
				}
				else
					updated = true;
			}
			
			return updated;
		}
		#endregion

		#region BtnSave_Click - Bottone di Save
		/// <summary>
		/// BtnSave_Click
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			if (OnSaveUsers != null)
				OnSaveUsers(sender, string.Empty, this.companyId);
		}
		#endregion

		#region BuildConnection - Costruisco stringa di connessione
		/// <summary>
		/// BuildConnection
		/// </summary>
		//---------------------------------------------------------------------
		private string BuildConnection(string companyId)
		{
			string connectionToCompanyServer = string.Empty, dbOwnerId = string.Empty;
		
			CompanyDb companyDb           = new CompanyDb();
			companyDb.ConnectionString    = this.sysDbConnString;
			companyDb.CurrentSqlConnection= this.sysDbConnection;
			ArrayList companyData         = new ArrayList();
			
			if (!companyDb.GetAllCompanyFieldsById(out companyData, companyId))
			{
				SendToCallerDiagnostic(companyDb.Diagnostic);
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
					this.dbOwnerInstance = serverDbInformation[1];
				this.dbCompanyName = companyItem.DbName;

				//Ora leggo le credenziali del dbo dal MSD_CompanyLogins
				CompanyUserDb companyUser = new CompanyUserDb();
				companyUser.ConnectionString = this.sysDbConnString;
				companyUser.CurrentSqlConnection = this.sysDbConnection;
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
						connectionToCompanyServer = string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, this.dbCompanyServer, this.dbCompanyName);
						this.dbOwnerDomain = Path.GetDirectoryName(this.dbOwnerLogin);
					}
					else
						connectionToCompanyServer = string.Format(NameSolverDatabaseStrings.SQLConnection, this.dbCompanyServer, this.dbCompanyName, this.dbOwnerLogin, this.dbOwnerPassword);
				}
			}
			return connectionToCompanyServer;
		}
		#endregion

		///<summary>
		/// ConnectToCompanyDatabase
		/// Metodo che si occupa di effettuare una connessione al database aziendale
		/// con le credenziali di amministrazione.
		///</summary>
		//----------------------------------------------------------------------
		private bool ConnectToCompanyDatabase()
		{
			bool isValidConnection = false;

			// CONNESSIONE AL DATABASE AZIENDALE
			companyConnSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			companyConnSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			companyConnSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			companyConnSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			companyConnSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

			// compongo la stringa di connessione per l'azienda
			string companyConnectionString = CreateCompanyConnectionString();
			if (string.IsNullOrEmpty(companyConnectionString))
			{
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				State = StateEnums.Editing;
				return isValidConnection;
			}

			companyConnSqlTransact.CurrentStringConnection = companyConnectionString;
			// eventualmente eseguo l'impersonificazione
			companyImpersonated = companyConnSqlTransact.LoginImpersonification
				(
				this.dbOwnerLogin,
				this.dbOwnerPassword,
				this.dbOwnerDomain,
				this.dbOwnerWinAuth,
				this.dbOwnerPrimary,
				this.dbOwnerInstance,
				false
				);
			if (companyImpersonated == null)
			{
				Cursor.Current = Cursors.Default;
				State = StateEnums.Editing;
				return isValidConnection;
			}
			else
				isValidConnection = true;

			return isValidConnection;
		}

		/// <summary>
		/// CreateCompanyConnectionString
		/// Compone la stringa di connessione al database aziendale
		/// </summary>
		//---------------------------------------------------------------------
		private string CreateCompanyConnectionString()
		{
			string connectionToCompanyServer = string.Empty, dbOwnerId = string.Empty;

			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.sysDbConnString;
			companyDb.CurrentSqlConnection = this.sysDbConnection;

			ArrayList companyData = new ArrayList();
			if (!companyDb.GetAllCompanyFieldsById(out companyData, this.companyId))
			{
				SendToCallerDiagnostic(companyDb.Diagnostic);
				companyData.Clear();
			}

			if (companyData.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)companyData[0];
				this.dbCompanyServer = companyItem.DbServer;
				this.dbCompanyName = companyItem.DbName;
				dbOwnerId = companyItem.DbOwner;
				this.companyUseSlave = companyItem.UseDBSlave;

				//setto le info generali sul dbName (Istanza primaria o secondaria)
				string[] serverDbInformation = companyItem.DbServer.Split(Path.DirectorySeparatorChar);
				this.dbOwnerPrimary = serverDbInformation[0];
				if (serverDbInformation.Length > 1)
					this.dbOwnerInstance = serverDbInformation[1];

				//Ora leggo le credenziali del dbo dal MSD_CompanyLogins
				CompanyUserDb companyUser = new CompanyUserDb();
				companyUser.ConnectionString = this.sysDbConnString;
				companyUser.CurrentSqlConnection = this.sysDbConnection;

				ArrayList userDboData = new ArrayList();
				companyUser.GetUserCompany(out userDboData, dbOwnerId, this.companyId);
				if (userDboData.Count > 0)
				{
					CompanyUser companyDbo = (CompanyUser)userDboData[0];
					this.dbOwnerLogin = companyDbo.DBDefaultUser;
					this.dbOwnerPassword = companyDbo.DBDefaultPassword;
					this.dbOwnerWinAuth = companyDbo.DBWindowsAuthentication;

					//ora compongo la stringa di connessione
					if (this.dbOwnerWinAuth)
					{
						connectionToCompanyServer = string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, this.dbCompanyServer, this.dbCompanyName);
						this.dbOwnerDomain = Path.GetDirectoryName(this.dbOwnerLogin);
					}
					else
						connectionToCompanyServer = string.Format(NameSolverDatabaseStrings.SQLConnection, this.dbCompanyServer, this.dbCompanyName, this.dbOwnerLogin, this.dbOwnerPassword);
				}
			}
			return connectionToCompanyServer;
		}

		///<summary>
		/// ConnectToDmsDatabase
		/// Metodo che si occupa di effettuare una connessione al database documentale
		/// con le credenziali di amministrazione.
		///</summary>
		//----------------------------------------------------------------------
		private bool ConnectToDmsDatabase()
		{
			bool isValidConnection = false;

			// CONNESSIONE AL DATABASE DOCUMENTALE
			dmsConnSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			dmsConnSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			dmsConnSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			dmsConnSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			dmsConnSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

			// se l'azienda gestisce il database documentale, devo caricare anche le sue informazioni
			if (companyUseSlave)
			{
				// compongo la stringa di connessione per il database documentale
				string dmsConnectionString = CreateDmsConnectionString();
				if (string.IsNullOrEmpty(dmsConnectionString))
				{
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					State = StateEnums.Editing;
					return isValidConnection;
				}

				dmsConnSqlTransact.CurrentStringConnection = dmsConnectionString;
				// eventualmente eseguo l'impersonificazione
				dmsImpersonated = dmsConnSqlTransact.LoginImpersonification
					(
					dmsDbOwnerLogin,
					dmsDbOwnerPassword,
					dmsDbOwnerDomain,
					dmsDbOwnerWinAuth,
					dmsDbOwnerPrimary,
					dmsDbOwnerInstance,
					false
					);

				if (dmsImpersonated == null)
				{
					Cursor.Current = Cursors.Default;
					State = StateEnums.Editing;
					isValidConnection = false;
					return isValidConnection;
				}
				else
					isValidConnection = true;
			}

			return isValidConnection;
		}

		///<summary>
		/// CreateDmsConnectionString
		/// Accedo al database di sistema e leggo tutte le informazioni per comporre la stringa 
		/// di connessione al database documentale
		///</summary>
		//----------------------------------------------------------------------
		private string CreateDmsConnectionString()
		{
			string dmsConnectionString = string.Empty;

			// devo verificare se c'e' uno slave associato all'azienda
			CompanyDBSlave dbSlave = new CompanyDBSlave();
			dbSlave.CurrentSqlConnection = this.sysDbConnection;
			dbSlave.ConnectionString = this.sysDbConnString;
			CompanyDBSlaveItem slaveItem;
			dbSlave.SelectSlaveForCompanyId(this.companyId, out slaveItem);

			if (slaveItem == null)
				return dmsConnectionString;

			dmsDatabaseName = slaveItem.DatabaseName;
			dmsServerName = slaveItem.ServerName;
			dmsDbOwnerPrimary = slaveItem.ServerName.Split(Path.DirectorySeparatorChar)[0];
			dmsDbOwnerInstance = slaveItem.ServerName.Split(Path.DirectorySeparatorChar).Length > 1
								? slaveItem.ServerName.Split(Path.DirectorySeparatorChar)[1]
								: string.Empty;

			// carico le info di connessione per l'utente dbowner del dms
			SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
			slaveLoginDb.CurrentSqlConnection = this.sysDbConnection;
			slaveLoginDb.ConnectionString = this.sysDbConnString;
			SlaveLoginItem loginItem;
			slaveLoginDb.SelectAllForSlaveAndLogin(slaveItem.SlaveId, slaveItem.SlaveDBOwner, out loginItem);

			if (loginItem == null)
				return dmsConnectionString;

			dmsDbOwnerLogin = loginItem.SlaveDBUser;
			dmsDbOwnerPassword = loginItem.SlaveDBPassword;
			dmsDbOwnerWinAuth = loginItem.SlaveDBWinAuth;

			//ora compongo la stringa di connessione
			if (dmsDbOwnerWinAuth)
			{
				dmsConnectionString = string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, dmsServerName, dmsDatabaseName);
				dmsDbOwnerDomain = Path.GetDirectoryName(dmsDbOwnerLogin);
			}
			else
				dmsConnectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, dmsServerName, dmsDatabaseName, dmsDbOwnerLogin, dmsDbOwnerPassword);

			return dmsConnectionString;
		}

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

		# region Gestione Help
		/// <summary>
		/// Funzione generale da richiamare nella form che invia il Diagnostic specifico (passato come parametro)
		/// al Diagnostic del chiamante (SysAdmin)
		/// </summary>
		/// <param name="currentDiagnostic">oggetto Diagnostic da controllare</param>
		//---------------------------------------------------------------------
		private void SendToCallerDiagnostic(Diagnostic currentDiagnostic)
		{
			if (currentDiagnostic.Error || currentDiagnostic.Warning || currentDiagnostic.Information)
			{
				diagnostic.Set(currentDiagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}
		}
				
		//---------------------------------------------------------------------
		private void CallHelp(object sender, string nameSpace, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, nameSpace, searchParameter);
		}
		# endregion
	}

	# region LoginItem class
	///<summary>
	/// Classe di appoggio per caricare le logins dentro una combobox
	///</summary>
	//=========================================================================
	public class LoginItem
	{
		private string loginName = string.Empty;
		private bool   isNTUser  = false;

		public string LoginName	{ get { return loginName; } set { loginName = value; } }
		public bool   IsNTUser  { get { return isNTUser;  } set { isNTUser  = value; } }

		//---------------------------------------------------------------------
		public LoginItem(string loginName, bool isNTUser)
		{
			LoginName = loginName;
			IsNTUser  = isNTUser;
		}
	}
	# endregion
}