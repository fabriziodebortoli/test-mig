
namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
    partial class MenuManagerDialog : System.Windows.Forms.Form
    {
        private System.Windows.Forms.ToolStripContainer MenuMngDlgToolStripContainer;
        private System.Windows.Forms.MenuStrip MenuMngDlgMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem OptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem NormalViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FavoritesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EnvironmentToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem SearchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RefreshMenuLoaderToolStripMenuItem;
        private System.Windows.Forms.ToolStrip MenuMngDlgToolStrip;
        private System.Windows.Forms.ToolStripButton NormalViewToolStripButton;
        private System.Windows.Forms.ToolStripButton FavoritesToolStripButton;
        private System.Windows.Forms.ToolStripButton EnvironmentToolStripButton;
        private System.Windows.Forms.ToolStripSeparator MenuMngDlgToolStripSeparator;
        private System.Windows.Forms.ToolStripButton SearchToolStripButton;
        private System.Windows.Forms.ToolStripButton RefreshMenuLoaderToolStripButton;
        protected MenuMngWinCtrl MenuManagerWinCtrl;
        private MenuManagerStatusStrip MenuFormStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel CompanyStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel UserStripStatusLabel;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button OkButton;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuManagerDialog));
			this.MenuMngDlgToolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.MenuManagerWinCtrl = new Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.MenuMngWinCtrl();
			this.MenuMngDlgToolStrip = new System.Windows.Forms.ToolStrip();
			this.NormalViewToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.FavoritesToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.EnvironmentToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.MenuMngDlgToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.SearchToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.RefreshMenuLoaderToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.CancelButton = new System.Windows.Forms.Button();
			this.OkButton = new System.Windows.Forms.Button();
			this.MenuMngDlgMenuStrip = new System.Windows.Forms.MenuStrip();
			this.OptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.NormalViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.FavoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.EnvironmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.SearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RefreshMenuLoaderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuFormStatusStrip = new Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.MenuManagerStatusStrip();
            this.CompanyStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.UserStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
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
			this.MenuMngDlgToolStripContainer.ContentPanel.Controls.Add(this.MenuManagerWinCtrl);
			resources.ApplyResources(this.MenuMngDlgToolStripContainer.ContentPanel, "MenuMngDlgToolStripContainer.ContentPanel");
			resources.ApplyResources(this.MenuMngDlgToolStripContainer, "MenuMngDlgToolStripContainer");
			this.MenuMngDlgToolStripContainer.Name = "MenuMngDlgToolStripContainer";
			// 
			// MenuMngDlgToolStripContainer.TopToolStripPanel
			// 
			this.MenuMngDlgToolStripContainer.TopToolStripPanel.Controls.Add(this.MenuMngDlgToolStrip);
			// 
			// MenuManagerWinCtrl
			// 
			this.MenuManagerWinCtrl.AllUsersCommandForeColor = System.Drawing.Color.Blue;
			this.MenuManagerWinCtrl.ApplicationsPanelWidth = 210;

			this.MenuManagerWinCtrl.CurrentUserCommandForeColor = System.Drawing.Color.Green;
			resources.ApplyResources(this.MenuManagerWinCtrl, "MenuManagerWinCtrl");
			this.MenuManagerWinCtrl.EnhancedCommandsViewEnabled = false;
			this.MenuManagerWinCtrl.KeyboardInputEnabled = true;
			this.MenuManagerWinCtrl.LoginManager = null;
			this.MenuManagerWinCtrl.MenuTreeWidth = 200;
			this.MenuManagerWinCtrl.MenuXmlParser = null;
			this.MenuManagerWinCtrl.Name = "MenuManagerWinCtrl";
			this.MenuManagerWinCtrl.PathFinder = null;
			this.MenuManagerWinCtrl.ShowEnhancedCommandsDescriptions = false;
			this.MenuManagerWinCtrl.ShowEnhancedCommandsReportDates = false;
			this.MenuManagerWinCtrl.ShowEnhancedCommandsToolBar = false;
			this.MenuManagerWinCtrl.ShowEnhancedCommandsView = false;
			this.MenuManagerWinCtrl.ShowTreeItemsStateImages = false;
			this.MenuManagerWinCtrl.SelectedCommandChanged += new Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.MenuMngCtrlEventHandler(this.MenuManagerWinCtrl_SelectedCommandChanged);
			this.MenuManagerWinCtrl.RunCommand += new Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.MenuMngCtrlEventHandler(this.MenuManagerWinCtrl_RunCommand);
			// 
			// MenuMngDlgToolStrip
			// 
			resources.ApplyResources(this.MenuMngDlgToolStrip, "MenuMngDlgToolStrip");
			this.MenuMngDlgToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.MenuMngDlgToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NormalViewToolStripButton,
            this.FavoritesToolStripButton,
            this.EnvironmentToolStripButton,
            this.MenuMngDlgToolStripSeparator,
            this.SearchToolStripButton,
            this.RefreshMenuLoaderToolStripButton});
			this.MenuMngDlgToolStrip.Name = "MenuMngDlgToolStrip";
			// 
			// NormalViewToolStripButton
			// 
			this.NormalViewToolStripButton.ForeColor = System.Drawing.Color.Navy;
			resources.ApplyResources(this.NormalViewToolStripButton, "NormalViewToolStripButton");
			this.NormalViewToolStripButton.Name = "NormalViewToolStripButton";
			this.NormalViewToolStripButton.Click += new System.EventHandler(this.NormalViewToolStripButton_Click);
			// 
			// FavoritesToolStripButton
			// 
			this.FavoritesToolStripButton.ForeColor = System.Drawing.Color.Navy;
			resources.ApplyResources(this.FavoritesToolStripButton, "FavoritesToolStripButton");
			this.FavoritesToolStripButton.Name = "FavoritesToolStripButton";
			// 
			// EnvironmentToolStripButton
			// 
			this.EnvironmentToolStripButton.ForeColor = System.Drawing.Color.Navy;
			resources.ApplyResources(this.EnvironmentToolStripButton, "EnvironmentToolStripButton");
			this.EnvironmentToolStripButton.Name = "EnvironmentToolStripButton";
			// 
			// MenuMngDlgToolStripSeparator
			// 
			this.MenuMngDlgToolStripSeparator.Name = "MenuMngDlgToolStripSeparator";
			resources.ApplyResources(this.MenuMngDlgToolStripSeparator, "MenuMngDlgToolStripSeparator");
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
			// CancelButton
			// 
			resources.ApplyResources(this.CancelButton, "CancelButton");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Name = "CancelButton";
			// 
			// OkButton
			// 
			resources.ApplyResources(this.OkButton, "OkButton");
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkButton.Name = "OkButton";
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
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
            this.NormalViewToolStripMenuItem,
            this.FavoritesToolStripMenuItem,
            this.EnvironmentToolStripMenuItem,
            this.toolStripSeparator1,
            this.SearchToolStripMenuItem,
            this.RefreshMenuLoaderToolStripMenuItem});
			this.OptionsToolStripMenuItem.Name = "OptionsToolStripMenuItem";
			resources.ApplyResources(this.OptionsToolStripMenuItem, "OptionsToolStripMenuItem");
			// 
			// NormalViewToolStripMenuItem
			// 
			resources.ApplyResources(this.NormalViewToolStripMenuItem, "NormalViewToolStripMenuItem");
			this.NormalViewToolStripMenuItem.Name = "NormalViewToolStripMenuItem";
			this.NormalViewToolStripMenuItem.Click += new System.EventHandler(this.NormalViewToolStripMenuItem_Click);
			// 
			// FavoritesToolStripMenuItem
			// 
			resources.ApplyResources(this.FavoritesToolStripMenuItem, "FavoritesToolStripMenuItem");
			this.FavoritesToolStripMenuItem.Name = "FavoritesToolStripMenuItem";
			// 
			// EnvironmentToolStripMenuItem
			// 
			resources.ApplyResources(this.EnvironmentToolStripMenuItem, "EnvironmentToolStripMenuItem");
			this.EnvironmentToolStripMenuItem.Name = "EnvironmentToolStripMenuItem";
			this.EnvironmentToolStripMenuItem.Click += new System.EventHandler(this.EnvironmentToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// SearchToolStripMenuItem
			// 
			this.SearchToolStripMenuItem.Name = "SearchToolStripMenuItem";
			resources.ApplyResources(this.SearchToolStripMenuItem, "SearchToolStripMenuItem");
			this.SearchToolStripMenuItem.Click += new System.EventHandler(this.SearchToolStripMenuItem_Click);
			// 
			// RefreshMenuLoaderToolStripMenuItem
			// 
			this.RefreshMenuLoaderToolStripMenuItem.Name = "RefreshMenuLoaderToolStripMenuItem";
			resources.ApplyResources(this.RefreshMenuLoaderToolStripMenuItem, "RefreshMenuLoaderToolStripMenuItem");
			this.RefreshMenuLoaderToolStripMenuItem.Click += new System.EventHandler(this.RefreshMenuLoaderToolStripMenuItem_Click);
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
            this.MenuFormStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CompanyStripStatusLabel,
            this.UserStripStatusLabel});
			this.MenuFormStatusStrip.SizingGrip = false;
			// 
			// CompanyStatusBarPanel
			// 
			this.CompanyStripStatusLabel.AutoSize = true;
			// 
			// UserStatusBarPanel
			// 
			this.UserStripStatusLabel.AutoSize = true;
			// 
			// MenuManagerDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.MenuMngDlgToolStripContainer);
			this.Controls.Add(this.MenuFormStatusStrip);
			this.Controls.Add(this.MenuMngDlgMenuStrip);
			this.MainMenuStrip = this.MenuMngDlgMenuStrip;
			this.Name = "MenuManagerDialog";
			this.MenuMngDlgToolStripContainer.ContentPanel.ResumeLayout(false);
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
    }
}
