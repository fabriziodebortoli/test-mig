using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class DetailCompanyUserLite
    {
        private Label LabelTitle;
        private TextBox TbIfDbowner;
        private Label LblExplication;
        private Label lblLogin;
        private TextBox TbLoginName;
        private GroupBox GroupLoginSettings;
        private CheckBox CbAdmin;
        private CheckBox CbDisable;
        private GroupBox GroupsLoginConnection;
        private TextBox TbPassword;
        private Label LblPassword;
        private Button BtnModify;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        //--------------------------------------------------------------------
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailCompanyUserLite));
			this.LabelTitle = new System.Windows.Forms.Label();
			this.lblLogin = new System.Windows.Forms.Label();
			this.TbLoginName = new System.Windows.Forms.TextBox();
			this.GroupLoginSettings = new System.Windows.Forms.GroupBox();
			this.EBDevCheckBox = new System.Windows.Forms.CheckBox();
			this.TbIfDbowner = new System.Windows.Forms.TextBox();
			this.CbDisable = new System.Windows.Forms.CheckBox();
			this.CbAdmin = new System.Windows.Forms.CheckBox();
			this.GroupsLoginConnection = new System.Windows.Forms.GroupBox();
			this.TbPassword = new System.Windows.Forms.TextBox();
			this.LblPassword = new System.Windows.Forms.Label();
			this.BtnModify = new System.Windows.Forms.Button();
			this.LblExplication = new System.Windows.Forms.Label();
			this.LbLogin = new System.Windows.Forms.Label();
			this.TxtLogin = new System.Windows.Forms.TextBox();
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
			// lblLogin
			// 
			resources.ApplyResources(this.lblLogin, "lblLogin");
			this.lblLogin.Name = "lblLogin";
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
			this.GroupsLoginConnection.Controls.Add(this.TxtLogin);
			this.GroupsLoginConnection.Controls.Add(this.LbLogin);
			this.GroupsLoginConnection.Controls.Add(this.TbPassword);
			this.GroupsLoginConnection.Controls.Add(this.LblPassword);
			this.GroupsLoginConnection.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GroupsLoginConnection.Name = "GroupsLoginConnection";
			this.GroupsLoginConnection.TabStop = false;
			// 
			// TbPassword
			// 
			resources.ApplyResources(this.TbPassword, "TbPassword");
			this.TbPassword.Name = "TbPassword";
			this.TbPassword.TextChanged += new System.EventHandler(this.TbPassword_TextChanged);
			// 
			// LblPassword
			// 
			resources.ApplyResources(this.LblPassword, "LblPassword");
			this.LblPassword.Name = "LblPassword";
			// 
			// BtnModify
			// 
			resources.ApplyResources(this.BtnModify, "BtnModify");
			this.BtnModify.Name = "BtnModify";
			this.BtnModify.Click += new System.EventHandler(this.BtnModify_Click);
			// 
			// LblExplication
			// 
			resources.ApplyResources(this.LblExplication, "LblExplication");
			this.LblExplication.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblExplication.Name = "LblExplication";
			// 
			// LbLogin
			// 
			resources.ApplyResources(this.LbLogin, "LbLogin");
			this.LbLogin.Name = "LbLogin";
			// 
			// TxtLogin
			// 
			resources.ApplyResources(this.TxtLogin, "TxtLogin");
			this.TxtLogin.Name = "TxtLogin";
			this.TxtLogin.TextChanged += new System.EventHandler(this.TxtLogin_TextChanged);
			// 
			// DetailCompanyUserLite
			// 
			this.AcceptButton = this.BtnModify;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.LblExplication);
			this.Controls.Add(this.GroupsLoginConnection);
			this.Controls.Add(this.GroupLoginSettings);
			this.Controls.Add(this.TbLoginName);
			this.Controls.Add(this.lblLogin);
			this.Controls.Add(this.LabelTitle);
			this.Controls.Add(this.BtnModify);
			this.Name = "DetailCompanyUserLite";
			this.ShowInTaskbar = false;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DetailCompanyUserLite_Closing);
			this.Deactivate += new System.EventHandler(this.DetailCompanyUserLite_Deactivate);
			this.Load += new System.EventHandler(this.DetailCompanyUserLite_Load);
			this.VisibleChanged += new System.EventHandler(this.DetailCompanyUserLite_VisibleChanged);
			this.GroupLoginSettings.ResumeLayout(false);
			this.GroupLoginSettings.PerformLayout();
			this.GroupsLoginConnection.ResumeLayout(false);
			this.GroupsLoginConnection.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

		private CheckBox EBDevCheckBox;
		private Label LbLogin;
		private TextBox TxtLogin;
	}
}
