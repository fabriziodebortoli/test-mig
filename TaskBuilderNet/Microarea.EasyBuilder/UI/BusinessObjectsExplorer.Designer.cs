using System;
namespace Microarea.EasyBuilder.UI
{
	partial class BusinessObjectsExplorer
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

				if (editor != null)
				{
					editor.ComponentDeleted -= new EventHandler<DeleteObjectEventArgs>(editor_ComponentDeleted);
					editor = null;
				}

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BusinessObjectsExplorer));
			this.toolBar = new System.Windows.Forms.ToolStrip();
			this.tbsFilter = new System.Windows.Forms.ToolStripDropDownButton();
			this.mnuFilterNotExposed = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuFilterInErrorState = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuFilterExposedAndUsed = new System.Windows.Forms.ToolStripMenuItem();
			this.treeBusinessObjects = new System.Windows.Forms.TreeView();
			this.lblTreeCaption = new System.Windows.Forms.Label();
			this.toolBar.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolBar
			// 
			this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbsFilter});
			resources.ApplyResources(this.toolBar, "toolBar");
			this.toolBar.Name = "toolBar";
			// 
			// tbsFilter
			// 
			this.tbsFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tbsFilter.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFilterNotExposed,
            this.mnuFilterInErrorState,
            this.mnuFilterExposedAndUsed});
			this.tbsFilter.Image = global::Microarea.EasyBuilder.Properties.Resources.Filter;
			resources.ApplyResources(this.tbsFilter, "tbsFilter");
			this.tbsFilter.Name = "tbsFilter";
			// 
			// mnuFilterNotExposed
			// 
			this.mnuFilterNotExposed.Checked = true;
			this.mnuFilterNotExposed.CheckState = System.Windows.Forms.CheckState.Checked;
			this.mnuFilterNotExposed.Name = "mnuFilterNotExposed";
			resources.ApplyResources(this.mnuFilterNotExposed, "mnuFilterNotExposed");
			this.mnuFilterNotExposed.Click += new System.EventHandler(this.mnuFilterNotExposed_Click);
			// 
			// mnuFilterInErrorState
			// 
			this.mnuFilterInErrorState.Checked = true;
			this.mnuFilterInErrorState.CheckState = System.Windows.Forms.CheckState.Checked;
			this.mnuFilterInErrorState.ForeColor = System.Drawing.Color.Red;
			this.mnuFilterInErrorState.Name = "mnuFilterInErrorState";
			resources.ApplyResources(this.mnuFilterInErrorState, "mnuFilterInErrorState");
			this.mnuFilterInErrorState.Click += new System.EventHandler(this.mnuFilterInErrorState_Click);
			// 
			// mnuFilterExposedAndUsed
			// 
			this.mnuFilterExposedAndUsed.ForeColor = System.Drawing.Color.Green;
			this.mnuFilterExposedAndUsed.Name = "mnuFilterExposedAndUsed";
			resources.ApplyResources(this.mnuFilterExposedAndUsed, "mnuFilterExposedAndUsed");
			this.mnuFilterExposedAndUsed.Click += new System.EventHandler(this.mnuFilterExposedAndUsed_Click);
			// 
			// treeBusinessObjects
			// 
			resources.ApplyResources(this.treeBusinessObjects, "treeBusinessObjects");
			this.treeBusinessObjects.Name = "treeBusinessObjects";
			this.treeBusinessObjects.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeDocuments_MouseDown);
			this.treeBusinessObjects.MouseMove += new System.Windows.Forms.MouseEventHandler(this.treeDocuments_MouseMove);
			// 
			// lblTreeCaption
			// 
			resources.ApplyResources(this.lblTreeCaption, "lblTreeCaption");
			this.lblTreeCaption.Name = "lblTreeCaption";
			// 
			// BusinessObjectsExplorer
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblTreeCaption);
			this.Controls.Add(this.treeBusinessObjects);
			this.Controls.Add(this.toolBar);
			this.Name = "BusinessObjectsExplorer";
			this.toolBar.ResumeLayout(false);
			this.toolBar.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolBar;
		private System.Windows.Forms.TreeView treeBusinessObjects;
		private System.Windows.Forms.Label lblTreeCaption;
		private System.Windows.Forms.ToolStripDropDownButton tbsFilter;
		private System.Windows.Forms.ToolStripMenuItem mnuFilterNotExposed;
		private System.Windows.Forms.ToolStripMenuItem mnuFilterExposedAndUsed;
		private System.Windows.Forms.ToolStripMenuItem mnuFilterInErrorState;
	}
}
