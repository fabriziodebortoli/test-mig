namespace Microarea.TaskBuilderNet.UI.WinControls
{
    partial class TabScrollCloseCtrl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TabScrollCloseCtrl));
            this.LeftScrollCloseButton = new System.Windows.Forms.Button();
            this.RightScrollCloseButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LeftScrollCloseButton
            // 
            this.LeftScrollCloseButton.BackColor = System.Drawing.Color.Lavender;
            this.LeftScrollCloseButton.FlatAppearance.BorderColor = System.Drawing.Color.RoyalBlue;
            this.LeftScrollCloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.LeftScrollCloseButton.Font = new System.Drawing.Font("Verdana", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LeftScrollCloseButton.ForeColor = System.Drawing.Color.Navy;
            this.LeftScrollCloseButton.Image = ((System.Drawing.Image)(resources.GetObject("LeftScrollCloseButton.Image")));
            this.LeftScrollCloseButton.Location = new System.Drawing.Point(0, 0);
            this.LeftScrollCloseButton.Name = "LeftScrollCloseButton";
            this.LeftScrollCloseButton.Size = new System.Drawing.Size(30, 30);
            this.LeftScrollCloseButton.TabIndex = 0;
            this.LeftScrollCloseButton.UseVisualStyleBackColor = false;
            this.LeftScrollCloseButton.Click += new System.EventHandler(this.LeftScrollCloseCtrl_Click);
            // 
            // RightScrollCloseButton
            // 
            this.RightScrollCloseButton.BackColor = System.Drawing.Color.Lavender;
            this.RightScrollCloseButton.FlatAppearance.BorderColor = System.Drawing.Color.RoyalBlue;
            this.RightScrollCloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RightScrollCloseButton.Font = new System.Drawing.Font("Verdana", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RightScrollCloseButton.ForeColor = System.Drawing.Color.Navy;
            this.RightScrollCloseButton.Image = ((System.Drawing.Image)(resources.GetObject("RightScrollCloseButton.Image")));
            this.RightScrollCloseButton.Location = new System.Drawing.Point(30, 0);
            this.RightScrollCloseButton.Name = "RightScrollCloseButton";
            this.RightScrollCloseButton.Size = new System.Drawing.Size(30, 30);
            this.RightScrollCloseButton.TabIndex = 1;
            this.RightScrollCloseButton.UseVisualStyleBackColor = false;
            this.RightScrollCloseButton.Click += new System.EventHandler(this.RightScrollCloseCtrl_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.BackColor = System.Drawing.Color.Lavender;
            this.CloseButton.FlatAppearance.BorderColor = System.Drawing.Color.RoyalBlue;
            this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseButton.Font = new System.Drawing.Font("Verdana", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CloseButton.ForeColor = System.Drawing.Color.Navy;
            this.CloseButton.Image = ((System.Drawing.Image)(resources.GetObject("CloseButton.Image")));
            this.CloseButton.Location = new System.Drawing.Point(60, 0);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(30, 30);
            this.CloseButton.TabIndex = 2;
            this.CloseButton.UseVisualStyleBackColor = false;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // TabScrollCloseCtrl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.LeftScrollCloseButton);
            this.Controls.Add(this.RightScrollCloseButton);
            this.Controls.Add(this.CloseButton);
            this.Font = new System.Drawing.Font("Marlett", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.ForeColor = System.Drawing.Color.Navy;
            this.Name = "TabScrollCloseCtrl";
            this.Size = new System.Drawing.Size(90, 30);
            this.ResumeLayout(false);

        }


        #endregion

        internal System.Windows.Forms.Button LeftScrollCloseButton;
        internal System.Windows.Forms.Button RightScrollCloseButton;
        internal System.Windows.Forms.Button CloseButton;
    }
}
