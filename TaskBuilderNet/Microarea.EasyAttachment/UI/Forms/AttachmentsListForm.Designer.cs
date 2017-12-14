namespace Microarea.EasyAttachment.UI.Forms
{
	partial class AttachmentsListForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AttachmentsListForm));
			this.docSlideShow = new Microarea.EasyAttachment.UI.Controls.DocSlideShow();
			this.AttachToMailBtn = new System.Windows.Forms.Button();
			this.CancelBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// docSlideShow
			// 
			this.docSlideShow.CurrentDoc = null;
			resources.ApplyResources(this.docSlideShow, "docSlideShow");
			this.docSlideShow.Name = "docSlideShow";
			this.docSlideShow.Title = "";
			this.docSlideShow.UserSelectionType = Microarea.EasyAttachment.UI.Controls.DocSlideShow.SelectionType.Single;
			// 
			// AttachToMailBtn
			// 
			this.AttachToMailBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.AttachToMailBtn, "AttachToMailBtn");
			this.AttachToMailBtn.Name = "AttachToMailBtn";
			this.AttachToMailBtn.UseVisualStyleBackColor = true;
			// 
			// CancelBtn
			// 
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.CancelBtn, "CancelBtn");
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.UseVisualStyleBackColor = true;
			// 
			// AttachmentsListForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.CancelButton = this.CancelBtn;
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.AttachToMailBtn);
			this.Controls.Add(this.docSlideShow);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "AttachmentsListForm";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.AttachmentsListForm_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private Controls.DocSlideShow docSlideShow;
		private System.Windows.Forms.Button AttachToMailBtn;
		private System.Windows.Forms.Button CancelBtn;
	}
}