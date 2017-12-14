namespace Microarea.TBPicComponents
{
	partial class TBThumbnailEx
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
			this.thumbnailEx = new GdPicture12.ThumbnailEx();
			this.SuspendLayout();
			// 
			// thumbnailEx
			// 
			this.thumbnailEx.AllowDropFiles = false;
			this.thumbnailEx.AllowMoveItems = false;
			this.thumbnailEx.BackColor = System.Drawing.SystemColors.Control;
			this.thumbnailEx.CheckBoxes = false;
			this.thumbnailEx.CheckBoxesMarginLeft = 0;
			this.thumbnailEx.CheckBoxesMarginTop = 0;
			this.thumbnailEx.DisplayAnnotations = true;
			this.thumbnailEx.Dock = System.Windows.Forms.DockStyle.Fill;
			this.thumbnailEx.HorizontalTextAlignment = GdPicture12.TextAlignment.TextAlignmentCenter;
			this.thumbnailEx.HotTracking = false;
			this.thumbnailEx.Location = new System.Drawing.Point(0, 0);
			this.thumbnailEx.LockGdViewerEvents = false;
			this.thumbnailEx.MultiSelect = false;
			this.thumbnailEx.Name = "thumbnailEx";
			this.thumbnailEx.OwnDrop = false;
			this.thumbnailEx.PauseThumbsLoading = false;
			this.thumbnailEx.PreloadAllItems = true;
			this.thumbnailEx.RotateExif = true;
			this.thumbnailEx.SelectedThumbnailBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.thumbnailEx.ShowText = true;
			this.thumbnailEx.Size = new System.Drawing.Size(150, 150);
			this.thumbnailEx.TabIndex = 0;
			this.thumbnailEx.TextMarginLeft = 0;
			this.thumbnailEx.TextMarginTop = 0;
			this.thumbnailEx.ThumbnailBackColor = System.Drawing.Color.Gray;
			this.thumbnailEx.ThumbnailBorder = true;
			this.thumbnailEx.ThumbnailSize = new System.Drawing.Size(64, 64);
			this.thumbnailEx.ThumbnailSpacing = new System.Drawing.Size(0, 0);
			this.thumbnailEx.VerticalTextAlignment = GdPicture12.TextAlignment.TextAlignmentCenter;
			// 
			// TBThumbnailEx
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.thumbnailEx);
			this.Name = "TBThumbnailEx";
			this.ResumeLayout(false);

		}

		#endregion

		private GdPicture12.ThumbnailEx thumbnailEx;
	}
}
