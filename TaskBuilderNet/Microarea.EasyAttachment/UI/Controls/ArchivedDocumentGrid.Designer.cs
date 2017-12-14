namespace Microarea.EasyAttachment.UI.Controls
{
    partial class ArchivedDocumentGrid
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
            f.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArchivedDocumentGrid));
            this.TSContainer = new System.Windows.Forms.ToolStripContainer();
            this.DataGridStatusStrip = new System.Windows.Forms.StatusStrip();
            this.TSSNrPagesLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.LblNoItem = new System.Windows.Forms.Label();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TSMICheckout = new System.Windows.Forms.ToolStripMenuItem();
            this.TSMICheckIn = new System.Windows.Forms.ToolStripMenuItem();
            this.TSMIUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.GridToolStrip = new System.Windows.Forms.ToolStrip();
            this.TSPreviousRowButton = new System.Windows.Forms.ToolStripButton();
            this.TSNextRowButton = new System.Windows.Forms.ToolStripButton();
            this.TSSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.TSPreviousPageButton = new System.Windows.Forms.ToolStripButton();
            this.TSNextPageButton = new System.Windows.Forms.ToolStripButton();
            this.TSBSelectAll = new System.Windows.Forms.ToolStripButton();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.TSContainer.BottomToolStripPanel.SuspendLayout();
            this.TSContainer.ContentPanel.SuspendLayout();
            this.TSContainer.TopToolStripPanel.SuspendLayout();
            this.TSContainer.SuspendLayout();
            this.DataGridStatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.GridToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // TSContainer
            // 
            resources.ApplyResources(this.TSContainer, "TSContainer");
            // 
            // TSContainer.BottomToolStripPanel
            // 
            this.TSContainer.BottomToolStripPanel.Controls.Add(this.DataGridStatusStrip);
            // 
            // TSContainer.ContentPanel
            // 
            this.TSContainer.ContentPanel.Controls.Add(this.LblNoItem);
            this.TSContainer.ContentPanel.Controls.Add(this.dataGridView);
            resources.ApplyResources(this.TSContainer.ContentPanel, "TSContainer.ContentPanel");
            this.TSContainer.Name = "TSContainer";
            // 
            // TSContainer.TopToolStripPanel
            // 
            this.TSContainer.TopToolStripPanel.BackColor = System.Drawing.Color.Lavender;
            this.TSContainer.TopToolStripPanel.Controls.Add(this.GridToolStrip);
            // 
            // DataGridStatusStrip
            // 
            this.DataGridStatusStrip.BackColor = System.Drawing.Color.Lavender;
            resources.ApplyResources(this.DataGridStatusStrip, "DataGridStatusStrip");
            this.DataGridStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSSNrPagesLabel});
            this.DataGridStatusStrip.Name = "DataGridStatusStrip";
            this.DataGridStatusStrip.SizingGrip = false;
            // 
            // TSSNrPagesLabel
            // 
            resources.ApplyResources(this.TSSNrPagesLabel, "TSSNrPagesLabel");
            this.TSSNrPagesLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.TSSNrPagesLabel.Name = "TSSNrPagesLabel";
            this.TSSNrPagesLabel.Spring = true;
            // 
            // LblNoItem
            // 
            resources.ApplyResources(this.LblNoItem, "LblNoItem");
            this.LblNoItem.Name = "LblNoItem";
            // 
            // dataGridView
            // 
            this.dataGridView.AllowDrop = true;
            this.dataGridView.AllowUserToResizeRows = false;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView.ContextMenuStrip = this.contextMenuStrip;
            resources.ApplyResources(this.dataGridView, "dataGridView");
            this.dataGridView.GridColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellDoubleClick);
            this.dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
            this.dataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_CellMouseDown);
            this.dataGridView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridView_CellPainting);
            this.dataGridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_ColumnHeaderMouseClick);
            this.dataGridView.RowContextMenuStripNeeded += new System.Windows.Forms.DataGridViewRowContextMenuStripNeededEventHandler(this.dataGridView_RowContextMenuStripNeeded);
            this.dataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.dataGridView_UserDeletingRow);
            this.dataGridView.SizeChanged += new System.EventHandler(this.dataGridView_SizeChanged);
            this.dataGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGridView_DragDrop);
            this.dataGridView.DragEnter += new System.Windows.Forms.DragEventHandler(this.dataGridView_DragEnter);
            this.dataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView_KeyDown);
            this.dataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGridView_MouseDown);
            this.dataGridView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGridView_MouseUp);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.TSMICheckout,
            this.TSMICheckIn,
            this.TSMIUndo});
            this.contextMenuStrip.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = global::Microarea.EasyAttachment.Properties.Resources.SaveDisk16x16;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            resources.ApplyResources(this.saveToolStripMenuItem, "saveToolStripMenuItem");
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::Microarea.EasyAttachment.Properties.Resources.Trash;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // TSMICheckout
            // 
            resources.ApplyResources(this.TSMICheckout, "TSMICheckout");
            this.TSMICheckout.Name = "TSMICheckout";
            this.TSMICheckout.Click += new System.EventHandler(this.TSMICheckout_Click);
            // 
            // TSMICheckIn
            // 
            resources.ApplyResources(this.TSMICheckIn, "TSMICheckIn");
            this.TSMICheckIn.Name = "TSMICheckIn";
            this.TSMICheckIn.Click += new System.EventHandler(this.TSMICheckIn_Click);
            // 
            // TSMIUndo
            // 
            resources.ApplyResources(this.TSMIUndo, "TSMIUndo");
            this.TSMIUndo.Name = "TSMIUndo";
            this.TSMIUndo.Click += new System.EventHandler(this.TSMIUndo_Click);
            // 
            // GridToolStrip
            // 
            this.GridToolStrip.BackColor = System.Drawing.Color.Lavender;
            resources.ApplyResources(this.GridToolStrip, "GridToolStrip");
            this.GridToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.GridToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSPreviousRowButton,
            this.TSNextRowButton,
            this.TSSeparator1,
            this.TSPreviousPageButton,
            this.TSNextPageButton,
            this.TSBSelectAll});
            this.GridToolStrip.Name = "GridToolStrip";
            // 
            // TSPreviousRowButton
            // 
            this.TSPreviousRowButton.AutoToolTip = false;
            this.TSPreviousRowButton.Image = global::Microarea.EasyAttachment.Properties.Resources.arrowup24x24;
            resources.ApplyResources(this.TSPreviousRowButton, "TSPreviousRowButton");
            this.TSPreviousRowButton.Name = "TSPreviousRowButton";
            this.TSPreviousRowButton.Click += new System.EventHandler(this.TSPreviousRowButton_Click);
            // 
            // TSNextRowButton
            // 
            this.TSNextRowButton.AutoToolTip = false;
            this.TSNextRowButton.Image = global::Microarea.EasyAttachment.Properties.Resources.arrowdown24x24;
            resources.ApplyResources(this.TSNextRowButton, "TSNextRowButton");
            this.TSNextRowButton.Name = "TSNextRowButton";
            this.TSNextRowButton.Click += new System.EventHandler(this.TSNextRowButton_Click);
            // 
            // TSSeparator1
            // 
            this.TSSeparator1.Name = "TSSeparator1";
            resources.ApplyResources(this.TSSeparator1, "TSSeparator1");
            // 
            // TSPreviousPageButton
            // 
            this.TSPreviousPageButton.AutoToolTip = false;
            this.TSPreviousPageButton.Image = global::Microarea.EasyAttachment.Properties.Resources.ArrowPagePrevious24x24;
            resources.ApplyResources(this.TSPreviousPageButton, "TSPreviousPageButton");
            this.TSPreviousPageButton.Name = "TSPreviousPageButton";
            this.TSPreviousPageButton.Click += new System.EventHandler(this.TSPreviousPageButton_Click);
            // 
            // TSNextPageButton
            // 
            this.TSNextPageButton.AutoToolTip = false;
            this.TSNextPageButton.Image = global::Microarea.EasyAttachment.Properties.Resources.ArrowPageNext24x24;
            resources.ApplyResources(this.TSNextPageButton, "TSNextPageButton");
            this.TSNextPageButton.Name = "TSNextPageButton";
            this.TSNextPageButton.Click += new System.EventHandler(this.TSNextPageButton_Click);
            // 
            // TSBSelectAll
            // 
            this.TSBSelectAll.Image = global::Microarea.EasyAttachment.Properties.Resources.Checkbox16;
            resources.ApplyResources(this.TSBSelectAll, "TSBSelectAll");
            this.TSBSelectAll.Name = "TSBSelectAll";
            this.TSBSelectAll.Click += new System.EventHandler(this.TSBSelectAll_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "paperclip.png");
            this.imageList1.Images.SetKeyName(1, "Modifying.png");
            this.imageList1.Images.SetKeyName(2, "Folder16x16.png");
            this.imageList1.Images.SetKeyName(3, "Transparent.png");
            // 
            // ArchivedDocumentGrid
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TSContainer);
            this.Name = "ArchivedDocumentGrid";
            this.TSContainer.BottomToolStripPanel.ResumeLayout(false);
            this.TSContainer.BottomToolStripPanel.PerformLayout();
            this.TSContainer.ContentPanel.ResumeLayout(false);
            this.TSContainer.ContentPanel.PerformLayout();
            this.TSContainer.TopToolStripPanel.ResumeLayout(false);
            this.TSContainer.TopToolStripPanel.PerformLayout();
            this.TSContainer.ResumeLayout(false);
            this.TSContainer.PerformLayout();
            this.DataGridStatusStrip.ResumeLayout(false);
            this.DataGridStatusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.GridToolStrip.ResumeLayout(false);
            this.GridToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripContainer TSContainer;
		private System.Windows.Forms.ToolStrip GridToolStrip;
		private System.Windows.Forms.ToolStripButton TSPreviousRowButton;
		private System.Windows.Forms.ToolStripButton TSNextRowButton;
		private System.Windows.Forms.ToolStripSeparator TSSeparator1;
		private System.Windows.Forms.ToolStripButton TSPreviousPageButton;
		private System.Windows.Forms.ToolStripButton TSNextPageButton;
		private System.Windows.Forms.StatusStrip DataGridStatusStrip;
		private System.Windows.Forms.ToolStripStatusLabel TSSNrPagesLabel;
        private System.Windows.Forms.Label LblNoItem;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripButton TSBSelectAll;
        private System.Windows.Forms.ToolStripMenuItem TSMICheckout;
        private System.Windows.Forms.ToolStripMenuItem TSMICheckIn;
        private System.Windows.Forms.ToolStripMenuItem TSMIUndo;
    }
}
