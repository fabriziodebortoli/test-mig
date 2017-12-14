using System;
using System.Collections;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Npgsql;
using Microarea.TaskBuilderNet.Data.PostgreDataAccess;
using Microarea.TaskBuilderNet.Interfaces;
using System.Data;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// ModifyCompanyUsersToLogin.
	/// </summary>
	//=========================================================================
	public partial class ModifyCompanyUsersToLoginPostgre : PlugInsForm
	{
		#region Varibili private
		private string			connectionString;
		private SqlConnection	currentConnection;
		private string			companyId;
		private string 			dbOwnerLogin	= string.Empty;
		private string 			dbOwnerPassword = string.Empty;
		private bool   			dbOwnerWinAuth  = false;
		private string 			dbOwnerDomain   = string.Empty;
		private string 			dbOwnerPrimary  = string.Empty;
		private string 			dbOwnerIstance  = string.Empty;
		private string 			dbCompanyName   = string.Empty;
		private string 			dbCompanyServer = string.Empty;
        private int port = 0;
		
		Diagnostic diagnostic = new Diagnostic("SystemAdminPluIn.ModifyCompanyUsersToLogin");

		enum LoginType { HimSelfAndSqlLogins , SqlLogin , Undefined};

		private System.ComponentModel.Container components = null;
		#endregion

		#region Eventi e Delegati
		public delegate void	ModifyTreeOfCompanies(object sender, string nodeType,string companyId);
		public event			ModifyTreeOfCompanies OnModifyTreeOfCompanies;

		public delegate void	ModifyTree(object sender, string nodeType);
		public event			ModifyTree OnModifyTree;

		public delegate void	SaveUsers(object sender, string id, string companyId);
		public event            SaveUsers OnSaveUsers;
		
		public delegate bool    AfterDisableUser(object sender, int loginId, int companyId);
		public event			AfterDisableUser OnAfterDisableUser;

		public delegate bool    DeleteAssociation(object sender, int loginId, int companyId);
		public event			DeleteAssociation OnDeleteAssociation;

		public delegate bool    UnLockAllForUser(object sender, string userName);
		public event			UnLockAllForUser OnUnLockAllForUser;

		public delegate bool	IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event			IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;

		public delegate void	AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event			AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;

		public delegate string	GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event			GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;

		public delegate void    CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event			CallHelpFromPopUp OnCallHelpFromPopUp;

		public delegate void	SendDiagnostic(object sender, Diagnostic diagnostic);
		public event			SendDiagnostic OnSendDiagnostic;
		#endregion

		/// <summary>
		/// ModifyCompanyUsersToLogin (Costruttore con parametri)
		/// </summary>
		//---------------------------------------------------------------------
        public ModifyCompanyUsersToLoginPostgre(string connectionString, SqlConnection connection, string companyId)
		{
			InitializeComponent();	

			this.companyId			= companyId;
			this.currentConnection	= connection;
			this.connectionString	= connectionString;

            string portQuery = String.Format("SELECT Port from MSD_Companies WHERE CompanyId={0}", companyId);

            if (currentConnection.State == ConnectionState.Closed)
                currentConnection.Open();
            SqlCommand command = new SqlCommand(portQuery, currentConnection);

            try
            {
                this.port = Int32.Parse(command.ExecuteScalar().ToString());
            }
            catch (Exception)
            {
                this.port = 0;
            }

			LabelTitle.Text = Strings.TitleModifyCompanyUsersToLogin;

			BuildListView();

			//disabilito tutte le checkbox fino a che non ho degli elementi checked
			//nella listview
			RbDeleteAll.Enabled			= false;
			RbDisableAll.Enabled		= false;
			RbEnableAll.Enabled			= false;
			RbModifyLogin.Enabled		= false;
			LblPassword.Enabled			= false;
			RbSelectExistLogin.Enabled	= false;
			RbCreateNewLogin.Enabled	= false;
			RbSelectExistLogin.Checked	= true;
			RbCreateNewLogin.Checked	= false;
			ComboExistLogins.Enabled	= false;
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

		#region LoadDbUsers - Carica tutti gli utenti assegnati a una azienda con DbUser = LoginName selezionata e la cui login non sia disabilitata
		/// <summary>
		/// LoadDbUsers
		/// </summary>
		//----------------------------------------------------------------------
		private void LoadPostgreUsers(string loginName)
		{
			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = connectionString;
			companyUserDb.CurrentSqlConnection = currentConnection;
			ArrayList usersOfCompany	= new ArrayList();

			bool result = companyUserDb.SelectAll(out usersOfCompany, this.companyId);
			if (!result)
			{
				if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information || companyUserDb.Diagnostic.Warning)
					diagnostic.Set(companyUserDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				usersOfCompany.Clear();
			}

			//verifico che la login (MSD_Logins) non sia disabilitata - in questo caso non lo mostro
			//nell'elenco
			//if (LoginIsDisabled(itemCompanyUser.LoginId))
			//	continue;
			for (int i = 0; i < usersOfCompany.Count; i++)
			{
				CompanyUser itemCompanyUser = (CompanyUser)usersOfCompany[i];
				//se passo stringa vuota, vuol dire che li voglio tutti
				if (loginName.Length > 0)
				{
					if (string.Compare(itemCompanyUser.DBDefaultUser, loginName, StringComparison.InvariantCultureIgnoreCase) != 0)
						continue;
				}
				
				//skippo anche il dbowner dell'azienda
				UserListItem listItemUser	= new UserListItem();
				listItemUser.IsModified		= false;
				listItemUser.CompanyId		= this.companyId;
				listItemUser.LoginId		= itemCompanyUser.LoginId;
				listItemUser.Login			= itemCompanyUser.Login;
				listItemUser.Description	= itemCompanyUser.Description.Replace("\r\n", " ");
				listItemUser.DbPassword     = itemCompanyUser.DBDefaultPassword;
				listItemUser.DbUser         = itemCompanyUser.DBDefaultUser;
				listItemUser.DbWindowsAuthentication = itemCompanyUser.DBWindowsAuthentication;
				listItemUser.LoginWindowsAuthentication = itemCompanyUser.WindowsAuthentication;
                listItemUser.ImageIndex = (itemCompanyUser.WindowsAuthentication) ? PlugInTreeNode.GetLoginsDefaultImageIndex : PlugInTreeNode.GetUserDefaultImageIndex;
				listItemUser.Text = itemCompanyUser.Login;
				if (itemCompanyUser.Disabled)
				{
					listItemUser.Disabled = true;
					listItemUser.ForeColor	= Color.Red;
				}
				else
					listItemUser.Disabled = false;
				
				listItemUser.SubItems.Add(listItemUser.Description.Replace("\r\n", " "));
				
				//se è il dbowner non lo inserisco
				if (!companyUserDb.IsDbo(itemCompanyUser.LoginId, this.companyId))
					ListViewUsersCompany.Items.Add(listItemUser);
				else
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					string message = string.Format(Strings.ModifyCompanyUsers, this.dbCompanyName, this.dbCompanyServer);
					extendedInfo.Add(Strings.Action, message);
					diagnostic.Set(DiagnosticType.Warning, string.Format(Strings.CannotChangeDbOwnerProperties, listItemUser.DbUser), extendedInfo);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
				}
			}
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



        /// <summary>
        /// TypeOfServerLogins
        /// In base alla tipologia degli utenti selezionati restituisce un enumerativo
        /// che rappresenta la tipologia di login che devono essere caricate dal server
        /// in base alla seguente convenzione:
        /// 1. selezione di 1 solo utente NT -> entrambi i tipi di login (NT lui e SQL)
        /// 2. selezione di più utenti NT    -> solo login SQL
        /// 3. selezione di uno o più utenti non NT -> solo login SQL
        /// </summary>
        //---------------------------------------------------------------------
        private LoginType TypeOfServerLogins()
        {
            int NtUserCount = 0;
            int SqlUserCount = 0;
            for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
            {
                UserListItem selectedUser = (UserListItem)ListViewUsersCompany.CheckedItems[i];
                if (selectedUser.LoginWindowsAuthentication)
                    NtUserCount++;
                else
                    SqlUserCount++;
            }

            if (NtUserCount == 1 && SqlUserCount == 0)
                return LoginType.HimSelfAndSqlLogins;

            // qui non ha importanza se ho anche selezionato degli utenti SQL
            if ((NtUserCount > 0) || (NtUserCount == 0 && SqlUserCount > 0))
                return LoginType.SqlLogin;
            else
                return LoginType.Undefined;
        }

		#region CbLogins_DropDown - Se clicco sulla Drop e non ci sono Logins, le carico
		/// <summary>
		/// CbLogins_DropDown
		/// </summary>
		//---------------------------------------------------------------------
		private void CbLogins_DropDown(object sender, System.EventArgs e)
		{
			if (CbLogins.Items.Count == 0) 
				LoadLogins();
		}
		#endregion

		#region LoadLogins - Carica le Logins disponibili sul server dove risiede il db aziendale
		/// <summary>
		/// LoadLogins
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadLogins()
		{
			//carico le login del server
			CbLogins.Items.Clear();

            PostgreAccess connSqlTransact = new PostgreAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
            connSqlTransact.OnAddUserAuthenticatedFromConsole += new PostgreAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
            connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new PostgreAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
            connSqlTransact.OnIsUserAuthenticatedFromConsole += new PostgreAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
            connSqlTransact.OnCallHelpFromPopUp += new PostgreAccess.CallHelpFromPopUp(CallHelp);
			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();

			string buildedStringConnection = BuildConnection(this.companyId);
			if (buildedStringConnection.Length == 0)
			{
				//non riesco a connettermi all'azienda.
				CbLogins.Items.Clear();
				CbLogins.Enabled = false;
				//TO DO GESTIRE
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
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
				false,
                port
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
				for (int i = 0; i < loginsOfCompany.Count; i++)
				{
					CompanyLoginPostgre  companyLogin = (CompanyLoginPostgre)loginsOfCompany[i];
					//se è un gruppo NT, un utente NT locale o l'utente sa lo skippo
					if (companyLogin.IsSuperUser)
						continue;
					if (string.Compare(companyLogin.Login, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0 || 
						string.Compare(companyLogin.Login, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;
					CbLogins.Items.Add(new LoginItem(companyLogin.Login, false));
				}
				CbLogins.DisplayMember = "loginName";
				CbLogins.ValueMember   = "isNTUser";
				//seleziono il primo
				if (CbLogins.Items.Count > 0)
					CbLogins.SelectedIndex = 0;
			}
		}
		#endregion

		/// <summary>
		/// LoadExistLogins
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadExistLogins()
		{
			//carico le login del server
			ComboExistLogins.Items.Clear();
			//Analizzo la situazione
			LoginType typeOfLogin = TypeOfServerLogins();
			if (typeOfLogin == LoginType.Undefined) 
				return;

            PostgreAccess connSqlTransact = new PostgreAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
            connSqlTransact.OnAddUserAuthenticatedFromConsole += new PostgreAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
            connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new PostgreAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
            connSqlTransact.OnIsUserAuthenticatedFromConsole += new PostgreAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
            connSqlTransact.OnCallHelpFromPopUp += new PostgreAccess.CallHelpFromPopUp(CallHelp);
		
			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();

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
			//eventualmente eseguo l'impersonificazione
			dataToConnectionServer = connSqlTransact.LoginImpersonification
				(
				this.dbOwnerLogin,
				this.dbOwnerPassword,
				this.dbOwnerDomain,
				this.dbOwnerWinAuth,
				this.dbOwnerPrimary,
				this.dbOwnerIstance,
				false,
                port
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
				for (int i = 0; i < loginsOfCompany.Count; i++)
				{
                    CompanyLoginPostgre companyLogin = (CompanyLoginPostgre)loginsOfCompany[i];
					//se è un gruppo NT, un utente NT locale o l'utente sa lo skippo
					if (companyLogin.IsSuperUser)
						continue;
					//ho selezionato un solo utente NT 
					if (typeOfLogin == LoginType.HimSelfAndSqlLogins)
						if (/*companyLogin.IsNTUser &&*/ (companyLogin.Login != ((UserListItem)ListViewUsersCompany.CheckedItems[0]).Login)) 
							continue;
					//ho selezionato uno o più utenti SQL
                    //if ((typeOfLogin == LoginType.SqlLogin) && (companyLogin.IsNTUser)) 
                    //    continue;
					//l'utente guest lo skippo
					if (string.Compare(companyLogin.Login, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0) 
						continue;
					//l'utente EasyLookSystem lo skippo
					if (string.Compare(companyLogin.Login, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0) 
						continue;
					
					ComboExistLogins.Items.Add(new LoginItem(companyLogin.Login,false));
				}
				
				ComboExistLogins.DisplayMember = "loginName";
				ComboExistLogins.ValueMember   = "isNTUser";
				//seleziono il primo
				if (ComboExistLogins.Items.Count > 0)
					ComboExistLogins.SelectedIndex = 0;
			}
		}

		#region BuildConnection - Costruisco stringa di connessione
		/// <summary>
		/// BuildConnection
		/// </summary>
		//---------------------------------------------------------------------
		private string BuildConnection(string companyId)
		{
			string connectionToCompanyServer = string.Empty, dbOwnerId = string.Empty;
		
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
					//ora compongo la stringa
					if (this.dbOwnerWinAuth)
					{
                        
						connectionToCompanyServer = 
							string.Format(NameSolverDatabaseStrings.PostgreWinNtConnection, this.dbCompanyServer, port, this.dbCompanyName, DatabaseLayerConsts.postgreDefaultSchema);

						this.dbOwnerDomain = Path.GetDirectoryName(this.dbOwnerLogin);
					}
					else
                        /*"Server={0};Port={1};Database={2};User Id={3};Password={4};SearchPath={5};Pooling=False";*/
						connectionToCompanyServer = 
							string.Format(NameSolverDatabaseStrings.PostgreConnection, this.dbCompanyServer, port, this.dbCompanyName, this.dbOwnerLogin.ToLower(), this.dbOwnerPassword, DatabaseLayerConsts.postgreDefaultSchema);
				}
			}

			return connectionToCompanyServer;
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

		/// <summary>
		/// CbLogins_SelectedIndexChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void CbLogins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (CbLogins.SelectedItem != null)
			{
				string loginName = ((LoginItem)CbLogins.SelectedItem).LoginName;
				if (string.Compare(loginName, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0 || 
					string.Compare(loginName, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					ListViewUsersCompany.Items.Clear();
					LoadPostgreUsers(loginName);
					RbDeleteAll.Enabled			= false;
					RbModifyLogin.Enabled		= false;
					RbDisableAll.Enabled		= false;
					RbEnableAll.Enabled			= false;
					GroupsChangeLogin.Enabled	= false;
				}
				else 
					if (loginName.Length > 0)
					{
						ListViewUsersCompany.Items.Clear();
                        LoadPostgreUsers(loginName);
						//se la listView è piena, abilito i radio button
						if (ListViewUsersCompany.Items.Count > 0)
						{
							RbDeleteAll.Enabled			= true;
							RbModifyLogin.Enabled		= true;
							RbModifyLogin.Checked       = true;
							LblPassword.Enabled			= true;
							RbSelectExistLogin.Enabled	= true;
							RbCreateNewLogin.Enabled	= true;
							ComboExistLogins.Enabled	= true;
							GroupsChangeLogin.Enabled   = true;
						}
						
						RbEnableAll.Enabled	 = false;
						RbDisableAll.Enabled = false;
					}
			}
		}

		/// <summary>
		/// RbModifyLogin_CheckedChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void RbModifyLogin_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbModifyLogin.Checked)
			{
				RbSelectExistLogin.Enabled	= true;
				RbCreateNewLogin.Enabled	= true;
				RbSelectExistLogin.Checked	= true;
				RbCreateNewLogin.Checked	= false;
				ComboExistLogins.Enabled	= true;
				LblPassword.Enabled         = true;
				TbPassword.Enabled			= false;
			}
			else
			{
				RbSelectExistLogin.Enabled	= false;
				RbCreateNewLogin.Enabled	= false;
				RbSelectExistLogin.Checked	= true;
				RbCreateNewLogin.Checked	= false;
				ComboExistLogins.Enabled	= false;
				LblPassword.Enabled         = true;
				TbPassword.Enabled			= false;
			}
		}

		/// <summary>
		/// RbDeleteAll_CheckedChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void RbDeleteAll_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbDeleteAll.Checked)
			{
				RbEnableAll.Checked   = false;
				RbModifyLogin.Checked = false;
				RbDisableAll.Checked  = false;
			}
		}

		/// <summary>
		/// RbDisableAll_CheckedChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void RbDisableAll_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbDisableAll.Checked)
			{
				RbEnableAll.Checked   = false;
				RbModifyLogin.Checked = false;
				RbDeleteAll.Checked   = false;
			}
		}

		/// <summary>
		/// RbEnableAll_CheckedChanged
		/// </summary>
		//---------------------------------------------------------
		private void RbEnableAll_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbEnableAll.Checked)
			{
				RbDisableAll.Checked	= false;
				RbModifyLogin.Checked	= false;
				RbDeleteAll.Checked		= false;
			}
		}

		/// <summary>
		/// RbSelectExistLogin_CheckedChanged
		/// </summary>
		//---------------------------------------------------------
		private void RbSelectExistLogin_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbSelectExistLogin.Checked)
				RbCreateNewLogin.Checked = false;
		}

		/// <summary>
		/// RbCreateNewLogin_CheckedChanged
		/// </summary>
		//---------------------------------------------------------
		private void RbCreateNewLogin_CheckedChanged(object sender, System.EventArgs e)
		{
			if (RbCreateNewLogin.Checked)
			{
				ComboExistLogins.Enabled	= false;
				TbNewLoginName.Enabled		= true;
				TbNewLoginName.Text			= string.Empty;
				TbPassword.Enabled			= true;
				TbPassword.Text				= string.Empty;
			}
			else
			{
				ComboExistLogins.Enabled	= true;
				TbNewLoginName.Enabled		= false;
				TbPassword.Enabled			= false;
			}
		}

		/// <summary>
		/// ComboExistLogins_DropDown
		/// </summary>
		//---------------------------------------------------------
		private void ComboExistLogins_DropDown(object sender, System.EventArgs e)
		{
			if (ComboExistLogins.Items.Count == 0)
				LoadExistLogins();
		}

		/// <summary>
		/// ComboExistLogins_SelectedIndexChanged
		/// </summary>
		//---------------------------------------------------------
		private void ComboExistLogins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (ComboExistLogins.SelectedItem != null)
				TbPassword.Enabled = ! (((LoginItem)ComboExistLogins.SelectedItem).IsNTUser);
		}

		/// <summary>
		/// BtnSave_Click
		/// </summary>
		//---------------------------------------------------------
		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			if (OnSaveUsers != null)
				OnSaveUsers(sender, string.Empty, this.companyId);
		}

		/// <summary>
		/// CheckData
		/// </summary>
		//---------------------------------------------------------
		private bool CheckData()
		{
			bool result = true;
			if (ListViewUsersCompany.CheckedItems.Count == 0)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.NotSelectedCompanyUsers);
				result = false;
			}
			if (this.RbModifyLogin.Checked)
			{
				if (RbSelectExistLogin.Checked)
				{
					if (ComboExistLogins.SelectedItem == null)
					{
						diagnostic.Set(DiagnosticType.Error, Strings.NotSelectedDatabaseLogin);
						result = false;
					}
				}
				if (RbCreateNewLogin.Checked)
				{
					if (TbNewLoginName.Text.Length == 0)
					{
						diagnostic.Set(DiagnosticType.Error, Strings.EmptyDatabaseLogin);
						result = false;
					}
				}
			}
			if (!result)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}
			return result;
		}

		/// <summary>
		/// Save
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------
		public bool Save()
		{
			bool result = false;

			if (CheckData())
			{
				if (RbDeleteAll.Checked)
					result = DeleteAllLogins();
				else 
					if (RbDisableAll.Checked)
						result = DisableAllLogins();
					else 
						if (RbEnableAll.Checked)
							result = EnableAllLogins();
						else 
							if (RbModifyLogin.Checked)
								result = ModifyAllLogins();
			}

			return result;
		}

		#region DisableAllLogins
		/// <summary>
		/// DisableAllLogins
		/// </summary>
		//---------------------------------------------------------
		public bool DisableAllLogins()
		{
			bool disabledAll = true;
			CompanyUserDb companyUserDb			= new CompanyUserDb();
			companyUserDb.ConnectionString		= connectionString;
			companyUserDb.CurrentSqlConnection	= currentConnection;

			for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
			{
				UserListItem currentItemSelected = (UserListItem)ListViewUsersCompany.CheckedItems[i];
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
					result = companyUserDb.Modify(currentItemSelected);
				else
				{
					//TO DO GESTIRE
					DialogResult askIfInsert = 
						MessageBox.Show(this, Strings.AskIfInsertingUser, Strings.JoinUserCompany, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
					
					if (askIfInsert == DialogResult.OK)
						result = companyUserDb.Add(currentItemSelected);
					else
						result = true;
				}

				if (!result)
				{
					if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
						diagnostic.Set(companyUserDb.Diagnostic);
					else
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDisableLogin, currentItemSelected.Login));
					disabledAll = disabledAll & false;
				}
				else
				{
					//devo comunicare al lockManager che ho disabilitato l'utente
					if (OnUnLockAllForUser != null)
						OnUnLockAllForUser(this, currentItemSelected.Login);
					if (OnModifyTreeOfCompanies != null)
						OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
					if (OnModifyTreeOfCompanies != null)
						OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);
				}
			}

			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(this, diagnostic);
				diagnostic.Clear();
			}
			
			return disabledAll;
		}
		#endregion

		#region EnableAllLogins
		/// <summary>
		/// EnableAllLogins
		/// </summary>
		//---------------------------------------------------------
		public bool EnableAllLogins()
		{
			bool enabledAll = true;
			CompanyUserDb companyUserDb			= new CompanyUserDb();
			companyUserDb.ConnectionString		= connectionString;
			companyUserDb.CurrentSqlConnection	= currentConnection;

			for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
			{
				UserListItem currentItemSelected = (UserListItem)ListViewUsersCompany.CheckedItems[i];
				string loginId = currentItemSelected.LoginId;
				currentItemSelected.Disabled = false;
				if (companyUserDb.ExistUser(loginId, this.companyId) != 0)
				{
					if (LoginIsDisabled(currentItemSelected.LoginId))
					{
						bool result = companyUserDb.Modify(currentItemSelected);
						if (!result)
						{
							if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information || companyUserDb.Diagnostic.Warning)
								diagnostic.Set(companyUserDb.Diagnostic);
							else
								diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotEnableLogin, currentItemSelected.Login));	
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
			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);

			return enabledAll;
		}
		#endregion

		#region DeleteAllLogins
		/// <summary>
		/// DeleteAllLogins
		/// </summary>
		//---------------------------------------------------------
		public bool DeleteAllLogins()
		{
			bool deleteAll = true;
			CompanyUserDb companyUserDb			= new CompanyUserDb();
			companyUserDb.ConnectionString		= connectionString;
			companyUserDb.CurrentSqlConnection	= currentConnection;
			
			//chiedo conferma
			DialogResult askToDo = 
				MessageBox.Show(this, Strings.AskBeforeDeleteLogin, Strings.Warning, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			
			if (askToDo == DialogResult.OK)
			{
				DialogResult askDeleteLogin = 
					MessageBox.Show(this, Strings.AskBeforeDropLogin, Strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				
				for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
				{
					UserListItem currentItemSelected = (UserListItem)ListViewUsersCompany.CheckedItems[i];
					
					if (companyUserDb.ExistUser(currentItemSelected.LoginId, this.companyId) != 0)
					{
						if (askDeleteLogin == DialogResult.Yes)
						{
							bool result = companyUserDb.CompanyDbLoginRevoke(currentItemSelected.LoginId, this.companyId);
							if (!result)
							{
								if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information || companyUserDb.Diagnostic.Warning)
                                    diagnostic.Set(companyUserDb.Diagnostic);
								else
									diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDeleteLogin, currentItemSelected.Login));
							}
							else
							{
								bool canDelete = false;
								// chiedo al LoginManager l'autenticazione per procedere con la cancellazione dell'associazione Utente-Azienda
								if (OnDeleteAssociation != null)
									canDelete = OnDeleteAssociation(this, Convert.ToInt32(currentItemSelected.LoginId), Convert.ToInt32(currentItemSelected.CompanyId));

								if (!canDelete)
								{
									// se non è stata fornita un'autenticazione valida visualizzo un msg e non procedo con l'elaborazione
									diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDeleteLogin, currentItemSelected.Login) + ". " + Strings.AuthenticationTokenNotValid);
									continue;
								}

								DeleteAllUsersWithThisLogin(this.companyId, currentItemSelected.DbUser, companyUserDb);

								//informo il lockManager della cancellazione
								if (OnUnLockAllForUser != null)
									OnUnLockAllForUser(this, currentItemSelected.Login);
							}
						}
						else
						{
							bool canDelete = false;
							// chiedo al LoginManager l'autenticazione per procedere con la cancellazione dell'associazione Utente-Azienda
							if (OnDeleteAssociation != null)
								canDelete = OnDeleteAssociation(this, Convert.ToInt32(currentItemSelected.LoginId), Convert.ToInt32(currentItemSelected.CompanyId));

							if (!canDelete)
							{
								// se non è stata fornita un'autenticazione valida visualizzo un msg e non procedo con l'elaborazione
								diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDeleteCompanyUser, currentItemSelected.Login) + ". " + Strings.AuthenticationTokenNotValid);
								continue;
							}

							bool result = companyUserDb.Delete(currentItemSelected.LoginId, this.companyId);
							if (!result)
							{
								if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
									diagnostic.Set(companyUserDb.Diagnostic);
								else
									diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDeleteLogin, currentItemSelected.Login));
							}
							else
							{
								//informo il lockManager della cancellazione
								if (OnUnLockAllForUser != null)
									OnUnLockAllForUser(this, currentItemSelected.Login);
							}
						}
					}
				}

				if (diagnostic.Error || diagnostic.Information || diagnostic.Warning)
				{
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
						OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}

				if (OnModifyTree != null) 
					OnModifyTree(this, ConstString.containerCompanies);
			}

			return deleteAll;
		}
		#endregion

		#region DeleteAllUsersWithThisLogin
		/// <summary>
		/// DeleteAllUsersWithThisLogin
		/// </summary>
		//---------------------------------------------------------------------
		private bool DeleteAllUsersWithThisLogin(string companyId, string loginName, CompanyUserDb companyUserDb)
		{
			bool deleteAll = true;

			ArrayList companiesOnSameServer = new ArrayList();
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;
			
			bool result = companyDb.SelectCompaniesSameServer(out companiesOnSameServer, this.dbCompanyServer);
			if (!result)
			{
				diagnostic.Set(companyDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				companiesOnSameServer.Clear();
				deleteAll = false;
			}
			
			//per ogni azienda il cui db risiede nel  medesimo server
			for (int i = 0; i < companiesOnSameServer.Count; i++)
			{
				CompanyItem companyItem = (CompanyItem)companiesOnSameServer[i];
				ArrayList usersOfCompany = new ArrayList();
				result = companyUserDb.SelectAll(out usersOfCompany, companyItem.CompanyId);
				if (!result)
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
					if (string.Compare(usersOfCompanyItems.DBDefaultUser, loginName, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						result = companyUserDb.Delete(usersOfCompanyItems.LoginId, companyItem.CompanyId);
						if (!result)
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
			return deleteAll;
		}
		#endregion

		#region ModifyAllLogins
		/// <summary>
		/// ModifyAllLogins
		/// </summary>
		//---------------------------------------------------------
		public bool ModifyAllLogins()
		{
			bool modifyAll = true, isNTUser = false;
			string loginName = string.Empty, loginPassword = string.Empty;
			
			CompanyUserDb companyUserDb			= new CompanyUserDb();
			companyUserDb.ConnectionString		= connectionString;
			companyUserDb.CurrentSqlConnection	= currentConnection;

			//Costruisco la stringa di connessione all'azienda selezionata (eventualmente
			//impersonificando il dbowner dell'azienda in questione
			PostgreAccess connSqlTransact = new PostgreAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
            connSqlTransact.OnAddUserAuthenticatedFromConsole += new PostgreAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
            connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new PostgreAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
            connSqlTransact.OnIsUserAuthenticatedFromConsole += new PostgreAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
            connSqlTransact.OnCallHelpFromPopUp += new PostgreAccess.CallHelpFromPopUp(CallHelp);
		
			UserImpersonatedData dataToConnectionServer	= new UserImpersonatedData();

			string buildedStringConnection = BuildConnection(this.companyId);
			if (buildedStringConnection.Length == 0)
			{
				//TO DO GESTIRE
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				return false;
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
					false,
                    port
				);
			if (dataToConnectionServer == null) 
			{
				//TO DO GESTIRE
				Cursor.Current = Cursors.Default;
				return false;
			}
			//sto utilizzando una login esistente..verifico la connessione prima di procedere
			if (RbSelectExistLogin.Checked)
			{
				isNTUser		= ((LoginItem)ComboExistLogins.SelectedItem).IsNTUser;
				loginName		= ((LoginItem)ComboExistLogins.SelectedItem).LoginName;
				loginPassword	= string.Empty;
				if (!isNTUser)
					loginPassword = TbPassword.Text;

				bool resultGrant = false;
				if (!connSqlTransact.ExistLogin(loginName))
				{
					resultGrant = GrantLoginToCompany(isNTUser, loginName, loginPassword, connSqlTransact) &&
						          TryToConnect(isNTUser, loginName, loginPassword);
				}
				else
					resultGrant = TryToConnect(isNTUser, loginName, loginPassword);

				if (!resultGrant)
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotChangePwdOfLogin, loginName));
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					return false;
				}
			}
			else
			{
				loginName		= TbNewLoginName.Text;
				loginPassword	= TbPassword.Text;
				isNTUser		= false;
				if (connSqlTransact.ExistLogin(loginName))
				{
					diagnostic.Set(DiagnosticType.Warning, string.Format(DatabaseItemsStrings.LoginAlreadyExist, loginName));
					diagnostic.Set(connSqlTransact.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					return false;
				}

				if (!AddLoginToCompany(isNTUser, loginName, loginPassword, connSqlTransact))
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotAddLogin, loginName));
					diagnostic.Set(connSqlTransact.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					return false;
				}
			}
			//eseguo la modifica x ogni utente checkinato
			for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
			{
				UserListItem currentItemSelected	= (UserListItem)ListViewUsersCompany.CheckedItems[i];
				string loginId						= currentItemSelected.LoginId;
				currentItemSelected.DbUser			= loginName;
				currentItemSelected.DbPassword		= loginPassword;
				currentItemSelected.DbWindowsAuthentication = isNTUser;
				
				if (companyUserDb.ExistUser(loginId, this.companyId) != 0)
				{
					if (!companyUserDb.Modify(currentItemSelected))
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
				else
					modifyAll = false;
			}
			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);

			return modifyAll;
		}
		#endregion

		#region Operazioni su Login SQL 

		#region GrantLoginToCompany 
		/// <summary>
		/// GrantLoginToCompany
		/// </summary>
		//---------------------------------------------------------------------
		private bool GrantLoginToCompany(bool isLoginNT, string loginName, string loginPassword, PostgreAccess connSqlTransact)
		{
            //if (isLoginNT)
            //{
            //    return (connSqlTransact.GrantDbAccess(loginName, loginName, this.dbCompanyName, connSqlTransact.CurrentConnection) &&
            //            connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName, connSqlTransact.CurrentConnection) &&
            //            connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName, connSqlTransact.CurrentConnection) &&
            //            connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName, connSqlTransact.CurrentConnection));
            //}
            //else
            //{
				return (connSqlTransact.GrantDbAccess(loginName) );
			//}
		}
		#endregion

		#region TryToConnect 
		/// <summary>
		/// TryToConnect
		/// </summary>
		//---------------------------------------------------------------------
		private bool TryToConnect(bool isLoginNT, string loginName, string loginPassword)
		{
            /*"Server={0};Port={1} ;Database={2};SearchPath={3};Integrated Security=true;Pooling=False"*/
            /*"Server={0};Port={1};Database={2};User Id={3};Password={4};SearchPath={5};Pooling=False"*/
			PostgreAccess tentativeConnSql = new PostgreAccess();
			tentativeConnSql.CurrentStringConnection =
				(isLoginNT)
				? string.Format(NameSolverDatabaseStrings.PostgreWinNtConnection, this.dbCompanyServer, port, this.dbCompanyName, DatabaseLayerConsts.postgreDefaultSchema)
				: string.Format(NameSolverDatabaseStrings.PostgreConnection, this.dbCompanyServer, port, this.dbCompanyName, loginName.ToLower(), loginPassword, DatabaseLayerConsts.postgreDefaultSchema);
			
			return tentativeConnSql.TryToConnect();
		}
		#endregion

		#region AddLoginToCompany
		/// <summary>
		/// AddLoginToCompany
		/// </summary>
		//---------------------------------------------------------------------
		private bool AddLoginToCompany(bool isLoginNT, string loginName, string loginPassword, PostgreAccess connSqlTransact)
		{
            //if (isLoginNT)
            //{
            //    return( connSqlTransact.SPGrantLogin(loginName, connSqlTransact.CurrentConnection) && 
            //            connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName, connSqlTransact.CurrentConnection) &&
            //            connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName, connSqlTransact.CurrentConnection) &&
            //            connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName, connSqlTransact.CurrentConnection) &&
            //            connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName, connSqlTransact.CurrentConnection));
            //}
            //else
            //{
				return (connSqlTransact.AddLogin(loginName, loginPassword) &&
						connSqlTransact.GrantDbAccess(loginName));
            //}
		}
		#endregion

		/// <summary>
		/// ListViewUsersCompany_ItemCheck
		/// Ogni volta che seleziono un utente nella lista degli utenti associati, pulisco la combo delle logins
		/// </summary>
		//---------------------------------------------------------------------
		private void ListViewUsersCompany_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			//a ogni selezione pulisco la combo
			if (ComboExistLogins.Items.Count > 0)
				ComboExistLogins.Items.Clear();

			//a seconda delle selezioni posso o meno abilitare i radiobutton Abilita e disabilita
			if (this.ListViewUsersCompany.CheckedItems.Count == 0 && e.NewValue == CheckState.Checked)
			{
				//sto per selezionare un elemento
				//se la login è disabilitata non posso fare niente
				UserListItem nextSelection = (UserListItem)ListViewUsersCompany.Items[e.Index];
				if (LoginIsDisabled(nextSelection.LoginId))
				{
					RbDisableAll.Enabled = false;
					RbEnableAll.Enabled = false;
				}
				else
				{
					//se l'utente è abilitato metto RbEnableAll = false
					RbEnableAll.Enabled = nextSelection.Disabled;
					RbDisableAll.Enabled = nextSelection.Disabled;
				}
			}
			else 
			{
				if (this.ListViewUsersCompany.CheckedItems.Count == 1 && e.NewValue == CheckState.Unchecked)
				{
					//deseleziono tutto
					RbDisableAll.Enabled = false;
					RbEnableAll.Enabled  = false;
				}
				else 
				{
					bool canEnable = false;
					bool canDisable = false;
					for (int i = 0; i < this.ListViewUsersCompany.CheckedItems.Count; i++)
					{
						if (e.NewValue == CheckState.Unchecked)
						{
							if (i == e.Index) continue;
						}
						UserListItem currentSelection = (UserListItem)ListViewUsersCompany.Items[i];
						if (LoginIsDisabled(currentSelection.LoginId))
							canEnable = true;
						else
							canDisable = currentSelection.Disabled;
					}

					if (e.NewValue == CheckState.Checked)
					{
						UserListItem nextSelection = (UserListItem)ListViewUsersCompany.Items[e.Index];
						if (LoginIsDisabled(nextSelection.LoginId))
							canEnable = true;
						else
							canDisable = nextSelection.Disabled;
					}

					this.RbEnableAll.Enabled = canEnable;
					this.RbDisableAll.Enabled = canDisable;
				}
			}

			if (!RbDeleteAll.Enabled && !RbModifyLogin.Enabled)
			{
				RbEnableAll.Checked = (RbEnableAll.Enabled && RbDisableAll.Enabled) || (RbEnableAll.Enabled);
				RbDisableAll.Checked = !(RbEnableAll.Enabled);
			}
		}
		#endregion

		/// <summary>
		/// ModifyCompanyUsersToLogin_Closing
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void ModifyCompanyUsersToLogin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// ModifyCompanyUsersToLogin_Deactivate
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void ModifyCompanyUsersToLogin_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// ModifyCompanyUsersToLogin_VisibleChanged
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void ModifyCompanyUsersToLogin_VisibleChanged(object sender, System.EventArgs e)
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
	}
}