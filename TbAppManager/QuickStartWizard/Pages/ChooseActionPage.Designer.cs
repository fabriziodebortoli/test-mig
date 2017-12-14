namespace Microarea.MenuManager.QuickStartWizard.Pages
{
	partial class ChooseActionPage
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseActionPage));
			this.RBBaseConfiguration = new System.Windows.Forms.RadioButton();
			this.RBSkipConfiguration = new System.Windows.Forms.RadioButton();
			this.GBoxActions = new System.Windows.Forms.GroupBox();
			this.LblDescriSkip = new System.Windows.Forms.Label();
			this.LblDescriBase = new System.Windows.Forms.Label();
			this.m_headerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).BeginInit();
			this.GBoxActions.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_headerPanel
			// 
			resources.ApplyResources(this.m_headerPanel, "m_headerPanel");
			// 
			// m_headerPicture
			// 
			resources.ApplyResources(this.m_headerPicture, "m_headerPicture");
			// 
			// m_titleLabel
			// 
			resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
			// 
			// m_subtitleLabel
			// 
			resources.ApplyResources(this.m_subtitleLabel, "m_subtitleLabel");
			// 
			// RBBaseConfiguration
			// 
			resources.ApplyResources(this.RBBaseConfiguration, "RBBaseConfiguration");
			this.RBBaseConfiguration.Checked = true;
			this.RBBaseConfiguration.Name = "RBBaseConfiguration";
			this.RBBaseConfiguration.TabStop = true;
			this.RBBaseConfiguration.UseVisualStyleBackColor = true;
			// 
			// RBSkipConfiguration
			// 
			resources.ApplyResources(this.RBSkipConfiguration, "RBSkipConfiguration");
			this.RBSkipConfiguration.Name = "RBSkipConfiguration";
			this.RBSkipConfiguration.UseVisualStyleBackColor = true;
			// 
			// GBoxActions
			// 
			this.GBoxActions.Controls.Add(this.LblDescriSkip);
			this.GBoxActions.Controls.Add(this.LblDescriBase);
			this.GBoxActions.Controls.Add(this.RBSkipConfiguration);
			this.GBoxActions.Controls.Add(this.RBBaseConfiguration);
			resources.ApplyResources(this.GBoxActions, "GBoxActions");
			this.GBoxActions.Name = "GBoxActions";
			this.GBoxActions.TabStop = false;
			// 
			// LblDescriSkip
			// 
			resources.ApplyResources(this.LblDescriSkip, "LblDescriSkip");
			this.LblDescriSkip.Name = "LblDescriSkip";
			// 
			// LblDescriBase
			// 
			resources.ApplyResources(this.LblDescriBase, "LblDescriBase");
			this.LblDescriBase.Name = "LblDescriBase";
			// 
			// ChooseActionPage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.GBoxActions);
			this.Name = "ChooseActionPage";
			this.Controls.SetChildIndex(this.GBoxActions, 0);
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.m_headerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
			this.GBoxActions.ResumeLayout(false);
			this.GBoxActions.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RadioButton RBBaseConfiguration;
		private System.Windows.Forms.RadioButton RBSkipConfiguration;
		private System.Windows.Forms.GroupBox GBoxActions;
		private System.Windows.Forms.Label LblDescriSkip;
		private System.Windows.Forms.Label LblDescriBase;
	}
}
