namespace ClickOnceDeployer
{
	partial class ClickOnceDeployerUIForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClickOnceDeployerUIForm));
			this.groupBoxActions = new System.Windows.Forms.GroupBox();
			this.comboBoxActions = new System.Windows.Forms.ComboBox();
			this.groupBoxVersion = new System.Windows.Forms.GroupBox();
			this.radioButtonRelease = new System.Windows.Forms.RadioButton();
			this.radioButtonDebug = new System.Windows.Forms.RadioButton();
			this.txtboxInstallationName = new System.Windows.Forms.TextBox();
			this.lblInstallationName = new System.Windows.Forms.Label();
			this.lblRoot = new System.Windows.Forms.Label();
			this.txtboxRoot = new System.Windows.Forms.TextBox();
			this.btnRunCmd = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.txtboxUiCulture = new System.Windows.Forms.TextBox();
			this.lblUiCulture = new System.Windows.Forms.Label();
			this.lblWebServicesPort = new System.Windows.Forms.Label();
			this.txtboxWebServicePort = new System.Windows.Forms.TextBox();
			this.lblUser = new System.Windows.Forms.Label();
			this.txtboxUser = new System.Windows.Forms.TextBox();
			this.groupBoxActions.SuspendLayout();
			this.groupBoxVersion.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxActions
			// 
			this.groupBoxActions.Controls.Add(this.comboBoxActions);
			this.groupBoxActions.Location = new System.Drawing.Point(13, 43);
			this.groupBoxActions.Name = "groupBoxActions";
			this.groupBoxActions.Size = new System.Drawing.Size(289, 50);
			this.groupBoxActions.TabIndex = 0;
			this.groupBoxActions.TabStop = false;
			this.groupBoxActions.Tag = "Deploy";
			this.groupBoxActions.Text = "Actions";
			// 
			// comboBoxActions
			// 
			this.comboBoxActions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxActions.FormattingEnabled = true;
			this.comboBoxActions.Location = new System.Drawing.Point(6, 19);
			this.comboBoxActions.Name = "comboBoxActions";
			this.comboBoxActions.Size = new System.Drawing.Size(277, 21);
			this.comboBoxActions.TabIndex = 0;
			// 
			// groupBoxVersion
			// 
			this.groupBoxVersion.Controls.Add(this.radioButtonRelease);
			this.groupBoxVersion.Controls.Add(this.radioButtonDebug);
			this.groupBoxVersion.Location = new System.Drawing.Point(13, 103);
			this.groupBoxVersion.Name = "groupBoxVersion";
			this.groupBoxVersion.Size = new System.Drawing.Size(289, 50);
			this.groupBoxVersion.TabIndex = 1;
			this.groupBoxVersion.TabStop = false;
			this.groupBoxVersion.Tag = "Release";
			this.groupBoxVersion.Text = "Version";
			// 
			// radioButtonRelease
			// 
			this.radioButtonRelease.AutoSize = true;
			this.radioButtonRelease.Checked = true;
			this.radioButtonRelease.Location = new System.Drawing.Point(171, 19);
			this.radioButtonRelease.Name = "radioButtonRelease";
			this.radioButtonRelease.Size = new System.Drawing.Size(64, 17);
			this.radioButtonRelease.TabIndex = 1;
			this.radioButtonRelease.TabStop = true;
			this.radioButtonRelease.Tag = "Release";
			this.radioButtonRelease.Text = "Release";
			this.radioButtonRelease.UseVisualStyleBackColor = true;
			// 
			// radioButtonDebug
			// 
			this.radioButtonDebug.AutoSize = true;
			this.radioButtonDebug.Location = new System.Drawing.Point(22, 19);
			this.radioButtonDebug.Name = "radioButtonDebug";
			this.radioButtonDebug.Size = new System.Drawing.Size(57, 17);
			this.radioButtonDebug.TabIndex = 0;
			this.radioButtonDebug.TabStop = true;
			this.radioButtonDebug.Tag = "Debug";
			this.radioButtonDebug.Text = "Debug";
			this.radioButtonDebug.UseVisualStyleBackColor = true;
			// 
			// txtboxInstallationName
			// 
			this.txtboxInstallationName.Enabled = false;
			this.txtboxInstallationName.Location = new System.Drawing.Point(13, 230);
			this.txtboxInstallationName.Name = "txtboxInstallationName";
			this.txtboxInstallationName.Size = new System.Drawing.Size(289, 20);
			this.txtboxInstallationName.TabIndex = 5;
			// 
			// lblInstallationName
			// 
			this.lblInstallationName.AutoSize = true;
			this.lblInstallationName.Location = new System.Drawing.Point(15, 214);
			this.lblInstallationName.Name = "lblInstallationName";
			this.lblInstallationName.Size = new System.Drawing.Size(85, 13);
			this.lblInstallationName.TabIndex = 4;
			this.lblInstallationName.Text = "InstallationName";
			// 
			// lblRoot
			// 
			this.lblRoot.AutoSize = true;
			this.lblRoot.Location = new System.Drawing.Point(15, 169);
			this.lblRoot.Name = "lblRoot";
			this.lblRoot.Size = new System.Drawing.Size(30, 13);
			this.lblRoot.TabIndex = 2;
			this.lblRoot.Text = "Root";
			// 
			// txtboxRoot
			// 
			this.txtboxRoot.Enabled = false;
			this.txtboxRoot.Location = new System.Drawing.Point(12, 185);
			this.txtboxRoot.Name = "txtboxRoot";
			this.txtboxRoot.Size = new System.Drawing.Size(289, 20);
			this.txtboxRoot.TabIndex = 3;
			// 
			// btnRunCmd
			// 
			this.btnRunCmd.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnRunCmd.Location = new System.Drawing.Point(12, 412);
			this.btnRunCmd.Name = "btnRunCmd";
			this.btnRunCmd.Size = new System.Drawing.Size(289, 23);
			this.btnRunCmd.TabIndex = 13;
			this.btnRunCmd.Text = "&Run";
			this.btnRunCmd.UseVisualStyleBackColor = true;
			this.btnRunCmd.Click += new System.EventHandler(this.btnRunCmd_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(314, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runToolStripMenuItem,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// runToolStripMenuItem
			// 
			this.runToolStripMenuItem.Name = "runToolStripMenuItem";
			this.runToolStripMenuItem.Size = new System.Drawing.Size(95, 22);
			this.runToolStripMenuItem.Text = "&Run";
			this.runToolStripMenuItem.Click += new System.EventHandler(this.btnRunCmd_Click);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(95, 22);
			this.exitToolStripMenuItem.Text = "&Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// txtboxUiCulture
			// 
			this.txtboxUiCulture.Enabled = false;
			this.txtboxUiCulture.Location = new System.Drawing.Point(13, 274);
			this.txtboxUiCulture.Name = "txtboxUiCulture";
			this.txtboxUiCulture.Size = new System.Drawing.Size(289, 20);
			this.txtboxUiCulture.TabIndex = 7;
			// 
			// lblUiCulture
			// 
			this.lblUiCulture.AutoSize = true;
			this.lblUiCulture.Location = new System.Drawing.Point(15, 258);
			this.lblUiCulture.Name = "lblUiCulture";
			this.lblUiCulture.Size = new System.Drawing.Size(50, 13);
			this.lblUiCulture.TabIndex = 6;
			this.lblUiCulture.Text = "UiCulture";
			// 
			// lblWebServicesPort
			// 
			this.lblWebServicesPort.AutoSize = true;
			this.lblWebServicesPort.Location = new System.Drawing.Point(15, 304);
			this.lblWebServicesPort.Name = "lblWebServicesPort";
			this.lblWebServicesPort.Size = new System.Drawing.Size(90, 13);
			this.lblWebServicesPort.TabIndex = 8;
			this.lblWebServicesPort.Text = "WebServicesPort";
			// 
			// txtboxWebServicePort
			// 
			this.txtboxWebServicePort.Enabled = false;
			this.txtboxWebServicePort.Location = new System.Drawing.Point(13, 320);
			this.txtboxWebServicePort.Name = "txtboxWebServicePort";
			this.txtboxWebServicePort.Size = new System.Drawing.Size(289, 20);
			this.txtboxWebServicePort.TabIndex = 9;
			// 
			// lblUser
			// 
			this.lblUser.AutoSize = true;
			this.lblUser.Location = new System.Drawing.Point(15, 345);
			this.lblUser.Name = "lblUser";
			this.lblUser.Size = new System.Drawing.Size(151, 13);
			this.lblUser.TabIndex = 10;
			this.lblUser.Text = "User (Application Pool Identity)";
			// 
			// txtboxUser
			// 
			this.txtboxUser.Location = new System.Drawing.Point(13, 361);
			this.txtboxUser.Name = "txtboxUser";
			this.txtboxUser.Size = new System.Drawing.Size(289, 20);
			this.txtboxUser.TabIndex = 11;
			// 
			// ClickOnceDeployerUIForm
			// 
			this.AcceptButton = this.btnRunCmd;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(314, 448);
			this.Controls.Add(this.lblUser);
			this.Controls.Add(this.txtboxUser);
			this.Controls.Add(this.lblWebServicesPort);
			this.Controls.Add(this.txtboxWebServicePort);
			this.Controls.Add(this.lblUiCulture);
			this.Controls.Add(this.txtboxUiCulture);
			this.Controls.Add(this.btnRunCmd);
			this.Controls.Add(this.txtboxRoot);
			this.Controls.Add(this.lblRoot);
			this.Controls.Add(this.lblInstallationName);
			this.Controls.Add(this.txtboxInstallationName);
			this.Controls.Add(this.groupBoxVersion);
			this.Controls.Add(this.groupBoxActions);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.MaximizeBox = false;
			this.Name = "ClickOnceDeployerUIForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "ClickOnceDeployer";
			this.Load += new System.EventHandler(this.ClickOnceDeployerUIForm_Load);
			this.groupBoxActions.ResumeLayout(false);
			this.groupBoxVersion.ResumeLayout(false);
			this.groupBoxVersion.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBoxActions;
		private System.Windows.Forms.GroupBox groupBoxVersion;
		private System.Windows.Forms.RadioButton radioButtonRelease;
		private System.Windows.Forms.RadioButton radioButtonDebug;
		private System.Windows.Forms.TextBox txtboxInstallationName;
		private System.Windows.Forms.Label lblInstallationName;
		private System.Windows.Forms.Label lblRoot;
		private System.Windows.Forms.TextBox txtboxRoot;
		private System.Windows.Forms.Button btnRunCmd;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ComboBox comboBoxActions;
		private System.Windows.Forms.TextBox txtboxUiCulture;
		private System.Windows.Forms.Label lblUiCulture;
		private System.Windows.Forms.Label lblWebServicesPort;
		private System.Windows.Forms.TextBox txtboxWebServicePort;
		private System.Windows.Forms.ToolStripMenuItem runToolStripMenuItem;
		private System.Windows.Forms.Label lblUser;
		private System.Windows.Forms.TextBox txtboxUser;
	}
}