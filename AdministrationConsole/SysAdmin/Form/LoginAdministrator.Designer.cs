using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class LoginAdministrator
    {
        private Label LblExplication;
        private Label LabelTitle;
        private GroupBox GroupsChangeLogin;
        private TextBox TbPassword;
        private Label LblPassword;
        private ComboBox ComboExistLogins;
        private Button BtnSave;
        private RadioButton RbChangePassword;
        private TextBox TbNewPassword;

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton RbDeleteLogin;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginAdministrator));
            this.LblExplication = new System.Windows.Forms.Label();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.GroupsChangeLogin = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TbPassword = new System.Windows.Forms.TextBox();
            this.LblPassword = new System.Windows.Forms.Label();
            this.ComboExistLogins = new System.Windows.Forms.ComboBox();
            this.BtnSave = new System.Windows.Forms.Button();
            this.RbChangePassword = new System.Windows.Forms.RadioButton();
            this.TbNewPassword = new System.Windows.Forms.TextBox();
            this.RbDeleteLogin = new System.Windows.Forms.RadioButton();
            this.GroupsChangeLogin.SuspendLayout();
            this.SuspendLayout();
            // 
            // LblExplication
            // 
            resources.ApplyResources(this.LblExplication, "LblExplication");
            this.LblExplication.Name = "LblExplication";
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
            // GroupsChangeLogin
            // 
            resources.ApplyResources(this.GroupsChangeLogin, "GroupsChangeLogin");
            this.GroupsChangeLogin.Controls.Add(this.label1);
            this.GroupsChangeLogin.Controls.Add(this.TbPassword);
            this.GroupsChangeLogin.Controls.Add(this.LblPassword);
            this.GroupsChangeLogin.Controls.Add(this.ComboExistLogins);
            this.GroupsChangeLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GroupsChangeLogin.Name = "GroupsChangeLogin";
            this.GroupsChangeLogin.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
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
            // ComboExistLogins
            // 
            this.ComboExistLogins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.ComboExistLogins, "ComboExistLogins");
            this.ComboExistLogins.Name = "ComboExistLogins";
            this.ComboExistLogins.SelectedIndexChanged += new System.EventHandler(this.ComboExistLogins_SelectedIndexChanged);
            this.ComboExistLogins.DropDown += new System.EventHandler(this.ComboExistLogins_DropDown);
            // 
            // BtnSave
            // 
            resources.ApplyResources(this.BtnSave, "BtnSave");
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // RbChangePassword
            // 
            resources.ApplyResources(this.RbChangePassword, "RbChangePassword");
            this.RbChangePassword.Name = "RbChangePassword";
            this.RbChangePassword.CheckedChanged += new System.EventHandler(this.RbChangePassword_CheckedChanged);
            // 
            // TbNewPassword
            // 
            resources.ApplyResources(this.TbNewPassword, "TbNewPassword");
            this.TbNewPassword.Name = "TbNewPassword";
            // 
            // RbDeleteLogin
            // 
            resources.ApplyResources(this.RbDeleteLogin, "RbDeleteLogin");
            this.RbDeleteLogin.Name = "RbDeleteLogin";
            this.RbDeleteLogin.CheckedChanged += new System.EventHandler(this.RbDeleteLogin_CheckedChanged);
            // 
            // LoginAdministrator
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.RbDeleteLogin);
            this.Controls.Add(this.TbNewPassword);
            this.Controls.Add(this.GroupsChangeLogin);
            this.Controls.Add(this.LblExplication);
            this.Controls.Add(this.LabelTitle);
            this.Controls.Add(this.BtnSave);
            this.Controls.Add(this.RbChangePassword);
            this.Name = "LoginAdministrator";
            this.ShowInTaskbar = false;
            this.Deactivate += new System.EventHandler(this.LoginAdministrator_Deactivate);
            this.VisibleChanged += new System.EventHandler(this.LoginAdministrator_VisibleChanged);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.LoginAdministrator_Closing);
            this.Load += new System.EventHandler(this.LoginAdministrator_Load);
            this.GroupsChangeLogin.ResumeLayout(false);
            this.GroupsChangeLogin.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
