using System.Windows.Forms;
using Microarea.TaskBuilderNet.UI.WinControls.Combo;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class CompanyLite

    {
        private System.ComponentModel.IContainer components;
        private Label lblCompany;
        private Label LblPreferredLanguage;
        private Label LabelTitle;
        private TextBox tbCompany;
        private TextBox tbCompanyId;
        private ComboBox cbProvider;
        private CheckBox cbUseTransaction;
        private CheckBox cbUseUnicode;
        private TabControl tabControl1;
        private TabPage Parametri;
        private Button BtnSave;
        private Button BtnDelete;
        private GroupBox UpdatedGroupBox;
        private RadioButton rbKey;
        private RadioButton rbPosition;
        private TabPage Anagrafica;
        private CheckBox UseAuditing;
        private CheckBox UseSecurity;
        private TextBox tbDescrizione;
        private Label lblDescrizione;
        private CheckBox CbCompanyDisabled;
        private CheckBox UseDataSynchro;

        private Label label2;
        private TabPage DBCompanySettings;
        private System.Windows.Forms.GroupBox DataSettingsGroupBox;
        private System.Windows.Forms.Label CollationLabel;
        private System.Windows.Forms.Label CollationDBLabel;
        private System.Windows.Forms.Label CultureLabel;
        private System.Windows.Forms.Label CultureDBLabel;
        private NativeCultureCombo cultureApplicationCombo;
        private CultureUICombo cultureUICombo;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Company));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Anagrafica = new System.Windows.Forms.TabPage();
            this.UseRowSecurityCBox = new System.Windows.Forms.CheckBox();
            this.DMSWarningLabel = new System.Windows.Forms.Label();
            this.UseDBSlave = new System.Windows.Forms.CheckBox();
            this.UseAuditing = new System.Windows.Forms.CheckBox();
            this.UseSecurity = new System.Windows.Forms.CheckBox();
            this.tbDescrizione = new System.Windows.Forms.TextBox();
            this.lblDescrizione = new System.Windows.Forms.Label();
            this.CbCompanyDisabled = new System.Windows.Forms.CheckBox();
            this.DBSizeGroupBox = new System.Windows.Forms.GroupBox();
            this.FreeSpaceLabel = new System.Windows.Forms.Label();
            this.DBSizePieChart = new Microarea.TaskBuilderNet.UI.WinControls.Others.PieChart2D();
            this.UsedSpaceLabel = new System.Windows.Forms.Label();
            this.FreeSpaceColor = new System.Windows.Forms.Label();
            this.UsedSpaceColor = new System.Windows.Forms.Label();
            this.DBCompanySettings = new System.Windows.Forms.TabPage();
            this.Parametri = new System.Windows.Forms.TabPage();
            this.DataSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.CultureDBLabel = new System.Windows.Forms.Label();
            this.CultureLabel = new System.Windows.Forms.Label();
            this.CollationDBLabel = new System.Windows.Forms.Label();
            this.CollationLabel = new System.Windows.Forms.Label();
            this.cbUseUnicode = new System.Windows.Forms.CheckBox();
            this.UpdatedGroupBox = new System.Windows.Forms.GroupBox();
            this.rbPosition = new System.Windows.Forms.RadioButton();
            this.rbKey = new System.Windows.Forms.RadioButton();
            this.LblPreferredLanguage = new System.Windows.Forms.Label();
            this.cbUseTransaction = new System.Windows.Forms.CheckBox();
            this.cultureUICombo = new Microarea.TaskBuilderNet.UI.WinControls.Combo.CultureUICombo();
            this.cultureApplicationCombo = new Microarea.TaskBuilderNet.UI.WinControls.Combo.NativeCultureCombo();
            this.DBSlave = new System.Windows.Forms.TabPage();
            this.SlavePanel = new System.Windows.Forms.Panel();
            this.cbProvider = new System.Windows.Forms.ComboBox();
            this.lblCompany = new System.Windows.Forms.Label();
            this.tbCompany = new System.Windows.Forms.TextBox();
            this.tbCompanyId = new System.Windows.Forms.TextBox();
            this.UseDataSynchro = new System.Windows.Forms.CheckBox();
            this.BtnSave = new System.Windows.Forms.Button();
            this.BtnDelete = new System.Windows.Forms.Button();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.Anagrafica.SuspendLayout();
            this.DBSizeGroupBox.SuspendLayout();
            this.Parametri.SuspendLayout();
            this.DataSettingsGroupBox.SuspendLayout();
            this.UpdatedGroupBox.SuspendLayout();
            this.DBSlave.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.Anagrafica);
            this.tabControl1.Controls.Add(this.DBCompanySettings);
            this.tabControl1.Controls.Add(this.Parametri);
            this.tabControl1.Controls.Add(this.DBSlave);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // Anagrafica
            // 
            resources.ApplyResources(this.Anagrafica, "Anagrafica");
            this.Anagrafica.Controls.Add(this.UseRowSecurityCBox);
            this.Anagrafica.Controls.Add(this.UseDataSynchro);
            this.Anagrafica.Controls.Add(this.DMSWarningLabel);
            this.Anagrafica.Controls.Add(this.UseDBSlave);
            this.Anagrafica.Controls.Add(this.UseAuditing);
            this.Anagrafica.Controls.Add(this.UseSecurity);
            this.Anagrafica.Controls.Add(this.tbDescrizione);
            this.Anagrafica.Controls.Add(this.lblDescrizione);
            this.Anagrafica.Controls.Add(this.CbCompanyDisabled);
            this.Anagrafica.Controls.Add(this.DBSizeGroupBox);
            this.Anagrafica.Name = "Anagrafica";
            // 
            // UseRowSecurityCBox
            // 
            resources.ApplyResources(this.UseRowSecurityCBox, "UseRowSecurityCBox");
            this.UseRowSecurityCBox.Name = "UseRowSecurityCBox";
            this.UseRowSecurityCBox.CheckedChanged += new System.EventHandler(this.UseRowSecurityCBox_CheckedChanged);
            // 
            // DMSWarningLabel
            // 
            this.DMSWarningLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.DMSWarningLabel, "DMSWarningLabel");
            this.DMSWarningLabel.ForeColor = System.Drawing.Color.Red;
            this.DMSWarningLabel.Name = "DMSWarningLabel";
            // 
            // UseDBSlave
            // 
            resources.ApplyResources(this.UseDBSlave, "UseDBSlave");
            this.UseDBSlave.Name = "UseDBSlave";
            this.UseDBSlave.CheckedChanged += new System.EventHandler(this.UseDBSlave_CheckedChanged);
            // 
            // UseAuditing
            // 
            resources.ApplyResources(this.UseAuditing, "UseAuditing");
            this.UseAuditing.Name = "UseAuditing";
            this.UseAuditing.CheckedChanged += new System.EventHandler(this.UseAuditing_CheckedChanged);
            // 
            // UseSecurity
            // 
            resources.ApplyResources(this.UseSecurity, "UseSecurity");
            this.UseSecurity.Name = "UseSecurity";
            this.UseSecurity.CheckedChanged += new System.EventHandler(this.UseSecurity_CheckedChanged);
            // 
            // tbDescrizione
            // 
            this.tbDescrizione.AcceptsReturn = true;
            this.tbDescrizione.AcceptsTab = true;
            resources.ApplyResources(this.tbDescrizione, "tbDescrizione");
            this.tbDescrizione.Name = "tbDescrizione";
            this.tbDescrizione.TextChanged += new System.EventHandler(this.tbDescrizione_TextChanged);
            // 
            // lblDescrizione
            // 
            this.lblDescrizione.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.lblDescrizione, "lblDescrizione");
            this.lblDescrizione.Name = "lblDescrizione";
            // 
            // CbCompanyDisabled
            // 
            resources.ApplyResources(this.CbCompanyDisabled, "CbCompanyDisabled");
            this.CbCompanyDisabled.Name = "CbCompanyDisabled";
            this.CbCompanyDisabled.CheckedChanged += new System.EventHandler(this.CbCompanyDisabled_CheckedChanged);
            // 
            // DBSizeGroupBox
            // 
            this.DBSizeGroupBox.Controls.Add(this.FreeSpaceLabel);
            this.DBSizeGroupBox.Controls.Add(this.DBSizePieChart);
            this.DBSizeGroupBox.Controls.Add(this.UsedSpaceLabel);
            this.DBSizeGroupBox.Controls.Add(this.FreeSpaceColor);
            this.DBSizeGroupBox.Controls.Add(this.UsedSpaceColor);
            resources.ApplyResources(this.DBSizeGroupBox, "DBSizeGroupBox");
            this.DBSizeGroupBox.Name = "DBSizeGroupBox";
            this.DBSizeGroupBox.TabStop = false;
            // 
            // FreeSpaceLabel
            // 
            resources.ApplyResources(this.FreeSpaceLabel, "FreeSpaceLabel");
            this.FreeSpaceLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.FreeSpaceLabel.Name = "FreeSpaceLabel";
            // 
            // DBSizePieChart
            // 
            resources.ApplyResources(this.DBSizePieChart, "DBSizePieChart");
            this.DBSizePieChart.Colors = new System.Drawing.Color[] {
        System.Drawing.Color.CornflowerBlue,
        System.Drawing.Color.Violet};
            this.DBSizePieChart.Name = "DBSizePieChart";
            this.DBSizePieChart.Slices = new float[] {
        40F,
        60F};
            // 
            // UsedSpaceLabel
            // 
            resources.ApplyResources(this.UsedSpaceLabel, "UsedSpaceLabel");
            this.UsedSpaceLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.UsedSpaceLabel.Name = "UsedSpaceLabel";
            // 
            // FreeSpaceColor
            // 
            this.FreeSpaceColor.BackColor = System.Drawing.Color.Violet;
            this.FreeSpaceColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.FreeSpaceColor, "FreeSpaceColor");
            this.FreeSpaceColor.Name = "FreeSpaceColor";
            // 
            // UsedSpaceColor
            // 
            this.UsedSpaceColor.BackColor = System.Drawing.Color.CornflowerBlue;
            this.UsedSpaceColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.UsedSpaceColor, "UsedSpaceColor");
            this.UsedSpaceColor.Name = "UsedSpaceColor";
            // 
            // DBCompanySettings
            // 
            resources.ApplyResources(this.DBCompanySettings, "DBCompanySettings");
            this.DBCompanySettings.Name = "DBCompanySettings";
            // 
            // Parametri
            // 
            resources.ApplyResources(this.Parametri, "Parametri");
            this.Parametri.Controls.Add(this.DataSettingsGroupBox);
            this.Parametri.Controls.Add(this.cbUseUnicode);
            this.Parametri.Controls.Add(this.UpdatedGroupBox);
            this.Parametri.Controls.Add(this.LblPreferredLanguage);
            this.Parametri.Controls.Add(this.cbUseTransaction);
            this.Parametri.Controls.Add(this.cultureUICombo);
            this.Parametri.Controls.Add(this.cultureApplicationCombo);
            this.Parametri.Name = "Parametri";
            // 
            // DataSettingsGroupBox
            // 
            this.DataSettingsGroupBox.Controls.Add(this.CultureDBLabel);
            this.DataSettingsGroupBox.Controls.Add(this.CultureLabel);
            this.DataSettingsGroupBox.Controls.Add(this.CollationDBLabel);
            this.DataSettingsGroupBox.Controls.Add(this.CollationLabel);
            this.DataSettingsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.DataSettingsGroupBox, "DataSettingsGroupBox");
            this.DataSettingsGroupBox.Name = "DataSettingsGroupBox";
            this.DataSettingsGroupBox.TabStop = false;
            // 
            // CultureDBLabel
            // 
            this.CultureDBLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CultureDBLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.CultureDBLabel, "CultureDBLabel");
            this.CultureDBLabel.Name = "CultureDBLabel";
            // 
            // UseDataSynchro
            // 
            resources.ApplyResources(this.UseDataSynchro, "UseDataSynchro");
            this.UseDataSynchro.Name = "UseDataSynchro";
            this.UseDataSynchro.CheckedChanged += new System.EventHandler(this.UseDataSynchro_CheckedChanged);

            // 
            // CultureLabel
            // 
            resources.ApplyResources(this.CultureLabel, "CultureLabel");
            this.CultureLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CultureLabel.Name = "CultureLabel";
            // 
            // CollationDBLabel
            // 
            this.CollationDBLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CollationDBLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.CollationDBLabel, "CollationDBLabel");
            this.CollationDBLabel.Name = "CollationDBLabel";
            // 
            // CollationLabel
            // 
            resources.ApplyResources(this.CollationLabel, "CollationLabel");
            this.CollationLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CollationLabel.Name = "CollationLabel";
            // 
            // cbUseUnicode
            // 
            resources.ApplyResources(this.cbUseUnicode, "cbUseUnicode");
            this.cbUseUnicode.Name = "cbUseUnicode";
            this.cbUseUnicode.CheckedChanged += new System.EventHandler(this.cbUseUnicode_CheckedChanged);
            // 
            // UpdatedGroupBox
            // 
            resources.ApplyResources(this.UpdatedGroupBox, "UpdatedGroupBox");
            this.UpdatedGroupBox.Controls.Add(this.rbPosition);
            this.UpdatedGroupBox.Controls.Add(this.rbKey);
            this.UpdatedGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.UpdatedGroupBox.Name = "UpdatedGroupBox";
            this.UpdatedGroupBox.TabStop = false;
            // 
            // rbPosition
            // 
            resources.ApplyResources(this.rbPosition, "rbPosition");
            this.rbPosition.Name = "rbPosition";
            this.rbPosition.TabStop = true;
            this.rbPosition.CheckedChanged += new System.EventHandler(this.rbPosition_CheckedChanged);
            // 
            // rbKey
            // 
            this.rbKey.Checked = true;
            resources.ApplyResources(this.rbKey, "rbKey");
            this.rbKey.Name = "rbKey";
            this.rbKey.TabStop = true;
            this.rbKey.CheckedChanged += new System.EventHandler(this.rbKey_CheckedChanged);
            // 
            // LblPreferredLanguage
            // 
            resources.ApplyResources(this.LblPreferredLanguage, "LblPreferredLanguage");
            this.LblPreferredLanguage.Name = "LblPreferredLanguage";
            // 
            // cbUseTransaction
            // 
            resources.ApplyResources(this.cbUseTransaction, "cbUseTransaction");
            this.cbUseTransaction.Name = "cbUseTransaction";
            this.cbUseTransaction.CheckedChanged += new System.EventHandler(this.cbUseTransaction_CheckedChanged);
            // 
            // cultureUICombo
            // 
            resources.ApplyResources(this.cultureUICombo, "cultureUICombo");
            this.cultureUICombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cultureUICombo.Name = "cultureUICombo";
            // 
            // cultureApplicationCombo
            // 
            this.cultureApplicationCombo.ApplicationLanguage = "";
            resources.ApplyResources(this.cultureApplicationCombo, "cultureApplicationCombo");
            this.cultureApplicationCombo.Name = "cultureApplicationCombo";
            // 
            // DBSlave
            // 
            this.DBSlave.BackColor = System.Drawing.SystemColors.Control;
            this.DBSlave.Controls.Add(this.SlavePanel);
            resources.ApplyResources(this.DBSlave, "DBSlave");
            this.DBSlave.Name = "DBSlave";
            // 
            // SlavePanel
            // 
            resources.ApplyResources(this.SlavePanel, "SlavePanel");
            this.SlavePanel.Name = "SlavePanel";
            // 
            // cbProvider
            // 
            resources.ApplyResources(this.cbProvider, "cbProvider");
            this.cbProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProvider.Name = "cbProvider";
            this.cbProvider.DropDown += new System.EventHandler(this.cbProvider_DropDown);
            this.cbProvider.SelectedIndexChanged += new System.EventHandler(this.cbProvider_SelectedIndexChanged);
            // 
            // lblCompany
            // 
            resources.ApplyResources(this.lblCompany, "lblCompany");
            this.lblCompany.Name = "lblCompany";
            // 
            // tbCompany
            // 
            this.tbCompany.AcceptsReturn = true;
            this.tbCompany.AcceptsTab = true;
            resources.ApplyResources(this.tbCompany, "tbCompany");
            this.tbCompany.Name = "tbCompany";
            this.tbCompany.Leave += new System.EventHandler(this.tbCompany_Leave);
            this.tbCompany.Validated += new System.EventHandler(this.tbCompany_Validated);
            // 
            // tbCompanyId
            // 
            resources.ApplyResources(this.tbCompanyId, "tbCompanyId");
            this.tbCompanyId.Name = "tbCompanyId";
            // 
            // BtnSave
            // 
            resources.ApplyResources(this.BtnSave, "BtnSave");
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // BtnDelete
            // 
            resources.ApplyResources(this.BtnDelete, "BtnDelete");
            this.BtnDelete.Name = "BtnDelete";
            this.BtnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // LabelTitle
            // 
            resources.ApplyResources(this.LabelTitle, "LabelTitle");
            this.LabelTitle.BackColor = System.Drawing.Color.CornflowerBlue;
            this.LabelTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LabelTitle.ForeColor = System.Drawing.Color.White;
            this.LabelTitle.Name = "LabelTitle";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // Company
            // 
            this.AcceptButton = this.BtnSave;
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbCompany);
            this.Controls.Add(this.tbCompanyId);
            this.Controls.Add(this.lblCompany);
            this.Controls.Add(this.LabelTitle);
            this.Controls.Add(this.BtnDelete);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.BtnSave);
            this.Controls.Add(this.cbProvider);
            this.Name = "Company";
            this.ShowInTaskbar = false;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Company_Closing);
            this.Deactivate += new System.EventHandler(this.Company_Deactivate);
            this.Load += new System.EventHandler(this.Company_Load);
            this.VisibleChanged += new System.EventHandler(this.Company_VisibleChanged);
            this.tabControl1.ResumeLayout(false);
            this.Anagrafica.ResumeLayout(false);
            this.Anagrafica.PerformLayout();
            this.DBSizeGroupBox.ResumeLayout(false);
            this.DBSizeGroupBox.PerformLayout();
            this.Parametri.ResumeLayout(false);
            this.Parametri.PerformLayout();
            this.DataSettingsGroupBox.ResumeLayout(false);
            this.DataSettingsGroupBox.PerformLayout();
            this.UpdatedGroupBox.ResumeLayout(false);
            this.DBSlave.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private Microarea.TaskBuilderNet.UI.WinControls.Others.PieChart2D DBSizePieChart;
        private Label FreeSpaceColor;
        private Label UsedSpaceColor;
        private Label FreeSpaceLabel;
        private Label UsedSpaceLabel;
        private GroupBox DBSizeGroupBox;
        private TabPage DBSlave;
        private Panel SlavePanel;
        private CheckBox UseDBSlave;
        private Label DMSWarningLabel;
        private CheckBox UseRowSecurityCBox;
    }
}
