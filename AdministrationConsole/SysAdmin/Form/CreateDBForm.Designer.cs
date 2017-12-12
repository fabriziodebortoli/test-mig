using System.Windows.Forms;
using Microarea.TaskBuilderNet.UI.WinControls.Combo;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	partial class CreateDBForm
    {
        private Button BtnOk;
        private Button BtnCancel;
        private Button BtnPathLogSelect;
        private Button BtnRestoreDefault;
        private Button BtnPathDataSelect;
        private TabControl TabDataBaseConfiguration;
        private TabPage GeneralPage;
        private TabPage DataPage;
        private TabPage LogPage;
        private TextBox TextBoxDataPath;
        private GroupBox GroupBoxGrowDataFile;
        private GroupBox GroupBoxDataProperties;
        private GroupBox GroupBoxLogProperties;
        private GroupBox GroupBoxGrowLogFile;
        private GroupBox GroupBoxMaxDimensionDataFile;
        private GroupBox GroupBoxMaxDimensionLogFile;
        private GroupBox GroupBoxGlobalSettings;
        private RadioButton RadioUnrestrictedGrowData;
        private RadioButton RadioRestrictedGrowData;
        private RadioButton RadioMegabyteGrowDataFile;
        private RadioButton RadioPercentGrowDataFile;
        private RadioButton RadioPercentGrowLogFile;
        private RadioButton RadioMegabyteGrowLogFile;
        private RadioButton RadioRestrictedGrowLog;
        private RadioButton RadioUnrestrictedGrowLog;
        private NumericUpDown MaxGrowFileDataUpDown;
        private NumericUpDown MegabyteDataGrowUpDown;
        private NumericUpDown PercentDataGrowUpDown;
        private NumericUpDown PercentLogGrowUpDown;
        private NumericUpDown MegabyteLogGrowUpDown;
        private NumericUpDown MaxGrowFileLogUpDown;
        private Label LblLogPath;
        private Label LblDataPath;
		private TextBox TextBoxLogPath;
        private CheckBox CbTruncateLog;
        private CheckBox CbAutoShrink;
		private CheckBox cbUseUnicode;
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.GroupBox DataFileGroupBox;
        private System.Windows.Forms.Label DataFileInitialSizeLabel;
        private System.Windows.Forms.TextBox DataFileInitialSizeTextBox;
        private System.Windows.Forms.GroupBox LogFileGroupBox;
        private System.Windows.Forms.Label LogFileInitialSizeLabel;
		private System.Windows.Forms.TextBox LogFileInitialSizeTextBox;
        private NativeCultureCombo DatabaseCultureComboBox;


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateDBForm));
			this.TabDataBaseConfiguration = new System.Windows.Forms.TabControl();
			this.GeneralPage = new System.Windows.Forms.TabPage();
			this.GroupBoxGlobalSettings = new System.Windows.Forms.GroupBox();
			this.DatabaseCultureComboBox = new Microarea.TaskBuilderNet.UI.WinControls.Combo.NativeCultureCombo();
			this.cbUseUnicode = new System.Windows.Forms.CheckBox();
			this.CbAutoShrink = new System.Windows.Forms.CheckBox();
			this.CbTruncateLog = new System.Windows.Forms.CheckBox();
			this.DataPage = new System.Windows.Forms.TabPage();
			this.GroupBoxDataProperties = new System.Windows.Forms.GroupBox();
			this.DataFileInitialSizeTextBox = new System.Windows.Forms.TextBox();
			this.DataFileInitialSizeLabel = new System.Windows.Forms.Label();
			this.GroupBoxGrowDataFile = new System.Windows.Forms.GroupBox();
			this.PercentDataGrowUpDown = new System.Windows.Forms.NumericUpDown();
			this.MegabyteDataGrowUpDown = new System.Windows.Forms.NumericUpDown();
			this.RadioPercentGrowDataFile = new System.Windows.Forms.RadioButton();
			this.RadioMegabyteGrowDataFile = new System.Windows.Forms.RadioButton();
			this.GroupBoxMaxDimensionDataFile = new System.Windows.Forms.GroupBox();
			this.MaxGrowFileDataUpDown = new System.Windows.Forms.NumericUpDown();
			this.RadioRestrictedGrowData = new System.Windows.Forms.RadioButton();
			this.RadioUnrestrictedGrowData = new System.Windows.Forms.RadioButton();
			this.DataFileGroupBox = new System.Windows.Forms.GroupBox();
			this.BtnPathDataSelect = new System.Windows.Forms.Button();
			this.TextBoxDataPath = new System.Windows.Forms.TextBox();
			this.LblDataPath = new System.Windows.Forms.Label();
			this.LogPage = new System.Windows.Forms.TabPage();
			this.BtnPathLogSelect = new System.Windows.Forms.Button();
			this.TextBoxLogPath = new System.Windows.Forms.TextBox();
			this.GroupBoxLogProperties = new System.Windows.Forms.GroupBox();
			this.GroupBoxGrowLogFile = new System.Windows.Forms.GroupBox();
			this.PercentLogGrowUpDown = new System.Windows.Forms.NumericUpDown();
			this.MegabyteLogGrowUpDown = new System.Windows.Forms.NumericUpDown();
			this.RadioPercentGrowLogFile = new System.Windows.Forms.RadioButton();
			this.RadioMegabyteGrowLogFile = new System.Windows.Forms.RadioButton();
			this.GroupBoxMaxDimensionLogFile = new System.Windows.Forms.GroupBox();
			this.MaxGrowFileLogUpDown = new System.Windows.Forms.NumericUpDown();
			this.RadioRestrictedGrowLog = new System.Windows.Forms.RadioButton();
			this.RadioUnrestrictedGrowLog = new System.Windows.Forms.RadioButton();
			this.LogFileGroupBox = new System.Windows.Forms.GroupBox();
			this.LogFileInitialSizeLabel = new System.Windows.Forms.Label();
			this.LogFileInitialSizeTextBox = new System.Windows.Forms.TextBox();
			this.LblLogPath = new System.Windows.Forms.Label();
			this.AzurePage = new System.Windows.Forms.TabPage();
			this.CbAzureAutoShrink = new System.Windows.Forms.CheckBox();
			this.ComboAzureDatabaseCulture = new Microarea.TaskBuilderNet.UI.WinControls.Combo.NativeCultureCombo();
			this.CbAzureUseUnicode = new System.Windows.Forms.CheckBox();
			this.ComboAzureMaxSize = new System.Windows.Forms.ComboBox();
			this.LblMaxSize = new System.Windows.Forms.Label();
			this.ComboAzureServerLevel = new System.Windows.Forms.ComboBox();
			this.LblServerLevel = new System.Windows.Forms.Label();
			this.ComboAzureEdition = new System.Windows.Forms.ComboBox();
			this.LblEdition = new System.Windows.Forms.Label();
			this.LblProgress = new System.Windows.Forms.Label();
			this.CircularProgress = new Microarea.TaskBuilderNet.UI.WinControls.CircularProgressBar();
			this.BtnOk = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnRestoreDefault = new System.Windows.Forms.Button();
			this.TabDataBaseConfiguration.SuspendLayout();
			this.GeneralPage.SuspendLayout();
			this.GroupBoxGlobalSettings.SuspendLayout();
			this.DataPage.SuspendLayout();
			this.GroupBoxDataProperties.SuspendLayout();
			this.GroupBoxGrowDataFile.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PercentDataGrowUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MegabyteDataGrowUpDown)).BeginInit();
			this.GroupBoxMaxDimensionDataFile.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MaxGrowFileDataUpDown)).BeginInit();
			this.LogPage.SuspendLayout();
			this.GroupBoxLogProperties.SuspendLayout();
			this.GroupBoxGrowLogFile.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PercentLogGrowUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MegabyteLogGrowUpDown)).BeginInit();
			this.GroupBoxMaxDimensionLogFile.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MaxGrowFileLogUpDown)).BeginInit();
			this.AzurePage.SuspendLayout();
			this.SuspendLayout();
			// 
			// TabDataBaseConfiguration
			// 
			resources.ApplyResources(this.TabDataBaseConfiguration, "TabDataBaseConfiguration");
			this.TabDataBaseConfiguration.Controls.Add(this.GeneralPage);
			this.TabDataBaseConfiguration.Controls.Add(this.DataPage);
			this.TabDataBaseConfiguration.Controls.Add(this.LogPage);
			this.TabDataBaseConfiguration.Controls.Add(this.AzurePage);
			this.TabDataBaseConfiguration.Name = "TabDataBaseConfiguration";
			this.TabDataBaseConfiguration.SelectedIndex = 0;
			// 
			// GeneralPage
			// 
			resources.ApplyResources(this.GeneralPage, "GeneralPage");
			this.GeneralPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.GeneralPage.Controls.Add(this.GroupBoxGlobalSettings);
			this.GeneralPage.Name = "GeneralPage";
			// 
			// GroupBoxGlobalSettings
			// 
			resources.ApplyResources(this.GroupBoxGlobalSettings, "GroupBoxGlobalSettings");
			this.GroupBoxGlobalSettings.Controls.Add(this.DatabaseCultureComboBox);
			this.GroupBoxGlobalSettings.Controls.Add(this.cbUseUnicode);
			this.GroupBoxGlobalSettings.Controls.Add(this.CbAutoShrink);
			this.GroupBoxGlobalSettings.Controls.Add(this.CbTruncateLog);
			this.GroupBoxGlobalSettings.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GroupBoxGlobalSettings.Name = "GroupBoxGlobalSettings";
			this.GroupBoxGlobalSettings.TabStop = false;
			// 
			// DatabaseCultureComboBox
			// 
			this.DatabaseCultureComboBox.ApplicationLanguage = "";
			resources.ApplyResources(this.DatabaseCultureComboBox, "DatabaseCultureComboBox");
			this.DatabaseCultureComboBox.Name = "DatabaseCultureComboBox";
			this.DatabaseCultureComboBox.OnSelectionChangeCommitted += new Microarea.TaskBuilderNet.UI.WinControls.Combo.NativeCultureCombo.SelectionChangeCommitted(this.DatabaseCultureComboBox_OnSelectionChangeCommitted);
			// 
			// cbUseUnicode
			// 
			resources.ApplyResources(this.cbUseUnicode, "cbUseUnicode");
			this.cbUseUnicode.Name = "cbUseUnicode";
			// 
			// CbAutoShrink
			// 
			resources.ApplyResources(this.CbAutoShrink, "CbAutoShrink");
			this.CbAutoShrink.Name = "CbAutoShrink";
			// 
			// CbTruncateLog
			// 
			this.CbTruncateLog.Checked = true;
			this.CbTruncateLog.CheckState = System.Windows.Forms.CheckState.Checked;
			resources.ApplyResources(this.CbTruncateLog, "CbTruncateLog");
			this.CbTruncateLog.Name = "CbTruncateLog";
			// 
			// DataPage
			// 
			resources.ApplyResources(this.DataPage, "DataPage");
			this.DataPage.Controls.Add(this.GroupBoxDataProperties);
			this.DataPage.Controls.Add(this.BtnPathDataSelect);
			this.DataPage.Controls.Add(this.TextBoxDataPath);
			this.DataPage.Controls.Add(this.LblDataPath);
			this.DataPage.Name = "DataPage";
			// 
			// GroupBoxDataProperties
			// 
			resources.ApplyResources(this.GroupBoxDataProperties, "GroupBoxDataProperties");
			this.GroupBoxDataProperties.Controls.Add(this.DataFileInitialSizeTextBox);
			this.GroupBoxDataProperties.Controls.Add(this.DataFileInitialSizeLabel);
			this.GroupBoxDataProperties.Controls.Add(this.GroupBoxGrowDataFile);
			this.GroupBoxDataProperties.Controls.Add(this.GroupBoxMaxDimensionDataFile);
			this.GroupBoxDataProperties.Controls.Add(this.DataFileGroupBox);
			this.GroupBoxDataProperties.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GroupBoxDataProperties.Name = "GroupBoxDataProperties";
			this.GroupBoxDataProperties.TabStop = false;
			// 
			// DataFileInitialSizeTextBox
			// 
			resources.ApplyResources(this.DataFileInitialSizeTextBox, "DataFileInitialSizeTextBox");
			this.DataFileInitialSizeTextBox.Name = "DataFileInitialSizeTextBox";
			this.DataFileInitialSizeTextBox.TextChanged += new System.EventHandler(this.DataFileInitialSizeTextBox_TextChanged);
			this.DataFileInitialSizeTextBox.Leave += new System.EventHandler(this.DataFileInitialSizeTextBox_Leave);
			// 
			// DataFileInitialSizeLabel
			// 
			this.DataFileInitialSizeLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.DataFileInitialSizeLabel, "DataFileInitialSizeLabel");
			this.DataFileInitialSizeLabel.Name = "DataFileInitialSizeLabel";
			// 
			// GroupBoxGrowDataFile
			// 
			this.GroupBoxGrowDataFile.Controls.Add(this.PercentDataGrowUpDown);
			this.GroupBoxGrowDataFile.Controls.Add(this.MegabyteDataGrowUpDown);
			this.GroupBoxGrowDataFile.Controls.Add(this.RadioPercentGrowDataFile);
			this.GroupBoxGrowDataFile.Controls.Add(this.RadioMegabyteGrowDataFile);
			this.GroupBoxGrowDataFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.GroupBoxGrowDataFile, "GroupBoxGrowDataFile");
			this.GroupBoxGrowDataFile.Name = "GroupBoxGrowDataFile";
			this.GroupBoxGrowDataFile.TabStop = false;
			// 
			// PercentDataGrowUpDown
			// 
			resources.ApplyResources(this.PercentDataGrowUpDown, "PercentDataGrowUpDown");
			this.PercentDataGrowUpDown.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.PercentDataGrowUpDown.Name = "PercentDataGrowUpDown";
			this.PercentDataGrowUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// MegabyteDataGrowUpDown
			// 
			resources.ApplyResources(this.MegabyteDataGrowUpDown, "MegabyteDataGrowUpDown");
			this.MegabyteDataGrowUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.MegabyteDataGrowUpDown.Name = "MegabyteDataGrowUpDown";
			this.MegabyteDataGrowUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// RadioPercentGrowDataFile
			// 
			this.RadioPercentGrowDataFile.Checked = true;
			resources.ApplyResources(this.RadioPercentGrowDataFile, "RadioPercentGrowDataFile");
			this.RadioPercentGrowDataFile.Name = "RadioPercentGrowDataFile";
			this.RadioPercentGrowDataFile.TabStop = true;
			this.RadioPercentGrowDataFile.CheckedChanged += new System.EventHandler(this.RadioPercentGrowDataFile_CheckedChanged);
			// 
			// RadioMegabyteGrowDataFile
			// 
			resources.ApplyResources(this.RadioMegabyteGrowDataFile, "RadioMegabyteGrowDataFile");
			this.RadioMegabyteGrowDataFile.Name = "RadioMegabyteGrowDataFile";
			this.RadioMegabyteGrowDataFile.CheckedChanged += new System.EventHandler(this.RadioMegabyteGrowDataFile_CheckedChanged);
			// 
			// GroupBoxMaxDimensionDataFile
			// 
			this.GroupBoxMaxDimensionDataFile.Controls.Add(this.MaxGrowFileDataUpDown);
			this.GroupBoxMaxDimensionDataFile.Controls.Add(this.RadioRestrictedGrowData);
			this.GroupBoxMaxDimensionDataFile.Controls.Add(this.RadioUnrestrictedGrowData);
			this.GroupBoxMaxDimensionDataFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.GroupBoxMaxDimensionDataFile, "GroupBoxMaxDimensionDataFile");
			this.GroupBoxMaxDimensionDataFile.Name = "GroupBoxMaxDimensionDataFile";
			this.GroupBoxMaxDimensionDataFile.TabStop = false;
			// 
			// MaxGrowFileDataUpDown
			// 
			resources.ApplyResources(this.MaxGrowFileDataUpDown, "MaxGrowFileDataUpDown");
			this.MaxGrowFileDataUpDown.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
			this.MaxGrowFileDataUpDown.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.MaxGrowFileDataUpDown.Name = "MaxGrowFileDataUpDown";
			this.MaxGrowFileDataUpDown.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			// 
			// RadioRestrictedGrowData
			// 
			resources.ApplyResources(this.RadioRestrictedGrowData, "RadioRestrictedGrowData");
			this.RadioRestrictedGrowData.Name = "RadioRestrictedGrowData";
			this.RadioRestrictedGrowData.CheckedChanged += new System.EventHandler(this.RadioRestrictedGrowData_CheckedChanged);
			// 
			// RadioUnrestrictedGrowData
			// 
			this.RadioUnrestrictedGrowData.Checked = true;
			resources.ApplyResources(this.RadioUnrestrictedGrowData, "RadioUnrestrictedGrowData");
			this.RadioUnrestrictedGrowData.Name = "RadioUnrestrictedGrowData";
			this.RadioUnrestrictedGrowData.TabStop = true;
			// 
			// DataFileGroupBox
			// 
			this.DataFileGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.DataFileGroupBox, "DataFileGroupBox");
			this.DataFileGroupBox.Name = "DataFileGroupBox";
			this.DataFileGroupBox.TabStop = false;
			// 
			// BtnPathDataSelect
			// 
			resources.ApplyResources(this.BtnPathDataSelect, "BtnPathDataSelect");
			this.BtnPathDataSelect.Name = "BtnPathDataSelect";
			this.BtnPathDataSelect.Click += new System.EventHandler(this.BtnPathDataSelect_Click);
			// 
			// TextBoxDataPath
			// 
			resources.ApplyResources(this.TextBoxDataPath, "TextBoxDataPath");
			this.TextBoxDataPath.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.TextBoxDataPath.Name = "TextBoxDataPath";
			// 
			// LblDataPath
			// 
			resources.ApplyResources(this.LblDataPath, "LblDataPath");
			this.LblDataPath.Name = "LblDataPath";
			// 
			// LogPage
			// 
			resources.ApplyResources(this.LogPage, "LogPage");
			this.LogPage.Controls.Add(this.BtnPathLogSelect);
			this.LogPage.Controls.Add(this.TextBoxLogPath);
			this.LogPage.Controls.Add(this.GroupBoxLogProperties);
			this.LogPage.Controls.Add(this.LblLogPath);
			this.LogPage.Name = "LogPage";
			// 
			// BtnPathLogSelect
			// 
			resources.ApplyResources(this.BtnPathLogSelect, "BtnPathLogSelect");
			this.BtnPathLogSelect.Name = "BtnPathLogSelect";
			this.BtnPathLogSelect.Click += new System.EventHandler(this.BtnPathLogSelect_Click);
			// 
			// TextBoxLogPath
			// 
			resources.ApplyResources(this.TextBoxLogPath, "TextBoxLogPath");
			this.TextBoxLogPath.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.TextBoxLogPath.Name = "TextBoxLogPath";
			// 
			// GroupBoxLogProperties
			// 
			resources.ApplyResources(this.GroupBoxLogProperties, "GroupBoxLogProperties");
			this.GroupBoxLogProperties.Controls.Add(this.GroupBoxGrowLogFile);
			this.GroupBoxLogProperties.Controls.Add(this.GroupBoxMaxDimensionLogFile);
			this.GroupBoxLogProperties.Controls.Add(this.LogFileGroupBox);
			this.GroupBoxLogProperties.Controls.Add(this.LogFileInitialSizeLabel);
			this.GroupBoxLogProperties.Controls.Add(this.LogFileInitialSizeTextBox);
			this.GroupBoxLogProperties.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GroupBoxLogProperties.Name = "GroupBoxLogProperties";
			this.GroupBoxLogProperties.TabStop = false;
			// 
			// GroupBoxGrowLogFile
			// 
			this.GroupBoxGrowLogFile.Controls.Add(this.PercentLogGrowUpDown);
			this.GroupBoxGrowLogFile.Controls.Add(this.MegabyteLogGrowUpDown);
			this.GroupBoxGrowLogFile.Controls.Add(this.RadioPercentGrowLogFile);
			this.GroupBoxGrowLogFile.Controls.Add(this.RadioMegabyteGrowLogFile);
			this.GroupBoxGrowLogFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.GroupBoxGrowLogFile, "GroupBoxGrowLogFile");
			this.GroupBoxGrowLogFile.Name = "GroupBoxGrowLogFile";
			this.GroupBoxGrowLogFile.TabStop = false;
			// 
			// PercentLogGrowUpDown
			// 
			resources.ApplyResources(this.PercentLogGrowUpDown, "PercentLogGrowUpDown");
			this.PercentLogGrowUpDown.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.PercentLogGrowUpDown.Name = "PercentLogGrowUpDown";
			this.PercentLogGrowUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// MegabyteLogGrowUpDown
			// 
			resources.ApplyResources(this.MegabyteLogGrowUpDown, "MegabyteLogGrowUpDown");
			this.MegabyteLogGrowUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.MegabyteLogGrowUpDown.Name = "MegabyteLogGrowUpDown";
			this.MegabyteLogGrowUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// RadioPercentGrowLogFile
			// 
			this.RadioPercentGrowLogFile.Checked = true;
			resources.ApplyResources(this.RadioPercentGrowLogFile, "RadioPercentGrowLogFile");
			this.RadioPercentGrowLogFile.Name = "RadioPercentGrowLogFile";
			this.RadioPercentGrowLogFile.TabStop = true;
			this.RadioPercentGrowLogFile.CheckedChanged += new System.EventHandler(this.RadioPercentGrowLogFile_CheckedChanged);
			// 
			// RadioMegabyteGrowLogFile
			// 
			resources.ApplyResources(this.RadioMegabyteGrowLogFile, "RadioMegabyteGrowLogFile");
			this.RadioMegabyteGrowLogFile.Name = "RadioMegabyteGrowLogFile";
			this.RadioMegabyteGrowLogFile.CheckedChanged += new System.EventHandler(this.RadioMegabyteGrowLogFile_CheckedChanged);
			// 
			// GroupBoxMaxDimensionLogFile
			// 
			this.GroupBoxMaxDimensionLogFile.Controls.Add(this.MaxGrowFileLogUpDown);
			this.GroupBoxMaxDimensionLogFile.Controls.Add(this.RadioRestrictedGrowLog);
			this.GroupBoxMaxDimensionLogFile.Controls.Add(this.RadioUnrestrictedGrowLog);
			this.GroupBoxMaxDimensionLogFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.GroupBoxMaxDimensionLogFile, "GroupBoxMaxDimensionLogFile");
			this.GroupBoxMaxDimensionLogFile.Name = "GroupBoxMaxDimensionLogFile";
			this.GroupBoxMaxDimensionLogFile.TabStop = false;
			// 
			// MaxGrowFileLogUpDown
			// 
			resources.ApplyResources(this.MaxGrowFileLogUpDown, "MaxGrowFileLogUpDown");
			this.MaxGrowFileLogUpDown.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
			this.MaxGrowFileLogUpDown.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.MaxGrowFileLogUpDown.Name = "MaxGrowFileLogUpDown";
			this.MaxGrowFileLogUpDown.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			// 
			// RadioRestrictedGrowLog
			// 
			resources.ApplyResources(this.RadioRestrictedGrowLog, "RadioRestrictedGrowLog");
			this.RadioRestrictedGrowLog.Name = "RadioRestrictedGrowLog";
			this.RadioRestrictedGrowLog.CheckedChanged += new System.EventHandler(this.RadioRestrictedGrowLog_CheckedChanged);
			// 
			// RadioUnrestrictedGrowLog
			// 
			this.RadioUnrestrictedGrowLog.Checked = true;
			resources.ApplyResources(this.RadioUnrestrictedGrowLog, "RadioUnrestrictedGrowLog");
			this.RadioUnrestrictedGrowLog.Name = "RadioUnrestrictedGrowLog";
			this.RadioUnrestrictedGrowLog.TabStop = true;
			// 
			// LogFileGroupBox
			// 
			this.LogFileGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LogFileGroupBox, "LogFileGroupBox");
			this.LogFileGroupBox.Name = "LogFileGroupBox";
			this.LogFileGroupBox.TabStop = false;
			// 
			// LogFileInitialSizeLabel
			// 
			this.LogFileInitialSizeLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LogFileInitialSizeLabel, "LogFileInitialSizeLabel");
			this.LogFileInitialSizeLabel.Name = "LogFileInitialSizeLabel";
			// 
			// LogFileInitialSizeTextBox
			// 
			resources.ApplyResources(this.LogFileInitialSizeTextBox, "LogFileInitialSizeTextBox");
			this.LogFileInitialSizeTextBox.Name = "LogFileInitialSizeTextBox";
			this.LogFileInitialSizeTextBox.TextChanged += new System.EventHandler(this.LogFileInitialSizeTextBox_TextChanged);
			this.LogFileInitialSizeTextBox.Leave += new System.EventHandler(this.LogFileInitialSizeTextBox_Leave);
			// 
			// LblLogPath
			// 
			resources.ApplyResources(this.LblLogPath, "LblLogPath");
			this.LblLogPath.Name = "LblLogPath";
			// 
			// AzurePage
			// 
			this.AzurePage.BackColor = System.Drawing.SystemColors.Control;
			this.AzurePage.Controls.Add(this.CbAzureAutoShrink);
			this.AzurePage.Controls.Add(this.ComboAzureDatabaseCulture);
			this.AzurePage.Controls.Add(this.CbAzureUseUnicode);
			this.AzurePage.Controls.Add(this.ComboAzureMaxSize);
			this.AzurePage.Controls.Add(this.LblMaxSize);
			this.AzurePage.Controls.Add(this.ComboAzureServerLevel);
			this.AzurePage.Controls.Add(this.LblServerLevel);
			this.AzurePage.Controls.Add(this.ComboAzureEdition);
			this.AzurePage.Controls.Add(this.LblEdition);
			resources.ApplyResources(this.AzurePage, "AzurePage");
			this.AzurePage.Name = "AzurePage";
			// 
			// CbAzureAutoShrink
			// 
			resources.ApplyResources(this.CbAzureAutoShrink, "CbAzureAutoShrink");
			this.CbAzureAutoShrink.Name = "CbAzureAutoShrink";
			// 
			// ComboAzureDatabaseCulture
			// 
			this.ComboAzureDatabaseCulture.ApplicationLanguage = "";
			resources.ApplyResources(this.ComboAzureDatabaseCulture, "ComboAzureDatabaseCulture");
			this.ComboAzureDatabaseCulture.Name = "ComboAzureDatabaseCulture";
			this.ComboAzureDatabaseCulture.OnSelectionChangeCommitted += new Microarea.TaskBuilderNet.UI.WinControls.Combo.NativeCultureCombo.SelectionChangeCommitted(this.ComboAzureDatabaseCulture_OnSelectionChangeCommitted);
			// 
			// CbAzureUseUnicode
			// 
			resources.ApplyResources(this.CbAzureUseUnicode, "CbAzureUseUnicode");
			this.CbAzureUseUnicode.Name = "CbAzureUseUnicode";
			// 
			// ComboAzureMaxSize
			// 
			this.ComboAzureMaxSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboAzureMaxSize.FormattingEnabled = true;
			resources.ApplyResources(this.ComboAzureMaxSize, "ComboAzureMaxSize");
			this.ComboAzureMaxSize.Name = "ComboAzureMaxSize";
			// 
			// LblMaxSize
			// 
			resources.ApplyResources(this.LblMaxSize, "LblMaxSize");
			this.LblMaxSize.Name = "LblMaxSize";
			// 
			// ComboAzureServerLevel
			// 
			this.ComboAzureServerLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboAzureServerLevel.FormattingEnabled = true;
			resources.ApplyResources(this.ComboAzureServerLevel, "ComboAzureServerLevel");
			this.ComboAzureServerLevel.Name = "ComboAzureServerLevel";
			// 
			// LblServerLevel
			// 
			resources.ApplyResources(this.LblServerLevel, "LblServerLevel");
			this.LblServerLevel.Name = "LblServerLevel";
			// 
			// ComboAzureEdition
			// 
			this.ComboAzureEdition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboAzureEdition.FormattingEnabled = true;
			this.ComboAzureEdition.Items.AddRange(new object[] {
            resources.GetString("ComboAzureEdition.Items"),
            resources.GetString("ComboAzureEdition.Items1"),
            resources.GetString("ComboAzureEdition.Items2")});
			resources.ApplyResources(this.ComboAzureEdition, "ComboAzureEdition");
			this.ComboAzureEdition.Name = "ComboAzureEdition";
			this.ComboAzureEdition.SelectionChangeCommitted += new System.EventHandler(this.ComboAzureEdition_SelectionChangeCommitted);
			// 
			// LblEdition
			// 
			resources.ApplyResources(this.LblEdition, "LblEdition");
			this.LblEdition.Name = "LblEdition";
			// 
			// LblProgress
			// 
			this.LblProgress.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblProgress, "LblProgress");
			this.LblProgress.Name = "LblProgress";
			// 
			// CircularProgress
			// 
			this.CircularProgress.ActiveSegmentColour = System.Drawing.SystemColors.Highlight;
			this.CircularProgress.AutoIncrementFrequency = 100D;
			this.CircularProgress.BehindTransistionSegmentIsActive = false;
			resources.ApplyResources(this.CircularProgress, "CircularProgress");
			this.CircularProgress.Name = "CircularProgress";
			this.CircularProgress.TransistionSegment = 6;
			this.CircularProgress.TransistionSegmentColour = System.Drawing.SystemColors.ActiveCaption;
			// 
			// BtnOk
			// 
			resources.ApplyResources(this.BtnOk, "BtnOk");
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// BtnCancel
			// 
			resources.ApplyResources(this.BtnCancel, "BtnCancel");
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// BtnRestoreDefault
			// 
			resources.ApplyResources(this.BtnRestoreDefault, "BtnRestoreDefault");
			this.BtnRestoreDefault.Name = "BtnRestoreDefault";
			this.BtnRestoreDefault.Click += new System.EventHandler(this.BtnRestoreDefault_Click);
			// 
			// CreateDBForm
			// 
			this.AcceptButton = this.BtnOk;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.CircularProgress);
			this.Controls.Add(this.BtnRestoreDefault);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.TabDataBaseConfiguration);
			this.Controls.Add(this.LblProgress);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CreateDBForm";
			this.ShowInTaskbar = false;
			this.TopMost = true;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CreateDBForm_FormClosing);
			this.TabDataBaseConfiguration.ResumeLayout(false);
			this.GeneralPage.ResumeLayout(false);
			this.GroupBoxGlobalSettings.ResumeLayout(false);
			this.DataPage.ResumeLayout(false);
			this.DataPage.PerformLayout();
			this.GroupBoxDataProperties.ResumeLayout(false);
			this.GroupBoxDataProperties.PerformLayout();
			this.GroupBoxGrowDataFile.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PercentDataGrowUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MegabyteDataGrowUpDown)).EndInit();
			this.GroupBoxMaxDimensionDataFile.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MaxGrowFileDataUpDown)).EndInit();
			this.LogPage.ResumeLayout(false);
			this.LogPage.PerformLayout();
			this.GroupBoxLogProperties.ResumeLayout(false);
			this.GroupBoxLogProperties.PerformLayout();
			this.GroupBoxGrowLogFile.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PercentLogGrowUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MegabyteLogGrowUpDown)).EndInit();
			this.GroupBoxMaxDimensionLogFile.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MaxGrowFileLogUpDown)).EndInit();
			this.AzurePage.ResumeLayout(false);
			this.AzurePage.PerformLayout();
			this.ResumeLayout(false);

        }
		#endregion

		private TabPage AzurePage;
		private Label LblEdition;
		private ComboBox ComboAzureEdition;
		private ComboBox ComboAzureServerLevel;
		private Label LblServerLevel;
		private ComboBox ComboAzureMaxSize;
		private Label LblMaxSize;
		private CheckBox CbAzureUseUnicode;
		private NativeCultureCombo ComboAzureDatabaseCulture;
		private CheckBox CbAzureAutoShrink;
		private TaskBuilderNet.UI.WinControls.CircularProgressBar CircularProgress;
		private Label LblProgress;
	}
}
