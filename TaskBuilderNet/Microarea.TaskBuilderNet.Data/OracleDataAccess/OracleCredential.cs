using System.Data.OracleClient;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.OracleDataAccess
{
	/// <summary>
	/// Credential
	/// Summary description for Credential
	/// </summary>
	/// =======================================================================
	public partial class OracleCredential : System.Windows.Forms.Form
	{
		#region Variables
		private bool	success;
		private bool	cancelButton	= false;
		private string  oracleService   = string.Empty;
		private Role	typeOfRole		= Role.User;
		
		public Diagnostic diagnostic = new Diagnostic("OracleCredential");
		public OracleUserImpersonatedData oracleUserImpersonated = new OracleUserImpersonatedData();
		#endregion

		#region Properties
		//---------------------------------------------------------------------------
		public bool Success	{ get { return success;	} set { success	= value; }}
		#endregion

		#region Events and delegates
		//---------------------------------------------------------------------
		public delegate void	ImpersonateUser(object sender, OracleUserImpersonatedData userAfterImpersonation);
		public event			ImpersonateUser OnImpersonateUser;
		//---------------------------------------------------------------------
		public delegate void	SendDiagnostic(object sender, Diagnostic diagnostic);
		public event			SendDiagnostic OnSendDiagnostic;
		//---------------------------------------------------------------------
		public delegate void	OpenHelpFromPopUp(object sender, string searchParameter);
		public event			OpenHelpFromPopUp OnOpenHelpFromPopUp;
		#endregion

		#region Costruttori
		//---------------------------------------------------------------------------
		public OracleCredential()
		{
			InitializeComponent();
			diagnostic.Clear();
			this.oracleService	= string.Empty;
			this.typeOfRole		= Role.User;
			SettingsText();
		}

		/// <summary>
		/// Non sono note le credenziali si chiedono quelle di system (perchè è l'utente di default) ma è possibile darne altre
		/// </summary>
		/// <param name="oracleService"></param>
		/// <param name="typeOfRole"></param>
		//---------------------------------------------------------------------------
		public OracleCredential(string oracleService, Role typeOfRole, bool enabledUserChange)
		{
			InitializeComponent();
			diagnostic.Clear();
			this.oracleService	= oracleService;
			this.typeOfRole = typeOfRole;
			SettingsText();

			BtnOK.Enabled		= true;
			BtnCancel.Enabled	= true;	
			TbOracleService.Text= this.oracleService;

			if (oracleUserImpersonated.WindowsAuthentication)
			{
				RadioWindowsAuthentication.Checked = true;
				ComboDomains.Enabled = true;
				LoadDomains();
				ComboDomains.SelectedIndex = ComboDomains.Items.IndexOf(oracleUserImpersonated.Domain);
			}
			else
			{
				RadioOracleServerAuthentication.Checked = true;
				ComboDomains.Enabled = false;
			}
			
			if (!enabledUserChange)
			{
				ComboDomains.Enabled = false;
				TbLogin.Enabled	= false;
				RadioWindowsAuthentication.Enabled = false;
				RadioOracleServerAuthentication.Enabled = false;
			}
		}

		//---------------------------------------------------------------------------
		public OracleCredential(string oracleService, Role typeOfRole) 
			: this (oracleService, typeOfRole, true)
		{}

		//---------------------------------------------------------------------------
		public OracleCredential(OracleUserImpersonatedData candidateOracleUser, Role typeOfRole) 
			: this(candidateOracleUser, typeOfRole, true)
		{}

		/// <summary>
		/// Usata per visualizzare la form prima di impersonificare
		/// (l'utente deve scrivere la pwd - e' la prima volta che si connette con queste credenziali)
		/// </summary>
		/// <param name="candidateOracleUser"></param>
		/// <param name="typeOfRole"></param>
		//---------------------------------------------------------------------------
		public OracleCredential(OracleUserImpersonatedData candidateOracleUser, Role typeOfRole, bool enabledUserChange)
		{
			InitializeComponent();
			diagnostic.Clear();

			oracleUserImpersonated.OracleService = candidateOracleUser.OracleService;
			oracleUserImpersonated.Password      = candidateOracleUser.Password;
			oracleUserImpersonated.Domain        = candidateOracleUser.Domain;
			oracleUserImpersonated.IsDba         = candidateOracleUser.IsDba;
			oracleUserImpersonated.WindowsAuthentication = candidateOracleUser.WindowsAuthentication;
			
			if (oracleUserImpersonated.WindowsAuthentication)
			{
				string loginNT = (candidateOracleUser.Login.Split(Path.DirectorySeparatorChar).Length == 1) 
					             ? candidateOracleUser.Domain + Path.DirectorySeparatorChar + candidateOracleUser.Login 
					             : candidateOracleUser.Login;
				oracleUserImpersonated.Login = loginNT;
			}
			else
				oracleUserImpersonated.Login = candidateOracleUser.Login;
			
			oracleUserImpersonated.IsCurrentUser = 
				string.Compare(WindowsIdentity.GetCurrent().Name, oracleUserImpersonated.Login, true, CultureInfo.InvariantCulture) == 0;

			this.oracleService	= candidateOracleUser.OracleService;
			this.typeOfRole		= typeOfRole;

			SettingsText();
			BtnOK.Enabled		= true;
			BtnCancel.Enabled	= true;	
			
			//ora carico i dati nella form
			if (oracleUserImpersonated.WindowsAuthentication)
			{
				RadioWindowsAuthentication.Checked = true;
				ComboDomains.Enabled = true;
				LoadDomains();
				ComboDomains.SelectedIndex = ComboDomains.Items.IndexOf(oracleUserImpersonated.Domain);
			}
			else
			{
				RadioOracleServerAuthentication.Checked = true;
				ComboDomains.Enabled = false;
			}

			TbLogin.Text = oracleUserImpersonated.Login;
			TbPassword.Text = string.Empty;
			TbOracleService.Text = oracleUserImpersonated.OracleService;
			if (!enabledUserChange)
			{
				ComboDomains.Enabled = false;
				TbLogin.Enabled	= false;
				RadioWindowsAuthentication.Enabled = false;
				RadioOracleServerAuthentication.Enabled = false;
			}
		}

		/// <summary>
		/// L'utente si è già connesso con queste credenziali, pertanto la form non viene mostrata ma semplicmente impersonifica
		/// </summary>
		/// <param name="candidateOracleUser"></param>
		//---------------------------------------------------------------------------
		public OracleCredential(OracleUserImpersonatedData candidateOracleUser)
		{
			InitializeComponent();
			diagnostic.Clear();
			SettingsText();
			
			oracleUserImpersonated.OracleService = candidateOracleUser.OracleService;
			oracleUserImpersonated.Password      = candidateOracleUser.Password;
			oracleUserImpersonated.Domain        = candidateOracleUser.Domain;
			oracleUserImpersonated.IsDba         = candidateOracleUser.IsDba;
			oracleUserImpersonated.WindowsAuthentication = candidateOracleUser.WindowsAuthentication;
			this.typeOfRole = (candidateOracleUser.IsDba) ? Role.Administrator : Role.User;

			if (candidateOracleUser.WindowsAuthentication)
			{
				string loginNT = (candidateOracleUser.Login.Split(Path.DirectorySeparatorChar).Length == 1) 
					? candidateOracleUser.Domain + Path.DirectorySeparatorChar + candidateOracleUser.Login 
					: candidateOracleUser.Login;
				oracleUserImpersonated.Login = loginNT;
			}
			else
				oracleUserImpersonated.Login = candidateOracleUser.Login;
			
			oracleUserImpersonated.IsCurrentUser = 
				string.Compare(WindowsIdentity.GetCurrent().Name, oracleUserImpersonated.Login, true, CultureInfo.InvariantCulture) == 0;

			if (oracleUserImpersonated.WindowsAuthentication)
			{
				LogonUserNS impersonationUser = null;
				if (!oracleUserImpersonated.IsCurrentUser)
				{
					impersonationUser = new LogonUserNS
						(
						oracleUserImpersonated.Login, 
						oracleUserImpersonated.Password, 
						oracleUserImpersonated.Domain
						);
					
					if (impersonationUser.Diagnostic.Error || impersonationUser.Diagnostic.Warning)
					{
						if (impersonationUser != null &&  impersonationUser.ImpersonatedUser != null)
							impersonationUser.ImpersonatedUser.Undo();
						diagnostic.Set(impersonationUser.Diagnostic);
						if (OnSendDiagnostic != null)
							OnSendDiagnostic(this, diagnostic);
						Success = false;
						return;
					}
				}
					
				if (CheckPermissionOrConnection())
				{
					oracleUserImpersonated.UserAfterImpersonate = 
						(oracleUserImpersonated.IsCurrentUser)
						? System.Security.Principal.WindowsIdentity.GetCurrent().Impersonate()
						: impersonationUser.ImpersonatedUser;
					
					if (OnImpersonateUser != null)	
						OnImpersonateUser(this, oracleUserImpersonated);
					Success = true;
				}
				else
				{
					if (impersonationUser != null &&  impersonationUser.ImpersonatedUser != null)
						impersonationUser.ImpersonatedUser.Undo();
					if (OnSendDiagnostic != null)
						OnSendDiagnostic(this, diagnostic);
					Success = false;
				}
			}
			else
			{
				oracleUserImpersonated.Login	= candidateOracleUser.Login;
				oracleUserImpersonated.Domain   = string.Empty;
					
				if (CheckPermissionOrConnection())
				{
					oracleUserImpersonated.UserAfterImpersonate = null;
					Success = true;
					this.Close();
				}
				else
				{
					if (OnSendDiagnostic != null)
						OnSendDiagnostic(this, diagnostic);
					Success = false;
				}	
			}

			if (OnImpersonateUser != null)	
				OnImpersonateUser(this, oracleUserImpersonated);
		}
		#endregion

		#region Text and Strings
		/// <summary>
		/// SettingsText
		/// </summary>
		//---------------------------------------------------------------------
		private void SettingsText()
		{
			if ((this.typeOfRole & Role.Administrator) == Role.Administrator)
				LblExplication.Text = OracleDataAccessStrings.AdminCredential;
			else if ((this.typeOfRole & Role.User) == Role.User)
				LblExplication.Text = OracleDataAccessStrings.CommonUserCredential;
		}

		/// <summary>
		/// SettingStrings
		/// </summary>
		//---------------------------------------------------------------------------
        private void SettingToolTips()
		{
			toolTipOracle.SetToolTip(TbLogin, OracleDataAccessStrings.UserOracleToolTip);
			toolTipOracle.SetToolTip(TbPassword, OracleDataAccessStrings.UserOraclePwdToolTip);
			toolTipOracle.SetToolTip(TbOracleService, OracleDataAccessStrings.ServiceOracleToolTip);
			toolTipOracle.SetToolTip(RadioWindowsAuthentication, OracleDataAccessStrings.WindowsAuthenticationToolTip);
			toolTipOracle.SetToolTip(RadioOracleServerAuthentication, OracleDataAccessStrings.OracleAuthenticationToolTip);
		}
		#endregion

		#region Funzioni sui Bottoni

		#region BtnOK_Click - Impersonificazione
		/// <summary>
		/// BtnOK_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------------
		private void BtnOK_Click(object sender, System.EventArgs e)
		{
			if (CheckData())
			{
				oracleUserImpersonated.WindowsAuthentication	= RadioWindowsAuthentication.Checked;
				oracleUserImpersonated.Password					= TbPassword.Text;
				oracleUserImpersonated.OracleService            = TbOracleService.Text;
				oracleUserImpersonated.UserBeforeImpersonate	= WindowsIdentity.GetCurrent().Name;
				
				if (RadioWindowsAuthentication.Checked)
				{
					string[] elementsAuth			= TbLogin.Text.Split(Path.DirectorySeparatorChar);
					oracleUserImpersonated.Login	= elementsAuth[1];
					oracleUserImpersonated.Domain   = elementsAuth[0].ToUpper(CultureInfo.InvariantCulture);
					
					OracleAccess tentativeConn = new OracleAccess();
					LogonUserNS impersonationUser = null;
					if (string.Compare(WindowsIdentity.GetCurrent().Name, TbLogin.Text, true, CultureInfo.InvariantCulture) != 0)
					{
						//se sono in sicurezza integrata impersonifico
						impersonationUser = new LogonUserNS
							(
								oracleUserImpersonated.Login, 
								TbPassword.Text, 
								oracleUserImpersonated.Domain
							);
						if (impersonationUser.Diagnostic.Error || impersonationUser.Diagnostic.Warning)
						{
							if (impersonationUser != null && impersonationUser.ImpersonatedUser != null)
								impersonationUser.ImpersonatedUser.Undo();
							diagnostic.Set(impersonationUser.Diagnostic);
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							if (OnSendDiagnostic != null)
								OnSendDiagnostic(sender, diagnostic);
							diagnostic.Clear();
							Success  = false;
							return ;
						}
					}
					
					if (CheckPermissionOrConnection())
					{
						if (string.Compare(WindowsIdentity.GetCurrent().Name, TbLogin.Text, true, CultureInfo.InvariantCulture) == 0)
							oracleUserImpersonated.UserAfterImpersonate = System.Security.Principal.WindowsIdentity.GetCurrent().Impersonate();
						else
							oracleUserImpersonated.UserAfterImpersonate  = impersonationUser.ImpersonatedUser;

						if (oracleUserImpersonated.UserAfterImpersonate != null)
						{
							Success  = true;
							this.Close();
						}
						else
						{
							if (impersonationUser != null &&  impersonationUser.ImpersonatedUser != null)
								impersonationUser.ImpersonatedUser.Undo();
							diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.WrongCredential);
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							if (OnSendDiagnostic != null)
								OnSendDiagnostic(sender, diagnostic);
							if (oracleUserImpersonated.UserAfterImpersonate != null)
								oracleUserImpersonated.UserAfterImpersonate.Undo();
							diagnostic.Clear();
							Success = false;
						}
					}
					else
					{
						if (impersonationUser != null && impersonationUser.ImpersonatedUser != null)
							impersonationUser.ImpersonatedUser.Undo();
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (OnSendDiagnostic != null)
							OnSendDiagnostic(sender, diagnostic);
						diagnostic.Clear();
						Success  = false;
					}
				}
				else
				{
					oracleUserImpersonated.Login	= TbLogin.Text;
					oracleUserImpersonated.Domain   = string.Empty;
					
					if (CheckPermissionOrConnection())
					{
						oracleUserImpersonated.UserAfterImpersonate = null;
						Success = true;
						this.Close();
					}
					else
					{
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (OnSendDiagnostic != null)
							OnSendDiagnostic(sender, diagnostic);
						diagnostic.Clear();
						Success = false;
					}
				}
				
				if (OnImpersonateUser != null)	
					OnImpersonateUser(this, oracleUserImpersonated);
			}
		}
		#endregion

		#region BtnCancel_Click - Abbandono 
		/// <summary>
		/// BtnCancel_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			Success			= false;
			cancelButton	= true;
			diagnostic.Set(DiagnosticType.Error, OracleDataAccessStrings.AbortingCredential);
			DiagnosticViewer.ShowDiagnostic(diagnostic);
			diagnostic.Clear();
			this.Close();
		}
		#endregion

		#region BtnHelp_Click - Help
		/// <summary>
		/// BtnHelp_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void BtnHelp_Click(object sender, System.EventArgs e)
		{
			string searchParameter = "Microarea.TaskBuilderNet.Data.OracleDataAccess.Credential";
			if (OnOpenHelpFromPopUp != null)
				OnOpenHelpFromPopUp(this, searchParameter);
		}
		#endregion

		#endregion

		#region Funzioni di Check

		#region CheckPermissionOrConnection - Check della connessione (Normal) o ruolo amministrativo (SYSDBA)
		/// <summary>
		/// CheckPermissionOrConnection
		/// Se l'utente è Normal prova la connessione; se è SYSDBA prima verifica che sia un utente amministratore
		/// e poi tenta la connessione
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------------
		private bool CheckPermissionOrConnection()
		{
			bool result = false;
			if ((this.typeOfRole & Role.Administrator) == Role.Administrator)
			{
				result = SuccessConnection(oracleUserImpersonated);
				if (result)
				{
					oracleUserImpersonated.IsDba = CheckIsDba(oracleUserImpersonated);
					result = oracleUserImpersonated.IsDba;
					if (!result)
						diagnostic.Set(DiagnosticType.Error, string.Format(OracleDataAccessStrings.IsNotDba, oracleUserImpersonated.Login, oracleUserImpersonated.OracleService));
				}
			}
			else 
				if ((this.typeOfRole & Role.User) == Role.User)
					result = SuccessConnection(oracleUserImpersonated);
	
			return result;
		}
		#endregion

