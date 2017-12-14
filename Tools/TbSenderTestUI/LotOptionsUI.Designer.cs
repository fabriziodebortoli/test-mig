namespace TbSenderTestUI
{
	partial class LotOptionsUI
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
			this.cmbDeliveryType = new System.Windows.Forms.ComboBox();
			this.lblDeliveryType = new System.Windows.Forms.Label();
			this.lblPrintingType = new System.Windows.Forms.Label();
			this.cmbPrintingType = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// cmbDeliveryType
			// 
			this.cmbDeliveryType.FormattingEnabled = true;
			this.cmbDeliveryType.Location = new System.Drawing.Point(18, 42);
			this.cmbDeliveryType.Name = "cmbDeliveryType";
			this.cmbDeliveryType.Size = new System.Drawing.Size(121, 21);
			this.cmbDeliveryType.TabIndex = 0;
			// 
			// lblDeliveryType
			// 
			this.lblDeliveryType.AutoSize = true;
			this.lblDeliveryType.Location = new System.Drawing.Point(15, 26);
			this.lblDeliveryType.Name = "lblDeliveryType";
			this.lblDeliveryType.Size = new System.Drawing.Size(72, 13);
			this.lblDeliveryType.TabIndex = 1;
			this.lblDeliveryType.Text = "Delivery Type";
			// 
			// lblPrintingType
			// 
			this.lblPrintingType.AutoSize = true;
			this.lblPrintingType.Location = new System.Drawing.Point(15, 79);
			this.lblPrintingType.Name = "lblPrintingType";
			this.lblPrintingType.Size = new System.Drawing.Size(69, 13);
			this.lblPrintingType.TabIndex = 3;
			this.lblPrintingType.Text = "Printing Type";
			// 
			// cmbPrintingType
			// 
			this.cmbPrintingType.FormattingEnabled = true;
			this.cmbPrintingType.Location = new System.Drawing.Point(18, 95);
			this.cmbPrintingType.Name = "cmbPrintingType";
			this.cmbPrintingType.Size = new System.Drawing.Size(121, 21);
			this.cmbPrintingType.TabIndex = 2;
			// 
			// LotOptionsUI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblPrintingType);
			this.Controls.Add(this.cmbPrintingType);
			this.Controls.Add(this.lblDeliveryType);
			this.Controls.Add(this.cmbDeliveryType);
			this.Name = "LotOptionsUI";
			this.Size = new System.Drawing.Size(244, 279);
			this.Load += new System.EventHandler(this.LotOptionsUI_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox cmbDeliveryType;
		private System.Windows.Forms.Label lblDeliveryType;
		private System.Windows.Forms.Label lblPrintingType;
		private System.Windows.Forms.ComboBox cmbPrintingType;
	}
}
