using System;
using System.Collections;
using System.ComponentModel;
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
using Microarea.TaskBuilderNet.Data.SQLDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// AddCompanyUserToOracleLogin.
	/// Aggiungo gruppi di utenti al database aziendale Oracle
	/// </summary>
	//=========================================================================
	public partial class AddCompanyUserToOracleLogin : PlugInsForm
	{
		#region Variabili
		private Diagnostic diagnostic = new Diagnostic("SysAdmin.AddCompanyUserToOracleLogin");

		private Container components = null;
		private string selectedApplicationUserName = string.Empty;
		private string selectedApplicationUserId = string.Empty;
		private string selectedApplicationUserPwd = string.Empty;

		private SqlConnection sysDbConnection;
		private string sysDbConnString;

		private string companyName = string.Empty;
		private string companyDbName = string.Empty;
		private string companyId = string.Empty;
		private string companyDbOwner = string.Empty;
		private bool companyUseSlave = false;
		private bool ownerIsAdmin = false;
		private string ownerDbUser = string.Empty;
		private string ownerDbPwd = string.Empty;
		private bool ownerDbWinAuth = false;

		private bool isNewOracleUser = false;
		private bool newOracleUserIsWinNT = false;

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

		#region Eventi e Delegati
		public delegate void ModifyTreeOfCompanies(object sender, string nodeType, string companyId);
		public event ModifyTreeOfCompanies OnModifyTreeOfCompanies;
		
		public delegate void SaveUsers(object sender, string id, string companyId);
		public event SaveUsers OnSaveUsers;
		
		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;
		
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
		
		public delegate void CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event CallHelpFromPopUp OnCallHelpFromPopUp;

		public delegate bool IsActivated(string application, string functionality);
		public event IsActivated OnIsActivated;
		
		//---------------------------------------------------------------------
		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;
		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;
		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;
		#endregion

		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public AddCompanyUserToOracleLogin(string connectionString, SqlConnection currentConnection, string companyId)
		{
			InitializeComponent();

			this.companyId = companyId;
			this.sysDbConnection = currentConnection;
			this.sysDbConnString = connectionString;
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
			ListViewUsersCompany.View = View.Details;
			ListViewUsersCompany.CheckBoxes = true;
			ListViewUsersCompany.AllowColumnReorder = true;
			ListViewUsersCompany.Activation = ItemActivation.OneClick;
			ListViewUsersCompany.Columns.Add(Strings.User, 170, HorizontalAlignment.Left);
			ListViewUsersCompany.Columns.Add(Strings.Description, 200, HorizontalAlignment.Left);
		}
		#endregion

		//--------------------------------------------------------------------
		private void LoadAssignedAndFreeOracleUsers()
		{
			//se avevo già un utente mi prendo le informazioni
			string currentUser = string.Empty;
			bool currentAuth = false;
			if (ComboOracleLogins.SelectedItem != null)
			{
				currentUser = ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId;
				currentAuth = ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent;
			}

			ComboOracleLogins.DataSource = null;
			ComboOracleLogins.Items.Clear();
			CompanyUserDb companyUsers = new CompanyUserDb();
			companyUsers.ConnectionString = this.sysDbConnString;
			companyUsers.CurrentSqlConnection = this.sysDbConnection;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateAdmin = oracleAccess.LoadSystemData(TextBoxOracleService.Text);
			OracleUserImpersonatedData oracleAdmin = oracleAccess.AdminImpersonification(candidateAdmin);
			if (oracleAdmin == null)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.WrongUserCredential, candidateAdmin.Login));
				return;
			}
			else
			{
				ArrayList freeUsers = companyUsers.LoadFreeOracleUsersForAssociation(TextBoxOracleService.Text, companyDbName, oracleAdmin);
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
					ComboOracleLogins.DataSource = freeUsers;
					ComboOracleLogins.DisplayMember = "OracleUserId";
					ComboOracleLogins.ValueMember = "OracleUserOSAuthent";
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

		//--------------------------------------------------------------------
		private void SelectOracleUser(string userToSelect, bool authenticationType)
		{
			if (ComboOracleLogins.Items.Count == 0)
			{
				ComboOracleLogins.Items.Add(new OracleUser(userToSelect, authenticationType));
				ComboOracleLogins.DisplayMember = "OracleUserId";
				ComboOracleLogins.ValueMember = "OracleUserOSAuthent";
				ComboOracleLogins.SelectedIndex = 0;
			}
			else
			{
				for (int i = 0; i < ComboOracleLogins.Items.Count; i++)
				{
					OracleUser current = (OracleUser)ComboOracleLogins.Items[i];
					if (string.Compare(current.OracleUserId, userToSelect.ToUpper(CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						ComboOracleLogins.SelectedIndex = i;
						break;
					}
				}
			}
			if (ComboOracleLogins.SelectedIndex != -1)
			{
				TextBoxOracleUserPwd.Enabled = !((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent;
				ComboOracleLogins.Enabled = true;
			}
			else
			{
				ComboOracleLogins.Enabled = false;
				TextBoxOracleUserPwd.Enabled = false;
			}
		}

		#region LoadOnlyOracleFreeUsers - Popolamento della lista nel caso di azienda Oracle
		/// <summary>
		/// LoadOnlyOracleFreeUsers
		/// Nel caso di azienda Oracle, la lista degli utenti disponibili all'associazione
		/// deve provedere solamente:
		/// - utente non dbowner
		/// - utente non già associato a qualche altra azienda Oracle (vincolo
		/// dato da Oracle stesso)
		/// </summary>
		//----------------------------------------------------------------------
		private void LoadOnlyOracleFreeUsers()
		{
			UserDb userDb = new UserDb();
			userDb.ConnectionString = sysDbConnString;
			userDb.CurrentSqlConnection = sysDbConnection;

			ArrayList users = new ArrayList();

			//seleziono tutti gli utenti applicativi disponibili 
			if (!userDb.SelectAllUsers(out users, false))
			{
				if (userDb.Diagnostic.Error || userDb.Diagnostic.Information || userDb.Diagnostic.Warning)
					diagnostic.Set(userDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(this, diagnostic);
				diagnostic.Clear();
				users.Clear();
			}

			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = sysDbConnString;
			companyUserDb.CurrentSqlConnection = sysDbConnection;

			for (int i = 0; i < users.Count; i++)
			{
				UserItem itemUser = (UserItem)users[i];

				if (string.Compare(itemUser.Login, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) != 0 &&
					string.Compare(itemUser.Login, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					if (companyUserDb.ExistUser(itemUser.LoginId, this.companyId) != 0)
						continue;
				}
				else
					continue;

				UserListItem listItemUser = new UserListItem();
				listItemUser.IsModified = false;
				listItemUser.CompanyId = companyId;
				listItemUser.LoginId = itemUser.LoginId;
				listItemUser.Login = itemUser.Login;
				listItemUser.Description = itemUser.Description.Replace("\r\n", " ");
				listItemUser.DbPassword = itemUser.Password;
				listItemUser.DbWindowsAuthentication = itemUser.WindowsAuthentication;
				listItemUser.ImageIndex =
					(itemUser.WindowsAuthentication) ? PlugInTreeNode.GetLoginsDefaultImageIndex : PlugInTreeNode.GetUserDefaultImageIndex;
				listItemUser.Text = itemUser.Login;

				if (itemUser.Disabled)
				{
					listItemUser.Disabled = true;
					listItemUser.ForeColor = Color.Red;
				}
				else
					listItemUser.Disabled = false;

				listItemUser.SubItems.Add(listItemUser.Description.Replace("\r\n", " "));
				ListViewUsersCompany.Items.Add(listItemUser);
			}

			if (ListViewUsersCompany.Items.Count == 0)
			{
				LblAllUsersJoined.Visible = true;
				TextBoxOracleUserPwd.Enabled = false;
				BtnOracleConnectionCheck.Enabled = false;
				BtnSave.Enabled = false;
				BtnSelectAll.Enabled = false;
				BtnUnselectAll.Enabled = false;
			}
			State = StateEnums.View;
		}
		#endregion

		#region Metodi Pubblici

		#region Save - Salva e crea le associazioni Utenti Applicativi - Azienda
		/// <summary>
		/// Save
		/// </summary>
		//---------------------------------------------------------------------
		public bool Save(object sender, System.EventArgs e)
		{
			// Ricavo il corrente cursore della form della console e lo salvo
			// per poterlo poi riassegnare in seguito, una volta terminata l'elaborazione
			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;

			bool updatingAll = false;
			string loginSelected = string.Empty;
			string loginPassword = string.Empty;
			bool isLoginNT = false;

			bool dmsToManage = false;

			if (CheckUserData(true) && CheckOracleConnection(true))
			{
				// se il modulo dms e' attivato e l'azienda ha uno slave associato procedo con i controlli sul database
				if (isDMSActivated && companyUseSlave)
				{
					// se non sono riuscita a connettermi al database documentale non procedo
					if (!ConnectToDmsDatabase())
						return false;
					
					// adesso devo controllare che l'utente abbia i permessi di amministrazione, per 
					if (!CheckHasRoleSysAdmin(dmsDbOwnerLogin))
						return false;

					dmsToManage = true;
				}

				currentConsoleFormCursor = this.TopLevelControl.Cursor;
				//setto il cursore
				this.TopLevelControl.Cursor = Cursors.WaitCursor;
				Cursor.Current = Cursors.WaitCursor;
				//Setto il testo nello statusPanel
				SetConsoleProgressBarText(sender, Strings.ProgressWaiting);
				if (rbSelectExistedOracleUser.Checked)
				{
					loginSelected = ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId;
					isLoginNT = ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent;
				}
				else
				{
					loginSelected = txtNewOracleUser.Text;
					isLoginNT = newOracleUserIsWinNT;
				}
				//se è una login non NT prendo la pwd
				loginPassword = (isLoginNT) ? string.Empty : TextBoxOracleUserPwd.Text;

				//aggiungo l'utente
				bool insertUser = true;
				if (isNewOracleUser)
					insertUser = AddNewOracleUser(TextBoxOracleService.Text, loginSelected, loginPassword, isLoginNT);

				if (!isNewOracleUser || (insertUser && isNewOracleUser))
				{
					CompanyUserDb companyUserDb = new CompanyUserDb();
					companyUserDb.ConnectionString = sysDbConnString;
					companyUserDb.CurrentSqlConnection = sysDbConnection;

					for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count; i++)
					{
						UserListItem currentItemSelected = (UserListItem)ListViewUsersCompany.CheckedItems[i];
						
						//prendo in considerazioni solo le login che hanno TAG<>'LoginNonEsiste'
						if ((currentItemSelected.Tag == null) ||
							(currentItemSelected.Tag != null &&
							string.Compare(currentItemSelected.Tag.ToString(), ConstString.LoginNotExist, StringComparison.InvariantCultureIgnoreCase) != 0))
						{
							currentItemSelected.DbWindowsAuthentication = false;
							currentItemSelected.DbUser = loginSelected;
							currentItemSelected.DbPassword = loginPassword;
							currentItemSelected.Disabled = false;
							currentItemSelected.IsAdmin = false;

							bool result = true;

							// prima creo i sinonimi sul database Oracle
							if (CreateSynonyms(companyDbName, TextBoxOracleService.Text, loginSelected, loginPassword, isLoginNT))
							{
                                // se devo gestire il database documentale procedo ad aggiungere la login al Easy Attachment
								if (dmsToManage)
									result = GrantLogin();
							}
							else
								result = false;

							if (result)
							{
								// inserisco la login nella MSD_CompanyLogins
								if (companyUserDb.ExistUser(currentItemSelected.LoginId, currentItemSelected.CompanyId) == 1)
									result = companyUserDb.Modify(currentItemSelected);
								else
									result = companyUserDb.Add(currentItemSelected);

								if (!result)
								{
									if (companyUserDb.Diagnostic.Warning || companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information)
										diagnostic.Set(companyUserDb.Diagnostic);
									else
										diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotAssociateUserToCompany, currentItemSelected.Login, companyName, companyDbName));
									updatingAll = false;
								}
								else
									updatingAll = true;

								// se l'aggiornamento delle logins della company e' andato a buon fine
								// procedo con quello del dms
								if (result && dmsToManage)
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

										if (!slaveLoginDb.ExistLoginForSlaveId(currentItemSelected.LoginId, dbSlaveItem.SlaveId))
											slaveLoginDb.Add(dbSlaveItem.SlaveId, currentItemSelected.LoginId, DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.DmsOraUserPw, false);
										else
											slaveLoginDb.Modify(dbSlaveItem.SlaveId, currentItemSelected.LoginId, DatabaseLayerConsts.DmsOraUser, DatabaseLayerConsts.DmsOraUserPw, false);
									}
								}
							}
						}
					}

					//Pulisco il testo del Msg nllo statusPanel
					SetConsoleProgressBarText(sender, string.Empty);

					//rimetto a posto il cursore
					this.TopLevelControl.Cursor = currentConsoleFormCursor;
					Cursor.Current = currentConsoleFormCursor;

					if (OnModifyTreeOfCompanies != null)
						OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, this.companyId);
					if (OnModifyTreeOfCompanies != null)
						OnModifyTreeOfCompanies(this, ConstString.containerCompanyRoles, this.companyId);
				}
				else
				{
					//Pulisco il testo del Msg nllo statusPanel
					SetConsoleProgressBarText(sender, string.Empty);
					//rimetto a posto il cursore
					this.TopLevelControl.Cursor = currentConsoleFormCursor;
					Cursor.Current = currentConsoleFormCursor;
					updatingAll = false;
				}
			}

			State = (updatingAll) ? StateEnums.View : StateEnums.Editing;

			if (diagnostic.Error || diagnostic.Warning)
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
				diagnostic.Clear();
			}

			return updatingAll;
		}
		#endregion

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
				catch (TBException tbExc)
				{
					Debug.Fail(tbExc.Message);
					diagnostic.Set(DiagnosticType.Error, tbExc.Message);
					result = false;
				}
				catch (Exception exc)
				{
					Debug.Fail(exc.Message);
					diagnostic.Set(DiagnosticType.Error, exc.Message);
					result = false;
				}
			}

			return result;
		}

		#region CreateSynonyms
		/// <summary>
		/// CreateSynonyms
		/// </summary>
		//---------------------------------------------------------------------
		private bool CreateSynonyms(string schemaName, string serviceName, string userName, string userPassword, bool userOSAuthent)
		{
			bool result = false;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateAdmin = oracleAccess.LoadSystemData(serviceName);
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
						if (oracleAccess.Diagnostic.Warning || oracleAccess.Diagnostic.Error || oracleAccess.Diagnostic.Information)
							diagnostic.Set(oracleAccess.Diagnostic);
						else
							diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotAssociateUserToCompany, oracleAdmin.Login, companyName, schemaName));
					}
				}
				catch (Exception exc)
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

		#endregion

		#region AddCompanyUserToOracleLogin_Load - Funzione di caricamento del Form
		/// <summary>
		/// AddCompanyUserToOracleLogin_Load
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyUserToOracleLogin_Load(object sender, System.EventArgs e)
		{
			LoadOracleServiceOfCompany();
			LoadAssignedAndFreeOracleUsers();
			if (ComboOracleLogins.Items.Count > 0)
				ComboOracleLogins.SelectedIndex = 0;

			BuildListView();
			LoadOnlyOracleFreeUsers();

			BtnUnselectAll.Enabled = false;
			BtnSave.Enabled = !string.IsNullOrEmpty(TextBoxOracleUserPwd.Text);

			rbSelectExistedOracleUser.Checked = true;
			rbNewOracleUser.Checked = false;
			txtNewOracleUser.Enabled = false;
			CbNTSecurity.Enabled = false;

			// l'evento posso spararlo solo nella Load, perche' nel costruttore non e' ancora stato 
			// agganciato e valorizzato!
			if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				isDMSActivated = true;
		}
		#endregion

		#region LoadOracleServiceOfCompany - Carico i dati relativi all'azienda
		/// <summary>
		/// LoadOracleServiceOfCompany
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadOracleServiceOfCompany()
		{
			string oracleService = string.Empty;
			
			ArrayList companyData = new ArrayList();
			CompanyDb companyDb = new CompanyDb();
			companyDb.CurrentSqlConnection = sysDbConnection;
			companyDb.ConnectionString = sysDbConnString;
			if (!companyDb.GetAllCompanyFieldsById(out companyData, companyId))
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(this, companyDb.Diagnostic);
				companyData.Clear();
			}

			if (companyData.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)companyData[0];
				oracleService = companyItem.DbServer;
				companyName = companyItem.Company;
				companyDbName = companyItem.DbName;
				companyDbOwner = companyItem.DbOwner;
				this.companyUseSlave = companyItem.UseDBSlave;
			}

			TextBoxOracleService.Text = oracleService;
			CompanyUserDb companyUser = new CompanyUserDb();
			companyUser.CurrentSqlConnection = sysDbConnection;
			companyUser.ConnectionString = sysDbConnString;
			bool disable = false;
			companyUser.SelectDataForUserCompany(companyDbOwner, companyId, out ownerIsAdmin, out ownerDbUser, out ownerDbPwd, out ownerDbWinAuth, out disable);
		}
		#endregion

		#region BtnOracleConnectionCheck_Click - Test sulla connessione Oracle
		/// <summary>
		/// BtnOracleConnectionCheck_Click
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnOracleConnectionCheck_Click(object sender, System.EventArgs e)
		{
			if (CheckUserData(false))
				CheckOracleConnection(false);
		}
		#endregion

		#region CheckOracleConnection - Esegue il check (eventualmente silente)
		/// <summary>
		/// CheckOracleConnection
		/// </summary>
		/// <param name="isSilent"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool CheckOracleConnection(bool isSilent)
		{
			//voglio inserire un nuovo utente ... ritorno true
			if (isNewOracleUser)
				return true;

			this.diagnostic.Clear();
			bool successConnection = false;

			string userOracle = ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId;
			bool isNTSecurity = ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent;

			OracleUserImpersonatedData oracleUserData = null;
			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp += new OracleAccess.CallHelpFromPopUp(SendHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole += new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole += new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole += new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateUser = new OracleUserImpersonatedData();
			candidateUser.Login = userOracle;
			candidateUser.WindowsAuthentication = isNTSecurity;
			candidateUser.OracleService = TextBoxOracleService.Text;
			candidateUser.IsDba = false;

			if (isNTSecurity)
			{
				candidateUser.Password = string.Empty;
				candidateUser.Domain = userOracle.Split(Path.DirectorySeparatorChar)[0];
				//non può cambiare le informazioni in finestra
				oracleUserData = oracleAccess.UserImpersonification(candidateUser, true, false);
			}
			else
			{
				candidateUser.Password = TextBoxOracleUserPwd.Text;
				candidateUser.Domain = string.Empty;
				//silente
				oracleUserData = oracleAccess.UserImpersonification(candidateUser);
			}

			if (oracleUserData == null)
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

			if (isNTSecurity)
			{
				if (oracleUserData != null)
					oracleUserData.Undo();
			}

			if (!isSilent)
				this.diagnostic.Clear();

			return successConnection;
		}
		#endregion

		#region CheckUserData - Verifica dei dati inseriti dall'utente
		/// <summary>
		/// CheckUserData
		/// Verifica dei dati inseriti dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckUserData(bool checkSelectedApplictionUser)
		{
			bool result = true;
			//il nome del servizio oracle non può essere vuoto
			if (string.IsNullOrEmpty(TextBoxOracleService.Text))
			{
				diagnostic.Set(DiagnosticType.Error, Strings.EmptyOracleServiceName);
				result = false;
			}

			//il nome del servizio oracle deve contenere caratteri validi
			if (TextBoxOracleService.Text.IndexOfAny(new char[] { '?', '*', '"', '$', '&', '(', ')', '=', '[', '#', ']', Path.DirectorySeparatorChar, '<', '>', ':', '!', '|' }) != -1)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInOracleServiceName);
				result = false;
			}

			if (this.rbSelectExistedOracleUser.Checked)
			{
				if ((ComboOracleLogins.SelectedItem == null) || (((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId.Length == 0))
				{
					diagnostic.Set(DiagnosticType.Error, Strings.NotSelectedCompanyUsers);
					result = false;
				}

				if ((ComboOracleLogins.SelectedItem == null) ||
					!((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent &&
					TextBoxOracleUserPwd.Text.Length == 0)
				{
					diagnostic.Set(DiagnosticType.Error, Strings.EmptyOracleOwnerPwd);
					result = false;
				}
			}

			if (rbNewOracleUser.Checked)
			{
				if (string.IsNullOrEmpty(txtNewOracleUser.Text))
				{
					diagnostic.Set(DiagnosticType.Error, Strings.EmptyOracleOwnerName);
					result = false;
				}
				else
				{
					if (!newOracleUserIsWinNT)
					{
						if (txtNewOracleUser.Text.CountChar(Path.DirectorySeparatorChar) != 0)
						{
							diagnostic.Set(DiagnosticType.Error, Strings.ForbiddedNtUser);
							result = false;
						}
					}

					if (txtNewOracleUser.Text.Trim().IndexOfAny(new char[] { '?', '*', '"', '$', '&', '/', '(', ')', '=', '[', '#', ']', '<', '>', ':', '!', '|' }) != -1)
					{
						diagnostic.Set(DiagnosticType.Warning, Strings.WrongCharactersInOracleOwnerName);
						result = false;
					}
				}
			}

			if (!newOracleUserIsWinNT && string.IsNullOrEmpty(TextBoxOracleUserPwd.Text))
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.EmptyOracleOwnerPwd);
				result = false;
			}

			//Eseguo il check sulla lista degli utenti selezionati (quando creo l'associazione)
			if (checkSelectedApplictionUser)
			{
				//se non ho selezionato alcun utente errore
				if (ListViewUsersCompany.CheckedItems.Count == 0)
				{
					diagnostic.Set(DiagnosticType.Error, Strings.NotSelectedCompanyUsers);
					result = false;
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

		#region BtnSelectAll_Click - Seleziono tutti gli utenti della lista
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
		#endregion

		#region BtnUnselectAll_Click - Deseleziono tutti gli utenti della lista
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

		#region BtnSave_Click - Premuto il bottone di Save
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

		#region ListViewUsersCompany_ItemCheck - Selezione di un elemento della lista
		/// <summary>
		/// ListViewUsersCompany_ItemCheck
		/// </summary>
		//---------------------------------------------------------------------
		private void ListViewUsersCompany_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			//Di default entrambi i bottoni sono abilitati
			BtnSelectAll.Enabled = true;
			BtnUnselectAll.Enabled = true;

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

			if (ListViewUsersCompany.CheckedItems.Count > 0 && e.NewValue == CheckState.Unchecked)
			{
				//non avrò alcun utente selezionato al termine dell'evento - disabilito il check
				if (ListViewUsersCompany.CheckedIndices.Count == 1)
				{
					//CbNTSecurity.Checked = false;
					//CbNTSecurity.Enabled = false;
				}
				//avrò un utente selezionato al termine dell'evento - il check è abilitato se l'utente 
				//è in sicurezza integrata
				else if (ListViewUsersCompany.CheckedIndices.Count == 2)
				{
					int selectedUserPos = 0;
					for (int i = 0; i < ListViewUsersCompany.CheckedItems.Count - 1; i++)
					{
						//scarto il valore di cui sto facendo uncheck
						if (i == e.Index) continue;
						//prendo il restante valore
						selectedUserPos = i;
					}
					if (ListViewUsersCompany.CheckedItems[selectedUserPos] is UserListItem)
					{
						UserListItem currentUserSelect = (UserListItem)ListViewUsersCompany.CheckedItems[selectedUserPos];
						//CbNTSecurity.Checked = false;
						//CbNTSecurity.Enabled = currentUserSelect.DbWindowsAuthentication;

					}
					else
					{
						//CbNTSecurity.Checked = false;
						//CbNTSecurity.Enabled = false;
					}

				}
				//ho più di 1 utente al termine dell'evento - disabilito il check
				else if (ListViewUsersCompany.CheckedIndices.Count > 2)
				{
					//CbNTSecurity.Checked = false;
					//CbNTSecurity.Enabled = false;
				}

			}
			else if (ListViewUsersCompany.CheckedItems.Count > 0 && e.NewValue == CheckState.Checked)
			{
				//CbNTSecurity.Checked = false;

				UserListItem currentUserSelect = (UserListItem)ListViewUsersCompany.Items[e.Index];
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
					UserListItem currentUserSelect = (UserListItem)ListViewUsersCompany.Items[e.Index];
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

			if ((e.NewValue == CheckState.Checked) || (e.NewValue == CheckState.Unchecked && ListViewUsersCompany.CheckedItems.Count > 1))
			{
				//eventualmente abilito il bottone di Save
				if (ComboOracleLogins.SelectedItem != null)
				{
					if (((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent)
						BtnSave.Enabled = true;
					else
						BtnSave.Enabled = (this.TextBoxOracleUserPwd.Text.Length > 0);
				}
			}
			else
				BtnSave.Enabled = false;
		}
		#endregion

		#region TextBoxOracleUserPwd_TextChanged - Input pwd di User Oracle
		/// <summary>
		/// TextBoxOracleUserPwd_TextChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void TextBoxOracleUserPwd_TextChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
			if (ComboOracleLogins.SelectedItem != null)
			{
				//sono in sicurezza integrata
				if (((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent)
					BtnSave.Enabled = true;
				else
				{
					if (ListViewUsersCompany.CheckedItems.Count > 0)
					{
						//se la pwd è vuota non può salvare
						if (TextBoxOracleUserPwd.Text.Length == 0)
							BtnSave.Enabled = false;
						else
							BtnSave.Enabled = true;
					}
					else
						BtnSave.Enabled = false;
				}
			}
			else
				BtnSave.Enabled = false;
		}
		#endregion

		#region ComboOracleLogins_SelectedIndexChanged
		/// <summary>
		/// ComboOracleLogins_SelectedIndexChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void ComboOracleLogins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (ComboOracleLogins.SelectedItem != null)
			{
				OracleUser current = (OracleUser)ComboOracleLogins.SelectedItem;
				if (current.OracleUserOSAuthent)
				{
					//se ho selezionato un utente NT non devo poter digitare la pwd
					TextBoxOracleUserPwd.Text = string.Empty;
					TextBoxOracleUserPwd.Enabled = false;
					//però abilito il bottone di Save se ci sono utenti selezionati
					BtnSave.Enabled = (ListViewUsersCompany.CheckedItems.Count > 0);
				}
				else
				{
					TextBoxOracleUserPwd.Enabled = true;
					if (string.IsNullOrEmpty(TextBoxOracleUserPwd.Text))
						this.BtnSave.Enabled = false;
					else
						BtnSave.Enabled = (ListViewUsersCompany.CheckedItems.Count > 0);
				}
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

		#region Funzioni per il Send di diagnostica alla MicroareaConsole
		/// <summary>
		/// AddCompanyUserToOracleLogin_Closing
		/// per usi futuri
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void AddCompanyUserToOracleLogin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// AddCompanyUserToOracleLogin_Deactivate
		/// per usi futuri
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void AddCompanyUserToOracleLogin_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// AddCompanyUserToOracleLogin_VisibleChanged
		/// Questa è qualla correntemente usata
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void AddCompanyUserToOracleLogin_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}
		#endregion

		//---------------------------------------------------------------------
		private void ComboOracleLogins_DropDown(object sender, System.EventArgs e)
		{
			if (ComboOracleLogins.Items.Count <= 1)
				LoadAssignedAndFreeOracleUsers();
		}

		//---------------------------------------------------------------------
		private void SendHelp(object sender, string nameSpace, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, nameSpace, searchParameter);
		}

		#region Funzioni relative alla ProgressBar
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

		/// <summary>
		/// SetConsoleProgressBarValue
		/// Dice alla MC di impostare il value della ProgressBar
		/// pari a currentValue
		/// </summary>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarValue(object sender, int currentValue)
		{
			if (OnSetProgressBarValue != null)
				OnSetProgressBarValue(sender, currentValue);
		}

		/// <summary>
		/// SetConsoleProgressBarText
		/// Dice alla MC quale deve essere il testo da visualizzare 
		/// accanto alla progressBar (pari a message)
		/// </summary>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarText(object sender, string message)
		{
			if (OnSetProgressBarText != null)
				OnSetProgressBarText(sender, message);
		}
		#endregion

		/// <summary>
		/// CbNTSecurity_CheckedChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void CbNTSecurity_CheckedChanged(object sender, System.EventArgs e)
		{
			if (CbNTSecurity.Checked)
			{
				txtNewOracleUser.Text = WindowsIdentity.GetCurrent().Name.ToUpper(CultureInfo.InvariantCulture);
				TextBoxOracleUserPwd.Text = string.Empty;
				TextBoxOracleUserPwd.Enabled = false;
				txtNewOracleUser.Enabled = false;

			}
			else
			{
				txtNewOracleUser.Text = string.Empty;
				TextBoxOracleUserPwd.Text = string.Empty;
				TextBoxOracleUserPwd.Enabled = true;
				txtNewOracleUser.Enabled = true;
			}
		}

		/// <summary>
		/// rbNewOracleUser_CheckedChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void rbNewOracleUser_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rbNewOracleUser.Checked)
			{
				ComboOracleLogins.Enabled = false;
				txtNewOracleUser.Enabled = true;
				CbNTSecurity.Enabled = !ExistNTCurrentUser();
				isNewOracleUser = true;
				TextBoxOracleUserPwd.Text = string.Empty;
				//disabilito il bottone di check
				BtnOracleConnectionCheck.Enabled = false;
			}
		}

		/// <summary>
		/// ExistNTCurrentUser
		/// </summary>
		/// <returns></returns>
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
					existUser = oracleAccess.ExistUserIntoDatabase(WindowsIdentity.GetCurrent().Name.ToUpper(CultureInfo.InvariantCulture), false);
				}
				catch (Exception exc)
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

		//---------------------------------------------------------------------
		private void txtNewOracleUser_Leave(object sender, System.EventArgs e)
		{
			txtNewOracleUser.Text = txtNewOracleUser.Text.ToUpper(CultureInfo.InvariantCulture);
			isNewOracleUser = true;
			newOracleUserIsWinNT = CbNTSecurity.Checked;
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
				isNewOracleUser = false;
				newOracleUserIsWinNT = false;
				CbNTSecurity.Enabled = false;
				CbNTSecurity.Checked = false;
			}
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
