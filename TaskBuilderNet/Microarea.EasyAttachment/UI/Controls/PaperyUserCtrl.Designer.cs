namespace Microarea.EasyAttachment.UI.Controls
{
	partial class PaperyUserCtrl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaperyUserCtrl));
			this.LblNotes = new System.Windows.Forms.Label();
			this.TxtNotes = new System.Windows.Forms.TextBox();
			this.BarcodeDetailsCtrl = new Microarea.EasyAttachment.UI.Controls.BarcodeDetails();
			this.SuspendLayout();
			// 
			// LblNotes
			// 
			resources.ApplyResources(this.LblNotes, "LblNotes");
			this.LblNotes.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblNotes.Name = "LblNotes";
			// 
			// TxtNotes
			// 
			resources.ApplyResources(this.TxtNotes, "TxtNotes");
			this.TxtNotes.Name = "TxtNotes";
			// 
			// BarcodeDetailsCtrl
			// 
			resources.ApplyResources(this.BarcodeDetailsCtrl, "BarcodeDetailsCtrl");
			this.BarcodeDetailsCtrl.BackColor = System.Drawing.Color.Lavender;
			this.BarcodeDetailsCtrl.EnableStatus = Microarea.EasyAttachment.UI.Controls.BarcodeEnableStatus.AlwaysDisabled;
			this.BarcodeDetailsCtrl.Name = "BarcodeDetailsCtrl";
			// 
			// PaperyUserCtrl
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.LblNotes);
			this.Controls.Add(this.BarcodeDetailsCtrl);
			this.Controls.Add(this.TxtNotes);
			this.Name = "PaperyUserCtrl";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LblNotes;
		private BarcodeDetails BarcodeDetailsCtrl;
		private System.Windows.Forms.TextBox TxtNotes;
	}
}
