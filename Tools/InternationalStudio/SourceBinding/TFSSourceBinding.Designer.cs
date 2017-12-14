namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	partial class TFSSourceBinding
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TFSSourceBinding));
			this.txtServer = new System.Windows.Forms.TextBox();
			this.Server = new System.Windows.Forms.Label();
			this.txtUser = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cbWorkspace = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.OK = new System.Windows.Forms.Button();
			this.Cancel = new System.Windows.Forms.Button();
			this.txtPort = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tFSBindingBindingSource = new System.Windows.Forms.BindingSource(this.components);
			((System.ComponentModel.ISupportInitialize)(this.tFSBindingBindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// txtServer
			// 
			this.txtServer.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.tFSBindingBindingSource, "Server", true));
			resources.ApplyResources(this.txtServer, "txtServer");
			this.txtServer.Name = "txtServer";
			this.txtServer.TextChanged += new System.EventHandler(this.txtServer_TextChanged);
			// 
			// Server
			// 
			resources.ApplyResources(this.Server, "Server");
			this.Server.Name = "Server";
			// 
			// txtUser
			// 
			this.txtUser.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.tFSBindingBindingSource, "User", true));
			resources.ApplyResources(this.txtUser, "txtUser");
			this.txtUser.Name = "txtUser";
			this.txtUser.TextChanged += new System.EventHandler(this.txtUser_TextChanged);
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// txtPassword
			// 
			this.txtPassword.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.tFSBindingBindingSource, "Password", true));
			resources.ApplyResources(this.txtPassword, "txtPassword");
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.UseSystemPasswordChar = true;
			this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// cbWorkspace
			// 
			this.cbWorkspace.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.tFSBindingBindingSource, "Workspace", true));
			this.cbWorkspace.DisplayMember = "Name";
			this.cbWorkspace.FormattingEnabled = true;
			resources.ApplyResources(this.cbWorkspace, "cbWorkspace");
			this.cbWorkspace.Name = "cbWorkspace";
			this.cbWorkspace.ValueMember = "Name";
			this.cbWorkspace.DropDown += new System.EventHandler(this.cbWorkspace_DropDown);
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// OK
			// 
			resources.ApplyResources(this.OK, "OK");
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Name = "OK";
			// 
			// Cancel
			// 
			resources.ApplyResources(this.Cancel, "Cancel");
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Name = "Cancel";
			// 
			// txtPort
			// 
			this.txtPort.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.tFSBindingBindingSource, "Port", true));
			resources.ApplyResources(this.txtPort, "txtPort");
			this.txtPort.Name = "txtPort";
			this.txtPort.TextChanged += new System.EventHandler(this.txtServer_TextChanged);
			// 
			// label4
			// 
			resources.ApplyResources(this.label4, "label4");
			this.label4.Name = "label4";
			// 
			// tFSBindingBindingSource
			// 
			this.tFSBindingBindingSource.DataSource = typeof(Microarea.Tools.TBLocalizer.SourceBinding.TFSBinding);
			// 
			// TFSSourceBinding
			// 
			this.AcceptButton = this.OK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.cbWorkspace);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.Server);
			this.Controls.Add(this.txtPassword);
			this.Controls.Add(this.txtUser);
			this.Controls.Add(this.txtPort);
			this.Controls.Add(this.txtServer);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "TFSSourceBinding";
			this.ShowInTaskbar = false;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TFSSourceBinding_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.tFSBindingBindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtServer;
		private System.Windows.Forms.Label Server;
		private System.Windows.Forms.TextBox txtUser;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cbWorkspace;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button OK;
		private System.Windows.Forms.Button Cancel;
		private System.Windows.Forms.BindingSource tFSBindingBindingSource;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.Label label4;
	}
}