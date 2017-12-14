namespace Microarea.EasyAttachment.UI.Forms
{
	partial class MassiveAttachResultDetails
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MassiveAttachResultDetails));
            this.BtnOK = new System.Windows.Forms.Button();
            this.detailLabel = new System.Windows.Forms.Label();
            this.showErrorBtn = new System.Windows.Forms.Button();
            this.resultListView = new Microarea.EasyAttachment.UI.Controls.ExtendedListView();
            this.errorLabel = new System.Windows.Forms.Label();
            this.resultPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.resultPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // detailLabel
            // 
            this.detailLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.detailLabel, "detailLabel");
            this.detailLabel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.detailLabel.Name = "detailLabel";
            // 
            // showErrorBtn
            // 
            resources.ApplyResources(this.showErrorBtn, "showErrorBtn");
            this.showErrorBtn.Image = global::Microarea.EasyAttachment.Properties.Resources.Help;
            this.showErrorBtn.Name = "showErrorBtn";
            this.showErrorBtn.UseVisualStyleBackColor = true;
            this.showErrorBtn.Click += new System.EventHandler(this.showErrorBtn_Click);
            // 
            // resultListView
            // 
            resources.ApplyResources(this.resultListView, "resultListView");
            this.resultListView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.resultListView.ForeColor = System.Drawing.SystemColors.ControlText;
            this.resultListView.MultiSelectEnabled = false;
            this.resultListView.Name = "resultListView";
            this.resultListView.UnselectEnabled = false;
            // 
            // errorLabel
            // 
            this.errorLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.errorLabel, "errorLabel");
            this.errorLabel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.errorLabel.Name = "errorLabel";
            // 
            // resultPictureBox
            // 
            resources.ApplyResources(this.resultPictureBox, "resultPictureBox");
            this.resultPictureBox.Image = global::Microarea.EasyAttachment.Properties.Resources.OK;
            this.resultPictureBox.Name = "resultPictureBox";
            this.resultPictureBox.TabStop = false;
            // 
            // MassiveAttachResultDetails
            // 
            this.AcceptButton = this.BtnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.Lavender;
            this.CancelButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.resultPictureBox);
            this.Controls.Add(this.errorLabel);
            this.Controls.Add(this.showErrorBtn);
            this.Controls.Add(this.detailLabel);
            this.Controls.Add(this.resultListView);
            this.Controls.Add(this.BtnOK);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MassiveAttachResultDetails";
            this.Load += new System.EventHandler(this.MassiveAttachResultDetails_Load);
            ((System.ComponentModel.ISupportInitialize)(this.resultPictureBox)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button BtnOK;
		private System.Windows.Forms.Label detailLabel;
		private Controls.ExtendedListView resultListView;
		private System.Windows.Forms.Button showErrorBtn;
		private System.Windows.Forms.Label errorLabel;
		private System.Windows.Forms.PictureBox resultPictureBox;
	}
}