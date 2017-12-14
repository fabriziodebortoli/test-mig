namespace Microarea.EasyAttachment.UI.Forms
{
	partial class InvalidBarcode
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InvalidBarcode));
			this.PBoxBarcode = new System.Windows.Forms.PictureBox();
			this.BtnOK = new System.Windows.Forms.Button();
			this.LblMessage1 = new System.Windows.Forms.Label();
			this.LblMessage2 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.PBoxBarcode)).BeginInit();
			this.SuspendLayout();
			// 
			// PBoxBarcode
			// 
			this.PBoxBarcode.Image = global::Microarea.EasyAttachment.Properties.Resources.BarcodeError64x64;
			resources.ApplyResources(this.PBoxBarcode, "PBoxBarcode");
			this.PBoxBarcode.Name = "PBoxBarcode";
			this.PBoxBarcode.TabStop = false;
			// 
			// BtnOK
			// 
			resources.ApplyResources(this.BtnOK, "BtnOK");
			this.BtnOK.Name = "BtnOK";
			this.BtnOK.UseVisualStyleBackColor = true;
			this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// LblMessage1
			// 
			this.LblMessage1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblMessage1, "LblMessage1");
			this.LblMessage1.Name = "LblMessage1";
			// 
			// LblMessage2
			// 
			this.LblMessage2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblMessage2, "LblMessage2");
			this.LblMessage2.Name = "LblMessage2";
			// 
			// InvalidBarcode
			// 
			this.AcceptButton = this.BtnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.LblMessage2);
			this.Controls.Add(this.LblMessage1);
			this.Controls.Add(this.BtnOK);
			this.Controls.Add(this.PBoxBarcode);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InvalidBarcode";
			this.ShowInTaskbar = false;
			((System.ComponentModel.ISupportInitialize)(this.PBoxBarcode)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox PBoxBarcode;
		private System.Windows.Forms.Button BtnOK;
		private System.Windows.Forms.Label LblMessage1;
		private System.Windows.Forms.Label LblMessage2;
	}
}