namespace Microarea.Console.Plugin.ServicesAdmin
{
    partial class TBSenderDetails
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TBSenderDetails));
            this.PanelTbSender = new System.Windows.Forms.Panel();
            this.LwLogs = new System.Windows.Forms.ListView();
            this.TypeColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MessageColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DateColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TimeColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RefreshContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RefreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PanelTitleTbSender = new System.Windows.Forms.Panel();
            this.PBTbSender = new System.Windows.Forms.PictureBox();
            this.LblTbsenderSubtitle = new System.Windows.Forms.Label();
            this.LblTitleTbSender = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.PanelTbSender.SuspendLayout();
            this.RefreshContextMenuStrip.SuspendLayout();
            this.PanelTitleTbSender.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PBTbSender)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelTbSender
            // 
            resources.ApplyResources(this.PanelTbSender, "PanelTbSender");
            this.PanelTbSender.BackColor = System.Drawing.SystemColors.Control;
            this.PanelTbSender.Controls.Add(this.LwLogs);
            this.PanelTbSender.Controls.Add(this.PanelTitleTbSender);
            this.PanelTbSender.Name = "PanelTbSender";
            // 
            // LwLogs
            // 
            this.LwLogs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TypeColHeader,
            this.MessageColHeader,
            this.DateColHeader,
            this.TimeColHeader});
            this.LwLogs.ContextMenuStrip = this.RefreshContextMenuStrip;
            resources.ApplyResources(this.LwLogs, "LwLogs");
            this.LwLogs.FullRowSelect = true;
            this.LwLogs.MultiSelect = false;
            this.LwLogs.Name = "LwLogs";
            this.LwLogs.UseCompatibleStateImageBehavior = false;
            this.LwLogs.View = System.Windows.Forms.View.Details;
            this.LwLogs.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.TbSenderLogListView_ColumnClick);
            this.LwLogs.DoubleClick += new System.EventHandler(this.TBSenderLogListView_DoubleClick);
            this.LwLogs.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TbSenderLogListView_KeyUp);
            this.LwLogs.Resize += new System.EventHandler(this.TbSenderLogListView_Resize);
            // 
            // TypeColHeader
            // 
            resources.ApplyResources(this.TypeColHeader, "TypeColHeader");
            // 
            // MessageColHeader
            // 
            resources.ApplyResources(this.MessageColHeader, "MessageColHeader");
            // 
            // DateColHeader
            // 
            resources.ApplyResources(this.DateColHeader, "DateColHeader");
            // 
            // TimeColHeader
            // 
            resources.ApplyResources(this.TimeColHeader, "TimeColHeader");
            // 
            // RefreshContextMenuStrip
            // 
            this.RefreshContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RefreshToolStripMenuItem});
            this.RefreshContextMenuStrip.Name = "RefreshContextMenuStrip";
            resources.ApplyResources(this.RefreshContextMenuStrip, "RefreshContextMenuStrip");
            // 
            // RefreshToolStripMenuItem
            // 
            resources.ApplyResources(this.RefreshToolStripMenuItem, "RefreshToolStripMenuItem");
            this.RefreshToolStripMenuItem.Name = "RefreshToolStripMenuItem";
            this.RefreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItem_Click);
            // 
            // PanelTitleTbSender
            // 
            this.PanelTitleTbSender.BackColor = System.Drawing.Color.Lavender;
            this.PanelTitleTbSender.Controls.Add(this.PBTbSender);
            this.PanelTitleTbSender.Controls.Add(this.LblTbsenderSubtitle);
            this.PanelTitleTbSender.Controls.Add(this.LblTitleTbSender);
            resources.ApplyResources(this.PanelTitleTbSender, "PanelTitleTbSender");
            this.PanelTitleTbSender.Name = "PanelTitleTbSender";
            // 
            // PBTbSender
            // 
            resources.ApplyResources(this.PBTbSender, "PBTbSender");
            this.PBTbSender.Name = "PBTbSender";
            this.PBTbSender.TabStop = false;
            // 
            // LblTbsenderSubtitle
            // 
            resources.ApplyResources(this.LblTbsenderSubtitle, "LblTbsenderSubtitle");
            this.LblTbsenderSubtitle.BackColor = System.Drawing.Color.Lavender;
            this.LblTbsenderSubtitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblTbsenderSubtitle.ForeColor = System.Drawing.Color.Navy;
            this.LblTbsenderSubtitle.Name = "LblTbsenderSubtitle";
            // 
            // LblTitleTbSender
            // 
            resources.ApplyResources(this.LblTitleTbSender, "LblTitleTbSender");
            this.LblTitleTbSender.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblTitleTbSender.ForeColor = System.Drawing.Color.Navy;
            this.LblTitleTbSender.Name = "LblTitleTbSender";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2});
            this.contextMenuStrip1.Name = "RefreshContextMenuStrip";
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            // 
            // toolStripMenuItem1
            // 
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            // 
            // toolStripMenuItem2
            // 
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            // 
            // TBSenderDetails
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelTbSender);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TBSenderDetails";
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.TbSenderDetail_Load);
            this.PanelTbSender.ResumeLayout(false);
            this.RefreshContextMenuStrip.ResumeLayout(false);
            this.PanelTitleTbSender.ResumeLayout(false);
            this.PanelTitleTbSender.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PBTbSender)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel PanelTbSender;
        private System.Windows.Forms.Panel PanelTitleTbSender;
		private System.Windows.Forms.Label LblTitleTbSender;
        private System.Windows.Forms.Label LblTbsenderSubtitle;
        private System.Windows.Forms.PictureBox PBTbSender;
		private System.Windows.Forms.ContextMenuStrip RefreshContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem RefreshToolStripMenuItem;
        private System.Windows.Forms.ListView LwLogs;
        private System.Windows.Forms.ColumnHeader TypeColHeader;
        private System.Windows.Forms.ColumnHeader MessageColHeader;
        private System.Windows.Forms.ColumnHeader DateColHeader;
        private System.Windows.Forms.ColumnHeader TimeColHeader;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
	}
}