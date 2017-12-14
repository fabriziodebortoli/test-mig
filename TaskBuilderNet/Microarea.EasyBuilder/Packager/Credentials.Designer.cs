namespace Microarea.EasyBuilder.Packager
{
	partial class Credentials
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Credentials));
			this.GroupBox = new System.Windows.Forms.GroupBox();
			this.LblCredentialsTestOutput = new System.Windows.Forms.Label();
			this.BtnTestCredentials = new System.Windows.Forms.Button();
			this.TxtPassword = new System.Windows.Forms.TextBox();
			this.LblPassword = new System.Windows.Forms.Label();
			this.TxtUsername = new System.Windows.Forms.TextBox();
			this.LblUsername = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.LblInformation = new System.Windows.Forms.Label();
			this.GroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// GroupBox
			// 
			this.GroupBox.BackColor = System.Drawing.Color.Transparent;
			this.GroupBox.Controls.Add(this.LblCredentialsTestOutput);
			this.GroupBox.Controls.Add(this.BtnTestCredentials);
			this.GroupBox.Controls.Add(this.TxtPassword);
			this.GroupBox.Controls.Add(this.LblPassword);
			this.GroupBox.Controls.Add(this.TxtUsername);
			this.GroupBox.Controls.Add(this.LblUsername);
			resources.ApplyResources(this.GroupBox, "GroupBox");
			this.GroupBox.Name = "GroupBox";
			this.GroupBox.TabStop = false;
			// 
			// LblCredentialsTestOutput
			// 
			resources.ApplyResources(this.LblCredentialsTestOutput, "LblCredentialsTestOutput");
			this.LblCredentialsTestOutput.Name = "LblCredentialsTestOutput";
			// 
			// BtnTestCredentials
			// 
			resources.ApplyResources(this.BtnTestCredentials, "BtnTestCredentials");
			this.BtnTestCredentials.Name = "BtnTestCredentials";
			this.BtnTestCredentials.UseVisualStyleBackColor = true;
			this.BtnTestCredentials.Click += new System.EventHandler(this.BtnTestCredentials_Click);
			// 
			// TxtPassword
			// 
			resources.ApplyResources(this.TxtPassword, "TxtPassword");
			this.TxtPassword.Name = "TxtPassword";
			this.TxtPassword.TextChanged += new System.EventHandler(this.EnableBtnTestCredentials);
			// 
			// LblPassword
			// 
			resources.ApplyResources(this.LblPassword, "LblPassword");
			this.LblPassword.Name = "LblPassword";
			// 
			// TxtUsername
			// 
			resources.ApplyResources(this.TxtUsername, "TxtUsername");
			this.TxtUsername.Name = "TxtUsername";
			this.TxtUsername.TextChanged += new System.EventHandler(this.EnableBtnTestCredentials);
			// 
			// LblUsername
			// 
			resources.ApplyResources(this.LblUsername, "LblUsername");
			this.LblUsername.Name = "LblUsername";
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Name = "btnOk";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// LblInformation
			// 
			resources.ApplyResources(this.LblInformation, "LblInformation");
			this.LblInformation.Name = "LblInformation";
			// 
			// Credentials
			// 
			this.AcceptButton = this.btnOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.Controls.Add(this.LblInformation);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.GroupBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Credentials";
			this.GroupBox.ResumeLayout(false);
			this.GroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox GroupBox;
		private System.Windows.Forms.Label LblCredentialsTestOutput;
		private System.Windows.Forms.Button BtnTestCredentials;
		private System.Windows.Forms.TextBox TxtPassword;
		private System.Windows.Forms.Label LblPassword;
		private System.Windows.Forms.TextBox TxtUsername;
		private System.Windows.Forms.Label LblUsername;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Label LblInformation;
	}
}