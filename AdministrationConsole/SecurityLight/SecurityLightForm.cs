using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SecurityLayer.SecurityLightObjects;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls;

namespace Microarea.Console.Plugin.SecurityLight
{
	/// <summary>
	/// Summary description for SecurityLightWinUI.
	/// </summary>
	//=====================================================================
	public partial class SecurityLightForm : PlugInsForm
	{
		private const string SQL_CONNECTION_STRING_SERVER_KEYWORD						= "Server";
		private const string SQL_CONNECTION_STRING_DATABASE_KEYWORD						= "Database";
		private const string SQL_CONNECTION_STRING_LOGIN_ACCOUNT_KEYWORD				= "User Id";
		private const string SQL_CONNECTION_STRING_LOGIN_PASSWORD_KEYWORD				= "Password";
		private const string SQL_CONNECTION_STRING_INTEGRATED_SECURITY_TOKEN			= "Integrated Security";
		private const string SQL_CONNECTION_STRING_SECURITY_SUPPORT_PROVIDER_INTERFACE	= "SSPI";
	
		private const string CompanyNameSqlCommandParameterName = "@CompanyName";
		private const string UserNameSqlCommandParameterName = "@UserName";

		private ConsoleEnvironmentInfo	consoleEnvironmentInfo = null;
		private SqlConnection			currentConnection = null;
		private string					currentConnectionString = String.Empty;
		private string					currentCompany = String.Empty;
		private int						currentCompanyId = -1;
		private string					currentUser = String.Empty;
		private int						currentUserId = -1;

		private SecurityLightMenuLoader currentMenuLoader = null;
		private ArrayList loadedMenus = null;
		
		private bool isLoadingMenu = false;
		private bool enableMenuLoad = true;

		private SqlCommand findCompanyIdSqlCommand = null;
		private SqlCommand findUserIdSqlCommand = null;

		public event System.EventHandler BeforeMenuLoad;
		public event System.EventHandler AfterMenuLoad;
		public event MenuParserEventHandler ScanStandardMenuComponentsStarted;
		public event MenuParserEventHandler ScanStandardMenuComponentsModuleIndexChanged;
		public event MenuParserEventHandler ScanStandardMenuComponentsEnded;
		public event MenuParserEventHandler ScanCustomMenuComponentsStarted;
		public event MenuParserEventHandler ScanCustomMenuComponentsModuleIndexChanged;
		public event MenuParserEventHandler ScanCustomMenuComponentsEnded;
		public event MenuParserEventHandler LoadAllMenuFilesStarted;
		public event MenuParserEventHandler LoadAllMenuFilesModuleIndexChanged;
		public event MenuParserEventHandler LoadAllMenuFilesEnded;
		
		//---------------------------------------------------------------------
		public SecurityLightForm(ConsoleEnvironmentInfo	aConsoleEnvironmentInfo)
		{
			consoleEnvironmentInfo = aConsoleEnvironmentInfo;
	
			InitializeComponent();

			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.SecurityLight.gif");
			if (imageStream != null)
				this.SecurityLightLogoPictureBox.Image = Image.FromStream(imageStream);

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.User.gif");
			if (imageStream != null)
				userImage = Image.FromStream(imageStream);
			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.Users.gif");
			if (imageStream != null)
				usersImage = Image.FromStream(imageStream);
			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.Company.gif");
			if (imageStream != null)
				companyImage = Image.FromStream(imageStream);
			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.Companies.gif");
			if (imageStream != null)
				companiesImage = Image.FromStream(imageStream);

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.AllowAccessMenuItem.gif");
			if (imageStream != null)
				allowAccessMenuItemImage = Image.FromStream(imageStream);
			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.DenyAccessMenuItem.gif");
			if (imageStream != null)
				denyAccessMenuItemImage = Image.FromStream(imageStream);
			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.ShowAccessRightsMenuItem.gif");
			if (imageStream != null)
				showAccessRightsMenuItemImage = Image.FromStream(imageStream);
			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.ApplyToOtherUsersMenuItem.gif");
			if (imageStream != null)
				applyToOtherUsersMenuItemImage = Image.FromStream(imageStream);
			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.ApplyToOtherCompaniesMenuItem.gif");
			if (imageStream != null)
				applyToOtherCompaniesMenuItemImage = Image.FromStream(imageStream);
		}
		
		//---------------------------------------------------------------------
		public SecurityLightForm() : this(null)
		{
		}
		
    	#region SecurityLightForm protected overridden methods

		//--------------------------------------------------------------------------------------------------------
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed (e);

			CloseConnection();
		}

		#endregion // SecurityLightForm protected overridden methods

		#region SecurityLightForm event handlers
		
		//--------------------------------------------------------------------------------------------------------
		private void MenuMngControl_MenuTreeViewFilled(object sender, System.EventArgs e)
		{
			if (this.MenuMngControl == null || sender != this.MenuMngControl)
				return;

			RefreshAllMenuNodesState();
		}

		//--------------------------------------------------------------------------------------------------------
		private void MenuMngControl_CommandsTreeViewFilled(object sender, System.EventArgs e)
		{
			if (this.MenuMngControl == null || sender != this.MenuMngControl)
				return;

			RefreshAllCommandsNodesState();
		}

