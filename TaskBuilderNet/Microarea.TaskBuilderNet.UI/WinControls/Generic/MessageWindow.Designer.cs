namespace Microarea.TaskBuilderNet.UI.WinControls.Generic
{
	partial class MessageWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageWindow));
            this.labelMessage = new System.Windows.Forms.Label();
            this.btn1 = new System.Windows.Forms.Button();
            this.chkDontAskAgain = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btn2 = new System.Windows.Forms.Button();
            this.btn3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelMessage
            // 
            resources.ApplyResources(this.labelMessage, "labelMessage");
            this.labelMessage.Name = "labelMessage";
            // 
            // btn1
            // 
            resources.ApplyResources(this.btn1, "btn1");
            this.btn1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn1.Name = "btn1";
            this.btn1.UseVisualStyleBackColor = true;
            // 
            // chkDontAskAgain
            // 
            resources.ApplyResources(this.chkDontAskAgain, "chkDontAskAgain");
            this.chkDontAskAgain.Name = "chkDontAskAgain";
            this.chkDontAskAgain.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Microarea.TaskBuilderNet.UI.Properties.Resources.Info;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // btn2
            // 
            resources.ApplyResources(this.btn2, "btn2");
            this.btn2.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn2.Name = "btn2";
            this.btn2.UseVisualStyleBackColor = true;
            // 
            // btn3
            // 
            resources.ApplyResources(this.btn3, "btn3");
            this.btn3.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn3.Name = "btn3";
            this.btn3.UseVisualStyleBackColor = true;
            // 
            // MessageWindow
            // 
            this.AcceptButton = this.btn1;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.chkDontAskAgain);
            this.Controls.Add(this.btn3);
            this.Controls.Add(this.btn2);
            this.Controls.Add(this.btn1);
            this.Controls.Add(this.labelMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MessageWindow";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelMessage;
		private System.Windows.Forms.Button btn1;
		private System.Windows.Forms.CheckBox chkDontAskAgain;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button btn2;
		private System.Windows.Forms.Button btn3;
	}
}
