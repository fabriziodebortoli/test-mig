using Microarea.TaskBuilderNet.Core.Generic;
namespace ManifestGenerator
{
	partial class Output
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
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
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Output));
			this.LogTextBox = new System.Windows.Forms.RichTextBox();
			this.ShortDescriptionLabel = new System.Windows.Forms.Label();
			this.DetailButton = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.ProgressBar = new Microarea.TaskBuilderNet.Core.Generic.SmoothProgressBar();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// LogTextBox
			// 
			resources.ApplyResources(this.LogTextBox, "LogTextBox");
			this.LogTextBox.BackColor = System.Drawing.Color.Lavender;
			this.LogTextBox.Name = "LogTextBox";
			this.LogTextBox.ReadOnly = true;
			this.LogTextBox.UseWaitCursor = true;
			// 
			// ShortDescriptionLabel
			// 
			this.ShortDescriptionLabel.AutoEllipsis = true;
			resources.ApplyResources(this.ShortDescriptionLabel, "ShortDescriptionLabel");
			this.ShortDescriptionLabel.Name = "ShortDescriptionLabel";
			this.ShortDescriptionLabel.UseWaitCursor = true;
			// 
			// DetailButton
			// 
			this.DetailButton.Cursor = System.Windows.Forms.Cursors.WaitCursor;
			resources.ApplyResources(this.DetailButton, "DetailButton");
			this.DetailButton.Name = "DetailButton";
			this.DetailButton.UseVisualStyleBackColor = true;
			this.DetailButton.UseWaitCursor = true;
			this.DetailButton.Click += new System.EventHandler(this.DetailButton_Click);
			// 
			// pictureBox1
			// 
			resources.ApplyResources(this.pictureBox1, "pictureBox1");
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.TabStop = false;
			this.pictureBox1.UseWaitCursor = true;
			// 
			// ProgressBar
			// 
			resources.ApplyResources(this.ProgressBar, "ProgressBar");
			this.ProgressBar.BackColor = System.Drawing.Color.White;
			this.ProgressBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(197)))), ((int)(((byte)(243)))));
			this.ProgressBar.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(197)))), ((int)(((byte)(243)))));
			this.ProgressBar.GradientStartColor = System.Drawing.Color.Lavender;
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.ProgressBar.UseWaitCursor = true;
			// 
			// Output
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.ProgressBar);
			this.Controls.Add(this.DetailButton);
			this.Controls.Add(this.ShortDescriptionLabel);
			this.Controls.Add(this.LogTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Output";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.UseWaitCursor = true;
			this.Load += new System.EventHandler(this.Output_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.RichTextBox LogTextBox;
		public System.Windows.Forms.Label ShortDescriptionLabel;
		private System.Windows.Forms.Button DetailButton;
		private SmoothProgressBar ProgressBar;
		private System.Windows.Forms.PictureBox pictureBox1;

	}
}