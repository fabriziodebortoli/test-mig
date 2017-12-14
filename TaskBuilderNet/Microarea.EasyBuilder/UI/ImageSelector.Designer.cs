namespace Microarea.EasyBuilder.UI
{
	partial class ImageSelector
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.listViewModuleImages = new System.Windows.Forms.ListView();
			this.columnHeaderImage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tsbImport = new System.Windows.Forms.ToolStripButton();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// listViewModuleImages
			// 
			this.listViewModuleImages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderImage});
			this.listViewModuleImages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewModuleImages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewModuleImages.HideSelection = false;
			this.listViewModuleImages.Location = new System.Drawing.Point(0, 0);
			this.listViewModuleImages.MultiSelect = false;
			this.listViewModuleImages.Name = "listViewModuleImages";
			this.listViewModuleImages.Size = new System.Drawing.Size(246, 386);
			this.listViewModuleImages.TabIndex = 0;
			this.listViewModuleImages.UseCompatibleStateImageBehavior = false;
			this.listViewModuleImages.View = System.Windows.Forms.View.Details;
			this.listViewModuleImages.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewModuleImages_MouseDoubleClick);
			// 
			// columnHeaderImage
			// 
			this.columnHeaderImage.Text = "Image";
			this.columnHeaderImage.Width = 242;
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.listViewModuleImages);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(246, 386);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.Size = new System.Drawing.Size(246, 413);
			this.toolStripContainer1.TabIndex = 2;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
			// 
			// toolStrip1
			// 
			this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbImport});
			this.toolStrip1.Location = new System.Drawing.Point(3, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(101, 27);
			this.toolStrip1.TabIndex = 0;
			// 
			// tsbImport
			// 
			this.tsbImport.Image = global::Microarea.EasyBuilder.Properties.Resources.AddFile;
			this.tsbImport.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.tsbImport.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbImport.Name = "tsbImport";
			this.tsbImport.Size = new System.Drawing.Size(67, 24);
			this.tsbImport.Text = "Import";
			this.tsbImport.Click += new System.EventHandler(this.tsbImport_Click);
			// 
			// ImageSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.toolStripContainer1);
			this.Name = "ImageSelector";
			this.Size = new System.Drawing.Size(246, 413);
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView listViewModuleImages;
		private System.Windows.Forms.ColumnHeader columnHeaderImage;
		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton tsbImport;
	}
}
