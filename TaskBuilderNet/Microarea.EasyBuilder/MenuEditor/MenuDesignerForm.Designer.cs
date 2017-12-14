
using System;
using System.Windows.Forms;
namespace Microarea.EasyBuilder.MenuEditor
{
	/// <remarks/>
	partial class MenuDesignerForm
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
				if (currentMenuModel != null)
				{
					currentMenuModel.PropertyValueChanged -= new EventHandler<MenuItemPropertyValueChangedEventArgs>(CurrentMenuModel_PropertyValueChanged);
					currentMenuModel.MenuModelCleared -= new EventHandler<EventArgs>(CurrentMenuModel_MenuModelCleared);
					currentMenuModel = null;
				}
				if (this.mainToolbar != null)
				{
					if (!this.mainToolbar.IsDisposed)
					{
						this.mainToolbar.FormClosing -= new FormClosingEventHandler(MainToolbar_FormClosing);
						this.mainToolbar.FormClosed -= new FormClosedEventHandler(MainToolbar_FormClosed);
						this.mainToolbar.Dispose();
					}

					this.mainToolbar = null;
				}
				if (imagesManager != null)
				{
					imagesManager.Dispose();
					imagesManager = null;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuDesignerForm));
			this.MenuDesignerContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.MoveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MoveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ActionsToolStripMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.InsertApplicationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.InsertGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.InsertMenuBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.InsertChildMenuBranchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.InsertCommandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.InsertDocumentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.InsertBatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.InsertReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.InsertFunctionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.InsertExecutableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.InserTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.InsertOfficeItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CutCopyPasteDeleteToolstripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.CutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.PasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DeleteMenuItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.PropertiesToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.ShowPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SetImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ResetImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuDesignerCtrl = new Microarea.EasyBuilder.MenuEditor.MenuDesignerControl();
			this.MenuDesignerContextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// MenuDesignerContextMenuStrip
			// 
			resources.ApplyResources(this.MenuDesignerContextMenuStrip, "MenuDesignerContextMenuStrip");
			this.MenuDesignerContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MoveUpToolStripMenuItem,
            this.MoveDownToolStripMenuItem,
            this.ActionsToolStripMenuSeparator,
            this.InsertApplicationToolStripMenuItem,
            this.InsertGroupToolStripMenuItem,
            this.InsertMenuBranchToolStripMenuItem,
            this.InsertChildMenuBranchToolStripMenuItem,
            this.InsertCommandToolStripMenuItem,
            this.CutCopyPasteDeleteToolstripSeparator,
            this.CutToolStripMenuItem,
            this.CopyToolStripMenuItem,
            this.PasteToolStripMenuItem,
            this.DeleteMenuItemToolStripMenuItem,
            this.PropertiesToolStripSeparator,
            this.ShowPropertiesToolStripMenuItem,
            this.SetImageToolStripMenuItem,
            this.ResetImageToolStripMenuItem});
			this.MenuDesignerContextMenuStrip.Name = "MenuDesignerContextMenuStrip";
			this.MenuDesignerContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.MenuDesignerContextMenuStrip_Opening);
			// 
			// MoveUpToolStripMenuItem
			// 
			resources.ApplyResources(this.MoveUpToolStripMenuItem, "MoveUpToolStripMenuItem");
			this.MoveUpToolStripMenuItem.Name = "MoveUpToolStripMenuItem";
			this.MoveUpToolStripMenuItem.Click += new System.EventHandler(this.MoveUpToolStripMenuItem_Click);
			// 
			// MoveDownToolStripMenuItem
			// 
			resources.ApplyResources(this.MoveDownToolStripMenuItem, "MoveDownToolStripMenuItem");
			this.MoveDownToolStripMenuItem.Name = "MoveDownToolStripMenuItem";
			this.MoveDownToolStripMenuItem.Click += new System.EventHandler(this.MoveDownToolStripMenuItem_Click);
			// 
			// ActionsToolStripMenuSeparator
			// 
			this.ActionsToolStripMenuSeparator.Name = "ActionsToolStripMenuSeparator";
			resources.ApplyResources(this.ActionsToolStripMenuSeparator, "ActionsToolStripMenuSeparator");
			// 
			// InsertApplicationToolStripMenuItem
			// 
			resources.ApplyResources(this.InsertApplicationToolStripMenuItem, "InsertApplicationToolStripMenuItem");
			this.InsertApplicationToolStripMenuItem.Name = "InsertApplicationToolStripMenuItem";
			this.InsertApplicationToolStripMenuItem.Click += new System.EventHandler(this.InsertApplicationToolStripMenuItem_Click);
			// 
			// InsertGroupToolStripMenuItem
			// 
			resources.ApplyResources(this.InsertGroupToolStripMenuItem, "InsertGroupToolStripMenuItem");
			this.InsertGroupToolStripMenuItem.Name = "InsertGroupToolStripMenuItem";
			this.InsertGroupToolStripMenuItem.Click += new System.EventHandler(this.InsertGroupToolStripMenuItem_Click);
			// 
			// InsertMenuBranchToolStripMenuItem
			// 
			resources.ApplyResources(this.InsertMenuBranchToolStripMenuItem, "InsertMenuBranchToolStripMenuItem");
			this.InsertMenuBranchToolStripMenuItem.Name = "InsertMenuBranchToolStripMenuItem";
			this.InsertMenuBranchToolStripMenuItem.Click += new System.EventHandler(this.InsertMenuBranchToolStripMenuItem_Click);
			// 
			// InsertChildMenuBranchToolStripMenuItem
			// 
			resources.ApplyResources(this.InsertChildMenuBranchToolStripMenuItem, "InsertChildMenuBranchToolStripMenuItem");
			this.InsertChildMenuBranchToolStripMenuItem.Name = "InsertChildMenuBranchToolStripMenuItem";
			this.InsertChildMenuBranchToolStripMenuItem.Click += new System.EventHandler(this.InsertChildMenuBranchToolStripMenuItem_Click);
			// 
			// InsertCommandToolStripMenuItem
			// 
			this.InsertCommandToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InsertDocumentToolStripMenuItem,
            this.InsertBatchToolStripMenuItem,
            this.InsertReportToolStripMenuItem,
            this.InsertFunctionToolStripMenuItem,
            this.InsertExecutableToolStripMenuItem,
            this.InserTextToolStripMenuItem,
            this.InsertOfficeItemToolStripMenuItem});
			this.InsertCommandToolStripMenuItem.Name = "InsertCommandToolStripMenuItem";
			resources.ApplyResources(this.InsertCommandToolStripMenuItem, "InsertCommandToolStripMenuItem");
			// 
			// InsertDocumentToolStripMenuItem
			// 
			resources.ApplyResources(this.InsertDocumentToolStripMenuItem, "InsertDocumentToolStripMenuItem");
			this.InsertDocumentToolStripMenuItem.Name = "InsertDocumentToolStripMenuItem";
			this.InsertDocumentToolStripMenuItem.Click += new System.EventHandler(this.InsertDocumentToolStripMenuItem_Click);
			// 
			// InsertBatchToolStripMenuItem
			// 
			resources.ApplyResources(this.InsertBatchToolStripMenuItem, "InsertBatchToolStripMenuItem");
			this.InsertBatchToolStripMenuItem.Name = "InsertBatchToolStripMenuItem";
			this.InsertBatchToolStripMenuItem.Click += new System.EventHandler(this.InsertBatchToolStripMenuItem_Click);
			// 
			// InsertReportToolStripMenuItem
			// 
			resources.ApplyResources(this.InsertReportToolStripMenuItem, "InsertReportToolStripMenuItem");
			this.InsertReportToolStripMenuItem.Name = "InsertReportToolStripMenuItem";
			this.InsertReportToolStripMenuItem.Click += new System.EventHandler(this.InsertReportToolStripMenuItem_Click);
			// 
			// InsertFunctionToolStripMenuItem
			// 
			resources.ApplyResources(this.InsertFunctionToolStripMenuItem, "InsertFunctionToolStripMenuItem");
			this.InsertFunctionToolStripMenuItem.Name = "InsertFunctionToolStripMenuItem";
			this.InsertFunctionToolStripMenuItem.Click += new System.EventHandler(this.InsertFunctionToolStripMenuItem_Click);
			// 
			// InsertExecutableToolStripMenuItem
			// 
			resources.ApplyResources(this.InsertExecutableToolStripMenuItem, "InsertExecutableToolStripMenuItem");
			this.InsertExecutableToolStripMenuItem.Name = "InsertExecutableToolStripMenuItem";
			this.InsertExecutableToolStripMenuItem.Click += new System.EventHandler(this.InsertExecutableToolStripMenuItem_Click);
			// 
			// InserTextToolStripMenuItem
			// 
			resources.ApplyResources(this.InserTextToolStripMenuItem, "InserTextToolStripMenuItem");
			this.InserTextToolStripMenuItem.Name = "InserTextToolStripMenuItem";
			this.InserTextToolStripMenuItem.Click += new System.EventHandler(this.InserTextToolStripMenuItem_Click);
			// 
			// InsertOfficeItemToolStripMenuItem
			// 
			resources.ApplyResources(this.InsertOfficeItemToolStripMenuItem, "InsertOfficeItemToolStripMenuItem");
			this.InsertOfficeItemToolStripMenuItem.Name = "InsertOfficeItemToolStripMenuItem";
			this.InsertOfficeItemToolStripMenuItem.Click += new System.EventHandler(this.InsertOfficeItemToolStripMenuItem_Click);
			// 
			// CutCopyPasteDeleteToolstripSeparator
			// 
			this.CutCopyPasteDeleteToolstripSeparator.Name = "CutCopyPasteDeleteToolstripSeparator";
			resources.ApplyResources(this.CutCopyPasteDeleteToolstripSeparator, "CutCopyPasteDeleteToolstripSeparator");
			// 
			// CutToolStripMenuItem
			// 
			resources.ApplyResources(this.CutToolStripMenuItem, "CutToolStripMenuItem");
			this.CutToolStripMenuItem.Name = "CutToolStripMenuItem";
			this.CutToolStripMenuItem.Click += new System.EventHandler(this.CutToolStripMenuItem_Click);
			// 
			// CopyToolStripMenuItem
			// 
			resources.ApplyResources(this.CopyToolStripMenuItem, "CopyToolStripMenuItem");
			this.CopyToolStripMenuItem.Name = "CopyToolStripMenuItem";
			this.CopyToolStripMenuItem.Click += new System.EventHandler(this.CopyToolStripMenuItem_Click);
			// 
			// PasteToolStripMenuItem
			// 
			resources.ApplyResources(this.PasteToolStripMenuItem, "PasteToolStripMenuItem");
			this.PasteToolStripMenuItem.Name = "PasteToolStripMenuItem";
			this.PasteToolStripMenuItem.Click += new System.EventHandler(this.PasteToolStripMenuItem_Click);
			// 
			// DeleteMenuItemToolStripMenuItem
			// 
			resources.ApplyResources(this.DeleteMenuItemToolStripMenuItem, "DeleteMenuItemToolStripMenuItem");
			this.DeleteMenuItemToolStripMenuItem.Name = "DeleteMenuItemToolStripMenuItem";
			this.DeleteMenuItemToolStripMenuItem.Click += new System.EventHandler(this.DeleteMenuItemToolStripMenuItem_Click);
			// 
			// PropertiesToolStripSeparator
			// 
			this.PropertiesToolStripSeparator.Name = "PropertiesToolStripSeparator";
			resources.ApplyResources(this.PropertiesToolStripSeparator, "PropertiesToolStripSeparator");
			// 
			// ShowPropertiesToolStripMenuItem
			// 
			resources.ApplyResources(this.ShowPropertiesToolStripMenuItem, "ShowPropertiesToolStripMenuItem");
			this.ShowPropertiesToolStripMenuItem.Name = "ShowPropertiesToolStripMenuItem";
			this.ShowPropertiesToolStripMenuItem.Click += new System.EventHandler(this.ShowPropertiesToolStripMenuItem_Click);
			// 
			// SetImageToolStripMenuItem
			// 
			this.SetImageToolStripMenuItem.Name = "SetImageToolStripMenuItem";
			resources.ApplyResources(this.SetImageToolStripMenuItem, "SetImageToolStripMenuItem");
			this.SetImageToolStripMenuItem.Click += new System.EventHandler(this.SetImageToolStripMenuItem_Click);
			// 
			// ResetImageToolStripMenuItem
			// 
			this.ResetImageToolStripMenuItem.Name = "ResetImageToolStripMenuItem";
			resources.ApplyResources(this.ResetImageToolStripMenuItem, "ResetImageToolStripMenuItem");
			this.ResetImageToolStripMenuItem.Click += new System.EventHandler(this.ResetImageToolStripMenuItem_Click);
			// 
			// MenuDesignerCtrl
			// 
			resources.ApplyResources(this.MenuDesignerCtrl, "MenuDesignerCtrl");
			this.MenuDesignerCtrl.ForeColor = System.Drawing.Color.Navy;
			this.MenuDesignerCtrl.MinimumSize = new System.Drawing.Size(476, 72);
			this.MenuDesignerCtrl.Name = "MenuDesignerCtrl";
			this.MenuDesignerCtrl.SelectionService = null;
			this.MenuDesignerCtrl.UiService = null;
			this.MenuDesignerCtrl.MenuItemMoved += new System.EventHandler<Microarea.EasyBuilder.MenuEditor.MenuDesignUIItemEventArgs>(this.MenuDesignerCtrl_MenuItemMoved);
			// 
			// MenuDesignerForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			resources.ApplyResources(this, "$this");
			this.ContextMenuStrip = this.MenuDesignerContextMenuStrip;
			this.Controls.Add(this.MenuDesignerCtrl);
			this.KeyPreview = true;
			this.Name = "MenuDesignerForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.MenuDesignerContextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private MenuDesignerControl MenuDesignerCtrl;
		private System.Windows.Forms.ContextMenuStrip MenuDesignerContextMenuStrip;
        private System.Windows.Forms.ToolStripSeparator ActionsToolStripMenuSeparator;
        private System.Windows.Forms.ToolStripMenuItem ShowPropertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DeleteMenuItemToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator PropertiesToolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem CutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CopyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem PasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MoveUpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem MoveDownToolStripMenuItem;
		private ToolStripMenuItem InsertApplicationToolStripMenuItem;
		private ToolStripMenuItem InsertGroupToolStripMenuItem;
		private ToolStripMenuItem InsertMenuBranchToolStripMenuItem;
		private ToolStripMenuItem InsertChildMenuBranchToolStripMenuItem;
		private ToolStripMenuItem InsertCommandToolStripMenuItem;
		private ToolStripMenuItem InsertDocumentToolStripMenuItem;
		private ToolStripMenuItem InsertBatchToolStripMenuItem;
		private ToolStripMenuItem InsertReportToolStripMenuItem;
		private ToolStripMenuItem InsertFunctionToolStripMenuItem;
		private ToolStripMenuItem InsertExecutableToolStripMenuItem;
		private ToolStripMenuItem InserTextToolStripMenuItem;
		private ToolStripMenuItem InsertOfficeItemToolStripMenuItem;
		private ToolStripSeparator CutCopyPasteDeleteToolstripSeparator;
		private ToolStripMenuItem SetImageToolStripMenuItem;
		private ToolStripMenuItem ResetImageToolStripMenuItem;
    }
}