		//--------------------------------------------------------------------------------------------------------
		private void MenuMngControl_DisplayMenuItemsContextMenu(object sender, MenuMngCtrlEventArgs e)
		{
			if (this.MenuMngControl == null || sender != this.MenuMngControl)
				return;

			ContextMenu menuItemsContextMenu = this.MenuMngControl.ItemsContextMenu;
			if (menuItemsContextMenu == null)
				return;
			
			menuItemsContextMenu.MenuItems.Clear();

			if (e == null || !(e.ItemType.IsGroup || e.ItemType.IsMenu || e.ItemType.IsCommand))
				return;

			MenuXmlNode clickedMenuNode = null;
			if (e.ItemType.IsGroup)
				clickedMenuNode = this.MenuMngControl.CurrentGroupNode;
			else if (e.ItemType.IsMenu)
				clickedMenuNode = this.MenuMngControl.CurrentMenuNode;
			else if (e.ItemType.IsCommand)
				clickedMenuNode = this.MenuMngControl.CurrentCommandNode;

			if (clickedMenuNode == null)
				return;

            SecurityLightManager aSecurityManager = GetNewSecurityManager();

			if (e.ItemType.IsGroup || e.ItemType.IsMenu)
			{
                SecurityLightManager.ChildrenProtectionType protectionType = aSecurityManager.GetChildrenProtectionType(clickedMenuNode, currentCompanyId, currentUserId);

                if (protectionType != SecurityLightManager.ChildrenProtectionType.All)
				{
					SecurityLightMenuItem denyAccessToAllObjectsMenuItem = new SecurityLightMenuItem(Strings.DenyAccessToAllObjectsMenuItemText);

					if (e.ItemType.IsGroup)
						denyAccessToAllObjectsMenuItem.Click += new System.EventHandler(DenyAccessToAllGroupObjectsMenuItem_Click);
					else
						denyAccessToAllObjectsMenuItem.Click += new System.EventHandler(DenyAccessToAllMenuObjectsMenuItem_Click);
					
					denyAccessToAllObjectsMenuItem.Image = denyAccessMenuItemImage;

					menuItemsContextMenu.MenuItems.Add(denyAccessToAllObjectsMenuItem);
				}
                if (protectionType != SecurityLightManager.ChildrenProtectionType.None)
				{
					SecurityLightMenuItem allowAccessToAllObjectsMenuItem = new SecurityLightMenuItem(Strings.AllowAccessToAllObjectsMenuItemText);
					
					if (e.ItemType.IsGroup)
						allowAccessToAllObjectsMenuItem.Click += new System.EventHandler(AllowAccessToAllGroupObjectsMenuItem_Click);
					else
						allowAccessToAllObjectsMenuItem.Click += new System.EventHandler(AllowAccessToAllMenuObjectsMenuItem_Click);
					
					allowAccessToAllObjectsMenuItem.Image = allowAccessMenuItemImage;

					menuItemsContextMenu.MenuItems.Add(allowAccessToAllObjectsMenuItem);
				}

				if (menuItemsContextMenu.MenuItems.Count > 0 && String.Compare(menuItemsContextMenu.MenuItems[menuItemsContextMenu.MenuItems.Count-1].Text, "-") != 0)
					menuItemsContextMenu.MenuItems.Add("-");
		
				SecurityLightMenuItem showAccessRightsMenuItem = new SecurityLightMenuItem(Strings.ShowAccessRightsMenuItemText);
			
				if (e.ItemType.IsGroup)
					showAccessRightsMenuItem.Click += new System.EventHandler(ShowGroupAccessRightsMenuItem_Click);
				else
					showAccessRightsMenuItem.Click += new System.EventHandler(ShowMenuAccessRightsMenuItem_Click);

				showAccessRightsMenuItem.Image = showAccessRightsMenuItemImage;
				menuItemsContextMenu.MenuItems.Add(showAccessRightsMenuItem);

				if (currentUserId != -1 || currentCompanyId != -1)
				{
					MenuItem applySameAccessRightsToMenuItem = new MenuItem(Strings.ApplySameAccessRightsToMenuItemText);
					if (currentUserId != -1)
					{				
						SecurityLightMenuItem applySameAccessRightsToOtherUsersMenuItem = new SecurityLightMenuItem(Strings.ApplyToOtherUsersMenuItemText);

						if (e.ItemType.IsGroup)
							applySameAccessRightsToOtherUsersMenuItem.Click += new System.EventHandler(ApplyGroupSameAccessRightsToOtherUsersMenuItem_Click);
						else
							applySameAccessRightsToOtherUsersMenuItem.Click += new System.EventHandler(ApplyMenuSameAccessRightsToOtherUsersMenuItem_Click);

						applySameAccessRightsToOtherUsersMenuItem.Image = applyToOtherUsersMenuItemImage;

						applySameAccessRightsToMenuItem.MenuItems.Add(applySameAccessRightsToOtherUsersMenuItem);
					}

					if (currentCompanyId != -1)
					{
						SecurityLightMenuItem applySameAccessRightsToOtherCompaniesMenuItem = new SecurityLightMenuItem(Strings.ApplyToOtherCompaniesMenuItemText);
				
						if (e.ItemType.IsGroup)
							applySameAccessRightsToOtherCompaniesMenuItem.Click += new System.EventHandler(ApplyGroupSameAccessRightsToOtherCompaniesMenuItem_Click);
						else
							applySameAccessRightsToOtherCompaniesMenuItem.Click += new System.EventHandler(ApplyMenuSameAccessRightsToOtherCompaniesMenuItem_Click);
				
						applySameAccessRightsToOtherCompaniesMenuItem.Image = applyToOtherCompaniesMenuItemImage;

						applySameAccessRightsToMenuItem.MenuItems.Add(applySameAccessRightsToOtherCompaniesMenuItem);
					}
					menuItemsContextMenu.MenuItems.Add(applySameAccessRightsToMenuItem);
				}

				return;
			}

			bool isAccessToCommandDenied = aSecurityManager.IsAccessToMenuCommandDenied(clickedMenuNode, currentCompanyId, currentUserId);
			if (isAccessToCommandDenied)
			{
				SecurityLightMenuItem allowAccessToCommandMenuItem = new SecurityLightMenuItem(Strings.AllowAccessToCommandMenuItemText, new System.EventHandler(AllowAccessToCommandMenuItem_Click));
					
				allowAccessToCommandMenuItem.Image = allowAccessMenuItemImage;

				menuItemsContextMenu.MenuItems.Add(allowAccessToCommandMenuItem);

                SecurityLightMenuItem allowUnattendedModeMenuItem = new SecurityLightMenuItem(Strings.AllowUnattendedModeMenuItemText, new System.EventHandler(AllowUnattendedModeMenuItem_Click));
                allowUnattendedModeMenuItem.Checked = aSecurityManager.IsAccessToMenuCommandInUnattendedModeAllowed(clickedMenuNode, currentCompanyId, currentUserId);
                menuItemsContextMenu.MenuItems.Add(allowUnattendedModeMenuItem);
            }
			else
			{
				SecurityLightMenuItem denyAccessToCommandMenuItem = new SecurityLightMenuItem(Strings.DenyAccessToCommandMenuItemText, new System.EventHandler(DenyAccessToCommandMenuItem_Click));
					
				denyAccessToCommandMenuItem.Image = denyAccessMenuItemImage;

				menuItemsContextMenu.MenuItems.Add(denyAccessToCommandMenuItem);
			}
		
			if (menuItemsContextMenu.MenuItems.Count > 0 && String.Compare(menuItemsContextMenu.MenuItems[menuItemsContextMenu.MenuItems.Count-1].Text, "-") != 0)
				menuItemsContextMenu.MenuItems.Add("-");

			SecurityLightMenuItem showCommandAccessRightsMenuItem = new SecurityLightMenuItem(Strings.ShowAccessRightsMenuItemText);
			showCommandAccessRightsMenuItem.Click += new System.EventHandler(ShowCommandAccessRightsMenuItem_Click);
			showCommandAccessRightsMenuItem.Image = showAccessRightsMenuItemImage;
			menuItemsContextMenu.MenuItems.Add(showCommandAccessRightsMenuItem);

			if (currentUserId != -1 || currentCompanyId != -1)
			{
				MenuItem applyCurrentAccessToMenuItem = new MenuItem(Strings.ApplyCurrentAccessToMenuItemText);

				if (currentUserId != -1)
				{	
					SecurityLightMenuItem applyToOtherUsersMenuItem = new SecurityLightMenuItem(Strings.ApplyToOtherUsersMenuItemText, new System.EventHandler(ApplyToOtherUsersMenuItem_Click));

					applyToOtherUsersMenuItem.Image = applyToOtherUsersMenuItemImage;

					applyCurrentAccessToMenuItem.MenuItems.Add(applyToOtherUsersMenuItem);
				}

				if (currentCompanyId != -1)
				{
					SecurityLightMenuItem applyToOtherCompaniesMenuItem = new SecurityLightMenuItem(Strings.ApplyToOtherCompaniesMenuItemText, new System.EventHandler(ApplyToOtherCompaniesMenuItem_Click));
				
					applyToOtherCompaniesMenuItem.Image = applyToOtherCompaniesMenuItemImage;
				
					applyCurrentAccessToMenuItem.MenuItems.Add(applyToOtherCompaniesMenuItem);
				}
				menuItemsContextMenu.MenuItems.Add(applyCurrentAccessToMenuItem);
			}
		}

		//--------------------------------------------------------------------------------------------------------
		private void DenyAccessToAllGroupObjectsMenuItem_Click(object sender, System.EventArgs e)
		{
			MenuXmlNode currentGroupMenuNode = this.MenuMngControl.CurrentGroupNode;
			if (currentGroupMenuNode == null)
				return;

			ArrayList menuItems = currentGroupMenuNode.MenuItems;
			if (menuItems == null || menuItems.Count == 0)
				return;

			this.Cursor = Cursors.WaitCursor;

			foreach (MenuXmlNode aMenuNode in menuItems)
				DenyAccessToAllMenuObjects(aMenuNode);
		
			RefreshAllMenuNodesState();
		
			this.Cursor = Cursors.Default;
		}

		//--------------------------------------------------------------------------------------------------------
		private void AllowAccessToAllGroupObjectsMenuItem_Click(object sender, System.EventArgs e)
		{
			MenuXmlNode currentGroupMenuNode = this.MenuMngControl.CurrentGroupNode;
			if (currentGroupMenuNode == null)
				return;

			ArrayList menuItems = currentGroupMenuNode.MenuItems;
			if (menuItems == null || menuItems.Count == 0)
				return;

			this.Cursor = Cursors.WaitCursor;

			foreach (MenuXmlNode aMenuNode in menuItems)
				AllowAccessToAllMenuObjects(aMenuNode);
		
			RefreshAllMenuNodesState();

			this.Cursor = Cursors.Default;
		}

		//--------------------------------------------------------------------------------------------------------
		private void DenyAccessToAllMenuObjectsMenuItem_Click(object sender, System.EventArgs e)
		{
			MenuXmlNode currentMenuNode = this.MenuMngControl.CurrentMenuNode;
			if (currentMenuNode == null)
				return;
		
			this.Cursor = Cursors.WaitCursor;

			DenyAccessToAllMenuObjects(currentMenuNode);
		
			RefreshAllMenuNodesState();

			this.Cursor = Cursors.Default;
		}

