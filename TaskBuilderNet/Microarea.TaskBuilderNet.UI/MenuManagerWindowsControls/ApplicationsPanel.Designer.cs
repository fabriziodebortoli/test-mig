
namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
    partial class MenuApplicationPanel
    {
        private System.Windows.Forms.LinkLabel TitleLabel;
        private System.Windows.Forms.ImageList PanelImageList;

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
            if (disposing)
            {
                if (titleImage != null)
                    titleImage.Dispose();
                
                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.TitleLabel = new System.Windows.Forms.LinkLabel();
            this.PanelImageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // TitleLabel
            // 
            this.TitleLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.TitleLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TitleLabel.ForeColor = System.Drawing.Color.Navy;
            this.TitleLabel.Location = new System.Drawing.Point(246, 0);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(200, 24);
            this.TitleLabel.TabIndex = 0;
            this.TitleLabel.TabStop = true;
            this.TitleLabel.Text = "Title";
            this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TitleLabel.Paint += new System.Windows.Forms.PaintEventHandler(this.TitleLabel_Paint);
            this.TitleLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TitleLabel_MouseUp);
            this.TitleLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TitleLabel_MouseMove);
            this.TitleLabel.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TitleLabel_KeyUp);
            // 
            // PanelImageList
            // 
            this.PanelImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.PanelImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.PanelImageList.TransparentColor = System.Drawing.Color.White;
            // 
            // MenuApplicationPanel
            // 
            this.Controls.Add(this.TitleLabel);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
