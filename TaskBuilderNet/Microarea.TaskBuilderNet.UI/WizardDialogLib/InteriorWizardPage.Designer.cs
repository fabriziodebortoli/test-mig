
namespace Microarea.TaskBuilderNet.UI.WizardDialogLib
{
    partial class InteriorWizardPage
    {
        protected System.Windows.Forms.Panel m_headerPanel;
        protected System.Windows.Forms.PictureBox m_headerPicture;
        protected System.Windows.Forms.Label m_titleLabel;
        protected System.Windows.Forms.Label m_subtitleLabel;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InteriorWizardPage));
            this.m_headerPanel = new System.Windows.Forms.Panel();
            this.m_subtitleLabel = new System.Windows.Forms.Label();
            this.m_titleLabel = new System.Windows.Forms.Label();
            this.m_headerPicture = new System.Windows.Forms.PictureBox();
            this.m_headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // m_headerPanel
            // 
            resources.ApplyResources(this.m_headerPanel, "m_headerPanel");
            this.m_headerPanel.BackColor = System.Drawing.Color.White;
            this.m_headerPanel.Controls.Add(this.m_subtitleLabel);
            this.m_headerPanel.Controls.Add(this.m_titleLabel);
            this.m_headerPanel.Controls.Add(this.m_headerPicture);
            this.m_headerPanel.Name = "m_headerPanel";
            // 
            // m_subtitleLabel
            // 
            resources.ApplyResources(this.m_subtitleLabel, "m_subtitleLabel");
            this.m_subtitleLabel.BackColor = System.Drawing.Color.Transparent;
            this.m_subtitleLabel.Name = "m_subtitleLabel";
            // 
            // m_titleLabel
            // 
            resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
            this.m_titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.m_titleLabel.Name = "m_titleLabel";
            // 
            // m_headerPicture
            // 
            resources.ApplyResources(this.m_headerPicture, "m_headerPicture");
            this.m_headerPicture.BackColor = System.Drawing.Color.White;
            this.m_headerPicture.Name = "m_headerPicture";
            this.m_headerPicture.TabStop = false;
            // 
            // InteriorWizardPage
            // 
            this.Controls.Add(this.m_headerPanel);
            this.Name = "InteriorWizardPage";
            resources.ApplyResources(this, "$this");
            this.m_headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
