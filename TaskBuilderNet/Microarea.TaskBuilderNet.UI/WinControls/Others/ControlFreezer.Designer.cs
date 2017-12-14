using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
    partial class ControlFreezer
    {
        private System.Windows.Forms.PictureBox staticImage;
        private Control sourceControl;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlFreezer));
            this.staticImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.staticImage)).BeginInit();
            this.SuspendLayout();
            // 
            // staticImage
            // 
            resources.ApplyResources(this.staticImage, "staticImage");
            this.staticImage.Name = "staticImage";
            this.staticImage.TabStop = false;
            // 
            // ControlFreezer
            // 
            this.Controls.Add(this.staticImage);
            this.Name = "ControlFreezer";
            ((System.ComponentModel.ISupportInitialize)(this.staticImage)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
