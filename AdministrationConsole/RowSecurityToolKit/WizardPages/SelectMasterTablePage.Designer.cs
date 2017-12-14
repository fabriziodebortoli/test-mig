namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	partial class SelectMasterTablePage
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectMasterTablePage));
			this.LblMasterTable = new System.Windows.Forms.Label();
			this.MasterTableComboBox = new System.Windows.Forms.ComboBox();
			this.AlertPictureBox = new System.Windows.Forms.PictureBox();
			this.LblColumns = new System.Windows.Forms.Label();
			this.ColumnsListBox = new System.Windows.Forms.CheckedListBox();
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
			// LblMasterTable
			// 
			resources.ApplyResources(this.LblMasterTable, "LblMasterTable");
			this.LblMasterTable.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblMasterTable.Name = "LblMasterTable";
			// 
			// MasterTableComboBox
			// 
			this.MasterTableComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.MasterTableComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.MasterTableComboBox.FormattingEnabled = true;
			resources.ApplyResources(this.MasterTableComboBox, "MasterTableComboBox");
			this.MasterTableComboBox.Name = "MasterTableComboBox";
			this.MasterTableComboBox.SelectedIndexChanged += new System.EventHandler(this.MasterTableComboBox_SelectedIndexChanged);
			this.MasterTableComboBox.Validated += new System.EventHandler(this.MasterTableComboBox_Validated);
			// 
			// AlertPictureBox
			// 
			resources.ApplyResources(this.AlertPictureBox, "AlertPictureBox");
			this.AlertPictureBox.Name = "AlertPictureBox";
			this.AlertPictureBox.TabStop = false;
			// 
			// LblColumns
			// 
			resources.ApplyResources(this.LblColumns, "LblColumns");
			this.LblColumns.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblColumns.Name = "LblColumns";
			// 
			// ColumnsListBox
			// 
			this.ColumnsListBox.BackColor = System.Drawing.SystemColors.Window;
			this.ColumnsListBox.CheckOnClick = true;
			resources.ApplyResources(this.ColumnsListBox, "ColumnsListBox");
			this.ColumnsListBox.FormattingEnabled = true;
			this.ColumnsListBox.Name = "ColumnsListBox";
			this.ColumnsListBox.ThreeDCheckBoxes = true;
			// 
			// SelectMasterTablePage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ColumnsListBox);
			this.Controls.Add(this.LblColumns);
			this.Controls.Add(this.LblMasterTable);
			this.Controls.Add(this.AlertPictureBox);
			this.Controls.Add(this.MasterTableComboBox);
			this.Name = "SelectMasterTablePage";
			this.Load += new System.EventHandler(this.SelectMasterTablePage_Load);
			this.Controls.SetChildIndex(this.MasterTableComboBox, 0);
			this.Controls.SetChildIndex(this.AlertPictureBox, 0);
			this.Controls.SetChildIndex(this.LblMasterTable, 0);
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.Controls.SetChildIndex(this.LblColumns, 0);
			this.Controls.SetChildIndex(this.ColumnsListBox, 0);
			this.m_headerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AlertPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LblMasterTable;
		private System.Windows.Forms.ComboBox MasterTableComboBox;
		private System.Windows.Forms.PictureBox AlertPictureBox;
		private System.Windows.Forms.Label LblColumns;
		private System.Windows.Forms.CheckedListBox ColumnsListBox;
	}
}
