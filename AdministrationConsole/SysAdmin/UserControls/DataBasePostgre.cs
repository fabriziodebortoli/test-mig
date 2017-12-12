using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using Npgsql;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.PostgreDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
	/// <summary>
	/// DataBaseSql.
	/// Tab di database per l'azienda con provider Npgsql
	/// </summary>
	//=========================================================================
	public partial class DataBasePostgre : System.Windows.Forms.UserControl
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
		private string selectedPostgreServerName = string.Empty;
		private string selectedPostgreIstanceName = string.Empty;
		private string selectedPostgreNewDatabaseName = string.Empty;
		private string selectedPostgreDatabaseName = string.Empty;
        private int postgrePort = DatabaseLayerConsts.postgreDefaultPort;
		private string selectedDbOwnerId = string.Empty;
		private string selectedDbOwnerName = string.Empty;
		private string selectedDbOwnerPwd = string.Empty;
		private bool selectedDbOwnerIsWinNT = false;
		private bool isNewPostgreCompany = false;
		private bool inserting = false;
     

		private SqlConnection currentConnection = null;
		private Diagnostic diagnostic = new Diagnostic("DatabasePostgre");
		private PostgreCredential userCredential = null;
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
        public string SelectedPostgreServerName { get { return selectedPostgreServerName; } set { selectedPostgreServerName = value; } }
        public string SelectedPostgreIstanceName { get { return selectedPostgreIstanceName; } set { selectedPostgreIstanceName = value; } }
        public string SelectedPostgreNewDatabaseName { get { return selectedPostgreNewDatabaseName; } set { selectedPostgreNewDatabaseName = value; } }
        public string SelectedPostgreDatabaseName { get { return selectedPostgreDatabaseName; } set { selectedPostgreDatabaseName = value; } }
		public string SelectedDbOwnerId { get { return selectedDbOwnerId; } set { selectedDbOwnerId = value; } }
		public string SelectedDbOwnerName { get { return selectedDbOwnerName; } set { selectedDbOwnerName = value; } }
		public string SelectedDbOwnerPwd { get { return selectedDbOwnerPwd; } set { selectedDbOwnerPwd = value; } }
		public bool SelectedDbOwnerIsWinNT { get { return selectedDbOwnerIsWinNT; } set { selectedDbOwnerIsWinNT = value; } }
        public bool IsNewPostgreCompany { get { return isNewPostgreCompany; } set { isNewPostgreCompany = value; } }
		public bool Inserting { get { return inserting; } set { inserting = value; } }
        public int PostgrePort { get { return postgrePort; } set { postgrePort = value; } }

	
		#endregion

		#region Costruttore
		/// <summary>
		/// DataBaseSql constructor
		/// </summary>
		//---------------------------------------------------------------------
		public DataBasePostgre(bool useUnicodeSet)
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
            string serverName = string.IsNullOrWhiteSpace(SelectedPostgreServerName) ? ServerNameSystemDb : SelectedPostgreServerName;
            string instanceName = string.IsNullOrWhiteSpace(SelectedPostgreIstanceName) ? ServerIstanceSystemDb : SelectedPostgreIstanceName;

			// inizializzo la combo con il server dell'azienda di origine
			if (!string.IsNullOrWhiteSpace(serverName))
			{
				if (instanceName.Length > 0)
					PostgreServerTextBox.Text=Path.Combine(serverName, instanceName);
				else
					PostgreServerTextBox.Text=serverName;
			}

            SelectedPostgreServerName = serverName;
            SelectedPostgreIstanceName = instanceName;

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
			selectedPostgreDatabaseName = string.IsNullOrWhiteSpace(CompanyDbName) ? DatabasesComboBox.DataSourceName : CompanyDbName;
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
			string serverName = string.IsNullOrWhiteSpace(SelectedPostgreServerName) ? ServerNameSystemDb : SelectedPostgreServerName;
			string instanceName = string.IsNullOrWhiteSpace(SelectedPostgreIstanceName) ? ServerIstanceSystemDb : SelectedPostgreIstanceName;

			// inizializzo la combo con il server dell'azienda di origine
			if (!string.IsNullOrWhiteSpace(serverName))
			{
				if (instanceName.Length > 0)
					PostgreServerTextBox.Text=Path.Combine(serverName, instanceName);
				else
					PostgreServerTextBox.Text=serverName;
			}

			SelectedPostgreServerName = serverName;
			SelectedPostgreIstanceName = instanceName;

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

				/*if (cbUserOwner.Items.Count > 0)
				{
					string userToSelect = UserConnected;
					foreach (UserItem ui in cbUserOwner.Items)
					{
						if (string.Compare(ui.LoginId, SelectedDbOwnerId, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							cbUserOwner.SelectUser(ui.Login);
							break;
						}
					}

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
				}*/
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
			selectedPostgreDatabaseName = string.IsNullOrWhiteSpace(CompanyDbName) ? DatabasesComboBox.DataSourceName : CompanyDbName;
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
			rbNewDb.Checked = isNewPostgreCompany;
			txtNewDataBaseName.Enabled = isNewPostgreCompany;
			DatabasesComboBox.Enabled = !isNewPostgreCompany;
			txtNewDataBaseName.Text = (isNewPostgreCompany) ? NewCompanyDbName : string.Empty;
			SettingToolTips(isNewPostgreCompany);
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
				toolTipSql.SetToolTip(PostgreServerTextBox, Strings.InsertingServerDbCompanyToolTip);
			else
			{
				toolTipSql.SetToolTip(PostgreServerTextBox, Strings.ModifyingServerDbCompanyToolTip);
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
			if (string.IsNullOrEmpty(PostgreServerTextBox.Text))
			{
				diagnostic.Set(DiagnosticType.Error, Strings.NoServerSelected);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			}
			else
			{
				DatabasesComboBox.ServerName = PostgreServerTextBox.Text;

				bool sameServer =
					(string.Compare(DatabasesComboBox.ServerName, this.currentConnection.DataSource, StringComparison.InvariantCultureIgnoreCase) == 0);

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
                        AskCredential(out userCredential, PostgreServerTextBox.Text, PostgreServerTextBox.Text);

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
						selectedPostgreDatabaseName = DatabasesComboBox.DataSourceName;
						break;
					}
					else
					{
						if (string.Compare(DatabasesComboBox.IstanceItem(i), CompanyDbName, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							DatabasesComboBox.SelectIndex(i);
							selectedPostgreDatabaseName = DatabasesComboBox.DataSourceName;
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
			IsNewPostgreCompany = ((RadioButton)sender).Checked;

			if (((RadioButton)sender).Checked)
			{
				PostgreServerTextBox.Enabled = true;
				DatabasesComboBox.Enabled = false;

				if (string.IsNullOrWhiteSpace(txtNewDataBaseName.Text))
					txtNewDataBaseName.Text = string.IsNullOrWhiteSpace(NewCompanyDbName) ? Company : NewCompanyDbName;

				SelectedPostgreNewDatabaseName = txtNewDataBaseName.Text;

				txtNewDataBaseName.Enabled = true;
				txtNewDataBaseName.Focus();

				if (OnSetValueForUnicodeCheckBox != null)
					OnSetValueForUnicodeCheckBox(!((RadioButton)sender).Checked, mustUseUnicodeSet);

				if (OnSetNewDatabaseName != null)
					OnSetNewDatabaseName();
			}
			else
			{
				DatabasesComboBox.Enabled = true;
				txtNewDataBaseName.Enabled = false;
				txtNewDataBaseName.Text = string.Empty;

				DatabasesComboBox.IsWindowsAuthentication = selectedDbOwnerIsWinNT;
				DatabasesComboBox.UserName = selectedDbOwnerName;
				DatabasesComboBox.UserPassword = selectedDbOwnerPwd;
				DatabasesComboBox.ServerName = PostgreServerTextBox.Text;

				if (!string.IsNullOrWhiteSpace(DatabasesComboBox.DataSourceName))
					selectedPostgreDatabaseName = DatabasesComboBox.DataSourceName;
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
							selectedPostgreDatabaseName = DatabasesComboBox.DataSourceName;
							break;
						}
						else
						{
							if (string.Compare(DatabasesComboBox.IstanceItem(i), CompanyDbName, StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								DatabasesComboBox.SelectIndex(i);
								selectedPostgreDatabaseName = DatabasesComboBox.DataSourceName;
								break;
							}
						}
					}
				}

				//TestUnicode();
			}
		}

        ///// <summary>
        ///// NGSqlServersCombo_OnSelectedServerSQL
        ///// Seleziono un server SQL dalla combobox
        ///// </summary>
        ////---------------------------------------------------------------------
        //private void NGSqlServersCombo_OnSetSelectedServerSQL(string serverName)
        //{
        //    if (serverName.Split(Path.DirectorySeparatorChar).Length > 1)
        //    {
        //        SelectedPostgreServerName = serverName.Split(Path.DirectorySeparatorChar)[0];
        //        SelectedPostgreIstanceName = serverName.Split(Path.DirectorySeparatorChar)[1];
        //    }
        //    else
        //    {
        //        SelectedPostgreServerName = serverName.Split(Path.DirectorySeparatorChar)[0];
        //        SelectedPostgreIstanceName = string.Empty;
        //    }

        //    // sul changed del server sql faccio la clear della combo dei database
        //    DatabasesComboBox.ClearListOfDb();
        //    DatabasesComboBox.DataSourceName = string.Empty;
        //}

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
            SelectedPostgreNewDatabaseName = ((TextBox)sender).Text;
		}

        ///// <summary>
        ///// PostgrePort_TextChanged
        ///// Imposto una porta (diverso dal default) per il DB Postgre da  connettersi
        ///// </summary>
        ////---------------------------------------------------------------------
        private void PostgrePort_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PostgrePortText.Text) || string.IsNullOrWhiteSpace(PostgrePortText.Text))
            {
                diagnostic.Set(DiagnosticType.Warning, Strings.PostgreEmptyPort);
                DiagnosticViewer.ShowDiagnostic(diagnostic);
                diagnostic.Clear();
                return;
            }

            try
            {
                postgrePort = Int32.Parse(PostgrePortText.Text);
            }
            catch (Exception)
            {
                diagnostic.Set(DiagnosticType.Warning, Strings.PostgrePortBadValue);
                DiagnosticViewer.ShowDiagnostic(diagnostic);
                diagnostic.Clear();
                return;
            }

        }

		//---------------------------------------------------------------------
		private void DatabasesComboBox_OnSelectDatabases(string dbName)
		{
            selectedPostgreDatabaseName = dbName;
		}

		/// <summary>
		/// Sulla Leave della combo-box dei database vado a controllare se il database è stato creato con 
		/// il set di caratteri Unicode, in modo da settare correttamente la check-box nella tab dei Parametri
		/// </summary>
		//---------------------------------------------------------------------
        //private void DatabasesComboBox_Leave(object sender, System.EventArgs e)
        //{
        //    TestUnicode();
        //}

		/// <summary>
		/// TestUnicode
		/// Accedo al server selezionato, costruisco la stringa di connessione e vado a vedere sulla tabella
		/// TB_DBMark se la colonna Application è di tipo nvarchar.
		/// </summary>
		//---------------------------------------------------------------------
        //private void TestUnicode()
        //{
        //    bool dbMarkExist = false;
        //    bool useUnicode = CheckUnicodeOnDB(out dbMarkExist);

        //    // Questo evento e' richiamato SOLO per il database aziendale, in modo da sincronizzare 
        //    // in automatico il valore della checkbox 'Use unicode' con l'effettivo valore del database
        //    if (OnSetValueForUnicodeCheckBox != null)
        //        OnSetValueForUnicodeCheckBox(rbSelectExistedDb.Checked, useUnicode);
        //}

		/// <summary>
		/// CheckUnicodeOnDB
		/// Accedo al server selezionato, costruisco la stringa di connessione e vado a vedere sulla tabella
		/// TB_DBMark se la colonna Application è di tipo nvarchar.
		/// Questo metodo viene richiamato anche in fase di salvataggio dell'azienda, in modo da confrontare 
		/// se lo stato Unicode sul db aziendale e sul db documentale coincidono.
		/// Se cosi' non fosse non procedo con la creazione e visualizzo opportuno messaggio.
		/// </summary>
		//---------------------------------------------------------------------
        //public bool CheckUnicodeOnDB(out bool dbMarkExist)
        //{
        //    dbMarkExist = false;

        //    if (string.IsNullOrEmpty(selectedPostgreDatabaseName))
        //        return false;

        //    string serverName = NGSqlServersCombo.SelectedSQLServer;
        //    string connectionString =
        //        (userCredential == null)
        //        ? ((DatabasesComboBox.IsWindowsAuthentication)
        //            ? string.Format(NameSolverDatabaseStrings.PostgreWinNtConnection, serverName, PostgrePort, selectedPostgreDatabaseName, DatabaseLayerConsts.postgreDefaultSchema)
        //            : string.Format(NameSolverDatabaseStrings.PostgreConnection, serverName, PostgrePort, selectedPostgreDatabaseName,
        //            DatabasesComboBox.UserName, DatabasesComboBox.UserPassword, DatabaseLayerConsts.postgreDefaultSchema))
        //        : ((userCredential.userImpersonated.WindowsAuthentication)
        //            ? string.Format(NameSolverDatabaseStrings.PostgreWinNtConnection, serverName, PostgrePort, selectedPostgreDatabaseName, DatabaseLayerConsts.postgreDefaultSchema)
        //            : string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, PostgrePort, selectedPostgreDatabaseName,
        //            userCredential.userImpersonated.Login, userCredential.userImpersonated.Password, DatabaseLayerConsts.postgreDefaultSchema));

        //    TBConnection myConnection = null;
        //    TBDatabaseSchema mySchema = null;
        //    bool useUnicode = false;

        //    try
        //    {
        //        // istanzio una TBConnection e la apro
        //        myConnection = new TBConnection(connectionString, DBMSType.POSTGRE);
        //        myConnection.Open();

        //        // istanzio TBDatabaseSchema sulla connessione
        //        mySchema = new TBDatabaseSchema(myConnection);

        //        // se la tabella di riferimento TB_DBMark NON esiste, restituisco il valore unicode impostato dall'utente
        //        // altrimenti procedo con il controllo sulla tabella....
        //        if (mySchema.ExistTable(DatabaseLayerConsts.TB_DBMark))
        //        {
        //            // analizzo lo schema della tabella e verifico il tipo della colonna Application
        //            DataTable cols = mySchema.GetTableSchema(DatabaseLayerConsts.TB_DBMark, false);

        //            foreach (DataRow col in cols.Rows)
        //            {
        //                if (string.Compare(col["ColumnName"].ToString(), "Application", StringComparison.InvariantCultureIgnoreCase) == 0)
        //                {
        //                    TBType providerType = (TBType)((SqlDbType)col["ProviderType"]);
        //                    useUnicode =
        //                        //string.Compare(TBDatabaseType.GetDBDataType(providerType, DBMSType.SQLSERVER), "NVarChar", StringComparison.InvariantCultureIgnoreCase) == 0;
        //                        string.Compare(col["DataTypeName"].ToString(), "NVarChar", StringComparison.InvariantCultureIgnoreCase) == 0;							
        //                    break;
        //                }
        //            }
        //            dbMarkExist = true;
        //        }
        //        else
        //        {
        //            // il database e' vuoto, quindi imposto il valore unicode come l'azienda
        //            dbMarkExist = false;

        //            //useUnicode = mustUseUnicodeSet;
        //        }
        //    }
        //    catch (TBException)
        //    {
        //    }
        //    finally
        //    {
        //        if (myConnection != null && myConnection.State != ConnectionState.Closed)
        //        {
        //            myConnection.Close();
        //            myConnection.Dispose();
        //        }
        //    }

        //    return useUnicode;
        //}

		# region AskCredential (chiedo le credenziali per connettermi al server e caricare tutti i database presenti)
		/// <summary>
		/// Richiesta credenziali per accedere al server SQL e poi caricare i database presenti nell'apposita combo-box
		/// </summary>
		/// <param name="userCredential">param out: credenziali utente per accesso server</param>
		/// <param name="primarySQLServer">server sql</param>
		/// <param name="istanceSQLSever">istanza sql</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool AskCredential(out PostgreCredential userCredential, string primarySQLServer, string istanceSQLSever)
		{
			bool credentialAreCorrect = false;
			string serverName = primarySQLServer;

			if (!string.IsNullOrWhiteSpace(istanceSQLSever))
				serverName = Path.Combine(primarySQLServer, istanceSQLSever);

            PostgreAccess currentConnection = new PostgreAccess();
			currentConnection.NameSpace = "Module.MicroareaConsole.SysAdmin";
            currentConnection.OnCallHelpFromPopUp += new PostgreAccess.CallHelpFromPopUp(SendCompleteHelp);

			// inizializzo la finestra con l'utente sa, e poi consento cmq all'utente di cambiare le credenziali
            userCredential = new PostgreCredential(serverName, DatabaseLayerConsts.LoginSa, string.Empty, SystemInformation.UserDomainName.ToUpper(CultureInfo.InvariantCulture), false, true, postgrePort);
            userCredential.OnOpenHelpFromPopUp += new PostgreCredential.OpenHelpFromPopUp(SendHelp);
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
		private bool AskCredential1(string login, string password, bool isWinAuth, string primarySQLServer, string instanceSQLSever, int port)
		{
			//@@TODO da completare 
			// 05.01.11: questo mi sembra fosse un tentativo di provare ad inserire le credenziali valide dentro
			// la lista degli utenti gia' loginati in Console e poi abbandonato
			bool credentialAreCorrect = false;
			string serverName = primarySQLServer;

			if (!string.IsNullOrWhiteSpace(instanceSQLSever))
				serverName = Path.Combine(primarySQLServer, instanceSQLSever);

            PostgreAccess currentConnection = new PostgreAccess();
			currentConnection.NameSpace = "Module.MicroareaConsole.SysAdmin";
            currentConnection.OnCallHelpFromPopUp += new PostgreAccess.CallHelpFromPopUp(SendCompleteHelp);
            currentConnection.OnAddUserAuthenticatedFromConsole += new PostgreAccess.AddUserAuthenticatedFromConsole(this.AddUserAuthentication);
            currentConnection.OnGetUserAuthenticatedPwdFromConsole += new PostgreAccess.GetUserAuthenticatedPwdFromConsole(this.GetUserAuthenticatedPwd);
            currentConnection.OnIsUserAuthenticatedFromConsole += new PostgreAccess.IsUserAuthenticatedFromConsole(this.IsUserAuthenticated);

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
					false,
                    port
				);

			// inizializzo la finestra con l'utente sa, e poi consento cmq all'utente di cambiare le credenziali
			userCredential = new PostgreCredential(serverName, DatabaseLayerConsts.LoginSa, string.Empty, SystemInformation.UserDomainName.ToUpper(CultureInfo.InvariantCulture), false, true, port);
			userCredential.OnOpenHelpFromPopUp += new PostgreCredential.OpenHelpFromPopUp(SendHelp);
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