namespace Microarea.EasyAttachment.UI.Forms
{
    partial class ModifySelectedRows
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifySelectedRows));
            this.SplitContainerForm = new System.Windows.Forms.SplitContainer();
            this.LblDescription = new System.Windows.Forms.Label();
            this.LblFreeTags = new System.Windows.Forms.Label();
            this.LblYear = new System.Windows.Forms.Label();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnOk = new System.Windows.Forms.Button();
            this.TBDescription = new System.Windows.Forms.TextBox();
            this.TBYear = new System.Windows.Forms.TextBox();
            this.TBFreeTags = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainerForm)).BeginInit();
            this.SplitContainerForm.Panel1.SuspendLayout();
            this.SplitContainerForm.Panel2.SuspendLayout();
            this.SplitContainerForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // SplitContainerForm
            // 
            resources.ApplyResources(this.SplitContainerForm, "SplitContainerForm");
            this.SplitContainerForm.Name = "SplitContainerForm";
            // 
            // SplitContainerForm.Panel1
            // 
            this.SplitContainerForm.Panel1.BackColor = System.Drawing.Color.AliceBlue;
            this.SplitContainerForm.Panel1.Controls.Add(this.LblDescription);
            this.SplitContainerForm.Panel1.Controls.Add(this.LblFreeTags);
            this.SplitContainerForm.Panel1.Controls.Add(this.LblYear);
            resources.ApplyResources(this.SplitContainerForm.Panel1, "SplitContainerForm.Panel1");
            // 
            // SplitContainerForm.Panel2
            // 
            resources.ApplyResources(this.SplitContainerForm.Panel2, "SplitContainerForm.Panel2");
            this.SplitContainerForm.Panel2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.SplitContainerForm.Panel2.Controls.Add(this.BtnCancel);
            this.SplitContainerForm.Panel2.Controls.Add(this.BtnOk);
            this.SplitContainerForm.Panel2.Controls.Add(this.TBDescription);
            this.SplitContainerForm.Panel2.Controls.Add(this.TBYear);
            this.SplitContainerForm.Panel2.Controls.Add(this.TBFreeTags);
            // 
            // LblDescription
            // 
            resources.ApplyResources(this.LblDescription, "LblDescription");
            this.LblDescription.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.LblDescription.Name = "LblDescription";
            // 
            // LblFreeTags
            // 
            resources.ApplyResources(this.LblFreeTags, "LblFreeTags");
            this.LblFreeTags.Name = "LblFreeTags";
            // 
            // LblYear
            // 
            resources.ApplyResources(this.LblYear, "LblYear");
            this.LblYear.Name = "LblYear";
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // BtnOk
            // 
            this.BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.BtnOk, "BtnOk");
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.UseVisualStyleBackColor = true;
            this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // TBDescription
            // 
            this.TBDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TBDescription, "TBDescription");
            this.TBDescription.Name = "TBDescription";
            // 
            // TBYear
            // 
            this.TBYear.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TBYear, "TBYear");
            this.TBYear.Name = "TBYear";
            // 
            // TBFreeTags
            // 
            this.TBFreeTags.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TBFreeTags, "TBFreeTags");
            this.TBFreeTags.Name = "TBFreeTags";
            // 
            // ModifySelectedRows
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.AliceBlue;
            this.Controls.Add(this.SplitContainerForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ModifySelectedRows";
            this.SplitContainerForm.Panel1.ResumeLayout(false);
            this.SplitContainerForm.Panel1.PerformLayout();
            this.SplitContainerForm.Panel2.ResumeLayout(false);
            this.SplitContainerForm.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainerForm)).EndInit();
            this.SplitContainerForm.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LblDescription;
        private System.Windows.Forms.Label LblFreeTags;
        private System.Windows.Forms.Label LblYear;
        private System.Windows.Forms.TextBox TBDescription;
        private System.Windows.Forms.TextBox TBFreeTags;
        private System.Windows.Forms.TextBox TBYear;
		private System.Windows.Forms.SplitContainer SplitContainerForm;
		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.Button BtnOk;
    }
}