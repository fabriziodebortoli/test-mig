
namespace Microarea.TaskBuilderNet.UI.WizardDialogLib
{
    partial class ExteriorWizardPage
    {
        protected System.Windows.Forms.PictureBox m_watermarkPicture;
        protected System.Windows.Forms.Label m_titleLabel;

        private System.ComponentModel.Container components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        //--------------------------------------------------------------------
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExteriorWizardPage));
            this.m_watermarkPicture = new System.Windows.Forms.PictureBox();
            this.m_titleLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.m_watermarkPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // m_watermarkPicture
            // 
            this.m_watermarkPicture.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.m_watermarkPicture, "m_watermarkPicture");
            this.m_watermarkPicture.Name = "m_watermarkPicture";
            this.m_watermarkPicture.TabStop = false;
            // 
            // m_titleLabel
            // 
            resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
            this.m_titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.m_titleLabel.Name = "m_titleLabel";
            // 
            // ExteriorWizardPage
            // 
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.m_titleLabel);
            this.Controls.Add(this.m_watermarkPicture);
            this.Name = "ExteriorWizardPage";
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)(this.m_watermarkPicture)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
