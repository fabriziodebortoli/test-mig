namespace Microarea.EasyBuilder.Packager
{
	partial class SolutionExplorer
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (tvFiles != null && tvFiles.Nodes != null)
				{
					System.IDisposable disposable = null;
					foreach (var node in tvFiles.Nodes)
					{
						disposable = node as System.IDisposable;
						if (disposable != null)
							disposable.Dispose();
					}
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SolutionExplorer));
            this.tscMain = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.tsCurrentFile = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tscCustomizations = new System.Windows.Forms.ToolStripContainer();
            this.lvApplications = new System.Windows.Forms.ListView();
            this.chApplications = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmsApplications = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiRenameApplication = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDeleteApplication = new System.Windows.Forms.ToolStripMenuItem();
            this.addModuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.publishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.AddApplicationToolstripButton = new System.Windows.Forms.ToolStripButton();
            this.tscFiles = new System.Windows.Forms.ToolStripContainer();
            this.tvFiles = new System.Windows.Forms.TreeView();
            this.cmsFilesManagement = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiDeleteModule = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRenameModule = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDisableModule = new System.Windows.Forms.ToolStripMenuItem();
            this.packModuleAssembliesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSetAsActiveModule = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDeleteItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSetActiveDocument = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiOpenFileLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiPublish = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripModules = new System.Windows.Forms.ToolStrip();
            this.AddModuleToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsExportApplication = new System.Windows.Forms.ToolStripButton();
            this.tsImportApplication = new System.Windows.Forms.ToolStripButton();
            this.tsRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsOptions = new System.Windows.Forms.ToolStripButton();
            this.tsShowDiagnostic = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.ofdAddFile = new System.Windows.Forms.OpenFileDialog();
            this.tscMain.BottomToolStripPanel.SuspendLayout();
            this.tscMain.ContentPanel.SuspendLayout();
            this.tscMain.TopToolStripPanel.SuspendLayout();
            this.tscMain.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tscCustomizations.ContentPanel.SuspendLayout();
            this.tscCustomizations.TopToolStripPanel.SuspendLayout();
            this.tscCustomizations.SuspendLayout();
            this.cmsApplications.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tscFiles.ContentPanel.SuspendLayout();
            this.tscFiles.TopToolStripPanel.SuspendLayout();
            this.tscFiles.SuspendLayout();
            this.cmsFilesManagement.SuspendLayout();
            this.toolStripModules.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tscMain
            // 
            // 
            // tscMain.BottomToolStripPanel
            // 
            this.tscMain.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // tscMain.ContentPanel
            // 
            this.tscMain.ContentPanel.Controls.Add(this.splitContainer1);
            resources.ApplyResources(this.tscMain.ContentPanel, "tscMain.ContentPanel");
            resources.ApplyResources(this.tscMain, "tscMain");
            this.tscMain.Name = "tscMain";
            // 
            // tscMain.TopToolStripPanel
            // 
            this.tscMain.TopToolStripPanel.Controls.Add(this.toolStrip1);
            this.tscMain.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // statusStrip1
            // 
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsStatus,
            this.tsProgressBar,
            this.tsCurrentFile});
            this.statusStrip1.Name = "statusStrip1";
            // 
            // tsStatus
            // 
            this.tsStatus.Name = "tsStatus";
            resources.ApplyResources(this.tsStatus, "tsStatus");
            // 
            // tsProgressBar
            // 
            resources.ApplyResources(this.tsProgressBar, "tsProgressBar");
            this.tsProgressBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.tsProgressBar.Name = "tsProgressBar";
            // 
            // tsCurrentFile
            // 
            this.tsCurrentFile.Name = "tsCurrentFile";
            resources.ApplyResources(this.tsCurrentFile, "tsCurrentFile");
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tscCustomizations);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tscFiles);
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // tscCustomizations
            // 
            // 
            // tscCustomizations.ContentPanel
            // 
            this.tscCustomizations.ContentPanel.Controls.Add(this.lvApplications);
            resources.ApplyResources(this.tscCustomizations.ContentPanel, "tscCustomizations.ContentPanel");
            resources.ApplyResources(this.tscCustomizations, "tscCustomizations");
            this.tscCustomizations.Name = "tscCustomizations";
            // 
            // tscCustomizations.TopToolStripPanel
            // 
            this.tscCustomizations.TopToolStripPanel.Controls.Add(this.toolStrip2);
            // 
            // lvApplications
            // 
            this.lvApplications.AllowDrop = true;
            this.lvApplications.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chApplications});
            this.lvApplications.ContextMenuStrip = this.cmsApplications;
            resources.ApplyResources(this.lvApplications, "lvApplications");
            this.lvApplications.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvApplications.HideSelection = false;
            this.lvApplications.MultiSelect = false;
            this.lvApplications.Name = "lvApplications";
            this.lvApplications.UseCompatibleStateImageBehavior = false;
            this.lvApplications.View = System.Windows.Forms.View.Details;
            this.lvApplications.SelectedIndexChanged += new System.EventHandler(this.lvCustomizations_SelectedIndexChanged);
            // 
            // chApplications
            // 
            resources.ApplyResources(this.chApplications, "chApplications");
            // 
            // cmsApplications
            // 
            this.cmsApplications.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiRenameApplication,
            this.tsmiDeleteApplication,
            this.addModuleToolStripMenuItem,
            this.publishToolStripMenuItem});
            this.cmsApplications.Name = "cmsCustomizations";
            resources.ApplyResources(this.cmsApplications, "cmsApplications");
            this.cmsApplications.Opening += new System.ComponentModel.CancelEventHandler(this.cmsApplications_Opening);
            // 
            // tsmiRenameApplication
            // 
            this.tsmiRenameApplication.Image = global::Microarea.EasyBuilder.Properties.Resources.RenameImage;
            this.tsmiRenameApplication.Name = "tsmiRenameApplication";
            resources.ApplyResources(this.tsmiRenameApplication, "tsmiRenameApplication");
            this.tsmiRenameApplication.Click += new System.EventHandler(this.renameCustomizationToolStripMenuItem_Click);
            // 
            // tsmiDeleteApplication
            // 
            this.tsmiDeleteApplication.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
            this.tsmiDeleteApplication.Name = "tsmiDeleteApplication";
            resources.ApplyResources(this.tsmiDeleteApplication, "tsmiDeleteApplication");
            this.tsmiDeleteApplication.Click += new System.EventHandler(this.deleteCustomizationToolStripMenuItem_Click);
            // 
            // addModuleToolStripMenuItem
            // 
            this.addModuleToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.AddModule;
            this.addModuleToolStripMenuItem.Name = "addModuleToolStripMenuItem";
            resources.ApplyResources(this.addModuleToolStripMenuItem, "addModuleToolStripMenuItem");
            this.addModuleToolStripMenuItem.Click += new System.EventHandler(this.addModuleToolStripMenuItem_Click_1);
            // 
            // publishToolStripMenuItem
            // 
            this.publishToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.PublishDocument;
            this.publishToolStripMenuItem.Name = "publishToolStripMenuItem";
            resources.ApplyResources(this.publishToolStripMenuItem, "publishToolStripMenuItem");
            this.publishToolStripMenuItem.Click += new System.EventHandler(this.PublishAllCustomizations);
            // 
            // toolStrip2
            // 
            resources.ApplyResources(this.toolStrip2, "toolStrip2");
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddApplicationToolstripButton});
            this.toolStrip2.Name = "toolStrip2";
            // 
            // AddApplicationToolstripButton
            // 
            this.AddApplicationToolstripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AddApplicationToolstripButton.Image = global::Microarea.EasyBuilder.Properties.Resources.Plus;
            resources.ApplyResources(this.AddApplicationToolstripButton, "AddApplicationToolstripButton");
            this.AddApplicationToolstripButton.Name = "AddApplicationToolstripButton";
            this.AddApplicationToolstripButton.Click += new System.EventHandler(this.AddApplicationToolstripButton_Click);
            // 
            // tscFiles
            // 
            // 
            // tscFiles.ContentPanel
            // 
            this.tscFiles.ContentPanel.Controls.Add(this.tvFiles);
            resources.ApplyResources(this.tscFiles.ContentPanel, "tscFiles.ContentPanel");
            resources.ApplyResources(this.tscFiles, "tscFiles");
            this.tscFiles.Name = "tscFiles";
            // 
            // tscFiles.TopToolStripPanel
            // 
            this.tscFiles.TopToolStripPanel.Controls.Add(this.toolStripModules);
            // 
            // tvFiles
            // 
            this.tvFiles.ContextMenuStrip = this.cmsFilesManagement;
            resources.ApplyResources(this.tvFiles, "tvFiles");
            this.tvFiles.HideSelection = false;
            this.tvFiles.Name = "tvFiles";
            this.tvFiles.ShowNodeToolTips = true;
            this.tvFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvFiles_AfterSelect);
            this.tvFiles.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tvFiles_MouseUp);
            // 
            // cmsFilesManagement
            // 
            this.cmsFilesManagement.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiDeleteModule,
            this.tsmiRenameModule,
            this.tsmiDisableModule,
            this.packModuleAssembliesToolStripMenuItem,
            this.tsmiSetAsActiveModule,
            this.tsmiDeleteItem,
            this.tsmiSetActiveDocument,
            this.tsmiOpenFileLocation,
            this.tsmiPublish});
            this.cmsFilesManagement.Name = "cmsFilesManagement";
            resources.ApplyResources(this.cmsFilesManagement, "cmsFilesManagement");
            this.cmsFilesManagement.Opening += new System.ComponentModel.CancelEventHandler(this.cmsFilesManagement_Opening);
            this.cmsFilesManagement.Opened += new System.EventHandler(this.cmsFilesManagement_Opened);
            // 
            // tsmiDeleteModule
            // 
            this.tsmiDeleteModule.Image = global::Microarea.EasyBuilder.Properties.Resources.DeleteModule;
            this.tsmiDeleteModule.Name = "tsmiDeleteModule";
            resources.ApplyResources(this.tsmiDeleteModule, "tsmiDeleteModule");
            this.tsmiDeleteModule.Click += new System.EventHandler(this.deleteModuleToolStripMenuItem_Click);
            // 
            // tsmiRenameModule
            // 
            this.tsmiRenameModule.Image = global::Microarea.EasyBuilder.Properties.Resources.RenameImage;
            this.tsmiRenameModule.Name = "tsmiRenameModule";
            resources.ApplyResources(this.tsmiRenameModule, "tsmiRenameModule");
            this.tsmiRenameModule.Click += new System.EventHandler(this.renameModuleToolStripMenuItem_Click);
            // 
            // tsmiDisableModule
            // 
            this.tsmiDisableModule.Image = global::Microarea.EasyBuilder.Properties.Resources.Off;
            this.tsmiDisableModule.Name = "tsmiDisableModule";
            resources.ApplyResources(this.tsmiDisableModule, "tsmiDisableModule");
            this.tsmiDisableModule.Click += new System.EventHandler(this.tsmiDisableModule_Click);
            // 
            // packModuleAssembliesToolStripMenuItem
            // 
            this.packModuleAssembliesToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Build24;
            this.packModuleAssembliesToolStripMenuItem.Name = "packModuleAssembliesToolStripMenuItem";
            resources.ApplyResources(this.packModuleAssembliesToolStripMenuItem, "packModuleAssembliesToolStripMenuItem");
            this.packModuleAssembliesToolStripMenuItem.Click += new System.EventHandler(this.GroupModuleAssemblies);
            // 
            // tsmiSetAsActiveModule
            // 
            this.tsmiSetAsActiveModule.Image = global::Microarea.EasyBuilder.Properties.Resources.DefaultCustomization;
            this.tsmiSetAsActiveModule.Name = "tsmiSetAsActiveModule";
            resources.ApplyResources(this.tsmiSetAsActiveModule, "tsmiSetAsActiveModule");
            this.tsmiSetAsActiveModule.Click += new System.EventHandler(this.setAsActiveModuleToolStripMenuItem_Click);
            // 
            // tsmiDeleteItem
            // 
            this.tsmiDeleteItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
            this.tsmiDeleteItem.Name = "tsmiDeleteItem";
            resources.ApplyResources(this.tsmiDeleteItem, "tsmiDeleteItem");
            this.tsmiDeleteItem.Click += new System.EventHandler(this.deleteItemToolStripMenuItem_Click);
            // 
            // tsmiSetActiveDocument
            // 
            this.tsmiSetActiveDocument.Image = global::Microarea.EasyBuilder.Properties.Resources.ActiveDocument;
            this.tsmiSetActiveDocument.Name = "tsmiSetActiveDocument";
            resources.ApplyResources(this.tsmiSetActiveDocument, "tsmiSetActiveDocument");
            this.tsmiSetActiveDocument.Click += new System.EventHandler(this.setActiveDocumentToolStripMenuItem_Click);
            // 
            // tsmiOpenFileLocation
            // 
            this.tsmiOpenFileLocation.Image = global::Microarea.EasyBuilder.Properties.Resources.Open;
            this.tsmiOpenFileLocation.Name = "tsmiOpenFileLocation";
            resources.ApplyResources(this.tsmiOpenFileLocation, "tsmiOpenFileLocation");
            this.tsmiOpenFileLocation.Click += new System.EventHandler(this.openFileLocationToolStripMenuItem_Click);
            // 
            // tsmiPublish
            // 
            this.tsmiPublish.Image = global::Microarea.EasyBuilder.Properties.Resources.PublishDocument;
            this.tsmiPublish.Name = "tsmiPublish";
            resources.ApplyResources(this.tsmiPublish, "tsmiPublish");
            this.tsmiPublish.Click += new System.EventHandler(this.tsmiPublishCustomization_Click);
            // 
            // toolStripModules
            // 
            resources.ApplyResources(this.toolStripModules, "toolStripModules");
            this.toolStripModules.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripModules.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddModuleToolStripButton});
            this.toolStripModules.Name = "toolStripModules";
            // 
            // AddModuleToolStripButton
            // 
            this.AddModuleToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AddModuleToolStripButton.Image = global::Microarea.EasyBuilder.Properties.Resources.AddModule;
            resources.ApplyResources(this.AddModuleToolStripButton, "AddModuleToolStripButton");
            this.AddModuleToolStripButton.Name = "AddModuleToolStripButton";
            this.AddModuleToolStripButton.Click += new System.EventHandler(this.AddModuleToolStripButton_Click);
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsExportApplication,
            this.tsImportApplication,
            this.tsRefresh,
            this.toolStripSeparator1,
            this.tsOptions,
            this.tsShowDiagnostic,
            this.toolStripSeparator5});
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // tsExportApplication
            // 
            this.tsExportApplication.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsExportApplication.Image = global::Microarea.EasyBuilder.Properties.Resources.Export;
            resources.ApplyResources(this.tsExportApplication, "tsExportApplication");
            this.tsExportApplication.Name = "tsExportApplication";
            this.tsExportApplication.Click += new System.EventHandler(this.tsExportPackage_Click);
            // 
            // tsImportApplication
            // 
            this.tsImportApplication.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsImportApplication.Image = global::Microarea.EasyBuilder.Properties.Resources.Import;
            resources.ApplyResources(this.tsImportApplication, "tsImportApplication");
            this.tsImportApplication.Name = "tsImportApplication";
            this.tsImportApplication.Click += new System.EventHandler(this.tsImportPackage_Click);
            // 
            // tsRefresh
            // 
            this.tsRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsRefresh.Image = global::Microarea.EasyBuilder.Properties.Resources.Refresh;
            resources.ApplyResources(this.tsRefresh, "tsRefresh");
            this.tsRefresh.Name = "tsRefresh";
            this.tsRefresh.Click += new System.EventHandler(this.tsRefresh_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // tsOptions
            // 
            this.tsOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsOptions.Image = global::Microarea.EasyBuilder.Properties.Resources.Options24;
            resources.ApplyResources(this.tsOptions, "tsOptions");
            this.tsOptions.Name = "tsOptions";
            this.tsOptions.Click += new System.EventHandler(this.tsOptions_Click);
            // 
            // tsShowDiagnostic
            // 
            this.tsShowDiagnostic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tsShowDiagnostic, "tsShowDiagnostic");
            this.tsShowDiagnostic.Image = global::Microarea.EasyBuilder.Properties.Resources.Diagnostic;
            this.tsShowDiagnostic.Name = "tsShowDiagnostic";
            this.tsShowDiagnostic.Click += new System.EventHandler(this.tsShowDiagnostic_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // ofdAddFile
            // 
            this.ofdAddFile.Multiselect = true;
            this.ofdAddFile.FileOk += new System.ComponentModel.CancelEventHandler(this.ofdAddFile_FileOk);
            // 
            // SolutionExplorer
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tscMain);
            this.Name = "SolutionExplorer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CustomListViewer_FormClosing);
            this.tscMain.BottomToolStripPanel.ResumeLayout(false);
            this.tscMain.BottomToolStripPanel.PerformLayout();
            this.tscMain.ContentPanel.ResumeLayout(false);
            this.tscMain.TopToolStripPanel.ResumeLayout(false);
            this.tscMain.TopToolStripPanel.PerformLayout();
            this.tscMain.ResumeLayout(false);
            this.tscMain.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tscCustomizations.ContentPanel.ResumeLayout(false);
            this.tscCustomizations.TopToolStripPanel.ResumeLayout(false);
            this.tscCustomizations.TopToolStripPanel.PerformLayout();
            this.tscCustomizations.ResumeLayout(false);
            this.tscCustomizations.PerformLayout();
            this.cmsApplications.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tscFiles.ContentPanel.ResumeLayout(false);
            this.tscFiles.TopToolStripPanel.ResumeLayout(false);
            this.tscFiles.TopToolStripPanel.PerformLayout();
            this.tscFiles.ResumeLayout(false);
            this.tscFiles.PerformLayout();
            this.cmsFilesManagement.ResumeLayout(false);
            this.toolStripModules.ResumeLayout(false);
            this.toolStripModules.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer tscMain;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.OpenFileDialog ofdAddFile;
		private System.Windows.Forms.ContextMenuStrip cmsFilesManagement;
		private System.Windows.Forms.ToolStripMenuItem tsmiDeleteItem;
		private System.Windows.Forms.ToolStripButton tsExportApplication;
		private System.Windows.Forms.ToolStripButton tsImportApplication;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel tsStatus;
		private System.Windows.Forms.ToolStripProgressBar tsProgressBar;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.TreeView tvFiles;
		private System.Windows.Forms.ToolStripContainer tscCustomizations;
		private System.Windows.Forms.ToolStripContainer tscFiles;
		private System.Windows.Forms.ContextMenuStrip cmsApplications;
		private System.Windows.Forms.ToolStripMenuItem tsmiRenameApplication;
		private System.Windows.Forms.ToolStripStatusLabel tsCurrentFile;
		private System.Windows.Forms.ToolStripMenuItem tsmiDeleteApplication;
		private System.Windows.Forms.ToolStripButton tsOptions;
		private System.Windows.Forms.ToolStripButton tsShowDiagnostic;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem tsmiSetAsActiveModule;
		private System.Windows.Forms.ToolStripMenuItem tsmiDeleteModule;
		private System.Windows.Forms.ToolStripMenuItem tsmiRenameModule;
		private System.Windows.Forms.ToolStripMenuItem tsmiSetActiveDocument;
		private System.Windows.Forms.ToolStripMenuItem tsmiDisableModule;
		private System.Windows.Forms.ToolStripButton tsRefresh;
		private System.Windows.Forms.ToolStripMenuItem tsmiOpenFileLocation;
		private System.Windows.Forms.ToolStripMenuItem tsmiPublish;
		private System.Windows.Forms.ListView lvApplications;
		private System.Windows.Forms.ColumnHeader chApplications;
		private System.Windows.Forms.ToolStrip toolStrip2;
		private System.Windows.Forms.ToolStripButton AddApplicationToolstripButton;
		private System.Windows.Forms.ToolStrip toolStripModules;
		private System.Windows.Forms.ToolStripButton AddModuleToolStripButton;
		private System.Windows.Forms.ToolStripMenuItem addModuleToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem publishToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem packModuleAssembliesToolStripMenuItem;
	}
}