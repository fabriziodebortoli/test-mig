namespace Microarea.EasyAttachment.UI.Forms
{
	partial class PaperyForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaperyForm));
            this.BtnClose = new System.Windows.Forms.Button();
            this.LblIntro = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.LblBarcodeType = new System.Windows.Forms.Label();
            this.PaperyCtrl = new Microarea.EasyAttachment.UI.Controls.PaperyUserCtrl();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnClose
            // 
            this.BtnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.BtnClose, "BtnClose");
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.UseVisualStyleBackColor = true;
            // 
            // LblIntro
            // 
            resources.ApplyResources(this.LblIntro, "LblIntro");
            this.LblIntro.Name = "LblIntro";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.PaperyCtrl);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // LblBarcodeType
            // 
            resources.ApplyResources(this.LblBarcodeType, "LblBarcodeType");
            this.LblBarcodeType.Name = "LblBarcodeType";
            // 
            // PaperyCtrl
            // 
            this.PaperyCtrl.BackColor = System.Drawing.Color.Lavender;
            this.PaperyCtrl.BarcodeEnableStatus = Microarea.EasyAttachment.UI.Controls.BarcodeEnableStatus.AlwaysDisabled;
            resources.ApplyResources(this.PaperyCtrl, "PaperyCtrl");
            this.PaperyCtrl.Name = "PaperyCtrl";
            // 
            // PaperyForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.Controls.Add(this.LblBarcodeType);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.LblIntro);
            this.Controls.Add(this.BtnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PaperyForm";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Controls.PaperyUserCtrl PaperyCtrl;
		private System.Windows.Forms.Button BtnClose;
		private System.Windows.Forms.Label LblIntro;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Label LblBarcodeType;


	}
}