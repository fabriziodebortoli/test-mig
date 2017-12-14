namespace Microarea.MenuManager.QuickStartWizard.Pages
{
	partial class SerialNumberPage
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SerialNumberPage));
			this.PnlContent = new System.Windows.Forms.Panel();
			this.m_headerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// m_headerPanel
			// 
			resources.ApplyResources(this.m_headerPanel, "m_headerPanel");
			// 
			// m_titleLabel
			// 
			resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
			// 
			// m_subtitleLabel
			// 
			resources.ApplyResources(this.m_subtitleLabel, "m_subtitleLabel");
			// 
			// PnlContent
			// 
			resources.ApplyResources(this.PnlContent, "PnlContent");
			this.PnlContent.Name = "PnlContent";
			// 
			// SingleSerialNumberPage
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.PnlContent);
			this.Name = "SingleSerialNumberPage";
			resources.ApplyResources(this, "$this");
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.Controls.SetChildIndex(this.PnlContent, 0);
			this.m_headerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Panel PnlContent;


    }
}
