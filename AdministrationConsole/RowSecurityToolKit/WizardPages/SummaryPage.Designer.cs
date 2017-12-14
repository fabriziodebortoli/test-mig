namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
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
			this.SummaryRichTextBox = new System.Windows.Forms.RichTextBox();
			this.SummaryLabel = new System.Windows.Forms.Label();
			this.OperationsListView = new System.Windows.Forms.ListView();
			((System.ComponentModel.ISupportInitialize)(this.m_watermarkPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// m_watermarkPicture
			// 
			this.m_watermarkPicture.Image = global::Microarea.Console.Plugin.RowSecurityToolKit.Strings.TopSecret;
			// 
			// m_titleLabel
			// 
			resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
			// 
			// SummaryRichTextBox
			// 
			resources.ApplyResources(this.SummaryRichTextBox, "SummaryRichTextBox");
			this.SummaryRichTextBox.Name = "SummaryRichTextBox";
			this.SummaryRichTextBox.ReadOnly = true;
			// 
			// SummaryLabel
			// 
			resources.ApplyResources(this.SummaryLabel, "SummaryLabel");
			this.SummaryLabel.Name = "SummaryLabel";
			// 
			// OperationsListView
			// 
			this.OperationsListView.FullRowSelect = true;
			this.OperationsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			resources.ApplyResources(this.OperationsListView, "OperationsListView");
			this.OperationsListView.MultiSelect = false;
			this.OperationsListView.Name = "OperationsListView";
			this.OperationsListView.UseCompatibleStateImageBehavior = false;
			this.OperationsListView.View = System.Windows.Forms.View.Details;
			this.OperationsListView.DoubleClick += new System.EventHandler(this.OperationsListView_DoubleClick);
			// 
			// SummaryPage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.OperationsListView);
			this.Controls.Add(this.SummaryRichTextBox);
			this.Controls.Add(this.SummaryLabel);
			this.Name = "SummaryPage";
			this.Controls.SetChildIndex(this.m_watermarkPicture, 0);
			this.Controls.SetChildIndex(this.m_titleLabel, 0);
			this.Controls.SetChildIndex(this.SummaryLabel, 0);
			this.Controls.SetChildIndex(this.SummaryRichTextBox, 0);
			this.Controls.SetChildIndex(this.OperationsListView, 0);
			((System.ComponentModel.ISupportInitialize)(this.m_watermarkPicture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox SummaryRichTextBox;
		private System.Windows.Forms.Label SummaryLabel;
		private System.Windows.Forms.ListView OperationsListView;
	}
}
