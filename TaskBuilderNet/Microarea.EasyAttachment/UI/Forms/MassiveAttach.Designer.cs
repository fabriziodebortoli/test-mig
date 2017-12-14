namespace Microarea.EasyAttachment.UI.Forms
{
    partial class MassiveAttach
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MassiveAttach));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripDropDownButtonAdd = new System.Windows.Forms.ToolStripDropDownButton();
			this.repositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fileSystemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.TsbDelete = new System.Windows.Forms.ToolStripButton();
			this.TsbRefresh = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.BtnClose = new System.Windows.Forms.Button();
			this.LblProcEnded = new System.Windows.Forms.Label();
			this.BtnOK = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.LblSuccess = new System.Windows.Forms.Label();
			this.PbSuccess = new System.Windows.Forms.PictureBox();
			this.PbDuplicateBC = new System.Windows.Forms.PictureBox();
			this.LblDuplicateBC = new System.Windows.Forms.Label();
			this.LblInfo = new System.Windows.Forms.Label();
			this.LblError = new System.Windows.Forms.Label();
			this.PbFailed = new System.Windows.Forms.PictureBox();
			this.LblNoBC = new System.Windows.Forms.Label();
			this.PbOnlyBarcode = new System.Windows.Forms.PictureBox();
			this.LblOK = new System.Windows.Forms.Label();
			this.PbWithError = new System.Windows.Forms.PictureBox();
			this.PbPapery = new System.Windows.Forms.PictureBox();
			this.PbNoBc = new System.Windows.Forms.PictureBox();
			this.LblNoPending = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.massiveAttachResult1 = new Microarea.EasyAttachment.UI.Forms.MassiveAttachResult();
			this.toolStrip1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PbSuccess)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PbDuplicateBC)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PbFailed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PbOnlyBarcode)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PbWithError)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PbPapery)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PbNoBc)).BeginInit();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButtonAdd,
            this.TsbDelete,
            this.TsbRefresh,
            this.toolStripButton1});
			resources.ApplyResources(this.toolStrip1, "toolStrip1");
			this.toolStrip1.Name = "toolStrip1";
			// 
			// toolStripDropDownButtonAdd
			// 
			this.toolStripDropDownButtonAdd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.repositoryToolStripMenuItem,
            this.fileSystemToolStripMenuItem,
            this.deviceToolStripMenuItem});
			this.toolStripDropDownButtonAdd.Image = global::Microarea.EasyAttachment.Properties.Resources.Add16x16;
			resources.ApplyResources(this.toolStripDropDownButtonAdd, "toolStripDropDownButtonAdd");
			this.toolStripDropDownButtonAdd.Name = "toolStripDropDownButtonAdd";
			// 
			// repositoryToolStripMenuItem
			// 
			this.repositoryToolStripMenuItem.Image = global::Microarea.EasyAttachment.Properties.Resources.Database16x16;
			this.repositoryToolStripMenuItem.Name = "repositoryToolStripMenuItem";
			resources.ApplyResources(this.repositoryToolStripMenuItem, "repositoryToolStripMenuItem");
			this.repositoryToolStripMenuItem.Click += new System.EventHandler(this.repositoryToolStripMenuItem_Click);
			// 
			// fileSystemToolStripMenuItem
			// 
			this.fileSystemToolStripMenuItem.Image = global::Microarea.EasyAttachment.Properties.Resources.Folder;
			this.fileSystemToolStripMenuItem.Name = "fileSystemToolStripMenuItem";
			resources.ApplyResources(this.fileSystemToolStripMenuItem, "fileSystemToolStripMenuItem");
			this.fileSystemToolStripMenuItem.Click += new System.EventHandler(this.tsBntChooseFiles_Click);
			// 
			// deviceToolStripMenuItem
			// 
			this.deviceToolStripMenuItem.Image = global::Microarea.EasyAttachment.Properties.Resources.Scanner_16x16;
			this.deviceToolStripMenuItem.Name = "deviceToolStripMenuItem";
			resources.ApplyResources(this.deviceToolStripMenuItem, "deviceToolStripMenuItem");
			this.deviceToolStripMenuItem.Click += new System.EventHandler(this.deviceToolStripMenuItem_Click);
			// 
			// TsbDelete
			// 
			resources.ApplyResources(this.TsbDelete, "TsbDelete");
			this.TsbDelete.Image = global::Microarea.EasyAttachment.Properties.Resources.Delete16x16;
			this.TsbDelete.Name = "TsbDelete";
			this.TsbDelete.Click += new System.EventHandler(this.TsbDelete_Click);
			// 
			// TsbRefresh
			// 
			resources.ApplyResources(this.TsbRefresh, "TsbRefresh");
			this.TsbRefresh.Image = global::Microarea.EasyAttachment.Properties.Resources.Refresh16x16;
			this.TsbRefresh.Name = "TsbRefresh";
			this.TsbRefresh.Click += new System.EventHandler(this.TsbRefresh_Click);
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.CheckOnClick = true;
			this.toolStripButton1.Image = global::Microarea.EasyAttachment.Properties.Resources.check_unselected;
			resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
			// 
			// BtnClose
			// 
			resources.ApplyResources(this.BtnClose, "BtnClose");
			this.BtnClose.Image = global::Microarea.EasyAttachment.Properties.Resources.Exit;
			this.BtnClose.Name = "BtnClose";
			this.BtnClose.UseVisualStyleBackColor = true;
			this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
			// 
			// LblProcEnded
			// 
			resources.ApplyResources(this.LblProcEnded, "LblProcEnded");
			this.LblProcEnded.BackColor = System.Drawing.Color.Lavender;
			this.LblProcEnded.ForeColor = System.Drawing.Color.Blue;
			this.LblProcEnded.Name = "LblProcEnded";
			// 
			// BtnOK
			// 
			resources.ApplyResources(this.BtnOK, "BtnOK");
			this.BtnOK.Image = global::Microarea.EasyAttachment.Properties.Resources.paperclip16;
			this.BtnOK.Name = "BtnOK";
			this.BtnOK.UseVisualStyleBackColor = true;
			this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// groupBox1
			// 
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Controls.Add(this.LblSuccess);
			this.groupBox1.Controls.Add(this.PbSuccess);
			this.groupBox1.Controls.Add(this.PbDuplicateBC);
			this.groupBox1.Controls.Add(this.LblDuplicateBC);
			this.groupBox1.Controls.Add(this.LblInfo);
			this.groupBox1.Controls.Add(this.LblError);
			this.groupBox1.Controls.Add(this.PbFailed);
			this.groupBox1.Controls.Add(this.LblNoBC);
			this.groupBox1.Controls.Add(this.PbOnlyBarcode);
			this.groupBox1.Controls.Add(this.LblOK);
			this.groupBox1.Controls.Add(this.PbWithError);
			this.groupBox1.Controls.Add(this.PbPapery);
			this.groupBox1.Controls.Add(this.PbNoBc);
			this.groupBox1.Controls.Add(this.LblNoPending);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// LblSuccess
			// 
			resources.ApplyResources(this.LblSuccess, "LblSuccess");
			this.LblSuccess.Name = "LblSuccess";
			// 
			// PbSuccess
			// 
			resources.ApplyResources(this.PbSuccess, "PbSuccess");
			this.PbSuccess.Name = "PbSuccess";
			this.PbSuccess.TabStop = false;
			// 
			// PbDuplicateBC
			// 
			resources.ApplyResources(this.PbDuplicateBC, "PbDuplicateBC");
			this.PbDuplicateBC.Name = "PbDuplicateBC";
			this.PbDuplicateBC.TabStop = false;
			// 
			// LblDuplicateBC
			// 
			resources.ApplyResources(this.LblDuplicateBC, "LblDuplicateBC");
			this.LblDuplicateBC.Name = "LblDuplicateBC";
			// 
			// LblInfo
			// 
			resources.ApplyResources(this.LblInfo, "LblInfo");
			this.LblInfo.Name = "LblInfo";
			// 
			// LblError
			// 
			resources.ApplyResources(this.LblError, "LblError");
			this.LblError.Name = "LblError";
			// 
			// PbFailed
			// 
			resources.ApplyResources(this.PbFailed, "PbFailed");
			this.PbFailed.Name = "PbFailed";
			this.PbFailed.TabStop = false;
			// 
			// LblNoBC
			// 
			resources.ApplyResources(this.LblNoBC, "LblNoBC");
			this.LblNoBC.Name = "LblNoBC";
			// 
			// PbOnlyBarcode
			// 
			resources.ApplyResources(this.PbOnlyBarcode, "PbOnlyBarcode");
			this.PbOnlyBarcode.Name = "PbOnlyBarcode";
			this.PbOnlyBarcode.TabStop = false;
			// 
			// LblOK
			// 
			resources.ApplyResources(this.LblOK, "LblOK");
			this.LblOK.Name = "LblOK";
			// 
			// PbWithError
			// 
			resources.ApplyResources(this.PbWithError, "PbWithError");
			this.PbWithError.Name = "PbWithError";
			this.PbWithError.TabStop = false;
			// 
			// PbPapery
			// 
			resources.ApplyResources(this.PbPapery, "PbPapery");
			this.PbPapery.Name = "PbPapery";
			this.PbPapery.TabStop = false;
			// 
			// PbNoBc
			// 
			resources.ApplyResources(this.PbNoBc, "PbNoBc");
			this.PbNoBc.Name = "PbNoBc";
			this.PbNoBc.TabStop = false;
			// 
			// LblNoPending
			// 
			resources.ApplyResources(this.LblNoPending, "LblNoPending");
			this.LblNoPending.Name = "LblNoPending";
			// 
			// progressBar1
			// 
			resources.ApplyResources(this.progressBar1, "progressBar1");
			this.progressBar1.Name = "progressBar1";
			// 
			// massiveAttachResult1
			// 
			resources.ApplyResources(this.massiveAttachResult1, "massiveAttachResult1");
			this.massiveAttachResult1.BackColor = System.Drawing.Color.Lavender;
			this.massiveAttachResult1.Name = "massiveAttachResult1";
			// 
			// MassiveAttach
			// 
			this.AllowDrop = true;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.massiveAttachResult1);
			this.Controls.Add(this.BtnOK);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.LblProcEnded);
			this.Controls.Add(this.BtnClose);
			this.Controls.Add(this.toolStrip1);
			this.Name = "MassiveAttach";
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MassiveAttach_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MassiveAttach_DragEnter);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PbSuccess)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PbDuplicateBC)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PbFailed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PbOnlyBarcode)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PbWithError)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PbPapery)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PbNoBc)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.Button BtnClose;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonAdd;
        private System.Windows.Forms.ToolStripMenuItem repositoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileSystemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deviceToolStripMenuItem;
        private System.Windows.Forms.Label LblProcEnded;
        private System.Windows.Forms.Button BtnOK;
        private MassiveAttachResult massiveAttachResult1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label LblNoBC;
		private System.Windows.Forms.PictureBox PbOnlyBarcode;
        private System.Windows.Forms.Label LblOK;
        private System.Windows.Forms.PictureBox PbPapery;
        private System.Windows.Forms.PictureBox PbNoBc;
        private System.Windows.Forms.Label LblNoPending;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.PictureBox PbDuplicateBC;
        private System.Windows.Forms.Label LblError;
		private System.Windows.Forms.PictureBox PbFailed;
        private System.Windows.Forms.Label LblDuplicateBC;
        private System.Windows.Forms.Label LblInfo;
        private System.Windows.Forms.PictureBox PbWithError;
        private System.Windows.Forms.ToolStripButton TsbDelete;
        private System.Windows.Forms.ToolStripButton TsbRefresh;
		private System.Windows.Forms.PictureBox PbSuccess;
		private System.Windows.Forms.Label LblSuccess;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
    }
}