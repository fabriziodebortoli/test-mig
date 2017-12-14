namespace Microarea.Console.Plugin.RowSecurityToolKit.Forms
{
	partial class CrsGenerator
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CrsGenerator));
			this.BtnCrypt = new System.Windows.Forms.Button();
			this.LblInfo = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// BtnCrypt
			// 
			resources.ApplyResources(this.BtnCrypt, "BtnCrypt");
			this.BtnCrypt.Name = "BtnCrypt";
			this.BtnCrypt.UseVisualStyleBackColor = true;
			this.BtnCrypt.Click += new System.EventHandler(this.BtnCrypt_Click);
			this.BtnCrypt.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BtnCrypt_KeyDown);
			// 
			// LblInfo
			// 
			resources.ApplyResources(this.LblInfo, "LblInfo");
			this.LblInfo.Name = "LblInfo";
			// 
			// CrsGenerator
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.LblInfo);
			this.Controls.Add(this.BtnCrypt);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CrsGenerator";
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button BtnCrypt;
		private System.Windows.Forms.Label LblInfo;
	}
}