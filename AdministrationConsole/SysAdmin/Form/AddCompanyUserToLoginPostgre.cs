using System;
using System.Collections;
using System.Data.SqlClient;
using Npgsql;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.PostgreDataAccess;
using Microarea.TaskBuilderNet.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// AddCompanyUserToLogin
	/// Form di associazione utenti applicativi all'azienda creando una login 1 a 1
	/// Le login applicative in MSD_Logins sono trasformate in login SQL e censite
	/// nella MSD_CompanyLogins (oppure MSD_SlaveLogins)
	/// </summary>
	//=========================================================================
	public partial class AddCompanyUserToLoginPostgre : PlugInsForm
	{
		#region Events and Delegates
		public delegate void ModifyTreeOfCompanies(object sender, string nodeType,string companyId);
		public event ModifyTreeOfCompanies OnModifyTreeOfCompanies;
		
		public delegate void SaveUsers(object sender, string id, string companyId);
		public event SaveUsers OnSaveUsers;
		
		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;
		
		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;
		
		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;
		
		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;
		
		public delegate void CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event CallHelpFromPopUp OnCallHelpFromPopUp;

		public delegate bool IsActivated(string application, string functionality);
		//public event IsActivated OnIsActivated;
		#endregion

		#region Private Variables
		private Diagnostic diagnostic = new Diagnostic("SysAdmin.AddCompanyUserToLoginPostgre");

		private DBMSType companyProvider;
		private SqlConnection sysDbConnection;
		private string sysDbConnString	= string.Empty;
		private string companyId		= string.Empty;

		// variabili di appoggio per il database aziendale
		private PostgreAccess companyConnPostgre = new PostgreAccess();
		private UserImpersonatedData companyImpersonated = new UserImpersonatedData();

		private string dbOwnerLogin		= string.Empty;
		private string dbOwnerPassword  = string.Empty;
		private bool   dbOwnerWinAuth   = false;
		private string dbOwnerDomain    = string.Empty;
		private string dbOwnerPrimary   = string.Empty;
		private string dbOwnerInstance  = string.Empty;
		private string dbCompanyName    = string.Empty;
		private string dbCompanyServer  = string.Empty;
        private int port = 0;
		private bool companyUseSlave = false;
		//

		// variabili di appoggio per il database documentale
        //@@Anastasia non gestisco temporaneamente
        //private bool isDMSActivated = false;

        //private  dmsConnSqlTransact = new TransactSQLAccess();
		//private UserImpersonatedData dmsImpersonated = new UserImpersonatedData();

		private string dmsDbOwnerLogin = string.Empty;
		private string dmsDbOwnerPassword = string.Empty;
		//private bool dmsDbOwnerWinAuth = false;
		private string dmsDbOwnerDomain = string.Empty;
		private string dmsDbOwnerPrimary = string.Empty;
		private string dmsDbOwnerInstance = string.Empty;
		private string dmsDatabaseName = string.Empty;
		private string dmsServerName = string.Empty;
		//
		#endregion

		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
        public AddCompanyUserToLoginPostgre
			(
				string			connectionString, 
				SqlConnection	currentConnection,
				string			companyId,
			    DBMSType		companyProvider,
				BrandLoader		aBrandLoader
			)
		{
			InitializeComponent();
			this.companyId			= companyId;
			this.sysDbConnection	= currentConnection;
			this.sysDbConnString	= connectionString;
			this.companyProvider    = companyProvider;
			State					= StateEnums.View;

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

			if (aBrandLoader != null)
			{
				string brandedCompany = aBrandLoader.GetCompanyName();
				if (brandedCompany != null && brandedCompany.Length > 0)
					LblExplication.Text = LblExplication.Text.Replace(NameSolverStrings.Microarea, brandedCompany);
			}
		}

		#region BuildListView - Costruisco il layout della griglia
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



        #region LoadPostgreUsers - Popolamento della lista nel caso di azienda Postgre
      
        //----------------------------------------------------------------------
        private void LoadPostgreUsers()
        {
            UserDb userDb = new UserDb();
            userDb.ConnectionString = sysDbConnString;
            userDb.CurrentSqlConnection = sysDbConnection;

            ArrayList users = new ArrayList();

            //seleziono tutti gli utenti applicativi disponibili 
            if (!userDb.SelectAllUsers(out users, false))
                SendToCallerDiagnostic(userDb.Diagnostic);

            CompanyUserDb companyUserDb = new CompanyUserDb();
            companyUserDb.ConnectionString = sysDbConnString;
            companyUserDb.CurrentSqlConnection = sysDbConnection;

            for (int i = 0; i < users.Count; i++)
            {
                UserItem itemUser = (UserItem)users[i];

                if (!companyUserDb.ExistUserInPostgreCompanys(itemUser.LoginId))
                    continue;

                UserListItem listItemUser = new UserListItem();
                listItemUser.IsModified = false;
                listItemUser.CompanyId = companyId;
                listItemUser.LoginId = itemUser.LoginId;
                listItemUser.Login = itemUser.Login;
                listItemUser.Description = itemUser.Description.Replace("\r\n", " ");
                listItemUser.DbPassword = itemUser.Password;
                listItemUser.DbWindowsAuthentication = itemUser.WindowsAuthentication;
                listItemUser.ImageIndex = (itemUser.WindowsAuthentication)
                    ? PlugInTreeNode.GetLoginsDefaultImageIndex : PlugInTreeNode.GetUserDefaultImageIndex;

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
                LblAllUsersJoined.Visible = true;
                BtnSave.Enabled = false;
                BtnSelectAll.Enabled = false;
                BtnUnselectAll.Enabled = false;
            }
            State = StateEnums.View;
        }
        #endregion


        
		#region LoadOnlyOracleFreeUsers - Popolamento della lista nel caso di azienda Oracle
		/// <summary>
		/// LoadOnlyOracleFreeUsers
		/// Nel caso di azienda Oracle, la lista degli utenti disponibili all'associazione
		/// deve provedere solamente:
		/// - utente non dbowner
		/// - utente non già associato a qualche altra azienda Oracle (vincolo dato da Oracle stesso)
		/// </summary>
		//----------------------------------------------------------------------
		private void LoadOnlyOracleFreeUsers()
		{
			UserDb userDb = new UserDb();
			userDb.ConnectionString		= sysDbConnString;
			userDb.CurrentSqlConnection = sysDbConnection;
		
			ArrayList users = new ArrayList();
			
			//seleziono tutti gli utenti applicativi disponibili 
			if (!userDb.SelectAllUsers(out users, false))
				SendToCallerDiagnostic(userDb.Diagnostic);
			
			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = sysDbConnString;
			companyUserDb.CurrentSqlConnection = sysDbConnection;
			
			for (int i = 0; i < users.Count; i++)
			{
				UserItem itemUser = (UserItem)users[i];
			
				if (!companyUserDb.ExistUserInOracleCompanys(itemUser.LoginId))
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
                    ? PlugInTreeNode.GetLoginsDefaultImageIndex : PlugInTreeNode.GetUserDefaultImageIndex;
				
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
				LblAllUsersJoined.Visible   = true;
				BtnSave.Enabled				= false;
				BtnSelectAll.Enabled        = false;
				BtnUnselectAll.Enabled      = false;
			}
			State = StateEnums.View;
		}
		#endregion

		#region LoadAllMicroareaUsers - Carico gli utenti applicativi
		/// <summary>
		/// LoadAllMicroareaUsers
		/// Carico tutti gli Utenti Applicativi per i quali NON esiste una Login in SQL pari al loro LogiName 
		/// (non è necessario controllare la password poichè non possono esistere due login, anche con 
		/// pwd diverse, nello stesso server SQL)
		/// </summary>
		//----------------------------------------------------------------------
		private void LoadAllMicroareaUsers()
		{
			UserDb userDb = new UserDb();
			userDb.ConnectionString		= sysDbConnString;
			userDb.CurrentSqlConnection = sysDbConnection;
			ArrayList users	= new ArrayList();
			
			if (!userDb.SelectAllUsersExceptSa(out users, false))
				SendToCallerDiagnostic(userDb.Diagnostic);

			for (int i = 0; i < users.Count; i++)
			{
				UserItem itemUser = (UserItem)users[i];

				//se esiste già la login (in MDS_CompanyLogins DBUser = itemUser.Login) non lo mostro
				CompanyUserDb companyUserDb = new CompanyUserDb();
				companyUserDb.ConnectionString = sysDbConnString;
				companyUserDb.CurrentSqlConnection = sysDbConnection;
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

					if  (!(disabled && string.IsNullOrEmpty(dbUser) && string.IsNullOrEmpty(dbPwd)))
						continue;
				}
				
				UserListItem listItemUser	= new UserListItem();
				listItemUser.IsModified		= false;
				listItemUser.CompanyId		= companyId;
				listItemUser.LoginId		= itemUser.LoginId;
				listItemUser.Login			= itemUser.Login;
				listItemUser.Description	= itemUser.Description.Replace("\r\n", " ");
				listItemUser.DbPassword     = itemUser.Password;
				listItemUser.DbWindowsAuthentication = itemUser.WindowsAuthentication;
				listItemUser.ImageIndex = (itemUser.WindowsAuthentication)
                    ? PlugInTreeNode.GetLoginsDefaultImageIndex : PlugInTreeNode.GetUserDefaultImageIndex;
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
				LblAllUsersJoined.Visible   = true;
				BtnSave.Enabled				= false;
				BtnSelectAll.Enabled        = false;
				BtnUnselectAll.Enabled      = false;
			}

			State = StateEnums.View;
		}
		#endregion

		#region SelectAll and UnselectAll
		/// <summary>
		/// BtnSelectAll_Click
		/// </summary>
		//----------------------------------------------------------------------
		private void BtnSelectAll_Click(object sender, System.EventArgs e)
		{
			for (int i=0; i < ListViewUsersCompany.Items.Count; i++)
			{
				if  (!ListViewUsersCompany.Items[i].Checked	)
					ListViewUsersCompany.Items[i].Checked = true;
			}
			BtnSelectAll.Enabled = false;
			BtnUnselectAll.Enabled = true;
		}

		/// <summary>
		/// BtnUnselectAll_Click
		/// </summary>
		//----------------------------------------------------------------------
		private void BtnUnselectAll_Click(object sender, System.EventArgs e)
		{
			for (int i=0; i < ListViewUsersCompany.Items.Count; i++)
			{
				if  (ListViewUsersCompany.Items[i].Checked	)
					ListViewUsersCompany.Items[i].Checked = false;
			}
			BtnSelectAll.Enabled = true;
			BtnUnselectAll.Enabled = false;
		}
		#endregion

		#region Events on form controls
		/// <summary>
		/// ListViewUsersCompany_ItemCheck - Gestione dei bottoni Seleziona Tutti /Deseleziona Tutti
		/// </summary>
		//----------------------------------------------------------------------
		private void ListViewUsersCompany_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			//Di default entrambi i bottoni sono abilitati
			BtnSelectAll.Enabled	= true;
			BtnUnselectAll.Enabled	= true;
			//tutti gli oggetti sono selezionati? Disabilito il bottone di SelectAll
			if (ListViewUsersCompany.CheckedItems.Count <= ListViewUsersCompany.Items.Count) 
			{ 
				if (e.CurrentValue == CheckState.Unchecked && e.NewValue != CheckState.Checked)
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
		/// BtnSave_Click
		/// </summary>
		//----------------------------------------------------------------------
		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			if (OnSaveUsers != null)
				OnSaveUsers(sender, string.Empty, this.companyId);
		}

		/// <summary>
		/// AddCompanyUserToLogin_Load
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyUserToLogin_Load(object sender, System.EventArgs e)
		{
			BuildListView();
			
			switch (companyProvider)
			{ 
				case DBMSType.SQLSERVER:
					LoadAllMicroareaUsers();
					break;

				case DBMSType.ORACLE:
					LoadOnlyOracleFreeUsers();
					break;

                case DBMSType.POSTGRE:
                    LoadPostgreUsers();
                    break;
			}

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

            //@@anastasia non gestisco DMs
			// l'evento posso spararlo solo nella Load, perche' nel costruttore non e' ancora stato 
			// agganciato e valorizzato!
            //if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
            //    isDMSActivated = true;
		}

		/// <summary>
		/// AddCompanyUserToLogin_Closing
		/// Invio la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyUserToLogin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// AddCompanyUserToLogin_Deactivate
		/// Invio la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyUserToLogin_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// AddCompanyUserToLogin_VisibleChanged
		/// Invio la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyUserToLogin_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
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
			companyConnPostgre.NameSpace = "Module.MicroareaConsole.SysAdmin";
            companyConnPostgre.OnAddUserAuthenticatedFromConsole += new PostgreAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
            companyConnPostgre.OnGetUserAuthenticatedPwdFromConsole += new PostgreAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
            companyConnPostgre.OnIsUserAuthenticatedFromConsole += new PostgreAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
            companyConnPostgre.OnCallHelpFromPopUp += new PostgreAccess.CallHelpFromPopUp(CallHelp);

			// compongo la stringa di connessione per l'azienda
			string companyConnectionString = CreateCompanyConnectionString();
			if (string.IsNullOrEmpty(companyConnectionString))
			{
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				State = StateEnums.Editing;
				return isValidConnection;
			}

			companyConnPostgre.CurrentStringConnection = companyConnectionString;
			// eventualmente eseguo l'impersonificazione
			companyImpersonated = companyConnPostgre.LoginImpersonification
				(
				this.dbOwnerLogin,
				this.dbOwnerPassword,
				this.dbOwnerDomain,
				this.dbOwnerWinAuth,
				this.dbOwnerPrimary,
				this.dbOwnerInstance,
				false,
                port
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

                    //@@Anastasia non gestico WinNtAuthentication
					//ora compongo la stringa di connessione
                    //if (this.dbOwnerWinAuth)
                    //{
                    //    connectionToCompanyServer = string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, this.dbCompanyServer, this.dbCompanyName);
                    //    this.dbOwnerDomain = Path.GetDirectoryName(this.dbOwnerLogin);
                    //}
                    //else

                    if (string.IsNullOrEmpty(dbOwnerPassword) || string.IsNullOrWhiteSpace(dbOwnerPassword))
                        dbOwnerPassword = DatabaseLayerConsts.postgreDefaultPassword;

					connectionToCompanyServer = string.Format(NameSolverDatabaseStrings.PostgreConnection, this.dbCompanyServer, port, this.dbCompanyName, this.dbOwnerLogin.ToLower(), this.dbOwnerPassword,DatabaseLayerConsts.postgreDefaultSchema);
				}
			}
			return connectionToCompanyServer;
		}

		///<summary>
        /// @@Anastasia non gestisco DMS
		/// ConnectToDmsDatabase
		/// Metodo che si occupa di effettuare una connessione al database documentale
		/// con le credenziali di amministrazione.
		///</summary>
		//----------------------------------------------------------------------
        //private bool ConnectToDmsDatabase()
        //{
        //    bool isValidConnection = false;

        //    // CONNESSIONE AL DATABASE DOCUMENTALE
        //    dmsConnSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
        //    dmsConnSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
        //    dmsConnSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
        //    dmsConnSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
        //    dmsConnSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

        //    // se l'azienda gestisce il database documentale, devo caricare anche le sue informazioni
        //    if (companyUseSlave)
        //    {
        //        // compongo la stringa di connessione per il database documentale
        //        string dmsConnectionString = CreateDmsConnectionString();
        //        if (string.IsNullOrEmpty(dmsConnectionString))
        //        {
        //            diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
        //            DiagnosticViewer.ShowDiagnostic(diagnostic);
        //            State = StateEnums.Editing;
        //            return isValidConnection;
        //        }

        //        dmsConnSqlTransact.CurrentStringConnection = dmsConnectionString;
        //        // eventualmente eseguo l'impersonificazione
        //        dmsImpersonated = dmsConnSqlTransact.LoginImpersonification
        //            (
        //            dmsDbOwnerLogin,
        //            dmsDbOwnerPassword,
        //            dmsDbOwnerDomain,
        //            dmsDbOwnerWinAuth,
        //            dmsDbOwnerPrimary,
        //            dmsDbOwnerInstance,
        //            false
        //            );

        //        if (dmsImpersonated == null)
        //        {
        //            Cursor.Current = Cursors.Default;
        //            State = StateEnums.Editing;
        //            isValidConnection = false;
        //            return isValidConnection;
        //        }
        //        else
        //            isValidConnection = true;
        //    }

        //    return isValidConnection;
        //}

		///<summary>
        ///@@Anastasia non gestisco DMS
		/// CreateDmsConnectionString
		/// Accedo al database di sistema e leggo tutte le informazioni per comporre la stringa 
		/// di connessione al database documentale
		///</summary>
		//----------------------------------------------------------------------
        //private string CreateDmsConnectionString()
        //{
        //    string dmsConnectionString = string.Empty;

        //    // devo verificare se c'e' uno slave associato all'azienda
        //    CompanyDBSlave dbSlave = new CompanyDBSlave();
        //    dbSlave.CurrentSqlConnection = this.sysDbConnection;
        //    dbSlave.ConnectionString = this.sysDbConnString;
        //    CompanyDBSlaveItem slaveItem;
        //    dbSlave.SelectSlaveForCompanyId(this.companyId, out slaveItem);

        //    if (slaveItem == null)
        //        return dmsConnectionString;

        //    dmsDatabaseName = slaveItem.DatabaseName;
        //    dmsServerName = slaveItem.ServerName;
        //    dmsDbOwnerPrimary = slaveItem.ServerName.Split(Path.DirectorySeparatorChar)[0];
        //    dmsDbOwnerInstance = slaveItem.ServerName.Split(Path.DirectorySeparatorChar).Length > 1
        //                        ? slaveItem.ServerName.Split(Path.DirectorySeparatorChar)[1]
        //                        : string.Empty;

        //    // carico le info di connessione per l'utente dbowner del dms
        //    SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
        //    slaveLoginDb.CurrentSqlConnection = this.sysDbConnection;
        //    slaveLoginDb.ConnectionString = this.sysDbConnString;
        //    SlaveLoginItem loginItem;
        //    slaveLoginDb.SelectAllForSlaveAndLogin(slaveItem.SlaveId, slaveItem.SlaveDBOwner, out loginItem);

        //    if (loginItem == null)
        //        return dmsConnectionString;

        //    dmsDbOwnerLogin = loginItem.SlaveDBUser;
        //    dmsDbOwnerPassword = loginItem.SlaveDBPassword;
        //    dmsDbOwnerWinAuth = loginItem.SlaveDBWinAuth;

        //    //ora compongo la stringa di connessione
        //    if (dmsDbOwnerWinAuth)
        //    {
        //        dmsConnectionString = string.Format(NameSolverDatabaseStrings.SQLWinNtConnection,dmsServerName, dmsDatabaseName);
        //        this.dmsDbOwnerDomain = Path.GetDirectoryName(dmsDbOwnerLogin);
        //    }
        //    else
        //        dmsConnectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, dmsServerName, dmsDatabaseName, dmsDbOwnerLogin, dmsDbOwnerPassword);
			
        //    return dmsConnectionString;
        //}



		///<summary>
		/// GrantLogin
		/// Metodo richiamato per ogni utente selezionato e che aggiunge/granta la login sul server postgre
		///</summary>
		//----------------------------------------------------------------------
		private bool GrantLogin(UserListItem userItem, bool isCompanyDb)
		{
			bool result = false;

			string loginId = userItem.LoginId;
			string loginName = userItem.Login;
			string loginPassword = userItem.DbPassword;
			bool isWindowsAuth = userItem.DbWindowsAuthentication;

			string dbName = isCompanyDb ? this.dbCompanyName : this.dmsDatabaseName;

			PostgreAccess connPostgre = /*isCompanyDb ?*/ companyConnPostgre /*: dmsConnSqlTransact*/;

				//se la login non esiste la devo prima creare
                if (!connPostgre.ExistLogin(loginName))
				{
                    //@@Anastasia non gestisco WinAuthentication
                    //if (isWindowsAuth)
                    //{
                    //    result =
                    //        connPostgre.SPGrantLogin(loginName, connPostgre.CurrentConnection) &&
                    //        connPostgre.SPGrantDbAccess(loginName, loginName, dbName, connPostgre.CurrentConnection) &&
                    //        connPostgre.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, dbName, connPostgre.CurrentConnection) &&
                    //        connPostgre.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, dbName, connPostgre.CurrentConnection) &&
                    //        connPostgre.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, dbName, connPostgre.CurrentConnection);
                    //}
                    //else
                    //{
                   
                       
					//}
					//se ci sono errori interrompo

					if (! connPostgre.AddLogin(loginName, loginPassword) )
					{
                        diagnostic.Set(connPostgre.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						State = StateEnums.Editing;
						Cursor.Current = Cursors.Default;
					}

                }
          
				if (!connPostgre.GrantDbAccess(loginName))
				{
                    diagnostic.Set(connPostgre.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					State = StateEnums.Editing;
					Cursor.Current = Cursors.Default;
                    
				}
                else 
                    result = true;

 
			userItem.DbUser = loginName;

			if (string.Compare(loginName, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
				userItem.IsAdmin = true;

			userItem.Disabled = false;

			// assegno la password alla login sulla base dei dati gia' memorizzati nel database di sistema
			// verificare se ha senso farlo per gli utenti in win auth??? (secondo me no, visto che sono senza pw)
            userItem.DbPassword =AssignLoginPasswordForCompany(loginName, loginPassword);
            //isCompanyDb 
                                //? 
            
                                //: AssignLoginPasswordForDms(loginName, loginPassword);

			return result;
		}

		# region Save

		//----------------------------------------------------------------------
		public void Save(object sender, System.EventArgs e)
		{
			bool result = false;

			//bool dmsToManage = false;

			// mi connetto al database aziendale, con le credenziali del dbowner
			result = ConnectToCompanyDatabase();

			// se non sono riuscita a connettermi al database aziendale non procedo
			if (!result)
				return; // messaggio di errore

			// se il modulo dms e' attivato e l'azienda ha uno slave associato procedo con i controlli sul database

            //@@Anastasia non gestisco DMS per adesso
            //if (isDMSActivated && companyUseSlave)
            //{
            //    result = ConnectToDmsDatabase();

            //    // se non sono riuscita a connettermi al database documentale non procedo
            //    if (!result)
            //        return; // messaggio di errore
				
            //    dmsToManage = true;
            //}

			UserListItem companyUserItem = null;

			// per tutti gli utenti selezionati nella griglia richiamo le operazioni di associazione delle login
			for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
			{
				UserListItem selectedItem = (UserListItem)ListViewUsersCompany.CheckedItems[i];

                if (string.IsNullOrEmpty(selectedItem.DbPassword) || string.IsNullOrWhiteSpace(selectedItem.DbPassword))
                {
                    diagnostic.Set(DiagnosticType.Information, Strings.PostgrePasswordIsEmpty);
                    DiagnosticViewer.ShowDiagnostic(diagnostic);
                    State = StateEnums.Editing;
                    return ;
                }

				// eseguo il grant della login sul server del database aziendale
                if (GrantLogin(selectedItem, true))
                {
                    companyUserItem = (UserListItem)selectedItem.Clone();

                    //@@Anastasia Non gestisco DMs
                    //// se gestisco il dms e non sono sullo stesso server effettuo gli altri grant
                    //if (dmsToManage)
                    //    result = GrantLogin(selectedItem, false);
                }
                else return;
				

				// se tutto e' andato a buon fine vado ad aggiornare le tabelle del database di sistema con le login
				if (result)
				{
					CompanyUserDb companyUserDb = new CompanyUserDb();
					companyUserDb.ConnectionString = this.sysDbConnString;
					companyUserDb.CurrentSqlConnection = this.sysDbConnection;

					//aggiungo o modifico in MSD_CompanyLogins la riga relativa alla login
					result = (companyUserDb.ExistUser(companyUserItem.LoginId, this.companyId) == 0)
						? companyUserDb.Add(companyUserItem)
						: companyUserDb.Modify(companyUserItem);

                    //@@Anastasia non gestisco dms
					// se l'aggiornamento delle logins della company e' andato a buon fine
					// procedo con quello del dms
                    //if (result && dmsToManage)
                    //{
                    //    // leggo il record associato alla company nella tabella MSD_CompanyDBSlaves, per avere lo slaveId
                    //    CompanyDBSlave companyDBSlave = new CompanyDBSlave();
                    //    companyDBSlave.ConnectionString = this.sysDbConnString;
                    //    companyDBSlave.CurrentSqlConnection = this.sysDbConnection;

                    //    CompanyDBSlaveItem dbSlaveItem;
                    //    if (companyDBSlave.SelectSlaveForCompanyIdAndSignature(this.companyId, DatabaseLayerConsts.DMSSignature, out dbSlaveItem))
                    //    {
                    //        SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
                    //        slaveLoginDb.ConnectionString = this.sysDbConnString;
                    //        slaveLoginDb.CurrentSqlConnection = this.sysDbConnection;

                    //        if (!slaveLoginDb.ExistLoginForSlaveId(selectedItem.LoginId, dbSlaveItem.SlaveId))
                    //            slaveLoginDb.Add(selectedItem, dbSlaveItem.SlaveId);
                    //        else
                    //            slaveLoginDb.Modify(selectedItem, dbSlaveItem.SlaveId);
                    //    }
                    //}

				}
			}

			if (companyImpersonated != null)
				companyImpersonated.Undo();
            //if (dmsImpersonated != null)
            //    dmsImpersonated.Undo();

			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies(sender, ConstString.containerCompanyUsers, this.companyId);
			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies(sender, ConstString.containerCompanyRoles, this.companyId);
		}

		
		# endregion



		# region AssignLoginPasswordForCompany
		///<summary>
		/// Per assegnare la corretta password nella MSD_CompanyLogins:
		/// - carico tutte le aziende che hanno il database sul medesimo server
		/// - per ognuna di queste carico gli utenti ad esse associate
		/// - se trovo l'utente allora leggo la password, altrimenti utilizzo quella dell'utente applicativo
		///</summary>
		//---------------------------------------------------------------------
		private string AssignLoginPasswordForCompany(string loginName, string loginPassword)
		{
			ArrayList companiesOnSameServer = new ArrayList();

			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.sysDbConnString;
			companyDb.CurrentSqlConnection = this.sysDbConnection;

			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = this.sysDbConnString;
			companyUserDb.CurrentSqlConnection = this.sysDbConnection;

			// carico tutte le aziende che risiedono sullo stesso server di quella che sto analizzando
			if (!companyDb.SelectCompaniesSameServer(out companiesOnSameServer, this.dbCompanyServer))
				return loginPassword;

			// per ogni azienda il cui db risiede nel medesimo server
			// carico gli utenti ad esse associati e cerco la password corretta per quell'utente (se già associato)
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
					// controllo solo il nome della login, che deve essere univoca
					CompanyUser user = (CompanyUser)usersOfCompany[j];
					if (string.Compare(user.DBDefaultUser, loginName, StringComparison.InvariantCultureIgnoreCase) == 0)
						return user.DBDefaultPassword;
				}
			}

			return loginPassword;
		}
		# endregion


        //@@Anastasia no gestisco DMS
        //# region AssignLoginPasswordForDms
        /////<summary>
        ///// Per assegnare la corretta password nella MSD_SlaveLogins:
        ///// - carico tutti gli slave che hanno il database sul medesimo server
        ///// - per ognuno di questi carico gli utenti ad esso associati
        ///// - se trovo l'utente allora leggo la password, altrimenti utilizzo quella dell'utente applicativo
        /////</summary>
        ////---------------------------------------------------------------------
        //private string AssignLoginPasswordForDms(string loginName, string loginPassword)
        //{
        //    List<CompanyDBSlaveItem> slaveOnSameServer = new List<CompanyDBSlaveItem>();

        //    CompanyDBSlave companyDbSlave = new CompanyDBSlave();
        //    companyDbSlave.ConnectionString = this.sysDbConnString;
        //    companyDbSlave.CurrentSqlConnection = this.sysDbConnection;

        //    // carico tutti gli slave che risiedono sullo stesso server di quella che sto analizzando
        //    if (!companyDbSlave.SelectSlavesOnSameServer(out slaveOnSameServer, this.dmsServerName))
        //        return loginPassword;

        //    SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
        //    slaveLoginDb.ConnectionString = this.sysDbConnString;
        //    slaveLoginDb.CurrentSqlConnection = this.sysDbConnection;

        //    foreach (CompanyDBSlaveItem dbSlaveItem in slaveOnSameServer)
        //    {
        //        List<SlaveLoginItem> loginItems = new List<SlaveLoginItem>();
        //        if (!slaveLoginDb.SelectAllForSlaveId(dbSlaveItem.SlaveId, out loginItems))
        //        {
        //            diagnostic.Set(slaveLoginDb.Diagnostic);
        //            DiagnosticViewer.ShowDiagnostic(diagnostic);

        //            if (OnSendDiagnostic != null)
        //            {
        //                OnSendDiagnostic(this, diagnostic);
        //                diagnostic.Clear();
        //            }
        //        }

        //        // controllo solo il nome della login, che deve essere univoca
        //        foreach (SlaveLoginItem loginItem in loginItems)
        //            if (string.Compare(loginItem.SlaveDBUser, loginName, StringComparison.InvariantCultureIgnoreCase) == 0)
        //                return loginItem.SlaveDBPassword;
        //    }

        //    return loginPassword;
        //}
        //# endregion



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

		# region Gestione help
		//---------------------------------------------------------------------
		private void CallHelp(object sender, string nameSpace, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, nameSpace, searchParameter);
		}

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
		# endregion
	}
}