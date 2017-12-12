using Microarea.TaskBuilderNet.Core.Generic;
namespace Microarea.EasyBuilder.UI
{
	partial class EnumsTreeControl
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
					components.Dispose();

				EventHandlers.RemoveEventHandlers(ref OpenProperties);
				EventHandlers.RemoveEventHandlers(ref DirtyChanged);
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnumsTreeControl));
			this.EnumsTreeView = new System.Windows.Forms.TreeView();
			this.CmsEnumsTreeView = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsAddEnumTag = new System.Windows.Forms.ToolStripMenuItem();
			this.tsAddEnumItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tsDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.tsProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.CmsEnumsTreeView.SuspendLayout();
			this.SuspendLayout();
			// 
			// EnumsTreeView
			// 
			this.EnumsTreeView.ContextMenuStrip = this.CmsEnumsTreeView;
			resources.ApplyResources(this.EnumsTreeView, "EnumsTreeView");
			this.EnumsTreeView.Name = "EnumsTreeView";
			this.EnumsTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.EnumsTreeView_MouseDown);
			this.EnumsTreeView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.EnumsTreeView_MouseMove);
			this.EnumsTreeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.EnumsTreeView_MouseUp);
			// 
			// CmsEnumsTreeView
			// 
			this.CmsEnumsTreeView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsAddEnumTag,
            this.tsAddEnumItem,
            this.tsDelete,
            this.tsProperties});
			this.CmsEnumsTreeView.Name = "CmsEnumsTreeView";
			resources.ApplyResources(this.CmsEnumsTreeView, "CmsEnumsTreeView");
			// 
			// tsAddEnumTag
			// 
			this.tsAddEnumTag.Image = global::Microarea.EasyBuilder.Properties.Resources.EnumTag;
			this.tsAddEnumTag.Name = "tsAddEnumTag";
			resources.ApplyResources(this.tsAddEnumTag, "tsAddEnumTag");
			this.tsAddEnumTag.Click += new System.EventHandler(this.TsAddEnumTag_Click);
			// 
			// tsAddEnumItem
			// 
			this.tsAddEnumItem.Image = global::Microarea.EasyBuilder.Properties.Resources.EnumItem;
			this.tsAddEnumItem.Name = "tsAddEnumItem";
			resources.ApplyResources(this.tsAddEnumItem, "tsAddEnumItem");
			this.tsAddEnumItem.Click += new System.EventHandler(this.TsAddEnumItem_Click);
			// 
			// tsDelete
			// 
			this.tsDelete.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
			this.tsDelete.Name = "tsDelete";
			resources.ApplyResources(this.tsDelete, "tsDelete");
			this.tsDelete.Click += new System.EventHandler(this.TsDelete_Click);
			// 
			// tsProperties
			// 
			this.tsProperties.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties;
			this.tsProperties.Name = "tsProperties";
			resources.ApplyResources(this.tsProperties, "tsProperties");
			this.tsProperties.Click += new System.EventHandler(this.PropertiesToolStripMenuItem_Click);
			// 
			// EnumsTreeControl
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.EnumsTreeView);
			this.Name = "EnumsTreeControl";
			this.CmsEnumsTreeView.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TreeView EnumsTreeView;
		private System.Windows.Forms.ContextMenuStrip CmsEnumsTreeView;
		private System.Windows.Forms.ToolStripMenuItem tsAddEnumTag;
		private System.Windows.Forms.ToolStripMenuItem tsAddEnumItem;
		private System.Windows.Forms.ToolStripMenuItem tsDelete;
		private System.Windows.Forms.ToolStripMenuItem tsProperties;
	}
}
