namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	partial class SelectEntityRelationsPage
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectEntityRelationsPage));
			this.LblMasterTable = new System.Windows.Forms.Label();
			this.LblColumns = new System.Windows.Forms.Label();
			this.DGVRelations = new System.Windows.Forms.DataGridView();
			this.SelDeselCBox = new System.Windows.Forms.CheckBox();
			this.EncryptFilesCheckBox = new System.Windows.Forms.CheckBox();
			this.m_headerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DGVRelations)).BeginInit();
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
			// LblColumns
			// 
			this.LblColumns.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblColumns, "LblColumns");
			this.LblColumns.Name = "LblColumns";
			// 
			// DGVRelations
			// 
			this.DGVRelations.AllowUserToDeleteRows = false;
			this.DGVRelations.BackgroundColor = System.Drawing.SystemColors.Window;
			this.DGVRelations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			resources.ApplyResources(this.DGVRelations, "DGVRelations");
			this.DGVRelations.Name = "DGVRelations";
			this.DGVRelations.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGVRelations_CellClick);
			this.DGVRelations.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGVRelations_CellEndEdit);
			this.DGVRelations.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DGVRelations_DataError);
			// 
			// SelDeselCBox
			// 
			resources.ApplyResources(this.SelDeselCBox, "SelDeselCBox");
			this.SelDeselCBox.Image = global::Microarea.Console.Plugin.RowSecurityToolKit.Strings.Check_selected;
			this.SelDeselCBox.Name = "SelDeselCBox";
			this.SelDeselCBox.UseVisualStyleBackColor = true;
			this.SelDeselCBox.CheckedChanged += new System.EventHandler(this.SelDeselCBox_CheckedChanged);
			// 
			// EncryptFilesCheckBox
			// 
			resources.ApplyResources(this.EncryptFilesCheckBox, "EncryptFilesCheckBox");
			this.EncryptFilesCheckBox.Name = "EncryptFilesCheckBox";
			this.EncryptFilesCheckBox.UseVisualStyleBackColor = true;
			// 
			// SelectEntityRelationsPage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.LblMasterTable);
			this.Controls.Add(this.SelDeselCBox);
			this.Controls.Add(this.LblColumns);
			this.Controls.Add(this.DGVRelations);
			this.Controls.Add(this.EncryptFilesCheckBox);
			this.Name = "SelectEntityRelationsPage";
			this.Controls.SetChildIndex(this.EncryptFilesCheckBox, 0);
			this.Controls.SetChildIndex(this.DGVRelations, 0);
			this.Controls.SetChildIndex(this.LblColumns, 0);
			this.Controls.SetChildIndex(this.SelDeselCBox, 0);
			this.Controls.SetChildIndex(this.LblMasterTable, 0);
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.m_headerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DGVRelations)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LblMasterTable;
		private System.Windows.Forms.Label LblColumns;
		private System.Windows.Forms.DataGridView DGVRelations;
		private System.Windows.Forms.CheckBox SelDeselCBox;
		private System.Windows.Forms.CheckBox EncryptFilesCheckBox;
	}
}
