using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.SecurityLayer.SecurityLightObjects;
using Microarea.TaskBuilderNet.UI.WinControls.Lists;

namespace Microarea.Console.Plugin.SecurityLight
{
	/// <summary>
	/// Summary description for AccessRightsOverviewDialog.
	/// </summary>
	public partial class AccessRightsOverviewDialog : System.Windows.Forms.Form
	{
		#region AccessRightsOverviewDialog private data members
		
		private MenuXmlNode menuNode = null;
		private SqlConnection systemDBConnection = null;
		private string[] userNamesToSkip = null;
		private bool accessRightsTreeViewFilled = false;
		private bool accessRightsTreeViewFilling = false;
		private bool stopAccessRightsTreeViewFilling = false;

		private int userImageIndex = -1;
		private int companyImageIndex = -1;
		private int menuItemImageIndex = -1;
		private int formImageIndex = -1;
		private int batchImageIndex = -1;
		private int reportImageIndex = -1;
		private int functionImageIndex = -1;
		private int excelDocumentImageIndex = -1;
		private int excelTemplateImageIndex = -1;
		private int wordDocumentImageIndex = -1;
		private int wordTemplateImageIndex = -1;

		private int objectDeniedStateImageIndex = -1;
		private int objectPartiallyDeniedStateImageIndex = -1;
		private int objectAllowedStateImageIndex = -1;
        private int objectUnattendedAllowedStateImageIndex = -1;

		#endregion // AccessRightsOverviewDialog private data members

