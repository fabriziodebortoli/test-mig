using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.DatabaseWinControls;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{

	/// <summary>
	/// CreateDBForm - Maschera x la creazione dei databases
	/// </summary>
	//=========================================================================
	public partial class CreateDBForm : System.Windows.Forms.Form
	{
		private string serverName = string.Empty;
		private string instanceName = string.Empty;
		private string databaseName = string.Empty;
		private string loginName = string.Empty;
		private string loginPassword = string.Empty;
		private bool isWindowsAuth = false;
		private string defaultPathDataFile = string.Empty;
		private string defaultPathLogFile = string.Empty;
		private int initialSizeData = 0;
		private int initialSizeLog = 0;
		private bool initUnicodeCheck = false;
		private string databaseCulture = string.Empty;
		private bool isCompanyDB = true;
		private int port = 0;
		private bool isAzureSQLDatabase = false;

		private bool elaborationIsRunning = false;

		private DBNetworkType dbNetworkType = DBNetworkType.Small;
		private DiagnosticViewer diagnosticViewer = new DiagnosticViewer();

		private Diagnostic createDBDiagnostic = new Diagnostic("SysAdmin.Form.CreateDBForm");
		private bool operationResult = false;

		#region Properties
		//---------------------------------------------------------------------
		public Diagnostic CreateDBDiagnostic { get { return createDBDiagnostic; } }
		public bool Result { get { return operationResult; } }

		//---------------------------------------------------------------------
		public string PathDataFile { get { return TextBoxDataPath.Text; } set { TextBoxDataPath.Text = value; } }
		public string PathLogFile { get { return TextBoxLogPath.Text; } set { TextBoxLogPath.Text = value; } }
		public string DefaultPathDataFile { get { return defaultPathDataFile; } set { defaultPathDataFile = value; } }
		public string DefaultPathLogFile { get { return defaultPathLogFile; } set { defaultPathLogFile = value; } }
		public string DatabaseCulture { get { return databaseCulture; } set { databaseCulture = value; } }

		//DataFile
		public bool IsPercentGrowDataFile { get { return RadioPercentGrowDataFile.Checked; } set { RadioPercentGrowDataFile.Checked = value; } }
		public int MegabyteDataGrow { get { return Convert.ToInt32(MegabyteDataGrowUpDown.Value); } set { MegabyteDataGrowUpDown.Value = Convert.ToDecimal(value); } }
		public int PercentDataGrow { get { return Convert.ToInt32(PercentDataGrowUpDown.Value); } set { PercentDataGrowUpDown.Value = Convert.ToDecimal(value); } }
		public bool IsUnrestrictedGrowData { get { return RadioUnrestrictedGrowData.Checked; } set { RadioUnrestrictedGrowData.Checked = value; } }
		public int MaxGrowFileData { get { return Convert.ToInt32(MaxGrowFileDataUpDown.Value); } set { MaxGrowFileDataUpDown.Value = Convert.ToDecimal(value); } }
		public int InitialSizeData { get { return initialSizeData; } set { initialSizeData = value; } }

		//LogFile
		public bool IsPercentGrowLogFile { get { return RadioPercentGrowLogFile.Checked; } set { RadioPercentGrowLogFile.Checked = value; } }
		public int MegabyteLogGrow { get { return Convert.ToInt32(MegabyteLogGrowUpDown.Value); } set { MegabyteLogGrowUpDown.Value = Convert.ToDecimal(value); } }
		public int PercentLogGrow { get { return Convert.ToInt32(PercentLogGrowUpDown.Value); } set { PercentLogGrowUpDown.Value = Convert.ToDecimal(value); } }
		public bool IsUnrestrictedGrowLog { get { return RadioUnrestrictedGrowLog.Checked; } set { RadioUnrestrictedGrowLog.Checked = value; } }
		public int MaxGrowFileLog { get { return Convert.ToInt32(MaxGrowFileLogUpDown.Value); } set { MaxGrowFileLogUpDown.Value = Convert.ToDecimal(value); } }
		public int InitialSizeLog { get { return initialSizeLog; } set { initialSizeLog = value; } }

		//General
		public bool IsTruncateLog { get { return CbTruncateLog.Checked; } set { CbTruncateLog.Checked = value; } }
		public bool IsAutoShrink
		{
			get { return isAzureSQLDatabase ? CbAzureAutoShrink.Checked : CbAutoShrink.Checked; }
			set { CbAzureAutoShrink.Checked = CbAutoShrink.Checked = value; }
		}
		public bool UseUnicode
		{
			get { return isAzureSQLDatabase ? CbAzureUseUnicode.Checked : cbUseUnicode.Checked; }
			set { CbAzureUseUnicode.Checked = cbUseUnicode.Checked = value; }
		}
		#endregion

		#region Constructor
		//---------------------------------------------------------------------
		public CreateDBForm
		(
			bool isCompanyDB,
			string serverName,
			string instanceName,
			string databaseName,
			string loginName,
			string loginPassword,
			bool isWindowsAuthentication,
			bool showUnicodeCheck,
			bool enableUnicodeCheck,
			bool initUnicodeCheckValue,
			DBNetworkType dbNetwork,
			string initDBCulture,
			bool disableDBCultureComboBox,
            int Port = 0,
			bool isAzureDb = false
		)
		{
			InitializeComponent();

			this.serverName = serverName;
			this.instanceName = instanceName;
			this.databaseName = databaseName;
			this.loginName = loginName;
			this.loginPassword = loginPassword;
			this.isWindowsAuth = isWindowsAuthentication;
			this.databaseCulture = initDBCulture;
			this.initUnicodeCheck = initUnicodeCheckValue;
			this.isCompanyDB = isCompanyDB;
			this.dbNetworkType = dbNetwork;
            this.port = Port;
			this.isAzureSQLDatabase = isAzureDb;

			InitializeSettings(showUnicodeCheck, enableUnicodeCheck, disableDBCultureComboBox);
		}
		#endregion

		#region InitializeSettings - Imposta i default sulla form
		/// <summary>
		/// InitializeSettings
		/// Imposta la visualizzazione dei controlli nella form, a seconda che si tratti di un db di sistema o aziendale
		/// </summary>
		/// <param name="showUnicodeCheck">useunicode visible</param>
		/// <param name="enableUnicodeCheck">useunicode enabled</param>
		/// <param name="disableDBCultureComboBox">disabilita la combo della DatabaseCulture</param>
		//---------------------------------------------------------------------
		private void InitializeSettings(bool showUnicodeCheck, bool enableUnicodeCheck, bool disableDBCultureComboBox)
		{
			// nascondo le tab che non mi servono a seconda del tipo di database
			if (isAzureSQLDatabase)
			{
				TabDataBaseConfiguration.TabPages.Remove(GeneralPage);
				TabDataBaseConfiguration.TabPages.Remove(DataPage);
				TabDataBaseConfiguration.TabPages.Remove(LogPage);
			}
			else
				TabDataBaseConfiguration.TabPages.Remove(AzurePage);

			if (string.Compare(System.Net.Dns.GetHostName(), serverName, StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				TextBoxDataPath.ReadOnly = true;
				TextBoxLogPath.ReadOnly = true;
				BtnPathLogSelect.Enabled = true;
				BtnPathDataSelect.Enabled = true;
			}
			else
			{
				TextBoxDataPath.ReadOnly = false;
				TextBoxLogPath.ReadOnly = false;
				BtnPathLogSelect.Enabled = false;
				BtnPathDataSelect.Enabled = false;
			}
			
			cbUseUnicode.Visible = CbAzureUseUnicode.Visible = showUnicodeCheck;
			cbUseUnicode.Enabled = CbAzureUseUnicode.Enabled = enableUnicodeCheck;
			cbUseUnicode.Checked = CbAzureUseUnicode.Checked = initUnicodeCheck;

			if (this.isCompanyDB)
			{
				// se si tratta di database aziendale e la licenza è SmallNetwork:
				// 1. impongo la maxsize del file di dati a 2048 MB (= 2GB)
				// 2. tutta la groupbox relativa alla Maximum file size diventa non modificabile
				if (dbNetworkType == DBNetworkType.Small)
				{
					RadioUnrestrictedGrowData.Enabled = false;
					RadioUnrestrictedGrowData.Checked = false;
					RadioRestrictedGrowData.Enabled = false;
					RadioRestrictedGrowData.Checked = true;
					MaxGrowFileDataUpDown.Enabled = false;
					MaxGrowFileDataUpDown.Value = 2048;
				}
			}
			else
				DatabaseCultureComboBox.Visible = ComboAzureDatabaseCulture.Visible = false; // non e' possibile scegliere la cultura per il database di sistema
			
			// carico i path del file di log e del file di dati, solo se db NON Azure
			if (!isAzureSQLDatabase)
			{
				DatabaseTask dbTask = new DatabaseTask();
				dbTask.GetSQLDataRootPath(serverName, instanceName, loginName, loginPassword, isWindowsAuth, out defaultPathDataFile, out defaultPathLogFile);
				TextBoxDataPath.Text = defaultPathDataFile;
				TextBoxLogPath.Text = defaultPathLogFile;
			}

			DatabaseCultureComboBox.Enabled = ComboAzureDatabaseCulture.Enabled = enableUnicodeCheck && !disableDBCultureComboBox;

			if (isAzureSQLDatabase)
			{ 
				// inizializzo la combo con le culture di database
				ComboAzureDatabaseCulture.LoadLanguages(true);
				ComboAzureDatabaseCulture.SetComboBoxText(DatabaseLayerStrings.DetailDBCulture);
				ComboAzureDatabaseCulture.ControlSetComboBoxWidth(320);
				ComboAzureDatabaseCulture.ApplicationLanguage = databaseCulture;
			}
			else
			{
				// inizializzo la combo con le culture di database
				DatabaseCultureComboBox.LoadLanguages(true);
				DatabaseCultureComboBox.SetComboBoxText(DatabaseLayerStrings.DetailDBCulture);
				DatabaseCultureComboBox.ControlSetComboBoxWidth(320);
				DatabaseCultureComboBox.ApplicationLanguage = databaseCulture;
			}

			this.Text = string.Format(Strings.CreateDBFormTitle, this.databaseName);

			if (isAzureSQLDatabase)
			{
				ComboAzureEdition.DataSource = AzureEditions.GetAzureEditions();
				ComboAzureEdition.DisplayMember = "EnumDescription";
				ComboAzureEdition.ValueMember = "EnumValue";
				ComboAzureEdition.SelectedValue = AzureEdition.Standard;

				FillAzureComboBoxes();
			}
		}
		#endregion

		#region CheckInputData - Controlla che i valori inseriti dall'utente siano corretti
		/// <summary>
		/// CheckInputData
		/// controlla che i valori inseriti dall'utente siano corretti
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckInputData()
		{
			bool isValidated = true;

			StringBuilder messageErrors = new StringBuilder();

			if (isAzureSQLDatabase)
			{
				if (ComboAzureEdition.SelectedItem == null)
				{
					messageErrors.AppendLine("Specify the edition!");
					isValidated = false;
				}
				if (ComboAzureServerLevel.SelectedItem == null || string.IsNullOrWhiteSpace((string)ComboAzureServerLevel.SelectedItem))
				{
					messageErrors.AppendLine("Specify the server level objective!");
					isValidated = false;
				}
				if (ComboAzureMaxSize.SelectedItem == null || string.IsNullOrWhiteSpace((string)ComboAzureMaxSize.SelectedItem))
				{
					messageErrors.AppendLine("Specify the maximum size!");
					isValidated = false;
				}
			}
			else
			{
				if (string.IsNullOrEmpty(TextBoxDataPath.Text))
				{
					messageErrors.AppendLine(Strings.DataFileNotFound);
					isValidated = false;
				}

				if (!string.IsNullOrEmpty(DataFileInitialSizeTextBox.Text))
					initialSizeData = Convert.ToInt32(DataFileInitialSizeTextBox.Text);

				if (!string.IsNullOrEmpty(LogFileInitialSizeTextBox.Text))
					initialSizeLog = Convert.ToInt32(LogFileInitialSizeTextBox.Text);

				// controllo l'ApplicationLanguage solo se è un db aziendale
				if (isCompanyDB)
				{
					// se la licenza è SmallNetwork ed è stata impostata la Size iniziale del file di dati
					// superiore ai 2048 MB (= 2GB) dò errore
					if (dbNetworkType == DBNetworkType.Small && initialSizeData > 2048)
					{
						initialSizeData = 0;
						DataFileInitialSizeTextBox.Text = string.Empty;
						messageErrors.AppendLine(Strings.MaxSizeLimitDataFile);
						isValidated = false;
					}
				}
			}

			// controllo l'ApplicationLanguage solo se è un db aziendale
			if (isCompanyDB)
			{
				if (string.IsNullOrEmpty(isAzureSQLDatabase ? ComboAzureDatabaseCulture.ApplicationLanguage : DatabaseCultureComboBox.ApplicationLanguage))
				{
					messageErrors.AppendLine(Strings.MissingDatabaseCulture);
					isValidated = false;
				}
			}

			if (!isValidated)
			{
				messageErrors.Append(Strings.AbortingDatabaseCreation);
				diagnosticViewer.Message = messageErrors.ToString();
				diagnosticViewer.Title = Strings.AbortingDbCreationTitle;
				diagnosticViewer.ShowIcon = MessageBoxIcon.Error;
				diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
				diagnosticViewer.Show();
			}

			return isValidated;
		}

		//---------------------------------------------------------------------
		private bool IsAValidSize(string sizeText)
		{
			if (string.IsNullOrWhiteSpace(sizeText))
				return true;

			Regex exp = new Regex("[0-9]*");
			Match m = exp.Match(sizeText);
			return (m.Success && m.Value.Equals(sizeText));
		}
		#endregion

		#region Funzionalità dei bottoni
		/// <summary>
		/// Premuto OK - Se i dati sono corretti chiudo la finestra e procedo
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			CreateDatabase();
		}

		/// <summary>
		/// Premuto il bottone di cancel
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			createDBDiagnostic.Set(DiagnosticType.Error, Strings.AbortingDatabaseCreation);
			operationResult = false;
			this.Close();
		}

		/// <summary>
		/// BtnRestoreDefault_Click
		/// Ripristina i valori di Default della maschera
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnRestoreDefault_Click(object sender, System.EventArgs e)
		{
			if (isAzureSQLDatabase)
			{
				ComboAzureEdition.SelectedValue = AzureEdition.Standard;
				FillAzureComboBoxes();
			}
			else
			{
				CbTruncateLog.Checked = true;
				CbAutoShrink.Checked = false;
				TextBoxDataPath.Text = DefaultPathDataFile;
				TextBoxLogPath.Text = DefaultPathLogFile;
				RadioPercentGrowDataFile.Checked = true;
				RadioPercentGrowLogFile.Checked = true;
				PercentDataGrowUpDown.Value = 10;
				PercentLogGrowUpDown.Value = 10;
				RadioUnrestrictedGrowData.Checked = true;
				RadioUnrestrictedGrowLog.Checked = true;
				MegabyteDataGrowUpDown.Value = 1;
				MegabyteLogGrowUpDown.Value = 1;
				cbUseUnicode.Checked = initUnicodeCheck;
				DataFileInitialSizeTextBox.Text = string.Empty;
				LogFileInitialSizeTextBox.Text = string.Empty;
				CbAzureUseUnicode.Checked = initUnicodeCheck;
			}
		}
		#endregion

		#region Settings per il file di data
		/// <summary>
		/// Browse il filesystem per selezionare una directory dove memorizzare il file dati
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnPathDataSelect_Click(object sender, System.EventArgs e)
		{
			//effettuando il browsing sulla macchina locale
			if (string.Compare(System.Net.Dns.GetHostName(), serverName, StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				OpenFolder browsingFilesystem = new OpenFolder();
				browsingFilesystem.ServerToBrowser = serverName;
				browsingFilesystem.Description = Strings.DirectorySelection;
				if (browsingFilesystem.ShowDialog() != DialogResult.Cancel)
				{
					if (browsingFilesystem.Path.Length > 0)
						TextBoxDataPath.Text = browsingFilesystem.Path;
				}
			}
		}

		/// <summary>
		/// Imposto la crescita del file di dati
		/// </summary>
		//---------------------------------------------------------------------
		private void RadioMegabyteGrowDataFile_CheckedChanged(object sender, System.EventArgs e)
		{
			MegabyteDataGrowUpDown.Enabled = ((RadioButton)sender).Checked;
			PercentDataGrowUpDown.Enabled = !(((RadioButton)sender).Checked);
		}

		/// <summary>
		/// Imposto la crescita Max per i file di dati
		/// </summary>
		//---------------------------------------------------------------------
		private void RadioRestrictedGrowData_CheckedChanged(object sender, System.EventArgs e)
		{
			MaxGrowFileDataUpDown.Enabled = ((RadioButton)sender).Checked;
		}

		///<summary>
		/// Imposto la perentuale di crescita
		///</summary>
		//---------------------------------------------------------------------------
		private void RadioPercentGrowDataFile_CheckedChanged(object sender, System.EventArgs e)
		{
			PercentDataGrowUpDown.Enabled = ((RadioButton)sender).Checked;
		}
		#endregion

		#region Settings per il file di log
		/// <summary>
		/// Browse il filesystem per selezionare una directory dove memorizzare il file log
		/// </summary>
		//---------------------------------------------------------------------
		private void BtnPathLogSelect_Click(object sender, System.EventArgs e)
		{
			//effettuando il browsing sulla macchina locale
			if (string.Compare(System.Net.Dns.GetHostName(), serverName, StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				OpenFolder browsingFilesystem = new OpenFolder();
				browsingFilesystem.ServerToBrowser = serverName;
				browsingFilesystem.Description = Strings.DirectorySelection;

				if (browsingFilesystem.ShowDialog() != DialogResult.Cancel)
				{
					if (browsingFilesystem.Path.Length > 0)
						TextBoxLogPath.Text = browsingFilesystem.Path;
				}
			}
		}

		/// <summary>
		/// Imposto la crescita del file di log
		/// </summary>
		//---------------------------------------------------------------------
		private void RadioMegabyteGrowLogFile_CheckedChanged(object sender, System.EventArgs e)
		{
			MegabyteLogGrowUpDown.Enabled = ((RadioButton)sender).Checked;
			PercentLogGrowUpDown.Enabled = !(((RadioButton)sender).Checked);
		}

		/// <summary>
		/// Imposto la crescita Max per il file di log
		/// </summary>
		//---------------------------------------------------------------------
		private void RadioRestrictedGrowLog_CheckedChanged(object sender, System.EventArgs e)
		{
			MaxGrowFileLogUpDown.Enabled = ((RadioButton)sender).Checked;
		}

		///<summary>
		/// Imposto la percentuale di crescita
		///</summary>
		//---------------------------------------------------------------------------
		private void RadioPercentGrowLogFile_CheckedChanged(object sender, System.EventArgs e)
		{
			PercentLogGrowUpDown.Enabled = ((RadioButton)sender).Checked;
		}
		#endregion

		# region Gestione dimensioni iniziali del file di dati e di log
		//---------------------------------------------------------------------
		private void DataFileInitialSizeTextBox_TextChanged(object sender, System.EventArgs e)
		{
			if (!IsAValidSize(((TextBox)sender).Text))
				((TextBox)sender).Text = string.Empty;
		}

		//---------------------------------------------------------------------
		private void LogFileInitialSizeTextBox_TextChanged(object sender, System.EventArgs e)
		{
			if (!IsAValidSize(((TextBox)sender).Text))
				((TextBox)sender).Text = string.Empty;
		}

		//---------------------------------------------------------------------
		private void DataFileInitialSizeTextBox_Leave(object sender, System.EventArgs e)
		{
			// se la stringa scritta contiene tutti zeri (fino ad un massimo di 9) allora forzo stringa vuota
			if (((TextBox)sender).Text == "0" || ((TextBox)sender).Text == "00" ||
				((TextBox)sender).Text == "000" || ((TextBox)sender).Text == "0000" ||
				((TextBox)sender).Text == "00000" || ((TextBox)sender).Text == "000000" ||
				((TextBox)sender).Text == "0000000" || ((TextBox)sender).Text == "0000000" ||
				((TextBox)sender).Text == "000000000")
				((TextBox)sender).Text = string.Empty;
		}

		//---------------------------------------------------------------------
		private void LogFileInitialSizeTextBox_Leave(object sender, System.EventArgs e)
		{
			// se la stringa scritta contiene tutti zeri (fino ad un massimo di 8) allora forzo stringa vuota
			if (((TextBox)sender).Text == "0" || ((TextBox)sender).Text == "00" ||
				((TextBox)sender).Text == "000" || ((TextBox)sender).Text == "0000" ||
				((TextBox)sender).Text == "00000" || ((TextBox)sender).Text == "000000" ||
				((TextBox)sender).Text == "0000000" || ((TextBox)sender).Text == "0000000")
				((TextBox)sender).Text = string.Empty;
		}
		#endregion

		# region Gestione combobox Azure
		//---------------------------------------------------------------------
		private void ComboAzureEdition_SelectionChangeCommitted(object sender, EventArgs e)
		{
			FillAzureComboBoxes();
		}

		//---------------------------------------------------------------------
		private void FillAzureComboBoxes()
		{
			AzureEdition azEdition = ComboAzureEdition.SelectedItem == null ? AzureEdition.Basic : (AzureEdition)((EnumComboItem)ComboAzureEdition.SelectedItem).EnumValue;

			ComboAzureServerLevel.BeginUpdate();
			ComboAzureServerLevel.DataSource = null;
			ComboAzureServerLevel.Items.Clear();
			List<string> levels = AzureServerLevelObjective.GetAzureServerLevelObjectiveByEdition(azEdition);
			if (levels != null)
			{
				ComboAzureServerLevel.DataSource = levels;

				switch (azEdition)
				{
					case AzureEdition.Basic:
						ComboAzureServerLevel.SelectedItem = AzureServerLevelObjective.Basic;
						break;
					case AzureEdition.Premium:
						ComboAzureServerLevel.SelectedItem = AzureServerLevelObjective.P1;
						break;
					case AzureEdition.Standard:
					default:
						ComboAzureServerLevel.SelectedItem = AzureServerLevelObjective.S2;
						break;
				}
			}
			ComboAzureServerLevel.EndUpdate();

			ComboAzureMaxSize.BeginUpdate();
			ComboAzureMaxSize.DataSource = null;
			ComboAzureMaxSize.Items.Clear();
			List<string> sizes = AzureMaxSize.GetAzureMaxSizeByEdition(azEdition);
			if (sizes != null)
			{
				ComboAzureMaxSize.DataSource = sizes;
				switch (azEdition)
				{
					case AzureEdition.Basic:
						ComboAzureMaxSize.SelectedItem = AzureMaxSize.GB2;
						break;
					case AzureEdition.Premium:
						ComboAzureMaxSize.SelectedItem = AzureMaxSize.GB500;
						break;
					case AzureEdition.Standard:
					default:
						ComboAzureMaxSize.SelectedItem = AzureMaxSize.GB250;
						break;
				}
			}
			ComboAzureMaxSize.EndUpdate();
		}
		#endregion

		/// <summary>
		/// Intercetto l'evento di Commit sulla ComboBox e inizializzo la variabile con la Culture specificata
		/// </summary>
		//---------------------------------------------------------------------
		private void DatabaseCultureComboBox_OnSelectionChangeCommitted(object sender, System.EventArgs e)
		{
			databaseCulture = DatabaseCultureComboBox.ApplicationLanguage;
		}

		//---------------------------------------------------------------------
		private void ComboAzureDatabaseCulture_OnSelectionChangeCommitted(object sender, EventArgs e)
		{
			databaseCulture = ComboAzureDatabaseCulture.ApplicationLanguage;
		}

		/// <summary>
		/// Evento intercettato alla fine dell'elaborazione di creazione db
		/// </summary>
		//---------------------------------------------------------------------
		private void CreateDbTask_OperationCompleted(object sender, DatabaseTaskEventArgs e)
		{
			elaborationIsRunning = false;

			if (e.Result)
			{
				operationResult = true;
				this.DialogResult = DialogResult.OK;
			}
			else
			{
				createDBDiagnostic.Set(e.DatabaseTaskDiagnostic);
				this.DialogResult = DialogResult.Cancel;
			}
		}

		/// <summary>
		/// Non consento di chiudere la form se l'elaborazione e' ancora in corso
		/// </summary>
		//---------------------------------------------------------------------
		private void CreateDBForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = elaborationIsRunning;
		}

		//---------------------------------------------------------------------
		private void EnableFormControls(bool enable)
		{
			Invoke(new MethodInvoker(() => 
			{
				// disabilito tutto il TabControl ed i controls
				TabDataBaseConfiguration.Enabled = enable;

				BtnRestoreDefault.Enabled = BtnRestoreDefault.Visible = enable;
				BtnOk.Enabled = BtnOk.Visible = enable;
				BtnCancel.Enabled = BtnCancel.Visible = enable;

				CircularProgress.Visible = !enable;
				LblProgress.Visible = CircularProgress.Visible;

				Cursor.Current = enable ? Cursors.WaitCursor : Cursors.Default;
				elaborationIsRunning = true;
			}
			));
		}

		///<summary>
		/// Metodo esposto dalla form per creare il database, leggendo i valori inseriti nella form
		/// Se richiamata esternamente senza fare la ShowDialog, consente di eseguire la creazione del
		/// database con i parametri per la creazione pre-caricati, in modo da eseguirla in modalita' silente.
		///</summary>
		//---------------------------------------------------------------------
		public void CreateDatabase()
		{
			databaseCulture = isAzureSQLDatabase ? ComboAzureDatabaseCulture.ApplicationLanguage : DatabaseCultureComboBox.ApplicationLanguage;

			// se c'e' qualche informazione mancante mi fermo e mostro la dialog
			if (!CheckInputData())
			{
				this.ShowDialog();
				return;
			}

			EnableFormControls(false);
			elaborationIsRunning = true;

			// per problemi di cross-thread metto i valori in variabili locali 
			string azureEditionCtrl		= (isAzureSQLDatabase) ? ((EnumComboItem)ComboAzureEdition.SelectedItem).EnumValue.ToString() : string.Empty;
			string azureServerLevelCtrl = (isAzureSQLDatabase) ? ComboAzureServerLevel.SelectedItem.ToString() : string.Empty;
			string azureMaxSizeCtrl		= (isAzureSQLDatabase) ? ComboAzureMaxSize.SelectedItem.ToString() : string.Empty;

			// Lancio l'elaborazione su un thread separato
			new Task(() =>
			{
				try
				{
					if (!string.IsNullOrEmpty(instanceName))
						serverName = Path.Combine(serverName, instanceName);

					string connString = string.Empty;

					if (isAzureSQLDatabase)
						connString = string.Format(NameSolverDatabaseStrings.SQLAzureConnection, serverName, DatabaseLayerConsts.MasterDatabase, loginName, loginPassword);
					else
						connString = (this.isWindowsAuth)
									? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, DatabaseLayerConsts.MasterDatabase)
									: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase, loginName, loginPassword);

					DatabaseTask createDbTask = new DatabaseTask();
					createDbTask.OperationCompleted += CreateDbTask_OperationCompleted;
					createDbTask.CurrentStringConnection = connString;

					if (isAzureSQLDatabase)
					{
						AzureCreateDBParameters createParams = new AzureCreateDBParameters();
						createParams.DatabaseName		= databaseName;
						createParams.Edition			= azureEditionCtrl;
						createParams.ServiceObjective	= azureServerLevelCtrl;
						createParams.MaxSize			= azureMaxSizeCtrl;
						createParams.AutoShrink			= IsAutoShrink;

						operationResult = createDbTask.CreateAzureDatabase(createParams);
					}
					else
					{
						SQLCreateDBParameters createParams		= new SQLCreateDBParameters();
						createParams.DatabaseName				= databaseName;
						createParams.DataPathFileName			= PathDataFile;
						createParams.DataFileGrowthByPercent	= IsPercentGrowDataFile;
						createParams.DataFileGrowth				= IsPercentGrowDataFile ? PercentDataGrow : MegabyteDataGrow;
						createParams.DataUnrestrictedFileGrowth = IsUnrestrictedGrowData;
						createParams.DataRestrictFileGrowthMB	= MaxGrowFileData;
						createParams.DataFileInitialSize		= InitialSizeData;
						createParams.LogPathFileName			= PathLogFile;
						createParams.LogFileGrowthByPercent		= IsPercentGrowLogFile;
						createParams.LogFileGrowth				= IsPercentGrowLogFile ? PercentLogGrow : MegabyteLogGrow;
						createParams.LogUnrestrictedFileGrowth	= IsUnrestrictedGrowLog;
						createParams.LogRestrictFileGrowthMB	= MaxGrowFileLog;
						createParams.LogFileInitialSize			= InitialSizeLog;
						createParams.TruncateLogFile			= IsTruncateLog;
						createParams.AutoShrink					= IsAutoShrink;

						operationResult = createDbTask.CreateSQLDatabase(createParams);
					}

					if (!operationResult)
						createDBDiagnostic.Set(createDbTask.Diagnostic);
				}
				catch (TBException ex)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(DatabaseLayerStrings.Description, ex.Message);
					extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.SysAdmin.Form.CreateDBForm");
					extendedInfo.Add(DatabaseLayerStrings.Function, "CreateDatabase");
					extendedInfo.Add(DatabaseLayerStrings.Source, ex.Source);
					extendedInfo.Add(DatabaseLayerStrings.StackTrace, ex.StackTrace);
					extendedInfo.Add(DatabaseLayerStrings.Number, ex.Number);
					extendedInfo.Add(DatabaseLayerStrings.Procedure, ex.Procedure);
					createDBDiagnostic.Set(DiagnosticType.Error, Strings.AbortingDatabaseCreation, extendedInfo);
				}
			})
			.Start();
		}

		//---------------------------------------------------------------------
		public void CreateDatabasePostgre()
		{
			databaseCulture = DatabaseCultureComboBox.ApplicationLanguage;

			try
			{
				if (!string.IsNullOrEmpty(instanceName))
					serverName = Path.Combine(serverName, instanceName);

				if (loginPassword.IsNullOrWhiteSpace())
					loginPassword = DatabaseLayerConsts.postgreDefaultPassword;

				string connString = (this.isWindowsAuth)
					? string.Format(NameSolverDatabaseStrings.PostgreWinNtConnection, serverName, port, DatabaseLayerConsts.postgreMasterDatabase, DatabaseLayerConsts.postgreDefaultSchema)
					: string.Format(NameSolverDatabaseStrings.PostgreConnection, serverName, port, DatabaseLayerConsts.postgreMasterDatabase, loginName.ToLower(), loginPassword, DatabaseLayerConsts.postgreDefaultSchema);

				DatabaseTask dbTask = new DatabaseTask();
				dbTask.CurrentStringConnection = connString;

				operationResult = dbTask.CreatePostgre(databaseName);

				if (!operationResult)
					createDBDiagnostic.Set(dbTask.Diagnostic);
			}
			catch (TBException ex)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, ex.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.SysAdmin.Form.CreateDBForm");
				extendedInfo.Add(DatabaseLayerStrings.Function, "CreateDatabasePostgre");
				extendedInfo.Add(DatabaseLayerStrings.Source, ex.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, ex.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, ex.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, ex.Procedure);
				createDBDiagnostic.Set(DiagnosticType.Error, Strings.AbortingDatabaseCreation, extendedInfo);
			}
		}
	}
}