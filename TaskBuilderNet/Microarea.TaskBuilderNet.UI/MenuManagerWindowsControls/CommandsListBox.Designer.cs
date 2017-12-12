
namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
    partial class CommandsListBox
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.typeImageList = new System.Windows.Forms.ImageList(this.components);
            this.stateImageList = new System.Windows.Forms.ImageList(this.components);
            // 
            // typeImageList
            // 
            this.typeImageList.ImageSize = new System.Drawing.Size(24, 24);
            this.typeImageList.TransparentColor = System.Drawing.Color.Magenta;
            // 
            // stateImageList
            // 
            this.stateImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.stateImageList.TransparentColor = System.Drawing.Color.Magenta;
            // 
            // CommandsListBox
            // 
            this.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.SelectionMode = System.Windows.Forms.SelectionMode.One;
            this.SelectedIndexChanged += new System.EventHandler(this.CommandsListView_SelectedIndexChanged);
        }

        #endregion
    }
}
