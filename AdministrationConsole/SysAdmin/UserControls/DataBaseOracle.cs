using System;
using System.Collections;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using Microarea.Console.Plugin.SysAdmin.Form;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.OracleDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
	/// <summary>
	/// DataBaseOracle
	/// Tab Page dei Settings dei Database Oracle
	/// </summary>
	//=========================================================================
	public partial class DataBaseOracle : System.Windows.Forms.UserControl
	{
		#region Events and delegates
		//non posso continuare, disabilito i bottoni di salva/cancella nella form della Company
		public event System.EventHandler OnUnableToContinue;

		public delegate void CallHelp(object sender, string nameSpace, string searchParameter);
		public event CallHelp OnCallHelp;

		// evento da agganciare per impostare uno specifico utente nella combobox
		public delegate void SetValueUsersComboBox(string selectedUser);
		public event SetValueUsersComboBox OnSetValueUsersComboBox;

		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;

		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;
		
		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;
		#endregion

		#region Variabili private
		private string	oracleService           = string.Empty;
		private string	serverNameSysDb			= string.Empty;
		private string	serverIstanceSystemDb	= string.Empty;
		private string	connectionString        = string.Empty;
		private string	companyId               = string.Empty;
		private string	userConnected			= string.Empty;
		private string  userDbOwnerId           = string.Empty;
		private bool	isNewOracleCompany		= false;
		private bool	isNewOracleUser         = false;
		private string  oracleCompanyName       = string.Empty;

		private SqlConnection	currentConnection	= null;
		private Diagnostic		diagnostic			= new Diagnostic("DatabaseOracleTab");

		private string	selectedDbOwnerName		= string.Empty;
		private string	selectedDbOwnerPwd		= string.Empty;
		private string	oracleServiceName		= string.Empty;
		private string	applicationUserName		= string.Empty;
		private string	applicationUserId		= string.Empty;
		private bool	selectedDbOwnerIsWinNT	= false;
		private string	selectedDbLanguage		= string.Empty;
		private string	selectedDbTerritory		= string.Empty;
		#endregion
		
		#region Proprietà pubbliche (settabili da designer)
		//---------------------------------------------------------------------
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string ServerNameSystemDb { get { return serverNameSysDb; } set { serverNameSysDb = value; }}
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string OracleService	{ get { return oracleService; } set { oracleService	= value; }}
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string ServerIstanceSystemDb { get { return serverIstanceSystemDb; } set { serverIstanceSystemDb = value; }}
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string CompanyId	{ get { return companyId; } set { companyId	= value; }}
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string ConnectionString { get { return connectionString; } set { connectionString = value; }}
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string UserConnected	{ get { return userConnected; } set { userConnected	= value; }}
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string UserDbOwnerId	{ get { return userDbOwnerId; } set { userDbOwnerId	= value; }}
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string OracleCompanyName		
		{
			get { return oracleCompanyName; } 
			set 
			{ 
				oracleCompanyName = value; 
				if (rbNewOracleUser.Checked && txtNewOracleUser.Text.Length == 0) 
					txtNewOracleUser.Text = oracleCompanyName.ToUpper(CultureInfo.InvariantCulture);
			}
		}
		#endregion

		#region Proprietà pubbliche (dopo le selezioni sullo UserControl)
		//---------------------------------------------------------------------
		public SqlConnection CurrentConnection			{ get { return currentConnection;		} set { currentConnection		= value; }}
		public string		 SelectedDbOwnerName		{ get { return selectedDbOwnerName;		} set { selectedDbOwnerName		= value;}}
		public string		 SelectedDbOwnerPwd			{ get { return selectedDbOwnerPwd;		} set { selectedDbOwnerPwd		= value;}}
		public bool			 SelectedDbOwnerIsWinNT		{ get { return selectedDbOwnerIsWinNT;	} set { selectedDbOwnerIsWinNT	= value;} }
		public string		 SelectedOracleServiceName	{ get { return oracleServiceName;		} set { oracleServiceName		= value; }}
		public string		 ApplicationUserName        { get { return applicationUserName;		} set { applicationUserName		= value; }}
		public string		 ApplicationUserId          { get { return applicationUserId;		} set { applicationUserId		= value; }}
		public string		 SelectedDbLanguage			{ get { return selectedDbLanguage;		} set { selectedDbLanguage		= value;}}
		public string		 SelectedDbTerritory		{ get { return selectedDbTerritory;		} set { selectedDbTerritory		= value;}}
		public bool			 IsNewOracleCompany         { get { return isNewOracleCompany;		} set { isNewOracleCompany		= value;}}
		public bool			 IsNewOracleUser            { get { return isNewOracleUser;         } set { isNewOracleUser         = value; }}
		public Diagnostic    Diagnostic                 { get { return diagnostic; }}
		#endregion

		//---------------------------------------------------------------------
		public DataBaseOracle()
		{
			InitializeComponent();
			diagnostic.Clear();
		}

		#region DataBaseOracle_Load - Carica la combo degli Utenti Applicativi
		/// <summary>
		/// DataBaseOracle_Load
		/// Quando carico lo user control nel tab, carico la combo degli utenti applicativi e ne seleziono uno
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void DataBaseOracle_Load(object sender, System.EventArgs e)
		{
			// non serve piu'
			//LoadData();
		}
		#endregion
	
		/// <summary>
		/// LoadOracleUsers
		/// carica la combo delle login di database ORACLE
		/// </summary>
		//--------------------------------------------------------------------
		private void LoadOracleUsers()
		{
			ComboOracleLogins.DataSource = null;
			ComboOracleLogins.Items.Clear();
			CompanyUserDb companyUsers = new CompanyUserDb();
			companyUsers.ConnectionString = this.ConnectionString;
			companyUsers.CurrentSqlConnection = this.CurrentConnection;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace								= "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp					+= new OracleAccess.CallHelpFromPopUp(SendCompleteHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole		+= new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole	+= new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole		+= new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateAdmin = new OracleUserImpersonatedData();
			candidateAdmin.Login	= "system";
			candidateAdmin.Password = string.Empty;
			candidateAdmin.OracleService = SelectedOracleServiceName;
			candidateAdmin.IsDba	= true;
			candidateAdmin.Domain	= string.Empty;
			
			OracleUserImpersonatedData oracleAdmin = oracleAccess.AdminImpersonification(candidateAdmin); 
			//oracleAccess.AdminImpersonification(oracleService);
			if (oracleAdmin == null)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.WrongUserCredential, candidateAdmin.Login));
				return;
			}
			else
			{
				ArrayList freeUsers = companyUsers.LoadFreeOracleUsersForAttach(SelectedOracleServiceName, oracleCompanyName, oracleAdmin);
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

		#region LoadData - Carica i dati di default
		/// <summary>
		/// LoadData
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadData()
		{
			diagnostic.Clear();
			if (cbUserOwner.Items.Count == 0)
			{
				//carico gli utenti applicativi 
				cbUserOwner.ServerName			= ServerNameSystemDb;
				cbUserOwner.IstanceName			= ServerIstanceSystemDb;
				cbUserOwner.ConnectionString	= ConnectionString;
				cbUserOwner.CurrentConnection	= CurrentConnection;
				cbUserOwner.CompanyId			= CompanyId;
				TextBoxOracleService.Text       = OracleService;
				
				cbUserOwner.LoadAssociatedUsers();

				if (cbUserOwner.Items.Count > 0)
				{
					string userSelected = SelectDbOwnerLogin(UserDbOwnerId);
					cbUserOwner.SelectUser(userSelected);
					userSelected = cbUserOwner.SelectedUserName;
		
					string dbOwnerName = string.Empty;
					string dbOwnerPwd = string.Empty;
					bool dbOwnerIsNT = false;

					if (UserDbOwnerId.Length == 0)
						TextBoxOracleUserPwd.Text = string.Empty;
					else
					{
						if (SelectDbownerName(CompanyId, UserDbOwnerId, out dbOwnerName, out dbOwnerPwd, out dbOwnerIsNT))
						{
							SelectOracleUser(dbOwnerName, dbOwnerIsNT);
							TextBoxOracleUserPwd.Text = dbOwnerPwd;
						}
					}
				}
				else
				{
					diagnostic.Set(DiagnosticType.Error, Strings.NoAvailableUsers);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					diagnostic.Clear();
					if (OnUnableToContinue != null)
						OnUnableToContinue(this, new EventArgs());
				}

				if (this.rbSelectExistedOracleUser.Checked)
				{
					CbNTSecurity.Enabled = false;
					txtNewOracleUser.Enabled = false;
				}
			}
			else
				cbUserOwner.SelectUser(UserConnected);

			SettingToolTips();
			
			if (TextBoxOracleService.Text.Length == 0)
			{
				ComboOracleLogins.Enabled			= false;
				rbNewOracleUser.Enabled				= false;
				rbSelectExistedOracleUser.Enabled	= false;
				TextBoxOracleUserPwd.Enabled		= false;
				txtNewOracleUser.Enabled			= false;
				CbNTSecurity.Enabled				= false;
				BtnOracleConnectionCheck.Enabled	= false;
			}
			BtnNTUsersSelect.Enabled = CbNTSecurity.Checked;
		}
		#endregion

		//--------------------------------------------------------------------
		private void SelectOracleUser(string userToSelect, bool authenticationType)
		{
			//ho selezionato un utente esistente dalla combo
			if (rbSelectExistedOracleUser.Checked)
			{
				if (ComboOracleLogins.Items.Count == 0)
				{
					ComboOracleLogins.Items.Add(new OracleUser(userToSelect, authenticationType));
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
							SelectedDbOwnerName = ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId;
							SelectedDbOwnerIsWinNT = ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent;
							break;
						}
					}
				}
			}
			//sto creando un nuovo utente Oracle
			else if (rbNewOracleUser.Checked)
			{
				IsNewOracleUser			= true;
				SelectedDbOwnerName		= txtNewOracleUser.Text;
				SelectedDbOwnerIsWinNT	= false;
			}
		}

		#region SelectDbOwnerLogin - Data la LoginId, prendo la Login dell'utente applicativo
		/// <summary>
		/// SelectDbOnwerLogin
		/// </summary>
		//---------------------------------------------------------------------
		private string SelectDbOwnerLogin(string loginId)
		{
			string loginName = string.Empty;
			UserDb loginDbowner = new UserDb();
			loginDbowner.ConnectionString = this.ConnectionString;
			loginDbowner.CurrentSqlConnection = this.CurrentConnection;
			ArrayList loginData = new ArrayList();
			
			if (loginId.Length == 0) 
				loginName = UserConnected;
			else
			{
				if (loginDbowner.GetAllUserFieldsById(out loginData, loginId) && (loginData.Count > 0))
				{
					UserItem loginItem = (UserItem)loginData[0];
					loginName = loginItem.Login;
				}
				
			}
			return loginName;
		}
		#endregion

		#region SelectDbownerName - Data LoginId, CompanyId, prendo il DbUser e DBPassword in MSD_CompanyLogins
		//---------------------------------------------------------------------
		private bool SelectDbownerName(string companyId, string loginId, out string ownerName, out string ownerPwd, out bool ownerIsWinNt)
		{
			bool isAdmin, isWinNt, isDisable;
			string dbOwnerName = string.Empty;
			string dbOwnerPwd  = string.Empty;
			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = this.ConnectionString;
			companyUserDb.CurrentSqlConnection = this.CurrentConnection;

			bool result = companyUserDb.SelectDataForUserCompany(loginId, companyId, out isAdmin, out dbOwnerName, out dbOwnerPwd, out isWinNt, out isDisable);
			ownerName = dbOwnerName;
			ownerPwd  = dbOwnerPwd;
			ownerIsWinNt = isWinNt;
			return result;
		}
		#endregion

		#region CheckUserData - Verifica la correttezza dei dati inseriti
		/// <summary>
		/// CheckUserData
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckUserData()
		{
			bool result = true;
			if (ComboOracleLogins.SelectedItem == null || ComboOracleLogins.SelectedIndex == -1)
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.EmptyOracleOwnerName);
				result = false;
			}
			if ((ComboOracleLogins.SelectedItem != null) && !((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent && (TextBoxOracleUserPwd.Text.Length == 0))
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.EmptyOracleOwnerPwd);
				result = false;
			}
			if (TextBoxOracleService.Text.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.EmptyOracleServiceName);
				TextBoxOracleUserPwd.Focus();
				result = false;
			}
			
			if (TextBoxOracleService.Text.IndexOfAny(new char[] {'?', '*', '"', '$', '&', '(', ')', '=', '[', '#', ']', Path.DirectorySeparatorChar, '<', '>', ':', '!', '|'}) != -1)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInOracleServiceName);
				result = false;
			}

			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				diagnostic.Clear();
			}

			return result;
		}
		#endregion

		#region BtnOracleConnectionCheck_Click - Testa la connessione Oracle con le info inserite
		/// <summary>
		/// BtnOracleConnectionCheck_Click
		/// Testa la connessione Oracle con le info inserite
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnOracleConnectionCheck_Click(object sender, System.EventArgs e)
		{
			diagnostic.Clear();
			if (CheckUserData())
				CheckOracleConnection(false);
		}
		#endregion

		#region Metodi di controllo con connessioni varie al server Oracle
		/// <summary>
		/// CheckOracleConnection - Esegue il check
		/// </summary>
		//---------------------------------------------------------------------
		public bool CheckOracleConnection(bool isSilent)
		{
			this.diagnostic.Clear();
			bool successConnection	= false;
			string oracleUserName	=  ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId;
			bool isUserWinNt		= ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp					+= new OracleAccess.CallHelpFromPopUp(SendCompleteHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole		+= new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole	+= new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole		+= new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateUser = new OracleUserImpersonatedData();
			candidateUser.Login	= oracleUserName;
			candidateUser.WindowsAuthentication = isUserWinNt;
			candidateUser.OracleService	= SelectedOracleServiceName;
			candidateUser.IsDba	= false;

			OracleUserImpersonatedData oracleUser = null;

			if (isUserWinNt)
			{
				candidateUser.Password	= string.Empty;
				candidateUser.Domain	= oracleUserName.Split(Path.DirectorySeparatorChar)[0];
				//non può cambiare le informazioni in finestra
				oracleUser = oracleAccess.UserImpersonification(candidateUser, true, false); 
			}
			else
			{
				candidateUser.Password = TextBoxOracleUserPwd.Text;
				candidateUser.Domain	= string.Empty;
				//silente
				oracleUser = oracleAccess.UserImpersonification(candidateUser); 
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
				if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
				{
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					diagnostic.Clear();
				}
			}
			
			if (isUserWinNt)
			{
				if (oracleUser != null)
					oracleUser.Undo(); 
			}
			
			return successConnection;
		}

		//---------------------------------------------------------------------
		private bool ExistNTCurrentUser()
		{
			bool existUser = false;

			OracleAccess oracleAccess = new OracleAccess();
			oracleAccess.NameSpace = "Module.MicroareaConsole.SysAdmin";
			oracleAccess.OnCallHelpFromPopUp					+= new OracleAccess.CallHelpFromPopUp(SendCompleteHelp);
			oracleAccess.OnAddUserAuthenticatedFromConsole		+= new OracleAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			oracleAccess.OnGetUserAuthenticatedPwdFromConsole	+= new OracleAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			oracleAccess.OnIsUserAuthenticatedFromConsole		+= new OracleAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);

			OracleUserImpersonatedData candidateAdmin = new OracleUserImpersonatedData();
			candidateAdmin.Login = "system";
			candidateAdmin.Password = string.Empty;
			candidateAdmin.OracleService = SelectedOracleServiceName;
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
				selectedDbLanguage	= oracleAdmin.Language;
				selectedDbTerritory = oracleAdmin.Territory;

				try
				{
					oracleAccess.LoadUserData(oracleAdmin.OracleService, oracleAdmin.Login, oracleAdmin.Password, oracleAdmin.WindowsAuthentication);
					oracleAccess.OpenConnection();
					existUser = oracleAccess.ExistUserIntoDatabase(WindowsIdentity.GetCurrent().Name.ToUpper(CultureInfo.InvariantCulture), true);
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
		#endregion

		/// <summary>
		/// HideDbChanges
		/// Disabilito le Informazioni sulla connessione
		/// (ad esempio, quando ho raggiunto un numero aziende = 2 e sono 
		/// in Standard Edition - non posso più fare attach/detach di db)
		/// </summary>
		//---------------------------------------------------------------------
		public void HideDbChanges()
		{
			GrBoxOracleConnData.Enabled = false;
		}

		//--------------------------------------------------------------------
		private void SettingToolTips()
		{
			if (TextBoxOracleService.Enabled)
				toolTipOracle.SetToolTip(TextBoxOracleService, Strings.OracleServiceInsertingToolTip);
			else
				toolTipOracle.SetToolTip(TextBoxOracleService, Strings.OracleServiceToolTip);
			toolTipOracle.SetToolTip(cbUserOwner, Strings.OracleApplicationUsersToolTip);
			toolTipOracle.SetToolTip(BtnOracleConnectionCheck, Strings.OracleConnectionButtonToolTip);
			toolTipOracle.SetToolTip(ComboOracleLogins, Strings.OracleLoginsToolTip);
			toolTipOracle.SetToolTip(TextBoxOracleUserPwd, Strings.OracleUserPasswordToolTip);
		}

		/// <summary>
		/// SettingLayout
		/// </summary>
		//---------------------------------------------------------------------
		public void SettingLayout()
		{
			if (IsNewOracleCompany)
			{
				TextBoxOracleService.Enabled = true;
				rbNewOracleUser.Checked = true;
			}
			else
			{
				TextBoxOracleService.Enabled = false;
				rbSelectExistedOracleUser.Checked = true;
			}
		}

		/// <summary>
		/// cbUserOwner_SelectedIndexChanged
		/// Ho selezionato un utente applicativo, imposto le properties che mi leggerò dalla form della company
		/// </summary>
		//---------------------------------------------------------------------
		private void cbUserOwner_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			ApplicationUserName = cbUserOwner.SelectedUserName;
			ApplicationUserId = cbUserOwner.SelectedUserId;

			// sparo un evento alla form che contiene lo user control per notificare
			// il nome dell'utente applicativo selezionato dall'apposita combobox
			if (OnSetValueUsersComboBox != null)
				OnSetValueUsersComboBox(ApplicationUserName);
		}

		//---------------------------------------------------------------------
		private void TextBoxOracleUserPwd_TextChanged(object sender, System.EventArgs e)
		{
			SelectedDbOwnerPwd = ((TextBox)sender).Text;
		}

		//---------------------------------------------------------------------
		private void ComboOracleLogins_DropDown(object sender, System.EventArgs e)
		{
			if (ComboOracleLogins.Items.Count <= 1)
				LoadOracleUsers();
		}

		//---------------------------------------------------------------------
		private void ComboOracleLogins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SelectedDbOwnerName			= ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserId;
			SelectedDbOwnerIsWinNT		= ((OracleUser)ComboOracleLogins.SelectedItem).OracleUserOSAuthent;
			TextBoxOracleUserPwd.Text	= string.Empty;
			//se sono in sicurezza integrata non inserisco la pwd
			TextBoxOracleUserPwd.Enabled = !SelectedDbOwnerIsWinNT;
		}

		/// <summary>
		/// rbNewOracleUser_CheckedChanged
		/// Sto creando un nuovo utente Oracle (e lo voglio aggiungere al datasource oracle)
		/// </summary>
		//---------------------------------------------------------------------
		private void rbNewOracleUser_CheckedChanged(object sender, System.EventArgs e)
		{
			if (TextBoxOracleService.Text.Length > 0)
			{
				rbNewOracleUser.Enabled = true;
				rbSelectExistedOracleUser.Enabled = true;
				CbNTSecurity.Enabled = false;
				if (rbNewOracleUser.Checked)
				{
					ComboOracleLogins.Enabled = false;
					txtNewOracleUser.Enabled = true;
					BtnOracleConnectionCheck.Enabled = false;
					
					if (IsNewOracleCompany && txtNewOracleUser.Text.Length == 0)
						txtNewOracleUser.Text = OracleCompanyName.ToUpper(CultureInfo.InvariantCulture);
					
					CbNTSecurity.Enabled = !ExistNTCurrentUser();
					
					//disabilito il bottone di verifica perchè se l'utente non esiste nel db
					//non posso neppure testare la connessione
					BtnOracleConnectionCheck.Enabled = false;
					SelectedDbOwnerName			= txtNewOracleUser.Text.ToUpper(CultureInfo.InvariantCulture);
					SelectedDbOwnerIsWinNT		= CbNTSecurity.Checked;
					TextBoxOracleUserPwd.Text	= string.Empty;
					TextBoxOracleUserPwd.Enabled= !SelectedDbOwnerIsWinNT;
					IsNewOracleUser				= true;
				}
			}
		}

		/// <summary>
		/// rbSelectExistedOracleUser_CheckedChanged
		/// Seleziono un utente esistente
		/// </summary>
		//---------------------------------------------------------------------
		private void rbSelectExistedOracleUser_CheckedChanged(object sender, System.EventArgs e)
		{
			if (TextBoxOracleService.Text.Length > 0)
			{
				if (rbSelectExistedOracleUser.Checked)
				{
					ComboOracleLogins.Enabled = true;
					txtNewOracleUser.Enabled = false;
					TextBoxOracleUserPwd.Text = string.Empty;
					//abilito il bottone di verifica della connessione
					BtnOracleConnectionCheck.Enabled = true;
					txtNewOracleUser.Text = string.Empty;
					IsNewOracleUser = false;
					CbNTSecurity.Enabled = false;
					CbNTSecurity.Checked = false;
				}
			}
		}

		/// <summary>
		/// CbNTSecurity_CheckedChanged
		/// Utilizzo come l'utente il CurrentUserNT
		/// </summary>
		//---------------------------------------------------------------------
		private void CbNTSecurity_CheckedChanged(object sender, System.EventArgs e)
		{
			if (CbNTSecurity.Checked)
			{
				txtNewOracleUser.Text			= WindowsIdentity.GetCurrent().Name.ToUpper(CultureInfo.InvariantCulture);
				TextBoxOracleUserPwd.Text		= string.Empty;
				TextBoxOracleUserPwd.Enabled	= false;
				BtnNTUsersSelect.Enabled		= true;
				txtNewOracleUser.Enabled		= false;
			}
			else
			{
				txtNewOracleUser.Text = string.Empty;
				if (rbNewOracleUser.Checked)
					txtNewOracleUser.Text = OracleCompanyName.ToUpper(CultureInfo.InvariantCulture);
				TextBoxOracleUserPwd.Text		= string.Empty;
				TextBoxOracleUserPwd.Enabled	= true;
				BtnNTUsersSelect.Enabled		= false;
				txtNewOracleUser.Enabled		= true;
			}

			SelectedDbOwnerName		= txtNewOracleUser.Text;
			SelectedDbOwnerIsWinNT	= CbNTSecurity.Checked;
			IsNewOracleUser			= rbNewOracleUser.Checked;
		}

		//---------------------------------------------------------------------
		private void SendCompleteHelp(object sender, string nameSpace, string searchParameter)
		{
			if (this.OnCallHelp != null)
				OnCallHelp(sender, nameSpace, searchParameter);
		}

		//---------------------------------------------------------------------
		private void BtnNTUsersSelect_Click(object sender, System.EventArgs e)
		{
			//Dominio corrente
			string domainSelected = Environment.UserDomainName.ToUpper(CultureInfo.InvariantCulture);
			string currentServer  = Environment.MachineName.ToUpper(CultureInfo.InvariantCulture);

			UsersBrowser usersBrowser		= new UsersBrowser(domainSelected, currentServer, false);
			usersBrowser.OnAfterUserSelected += new UsersBrowser.AfterUserSelected(this.AfterUserSelected);
			usersBrowser.OnOpenHelpFromPopUp += new UsersBrowser.OpenHelpFromPopUp(this.SendCompleteHelp);
			usersBrowser.ShowDialog();
		}

		//---------------------------------------------------------------------
		private void AfterUserSelected(object sender, string domainSelected, string usersSelected)
		{
			txtNewOracleUser.Text	= usersSelected.ToUpper(CultureInfo.InvariantCulture);
			SelectedDbOwnerName		= txtNewOracleUser.Text;
			IsNewOracleUser			= true;
			SelectedDbOwnerIsWinNT	= true;
		}

		//---------------------------------------------------------------------
		private void txtNewOracleUser_Leave(object sender, System.EventArgs e)
		{
			SelectedDbOwnerName		= txtNewOracleUser.Text.ToUpper(CultureInfo.InvariantCulture);
			txtNewOracleUser.Text	= SelectedDbOwnerName;
			IsNewOracleUser			= true;
			SelectedDbOwnerIsWinNT  = CbNTSecurity.Checked;
		}

		//---------------------------------------------------------------------
		private void TextBoxOracleService_Leave(object sender, System.EventArgs e)
		{
			SelectedOracleServiceName = ((TextBox)sender).Text;
			
			if (SelectedOracleServiceName.Length > 0)
			{
				TextBoxOracleUserPwd.Enabled		= true;
				rbNewOracleUser.Enabled				= true;
				rbSelectExistedOracleUser.Enabled	= true;
				
				if (rbNewOracleUser.Checked)
				{
					ComboOracleLogins.Enabled	= false;
					txtNewOracleUser.Enabled	= true;
					BtnOracleConnectionCheck.Enabled = false;
					rbNewOracleUser_CheckedChanged(sender, e); // forzo il checked per fargli sentire il changed
				}
				else
				{
					ComboOracleLogins.Enabled	= true;
					txtNewOracleUser.Enabled	= false;
					BtnOracleConnectionCheck.Enabled = true;
				}
			}
			else
			{
				TextBoxOracleUserPwd.Enabled		= false;
				rbNewOracleUser.Enabled				= false;
				rbSelectExistedOracleUser.Enabled	= false;
				BtnOracleConnectionCheck.Enabled	= false;
			}
		}

		///<summary>
		/// Disabilita la combobox degli utenti applicativi
		/// Utilizzato con la gestione degli slave
		///</summary>
		//---------------------------------------------------------------------
		public void EnableUsersComboBox(bool enable)
		{
			cbUserOwner.Enabled = enable;
		}

		#region Funzioni per la verifica dell'autenticazione di un utente (viene interrogata la Console)
		/// <summary>
		/// AddUserAuthentication
		/// Aggiunge l'utente specificato alla lista degli utenti della Console
		/// (lista che contiene gli utenti autenticati)
		/// </summary>
		//---------------------------------------------------------------------
		private void AddUserAuthentication(string login, string password, string serverName, DBMSType dbType)
		{
			if (OnAddUserAuthenticatedFromConsole != null)
				OnAddUserAuthenticatedFromConsole(login, password, serverName, dbType);
		}

		/// <summary>
		/// GetUserAuthenticatedPwd
		/// Richiede alla Console, per l'utente specificato, la sua password 
		/// (già inserita precedentmente poichè l'utente in questione risulta autenticato
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
		/// Interroga la Console per verificare se l'utente specificato da login e password risulta autenticato
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
	}
}