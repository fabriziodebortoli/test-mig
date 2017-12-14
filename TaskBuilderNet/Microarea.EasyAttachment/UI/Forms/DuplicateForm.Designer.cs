using Microarea.EasyAttachment.UI.Controls;
namespace Microarea.EasyAttachment.UI.Forms
{
    partial class DuplicateForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DuplicateForm));
			this.LblTitle = new System.Windows.Forms.Label();
			this.LblSubTitle = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.LlblArrowArchive = new System.Windows.Forms.LinkLabel();
			this.LlblArchive = new System.Windows.Forms.LinkLabel();
			this.LlblArrowSobstitute = new System.Windows.Forms.LinkLabel();
			this.LlblSobstitute = new System.Windows.Forms.LinkLabel();
			this.LblArchive = new System.Windows.Forms.Label();
			this.LblSobstitute = new System.Windows.Forms.Label();
			this.LblExistingDoc = new System.Windows.Forms.Label();
			this.LblCheckDuplicateResult = new System.Windows.Forms.Label();
			this.LblNewDoc = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.LlblSkip = new System.Windows.Forms.LinkLabel();
			this.LlblArrowSkip = new System.Windows.Forms.LinkLabel();
			this.documentSpecificationNEW = new Microarea.EasyAttachment.UI.Controls.DocumentDetails();
			this.documentSpecificationOLD = new Microarea.EasyAttachment.UI.Controls.DocumentDetails();
			this.SuspendLayout();
			// 
			// LblTitle
			// 
			resources.ApplyResources(this.LblTitle, "LblTitle");
			this.LblTitle.ForeColor = System.Drawing.Color.DarkSlateBlue;
			this.LblTitle.Name = "LblTitle";
			// 
			// LblSubTitle
			// 
			resources.ApplyResources(this.LblSubTitle, "LblSubTitle");
			this.LblSubTitle.Name = "LblSubTitle";
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// LlblArrowArchive
			// 
			this.LlblArrowArchive.Image = global::Microarea.EasyAttachment.Properties.Resources.GreenArrow;
			resources.ApplyResources(this.LlblArrowArchive, "LlblArrowArchive");
			this.LlblArrowArchive.Name = "LlblArrowArchive";
			this.LlblArrowArchive.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LlblArrowArchive_LinkClicked);
			// 
			// LlblArchive
			// 
			this.LlblArchive.ActiveLinkColor = System.Drawing.Color.DarkSlateBlue;
			resources.ApplyResources(this.LlblArchive, "LlblArchive");
			this.LlblArchive.DisabledLinkColor = System.Drawing.Color.DarkSlateBlue;
			this.LlblArchive.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.LlblArchive.LinkColor = System.Drawing.Color.DarkSlateBlue;
			this.LlblArchive.Name = "LlblArchive";
			this.LlblArchive.TabStop = true;
			this.LlblArchive.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LlblArchive_LinkClicked);
			// 
			// LlblArrowSobstitute
			// 
			this.LlblArrowSobstitute.Image = global::Microarea.EasyAttachment.Properties.Resources.GreenArrow;
			resources.ApplyResources(this.LlblArrowSobstitute, "LlblArrowSobstitute");
			this.LlblArrowSobstitute.Name = "LlblArrowSobstitute";
			this.LlblArrowSobstitute.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LlblArrowSobstitute_LinkClicked);
			// 
			// LlblSobstitute
			// 
			this.LlblSobstitute.ActiveLinkColor = System.Drawing.Color.DarkSlateBlue;
			resources.ApplyResources(this.LlblSobstitute, "LlblSobstitute");
			this.LlblSobstitute.DisabledLinkColor = System.Drawing.Color.DarkSlateBlue;
			this.LlblSobstitute.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.LlblSobstitute.LinkColor = System.Drawing.Color.DarkSlateBlue;
			this.LlblSobstitute.Name = "LlblSobstitute";
			this.LlblSobstitute.TabStop = true;
			this.LlblSobstitute.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LlblSobstitute_LinkClicked);
			// 
			// LblArchive
			// 
			resources.ApplyResources(this.LblArchive, "LblArchive");
			this.LblArchive.Name = "LblArchive";
			// 
			// LblSobstitute
			// 
			resources.ApplyResources(this.LblSobstitute, "LblSobstitute");
			this.LblSobstitute.Name = "LblSobstitute";
			// 
			// LblExistingDoc
			// 
			resources.ApplyResources(this.LblExistingDoc, "LblExistingDoc");
			this.LblExistingDoc.Name = "LblExistingDoc";
			// 
			// LblCheckDuplicateResult
			// 
			resources.ApplyResources(this.LblCheckDuplicateResult, "LblCheckDuplicateResult");
			this.LblCheckDuplicateResult.Name = "LblCheckDuplicateResult";
			// 
			// LblNewDoc
			// 
			resources.ApplyResources(this.LblNewDoc, "LblNewDoc");
			this.LblNewDoc.Name = "LblNewDoc";
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// LlblSkip
			// 
			this.LlblSkip.ActiveLinkColor = System.Drawing.Color.DarkSlateBlue;
			resources.ApplyResources(this.LlblSkip, "LlblSkip");
			this.LlblSkip.DisabledLinkColor = System.Drawing.Color.DarkSlateBlue;
			this.LlblSkip.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.LlblSkip.LinkColor = System.Drawing.Color.DarkSlateBlue;
			this.LlblSkip.Name = "LlblSkip";
			this.LlblSkip.TabStop = true;
			this.LlblSkip.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LlblSkip_LinkClicked);
			// 
			// LlblArrowSkip
			// 
			this.LlblArrowSkip.Image = global::Microarea.EasyAttachment.Properties.Resources.GreenArrow;
			resources.ApplyResources(this.LlblArrowSkip, "LlblArrowSkip");
			this.LlblArrowSkip.Name = "LlblArrowSkip";
			this.LlblArrowSkip.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LlblArrowSkip_LinkClicked);
			// 
			// documentSpecificationNEW
			// 
			this.documentSpecificationNEW.BackColor = System.Drawing.Color.Lavender;
			resources.ApplyResources(this.documentSpecificationNEW, "documentSpecificationNEW");
			this.documentSpecificationNEW.Name = "documentSpecificationNEW";
			// 
			// documentSpecificationOLD
			// 
			this.documentSpecificationOLD.BackColor = System.Drawing.Color.Lavender;
			resources.ApplyResources(this.documentSpecificationOLD, "documentSpecificationOLD");
			this.documentSpecificationOLD.Name = "documentSpecificationOLD";
			// 
			// DuplicateForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.LlblSkip);
			this.Controls.Add(this.LlblArrowSkip);
			this.Controls.Add(this.LblSobstitute);
			this.Controls.Add(this.LblArchive);
			this.Controls.Add(this.documentSpecificationNEW);
			this.Controls.Add(this.documentSpecificationOLD);
			this.Controls.Add(this.LlblSobstitute);
			this.Controls.Add(this.LlblArchive);
			this.Controls.Add(this.LlblArrowSobstitute);
			this.Controls.Add(this.LlblArrowArchive);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.LblNewDoc);
			this.Controls.Add(this.LblCheckDuplicateResult);
			this.Controls.Add(this.LblExistingDoc);
			this.Controls.Add(this.LblSubTitle);
			this.Controls.Add(this.LblTitle);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DuplicateForm";
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LblTitle;
        private System.Windows.Forms.Label LblSubTitle;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.LinkLabel LlblArrowArchive;
        private System.Windows.Forms.LinkLabel LlblArchive;
        private System.Windows.Forms.LinkLabel LlblArrowSobstitute;
        private System.Windows.Forms.LinkLabel LlblSobstitute;
        private System.Windows.Forms.Label LblArchive;
        private System.Windows.Forms.Label LblSobstitute;
        private DocumentDetails documentSpecificationOLD;
        private DocumentDetails documentSpecificationNEW;
        private System.Windows.Forms.Label LblExistingDoc;
        private System.Windows.Forms.Label LblCheckDuplicateResult;
        private System.Windows.Forms.Label LblNewDoc;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel LlblSkip;
		private System.Windows.Forms.LinkLabel LlblArrowSkip;
    }
}