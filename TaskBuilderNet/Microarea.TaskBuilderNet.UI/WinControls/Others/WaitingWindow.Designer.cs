
namespace Microarea.TaskBuilderNet.UI.WinControls
{
    partial class WaitingWindow
    {
        private System.Windows.Forms.Label LblMessage;
        private System.ComponentModel.Container components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaitingWindow));
            this.LblMessage = new System.Windows.Forms.Label();
            this.PbWaiting = new Microarea.TaskBuilderNet.Core.Generic.SmoothProgressBar();
            this.SuspendLayout();
            // 
            // LblMessage
            // 
            resources.ApplyResources(this.LblMessage, "LblMessage");
            this.LblMessage.Name = "LblMessage";
            // 
            // PbWaiting
            // 
            resources.ApplyResources(this.PbWaiting, "PbWaiting");
            this.PbWaiting.BackColor = System.Drawing.Color.White;
            this.PbWaiting.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(185)))), ((int)(((byte)(232)))));
            this.PbWaiting.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(185)))), ((int)(((byte)(232)))));
            this.PbWaiting.GradientStartColor = System.Drawing.Color.Lavender;
            this.PbWaiting.Name = "PbWaiting";
            this.PbWaiting.Step = 1;
            this.PbWaiting.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // WaitingWindow
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.White;
            this.ControlBox = false;
            this.Controls.Add(this.PbWaiting);
            this.Controls.Add(this.LblMessage);
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WaitingWindow";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.ResumeLayout(false);

        }
        #endregion

        public Core.Generic.SmoothProgressBar PbWaiting;
    }
}
