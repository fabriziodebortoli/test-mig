
namespace Microarea.Console.Core.DataManager.Common
{
    partial class Images
    {
        #region Private members
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ImageList smallPictureImageList;
        private System.Windows.Forms.ImageList largePictureImageList;
        private System.ComponentModel.IContainer components;
        #endregion

        #region Dispose Method
        //---------------------------------------------------------------------------
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
        #endregion

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.smallPictureImageList = new System.Windows.Forms.ImageList(this.components);
            this.largePictureImageList = new System.Windows.Forms.ImageList(this.components);
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Magenta;
            // 
            // smallPictureImageList
            // 
            this.smallPictureImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.smallPictureImageList.ImageSize = new System.Drawing.Size(47, 47);
            this.smallPictureImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // largePictureImageList
            // 
            this.largePictureImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.largePictureImageList.ImageSize = new System.Drawing.Size(255, 255);
            this.largePictureImageList.TransparentColor = System.Drawing.Color.Transparent;

        }
        #endregion
    }
    
}
