namespace Microarea.EasyAttachment.UI.Forms
{
	partial class Acquisition
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Acquisition));
            this.FileGroupBox = new System.Windows.Forms.GroupBox();
            this.GbMultiFileOptions = new System.Windows.Forms.GroupBox();
            this.RBBarcode = new System.Windows.Forms.RadioButton();
            this.RBSeparatorSheet = new System.Windows.Forms.RadioButton();
            this.RBCreateSeparateFile = new System.Windows.Forms.RadioButton();
            this.FileTypeLabel = new System.Windows.Forms.Label();
            this.FileNameTextBox = new System.Windows.Forms.TextBox();
            this.FileNameLabel = new System.Windows.Forms.Label();
            this.CBCreateSeparateFile = new System.Windows.Forms.CheckBox();
            this.FileTypeComboBox = new Microarea.EasyAttachment.UI.Controls.ExtensionImageComboBox();
            this.ScanButton = new System.Windows.Forms.Button();
            this.SourceGroupBox = new System.Windows.Forms.GroupBox();
            this.DefaultSourceNameLabel = new System.Windows.Forms.Label();
            this.ChangeSourceButton = new System.Windows.Forms.Button();
            this.SourceTextBox = new System.Windows.Forms.TextBox();
            this.CancButton = new System.Windows.Forms.Button();
            this.FileGroupBox.SuspendLayout();
            this.GbMultiFileOptions.SuspendLayout();
            this.SourceGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // FileGroupBox
            // 
            this.FileGroupBox.Controls.Add(this.GbMultiFileOptions);
            this.FileGroupBox.Controls.Add(this.FileTypeLabel);
            this.FileGroupBox.Controls.Add(this.FileNameTextBox);
            this.FileGroupBox.Controls.Add(this.FileNameLabel);
            this.FileGroupBox.Controls.Add(this.CBCreateSeparateFile);
            this.FileGroupBox.Controls.Add(this.FileTypeComboBox);
            this.FileGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.FileGroupBox, "FileGroupBox");
            this.FileGroupBox.Name = "FileGroupBox";
            this.FileGroupBox.TabStop = false;
            // 
            // GbMultiFileOptions
            // 
            this.GbMultiFileOptions.Controls.Add(this.RBBarcode);
            this.GbMultiFileOptions.Controls.Add(this.RBSeparatorSheet);
            this.GbMultiFileOptions.Controls.Add(this.RBCreateSeparateFile);
            resources.ApplyResources(this.GbMultiFileOptions, "GbMultiFileOptions");
            this.GbMultiFileOptions.Name = "GbMultiFileOptions";
            this.GbMultiFileOptions.TabStop = false;
            this.GbMultiFileOptions.EnabledChanged += new System.EventHandler(this.GbMultiFileOptions_EnabledChanged);
            // 
            // RBBarcode
            // 
            resources.ApplyResources(this.RBBarcode, "RBBarcode");
            this.RBBarcode.Name = "RBBarcode";
            this.RBBarcode.TabStop = true;
            this.RBBarcode.UseVisualStyleBackColor = true;
            // 
            // RBSeparatorSheet
            // 
            resources.ApplyResources(this.RBSeparatorSheet, "RBSeparatorSheet");
            this.RBSeparatorSheet.Name = "RBSeparatorSheet";
            this.RBSeparatorSheet.TabStop = true;
            this.RBSeparatorSheet.UseVisualStyleBackColor = true;
            // 
            // RBCreateSeparateFile
            // 
            resources.ApplyResources(this.RBCreateSeparateFile, "RBCreateSeparateFile");
            this.RBCreateSeparateFile.Name = "RBCreateSeparateFile";
            this.RBCreateSeparateFile.TabStop = true;
            this.RBCreateSeparateFile.UseVisualStyleBackColor = true;
            // 
            // FileTypeLabel
            // 
            resources.ApplyResources(this.FileTypeLabel, "FileTypeLabel");
            this.FileTypeLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.FileTypeLabel.Name = "FileTypeLabel";
            // 
            // FileNameTextBox
            // 
            resources.ApplyResources(this.FileNameTextBox, "FileNameTextBox");
            this.FileNameTextBox.Name = "FileNameTextBox";
            // 
            // FileNameLabel
            // 
            resources.ApplyResources(this.FileNameLabel, "FileNameLabel");
            this.FileNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.FileNameLabel.Name = "FileNameLabel";
            // 
            // CBCreateSeparateFile
            // 
            resources.ApplyResources(this.CBCreateSeparateFile, "CBCreateSeparateFile");
            this.CBCreateSeparateFile.Name = "CBCreateSeparateFile";
            this.CBCreateSeparateFile.UseVisualStyleBackColor = true;
            this.CBCreateSeparateFile.CheckedChanged += new System.EventHandler(this.CBCreateSeparateFile_CheckedChanged);
            this.CBCreateSeparateFile.EnabledChanged += new System.EventHandler(this.CBCreateSeparateFile_EnabledChanged);
            // 
            // FileTypeComboBox
            // 
            this.FileTypeComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.FileTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FileTypeComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.FileTypeComboBox, "FileTypeComboBox");
            this.FileTypeComboBox.Name = "FileTypeComboBox";
            this.FileTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.FileTypeComboBox_SelectedIndexChanged);
            // 
            // ScanButton
            // 
            this.ScanButton.Image = global::Microarea.EasyAttachment.Properties.Resources.Scanner_24x24;
            resources.ApplyResources(this.ScanButton, "ScanButton");
            this.ScanButton.Name = "ScanButton";
            this.ScanButton.UseVisualStyleBackColor = true;
            this.ScanButton.Click += new System.EventHandler(this.ScanButton_Click);
            // 
            // SourceGroupBox
            // 
            this.SourceGroupBox.Controls.Add(this.DefaultSourceNameLabel);
            this.SourceGroupBox.Controls.Add(this.ChangeSourceButton);
            this.SourceGroupBox.Controls.Add(this.SourceTextBox);
            this.SourceGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.SourceGroupBox, "SourceGroupBox");
            this.SourceGroupBox.Name = "SourceGroupBox";
            this.SourceGroupBox.TabStop = false;
            // 
            // DefaultSourceNameLabel
            // 
            resources.ApplyResources(this.DefaultSourceNameLabel, "DefaultSourceNameLabel");
            this.DefaultSourceNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DefaultSourceNameLabel.Name = "DefaultSourceNameLabel";
            // 
            // ChangeSourceButton
            // 
            resources.ApplyResources(this.ChangeSourceButton, "ChangeSourceButton");
            this.ChangeSourceButton.Name = "ChangeSourceButton";
            this.ChangeSourceButton.UseVisualStyleBackColor = true;
            this.ChangeSourceButton.Click += new System.EventHandler(this.ChangeSourceButton_Click);
            // 
            // SourceTextBox
            // 
            resources.ApplyResources(this.SourceTextBox, "SourceTextBox");
            this.SourceTextBox.Name = "SourceTextBox";
            // 
            // CancButton
            // 
            this.CancButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.CancButton, "CancButton");
            this.CancButton.Name = "CancButton";
            this.CancButton.UseVisualStyleBackColor = true;
            // 
            // Acquisition
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.CancelButton = this.CancButton;
            this.Controls.Add(this.CancButton);
            this.Controls.Add(this.SourceGroupBox);
            this.Controls.Add(this.ScanButton);
            this.Controls.Add(this.FileGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Acquisition";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Acquisition_FormClosed);
            this.FileGroupBox.ResumeLayout(false);
            this.FileGroupBox.PerformLayout();
            this.GbMultiFileOptions.ResumeLayout(false);
            this.SourceGroupBox.ResumeLayout(false);
            this.SourceGroupBox.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox FileGroupBox;
		private Microarea.EasyAttachment.UI.Controls.ExtensionImageComboBox FileTypeComboBox;
		private System.Windows.Forms.CheckBox CBCreateSeparateFile;
		private System.Windows.Forms.Button ScanButton;
		private System.Windows.Forms.GroupBox SourceGroupBox;
		private System.Windows.Forms.Button ChangeSourceButton;
		private System.Windows.Forms.TextBox SourceTextBox;
		private System.Windows.Forms.TextBox FileNameTextBox;
		private System.Windows.Forms.Label FileNameLabel;
		private System.Windows.Forms.Label FileTypeLabel;
		private System.Windows.Forms.Label DefaultSourceNameLabel;
        private System.Windows.Forms.Button CancButton;
        private System.Windows.Forms.GroupBox GbMultiFileOptions;
        private System.Windows.Forms.RadioButton RBBarcode;
        private System.Windows.Forms.RadioButton RBSeparatorSheet;
        private System.Windows.Forms.RadioButton RBCreateSeparateFile;
	}
}