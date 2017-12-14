namespace Microarea.EasyAttachment.UI.Forms
{
	partial class MassiveAttachDetails
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MassiveAttachDetails));
            this.imageBtn = new System.Windows.Forms.Button();
            this.BtnOK = new System.Windows.Forms.Button();
            this.erpDocumentListView = new Microarea.EasyAttachment.UI.Controls.ExtendedListView();
            this.barcodeDetails = new Microarea.EasyAttachment.UI.Controls.BarcodeDetails();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // imageBtn
            // 
            resources.ApplyResources(this.imageBtn, "imageBtn");
            this.imageBtn.Image = global::Microarea.EasyAttachment.Properties.Resources.Airplane;
            this.imageBtn.Name = "imageBtn";
            this.imageBtn.UseVisualStyleBackColor = true;
            // 
            // BtnOK
            // 
            this.BtnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // erpDocumentListView
            // 
            resources.ApplyResources(this.erpDocumentListView, "erpDocumentListView");
            this.erpDocumentListView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.erpDocumentListView.MultiSelectEnabled = false;
            this.erpDocumentListView.Name = "erpDocumentListView";
            this.erpDocumentListView.UnselectEnabled = false;
            // 
            // barcodeDetails
            // 
            resources.ApplyResources(this.barcodeDetails, "barcodeDetails");
            this.barcodeDetails.BackColor = System.Drawing.Color.Lavender;
            this.barcodeDetails.EnableStatus = Microarea.EasyAttachment.UI.Controls.BarcodeEnableStatus.AlwaysEnabled;
            this.barcodeDetails.Name = "barcodeDetails";
            // 
            // label1
            // 
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // MassiveAttachDetails
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.CancelButton = this.BtnOK;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.imageBtn);
            this.Controls.Add(this.erpDocumentListView);
            this.Controls.Add(this.barcodeDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MassiveAttachDetails";
            this.Load += new System.EventHandler(this.MassiveAttachDetails_Load);
            this.ResumeLayout(false);

		}

		#endregion

		private Controls.BarcodeDetails barcodeDetails;
		private Controls.ExtendedListView erpDocumentListView;
		private System.Windows.Forms.Button imageBtn;
		private System.Windows.Forms.Button BtnOK;
		private System.Windows.Forms.Label label1;
	}
}