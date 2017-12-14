using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// UserLite
	/// Gestione degli Utenti Applicativi
	/// </summary>
	//=========================================================================
	public partial class UserLite : PlugInsForm
	{
		#region Variabili Private
		private string connectionString = string.Empty;
		private SqlConnection currentConnection = null;
		private Diagnostic diagnostic = new Diagnostic("SysAdminPlugIn.UserLite");

		private List<string> noAdmittedUserNames = new List<string>();

		private string originalPassword = string.Empty;
		private bool isModifiedPassword = false;
		private int MinPasswordLength = 0;
		private int PasswordDuration = 0;
		private bool UseStrongPwd = false;

		private bool isGuest = false;
		private bool isAzureSQLDatabase = false;

		// mi serve per sapere quale operazione di salvataggio richiamare
		// se sto creando un nuovo utente o modificando uno esistente
		private bool isNewUser = true;
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
		public UserLite(SysAdminStatus status, bool isAzureSQLDatabase, PathFinder aPathFinder)
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

			tbPassword.Enabled = true;
			cbUserMustChangePassword.Checked = false;
			cbUserCannotChangePassword.Checked = false;
			cbPasswordNeverExpired.Checked = false;
			cbPrivateAreaAdmin.Checked = false;
			cbExpiredDateCannotChange.Checked = false;
			cbLocked.Enabled = false;
			cbLocked.Checked = false;
			nrLoginFailedCount.Text = "0";
			nrLoginFailedCount.Enabled = false;
			LblLoginFailedCount.Enabled = false;

			//popolo la lingua di applicazione
			cultureApplicationCombo.LoadLanguages();
			//popolo la lingua di interfaccia
			cultureUICombo.LoadLanguagesUI();

			if (String.Compare(aPathFinder.Edition, Edition.Enterprise.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				CbSmarClientAccess.Visible = false;
				CBConcurrentAccess.Checked = true;
			}

			LabelTitle.Text = Strings.TitleInsertingNewUser;

			SettingsToolTips();

			State = StateEnums.View;
		}

		/// <summary>
		/// Costruttore MODIFICA UTENTE
		/// </summary>
		//---------------------------------------------------------------------
		public UserLite(SysAdminStatus status, bool isAzureSQLDatabase, string loginId,	PathFinder aPathFinder)
		{
			InitializeComponent();
			//pulisco il diagnostico
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

			tbPassword.Enabled = true;
			this.connectionString = status.ConnectionString;
			this.currentConnection = status.CurrentConnection;
			
			//popolo la lingua di applicazione
			cultureApplicationCombo.LoadLanguages();
			//popolo la lingua di interfaccia
			cultureUICombo.LoadLanguagesUI();

			UserDb dbUser = new UserDb();
			dbUser.ConnectionString = connectionString;
			dbUser.CurrentSqlConnection = this.currentConnection;
			ArrayList fieldsUser = new ArrayList();

			bool result = dbUser.GetAllUserFieldsById(out fieldsUser, loginId);
			if (!result)
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
					cbDisabled.Enabled = cbDisabled.Checked;
				}
				else
					if (dbUser.IsAssociated(tbLoginId.Text))
						tbLogin.Enabled = false;
					
				if (string.Compare(tbLogin.Text, NameSolverStrings.GuestLogin, true, CultureInfo.InvariantCulture) == 0)
				{
					isGuest = true;
					tbLogin.Enabled = false;
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
		}
		#endregion

		#region SettingsToolTips
		/// <summary>
		/// SettingsToolTips
		/// </summary>
		//---------------------------------------------------------------------
		private void SettingsToolTips()
		{
			toolTip.SetToolTip(tbLogin, Strings.UserNameToolTip);
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
		/// <summary>
		/// CheckValidator
		/// Verifica validità dei dati inputati
		/// </summary>
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
			if (isAzureSQLDatabase && noAdmittedUserNames.ContainsNoCase(loginName))
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.WrongLoginName, loginName));
				result = false;
			}
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
			if (!cbPasswordNeverExpired.Checked && expirationDate.Value.ToShortDateString().Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoEmptyValue, Strings.ExpirationDate));
				expirationDate.Value = DateTime.Now.AddDays(PasswordDuration);
				expirationDate.Text = DateTime.Now.AddDays(PasswordDuration).ToShortDateString();
				result = false;
			}

			//se la password non è vuota ed è diversa da Empty faccio il test sulla lunghezza
			//controllo che la lunghezza sia corretta
			if (tbPassword.Text.Length < MinPasswordLength)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoPasswordLength, MinPasswordLength.ToString()));
				result = false;
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
		/// Save
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

			tbLogin.Text = tbLogin.Text.Trim(); // ensures robustness against bad coding practices
			string loginName = tbLogin.Text;

			if (!CheckValidator(loginName))
				return;

			if (cultureUICombo.SelectedIndex != -1)
				preferredLanguage = cultureUICombo.SelectedLanguageValue;
			applicationLanguage = cultureApplicationCombo.ApplicationLanguage;

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
					false,
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
			if (!CbSmarClientAccess.Checked) dbUser.RemoveReserverdCal(tbLoginId.Text);

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
		#endregion

		#region SaveNewUser - Inserisco un nuovo Utente Applicativo
		/// <summary>
		/// SaveNewUser
		/// </summary>
		//---------------------------------------------------------------------
		private void SaveNewUser(object sender, System.EventArgs e)
		{
			tbLogin.Text = tbLogin.Text.Trim(); // ensures robustness against bad coding practices
			string loginName = tbLogin.Text;

			if (!CheckValidator(loginName))
				return;

			State = StateEnums.Processing;
			string preferredLanguage = string.Empty;
			if (cultureUICombo.SelectedIndex != -1)
				preferredLanguage = cultureUICombo.SelectedLanguageValue;
			string applicationLanguage = cultureApplicationCombo.ApplicationLanguage;

			if (!isGuest)
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
								false,
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
						false,
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
						OnAddGuestUser?.Invoke(sender, e);
					
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

			OnAfterDisabledCheckedChanged?.Invoke(sender, tbLoginId.Text, cbDisabled.Checked);
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
			ArrayList dataUser = new ArrayList();

			string loginDeleted = string.Empty;
			bool result = dbUser.GetAllUserFieldsById(out dataUser, tbLoginId.Text);
			if (dataUser.Count != 0 && result)
			{
				UserItem userItem = (UserItem)dataUser[0];
				loginDeleted = userItem.Login;
			}

			if (dbUser.Delete(tbLoginId.Text))
			{
				OnTraceAction?.Invoke(string.Empty, loginDeleted, TraceActionType.DeleteUser, DatabaseLayerConsts.MicroareaConsole);
				//devo fare il reload dei utenti
				OnModifyTree?.Invoke(sender, ConstString.containerUsers);
				//devo anche fare il reload degli utenti delle company e dei ruoli
				OnModifyTree?.Invoke(sender, ConstString.containerCompanies);
				if (isGuest)
					OnDeleteGuestUser?.Invoke(sender, e);
			}
			else
			{
				diagnostic.Set(dbUser.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(sender, diagnostic);
					diagnostic.Clear();
				}
			}
		}
		#endregion

		#region Eventi sui Controlli della Form

		#region cbPasswordNeverExpired_CheckedChanged - Selezionato Password non spira mai
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
				expirationDate.Value = System.DateTime.Now.AddDays(PasswordDuration);
				expirationDate.Text = expirationDate.Value.ToShortDateString();
			}
		}
		#endregion

		#region cbExpiredDateCannotChange_CheckedChanged - Selezionato la data di scadenza non può essere modificata
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
				if (tbLogin.Text.Length == 0)
					tbLogin.Focus();
			}
			else
				this.tbDescrizione.Focus();

			if (isAzureSQLDatabase)
			{
				// nomi login non ammessi per Azure
				noAdmittedUserNames.Add("admin");
				noAdmittedUserNames.Add("administrator");
				noAdmittedUserNames.Add("guest");
				noAdmittedUserNames.Add("root");
				noAdmittedUserNames.Add("sa");
			}
		}
		#endregion

		#region Altre Funzioni

        //metodi per il blocco del balloon prematuramente scomparsi li lascio perchè potrebbero tornare a servire
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
			OnSendDiagnostic?.Invoke(this, diagnostic); diagnostic.Clear();
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
			if (this.OnCallHelp != null)
				OnCallHelp(sender, nameSpace, searchParameter);
		}

        //---------------------------------------------------------------------
        private void StateChanged(object sender, EventArgs e)
        {
            State = StateEnums.Editing;
        }       
	}
}