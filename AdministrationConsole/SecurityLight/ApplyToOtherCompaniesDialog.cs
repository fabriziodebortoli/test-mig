using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.SecurityLayer.SecurityLightObjects;

namespace Microarea.Console.Plugin.SecurityLight
{
	/// <summary>
	/// Summary description for ApplyToOtherCompaniesDialog.
	/// </summary>
	public partial class ApplyToOtherCompaniesDialog : System.Windows.Forms.Form
	{
		
		#region ApplyToOtherCompaniesDialog private data members

		private MenuXmlNode		menuNode = null;
		private int				currentCompanyId = -1;
		private int				currentUserId = -1;
		private SqlConnection	systemDBConnection = null;	

		private string[] companyNamesToSkip = null;

		ArrayList initialCheckedCompaniesList = null;
		
		#endregion // ApplyToOtherCompaniesDialog private data members

		public ApplyToOtherCompaniesDialog(MenuXmlNode aMenuNode, int aCompanyId, int aUserId, SqlConnection aConnection, string[] aCompanyNamesToSkipList)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.ApplyToOtherCompanies.gif");
			if (imageStream != null)
				this.ApplyToOtherCompaniesPictureBox.Image = Image.FromStream(imageStream);

			currentCompanyId = aCompanyId;
			currentUserId = aUserId;

			if (aMenuNode == null || !(aMenuNode.IsGroup || aMenuNode.IsMenu || aMenuNode.IsCommand))
				throw new ArgumentException("Invalid menu node passed as argument to the ApplyToOtherCompaniesDialog constructor.");
			menuNode = aMenuNode;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
				throw new ArgumentException("Invalid connection passed as argument to the ApplyToOtherCompaniesDialog constructor.");
			systemDBConnection = aConnection;

			companyNamesToSkip = aCompanyNamesToSkipList;
		}

		#region ApplyToOtherUsersDialog protected overridden methods

		//--------------------------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
		
			FillCompaniesListView();
	
