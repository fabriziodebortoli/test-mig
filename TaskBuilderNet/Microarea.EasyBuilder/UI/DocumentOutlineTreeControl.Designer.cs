using System;

namespace Microarea.EasyBuilder.UI
{
	partial class DocumentOutlineTreeControl
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
				if (selectionService != null)
					selectionService.SelectionChanged -= new EventHandler(SelectionService_SelectionChanged);
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocumentOutlineTreeControl));
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonOpenLocation = new System.Windows.Forms.ToolStripButton();
			this.buttonRefresh = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonTM = new System.Windows.Forms.ToolStripButton();
			this.buttonTG = new System.Windows.Forms.ToolStripButton();
			this.buttonTD = new System.Windows.Forms.ToolStripButton();
			this.buttonAddFromFileSystem = new System.Windows.Forms.ToolStripButton();
			this.treeDocOutlineManager = new System.Windows.Forms.TreeView();
			this.cmDocOutlineContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsmiOpenProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
			this.newTileTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.deafultTileGroupTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.deafultTileManagerTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolBarTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.newHeaderStripTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolBarButtonTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.newSeparatorTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.tileFromFileSystemTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.newHeaderStripButtonTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.newViewTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.inHereTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.inNewWindowTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.fileSystemLocationTsmi = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip.SuspendLayout();
			this.cmDocOutlineContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip
			// 
			resources.ApplyResources(this.toolStrip, "toolStrip");
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator3,
            this.buttonOpenLocation,
            this.buttonRefresh,
            this.toolStripSeparator1,
            this.buttonTM,
            this.buttonTG,
            this.buttonTD,
            this.buttonAddFromFileSystem});
			this.toolStrip.Name = "toolStrip";
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(2);
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Padding = new System.Windows.Forms.Padding(2);
			resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
			// 
			// buttonOpenLocation
			// 
			this.buttonOpenLocation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonOpenLocation.Image = global::Microarea.EasyBuilder.Properties.Resources.Folder;
			this.buttonOpenLocation.Name = "buttonOpenLocation";
			this.buttonOpenLocation.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
			resources.ApplyResources(this.buttonOpenLocation, "buttonOpenLocation");
			this.buttonOpenLocation.Click += new System.EventHandler(this.tsmiOpenLocation_Click);
			this.buttonOpenLocation.DoubleClick += new System.EventHandler(this.tsmiOpenLocation_Click);
			// 
			// buttonRefresh
			// 
			this.buttonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRefresh.Image = global::Microarea.EasyBuilder.Properties.Resources.Refresh;
			resources.ApplyResources(this.buttonRefresh, "buttonRefresh");
			this.buttonRefresh.Name = "buttonRefresh";
			this.buttonRefresh.Click += new System.EventHandler(this.tsRefresh_Click);
			this.buttonRefresh.DoubleClick += new System.EventHandler(this.tsRefresh_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(2);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Padding = new System.Windows.Forms.Padding(2);
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// buttonTM
			// 
			this.buttonTM.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonTM.Image = global::Microarea.EasyBuilder.Properties.Resources.TileManager;
			resources.ApplyResources(this.buttonTM, "buttonTM");
			this.buttonTM.Name = "buttonTM";
			this.buttonTM.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
			this.buttonTM.Tag = TileManagerProperties.GetDocOutObj();
			this.buttonTM.Click += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			this.buttonTM.DoubleClick += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			// 
			// buttonTG
			// 
			this.buttonTG.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonTG.Image = global::Microarea.EasyBuilder.Properties.Resources.TileGroup;
			resources.ApplyResources(this.buttonTG, "buttonTG");
			this.buttonTG.Name = "buttonTG";
			this.buttonTG.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
			this.buttonTG.Tag = TileGroupProperties.GetDocOutObj();
			this.buttonTG.Click += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			this.buttonTG.DoubleClick += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			// 
			// buttonTD
			// 
			resources.ApplyResources(this.buttonTD, "buttonTD");
			this.buttonTD.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonTD.Image = global::Microarea.EasyBuilder.Properties.Resources.TileDialog;
			this.buttonTD.Margin = new System.Windows.Forms.Padding(6);
			this.buttonTD.Name = "buttonTD";
			this.buttonTD.Padding = new System.Windows.Forms.Padding(8, 16, 8, 16);
			this.buttonTD.Click += new System.EventHandler(this.tsTileDialog_Click);
			this.buttonTD.DoubleClick += new System.EventHandler(this.tsTileDialog_Click);
			// 
			// buttonAddFromFileSystem
			// 
			this.buttonAddFromFileSystem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonAddFromFileSystem.Image = global::Microarea.EasyBuilder.Properties.Resources.AddModule;
			resources.ApplyResources(this.buttonAddFromFileSystem, "buttonAddFromFileSystem");
			this.buttonAddFromFileSystem.Name = "buttonAddFromFileSystem";
			this.buttonAddFromFileSystem.Click += new System.EventHandler(this.tsbAddFromFileSystem_Click);
			// 
			// treeDocOutlineManager
			// 
			this.treeDocOutlineManager.AllowDrop = true;
			resources.ApplyResources(this.treeDocOutlineManager, "treeDocOutlineManager");
			this.treeDocOutlineManager.ContextMenuStrip = this.cmDocOutlineContextMenu;
			this.treeDocOutlineManager.ItemHeight = 22;
			this.treeDocOutlineManager.Name = "treeDocOutlineManager";
			this.treeDocOutlineManager.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeDocOutlineManager_DrawNode);
			this.treeDocOutlineManager.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeDocOutlineManager_ItemDrag);
			this.treeDocOutlineManager.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeDocOutlineManager_AfterSelect);
			this.treeDocOutlineManager.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeDocOutlineManager_NodeMouseDoubleClick);
			this.treeDocOutlineManager.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeDocOutlineManager_DragDrop);
			this.treeDocOutlineManager.DragOver += new System.Windows.Forms.DragEventHandler(this.treeDocOutlineManager_DragOver);
			this.treeDocOutlineManager.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeDocOutlineManager_MouseDown);
			// 
			// cmDocOutlineContextMenu
			// 
			this.cmDocOutlineContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiOpenProperties,
            this.tsmiAdd,
            this.tsmiOpen,
            this.tsmiDelete});
			this.cmDocOutlineContextMenu.Name = "cmsFormEditorContextMenu";
			resources.ApplyResources(this.cmDocOutlineContextMenu, "cmDocOutlineContextMenu");
			// 
			// tsmiOpenProperties
			// 
			this.tsmiOpenProperties.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties;
			this.tsmiOpenProperties.Name = "tsmiOpenProperties";
			resources.ApplyResources(this.tsmiOpenProperties, "tsmiOpenProperties");
			this.tsmiOpenProperties.Click += new System.EventHandler(this.tsmiOpenProperties_Click);
			// 
			// tsmiAdd
			// 
			this.tsmiAdd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newTileTsmi,
            this.deafultTileGroupTsmi,
            this.deafultTileManagerTsmi,
            this.newToolBarTsmi,
            this.newHeaderStripTsmi,
            this.newToolBarButtonTsmi,
            this.newSeparatorTsmi,
            this.tileFromFileSystemTsmi,
            this.newHeaderStripButtonTsmi,
            this.newViewTsmi});
			resources.ApplyResources(this.tsmiAdd, "tsmiAdd");
			this.tsmiAdd.Image = global::Microarea.EasyBuilder.Properties.Resources.AddFile;
			this.tsmiAdd.Name = "tsmiAdd";
			// 
			// newTileTsmi
			// 
			this.newTileTsmi.Name = "newTileTsmi";
			resources.ApplyResources(this.newTileTsmi, "newTileTsmi");
			this.newTileTsmi.Click += new System.EventHandler(this.tsTileDialog_Click);
			this.newTileTsmi.DoubleClick += new System.EventHandler(this.tsTileDialog_Click);
			// 
			// deafultTileGroupTsmi
			// 
			this.deafultTileGroupTsmi.Name = "deafultTileGroupTsmi";
			resources.ApplyResources(this.deafultTileGroupTsmi, "deafultTileGroupTsmi");
			this.deafultTileGroupTsmi.Tag = TileGroupProperties.GetDocOutObj();
			this.deafultTileGroupTsmi.Click += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			this.deafultTileGroupTsmi.DoubleClick += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			// 
			// deafultTileManagerTsmi
			// 
			this.deafultTileManagerTsmi.Name = "deafultTileManagerTsmi";
			resources.ApplyResources(this.deafultTileManagerTsmi, "deafultTileManagerTsmi");
			this.deafultTileManagerTsmi.Tag = TileManagerProperties.GetDocOutObj();
			this.deafultTileManagerTsmi.Click += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			this.deafultTileManagerTsmi.DoubleClick += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			// 
			// newToolBarTsmi
			// 
			this.newToolBarTsmi.Name = "newToolBarTsmi";
			resources.ApplyResources(this.newToolBarTsmi, "newToolBarTsmi");
			this.newToolBarTsmi.Tag = ToolbarProperties.GetDocOutObj();
			this.newToolBarTsmi.Click += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			this.newToolBarTsmi.DoubleClick += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			// 
			// newHeaderStripTsmi
			// 
			this.newHeaderStripTsmi.Name = "newHeaderStripTsmi";
			resources.ApplyResources(this.newHeaderStripTsmi, "newHeaderStripTsmi");
			this.newHeaderStripTsmi.Tag = HeaderStripProperties.GetDocOutObj();
			this.newHeaderStripTsmi.Click += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			this.newHeaderStripTsmi.DoubleClick += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			// 
			// newToolBarButtonTsmi
			// 
			this.newToolBarButtonTsmi.Name = "newToolBarButtonTsmi";
			resources.ApplyResources(this.newToolBarButtonTsmi, "newToolBarButtonTsmi");
			this.newToolBarButtonTsmi.Tag = ToolbarButtonProperties.GetDocOutObj();
			this.newToolBarButtonTsmi.Click += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			this.newToolBarButtonTsmi.DoubleClick += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			// 
			// newSeparatorTsmi
			// 
			this.newSeparatorTsmi.Name = "newSeparatorTsmi";
			resources.ApplyResources(this.newSeparatorTsmi, "newSeparatorTsmi");
			this.newSeparatorTsmi.Tag = ToolbarSeparatorProperties.GetDocOutObj();
			this.newSeparatorTsmi.Click += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			this.newSeparatorTsmi.DoubleClick += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			// 
			// tileFromFileSystemTsmi
			// 
			this.tileFromFileSystemTsmi.Name = "tileFromFileSystemTsmi";
			resources.ApplyResources(this.tileFromFileSystemTsmi, "tileFromFileSystemTsmi");
			this.tileFromFileSystemTsmi.Click += new System.EventHandler(this.tsbAddFromFileSystem_Click);
			this.tileFromFileSystemTsmi.DoubleClick += new System.EventHandler(this.tsbAddFromFileSystem_Click);
			// 
			// newHeaderStripButtonTsmi
			// 
			this.newHeaderStripButtonTsmi.Name = "newHeaderStripButtonTsmi";
			resources.ApplyResources(this.newHeaderStripButtonTsmi, "newHeaderStripButtonTsmi");
			this.newHeaderStripButtonTsmi.Click += new System.EventHandler(this.tsHeaderStripButton_Click);
			this.newHeaderStripButtonTsmi.DoubleClick += new System.EventHandler(this.tsHeaderStripButton_Click);
			// 
			// newViewTsmi
			// 
			this.newViewTsmi.Name = "newViewTsmi";
			resources.ApplyResources(this.newViewTsmi, "newViewTsmi");
			this.newViewTsmi.Tag = ViewProperties.GetDocOutObj();
			this.newViewTsmi.Click += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			this.newViewTsmi.DoubleClick += new System.EventHandler(this.ToolStripClickToAddItem_Click);
			// 
			// tsmiOpen
			// 
			this.tsmiOpen.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.inHereTsmi,
            this.inNewWindowTsmi,
            this.fileSystemLocationTsmi});
			this.tsmiOpen.Image = global::Microarea.EasyBuilder.Properties.Resources.Open;
			this.tsmiOpen.Name = "tsmiOpen";
			resources.ApplyResources(this.tsmiOpen, "tsmiOpen");
			// 
			// inHereTsmi
			// 
			this.inHereTsmi.Name = "inHereTsmi";
			resources.ApplyResources(this.inHereTsmi, "inHereTsmi");
			this.inHereTsmi.Click += new System.EventHandler(this.tsmiOpen_Click);
			this.inHereTsmi.DoubleClick += new System.EventHandler(this.tsmiOpen_Click);
			// 
			// inNewWindowTsmi
			// 
			this.inNewWindowTsmi.Name = "inNewWindowTsmi";
			resources.ApplyResources(this.inNewWindowTsmi, "inNewWindowTsmi");
			this.inNewWindowTsmi.Click += new System.EventHandler(this.tsmiOpenInNewWindow_Click);
			this.inNewWindowTsmi.DoubleClick += new System.EventHandler(this.tsmiOpenInNewWindow_Click);
			// 
			// fileSystemLocationTsmi
			// 
			this.fileSystemLocationTsmi.Name = "fileSystemLocationTsmi";
			resources.ApplyResources(this.fileSystemLocationTsmi, "fileSystemLocationTsmi");
			this.fileSystemLocationTsmi.Click += new System.EventHandler(this.tsmiOpenLocation_Click);
			this.fileSystemLocationTsmi.DoubleClick += new System.EventHandler(this.tsmiOpenLocation_Click);
			// 
			// tsmiDelete
			// 
			this.tsmiDelete.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
			this.tsmiDelete.Name = "tsmiDelete";
			resources.ApplyResources(this.tsmiDelete, "tsmiDelete");
			this.tsmiDelete.Click += new System.EventHandler(this.tsmiDelete_Click);
			// 
			// DocumentOutlineTreeControl
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.treeDocOutlineManager);
			this.Controls.Add(this.toolStrip);
			this.Name = "DocumentOutlineTreeControl";
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.cmDocOutlineContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}




		#endregion

		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton buttonTM;
		private System.Windows.Forms.ToolStripButton buttonTG;
		private System.Windows.Forms.ToolStripButton buttonTD;
		private System.Windows.Forms.ToolStripButton buttonOpenLocation;
		private System.Windows.Forms.ToolStripButton buttonRefresh;
		private System.Windows.Forms.TreeView treeDocOutlineManager;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ContextMenuStrip cmDocOutlineContextMenu;
		private System.Windows.Forms.ToolStripMenuItem tsmiDelete;
		private System.Windows.Forms.ToolStripMenuItem tsmiAdd;
		private System.Windows.Forms.ToolStripMenuItem tsmiOpen;
		private System.Windows.Forms.ToolStripButton buttonAddFromFileSystem;
		private System.Windows.Forms.ToolStripMenuItem inHereTsmi;
		private System.Windows.Forms.ToolStripMenuItem inNewWindowTsmi;
		private System.Windows.Forms.ToolStripMenuItem fileSystemLocationTsmi;
		private System.Windows.Forms.ToolStripMenuItem tsmiOpenProperties;
		private System.Windows.Forms.ToolStripMenuItem newTileTsmi;
		private System.Windows.Forms.ToolStripMenuItem deafultTileGroupTsmi;
		private System.Windows.Forms.ToolStripMenuItem deafultTileManagerTsmi;
		private System.Windows.Forms.ToolStripMenuItem newToolBarTsmi;
		private System.Windows.Forms.ToolStripMenuItem newHeaderStripTsmi;
		private System.Windows.Forms.ToolStripMenuItem newToolBarButtonTsmi;
		private System.Windows.Forms.ToolStripMenuItem tileFromFileSystemTsmi;
		private System.Windows.Forms.ToolStripMenuItem newHeaderStripButtonTsmi;
		private System.Windows.Forms.ToolStripMenuItem newViewTsmi;
		private System.Windows.Forms.ToolStripMenuItem newSeparatorTsmi;
	}
}
