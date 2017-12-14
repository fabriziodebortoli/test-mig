using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.Console.Plugin.SysAdmin.UserControls;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// Form che consente di effettuare operazioni di Backup di database
	/// (database di sistema, aziendale e DMS) dall'Administration Console
	/// </summary>
	//=========================================================================
	public partial class BackupDatabase : PlugInsForm
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

		public delegate bool IsActivated(string application, string functionality);
		public event IsActivated OnIsActivated;
		# endregion

		// Oggetto di diagnostica per la form
		public Diagnostic BackupDBDiagnostic = new Diagnostic("SysAdmin.BackupDatabase");

		# region Private Variables
		// lista delle informazioni dei database di cui fare il backup
		private IDictionary<BackupDBType, BackupConnectionInfo> DictBackupInfo = new Dictionary<BackupDBType, BackupConnectionInfo>();

		// per accedere alle funzioni di Backup
		private DatabaseTask dbTask = new DatabaseTask();

		private bool isCompanyDB = true;
		private string companyId = string.Empty;
		private bool runningFromServer = false;

		private SysAdminStatus sysStatus = null;

		// per tenere traccia delle informazioni sull'azienda estrapolate dal database di sistema
		private CompanyItem companyItem = null;

		// per memorizzare i valori estratti dalle tabelle di sistema relative ai backup
		private string backupSetName = string.Empty;
		private string backupSetDescri = string.Empty;
		private DateTime backupFinishDate = new DateTime(1973, 4, 26);
		private string logicalDeviceName = string.Empty;
		private string physicalDeviceName = string.Empty;

		// per popolare il DataGrid dei backups
		private BakCommonObjects backupsDT = new BakCommonObjects();

		//---------- Gestione DMS
		private bool isDMSActivated = false;
		# endregion

		# region Constructor e Load
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="isCompanyDB">bool</param>
		/// <param name="companyId">companyId on selected node</param>
		/// <param name="sysStatus">SysAdminStatus</param>
		/// <param name="runningFromServer">bool (if console running from server or not)</param>
		//---------------------------------------------------------------------
		public BackupDatabase(bool isCompanyDB, string companyId, SysAdminStatus sysStatus, bool runningFromServer)
		{
			InitializeComponent();

			this.isCompanyDB = isCompanyDB;
			this.companyId = companyId;
			this.sysStatus = sysStatus;
			this.runningFromServer = runningFromServer;
		}

		/// <summary>
		/// Load della form
		/// Se sono riuscita a caricare le informazioni che mi occorrono dal db di sistema allora procedo
		/// nel disegno della form e nell'impostazione dei controls
		/// </summary>
		//---------------------------------------------------------------------
		private void BackupDatabase_Load(object sender, System.EventArgs e)
		{
			// verifico se il database documentale e' attivato
			// qui e non nel costruttore altrimenti l'evento non e' ancora stato agganciato
			if (OnIsActivated != null && OnIsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				isDMSActivated = true;

			LoadInformation();
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
			if (isCompanyDB)
			{
				//--------------------------
				// DATABASE AZIENDALE
				//--------------------------
				if (!LoadCompanyData())
				{
					BackupDBDiagnostic.Set(DiagnosticType.Error, Strings.UnableToPerformBackup);
					return;
				}
			}
			else
			{
				//--------------------------
				// DATABASE DI SISTEMA
				//--------------------------
				BackupConnectionInfo sysDbBakInfo = new BackupConnectionInfo
						(
						BackupDBType.SYSDB,
						string.IsNullOrWhiteSpace(sysStatus.ServerIstanceName) ? sysStatus.ServerName : Path.Combine(sysStatus.ServerName, sysStatus.ServerIstanceName),
						sysStatus.DataSource,
						sysStatus.User,
						sysStatus.Password,
						sysStatus.IntegratedConnection
						);

				LoadBackupInfoFromMSDB(sysDbBakInfo);
				// aggiungo nel dictionary le info del database di sistema
				DictBackupInfo.Add(new KeyValuePair<BackupDBType, BackupConnectionInfo>(BackupDBType.SYSDB, sysDbBakInfo));
			}

			SetControlsValue();

			// se e' andato tutto bene carico le info nel DataGrid
			LoadInfoInDataGrid();
		}
		# endregion

		# region SetControlsValue - Impostazione dei controlli della form a seconda delle operazioni da effettuare
		/// <summary>
		/// imposto la visibilità ed i testi dei controlli discriminando sul tipo attività
		/// e carico dal database le informazioni che mi occorrono (richiamata nella Load della form)
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			VerifyBackupCheckBox.Checked = true;
			ShowDetailsButton.Enabled = false;

			// la groupbox delle info del backup e' visibile solo per il db di sistema
			DBInfoGroupBox.Visible = !isCompanyDB;

			if (!isCompanyDB)
			{
				// solo per il db di sistema valorizzo alcuni controls con le info sul backup
				BkpDescriTextBox.Text = (!string.IsNullOrWhiteSpace(backupSetDescri)) ? backupSetDescri : "---";
				LastBkpDateTextBox.Text = (backupFinishDate.Year != 1973) ? string.Concat(backupFinishDate.ToLongDateString(), " ", backupFinishDate.ToLongTimeString()) : "---";

				BackupConnectionInfo bci = null;
				if (DictBackupInfo.TryGetValue(BackupDBType.SYSDB, out bci) && bci != null)
				{
					DatabaseTextBox.Text = bci.DbName;

					// se sto runnando lato client azzero il path dell'ultimo backup effettuato
					if (!runningFromServer)
						physicalDeviceName = string.Empty;

					// inizializzo con il path dell'ultimo bkp effettuato, se esiste
					if (!string.IsNullOrWhiteSpace(physicalDeviceName))
						bci.BackupFilePath = physicalDeviceName;
				}
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
					BackupDBDiagnostic.Set(companyDb.Diagnostic);
				else
					BackupDBDiagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
				company.Clear();
				return false;
			}

			if (company.Count == 0)
				return false;

			// leggo le info dell'azienda
			companyItem = (CompanyItem)company[0];

			// carico le info dell'owner del db aziendale e relativa aggiunta del database da backuppare solo se si tratta di SQL
			if (TBDatabaseType.GetDBMSType(companyItem.Provider) == DBMSType.SQLSERVER)
			{
				// Leggo le info dell'owner da CompanyLogins per quell'azienda
				CompanyUserDb companyOwner = new CompanyUserDb();
				companyOwner.ConnectionString = sysStatus.ConnectionString;
				companyOwner.CurrentSqlConnection = sysStatus.CurrentConnection;

				ArrayList companyOwnerData = new ArrayList();
				if (!companyOwner.GetUserCompany(out companyOwnerData, companyItem.DbOwner, companyId))
				{
					if (companyOwner.Diagnostic.Error || companyOwner.Diagnostic.Warning || companyOwner.Diagnostic.Information)
						BackupDBDiagnostic.Set(companyOwner.Diagnostic);
					else
						BackupDBDiagnostic.Set(DiagnosticType.Error, Strings.CannotReadingCompanyInfo);
					companyOwnerData.Clear();
					return false;
				}

				// leggo le info del dbowner
				CompanyUser companyUser = (CompanyUser)companyOwnerData[0];

				// aggiungo nel dictionary le info del database aziendale
				BackupConnectionInfo companyBakInfo = new BackupConnectionInfo
						(
						BackupDBType.ERP,
						companyItem.DbServer,
						companyItem.DbName,
						companyUser.DBDefaultUser,
						companyUser.DBDefaultPassword,
						companyUser.DBWindowsAuthentication
						);
				LoadBackupInfoFromMSDB(companyBakInfo);
				DictBackupInfo.Add(new KeyValuePair<BackupDBType, BackupConnectionInfo>(BackupDBType.ERP, companyBakInfo));
			}

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
							{
								BackupConnectionInfo dmsBakInfo = new BackupConnectionInfo
										(
										BackupDBType.DMS,
										dmsSlaveItem.ServerName,
										dmsSlaveItem.DatabaseName,
										dmsSlaveLoginItem.SlaveDBUser,
										dmsSlaveLoginItem.SlaveDBPassword,
										dmsSlaveLoginItem.SlaveDBWinAuth
										);
								LoadBackupInfoFromMSDB(dmsBakInfo);
								DictBackupInfo.Add(new KeyValuePair<BackupDBType, BackupConnectionInfo>(BackupDBType.DMS, dmsBakInfo));
							}
						}
					}
				}
			}

			return true;
		}

		///<summary>
		/// Carico le informazioni dei backup nel DataGrid
		///</summary>
		//---------------------------------------------------------------------
		private void LoadInfoInDataGrid()
		{
			foreach (KeyValuePair<BackupDBType, BackupConnectionInfo> bak in DictBackupInfo)
			{
				// se si tratta del db di sistema skippo i db aziendali oppure
				// se si tratta dei db di aziendali skippo il db di sistema
				if (
					(!isCompanyDB && bak.Key != BackupDBType.SYSDB) ||
					(isCompanyDB && bak.Key == BackupDBType.SYSDB)
					)
					continue;

				DataRow dRow = backupsDT.NewRow();

				dRow[DataTableConsts.BackupDBType] = bak.Value.BackupDBType;
				dRow[DataTableConsts.Selected] = true;
				dRow[DataTableConsts.DBName] = bak.Value.DbName;
				dRow[DataTableConsts.BakPath] = bak.Value.BackupFilePath;
				dRow[DataTableConsts.Browse] = "...";
				dRow[DataTableConsts.BackupConnectionInfo] = bak.Value;

				backupsDT.Rows.Add(dRow);
			}

			backupsDT.AcceptChanges();

			// assegno il datatable al datasource
			BackupInfoGridUserCtrl.BakDataGridView_DataSource = backupsDT;

			// se sto runnando lato client non consento di browsare i file
			if (!runningFromServer)
				BackupInfoGridUserCtrl.SetCellsReadOnly();
		}
		# endregion

		# region LoadBackupInfoFromMSDB (interrogo le tabelle di backupset e backupmediafamily nel db di sistema msdb)
		/// <summary>
		/// query sulle tabelle backupset e backupmediafamily nel db di sistema msdb per estrapolare informazioni
		/// sulla cronologia dei backup
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadBackupInfoFromMSDB(BackupConnectionInfo bakInfo)
		{
			// inizializzo di default il path del backup nella Custom
			string bakFilName = string.Concat(bakInfo.DbName, ".bak");
			bakInfo.BackupFilePath = 
				(isCompanyDB)
					? Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomCompanyDataTransferBackupPath(companyItem.Company), bakFilName)
					: Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomCompanyDataTransferBackupPath(NameSolverStrings.AllCompanies), bakFilName);

			// compongo la stringa di connessione al database msdb
			string msdbConnectionString = 
				bakInfo.IsWinAuth
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, bakInfo.ServerName, "msdb")
				: string.Format(NameSolverDatabaseStrings.SQLConnection, bakInfo.ServerName, "msdb", bakInfo.Login, bakInfo.Pw);

			// join sulle tabelle presenti nel database di sistema msdb, in modo da reperire le
			// informazioni relative (se esistono) all'ultimo backup effettuato per il database indicato
			string query = @"SELECT backupset.description AS Description, 
							backupset.backup_finish_date AS FinishDate,
							backupmediafamily.physical_device_name AS PhysicalDevice
							FROM backupmediafamily 
							INNER JOIN backupset ON backupmediafamily.media_set_id = backupset.media_set_id
							WHERE (backupset.database_name = @dbName) 
							AND (backupset.server_name = @serverName)
							ORDER BY backupset.backup_finish_date DESC";

			try
			{
				using (TBConnection connection = new TBConnection(msdbConnectionString, DBMSType.SQLSERVER))
				{
					connection.Open();

					using (TBCommand command = new TBCommand(query, connection))
					{
						command.Parameters.Add("@dbName", bakInfo.DbName);
						command.Parameters.Add("@serverName", bakInfo.ServerName);

						using (IDataReader dataReader = command.ExecuteReader())
						{
							if (dataReader != null && connection.DataReaderHasRows(dataReader))
							{
								while (dataReader.Read())
								{
									backupSetDescri = dataReader["Description"].ToString();
									backupFinishDate = (DateTime)dataReader["FinishDate"];
									physicalDeviceName = dataReader["PhysicalDevice"].ToString(); // serve per riproporre l'ultimo path del backup effettuato
									break; // ordinando per data descrescente considero i dati della prima row estratta
								}
							}
						}
					}
				}
			}
			catch (TBException e)
			{
				BackupDBDiagnostic.SetError(Strings.ErrorInMsdbConnection + e.Message);
			}

			// se ho trovato il nome del path di backup piu' recente lo imposto
			if (!string.IsNullOrWhiteSpace(physicalDeviceName))
				bakInfo.BackupFilePath = physicalDeviceName;
		}
		#endregion

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
				string.Format(Strings.StartBkpRestoreProcess, Strings.BackupProcessName),
				Strings.BackupDatabase,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question
				)
				== DialogResult.No)
				return;

			BackupDBDiagnostic.Clear();
			// controllo i valori inseriti nella form e se ho degli errori/warnings li mostro e mi fermo
			CheckValidator();
			if (BackupDBDiagnostic.Error || BackupDBDiagnostic.Warning)
			{
				ShowBackupDiagnostic();
				return;
			}

			// se e' tutto ok inizio con i backup
			StartBackupProcess();
		}

		///<summary>
		/// Lancio l'elaborazione su un thread separato
		///</summary>
		//---------------------------------------------------------------------
		public Thread StartBackupProcess()
		{
			Thread myThread = new Thread(new ParameterizedThreadStart(InternalBackupProcess));

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
		private void InternalBackupProcess(object sender)
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
			// ESECUZIONE DEL BACKUP DEL DATABASE E VERIFICA INTEGRITA' DATI
			//---------------------------------------------------------------------------------------------------

			// faccio un loop su tutte le righe presenti nel DataGrid e solo per quelle selezionate
			// procedo ad eseguire il backup
			foreach (DataGridViewRow dgvr in BackupInfoGridUserCtrl.BakDataGridView_Control.Rows)
			{
				DataRowView row = (DataRowView)dgvr.DataBoundItem;
				if (row == null)
					continue;

				// se la riga non e' selezionata la skippo
				if (!(bool)row[DataTableConsts.Selected])
					continue;

				RunBackup(row, sender);
			}

			//Setto al max la progressBar
			SetConsoleProgressBarValue(sender, 100);

			//Disabilito la progressBar
			SetConsoleProgressBarText(sender, string.Empty);
			DisableConsoleProgressBar(sender);
			SetConsoleTreeViewEnabled(true);
			//Ri-abilito il cursore
			Cursor.Current = Cursors.Default;

			// mostro la diagnostica
			ShowBackupDiagnostic();

			EnableFormControls(true);
		}

		///<summary>
		/// Esecuzione backup
		///</summary>
		//---------------------------------------------------------------------
		private void RunBackup(DataRowView row, object sender)
		{
			string dbName = row[DataTableConsts.DBName].ToString();
			string bakFilePath = row[DataTableConsts.BakPath].ToString();
			BackupConnectionInfo bci = row[DataTableConsts.BackupConnectionInfo] as BackupConnectionInfo;

			if (bci == null || string.IsNullOrWhiteSpace(dbName) || string.IsNullOrWhiteSpace(bakFilePath))
				return;

			SetConsoleProgressBarValue(sender, 10);
			Cursor.Current = Cursors.WaitCursor;
			EnableConsoleProgressBar(sender);

			// se non esiste il folder lo creo
			if (!Directory.Exists(Path.GetDirectoryName(bakFilePath)))
				Directory.CreateDirectory(Path.GetDirectoryName(bakFilePath));

			// procedo con l'esecuzione del backup
			SetConsoleProgressBarText(sender, string.Format(Strings.ExecutingBackupDB, dbName));

			dbTask.Diagnostic.Clear();

			dbTask.CurrentStringConnection = bci.ConnectionString;

			SQLBackupDBParameters bakParams = new SQLBackupDBParameters();
			bakParams.DatabaseName = dbName;
			bakParams.BackupFilePath = bakFilePath;
			bakParams.Overwrite = OverwriteFileCheckBox.Checked;

			if (dbTask.Backup(bakParams))
			{
				if (!isCompanyDB)
					UpdateDescriptions(dbName);

				BackupDBDiagnostic.Set
					(
					DiagnosticType.Information,
					string.Format(Strings.OperationEndedSuccessfully, Strings.BackupProcessName, dbName),
					new ExtendedInfo(Strings.BackupFilePath, bakFilePath)
					);

				// se l'operazione di backup è andata a buon fine e l'utente ha scelto anche di effettuare
				// la verifica del file, allora procedo con l'istruzione RESTORE VERIFYONLY
				if (VerifyBackupCheckBox.Checked)
				{
					SetConsoleProgressBarText(sender, string.Format(Strings.VerifyingBackupFile, Path.GetFileName(bakFilePath)));

					// eseguo la verifica dei file 
					dbTask.VerifyBackupFile(bakFilePath);
					BackupDBDiagnostic.Set(dbTask.Diagnostic);
				}
			}
			else
				BackupDBDiagnostic.Set(dbTask.Diagnostic);

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
		/// Aggiorno le descrizioni
		///</summary>
		//---------------------------------------------------------------------
		private void UpdateDescriptions(string dbName)
		{
			Invoke(new MethodInvoker(() =>
			{
				LastBkpDateTextBox.Text = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
				BkpDescriTextBox.Text = string.Format(DatabaseLayerStrings.CompleteBackup, dbName);
			}));
		}

		///<summary>
		/// Visualizzo la diagnostica
		///</summary>
		//---------------------------------------------------------------------
		private void ShowBackupDiagnostic()
		{
			Invoke(new MethodInvoker(() =>
				{
					DiagnosticView notifications = new DiagnosticView(BackupDBDiagnostic);
					notifications.ShowDialog();

					dbTask.Diagnostic.Clear();
					ShowDetailsButton.Enabled = true;
				}));
		}
		# endregion

		# region Eventi intercettati sulla form e controls
		/// <summary>
		/// se il parent è null significa che ho pulito la workingarea
		/// devo fare la disconnessione dalla connessione principale
		/// </summary>
		//---------------------------------------------------------------------
		private void BackupDatabase_ParentChanged(object sender, System.EventArgs e)
		{
			if (this.Parent == null)
				Cursor.Current = Cursors.Default; 
		}

		# region Visualizzazione tooltip sui vari control della form
		//---------------------------------------------------------------------
		private void VerifyBackupCheckBox_MouseHover(object sender, System.EventArgs e)
		{
			FilePathToolTip.SetToolTip(VerifyBackupCheckBox, Strings.VerifyBackupInfo);
		}

		//---------------------------------------------------------------------
		private void OverwriteFileCheckBox_MouseHover(object sender, EventArgs e)
		{
			FilePathToolTip.SetToolTip(OverwriteFileCheckBox, Strings.OverwriteExistingBakFile);
		}
		# endregion

		/// <summary>
		/// Visualizzazione del DiagnosticViewer, per rivedere i msg memorizzati durante le elaborazioni
		/// </summary>
		//---------------------------------------------------------------------
		private void ShowDetailsButton_Click(object sender, System.EventArgs e)
		{
			if (BackupDBDiagnostic.Error || BackupDBDiagnostic.Information || BackupDBDiagnostic.Warning)
				DiagnosticViewer.ShowDiagnostic(BackupDBDiagnostic);
		}

		///<summary>
		/// CheckValidator
		/// Controllo preventivo sui valori inseriti nella form
		///</summary>
		//---------------------------------------------------------------------
		private void CheckValidator()
		{
			BackupDBDiagnostic.Clear();

			int dgRowsCount = BackupInfoGridUserCtrl.BakDataGridView_Control.Rows.Count;

			// se nel datagrid ho piu' di una riga
			// devo controllare il numero delle righe selezionate 
			// in modo che non sia stato specificato lo stesso nome per i due backup
			if (dgRowsCount > 1)
			{
				DataGridViewRow dgvr1 = BackupInfoGridUserCtrl.BakDataGridView_Control.Rows[0];
				DataGridViewRow dgvr2 = BackupInfoGridUserCtrl.BakDataGridView_Control.Rows[1];

				// se entrambe le righe sono selezionate, controllo che i path del backup siano differenti
				// (per evitare backup di database multipli che poi non siamo in grado di ripristinare)
				if ((bool)dgvr1.Cells[DataTableConsts.Selected].Value && (bool)dgvr2.Cells[DataTableConsts.Selected].Value)
				{
					if (
						string.Compare
						(
						dgvr1.Cells[DataTableConsts.BakPath].Value.ToString(),
						dgvr2.Cells[DataTableConsts.BakPath].Value.ToString(),
						StringComparison.InvariantCultureIgnoreCase) == 0
						)
					{
						BackupDBDiagnostic.SetError(Strings.DuplicateBackupFileNames);
						return;
					}
				}
			}

			bool existSelRows = false;

			foreach (DataGridViewRow dgvr in BackupInfoGridUserCtrl.BakDataGridView_Control.Rows)
			{
				// se la riga non e' selezionata non la considero
				if (!(bool)dgvr.Cells[DataTableConsts.Selected].Value)
					continue;

				existSelRows = true;

				string fileBak = dgvr.Cells[DataTableConsts.BakPath].Value.ToString();

				// se non è stato indicato un path non procedo
				if (string.IsNullOrWhiteSpace(fileBak))
				{
					BackupDBDiagnostic.SetError(string.Format(Strings.EmptyBackupFile, fileBak));
					continue;
				}

				// se il nome del file è vuoto non procedo
				if (string.IsNullOrWhiteSpace(Path.GetFileName(fileBak)))
				{
					BackupDBDiagnostic.SetError(string.Format(Strings.FileNotExists, fileBak));
					continue;
				}

				// se il file non ha estensione aggiungo in automatico .bak in fondo
				if (string.IsNullOrWhiteSpace(Path.GetExtension(fileBak)))
				{
					dgvr.Cells[DataTableConsts.BakPath].Value += ".bak";
					fileBak = dgvr.Cells[DataTableConsts.BakPath].Value.ToString();
				}

				// se il file esiste devo informare che per procedere deve selezionare la checkbox di Overwrite
				if (File.Exists(fileBak) && !OverwriteFileCheckBox.Checked)
				{
					BackupDBDiagnostic.SetWarning
						(
						string.Format(Strings.BackupFileAlreadyExists,
						Path.GetFileName(fileBak),
						OverwriteFileCheckBox.Text)
						);
					continue;
				}
			}

			// se nessuna riga e' stata selezionata visualizzo un msg
			if (!existSelRows)
				BackupDBDiagnostic.SetWarning(Strings.NoBackupsSelected);
		}
		# endregion

		#region Funzioni relative alla ProgressBar e al tree in Console
		///<summary>
		/// Abilita / Disabilita il tree della Console
		///</summary>
		//---------------------------------------------------------------------------
		private void SetConsoleTreeViewEnabled(bool enable)
		{
			OnEnableConsoleTreeView?.Invoke(enable);
		}

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