		//--------------------------------------------------------------------------------------------------------
		private void AllowAccessToAllMenuObjectsMenuItem_Click(object sender, System.EventArgs e)
		{
			MenuXmlNode currentMenuNode = this.MenuMngControl.CurrentMenuNode;
			if (currentMenuNode == null)
				return;

			this.Cursor = Cursors.WaitCursor;

			AllowAccessToAllMenuObjects(currentMenuNode);

			RefreshAllMenuNodesState();

			this.Cursor = Cursors.Default;
		}

		//--------------------------------------------------------------------------------------------------------
		private void DenyAccessToCommandMenuItem_Click(object sender, System.EventArgs e)
		{
			MenuXmlNode currentCommandMenuNode = this.MenuMngControl.CurrentCommandNode;
			if (currentCommandMenuNode == null)
				return;
	
			this.Cursor = Cursors.WaitCursor;

            SecurityLightManager aSecurityManager = GetNewSecurityManager();
			
			if (!aSecurityManager.DenyAccessToMenuCommand(currentCommandMenuNode, currentCompanyId, currentUserId))
			{
				this.Cursor = Cursors.Default;

				MessageBox.Show
					(
					this, 
					String.Format(Strings.AccessRightChangeFailedMsgFmt, currentCommandMenuNode.ItemObject), 
					Strings.AccessRightChangeErrorCaption, 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Error
					);
				return;
			}

			this.MenuMngControl.AdjustCommandNodeStateImageIndex(this.MenuMngControl.FindCommandTreeNode(currentCommandMenuNode));

			RefreshAllMenuNodesState();

			this.Cursor = Cursors.Default;

			// Problema: se non si riseleziona esplicitamente il nodo corrente non aggiorna correttamente il nodo 
			// individuato da this.MenuMngControl.CurrentCommandNode (se clicco di nuovo sul nodo, esso risulta 
			// ancora con state uguale a allowed e mi ripropone il context menu errato)
			this.MenuMngControl.SelectCommandNode(currentCommandMenuNode);
		}

		//--------------------------------------------------------------------------------------------------------
		private void AllowAccessToCommandMenuItem_Click(object sender, System.EventArgs e)
		{
			MenuXmlNode currentCommandMenuNode = this.MenuMngControl.CurrentCommandNode;
			if (currentCommandMenuNode == null)
				return;
	
			this.Cursor = Cursors.WaitCursor;

            SecurityLightManager aSecurityManager = GetNewSecurityManager();
			
			if (!aSecurityManager.AllowAccessToMenuCommand(currentCommandMenuNode, currentCompanyId, currentUserId))
			{
				this.Cursor = Cursors.Default;

				MessageBox.Show
					(
					this, 
					String.Format(Strings.AccessRightChangeFailedMsgFmt, currentCommandMenuNode.ItemObject), 
					Strings.AccessRightChangeErrorCaption, 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Error
					);
				return;
			}

			this.MenuMngControl.AdjustCommandNodeStateImageIndex(this.MenuMngControl.FindCommandTreeNode(currentCommandMenuNode));

			RefreshAllMenuNodesState();

			this.Cursor = Cursors.Default;

			// Problema: se non si riseleziona esplicitamente il nodo corrente non aggiorna correttamente il nodo 
			// individuato da this.MenuMngControl.CurrentCommandNode (se clicco di nuovo sul nodo, esso risulta 
			// ancora con state uguale a denied e mi ripropone il context menu errato)
			this.MenuMngControl.SelectCommandNode(currentCommandMenuNode);
		}

