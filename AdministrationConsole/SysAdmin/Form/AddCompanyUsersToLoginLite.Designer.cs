using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class AddCompanyUsersToLoginLite
    {
        private Label LabelTitle;
        private Label LblExplication;
        private ListView ListViewUsersCompany;
        private Button BtnUnselectAll;
        private Button BtnSelectAll;
        private Button BtnSave;
        private GroupBox GroupDatabaseLogin;
        private TextBox TbLoginPassword;
        private Label LblAllUsersJoined;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddCompanyUsersToLoginLite));
			this.LabelTitle = new System.Windows.Forms.Label();
			this.LblExplication = new System.Windows.Forms.Label();
			this.ListViewUsersCompany = new System.Windows.Forms.ListView();
			this.BtnUnselectAll = new System.Windows.Forms.Button();
			this.BtnSelectAll = new System.Windows.Forms.Button();
			this.BtnSave = new System.Windows.Forms.Button();
			this.GroupDatabaseLogin = new System.Windows.Forms.GroupBox();
			this.TxtLogin = new System.Windows.Forms.TextBox();
			this.LblLogin = new System.Windows.Forms.Label();
			this.LblPassword = new System.Windows.Forms.Label();
			this.TbLoginPassword = new System.Windows.Forms.TextBox();
			this.LblAllUsersJoined = new System.Windows.Forms.Label();
			this.GroupDatabaseLogin.SuspendLayout();
			this.SuspendLayout();
			// 
			// LabelTitle
			// 
			resources.ApplyResources(this.LabelTitle, "LabelTitle");
			this.LabelTitle.BackColor = System.Drawing.Color.CornflowerBlue;
			this.LabelTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
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
			// BtnUnselectAll
			// 
			resources.ApplyResources(this.BtnUnselectAll, "BtnUnselectAll");
			this.BtnUnselectAll.Name = "BtnUnselectAll";
			this.BtnUnselectAll.Click += new System.EventHandler(this.BtnUnselectAll_Click);
			// 
			// BtnSelectAll
			// 
			resources.ApplyResources(this.BtnSelectAll, "BtnSelectAll");
			this.BtnSelectAll.Name = "BtnSelectAll";
			this.BtnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);
			// 
			// BtnSave
			// 
			resources.ApplyResources(this.BtnSave, "BtnSave");
			this.BtnSave.Name = "BtnSave";
			this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
			// 
			// GroupDatabaseLogin
			// 
			this.GroupDatabaseLogin.Controls.Add(this.TxtLogin);
			this.GroupDatabaseLogin.Controls.Add(this.LblLogin);
			this.GroupDatabaseLogin.Controls.Add(this.LblPassword);
			this.GroupDatabaseLogin.Controls.Add(this.TbLoginPassword);
			this.GroupDatabaseLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.GroupDatabaseLogin, "GroupDatabaseLogin");
			this.GroupDatabaseLogin.Name = "GroupDatabaseLogin";
			this.GroupDatabaseLogin.TabStop = false;
			// 
			// TxtLogin
			// 
			resources.ApplyResources(this.TxtLogin, "TxtLogin");
			this.TxtLogin.Name = "TxtLogin";
			// 
			// LblLogin
			// 
			resources.ApplyResources(this.LblLogin, "LblLogin");
			this.LblLogin.Name = "LblLogin";
			// 
			// LblPassword
			// 
			resources.ApplyResources(this.LblPassword, "LblPassword");
			this.LblPassword.Name = "LblPassword";
			// 
			// TbLoginPassword
			// 
			resources.ApplyResources(this.TbLoginPassword, "TbLoginPassword");
			this.TbLoginPassword.Name = "TbLoginPassword";
			// 
			// LblAllUsersJoined
			// 
			resources.ApplyResources(this.LblAllUsersJoined, "LblAllUsersJoined");
			this.LblAllUsersJoined.ForeColor = System.Drawing.Color.Red;
			this.LblAllUsersJoined.Name = "LblAllUsersJoined";
			// 
			// AddCompanyUsersToLoginLite
			// 
			this.AcceptButton = this.BtnSave;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.LblAllUsersJoined);
			this.Controls.Add(this.GroupDatabaseLogin);
			this.Controls.Add(this.BtnSave);
			this.Controls.Add(this.BtnUnselectAll);
			this.Controls.Add(this.BtnSelectAll);
			this.Controls.Add(this.ListViewUsersCompany);
			this.Controls.Add(this.LblExplication);
			this.Controls.Add(this.LabelTitle);
			this.Name = "AddCompanyUsersToLoginLite";
			this.ShowInTaskbar = false;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.AddCompanyUsersToLoginLite_Closing);
			this.Deactivate += new System.EventHandler(this.AddCompanyUsersToLoginLite_Deactivate);
			this.Load += new System.EventHandler(this.AddCompanyUsersToLoginLite_Load);
			this.VisibleChanged += new System.EventHandler(this.AddCompanyUsersToLoginLite_VisibleChanged);
			this.GroupDatabaseLogin.ResumeLayout(false);
			this.GroupDatabaseLogin.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
		#endregion

		private Label LblPassword;
		private Label LblLogin;
		private TextBox TxtLogin;
	}
}
