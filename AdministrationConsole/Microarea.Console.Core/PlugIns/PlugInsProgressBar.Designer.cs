using System.Windows.Forms;

namespace Microarea.Console.Core.PlugIns
{
    partial class PlugInsProgressBar
    {
        private ProgressBar plugInProgressBar;
        private Label textProgressBar;
        private System.ComponentModel.Container components = null;


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlugInsProgressBar));
            this.plugInProgressBar = new System.Windows.Forms.ProgressBar();
            this.textProgressBar = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // plugInProgressBar
            // 
            resources.ApplyResources(this.plugInProgressBar, "plugInProgressBar");
            this.plugInProgressBar.Name = "plugInProgressBar";
            // 
            // textProgressBar
            // 
            this.textProgressBar.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.textProgressBar, "textProgressBar");
            this.textProgressBar.Name = "textProgressBar";
            // 
            // PlugInsProgressBar
            // 
            this.AllowDrop = true;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.plugInProgressBar);
            this.Controls.Add(this.textProgressBar);
            this.Name = "PlugInsProgressBar";
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);

        }
        #endregion

    }
}
