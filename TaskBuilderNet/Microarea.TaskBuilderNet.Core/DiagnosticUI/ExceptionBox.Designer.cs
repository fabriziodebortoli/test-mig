
namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
    partial class ExceptionBox
    {
        private System.Windows.Forms.Label label;
        private System.Windows.Forms.TextBox exceptionTextBox;
        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.Button buttonIgnore;

        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
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
        void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionBox));
            this.exceptionTextBox = new System.Windows.Forms.TextBox();
            this.label = new System.Windows.Forms.Label();
            this.buttonAbort = new System.Windows.Forms.Button();
            this.buttonIgnore = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // exceptionTextBox
            // 
            resources.ApplyResources(this.exceptionTextBox, "exceptionTextBox");
            this.exceptionTextBox.Name = "exceptionTextBox";
            this.exceptionTextBox.ReadOnly = true;
            // 
            // label
            // 
            resources.ApplyResources(this.label, "label");
            this.label.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label.Name = "label";
            // 
            // buttonAbort
            // 
            resources.ApplyResources(this.buttonAbort, "buttonAbort");
            this.buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.buttonAbort.Name = "buttonAbort";
            // 
            // buttonIgnore
            // 
            resources.ApplyResources(this.buttonIgnore, "buttonIgnore");
            this.buttonIgnore.DialogResult = System.Windows.Forms.DialogResult.Ignore;
            this.buttonIgnore.Name = "buttonIgnore";
            // 
            // ExceptionBox
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.buttonIgnore);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.label);
            this.Controls.Add(this.exceptionTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExceptionBox";
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
