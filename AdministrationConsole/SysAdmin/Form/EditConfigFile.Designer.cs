using System.Windows.Forms;
using Microarea.TaskBuilderNet.UI.WinControls.Combo;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class EditConfigFile
    {
        private Label LabelTitle;
        private Label LblPreferredLanguage;
        private GroupBox GroupBoxLanguage;
        private GroupBox GroupPolicyPwdUser;
        private NumericUpDown nudMinPwdLenght;
        private Label LblPwdMinLenght;
        private Label LblPwdDuration;
        private NumericUpDown nudPwdTimeLenght;
        private NumericUpDown nudMaxTbLoader;
        private NumericUpDown nudMaxLoginFailed;
        private CheckBox cbUseStrongPwd;
        public ToolTip toolTip;
        private Label LblNrMaxLoginFailed;
        private Label LblMaxTbLoader;
        private GroupBox groupBox1;
        private Label label2;
        private Label label3;
        private NumericUpDown nudWebServicesPort;
        private NumericUpDown nudWebServicesTimeOut;
        private Label label4;
        private NativeCultureCombo cultureApplicationCombo;
        private System.Windows.Forms.GroupBox SMTPGroupBox;
        private System.Windows.Forms.Label SMTPRelayServerLabel;
        private System.Windows.Forms.TextBox SMTPRelayServerTextBox;
        private CheckBox SMTPUseSpecificCredentialsCheckBox;
        private Label PasswordLabel;
        private Label UserNameLabel;
        private Label DomainLabel;
        private TextBox SMTPPasswordTextBox;
        private TextBox SMTPUserNameTextBox;
        private Label PortLabel;
        private NumericUpDown SMTPPortNumericUpDown;
        private ComboBox SMTPDomainsComboBox;
        private CultureUICombo cultureUICombo;

        private System.ComponentModel.IContainer components;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditConfigFile));
            this.LblPreferredLanguage = new System.Windows.Forms.Label();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.GroupBoxLanguage = new System.Windows.Forms.GroupBox();
            this.cultureUICombo = new Microarea.TaskBuilderNet.UI.WinControls.Combo.CultureUICombo();
            this.cultureApplicationCombo = new Microarea.TaskBuilderNet.UI.WinControls.Combo.NativeCultureCombo();
            this.label4 = new System.Windows.Forms.Label();
            this.GroupPolicyPwdUser = new System.Windows.Forms.GroupBox();
            this.nudMaxLoginFailed = new System.Windows.Forms.NumericUpDown();
            this.LblNrMaxLoginFailed = new System.Windows.Forms.Label();
            this.nudPwdTimeLenght = new System.Windows.Forms.NumericUpDown();
            this.LblPwdDuration = new System.Windows.Forms.Label();
            this.LblPwdMinLenght = new System.Windows.Forms.Label();
            this.nudMinPwdLenght = new System.Windows.Forms.NumericUpDown();
            this.cbUseStrongPwd = new System.Windows.Forms.CheckBox();
            this.CbAutologin = new System.Windows.Forms.CheckBox();
            this.PbAutologin = new System.Windows.Forms.PictureBox();
            this.nudTBLoaderTimeout = new System.Windows.Forms.NumericUpDown();
            this.nudMaxLoginPerTBLoader = new System.Windows.Forms.NumericUpDown();
            this.LblTBLoaderTimeout = new System.Windows.Forms.Label();
            this.LblMaxLoginPerTBLoader = new System.Windows.Forms.Label();
            this.nudMaxTbLoader = new System.Windows.Forms.NumericUpDown();
            this.LblMaxTbLoader = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.LblDefaultWcfCallTimeout = new System.Windows.Forms.Label();
            this.LblDefaultDataTransferWcfCall = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.LMLogCheckBox = new System.Windows.Forms.CheckBox();
            this.LblEnableVerboseLog = new System.Windows.Forms.Label();
            this.nudWebServicesTimeOut = new System.Windows.Forms.NumericUpDown();
            this.nudWebServicesPort = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.EALogCheckBox = new System.Windows.Forms.CheckBox();
            this.SMTPGroupBox = new System.Windows.Forms.GroupBox();
            this.SMTPFrom = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SMTPPortNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.SMTPDomainsComboBox = new System.Windows.Forms.ComboBox();
            this.SMTPPasswordTextBox = new System.Windows.Forms.TextBox();
            this.SMTPUserNameTextBox = new System.Windows.Forms.TextBox();
            this.PortLabel = new System.Windows.Forms.Label();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.UserNameLabel = new System.Windows.Forms.Label();
            this.DomainLabel = new System.Windows.Forms.Label();
            this.SMTPUseSSLCheckBox = new System.Windows.Forms.CheckBox();
            this.SMTPUseSpecificCredentialsCheckBox = new System.Windows.Forms.CheckBox();
            this.SMTPRelayServerTextBox = new System.Windows.Forms.TextBox();
            this.SMTPRelayServerLabel = new System.Windows.Forms.Label();
            this.GroupTBLoaderParameters = new System.Windows.Forms.GroupBox();
            this.nudTBWCFDataTransferTimeout = new System.Windows.Forms.NumericUpDown();
            this.nudTBWCFDefaultTimeout = new System.Windows.Forms.NumericUpDown();
            this.LblDBSizeMaxLevel = new System.Windows.Forms.Label();
            this.GBoxOtherSettings = new System.Windows.Forms.GroupBox();
            this.NudDBSizeLevel = new System.Windows.Forms.NumericUpDown();
            this.TxtPingMailRecipient = new System.Windows.Forms.TextBox();
            this.CkbSendPingMail = new System.Windows.Forms.CheckBox();
            this.GroupBoxLanguage.SuspendLayout();
            this.GroupPolicyPwdUser.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxLoginFailed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPwdTimeLenght)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinPwdLenght)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbAutologin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTBLoaderTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxLoginPerTBLoader)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxTbLoader)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudWebServicesTimeOut)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWebServicesPort)).BeginInit();
            this.SMTPGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SMTPPortNumericUpDown)).BeginInit();
            this.GroupTBLoaderParameters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTBWCFDataTransferTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTBWCFDefaultTimeout)).BeginInit();
            this.GBoxOtherSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NudDBSizeLevel)).BeginInit();
            this.SuspendLayout();
            // 
            // LblPreferredLanguage
            // 
            resources.ApplyResources(this.LblPreferredLanguage, "LblPreferredLanguage");
            this.LblPreferredLanguage.Name = "LblPreferredLanguage";
            // 
            // LabelTitle
            // 
            this.LabelTitle.BackColor = System.Drawing.Color.CornflowerBlue;
            this.LabelTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.LabelTitle, "LabelTitle");
            this.LabelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LabelTitle.ForeColor = System.Drawing.Color.White;
            this.LabelTitle.Name = "LabelTitle";
            // 
            // GroupBoxLanguage
            // 
            resources.ApplyResources(this.GroupBoxLanguage, "GroupBoxLanguage");
            this.GroupBoxLanguage.Controls.Add(this.cultureUICombo);
            this.GroupBoxLanguage.Controls.Add(this.cultureApplicationCombo);
            this.GroupBoxLanguage.Controls.Add(this.label4);
            this.GroupBoxLanguage.Controls.Add(this.LblPreferredLanguage);
            this.GroupBoxLanguage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GroupBoxLanguage.Name = "GroupBoxLanguage";
            this.GroupBoxLanguage.TabStop = false;
            // 
            // cultureUICombo
            // 
            this.cultureUICombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cultureUICombo.FormattingEnabled = true;
            resources.ApplyResources(this.cultureUICombo, "cultureUICombo");
            this.cultureUICombo.Name = "cultureUICombo";
            this.cultureUICombo.SelectedIndexChanged += new System.EventHandler(this.cultureUICombo_SelectedIndexChanged);
            // 
            // cultureApplicationCombo
            // 
            this.cultureApplicationCombo.ApplicationLanguage = "";
            resources.ApplyResources(this.cultureApplicationCombo, "cultureApplicationCombo");
            this.cultureApplicationCombo.Name = "cultureApplicationCombo";
            this.cultureApplicationCombo.OnSelectionChangeCommitted += new Microarea.TaskBuilderNet.UI.WinControls.Combo.NativeCultureCombo.SelectionChangeCommitted(this.cultureApplicationCombo_OnSelectionChangeCommitted);
            // 
            // label4
            // 
            this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // GroupPolicyPwdUser
            // 
            resources.ApplyResources(this.GroupPolicyPwdUser, "GroupPolicyPwdUser");
            this.GroupPolicyPwdUser.Controls.Add(this.nudMaxLoginFailed);
            this.GroupPolicyPwdUser.Controls.Add(this.LblNrMaxLoginFailed);
            this.GroupPolicyPwdUser.Controls.Add(this.nudPwdTimeLenght);
            this.GroupPolicyPwdUser.Controls.Add(this.LblPwdDuration);
            this.GroupPolicyPwdUser.Controls.Add(this.LblPwdMinLenght);
            this.GroupPolicyPwdUser.Controls.Add(this.nudMinPwdLenght);
            this.GroupPolicyPwdUser.Controls.Add(this.cbUseStrongPwd);
            this.GroupPolicyPwdUser.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GroupPolicyPwdUser.Name = "GroupPolicyPwdUser";
            this.GroupPolicyPwdUser.TabStop = false;
            // 
            // nudMaxLoginFailed
            // 
            resources.ApplyResources(this.nudMaxLoginFailed, "nudMaxLoginFailed");
            this.nudMaxLoginFailed.Name = "nudMaxLoginFailed";
            this.nudMaxLoginFailed.Validating += new System.ComponentModel.CancelEventHandler(this.NudMaxLoginFailed_Validating);
            // 
            // LblNrMaxLoginFailed
            // 
            resources.ApplyResources(this.LblNrMaxLoginFailed, "LblNrMaxLoginFailed");
            this.LblNrMaxLoginFailed.Name = "LblNrMaxLoginFailed";
            // 
            // nudPwdTimeLenght
            // 
            resources.ApplyResources(this.nudPwdTimeLenght, "nudPwdTimeLenght");
            this.nudPwdTimeLenght.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.nudPwdTimeLenght.Name = "nudPwdTimeLenght";
            this.nudPwdTimeLenght.Validating += new System.ComponentModel.CancelEventHandler(this.NudPwdTimeLenght_Validating);
            // 
            // LblPwdDuration
            // 
            resources.ApplyResources(this.LblPwdDuration, "LblPwdDuration");
            this.LblPwdDuration.Name = "LblPwdDuration";
            // 
            // LblPwdMinLenght
            // 
            resources.ApplyResources(this.LblPwdMinLenght, "LblPwdMinLenght");
            this.LblPwdMinLenght.Name = "LblPwdMinLenght";
            // 
            // nudMinPwdLenght
            // 
            resources.ApplyResources(this.nudMinPwdLenght, "nudMinPwdLenght");
            this.nudMinPwdLenght.Name = "nudMinPwdLenght";
            this.nudMinPwdLenght.Validating += new System.ComponentModel.CancelEventHandler(this.MinPwdLenght_Validating);
            // 
            // cbUseStrongPwd
            // 
            resources.ApplyResources(this.cbUseStrongPwd, "cbUseStrongPwd");
            this.cbUseStrongPwd.Checked = true;
            this.cbUseStrongPwd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbUseStrongPwd.Name = "cbUseStrongPwd";
            this.cbUseStrongPwd.CheckedChanged += new System.EventHandler(this.cbUseStrongPwd_CheckedChanged);
            // 
            // CbAutologin
            // 
            resources.ApplyResources(this.CbAutologin, "CbAutologin");
            this.CbAutologin.Name = "CbAutologin";
            this.CbAutologin.UseVisualStyleBackColor = true;
            this.CbAutologin.CheckedChanged += new System.EventHandler(this.CbAutologin_CheckedChanged);
            // 
            // PbAutologin
            // 
            resources.ApplyResources(this.PbAutologin, "PbAutologin");
            this.PbAutologin.Name = "PbAutologin";
            this.PbAutologin.TabStop = false;
            // 
            // nudTBLoaderTimeout
            // 
            resources.ApplyResources(this.nudTBLoaderTimeout, "nudTBLoaderTimeout");
            this.nudTBLoaderTimeout.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudTBLoaderTimeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.nudTBLoaderTimeout.Name = "nudTBLoaderTimeout";
            this.nudTBLoaderTimeout.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.nudTBLoaderTimeout.Validating += new System.ComponentModel.CancelEventHandler(this.NudTBLoaderTimeout_Validating);
            // 
            // nudMaxLoginPerTBLoader
            // 
            resources.ApplyResources(this.nudMaxLoginPerTBLoader, "nudMaxLoginPerTBLoader");
            this.nudMaxLoginPerTBLoader.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.nudMaxLoginPerTBLoader.Name = "nudMaxLoginPerTBLoader";
            this.nudMaxLoginPerTBLoader.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudMaxLoginPerTBLoader.Validating += new System.ComponentModel.CancelEventHandler(this.NudMaxLoginPerTBLoader_Validating);
            // 
            // LblTBLoaderTimeout
            // 
            resources.ApplyResources(this.LblTBLoaderTimeout, "LblTBLoaderTimeout");
            this.LblTBLoaderTimeout.Name = "LblTBLoaderTimeout";
            this.toolTip.SetToolTip(this.LblTBLoaderTimeout, resources.GetString("LblTBLoaderTimeout.ToolTip"));
            // 
            // LblMaxLoginPerTBLoader
            // 
            resources.ApplyResources(this.LblMaxLoginPerTBLoader, "LblMaxLoginPerTBLoader");
            this.LblMaxLoginPerTBLoader.Name = "LblMaxLoginPerTBLoader";
            // 
            // nudMaxTbLoader
            // 
            resources.ApplyResources(this.nudMaxTbLoader, "nudMaxTbLoader");
            this.nudMaxTbLoader.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.nudMaxTbLoader.Name = "nudMaxTbLoader";
            this.nudMaxTbLoader.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudMaxTbLoader.Validating += new System.ComponentModel.CancelEventHandler(this.NudMaxTbLoader_Validating);
            // 
            // LblMaxTbLoader
            // 
            resources.ApplyResources(this.LblMaxTbLoader, "LblMaxTbLoader");
            this.LblMaxTbLoader.Name = "LblMaxTbLoader";
            // 
            // toolTip
            // 
            this.toolTip.ShowAlways = true;
            // 
            // LblDefaultWcfCallTimeout
            // 
            resources.ApplyResources(this.LblDefaultWcfCallTimeout, "LblDefaultWcfCallTimeout");
            this.LblDefaultWcfCallTimeout.Name = "LblDefaultWcfCallTimeout";
            this.toolTip.SetToolTip(this.LblDefaultWcfCallTimeout, resources.GetString("LblDefaultWcfCallTimeout.ToolTip"));
            // 
            // LblDefaultDataTransferWcfCall
            // 
            resources.ApplyResources(this.LblDefaultDataTransferWcfCall, "LblDefaultDataTransferWcfCall");
            this.LblDefaultDataTransferWcfCall.Name = "LblDefaultDataTransferWcfCall";
            this.toolTip.SetToolTip(this.LblDefaultDataTransferWcfCall, resources.GetString("LblDefaultDataTransferWcfCall.ToolTip"));
            // 
            // numericUpDown1
            // 
            resources.ApplyResources(this.numericUpDown1, "numericUpDown1");
            this.numericUpDown1.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.toolTip.SetToolTip(this.numericUpDown1, resources.GetString("numericUpDown1.ToolTip"));
            this.numericUpDown1.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.numericUpDown1.Validating += new System.ComponentModel.CancelEventHandler(this.NudWeMSCAL_Validating);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.numericUpDown1);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.LMLogCheckBox);
            this.groupBox1.Controls.Add(this.LblEnableVerboseLog);
            this.groupBox1.Controls.Add(this.nudWebServicesTimeOut);
            this.groupBox1.Controls.Add(this.nudWebServicesPort);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.EALogCheckBox);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // LMLogCheckBox
            // 
            resources.ApplyResources(this.LMLogCheckBox, "LMLogCheckBox");
            this.LMLogCheckBox.Name = "LMLogCheckBox";
            this.LMLogCheckBox.UseVisualStyleBackColor = true;
            this.LMLogCheckBox.CheckedChanged += new System.EventHandler(this.LMLogCheckBox_CheckedChanged);
            // 
            // LblEnableVerboseLog
            // 
            resources.ApplyResources(this.LblEnableVerboseLog, "LblEnableVerboseLog");
            this.LblEnableVerboseLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblEnableVerboseLog.Name = "LblEnableVerboseLog";
            // 
            // nudWebServicesTimeOut
            // 
            resources.ApplyResources(this.nudWebServicesTimeOut, "nudWebServicesTimeOut");
            this.nudWebServicesTimeOut.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.nudWebServicesTimeOut.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.nudWebServicesTimeOut.Name = "nudWebServicesTimeOut";
            this.nudWebServicesTimeOut.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudWebServicesTimeOut.Validating += new System.ComponentModel.CancelEventHandler(this.NudWebServicesTimeOut_Validating);
            // 
            // nudWebServicesPort
            // 
            resources.ApplyResources(this.nudWebServicesPort, "nudWebServicesPort");
            this.nudWebServicesPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudWebServicesPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudWebServicesPort.Name = "nudWebServicesPort";
            this.nudWebServicesPort.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            this.nudWebServicesPort.Validating += new System.ComponentModel.CancelEventHandler(this.NudWebServicesPort_Validating);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // EALogCheckBox
            // 
            resources.ApplyResources(this.EALogCheckBox, "EALogCheckBox");
            this.EALogCheckBox.Name = "EALogCheckBox";
            this.EALogCheckBox.UseVisualStyleBackColor = true;
            this.EALogCheckBox.CheckedChanged += new System.EventHandler(this.EALogCheckBox_CheckedChanged);
            // 
            // SMTPGroupBox
            // 
            resources.ApplyResources(this.SMTPGroupBox, "SMTPGroupBox");
            this.SMTPGroupBox.Controls.Add(this.SMTPFrom);
            this.SMTPGroupBox.Controls.Add(this.label1);
            this.SMTPGroupBox.Controls.Add(this.SMTPPortNumericUpDown);
            this.SMTPGroupBox.Controls.Add(this.SMTPDomainsComboBox);
            this.SMTPGroupBox.Controls.Add(this.SMTPPasswordTextBox);
            this.SMTPGroupBox.Controls.Add(this.SMTPUserNameTextBox);
            this.SMTPGroupBox.Controls.Add(this.PortLabel);
            this.SMTPGroupBox.Controls.Add(this.PasswordLabel);
            this.SMTPGroupBox.Controls.Add(this.UserNameLabel);
            this.SMTPGroupBox.Controls.Add(this.DomainLabel);
            this.SMTPGroupBox.Controls.Add(this.SMTPUseSSLCheckBox);
            this.SMTPGroupBox.Controls.Add(this.SMTPUseSpecificCredentialsCheckBox);
            this.SMTPGroupBox.Controls.Add(this.SMTPRelayServerTextBox);
            this.SMTPGroupBox.Controls.Add(this.SMTPRelayServerLabel);
            this.SMTPGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SMTPGroupBox.Name = "SMTPGroupBox";
            this.SMTPGroupBox.TabStop = false;
            // 
            // SMTPFrom
            // 
            resources.ApplyResources(this.SMTPFrom, "SMTPFrom");
            this.SMTPFrom.Name = "SMTPFrom";
            this.SMTPFrom.Validating += new System.ComponentModel.CancelEventHandler(this.SMTPFrom_Validating);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // SMTPPortNumericUpDown
            // 
            resources.ApplyResources(this.SMTPPortNumericUpDown, "SMTPPortNumericUpDown");
            this.SMTPPortNumericUpDown.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.SMTPPortNumericUpDown.Name = "SMTPPortNumericUpDown";
            this.SMTPPortNumericUpDown.Validating += new System.ComponentModel.CancelEventHandler(this.SMTPPortNumericUpDown_Validating);
            // 
            // SMTPDomainsComboBox
            // 
            this.SMTPDomainsComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.SMTPDomainsComboBox, "SMTPDomainsComboBox");
            this.SMTPDomainsComboBox.Name = "SMTPDomainsComboBox";
            this.SMTPDomainsComboBox.SelectedIndexChanged += new System.EventHandler(this.SMTPDomainsComboBox_SelectedIndexChanged);
            // 
            // SMTPPasswordTextBox
            // 
            resources.ApplyResources(this.SMTPPasswordTextBox, "SMTPPasswordTextBox");
            this.SMTPPasswordTextBox.Name = "SMTPPasswordTextBox";
            this.SMTPPasswordTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.SMTPPasswordTextBox_Validating);
            // 
            // SMTPUserNameTextBox
            // 
            resources.ApplyResources(this.SMTPUserNameTextBox, "SMTPUserNameTextBox");
            this.SMTPUserNameTextBox.Name = "SMTPUserNameTextBox";
            this.SMTPUserNameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.SMTPUserNameTextBox_Validating);
            // 
            // PortLabel
            // 
            resources.ApplyResources(this.PortLabel, "PortLabel");
            this.PortLabel.Name = "PortLabel";
            // 
            // PasswordLabel
            // 
            resources.ApplyResources(this.PasswordLabel, "PasswordLabel");
            this.PasswordLabel.Name = "PasswordLabel";
            // 
            // UserNameLabel
            // 
            resources.ApplyResources(this.UserNameLabel, "UserNameLabel");
            this.UserNameLabel.Name = "UserNameLabel";
            // 
            // DomainLabel
            // 
            resources.ApplyResources(this.DomainLabel, "DomainLabel");
            this.DomainLabel.Name = "DomainLabel";
            // 
            // SMTPUseSSLCheckBox
            // 
            this.SMTPUseSSLCheckBox.AutoEllipsis = true;
            resources.ApplyResources(this.SMTPUseSSLCheckBox, "SMTPUseSSLCheckBox");
            this.SMTPUseSSLCheckBox.Name = "SMTPUseSSLCheckBox";
            this.SMTPUseSSLCheckBox.UseVisualStyleBackColor = true;
            this.SMTPUseSSLCheckBox.CheckedChanged += new System.EventHandler(this.SMTPUseSSLCheckBox_CheckedChanged);
            // 
            // SMTPUseSpecificCredentialsCheckBox
            // 
            this.SMTPUseSpecificCredentialsCheckBox.AutoEllipsis = true;
            resources.ApplyResources(this.SMTPUseSpecificCredentialsCheckBox, "SMTPUseSpecificCredentialsCheckBox");
            this.SMTPUseSpecificCredentialsCheckBox.Name = "SMTPUseSpecificCredentialsCheckBox";
            this.SMTPUseSpecificCredentialsCheckBox.UseVisualStyleBackColor = true;
            this.SMTPUseSpecificCredentialsCheckBox.CheckedChanged += new System.EventHandler(this.SMTPUseSpecificCredentialsCheckBox_CheckedChanged);
            // 
            // SMTPRelayServerTextBox
            // 
            resources.ApplyResources(this.SMTPRelayServerTextBox, "SMTPRelayServerTextBox");
            this.SMTPRelayServerTextBox.Name = "SMTPRelayServerTextBox";
            this.SMTPRelayServerTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.SMTPRelayServerTextBox_Validating);
            // 
            // SMTPRelayServerLabel
            // 
            resources.ApplyResources(this.SMTPRelayServerLabel, "SMTPRelayServerLabel");
            this.SMTPRelayServerLabel.Name = "SMTPRelayServerLabel";
            // 
            // GroupTBLoaderParameters
            // 
            resources.ApplyResources(this.GroupTBLoaderParameters, "GroupTBLoaderParameters");
            this.GroupTBLoaderParameters.Controls.Add(this.nudTBWCFDataTransferTimeout);
            this.GroupTBLoaderParameters.Controls.Add(this.nudTBWCFDefaultTimeout);
            this.GroupTBLoaderParameters.Controls.Add(this.nudMaxLoginPerTBLoader);
            this.GroupTBLoaderParameters.Controls.Add(this.LblDefaultDataTransferWcfCall);
            this.GroupTBLoaderParameters.Controls.Add(this.LblDefaultWcfCallTimeout);
            this.GroupTBLoaderParameters.Controls.Add(this.LblMaxLoginPerTBLoader);
            this.GroupTBLoaderParameters.Controls.Add(this.nudTBLoaderTimeout);
            this.GroupTBLoaderParameters.Controls.Add(this.LblTBLoaderTimeout);
            this.GroupTBLoaderParameters.Controls.Add(this.nudMaxTbLoader);
            this.GroupTBLoaderParameters.Controls.Add(this.LblMaxTbLoader);
            this.GroupTBLoaderParameters.Name = "GroupTBLoaderParameters";
            this.GroupTBLoaderParameters.TabStop = false;
            // 
            // nudTBWCFDataTransferTimeout
            // 
            resources.ApplyResources(this.nudTBWCFDataTransferTimeout, "nudTBWCFDataTransferTimeout");
            this.nudTBWCFDataTransferTimeout.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.nudTBWCFDataTransferTimeout.Name = "nudTBWCFDataTransferTimeout";
            this.nudTBWCFDataTransferTimeout.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudTBWCFDataTransferTimeout.Validating += new System.ComponentModel.CancelEventHandler(this.NudTBWCFDataTransferTimeout_Validating);
            // 
            // nudTBWCFDefaultTimeout
            // 
            resources.ApplyResources(this.nudTBWCFDefaultTimeout, "nudTBWCFDefaultTimeout");
            this.nudTBWCFDefaultTimeout.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.nudTBWCFDefaultTimeout.Name = "nudTBWCFDefaultTimeout";
            this.nudTBWCFDefaultTimeout.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudTBWCFDefaultTimeout.Validating += new System.ComponentModel.CancelEventHandler(this.NudTBWCFDefaultTimeout_Validating);
            // 
            // LblDBSizeMaxLevel
            // 
            resources.ApplyResources(this.LblDBSizeMaxLevel, "LblDBSizeMaxLevel");
            this.LblDBSizeMaxLevel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblDBSizeMaxLevel.Name = "LblDBSizeMaxLevel";
            // 
            // GBoxOtherSettings
            // 
            resources.ApplyResources(this.GBoxOtherSettings, "GBoxOtherSettings");
            this.GBoxOtherSettings.Controls.Add(this.NudDBSizeLevel);
            this.GBoxOtherSettings.Controls.Add(this.LblDBSizeMaxLevel);
            this.GBoxOtherSettings.Controls.Add(this.CbAutologin);
            this.GBoxOtherSettings.Controls.Add(this.PbAutologin);
            this.GBoxOtherSettings.Controls.Add(this.TxtPingMailRecipient);
            this.GBoxOtherSettings.Controls.Add(this.CkbSendPingMail);
            this.GBoxOtherSettings.Name = "GBoxOtherSettings";
            this.GBoxOtherSettings.TabStop = false;
            // 
            // NudDBSizeLevel
            // 
            this.NudDBSizeLevel.DecimalPlaces = 2;
            resources.ApplyResources(this.NudDBSizeLevel, "NudDBSizeLevel");
            this.NudDBSizeLevel.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.NudDBSizeLevel.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            131072});
            this.NudDBSizeLevel.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            131072});
            this.NudDBSizeLevel.Name = "NudDBSizeLevel";
            this.NudDBSizeLevel.Value = new decimal(new int[] {
            180,
            0,
            0,
            131072});
            this.NudDBSizeLevel.Validating += new System.ComponentModel.CancelEventHandler(this.NudDBSizeLevel_Validating);
            // 
            // TxtPingMailRecipient
            // 
            resources.ApplyResources(this.TxtPingMailRecipient, "TxtPingMailRecipient");
            this.TxtPingMailRecipient.Name = "TxtPingMailRecipient";
            // 
            // CkbSendPingMail
            // 
            this.CkbSendPingMail.AutoEllipsis = true;
            resources.ApplyResources(this.CkbSendPingMail, "CkbSendPingMail");
            this.CkbSendPingMail.Name = "CkbSendPingMail";
            this.CkbSendPingMail.UseVisualStyleBackColor = true;
            this.CkbSendPingMail.CheckedChanged += new System.EventHandler(this.CkbSendPingMail_CheckedChanged);
            // 
            // EditConfigFile
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.GBoxOtherSettings);
            this.Controls.Add(this.GroupTBLoaderParameters);
            this.Controls.Add(this.SMTPGroupBox);
            this.Controls.Add(this.GroupPolicyPwdUser);
            this.Controls.Add(this.GroupBoxLanguage);
            this.Controls.Add(this.LabelTitle);
            this.Controls.Add(this.groupBox1);
            this.Name = "EditConfigFile";
            this.ShowInTaskbar = false;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.EditConfigFile_Closing);
            this.Deactivate += new System.EventHandler(this.EditConfigFile_Deactivate);
            this.VisibleChanged += new System.EventHandler(this.EditConfigFile_VisibleChanged);
            this.GroupBoxLanguage.ResumeLayout(false);
            this.GroupBoxLanguage.PerformLayout();
            this.GroupPolicyPwdUser.ResumeLayout(false);
            this.GroupPolicyPwdUser.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxLoginFailed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPwdTimeLenght)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinPwdLenght)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbAutologin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTBLoaderTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxLoginPerTBLoader)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxTbLoader)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudWebServicesTimeOut)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWebServicesPort)).EndInit();
            this.SMTPGroupBox.ResumeLayout(false);
            this.SMTPGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SMTPPortNumericUpDown)).EndInit();
            this.GroupTBLoaderParameters.ResumeLayout(false);
            this.GroupTBLoaderParameters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTBWCFDataTransferTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTBWCFDefaultTimeout)).EndInit();
            this.GBoxOtherSettings.ResumeLayout(false);
            this.GBoxOtherSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NudDBSizeLevel)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private TextBox SMTPFrom;
        private Label label1;
        private CheckBox SMTPUseSSLCheckBox;
        private NumericUpDown nudMaxLoginPerTBLoader;
        private Label LblMaxLoginPerTBLoader;
        private NumericUpDown nudTBLoaderTimeout;
        private Label LblTBLoaderTimeout;
        private GroupBox GroupTBLoaderParameters;
        private NumericUpDown nudTBWCFDataTransferTimeout;
        private NumericUpDown nudTBWCFDefaultTimeout;
        private Label LblDefaultDataTransferWcfCall;
        private Label LblDefaultWcfCallTimeout;
        private CheckBox CbAutologin;
        private PictureBox PbAutologin;
        private Label LblDBSizeMaxLevel;
        private GroupBox GBoxOtherSettings;
        private NumericUpDown NudDBSizeLevel;
        private CheckBox EALogCheckBox;
        private CheckBox LMLogCheckBox;
        private Label LblEnableVerboseLog;
        private CheckBox CkbSendPingMail;
        private TextBox TxtPingMailRecipient;
        private NumericUpDown numericUpDown1;
        private Label label5;

    }
}
