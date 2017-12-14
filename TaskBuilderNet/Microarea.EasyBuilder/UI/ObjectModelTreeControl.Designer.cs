using System;
using Microarea.TaskBuilderNet.Core.Generic;
namespace Microarea.EasyBuilder.UI
{
	partial class ObjectModelTreeControl
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

				EventHandlers.RemoveEventHandlers(ref OpenProperties);
				EventHandlers.RemoveEventHandlers(ref RefreshModel);
				EventHandlers.RemoveEventHandlers(ref AddDbt);
				EventHandlers.RemoveEventHandlers(ref AddField);
				EventHandlers.RemoveEventHandlers(ref DeleteObject);
				EventHandlers.RemoveEventHandlers(ref AddHotLink);
				EventHandlers.RemoveEventHandlers(ref AddDataManager);
				EventHandlers.RemoveEventHandlers(ref DeclareComponent);
				
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectModelTreeControl));
            this.treeDataManagers = new System.Windows.Forms.TreeView();
            this.cmsFormEditorContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.addDBTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addLocalFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addHotLinkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wrapExistingHotlinkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.tsbProperties = new System.Windows.Forms.ToolStripButton();
            this.tsbRefresh = new System.Windows.Forms.ToolStripButton();
            this.tsbAddNewDbt = new System.Windows.Forms.ToolStripButton();
            this.tsbAddField = new System.Windows.Forms.ToolStripButton();
            this.tsbAddLocalField = new System.Windows.Forms.ToolStripButton();
            this.tsbAddNewHotlink = new System.Windows.Forms.ToolStripButton();
            this.tsbAddHotlinkFromTemplate = new System.Windows.Forms.ToolStripButton();
            this.tsbDelete = new System.Windows.Forms.ToolStripButton();
            this.treeFinder = new Microarea.EasyBuilder.UI.TreeFinder();
            this.cmsFormEditorContextMenu.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeDataManagers
            // 
            this.treeDataManagers.AllowDrop = true;
            resources.ApplyResources(this.treeDataManagers, "treeDataManagers");
            this.treeDataManagers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeDataManagers.ContextMenuStrip = this.cmsFormEditorContextMenu;
            this.treeDataManagers.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.treeDataManagers.HideSelection = false;
            this.treeDataManagers.ItemHeight = 20;
            this.treeDataManagers.Name = "treeDataManagers";
            this.treeDataManagers.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeDataManagers_DrawNode);
            this.treeDataManagers.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeDataManagers_ItemDrag);
            this.treeDataManagers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeDataManagers_AfterSelect);
            this.treeDataManagers.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeDataManagers_DragDrop);
            this.treeDataManagers.DragOver += new System.Windows.Forms.DragEventHandler(this.treeDataManagers_DragOver);
            this.treeDataManagers.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeDataManagers_KeyDown);
            this.treeDataManagers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeDataManagers_MouseDown);
            // 
            // cmsFormEditorContextMenu
            // 
            this.cmsFormEditorContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsProperties,
            this.tsDelete,
            this.toolStripSeparator1,
            this.addDBTToolStripMenuItem,
            this.addFieldToolStripMenuItem,
            this.addLocalFieldToolStripMenuItem,
            this.addHotLinkToolStripMenuItem,
            this.wrapExistingHotlinkToolStripMenuItem,
            this.toolStripSeparator2,
            this.refreshToolStripMenuItem});
            this.cmsFormEditorContextMenu.Name = "cmsFormEditorContextMenu";
            resources.ApplyResources(this.cmsFormEditorContextMenu, "cmsFormEditorContextMenu");
            // 
            // tsProperties
            // 
            this.tsProperties.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties;
            this.tsProperties.Name = "tsProperties";
            resources.ApplyResources(this.tsProperties, "tsProperties");
            this.tsProperties.Click += new System.EventHandler(this.TsProperties_Click);
            // 
            // tsDelete
            // 
            this.tsDelete.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
            this.tsDelete.Name = "tsDelete";
            resources.ApplyResources(this.tsDelete, "tsDelete");
            this.tsDelete.Click += new System.EventHandler(this.tsDelete_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // addDBTToolStripMenuItem
            // 
            this.addDBTToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Master;
            this.addDBTToolStripMenuItem.Name = "addDBTToolStripMenuItem";
            resources.ApplyResources(this.addDBTToolStripMenuItem, "addDBTToolStripMenuItem");
            this.addDBTToolStripMenuItem.Click += new System.EventHandler(this.addDBTToolStripMenuItem_Click);
            // 
            // addFieldToolStripMenuItem
            // 
            this.addFieldToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.DatabaseItem;
            this.addFieldToolStripMenuItem.Name = "addFieldToolStripMenuItem";
            resources.ApplyResources(this.addFieldToolStripMenuItem, "addFieldToolStripMenuItem");
            this.addFieldToolStripMenuItem.Click += new System.EventHandler(this.addFieldToolStripMenuItem_Click);
            // 
            // addLocalFieldToolStripMenuItem
            // 
            this.addLocalFieldToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Field;
            this.addLocalFieldToolStripMenuItem.Name = "addLocalFieldToolStripMenuItem";
            resources.ApplyResources(this.addLocalFieldToolStripMenuItem, "addLocalFieldToolStripMenuItem");
            this.addLocalFieldToolStripMenuItem.Click += new System.EventHandler(this.addLocalFieldToolStripMenuItem_Click);
            // 
            // addHotLinkToolStripMenuItem
            // 
            this.addHotLinkToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.HotLinkFromTemplate;
            this.addHotLinkToolStripMenuItem.Name = "addHotLinkToolStripMenuItem";
            resources.ApplyResources(this.addHotLinkToolStripMenuItem, "addHotLinkToolStripMenuItem");
            this.addHotLinkToolStripMenuItem.Click += new System.EventHandler(this.addHotLinkToolStripMenuItem_Click);
            // 
            // wrapExistingHotlinkToolStripMenuItem
            // 
            this.wrapExistingHotlinkToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.HotLink;
            this.wrapExistingHotlinkToolStripMenuItem.Name = "wrapExistingHotlinkToolStripMenuItem";
            resources.ApplyResources(this.wrapExistingHotlinkToolStripMenuItem, "wrapExistingHotlinkToolStripMenuItem");
            this.wrapExistingHotlinkToolStripMenuItem.Click += new System.EventHandler(this.wrapExistingHotlinkToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Refresh;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            resources.ApplyResources(this.refreshToolStripMenuItem, "refreshToolStripMenuItem");
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbProperties,
            this.tsbRefresh,
            this.tsbAddNewDbt,
            this.tsbAddField,
            this.tsbAddLocalField,
            this.tsbAddNewHotlink,
            this.tsbAddHotlinkFromTemplate,
            this.tsbDelete});
            resources.ApplyResources(this.toolStrip, "toolStrip");
            this.toolStrip.Name = "toolStrip";
            // 
            // tsbProperties
            // 
            this.tsbProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbProperties.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties;
            resources.ApplyResources(this.tsbProperties, "tsbProperties");
            this.tsbProperties.Name = "tsbProperties";
            this.tsbProperties.Click += new System.EventHandler(this.TsProperties_Click);
            // 
            // tsbRefresh
            // 
            this.tsbRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRefresh.Image = global::Microarea.EasyBuilder.Properties.Resources.Refresh;
            resources.ApplyResources(this.tsbRefresh, "tsbRefresh");
            this.tsbRefresh.Name = "tsbRefresh";
            this.tsbRefresh.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // tsbAddNewDbt
            // 
            this.tsbAddNewDbt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAddNewDbt.Image = global::Microarea.EasyBuilder.Properties.Resources.Master;
            resources.ApplyResources(this.tsbAddNewDbt, "tsbAddNewDbt");
            this.tsbAddNewDbt.Name = "tsbAddNewDbt";
            this.tsbAddNewDbt.Click += new System.EventHandler(this.addDBTToolStripMenuItem_Click);
            // 
            // tsbAddField
            // 
            this.tsbAddField.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAddField.Image = global::Microarea.EasyBuilder.Properties.Resources.DatabaseItem;
            resources.ApplyResources(this.tsbAddField, "tsbAddField");
            this.tsbAddField.Name = "tsbAddField";
            this.tsbAddField.Click += new System.EventHandler(this.addFieldToolStripMenuItem_Click);
            // 
            // tsbAddLocalField
            // 
            this.tsbAddLocalField.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAddLocalField.Image = global::Microarea.EasyBuilder.Properties.Resources.Field;
            resources.ApplyResources(this.tsbAddLocalField, "tsbAddLocalField");
            this.tsbAddLocalField.Name = "tsbAddLocalField";
            this.tsbAddLocalField.Click += new System.EventHandler(this.addLocalFieldToolStripMenuItem_Click);
            // 
            // tsbAddNewHotlink
            // 
            this.tsbAddNewHotlink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAddNewHotlink.Image = global::Microarea.EasyBuilder.Properties.Resources.HotLink;
            resources.ApplyResources(this.tsbAddNewHotlink, "tsbAddNewHotlink");
            this.tsbAddNewHotlink.Name = "tsbAddNewHotlink";
            this.tsbAddNewHotlink.Click += new System.EventHandler(this.addHotLinkToolStripMenuItem_Click);
            // 
            // tsbAddHotlinkFromTemplate
            // 
            this.tsbAddHotlinkFromTemplate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbAddHotlinkFromTemplate.Image = global::Microarea.EasyBuilder.Properties.Resources.HotLinkFromTemplate;
            resources.ApplyResources(this.tsbAddHotlinkFromTemplate, "tsbAddHotlinkFromTemplate");
            this.tsbAddHotlinkFromTemplate.Name = "tsbAddHotlinkFromTemplate";
            this.tsbAddHotlinkFromTemplate.Click += new System.EventHandler(this.wrapExistingHotlinkToolStripMenuItem_Click);
            // 
            // tsbDelete
            // 
            this.tsbDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbDelete.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
            resources.ApplyResources(this.tsbDelete, "tsbDelete");
            this.tsbDelete.Name = "tsbDelete";
            this.tsbDelete.Click += new System.EventHandler(this.tsDelete_Click);
            // 
            // treeFinder
            // 
            resources.ApplyResources(this.treeFinder, "treeFinder");
            this.treeFinder.Name = "treeFinder";
            this.treeFinder.TreeView = this.treeDataManagers;
            // 
            // ObjectModelTreeControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeFinder);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.treeDataManagers);
            this.Name = "ObjectModelTreeControl";
            this.cmsFormEditorContextMenu.ResumeLayout(false);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TreeView treeDataManagers;
		private System.Windows.Forms.ContextMenuStrip cmsFormEditorContextMenu;
		private System.Windows.Forms.ToolStripMenuItem tsProperties;
		private System.Windows.Forms.ToolStripMenuItem addDBTToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem tsDelete;
		private System.Windows.Forms.ToolStripMenuItem addFieldToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addHotLinkToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem wrapExistingHotlinkToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addLocalFieldToolStripMenuItem;
		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton tsbProperties;
		private System.Windows.Forms.ToolStripButton tsbRefresh;
		private System.Windows.Forms.ToolStripButton tsbAddNewDbt;
		private System.Windows.Forms.ToolStripButton tsbAddField;
		private System.Windows.Forms.ToolStripButton tsbAddLocalField;
		private System.Windows.Forms.ToolStripButton tsbAddNewHotlink;
		private System.Windows.Forms.ToolStripButton tsbAddHotlinkFromTemplate;
		private System.Windows.Forms.ToolStripButton tsbDelete;
		private TreeFinder treeFinder;
	}
}
