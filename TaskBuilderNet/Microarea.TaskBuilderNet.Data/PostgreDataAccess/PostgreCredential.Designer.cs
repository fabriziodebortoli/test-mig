using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Data.PostgreDataAccess
{
    partial class PostgreCredential
    {
        private Label LblLogin;
        private TextBox TbLogin;
        private Label LblPassword;
        private Button BtnOK;
        private Button BtnCancel;
        private TextBox TbPassword;
        private RadioButton RadioSQLServerAuthentication;
        private RadioButton RadioWindowsAuthentication;
        private Label LblDomain;
        private ComboBox ComboDomains;
        private GroupBox groupBoxTypeCredential;
        private RadioButton radioNewCredential;
        private RadioButton radioCurrentCredential;
        private GroupBox groupBoxTypeOfAuthentication;
        private PictureBox imageLogin;
        private ToolTip toolTip;
        private System.Windows.Forms.Button BtnHelp;
        private IContainer components;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PostgreCredential));
            this.LblLogin = new System.Windows.Forms.Label();
            this.TbLogin = new System.Windows.Forms.TextBox();
            this.LblPassword = new System.Windows.Forms.Label();
            this.TbPassword = new System.Windows.Forms.TextBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.RadioSQLServerAuthentication = new System.Windows.Forms.RadioButton();
            this.RadioWindowsAuthentication = new System.Windows.Forms.RadioButton();
            this.LblDomain = new System.Windows.Forms.Label();
            this.ComboDomains = new System.Windows.Forms.ComboBox();
            this.groupBoxTypeCredential = new System.Windows.Forms.GroupBox();
            this.imageLogin = new System.Windows.Forms.PictureBox();
            this.groupBoxTypeOfAuthentication = new System.Windows.Forms.GroupBox();
            this.radioNewCredential = new System.Windows.Forms.RadioButton();
            this.radioCurrentCredential = new System.Windows.Forms.RadioButton();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.BtnHelp = new System.Windows.Forms.Button();
            this.groupBoxTypeCredential.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageLogin)).BeginInit();
            this.groupBoxTypeOfAuthentication.SuspendLayout();
            this.SuspendLayout();
            // 
            // LblLogin
            // 
            this.LblLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.LblLogin, "LblLogin");
            this.LblLogin.Name = "LblLogin";
            // 
            // TbLogin
            // 
            resources.ApplyResources(this.TbLogin, "TbLogin");
            this.TbLogin.Name = "TbLogin";
            this.TbLogin.TextChanged += new System.EventHandler(this.TbLogin_TextChanged);
            // 
            // LblPassword
            // 
            this.LblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.LblPassword, "LblPassword");
            this.LblPassword.Name = "LblPassword";
            // 
            // TbPassword
            // 
            this.TbPassword.AcceptsReturn = true;
            resources.ApplyResources(this.TbPassword, "TbPassword");
            this.TbPassword.Name = "TbPassword";
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // RadioSQLServerAuthentication
            // 
            resources.ApplyResources(this.RadioSQLServerAuthentication, "RadioSQLServerAuthentication");
            this.RadioSQLServerAuthentication.Name = "RadioSQLServerAuthentication";
            this.RadioSQLServerAuthentication.TabStop = true;
            this.RadioSQLServerAuthentication.CheckedChanged += new System.EventHandler(this.RadioSQLServerAuthentication_CheckedChanged);
            // 
            // RadioWindowsAuthentication
            // 
            resources.ApplyResources(this.RadioWindowsAuthentication, "RadioWindowsAuthentication");
            this.RadioWindowsAuthentication.Name = "RadioWindowsAuthentication";
            this.RadioWindowsAuthentication.TabStop = true;
            this.RadioWindowsAuthentication.CheckedChanged += new System.EventHandler(this.RadioWindowsAuthentication_CheckedChanged);
            // 
            // LblDomain
            // 
            this.LblDomain.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.LblDomain, "LblDomain");
            this.LblDomain.Name = "LblDomain";
            // 
            // ComboDomains
            // 
            resources.ApplyResources(this.ComboDomains, "ComboDomains");
            this.ComboDomains.Name = "ComboDomains";
            this.ComboDomains.DropDown += new System.EventHandler(this.ComboDomains_DropDown);
            // 
            // groupBoxTypeCredential
            // 
            this.groupBoxTypeCredential.Controls.Add(this.imageLogin);
            this.groupBoxTypeCredential.Controls.Add(this.groupBoxTypeOfAuthentication);
            this.groupBoxTypeCredential.Controls.Add(this.radioNewCredential);
            this.groupBoxTypeCredential.Controls.Add(this.radioCurrentCredential);
            this.groupBoxTypeCredential.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.groupBoxTypeCredential, "groupBoxTypeCredential");
            this.groupBoxTypeCredential.Name = "groupBoxTypeCredential";
            this.groupBoxTypeCredential.TabStop = false;
            // 
            // imageLogin
            // 
            resources.ApplyResources(this.imageLogin, "imageLogin");
            this.imageLogin.Name = "imageLogin";
            this.imageLogin.TabStop = false;
            // 
            // groupBoxTypeOfAuthentication
            // 
            this.groupBoxTypeOfAuthentication.Controls.Add(this.RadioWindowsAuthentication);
            this.groupBoxTypeOfAuthentication.Controls.Add(this.RadioSQLServerAuthentication);
            this.groupBoxTypeOfAuthentication.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.groupBoxTypeOfAuthentication, "groupBoxTypeOfAuthentication");
            this.groupBoxTypeOfAuthentication.Name = "groupBoxTypeOfAuthentication";
            this.groupBoxTypeOfAuthentication.TabStop = false;
            // 
            // radioNewCredential
            // 
            resources.ApplyResources(this.radioNewCredential, "radioNewCredential");
            this.radioNewCredential.Name = "radioNewCredential";
            this.radioNewCredential.CheckedChanged += new System.EventHandler(this.radioNewCredential_CheckedChanged);
            // 
            // radioCurrentCredential
            // 
            this.radioCurrentCredential.Checked = true;
            resources.ApplyResources(this.radioCurrentCredential, "radioCurrentCredential");
            this.radioCurrentCredential.Name = "radioCurrentCredential";
            this.radioCurrentCredential.TabStop = true;
            this.radioCurrentCredential.CheckedChanged += new System.EventHandler(this.radioCurrentCredential_CheckedChanged);
            // 
            // toolTip
            // 
            this.toolTip.ShowAlways = true;
            // 
            // BtnHelp
            // 
            resources.ApplyResources(this.BtnHelp, "BtnHelp");
            this.BtnHelp.Name = "BtnHelp";
            this.BtnHelp.Click += new System.EventHandler(this.BtnHelp_Click);
            // 
            // PostgreCredential
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.BtnHelp);
            this.Controls.Add(this.groupBoxTypeCredential);
            this.Controls.Add(this.ComboDomains);
            this.Controls.Add(this.LblDomain);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.TbPassword);
            this.Controls.Add(this.TbLogin);
            this.Controls.Add(this.LblPassword);
            this.Controls.Add(this.LblLogin);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PostgreCredential";
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Credential_Closing);
            this.groupBoxTypeCredential.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imageLogin)).EndInit();
            this.groupBoxTypeOfAuthentication.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
