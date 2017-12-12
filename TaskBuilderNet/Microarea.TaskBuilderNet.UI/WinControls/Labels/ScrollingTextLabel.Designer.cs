
namespace Microarea.TaskBuilderNet.UI.WinControls.Labels
{
    partial class ScrollingTextLabel
    {
		private System.Windows.Forms.Label textLabel;
        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        //--------------------------------------------------------------------------------
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
        //--------------------------------------------------------------------------------
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScrollingTextLabel));
			this.textLabel = new System.Windows.Forms.Label();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// textLabel
			// 
			resources.ApplyResources(this.textLabel, "textLabel");
			this.textLabel.Name = "textLabel";
			// 
			// timer1
			// 
			this.timer.Tick += new System.EventHandler(this.Timer_Tick);
			// 
			// ScrollingTextLabel
			// 
			this.Controls.Add(this.textLabel);
			this.Name = "ScrollingTextLabel";
			resources.ApplyResources(this, "$this");
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

		private System.Windows.Forms.Timer timer;
		private System.ComponentModel.IContainer components;
    }
}
