namespace Microarea.TaskBuilderNet.UI.WinControls
{
    //=========================================================================
    partial class MessageAlert
    {

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageAlert));
            this.NoButton = new System.Windows.Forms.Button();
            this.YesButton = new System.Windows.Forms.Button();
            this.Okbtn = new System.Windows.Forms.Button();
            this.AlertPictureBox = new System.Windows.Forms.PictureBox();
            this.RetryButton = new System.Windows.Forms.Button();
            this.UndoButton = new System.Windows.Forms.Button();
            this.MessageLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.AlertPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // NoButton
            // 
            resources.ApplyResources(this.NoButton, "NoButton");
            this.NoButton.BackColor = System.Drawing.Color.Lavender;
            this.NoButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.NoButton.Name = "NoButton";
            this.NoButton.UseVisualStyleBackColor = false;
            // 
            // YesButton
            // 
            resources.ApplyResources(this.YesButton, "YesButton");
            this.YesButton.BackColor = System.Drawing.Color.Lavender;
            this.YesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.YesButton.Name = "YesButton";
            this.YesButton.UseVisualStyleBackColor = false;
            // 
            // Okbtn
            // 
            resources.ApplyResources(this.Okbtn, "Okbtn");
            this.Okbtn.BackColor = System.Drawing.Color.Lavender;
            this.Okbtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Okbtn.Name = "Okbtn";
            this.Okbtn.UseVisualStyleBackColor = false;
            // 
            // AlertPictureBox
            // 
            resources.ApplyResources(this.AlertPictureBox, "AlertPictureBox");
            this.AlertPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.AlertPictureBox.Name = "AlertPictureBox";
            this.AlertPictureBox.TabStop = false;
            // 
            // RetryButton
            // 
            resources.ApplyResources(this.RetryButton, "RetryButton");
            this.RetryButton.BackColor = System.Drawing.Color.Lavender;
            this.RetryButton.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this.RetryButton.Name = "RetryButton";
            this.RetryButton.UseVisualStyleBackColor = false;
            // 
            // UndoButton
            // 
            resources.ApplyResources(this.UndoButton, "UndoButton");
            this.UndoButton.BackColor = System.Drawing.Color.Lavender;
            this.UndoButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.UndoButton.Name = "UndoButton";
            this.UndoButton.UseVisualStyleBackColor = false;
            // 
            // MessageLabel
            // 
            resources.ApplyResources(this.MessageLabel, "MessageLabel");
            this.MessageLabel.BackColor = System.Drawing.Color.Transparent;
            this.MessageLabel.ForeColor = System.Drawing.Color.Navy;
            this.MessageLabel.Name = "MessageLabel";
            // 
            // MessageAlert
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(255)))));
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.Okbtn);
            this.Controls.Add(this.YesButton);
            this.Controls.Add(this.NoButton);
            this.Controls.Add(this.RetryButton);
            this.Controls.Add(this.AlertPictureBox);
            this.Controls.Add(this.MessageLabel);
            this.Controls.Add(this.UndoButton);
            this.ForeColor = System.Drawing.Color.Navy;
            this.Name = "MessageAlert";
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.AlertPictureBox)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.PictureBox AlertPictureBox;
        private System.Windows.Forms.Label MessageLabel;
        private System.Windows.Forms.Button NoButton;
        private System.Windows.Forms.Button YesButton;
        private System.Windows.Forms.Button Okbtn;
        private System.Windows.Forms.Button RetryButton;
        private System.Windows.Forms.Button UndoButton;


    }
}