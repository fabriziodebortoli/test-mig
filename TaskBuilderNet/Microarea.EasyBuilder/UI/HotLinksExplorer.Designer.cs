using System.Windows.Forms;

namespace Microarea.EasyBuilder.UI
{
	partial class HotLinksExplorer
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
			this.toolBar = new System.Windows.Forms.ToolStrip();
			this.tbShowTemplates = new System.Windows.Forms.ToolStripButton();
			this.treeHotLinks = new System.Windows.Forms.TreeView();
			this.lblTreeCaption = new System.Windows.Forms.Label();
			this.toolBar.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolBar
			// 
			this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbShowTemplates});
			this.toolBar.Location = new System.Drawing.Point(0, 0);
			this.toolBar.Name = "toolBar";
			this.toolBar.Size = new System.Drawing.Size(459, 25);
			this.toolBar.TabIndex = 0;
			this.toolBar.Text = "toolStrip1";
			// 
			// tbShowTemplates
			// 
			this.tbShowTemplates.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.tbShowTemplates.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tbShowTemplates.Image = global::Microarea.EasyBuilder.Properties.Resources.Repository;
			this.tbShowTemplates.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tbShowTemplates.Name = "tbShowTemplates";
			this.tbShowTemplates.Size = new System.Drawing.Size(23, 22);
			this.tbShowTemplates.Text = "Show/Hide Templates";
			this.tbShowTemplates.Click += new System.EventHandler(this.tbShowTemplates_Click);
			// 
			// treeHotLinks
			// 
			this.treeHotLinks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeHotLinks.Location = new System.Drawing.Point(3, 51);
			this.treeHotLinks.Name = "treeHotLinks";
			this.treeHotLinks.Size = new System.Drawing.Size(453, 454);
			this.treeHotLinks.TabIndex = 1;
			this.treeHotLinks.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeHotLinks_MouseDoubleClick);
			this.treeHotLinks.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeHotLinks_MouseDown);
			this.treeHotLinks.MouseMove += new System.Windows.Forms.MouseEventHandler(this.treeHotLinks_MouseMove);
			// 
			// lblTreeCaption
			// 
			this.lblTreeCaption.AutoSize = true;
			this.lblTreeCaption.Font = new System.Drawing.Font("Verdana", 9F);
			this.lblTreeCaption.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.lblTreeCaption.Location = new System.Drawing.Point(3, 27);
			this.lblTreeCaption.Name = "lblTreeCaption";
			this.lblTreeCaption.Size = new System.Drawing.Size(125, 14);
			this.lblTreeCaption.TabIndex = 2;
			this.lblTreeCaption.Text = "Available HotLinks ";
			// 
			// HotLinksExplorer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblTreeCaption);
			this.Controls.Add(this.treeHotLinks);
			this.Controls.Add(this.toolBar);
			this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "HotLinksExplorer";
			this.Size = new System.Drawing.Size(459, 499);
			this.toolBar.ResumeLayout(false);
			this.toolBar.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolBar;
		private System.Windows.Forms.TreeView treeHotLinks;
		private System.Windows.Forms.Label lblTreeCaption;
		private System.Windows.Forms.ToolStripButton tbShowTemplates;

		/// <summary>
		/// 
		/// </summary>
		public TreeView TreeHotLinks
		{
			get { return treeHotLinks; 	}
			set { treeHotLinks = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public ToolStrip ToolBar
		{
			get { return toolBar; }
			set { toolBar = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Label LblTreeCaption
		{
			get { return lblTreeCaption; }
			set { lblTreeCaption = value; }
		}
	}
}
