using Microarea.TaskBuilderNet.Core.Generic;
namespace Microarea.TaskBuilderNet.UI.WinControls
{
    partial class Splash
    {
		public SmoothProgressBar Progressbar;
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox SplashBox;

        /// <summary>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Splash));
			this.SplashBox = new System.Windows.Forms.PictureBox();
			this.Progressbar = new Microarea.TaskBuilderNet.Core.Generic.SmoothProgressBar();
			((System.ComponentModel.ISupportInitialize)(this.SplashBox)).BeginInit();
			this.SuspendLayout();
			// 
			// SplashBox
			// 
			this.SplashBox.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.SplashBox, "SplashBox");
			this.SplashBox.Name = "SplashBox";
			this.SplashBox.TabStop = false;
			// 
			// Progressbar
			// 
			resources.ApplyResources(this.Progressbar, "Progressbar");
			this.Progressbar.BackColor = System.Drawing.Color.White;
			this.Progressbar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(185)))), ((int)(((byte)(232)))));
			this.Progressbar.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(185)))), ((int)(((byte)(232)))));
			this.Progressbar.GradientStartColor = System.Drawing.Color.Lavender;
			this.Progressbar.Name = "Progressbar";
			this.Progressbar.Step = 1;
			this.Progressbar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			// 
			// Splash
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Window;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.Progressbar);
			this.Controls.Add(this.SplashBox);
			this.ForeColor = System.Drawing.SystemColors.WindowText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "Splash";
			this.ShowInTaskbar = false;
			((System.ComponentModel.ISupportInitialize)(this.SplashBox)).EndInit();
			this.ResumeLayout(false);

        }
        #endregion
    }
}
