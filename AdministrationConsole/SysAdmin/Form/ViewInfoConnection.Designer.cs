using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class ViewInfoConnection
    {
        private GroupBox GBoxConnection;
        private Label LblDataBaseName;
        private TextBox TBDataBaseName;
        private Label LblLogin;
        private Label LblPassword;
        private TextBox TbLogin;
        private TextBox TbPassword;
        private Label LabelTitle;
        private Label LblExplication;
        private TextBox TbServerName;
        private Label LblServer;
        private Container components = null;
        private System.Windows.Forms.Label label1;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewInfoConnection));
			this.GBoxConnection = new System.Windows.Forms.GroupBox();
			this.TbPassword = new System.Windows.Forms.TextBox();
			this.TbLogin = new System.Windows.Forms.TextBox();
			this.LblPassword = new System.Windows.Forms.Label();
			this.LblLogin = new System.Windows.Forms.Label();
			this.LblDataBaseName = new System.Windows.Forms.Label();
			this.TBDataBaseName = new System.Windows.Forms.TextBox();
			this.LabelTitle = new System.Windows.Forms.Label();
			this.LblExplication = new System.Windows.Forms.Label();
			this.TbServerName = new System.Windows.Forms.TextBox();
			this.LblServer = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.GBoxConnection.SuspendLayout();
			this.SuspendLayout();
			// 
			// GBoxConnection
			// 
			resources.ApplyResources(this.GBoxConnection, "GBoxConnection");
			this.GBoxConnection.BackColor = System.Drawing.SystemColors.Control;
			this.GBoxConnection.Controls.Add(this.TbPassword);
			this.GBoxConnection.Controls.Add(this.TbLogin);
			this.GBoxConnection.Controls.Add(this.LblPassword);
			this.GBoxConnection.Controls.Add(this.LblLogin);
			this.GBoxConnection.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GBoxConnection.ForeColor = System.Drawing.SystemColors.ControlText;
			this.GBoxConnection.Name = "GBoxConnection";
			this.GBoxConnection.TabStop = false;
			// 
			// TbPassword
			// 
			resources.ApplyResources(this.TbPassword, "TbPassword");
			this.TbPassword.Name = "TbPassword";
			this.TbPassword.ReadOnly = true;
			// 
			// TbLogin
			// 
			resources.ApplyResources(this.TbLogin, "TbLogin");
			this.TbLogin.Name = "TbLogin";
			this.TbLogin.ReadOnly = true;
			// 
			// LblPassword
			// 
			resources.ApplyResources(this.LblPassword, "LblPassword");
			this.LblPassword.Name = "LblPassword";
			// 
			// LblLogin
			// 
			resources.ApplyResources(this.LblLogin, "LblLogin");
			this.LblLogin.Name = "LblLogin";
			// 
			// LblDataBaseName
			// 
			resources.ApplyResources(this.LblDataBaseName, "LblDataBaseName");
			this.LblDataBaseName.BackColor = System.Drawing.SystemColors.Control;
			this.LblDataBaseName.ForeColor = System.Drawing.SystemColors.ControlText;
			this.LblDataBaseName.Name = "LblDataBaseName";
			// 
			// TBDataBaseName
			// 
			resources.ApplyResources(this.TBDataBaseName, "TBDataBaseName");
			this.TBDataBaseName.Name = "TBDataBaseName";
			this.TBDataBaseName.ReadOnly = true;
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
			this.LblExplication.ForeColor = System.Drawing.SystemColors.ControlText;
			this.LblExplication.Name = "LblExplication";
			// 
			// TbServerName
			// 
			resources.ApplyResources(this.TbServerName, "TbServerName");
			this.TbServerName.Name = "TbServerName";
			this.TbServerName.ReadOnly = true;
			// 
			// LblServer
			// 
			resources.ApplyResources(this.LblServer, "LblServer");
			this.LblServer.BackColor = System.Drawing.SystemColors.Control;
			this.LblServer.ForeColor = System.Drawing.SystemColors.ControlText;
			this.LblServer.Name = "LblServer";
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.BackColor = System.Drawing.SystemColors.Control;
			this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label1.Name = "label1";
			// 
			// ViewInfoConnection
			// 
			resources.ApplyResources(this, "$this");
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.LblServer);
			this.Controls.Add(this.TbServerName);
			this.Controls.Add(this.TBDataBaseName);
			this.Controls.Add(this.LblDataBaseName);
			this.Controls.Add(this.LblExplication);
			this.Controls.Add(this.LabelTitle);
			this.Controls.Add(this.GBoxConnection);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "ViewInfoConnection";
			this.ShowInTaskbar = false;
			this.TopMost = true;
			this.Deactivate += new System.EventHandler(this.ViewInfoConnection_Deactivate);
			this.VisibleChanged += new System.EventHandler(this.ViewInfoConnection_VisibleChanged);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ViewInfoConnection_Closing);
			this.GBoxConnection.ResumeLayout(false);
			this.GBoxConnection.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion
    }
}
