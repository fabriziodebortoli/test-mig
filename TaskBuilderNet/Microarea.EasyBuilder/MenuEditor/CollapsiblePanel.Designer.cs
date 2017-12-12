
namespace Microarea.EasyBuilder.MenuEditor
{
    partial class CollapsiblePanel
    {
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Label TitleLabel;
        private System.Windows.Forms.ImageList PanelImageList;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
				if (components != null)
				{
					components.Dispose();
					components = null;
				}
				if (titleMouseDownTimer != null)
				{
					titleMouseDownTimer.Dispose();
					titleMouseDownTimer = null;
				}
				if (grayAttributes != null)
				{
					grayAttributes.Dispose();
					grayAttributes = null;
				}
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.TitleLabel = new System.Windows.Forms.Label();
			this.PanelImageList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// TitleLabel
			// 
			this.TitleLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.TitleLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TitleLabel.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Bold);
			this.TitleLabel.ForeColor = System.Drawing.Color.Navy;
			this.TitleLabel.Location = new System.Drawing.Point(0, 0);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Size = new System.Drawing.Size(200, 24);
			this.TitleLabel.TabIndex = 0;
			this.TitleLabel.TabStop = true;
			this.TitleLabel.Text = "Title";
			this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.TitleLabel.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TitleLabel_KeyUp);
			this.TitleLabel.SizeChanged += new System.EventHandler(this.TitleLabel_SizeChanged);
			this.TitleLabel.Paint += new System.Windows.Forms.PaintEventHandler(this.TitleLabel_Paint);
			this.TitleLabel.GotFocus += new System.EventHandler(this.TitleLabel_GotFocus);
			this.TitleLabel.LostFocus += new System.EventHandler(this.TitleLabel_LostFocus);
			this.TitleLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleLabel_MouseDown);
			this.TitleLabel.MouseLeave += new System.EventHandler(this.TitleLabel_MouseLeave);
			this.TitleLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TitleLabel_MouseMove);
			this.TitleLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TitleLabel_MouseUp);
			// 
			// PanelImageList
			// 
			this.PanelImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.PanelImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.PanelImageList.TransparentColor = System.Drawing.Color.White;
			// 
			// CollapsiblePanel
			// 
			this.Controls.Add(this.TitleLabel);
			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold);
			this.TabStop = true;
			this.ResumeLayout(false);

        }
        #endregion
    }
}
