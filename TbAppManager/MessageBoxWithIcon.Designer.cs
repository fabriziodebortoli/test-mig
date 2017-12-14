namespace Microarea.MenuManager
{
    partial class MessageBoxWithIcon
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageBoxWithIcon));
            this.PbOK = new System.Windows.Forms.PictureBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.LblInfo = new System.Windows.Forms.Label();
            this.PbError = new System.Windows.Forms.PictureBox();
            this.PbWarning = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PbOK)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbError)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbWarning)).BeginInit();
            this.SuspendLayout();
            // 
            // PbOK
            // 
            this.PbOK.Image = ((System.Drawing.Image)(resources.GetObject("PbOK.Image")));
            this.PbOK.Location = new System.Drawing.Point(16, 11);
            this.PbOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PbOK.Name = "PbOK";
            this.PbOK.Size = new System.Drawing.Size(55, 50);
            this.PbOK.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PbOK.TabIndex = 0;
            this.PbOK.TabStop = false;
            this.PbOK.Visible = false;
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(247, 80);
            this.BtnOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(100, 25);
            this.BtnOK.TabIndex = 1;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // LblInfo
            // 
            this.LblInfo.Location = new System.Drawing.Point(79, 12);
            this.LblInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LblInfo.Name = "LblInfo";
            this.LblInfo.Size = new System.Drawing.Size(270, 54);
            this.LblInfo.TabIndex = 2;
            this.LblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PbError
            // 
            this.PbError.Image = ((System.Drawing.Image)(resources.GetObject("PbError.Image")));
            this.PbError.Location = new System.Drawing.Point(16, 9);
            this.PbError.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PbError.Name = "PbError";
            this.PbError.Size = new System.Drawing.Size(55, 50);
            this.PbError.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PbError.TabIndex = 0;
            this.PbError.TabStop = false;
            this.PbError.Visible = false;
            // 
            // PbWarning
            // 
            this.PbWarning.Image = ((System.Drawing.Image)(resources.GetObject("PbWarning.Image")));
            this.PbWarning.Location = new System.Drawing.Point(16, 11);
            this.PbWarning.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PbWarning.Name = "PbWarning";
            this.PbWarning.Size = new System.Drawing.Size(55, 50);
            this.PbWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PbWarning.TabIndex = 0;
            this.PbWarning.TabStop = false;
            this.PbWarning.Visible = false;
            // 
            // MessageBoxWithIcon
            // 
            this.AcceptButton = this.BtnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 110);
            this.Controls.Add(this.LblInfo);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.PbError);
            this.Controls.Add(this.PbWarning);
            this.Controls.Add(this.PbOK);
            this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageBoxWithIcon";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this.PbOK)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbError)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbWarning)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox PbOK;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Label LblInfo;
        private System.Windows.Forms.PictureBox PbError;
        private System.Windows.Forms.PictureBox PbWarning;
    }
}