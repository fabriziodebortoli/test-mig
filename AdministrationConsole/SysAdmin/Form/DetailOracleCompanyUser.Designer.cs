using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class DetailOracleCompanyUser
    {
        private Label LabelTitle;
        private Label LblExplication;
        private TextBox TbLoginName;
        private GroupBox GroupLoginSettings;
        private TextBox TbIfDbowner;
        private CheckBox CbDisable;
        private CheckBox CbAdmin;
        private GroupBox GroupsLoginConnection;
        private Label LblPassword;
        private Button BtnOracleConnectionCheck;
        private Button BtnModify;
        private TextBox TextBoxOracleService;
        private Label LblServiceOracle;
        private TextBox TbOracleUserPwd;
        private CheckBox CbChangeApplicationUser;
        private Label LblLogin;
        private ComboBox ComboOracleLogins;
        private RadioButton rbSelectExistedOracleUser;
        private RadioButton rbNewOracleUser;
        private CheckBox CbNTSecurity;
        private TextBox txtNewOracleUser;
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Dispose
        /// Clean up any resources being used.
        /// </summary>
        // --------------------------------------------------------------------
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailOracleCompanyUser));
			this.LabelTitle = new System.Windows.Forms.Label();
			this.LblExplication = new System.Windows.Forms.Label();
			this.LblLogin = new System.Windows.Forms.Label();
			this.TbLoginName = new System.Windows.Forms.TextBox();
			this.GroupLoginSettings = new System.Windows.Forms.GroupBox();
			this.EBDevCheckBox = new System.Windows.Forms.CheckBox();
			this.TbIfDbowner = new System.Windows.Forms.TextBox();
			this.CbDisable = new System.Windows.Forms.CheckBox();
			this.CbAdmin = new System.Windows.Forms.CheckBox();
			this.GroupsLoginConnection = new System.Windows.Forms.GroupBox();
			this.CbNTSecurity = new System.Windows.Forms.CheckBox();
			this.txtNewOracleUser = new System.Windows.Forms.TextBox();
			this.rbNewOracleUser = new System.Windows.Forms.RadioButton();
			this.rbSelectExistedOracleUser = new System.Windows.Forms.RadioButton();
			this.CbChangeApplicationUser = new System.Windows.Forms.CheckBox();
			this.LblServiceOracle = new System.Windows.Forms.Label();
			this.TextBoxOracleService = new System.Windows.Forms.TextBox();
			this.ComboOracleLogins = new System.Windows.Forms.ComboBox();
			this.LblPassword = new System.Windows.Forms.Label();
			this.TbOracleUserPwd = new System.Windows.Forms.TextBox();
			this.BtnOracleConnectionCheck = new System.Windows.Forms.Button();
			this.BtnModify = new System.Windows.Forms.Button();
			this.GroupLoginSettings.SuspendLayout();
			this.GroupsLoginConnection.SuspendLayout();
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
			this.LblExplication.BackColor = System.Drawing.SystemColors.Control;
			this.LblExplication.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblExplication.Name = "LblExplication";
			// 
			// LblLogin
			// 
			resources.ApplyResources(this.LblLogin, "LblLogin");
			this.LblLogin.Name = "LblLogin";
			// 
			// TbLoginName
			// 
			resources.ApplyResources(this.TbLoginName, "TbLoginName");
			this.TbLoginName.Name = "TbLoginName";
			this.TbLoginName.ReadOnly = true;
			// 
			// GroupLoginSettings
			// 
			resources.ApplyResources(this.GroupLoginSettings, "GroupLoginSettings");
			this.GroupLoginSettings.Controls.Add(this.EBDevCheckBox);
			this.GroupLoginSettings.Controls.Add(this.TbIfDbowner);
			this.GroupLoginSettings.Controls.Add(this.CbDisable);
			this.GroupLoginSettings.Controls.Add(this.CbAdmin);
			this.GroupLoginSettings.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GroupLoginSettings.Name = "GroupLoginSettings";
			this.GroupLoginSettings.TabStop = false;
			// 
			// EBDevCheckBox
			// 
			resources.ApplyResources(this.EBDevCheckBox, "EBDevCheckBox");
			this.EBDevCheckBox.Name = "EBDevCheckBox";
			this.EBDevCheckBox.CheckedChanged += new System.EventHandler(this.EBDevCheckBox_CheckedChanged);
			// 
			// TbIfDbowner
			// 
			resources.ApplyResources(this.TbIfDbowner, "TbIfDbowner");
			this.TbIfDbowner.Name = "TbIfDbowner";
			this.TbIfDbowner.ReadOnly = true;
			// 
			// CbDisable
			// 
			resources.ApplyResources(this.CbDisable, "CbDisable");
			this.CbDisable.Name = "CbDisable";
			this.CbDisable.CheckedChanged += new System.EventHandler(this.CbDisable_CheckedChanged);
			// 
			// CbAdmin
			// 
			resources.ApplyResources(this.CbAdmin, "CbAdmin");
			this.CbAdmin.Name = "CbAdmin";
			this.CbAdmin.CheckedChanged += new System.EventHandler(this.CbAdmin_CheckedChanged);
			// 
			// GroupsLoginConnection
			// 
			resources.ApplyResources(this.GroupsLoginConnection, "GroupsLoginConnection");
			this.GroupsLoginConnection.Controls.Add(this.CbNTSecurity);
			this.GroupsLoginConnection.Controls.Add(this.txtNewOracleUser);
			this.GroupsLoginConnection.Controls.Add(this.rbNewOracleUser);
			this.GroupsLoginConnection.Controls.Add(this.rbSelectExistedOracleUser);
			this.GroupsLoginConnection.Controls.Add(this.CbChangeApplicationUser);
			this.GroupsLoginConnection.Controls.Add(this.LblServiceOracle);
			this.GroupsLoginConnection.Controls.Add(this.TextBoxOracleService);
			this.GroupsLoginConnection.Controls.Add(this.ComboOracleLogins);
			this.GroupsLoginConnection.Controls.Add(this.LblPassword);
			this.GroupsLoginConnection.Controls.Add(this.TbOracleUserPwd);
			this.GroupsLoginConnection.Controls.Add(this.BtnOracleConnectionCheck);
			this.GroupsLoginConnection.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GroupsLoginConnection.Name = "GroupsLoginConnection";
			this.GroupsLoginConnection.TabStop = false;
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
			// CbChangeApplicationUser
			// 
			resources.ApplyResources(this.CbChangeApplicationUser, "CbChangeApplicationUser");
			this.CbChangeApplicationUser.Name = "CbChangeApplicationUser";
			this.CbChangeApplicationUser.CheckedChanged += new System.EventHandler(this.CbChangeApplicationUser_CheckedChanged);
			// 
			// LblServiceOracle
			// 
			resources.ApplyResources(this.LblServiceOracle, "LblServiceOracle");
			this.LblServiceOracle.Name = "LblServiceOracle";
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
			// LblPassword
			// 
			resources.ApplyResources(this.LblPassword, "LblPassword");
			this.LblPassword.Name = "LblPassword";
			// 
			// TbOracleUserPwd
			// 
			resources.ApplyResources(this.TbOracleUserPwd, "TbOracleUserPwd");
			this.TbOracleUserPwd.Name = "TbOracleUserPwd";
			this.TbOracleUserPwd.TextChanged += new System.EventHandler(this.TbOracleUserPwd_TextChanged);
			// 
			// BtnOracleConnectionCheck
			// 
			resources.ApplyResources(this.BtnOracleConnectionCheck, "BtnOracleConnectionCheck");
			this.BtnOracleConnectionCheck.Name = "BtnOracleConnectionCheck";
			this.BtnOracleConnectionCheck.Click += new System.EventHandler(this.BtnOracleConnectionCheck_Click);
			// 
			// BtnModify
			// 
			resources.ApplyResources(this.BtnModify, "BtnModify");
			this.BtnModify.Name = "BtnModify";
			this.BtnModify.Click += new System.EventHandler(this.BtnModify_Click);
			// 
			// DetailOracleCompanyUser
			// 
			this.AcceptButton = this.BtnModify;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.BtnModify);
			this.Controls.Add(this.GroupsLoginConnection);
			this.Controls.Add(this.GroupLoginSettings);
			this.Controls.Add(this.TbLoginName);
			this.Controls.Add(this.LblLogin);
			this.Controls.Add(this.LblExplication);
			this.Controls.Add(this.LabelTitle);
			this.Name = "DetailOracleCompanyUser";
			this.ShowInTaskbar = false;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DetailOracleCompanyUser_Closing);
			this.Deactivate += new System.EventHandler(this.DetailOracleCompanyUser_Deactivate);
			this.Load += new System.EventHandler(this.DetailOracleCompanyUser_Load);
			this.VisibleChanged += new System.EventHandler(this.DetailOracleCompanyUser_VisibleChanged);
			this.GroupLoginSettings.ResumeLayout(false);
			this.GroupLoginSettings.PerformLayout();
			this.GroupsLoginConnection.ResumeLayout(false);
			this.GroupsLoginConnection.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

		private CheckBox EBDevCheckBox;
    }
}