#pragma warning disable 0618
		// disabilito temporaneamente warning CS0618: 'System.Data.OracleClient.OracleConnection' is obsolete: 
		// 'OracleConnection has been deprecated. http://go.microsoft.com/fwlink/?LinkID=144260'

		#region CheckIsDba - True se l'utente è un amministratore
		/// <summary>
		/// CheckIsDba
		/// True se l'utente è un amministratore
		/// </summary>
		/// <param name="userImpersonate"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool CheckIsDba(OracleUserImpersonatedData userImpersonate)
		{
			bool isDba = false;

			OracleConnection	testOracleConnection	= null;
			OracleDataReader	reader					= null;
			OracleCommand		command					= null; 

			OracleUserInfo oracleUserInfo		= new OracleUserInfo();
			oracleUserInfo.OracleService		= userImpersonate.OracleService;
			oracleUserInfo.OracleUserId			= userImpersonate.Login;
			oracleUserInfo.OracleUserPwd		= userImpersonate.Password;
			oracleUserInfo.OracleUserIsWinNT	= userImpersonate.WindowsAuthentication;
			oracleUserInfo.BuildStringConnection();

			if (oracleUserInfo.ConnectionString.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(OracleDataAccessStrings.ConnectionFailed, oracleUserInfo.OracleService, oracleUserInfo.OracleUserId));
				return isDba;
			}

			try
			{
				//tento la connessione	
				testOracleConnection = new OracleConnection(oracleUserInfo.ConnectionString);
				testOracleConnection.Open();
				//ora provo a fare una query
				command = new OracleCommand("select USERNAME from DBA_USERS", testOracleConnection);
				reader = command.ExecuteReader();	
				reader.Close();
				//è andato tutto bene
				isDba = true;
			}
			catch(OracleException exOracleConnection)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				if (exOracleConnection.InnerException != null)
				{
					extendedInfo.Add(DatabaseLayerStrings.Description,	exOracleConnection.InnerException.Message);
					extendedInfo.Add(DatabaseLayerStrings.Source,		exOracleConnection.InnerException.Source);
					extendedInfo.Add(DatabaseLayerStrings.StackTrace,	exOracleConnection.InnerException.StackTrace);
					extendedInfo.Add(DatabaseLayerStrings.Procedure,		exOracleConnection.InnerException.TargetSite.Name);
				}
				else
				{
					extendedInfo.Add(DatabaseLayerStrings.Description,	exOracleConnection.Message);
					extendedInfo.Add(DatabaseLayerStrings.Source,		exOracleConnection.Source);
					extendedInfo.Add(DatabaseLayerStrings.StackTrace,	exOracleConnection.StackTrace);
					extendedInfo.Add(DatabaseLayerStrings.Procedure,		exOracleConnection.TargetSite.Name);
				}
				extendedInfo.Add(DatabaseLayerStrings.Function,	 "CheckIsDba");
				extendedInfo.Add(DatabaseLayerStrings.Library,	 "Microarea.TaskBuilderNet.Data.OracleDataAccess");
				diagnostic.Set(DiagnosticType.Error, string.Format(OracleDataAccessStrings.ConnectionFailed, oracleUserInfo.OracleService, oracleUserInfo.OracleUserId), extendedInfo);
			}
			finally
			{
				if (testOracleConnection != null)
				{
					testOracleConnection.Close();
					testOracleConnection.Dispose();
					if (reader != null && !reader.IsClosed)
						reader.Close();
				}
			}
			return isDba;
		}
		#endregion

