namespace Microarea.TBPicComponents
{
	partial class TBPicViewerAdv
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
			this.GdViewerToolStrip = new System.Windows.Forms.ToolStrip();
			this.ZoomInToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ZoomOutToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.PreviousToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.NextToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ToolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.AddTagViaOCRToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.PreviewPictureBox = new System.Windows.Forms.PictureBox();
			this.LblNAPreview = new System.Windows.Forms.Label();
			this.GdViewerToolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// GdViewerToolStrip
			// 
			this.GdViewerToolStrip.BackColor = System.Drawing.Color.Lavender;
			this.GdViewerToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.GdViewerToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ZoomInToolStripButton,
            this.ZoomOutToolStripButton,
            this.ToolStripSeparator1,
            this.PreviousToolStripButton,
            this.NextToolStripButton,
            this.ToolStripSeparator2,
            this.AddTagViaOCRToolStripButton});
			this.GdViewerToolStrip.Location = new System.Drawing.Point(0, 0);
			this.GdViewerToolStrip.Name = "GdViewerToolStrip";
			this.GdViewerToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.GdViewerToolStrip.Size = new System.Drawing.Size(238, 25);
			this.GdViewerToolStrip.TabIndex = 2;
			// 
			// ZoomInToolStripButton
			// 
			this.ZoomInToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ZoomInToolStripButton.Image = global::Microarea.TBPicComponents.Properties.Resources.ZoomIn_16;
			this.ZoomInToolStripButton.Name = "ZoomInToolStripButton";
			this.ZoomInToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.ZoomInToolStripButton.Text = "Zoom in";
			this.ZoomInToolStripButton.Visible = false;
			this.ZoomInToolStripButton.Click += new System.EventHandler(this.ZoomInToolStripButton_Click);
			// 
			// ZoomOutToolStripButton
			// 
			this.ZoomOutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ZoomOutToolStripButton.Image = global::Microarea.TBPicComponents.Properties.Resources.ZoomOut_16;
			this.ZoomOutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ZoomOutToolStripButton.Name = "ZoomOutToolStripButton";
			this.ZoomOutToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.ZoomOutToolStripButton.Text = "Zoom out";
			this.ZoomOutToolStripButton.Visible = false;
			this.ZoomOutToolStripButton.Click += new System.EventHandler(this.ZoomOutToolStripButton_Click);
			// 
			// ToolStripSeparator1
			// 
			this.ToolStripSeparator1.Name = "ToolStripSeparator1";
			this.ToolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			this.ToolStripSeparator1.Visible = false;
			// 
			// PreviousToolStripButton
			// 
			this.PreviousToolStripButton.AutoToolTip = false;
			this.PreviousToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.PreviousToolStripButton.Image = global::Microarea.TBPicComponents.Properties.Resources.Up_16;
			this.PreviousToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.PreviousToolStripButton.Name = "PreviousToolStripButton";
			this.PreviousToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.PreviousToolStripButton.Text = "Previous";
			this.PreviousToolStripButton.ToolTipText = "Previous page";
			this.PreviousToolStripButton.Visible = false;
			this.PreviousToolStripButton.Click += new System.EventHandler(this.PreviousToolStripButton_Click);
			// 
			// NextToolStripButton
			// 
			this.NextToolStripButton.AutoToolTip = false;
			this.NextToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.NextToolStripButton.Image = global::Microarea.TBPicComponents.Properties.Resources.Down_16;
			this.NextToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.NextToolStripButton.Name = "NextToolStripButton";
			this.NextToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.NextToolStripButton.Text = "Next";
			this.NextToolStripButton.ToolTipText = "Next page";
			this.NextToolStripButton.Visible = false;
			this.NextToolStripButton.Click += new System.EventHandler(this.NextToolStripButton_Click);
			// 
			// ToolStripSeparator2
			// 
			this.ToolStripSeparator2.Name = "ToolStripSeparator2";
			this.ToolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			this.ToolStripSeparator2.Visible = false;
			// 
			// AddTagViaOCRToolStripButton
			// 
			this.AddTagViaOCRToolStripButton.Image = global::Microarea.TBPicComponents.Properties.Resources.Plus_16;
			this.AddTagViaOCRToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.AddTagViaOCRToolStripButton.Name = "AddTagViaOCRToolStripButton";
			this.AddTagViaOCRToolStripButton.Size = new System.Drawing.Size(114, 22);
			this.AddTagViaOCRToolStripButton.Text = "Add tag via OCR";
			this.AddTagViaOCRToolStripButton.Visible = false;
			this.AddTagViaOCRToolStripButton.Click += new System.EventHandler(this.AddTagViaOCRToolStripButton_Click);
			// 
			// PreviewPictureBox
			// 
			this.PreviewPictureBox.Image = global::Microarea.TBPicComponents.Properties.Resources.PreviewNotAvailable_100;
			this.PreviewPictureBox.Location = new System.Drawing.Point(69, 74);
			this.PreviewPictureBox.Name = "PreviewPictureBox";
			this.PreviewPictureBox.Size = new System.Drawing.Size(100, 100);
			this.PreviewPictureBox.TabIndex = 3;
			this.PreviewPictureBox.TabStop = false;
			this.PreviewPictureBox.Visible = false;
			// 
			// LblNAPreview
			// 
			this.LblNAPreview.AutoSize = true;
			this.LblNAPreview.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LblNAPreview.Location = new System.Drawing.Point(55, 186);
			this.LblNAPreview.Name = "LblNAPreview";
			this.LblNAPreview.Size = new System.Drawing.Size(0, 13);
			this.LblNAPreview.TabIndex = 4;
			this.LblNAPreview.Visible = false;
			// 
			// TBPicViewerAdv
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.LblNAPreview);
			this.Controls.Add(this.PreviewPictureBox);
			this.Controls.Add(this.GdViewerToolStrip);
			this.Name = "TBPicViewerAdv";
			this.Size = new System.Drawing.Size(238, 248);
			this.Controls.SetChildIndex(this.GdViewerToolStrip, 0);
			this.Controls.SetChildIndex(this.PreviewPictureBox, 0);
			this.Controls.SetChildIndex(this.LblNAPreview, 0);
			this.GdViewerToolStrip.ResumeLayout(false);
			this.GdViewerToolStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip GdViewerToolStrip;
		private System.Windows.Forms.ToolStripButton ZoomInToolStripButton;
		private System.Windows.Forms.ToolStripButton ZoomOutToolStripButton;
		private System.Windows.Forms.ToolStripSeparator ToolStripSeparator1;
		private System.Windows.Forms.ToolStripButton PreviousToolStripButton;
		private System.Windows.Forms.ToolStripButton NextToolStripButton;
		private System.Windows.Forms.ToolStripSeparator ToolStripSeparator2;
		private System.Windows.Forms.ToolStripButton AddTagViaOCRToolStripButton;
		private System.Windows.Forms.PictureBox PreviewPictureBox;
		private System.Windows.Forms.Label LblNAPreview;
	}
}