		//---------------------------------------------------------------------
		// Constructor
		//---------------------------------------------------------------------
		public AccessRightsOverviewDialog(MenuXmlNode aMenuNode, SqlConnection aConnection, string[] aUserNamesToSkipList)
		{
			if (aMenuNode == null)
				throw new ArgumentNullException("Invalid menu node passed as argument to the AccessRightsOverviewDialog constructor.");

			if 
				(
				!(aMenuNode.IsGroup || aMenuNode.IsMenu || aMenuNode.IsCommand) ||
				(aMenuNode.IsCommand && !SecuredCommand.IsDeniableCommand(aMenuNode))
				)
				throw new ArgumentException("Invalid menu node passed as argument to the AccessRightsOverviewDialog constructor.");

			if (aConnection == null || aConnection.State != ConnectionState.Open)
				throw new ArgumentException("Invalid database connection passed as argument to the AccessRightsOverviewDialog constructor.");
			
			menuNode = aMenuNode;
			systemDBConnection = aConnection;
			userNamesToSkip = aUserNamesToSkipList;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.ShowAccessRights.gif");
			if (imageStream != null)
				this.ShowAccessRightsPictureBox.Image = Image.FromStream(imageStream);
			
			if (menuNode.IsGroup)
			{
				this.ObjectTypeLabel.Text = Strings.GroupObjectDescriptionText;		
				this.NamespaceCaptionLabel.Text = Strings.GroupTitleCaptionText;
				this.NamespaceLabel.Text = menuNode.Title;
			}
			else if (menuNode.IsMenu)
			{
				this.ObjectTypeLabel.Text = Strings.MenuObjectDescriptionText;
				this.NamespaceCaptionLabel.Text = Strings.MenuTitleCaptionText;
				this.NamespaceLabel.Text = menuNode.Title;
			}
			else if (menuNode.IsCommand)
			{
				this.ObjectTypeCaptionLabel.Text = Strings.CommandObjectTypeCaptionText;
				if (menuNode.IsRunDocument)
				{
					this.ObjectTypeLabel.Text = Strings.FormObjectDescriptionText;

					Stream commandImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.RunDocument.gif");
					if (commandImageStream != null)
						this.ObjectTypeLabel.Image = Image.FromStream(commandImageStream);
				}
				else if (menuNode.IsRunBatch)
				{
					this.ObjectTypeLabel.Text = Strings.BatchObjectDescriptionText;

					Stream commandImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.RunBatch.gif");
					if (commandImageStream != null)
						this.ObjectTypeLabel.Image = Image.FromStream(commandImageStream);
				}
				else if (menuNode.IsRunReport)
				{
					this.ObjectTypeLabel.Text = Strings.ReportObjectDescriptionText;

					Stream commandImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.RunReport.gif");
					if (commandImageStream != null)
						this.ObjectTypeLabel.Image = Image.FromStream(commandImageStream);
				}
				else if (menuNode.IsRunFunction)
				{
					this.ObjectTypeLabel.Text = Strings.FunctionObjectDescriptionText;

					Stream commandImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.RunFunction.gif");
					if (commandImageStream != null)
						this.ObjectTypeLabel.Image = Image.FromStream(commandImageStream);
				}

                else if (menuNode.IsExcelDocument || menuNode.IsExcelDocument2007)
				{
					this.ObjectTypeLabel.Text = Strings.ExcelDocumentObjectDescriptionText;

					Stream commandImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.ExcelDocument.gif");
					if (commandImageStream != null)
						this.ObjectTypeLabel.Image = Image.FromStream(commandImageStream);
				}
                else if (menuNode.IsExcelTemplate || menuNode.IsExcelTemplate2007)
				{
					this.ObjectTypeLabel.Text = Strings.ExcelTemplateObjectDescriptionText;

					Stream commandImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.ExcelTemplate.gif");
					if (commandImageStream != null)
						this.ObjectTypeLabel.Image = Image.FromStream(commandImageStream);
				}
                else if (menuNode.IsWordDocument || menuNode.IsWordDocument2007)
				{
					this.ObjectTypeLabel.Text = Strings.WordDocumentObjectDescriptionText;

					Stream commandImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.WordDocument.gif");
					if (commandImageStream != null)
						this.ObjectTypeLabel.Image = Image.FromStream(commandImageStream);
				}
                else if (menuNode.IsWordTemplate || menuNode.IsWordTemplate2007)
				{
					this.ObjectTypeLabel.Text = Strings.WordTemplateObjectDescriptionText;

					Stream commandImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.WordTemplate.gif");
					if (commandImageStream != null)
						this.ObjectTypeLabel.Image = Image.FromStream(commandImageStream);
				}
				
				this.NamespaceLabel.Text = menuNode.ItemObject;
			}

			Stream treeViewImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.TreeView.User.gif");
			if (treeViewImageStream != null)
				userImageIndex = this.AccessRightsTreeView.AddImageToNodesImageList(Image.FromStream(treeViewImageStream));

			treeViewImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.TreeView.Company.gif");
			if (treeViewImageStream != null)
				companyImageIndex = this.AccessRightsTreeView.AddImageToNodesImageList(Image.FromStream(treeViewImageStream));

			treeViewImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.TreeView.MenuItem.gif");
			if (treeViewImageStream != null)
				menuItemImageIndex = this.AccessRightsTreeView.AddImageToNodesImageList(Image.FromStream(treeViewImageStream));

			treeViewImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.TreeView.Form.gif");
			if (treeViewImageStream != null)
				formImageIndex = this.AccessRightsTreeView.AddImageToNodesImageList(Image.FromStream(treeViewImageStream));

			treeViewImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.TreeView.Batch.gif");
			if (treeViewImageStream != null)
				batchImageIndex = this.AccessRightsTreeView.AddImageToNodesImageList(Image.FromStream(treeViewImageStream));

			treeViewImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.TreeView.Report.gif");
			if (treeViewImageStream != null)
				reportImageIndex = this.AccessRightsTreeView.AddImageToNodesImageList(Image.FromStream(treeViewImageStream));

			treeViewImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.TreeView.Function.gif");
			if (treeViewImageStream != null)
				functionImageIndex = this.AccessRightsTreeView.AddImageToNodesImageList(Image.FromStream(treeViewImageStream));

			treeViewImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.TreeView.ExcelDocument.gif");
			if (treeViewImageStream != null)
				excelDocumentImageIndex = this.AccessRightsTreeView.AddImageToNodesImageList(Image.FromStream(treeViewImageStream));

			treeViewImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.TreeView.ExcelTemplate.gif");
			if (treeViewImageStream != null)
				excelTemplateImageIndex = this.AccessRightsTreeView.AddImageToNodesImageList(Image.FromStream(treeViewImageStream));

			treeViewImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.TreeView.WordDocument.gif");
			if (treeViewImageStream != null)
				wordDocumentImageIndex = this.AccessRightsTreeView.AddImageToNodesImageList(Image.FromStream(treeViewImageStream));

			treeViewImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.TreeView.WordTemplate.gif");
			if (treeViewImageStream != null)
				wordTemplateImageIndex = this.AccessRightsTreeView.AddImageToNodesImageList(Image.FromStream(treeViewImageStream));


			Stream stateImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.DeniedObject.gif");
			if (stateImageStream != null)
				objectDeniedStateImageIndex = this.AccessRightsTreeView.AddImageToStateImageList(Image.FromStream(stateImageStream));
			stateImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.PartiallyDeniedObject.gif");
			if (stateImageStream != null)
				objectPartiallyDeniedStateImageIndex = this.AccessRightsTreeView.AddImageToStateImageList(Image.FromStream(stateImageStream));
			stateImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.AllowedObject.gif");
			if (stateImageStream != null)
				objectAllowedStateImageIndex = this.AccessRightsTreeView.AddImageToStateImageList(Image.FromStream(stateImageStream));
			stateImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".UnattendedAllowed.gif");
            if (stateImageStream != null)
                objectUnattendedAllowedStateImageIndex = this.AccessRightsTreeView.AddImageToStateImageList(Image.FromStream(stateImageStream));
	
