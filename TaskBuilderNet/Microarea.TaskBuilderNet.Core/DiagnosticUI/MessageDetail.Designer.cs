
namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
    partial class MessageDetail
    {
        private System.Windows.Forms.ImageList imagesItem;
        private System.Windows.Forms.RichTextBox DetailRichTextBox;
        private System.Windows.Forms.Label LabelExtendedInfo;
        private MessagesListBox DetailMessage;

        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageDetail));
			this.imagesItem = new System.Windows.Forms.ImageList(this.components);
			this.DetailRichTextBox = new System.Windows.Forms.RichTextBox();
			this.LabelExtendedInfo = new System.Windows.Forms.Label();
			this.DetailMessage = new MessagesListBox(this.components);
			this.SuspendLayout();
			// 
			// imagesItem
			// 
			this.imagesItem.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imagesItem.ImageStream")));
			this.imagesItem.TransparentColor = System.Drawing.Color.Transparent;
			this.imagesItem.Images.SetKeyName(0, "");
			this.imagesItem.Images.SetKeyName(1, "");
			this.imagesItem.Images.SetKeyName(2, "");
			// 
			// DetailRichTextBox
			// 
			resources.ApplyResources(this.DetailRichTextBox, "DetailRichTextBox");
			this.DetailRichTextBox.Name = "DetailRichTextBox";
			this.DetailRichTextBox.ReadOnly = true;
			// 
			// LabelExtendedInfo
			// 
			resources.ApplyResources(this.LabelExtendedInfo, "LabelExtendedInfo");
			this.LabelExtendedInfo.Name = "LabelExtendedInfo";
			// 
			// DetailMessage
			// 
			this.DetailMessage.AllowDrop = true;
			resources.ApplyResources(this.DetailMessage, "DetailMessage");
			this.DetailMessage.BackColor = System.Drawing.Color.White;
			this.DetailMessage.Name = "DetailMessage";
			this.DetailMessage.SelectedIndex = -1;
			this.DetailMessage.SelectedItem = null;
			// 
			// MessageDetail
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.LabelExtendedInfo);
			this.Controls.Add(this.DetailRichTextBox);
			this.Controls.Add(this.DetailMessage);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MessageDetail";
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);

        }
        #endregion
    }
}
