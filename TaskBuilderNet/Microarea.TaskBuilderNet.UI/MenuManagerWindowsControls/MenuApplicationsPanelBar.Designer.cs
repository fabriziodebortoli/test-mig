
namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
    partial class MenuApplicationsPanelBar
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuApplicationsPanelBar));
            this.BrandedMenuLogoPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.BrandedMenuLogoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // BrandedMenuLogoPictureBox
            // 
            resources.ApplyResources(this.BrandedMenuLogoPictureBox, "BrandedMenuLogoPictureBox");
            this.BrandedMenuLogoPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.BrandedMenuLogoPictureBox.Name = "BrandedMenuLogoPictureBox";
            this.BrandedMenuLogoPictureBox.TabStop = false;
            // 
            // MenuApplicationsPanelBar
            // 
            this.Controls.Add(this.BrandedMenuLogoPictureBox);
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)(this.BrandedMenuLogoPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.PictureBox BrandedMenuLogoPictureBox;

        #endregion
    }
}