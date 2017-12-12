using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class AddCompanyUserToLogin
    {
        private Label LabelTitle;
        private Label LblExplication;
        private Label LblAllUsersJoined;
        private ListView ListViewUsersCompany;
        private Button BtnSelectAll;
        private Button BtnUnselectAll;
        private Button BtnSave;
        private System.ComponentModel.Container components = null;

        /// <summary>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddCompanyUserToLogin));
			this.LabelTitle = new System.Windows.Forms.Label();
			this.LblExplication = new System.Windows.Forms.Label();
			this.ListViewUsersCompany = new System.Windows.Forms.ListView();
			this.BtnSelectAll = new System.Windows.Forms.Button();
			this.BtnUnselectAll = new System.Windows.Forms.Button();
			this.BtnSave = new System.Windows.Forms.Button();
			this.LblAllUsersJoined = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// LabelTitle
			// 
			this.LabelTitle.BackColor = System.Drawing.Color.CornflowerBlue;
			this.LabelTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.LabelTitle, "LabelTitle");
			this.LabelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelTitle.ForeColor = System.Drawing.Color.White;
			this.LabelTitle.Name = "LabelTitle";
			// 
			// LblExplication
			// 
			resources.ApplyResources(this.LblExplication, "LblExplication");
			this.LblExplication.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblExplication.Name = "LblExplication";
			// 
			// ListViewUsersCompany
			// 
			this.ListViewUsersCompany.AllowDrop = true;
			resources.ApplyResources(this.ListViewUsersCompany, "ListViewUsersCompany");
			this.ListViewUsersCompany.CheckBoxes = true;
			this.ListViewUsersCompany.FullRowSelect = true;
			this.ListViewUsersCompany.GridLines = true;
			this.ListViewUsersCompany.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.ListViewUsersCompany.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("ListViewUsersCompany.Items")))});
			this.ListViewUsersCompany.MultiSelect = false;
			this.ListViewUsersCompany.Name = "ListViewUsersCompany";
			this.ListViewUsersCompany.UseCompatibleStateImageBehavior = false;
			this.ListViewUsersCompany.View = System.Windows.Forms.View.Details;
			this.ListViewUsersCompany.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ListViewUsersCompany_ItemCheck);
			// 
			// BtnSelectAll
			// 
			resources.ApplyResources(this.BtnSelectAll, "BtnSelectAll");
			this.BtnSelectAll.Name = "BtnSelectAll";
			this.BtnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);
			// 
			// BtnUnselectAll
			// 
			resources.ApplyResources(this.BtnUnselectAll, "BtnUnselectAll");
			this.BtnUnselectAll.Name = "BtnUnselectAll";
			this.BtnUnselectAll.Click += new System.EventHandler(this.BtnUnselectAll_Click);
			// 
			// BtnSave
			// 
			resources.ApplyResources(this.BtnSave, "BtnSave");
			this.BtnSave.Name = "BtnSave";
			this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
			// 
			// LblAllUsersJoined
			// 
			resources.ApplyResources(this.LblAllUsersJoined, "LblAllUsersJoined");
			this.LblAllUsersJoined.ForeColor = System.Drawing.Color.Red;
			this.LblAllUsersJoined.Name = "LblAllUsersJoined";
			// 
			// AddCompanyUserToLogin
			// 
			this.AcceptButton = this.BtnSave;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.LblAllUsersJoined);
			this.Controls.Add(this.BtnSave);
			this.Controls.Add(this.BtnUnselectAll);
			this.Controls.Add(this.BtnSelectAll);
			this.Controls.Add(this.ListViewUsersCompany);
			this.Controls.Add(this.LblExplication);
			this.Controls.Add(this.LabelTitle);
			this.Name = "AddCompanyUserToLogin";
			this.ShowInTaskbar = false;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.AddCompanyUserToLogin_Closing);
			this.Deactivate += new System.EventHandler(this.AddCompanyUserToLogin_Deactivate);
			this.Load += new System.EventHandler(this.AddCompanyUserToLogin_Load);
			this.VisibleChanged += new System.EventHandler(this.AddCompanyUserToLogin_VisibleChanged);
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion
    }
}
