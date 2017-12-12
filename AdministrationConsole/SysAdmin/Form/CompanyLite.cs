using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microarea.Console.Core.DBLibrary.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.Console.Plugin.SysAdmin.UserControls;
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
	/// Form per la gestione dell'anagrafica azienda e del database ad esso associato.
	/// </summary>
	//=========================================================================
	public partial class CompanyLite : PlugInsForm
	{
		#region Private members
		private string connectionString = string.Empty;
		private SqlConnection currentConnection = null;
		private string companyDbName = string.Empty;
		private string userDbOwnerId = string.Empty;
		private string userConnected = string.Empty;
		private string userPwdConnected = string.Empty;
		private string userSystemConnected = string.Empty;
		private string dataSourceSysAdmin = string.Empty;
		private string serverNameSysAdmin = string.Empty;
		private string istanceNameSysAdmin = string.Empty;
		private bool integratedConnection = false;
		private string companyId = string.Empty;
		private string companyName = string.Empty;
		private string oldOwnerId = string.Empty;
		private string oldOwnerDBName = string.Empty;
 		private bool dboIsEasyBuilderDeveloper = false; // serve per tenere traccia del flag impostato sul dbo

		private TreeView treeConsole = null;
		private DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
		private PathFinder aPathFinder;
		private Diagnostic diagnostic = new Diagnostic("SysAdminPlugIn.CompanyLite");
		private bool inserting = false;
		private LicenceInfo licenceInfo = null;
		private ProgressForm progressForm = null;

		private string companyDBCulture = string.Empty;
		private int companyLcid = 0;
		private bool supportColsCollation = false;

		private string dbCollation = string.Empty;
		private string columnCollation = string.Empty;

		private bool hideDbChanges = false; // se StandardEdition e due aziende censite oppure licenza undefined non faccio modificare i database

		// plugins attivati 
		private bool isDMSActivated = false; 
		private bool isSecurityActivated = false;
		private bool isAuditingActivated = false;
		private bool isRowSecurityActivated = false;
        private bool isDataSynchroActivated = false; // se il modulo DataSynchronizer e' attivato
        private bool sql2012MsgDisplayed = false;

		// informazioni della company corrente estratte dal db di sistema
		private CompanyItem companyItem = null;
		#endregion

		#region Delegati ed Eventi
		//---------------------------------------------------------------------
		public delegate void ModifyTree(object sender, string nodeType);
		public event ModifyTree OnModifyTree;
		//---------------------------------------------------------------------
		public delegate void ModifyTreeOfCompanies(object sender, string nodeType, string companyId);
		public event ModifyTreeOfCompanies OnModifyTreeOfCompanies;
		//---------------------------------------------------------------------
		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;
		//---------------------------------------------------------------------
		public delegate void AfterChangedOSLSecurity(object sender, string companyId, bool security);
		public event AfterChangedOSLSecurity OnAfterChangedOSLSecurity;
		//---------------------------------------------------------------------
		public delegate void AfterChangedAuditing(object sender, string companyId, bool activity);
		public event AfterChangedAuditing OnAfterChangedAuditing;
		//---------------------------------------------------------------------
		public delegate void AfterChangedCompanyDisable(object sender, string companyId, bool isDisabled);
		public event AfterChangedCompanyDisable OnAfterChangedCompanyDisable;
		//---------------------------------------------------------------------
		public delegate void AfterModifyCompany(object sender, string companyId);
		public event AfterModifyCompany OnAfterModifyCompany;
		//---------------------------------------------------------------------
		public delegate void AfterDeleteCompany(object sender, string companyId);
		public event AfterDeleteCompany OnAfterDeleteCompany;
		//---------------------------------------------------------------------
		public delegate void EnableProgressBar(object sender);
		public event EnableProgressBar OnEnableProgressBar;
		//---------------------------------------------------------------------
		public delegate void DisableProgressBar(object sender);
		public event DisableProgressBar OnDisableProgressBar;
		//---------------------------------------------------------------------
		public delegate void SetProgressBarStep(object sender, int step);
		public event SetProgressBarStep OnSetProgressBarStep;
		//---------------------------------------------------------------------
		public delegate void SetProgressBarValue(object sender, int currentValue);
		public event SetProgressBarValue OnSetProgressBarValue;
		//---------------------------------------------------------------------
		public delegate void SetProgressBarText(object sender, string message);
		public event SetProgressBarText OnSetProgressBarText;
		//---------------------------------------------------------------------
		public delegate void SetCyclicStepProgressBar();
		public event SetCyclicStepProgressBar OnSetCyclicStepProgressBar;
		//---------------------------------------------------------------------
		public delegate void CallHelp(object sender, string nameSpace, string searchParameter);
		public event CallHelp OnCallHelp;
		//---------------------------------------------------------------------
		public delegate void CreateDBStructure(object sender, string companyId);
		public event CreateDBStructure OnCreateDBStructure;
		//---------------------------------------------------------------------
		public delegate bool CheckDBRequirementsUsed(string connectionString, DBMSType dbType, bool candidateUnicode, out bool isUnicode, out bool italianTableName);
		public event CheckDBRequirementsUsed OnCheckDBRequirementsUsed;
		//---------------------------------------------------------------------
		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;
		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;
		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;
        //---------------------------------------------------------------------
        //public event EventHandler OnDisableSaveButton;
        public event EventHandler OnEnableSaveButton;
		//---------------------------------------------------------------------
		public delegate bool BeforeDeleteCompany(object sender, int companyId);
		public event BeforeDeleteCompany OnBeforeDeleteCompany;
		//---------------------------------------------------------------------
		public delegate void AfterSaveNewCompany(string companyId, bool isDisabled);
		public event AfterSaveNewCompany OnAfterSaveNewCompany;
		//---------------------------------------------------------------------
		public delegate bool IsActivated(string application, string functionality);
		public event IsActivated OnIsActivated;
		#endregion

		#region Properties
		public string DataSourceSysAdmin { get { return dataSourceSysAdmin; } set { dataSourceSysAdmin = value; } }
		public string ServerNameSystemDb { get { return serverNameSysAdmin; } set { serverNameSysAdmin = value; } }
		public string ServerIstanceSystemDb { get { return istanceNameSysAdmin; } set { istanceNameSysAdmin = value; } }
		public bool IntegratedConnection { get { return integratedConnection; } set { integratedConnection = value; } }
		public string UserConnected { get { return userConnected; } set { userConnected = value; } }
		public string UserSystemConnected { get { return userSystemConnected; } set { userSystemConnected = value; } }
		public string UserPwdConnected { get { return userPwdConnected; } set { userPwdConnected = value; } }
		public TreeView TreeConsole { get { return treeConsole; } set { treeConsole = value; } }
		public string NameOfCompanyDb { get { return companyDbName; } }
		public string NameOfCompany { get { return companyName; } }
		public Diagnostic Diagnostic { get { return diagnostic; } }
		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore per l'inserimento di una nuova azienda
		/// </summary>
		//---------------------------------------------------------------------
		public CompanyLite(string ownerSystemDb, string connectionString, SqlConnection currentConnection, PathFinder pf, LicenceInfo li)
		{
			InitializeComponent();
			diagnostic.Clear();

			UserSystemConnected = ownerSystemDb;
			this.connectionString = connectionString;
			this.currentConnection = currentConnection;
			this.aPathFinder = pf;
			this.licenceInfo = li;
			inserting = true;

			cbUseUnicode.Checked = li.UseUnicodeSet();

			LabelTitle.Text = Strings.LabelCompanyInserting;
			State = StateEnums.View;

			if (
				(string.Compare(li.Edition, NameSolverStrings.StandardEdition, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				CompaniesNumber() >= 2)
				|| //nessuna info sull'edition oppure indefinita
				(string.IsNullOrWhiteSpace(li.Edition) || string.Compare(li.Edition, "Undefined", StringComparison.InvariantCultureIgnoreCase) == 0)
				)
				hideDbChanges = true;
		}

		/// <summary>
		/// Costruttore per la modifica di un'azienda esistente
		/// </summary>
		//---------------------------------------------------------------------
		public CompanyLite(string ownerSystemDb, string connectionString, SqlConnection currentConnection, string id, PathFinder pf, LicenceInfo li)
		{
			InitializeComponent();
			diagnostic.Clear();

			UserSystemConnected = ownerSystemDb;

			BtnSave.Enabled = true;
			BtnDelete.Enabled = true;
			this.connectionString = connectionString;
			this.currentConnection = currentConnection;
			this.companyId = id;
			this.licenceInfo = li;
			cbUseTransaction.Checked = true;
			cbUseUnicode.Enabled = true;
			this.aPathFinder = pf;
			PopolateCombosLanguage();

			if (
				(string.Compare(li.Edition, NameSolverStrings.StandardEdition, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				CompaniesNumber() >= 2)
				|| //nessuna info sull'edition oppure indefinita
				(string.IsNullOrWhiteSpace(li.Edition) || string.Compare(li.Edition, "Undefined", StringComparison.InvariantCultureIgnoreCase) == 0)
				)
				hideDbChanges = true;

			// carico le informazioni dell'azienda e valorizzo le variabili che mi servono
			ArrayList fieldsAzienda = new ArrayList();
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = connectionString;
			companyDb.CurrentSqlConnection = currentConnection;

			if (!companyDb.GetAllCompanyFieldsById(out fieldsAzienda, id))
			{
				if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
					diagnostic.Set(companyDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);

				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				fieldsAzienda.Clear();
			}

			if (fieldsAzienda.Count > 0)
			{
				companyItem = (CompanyItem)fieldsAzienda[0];
				tbCompanyId.Text = companyItem.CompanyId;
				tbCompany.Text = companyItem.Company.Trim();
				oldOwnerId = companyItem.DbOwner;

				switch (companyItem.Provider)
				{
                    case NameSolverDatabaseStrings.SQLODBCProvider:
					case NameSolverDatabaseStrings.SQLOLEDBProvider:
						{
							ServerNameSystemDb = companyItem.DbServer.Split(Path.DirectorySeparatorChar)[0].ToUpper(CultureInfo.InvariantCulture);
							ServerIstanceSystemDb = companyItem.DbServer.Split(Path.DirectorySeparatorChar).Length > 1
													? companyItem.DbServer.Split(Path.DirectorySeparatorChar)[1].ToUpper(CultureInfo.InvariantCulture)
													: string.Empty;
							companyDbName = companyItem.DbName;
							companyName = companyItem.Company;
                      
							break;
						}
				}

				tbDescrizione.Text = companyItem.Description;
				UseAuditing.Checked = companyItem.UseAuditing;
				UseSecurity.Checked = companyItem.UseSecurity;
				UseDBSlave.Checked = companyItem.UseDBSlave;
				UseRowSecurityCBox.Checked = companyItem.UseRowSecurity;
                UseDataSynchro.Checked = companyItem.UseDataSynchro;

                cbUseTransaction.Checked = companyItem.UseTransaction;
				CbCompanyDisabled.Checked = companyItem.Disabled;
				rbKey.Checked = companyItem.UseKeyedUpdate;
				rbPosition.Checked = !companyItem.UseKeyedUpdate;
				cbUseUnicode.Checked = companyItem.UseUnicode;
				// cbUseUnicode deve essere read only se l'azienda è stata già creata
				cbUseUnicode.Enabled = false;
				// seleziono la lingua di interfaccia
				cultureUICombo.SetUILanguage(companyItem.PreferredLanguage);
				// seleziono la lingua di applicazione
				cultureApplicationCombo.ApplicationLanguage = companyItem.ApplicationLanguage;

				// il provider non può essere mai modificato
				PopolateComboProvider(companyItem.ProviderDesc);

				LabelTitle.Text = string.Format(Strings.LabelCompanyModifying, tbCompany.Text);

				BuildTabDatabaseSettings();
				if (companyItem.UseDBSlave)
					BuildTabDBSlave();

				// carico i valori letti da database dentro i controls dello user control
				SetValuesInDBUserControls();

				State = StateEnums.View;
			}
		}

		///<summary>
		/// Legge dalle tabelle di sistema i valori legati all'azienda ed ai suoi utenti e 
		/// carica le informazioni corrette nei vari usercontrol
		///</summary>
		//---------------------------------------------------------------------
		private void SetValuesInDBUserControls()
		{
			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.CurrentSqlConnection = this.currentConnection;
			companyUserDb.ConnectionString = this.connectionString;
			ArrayList dbOwnerData = new ArrayList();
			companyUserDb.GetUserCompany(out dbOwnerData, companyItem.DbOwner, companyItem.CompanyId);

			if (dbOwnerData.Count == 0)
				return;

			switch (companyItem.Provider)
			{
				case NameSolverDatabaseStrings.SQLODBCProvider:
				case NameSolverDatabaseStrings.SQLOLEDBProvider:
					{
						PopolateDatabaseCultureSettings(companyItem);

						//carico i dati sql nello userControl
						DataBaseSqlLite dbSql = (DataBaseSqlLite)DBCompanySettings.Controls[0];
						dbSql.SelectedSQLServerName = companyItem.DbServer.Split(Path.DirectorySeparatorChar)[0];
						dbSql.SelectedSQLIstanceName = companyItem.DbServer.Split(Path.DirectorySeparatorChar).Length > 1
														? companyItem.DbServer.Split(Path.DirectorySeparatorChar)[1]
														: string.Empty;
						dbSql.IsNewSQLCompany = false;
						dbSql.SelectedSQLNewDatabaseName = string.Empty;
						dbSql.SelectedSQLDatabaseName = companyItem.DbName;
						dbSql.CompanyDbName = companyItem.DbName;
						dbSql.Company = companyItem.Company;

						if (dbOwnerData.Count > 0)
						{
							CompanyUser dbOwnerSql = (CompanyUser)dbOwnerData[0];
							dbSql.SelectedDbOwnerId = companyItem.DbOwner;
							dbSql.SelectedDbOwnerName = dbOwnerSql.DBDefaultUser;
							dbSql.SelectedDbOwnerPwd = dbOwnerSql.DBDefaultPassword;
							dbSql.SelectedDbOwnerIsWinNT = dbOwnerSql.DBWindowsAuthentication;
							dbSql.UserConnected = dbOwnerSql.DBDefaultUser;
							dbSql.UserPwdConnected = dbOwnerSql.DBDefaultPassword;
							dbSql.LoadData(false);
							oldOwnerDBName = dbOwnerSql.DBDefaultUser;
							dboIsEasyBuilderDeveloper = dbOwnerSql.EasyBuilderDeveloper;
						}

						// StandardEdition, nessuna info sull'edition oppure indefinita
						if (hideDbChanges)
							dbSql.HideDbChanges();

						// Solo per chi ha l'edizione Standard o Professional Pro-Lite e solo per SQL Server 
						// mostro il grafico a torta con le dimensioni del database
						ShowPieChart(companyItem);

						// la combobox del dbo per il db aziendale e' abilitata solo se non gestisco gli slave
						//dbSql.EnableUsersComboBox(!companyItem.UseDBSlave);
						dbSql.EnableUsersComboBox(false); // per ora disabilito sempre la combobox
						break;
					}
			}

			// se l'azienda gestisce il database documentale, devo caricare anche le sue informazioni
			if (companyItem.UseDBSlave)
			{
				CompanyDBSlave dbSlave = new CompanyDBSlave();
				dbSlave.CurrentSqlConnection = this.currentConnection;
				dbSlave.ConnectionString = this.connectionString;
				CompanyDBSlaveItem slaveItem;
				dbSlave.SelectSlaveForCompanyId(this.companyId, out slaveItem);

                DataBaseSqlLite dmsSql = (DataBaseSqlLite)SlavePanel.Controls[0];
				if (dmsSql == null || slaveItem == null)
					return;

				dmsSql.SelectedSQLServerName = slaveItem.ServerName.Split(Path.DirectorySeparatorChar)[0];
				dmsSql.SelectedSQLIstanceName = slaveItem.ServerName.Split(Path.DirectorySeparatorChar).Length > 1
												? slaveItem.ServerName.Split(Path.DirectorySeparatorChar)[1]
												: string.Empty;
				dmsSql.IsNewSQLCompany = false;
				dmsSql.SelectedSQLNewDatabaseName = string.Empty;
				dmsSql.SelectedSQLDatabaseName = slaveItem.DatabaseName;
				dmsSql.CompanyDbName = slaveItem.DatabaseName;
				dmsSql.Company = companyItem.Company;

				SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
				slaveLoginDb.CurrentSqlConnection = this.currentConnection;
				slaveLoginDb.ConnectionString = this.connectionString;
				SlaveLoginItem loginItem;
				slaveLoginDb.SelectAllForSlaveAndLogin(slaveItem.SlaveId, slaveItem.SlaveDBOwner, out loginItem);

				if (loginItem != null)
				{
					CompanyUser dbOwnerSql = (CompanyUser)dbOwnerData[0];
					dmsSql.SelectedDbOwnerId = slaveItem.SlaveDBOwner;
					dmsSql.SelectedDbOwnerName = loginItem.SlaveDBUser;
					dmsSql.SelectedDbOwnerPwd = loginItem.SlaveDBPassword;
					dmsSql.SelectedDbOwnerIsWinNT = loginItem.SlaveDBWinAuth;
					dmsSql.UserConnected = loginItem.SlaveDBUser;
					dmsSql.UserPwdConnected = loginItem.SlaveDBPassword;
					dmsSql.LoadData(true);
				}
			}
		}
		#endregion

		/// <summary>
		/// Numero delle aziende configurate nel db di sistema (MSD_Companies)
		/// </summary>
		//---------------------------------------------------------------------
		private int CompaniesNumber()
		{
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;
			return companyDb.CompaniesNumber();
		}

		# region Controlli di esistenza di database SQL
		/// <summary>
		/// CheckIfSqlDbExist
		/// Verifica se il db di SQL Server che si vuole creare esiste già
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckIfSqlDbExist(TransactSQLAccess currentConnection, UserInfo contextUser)
		{
			//controllo che nel systemdb corrente non ci sia alcuna company con quel nome
			CompanyDb listOfCompanies = new CompanyDb();
			listOfCompanies.ConnectionString = this.connectionString;
			listOfCompanies.CurrentSqlConnection = this.currentConnection;

			ArrayList companiesSameName = new ArrayList();
			if (listOfCompanies.SelectCompanyByDesc(out companiesSameName, tbCompany.Text))
			{
				if (companiesSameName.Count > 0)
				{
					bool cantContinue = true;
					for (int i = 0; i < companiesSameName.Count; i++)
					{
						CompanyItem currentItem = (CompanyItem)companiesSameName[i];
						if (currentItem.CompanyId == this.companyId)
							continue;
						else
							cantContinue = false;
					}
					if (!cantContinue)
					{
						diagnostic.Set(DiagnosticType.Error, Strings.CompanyWithSameName);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (this.OnSendDiagnostic != null)
						{
							this.OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}
						return false;
					}
				}
			}

			if (currentConnection.ExistDataBase(contextUser.DatabaseName))
			{
				diagnosticViewer.Message = String.Format(Strings.DataBaseAlreadyExist, contextUser.DatabaseName);
				diagnosticViewer.Title = Strings.Error;
				diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
				diagnosticViewer.ShowIcon = MessageBoxIcon.Error;
				diagnosticViewer.DefaultButton = MessageBoxDefaultButton.Button2;
				DialogResult proceeding = diagnosticViewer.Show();

				if (proceeding == DialogResult.No)
					return false;

				bool isWinAuthentication = (contextUser.Domain.Length != 0);

				// se ci sono stati errori durante la cancellazione interrompo il processo
				if (!currentConnection.DeleteDataBase
					(contextUser.ServerComplete, contextUser.DatabaseName, contextUser.LoginName, contextUser.LoginPassword, isWinAuthentication))
					return false;
			}

			return true;
		}
		# endregion

		# region UpdatingTables (aggiornamento tabelle del db di sistema)
		/// <summary>
		/// UpdatingTables
		/// Dopo la creazione del database vado ad aggiornare le tabelle del db di sistema,
		/// inserisco la company, inserisco gli utenti, etc.
		/// </summary>
		//---------------------------------------------------------------------
		private bool UpdatingTables(UserInfo contextUser, bool isCreation, string preferrendLanguage, string applicationLanguage, bool isValidCompany)
		{
			bool updating = false;
			bool isWinNt = (contextUser.Domain.Length == 0) ? false : true;
			string serverOrServiceName = string.Empty;

			UserListItem userListItem = new UserListItem();
			userListItem.LoginId = contextUser.LoginId;
			userListItem.Login = contextUser.LoginName;

			switch (contextUser.DbType)
			{
				case DBMSType.SQLSERVER:
					userListItem.DbUser = contextUser.LoginName;
					userListItem.DbPassword = contextUser.LoginPassword;
					serverOrServiceName = contextUser.ServerComplete;
					break;
			}

			userListItem.IsAdmin = true; 
			userListItem.DbWindowsAuthentication = isWinNt;

			// Controllo che in MSD_Logins esista l'utente dbo
			UserDb userdb = new UserDb();
			userdb.ConnectionString = this.connectionString;
			userdb.CurrentSqlConnection = this.currentConnection;

			// Leggo le info da MSD_Logins (eccetto la pwd) //ci sono sempre
			ArrayList user = new ArrayList();
			if (!userdb.LoadFromLogin(out user, contextUser.LoginName))
			{
				if (userdb.Diagnostic.Error || userdb.Diagnostic.Information || userdb.Diagnostic.Warning)
					diagnostic.Set(userdb.Diagnostic);
				diagnostic.Set(DiagnosticType.Error, (isCreation) ? Strings.AbortingDBCreation : Strings.AbortingDBModify);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				updating = false;
				return updating;
			}

			contextUser.LoginId = ((UserItem)user[0]).LoginId;
			userListItem.LoginId = contextUser.LoginId;
			userListItem.Login = ((UserItem)user[0]).Login;

			//Aggiorno MSD_Companies
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;

			ArrayList searchCompany = new ArrayList();
			if (tbCompanyId.Text.Length > 0)
			{
				if (!companyDb.GetAllCompanyFieldsById(out searchCompany, tbCompanyId.Text))
				{
					if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Information || companyDb.Diagnostic.Warning)
						diagnostic.Set(companyDb.Diagnostic);
					diagnostic.Set(DiagnosticType.Error, (isCreation) ? Strings.AbortingDBCreation : Strings.AbortingDBModify);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					updating = false;
					return updating;
				}
			}

			//non è presente lo devo inserire
			if (searchCompany.Count == 0)
			{
				// Sto aggiungendo la company (contestuale alla creazione del db oppure agganciando un db esistente)
				bool addCompany = companyDb.Add
					(
					tbCompany.Text.Trim(),
					tbDescrizione.Text,
					((ProviderItem)cbProvider.SelectedItem).ProviderId,
					serverOrServiceName,
					contextUser.DatabaseName,
					string.Empty,
					string.Empty,
					contextUser.LoginId,
					UseSecurity.Checked,
					UseAuditing.Checked,
					cbUseTransaction.Checked,
					rbKey.Checked,
					isWinNt,
					preferrendLanguage,
					applicationLanguage,
					CbCompanyDisabled.Checked,
					cbUseUnicode.Checked,
					isValidCompany,
					companyLcid,
					supportColsCollation,
					0,
					false, //UseDBSlave.Checked // non lo aggiorno qui ma lo faccio a mano se tutto va a buon fine
					UseRowSecurityCBox.Checked,
                    UseDataSynchro.Checked
                    );

				if (!addCompany)
				{
					if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Information || companyDb.Diagnostic.Warning)
						diagnostic.Set(companyDb.Diagnostic);
					diagnostic.Set(DiagnosticType.Error, Strings.AbortingDBCreation);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					updating = false;
					return updating;
				}

				string newCompanyId = companyDb.LastCompanyId().ToString();
				if (newCompanyId == "0")
				{
					if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Information || companyDb.Diagnostic.Warning)
						diagnostic.Set(companyDb.Diagnostic);
					diagnostic.Set(DiagnosticType.Error, Strings.AbortingDBCreation);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					updating = false;
					return updating;
				}
				tbCompanyId.Text = newCompanyId;

				// solo se l'azienda è sottoposta a sicurezza "sparo" un evento al plugin SecurityAdmin (visto con Lara)
				if (UseSecurity.Checked)
				{
					if (OnAfterSaveNewCompany != null)
						OnAfterSaveNewCompany(newCompanyId, CbCompanyDisabled.Checked);
				}
			}
			else
			{
				// ho creato il db oppure associato un db esistente, ma l'anagrafica della company esisteva già
				CompanyItem searchCompanyItem = (CompanyItem)searchCompany[0];
				bool modCompany = companyDb.Modify
					(
					searchCompanyItem.CompanyId,
					tbCompany.Text.Trim(),
					tbDescrizione.Text,
					((ProviderItem)cbProvider.SelectedItem).ProviderId,
					serverOrServiceName,
					contextUser.DatabaseName,
					string.Empty,
					string.Empty,
					contextUser.LoginId,
					UseSecurity.Checked,
					UseAuditing.Checked,
					cbUseTransaction.Checked,
					rbKey.Checked,
					isWinNt,
					preferrendLanguage,
					applicationLanguage,
					CbCompanyDisabled.Checked,
					cbUseUnicode.Checked,
					isValidCompany,
					companyLcid,
					supportColsCollation,
					0,
					false, //UseDBSlave.Checked // non lo aggiorno qui ma lo faccio a mano se tutto va a buon fine
					UseRowSecurityCBox.Checked,
                    UseDataSynchro.Checked
                    );

				if (!modCompany)
				{
					if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Information || companyDb.Diagnostic.Warning)
						diagnostic.Set(companyDb.Diagnostic);
					diagnostic.Set(DiagnosticType.Error, Strings.AbortingDBModify);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					updating = false;
					return updating;
				}
				tbCompanyId.Text = searchCompanyItem.CompanyId;
			}

			userListItem.CompanyId = tbCompanyId.Text;
			//Aggiorno MSD_CompanyUser
			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = this.connectionString;
			companyUserDb.CurrentSqlConnection = this.currentConnection;

			bool result = false;

			// prima di aggiungere/modificare l'utente dbo associato vado ad impostare il flag EBDeveloper esistente
			userListItem.EasyBuilderDeveloper = dboIsEasyBuilderDeveloper;

			if (companyUserDb.ExistUser(contextUser.LoginId, tbCompanyId.Text) == 0)
				result = companyUserDb.Add(userListItem);
			else
				result = companyUserDb.Modify(userListItem);

			//per tutte le aziende che puntano a quel db devo modificare il dbowner a livello anagrafico
			CompanyDb companiesWithSameDb = new CompanyDb();
			companiesWithSameDb.ConnectionString = this.connectionString;
			companiesWithSameDb.CurrentSqlConnection = this.currentConnection;

			ArrayList companiesToModify = new ArrayList();
			companiesWithSameDb.SelectCompaniesSameServerAndDb
				(out companiesToModify, serverOrServiceName, contextUser.DatabaseName, tbCompanyId.Text);

			if (companiesToModify.Count > 0)
			{
				for (int i = 0; i < companiesToModify.Count; i++)
				{
					//per ognuna di esse va modificato il dbowner
					CompanyItem currentCompany = (CompanyItem)companiesToModify[i];
					string oldbOwner = currentCompany.DbOwner;
					currentCompany.DbOwner = contextUser.LoginId;
					//aggiorno l'azienda
					result = companyDb.Modify
						(
						currentCompany.CompanyId,
						currentCompany.Company,
						currentCompany.Description,
						currentCompany.ProviderId,
						currentCompany.DbServer,
						currentCompany.DbName,
						string.Empty,
						string.Empty,
						contextUser.LoginId, /* nuovo dbowner */
						currentCompany.UseSecurity,
						currentCompany.UseAuditing,
						currentCompany.UseTransaction,
						currentCompany.UseKeyedUpdate,
						currentCompany.DBAuthenticationWindows,
						currentCompany.PreferredLanguage,
						currentCompany.ApplicationLanguage,
						currentCompany.Disabled,
						currentCompany.UseUnicode,
						isValidCompany,
						companyLcid,
						supportColsCollation,
						currentCompany.Port,
						false, //currentCompany.UseDBSlave //qui non aggiorno il flag perche' vado cross su tutte le aziende
						currentCompany.UseRowSecurity,
                        currentCompany.UseDataSynchro
                        );

					//aggiorno la MSDCompanyLogins
					CompanyUserDb currentDbOwner = new CompanyUserDb();
					currentDbOwner.ConnectionString = this.connectionString;
					currentDbOwner.CurrentSqlConnection = this.currentConnection;

					//cancello il vecchio dbowner
					if (companyUserDb.ExistUser(oldbOwner, currentCompany.CompanyId) == 1)
						companyUserDb.Delete(oldbOwner, currentCompany.CompanyId);

					//@@TODO: gestire inserimento del nuovo dbowner anche per il Easy Attachment

					//aggiungo o modifico il nuovo
					UserListItem newDbownerItem = new UserListItem();
					newDbownerItem.LoginId = contextUser.LoginId;
					newDbownerItem.IsAdmin = true;
					newDbownerItem.Login = contextUser.LoginName;
					newDbownerItem.DbUser = contextUser.LoginName;
					newDbownerItem.DbPassword = contextUser.LoginPassword;
					newDbownerItem.CompanyId = currentCompany.CompanyId;
					newDbownerItem.DbWindowsAuthentication = contextUser.IsWinNT;

					// prima di aggiungere/modificare l'utente dbo associato vado ad impostare il flag EBDeveloper esistente
					newDbownerItem.EasyBuilderDeveloper = dboIsEasyBuilderDeveloper;

					if (companyUserDb.ExistUser(contextUser.LoginId, currentCompany.CompanyId) == 0)
						result = companyUserDb.Add(newDbownerItem);
					else
						result = companyUserDb.Modify(newDbownerItem);
				}
			}

			if (!result)
			{
				if (companyUserDb.Diagnostic.Error || companyUserDb.Diagnostic.Information || companyUserDb.Diagnostic.Warning)
				{
					diagnostic.Set(companyUserDb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
				}
				updating = false;
			}
			else
				updating = true;

			// valorizzo alcune variabili che servono poi successivamente
			companyId = tbCompanyId.Text;
			companyDbName = contextUser.DatabaseName;
			companyName = tbCompany.Text.Trim();

			return updating;
		}
		# endregion

		# region Salvataggio e cancellazione azienda
		/// <summary>
		/// Salvo i dati ed eventualmente creo il db
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			SaveCompany();
		}

		/// <summary>
		/// Effettuo il salvataggio di una generica azienda (poi richiamo le routine per gli specifici database)
		/// </summary>
		//---------------------------------------------------------------------------
		public bool SaveCompany()
		{
			Cursor.Current = Cursors.WaitCursor;
			TreeConsole.Cursor = Cursors.WaitCursor;

			bool result = false;
			bool isNewDb = false;
			
			sql2012MsgDisplayed = false;

			Control companySetting = DBCompanySettings.Controls[0];
			if (companySetting is DataBaseSqlLite)
			{
                DataBaseSqlLite dbSql = (DataBaseSqlLite)companySetting;
				isNewDb = dbSql.IsNewSQLCompany;
				companyDbName = dbSql.SelectedSQLDatabaseName;
				result = SaveCompanyForSQLProvider();
			}

			string action = (isNewDb) ? Strings.CreationAction : Strings.ModifyAction;
			if (result)
			{
				diagnostic.Set
					(DiagnosticType.Information,
					(inserting)
					? string.Format(Strings.AfterCompanySuccessInserting, string.Concat(companyName, " (" + companyDbName + ")"))
					: string.Format(Strings.AfterCompanySuccessModifying, string.Concat(companyName, " (" + companyDbName + ")"))
					);

				State = StateEnums.View;
			}
			else
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotCompanySave, action, string.Concat(companyName, " (" + companyDbName + ")")));
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				State = StateEnums.Editing;
				diagnostic.Clear();
			}

			// se l'operazione è andata a buon fine procedo con la creazione db
			if (result)
			{
				// ho creato una nuova azienda chiamo la OnClickUpdateDatabase in ApplicationDbAdmin
                if (companySetting is DataBaseSqlLite && isNewDb)
				{
					if (OnCreateDBStructure != null)
						OnCreateDBStructure(this, tbCompanyId.Text);
				}
				else
				{
					// non ho creato una nuova azienda ma:
					// 1. e' cambiato il valore della checkbox del documentale (controllo effettuato solo solo se il modulo e' attivato)
					// 2. e' cambiato il valore della checkbox del rowsecurity (controllo effettuato solo solo se il modulo e' attivato)
					if (
						(isDMSActivated && UseDBSlave.Checked && (inserting || (companyItem != null && !companyItem.UseDBSlave))) ||
						(isRowSecurityActivated && UseRowSecurityCBox.Checked && (inserting || (companyItem != null && !companyItem.UseRowSecurity)))
						)
						if (OnCreateDBStructure != null)
							OnCreateDBStructure(this, tbCompanyId.Text);
				}
			}

			TreeConsole.Cursor = Cursors.Default;
			Cursor.Current = Cursors.Default;
			return result;
		}

		//---------------------------------------------------------------------
		private void BtnDelete_Click(object sender, System.EventArgs e)
		{
			// prima di procedere nella cancellazione dell'azienda devo richiedere 
			// l'AuthenticationToken per il LoginManager
			bool canDelete = false;
			if (OnBeforeDeleteCompany != null)
				canDelete = OnBeforeDeleteCompany(sender, Convert.ToInt32(this.companyId));

			if (!canDelete)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.AuthenticationTokenNotValid);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				return;
			}

			Cursor.Current = Cursors.WaitCursor;

			if (DBCompanySettings.Controls.Count > 0)
			{
				if (DBCompanySettings.Controls[0] is DataBaseSqlLite)
					DeleteSqlData(sender, e);
			}

			Cursor.Current = Cursors.Default;
		}
		# endregion

		# region BuildStringConnection (per SQL)
		//---------------------------------------------------------------------
		private string BuildStringConnection(UserInfo user)
		{
			if (user.DbType == DBMSType.SQLSERVER)
				return
					(user.IsWinNT)
					? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, user.ServerComplete, user.DatabaseName)
					: string.Format(NameSolverDatabaseStrings.SQLConnection, user.ServerComplete, user.DatabaseName, user.LoginName, user.LoginPassword);

			return string.Empty;
		}
		# endregion

		# region Metodi per gestire le logins e gli utenti per SQL Server
		///<summary>
		/// TryToConnect
		/// Imposta la stringa di connessione e prova a connettersi
		///</summary>
		//---------------------------------------------------------------------
		private bool TryToConnect(TransactSQLAccess tentativeConnSql, UserInfo contextUserInfo)
		{
			tentativeConnSql.CurrentStringConnection = BuildStringConnection(contextUserInfo);
			return tentativeConnSql.TryToConnect();
		}

		///<summary>
		/// InsertLogin
		/// Inserisce una login sul server e la associa al database
		///</summary>
		//---------------------------------------------------------------------
		private bool InsertLogin(TransactSQLAccess connToServer, UserInfo contextUser)
		{
			bool result = false;

			//se ho settato autenticazione Windows
			if (contextUser.IsWinNT)
			{
				result =
					connToServer.SPGrantLogin(contextUser.LoginName) &&
					connToServer.SPGrantDbAccess(contextUser.LoginName, contextUser.LoginName, DatabaseLayerConsts.MasterDatabase) &&
					connToServer.SPAddSrvRoleMember(contextUser.LoginName, DatabaseLayerConsts.RoleSysAdmin, string.Empty);
			}
			else
			{
				result =
					connToServer.SPAddLogin(contextUser.LoginName, contextUser.LoginPassword, DatabaseLayerConsts.MasterDatabase) &&
					connToServer.SPGrantDbAccess(contextUser.LoginName, contextUser.LoginName, DatabaseLayerConsts.MasterDatabase) &&
					connToServer.SPAddSrvRoleMember(contextUser.LoginName, DatabaseLayerConsts.RoleSysAdmin, string.Empty);
			}

			return result;
		}

		///<summary>
		/// AskCredential
		/// Chiede le crenziali per impersonificare un utente
		///</summary>
		//---------------------------------------------------------------------
		private UserImpersonatedData AskCredential
			(
			TransactSQLAccess connToServer,
			UserInfo contextUser,
			bool canChangeCredential,
			bool askPassword
			)
		{
			return connToServer.LoginImpersonification
				(
				contextUser.LoginName,
				(askPassword) ? string.Empty : contextUser.LoginPassword,
				contextUser.Domain,
				contextUser.IsWinNT,
				contextUser.ServerPrimary,
				contextUser.ServerIstance,
				canChangeCredential
				);
		}

		///<summary>
		/// SetCompanyDboUser
		/// Chiede le credenziali di un utente valido in modo da poter inserire l'utente applicativo 
		/// ed associarlo al database aziendale (l'associazione avviene 1 a 1)
		///</summary>
		//---------------------------------------------------------------------
		private int SetCompanyDboUser(TransactSQLAccess connToServer, UserInfo contextUser)
		{
			string oldConnToServer = connToServer.CurrentStringConnection;

			//mostro la finestra per l'autenticazione - Di default propongo sa
			UserInfo contextUserSa = new UserInfo();
			contextUserSa.LoginName = DatabaseLayerConsts.LoginSa;
			contextUserSa.LoginPassword = string.Empty;
			contextUserSa.Domain = string.Empty;
			contextUserSa.ServerPrimary = contextUser.ServerPrimary;
			contextUserSa.ServerIstance = contextUser.ServerIstance;
			contextUserSa.DatabaseName = DatabaseLayerConsts.MasterDatabase;
			contextUserSa.DbType = DBMSType.SQLSERVER;

			//Chiedo un utente per effettuare il check
			UserImpersonatedData newCredential = AskCredential(connToServer, contextUserSa, true, true);
			//se l'impersonificazione non è andata a buon fine
			if (newCredential == null)
				return -1; //ho premuto cancel

			//Ok ho impersonificato
			UserInfo userForConnection = new UserInfo();
			userForConnection.LoginName = newCredential.Login;
			userForConnection.LoginPassword = newCredential.Password;
			userForConnection.Domain = newCredential.Domain;
			//mi connetto al master database
			userForConnection.DatabaseName = DatabaseLayerConsts.MasterDatabase;
			//il resto lo setto come l'utente
			userForConnection.ServerIstance = contextUser.ServerIstance;
			userForConnection.ServerPrimary = contextUser.ServerPrimary;

			//costruisco la stringa di connessione
			if (TryToConnect(connToServer, userForConnection))
			{
				bool ended = false;
				if (userForConnection.LoginName == contextUser.LoginName)
				{
					contextUser.LoginPassword = userForConnection.LoginPassword;
					connToServer.CurrentStringConnection = BuildStringConnection(contextUser);
					ended = true;
				}

				while (!ended)
				{
					//Ok (ha senso richiedere le credenziali di sa???  o bisogna usare userForConnection.LoginName
					if (contextUser.LoginName == DatabaseLayerConsts.LoginSa)
					{
						//l'utente è sa ma ho dato una pwd sbagliata
						UserImpersonatedData userSaCredential = AskCredential(connToServer, contextUser, false, true);
						if (userSaCredential != null)
						{
							contextUser.Impersonification = userSaCredential;
							contextUser.LoginPassword = userSaCredential.Password;
							connToServer.CurrentStringConnection = BuildStringConnection(contextUser);
							ended = true;
						}
						else
						{
							ended = true;
							return -1;
						}
					}
					else if (connToServer.ExistLogin(contextUser.LoginName))
					{
						if (!connToServer.ExistLoginIntoDb(contextUser.LoginName, contextUser.DatabaseName))
						{
							//TO do
						}
						//l'utente imputato esiste è sbagliata la pwd
						UserImpersonatedData userCredential = AskCredential(connToServer, contextUser, false, true);
						if (userCredential != null)
						{
							contextUser.LoginPassword = userCredential.Password;
							contextUser.Impersonification = userCredential;
							connToServer.CurrentStringConnection = BuildStringConnection(contextUser);
							if (CheckHasRoleSysAdmin(contextUser, connToServer))
								ended = true;
						}
						else
						{
							ended = true;
							return -1;
						}
					}
					else
					{
						//l'utente non esiste
						DialogResult insertNewUser = MessageBox.Show
							(
							string.Format(Strings.AskIfInsertingUserWithParam, contextUser.LoginName, contextUser.ServerComplete),
							Strings.AddNewUser,
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question
							);
						if (insertNewUser == DialogResult.No)
						{
							ended = true;
							return -1;
						}
						else
						{
							ended = InsertLogin(connToServer, contextUser);
							if (!ended)
							{
								if (connToServer.Diagnostic.Error || connToServer.Diagnostic.Warning || connToServer.Diagnostic.Information)
									diagnostic.Set(connToServer.Diagnostic);

								return -1;
							}
						}
					}
				}
			}

			//ho finito --- Posso Continuare
			if (newCredential != null)
				newCredential.Undo();

			return 0;
		}

		///<summary>
		/// CheckRoleDbOwner
		/// Controllo i ruoli associati ad un utente e, se le credenziali sono corrette, gli associo quelli mancanti
		///</summary>
		//---------------------------------------------------------------------------
		private int CheckRoleDbOwner(UserInfo currentUser, TransactSQLAccess currentConnection)
		{
			bool result = false;

			if (!currentConnection.LoginIsSystemAdminRole(currentUser.LoginName, DatabaseLayerConsts.RoleSysAdmin))
			{
				string messageForPermissionUser = string.Format(Strings.NoPermissionUser, currentUser.LoginName);
				diagnosticViewer.Message = messageForPermissionUser;
				diagnosticViewer.Title = Strings.Error;
				diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
				diagnosticViewer.ShowIcon = MessageBoxIcon.Error;
				DialogResult responseForRole = diagnosticViewer.Show();
				if (responseForRole == DialogResult.No)
					return -1;

				string ownerLogin = string.Empty;
				string ownerPwd = string.Empty;
				string ownerDomain = string.Empty;
				string oldString = currentConnection.CurrentStringConnection;

				//Siccome non ha un ruolo sysadmin devo chiedere nuove credenziali
				result = AskAndTestNewCredential
					(
					currentUser.ServerComplete,
					DatabaseLayerConsts.LoginSa,
					string.Empty,
					out ownerLogin,
					out ownerPwd,
					out ownerDomain,
					string.Empty,
					currentUser.ServerPrimary,
					currentUser.ServerIstance,
					currentConnection
					);

				if (!result)
					return -1;

				result = currentConnection.SPAddSrvRoleMember(currentUser.LoginName, DatabaseLayerConsts.RoleSysAdmin, string.Empty);
				currentConnection.CurrentStringConnection = oldString;

				if (!result)
				{
					if (currentConnection.Diagnostic.Error || currentConnection.Diagnostic.Warning || currentConnection.Diagnostic.Information)
						diagnostic.Set(currentConnection.Diagnostic);
					return -1;
				}
			}

			return 0;
		}
		
		///<summary>
		/// DeleteOldDbo
		/// Cambiando l'utente applicativo associato ad un database, forzo la cancellazione del
		/// legame nella tabella MSD_CompanyLogins
		///</summary>
		//---------------------------------------------------------------------
		private bool DeleteOldDbo(string nameOfOldDbo, string companyId)
		{
			bool isDeleted = false;
			if (companyId.Length == 0)
				return true;

			ArrayList oldDboData = new ArrayList();
			UserDb userDb = new UserDb();
			userDb.CurrentSqlConnection = this.currentConnection;
			userDb.ConnectionString = this.connectionString;

			bool searchResult = userDb.LoadFromLogin(out oldDboData, nameOfOldDbo);
			if (!searchResult)
			{
				if (userDb.Diagnostic.Error || userDb.Diagnostic.Warning || userDb.Diagnostic.Information)
				{
					diagnostic.Set(userDb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
				}
				oldDboData.Clear();
				return isDeleted;
			}

			if (oldDboData.Count > 0)
			{
				string loginId = ((UserItem)oldDboData[0]).LoginId;
				CompanyUserDb companyUserDb = new CompanyUserDb();
				companyUserDb.CurrentSqlConnection = this.currentConnection;
				companyUserDb.ConnectionString = this.connectionString;
				ArrayList userCompany = new ArrayList();
				searchResult = companyUserDb.GetUserCompany(out userCompany, loginId, companyId);
				if ((searchResult) && (userCompany.Count > 0))
				{
					CompanyUser companyUser = (CompanyUser)userCompany[0];
					//lo cancello
					isDeleted = companyUserDb.ForceDelete(companyUser.LoginId, companyUser.CompanyId);
				}
				else
					if (userCompany.Count == 0)
						isDeleted = true;
			}
			else
				isDeleted = true;

			return isDeleted;
		}

		///<summary>
		/// ChangeCompanyDbOwner
		/// Cambio il dbowner del database aziendale (per SQL Server)
		///</summary>
		//---------------------------------------------------------------------
		private bool ChangeCompanyDbOwner(TransactSQLAccess connSqlTransact, UserInfo currentUser)
		{
			bool successChanged = connSqlTransact.ChangeDbo
				(
				currentUser.ServerComplete,
				currentUser.IsWinNT,
				currentUser.LoginName,
				currentUser.LoginPassword,
				currentUser.DatabaseName
				);

			if (!successChanged)
			{
				if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning || connSqlTransact.Diagnostic.Information)
					diagnostic.Set(connSqlTransact.Diagnostic);
			}

			return successChanged;
		}

		/// <summary>
		/// CheckHasRoleSysAdmin
		/// Verifica che l'utente che deve diventare il dbo abbia il ruolo SysAdmin (nel caso lo aggiunge su richiesta)
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckHasRoleSysAdmin(UserInfo contextUser, TransactSQLAccess currentConnection)
		{
			bool result = false;

			if (!currentConnection.LoginIsSystemAdminRole(contextUser.LoginName, DatabaseLayerConsts.RoleSysAdmin))
			{
				string messageForPermissionUser = string.Format(Strings.NoPermissionUser, contextUser.LoginName);
				diagnosticViewer.Message = messageForPermissionUser;
				diagnosticViewer.Title = Strings.Error;
				diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
				diagnosticViewer.ShowIcon = MessageBoxIcon.Error;
				DialogResult responseForRole = diagnosticViewer.Show();
				if (responseForRole == DialogResult.No)
					return result;

				string ownerLogin = string.Empty;
				string ownerPwd = string.Empty;
				string ownerDomain = string.Empty;
				string oldString = currentConnection.CurrentStringConnection;

				//Siccome non ha un ruolo sysadmin devo chiedere nuove credenziali
				result = AskAndTestNewCredential
					(
					contextUser.ServerComplete,
					DatabaseLayerConsts.LoginSa,
					string.Empty,
					out ownerLogin,
					out ownerPwd,
					out ownerDomain,
					string.Empty,
					contextUser.ServerPrimary,
					contextUser.ServerIstance,
					currentConnection
					);

				if (!result)
					return false;

				result = currentConnection.SPGrantDbAccess(contextUser.LoginName, contextUser.LoginName, DatabaseLayerConsts.MasterDatabase) &&
						currentConnection.SPAddSrvRoleMember(contextUser.LoginName, DatabaseLayerConsts.RoleSysAdmin, string.Empty);

				currentConnection.CurrentStringConnection = oldString;

				if (!result)
				{
					if (currentConnection.Diagnostic.Error || currentConnection.Diagnostic.Warning || currentConnection.Diagnostic.Information)
					{
						diagnostic.Set(currentConnection.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					}
				}
			}
			else
				result = true;

			return result;
		}

		///<summary>
		/// AskAndTestNewCredential
		/// Chiede e verifica le nuove credenziali inserite
		///</summary>
		//---------------------------------------------------------------------
		private bool AskAndTestNewCredential
		(
			string serverName,
			string logiName,
			string loginPassword,
			out string ownerLogin,
			out string ownerPassword,
			out string ownerDomain,
			string domain,
			string primarySQLServer,
			string istanceSQLSever,
			TransactSQLAccess currentConnection
		)
		{
			string ownerLog = logiName;
			string ownerPwd = loginPassword;
			string ownerDom = domain;
			bool isWinAuth = (domain.Length == 0) ? false : true;
			bool credentialAreCorrect = false;
			string oldConnection = currentConnection.CurrentStringConnection;

			UserImpersonatedData newCredential = currentConnection.LoginImpersonification
				(
				logiName,
				string.Empty,
				domain,
				isWinAuth,
				primarySQLServer,
				istanceSQLSever,
				false
				);

			if (newCredential != null)
			{
				//ora provo a connettermi
				currentConnection.CurrentStringConnection =
					(newCredential.WindowsAuthentication)
					? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, DatabaseLayerConsts.MasterDatabase)
					: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase, newCredential.Login, newCredential.Password);

				credentialAreCorrect = currentConnection.TryToConnect();

				//se le credenziali sono sbagliate risetto la stringa come era prima
				if (!credentialAreCorrect)
					currentConnection.CurrentStringConnection = oldConnection;
				else
				{
					ownerLog = newCredential.Login;
					ownerPwd = newCredential.Password;
					ownerDom = newCredential.Domain;
				}
			}

			ownerLogin = ownerLog;
			ownerPassword = ownerPwd;
			ownerDomain = ownerDom;
			//se sono giuste, mi tengo la stringa nuova e vado avanti
			return credentialAreCorrect;
		}
		# endregion

		#region LoadDefaultData
		/// <summary>
		/// Se sono in modifica, carico i dati dell'azienda selezionata
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadDefaultData()
		{
			tbCompanyId.Text = string.Empty;
			PopolateCombosLanguage();

			cbUseTransaction.Checked = true;
			BtnSave.Text = Strings.AddCompany;
			BtnSave.Enabled = true;
			BtnDelete.Enabled = false;
			CbCompanyDisabled.Checked = false;

			//Siccome sto inserendo una nuova azienda, il flag relativo all'uso di Unicode è editabile
			cbUseUnicode.Enabled = true;

			//carico il dbowner x la nuova tab
			PopolateComboProvider(string.Empty);
			BuildTabDatabaseSettings();
			BuildTabDBSlave();

			if (DBCompanySettings.Controls.Count > 0)
			{
				if (DBCompanySettings.Controls[0] is DataBaseSqlLite)
				{
                    DataBaseSqlLite dbSql = (DataBaseSqlLite)DBCompanySettings.Controls[0];
					dbSql.LoadData(false);

					// StandardEdition, nessuna info sull'edition oppure indefinita
					if (hideDbChanges)
						dbSql.HideDbChanges();
				}
			}
		}
		#endregion

		#region Funzioni che popolano i controls della form e che effettuano i check dei dati inseriti
		/// <summary>
		/// Popola le combo PreferredLanguage con le lingue installate 
		/// </summary>
		//---------------------------------------------------------------------
		private void PopolateCombosLanguage()
		{
			cultureUICombo.LoadLanguagesUI();
			cultureApplicationCombo.LoadLanguages();
		}

		/// <summary>
		/// PopolateComboProvider
		/// Popolo la combo box leggendo la tabella MSD_Providers
		/// </summary>
		//---------------------------------------------------------------------
		private void PopolateComboProvider(string providerToSelect)
		{
			if (cbProvider.DataSource != null)
				cbProvider.DataSource = null;

			cbProvider.Items.Clear();

			ProviderDb providerDb = new ProviderDb();
			providerDb.ConnectionString = this.connectionString;
			providerDb.CurrentSqlConnection = this.currentConnection;

			ArrayList providersList = new ArrayList();

			if (!providerDb.SelectAllProviders(out providersList))
			{
				diagnostic.Set(providerDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				providersList.Clear();
			}

			if (providersList.Count > 0)
			{
				for (int i = 0; i < providersList.Count; i++)
					cbProvider.Items.Add(providersList[i]);

				cbProvider.DisplayMember = "Description";
				cbProvider.ValueMember = "ProviderId";
			}

			if (providerToSelect.Length == 0)
				providerToSelect = DatabaseLayerConsts.SqlOleProviderDescription;

			int pos = cbProvider.FindStringExact(providerToSelect, -1);
			if (pos >= 0)
				cbProvider.SelectedIndex = pos;

			// se siamo in SmallNetwork metto readonly la combo ed inizializzo solo con il provider SqlServer
			if (licenceInfo.DBNetworkType == DBNetworkType.Small)
			{
				providerToSelect = DatabaseLayerConsts.SqlOleProviderDescription;
				this.cbProvider.Enabled = false;
			}
		}

		///<summary>
		/// PopolateDatabaseCultureSettings
		/// Per aggiornare correttamente le label relative alla DatabaseCulture e alla
		/// Collation applicata sulle colonne nella tab 'Parametri' (visualizzata solo per SQL Server)
		///</summary>
		//---------------------------------------------------------------------
		private void PopolateDatabaseCultureSettings(CompanyItem companyItem)
		{
			if (companyItem.DatabaseCulture != 0)
			{
				CultureInfo ci = new CultureInfo(companyItem.DatabaseCulture);
				CultureDBLabel.Text = ci.DisplayName;
			}

			CollationDBLabel.Text =
				DBGenericFunctions.ReadCollationFromDatabase(BuildConnection(companyItem.CompanyId), DBMSType.SQLSERVER);
		}

		///<summary>
		/// ShowPieChart
		/// Solo per chi ha l'edizione Standard o Professional Pro-Lite e solo per SQL Server 
		/// mostro il grafico a torta con le percentuali di utilizzo della dimensione fissa del database (impostata a 2GB)
		///</summary>
		//---------------------------------------------------------------------
		private void ShowPieChart(CompanyItem companyItem)
		{
			bool showControls = (licenceInfo.DBNetworkType == DBNetworkType.Small);

			DBSizeGroupBox.Visible = showControls;
			DBSizePieChart.Visible = showControls;
			UsedSpaceColor.Visible = showControls;
			FreeSpaceColor.Visible = showControls;
			UsedSpaceLabel.Visible = showControls;
			FreeSpaceLabel.Visible = showControls;

			// se non devo mostrare i controls non procedo con il calcolo delle percentuali
			if (!showControls)
				return;

			float usagePercentage =
				Functions.GetDBPercentageUsedSize(BuildConnection(companyItem.CompanyId));

			DBSizePieChart.Slices = new float[] { usagePercentage, 100.0F - usagePercentage };
			this.UsedSpaceLabel.Text = string.Format(this.UsedSpaceLabel.Text, DBSizePieChart.Slices[0].ToString());
			this.FreeSpaceLabel.Text = string.Format(this.FreeSpaceLabel.Text, DBSizePieChart.Slices[1].ToString());
		}

		/// <summary>
		/// CheckSqlCompanyValidator
		/// Check validità dei dati inseriti nella form per SqlServer
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckSqlValidator(DataBaseSqlLite dbSql)
		{
			bool result = true;

			// il nome azienda non può terminare con il carattere '.' per potenziali problemi legati alla composizione del namespace
			if (tbCompany.Text.IndexOfAny(Path.GetInvalidPathChars()) != -1 ||
				tbCompany.Text.Trim().IndexOfAny(new char[] { '?', '*', Path.DirectorySeparatorChar, '/', '<', '>', ':', '!', '|', '+', '[', ']', ',', '@', '=' }) != -1 ||
				tbCompany.Text.Trim().EndsWith("."))
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.WrongCharactersInCompanyName);
				result = false;
			}
			if (string.IsNullOrWhiteSpace(tbCompany.Text.Trim()))
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.EmptyCompany);
				result = false;
			}
			if (cbProvider.SelectedItem == null || cbProvider.Items.Count == 0)
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.NotSelectedProvider);
				result = false;
			}

			if (this.DBCompanySettings.Controls.Count > 0)
				dbSql = (DataBaseSqlLite)this.DBCompanySettings.Controls[0];

			if (dbSql != null)
			{
				if (string.IsNullOrWhiteSpace(dbSql.SelectedSQLServerName))
				{
					diagnostic.Set(DiagnosticType.Warning, Strings.NotSelectedSQLServer);
					result = false;
				}
				if (string.IsNullOrWhiteSpace(dbSql.SelectedDbOwnerName))
				{
					diagnostic.Set(DiagnosticType.Warning, Strings.NotSelectedCompanyUsers);
					result = false;
				}
				if (dbSql.IsNewSQLCompany)
				{
					if (!string.IsNullOrWhiteSpace(dbSql.SelectedSQLNewDatabaseName) &&
						(dbSql.SelectedSQLNewDatabaseName.IndexOfAny(
						new char[] { '?', '*', Path.DirectorySeparatorChar, '/', '<', '>', ':', '!', '|', '+', '[', ']', ',', '@', '=' }) != -1))
					{
						diagnostic.Set(DiagnosticType.Warning, Strings.WrongCharactersInDatabaseName);
						result = false;
					}

					if (string.IsNullOrWhiteSpace(dbSql.SelectedSQLNewDatabaseName))
					{
						diagnostic.Set(DiagnosticType.Warning, Strings.DatabaseEmpty);
						result = false;
					}
					else if (!IsValidDatabaseNameForSQL(dbSql.SelectedSQLNewDatabaseName))
					{
						// controllo che il nome del database non corrisponda ad uno di quelli di sistema/esempio
						diagnostic.Set(DiagnosticType.Warning, string.Format(Strings.InvalidDatabase, dbSql.SelectedSQLNewDatabaseName));
						result = false;
					}
				}
				else
					if (string.IsNullOrWhiteSpace(dbSql.SelectedSQLDatabaseName))
					{
						diagnostic.Set(DiagnosticType.Warning, Strings.NotSelectedDatabase);
						result = false;
					}
			}

			// devo effettuare un controllo incrociato in modo da escludere eventuali dms con unicode e collate diversa.
			//if (UseDBSlave.Checked)
			//	result = result && CheckDMSValidator();

			return result;
		}

  		/// <summary>
		/// CheckDMSValidator
		/// Check validità dei dati inseriti per il database documentale
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckDMSValidator()
		{
			bool result = true;

            // carico le info dello usercontrol del Easy Attachment
            DataBaseSqlLite dmsDb = null;
			if (SlavePanel.Controls.Count > 0)
			{
				if (SlavePanel.Controls[0] is DataBaseSqlLite)
					dmsDb = (DataBaseSqlLite)SlavePanel.Controls[0];
			}

			if (dmsDb == null)
				return result;

			if (string.IsNullOrWhiteSpace(dmsDb.SelectedSQLServerName))
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.NotSelectedSQLServer);
				result = false;
			}
			if (string.IsNullOrWhiteSpace(dmsDb.SelectedDbOwnerName))
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.NotSelectedCompanyUsers);
				result = false;
			}

			if (dmsDb.IsNewSQLCompany)
			{
				if (!string.IsNullOrWhiteSpace(dmsDb.SelectedSQLNewDatabaseName) &&
					(dmsDb.SelectedSQLNewDatabaseName.IndexOfAny(
					new char[] { '?', '*', Path.DirectorySeparatorChar, '/', '<', '>', ':', '!', '|', '+', '[', ']', ',', '@', '=' }) != -1))
				{
					diagnostic.Set(DiagnosticType.Warning, Strings.WrongCharactersInDatabaseName);
					result = false;
				}

				if (string.IsNullOrWhiteSpace(dmsDb.SelectedSQLNewDatabaseName))
				{
					diagnostic.Set(DiagnosticType.Warning, Strings.DatabaseEmpty);
					result = false;
				}
				else if (!IsValidDatabaseNameForSQL(dmsDb.SelectedSQLNewDatabaseName))
				{
					// controllo che il nome del database non corrisponda ad uno di quelli di sistema/esempio
					diagnostic.Set(DiagnosticType.Warning, string.Format(Strings.InvalidDatabase, dmsDb.SelectedSQLNewDatabaseName));
					result = false;
				}
			}
			else
				if (string.IsNullOrWhiteSpace(dmsDb.SelectedSQLDatabaseName))
				{
					diagnostic.Set(DiagnosticType.Warning, Strings.NotSelectedDatabase);
					result = false;
				}

			// in base al provider selezionato carico le info del database aziendale dallo usercontrol corretto
			ProviderItem selectedProvider = (ProviderItem)cbProvider.SelectedItem;
			if (selectedProvider != null)
			{
                DataBaseSqlLite dbSql = null;
				if (string.Compare(selectedProvider.ProviderValue, NameSolverDatabaseStrings.SQLOLEDBProvider, StringComparison.InvariantCultureIgnoreCase) == 0 || 
					string.Compare(selectedProvider.ProviderValue, NameSolverDatabaseStrings.SQLODBCProvider, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					if (DBCompanySettings.Controls.Count > 0 && DBCompanySettings.Controls[0] is DataBaseSqlLite)
						dbSql = (DataBaseSqlLite)DBCompanySettings.Controls[0];
				}
				// effettuo un controllo che, a parita' di istanza di SQL, i nomi dei due database siano differenti (solo per SQL)
				if (dbSql != null)
				{
					// se il nome server e il nome istanza sono uguali
					if (string.Compare(dmsDb.SelectedSQLServerName, dbSql.SelectedSQLServerName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
						string.Compare(dmsDb.SelectedSQLIstanceName, dbSql.SelectedSQLIstanceName, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						string dmsDbName = dmsDb.SelectedSQLDatabaseName;
						string companyDbName = dbSql.SelectedSQLDatabaseName;

						// se i nomi dei database sono uguali visualizzo un msg e non procedo
						if (string.Compare(dmsDbName, companyDbName, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							diagnostic.Set(DiagnosticType.Warning, string.Format(Strings.CompanyAndDMSDbWithSameName, dmsDbName));
							return false;
						}
					}
				}
			}

			// devo effettuare un controllo incrociato in modo da escludere eventuali dms con unicode e collate diversa.
			// faccio il controllo solo se sto agganciando un db esistente
			if (!dmsDb.IsNewSQLCompany)
			{
				bool dbMarkExist = false;
				// controllo il valore unicode nel database dms
				bool dmsUseUnicode = dmsDb.CheckUnicodeOnDB(out dbMarkExist);
				// ulteriore controllo in caso di mancata connessione (se ho scritto male il nome del db o del server)
				if (dmsDb.DBSqlDiagnostic.Error)
				{
					DiagnosticViewer.ShowDiagnosticAndClear(dmsDb.DBSqlDiagnostic);
					return false;
				}			
				
				// se la tabella non esiste allora ritorno true e procedo
				if (!dbMarkExist)
					return true;

				// se e' diverso da quello impostato automaticamente per il db aziendale visualizzo un msg e non procedo.
				if (cbUseUnicode.Checked != dmsUseUnicode)
				{
					// se il modulo e' attivato visualizzo messaggio
					if (isDMSActivated)
					{
						diagnostic.Set(DiagnosticType.Warning, Strings.WrongDMSUnicodeSetting);
						result = false;
					}
					else// se il modulo non e' attivato tolgo in automatico il flag? oppure visualizzo altro messaggio?
						UseDBSlave.Checked = false;
				}
			}

			return result;
		}
		#endregion

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

		#region Funzioni relative alla ProgressBar
		/// <summary>
		/// Dice alla MC di abilitare la ProgressBar
		/// </summary>
		//---------------------------------------------------------------------------
		private void EnableConsoleProgressBar(object sender)
		{
			if (OnEnableProgressBar != null)
				OnEnableProgressBar(sender);
		}

		/// <summary>
		/// Dice alla MC di disabilitare la ProgressBar
		/// </summary>
		//---------------------------------------------------------------------------
		private void DisableConsoleProgressBar(object sender)
		{
			if (OnDisableProgressBar != null)
				OnDisableProgressBar(sender);
		}

		/// <summary>
		/// Dice alla MC di impostare lo Step della ProgressBar
		/// al valore di step
		/// </summary>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarStep(object sender, int step)
		{
			if (OnSetProgressBarStep != null)
				OnSetProgressBarStep(sender, step);
		}

		/// <summary>
		/// Dice alla MC di impostare il value della ProgressBar
		/// pari a currentValue
		/// </summary>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarValue(object sender, int currentValue)
		{
			if (OnSetProgressBarValue != null)
				OnSetProgressBarValue(sender, currentValue);
		}

		/// <summary>
		/// Dice alla MC quale deve essere il testo da visualizzare 
		/// accanto alla progressBar (pari a message)
		/// </summary>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarText(object sender, string message)
		{
			if (OnSetProgressBarText != null)
				OnSetProgressBarText(sender, message);
		}

		//---------------------------------------------------------------------------
		private void SetConsoleCyclicStepProgressBar()
		{
			if (OnSetCyclicStepProgressBar != null)
				OnSetCyclicStepProgressBar();
		}
		#endregion

		/// <summary>
		/// ManageUsersAndLoginsForCompany (per SQL Server)
		/// Estraggo dal db di sistema tutti gli utenti associati all'azienda appena salvata e, per ognuno,
		/// vado ad aggiornare il server SQL, inserendo la login a livello di server e l'utente a livello di db.
		/// E' necessario effettuare l'operazione perchè dando la possibilità in ogni momento di cambiare il 
		/// server sql a cui punta l'azienda, bisogna aggiungere gli utenti 
		/// </summary>
		//---------------------------------------------------------------------
		private bool ManageUsersAndLoginsForCompany(TransactSQLAccess connSqlTransact, string databaseName)
		{
			if (this.currentConnection == null)
				return false;

			CompanyUserDb user = null;
			SqlDataReader myReader = null;
			bool result = true;

			try
			{
				if (this.currentConnection.State != ConnectionState.Open)
					this.currentConnection.Open();

				user = new CompanyUserDb(this.connectionString);
				user.CurrentSqlConnection = this.currentConnection;
				user.GetAll(out myReader, tbCompanyId.Text);

				if (myReader == null)
					return false;

				while (myReader.Read())
				{
					string login = myReader["DBUser"].ToString();
					string password = Crypto.Decrypt(myReader["DBPassword"].ToString());
					bool dbWinAuth = bool.Parse(myReader["DBWindowsAuthentication"].ToString());

					// skippo l'utente sa
					if (string.Compare(login, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;

					// se la login risulta dbo del database allora non procedo al suo inserimento, infatti l'utente è già
					// stato assegnato prima quando si è creato fisicamente il database
					if (connSqlTransact.ExistUserAndLoginIntoDb(Consts.DboUser, login, databaseName))
						continue;

					// se la login e' gia' associata al database non procedo
					if (connSqlTransact.ExistLoginIntoDb(login, databaseName))
						continue;

					if (dbWinAuth)
					{
						result =
							connSqlTransact.SPGrantLogin(login) &&
							connSqlTransact.SPGrantDbAccess(login, login, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDataWriter, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDataReader, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDbOwner, databaseName);
					}
					else
					{
						result =
							connSqlTransact.SPAddLogin(login, password, DatabaseLayerConsts.MasterDatabase) &&
							connSqlTransact.SPGrantDbAccess(login, login, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDataWriter, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDataReader, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDbOwner, databaseName);
					}
				}

				if (myReader != null)
					myReader.Close();
			}
			catch
			{
				result = false;
				if (myReader != null)
					myReader.Close();

				Debug.WriteLine("Error in ManageUsersAndLoginsForCompany method");
				DiagnosticViewer.ShowDiagnostic(connSqlTransact.Diagnostic);
				if (this.OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}

			return result;
		}

		# region Controlli di compatibilità per la DatabaseCulture
		/// <summary>
		/// CompatibilitySQLDatabaseCulture - per SQL Server
		/// Effettua controlli per capire se la culture specificata è compatibile con quella letta dal database
		/// </summary>
		/// <param name="contextUser">oggetto di tipo UserInfo</param>
		/// <returns>
		/// 0: tutto ok. 
		/// 1: errore bloccante per collate diverse e collate sul db non Latina.
		/// 2: warning relativo a collate diverse e richiesta se procedere
		/// </returns>
		//---------------------------------------------------------------------
		private int CompatibilitySQLDatabaseCulture(UserInfo contextUser)
		{
			// companyDBCulture è valorizzato dalla finestra del DatabaseCreation
			if (!string.IsNullOrWhiteSpace(companyDBCulture))
			{
				// mi serve per assegnare il valore dell'LCID nell'anagrafica azienda
				// comanda sempre quella impostata sul database
				CultureInfo ci = new CultureInfo(companyDBCulture);
				companyLcid = ci.LCID;

				columnCollation = CultureHelper.GetWindowsCollation(ci.LCID);
				dbCollation = NameSolverDatabaseStrings.SQLLatinCollation;

				supportColsCollation = ((columnCollation != dbCollation) &&
					!CultureHelper.IsCollationCompatibleWithCulture(companyLcid, dbCollation));

				return 0;
			}
			else
			{
				// DATABASE NON CREATO DA ZERO!
				string connectionString = BuildStringConnection(contextUser);

				// mi connetto al db e controllo che ci siano delle tabelle. se così non fosse devo alterare la sua
				// collate in latina, ovviamente solo se diversa
				DBGenericFunctions.SetDatabaseCollationIfNoUserTables(connectionString, contextUser.DbType);

				companyLcid = DBGenericFunctions.AssignDatabaseCultureValue
					(
					this.licenceInfo.IsoState,
					connectionString,
					contextUser.DbType,
					contextUser.UseUnicode
					);

				if (companyLcid != 0)
				{
					supportColsCollation = DBGenericFunctions.CalculateSupportColumnCollation
						(
						connectionString,
						companyLcid,
						out dbCollation,
						out columnCollation,
						contextUser.DbType,
						contextUser.UseUnicode
						);

					// errore bloccante non è possibile associare il db: ha una collate <> dal latino e sulle colonne è diversa
					if (supportColsCollation && dbCollation != NameSolverDatabaseStrings.SQLLatinCollation)
						return 1;
				}

				// si tratta di un'azienda esistente alla quale sto associando un database esistente
				if (this.companyId.Length > 0)
				{
					CompanyDb myCompanyDb = new CompanyDb();
					myCompanyDb.CurrentSqlConnection = this.currentConnection;
					int currentCompanyLcid = myCompanyDb.GetCompanyDatabaseCulture(this.companyId);

					// se l'LCID letto estrapolato dall'isostato e' 0  nel dubbio gli assegno quello letto dal db
					if (companyLcid == 0)
					{
						companyLcid = currentCompanyLcid;
						// ri-eseguo il controllo del flag di supporto della collation di colonna
						supportColsCollation =
							DBGenericFunctions.CalculateSupportColumnCollation
							(
							connectionString,
							companyLcid,
							out dbCollation,
							out columnCollation,
							contextUser.DbType,
							contextUser.UseUnicode
							);

						// errore bloccante non è possibile associare il db: ha una collate <> dal latino e sulle colonne è diversa
						if (supportColsCollation && dbCollation != NameSolverDatabaseStrings.SQLLatinCollation)
							return 1;
					}
					else if (currentCompanyLcid != companyLcid)
						return 2;
				}

				return 0;
			}
		}
		# endregion

		#region BuildConnection - Stringa di connessione all'azienda (per SQL)
		/// <summary>
		/// BuildConnection - Stringa di connessione all'azienda (per SQL)
		/// </summary>
		//---------------------------------------------------------------------
		private string BuildConnection(string companyId)
		{
			string connectionToCompanyServer = string.Empty, dbOwnerId = string.Empty;

			//leggo i dati dall'azienda
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.connectionString;
			companyDb.CurrentSqlConnection = this.currentConnection;

			ArrayList companyData = new ArrayList();
			if (!companyDb.GetAllCompanyFieldsById(out companyData, companyId))
			{
				diagnostic.Set(companyDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				companyData.Clear();
			}

			if (companyData.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)companyData[0];
				dbOwnerId = companyItem.DbOwner;

				//Ora leggo le credenziali del dbo dal MSD_CompanyLogins
				CompanyUserDb companyUser = new CompanyUserDb();
				companyUser.ConnectionString = this.connectionString;
				companyUser.CurrentSqlConnection = this.currentConnection;
				ArrayList userDboData = new ArrayList();

				companyUser.GetUserCompany(out userDboData, dbOwnerId, companyId);

				if (userDboData.Count > 0)
				{
					CompanyUser companyDbo = (CompanyUser)userDboData[0];
					//ora compongo la stringa di connessione
					connectionToCompanyServer =
						(companyDbo.DBWindowsAuthentication)
						? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, companyItem.DbServer, companyItem.DbName)
						: string.Format(NameSolverDatabaseStrings.SQLConnection, companyItem.DbServer, companyItem.DbName, companyDbo.DBDefaultUser, companyDbo.DBDefaultPassword);
				}
			}

			return connectionToCompanyServer;
		}
		#endregion

		#region Funzioni per la creazione/cancellazione del database
		/// <summary>
		/// Creazione del database (per SQL Server)
		/// Richiama la form con i dettagli per la creazione del database aziendale
		/// </summary>
		//---------------------------------------------------------------------
		private bool CreateDatabase(UserInfo contextUser, bool showUnicodeCheck, out bool useUnicode, bool showDialog, bool disableDBCultureComboBox)
		{
			// in fase di creazione database devo proporre una culture per il database  da visualizzare poi nella CreateDBForm
			// ma se ho scelto di disabilitare la combobox significa che sto gestendo il documentale e allora 
			// devo inizializzarla con quella prescelta per l'azienda
			string initializeDBCulture = string.Empty;
			if (disableDBCultureComboBox)
			{
				CultureInfo ci = new CultureInfo(companyLcid);
				initializeDBCulture = ci.Name;
			}
			else
				initializeDBCulture = DBGenericFunctions.PurposeDatabaseCultureForDBCreation(licenceInfo.IsoState);

			// se devo mostrare la form carico i parametri per la creazione del database e la mostro a video
			if (showDialog)
			{
				CreateDBForm createDBForm = new CreateDBForm
				(
					true,                           // isCompanyDB
					contextUser.ServerPrimary,      // serverName
					contextUser.ServerIstance,      // istanceName
					contextUser.DatabaseName,       // databaseName
					contextUser.LoginName,          // loginName
					contextUser.LoginPassword,      // password
					!string.IsNullOrWhiteSpace(contextUser.Domain),// isWinAuth
					showUnicodeCheck,               // showUnicodeCheck,
					true,                           // enableUnicodeCheck,
					this.cbUseUnicode.Checked,      // initUnicodeCheckValue
					this.licenceInfo.DBNetworkType, // dbNetworkType
					initializeDBCulture,            // initDBCulture (only company db),
					disableDBCultureComboBox,       // disableDBCultureComboBox (it's true only for Easy Attachment db)
					isAzureDb: licenceInfo.IsAzureSQLDatabase
				);

				if (createDBForm.ShowDialog() == DialogResult.OK)
					companyDBCulture = createDBForm.DatabaseCulture;

				// inizializzo cmq questa variabile
				useUnicode = createDBForm.UseUnicode;

				if (!createDBForm.Result)
				{
					if (createDBForm.CreateDBDiagnostic.Error || createDBForm.CreateDBDiagnostic.Warning || createDBForm.CreateDBDiagnostic.Information)
						diagnostic.Set(createDBForm.CreateDBDiagnostic);
					else
						diagnostic.Set(DiagnosticType.Error, Strings.AbortingDBCreation);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
				}

				return createDBForm.Result;
			}

			string serverName = contextUser.ServerPrimary;
			if (!string.IsNullOrEmpty(contextUser.ServerIstance))
				serverName = Path.Combine(serverName, contextUser.ServerIstance);

			string connString = string.Empty;

			if (licenceInfo.IsAzureSQLDatabase)
				connString = string.Format(NameSolverDatabaseStrings.SQLAzureConnection, serverName, DatabaseLayerConsts.MasterDatabase, contextUser.LoginName, contextUser.LoginPassword);
			else
				connString = (!string.IsNullOrWhiteSpace(contextUser.Domain))
							? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, DatabaseLayerConsts.MasterDatabase)
							: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase, contextUser.LoginName, contextUser.LoginPassword);

			bool result = false;
			DatabaseTask createDbTask = new DatabaseTask();
			createDbTask.OperationCompleted += CreateDbTask_OperationCompleted;
			createDbTask.CurrentStringConnection = connString;

			if (licenceInfo.IsAzureSQLDatabase)
			{
				new Task(() =>
				{
					AzureCreateDBParameters azureParams = new AzureCreateDBParameters();
					azureParams.DatabaseName = contextUser.DatabaseName;
					result = createDbTask.CreateAzureDatabase(azureParams);
				})
				.Start();

				progressForm = new ProgressForm();
				progressForm.Text = string.Format("Creating database '{0}'", contextUser.DatabaseName);
				progressForm.ShowDialog();
			}
			else
			{
				SQLCreateDBParameters createParams = new SQLCreateDBParameters();
				createParams.DatabaseName = contextUser.DatabaseName;
				result = createDbTask.CreateSQLDatabase(createParams);
			}

			companyDBCulture = initializeDBCulture;
			useUnicode = licenceInfo.UseUnicodeSet();

			return result;
		}

		//---------------------------------------------------------------------
		private void CreateDbTask_OperationCompleted(object sender, DatabaseTaskEventArgs e)
		{
			Invoke(new MethodInvoker(() =>
			{
				if (progressForm != null)
				{
					progressForm.ElaborationInProgress = false;
					progressForm.Close();
				}

				if (e.Result)
					diagnostic.Set(e.DatabaseTaskDiagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, Strings.AbortingDBCreation);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			}
			));
		}

		/// <summary>
		/// DeleteSqlData
		/// Cancellazione azienda per SQL:
		/// 1. anagrafica nel db di sistema (SEMPRE)
		/// 2. database aziendale (su richiesta)
		/// 3. database documentale (su richiesta e se il modulo e' attivato)
		///</summary>
		//---------------------------------------------------------------------
		public bool DeleteSqlData(object sender, System.EventArgs e)
		{
			bool companyDbDeleted = false;
			bool dmsDbDeleted = false;
			bool isDMSActivated = false;
			if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				isDMSActivated = true; // devo richiedere le info perche' se non apro il DE dell'azienda potrei non avere il valore corretto

			// visualizzo la form con le opzioni di cancellazione
			DeleteCompany dc = new DeleteCompany(companyItem.UseDBSlave, isDMSActivated, true);
			// se risponde no non procedo
			if (dc.ShowDialog() == DialogResult.Cancel)
				return companyDbDeleted;

			// leggo tutte le informazioni dell'azienda corrente
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = connectionString;
			companyDb.CurrentSqlConnection = currentConnection;
			// carico le info della company da eliminare
			ArrayList companyToDelete = new ArrayList();
			if (!companyDb.GetAllCompanyFieldsById(out companyToDelete, companyId))
			{
				if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
				{
					diagnostic.Set(companyDb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					companyToDelete.Clear();
				}
			}

			// se non c'e' nulla da cancellare ritorno
			if (companyToDelete.Count == 0)
				return companyDbDeleted;

			CompanyItem currentCompany = ((CompanyItem)companyToDelete[0]);

			// carico tutte le aziende che puntano allo stesso database
			ArrayList moreCompaniesToDelete = new ArrayList();
			companyDb.SelectCompaniesSameDataSource(out moreCompaniesToDelete, currentCompany.DbName, currentCompany.DbServer);
			// se più di un'azienda è associata allo stesso database aziendale e ho richiesto di eliminare anche il db
			// visualizzo un messaggio e chiedo se si vuole procedere
			if (dc.DeleteCompanyDB && moreCompaniesToDelete.Count > 1)
			{
				//Disabilito la progressBar
				SetConsoleProgressBarText(sender, string.Empty);
				DisableConsoleProgressBar(sender);
				diagnosticViewer.Message = string.Format(Strings.AskIfDeleteMoreCompanies, currentCompany.DbName);
				diagnosticViewer.Title = Strings.Delete;
				diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
				diagnosticViewer.ShowIcon = MessageBoxIcon.Warning;
				diagnosticViewer.DefaultButton = MessageBoxDefaultButton.Button2;
				DialogResult resultAllDelete = diagnosticViewer.Show();
				if (resultAllDelete == DialogResult.No)
					return companyDbDeleted;
			}

			// TODO: qui dovrei controllare se il database documentale e' agganciato a piu' aziende...
			// ed eventualmente dare un messaggio come qui sopra viene fatto per la company
			// Al momento non è gestita la cancellazione delle aziende agganciate allo stesso database 
			// documentale che stiamo cancellando
			// se più aziende hanno uno slave associato allo stesso database chiedo se si vuole procedere
			// if (dc.DeleteDMSDB && currentCompany.UseDBSlave && dc.IsDMSActivated)

			// istanzio una volta sola la classe per l'impersonificazione (dopo è usata in due punti)
			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(this.AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(this.GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(this.IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(SendHelp);
			connSqlTransact.CurrentStringConnection = connectionString;
			connSqlTransact.CurrentConnection = currentConnection;

			string serverName = currentCompany.DbServer;
			string dataSourceName = currentCompany.DbName;
			string ownerId = currentCompany.DbOwner;
			//lo espongo per comunicarlo al lock manager se necessario
			companyDbName = currentCompany.DbName;

			// carico le informazioni dell'utente dbowner (mi servono anche dopo per l'eventuale DMS)
			CompanyUserDb userOwnerdb = new CompanyUserDb();
			userOwnerdb.ConnectionString = connectionString;
			userOwnerdb.CurrentSqlConnection = currentConnection;

			ArrayList userData = new ArrayList();
			userOwnerdb.GetUserCompany(out userData, ownerId, companyId);

			// non esiste il dbowner e non procedo
			if (userData.Count == 0)
				return companyDbDeleted;
			
			CompanyUser currentUser = ((CompanyUser)userData[0]);

			// cancello il database aziendale
			if (dc.DeleteCompanyDB)
			{
				SetConsoleProgressBarValue(sender, 3);
				SetConsoleProgressBarText(sender, Strings.ProgressWaiting);
				SetConsoleProgressBarStep(sender, 3);
				EnableConsoleProgressBar(sender);
				this.Cursor = Cursors.WaitCursor;

				string ownerLogin = Path.GetFileName(currentUser.DBDefaultUser);
				string ownerDomain = currentUser.DBWindowsAuthentication ? Path.GetDirectoryName(currentUser.DBDefaultUser) : string.Empty;
				string[] serverIstance = serverName.Split(Path.DirectorySeparatorChar);
				string ownerPassword = currentUser.DBDefaultPassword;

				string tempDbServer = string.Empty;
				string tempDbIstance = string.Empty;
				if (serverIstance.Length > 1)
					tempDbIstance = serverIstance[1];
				tempDbServer = serverIstance[0];

				SetConsoleProgressBarText(sender, string.Empty);
				DisableConsoleProgressBar(sender);

				// richiedo le credenziali di connessione
				UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
				dataToConnectionServer = connSqlTransact.LoginImpersonification
					(
					ownerLogin,
					ownerPassword,
					ownerDomain,
					currentUser.DBWindowsAuthentication,
					tempDbServer,
					tempDbIstance,
					false
					);

				if (dataToConnectionServer == null)
				{
					SetConsoleProgressBarText(sender, string.Empty);
					DisableConsoleProgressBar(sender);
					Cursor.Current = Cursors.Default;
				}
				else
					companyDbDeleted = true;

				SetConsoleProgressBarText(sender, Strings.ProgressWaiting);
				EnableConsoleProgressBar(sender);

				// se sono riuscita a connettermi procedo ad eliminare il database
				if (companyDbDeleted)
				{
					companyDbDeleted = connSqlTransact.DeleteDataBase
						(
						serverName,
						dataSourceName,
						currentUser.DBWindowsAuthentication ? Path.Combine(dataToConnectionServer.Domain, dataToConnectionServer.Login) : currentUser.DBDefaultUser,
						currentUser.DBWindowsAuthentication ? dataToConnectionServer.Password : currentUser.DBDefaultPassword,
						currentUser.DBWindowsAuthentication
						);

					if (!companyDbDeleted)
						diagnostic.Set(connSqlTransact.Diagnostic);
				}

				if (dataToConnectionServer != null)
					dataToConnectionServer.Undo();

				SetConsoleProgressBarText(sender, string.Empty);
				DisableConsoleProgressBar(sender);
			}

			// cancello il database documentale (se l'azienda lo supporta e se il modulo e' attivato)
			if (dc.DeleteDMSDB && currentCompany.UseDBSlave && isDMSActivated)
			{
				SetConsoleProgressBarValue(sender, 3);
				SetConsoleProgressBarText(sender, Strings.ProgressWaiting);
				SetConsoleProgressBarStep(sender, 3);
				EnableConsoleProgressBar(sender);
				this.Cursor = Cursors.WaitCursor;

				// carico le info dello Slave
				CompanyDBSlave dbSlave = new CompanyDBSlave();
				dbSlave.ConnectionString = connectionString;
				dbSlave.CurrentSqlConnection = currentConnection;
				CompanyDBSlaveItem dbSlaveItem = new CompanyDBSlaveItem();
				if (!dbSlave.SelectSlaveForCompanyId(currentCompany.CompanyId, out dbSlaveItem))
				{
					if (dbSlave.Diagnostic.Error || dbSlave.Diagnostic.Warning || dbSlave.Diagnostic.Information)
					{
						diagnostic.Set(dbSlave.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}
					}
				}

				string dmsServerName = dbSlaveItem.ServerName;
				string dmsDataSourceName = dbSlaveItem.DatabaseName;
				string dmsOwnerId = dbSlaveItem.SlaveDBOwner;

				// carico le informazioni dell'utente dbo dello Slave
				SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
				slaveLoginDb.ConnectionString = connectionString;
				slaveLoginDb.CurrentSqlConnection = currentConnection;
				SlaveLoginItem loginItem = new SlaveLoginItem();
				if (!slaveLoginDb.SelectDboOwnerForCompanyId(currentCompany.CompanyId, out loginItem))
				{
					if (slaveLoginDb.Diagnostic.Error || slaveLoginDb.Diagnostic.Warning || slaveLoginDb.Diagnostic.Information)
					{
						diagnostic.Set(slaveLoginDb.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}
					}
				}

				string ownerLogin = Path.GetFileName(loginItem.SlaveDBUser);
				string ownerDomain = loginItem.SlaveDBWinAuth ? Path.GetDirectoryName(loginItem.SlaveDBUser) : string.Empty;
				string[] serverInstance = dmsServerName.Split(Path.DirectorySeparatorChar);
				string ownerPassword = loginItem.SlaveDBPassword;

				string tempDbServer = string.Empty;
				string tempDbIstance = string.Empty;
				if (serverInstance.Length > 1)
					tempDbIstance = serverInstance[1];
				tempDbServer = serverInstance[0];

				SetConsoleProgressBarText(sender, string.Empty);
				DisableConsoleProgressBar(sender);

				UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();

				// se l'utente dbowner e' in sql auth effettuo la solita impersonificazione
				if (!currentUser.DBWindowsAuthentication)
				{
					// richiedo le credenziali di connessione
					dataToConnectionServer = connSqlTransact.LoginImpersonification
						(
						ownerLogin,
						ownerPassword,
						ownerDomain,
						loginItem.SlaveDBWinAuth,
						tempDbServer,
						tempDbIstance,
						false
						);

					if (dataToConnectionServer == null)
					{
						SetConsoleProgressBarText(sender, string.Empty);
						DisableConsoleProgressBar(sender);
						Cursor.Current = Cursors.Default;
					}
					else
						dmsDbDeleted = true;
				}
				else
				{
					// se l'utente dbowner e' in win auth effettuo impersonificazione silente
					// senza richiedere le credenziali di connessione (accedo con l'utente DmsSqlUser)
					dataToConnectionServer = connSqlTransact.LoginImpersonification
						(
						ownerLogin,
						ownerPassword,
						ownerDomain,
						loginItem.SlaveDBWinAuth,
						true
						);

					if (dataToConnectionServer == null)
					{
						SetConsoleProgressBarText(sender, string.Empty);
						DisableConsoleProgressBar(sender);
						Cursor.Current = Cursors.Default;
					}
					else
						dmsDbDeleted = true;
				}

				SetConsoleProgressBarText(sender, Strings.ProgressWaiting);
				EnableConsoleProgressBar(sender);

				// se sono riuscita a connettermi procedo ad eliminare il database
				if (dmsDbDeleted)
				{
					dmsDbDeleted = connSqlTransact.DeleteDataBase
						(
						dmsServerName,
						dmsDataSourceName,
						loginItem.SlaveDBWinAuth ? Path.Combine(dataToConnectionServer.Domain, dataToConnectionServer.Login) : loginItem.SlaveDBUser,
						loginItem.SlaveDBWinAuth ? dataToConnectionServer.Password : loginItem.SlaveDBPassword,
						loginItem.SlaveDBWinAuth
						);

					if (!dmsDbDeleted)
						diagnostic.Set(connSqlTransact.Diagnostic);

					if (dataToConnectionServer != null)
						dataToConnectionServer.Undo();
				}

				SetConsoleProgressBarText(sender, string.Empty);
				DisableConsoleProgressBar(sender);
			}

			connSqlTransact = null;

			bool companyDelete = false;
			// se cancello anche il database aziendale elimino tutte le aziende che puntano a lui
			if (dc.DeleteCompanyDB && moreCompaniesToDelete.Count > 0)
			{
				// In ogni caso cancello tutte le aziende che puntavano al database appena eliminato
				for (int i = 0; i < moreCompaniesToDelete.Count; i++)
				{
					companyDelete = companyDb.Delete(((CompanyItem)moreCompaniesToDelete[i]).CompanyId);
					// se non sono riuscita ad eliminare la prima azienda non procedo
					if (!companyDelete)
					{
						if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning)
						{
							diagnostic.Set(companyDb.Diagnostic);
							break;
						}
					}
				}
			}
			else
			{
				//cancello tutta l'anagrafica azienda
				companyDelete = companyDb.Delete(currentCompany.CompanyId);

				if (!companyDelete)
				{
					if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning)
						diagnostic.Set(companyDb.Diagnostic);
				}
			}

			if (!companyDelete)
				diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.DeleteDatabase);

			// se ci sono dei msg nel diagnostico li mostro
			if (diagnostic.Warning || diagnostic.Error)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}

			//Disabilito la progressBar
			SetConsoleProgressBarText(sender, string.Empty);
			DisableConsoleProgressBar(sender);

			//se ho cancellato l'azienda, devo ridisegnare la porzione di tree
			//interessata e mandare un evento ai plugIns interessati
			if (companyDelete)
			{
				if (OnAfterDeleteCompany != null)
					OnAfterDeleteCompany(sender, companyId);
				if (OnModifyTree != null)
					OnModifyTree(sender, ConstString.containerCompanies);
			}

			Cursor.Current = Cursors.Default;
			return companyDelete;
		}

		///<summary>
		/// Evento richiamato dalla form DeleteCompany per visualizzare o meno la checkbox
		/// di cancellazione del database documentale (viene proposta solo se il modulo e' attivato)
		///</summary>
		//---------------------------------------------------------------------
		private bool deleteCompany_OnIsActivated(string application, string functionality)
		{
			if (OnIsActivated != null && OnIsActivated(application, functionality))
				return true;

			return false;
		}

		/// <summary>
		/// PER SQL SERVER
		/// Svuota il db di azienda da tutti gli oggetti presenti (tables, views, stored procedures, triggers)
		/// </summary>
		//---------------------------------------------------------------------
		public bool DeleteAllSqlServerObjects(object sender, System.EventArgs e)
		{
			bool companyDbObjectsDeleted = false;
			diagnosticViewer.Message = Strings.DeleteAllObjectsMsg;
			diagnosticViewer.Title = Strings.Delete;
			diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
			diagnosticViewer.ShowIcon = MessageBoxIcon.Question;
			diagnosticViewer.DefaultButton = MessageBoxDefaultButton.Button2;
			DialogResult result = diagnosticViewer.Show();

			if (result == DialogResult.No)
				return companyDbObjectsDeleted;

			SetConsoleProgressBarValue(sender, 3);
			SetConsoleProgressBarStep(sender, 3);
			EnableConsoleProgressBar(sender);
			SetConsoleCyclicStepProgressBar();

			this.State = StateEnums.Processing;
			Cursor.Current = Cursors.WaitCursor;

			//leggo i dati dell'azienda (relativi al suo db)
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = connectionString;
			companyDb.CurrentSqlConnection = currentConnection;

			ArrayList companyObjectsToDelete = new ArrayList();
			if (!companyDb.GetAllCompanyFieldsById(out companyObjectsToDelete, this.companyId))
			{
				diagnostic.Set(companyDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				companyObjectsToDelete.Clear();
			}

			if (companyObjectsToDelete.Count > 0)
			{
				string serverName = ((CompanyItem)companyObjectsToDelete[0]).DbServer;
				string dataSourceName = ((CompanyItem)companyObjectsToDelete[0]).DbName;
				string ownerId = ((CompanyItem)companyObjectsToDelete[0]).DbOwner;

				CompanyUserDb userOwnerdb = new CompanyUserDb();
				userOwnerdb.ConnectionString = connectionString;
				userOwnerdb.CurrentSqlConnection = currentConnection;
				ArrayList userData = new ArrayList();
				userOwnerdb.GetUserCompany(out userData, ownerId, this.companyId);

				TransactSQLAccess connSqlTransact = new TransactSQLAccess();
				connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
				connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(SendHelp);
				connSqlTransact.OnSetCyclicStepProgressBar += new TransactSQLAccess.SetCyclicStepProgressBar(SetConsoleCyclicStepProgressBar);
				connSqlTransact.OnSetProgressBarText += new TransactSQLAccess.SetProgressBarText(SetConsoleProgressBarText);
				connSqlTransact.CurrentStringConnection = connectionString;
				connSqlTransact.CurrentConnection = currentConnection;

				if (((CompanyUser)userData[0]).DBWindowsAuthentication)
				{
					UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
					string ownerLogin = Path.GetFileName(((CompanyUser)userData[0]).DBDefaultUser);
					string ownerDomain = Path.GetDirectoryName(((CompanyUser)userData[0]).DBDefaultUser);
					string[] serverIstance = serverName.Split(Path.DirectorySeparatorChar);
					string tempDbServer = string.Empty;
					string tempDbIstance = string.Empty;
					string ownerPassword = ((CompanyUser)userData[0]).DBDefaultPassword;
					//si tratta di una istanza
					if (serverIstance.Length > 1)
						tempDbIstance = serverIstance[1];
					//è una installazione primaria, l'istanza è vuota
					tempDbServer = serverIstance[0];

					dataToConnectionServer = connSqlTransact.LoginImpersonification
						(
							ownerLogin,
							ownerPassword,
							ownerDomain,
							true,
							tempDbServer,
							tempDbIstance,
							false
						);
					if (dataToConnectionServer == null)
						return companyDbObjectsDeleted;

					//svuoto il db (cancello tutti gli oggetti del tipo tabella, sp, triggers e indici
					companyDbObjectsDeleted =
						connSqlTransact.DeleteDatabaseObjects
						(
							serverName,
							dataSourceName,
							Path.Combine(dataToConnectionServer.Domain, dataToConnectionServer.Login),
							dataToConnectionServer.Password,
							true
						);

					DisableConsoleProgressBar(sender);
					Cursor.Current = Cursors.Default;

					if (companyDbObjectsDeleted)
					{
						diagnostic.Set(DiagnosticType.Information, Strings.DBObjectsDeleted);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					}
					else
					{
						if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning || connSqlTransact.Diagnostic.Information)
							diagnostic.Set(connSqlTransact.Diagnostic);
						else
							diagnostic.Set(DiagnosticType.Warning, DatabaseItemsStrings.DeleteDatabase);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					}
					if (dataToConnectionServer != null)
						dataToConnectionServer.Undo();
				}
				//sono in sql autentication
				else
				{
					//svuoto il db (cancello tutti gli oggetti del tipo tabella, sp, triggers e indici
					companyDbObjectsDeleted =
						connSqlTransact.DeleteDatabaseObjects
						(
							serverName,
							dataSourceName,
							((CompanyUser)userData[0]).DBDefaultUser,
							((CompanyUser)userData[0]).DBDefaultPassword,
							false
						);

					DisableConsoleProgressBar(sender);
					Cursor.Current = Cursors.Default;

					if (companyDbObjectsDeleted)
					{
						diagnostic.Set(DiagnosticType.Information, Strings.DBObjectsDeleted);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					}
					else
					{
						if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning || connSqlTransact.Diagnostic.Information)
							diagnostic.Set(connSqlTransact.Diagnostic);
						else
							diagnostic.Set(DiagnosticType.Warning, DatabaseItemsStrings.DeleteDatabase);

						DiagnosticViewer.ShowDiagnostic(diagnostic);
					}
				}
			}

			this.State = StateEnums.View;

			return companyDbObjectsDeleted;
		}
		#endregion

		#region Funzioni di send della diagnostica
		//---------------------------------------------------------------------
		private void Company_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		//---------------------------------------------------------------------
		private void Company_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		//---------------------------------------------------------------------
		private void Company_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}
		#endregion

		#region BuildTabDatabaseSettings - aggancio le tab con gli usercontrol per tipo provider
		///<summary>
		/// BuildTabDatabaseSettings
		/// a seconda dei provider censiti nel database di sistema aggancio gli user control per
		/// l'inserimento dei dati dei diversi database (da caricare nella tab 'Database')
		///</summary>
		//---------------------------------------------------------------------
		public void BuildTabDatabaseSettings()
		{
			if (cbProvider.SelectedItem == null)
				return;

			DBCompanySettings.Controls.Clear();
			ProviderItem selectedProvider = (ProviderItem)cbProvider.SelectedItem;

			if (OnEnableSaveButton != null)
				OnEnableSaveButton(this, new EventArgs());

			BtnSave.Enabled = true;
			BtnDelete.Enabled = !inserting;

			switch (selectedProvider.ProviderValue)
			{
				case NameSolverDatabaseStrings.SQLODBCProvider:
				case NameSolverDatabaseStrings.SQLOLEDBProvider:
					{
						DataSettingsGroupBox.Visible = true;

						if (DBCompanySettings.Controls.Count > 0)
						{
							if (DBCompanySettings.Controls[0] is DataBaseSql)
								return;
							else
								DBCompanySettings.Controls.RemoveAt(0);
						}

						DataBaseSqlLite dbSql = new DataBaseSqlLite(licenceInfo.UseUnicodeSet());
						dbSql.OnCallHelp += new DataBaseSqlLite.CallHelp(SendHelp);
						dbSql.OnSetValueForUnicodeCheckBox += new DataBaseSqlLite.SetValueForUnicodeCheckBox(dbSql_OnSetValueForUnicodeCheckBox);
						dbSql.OnSetValueUsersComboBox += new DataBaseSqlLite.SetValueUsersComboBox(dbSql_OnSetValueUsersComboBox);
						dbSql.Inserting = inserting;

						dbSql.CompanyDbName = tbCompany.Text;

						dbSql.Company = tbCompany.Text;
						dbSql.ServerNameSystemDb = this.ServerNameSystemDb;
						dbSql.ServerIstanceSystemDb = this.ServerIstanceSystemDb;
						dbSql.UserConnected = (UserConnected.Length == 0) ? UserSystemConnected : UserConnected;
						dbSql.UserPwdConnected = this.UserPwdConnected;
						dbSql.CompanyId = this.companyId;
						dbSql.ConnectionString = this.connectionString;
						dbSql.CurrentConnection = this.currentConnection;

						dbSql.LoadData(false);
                        // enable checkbox UseDataSynchronizer for SQL
                        UseDataSynchro.Enabled = true;

                        DBCompanySettings.Controls.Add(dbSql);

						ShowHideUseDBSlaveControls();
						break;
					}
			}
		}
		# endregion

		#region Eventi intercettati sui controls
		/// <summary>
		/// Quando carico la form, setto il focus sul campo nome azienda e visualizzo o meno la tab del documentale (solo se e' attivato)
		/// NB: devo fare qui le chiamate agli eventi (non lo posso fare nel costruttore perche' li' gli eventi non sono ancora agganciati)
		/// </summary>
		//---------------------------------------------------------------------
		private void Company_Load(object sender, System.EventArgs e)
		{
			// chiedo alla MC l'attivazione di alcuni plugins
			if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				isDMSActivated = true;
			if (OnIsActivated != null && OnIsActivated(DatabaseLayerConsts.MicroareaConsole, DatabaseLayerConsts.SecurityAdmin))
				isSecurityActivated = true;
			if (OnIsActivated != null && OnIsActivated(DatabaseLayerConsts.MicroareaConsole, DatabaseLayerConsts.AuditingAdmin))
				isAuditingActivated = true;
			if (OnIsActivated != null && OnIsActivated(DatabaseLayerConsts.MicroareaConsole, DatabaseLayerConsts.RowSecurityToolKit))
				isRowSecurityActivated = true;
            if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.DataSynchroFunctionality))
                isDataSynchroActivated = true;

            if (string.IsNullOrWhiteSpace(tbCompany.Text))
				tbCompany.Focus();

			// abilito o meno le opzioni a seconda dell'attivazione del plugin
			UseSecurity.Enabled = isSecurityActivated;
			UseAuditing.Enabled = isAuditingActivated;
			UseRowSecurityCBox.Visible = isRowSecurityActivated;
            UseDataSynchro.Visible = isDataSynchroActivated;

            // il provider non puo' mai essere modificato
            cbProvider.Enabled = false;

			ShowHideUseDBSlaveControls();
		}

		/// <summary>
		/// Sul leave della text box dove indico il nome dell'azienda, vado ad aggiornare
		/// la proposizione del nome del database aziendale e documentale
		/// </summary>
		//---------------------------------------------------------------------
		private void tbCompany_Leave(object sender, System.EventArgs e)
		{
			if (tabControl1.TabPages.Contains(DBCompanySettings))
			{
				if (DBCompanySettings.Controls.Count > 0)
				{
					if (DBCompanySettings.Controls[0] is DataBaseSqlLite)
					{
                        DataBaseSqlLite dbSql = (DataBaseSqlLite)DBCompanySettings.Controls[0];
						if (dbSql.Inserting)
							dbSql.CompanyDbName = ((TextBox)sender).Text;
					}
				}
			}

			if (tabControl1.TabPages.Contains(DBSlave))
			{
				// carico le info dello usercontrol del Easy Attachment
				if (SlavePanel.Controls.Count > 0)
				{
					if (SlavePanel.Controls[0] is DataBaseSqlLite)
					{
                        DataBaseSqlLite dmsDb = (DataBaseSqlLite)SlavePanel.Controls[0];
						if (dmsDb.Inserting)
							dmsDb.CompanyDbName = ((TextBox)sender).Text + DatabaseLayerConsts.DMSSignature;
					}
				}
			}
		}

		//---------------------------------------------------------------------
		private void tbCompany_Validated(object sender, System.EventArgs e)
		{
			tbCompany_Leave(sender, e);
		}

		//---------------------------------------------------------------------
		private void tbDescrizione_TextChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void cbProvider_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
			BuildTabDatabaseSettings();
		}

		//---------------------------------------------------------------------
		private void cbProvider_DropDown(object sender, System.EventArgs e)
		{
			if (cbProvider.Items.Count > 0)
			{
				string selectedProvider = ((ComboBox)sender).Text;
				PopolateComboProvider(selectedProvider);
			}
			else
				PopolateComboProvider(string.Empty);
		}

		/// <summary>
		/// evento sparato dagli user control DatabaseSql, in modo da settare correttamente il valore
		/// della checkbox "Use unicode character set" nella tab. Parameters, ma solo se sono in modifica
		/// </summary>
		/// <param name="setRO">true: non editabile</param>
		/// <param name="setChecked">true: set value a true</param>
		//---------------------------------------------------------------------
		private void dbSql_OnSetValueForUnicodeCheckBox(bool setRO, bool setChecked)
		{
			cbUseUnicode.Enabled = !setRO;
			cbUseUnicode.Checked = setChecked;
		}

		///<summary>
		/// Evento sparato dallo usercontrol DatabaseSql presente nella tab con le informazioni del
		/// database aziendale (NON quello documentale), dopo aver selezionato  l'utente applicativo
		/// nell'apposita combobox. Tale evento arriva SOLO in inserimento di una nuova azienda.
		/// L'utente applicativo scelto che diventera' dbo del database aziendale, viene impostato
		/// in automatico anche per il database documentale
		///</summary>
		//---------------------------------------------------------------------
		private void dbSql_OnSetValueUsersComboBox(string selectedUser)
		{
			if (SlavePanel == null || SlavePanel.Controls == null || SlavePanel.Controls.Count == 0)
				return;

            DataBaseSqlLite dmsSql = (DataBaseSqlLite)SlavePanel.Controls[0];
			if (dmsSql == null)
				return;

			dmsSql.ForceSelectedUser(selectedUser);
		}

		//---------------------------------------------------------------------
		private void UseRowSecurityCBox_CheckedChanged(object sender, EventArgs e)
		{
			State = StateEnums.Editing;
		}
        //---------------------------------------------------------------------
        private void UseDataSynchro_CheckedChanged(object sender, EventArgs e)
        {
            State = StateEnums.Editing;
        }
        //---------------------------------------------------------------------
        private void UseSecurity_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void UseAuditing_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void CbCompanyDisabled_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void rbPosition_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void rbKey_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void cbUseUnicode_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void cbUseTransaction_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}
		#endregion

		# region Gestione help
		//---------------------------------------------------------------------
		private void SendHelp(object sender, string nameSpace, string searchParameter)
		{
			if (this.OnCallHelp != null)
				OnCallHelp(sender, nameSpace, searchParameter);
		}
		# endregion

		# region Gestione show/hide tabPage database secondari
		//---------------------------------------------------------------------
		private void HideTabPage(TabPage tp)
		{
			if (tabControl1.TabPages.Contains(tp))
				tabControl1.TabPages.Remove(tp);
		}

		//---------------------------------------------------------------------
		private void ShowTabPage(TabPage tp)
		{
			ShowTabPage(tp, tabControl1.TabPages.Count);
		}

		//---------------------------------------------------------------------
		private void ShowTabPage(TabPage tp, int index)
		{
			if (tabControl1.TabPages.Contains(tp))
				return;
			InsertTabPage(tp, index);
		}

		//---------------------------------------------------------------------
		private void InsertTabPage(TabPage tabpage, int index)
		{
			if (index < 0 || index > tabControl1.TabCount)
				return; //throw new ArgumentException("Index out of Range.");

			tabControl1.TabPages.Add(tabpage);

			if (index < tabControl1.TabCount - 1)
			{
				do
				{
					SwapTabPages(tabpage, (tabControl1.TabPages[tabControl1.TabPages.IndexOf(tabpage) - 1]));
				}
				while (tabControl1.TabPages.IndexOf(tabpage) != index);
			}

			// non rendo attiva la tab
			//tabControl1.SelectedTab = tabpage;
		}

		//---------------------------------------------------------------------
		private void SwapTabPages(TabPage tp1, TabPage tp2)
		{
			if (!tabControl1.TabPages.Contains(tp1) || !tabControl1.TabPages.Contains(tp2))
				return; //throw new ArgumentException("TabPages must be in the TabControls TabPageCollection.");

			int index1 = tabControl1.TabPages.IndexOf(tp1);
			int index2 = tabControl1.TabPages.IndexOf(tp2);
			tabControl1.TabPages[index1] = tp2;
			tabControl1.TabPages[index2] = tp1;
		}
		#endregion

		#region Gestione database documentale
		/// <summary>
		/// ManageUsersForDMS
		/// Estraggo dal db di sistema tutti gli utenti associati all'azienda appena salvata e, per ognuno,
		/// vado ad aggiornare il server SQL, inserendo la login a livello di server e l'utente a livello di db.
		/// E' necessario effettuare l'operazione perchè dando la possibilità in ogni momento di cambiare il 
		/// server sql a cui punta l'azienda, bisogna aggiungere gli utenti 
		/// </summary>
		//---------------------------------------------------------------------
		private bool ManageUsersForDMS(TransactSQLAccess connSqlTransact, string databaseName)
		{
			if (this.currentConnection == null)
				return false;

			bool result = true;

			try
			{
				if (this.currentConnection.State != ConnectionState.Open)
					this.currentConnection.Open();

				// carico l'elenco degli utenti applicativi associati all'azienda (con le loro info di login)
				CompanyUserDb user = new CompanyUserDb(this.connectionString);
				user.CurrentSqlConnection = this.currentConnection;
				ArrayList usersList = null;
				user.SelectAll(out usersList, tbCompanyId.Text);

				// leggo il record associato alla company nella tabella MSD_CompanyDBSlaves, per avere lo slaveId
				CompanyDBSlave companyDBSlave = new CompanyDBSlave();
				companyDBSlave.ConnectionString = this.connectionString;
				companyDBSlave.CurrentSqlConnection = this.currentConnection;
				CompanyDBSlaveItem dbSlaveItem;
				if (!companyDBSlave.SelectSlaveForCompanyIdAndSignature(this.companyId, DatabaseLayerConsts.DMSSignature, out dbSlaveItem) ||
					dbSlaveItem == null)
					return false;

				// istanzio la classe per inserire/aggiornare il record nella MSD_SlaveLogins
				SlaveLoginDb slaveLogin = new SlaveLoginDb();
				slaveLogin.CurrentSqlConnection = this.currentConnection;
				slaveLogin.ConnectionString = this.connectionString;

				foreach (CompanyUser cu in usersList)
				{
					// leggo le info della login associata all'utente
					string loginId = cu.LoginId;
					string login = cu.DBDefaultUser;
					string password = cu.DBDefaultPassword;
					bool dbWinAuth = cu.DBWindowsAuthentication;

					// skippo l'utente sa
					if (string.Compare(login, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;

					// se la login risulta dbo del database allora non procedo al suo inserimento, infatti l'utente è già
					// stato assegnato prima quando si è creato fisicamente il database
					if (connSqlTransact.ExistUserAndLoginIntoDb(Consts.DboUser, login, databaseName))
						continue;

					// se la login e' gia' associata al database aggiorno solo le tabelle e vado avanti
					if (connSqlTransact.ExistLoginIntoDb(login, databaseName))
					{
						// se la login non esiste l'aggiungo... altrimenti la modifico(??)
						if (!slaveLogin.ExistLoginForSlaveId(loginId, dbSlaveItem.SlaveId))
							result = slaveLogin.Add(dbSlaveItem.SlaveId, loginId, login, password, dbWinAuth);
						else
							result = slaveLogin.Modify(dbSlaveItem.SlaveId, loginId, login, password, dbWinAuth);

						if (!result)
						{
							if (slaveLogin.Diagnostic.Error || slaveLogin.Diagnostic.Information || slaveLogin.Diagnostic.Warning)
								diagnostic.Set(companyDBSlave.Diagnostic);
						}

						continue;
					}

					//TODOMICHI: se la login non esiste in SQL cosa faccio, ma metto cmq ma poi non riuscira' a vedere il DMS?
					// se sono riuscita ad inserire la login sul server SQL allora procedo a inserirla nella tabella MSD_SlaveLogins
					if (result)
					{
						// se la login non esiste l'aggiungo... altrimenti la modifico(??)
						if (!slaveLogin.ExistLoginForSlaveId(loginId, dbSlaveItem.SlaveId))
							result = slaveLogin.Add(dbSlaveItem.SlaveId, loginId, login, password, dbWinAuth);
						else
							result = slaveLogin.Modify(dbSlaveItem.SlaveId, loginId, login, password, dbWinAuth);

						if (!result)
						{
							if (slaveLogin.Diagnostic.Error || slaveLogin.Diagnostic.Information || slaveLogin.Diagnostic.Warning)
								diagnostic.Set(companyDBSlave.Diagnostic);
						}
					}
					else
						if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Information || connSqlTransact.Diagnostic.Warning)
						diagnostic.Set(connSqlTransact.Diagnostic);
				}
			}
			catch
			{
				result = false;
				Debug.WriteLine("Error in ManageUsersForDMS method");
				DiagnosticViewer.ShowDiagnostic(connSqlTransact.Diagnostic);
				if (this.OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}

			return result;
		}
		
		/// <summary>
		 /// ManageUsersAndLoginsForDMS
		 /// Estraggo dal db di sistema tutti gli utenti associati all'azienda appena salvata e, per ognuno,
		 /// vado ad aggiornare il server SQL, inserendo la login a livello di server e l'utente a livello di db.
		 /// E' necessario effettuare l'operazione perchè dando la possibilità in ogni momento di cambiare il 
		 /// server sql a cui punta l'azienda, bisogna aggiungere gli utenti 
		 /// </summary>
		//---------------------------------------------------------------------
		private bool ManageUsersAndLoginsForDMS(TransactSQLAccess connSqlTransact, string databaseName)
		{
			if (this.currentConnection == null)
				return false;

			bool result = true;

			try
			{
				if (this.currentConnection.State != ConnectionState.Open)
					this.currentConnection.Open();

				// carico l'elenco degli utenti applicativi associati all'azienda (con le loro info di login)
				CompanyUserDb user = new CompanyUserDb(this.connectionString);
				user.CurrentSqlConnection = this.currentConnection;
				ArrayList usersList = null;
				user.SelectAll(out usersList, tbCompanyId.Text);

				// leggo il record associato alla company nella tabella MSD_CompanyDBSlaves, per avere lo slaveId
				CompanyDBSlave companyDBSlave = new CompanyDBSlave();
				companyDBSlave.ConnectionString = this.connectionString;
				companyDBSlave.CurrentSqlConnection = this.currentConnection;
				CompanyDBSlaveItem dbSlaveItem;
				if (!companyDBSlave.SelectSlaveForCompanyIdAndSignature(this.companyId, DatabaseLayerConsts.DMSSignature, out dbSlaveItem) ||
					dbSlaveItem == null)
					return false;

				// istanzio la classe per inserire/aggiornare il record nella MSD_SlaveLogins
				SlaveLoginDb slaveLogin = new SlaveLoginDb();
				slaveLogin.CurrentSqlConnection = this.currentConnection;
				slaveLogin.ConnectionString = this.connectionString;

				foreach (CompanyUser cu in usersList)
				{
					// leggo le info della login associata all'utente
					string loginId = cu.LoginId;
					string login = cu.DBDefaultUser;
					string password = cu.DBDefaultPassword;
					bool dbWinAuth = cu.DBWindowsAuthentication;

					// skippo l'utente sa
					if (string.Compare(login, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;

					// se l'utente associato e' in win auth ed e' anche dbowner del db
					// devo controllare la login DmsSqlUser
/*					if (dbWinAuth && loginId == dbSlaveItem.SlaveDBOwner)
					{
						// se il dbowner sul server SQL e' effettivamente DmsSqlUser continuo
						if (connSqlTransact.ExistUserAndLoginIntoDb
							(
							Microarea.TaskBuilderNet.Data.SQLDataAccess.Consts.DboUser,
							DatabaseLayerConsts.DmsSqlUser,
							databaseName
							))
							continue;
					}
*/

					// se la login risulta dbo del database allora non procedo al suo inserimento, infatti l'utente è già
					// stato assegnato prima quando si è creato fisicamente il database
					if (connSqlTransact.ExistUserAndLoginIntoDb(Microarea.TaskBuilderNet.Data.SQLDataAccess.Consts.DboUser, login, databaseName))
						continue;

					// se la login e' gia' associata al database aggiorno solo le tabelle e vado avanti
					if (connSqlTransact.ExistLoginIntoDb(login, databaseName))
					{
						// se la login non esiste l'aggiungo... altrimenti la modifico(??)
						if (!slaveLogin.ExistLoginForSlaveId(loginId, dbSlaveItem.SlaveId))
							result = slaveLogin.Add(dbSlaveItem.SlaveId, loginId, login, password, dbWinAuth);
						else
							result = slaveLogin.Modify(dbSlaveItem.SlaveId, loginId, login, password, dbWinAuth);

						if (!result)
						{
							if (slaveLogin.Diagnostic.Error || slaveLogin.Diagnostic.Information || slaveLogin.Diagnostic.Warning)
								diagnostic.Set(companyDBSlave.Diagnostic);
						}

						continue;
					}

					// faccio il grant della login sul server SQL
					if (dbWinAuth)
					{
						result =
							connSqlTransact.SPGrantLogin(login) &&
							connSqlTransact.SPGrantDbAccess(login, login, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDataWriter, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDataReader, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDbOwner, databaseName);

						// se e' dbowner devo dare anche i privilegi di amministratore (importante per un'azienda Oracle)
						if (dbSlaveItem.SlaveDBOwner == loginId)
							connSqlTransact.SPAddSrvRoleMember(login, DatabaseLayerConsts.RoleSysAdmin, string.Empty);
					}
					else
					{
						result =
							connSqlTransact.SPAddLogin(login, password, DatabaseLayerConsts.MasterDatabase) &&
							connSqlTransact.SPGrantDbAccess(login, login, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDataWriter, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDataReader, databaseName) &&
							connSqlTransact.SPAddRoleMember(login, DatabaseLayerConsts.RoleDbOwner, databaseName);

						// se e' dbowner devo dare anche i privilegi di amministratore (importante per un'azienda Oracle)
						if (dbSlaveItem.SlaveDBOwner == loginId)
							connSqlTransact.SPAddSrvRoleMember(login, DatabaseLayerConsts.RoleSysAdmin, string.Empty);
					}

					// se sono riuscita ad inserire la login sul server SQL allora procedo a inserirla nella tabella MSD_SlaveLogins
					if (result)
					{
						// se la login non esiste l'aggiungo... altrimenti la modifico(??)
						if (!slaveLogin.ExistLoginForSlaveId(loginId, dbSlaveItem.SlaveId))
							result = slaveLogin.Add(dbSlaveItem.SlaveId, loginId, login, password, dbWinAuth);
						else
							result = slaveLogin.Modify(dbSlaveItem.SlaveId, loginId, login, password, dbWinAuth);

						if (!result)
						{
							if (slaveLogin.Diagnostic.Error || slaveLogin.Diagnostic.Information || slaveLogin.Diagnostic.Warning)
								diagnostic.Set(companyDBSlave.Diagnostic);
						}
					}
					else
						if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Information || connSqlTransact.Diagnostic.Warning)
							diagnostic.Set(connSqlTransact.Diagnostic);
				}
			}
			catch
			{
				result = false;
				Debug.WriteLine("Error in ManageUsersAndLoginsForDMS method");
				DiagnosticViewer.ShowDiagnostic(connSqlTransact.Diagnostic);
				if (this.OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}

			return result;
		}

		///<summary>
		/// DeleteSlave
		/// Metodo che mi consente di eliminare tutti i riferimenti di uno slave dalle tabelle di sistema. 
		/// Con il companyId viene fatta una query sulla tabella MSD_CompanyDBSlaves, cosi' viene identificato
		/// il record da eliminare. 
		/// Se il record e' valido richiamo l'apposita stored procedure che fa pulizia sulle tabelle.
		///</summary>
		//---------------------------------------------------------------------
		private bool DeleteSlave()
		{
			CompanyDBSlave companyDBSlave = new CompanyDBSlave();
			companyDBSlave.ConnectionString = this.connectionString;
			companyDBSlave.CurrentSqlConnection = this.currentConnection;

			CompanyDBSlaveItem dbSlaveItem;
			// devo prima controllare se esiste il record slave per la companyId 
			if (companyDBSlave.SelectSlaveForCompanyId(this.companyId, out dbSlaveItem))
			{
				// significa che non ci sono slave associati all'azienda, quindi ritorno true
				if (dbSlaveItem == null)
					return true;

				// se esiste lo slave associato, richiamo una stored procedure che elimina
				// il record nella tabella MSD_CompanyDBSlaves e le varie login dalla tabella MSD_SlaveLogins
				if (!companyDBSlave.Delete(dbSlaveItem.SlaveId, this.companyId))
				{
					if (companyDBSlave.Diagnostic.Error || companyDBSlave.Diagnostic.Information || companyDBSlave.Diagnostic.Warning)
						diagnostic.Set(companyDBSlave.Diagnostic);
					diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.CompanyDBSlaveModify, dbSlaveItem.SlaveId));
					return false;
				}
			}

			return true;
		}

		///<summary>
		/// Aggiorno le tabelle del database di sistema relative al database documentale
		/// 1. inserisco uno slave nella tabella MSD_CompanyDbSlaves
		/// 2. inserisco la login nella tabella MSD_SlaveLogins
		///</summary>
		//---------------------------------------------------------------------
		private bool UpdatingDMSTables(UserInfo contextUser)
		{
			// inserisco/aggiorno il record nella tabella MSD_CompanyDBSlaves
			CompanyDBSlave companyDBSlave = new CompanyDBSlave();
			companyDBSlave.ConnectionString = this.connectionString;
			companyDBSlave.CurrentSqlConnection = this.currentConnection;

			CompanyDBSlaveItem dbSlaveItem;
			// devo prima controllare se esiste il record 
			if (companyDBSlave.SelectSlaveForCompanyId(this.companyId, out dbSlaveItem))
			{
				if (dbSlaveItem != null)
				{
					// se esiste modifico le informazioni nel record
					if (!companyDBSlave.Modify(dbSlaveItem.SlaveId, this.companyId, contextUser.ServerComplete, contextUser.DatabaseName, contextUser.LoginId))
					{
						if (companyDBSlave.Diagnostic.Error || companyDBSlave.Diagnostic.Information || companyDBSlave.Diagnostic.Warning)
							diagnostic.Set(companyDBSlave.Diagnostic);
						diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.CompanyDBSlaveModify, dbSlaveItem.SlaveId));
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						return false;
					}
				}
				else
				{
					// altrimenti memorizzo la nuova riga con le info dello slave
					if (!companyDBSlave.Add(this.companyId, DatabaseLayerConsts.DMSSignature, contextUser.ServerComplete, contextUser.DatabaseName, contextUser.LoginId))
					{
						if (companyDBSlave.Diagnostic.Error || companyDBSlave.Diagnostic.Information || companyDBSlave.Diagnostic.Warning)
							diagnostic.Set(companyDBSlave.Diagnostic);
						diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.CompanyDBSlaveAdd, this.companyName));
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						return false;
					}

					// rileggo il record appena inserito
					if (!companyDBSlave.SelectSlaveForCompanyIdAndSignature(this.companyId, DatabaseLayerConsts.DMSSignature, out dbSlaveItem))
					{
						if (companyDBSlave.Diagnostic.Error || companyDBSlave.Diagnostic.Information || companyDBSlave.Diagnostic.Warning)
							diagnostic.Set(companyDBSlave.Diagnostic);
						diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyDBSlaveRead);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						return false;
					}
				}
			}

			bool result = false;

			// Aggiorno la MSD_SlaveLogins con le informazioni dell'utente
			SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
			slaveLoginDb.ConnectionString = this.connectionString;
			slaveLoginDb.CurrentSqlConnection = this.currentConnection;

			bool isWinNt = !string.IsNullOrWhiteSpace(contextUser.Domain);

			if (!slaveLoginDb.ExistLoginForSlaveId(contextUser.LoginId, dbSlaveItem.SlaveId))
				result = slaveLoginDb.Add(dbSlaveItem.SlaveId, contextUser.LoginId, contextUser.LoginName, contextUser.LoginPassword, isWinNt);
			else
				result = slaveLoginDb.Modify(dbSlaveItem.SlaveId, contextUser.LoginId, contextUser.LoginName, contextUser.LoginPassword, isWinNt);

			if (!result)
			{
				if (slaveLoginDb.Diagnostic.Error || slaveLoginDb.Diagnostic.Information || slaveLoginDb.Diagnostic.Warning)
					diagnostic.Set(slaveLoginDb.Diagnostic);
				diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.SlaveLoginUpdate);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				return result;
			}

			return result;
		}

		///<summary>
		/// UseDBSlave_CheckedChanged
		/// Visualizzo o nascondo la tabpage con i dati del database documentale
		/// (controllo anche che sia attivato il modulo)
		///</summary>
		//---------------------------------------------------------------------
		private void UseDBSlave_CheckedChanged(object sender, EventArgs e)
		{
			State = StateEnums.Editing;

			if (((CheckBox)sender).Checked && isDMSActivated)
			{
				ShowTabPage(DBSlave);
				BuildTabDBSlave();
			}
			else
				HideTabPage(DBSlave);

			// la label di warning la visualizzo solo se il prodotto non e' attivato
			DMSWarningLabel.Visible = ((CheckBox)sender).Checked && !isDMSActivated;
		}

		///<summary>
		/// BuildTabDBSlave
		/// a seconda dei provider censiti nel database di sistema aggancio gli user control
		/// all'apposito Panel per l'inserimento dei dati degli slave
		///</summary>
		//---------------------------------------------------------------------
		private void BuildTabDBSlave()
		{
			if (cbProvider.SelectedItem != null)
			{
				SlavePanel.Controls.Clear();
				ProviderItem selectedProvider = (ProviderItem)cbProvider.SelectedItem;

				if (SlavePanel.Controls.Count > 0)
				{
					if (SlavePanel.Controls[0] is DataBaseSqlLite)
						return;
					else
						SlavePanel.Controls.RemoveAt(0);
				}

                // aggancio lo user control di SQL per il database documentale
                DataBaseSqlLite dmsSql = new DataBaseSqlLite(licenceInfo.UseUnicodeSet());
				// non imposto l'evento di OnSetValueForUnicodeCheckBox perche' il check di congruenza del db documentale
				// lo faccio sul salvataggio dell'azienda
				dmsSql.OnSetNewDatabaseName += new DataBaseSqlLite.SetNewDatabaseName(dmsSql_OnSetNewDatabaseName);
				dmsSql.EnableUsersComboBox(false); // disabilito la combobox degli utenti applicativi

				dmsSql.Inserting = (companyItem != null) ? !companyItem.UseDBSlave : inserting;

				// il nome del nuovo database documentale di default ha il suffisso DMS
				dmsSql.CompanyDbName = (string.IsNullOrWhiteSpace(this.companyDbName) ? tbCompany.Text : this.companyDbName) + DatabaseLayerConsts.DMSSignature;

				switch (selectedProvider.ProviderValue)
				{
                    case NameSolverDatabaseStrings.SQLODBCProvider:
					case NameSolverDatabaseStrings.SQLOLEDBProvider:
						{
							dmsSql.ServerNameSystemDb = this.ServerNameSystemDb;
							dmsSql.ServerIstanceSystemDb = this.ServerIstanceSystemDb;

							// devo proporre l'utente applicativo scelto per il database aziendale
							if (DBCompanySettings != null && DBCompanySettings.Controls != null && DBCompanySettings.Controls.Count > 0)
							{
								if (DBCompanySettings.Controls[0] is DataBaseSqlLite)
								{
                                    DataBaseSqlLite companyDbSql = (DataBaseSqlLite)DBCompanySettings.Controls[0];

									dmsSql.SelectedSQLServerName = companyDbSql.SelectedSQLServerName;
									dmsSql.SelectedSQLIstanceName = companyDbSql.SelectedSQLIstanceName;
									dmsSql.SelectedDbOwnerId = companyDbSql.SelectedDbOwnerId;
									dmsSql.SelectedDbOwnerName = companyDbSql.SelectedDbOwnerName;
									dmsSql.SelectedDbOwnerPwd = companyDbSql.SelectedDbOwnerPwd;
									dmsSql.SelectedDbOwnerIsWinNT = companyDbSql.SelectedDbOwnerIsWinNT;
									dmsSql.UserConnected = companyDbSql.UserConnected;
									dmsSql.UserPwdConnected = companyDbSql.UserPwdConnected;
								}
							}

							dmsSql.CompanyId = this.companyId;
							dmsSql.ConnectionString = this.connectionString;
							dmsSql.CurrentConnection = this.currentConnection;
							dmsSql.LoadData(true);

							SlavePanel.Controls.Add(dmsSql);
							break;
						}
				}
			}
		}

		///<summary>
		/// Propongo il nome del nuovo database documentale impostando il nome azienda + il suffisso DMS
		///</summary>
		//---------------------------------------------------------------------
		private void dmsSql_OnSetNewDatabaseName()
		{
			if (SlavePanel == null || SlavePanel.Controls == null || SlavePanel.Controls.Count == 0)
				return;

            DataBaseSqlLite dmsSql = (DataBaseSqlLite)SlavePanel.Controls[0];
			if (dmsSql == null)
				return;

			if (string.IsNullOrWhiteSpace(tbCompany.Text) ||
				!tbCompany.Text.EndsWith(DatabaseLayerConsts.DMSSignature, StringComparison.InvariantCultureIgnoreCase))
				dmsSql.CompanyDbName = tbCompany.Text + DatabaseLayerConsts.DMSSignature;
		}

		///<summary>
		/// Gestione visibilita' e stato della checkbox relativa al DBSlave
		///</summary>
		//---------------------------------------------------------------------
		private void ShowHideUseDBSlaveControls()
		{
			if (inserting)
			{
				UseDBSlave.Visible = isDMSActivated;
				DMSWarningLabel.Visible = false;
			}
			else
			{
				// se l'azienda sul db di sistema usa lo slave e il dms non e' attivato visualizzo la label con warning
				DMSWarningLabel.Visible = (companyItem.UseDBSlave && !isDMSActivated);
				// il flag Usa documentale e' visibile se:
				// 1. usa lo slave e il modulo non e' attivato (e visualizzo anche la label con warning)
				// 2. oppure se il modulo e' attivato correttamente
				UseDBSlave.Visible = (companyItem.UseDBSlave && !isDMSActivated) || isDMSActivated;
			}

			if (UseDBSlave.Checked && isDMSActivated)
				ShowTabPage(DBSlave);
			else
				HideTabPage(DBSlave);
		}
		# endregion

		///<summary>
		/// SaveCompanyWithSQLProvider - salvataggio di un'azienda con provider SqlServer
		///</summary>
		//---------------------------------------------------------------------
		private bool SaveCompanyForSQLProvider()
		{
			diagnostic.Clear();

			BtnSave.Enabled = false; //disabilito il bottone di save
			Cursor.Current = Cursors.WaitCursor; //Cursore in stato di attesa

            //Collezione delle info x Sql 
            DataBaseSqlLite dbSql = (DataBaseSqlLite)DBCompanySettings.Controls[0];
			if (dbSql == null)
				return false;

			//--------------------------------------------------------------
			// CONTROLLI DI VALIDAZIONE DEI DATI INSERITI NELLA FORM
			//--------------------------------------------------------------
			// effettuo i primi controlli di validazione per quanto riguarda la tab del db aziendale
			if (!CheckSqlValidator(dbSql))
			{
				BtnSave.Enabled = true;
				Cursor.Current = Cursors.Default;
				return false;
			}

			// effettuo i primi controlli di validazione per quanto riguarda la tab del db DMS (solo se attivato)
			if (isDMSActivated && UseDBSlave.Checked)
			{
				if (!CheckDMSValidator())
				{
					BtnSave.Enabled = true;
					Cursor.Current = Cursors.Default;
					return false;
				}
			}

			//--------------------------------------------------------------
			// MI CONNETTO AL SERVER PER IL DATABASE AZIENDALE E CONTROLLO LE SUE IMPOSTAZIONI 
			//--------------------------------------------------------------
			TransactSQLAccess transactCompanyDb = new TransactSQLAccess();
			UserInfo contextUserCompanyDb = new UserInfo();

			string dbSqlDatabaseName = dbSql.SelectedSQLDatabaseName;

			// mi impersonifico, eseguo la connessione al server, controllo le impostazioni del database aziendale (unicode, collate, etc.)
			// creo anche la login su SQL
			if (!CheckSqlCompanyDbSettings(dbSql, dbSqlDatabaseName, transactCompanyDb, contextUserCompanyDb))
			{
				if (contextUserCompanyDb.Impersonification != null)
					contextUserCompanyDb.Impersonification.Undo();
				BtnSave.Enabled = true;
				Cursor.Current = Cursors.Default;
				return false;
			}

			//--------------------------------------------------------------
			// MI CONNETTO AL SERVER PER IL DATABASE DMS E CONTROLLO LE SUE IMPOSTAZIONI 
			//--------------------------------------------------------------
			TransactSQLAccess transactDMSDb = new TransactSQLAccess();
			UserInfo contextUserDMSDb = new UserInfo();

			if (isDMSActivated && UseDBSlave.Checked)
			{
                // Carico la collezione delle info inserite nello user control
                DataBaseSqlLite dmsSql = (DataBaseSqlLite)SlavePanel.Controls[0];
				if (dmsSql == null)
					return false;

				string dmsDatabaseName = dmsSql.SelectedSQLDatabaseName;

				// mi impersonifico, eseguo la connessione al server, controllo le impostazioni del database DMS (unicode, collate, etc.)
				// creo anche la login su SQL
				if (!CheckDMSDbSettings(dmsSql, dmsDatabaseName, transactDMSDb, contextUserDMSDb))
				{
					if (contextUserDMSDb.Impersonification != null)
						contextUserDMSDb.Impersonification.Undo();
					Cursor.Current = Cursors.Default;
					BtnSave.Enabled = true;
					return false;
				}
			}

			// leggo le lingue impostate nella form
			string preferredLanguage = (cultureUICombo.SelectedItem != null) ? cultureUICombo.SelectedLanguageValue : string.Empty;
			string applicationLanguage = cultureApplicationCombo.ApplicationLanguage;

			//--------------------------------------------------------------
			// GESTIONE DATABASE AZIENDALE
			//--------------------------------------------------------------
			bool isCompanyDbSaved = true;

			// Creo il database aziendale se necessario, altrimenti mi connetto all'esistente
			if (dbSql.IsNewSQLCompany)
			{
				bool useUnicode = false;

				bool resultCreationDB = CreateDatabase(contextUserCompanyDb, false, out useUnicode, dbSql.ShowCreationParameters, false);
				if (!resultCreationDB)
				{
					if (contextUserCompanyDb.Impersonification != null)
						contextUserCompanyDb.Impersonification.Undo();
					if (this.OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					isCompanyDbSaved = false;
				}
				else
				{
					//setto il valore in base a come ho creato il db
					contextUserCompanyDb.UseUnicode = useUnicode;
					if (contextUserCompanyDb.Impersonification != null)
						contextUserCompanyDb.Impersonification.Undo();

					// dalla database culture impostata in fase di creazione db 
					// estrapolo i valori per l'LCID e la colonna SupportColumnCollation da memorizzare sulla msd_companies
					CompatibilitySQLDatabaseCulture(contextUserCompanyDb);

					isCompanyDbSaved = resultCreationDB &&
						UpdatingTables(contextUserCompanyDb, dbSql.IsNewSQLCompany, preferredLanguage, applicationLanguage, true);
				}
			}
			else
			{
				//sono in modifica
				/*string dboOfDataBase = string.Empty;
				if (!transactCompanyDb.CurrentDbo(contextUserCompanyDb.LoginName, contextUserCompanyDb.DatabaseName, out dboOfDataBase) ||
					(string.IsNullOrWhiteSpace(dboOfDataBase)))
				{
					if (!ChangeCompanyDbOwner(transactCompanyDb, contextUserCompanyDb))
						isCompanyDbSaved = false;
					else
					{
						if (!DeleteOldDbo(dboOfDataBase, this.companyId))
						{
							diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotDeleatedOldDbo, dboOfDataBase));
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							if (OnSendDiagnostic != null)
							{
								OnSendDiagnostic(this, diagnostic);
								diagnostic.Clear();
							}
						}
					}
				}*/

				// vado ad aggiornare le varie tabelle di sistema
				isCompanyDbSaved = isCompanyDbSaved &&
					UpdatingTables(contextUserCompanyDb, dbSql.Inserting, preferredLanguage, applicationLanguage, true);
			}

			if (contextUserCompanyDb.Impersonification != null)
				contextUserCompanyDb.Impersonification.Undo();

			// alla fine di tutto, devo rimettere a posto le login di SQL e gli utenti associati al database
			// sul server SQL, perchè potrebbe essere stato cambiato dall'utente.
			//isCompanyDbSaved = isCompanyDbSaved && ManageUsersAndLoginsForCompany(transactCompanyDb, contextUserCompanyDb.DatabaseName);

			//--------------------------------------------------------------
			// GESTIONE DMS
			//--------------------------------------------------------------
			bool isDMSDbSaved = true;

			// se sono riuscita a salvare l'azienda allora procedo con il DMS
			if (isCompanyDbSaved)
			{
				if (isDMSActivated)
				{
					// se l'azienda usa gli slave, allora procedo a salvare le informazioni 
					if (UseDBSlave.Checked)
					{
                        // Carico la collezione delle info inserite nello user control
                        DataBaseSqlLite dmsSql = (DataBaseSqlLite)SlavePanel.Controls[0];
						if (dmsSql == null)
							return false;

						// Creo il database DMS se necessario, altrimenti mi connetto all'esistente
						if (dmsSql.IsNewSQLCompany)
						{
							bool useUnicode;
							bool resultCreationDB = CreateDatabase(contextUserDMSDb, false, out useUnicode, dmsSql.ShowCreationParameters, true);
							if (!resultCreationDB)
							{
								if (contextUserDMSDb.Impersonification != null)
									contextUserDMSDb.Impersonification.Undo();
								if (this.OnSendDiagnostic != null)
								{
									OnSendDiagnostic(this, diagnostic);
									diagnostic.Clear();
								}
								isDMSDbSaved = false;
							}
							else
							{
								// per il DMS non devo impostare ne' il valore Unicode ne' la DatabaseCulture
								// perche' la leggo dall'anagrafica azienda
								if (contextUserDMSDb.Impersonification != null)
									contextUserDMSDb.Impersonification.Undo();

								isDMSDbSaved = resultCreationDB && UpdatingDMSTables(contextUserDMSDb);
							}
						}
						else
						{
							//sono in modifica
							/*string dboOfDataBase = string.Empty;
							if (!transactDMSDb.CurrentDbo(contextUserDMSDb.LoginName, contextUserDMSDb.DatabaseName, out dboOfDataBase) ||
								(dboOfDataBase.Length == 0))
							{
								if (!ChangeCompanyDbOwner(transactDMSDb, contextUserDMSDb))
									isDMSDbSaved = false;
							}*/

							// vado ad aggiornare le varie tabelle di sistema
							isDMSDbSaved = isDMSDbSaved & UpdatingDMSTables(contextUserDMSDb);
						}

						if (contextUserDMSDb.Impersonification != null)
							contextUserDMSDb.Impersonification.Undo();

						// alla fine di tutto, devo rimettere a posto gli utenti associati al database
						// sul server SQL, perchè potrebbe essere stato cambiato dall'utente.
						isDMSDbSaved = isDMSDbSaved && ManageUsersForDMS(transactDMSDb, contextUserDMSDb.DatabaseName);
					}
					else // l'azienda non usa gli slave
					{
						// mi ha tolto il flag e quindi elimino i riferimenti nel db di sistema
						isDMSDbSaved = DeleteSlave();
					}
				}
				else
				{
					// anche se il modulo non e' attivato e mi ha tolto il flag allora elimino i riferimenti nel database di sistema
					// se ha lasciato il flag invece non salvo niente
					if (!UseDBSlave.Checked)
						isDMSDbSaved = DeleteSlave();
				}

				// se il modulo e' attivato
				// se ho scelto di usare il documentale
				// se sono riuscita a salvare tutto il Easy Attachment allora aggiorno il valore di UseDBSlave mettendolo a true
				if (isDMSActivated && UseDBSlave.Checked && isDMSDbSaved)
				{
					CompanyDb coDb = new CompanyDb();
					coDb.ConnectionString = this.connectionString;
					coDb.CurrentSqlConnection = this.currentConnection;
					isDMSDbSaved = coDb.UpdateUseDBSlaveValue(companyId, true);
				}
			}

			if (OnAfterChangedAuditing != null)
				OnAfterChangedAuditing(this, tbCompanyId.Text, UseAuditing.Checked);
			if (OnAfterChangedOSLSecurity != null)
				OnAfterChangedOSLSecurity(this, tbCompanyId.Text, UseSecurity.Checked);
			if (OnAfterChangedCompanyDisable != null)
				OnAfterChangedCompanyDisable(this, tbCompanyId.Text, CbCompanyDisabled.Checked);

			if (isCompanyDbSaved && isDMSDbSaved) // se tutti i salvataggi sono andati a buon fine allora ridisegno il tree e riabilito i pulsanti
			{
				if (OnModifyTreeOfCompanies != null)
					OnModifyTreeOfCompanies(this, ConstString.containerCompanyUsers, tbCompanyId.Text);
				if (OnModifyTree != null)
					OnModifyTree(this, ConstString.containerCompanies);
				if (OnAfterModifyCompany != null)
					OnAfterModifyCompany(this, tbCompanyId.Text);
			}

			return (isCompanyDbSaved && isDMSDbSaved);
		}

		///<summary>
		/// Viene effettuata l'impersonificazione per la connessione al database DMS e controllate
		/// le varie impostazioni del database (check esistenza, unicode, collate) e creato il dbowner sul db
		///</summary>
		//---------------------------------------------------------------------
		private bool CheckDMSDbSettings(DataBaseSqlLite dmsSql, string dmsDatabaseName, TransactSQLAccess transactDMSDb, UserInfo contextUserDMSDb)
		{
			transactDMSDb.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(this.AddUserAuthentication);
			transactDMSDb.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(this.GetUserAuthenticatedPwd);
			transactDMSDb.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(this.IsUserAuthenticated);

			//Creazione del contesto Utente
			contextUserDMSDb.ServerPrimary = dmsSql.SelectedSQLServerName;
			contextUserDMSDb.ServerIstance = dmsSql.SelectedSQLIstanceName;
			contextUserDMSDb.DatabaseName = dmsDatabaseName;
			contextUserDMSDb.LoginId = dmsSql.SelectedDbOwnerId;
			contextUserDMSDb.LoginName = dmsSql.SelectedDbOwnerName;
			contextUserDMSDb.LoginPassword = dmsSql.SelectedDbOwnerPwd;
			contextUserDMSDb.DbType = DBMSType.SQLSERVER;

			if (dmsSql.SelectedDbOwnerIsWinNT)
				contextUserDMSDb.Domain = contextUserDMSDb.LoginName.Split(Path.DirectorySeparatorChar)[0];

			//impersonifico l'utente
			if (contextUserDMSDb.Impersonification == null)
			{
				contextUserDMSDb.Impersonification = new UserImpersonatedData();
				if (contextUserDMSDb.IsWinNT)
				{
					contextUserDMSDb.Impersonification = this.AskCredential(transactDMSDb, contextUserDMSDb, false, false);
					if (contextUserDMSDb.Impersonification == null)
					{
						diagnostic.Set(DiagnosticType.Warning, DatabaseItemsStrings.StopDataBaseCreation);
						return false;
					}
					contextUserDMSDb.LoginPassword = contextUserDMSDb.Impersonification.Password;
				}
				else
				{
					string pwd = OnGetUserAuthenticatedPwdFromConsole(contextUserDMSDb.LoginName, contextUserDMSDb.ServerComplete);
					bool alreadyLogged = OnIsUserAuthenticatedFromConsole(contextUserDMSDb.LoginName, pwd, contextUserDMSDb.ServerComplete);

					if (alreadyLogged)
						contextUserDMSDb.Impersonification.Password = contextUserDMSDb.LoginPassword = pwd;
					else
						contextUserDMSDb.Impersonification.Password = contextUserDMSDb.LoginPassword;

					contextUserDMSDb.Impersonification.Login = contextUserDMSDb.LoginName;
					contextUserDMSDb.Impersonification.Domain = string.Empty;
					contextUserDMSDb.Impersonification.WindowsAuthentication = false;
				}

				//ho impersonificato correttamente prendo la pwd costruisco la stringa di connessione
				transactDMSDb.CurrentStringConnection = BuildStringConnection(contextUserDMSDb);
			}

			// provo a connettermi al database con le credenziali indicate
			if (!TryToConnect(transactDMSDb, contextUserDMSDb))
			{
				DiagnosticViewer.ShowDiagnosticAndClear(transactDMSDb.Diagnostic);
				return false;
			}

			// se l'utente non e' dbowner non procedo
			if (!transactDMSDb.LoginIsDBOwnerRole(contextUserDMSDb.LoginName))
			{
				DiagnosticViewer.ShowErrorTrace(string.Format(Strings.NoPermissionUserLite, contextUserDMSDb.LoginName), "", Strings.Warning);
				return false;
			}

			// Ora tento la connessione devo ciclare fino a che non ho finito
			/*bool ended = false;
			bool pressCancelDuringSettingDbo = false;
			bool pressCancelDuringCheckRoleDbo = false;

			while (!ended)
			{
				if (!TryToConnect(transactDMSDb, contextUserDMSDb))
				{
					// assegno l'utente dbowner al database (associazione 1 a 1)
					int resultAfterSettingDbo = SetCompanyDboUser(transactDMSDb, contextUserDMSDb);
					if (resultAfterSettingDbo == 0 || resultAfterSettingDbo == -1)
					{
						ended = true;
						pressCancelDuringSettingDbo = (resultAfterSettingDbo == -1) ? true : false;
					}
				}
				else
				{
					// Ora verifico che l'utente abbia il Ruolo SysAdmin
					int resultAfterCheckRole = CheckRoleDbOwner(contextUserDMSDb, transactDMSDb);
					if (resultAfterCheckRole == 0 || resultAfterCheckRole == -1)
						ended = true;
					pressCancelDuringCheckRoleDbo = (resultAfterCheckRole == -1) ? true : false;
				}
			}

			if (pressCancelDuringCheckRoleDbo || pressCancelDuringSettingDbo)
				return false;*/

			// devo effettuare dei controlli sulla versione del server utilizzato per il DMS
			using (SqlConnection connection = new SqlConnection(transactDMSDb.CurrentStringConnection))
			{
				try
				{
					connection.Open();

					// se la versione del server per il db DMS non e' diversa da SQL2000/MSDE
					// non procedo (per via di incompatibilita' di tipi base utilizzati)
					SQLServerEdition sqlVersion = TBCheckDatabase.GetSQLServerEdition(connection);
					if (sqlVersion == SQLServerEdition.MSDE2000 || sqlVersion == SQLServerEdition.SqlServer2000)
					{
						diagnostic.Set(DiagnosticType.Warning, Strings.InvalidSQLVersionForDMSDb);
						return false;
					}

					// se i serial numbers non sono validi e il server e' un SQL2012 visualizzo un messaggio di avvertimento 
					// (solo se non e' gia' stato mostrato gia' per il database aziendale)
					if (!licenceInfo.IsSQL2012Allowed && TBCheckDatabase.IsSql2012Edition(connection) && !sql2012MsgDisplayed)
					{
						if (DiagnosticViewer.ShowQuestion(Strings.WarnConnectingToSQL2012Server, Strings.Warning) == DialogResult.No)
							return false;
					}
				}
				catch (SqlException e)
				{
					DiagnosticViewer.ShowWarning(e.Message, Strings.Warning);
					return false;
				}
			}

			//imposto il nome del db
			contextUserDMSDb.DatabaseName = dmsDatabaseName;

			// il database e' nuovo
			if (dmsSql.IsNewSQLCompany)
			{
				if (!CheckIfSqlDbExist(transactDMSDb, contextUserDMSDb))
				{
					diagnostic.Set(DiagnosticType.Warning, DatabaseItemsStrings.StopDataBaseCreation);
					return false;
				}
			}
			else // il database esiste gia'
			{
				contextUserDMSDb.UseUnicode = cbUseUnicode.Checked;
				//setto il valore in base all'anagrafica dell'azienda
				bool isUnicode = false, italianTableName = false;

				//verificare requirements database
				OnCheckDBRequirementsUsed(BuildStringConnection(contextUserDMSDb), DBMSType.SQLSERVER, contextUserDMSDb.UseUnicode, out isUnicode, out italianTableName);

				# region Check valore Unicode
				if (isUnicode != contextUserDMSDb.UseUnicode)
				{
					//errore 
					string dbUnicodeType = (isUnicode) ? Strings.DbIsUnicode : Strings.DbIsNotUnicode;
					string companyUnicodeType = (contextUserDMSDb.UseUnicode) ? Strings.CompanyIsUnicode : Strings.CompanyIsNotUnicode;
					string detail = string.Format(Strings.WrongUnicodeSetting, (contextUserDMSDb.UseUnicode) ? Strings.UnicodeSetting : Strings.NotUnicodeSetting);
					string message = string.Format("{0}{1}.{2}!", dbUnicodeType, companyUnicodeType, detail);
					diagnostic.Set(DiagnosticType.Error, message);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					return false;
				}
				# endregion

				// associazione db esistente ad un'azienda
				// CONTROLLO DI COMPATIBILITA' DELLA DATABASECULTURE
				int checkDb = CompatibilitySQLDatabaseCulture(contextUserDMSDb);

				// se la collate del db è diversa da quella di colonna e inoltre la collate di db è diversa da
				// latina visualizzo un messaggio e non faccio procedere.
				if (checkDb == 1)
				{
					DiagnosticViewer.ShowCustomizeIconMessage(Strings.IncompatibleCollationSetting, Strings.Warning, MessageBoxIcon.Error);
					return false;
				}

				// se la DatabaseCulture impostata sull'anagrafica della company è diversa da quella del database
				// visualizzo un msg all'utente e non procedo. 
				if (checkDb == 2)
				{
					DiagnosticViewer.ShowCustomizeIconMessage(Strings.IncompatibleCollationSettingForDMS, Strings.Warning, MessageBoxIcon.Error);
					return false;
				}
			}

			return true;
		}

		///<summary>
		/// Viene effettuata l'impersonificazione per la connessione al database aziendale e controllate
		/// le varie impostazioni del database (check esistenza, unicode, collate) e creato il dbowner sul db
		///</summary>
		//---------------------------------------------------------------------
		private bool CheckSqlCompanyDbSettings
			(
            DataBaseSqlLite dbSql,
			string dbSqlDatabaseName,
			TransactSQLAccess transactCompanyDb,
			UserInfo contextUserCompanyDb
			)
		{
			transactCompanyDb.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(this.AddUserAuthentication);
			transactCompanyDb.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(this.GetUserAuthenticatedPwd);
			transactCompanyDb.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(this.IsUserAuthenticated);

			contextUserCompanyDb.ServerPrimary = dbSql.SelectedSQLServerName;
			contextUserCompanyDb.ServerIstance = dbSql.SelectedSQLIstanceName;
			contextUserCompanyDb.DatabaseName = dbSqlDatabaseName;
			contextUserCompanyDb.LoginId = dbSql.SelectedDbOwnerId;
			contextUserCompanyDb.LoginName = dbSql.SelectedDbOwnerName;
			contextUserCompanyDb.LoginPassword = dbSql.SelectedDbOwnerPwd;
			contextUserCompanyDb.DbType = DBMSType.SQLSERVER;

			if (dbSql.SelectedDbOwnerIsWinNT)
				contextUserCompanyDb.Domain = contextUserCompanyDb.LoginName.Split(Path.DirectorySeparatorChar)[0];

			//impersonifico l'utente
			if (contextUserCompanyDb.Impersonification == null)
			{
				contextUserCompanyDb.Impersonification = new UserImpersonatedData();
				if (contextUserCompanyDb.IsWinNT)
				{
					contextUserCompanyDb.Impersonification = this.AskCredential(transactCompanyDb, contextUserCompanyDb, false, false);
					if (contextUserCompanyDb.Impersonification == null)
					{
						diagnostic.Set(DiagnosticType.Warning, DatabaseItemsStrings.StopDataBaseCreation);
						return false;
					}
					contextUserCompanyDb.LoginPassword = contextUserCompanyDb.Impersonification.Password;
				}
				else
				{
					string pwd = OnGetUserAuthenticatedPwdFromConsole(contextUserCompanyDb.LoginName, contextUserCompanyDb.ServerComplete);
					bool alreadyLogged = OnIsUserAuthenticatedFromConsole(contextUserCompanyDb.LoginName, pwd, contextUserCompanyDb.ServerComplete);

					if (alreadyLogged)
						contextUserCompanyDb.Impersonification.Password = contextUserCompanyDb.LoginPassword = pwd;
					else
						contextUserCompanyDb.Impersonification.Password = contextUserCompanyDb.LoginPassword;

					contextUserCompanyDb.Impersonification.Login = contextUserCompanyDb.LoginName;
					contextUserCompanyDb.Impersonification.Domain = string.Empty;
					contextUserCompanyDb.Impersonification.WindowsAuthentication = false;
				}

				//ho impersonificato correttamente prendo la pwd costruisco la stringa di connessione
				transactCompanyDb.CurrentStringConnection = BuildStringConnection(contextUserCompanyDb);
			}

			// provo a connettermi al database con le credenziali indicate
			if (!TryToConnect(transactCompanyDb, contextUserCompanyDb))
			{
				DiagnosticViewer.ShowDiagnosticAndClear(transactCompanyDb.Diagnostic);
				return false;
			}

			// se l'utente non e' dbowner non procedo
			if (!transactCompanyDb.LoginIsDBOwnerRole(contextUserCompanyDb.LoginName))
			{
				DiagnosticViewer.ShowErrorTrace(string.Format(Strings.NoPermissionUserLite, contextUserCompanyDb.LoginName), "", Strings.Warning);
				return false;
			}

			//Ora tento la connessione e devo ciclare fino a che non ho finito
			/*bool ended = false;
			bool pressCancelDuringSettingDbo = false;
			bool pressCancelDuringCheckRoleDbo = false;

			while (!ended)
			{
				if (!TryToConnect(transactCompanyDb, contextUserCompanyDb))
				{
					// assegno l'utente dbowner al database (associazione 1 a 1)
					int resultAfterSettingDbo = SetCompanyDboUser(transactCompanyDb, contextUserCompanyDb);
					if (resultAfterSettingDbo == 0 || resultAfterSettingDbo == -1)
					{
						ended = true;
						pressCancelDuringSettingDbo = (resultAfterSettingDbo == -1) ? true : false;
					}
				}
				else
				{
					// Ora verifico che l'utente abbia il Ruolo SysAdmin
					int resultAfterCheckRole = CheckRoleDbOwner(contextUserCompanyDb, transactCompanyDb);
					if (resultAfterCheckRole == 0 || resultAfterCheckRole == -1)
						ended = true;
					pressCancelDuringCheckRoleDbo = (resultAfterCheckRole == -1) ? true : false;
				}
			}

			if (pressCancelDuringCheckRoleDbo || pressCancelDuringSettingDbo)
				return false;*/

			// se i serial number Microarea non sono licenziati per la versione di SQL Server 2012
			// visualizzo un messaggio di avvertimento all'utente
			if (!licenceInfo.IsSQL2012Allowed)
			{
				using (TBConnection myConnection = new TBConnection(transactCompanyDb.CurrentStringConnection, DBMSType.SQLSERVER))
				{
					try
					{
						myConnection.Open();
						if (TBCheckDatabase.IsSql2012Edition(myConnection))
						{
							sql2012MsgDisplayed = true;
							if (DiagnosticViewer.ShowQuestion(Strings.WarnConnectingToSQL2012Server, Strings.Warning) == DialogResult.No)
								return false;
						}
					}
					catch (TBException e)
					{
						DiagnosticViewer.ShowWarning(e.Message, Strings.Warning);
						return false;
					}
				}
			}
			
			// il database e' nuovo
			if (dbSql.IsNewSQLCompany)
			{
				//imposto il nome del db da creare
				contextUserCompanyDb.DatabaseName = dbSqlDatabaseName;

				// controllo che il database specificato non esista gia'
				if (!CheckIfSqlDbExist(transactCompanyDb, contextUserCompanyDb))
				{
					diagnostic.Set(DiagnosticType.Warning, DatabaseItemsStrings.StopDataBaseCreation);
					return false;
				}
			}
			else // il database esiste gia'
			{
				// controllo la size del db (se DBNetworkType == Small non deve essere superiore a 2GB)
				if (InstallationData.CheckDBSize &&
					Functions.IsDBSizeOverMaxLimit(transactCompanyDb.CurrentStringConnection))
				{
					diagnostic.Set(DiagnosticType.Error, Strings.DBSizeError);
					return false;
				}

				contextUserCompanyDb.UseUnicode = cbUseUnicode.Checked;
				//setto il valore in base all'anagrafica dell'azienda
				bool isUnicode = false, italianTableName = false;

				//verificare requirements database
				OnCheckDBRequirementsUsed(BuildStringConnection(contextUserCompanyDb), DBMSType.SQLSERVER, contextUserCompanyDb.UseUnicode, out isUnicode, out italianTableName);

				# region Check valore Unicode
				if (isUnicode != contextUserCompanyDb.UseUnicode)
				{
					//errore 
					string dbUnicodeType = (isUnicode) ? Strings.DbIsUnicode : Strings.DbIsNotUnicode;
					string companyUnicodeType = (contextUserCompanyDb.UseUnicode) ? Strings.CompanyIsUnicode : Strings.CompanyIsNotUnicode;
					string detail = string.Format(Strings.WrongUnicodeSetting, (contextUserCompanyDb.UseUnicode) ? Strings.UnicodeSetting : Strings.NotUnicodeSetting);
					string message = string.Format("{0}{1}.{2}!", dbUnicodeType, companyUnicodeType, detail);
					diagnostic.Set(DiagnosticType.Error, message);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					return false;
				}
				# endregion

				//bool setIsValid = true;
				if (italianTableName)
				{
					// TODO... CAMBIARE IL MSG: L'UTENTE NON HA SCELTA E L'OPERAZIONE NON PUO' ESSERE TERMINATA
					DialogResult res = DiagnosticViewer.ShowQuestion(Strings.WrongItalianDBVersion, Strings.Warning);
					if (res == DialogResult.No)
						return false;
					//else
					//setIsValid = false;
				}

				// associazione db esistente ad un'azienda
				// CONTROLLO DI COMPATIBILITA' DELLA DATABASECULTURE
				int checkDb = CompatibilitySQLDatabaseCulture(contextUserCompanyDb);

				// se la collate del db è diversa da quella di colonna e inoltre la collate di db è diversa da
				// latina visualizzo un messaggio e non faccio procedere.
				if (checkDb == 1)
				{
					DiagnosticViewer.ShowCustomizeIconMessage(Strings.IncompatibleCollationSetting, Strings.Warning, MessageBoxIcon.Error);
					return false;
				}

				// se la DatabaseCulture impostata sull'anagrafica della company è diversa da quella del database
				// chiedo all'utente se vuole sovrascriverla (ma solo se non ha un dms associato)
				if (checkDb == 2)
				{
					if (isDMSActivated && UseDBSlave.Checked)
					{
						DiagnosticViewer.ShowCustomizeIconMessage(Strings.WrongDatabaseCultureSettingWithDMS, Strings.Warning, MessageBoxIcon.Error);
						return false;
					}
					else
					{
						DialogResult res = DiagnosticViewer.ShowQuestion(Strings.WrongDatabaseCultureSetting, Strings.Warning);
						if (res == DialogResult.No)
							return false;
					}
				}
			}

			return true;
		}

		///<summary>
		/// IsValidDatabaseNameForSQL
		/// Verifica che il nome scelto per un database SQL non corrisponda con quelli di sistema o di esempio
		///</summary>
		//---------------------------------------------------------------------
		private bool IsValidDatabaseNameForSQL(string dbToCheck)
		{
			bool isValid = true;

			// collezione di database di sistema o di esempio da skippare
			List<string> dataBaseToSkip = new List<string>();
			dataBaseToSkip.Add("master");
			dataBaseToSkip.Add("model");
			dataBaseToSkip.Add("msdb");
			dataBaseToSkip.Add("tempdb");
			dataBaseToSkip.Add("pubs");
			dataBaseToSkip.Add("Northwind");
			dataBaseToSkip.Add("AdventureWorks");
			dataBaseToSkip.Add("AdventureWorksLT");
			dataBaseToSkip.Add("ReportServer");
			dataBaseToSkip.Add("ReportServerTempDB");

			foreach (string dbToSkip in dataBaseToSkip)
			{
				if (string.Compare(dbToCheck, dbToSkip, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					isValid = false;
					break;
				}
			}

			return isValid;
		}
	}
}