namespace Microarea.Console.Plugin.HermesAdmin
{
	partial class MailServerSettingsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MailServerSettingsForm));
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblUserInformation = new System.Windows.Forms.Label();
			this.lblUserName = new System.Windows.Forms.Label();
			this.txtUserName = new System.Windows.Forms.TextBox();
			this.txtEMailAddress = new System.Windows.Forms.TextBox();
			this.lblEMailAddress = new System.Windows.Forms.Label();
			this.txtIncomingMailServer = new System.Windows.Forms.TextBox();
			this.lblIncomingMailServer = new System.Windows.Forms.Label();
			this.lblServerInformation = new System.Windows.Forms.Label();
			this.txtOutgoingMailServer = new System.Windows.Forms.TextBox();
			this.lblOutgoingMailServer = new System.Windows.Forms.Label();
			this.lblAccountType = new System.Windows.Forms.Label();
			this.cmbAccountType = new System.Windows.Forms.ComboBox();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.lblPassword = new System.Windows.Forms.Label();
			this.txtLoginName = new System.Windows.Forms.TextBox();
			this.lblLoginName = new System.Windows.Forms.Label();
			this.lblLogonInformation = new System.Windows.Forms.Label();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnUndo = new System.Windows.Forms.Button();
			this.nudInPort = new System.Windows.Forms.NumericUpDown();
			this.lblInPort = new System.Windows.Forms.Label();
			this.lblOutPort = new System.Windows.Forms.Label();
			this.nudOutPort = new System.Windows.Forms.NumericUpDown();
			this.chkOutUseSSL = new System.Windows.Forms.CheckBox();
			this.chkInUseSSL = new System.Windows.Forms.CheckBox();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			((System.ComponentModel.ISupportInitialize)(this.nudInPort)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudOutPort)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// lblTitle
			// 
			resources.ApplyResources(this.lblTitle, "lblTitle");
			this.lblTitle.BackColor = System.Drawing.Color.CornflowerBlue;
			this.lblTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblTitle.ForeColor = System.Drawing.Color.White;
			this.lblTitle.Name = "lblTitle";
			// 
			// lblUserInformation
			// 
			resources.ApplyResources(this.lblUserInformation, "lblUserInformation");
			this.lblUserInformation.Name = "lblUserInformation";
			// 
			// lblUserName
			// 
			resources.ApplyResources(this.lblUserName, "lblUserName");
			this.lblUserName.Name = "lblUserName";
			// 
			// txtUserName
			// 
			resources.ApplyResources(this.txtUserName, "txtUserName");
			this.txtUserName.Name = "txtUserName";
			this.txtUserName.TextChanged += new System.EventHandler(this.txtUserName_TextChanged);
			// 
			// txtEMailAddress
			// 
			resources.ApplyResources(this.txtEMailAddress, "txtEMailAddress");
			this.txtEMailAddress.Name = "txtEMailAddress";
			this.txtEMailAddress.TextChanged += new System.EventHandler(this.txtEMailAddress_TextChanged);
			this.txtEMailAddress.Validating += new System.ComponentModel.CancelEventHandler(this.txtEMailAddress_Validating);
			this.txtEMailAddress.Validated += new System.EventHandler(this.txtEMailAddress_Validated);
			// 
			// lblEMailAddress
			// 
			resources.ApplyResources(this.lblEMailAddress, "lblEMailAddress");
			this.lblEMailAddress.Name = "lblEMailAddress";
			// 
			// txtIncomingMailServer
			// 
			resources.ApplyResources(this.txtIncomingMailServer, "txtIncomingMailServer");
			this.txtIncomingMailServer.Name = "txtIncomingMailServer";
			this.txtIncomingMailServer.TextChanged += new System.EventHandler(this.txtIncomingMailServer_TextChanged);
			// 
			// lblIncomingMailServer
			// 
			resources.ApplyResources(this.lblIncomingMailServer, "lblIncomingMailServer");
			this.lblIncomingMailServer.Name = "lblIncomingMailServer";
			// 
			// lblServerInformation
			// 
			resources.ApplyResources(this.lblServerInformation, "lblServerInformation");
			this.lblServerInformation.Name = "lblServerInformation";
			// 
			// txtOutgoingMailServer
			// 
			resources.ApplyResources(this.txtOutgoingMailServer, "txtOutgoingMailServer");
			this.txtOutgoingMailServer.Name = "txtOutgoingMailServer";
			this.txtOutgoingMailServer.TextChanged += new System.EventHandler(this.txtOutgoingMailServer_TextChanged);
			// 
			// lblOutgoingMailServer
			// 
			resources.ApplyResources(this.lblOutgoingMailServer, "lblOutgoingMailServer");
			this.lblOutgoingMailServer.Name = "lblOutgoingMailServer";
			// 
			// lblAccountType
			// 
			resources.ApplyResources(this.lblAccountType, "lblAccountType");
			this.lblAccountType.Name = "lblAccountType";
			// 
			// cmbAccountType
			// 
			this.cmbAccountType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAccountType.FormattingEnabled = true;
			resources.ApplyResources(this.cmbAccountType, "cmbAccountType");
			this.cmbAccountType.Name = "cmbAccountType";
			this.cmbAccountType.SelectedIndexChanged += new System.EventHandler(this.cmbAccountType_SelectedIndexChanged);
			// 
			// txtPassword
			// 
			resources.ApplyResources(this.txtPassword, "txtPassword");
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.UseSystemPasswordChar = true;
			this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
			// 
			// lblPassword
			// 
			resources.ApplyResources(this.lblPassword, "lblPassword");
			this.lblPassword.Name = "lblPassword";
			// 
			// txtLoginName
			// 
			resources.ApplyResources(this.txtLoginName, "txtLoginName");
			this.txtLoginName.Name = "txtLoginName";
			this.txtLoginName.TextChanged += new System.EventHandler(this.txtLoginName_TextChanged);
			// 
			// lblLoginName
			// 
			resources.ApplyResources(this.lblLoginName, "lblLoginName");
			this.lblLoginName.Name = "lblLoginName";
			// 
			// lblLogonInformation
			// 
			resources.ApplyResources(this.lblLogonInformation, "lblLogonInformation");
			this.lblLogonInformation.Name = "lblLogonInformation";
			// 
			// btnSave
			// 
			resources.ApplyResources(this.btnSave, "btnSave");
			this.btnSave.Name = "btnSave";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// btnUndo
			// 
			resources.ApplyResources(this.btnUndo, "btnUndo");
			this.btnUndo.Name = "btnUndo";
			this.btnUndo.UseVisualStyleBackColor = true;
			this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
			// 
			// nudInPort
			// 
			resources.ApplyResources(this.nudInPort, "nudInPort");
			this.nudInPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
			this.nudInPort.Name = "nudInPort";
			this.nudInPort.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
			this.nudInPort.ValueChanged += new System.EventHandler(this.nudInPort_ValueChanged);
			// 
			// lblInPort
			// 
			resources.ApplyResources(this.lblInPort, "lblInPort");
			this.lblInPort.Name = "lblInPort";
			// 
			// lblOutPort
			// 
			resources.ApplyResources(this.lblOutPort, "lblOutPort");
			this.lblOutPort.Name = "lblOutPort";
			// 
			// nudOutPort
			// 
			resources.ApplyResources(this.nudOutPort, "nudOutPort");
			this.nudOutPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
			this.nudOutPort.Name = "nudOutPort";
			this.nudOutPort.Value = new decimal(new int[] {
            110,
            0,
            0,
            0});
			this.nudOutPort.ValueChanged += new System.EventHandler(this.nudOutPort_ValueChanged);
			// 
			// chkOutUseSSL
			// 
			resources.ApplyResources(this.chkOutUseSSL, "chkOutUseSSL");
			this.chkOutUseSSL.Name = "chkOutUseSSL";
			this.chkOutUseSSL.UseVisualStyleBackColor = true;
			this.chkOutUseSSL.CheckedChanged += new System.EventHandler(this.chkOutUseSSL_CheckedChanged);
			// 
			// chkInUseSSL
			// 
			resources.ApplyResources(this.chkInUseSSL, "chkInUseSSL");
			this.chkInUseSSL.Name = "chkInUseSSL";
			this.chkInUseSSL.UseVisualStyleBackColor = true;
			this.chkInUseSSL.CheckedChanged += new System.EventHandler(this.chkInUseSSL_CheckedChanged);
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// MailServerSettingsForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.chkInUseSSL);
			this.Controls.Add(this.nudOutPort);
			this.Controls.Add(this.nudInPort);
			this.Controls.Add(this.chkOutUseSSL);
			this.Controls.Add(this.lblOutPort);
			this.Controls.Add(this.lblInPort);
			this.Controls.Add(this.btnUndo);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.txtPassword);
			this.Controls.Add(this.lblPassword);
			this.Controls.Add(this.txtLoginName);
			this.Controls.Add(this.lblLoginName);
			this.Controls.Add(this.lblLogonInformation);
			this.Controls.Add(this.cmbAccountType);
			this.Controls.Add(this.lblAccountType);
			this.Controls.Add(this.txtOutgoingMailServer);
			this.Controls.Add(this.lblOutgoingMailServer);
			this.Controls.Add(this.txtIncomingMailServer);
			this.Controls.Add(this.lblIncomingMailServer);
			this.Controls.Add(this.lblServerInformation);
			this.Controls.Add(this.txtEMailAddress);
			this.Controls.Add(this.lblEMailAddress);
			this.Controls.Add(this.txtUserName);
			this.Controls.Add(this.lblUserName);
			this.Controls.Add(this.lblUserInformation);
			this.Controls.Add(this.lblTitle);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "MailServerSettingsForm";
			this.Load += new System.EventHandler(this.MailServerSettingsForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.nudInPort)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudOutPort)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblUserInformation;
		private System.Windows.Forms.Label lblUserName;
		private System.Windows.Forms.TextBox txtUserName;
		private System.Windows.Forms.TextBox txtEMailAddress;
		private System.Windows.Forms.Label lblEMailAddress;
		private System.Windows.Forms.TextBox txtIncomingMailServer;
		private System.Windows.Forms.Label lblIncomingMailServer;
		private System.Windows.Forms.Label lblServerInformation;
		private System.Windows.Forms.TextBox txtOutgoingMailServer;
		private System.Windows.Forms.Label lblOutgoingMailServer;
		private System.Windows.Forms.Label lblAccountType;
		private System.Windows.Forms.ComboBox cmbAccountType;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.TextBox txtLoginName;
		private System.Windows.Forms.Label lblLoginName;
		private System.Windows.Forms.Label lblLogonInformation;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnUndo;
		private System.Windows.Forms.NumericUpDown nudInPort;
		private System.Windows.Forms.Label lblInPort;
		private System.Windows.Forms.Label lblOutPort;
		private System.Windows.Forms.NumericUpDown nudOutPort;
		private System.Windows.Forms.CheckBox chkOutUseSSL;
		private System.Windows.Forms.CheckBox chkInUseSSL;
        private System.Windows.Forms.ErrorProvider errorProvider;
	}
}