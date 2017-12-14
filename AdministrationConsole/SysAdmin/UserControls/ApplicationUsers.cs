using System.Collections;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
    public enum ProviderType { SQLServer, ORACLE, POSTGRE };
	
	/// <summary>
	/// ApplicationUsers
	/// Carica gli utenti applicativi in base ad alcune politiche
	/// </summary>
	// ========================================================================
	public partial class ApplicationUsers : System.Windows.Forms.ComboBox
	{
		#region Eventi
		//---------------------------------------------------------------------
		public delegate void SendDiagnostic (object sender, Diagnostic diagnostic);
		public event         SendDiagnostic	OnSendDiagnostic;
		#endregion

		#region Variabili private
		//---------------------------------------------------------------------
		private string        serverNameSystemDb           = string.Empty;
		private string		  serverName				   = string.Empty;
		private string		  istanceName				   = string.Empty;
		private string        companyId                    = string.Empty;
		private ProviderType  companyProvider			   = ProviderType.SQLServer;
		private string        connectionString             = string.Empty;
		private string        selectedUserId               = string.Empty;
		private string        selectedUserName             = string.Empty;
		private string        selectedUserPwd              = string.Empty;
		private bool		  selectedUserIsWinNT          = false;
		private bool		  viewLocalUsers			   = true;
		private bool		  viewNTUsers				   = true;
		private bool		  onlyNotAssignedUsers		   = true;
		private bool          includeGuest				   = false;
		private bool          includeEasyLookSystem        = false;
		private Diagnostic    diagnostic				   = new Diagnostic("ApplicationUsers");
		private SqlConnection currentConnection            = null;
		#endregion

		#region Proprietà del controllo
		//---------------------------------------------------------------------
		//Nome del server su cui sta il db di sistema (valuto se è un mixed server NT oppure no)
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string ServerName { get { return serverName; } set { serverName = value; }}
		//Nome dell'istanza su cui sta il db di sistema (valuto se è un mixed server  oppure no)
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string IstanceName { get { return istanceName; } set { istanceName = value; }}
		//Id dell'azienda (vuota se sono in inserimento)
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string CompanyId { get { return companyId; } set { companyId = value; }}
		//Stringa di connessione al db di sistema
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string ConnectionString { get { return connectionString; } set { connectionString = value; }}
		//Connessione corrente
		public SqlConnection CurrentConnection { get { return currentConnection; } set { currentConnection = value; }}
		//Tipo Azienda (ORACLE o SQL)
		[DefaultValue(ProviderType.SQLServer), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public ProviderType CompanyProvider { get { return companyProvider; } set { companyProvider = value; }}
		//false non visualizzo gli utenti NT locali
		[DefaultValue(true), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public bool ViewLocalUsers { get { return viewLocalUsers; } set { viewLocalUsers = value; }}
		//true se visualizzo tutti gli utenti NT
		[DefaultValue(true), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public bool ViewNTUsers { get { return viewNTUsers; } set { viewNTUsers = value; }}
		//true se voglio caricare solo gli utenti applicativi non ancora assegnati
		[DefaultValue(true), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public bool OnlyNotAssignedUsers { get { return onlyNotAssignedUsers; } set { onlyNotAssignedUsers = value; }}
		//se nell'elenco degli utenti si deve includere anche l'utente guest
		[DefaultValue(false), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public bool IncludeGuest { get { return includeGuest; } set { includeGuest = value; }}
		//se nell'elenco degli utenti si deve includere anche l'utente EasyLookSystem
		[DefaultValue(false), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public bool IncludeEasyLookSystem { get { return includeEasyLookSystem; } set { includeEasyLookSystem = value; }}

		public string SelectedUserName { get { return selectedUserName;} set { selectedUserName = value; }}
		//Pwd Utente Applicativo selezionato
		public string SelectedUserPwd { get { return selectedUserPwd;} set { selectedUserPwd = value; }}
		//Id Utente Applicativo selezionato
		public string SelectedUserId { get { return selectedUserId;} set { selectedUserId = value; }}
		//True se l' Utente Applicativo selezionato è NT
		public bool SelectedUserIsWinNT { get { return selectedUserIsWinNT;} set { selectedUserIsWinNT = value; }}
		#endregion

		#region Costruttore
		/// <summary>
		/// ApplicationUsers
		/// </summary>
		//---------------------------------------------------------------------
		public ApplicationUsers()
		{
			InitializeComponent();
			diagnostic.Clear();
		}
		#endregion

		#region Metodi Pubblici richiamabili al di fuori dello UserControl

		#region ClearUsers - Clear della combo
		/// <summary>
		/// ClearUsers
		/// Clear della combo
		/// </summary>
		//---------------------------------------------------------------------
		public void ClearUsers()
		{
			if (this.Items.Count > 0)
			{
				this.DataSource = null;
				this.Items.Clear();
			}
		}
		#endregion

		#region LoadAssociatedUsers 
		/// <summary>
		/// LoadAssociatedUsers
		/// Carico gli Utenti Applicativi 
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadAssociatedUsers()
		{
			//Il server dove voglio salvare e/o modificare il db dell'azienda devo verificare se è un'istanza o un'istallazione primaria
			ServerName = currentConnection.DataSource.Split(Path.DirectorySeparatorChar)[0];

			if (currentConnection.DataSource.Split(Path.DirectorySeparatorChar).Length > 1)
				IstanceName = currentConnection.DataSource.Split(Path.DirectorySeparatorChar)[1];
			
			ClearUsers();

			ArrayList usersList				 = new ArrayList();
			UserDb userDb					 = new UserDb();
			userDb.ConnectionString			 = ConnectionString;
			userDb.CurrentSqlConnection		 = CurrentConnection;
				
			if (string.Compare(Dns.GetHostName().ToUpper(CultureInfo.InvariantCulture), ServerName, true, CultureInfo.InvariantCulture) == 0)
			{
				bool result = userDb.SelectAllUsers(out usersList, false);
				if (!result)
				{
					diagnostic.Set(userDb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					return;
				}
			}
			else
			{
				bool result = userDb.SelectAllUsersExceptLocal(out usersList, ServerName);
				if (!result)
				{
					diagnostic.Set(userDb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					return;
				}
			}

			//elimino da usersList tutti quegli utenti che sono già associati alla company
			ArrayList onlyNotAssignedUsers = new ArrayList();
			//se sono in modifica dell'azienda
			if (OnlyNotAssignedUsers && CompanyId.Length > 0)
			{
				for (int i = 0; i < usersList.Count; i++)
				{
					UserItem currentUser = (UserItem)usersList[i];
					if (currentUser == null)
						continue;
					CompanyUserDb companyUser			= new CompanyUserDb();
					companyUser.ConnectionString		= ConnectionString;
					companyUser.CurrentSqlConnection	= CurrentConnection;
					
					if ((companyUser.ExistUser(currentUser.LoginId, CompanyId) != 0) &&
						!companyUser.IsDbo(currentUser.LoginId, CompanyId))
						continue;
					if (string.Compare(currentUser.Login, NameSolverStrings.GuestLogin, true, CultureInfo.InvariantCulture) == 0 && !includeGuest)
						continue;
					if (string.Compare(currentUser.Login, NameSolverStrings.EasyLookSystemLogin, true, CultureInfo.InvariantCulture) == 0 && !includeEasyLookSystem)
						continue;
					
					onlyNotAssignedUsers.Add(usersList[i]);
				}
			}
			else
			{
				for (int i = 0; i < usersList.Count; i++)
				{
					UserItem currentUser = (UserItem)usersList[i];
					if (currentUser == null)
						continue;
					if (string.Compare(currentUser.Login, NameSolverStrings.GuestLogin, true, CultureInfo.InvariantCulture) == 0 && !includeGuest)
						continue;
					if (string.Compare(currentUser.Login, NameSolverStrings.EasyLookSystemLogin, true, CultureInfo.InvariantCulture) == 0 && !includeEasyLookSystem)
						continue;
					onlyNotAssignedUsers.Add(usersList[i]);
				}
				//onlyNotAssignedUsers = usersList;
			}

			if (onlyNotAssignedUsers.Count > 0)
			{
				this.DataSource		=  onlyNotAssignedUsers;
				this.DisplayMember	= "Login";
				this.ValueMember	= "LoginId";
			}
		}
		#endregion

		#region SelectUser - Seleziono l'utente userName nella lista
		/// <summary>
		/// SelectUser
		/// </summary>
		/// <param name="userName"></param>
		//---------------------------------------------------------------------
		public void SelectUser(string userName)
		{
			for (int j=0; j < this.Items.Count; j++)
			{
				UserItem myItemUser = (UserItem)this.Items[j];
				
				if (myItemUser == null)
					continue;
				
				if (string.Compare(userName, myItemUser.Login, true, CultureInfo.InvariantCulture) == 0)
				{
					this.SelectedIndex = j;
					break;
				}
			}

			//se non l'ho trovato, prendo il primo
			if ((this.SelectedIndex == -1) && (this.Items.Count > 0))
				this.SelectedIndex = 0;
		}
		#endregion

		#endregion

		#region ApplicationUsers_SelectedIndexChanged - Selezione di un Utente Applicativo
		/// <summary>
		/// ApplicationUsers_SelectedIndexChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void ApplicationUsers_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (this.SelectedItem != null)
			{
				selectedUserName = ((UserItem)this.SelectedItem).Login;
				selectedUserId = ((UserItem)this.SelectedItem).LoginId;
				selectedUserPwd = ((UserItem)this.SelectedItem).Password;
				selectedUserIsWinNT = ((UserItem)this.SelectedItem).WindowsAuthentication;
			}
		}
		#endregion
	}
}
