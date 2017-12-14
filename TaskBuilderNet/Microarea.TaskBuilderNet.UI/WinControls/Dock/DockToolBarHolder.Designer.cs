
namespace Microarea.TaskBuilderNet.UI.WinControls.DockToolBar
{
    partial class DockToolBarHolder
    {
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.ToolBar toolBar = null;
        private System.Windows.Forms.Form floatingForm = new System.Windows.Forms.Form();
        private System.Windows.Forms.Panel panel = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DockToolBarHolder));
            this.panel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panel.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.panel, "panel");
            this.panel.Name = "panel";
            // 
            // DockToolBarHolder
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.panel);
            resources.ApplyResources(this, "$this");
            this.Name = "DockToolBarHolder";
            this.ResumeLayout(false);

        }
        #endregion

    }
}
