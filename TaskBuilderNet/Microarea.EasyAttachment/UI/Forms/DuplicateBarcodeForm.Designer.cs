namespace Microarea.EasyAttachment.UI.Forms
{
	partial class DuplicateBarcodeForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DuplicateBarcodeForm));
			this.LblDescriAttach = new System.Windows.Forms.Label();
			this.LblAttach = new System.Windows.Forms.LinkLabel();
			this.BtnNo = new System.Windows.Forms.Button();
			this.PBoxAttachment = new System.Windows.Forms.PictureBox();
			this.BtnYes = new System.Windows.Forms.Button();
			this.BtnPreview = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.PBoxAttachment)).BeginInit();
			this.SuspendLayout();
			// 
			// LblDescriAttach
			// 
			resources.ApplyResources(this.LblDescriAttach, "LblDescriAttach");
			this.LblDescriAttach.Name = "LblDescriAttach";
			// 
			// LblAttach
			// 
			this.LblAttach.ActiveLinkColor = System.Drawing.Color.DarkSlateBlue;
			this.LblAttach.DisabledLinkColor = System.Drawing.Color.DarkSlateBlue;
			resources.ApplyResources(this.LblAttach, "LblAttach");
			this.LblAttach.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.LblAttach.LinkColor = System.Drawing.Color.DarkSlateBlue;
			this.LblAttach.Name = "LblAttach";
			this.LblAttach.TabStop = true;
			this.LblAttach.UseCompatibleTextRendering = true;
			// 
			// BtnNo
			// 
			resources.ApplyResources(this.BtnNo, "BtnNo");
			this.BtnNo.Name = "BtnNo";
			this.BtnNo.UseVisualStyleBackColor = true;
			this.BtnNo.Click += new System.EventHandler(this.BtnNo_Click);
			// 
			// PBoxAttachment
			// 
			this.PBoxAttachment.Image = global::Microarea.EasyAttachment.Properties.Resources.DocAttach48x48;
			resources.ApplyResources(this.PBoxAttachment, "PBoxAttachment");
			this.PBoxAttachment.Name = "PBoxAttachment";
			this.PBoxAttachment.TabStop = false;
			// 
			// BtnYes
			// 
			resources.ApplyResources(this.BtnYes, "BtnYes");
			this.BtnYes.Name = "BtnYes";
			this.BtnYes.UseVisualStyleBackColor = true;
			this.BtnYes.Click += new System.EventHandler(this.BtnYes_Click);
			// 
			// BtnPreview
			// 
			this.BtnPreview.Image = global::Microarea.EasyAttachment.Properties.Resources.Preview16x16;
			resources.ApplyResources(this.BtnPreview, "BtnPreview");
			this.BtnPreview.Name = "BtnPreview";
			this.BtnPreview.UseVisualStyleBackColor = true;
			this.BtnPreview.Click += new System.EventHandler(this.BtnPreview_Click);
			// 
			// DuplicateBarcodeForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.BtnPreview);
			this.Controls.Add(this.BtnYes);
			this.Controls.Add(this.PBoxAttachment);
			this.Controls.Add(this.BtnNo);
			this.Controls.Add(this.LblAttach);
			this.Controls.Add(this.LblDescriAttach);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DuplicateBarcodeForm";
			this.ShowInTaskbar = false;
			((System.ComponentModel.ISupportInitialize)(this.PBoxAttachment)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label LblDescriAttach;
		private System.Windows.Forms.LinkLabel LblAttach;
		private System.Windows.Forms.Button BtnNo;
		private System.Windows.Forms.PictureBox PBoxAttachment;
		private System.Windows.Forms.Button BtnYes;
		private System.Windows.Forms.Button BtnPreview;
	}
}