namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	partial class SelectMasterInfoPage
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectMasterInfoPage));
			this.LblDocNs = new System.Windows.Forms.Label();
			this.DocNsComboBox = new System.Windows.Forms.ComboBox();
			this.LblMasterTable = new System.Windows.Forms.Label();
			this.LblColumns = new System.Windows.Forms.Label();
			this.AlertPictureBox = new System.Windows.Forms.PictureBox();
			this.m_headerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AlertPictureBox)).BeginInit();
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
			// LblDocNs
			// 
			resources.ApplyResources(this.LblDocNs, "LblDocNs");
			this.LblDocNs.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblDocNs.Name = "LblDocNs";
			// 
			// DocNsComboBox
			// 
			this.DocNsComboBox.FormattingEnabled = true;
			resources.ApplyResources(this.DocNsComboBox, "DocNsComboBox");
			this.DocNsComboBox.Name = "DocNsComboBox";
			this.DocNsComboBox.SelectedIndexChanged += new System.EventHandler(this.DocNsComboBox_SelectedIndexChanged);
			this.DocNsComboBox.Validated += new System.EventHandler(this.DocNsComboBox_Validated);
			// 
			// LblMasterTable
			// 
			resources.ApplyResources(this.LblMasterTable, "LblMasterTable");
			this.LblMasterTable.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblMasterTable.Name = "LblMasterTable";
			// 
			// LblColumns
			// 
			this.LblColumns.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblColumns, "LblColumns");
			this.LblColumns.Name = "LblColumns";
			// 
			// AlertPictureBox
			// 
			resources.ApplyResources(this.AlertPictureBox, "AlertPictureBox");
			this.AlertPictureBox.Name = "AlertPictureBox";
			this.AlertPictureBox.TabStop = false;
			// 
			// SelectMasterInfoPage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.AlertPictureBox);
			this.Controls.Add(this.LblMasterTable);
			this.Controls.Add(this.LblColumns);
			this.Controls.Add(this.LblDocNs);
			this.Controls.Add(this.DocNsComboBox);
			this.Name = "SelectMasterInfoPage";
			this.Load += new System.EventHandler(this.SelectMasterInfoPage_Load);
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.Controls.SetChildIndex(this.DocNsComboBox, 0);
			this.Controls.SetChildIndex(this.LblDocNs, 0);
			this.Controls.SetChildIndex(this.LblColumns, 0);
			this.Controls.SetChildIndex(this.LblMasterTable, 0);
			this.Controls.SetChildIndex(this.AlertPictureBox, 0);
			this.m_headerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AlertPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LblDocNs;
		private System.Windows.Forms.ComboBox DocNsComboBox;
		private System.Windows.Forms.Label LblMasterTable;
		private System.Windows.Forms.Label LblColumns;
		private System.Windows.Forms.PictureBox AlertPictureBox;
	}
}
