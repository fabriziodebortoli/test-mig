namespace Microarea.EasyAttachment.UI.Forms
{
	partial class QuickAttachment
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickAttachment));
            this.TSContainer = new System.Windows.Forms.ToolStripContainer();
            this.DGVDocuments = new System.Windows.Forms.DataGridView();
            this.TSActions = new System.Windows.Forms.ToolStrip();
            this.TSBtnAdd = new System.Windows.Forms.ToolStripDropDownButton();
            this.TSRepository = new System.Windows.Forms.ToolStripMenuItem();
            this.TSFileSystem = new System.Windows.Forms.ToolStripMenuItem();
            this.TSDevice = new System.Windows.Forms.ToolStripMenuItem();
            this.TSSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.TSDelete = new System.Windows.Forms.ToolStripButton();
            this.TSClearAll = new System.Windows.Forms.ToolStripButton();
            this.TSSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.TSModify = new System.Windows.Forms.ToolStripButton();
            this.TSSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.TSAttach = new System.Windows.Forms.ToolStripButton();
            this.QASplitContainer = new System.Windows.Forms.SplitContainer();
            this.BtnApply = new System.Windows.Forms.Button();
            this.TxtHeading = new System.Windows.Forms.TextBox();
            this.LblHeading = new System.Windows.Forms.Label();
            this.TSContainer.ContentPanel.SuspendLayout();
            this.TSContainer.TopToolStripPanel.SuspendLayout();
            this.TSContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGVDocuments)).BeginInit();
            this.TSActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.QASplitContainer)).BeginInit();
            this.QASplitContainer.Panel1.SuspendLayout();
            this.QASplitContainer.Panel2.SuspendLayout();
            this.QASplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // TSContainer
            // 
            // 
            // TSContainer.ContentPanel
            // 
            this.TSContainer.ContentPanel.Controls.Add(this.DGVDocuments);
            resources.ApplyResources(this.TSContainer.ContentPanel, "TSContainer.ContentPanel");
            resources.ApplyResources(this.TSContainer, "TSContainer");
            this.TSContainer.Name = "TSContainer";
            // 
            // TSContainer.TopToolStripPanel
            // 
            this.TSContainer.TopToolStripPanel.Controls.Add(this.TSActions);
            // 
            // DGVDocuments
            // 
            this.DGVDocuments.AllowDrop = true;
            this.DGVDocuments.AllowUserToAddRows = false;
            this.DGVDocuments.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DGVDocuments.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(this.DGVDocuments, "DGVDocuments");
            this.DGVDocuments.Name = "DGVDocuments";
            this.DGVDocuments.RowHeadersVisible = false;
            this.DGVDocuments.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGVDocuments.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGVDocuments_CellContentClick);
            this.DGVDocuments.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGVDocuments_CellContentDoubleClick);
            this.DGVDocuments.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DGVDocuments_CellFormatting);
            this.DGVDocuments.DragDrop += new System.Windows.Forms.DragEventHandler(this.DGVDocuments_DragDrop);
            this.DGVDocuments.DragEnter += new System.Windows.Forms.DragEventHandler(this.DGVDocuments_DragEnter);
            // 
            // TSActions
            // 
            resources.ApplyResources(this.TSActions, "TSActions");
            this.TSActions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.TSActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSBtnAdd,
            this.TSSeparator1,
            this.TSDelete,
            this.TSClearAll,
            this.TSSeparator2,
            this.TSModify,
            this.TSSeparator3,
            this.TSAttach});
            this.TSActions.Name = "TSActions";
            // 
            // TSBtnAdd
            // 
            this.TSBtnAdd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSRepository,
            this.TSFileSystem,
            this.TSDevice});
            this.TSBtnAdd.Image = global::Microarea.EasyAttachment.Properties.Resources.Add16x16;
            resources.ApplyResources(this.TSBtnAdd, "TSBtnAdd");
            this.TSBtnAdd.Name = "TSBtnAdd";
            // 
            // TSRepository
            // 
            this.TSRepository.Image = global::Microarea.EasyAttachment.Properties.Resources.Database16x16;
            this.TSRepository.Name = "TSRepository";
            resources.ApplyResources(this.TSRepository, "TSRepository");
            this.TSRepository.Click += new System.EventHandler(this.TSRepository_Click);
            // 
            // TSFileSystem
            // 
            this.TSFileSystem.Image = global::Microarea.EasyAttachment.Properties.Resources.Folder;
            this.TSFileSystem.Name = "TSFileSystem";
            resources.ApplyResources(this.TSFileSystem, "TSFileSystem");
            this.TSFileSystem.Click += new System.EventHandler(this.TSFileSystem_Click);
            // 
            // TSDevice
            // 
            this.TSDevice.Image = global::Microarea.EasyAttachment.Properties.Resources.Scanner_16x16;
            this.TSDevice.Name = "TSDevice";
            resources.ApplyResources(this.TSDevice, "TSDevice");
            this.TSDevice.Click += new System.EventHandler(this.TSDevice_Click);
            // 
            // TSSeparator1
            // 
            this.TSSeparator1.Name = "TSSeparator1";
            resources.ApplyResources(this.TSSeparator1, "TSSeparator1");
            // 
            // TSDelete
            // 
            this.TSDelete.Image = global::Microarea.EasyAttachment.Properties.Resources.Delete16x16;
            resources.ApplyResources(this.TSDelete, "TSDelete");
            this.TSDelete.Name = "TSDelete";
            this.TSDelete.Click += new System.EventHandler(this.TSDelete_Click);
            // 
            // TSClearAll
            // 
            this.TSClearAll.Image = global::Microarea.EasyAttachment.Properties.Resources.Trash;
            resources.ApplyResources(this.TSClearAll, "TSClearAll");
            this.TSClearAll.Name = "TSClearAll";
            this.TSClearAll.Click += new System.EventHandler(this.TSClearAll_Click);
            // 
            // TSSeparator2
            // 
            this.TSSeparator2.Name = "TSSeparator2";
            resources.ApplyResources(this.TSSeparator2, "TSSeparator2");
            // 
            // TSModify
            // 
            this.TSModify.Image = global::Microarea.EasyAttachment.Properties.Resources.infoSmall;
            resources.ApplyResources(this.TSModify, "TSModify");
            this.TSModify.Name = "TSModify";
            this.TSModify.Click += new System.EventHandler(this.TSModify_Click);
            // 
            // TSSeparator3
            // 
            this.TSSeparator3.Name = "TSSeparator3";
            resources.ApplyResources(this.TSSeparator3, "TSSeparator3");
            // 
            // TSAttach
            // 
            this.TSAttach.Image = global::Microarea.EasyAttachment.Properties.Resources.paperclip16;
            resources.ApplyResources(this.TSAttach, "TSAttach");
            this.TSAttach.Name = "TSAttach";
            // 
            // QASplitContainer
            // 
            this.QASplitContainer.BackColor = System.Drawing.Color.LightSteelBlue;
            resources.ApplyResources(this.QASplitContainer, "QASplitContainer");
            this.QASplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.QASplitContainer.Name = "QASplitContainer";
            // 
            // QASplitContainer.Panel1
            // 
            this.QASplitContainer.Panel1.Controls.Add(this.BtnApply);
            this.QASplitContainer.Panel1.Controls.Add(this.TxtHeading);
            this.QASplitContainer.Panel1.Controls.Add(this.LblHeading);
            // 
            // QASplitContainer.Panel2
            // 
            this.QASplitContainer.Panel2.Controls.Add(this.TSContainer);
            // 
            // BtnApply
            // 
            this.BtnApply.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.BtnApply, "BtnApply");
            this.BtnApply.Image = global::Microarea.EasyAttachment.Properties.Resources.arrowdown24x24;
            this.BtnApply.Name = "BtnApply";
            this.BtnApply.UseVisualStyleBackColor = true;
            this.BtnApply.Click += new System.EventHandler(this.BtnApply_Click);
            // 
            // TxtHeading
            // 
            this.TxtHeading.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TxtHeading, "TxtHeading");
            this.TxtHeading.Name = "TxtHeading";
            // 
            // LblHeading
            // 
            resources.ApplyResources(this.LblHeading, "LblHeading");
            this.LblHeading.Name = "LblHeading";
            // 
            // QuickAttachment
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.Controls.Add(this.QASplitContainer);
            this.Name = "QuickAttachment";
            this.TSContainer.ContentPanel.ResumeLayout(false);
            this.TSContainer.TopToolStripPanel.ResumeLayout(false);
            this.TSContainer.TopToolStripPanel.PerformLayout();
            this.TSContainer.ResumeLayout(false);
            this.TSContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGVDocuments)).EndInit();
            this.TSActions.ResumeLayout(false);
            this.TSActions.PerformLayout();
            this.QASplitContainer.Panel1.ResumeLayout(false);
            this.QASplitContainer.Panel1.PerformLayout();
            this.QASplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.QASplitContainer)).EndInit();
            this.QASplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer TSContainer;
		private System.Windows.Forms.DataGridView DGVDocuments;
		private System.Windows.Forms.ToolStrip TSActions;
		private System.Windows.Forms.ToolStripDropDownButton TSBtnAdd;
		private System.Windows.Forms.ToolStripMenuItem TSRepository;
		private System.Windows.Forms.ToolStripMenuItem TSFileSystem;
		private System.Windows.Forms.ToolStripMenuItem TSDevice;
		private System.Windows.Forms.ToolStripButton TSDelete;
		private System.Windows.Forms.ToolStripButton TSClearAll;
		private System.Windows.Forms.ToolStripSeparator TSSeparator1;
		private System.Windows.Forms.ToolStripButton TSAttach;
		private System.Windows.Forms.SplitContainer QASplitContainer;
		private System.Windows.Forms.TextBox TxtHeading;
		private System.Windows.Forms.Label LblHeading;
		private System.Windows.Forms.ToolStripButton TSModify;
		private System.Windows.Forms.ToolStripSeparator TSSeparator2;
        private System.Windows.Forms.ToolStripSeparator TSSeparator3;
        private System.Windows.Forms.Button BtnApply;
       
	}
}