namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	partial class ChooseCompanyPage
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseCompanyPage));
			this.CompaniesCBox = new System.Windows.Forms.ComboBox();
			this.LblCompany = new System.Windows.Forms.Label();
			this.MessageLabel = new System.Windows.Forms.Label();
			this.StatusPictureBox = new System.Windows.Forms.PictureBox();
			this.m_headerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StatusPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// m_headerPicture
			// 
			this.m_headerPicture.Image = global::Microarea.Console.Plugin.RowSecurityToolKit.Strings.TopSecretSmall;
			// 
			// m_titleLabel
			// 
			resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
			// 
			// m_subtitleLabel
			// 
			resources.ApplyResources(this.m_subtitleLabel, "m_subtitleLabel");
			// 
			// CompaniesCBox
			// 
			this.CompaniesCBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CompaniesCBox.FormattingEnabled = true;
			resources.ApplyResources(this.CompaniesCBox, "CompaniesCBox");
			this.CompaniesCBox.Name = "CompaniesCBox";
			this.CompaniesCBox.SelectedIndexChanged += new System.EventHandler(this.CompaniesCBox_SelectedIndexChanged);
			// 
			// LblCompany
			// 
			this.LblCompany.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblCompany, "LblCompany");
			this.LblCompany.Name = "LblCompany";
			// 
			// MessageLabel
			// 
			this.MessageLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.MessageLabel, "MessageLabel");
			this.MessageLabel.Name = "MessageLabel";
			// 
			// StatusPictureBox
			// 
			resources.ApplyResources(this.StatusPictureBox, "StatusPictureBox");
			this.StatusPictureBox.Name = "StatusPictureBox";
			this.StatusPictureBox.TabStop = false;
			// 
			// ChooseCompanyPage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.StatusPictureBox);
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.CompaniesCBox);
			this.Controls.Add(this.LblCompany);
			this.Name = "ChooseCompanyPage";
			this.Load += new System.EventHandler(this.ChooseCompanyPage_Load);
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.Controls.SetChildIndex(this.LblCompany, 0);
			this.Controls.SetChildIndex(this.CompaniesCBox, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.StatusPictureBox, 0);
			this.m_headerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StatusPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ComboBox CompaniesCBox;
		private System.Windows.Forms.Label LblCompany;
		private System.Windows.Forms.Label MessageLabel;
		private System.Windows.Forms.PictureBox StatusPictureBox;
	}
}
