using System.ComponentModel;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.DatabaseWinControls;


namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class Login
    {
        private TextBox txtNewDataBaseName;
        private Button cmdOK;
		private Button cmdCancel;
        private GroupBox groupBox2;
        private RadioButton rbSelectExistedDb;
        private RadioButton rbNewDb;
		private ToolTip toolTip;
        private IContainer components;

		private TextBox txtPassword;
		private TextBox txtName;
		private Label lblPassword;
		private Label lblName;
		private Label lblPhase1;
		private GroupBox groupBox1;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
			this.cmdOK = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.DatabasesComboBox = new Microarea.TaskBuilderNet.Data.DatabaseWinControls.DatabasesCombo(this.components);
			this.txtNewDataBaseName = new System.Windows.Forms.TextBox();
			this.rbNewDb = new System.Windows.Forms.RadioButton();
			this.rbSelectExistedDb = new System.Windows.Forms.RadioButton();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.txtName = new System.Windows.Forms.TextBox();
			this.lblPassword = new System.Windows.Forms.Label();
			this.lblName = new System.Windows.Forms.Label();
			this.lblPhase1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.NGSqlServersCombo = new Microarea.TaskBuilderNet.Data.DatabaseWinControls.NGSQLServersCombo(this.components);
			this.LogoPictureBox = new System.Windows.Forms.PictureBox();
			this.AdminConsoleLabel = new System.Windows.Forms.Label();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// cmdOK
			// 
			resources.ApplyResources(this.cmdOK, "cmdOK");
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// cmdCancel
			// 
			resources.ApplyResources(this.cmdCancel, "cmdCancel");
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// groupBox2
			// 
			resources.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Controls.Add(this.DatabasesComboBox);
			this.groupBox2.Controls.Add(this.txtNewDataBaseName);
			this.groupBox2.Controls.Add(this.rbNewDb);
			this.groupBox2.Controls.Add(this.rbSelectExistedDb);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			// 
			// DatabasesComboBox
			// 
			resources.ApplyResources(this.DatabasesComboBox, "DatabasesComboBox");
			this.DatabasesComboBox.DataSourceName = "";
			this.DatabasesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.DatabasesComboBox.FormattingEnabled = true;
			this.DatabasesComboBox.IsWindowsAuthentication = false;
			this.DatabasesComboBox.Name = "DatabasesComboBox";
			this.DatabasesComboBox.PortNumber = 0;
			this.DatabasesComboBox.ProviderType = Microarea.TaskBuilderNet.Interfaces.DBMSType.SQLSERVER;
			this.DatabasesComboBox.ServerName = "";
			this.DatabasesComboBox.Sorted = true;
			this.DatabasesComboBox.UserName = "";
			this.DatabasesComboBox.UserPassword = "";
			this.DatabasesComboBox.OnDropDownDatabases += new Microarea.TaskBuilderNet.Data.DatabaseWinControls.DatabasesCombo.DropDownDatabases(this.DatabasesComboBox_OnDropDownDatabases);
			// 
			// txtNewDataBaseName
			// 
			this.txtNewDataBaseName.AcceptsReturn = true;
			resources.ApplyResources(this.txtNewDataBaseName, "txtNewDataBaseName");
			this.txtNewDataBaseName.Name = "txtNewDataBaseName";
			this.txtNewDataBaseName.TextChanged += new System.EventHandler(this.txtNewDataBaseName_TextChanged);
			// 
			// rbNewDb
			// 
			resources.ApplyResources(this.rbNewDb, "rbNewDb");
			this.rbNewDb.Name = "rbNewDb";
			this.rbNewDb.TabStop = true;
			this.rbNewDb.CheckedChanged += new System.EventHandler(this.rbNewDb_CheckedChanged);
			// 
			// rbSelectExistedDb
			// 
			resources.ApplyResources(this.rbSelectExistedDb, "rbSelectExistedDb");
			this.rbSelectExistedDb.Checked = true;
			this.rbSelectExistedDb.Name = "rbSelectExistedDb";
			this.rbSelectExistedDb.TabStop = true;
			this.rbSelectExistedDb.CheckedChanged += new System.EventHandler(this.rbSelectExistedDb_CheckedChanged);
			// 
			// toolTip
			// 
			this.toolTip.ShowAlways = true;
			// 
			// txtPassword
			// 
			resources.ApplyResources(this.txtPassword, "txtPassword");
			this.txtPassword.Name = "txtPassword";
			// 
			// txtName
			// 
			resources.ApplyResources(this.txtName, "txtName");
			this.txtName.Name = "txtName";
			// 
			// lblPassword
			// 
			resources.ApplyResources(this.lblPassword, "lblPassword");
			this.lblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblPassword.Name = "lblPassword";
			// 
			// lblName
			// 
			resources.ApplyResources(this.lblName, "lblName");
			this.lblName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblName.Name = "lblName";
			// 
			// lblPhase1
			// 
			this.lblPhase1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lblPhase1, "lblPhase1");
			this.lblPhase1.Name = "lblPhase1";
			// 
			// groupBox1
			// 
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Controls.Add(this.NGSqlServersCombo);
			this.groupBox1.Controls.Add(this.lblPhase1);
			this.groupBox1.Controls.Add(this.lblName);
			this.groupBox1.Controls.Add(this.lblPassword);
			this.groupBox1.Controls.Add(this.txtName);
			this.groupBox1.Controls.Add(this.txtPassword);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// NGSqlServersCombo
			// 
			resources.ApplyResources(this.NGSqlServersCombo, "NGSqlServersCombo");
			this.NGSqlServersCombo.BackColor = System.Drawing.SystemColors.Window;
			this.NGSqlServersCombo.Name = "NGSqlServersCombo";
			this.NGSqlServersCombo.SelectedSQLServer = "";
			this.NGSqlServersCombo.OnSetSelectedServerSQL += new Microarea.TaskBuilderNet.Data.DatabaseWinControls.NGSQLServersCombo.SetSelectedServerSQL(this.NGSqlServersCombo_OnSetSelectedServerSQL);
			this.NGSqlServersCombo.OnChangeServerName += new Microarea.TaskBuilderNet.Data.DatabaseWinControls.NGSQLServersCombo.ChangeServerName(this.NGSqlServersCombo_OnChangeServerName);
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
			this.AdminConsoleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(185)))), ((int)(((byte)(236)))));
			this.AdminConsoleLabel.Name = "AdminConsoleLabel";
			// 
			// Login
			// 
			this.AcceptButton = this.cmdOK;
			resources.ApplyResources(this, "$this");
			this.CancelButton = this.cmdCancel;
			this.Controls.Add(this.AdminConsoleLabel);
			this.Controls.Add(this.LogoPictureBox);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOK);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Login";
			this.ShowInTaskbar = false;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Login_Closing);
			this.Load += new System.EventHandler(this.Login_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
			this.ResumeLayout(false);

        }
        #endregion

		private NGSQLServersCombo NGSqlServersCombo;
		private DatabasesCombo DatabasesComboBox;
	}
}
