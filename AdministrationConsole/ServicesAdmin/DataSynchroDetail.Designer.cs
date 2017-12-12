namespace Microarea.Console.Plugin.ServicesAdmin
{
	partial class DataSynchroDetail
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataSynchroDetail));
			this.PanelDataSynchro = new System.Windows.Forms.Panel();
			this.PanelDSDetails = new System.Windows.Forms.Panel();
			this.TabDetails = new System.Windows.Forms.TabControl();
			this.TabPageOpTrace = new System.Windows.Forms.TabPage();
			this.DSLogListView = new System.Windows.Forms.ListView();
			this.TypeColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.MessageColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.DateColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.TimeColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.RefreshContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.RefreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ClearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.TabPageActions = new System.Windows.Forms.TabPage();
			this.GBoxActions = new System.Windows.Forms.GroupBox();
			this.LblInit = new System.Windows.Forms.Label();
			this.BtnInit = new System.Windows.Forms.Button();
			this.PanelDSTitle = new System.Windows.Forms.Panel();
			this.DSPictureBox = new System.Windows.Forms.PictureBox();
			this.LblSubtitle = new System.Windows.Forms.Label();
			this.LblTitle = new System.Windows.Forms.Label();
			this.PanelDataSynchro.SuspendLayout();
			this.PanelDSDetails.SuspendLayout();
			this.TabDetails.SuspendLayout();
			this.TabPageOpTrace.SuspendLayout();
			this.RefreshContextMenuStrip.SuspendLayout();
			this.TabPageActions.SuspendLayout();
			this.GBoxActions.SuspendLayout();
			this.PanelDSTitle.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DSPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// PanelDataSynchro
			// 
			resources.ApplyResources(this.PanelDataSynchro, "PanelDataSynchro");
			this.PanelDataSynchro.BackColor = System.Drawing.SystemColors.Control;
			this.PanelDataSynchro.Controls.Add(this.PanelDSDetails);
			this.PanelDataSynchro.Controls.Add(this.PanelDSTitle);
			this.PanelDataSynchro.Name = "PanelDataSynchro";
			// 
			// PanelDSDetails
			// 
			resources.ApplyResources(this.PanelDSDetails, "PanelDSDetails");
			this.PanelDSDetails.BackColor = System.Drawing.SystemColors.Control;
			this.PanelDSDetails.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.PanelDSDetails.Controls.Add(this.TabDetails);
			this.PanelDSDetails.Name = "PanelDSDetails";
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
			this.TabPageOpTrace.Controls.Add(this.DSLogListView);
			resources.ApplyResources(this.TabPageOpTrace, "TabPageOpTrace");
			this.TabPageOpTrace.Name = "TabPageOpTrace";
			this.TabPageOpTrace.UseVisualStyleBackColor = true;
			// 
			// DSLogListView
			// 
			this.DSLogListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TypeColHeader,
            this.MessageColHeader,
            this.DateColHeader,
            this.TimeColHeader});
			this.DSLogListView.ContextMenuStrip = this.RefreshContextMenuStrip;
			resources.ApplyResources(this.DSLogListView, "DSLogListView");
			this.DSLogListView.FullRowSelect = true;
			this.DSLogListView.MultiSelect = false;
			this.DSLogListView.Name = "DSLogListView";
			this.DSLogListView.UseCompatibleStateImageBehavior = false;
			this.DSLogListView.View = System.Windows.Forms.View.Details;
			this.DSLogListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.DSLogListView_ColumnClick);
			this.DSLogListView.DoubleClick += new System.EventHandler(this.DSLogListView_DoubleClick);
			this.DSLogListView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.DSLogListView_KeyUp);
			this.DSLogListView.Resize += new System.EventHandler(this.DSLogListView_Resize);
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
			this.TabPageActions.Controls.Add(this.GBoxActions);
			resources.ApplyResources(this.TabPageActions, "TabPageActions");
			this.TabPageActions.Name = "TabPageActions";
			this.TabPageActions.UseVisualStyleBackColor = true;
			// 
			// GBoxActions
			// 
			resources.ApplyResources(this.GBoxActions, "GBoxActions");
			this.GBoxActions.Controls.Add(this.LblInit);
			this.GBoxActions.Controls.Add(this.BtnInit);
			this.GBoxActions.Name = "GBoxActions";
			this.GBoxActions.TabStop = false;
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
			// PanelDSTitle
			// 
			this.PanelDSTitle.BackColor = System.Drawing.Color.Lavender;
			this.PanelDSTitle.Controls.Add(this.DSPictureBox);
			this.PanelDSTitle.Controls.Add(this.LblSubtitle);
			this.PanelDSTitle.Controls.Add(this.LblTitle);
			resources.ApplyResources(this.PanelDSTitle, "PanelDSTitle");
			this.PanelDSTitle.Name = "PanelDSTitle";
			// 
			// DSPictureBox
			// 
			resources.ApplyResources(this.DSPictureBox, "DSPictureBox");
			this.DSPictureBox.Name = "DSPictureBox";
			this.DSPictureBox.TabStop = false;
			// 
			// LblSubtitle
			// 
			resources.ApplyResources(this.LblSubtitle, "LblSubtitle");
			this.LblSubtitle.BackColor = System.Drawing.Color.Lavender;
			this.LblSubtitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblSubtitle.ForeColor = System.Drawing.Color.Navy;
			this.LblSubtitle.Name = "LblSubtitle";
			// 
			// LblTitle
			// 
			resources.ApplyResources(this.LblTitle, "LblTitle");
			this.LblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblTitle.ForeColor = System.Drawing.Color.Navy;
			this.LblTitle.Name = "LblTitle";
			// 
			// DataSynchroDetail
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.PanelDataSynchro);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DataSynchroDetail";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.DataSynchroDetail_Load);
			this.PanelDataSynchro.ResumeLayout(false);
			this.PanelDSDetails.ResumeLayout(false);
			this.TabDetails.ResumeLayout(false);
			this.TabPageOpTrace.ResumeLayout(false);
			this.RefreshContextMenuStrip.ResumeLayout(false);
			this.TabPageActions.ResumeLayout(false);
			this.GBoxActions.ResumeLayout(false);
			this.PanelDSTitle.ResumeLayout(false);
			this.PanelDSTitle.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DSPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel PanelDataSynchro;
		private System.Windows.Forms.Panel PanelDSTitle;
		private System.Windows.Forms.Panel PanelDSDetails;
		private System.Windows.Forms.Label LblTitle;
		private System.Windows.Forms.Label LblSubtitle;
		private System.Windows.Forms.ListView DSLogListView;
		private System.Windows.Forms.ColumnHeader TypeColHeader;
		private System.Windows.Forms.ColumnHeader MessageColHeader;
		private System.Windows.Forms.ColumnHeader DateColHeader;
		private System.Windows.Forms.PictureBox DSPictureBox;
		private System.Windows.Forms.ColumnHeader TimeColHeader;
		private System.Windows.Forms.ContextMenuStrip RefreshContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem RefreshToolStripMenuItem;
		private System.Windows.Forms.TabControl TabDetails;
		private System.Windows.Forms.TabPage TabPageOpTrace;
		private System.Windows.Forms.TabPage TabPageActions;
		private System.Windows.Forms.GroupBox GBoxActions;
		private System.Windows.Forms.Label LblInit;
		private System.Windows.Forms.Button BtnInit;
		private System.Windows.Forms.ToolStripMenuItem ClearToolStripMenuItem;
	}
}