			WorkInProgressLabel.Visible = false;
		}

		//--------------------------------------------------------------------------------------------------------
		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated (e);

			if (!accessRightsTreeViewFilled)
				FillAccessRightsTreeView();
		}

		//--------------------------------------------------------------------------------------------------------
		private void FillAccessRightsTreeView()
		{
			if 
				(
				accessRightsTreeViewFilled || 
				accessRightsTreeViewFilling ||
				menuNode == null ||
				systemDBConnection == null || 
				systemDBConnection.State != ConnectionState.Open
				)
				return;

			this.Refresh();

			Application.DoEvents();

			accessRightsTreeViewFilling = true;
			stopAccessRightsTreeViewFilling = false;

			this.AccessRightsTreeView.Cursor = Cursors.WaitCursor;
			this.AccessRightsTreeView.Enabled = false;

			WorkInProgressLabel.Visible = true;

			StartLookForAccessRightsAnimation();

            SecurityLightManager.User[] users = SecurityLightManager.GetAllUsers(systemDBConnection, userNamesToSkip);
			if (users != null && users.Length > 0)
			{
				Application.DoEvents();  // per consentire all'animazione di andare avanti!!!
				
				SecuredCommandType commandType = menuNode.IsCommand ? SecuredCommand.GetSecuredCommandType(menuNode) : SecuredCommandType.Undefined;

                foreach (SecurityLightManager.User aUser in users)
				{
					if (aUser == null || aUser.Id == -1)
						continue;

                    SecurityLightManager.Company[] userCompanies = SecurityLightManager.GetUserCompanies(aUser.Id, systemDBConnection);
					if (userCompanies == null || userCompanies.Length == 0)
						continue;
					
					StateTreeNode userTreeNode = new StateTreeNode(aUser.Name);
					userTreeNode.Tag = aUser.Id;
					userTreeNode.ImageIndex = userTreeNode.SelectedImageIndex = userImageIndex;

                    foreach (SecurityLightManager.Company aUserCompany in userCompanies)
					{
						StateTreeNode userCompanyTreeNode = new StateTreeNode(aUserCompany.Name);
						userCompanyTreeNode.ImageIndex = userCompanyTreeNode.SelectedImageIndex = companyImageIndex;

                        SecurityLightManager.ChildrenProtectionType userCompanyProtectionType = SecurityLightManager.ChildrenProtectionType.None;
						if (menuNode.IsCommand)
						{
							bool isCommandDenied = SecuredCommand.IsAccessToObjectDenied(menuNode.ItemObject, commandType, aUserCompany.Id, aUser.Id, systemDBConnection);
                            userCompanyProtectionType = isCommandDenied ? SecurityLightManager.ChildrenProtectionType.All : SecurityLightManager.ChildrenProtectionType.None;
						}
						else
						{
							AppendMenuNodes(userCompanyTreeNode, menuNode, aUser.Id, aUserCompany.Id);
							userCompanyProtectionType = GetChildrenTreeNodesProtectionType(userCompanyTreeNode);
						}
                        if (userCompanyProtectionType == SecurityLightManager.ChildrenProtectionType.All)
                        {
                            if (menuNode.IsCommand && SecuredCommand.IsAccessToObjectInUnattendedModeAllowed(menuNode.ItemObject, commandType, aUserCompany.Id, aUser.Id, systemDBConnection))
                                userCompanyTreeNode.StateImageIndex = objectUnattendedAllowedStateImageIndex;
                            else
                                userCompanyTreeNode.StateImageIndex = objectDeniedStateImageIndex;
                        }
                        else if (userCompanyProtectionType == SecurityLightManager.ChildrenProtectionType.Partial)
							userCompanyTreeNode.StateImageIndex = objectPartiallyDeniedStateImageIndex;
                        else if (userCompanyProtectionType == SecurityLightManager.ChildrenProtectionType.None)
							userCompanyTreeNode.StateImageIndex = objectAllowedStateImageIndex;
					
						userCompanyTreeNode.Tag = userCompanyProtectionType;

						userTreeNode.Nodes.Add(userCompanyTreeNode);
					
						Application.DoEvents();  // per consentire all'animazione di andare avanti!!!
		
						if (stopAccessRightsTreeViewFilling)
							break;
					}

                    SecurityLightManager.ChildrenProtectionType userProtectionType = GetChildrenTreeNodesProtectionType(userTreeNode);

                    if (userProtectionType == SecurityLightManager.ChildrenProtectionType.All)
						userTreeNode.StateImageIndex = objectDeniedStateImageIndex;
                    else if (userProtectionType == SecurityLightManager.ChildrenProtectionType.Partial)
						userTreeNode.StateImageIndex = objectPartiallyDeniedStateImageIndex;
                    else if (userProtectionType == SecurityLightManager.ChildrenProtectionType.None)
						userTreeNode.StateImageIndex = objectAllowedStateImageIndex;
					
					this.AccessRightsTreeView.Nodes.Add(userTreeNode);
				
					Application.DoEvents();  // per consentire all'animazione di andare avanti!!!
					if (stopAccessRightsTreeViewFilling)
						break;
				}
			}

			StopLookForAccessRightsAnimation();

			WorkInProgressLabel.Visible = false;

			accessRightsTreeViewFilling = false;
			accessRightsTreeViewFilled = true;

			this.AccessRightsTreeView.Enabled = true;
			this.AccessRightsTreeView.Cursor = Cursors.Default;
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void AppendMenuNodes(StateTreeNode aTreeNode, MenuXmlNode aMenuNode, int aUserId, int aCompanyId)
		{
			if (aTreeNode == null || aMenuNode == null || !(aMenuNode.IsGroup || aMenuNode.IsMenu))
				return;

			ArrayList commandChildren = aMenuNode.CommandItems;
			if (commandChildren != null && commandChildren.Count > 0)
			{
				foreach (MenuXmlNode aCommandNode in commandChildren)
				{
					if (aCommandNode == null)
						continue;
					StateTreeNode commandTreeNode = new StateTreeNode(aCommandNode.Title);

					Application.DoEvents(); // per consentire all'animazione di andare avanti!!!

					SecuredCommandType commandType = SecuredCommand.GetSecuredCommandType(aCommandNode);
					if (commandType == SecuredCommandType.Undefined)
						continue;

					if (commandType == SecuredCommandType.Form)
						commandTreeNode.ImageIndex = commandTreeNode.SelectedImageIndex = formImageIndex;
					else if (commandType == SecuredCommandType.Batch)
						commandTreeNode.ImageIndex = commandTreeNode.SelectedImageIndex = batchImageIndex;
					else if (commandType == SecuredCommandType.Report)
						commandTreeNode.ImageIndex = commandTreeNode.SelectedImageIndex = reportImageIndex;
					else if (commandType == SecuredCommandType.Function)
						commandTreeNode.ImageIndex = commandTreeNode.SelectedImageIndex = functionImageIndex;
					else if (commandType == SecuredCommandType.ExcelDocument)
						commandTreeNode.ImageIndex = commandTreeNode.SelectedImageIndex = excelDocumentImageIndex;
					else if (commandType == SecuredCommandType.ExcelTemplate)
						commandTreeNode.ImageIndex = commandTreeNode.SelectedImageIndex = excelTemplateImageIndex;
					else if (commandType == SecuredCommandType.WordDocument)
						commandTreeNode.ImageIndex = commandTreeNode.SelectedImageIndex = wordDocumentImageIndex;
					else if (commandType == SecuredCommandType.WordTemplate)
						commandTreeNode.ImageIndex = commandTreeNode.SelectedImageIndex = wordTemplateImageIndex;

					bool isCommandDenied = SecuredCommand.IsAccessToObjectDenied(aCommandNode.ItemObject, commandType, aCompanyId, aUserId, systemDBConnection);

                    if (isCommandDenied)
                    {
                        if (SecuredCommand.IsAccessToObjectInUnattendedModeAllowed(aCommandNode.ItemObject, commandType, aCompanyId, aUserId, systemDBConnection))
                            commandTreeNode.StateImageIndex = objectUnattendedAllowedStateImageIndex;
                        else
                            commandTreeNode.StateImageIndex = objectDeniedStateImageIndex;
                    }
                    else
                        commandTreeNode.StateImageIndex = objectAllowedStateImageIndex;

                    commandTreeNode.Tag = isCommandDenied ? SecurityLightManager.ChildrenProtectionType.All : SecurityLightManager.ChildrenProtectionType.None;

					aTreeNode.Nodes.Add(commandTreeNode);
				
					Application.DoEvents(); // per consentire all'animazione di andare avanti!!!
				
					if (stopAccessRightsTreeViewFilling)
						return;
				}
			}
			
			ArrayList menuChildren = aMenuNode.MenuItems;
			if (menuChildren != null && menuChildren.Count > 0)
			{
				foreach (MenuXmlNode aChildMenuNode in menuChildren)
				{
					if (aChildMenuNode == null)
						continue;

					StateTreeNode childMenuTreeNode = new StateTreeNode(aChildMenuNode.Title);
					childMenuTreeNode.ImageIndex = childMenuTreeNode.SelectedImageIndex = menuItemImageIndex;

					Application.DoEvents(); // per consentire all'animazione di andare avanti!!!

					AppendMenuNodes(childMenuTreeNode, aChildMenuNode, aUserId, aCompanyId);

                    SecurityLightManager.ChildrenProtectionType childMenuProtectionType = GetChildrenTreeNodesProtectionType(childMenuTreeNode);

					childMenuTreeNode.Tag = childMenuProtectionType;

                    if (childMenuProtectionType == SecurityLightManager.ChildrenProtectionType.All)
						childMenuTreeNode.StateImageIndex = objectDeniedStateImageIndex;
                    else if (childMenuProtectionType == SecurityLightManager.ChildrenProtectionType.Partial)
						childMenuTreeNode.StateImageIndex = objectPartiallyDeniedStateImageIndex;
                    else if (childMenuProtectionType == SecurityLightManager.ChildrenProtectionType.None)
						childMenuTreeNode.StateImageIndex = objectAllowedStateImageIndex;

					aTreeNode.Nodes.Add(childMenuTreeNode);
		
					Application.DoEvents(); // per consentire all'animazione di andare avanti!!!
		
					if (stopAccessRightsTreeViewFilling)
						return;
				}
			}

		}

		//--------------------------------------------------------------------------------------------------------
        private SecurityLightManager.ChildrenProtectionType GetChildrenTreeNodesProtectionType(StateTreeNode aTreeNode)
		{
			if (aTreeNode == null || aTreeNode.NodesCount == 0)
                return SecurityLightManager.ChildrenProtectionType.None;

			bool areAllChildMenusFullDenied = true;
			bool areAllChildMenusFullAllowed = true;
					
			foreach (StateTreeNode aDescendantNode in aTreeNode.Nodes)
			{
                if (aDescendantNode.Tag == null || !(aDescendantNode.Tag is SecurityLightManager.ChildrenProtectionType))
					continue;

                if (((SecurityLightManager.ChildrenProtectionType)aDescendantNode.Tag) == SecurityLightManager.ChildrenProtectionType.Partial)
                    return SecurityLightManager.ChildrenProtectionType.Partial;

                if (((SecurityLightManager.ChildrenProtectionType)aDescendantNode.Tag) == SecurityLightManager.ChildrenProtectionType.None)
					areAllChildMenusFullDenied = false;
                else if (((SecurityLightManager.ChildrenProtectionType)aDescendantNode.Tag) == SecurityLightManager.ChildrenProtectionType.All)
					areAllChildMenusFullAllowed = false;
				if (!areAllChildMenusFullDenied && !areAllChildMenusFullAllowed)
                    return SecurityLightManager.ChildrenProtectionType.Partial;
			}

			if (areAllChildMenusFullDenied)
                return SecurityLightManager.ChildrenProtectionType.All;

            return SecurityLightManager.ChildrenProtectionType.None;
		}

		//--------------------------------------------------------------------------------------------------------
		private void StartLookForAccessRightsAnimation()
		{
			if (this.LookForAccessRightsAnimationPanel == null || this.LookForAccessRightsAnimationPanel.AnimationInProgress)
				return;

			Stream animatedEyeImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.LookForAccessRights.gif");
			if (animatedEyeImageStream != null)
			{
				this.LookForAccessRightsAnimationPanel.AnimatedImage = Image.FromStream(animatedEyeImageStream);
				this.LookForAccessRightsAnimationPanel.StartAnimation();
				this.LookForAccessRightsAnimationPanel.BringToFront();
			}
			this.LookForAccessRightsAnimationPanel.Visible = (this.LookForAccessRightsAnimationPanel.AnimatedImage != null);
		
			Application.DoEvents();  // per consentire all'animazione di andare avanti!!!
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void StopLookForAccessRightsAnimation()
		{
			this.LookForAccessRightsAnimationPanel.SendToBack();
			this.LookForAccessRightsAnimationPanel.StopAnimation();
			this.LookForAccessRightsAnimationPanel.Visible = false;
		
			Application.DoEvents();
		}

		//--------------------------------------------------------------------------------------------------------
		private void CancelDlgButton_Click(object sender, System.EventArgs e)
		{
			if (accessRightsTreeViewFilling)
			{
				if (MessageBox.Show
					(
					this, 
					Strings.StopCurrentOperationQuestionText, 
					Strings.StopLoadingCaption, 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question
					) == DialogResult.Yes
					)
				{
					stopAccessRightsTreeViewFilling = true;
					this.LookForAccessRightsAnimationPanel.Visible = false;
				}
				else
				{
					this.DialogResult = DialogResult.None;
					return;
				}
			}
		
		}
	}
}
