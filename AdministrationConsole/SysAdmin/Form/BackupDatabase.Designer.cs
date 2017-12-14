using Microarea.Console.Plugin.SysAdmin.UserControls;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	partial class BackupDatabase
    {
        private System.Windows.Forms.Label LabelTitle;
        private System.Windows.Forms.Label DatabaseLabel;
		private System.Windows.Forms.GroupBox ParamsGroupBox;
		private System.Windows.Forms.CheckBox VerifyBackupCheckBox;
        private System.Windows.Forms.Button OKButton;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.Label DatabaseTextBox;
        private System.Windows.Forms.GroupBox DBInfoGroupBox;
        private System.Windows.Forms.Label BkpDescriLabel;
        private System.Windows.Forms.Label LastBkpDateLabel;
        private System.Windows.Forms.Label BkpDescriTextBox;
        private System.Windows.Forms.Label LastBkpDateTextBox;
        private System.Windows.Forms.ToolTip FilePathToolTip;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackupDatabase));
			this.LabelTitle = new System.Windows.Forms.Label();
			this.DatabaseLabel = new System.Windows.Forms.Label();
			this.DatabaseTextBox = new System.Windows.Forms.Label();
			this.ParamsGroupBox = new System.Windows.Forms.GroupBox();
			this.OverwriteFileCheckBox = new System.Windows.Forms.CheckBox();
			this.ShowDetailsButton = new System.Windows.Forms.Button();
			this.BackupInfoGridUserCtrl = new Microarea.Console.Plugin.SysAdmin.UserControls.BackupInfoGrid();
			this.VerifyBackupCheckBox = new System.Windows.Forms.CheckBox();
			this.OKButton = new System.Windows.Forms.Button();
			this.DescriptionLabel = new System.Windows.Forms.Label();
			this.DBInfoGroupBox = new System.Windows.Forms.GroupBox();
			this.LastBkpDateTextBox = new System.Windows.Forms.Label();
			this.BkpDescriLabel = new System.Windows.Forms.Label();
			this.LastBkpDateLabel = new System.Windows.Forms.Label();
			this.BkpDescriTextBox = new System.Windows.Forms.Label();
			this.FilePathToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.ParamsGroupBox.SuspendLayout();
			this.DBInfoGroupBox.SuspendLayout();
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
			// DatabaseLabel
			// 
			resources.ApplyResources(this.DatabaseLabel, "DatabaseLabel");
			this.DatabaseLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DatabaseLabel.Name = "DatabaseLabel";
			// 
			// DatabaseTextBox
			// 
			resources.ApplyResources(this.DatabaseTextBox, "DatabaseTextBox");
			this.DatabaseTextBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DatabaseTextBox.Name = "DatabaseTextBox";
			// 
			// ParamsGroupBox
			// 
			resources.ApplyResources(this.ParamsGroupBox, "ParamsGroupBox");
			this.ParamsGroupBox.Controls.Add(this.OverwriteFileCheckBox);
			this.ParamsGroupBox.Controls.Add(this.ShowDetailsButton);
			this.ParamsGroupBox.Controls.Add(this.BackupInfoGridUserCtrl);
			this.ParamsGroupBox.Controls.Add(this.VerifyBackupCheckBox);
			this.ParamsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ParamsGroupBox.Name = "ParamsGroupBox";
			this.ParamsGroupBox.TabStop = false;
			// 
			// OverwriteFileCheckBox
			// 
			resources.ApplyResources(this.OverwriteFileCheckBox, "OverwriteFileCheckBox");
			this.OverwriteFileCheckBox.Name = "OverwriteFileCheckBox";
			this.OverwriteFileCheckBox.MouseHover += new System.EventHandler(this.OverwriteFileCheckBox_MouseHover);
			// 
			// ShowDetailsButton
			// 
			resources.ApplyResources(this.ShowDetailsButton, "ShowDetailsButton");
			this.ShowDetailsButton.Name = "ShowDetailsButton";
			this.ShowDetailsButton.Click += new System.EventHandler(this.ShowDetailsButton_Click);
			// 
			// BackupInfoGridUserCtrl
			// 
			resources.ApplyResources(this.BackupInfoGridUserCtrl, "BackupInfoGridUserCtrl");
			this.BackupInfoGridUserCtrl.BakDataGridView_DataSource = null;
			this.BackupInfoGridUserCtrl.Name = "BackupInfoGridUserCtrl";
			// 
			// VerifyBackupCheckBox
			// 
			resources.ApplyResources(this.VerifyBackupCheckBox, "VerifyBackupCheckBox");
			this.VerifyBackupCheckBox.Name = "VerifyBackupCheckBox";
			this.VerifyBackupCheckBox.MouseHover += new System.EventHandler(this.VerifyBackupCheckBox_MouseHover);
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
			// DBInfoGroupBox
			// 
			resources.ApplyResources(this.DBInfoGroupBox, "DBInfoGroupBox");
			this.DBInfoGroupBox.Controls.Add(this.DatabaseLabel);
			this.DBInfoGroupBox.Controls.Add(this.LastBkpDateTextBox);
			this.DBInfoGroupBox.Controls.Add(this.BkpDescriLabel);
			this.DBInfoGroupBox.Controls.Add(this.LastBkpDateLabel);
			this.DBInfoGroupBox.Controls.Add(this.BkpDescriTextBox);
			this.DBInfoGroupBox.Controls.Add(this.DatabaseTextBox);
			this.DBInfoGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DBInfoGroupBox.Name = "DBInfoGroupBox";
			this.DBInfoGroupBox.TabStop = false;
			// 
			// LastBkpDateTextBox
			// 
			resources.ApplyResources(this.LastBkpDateTextBox, "LastBkpDateTextBox");
			this.LastBkpDateTextBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LastBkpDateTextBox.Name = "LastBkpDateTextBox";
			// 
			// BkpDescriLabel
			// 
			resources.ApplyResources(this.BkpDescriLabel, "BkpDescriLabel");
			this.BkpDescriLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BkpDescriLabel.Name = "BkpDescriLabel";
			// 
			// LastBkpDateLabel
			// 
			resources.ApplyResources(this.LastBkpDateLabel, "LastBkpDateLabel");
			this.LastBkpDateLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LastBkpDateLabel.Name = "LastBkpDateLabel";
			// 
			// BkpDescriTextBox
			// 
			resources.ApplyResources(this.BkpDescriTextBox, "BkpDescriTextBox");
			this.BkpDescriTextBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BkpDescriTextBox.Name = "BkpDescriTextBox";
			// 
			// FilePathToolTip
			// 
			this.FilePathToolTip.AutoPopDelay = 5000;
			this.FilePathToolTip.InitialDelay = 200;
			this.FilePathToolTip.ReshowDelay = 100;
			// 
			// BackupDatabase
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.DescriptionLabel);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.ParamsGroupBox);
			this.Controls.Add(this.LabelTitle);
			this.Controls.Add(this.DBInfoGroupBox);
			this.Name = "BackupDatabase";
			this.Load += new System.EventHandler(this.BackupDatabase_Load);
			this.ParentChanged += new System.EventHandler(this.BackupDatabase_ParentChanged);
			this.ParamsGroupBox.ResumeLayout(false);
			this.DBInfoGroupBox.ResumeLayout(false);
			this.DBInfoGroupBox.PerformLayout();
			this.ResumeLayout(false);

        }
        #endregion

		private BackupInfoGrid BackupInfoGridUserCtrl;
		private System.Windows.Forms.CheckBox OverwriteFileCheckBox;


	}
}
