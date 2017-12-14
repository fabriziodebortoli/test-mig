using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.Generic;
using System.Threading.Tasks;

namespace Microarea.EasyBuilder.UI
{
	partial class DatabaseExplorer
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseExplorer));
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.tvTables = new Microarea.TaskBuilderNet.UI.WinControls.Lists.HighlightedTreeView();
			this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.addTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addMasterTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.addFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.importTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importMasterTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.tbAdd = new System.Windows.Forms.ToolStripButton();
			this.tbDelete = new System.Windows.Forms.ToolStripButton();
			this.tbProperties = new System.Windows.Forms.ToolStripButton();
			this.tbFilter = new System.Windows.Forms.ToolStripButton();
			this.tsbSaveToObjectModel = new System.Windows.Forms.ToolStripButton();
			this.tbSaveToDatabase = new System.Windows.Forms.ToolStripButton();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tslblMatched = new System.Windows.Forms.ToolStripLabel();
			this.treeFinder = new Microarea.EasyBuilder.UI.TreeFinder();
			this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.contextMenu.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.BottomToolStripPanel
			// 
			this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.treeFinder);
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.tvTables);
			resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
			resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
			this.toolStripContainer1.Name = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip);
			// 
			// tvTables
			// 
			this.tvTables.ContextMenuStrip = this.contextMenu;
			resources.ApplyResources(this.tvTables, "tvTables");
			this.tvTables.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.tvTables.HideSelection = false;
			this.tvTables.Name = "tvTables";
			this.tvTables.ShowNodeToolTips = true;
			this.tvTables.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvTables_AfterSelect);
			this.tvTables.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tvTables_MouseDoubleClick);
			this.tvTables.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvTables_MouseDown);
			this.tvTables.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tvTables_MouseMove);
			// 
			// contextMenu
			// 
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addMasterTableToolStripMenuItem,
            this.addTableToolStripMenuItem,
			this.addFieldToolStripMenuItem,
			this.propertiesToolStripMenuItem,
			this.deleteToolStripMenuItem,
			this.importTableToolStripMenuItem,
            this.importMasterTableToolStripMenuItem});
			this.contextMenu.Name = "tableContextMenu";
			resources.ApplyResources(this.contextMenu, "contextMenu");
			// 
			// addMasterTableToolStripMenuItem
			// 
			this.addMasterTableToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Table;
			this.addMasterTableToolStripMenuItem.Name = "addMasterTableToolStripMenuItem";
			resources.ApplyResources(this.addMasterTableToolStripMenuItem, "addMasterTableToolStripMenuItem");
			this.addMasterTableToolStripMenuItem.Click += new System.EventHandler(this.addMasterTableToolStripMenuItem_Click);
            // 
            // addTableToolStripMenuItem
            // 
            this.addTableToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Table;
            this.addTableToolStripMenuItem.Name = "addTableToolStripMenuItem";
            resources.ApplyResources(this.addTableToolStripMenuItem, "addTableToolStripMenuItem");
            this.addTableToolStripMenuItem.Click += new System.EventHandler(this.addTableToolStripMenuItem_Click);
            // 
            // addFieldToolStripMenuItem
            // 
            this.addFieldToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Field;
			this.addFieldToolStripMenuItem.Name = "addFieldToolStripMenuItem";
			resources.ApplyResources(this.addFieldToolStripMenuItem, "addFieldToolStripMenuItem");
			this.addFieldToolStripMenuItem.Click += new System.EventHandler(this.addFieldToolStripMenuItem_Click);
			// 
			// propertiesToolStripMenuItem
			// 
			this.propertiesToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties;
			this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
			resources.ApplyResources(this.propertiesToolStripMenuItem, "propertiesToolStripMenuItem");
			this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// importTableToolStripMenuItem
			// 
			this.importTableToolStripMenuItem.Name = "importTableToolStripMenuItem";
			resources.ApplyResources(this.importTableToolStripMenuItem, "importTableToolStripMenuItem");
			this.importTableToolStripMenuItem.Click += new System.EventHandler(this.importTableToolStripMenuItem_Click);

            // 
            // importMasterTableToolStripMenuItem
            // 
            this.importMasterTableToolStripMenuItem.Name = "importMasterTableToolStripMenuItem";
            resources.ApplyResources(this.importMasterTableToolStripMenuItem, "importMasterTableToolStripMenuItem");
            this.importMasterTableToolStripMenuItem.Click += new System.EventHandler(this.importMasterTableToolStripMenuItem_Click);

            // 
            // toolStrip
            // 
            resources.ApplyResources(this.toolStrip, "toolStrip");
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.tbAdd,
			this.tbDelete,
			this.tbProperties,
			this.tbFilter,
			this.tsbSaveToObjectModel,
			this.tbSaveToDatabase});
			this.toolStrip.Name = "toolStrip";
			// 
			// tbAdd
			// 
			this.tbAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tbAdd.Image = global::Microarea.EasyBuilder.Properties.Resources.New;
			resources.ApplyResources(this.tbAdd, "tbAdd");
			this.tbAdd.Name = "tbAdd";
			this.tbAdd.Click += new System.EventHandler(this.tbAdd_Click);
			// 
			// tbDelete
			// 
			this.tbDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tbDelete.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
			resources.ApplyResources(this.tbDelete, "tbDelete");
			this.tbDelete.Name = "tbDelete";
			this.tbDelete.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// tbProperties
			// 
			this.tbProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tbProperties.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties;
			resources.ApplyResources(this.tbProperties, "tbProperties");
			this.tbProperties.Name = "tbProperties";
			this.tbProperties.Click += new System.EventHandler(this.tbProperties_Click);
			// 
			// tbFilter
			// 
			this.tbFilter.CheckOnClick = true;
			this.tbFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tbFilter.Image = global::Microarea.EasyBuilder.Properties.Resources.Filter;
			resources.ApplyResources(this.tbFilter, "tbFilter");
			this.tbFilter.Name = "tbFilter";
			this.tbFilter.CheckedChanged += new System.EventHandler(this.cbMyItems_CheckedChanged);
			// 
			// tsbSaveToObjectModel
			// 
			this.tsbSaveToObjectModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbSaveToObjectModel.Image = global::Microarea.EasyBuilder.Properties.Resources.SaveToModel;
			resources.ApplyResources(this.tsbSaveToObjectModel, "tsbSaveToObjectModel");
			this.tsbSaveToObjectModel.Name = "tsbSaveToObjectModel";
			this.tsbSaveToObjectModel.Click += new System.EventHandler(this.tsbSaveToObjectModel_Click);
			// 
			// tbSaveToDatabase
			// 
			this.tbSaveToDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tbSaveToDatabase.Image = global::Microarea.EasyBuilder.Properties.Resources.SaveToDatabase;
			resources.ApplyResources(this.tbSaveToDatabase, "tbSaveToDatabase");
			this.tbSaveToDatabase.Name = "tbSaveToDatabase";
			this.tbSaveToDatabase.Click += new System.EventHandler(this.tbSaveToDatabase_Click);
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// tslblMatched
			// 
			this.tslblMatched.Name = "tslblMatched";
			resources.ApplyResources(this.tslblMatched, "tslblMatched");
			// 
			// treeFinder
			// 
			this.treeFinder.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.treeFinder, "treeFinder");
			this.treeFinder.Name = "treeFinder";
			this.treeFinder.TreeView = this.tvTables;
			// 
			// DatabaseExplorer
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.toolStripContainer1);
			this.Name = "DatabaseExplorer";
			this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.contextMenu.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Microarea.TaskBuilderNet.UI.WinControls.Lists.HighlightedTreeView tvTables;
		private System.Windows.Forms.ContextMenuStrip contextMenu;
		private System.Windows.Forms.ToolStripMenuItem addFieldToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton tsbSaveToObjectModel;
		private System.Windows.Forms.ToolStripButton tbSaveToDatabase;
		private System.Windows.Forms.ToolStripButton tbFilter;
		private System.Windows.Forms.ToolStripButton tbAdd;
		private System.Windows.Forms.ToolStripButton tbDelete;
		private System.Windows.Forms.ToolStripButton tbProperties;
        private System.Windows.Forms.ToolStripMenuItem addMasterTableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTableToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem importTableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importMasterTableToolStripMenuItem;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private TreeFinder treeFinder;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripLabel tslblMatched;
	}
}
