
namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
    partial class DiagnosticBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TabControl tabMessages;
        private System.Windows.Forms.TabPage ErrorsPage;
        private System.Windows.Forms.Button BtnOk;
        private System.Windows.Forms.TextBox errorsTextBox;
        private System.Windows.Forms.TabPage WarningsPage;
        private System.Windows.Forms.RichTextBox warningsTextBox;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        
        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiagnosticBox));
			this.lblTitle = new System.Windows.Forms.Label();
			this.BtnOk = new System.Windows.Forms.Button();
			this.tabMessages = new System.Windows.Forms.TabControl();
			this.ErrorsPage = new System.Windows.Forms.TabPage();
			this.errorsTextBox = new System.Windows.Forms.TextBox();
			this.WarningsPage = new System.Windows.Forms.TabPage();
			this.warningsTextBox = new System.Windows.Forms.RichTextBox();
			this.tabMessages.SuspendLayout();
			this.ErrorsPage.SuspendLayout();
			this.WarningsPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblTitle
			// 
			resources.ApplyResources(this.lblTitle, "lblTitle");
			this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblTitle.Name = "lblTitle";
			// 
			// BtnOk
			// 
			this.BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.BtnOk, "BtnOk");
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// tabMessages
			// 
			this.tabMessages.Controls.Add(this.ErrorsPage);
			this.tabMessages.Controls.Add(this.WarningsPage);
			resources.ApplyResources(this.tabMessages, "tabMessages");
			this.tabMessages.Multiline = true;
			this.tabMessages.Name = "tabMessages";
			this.tabMessages.SelectedIndex = 0;
			// 
			// ErrorsPage
			// 
			this.ErrorsPage.Controls.Add(this.errorsTextBox);
			resources.ApplyResources(this.ErrorsPage, "ErrorsPage");
			this.ErrorsPage.Name = "ErrorsPage";
			// 
			// errorsTextBox
			// 
			resources.ApplyResources(this.errorsTextBox, "errorsTextBox");
			this.errorsTextBox.Name = "errorsTextBox";
			this.errorsTextBox.ReadOnly = true;
			// 
			// WarningsPage
			// 
			this.WarningsPage.Controls.Add(this.warningsTextBox);
			resources.ApplyResources(this.WarningsPage, "WarningsPage");
			this.WarningsPage.Name = "WarningsPage";
			// 
			// warningsTextBox
			// 
			resources.ApplyResources(this.warningsTextBox, "warningsTextBox");
			this.warningsTextBox.Name = "warningsTextBox";
			// 
			// DiagnosticBox
			// 
			this.AcceptButton = this.BtnOk;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.tabMessages);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.lblTitle);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DiagnosticBox";
			this.ShowInTaskbar = false;
			this.TopMost = true;
			this.Load += new System.EventHandler(this.DiagnosticBox_Load);
			this.tabMessages.ResumeLayout(false);
			this.ErrorsPage.ResumeLayout(false);
			this.ErrorsPage.PerformLayout();
			this.WarningsPage.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        #endregion

    }
}