        //--------------------------------------------------------------------------------------------------------
        private void AllowUnattendedModeMenuItem_Click(object sender, System.EventArgs e)
        {
            if (sender == null || !(sender is SecurityLightMenuItem))
                return;

            MenuXmlNode currentCommandMenuNode = this.MenuMngControl.CurrentCommandNode;
            if (currentCommandMenuNode == null)
                return;

            this.Cursor = Cursors.WaitCursor;

            SecurityLightManager aSecurityManager = GetNewSecurityManager();

            if (!aSecurityManager.SetAccessToMenuCommandInUnattendedMode(currentCommandMenuNode, currentCompanyId, currentUserId, !((SecurityLightMenuItem)sender).Checked))
            {
                this.Cursor = Cursors.Default;

                MessageBox.Show
                    (
                    this,
                    String.Format(Strings.AccessRightChangeFailedMsgFmt, currentCommandMenuNode.ItemObject),
                    Strings.AccessRightChangeErrorCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                return;
            }
            ((SecurityLightMenuItem)sender).Checked = !((SecurityLightMenuItem)sender).Checked;

            this.MenuMngControl.AdjustCommandNodeStateImageIndex(this.MenuMngControl.FindCommandTreeNode(currentCommandMenuNode));

            RefreshAllMenuNodesState();

            this.Cursor = Cursors.Default;

            // Problema: se non si riseleziona esplicitamente il nodo corrente non aggiorna correttamente il nodo 
            // individuato da this.MenuMngControl.CurrentCommandNode (se clicco di nuovo sul nodo, esso risulta 
            // ancora con state uguale a denied e mi ripropone il context menu errato)
            this.MenuMngControl.SelectCommandNode(currentCommandMenuNode);
        }

        //--------------------------------------------------------------------------------------------------------
		private void ShowGroupAccessRightsMenuItem_Click(object sender, System.EventArgs e)
		{
			MenuXmlNode currentGroupNode = this.MenuMngControl.CurrentGroupNode;
			if (currentGroupNode == null)
				return;

			bool allDenied = false;
			if (currentUserId != -1 || currentCompanyId != -1)
			{
                SecurityLightManager aSecurityManager = GetNewSecurityManager();
                SecurityLightManager.ChildrenProtectionType allProtectionType = aSecurityManager.GetChildrenProtectionType(currentGroupNode, -1, -1);
                allDenied = (allProtectionType == SecurityLightManager.ChildrenProtectionType.All);
			}
			else
				allDenied = currentGroupNode.AccessDeniedState;

			if (allDenied)
			{
				MessageBox.Show
					(
					this, 
					String.Format(Strings.GroupAccessDeniedForAllMsgFmt, currentGroupNode.Title), 
					Strings.NoAccessRightsOverviewCaption, 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Information
					);
				return;
			}
	
			AccessRightsOverviewDialog aDlg = new AccessRightsOverviewDialog(currentGroupNode, currentConnection, this.GetUserNamesToSkip());
			aDlg.ShowDialog(this);
		}

		//--------------------------------------------------------------------------------------------------------
		private void ShowMenuAccessRightsMenuItem_Click(object sender, System.EventArgs e)
		{
			MenuXmlNode currentMenuNode = this.MenuMngControl.CurrentMenuNode;
			if (currentMenuNode == null)
				return;

			bool allDenied = false;
			if (currentUserId != -1 || currentCompanyId != -1)
			{
                SecurityLightManager aSecurityManager = GetNewSecurityManager();
                SecurityLightManager.ChildrenProtectionType allProtectionType = aSecurityManager.GetChildrenProtectionType(currentMenuNode, -1, -1);
                allDenied = (allProtectionType == SecurityLightManager.ChildrenProtectionType.All);
			}
			else
				allDenied = currentMenuNode.AccessDeniedState;

			if (allDenied)
			{
				MessageBox.Show
					(
					this, 
					String.Format(Strings.MenuAccessDeniedForAllMsgFmt, currentMenuNode.Title), 
					Strings.NoAccessRightsOverviewCaption, 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Information
					);
				return;
			}

			AccessRightsOverviewDialog aDlg = new AccessRightsOverviewDialog(currentMenuNode, currentConnection, this.GetUserNamesToSkip());
			aDlg.ShowDialog(this);
		}

		//--------------------------------------------------------------------------------------------------------
		private void ShowCommandAccessRightsMenuItem_Click(object sender, System.EventArgs e)
		{
			MenuXmlNode currentCommandMenuNode = this.MenuMngControl.CurrentCommandNode;
			if (currentCommandMenuNode == null)
				return;
			
			bool allDenied = false;
			if (currentUserId != -1 || currentCompanyId != -1)
			{
                SecurityLightManager aSecurityManager = GetNewSecurityManager();
				allDenied = aSecurityManager.IsAccessToMenuCommandDenied(currentCommandMenuNode, -1, -1, false);
			}
			else
				allDenied = currentCommandMenuNode.AccessDeniedState;

			if (allDenied)
			{
				MessageBox.Show
					(
					this, 
					String.Format(Strings.CommandAccessDeniedForAllMsgFmt, currentCommandMenuNode.Title, currentCommandMenuNode.ItemObject), 
					Strings.NoAccessRightsOverviewCaption, 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Information
					);
				return;
			}

			AccessRightsOverviewDialog aDlg = new AccessRightsOverviewDialog(currentCommandMenuNode, currentConnection, this.GetUserNamesToSkip());
			aDlg.ShowDialog(this);
		}

		//--------------------------------------------------------------------------------------------------------
		private void ApplyGroupSameAccessRightsToOtherUsersMenuItem_Click(object sender, System.EventArgs e)
		{
			ApplyObjectAccessRightToOtherUsers(this.MenuMngControl.CurrentGroupNode);
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void ApplyMenuSameAccessRightsToOtherUsersMenuItem_Click(object sender, System.EventArgs e)
		{
			ApplyObjectAccessRightToOtherUsers(this.MenuMngControl.CurrentMenuNode);
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void ApplyToOtherUsersMenuItem_Click(object sender, System.EventArgs e)
		{
			ApplyObjectAccessRightToOtherUsers(this.MenuMngControl.CurrentCommandNode);
		}

		//--------------------------------------------------------------------------------------------------------
		private void ApplyGroupSameAccessRightsToOtherCompaniesMenuItem_Click(object sender, System.EventArgs e)
		{
			ApplyObjectAccessRightToOtherCompanies(this.MenuMngControl.CurrentGroupNode);
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void ApplyMenuSameAccessRightsToOtherCompaniesMenuItem_Click(object sender, System.EventArgs e)
		{
			ApplyObjectAccessRightToOtherCompanies(this.MenuMngControl.CurrentMenuNode);
		}

		//--------------------------------------------------------------------------------------------------------
		private void ApplyToOtherCompaniesMenuItem_Click(object sender, System.EventArgs e)
		{
			ApplyObjectAccessRightToOtherCompanies(this.MenuMngControl.CurrentCommandNode);
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void UsersComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			string selectedUser = (this.UsersComboBox.SelectedIndex > 0 && (this.UsersComboBox.SelectedItem is string)) ? (string)this.UsersComboBox.SelectedItem : NameSolverStrings.AllUsers;
			if (selectedUser == null || selectedUser.Length == 0 || String.Compare(selectedUser, Strings.AllUsersComboBoxItemText) == 0)
				selectedUser = NameSolverStrings.AllUsers;

			if (String.Compare(currentUser, selectedUser) != 0)
			{
				currentUser = selectedUser;
			
				currentUserId = -1;
				if (String.Compare(currentUser, NameSolverStrings.AllUsers) != 0)
				{
					this.UserPictureBox.Image = userImage;

					SqlDataReader userReader = null;
					try
					{
						OpenConnection();

						if (currentConnection != null && IsConnectionOpen)
						{
							if (findUserIdSqlCommand == null)
							{
								string queryText = @"SELECT MSD_Logins.LoginId FROM MSD_Logins WHERE MSD_Logins.Login = " + UserNameSqlCommandParameterName;

								findUserIdSqlCommand = new SqlCommand(queryText, currentConnection);
								//	if (this.consoleEnvironmentInfo.UseUnicode)
								//		findUserIdSqlCommand.Parameters.Add(UserNameSqlCommandParameterName, SqlDbType.NVarChar, 50);
								//	else
								findUserIdSqlCommand.Parameters.Add(UserNameSqlCommandParameterName, SqlDbType.VarChar, 50);
								findUserIdSqlCommand.Prepare();
							}
						
							findUserIdSqlCommand.Parameters[UserNameSqlCommandParameterName].Value = new SqlString(currentUser);

							userReader = findUserIdSqlCommand.ExecuteReader(CommandBehavior.SingleRow);

							if (userReader.Read())
							{
								currentUserId = Convert.ToInt32(userReader["LoginId"]);
							}

							userReader.Close();
						}
					}
					catch(Exception exception)
					{
						Debug.WriteLine("SqlException raised in SecurityLightForm.UsersComboBox_SelectedIndexChanged: " + exception.Message);
					}
					finally
					{
						if (userReader != null && !userReader.IsClosed)
							userReader.Close();
					}
				}
				else
					this.UserPictureBox.Image = usersImage;
			}
			
			currentUser = selectedUser;

			FillCompaniesComboBox();
		}
			
		//----------------------------------------------------------------------------
		private void CompaniesComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			string selectedCompany = (this.CompaniesComboBox.SelectedIndex > 0 && (this.CompaniesComboBox.SelectedItem is string)) ? (string)this.CompaniesComboBox.SelectedItem : NameSolverStrings.AllCompanies;
			if (selectedCompany == null || selectedCompany.Length == 0 || String.Compare(selectedCompany, Strings.AllCompaniesComboBoxItemText) == 0)
				selectedCompany = NameSolverStrings.AllCompanies;

			if (String.Compare(currentCompany, selectedCompany) != 0)
			{
				currentCompany = selectedCompany;
			
				currentCompanyId = -1;
				if (String.Compare(currentCompany, NameSolverStrings.AllCompanies) != 0)
				{
					this.CompanyPictureBox.Image = companyImage;

					SqlDataReader companyReader = null;
					try
					{
						OpenConnection();

						if (currentConnection != null && IsConnectionOpen)
						{
							if (findCompanyIdSqlCommand == null)
							{
								string queryText = "SELECT MSD_Companies.CompanyId FROM MSD_Companies WHERE MSD_Companies.Company = " + CompanyNameSqlCommandParameterName;

								findCompanyIdSqlCommand = new SqlCommand(queryText, currentConnection);
								//	if (this.consoleEnvironmentInfo.UseUnicode)
								//		findCompanyIdSqlCommand.Parameters.Add(CompanyNameSqlCommandParameterName, SqlDbType.NVarChar, 50);
								//	else
								findCompanyIdSqlCommand.Parameters.Add(CompanyNameSqlCommandParameterName, SqlDbType.VarChar, 50);
								findCompanyIdSqlCommand.Prepare();
							}
						
							findCompanyIdSqlCommand.Parameters[CompanyNameSqlCommandParameterName].Value = new SqlString(currentCompany);

							companyReader = findCompanyIdSqlCommand.ExecuteReader(CommandBehavior.SingleRow);

							if (companyReader.Read())
							{
								currentCompanyId = Convert.ToInt32(companyReader["CompanyId"]);
							}

							companyReader.Close();
						}
					}
					catch(Exception exception)
					{
						Debug.WriteLine("SqlException raised in SecurityLightForm.CompaniesComboBox_SelectedIndexChanged: " + exception.Message);
					}
					finally
					{
						if (companyReader != null && !companyReader.IsClosed)
							companyReader.Close();
					}
				}
				else
					this.CompanyPictureBox.Image = companiesImage;
			}
		
			LoadCurrentUserMenu();
		}

		//----------------------------------------------------------------------------
		public void MenuLoader_ScanStandardMenuComponentsStarted(object sender, MenuParserEventArgs e)
		{ 
			if (ScanStandardMenuComponentsStarted != null)
				ScanStandardMenuComponentsStarted(this, e);
			
		}

		//----------------------------------------------------------------------------
		public void MenuLoader_ScanStandardMenuComponentsModuleIndexChanged(object sender, MenuParserEventArgs e)
		{ 
			if (ScanStandardMenuComponentsModuleIndexChanged != null)
				ScanStandardMenuComponentsModuleIndexChanged(this, e);
		}

		//----------------------------------------------------------------------------
		public void MenuLoader_ScanStandardMenuComponentsEnded(object sender, MenuParserEventArgs e)
		{ 
			if (ScanStandardMenuComponentsEnded != null)
				ScanStandardMenuComponentsEnded(this, e);
		}

		//----------------------------------------------------------------------------
		public void MenuLoader_ScanCustomMenuComponentsStarted(object sender, MenuParserEventArgs e)
		{ 
			if (ScanCustomMenuComponentsStarted != null)
				ScanCustomMenuComponentsStarted(this, e);
			
		}

		//----------------------------------------------------------------------------
		public void MenuLoader_ScanCustomMenuComponentsModuleIndexChanged(object sender, MenuParserEventArgs e)
		{ 
			if (ScanCustomMenuComponentsModuleIndexChanged != null)
				ScanCustomMenuComponentsModuleIndexChanged(this, e);
		}

		//----------------------------------------------------------------------------
		public void MenuLoader_ScanCustomMenuComponentsEnded(object sender, MenuParserEventArgs e)
		{ 
			if (ScanCustomMenuComponentsEnded != null)
				ScanCustomMenuComponentsEnded(this, e);
		}

		//----------------------------------------------------------------------------
		public void MenuLoader_LoadAllMenuFilesStarted (object sender, MenuParserEventArgs e)
		{ 
			if (LoadAllMenuFilesStarted != null)
				LoadAllMenuFilesStarted(this, e);
		}

		//----------------------------------------------------------------------------
		public void MenuLoader_LoadAllMenuFilesModuleIndexChanged (object sender, MenuParserEventArgs e)
		{ 
			if (LoadAllMenuFilesModuleIndexChanged != null)
				LoadAllMenuFilesModuleIndexChanged(this, e);
		}
		
		//----------------------------------------------------------------------------
		public void MenuLoader_LoadAllMenuFilesEnded (object sender, MenuParserEventArgs e)
		{ 
			if (LoadAllMenuFilesEnded != null)
				LoadAllMenuFilesEnded(this, e);
		}

        //--------------------------------------------------------------------------------------------------------
        private void RebuildAllSLDenyFilesButton_Click(object sender, EventArgs e)
        {
            if (
                currentConnection == null ||
                !IsConnectionOpen ||
                this.MenuMngControl == null ||
                this.MenuMngControl.IsDisposed ||
                this.MenuMngControl.PathFinder == null
                )
                return;

            this.Cursor = Cursors.WaitCursor;

            SecurityLightManager.RebuildAllSLDenyFiles(this.MenuMngControl.PathFinder, currentConnection, GetUserNamesToSkip());

            this.Cursor = Cursors.Default;
        }
        
        #endregion // SecurityLightForm event handlers
		
		#region SecurityLightForm private methods

		//--------------------------------------------------------------------------------------------------------
		private void OpenConnection()
		{
			if (currentConnection != null && IsConnectionOpen)
				return;

			if (currentConnectionString == null || currentConnectionString.Length == 0)
				return;

			try
			{
				if (currentConnection == null)
					currentConnection = new SqlConnection(currentConnectionString);
				
				// The Open method uses the information in the ConnectionString
				// property to contact the data source and establish an open connection
				currentConnection.Open();
			}
			catch (SqlException e)
			{
				if (MessageBox.Show
					(
					this, 
					String.Format(Strings.OpenConnectionErrorMsgFmt, e.Message), 
					Strings.ConnectionErrorCaption, 
					MessageBoxButtons.RetryCancel, 
					MessageBoxIcon.Error
					) == DialogResult.Retry
					)
					OpenConnection();
			}
		}

		//--------------------------------------------------------------------------------------------------------
		private void CloseConnection()
		{
			if (findCompanyIdSqlCommand != null)
				findCompanyIdSqlCommand.Dispose();
			findCompanyIdSqlCommand = null;
				
			if (findUserIdSqlCommand != null)
				findUserIdSqlCommand.Dispose();
			findUserIdSqlCommand = null;

			if (currentConnection != null)
			{
				if (IsConnectionOpen)
					currentConnection.Close();
			
				currentConnection.Dispose();
			}

			currentConnection = null;
			currentConnectionString = String.Empty;
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void FillCompaniesComboBox()
		{
			string currentlySelectedCompanyName = (this.CompaniesComboBox.SelectedIndex >= 0 && (this.CompaniesComboBox.SelectedItem is string)) ? (string)this.CompaniesComboBox.SelectedItem : String.Empty;

			this.CompaniesComboBox.Items.Clear();

			SqlCommand selectCompaniesSqlCommand = null;
			SqlDataReader companiesReader = null;

			try
			{
				OpenConnection();

				if (currentConnection == null || !IsConnectionOpen)
					return;
	
				this.CompaniesComboBox.Items.Add(Strings.AllCompaniesComboBoxItemText);

				string queryText = String.Empty;
				if (currentUserId == -1 || currentUser == null || currentUser.Length == 0 || String.Compare(currentUser, NameSolverStrings.AllCompanies) == 0)
					queryText = "SELECT MSD_Companies.Company FROM MSD_Companies ORDER BY MSD_Companies.Company";
				else
					queryText = @"SELECT MSD_CompanyLogins.CompanyId, MSD_Companies.Company FROM MSD_CompanyLogins INNER JOIN
								MSD_Companies ON MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyId WHERE LoginId = " + currentUserId.ToString() + " ORDER BY MSD_Companies.Company";

				selectCompaniesSqlCommand = new SqlCommand(queryText, currentConnection);

				companiesReader = selectCompaniesSqlCommand.ExecuteReader();

				while (companiesReader.Read())
				{
					string companyName = companiesReader["Company"].ToString();
					if (companyName != null && companyName.Length > 0)
						this.CompaniesComboBox.Items.Add(companyName);
				}

				companiesReader.Close();
			}
			catch(SqlException exception)
			{
				Debug.WriteLine("SqlException raised in SecurityLightForm.FillUsersComboBox: " + exception.Message);
			}
			finally
			{
				if (companiesReader != null && !companiesReader.IsClosed)
					companiesReader.Close();

				if (selectCompaniesSqlCommand != null)
					selectCompaniesSqlCommand.Dispose();
			}

			if (this.CompaniesComboBox.Items.Count > 0)
			{
				int selIdx = (currentlySelectedCompanyName != null && currentlySelectedCompanyName.Length > 0) ? this.CompaniesComboBox.FindStringExact(currentlySelectedCompanyName) : 0;
				if (selIdx >= 0 && selIdx < this.CompaniesComboBox.Items.Count)
					this.CompaniesComboBox.SelectedIndex = selIdx;
				else
					this.CompaniesComboBox.SelectedIndex = 0;
			}
		}

		//--------------------------------------------------------------------------------------------------------
		private void FillUsersComboBox()
		{
			string currentlySelectedUserName = (this.UsersComboBox.SelectedIndex >= 0 && (this.UsersComboBox.SelectedItem is string)) ? (string)this.UsersComboBox.SelectedItem : String.Empty;

			this.UsersComboBox.Items.Clear();

			SqlCommand selectUsersSqlCommand = null;
			SqlDataReader usersReader = null;

			try
			{
				OpenConnection();

				if (currentConnection == null || !IsConnectionOpen)
					return;
	
				this.UsersComboBox.Items.Add(Strings.AllUsersComboBoxItemText);

				string queryText = "SELECT MSD_Logins.Login FROM MSD_Logins ORDER BY MSD_Logins.Login";

				selectUsersSqlCommand = new SqlCommand(queryText, currentConnection);

				usersReader  = selectUsersSqlCommand.ExecuteReader();

				while (usersReader.Read())
				{
					string userName = usersReader["Login"].ToString();

					if	(string.IsNullOrEmpty(userName) ||
						(string.Compare(userName, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0) ||
						(string.Compare(userName, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0))
						continue;

					this.UsersComboBox.Items.Add(userName);
				}

				usersReader.Close();
			}
			catch(SqlException exception)
			{
				Debug.WriteLine("SqlException raised in SecurityLightForm.FillUsersComboBox: " + exception.Message);
			}
			finally
			{
				if (usersReader != null && !usersReader.IsClosed)
					usersReader.Close();

				if (selectUsersSqlCommand != null)
					selectUsersSqlCommand.Dispose();
			}

			if (this.UsersComboBox.Items.Count > 0)
			{
				int selIdx = (currentlySelectedUserName != null && currentlySelectedUserName.Length > 0) ? this.UsersComboBox.FindStringExact(currentlySelectedUserName) : 0;
				if (selIdx >= 0 && selIdx < this.UsersComboBox.Items.Count)
					this.UsersComboBox.SelectedIndex = selIdx;
				else
					this.UsersComboBox.SelectedIndex = 0;
			}
		}

		//--------------------------------------------------------------------------------------------------------
		private void LoadCurrentUserMenu()
		{
			if (!enableMenuLoad || this.MenuMngControl == null || this.MenuMngControl.Disposing)
				return;

			if (currentCompany == null || currentCompany.Length == 0 || String.Compare(currentCompany, Strings.AllCompaniesComboBoxItemText) == 0)
			{
				currentCompanyId = -1;
				currentCompany = NameSolverStrings.AllCompanies;
			}

			if (currentUser == null || currentUser.Length == 0 || String.Compare(currentUser, Strings.AllUsersComboBoxItemText) == 0)
				currentUser = NameSolverStrings.AllUsers;

			if (
				this.MenuMngControl.PathFinder != null &&
				String.Compare(currentCompany, this.MenuMngControl.PathFinder.Company) == 0 &&
				String.Compare(currentUser, this.MenuMngControl.PathFinder.User) == 0
				)
				return;

			if (BeforeMenuLoad != null)
				BeforeMenuLoad(this, System.EventArgs.Empty);

			isLoadingMenu = true;

			MenuParserSelection currentMenuSelection = null;
			if (
				this.MenuMngControl.PathFinder != null &&
				this.MenuMngControl.MenuXmlParser != null
				)
			{
				currentMenuSelection = new MenuParserSelection();
				currentMenuSelection.ApplicationName	= this.MenuMngControl.CurrentApplicationName;	
				currentMenuSelection.GroupName			= this.MenuMngControl.CurrentGroupName;
				currentMenuSelection.MenuPath			= this.MenuMngControl.CurrentMenuPath;
				currentMenuSelection.CommandPath		= this.MenuMngControl.CurrentCommandPath;
			}

			this.MenuMngControl.PathFinder = null;
			this.MenuMngControl.MenuXmlParser = null;

			currentMenuLoader = GetLoadedMenu(currentCompany, currentUser);
			if (currentMenuLoader == null || currentMenuLoader.PathFinder == null || currentMenuLoader.CurrentMenuParser == null)
			{
				PathFinder pathFinder = new PathFinder(currentCompany, currentUser);
				

				currentMenuLoader = new SecurityLightMenuLoader(pathFinder);

				currentMenuLoader.ScanStandardMenuComponentsStarted				+= new MenuParserEventHandler(MenuLoader_ScanStandardMenuComponentsStarted);
				currentMenuLoader.ScanStandardMenuComponentsModuleIndexChanged	+= new MenuParserEventHandler(MenuLoader_ScanStandardMenuComponentsModuleIndexChanged);
				currentMenuLoader.ScanStandardMenuComponentsEnded				+= new MenuParserEventHandler(MenuLoader_ScanStandardMenuComponentsEnded);

				currentMenuLoader.ScanCustomMenuComponentsStarted				+= new MenuParserEventHandler(MenuLoader_ScanCustomMenuComponentsStarted);
				currentMenuLoader.ScanCustomMenuComponentsModuleIndexChanged	+= new MenuParserEventHandler(MenuLoader_ScanCustomMenuComponentsModuleIndexChanged);
				currentMenuLoader.ScanCustomMenuComponentsEnded					+= new MenuParserEventHandler(MenuLoader_ScanCustomMenuComponentsEnded);

				currentMenuLoader.LoadAllMenuFilesStarted						+= new MenuParserEventHandler(MenuLoader_LoadAllMenuFilesStarted);
				currentMenuLoader.LoadAllMenuFilesModuleIndexChanged			+= new MenuParserEventHandler(MenuLoader_LoadAllMenuFilesModuleIndexChanged);
				currentMenuLoader.LoadAllMenuFilesEnded							+= new MenuParserEventHandler(MenuLoader_LoadAllMenuFilesEnded);

				MenuXmlParser currentMenuParser = currentMenuLoader.LoadAllMenuFiles();

				SetLoadedMenu(currentMenuLoader);

				this.MenuMngControl.PathFinder = pathFinder;

				this.MenuMngControl.MenuXmlParser = currentMenuParser;
			}
			else
			{
				this.MenuMngControl.PathFinder = currentMenuLoader.PathFinder;

				currentMenuLoader.CleanAllAccessibilityNodeStates();
				this.MenuMngControl.MenuXmlParser = currentMenuLoader.CurrentMenuParser;
			}

			this.MenuMngControl.Select(currentMenuSelection);

			isLoadingMenu = false;
			
			if (AfterMenuLoad != null)
				AfterMenuLoad(this, System.EventArgs.Empty);
		}
		
		//--------------------------------------------------------------------------------------------------------
		private SecurityLightMenuLoader GetLoadedMenu(string aCompanyName, string aUserName)
		{
			if 
				(
				loadedMenus == null ||
				loadedMenus.Count == 0
				)
				return null;

			foreach (SecurityLightMenuLoader aMenuLoader in loadedMenus)
			{
				if 
					(
					aMenuLoader != null && 
					String.Compare(aMenuLoader.CompanyName, aCompanyName) == 0 &&
					String.Compare(aMenuLoader.UserName, aUserName) == 0
					)
					return aMenuLoader;
			}

			return null;
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void SetLoadedMenu(SecurityLightMenuLoader aMenuLoader)
		{
			if (aMenuLoader == null)
				return;

			if (loadedMenus == null)
				loadedMenus = new ArrayList();
			
			loadedMenus.Add(aMenuLoader);
		}
		
		//--------------------------------------------------------------------------------------------------------
        private SecurityLightManager GetNewSecurityManager()
		{
            return new SecurityLightManager(currentMenuLoader, currentConnection, GetUserNamesToSkip());
		}

		//--------------------------------------------------------------------------------------------------------
        private void DenyAccessToAllMenuObjects(MenuXmlNode aMenuNode, SecurityLightManager aSecurityManager)
		{
			if (aMenuNode == null || !aMenuNode.IsMenu || aSecurityManager == null)
				return;

			ArrayList menuItems = aMenuNode.MenuItems;
			if (menuItems != null && menuItems.Count > 0)
			{
				foreach (MenuXmlNode aSubMenuNode in menuItems)
					DenyAccessToAllMenuObjects(aSubMenuNode, aSecurityManager);
			}

			ArrayList commandItems = aMenuNode.CommandItems;
			if (commandItems != null && commandItems.Count > 0)
			{
				foreach (MenuXmlNode aCommandNode in commandItems)
					aSecurityManager.DenyAccessToMenuCommand(aCommandNode, currentCompanyId, currentUserId);
			}

			aMenuNode.AccessDeniedState = true;

			this.MenuMngControl.AdjustSubTreeNodeStateImageIndex(this.MenuMngControl.FindMenuNode(aMenuNode));
		}

		//--------------------------------------------------------------------------------------------------------
		private void DenyAccessToAllMenuObjects(MenuXmlNode aMenuNode)
		{
			DenyAccessToAllMenuObjects(aMenuNode, GetNewSecurityManager());
		}

		//--------------------------------------------------------------------------------------------------------
        private void AllowAccessToAllMenuObjects(MenuXmlNode aMenuNode, SecurityLightManager aSecurityManager)
		{
			if (aMenuNode == null || !aMenuNode.IsMenu || aSecurityManager == null)
				return;

			ArrayList menuItems = aMenuNode.MenuItems;
			if (menuItems != null && menuItems.Count > 0)
			{
				foreach (MenuXmlNode aSubMenuNode in menuItems)
					AllowAccessToAllMenuObjects(aSubMenuNode, aSecurityManager);
			}

			ArrayList commandItems = aMenuNode.CommandItems;
			if (commandItems != null && commandItems.Count > 0)
			{
				foreach (MenuXmlNode aCommandNode in commandItems)
					aSecurityManager.AllowAccessToMenuCommand(aCommandNode, currentCompanyId, currentUserId);
			}

			aMenuNode.AccessAllowedState = true;

			this.MenuMngControl.AdjustSubTreeNodeStateImageIndex(this.MenuMngControl.FindMenuNode(aMenuNode));
		}

		//--------------------------------------------------------------------------------------------------------
		private void AllowAccessToAllMenuObjects(MenuXmlNode aMenuNode)
		{
			AllowAccessToAllMenuObjects(aMenuNode, GetNewSecurityManager());
		}

		//--------------------------------------------------------------------------------------------------------
		private void RefreshAllMenuNodesState()
		{
			if (this.MenuMngControl == null)
				return;

			RefreshMenuNodesState(this.MenuMngControl.MenuTreeNodes);
		}

		//--------------------------------------------------------------------------------------------------------
		private void RefreshMenuNodesState(MenuTreeNodeCollection menuTreeNodes)
		{
			if (menuTreeNodes == null || menuTreeNodes.Count == 0)
				return;

			foreach (MenuTreeNode aMenuTreeNode in menuTreeNodes)
			{
				SetMenuNodeState(aMenuTreeNode.Node);

				RefreshMenuNodesState(aMenuTreeNode.Nodes);
			}
		}

		//---------------------------------------------------------------------
		private void SetMenuNodeState(MenuXmlNode aMenuNode)
		{
			if (aMenuNode == null || !aMenuNode.IsMenu || (!aMenuNode.HasMenuChildNodes() && !aMenuNode.HasCommandChildNodes()))
				return;

            SecurityLightManager aSecurityManager = GetNewSecurityManager();

            SecurityLightManager.ChildrenProtectionType protectionType = aSecurityManager.GetChildrenProtectionType(aMenuNode, currentCompanyId, currentUserId);

            if (protectionType == SecurityLightManager.ChildrenProtectionType.All)
				aMenuNode.AccessDeniedState = true;
            else if (protectionType == SecurityLightManager.ChildrenProtectionType.None)
				aMenuNode.AccessAllowedState = true;
			else
				aMenuNode.AccessPartiallyAllowedState = true;

			this.MenuMngControl.AdjustSubTreeNodeStateImageIndex(this.MenuMngControl.FindMenuNode(aMenuNode));
		}
		
		//---------------------------------------------------------------------
		private void RefreshCommandNodeState(MenuXmlNode aMenuNode)
		{
			if (aMenuNode == null || !(aMenuNode.IsRunDocument || aMenuNode.IsRunBatch || aMenuNode.IsRunReport || aMenuNode.IsRunFunction || aMenuNode.IsOfficeItem))
				return;

            SecurityLightManager aSecurityManager = GetNewSecurityManager();

            bool isAccessToCommandDenied = aSecurityManager.IsAccessToMenuCommandDenied(aMenuNode, currentCompanyId, currentUserId, false);

            if (isAccessToCommandDenied)
            {
                aMenuNode.AccessDeniedState = true;
                if (aSecurityManager.IsAccessToMenuCommandInUnattendedModeAllowed(aMenuNode, currentCompanyId, currentUserId, false))
                    aMenuNode.AccessInUnattendedModeAllowedState = true;
            }
            else
                aMenuNode.AccessAllowedState = true;
		
			this.MenuMngControl.AdjustCommandNodeStateImageIndex(this.MenuMngControl.FindCommandTreeNode(aMenuNode));
		}

		//---------------------------------------------------------------------
		private void RefreshAllCommandsNodesState()
		{
			if (this.MenuMngControl == null)
				return;

			MenuTreeNodeCollection commandTreeNodes = this.MenuMngControl.CommandsTreeNodes;
			
			if (commandTreeNodes == null || commandTreeNodes.Count == 0)
				return;

			foreach (MenuTreeNode aCommandTreeNode in commandTreeNodes)
				RefreshCommandNodeState(aCommandTreeNode.Node);
		}

		//--------------------------------------------------------------------------------------------------------
		private void ApplyObjectAccessRightToOtherUsers(MenuXmlNode aMenuNode)
		{
			if (aMenuNode == null || !(aMenuNode.IsGroup || aMenuNode.IsMenu || aMenuNode.IsCommand))
				return;

			if (currentUserId == -1)
				return; // è selezionato <All Users>

			ArrayList userNamesToSkipInDlg = new ArrayList();

			userNamesToSkipInDlg.Add(currentUser);
			string[] userNamesToSkip = GetUserNamesToSkip();
			if (userNamesToSkip != null && userNamesToSkip.Length > 0)
				userNamesToSkipInDlg.AddRange(userNamesToSkip);

			ApplyToOtherUsersDialog aDlg = new ApplyToOtherUsersDialog(aMenuNode, currentCompanyId, currentUserId, currentConnection, (string[])userNamesToSkipInDlg.ToArray(typeof(string)));

			if (aDlg.ShowDialog(this) != DialogResult.OK)
				return;

			int[] checkedUserIds = aDlg.GetCheckedUserIds();
			if (checkedUserIds == null || checkedUserIds.Length == 0)
				return;

			this.Cursor = Cursors.WaitCursor;

			SecuredCommand currentSecuredCommand = (aMenuNode.IsCommand) ? new SecuredCommand(null, aMenuNode.ItemObject, SecuredCommand.GetSecuredCommandType(aMenuNode), currentConnection, userNamesToSkip) : null;
			bool isCommandDenied = (aMenuNode.IsCommand) ? currentSecuredCommand.IsAccessDenied(currentCompanyId, currentUserId) : false;
            bool isAccessToObjectInUnattendedModeAllowed = (isCommandDenied && currentSecuredCommand.IsAccessInUnattendedModeAllowed(currentCompanyId, currentUserId));

			foreach (int anotherUserId in checkedUserIds)
			{
                if (aMenuNode.IsCommand)
                {
                    SetSecuredCommandAccessRight(currentSecuredCommand, currentCompanyId, anotherUserId, isCommandDenied);
                    currentSecuredCommand.SetAccessInUnattendedMode(currentCompanyId, anotherUserId, isAccessToObjectInUnattendedModeAllowed);
                }
                else if (aMenuNode.IsGroup || aMenuNode.IsMenu)
                    ApplyCurrentAccessRightToAllCommandChildren(aMenuNode, currentCompanyId, anotherUserId, userNamesToSkip);
			}

            if (this.MenuMngControl != null && this.MenuMngControl.PathFinder != null && currentSecuredCommand != null)
                SecurityLightManager.RefreshDeniedAccessesInSLDenyFiles
                (
                    currentCompanyId,
                    currentSecuredCommand.Type,
                    this.MenuMngControl.PathFinder,
                    currentConnection,
                    userNamesToSkip
                );

			this.Cursor = Cursors.Default;
		}
        
		//--------------------------------------------------------------------------------------------------------
		private void ApplyObjectAccessRightToOtherCompanies(MenuXmlNode aMenuNode)
		{
			if (aMenuNode == null || !(aMenuNode.IsGroup || aMenuNode.IsMenu || aMenuNode.IsCommand))
				return;

			if (currentCompanyId == -1)
				return; // è selezionato <All Companies>

			ApplyToOtherCompaniesDialog aDlg = new ApplyToOtherCompaniesDialog(aMenuNode, currentCompanyId, currentUserId, currentConnection, new string[] { currentCompany } );

			if (aDlg.ShowDialog(this) != DialogResult.OK)
				return;

			int[] checkedCompanyIds = aDlg.GetCheckedCompanyIds();
			if (checkedCompanyIds == null || checkedCompanyIds.Length == 0)
				return;

			this.Cursor = Cursors.WaitCursor;
			
			string[] userNamesToSkip = GetUserNamesToSkip();
		
			SecuredCommand currentSecuredCommand = (aMenuNode.IsCommand) ? new SecuredCommand(null, aMenuNode.ItemObject, SecuredCommand.GetSecuredCommandType(aMenuNode), currentConnection, userNamesToSkip) : null;
			bool isCommandDenied = (aMenuNode.IsCommand) ? currentSecuredCommand.IsAccessDenied(currentCompanyId, currentUserId) : false;
            bool isAccessToObjectInUnattendedModeAllowed = (isCommandDenied && currentSecuredCommand.IsAccessInUnattendedModeAllowed(currentCompanyId, currentUserId));

			foreach (int anotherCompanyId in checkedCompanyIds)
			{
                if (aMenuNode.IsCommand)
                {
                    SetSecuredCommandAccessRight(currentSecuredCommand, anotherCompanyId, currentUserId, isCommandDenied);
                    currentSecuredCommand.SetAccessInUnattendedMode(anotherCompanyId, currentUserId, isAccessToObjectInUnattendedModeAllowed);

                    if (this.MenuMngControl != null && this.MenuMngControl.PathFinder != null)
                        SecurityLightManager.RefreshDeniedAccessesInSLDenyFiles
                        (
                            anotherCompanyId,
                            currentUserId,
                            currentSecuredCommand.Type,
                            this.MenuMngControl.PathFinder,
                            currentConnection,
                            userNamesToSkip
                        );
                }
                else if (aMenuNode.IsGroup || aMenuNode.IsMenu)
					ApplyCurrentAccessRightToAllCommandChildren(aMenuNode, anotherCompanyId, currentUserId, userNamesToSkip);
			}

			this.Cursor = Cursors.Default;
		}

		//---------------------------------------------------------------------
		public void SetMenuCommandCurrentAccessRight(MenuXmlNode aMenuNode, int aCompanyId, int aUserId, string[] aUserNamesToSkipList)
		{
			if (aMenuNode == null || !aMenuNode.IsCommand || (aCompanyId == currentCompanyId && aUserId == currentUserId))
				return;
		
			SecuredCommand currentSecuredCommand = new SecuredCommand(null, aMenuNode.ItemObject, SecuredCommand.GetSecuredCommandType(aMenuNode), currentConnection, aUserNamesToSkipList);

			SetSecuredCommandAccessRight(currentSecuredCommand, aCompanyId, aUserId, currentSecuredCommand.IsAccessDenied(currentCompanyId, currentUserId));
		}
		
		//---------------------------------------------------------------------
		public void SetSecuredCommandAccessRight(SecuredCommand aSecuredCommand, int aCompanyId, int aUserId, bool deny)
		{
			if (aSecuredCommand == null || (aCompanyId == currentCompanyId && aUserId == currentUserId))
				return;

			if (deny)
				aSecuredCommand.DenyAccess(aCompanyId, aUserId);
			else
				aSecuredCommand.AllowAccess(aCompanyId, aUserId);
		}
		
		//---------------------------------------------------------------------
		public void ApplyCurrentAccessRightToAllCommandChildren(MenuXmlNode aMenuNode, int aCompanyId, int aUserId, string[] aUserNamesToSkipList)
		{
			if (aMenuNode == null || !(aMenuNode.IsGroup || aMenuNode.IsMenu) || (aCompanyId == currentCompanyId && aUserId == currentUserId))
				return;

			ArrayList menuChildren = aMenuNode.MenuItems;
			if (menuChildren != null && menuChildren.Count > 0)
			{
				foreach (MenuXmlNode aChildMenuNode in menuChildren)
				{
					if (aChildMenuNode == null)
						continue;
					
					ApplyCurrentAccessRightToAllCommandChildren(aChildMenuNode, aCompanyId, aUserId, aUserNamesToSkipList);
				}
			}

			ArrayList commandChildren = (aMenuNode.IsMenu) ? aMenuNode.CommandItems : null;
			if (commandChildren != null && commandChildren.Count > 0)
			{
				foreach (MenuXmlNode aCommandNode in commandChildren)
				{
					if (aCommandNode == null)
						continue;

					SetMenuCommandCurrentAccessRight(aCommandNode, aCompanyId, aUserId, aUserNamesToSkipList);
				}
			}
		}
		
		#endregion // SecurityLightForm private methods

		#region SecurityLightForm public properties

        //--------------------------------------------------------------------------------------------------------------------------------
        public string CurrentConnectionString { get { return currentConnectionString; } }
        
        //--------------------------------------------------------------------------------------------------------------------------------
		public bool IsConnectionOpen { get{ return (currentConnection != null) && ((currentConnection.State & ConnectionState.Open) == ConnectionState.Open); } }
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsLoadingMenu { get{ return isLoadingMenu; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool EnableMenuLoad 
		{
			get{ return enableMenuLoad; }
			set
			{
				if (enableMenuLoad == value)
					return;
				enableMenuLoad = value;
				
				LoadCurrentUserMenu();
			}
		}

		#endregion // SecurityLightForm public properties
	
		#region SecurityLightForm public methods

		//--------------------------------------------------------------------------------------------------------
		public void ClearInfo()
		{
			CloseConnection();
		
			if (this.MenuMngControl != null)
			{
				this.MenuMngControl.MenuXmlParser = null;
				this.MenuMngControl.PathFinder = null;
			}

			currentMenuLoader = null;
			
			if (loadedMenus != null)
				loadedMenus.Clear();

			loadedMenus = null;

			this.CompaniesComboBox.Items.Clear();
			this.UsersComboBox.Items.Clear();

			currentUser = String.Empty;
			currentCompany = String.Empty;
			currentCompanyId = -1;
		}
		
		//--------------------------------------------------------------------------------------------------------
		public void UpdateConnectionData(string serverInstance)
		{
			if 
				(
				consoleEnvironmentInfo == null || 
				consoleEnvironmentInfo.ConsoleUserInfo == null ||
				consoleEnvironmentInfo.ConsoleUserInfo.ServerName == null ||
				consoleEnvironmentInfo.ConsoleUserInfo.ServerName.Length == 0 ||
				consoleEnvironmentInfo.ConsoleUserInfo.DbName == null ||
				consoleEnvironmentInfo.ConsoleUserInfo.DbName.Length == 0 ||
				(!consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth && (consoleEnvironmentInfo.ConsoleUserInfo.UserName == null || consoleEnvironmentInfo.ConsoleUserInfo.UserName.Length == 0))
				)
			{
				ClearInfo();

				return;
			}

			string server = consoleEnvironmentInfo.ConsoleUserInfo.ServerName;
			if (serverInstance != null && serverInstance != String.Empty)
				server += Path.DirectorySeparatorChar + serverInstance;
			
			string newConnectionString = SQL_CONNECTION_STRING_SERVER_KEYWORD + "=" + server + ";";
			newConnectionString +=  SQL_CONNECTION_STRING_DATABASE_KEYWORD + "=" + consoleEnvironmentInfo.ConsoleUserInfo.DbName + ";";

			// If we are using Windows authentication when connecting to SQL Server, we avoid embedding user
			// names and passwords in the connection string.
			// We use the Integrated Security keyword, set to a value of SSPI, to specify Windows Authentication:
			if (consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth)
			{
				newConnectionString +=  SQL_CONNECTION_STRING_INTEGRATED_SECURITY_TOKEN;// the connection should use Windows integrated security (NT authentication)
				newConnectionString +=  "=";
				newConnectionString +=  SQL_CONNECTION_STRING_SECURITY_SUPPORT_PROVIDER_INTERFACE;
				newConnectionString +=  ";";
			}
			else
			{
				newConnectionString +=  SQL_CONNECTION_STRING_LOGIN_ACCOUNT_KEYWORD + "=" + consoleEnvironmentInfo.ConsoleUserInfo.UserName;
				if (consoleEnvironmentInfo.ConsoleUserInfo.UserPwd != null && consoleEnvironmentInfo.ConsoleUserInfo.UserPwd != String.Empty)
					newConnectionString +=  ";" + SQL_CONNECTION_STRING_LOGIN_PASSWORD_KEYWORD + "=" + consoleEnvironmentInfo.ConsoleUserInfo.UserPwd;
				newConnectionString +=  ";";
			}

			if (String.Compare(newConnectionString, currentConnectionString) != 0)
			{
				CloseConnection();
				currentConnectionString = newConnectionString;
			}

			OpenConnection();
		
			FillUsersComboBox();
		}

		//--------------------------------------------------------------------------------------------------------
		public void UpdateConnectionData()
		{
			UpdateConnectionData(String.Empty);
		}
		
		//--------------------------------------------------------------------------------------------------------
		public string[] GetUserNamesToSkip()
		{
			ArrayList userNamesToSkip = new ArrayList();
			userNamesToSkip.Add(NameSolverStrings.EasyLookSystemLogin);
			userNamesToSkip.Add(NameSolverStrings.GuestLogin);
			return (string[])userNamesToSkip.ToArray(typeof(string));
		}

		//--------------------------------------------------------------------------------------------------------
		public void RefreshUsersAndCompanies()
		{
			FillUsersComboBox();
		}
		
		//--------------------------------------------------------------------------------------------------------
		public void DeleteCompanyAccessRights(int aCompanyId)
		{
            SecurityLightManager.DeleteCompanyAccessRights(aCompanyId, currentConnection);
		}
		
		//--------------------------------------------------------------------------------------------------------
		public void DeleteUserAccessRights(int aUserId)
		{
            SecurityLightManager.DeleteUserAccessRights(aUserId, currentConnection);
		}
		
		//--------------------------------------------------------------------------------------------------------
		public void DeleteCompanyUserAccessRights(int aCompanyId, int aUserId)
		{
            SecurityLightManager.DeleteCompanyUserAccessRights(aCompanyId, aUserId, currentConnection);
		}
		
		#endregion // SecurityLightForm public methods
	}
}
