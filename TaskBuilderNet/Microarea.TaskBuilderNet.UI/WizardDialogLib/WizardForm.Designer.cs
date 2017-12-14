
namespace Microarea.TaskBuilderNet.UI.WizardDialogLib
{
    partial class WizardForm
    {
        public System.Windows.Forms.Button FinishWizardButton;
        public System.Windows.Forms.Button NextPageButton;
        public System.Windows.Forms.Button CancelWizardButton;
        public System.Windows.Forms.Button PageHelpButton;
        public System.Windows.Forms.GroupBox PageSeparator;
        public System.Windows.Forms.Button PreviousPageButton;

        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //--------------------------------------------------------------------
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardForm));
			this.PageSeparator = new System.Windows.Forms.GroupBox();
			this.FinishWizardButton = new System.Windows.Forms.Button();
			this.PreviousPageButton = new System.Windows.Forms.Button();
			this.NextPageButton = new System.Windows.Forms.Button();
			this.CancelWizardButton = new System.Windows.Forms.Button();
			this.PageHelpButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// PageSeparator
			// 
			resources.ApplyResources(this.PageSeparator, "PageSeparator");
			this.PageSeparator.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PageSeparator.Name = "PageSeparator";
			this.PageSeparator.TabStop = false;
			// 
			// FinishWizardButton
			// 
			resources.ApplyResources(this.FinishWizardButton, "FinishWizardButton");
			this.FinishWizardButton.Name = "FinishWizardButton";
			this.FinishWizardButton.Click += new System.EventHandler(this.OnClickFinish);
			// 
			// PreviousPageButton
			// 
			resources.ApplyResources(this.PreviousPageButton, "PreviousPageButton");
			this.PreviousPageButton.Name = "PreviousPageButton";
			this.PreviousPageButton.Click += new System.EventHandler(this.OnClickBack);
			// 
			// NextPageButton
			// 
			resources.ApplyResources(this.NextPageButton, "NextPageButton");
			this.NextPageButton.Name = "NextPageButton";
			this.NextPageButton.Click += new System.EventHandler(this.OnClickNext);
			// 
			// CancelWizardButton
			// 
			resources.ApplyResources(this.CancelWizardButton, "CancelWizardButton");
			this.CancelWizardButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelWizardButton.Name = "CancelWizardButton";
			this.CancelWizardButton.Click += new System.EventHandler(this.OnClickCancel);
			// 
			// PageHelpButton
			// 
			resources.ApplyResources(this.PageHelpButton, "PageHelpButton");
			this.PageHelpButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.PageHelpButton.Name = "PageHelpButton";
			this.PageHelpButton.Click += new System.EventHandler(this.OnClickHelp);
			// 
			// WizardForm
			// 
			this.AcceptButton = this.NextPageButton;
			resources.ApplyResources(this, "$this");
			this.CancelButton = this.CancelWizardButton;
			this.Controls.Add(this.PageHelpButton);
			this.Controls.Add(this.PreviousPageButton);
			this.Controls.Add(this.NextPageButton);
			this.Controls.Add(this.CancelWizardButton);
			this.Controls.Add(this.FinishWizardButton);
			this.Controls.Add(this.PageSeparator);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WizardForm";
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);

        }
        #endregion

    }
}
