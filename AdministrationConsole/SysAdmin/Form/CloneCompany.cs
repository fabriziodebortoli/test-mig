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
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.SQLDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// CloneCompany
	/// Form utilizzata per clonare una company
	/// </summary>
	//=========================================================================
	public partial class CloneCompany : PlugInsForm
	{
		private SqlConnection sysDbConnection = null;
		private string sysDbConnString = string.Empty;

		private string companyId = string.Empty;
		private string newCompanyId = string.Empty;
		private string dbOwnerLogin = string.Empty;
		private string dbOwnerPassword = string.Empty;
		private bool dbOwnerWinAuth = false;
		private string dbOwnerDomain = string.Empty;
		private string dbOwnerPrimary = string.Empty;
		private string dbOwnerIstance = string.Empty;
		private string dbCompanyName = string.Empty;
		private string dbCompanyServer = string.Empty;

		private int sourceDatabaseCulture = 0;		// valore del DatabaseCulture nell'azienda di origine
		private bool sourceSupportColumnCollation = false;	// valore del SupportColumnCollation nell'azienda di origine
		private bool sourceDbUseUnicode = false;	// valore del flag Unicode nell'azienda di origine
		private bool destinationDbUseUnicode = false;	// valore del flag Unicode impostato dall'utente nell'azienda di destinazione

		private string selectedDbCulture = string.Empty;

		// path dei file di dati e di log del database appena creato
		private string dataFilePath = string.Empty;
		private string logFilePath = string.Empty;

		private LicenceInfo licenceInfo = null;
		private Diagnostic diagnostic = new Diagnostic("SysAdmin.CloneCompany");
		private DiagnosticViewer diagnosticViewer = new DiagnosticViewer();

		public Diagnostic Diagnostic { get { return diagnostic; } }

		#region Eventi e Delegati
		public delegate void ModifyTree(object sender, string nodeType);
		public event ModifyTree OnModifyTree;

		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;

		public delegate void AfterClonedCompany(string companyId);
		public event AfterClonedCompany OnAfterClonedCompany;

		public delegate void EnableProgressBar(object sender);
		public event EnableProgressBar OnEnableProgressBar;

		public delegate void DisableProgressBar(object sender);
		public event DisableProgressBar OnDisableProgressBar;

		public delegate void SetProgressBarText(object sender, string message);
		public event SetProgressBarText OnSetProgressBarText;

		public delegate void SetCyclicStepProgressBar();
		public event SetCyclicStepProgressBar OnSetCyclicStepProgressBar;

		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string serverName);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;

		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;

		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string serverName);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;

		public delegate void CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event CallHelpFromPopUp OnCallHelpFromPopUp;
		#endregion

		#region Costruttore
		//---------------------------------------------------------------------
		public CloneCompany(string connectionString, SqlConnection connection, LicenceInfo licenceInfo, string companyId)
		{
			InitializeComponent();

			this.licenceInfo = licenceInfo;
			this.sysDbConnString = connectionString;
			this.sysDbConnection = connection;
			this.companyId = companyId;

			if (PopolateTextSourceCompany())
			{
				txtNewDataBaseName.ReadOnly = true;
				cbUserOwner.Enabled = false;
				NGSqlServersCombo.Enabled = false;
				RadioDbCompanyClone.Checked = true;
			}
			else
				btnCloneCompany.Enabled = false;
		}
		#endregion

		#region PopolateTextSourceCompany - Imposta il nome dell'Azienda sorgente e carica gli utenti associati
		/// <summary>
		/// PopolateTextSourceCompany
		/// </summary>
		//---------------------------------------------------------------------
		private bool PopolateTextSourceCompany()
		{
			bool existCompany = false;
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = sysDbConnString;
			companyDb.CurrentSqlConnection = sysDbConnection;

			ArrayList companyList = new ArrayList();
			if (!companyDb.GetAllCompanyFieldsById(out companyList, companyId))
			{
				if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
					diagnostic.Set(companyDb.Diagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				companyList.Clear();
			}

			if (companyList.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)companyList[0];
				tbSourceCompany.Text = companyItem.Company;
				LabelTitle.Text = string.Format(LabelTitle.Text, companyItem.Company);
				txtNewDataBaseName.Text = companyItem.DbName;
				sourceDbUseUnicode = companyItem.UseUnicode;
				sourceDatabaseCulture = companyItem.DatabaseCulture;
				sourceSupportColumnCollation = companyItem.SupportColumnCollation;
				// inizializzo la combo con il server dell'azienda di origine
				NGSqlServersCombo.InitDefaultServer(companyItem.DbServer);

				PopolateComboUsers();
				existCompany = true;
			}

			return existCompany;
		}
		#endregion

		#region PopolateComboUsers
		/// <summary>
		/// PopolateComboUsers
		/// Popola la combobox con tutti gli utenti se sono sul server locale o solo quelli di dominio se sono su un server remoto
		/// </summary>
		//---------------------------------------------------------------------
		private void PopolateComboUsers()
		{
			string loginMode = string.Empty;

			string currentServer = System.Net.Dns.GetHostName();

			if (cbUserOwner.Items.Count > 0)
			{
				cbUserOwner.DataSource = null;
				cbUserOwner.Items.Clear();
			}

			ArrayList usersList = new ArrayList();

			if (string.Compare(currentServer, NGSqlServersCombo.ServerName, StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				UserDb userDb = new UserDb();
				userDb.ConnectionString = sysDbConnString;
				userDb.CurrentSqlConnection = sysDbConnection;

				//se sono sul server locale prendo tutti gli utenti
				if (string.Compare(currentServer, NGSqlServersCombo.ServerName, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					if (!userDb.SelectAllUsers(out usersList, false))
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
						return;
					}
				}
				else
				{
					// secondo me non ci passa mai... da togliere?
					if (!userDb.SelectAllUsersExceptLocal(out usersList, NGSqlServersCombo.SelectedSQLServer))
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
						return;
					}
				}
			}
			//mi connetto ad un server remoto
			else
			{
				//prendo tutti gli utenti NT o Microarea, ma non gli utenti locali alla macchina
				//Es. Se mi connetto a TST-DOTNET non posso prendere gli utenti USR-GIUSTINADI\<utente>
				UserDb userDb = new UserDb();
				userDb.ConnectionString = sysDbConnString;
				userDb.CurrentSqlConnection = sysDbConnection;
				if (!userDb.SelectAllUsersExceptLocal(out usersList, currentServer))
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
					return;
				}
			}

			ArrayList listOfUsers = new ArrayList();

			foreach (UserItem currentUser in usersList)
			{
				if (string.Compare(currentUser.Login, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0 ||
					string.Compare(currentUser.Login, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
					continue;
				listOfUsers.Add(currentUser);
			}

			if (listOfUsers.Count > 0)
			{
				cbUserOwner.DataSource = listOfUsers;
				cbUserOwner.DisplayMember = "Login";
				cbUserOwner.ValueMember = "LoginId";

				CompanyDb companyDb = new CompanyDb();
				companyDb.ConnectionString = sysDbConnString;
				companyDb.CurrentSqlConnection = sysDbConnection;

				ArrayList fieldsAzienda = new ArrayList();
				if (!companyDb.GetAllCompanyFieldsById(out fieldsAzienda, companyId))
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
					}
					fieldsAzienda.Clear();
				}

				if (fieldsAzienda.Count > 0)
				{
					CompanyItem companyItem = (CompanyItem)fieldsAzienda[0];
					string tempOwnerId = companyItem.DbOwner;
					for (int j = 0; j < cbUserOwner.Items.Count; j++)
					{
						UserItem myItemUser = (UserItem)cbUserOwner.Items[j];
						if (myItemUser == null)
							continue;
						if (string.Compare(myItemUser.LoginId, tempOwnerId, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							cbUserOwner.SelectedIndex = j;
							break;
						}
					}
				}
			}
		}
		#endregion

		#region btnCloneCompany_Click - Selezionato il bottone di Clonazione dell'Azienda
		/// <summary>
		/// btnCloneCompany_Click
		/// </summary>
		//---------------------------------------------------------------------
		private void btnCloneCompany_Click(object sender, System.EventArgs e)
		{
			if (CheckDataValidation())
			{
				State = StateEnums.Processing;
				btnCloneCompany.Enabled = false;

				//Creo le anagrafiche
				CompanyDb companyDb = new CompanyDb();
				companyDb.ConnectionString = sysDbConnString;
				companyDb.CurrentSqlConnection = sysDbConnection;

				//verifico i dati
				string companySourceId = companyId;
				string companyDestName = tbDestCompany.Text;
				string ownerId = ((UserItem)cbUserOwner.SelectedItem).LoginId;
				string ownerLogin = ((UserItem)cbUserOwner.SelectedItem).Login;
				string ownerPassword = ((UserItem)cbUserOwner.SelectedItem).Password;
				bool ownerWinAuthentication = ((UserItem)cbUserOwner.SelectedItem).WindowsAuthentication;
				string companyDbName = txtNewDataBaseName.Text;
				string companyDbServer = NGSqlServersCombo.SelectedSQLServer;

				//creo il nuovo db 
				if (RadioDbCompanyClone.Checked || RadioDbCompanyEmpty.Checked)
				{
					if (!CreateCompanyDb(companyDbServer, companyDbName, ownerId, ownerLogin, ownerPassword, ownerWinAuthentication))
					{
						btnCloneCompany.Enabled = true;
						State = StateEnums.Editing;
						return;
					}
				}

				//il nuovo db viene popolato con i dati del db sorg
				if (RadioDbCompanyClone.Checked)
				{
					if (!PopolateCompanyDb(companySourceId, companyDbServer, companyDbName, ownerLogin, ownerPassword, ownerWinAuthentication))
					{
						diagnostic.Set(DiagnosticType.Error, Strings.CannotClone);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(sender, diagnostic);
							diagnostic.Clear();
						}

						//disabilito la progressBar
						if (OnSetProgressBarText != null)
							OnSetProgressBarText(sender, string.Empty);
						if (OnDisableProgressBar != null)
							OnDisableProgressBar(sender);
						Cursor.Current = Cursors.Default;
						State = StateEnums.Editing;
						btnCloneCompany.Enabled = true;

						if (OnSetProgressBarText != null)
							OnSetProgressBarText(sender, string.Empty);
						if (OnDisableProgressBar != null)
							OnDisableProgressBar(sender);
						Cursor.Current = Cursors.Default;
						return;
					}
				}
				Application.DoEvents();

				//Esiste già una company archiviata in MSD_Companies con quel nome?
				ArrayList company = new ArrayList();

				if (!companyDb.SelectCompanyByDesc(out company, companyDestName))
				{
					if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
						diagnostic.Set(companyDb.Diagnostic);
					diagnostic.Set(DiagnosticType.Error, Strings.CannotClone);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(sender, diagnostic);
						diagnostic.Clear();
					}
					State = StateEnums.Editing;
					btnCloneCompany.Enabled = true;

					if (OnSetProgressBarText != null)
						OnSetProgressBarText(sender, string.Empty);
					if (OnDisableProgressBar != null)
						OnDisableProgressBar(sender);
					Cursor.Current = Cursors.Default;
					return;
				}

				if (company.Count > 0)
				{
					if (!companyDb.Delete(((CompanyItem)company[0]).CompanyId))
					{
						if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
							diagnostic.Set(companyDb.Diagnostic);
						diagnostic.Set(DiagnosticType.Error, Strings.CannotClone);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(sender, diagnostic);
							diagnostic.Clear();
						}
						State = StateEnums.Editing;
						btnCloneCompany.Enabled = true;

						if (OnSetProgressBarText != null)
							OnSetProgressBarText(sender, string.Empty);
						if (OnDisableProgressBar != null)
							OnDisableProgressBar(sender);
						Cursor.Current = Cursors.Default;
						return;
					}
				}
				Application.DoEvents();

				if (!companyDb.Clone(companySourceId, companyDestName, companyDbServer, companyDbName, ownerId))
				{
					if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
						diagnostic.Set(companyDb.Diagnostic);
					diagnostic.Set(DiagnosticType.Error, Strings.CannotClone);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(sender, diagnostic);
						diagnostic.Clear();
					}
					State = StateEnums.Editing;
					btnCloneCompany.Enabled = true;

					if (OnSetProgressBarText != null)
						OnSetProgressBarText(sender, string.Empty);
					if (OnDisableProgressBar != null)
						OnDisableProgressBar(sender);
					Cursor.Current = Cursors.Default;
					return;
				}

				newCompanyId = companyDb.LastCompanyId().ToString();
				if (newCompanyId == "0")
				{
					if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
						diagnostic.Set(companyDb.Diagnostic);
					diagnostic.Set(DiagnosticType.Error, Strings.CannotClone);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					State = StateEnums.Editing;
					btnCloneCompany.Enabled = true;

					if (OnSetProgressBarText != null)
						OnSetProgressBarText(sender, string.Empty);
					if (OnDisableProgressBar != null)
						OnDisableProgressBar(sender);
					Cursor.Current = Cursors.Default;
					return;
				}
				Application.DoEvents();

				// aggiorno la tabella MSD_Companies per le colonne UseUnicode, DatabaseCulture e SupportColumnCollation
				UpdateCompanyData(newCompanyId);

				if (RadioDbCompanyClone.Checked || RadioDbCompanyEmpty.Checked)
				{
					//Aggiungo gli altri utenti
					AddLoginsToDataBase(newCompanyId, companyDbServer, companyDbName, ownerId, ownerLogin, ownerPassword, ownerWinAuthentication);
					Application.DoEvents();
					//la stored procedure  mi ha copiato tutto - potrebbe esserci anche un utente sa 
					//presente come dbowner nell'azienda sorgente ma che  non ci deve essere nell'azienda
					//di destinazione  (non si può associare un utente sa) quindi se c'è e non è il
					//dbowner lo cancello dall'anagrafica (in sql l'ho skippato)
					CompanyUserDb companyUserSa = new CompanyUserDb();
					companyUserSa.ConnectionString = sysDbConnString;
					companyUserSa.CurrentSqlConnection = sysDbConnection;

					ArrayList usersOfCompany = new ArrayList();
					companyUserSa.SelectAll(out usersOfCompany, newCompanyId);
					if (usersOfCompany.Count > 0)
					{
						for (int i = 0; i < usersOfCompany.Count; i++)
						{
							CompanyUser currentUser = (CompanyUser)usersOfCompany[i];
							if (currentUser.DBDefaultUser == DatabaseLayerConsts.LoginSa && currentUser.LoginId != ownerId)
								companyUserSa.Delete(currentUser.LoginId, newCompanyId);

							if (currentUser.LoginId == ownerId && currentUser.Disabled)
							{
								UserListItem currentUserItem = new UserListItem();
								currentUserItem.CompanyId = currentUser.CompanyId;
								currentUserItem.LoginId = currentUser.LoginId;
								currentUserItem.DbUser = currentUser.DBDefaultUser;
								currentUserItem.DbPassword = currentUser.DBDefaultPassword;
								currentUserItem.DbWindowsAuthentication = currentUser.DBWindowsAuthentication;
								currentUserItem.IsAdmin = currentUser.Admin;
								currentUserItem.EasyBuilderDeveloper = currentUser.EasyBuilderDeveloper;
								currentUserItem.Disabled = false;

								if (!companyUserSa.Modify(currentUserItem))
								{
									if (companyUserSa.Diagnostic.Error || companyUserSa.Diagnostic.Warning || companyUserSa.Diagnostic.Information)
										diagnostic.Set(companyUserSa.Diagnostic);
									diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.UserModify, currentUser.Login));
								}
							}
						}

						if (diagnostic.Information || diagnostic.Error || diagnostic.Warning)
						{
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							if (OnSendDiagnostic != null)
							{
								OnSendDiagnostic(this, diagnostic);
								diagnostic.Clear();
							}
							diagnostic.Clear();
						}
					}
				}

				if (OnAfterClonedCompany != null)
					OnAfterClonedCompany(newCompanyId);

				Application.DoEvents();

				//disabilito la progressBar
				if (OnSetProgressBarText != null)
					OnSetProgressBarText(sender, string.Empty);
				if (OnDisableProgressBar != null)
					OnDisableProgressBar(sender);
				Cursor.Current = Cursors.Default;

				State = StateEnums.View;
				btnCloneCompany.Enabled = true;

				if (OnModifyTree != null)
					OnModifyTree(this, ConstString.containerCompanies);
			}
		}
		#endregion

		# region UpdateCompanyData
		///<summary>
		/// UpdateCompanyData
		/// Aggiorno le colonne UseUnicode,DatabaseCulture,SupportColumnCollation nell'azienda appena 
		/// inserita nella MSD_Companies dalla clonazione
		///</summary>
		//---------------------------------------------------------------------
		private void UpdateCompanyData(string newCompanyId)
		{
			string updateQuery = @"UPDATE MSD_Companies 
								SET UseUnicode = @useUnicode, DatabaseCulture = @dbCulture, SupportColumnCollation = @colCollation
								WHERE CompanyId = @newCompanyId";

			SqlConnection myConnection = new SqlConnection();
			SqlCommand myCommand = new SqlCommand(updateQuery);

			try
			{
				myConnection.ConnectionString = sysDbConnString;
				myConnection.Open();

				if (myConnection.State == ConnectionState.Open)
				{
					myCommand.Connection = myConnection;

					// se ho scelto di clonare il db imposto i valori uguali a quelli dell'azienda di origine
					if (RadioDbCompanyClone.Checked)
					{
						myCommand.Parameters.AddWithValue("@useUnicode", sourceDbUseUnicode);
						myCommand.Parameters.AddWithValue("@dbCulture", sourceDatabaseCulture);
						myCommand.Parameters.AddWithValue("@colCollation", sourceSupportColumnCollation);
						myCommand.Parameters.AddWithValue("@newCompanyId", newCompanyId);
					}

					// se ho scelto di creare un'azienda e il db vuoti allora imposto quelli inseriti dall'utente
					if (RadioDbCompanyEmpty.Checked)
					{
						// mi serve per assegnare il valore dell'LCID nell'anagrafica azienda
						// comanda sempre quella impostata sul database
						CultureInfo ci = new CultureInfo(selectedDbCulture);

						string columnCollation = CultureHelper.GetWindowsCollation(ci.LCID);
						string dbCollation = NameSolverDatabaseStrings.SQLLatinCollation;

						bool supportColsCollation = ((columnCollation != dbCollation) &&
						!CultureHelper.IsCollationCompatibleWithCulture(ci.LCID, dbCollation));

						myCommand.Parameters.AddWithValue("@useUnicode", destinationDbUseUnicode);
						myCommand.Parameters.AddWithValue("@dbCulture", ci.LCID);
						myCommand.Parameters.AddWithValue("@colCollation", supportColsCollation);
						myCommand.Parameters.AddWithValue("@newCompanyId", newCompanyId);
					}

					myCommand.ExecuteNonQuery();
				}
			}
			catch (SqlException ex)
			{
				diagnostic.Set(DiagnosticType.Error, string.Concat(Strings.Error, " " , ex.ToString()));
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			}
			finally
			{
				if (myConnection.State != ConnectionState.Open)
				{
					myConnection.Close();
					myConnection.Dispose();
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

		#region CreateCompanyDb - Creo il db dell'azienda
		/// <summary>
		/// CreateCompanyDb
		/// </summary>
		//---------------------------------------------------------------------
		private bool CreateCompanyDb
			(
			string companyDbServer,
			string companyDbName,
			string ownerId,
			string ownerLogin,
			string ownerPassword,
			bool ownerWinAuthentication
			)
		{
			bool resultCreationDB = false;

			string newLogin = string.Empty;
			string newPassword = string.Empty;
			string companyIstance = string.Empty;
			string[] serverIstance = companyDbServer.Split(Path.DirectorySeparatorChar);
			if (serverIstance.Length > 1)
				companyIstance = serverIstance[1];
			string companyServer = serverIstance[0];

			//per l'impersonificazione
			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
			//per creare il db
			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);
			connSqlTransact.OnSetCyclicStepProgressBar += new TransactSQLAccess.SetCyclicStepProgressBar(OnSetCyclicStepProgressBar);
			connSqlTransact.OnSetProgressBarText += new TransactSQLAccess.SetProgressBarText(OnSetProgressBarText);

			//se si tratta di una login NT devo effettuare l'impersonificazione
			if (ownerWinAuthentication)
			{
				//impersonifico
				dataToConnectionServer = connSqlTransact.LoginImpersonification
					(
						Path.GetFileName(ownerLogin),
						ownerPassword,
						Path.GetDirectoryName(ownerLogin),
						ownerWinAuthentication,
						companyServer,
						companyIstance,
						false
					);

				if (dataToConnectionServer == null)
					return resultCreationDB;

				//imposto la stringa di connessione al master
				connSqlTransact.CurrentStringConnection =
					string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, companyDbServer, DatabaseLayerConsts.MasterDatabase);

				newLogin = dataToConnectionServer.Domain + Path.DirectorySeparatorChar + dataToConnectionServer.Login;
			}
			//altrimenti l'utente è SQL, quindi non ho bisogno di effettuare l'impersonificazione
			else
			{
				dataToConnectionServer.Login = ownerLogin;
				dataToConnectionServer.Domain = string.Empty;
				dataToConnectionServer.Password = ownerPassword;
				dataToConnectionServer.WindowsAuthentication = false;
				dataToConnectionServer.UserAfterImpersonate = null;

				//imposto la stringa di connessione al master
				connSqlTransact.CurrentStringConnection =
					string.Format(NameSolverDatabaseStrings.SQLConnection, companyDbServer, DatabaseLayerConsts.MasterDatabase, dataToConnectionServer.Login, dataToConnectionServer.Password);

				newLogin = dataToConnectionServer.Login;
				newPassword = dataToConnectionServer.Password;
			}

			Application.DoEvents();

			//ora eseguo i controlli prima di creare il db
			if (!connSqlTransact.ExistLogin(newLogin))
			{
				diagnosticViewer.Message = string.Format(Strings.WrongUser, companyDbServer, newLogin);
				diagnosticViewer.Title = Strings.Error;
				diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
				diagnosticViewer.ShowIcon = MessageBoxIcon.Warning;
				DialogResult askToDo = diagnosticViewer.Show();
				if (askToDo == DialogResult.Yes)
				{
					//inserisco l'utente
					string oldString = connSqlTransact.CurrentStringConnection;

					//chiedo nuove credenziali utente - di default propongo sa
					UserImpersonatedData oldDataToConnectionServer = dataToConnectionServer;
					dataToConnectionServer = connSqlTransact.LoginImpersonification
						(
							DatabaseLayerConsts.LoginSa,
							string.Empty,
							string.Empty,
						//typeAuthenticationLogin,
							false,
							companyServer,
							companyIstance,
							true
						);
					//se l'impersonificazione non è andata a buoon fine
					if (dataToConnectionServer == null)
					{
						Cursor.Current = Cursors.Default;
						return false;
					}

					connSqlTransact.CurrentStringConnection =
						(dataToConnectionServer.WindowsAuthentication)
						? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, companyDbServer, DatabaseLayerConsts.MasterDatabase)
						: string.Format(NameSolverDatabaseStrings.SQLConnection, companyDbServer, DatabaseLayerConsts.MasterDatabase, dataToConnectionServer.Login, dataToConnectionServer.Password);

					string[] typeOfUser = newLogin.Split(Path.DirectorySeparatorChar);

					Application.DoEvents();

					//se la login effettivamente non esiste
					if (!connSqlTransact.ExistLogin(newLogin))
					{
						bool loginInserted = false;
						if (typeOfUser.Length > 1)
						{
							//dominio + utente ---> NT
							loginInserted = connSqlTransact.SPGrantLogin(newLogin) &&
											connSqlTransact.SPGrantDbAccess(newLogin, newLogin, DatabaseLayerConsts.MasterDatabase) &&
											connSqlTransact.SPAddSrvRoleMember(newLogin, DatabaseLayerConsts.RoleSysAdmin, string.Empty);
						}
						else
						{
							loginInserted = connSqlTransact.SPAddLogin(newLogin, newPassword, DatabaseLayerConsts.MasterDatabase) &&
											connSqlTransact.SPGrantDbAccess(newLogin, newLogin, DatabaseLayerConsts.MasterDatabase) &&
											connSqlTransact.SPAddSrvRoleMember(newLogin, DatabaseLayerConsts.RoleSysAdmin, string.Empty);
						}

						//impossibile inserire, interrompo
						if (!loginInserted)
						{
							if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning || connSqlTransact.Diagnostic.Information)
							{
								diagnostic.Set(connSqlTransact.Diagnostic);
								DiagnosticViewer.ShowDiagnostic(diagnostic);
							}
							//diagnostic.Clear();
							Cursor.Current = Cursors.Default;
							if (dataToConnectionServer != null)
								dataToConnectionServer.Undo();
							return false;
						}
						connSqlTransact.CurrentStringConnection = oldString;
						dataToConnectionServer = oldDataToConnectionServer;
					}
					else
					{
						//l'utente come login esiste ma non posso collegarmi (la password potrebbe essere diversa!)
						diagnosticViewer.Message = string.Format(Strings.WrongPassword, newLogin);
						diagnosticViewer.Title = Strings.Error;
						diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
						diagnosticViewer.ShowIcon = MessageBoxIcon.Error;
						diagnosticViewer.Show();

						Cursor.Current = Cursors.Default;
						if (dataToConnectionServer != null)
							dataToConnectionServer.Undo();
						return false;
					}
				}
				else
				{
					if (dataToConnectionServer != null)
						dataToConnectionServer.Undo();
					return resultCreationDB;
				}
			}

			Application.DoEvents();

			//Se la login esiste ma non ha il ruolo sysadmin, dò un messaggio e interrompo
			if (!connSqlTransact.LoginIsSystemAdminRole(newLogin, DatabaseLayerConsts.RoleSysAdmin))
			{
				string userCompleteName =
					(dataToConnectionServer.Domain.Length == 0)
					? dataToConnectionServer.Login
					: string.Concat(dataToConnectionServer.Domain, Path.DirectorySeparatorChar, dataToConnectionServer.Login);

				DialogResult askIfSetPermissions = MessageBox.Show
					(
						this,
						string.Format(Strings.NoPermissionUser, userCompleteName),
						Strings.Error,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Error
					);

				if (askIfSetPermissions == DialogResult.No)
				{
					if (dataToConnectionServer != null)
						dataToConnectionServer.Undo();
					return resultCreationDB;
				}
				//devo settare le permission
				string oldString = connSqlTransact.CurrentStringConnection;

				//chiedo nuove credenziali utente
				//di default propongo sa
				UserImpersonatedData oldDataToConnectionServer = dataToConnectionServer;
				dataToConnectionServer = connSqlTransact.LoginImpersonification
					(
					DatabaseLayerConsts.LoginSa,
					string.Empty,
					string.Empty,
					//typeAuthenticationLogin,
					false,
					companyServer,
					companyIstance,
					true
					);
				//se l'impersonificazione non è andata a buoon fine
				if (dataToConnectionServer == null)
				{
					Cursor.Current = Cursors.Default;
					return false;
				}

				connSqlTransact.CurrentStringConnection =
					(dataToConnectionServer.WindowsAuthentication)
					? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, companyDbServer, DatabaseLayerConsts.MasterDatabase)
					: string.Format(NameSolverDatabaseStrings.SQLConnection, companyDbServer, DatabaseLayerConsts.MasterDatabase, dataToConnectionServer.Login, dataToConnectionServer.Password);

				if (!connSqlTransact.SPAddSrvRoleMember(newLogin, DatabaseLayerConsts.RoleSysAdmin, string.Empty))
				{
					if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning)
					{
						diagnostic.Set(connSqlTransact.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}
					}
					if (dataToConnectionServer != null)
						dataToConnectionServer.Undo();
					connSqlTransact.CurrentStringConnection = oldString;
					dataToConnectionServer = oldDataToConnectionServer;
					return false;
				}

				connSqlTransact.CurrentStringConnection = oldString;
				dataToConnectionServer = oldDataToConnectionServer;
			}

			if (connSqlTransact.ExistDataBase(companyDbName))
			{
				diagnosticViewer.Message = string.Format(Strings.DataBaseAlreadyExist, companyDbName);
				diagnosticViewer.Title = Strings.Error;
				diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
				diagnosticViewer.ShowIcon = MessageBoxIcon.Error;
				diagnosticViewer.DefaultButton = MessageBoxDefaultButton.Button2;
				DialogResult proceeding = diagnosticViewer.Show();
				if (proceeding == DialogResult.No)
				{
					if (dataToConnectionServer != null)
						dataToConnectionServer.Undo();
					return resultCreationDB;
				}

				if (!connSqlTransact.DeleteDataBase(companyDbServer, companyDbName, newLogin, newPassword, dataToConnectionServer.WindowsAuthentication))
				{
					if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning)
					{
						diagnostic.Set(connSqlTransact.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}
					}
					if (dataToConnectionServer != null)
						dataToConnectionServer.Undo();
					return false;
				}
			}
			else if (connSqlTransact.Diagnostic.Error || connSqlTransact.Diagnostic.Warning)
			{
				diagnostic.Set(connSqlTransact.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				if (dataToConnectionServer != null)
					dataToConnectionServer.Undo();
				return resultCreationDB;
			}

			// inizializzo la combo della Culture con la solita 'scaletta' prevista
			string initDbCulture = DBGenericFunctions.PurposeDatabaseCultureForDBCreation(licenceInfo.IsoState);

			// se sto clonando il database devo impostare i valori della culture sulla base del db di origine
			if (RadioDbCompanyClone.Checked)
			{
				// mi serve per assegnare il valore dell'LCID nell'anagrafica azienda
				// comanda sempre quella impostata sul database
				CultureInfo ci = new CultureInfo(sourceDatabaseCulture);
				initDbCulture = ci.Name; // inizializzo con la Culture letta dal db di origine
			}

			// mostro la nuova form con i parametri per la creazione del database
			CreateDBForm createDBForm = new CreateDBForm
			(
				true,				// isCompanyDB
				companyServer,		// serverName
				companyIstance,		// istanceName
				companyDbName,		// databaseName
				newLogin,			// loginName
				dataToConnectionServer.WindowsAuthentication ? string.Empty : dataToConnectionServer.Password, // password
				dataToConnectionServer.WindowsAuthentication,	// isWinAuth
				true,				// showUnicodeCheck,
				!RadioDbCompanyClone.Checked,	// enableUnicodeCheck,
				RadioDbCompanyClone.Checked ? sourceDbUseUnicode : licenceInfo.UseUnicodeSet(), // initUnicodeCheckValue
				this.licenceInfo.DBNetworkType,	// dbNetworkType
				initDbCulture,					// initDBCulture (only company db)
				false							// disableDBCultureComboBox
			);

			DialogResult dr = createDBForm.ShowDialog();

			// se l'utente ha cliccato su Cancel non procedo
			if (dr == DialogResult.Cancel)
				resultCreationDB = false;
			else
			{
				resultCreationDB = createDBForm.Result;
				destinationDbUseUnicode = createDBForm.UseUnicode;
				selectedDbCulture = createDBForm.DatabaseCulture;

				// compongo il nome fisico dei file di dati e di log del database appena creato
				// che serve poi per il restore
				dataFilePath = Path.Combine(createDBForm.PathDataFile, string.Concat(companyDbName, DatabaseTaskConsts.DataFileSuffix));
				logFilePath = Path.Combine(createDBForm.PathLogFile, string.Concat(companyDbName, DatabaseTaskConsts.LogFileSuffix));
			}

			if (!resultCreationDB)
			{
				if (createDBForm.CreateDBDiagnostic.Error || createDBForm.CreateDBDiagnostic.Warning || createDBForm.CreateDBDiagnostic.Information)
					diagnostic.Set(createDBForm.CreateDBDiagnostic);
				else
					diagnostic.Set(DiagnosticType.Error, Strings.AbortingDBCreation);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}

			if (dataToConnectionServer != null)
				dataToConnectionServer.Undo();

			return resultCreationDB;
		}
		#endregion

		#region PopolateCompanyDb - Popolo il db creato con i dati dell'Azienda Sorgente
		/// <summary>
		/// PopolateCompanyDb
		/// Popolo il db creato con i dati dell'Azienda Sorgente
		/// </summary>
		//---------------------------------------------------------------------------
		private bool PopolateCompanyDb
			(
				string companyIdSource,
				string companyDbServer,
				string companyDbName,
				string ownerLogin,
				string ownerPassword,
				bool ownerWinAuthentication
			)
		{
			bool success = false;
			string companyDbServerSource = string.Empty;
			string companyDbNameSource = string.Empty;
			string companyDbOwnerSource = string.Empty;
			string companyLoginSource = string.Empty;
			string companyPasswordSource = string.Empty;
			bool companyWinAuthSource = false;

			//leggo i dati della company
			CompanyDb companydb = new CompanyDb();
			companydb.ConnectionString = sysDbConnString;
			companydb.CurrentSqlConnection = sysDbConnection;

			ArrayList companyData = new ArrayList();
			if (!companydb.GetAllCompanyFieldsById(out companyData, companyIdSource))
			{
				diagnostic.Set(companydb.Diagnostic);
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
				companyDbServerSource = companyItem.DbServer;
				companyDbNameSource = companyItem.DbName;
				companyDbOwnerSource = companyItem.DbOwner;
				UserDb userdb = new UserDb();
				userdb.ConnectionString = sysDbConnString;
				userdb.CurrentSqlConnection = sysDbConnection;
				ArrayList userData = new ArrayList();
				success = userdb.GetAllUserFieldsById(out userData, companyDbOwnerSource);
				if (!success)
				{
					diagnostic.Set(userdb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
						userData.Clear();
					}
				}
				if (userData.Count > 0)
				{
					UserItem userItem = (UserItem)userData[0];
					companyLoginSource = userItem.Login;
					companyPasswordSource = userItem.Password;
					companyWinAuthSource = userItem.WindowsAuthentication;
				}
				else
					return success;
			}
			else
				return success;

			string companyIstance = string.Empty;
			string[] serverIstance = companyDbServer.Split(Path.DirectorySeparatorChar);
			if (serverIstance.Length > 1)
				companyIstance = serverIstance[1];
			string companyServer = serverIstance[0];

			//per l'impersonificazione
			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
			//per creare il db
			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);
			connSqlTransact.OnEnableProgressBar += new TransactSQLAccess.EnableProgressBar(OnEnableProgressBar);
			connSqlTransact.OnDisableProgressBar += new TransactSQLAccess.DisableProgressBar(OnDisableProgressBar);
			connSqlTransact.OnSetProgressBarText += new TransactSQLAccess.SetProgressBarText(OnSetProgressBarText);
			connSqlTransact.OnSetCyclicStepProgressBar += new TransactSQLAccess.SetCyclicStepProgressBar(OnSetCyclicStepProgressBar);

			//se si tratta di una login NT devo effettuare l'impersonificazione
			if (ownerWinAuthentication)
			{
				//impersonifico
				dataToConnectionServer = connSqlTransact.LoginImpersonification
					(
						Path.GetFileName(ownerLogin),
						ownerPassword,
						Path.GetDirectoryName(ownerLogin),
						ownerWinAuthentication,
						companyServer,
						companyIstance,
						false
					);
				if (dataToConnectionServer == null)
					return success;

				//imposto la stringa di connessione al master
				connSqlTransact.CurrentStringConnection =
					string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, companyDbServer, DatabaseLayerConsts.MasterDatabase);
			}
			//altrimenti l'utente è SQL, quindi non ho bisogno di effettuare l'impersonificazione
			else
			{
				dataToConnectionServer.Login = ownerLogin;
				dataToConnectionServer.Domain = string.Empty;
				dataToConnectionServer.Password = ownerPassword;
				dataToConnectionServer.WindowsAuthentication = false;
				dataToConnectionServer.UserAfterImpersonate = null;
				//imposto la stringa di connessione al master
				connSqlTransact.CurrentStringConnection = string.Format
					(
					NameSolverDatabaseStrings.SQLConnection, 
					companyDbServer,
					DatabaseLayerConsts.MasterDatabase, 
					dataToConnectionServer.Login, 
					dataToConnectionServer.Password
					);
			}

			// eseguo la clonazione del database, eseguendo un backup del database di origine
			// e successivo ripristino di tale backup sul database di destinazione appena creato
			success = CloneDatabase
				(
					companyDbServerSource,
					companyDbNameSource,
					companyLoginSource,
					companyPasswordSource,
					companyWinAuthSource,
					companyDbServer,
					companyDbName,
					ownerLogin,
					ownerPassword,
					ownerWinAuthentication
				);

			if (!success)
			{
				if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
				{
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
				}
			}

			if (dataToConnectionServer != null)
				dataToConnectionServer.Undo();

			return success;
		}

		///<summary>
		/// CloneDatabase
		/// Eseguo la clonazione del database, eseguendo un backup del database di origine
		/// e successivo ripristino di tale backup sul database di destinazione appena creato
		///</summary>
		//---------------------------------------------------------------------
		private bool CloneDatabase
			(
				string sourceServer,
				string sourceDataBase,
				string sourceLogin,
				string sourcePassword,
				bool sourceWinAuthentication,
				string destServer,
				string destDataBase,
				string destLogin,
				string destPassword,
				bool destWinAuthentication
			)
		{
			bool success = false;

			string dbSourceConnString = (sourceWinAuthentication)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, sourceServer, sourceDataBase/* "master"*/)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, sourceServer, /*"master"*/ sourceDataBase, sourceLogin, sourcePassword);

			string dbDestinationConnString = (destWinAuthentication)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, destServer, destDataBase)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, destServer, destDataBase, destLogin, destPassword);

			// Per motivi di policy di Win7 il processo SQLServer non puo' piu' accedere al folder Temp dell'utente corrente
			// quindi mi appoggio alla nostra directory Custom
			string bakTempPath = Path.Combine
				(
				Microarea.TaskBuilderNet.Core.NameSolver.BasePathFinder.BasePathFinderInstance.GetCustomCompanyDataTransferBackupPath(NameSolverStrings.AllCompanies),
				string.Concat(sourceDataBase, "Clonation.bak")
				);
			// se la cartella non esiste la creo al volo
			string bakDirName = Path.GetDirectoryName(bakTempPath);
			if (!Directory.Exists(bakDirName))
				Directory.CreateDirectory(bakDirName);

			DatabaseTask dbTask = new DatabaseTask();
			dbTask.CurrentStringConnection = dbSourceConnString;

			try
			{
				SQLBackupDBParameters bakParams = new SQLBackupDBParameters();
				bakParams.DatabaseName = sourceDataBase;
				bakParams.BackupFilePath = bakTempPath;
				bakParams.Overwrite = true;
				
				// eseguo il backup del database di origine
				success = dbTask.Backup(bakParams);

				if (success)
				{
					// se l'operazione di backup è andata a buon fine, carico dal file .bak le informazioni
					// logiche dei file di dati e di log 
					DataTable filesList = dbTask.LoadFileListOnly(bakTempPath);

					if (filesList != null)
					{
						// inizio a riempire i parametri per il restore
						SQLRestoreDBParameters restoreParams = new SQLRestoreDBParameters();
						restoreParams.DatabaseName = destDataBase;
						restoreParams.RestoreFilePath = bakTempPath;
						restoreParams.ForceRestore = true;

						for (int i = 0; i < filesList.Rows.Count; i++)
						{
							DataRow row = filesList.Rows[i];
							if (row == null)
							{
								success = false;
								break;
							}

							// leggo i nomi dei file logici dal backup
							if (i == 0)
								restoreParams.DataLogicalName = row[0].ToString();
							else
								restoreParams.LogLogicalName = row[0].ToString();
						}

						if (success)
						{
							restoreParams.DataPhysicalName = dataFilePath;
							restoreParams.LogPhysicalName = logFilePath;
							// apro la connessione sul db master sul server di destinazione ed
							// eseguo il restore sul database di destinazione
							dbTask.CurrentStringConnection = dbDestinationConnString;

							success = dbTask.RestoreWithMove(restoreParams);
						}
					}
				}

				if (!success)
				{
					if (dbTask.Diagnostic.Error || dbTask.Diagnostic.Warning || dbTask.Diagnostic.Information)
					{
						diagnostic.Set(dbTask.Diagnostic);
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						if (this.OnSendDiagnostic != null)
						{
							OnSendDiagnostic(this, diagnostic);
							diagnostic.Clear();
						}
					}
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, e.Message);
				extendedInfo.Add(Strings.Number, e.Number);
				extendedInfo.Add(Strings.Function, "CloneDatabase");
				diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyCloned, extendedInfo);
			}

			// in ogni caso elimino il file di backup creato nella Custom (se esiste)
			if (File.Exists(bakTempPath))
				File.Delete(bakTempPath);

			return success;
		}
		#endregion

		#region AddLoginsToDataBase - Aggiunge gli utenti al db aziendale creato
		/// <summary>
		/// AddLoginsToDataBase
		/// Aggiunge gli utenti al db aziendale creato
		/// </summary>
		//---------------------------------------------------------------------
		private void AddLoginsToDataBase
			(
				string companyId,
				string companyDbServer,
				string companyDbName,
				string ownerId,
				string ownerLogin,
				string ownerPassword,
				bool ownerWinAuthentication
			)
		{
			string builtStringConnection = string.Empty;

			//per l'impersonificazione
			UserImpersonatedData dataToConnectionServer = new UserImpersonatedData();
			//per creare il db
			TransactSQLAccess connSqlTransact = new TransactSQLAccess();
			connSqlTransact.NameSpace = "Module.MicroareaConsole.SysAdmin";
			connSqlTransact.OnAddUserAuthenticatedFromConsole += new TransactSQLAccess.AddUserAuthenticatedFromConsole(AddUserAuthentication);
			connSqlTransact.OnGetUserAuthenticatedPwdFromConsole += new TransactSQLAccess.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			connSqlTransact.OnIsUserAuthenticatedFromConsole += new TransactSQLAccess.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
			connSqlTransact.OnCallHelpFromPopUp += new TransactSQLAccess.CallHelpFromPopUp(CallHelp);
			connSqlTransact.OnSetCyclicStepProgressBar += new TransactSQLAccess.SetCyclicStepProgressBar(OnSetCyclicStepProgressBar);
			connSqlTransact.OnSetProgressBarText += new TransactSQLAccess.SetProgressBarText(OnSetProgressBarText);

			//Per accedere al registry del server
			CompanyUserDb companyUserDbo = new CompanyUserDb();
			companyUserDbo.ConnectionString = sysDbConnString;
			companyUserDbo.CurrentSqlConnection = sysDbConnection;
			ArrayList usersOfCompany = new ArrayList();

			//seleziono tutti gli utenti dell'azienda corrente meno il dbowner
			bool result = companyUserDbo.SelectAllExceptSa(out usersOfCompany, companyId);
			if (!result)
			{
				if (companyUserDbo.Diagnostic.Error || companyUserDbo.Diagnostic.Warning || companyUserDbo.Diagnostic.Information)
				{
					diagnostic.Set(companyUserDbo.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (this.OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
				}
				usersOfCompany.Clear();
			}

			// aggiungo il dbowner
			if (companyUserDbo.ExistUser(ownerId, companyId) == 0)
			{
				UserListItem userDbo = new UserListItem();
				userDbo.LoginId = ownerId;
				userDbo.CompanyId = companyId;
				userDbo.Login = ownerLogin;
				userDbo.DbWindowsAuthentication = ownerWinAuthentication;
				userDbo.DbPassword = (ownerWinAuthentication) ? string.Empty : ownerPassword;
				userDbo.DbUser = ownerLogin;
				userDbo.IsAdmin = true;
				result = companyUserDbo.Add(userDbo);
				if (!result)
				{
					string message = string.Format(Strings.CannotCreateCompanyUser, ownerLogin, companyDbName);
					diagnostic.Set(DiagnosticType.Error, message);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					return;
				}
			}

			if (usersOfCompany.Count > 0)
			{
				//costruisco la stringa di connessione all'azienda di destinazione
				builtStringConnection = BuildConnection(newCompanyId);
				if (builtStringConnection.Length > 0)
				{
					connSqlTransact.CurrentStringConnection = builtStringConnection;
					//eventualmente impersonifico l'utente
					dataToConnectionServer = connSqlTransact.LoginImpersonification
						(
						this.dbOwnerLogin,
						this.dbOwnerPassword,
						this.dbOwnerDomain,
						this.dbOwnerWinAuth,
						this.dbOwnerPrimary,
						this.dbOwnerIstance,
						false
						);
					if (dataToConnectionServer == null)
					{
						Cursor.Current = Cursors.Default;
						return;
					}

					//Per ognuno degli utenti dell'azienda sorgente
					for (int i = 0; i < usersOfCompany.Count; i++)
					{
						CompanyUser userToAdd = (CompanyUser)usersOfCompany[i];
						//se è il dbowner (azienda corrente) lo skippo
						if (companyUserDbo.IsDbo(userToAdd.LoginId, companyId))
							continue;
						string dboOfDataBase = string.Empty;
						if (!connSqlTransact.CurrentDbo(userToAdd.DBDefaultUser, txtNewDataBaseName.Text, out dboOfDataBase))
						{
							result = false;
							if (!connSqlTransact.ExistLogin(userToAdd.DBDefaultUser))
							{
								if (string.Compare(userToAdd.DBDefaultUser, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) != 0)
									//se la login non esiste, creo la login e la granto al db
									result = AddLoginToCompany(userToAdd.DBWindowsAuthentication, userToAdd.DBDefaultUser, userToAdd.DBDefaultPassword, connSqlTransact);
							}
							else
								if (!connSqlTransact.ExistLoginIntoDb(userToAdd.DBDefaultUser, txtNewDataBaseName.Text))
								{
									if (string.Compare(userToAdd.DBDefaultUser, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) != 0)
										//se la login esiste ma non è grantata la granto
										result = GrantLoginToCompany(userToAdd.DBWindowsAuthentication, userToAdd.DBDefaultUser, userToAdd.DBDefaultPassword, connSqlTransact);
								}
								else //altrimenti non faccio niente
									result = true;

							if (!result)
							{
								string message = string.Format(Strings.CannotCreateCompanyUser, userToAdd.DBDefaultUser, txtNewDataBaseName.Text);
								diagnostic.Set(DiagnosticType.Error, message);
								DiagnosticViewer.ShowDiagnostic(diagnostic);
								if (OnSendDiagnostic != null)
								{
									OnSendDiagnostic(this, diagnostic);
									diagnostic.Clear();
								}
							}
						}
					}
				}

				if (dataToConnectionServer != null)
					dataToConnectionServer.Undo();
			}
		}
		#endregion

		#region GrantLoginToCompany - Granto una login a un db aziendale e gli associo i ruoli
		/// <summary>
		/// GrantLoginToCompany
		/// </summary>
		//---------------------------------------------------------------------
		private bool GrantLoginToCompany(bool isLoginNT, string loginName, string loginPassword, TransactSQLAccess connSqlTransact)
		{
			if (isLoginNT)
			{
				return (connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName));
			}
			else
			{
				return (connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName));
			}
		}
		#endregion

		#region AddLoginToCompany
		/// <summary>
		/// AddLoginToCompany
		/// </summary>
		//---------------------------------------------------------------------
		private bool AddLoginToCompany(bool isLoginNT, string loginName, string loginPassword, TransactSQLAccess connSqlTransact)
		{
			if (isLoginNT)
			{
				return (connSqlTransact.SPGrantLogin(loginName) &&
						connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName));
			}
			else
			{
				return (connSqlTransact.SPAddLogin(loginName, loginPassword, DatabaseLayerConsts.MasterDatabase) &&
						connSqlTransact.SPGrantDbAccess(loginName, loginName, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataWriter, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDataReader, this.dbCompanyName) &&
						connSqlTransact.SPAddRoleMember(loginName, DatabaseLayerConsts.RoleDbOwner, this.dbCompanyName));
			}
		}
		#endregion

		#region BuildStringConnectionUser - Costruisce la stringa di connessione
		/// <summary>
		/// N.B. sostituire questo metodo ovunque viene costruita la stringa di connessione!!!
		/// in un secondo tempo.
		/// BuildStringConnectionUser - Costruisce la stringa di connessione
		/// </summary>
		/// <returns>la stringa di connessione</returns>
		//---------------------------------------------------------------------
		private string BuildStringConnectionUser(bool toMaster)
		{
			string dataSource = (toMaster) ? DatabaseLayerConsts.MasterDatabase : this.dbCompanyName;

			string connString = (this.dbOwnerWinAuth)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, this.dbCompanyServer, this.dbCompanyName)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, this.dbCompanyServer, this.dbCompanyName, this.dbOwnerLogin, this.dbOwnerPassword);

			return connString;
		}
		#endregion

		#region BuildConnection - Costruisco stringa di connessione
		/// <summary>
		/// BuildConnection
		/// </summary>
		//---------------------------------------------------------------------
		private string BuildConnection(string companyId)
		{
			string connectionToCompanyServer = string.Empty;
			string dbOwnerId = string.Empty;

			//leggo i dati dall'azienda
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = this.sysDbConnString;
			companyDb.CurrentSqlConnection = this.sysDbConnection;

			ArrayList companyData = new ArrayList();
			if (!companyDb.GetAllCompanyFieldsById(out companyData, companyId))
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
				}
				companyData.Clear();
			}

			if (companyData.Count > 0)
			{
				CompanyItem companyItem = (CompanyItem)companyData[0];
				this.dbCompanyServer = companyItem.DbServer;
				this.dbCompanyName = companyItem.DbServer;
				dbOwnerId = companyItem.DbOwner;

				//setto le info generali sul dbName (Istanza primaria o secondaria)
				string[] serverDbInformation = companyItem.DbServer.Split(Path.DirectorySeparatorChar);
				this.dbOwnerPrimary = serverDbInformation[0];
				if (serverDbInformation.Length > 1)
					this.dbOwnerIstance = serverDbInformation[1];
				this.dbCompanyName = companyItem.DbName;

				//Ora leggo le credenziali del dbo dal MSD_CompanyLogins
				CompanyUserDb companyUser = new CompanyUserDb();
				companyUser.ConnectionString = this.sysDbConnString;
				companyUser.CurrentSqlConnection = this.sysDbConnection;
				ArrayList userDboData = new ArrayList();
				companyUser.GetUserCompany(out userDboData, dbOwnerId, companyId);
				if (userDboData.Count > 0)
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
						connectionToCompanyServer =
							string.Format(NameSolverDatabaseStrings.SQLConnection, this.dbCompanyServer, this.dbCompanyName, this.dbOwnerLogin, this.dbOwnerPassword);
				}
			}
			return connectionToCompanyServer;
		}
		#endregion

		#region CheckDataValidation - Verifica la validità dei dati introdutti dall'utente
		/// <summary>
		/// CheckDataValidation
		/// Verifica la validità dei dati introdutti dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckDataValidation()
		{
			diagnostic.Clear();
			bool allIsCorrect = true;
			if (string.Compare(tbDestCompany.Text, tbSourceCompany.Text, true, CultureInfo.InvariantCulture) == 0)
			{
				allIsCorrect = false;
				diagnostic.Set(DiagnosticType.Error, Strings.SameSourceDestCompany);
			}
			if (string.IsNullOrEmpty(tbDestCompany.Text))
			{
				allIsCorrect = false;
				diagnostic.Set(DiagnosticType.Error, Strings.EmptyDestCompanyName);
			}
			if (tbDestCompany.Text.IndexOfAny(Path.GetInvalidPathChars()) != -1 ||
				tbDestCompany.Text.Trim().IndexOfAny(new char[] { '?', '*', Path.DirectorySeparatorChar, '/', '<', '>', ':', '!', '|' }) != -1 ||
				tbDestCompany.Text.Trim().EndsWith("."))
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.WrongCharactersInCompanyName);
				allIsCorrect = false;
			}
			if (string.IsNullOrEmpty(txtNewDataBaseName.Text))
			{
				allIsCorrect = false;
				diagnostic.Set(DiagnosticType.Error, Strings.EmptyNewDataBaseName);
			}
			if (string.IsNullOrEmpty(NGSqlServersCombo.SelectedSQLServer))
			{
				allIsCorrect = false;
				diagnostic.Set(DiagnosticType.Error, Strings.EmptyDataBaseServer);
			}
			if (cbUserOwner.SelectedItem == null)
			{
				allIsCorrect = false;
				diagnostic.Set(DiagnosticType.Error, Strings.EmptyDboName);
			}

			if (!allIsCorrect)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}
			return allIsCorrect;
		}
		#endregion

		#region RadioDbCompanyEmpty_CheckedChanged - Scelto di creare un nuovo db aziendale vuoto
		/// <summary>
		/// RadioDbCompanyEmpty_CheckedChanged
		/// Scelto di creare un nuovo db aziendale vuoto
		/// </summary>
		//---------------------------------------------------------------------
		private void RadioDbCompanyEmpty_CheckedChanged(object sender, System.EventArgs e)
		{
			txtNewDataBaseName.Text = tbDestCompany.Text;
			txtNewDataBaseName.ReadOnly = false;
			NGSqlServersCombo.Enabled = true;
			cbUserOwner.Enabled = true;
			PopolateComboUsers();
		}
		#endregion

		#region RadioDbCompanyClone_CheckedChanged - Scelto di creare un nuovo db aziendale con i dati dell'azienda sorgente
		/// <summary>
		/// RadioDbCompanyClone_CheckedChanged
		/// Scelto di creare un nuovo db aziendale con i dati dell'azienda sorgente
		/// </summary>
		//---------------------------------------------------------------------
		private void RadioDbCompanyClone_CheckedChanged(object sender, System.EventArgs e)
		{
			txtNewDataBaseName.Text = tbDestCompany.Text;
			txtNewDataBaseName.ReadOnly = false;
			NGSqlServersCombo.Enabled = true;
			cbUserOwner.Enabled = true;
			PopolateComboUsers();
		}
		#endregion

		#region tbDestCompany_TextChanged - Introduco il nome per l'azienda di destinazione
		/// <summary>
		/// tbDestCompany_TextChanged
		/// Introduco il nome per l'azienda di destinazione
		/// </summary>
		//---------------------------------------------------------------------
		private void tbDestCompany_TextChanged(object sender, System.EventArgs e)
		{
			txtNewDataBaseName.Text = ((TextBox)sender).Text;
		}
		#endregion

		#region CloneCompany_Load - Al caricamento, focus sul nome dell'azienda di destinazione
		/// <summary>
		/// CloneCompany_Load
		/// Al caricamento, focus sul nome dell'azienda di destinazione
		/// </summary>
		//---------------------------------------------------------------------
		private void CloneCompany_Load(object sender, System.EventArgs e)
		{
			tbDestCompany.Focus();
		}
		#endregion

		/// <summary>
		/// CloneCompany_Closing
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void CloneCompany_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			OnSendDiagnostic?.Invoke(sender, diagnostic);
		}

		/// <summary>
		/// CloneCompany_Deactivate
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void CloneCompany_Deactivate(object sender, System.EventArgs e)
		{
			OnSendDiagnostic?.Invoke(sender, diagnostic);
		}

		/// <summary>
		/// CloneCompany_VisibleChanged
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void CloneCompany_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
				OnSendDiagnostic?.Invoke(sender, diagnostic);
		}

		//---------------------------------------------------------------------
		private void CallHelp(object sender, string nameSpace, string searchParameter)
		{
			OnCallHelpFromPopUp?.Invoke(sender, nameSpace, searchParameter);
		}
	}
}