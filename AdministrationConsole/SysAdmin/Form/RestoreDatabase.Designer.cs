using Microarea.Console.Plugin.SysAdmin.UserControls;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	partial class RestoreDatabase
    {
		private System.Windows.Forms.Label LabelTitle;
		private System.Windows.Forms.GroupBox ParamsGroupBox;
        private System.Windows.Forms.GroupBox RestoreFilesGroupBox;
        private System.Windows.Forms.Button OKButton;
        private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Label DescriptionLabel;
		private System.Windows.Forms.Button ShowDetailsButton;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RestoreDatabase));
			this.LabelTitle = new System.Windows.Forms.Label();
			this.ParamsGroupBox = new System.Windows.Forms.GroupBox();
			this.ShowDetailsButton = new System.Windows.Forms.Button();
			this.RestoreFilesGroupBox = new System.Windows.Forms.GroupBox();
			this.OKButton = new System.Windows.Forms.Button();
			this.DescriptionLabel = new System.Windows.Forms.Label();
			this.BackupInfoGridUserCtrl = new Microarea.Console.Plugin.SysAdmin.UserControls.BackupInfoGrid();
			this.FileListDataGridView = new System.Windows.Forms.DataGridView();
			this.LoadBackupInfoButton = new System.Windows.Forms.Button();
			this.ParamsGroupBox.SuspendLayout();
			this.RestoreFilesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FileListDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// LabelTitle
			// 
			this.LabelTitle.BackColor = System.Drawing.Color.CornflowerBlue;
			this.LabelTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.LabelTitle, "LabelTitle");
			this.LabelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelTitle.ForeColor = System.Drawing.Color.White;
			this.LabelTitle.Name = "LabelTitle";
			// 
			// ParamsGroupBox
			// 
			resources.ApplyResources(this.ParamsGroupBox, "ParamsGroupBox");
			this.ParamsGroupBox.Controls.Add(this.BackupInfoGridUserCtrl);
			this.ParamsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ParamsGroupBox.Name = "ParamsGroupBox";
			this.ParamsGroupBox.TabStop = false;
			// 
			// ShowDetailsButton
			// 
			resources.ApplyResources(this.ShowDetailsButton, "ShowDetailsButton");
			this.ShowDetailsButton.Name = "ShowDetailsButton";
			this.ShowDetailsButton.Click += new System.EventHandler(this.ShowDetailsButton_Click);
			// 
			// RestoreFilesGroupBox
			// 
			resources.ApplyResources(this.RestoreFilesGroupBox, "RestoreFilesGroupBox");
			this.RestoreFilesGroupBox.Controls.Add(this.LoadBackupInfoButton);
			this.RestoreFilesGroupBox.Controls.Add(this.FileListDataGridView);
			this.RestoreFilesGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RestoreFilesGroupBox.Name = "RestoreFilesGroupBox";
			this.RestoreFilesGroupBox.TabStop = false;
			// 
			// OKButton
			// 
			resources.ApplyResources(this.OKButton, "OKButton");
			this.OKButton.Name = "OKButton";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// DescriptionLabel
			// 
			resources.ApplyResources(this.DescriptionLabel, "DescriptionLabel");
			this.DescriptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DescriptionLabel.Name = "DescriptionLabel";
			// 
			// BackupInfoGridUserCtrl
			// 
			resources.ApplyResources(this.BackupInfoGridUserCtrl, "BackupInfoGridUserCtrl");
			this.BackupInfoGridUserCtrl.BakDataGridView_DataSource = null;
			this.BackupInfoGridUserCtrl.Name = "BackupInfoGridUserCtrl";
			// 
			// FileListDataGridView
			// 
			this.FileListDataGridView.AllowUserToAddRows = false;
			this.FileListDataGridView.AllowUserToDeleteRows = false;
			resources.ApplyResources(this.FileListDataGridView, "FileListDataGridView");
			this.FileListDataGridView.BackgroundColor = System.Drawing.Color.Lavender;
			this.FileListDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.FileListDataGridView.MultiSelect = false;
			this.FileListDataGridView.Name = "FileListDataGridView";
			this.FileListDataGridView.RowHeadersVisible = false;
			// 
			// LoadBackupInfoButton
			// 
			resources.ApplyResources(this.LoadBackupInfoButton, "LoadBackupInfoButton");
			this.LoadBackupInfoButton.Name = "LoadBackupInfoButton";
			this.LoadBackupInfoButton.UseVisualStyleBackColor = true;
			this.LoadBackupInfoButton.Click += new System.EventHandler(this.LoadBackupInfoButton_Click);
			// 
			// RestoreDatabase
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.ShowDetailsButton);
			this.Controls.Add(this.DescriptionLabel);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.RestoreFilesGroupBox);
			this.Controls.Add(this.ParamsGroupBox);
			this.Controls.Add(this.LabelTitle);
			this.Name = "RestoreDatabase";
			this.Load += new System.EventHandler(this.RestoreDatabase_Load);
			this.ParentChanged += new System.EventHandler(this.RestoreDatabase_ParentChanged);
			this.ParamsGroupBox.ResumeLayout(false);
			this.RestoreFilesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.FileListDataGridView)).EndInit();
			this.ResumeLayout(false);

        }
        #endregion

		private BackupInfoGrid BackupInfoGridUserCtrl;
		private System.Windows.Forms.DataGridView FileListDataGridView;
		private System.Windows.Forms.Button LoadBackupInfoButton;

	}
}
