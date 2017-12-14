using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.UI.WinControls.Lists
{
	/// <summary>
	/// Summary description for UsersBrowserControl.
	/// User control per elencare, selezionando il dominio o il nome PC
	/// gli utenti e i gruppi NT
	/// </summary>
	// ========================================================================
	public partial class UsersBrowserControl : System.Windows.Forms.UserControl
	{

		//---------------------------------------------------------------------
		private bool		multiSelection	= false;
		private string		domainName		= string.Empty;
		private string		computerName	= string.Empty;
		private string		usersSelected		= string.Empty;
		
		//---------------------------------------------------------------------
		public string UsersSelected 	 { get { return usersSelected;      }}

		//---------------------------------------------------------------------
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string DomainName { get { return domainName.ToUpper(CultureInfo.InvariantCulture); } set { domainName = value.ToUpper(CultureInfo.InvariantCulture); }}
		[DefaultValue(""), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public string ComputerName { get { return computerName.ToUpper(CultureInfo.InvariantCulture); } set { computerName = value.ToUpper(CultureInfo.InvariantCulture); }}
		[DefaultValue(false), System.ComponentModel.RefreshProperties(RefreshProperties.Repaint)]
		public bool MultiSelection 	 { get { return multiSelection;      } set { multiSelection	   = value; }}

		//---------------------------------------------------------------------
		public UsersBrowserControl()
		{
			InitializeComponent();
			BuildListView();
		}

		//---------------------------------------------------------------------
		private void BuildListView()
		{
			listUsers.View		= View.Details;
			listUsers.Sorting	= SortOrder.Ascending;
			listUsers.Columns.Add(WinControlsStrings.User, 150, HorizontalAlignment.Left);
			listUsers.Columns.Add(WinControlsStrings.Class, -2, HorizontalAlignment.Left);
			listUsers.MultiSelect = MultiSelection;
			
		}

    	//---------------------------------------------------------------------
		private void cbListDomains_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (((ComboBox)sender).SelectedItem != null)
			{
				string selectedDomain = ((ComboBox)sender).SelectedItem.ToString();
				LoadAllNTUser(selectedDomain);
				DomainName = selectedDomain;
				
			}
		}

		//---------------------------------------------------------------------
		private void LoadAllNTUser(string domain)
		{
			
			listUsers.Items.Clear();
			String sADsPath = ConstString.providerNT + domain;
			try
			{
				if (DirectoryEntry.Exists(sADsPath))
				{
					DirectoryEntry entry = new DirectoryEntry(sADsPath);
					foreach(DirectoryEntry child in entry.Children) 
					{
						if ( ( string.Compare(child.SchemaClassName, "User",  true, CultureInfo.InvariantCulture) == 0) ) 
						{
							ListViewItem item = new ListViewItem();
							item.Text		  = child.Name;
							item.SubItems.Add(child.Properties["FullName"].Value.ToString());
							listUsers.Items.Add(item);
						}
							
					}

				}
			}
			catch(SecurityException)
			{
				btnAdd.Enabled			= false;
				tbSelectedUser.Enabled	= false;
				
				listUsers.Enabled		= false;

			}
			catch(Exception )
			{
				btnAdd.Enabled			= false;
				tbSelectedUser.Enabled	= false;
				
				listUsers.Enabled		= false;
			}
			if (listUsers.Items.Count == 0)
			{
				btnAdd.Enabled			= false;
				tbSelectedUser.Enabled	= false;
				listUsers.Enabled		= false;
			}
			else
			{
				btnAdd.Enabled			= true;
				tbSelectedUser.Enabled	= true;
				listUsers.Enabled		= true;
				listUsers.Focus();
			}
		}

		//---------------------------------------------------------------------
		private void listUsers_DoubleClick(object sender, System.EventArgs e)
		{
			int selectedItems		= listUsers.SelectedItems.Count;
			string selectedDomain	= cbListDomains.SelectedItem.ToString().ToUpper(CultureInfo.InvariantCulture);
			
			if (!MultiSelection) 
			{
				tbSelectedUser.Text = selectedDomain + Path.DirectorySeparatorChar + listUsers.SelectedItems[0].Text.ToUpper(CultureInfo.InvariantCulture);
			}

			else
			{
				if (selectedItems > 0)
				{
					for (int i=0; i < listUsers.SelectedItems.Count; i++)
					{
						string userOrGroupToAdd = selectedDomain + Path.DirectorySeparatorChar + listUsers.SelectedItems[i].Text.ToUpper(CultureInfo.InvariantCulture);
						if (tbSelectedUser.Text.Length == 0)
							tbSelectedUser.Text = userOrGroupToAdd;
						else
						{
							if (!AlreadyAddedIntoTextBox(userOrGroupToAdd))
								tbSelectedUser.Text += ";" + userOrGroupToAdd;
						}
					}
				}
			}
			usersSelected = tbSelectedUser.Text;
		}

		//---------------------------------------------------------------------
		private bool AlreadyAddedIntoTextBox(string userOrGroup)
		{
			bool isPresent = false;
			//se mi arriva vuoto non lo faccio inserire
			if (userOrGroup.Length == 0) return true;
			//gli aggiungo il terminatore
			userOrGroup = userOrGroup + ";";
			if (tbSelectedUser.Text.IndexOf(userOrGroup) >= 0)
				isPresent = true;
			return isPresent;
		}

		//---------------------------------------------------------------------
		private void btnAdd_Click(object sender, System.EventArgs e)
		{
			int selectedItems		= listUsers.SelectedItems.Count;
			string selectedDomain	= cbListDomains.SelectedItem.ToString().ToUpper(CultureInfo.InvariantCulture);
			if (!MultiSelection)
			{
				string userOrGroupToAdd = selectedDomain + Path.DirectorySeparatorChar + listUsers.SelectedItems[0].Text.ToUpper(CultureInfo.InvariantCulture);
				tbSelectedUser.Text = userOrGroupToAdd;
			}
			else
			{
				if (selectedItems > 0)
				{
					for (int i=0; i < listUsers.SelectedItems.Count; i++)
					{
						string userOrGroupToAdd = selectedDomain + Path.DirectorySeparatorChar + listUsers.SelectedItems[i].Text.ToUpper(CultureInfo.InvariantCulture);
						if (tbSelectedUser.Text.Length == 0)
							tbSelectedUser.Text = userOrGroupToAdd;
						else
						{
							if (!AlreadyAddedIntoTextBox(userOrGroupToAdd))
								tbSelectedUser.Text += ";" + userOrGroupToAdd ;
						}
					}
					
				}
			}
			usersSelected = tbSelectedUser.Text;
		}

		

		//---------------------------------------------------------------------
		public void Inizialize()
		{
			cbListDomains.Items.Clear();
			listUsers.Items.Clear();
			ComputerName = (ComputerName.Length > 0) ? ComputerName : SystemInformation.ComputerName;
			//inserisco il nome del computer come dominio
			cbListDomains.Items.Add(ComputerName);
			//se il pc ha un dominio diverso da quello specificato lo inserisco nella combo
			if (string.Compare(ComputerName, SystemInformation.UserDomainName, true, CultureInfo.InvariantCulture) != 0)
				cbListDomains.Items.Add(SystemInformation.UserDomainName);
			//seleziono il domain name
			int pos = cbListDomains.FindStringExact(DomainName);
			if (pos >= 0)
				cbListDomains.SelectedIndex = pos;
			cbListDomains.Focus();
		}

		//---------------------------------------------------------------------
		
		private void UsersBrowserControl_Load(object sender, System.EventArgs e)
		{
			Inizialize();
		}
		

	}
}
