namespace Microarea.Console.Plugin.RowSecurityToolKit.Forms
{
	partial class RSStartForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RSStartForm));
			this.TitleLabel = new System.Windows.Forms.Label();
			this.OperationsGBox = new System.Windows.Forms.GroupBox();
			this.CryptLinkLabel = new System.Windows.Forms.LinkLabel();
			this.EntitiesOverviewLinkLabel = new System.Windows.Forms.LinkLabel();
			this.EntitiesLinkLabel = new System.Windows.Forms.LinkLabel();
			this.MainPictureBox = new System.Windows.Forms.PictureBox();
			this.OperationsGBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// TitleLabel
			// 
			this.TitleLabel.AllowDrop = true;
			this.TitleLabel.BackColor = System.Drawing.Color.CornflowerBlue;
			this.TitleLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.TitleLabel, "TitleLabel");
			this.TitleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TitleLabel.ForeColor = System.Drawing.Color.White;
			this.TitleLabel.Name = "TitleLabel";
			// 
			// OperationsGBox
			// 
			this.OperationsGBox.Controls.Add(this.CryptLinkLabel);
			this.OperationsGBox.Controls.Add(this.EntitiesOverviewLinkLabel);
			this.OperationsGBox.Controls.Add(this.EntitiesLinkLabel);
			this.OperationsGBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.OperationsGBox, "OperationsGBox");
			this.OperationsGBox.Name = "OperationsGBox";
			this.OperationsGBox.TabStop = false;
			// 
			// CryptLinkLabel
			// 
			resources.ApplyResources(this.CryptLinkLabel, "CryptLinkLabel");
			this.CryptLinkLabel.Name = "CryptLinkLabel";
			this.CryptLinkLabel.TabStop = true;
			this.CryptLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CryptLinkLabel_LinkClicked);
			// 
			// EntitiesOverviewLinkLabel
			// 
			resources.ApplyResources(this.EntitiesOverviewLinkLabel, "EntitiesOverviewLinkLabel");
			this.EntitiesOverviewLinkLabel.Name = "EntitiesOverviewLinkLabel";
			this.EntitiesOverviewLinkLabel.TabStop = true;
			this.EntitiesOverviewLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.EntitiesOverviewLinkLabel_LinkClicked);
			// 
			// EntitiesLinkLabel
			// 
			resources.ApplyResources(this.EntitiesLinkLabel, "EntitiesLinkLabel");
			this.EntitiesLinkLabel.Name = "EntitiesLinkLabel";
			this.EntitiesLinkLabel.TabStop = true;
			this.EntitiesLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.EntitiesLinkLabel_LinkClicked);
			// 
			// MainPictureBox
			// 
			this.MainPictureBox.Image = global::Microarea.Console.Plugin.RowSecurityToolKit.Strings.TopSecret;
			resources.ApplyResources(this.MainPictureBox, "MainPictureBox");
			this.MainPictureBox.InitialImage = global::Microarea.Console.Plugin.RowSecurityToolKit.Strings.TopSecret;
			this.MainPictureBox.Name = "MainPictureBox";
			this.MainPictureBox.TabStop = false;
			// 
			// RSStartForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.MainPictureBox);
			this.Controls.Add(this.OperationsGBox);
			this.Controls.Add(this.TitleLabel);
			this.Name = "RSStartForm";
			this.OperationsGBox.ResumeLayout(false);
			this.OperationsGBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label TitleLabel;
		private System.Windows.Forms.GroupBox OperationsGBox;
		private System.Windows.Forms.LinkLabel EntitiesOverviewLinkLabel;
		private System.Windows.Forms.LinkLabel EntitiesLinkLabel;
		private System.Windows.Forms.LinkLabel CryptLinkLabel;
		private System.Windows.Forms.PictureBox MainPictureBox;
	}
}