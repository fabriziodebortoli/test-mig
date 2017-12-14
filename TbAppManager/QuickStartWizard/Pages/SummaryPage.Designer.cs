namespace Microarea.MenuManager.QuickStartWizard.Pages
{
	partial class SummaryPage
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SummaryPage));
			this.LblSummaryInfo = new System.Windows.Forms.Label();
			this.SummaryRichTextBox = new System.Windows.Forms.RichTextBox();
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
			// LblSummaryInfo
			// 
			resources.ApplyResources(this.LblSummaryInfo, "LblSummaryInfo");
			this.LblSummaryInfo.Name = "LblSummaryInfo";
			// 
			// SummaryRichTextBox
			// 
			resources.ApplyResources(this.SummaryRichTextBox, "SummaryRichTextBox");
			this.SummaryRichTextBox.BackColor = System.Drawing.Color.White;
			this.SummaryRichTextBox.Name = "SummaryRichTextBox";
			this.SummaryRichTextBox.ReadOnly = true;
			// 
			// SummaryPage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.SummaryRichTextBox);
			this.Controls.Add(this.LblSummaryInfo);
			this.Name = "SummaryPage";
			this.Controls.SetChildIndex(this.LblSummaryInfo, 0);
			this.Controls.SetChildIndex(this.m_watermarkPicture, 0);
			this.Controls.SetChildIndex(this.m_titleLabel, 0);
			this.Controls.SetChildIndex(this.SummaryRichTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.m_watermarkPicture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LblSummaryInfo;
		private System.Windows.Forms.RichTextBox SummaryRichTextBox;

	}
}
