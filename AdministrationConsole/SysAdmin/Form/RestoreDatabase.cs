using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using Microarea.Console.Core.PlugIns;
using Microarea.Console.Plugin.SysAdmin.UserControls;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// Form che consente di effettuare operazioni di Restore di database
	/// (database aziendale e DMS) dall'Administration Console
	/// </summary>
	//=========================================================================
	public partial class RestoreDatabase : PlugInsForm
	{
		# region Delegates and events
		public delegate void EnableProgressBar(object sender);
		public event EnableProgressBar OnEnableProgressBar;

		public delegate void DisableProgressBar(object sender);
		public event DisableProgressBar OnDisableProgressBar;

		public delegate void SetProgressBarStep(object sender, int step);
		public event SetProgressBarStep OnSetProgressBarStep;

		public delegate void SetProgressBarValue(object sender, int currentValue);
		public event SetProgressBarValue OnSetProgressBarValue;

		public delegate void SetProgressBarText(object sender, string message);
		public event SetProgressBarText OnSetProgressBarText;

		public delegate void SetCyclicStepProgressBar();
		public event SetCyclicStepProgressBar OnSetCyclicStepProgressBar;

		public delegate void EnableConsoleTreeView(bool enable);
		public event EnableConsoleTreeView OnEnableConsoleTreeView;

		public delegate bool CheckRestoredDatabase(string connectionString, DBMSType dbType, bool candidateUnicode, out bool isUnicode, out bool italianTableName);
		public event CheckRestoredDatabase OnCheckRestoredDatabase;

		// Update del tree delle companies (da parte del SysAdmin)
		public delegate void ModifyTree(object sender, string nodeType);
		public event ModifyTree OnModifyTree;

		public delegate void ModifyTreeOfCompanies(object sender, string nodeType, string companyId);
		public event ModifyTreeOfCompanies OnModifyTreeOfCompanies;

		public delegate void AfterModifyCompany(object sender, string companyId);
		public event AfterModifyCompany OnAfterModifyCompany;

		public delegate bool IsActivated(string application, string functionality);
		public event IsActivated OnIsActivated;
		# endregion

		// Oggetto di diagnostica per la form
		public Diagnostic RestoreDBDiagnostic = new Diagnostic("SysAdmin.RestoreDatabase");

		# region Private Variables
		// costanti per i nomi delle colonne del DataGrid con i nomi dei file logici e fisici
		private const string dbName = "DB";
		private const string logicalFileName = "LogicalFile";
		private const string physicalFileName = "PhysicalFile";

		// lista delle informazioni dei database di cui fare il restore
		private IDictionary<BackupDBType, BackupConnectionInfo> DictBackupInfo = new Dictionary<BackupDBType, BackupConnectionInfo>();

		// per accedere alle funzioni di Backup/Restore e connessione attiva
		private DatabaseTask dbTask = new DatabaseTask();

		private string companyId = string.Empty;
		private string isoState = string.Empty;
		private SysAdminStatus sysStatus = null;

		// per tenere traccia delle informazioni sull'azienda estrapolate dal database di sistema
		private CompanyItem companyItem = null;
		private CompanyUser companyUser = null;

		// per popolare il DataGrid dei backups
		private BakCommonObjects backupsDT = new BakCommonObjects();

		//---------- Gestione DMS
		private bool isDMSActivated = false;
		# endregion

		# region Constructor and Load
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="companyId">companyId on selected node</param>
		/// <param name="sysStatus">SysAdminStatus</param>
		/// <param name="isoState">Iso State Code</param>
		//---------------------------------------------------------------------
		public RestoreDatabase(string companyId, SysAdminStatus sysStatus, string isoState)
		{
			InitializeComponent();

			this.companyId = companyId;
			this.sysStatus = sysStatus;
			this.isoState = isoState;
		}

		/// <summary>
		/// Load della form
		/// Se sono riuscita a caricare le informazioni che mi occorrono dal db di sistema allora procedo
		/// nel disegno della form e nell'impostazione dei controls
		/// </summary>
		//---------------------------------------------------------------------
		private void RestoreDatabase_Load(object sender, System.EventArgs e)
		{
			// verifico se il database documentale e' attivato
			// qui e non nel costruttore altrimenti l'evento non e' ancora stato agganciato
			if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				isDMSActivated = true;

			BackupInfoGridUserCtrl.AfterBackupFileChanged += new System.EventHandler(BackupInfoGrid_AfterBackupFileChanged);

			LoadInformation();
		}

		///<summary>
		/// Evento ruotato dal Datagrid ogni volta che si modifica il path del backup
		///</summary>
		//---------------------------------------------------------------------
		private void BackupInfoGrid_AfterBackupFileChanged(object sender, EventArgs e)
		{
			bool atLeastOneSelected = false;
			foreach (DataGridViewRow dgvr in BackupInfoGridUserCtrl.BakDataGridView_Control.Rows)
			{
				// se la riga e' selezionata
				if ((bool)dgvr.Cells[DataTableConsts.Selected].Value)
				{
					if (!string.IsNullOrWhiteSpace(dgvr.Cells[DataTableConsts.BakPath].Value.ToString()))
					{
						atLeastOneSelected = true;
						break;
					}
				}
			}

			// abilito il pulsante solo se c'e' almeno una riga selezionata
			this.LoadBackupInfoButton.Enabled = atLeastOneSelected;

			// abilito il pulsante solo se c'e' almeno una riga selezionata
			this.FileListDataGridView.Rows.Clear();
			this.FileListDataGridView.Enabled = false;
		}
		# endregion

		# region LoadInformation
		/// <summary>
		/// LoadInformation
		/// Carico le informazioni per stabilire una connessione valida all'azienda selezionata
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadInformation()
		{
			if (!LoadCompanyData())
			{
				RestoreDBDiagnostic.Set(DiagnosticType.Error, Strings.UnableToPerformRestore);
				return;
			}

			SetControlsValue();

			LoadInfoInDataGrid();

			if (!DesignMode)
				DefineFileListDataGridStyle();
		}

		/// <summary>
		/// imposto la visibilità ed i testi dei controlli discriminando sul tipo attività
		/// e carico dal database le informazioni che mi occorrono (richiamata nella Load della form)
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			ShowDetailsButton.Enabled = false;
			OKButton.Enabled = false;

			this.LoadBackupInfoButton.Enabled = false;
			this.FileListDataGridView.Enabled = false;
		}

		///<summary>
		/// Carico le informazioni dei backup nel DataGrid
		///</summary>
		//---------------------------------------------------------------------
		private void LoadInfoInDataGrid()
		{
			foreach (KeyValuePair<BackupDBType, BackupConnectionInfo> bak in DictBackupInfo)
			{
				DataRow dRow = backupsDT.NewRow();

				dRow[DataTableConsts.BackupDBType] = bak.Value.BackupDBType;
				dRow[DataTableConsts.Selected] = false;
				dRow[DataTableConsts.DBName] = bak.Value.DbName;
				dRow[DataTableConsts.BakPath] = bak.Value.BackupFilePath;
				dRow[DataTableConsts.Browse] = "...";
				dRow[DataTableConsts.BackupConnectionInfo] = bak.Value;

				backupsDT.Rows.Add(dRow);
			}

			backupsDT.AcceptChanges();

			// assegno il datatable al datasource
			BackupInfoGridUserCtrl.BakDataGridView_DataSource = backupsDT;
		}

		///<summary>
		/// Definizione dello stile del DataGrid contenente i nomi dei file logici e fisici 
		/// letti dentro il file di backup
		///</summary>
		//---------------------------------------------------------------------
		private void DefineFileListDataGridStyle()
		{
			// Immagine per il tipo di database
			DataGridViewImageColumn backupDBTypeColumn = new DataGridViewImageColumn();
			backupDBTypeColumn.Name = DataTableConsts.BackupDBType;
			backupDBTypeColumn.DataPropertyName = DataTableConsts.BackupDBType;
			backupDBTypeColumn.HeaderText = string.Empty;
			backupDBTypeColumn.Width = 25;
			backupDBTypeColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			backupDBTypeColumn.ReadOnly = true;

			Assembly plugInsAssembly = typeof(PlugIn).Assembly;
			Image image = Image.FromStream(plugInsAssembly.GetManifestResourceStream(DatabaseLayerConsts.NamespacePlugInsImg + ".MagoNet16.png"));
			if (image != null)
				backupDBTypeColumn.DefaultCellStyle.NullValue = image;

			FileListDataGridView.Columns.Add(backupDBTypeColumn);

			// colonna Nome database
			DataGridViewTextBoxColumn dbNameColumn = new DataGridViewTextBoxColumn();
			dbNameColumn.Name = dbName;
			dbNameColumn.DataPropertyName = dbName;
			dbNameColumn.HeaderText = Strings.Database;
			dbNameColumn.Width = 250;
			dbNameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			dbNameColumn.ReadOnly = true;
			dbNameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			FileListDataGridView.Columns.Add(dbNameColumn);

			// colonna nome file logico
			DataGridViewTextBoxColumn logicalFileNameColumn = new DataGridViewTextBoxColumn();
			logicalFileNameColumn.Name = logicalFileName;
			logicalFileNameColumn.DataPropertyName = logicalFileName;
			logicalFileNameColumn.HeaderText = Strings.LogicalFileNameCol;
			logicalFileNameColumn.Width = 250;
			logicalFileNameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			logicalFileNameColumn.ReadOnly = false;
			logicalFileNameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			FileListDataGridView.Columns.Add(logicalFileNameColumn);

			// colonna nome file fisico
			DataGridViewTextBoxColumn physicalFileNameColumn = new DataGridViewTextBoxColumn();
			physicalFileNameColumn.Name = physicalFileName;
			physicalFileNameColumn.DataPropertyName = physicalFileName;
			physicalFileNameColumn.HeaderText = Strings.PhysicalFileNameCol;
			physicalFileNameColumn.Width = 320;
			physicalFileNameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			physicalFileNameColumn.ReadOnly = false;
			physicalFileNameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			FileListDataGridView.Columns.Add(physicalFileNameColumn);
		}
		# endregion

		# region LoadFileListOnly e riempimento datagridview con i nomi fisici e logici letti dal db
		///	<summary>
		/// Esecuzione dell'istruzione RESTORE FILELISTONLY per caricare i nomi dei file logici e fisici del backup
		/// Sul click del pulsante Load carico le informazioni nel datagrid
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadBackupInfoButton_Click(object sender, System.EventArgs e)
		{
			FileListDataGridView.Enabled = true;

			// elimino tutte le righe dal datagrid
			FileListDataGridView.Rows.Clear();

			foreach (DataGridViewRow dgvr in BackupInfoGridUserCtrl.BakDataGridView_Control.Rows)
			{
				// se la riga e' selezionata
				if ((bool)dgvr.Cells[DataTableConsts.Selected].Value)
				{
					// se esiste un path indicato e il file bak esiste
					if (!string.IsNullOrWhiteSpace(dgvr.Cells[DataTableConsts.BakPath].Value.ToString()) &&
						File.Exists(dgvr.Cells[DataTableConsts.BakPath].Value.ToString()))
					{
						DataRowView row = (DataRowView)dgvr.DataBoundItem;
						if (row == null)
							continue;

						BackupConnectionInfo bci = row[DataTableConsts.BackupConnectionInfo] as BackupConnectionInfo;
						if (bci != null)
							LoadFileListOnly(bci);
					}
				}
			}
		}

		///<summary>
		/// Carico le info contenute nel file di backup
		///</summary>
		//---------------------------------------------------------------------
		private void LoadFileListOnly(BackupConnectionInfo bci)
		{
			DatabaseTask myDbTask = new DatabaseTask();
			myDbTask.CurrentStringConnection = bci.ConnectionString;

			// carico i nomi dei file logici e fisici del backup
			DataTable filesList = myDbTask.LoadFileListOnly(bci.BackupFilePath);

			if (filesList == null)
			{
				RestoreDBDiagnostic.Set(myDbTask.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(RestoreDBDiagnostic);
				myDbTask.Diagnostic.Clear();
				return;
			}

			// abilito i pulsanti
			FileListDataGridView.Enabled = true;
			this.OKButton.Enabled = true;

			Assembly plugInsAssembly = typeof(PlugIn).Assembly;

			// carico le informazioni nel secondo datagrid
			foreach (DataRow row in filesList.Rows)
			{
				DataGridViewRow dgvr = new DataGridViewRow();
				if (bci.BackupDBType == BackupDBType.ERP)
				{
					DataGridViewCell imageCell = new DataGridViewImageCell();
					imageCell.Value = Image.FromStream(plugInsAssembly.GetManifestResourceStream(DatabaseLayerConsts.NamespacePlugInsImg + ".MagoNet16.png"));
					dgvr.Cells.Add(imageCell);
				}
				if (bci.BackupDBType == BackupDBType.DMS)
				{
					DataGridViewCell imageCell = new DataGridViewImageCell();
					imageCell.Value = Image.FromStream(plugInsAssembly.GetManifestResourceStream(DatabaseLayerConsts.NamespacePlugInsImg + ".EasyAttachment16.png"));
					dgvr.Cells.Add(imageCell);
				}

				DataGridViewCell dbNameCell = new DataGridViewTextBoxCell();
				dbNameCell.Value = bci.DbName;
				dgvr.Cells.Add(dbNameCell);

				DataGridViewCell logicalNameCell = new DataGridViewTextBoxCell();
				logicalNameCell.Value = row[0].ToString();
				dgvr.Cells.Add(logicalNameCell);

				DataGridViewCell physicalNameCell = new DataGridViewTextBoxCell();
				physicalNameCell.Value = row[1].ToString();
				dgvr.Cells.Add(physicalNameCell);

				FileListDataGridView.Rows.Add(dgvr);
			}
		}
		# endregion

		# region LoadCompanyData - dato l'id della company ricerco le sue informazioni dal db di sistema
		/// <summary>
		/// Load dei dati dell'azienda dal database di sistema dato il suo id
		/// </summary>
		//---------------------------------------------------------------------
		private bool LoadCompanyData()
		{
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = sysStatus.ConnectionString;
			companyDb.CurrentSqlConnection = sysStatus.CurrentConnection;

			ArrayList company = new ArrayList();
			if (!companyDb.GetAllCompanyFieldsById(out company, companyId))
			{
				if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
					RestoreDBDiagnostic.Set(companyDb.Diagnostic);
				else
					RestoreDBDiagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				company.Clear();
				return false;
			}

			if (company.Count == 0)
				return false;

			// leggo le info dell'azienda
			companyItem = (CompanyItem)company[0];

			// Leggo le info dell'owner da CompanyLogins per quell'azienda
			CompanyUserDb companyOwner = new CompanyUserDb();
			companyOwner.ConnectionString = sysStatus.ConnectionString;
			companyOwner.CurrentSqlConnection = sysStatus.CurrentConnection;

			ArrayList companyOwnerData = new ArrayList();
			if (!companyOwner.GetUserCompany(out companyOwnerData, companyItem.DbOwner, companyId))
			{
				if (companyOwner.Diagnostic.Error || companyOwner.Diagnostic.Warning || companyOwner.Diagnostic.Information)
					RestoreDBDiagnostic.Set(companyOwner.Diagnostic);
				else
					RestoreDBDiagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				companyOwnerData.Clear();
				return false;
			}

			// leggo le info del dbowner
			companyUser = (CompanyUser)companyOwnerData[0];

			// aggiungo nel dictionary le info del database aziendale (SOLO PER SQL!!!)
			DictBackupInfo.Add(new KeyValuePair<BackupDBType, BackupConnectionInfo>
					(
					BackupDBType.ERP,
					new BackupConnectionInfo
						(
						BackupDBType.ERP,
						companyItem.DbServer,
						companyItem.DbName,
						companyUser.DBDefaultUser,
						companyUser.DBDefaultPassword,
						companyUser.DBWindowsAuthentication
						)
					));

			// se e' attivato anche il modulo DMS e l'azienda gestisce gli slave
			// procedo a leggere le info per la connessione
			if (isDMSActivated && companyItem.UseDBSlave)
			{
				// leggo il record associato alla company nella tabella MSD_CompanyDBSlaves, per avere lo slaveId
				CompanyDBSlave companyDBSlave = new CompanyDBSlave();
				companyDBSlave.ConnectionString = sysStatus.ConnectionString;
				companyDBSlave.CurrentSqlConnection = sysStatus.CurrentConnection;

				CompanyDBSlaveItem dmsSlaveItem = null;

				if (companyDBSlave.SelectSlaveForCompanyIdAndSignature(this.companyId, DatabaseLayerConsts.DMSSignature, out dmsSlaveItem))
				{
					if (dmsSlaveItem != null)
					{
						SlaveLoginDb slaveLoginDb = new SlaveLoginDb();
						slaveLoginDb.ConnectionString = sysStatus.ConnectionString;
						slaveLoginDb.CurrentSqlConnection = sysStatus.CurrentConnection;

						SlaveLoginItem dmsSlaveLoginItem = null;

						// leggo le info per comporre la stringa di connessione al db DMS
						if (slaveLoginDb.SelectAllForSlaveAndLogin(dmsSlaveItem.SlaveId, dmsSlaveItem.SlaveDBOwner, out dmsSlaveLoginItem))
						{
							// aggiungo nel dictionary le info del database DMS
							if (dmsSlaveLoginItem != null)
								DictBackupInfo.Add(new KeyValuePair<BackupDBType, BackupConnectionInfo>
									(
									BackupDBType.DMS,
									new BackupConnectionInfo
										(
										BackupDBType.DMS,
										dmsSlaveItem.ServerName,
										dmsSlaveItem.DatabaseName,
										dmsSlaveLoginItem.SlaveDBUser,
										dmsSlaveLoginItem.SlaveDBPassword,
										dmsSlaveLoginItem.SlaveDBWinAuth
										)
									));
						}
					}
				}
			}

			return true;
		}
		# endregion

		# region CheckCompanyDBVersionAfterRestore
		/// <summary>
		/// controlla il database che ho appena ripristinato:
		/// - se esiste la tabella TB_DBMark
		/// - se usa il set di caratteri Unicode
		/// - se è una versione italiana (ovvero Mago.Net 1.2.5)
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckCompanyDBVersionAfterRestore()
		{
			// compongo la stringa di connessione al database associato all'azienda
			string companyConnectionString =
				(companyUser.DBWindowsAuthentication)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, companyItem.DbServer, companyItem.DbName)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, companyItem.DbServer, companyItem.DbName, companyUser.DBDefaultUser, companyUser.DBDefaultPassword);

			bool isUnicode = false, isItalianVersionDB = false, existTB_DBMark = false;

			// evento per l'ApplicationDBAdmin che si connette al db aziendale e fa gli opportuni controlli
			if (OnCheckRestoredDatabase != null)
				existTB_DBMark = OnCheckRestoredDatabase(companyConnectionString, DBMSType.SQLSERVER, companyItem.UseUnicode, out isUnicode, out isItalianVersionDB);

			// se la TB_DBMark non esiste non procedo
			if (!existTB_DBMark)
			{
				RestoreDBDiagnostic.SetWarning(string.Format(DatabaseLayerStrings.DbNoCompatible, companyItem.DbName));
				return false;
			}
			else
			{
				// modifico l'anagrafica dell'azienda impostando il flag Unicode e il flag IsValid
				CompanyDb companyDb = new CompanyDb();
				companyDb.ConnectionString = sysStatus.ConnectionString;
				companyDb.CurrentSqlConnection = sysStatus.CurrentConnection;

				// se la DatabaseCulture è  uguale a zero si tratta di un restore effettuato su un'azienda
				// prima della 2.5, pertanto devo andare a valorizzare la colonna leggendo direttamente dal database
				if (companyItem.DatabaseCulture == 0)
				{
					companyItem.DatabaseCulture =
						DBGenericFunctions.AssignDatabaseCultureValue(this.isoState, companyConnectionString, DBMSType.SQLSERVER, isUnicode);
				}
				else
				{
					// devo controllare la compatibilità tra la collate presente sul db e quella memorizzata sulla
					// MSD_Companies. Se diverse devo impostare quella del database, e visualizzando un opportuno messaggio
					int dbCultureValue =
						DBGenericFunctions.AssignDatabaseCultureValue(this.isoState, companyConnectionString, DBMSType.SQLSERVER, isUnicode);

					// se l'LCID letto estrapolato dall'isostato e' diverso da 0 e diverso da quello letto da database allora lo assegno
					if (dbCultureValue != 0 && dbCultureValue != companyItem.DatabaseCulture)
					{
						RestoreDBDiagnostic.SetInformation(DatabaseLayerStrings.ModifyDBCultureValue);
						companyItem.DatabaseCulture = dbCultureValue;
					}
				}

				bool supportColumnCollation =
					DBGenericFunctions.CalculateSupportColumnCollation(companyConnectionString, companyItem.DatabaseCulture, DBMSType.SQLSERVER, isUnicode);

				companyDb.Modify
					(
					companyItem.CompanyId, companyItem.Company, companyItem.Description,
					companyItem.ProviderId, companyItem.DbServer, companyItem.DbName, companyItem.DbDefaultUser,
					companyItem.DbDefaultPassword, companyItem.DbOwner, companyItem.UseSecurity, companyItem.UseAuditing,
					companyItem.UseTransaction, companyItem.UseKeyedUpdate, companyItem.DBAuthenticationWindows,
					companyItem.PreferredLanguage, companyItem.ApplicationLanguage, companyItem.Disabled,
					isUnicode, !isItalianVersionDB,
					companyItem.DatabaseCulture,
					supportColumnCollation,
					companyItem.Port,
					companyItem.UseDBSlave,
					companyItem.UseRowSecurity,
					companyItem.UseDataSynchro
					);

				ReloadConsoleTree();

				if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning)
				{
					RestoreDBDiagnostic.Set(companyDb.Diagnostic);
					return false;
				}

				// se il flag 'Utilizza il set di caratteri unicode' impostato sull'anagrafica azienda non corrisponde
				// dalla situazione effettiva presente sul database appena ripristinato, comanda quest'ultimo valore
				// e lo forzo sulla MSD_Companies. Inoltre notifico un messaggio all'utente
				if (isUnicode != companyItem.UseUnicode)
					RestoreDBDiagnostic.SetInformation(DatabaseLayerStrings.ModifyUnicodeValue);

				// se la versione del db è in italiano (ovvero prima della 2.0) visualizzo un opportuno messaggio
				if (isItalianVersionDB)
				{
					RestoreDBDiagnostic.SetWarning(DatabaseLayerStrings.DbItalianVersion);
					return false;
				}
			}

			// ritorna true solo se il db contiene la TB_DBMark, se l'anagrafica azienda è stata modificata senza errori e 
			// se il db non è in versione italiana.
			return true;
		}
		# endregion

		# region Esecuzione procedura (Button OK)
		/// <summary>
		/// Click del pulsante OK
		/// Lancio su un thread separato l'elaborazione
		/// </summary>
		//---------------------------------------------------------------------
		private void OKButton_Click(object sender, System.EventArgs e)
		{
			if (MessageBox.Show
				(
				string.Format(Strings.StartBkpRestoreProcess, Strings.RestoreProcessName),
				Strings.RestoreDataBase,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question
				)
				== DialogResult.No)
				return;

			RestoreDBDiagnostic.Clear();
			
			// controllo i valori inseriti nella form e se ho degli errori/warnings li mostro e mi fermo
			CheckValidator();
			
			if (RestoreDBDiagnostic.Error || RestoreDBDiagnostic.Warning)
			{
				ShowRestoreDiagnostic();
				return;
			}

			StartRestoreProcess();
		}

		///<summary>
		/// Lancio l'elaborazione su un thread separato
		///</summary>
		//---------------------------------------------------------------------
		public Thread StartRestoreProcess()
		{
			Thread myThread = new Thread(new ParameterizedThreadStart(InternalRestoreProcess));

			// serve per evitare l'errore "DragDrop registration did not succeed" riscontrato richiamando
			// la ShowDiagnostic statica per visualizzare i messaggi di un altro thread
			myThread.SetApartmentState(ApartmentState.STA);

			// quando si istanzia un nuovo Thread bisogna assegnargli le CurrentCulture, altrimenti le
			// traduzioni in lingue differenti da quelle del sistema operativo non funzionano!!!
			myThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			myThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			myThread.Start();
			return myThread;
		}

		/// <summary>
		/// Faccio gli opportuni controlli e poi procedo con l'elaborazione
		/// </summary>
		//---------------------------------------------------------------------
		private void InternalRestoreProcess(object sender)
		{
			EnableFormControls(false);

			//Abilito la progressBar
			SetConsoleProgressBarValue(sender, 10);
			SetConsoleProgressBarText(sender, Strings.ProgressWaiting);
			SetConsoleProgressBarStep(sender, 3);
			EnableConsoleProgressBar(sender);
			SetConsoleCyclicStepProgressBar();
			SetConsoleTreeViewEnabled(false);

			//Disabilito il cursore
			Cursor.Current = Cursors.WaitCursor;

			//---------------------------------------------------------------------------------------------------
			// ESECUZIONE DEL RESTORE DA FILE
			//---------------------------------------------------------------------------------------------------
			// faccio un loop su tutte le righe presenti nel DataGrid e solo per quelle selezionate
			// procedo ad eseguire il restore
			foreach (DataGridViewRow dgvr in BackupInfoGridUserCtrl.BakDataGridView_Control.Rows)
			{
				DataRowView row = (DataRowView)dgvr.DataBoundItem;
				if (row == null)
					continue;

				// se la riga non e' selezionata la skippo
				if (!(bool)row[DataTableConsts.Selected])
					continue;

				RunRestoreWithMove(row, sender);
			}

			// per il db DMS devo controllare che i suoi valori corrispondano a quelli del db aziendale,
			// altrimenti devo togliere l'associazione nell'anagrafica azienda
			// N.B. questo controllo devo effettuarlo sempre, indipendentemente dal fatto che ho eseguito o meno
			// il suo restore, infatti potrebbero non essere piu' valide le impostazioni dell'azienda
			BackupConnectionInfo bci = null;
			if (DictBackupInfo.TryGetValue(BackupDBType.DMS, out bci) && bci != null)
			{
				if (!CheckDmsDBVersionAfterRestore(bci))
				{
					// se la cancellazione dello slave e' andata a buon fine ricarico il tree per aggiornare i nodi
					if (DeleteSlave())
					{
						RestoreDBDiagnostic.Set(DiagnosticType.Warning, Strings.DeleteSlaveAfterRestore);
						ReloadConsoleTree();
					}
				}
			}

			//Setto al max la progressBar
			SetConsoleProgressBarValue(sender, 100);

			//Disabilito la progressBar
			SetConsoleProgressBarText(sender, string.Empty);
			DisableConsoleProgressBar(sender);
			SetConsoleTreeViewEnabled(true);
			//Ri-abilito il cursore
			Cursor.Current = Cursors.Default;

			// mostro la diagnostica da parte di altre
			ShowRestoreDiagnostic();

			EnableFormControls(true);
		}

		///<summary>
		/// Esecuzione restore
		///</summary>
		//---------------------------------------------------------------------
		private void RunRestoreWithMove(DataRowView row, object sender)
		{
			string databaseName = row[DataTableConsts.DBName].ToString();
			string bakFilePath = row[DataTableConsts.BakPath].ToString();
			BackupConnectionInfo bci = row[DataTableConsts.BackupConnectionInfo] as BackupConnectionInfo;

			if (bci == null || string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(bakFilePath))
				return;

			string dataLogicalName = string.Empty;
			string dataPhysicalName = string.Empty;
			string logLogicalName = string.Empty;
			string logPhysicalName = string.Empty;
			bool firstRow = true;

			// devo leggere le info dal secondo datagrid
			foreach (DataGridViewRow dgvr in FileListDataGridView.Rows)
			{
				// skippo le righe con nome database diverso
				if (string.Compare(dgvr.Cells[dbName].Value.ToString(), databaseName, StringComparison.InvariantCultureIgnoreCase) != 0)
					continue;

				// la prima riga contiene le informazioni del file di dati
				if (firstRow)
				{
					dataLogicalName = dgvr.Cells[logicalFileName].Value.ToString();
					dataPhysicalName = dgvr.Cells[physicalFileName].Value.ToString();
					firstRow = false;
				}
				else
				{
					// la seconda riga contiene le informazioni del file di log
					logLogicalName = dgvr.Cells[logicalFileName].Value.ToString();
					logPhysicalName = dgvr.Cells[physicalFileName].Value.ToString();
				}
			}

			SetConsoleProgressBarValue(sender, 10);
			Cursor.Current = Cursors.WaitCursor;
			EnableConsoleProgressBar(sender);

			// procedo con l'esecuzione del restore
			SetConsoleProgressBarText(sender, string.Format(Strings.ExecutingRestoreDB, databaseName));

			dbTask.Diagnostic.Clear();
			dbTask.CurrentStringConnection = bci.ConnectionString;

			SQLRestoreDBParameters restoreParams = new SQLRestoreDBParameters();
			restoreParams.DatabaseName = databaseName;
			restoreParams.RestoreFilePath = bakFilePath;
			restoreParams.ForceRestore = true;
			restoreParams.DataLogicalName = dataLogicalName;
			restoreParams.DataPhysicalName = dataPhysicalName;
			restoreParams.LogLogicalName = logLogicalName;
			restoreParams.LogPhysicalName = logPhysicalName;

			if (dbTask.RestoreWithMove(restoreParams))
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Database, databaseName);
				extendedInfo.Add(Strings.BackupFilePath, bakFilePath);
				extendedInfo.Add(Strings.DataFileName, dataLogicalName);
				extendedInfo.Add(Strings.FilePosition, dataPhysicalName);
				extendedInfo.Add(Strings.LogFileName, logLogicalName);
				extendedInfo.Add(Strings.FilePosition, logPhysicalName);

				RestoreDBDiagnostic.Set
					(
					DiagnosticType.Information,
					string.Format(Strings.OperationEndedSuccessfully, Strings.RestoreProcessName, databaseName),
					extendedInfo
					);

				// il controllo del database va effettuato solo per il db aziendale
				if (bci.BackupDBType == BackupDBType.ERP)
					CheckCompanyDatabase();
			}
			else
				RestoreDBDiagnostic.Set(dbTask.Diagnostic);

			SetConsoleProgressBarText(sender, string.Empty);
		}
		# endregion

		# region Metodi gestione cross-thread
		//---------------------------------------------------------------------
		private void EnableFormControls(bool enable)
		{
			Invoke(new MethodInvoker(() => { this.Enabled = enable; }));
		}

		///<summary>
		/// Invoco il metodo per il controllo sul database dopo aver effettuato il restore
		/// (devo utilizzare l'InvokeRequired perche' esegue delle operazioni sul tree della Console)
		///</summary>
		//---------------------------------------------------------------------
		private void CheckCompanyDatabase()
		{
			Invoke(new MethodInvoker(() => 
			{ 
				if (CheckCompanyDBVersionAfterRestore())
					RestoreDBDiagnostic.SetInformation(DatabaseLayerStrings.DbIsToUpdate);
			}));
		}

		//---------------------------------------------------------------------
		private void ReloadConsoleTree()
		{
			Invoke(new MethodInvoker(() =>
			{
				// forzo l'update del tree della console in modo da visualizzare un eventuale cambio di stato dei nodi
				OnModifyTreeOfCompanies?.Invoke(this, ConstString.containerCompanies, companyItem.CompanyId);
				OnModifyTree?.Invoke(this, ConstString.containerCompanies);
				OnAfterModifyCompany?.Invoke(this, companyItem.CompanyId);
			}));
		}

		///<summary>
		/// Visualizzo la diagnostica
		///</summary>
		//---------------------------------------------------------------------
		private void ShowRestoreDiagnostic()
		{
			Invoke(new MethodInvoker(() =>
			{
				DiagnosticView notifications = new DiagnosticView(RestoreDBDiagnostic);
				notifications.ShowDialog();

				dbTask.Diagnostic.Clear();
				ShowDetailsButton.Enabled = true;
			}));
		}
		# endregion

		#region Gestione DMS
		/// <summary>
		/// controlla il database che ho appena ripristinato:
		/// - se esiste la tabella TB_DBMark
		/// - se usa il set di caratteri Unicode
		/// - se è una versione italiana (ovvero Mago.Net 1.2.5)
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckDmsDBVersionAfterRestore(BackupConnectionInfo bci)
		{
			// ri-leggo le info dell'azienda (che potrei aver modificato in precedenza)
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = sysStatus.ConnectionString;
			companyDb.CurrentSqlConnection = sysStatus.CurrentConnection;

			ArrayList company = new ArrayList();
			if (!companyDb.GetAllCompanyFieldsById(out company, companyId))
			{
				if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
					RestoreDBDiagnostic.Set(companyDb.Diagnostic);
				else
					RestoreDBDiagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				company.Clear();
				return false;
			}

			if (company.Count == 0)
				return false;

			companyItem = (CompanyItem)company[0];

			bool isUnicode = false, isItalianVersionDB = false, existTB_DBMark = false;

			// evento per l'ApplicationDBAdmin che si connette al db DMS e fa gli opportuni controlli
			if (OnCheckRestoredDatabase != null)
				existTB_DBMark = OnCheckRestoredDatabase(bci.ConnectionString, DBMSType.SQLSERVER, companyItem.UseUnicode, out isUnicode, out isItalianVersionDB);

			// se la TB_DBMark non esiste non procedo
			if (!existTB_DBMark)
			{
				RestoreDBDiagnostic.SetWarning(string.Format(DatabaseLayerStrings.DbNoCompatible, bci.DbName));
				return false;
			}
			else
			{
				// devo controllare la compatibilità tra la collate presente sul db e quella memorizzata sulla
				// MSD_Companies. Se diverse ritorno false
				int dmsCultureValue =
					DBGenericFunctions.AssignDatabaseCultureValue(this.isoState, bci.ConnectionString, DBMSType.SQLSERVER, isUnicode);

				// se l'LCID letto estrapolato dall'isostato e' diverso da 0 e diverso da quello letto da database allora lo assegno
				if (dmsCultureValue != 0 && dmsCultureValue != companyItem.DatabaseCulture)
					return false;

				// se il flag 'Utilizza il set di caratteri unicode' impostato sull'anagrafica azienda non corrisponde
				// dalla situazione effettiva presente sul database appena ripristinato, torno false
				if (isUnicode != companyItem.UseUnicode)
					return false;

				// se la versione del db è in italiano (ovvero prima della 2.0) ritorno false
				// controllo inutile per il DMS ma ho gia' in canna l'informazione e non mi costa nulla testarla
				if (isItalianVersionDB)
					return false;
			}

			// ritorna true solo se il db contiene la TB_DBMark, se l'anagrafica azienda è stata modificata senza errori e 
			// se il db non è in versione italiana.
			return true;
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
			// elimino dalle anagrafiche i dati dello slave del dms
			CompanyDBSlave companyDBSlave = new CompanyDBSlave();
			companyDBSlave.ConnectionString = sysStatus.ConnectionString;
			companyDBSlave.CurrentSqlConnection = sysStatus.CurrentConnection;

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
						RestoreDBDiagnostic.Set(companyDBSlave.Diagnostic);
					RestoreDBDiagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.CompanyDBSlaveModify, dbSlaveItem.SlaveId));
					return false;
				}
			}

			// aggiorno il flag UseDBSlaves della tabella MSD_Companies
			CompanyDb companyDb = new CompanyDb();
			companyDb.ConnectionString = sysStatus.ConnectionString;
			companyDb.CurrentSqlConnection = sysStatus.CurrentConnection;
			if (!companyDb.UpdateUseDBSlaveValue(this.companyId, false))
			{
				RestoreDBDiagnostic.Set(companyDb.Diagnostic);
				return false;
			}

			return true;
		}
		# endregion

		# region Eventi intercettati sulla form e controls
		/// <summary>
		/// se il parent è null significa che ho pulito la workingarea
		/// devo fare la disconnessione dalla connessione principale
		/// </summary>
		//---------------------------------------------------------------------
		private void RestoreDatabase_ParentChanged(object sender, System.EventArgs e)
		{
			if (this.Parent == null)
			{
				//Ri-abilito il cursore
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Visualizzazione del DiagnosticViewer, per rivedere i msg memorizzati durante le elaborazioni
		/// </summary>
		//---------------------------------------------------------------------
		private void ShowDetailsButton_Click(object sender, System.EventArgs e)
		{
			if (RestoreDBDiagnostic.Error || RestoreDBDiagnostic.Information || RestoreDBDiagnostic.Warning)
				DiagnosticViewer.ShowDiagnostic(RestoreDBDiagnostic);
		}

		///<summary>
		/// CheckValidator
		/// Controllo preventivo sui valori inseriti nella form
		///</summary>
		//---------------------------------------------------------------------
		private void CheckValidator()
		{
			RestoreDBDiagnostic.Clear();

			foreach (DataGridViewRow dgvr in FileListDataGridView.Rows)
			{
				string logicName = dgvr.Cells[logicalFileName].Value.ToString();
				string physicalName = dgvr.Cells[physicalFileName].Value.ToString();

				// se i nomi dei file sono vuoti non procedo
				if (string.IsNullOrWhiteSpace(logicName) || string.IsNullOrWhiteSpace(physicalName))
				{
					RestoreDBDiagnostic.SetError(Strings.UnableToPerformRestore + " " + Strings.RestoreFilesListValid);
					continue;
				}

				// se i nomi dei file fisici non esistono non procedo
				if (string.IsNullOrWhiteSpace(Path.GetFileName(physicalName)) || !File.Exists(physicalName))
				{
					RestoreDBDiagnostic.SetError(Strings.UnableToPerformRestore + " " + Strings.RestoreFilesListValid);
					continue;
				}
			}
		}

		# endregion

		///<summary>
		/// Abilita / Disabilita il tree della Console
		///</summary>
		//---------------------------------------------------------------------------
		private void SetConsoleTreeViewEnabled(bool enable)
		{
			OnEnableConsoleTreeView?.Invoke(enable);
		}

		#region Funzioni relative alla ProgressBar
		///<summary>
		/// Dice alla MC di abilitare la ProgressBar
		///</summary>
		//---------------------------------------------------------------------------
		private void EnableConsoleProgressBar(object sender)
		{
			OnEnableProgressBar?.Invoke(sender);
		}

		///<summary>
		/// Dice alla MC di disabilitare la ProgressBar
		///</summary>
		//---------------------------------------------------------------------------
		private void DisableConsoleProgressBar(object sender)
		{
			OnDisableProgressBar?.Invoke(sender);
		}

		///<summary>
		/// Dice alla MC di impostare lo Step della ProgressBar
		///</summary>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarStep(object sender, int step)
		{
			OnSetProgressBarStep?.Invoke(sender, step);
		}

		///<summary>
		/// Dice alla MC di impostare il value della ProgressBar
		///</summary>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarValue(object sender, int currentValue)
		{
			OnSetProgressBarValue?.Invoke(sender, currentValue);
		}

		///<summary>
		/// Dice alla MC quale deve essere il testo da visualizzare accanto alla progressBar 
		///</summary>
		//---------------------------------------------------------------------------
		private void SetConsoleProgressBarText(object sender, string message)
		{
			OnSetProgressBarText?.Invoke(sender, message);
		}

		///<summary>
		/// Dice alla MC di incrementare la ProgressBar in modo ciclico
		///</summary>
		//---------------------------------------------------------------------------
		private void SetConsoleCyclicStepProgressBar()
		{
			OnSetCyclicStepProgressBar?.Invoke();
		}
		#endregion
	}
}