using System;
using System.DirectoryServices;
using System.Globalization;
using System.Security.Principal;

using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	//============================================================================================
	/// <summary>
	/// Summary description for AuthenticationPageContent.
	/// </summary>
	public partial class AuthenticationPageContent : System.Windows.Forms.UserControl
	{
		private WTEScheduledTaskObj task = null;
		private bool			impersonationPasswordChanged = false;

		//--------------------------------------------------------------------------------------------------------------------------------
		public AuthenticationPageContent()
		{
			InitializeComponent();

			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void FillDomainsComboBox()
		{
			ImpersonationDomainsComboBox.Items.Clear();

			string localComputerName = System.Net.Dns.GetHostName();
	
			// Use the domain and computer name to direct the DirectoryEntry component 
			// to examine your local computer using the WinNT service provider for Active
			// Directory.
			// Discover all domains on the network: the domains can be found by 
			// enumerating the children of the following DirectoryEntry. 
			string activeDirectoryPath = "WinNT://" + localComputerName + ",computer";
			System.DirectoryServices.DirectoryEntry entry = new System.DirectoryServices.DirectoryEntry(activeDirectoryPath);

			if (
				entry != null &&
				entry.Parent != null &&
				String.Compare(entry.Parent.Name, "workgroup", true, CultureInfo.InvariantCulture) != 0 &&
				IsDomainConnected(entry.Parent.Name)
				)
				ImpersonationDomainsComboBox.Items.Add(entry.Parent.Name);
		}
		
		//----------------------------------------------------------------
		private bool IsDomainConnected(string domainName)
		{
			if (domainName == null || domainName.Length == 0)
				return false;

			try
			{
				DirectoryEntry domainEntry = new DirectoryEntry("LDAP://" + domainName);
				
				// domainEntry is the node in the Active Directory hierarchy where the 
				// search starts.
				DirectorySearcher mySearcher = new	DirectorySearcher(domainEntry);

				mySearcher.FindOne();
				
				return true;
			}
			catch (Exception) 
			{
				return false;
			}
		}



		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnLoad(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnLoad(e);

			FillDomainsComboBox();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnValidated(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnValidated(e);

			if (task == null)
				return;

			task.IsToImpersonateWindowsUser = WindowsUserRadioButton.Checked;
			if (task.IsToImpersonateWindowsUser)
			{
				task.ImpersonationDomain = ImpersonationDomainsComboBox.Text;
				task.ImpersonationUser = ImpersonationUserTextBox.Text;
				if (impersonationPasswordChanged)
					task.ImpersonationPassword = ImpersonationPasswordTextBox.Text;
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void WindowsUserRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateAuthenticationPage();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void ASPNETUserRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateAuthenticationPage();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void ImpersonationPasswordTextBox_TextChanged(object sender, System.EventArgs e)
		{
			impersonationPasswordChanged = true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateAuthenticationPage()
		{
			bool useImpersonation = WindowsUserRadioButton.Checked;
			CurrentUserRadioButton.Checked = !useImpersonation;

			ImpersonationGroupBox.Enabled = useImpersonation;
	
			if (useImpersonation)
			{
				string currentSelectedDomain = ImpersonationDomainsComboBox.Text;
				if (currentSelectedDomain == null || currentSelectedDomain.Length == 0)
				{
					System.Security.Principal.WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
					// currentIdentity.Name contains the Windows logon name of the user on 
					// whose behalf the code is being run.
					// The logon name is in the form DOMAIN\USERNAME
					string[] logonTokens = currentIdentity.Name.Split('\\');
					if (logonTokens != null && logonTokens.Length == 2)
					{
						ImpersonationDomainsComboBox.Text = currentSelectedDomain = logonTokens[0];
						ImpersonationUserTextBox.Text = logonTokens[1];
					}
				}
				
				if (ImpersonationDomainsComboBox.Items.Count > 0)
				{
					int domainIdx = 0;
					if (currentSelectedDomain != null && currentSelectedDomain.Length > 0)
						domainIdx = ImpersonationDomainsComboBox.FindStringExact(currentSelectedDomain);
					ImpersonationDomainsComboBox.SelectedIndex = domainIdx;
				}

				ImpersonationDomainsComboBox.Focus();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public WTEScheduledTaskObj Task
		{
			get { return task; }
			set 
			{
				task = value;

				if (task != null)
				{
					WindowsUserRadioButton.Checked = task.IsToImpersonateWindowsUser;
					ImpersonationDomainsComboBox.Text = task.ImpersonationDomain;
					ImpersonationUserTextBox.Text = task.ImpersonationUser;
				}

				impersonationPasswordChanged = false;
			
				UpdateAuthenticationPage();
			}
		}
	}
}