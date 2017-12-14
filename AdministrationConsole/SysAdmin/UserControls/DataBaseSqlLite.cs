using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
	/// <summary>
	/// DataBaseSql.
	/// Tab di database per l'azienda con provider SQL
	/// </summary>
	//=========================================================================
	public partial class DataBaseSqlLite : System.Windows.Forms.UserControl
	{
		#region Eventi
		public delegate void CallHelp(object sender, string nameSpace, string searchParameter);
		public event CallHelp OnCallHelp;

		// evento da agganciare per settare il valore della checkbox Unicode
		public delegate void SetValueForUnicodeCheckBox(bool setRO, bool setChecked);
		public event SetValueForUnicodeCheckBox OnSetValueForUnicodeCheckBox;

		// evento da agganciare per impostare uno specifico utente nella combobox
		public delegate void SetValueUsersComboBox(string selectedUser);
		public event SetValueUsersComboBox OnSetValueUsersComboBox;

		// evento da agganciare per impostare uno specifico nome al database da creare
		public delegate void SetNewDatabaseName();
		public event SetNewDatabaseName OnSetNewDatabaseName;

		//---------------------------------------------------------------------
		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;
		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;
		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;
		#endregion

		#region Variabili
		//---------------------------------------------------------------------
		//IN
		private string dataSourceSystemDb = string.Empty;
		private string serverNameSysDb = string.Empty;
		private string serverIstanceSystemDb = string.Empty;
		private string userConnected = string.Empty;
		private string userPwdConnected = string.Empty;
		private string connectionString = string.Empty;
		private string companyId = string.Empty;
		private string companyDbName = string.Empty;
		private string company = string.Empty;
		private string newCompanyDbName = string.Empty;
		private bool mustUseUnicodeSet = false;
		//OUT
		private string selectedSQLServerName = string.Empty;
		private string selectedSQLIstanceName = string.Empty;
		private string selectedSQLNewDatabaseName = string.Empty;
		private string selectedSQLDatabaseName = string.Empty;
		private string selectedDbOwnerId = string.Empty;
		private string selectedDbOwnerName = string.Empty;
		private string selectedDbOwnerPwd = string.Empty;
		private bool selectedDbOwnerIsWinNT = false;
		private bool isNewSQLCompany = false;
		private bool inserting = false;
		private bool isDmsTab = false;

		private SqlConnection currentConnection = null;
		private Diagnostic diagnostic = new Diagnostic("DatabaseSql");
		#endregion

		#region Proprietà del controllo
		//Proprietà esposte nel design
		//---------------------------------------------------------------------
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string ServerNameSystemDb { get { return serverNameSysDb; } set { serverNameSysDb = value; } }
		//---------------------------------------------------------------------
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string ServerIstanceSystemDb { get { return serverIstanceSystemDb; } set { serverIstanceSystemDb = value; } }
		//---------------------------------------------------------------------
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string DataSourceSystemDb { get { return dataSourceSystemDb; } set { dataSourceSystemDb = value; } }
		//---------------------------------------------------------------------
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string UserConnected { get { return userConnected; } set { userConnected = value; } }
		//---------------------------------------------------------------------
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string UserPwdConnected { get { return userPwdConnected; } set { userPwdConnected = value; } }
		//---------------------------------------------------------------------
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string CompanyId { get { return companyId; } set { companyId = value; } }
		//---------------------------------------------------------------------
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string ConnectionString { get { return connectionString; } set { connectionString = value; } }

		//Proprietà pubbliche
		//---------------------------------------------------------------------
		public SqlConnection CurrentConnection { get { return currentConnection; } set { currentConnection = value; } }

		public string CompanyDbName
		{
			get { return companyDbName; }
			set
			{
				companyDbName = value;
				if (string.IsNullOrWhiteSpace(TxtDatabaseName.Text) && inserting)
					TxtDatabaseName.Text = companyDbName;
			}
		}
		public string Company { get { return company; } set { company = value; } }
		public string SelectedSQLServerName { get { return selectedSQLServerName; } set { selectedSQLServerName = value; } }
		public string SelectedSQLIstanceName { get { return selectedSQLIstanceName; } set { selectedSQLIstanceName = value; } }
		public string SelectedSQLNewDatabaseName { get { return selectedSQLNewDatabaseName; } set { selectedSQLNewDatabaseName = value; } }
		public string SelectedSQLDatabaseName { get { return selectedSQLDatabaseName; } set { selectedSQLDatabaseName = value; } }
		public string SelectedDbOwnerId { get { return selectedDbOwnerId; } set { selectedDbOwnerId = value; } }
		public string SelectedDbOwnerName { get { return selectedDbOwnerName; } set { selectedDbOwnerName = value; } }
		public string SelectedDbOwnerPwd { get { return selectedDbOwnerPwd; } set { selectedDbOwnerPwd = value; } }
		public bool SelectedDbOwnerIsWinNT { get { return selectedDbOwnerIsWinNT; } set { selectedDbOwnerIsWinNT = value; } }
		public bool IsNewSQLCompany { get { return isNewSQLCompany; } set { isNewSQLCompany = value; } }
		public bool Inserting { get { return inserting; } set { inserting = value; } }

		// serve all'esterno per decidere se effettuare la ShowDialog oppure eseguire la creazione del db in modalita' silente
		public bool ShowCreationParameters { get { return false; } }

		public Diagnostic DBSqlDiagnostic { get { return diagnostic; } }
		#endregion

		#region Costruttore
		/// <summary>
		/// DataBaseSql constructor
		/// </summary>
		//---------------------------------------------------------------------
		public DataBaseSqlLite(bool useUnicodeSet)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			mustUseUnicodeSet = useUnicodeSet;
			diagnostic.Clear();
		}
		#endregion

		# region LoadData
		/// <summary>
		/// LoadData (richiamata dalla form della Company)
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadData(bool isDmsTab)
		{
			this.isDmsTab = isDmsTab;

			string serverName = string.IsNullOrWhiteSpace(SelectedSQLServerName) ? ServerNameSystemDb : SelectedSQLServerName;
			string instanceName = string.IsNullOrWhiteSpace(SelectedSQLIstanceName) ? ServerIstanceSystemDb : SelectedSQLIstanceName;

			// per il DMS ho gia' pre-impostato il nome del server, quindi non serve impostare quello del db di sistema
			if (isDmsTab)
			{
				serverName = SelectedSQLServerName;
				instanceName = SelectedSQLIstanceName;
			}

			// inizializzo la combo con il server dell'azienda di origine
			if (!string.IsNullOrWhiteSpace(serverName))
				TxtServerName.Text = (instanceName.Length > 0) ? Path.Combine(serverName, instanceName) : serverName;

			SelectedSQLServerName = serverName;
			SelectedSQLIstanceName = instanceName;

			if (!isDmsTab)
			{
				// Carico la combo degli utenti Applicativi
				if (cbUserOwner.Items.Count == 0)
				{
					cbUserOwner.ServerName = serverName;
					cbUserOwner.IstanceName = instanceName;
					cbUserOwner.ConnectionString = ConnectionString;
					cbUserOwner.CurrentConnection = CurrentConnection;
					cbUserOwner.CompanyId = this.CompanyId;
					cbUserOwner.LoadAssociatedUsers();

					if (cbUserOwner.Items.Count > 0)
					{
						string userToSelect = (string.IsNullOrWhiteSpace(SelectedDbOwnerName)) ? UserConnected : SelectedDbOwnerName;

						bool found = false;
						foreach (UserItem ui in cbUserOwner.Items)
						{
							if (string.Compare(ui.Login, userToSelect, StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								found = true;
								break;
							}
						}

						if (!found)
							if (string.IsNullOrWhiteSpace(userToSelect))
								userToSelect = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
						cbUserOwner.SelectUser(userToSelect);

						SelectedDbOwnerId = cbUserOwner.SelectedUserId;
						SelectedDbOwnerName = cbUserOwner.SelectedUserName;
						SelectedDbOwnerPwd = cbUserOwner.SelectedUserPwd;
						SelectedDbOwnerIsWinNT = cbUserOwner.SelectedUserIsWinNT;
					}
					else
					{
						diagnostic.Set(DiagnosticType.Warning, Strings.NotSelectedCompanyUsers);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						diagnostic.Clear();
					}
				}
				else
				{
					string userToSelect = UserConnected;
					bool isFound = false;
					foreach (UserItem ui in cbUserOwner.Items)
					{
						if (string.Compare(ui.Login, userToSelect, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							isFound = true;
							break;
						}
					}

					if (isFound)
						cbUserOwner.SelectUser(UserConnected);
					else
					{
						// questo e' un workaround per valorizzare correttamente la combobox degli utenti con
						// il nome del dbowner. si verifica per il db Easy Attachment agganciato ad un database az. Oracle
						// (in questo caso l'assegnazione del dbowner e' diversa)
						foreach (UserItem ui in cbUserOwner.Items)
						{
							if (string.Compare(ui.LoginId, SelectedDbOwnerId, StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								cbUserOwner.SelectUser(ui.Login);
								break;
							}
						}
					}
				}
			}
			else
			{
				string userNameToForce = SelectedDbOwnerName;

				// Carico la combo degli utenti Applicativi
				cbUserOwner.ServerName = serverName;
				cbUserOwner.IstanceName = instanceName;
				cbUserOwner.ConnectionString = ConnectionString;
				cbUserOwner.CurrentConnection = CurrentConnection;
				cbUserOwner.CompanyId = this.CompanyId;
				cbUserOwner.LoadAssociatedUsers();

				cbUserOwner.SelectUser(userNameToForce);
			}

			TxtDatabaseName.Text = this.CompanyDbName;
			selectedSQLDatabaseName = TxtDatabaseName.Text;
		}
		# endregion

		#region Metodi Pubblici
		///<summary>
		/// Disabilita la groupbox con le informazioni dei database
		/// Utilizzata con la Standard Edition per il limite delle due aziende.
		///</summary>
		//---------------------------------------------------------------------
		public void HideDbChanges()
		{
			TxtDatabaseName.Enabled = false;
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

		///<summary>
		/// Metodo che mi serve per forzare la selezione di un utente applicativo
		/// specifico nell'apposita combobox.
		/// Utilizzato con la gestione degli slave
		/// </summary>
		//---------------------------------------------------------------------
		public void ForceSelectedUser(string userName)
		{
			cbUserOwner.SelectUser(userName);
		}
		#endregion

		/// <summary>
		/// cbUserOwner_SelectedIndexChanged
		/// Selezione di un Utente Applicativo dalla combobox
		/// </summary>
		//---------------------------------------------------------------------
		private void cbUserOwner_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SelectedDbOwnerId = cbUserOwner.SelectedUserId;
			SelectedDbOwnerName = cbUserOwner.SelectedUserName;
			SelectedDbOwnerPwd = cbUserOwner.SelectedUserPwd;
			SelectedDbOwnerIsWinNT = cbUserOwner.SelectedUserIsWinNT;

			// sparo un evento alla form che contiene lo user control per notificare
			// il nome dell'utente applicativo selezionato dall'apposita combobox
			if (OnSetValueUsersComboBox != null)
				OnSetValueUsersComboBox(SelectedDbOwnerName);
		}

		/// <summary>
		/// TestUnicode
		/// Accedo al server selezionato, costruisco la stringa di connessione e vado a vedere sulla tabella
		/// TB_DBMark se la colonna Application è di tipo nvarchar.
		/// </summary>
		//---------------------------------------------------------------------
		private void TestUnicode()
		{
			bool dbMarkExist = false;
			bool useUnicode = CheckUnicodeOnDB(out dbMarkExist);

			// Questo evento e' richiamato SOLO per il database aziendale, in modo da sincronizzare 
			// in automatico il valore della checkbox 'Use unicode' con l'effettivo valore del database
			if (OnSetValueForUnicodeCheckBox != null)
				OnSetValueForUnicodeCheckBox(true, useUnicode);
		}

		/// <summary>
		/// CheckUnicodeOnDB
		/// Accedo al server selezionato, costruisco la stringa di connessione e vado a vedere sulla tabella
		/// TB_DBMark se la colonna Application è di tipo nvarchar.
		/// Questo metodo viene richiamato anche in fase di salvataggio dell'azienda, in modo da confrontare 
		/// se lo stato Unicode sul db aziendale e sul db documentale coincidono.
		/// Se cosi' non fosse non procedo con la creazione e visualizzo opportuno messaggio.
		/// </summary>
		//---------------------------------------------------------------------
		public bool CheckUnicodeOnDB(out bool dbMarkExist)
		{
			dbMarkExist = false;

			if (string.IsNullOrEmpty(selectedSQLDatabaseName))
				return false;

			string serverName = TxtServerName.Text;
			// devo provare a connettermi al db con le credenziali del db di sistema
			string connectionString = 
				string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, selectedSQLDatabaseName,	selectedDbOwnerName, selectedDbOwnerPwd);

			TBConnection myConnection = null;
			TBDatabaseSchema mySchema = null;
			bool useUnicode = false;

			try
			{
				// istanzio una TBConnection e la apro
				myConnection = new TBConnection(connectionString, DBMSType.SQLSERVER);
				myConnection.Open();

				// istanzio TBDatabaseSchema sulla connessione
				mySchema = new TBDatabaseSchema(myConnection);

				// se la tabella di riferimento TB_DBMark NON esiste, restituisco il valore unicode impostato dall'utente
				// altrimenti procedo con il controllo sulla tabella....
				if (mySchema.ExistTable(DatabaseLayerConsts.TB_DBMark))
				{
					// analizzo lo schema della tabella e verifico il tipo della colonna Application
					DataTable cols = mySchema.GetTableSchema(DatabaseLayerConsts.TB_DBMark, false);

					foreach (DataRow col in cols.Rows)
					{
						if (string.Compare(col["ColumnName"].ToString(), "Application", StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							TBType providerType = (TBType)((SqlDbType)col["ProviderType"]);
							useUnicode =
								//string.Compare(TBDatabaseType.GetDBDataType(providerType, DBMSType.SQLSERVER), "NVarChar", StringComparison.InvariantCultureIgnoreCase) == 0;
								string.Compare(col["DataTypeName"].ToString(), "NVarChar", StringComparison.InvariantCultureIgnoreCase) == 0;							
							break;
						}
					}
					dbMarkExist = true;
				}
				else
				{
					// il database e' vuoto, quindi imposto il valore unicode come l'azienda
					dbMarkExist = false;

					//useUnicode = mustUseUnicodeSet;
				}
			}
			catch (TBException e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				ei.Add(DatabaseLayerStrings.Number, e.Number);
				ei.Add(DatabaseLayerStrings.Function, "CheckUnicodeOnDB");
				ei.Add(DatabaseLayerStrings.Library, "Microarea.Console.Plugin.SysAdmin.UserControl.DatabaseSqlLite");
				ei.Add(DatabaseLayerStrings.Source, e.Source);
				ei.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.ErrDataBaseConnection, selectedSQLDatabaseName), ei);
			}
			finally
			{
				if (myConnection != null && myConnection.State != ConnectionState.Closed)
				{
					myConnection.Close();
					myConnection.Dispose();
				}
			}

			return useUnicode;
		}

		#region Funzioni per la verifica dell'autenticazione di un utente (viene interrogata la Console)
		/// <summary>
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
		/// (già inserita precedentemente poichè l'utente in questione risulta autenticato)
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
		#endregion

		# region Gestione Help
		//---------------------------------------------------------------------
		private void SendHelp(object sender, string searchParameter)
		{
			if (this.OnCallHelp != null)
				OnCallHelp(sender, "Module.MicroareaConsole.SysAdmin", searchParameter);
		}

		//---------------------------------------------------------------------
		private void SendCompleteHelp(object sender, string nameSpace, string searchParameter)
		{
			if (this.OnCallHelp != null)
				OnCallHelp(sender, nameSpace, searchParameter);
		}
		#endregion

		/// <summary>
		/// Sulla Leave della textbox dei database vado a controllare se il database è stato creato con 
		/// il set di caratteri Unicode, in modo da settare correttamente la check-box nella tab dei Parametri
		/// </summary>
		//---------------------------------------------------------------------
		private void TxtDatabaseName_Leave(object sender, EventArgs e)
		{
			selectedSQLDatabaseName = TxtDatabaseName.Text;

			if (!isDmsTab) // il test unicode va fatto solo sulla company
				TestUnicode();

			if (OnSetNewDatabaseName != null)
				OnSetNewDatabaseName();
		}

		private void TxtDatabaseName_TextChanged(object sender, EventArgs e)
		{
			selectedSQLDatabaseName = TxtDatabaseName.Text;
		}
	}
}