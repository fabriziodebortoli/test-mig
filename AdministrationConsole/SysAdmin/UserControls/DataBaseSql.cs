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
using Microarea.TaskBuilderNet.Data.SQLDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
	/// <summary>
	/// DataBaseSql.
	/// Tab di database per l'azienda con provider SQL
	/// </summary>
	//=========================================================================
	public partial class DataBaseSql : System.Windows.Forms.UserControl
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

		private SqlConnection currentConnection = null;
		private Diagnostic diagnostic = new Diagnostic("DatabaseSql");
		private SQLCredential userCredential = null;
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

		public string NewCompanyDbName
		{
			get { return newCompanyDbName; }
			set
			{
				newCompanyDbName = value;
				if (rbNewDb.Checked)
					txtNewDataBaseName.Text = newCompanyDbName;
			}
		}

		public string CompanyDbName { get { return companyDbName; } set { companyDbName = value; } }
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
		public bool ShowCreationParameters { get { return CbShowCreationParam.Checked; } }
		#endregion

		#region Costruttore
		/// <summary>
		/// DataBaseSql constructor
		/// </summary>
		//---------------------------------------------------------------------
		public DataBaseSql(bool useUnicodeSet)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			txtNewDataBaseName.Text = string.Empty;
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
			{
				if (instanceName.Length > 0)
					NGSqlServersCombo.InitDefaultServer(Path.Combine(serverName, instanceName));
				else
					NGSqlServersCombo.InitDefaultServer(serverName);
			}

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

			// Inizializzo la combo dei DBs
			DatabasesComboBox.Add(this.CompanyDbName);
			DatabasesComboBox.SelectIndex(0);
			selectedSQLDatabaseName = string.IsNullOrWhiteSpace(CompanyDbName) ? DatabasesComboBox.DataSourceName : CompanyDbName;
			DatabasesComboBox.UserName = SelectedDbOwnerName;
			DatabasesComboBox.UserPassword = SelectedDbOwnerPwd;
			DatabasesComboBox.IsWindowsAuthentication = SelectedDbOwnerIsWinNT;
		}

		/// <summary>
		/// LoadDataForMixedDbType (richiamata dalla form della Company)
		/// metodo richiamato solo nel caso di situazioni miste, ovvero quando l'azienda e' Oracle
		/// e i database slave sono SQL.
		/// Questo perche' la gestione dell'utente applicativo dbowner da proporre per Oracle e' 
		/// diversa da quella di SQL.
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadDataForMixedDbType()
		{
			string serverName = SelectedSQLServerName;
			string instanceName = SelectedSQLIstanceName;

			// inizializzo la combo con il server dell'azienda di origine
			if (!string.IsNullOrWhiteSpace(serverName))
			{
				if (instanceName.Length > 0)
					NGSqlServersCombo.InitDefaultServer(Path.Combine(serverName, instanceName));
				else
					NGSqlServersCombo.InitDefaultServer(serverName);
			}

			SelectedSQLServerName = serverName;
			SelectedSQLIstanceName = instanceName;

			// Carico la combo degli utenti Applicativi
			if (cbUserOwner.Items.Count == 0)
			{
				// mi tengo da parte 
				string userNameToForce = SelectedDbOwnerName;

				cbUserOwner.ServerName = serverName;
				cbUserOwner.IstanceName = instanceName;
				cbUserOwner.ConnectionString = ConnectionString;
				cbUserOwner.CurrentConnection = CurrentConnection;
				cbUserOwner.CompanyId = this.CompanyId;
				cbUserOwner.LoadAssociatedUsers();

				cbUserOwner.SelectUser(userNameToForce);
			}
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

			// Inizializzo la combo dei DBs
			DatabasesComboBox.Add(this.CompanyDbName);
			DatabasesComboBox.SelectIndex(0);
			selectedSQLDatabaseName = string.IsNullOrWhiteSpace(CompanyDbName) ? DatabasesComboBox.DataSourceName : CompanyDbName;
			DatabasesComboBox.UserName = SelectedDbOwnerName;
			DatabasesComboBox.UserPassword = SelectedDbOwnerPwd;
			DatabasesComboBox.IsWindowsAuthentication = SelectedDbOwnerIsWinNT;
		}

		# endregion

		#region Metodi Pubblici
		/// <summary>
		/// SettingLayout - Abilita/Disabilita controlli nello UserControl
		/// </summary>
		//---------------------------------------------------------------------
		public void SettingLayout()
		{
			rbNewDb.Checked = isNewSQLCompany;
			txtNewDataBaseName.Enabled = isNewSQLCompany;
			CbShowCreationParam.Enabled = isNewSQLCompany;
			DatabasesComboBox.Enabled = !isNewSQLCompany;
			txtNewDataBaseName.Text = (isNewSQLCompany) ? NewCompanyDbName : string.Empty;
			SettingToolTips(isNewSQLCompany);
		}

		/// <summary>
		/// SettingToolTips - Imposta i toolTip
		/// </summary>
		//---------------------------------------------------------------------
		private void SettingToolTips(bool isInsertingMode)
		{
			toolTipSql.SetToolTip(cbUserOwner, Strings.SqlApplicationUsersToolTip);
			toolTipSql.SetToolTip(rbSelectExistedDb, Strings.ExistCompanyDataBaseToolTip);
			toolTipSql.SetToolTip(DatabasesComboBox, Strings.ExistCompanyDataBaseToolTip);
			toolTipSql.SetToolTip(rbNewDb, Strings.NewCompanyDatabaseToolTip);
			toolTipSql.SetToolTip(txtNewDataBaseName, Strings.NewCompanyDatabaseToolTip);

			if (isInsertingMode)
				toolTipSql.SetToolTip(NGSqlServersCombo, Strings.InsertingServerDbCompanyToolTip);
			else
			{
				toolTipSql.SetToolTip(NGSqlServersCombo, Strings.ModifyingServerDbCompanyToolTip);
				toolTipSql.SetToolTip(lblPhase1, Strings.ModifyingServerDbCompanyToolTip);
			}
		}

		///<summary>
		/// Disabilita la groupbox con le informazioni dei database
		/// Utilizzata con la Standard Edition per il limite delle due aziende.
		///</summary>
		//---------------------------------------------------------------------
		public void HideDbChanges()
		{
			GroupBoxPhase3.Enabled = false;
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

		///<summary>
		/// Metodo che mi serve per forzare la selezione di un utente applicativo
		/// specifico nell'apposita combobox.
		/// Utilizzato con la gestione degli slave
		/// </summary>
		//---------------------------------------------------------------------
		public void ForceNewDatabaseName(string databaseName)
		{
			if (txtNewDataBaseName.Enabled && string.IsNullOrWhiteSpace(txtNewDataBaseName.Text))
				txtNewDataBaseName.Text = databaseName;
		}
		#endregion

		/// <summary>
		/// DatabasesComboBox_Enter
		/// Selezione di un Db Esistente (Attach di database)
		/// </summary>
		//---------------------------------------------------------------------
		private void DatabasesComboBox_Enter(object sender, System.EventArgs e)
		{
			if (string.IsNullOrEmpty(NGSqlServersCombo.SelectedSQLServer))
			{
				diagnostic.Set(DiagnosticType.Error, Strings.NoServerSelected);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			}
			else
			{
				DatabasesComboBox.ServerName = NGSqlServersCombo.SelectedSQLServer;

				bool sameServer =
					(string.Compare(DatabasesComboBox.ServerName, this.currentConnection.DataSource, StringComparison.InvariantCultureIgnoreCase) == 0);

				if (cbUserOwner.SelectedItem == null)
				{
					diagnostic.Set(DiagnosticType.Error, Strings.NotSelectedLogins);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					return;
				}

				UserItem userOwner = (UserItem)cbUserOwner.SelectedItem;

				CompanyUserDb companyUser = new CompanyUserDb();
				companyUser.ConnectionString = this.connectionString;
				companyUser.CurrentSqlConnection = this.currentConnection;

				ArrayList companyUserData = new ArrayList();
				if (this.companyId.Length > 0)
				{
					bool existCompanyUser = companyUser.GetUserCompany(out companyUserData, userOwner.LoginId, this.companyId);
					if (existCompanyUser && companyUserData.Count > 0)
					{
						CompanyUser companyOwnerData = (CompanyUser)companyUserData[0];
						DatabasesComboBox.IsWindowsAuthentication = companyOwnerData.DBWindowsAuthentication;
						if (!companyOwnerData.DBWindowsAuthentication)
						{
							DatabasesComboBox.UserName = companyOwnerData.DBDefaultUser;
							DatabasesComboBox.UserPassword = companyOwnerData.DBDefaultPassword;
						}
						DatabasesComboBox.LoadAllDataBases(sameServer ? this.currentConnection.Database : string.Empty);
					}
					else
					{
						DatabasesComboBox.IsWindowsAuthentication = userOwner.WindowsAuthentication;
						if (!userOwner.WindowsAuthentication)
						{
							DatabasesComboBox.UserName = userOwner.Login;
							DatabasesComboBox.UserPassword = userOwner.Password;
						}
						DatabasesComboBox.LoadAllDataBases(sameServer ? this.currentConnection.Database : string.Empty);
					}
				}
				else
				{
					DatabasesComboBox.IsWindowsAuthentication = userOwner.WindowsAuthentication;
					if (!userOwner.WindowsAuthentication)
					{
						DatabasesComboBox.UserName = userOwner.Login;
						DatabasesComboBox.UserPassword = userOwner.Password;
					}
					DatabasesComboBox.LoadAllDataBases(sameServer ? this.currentConnection.Database : string.Empty);
				}

				if (DatabasesComboBox.DBItems == 0)
				{
					bool success =
						AskCredential(out userCredential, NGSqlServersCombo.ServerName, NGSqlServersCombo.InstanceName);

					if (success && userCredential != null && userCredential.userImpersonated != null)
					{
						DatabasesComboBox.IsWindowsAuthentication = userCredential.userImpersonated.WindowsAuthentication;
						DatabasesComboBox.UserName =
							(DatabasesComboBox.IsWindowsAuthentication)
							? Path.Combine(userCredential.userImpersonated.Domain, userCredential.userImpersonated.Login)
							: userCredential.userImpersonated.Login;

						DatabasesComboBox.UserPassword = userCredential.userImpersonated.Password;
						DatabasesComboBox.LoadAllDataBases(sameServer ? this.currentConnection.Database : string.Empty);
					}
				}

				for (int i = 0; i < DatabasesComboBox.DBItems; i++)
				{
					if (string.IsNullOrWhiteSpace(CompanyDbName))
					{
						DatabasesComboBox.SelectIndex(i);
						selectedSQLDatabaseName = DatabasesComboBox.DataSourceName;
						break;
					}
					else
					{
						if (string.Compare(DatabasesComboBox.IstanceItem(i), CompanyDbName, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							DatabasesComboBox.SelectIndex(i);
							selectedSQLDatabaseName = DatabasesComboBox.DataSourceName;
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// rbNewDb_CheckedChanged
		/// Evento sui radio-button di scelta del database (nuovo o già esistente)
		/// </summary>
		//---------------------------------------------------------------------
		private void rbNewDb_CheckedChanged(object sender, System.EventArgs e)
		{
			IsNewSQLCompany = ((RadioButton)sender).Checked;

			if (((RadioButton)sender).Checked)
			{
				NGSqlServersCombo.Enabled = true;
				DatabasesComboBox.Enabled = false;

				if (string.IsNullOrWhiteSpace(txtNewDataBaseName.Text))
					txtNewDataBaseName.Text = string.IsNullOrWhiteSpace(NewCompanyDbName) ? Company : NewCompanyDbName;

				SelectedSQLNewDatabaseName = txtNewDataBaseName.Text;

				txtNewDataBaseName.Enabled = true;
				CbShowCreationParam.Enabled = true;
				txtNewDataBaseName.Focus();

				if (OnSetValueForUnicodeCheckBox != null)
					OnSetValueForUnicodeCheckBox(!((RadioButton)sender).Checked, mustUseUnicodeSet);

				if (OnSetNewDatabaseName != null)
					OnSetNewDatabaseName();
			}
			else
			{
				DatabasesComboBox.Enabled = true;
				CbShowCreationParam.Enabled = false;
				txtNewDataBaseName.Enabled = false;
				txtNewDataBaseName.Text = string.Empty;

				DatabasesComboBox.IsWindowsAuthentication = selectedDbOwnerIsWinNT;
				DatabasesComboBox.UserName = selectedDbOwnerName;
				DatabasesComboBox.UserPassword = selectedDbOwnerPwd;
				DatabasesComboBox.ServerName = NGSqlServersCombo.SelectedSQLServer;

				if (!string.IsNullOrWhiteSpace(DatabasesComboBox.DataSourceName))
					selectedSQLDatabaseName = DatabasesComboBox.DataSourceName;
				else
				{
					DatabasesComboBox.ClearListOfDb();
					DatabasesComboBox.DataSourceName = string.Empty;
					DatabasesComboBox.LoadAllDataBases();

					for (int i = 0; i < DatabasesComboBox.DBItems; i++)
					{
						if (string.IsNullOrWhiteSpace(CompanyDbName))
						{
							DatabasesComboBox.SelectIndex(i);
							selectedSQLDatabaseName = DatabasesComboBox.DataSourceName;
							break;
						}
						else
						{
							if (string.Compare(DatabasesComboBox.IstanceItem(i), CompanyDbName, StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								DatabasesComboBox.SelectIndex(i);
								selectedSQLDatabaseName = DatabasesComboBox.DataSourceName;
								break;
							}
						}
					}
				}

				TestUnicode();
			}
		}

		/// <summary>
		/// NGSqlServersCombo_OnSelectedServerSQL
		/// Seleziono un server SQL dalla combobox
		/// </summary>
		//---------------------------------------------------------------------
		private void NGSqlServersCombo_OnSetSelectedServerSQL(string serverName)
		{
			if (serverName.Split(Path.DirectorySeparatorChar).Length > 1)
			{
				SelectedSQLServerName = serverName.Split(Path.DirectorySeparatorChar)[0];
				SelectedSQLIstanceName = serverName.Split(Path.DirectorySeparatorChar)[1];
			}
			else
			{
				SelectedSQLServerName = serverName.Split(Path.DirectorySeparatorChar)[0];
				SelectedSQLIstanceName = string.Empty;
			}

			// sul changed del server sql faccio la clear della combo dei database
			DatabasesComboBox.ClearListOfDb();
			DatabasesComboBox.DataSourceName = string.Empty;
		}

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

			//se sono in modifica di una azienda non pulisco la combo dei Db
			//viceversa se sono in inserimento pulisco
			if (Inserting)
			{
				DatabasesComboBox.ClearListOfDb();
				DatabasesComboBox.DataSourceName = string.Empty;
			}

			// sparo un evento alla form che contiene lo user control per notificare
			// il nome dell'utente applicativo selezionato dall'apposita combobox
			if (OnSetValueUsersComboBox != null)
				OnSetValueUsersComboBox(SelectedDbOwnerName);
		}

		/// <summary>
		/// txtNewDataBaseName_TextChanged
		/// Imposto un nome (diverso dal default) per il DB che sto creando
		/// </summary>
		//---------------------------------------------------------------------
		private void txtNewDataBaseName_TextChanged(object sender, System.EventArgs e)
		{
			SelectedSQLNewDatabaseName = ((TextBox)sender).Text;
		}

		//---------------------------------------------------------------------
		private void DatabasesComboBox_OnSelectDatabases(string dbName)
		{
			selectedSQLDatabaseName = dbName;
		}

		/// <summary>
		/// Sulla Leave della combo-box dei database vado a controllare se il database è stato creato con 
		/// il set di caratteri Unicode, in modo da settare correttamente la check-box nella tab dei Parametri
		/// </summary>
		//---------------------------------------------------------------------
		private void DatabasesComboBox_Leave(object sender, System.EventArgs e)
		{
			TestUnicode();
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
				OnSetValueForUnicodeCheckBox(rbSelectExistedDb.Checked, useUnicode);
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

			string serverName = NGSqlServersCombo.SelectedSQLServer;
			string connectionString =
				(userCredential == null)
				? ((DatabasesComboBox.IsWindowsAuthentication)
					? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, selectedSQLDatabaseName)
					: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, selectedSQLDatabaseName,
					DatabasesComboBox.UserName, DatabasesComboBox.UserPassword))
				: ((userCredential.userImpersonated.WindowsAuthentication)
					? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, selectedSQLDatabaseName)
					: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, selectedSQLDatabaseName,
					userCredential.userImpersonated.Login, userCredential.userImpersonated.Password));

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
			catch (TBException)
			{
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

		# region AskCredential (chiedo le credenziali per connettermi al server e caricare tutti i database presenti)
		/// <summary>
		/// Richiesta credenziali per accedere al server SQL e poi caricare i database presenti nell'apposita combo-box
		/// </summary>
		/// <param name="userCredential">param out: credenziali utente per accesso server</param>
		/// <param name="primarySQLServer">server sql</param>
		/// <param name="istanceSQLSever">istanza sql</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool AskCredential(out SQLCredential userCredential, string primarySQLServer, string istanceSQLSever)
		{
			bool credentialAreCorrect = false;
			string serverName = primarySQLServer;

			if (!string.IsNullOrWhiteSpace(istanceSQLSever))
				serverName = Path.Combine(primarySQLServer, istanceSQLSever);

			TransactSQLAccess currentConnection = new TransactSQLAccess();
			currentConnection.NameSpace = "Module.MicroareaConsole.SysAdmin";
			currentConnection.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(SendCompleteHelp);

			// inizializzo la finestra con l'utente sa, e poi consento cmq all'utente di cambiare le credenziali
			userCredential = new SQLCredential(serverName, DatabaseLayerConsts.LoginSa, string.Empty, SystemInformation.UserDomainName.ToUpper(CultureInfo.InvariantCulture), false, true);
			userCredential.OnOpenHelpFromPopUp += new SQLCredential.OpenHelpFromPopUp(SendHelp);
			userCredential.ShowDialog();

			if (userCredential.Success)
			{
				// se le credenziali sono corrette allora compongo la stringa di connessione e provo a connettermi
				currentConnection.CurrentStringConnection =
					(userCredential.userImpersonated.WindowsAuthentication)
					? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, DatabaseLayerConsts.MasterDatabase)
					: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase,
					userCredential.userImpersonated.Login, userCredential.userImpersonated.Password);

				credentialAreCorrect = currentConnection.TryToConnect();
			}

			return credentialAreCorrect;
		}

		/// <summary>
		/// Richiesta credenziali per accedere al server SQL e poi caricare i database presenti nell'apposita combo-box
		/// </summary>
		/// <param name="userCredential">param out: credenziali utente per accesso server</param>
		/// <param name="primarySQLServer">server sql</param>
		/// <param name="istanceSQLSever">istanza sql</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool AskCredential1(string login, string password, bool isWinAuth, string primarySQLServer, string instanceSQLSever)
		{
			//@@TODO da completare 
			// 05.01.11: questo mi sembra fosse un tentativo di provare ad inserire le credenziali valide dentro
			// la lista degli utenti gia' loginati in Console e poi abbandonato
			bool credentialAreCorrect = false;
			string serverName = primarySQLServer;

			if (!string.IsNullOrWhiteSpace(instanceSQLSever))
				serverName = Path.Combine(primarySQLServer, instanceSQLSever);

			TransactSQLAccess currentConnection = new TransactSQLAccess();
			currentConnection.NameSpace = "Module.MicroareaConsole.SysAdmin";
			currentConnection.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(SendCompleteHelp);
			currentConnection.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(this.AddUserAuthentication);
			currentConnection.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(this.GetUserAuthenticatedPwd);
			currentConnection.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(this.IsUserAuthenticated);

			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
			//eventualmente eseguo l'impersonificazione
			dataToConnectionServer =
				currentConnection.LoginImpersonification
				(
					login,
					password,
					SystemInformation.UserDomainName.ToUpper(CultureInfo.InvariantCulture),
					isWinAuth,
					primarySQLServer,
					instanceSQLSever,
					false
				);

			// inizializzo la finestra con l'utente sa, e poi consento cmq all'utente di cambiare le credenziali
			userCredential = new SQLCredential(serverName, DatabaseLayerConsts.LoginSa, string.Empty, SystemInformation.UserDomainName.ToUpper(CultureInfo.InvariantCulture), false, true);
			userCredential.OnOpenHelpFromPopUp += new SQLCredential.OpenHelpFromPopUp(SendHelp);
			userCredential.ShowDialog();

			if (userCredential.Success)
			{
				// se le credenziali sono corrette allora compongo la stringa di connessione e provo a connettermi
				currentConnection.CurrentStringConnection =
					(userCredential.userImpersonated.WindowsAuthentication)
					? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, DatabaseLayerConsts.MasterDatabase)
					: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase,
					userCredential.userImpersonated.Login, userCredential.userImpersonated.Password);

				credentialAreCorrect = currentConnection.TryToConnect();
			}

			return credentialAreCorrect;
		}
		# endregion

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
		# endregion
	}
}