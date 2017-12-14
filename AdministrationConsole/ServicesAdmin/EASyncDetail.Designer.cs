namespace Microarea.Console.Plugin.ServicesAdmin
{
	partial class EASyncDetail
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EASyncDetail));
			this.PanelEASync = new System.Windows.Forms.Panel();
			this.PanelEADetails = new System.Windows.Forms.Panel();
			this.TabDetails = new System.Windows.Forms.TabControl();
			this.TabPageOpTrace = new System.Windows.Forms.TabPage();
			this.EALogListView = new System.Windows.Forms.ListView();
			this.TypeColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.MessageColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.DateColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.TimeColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.RefreshContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.RefreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ClearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.TabPageActions = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.LblStop = new System.Windows.Forms.Label();
			this.BtnStop = new System.Windows.Forms.Button();
			this.LblInit = new System.Windows.Forms.Label();
			this.BtnInit = new System.Windows.Forms.Button();
			this.LblClear = new System.Windows.Forms.Label();
			this.BtnClear = new System.Windows.Forms.Button();
			this.PanelEATitle = new System.Windows.Forms.Panel();
			this.EAPictureBox = new System.Windows.Forms.PictureBox();
			this.EASubtitleLabel = new System.Windows.Forms.Label();
			this.EATitleLabel = new System.Windows.Forms.Label();
			this.PanelEASync.SuspendLayout();
			this.PanelEADetails.SuspendLayout();
			this.TabDetails.SuspendLayout();
			this.TabPageOpTrace.SuspendLayout();
			this.RefreshContextMenuStrip.SuspendLayout();
			this.TabPageActions.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.PanelEATitle.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EAPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// PanelEASync
			// 
			resources.ApplyResources(this.PanelEASync, "PanelEASync");
			this.PanelEASync.BackColor = System.Drawing.SystemColors.Control;
			this.PanelEASync.Controls.Add(this.PanelEADetails);
			this.PanelEASync.Controls.Add(this.PanelEATitle);
			this.PanelEASync.Name = "PanelEASync";
			// 
			// PanelEADetails
			// 
			resources.ApplyResources(this.PanelEADetails, "PanelEADetails");
			this.PanelEADetails.BackColor = System.Drawing.SystemColors.Control;
			this.PanelEADetails.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.PanelEADetails.Controls.Add(this.TabDetails);
			this.PanelEADetails.Name = "PanelEADetails";
			// 
			// TabDetails
			// 
			resources.ApplyResources(this.TabDetails, "TabDetails");
			this.TabDetails.Controls.Add(this.TabPageOpTrace);
			this.TabDetails.Controls.Add(this.TabPageActions);
			this.TabDetails.Name = "TabDetails";
			this.TabDetails.SelectedIndex = 0;
			this.TabDetails.SelectedIndexChanged += new System.EventHandler(this.TabDetails_SelectedIndexChanged);
			// 
			// TabPageOpTrace
			// 
			this.TabPageOpTrace.Controls.Add(this.EALogListView);
			resources.ApplyResources(this.TabPageOpTrace, "TabPageOpTrace");
			this.TabPageOpTrace.Name = "TabPageOpTrace";
			this.TabPageOpTrace.UseVisualStyleBackColor = true;
			// 
			// EALogListView
			// 
			this.EALogListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TypeColHeader,
            this.MessageColHeader,
            this.DateColHeader,
            this.TimeColHeader});
			this.EALogListView.ContextMenuStrip = this.RefreshContextMenuStrip;
			resources.ApplyResources(this.EALogListView, "EALogListView");
			this.EALogListView.FullRowSelect = true;
			this.EALogListView.MultiSelect = false;
			this.EALogListView.Name = "EALogListView";
			this.EALogListView.UseCompatibleStateImageBehavior = false;
			this.EALogListView.View = System.Windows.Forms.View.Details;
			this.EALogListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.EALogListView_ColumnClick);
			this.EALogListView.DoubleClick += new System.EventHandler(this.EALogListView_DoubleClick);
			this.EALogListView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.EALogListView_KeyUp);
			this.EALogListView.Resize += new System.EventHandler(this.EALogListView_Resize);
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
            this.RefreshToolStripMenuItem,
            this.ClearToolStripMenuItem});
			this.RefreshContextMenuStrip.Name = "RefreshContextMenuStrip";
			resources.ApplyResources(this.RefreshContextMenuStrip, "RefreshContextMenuStrip");
			// 
			// RefreshToolStripMenuItem
			// 
			resources.ApplyResources(this.RefreshToolStripMenuItem, "RefreshToolStripMenuItem");
			this.RefreshToolStripMenuItem.Name = "RefreshToolStripMenuItem";
			this.RefreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItem_Click);
			// 
			// ClearToolStripMenuItem
			// 
			resources.ApplyResources(this.ClearToolStripMenuItem, "ClearToolStripMenuItem");
			this.ClearToolStripMenuItem.Name = "ClearToolStripMenuItem";
			this.ClearToolStripMenuItem.Click += new System.EventHandler(this.ClearToolStripMenuItem_Click);
			// 
			// TabPageActions
			// 
			this.TabPageActions.Controls.Add(this.groupBox1);
			resources.ApplyResources(this.TabPageActions, "TabPageActions");
			this.TabPageActions.Name = "TabPageActions";
			this.TabPageActions.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Controls.Add(this.LblStop);
			this.groupBox1.Controls.Add(this.BtnStop);
			this.groupBox1.Controls.Add(this.LblInit);
			this.groupBox1.Controls.Add(this.BtnInit);
			this.groupBox1.Controls.Add(this.LblClear);
			this.groupBox1.Controls.Add(this.BtnClear);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// LblStop
			// 
			resources.ApplyResources(this.LblStop, "LblStop");
			this.LblStop.Name = "LblStop";
			// 
			// BtnStop
			// 
			resources.ApplyResources(this.BtnStop, "BtnStop");
			this.BtnStop.Name = "BtnStop";
			this.BtnStop.UseVisualStyleBackColor = true;
			this.BtnStop.Click += new System.EventHandler(this.BtnStop_Click);
			// 
			// LblInit
			// 
			resources.ApplyResources(this.LblInit, "LblInit");
			this.LblInit.Name = "LblInit";
			// 
			// BtnInit
			// 
			resources.ApplyResources(this.BtnInit, "BtnInit");
			this.BtnInit.Name = "BtnInit";
			this.BtnInit.UseVisualStyleBackColor = true;
			this.BtnInit.Click += new System.EventHandler(this.BtnInit_Click);
			// 
			// LblClear
			// 
			resources.ApplyResources(this.LblClear, "LblClear");
			this.LblClear.Name = "LblClear";
			// 
			// BtnClear
			// 
			resources.ApplyResources(this.BtnClear, "BtnClear");
			this.BtnClear.Name = "BtnClear";
			this.BtnClear.UseVisualStyleBackColor = true;
			this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
			// 
			// PanelEATitle
			// 
			this.PanelEATitle.BackColor = System.Drawing.Color.Lavender;
			this.PanelEATitle.Controls.Add(this.EAPictureBox);
			this.PanelEATitle.Controls.Add(this.EASubtitleLabel);
			this.PanelEATitle.Controls.Add(this.EATitleLabel);
			resources.ApplyResources(this.PanelEATitle, "PanelEATitle");
			this.PanelEATitle.Name = "PanelEATitle";
			// 
			// EAPictureBox
			// 
			resources.ApplyResources(this.EAPictureBox, "EAPictureBox");
			this.EAPictureBox.Name = "EAPictureBox";
			this.EAPictureBox.TabStop = false;
			// 
			// EASubtitleLabel
			// 
			resources.ApplyResources(this.EASubtitleLabel, "EASubtitleLabel");
			this.EASubtitleLabel.BackColor = System.Drawing.Color.Lavender;
			this.EASubtitleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EASubtitleLabel.ForeColor = System.Drawing.Color.Navy;
			this.EASubtitleLabel.Name = "EASubtitleLabel";
			// 
			// EATitleLabel
			// 
			resources.ApplyResources(this.EATitleLabel, "EATitleLabel");
			this.EATitleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EATitleLabel.ForeColor = System.Drawing.Color.Navy;
			this.EATitleLabel.Name = "EATitleLabel";
			// 
			// EASyncDetail
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.PanelEASync);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EASyncDetail";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.EASyncDetail_Load);
			this.PanelEASync.ResumeLayout(false);
			this.PanelEADetails.ResumeLayout(false);
			this.TabDetails.ResumeLayout(false);
			this.TabPageOpTrace.ResumeLayout(false);
			this.RefreshContextMenuStrip.ResumeLayout(false);
			this.TabPageActions.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.PanelEATitle.ResumeLayout(false);
			this.PanelEATitle.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EAPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel PanelEASync;
		private System.Windows.Forms.Panel PanelEATitle;
		private System.Windows.Forms.Panel PanelEADetails;
		private System.Windows.Forms.Label EATitleLabel;
		private System.Windows.Forms.Label EASubtitleLabel;
		private System.Windows.Forms.ListView EALogListView;
		private System.Windows.Forms.ColumnHeader TypeColHeader;
		private System.Windows.Forms.ColumnHeader MessageColHeader;
		private System.Windows.Forms.ColumnHeader DateColHeader;
		private System.Windows.Forms.PictureBox EAPictureBox;
		private System.Windows.Forms.ColumnHeader TimeColHeader;
		private System.Windows.Forms.ContextMenuStrip RefreshContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem RefreshToolStripMenuItem;
		private System.Windows.Forms.TabControl TabDetails;
		private System.Windows.Forms.TabPage TabPageOpTrace;
		private System.Windows.Forms.TabPage TabPageActions;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label LblClear;
		private System.Windows.Forms.Button BtnClear;
		private System.Windows.Forms.Label LblInit;
		private System.Windows.Forms.Button BtnInit;
		private System.Windows.Forms.ToolStripMenuItem ClearToolStripMenuItem;
		private System.Windows.Forms.Button BtnStop;
		private System.Windows.Forms.Label LblStop;
	}
}