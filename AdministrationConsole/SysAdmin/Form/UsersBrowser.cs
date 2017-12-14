
namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// Summary description for UsersBrowser.
	/// </summary>
	// ========================================================================
	public partial class UsersBrowser : System.Windows.Forms.Form
	{
		//---------------------------------------------------------------------
		public delegate void AfterUserSelected(object sender, string domainName, string usersToAdd);
		public event AfterUserSelected OnAfterUserSelected;

		public delegate void OpenHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event         OpenHelpFromPopUp				OnOpenHelpFromPopUp;


		//---------------------------------------------------------------------
		public UsersBrowser(string domainSelected, string computerName, bool multiSelection)
		{
			InitializeComponent();

			usersBrowserControl.DomainName = domainSelected;
			usersBrowserControl.MultiSelection = multiSelection;
			usersBrowserControl.ComputerName   = computerName;
		}
		
		//---------------------------------------------------------------------
		private void BtnOK_Click(object sender, System.EventArgs e)
		{
			this.Close();
			if (OnAfterUserSelected != null) OnAfterUserSelected(this, usersBrowserControl.DomainName, usersBrowserControl.UsersSelected);
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		//---------------------------------------------------------------------
		private void BtnHelp_Click(object sender, System.EventArgs e)
		{
			string nameSpace		= "Module.MicroareaConsole.SysAdmin";
			string searchParameter  = "Microarea.Console.Plugin.SysAdmin.Form.UsersBrowser";
			if (OnOpenHelpFromPopUp != null)
				OnOpenHelpFromPopUp(this, nameSpace, searchParameter);
		}
	}
}
