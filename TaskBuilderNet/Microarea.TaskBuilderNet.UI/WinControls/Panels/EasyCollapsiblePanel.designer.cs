namespace Microarea.TaskBuilderNet.UI.WinControls
{
    partial class EasyCollapsiblePanel
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EasyCollapsiblePanel));
			this.pnlHeader = new System.Windows.Forms.Panel();
			this.pictureBoxExpandCollapse = new System.Windows.Forms.PictureBox();
			this.pictureBoxImage = new System.Windows.Forms.PictureBox();
			this.timerAnimation = new System.Windows.Forms.Timer(this.components);
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.PanelImageList = new System.Windows.Forms.ImageList(this.components);
			this.pnlHeader.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxExpandCollapse)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage)).BeginInit();
			this.SuspendLayout();
			// 
			// pnlHeader
			// 
			resources.ApplyResources(this.pnlHeader, "pnlHeader");
			this.pnlHeader.BackColor = System.Drawing.Color.Transparent;
			this.pnlHeader.Controls.Add(this.pictureBoxExpandCollapse);
			this.pnlHeader.Controls.Add(this.pictureBoxImage);
			this.pnlHeader.Name = "pnlHeader";
			this.pnlHeader.MouseLeave += new System.EventHandler(this.pnlHeader_MouseLeave);
			this.pnlHeader.MouseHover += new System.EventHandler(this.pnlHeader_MouseHover);
			// 
			// pictureBoxExpandCollapse
			// 
			resources.ApplyResources(this.pictureBoxExpandCollapse, "pictureBoxExpandCollapse");
			this.pictureBoxExpandCollapse.BackColor = System.Drawing.Color.Transparent;
			this.pictureBoxExpandCollapse.Name = "pictureBoxExpandCollapse";
			this.pictureBoxExpandCollapse.TabStop = false;
			this.pictureBoxExpandCollapse.Click += new System.EventHandler(this.pictureBoxExpandCollapse_Click);
			this.pictureBoxExpandCollapse.MouseLeave += new System.EventHandler(this.pictureBoxExpandCollapse_MouseLeave);
			this.pictureBoxExpandCollapse.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxExpandCollapse_MouseMove);
			// 
			// pictureBoxImage
			// 
			this.pictureBoxImage.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.pictureBoxImage, "pictureBoxImage");
			this.pictureBoxImage.Name = "pictureBoxImage";
			this.pictureBoxImage.TabStop = false;
			// 
			// timerAnimation
			// 
			this.timerAnimation.Interval = 50;
			this.timerAnimation.Tick += new System.EventHandler(this.timerAnimation_Tick);
			// 
			// PanelImageList
			// 
			this.PanelImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			resources.ApplyResources(this.PanelImageList, "PanelImageList");
			this.PanelImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// EasyCollapsiblePanel
			// 
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.pnlHeader);
			resources.ApplyResources(this, "$this");
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pnlHeader.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxExpandCollapse)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox pictureBoxImage;
        private System.Windows.Forms.Timer timerAnimation;
        private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.ImageList PanelImageList;
		private System.Windows.Forms.PictureBox pictureBoxExpandCollapse;
    }
}
