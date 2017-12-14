namespace Microarea.Tools.TBLocalizer.Forms
{
    partial class AskHTMLExport
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
            this.chkSeparateFiles = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkBaseStringAsAttribute = new System.Windows.Forms.CheckBox();
            this.rbtOn3Columns = new System.Windows.Forms.RadioButton();
            this.rbtOn2Columns = new System.Windows.Forms.RadioButton();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbtTargetFillWithSupport = new System.Windows.Forms.RadioButton();
            this.rbtTargetLeaveBlank = new System.Windows.Forms.RadioButton();
            this.chkSkipUntranslatable = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkSeparateFiles
            // 
            this.chkSeparateFiles.AutoSize = true;
            this.chkSeparateFiles.Location = new System.Drawing.Point(16, 19);
            this.chkSeparateFiles.Name = "chkSeparateFiles";
            this.chkSeparateFiles.Size = new System.Drawing.Size(203, 17);
            this.chkSeparateFiles.TabIndex = 0;
            this.chkSeparateFiles.Text = "create separate files per each module";
            this.chkSeparateFiles.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkBaseStringAsAttribute);
            this.groupBox1.Controls.Add(this.rbtOn3Columns);
            this.groupBox1.Controls.Add(this.rbtOn2Columns);
            this.groupBox1.Location = new System.Drawing.Point(14, 47);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(280, 96);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Output";
            // 
            // chkBaseStringAsAttribute
            // 
            this.chkBaseStringAsAttribute.AutoSize = true;
            this.chkBaseStringAsAttribute.Location = new System.Drawing.Point(7, 68);
            this.chkBaseStringAsAttribute.Name = "chkBaseStringAsAttribute";
            this.chkBaseStringAsAttribute.Size = new System.Drawing.Size(180, 17);
            this.chkBaseStringAsAttribute.TabIndex = 2;
            this.chkBaseStringAsAttribute.Text = "output base string as an attribute";
            this.chkBaseStringAsAttribute.UseVisualStyleBackColor = true;
            // 
            // rbtOn3Columns
            // 
            this.rbtOn3Columns.AutoSize = true;
            this.rbtOn3Columns.Location = new System.Drawing.Point(6, 42);
            this.rbtOn3Columns.Name = "rbtOn3Columns";
            this.rbtOn3Columns.Size = new System.Drawing.Size(185, 17);
            this.rbtOn3Columns.TabIndex = 1;
            this.rbtOn3Columns.Text = "3 columns (base - support - target)";
            this.rbtOn3Columns.UseVisualStyleBackColor = true;
            // 
            // rbtOn2Columns
            // 
            this.rbtOn2Columns.AutoSize = true;
            this.rbtOn2Columns.Checked = true;
            this.rbtOn2Columns.Location = new System.Drawing.Point(6, 19);
            this.rbtOn2Columns.Name = "rbtOn2Columns";
            this.rbtOn2Columns.Size = new System.Drawing.Size(141, 17);
            this.rbtOn2Columns.TabIndex = 0;
            this.rbtOn2Columns.TabStop = true;
            this.rbtOn2Columns.Text = "2 columns (base - target)";
            this.rbtOn2Columns.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(12, 266);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(106, 266);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbtTargetFillWithSupport);
            this.groupBox2.Controls.Add(this.rbtTargetLeaveBlank);
            this.groupBox2.Location = new System.Drawing.Point(16, 149);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(278, 71);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Target column";
            // 
            // rbtTargetFillWithSupport
            // 
            this.rbtTargetFillWithSupport.AutoSize = true;
            this.rbtTargetFillWithSupport.Location = new System.Drawing.Point(6, 42);
            this.rbtTargetFillWithSupport.Name = "rbtTargetFillWithSupport";
            this.rbtTargetFillWithSupport.Size = new System.Drawing.Size(94, 17);
            this.rbtTargetFillWithSupport.TabIndex = 1;
            this.rbtTargetFillWithSupport.Text = "fill with support";
            this.rbtTargetFillWithSupport.UseVisualStyleBackColor = true;
            // 
            // rbtTargetLeaveBlank
            // 
            this.rbtTargetLeaveBlank.AutoSize = true;
            this.rbtTargetLeaveBlank.Checked = true;
            this.rbtTargetLeaveBlank.Location = new System.Drawing.Point(6, 19);
            this.rbtTargetLeaveBlank.Name = "rbtTargetLeaveBlank";
            this.rbtTargetLeaveBlank.Size = new System.Drawing.Size(255, 17);
            this.rbtTargetLeaveBlank.TabIndex = 0;
            this.rbtTargetLeaveBlank.TabStop = true;
            this.rbtTargetLeaveBlank.Text = "use translation if available, leave blank otherwise";
            this.rbtTargetLeaveBlank.UseVisualStyleBackColor = true;
            // 
            // chkSkipUntranslatable
            // 
            this.chkSkipUntranslatable.Location = new System.Drawing.Point(19, 222);
            this.chkSkipUntranslatable.Name = "chkSkipUntranslatable";
            this.chkSkipUntranslatable.Size = new System.Drawing.Size(275, 38);
            this.chkSkipUntranslatable.TabIndex = 5;
            this.chkSkipUntranslatable.Text = "skip items that do not change between base and support (untranslatable)";
            this.chkSkipUntranslatable.UseVisualStyleBackColor = true;
            // 
            // AskHTMLExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 305);
            this.Controls.Add(this.chkSkipUntranslatable);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.chkSeparateFiles);
            this.Name = "AskHTMLExport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export to HTML";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AskHTMLExport_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkSeparateFiles;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbtOn3Columns;
        private System.Windows.Forms.RadioButton rbtOn2Columns;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbtTargetFillWithSupport;
        private System.Windows.Forms.RadioButton rbtTargetLeaveBlank;
        private System.Windows.Forms.CheckBox chkSkipUntranslatable;
        private System.Windows.Forms.CheckBox chkBaseStringAsAttribute;
    }
}