using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for MailNotificationsPageContent.
	/// </summary>
	public partial class MailNotificationsPageContent : System.Windows.Forms.UserControl
	{
		private WTEScheduledTaskObj task = null;

    	public MailNotificationsPageContent()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public WTEScheduledTaskObj Task
		{
			get { return task; }
			set 
			{
				task = value;

				SendMailUsingSMTPCheckBox.Checked = (task != null && task.SendMailUsingSMTP);
			
				taskNotificationRecipientBindingSource.Clear();
				if (task != null && task.NotificationRecipients != null)
				{
					foreach(WTETaskNotificationRecipient recipientItem in task.NotificationRecipients)
						taskNotificationRecipientBindingSource.Add(new WTETaskNotificationRecipient(recipientItem));
				}
			}
		}
	
		//--------------------------------------------------------------------------------------------------------------------------------
		private void SendMailUsingSMTPCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if (task != null)
				task.SendMailUsingSMTP = SendMailUsingSMTPCheckBox.Checked;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
				FindForm().FormClosing += new FormClosingEventHandler(MailNotificationsPageContent_FormClosing);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void MailNotificationsPageContent_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (FindForm().DialogResult == DialogResult.OK)
			{
				foreach (WTETaskNotificationRecipient recipientItem in taskNotificationRecipientBindingSource)
					if (!recipientItem.IsValid())
					{
						e.Cancel = true;
						return;
					}
				
				if (task == null)
					return;

				task.ClearNotificationRecipientsList();
				foreach (WTETaskNotificationRecipient recipientItem in taskNotificationRecipientBindingSource)
					task.AddNotificationRecipient(recipientItem);
			}
		}
	
		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnValidated(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnValidated(e);

			
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			MessageBox.Show(this, e.Exception.Message);
		}

		
	}
}
