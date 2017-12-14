using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Security;
using System.Threading;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

using Microarea.Console.Core.PlugIns;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// User
	/// Gestione degli Utenti Applicativi
	/// Both the DirectoryEntry component and DirectorySearcher component require that you have the ADSI SDK or ADSI runtime installed 
	/// on your computer in order to create applications with their functionality. 
	/// ADSI 2.5 is installed by default with Windows 2000 or Windows XP.
	/// If you are using a previous version of Windows, you can install the SDK yourself from the Microsoft Web site.
	/// L'indirizzo è il seguente : http://www.microsoft.com/ntserver/nts/downloads/other/ADSI25/default.asp
	/// </summary>
	//=========================================================================
	public partial class User : PlugInsForm
	{
		#region Variabili e proprietà
		private string connectionString = string.Empty;
		private SqlConnection currentConnection = null;
		private BrandLoader brandLoader = null;
		private Diagnostic diagnostic = new Diagnostic("SysAdminPlugIn.User");

		private string domainSelected = string.Empty;
		private string userToAdd = string.Empty;
		private string connectedServer = string.Empty;

		private string originalPassword = string.Empty;
		private bool isModifiedPassword = false;
		private int MinPasswordLength = 0;
		private int PasswordDuration = 0;
		private bool UseStrongPwd = false;

		// mi serve per sapere quale operazione di salvataggio richiamare
		private bool isNewUser = true;

		private bool isGuest = false;
		private bool isAzureSQLDatabase = false;
		public Diagnostic Diagnostic { get { return diagnostic; } }
		#endregion

		#region Delegati ed Eventi
		public EventHandler OnAddGuestUser;
		public EventHandler OnDeleteGuestUser;

		public delegate void ModifyTree(object sender, string nodeType);
		public event ModifyTree OnModifyTree;

		public delegate void AfterDisabledCheckedChanged(object sender, string loginId, bool disabled);
		public event AfterDisabledCheckedChanged OnAfterDisabledCheckedChanged;

		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;

		public delegate void CallHelp(object sender, string nameSpace, string searchParameter);
		public event CallHelp OnCallHelp;

		public delegate void TraceAction(string company, string login, TraceActionType type, string processName);
		public event TraceAction OnTraceAction;
		#endregion

		#region Costruttori
		/// <summary>
		/// Costruttore NUOVO UTENTE
		/// </summary>
		//---------------------------------------------------------------------
		public User(SysAdminStatus status, bool isAzureSQLDatabase, PathFinder aPathFinder, BrandLoader aBrandLoader)
		{
			InitializeComponent();

			isNewUser = true;
			this.isAzureSQLDatabase = isAzureSQLDatabase;

			if (aPathFinder == null || InstallationData.ServerConnectionInfo == null)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadConfigFile);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				return;
			}

			PasswordDuration	= InstallationData.ServerConnectionInfo.PasswordDuration;
			MinPasswordLength	= InstallationData.ServerConnectionInfo.MinPasswordLength;
			UseStrongPwd		= InstallationData.ServerConnectionInfo.UseStrongPwd;
			isModifiedPassword = false;

			expirationDate.Value = DateTime.Now.AddDays(InstallationData.ServerConnectionInfo.PasswordDuration);
			expirationDate.Text = DateTime.Now.AddDays(InstallationData.ServerConnectionInfo.PasswordDuration).ToShortDateString();

			this.connectionString = status.ConnectionString;
			this.currentConnection = status.CurrentConnection;

			if (!this.isAzureSQLDatabase)
			{
				string currentServer = status.ServerName;
				if (!string.IsNullOrWhiteSpace(status.ServerIstanceName))
					currentServer += Path.DirectorySeparatorChar + status.ServerIstanceName;
				connectedServer = (Path.GetDirectoryName(currentServer).Length == 0) ? currentServer : Path.GetDirectoryName(currentServer);
			}

			cbDomains.Enabled = false;
			cbLocked.Enabled = false;
			cbLocked.Checked = false;
			nrLoginFailedCount.Text = "0";
			nrLoginFailedCount.Enabled = false;
			LblLoginFailedCount.Enabled = false;
			PopolateComboDomains();

			//popolo la lingua di applicazione
			cultureApplicationCombo.LoadLanguages();
			//popolo la lingua di interfaccia
			cultureUICombo.LoadLanguagesUI();
			cbDomains.SelectedIndex = 0;

			if (String.Compare(aPathFinder.Edition, Edition.Enterprise.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				CbSmarClientAccess.Visible = false;
				CBConcurrentAccess.Checked = true;
			}

			LabelTitle.Text = Strings.TitleInsertingNewUser;

			SettingsToolTips();

			State = StateEnums.View;

			brandLoader = aBrandLoader;

			if (isAzureSQLDatabase)
				InitControlsForAzure();
		}

		/// <summary>
		/// Costruttore MODIFICA UTENTE
		/// </summary>
		//---------------------------------------------------------------------
		public User(SysAdminStatus status, bool isAzureSQLDatabase, string loginId, PathFinder aPathFinder, BrandLoader aBrandLoader)
		{
			InitializeComponent();
			
			diagnostic.Clear();

			isNewUser = false;
			this.isAzureSQLDatabase = isAzureSQLDatabase;

			if (aPathFinder == null || InstallationData.ServerConnectionInfo == null)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadConfigFile);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				return;
			}

			MinPasswordLength = InstallationData.ServerConnectionInfo.MinPasswordLength;
			PasswordDuration = InstallationData.ServerConnectionInfo.PasswordDuration;
			UseStrongPwd = InstallationData.ServerConnectionInfo.UseStrongPwd;
			isModifiedPassword = false;

			if (!this.isAzureSQLDatabase)
			{
				string currentServer = status.ServerName;
				if (!String.IsNullOrWhiteSpace(status.ServerIstanceName))
					currentServer += Path.DirectorySeparatorChar + status.ServerIstanceName;
				connectedServer = (Path.GetDirectoryName(currentServer).Length == 0) ? currentServer : Path.GetDirectoryName(currentServer);
			}

			connectionString = status.ConnectionString;
			currentConnection = status.CurrentConnection;

			tbPassword.Enabled = true;
			cbDomains.Enabled = false;

			PopolateComboDomains();
			//popolo la lingua di applicazione
			cultureApplicationCombo.LoadLanguages();
			//popolo la lingua di interfaccia
			cultureUICombo.LoadLanguagesUI();

			UserDb dbUser = new UserDb();
			dbUser.ConnectionString = connectionString;
			dbUser.CurrentSqlConnection = this.currentConnection;
			ArrayList fieldsUser = new ArrayList();
			if (!dbUser.GetAllUserFieldsById(out fieldsUser, loginId))
			{
				if (dbUser.Diagnostic.Error || dbUser.Diagnostic.Warning || dbUser.Diagnostic.Information)
					diagnostic.Set(dbUser.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers);

				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				fieldsUser.Clear();
			}

			if (fieldsUser.Count > 0)
			{
				UserItem utenteItem = (UserItem)fieldsUser[0];
				LabelTitle.Text = string.Format(Strings.TitleModifyingUser, utenteItem.Login);
                // SetBalloonBlockedMessageStatus(utenteItem.BalloonBlockedType);
				tbLoginId.Text = utenteItem.LoginId;
				tbLogin.Text = utenteItem.Login;
				if (!utenteItem.WindowsAuthentication)
				{
					//in questo modo la pwd, anche se vuota, è sempre una serie di asterischi
					if (utenteItem.Password.Length > 0)
						tbPassword.Text = utenteItem.Password;
					else
						tbPassword.Text = (UseStrongPwd) ? ConstString.passwordEmpty : ConstString.passwordEmpty; // ?? era già così??? perchè???
					originalPassword = tbPassword.Text;
				}
				else
				{
					lblPassword.Enabled = false;
					tbPassword.Enabled = false;
					//Utente in sicurezza integrata non si può connettere con EasyLook (e in generale alle applicazioni web)
					CbWebAccess.Enabled = false;
				}

				tbDescrizione.Text = utenteItem.Description;
				expirationDate.Value = Convert.ToDateTime(utenteItem.ExpiredDatePassword);
				expirationDate.Text = Convert.ToDateTime(utenteItem.ExpiredDatePassword).ToShortDateString();
				TbEmailAddress.Text = utenteItem.EMailAddress;
				cbDisabled.Checked = utenteItem.Disabled;
				radioWindowsAuthentication.Checked = utenteItem.WindowsAuthentication;
				radioSQLServerAuthentication.Checked = !utenteItem.WindowsAuthentication;
				btnSearch.Enabled = utenteItem.WindowsAuthentication;

				if (utenteItem.WindowsAuthentication)
				{
					string domain = Path.GetDirectoryName(utenteItem.Login);
					if (cbDomains.Items.Contains(domain))
						cbDomains.SelectedIndex = cbDomains.Items.IndexOf(domain);
					else
					{
						cbDomains.Items.Insert(0, domain);
						cbDomains.SelectedIndex = 0;
					}
					radioSQLServerAuthentication.Enabled = false;
				}
				else
					cbDomains.Enabled = false;

				cbUserCannotChangePassword.Checked = utenteItem.UserCannotChangePassword;
				cbUserMustChangePassword.Checked = utenteItem.UserMustChangePassword;
				cbExpiredDateCannotChange.Checked = utenteItem.ExpiredDateCannotChange;
				cbPasswordNeverExpired.Checked = utenteItem.PasswordNeverExpired;
				cbPrivateAreaAdmin.Checked = utenteItem.PrivateAreaAdmin;

				//prima gli attributi erano indipendenti.
				//ora sono esclusivi, imposto i  flag con questo ordine 
				//in modo da amantenere una gerarchia in caso ci fossero attributi multipli a true
				/*1*/
				CbWebAccess.Checked = utenteItem.WebAccess;
				/*2*/
				CBConcurrentAccess.Checked = utenteItem.ConcurrentAccess;
				/*3*/
				//se ent le named non esistono
				if (String.Compare(aPathFinder.Edition, Edition.Enterprise.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					if (!CBConcurrentAccess.Checked)
						CBConcurrentAccess.Checked = utenteItem.SmartClientAccess;
					CbSmarClientAccess.Visible = false;
				}
				else
					CbSmarClientAccess.Checked = utenteItem.SmartClientAccess;

				if (utenteItem.Locked)
				{
					cbLocked.Enabled = true;
					cbLocked.Checked = true;
					nrLoginFailedCount.Enabled = true;
					LblLoginFailedCount.Enabled = true;
				}
				else
				{
					cbLocked.Enabled = false;
					cbLocked.Checked = false;
					nrLoginFailedCount.Enabled = false;
					LblLoginFailedCount.Enabled = false;
				}

				nrLoginFailedCount.Text = utenteItem.NrLoginFailedCount.ToString();
				string preferredLanguage = utenteItem.PreferredLanguage;
				string applicationLanguage = utenteItem.ApplicationLanguage;

				cultureUICombo.SetUILanguage(preferredLanguage);
				cultureApplicationCombo.ApplicationLanguage = applicationLanguage;

				if (dbUser.UserIsDbowner(tbLoginId.Text))
				{
					tbLogin.Enabled = false;
					btnSearch.Enabled = false;
					radioWindowsAuthentication.Enabled = false;
					lblDomain.Enabled = false;
					cbDomains.Enabled = false;
					radioSQLServerAuthentication.Enabled = false;
					cbDisabled.Enabled = cbDisabled.Checked;
				}
				else
					if (dbUser.IsAssociated(tbLoginId.Text))
					{
						tbLogin.Enabled = false;
						btnSearch.Enabled = false;
						radioWindowsAuthentication.Enabled = false;
						lblDomain.Enabled = false;
						cbDomains.Enabled = false;
						radioSQLServerAuthentication.Enabled = false;
					}

				if (string.Compare(tbLogin.Text, NameSolverStrings.GuestLogin, true, CultureInfo.InvariantCulture) == 0)
				{
					isGuest = true;
					tbLogin.Enabled = false;
					btnSearch.Enabled = false;
					groupBoxAuthenticationMode.Enabled = false;
					EmailAddress.Enabled = true;
					CbSmarClientAccess.Enabled = false;
					CBConcurrentAccess.Enabled = false;
					CbWebAccess.Enabled = false;
					cbUserMustChangePassword.Enabled = false;
					cbUserCannotChangePassword.Enabled = false;
					cbPasswordNeverExpired.Enabled = false;
					cbPrivateAreaAdmin.Enabled = false;
					cbExpiredDateCannotChange.Enabled = false;
					cbDisabled.Enabled = true;
					isModifiedPassword = false;
				}
			}

			SettingsToolTips();
			State = StateEnums.View;

			brandLoader = aBrandLoader;

			if (isAzureSQLDatabase)
				InitControlsForAzure();
		}

		/// <summary>
		/// Costruttore UTENTE GUEST o Anonymous
		/// </summary>
		//---------------------------------------------------------------------
		public User(SysAdminStatus status, PathFinder aPathFinder, BrandLoader aBrandLoader, string userSystemName)
		{
			InitializeComponent();

			isNewUser = true;

			if (string.Compare(userSystemName, NameSolverStrings.GuestLogin, true, CultureInfo.InvariantCulture) == 0)
				isGuest = true;

			if (aPathFinder == null || InstallationData.ServerConnectionInfo == null)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.CannotReadConfigFile);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				return;
			}

			connectionString = status.ConnectionString;
			currentConnection = status.CurrentConnection;

			tbLogin.Enabled = false;
			btnSearch.Enabled = false;
			groupBoxAuthenticationMode.Enabled = false;
			radioSQLServerAuthentication.Checked = true;
			tbPassword.Enabled = false;
			cbDisabled.Checked = false;
			cbDisabled.Enabled = true;
			cbLocked.Checked = false;
			cbLocked.Enabled = false;
			nrLoginFailedCount.Text = "0";
			nrLoginFailedCount.Enabled = false;
			LblLoginFailedCount.Enabled = false;

			if (isGuest)
			{
				tbLogin.Text = NameSolverStrings.GuestLogin;
				tbPassword.Text = NameSolverStrings.GuestPwd;
				EmailAddress.Enabled = true;

				CbSmarClientAccess.Checked = false;
				CbSmarClientAccess.Enabled = false;
				CBConcurrentAccess.Enabled = false;
				CBConcurrentAccess.Checked = false;
				CbWebAccess.Checked = true;
				CbWebAccess.Enabled = false;
				cbExpiredDateCannotChange.Checked = false;
			}

			cbUserMustChangePassword.Checked = false;
			cbUserCannotChangePassword.Checked = true;
			cbPasswordNeverExpired.Checked = true;
			cbUserMustChangePassword.Enabled = false;
			cbUserCannotChangePassword.Enabled = false;
			cbPasswordNeverExpired.Enabled = false;
			cbExpiredDateCannotChange.Enabled = false;
			isModifiedPassword = false;

			PopolateComboDomains();
			//popolo la lingua di Applicazione
			cultureApplicationCombo.LoadLanguages();
			//popolo la lingua di interfaccia
			cultureUICombo.LoadLanguagesUI();

			cbDomains.SelectedIndex = 0;
 
			LabelTitle.Text = Strings.TitleInsertingNewUser;

			SettingsToolTips();

			State = StateEnums.View;

			brandLoader = aBrandLoader;
		}
		#endregion

		#region SettingsToolTips
		/// <summary>
		/// SettingsToolTips
		/// </summary>
		//---------------------------------------------------------------------
		private void SettingsToolTips()
		{
			toolTip.SetToolTip(btnSearch, Strings.ButtonNtUserToolTip);
			toolTip.SetToolTip(tbLogin, Strings.UserNameToolTip);
			toolTip.SetToolTip(radioWindowsAuthentication, Strings.NTUsersToolTip);
			toolTip.SetToolTip(radioSQLServerAuthentication, Strings.SQLUsersToolTip);
			toolTip.SetToolTip(cbDomains, Strings.UserDomainToolTip);
			toolTip.SetToolTip(tbPassword, Strings.UserPasswordToolTip);
			toolTip.SetToolTip(tbDescrizione, Strings.UserDescriptionToolTip);
			toolTip.SetToolTip(cultureUICombo, Strings.PreferredLanguageUserToolTip);
			toolTip.SetToolTip(TbEmailAddress, Strings.EmailUserToolTip);
			toolTip.SetToolTip(cbUserMustChangePassword, Strings.UserMustChangePwdUserToolTip);
			toolTip.SetToolTip(cbUserCannotChangePassword, Strings.UserCannotChangePwdToolTip);
			toolTip.SetToolTip(cbPasswordNeverExpired, Strings.UserPwdNeverExpiredToolTip);
			toolTip.SetToolTip(cbPrivateAreaAdmin, Strings.UserPrivateAreaAdminToolTip);

			toolTip.SetToolTip(cbExpiredDateCannotChange, Strings.UserPwdDateCannotChangeToolTip);
			toolTip.SetToolTip(expirationDate, Strings.UserPwdExpirationDateToolTip);
			toolTip.SetToolTip(cbDisabled, Strings.UserDisabledToolTip);
		}
		#endregion

		#region CheckValidator - Verifica i dati inseriti dall'utente
		//---------------------------------------------------------------------
		private bool CheckValidator(string loginName)
		{
			bool result = true;
			if (string.IsNullOrWhiteSpace(loginName))
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoEmptyValue, "Login"));
				result = false;
			}
			if (string.Compare(loginName, ConstString.AllUsers, true, CultureInfo.InvariantCulture) == 0)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.WrongLoginName, ConstString.AllUsers));
				result = false;
			}
			if (string.Compare(loginName, ConstString.Standard, true, CultureInfo.InvariantCulture) == 0)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.WrongLoginName, ConstString.Standard));
				result = false;
			}
			if (string.Compare(loginName, NameSolverStrings.GuestLogin, true, CultureInfo.InvariantCulture) == 0 && !isGuest)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.WrongLoginName, NameSolverStrings.GuestLogin));
				result = false;
			}

			// ulteriore controllo per i nomi di login non ammessi in Azure
			if (isAzureSQLDatabase && AzureLoginNameChecker.IsValidAzureLogin(loginName))
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.WrongLoginName, loginName));
				result = false;
			}

			if (radioSQLServerAuthentication.Checked)
			{
				if ((loginName.IndexOfAny(Path.GetInvalidPathChars()) != -1) ||
					(loginName.IndexOfAny(new char[] { '?', '\r', '\n', '*', '\'', Path.DirectorySeparatorChar, '/', '<', '>', ':', '!', '|', '+', '[', ']', ',', '@', '=' }) != -1))
				{
					diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInUserName);
					result = false;
				}

				if (UseStrongPwd && !tbPassword.Text.IsStrongPassword(this.MinPasswordLength))
				{
					diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInPwd);
					result = false;
				}
			}
			else
			{
				//leggo gli utenti dalla textbox multiline
				string[] logins = loginName.Split(';');
				for (int i = 0; i <= logins.Length - 1; i++)
				{
					string loginCurrent = logins[i];

					string[] elementOfLoginName = loginCurrent.Split(Path.DirectorySeparatorChar);
					if (elementOfLoginName.Length == 1)
					{
						diagnostic.Set(DiagnosticType.Error, Strings.EmptyDomain);
						result = false;
					}
					else
					{
						if (elementOfLoginName[0].IndexOfAny(new char[] { '?', '*', '\'', Path.DirectorySeparatorChar, '/', '<', '>', ':', '!', '|', '+', '[', ']', ',', '@', '=' }) != -1)
						{
							diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInDomainUser);
							result = false;
						}
						if (elementOfLoginName[1].IndexOfAny(new char[] { '?', '*', '\'', Path.DirectorySeparatorChar, '/', '<', '>', ':', '!', '|', '+', '[', ']', ',', '@', '=' }) != -1)
						{
							diagnostic.Set(DiagnosticType.Error, Strings.WrongCharactersInUserName);
							result = false;
						}
						if (elementOfLoginName[0].Length == 0)
						{
							diagnostic.Set(DiagnosticType.Error, Strings.EmptyDomain);
							result = false;
						}
						if (elementOfLoginName[1].Length == 0)
						{
							diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoEmptyValue, "Login"));
							result = false;
						}
					}
				}
			}

			if (!cbPasswordNeverExpired.Checked && expirationDate.Value.ToShortDateString().Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoEmptyValue, Strings.ExpirationDate));
				expirationDate.Value = DateTime.Now.AddDays(PasswordDuration);
				expirationDate.Text = DateTime.Now.AddDays(PasswordDuration).ToShortDateString();
				result = false;
			}

			//se la password non è vuota ed è diversa da Empty faccio il test sulla lunghezza
			//if (string.Compare(tbPassword.Text, ConstString.passwordEmpty, true) != 0 && tbPassword.Text.Length != 0 && radioSQLServerAuthentication.Checked)
			//!cbEnableEmptyPwd.Checked &&
			if (radioSQLServerAuthentication.Checked)
			{
				//controllo che la lunghezza sia corretta
				if (tbPassword.Text.Length < MinPasswordLength)
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoPasswordLength, MinPasswordLength.ToString()));
					result = false;
				}
			}

			if (expirationDate.Value.ToShortDateString().Length > 0)
			{
				try
				{
					DateTime tempDT = DateTime.Parse(expirationDate.Value.ToShortDateString(), Thread.CurrentThread.CurrentCulture);
				}
				catch (InvalidCastException)
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoDateValue, Strings.ExpirationDate));
					result = false;
				}
				catch (ArgumentNullException)
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoDateValue, Strings.ExpirationDate));
					result = false;
				}
				catch (FormatException)
				{
					diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoDateValue, Strings.ExpirationDate));
					result = false;
				}
			}

			if (radioWindowsAuthentication.Checked)
			{
				if (cbDomains.SelectedIndex == -1 || cbDomains.SelectedItem == null)
				{
					if (cbDomains.Items.Count == 0)
					{
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoEmptyValue, Strings.Domain));
						result = false;
					}
					else
						cbDomains.SelectedItem = cbDomains.Items[0];
				}
			}

			if (!result)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (this.OnSendDiagnostic != null)
				{
					this.OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}

			return result;
		}
		#endregion

		/// <summary>
		/// Se la licenza e' per Azure vado a nascondere i control relativi
		/// per la Win Authentication
		/// </summary>
		//---------------------------------------------------------------------
		private void InitControlsForAzure()
		{
			btnSearch.Visible = false;
			radioWindowsAuthentication.Visible = false;
			radioSQLServerAuthentication.Location = radioWindowsAuthentication.Location;
			lblDomain.Visible = false;
			lblPassword.Location = lblDomain.Location;
			cbDomains.Visible = false;
			tbPassword.Location = cbDomains.Location;
		}

		/// <summary>
		/// Metodo esposto esternamente per dare la possibilita'
		/// di richiamare il Save dalla toolbar dell'AdminConsole
		/// Al suo interno richiama il metodo apposito, a seconda del fatto
		/// se sono in modifica di un utente esistente o se ne sto creando uno nuovo.
		/// </summary>
		//---------------------------------------------------------------------
		public void Save(object sender, System.EventArgs e)
		{
			if (isNewUser)
				SaveNewUser(sender, e);
			else
				SaveExistingUser(sender, e);
		}

		#region SaveExistingUser - Salva le modifiche di un Utente Applicativo precedentemente inserito
		/// <summary>
		/// SaveExistingUser
		/// </summary>
		//---------------------------------------------------------------------
		private void SaveExistingUser(object sender, System.EventArgs e)
		{
			string preferredLanguage = string.Empty, applicationLanguage = string.Empty;

			isModifiedPassword = (string.Compare(originalPassword, tbPassword.Text, false) != 0);

			tbLogin.Text = tbLogin.Text.Trim();	
			string loginName = tbLogin.Text;

			if (CheckValidator(loginName))
			{
				if (cultureUICombo.SelectedIndex != -1)
					preferredLanguage = cultureUICombo.SelectedLanguageValue;
				applicationLanguage = cultureApplicationCombo.ApplicationLanguage;

				if (radioWindowsAuthentication.Checked)
				{
					//leggo gli utenti dalla textbox multiline
					string[] logins = loginName.Split(';');
					for (int i = 0; i < logins.Length; i++)
					{
						string loginCurrent = logins[i];
						int ifCanInsert = ExistUser(loginCurrent);
						if (ifCanInsert == -1)
						{
							string msg = string.Format(Strings.CannotVerifyNTUser, Path.GetFileName(loginCurrent), Path.GetDirectoryName(loginCurrent));
							string msgAsk = string.Concat(msg + ".", Strings.AskIfInsertingNTUser);
							DialogResult askIfContinue = MessageBox.Show(this, msgAsk, Strings.Error, MessageBoxButtons.YesNo, MessageBoxIcon.Error);
							if (askIfContinue == DialogResult.No)
							{
								string message = string.Format(Strings.NoExistUser, loginName);
								diagnostic.Set(DiagnosticType.Error, message);
								DiagnosticViewer.ShowDiagnostic(diagnostic);
								if (OnSendDiagnostic != null)
								{
									OnSendDiagnostic(this, diagnostic);
									diagnostic.Clear();
								}
								continue;
							}
						}

						if (ifCanInsert == -1 || ifCanInsert == 1)
						{
							UserDb dbUser = new UserDb();
							dbUser.ConnectionString = this.connectionString;
							dbUser.CurrentSqlConnection = this.currentConnection;
							string passwordToModify = string.Empty;
							if (string.Compare(tbPassword.Text, ConstString.passwordEmpty, true, CultureInfo.InvariantCulture) != 0)
								passwordToModify = tbPassword.Text;

							bool result = dbUser.Modify
								(
								tbLoginId.Text,
								radioWindowsAuthentication.Checked,
								loginCurrent,
								passwordToModify,
								tbDescrizione.Text,
								expirationDate.Value.ToShortDateString(),
								cbDisabled.Checked,
								cbUserMustChangePassword.Checked,
								cbUserCannotChangePassword.Checked,
								cbExpiredDateCannotChange.Checked,
								cbPasswordNeverExpired.Checked,
								preferredLanguage,
								applicationLanguage,
								TbEmailAddress.Text,
								CbWebAccess.Checked,
								CbSmarClientAccess.Checked,
								CBConcurrentAccess.Checked,
								cbLocked.Checked,
								cbPrivateAreaAdmin.Checked,
								nrLoginFailedCount.Text
								);

							if (!result)
							{
								if (dbUser.Diagnostic.Error || dbUser.Diagnostic.Warning || dbUser.Diagnostic.Information)
								{
									diagnostic.Set(dbUser.Diagnostic);
									DiagnosticViewer.ShowDiagnostic(diagnostic);
									if (OnSendDiagnostic != null)
									{
										OnSendDiagnostic(sender, diagnostic);
										diagnostic.Clear();
									}
								}
								continue;
							}
                            //Se l'utente fosse stato prima named sarebbe  il caso di vedere se aveva delle cal assegnate e cancellare l'assegnazione.
                            if (!CbSmarClientAccess.Checked) dbUser.RemoveReserverdCal(tbLoginId.Text);
						}
						else
						{
							MessageBox.Show(this, string.Format(Strings.NoExistUser, loginName), Strings.AddNewUser, MessageBoxButtons.OK, MessageBoxIcon.Error);
							continue;
						}

					}
					State = StateEnums.View;
					OnModifyTree?.Invoke(sender, ConstString.containerUsers);
				}
				else
				{
					if (isModifiedPassword)
					{
						//chiedo conferma della password
						ConfirmPassword confirmPassword = new ConfirmPassword();
						if (string.Compare(tbPassword.Text, ConstString.passwordEmpty, true, CultureInfo.InvariantCulture) == 0)
							confirmPassword.OldPassword = string.Empty;
						else
							confirmPassword.OldPassword = tbPassword.Text;
						confirmPassword.Focus();
						confirmPassword.ShowDialog();

						if (confirmPassword.DialogResult != DialogResult.OK)
						{
							State = StateEnums.Editing;
							return;
						}
						if (confirmPassword.Diagnostic.Error || confirmPassword.Diagnostic.Warning || confirmPassword.Diagnostic.Information)
						{
							diagnostic.Set(confirmPassword.Diagnostic);
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							State = StateEnums.Editing;
						}
					}

					UserDb dbUser = new UserDb();
					dbUser.ConnectionString = this.connectionString;
					dbUser.CurrentSqlConnection = this.currentConnection;
					string passwordToModify = string.Empty;

					if (string.Compare(tbPassword.Text, ConstString.passwordEmpty, true, CultureInfo.InvariantCulture) != 0)
						passwordToModify = tbPassword.Text;

					bool result = dbUser.Modify
						(
							tbLoginId.Text,
							radioWindowsAuthentication.Checked,
							loginName,
							passwordToModify,
							tbDescrizione.Text,
							expirationDate.Value.ToShortDateString(),
							cbDisabled.Checked,
							cbUserMustChangePassword.Checked,
							cbUserCannotChangePassword.Checked,
							cbExpiredDateCannotChange.Checked,
							cbPasswordNeverExpired.Checked,
							preferredLanguage,
							applicationLanguage,
							TbEmailAddress.Text,
							CbWebAccess.Checked,
							CbSmarClientAccess.Checked,
							CBConcurrentAccess.Checked,
							cbLocked.Checked,
							cbPrivateAreaAdmin.Checked,
                            nrLoginFailedCount.Text
						);

					if (!result)
					{
						if (dbUser.Diagnostic.Error || dbUser.Diagnostic.Warning || dbUser.Diagnostic.Information)
						{
							diagnostic.Set(dbUser.Diagnostic);
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							if (OnSendDiagnostic != null)
							{
								OnSendDiagnostic(sender, diagnostic);
								diagnostic.Clear();
							}
						}
						State = StateEnums.Editing;
						return;
					}
                    //Se l'utente fosse stato prima named sarebbe  il caso di vedere se aveva delle cal assegnate e cancellare l'assegnazione.
                    if (!CbSmarClientAccess.Checked)
						dbUser.RemoveReserverdCal(tbLoginId.Text);
				}

				State = StateEnums.View;
				OnAfterDisabledCheckedChanged?.Invoke(sender, tbLoginId.Text, cbDisabled.Checked);

				//cerco nelle CompanyLogins se l'utente è stato assegnato a qualche company, se si metto il disabled = il check del disabled
				CompanyUserDb companyUserDisabled = new CompanyUserDb();
				companyUserDisabled.ConnectionString = connectionString;
				companyUserDisabled.CurrentSqlConnection = currentConnection;

				if (!companyUserDisabled.ModifyDisableStatusCompanyUsers(tbLoginId.Text, cbDisabled.Checked))
				{
					diagnostic.Set(companyUserDisabled.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(sender, diagnostic);
						diagnostic.Clear();
					}
				}

				//ho modificato la pwd effettuo il trace dell'operazione
				if (isModifiedPassword)
					OnTraceAction?.Invoke(string.Empty, loginName, TraceActionType.ChangePassword, DatabaseLayerConsts.MicroareaConsole);

				OnModifyTree?.Invoke(sender, ConstString.containerCompanies);
			}
		}
		#endregion

		#region SaveNewUser - Inserisco un nuovo Utente Applicativo
		/// <summary>
		/// SaveNewUser
		/// </summary>
		//---------------------------------------------------------------------
		private void SaveNewUser(object sender, EventArgs e)
		{
			tbLogin.Text = tbLogin.Text.Trim();	// ensures robustness against bad coding practices
			string loginName = tbLogin.Text;

			if (CheckValidator(loginName))
			{
				State = StateEnums.Processing;
				string preferredLanguage = string.Empty, applicationLanguage = string.Empty;
				if (cultureUICombo.SelectedIndex != -1)
					preferredLanguage = cultureUICombo.SelectedLanguageValue;
				applicationLanguage = cultureApplicationCombo.ApplicationLanguage;

				if (radioWindowsAuthentication.Checked)
				{
					//leggo gli utenti dalla textbox multiline
					string[] logins = loginName.Split(';');
					for (int i = 0; i <= logins.Length - 1; i++)
					{
						string loginCurrent = logins[i];
						int ifCanInsert = ExistUser(loginCurrent);
						if (ifCanInsert == -1)
						{
							string msg = string.Format(Strings.CannotVerifyNTUser, Path.GetFileName(loginCurrent), Path.GetDirectoryName(loginCurrent));
							string msgAsk = string.Concat(msg + ".", Strings.AskIfInsertingNTUser);
							DialogResult askIfContinue = MessageBox.Show(this, msgAsk, Strings.Error, MessageBoxButtons.YesNo, MessageBoxIcon.Error);
							if (askIfContinue == DialogResult.No)
							{
								string message = string.Format(Strings.NoExistUser, loginCurrent);
								diagnostic.Set(DiagnosticType.Error, message);
								DiagnosticViewer.ShowDiagnostic(diagnostic);
								if (OnSendDiagnostic != null)
								{
									OnSendDiagnostic(this, diagnostic);
									diagnostic.Clear();
								}
								continue;
							}
						}

						if (ifCanInsert == -1 || ifCanInsert == 1)
						{
							UserDb dbUser = new UserDb();
							dbUser.ConnectionString = this.connectionString;
							dbUser.CurrentSqlConnection = this.currentConnection;
							if (!dbUser.ExistLogin(loginCurrent))
							{
								bool resultIns = dbUser.Add
									(
									radioWindowsAuthentication.Checked,
									loginCurrent,
									tbPassword.Text,
									tbDescrizione.Text,
									expirationDate.Value.ToShortDateString(),
									cbDisabled.Checked,
									cbUserMustChangePassword.Checked,
									cbUserCannotChangePassword.Checked,
									cbExpiredDateCannotChange.Checked,
									cbPasswordNeverExpired.Checked,
									preferredLanguage,
									applicationLanguage,
									TbEmailAddress.Text,
									CbWebAccess.Checked,
									CbSmarClientAccess.Checked,
									CBConcurrentAccess.Checked,
									cbLocked.Checked,
									cbPrivateAreaAdmin.Checked,
                                    nrLoginFailedCount.Text 
									);

								if (!resultIns)
								{
									diagnostic.Set(dbUser.Diagnostic);
									DiagnosticViewer.ShowDiagnostic(diagnostic);
									if (OnSendDiagnostic != null)
									{
										OnSendDiagnostic(sender, diagnostic);
										diagnostic.Clear();
									}
									continue;
								}
								else
									//Leggo l'id appena inserito
									tbLoginId.Text = dbUser.LastLoginId().ToString();
							}
							else
							{
								string messageUserExist = string.Format(Strings.ExistUser, loginCurrent);
								diagnostic.Set(DiagnosticType.Error, messageUserExist);
								DiagnosticViewer.ShowDiagnostic(diagnostic);
								if (OnSendDiagnostic != null)
								{
									OnSendDiagnostic(this, diagnostic);
									diagnostic.Clear();
								}
								continue;
							}
						}
						else
						{
							string messageUserNotExist = string.Format(Strings.NoExistUser, loginCurrent);
							MessageBox.Show(this, messageUserNotExist, Strings.AddNewUser, MessageBoxButtons.OK, MessageBoxIcon.Error);
							continue;
						}
					}
					State = StateEnums.View;
					OnModifyTree?.Invoke(sender, ConstString.containerUsers);
				}
				else if (!isGuest)
				{
					//chiedo conferma della password
					ConfirmPassword confirmPassword = new ConfirmPassword();
					confirmPassword.OldPassword = tbPassword.Text;
					confirmPassword.Focus();
					confirmPassword.ShowDialog();

					if (confirmPassword.DialogResult != DialogResult.OK)
					{
						State = StateEnums.Editing;
						return;
					}
					else
					{
						if (confirmPassword.Diagnostic.Error || confirmPassword.Diagnostic.Warning || confirmPassword.Diagnostic.Information)
						{
							diagnostic.Set(confirmPassword.Diagnostic);
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							if (OnSendDiagnostic != null)
							{
								OnSendDiagnostic(this, diagnostic);
								diagnostic.Clear();
							}
							State = StateEnums.Editing;
							return;
						}

						UserDb dbUser = new UserDb();
						dbUser.ConnectionString = connectionString;
						dbUser.CurrentSqlConnection = currentConnection;
						if (!dbUser.ExistLogin(loginName))
						{
							bool resultIns = dbUser.Add
								(
									radioWindowsAuthentication.Checked,
									loginName,
									tbPassword.Text,
									tbDescrizione.Text,
									expirationDate.Value.ToShortDateString(),
									cbDisabled.Checked,
									cbUserMustChangePassword.Checked,
									cbUserCannotChangePassword.Checked,
									cbExpiredDateCannotChange.Checked,
									cbPasswordNeverExpired.Checked,
									preferredLanguage,
									applicationLanguage,
									TbEmailAddress.Text,
									CbWebAccess.Checked,
									CbSmarClientAccess.Checked,
									CBConcurrentAccess.Checked,
									cbLocked.Checked,
									cbPrivateAreaAdmin.Checked,
                                    nrLoginFailedCount.Text
								);
							if (!resultIns)
							{
								diagnostic.Set(dbUser.Diagnostic);
								DiagnosticViewer.ShowDiagnostic(diagnostic);
								if (OnSendDiagnostic != null)
								{
									OnSendDiagnostic(sender, diagnostic);
									diagnostic.Clear();
								}
								State = StateEnums.Editing;
								return;
							}

							State = StateEnums.View;
							OnModifyTree?.Invoke(sender, ConstString.containerUsers);
							//Leggo l'id appena inserito
							tbLoginId.Text = dbUser.LastLoginId().ToString();
						}
						else
						{
							string message = string.Format(Strings.ExistUser, loginName);
							diagnostic.Set(DiagnosticType.Error, message);
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							if (OnSendDiagnostic != null)
							{
								OnSendDiagnostic(sender, diagnostic);
								diagnostic.Clear();
							}
							State = StateEnums.Editing;
						}
					}
				}
				else if (isGuest)
				{
					UserDb dbUser = new UserDb();
					dbUser.ConnectionString = connectionString;
					dbUser.CurrentSqlConnection = currentConnection;
					if (!dbUser.ExistLogin(loginName))
					{
						bool resultIns = dbUser.Add
							(
							radioWindowsAuthentication.Checked,
							loginName,
							tbPassword.Text,
							tbDescrizione.Text,
							expirationDate.Value.ToShortDateString(),
							cbDisabled.Checked,
							cbUserMustChangePassword.Checked,
							cbUserCannotChangePassword.Checked,
							cbExpiredDateCannotChange.Checked,
							cbPasswordNeverExpired.Checked,
							preferredLanguage,
							applicationLanguage,
							TbEmailAddress.Text,
							CbWebAccess.Checked,
							CbSmarClientAccess.Checked,
							CBConcurrentAccess.Checked,
							cbLocked.Checked,
							cbPrivateAreaAdmin.Checked,
							nrLoginFailedCount.Text
							);

						if (!resultIns)
						{
							diagnostic.Set(dbUser.Diagnostic);
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							if (OnSendDiagnostic != null)
							{
								OnSendDiagnostic(sender, diagnostic);
								diagnostic.Clear();
							}
							State = StateEnums.Editing;
							return;
						}
						if (isGuest && resultIns)
						{
							if (OnAddGuestUser != null)
								OnAddGuestUser(sender, e);
						}

						State = StateEnums.View;
						if (OnModifyTree != null)
							OnModifyTree(sender, ConstString.containerUsers);
						//Leggo l'id appena inserito
						tbLoginId.Text = dbUser.LastLoginId().ToString();
					}
					else
					{
						string message = string.Format(Strings.ExistUser, loginName);
						diagnostic.Set(DiagnosticType.Error, message);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(sender, diagnostic);
							diagnostic.Clear();
						}
						State = StateEnums.Editing;
					}
				}

				OnAfterDisabledCheckedChanged?.Invoke(sender, tbLoginId.Text, cbDisabled.Checked);
			}
		}
		#endregion

		#region Delete - Cancello un Utente Applicativo
		/// <summary>
		/// Delete
		/// </summary>
		//---------------------------------------------------------------------
		public void Delete(object sender, System.EventArgs e)
		{
			UserDb dbUser = new UserDb();
			dbUser.ConnectionString = connectionString;
			dbUser.CurrentSqlConnection = currentConnection;
			
			string loginDeleted = string.Empty;

			ArrayList dataUser = new ArrayList();
			bool result = dbUser.GetAllUserFieldsById(out dataUser, tbLoginId.Text);
			if (dataUser.Count != 0 && result)
			{
				UserItem userItem = (UserItem)dataUser[0];
				loginDeleted = userItem.Login;
			}

			result = dbUser.Delete(tbLoginId.Text);
			if (!result)
			{
				diagnostic.Set(dbUser.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(sender, diagnostic);
					diagnostic.Clear();
				}
			}
			else
			{
				OnTraceAction?.Invoke(string.Empty, loginDeleted, TraceActionType.DeleteUser, DatabaseLayerConsts.MicroareaConsole);
				//devo fare il reload dei utenti
				OnModifyTree?.Invoke (sender, ConstString.containerUsers);
				//devo anche fare il reload degli utenti delle company e dei ruoli
				OnModifyTree?.Invoke(sender, ConstString.containerCompanies);

				if (isGuest && result)
					OnDeleteGuestUser?.Invoke(sender, e);
			}
		}
		#endregion

		#region Eventi sui Controlli della Form

		#region radioWindowsAuthentication_CheckedChanged - Selezionata l'autenticazione NT
		//---------------------------------------------------------------------
		private void radioWindowsAuthentication_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				radioSQLServerAuthentication.Checked = false;
				btnSearch.Enabled = true;
				tbPassword.Enabled = false;
				cbDomains.Enabled = true;
				//disabilito tutte le check 
				//relative alla password
				cbUserCannotChangePassword.Checked = false;
				cbUserMustChangePassword.Checked = false;
				cbPasswordNeverExpired.Checked = false;
				cbExpiredDateCannotChange.Checked = false;
				cbUserCannotChangePassword.Enabled = false;
				cbUserMustChangePassword.Enabled = false;
				cbPasswordNeverExpired.Enabled = false;
				cbExpiredDateCannotChange.Enabled = false;
				lblExpirationDate.Enabled = false;
				expirationDate.Enabled = false;
				tbPassword.Text = string.Empty;
				cbDomains.SelectedIndex = -1;
				//seleziono il primo dominio dall'elenco
				if (cbDomains.Items.Count > 0)
					cbDomains.SelectedIndex = 0;
				//disabilito la checbox WebAccess - Richiesto da EasyLook
				//che non accetta utenti in sicurezza integrata
				//abilito una delle due altre check (named se è visible, altrimenti vuol dire che è solo floating)
				if (CbSmarClientAccess.Visible)
					CbSmarClientAccess.Checked = true;
				else
					CBConcurrentAccess.Checked = true;
				CbWebAccess.Enabled = false;
			}
		}
		#endregion

		#region radioSQLServerAuthentication_CheckedChanged - Selezionata l'autenticazione
		//---------------------------------------------------------------------
		private void radioSQLServerAuthentication_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				radioWindowsAuthentication.Checked = false;
				btnSearch.Enabled = false;
				tbPassword.Enabled = true;
				cbDomains.Enabled = false;
				CbWebAccess.Enabled = true;
				//abilito tutte le check relative alla password
				cbUserCannotChangePassword.Checked = false;
				cbUserMustChangePassword.Checked = false;
				cbPasswordNeverExpired.Checked = false;
				cbExpiredDateCannotChange.Checked = false;
				cbUserCannotChangePassword.Enabled = true;
				cbUserMustChangePassword.Enabled = true;
				cbPasswordNeverExpired.Enabled = true;
				cbExpiredDateCannotChange.Enabled = true;
				lblExpirationDate.Enabled = true;
				expirationDate.Enabled = true;
				if (!string.IsNullOrWhiteSpace(tbLogin.Text))
				{
					string[] DomainAndLogin = tbLogin.Text.Split(Path.DirectorySeparatorChar);
					if (DomainAndLogin.Length > 1)
						tbLogin.Text = DomainAndLogin[1];
				}
			}
		}
		#endregion

		#region btnSearch_Click - Premuto il bottone di Search degli utenti di dominio
		/// <summary>
		/// carica una finestra con tutti gli utenti NT
		/// </summary>
		//---------------------------------------------------------------------
		private void btnSearch_Click(object sender, System.EventArgs e)
		{
			if (radioWindowsAuthentication.Checked && cbDomains.SelectedItem != null)
			{
				Cursor.Current = Cursors.WaitCursor;
				domainSelected = cbDomains.SelectedItem.ToString();
				UsersBrowser usersBrowser = new UsersBrowser(domainSelected, this.connectedServer, true);
				usersBrowser.OnAfterUserSelected += new UsersBrowser.AfterUserSelected(this.AfterUserSelected);
				usersBrowser.OnOpenHelpFromPopUp += new UsersBrowser.OpenHelpFromPopUp(this.SendCompleteHelp);
				usersBrowser.ShowDialog();
				Cursor.Current = Cursors.Default;
			}
		}
		#endregion

		#region cbDomains_Enter - Caricamento della Combo cbDomains
		/// <summary>
		/// cbDomains_Enter
		/// </summary>
		//---------------------------------------------------------------------
		private void cbDomains_Enter(object sender, System.EventArgs e)
		{
			if (cbDomains.Items.Count == 0)
			{
				cbDomains.Items.Clear();
				string sCompName = (connectedServer != System.Net.Dns.GetHostName())
									? connectedServer
									: System.Net.Dns.GetHostName();

				cbDomains.Items.Add(sCompName);
				string sDomainName = SystemInformation.UserDomainName;
				if (string.Compare(sCompName, sDomainName, true, CultureInfo.InvariantCulture) != 0)
					cbDomains.Items.Add(sDomainName);
			}
		}
		#endregion

		#region PopolateComboDomains - Carica nella Combo i Domini/Workgroup a cui il computer appartiene
		/// <summary>
		/// PopolateComboDomains
		/// </summary>
		//---------------------------------------------------------------------
		private void PopolateComboDomains()
		{
			string sCompName = string.Empty;
			cbDomains.Items.Clear();

			if (string.Compare(connectedServer.ToUpper(CultureInfo.InvariantCulture), "LocalHost".ToUpper(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture) == 0)
				sCompName = System.Net.Dns.GetHostName();
			else
				sCompName = (connectedServer != System.Net.Dns.GetHostName())
							? connectedServer
							: System.Net.Dns.GetHostName();

			cbDomains.Items.Add(sCompName);
			string sDomainName = SystemInformation.UserDomainName;

			if (string.Compare(sCompName, sDomainName, true, CultureInfo.InvariantCulture) != 0)
				cbDomains.Items.Add(sDomainName);
		}
		#endregion

		#region cbDomains_SelectedIndexChanged - Selezione di un Dominio e/o workgroup
		/// <summary>
		/// cbDomains_SelectedIndexChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void cbDomains_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if ((cbDomains.SelectedItem != null) && (radioWindowsAuthentication.Checked))
			{
				string domainSelected = cbDomains.SelectedItem.ToString();
				string[] DomainAndLogin = tbLogin.Text.Split(Path.DirectorySeparatorChar);
				if (DomainAndLogin.Length == 1)
					tbLogin.Text = domainSelected + Path.DirectorySeparatorChar + DomainAndLogin[0];
				else
				{
					if (string.Compare(DomainAndLogin[0], domainSelected, true, CultureInfo.InvariantCulture) != 0)
						tbLogin.Text = domainSelected + Path.DirectorySeparatorChar + DomainAndLogin[1];
				}
			}
		}
		#endregion

		#region cbPasswordNeverExpired_CheckedChanged - Selezionato Password non spira mai
		/// <summary>
		/// cbPasswordNeverExpired_CheckedChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void cbPasswordNeverExpired_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
			if (((CheckBox)sender).Checked)
			{
				cbExpiredDateCannotChange.Checked = false;
				cbExpiredDateCannotChange.Enabled = false;
				lblExpirationDate.Enabled = false;
				expirationDate.Enabled = false;
			}
			else
			{
				cbExpiredDateCannotChange.Enabled = true;
				lblExpirationDate.Enabled = true;
				expirationDate.Enabled = true;
			}

			if (expirationDate.Text.Length == 0)
			{
				expirationDate.Value = DateTime.Now.AddDays(PasswordDuration);
				expirationDate.Text = expirationDate.Value.ToShortDateString();
			}
		}
		#endregion

		#region cbExpiredDateCannotChange_CheckedChanged - Selezionato la data di scadenza non può essere modificata
		/// <summary>
		/// cbExpiredDateCannotChange_CheckedChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void cbExpiredDateCannotChange_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
			if (((CheckBox)sender).Checked)
			{
				cbPasswordNeverExpired.Checked = false;
				cbPasswordNeverExpired.Enabled = false;
			}
			else
				cbPasswordNeverExpired.Enabled = true;
		}
		#endregion

		#region cbUserMustChangePassword_CheckedChanged - Selezionato Utente deve cambiare la password alla prox login
		//---------------------------------------------------------------------
		private void cbUserMustChangePassword_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
			cbUserCannotChangePassword.Enabled = !((CheckBox)sender).Checked;
		}
		#endregion

		#region cbUserCannotChangePassword_CheckedChanged - Selezionato Utente non può cambiare la password
		//---------------------------------------------------------------------
		private void cbUserCannotChangePassword_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
			cbUserMustChangePassword.Enabled = !((CheckBox)sender).Checked;
		}
		#endregion

		#endregion

		#region OnLoad - Caricamento della Form
		/// <summary>
		/// OnLoad
		/// Quando carico la form, se sto inserendo un nuovo utente dò il focus alla login, sennò no
		/// </summary>
		//---------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!isGuest)
			{
				if (radioWindowsAuthentication.Checked)
				{
					string[] DomainAndLogin = tbLogin.Text.Split(Path.DirectorySeparatorChar);
					if (DomainAndLogin.Length == 1)
						tbLogin.Focus();
				}
				else
				{
					if (tbLogin.Text.Length == 0)
						tbLogin.Focus();
				}
			}
			else
				this.tbDescrizione.Focus();

			if (brandLoader != null)
			{
				string brandedCompany = brandLoader.GetCompanyName();
				if (brandedCompany != null && brandedCompany.Length > 0)
				{
					groupBoxAuthenticationMode.Text = groupBoxAuthenticationMode.Text.Replace(NameSolverStrings.Microarea, brandedCompany);
					radioSQLServerAuthentication.Text = radioSQLServerAuthentication.Text.Replace(NameSolverStrings.Microarea, brandedCompany);
				}
			}
		}
		#endregion

		#region Altre Funzioni

		#region ExistUser - Verifica se l'Utente selezionato esiste effettivamente nel dominio / Workgroup
		/// <summary>
		/// ExistUser
		/// Si connette al dominio/Computer e verifica che l'utente esista
		/// Connect to a user on a computer.		   "WinNT://<domain name>/<computer name>/<user name>". 
		/// If you are connecting to a local computer, "WinNT://<computer name>/<user name>". 
		/// Connect to a group on a computer.          "WinNT://<domain name>/<computer name>/<group name>". 
		/// If you are connecting to a local computer, "WinNT://<computer name>/<group name>". 
		/// </summary>
		/// <param name="user"></param>
		/// <returns>-1 se securityException
		///           0 se non trovato
		///           1 se trovato
		/// </returns>
		//---------------------------------------------------------------------
		public int ExistUser(string userOrGroup)
		{
			int exist = 0;
			if (userOrGroup.Length == 0)
				return exist;
			if (Path.GetDirectoryName(userOrGroup).Length == 0)
				return exist;
			if (Path.GetFileName(userOrGroup).Length == 0)
				return exist;

			Cursor.Current = Cursors.WaitCursor;
			string sADsPath = string.Empty;
			string computerName = SystemInformation.ComputerName.ToUpper(CultureInfo.InvariantCulture);
			string domainUserName = Path.GetDirectoryName(userOrGroup).ToUpper(CultureInfo.InvariantCulture);
			userOrGroup = Path.GetFileName(userOrGroup);

			if (computerName.Length == 0 || domainUserName.Length == 0)
				return exist;

			if (string.Compare(computerName, domainUserName, true, CultureInfo.InvariantCulture) == 0)
				//se sono uguali è un local PC, cerco tra gli utenti locali al pc
				sADsPath = string.Concat(ConstString.providerNT, computerName, "/", userOrGroup);
			else
				//altrimenti è in un dominio, cerco tra gli utenti di dominio
				sADsPath = string.Concat(ConstString.providerNT, domainUserName, "/", userOrGroup);

			try
			{
				if (DirectoryEntry.Exists(sADsPath))
					exist = 1;
			}
			catch (SecurityException securityEx)
			{
				Debug.Fail(securityEx.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, securityEx.Message);
				extendedInfo.Add(Strings.Function, "ExistUser");
				extendedInfo.Add(Strings.DefinedInto, "SysAdminPlugIn");
				string message = string.Format(Strings.CannotVerifyNTUser, userOrGroup, domainUserName);
				diagnostic.Set(DiagnosticType.Warning, message, extendedInfo);
				exist = -1;
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Function, "ExistUser");
				extendedInfo.Add(Strings.DefinedInto, "SysAdminPlugIn");
				string message = string.Format(Strings.CannotVerifyNTUser, userOrGroup, domainUserName);
				diagnostic.Set(DiagnosticType.Warning, message, extendedInfo);
				exist = 0;
			}

			Cursor.Current = Cursors.Default;
			return exist;
		}
		#endregion

		#region AfterUserSelected - Imposta il dominio/workgroup correttamente, dopo che è stato selezionato l'Utente
		//---------------------------------------------------------------------
		private void AfterUserSelected(object sender, string domainSelected, string usersSelected)
		{
			this.domainSelected = domainSelected;
			tbLogin.Text = usersSelected;
			if (cbDomains.Items.Contains(domainSelected))
			{
				int pos = cbDomains.Items.IndexOf(domainSelected);
				cbDomains.SelectedIndex = pos;
			}
		}
		#endregion

        //metodi per il blocco del balloon prematuramente scomparsi li lascio  perchè potrebbero tornare a servire
        //---------------------------------------------------------------------
        private int GetBalloonBlockedMessageStatus()
        {
            int balloonBlocked = (int)MessageType.None;
            if (CkbContract.Checked)
                balloonBlocked = balloonBlocked | (int)MessageType.Contract;
            if (CkbUpdates.Checked)
                balloonBlocked = balloonBlocked | (int)MessageType.Updates;
            if (CkbAdvertisement.Checked)
                balloonBlocked = balloonBlocked | (int)MessageType.Advrtsm;
            return balloonBlocked;
        }

        //---------------------------------------------------------------------
        private void SetBalloonBlockedMessageStatus()
        {
            SetBalloonBlockedMessageStatus(0);
        }
        //---------------------------------------------------------------------
        private void SetBalloonBlockedMessageStatus(int messageType)
        {
            MessageType mt = (MessageType)messageType;

            (CkbContract.Checked) = ((mt & MessageType.Contract) == MessageType.Contract);
            (CkbAdvertisement.Checked) = ((mt & MessageType.Advrtsm) == MessageType.Advrtsm);
            (CkbUpdates.Checked) = ((mt & MessageType.Updates) == MessageType.Updates);
        }
		#endregion

		#region Funzioni che fanno Send di diagnostica		
		//---------------------------------------------------------------------
		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			OnSendDiagnostic?.Invoke(this, diagnostic);
			diagnostic.Clear();
		}

		//---------------------------------------------------------------------
		protected override void OnDeactivate(System.EventArgs e)
		{
			base.OnDeactivate(e);
			OnSendDiagnostic?.Invoke(this, diagnostic);
			diagnostic.Clear();
		}

		//---------------------------------------------------------------------
		protected override void OnVisibleChanged(System.EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (!this.Visible)
			{
				OnSendDiagnostic?.Invoke(this, diagnostic);
				diagnostic.Clear();
			}
		}
		#endregion

		/// <summary>
		/// cbDomains_Leave
		/// </summary>
		//---------------------------------------------------------------------
		private void cbDomains_Leave(object sender, System.EventArgs e)
		{
			string toInsert = ((ComboBox)sender).Text;
			if (cbDomains.Items.IndexOf(toInsert) == -1)
			{
				int pos = cbDomains.Items.Add(toInsert);
				cbDomains.SelectedIndex = pos;
			}
		}

		/// <summary>
		/// cbLocked_CheckStateChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void cbLocked_CheckStateChanged(object sender, System.EventArgs e)
		{
			if (cbLocked.CheckState == CheckState.Unchecked)
			{
				cbLocked.Checked = false;
				cbLocked.Enabled = false;
				nrLoginFailedCount.Text = "0";
				nrLoginFailedCount.Enabled = false;
				LblLoginFailedCount.Enabled = false;
			}
		}

		//---------------------------------------------------------------------
		private void SendCompleteHelp(object sender, string nameSpace, string searchParameter)
		{
			OnCallHelp?.Invoke(sender, nameSpace, searchParameter);
		}

        //---------------------------------------------------------------------
        private void StateChanged(object sender, EventArgs e)
        {
            State = StateEnums.Editing;
        }
	}
}