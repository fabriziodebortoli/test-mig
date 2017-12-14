
namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	partial class SaveAsMenuMngDialog
    {
        private System.Windows.Forms.MenuStrip MenuMngDlgMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem OptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SearchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RefreshMenuLoaderToolStripMenuItem;
        protected System.Windows.Forms.TextBox FileNameTextBox;
        protected System.Windows.Forms.Label lblFileName;
        protected System.Windows.Forms.Label lblFileType;
        protected System.Windows.Forms.RadioButton UserRadioButton;
        protected System.Windows.Forms.RadioButton AllUsersRadioButton;
        protected System.Windows.Forms.RadioButton StandardRadioButton;
        protected System.Windows.Forms.ComboBox UsersComboBox;
        private MenuManagerStatusStrip MenuFormStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel CompanyStatusStripLabel;
        private System.Windows.Forms.ToolStripStatusLabel UserStatusStripLabel;
        private System.Windows.Forms.Button CancelSaveButton;
        private System.Windows.Forms.Button OKSaveButton;

        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveAsMenuMngDialog));
			this.MenuMngDlgToolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.cbxFileType = new System.Windows.Forms.ComboBox();
			this.lblFileType = new System.Windows.Forms.Label();
			this.FileNameTextBox = new System.Windows.Forms.TextBox();
			this.lblFileName = new System.Windows.Forms.Label();
			this.UserRadioButton = new System.Windows.Forms.RadioButton();
			this.UsersComboBox = new System.Windows.Forms.ComboBox();
			this.AllUsersRadioButton = new System.Windows.Forms.RadioButton();
			this.StandardRadioButton = new System.Windows.Forms.RadioButton();
			this.MenuMngDlgToolStrip = new System.Windows.Forms.ToolStrip();
			this.SearchToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.RefreshMenuLoaderToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.MenuMngDlgMenuStrip = new System.Windows.Forms.MenuStrip();
			this.OptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RefreshMenuLoaderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.OKSaveButton = new System.Windows.Forms.Button();
			this.CancelSaveButton = new System.Windows.Forms.Button();
			this.MenuManagerWinCtrl = new MenuMngWinCtrl();
			this.MenuFormStatusStrip = new MenuManagerStatusStrip();
            this.CompanyStatusStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.UserStatusStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.MenuMngDlgToolStripContainer.ContentPanel.SuspendLayout();
			this.MenuMngDlgToolStripContainer.TopToolStripPanel.SuspendLayout();
			this.MenuMngDlgToolStripContainer.SuspendLayout();
			this.MenuMngDlgToolStrip.SuspendLayout();
			this.MenuMngDlgMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// MenuMngDlgToolStripContainer
			// 
			// 
			// MenuMngDlgToolStripContainer.ContentPanel
			// 
			this.MenuMngDlgToolStripContainer.ContentPanel.Controls.Add(this.cbxFileType);
			this.MenuMngDlgToolStripContainer.ContentPanel.Controls.Add(this.MenuManagerWinCtrl);
			this.MenuMngDlgToolStripContainer.ContentPanel.Controls.Add(this.lblFileType);
			this.MenuMngDlgToolStripContainer.ContentPanel.Controls.Add(this.FileNameTextBox);
			this.MenuMngDlgToolStripContainer.ContentPanel.Controls.Add(this.lblFileName);
			this.MenuMngDlgToolStripContainer.ContentPanel.Controls.Add(this.UserRadioButton);
			this.MenuMngDlgToolStripContainer.ContentPanel.Controls.Add(this.UsersComboBox);
			this.MenuMngDlgToolStripContainer.ContentPanel.Controls.Add(this.AllUsersRadioButton);
			this.MenuMngDlgToolStripContainer.ContentPanel.Controls.Add(this.StandardRadioButton);
			resources.ApplyResources(this.MenuMngDlgToolStripContainer.ContentPanel, "MenuMngDlgToolStripContainer.ContentPanel");
			resources.ApplyResources(this.MenuMngDlgToolStripContainer, "MenuMngDlgToolStripContainer");
			this.MenuMngDlgToolStripContainer.MinimumSize = new System.Drawing.Size(892, 450);
			this.MenuMngDlgToolStripContainer.Name = "MenuMngDlgToolStripContainer";
			// 
			// MenuMngDlgToolStripContainer.TopToolStripPanel
			// 
			this.MenuMngDlgToolStripContainer.TopToolStripPanel.Controls.Add(this.MenuMngDlgToolStrip);
			// 
			// cbxFileType
			// 
			resources.ApplyResources(this.cbxFileType, "cbxFileType");
			this.cbxFileType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxFileType.Name = "cbxFileType";
			// 
			// lblFileType
			// 
			resources.ApplyResources(this.lblFileType, "lblFileType");
			this.lblFileType.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFileType.Name = "lblFileType";
			// 
			// FileNameTextBox
			// 
			resources.ApplyResources(this.FileNameTextBox, "FileNameTextBox");
			this.FileNameTextBox.Name = "FileNameTextBox";
			// 
			// lblFileName
			// 
			resources.ApplyResources(this.lblFileName, "lblFileName");
			this.lblFileName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFileName.Name = "lblFileName";
			// 
			// UserRadioButton
			// 
			resources.ApplyResources(this.UserRadioButton, "UserRadioButton");
			this.UserRadioButton.Checked = true;
			this.UserRadioButton.Name = "UserRadioButton";
			this.UserRadioButton.TabStop = true;
			this.UserRadioButton.CheckedChanged += new System.EventHandler(this.UserRadioButton_CheckedChanged);
			// 
			// UsersComboBox
			// 
			resources.ApplyResources(this.UsersComboBox, "UsersComboBox");
			this.UsersComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.UsersComboBox.Name = "UsersComboBox";
			// 
			// AllUsersRadioButton
			// 
			resources.ApplyResources(this.AllUsersRadioButton, "AllUsersRadioButton");
			this.AllUsersRadioButton.Name = "AllUsersRadioButton";
			// 
			// StandardRadioButton
			// 
			resources.ApplyResources(this.StandardRadioButton, "StandardRadioButton");
			this.StandardRadioButton.Name = "StandardRadioButton";
			// 
			// MenuMngDlgToolStrip
			// 
			resources.ApplyResources(this.MenuMngDlgToolStrip, "MenuMngDlgToolStrip");
			this.MenuMngDlgToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.MenuMngDlgToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SearchToolStripButton,
            this.RefreshMenuLoaderToolStripButton});
			this.MenuMngDlgToolStrip.Name = "MenuMngDlgToolStrip";
			// 
			// SearchToolStripButton
			// 
			this.SearchToolStripButton.ForeColor = System.Drawing.Color.Navy;
			resources.ApplyResources(this.SearchToolStripButton, "SearchToolStripButton");
			this.SearchToolStripButton.Name = "SearchToolStripButton";
			this.SearchToolStripButton.Click += new System.EventHandler(this.SearchToolStripButton_Click);
			// 
			// RefreshMenuLoaderToolStripButton
			// 
			this.RefreshMenuLoaderToolStripButton.ForeColor = System.Drawing.Color.Navy;
			resources.ApplyResources(this.RefreshMenuLoaderToolStripButton, "RefreshMenuLoaderToolStripButton");
			this.RefreshMenuLoaderToolStripButton.Name = "RefreshMenuLoaderToolStripButton";
			this.RefreshMenuLoaderToolStripButton.Click += new System.EventHandler(this.RefreshMenuLoaderToolStripButton_Click);
			// 
			// MenuMngDlgMenuStrip
			// 
			this.MenuMngDlgMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OptionsToolStripMenuItem});
			resources.ApplyResources(this.MenuMngDlgMenuStrip, "MenuMngDlgMenuStrip");
			this.MenuMngDlgMenuStrip.Name = "MenuMngDlgMenuStrip";
			// 
			// OptionsToolStripMenuItem
			// 
			this.OptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SearchToolStripMenuItem,
            this.RefreshMenuLoaderToolStripMenuItem});
			this.OptionsToolStripMenuItem.Name = "OptionsToolStripMenuItem";
			resources.ApplyResources(this.OptionsToolStripMenuItem, "OptionsToolStripMenuItem");
			// 
			// SearchToolStripMenuItem
			// 
			resources.ApplyResources(this.SearchToolStripMenuItem, "SearchToolStripMenuItem");
			this.SearchToolStripMenuItem.Name = "SearchToolStripMenuItem";
			this.SearchToolStripMenuItem.Click += new System.EventHandler(this.SearchToolStripMenuItem_Click);
			// 
			// RefreshMenuLoaderToolStripMenuItem
			// 
			resources.ApplyResources(this.RefreshMenuLoaderToolStripMenuItem, "RefreshMenuLoaderToolStripMenuItem");
			this.RefreshMenuLoaderToolStripMenuItem.Name = "RefreshMenuLoaderToolStripMenuItem";
			this.RefreshMenuLoaderToolStripMenuItem.Click += new System.EventHandler(this.RefreshMenuLoaderToolStripMenuItem_Click);
			// 
			// OKSaveButton
			// 
			resources.ApplyResources(this.OKSaveButton, "OKSaveButton");
			this.OKSaveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKSaveButton.Name = "OKSaveButton";
			this.OKSaveButton.Click += new System.EventHandler(this.OKSaveButton_Click);
			// 
			// CancelSaveButton
			// 
			resources.ApplyResources(this.CancelSaveButton, "CancelSaveButton");
			this.CancelSaveButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelSaveButton.Name = "CancelSaveButton";
			// 
			// MenuManagerWinCtrl
			// 
			this.MenuManagerWinCtrl.AllUsersCommandForeColor = System.Drawing.Color.Blue;
			resources.ApplyResources(this.MenuManagerWinCtrl, "MenuManagerWinCtrl");
			this.MenuManagerWinCtrl.ApplicationsPanelWidth = 220;
			this.MenuManagerWinCtrl.CurrentUserCommandForeColor = System.Drawing.Color.Green;
			this.MenuManagerWinCtrl.EnhancedCommandsViewEnabled = true;
			this.MenuManagerWinCtrl.KeyboardInputEnabled = true;
			this.MenuManagerWinCtrl.LoginManager = null;
			this.MenuManagerWinCtrl.MenuTreeWidth = 280;
			this.MenuManagerWinCtrl.MenuXmlParser = null;
			this.MenuManagerWinCtrl.MinimumSize = new System.Drawing.Size(892, 400);
			this.MenuManagerWinCtrl.Name = "MenuManagerWinCtrl";
			this.MenuManagerWinCtrl.PathFinder = null;
			this.MenuManagerWinCtrl.ShowEnhancedCommandsDescriptions = false;
			this.MenuManagerWinCtrl.ShowEnhancedCommandsReportDates = false;
			this.MenuManagerWinCtrl.ShowEnhancedCommandsToolBar = false;
			this.MenuManagerWinCtrl.ShowEnhancedCommandsView = false;
			this.MenuManagerWinCtrl.ShowTreeItemsStateImages = false;
			this.MenuManagerWinCtrl.RunCommand += new MenuMngCtrlEventHandler(this.MenuManagerWinCtrl_RunCommand);
			this.MenuManagerWinCtrl.SelectedMenuChanged += new MenuMngCtrlTreeViewEventHandler(this.MenuManagerWinCtrl_SelectedMenuChanged);
			this.MenuManagerWinCtrl.SelectedCommandChanged += new MenuMngCtrlEventHandler(this.MenuManagerWinCtrl_SelectedCommandChanged);
			// 
			// MenuFormStatusBar
			// 
			resources.ApplyResources(this.MenuFormStatusStrip, "MenuFormStatusBar");
			this.MenuFormStatusStrip.InfoProgressMaximum = 0;
			this.MenuFormStatusStrip.InfoProgressMinimum = 0;
			this.MenuFormStatusStrip.InfoProgressStep = 0;
			this.MenuFormStatusStrip.InfoProgressValue = 0;
			
			this.MenuFormStatusStrip.IsProgressBarVisible = false;
			this.MenuFormStatusStrip.Name = "MenuFormStatusBar";
            //this.MenuFormStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            //this.CompanyStatusStripLabel,
            //this.UserStatusStripLabel});
			this.MenuFormStatusStrip.SizingGrip = false;
			// 
			// CompanyStatusBarPanel
			// 
			this.CompanyStatusStripLabel.AutoSize = true;
			// 
			// UserStatusBarPanel
			// 
			this.UserStatusStripLabel.AutoSize = true;
			// 
			// SaveAsMenuMngDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.CancelSaveButton);
			this.Controls.Add(this.OKSaveButton);
			this.Controls.Add(this.MenuMngDlgToolStripContainer);
			this.Controls.Add(this.MenuFormStatusStrip);
			this.Controls.Add(this.MenuMngDlgMenuStrip);
			this.MainMenuStrip = this.MenuMngDlgMenuStrip;
			this.Name = "SaveAsMenuMngDialog";
			this.MenuMngDlgToolStripContainer.ContentPanel.ResumeLayout(false);
			this.MenuMngDlgToolStripContainer.ContentPanel.PerformLayout();
			this.MenuMngDlgToolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.MenuMngDlgToolStripContainer.TopToolStripPanel.PerformLayout();
			this.MenuMngDlgToolStripContainer.ResumeLayout(false);
			this.MenuMngDlgToolStripContainer.PerformLayout();
			this.MenuMngDlgToolStrip.ResumeLayout(false);
			this.MenuMngDlgToolStrip.PerformLayout();
			this.MenuMngDlgMenuStrip.ResumeLayout(false);
			this.MenuMngDlgMenuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer MenuMngDlgToolStripContainer;
        protected MenuMngWinCtrl MenuManagerWinCtrl;
        private System.Windows.Forms.ToolStrip MenuMngDlgToolStrip;
        private System.Windows.Forms.ToolStripButton SearchToolStripButton;
		private System.Windows.Forms.ToolStripButton RefreshMenuLoaderToolStripButton;
		protected System.Windows.Forms.ComboBox cbxFileType;

    }
}
