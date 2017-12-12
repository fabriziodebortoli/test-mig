using System;
using Microarea.TaskBuilderNet.Core.Generic;
namespace Microarea.EasyBuilder.UI
{
	partial class ViewOutlineTreeControl
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
				EventHandlers.RemoveEventHandlers(ref PromoteControl);
				EventHandlers.RemoveEventHandlers(ref DeleteObject);
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewOutlineTreeControl));
            this.treeViewOutlineManagers = new System.Windows.Forms.TreeView();
            this.cmsFormEditorContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.highlightContainerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.promoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToJsonMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToTemplateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsComboShowWith = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsFilterLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsButtonProperties = new System.Windows.Forms.ToolStripButton();
            this.tsButtonRefresh = new System.Windows.Forms.ToolStripButton();
            this.tsButtonDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.treeFinder = new Microarea.EasyBuilder.UI.TreeFinder();
            this.cmsFormEditorContextMenu.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeViewOutlineManagers
            // 
            this.treeViewOutlineManagers.AllowDrop = true;
            resources.ApplyResources(this.treeViewOutlineManagers, "treeViewOutlineManagers");
            this.treeViewOutlineManagers.ContextMenuStrip = this.cmsFormEditorContextMenu;
            this.treeViewOutlineManagers.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.treeViewOutlineManagers.HideSelection = false;
            this.treeViewOutlineManagers.ItemHeight = 18;
            this.treeViewOutlineManagers.Name = "treeViewOutlineManagers";
            this.treeViewOutlineManagers.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeViewOutlineManagers_DrawNode);
            this.treeViewOutlineManagers.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewOutlineManagers_ItemDrag);
            this.treeViewOutlineManagers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewOutlineManagers_AfterSelect);
            this.treeViewOutlineManagers.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewOutlineManagers_DragDrop);
            this.treeViewOutlineManagers.DragOver += new System.Windows.Forms.DragEventHandler(this.treeViewOutlineManagers_DragOver);
            this.treeViewOutlineManagers.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeViewOutlineManagers_KeyDown);
            this.treeViewOutlineManagers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeViewOutlineManagers_MouseDown);
            // 
            // cmsFormEditorContextMenu
            // 
            this.cmsFormEditorContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsFormEditorContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.propertiesToolStripMenuItem,
            this.highlightContainerToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.promoteToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.SaveAsToolStripMenuItem});
            this.cmsFormEditorContextMenu.Name = "cmsFormEditorContextMenu";
            resources.ApplyResources(this.cmsFormEditorContextMenu, "cmsFormEditorContextMenu");
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties;
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            resources.ApplyResources(this.propertiesToolStripMenuItem, "propertiesToolStripMenuItem");
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.TsProperties_Click);
            // 
            // highlightContainerToolStripMenuItem
            // 
            this.highlightContainerToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Highlight;
            this.highlightContainerToolStripMenuItem.Name = "highlightContainerToolStripMenuItem";
            resources.ApplyResources(this.highlightContainerToolStripMenuItem, "highlightContainerToolStripMenuItem");
            this.highlightContainerToolStripMenuItem.Click += new System.EventHandler(this.highlightContainerToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.tsDelete_Click);
            // 
            // promoteToolStripMenuItem
            // 
            this.promoteToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties;
            this.promoteToolStripMenuItem.Name = "promoteToolStripMenuItem";
            resources.ApplyResources(this.promoteToolStripMenuItem, "promoteToolStripMenuItem");
            this.promoteToolStripMenuItem.Click += new System.EventHandler(this.TsPromote_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Refresh;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            resources.ApplyResources(this.refreshToolStripMenuItem, "refreshToolStripMenuItem");
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // SaveAsToolStripMenuItem
            // 
            this.SaveAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToJsonMenuItem,
            this.exportToTemplateMenuItem});
            this.SaveAsToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.SaveAs24;
            this.SaveAsToolStripMenuItem.Name = "SaveAsToolStripMenuItem";
            resources.ApplyResources(this.SaveAsToolStripMenuItem, "SaveAsToolStripMenuItem");
            // 
            // exportToJsonMenuItem
            // 
            this.exportToJsonMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.ObjectModel24;
            this.exportToJsonMenuItem.Name = "exportToJsonMenuItem";
            resources.ApplyResources(this.exportToJsonMenuItem, "exportToJsonMenuItem");
            this.exportToJsonMenuItem.Click += new System.EventHandler(this.ExportToJsonMenuItem_Click);
            
            // 
            // exportToTemplateMenuItem
            // 
            this.exportToTemplateMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Enums;
            this.exportToTemplateMenuItem.Name = "exportToTemplateMenuItem";
            resources.ApplyResources(this.exportToTemplateMenuItem, "exportToTemplateMenuItem");
            this.exportToTemplateMenuItem.Click += new System.EventHandler(this.ExportToTemplateMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // tsComboShowWith
            // 
            this.tsComboShowWith.AutoToolTip = true;
            this.tsComboShowWith.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tsComboShowWith.Items.AddRange(new object[] {
            resources.GetString("tsComboShowWith.Items"),
            resources.GetString("tsComboShowWith.Items1"),
            resources.GetString("tsComboShowWith.Items2"),
            resources.GetString("tsComboShowWith.Items3")});
            this.tsComboShowWith.Name = "tsComboShowWith";
            resources.ApplyResources(this.tsComboShowWith, "tsComboShowWith");
            this.tsComboShowWith.SelectedIndexChanged += new System.EventHandler(this.OnEventTsShowWith_SelectedIndexChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(2);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Padding = new System.Windows.Forms.Padding(2);
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // tsFilterLabel
            // 
            resources.ApplyResources(this.tsFilterLabel, "tsFilterLabel");
            this.tsFilterLabel.ForeColor = System.Drawing.SystemColors.Desktop;
            this.tsFilterLabel.Image = global::Microarea.EasyBuilder.Properties.Resources.Filter;
            this.tsFilterLabel.Name = "tsFilterLabel";
            this.tsFilterLabel.Click += new System.EventHandler(this.tsFilterLabel_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(2);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Padding = new System.Windows.Forms.Padding(2);
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // tsButtonProperties
            // 
            this.tsButtonProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsButtonProperties.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties;
            this.tsButtonProperties.Name = "tsButtonProperties";
            resources.ApplyResources(this.tsButtonProperties, "tsButtonProperties");
            this.tsButtonProperties.Click += new System.EventHandler(this.TsProperties_Click);
            // 
            // tsButtonRefresh
            // 
            this.tsButtonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsButtonRefresh.Image = global::Microarea.EasyBuilder.Properties.Resources.Refresh;
            resources.ApplyResources(this.tsButtonRefresh, "tsButtonRefresh");
            this.tsButtonRefresh.Name = "tsButtonRefresh";
            this.tsButtonRefresh.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // tsButtonDelete
            // 
            this.tsButtonDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsButtonDelete.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
            resources.ApplyResources(this.tsButtonDelete, "tsButtonDelete");
            this.tsButtonDelete.Name = "tsButtonDelete";
            this.tsButtonDelete.Click += new System.EventHandler(this.tsDelete_Click);
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsComboShowWith,
            this.toolStripSeparator1,
            this.tsFilterLabel,
            this.toolStripSeparator2,
            this.tsButtonProperties,
            this.tsButtonRefresh,
            this.tsButtonDelete});
            resources.ApplyResources(this.toolStrip, "toolStrip");
            this.toolStrip.Name = "toolStrip";
            // 
            // treeFinder
            // 
            resources.ApplyResources(this.treeFinder, "treeFinder");
            this.treeFinder.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.treeFinder.Name = "treeFinder";
            this.treeFinder.TreeView = this.treeViewOutlineManagers;
            // 
            // ViewOutlineTreeControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeFinder);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.treeViewOutlineManagers);
            this.Name = "ViewOutlineTreeControl";
            this.cmsFormEditorContextMenu.ResumeLayout(false);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        #endregion

        private System.Windows.Forms.TreeView treeViewOutlineManagers;
		private TreeFinder treeFinder;

		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;

		private System.Windows.Forms.ToolStripMenuItem	promoteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem	propertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem	deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem	refreshToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem  highlightContainerToolStripMenuItem;

		private System.Windows.Forms.ContextMenuStrip	cmsFormEditorContextMenu;
		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton tsButtonDelete;
		private System.Windows.Forms.ToolStripButton tsButtonRefresh;
		private System.Windows.Forms.ToolStripButton tsButtonProperties;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripLabel tsFilterLabel;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripComboBox tsComboShowWith;
        private System.Windows.Forms.ToolStripMenuItem SaveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToJsonMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToTemplateMenuItem;
    }
}
