namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	partial class LogViewer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogViewer));
			this.SelToolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.FilesListView = new System.Windows.Forms.ListView();
			this.TypeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.FileColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.CompanyColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.UserColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.DataColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.FilesContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.OpenWithInternetExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CopyPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.FilesDeleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.LogImageList = new System.Windows.Forms.ImageList(this.components);
			this.FiltersToolStrip = new System.Windows.Forms.ToolStrip();
			this.FromDateToolStripLabel = new System.Windows.Forms.ToolStripLabel();
			this.ToDateToolStripLabel = new System.Windows.Forms.ToolStripLabel();
			this.MessagesToolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.MessagesListView = new System.Windows.Forms.ListView();
			this.DateColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.TimeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.DescriColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.MsgToolStrip = new System.Windows.Forms.ToolStrip();
			this.ErrorToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.WarningToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.InfoToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.TotErrorsToolStripLabel = new System.Windows.Forms.ToolStripLabel();
			this.SelectionsPanel = new System.Windows.Forms.Panel();
			this.SplitContainer = new System.Windows.Forms.SplitContainer();
			this.SelToolStripContainer.ContentPanel.SuspendLayout();
			this.SelToolStripContainer.TopToolStripPanel.SuspendLayout();
			this.SelToolStripContainer.SuspendLayout();
			this.FilesContextMenuStrip.SuspendLayout();
			this.FiltersToolStrip.SuspendLayout();
			this.MessagesToolStripContainer.ContentPanel.SuspendLayout();
			this.MessagesToolStripContainer.TopToolStripPanel.SuspendLayout();
			this.MessagesToolStripContainer.SuspendLayout();
			this.MsgToolStrip.SuspendLayout();
			this.SelectionsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// SelToolStripContainer
			// 
			// 
			// SelToolStripContainer.ContentPanel
			// 
			this.SelToolStripContainer.ContentPanel.Controls.Add(this.FilesListView);
			resources.ApplyResources(this.SelToolStripContainer.ContentPanel, "SelToolStripContainer.ContentPanel");
			resources.ApplyResources(this.SelToolStripContainer, "SelToolStripContainer");
			this.SelToolStripContainer.Name = "SelToolStripContainer";
			// 
			// SelToolStripContainer.TopToolStripPanel
			// 
			this.SelToolStripContainer.TopToolStripPanel.Controls.Add(this.FiltersToolStrip);
			// 
			// FilesListView
			// 
			this.FilesListView.AllowColumnReorder = true;
			this.FilesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TypeColumnHeader,
            this.FileColumnHeader,
            this.CompanyColumnHeader,
            this.UserColumnHeader,
            this.DataColumnHeader});
			this.FilesListView.ContextMenuStrip = this.FilesContextMenuStrip;
			resources.ApplyResources(this.FilesListView, "FilesListView");
			this.FilesListView.FullRowSelect = true;
			this.FilesListView.HideSelection = false;
			this.FilesListView.LargeImageList = this.LogImageList;
			this.FilesListView.MultiSelect = false;
			this.FilesListView.Name = "FilesListView";
			this.FilesListView.SmallImageList = this.LogImageList;
			this.FilesListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.FilesListView.UseCompatibleStateImageBehavior = false;
			this.FilesListView.View = System.Windows.Forms.View.Details;
			this.FilesListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.FilesListView_ColumnClick);
			this.FilesListView.ItemActivate += new System.EventHandler(this.FilesListView_ItemActivate);
			this.FilesListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.FilesListView_ItemSelectionChanged);
			// 
			// TypeColumnHeader
			// 
			resources.ApplyResources(this.TypeColumnHeader, "TypeColumnHeader");
			// 
			// FileColumnHeader
			// 
			resources.ApplyResources(this.FileColumnHeader, "FileColumnHeader");
			// 
			// CompanyColumnHeader
			// 
			resources.ApplyResources(this.CompanyColumnHeader, "CompanyColumnHeader");
			// 
			// UserColumnHeader
			// 
			resources.ApplyResources(this.UserColumnHeader, "UserColumnHeader");
			// 
			// DataColumnHeader
			// 
			resources.ApplyResources(this.DataColumnHeader, "DataColumnHeader");
			// 
			// FilesContextMenuStrip
			// 
			resources.ApplyResources(this.FilesContextMenuStrip, "FilesContextMenuStrip");
			this.FilesContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenWithInternetExplorerToolStripMenuItem,
            this.CopyPathToolStripMenuItem,
            this.toolStripSeparator1,
            this.FilesDeleteFileToolStripMenuItem});
			this.FilesContextMenuStrip.Name = "FilesContextMenuStrip";
			// 
			// OpenWithInternetExplorerToolStripMenuItem
			// 
			this.OpenWithInternetExplorerToolStripMenuItem.Image = global::Microarea.TaskBuilderNet.Core.DiagnosticUI.DiagnosticViewerStrings.IExplorer;
			resources.ApplyResources(this.OpenWithInternetExplorerToolStripMenuItem, "OpenWithInternetExplorerToolStripMenuItem");
			this.OpenWithInternetExplorerToolStripMenuItem.Name = "OpenWithInternetExplorerToolStripMenuItem";
			this.OpenWithInternetExplorerToolStripMenuItem.Click += new System.EventHandler(this.OpenWithInternetExplorerToolStripMenuItem_Click);
			// 
			// CopyPathToolStripMenuItem
			// 
			this.CopyPathToolStripMenuItem.Image = global::Microarea.TaskBuilderNet.Core.DiagnosticUI.DiagnosticViewerStrings.CopyPath;
			resources.ApplyResources(this.CopyPathToolStripMenuItem, "CopyPathToolStripMenuItem");
			this.CopyPathToolStripMenuItem.Name = "CopyPathToolStripMenuItem";
			this.CopyPathToolStripMenuItem.Click += new System.EventHandler(this.CopyPathToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// FilesDeleteFileToolStripMenuItem
			// 
			this.FilesDeleteFileToolStripMenuItem.Image = global::Microarea.TaskBuilderNet.Core.DiagnosticUI.DiagnosticViewerStrings.RecycleBinEmpty;
			resources.ApplyResources(this.FilesDeleteFileToolStripMenuItem, "FilesDeleteFileToolStripMenuItem");
			this.FilesDeleteFileToolStripMenuItem.Name = "FilesDeleteFileToolStripMenuItem";
			this.FilesDeleteFileToolStripMenuItem.Click += new System.EventHandler(this.FilesDeleteFileToolStripMenuItem_Click);
			// 
			// LogImageList
			// 
			this.LogImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("LogImageList.ImageStream")));
			this.LogImageList.TransparentColor = System.Drawing.Color.Magenta;
			this.LogImageList.Images.SetKeyName(0, "Error.bmp");
			this.LogImageList.Images.SetKeyName(1, "Warning.bmp");
			this.LogImageList.Images.SetKeyName(2, "Information.bmp");
			this.LogImageList.Images.SetKeyName(3, "ApplicationXml.bmp");
			this.LogImageList.Images.SetKeyName(4, "DatabaseXml.bmp");
			this.LogImageList.Images.SetKeyName(5, "GenericXml.bmp");
			this.LogImageList.Images.SetKeyName(6, "IExplorer.bmp");
			this.LogImageList.Images.SetKeyName(7, "CopyPath.bmp");
			this.LogImageList.Images.SetKeyName(8, "DeleteFile.bmp");
			this.LogImageList.Images.SetKeyName(9, "RecycleBinEmpty.png");
			this.LogImageList.Images.SetKeyName(10, "DataManager.png");
			// 
			// FiltersToolStrip
			// 
			resources.ApplyResources(this.FiltersToolStrip, "FiltersToolStrip");
			this.FiltersToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.FiltersToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FromDateToolStripLabel,
            this.ToDateToolStripLabel});
			this.FiltersToolStrip.Name = "FiltersToolStrip";
			// 
			// FromDateToolStripLabel
			// 
			resources.ApplyResources(this.FromDateToolStripLabel, "FromDateToolStripLabel");
			this.FromDateToolStripLabel.Image = global::Microarea.TaskBuilderNet.Core.DiagnosticUI.DiagnosticViewerStrings.OrderByDate;
			this.FromDateToolStripLabel.Name = "FromDateToolStripLabel";
			// 
			// ToDateToolStripLabel
			// 
			resources.ApplyResources(this.ToDateToolStripLabel, "ToDateToolStripLabel");
			this.ToDateToolStripLabel.Image = global::Microarea.TaskBuilderNet.Core.DiagnosticUI.DiagnosticViewerStrings.OrderByDate;
			this.ToDateToolStripLabel.Name = "ToDateToolStripLabel";
			// 
			// MessagesToolStripContainer
			// 
			// 
			// MessagesToolStripContainer.ContentPanel
			// 
			this.MessagesToolStripContainer.ContentPanel.Controls.Add(this.MessagesListView);
			resources.ApplyResources(this.MessagesToolStripContainer.ContentPanel, "MessagesToolStripContainer.ContentPanel");
			resources.ApplyResources(this.MessagesToolStripContainer, "MessagesToolStripContainer");
			this.MessagesToolStripContainer.Name = "MessagesToolStripContainer";
			// 
			// MessagesToolStripContainer.TopToolStripPanel
			// 
			this.MessagesToolStripContainer.TopToolStripPanel.Controls.Add(this.MsgToolStrip);
			// 
			// MessagesListView
			// 
			this.MessagesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DateColumnHeader,
            this.TimeColumnHeader,
            this.DescriColumnHeader});
			resources.ApplyResources(this.MessagesListView, "MessagesListView");
			this.MessagesListView.FullRowSelect = true;
			this.MessagesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.MessagesListView.HideSelection = false;
			this.MessagesListView.LargeImageList = this.LogImageList;
			this.MessagesListView.Name = "MessagesListView";
			this.MessagesListView.SmallImageList = this.LogImageList;
			this.MessagesListView.UseCompatibleStateImageBehavior = false;
			this.MessagesListView.View = System.Windows.Forms.View.Details;
			this.MessagesListView.DoubleClick += new System.EventHandler(this.MessagesListView_DoubleClick);
			// 
			// DateColumnHeader
			// 
			resources.ApplyResources(this.DateColumnHeader, "DateColumnHeader");
			// 
			// TimeColumnHeader
			// 
			resources.ApplyResources(this.TimeColumnHeader, "TimeColumnHeader");
			// 
			// DescriColumnHeader
			// 
			resources.ApplyResources(this.DescriColumnHeader, "DescriColumnHeader");
			// 
			// MsgToolStrip
			// 
			resources.ApplyResources(this.MsgToolStrip, "MsgToolStrip");
			this.MsgToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.MsgToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ErrorToolStripButton,
            this.WarningToolStripButton,
            this.InfoToolStripButton,
            this.TotErrorsToolStripLabel});
			this.MsgToolStrip.Name = "MsgToolStrip";
			// 
			// ErrorToolStripButton
			// 
			this.ErrorToolStripButton.Checked = true;
			this.ErrorToolStripButton.CheckOnClick = true;
			this.ErrorToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ErrorToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.ErrorToolStripButton, "ErrorToolStripButton");
			this.ErrorToolStripButton.Name = "ErrorToolStripButton";
			this.ErrorToolStripButton.CheckedChanged += new System.EventHandler(this.ErrorToolStripButton_CheckedChanged);
			// 
			// WarningToolStripButton
			// 
			this.WarningToolStripButton.Checked = true;
			this.WarningToolStripButton.CheckOnClick = true;
			this.WarningToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.WarningToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.WarningToolStripButton, "WarningToolStripButton");
			this.WarningToolStripButton.Name = "WarningToolStripButton";
			this.WarningToolStripButton.CheckedChanged += new System.EventHandler(this.WarningToolStripButton_CheckedChanged);
			// 
			// InfoToolStripButton
			// 
			this.InfoToolStripButton.Checked = true;
			this.InfoToolStripButton.CheckOnClick = true;
			this.InfoToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.InfoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.InfoToolStripButton, "InfoToolStripButton");
			this.InfoToolStripButton.Name = "InfoToolStripButton";
			this.InfoToolStripButton.CheckedChanged += new System.EventHandler(this.InfoToolStripButton_CheckedChanged);
			// 
			// TotErrorsToolStripLabel
			// 
			this.TotErrorsToolStripLabel.Name = "TotErrorsToolStripLabel";
			resources.ApplyResources(this.TotErrorsToolStripLabel, "TotErrorsToolStripLabel");
			// 
			// SelectionsPanel
			// 
			this.SelectionsPanel.Controls.Add(this.SelToolStripContainer);
			resources.ApplyResources(this.SelectionsPanel, "SelectionsPanel");
			this.SelectionsPanel.Name = "SelectionsPanel";
			// 
			// SplitContainer
			// 
			this.SplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.SplitContainer, "SplitContainer");
			this.SplitContainer.Name = "SplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.SelectionsPanel);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.MessagesToolStripContainer);
			// 
			// LogViewer
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.SplitContainer);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "LogViewer";
			this.SelToolStripContainer.ContentPanel.ResumeLayout(false);
			this.SelToolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.SelToolStripContainer.TopToolStripPanel.PerformLayout();
			this.SelToolStripContainer.ResumeLayout(false);
			this.SelToolStripContainer.PerformLayout();
			this.FilesContextMenuStrip.ResumeLayout(false);
			this.FiltersToolStrip.ResumeLayout(false);
			this.FiltersToolStrip.PerformLayout();
			this.MessagesToolStripContainer.ContentPanel.ResumeLayout(false);
			this.MessagesToolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.MessagesToolStripContainer.TopToolStripPanel.PerformLayout();
			this.MessagesToolStripContainer.ResumeLayout(false);
			this.MessagesToolStripContainer.PerformLayout();
			this.MsgToolStrip.ResumeLayout(false);
			this.MsgToolStrip.PerformLayout();
			this.SelectionsPanel.ResumeLayout(false);
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripLabel FromDateToolStripLabel;
		private System.Windows.Forms.ToolStripLabel ToDateToolStripLabel;
		private System.Windows.Forms.ListView MessagesListView;
		private System.Windows.Forms.ColumnHeader DateColumnHeader;
		private System.Windows.Forms.ColumnHeader TimeColumnHeader;
		private System.Windows.Forms.ColumnHeader DescriColumnHeader;
		private System.Windows.Forms.Panel SelectionsPanel;
		private System.Windows.Forms.ToolStripContainer SelToolStripContainer;
		private System.Windows.Forms.ListView FilesListView;
		private System.Windows.Forms.ColumnHeader FileColumnHeader;
		private System.Windows.Forms.ColumnHeader TypeColumnHeader;
		private System.Windows.Forms.ColumnHeader CompanyColumnHeader;
		private System.Windows.Forms.ColumnHeader UserColumnHeader;
		private System.Windows.Forms.ColumnHeader DataColumnHeader;
		private System.Windows.Forms.SplitContainer SplitContainer;
		private System.Windows.Forms.ToolStrip FiltersToolStrip;
		private System.Windows.Forms.ToolStripContainer MessagesToolStripContainer;
		private System.Windows.Forms.ToolStrip MsgToolStrip;
		private System.Windows.Forms.ToolStripButton ErrorToolStripButton;
		private System.Windows.Forms.ToolStripButton WarningToolStripButton;
		private System.Windows.Forms.ToolStripButton InfoToolStripButton;
		private System.Windows.Forms.ImageList LogImageList;
		private System.Windows.Forms.ToolStripLabel TotErrorsToolStripLabel;
		private System.Windows.Forms.ContextMenuStrip FilesContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem OpenWithInternetExplorerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem CopyPathToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem FilesDeleteFileToolStripMenuItem;




	}
}