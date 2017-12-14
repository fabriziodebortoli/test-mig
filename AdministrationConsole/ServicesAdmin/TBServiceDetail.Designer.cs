using System.Windows.Forms;

namespace Microarea.Console.Plugin.ServicesAdmin
{
    partial class TBServiceDetail
    {
		private Panel panelThreads;
		private Panel panelTitle;
		private System.ComponentModel.IContainer components;
        protected Label SubtitleLabel;
		protected Label TitleLabel;
		private ImageList images;
		private ContextMenuStrip ThreadContextMenuStrip;

        /// <summary>
        /// Dispose
        /// Clean up any resources being used.
        /// </summary>
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
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TBServiceDetail));
			this.panelThreads = new System.Windows.Forms.Panel();
			this.panelDetails = new System.Windows.Forms.Panel();
			this.TwThreads = new System.Windows.Forms.TreeView();
			this.images = new System.Windows.Forms.ImageList(this.components);
			this.panelTitle = new System.Windows.Forms.Panel();
			this.SubtitleLabel = new System.Windows.Forms.Label();
			this.TitleLabel = new System.Windows.Forms.Label();
			this.ThreadContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.CloseStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RefreshAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.panelThreads.SuspendLayout();
			this.panelDetails.SuspendLayout();
			this.panelTitle.SuspendLayout();
			this.ThreadContextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelThreads
			// 
			this.panelThreads.AllowDrop = true;
			resources.ApplyResources(this.panelThreads, "panelThreads");
			this.panelThreads.BackColor = System.Drawing.Color.White;
			this.panelThreads.Controls.Add(this.panelDetails);
			this.panelThreads.Controls.Add(this.panelTitle);
			this.panelThreads.Name = "panelThreads";
			// 
			// panelDetails
			// 
			this.panelDetails.AllowDrop = true;
			resources.ApplyResources(this.panelDetails, "panelDetails");
			this.panelDetails.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelDetails.Controls.Add(this.TwThreads);
			this.panelDetails.Name = "panelDetails";
			// 
			// TwThreads
			// 
			resources.ApplyResources(this.TwThreads, "TwThreads");
			this.TwThreads.ImageList = this.images;
			this.TwThreads.Name = "TwThreads";
			this.TwThreads.ShowNodeToolTips = true;
			this.TwThreads.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Tree_MouseDown);
			// 
			// images
			// 
			this.images.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("images.ImageStream")));
			this.images.TransparentColor = System.Drawing.Color.Magenta;
			this.images.Images.SetKeyName(0, "process.PNG");
			this.images.Images.SetKeyName(1, "");
			this.images.Images.SetKeyName(2, "Doc.gif");
			// 
			// panelTitle
			// 
			this.panelTitle.BackColor = System.Drawing.Color.Lavender;
			this.panelTitle.Controls.Add(this.SubtitleLabel);
			this.panelTitle.Controls.Add(this.TitleLabel);
			resources.ApplyResources(this.panelTitle, "panelTitle");
			this.panelTitle.Name = "panelTitle";
			// 
			// SubtitleLabel
			// 
			resources.ApplyResources(this.SubtitleLabel, "SubtitleLabel");
			this.SubtitleLabel.BackColor = System.Drawing.Color.Lavender;
			this.SubtitleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SubtitleLabel.ForeColor = System.Drawing.Color.Navy;
			this.SubtitleLabel.Name = "SubtitleLabel";
			// 
			// TitleLabel
			// 
			resources.ApplyResources(this.TitleLabel, "TitleLabel");
			this.TitleLabel.BackColor = System.Drawing.Color.Lavender;
			this.TitleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TitleLabel.ForeColor = System.Drawing.Color.Navy;
			this.TitleLabel.Name = "TitleLabel";
			// 
			// ThreadContextMenuStrip
			// 
			this.ThreadContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CloseStripMenuItem,
            this.RefreshAllToolStripMenuItem});
			this.ThreadContextMenuStrip.Name = "ThreadContextMenuStrip";
			resources.ApplyResources(this.ThreadContextMenuStrip, "ThreadContextMenuStrip");
			// 
			// CloseStripMenuItem
			// 
			resources.ApplyResources(this.CloseStripMenuItem, "CloseStripMenuItem");
			this.CloseStripMenuItem.Name = "CloseStripMenuItem";
			this.CloseStripMenuItem.Click += new System.EventHandler(this.CloseStripMenuItem_Click);
			// 
			// RefreshAllToolStripMenuItem
			// 
			resources.ApplyResources(this.RefreshAllToolStripMenuItem, "RefreshAllToolStripMenuItem");
			this.RefreshAllToolStripMenuItem.Name = "RefreshAllToolStripMenuItem";
			this.RefreshAllToolStripMenuItem.Click += new System.EventHandler(this.refreshAllToolStripMenuItem_Click);
			// 
			// TBServiceDetail
			// 
			this.AllowDrop = true;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.panelThreads);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TBServiceDetail";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.TBServiceDetail_Load);
			this.panelThreads.ResumeLayout(false);
			this.panelDetails.ResumeLayout(false);
			this.panelTitle.ResumeLayout(false);
			this.ThreadContextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        #endregion

		private Panel panelDetails;
		private TreeView TwThreads;
		private ToolStripMenuItem CloseStripMenuItem;
		private ToolStripMenuItem RefreshAllToolStripMenuItem;




	}
}
