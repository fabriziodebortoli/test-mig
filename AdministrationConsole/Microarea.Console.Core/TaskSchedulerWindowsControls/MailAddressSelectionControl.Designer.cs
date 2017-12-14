
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class MailAddressSelectionControl
    {
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MailAddressSelectionControl));
            this.AddressBookButton = new MailAddressSelectionControl.MailAddressesSelectionButton();
            this.AddressTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // AddressBookButton
            // 
            resources.ApplyResources(this.AddressBookButton, "AddressBookButton");
            this.AddressBookButton.Name = "AddressBookButton";
            this.AddressBookButton.RecipientsList = "";
            this.AddressBookButton.BeforeRecipientsListChange += new System.EventHandler(this.AddressBookButton_BeforeRecipientsListChange);
            this.AddressBookButton.RecipientsListChanged += new MailAddressSelectionControl.MailAddressesSelectionButton.RecipientsListChangedEventHandler(this.AddressBookButton_RecipientsListChanged);
            // 
            // AddressTextBox
            // 
            resources.ApplyResources(this.AddressTextBox, "AddressTextBox");
            this.AddressTextBox.Name = "AddressTextBox";
            this.AddressTextBox.Validated += new System.EventHandler(this.AddressTextBox_Validated);
            this.AddressTextBox.TextChanged += new System.EventHandler(this.AddressTextBox_TextChanged);
            // 
            // MailAddressSelectionControl
            // 
            this.Controls.Add(this.AddressTextBox);
            this.Controls.Add(this.AddressBookButton);
            resources.ApplyResources(this, "$this");
            this.Name = "MailAddressSelectionControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.RichTextBox AddressTextBox;
        private MailAddressesSelectionButton AddressBookButton;
    }
}
