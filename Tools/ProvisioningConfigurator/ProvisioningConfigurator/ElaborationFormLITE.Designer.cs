namespace Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator
{
	partial class ElaborationFormLITE
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ElaborationForm));
			this.LblPresentation2 = new System.Windows.Forms.Label();
			this.ListOperations = new System.Windows.Forms.ListView();
			this.QSImageList = new System.Windows.Forms.ImageList(this.components);
			this.PictBoxProgress = new System.Windows.Forms.PictureBox();
			this.BtnClose = new System.Windows.Forms.Button();
			this.LblDetail = new System.Windows.Forms.Label();
			this.BtnShowDiagnostic = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.PictBoxProgress)).BeginInit();
			this.SuspendLayout();
			// 
			// LblPresentation2
			// 
			resources.ApplyResources(this.LblPresentation2, "LblPresentation2");
			this.LblPresentation2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblPresentation2.Name = "LblPresentation2";
			// 
			// ListOperations
			// 
			this.ListOperations.BackColor = System.Drawing.Color.White;
			this.ListOperations.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ListOperations.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.ListOperations.HideSelection = false;
			this.ListOperations.LargeImageList = this.QSImageList;
			resources.ApplyResources(this.ListOperations, "ListOperations");
			this.ListOperations.MultiSelect = false;
			this.ListOperations.Name = "ListOperations";
			this.ListOperations.Scrollable = false;
			this.ListOperations.ShowGroups = false;
			this.ListOperations.SmallImageList = this.QSImageList;
			this.ListOperations.UseCompatibleStateImageBehavior = false;
			this.ListOperations.View = System.Windows.Forms.View.Details;
			// 
			// QSImageList
			// 
			this.QSImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("QSImageList.ImageStream")));
			this.QSImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.QSImageList.Images.SetKeyName(0, "GreenResult.png");
			this.QSImageList.Images.SetKeyName(1, "RedResult.png");
			this.QSImageList.Images.SetKeyName(2, "Information.png");
			// 
			// PictBoxProgress
			// 
			resources.ApplyResources(this.PictBoxProgress, "PictBoxProgress");
			this.PictBoxProgress.Name = "PictBoxProgress";
			this.PictBoxProgress.TabStop = false;
			// 
			// BtnClose
			// 
			this.BtnClose.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.BtnClose, "BtnClose");
			this.BtnClose.Name = "BtnClose";
			this.BtnClose.UseVisualStyleBackColor = false;
			this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
			// 
			// LblDetail
			// 
			this.LblDetail.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblDetail, "LblDetail");
			this.LblDetail.Name = "LblDetail";
			// 
			// BtnShowDiagnostic
			// 
			this.BtnShowDiagnostic.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.BtnShowDiagnostic, "BtnShowDiagnostic");
			this.BtnShowDiagnostic.Name = "BtnShowDiagnostic";
			this.BtnShowDiagnostic.UseVisualStyleBackColor = false;
			this.BtnShowDiagnostic.Click += new System.EventHandler(this.BtnShowDiagnostic_Click);
			// 
			// ElaborationForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.BtnShowDiagnostic);
			this.Controls.Add(this.LblDetail);
			this.Controls.Add(this.BtnClose);
			this.Controls.Add(this.PictBoxProgress);
			this.Controls.Add(this.LblPresentation2);
			this.Controls.Add(this.ListOperations);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ElaborationForm";
			this.ShowInTaskbar = false;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ElaborationForm_FormClosing);
			this.Load += new System.EventHandler(this.ElaborationForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.PictBoxProgress)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LblPresentation2;
		private System.Windows.Forms.ListView ListOperations;
		private System.Windows.Forms.PictureBox PictBoxProgress;
		private System.Windows.Forms.Button BtnClose;
		private System.Windows.Forms.ImageList QSImageList;
		private System.Windows.Forms.Label LblDetail;
		private System.Windows.Forms.Button BtnShowDiagnostic;
	}
}
