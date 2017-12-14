using System;
using System.Collections.Generic;
using System.Text;

namespace Microarea.Library.TranslationManager
{
    partial class Repetitions
    {
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox ENTBlankSpaces;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox ENTValuesTooLong;
        private System.Windows.Forms.Button CMDOk;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Repetitions));
            this.CMDOk = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.ENTBlankSpaces = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ENTValuesTooLong = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // CMDOk
            // 
            resources.ApplyResources(this.CMDOk, "CMDOk");
            this.CMDOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CMDOk.Name = "CMDOk";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // ENTBlankSpaces
            // 
            resources.ApplyResources(this.ENTBlankSpaces, "ENTBlankSpaces");
            this.ENTBlankSpaces.Name = "ENTBlankSpaces";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // ENTValuesTooLong
            // 
            resources.ApplyResources(this.ENTValuesTooLong, "ENTValuesTooLong");
            this.ENTValuesTooLong.Name = "ENTValuesTooLong";
            // 
            // Repetitions
            // 
            this.AcceptButton = this.CMDOk;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.CMDOk;
            this.ControlBox = false;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ENTValuesTooLong);
            this.Controls.Add(this.ENTBlankSpaces);
            this.Controls.Add(this.CMDOk);
            this.Name = "Repetitions";
            this.Load += new System.EventHandler(this.Repetitions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
