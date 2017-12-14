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
	/// Summary description for ApplyToOtherUsersDialog.
	/// </summary>
	public partial class ApplyToOtherUsersDialog : System.Windows.Forms.Form
	{
		#region ApplyToOtherUsersDialog private data members

		private MenuXmlNode		menuNode = null;
		private int				currentCompanyId = -1;
		private int				currentUserId = -1;
		private SqlConnection	systemDBConnection = null;	

		private string[] userNamesToSkip = null;

		ArrayList initialCheckedUsersList = null;

		#endregion // ApplyToOtherUsersDialog private data members

		public ApplyToOtherUsersDialog(MenuXmlNode aMenuNode, int aCompanyId, int aUserId, SqlConnection aConnection, string[] aUserNamesToSkipList)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.ApplyToOtherUsers.gif");
			if (imageStream != null)
				this.ApplyToOtherUsersPictureBox.Image = Image.FromStream(imageStream);

			currentCompanyId = aCompanyId;
			currentUserId = aUserId;

			if (aMenuNode == null || !(aMenuNode.IsGroup || aMenuNode.IsMenu || aMenuNode.IsCommand))
				throw new ArgumentException("Invalid menu node passed as argument to the ApplyToOtherUsersDialog constructor.");
			menuNode = aMenuNode;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
				throw new ArgumentException("Invalid connection passed as argument to the ApplyToOtherUsersDialog constructor.");
			systemDBConnection = aConnection;
		
			userNamesToSkip = aUserNamesToSkipList;
		}

		#region ApplyToOtherUsersDialog protected overridden methods

		//--------------------------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
		
			FillUsersListView();
		
			if (this.AvailableUsersListView.Items.Count == 0)
			{
				MessageBox.Show
					(
					this, 
					Strings.NoUserAvailableInformationMsgText, 
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
		
		#region ApplyToOtherUsersDialog event handlers

		//--------------------------------------------------------------------------------------------------------
		private void AvailableUsersListView_SizeChanged(object sender, System.EventArgs e)
		{
			if (this.AvailableUsersListView.Columns.Count == 0)
				return;

			this.AvailableUsersListView.Columns[0].Width = this.AvailableUsersListView.ClientRectangle.Width;
		}

		//--------------------------------------------------------------------------------------------------------
		private void ApplyButton_Click(object sender, System.EventArgs e)
		{
			if (initialCheckedUsersList != null)
			{
				if 
					(
					this.AvailableUsersListView.CheckedItems != null &&
					this.AvailableUsersListView.CheckedItems.Count == initialCheckedUsersList.Count
					)
				{
					bool sameList = true;
					foreach (ListViewItem aCheckedItem in this.AvailableUsersListView.CheckedItems)
					{
						if (!initialCheckedUsersList.Contains(aCheckedItem))
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
		private void AvailableUsersListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			if (e == null || e.Index == -1 || initialCheckedUsersList == null)
				return;

			ListViewItem checkedItem = this.AvailableUsersListView.Items[e.Index];
			if (checkedItem != null && initialCheckedUsersList.Contains(checkedItem) && e.NewValue != CheckState.Checked)
				e.NewValue = CheckState.Checked;
		}
		
		#endregion // ApplyToOtherUsersDialog event handlers

		#region ApplyToOtherUsersDialog private methods

		//--------------------------------------------------------------------------------------------------------
		private void FillUsersListView()
		{
			this.AvailableUsersListView.Items.Clear();
			initialCheckedUsersList = null;

			if (systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
				return;

			SqlCommand selectUsersSqlCommand = null;
			SqlDataReader usersReader = null;

			try
			{
				string queryText = String.Empty;
				if (currentCompanyId == -1)
					queryText = "SELECT MSD_Logins.Login, MSD_Logins.LoginId FROM MSD_Logins ORDER BY MSD_Logins.Login";
				else
					queryText = @"SELECT MSD_Logins.Login, MSD_Logins.LoginId FROM MSD_Logins INNER JOIN
								MSD_CompanyLogins ON MSD_Logins.LoginId = MSD_CompanyLogins.LoginId WHERE MSD_CompanyLogins.CompanyId = " + currentCompanyId.ToString() + " ORDER BY MSD_Logins.Login";

				selectUsersSqlCommand = new SqlCommand(queryText, systemDBConnection);

				usersReader = selectUsersSqlCommand.ExecuteReader();

				while (usersReader.Read())
				{
					if (currentUserId != -1 && currentUserId == (int)usersReader["LoginId"])
						continue;

					string userName = usersReader["Login"].ToString();
					if (userName == null || userName.Length == 0)
						continue;
					
					if (userNamesToSkip != null && userNamesToSkip.Length > 0)
					{
						bool skipUser = false;
						foreach (string aUserNameToSkip in userNamesToSkip)
						{
							if (aUserNameToSkip != null && aUserNameToSkip.Length > 0 && String.Compare(userName, aUserNameToSkip) == 0)
							{
								skipUser = true;
								break;
							}
						}
						if (skipUser)
							continue;
					}

					ListViewItem userListViewItem = new ListViewItem(userName);
					userListViewItem.Tag = (int)usersReader["LoginId"];
					this.AvailableUsersListView.Items.Add(userListViewItem);
				}
				usersReader.Close();

				if (this.AvailableUsersListView.Items.Count > 0)
				{
                    SecurityLightManager securityManager = new SecurityLightManager(null, systemDBConnection, userNamesToSkip);
					foreach(ListViewItem aUserItem in this.AvailableUsersListView.Items)
					{
						if (aUserItem == null || aUserItem.Tag == null || !(aUserItem.Tag is int) || (int)aUserItem.Tag == -1)
							continue;

						UpdateListViewItem(aUserItem, securityManager);
					}

					initialCheckedUsersList = new ArrayList(this.AvailableUsersListView.CheckedItems);

					if (initialCheckedUsersList.Count == this.AvailableUsersListView.Items.Count)
					{
						MessageBox.Show
							(
							this, 
							Strings.NoUserAvailableInformationMsgText, 
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
				Debug.Fail("SqlException raised in ApplyToOtherUsersDialog.FillUsersListView: " + exception.Message);
			}
			finally
			{
				if (usersReader != null && !usersReader.IsClosed)
					usersReader.Close();

				if (selectUsersSqlCommand != null)
					selectUsersSqlCommand.Dispose();
			}
		}

		//--------------------------------------------------------------------------------------------------------
		private void UpdateListViewItem(ListViewItem aUserItem, SecurityLightManager aSecurityManager)
		{
			if 
				(
				menuNode == null || 
				(!menuNode.IsCommand && aSecurityManager == null) ||
				aUserItem == null || 
				aUserItem.Tag == null || 
				!(aUserItem.Tag is int) || 
				(int)aUserItem.Tag == -1
				)
				return;

			if (menuNode.IsGroup || menuNode.IsMenu)
			{
                SecurityLightManager.ChildrenProtectionType protectionType = aSecurityManager.GetChildrenProtectionType(menuNode, currentCompanyId, (int)aUserItem.Tag);

                if (protectionType == SecurityLightManager.ChildrenProtectionType.All)
				{
					aUserItem.ImageIndex = 0;
					aUserItem.Checked = menuNode.AccessDeniedState;
				}
                else if (protectionType == SecurityLightManager.ChildrenProtectionType.None)
				{
					aUserItem.ImageIndex = 1;
					aUserItem.Checked = menuNode.AccessAllowedState;
				}
				else
				{
					aUserItem.ImageIndex = 2;
					aUserItem.Checked = aSecurityManager.HasSameAccessRightsForBothUsers(menuNode, currentCompanyId, (int)aUserItem.Tag, currentUserId);
				}
				if (aUserItem.Checked)
					aUserItem.ForeColor = SystemColors.GrayText;
			}
			else if (menuNode.IsCommand)
			{
				SecuredCommand aTmpSecuredCommand = new SecuredCommand(null, menuNode.ItemObject, SecuredCommand.GetSecuredCommandType(menuNode), systemDBConnection, userNamesToSkip);
				if (aTmpSecuredCommand.IsAccessDenied(currentCompanyId, (int)aUserItem.Tag))
				{
					aUserItem.ImageIndex = 0;
					aUserItem.Checked = menuNode.AccessDeniedState;
				}
				else
				{
					aUserItem.ImageIndex = 1;
					aUserItem.Checked = menuNode.AccessAllowedState;
				}
				if (aUserItem.Checked)
					aUserItem.ForeColor = SystemColors.GrayText;
			}
		}

		#endregion // ApplyToOtherUsersDialog private methods

		#region ApplyToOtherUsersDialog public properties
		
		//--------------------------------------------------------------------------------------------------------
		public bool AreAllUsersChecked
		{
			get 
			{
				return 
					(
					this.AvailableUsersListView.Items != null &&
					this.AvailableUsersListView.Items.Count > 0 &&
					this.AvailableUsersListView.CheckedItems != null &&
					this.AvailableUsersListView.CheckedItems.Count == this.AvailableUsersListView.Items.Count
					);
			}
		}

		#endregion // ApplyToOtherUsersDialog public properties

		#region ApplyToOtherUsersDialog public methods

		//--------------------------------------------------------------------------------------------------------
		public int[] GetCheckedUserIds()
		{
			if (this.AvailableUsersListView.CheckedItems == null || this.AvailableUsersListView.CheckedItems.Count == 0)
				return null;

			if (AreAllUsersChecked)
				return new int[] { -1 };

			ArrayList checkedUserIds = new ArrayList();

			foreach (ListViewItem aCheckedItem in this.AvailableUsersListView.CheckedItems)
			{
				if (aCheckedItem.Tag == null || !(aCheckedItem.Tag is int))
					continue;

				checkedUserIds.Add((int)aCheckedItem.Tag);
			}

			return (checkedUserIds.Count > 0) ? (int[])checkedUserIds.ToArray(typeof(int)) : null;
		}
		
		#endregion // ApplyToOtherUsersDialog public methods
	}
}
