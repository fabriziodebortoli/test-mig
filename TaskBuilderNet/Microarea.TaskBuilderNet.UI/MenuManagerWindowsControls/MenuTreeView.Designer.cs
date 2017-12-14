
namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
    partial class MenuTreeView
    {

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
        
        //--------------------------------------------------------------------------------------------------------------------------------
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.typeImageList = new System.Windows.Forms.ImageList(this.components);
			this.stateImageList = new System.Windows.Forms.ImageList(this.components);
			this.CommandToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// typeImageList
			// 
			this.typeImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.typeImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.typeImageList.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// stateImageList
			// 
			this.stateImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.stateImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.stateImageList.TransparentColor = System.Drawing.Color.Magenta;
			this.ResumeLayout(false);

        }

        private System.ComponentModel.IContainer components;

    }
}
