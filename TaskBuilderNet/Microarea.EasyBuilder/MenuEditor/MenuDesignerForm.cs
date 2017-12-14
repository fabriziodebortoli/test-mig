using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WinControls.Dock;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	/// <remarks/>
	public partial class MenuDesignerForm : DockContent, IDirtyManager
    {
		private TBDockContent<MainToolbar> mainToolbar;

		private MenuModel currentMenuModel;
        private XmlDocument menuXmlDocument;
		private IMenuDomUpdater menuDomUpdater;

		private ImagesManager imagesManager = new ImagesManager();
		
		private bool isDirty;
		private bool suspendDirtyChanges;
		private bool atLeastOnModificationOccurred;
		/// <summary>
		/// Occurs when the dirty flag is set
		/// </summary>
		public event EventHandler<DirtyChangedEventArgs> DirtyChanged;

		private MenuDesignUIItem lastOperationCutItem;
		private string currentUser;

		//---------------------------------------------------------------------
		/// <remarks />
		public ISelectionService SelectionService { get { return this.MenuDesignerCtrl.SelectionService; } }

		//---------------------------------------------------------------------
		/// <remarks />
		public IUIService UIService { get { return this.MenuDesignerCtrl.UiService; } }

		//----------------------------------------------------------------------------------------------
		/// <remarks />
		[Browsable(false)]
		public bool IsDirty
		{
			get { return isDirty; }
			set
			{
				if (suspendDirtyChanges)
					return;

				if (value == isDirty)
					return;

				isDirty = value;
				OnDirtyChanged();
				StringBuilder formTextBuilder = new StringBuilder(
					Path.GetFileName(this.MenuDesignerCtrl.MenuFile)
					).Append(
					MenuEditorStrings.MenuDesignerTitlePostfix
				);

				if (isDirty)
				{
					formTextBuilder.Append("*");
					atLeastOnModificationOccurred = true;
				}

				this.Text = formTextBuilder.ToString();
			}
		}

		//---------------------------------------------------------------------
		/// <remarks />
		[Browsable(false)]
		public bool AtLeastOnModificationOccurred
		{
			get { return atLeastOnModificationOccurred; }
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the current user connected to the program.
		/// </summary>
		[Browsable(false)]
		public string CurrentUser
		{
			get { return currentUser; }
			set { currentUser = value; }
		}

		//---------------------------------------------------------------------
		private void OnDirtyChanged()
		{
			if (DirtyChanged != null)
				DirtyChanged(this, new DirtyChangedEventArgs(false));
		}

		//---------------------------------------------------------------------
		/// <remarks />
		public MenuDesignerForm(XmlDocument fullMenuXmlDoc, string userName)
        {
			if (fullMenuXmlDoc == null)
				throw new ArgumentException("fullMenuXmlDoc is empty");

			if (userName.IsNullOrEmpty())
				throw new ArgumentException("userName is null or empty");

			this.currentUser = userName;

            InitializeComponent();

			this.MenuDesignerCtrl.SelectionService = new EasyBuilderSelectionService();
			this.MenuDesignerCtrl.UiService = new FormEditorUIService(this);

			this.InsertApplicationToolStripMenuItem.Text = string.Format
				(
				MenuEditorStrings.CreateApplication, 
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName
				);

			OpenMenuDesignerForm(fullMenuXmlDoc);
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			CreateMainToolbar(((DockContent)this).DockPanel);
		}

		//-----------------------------------------------------------------------------
		private void CreateMainToolbar(DockPanel hostingPanel)
		{
			this.mainToolbar = TBDockContent<MainToolbar>.CreateDockablePane
								(
									hostingPanel,
									WeifenLuo.WinFormsUI.Docking.DockState.DockTop,
									Resources.EasyBuilderIcon,
									hostingPanel,
									this
								);

			mainToolbar.FormClosing += new FormClosingEventHandler(MainToolbar_FormClosing);
			mainToolbar.FormClosed += new FormClosedEventHandler(MainToolbar_FormClosed);
		}

		//-----------------------------------------------------------------------------
		void MainToolbar_FormClosing(object sender, FormClosingEventArgs e)
		{
			AskAndSaveAndClose(e);
		}
		//-----------------------------------------------------------------------------
		void MainToolbar_FormClosed(object sender, FormClosedEventArgs e)
		{
			Close();
		}

		//---------------------------------------------------------------------
		/// <remarks/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            Point mousePosition = Control.MousePosition;
				
			if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.None)
			{
				if (!this.MenuDesignerCtrl.DesignSelectedItem.MenuItem.CanBeCustomized)
					return;

				DeleteMenuDesignUIItem(this.MenuDesignerCtrl.DesignSelectedItem);
				e.Handled = true;
				return;
			}

			// CTRL + C or CTRL + INSERT ===> Copy: Copies the selected item to the Clipboard.
			if ((e.KeyCode == Keys.C && e.Control) || (e.KeyCode == Keys.Insert && e.Control))
			{
				CopyToolStripMenuItem.Tag = FindMenuItem(mousePosition);

				CopyToolStripMenuItem_Click(this, EventArgs.Empty);

				e.Handled = true;
				return;
			}
		
			// CTRL + X or SHIFT + DELETE ===> Cut: Deletes the selected item from the menu and copies it to the Clipboard.
			if ((e.KeyCode == Keys.X && e.Control) || (e.KeyCode == Keys.Delete && e.Shift))
			{
				MenuDesignUIItem menuItem = FindMenuItem(mousePosition);
				if (menuItem == null || menuItem.MenuItem == null || !menuItem.MenuItem.CanBeCustomized)
					return;

				CutToolStripMenuItem.Tag = menuItem;

				CutToolStripMenuItem_Click(this, EventArgs.Empty);
				
				e.Handled = true;
				return;
			}

			// CTRL + V or SHIFT + INSERT ===> Paste: Inserts the Clipboard contents at the menu insertion point.
			if ((e.KeyCode == Keys.V && e.Control) || (e.KeyCode == Keys.Insert && e.Shift))
			{
				MenuDesignUIItem menuItem = FindMenuItemContainer(mousePosition);
				if (menuItem == null ||  menuItem.MenuItem == null || !menuItem.MenuItem.CanBeCustomized)
					return;

				PasteToolStripMenuItem.Tag = menuItem;

				PasteToolStripMenuItem_Click(this, EventArgs.Empty);

				e.Handled = true;
				return;
			}

			base.OnKeyDown(e);
        }

		//---------------------------------------------------------------------
		private static bool SetClipboardData(object dataToCopy)
		{
			try
			{
				Clipboard.Clear();

				if (dataToCopy == null || !(dataToCopy is BaseMenuItem || dataToCopy is MenuCommand))
					return false;

				DataFormats.Format format = DataFormats.GetFormat(dataToCopy.GetType().FullName);

				IDataObject dataObj = new DataObject();
				dataObj.SetData(format.Name, false, dataToCopy);

				// Places nonpersistent data on the system Clipboard.
				Clipboard.SetDataObject(dataObj);

				return true;
			}
			catch
			{
				return false;
			}
		}

		//---------------------------------------------------------------------
		private static object GetClipboardData(Type aDataType, bool autoConvert = false)
		{
			if (aDataType == null)
				return null;

			try
			{
				string format = aDataType.FullName;
				IDataObject data = Clipboard.GetDataObject();
				if (data == null || !data.GetDataPresent(format, autoConvert))
					return null;

				return data.GetData(format, autoConvert);
			}
			catch
			{
				return null;
			}
		}

		//---------------------------------------------------------------------
		internal static bool CopyToClipboard(MenuDesignUIItem aMenuDesignUIItem)
		{
			if (aMenuDesignUIItem == null || aMenuDesignUIItem.MenuItem == null)
				return false;

			object dataToSet = aMenuDesignUIItem.MenuItem;

			string reflectionMethodname = ReflectionUtils.GetMethodName((BaseMenuItem b) => b.Clone());
			MethodInfo cloneMethod = dataToSet.GetType().GetMethod(reflectionMethodname, new Type[0]);
			if (cloneMethod == null || !cloneMethod.IsPublic)
				return false;

			dataToSet = cloneMethod.Invoke(dataToSet, null);

			return SetClipboardData(dataToSet);
		}

		//---------------------------------------------------------------------
		internal bool PasteFromClipboard(BaseMenuItem itemToPaste,  MenuDesignUIItem aMenuDesignUIItem)
		{
			MenuApplication applicationToPaste = itemToPaste as MenuApplication;
			if (applicationToPaste != null)
			{
				CollapsiblePanel applicationPanelBefore = null;
				if (aMenuDesignUIItem != null && aMenuDesignUIItem.Application != null)
					applicationPanelBefore = aMenuDesignUIItem.Control as CollapsiblePanel;

				CollapsiblePanel newApplicationPanel =
					this.MenuDesignerCtrl.InsertApplicationPanel(applicationToPaste, applicationPanelBefore);

				if (newApplicationPanel != null)
				{
					newApplicationPanel.IsDesignSelected = true;

					newApplicationPanel.Focus();
				}

				return (newApplicationPanel != null);
			}

			MenuGroup groupToPaste = itemToPaste as MenuGroup;
			if (groupToPaste != null)
			{
				CollapsiblePanelLinkLabel groupLinkLabelBefore = null;
				CollapsiblePanel applicationPanel = null;
				if (aMenuDesignUIItem != null)
				{
					if
						(
						aMenuDesignUIItem.Group != null &&
						aMenuDesignUIItem.Control.Parent != null &&
						aMenuDesignUIItem.Control.Parent is CollapsiblePanel
						)
					{
						groupLinkLabelBefore = aMenuDesignUIItem.Control as CollapsiblePanelLinkLabel;
						applicationPanel = aMenuDesignUIItem.Control.Parent as CollapsiblePanel;
					}
					else if (aMenuDesignUIItem.Application != null)
					{
						applicationPanel = aMenuDesignUIItem.Control as CollapsiblePanel;
						if (applicationPanel != null && !applicationPanel.IsExpanded)
						{
							applicationPanel.Expand();
							Application.DoEvents();
						}
					}
					else
						return false; // non sono su un pannello di applicazione, nè su una label di gruppo!!!
				}

                
				CollapsiblePanelLinkLabel newGroupLinkLabel = this.MenuDesignerCtrl.InsertGroupLinkLabel(groupToPaste, applicationPanel, groupLinkLabelBefore);

				if (newGroupLinkLabel != null)
				{
					newGroupLinkLabel.IsDesignSelected = true;

					newGroupLinkLabel.Focus();
				}

                bool bIsValid = groupToPaste.ValidateStructure();
                if (!bIsValid)
                {
                    newGroupLinkLabel.BackColor = this.MenuDesignerCtrl.ErrorColor;
                }
                else 
                {
                    newGroupLinkLabel.BackColor = this.MenuDesignerCtrl.OkGroupColor;
                }
				return (newGroupLinkLabel != null);
			}

			MenuBranch menuBranchToPaste = itemToPaste as MenuBranch;
			if (menuBranchToPaste != null)
			{
				MenuEditorTreeNode nodeBefore = null;
				MenuEditorTreeNode parentNode = null;
				if (aMenuDesignUIItem != null)
				{
					if
						(
						aMenuDesignUIItem.MenuBranch != null &&
						aMenuDesignUIItem.TreeNode != null
						)
					{
						parentNode = aMenuDesignUIItem.TreeNode;
						//parentNode = nodeBefore.Parent as MenuEditorTreeNode;
					}
					else if (aMenuDesignUIItem.Group != null)
					{
						if (!aMenuDesignUIItem.IsDesignSelected)
						{
							this.MenuDesignerCtrl.SelectGroupLabel(aMenuDesignUIItem.Control as CollapsiblePanelLinkLabel);
							Application.DoEvents();
						}

						TreeView branchesTreeView = this.MenuDesignerCtrl.GetMenuBranchesTreeView();
						if (branchesTreeView.Nodes.Count > 0)
							nodeBefore = branchesTreeView.Nodes[branchesTreeView.Nodes.Count - 1] as MenuEditorTreeNode;

						parentNode = null;

                        MenuGroup oMenuGroup = aMenuDesignUIItem.Group;

                        if (oMenuGroup != null)
                        {

                            bool bValidate = oMenuGroup.ValidateStructure();

                            CollapsiblePanelLinkLabel oGroupLabel = this.MenuDesignerCtrl.FindGroupLinkLabel(oMenuGroup);
                            if (!bValidate)
                            {
                                // at least one child has some validation problem.
                                oGroupLabel.BackColor = this.MenuDesignerCtrl.ErrorColor;
                            }
                            else
                            {
                                oGroupLabel.BackColor = this.MenuDesignerCtrl.OkGroupColor;
                            }
                        }
					}
					else
						return false; // non sono su una label di gruppo e nemmeno su un nodo di un ramo di menù!!!

                   
				}

				MenuEditorTreeNode treeNode = this.MenuDesignerCtrl.InsertMenuBranch(menuBranchToPaste, parentNode, nodeBefore);

				if (treeNode == null)
					return false;

				if (treeNode.Parent != null && !treeNode.Parent.IsExpanded)
					treeNode.Parent.Expand();

				treeNode.EnsureVisible();

				this.MenuDesignerCtrl.GetMenuBranchesTreeView().SelectedNode = treeNode;

				return true;
			}

			MenuCommand menuCommandToPaste = itemToPaste as MenuCommand;
			if (menuCommandToPaste != null)
			{
				MenuEditorTreeNode nodeBefore = null;
				if (aMenuDesignUIItem != null)
				{
					if
						(
						aMenuDesignUIItem.Command != null &&
						aMenuDesignUIItem.TreeNode != null
						)
					{
						nodeBefore = aMenuDesignUIItem.TreeNode;
					}
					else if (aMenuDesignUIItem.MenuBranch != null)
					{
						aMenuDesignUIItem.IsDesignSelected = true;

						TreeView commandsTreeView = this.MenuDesignerCtrl.GetCommandsTreeView();
						if (commandsTreeView.Nodes.Count > 0)
							nodeBefore = commandsTreeView.Nodes[commandsTreeView.Nodes.Count - 1] as MenuEditorTreeNode;
					}
					else
						return false; // non sono su un nodo di un ramo di menù e nemmeno su un comando!!!
				}

				MenuEditorTreeNode treeNode = this.MenuDesignerCtrl.InsertMenuCommand(menuCommandToPaste, nodeBefore);
				if (treeNode == null)
					return false;

				treeNode.EnsureVisible();

				this.MenuDesignerCtrl.GetCommandsTreeView().SelectedNode = treeNode;

				return true;
			}

			return false;
		}


		//---------------------------------------------------------------------
        private void RefreshContent()
        {
            if (this.MenuDesignerCtrl == null || this.MenuDesignerCtrl.IsDisposed)
                return;

            ClearContent();

            if 
                (
				this.MenuDesignerCtrl.MenuFile == null ||
				this.MenuDesignerCtrl.MenuFile.Length == 0 ||
                currentMenuModel == null || 
                currentMenuModel.Applications == null ||
                currentMenuModel.Applications.Count == 0
                )
                return;

            for (int appIdx = 0; appIdx <currentMenuModel.Applications.Count; appIdx++)
            {
                MenuApplication loadedApplication = currentMenuModel.Applications[appIdx] as MenuApplication;
                if (loadedApplication == null)
                    continue;

                this.MenuDesignerCtrl.AddApplication(loadedApplication);
            }
        }

		//---------------------------------------------------------------------
        private void ClearContent()
        {
            if (this.MenuDesignerCtrl == null || this.MenuDesignerCtrl.IsDisposed)
                return;

            this.MenuDesignerCtrl.ClearContent();
        }

		//---------------------------------------------------------------------
        private void DeleteMenuDesignUIItem(MenuDesignUIItem aMenuDesignUIItem)
        {
            if
                (
                aMenuDesignUIItem == null ||
                aMenuDesignUIItem.MenuItem == null ||
                !aMenuDesignUIItem.IsDesignSelected
                )
                return;

			BaseMenuItem aBaseMenuItem = aMenuDesignUIItem.MenuItem as BaseMenuItem;
			MenuCommand aMenuCommand = aMenuDesignUIItem.MenuItem as MenuCommand;
			string message = String.Empty;

			if (aBaseMenuItem != null)
				message = String.Format(MenuEditorStrings.DeleteMenuItemMessageAlertTextFormat, aBaseMenuItem.Name, aBaseMenuItem.GetMenuItemTypeDescription());
			else if (aMenuCommand != null)
				message = String.Format(MenuEditorStrings.DeleteMenuItemMessageAlertTextFormat, aMenuCommand.Title, aMenuCommand.GetMenuCommandTypeDescription());

			DialogResult res = MessageBox.Show(this, message, Resources.DeletingFiles, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
			if (res != DialogResult.Yes)
				return;

			if (currentMenuModel == null)
				return;

            MenuBranch oBranch = currentMenuModel.GetCommandMenuBranch(aMenuCommand);
            
            MenuEditorTreeNode oNode = this.MenuDesignerCtrl.FindMenuBranchTreeNode(oBranch);

            BaseMenuItem oParentItem = currentMenuModel.GetMenuBranchParent(aMenuDesignUIItem.MenuBranch ?? oBranch);
			bool ok = currentMenuModel.RemoveMenuItem(aMenuDesignUIItem.MenuItem);
			if (ok)
			{
                
				this.MenuDesignerCtrl.RemoveMenuItem(aMenuDesignUIItem);

                // check what to do about parent node model.
                MenuBranch oParentBranch = oParentItem as MenuBranch;
                if ((oParentBranch == null && (aMenuDesignUIItem.IsParentToBeRemoved())) || (aMenuDesignUIItem.IsParentToBeRemoved() && (oParentBranch != null && oParentBranch.Commands.Count == 0)))
                {                    
                    this.currentMenuModel.RemoveMenuItem(oParentItem);
                    MenuDesignUIItem oParentMenuItem = this.MenuDesignerCtrl.GetMenuUIItem(oParentItem);
                    this.MenuDesignerCtrl.RemoveMenuItem(oParentMenuItem);
                }
                if (oNode != null)
                {
                    bool bIsValid = oNode.ValidateStructure(oNode.Parent as MenuEditorTreeNode);

                    if (bIsValid)
                    {
                        oNode.BackColor = this.MenuDesignerCtrl.OkColor;
                    }
                    else
                    {
                        oNode.BackColor = this.MenuDesignerCtrl.ErrorColor;
                    }
                }

                // look for the group the node belongs to.
                MenuDesignUIItem oGroup = this.MenuDesignerCtrl.FindNodeGroup(aMenuDesignUIItem.MenuBranch ?? oBranch);
                if (oGroup != null)
                {
                    MenuGroup oMenuGroup = oGroup.Group;

                    bool bValidate = oMenuGroup.ValidateStructure();

                    CollapsiblePanelLinkLabel oGroupLabel = this.MenuDesignerCtrl.FindGroupLinkLabel(oMenuGroup);
                    if (!bValidate)
                    {
                        // at least one child has some validation problem.
                        oGroupLabel.BackColor = this.MenuDesignerCtrl.ErrorColor    ;
                    }
                    else
                    {
                        oGroupLabel.BackColor = this.MenuDesignerCtrl.OkGroupColor;
                    }
                }
				IsDirty = true;
			}
        }

		//---------------------------------------------------------------------
		private void ShowMenuDesignUIItemProperties(MenuDesignUIItem aMenuDesignUIItem)
		{
			if
				(
				aMenuDesignUIItem == null ||
				aMenuDesignUIItem.MenuItem == null ||
				!aMenuDesignUIItem.IsDesignSelected
				)
				return;

			mainToolbar.HostedControl.OnOpenProperties(this, EventArgs.Empty);
		}

		//---------------------------------------------------------------------
        private bool DoInsertCommandToolStripMenuItemClick(Type aCommandType)
        {
            if
                (
                aCommandType == null ||
                !aCommandType.IsSubclassOf(typeof(MenuCommand))
                )
                return false;

            MenuDesignUIItem aMenuDesignUIItem = this.InsertCommandToolStripMenuItem.Tag as MenuDesignUIItem;
            if
                (
                aMenuDesignUIItem == null ||
                aMenuDesignUIItem.MenuItem == null ||
                !aMenuDesignUIItem.IsDesignSelected ||
                (!(aMenuDesignUIItem.MenuItem is MenuBranch) && !(aMenuDesignUIItem.MenuItem is MenuCommand))
                )
                return false;

            MenuEditorTreeNode menuBranchTreeNode = null;
			MenuEditorTreeNode nodeBefore = null;
            if (aMenuDesignUIItem.MenuItem is MenuBranch)
                menuBranchTreeNode = aMenuDesignUIItem.TreeNode;
            else
                nodeBefore = aMenuDesignUIItem.TreeNode;

			MenuCommand aMenuCommand = aCommandType.Assembly.CreateInstance(aCommandType.FullName) as MenuCommand;

			BaseMenuItem menuItemBefore = (nodeBefore != null && nodeBefore.MenuItem != null)
				? nodeBefore.MenuItem
				: null;

			MenuBranch menuItemParent = (menuBranchTreeNode != null && menuBranchTreeNode.MenuItem != null)
				? menuBranchTreeNode.MenuItem as MenuBranch
				: menuItemBefore.Site.Container as MenuBranch;

			if (menuItemParent == null)
				return false;

			bool ok = currentMenuModel.AddMenuItem(aMenuCommand, menuItemParent, menuItemBefore, true);
			if (!ok)
				return false;

			ok = this.MenuDesignerCtrl.InsertNewCommand(aMenuCommand, menuBranchTreeNode, nodeBefore);
			if (!ok)
				return false;

			IsDirty = true;

			return ok;
        }

		//---------------------------------------------------------------------
		private static XmlDocument LoadOrCreateCurrentXmlDocument(string filePath)
		{
			if (String.IsNullOrEmpty(filePath) || !Path.IsPathRooted(filePath))
				return null;

			XmlDocument requestedXml = new XmlDocument();

			if (File.Exists(filePath))
				requestedXml.Load(filePath);

			return requestedXml;
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public void OpenMenuDesignerForm(XmlDocument fullMenuXmlDoc)
        {
			String aMenuFilePath = MenuEditorEngine.PrepareMenuFileForEasyBuilderApp(
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp,
				currentUser
				);

			this.Text = Path.GetFileName(aMenuFilePath) + MenuEditorStrings.MenuDesignerTitlePostfix;

			menuXmlDocument = LoadOrCreateCurrentXmlDocument(aMenuFilePath);

			currentMenuModel = MenuEditorEngine.LoadMenuModel(fullMenuXmlDoc);
			if (currentMenuModel == null)
				return;

			currentMenuModel.ApplyMenuEditorAttributes(menuXmlDocument);

			imagesManager.SubscribeToMenuModelPropertyChanges(currentMenuModel, aMenuFilePath);

			currentMenuModel.PropertyValueChanged += new EventHandler<MenuItemPropertyValueChangedEventArgs>(CurrentMenuModel_PropertyValueChanged);
            currentMenuModel.MenuModelCleared += new EventHandler<EventArgs>(CurrentMenuModel_MenuModelCleared);

			menuDomUpdater = new MenuDomUpdater(currentMenuModel, menuXmlDocument, fullMenuXmlDoc);
			 
			this.MenuDesignerCtrl.MenuFile = aMenuFilePath;

            RefreshContent();
       }

		//---------------------------------------------------------------------
		/// <remarks/>
        public bool SaveCurrentMenuFile(bool publish)
        {
			if (!EBLicenseManager.CanISave)
			{
				UIService.ShowMessage(
					Resources.FunctionalityNotAllowedWithCurrentActivation,
					NameSolverStrings.EasyStudioDesigner,
					MessageBoxButtons.OK
					);
				return false;
			}
			if (currentMenuModel == null || String.IsNullOrEmpty(this.MenuDesignerCtrl.MenuFile) || !Path.IsPathRooted(this.MenuDesignerCtrl.MenuFile))
                return false;
          
            try
            {
				string dirPath = Path.GetDirectoryName(this.MenuDesignerCtrl.MenuFile);
				if (!Directory.Exists(dirPath))
					Directory.CreateDirectory(dirPath);

				BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(this.MenuDesignerCtrl.MenuFile, true, false, publish ? "" : currentUser);
				
				this.menuXmlDocument.Save(this.MenuDesignerCtrl.MenuFile);
				IsDirty = false;
            }
            catch (UnauthorizedAccessException)
            {
				DialogResult res = UIService.ShowMessage(
					String.Format(MenuEditorStrings.NotAccessibleFileSaveFailedMsg, this.MenuDesignerCtrl.MenuFile),
					NameSolverStrings.EasyStudioDesigner,
					MessageBoxButtons.RetryCancel
					);
               if (res == DialogResult.Retry)
                    return SaveCurrentMenuFile(publish);

                return false;
            }
            catch (Exception exception)
            {
                Debug.Fail("Exception raised during MenuDesignerForm.SaveCurrentMenuFile: " + exception.Message);
                return false;
            }

			if (publish)
			{
				BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
				BaseCustomizationContext.CustomizationContextInstance.PublishMenu(this.MenuDesignerCtrl.MenuFile);
			}

			return true;
        }

		//---------------------------------------------------------------------
        private void CurrentMenuModel_PropertyValueChanged(object sender, MenuItemPropertyValueChangedEventArgs e)
        {
            if (e == null || e.ChangedItem == null || e.Property == null)
				return;

			IsDirty = true;
            bool isAppeareanceAffected = false;
            // Controllo che si tratti di una proprietà che influisce sull'aspetto del menù e che, quindi, vada
            // effettivamente aggiornata l'interfaccia
            object[] customAttrs = e.Property.GetCustomAttributes(true);
            if (customAttrs == null || customAttrs.Length == 0)
                return;

			AffectsAppearanceAttribute affectsAppearanceAttribute = null;
			CategoryAttribute categoryAttribute = null;
            foreach (object aCustomAttribute in customAttrs)
            {
                if (aCustomAttribute == null)
                    continue;

				affectsAppearanceAttribute = aCustomAttribute as AffectsAppearanceAttribute;
				categoryAttribute = aCustomAttribute as CategoryAttribute;

                if (
					(affectsAppearanceAttribute != null && affectsAppearanceAttribute.AffectsAppearance) ||
					(categoryAttribute != null && String.Compare(categoryAttribute.Category, "Appearance", StringComparison.InvariantCultureIgnoreCase) == 0)
                    )
                {
                    isAppeareanceAffected = true;
                    break;
                }
            }
	
			if (isAppeareanceAffected)
				MenuDesignerCtrl.UpdateMenuItemUI(e.ChangedItem);
        }

		//---------------------------------------------------------------------
		private void CurrentMenuModel_MenuModelCleared(object sender, EventArgs e)
		{
			ClearContent();
		}

		//---------------------------------------------------------------------
        private void MenuDesignerContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
			Point mousePosition = Control.MousePosition;
			MenuDesignUIItem menuItem = this.MenuDesignerCtrl.GetMenuDesignUIItemFromMousePoint(Control.MousePosition);

			if (menuItem == null)
			{
				ConfigureContextMenu(mousePosition, e);
				return;
			}

            this.ShowPropertiesToolStripMenuItem.Enabled = true;
            this.ShowPropertiesToolStripMenuItem.Tag = menuItem;

            this.MoveUpToolStripMenuItem.Visible = true;
            this.MoveDownToolStripMenuItem.Visible = true;

			string applicationName = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName;

            if (menuItem.MenuItem is MenuApplication)
            {
				//bool moreApps = currentMenuModel.Applications.Count > 0;
				this.MoveUpToolStripMenuItem.Enabled = false;// moreApps && currentMenuModel.Applications[0] != menuItem.MenuItem;
				this.MoveDownToolStripMenuItem.Enabled = false;//moreApps && currentMenuModel.Applications[currentMenuModel.Applications.Count - 1] != menuItem.MenuItem;

				MenuApplication currentApp = currentMenuModel.GetApplication(applicationName);
				this.InsertApplicationToolStripMenuItem.Visible = currentApp == null;
				this.InsertApplicationToolStripMenuItem.Tag = menuItem;
				this.InsertGroupToolStripMenuItem.Visible = true;
				this.InsertGroupToolStripMenuItem.Tag = menuItem;

				this.InsertMenuBranchToolStripMenuItem.Visible = false;
				this.InsertMenuBranchToolStripMenuItem.Tag = null;
				this.InsertChildMenuBranchToolStripMenuItem.Visible = false;
				this.InsertChildMenuBranchToolStripMenuItem.Tag = null;
				this.InsertCommandToolStripMenuItem.Visible = false;
				this.InsertCommandToolStripMenuItem.Tag = null;

				this.SetImageToolStripMenuItem.Visible = menuItem.MenuItem.CanBeCustomized;
				this.SetImageToolStripMenuItem.Tag = menuItem;

				this.ResetImageToolStripMenuItem.Visible = menuItem.MenuItem.CanBeCustomized && ExistsImageForItem(menuItem.MenuItem);
				this.ResetImageToolStripMenuItem.Tag = menuItem;
            }
            else if (menuItem.MenuItem is MenuGroup)
            {
				MenuApplication aMenuApplication = menuItem.MenuItem.Site.Container as MenuApplication;
				bool moreGroups = aMenuApplication.Groups.Count > 0;

				this.MoveUpToolStripMenuItem.Enabled =
					moreGroups &&
					aMenuApplication.Groups[0] != menuItem.MenuItem &&
					menuItem.MenuItem.CanBeCustomized;

				this.MoveDownToolStripMenuItem.Enabled =
					moreGroups &&
					aMenuApplication.Groups[aMenuApplication.Groups.Count - 1] != menuItem.MenuItem &&
					menuItem.MenuItem.CanBeCustomized;

				this.InsertGroupToolStripMenuItem.Visible = true;
				this.InsertGroupToolStripMenuItem.Tag = menuItem;
				this.InsertMenuBranchToolStripMenuItem.Visible = true;
				this.InsertMenuBranchToolStripMenuItem.Tag = menuItem;
				this.InsertChildMenuBranchToolStripMenuItem.Visible = false;
				this.InsertChildMenuBranchToolStripMenuItem.Tag = null;
				this.InsertMenuBranchToolStripMenuItem.Text = MenuEditorStrings.InsertMenuBranchToolStripMenuItem;

				this.InsertCommandToolStripMenuItem.Visible = false;

				this.SetImageToolStripMenuItem.Visible = menuItem.MenuItem.CanBeCustomized;
				this.SetImageToolStripMenuItem.Tag = menuItem;

				this.ResetImageToolStripMenuItem.Visible = menuItem.MenuItem.CanBeCustomized && ExistsImageForItem(menuItem.MenuItem);
				this.ResetImageToolStripMenuItem.Tag = menuItem;
            }
            else if (menuItem.MenuItem is MenuBranch)
            {
                this.InsertMenuBranchToolStripMenuItem.Visible = true;
                this.InsertMenuBranchToolStripMenuItem.Tag = menuItem;
				this.InsertMenuBranchToolStripMenuItem.Text = MenuEditorStrings.InsertSiblingMenuBranchToolStripMenuItem;

                this.InsertChildMenuBranchToolStripMenuItem.Tag = menuItem;
                this.InsertChildMenuBranchToolStripMenuItem.Visible = true;
                this.InsertCommandToolStripMenuItem.Visible = true;
                this.InsertCommandToolStripMenuItem.Tag = menuItem;
                bool bCanAddChild = currentMenuModel.CanAddChild(menuItem.TreeNode != null ? menuItem.TreeNode.MenuItem : null);
                if (bCanAddChild)
                {

                    this.InsertChildMenuBranchToolStripMenuItem.Enabled = true;
                }
                else 
                {
                    this.InsertChildMenuBranchToolStripMenuItem.Enabled = false;
                    this.InsertChildMenuBranchToolStripMenuItem.ToolTipText = Resources.ChildBranchNotAllowed;
                }
                bool bCanAddCommand = currentMenuModel.CanAddCommand(menuItem.TreeNode != null ? menuItem.TreeNode.MenuItem : null);
                if (bCanAddCommand)
                {

                    this.InsertCommandToolStripMenuItem.Enabled = true;
                }
                else
                {
                    this.InsertCommandToolStripMenuItem.Enabled = false;                    
                    this.InsertCommandToolStripMenuItem.ToolTipText = Resources.CommandNotAllowed;
                }
				this.InsertGroupToolStripMenuItem.Visible = false;
				this.InsertGroupToolStripMenuItem.Tag = null;

				this.MoveUpToolStripMenuItem.Enabled =
					(menuItem.MenuItem.CanBeCustomized && menuItem.TreeNode != null)
					? (menuItem.TreeNode.PrevNode != null)
					: false;

				this.MoveDownToolStripMenuItem.Enabled =
					(menuItem.MenuItem.CanBeCustomized && menuItem.TreeNode != null)
					? (menuItem.TreeNode.NextNode != null)
					: false;

				this.SetImageToolStripMenuItem.Visible = false;
				this.SetImageToolStripMenuItem.Tag = null;

				this.ResetImageToolStripMenuItem.Visible = false;
				this.ResetImageToolStripMenuItem.Tag = null;
            }
			else
            {
				this.InsertGroupToolStripMenuItem.Visible = false;
				this.InsertGroupToolStripMenuItem.Tag = null;

				this.InsertCommandToolStripMenuItem.Visible = true;
                this.InsertCommandToolStripMenuItem.Tag = menuItem;

				this.InsertMenuBranchToolStripMenuItem.Visible = false;
				this.InsertMenuBranchToolStripMenuItem.Tag = null;

				this.InsertChildMenuBranchToolStripMenuItem.Visible = false;
				this.InsertChildMenuBranchToolStripMenuItem.Tag = null;

				this.MoveUpToolStripMenuItem.Enabled =
					(menuItem.MenuItem.CanBeCustomized && menuItem.TreeNode != null)
					? (menuItem.TreeNode.PrevNode != null)
					: false;

				this.MoveDownToolStripMenuItem.Enabled =
					(menuItem.MenuItem.CanBeCustomized && menuItem.TreeNode != null)
					? (menuItem.TreeNode.NextNode != null)
					: false;

				this.SetImageToolStripMenuItem.Visible = false;
				this.SetImageToolStripMenuItem.Tag = null;

				this.ResetImageToolStripMenuItem.Visible = false;
				this.ResetImageToolStripMenuItem.Tag = null;
            }

            this.MoveUpToolStripMenuItem.Tag = menuItem;
            this.MoveDownToolStripMenuItem.Tag = menuItem;

			this.CutToolStripMenuItem.Enabled = menuItem.MenuItem.CanBeCustomized;
			this.CutToolStripMenuItem.Tag = menuItem.MenuItem.CanBeCustomized ? menuItem : null;

			this.CopyToolStripMenuItem.Enabled = true;
            this.CopyToolStripMenuItem.Tag = menuItem;

       

                
			this.PasteToolStripMenuItem.Enabled = menuItem.IsValidClipboardDataTarget();
			this.PasteToolStripMenuItem.Tag = menuItem;


			this.DeleteMenuItemToolStripMenuItem.Enabled = menuItem.MenuItem.CanBeCustomized;
			this.DeleteMenuItemToolStripMenuItem.Tag = menuItem.MenuItem.CanBeCustomized ? menuItem : null; 
        }

		//---------------------------------------------------------------------
		private bool ExistsImageForItem(BaseMenuItem menuItem)
		{
			DirectoryInfo menuFolderDirInfo = new DirectoryInfo(Path.GetDirectoryName(this.MenuDesignerCtrl.MenuFile));

			return menuFolderDirInfo.GetFiles(String.Format("{0}.???", menuItem.Name)).Length > 0;
		}

		//---------------------------------------------------------------------
		private MenuDesignUIItem FindMenuItem(Point mousePosition)
		{
			MenuDesignUIItem menuItem = this.MenuDesignerCtrl.GetMenuDesignUIItemFromMousePoint(mousePosition);
			if (menuItem != null)
				return menuItem;

			Control aControl = this.MenuDesignerCtrl.GetChildAtPoint(
					this.MenuDesignerCtrl.PointToClient(mousePosition)
					);

			if (aControl == MenuDesignerCtrl.GetCommandsTreeView())
			{
				MenuEditorTreeNode selectedNode = this.MenuDesignerCtrl.GetCommandsTreeView().SelectedNode as MenuEditorTreeNode;
				return selectedNode != null ? new MenuDesignUIItem(selectedNode) : null;
			}

			if (aControl == MenuDesignerCtrl.GetMenuBranchesTreeView())
			{
				MenuEditorTreeNode selectedNode = this.MenuDesignerCtrl.GetMenuBranchesTreeView().SelectedNode as MenuEditorTreeNode;
				return selectedNode != null ? new MenuDesignUIItem(selectedNode) : null;
			}

			if (aControl is CollapsiblePanelBar)
			{
				CollapsiblePanelLinkLabel currentPanel = this.MenuDesignerCtrl.CurrentSelectedGroupLabel;
				return currentPanel != null ? new MenuDesignUIItem(currentPanel) : null;
			}

			return null;
		}

		//---------------------------------------------------------------------
		private MenuDesignUIItem FindMenuItemContainer(Point mousePosition)
		{
			Control aControl = this.MenuDesignerCtrl.GetChildAtPoint(
					this.MenuDesignerCtrl.PointToClient(mousePosition)
					);

			if (aControl == null)
				return null;

			if (aControl == MenuDesignerCtrl.GetCommandsTreeView())
			{
				MenuEditorTreeNode selectedNode = this.MenuDesignerCtrl.GetMenuBranchesTreeView().SelectedNode as MenuEditorTreeNode;
				return selectedNode != null ? new MenuDesignUIItem(selectedNode) : null;
			}

			if (aControl == MenuDesignerCtrl.GetMenuBranchesTreeView())
			{
				CollapsiblePanelLinkLabel currentPanel = this.MenuDesignerCtrl.CurrentSelectedGroupLabel;
				return currentPanel != null ? new MenuDesignUIItem(currentPanel) : null;
			}

			//CollapsiblePanelBar aCollapsiblePanelBar = aControl as CollapsiblePanelBar;
			//if (aCollapsiblePanelBar != null)
			//{
			//    foreach (CollapsiblePanel panel in aCollapsiblePanelBar.Panels)
			//    {
			//        if (!panel.IsDesignSelected)
			//            continue;

			//        return new MenuDesignUIItem(panel);
			//    }
			//}
			return null;
		}

        //---------------------------------------------------------------------
        private bool IsRootBranch(TreeNode oNode)
        {
            if (oNode == null) 
            {
                return false;
            }
            TreeView oTreeView = this.MenuDesignerCtrl.GetMenuBranchesTreeView();
            foreach (TreeNode oCurrentNode in oTreeView.Nodes)
            {
                if (oNode == oCurrentNode)
                {
                    // the given node has been found among the roots in the tree view control.
                    return true;
                }
            }
            return false;
        }

		//---------------------------------------------------------------------
		private void ConfigureContextMenu(Point mousePosition, CancelEventArgs e)
		{
			Control aControl = this.MenuDesignerCtrl.GetChildAtPoint(
				this.MenuDesignerCtrl.PointToClient(mousePosition)
				);
          
			if (aControl is CollapsiblePanelBar)
			{
				string applicationName = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName;
				if (currentMenuModel.GetApplication(applicationName) != null)
				{
					e.Cancel = true;
					return;
				}
				this.MoveUpToolStripMenuItem.Enabled = false;
				this.MoveDownToolStripMenuItem.Enabled = false;

				this.InsertApplicationToolStripMenuItem.Visible = true;
				this.InsertApplicationToolStripMenuItem.Tag = null;
				this.InsertGroupToolStripMenuItem.Visible = false;
				this.InsertGroupToolStripMenuItem.Tag = null;

				this.InsertMenuBranchToolStripMenuItem.Visible = false;
				this.InsertMenuBranchToolStripMenuItem.Tag = null;
				this.InsertChildMenuBranchToolStripMenuItem.Visible = false;
				this.InsertChildMenuBranchToolStripMenuItem.Tag = null;
				this.InsertCommandToolStripMenuItem.Visible = false;
				this.InsertCommandToolStripMenuItem.Tag = null;

				this.PasteToolStripMenuItem.Enabled = IsValidClipboardDataTarget(typeof(MenuApplication));
				this.PasteToolStripMenuItem.Tag = FindMenuItemContainer(mousePosition);
			}

			if (aControl == MenuDesignerCtrl.GetCommandsTreeView())
			{
				this.InsertGroupToolStripMenuItem.Visible = false;
				this.InsertGroupToolStripMenuItem.Tag = null;

				MenuEditorTreeNode selectedNode = this.MenuDesignerCtrl.GetMenuBranchesTreeView().SelectedNode as MenuEditorTreeNode;

				this.InsertCommandToolStripMenuItem.Visible = selectedNode != null;

				CutCopyPasteDeleteToolstripSeparator.Visible = selectedNode != null;

				this.InsertCommandToolStripMenuItem.Tag =
					selectedNode != null ?
					new MenuDesignUIItem(selectedNode) : null;

				this.InsertMenuBranchToolStripMenuItem.Visible = false;
				this.InsertMenuBranchToolStripMenuItem.Tag = null;

				this.InsertChildMenuBranchToolStripMenuItem.Visible = false;
				this.InsertChildMenuBranchToolStripMenuItem.Tag = null;

				this.MoveUpToolStripMenuItem.Enabled = false;

				this.MoveDownToolStripMenuItem.Enabled = false;

				this.PasteToolStripMenuItem.Enabled = IsValidClipboardDataTarget(typeof(MenuCommand));
				this.PasteToolStripMenuItem.Tag = selectedNode != null ?
					new MenuDesignUIItem(selectedNode) : null;

                TreeView oTreeView = this.MenuDesignerCtrl.GetMenuBranchesTreeView();
                TreeNode oSelectedNode = oTreeView.SelectedNode;

                if (oSelectedNode != null)
                {
                    if (IsRootBranch(oSelectedNode))
                    {
                        // the selected node is a root, it 
                        // can not have commands as children.
                        this.InsertCommandToolStripMenuItem.Enabled = false;
                        this.InsertCommandToolStripMenuItem.ToolTipText = Resources.CommandNotAllowed;

                        this.PasteToolStripMenuItem.Enabled = false;
                        this.PasteToolStripMenuItem.ToolTipText = Resources.CommandNotAllowed;

                    }
                    else
                    {
                        this.InsertCommandToolStripMenuItem.Enabled = true;
                        this.InsertCommandToolStripMenuItem.ToolTipText = string.Empty;

                        this.PasteToolStripMenuItem.Enabled = true;
                        this.PasteToolStripMenuItem.ToolTipText = string.Empty;
                    }
                }
			}

			if (aControl == MenuDesignerCtrl.GetMenuBranchesTreeView())
			{
				CollapsiblePanelLinkLabel currentPanel = this.MenuDesignerCtrl.CurrentSelectedGroupLabel;
				this.InsertMenuBranchToolStripMenuItem.Visible = currentPanel != null;

				CutCopyPasteDeleteToolstripSeparator.Visible = currentPanel != null;

				this.InsertMenuBranchToolStripMenuItem.Tag =
					currentPanel != null ?
					new MenuDesignUIItem(currentPanel) : null;

				this.InsertMenuBranchToolStripMenuItem.Text = MenuEditorStrings.InsertMenuBranchToolStripMenuItem;
				this.InsertChildMenuBranchToolStripMenuItem.Visible = false;
				this.InsertChildMenuBranchToolStripMenuItem.Tag = null;
				this.InsertCommandToolStripMenuItem.Visible = false;
				this.InsertCommandToolStripMenuItem.Tag = null;

				this.InsertGroupToolStripMenuItem.Visible = false;
				this.InsertGroupToolStripMenuItem.Tag = null;

				this.MoveUpToolStripMenuItem.Enabled = false;

				this.MoveDownToolStripMenuItem.Enabled = false;

				this.PasteToolStripMenuItem.Enabled = IsValidClipboardDataTarget(typeof(MenuBranch));
				this.PasteToolStripMenuItem.Tag = currentPanel != null ?
					new MenuDesignUIItem(currentPanel) : null;
			}

			this.CutToolStripMenuItem.Enabled = false;
			this.CutToolStripMenuItem.Tag = null;

			this.CopyToolStripMenuItem.Enabled = false;
			this.CopyToolStripMenuItem.Tag = null;

			this.DeleteMenuItemToolStripMenuItem.Enabled = false;
			this.DeleteMenuItemToolStripMenuItem.Tag = null;

			this.ShowPropertiesToolStripMenuItem.Enabled = false;
			this.ShowPropertiesToolStripMenuItem.Tag = null;

			this.SetImageToolStripMenuItem.Visible = false;
			this.SetImageToolStripMenuItem.Tag = null;

			this.ResetImageToolStripMenuItem.Visible = false;
			this.ResetImageToolStripMenuItem.Tag = null;
          
		}

		//---------------------------------------------------------------------
		private static bool IsValidClipboardDataTarget(Type menuItemType)
		{
			IDataObject data = Clipboard.GetDataObject();
			if (data == null)
				return false;

			if (menuItemType == typeof(MenuCommand))
			{
				if (data.GetDataPresent(typeof(DocumentMenuCommand).FullName, true))
					return true;
				if (data.GetDataPresent(typeof(ReportMenuCommand).FullName, true))
					return true;
				if (data.GetDataPresent(typeof(BatchMenuCommand).FullName, true))
					return true;
				if (data.GetDataPresent(typeof(FunctionMenuCommand).FullName, true))
					return true;
				if (data.GetDataPresent(typeof(ExeMenuCommand).FullName, true))
					return true;
				if (data.GetDataPresent(typeof(TextMenuCommand).FullName, true))
					return true;
				if (data.GetDataPresent(typeof(OfficeItemMenuCommand).FullName, true))
					return true;
			}

			return (data.GetDataPresent(menuItemType.FullName));
		}

		//---------------------------------------------------------------------
		private void SetImageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string sourceFilePath = null;
			using (OpenFileDialog ofd = new OpenFileDialog())
			{
				ofd.Multiselect = false;

				ofd.Title = Resources.MenuEditorSelectImage;
				ofd.Filter = Resources.MenuEditorFilterAllFiles;

				DialogResult res = ofd.ShowDialog();
				if (res != DialogResult.OK)
					return;

				sourceFilePath = ofd.FileName;
			}

			MenuDesignUIItem selectedMenuDesignUIItem = SetImageToolStripMenuItem.Tag as MenuDesignUIItem;
			if (selectedMenuDesignUIItem == null)
				return;

			BaseMenuItem aBaseMenuItem = selectedMenuDesignUIItem.MenuItem;
			if (aBaseMenuItem == null)
				return;

			string sourceFileExt = Path.GetExtension(sourceFilePath);

			string targetFolder = Path.GetDirectoryName(this.MenuDesignerCtrl.MenuFile);

			string targetFilePath = Path.Combine(
				targetFolder,
				String.Format("{0}{1}", aBaseMenuItem.Name, sourceFileExt)
				);

			FileInfo[] targetFileInfos = new DirectoryInfo(targetFolder).GetFiles(
				String.Format("{0}.???", Path.GetFileNameWithoutExtension(targetFilePath))
				);
			if (targetFileInfos != null && targetFileInfos.Length > 0)
			{
				if (this.UIService != null)
				{
					DialogResult diagRes = this.UIService.ShowMessage(
						MenuEditorStrings.ImageFileAlreadyExistsDoYouWantToOverride,
						MenuEditorStrings.ImageFileAlreadyExistsErrorCaption,
						MessageBoxButtons.OKCancel
						);

					if (diagRes != System.Windows.Forms.DialogResult.OK)
						return;
				}

				foreach (FileInfo targetFileInfo in targetFileInfos)
				{
					if (targetFileInfo.IsReadOnly)
						targetFileInfo.IsReadOnly = false;

					targetFileInfo.Delete();
				}
			}

			try
			{
				File.Copy(sourceFilePath, targetFilePath, true);
			}
			catch (Exception)
			{
				throw;
			}

			BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(targetFilePath);

			MenuDesignerCtrl.UpdateMenuItemUI(aBaseMenuItem);

			//IsDirty non deve essere impostato perche non c' `e nulla da salvare?. 
			IsDirty = true;
		}

		//---------------------------------------------------------------------
		private void ResetImageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MenuDesignUIItem selectedMenuDesignUIItem = ResetImageToolStripMenuItem.Tag as MenuDesignUIItem;
			if (selectedMenuDesignUIItem == null)
				return;

			BaseMenuItem aBaseMenuItem = selectedMenuDesignUIItem.MenuItem;
			if (aBaseMenuItem == null)
				return;

			DirectoryInfo menuFolderDirInfo = new DirectoryInfo(Path.GetDirectoryName(this.MenuDesignerCtrl.MenuFile));

			FileInfo[] imagesToBeDeleted = menuFolderDirInfo.GetFiles(String.Format("{0}.???", aBaseMenuItem.Name));
			foreach (FileInfo imageToBeDeletedFileInfo in imagesToBeDeleted)
			{
				if (imageToBeDeletedFileInfo.IsReadOnly)
					imageToBeDeletedFileInfo.IsReadOnly = false;
				
				try
				{
					BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.EasyBuilderAppFileListManager.RemoveFromCustomListAndFromFileSystem(imageToBeDeletedFileInfo.FullName);
				} catch {}
			}

			MenuDesignerCtrl.UpdateMenuItemUI(aBaseMenuItem);
		}

		//---------------------------------------------------------------------
        private void MoveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuDesignUIItem aMenuDesignUIItem = this.MoveUpToolStripMenuItem.Tag as MenuDesignUIItem;
            if
                (
                aMenuDesignUIItem == null ||
                aMenuDesignUIItem.MenuItem == null ||
                !aMenuDesignUIItem.IsDesignSelected 
                )
                return;
            
            if (aMenuDesignUIItem.Application != null)
            {
                CollapsiblePanel applicationPanelToMove = aMenuDesignUIItem.Control as CollapsiblePanel;
                CollapsiblePanel previousApplicationPanel = this.MenuDesignerCtrl.GetPreviousApplicationPanel(applicationPanelToMove);
                if (previousApplicationPanel == null)
                    return;

                this.MenuDesignerCtrl.MoveApplicationPanel(applicationPanelToMove, this.MenuDesignerCtrl.GetPreviousApplicationPanel(previousApplicationPanel));
            }

            if (aMenuDesignUIItem.Group != null)
            {
                CollapsiblePanelLinkLabel groupLinkLabelToMove = aMenuDesignUIItem.Control as CollapsiblePanelLinkLabel;
                CollapsiblePanelLinkLabel previousGroupLinkLabel = MenuDesignerControl.GetPreviousGroupLinkLabel(groupLinkLabelToMove);
                if (previousGroupLinkLabel == null)
                    return;

                this.MenuDesignerCtrl.MoveGroupLinkLabel(groupLinkLabelToMove, MenuDesignerControl.GetPreviousGroupLinkLabel(previousGroupLinkLabel));
            }

            if (aMenuDesignUIItem.MenuBranch != null)
            {
				MenuEditorTreeNode menuBranchNodeToMove = aMenuDesignUIItem.TreeNode;
				MenuEditorTreeNode previousMenuBranchNode = menuBranchNodeToMove.PrevNode as MenuEditorTreeNode;
                if (previousMenuBranchNode == null)
                    return;

                this.MenuDesignerCtrl.MoveMenuBranchNode(menuBranchNodeToMove, menuBranchNodeToMove.Parent, previousMenuBranchNode.PrevNode);
            }

            if (aMenuDesignUIItem.Command != null)
            {
				MenuEditorTreeNode commandNodeToMove = aMenuDesignUIItem.TreeNode;
				MenuEditorTreeNode previousCommandNode = commandNodeToMove.PrevNode as MenuEditorTreeNode;
                if (previousCommandNode == null)
                    return;

                this.MenuDesignerCtrl.MoveCommandNode(commandNodeToMove, previousCommandNode.PrevNode);
            }
        }

		//---------------------------------------------------------------------
        private void MoveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuDesignUIItem aMenuDesignUIItem = this.MoveDownToolStripMenuItem.Tag as MenuDesignUIItem;
            if
                (
                aMenuDesignUIItem == null ||
                aMenuDesignUIItem.MenuItem == null ||
                !aMenuDesignUIItem.IsDesignSelected
                )
                return;

            if (aMenuDesignUIItem.Application != null)
            {
                CollapsiblePanel applicationPanelToMove = aMenuDesignUIItem.Control as CollapsiblePanel;
                CollapsiblePanel nextApplicationPanel = this.MenuDesignerCtrl.GetNextApplicationPanel(applicationPanelToMove);
                if (nextApplicationPanel == null)
                    return;

                this.MenuDesignerCtrl.MoveApplicationPanel(applicationPanelToMove, nextApplicationPanel);
            }

            if (aMenuDesignUIItem.Group != null)
            {
                CollapsiblePanelLinkLabel groupLinkLabelToMove = aMenuDesignUIItem.Control as CollapsiblePanelLinkLabel;
                CollapsiblePanelLinkLabel nextGroupLinkLabel = MenuDesignerControl.GetNextGroupLinkLabel(groupLinkLabelToMove);
                if (nextGroupLinkLabel == null)
                    return;

                this.MenuDesignerCtrl.MoveGroupLinkLabel(groupLinkLabelToMove, nextGroupLinkLabel);
            }

            if (aMenuDesignUIItem.MenuBranch != null)
            {
				MenuEditorTreeNode menuBranchNodeToMove = aMenuDesignUIItem.TreeNode;
				MenuEditorTreeNode nextMenuBranchNode = menuBranchNodeToMove.NextNode as MenuEditorTreeNode;
                if (nextMenuBranchNode == null)
                    return;

                this.MenuDesignerCtrl.MoveMenuBranchNode(menuBranchNodeToMove, menuBranchNodeToMove.Parent, nextMenuBranchNode);
            }

            if (aMenuDesignUIItem.Command != null)
            {
				MenuEditorTreeNode commandNodeToMove = aMenuDesignUIItem.TreeNode;
				MenuEditorTreeNode nextCommandNode = commandNodeToMove.NextNode as MenuEditorTreeNode;
                if (nextCommandNode == null)
                    return;

                this.MenuDesignerCtrl.MoveCommandNode(commandNodeToMove, nextCommandNode);
            }
        }

		//---------------------------------------------------------------------
        private void InsertApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuDesignUIItem aMenuDesignUIItem = this.InsertApplicationToolStripMenuItem.Tag as MenuDesignUIItem;
            if (
				aMenuDesignUIItem != null &&
                (!(aMenuDesignUIItem.MenuItem is MenuApplication))
				)
                return;

			//Nel corrente contesto di customizzazione  possiamo aggiungere una sola applicazione.
			if (currentMenuModel.GetApplication(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName) != null)
				return;

			CollapsiblePanel applicationPanelBefore = null;
			if (aMenuDesignUIItem != null)
				applicationPanelBefore = aMenuDesignUIItem.Control as CollapsiblePanel;

			MenuApplication aMenuApplication = new MenuApplication();

			bool ok = currentMenuModel.AddMenuItem(
				aMenuApplication,
				null,
				applicationPanelBefore != null ? applicationPanelBefore.MenuApplication : null,
				true
				);
			if (!ok)
				return;

			this.MenuDesignerCtrl.InsertNewApplication(aMenuApplication, applicationPanelBefore);            
        }

		//---------------------------------------------------------------------
        private void InsertGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuDesignUIItem aMenuDesignUIItem = this.InsertGroupToolStripMenuItem.Tag as MenuDesignUIItem;
            if
                (
                aMenuDesignUIItem == null ||
                aMenuDesignUIItem.MenuItem == null ||
                (!(aMenuDesignUIItem.MenuItem is MenuApplication) && !(aMenuDesignUIItem.MenuItem is MenuGroup))
                )
                return;

            CollapsiblePanel applicationPanel = null;
            CollapsiblePanelLinkLabel groupLinkLabelBefore = aMenuDesignUIItem.Control as CollapsiblePanelLinkLabel;
            if (groupLinkLabelBefore == null)
            {
                if (aMenuDesignUIItem.MenuItem is MenuApplication)
                    applicationPanel = aMenuDesignUIItem.Control as CollapsiblePanel;
            }
            else
                applicationPanel = groupLinkLabelBefore.Parent as CollapsiblePanel;
            
            if (applicationPanel == null)
                return;

			MenuGroup aMenuGroup = new MenuGroup();

			bool ok = currentMenuModel.AddMenuItem(
				aMenuGroup,
				applicationPanel != null ? applicationPanel.MenuApplication : null,
				groupLinkLabelBefore != null ? groupLinkLabelBefore.MenuGroup : null,
				true
				);
			if (!ok)
				return;

			this.MenuDesignerCtrl.InsertNewGroup(aMenuGroup, applicationPanel, groupLinkLabelBefore);
        }

		//---------------------------------------------------------------------
        private void InsertMenuBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuDesignUIItem aMenuDesignUIItem = this.InsertMenuBranchToolStripMenuItem.Tag as MenuDesignUIItem;
            if
                (
                aMenuDesignUIItem == null ||
                aMenuDesignUIItem.MenuItem == null ||
                !aMenuDesignUIItem.IsDesignSelected ||
                (!(aMenuDesignUIItem.MenuItem is MenuGroup) && !(aMenuDesignUIItem.MenuItem is MenuBranch))
                )
                return;

            CollapsiblePanelLinkLabel groupLinkLabel = aMenuDesignUIItem.Control as CollapsiblePanelLinkLabel;
            if (groupLinkLabel == null && !(aMenuDesignUIItem.MenuItem is MenuBranch))
                return; // non si è posizionati su un gruppo e nemmeno su di un ramo di menù!!!
			MenuEditorTreeNode nodeBefore = (aMenuDesignUIItem.MenuItem is MenuBranch) ? aMenuDesignUIItem.TreeNode : null;
			
			MenuBranch aMenuBranch = new MenuBranch();

			bool ok = currentMenuModel.AddMenuItem(
				aMenuBranch,
				groupLinkLabel != null ? groupLinkLabel.MenuGroup : aMenuDesignUIItem.MenuItem.Site.Container as BaseMenuItem,
				nodeBefore != null ? nodeBefore.MenuItem : null,
				true
				);
			if (!ok)
				return;

            this.MenuDesignerCtrl.InsertNewMenuBranch(aMenuBranch, groupLinkLabel, nodeBefore);
            MenuEditorTreeNode oCreatedBranch = this.MenuDesignerCtrl.GetSeletedTreeNode();
            

            MenuDesignUIItem oAddedItem = this.MenuDesignerCtrl.GetMenuUIItem(aMenuBranch);
            InsertChildMenuBranch(oAddedItem);
            
        }

		//---------------------------------------------------------------------
        private void InsertChildMenuBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuDesignUIItem aMenuDesignUIItem = this.InsertChildMenuBranchToolStripMenuItem.Tag as MenuDesignUIItem;
            InsertChildMenuBranch(aMenuDesignUIItem);
        }

        private void InsertChildMenuBranch(MenuDesignUIItem aMenuDesignUIItem)
        {
            if
                (
                aMenuDesignUIItem == null ||
                aMenuDesignUIItem.MenuItem == null ||
                !aMenuDesignUIItem.IsDesignSelected ||
                !(aMenuDesignUIItem.MenuItem is MenuBranch)
                )
                return;

            MenuBranch aMenuBranch = new MenuBranch();

            bool ok = currentMenuModel.AddMenuItem(
                aMenuBranch,
                aMenuDesignUIItem.TreeNode != null ? aMenuDesignUIItem.TreeNode.MenuItem : null,
                null,
                true
                );
            if (!ok)
                return;

            this.MenuDesignerCtrl.InsertNewMenuBranch(aMenuBranch, aMenuDesignUIItem.TreeNode, null);
        }

		//---------------------------------------------------------------------
        private void InsertDocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoInsertCommandToolStripMenuItemClick(typeof(DocumentMenuCommand));         
        }

		//---------------------------------------------------------------------
        private void InsertBatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoInsertCommandToolStripMenuItemClick(typeof(BatchMenuCommand));
        }

		//---------------------------------------------------------------------
        private void InsertReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoInsertCommandToolStripMenuItemClick(typeof(ReportMenuCommand));
        }

		//---------------------------------------------------------------------
        private void InsertFunctionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoInsertCommandToolStripMenuItemClick(typeof(FunctionMenuCommand));
        }

		//---------------------------------------------------------------------
        private void InsertExecutableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoInsertCommandToolStripMenuItemClick(typeof(ExeMenuCommand));
        }

		//---------------------------------------------------------------------
        private void InserTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoInsertCommandToolStripMenuItemClick(typeof(TextMenuCommand));
        }

		//---------------------------------------------------------------------
        private void InsertOfficeItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoInsertCommandToolStripMenuItemClick(typeof(OfficeItemMenuCommand));
        }

		//---------------------------------------------------------------------
        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuDesignUIItem aMenuDesignUIItem = this.CutToolStripMenuItem.Tag as MenuDesignUIItem;
            if
                (
                aMenuDesignUIItem == null ||
                aMenuDesignUIItem.MenuItem == null ||
                !aMenuDesignUIItem.IsDesignSelected
                )
                return;

            CopyToClipboard(aMenuDesignUIItem);

			SetCutOperation(lastOperationCutItem, false);

			lastOperationCutItem = aMenuDesignUIItem;

			SetCutOperation(lastOperationCutItem, true);
        }

		//---------------------------------------------------------------------
		private static void SetCutOperation(MenuDesignUIItem lastOperationCutItem, bool isCut)
		{
			if (lastOperationCutItem != null)
			{
				if (lastOperationCutItem.TreeNode != null)
					lastOperationCutItem.TreeNode.IsCut = isCut;
				else
				{
					CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = lastOperationCutItem.Control as CollapsiblePanelLinkLabel;
					if (aCollapsiblePanelLinkLabel != null)
						aCollapsiblePanelLinkLabel.IsCut = isCut;
				}
			}
		}

		//---------------------------------------------------------------------
        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuDesignUIItem aMenuDesignUIItem = this.CopyToolStripMenuItem.Tag as MenuDesignUIItem;
            if
                (
                aMenuDesignUIItem == null ||
                aMenuDesignUIItem.MenuItem == null ||
                !aMenuDesignUIItem.IsDesignSelected
                )
                return;

            CopyToClipboard(aMenuDesignUIItem);

			//Un Copy cancella ogni eventuale Cut pending.
			SetCutOperation(lastOperationCutItem, false);
			lastOperationCutItem = null;
        }

		//---------------------------------------------------------------------
        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeView oTreeView = this.MenuDesignerCtrl.GetMenuBranchesTreeView();
            
            TreeNode oSelectedNode = oTreeView.SelectedNode;

             if (oSelectedNode != null)
             {
                 Control aControl = this.MenuDesignerCtrl.GetChildAtPoint(
                    this.MenuDesignerCtrl.PointToClient(PasteToolStripMenuItem.Owner.Location)
                  );

                 //if (aControl == this.MenuDesignerCtrl.GetMenuBranchesTreeView())
                 //{
                 //    int i = 0;
                 //    i++;
                 //}
                 if (IsRootBranch(oSelectedNode) && aControl == this.MenuDesignerCtrl.GetCommandsTreeView())
                 {
                     // no paste can be done on a root branch.
                     return;
                 }
             }
            MenuDesignUIItem aMenuDesignUIItem = this.PasteToolStripMenuItem.Tag as MenuDesignUIItem;
            if
                (
                aMenuDesignUIItem == null ||
                aMenuDesignUIItem.MenuItem == null ||
                !aMenuDesignUIItem.IsDesignSelected
                )
                return;

			BaseMenuItem cutItem = MenuModel.GetMenuItemFromClipboard();
			if (cutItem == null)
				return;

			bool ok = true;
			if (lastOperationCutItem != null)
				ok = currentMenuModel.RemoveMenuItem(cutItem);

			if (!ok)
				return;

			this.MenuDesignerCtrl.RemoveMenuItem(lastOperationCutItem);

			// Imposto i nome uguali a stringa vuota in modo da forzare il
			// naming automatico. I figli invece possono conservare il nome, tanto se cambio il nome alla radice del ramo che
			//sto copiando i loro nomi saranno comunque univoci.
			cutItem.Name = String.Empty;

			ok = currentMenuModel.AddMenuItem(cutItem, aMenuDesignUIItem.MenuItem, null, lastOperationCutItem == null);
			if (!ok)
				return;

			ok = PasteFromClipboard(cutItem, aMenuDesignUIItem);
			if (!ok)
				return;

			if (lastOperationCutItem != null)
				MenuDesignerControl.EmptyClipboard();

			SetCutOperation(lastOperationCutItem, false);
			lastOperationCutItem = null;

			IsDirty = true;
        }

		//---------------------------------------------------------------------
		private static void ClearChildrenNames(BaseMenuItem cutItem)
		{
			if (cutItem == null)
				return;

			cutItem.Name = String.Empty;

			IContainer container = cutItem as IContainer;

			if (container == null)
				return;

			foreach (BaseMenuItem menuItem in container.Components)
			{
				menuItem.Name = String.Empty;
				ClearChildrenNames(menuItem);
			}
		}

		//---------------------------------------------------------------------
        private void DeleteMenuItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteMenuDesignUIItem(this.DeleteMenuItemToolStripMenuItem.Tag as MenuDesignUIItem);
        }

		//---------------------------------------------------------------------
        private void ShowPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMenuDesignUIItemProperties(this.ShowPropertiesToolStripMenuItem.Tag as MenuDesignUIItem);
        }

		//---------------------------------------------------------------------
        private void MenuDesignerCtrl_MenuItemMoved(object sender, MenuDesignUIItemEventArgs args)
        {
			if (
				args.WorkingMenuDesignUIItem != null &&
				args.WorkingMenuDesignUIItem.MenuItem != null &&
				currentMenuModel != null
				)
            {
				currentMenuModel.MoveMenuItem(args.WorkingMenuDesignUIItem.MenuItem, args.TargetMenuItem, args.MenuObjectBefore);
            }

			IsDirty = true;
        }

		//---------------------------------------------------------------------
		/// <remarks />
		public bool AskAndSaveAndClose(CancelEventArgs e)
		{
			if (!IsDirty)
				return true;

			using (SaveMenu sm = new SaveMenu(BaseCustomizationContext.CustomizationContextInstance.IsCurrentEasyBuilderAppAStandardization))
			{
				switch (sm.ShowDialog())
				{
					case DialogResult.Cancel:
						e.Cancel = true;
						return false;
					case DialogResult.Yes:
					case DialogResult.OK:
						//Per le standardizzazioni il menu è già pubblicato quindi il flag sm.PublishMenu
						//è a false perchè non ha senso pubblicare un file già pubblicato.
						SaveCurrentMenuFile(sm.PublishMenu);
						return true;
					default:
						IsDirty = false;
						return false;
				}
			}
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Sets the dirty flag to allow saving modifications
		/// </summary>
		public void SetDirty(bool dirty)
		{
			if (isDirty != dirty)
			{
				isDirty = dirty;
				OnDirtyChanged();
			}
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Suspends dirty changes
		/// </summary>
		public bool SuspendDirtyChanges
		{
			get
			{
				return this.suspendDirtyChanges;
			}
			set
			{
				this.suspendDirtyChanges = value;
			}
		}
	}
}
