namespace Microarea.TaskBuilderNet.UI.WinControls
{
	//=========================================================================
	partial class SensitiveTextBox
	{
        private System.ComponentModel.IContainer components = null;

        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SensitiveTextBox));
			this.ImagePictureBox = new System.Windows.Forms.PictureBox();
			this.TextBoxPanel = new System.Windows.Forms.Panel();
			this.TextBox = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.ImagePictureBox)).BeginInit();
			this.TextBoxPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ImagePictureBox
			// 
			this.ImagePictureBox.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.ImagePictureBox, "ImagePictureBox");
			this.ImagePictureBox.Name = "ImagePictureBox";
			this.ImagePictureBox.TabStop = false;
			this.ImagePictureBox.Click += new System.EventHandler(this.ImagePictureBox_Click);
			// 
			// TextBoxPanel
			// 
			resources.ApplyResources(this.TextBoxPanel, "TextBoxPanel");
			this.TextBoxPanel.Controls.Add(this.TextBox);
			this.TextBoxPanel.Name = "TextBoxPanel";
			// 
			// TextBox
			// 
			this.TextBox.BackColor = System.Drawing.Color.White;
			this.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.TextBox.ForeColor = System.Drawing.Color.Navy;
			resources.ApplyResources(this.TextBox, "TextBox");
			this.TextBox.Name = "TextBox";
			this.TextBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
			this.TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyDown);
			// 
			// SensitiveTextBox
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			resources.ApplyResources(this, "$this");
			this.BackColor = System.Drawing.Color.White;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.TextBoxPanel);
			this.Controls.Add(this.ImagePictureBox);
			this.ForeColor = System.Drawing.Color.Navy;
			this.Name = "SensitiveTextBox";
			((System.ComponentModel.ISupportInitialize)(this.ImagePictureBox)).EndInit();
			this.TextBoxPanel.ResumeLayout(false);
			this.TextBoxPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        private System.Windows.Forms.PictureBox ImagePictureBox;
        private System.Windows.Forms.Panel TextBoxPanel;
        private System.Windows.Forms.TextBox TextBox;
    }
}
