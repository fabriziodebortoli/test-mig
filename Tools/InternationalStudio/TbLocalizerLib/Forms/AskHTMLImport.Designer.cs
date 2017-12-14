namespace Microarea.Tools.TBLocalizer.Forms
{
    partial class AskHTMLImport
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
            this.chkOverwiteExisting = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.chkSkipIfBaseDoNotMatch = new System.Windows.Forms.CheckBox();
            this.chkVerboseOutput = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkOverwiteExisting
            // 
            this.chkOverwiteExisting.AutoSize = true;
            this.chkOverwiteExisting.Location = new System.Drawing.Point(12, 12);
            this.chkOverwiteExisting.Name = "chkOverwiteExisting";
            this.chkOverwiteExisting.Size = new System.Drawing.Size(163, 17);
            this.chkOverwiteExisting.TabIndex = 0;
            this.chkOverwiteExisting.Text = "overwrite existing translations";
            this.chkOverwiteExisting.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(106, 88);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(12, 88);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // chkSkipIfBaseDoNotMatch
            // 
            this.chkSkipIfBaseDoNotMatch.AutoSize = true;
            this.chkSkipIfBaseDoNotMatch.Location = new System.Drawing.Point(12, 35);
            this.chkSkipIfBaseDoNotMatch.Name = "chkSkipIfBaseDoNotMatch";
            this.chkSkipIfBaseDoNotMatch.Size = new System.Drawing.Size(177, 17);
            this.chkSkipIfBaseDoNotMatch.TabIndex = 6;
            this.chkSkipIfBaseDoNotMatch.Text = "skip if base strings do not match";
            this.chkSkipIfBaseDoNotMatch.UseVisualStyleBackColor = true;
            // 
            // chkVerboseOutput
            // 
            this.chkVerboseOutput.AutoSize = true;
            this.chkVerboseOutput.Location = new System.Drawing.Point(12, 58);
            this.chkVerboseOutput.Name = "chkVerboseOutput";
            this.chkVerboseOutput.Size = new System.Drawing.Size(97, 17);
            this.chkVerboseOutput.TabIndex = 7;
            this.chkVerboseOutput.Text = "verbose output";
            this.chkVerboseOutput.UseVisualStyleBackColor = true;
            // 
            // AskHTMLImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(257, 125);
            this.Controls.Add(this.chkVerboseOutput);
            this.Controls.Add(this.chkSkipIfBaseDoNotMatch);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.chkOverwiteExisting);
            this.Name = "AskHTMLImport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import from HTML";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AskHTMLImport_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkOverwiteExisting;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.CheckBox chkSkipIfBaseDoNotMatch;
        private System.Windows.Forms.CheckBox chkVerboseOutput;
    }
}