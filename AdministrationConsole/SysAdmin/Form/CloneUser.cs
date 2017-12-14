using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.SQLDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// Summary description for CloneUser.
	/// </summary>
	//=========================================================================
	public partial class CloneUser : PlugInsForm
	{
		#region Private variables
		private Diagnostic        diagnostic        = new Diagnostic("SysAdminPlugIn.CloneUser");
		private TransactSQLAccess connSqlTransact	= null;

		private string			sysDbConnString	= string.Empty; //stringa connessione db sistema
		private SqlConnection	sysDbConnection	= null;			// connessione aperta sul db sistema
		private string			treeCompanyId		= string.Empty;	// nodo azienda selezionato
		private string			treeLoginId		    = string.Empty;	// nodo utente selezionato
		private string			oldCompanyId		= string.Empty; // mi tengo l'old per non caricare + volte le stesse login

		private UserItem	selectedUser	= null; // info utente selezionato da clonare
		private CompanyItem selectedCompany = null; // info azienda selezionata (di destinazione) su cui si clona l'utente
		
		// nel caso in cui l'azienda di origine e quella di destinazione siano diverse, devo dare la possibilità
		// di associare all'azienda di destinazione un utente che magari è già censito nella MSD_Logins ma non è stato
		// associato al server di appartenenza dell'azienda
		private bool		skipInsertUser		= false;// se devo skippare l'inserimento dell'utente
		private UserItem	existingUserItem	= null; // info utente già esistente nel caso lo vada ad associare ad un'azienda diversa
		
		private bool		isTheSameCompany	= false;// se sto clonando sulla stessa azienda

		private int		minPasswordLength	= -1;
		private int		passwordDuration	= -1;
		private bool	useStrongPwd		= false;

		// variabili di appoggio per il database aziendale
		private TransactSQLAccess companyConnSqlTransact = new TransactSQLAccess();
		private UserImpersonatedData companyImpersonated = new UserImpersonatedData();

		private string dbOwnerLogin		= string.Empty;
		private string dbOwnerPassword	= string.Empty;
		private bool   dbOwnerWinAuth	= false;
		private string dbOwnerDomain	= string.Empty;
		private string dbOwnerPrimary	= string.Empty;
		private string dbOwnerInstance	= string.Empty;
		private string dbCompanyName	= string.Empty;
		private string dbCompanyServer	= string.Empty;

		// variabili di appoggio per il database documentale
		private bool isDMSActivated = false;
		private bool dmsToManage = false;

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
		# endregion

		#region Events and Delegates
		//---------------------------------------------------------------------
		public delegate void ModifyTreeOfCompanies(object sender, string nodeType, string companyId);
		public event ModifyTreeOfCompanies OnModifyTreeOfCompanies;

		public delegate void ModifyTree(object sender, string nodeType);
		public event ModifyTree	OnModifyTree;

		public delegate void AfterClonedUserCompany(string companyId);
		public event AfterClonedUserCompany	OnAfterClonedUserCompany;
		
		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic	OnSendDiagnostic;

		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;

		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;
		
		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;
		
		public delegate void CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event CallHelpFromPopUp OnCallHelpFromPopUp;

		public delegate bool IsActivated(string application, string functionality);
		public event IsActivated OnIsActivated;
		#endregion

		# region Constructor
		//---------------------------------------------------------------------
		public CloneUser(string connString, SqlConnection connection, string companyId, string loginId, PathFinder pathFinder)
		{
			InitializeComponent();

			this.sysDbConnString	= connString;
			this.sysDbConnection	= connection;
			this.treeCompanyId		= companyId;
			this.treeLoginId		= loginId;

			minPasswordLength = InstallationData.ServerConnectionInfo.MinPasswordLength;
			passwordDuration = InstallationData.ServerConnectionInfo.PasswordDuration;
			useStrongPwd = InstallationData.ServerConnectionInfo.UseStrongPwd;

			NewLoginRadioButton.Checked = true;

			// carico il nome dell'azienda e dell'utente di origine e li visualizzo nelle apposite label
			LoadDataInSourceInfoLabel();
			LoadCompaniesComboBox(); // carico l'elenco delle aziende
		}

		//---------------------------------------------------------------------
		private void CloneUser_Load(object sender, EventArgs e)
		{
			// l'evento posso spararlo solo nella Load, perche' nel costruttore non e' ancora stato 
			// agganciato e valorizzato!
			if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				isDMSActivated = true;
		}
		# endregion

		# region LoadDataInSourceInfoLabel
		/// <summary>
		/// Visualizzo nella apposite label il nome dell'azienda e dell'utente di origine per la clonazione
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadDataInSourceInfoLabel()
		{
			SourceCompanyNameLabel.Text = string.Empty;
			SourceUserNameLabel.Text	= string.Empty;

			CompanyDb companyDb	= new CompanyDb();
			companyDb.ConnectionString = sysDbConnString;
			companyDb.CurrentSqlConnection = sysDbConnection;
			ArrayList companyFields = new ArrayList();
			if (companyDb.GetAllCompanyFieldsById(out companyFields, this.treeCompanyId))
			{
				if (companyFields.Count > 0)
				{
					CompanyItem cItem = (CompanyItem)companyFields[0];
					SourceCompanyNameLabel.Text = (cItem != null) ? cItem.Company : string.Empty;
					SourceCompanyNameLabel.Tag = cItem;
				}
			}
			
			UserDb userDb = new UserDb(sysDbConnString);
			userDb.CurrentSqlConnection = sysDbConnection;
			ArrayList userFields = new ArrayList();
			if (userDb.GetAllUserFieldsById(out userFields, this.treeLoginId))
			{
				if (userFields.Count > 0)
				{
					UserItem uItem = (UserItem)userFields[0];
					SourceUserNameLabel.Text = (uItem != null) ? uItem.Login : string.Empty;
					SourceUserNameLabel.Tag = uItem;
					selectedUser = uItem;
				}
			}
		}
		# endregion

		#region LoadCompaniesComboBox - Carica nella combobox l'elenco di tutte le Aziende
		/// <summary>
		/// LoadCompaniesComboBox
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadCompaniesComboBox()
		{
			CompaniesComboBox.Items.Clear();
			
			CompanyDb companyDb	= new CompanyDb();
			companyDb.ConnectionString = sysDbConnString;
			companyDb.CurrentSqlConnection = sysDbConnection;

			ArrayList companyList = new ArrayList();
			if (!companyDb.SelectAllCompanies(out companyList))
			{
				SendToCallerDiagnostic(companyDb.Diagnostic);
				companyList.Clear();
			}

			if (companyList.Count > 0)
			{
				CompaniesComboBox.DataSource    = companyList;
				CompaniesComboBox.DisplayMember = "Company";
				CompaniesComboBox.ValueMember   = "CompanyId";
			}

			if (CompaniesComboBox.Items.Count > 0)
			{
				// inizializzo la combobox con il nome dell'azienda da cui sono partito, se esiste
				CompaniesComboBox.SelectedIndex = 
					(CompaniesComboBox.FindStringExact(((CompanyItem)(SourceCompanyNameLabel.Tag)).Company) != -1)
					? CompaniesComboBox.FindStringExact(((CompanyItem)(SourceCompanyNameLabel.Tag)).Company)
					: 0;
			}
		}
		#endregion

		# region Eventi sui controls
		/// <summary>
		/// NewLoginRadioButton_CheckedChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void NewLoginRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			LoginsComboBox.Enabled	= !(((RadioButton)sender).Checked);
			NewLoginTextBox.Enabled = (((RadioButton)sender).Checked);
			LoginPasswordTextBox.Clear();

			LoginWinAuthCheckBox.Checked = UserWinAuthCheckBox.Checked;
			NewLoginTextBox.Enabled = !LoginWinAuthCheckBox.Checked;
		}

		//---------------------------------------------------------------------
		private void SelectLoginRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			LoginWinAuthCheckBox.Checked = !(((RadioButton)sender).Checked);
			LoginWinAuthCheckBox.Enabled = !(((RadioButton)sender).Checked);

			NewLoginTextBox.Enabled = !(((RadioButton)sender).Checked);
			if (!NewLoginTextBox.Enabled)
				NewLoginTextBox.Clear();
		}

		/// <summary>
		/// CompaniesComboBox_SelectedIndexChanged
		/// Memorizzo le informazioni dell'azienda su cui voglio clonare l'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void CompaniesComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (((ComboBox)sender).SelectedIndex >= 0)
				selectedCompany = (CompanyItem)((ComboBox)sender).SelectedItem;

			if (CompaniesComboBox.SelectedIndex != -1 && LoginsComboBox.Enabled && 
				selectedCompany != null && oldCompanyId != selectedCompany.CompanyId)
			{
				LoadLoginsComboBox();
				oldCompanyId = selectedCompany.CompanyId;
			}
		}

		/// <summary>
		/// UserNameTextBox_Leave
		/// Inizializzo il nome della login con il nome specificato per l'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void UserNameTextBox_Leave(object sender, System.EventArgs e)
		{
			if (UserWinAuthCheckBox.Checked)
				UserNameTextBox.Text = UserNameTextBox.Text.ToUpperInvariant();

			if (NewLoginRadioButton.Checked && NewLoginTextBox.Text.Length == 0)
				NewLoginTextBox.Text = ((TextBox)sender).Text;
		}

		/// <summary>
		/// LoginsComboBox_EnabledChanged
		/// Quando si abilita la combo carico le logins
		/// </summary>
		//---------------------------------------------------------------------
		private void LoginsComboBox_EnabledChanged(object sender, System.EventArgs e)
		{
			if (LoginsComboBox.Enabled && selectedCompany != null && oldCompanyId != selectedCompany.CompanyId)
			{
				LoadLoginsComboBox();
				oldCompanyId = selectedCompany.CompanyId;
			}
		}

		///<summary>
		/// UserWinAuthCheckBox_CheckedChanged
		/// Se l'utente e' WinAuth non faccio inserire la password e la pulisco
		///</summary>
		//---------------------------------------------------------------------
		private void UserWinAuthCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (LoginWinAuthCheckBox.Enabled)
				LoginWinAuthCheckBox.Checked = ((CheckBox)sender).Checked;

			if (LoginWinAuthCheckBox.Checked)
			{
				if (NewLoginRadioButton.Checked)
				{
					UserPasswordTextBox.Enabled = !NewLoginRadioButton.Checked;
					UserPasswordTextBox.Clear();
				}
			}

			UserPasswordTextBox.Enabled = !((CheckBox)sender).Checked;
			if (((CheckBox)sender).Checked)
				UserPasswordTextBox.Clear();

			if (((CheckBox)sender).Checked)
			{
				if (!string.IsNullOrEmpty(UserNameTextBox.Text))
					UserNameTextBox.Text = UserNameTextBox.Text.ToUpperInvariant();
				if (!string.IsNullOrEmpty(NewLoginTextBox.Text))
					NewLoginTextBox.Text = NewLoginTextBox.Text.ToUpperInvariant();
			}
		}

		///<summary>
		/// LoginWinAuthCheckBox_CheckedChanged
		/// Se la login e' WinAuth non faccio inserire la password e la pulisco
		///</summary>
		//---------------------------------------------------------------------
		private void LoginWinAuthCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			LoginPasswordTextBox.Enabled = !((CheckBox)sender).Checked;

			NewLoginRadioButton.Enabled = !((CheckBox)sender).Checked;
			SelectLoginRadioButton.Enabled = !((CheckBox)sender).Checked;

			if (((CheckBox)sender).Checked)
			{
				LoginPasswordTextBox.Clear();
				NewLoginTextBox.Enabled = false;

				if (UserWinAuthCheckBox.Checked && !string.IsNullOrEmpty(UserNameTextBox.Text))
					NewLoginTextBox.Text = UserNameTextBox.Text.ToUpperInvariant();
			}
			else
				if (NewLoginRadioButton.Checked)
				{
					NewLoginTextBox.Enabled = true;
					NewLoginTextBox.Clear();
				}
		}
		# endregion

		# region Procedura di clonazione
		/// <summary>
		/// Click sul pulsante di Clonazione utente
		/// </summary>
		//---------------------------------------------------------------------
		private void CloneUserButton_Click(object sender, System.EventArgs e)
		{
			diagnostic.Clear();

			// controllo se i dati inseriti nella form sono coerenti
			if (!CheckData())
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				return;
			}

			// faccio gli opportuni controlli preventivi sull'utente applicativo e sulla login di database
			if (!VerifyUserAndLogin())
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				return;
			}

			// se la login è nuova: la aggiungo al server SQL e la associo al database con i specifici ruoli
			if (NewLoginRadioButton.Checked)
			{
				if (NewAddLogin(LoginWinAuthCheckBox.Checked, NewLoginTextBox.Text, LoginPasswordTextBox.Text, false))
				{
					if (isDMSActivated && dmsToManage)
					{
						if (!NewAddLogin(LoginWinAuthCheckBox.Checked, NewLoginTextBox.Text, LoginPasswordTextBox.Text, true))
							return;
					}
				}
				else
					return;
			}
				
			// se la login esiste già: la associo al database con i specifici ruoli
			if (SelectLoginRadioButton.Checked)
			{
				CompanyLogin login = ((CompanyLogin)(LoginsComboBox.SelectedItem));
				if (login == null)
					return;

				if (NewGrantLogin(login.IsNTUser, login.Login, LoginPasswordTextBox.Text, false))
				{
					if (isDMSActivated && dmsToManage)
					{
						if (!NewGrantLogin(login.IsNTUser, login.Login, LoginPasswordTextBox.Text, true))
							return;
					}
				}
				else
					return;
			}

			// aggiorno le tabelle del database di sistema
			if (!UpdateSystemDBTables())
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				return;
			}

			// eventi per aggiornare il tree della Console
			if (OnModifyTree != null) 
				OnModifyTree(sender, ConstString.containerUsers);
			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies(sender, ConstString.containerCompanyUsers, selectedCompany.CompanyId);
			if (OnModifyTreeOfCompanies != null) 
				OnModifyTreeOfCompanies(sender, ConstString.containerCompanyRoles, selectedCompany.CompanyId);
			if (OnAfterClonedUserCompany != null) 
				OnAfterClonedUserCompany(selectedCompany.CompanyId);
		}
		# endregion

		#region LoadLoginsComboBox - Carica nella combo le Logins presenti sul server SQL
		/// <summary>
		/// LoadLoginsComboBox
		/// Carico nella combobox le login di database presenti sul server SQL, ovvero dove risiede
		/// il database associato all'azienda su cui voglio clonare l'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadLoginsComboBox()
		{
			LoginsComboBox.DataSource = null;
			LoginsComboBox.Items.Clear();
			Cursor.Current = Cursors.WaitCursor;

			if (selectedCompany == null)
				return;

			string companyConnectionString = CreateCompanyConnectionString();

			if (string.IsNullOrWhiteSpace(companyConnectionString)) 
			{
				NewLoginRadioButton.Checked = true;
				LoginsComboBox.Items.Clear();
				LoginsComboBox.Enabled = false;
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				return;
			}

			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole		+= new TransactSQLAccess.AddUserAuthenticatedFromConsole	(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole	+= new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole	(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole		+= new TransactSQLAccess.IsUserAuthenticatedFromConsole		(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp						+= new TransactSQLAccess.CallHelpFromPopUp(CallHelp);
			connSqlTransact.CurrentStringConnection = companyConnectionString;

			UserImpersonatedData dataToConnectionServer = connSqlTransact.LoginImpersonification
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
				NewLoginRadioButton.Checked = true;
				LoginsComboBox.Items.Clear();
				LoginsComboBox.Enabled = false;
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				Cursor.Current = Cursors.Default;
				return;
			}
			
			ArrayList loginsOfCompany = new ArrayList(); 
			if (!connSqlTransact.GetLogins(out loginsOfCompany)) // leggo tutte le logins
			{
				diagnostic.Set(connSqlTransact.Diagnostic);
				diagnostic.Set(DiagnosticType.Error, Strings.NotExistLogins);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				Cursor.Current = Cursors.Default;
				if (dataToConnectionServer != null)
					dataToConnectionServer.Undo();
				return;
			}

			ArrayList validLogins = new ArrayList(); // array delle sole login valide!
			if (loginsOfCompany != null && loginsOfCompany.Count > 0)
			{
				for (int i = 0; i < loginsOfCompany.Count; i++)
				{
					CompanyLogin companyLogin = (CompanyLogin)loginsOfCompany[i];
					// skippo il gruppo NT, l'utente NT (locale o no), l'utente sa, l'utente EasyLookSystem, l'utente Anonymous
					if ((companyLogin.IsNTGroup || 
						companyLogin.IsLocalNTUser || 
						companyLogin.IsSaUser || 
						companyLogin.IsNTUser) ||
						(string.Compare(companyLogin.Login, NameSolverStrings.GuestLogin, true, CultureInfo.InvariantCulture) == 0) ||
						(string.Compare(companyLogin.Login, NameSolverStrings.EasyLookSystemLogin, true, CultureInfo.InvariantCulture) == 0))
						continue;
					validLogins.Add(companyLogin);
				}

				if (validLogins != null && validLogins.Count > 0)
				{
					LoginsComboBox.DataSource = validLogins;
					LoginsComboBox.DisplayMember = "Login";
					LoginsComboBox.ValueMember = "IsNTUser";

					// Ora leggo la login di database associata all'utente che voglio clonare dalla tabella MSD_CompanyLogins
					// e se questa esiste sul server dell'azienda di destinazione mi posiziono su quella nella combobox delle login
					CompanyUserDb companyUser = new CompanyUserDb(sysDbConnString);
					companyUser.CurrentSqlConnection = this.sysDbConnection;
					ArrayList loginData = new ArrayList();
					companyUser.GetUserCompany(out loginData, this.treeLoginId, selectedCompany.CompanyId);

					string login = string.Empty, password = string.Empty;
					if (loginData.Count > 0)
					{
						CompanyUser companyDbo = (CompanyUser)loginData[0];
						login = companyDbo.DBDefaultUser;
						password = companyDbo.DBDefaultPassword;
					}

					if (login.Length > 0)
						LoginsComboBox.SelectedIndex = (LoginsComboBox.FindStringExact(login) != -1) ? LoginsComboBox.FindStringExact(login) : 0;
				}
			}

			if (dataToConnectionServer != null)
				dataToConnectionServer.Undo();

			Cursor.Current = Cursors.Default;
		}
		#endregion

		# region CheckData - controllo i dati che ha inserito l'utente nella form
		/// <summary>
		/// Controllo preventivo sui dati inseriti nella form, prima di eseguire la procedura vera e propria
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckData()
		{
			bool result = true;

			if (CompaniesComboBox.SelectedItem == null)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoEmptyValue, Strings.TargetCompany));
				result = false;
			}

			if (string.IsNullOrEmpty(UserNameTextBox.Text))
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoEmptyValue, Strings.User));
				result = false;
			}

			if ((SelectLoginRadioButton.Checked && LoginsComboBox.SelectedItem == null) ||
				(this.NewLoginRadioButton.Checked && string.IsNullOrEmpty(NewLoginTextBox.Text)))
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoEmptyValue, Strings.DBUser));
				result = false;
			}

			// array di appoggio con i caratteri invalidi
			char[] invalidChars = new char[] { '?', '\r', '\n', '*', '/', '<', '>', ':', '!', '|', '+', '[', ']', ',', '@', '=' };
			char[] winAuthChars = new char[] { '\'', Path.DirectorySeparatorChar };

			if (UserWinAuthCheckBox.Checked)
			{
				if (UserNameTextBox.Text.IndexOfAny(invalidChars) != -1)
				{
					diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInUserName);
					result = false;
				}
			}
			else
			{
				if (
					(UserNameTextBox.Text.IndexOfAny(invalidChars) != -1) ||
					(UserNameTextBox.Text.IndexOfAny(winAuthChars) != -1) ||
					(UserNameTextBox.Text.IndexOfAny(Path.GetInvalidPathChars()) != -1)
					)
				{
					diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInUserName);
					result = false;
				}
			}

			if (this.NewLoginRadioButton.Checked)
			{
				if (LoginWinAuthCheckBox.Checked)
				{
					if (NewLoginTextBox.Text.IndexOfAny(invalidChars) != -1)
					{
						diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInUserName);
						result = false;
					}
				}
				else
				{
					if (NewLoginTextBox.Text.IndexOfAny(invalidChars) != -1 ||
						NewLoginTextBox.Text.IndexOfAny(winAuthChars) != -1 ||
						NewLoginTextBox.Text.IndexOfAny(Path.GetInvalidPathChars()) != -1)
					{
						diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInUserName);
						result = false;
					}
				}
			}
				
			return result;
		}
		# endregion

		# region VerifyUserAndLogin - verifico che l'utente e la login indicati vadano bene
		/// <summary>
		/// Prima di procedere con la clonazione vera e propria devo effettuare ulteriori controlli:
		/// 1. l'utente indicato NON deve esistere nella tabella MSD_Logins
		/// 2. se si è scelto di inserire una nuova login di database, controllare che non esista già sul server
		///    e che non sia già stata associata al database
		/// 3. se la login è stata scelta tra le esistenti devo provare a connettermi con la password indicata
		/// </summary>
		//---------------------------------------------------------------------
		private bool VerifyUserAndLogin()
		{
			isTheSameCompany = (selectedCompany.CompanyId == ((CompanyItem)SourceCompanyNameLabel.Tag).CompanyId);

			//===========================================================================
			// CONTROLLO UTENTI APPLICATIVI
			//===========================================================================
			// 1. l'utente indicato NON deve esistere nella tabella MSD_Logins
			if (!CheckApplicationUser())
				return false;

			//===========================================================================
			// CONTROLLO LOGINS DI DATABASE
			//===========================================================================
			// 2. se si vuole inserire una nuova login di database, devo controllare che:
			//    - non esista già tra gli account di accesso al server
			//	  - non sia già stata associata come utente al database
 			// 3. per una login già esistente, devo provare a connettermi con la password indicata
			if (!CheckLoginNew())
				return false;

			return true;
		}

		///<summary>
		/// Vengono effettuati tutta una serie di controlli preventivi sull'utente applicativo
		/// in base al tipo di clonazione che si vuole effettuare
		///</summary>
		//---------------------------------------------------------------------
		private bool CheckApplicationUser()
		{
			bool result = true;

			UserDb user = new UserDb(this.sysDbConnString);
			user.CurrentSqlConnection = this.sysDbConnection;

			// se l'azienda di origine è uguale a quella di destinazione devo controllare che l'utente applicativo
			// specificato NON esista nella tabella MSD_Logins
			if (isTheSameCompany)
			{
				// 1. controllo che l'utente indicato NON esista nella tabella MSD_Logins
				if (user.ExistLoginAlsoDisabled(UserNameTextBox.Text))
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.ExistUser, UserNameTextBox.Text));
					return false;
				}
				else
				{
					// se l'utente applicativo deve essere inserito devo controllare anche che la sua password
					// soddisfi i parametri generali, relativamente alla complessità ed alla lunghezza.
					// solo se non e' in windows authentication
					if (!UserWinAuthCheckBox.Checked)
					{
						if (useStrongPwd && !UserPasswordTextBox.Text.IsStrongPassword(minPasswordLength))
						{
							diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInPwd);
							result = false;
						}

						if (UserPasswordTextBox.Text.Length < minPasswordLength)
						{
							diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoPasswordLength, minPasswordLength.ToString()));
							result = false;
						}
					}

					if (!result)
						return result;
				}
			}
			else
			{
				// se sto clonando su un'altra azienda, devo lasciare la possibilità di associare all'azienda di 
				// destinazione un utente applicativo che già è stato censito nel db di sistema ma non ancora associato.
				// in questo caso non controllo l'esistenza dell'utente, bensì faccio il match tra la password 
				// inserita e quella memorizzata nella MSD_Logins e faccio procedere solo se combaciano.
				ArrayList userData = new ArrayList();
				if (user.LoadFromLogin(out userData, UserNameTextBox.Text))
				{
					if (userData != null && userData.Count > 0)
					{
						existingUserItem = (UserItem)userData[0];

						// devo controllare però che l'utente non sia già associato all'azienda di destinazione,
						// altrimenti non procedo
						CompanyUserDb coUserDb = new CompanyUserDb(this.sysDbConnString);
						coUserDb.CurrentSqlConnection = this.sysDbConnection;
						if (coUserDb.ExistUser(existingUserItem.LoginId, selectedCompany.CompanyId) != 0)
						{
							diagnostic.Set(DiagnosticType.Error, string.Format(Strings.AlreadyExistLoginIntoDb, existingUserItem.Login));
							return false;
						}

						// se e' attivato il documentale e l'azienda di destinazione ha uno slave
						// devo anche controllare che non sia stato associato anche al dms
						if (isDMSActivated && selectedCompany.UseDBSlave)
						{
							SlaveLoginDb slaveLogin = new SlaveLoginDb();
							slaveLogin.ConnectionString = this.sysDbConnString;
							slaveLogin.CurrentSqlConnection = this.sysDbConnection;

							if (slaveLogin.ExistLoginForCompanyId(existingUserItem.LoginId, selectedCompany.CompanyId))
							{
								diagnostic.Set(DiagnosticType.Error, string.Format(Strings.AlreadyExistLoginIntoDb, existingUserItem.Login));
								return false;
							}
						}

						// faccio il match delle password, solo se non e' in windows authentication
						if (!UserWinAuthCheckBox.Checked)
						{
							if (string.Compare(existingUserItem.Password, UserPasswordTextBox.Text, false) != 0)
							{
								diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UserPasswordNotMatch, existingUserItem.Login));
								result = false;
							}
							else
								skipInsertUser = true;
						}
					}
				}
				else
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.UsersReading, UserNameTextBox.Text));
					result = false;
				}

				if (!result)
					return result;
			}

			return result;
		}

		///<summary>
		/// Vengono effettuati tutta una serie di controlli preventivi sulla login (nuova o esistente)
		/// in base al tipo di clonazione che si vuole effettuare
		///</summary>
		//---------------------------------------------------------------------
		private bool CheckLogin()
		{
			bool result = true;

			if (connSqlTransact == null)
			{
				connSqlTransact = new TransactSQLAccess();
				connSqlTransact.CurrentStringConnection = CreateCompanyConnectionString();
			}

			if (NewLoginRadioButton.Checked)
			{
				if (connSqlTransact.ExistLogin(NewLoginTextBox.Text))
				{
					// se l'utente in sicurezza integrata esiste gia' in SQL
					// non blocco e procedo
					if (!LoginWinAuthCheckBox.Checked)
					{
						diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.LoginAlreadyExist, NewLoginTextBox.Text));
						diagnostic.Set(connSqlTransact.Diagnostic);
						return false;
					}
				}

				// devo cmq controllare che la login specificata non sia già stata associata al database
				if (connSqlTransact.ExistLoginIntoDb(NewLoginTextBox.Text, selectedCompany.DbName))
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.AlreadyExistLoginIntoDb, NewLoginTextBox.Text));
					diagnostic.Set(connSqlTransact.Diagnostic);
					return false;
				}

				// se la login è nuova, prima devo controllare che la sua password
				// soddisfi i parametri generali, relativamente alla complessità ed alla lunghezza.
				// solo se non e' in windows authentication
				if (!LoginWinAuthCheckBox.Checked)
				{
					if (useStrongPwd && !LoginPasswordTextBox.Text.IsStrongPassword(minPasswordLength))
					{
						diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInPwd);
						result = false;
					}

					if (LoginPasswordTextBox.Text.Length < minPasswordLength)
					{
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoPasswordLength, minPasswordLength.ToString()));
						result = false;
					}
				}

				if (!result)
					return result;
			}

			// 3. per una login già esistente, devo provare a connettermi con la password indicata
			if (SelectLoginRadioButton.Checked)
			{
				TransactSQLAccess localSqlTransact = new TransactSQLAccess();
				// compongo la stringa di connessione al db aziendale con la login gia' esistente
				localSqlTransact.CurrentStringConnection =
					((CompanyLogin)(LoginsComboBox.SelectedItem)).IsNTUser
					? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, selectedCompany.DbServer, selectedCompany.DbName)
					: string.Format(NameSolverDatabaseStrings.SQLConnection, selectedCompany.DbServer, selectedCompany.DbName, ((CompanyLogin)(LoginsComboBox.SelectedItem)).Login, LoginPasswordTextBox.Text);

				// provo a connettermi
				// se non riesco a connettermi e viene ritornato l'errore nr. 4060 potrebbe
				// significare che la login esiste sul server ma che non è associata al database pertanto 
				// varrebbe la pena chiedere all'utente se desidere associare la login prima di clonare l'utente??
				if (!localSqlTransact.TryToConnect())
				{
					IDiagnosticItems items = localSqlTransact.Diagnostic.AllMessages();
					if (items != null)
					{
						foreach (DiagnosticItem item in items)
						{
							if (item.ExtendedInfo != null)
							{
								foreach (ExtendedInfoItem exItem in item.ExtendedInfo)
									if (string.Compare(Strings.Number, exItem.Name, true) == 0)
									{
										if (exItem.Info.ToString() == "4060")
										{
											// La login esiste sul server ma non è associata al database. 
											// Allora la associamo in automatico.
											result = true;
										}
										if (exItem.Info.ToString() == "18456")
										{
											diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CannotConnectWithLogin);
											result = false;
										}
									}
							}
						}
					}
				}
			}

			return result;
		}

		///<summary>
		/// Vengono effettuati tutta una serie di controlli preventivi sulla login (nuova o esistente)
		/// in base al tipo di clonazione che si vuole effettuare
		///</summary>
		//---------------------------------------------------------------------
		private bool CheckLoginNew()
		{
			bool result = true;

			// mi connetto al database aziendale, con le credenziali del dbowner
			result = ConnectToCompanyDatabase();

			// se non sono riuscita a connettermi al database aziendale non procedo
			if (!result)
				return result; // messaggio di errore

			// se il modulo dms e' attivato e l'azienda ha uno slave associato procedo con i controlli sul database
			if (isDMSActivated && selectedCompany.UseDBSlave)
			{
				result = ConnectToDmsDatabase();

				// se non sono riuscita a connettermi al database documentale non procedo
				if (!result)
					return result; // messaggio di errore

				dmsToManage = true;
			}

			// se la login e' nuova faccio altri controlli
			if (NewLoginRadioButton.Checked)
			{
				if (companyConnSqlTransact.ExistLogin(NewLoginTextBox.Text))
				{
					// se l'utente in sicurezza integrata esiste gia' in SQL non blocco e procedo
					if (!LoginWinAuthCheckBox.Checked)
					{
						diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.LoginAlreadyExist, NewLoginTextBox.Text));
						diagnostic.Set(companyConnSqlTransact.Diagnostic);
						return false;
					}
				}

				// devo cmq controllare che la login specificata non sia già stata associata al database
				if (companyConnSqlTransact.ExistLoginIntoDb(NewLoginTextBox.Text, selectedCompany.DbName))
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.AlreadyExistLoginIntoDb, NewLoginTextBox.Text));
					diagnostic.Set(companyConnSqlTransact.Diagnostic);
					return false;
				}

				// se la login è nuova, prima devo controllare che la sua password
				// soddisfi i parametri generali, relativamente alla complessità ed alla lunghezza.
				// solo se non e' in windows authentication
				if (!LoginWinAuthCheckBox.Checked)
				{
					if (useStrongPwd && !LoginPasswordTextBox.Text.IsStrongPassword(minPasswordLength))
					{
						diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInPwd);
						result = false;
					}

					if (LoginPasswordTextBox.Text.Length < minPasswordLength)
					{
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoPasswordLength, minPasswordLength.ToString()));
						result = false;
					}
				}

				if (dmsToManage)
				{
					if (dmsConnSqlTransact.ExistLogin(NewLoginTextBox.Text))
					{
						// se l'utente in sicurezza integrata esiste gia' in SQL non blocco e procedo
						if (!LoginWinAuthCheckBox.Checked)
						{
							diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.LoginAlreadyExist, NewLoginTextBox.Text));
							diagnostic.Set(dmsConnSqlTransact.Diagnostic);
							return false;
						}
					}

					// devo cmq controllare che la login specificata non sia già stata associata al database
					if (dmsConnSqlTransact.ExistLoginIntoDb(NewLoginTextBox.Text, selectedCompany.DbName))
					{
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.AlreadyExistLoginIntoDb, NewLoginTextBox.Text));
						diagnostic.Set(dmsConnSqlTransact.Diagnostic);
						return false;
					}
				}

				if (!result)
					return result;
			}

			// 3. per una login già esistente, devo provare a connettermi con la password indicata
			if (SelectLoginRadioButton.Checked)
			{
				TransactSQLAccess localSqlTransact = new TransactSQLAccess();
				
				// la login esiste sul server, quindi controllo se e' stata associata anche al db
				if (companyConnSqlTransact.ExistLoginIntoDb(((CompanyLogin)(LoginsComboBox.SelectedItem)).Login, selectedCompany.DbName))
				{
					// compongo la stringa di connessione al db aziendale con la login gia' esistente
					localSqlTransact.CurrentStringConnection =
						((CompanyLogin)(LoginsComboBox.SelectedItem)).IsNTUser
						? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, selectedCompany.DbServer, selectedCompany.DbName)
						: string.Format(NameSolverDatabaseStrings.SQLConnection, selectedCompany.DbServer, selectedCompany.DbName, ((CompanyLogin)(LoginsComboBox.SelectedItem)).Login, LoginPasswordTextBox.Text);

					// provo a connettermi al database aziendale con la login specificata
					if (!localSqlTransact.TryToConnect())
					{
						diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CannotConnectWithLogin);
						result = false;
					}
				}
				else
				{
					// se la login non e' associata al db devo cmq controllare che la pw della login sia corretta
					// compongo la stringa di connessione al master con la login gia' esistente
					localSqlTransact.CurrentStringConnection =
						((CompanyLogin)(LoginsComboBox.SelectedItem)).IsNTUser
						? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, selectedCompany.DbServer, DatabaseLayerConsts.MasterDatabase)
						: string.Format(NameSolverDatabaseStrings.SQLConnection, selectedCompany.DbServer, DatabaseLayerConsts.MasterDatabase, ((CompanyLogin)(LoginsComboBox.SelectedItem)).Login, LoginPasswordTextBox.Text);

					// provo a connettermi al database aziendale con la login specificata
					if (!localSqlTransact.TryToConnect())
					{
						diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CannotConnectWithLogin);
						result = false;
					}
				}

				// provo a connettermi al database documentale con la login specificata
				if (result && dmsToManage)
				{
					localSqlTransact.CurrentStringConnection =
						((CompanyLogin)(LoginsComboBox.SelectedItem)).IsNTUser
						? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, dmsServerName, dmsDatabaseName)
						: string.Format(NameSolverDatabaseStrings.SQLConnection, dmsServerName, dmsDatabaseName, ((CompanyLogin)(LoginsComboBox.SelectedItem)).Login, LoginPasswordTextBox.Text);

					// provo a connettermi
					if (!localSqlTransact.TryToConnect())
					{
						diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CannotConnectWithLogin);
						result = false;
					}
				}
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
			if (!companyDb.GetAllCompanyFieldsById(out companyData, selectedCompany.CompanyId))
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
				companyUser.GetUserCompany(out userDboData, dbOwnerId, selectedCompany.CompanyId);
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
			dbSlave.SelectSlaveForCompanyId(selectedCompany.CompanyId, out slaveItem);

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
			if (selectedCompany.UseDBSlave)
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

		# region AddLoginToCompany & GrantLoginToCompany
		/// <summary>
		/// AddLoginToCompany
		/// aggiunge la login agli account di accesso di SQL, associa la login al database con i ruoli
		/// </summary>
		//---------------------------------------------------------------------
		private bool AddLoginToCompany()
		{
			if (LoginWinAuthCheckBox.Checked)
				return
					(
					connSqlTransact.SPGrantLogin(NewLoginTextBox.Text) &&
					connSqlTransact.SPGrantDbAccess(NewLoginTextBox.Text, NewLoginTextBox.Text, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(NewLoginTextBox.Text, DatabaseLayerConsts.RoleDataWriter, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(NewLoginTextBox.Text, DatabaseLayerConsts.RoleDataReader, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(NewLoginTextBox.Text, DatabaseLayerConsts.RoleDbOwner, selectedCompany.DbName)
					);
			else
				return 
					(
					connSqlTransact.SPAddLogin(NewLoginTextBox.Text, LoginPasswordTextBox.Text, DatabaseLayerConsts.MasterDatabase) &&
					connSqlTransact.SPGrantDbAccess(NewLoginTextBox.Text, NewLoginTextBox.Text, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(NewLoginTextBox.Text, DatabaseLayerConsts.RoleDataWriter, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(NewLoginTextBox.Text, DatabaseLayerConsts.RoleDataReader, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(NewLoginTextBox.Text, DatabaseLayerConsts.RoleDbOwner, selectedCompany.DbName)
					);
		}

		/// <summary>
		/// GrantLoginToCompany
		/// associa la login al database con i ruoli
		/// </summary>
		//---------------------------------------------------------------------
		private bool GrantLoginToCompany()
		{
			CompanyLogin login = ((CompanyLogin)(LoginsComboBox.SelectedItem));
			if (login == null)
				return false;

			if (login.IsNTUser)
			{
				return 
					(
					connSqlTransact.SPGrantDbAccess(login.Login, login.Login, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(login.Login, DatabaseLayerConsts.RoleDataWriter, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(login.Login, DatabaseLayerConsts.RoleDataReader, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(login.Login, DatabaseLayerConsts.RoleDbOwner, selectedCompany.DbName)
					);
			}
			else
			{
				return 
					(
					connSqlTransact.SPGrantDbAccess(login.Login, login.Login, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(login.Login, DatabaseLayerConsts.RoleDataWriter, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(login.Login, DatabaseLayerConsts.RoleDataReader, selectedCompany.DbName) &&
					connSqlTransact.SPAddRoleMember(login.Login, DatabaseLayerConsts.RoleDbOwner, selectedCompany.DbName)
					);
			}
		}
		# endregion

		# region UpdateSystemDBTables - aggiorno i dati nelle tabelle del db di sistema
		/// <summary>
		/// Aggiorno le tabelle del database di sistema, ovvero:
		/// INSERT INTO MSD_Logins (solo se devo inserire il nuovo utente applicativo)
		/// INSERT INTO MSD_CompanyLogins
		/// INSERT INTO MSD_SlaveLogins (solo se gestisco il documentale)
		/// CALL SP MSD_CloneCompanyLoginGrants
		/// </summary>
		//---------------------------------------------------------------------
		private bool UpdateSystemDBTables()
		{
			int lastLoginId = -1;

			if (!skipInsertUser)
			{
				// inserisco un record nella tabella MSD_Logins per aggiungere il nuovo utente applicativo
				UserDb newUser = new UserDb(this.sysDbConnString);
				newUser.CurrentSqlConnection = this.sysDbConnection;

				if (!newUser.Add
					(
					UserWinAuthCheckBox.Checked, // windowsAuthentication
					UserNameTextBox.Text,		// login
					UserWinAuthCheckBox.Checked ? string.Empty : UserPasswordTextBox.Text,	// password
					selectedUser.Description,
					selectedUser.ExpiredDatePassword,
					selectedUser.Disabled,
					UserWinAuthCheckBox.Checked ? false : selectedUser.UserMustChangePassword,
					UserWinAuthCheckBox.Checked ? false : selectedUser.UserCannotChangePassword,
					UserWinAuthCheckBox.Checked ? false : selectedUser.ExpiredDateCannotChange,
					UserWinAuthCheckBox.Checked ? false : selectedUser.PasswordNeverExpired,
					selectedUser.PreferredLanguage,
					selectedUser.ApplicationLanguage,
					selectedUser.EMailAddress,
					UserWinAuthCheckBox.Checked ? false : selectedUser.WebAccess,
					selectedUser.SmartClientAccess,
					selectedUser.ConcurrentAccess,
					selectedUser.Locked,
					selectedUser.PrivateAreaAdmin,
                    "0"/*,
                    selectedUser.BalloonBlockedType*/
					))
				{
					diagnostic.Set(newUser.Diagnostic);
					return false;
				}

				lastLoginId = newUser.LastLoginId();
			}

			// inserisco un record nella tabella MSD_CompanyLogins per associare il nuovo utente applicativo
			// all'azienda, indicando anche la login di database
			CompanyUserDb newCompanyUser = new CompanyUserDb(this.sysDbConnString);
			newCompanyUser.CurrentSqlConnection = this.sysDbConnection;
			if (!newCompanyUser.Add
				(
				selectedCompany.CompanyId,
				(skipInsertUser) ? existingUserItem.LoginId : lastLoginId.ToString(),
				false,
				false,
				(NewLoginRadioButton.Checked) ? NewLoginTextBox.Text : ((CompanyLogin)(LoginsComboBox.SelectedItem)).Login,
				LoginPasswordTextBox.Text,
				(NewLoginRadioButton.Checked) ? LoginWinAuthCheckBox.Checked : ((CompanyLogin)(LoginsComboBox.SelectedItem)).IsNTUser
				))
			{
				diagnostic.Set(newCompanyUser.Diagnostic);
				return false;
			}

			if (isDMSActivated && dmsToManage)
			{
				CompanyDBSlave dbSlave = new CompanyDBSlave();
				dbSlave.ConnectionString = this.sysDbConnString;
				dbSlave.CurrentSqlConnection = this.sysDbConnection;

				CompanyDBSlaveItem slaveItem = new CompanyDBSlaveItem();
				if (!dbSlave.SelectSlaveForCompanyId(selectedCompany.CompanyId, out slaveItem))
				{
					diagnostic.Set(dbSlave.Diagnostic);
					return false;
				}

				// inserisco un record nella tabella MSD_SlaveLogins per associare il nuovo utente applicativo
				// allo slave aziendale, indicando anche la login di database
				SlaveLoginDb newSlaveLogin = new SlaveLoginDb();
				newSlaveLogin.ConnectionString = this.sysDbConnString;
				newSlaveLogin.CurrentSqlConnection = this.sysDbConnection;
				if (!newSlaveLogin.Add
					(
					slaveItem.SlaveId,
					(skipInsertUser) ? existingUserItem.LoginId : lastLoginId.ToString(),
					(NewLoginRadioButton.Checked) ? NewLoginTextBox.Text : ((CompanyLogin)(LoginsComboBox.SelectedItem)).Login,
					LoginPasswordTextBox.Text,
					(NewLoginRadioButton.Checked) ? LoginWinAuthCheckBox.Checked : ((CompanyLogin)(LoginsComboBox.SelectedItem)).IsNTUser
					))
				{
					diagnostic.Set(newSlaveLogin.Diagnostic);
					return false;
				}
			}

			// call sp per clonare i grant di Security sull'utente di destinazione (INSERT INTO MSD_ObjectGrants)
			if (!CallCloneCompanyLoginGrants(lastLoginId))
				return false;

			// se sto clonando un utente sulla stessa azienda ed l'utente di partenza era stato associato 
			// a uno o più ruoli di Security, allora assegno questa associazione anche all'utente di destinazione
			if (isTheSameCompany && !CloneRoleAssociationToUser(lastLoginId))
				return false;

			// Gestione SecurityLight
			// se l'utente di origine aveva degli accessi negati con il plugIn SecurityLight, allora imposto gli
			// stessi accessi anche all'utente di destinazione
			if (!CloneSLDeniedAccesses(lastLoginId))
				return false;

			return true;
		}
		# endregion

		# region Esecuzione della sp MSD_CloneCompanyLoginGrants
		/// <summary>
		/// CallCloneCompanyLoginGrants
		/// richiama la stored procedure MSD_CloneCompanyLoginGrants, che si occupa di "clonare" tutti
		/// gli oggetti protetti anche per il nuovo utente
		/// </summary>
		//---------------------------------------------------------------------
		private bool CallCloneCompanyLoginGrants(int dstLoginId)
		{
			bool result = false;

			SqlCommand myCommand  = new SqlCommand();
			myCommand.Connection  = this.sysDbConnection;
			myCommand.CommandText = "MSD_CloneCompanyLoginGrants";
			myCommand.CommandType = CommandType.StoredProcedure;
			myCommand.Parameters.AddWithValue("@par_srccompanyid",	Int32.Parse(treeCompanyId));
			myCommand.Parameters.AddWithValue("@par_srcloginid",	Int32.Parse(treeLoginId));
			myCommand.Parameters.AddWithValue("@par_dstcompanyid",	Int32.Parse(selectedCompany.CompanyId));
			myCommand.Parameters.AddWithValue("@par_dstloginid",	dstLoginId);
			
			try
			{
				myCommand.ExecuteNonQuery();
				result = true;
			}
			catch(SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description,		e.Message);
				extendedInfo.Add(Strings.Procedure,		e.Procedure);
				extendedInfo.Add(Strings.Server,			e.Server);
				extendedInfo.Add(Strings.Number,			e.Number);
				extendedInfo.Add(Strings.LineNumber,		e.LineNumber);
				extendedInfo.Add(Strings.Function,			"CloneUser.CallCloneCompanyRoleLogin");
				extendedInfo.Add(Strings.StoredProcedure,	"MSD_CloneCompanyLoginGrants");
				extendedInfo.Add(Strings.DefinedInto,		"SysAdminPlugIn");
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.UserCloned, dstLoginId), extendedInfo);
			}

			return result;
		}
		# endregion

		# region CloneRoleAssociationToUser (clono anche l'associazione utente al ruolo, se esiste)
		/// <summary>
		/// CloneRoleAssociationToUser
		/// Se l'utente di partenza era stato associato a uno o più ruoli del Security, li associo anche
		/// all'utente di destinazione
		/// </summary>
		//---------------------------------------------------------------------
		private bool CloneRoleAssociationToUser(int dstLoginId)
		{
			bool result = false;

			string insertCommand = 
				@"INSERT INTO MSD_CompanyRolesLogins(CompanyId,	LoginId, RoleId)
				  SELECT @dstcompanyid, @dstloginid, RoleId
				  FROM MSD_CompanyRolesLogins
				  WHERE CompanyId = @srccompanyid AND LoginId = @srcloginid";
			
			SqlCommand myCommand  = new SqlCommand();
			myCommand.Connection  = this.sysDbConnection;
			myCommand.CommandText = insertCommand;
			myCommand.Parameters.AddWithValue("@dstcompanyid",	Int32.Parse(selectedCompany.CompanyId));
			myCommand.Parameters.AddWithValue("@dstloginid",	dstLoginId);
			myCommand.Parameters.AddWithValue("@srccompanyid",	Int32.Parse(treeCompanyId));
			myCommand.Parameters.AddWithValue("@srcloginid",	Int32.Parse(treeLoginId));
			
			try
			{
				myCommand.ExecuteNonQuery();
				result = true;
			}
			catch(SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description,		e.Message);
				extendedInfo.Add(Strings.Procedure,		e.Procedure);
				extendedInfo.Add(Strings.Server,			e.Server);
				extendedInfo.Add(Strings.Number,			e.Number);
				extendedInfo.Add(Strings.LineNumber,		e.LineNumber);
				extendedInfo.Add(Strings.Function,			"CloneUser.CloneRoleAssociationToUser");
				extendedInfo.Add(Strings.DefinedInto,		"SysAdminPlugIn");
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.RoleCloned, dstLoginId), extendedInfo);
			}

			return result;
		}
		# endregion

		# region CloneSLDeniedAccesses (clono anche gli accessi del SecurityLight, se esistono)
		/// <summary>
		/// Se l'utente di origine aveva delle impostazioni con il SecurityLight, allora li clono anche
		/// sull'utente di destinazione (INSERT INTO MSD_SLDeniedAccesses)
		/// </summary>
		//---------------------------------------------------------------------
		private bool CloneSLDeniedAccesses(int dstLoginId)
		{
			bool result = false;

			string insertCommand = 
				@"IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[MSD_SLDeniedAccesses]') 
				AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
				BEGIN
				INSERT INTO MSD_SLDeniedAccesses(ObjectId, CompanyId, UserId)
				  SELECT ObjectId, @dstcompanyid, @dstloginid
				  FROM MSD_SLDeniedAccesses
				  WHERE CompanyId = @srccompanyid AND UserId = @srcloginid
				END";
			
			SqlCommand myCommand  = new SqlCommand();
			myCommand.Connection  = this.sysDbConnection;
			myCommand.CommandText = insertCommand;
			myCommand.Parameters.AddWithValue("@dstcompanyid",	Int32.Parse(selectedCompany.CompanyId));
			myCommand.Parameters.AddWithValue("@dstloginid",	dstLoginId);
			myCommand.Parameters.AddWithValue("@srccompanyid",	Int32.Parse(treeCompanyId));
			myCommand.Parameters.AddWithValue("@srcloginid",	Int32.Parse(treeLoginId));
			
			try
			{
				myCommand.ExecuteNonQuery();
				result = true;
			}
			catch(SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description,		e.Message);
				extendedInfo.Add(Strings.Procedure,		e.Procedure);
				extendedInfo.Add(Strings.Server,			e.Server);
				extendedInfo.Add(Strings.Number,			e.Number);
				extendedInfo.Add(Strings.LineNumber,		e.LineNumber);
				extendedInfo.Add(Strings.Function,			"CloneUser.CloneSLDeniedAccesses");
				extendedInfo.Add(Strings.DefinedInto,		"SysAdminPlugIn");
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.RoleCloned, dstLoginId), extendedInfo);
			}

			return result;
		}
		# endregion

		# region Gestione help ed invio diagnostica al chiamante
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
	}
}