
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class MailNotificationsPageContent
    {
		private System.Windows.Forms.CheckBox SendMailUsingSMTPCheckBox;

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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MailNotificationsPageContent));
			this.SendMailUsingSMTPCheckBox = new System.Windows.Forms.CheckBox();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.Recipient = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.isToNotifyOnSuccessDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.isToNotifyOnFailureDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.taskNotificationRecipientBindingSource = new System.Windows.Forms.BindingSource(this.components);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.taskNotificationRecipientBindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SendMailUsingSMTPCheckBox
			// 
			resources.ApplyResources(this.SendMailUsingSMTPCheckBox, "SendMailUsingSMTPCheckBox");
			this.SendMailUsingSMTPCheckBox.Name = "SendMailUsingSMTPCheckBox";
			this.SendMailUsingSMTPCheckBox.CheckedChanged += new System.EventHandler(this.SendMailUsingSMTPCheckBox_CheckedChanged);
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToOrderColumns = true;
			resources.ApplyResources(this.dataGridView1, "dataGridView1");
			this.dataGridView1.AutoGenerateColumns = false;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Recipient,
            this.isToNotifyOnSuccessDataGridViewCheckBoxColumn,
            this.isToNotifyOnFailureDataGridViewCheckBoxColumn});
			this.dataGridView1.DataSource = this.taskNotificationRecipientBindingSource;
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridView1_DataError);
			// 
			// Recipient
			// 
			this.Recipient.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Recipient.DataPropertyName = "Recipient";
			resources.ApplyResources(this.Recipient, "Recipient");
			this.Recipient.Name = "Recipient";
			// 
			// isToNotifyOnSuccessDataGridViewCheckBoxColumn
			// 
			this.isToNotifyOnSuccessDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.isToNotifyOnSuccessDataGridViewCheckBoxColumn.DataPropertyName = "IsToNotifyOnSuccess";
			resources.ApplyResources(this.isToNotifyOnSuccessDataGridViewCheckBoxColumn, "isToNotifyOnSuccessDataGridViewCheckBoxColumn");
			this.isToNotifyOnSuccessDataGridViewCheckBoxColumn.Name = "isToNotifyOnSuccessDataGridViewCheckBoxColumn";
			// 
			// isToNotifyOnFailureDataGridViewCheckBoxColumn
			// 
			this.isToNotifyOnFailureDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.isToNotifyOnFailureDataGridViewCheckBoxColumn.DataPropertyName = "IsToNotifyOnFailure";
			resources.ApplyResources(this.isToNotifyOnFailureDataGridViewCheckBoxColumn, "isToNotifyOnFailureDataGridViewCheckBoxColumn");
			this.isToNotifyOnFailureDataGridViewCheckBoxColumn.Name = "isToNotifyOnFailureDataGridViewCheckBoxColumn";
			// 
			// taskNotificationRecipientBindingSource
			// 
			this.taskNotificationRecipientBindingSource.AllowNew = true;
			this.taskNotificationRecipientBindingSource.DataSource = typeof(Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects.WTETaskNotificationRecipient);
			// 
			// MailNotificationsPageContent
			// 
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.SendMailUsingSMTPCheckBox);
			resources.ApplyResources(this, "$this");
			this.Name = "MailNotificationsPageContent";
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.taskNotificationRecipientBindingSource)).EndInit();
			this.ResumeLayout(false);

        }
        #endregion

		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.BindingSource taskNotificationRecipientBindingSource;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.DataGridViewTextBoxColumn Recipient;
		private System.Windows.Forms.DataGridViewCheckBoxColumn isToNotifyOnSuccessDataGridViewCheckBoxColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn isToNotifyOnFailureDataGridViewCheckBoxColumn;

    }
}
