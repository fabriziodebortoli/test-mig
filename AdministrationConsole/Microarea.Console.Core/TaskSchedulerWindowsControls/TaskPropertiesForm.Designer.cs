
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class TaskPropertiesForm
    {
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.TabControl TaskTabControl;
        private System.Windows.Forms.TabPage GeneralPropertiesPage;
        private GeneralTaskPropertiesPageContent GeneralPropertiesPageContent;
        private System.Windows.Forms.TabPage CommandPropertiesPage;
        private System.Windows.Forms.Label CommandTypeLabel;
        private System.Windows.Forms.ComboBox CommandTypeComboBox;
        private System.Windows.Forms.Button SelectCommandButton;
        private System.Windows.Forms.Button SelectMenuCommandBtn;
        private System.Windows.Forms.Label CommandLabel;
        private System.Windows.Forms.RichTextBox CommandTextBox;
        private System.Windows.Forms.Button CommandParametersButton;
        private System.Windows.Forms.Label MessageTextLabel;
        private System.Windows.Forms.TextBox MessageTextBox;
        private System.Windows.Forms.CheckBox RunIconizedCheckBox;
        private System.Windows.Forms.CheckBox PrintReportCheckBox;
        private System.Windows.Forms.CheckBox CloseOnEndCheckBox;
        private System.Windows.Forms.CheckBox WaitForProcessHasExitedCheckBox;
        private System.Windows.Forms.CheckBox ValidateDataCheckBox;
        private System.Windows.Forms.CheckBox SetApplicationDateCheckBox;
        private System.Windows.Forms.CheckBox CompressAttachmentsCheckBox;
        private System.Windows.Forms.RadioButton UseDefaultBrowserRadioButton;
        private System.Windows.Forms.RadioButton UseInternetExplorerRadioButton;
        private System.Windows.Forms.CheckBox CreateNewBrowserInstanceCheckBox;
        private System.Windows.Forms.TabPage SchedulingPropertiesPage;
        private SchedulingPropertiesPageContent SchedulingPageContent;
        private System.Windows.Forms.TabPage NotificationsPage;
        private MailNotificationsPageContent TaskMailNotificationsPageContent;
        private System.Windows.Forms.Label ReportMailRecipientLabel;
        private MailAddressSelectionControl ReportMailRecipientControl;
        private MailAddressSelectionControl MailTaskRecipientsControl;
        private System.Windows.Forms.RadioButton SendMailWithReportAsAttachmentRadioButton;
        private System.Windows.Forms.GroupBox ReportOptionsGroupBox;
        private System.Windows.Forms.RadioButton SaveReportAsFileRadioButton;
        private System.Windows.Forms.TextBox ReportFileNameTextBox;
        private System.Windows.Forms.Button BrowseReportFileNameButton;
        private System.Windows.Forms.Label ReportFileNameLabel;
        private System.Windows.Forms.GroupBox ReportFormatGroupBox;
		private System.Windows.Forms.RadioButton ExcelFormatCheckBox;
		private System.Windows.Forms.RadioButton RDEFormatCheckBox;
		private System.Windows.Forms.RadioButton PDFFormatCheckBox;
        private System.Windows.Forms.CheckBox ConcatPDFFilesCheckBox;
        private System.Windows.Forms.CheckBox OverwriteCompanyDBBackupCheckBox;
        private System.Windows.Forms.CheckBox VerifyCompanyDBBackupCheckBox;
        private System.Windows.Forms.RadioButton NormalReportExecutionRadioButton;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //--------------------------------------------------------------------------------------------------------------------------------
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskPropertiesForm));
			this.TaskTabControl = new System.Windows.Forms.TabControl();
			this.GeneralPropertiesPage = new System.Windows.Forms.TabPage();
			this.GeneralPropertiesPageContent = new Microarea.Console.Core.TaskSchedulerWindowsControls.GeneralTaskPropertiesPageContent();
			this.CommandPropertiesPage = new System.Windows.Forms.TabPage();
			this.RunIconizedCheckBox = new System.Windows.Forms.CheckBox();
			this.PrintReportCheckBox = new System.Windows.Forms.CheckBox();
			this.ReportOptionsGroupBox = new System.Windows.Forms.GroupBox();
			this.NormalReportExecutionRadioButton = new System.Windows.Forms.RadioButton();
			this.ConcatPDFFilesCheckBox = new System.Windows.Forms.CheckBox();
			this.ReportFormatGroupBox = new System.Windows.Forms.GroupBox();
			this.ExcelFormatCheckBox = new System.Windows.Forms.RadioButton();
			this.RDEFormatCheckBox = new System.Windows.Forms.RadioButton();
			this.PDFFormatCheckBox = new System.Windows.Forms.RadioButton();
			this.BrowseReportFileNameButton = new System.Windows.Forms.Button();
			this.ReportFileNameTextBox = new System.Windows.Forms.TextBox();
			this.ReportFileNameLabel = new System.Windows.Forms.Label();
			this.SaveReportAsFileRadioButton = new System.Windows.Forms.RadioButton();
			this.ReportMailRecipientLabel = new System.Windows.Forms.Label();
			this.SendMailWithReportAsAttachmentRadioButton = new System.Windows.Forms.RadioButton();
			this.ReportMailRecipientControl = new Microarea.Console.Core.TaskSchedulerWindowsControls.MailAddressSelectionControl();
			this.CompressAttachmentsCheckBox = new System.Windows.Forms.CheckBox();
			this.MessageTextBox = new System.Windows.Forms.TextBox();
			this.CreateNewBrowserInstanceCheckBox = new System.Windows.Forms.CheckBox();
			this.WaitForProcessHasExitedCheckBox = new System.Windows.Forms.CheckBox();
			this.CommandParametersButton = new System.Windows.Forms.Button();
			this.CommandTypeComboBox = new System.Windows.Forms.ComboBox();
			this.CommandTypeLabel = new System.Windows.Forms.Label();
			this.SelectMenuCommandBtn = new System.Windows.Forms.Button();
			this.SelectCommandButton = new System.Windows.Forms.Button();
			this.CommandTextBox = new System.Windows.Forms.RichTextBox();
			this.CommandLabel = new System.Windows.Forms.Label();
			this.CloseOnEndCheckBox = new System.Windows.Forms.CheckBox();
			this.MailTaskRecipientsControl = new Microarea.Console.Core.TaskSchedulerWindowsControls.MailAddressSelectionControl();
			this.VerifyCompanyDBBackupCheckBox = new System.Windows.Forms.CheckBox();
			this.OverwriteCompanyDBBackupCheckBox = new System.Windows.Forms.CheckBox();
			this.ValidateDataCheckBox = new System.Windows.Forms.CheckBox();
			this.UseDefaultBrowserRadioButton = new System.Windows.Forms.RadioButton();
			this.SetApplicationDateCheckBox = new System.Windows.Forms.CheckBox();
			this.UseInternetExplorerRadioButton = new System.Windows.Forms.RadioButton();
			this.MessageTextLabel = new System.Windows.Forms.Label();
			this.SchedulingPropertiesPage = new System.Windows.Forms.TabPage();
			this.SchedulingPageContent = new Microarea.Console.Core.TaskSchedulerWindowsControls.SchedulingPropertiesPageContent();
			this.NotificationsPage = new System.Windows.Forms.TabPage();
			this.TaskMailNotificationsPageContent = new Microarea.Console.Core.TaskSchedulerWindowsControls.MailNotificationsPageContent();
			this.OkBtn = new System.Windows.Forms.Button();
			this.CancelBtn = new System.Windows.Forms.Button();
			this.taskStatusStrip = new System.Windows.Forms.StatusStrip();
			this.taskToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.TaskTabControl.SuspendLayout();
			this.GeneralPropertiesPage.SuspendLayout();
			this.CommandPropertiesPage.SuspendLayout();
			this.ReportOptionsGroupBox.SuspendLayout();
			this.ReportFormatGroupBox.SuspendLayout();
			this.SchedulingPropertiesPage.SuspendLayout();
			this.NotificationsPage.SuspendLayout();
			this.taskStatusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// TaskTabControl
			// 
			resources.ApplyResources(this.TaskTabControl, "TaskTabControl");
			this.TaskTabControl.Controls.Add(this.GeneralPropertiesPage);
			this.TaskTabControl.Controls.Add(this.CommandPropertiesPage);
			this.TaskTabControl.Controls.Add(this.SchedulingPropertiesPage);
			this.TaskTabControl.Controls.Add(this.NotificationsPage);
			this.TaskTabControl.Name = "TaskTabControl";
			this.TaskTabControl.SelectedIndex = 0;
			this.TaskTabControl.SelectedIndexChanged += new System.EventHandler(this.TaskTabControl_SelectedIndexChanged);
			// 
			// GeneralPropertiesPage
			// 
			this.GeneralPropertiesPage.Controls.Add(this.GeneralPropertiesPageContent);
			resources.ApplyResources(this.GeneralPropertiesPage, "GeneralPropertiesPage");
			this.GeneralPropertiesPage.Name = "GeneralPropertiesPage";
			// 
			// GeneralPropertiesPageContent
			// 
			resources.ApplyResources(this.GeneralPropertiesPageContent, "GeneralPropertiesPageContent");
			this.GeneralPropertiesPageContent.CodeText = "";
			this.GeneralPropertiesPageContent.CurrentConnectionString = "";
			this.GeneralPropertiesPageContent.Name = "GeneralPropertiesPageContent";
			this.GeneralPropertiesPageContent.Task = null;
			this.GeneralPropertiesPageContent.OnValidatedCode += new Microarea.Console.Core.TaskSchedulerWindowsControls.TaskCodeValidationEventHandler(this.GeneralPropertiesPageContent_OnValidatedCode);
			// 
			// CommandPropertiesPage
			// 
			this.CommandPropertiesPage.Controls.Add(this.RunIconizedCheckBox);
			this.CommandPropertiesPage.Controls.Add(this.PrintReportCheckBox);
			this.CommandPropertiesPage.Controls.Add(this.ReportOptionsGroupBox);
			this.CommandPropertiesPage.Controls.Add(this.MessageTextBox);
			this.CommandPropertiesPage.Controls.Add(this.CreateNewBrowserInstanceCheckBox);
			this.CommandPropertiesPage.Controls.Add(this.WaitForProcessHasExitedCheckBox);
			this.CommandPropertiesPage.Controls.Add(this.CommandParametersButton);
			this.CommandPropertiesPage.Controls.Add(this.CommandTypeComboBox);
			this.CommandPropertiesPage.Controls.Add(this.CommandTypeLabel);
			this.CommandPropertiesPage.Controls.Add(this.SelectMenuCommandBtn);
			this.CommandPropertiesPage.Controls.Add(this.SelectCommandButton);
			this.CommandPropertiesPage.Controls.Add(this.CommandTextBox);
			this.CommandPropertiesPage.Controls.Add(this.CommandLabel);
			this.CommandPropertiesPage.Controls.Add(this.CloseOnEndCheckBox);
			this.CommandPropertiesPage.Controls.Add(this.MailTaskRecipientsControl);
			this.CommandPropertiesPage.Controls.Add(this.VerifyCompanyDBBackupCheckBox);
			this.CommandPropertiesPage.Controls.Add(this.OverwriteCompanyDBBackupCheckBox);
			this.CommandPropertiesPage.Controls.Add(this.ValidateDataCheckBox);
			this.CommandPropertiesPage.Controls.Add(this.UseDefaultBrowserRadioButton);
			this.CommandPropertiesPage.Controls.Add(this.SetApplicationDateCheckBox);
			this.CommandPropertiesPage.Controls.Add(this.UseInternetExplorerRadioButton);
			this.CommandPropertiesPage.Controls.Add(this.MessageTextLabel);
			resources.ApplyResources(this.CommandPropertiesPage, "CommandPropertiesPage");
			this.CommandPropertiesPage.Name = "CommandPropertiesPage";
			// 
			// RunIconizedCheckBox
			// 
			resources.ApplyResources(this.RunIconizedCheckBox, "RunIconizedCheckBox");
			this.RunIconizedCheckBox.Name = "RunIconizedCheckBox";
			// 
			// PrintReportCheckBox
			// 
			resources.ApplyResources(this.PrintReportCheckBox, "PrintReportCheckBox");
			this.PrintReportCheckBox.Name = "PrintReportCheckBox";
			// 
			// ReportOptionsGroupBox
			// 
			this.ReportOptionsGroupBox.Controls.Add(this.NormalReportExecutionRadioButton);
			this.ReportOptionsGroupBox.Controls.Add(this.ConcatPDFFilesCheckBox);
			this.ReportOptionsGroupBox.Controls.Add(this.ReportFormatGroupBox);
			this.ReportOptionsGroupBox.Controls.Add(this.BrowseReportFileNameButton);
			this.ReportOptionsGroupBox.Controls.Add(this.ReportFileNameTextBox);
			this.ReportOptionsGroupBox.Controls.Add(this.ReportFileNameLabel);
			this.ReportOptionsGroupBox.Controls.Add(this.SaveReportAsFileRadioButton);
			this.ReportOptionsGroupBox.Controls.Add(this.ReportMailRecipientLabel);
			this.ReportOptionsGroupBox.Controls.Add(this.SendMailWithReportAsAttachmentRadioButton);
			this.ReportOptionsGroupBox.Controls.Add(this.ReportMailRecipientControl);
			this.ReportOptionsGroupBox.Controls.Add(this.CompressAttachmentsCheckBox);
			this.ReportOptionsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.ReportOptionsGroupBox, "ReportOptionsGroupBox");
			this.ReportOptionsGroupBox.Name = "ReportOptionsGroupBox";
			this.ReportOptionsGroupBox.TabStop = false;
			// 
			// NormalReportExecutionRadioButton
			// 
			resources.ApplyResources(this.NormalReportExecutionRadioButton, "NormalReportExecutionRadioButton");
			this.NormalReportExecutionRadioButton.Name = "NormalReportExecutionRadioButton";
			// 
			// ConcatPDFFilesCheckBox
			// 
			resources.ApplyResources(this.ConcatPDFFilesCheckBox, "ConcatPDFFilesCheckBox");
			this.ConcatPDFFilesCheckBox.Name = "ConcatPDFFilesCheckBox";
			// 
			// ReportFormatGroupBox
			// 
			this.ReportFormatGroupBox.Controls.Add(this.ExcelFormatCheckBox);
			this.ReportFormatGroupBox.Controls.Add(this.RDEFormatCheckBox);
			this.ReportFormatGroupBox.Controls.Add(this.PDFFormatCheckBox);
			resources.ApplyResources(this.ReportFormatGroupBox, "ReportFormatGroupBox");
			this.ReportFormatGroupBox.Name = "ReportFormatGroupBox";
			this.ReportFormatGroupBox.TabStop = false;
			// 
			// ExcelFormatCheckBox
			// 
			resources.ApplyResources(this.ExcelFormatCheckBox, "ExcelFormatCheckBox");
			this.ExcelFormatCheckBox.Name = "ExcelFormatCheckBox";
			// 
			// RDEFormatCheckBox
			// 
			resources.ApplyResources(this.RDEFormatCheckBox, "RDEFormatCheckBox");
			this.RDEFormatCheckBox.Name = "RDEFormatCheckBox";
			// 
			// PDFFormatCheckBox
			// 
			resources.ApplyResources(this.PDFFormatCheckBox, "PDFFormatCheckBox");
			this.PDFFormatCheckBox.Name = "PDFFormatCheckBox";
			this.PDFFormatCheckBox.CheckedChanged += new System.EventHandler(this.PDFFormatCheckBox_CheckedChanged);
			// 
			// BrowseReportFileNameButton
			// 
			resources.ApplyResources(this.BrowseReportFileNameButton, "BrowseReportFileNameButton");
			this.BrowseReportFileNameButton.Name = "BrowseReportFileNameButton";
			this.BrowseReportFileNameButton.Click += new System.EventHandler(this.BrowseReportFileNameButton_Click);
			// 
			// ReportFileNameTextBox
			// 
			resources.ApplyResources(this.ReportFileNameTextBox, "ReportFileNameTextBox");
			this.ReportFileNameTextBox.Name = "ReportFileNameTextBox";
			// 
			// ReportFileNameLabel
			// 
			resources.ApplyResources(this.ReportFileNameLabel, "ReportFileNameLabel");
			this.ReportFileNameLabel.Name = "ReportFileNameLabel";
			// 
			// SaveReportAsFileRadioButton
			// 
			resources.ApplyResources(this.SaveReportAsFileRadioButton, "SaveReportAsFileRadioButton");
			this.SaveReportAsFileRadioButton.Name = "SaveReportAsFileRadioButton";
			this.SaveReportAsFileRadioButton.CheckedChanged += new System.EventHandler(this.SaveReportAsFileRadioButton_CheckedChanged);
			// 
			// ReportMailRecipientLabel
			// 
			resources.ApplyResources(this.ReportMailRecipientLabel, "ReportMailRecipientLabel");
			this.ReportMailRecipientLabel.Name = "ReportMailRecipientLabel";
			// 
			// SendMailWithReportAsAttachmentRadioButton
			// 
			resources.ApplyResources(this.SendMailWithReportAsAttachmentRadioButton, "SendMailWithReportAsAttachmentRadioButton");
			this.SendMailWithReportAsAttachmentRadioButton.Name = "SendMailWithReportAsAttachmentRadioButton";
			this.SendMailWithReportAsAttachmentRadioButton.CheckedChanged += new System.EventHandler(this.SendMailWithReportAsAttachmentRadioButton_CheckedChanged);
			// 
			// ReportMailRecipientControl
			// 
			resources.ApplyResources(this.ReportMailRecipientControl, "ReportMailRecipientControl");
			this.ReportMailRecipientControl.Name = "ReportMailRecipientControl";
			// 
			// CompressAttachmentsCheckBox
			// 
			resources.ApplyResources(this.CompressAttachmentsCheckBox, "CompressAttachmentsCheckBox");
			this.CompressAttachmentsCheckBox.Name = "CompressAttachmentsCheckBox";
			// 
			// MessageTextBox
			// 
			this.MessageTextBox.AcceptsReturn = true;
			resources.ApplyResources(this.MessageTextBox, "MessageTextBox");
			this.MessageTextBox.Name = "MessageTextBox";
			// 
			// CreateNewBrowserInstanceCheckBox
			// 
			resources.ApplyResources(this.CreateNewBrowserInstanceCheckBox, "CreateNewBrowserInstanceCheckBox");
			this.CreateNewBrowserInstanceCheckBox.Name = "CreateNewBrowserInstanceCheckBox";
			// 
			// WaitForProcessHasExitedCheckBox
			// 
			resources.ApplyResources(this.WaitForProcessHasExitedCheckBox, "WaitForProcessHasExitedCheckBox");
			this.WaitForProcessHasExitedCheckBox.Name = "WaitForProcessHasExitedCheckBox";
			// 
			// CommandParametersButton
			// 
			resources.ApplyResources(this.CommandParametersButton, "CommandParametersButton");
			this.CommandParametersButton.Name = "CommandParametersButton";
			this.CommandParametersButton.Click += new System.EventHandler(this.CommandParametersButton_Click);
			// 
			// CommandTypeComboBox
			// 
			this.CommandTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.CommandTypeComboBox, "CommandTypeComboBox");
			this.CommandTypeComboBox.Items.AddRange(new object[] {
            resources.GetString("CommandTypeComboBox.Items"),
            resources.GetString("CommandTypeComboBox.Items1"),
            resources.GetString("CommandTypeComboBox.Items2"),
            resources.GetString("CommandTypeComboBox.Items3"),
            resources.GetString("CommandTypeComboBox.Items4"),
            resources.GetString("CommandTypeComboBox.Items5"),
            resources.GetString("CommandTypeComboBox.Items6"),
            resources.GetString("CommandTypeComboBox.Items7"),
            resources.GetString("CommandTypeComboBox.Items8")});
			this.CommandTypeComboBox.Name = "CommandTypeComboBox";
			this.CommandTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.CommandTypeComboBox_SelectedIndexChanged);
			// 
			// CommandTypeLabel
			// 
			resources.ApplyResources(this.CommandTypeLabel, "CommandTypeLabel");
			this.CommandTypeLabel.Name = "CommandTypeLabel";
			// 
			// SelectMenuCommandBtn
			// 
			resources.ApplyResources(this.SelectMenuCommandBtn, "SelectMenuCommandBtn");
			this.SelectMenuCommandBtn.Name = "SelectMenuCommandBtn";
			this.SelectMenuCommandBtn.Click += new System.EventHandler(this.SelectMenuCommandBtn_Click);
			// 
			// SelectCommandButton
			// 
			resources.ApplyResources(this.SelectCommandButton, "SelectCommandButton");
			this.SelectCommandButton.Name = "SelectCommandButton";
			this.SelectCommandButton.Click += new System.EventHandler(this.SelectCommandButton_Click);
			// 
			// CommandTextBox
			// 
			resources.ApplyResources(this.CommandTextBox, "CommandTextBox");
			this.CommandTextBox.Name = "CommandTextBox";
			this.CommandTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.CommandTextBox_Validating);
			// 
			// CommandLabel
			// 
			resources.ApplyResources(this.CommandLabel, "CommandLabel");
			this.CommandLabel.Name = "CommandLabel";
			// 
			// CloseOnEndCheckBox
			// 
			resources.ApplyResources(this.CloseOnEndCheckBox, "CloseOnEndCheckBox");
			this.CloseOnEndCheckBox.Name = "CloseOnEndCheckBox";
			// 
			// MailTaskRecipientsControl
			// 
			resources.ApplyResources(this.MailTaskRecipientsControl, "MailTaskRecipientsControl");
			this.MailTaskRecipientsControl.Name = "MailTaskRecipientsControl";
			// 
			// VerifyCompanyDBBackupCheckBox
			// 
			resources.ApplyResources(this.VerifyCompanyDBBackupCheckBox, "VerifyCompanyDBBackupCheckBox");
			this.VerifyCompanyDBBackupCheckBox.Name = "VerifyCompanyDBBackupCheckBox";
			// 
			// OverwriteCompanyDBBackupCheckBox
			// 
			resources.ApplyResources(this.OverwriteCompanyDBBackupCheckBox, "OverwriteCompanyDBBackupCheckBox");
			this.OverwriteCompanyDBBackupCheckBox.Name = "OverwriteCompanyDBBackupCheckBox";
			// 
			// ValidateDataCheckBox
			// 
			resources.ApplyResources(this.ValidateDataCheckBox, "ValidateDataCheckBox");
			this.ValidateDataCheckBox.Name = "ValidateDataCheckBox";
			// 
			// UseDefaultBrowserRadioButton
			// 
			resources.ApplyResources(this.UseDefaultBrowserRadioButton, "UseDefaultBrowserRadioButton");
			this.UseDefaultBrowserRadioButton.Name = "UseDefaultBrowserRadioButton";
			this.UseDefaultBrowserRadioButton.CheckedChanged += new System.EventHandler(this.BrowserChoiceRadioButton_CheckedChanged);
			// 
			// SetApplicationDateCheckBox
			// 
			resources.ApplyResources(this.SetApplicationDateCheckBox, "SetApplicationDateCheckBox");
			this.SetApplicationDateCheckBox.Name = "SetApplicationDateCheckBox";
			// 
			// UseInternetExplorerRadioButton
			// 
			resources.ApplyResources(this.UseInternetExplorerRadioButton, "UseInternetExplorerRadioButton");
			this.UseInternetExplorerRadioButton.Name = "UseInternetExplorerRadioButton";
			this.UseInternetExplorerRadioButton.CheckedChanged += new System.EventHandler(this.BrowserChoiceRadioButton_CheckedChanged);
			// 
			// MessageTextLabel
			// 
			resources.ApplyResources(this.MessageTextLabel, "MessageTextLabel");
			this.MessageTextLabel.Name = "MessageTextLabel";
			// 
			// SchedulingPropertiesPage
			// 
			this.SchedulingPropertiesPage.Controls.Add(this.SchedulingPageContent);
			resources.ApplyResources(this.SchedulingPropertiesPage, "SchedulingPropertiesPage");
			this.SchedulingPropertiesPage.Name = "SchedulingPropertiesPage";
			// 
			// SchedulingPageContent
			// 
			resources.ApplyResources(this.SchedulingPageContent, "SchedulingPageContent");
			this.SchedulingPageContent.Name = "SchedulingPageContent";
			this.SchedulingPageContent.Task = null;
			this.SchedulingPageContent.SchedulingModeChanged += new System.EventHandler(this.SchedulingPageContent_SchedulingModeChanged);
			// 
			// NotificationsPage
			// 
			this.NotificationsPage.Controls.Add(this.TaskMailNotificationsPageContent);
			resources.ApplyResources(this.NotificationsPage, "NotificationsPage");
			this.NotificationsPage.Name = "NotificationsPage";
			// 
			// TaskMailNotificationsPageContent
			// 
			resources.ApplyResources(this.TaskMailNotificationsPageContent, "TaskMailNotificationsPageContent");
			this.TaskMailNotificationsPageContent.Name = "TaskMailNotificationsPageContent";
			this.TaskMailNotificationsPageContent.Task = null;
			// 
			// OkBtn
			// 
			resources.ApplyResources(this.OkBtn, "OkBtn");
			this.OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkBtn.Name = "OkBtn";
			// 
			// CancelBtn
			// 
			resources.ApplyResources(this.CancelBtn, "CancelBtn");
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Name = "CancelBtn";
			// 
			// taskStatusStrip
			// 
			this.taskStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.taskToolStripStatusLabel});
			resources.ApplyResources(this.taskStatusStrip, "taskStatusStrip");
			this.taskStatusStrip.Name = "taskStatusStrip";
			// 
			// taskToolStripStatusLabel
			// 
			this.taskToolStripStatusLabel.Name = "taskToolStripStatusLabel";
			resources.ApplyResources(this.taskToolStripStatusLabel, "taskToolStripStatusLabel");
			// 
			// TaskPropertiesForm
			// 
			this.AcceptButton = this.CancelBtn;
			resources.ApplyResources(this, "$this");
			this.CancelButton = this.CancelBtn;
			this.ControlBox = false;
			this.Controls.Add(this.taskStatusStrip);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.OkBtn);
			this.Controls.Add(this.TaskTabControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TaskPropertiesForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.TaskTabControl.ResumeLayout(false);
			this.GeneralPropertiesPage.ResumeLayout(false);
			this.CommandPropertiesPage.ResumeLayout(false);
			this.CommandPropertiesPage.PerformLayout();
			this.ReportOptionsGroupBox.ResumeLayout(false);
			this.ReportOptionsGroupBox.PerformLayout();
			this.ReportFormatGroupBox.ResumeLayout(false);
			this.SchedulingPropertiesPage.ResumeLayout(false);
			this.NotificationsPage.ResumeLayout(false);
			this.taskStatusStrip.ResumeLayout(false);
			this.taskStatusStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

		private System.Windows.Forms.StatusStrip taskStatusStrip;
		private System.Windows.Forms.ToolStripStatusLabel taskToolStripStatusLabel;

    }
}
