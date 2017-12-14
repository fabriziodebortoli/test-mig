namespace Microarea.EasyBuilder.UI
{
	/// <remarks/>
	partial class SaveCustomization
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveCustomization));
            this.txtCustomizationName = new System.Windows.Forms.TextBox();
            this.lblCustomization = new System.Windows.Forms.Label();
            this.btnYes = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.lblText = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.chkPublish = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtCustomizationName
            // 
            resources.ApplyResources(this.txtCustomizationName, "txtCustomizationName");
            this.txtCustomizationName.Name = "txtCustomizationName";
            this.txtCustomizationName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCustomizationName_KeyPress);
            // 
            // lblCustomization
            // 
            resources.ApplyResources(this.lblCustomization, "lblCustomization");
            this.lblCustomization.Name = "lblCustomization";
            // 
            // btnYes
            // 
            resources.ApplyResources(this.btnYes, "btnYes");
            this.btnYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnYes.Name = "btnYes";
            this.btnYes.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnNo
            // 
            resources.ApplyResources(this.btnNo, "btnNo");
            this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.btnNo.Name = "btnNo";
            this.btnNo.UseVisualStyleBackColor = true;
            // 
            // lblText
            // 
            resources.ApplyResources(this.lblText, "lblText");
            this.lblText.Name = "lblText";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Image = global::Microarea.EasyBuilder.Properties.Resources.Info;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // chkPublish
            // 
            resources.ApplyResources(this.chkPublish, "chkPublish");
            this.chkPublish.Name = "chkPublish";
            this.chkPublish.UseVisualStyleBackColor = true;
            // 
            // SaveCustomization
            // 
            this.AcceptButton = this.btnYes;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblText);
            this.Controls.Add(this.btnNo);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnYes);
            this.Controls.Add(this.lblCustomization);
            this.Controls.Add(this.txtCustomizationName);
            this.Controls.Add(this.chkPublish);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SaveCustomization";
            this.Load += new System.EventHandler(this.SaveWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtCustomizationName;
		private System.Windows.Forms.Label lblCustomization;
		private System.Windows.Forms.Button btnYes;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnNo;
		private System.Windows.Forms.Label lblText;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.CheckBox chkPublish;
	}
}
