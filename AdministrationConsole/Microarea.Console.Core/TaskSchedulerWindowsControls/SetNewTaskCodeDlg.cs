using System;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for SetNewTaskCodeDlg.
	/// </summary>
	public partial class SetNewTaskCodeDlg : System.Windows.Forms.Form
	{
		private int		companyId				= -1; 
		private int		loginId					= -1;
		private string	currentConnectionString = String.Empty;

    	public SetNewTaskCodeDlg(int companyId, int loginId, string connectionString)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.companyId					= companyId;
			this.loginId					= loginId;
			this.currentConnectionString	= connectionString;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OkBtn_Click(object sender, System.EventArgs e)
		{
			if (CodeTextBox.Text == String.Empty)
			{
				MessageBox.Show(TaskSchedulerWindowsControlsStrings.EmptyCodeErrMsg);
			
				if (CodeTextBox.CanFocus)
					CodeTextBox.Focus();

				this.DialogResult = DialogResult.None;
	
				return;
			}

            if (!WTEScheduledTask.IsValidTaskCode(CodeTextBox.Text, companyId, loginId, currentConnectionString))
            {
                MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.CodeAlreadyUsedErrMsgFmt, CodeTextBox.Text, WTEScheduledTask.TaskCodeUniquePrefixLength));

                CodeTextBox.Text = String.Empty;
                if (CodeTextBox.CanFocus)
                    CodeTextBox.Focus();

                this.DialogResult = DialogResult.None;

                return;
            }
        }
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public string NewCode { get { return CodeTextBox.Text; } }
	}
}
