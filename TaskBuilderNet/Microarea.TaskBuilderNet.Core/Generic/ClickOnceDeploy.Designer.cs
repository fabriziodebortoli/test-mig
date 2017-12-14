namespace Microarea.TaskBuilderNet.Core.Generic
{
	partial class ClickOnceDeploy
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClickOnceDeploy));
			this.downloadProgress = new Microarea.TaskBuilderNet.Core.Generic.SmoothProgressBar();
			this.labelDescription = new System.Windows.Forms.Label();
			this.messageLabel = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// downloadProgress
			// 
			resources.ApplyResources(this.downloadProgress, "downloadProgress");
			this.downloadProgress.BackColor = System.Drawing.Color.Transparent;
			this.downloadProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(197)))), ((int)(((byte)(243)))));
			this.downloadProgress.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(197)))), ((int)(((byte)(243)))));
			this.downloadProgress.GradientStartColor = System.Drawing.Color.Lavender;
			this.downloadProgress.Name = "downloadProgress";
			this.downloadProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.downloadProgress.UseWaitCursor = true;
			// 
			// labelDescription
			// 
			this.labelDescription.AutoEllipsis = true;
			resources.ApplyResources(this.labelDescription, "labelDescription");
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.UseWaitCursor = true;
			// 
			// messageLabel
			// 
			this.messageLabel.AutoEllipsis = true;
			resources.ApplyResources(this.messageLabel, "messageLabel");
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.UseWaitCursor = true;
			// 
			// pictureBox1
			// 
			resources.ApplyResources(this.pictureBox1, "pictureBox1");
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.TabStop = false;
			this.pictureBox1.UseWaitCursor = true;
			// 
			// ClickOnceDeploy
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.messageLabel);
			this.Controls.Add(this.labelDescription);
			this.Controls.Add(this.downloadProgress);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ClickOnceDeploy";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.TopMost = true;
			this.UseWaitCursor = true;
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private SmoothProgressBar downloadProgress;
		private System.Windows.Forms.Label labelDescription;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}