			if (this.AvailableCompaniesListView.Items.Count == 0)
			{
				MessageBox.Show
					(
					this, 
					Strings.NoCompanyAvailableInformationMsgText, 
					Strings.OperationCancelledCaption, 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Information
					);
				this.DialogResult = DialogResult.Cancel;
				this.Close();
				return;
			}
		}

		#endregion // ApplyToOtherUsersDialog protected overridden methods
		
		#region ApplyToOtherCompaniesDialog event handlers

		//--------------------------------------------------------------------------------------------------------
		private void AvailableCompaniesListView_SizeChanged(object sender, System.EventArgs e)
		{
			if (this.AvailableCompaniesListView.Columns.Count == 0)
				return;

			this.AvailableCompaniesListView.Columns[0].Width = this.AvailableCompaniesListView.ClientRectangle.Width;
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void ApplyButton_Click(object sender, System.EventArgs e)
		{
			if (initialCheckedCompaniesList != null)
			{
				if 
					(
					this.AvailableCompaniesListView.CheckedItems != null &&
					this.AvailableCompaniesListView.CheckedItems.Count == initialCheckedCompaniesList.Count
					)
				{
					bool sameList = true;
					foreach (ListViewItem aCheckedItem in this.AvailableCompaniesListView.CheckedItems)
					{
						if (!initialCheckedCompaniesList.Contains(aCheckedItem))
						{
							sameList = false;
							break;
						}
					}
					if (sameList)
					{
						this.DialogResult = DialogResult.Cancel;
						return;
					}
				}
			}
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void AvailableCompaniesListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			if (e == null || e.Index == -1 || initialCheckedCompaniesList == null)
				return;

			ListViewItem checkedItem = this.AvailableCompaniesListView.Items[e.Index];
			if (checkedItem != null && initialCheckedCompaniesList.Contains(checkedItem) && e.NewValue != CheckState.Checked)
				e.NewValue = CheckState.Checked;
		}
		
		#endregion // ApplyToOtherCompaniesDialog event handlers
		
		#region ApplyToOtherCompaniesDialog private methods

		//--------------------------------------------------------------------------------------------------------
		private void FillCompaniesListView()
		{
			this.AvailableCompaniesListView.Items.Clear();
			initialCheckedCompaniesList = null;

			if (systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
				return;

			SqlCommand selectCompaniesSqlCommand = null;
			SqlDataReader companiesReader = null;

			try
			{
				string queryText = String.Empty;
				if (currentUserId == -1)
					queryText = "SELECT MSD_Companies.Company, MSD_Companies.CompanyId FROM MSD_Companies ORDER BY MSD_Companies.Company";
				else
					queryText = @"SELECT MSD_Companies.Company, MSD_Companies.CompanyId FROM MSD_Companies INNER JOIN
								MSD_CompanyLogins ON MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyId WHERE LoginId = " + currentUserId.ToString() + " ORDER BY MSD_Companies.Company";
				
				selectCompaniesSqlCommand = new SqlCommand(queryText, systemDBConnection);

				companiesReader = selectCompaniesSqlCommand.ExecuteReader();

				while (companiesReader.Read())
				{
					if (currentCompanyId != -1 && currentCompanyId == (int)companiesReader["CompanyId"])
						continue;

					string companyName = companiesReader["Company"].ToString();
					if (companyName == null || companyName.Length == 0)
						continue;
		
					if (companyNamesToSkip != null && companyNamesToSkip.Length > 0)
					{
						bool skipCompany = false;
						foreach (string aCompanyNameToSkip in companyNamesToSkip)
						{
							if (aCompanyNameToSkip != null && aCompanyNameToSkip.Length > 0 && String.Compare(companyName, aCompanyNameToSkip) == 0)
							{
								skipCompany = true;
								break;
							}
						}
						if (skipCompany)
							continue;
					}

					ListViewItem companyListViewItem = new ListViewItem(companyName);
					companyListViewItem.Tag = (int)companiesReader["CompanyId"];
					this.AvailableCompaniesListView.Items.Add(companyListViewItem);
				}
				companiesReader.Close();
			
				if (this.AvailableCompaniesListView.Items.Count > 0)
				{
                    SecurityLightManager securityManager = new SecurityLightManager(null, systemDBConnection, null);
					foreach(ListViewItem aCompanyItem in this.AvailableCompaniesListView.Items)
					{
						if (aCompanyItem == null || aCompanyItem.Tag == null || !(aCompanyItem.Tag is int) || (int)aCompanyItem.Tag == -1)
							continue;

						UpdateListViewItem(aCompanyItem, securityManager);
					}
				
					initialCheckedCompaniesList = new ArrayList(this.AvailableCompaniesListView.CheckedItems);

					if (initialCheckedCompaniesList.Count == this.AvailableCompaniesListView.Items.Count)
					{
						MessageBox.Show
							(
							this, 
							Strings.NoCompanyAvailableInformationMsgText, 
							Strings.OperationCancelledCaption, 
							MessageBoxButtons.OK, 
							MessageBoxIcon.Information
							);
						this.DialogResult = DialogResult.Cancel;
						this.Close();
						return;
					}
				}
			}
			catch(SqlException exception)
			{
				Debug.Fail("SqlException raised in ApplyToOtherCompaniesDialog.FillCompaniesListView: " + exception.Message);
			}
			finally
			{
				if (companiesReader != null && !companiesReader.IsClosed)
					companiesReader.Close();

				if (selectCompaniesSqlCommand != null)
					selectCompaniesSqlCommand.Dispose();
			}
		}

		//--------------------------------------------------------------------------------------------------------
        private void UpdateListViewItem(ListViewItem aCompanyItem, SecurityLightManager aSecurityManager)
		{
			if 
				(
				menuNode == null || 
				(!menuNode.IsCommand && aSecurityManager == null) ||
				aCompanyItem == null || 
				aCompanyItem.Tag == null || 
				!(aCompanyItem.Tag is int) || 
				(int)aCompanyItem.Tag == -1
				)
				return;

			if (menuNode.IsGroup || menuNode.IsMenu)
			{
                SecurityLightManager.ChildrenProtectionType protectionType = aSecurityManager.GetChildrenProtectionType(menuNode, (int)aCompanyItem.Tag, currentUserId);

                if (protectionType == SecurityLightManager.ChildrenProtectionType.All)
				{
					aCompanyItem.ImageIndex = 0;
					aCompanyItem.Checked = menuNode.AccessDeniedState;
				}
                else if (protectionType == SecurityLightManager.ChildrenProtectionType.None)
				{
					aCompanyItem.ImageIndex = 1;
					aCompanyItem.Checked = menuNode.AccessAllowedState;
				}
				else
				{
					aCompanyItem.ImageIndex = 2;
					aCompanyItem.Checked = aSecurityManager.HasSameAccessRightsForBothCompanies(menuNode, currentUserId, (int)aCompanyItem.Tag, currentCompanyId);
				}
				if (aCompanyItem.Checked)
					aCompanyItem.ForeColor = SystemColors.GrayText;
			}
			else if (menuNode.IsCommand)
			{
				SecuredCommand aTmpSecuredCommand = new SecuredCommand(null, menuNode.ItemObject, SecuredCommand.GetSecuredCommandType(menuNode), systemDBConnection, null);
				if (aTmpSecuredCommand.IsAccessDenied((int)aCompanyItem.Tag, currentUserId))
				{
					aCompanyItem.ImageIndex = 0;
					aCompanyItem.Checked = menuNode.AccessDeniedState;
				}
				else
				{
					aCompanyItem.ImageIndex = 1;
					aCompanyItem.Checked = menuNode.AccessAllowedState;
				}
			}
		}

		#endregion // ApplyToOtherCompaniesDialog private methods

		#region ApplyToOtherCompaniesDialog public properties
		
		//--------------------------------------------------------------------------------------------------------
		public bool AreAllCompaniesChecked
		{
			get 
			{
				return 
					(
					this.AvailableCompaniesListView.Items != null &&
					this.AvailableCompaniesListView.Items.Count > 0 &&
					this.AvailableCompaniesListView.CheckedItems != null &&
					this.AvailableCompaniesListView.CheckedItems.Count == this.AvailableCompaniesListView.Items.Count
					);
			}
		}

		#endregion // ApplyToOtherCompaniesDialog public properties

		#region ApplyToOtherCompaniesDialog public methods
		
		//--------------------------------------------------------------------------------------------------------
		public int[] GetCheckedCompanyIds()
		{
			if (this.AvailableCompaniesListView.CheckedItems == null || this.AvailableCompaniesListView.CheckedItems.Count == 0)
				return null;

			if (AreAllCompaniesChecked)
				return new int[] { -1 };

			ArrayList checkedCompanyIds = new ArrayList();

			foreach (ListViewItem aCheckedItem in this.AvailableCompaniesListView.CheckedItems)
			{
				if (aCheckedItem.Tag == null || !(aCheckedItem.Tag is int))
					continue;

				checkedCompanyIds.Add((int)aCheckedItem.Tag);
			}

			return (checkedCompanyIds.Count > 0) ? (int[])checkedCompanyIds.ToArray(typeof(int)) : null;
		}
	
		#endregion // ApplyToOtherCompaniesDialog public methods

	}
}
