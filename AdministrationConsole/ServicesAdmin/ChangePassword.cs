using System;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.ServicesAdmin 
{

	public partial class ChangePassword : System.Windows.Forms.Form
	{
        private string oldPassword = string.Empty;

		public string OldPassword	 { get { return oldPassword;    } set { oldPassword    = value; }}
		
		public delegate void ChangePasswordEventHandler  (object sender, string newPassword);
		public event ChangePasswordEventHandler	OnChangePasswordEvent;

		public ChangePassword()
		{
		
			InitializeComponent();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new ChangePassword());
		}

		//---------------------------------------------------------------------
		private void btnOk_Click(object sender, System.EventArgs e)
		{
			if (String.Compare(this.oldPassword.Trim(), this.txtOldPassword.Text.Trim()) != 0)
			{	
				MessageBox.Show(Strings.OldPassword, Strings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				this.txtOldPassword.Focus();
				return;
			}

			if (String.Compare(this.txtNewPassword.Text.Trim(), this.txtConfirmPassword.Text.Trim()) != 0)
			{	
				MessageBox.Show(Strings.ConfirmPassword, Strings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				this.txtConfirmPassword.Focus();
				return;
			}
			
			this.DialogResult = DialogResult.OK;

			if( OnChangePasswordEvent != null)
				OnChangePasswordEvent(this, this.txtConfirmPassword.Text.Trim());

			this.Close();
	
		}
		//---------------------------------------------------------------------
		private void btnAndu_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		//---------------------------------------------------------------------
		
	}
}
