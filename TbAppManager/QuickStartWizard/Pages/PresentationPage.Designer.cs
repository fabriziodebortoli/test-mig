namespace Microarea.MenuManager.QuickStartWizard.Pages
{
	partial class PresentationPage
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PresentationPage));
			this.LblPresentation = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_watermarkPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// m_watermarkPicture
			// 
			resources.ApplyResources(this.m_watermarkPicture, "m_watermarkPicture");
			// 
			// m_titleLabel
			// 
			resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
			// 
			// LblPresentation
			// 
			this.LblPresentation.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblPresentation, "LblPresentation");
			this.LblPresentation.Name = "LblPresentation";
			// 
			// PresentationPage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.LblPresentation);
			this.Name = "PresentationPage";
			this.Controls.SetChildIndex(this.m_watermarkPicture, 0);
			this.Controls.SetChildIndex(this.m_titleLabel, 0);
			this.Controls.SetChildIndex(this.LblPresentation, 0);
			((System.ComponentModel.ISupportInitialize)(this.m_watermarkPicture)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label LblPresentation;
	}
}
