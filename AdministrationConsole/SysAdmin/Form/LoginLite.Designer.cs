using System.ComponentModel;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.DatabaseWinControls;


namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class LoginLite
    {
        private Button BtnOk;
        private Button BtnCancel;

        private TextBox TxtPassword;
        private TextBox TxtLogin;
        private Label LblPassword;
        private Label LblLogin;
        private Label LblServer;
        private GroupBox GbConnection;
        private PictureBox LogoPictureBox;
        private Label AdminConsoleLabel;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginLite));
            this.BtnOk = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.TxtPassword = new System.Windows.Forms.TextBox();
            this.TxtLogin = new System.Windows.Forms.TextBox();
            this.LblPassword = new System.Windows.Forms.Label();
            this.LblLogin = new System.Windows.Forms.Label();
            this.LblServer = new System.Windows.Forms.Label();
            this.GbConnection = new System.Windows.Forms.GroupBox();
            this.LblDatabase = new System.Windows.Forms.Label();
            this.TxtDatabase = new System.Windows.Forms.TextBox();
            this.TxtServerName = new System.Windows.Forms.TextBox();
            this.LogoPictureBox = new System.Windows.Forms.PictureBox();
            this.AdminConsoleLabel = new System.Windows.Forms.Label();
            this.GbConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnOk
            // 
            resources.ApplyResources(this.BtnOk, "BtnOk");
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // BtnCancel
            // 
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // TxtPassword
            // 
            resources.ApplyResources(this.TxtPassword, "TxtPassword");
            this.TxtPassword.Name = "TxtPassword";
            // 
            // TxtLogin
            // 
            resources.ApplyResources(this.TxtLogin, "TxtLogin");
            this.TxtLogin.Name = "TxtLogin";
            // 
            // LblPassword
            // 
            resources.ApplyResources(this.LblPassword, "LblPassword");
            this.LblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblPassword.Name = "LblPassword";
            // 
            // LblLogin
            // 
            resources.ApplyResources(this.LblLogin, "LblLogin");
            this.LblLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblLogin.Name = "LblLogin";
            // 
            // LblServer
            // 
            this.LblServer.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.LblServer, "LblServer");
            this.LblServer.Name = "LblServer";
            // 
            // GbConnection
            // 
            resources.ApplyResources(this.GbConnection, "GbConnection");
            this.GbConnection.Controls.Add(this.LblServer);
            this.GbConnection.Controls.Add(this.LblLogin);
            this.GbConnection.Controls.Add(this.LblDatabase);
            this.GbConnection.Controls.Add(this.LblPassword);
            this.GbConnection.Controls.Add(this.TxtDatabase);
            this.GbConnection.Controls.Add(this.TxtServerName);
            this.GbConnection.Controls.Add(this.TxtLogin);
            this.GbConnection.Controls.Add(this.TxtPassword);
            this.GbConnection.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GbConnection.Name = "GbConnection";
            this.GbConnection.TabStop = false;
            // 
            // LblDatabase
            // 
            resources.ApplyResources(this.LblDatabase, "LblDatabase");
            this.LblDatabase.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblDatabase.Name = "LblDatabase";
            // 
            // TxtDatabase
            // 
            resources.ApplyResources(this.TxtDatabase, "TxtDatabase");
            this.TxtDatabase.Name = "TxtDatabase";
            // 
            // TxtServerName
            // 
            resources.ApplyResources(this.TxtServerName, "TxtServerName");
            this.TxtServerName.Name = "TxtServerName";
            // 
            // LogoPictureBox
            // 
            resources.ApplyResources(this.LogoPictureBox, "LogoPictureBox");
            this.LogoPictureBox.Name = "LogoPictureBox";
            this.LogoPictureBox.TabStop = false;
            // 
            // AdminConsoleLabel
            // 
            resources.ApplyResources(this.AdminConsoleLabel, "AdminConsoleLabel");
            this.AdminConsoleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.AdminConsoleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(207)))), ((int)(((byte)(131)))));
            this.AdminConsoleLabel.Name = "AdminConsoleLabel";
            // 
            // LoginLite
            // 
            this.AcceptButton = this.BtnOk;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.BtnCancel;
            this.Controls.Add(this.AdminConsoleLabel);
            this.Controls.Add(this.LogoPictureBox);
            this.Controls.Add(this.GbConnection);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOk);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginLite";
            this.ShowInTaskbar = false;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Login_Closing);
            this.Load += new System.EventHandler(this.Login_Load);
            this.GbConnection.ResumeLayout(false);
            this.GbConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private Label LblDatabase;
        private TextBox TxtDatabase;
        private TextBox TxtServerName;
    }
}
