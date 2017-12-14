using System;
using System.Collections;
using System.Data.SqlClient;
using System.Globalization;
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
	public partial class DetailCompanyUser : PlugInsForm
	{
		#region Eventi e Delegati
		public delegate void ModifyTreeOfCompanies(object sender, string nodeType,string companyId);
		public event ModifyTreeOfCompanies OnModifyTreeOfCompanies;

		public delegate void AfterChangeDisabledCheckBox(object sender, string companyId, string loginId, bool disabled);
		public event AfterChangeDisabledCheckBox OnAfterChangeDisabledCheckBox;

		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;
		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;
		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;

		public delegate void CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event CallHelpFromPopUp OnCallHelpFromPopUp;

		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic	OnSendDiagnostic;


		public delegate bool IsActivated(string application, string functionality);
		public event IsActivated OnIsActivated;
		#endregion

		#region Variabili private
		Diagnostic diagnostic = new Diagnostic("SysAdmin.DetailCompanyUser"); 
		
		private SqlConnection sysDbConnection;
		private string sysDbConnString = string.Empty;

		private string companyId = string.Empty;
		private string loginId = string.Empty;
		private string userName = string.Empty;
		
		private bool isCurrentUserNt = false;
		private bool isGuest = false;
		private bool isEasyLookSystem = false;
		private bool hasClicked = false;

		private bool userIsDbo = false;

		// selezioni effettuate nella form
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
		//
		// se EasyBuilder e' attivato
		private bool isEasyBuilderActivated = false;

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
		//
		#endregion

		//--------------------------------------------------------------------
		public DetailCompanyUser
		(
			string connectionString,
			SqlConnection currentConnection,
			string companyId,
			string loginId,
			string userName
		)
		{
			InitializeComponent();
			this.sysDbConnString	= connectionString;
			this.sysDbConnection	= currentConnection;
			this.companyId			= companyId;
			this.loginId			= loginId;
			this.userName			= userName;
			State                   = StateEnums.View;
		}

    	#region Metodi per il caricamento delle info di utente/login
		///<summary>
		/// LoadCompanyUserData
		/// Leggo dal database di sistema i dati dell'utente associato che sto modificando e valorizzo
		/// i controls nella form
		///</summary>
		//--------------------------------------------------------------------
		private void LoadCompanyUserData()
		{
			bool loginDisabled = false;
			
			UserDb userDb = new UserDb();
			userDb.ConnectionString = sysDbConnString;
			userDb.CurrentSqlConnection = sysDbConnection;

			// carico le informazioni dell'utente applicativo
			ArrayList loginData = new ArrayList();
			userDb.GetAllUserFieldsById(out loginData, this.loginId);

			if (loginData != null && loginData.Count > 0)
			{
				UserItem infoLogin = (UserItem)loginData[0];
				loginDisabled = infoLogin.Disabled;
				isGuest = (string.Compare(infoLogin.Login, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0);
				isEasyLookSystem = (string.Compare(infoLogin.Login, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0);
			}

			// carico le informazioni dell'utente associato
			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = sysDbConnString;
			companyUserDb.CurrentSqlConnection = sysDbConnection;
			
			ArrayList companyUser = new ArrayList();
			companyUserDb.GetUserCompany(out companyUser, this.loginId, this.companyId);

			if (companyUser != null && companyUser.Count > 0)
			{
				RbSelectExistLogin.Checked = true;
				CompanyUser companyUserItem = (CompanyUser)companyUser[0];

				//leggo i valori e imposto le checkbox
				CbAdmin.Checked		= companyUserItem.Admin;
				CbDisable.Checked	= companyUserItem.Disabled;
				hasClicked = false;
				CbDisable.Enabled   = !loginDisabled;

				// la checkbox di EasyBuilder e' visibile solo se e' attivato
				EBDevCheckBox.Checked = companyUserItem.EasyBuilderDeveloper;
                if (isEasyBuilderActivated)
                {
                    EBDevCheckBox.Visible = true;
                }
                else
                {
                    EBDevCheckBox.Visible = false;
                    EBDevCheckBox.Checked = false;
                }
              
				//leggo le login e seleziono la prima
				ComboExistLogins.Items.Add(new LoginItem(companyUserItem.DBDefaultUser, companyUserItem.DBWindowsAuthentication));
				ComboExistLogins.DisplayMember = "loginName";
				ComboExistLogins.ValueMember   = "isNTUser";
				ComboExistLogins.SelectedIndex = 0;

				if (companyUserItem.DBWindowsAuthentication)
				{
					TbPassword.Text = string.Empty;
					TbPassword.Enabled = false;
				}
				else
				{
					TbPassword.Enabled = true;
					TbPassword.Text = string.IsNullOrEmpty(companyUserItem.DBDefaultPassword) ? ConstString.passwordEmpty : companyUserItem.DBDefaultPassword;
				}

				TbIfDbowner.Visible = false;

				userIsDbo = companyUserDb.IsDbo(this.loginId, this.companyId);

				// se si tratta dell'utente dbowner disabilito tutti i controls
				if (userIsDbo)
				{
					CbAdmin.Enabled				= false;
					CbDisable.Enabled			= false;
					ComboExistLogins.Enabled	= false;
					RbSelectExistLogin.Enabled	= false;
					RbCreateNewLogin.Enabled	= false;
					TbNewLoginName.Enabled		= false;
					LblPassword.Enabled         = false;
					TbPassword.Enabled			= false;
					BtnModify.Enabled			= false;
					TbIfDbowner.Visible			= true;
				}

				isCurrentUserNt = companyUserItem.WindowsAuthentication;
			}

			// se EasyBuilder e' attivato allora abilito il solo pulsante di salvataggio
			if (isEasyBuilderActivated)
				this.BtnModify.Enabled = true;

			//non posso modificare i settaggi
			if (isGuest || isEasyLookSystem)
			{
				this.GroupLoginSettings.Enabled = false;
				this.GroupsLoginConnection.Enabled = false;
				this.BtnModify.Enabled = false;
			}


			State  = StateEnums.View;
		}

		///<summary>
		/// Carico le logins associate all'azienda nella combobox
		///</summary>
		//---------------------------------------------------------------------
		private void LoadLogins()
		{
			//carico le login del server
			ComboExistLogins.Items.Clear();

			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

			string companyConnectionString = BuildCompanyConnectionString(this.companyId);

			if (string.IsNullOrWhiteSpace(companyConnectionString))
			{
				//non riesco a comporre la stringa di connessione all'azienda
				ComboExistLogins.Items.Clear();
				ComboExistLogins.Enabled = false;
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				return;
			}

			connSqlTransact.CurrentStringConnection = companyConnectionString;

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
					this.dbOwnerInstance,
					false
				);

			if (dataToConnectionServer == null)
			{
				Cursor.Current = Cursors.Default;
				return;
			}

			//sono sull'azienda, leggo le login
			ArrayList loginsOfCompany = new ArrayList();
			if (!connSqlTransact.GetLogins(out loginsOfCompany))
			{
				diagnostic.Set(DiagnosticType.Error, Strings.NotExistLogins);
				Cursor.Current = Cursors.Default;
				if (dataToConnectionServer != null)
					dataToConnectionServer.Undo();
				return;
			}

			if (loginsOfCompany != null && loginsOfCompany.Count > 0)
			{
				for (int i = 0; i < loginsOfCompany.Count; i++)
				{
					CompanyLogin companyLogin = (CompanyLogin)loginsOfCompany[i];

					//se è un gruppo NT, un utente NT locale o l'utente sa lo skippo
					if (companyLogin.IsNTGroup || companyLogin.IsLocalNTUser || companyLogin.IsSaUser)
						continue;

					//posso caricare solo login SQL + login NT sè stesso
					if (isCurrentUserNt)
					{
						if (companyLogin.IsNTUser)
							if (string.Compare(companyLogin.Login, this.TbLoginName.Text, StringComparison.InvariantCultureIgnoreCase) != 0)
								continue;
					}
					else
						if (companyLogin.IsNTUser)
							continue;

					ComboExistLogins.Items.Add(new LoginItem(companyLogin.Login, companyLogin.IsNTUser));
				}

				ComboExistLogins.DisplayMember = "loginName";
				ComboExistLogins.ValueMember = "isNTUser";

				//seleziono il primo
				if (ComboExistLogins.Items.Count > 0)
					ComboExistLogins.SelectedIndex = 0;
			}
		}
		# endregion

		# region Salvataggio modifiche
		///<summary>
		/// Click sul pulsante di salvataggio delle modifiche effettuate
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnModify_Click(object sender, System.EventArgs e)
		{
			bool result = SaveCompanyUser();
				//ModifyCompanyUser();

			State = (result) ? StateEnums.View : StateEnums.Editing;
			
			if (diagnostic.Error | diagnostic.Information | diagnostic.Warning)
				DiagnosticViewer.ShowDiagnosticAndClear(diagnostic);
		}

		///<summary>
		/// Nuovo metodo per il salvataggio delle informazioni dell'utente associato
		/// Esegue il salvataggio delle informazioni della form
		///</summary>
		//---------------------------------------------------------------------
		public bool SaveCompanyUser()
		{
			if (isGuest || isEasyLookSystem)
				return false;

			bool result = false;

			// memorizzo la login e la password da aggiungere o da creare inserita dall'utente
			if (RbCreateNewLogin.Checked)
			{
				loginSelected = TbNewLoginName.Text;
				loginPassword = TbPassword.Text;
				isLoginNT = false;
			}

			if (RbSelectExistLogin.Checked)
			{
				if (this.ComboExistLogins.SelectedItem == null)
				{
					State = StateEnums.Editing;
					return result;
				}

				loginSelected = ((LoginItem)ComboExistLogins.SelectedItem).LoginName;
				loginPassword = string.Empty;
				isLoginNT = ((LoginItem)ComboExistLogins.SelectedItem).IsNTUser;

				if (!isLoginNT)
				{
					if (string.Compare(TbPassword.Text, ConstString.passwordEmpty, StringComparison.InvariantCultureIgnoreCase) != 0)
						loginPassword = TbPassword.Text;
				}
			}
			//

			// se l'utente e' dbowner devo solo modificare il flag di EasyBuilder
			// non entro nel merito di tutto il resto...
			if (userIsDbo && isEasyBuilderActivated)
			{
				result = UpdateEBDeveloperForDbo();

				if (result)
				{
					State = StateEnums.View;

					if (OnModifyTreeOfCompanies != null)
						OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
					if (OnModifyTreeOfCompanies != null)
						OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);
				}
				else
					State = StateEnums.Editing;

				return result;
			}

			bool dmsToManage = false;

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
				// Aggiorno la MSD_CompanyLogins (e l'eventuale MSD_SlaveLogins) 
				// aggiungendo/modificando le righe ed impostando la DBUser=LoginName e DBPassword=Password
				result = UpdateSystemDBTables(dmsToManage);

				if (result)
				{
					State = StateEnums.View;

					if (OnAfterChangeDisabledCheckBox != null)
						OnAfterChangeDisabledCheckBox(this, this.companyId, this.loginId, CbDisable.Checked);
					if (OnModifyTreeOfCompanies != null)
						OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
					if (OnModifyTreeOfCompanies != null)
						OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);
				}
				else
					State = StateEnums.Editing;
			}
			
			if (companyImpersonated != null)
				companyImpersonated.Undo();
			if (dmsImpersonated != null)
				dmsImpersonated.Undo();

			return result;
		}

		/// <summary>
		/// UpdateEBDeveloperForDbo
		/// esegue l'aggiornamento del solo flag di EasyBuilder sulla tabella MSD_CompanyLogins
		/// per l'utente dbowner
		/// </summary>
		//---------------------------------------------------------------------
		private bool UpdateEBDeveloperForDbo()
		{
			UserListItem currentItemSelected = new UserListItem();
			currentItemSelected.LoginId = this.loginId;
			currentItemSelected.CompanyId = this.companyId;
			currentItemSelected.Login = this.userName;
			currentItemSelected.IsAdmin = CbAdmin.Checked;
			currentItemSelected.Disabled = CbDisable.Checked;
			currentItemSelected.EasyBuilderDeveloper = EBDevCheckBox.Checked;
			currentItemSelected.DbUser = loginSelected;
			currentItemSelected.DbPassword = loginPassword;
			currentItemSelected.DbWindowsAuthentication = isLoginNT;

			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = this.sysDbConnString;
			companyUserDb.CurrentSqlConnection = this.sysDbConnection;

			bool result = companyUserDb.Modify(currentItemSelected);

			if (!result)
			{
				if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
				{
					diagnostic.Set(companyUserDb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
				}

				State = StateEnums.Editing;
			}

			return result;
		}

		/// <summary>
		/// UpdateSystemDBTables
		/// esegue l'aggiornamento delle login sulle tabelle del database di sistema
		/// (MSD_CompanyLogins e MSD_SlaveLogins)
		/// </summary>
		//---------------------------------------------------------------------
		private bool UpdateSystemDBTables(bool isDmsManage)
		{
			bool result = false;

			// rimuovo l'associazione delle login ai databases direttamente da SQL Server
			RevokeDbLogins(isDmsManage);

			UserListItem currentItemSelected = new UserListItem();
			currentItemSelected.LoginId = this.loginId;
			currentItemSelected.CompanyId = this.companyId;
			currentItemSelected.Login = this.userName;
			currentItemSelected.IsAdmin = CbAdmin.Checked;
			currentItemSelected.Disabled = CbDisable.Checked;
			currentItemSelected.EasyBuilderDeveloper = EBDevCheckBox.Checked;
			currentItemSelected.DbUser = loginSelected;
			currentItemSelected.DbPassword = loginPassword;
			currentItemSelected.DbWindowsAuthentication = isLoginNT;

			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = this.sysDbConnString;
			companyUserDb.CurrentSqlConnection = this.sysDbConnection;

			// vado a modificare le info dell'utente sul database di sistema
			// se l'utente e' stato disabilitato visualizzo anche un messaggio ad hoc
			if (CbDisable.Checked && hasClicked)
			{
				DialogResult confirmUserDisabled =
					MessageBox.Show
					(
						this,
						Strings.AskBeforeDisableUser,
						Strings.Warning,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2
					);

				if (confirmUserDisabled == DialogResult.Yes)
					result = companyUserDb.Modify(currentItemSelected);
				else
				{
					State = StateEnums.Editing;
					return false;
				}
			}
			else
				result = companyUserDb.Modify(currentItemSelected);

			if (!result)
			{
				if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
				{
					diagnostic.Set(companyUserDb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
				}

				State = StateEnums.Editing;
			}

			// se sono riuscita ad aggiornare la tabella MSD_CompanyLogins
			// procedo ad aggiornare anche la MSD_SlaveLogins
			if (result && isDmsManage)
			{
				CompanyDBSlave companyDBSlave = new CompanyDBSlave();
				companyDBSlave.ConnectionString = this.sysDbConnString;
				companyDBSlave.CurrentSqlConnection = this.sysDbConnection;

				// leggo il record associato alla company nella tabella MSD_CompanyDBSlaves, per avere lo slaveId
				CompanyDBSlaveItem dbSlaveItem = new CompanyDBSlaveItem();
				if (companyDBSlave.SelectSlaveForCompanyIdAndSignature(currentItemSelected.CompanyId, DatabaseLayerConsts.DMSSignature, out dbSlaveItem))
				{
					SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
					slaveLoginDb.ConnectionString = this.sysDbConnString;
					slaveLoginDb.CurrentSqlConnection = this.sysDbConnection;

					result = slaveLoginDb.ExistLoginForSlaveId(currentItemSelected.LoginId, dbSlaveItem.SlaveId)
						? slaveLoginDb.Modify(currentItemSelected, dbSlaveItem.SlaveId)
						: slaveLoginDb.Add(currentItemSelected, dbSlaveItem.SlaveId);
				}
			}

			return result;
		}
		# endregion

		# region Check logins
		///<summary>
		/// AddUsersOnSameServer
		/// Richiamata se i due database sono sul medesimo server (oppure se devo gestire solo il database aziendale)
		///</summary>
		//---------------------------------------------------------------------
		private bool AddUsersOnSameServer(bool dmsToManage)
		{
			bool result = false;

			//Step 1. Se la login è nuova, la inserisco, controllando che non esista
			//        nel caso dò un messaggio di errore e mi fermo
			if (RbCreateNewLogin.Checked)
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
			if (RbSelectExistLogin.Checked)
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
			if (RbCreateNewLogin.Checked)
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
			if (RbSelectExistLogin.Checked)
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
		# endregion

		# region Add/grant logins - Try to connect
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
		# endregion

		# region Gestione connessioni ai database
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
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, companyDb.Diagnostic);
					diagnostic.Clear();
				}
				companyData.Clear();
			}

			if (companyData != null && companyData.Count > 0)
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
				if (userDboData != null && userDboData.Count > 0)
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
				this.dmsDbOwnerDomain = Path.GetDirectoryName(dmsDbOwnerLogin);
			}
			else
				dmsConnectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, dmsServerName, dmsDatabaseName, dmsDbOwnerLogin, dmsDbOwnerPassword);

			return dmsConnectionString;
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
		# endregion

		#region Revoke logins
		///<summary>
		/// Rimuove le login dal database aziendale (e da quello documentale se richiesto)
		/// ma solo se il nome login e la password sono diverse da quelle memorizzate
		/// </summary>
		//---------------------------------------------------------------------
		private void RevokeDbLogins(bool dmsToManage)
		{
			bool result = false;

			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = sysDbConnString;
			companyUserDb.CurrentSqlConnection = sysDbConnection;

			// carico le info dell'utente associato
			ArrayList companyUser = new ArrayList();
			companyUserDb.GetUserCompany(out companyUser, this.loginId, this.companyId);

			if (companyUser != null && companyUser.Count > 0)
			{
				CompanyUser itemCompanyUser = (CompanyUser)companyUser[0];

				if (string.Compare(itemCompanyUser.DBDefaultUser, loginSelected, StringComparison.InvariantCultureIgnoreCase) != 0 ||
					string.Compare(itemCompanyUser.DBDefaultPassword, loginPassword, StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					//eventualmente cancello la "vecchia login su SQL"
					if (companyUserDb.CanDropLogin(itemCompanyUser.DBDefaultUser, itemCompanyUser.CompanyId))
						result = RevokeCompanyLogin(itemCompanyUser.DBDefaultUser, itemCompanyUser.CompanyId);

					if (dmsToManage)
					{
						SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
						slaveLoginDb.ConnectionString = sysDbConnString;
						slaveLoginDb.CurrentSqlConnection = sysDbConnection;

						if (slaveLoginDb.CanDropLogin(itemCompanyUser.DBDefaultUser, companyId))
							RevokeDmsLogin(itemCompanyUser.DBDefaultUser, companyId);
					}
				}
			}
		}

		///<summary>
		/// Rimuove la login dal database aziendale
		/// Metodo pubblico perche' richiamato anche dalla cancellazione utente nel SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		public bool RevokeCompanyLogin(string userName, string companyId)
		{
			bool result = false;

			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

			string companyConnectionString = CreateCompanyConnectionString();

			if (string.IsNullOrWhiteSpace(companyConnectionString))
			{
				//non riesco a comporre la stringa di connessione al database aziendale
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				return false;
			}

			connSqlTransact.CurrentStringConnection = companyConnectionString;

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
					this.dbOwnerInstance,
					false
				);

			if (dataToConnectionServer == null)
			{
				Cursor.Current = Cursors.Default;
				return false;
			}

			//se la login esiste
			if (connSqlTransact.ExistLogin(userName))
				result = connSqlTransact.SPRevokeDbAccess(userName, this.dbCompanyName);

			if (dataToConnectionServer != null)
				dataToConnectionServer.Undo();

			return result;
		}

		///<summary>
		/// Rimuove la login dal database dms
		/// Metodo pubblico perche' richiamato anche dalla cancellazione utente nel SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		public bool RevokeDmsLogin(string userName, string companyId)
		{
			bool result = false;

			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

			string dmsConnectionString = CreateDmsConnectionString();

			if (string.IsNullOrWhiteSpace(dmsConnectionString))
			{
				//non riesco a comporre la stringa di connessione al dms
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				return false;
			}

			connSqlTransact.CurrentStringConnection = dmsConnectionString;

			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
			//eventualmente eseguo l'impersonificazione
			dataToConnectionServer =
				connSqlTransact.LoginImpersonification
				(
					this.dmsDbOwnerLogin,
					this.dmsDbOwnerPassword,
					this.dmsDbOwnerDomain,
					this.dmsDbOwnerWinAuth,
					this.dmsDbOwnerPrimary,
					this.dmsDbOwnerInstance,
					false
				);

			if (dataToConnectionServer == null)
			{
				Cursor.Current = Cursors.Default;
				return false;
			}

			//se la login esiste
			if (connSqlTransact.ExistLogin(userName))
				result = connSqlTransact.SPRevokeDbAccess(userName, dmsDatabaseName);

			if (dataToConnectionServer != null)
				dataToConnectionServer.Undo();

			return result;
		}
		#endregion

		# region Eventi sui controls della form
		//---------------------------------------------------------------------
		private void DetailCompanyUser_Load(object sender, System.EventArgs e)
		{
			LabelTitle.Text = string.Format(LabelTitle.Text, this.userName);
			TbLoginName.Text = this.userName;

			//setto a false ciò che non è disponibile
			TbNewLoginName.Enabled = false;
			TbPassword.Enabled     = false;
			TbIfDbowner.Visible	   = false;

			// l'evento posso spararlo solo nella Load, perche' nel costruttore non e' ancora stato 
			// agganciato e valorizzato!
			if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				isDMSActivated = true;

            isEasyBuilderActivated = OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner);
				

			//carico le informazioni dell'utente
			LoadCompanyUserData();
		}

		//---------------------------------------------------------------------
		private void ComboExistLogins_DropDown(object sender, System.EventArgs e)
		{
			LoadLogins();
		}

		//---------------------------------------------------------------------
		private void ComboExistLogins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;

			if (ComboExistLogins.SelectedItem != null)
			{
				bool isNtUser = ((LoginItem)ComboExistLogins.SelectedItem).IsNTUser;
				TbPassword.Enabled = !isNtUser;
				LblPassword.Enabled = !isNtUser;
				if (!isNtUser)
					TbPassword.Clear();
			}
		}

		//---------------------------------------------------------------------
		private void RbCreateNewLogin_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;

			if (RbCreateNewLogin.Checked)
			{
				ComboExistLogins.Enabled = false;
				TbNewLoginName.Enabled = true;
				TbNewLoginName.Text = string.Empty;
				TbPassword.Enabled = true;
				TbPassword.Text = string.Empty;
			}
		}

		//---------------------------------------------------------------------
		private void RbSelectExistLogin_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;

			if (RbSelectExistLogin.Checked)
			{
				ComboExistLogins.Enabled = true;
				TbNewLoginName.Enabled = false;
				TbPassword.Enabled = true;
			}
		}

		//---------------------------------------------------------------------
		private void DetailCompanyUser_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		//---------------------------------------------------------------------
		private void DetailCompanyUser_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		//---------------------------------------------------------------------
		private void DetailCompanyUser_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}

		//---------------------------------------------------------------------
		private void TbPassword_TextChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void TbNewLoginName_TextChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void CbAdmin_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void CbDisable_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
			hasClicked = true;
		}

		//---------------------------------------------------------------------
		private void EBDevCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			State = StateEnums.Editing;
		}
		#endregion

		# region Eventi impersonificazione utente
		///<summary>
		/// AddUserAuthentication
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

		//---------------------------------------------------------------------
		private void CallHelp(object sender, string nameSpace, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, nameSpace, searchParameter);
		}

		//---------------------------------------------------------------------
		// METODI NON PIU' UTILIZZATI - VECCHIA GESTIONE
		//---------------------------------------------------------------------

		///<summary>
		/// Esegue il salvataggio delle informazioni della form
		/// VECCHIA GESTIONE
		///</summary>
		//---------------------------------------------------------------------
		public bool ModifyCompanyUser()
		{
			if (isGuest || isEasyLookSystem)
				return false;

			//salvo
			bool modifyAll = true;
			string loginName = string.Empty;
			string loginPassword = string.Empty;
			bool isNTUser = false;

			ArrayList companyUser = new ArrayList();
			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = sysDbConnString;
			companyUserDb.CurrentSqlConnection = sysDbConnection;
			companyUserDb.GetUserCompany(out companyUser, this.loginId, this.companyId);

			if (companyUser != null && companyUser.Count > 0)
			{
				//Costruisco la stringa di connessione all'azienda selezionata (eventualmente
				//impersonificando il dbowner dell'azienda in questione
				TransactSQLAccess connSqlTransact = new TransactSQLAccess();
				connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
				connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
				connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
				connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
				connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);

				string companyConnectionString = BuildCompanyConnectionString(this.companyId);

				if (string.IsNullOrWhiteSpace(companyConnectionString))
				{
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
					DiagnosticViewer.ShowDiagnostic(diagnostic);

					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}

					State = StateEnums.Editing;
					return false;
				}

				connSqlTransact.CurrentStringConnection = companyConnectionString;

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
						this.dbOwnerInstance,
						false
					);

				if (dataToConnectionServer == null)
				{
					Cursor.Current = Cursors.Default;
					State = StateEnums.Editing;
					return false;
				}

				//sto utilizzando una login esistente.. verifico la connessione prima di procedere
				if (RbSelectExistLogin.Checked)
				{
					isNTUser = ((LoginItem)ComboExistLogins.SelectedItem).IsNTUser;
					loginName = ((LoginItem)ComboExistLogins.SelectedItem).LoginName;
					loginPassword = string.Empty;

					if (!isNTUser)
					{
						if (string.Compare(TbPassword.Text, ConstString.passwordEmpty, StringComparison.InvariantCultureIgnoreCase) != 0)
							loginPassword = TbPassword.Text;
					}

					if (string.IsNullOrWhiteSpace(loginName))
					{
						diagnostic.Set(DiagnosticType.Warning, Strings.NotSelectedLogins);
						DiagnosticViewer.ShowDiagnostic(diagnostic);

						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}

						if (dataToConnectionServer != null)
							dataToConnectionServer.Undo();

						Cursor.Current = Cursors.Default;
						State = StateEnums.Editing;
						return false;
					}

					bool resultGrant = false;

					// se la login non esiste sul database prima la granto e poi provo a connettermi
					if (!connSqlTransact.ExistLoginIntoDb(loginName, this.dbCompanyName))
						resultGrant = GrantLoginToCompany(isNTUser, loginName, loginPassword, connSqlTransact) &&
								TryToConnect(isNTUser, loginName, loginPassword);
					else // se la login esiste sul database provo solo a connettermi
						resultGrant = TryToConnect(isNTUser, loginName, loginPassword);

					if (!resultGrant)
					{
						diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CannotConnectWithLogin);
						DiagnosticViewer.ShowDiagnostic(diagnostic);

						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}

						if (dataToConnectionServer != null)
							dataToConnectionServer.Undo();

						State = StateEnums.Editing;
						return false;
					}
				}
				else
				{	// sto inserendo una nuova login
					loginName = TbNewLoginName.Text;
					loginPassword = TbPassword.Text;
					isNTUser = false;

					if (string.IsNullOrWhiteSpace(loginName))
					{
						diagnostic.Set(DiagnosticType.Warning, Strings.NoLoginSpecified);
						DiagnosticViewer.ShowDiagnostic(diagnostic);

						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}

						if (dataToConnectionServer != null)
							dataToConnectionServer.Undo();

						Cursor.Current = Cursors.Default;
						State = StateEnums.Editing;
						return false;
					}

					// se la login esiste sul server non procedo
					if (connSqlTransact.ExistLogin(loginName))
					{
						diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.LoginAlreadyExist, loginName));
						diagnostic.Set(connSqlTransact.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);

						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}

						if (dataToConnectionServer != null)
							dataToConnectionServer.Undo();

						State = StateEnums.Editing;
						return false;
					}

					// provo ad aggiungere la login all'azienda
					if (!AddLoginToCompany(isNTUser, loginName, loginPassword, connSqlTransact))
					{
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotAddLogin, loginName));
						diagnostic.Set(connSqlTransact.Diagnostic);

						DiagnosticViewer.ShowDiagnostic(diagnostic);

						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}

						if (dataToConnectionServer != null)
							dataToConnectionServer.Undo();

						State = StateEnums.Editing;
						return false;
					}
				}

				CompanyUser itemCompanyUser = (CompanyUser)companyUser[0];

				if ((string.Compare(itemCompanyUser.DBDefaultUser, loginName, StringComparison.InvariantCultureIgnoreCase) != 0) ||
					(string.Compare(itemCompanyUser.DBDefaultPassword, loginPassword, StringComparison.InvariantCultureIgnoreCase) != 0))
				{
					//eventualmente cancello la "vecchia login su SQL"
					if (companyUserDb.CanDropLogin(itemCompanyUser.DBDefaultUser, itemCompanyUser.CompanyId))
						RevokeCompanyLogin(itemCompanyUser.DBDefaultUser, itemCompanyUser.CompanyId);
				}

				UserListItem currentItemSelected = new UserListItem();
				currentItemSelected.LoginId = this.loginId;
				currentItemSelected.CompanyId = this.companyId;
				currentItemSelected.Login = this.userName;
				currentItemSelected.IsAdmin = CbAdmin.Checked;
				currentItemSelected.Disabled = CbDisable.Checked;
				currentItemSelected.EasyBuilderDeveloper = EBDevCheckBox.Checked;
				currentItemSelected.DbUser = loginName;
				currentItemSelected.DbPassword = loginPassword;
				currentItemSelected.DbWindowsAuthentication = isNTUser;

				bool result = false;

				// vado a modificare le info dell'utente sul database di sistema
				if (CbDisable.Checked && hasClicked)
				{
					DialogResult confirmUserDisabled =
						MessageBox.Show
						(
							this,
							Strings.AskBeforeDisableUser,
							Strings.Warning,
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question,
							MessageBoxDefaultButton.Button2
						);

					if (confirmUserDisabled == DialogResult.Yes)
						result = companyUserDb.Modify(currentItemSelected);
					else
					{
						if (dataToConnectionServer != null)
							dataToConnectionServer.Undo();

						State = StateEnums.Editing;
						return false;
					}
				}
				else
					result = companyUserDb.Modify(currentItemSelected);

				if (!result)
				{
					if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
					{
						diagnostic.Set(companyUserDb.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);

						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}
					}

					if (dataToConnectionServer != null)
						dataToConnectionServer.Undo();

					State = StateEnums.Editing;
				}
				else
					State = StateEnums.View;

				if (OnAfterChangeDisabledCheckBox != null)
					OnAfterChangeDisabledCheckBox(this, this.companyId, this.loginId, CbDisable.Checked);
				if (OnModifyTreeOfCompanies != null)
					OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
				if (OnModifyTreeOfCompanies != null)
					OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);
				if (dataToConnectionServer != null)
					dataToConnectionServer.Undo();
			}

			return modifyAll;
		}

		//---------------------------------------------------------------------
		private string BuildCompanyConnectionString(string companyId)
		{
			string connectionToCompanyServer = string.Empty;
			string dbOwnerId = string.Empty;

			//leggo i dati dall'azienda
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.sysDbConnString;
			companyDb.CurrentSqlConnection = this.sysDbConnection;
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

			if (companyData != null && companyData.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)companyData[0];
				this.dbCompanyServer = companyItem.DbServer;
				this.dbCompanyName = companyItem.DbServer;
				dbOwnerId = companyItem.DbOwner;

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

				if (userDboData != null && userDboData.Count > 0)
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
						connectionToCompanyServer =
							string.Format(NameSolverDatabaseStrings.SQLConnection, this.dbCompanyServer, this.dbCompanyName, this.dbOwnerLogin, this.dbOwnerPassword);
				}
			}

			return connectionToCompanyServer;
		}

		//---------------------------------------------------------------------
		private bool GrantLoginToCompany(bool isLoginNT, string loginName, string loginPassword, TransactSQLAccess connSqlTransact)
		{
			if (isLoginNT)
			{
				return
					connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName);
			}
			else
			{
				return
					connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName);
			}
		}

		//---------------------------------------------------------------------
		private bool TryToConnect(bool isLoginNT, string loginName, string loginPassword)
		{
			TransactSQLAccess tentativeConnSql = new TransactSQLAccess();

			tentativeConnSql.CurrentStringConnection = (isLoginNT)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, this.dbCompanyServer, this.dbCompanyName)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, this.dbCompanyServer, this.dbCompanyName, loginName, loginPassword);

			return tentativeConnSql.TryToConnect();
		}

		//---------------------------------------------------------------------
		private bool AddLoginToCompany(bool isLoginNT, string loginName, string loginPassword, TransactSQLAccess connSqlTransact)
		{
			if (isLoginNT)
			{
				return
					connSqlTransact.SPGrantLogin(loginName) &&
					connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName);
			}
			else
			{
				return
					connSqlTransact.SPAddLogin(loginName, loginPassword, DatabaseLayerConsts.MasterDatabase) &&
					connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
					connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName);
			}
		}
	}
}