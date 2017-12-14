using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class ModifyCompanyUsersToOracleLogin
    {
        private Label LabelTitle;
        private GroupBox GroupDatabaseLogin;
        private Label LblInfoUsers;
        private ListView ListViewUsersCompany;
        private ComboBox CbOracleUsers;
        private Label LblExplication;
        private RadioButton RbDeleteAll;
        private RadioButton RbDisableAll;
        private RadioButton RbEnableAll;
        private RadioButton RbModifyLogin;
        private GroupBox GrBoxOracleConnData;
        private Button BtnOracleConnectionCheck;
        private Label LblOracleUserPwd;
        private Label LblSelectOracleUser;
        private TextBox TextBoxOracleUserPwd;
        private Button BtnSave;
        private GroupBox GroupBoxActions;
        private ComboBox ComboOracleLogins;
        private RadioButton rbSelectExistedOracleUser;
        private RadioButton rbNewOracleUser;
        private TextBox txtNewOracleUser;
        private CheckBox CbNTSecurity;
        private System.ComponentModel.Container components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyCompanyUsersToOracleLogin));
            this.LabelTitle = new System.Windows.Forms.Label();
            this.GroupDatabaseLogin = new System.Windows.Forms.GroupBox();
            this.LblSelectOracleUser = new System.Windows.Forms.Label();
            this.LblInfoUsers = new System.Windows.Forms.Label();
            this.ListViewUsersCompany = new System.Windows.Forms.ListView();
            this.CbOracleUsers = new System.Windows.Forms.ComboBox();
            this.LblExplication = new System.Windows.Forms.Label();
            this.RbDeleteAll = new System.Windows.Forms.RadioButton();
            this.RbDisableAll = new System.Windows.Forms.RadioButton();
            this.RbEnableAll = new System.Windows.Forms.RadioButton();
            this.RbModifyLogin = new System.Windows.Forms.RadioButton();
            this.GrBoxOracleConnData = new System.Windows.Forms.GroupBox();
            this.CbNTSecurity = new System.Windows.Forms.CheckBox();
            this.txtNewOracleUser = new System.Windows.Forms.TextBox();
            this.rbNewOracleUser = new System.Windows.Forms.RadioButton();
            this.rbSelectExistedOracleUser = new System.Windows.Forms.RadioButton();
            this.ComboOracleLogins = new System.Windows.Forms.ComboBox();
            this.BtnOracleConnectionCheck = new System.Windows.Forms.Button();
            this.LblOracleUserPwd = new System.Windows.Forms.Label();
            this.TextBoxOracleUserPwd = new System.Windows.Forms.TextBox();
            this.BtnSave = new System.Windows.Forms.Button();
            this.GroupBoxActions = new System.Windows.Forms.GroupBox();
            this.GroupDatabaseLogin.SuspendLayout();
            this.GrBoxOracleConnData.SuspendLayout();
            this.GroupBoxActions.SuspendLayout();
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
            // GroupDatabaseLogin
            // 
            resources.ApplyResources(this.GroupDatabaseLogin, "GroupDatabaseLogin");
            this.GroupDatabaseLogin.Controls.Add(this.LblSelectOracleUser);
            this.GroupDatabaseLogin.Controls.Add(this.LblInfoUsers);
            this.GroupDatabaseLogin.Controls.Add(this.ListViewUsersCompany);
            this.GroupDatabaseLogin.Controls.Add(this.CbOracleUsers);
            this.GroupDatabaseLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GroupDatabaseLogin.Name = "GroupDatabaseLogin";
            this.GroupDatabaseLogin.TabStop = false;
            // 
            // LblSelectOracleUser
            // 
            resources.ApplyResources(this.LblSelectOracleUser, "LblSelectOracleUser");
            this.LblSelectOracleUser.Name = "LblSelectOracleUser";
            // 
            // LblInfoUsers
            // 
            resources.ApplyResources(this.LblInfoUsers, "LblInfoUsers");
            this.LblInfoUsers.Name = "LblInfoUsers";
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
            // CbOracleUsers
            // 
            resources.ApplyResources(this.CbOracleUsers, "CbOracleUsers");
            this.CbOracleUsers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CbOracleUsers.Name = "CbOracleUsers";
            this.CbOracleUsers.SelectedIndexChanged += new System.EventHandler(this.CbOracleUsers_SelectedIndexChanged);
            this.CbOracleUsers.DropDown += new System.EventHandler(this.CbOracleUsers_DropDown);
            // 
            // LblExplication
            // 
            resources.ApplyResources(this.LblExplication, "LblExplication");
            this.LblExplication.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblExplication.Name = "LblExplication";
            // 
            // RbDeleteAll
            // 
            resources.ApplyResources(this.RbDeleteAll, "RbDeleteAll");
            this.RbDeleteAll.Name = "RbDeleteAll";
            this.RbDeleteAll.CheckedChanged += new System.EventHandler(this.RbDeleteAll_CheckedChanged);
            // 
            // RbDisableAll
            // 
            resources.ApplyResources(this.RbDisableAll, "RbDisableAll");
            this.RbDisableAll.Name = "RbDisableAll";
            this.RbDisableAll.CheckedChanged += new System.EventHandler(this.RbDisableAll_CheckedChanged);
            // 
            // RbEnableAll
            // 
            resources.ApplyResources(this.RbEnableAll, "RbEnableAll");
            this.RbEnableAll.Name = "RbEnableAll";
            this.RbEnableAll.CheckedChanged += new System.EventHandler(this.RbEnableAll_CheckedChanged);
            // 
            // RbModifyLogin
            // 
            resources.ApplyResources(this.RbModifyLogin, "RbModifyLogin");
            this.RbModifyLogin.Checked = true;
            this.RbModifyLogin.Name = "RbModifyLogin";
            this.RbModifyLogin.TabStop = true;
            this.RbModifyLogin.CheckedChanged += new System.EventHandler(this.RbModifyLogin_CheckedChanged);
            // 
            // GrBoxOracleConnData
            // 
            resources.ApplyResources(this.GrBoxOracleConnData, "GrBoxOracleConnData");
            this.GrBoxOracleConnData.Controls.Add(this.CbNTSecurity);
            this.GrBoxOracleConnData.Controls.Add(this.txtNewOracleUser);
            this.GrBoxOracleConnData.Controls.Add(this.rbNewOracleUser);
            this.GrBoxOracleConnData.Controls.Add(this.rbSelectExistedOracleUser);
            this.GrBoxOracleConnData.Controls.Add(this.ComboOracleLogins);
            this.GrBoxOracleConnData.Controls.Add(this.BtnOracleConnectionCheck);
            this.GrBoxOracleConnData.Controls.Add(this.LblOracleUserPwd);
            this.GrBoxOracleConnData.Controls.Add(this.TextBoxOracleUserPwd);
            this.GrBoxOracleConnData.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GrBoxOracleConnData.Name = "GrBoxOracleConnData";
            this.GrBoxOracleConnData.TabStop = false;
            // 
            // CbNTSecurity
            // 
            resources.ApplyResources(this.CbNTSecurity, "CbNTSecurity");
            this.CbNTSecurity.Name = "CbNTSecurity";
            this.CbNTSecurity.CheckedChanged += new System.EventHandler(this.CbNTSecurity_CheckedChanged);
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
            // rbSelectExistedOracleUser
            // 
            resources.ApplyResources(this.rbSelectExistedOracleUser, "rbSelectExistedOracleUser");
            this.rbSelectExistedOracleUser.Checked = true;
            this.rbSelectExistedOracleUser.Name = "rbSelectExistedOracleUser";
            this.rbSelectExistedOracleUser.TabStop = true;
            this.rbSelectExistedOracleUser.CheckedChanged += new System.EventHandler(this.rbSelectExistedOracleUser_CheckedChanged);
            // 
            // ComboOracleLogins
            // 
            resources.ApplyResources(this.ComboOracleLogins, "ComboOracleLogins");
            this.ComboOracleLogins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboOracleLogins.Name = "ComboOracleLogins";
            this.ComboOracleLogins.SelectedIndexChanged += new System.EventHandler(this.ComboOracleLogins_SelectedIndexChanged);
            this.ComboOracleLogins.DropDown += new System.EventHandler(this.ComboOracleLogins_DropDown);
            // 
            // BtnOracleConnectionCheck
            // 
            resources.ApplyResources(this.BtnOracleConnectionCheck, "BtnOracleConnectionCheck");
            this.BtnOracleConnectionCheck.Name = "BtnOracleConnectionCheck";
            this.BtnOracleConnectionCheck.Click += new System.EventHandler(this.BtnOracleConnectionCheck_Click);
            // 
            // LblOracleUserPwd
            // 
            resources.ApplyResources(this.LblOracleUserPwd, "LblOracleUserPwd");
            this.LblOracleUserPwd.Name = "LblOracleUserPwd";
            // 
            // TextBoxOracleUserPwd
            // 
            resources.ApplyResources(this.TextBoxOracleUserPwd, "TextBoxOracleUserPwd");
            this.TextBoxOracleUserPwd.Name = "TextBoxOracleUserPwd";
            // 
            // BtnSave
            // 
            resources.ApplyResources(this.BtnSave, "BtnSave");
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // GroupBoxActions
            // 
            resources.ApplyResources(this.GroupBoxActions, "GroupBoxActions");
            this.GroupBoxActions.Controls.Add(this.RbDeleteAll);
            this.GroupBoxActions.Controls.Add(this.RbDisableAll);
            this.GroupBoxActions.Controls.Add(this.RbEnableAll);
            this.GroupBoxActions.Controls.Add(this.RbModifyLogin);
            this.GroupBoxActions.Controls.Add(this.GrBoxOracleConnData);
            this.GroupBoxActions.Name = "GroupBoxActions";
            this.GroupBoxActions.TabStop = false;
            // 
            // ModifyCompanyUsersToOracleLogin
            // 
            this.AcceptButton = this.BtnSave;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.GroupBoxActions);
            this.Controls.Add(this.BtnSave);
            this.Controls.Add(this.LblExplication);
            this.Controls.Add(this.GroupDatabaseLogin);
            this.Controls.Add(this.LabelTitle);
            this.Name = "ModifyCompanyUsersToOracleLogin";
            this.ShowInTaskbar = false;
            this.Deactivate += new System.EventHandler(this.ModifyCompanyUsersToOracleLogin_Deactivate);
            this.VisibleChanged += new System.EventHandler(this.ModifyCompanyUsersToOracleLogin_VisibleChanged);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ModifyCompanyUsersToOracleLogin_Closing);
            this.Load += new System.EventHandler(this.ModifyCompanyUsersToOracleLogin_Load);
            this.GroupDatabaseLogin.ResumeLayout(false);
            this.GroupDatabaseLogin.PerformLayout();
            this.GrBoxOracleConnData.ResumeLayout(false);
            this.GrBoxOracleConnData.PerformLayout();
            this.GroupBoxActions.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
