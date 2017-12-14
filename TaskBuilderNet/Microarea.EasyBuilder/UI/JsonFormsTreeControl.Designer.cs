using System;

namespace Microarea.EasyBuilder.UI
{
    partial class JsonFormsTreeControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JsonFormsTreeControl));
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.tsbAdd = new System.Windows.Forms.ToolStripButton();
			this.tsbRename = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbDelete = new System.Windows.Forms.ToolStripButton();
			this.tsbOpen = new System.Windows.Forms.ToolStripButton();
			this.tsbRefresh = new System.Windows.Forms.ToolStripButton();
			this.tsbOpenLocation = new System.Windows.Forms.ToolStripButton();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cmsFormEditorContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openInNewWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.treeViewManager = new System.Windows.Forms.TreeView();
			this.treeFinder = new Microarea.EasyBuilder.UI.TreeFinder();
			this.toolStrip.SuspendLayout();
			this.cmsFormEditorContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip
			// 
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbAdd,
            this.tsbRename,
            this.toolStripSeparator1,
            this.tsbDelete,
            this.tsbOpen,
            this.tsbRefresh,
            this.tsbOpenLocation});
			resources.ApplyResources(this.toolStrip, "toolStrip");
			this.toolStrip.Name = "toolStrip";
			// 
			// tsbAdd
			// 
			resources.ApplyResources(this.tsbAdd, "tsbAdd");
			this.tsbAdd.Image = global::Microarea.EasyBuilder.Properties.Resources.AddFile;
			this.tsbAdd.Name = "tsbAdd";
			this.tsbAdd.Click += new System.EventHandler(this.tsbAdd_Click);
			this.tsbAdd.DoubleClick += new System.EventHandler(this.tsbAdd_Click);
			// 
			// tsbRename
			// 
			this.tsbRename.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbRename.Image = global::Microarea.EasyBuilder.Properties.Resources.RenameImage;
			this.tsbRename.Name = "tsbRename";
			resources.ApplyResources(this.tsbRename, "tsbRename");
			this.tsbRename.Click += new System.EventHandler(this.tsbRename_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// tsbDelete
			// 
			this.tsbDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsbDelete, "tsbDelete");
			this.tsbDelete.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
			this.tsbDelete.Name = "tsbDelete";
			this.tsbDelete.Click += new System.EventHandler(this.tsbDelete_Click);
			// 
			// tsbOpen
			// 
			this.tsbOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsbOpen, "tsbOpen");
			this.tsbOpen.Image = global::Microarea.EasyBuilder.Properties.Resources.Open;
			this.tsbOpen.Name = "tsbOpen";
			this.tsbOpen.Click += new System.EventHandler(this.tsbOpen_Click);
			// 
			// tsbRefresh
			// 
			this.tsbRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsbRefresh, "tsbRefresh");
			this.tsbRefresh.Image = global::Microarea.EasyBuilder.Properties.Resources.Refresh;
			this.tsbRefresh.Name = "tsbRefresh";
			this.tsbRefresh.Click += new System.EventHandler(this.tsbRefresh_Click);
			this.tsbRefresh.DoubleClick += new System.EventHandler(this.tsbRefresh_Click);
			// 
			// tsbOpenLocation
			// 
			this.tsbOpenLocation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbOpenLocation.Image = global::Microarea.EasyBuilder.Properties.Resources.Folder;
			this.tsbOpenLocation.Name = "tsbOpenLocation";
			resources.ApplyResources(this.tsbOpenLocation, "tsbOpenLocation");
			this.tsbOpenLocation.Click += new System.EventHandler(this.openLocationToolStripMenuItem_Click);
			this.tsbOpenLocation.DoubleClick += new System.EventHandler(this.openLocationToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.tsbDelete_Click);
			// 
			// addToolStripMenuItem
			// 
			this.addToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.AddFile;
			this.addToolStripMenuItem.Name = "addToolStripMenuItem";
			resources.ApplyResources(this.addToolStripMenuItem, "addToolStripMenuItem");
			this.addToolStripMenuItem.Click += new System.EventHandler(this.tsbAdd_Click);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Open;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
			this.openToolStripMenuItem.Click += new System.EventHandler(this.tsbOpen_Click);
			// 
			// cmsFormEditorContextMenu
			// 
			this.cmsFormEditorContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.addToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.openToolStripMenuItem,
            this.openInNewWindowToolStripMenuItem,
            this.openLocationToolStripMenuItem});
			this.cmsFormEditorContextMenu.Name = "cmsFormEditorContextMenu";
			resources.ApplyResources(this.cmsFormEditorContextMenu, "cmsFormEditorContextMenu");
			// 
			// renameToolStripMenuItem
			// 
			this.renameToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.RenameImage;
			this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
			resources.ApplyResources(this.renameToolStripMenuItem, "renameToolStripMenuItem");
			this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
			this.renameToolStripMenuItem.DoubleClick += new System.EventHandler(this.renameToolStripMenuItem_Click);
			// 
			// openInNewWindowToolStripMenuItem
			// 
			this.openInNewWindowToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Open;
			this.openInNewWindowToolStripMenuItem.Name = "openInNewWindowToolStripMenuItem";
			resources.ApplyResources(this.openInNewWindowToolStripMenuItem, "openInNewWindowToolStripMenuItem");
			this.openInNewWindowToolStripMenuItem.Click += new System.EventHandler(this.tsbOpenInNewWindow_Click);
			// 
			// openLocationToolStripMenuItem
			// 
			this.openLocationToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Folder;
			this.openLocationToolStripMenuItem.Name = "openLocationToolStripMenuItem";
			resources.ApplyResources(this.openLocationToolStripMenuItem, "openLocationToolStripMenuItem");
			this.openLocationToolStripMenuItem.Click += new System.EventHandler(this.openLocationToolStripMenuItem_Click);
			this.openLocationToolStripMenuItem.DoubleClick += new System.EventHandler(this.openLocationToolStripMenuItem_Click);
			// 
			// treeViewManager
			// 
			this.treeViewManager.ContextMenuStrip = this.cmsFormEditorContextMenu;
			resources.ApplyResources(this.treeViewManager, "treeViewManager");
			this.treeViewManager.Name = "treeViewManager";
			this.treeViewManager.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewManager_AfterSelect);
			this.treeViewManager.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewManager_NodeMouseClick);
			this.treeViewManager.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewManager_NodeMouseDoubleClick);
			this.treeViewManager.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeViewManager_KeyDown);
			// 
			// treeFinder
			// 
			resources.ApplyResources(this.treeFinder, "treeFinder");
			this.treeFinder.Name = "treeFinder";
			this.treeFinder.TreeView = this.treeViewManager;
			// 
			// JsonFormsTreeControl
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.treeViewManager);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.treeFinder);
			this.Name = "JsonFormsTreeControl";
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.cmsFormEditorContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }




		#endregion

		private System.Windows.Forms.TreeView treeViewManager;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ContextMenuStrip cmsFormEditorContextMenu;

        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openLocationToolStripMenuItem;

        private System.Windows.Forms.ToolStripButton tsbAdd;
        private System.Windows.Forms.ToolStripButton tsbOpen;
        private System.Windows.Forms.ToolStripButton tsbDelete;
		
        private TreeFinder treeFinder;
        private System.Windows.Forms.ToolStripButton tsbRefresh;
		private System.Windows.Forms.ToolStripButton tsbRename;
		private System.Windows.Forms.ToolStripButton tsbOpenLocation;
		private System.Windows.Forms.ToolStripMenuItem openInNewWindowToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
	}
}