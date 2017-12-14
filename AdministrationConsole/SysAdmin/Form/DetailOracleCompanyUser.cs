using System;
using System.Collections;
using System.Data.SqlClient;
using System.Diagnostics;
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
using Microarea.TaskBuilderNet.Data.SQLDataAccess;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// DetailOracleCompanyUser.
	/// Visualizzazione del Dettaglio Utente Oracle (ed eventualmente modifica)
	/// </summary>
	// ========================================================================
	public partial class DetailOracleCompanyUser : PlugInsForm
	{
		#region Eventi e Delegati
		//---------------------------------------------------------------------
		public delegate void	ModifyTreeOfCompanies (object sender, string nodeType,string companyId);
		public event			ModifyTreeOfCompanies OnModifyTreeOfCompanies;
		//---------------------------------------------------------------------
		public delegate void	AfterChangeDisabledCheckBox (object sender, string companyId, string loginId, bool disabled);
		public event			AfterChangeDisabledCheckBox OnAfterChangeDisabledCheckBox;
		//---------------------------------------------------------------------
		public delegate void	SendDiagnostic  (object sender, Diagnostic diagnostic);
		public event			SendDiagnostic	OnSendDiagnostic;
		//---------------------------------------------------------------------
		public delegate void    CallHelpFromPopUp	(object sender, string nameSpace, string searchParameter);
		public event			CallHelpFromPopUp	OnCallHelpFromPopUp;
		//---------------------------------------------------------------------
		public delegate void	EnableProgressBar (object sender);
		public event			EnableProgressBar OnEnableProgressBar;
		//---------------------------------------------------------------------
		public delegate void	DisableProgressBar (object sender);
		public event			DisableProgressBar OnDisableProgressBar;
		//---------------------------------------------------------------------
		public delegate void	SetProgressBarStep (object sender, int step);
		public event			SetProgressBarStep OnSetProgressBarStep;
		//---------------------------------------------------------------------
		public delegate void	SetProgressBarValue (object sender, int currentValue);
		public event			SetProgressBarValue OnSetProgressBarValue;
		//---------------------------------------------------------------------
		public delegate void	SetProgressBarText (object sender, string message);
		public event			SetProgressBarText OnSetProgressBarText;
		//---------------------------------------------------------------------
		public delegate bool	IsUserAuthenticatedFromConsole	(string login, string password, string serverName);
		public event			IsUserAuthenticatedFromConsole	OnIsUserAuthenticatedFromConsole;
		//---------------------------------------------------------------------
		public delegate void	AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event			AddUserAuthenticatedFromConsole	OnAddUserAuthenticatedFromConsole;
		//---------------------------------------------------------------------
		public delegate string	GetUserAuthenticatedPwdFromConsole	(string login, string serverName);
		public event			GetUserAuthenticatedPwdFromConsole  OnGetUserAuthenticatedPwdFromConsole;

		public delegate bool IsActivated(string application, string functionality);
		public event IsActivated OnIsActivated;

		#endregion

		#region Variabili
		//---------------------------------------------------------------------
		private Diagnostic diagnostic = new Diagnostic("SysAdmin.DetailOracleCompanyUser");

		private string	selectedApplicationUserName	= string.Empty;
		private string	selectedApplicationUserId	= string.Empty;
		private string	selectedApplicationUserPwd	= string.Empty;
	
		private string	sysDbConnString	= string.Empty;
		private SqlConnection sysDbConnection;

		private string	companyId					= string.Empty;
		private string  loginId						= string.Empty;
		private string	userName					= string.Empty;
		private bool    isNTUser					= false;
		private bool	isNTLogin					= false;
		private string  databaseName				= string.Empty;
		private string  company						= string.Empty;
		private string	companyName					= string.Empty;
		private string	companyDbName				= string.Empty;
		private string	companyDbOwner				= string.Empty;
		private bool	companyUseSlave				= false;

		private bool	ownerIsAdmin				= false;
		private string	ownerDbUser					= string.Empty;
		private string	ownerDbPwd					= string.Empty;
		private bool	ownerDbWinAuth				= false;
		private bool	isNewOracleUser				= false;
		private bool	newOracleUserIsWinNT		= false;

		private bool userIsDbo = false;

		// se EasyBuilder e' attivato
		private bool isEasyBuilderActivated = false;

		// variabili per il database documentale
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

		/// <summary>
		/// Costruttore (con parametri)
		/// </summary>
		//---------------------------------------------------------------------
		public DetailOracleCompanyUser
			(
				string			connectionString, 
				SqlConnection	currentConnection,
				string			companyId,
				string          loginId,
				string			userName
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

		#region ModifyCompanyUser - Modifico i dati dell'utente
		/// <summary>
		/// ModifyCompanyUser
		/// Modifico i dati dell'utente
		/// </summary>
		//---------------------------------------------------------------------
		public bool ModifyCompanyUser()
		{
			// Ricavo il corrente cursore della form della console e lo salvo
			// per poterlo poi riassegnare in seguito, una volta terminata l'elaborazione
			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;

			bool result	= false;

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

			isNTUser = false;

			bool dmsToManage = false;

			if (CheckUserData())
			{
				// se il modulo dms e' attivato e l'azienda ha uno slave associato procedo con i controlli sul database
				if (isDMSActivated && companyUseSlave)
				{
					// se non sono riuscita a connettermi al database documentale non procedo
					if (!ConnectToDmsDatabase())
						return false;

					// adesso devo controllare che l'utente abbia i permessi di amministrazione
					if (!CheckHasRoleSysAdmin(dmsDbOwnerLogin))
						return false;

					dmsToManage = true;
				}

				//setto il cursore
				this.TopLevelControl.Cursor = Cursors.WaitCursor;
				Cursor.Current				= Cursors.WaitCursor;
				SetConsoleProgressBarText(this, Strings.ProgressWaiting);

				string oracleUser = rbSelectExistedOracleUser.Checked ? ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId : txtNewOracleUser.Text;
				isNTUser = rbSelectExistedOracleUser.Checked ? ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent : CbNTSecurity.Checked;
				
				string oracleUserPwd = (isNTUser) ? string.Empty : TbOracleUserPwd.Text;
				bool insertUser = true;
				
				if (isNewOracleUser)
					insertUser = AddNewOracleUser(TextBoxOracleService.Text, oracleUser, oracleUserPwd, isNTUser);
				
				if (insertUser && CheckOracleConnection(true))
				{
					ArrayList companyUser               = new ArrayList();
					CompanyUserDb companyUserDb			= new CompanyUserDb();
					companyUserDb.ConnectionString		= sysDbConnString;
					companyUserDb.CurrentSqlConnection	= sysDbConnection;
					//Leggo le info dell'utente
					companyUserDb.GetUserCompany(out companyUser, this.loginId, this.companyId);

					if (companyUser.Count > 0)
					{
						CompanyUser itemCompanyUser			= (CompanyUser)companyUser[0];
						UserListItem currentItemSelected	= new UserListItem();
						currentItemSelected.Login			= itemCompanyUser.Login;
						currentItemSelected.LoginId			= itemCompanyUser.LoginId;
						currentItemSelected.CompanyId		= itemCompanyUser.CompanyId;
						currentItemSelected.DbUser			= oracleUser;
						currentItemSelected.DbPassword		= oracleUserPwd;
						currentItemSelected.IsAdmin			= CbAdmin.Checked;
						currentItemSelected.EasyBuilderDeveloper = EBDevCheckBox.Checked;
						currentItemSelected.Disabled		= CbDisable.Checked;
						currentItemSelected.DbWindowsAuthentication = isNTUser;

						if (CbDisable.Checked)
						{
							DialogResult confirmUserDisabled = MessageBox.Show
								(
								this, 
								Strings.AskBeforeDisableUser, 
								Strings.Warning, 
								MessageBoxButtons.YesNo, 
								MessageBoxIcon.Question, 
								MessageBoxDefaultButton.Button2
								);
							if (confirmUserDisabled == DialogResult.No)
							{
								//rimetto a posto il cursore
								this.TopLevelControl.Cursor = currentConsoleFormCursor;
								Cursor.Current = currentConsoleFormCursor;
								SetConsoleProgressBarText(this, string.Empty);
								return result;
							}
						}
						
						//prelevo le vecchie info dell'utente se ho modificato l'associazione
						if (CbChangeApplicationUser.Checked)
						{
							ArrayList userOfCompany = new ArrayList();
							if (companyUserDb.GetUserCompany(out userOfCompany, itemCompanyUser.LoginId, itemCompanyUser.CompanyId))
							{
								CompanyUser currentOldUser = (CompanyUser)userOfCompany[0];
								if (companyUserDb.CanDropLogin(currentOldUser.DBDefaultUser, this.companyId))
									DropSynonyms(this.companyDbName, TextBoxOracleService.Text, currentOldUser.DBDefaultUser, currentOldUser.DBDefaultPassword, currentOldUser.DBWindowsAuthentication);
							}
						}

						//prima dovrei cancellare associazione solo se la login Oracle non è usata su questo schema da altri utenti
						if (CreateSynonyms(companyDbName, currentItemSelected.DbUser, currentItemSelected.DbPassword, currentItemSelected.DbWindowsAuthentication))
						{
							result = companyUserDb.Modify(currentItemSelected);
							//rimetto a posto il cursore
							this.TopLevelControl.Cursor = currentConsoleFormCursor;
							Cursor.Current = currentConsoleFormCursor;
							SetConsoleProgressBarText(this, string.Empty);

							if (!result)
							{
								if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Information)
									diagnostic.Set(companyUserDb.Diagnostic);
								else
									diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UnableToModifyOracleUser, this.userName));
								DiagnosticViewer.ShowDiagnostic(diagnostic);
								if (OnSendDiagnostic != null)
									OnSendDiagnostic(this, diagnostic);
								diagnostic.Clear();
							}
							else
							{
								// se l'aggiornamento delle logins della company e' andato a buon fine
								// procedo con quello del dms
								if (dmsToManage)
								{
									result = GrantLogin();
									
									if (result)
									{
										// leggo il record associato alla company nella tabella MSD_CompanyDBSlaves, per avere lo slaveId
										CompanyDBSlave companyDBSlave = new CompanyDBSlave();
										companyDBSlave.ConnectionString = this.sysDbConnString;
										companyDBSlave.CurrentSqlConnection = this.sysDbConnection;

										CompanyDBSlaveItem dbSlaveItem;
										if (companyDBSlave.SelectSlaveForCompanyIdAndSignature(this.companyId, DatabaseLayerConsts.DMSSignature, out dbSlaveItem))
										{
											SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
											slaveLoginDb.ConnectionString = this.sysDbConnString;
											slaveLoginDb.CurrentSqlConnection = this.sysDbConnection;

											//prelevo le vecchie info dell'utente se ho modificato l'associazione
											if (CbChangeApplicationUser.Checked)
											{
												ArrayList userOfCompany = new ArrayList();
												if (companyUserDb.GetUserCompany(out userOfCompany, itemCompanyUser.LoginId, itemCompanyUser.CompanyId))
												{
													CompanyUser currentOldUser = (CompanyUser)userOfCompany[0];
													if (slaveLoginDb.CanDropLogin(currentOldUser.DBDefaultUser, this.companyId))
													{
														if (dmsConnSqlTransact.ExistLogin(currentOldUser.DBDefaultUser))
															dmsConnSqlTransact.SPRevokeDbAccess(currentOldUser.DBDefaultUser, dmsDatabaseName);
													}
												}
											}

											if (!slaveLoginDb.ExistLoginForSlaveId(currentItemSelected.LoginId, dbSlaveItem.SlaveId))
												slaveLoginDb.Add(dbSlaveItem.SlaveId, currentItemSelected.LoginId, DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.DmsOraUserPw, false);
											else
												slaveLoginDb.Modify(dbSlaveItem.SlaveId, currentItemSelected.LoginId, DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.DmsOraUserPw, false);
										}
									}
								}

								//pulisco il diagnostico, no errori
								diagnostic.Clear();
								State = StateEnums.View;
								if (OnAfterChangeDisabledCheckBox != null)
									OnAfterChangeDisabledCheckBox(this, this.companyId, this.loginId, CbDisable.Checked);
								if (OnModifyTreeOfCompanies != null)
									OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
								if (OnModifyTreeOfCompanies != null)
									OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);
							}
						}
						else
						{
							//rimetto a posto il cursore
							this.TopLevelControl.Cursor = currentConsoleFormCursor;
							Cursor.Current = currentConsoleFormCursor;
							SetConsoleProgressBarText(this, string.Empty);
						}
					}
				}
				else
				{
					//rimetto a posto il cursore
					this.TopLevelControl.Cursor = currentConsoleFormCursor;
					Cursor.Current = currentConsoleFormCursor;
					SetConsoleProgressBarText(this, string.Empty);
				}
			}
			
			return result;
		}

		/// <summary>
		/// UpdateEBDeveloperForDbo
		/// esegue l'aggiornamento del solo flag di EasyBuilder sulla tabella MSD_CompanyLogins
		/// per l'utente dbowner
		/// </summary>
		//--------------------------------------------------------------------
		private bool UpdateEBDeveloperForDbo()
		{
			string oracleUser = rbSelectExistedOracleUser.Checked ? ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId : txtNewOracleUser.Text;
			isNTUser = rbSelectExistedOracleUser.Checked ? ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent : CbNTSecurity.Checked;

			string oracleUserPwd = (isNTUser) ? string.Empty : TbOracleUserPwd.Text;

			UserListItem currentItemSelected = new UserListItem();
			currentItemSelected.LoginId = this.loginId;
			currentItemSelected.CompanyId = this.companyId;
			currentItemSelected.Login = this.userName;
			currentItemSelected.IsAdmin = CbAdmin.Checked;
			currentItemSelected.Disabled = CbDisable.Checked;
			currentItemSelected.EasyBuilderDeveloper = EBDevCheckBox.Checked;
			currentItemSelected.DbUser = oracleUser;
			currentItemSelected.DbPassword = oracleUserPwd;
			currentItemSelected.DbWindowsAuthentication = isNTUser;

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
		#endregion

		#region DetailOracleCompanyUser_Load - Caricamento della Form
		/// <summary>
		/// DetailOracleCompanyUser_Load
		/// </summary>
		//--------------------------------------------------------------------
		private void DetailOracleCompanyUser_Load(object sender, System.EventArgs e)
		{
			LabelTitle.Text						= string.Format(LabelTitle.Text, this.userName);
			TbLoginName.Text					= this.userName;
			TbIfDbowner.Visible					= false;
			CbChangeApplicationUser.Checked		= false;
			TbOracleUserPwd.Enabled				= CbChangeApplicationUser.Checked;
			rbSelectExistedOracleUser.Enabled	= false;
			rbNewOracleUser.Enabled				= false;
			BtnOracleConnectionCheck.Enabled	= !rbNewOracleUser.Checked;

			// l'evento posso spararlo solo nella Load, perche' nel costruttore non e' ancora stato 
			// agganciato e valorizzato!
			if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				isDMSActivated = true;

            if ((OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner)))
                isEasyBuilderActivated = true;

			LoadOracleCompanyData();
			LoadOracleCompanyUserData();

			ComboOracleLogins.Enabled	= CbChangeApplicationUser.Checked && rbSelectExistedOracleUser.Checked;
			txtNewOracleUser.Enabled	= CbChangeApplicationUser.Checked && rbNewOracleUser.Checked;
			CbNTSecurity.Enabled		= CbChangeApplicationUser.Checked && rbNewOracleUser.Checked && !ExistNTCurrentUser();

			State = StateEnums.View;
		}
		#endregion

		//--------------------------------------------------------------------
		private void LoadAssignedAndFreeOracleUsers()
		{
			//se avevo già un utente mi prendo le informazioni
			string currentUser = string.Empty;
			bool   currentAuth = false;
			if (ComboOracleLogins.SelectedItem != null)
			{
				currentUser = ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId;
				currentAuth = ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent;
			}

			//carico la combo
			ComboOracleLogins.DataSource = null;
			ComboOracleLogins.Items.Clear();
			CompanyUserDb companyUsers = new CompanyUserDb();
			companyUsers.ConnectionString = this.sysDbConnString;
			companyUsers.CurrentSqlConnection = this.sysDbConnection;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace								= "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp					+= new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole		+= new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole	+= new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole		+= new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateAdmin = oracleAccess.LoadSystemData(TextBoxOracleService.Text);
			OracleUserImpersonatedData oracleAdmin = oracleAccess.AdminImpersonification(candidateAdmin); 
			if (oracleAdmin == null)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.WrongUserCredential, candidateAdmin.Login));
				return;
			}
			else
			{ 
				ArrayList freeUsers = companyUsers.LoadFreeOracleUsersForAssociation(TextBoxOracleService.Text, this.databaseName, oracleAdmin);
				ArrayList usersFromSchema = companyUsers.LoadUsersFromOracleSchema(TextBoxOracleService.Text, companyDbName, oracleAdmin, oracleAccess);
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
				
				//se avevo selezionato un utente, mi riposiziono su di esso
				if (currentUser.Length > 0)
					SelectOracleUser(currentUser, currentAuth);
			}

			if (oracleAdmin != null)
				oracleAdmin.Undo();
		}

		#region LoadOracleCompanyData - Carico i dati relativi all'azienda
		/// <summary>
		/// LoadOracleCompanyData
		/// Carico i dati relativi all'azienda
		/// </summary>
		//--------------------------------------------------------------------
		private void LoadOracleCompanyData()
		{
			CompanyDb companyDb				= new CompanyDb();
			companyDb.ConnectionString		= sysDbConnString;
			companyDb.CurrentSqlConnection	= sysDbConnection;

			ArrayList companyData			= new ArrayList();
			companyDb.GetAllCompanyFieldsById(out companyData, this.companyId);
			if (companyData.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)companyData[0];
				TextBoxOracleService.Text = companyItem.DbServer;
				this.databaseName = companyItem.DbName;
				this.company      = companyItem.Company;
				companyName		  = companyItem.Company;
				companyDbName	  = companyItem.DbName;
				companyDbOwner    = companyItem.DbOwner;
				this.companyUseSlave = companyItem.UseDBSlave;
			}

			CompanyUserDb companyDbowner = new CompanyUserDb();
			companyDbowner.CurrentSqlConnection	= sysDbConnection;
			companyDbowner.ConnectionString		= sysDbConnString;
			bool disable = false;
			companyDbowner.SelectDataForUserCompany(companyDbOwner, companyId, out ownerIsAdmin, out ownerDbUser, out ownerDbPwd, out ownerDbWinAuth, out disable);
		}
		#endregion

		#region LoadOracleCompanyUserData - Carico i dati dell'utente associato selezionato
		/// <summary>
		/// LoadOracleCompanyUserData
		/// Carico i dati dell'utente associato selezionato
		/// </summary>
		//--------------------------------------------------------------------
		private void LoadOracleCompanyUserData()
		{
			bool loginDisabled = false;

			UserDb userDb = new UserDb();
			userDb.ConnectionString = sysDbConnString;
			userDb.CurrentSqlConnection = sysDbConnection;
			ArrayList loginData = new ArrayList();
			userDb.GetAllUserFieldsById(out loginData, this.loginId);
			if (loginData.Count > 0)
			{
				UserItem infoLogin = (UserItem)loginData[0];
				loginDisabled = infoLogin.Disabled;
			}
			
			CompanyUserDb companyUserDb			= new CompanyUserDb();
			companyUserDb.ConnectionString		= sysDbConnString;
			companyUserDb.CurrentSqlConnection	= sysDbConnection;
			
			ArrayList companyUser				= new ArrayList();
			companyUserDb.GetUserCompany(out companyUser, this.loginId, this.companyId);
			if (companyUser.Count > 0)
			{
				CompanyUser companyUserItem = (CompanyUser)companyUser[0];
				
				//leggo l'id
				string loginId		= companyUserItem.LoginId;
				string loginName	= companyUserItem.Login;
				
				//leggo i valori e imposto le checkbox
				CbAdmin.Checked		= companyUserItem.Admin;
				CbDisable.Checked	= companyUserItem.Disabled;
				CbDisable.Enabled   = !loginDisabled;

				// la checkbox di EasyBuilder e' visibile solo se e' attivato
				EBDevCheckBox.Checked = companyUserItem.EasyBuilderDeveloper;
				EBDevCheckBox.Visible = isEasyBuilderActivated;
				//se la login è NT può essere associata a un utente di database NT altrimenti no
				isNTLogin	= companyUserItem.WindowsAuthentication;
				isNTUser	= companyUserItem.DBWindowsAuthentication;

				TbOracleUserPwd.Text = isNTUser ? string.Empty : companyUserItem.DBDefaultPassword;
				TbOracleUserPwd.Enabled = isNTUser ? false : CbChangeApplicationUser.Checked;

				SelectOracleUser(companyUserItem.DBDefaultUser, companyUserItem.DBWindowsAuthentication);
				TbIfDbowner.Visible = false;

				// verifico se l'utente e' dbowner
				userIsDbo = companyUserDb.IsDbo(this.loginId, this.companyId);

				if (userIsDbo)
				{
					CbAdmin.Enabled						= false;
					CbDisable.Enabled					= false;
					LblServiceOracle.Enabled			= false;
					TextBoxOracleService.Enabled        = false;
					CbChangeApplicationUser.Enabled		= false;
					rbNewOracleUser.Enabled				= false;
					rbSelectExistedOracleUser.Enabled	= false;
					txtNewOracleUser.Enabled			= false;
					CbNTSecurity.Enabled				= false;
					ComboOracleLogins.Enabled			= false;
					LblPassword.Enabled					= false;
					TbOracleUserPwd.Enabled				= false;
					BtnModify.Enabled					= false;
					TbIfDbowner.Visible					= true;
					BtnOracleConnectionCheck.Enabled	= true;
				}

				// se EasyBuilder e' attivato allora abilito il solo pulsante di salvataggio
				if (isEasyBuilderActivated)
					this.BtnModify.Enabled = true;
			}
		}
		#endregion

		//--------------------------------------------------------------------
		private void SelectOracleUser(string userToSelect, bool authenticationType)
		{
			if (ComboOracleLogins.Items.Count == 0)
			{
				ComboOracleLogins.Items.Add(new OracleUser(userToSelect,authenticationType));
				ComboOracleLogins.DisplayMember = "OracleUserId";
				ComboOracleLogins.ValueMember	= "OracleUserOSAuthent";
				ComboOracleLogins.SelectedIndex = 0;
			}
			else
			{
				for (int i = 0; i < ComboOracleLogins.Items.Count; i++)
				{
					OracleUser current = (OracleUser)ComboOracleLogins.Items[i];
					if (string.Compare(current.OracleUserId, userToSelect.ToUpper(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture) == 0)
					{
						ComboOracleLogins.SelectedIndex = i;
						break;
					}
				}
			}
			if (ComboOracleLogins.SelectedIndex != -1)
			{
				if (CbChangeApplicationUser.Checked)
				{
					TbOracleUserPwd.Enabled		= (CbChangeApplicationUser.Checked) ? !((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent : false;
					ComboOracleLogins.Enabled	= CbChangeApplicationUser.Checked;
				}
			}
			else
			{
				ComboOracleLogins.Enabled	= false;
				TbOracleUserPwd.Enabled     = false;	
			}
		}

		#region BtnModify_Click  - Premuto il bottone di Modifica
		/// <summary>
		/// BtnModify_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void BtnModify_Click(object sender, System.EventArgs e)
		{
			if (ModifyCompanyUser())
				State = StateEnums.View;
			else
			{
				State = StateEnums.Editing;
				if (diagnostic.Error || diagnostic.Warning)
					DiagnosticViewer.ShowDiagnostic(diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotAssociateUserToCompany, TbLoginName.Text, companyName, companyDbName));

				//non prendo le info (connessione avvenuta con successo ma metto in console solo gli errori/warnings)
				//questo perchè mi ritroverei un sacco di "connessioni con successo" senza importanza (qui)
				if (diagnostic.Error || diagnostic.Warning)
				{
					if (OnSendDiagnostic != null)
						OnSendDiagnostic(sender, diagnostic);
					diagnostic.Clear();
				}
			}
		}
		#endregion

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
			
			OracleUserImpersonatedData candidateAdmin	= oracleAccess.LoadSystemData(oracleService);
			OracleUserImpersonatedData oracleAdmin		= oracleAccess.AdminImpersonification(candidateAdmin); 
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

		//--------------------------------------------------------------------
		private bool CreateSynonyms(string schemaName, string userName, string userPassword, bool userOSAuthent)
		{
			//se non ho cambiato le credenziali, non faccio nulla a livello di sinonimi
			if (!CbChangeApplicationUser.Checked) 
				return true;
			
			bool result = false;
			
			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateAdmin	= oracleAccess.LoadSystemData(TextBoxOracleService.Text);
			OracleUserImpersonatedData oracleAdmin		= oracleAccess.AdminImpersonification(candidateAdmin);

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
		public bool DropSynonyms(string schemaName, string serviceName, string userName, string userPassword, bool userOSAuthent)
		{
			bool result = false;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateAdmin	= oracleAccess.LoadSystemData(serviceName);
			OracleUserImpersonatedData oracleAdmin		= oracleAccess.AdminImpersonification(candidateAdmin);

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

		#region BtnOracleConnectionCheck_Click - Test della Connection ad Oracle
		/// <summary>
		/// BtnOracleConnectionCheck_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void BtnOracleConnectionCheck_Click(object sender, System.EventArgs e)
		{
			if (CheckUserData())
				CheckOracleConnection(false);
		}
		#endregion

		#region CheckOracleConnection - Esegue il check
		/// <summary>
		/// CheckOracleConnection
		/// </summary>
		/// <param name="isSilent"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------
		private bool CheckOracleConnection(bool isSilent)
		{
			diagnostic.Clear();
			
			OracleUserImpersonatedData oracleUser = null;
			string  oracleUserName = string.Empty;
			bool isUserNt = false;
			
			if (this.rbSelectExistedOracleUser.Checked)
			{
				oracleUserName	= ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId;
				isUserNt		= ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent;
			}
			else if (this.rbNewOracleUser.Checked)
			{
				oracleUserName	= txtNewOracleUser.Text;
				isUserNt		= CbNTSecurity.Checked;
			}
			
			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateUser = new OracleUserImpersonatedData();
			candidateUser.Login	= oracleUserName;
			candidateUser.WindowsAuthentication = isUserNt;
			candidateUser.OracleService = TextBoxOracleService.Text;
			candidateUser.IsDba = false;
			if (isUserNt)
			{
				candidateUser.Password	= string.Empty;
				candidateUser.Domain	= oracleUserName.Split(Path.DirectorySeparatorChar)[0];
				//non può cambiare le informazioni in finestra
				oracleUser = oracleAccess.UserImpersonification(candidateUser, true, false); 
			}
			else
			{
				candidateUser.Password = TbOracleUserPwd.Text;
				candidateUser.Domain	= string.Empty;
				//silente
				oracleUser = oracleAccess.UserImpersonification(candidateUser); 
			}

			bool successConnection = false;

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
			
			successConnection = !oracleAccess.Diagnostic.Error;
			if (isSilent)
			{
				//silente - mostro solo gli errori
				if (oracleAccess.Diagnostic.Error || oracleAccess.Diagnostic.Warning)
					diagnostic.Set(oracleAccess.Diagnostic);
				else if (!successConnection) 
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, Strings.FailedOracleConnectionDetail);
					extendedInfo.Add(Strings.OracleProvider,   (TextBoxOracleService.Text.Length > 0) ? TextBoxOracleService.Text : Strings.Undefined);
					extendedInfo.Add(Strings.OracleUserId,		(oracleUserName.Length > 0) ? oracleUserName : Strings.Undefined);
					extendedInfo.Add(Strings.OraclePassword,	(TbOracleUserPwd.Text.Length  > 0) ? TbOracleUserPwd.Text : Strings.Undefined);
					diagnostic.Set(DiagnosticType.Error,  string.Format(Strings.FailedOracleConnection, company), extendedInfo);
				}
			}
			else
			{
				if (oracleAccess.Diagnostic.Error || oracleAccess.Diagnostic.Warning || oracleAccess.Diagnostic.Information)
					diagnostic.Set(oracleAccess.Diagnostic);
				else if (!successConnection) 
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, Strings.FailedOracleConnectionDetail);
					extendedInfo.Add(Strings.OracleProvider,   (TextBoxOracleService.Text.Length > 0) ? TextBoxOracleService.Text : Strings.Undefined);
					extendedInfo.Add(Strings.OracleUserId,		(oracleUserName.Length > 0) ? oracleUserName : Strings.Undefined);
					extendedInfo.Add(Strings.OraclePassword,	(TbOracleUserPwd.Text.Length  > 0) ? TbOracleUserPwd.Text : Strings.Undefined);
					diagnostic.Set(DiagnosticType.Error,  string.Format(Strings.FailedOracleConnection, company), extendedInfo);
					
				}
				if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
				{
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					diagnostic.Clear();
				}
			}
			if (isUserNt)
			{
				if (oracleUser != null)
					oracleUser.Undo(); 
			}
			return successConnection;
		}
		#endregion

		#region CheckUserData - Verifica dei dati introdotti dall'utente
		/// <summary>
		/// CheckUserData
		/// </summary>
		/// <returns></returns>
		//--------------------------------------------------------------------
		private bool CheckUserData()
		{
			bool result = true;
			
			diagnostic.Clear();
			
			//se non ho selezionato nessuna login errore
			if (ComboOracleLogins.SelectedIndex == -1)
			{
				diagnostic.Set(DiagnosticType.Error,Strings.NotSelectedCompanyUsers);
				result = false;
			}
			if (CbChangeApplicationUser.Checked)
			{
				if ((ComboOracleLogins.SelectedItem == null) || ( ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId.Length == 0))
				{
					diagnostic.Set(DiagnosticType.Error, Strings.NotSelectedCompanyUsers);
					result = false;
				}
				if ((ComboOracleLogins.SelectedItem == null) || ! ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent && this.TbOracleUserPwd.Text.Length == 0)
				{
					diagnostic.Set(DiagnosticType.Error, Strings.EmptyOracleOwnerPwd);
					result = false;
				}
				if (rbNewOracleUser.Checked)
				{
					
					if (txtNewOracleUser.Text.Length == 0)
					{
						diagnostic.Set(DiagnosticType.Error, Strings.EmptyOracleOwnerName);
						result = false;
					}
					else if (txtNewOracleUser.Text.Length != 0)
					{
						if (!newOracleUserIsWinNT)
						{
							if (txtNewOracleUser.Text.CountChar(Path.DirectorySeparatorChar) != 0)
							{
								diagnostic.Set(DiagnosticType.Error, Strings.ForbiddedNtUser);
								result = false;
							}
						}

						if (txtNewOracleUser.Text.Trim().IndexOfAny(new char[] {'?', '*', '"', '$', '&', '/', '(', ')', '=', '[', '#', ']', '<', '>', ':', '!', '|'}) != -1)
						{
							diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInOracleOwnerName);
							result = false;
						}
					}
				}
			}
			//se ci sono errori, mostro la finestra degli errori
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
		
		#region CheckedChanged 
		//--------------------------------------------------------------------
		private void CbDisable_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void EBDevCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//--------------------------------------------------------------------
		private void CbAdmin_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}
		#endregion

		#region TbOracleUserName_TextChanged - Check sull'utente NT
		/// <summary>
		/// TbOracleUserName_TextChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void TbOracleUserName_TextChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
			//se ho imputato una login nt e l'utente che sto modificando è non NT errore
			if (((TextBox)sender).Text.Split(Path.DirectorySeparatorChar).Length > 1 && !isNTLogin)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.ForbiddedNtUser);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
				diagnostic.Clear();
				TbOracleUserPwd.Enabled = CbChangeApplicationUser.Checked;
			}
			//sto imputando un utente NT (e lo posso fare) devo disabilitare la pwd
			//l'utente che voglio associare (isNTUser) è anch'esso NT
			else if ( ((TextBox)sender).Text.Split(Path.DirectorySeparatorChar).Length > 1 )
			{
				isNTUser = true;
				TbOracleUserPwd.Text = string.Empty;
				TbOracleUserPwd.Enabled = false;
			}
			//la pwd è abilitata in ogni altro caso, e l'utente è un utente non NT
			else
			{
				isNTUser = false;
				TbOracleUserPwd.Enabled = CbChangeApplicationUser.Checked;
			}
		}
		#endregion

		#region TbOracleUserPwd_TextChanged 
		/// <summary>
		/// TbOracleUserPwd_TextChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void TbOracleUserPwd_TextChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}
		#endregion

		#region CbChangeApplicationUser_CheckedChanged - Abilito /Disabilito modifica User,Pwd
		/// <summary>
		/// CbChangeApplicationUser_CheckedChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void CbChangeApplicationUser_CheckedChanged(object sender, System.EventArgs e)
		{
			if (CbChangeApplicationUser.Checked)
			{
				rbSelectExistedOracleUser.Enabled	= CbChangeApplicationUser.Checked;
				rbNewOracleUser.Enabled				= CbChangeApplicationUser.Checked;
				ComboOracleLogins.Enabled			= CbChangeApplicationUser.Checked && rbSelectExistedOracleUser.Checked;
				txtNewOracleUser.Enabled			= CbChangeApplicationUser.Checked && rbNewOracleUser.Checked;
				txtNewOracleUser.Text				= string.Empty;
				CbNTSecurity.Enabled				= CbChangeApplicationUser.Checked && rbNewOracleUser.Checked && !ExistNTCurrentUser();
				bool testNTUser						= (rbNewOracleUser.Checked) ? CbNTSecurity.Checked : isNTUser;
				TbOracleUserPwd.Enabled				= (CbChangeApplicationUser.Checked) ? !testNTUser : CbChangeApplicationUser.Checked;	
			}
			else
			{
				rbSelectExistedOracleUser.Enabled	= false;
				rbNewOracleUser.Enabled				= false;
				txtNewOracleUser.Enabled			= false;
				ComboOracleLogins.Enabled			= false;
				TbOracleUserPwd.Enabled				= false;
				CbNTSecurity.Enabled				= false;
			}
			BtnOracleConnectionCheck.Enabled = !rbNewOracleUser.Checked;
		}
		#endregion

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
			candidateAdmin.Password = string.Empty;
			candidateAdmin.OracleService = TextBoxOracleService.Text;
			candidateAdmin.IsDba = true;
			candidateAdmin.Domain = string.Empty;
			OracleUserImpersonatedData oracleAdmin = oracleAccess.AdminImpersonification(candidateAdmin); 
			//oracleAccess.AdminImpersonification(oracleService);
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
					existUser  = oracleAccess.ExistUserIntoDatabase(WindowsIdentity.GetCurrent().Name.ToUpper(CultureInfo.InvariantCulture), true);
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

		#region RbSelectExistLogin_CheckedChanged
		/// <summary>
		/// RbSelectExistLogin_CheckedChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void RbSelectExistLogin_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				ComboOracleLogins.Enabled	= true;
				if (ComboOracleLogins.SelectedItem != null)
				{
					if (((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent)
					{
						TbOracleUserPwd.Text    = string.Empty;
						TbOracleUserPwd.Enabled = false;
					}
					else
						TbOracleUserPwd.Enabled = true;
				}
			}
		}
		#endregion

		#region RbNewLoginName_CheckedChanged
		/// <summary>
		/// RbNewLoginName_CheckedChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void RbNewLoginName_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				ComboOracleLogins.Enabled	= false;
				TbOracleUserPwd.Enabled		= true;
			}
		}
		#endregion
		
		#region ComboOracleLogins_SelectedIndexChanged
		/// <summary>
		/// ComboOracleLogins_SelectedIndexChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void ComboOracleLogins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (ComboOracleLogins.SelectedItem != null)
			{
				OracleUser current = (OracleUser)ComboOracleLogins.SelectedItem;
				if (current.OracleUserOSAuthent && CbChangeApplicationUser.Checked)
				{
					//se ho selezionato un utente NT non devo poter digitare la pwd
					TbOracleUserPwd.Text	= string.Empty;
					TbOracleUserPwd.Enabled	= false;
				}
				else
					TbOracleUserPwd.Enabled	= CbChangeApplicationUser.Checked;
			}
		}
		#endregion

		#region Funzioni per l'invio di Diagnostica alla Console
		/// <summary>
		/// DetailOracleCompanyUser_VisibleChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void DetailOracleCompanyUser_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}

		/// <summary>
		/// DetailOracleCompanyUser_Closing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void DetailOracleCompanyUser_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// DetailOracleCompanyUser_Deactivate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void DetailOracleCompanyUser_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
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

		#region ComboOracleLogins_DropDown - Caricamento della combo degli utenti Oracle
		/// <summary>
		/// ComboOracleLogins_DropDown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------
		private void ComboOracleLogins_DropDown(object sender, System.EventArgs e)
		{
			if (ComboOracleLogins.Items.Count <= 1)
			{
				LoadAssignedAndFreeOracleUsers();
				//pulisco la pwd
				TbOracleUserPwd.Text = string.Empty;
			}
		}
		#endregion

		//--------------------------------------------------------------------
		private void CbNTSecurity_CheckStateChanged(object sender, System.EventArgs e)
		{
			ComboOracleLogins.Enabled	= CbChangeApplicationUser.Checked;
			TbOracleUserPwd.Text		= string.Empty;
		}

		//---------------------------------------------------------------------
		private void SendHelp(object sender, string nameSpace, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, nameSpace, searchParameter);
		}

		#region Funzioni relative alla ProgressBar

		#region EnableConsoleProgressBar - Dice alla MC di abilitare la ProgressBar
		/// <summary>
		/// EnableConsoleProgressBar
		/// Dice alla MC di abilitare la ProgressBar
		/// </summary>
		/// <param name="sender"></param>
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
		/// <param name="sender"></param>
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
		/// Dice alla MC di impostare lo Step della ProgressBar al valore di step
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="step"></param>
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
		/// <param name="sender"></param>
		/// <param name="currentValue"></param>
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
		/// <param name="sender"></param>
		/// <param name="message"></param>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarText(object sender, string message)
		{
			if (OnSetProgressBarText != null)
				OnSetProgressBarText(sender, message);
		}
		#endregion

		#endregion

		//---------------------------------------------------------------------
		private void rbSelectExistedOracleUser_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rbSelectExistedOracleUser.Checked)
			{
				ComboOracleLogins.Enabled		= true;
				txtNewOracleUser.Enabled		= false;
				txtNewOracleUser.Text			= string.Empty;
				TbOracleUserPwd.Text			= string.Empty;
				isNewOracleUser					= false;
				newOracleUserIsWinNT			= false;
				CbNTSecurity.Enabled			= false;
				CbNTSecurity.Checked			= false;
				//abilito il bottone di verifica della connessione
				BtnOracleConnectionCheck.Enabled = true;
			}
		}

		//---------------------------------------------------------------------
		private void rbNewOracleUser_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rbNewOracleUser.Checked)
			{
				ComboOracleLogins.Enabled		= false;
				txtNewOracleUser.Enabled		= true;
				CbNTSecurity.Enabled			= !ExistNTCurrentUser();
				isNewOracleUser					= true;
				TbOracleUserPwd.Text			= string.Empty;
				//disabilito il bottone di check
				BtnOracleConnectionCheck.Enabled = false;
			}
		}

		//---------------------------------------------------------------------
		private void CbNTSecurity_CheckedChanged(object sender, System.EventArgs e)
		{
			if (CbNTSecurity.Checked)
			{
				txtNewOracleUser.Text		= WindowsIdentity.GetCurrent().Name.ToUpper(CultureInfo.InvariantCulture);
				TbOracleUserPwd.Text		= string.Empty;
				TbOracleUserPwd.Enabled		= false;
				txtNewOracleUser.Enabled	= false;
				newOracleUserIsWinNT		= true;
			}
			else
			{
				txtNewOracleUser.Text		= string.Empty;
				TbOracleUserPwd.Text		= string.Empty;
				TbOracleUserPwd.Enabled		= true;
				txtNewOracleUser.Enabled	= true;
				newOracleUserIsWinNT		= false;
			}
		}

		//---------------------------------------------------------------------
		private void txtNewOracleUser_Leave(object sender, System.EventArgs e)
		{
			txtNewOracleUser.Text	= txtNewOracleUser.Text.ToUpper(CultureInfo.InvariantCulture);
			isNewOracleUser			= true;
			newOracleUserIsWinNT	= (CbNTSecurity.Checked);
		}

		# region Gestione documentale
		///<summary>
		/// ConnectToDmsDatabase
		/// Metodo che si occupa di effettuare una connessione al database documentale con le credenziali di amministrazione.
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
			dmsConnSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(SendHelp);

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
				this.dmsDbOwnerDomain = Path.GetDirectoryName(dmsDbOwnerLogin);
			}
			else
				dmsConnectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, dmsServerName, dmsDatabaseName, dmsDbOwnerLogin, dmsDbOwnerPassword);

			return dmsConnectionString;
		}

		///<summary>
		/// CheckHasRoleSysAdmin
		/// Metodo che mi consente di controllare se l'utente dbowner del database documentale ha le
		/// permission per associare la login generica al database
		///</summary>
		//----------------------------------------------------------------------
		private bool CheckHasRoleSysAdmin(string newLogin)
		{
			bool result = false;

			// controllo se la login ha il ruolo sysadmin
			if (!dmsConnSqlTransact.LoginIsSystemAdminRole(newLogin, DatabaseLayerConsts.RoleSysAdmin))
			{
				string userCompleteName = (string.IsNullOrWhiteSpace(dmsImpersonated.Domain))
					? dmsImpersonated.Login
					: Path.Combine(dmsImpersonated.Domain, dmsImpersonated.Login);

				DialogResult askIfSetPermissions = MessageBox.Show
					(
						this,
						string.Format(Strings.NoPermissionUser, userCompleteName),
						Strings.Error,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Error
					);

				if (askIfSetPermissions == DialogResult.No)
				{
					if (dmsImpersonated != null)
						dmsImpersonated.Undo();
					return result;
				}

				string oldString = dmsConnSqlTransact.CurrentStringConnection;

				//devo settare le permission, quindi chiedo nuove credenziali utente (di default propongo sa)
				UserImpersonatedData oldDataToConnectionServer = dmsImpersonated;
				dmsImpersonated = dmsConnSqlTransact.LoginImpersonification
					(
					DatabaseLayerConsts.LoginSa,
					string.Empty,
					string.Empty,
					false, //typeAuthenticationLogin,
					dmsDbOwnerPrimary,
					dmsDbOwnerInstance,
					true
					);
				//se l'impersonificazione non è andata a buon fine ritorno
				if (dmsImpersonated == null)
				{
					Cursor.Current = Cursors.Default;
					return result;
				}

				dmsConnSqlTransact.CurrentStringConnection =
					(dmsImpersonated.WindowsAuthentication)
					? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, dmsServerName, DatabaseLayerConsts.MasterDatabase)
					: string.Format(NameSolverDatabaseStrings.SQLConnection, dmsServerName, DatabaseLayerConsts.MasterDatabase, dmsImpersonated.Login, dmsImpersonated.Password);

				if (!dmsConnSqlTransact.SPAddSrvRoleMember(newLogin, DatabaseLayerConsts.RoleSysAdmin, string.Empty))
				{
					if (dmsConnSqlTransact.Diagnostic.Error || dmsConnSqlTransact.Diagnostic.Warning)
					{
						diagnostic.Set(dmsConnSqlTransact.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}
					}
					if (dmsImpersonated != null)
						dmsImpersonated.Undo();
					dmsConnSqlTransact.CurrentStringConnection = oldString;
					dmsImpersonated = oldDataToConnectionServer;
					return result;
				}

				dmsConnSqlTransact.CurrentStringConnection = oldString;
				dmsImpersonated = oldDataToConnectionServer;
				result = true;
			}
			else
				result = true;

			return result;
		}

		///<summary>
		/// GrantLogin
		/// Metodo richiamato per ogni utente selezionato e che aggiunge/granta la login sul server SQL
        /// Abbiamo deciso che per l'associazione utenti applicativi di un database Easy Attachment agganciato ad un'azienda Oracle 
		/// creiamo sempre e solo una login "DmsOraUser" (con pw= resUarOmsD.) e poi assegniamo gli utenti 1 a n.
		///</summary>
		//----------------------------------------------------------------------
		private bool GrantLogin()
		{
			bool result = false;
			bool dmsOraWinAuth = false; //  la login la inseriamo in SQL Authentication

			//se la login non esiste la devo prima creare
			if (!dmsConnSqlTransact.ExistLogin(DatabaseLayerConsts.DmsOraUser))
			{
				// al momento assegniamo una login in SQL Authentication, pertanto non e' previsto avere un server SQL 
				// configurato con la sola sicurezza integrata
				if (dmsOraWinAuth)
				{
					result =
						dmsConnSqlTransact.SPGrantLogin(DatabaseLayerConsts.DmsOraUser) &&
						dmsConnSqlTransact.SPGrantDbAccess(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.DmsOraUser, dmsDatabaseName) &&
						dmsConnSqlTransact.SPAddRoleMember(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.RoleDataWriter, dmsDatabaseName) &&
						dmsConnSqlTransact.SPAddRoleMember(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.RoleDataReader, dmsDatabaseName) &&
						dmsConnSqlTransact.SPAddRoleMember(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.RoleDbOwner, dmsDatabaseName);
				}
				else
				{
					result =
						dmsConnSqlTransact.SPAddLogin(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.DmsOraUserPw, DatabaseLayerConsts.MasterDatabase) &&
						dmsConnSqlTransact.SPGrantDbAccess(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.DmsOraUser, dmsDatabaseName) &&
						dmsConnSqlTransact.SPAddRoleMember(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.RoleDataWriter, dmsDatabaseName) &&
						dmsConnSqlTransact.SPAddRoleMember(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.RoleDataReader, dmsDatabaseName) &&
						dmsConnSqlTransact.SPAddRoleMember(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.RoleDbOwner, dmsDatabaseName);
				}

				//se ci sono errori interrompo
				if (!result)
				{
					diagnostic.Set(dmsConnSqlTransact.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					State = StateEnums.Editing;
					Cursor.Current = Cursors.Default;
				}
			}
			else
			{
				//se invece la login esiste controllo che non sia già associato come User al Db
				if (!dmsConnSqlTransact.ExistUserIntoDb(DatabaseLayerConsts.DmsOraUser, dmsDatabaseName))
				{
					result =
						dmsConnSqlTransact.SPGrantDbAccess(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.DmsOraUser, dmsDatabaseName) &&
						dmsConnSqlTransact.SPAddRoleMember(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.RoleDataWriter, dmsDatabaseName) &&
						dmsConnSqlTransact.SPAddRoleMember(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.RoleDataReader, dmsDatabaseName) &&
						dmsConnSqlTransact.SPAddRoleMember(DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.RoleDbOwner, dmsDatabaseName);

					if (!result)
					{
						diagnostic.Set(dmsConnSqlTransact.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						State = StateEnums.Editing;
						Cursor.Current = Cursors.Default;
					}
				}
				else
					//@@TODO: secondo me qui sarebbe il caso fare una tryconnect con la pw. se fallisce non procedo
					result = true;
			}

			return result;
		}
		# endregion
	}
}