using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.TaskBuilderNet.Data.SQLDataAccess
{
	/// <summary>
	/// Credential
	/// Chiede le credenziali utente per effettuare l'impersonificazione
	/// </summary>
	// ========================================================================
	public partial class SQLCredential : System.Windows.Forms.Form
	{
		private bool success;
		private bool cancelButton = false;

		private string serverName		= string.Empty;
		private string loginName		= string.Empty;
		private string loginPassword	= string.Empty;
		
		private DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
		public UserImpersonatedData userImpersonated = new UserImpersonatedData();

		//---------------------------------------------------------------------
		public bool Success { get { return success; } set { success = value; } }

		//---------------------------------------------------------------------
		public delegate void ImpersonateUser(object sender, UserImpersonatedData userAfterImpersonation);
		public event ImpersonateUser OnImpersonateUser;

		public delegate void OpenHelpFromPopUp(object sender, string searchParameter);
		public event OpenHelpFromPopUp OnOpenHelpFromPopUp;

		#region Costruttori 
		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public SQLCredential(string serverName, string login, string password, string domain, bool winAuth, bool enableChangeCredential)
		{
			InitializeComponent();

			this.Text = string.Format(SQLDataAccessStrings.ConnectionTo, serverName);
			BtnOK.Enabled		= false;
			BtnCancel.Enabled	= true;				   
			ComboDomains.Items.Clear();
			ComboDomains.Items.Add(domain.ToUpper(CultureInfo.InvariantCulture));
			ComboDomains.SelectedIndex = 0;
			
			if (winAuth)
			{
				RadioWindowsAuthentication.Checked = true;
				userImpersonated.Login			   = Path.GetFileName(login);
				TbLogin.Text					   = Path.GetFileName(login);
			}
			else
			{
				RadioSQLServerAuthentication.Checked = true;
				userImpersonated.Login				 = login;
				TbLogin.Text						 = login;
				this.serverName = serverName;
				this.loginName = login;
				this.loginPassword = password;
			}

			TbPassword.Text						   = password;
			userImpersonated.Password			   = password;
			userImpersonated.Domain				   = domain;
			userImpersonated.WindowsAuthentication = winAuth;
			radioNewCredential.Enabled			   = enableChangeCredential;
			radioCurrentCredential.Enabled		   = enableChangeCredential;
			groupBoxTypeOfAuthentication.Enabled   = radioNewCredential.Checked;
			ComboDomains.Enabled                   = radioNewCredential.Checked;
			TbLogin.Enabled                        = radioNewCredential.Checked;
			TbPassword.Focus();
			
			if (enableChangeCredential)
			{
				toolTip.SetToolTip(TbLogin, string.Format(SQLDataAccessStrings.ToolTipLogin, serverName));
				toolTip.SetToolTip(TbPassword, string.Format(SQLDataAccessStrings.ToolTipPassword, login));
			}
		}

		/// <summary>
		/// Costruttore 2 (non chiede le credenziali)
		/// </summary>
		//---------------------------------------------------------------------
		public SQLCredential(string login, string password, string domain, bool windowsAuthentication)
		{
			InitializeComponent();
			userImpersonated.Login				   = login;
			userImpersonated.Password			   = password;
			userImpersonated.Domain				   = domain;
			userImpersonated.WindowsAuthentication = windowsAuthentication;
			userImpersonated.UserBeforeImpersonate = WindowsIdentity.GetCurrent().Name;

			if (windowsAuthentication)
			{
				LogonUserNS impersonationUser = new LogonUserNS(login, password, domain);

				if (impersonationUser.Diagnostic.Error || impersonationUser.Diagnostic.Warning)
					DiagnosticViewer.ShowDiagnostic(impersonationUser.Diagnostic);
				else
				{
					userImpersonated.UserAfterImpersonate = impersonationUser.ImpersonatedUser;
					if (OnImpersonateUser != null)	
						OnImpersonateUser(this, userImpersonated);
					success = true;
				}
			}
			else
			{
				userImpersonated.UserAfterImpersonate = null;
				if (OnImpersonateUser != null)	
					OnImpersonateUser(this, userImpersonated);
				success = true;
			}
		}

		/// <summary>
		/// Costruttore 3 (non chiede le credenziali e niente msg)
		/// </summary>
		//---------------------------------------------------------------------
		public SQLCredential(string login, string password, string domain, bool winAuth, bool silent /*true*/)
		{
			InitializeComponent();
			userImpersonated.Login				   = login;
			userImpersonated.Password			   = password;
			userImpersonated.Domain				   = domain;
			userImpersonated.WindowsAuthentication = winAuth;
			userImpersonated.UserBeforeImpersonate = WindowsIdentity.GetCurrent().Name;
			
			if (winAuth)
			{
				LogonUserNS impersonationUser = new LogonUserNS(login, password, domain);
				
				if (impersonationUser.Diagnostic.Error || impersonationUser.Diagnostic.Warning)
					DiagnosticViewer.ShowDiagnostic(impersonationUser.Diagnostic);
				else
				{
					userImpersonated.UserAfterImpersonate = impersonationUser.ImpersonatedUser;
					if (OnImpersonateUser != null)	
						OnImpersonateUser(this, userImpersonated);
					success = true;
				}
			}
			else
			{
				userImpersonated.UserAfterImpersonate = null;
				if (OnImpersonateUser != null)	
					OnImpersonateUser(this, userImpersonated);
				success = true;
			}
		}
		#endregion
	
		#region TbLogin_TextChanged - Se la login è vuota, il bottone OK è disabilitato
		/// <summary>
		/// TbLogin_TextChanged
		/// Quando il nome utente è vuoto, disabilito il bottone OK
		/// </summary>
		//---------------------------------------------------------------------
		private void TbLogin_TextChanged(object sender, System.EventArgs e)
		{
			BtnOK.Enabled = (TbLogin.Text != null && TbLogin.Text.Length > 0);
		}
		#endregion

		#region BtnOK_Click - Effettuo l'impersonificazione
		/// <summary>
		/// BtnOK_Click
		/// Ho premuto il bottone OK, lancio evento con i dati di login
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnOK_Click(object sender, System.EventArgs e)
		{
			TransactSQLAccess tentativeConnSql = new TransactSQLAccess();

			//se uso le credenziali correnti, non faccio nulla a parte
			//ritornare i dati della connessione precedente
			if (radioCurrentCredential.Checked)
			{
				userImpersonated.Login				   = TbLogin.Text;
				userImpersonated.Password			   = TbPassword.Text;
				userImpersonated.Domain				   = ComboDomains.SelectedItem.ToString().ToUpper(CultureInfo.InvariantCulture);
				userImpersonated.WindowsAuthentication = RadioWindowsAuthentication.Checked;
				userImpersonated.UserBeforeImpersonate = WindowsIdentity.GetCurrent().Name;
				
				if (RadioWindowsAuthentication.Checked)
				{
					if (ComboDomains.SelectedItem == null)
					{
						diagnosticViewer.Message = SQLDataAccessStrings.EmptyUserDomain;
						diagnosticViewer.Title = SQLDataAccessStrings.Credential;
						diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
						diagnosticViewer.ShowIcon    = MessageBoxIcon.Warning;
						diagnosticViewer.Show();
						success = false;
						return;
					}
					
					//chiamo la LogonUser solo se non sto lavorando sulla macchina locale
					LogonUserNS impersonationUser = 
						new LogonUserNS(TbLogin.Text, TbPassword.Text, ComboDomains.SelectedItem.ToString().ToUpper(CultureInfo.InvariantCulture));

					if (impersonationUser.Diagnostic.Error || impersonationUser.Diagnostic.Warning)
					{
						DiagnosticViewer.ShowDiagnostic(impersonationUser.Diagnostic);
						success = false;
					}
					else
					{
						userImpersonated.UserAfterImpersonate = impersonationUser.ImpersonatedUser;
						if (userImpersonated.UserAfterImpersonate == null)
						{
							diagnosticViewer.Message = SQLDataAccessStrings.WrongCredential;
							diagnosticViewer.Title = SQLDataAccessStrings.Credential;
							diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
							diagnosticViewer.ShowIcon    = MessageBoxIcon.Warning;
							diagnosticViewer.Show();
							success = false;
						}
						else
						{
							if (OnImpersonateUser != null)	
								OnImpersonateUser(sender, userImpersonated);
							success = true;
							this.Close();
						}
					}
				}
				else
				{
					//devo testare se l'utente esiste!!!!
					if (SuccessConnection(userImpersonated.Login, userImpersonated.Password, tentativeConnSql))
					{
						userImpersonated.UserAfterImpersonate  = null;
						if (OnImpersonateUser != null)	
							OnImpersonateUser(sender, userImpersonated);
						success = true;
						this.Close();
					}
					else
					{
						DiagnosticViewer.ShowDiagnostic(tentativeConnSql.Diagnostic);
						success = false;
					}
				}
			}
			// altrimenti, se scelgo di introdurre le nuove credenziali
			else if (radioNewCredential.Checked)
			{
				//se è una windows authentication, devo fare una impersonificazione
				//dell'utente, e ritorno tale utente
				if (RadioWindowsAuthentication.Checked)
				{
					if (ComboDomains.SelectedItem != null)
					{
						userImpersonated.Login				   = TbLogin.Text;
						userImpersonated.Password			   = TbPassword.Text;
						userImpersonated.Domain				   = ComboDomains.SelectedItem.ToString().ToUpper(CultureInfo.InvariantCulture);
						userImpersonated.WindowsAuthentication = RadioWindowsAuthentication.Checked;
						userImpersonated.UserBeforeImpersonate = WindowsIdentity.GetCurrent().Name;
						
						LogonUserNS impersonationUser = 
							new LogonUserNS(TbLogin.Text, TbPassword.Text, ComboDomains.SelectedItem.ToString().ToUpper(CultureInfo.InvariantCulture));

						if (impersonationUser.Diagnostic.Error || impersonationUser.Diagnostic.Warning)
						{
							DiagnosticViewer.ShowDiagnostic(impersonationUser.Diagnostic);
							success  = false;
						}
						else
						{
							userImpersonated.UserAfterImpersonate  = impersonationUser.ImpersonatedUser;
							if (OnImpersonateUser != null) 
								OnImpersonateUser(sender, userImpersonated);
							if (userImpersonated.UserAfterImpersonate != null)
							{
								success  = true;
								this.Close();
							}
							else
							{
								diagnosticViewer.Message = SQLDataAccessStrings.WrongCredential;
								diagnosticViewer.Title = SQLDataAccessStrings.Credential;
								diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
								diagnosticViewer.ShowIcon	 = MessageBoxIcon.Warning;
								diagnosticViewer.Show();
								success = false;
							}
						}
					}
					else
					{
						diagnosticViewer.Message = SQLDataAccessStrings.EmptyUserDomain;
						diagnosticViewer.Title = SQLDataAccessStrings.Credential;
						diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
						diagnosticViewer.ShowIcon	 = MessageBoxIcon.Warning;
						diagnosticViewer.Show();
						success = false;
					}
				}
				else
				{
					if (SuccessConnection(TbLogin.Text, TbPassword.Text, tentativeConnSql))
					{
						//Ritorno la login e la password nuove per la connessione
						userImpersonated.Login				   = TbLogin.Text;
						userImpersonated.Password			   = TbPassword.Text;
						userImpersonated.Domain				   = string.Empty;
						userImpersonated.WindowsAuthentication = RadioWindowsAuthentication.Checked;
						userImpersonated.UserBeforeImpersonate = WindowsIdentity.GetCurrent().Name;
						userImpersonated.UserAfterImpersonate  = null;
						if (OnImpersonateUser != null)
							OnImpersonateUser(sender, userImpersonated);
						success = true;
						this.Close();
					}
					else
					{
						DiagnosticViewer.ShowDiagnostic(tentativeConnSql.Diagnostic);
						success = false;
					}
				}
			}
		}
		#endregion

		#region SuccessConnection - Verifica che con le credenziali sia possibile connettersi
		/// <summary>
		/// SuccessConnection
		/// Verifica che con le credenziali sia possibile connettersi
		/// </summary>
		//---------------------------------------------------------------------
		private bool SuccessConnection(string loginName, string loginPassword, TransactSQLAccess tentativeConnSql)
		{
			bool successConnection = false;

			tentativeConnSql.CurrentStringConnection = 
				string.Format("Data Source={0};User ID={1};Password={2}", this.serverName, loginName, loginPassword);

			if (tentativeConnSql.TryToConnect()) 
				successConnection = true;

			if (tentativeConnSql.CurrentConnection != null)
			{
				tentativeConnSql.CurrentConnection.Close();
				tentativeConnSql.CurrentConnection.Dispose();
				tentativeConnSql.CurrentConnection = null;
			}

			return successConnection;
		}
		#endregion

		#region BtnCancel_Click - Interrompo e chiudo la finestra
		/// <summary>
		/// 2
		/// Chiudo la form
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			// tolta la visualizzazione del msg (del tutto inutile)
			success = false;
			cancelButton = true;
			this.Close();
		}
		#endregion

		#region ComboDomains_DropDown - DD di visualizzazione del dominio / gruppo di lavoro
		/// <summary>
		/// ComboDomains_DropDown
		/// Visualizza il dominio
		/// </summary>
		//---------------------------------------------------------------------
		private void ComboDomains_DropDown(object sender, System.EventArgs e)
		{
			ComboDomains.Items.Clear();
			ComboDomains.Items.Add(System.Net.Dns.GetHostName().ToUpper(CultureInfo.InvariantCulture));
			ComboDomains.Items.Add(SystemInformation.UserDomainName);
			
			/*string sADsPath = "WinNT://" + sCompName + ",computer";
			DirectoryEntry entry = new DirectoryEntry(sADsPath);
			if (string.Compare(entry.Parent.Name.ToUpper(CultureInfo.InvariantCulture), "WORKGROUP", true, CultureInfo.InvariantCulture) != 0)
				if (CheckIfDomainIsConnected(entry.Parent.Name.ToUpper(CultureInfo.InvariantCulture)))
					ComboDomains.Items.Add(entry.Parent.Name.ToUpper(CultureInfo.InvariantCulture));
			if (ComboDomains.Items.Count > 0) 
				ComboDomains.SelectedIndex = 0;*/
		}
		#endregion

		/*
		#region CheckIfDomainIsConnected - Verifica se si è connessi al dominio / gruppo di lavoro

		/// <summary>
		/// CheckIfDomainIsConnected
		/// Controlla se è possibile effettuare il browsing del dominio selezionato
		/// </summary>
		/// <param name="domain"></param>
		/// <returns></returns>
		//----------------------------------------------------------------
		private bool CheckIfDomainIsConnected(string domain)
		{
			bool isConnected			 = false;
			DirectoryEntry entry		 = new DirectoryEntry("LDAP://" + domain);
			DirectorySearcher mySearcher = new DirectorySearcher(entry);
			try
			{
				SearchResult results = mySearcher.FindOne();
				isConnected = true;
			}
			// Valuto una Exception generale perchè il metodo non genera nessuna exception particolare
			// seguito tecnica suggerita msdn
			catch (Exception) {}
			return isConnected;
		}

		#endregion
		*/

		#region RadioSQLServerAuthentication_CheckedChanged - Se scelgo la Sql Authentication, disabilito la combo dei Domains
		/// <summary>
		/// RadioSQLServerAuthentication_CheckedChanged
		/// Se scelgo la Sql Authentication, disabilito la combo dei Domains
		/// </summary>
		//---------------------------------------------------------------------
		private void RadioSQLServerAuthentication_CheckedChanged(object sender, System.EventArgs e)
		{
			ComboDomains.Enabled = !RadioSQLServerAuthentication.Checked;
		}
		#endregion

		#region radioCurrentCredential_CheckedChanged - Scelgo di usare le credenziali correnti
		/// <summary>
		/// radioCurrentCredential_CheckedChanged
		/// Scelgo di usare le credenziali correnti
		/// </summary>
		//---------------------------------------------------------------------
		private void radioCurrentCredential_CheckedChanged(object sender, System.EventArgs e)
		{
			if (radioCurrentCredential.Checked)
			{
				groupBoxTypeOfAuthentication.Enabled  = false;
				RadioSQLServerAuthentication.Enabled  = false;
				RadioWindowsAuthentication.Enabled	  = false;
				ComboDomains.Enabled				  = false;
				TbLogin.Enabled						  = false;
				TbPassword.Enabled					  = true;
				TbPassword.Focus();
			}
			else
			{
				groupBoxTypeOfAuthentication.Enabled  = true;
				RadioSQLServerAuthentication.Enabled  = true;
				RadioWindowsAuthentication.Enabled	  = true;
				ComboDomains.Enabled				  = RadioWindowsAuthentication.Checked;
				TbLogin.Enabled						  = true;
				TbPassword.Enabled					  = true;
				TbLogin.Focus();
			}
		}
		#endregion

		#region radioNewCredential_CheckedChanged - Scelgo di usare nuove credenziali
		/// <summary>
		/// radioNewCredential_CheckedChanged
		/// Se scelgo di introdurre le nuove credenziali, abilito di default la windows authentication
		/// </summary>
		//---------------------------------------------------------------------
		private void radioNewCredential_CheckedChanged(object sender, System.EventArgs e)
		{
			if (radioNewCredential.Checked)
			{
				if (TbLogin.Text.Length > 0)
				{
					if (TbLogin.Text.Split(Path.DirectorySeparatorChar).Length > 1)
					{
						RadioWindowsAuthentication.Checked = true;
						RadioSQLServerAuthentication.Checked = false;
					}
					else
					{
						RadioWindowsAuthentication.Checked = false;
						RadioSQLServerAuthentication.Checked = true;
					}
				}
				TbLogin.Focus();
			}
		}
		#endregion

		#region RadioWindowsAuthentication_CheckedChanged - Scelgo autenticazione NT
		/// <summary>
		/// RadioWindowsAuthentication_CheckedChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void RadioWindowsAuthentication_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				ComboDomains.Enabled   = true;
				TbLogin.Enabled		   = true;
				TbPassword.Enabled	   = true;
				TbLogin.Focus();
			}
		}
		#endregion

		#region Credential_Closing - l'utente non può chiudere la finestra senza prima aver inserito le credenziali
		/// <summary>
		/// Credential_Closing
		/// Faccio in modo che l'utente non possa chiudere la finestra senza prima aver editato le credenziali
		/// </summary>
		//---------------------------------------------------------------------
		private void Credential_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{	
			if (success || cancelButton)
				e.Cancel = false;
			else 
				e.Cancel = true;
		}
		#endregion

		#region TbPassword_Enter - Imposto il focus alla password
		/// <summary>
		/// TbPassword_Enter
		/// </summary>
		//---------------------------------------------------------------------
		private void TbPassword_Enter(object sender, System.EventArgs e)
		{
			TbPassword.Focus();
		}
		#endregion

		#region BtnHelp_Click - Help
		/// <summary>
		/// BtnHelp_Click
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnHelp_Click(object sender, System.EventArgs e)
		{
			if (OnOpenHelpFromPopUp != null)
				OnOpenHelpFromPopUp(this, "Microarea.TaskBuilderNet.Data.SQLDataAccess.Credential");
		}
		#endregion
	}
}