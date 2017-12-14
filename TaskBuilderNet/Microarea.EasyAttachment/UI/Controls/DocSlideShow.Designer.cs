namespace Microarea.EasyAttachment.UI.Controls
{
    partial class DocSlideShow
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocSlideShow));
            this.ContextMenuOpenAttach = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openAttachmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openArchivedDocumentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.BtnSwitcView = new System.Windows.Forms.Button();
            this.BtnPaperyDocs = new System.Windows.Forms.CheckBox();
            this.BtnRefresh = new System.Windows.Forms.Button();
            this.BtnLeft = new System.Windows.Forms.Button();
            this.BtnRight = new System.Windows.Forms.Button();
            this.BtnRemoveAttach = new System.Windows.Forms.Button();
            this.GBRecent = new System.Windows.Forms.GroupBox();
            this.LblDocName = new System.Windows.Forms.Label();
            this.PanelSlideShow = new System.Windows.Forms.Panel();
            this.DocListView = new System.Windows.Forms.ListView();
            this.ColName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.LblNoAttach = new System.Windows.Forms.Label();
            this.PBNoAttach = new System.Windows.Forms.PictureBox();
            this.PbDocSlideShow = new System.Windows.Forms.PictureBox();
            this.ContextMenuOpenAttach.SuspendLayout();
            this.GBRecent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PBNoAttach)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbDocSlideShow)).BeginInit();
            this.SuspendLayout();
            // 
            // ContextMenuOpenAttach
            // 
            this.ContextMenuOpenAttach.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openAttachmentToolStripMenuItem,
            this.openArchivedDocumentToolStripMenuItem});
            this.ContextMenuOpenAttach.Name = "ContextMenuOpenAttach";
            resources.ApplyResources(this.ContextMenuOpenAttach, "ContextMenuOpenAttach");
            // 
            // openAttachmentToolStripMenuItem
            // 
            resources.ApplyResources(this.openAttachmentToolStripMenuItem, "openAttachmentToolStripMenuItem");
            this.openAttachmentToolStripMenuItem.Name = "openAttachmentToolStripMenuItem";
            this.openAttachmentToolStripMenuItem.Click += new System.EventHandler(this.openAttachmentToolStripMenuItem_Click);
            // 
            // openArchivedDocumentToolStripMenuItem
            // 
            resources.ApplyResources(this.openArchivedDocumentToolStripMenuItem, "openArchivedDocumentToolStripMenuItem");
            this.openArchivedDocumentToolStripMenuItem.Name = "openArchivedDocumentToolStripMenuItem";
            this.openArchivedDocumentToolStripMenuItem.Click += new System.EventHandler(this.openArchivedDocumentToolStripMenuItem_Click);
            // 
            // toolTip
            // 
            this.toolTip.AutomaticDelay = 100;
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.toolTip.InitialDelay = 200;
            this.toolTip.IsBalloon = true;
            this.toolTip.ReshowDelay = 20;
            // 
            // BtnSwitcView
            // 
            resources.ApplyResources(this.BtnSwitcView, "BtnSwitcView");
            this.BtnSwitcView.BackColor = System.Drawing.Color.Transparent;
            this.BtnSwitcView.Name = "BtnSwitcView";
            this.toolTip.SetToolTip(this.BtnSwitcView, resources.GetString("BtnSwitcView.ToolTip"));
            this.BtnSwitcView.UseVisualStyleBackColor = false;
            this.BtnSwitcView.Click += new System.EventHandler(this.BtnSwitcView_Click);
            // 
            // BtnPaperyDocs
            // 
            resources.ApplyResources(this.BtnPaperyDocs, "BtnPaperyDocs");
            this.BtnPaperyDocs.Image = global::Microarea.EasyAttachment.Properties.Resources.PaperAirplane16x16;
            this.BtnPaperyDocs.Name = "BtnPaperyDocs";
            this.toolTip.SetToolTip(this.BtnPaperyDocs, resources.GetString("BtnPaperyDocs.ToolTip"));
            this.BtnPaperyDocs.UseVisualStyleBackColor = true;
            this.BtnPaperyDocs.Click += new System.EventHandler(this.BtnPaperyDocs_Click);
            // 
            // BtnRefresh
            // 
            resources.ApplyResources(this.BtnRefresh, "BtnRefresh");
            this.BtnRefresh.BackColor = System.Drawing.SystemColors.Control;
            this.BtnRefresh.Name = "BtnRefresh";
            this.toolTip.SetToolTip(this.BtnRefresh, resources.GetString("BtnRefresh.ToolTip"));
            this.BtnRefresh.UseVisualStyleBackColor = true;
            this.BtnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // BtnLeft
            // 
            resources.ApplyResources(this.BtnLeft, "BtnLeft");
            this.BtnLeft.Name = "BtnLeft";
            this.toolTip.SetToolTip(this.BtnLeft, resources.GetString("BtnLeft.ToolTip"));
            this.BtnLeft.UseVisualStyleBackColor = true;
            this.BtnLeft.Click += new System.EventHandler(this.BtnLeft_Click);
            // 
            // BtnRight
            // 
            resources.ApplyResources(this.BtnRight, "BtnRight");
            this.BtnRight.Name = "BtnRight";
            this.toolTip.SetToolTip(this.BtnRight, resources.GetString("BtnRight.ToolTip"));
            this.BtnRight.UseVisualStyleBackColor = true;
            this.BtnRight.Click += new System.EventHandler(this.BtnRight_Click);
            // 
            // BtnRemoveAttach
            // 
            resources.ApplyResources(this.BtnRemoveAttach, "BtnRemoveAttach");
            this.BtnRemoveAttach.BackColor = System.Drawing.Color.Transparent;
            this.BtnRemoveAttach.Name = "BtnRemoveAttach";
            this.toolTip.SetToolTip(this.BtnRemoveAttach, resources.GetString("BtnRemoveAttach.ToolTip"));
            this.BtnRemoveAttach.UseVisualStyleBackColor = false;
            this.BtnRemoveAttach.Click += new System.EventHandler(this.BtnRemoveAttach_Click);
            // 
            // GBRecent
            // 
            resources.ApplyResources(this.GBRecent, "GBRecent");
            this.GBRecent.BackColor = System.Drawing.Color.Transparent;
            this.GBRecent.Controls.Add(this.BtnPaperyDocs);
            this.GBRecent.Controls.Add(this.BtnSwitcView);
            this.GBRecent.Controls.Add(this.BtnRefresh);
            this.GBRecent.Controls.Add(this.BtnLeft);
            this.GBRecent.Controls.Add(this.BtnRight);
            this.GBRecent.Controls.Add(this.LblDocName);
            this.GBRecent.Controls.Add(this.PanelSlideShow);
            this.GBRecent.Controls.Add(this.DocListView);
            this.GBRecent.ForeColor = System.Drawing.Color.Navy;
            this.GBRecent.Name = "GBRecent";
            this.GBRecent.TabStop = false;
            // 
            // LblDocName
            // 
            resources.ApplyResources(this.LblDocName, "LblDocName");
            this.LblDocName.BackColor = System.Drawing.Color.Lavender;
            this.LblDocName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LblDocName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblDocName.ForeColor = System.Drawing.Color.Black;
            this.LblDocName.Name = "LblDocName";
            // 
            // PanelSlideShow
            // 
            resources.ApplyResources(this.PanelSlideShow, "PanelSlideShow");
            this.PanelSlideShow.Name = "PanelSlideShow";
            // 
            // DocListView
            // 
            resources.ApplyResources(this.DocListView, "DocListView");
            this.DocListView.BackColor = System.Drawing.Color.Lavender;
            this.DocListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColName});
            this.DocListView.FullRowSelect = true;
            this.DocListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.DocListView.HideSelection = false;
            this.DocListView.Name = "DocListView";
            this.DocListView.ShowItemToolTips = true;
            this.DocListView.SmallImageList = this.imageList1;
            this.DocListView.UseCompatibleStateImageBehavior = false;
            this.DocListView.View = System.Windows.Forms.View.Details;
            this.DocListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.DocListView_ItemSelectionChanged);
            this.DocListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this._MouseDown);
            this.DocListView.MouseHover += new System.EventHandler(this._MouseHover);
            this.DocListView.MouseUp += new System.Windows.Forms.MouseEventHandler(this._MouseUp);
            // 
            // ColName
            // 
            resources.ApplyResources(this.ColName, "ColName");
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Ext_ZIP16x16.png");
            this.imageList1.Images.SetKeyName(1, "EXT_XML16x16.png");
            this.imageList1.Images.SetKeyName(2, "Ext_XLS16x16.png");
            this.imageList1.Images.SetKeyName(3, "Ext_WMV16x16.png");
            this.imageList1.Images.SetKeyName(4, "Ext_WAV16x16.png");
            this.imageList1.Images.SetKeyName(5, "Ext_TXT16x16.png");
            this.imageList1.Images.SetKeyName(6, "Ext_TIFF16x16.png");
            this.imageList1.Images.SetKeyName(7, "Ext_RTF16x16.png");
            this.imageList1.Images.SetKeyName(8, "Ext_RAR16x16.png");
            this.imageList1.Images.SetKeyName(9, "Ext_PPT16x16.png");
            this.imageList1.Images.SetKeyName(10, "Ext_PNG16x16.png");
            this.imageList1.Images.SetKeyName(11, "Ext_PDF16x16.png");
            this.imageList1.Images.SetKeyName(12, "Ext_MPEG16x16.png");
            this.imageList1.Images.SetKeyName(13, "Ext_MP316x16.png");
            this.imageList1.Images.SetKeyName(14, "Ext_MAIL16x16.png");
            this.imageList1.Images.SetKeyName(15, "Ext_JPG16x16.png");
            this.imageList1.Images.SetKeyName(16, "Ext_HTML16x16.png");
            this.imageList1.Images.SetKeyName(17, "Ext_GZIP16x16.png");
            this.imageList1.Images.SetKeyName(18, "Ext_GIF16x16.png");
            this.imageList1.Images.SetKeyName(19, "Ext_DOC16x16.png");
            this.imageList1.Images.SetKeyName(20, "Ext_Default16x16.png");
            this.imageList1.Images.SetKeyName(21, "Ext_BMP16x16.png");
            this.imageList1.Images.SetKeyName(22, "Ext_AVI16x16.png");
            this.imageList1.Images.SetKeyName(23, "PaperAirplane16x16.png");
            // 
            // LblNoAttach
            // 
            resources.ApplyResources(this.LblNoAttach, "LblNoAttach");
            this.LblNoAttach.Name = "LblNoAttach";
            // 
            // PBNoAttach
            // 
            resources.ApplyResources(this.PBNoAttach, "PBNoAttach");
            this.PBNoAttach.Name = "PBNoAttach";
            this.PBNoAttach.TabStop = false;
            // 
            // PbDocSlideShow
            // 
            resources.ApplyResources(this.PbDocSlideShow, "PbDocSlideShow");
            this.PbDocSlideShow.Name = "PbDocSlideShow";
            this.PbDocSlideShow.TabStop = false;
            // 
            // DocSlideShow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.BtnRemoveAttach);
            this.Controls.Add(this.PbDocSlideShow);
            this.Controls.Add(this.LblNoAttach);
            this.Controls.Add(this.PBNoAttach);
            this.Controls.Add(this.GBRecent);
            resources.ApplyResources(this, "$this");
            this.Name = "DocSlideShow";
            this.ContextMenuOpenAttach.ResumeLayout(false);
            this.GBRecent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PBNoAttach)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbDocSlideShow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ContextMenuStrip ContextMenuOpenAttach;
        private System.Windows.Forms.ToolStripMenuItem openAttachmentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openArchivedDocumentToolStripMenuItem;
        private System.Windows.Forms.Button BtnRight;
        private System.Windows.Forms.Button BtnLeft;
        private System.Windows.Forms.GroupBox GBRecent;
        private System.Windows.Forms.PictureBox PbDocSlideShow;
        private System.Windows.Forms.Label LblNoAttach;
        private System.Windows.Forms.PictureBox PBNoAttach;
        private System.Windows.Forms.Button BtnRemoveAttach;
        private System.Windows.Forms.Label LblDocName;
		private System.Windows.Forms.Button BtnRefresh;
        private System.Windows.Forms.ListView DocListView;
        private System.Windows.Forms.Button BtnSwitcView;
		private System.Windows.Forms.ColumnHeader ColName;
        private System.Windows.Forms.Panel PanelSlideShow;
        private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.CheckBox BtnPaperyDocs;
    }
}
