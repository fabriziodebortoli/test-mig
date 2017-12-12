
namespace Microarea.Console.Plugin.AuditingAdmin
{
    partial class ApplicationsTree
    {
        private System.ComponentModel.IContainer components;
		private System.Windows.Forms.TreeView trwApplications;
        private System.Windows.Forms.ListView lstColumns;
		private System.Windows.Forms.Button btnAddColumns;
        private System.Windows.Forms.PictureBox ptbTracedTables;
		private System.Windows.Forms.PictureBox ptbTables;
		private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.PictureBox ptbStopTraceTables;
        private System.Windows.Forms.ToolTip LanguageToolTip;
        private System.Windows.Forms.CheckBox cbNoTracedFilter;
        private System.Windows.Forms.CheckBox cbTracedFilter;
        private System.Windows.Forms.CheckBox cbStopTraceFilter;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Label lblBlack;
        private System.Windows.Forms.Label lblOrange;
        private System.Windows.Forms.Label lblGreen;
        private System.Windows.Forms.Panel pnlInscription;
        private System.Windows.Forms.GroupBox gbTable;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnPauseTrace;
        private System.Windows.Forms.Label lbStartTrace;
        private System.Windows.Forms.Label lbPauseTrace;
        private System.Windows.Forms.Label lbStopTrace;
        private System.Windows.Forms.Button btnStartTrace;
        private System.Windows.Forms.Button btnStopTrace;
		private System.Windows.Forms.Button SelDeselAllButton;
        private EnumOperations enumOperations;

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
        //---------------------------------------------------------------------
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplicationsTree));
			this.trwApplications = new System.Windows.Forms.TreeView();
			this.TreeContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.StartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ResumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.PauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.StopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.lstColumns = new System.Windows.Forms.ListView();
			this.btnAddColumns = new System.Windows.Forms.Button();
			this.ptbTables = new System.Windows.Forms.PictureBox();
			this.ptbStopTraceTables = new System.Windows.Forms.PictureBox();
			this.ptbTracedTables = new System.Windows.Forms.PictureBox();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.LanguageToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.cbNoTracedFilter = new System.Windows.Forms.CheckBox();
			this.cbTracedFilter = new System.Windows.Forms.CheckBox();
			this.cbStopTraceFilter = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.btnQuery = new System.Windows.Forms.Button();
			this.lblBlack = new System.Windows.Forms.Label();
			this.lblOrange = new System.Windows.Forms.Label();
			this.lblGreen = new System.Windows.Forms.Label();
			this.pnlInscription = new System.Windows.Forms.Panel();
			this.gbTable = new System.Windows.Forms.GroupBox();
			this.btnStartTrace = new System.Windows.Forms.Button();
			this.lbStopTrace = new System.Windows.Forms.Label();
			this.lbPauseTrace = new System.Windows.Forms.Label();
			this.lbStartTrace = new System.Windows.Forms.Label();
			this.btnStopTrace = new System.Windows.Forms.Button();
			this.btnPauseTrace = new System.Windows.Forms.Button();
			this.lblTitle = new System.Windows.Forms.Label();
			this.SelDeselAllButton = new System.Windows.Forms.Button();
			this.TreeContextMenuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ptbTables)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ptbStopTraceTables)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ptbTracedTables)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.pnlInscription.SuspendLayout();
			this.gbTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// trwApplications
			// 
			resources.ApplyResources(this.trwApplications, "trwApplications");
			this.trwApplications.ItemHeight = 16;
			this.trwApplications.Name = "trwApplications";
			this.trwApplications.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.trwApplications_BeforeExpand);
			this.trwApplications.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trwApplications_AfterSelect);
			this.trwApplications.MouseMove += new System.Windows.Forms.MouseEventHandler(this.trwApplications_MouseMove);
			this.trwApplications.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trwApplications_MouseDown);
			// 
			// TreeContextMenuStrip
			// 
			this.TreeContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StartToolStripMenuItem,
            this.ResumeToolStripMenuItem,
            this.PauseToolStripMenuItem,
            this.StopToolStripMenuItem});
			this.TreeContextMenuStrip.Name = "TreeContextMenuStrip";
			resources.ApplyResources(this.TreeContextMenuStrip, "TreeContextMenuStrip");
			// 
			// StartToolStripMenuItem
			// 
			this.StartToolStripMenuItem.Image = global::Microarea.Console.Plugin.AuditingAdmin.Strings.Play;
			this.StartToolStripMenuItem.Name = "StartToolStripMenuItem";
			resources.ApplyResources(this.StartToolStripMenuItem, "StartToolStripMenuItem");
			this.StartToolStripMenuItem.Click += new System.EventHandler(this.StartToolStripMenuItem_Click);
			// 
			// ResumeToolStripMenuItem
			// 
			this.ResumeToolStripMenuItem.Image = global::Microarea.Console.Plugin.AuditingAdmin.Strings.Refresh;
			this.ResumeToolStripMenuItem.Name = "ResumeToolStripMenuItem";
			resources.ApplyResources(this.ResumeToolStripMenuItem, "ResumeToolStripMenuItem");
			this.ResumeToolStripMenuItem.Click += new System.EventHandler(this.ResumeToolStripMenuItem_Click);
			// 
			// PauseToolStripMenuItem
			// 
			this.PauseToolStripMenuItem.Image = global::Microarea.Console.Plugin.AuditingAdmin.Strings.Pause;
			this.PauseToolStripMenuItem.Name = "PauseToolStripMenuItem";
			resources.ApplyResources(this.PauseToolStripMenuItem, "PauseToolStripMenuItem");
			this.PauseToolStripMenuItem.Click += new System.EventHandler(this.PauseToolStripMenuItem_Click);
			// 
			// StopToolStripMenuItem
			// 
			this.StopToolStripMenuItem.Image = global::Microarea.Console.Plugin.AuditingAdmin.Strings.Stop;
			this.StopToolStripMenuItem.Name = "StopToolStripMenuItem";
			resources.ApplyResources(this.StopToolStripMenuItem, "StopToolStripMenuItem");
			this.StopToolStripMenuItem.Click += new System.EventHandler(this.StopToolStripMenuItem_Click);
			// 
			// lstColumns
			// 
			resources.ApplyResources(this.lstColumns, "lstColumns");
			this.lstColumns.ContextMenuStrip = this.TreeContextMenuStrip;
			this.lstColumns.Name = "lstColumns";
			this.lstColumns.UseCompatibleStateImageBehavior = false;
			this.lstColumns.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstColumns_ItemCheck);
			this.lstColumns.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lstColumns_MouseMove);
			// 
			// btnAddColumns
			// 
			resources.ApplyResources(this.btnAddColumns, "btnAddColumns");
			this.btnAddColumns.ForeColor = System.Drawing.SystemColors.ControlText;
			this.btnAddColumns.Name = "btnAddColumns";
			this.btnAddColumns.Click += new System.EventHandler(this.btnAddColumns_Click);
			// 
			// ptbTables
			// 
			resources.ApplyResources(this.ptbTables, "ptbTables");
			this.ptbTables.Name = "ptbTables";
			this.ptbTables.TabStop = false;
			// 
			// ptbStopTraceTables
			// 
			resources.ApplyResources(this.ptbStopTraceTables, "ptbStopTraceTables");
			this.ptbStopTraceTables.Name = "ptbStopTraceTables";
			this.ptbStopTraceTables.TabStop = false;
			// 
			// ptbTracedTables
			// 
			resources.ApplyResources(this.ptbTracedTables, "ptbTracedTables");
			this.ptbTracedTables.Name = "ptbTracedTables";
			this.ptbTracedTables.TabStop = false;
			// 
			// imageList
			// 
			this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			resources.ApplyResources(this.imageList, "imageList");
			this.imageList.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// LanguageToolTip
			// 
			this.LanguageToolTip.AutoPopDelay = 5000;
			this.LanguageToolTip.InitialDelay = 1500;
			this.LanguageToolTip.ReshowDelay = 100;
			// 
			// cbNoTracedFilter
			// 
			this.cbNoTracedFilter.Checked = true;
			this.cbNoTracedFilter.CheckState = System.Windows.Forms.CheckState.Checked;
			resources.ApplyResources(this.cbNoTracedFilter, "cbNoTracedFilter");
			this.cbNoTracedFilter.Name = "cbNoTracedFilter";
			this.cbNoTracedFilter.CheckedChanged += new System.EventHandler(this.cbNoTracedFilter_CheckedChanged);
			// 
			// cbTracedFilter
			// 
			this.cbTracedFilter.Checked = true;
			this.cbTracedFilter.CheckState = System.Windows.Forms.CheckState.Checked;
			resources.ApplyResources(this.cbTracedFilter, "cbTracedFilter");
			this.cbTracedFilter.Name = "cbTracedFilter";
			this.cbTracedFilter.CheckedChanged += new System.EventHandler(this.cbTracedFilter_CheckedChanged);
			// 
			// cbStopTraceFilter
			// 
			this.cbStopTraceFilter.Checked = true;
			this.cbStopTraceFilter.CheckState = System.Windows.Forms.CheckState.Checked;
			resources.ApplyResources(this.cbStopTraceFilter, "cbStopTraceFilter");
			this.cbStopTraceFilter.Name = "cbStopTraceFilter";
			this.cbStopTraceFilter.CheckedChanged += new System.EventHandler(this.cbStopTraceFilter_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cbNoTracedFilter);
			this.groupBox1.Controls.Add(this.cbTracedFilter);
			this.groupBox1.Controls.Add(this.cbStopTraceFilter);
			this.groupBox1.Controls.Add(this.ptbTracedTables);
			this.groupBox1.Controls.Add(this.ptbStopTraceTables);
			this.groupBox1.Controls.Add(this.ptbTables);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// btnQuery
			// 
			resources.ApplyResources(this.btnQuery, "btnQuery");
			this.btnQuery.Name = "btnQuery";
			this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
			// 
			// lblBlack
			// 
			resources.ApplyResources(this.lblBlack, "lblBlack");
			this.lblBlack.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblBlack.ForeColor = System.Drawing.Color.Black;
			this.lblBlack.Name = "lblBlack";
			// 
			// lblOrange
			// 
			resources.ApplyResources(this.lblOrange, "lblOrange");
			this.lblOrange.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblOrange.ForeColor = System.Drawing.Color.Orange;
			this.lblOrange.Name = "lblOrange";
			// 
			// lblGreen
			// 
			resources.ApplyResources(this.lblGreen, "lblGreen");
			this.lblGreen.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblGreen.ForeColor = System.Drawing.Color.Green;
			this.lblGreen.Name = "lblGreen";
			// 
			// pnlInscription
			// 
			resources.ApplyResources(this.pnlInscription, "pnlInscription");
			this.pnlInscription.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlInscription.Controls.Add(this.lblBlack);
			this.pnlInscription.Controls.Add(this.lblOrange);
			this.pnlInscription.Controls.Add(this.lblGreen);
			this.pnlInscription.Name = "pnlInscription";
			// 
			// gbTable
			// 
			resources.ApplyResources(this.gbTable, "gbTable");
			this.gbTable.Controls.Add(this.btnStartTrace);
			this.gbTable.Controls.Add(this.lbStopTrace);
			this.gbTable.Controls.Add(this.lbPauseTrace);
			this.gbTable.Controls.Add(this.lbStartTrace);
			this.gbTable.Controls.Add(this.btnStopTrace);
			this.gbTable.Controls.Add(this.btnPauseTrace);
			this.gbTable.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.gbTable.Name = "gbTable";
			this.gbTable.TabStop = false;
			// 
			// btnStartTrace
			// 
			resources.ApplyResources(this.btnStartTrace, "btnStartTrace");
			this.btnStartTrace.ForeColor = System.Drawing.Color.Black;
			this.btnStartTrace.Name = "btnStartTrace";
			this.btnStartTrace.Click += new System.EventHandler(this.btnStartTrace_Click);
			this.btnStartTrace.MouseHover += new System.EventHandler(this.btState_MouseHover);
			// 
			// lbStopTrace
			// 
			this.lbStopTrace.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lbStopTrace, "lbStopTrace");
			this.lbStopTrace.Name = "lbStopTrace";
			// 
			// lbPauseTrace
			// 
			this.lbPauseTrace.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lbPauseTrace, "lbPauseTrace");
			this.lbPauseTrace.Name = "lbPauseTrace";
			// 
			// lbStartTrace
			// 
			this.lbStartTrace.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lbStartTrace, "lbStartTrace");
			this.lbStartTrace.Name = "lbStartTrace";
			// 
			// btnStopTrace
			// 
			resources.ApplyResources(this.btnStopTrace, "btnStopTrace");
			this.btnStopTrace.ForeColor = System.Drawing.Color.Black;
			this.btnStopTrace.Name = "btnStopTrace";
			this.btnStopTrace.Click += new System.EventHandler(this.btnStopTrace_Click);
			this.btnStopTrace.MouseHover += new System.EventHandler(this.btState_MouseHover);
			// 
			// btnPauseTrace
			// 
			resources.ApplyResources(this.btnPauseTrace, "btnPauseTrace");
			this.btnPauseTrace.ForeColor = System.Drawing.Color.Black;
			this.btnPauseTrace.Name = "btnPauseTrace";
			this.btnPauseTrace.Click += new System.EventHandler(this.btnPauseTrace_Click);
			this.btnPauseTrace.MouseHover += new System.EventHandler(this.btState_MouseHover);
			// 
			// lblTitle
			// 
			this.lblTitle.AllowDrop = true;
			resources.ApplyResources(this.lblTitle, "lblTitle");
			this.lblTitle.BackColor = System.Drawing.Color.CornflowerBlue;
			this.lblTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblTitle.ForeColor = System.Drawing.Color.White;
			this.lblTitle.Name = "lblTitle";
			// 
			// SelDeselAllButton
			// 
			resources.ApplyResources(this.SelDeselAllButton, "SelDeselAllButton");
			this.SelDeselAllButton.Name = "SelDeselAllButton";
			this.SelDeselAllButton.Click += new System.EventHandler(this.SelDeselAllButton_Click);
			// 
			// ApplicationsTree
			// 
			this.AllowDrop = true;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.SelDeselAllButton);
			this.Controls.Add(this.lblTitle);
			this.Controls.Add(this.gbTable);
			this.Controls.Add(this.btnQuery);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.pnlInscription);
			this.Controls.Add(this.btnAddColumns);
			this.Controls.Add(this.lstColumns);
			this.Controls.Add(this.trwApplications);
			this.Name = "ApplicationsTree";
			this.VisibleChanged += new System.EventHandler(this.ApplicationsTree_VisibleChanged);
			this.TreeContextMenuStrip.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ptbTables)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ptbStopTraceTables)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ptbTracedTables)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.pnlInscription.ResumeLayout(false);
			this.pnlInscription.PerformLayout();
			this.gbTable.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        #endregion

		private System.Windows.Forms.ContextMenuStrip TreeContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem StartToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ResumeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem PauseToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem StopToolStripMenuItem;

	}
}
