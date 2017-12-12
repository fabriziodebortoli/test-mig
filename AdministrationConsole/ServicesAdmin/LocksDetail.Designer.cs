using System.ComponentModel;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;

namespace Microarea.Console.Plugin.ServicesAdmin
{
    partial class LocksDetail
    {
        private ContextMenu contextMenuUser = null;
        private ContextMenu contextRootTree = null;
        private ContextMenu contextMenuCompany = null;
        private ContextMenu contextMenuUserDataGrid = null;
        private ContextMenu contextMenuCompanyDataGrid = null;
        private Panel panelUsers;
        private Panel panelTitle;
        private Panel panelDetails;
        private Label SubtitleLabel;
        private Label TitleLabel;
        private Splitter splitter1;
        private PlugInsTreeView treeUsersLocks;
        private TabControl tabLockViewer;
        private TabPage tabPageUser;
        private Splitter splitter2;
        private TabPage tabPageCompany;
        private Splitter splitter3;
        private DataGrid ViewCompaniesLocksDataGrid;
        private DataGrid ViewUsersLocksDataGrid;
        private PlugInsTreeView treeCompaniesLocks;

        /// <summary>
        /// Dispose
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LocksDetail));
			this.panelUsers = new System.Windows.Forms.Panel();
			this.panelDetails = new System.Windows.Forms.Panel();
			this.tabLockViewer = new System.Windows.Forms.TabControl();
			this.tabPageUser = new System.Windows.Forms.TabPage();
			this.ViewUsersLocksDataGrid = new System.Windows.Forms.DataGrid();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.treeUsersLocks = new PlugInsTreeView();
			this.tabPageCompany = new System.Windows.Forms.TabPage();
			this.ViewCompaniesLocksDataGrid = new System.Windows.Forms.DataGrid();
			this.splitter3 = new System.Windows.Forms.Splitter();
			this.treeCompaniesLocks = new PlugInsTreeView();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panelTitle = new System.Windows.Forms.Panel();
			this.SubtitleLabel = new System.Windows.Forms.Label();
			this.TitleLabel = new System.Windows.Forms.Label();
			this.panelUsers.SuspendLayout();
			this.panelDetails.SuspendLayout();
			this.tabLockViewer.SuspendLayout();
			this.tabPageUser.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ViewUsersLocksDataGrid)).BeginInit();
			this.tabPageCompany.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ViewCompaniesLocksDataGrid)).BeginInit();
			this.panelTitle.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelUsers
			// 
			resources.ApplyResources(this.panelUsers, "panelUsers");
			this.panelUsers.Controls.Add(this.panelDetails);
			this.panelUsers.Controls.Add(this.panelTitle);
			this.panelUsers.Name = "panelUsers";
			// 
			// panelDetails
			// 
			this.panelDetails.AllowDrop = true;
			resources.ApplyResources(this.panelDetails, "panelDetails");
			this.panelDetails.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelDetails.Controls.Add(this.tabLockViewer);
			this.panelDetails.Controls.Add(this.splitter1);
			this.panelDetails.Name = "panelDetails";
			// 
			// tabLockViewer
			// 
			this.tabLockViewer.Controls.Add(this.tabPageUser);
			this.tabLockViewer.Controls.Add(this.tabPageCompany);
			resources.ApplyResources(this.tabLockViewer, "tabLockViewer");
			this.tabLockViewer.Name = "tabLockViewer";
			this.tabLockViewer.SelectedIndex = 0;
			this.tabLockViewer.TabIndexChanged += new System.EventHandler(this.tabLockViewer_TabIndexChanged);
			this.tabLockViewer.SelectedIndexChanged += new System.EventHandler(this.tabLockViewer_SelectedIndexChanged);
			// 
			// tabPageUser
			// 
			this.tabPageUser.Controls.Add(this.ViewUsersLocksDataGrid);
			this.tabPageUser.Controls.Add(this.splitter2);
			this.tabPageUser.Controls.Add(this.treeUsersLocks);
			resources.ApplyResources(this.tabPageUser, "tabPageUser");
			this.tabPageUser.Name = "tabPageUser";
			// 
			// ViewUsersLocksDataGrid
			// 
			this.ViewUsersLocksDataGrid.AllowNavigation = false;
			this.ViewUsersLocksDataGrid.BackgroundColor = System.Drawing.Color.Lavender;
			this.ViewUsersLocksDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ViewUsersLocksDataGrid.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
			resources.ApplyResources(this.ViewUsersLocksDataGrid, "ViewUsersLocksDataGrid");
			this.ViewUsersLocksDataGrid.CaptionForeColor = System.Drawing.Color.Navy;
			this.ViewUsersLocksDataGrid.DataMember = "";
			this.ViewUsersLocksDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ViewUsersLocksDataGrid.Name = "ViewUsersLocksDataGrid";
			this.ViewUsersLocksDataGrid.ReadOnly = true;
			this.ViewUsersLocksDataGrid.SizeChanged += new System.EventHandler(this.ViewUsersLocksDataGrid_SizeChanged);
			this.ViewUsersLocksDataGrid.CurrentCellChanged += new System.EventHandler(this.ViewUsersLocksDataGrid_CurrentCellChanged);
			this.ViewUsersLocksDataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ViewUsersLocksDataGrid_MouseDown);
			// 
			// splitter2
			// 
			resources.ApplyResources(this.splitter2, "splitter2");
			this.splitter2.Name = "splitter2";
			this.splitter2.TabStop = false;
			// 
			// treeUsersLocks
			// 
			this.treeUsersLocks.AllowDrop = true;
			resources.ApplyResources(this.treeUsersLocks, "treeUsersLocks");
			this.treeUsersLocks.FullRowSelect = true;
			this.treeUsersLocks.ItemHeight = 20;
			this.treeUsersLocks.Name = "treeUsersLocks";
			this.treeUsersLocks.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeUsersLocks_AfterSelect);
			this.treeUsersLocks.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeUsersLocks_MouseDown);
			// 
			// tabPageCompany
			// 
			this.tabPageCompany.Controls.Add(this.ViewCompaniesLocksDataGrid);
			this.tabPageCompany.Controls.Add(this.splitter3);
			this.tabPageCompany.Controls.Add(this.treeCompaniesLocks);
			resources.ApplyResources(this.tabPageCompany, "tabPageCompany");
			this.tabPageCompany.Name = "tabPageCompany";
			// 
			// ViewCompaniesLocksDataGrid
			// 
			this.ViewCompaniesLocksDataGrid.AllowNavigation = false;
			this.ViewCompaniesLocksDataGrid.BackgroundColor = System.Drawing.Color.Lavender;
			this.ViewCompaniesLocksDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ViewCompaniesLocksDataGrid.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
			resources.ApplyResources(this.ViewCompaniesLocksDataGrid, "ViewCompaniesLocksDataGrid");
			this.ViewCompaniesLocksDataGrid.CaptionForeColor = System.Drawing.Color.Navy;
			this.ViewCompaniesLocksDataGrid.DataMember = "";
			this.ViewCompaniesLocksDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ViewCompaniesLocksDataGrid.Name = "ViewCompaniesLocksDataGrid";
			this.ViewCompaniesLocksDataGrid.ReadOnly = true;
			this.ViewCompaniesLocksDataGrid.SizeChanged += new System.EventHandler(this.ViewCompaniesLocksDataGrid_SizeChanged);
			this.ViewCompaniesLocksDataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ViewCompaniesLocksDataGrid_MouseDown);
			// 
			// splitter3
			// 
			resources.ApplyResources(this.splitter3, "splitter3");
			this.splitter3.Name = "splitter3";
			this.splitter3.TabStop = false;
			// 
			// treeCompaniesLocks
			// 
			this.treeCompaniesLocks.AllowDrop = true;
			resources.ApplyResources(this.treeCompaniesLocks, "treeCompaniesLocks");
			this.treeCompaniesLocks.FullRowSelect = true;
			this.treeCompaniesLocks.ItemHeight = 20;
			this.treeCompaniesLocks.Name = "treeCompaniesLocks";
			this.treeCompaniesLocks.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeCompaniesLocks_AfterSelect);
			this.treeCompaniesLocks.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeCompaniesLocks_MouseDown);
			// 
			// splitter1
			// 
			resources.ApplyResources(this.splitter1, "splitter1");
			this.splitter1.Name = "splitter1";
			this.splitter1.TabStop = false;
			// 
			// panelTitle
			// 
			this.panelTitle.BackColor = System.Drawing.Color.Lavender;
			this.panelTitle.Controls.Add(this.SubtitleLabel);
			this.panelTitle.Controls.Add(this.TitleLabel);
			resources.ApplyResources(this.panelTitle, "panelTitle");
			this.panelTitle.Name = "panelTitle";
			// 
			// SubtitleLabel
			// 
			resources.ApplyResources(this.SubtitleLabel, "SubtitleLabel");
			this.SubtitleLabel.BackColor = System.Drawing.Color.Lavender;
			this.SubtitleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SubtitleLabel.ForeColor = System.Drawing.Color.Navy;
			this.SubtitleLabel.Name = "SubtitleLabel";
			// 
			// TitleLabel
			// 
			resources.ApplyResources(this.TitleLabel, "TitleLabel");
			this.TitleLabel.BackColor = System.Drawing.Color.Lavender;
			this.TitleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TitleLabel.ForeColor = System.Drawing.Color.Navy;
			this.TitleLabel.Name = "TitleLabel";
			// 
			// LocksDetail
			// 
			this.AllowDrop = true;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.panelUsers);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LocksDetail";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.LocksDetail_Load);
			this.panelUsers.ResumeLayout(false);
			this.panelDetails.ResumeLayout(false);
			this.tabLockViewer.ResumeLayout(false);
			this.tabPageUser.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ViewUsersLocksDataGrid)).EndInit();
			this.tabPageCompany.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ViewCompaniesLocksDataGrid)).EndInit();
			this.panelTitle.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        #endregion

        private IContainer components;
    }
}
