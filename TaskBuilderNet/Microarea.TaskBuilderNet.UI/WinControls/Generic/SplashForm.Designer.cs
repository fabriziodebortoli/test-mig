
namespace Microarea.TaskBuilderNet.UI.WinControls
{
    partial class SplashForm
    {
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
            this.AnimationPanel = new WinControls.AnimationPanel();
            this.SuspendLayout();
            // 
            // AnimationPanel
            // 
            this.AnimationPanel.AnimatedImage = null;
            this.AnimationPanel.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.AnimationPanel, "AnimationPanel");
            this.AnimationPanel.Name = "AnimationPanel";
            this.AnimationPanel.RepeatAnimationForever = false;
            this.AnimationPanel.SizeMode = AnimatedImageSizeMode.AutoSize;
            // 
            // SplashForm
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.White;
            this.ControlBox = false;
            this.Controls.Add(this.AnimationPanel);
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashForm";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