#pragma warning restore 0618

		#region SuccessConnection - Verifica che con le credenziali sia possibile connettersi
		/// <summary>
		/// SuccessConnection
		/// Verifica che con le credenziali sia possibile connettersi
		/// </summary>
		/// <param name="loginName"></param>
		/// <param name="loginPassword"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool SuccessConnection(OracleUserImpersonatedData userImpersonate)
		{
			OracleAccess tentativeConn = new OracleAccess();

			tentativeConn.LoadUserData(userImpersonate.OracleService, userImpersonate.Login, userImpersonate.Login, userImpersonate.Password, userImpersonate.WindowsAuthentication);

			bool successConnection = tentativeConn.TryToConnect(userImpersonate);
			if (tentativeConn.Diagnostic.Error || tentativeConn.Diagnostic.Warning || tentativeConn.Diagnostic.Information)
				diagnostic.Set(tentativeConn.Diagnostic);
			
			return successConnection;
		}
		#endregion

		#region CheckData - Verifica la correttezza sintattica degli input utente
		/// <summary>
		/// CheckData
		/// Verifica la correttezza sintattica degli input utente
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------------
		private bool CheckData()
		{
			bool result = true;
			if (TbLogin.Text.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, OracleDataAccessStrings.EmptyLoginName);
				result = false;
			}
			//TODO UNICODE c'era £ e mancava l' euro
			if (RadioOracleServerAuthentication.Checked && TbLogin.Text.IndexOfAny(new char[] {'?', '*', '"', '$', '&', '/', Path.DirectorySeparatorChar, '(', ')', '=', '[', '#', ']', '<', '>', ':', '!', '|'}) != -1)
			{
				diagnostic.Set(DiagnosticType.Error, OracleDataAccessStrings.WrongCharacters);
				result = false;
			}
			//TODO UNICODE c'era £ e mancava l' euro
			if (RadioWindowsAuthentication.Checked && TbLogin.Text.IndexOfAny(new char[] {'?', '*', '"', '$', '&', '/', '(', ')', '=', '[', '#', ']', '<', '>', ':', '!', '|'}) != -1)
			{
				diagnostic.Set(DiagnosticType.Error, OracleDataAccessStrings.WrongCharacters);
				result = false;
			}
			if (TbPassword.Text.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, OracleDataAccessStrings.EmptyPassword);
				result = false;
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

		#endregion

		#region Funzioni di Load e Closing della Form

		#region AdminCredential_Closing - Chiusura se è tutto ok
		/// <summary>
		/// AdminCredential_Closing
		/// Chiusura se è tutto ok
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------------
		private void AdminCredential_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if ((Success) || (cancelButton))
				e.Cancel = false;
			else 
				e.Cancel = true;
		}
		#endregion

		#region AdminCredential_Load - Al caricamento della form imposto i Strings
		/// <summary>
		/// AdminCredential_Load
		/// Caricamento della form - imposto i Strings
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------------
		private void AdminCredential_Load(object sender, System.EventArgs e)
		{
            SettingToolTips();
		}
		#endregion

		#endregion

		#region Eventi sui controlli

		#region TbLogin_TextChanged - se la login è vuota il bottone di Ok è disabilitato
		/// <summary>
		/// TbLogin_TextChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------------
		private void TbLogin_TextChanged(object sender, System.EventArgs e)
		{
			if (this.RadioOracleServerAuthentication.Checked)
			{
				if (TbLogin.Text.Length == 0)
					BtnOK.Enabled = false;
				else
					BtnOK.Enabled = true;
			}
			else if (this.RadioWindowsAuthentication.Checked)
			{
				if (TbLogin.Text.Length == 0 || this.TbPassword.Text.Length == 0)
					BtnOK.Enabled = false;
				else
					BtnOK.Enabled = true;
			}
		}
		#endregion
		
		#region RadioWindowsAuthentication_CheckedChanged - Ho selezionato la sicurezza integrata
		/// <summary>
		/// RadioWindowsAuthentication_CheckedChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------------
		private void RadioWindowsAuthentication_CheckedChanged(object sender, System.EventArgs e)
		{
			TbLogin.Text = WindowsIdentity.GetCurrent().Name.ToUpper(CultureInfo.InvariantCulture);
			TbPassword.Text = string.Empty;
			if (RadioWindowsAuthentication.Checked)
			{
				ComboDomains.Enabled = true;
				LoadDomains();
			}
			else
				ComboDomains.Enabled = false;
		}
		#endregion

		#region RadioOracleServerAuthentication_CheckedChanged - Ho selezionato la sicurezza non integrata
		/// <summary>
		/// RadioOracleServerAuthentication_CheckedChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------------
		private void RadioOracleServerAuthentication_CheckedChanged(object sender, System.EventArgs e)
		{
			TbLogin.Text	= string.Empty;
			TbPassword.Text = string.Empty;
		}
		#endregion

		#region TbPassword_Enter - Mi posiziono sulla pwd
		/// <summary>
		/// TbPassword_Enter - Mi posiziono sulla pwd
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------------
		private void TbPassword_Enter(object sender, System.EventArgs e)
		{
			TbPassword.Focus();
		}
		#endregion

		#region TbPassword_TextChanged - Introduco una pwd, se la pwd<>vuoto, abilito il bottone di Ok
		/// <summary>
		/// TbPassword_TextChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void TbPassword_TextChanged(object sender, System.EventArgs e)
		{
			if (this.RadioWindowsAuthentication.Checked)
			{
				if (TbPassword.Text.Length == 0)
					BtnOK.Enabled = false;
				else
					BtnOK.Enabled = true;
			}
		}
		#endregion

		#endregion

		#region Funzioni per determinare il dominio/workgroup NT
		/// <summary>
		/// ComboDomains_DropDown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void ComboDomains_DropDown(object sender, System.EventArgs e)
		{
			LoadDomains();
		}

		/// <summary>
		/// LoadDomains
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadDomains()
		{
			ComboDomains.Items.Clear();
			ComboDomains.Items.Add(System.Net.Dns.GetHostName().ToUpper(CultureInfo.InvariantCulture));
			ComboDomains.Items.Add(SystemInformation.UserDomainName);
		}
		#endregion
	}
}