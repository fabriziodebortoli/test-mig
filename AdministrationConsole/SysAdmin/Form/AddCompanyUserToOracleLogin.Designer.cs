using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class AddCompanyUserToOracleLogin
    {
        //---------------------------------------------------------------------
        private Label LabelTitle;
        private Label LblExplication;
        private Label LblOracleService;
        private GroupBox GrBoxOracleConnData;
        private Label LblOracleUserPwd;
        private Label LblAllUsersJoined;
        private ListView ListViewUsersCompany;
        private Button BtnSave;
        private Button BtnUnselectAll;
        private Button BtnSelectAll;
        private TextBox TextBoxOracleService;
        private ComboBox ComboOracleLogins;
        private TextBox TextBoxOracleUserPwd;
        private Button BtnOracleConnectionCheck;
        private RadioButton rbNewOracleUser;
        private TextBox txtNewOracleUser;
        private RadioButton rbSelectExistedOracleUser;
        private CheckBox CbNTSecurity;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddCompanyUserToOracleLogin));
			this.LabelTitle = new System.Windows.Forms.Label();
			this.LblExplication = new System.Windows.Forms.Label();
			this.GrBoxOracleConnData = new System.Windows.Forms.GroupBox();
			this.CbNTSecurity = new System.Windows.Forms.CheckBox();
			this.rbSelectExistedOracleUser = new System.Windows.Forms.RadioButton();
			this.txtNewOracleUser = new System.Windows.Forms.TextBox();
			this.rbNewOracleUser = new System.Windows.Forms.RadioButton();
			this.BtnOracleConnectionCheck = new System.Windows.Forms.Button();
			this.TextBoxOracleUserPwd = new System.Windows.Forms.TextBox();
			this.LblOracleService = new System.Windows.Forms.Label();
			this.TextBoxOracleService = new System.Windows.Forms.TextBox();
			this.ComboOracleLogins = new System.Windows.Forms.ComboBox();
			this.LblOracleUserPwd = new System.Windows.Forms.Label();
			this.LblAllUsersJoined = new System.Windows.Forms.Label();
			this.ListViewUsersCompany = new System.Windows.Forms.ListView();
			this.BtnSave = new System.Windows.Forms.Button();
			this.BtnUnselectAll = new System.Windows.Forms.Button();
			this.BtnSelectAll = new System.Windows.Forms.Button();
			this.GrBoxOracleConnData.SuspendLayout();
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
			this.LblExplication.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblExplication, "LblExplication");
			this.LblExplication.Name = "LblExplication";
			// 
			// GrBoxOracleConnData
			// 
			this.GrBoxOracleConnData.Controls.Add(this.CbNTSecurity);
			this.GrBoxOracleConnData.Controls.Add(this.rbSelectExistedOracleUser);
			this.GrBoxOracleConnData.Controls.Add(this.txtNewOracleUser);
			this.GrBoxOracleConnData.Controls.Add(this.rbNewOracleUser);
			this.GrBoxOracleConnData.Controls.Add(this.BtnOracleConnectionCheck);
			this.GrBoxOracleConnData.Controls.Add(this.TextBoxOracleUserPwd);
			this.GrBoxOracleConnData.Controls.Add(this.LblOracleService);
			this.GrBoxOracleConnData.Controls.Add(this.TextBoxOracleService);
			this.GrBoxOracleConnData.Controls.Add(this.ComboOracleLogins);
			this.GrBoxOracleConnData.Controls.Add(this.LblOracleUserPwd);
			this.GrBoxOracleConnData.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.GrBoxOracleConnData, "GrBoxOracleConnData");
			this.GrBoxOracleConnData.Name = "GrBoxOracleConnData";
			this.GrBoxOracleConnData.TabStop = false;
			// 
			// CbNTSecurity
			// 
			resources.ApplyResources(this.CbNTSecurity, "CbNTSecurity");
			this.CbNTSecurity.Name = "CbNTSecurity";
			this.CbNTSecurity.CheckedChanged += new System.EventHandler(this.CbNTSecurity_CheckedChanged);
			// 
			// rbSelectExistedOracleUser
			// 
			resources.ApplyResources(this.rbSelectExistedOracleUser, "rbSelectExistedOracleUser");
			this.rbSelectExistedOracleUser.Checked = true;
			this.rbSelectExistedOracleUser.Name = "rbSelectExistedOracleUser";
			this.rbSelectExistedOracleUser.TabStop = true;
			this.rbSelectExistedOracleUser.CheckedChanged += new System.EventHandler(this.rbSelectExistedOracleUser_CheckedChanged);
			// 
			// txtNewOracleUser
			// 
			resources.ApplyResources(this.txtNewOracleUser, "txtNewOracleUser");
			this.txtNewOracleUser.Name = "txtNewOracleUser";
			this.txtNewOracleUser.Leave += new System.EventHandler(this.txtNewOracleUser_Leave);
			// 
			// rbNewOracleUser
			// 
			resources.ApplyResources(this.rbNewOracleUser, "rbNewOracleUser");
			this.rbNewOracleUser.Name = "rbNewOracleUser";
			this.rbNewOracleUser.CheckedChanged += new System.EventHandler(this.rbNewOracleUser_CheckedChanged);
			// 
			// BtnOracleConnectionCheck
			// 
			resources.ApplyResources(this.BtnOracleConnectionCheck, "BtnOracleConnectionCheck");
			this.BtnOracleConnectionCheck.Name = "BtnOracleConnectionCheck";
			this.BtnOracleConnectionCheck.Click += new System.EventHandler(this.BtnOracleConnectionCheck_Click);
			// 
			// TextBoxOracleUserPwd
			// 
			resources.ApplyResources(this.TextBoxOracleUserPwd, "TextBoxOracleUserPwd");
			this.TextBoxOracleUserPwd.Name = "TextBoxOracleUserPwd";
			this.TextBoxOracleUserPwd.TextChanged += new System.EventHandler(this.TextBoxOracleUserPwd_TextChanged);
			// 
			// LblOracleService
			// 
			resources.ApplyResources(this.LblOracleService, "LblOracleService");
			this.LblOracleService.Name = "LblOracleService";
			// 
			// TextBoxOracleService
			// 
			resources.ApplyResources(this.TextBoxOracleService, "TextBoxOracleService");
			this.TextBoxOracleService.Name = "TextBoxOracleService";
			this.TextBoxOracleService.ReadOnly = true;
			// 
			// ComboOracleLogins
			// 
			this.ComboOracleLogins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.ComboOracleLogins, "ComboOracleLogins");
			this.ComboOracleLogins.Name = "ComboOracleLogins";
			this.ComboOracleLogins.DropDown += new System.EventHandler(this.ComboOracleLogins_DropDown);
			this.ComboOracleLogins.SelectedIndexChanged += new System.EventHandler(this.ComboOracleLogins_SelectedIndexChanged);
			// 
			// LblOracleUserPwd
			// 
			resources.ApplyResources(this.LblOracleUserPwd, "LblOracleUserPwd");
			this.LblOracleUserPwd.Name = "LblOracleUserPwd";
			// 
			// LblAllUsersJoined
			// 
			resources.ApplyResources(this.LblAllUsersJoined, "LblAllUsersJoined");
			this.LblAllUsersJoined.ForeColor = System.Drawing.Color.Red;
			this.LblAllUsersJoined.Name = "LblAllUsersJoined";
			// 
			// ListViewUsersCompany
			// 
			this.ListViewUsersCompany.AllowDrop = true;
			resources.ApplyResources(this.ListViewUsersCompany, "ListViewUsersCompany");
			this.ListViewUsersCompany.CheckBoxes = true;
			this.ListViewUsersCompany.FullRowSelect = true;
			this.ListViewUsersCompany.GridLines = true;
			this.ListViewUsersCompany.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.ListViewUsersCompany.MultiSelect = false;
			this.ListViewUsersCompany.Name = "ListViewUsersCompany";
			this.ListViewUsersCompany.UseCompatibleStateImageBehavior = false;
			this.ListViewUsersCompany.View = System.Windows.Forms.View.Details;
			this.ListViewUsersCompany.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ListViewUsersCompany_ItemCheck);
			// 
			// BtnSave
			// 
			resources.ApplyResources(this.BtnSave, "BtnSave");
			this.BtnSave.Name = "BtnSave";
			this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
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
			// AddCompanyUserToOracleLogin
			// 
			this.AcceptButton = this.BtnSave;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.BtnSave);
			this.Controls.Add(this.BtnUnselectAll);
			this.Controls.Add(this.BtnSelectAll);
			this.Controls.Add(this.ListViewUsersCompany);
			this.Controls.Add(this.LblAllUsersJoined);
			this.Controls.Add(this.GrBoxOracleConnData);
			this.Controls.Add(this.LblExplication);
			this.Controls.Add(this.LabelTitle);
			this.Name = "AddCompanyUserToOracleLogin";
			this.ShowInTaskbar = false;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.AddCompanyUserToOracleLogin_Closing);
			this.Deactivate += new System.EventHandler(this.AddCompanyUserToOracleLogin_Deactivate);
			this.Load += new System.EventHandler(this.AddCompanyUserToOracleLogin_Load);
			this.VisibleChanged += new System.EventHandler(this.AddCompanyUserToOracleLogin_VisibleChanged);
			this.GrBoxOracleConnData.ResumeLayout(false);
			this.GrBoxOracleConnData.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion
    }
}
