namespace Microarea.Console.Plugin.HermesAdmin
{
    partial class LogSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogSettingsForm));
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnUndo = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.chkLogging = new System.Windows.Forms.CheckBox();
            this.grpLogging = new System.Windows.Forms.GroupBox();
            this.btnBrowseLog = new System.Windows.Forms.Button();
            this.txtLogPath = new System.Windows.Forms.TextBox();
            this.chkLimilabsLog = new System.Windows.Forms.CheckBox();
            this.lblLogPath = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.grpLogging.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            resources.ApplyResources(this.lblTitle, "lblTitle");
            this.lblTitle.BackColor = System.Drawing.Color.CornflowerBlue;
            this.lblTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Name = "lblTitle";
            // 
            // btnUndo
            // 
            resources.ApplyResources(this.btnUndo, "btnUndo");
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.UseVisualStyleBackColor = true;
            this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // chkLogging
            // 
            resources.ApplyResources(this.chkLogging, "chkLogging");
            this.chkLogging.Name = "chkLogging";
            this.chkLogging.UseVisualStyleBackColor = true;
            this.chkLogging.CheckedChanged += new System.EventHandler(this.chkLogging_CheckedChanged);
            // 
            // grpLogging
            // 
            this.grpLogging.Controls.Add(this.btnBrowseLog);
            this.grpLogging.Controls.Add(this.txtLogPath);
            this.grpLogging.Controls.Add(this.chkLimilabsLog);
            this.grpLogging.Controls.Add(this.lblLogPath);
            resources.ApplyResources(this.grpLogging, "grpLogging");
            this.grpLogging.Name = "grpLogging";
            this.grpLogging.TabStop = false;
            // 
            // btnBrowseLog
            // 
            resources.ApplyResources(this.btnBrowseLog, "btnBrowseLog");
            this.btnBrowseLog.Name = "btnBrowseLog";
            this.btnBrowseLog.UseVisualStyleBackColor = true;
            this.btnBrowseLog.Click += new System.EventHandler(this.btnBrowseLog_Click);
            // 
            // txtLogPath
            // 
            resources.ApplyResources(this.txtLogPath, "txtLogPath");
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.TextChanged += new System.EventHandler(this.txtLogPath_TextChanged);
            this.txtLogPath.Leave += new System.EventHandler(this.txtLogPath_Leave);
            // 
            // chkLimilabsLog
            // 
            resources.ApplyResources(this.chkLimilabsLog, "chkLimilabsLog");
            this.chkLimilabsLog.Name = "chkLimilabsLog";
            this.chkLimilabsLog.UseVisualStyleBackColor = true;
            this.chkLimilabsLog.CheckedChanged += new System.EventHandler(this.chkLimilabsLog_CheckedChanged);
            // 
            // lblLogPath
            // 
            resources.ApplyResources(this.lblLogPath, "lblLogPath");
            this.lblLogPath.Name = "lblLogPath";
            // 
            // LogSettingsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkLogging);
            this.Controls.Add(this.grpLogging);
            this.Controls.Add(this.btnUndo);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LogSettingsForm";
            this.Load += new System.EventHandler(this.LogSettingsForm_Load);
            this.grpLogging.ResumeLayout(false);
            this.grpLogging.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox chkLogging;
        private System.Windows.Forms.GroupBox grpLogging;
        private System.Windows.Forms.TextBox txtLogPath;
        private System.Windows.Forms.CheckBox chkLimilabsLog;
        private System.Windows.Forms.Label lblLogPath;
        private System.Windows.Forms.Button btnBrowseLog;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
	}
}