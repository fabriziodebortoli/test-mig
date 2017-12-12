using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class ModifyCompanyUsersToLoginPostgre
    {
        private Label LabelTitle;
        private Label LblExplication;
        private GroupBox GroupDatabaseLogin;
        private ComboBox CbLogins;
        private ListView ListViewUsersCompany;
        private Label LblInfoUsers;
        private RadioButton RbDeleteAll;
        private RadioButton RbDisableAll;
        private RadioButton RbModifyLogin;
        private RadioButton RbSelectExistLogin;
        private RadioButton RbCreateNewLogin;
        private Button BtnSave;
        private ComboBox ComboExistLogins;
        private TextBox TbNewLoginName;
        private TextBox TbPassword;
        private GroupBox GroupsChangeLogin;
        private Label LblPassword;
        private RadioButton RbEnableAll;
        private System.Windows.Forms.Label RbDeleteAllCommentLabel;
        private System.Windows.Forms.Label RbDisableAllCommentLabel;
        private System.Windows.Forms.Label RbEnableAllCommentLabel;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyCompanyUsersToLogin));
            this.LabelTitle = new System.Windows.Forms.Label();
            this.LblExplication = new System.Windows.Forms.Label();
            this.GroupDatabaseLogin = new System.Windows.Forms.GroupBox();
            this.LblInfoUsers = new System.Windows.Forms.Label();
            this.ListViewUsersCompany = new System.Windows.Forms.ListView();
            this.CbLogins = new System.Windows.Forms.ComboBox();
            this.RbDeleteAll = new System.Windows.Forms.RadioButton();
            this.RbDisableAll = new System.Windows.Forms.RadioButton();
            this.RbModifyLogin = new System.Windows.Forms.RadioButton();
            this.GroupsChangeLogin = new System.Windows.Forms.GroupBox();
            this.TbNewLoginName = new System.Windows.Forms.TextBox();
            this.TbPassword = new System.Windows.Forms.TextBox();
            this.LblPassword = new System.Windows.Forms.Label();
            this.RbCreateNewLogin = new System.Windows.Forms.RadioButton();
            this.ComboExistLogins = new System.Windows.Forms.ComboBox();
            this.RbSelectExistLogin = new System.Windows.Forms.RadioButton();
            this.BtnSave = new System.Windows.Forms.Button();
            this.RbEnableAll = new System.Windows.Forms.RadioButton();
            this.RbDeleteAllCommentLabel = new System.Windows.Forms.Label();
            this.RbDisableAllCommentLabel = new System.Windows.Forms.Label();
            this.RbEnableAllCommentLabel = new System.Windows.Forms.Label();
            this.GroupDatabaseLogin.SuspendLayout();
            this.GroupsChangeLogin.SuspendLayout();
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
            // GroupDatabaseLogin
            // 
            resources.ApplyResources(this.GroupDatabaseLogin, "GroupDatabaseLogin");
            this.GroupDatabaseLogin.Controls.Add(this.LblInfoUsers);
            this.GroupDatabaseLogin.Controls.Add(this.ListViewUsersCompany);
            this.GroupDatabaseLogin.Controls.Add(this.CbLogins);
            this.GroupDatabaseLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GroupDatabaseLogin.Name = "GroupDatabaseLogin";
            this.GroupDatabaseLogin.TabStop = false;
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
            // CbLogins
            // 
            this.CbLogins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.CbLogins, "CbLogins");
            this.CbLogins.Name = "CbLogins";
            this.CbLogins.SelectedIndexChanged += new System.EventHandler(this.CbLogins_SelectedIndexChanged);
            this.CbLogins.DropDown += new System.EventHandler(this.CbLogins_DropDown);
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
            // RbModifyLogin
            // 
            resources.ApplyResources(this.RbModifyLogin, "RbModifyLogin");
            this.RbModifyLogin.Checked = true;
            this.RbModifyLogin.Name = "RbModifyLogin";
            this.RbModifyLogin.TabStop = true;
            this.RbModifyLogin.CheckedChanged += new System.EventHandler(this.RbModifyLogin_CheckedChanged);
            // 
            // GroupsChangeLogin
            // 
            resources.ApplyResources(this.GroupsChangeLogin, "GroupsChangeLogin");
            this.GroupsChangeLogin.Controls.Add(this.TbNewLoginName);
            this.GroupsChangeLogin.Controls.Add(this.TbPassword);
            this.GroupsChangeLogin.Controls.Add(this.LblPassword);
            this.GroupsChangeLogin.Controls.Add(this.RbCreateNewLogin);
            this.GroupsChangeLogin.Controls.Add(this.ComboExistLogins);
            this.GroupsChangeLogin.Controls.Add(this.RbSelectExistLogin);
            this.GroupsChangeLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GroupsChangeLogin.Name = "GroupsChangeLogin";
            this.GroupsChangeLogin.TabStop = false;
            // 
            // TbNewLoginName
            // 
            resources.ApplyResources(this.TbNewLoginName, "TbNewLoginName");
            this.TbNewLoginName.Name = "TbNewLoginName";
            // 
            // TbPassword
            // 
            resources.ApplyResources(this.TbPassword, "TbPassword");
            this.TbPassword.Name = "TbPassword";
            // 
            // LblPassword
            // 
            resources.ApplyResources(this.LblPassword, "LblPassword");
            this.LblPassword.Name = "LblPassword";
            // 
            // RbCreateNewLogin
            // 
            resources.ApplyResources(this.RbCreateNewLogin, "RbCreateNewLogin");
            this.RbCreateNewLogin.Name = "RbCreateNewLogin";
            this.RbCreateNewLogin.CheckedChanged += new System.EventHandler(this.RbCreateNewLogin_CheckedChanged);
            // 
            // ComboExistLogins
            // 
            this.ComboExistLogins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.ComboExistLogins, "ComboExistLogins");
            this.ComboExistLogins.Name = "ComboExistLogins";
            this.ComboExistLogins.SelectedIndexChanged += new System.EventHandler(this.ComboExistLogins_SelectedIndexChanged);
            this.ComboExistLogins.DropDown += new System.EventHandler(this.ComboExistLogins_DropDown);
            // 
            // RbSelectExistLogin
            // 
            resources.ApplyResources(this.RbSelectExistLogin, "RbSelectExistLogin");
            this.RbSelectExistLogin.Checked = true;
            this.RbSelectExistLogin.Name = "RbSelectExistLogin";
            this.RbSelectExistLogin.TabStop = true;
            // 
            // BtnSave
            // 
            resources.ApplyResources(this.BtnSave, "BtnSave");
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // RbEnableAll
            // 
            resources.ApplyResources(this.RbEnableAll, "RbEnableAll");
            this.RbEnableAll.Name = "RbEnableAll";
            this.RbEnableAll.CheckedChanged += new System.EventHandler(this.RbEnableAll_CheckedChanged);
            // 
            // RbDeleteAllCommentLabel
            // 
            resources.ApplyResources(this.RbDeleteAllCommentLabel, "RbDeleteAllCommentLabel");
            this.RbDeleteAllCommentLabel.Name = "RbDeleteAllCommentLabel";
            // 
            // RbDisableAllCommentLabel
            // 
            resources.ApplyResources(this.RbDisableAllCommentLabel, "RbDisableAllCommentLabel");
            this.RbDisableAllCommentLabel.Name = "RbDisableAllCommentLabel";
            // 
            // RbEnableAllCommentLabel
            // 
            resources.ApplyResources(this.RbEnableAllCommentLabel, "RbEnableAllCommentLabel");
            this.RbEnableAllCommentLabel.Name = "RbEnableAllCommentLabel";
            // 
            // ModifyCompanyUsersToLogin
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.LblExplication);
            this.Controls.Add(this.RbModifyLogin);
            this.Controls.Add(this.RbEnableAllCommentLabel);
            this.Controls.Add(this.RbDisableAllCommentLabel);
            this.Controls.Add(this.RbDeleteAllCommentLabel);
            this.Controls.Add(this.RbEnableAll);
            this.Controls.Add(this.GroupsChangeLogin);
            this.Controls.Add(this.RbDisableAll);
            this.Controls.Add(this.GroupDatabaseLogin);
            this.Controls.Add(this.LabelTitle);
            this.Controls.Add(this.RbDeleteAll);
            this.Controls.Add(this.BtnSave);
            this.Name = "ModifyCompanyUsersToLogin";
            this.ShowInTaskbar = false;
            this.Deactivate += new System.EventHandler(this.ModifyCompanyUsersToLogin_Deactivate);
            this.VisibleChanged += new System.EventHandler(this.ModifyCompanyUsersToLogin_VisibleChanged);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ModifyCompanyUsersToLogin_Closing);
            this.GroupDatabaseLogin.ResumeLayout(false);
            this.GroupDatabaseLogin.PerformLayout();
            this.GroupsChangeLogin.ResumeLayout(false);
            this.GroupsChangeLogin.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
