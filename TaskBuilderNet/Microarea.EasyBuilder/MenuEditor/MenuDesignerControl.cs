using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microarea.EasyBuilder.Properties;
using Microarea.EasyBuilder.UI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	/// <remarks/>
	internal partial class MenuDesignerControl : System.Windows.Forms.UserControl
    {
		public event EventHandler<MenuDesignUIItemEventArgs> MenuItemMoved;
		
        public event EventHandler ApplicationsPanelCleared;
        public event EventHandler MenuBranchesTreeViewNodesCleared;
        public event EventHandler CommandsTreeViewNodesCleared;
        public event ControlEventHandler ApplicationPanelRemoved;
        public event ControlEventHandler GroupLinkLabelRemoved;

        private string menuFileFullPath = String.Empty;
        private DirectoryInfo menuDirectoryInfo;

        private CollapsiblePanelLinkLabel currentSelectedGroupLabel;
      
        private bool refreshApplicationsPanelAfterDagOver;
        private CollapsiblePanel collapsiblePanelToRefreshAfterDagOver;
		private MenuEditorTreeNode treeNodeToRefreshAfterDagOver;

        private MenuDesignUIItem currentDesignSelectedItem;
		private ISelectionService selectionService;
		private IUIService uiService;

        Color m_oErrorColor = Color.DarkOrange;

        Color m_oOkColor = Color.White;

        Color m_oOkGroupColor = Color.Transparent;

        public Color OkGroupColor
        {
            get { return m_oOkGroupColor; }
            set { m_oOkGroupColor = value; }
        }

        public Color OkColor
        {
            get { return m_oOkColor; }
            set { m_oOkColor = value; }
        }

        public Color ErrorColor
        {
            get { return m_oErrorColor; }
            set { m_oErrorColor = value; }
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		public ISelectionService SelectionService
		{
			get { return selectionService; }
			set { selectionService = value; }
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public IUIService UiService
		{
			get { return uiService; }
			set { uiService = value; }
		}

        //---------------------------------------------------------------------
		/// <remarks/>
        public MenuDesignerControl()
        {
            InitializeComponent();

            this.ApplicationsPanel.KeyUp += new KeyEventHandler(ApplicationsPanel_KeyUp);

			CommandsTreeView.Font = StaticResources.CustomizableItemFont;
			CommandsTreeView.ImageList = ImageLists.MenuCommandTree;

			MenuBranchesTreeView.Font = StaticResources.CustomizableItemFont;
			MenuBranchesTreeView.ImageList = ImageLists.MenuBranchTree;
        }

        //---------------------------------------------------------------------
		internal MenuEditorTreeNode InsertMenuBranch(
			MenuBranch aMenuToAdd,
			TreeNode parentTreeNode,
			TreeNode nodeBefore
			)
        {
			if (aMenuToAdd == null || currentSelectedGroupLabel == null)
                return null;

            TreeNodeCollection targetNodes = (parentTreeNode != null) ? parentTreeNode.Nodes : this.MenuBranchesTreeView.Nodes;

			MenuEditorTreeNode menuTreeNode = new MenuEditorTreeNode(aMenuToAdd);


            if (aMenuToAdd.Menus != null && aMenuToAdd.Menus.Count > 0)
            {
                foreach (MenuBranch submenu in aMenuToAdd.Menus)
                    AddMenuBranch(submenu, menuTreeNode);
            }           

            if (nodeBefore == null || parentTreeNode == nodeBefore.Parent)
                targetNodes.Insert((nodeBefore != null) ? (nodeBefore.Index + 1) : 0, menuTreeNode);
            else
                targetNodes.Add(menuTreeNode);

            MenuEditorTreeNode oParentNode = parentTreeNode as MenuEditorTreeNode;
            if (oParentNode != null) 
            {
               bool bIsValid =  oParentNode.ValidateStructure(oParentNode.Parent as MenuEditorTreeNode);
               if (bIsValid)
               {
                   oParentNode.BackColor = m_oOkColor;
               }
               else 
               {
                   oParentNode.BackColor = m_oErrorColor;
               }
            }
			return menuTreeNode;
        }

        //---------------------------------------------------------------------
		private MenuEditorTreeNode AddMenuBranch(MenuBranch aMenuToAdd, MenuEditorTreeNode parentTreeNode)
        {
			MenuEditorTreeNode nodeBefore = null;
            if (parentTreeNode == null)
            {
                if (this.MenuBranchesTreeView.Nodes != null && this.MenuBranchesTreeView.Nodes.Count > 0)
					nodeBefore = this.MenuBranchesTreeView.Nodes[this.MenuBranchesTreeView.Nodes.Count - 1] as MenuEditorTreeNode;
            }
            else
				nodeBefore = parentTreeNode.LastNode as MenuEditorTreeNode;

            MenuEditorTreeNode oAddedNode = InsertMenuBranch(aMenuToAdd, parentTreeNode, nodeBefore);

            bool bIsValid = oAddedNode.ValidateStructure(parentTreeNode);
            if (!bIsValid) 
            {
                oAddedNode.BackColor = m_oErrorColor;
            }
            return oAddedNode;
        }
        
        //---------------------------------------------------------------------
		private MenuEditorTreeNode MoveMenuBranch(MenuBranch aMenuToMove, TreeNode parentTreeNode, TreeNode nodeBefore)
        {
            if (aMenuToMove == null)
                return null;

            // Cerco il nodo corrispondente e, se lo trovo, lo rimuovo e reinserisco dove 
            // lo si vuole spostare
			MenuEditorTreeNode treeNodeToRemove = FindMenuBranchTreeNode(aMenuToMove);
            if (treeNodeToRemove == null)
                return null;

            if (parentTreeNode != null)
            {
                // Devo controllare che non si stia per spostare un ramo di menù in se stesso
                // o in uno dei suoi stessi sottorami: non è un'operazione consentita!
				MenuEditorTreeNode nodeAncestor = parentTreeNode as MenuEditorTreeNode;
                while (nodeAncestor != null)
                {
                    if (treeNodeToRemove == nodeAncestor)
                    {
						uiService.ShowMessage(
							MenuEditorStrings.InvalidMenuBranchMoveOperation,
							NameSolverStrings.EasyStudioDesigner,
							MessageBoxButtons.OK
							);

                        return null;
                    }
					nodeAncestor = nodeAncestor.Parent as MenuEditorTreeNode;
                }
            }

            treeNodeToRemove.Remove();

			MenuEditorTreeNode movedMenuBranchNode = InsertMenuBranch(aMenuToMove, parentTreeNode, nodeBefore);

            if (movedMenuBranchNode != null)
            {
                Cursor currentCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                this.MenuBranchesTreeView.SelectedNode = movedMenuBranchNode;

                this.MenuBranchesTreeView.Focus();

                this.Cursor = currentCursor;
            }
            return movedMenuBranchNode;
        }

        //---------------------------------------------------------------------
        private MenuEditorTreeNode MoveMenuBranchAsLast(MenuBranch aMenuToMove, MenuEditorTreeNode parentTreeNode)
        {
			MenuEditorTreeNode nodeBefore = null;
            if (parentTreeNode == null)
            {
                if (this.MenuBranchesTreeView.Nodes != null && this.MenuBranchesTreeView.Nodes.Count > 0)
					nodeBefore = this.MenuBranchesTreeView.Nodes[this.MenuBranchesTreeView.Nodes.Count - 1] as MenuEditorTreeNode;
            }
            else
				nodeBefore = parentTreeNode.LastNode as MenuEditorTreeNode;

            return MoveMenuBranch(aMenuToMove, parentTreeNode, nodeBefore);
        }

        //---------------------------------------------------------------------
        private void AddMenuCommands(MenuBranch aCommandsParentMenu)
        {
            this.CommandsTreeView.Nodes.Clear();

            if (aCommandsParentMenu == null)
                return;

            if (aCommandsParentMenu.Commands == null || aCommandsParentMenu.Commands.Count == 0)
                return;

            foreach (MenuCommand aCommandToAdd in aCommandsParentMenu.Commands)
                AddMenuCommand(aCommandToAdd);
        }
        
        //---------------------------------------------------------------------
		internal MenuEditorTreeNode InsertMenuCommand(BaseMenuItem aMenuCommandToInsert, TreeNode nodeBefore)
        {
            if (aMenuCommandToInsert == null)
                return null;

			MenuEditorTreeNode commandTreeNode = new MenuEditorTreeNode(aMenuCommandToInsert);

            this.CommandsTreeView.Nodes.Insert((nodeBefore != null) ? (nodeBefore.Index + 1) : 0, commandTreeNode);

			return commandTreeNode;
        }

        //---------------------------------------------------------------------
        private MenuEditorTreeNode AddMenuCommand(MenuCommand aMenuCommandToAdd)
        {
			MenuEditorTreeNode nodeBefore = null;
            if (this.CommandsTreeView.Nodes != null && this.CommandsTreeView.Nodes.Count > 0)
				nodeBefore = this.CommandsTreeView.Nodes[this.CommandsTreeView.Nodes.Count - 1] as MenuEditorTreeNode;

            return InsertMenuCommand(aMenuCommandToAdd, nodeBefore);
        }
        
        //---------------------------------------------------------------------
        private System.Drawing.Image GetMenuItemImage(BaseMenuItem baseMenuItem)
        {
			if (baseMenuItem == null)
                return null;

			if (!String.IsNullOrEmpty(baseMenuItem.ImageFilePath) && File.Exists(baseMenuItem.ImageFilePath))
				return ImagesHelper.LoadBitmapWithoutLockFile(baseMenuItem.ImageFilePath);

			if (menuDirectoryInfo == null)
				return null;

            FileInfo menuItemImageFileInfo = null;
			FileInfo[] fileInfos = menuDirectoryInfo.GetFiles(baseMenuItem.Name + ".*");

            if (fileInfos != null && fileInfos.Length > 0)
            {
                foreach (FileInfo aFileInfo in fileInfos)
                {
					if (!MenuXmlParser.IsImageFileSupported(aFileInfo))
						continue;

                    // Occorre controllare che il nome del file corrisponda
                    // effettivamente a <imgFileName>.<ext>. Infatti, avendo
                    // i nomi dei file di immagine generalmente la forma di
                    // un namespace, possono contenere a loro volta dei punti
                    // e, quindi, si possono incontrare file del tipo
                    // <imgFileName>.<restante_parte_di_namespace>.<ext>
					if (String.Compare(baseMenuItem.Name + aFileInfo.Extension, aFileInfo.Name, true, CultureInfo.InstalledUICulture) == 0)
                    {
                        menuItemImageFileInfo = aFileInfo;
                        break;
                    }
                }
            }

			if (menuItemImageFileInfo != null)
				return ImagesHelper.LoadBitmapWithoutLockFile(menuItemImageFileInfo.FullName);

            return null;
        }

        //---------------------------------------------------------------------
        internal void SelectGroupLabel(CollapsiblePanelLinkLabel aGroupLabel)
        {
            if 
                (
                aGroupLabel == null || 
                aGroupLabel.MenuGroup == null ||
                currentSelectedGroupLabel == aGroupLabel
                )
                return;

            ClearMenuBranchesTreeView();

            currentSelectedGroupLabel = aGroupLabel;
            if (!currentSelectedGroupLabel.IsDesignSelected)
                return;

            MenuGroup group = aGroupLabel.MenuGroup;
            if (group == null || group.Menus == null || group.Menus.Count == 0)
                return;

            foreach (MenuBranch aMenuBranch in group.Menus)
            {
                if (aMenuBranch == null)
                    continue;
                
                AddMenuBranch(aMenuBranch, null);
            }
        }

        //---------------------------------------------------------------------
        private void ClearMenuBranchesTreeView()
        {
            if (this.MenuBranchesTreeView == null || this.MenuBranchesTreeView.IsDisposed || this.MenuBranchesTreeView.Nodes.Count == 0)
                return;

            ClearCommandsTreeView();

            if
                (
                currentDesignSelectedItem != null &&
                currentDesignSelectedItem.TreeNode != null &&
                currentDesignSelectedItem.TreeNode.TreeView == this.MenuBranchesTreeView
                )
            {
                currentDesignSelectedItem = null;

				OnDesignSelectionChanged(MenuDesignUIItemEventArgs.Empty);
            }

            this.MenuBranchesTreeView.Nodes.Clear();
            
            if (MenuBranchesTreeViewNodesCleared != null)
                MenuBranchesTreeViewNodesCleared(this, EventArgs.Empty);
        }

        //---------------------------------------------------------------------
        private void ClearCommandsTreeView()
        {
            if (this.CommandsTreeView == null || this.CommandsTreeView.IsDisposed || this.CommandsTreeView.Nodes.Count == 0)
                return;

            if
                (
                currentDesignSelectedItem != null &&
                currentDesignSelectedItem.TreeNode != null &&
                currentDesignSelectedItem.TreeNode.TreeView == this.CommandsTreeView
                )
            {
                currentDesignSelectedItem = null;

				OnDesignSelectionChanged(MenuDesignUIItemEventArgs.Empty);
            }

            this.CommandsTreeView.Nodes.Clear();

            if (CommandsTreeViewNodesCleared != null)
                CommandsTreeViewNodesCleared(this, EventArgs.Empty);
        }

		//---------------------------------------------------------------------
		private void OnDesignSelectionChanged(MenuDesignUIItemEventArgs args)
		{
			object[] newSelection = null;
			if (args != null && args.WorkingMenuDesignUIItem != null && args.WorkingMenuDesignUIItem.MenuItem != null)
				newSelection = new object[] { args.WorkingMenuDesignUIItem.MenuItem };

			selectionService.SetSelectedComponents(newSelection);
		}

        //---------------------------------------------------------------------
        private CollapsiblePanel FindApplicationPanel(MenuApplication aMenuApplication)
        {
            if
                (
                aMenuApplication == null ||
                this.ApplicationsPanel == null ||
                this.ApplicationsPanel.Panels == null ||
                this.ApplicationsPanel.PanelsCount == 0
                )
                return null;
            foreach (CollapsiblePanel aPanel in this.ApplicationsPanel.Panels)
            {
				if (aPanel != null && aPanel.MenuApplication == aMenuApplication)
                    return aPanel;
            }
            return null;
        }

        //---------------------------------------------------------------------
        public CollapsiblePanelLinkLabel FindGroupLinkLabel(MenuGroup aGroup)
        {
            if
                (
                aGroup == null ||
                this.ApplicationsPanel == null ||
                this.ApplicationsPanel.Panels == null ||
                this.ApplicationsPanel.PanelsCount == 0
                )
                return null;

            foreach (CollapsiblePanel aPanel in this.ApplicationsPanel.Panels)
            {
				if (aPanel != null && aPanel.MenuApplication != null)
                {
                    foreach (Control aPanelControl in aPanel.Controls)
                    {
						CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = aPanelControl as CollapsiblePanelLinkLabel;
						if (aCollapsiblePanelLinkLabel == null )
                            continue;

						if (aCollapsiblePanelLinkLabel.MenuGroup == aGroup)
							return aCollapsiblePanelLinkLabel;
                    }
                }
            }
            return null;
        }

        //---------------------------------------------------------------------
        private MenuDesignUIItem FindGroupByName(string groupNameToSearch)
        {
            if
                (
                this.ApplicationsPanel == null ||
                this.ApplicationsPanel.Panels == null ||
                this.ApplicationsPanel.PanelsCount == 0 ||
                String.IsNullOrEmpty(groupNameToSearch)
                )
                return null;

            foreach (CollapsiblePanel aPanel in this.ApplicationsPanel.Panels)
            {
                if (aPanel != null && aPanel.MenuApplication != null)
                {
                    foreach (Control aPanelControl in aPanel.Controls)
                    {
						CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = aPanelControl as CollapsiblePanelLinkLabel;
						if (aCollapsiblePanelLinkLabel == null)
							continue;
                        
                        if (String.Compare(aCollapsiblePanelLinkLabel.MenuGroup.Name, groupNameToSearch) == 0)
							return new MenuDesignUIItem(aCollapsiblePanelLinkLabel);
                    }
                }
            }
            return null;
        }

        //---------------------------------------------------------------------
		public MenuEditorTreeNode FindMenuBranchTreeNode(MenuBranch aMenuBranch)
        {
            if (aMenuBranch == null || this.MenuBranchesTreeView == null)
                return null;
            
            return FindMenuBranchTreeNode(aMenuBranch, this.MenuBranchesTreeView.Nodes);
        }

        //---------------------------------------------------------------------
		private MenuEditorTreeNode FindMenuBranchTreeNode(MenuBranch aMenuBranch, TreeNodeCollection branchNodesToSearch)
        {
            if (aMenuBranch == null || branchNodesToSearch == null || branchNodesToSearch.Count == 0)
                return null;

            foreach (MenuEditorTreeNode aMenuBranchNode in branchNodesToSearch)
            {
                if (aMenuBranchNode == null || aMenuBranchNode.MenuItem == null)
                    continue;

				if (aMenuBranchNode.MenuItem == aMenuBranch)
                    return aMenuBranchNode;

				MenuEditorTreeNode subNodeFound = FindMenuBranchTreeNode(aMenuBranch, aMenuBranchNode.Nodes);
                if (subNodeFound != null)
                    return subNodeFound;
            }

            return null;
        }
        
        //---------------------------------------------------------------------
        private MenuBranch FindMenuBranchByName(string menuBranchNameToSearch)
        {
            if
                (
                this.ApplicationsPanel == null ||
                this.ApplicationsPanel.Panels == null ||
                this.ApplicationsPanel.PanelsCount == 0 ||
                String.IsNullOrEmpty(menuBranchNameToSearch)
                )
                return null;

            foreach (CollapsiblePanel aPanel in this.ApplicationsPanel.Panels)
            {
				if (aPanel != null && aPanel.MenuApplication != null)
                {
                    foreach (Control aPanelControl in aPanel.Controls)
                    {
						CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = aPanelControl as CollapsiblePanelLinkLabel;
						if (aCollapsiblePanelLinkLabel == null)
                            continue;

                        if
                            (aCollapsiblePanelLinkLabel.MenuGroup != null &&
							aCollapsiblePanelLinkLabel.MenuGroup.Menus != null &&
							aCollapsiblePanelLinkLabel.MenuGroup.Menus.Count > 0 
                            )
                        {
							MenuBranch menuBranchFound = FindMenuBranchByName(menuBranchNameToSearch, aCollapsiblePanelLinkLabel.MenuGroup.Menus);
                            if (menuBranchFound != null)
                                return menuBranchFound;
                        }
                    }
                }
            }
            return null;
        }

        //---------------------------------------------------------------------
		private MenuBranch FindMenuBranchByName(string menuBranchNameToSearch, List<MenuBranch> branchNodesToSearch)
        {
            if (String.IsNullOrEmpty(menuBranchNameToSearch) || branchNodesToSearch == null || branchNodesToSearch.Count == 0)
                return null;

            foreach (MenuBranch aMenuBranch in branchNodesToSearch)
            {
                if (aMenuBranch == null)
                    continue;

                if (String.Compare(aMenuBranch.Name, menuBranchNameToSearch) == 0)
                    return aMenuBranch;

                MenuBranch subMenuFound = FindMenuBranchByName(menuBranchNameToSearch, aMenuBranch.Menus);
                if (subMenuFound != null)
                    return subMenuFound;
            }

            return null;
        }

        //---------------------------------------------------------------------
		private MenuEditorTreeNode FindAndUpdateCommandTreeNode(MenuCommand aMenuCommand)
        {
            if 
                (
                aMenuCommand == null || 
                this.CommandsTreeView == null || 
                this.CommandsTreeView.Nodes == null || 
                this.CommandsTreeView.Nodes.Count == 0
                )
                return null;

			foreach (MenuEditorTreeNode aCommandNode in this.CommandsTreeView.Nodes)
            {
				if (aCommandNode != null && aCommandNode.MenuItem == aMenuCommand)
				{
					aCommandNode.Text = (aMenuCommand.Title != null) ? aMenuCommand.Title.ToString() : null;
					return aCommandNode;
				}
            }

            return null;
        }

        //---------------------------------------------------------------------
        private DragDropEffects GetApplicationsPanelDragDropEffect(IDataObject data, DragDropEffects allowedEffect)
        {
            if (data == null)
                return DragDropEffects.None;

            Point currentMousePos = Control.MousePosition;
            CollapsiblePanel collapsiblePanelAtPoint = this.ApplicationsPanel.GetChildAtPoint(this.ApplicationsPanel.PointToClient(currentMousePos)) as CollapsiblePanel;
            
            if (data.GetDataPresent(typeof(MenuApplication)))
            {
                if (collapsiblePanelAtPoint != null)
                    return DragDropEffects.None;

                // Non sono su un CollapsiblePanel => mi "metto" appena sotto a quello precedente
                int feedbackYOffset = CollapsiblePanel.LabelsYSpacing / 2;
                Point appPanelClientMousePos = this.ApplicationsPanel.PointToClient(currentMousePos);

                foreach (Control aChildControl in this.ApplicationsPanel.Controls)
                {
                    if (aChildControl != null && aChildControl is CollapsiblePanel)
                    {
                        if (aChildControl.Bottom <= appPanelClientMousePos.Y && aChildControl.Bottom >= feedbackYOffset)
                            feedbackYOffset = aChildControl.Bottom;
                    }
                }
                feedbackYOffset += CollapsiblePanel.LabelsYSpacing / 2;

                Rectangle feedbackRect = new Rectangle(4, feedbackYOffset, this.ApplicationsPanel.Width - 8, 2);
                Region outerRegion = new Region(this.ApplicationsPanel.ClientRectangle);
                outerRegion.Exclude(feedbackRect);
                this.ApplicationsPanel.Invalidate(outerRegion);

                Application.DoEvents();

                Graphics g = this.ApplicationsPanel.CreateGraphics();
                ControlPaint.DrawFocusRectangle(g, feedbackRect);
                g.Dispose();

                refreshApplicationsPanelAfterDagOver = true;

                return allowedEffect;
            }

            if (data.GetDataPresent(typeof(MenuGroup)))
            {
                if (collapsiblePanelAtPoint == null)
                    return DragDropEffects.None;

                if (collapsiblePanelAtPoint.IsExpanded)
                {
                    int feedbackYOffset = 2;
                    Point panelClientMousePos = collapsiblePanelAtPoint.PointToClient(currentMousePos);
                    CollapsiblePanelLinkLabel groupLabelAtPoint = collapsiblePanelAtPoint.GetChildAtPoint(panelClientMousePos) as CollapsiblePanelLinkLabel;
                    if (groupLabelAtPoint == null)
                    {
                        // Non sono su una CollapsiblePanelLinkLabel => mi "metto" appena sotto a quella precedente
                        foreach(Control aChildControl in collapsiblePanelAtPoint.Controls)
                        {
                            if (aChildControl != null && aChildControl is CollapsiblePanelLinkLabel)
                            {
                                if (aChildControl.Bottom <= panelClientMousePos.Y && aChildControl.Bottom >= feedbackYOffset)
                                    feedbackYOffset = aChildControl.Bottom;
                            }
                        }
                        feedbackYOffset += 2;
                    }
                    else
                    {
                        // Sono su una CollapsiblePanelLinkLabel => mi "metto" appena sopra
                        feedbackYOffset = groupLabelAtPoint.Top - 2;
                    }

                    Rectangle feedbackRect = new Rectangle(4,feedbackYOffset,collapsiblePanelAtPoint.Width - 8, 2);
                    Region outerRegion = new Region(collapsiblePanelAtPoint.ClientRectangle);
                    outerRegion.Exclude(feedbackRect);
                    collapsiblePanelAtPoint.Invalidate(outerRegion);

                    Application.DoEvents();

                    Graphics g = collapsiblePanelAtPoint.CreateGraphics();
                    ControlPaint.DrawFocusRectangle(g, feedbackRect);
                    g.Dispose();

                    collapsiblePanelToRefreshAfterDagOver = collapsiblePanelAtPoint;
                }

                return allowedEffect;
            }

            return DragDropEffects.None;
        }

        //---------------------------------------------------------------------
        private DragDropEffects GetGroupLabelDragDropEffect(CollapsiblePanelLinkLabel aGroupLabel, IDataObject data, DragDropEffects allowedEffect)
        {
            if 
                (
                aGroupLabel == null || 
                aGroupLabel.Parent == null ||
                !(aGroupLabel.Parent is CollapsiblePanel) ||
                data == null || 
                !data.GetDataPresent(typeof(MenuGroup))
                )
                return DragDropEffects.None;

            CollapsiblePanel parentPanel = aGroupLabel.Parent as CollapsiblePanel;

            // Sono su una CollapsiblePanelLinkLabel => mi "metto" appena sopra
            Rectangle feedbackRect = new Rectangle(4, aGroupLabel.Top - 2, parentPanel.Width - 8, 2);
            Region outerRegion = new Region(parentPanel.ClientRectangle);
            outerRegion.Exclude(feedbackRect);
            parentPanel.Invalidate(outerRegion);

            Application.DoEvents();

            Graphics g = parentPanel.CreateGraphics();
            ControlPaint.DrawFocusRectangle(g, feedbackRect);
            g.Dispose();

            collapsiblePanelToRefreshAfterDagOver = parentPanel;

            return allowedEffect;
        }

        //---------------------------------------------------------------------
        private DragDropEffects GetMenuBranchesTreeViewDragDropEffect(IDataObject data, DragDropEffects allowedEffect)
        {
            if (data == null || currentSelectedGroupLabel == null)
                return DragDropEffects.None;

            if
               (
               treeNodeToRefreshAfterDagOver != null &&
               treeNodeToRefreshAfterDagOver.TreeView != null &&
               !treeNodeToRefreshAfterDagOver.TreeView.IsDisposed &&
               treeNodeToRefreshAfterDagOver.Bounds != Rectangle.Empty
               )
            {
                Rectangle nodeOuterRect = new Rectangle(0, treeNodeToRefreshAfterDagOver.Bounds.Top, treeNodeToRefreshAfterDagOver.TreeView.Width, treeNodeToRefreshAfterDagOver.Bounds.Height);
                nodeOuterRect.Inflate(0, 2);
                treeNodeToRefreshAfterDagOver.TreeView.Invalidate(nodeOuterRect);

                Application.DoEvents();
            }
            Point currentMouseClientPos = this.MenuBranchesTreeView.PointToClient(Control.MousePosition);
			MenuEditorTreeNode menuBranchAtPoint = this.MenuBranchesTreeView.GetNodeAt(currentMouseClientPos) as MenuEditorTreeNode;

            treeNodeToRefreshAfterDagOver = menuBranchAtPoint;
            
            if (data.GetDataPresent(typeof(MenuBranch)))
            {

                MenuBranch draggedMenuBranch = data.GetData(typeof(MenuBranch)) as MenuBranch;
                if (menuBranchAtPoint == null && draggedMenuBranch.Menus.Count == 0)
                {
                    // it would mean a new root without children,
                    // which would violate the 4 level structure.
                    return DragDropEffects.None;
                }

                if (draggedMenuBranch == null || (draggedMenuBranch.Menus.Count > 0 && menuBranchAtPoint != null))
                {
                    // this is moving a root with children 
                    // as new child of another node,
                    // which would violate the 4 level structure.
                    return DragDropEffects.None;
                }

                if (menuBranchAtPoint == null)
                    return allowedEffect;

                if (menuBranchAtPoint.Parent != null)
                {
                    // it would mean having a root with no leaf children,
                    // which would violate the 4 level structure.
                    return DragDropEffects.None;
                }
                MenuBranch oParentBranch = FindParentBranch(draggedMenuBranch);


                if (oParentBranch != null && oParentBranch.Menus.Count == 1)
                {
                    // the to be moved node's parent has just that child, can not 
                    // move it or the four level structure would be altered.
                    return DragDropEffects.None;
                }
                // Se sono nella parte alta di un nodo di menù, il nuovo branch va inserito sopra a 
                // quello su cui mi trovo, se sono nella parte bassa sotto, altrimenti va aggiunto
                // come suo ultimo sottonodo
                if 
                   (
                   menuBranchAtPoint.Bounds.Top + menuBranchAtPoint.Bounds.Height / 3 > currentMouseClientPos.Y ||
                   menuBranchAtPoint.Bounds.Bottom - menuBranchAtPoint.Bounds.Height / 3 < currentMouseClientPos.Y
                   )
                {
                    int feedbackYOffset = 0;
					MenuEditorTreeNode referenceNode = menuBranchAtPoint;
                    if (menuBranchAtPoint.Bounds.Top + menuBranchAtPoint.Bounds.Height / 3 > currentMouseClientPos.Y)
                    {
                        if (menuBranchAtPoint.PrevVisibleNode != null)
                            referenceNode = menuBranchAtPoint.PrevVisibleNode as MenuEditorTreeNode;
                        feedbackYOffset = menuBranchAtPoint.Bounds.Top;
                    }
                    else
                        feedbackYOffset = menuBranchAtPoint.Bounds.Bottom;

                    Rectangle feedbackRect = new Rectangle(referenceNode.Bounds.Left, feedbackYOffset, referenceNode.Bounds.Width, 2);

                    Graphics treeViewGraphics = this.MenuBranchesTreeView.CreateGraphics();
                    ControlPaint.DrawFocusRectangle(treeViewGraphics, feedbackRect);
                    treeViewGraphics.Dispose();
                }
                else
                {
                    // Sono su un nodo di menù => il nuovo branch va inserito come sotto-nodo 
                    // di quello su cui mi trovo

                    // Devo controllare che non si stia per spostare un ramo di menù in
                    // se stesso o in uno dei suoi stessi sottorami: non è un'operazione
                    // consentita!
                    if ((allowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                    {
                        // MenuBranch draggedMenuBranch = data.GetData(typeof(MenuBranch)) as MenuBranch;
                        if (draggedMenuBranch != null)
                        {

                            if (draggedMenuBranch.Menus.Count > 0)
                            {
                                // the dragged node has more than one child, it can not be moved 
                                // to become a new root child.
                                return DragDropEffects.None;
                            }
                            
							MenuEditorTreeNode menuBranchTreeNodeToMove = FindMenuBranchTreeNode(draggedMenuBranch);
                            if (menuBranchTreeNodeToMove != null)
                            {
								MenuEditorTreeNode nodeAncestor = menuBranchAtPoint;
                                
                                while (nodeAncestor != null)
                                {
                                    if (menuBranchTreeNodeToMove == nodeAncestor)
                                        return DragDropEffects.None;
									nodeAncestor = nodeAncestor.Parent as MenuEditorTreeNode;
                                }
                            }
                        }
                    }

                    Graphics treeViewGraphics = this.MenuBranchesTreeView.CreateGraphics();
                    ControlPaint.DrawFocusRectangle(treeViewGraphics, menuBranchAtPoint.Bounds);
                    treeViewGraphics.Dispose();
                }

                return allowedEffect;
            }

            if (
                data.GetDataPresent(typeof(DocumentMenuCommand)) ||
                data.GetDataPresent(typeof(BatchMenuCommand)) ||
                data.GetDataPresent(typeof(ReportMenuCommand)) ||
                data.GetDataPresent(typeof(FunctionMenuCommand)) ||
                data.GetDataPresent(typeof(TextMenuCommand)) ||
                data.GetDataPresent(typeof(ExeMenuCommand)) ||
                data.GetDataPresent(typeof(OfficeItemMenuCommand))
                )
            {
                if (menuBranchAtPoint == null || menuBranchAtPoint.Parent == null)
                {
                    return DragDropEffects.None;
                }

                Graphics treeViewGraphics = this.MenuBranchesTreeView.CreateGraphics();
                ControlPaint.DrawFocusRectangle(treeViewGraphics, menuBranchAtPoint.Bounds);
                treeViewGraphics.Dispose();

                return allowedEffect;
            }

            return DragDropEffects.None;
        }

        //---------------------------------------------------------------------
        private DragDropEffects GetCommandsTreeViewDragDropEffect(IDataObject data, DragDropEffects allowedEffect)
        {

            if (data == null || this.MenuBranchesTreeView.SelectedNode == null)
                return DragDropEffects.None;

            if
               (
               treeNodeToRefreshAfterDagOver != null &&
               treeNodeToRefreshAfterDagOver.TreeView != null &&
               !treeNodeToRefreshAfterDagOver.TreeView.IsDisposed &&
               treeNodeToRefreshAfterDagOver.Bounds != Rectangle.Empty
               )
            {
                Rectangle nodeOuterRect = new Rectangle(0, treeNodeToRefreshAfterDagOver.Bounds.Top, treeNodeToRefreshAfterDagOver.TreeView.Width, treeNodeToRefreshAfterDagOver.Bounds.Height);
                nodeOuterRect.Inflate(0, 2);
                treeNodeToRefreshAfterDagOver.TreeView.Invalidate(nodeOuterRect);

                Application.DoEvents();
            }
            
            Point currentMouseClientPos = this.CommandsTreeView.PointToClient(Control.MousePosition);
			MenuEditorTreeNode commandAtPoint = this.CommandsTreeView.GetNodeAt(currentMouseClientPos) as MenuEditorTreeNode;
          
            treeNodeToRefreshAfterDagOver = commandAtPoint;
            
            if (
                data.GetDataPresent(typeof(DocumentMenuCommand)) ||
                data.GetDataPresent(typeof(BatchMenuCommand)) ||
                data.GetDataPresent(typeof(ReportMenuCommand)) ||
                data.GetDataPresent(typeof(FunctionMenuCommand)) ||
                data.GetDataPresent(typeof(TextMenuCommand)) ||
                data.GetDataPresent(typeof(ExeMenuCommand)) ||
                data.GetDataPresent(typeof(OfficeItemMenuCommand))
                )
            {
                if (commandAtPoint == null)
                    return allowedEffect;

                // Se sono nella parte alta di un nodo di comando, il nuovo comando va inserito sopra a 
                // quello su cui mi trovo, altrimenti sotto
                int feedbackYOffset = 0;
                if (commandAtPoint.Bounds.Top + commandAtPoint.Bounds.Height / 2 > currentMouseClientPos.Y)
                    feedbackYOffset = commandAtPoint.Bounds.Top;
                else
                    feedbackYOffset = commandAtPoint.Bounds.Bottom;

                Rectangle feedbackRect = new Rectangle(commandAtPoint.Bounds.Left, feedbackYOffset, commandAtPoint.Bounds.Width, 2);

                Graphics treeViewGraphics = this.CommandsTreeView.CreateGraphics();
                ControlPaint.DrawFocusRectangle(treeViewGraphics, feedbackRect);
                treeViewGraphics.Dispose();

                return allowedEffect;
            }
            return DragDropEffects.None;
        }

        //---------------------------------------------------------------------
        private void ClearDragFeedback()
        {
            if (refreshApplicationsPanelAfterDagOver)
                this.ApplicationsPanel.Refresh();
            else if (collapsiblePanelToRefreshAfterDagOver != null && !collapsiblePanelToRefreshAfterDagOver.IsDisposed)
                collapsiblePanelToRefreshAfterDagOver.Refresh();

            collapsiblePanelToRefreshAfterDagOver = null;

            refreshApplicationsPanelAfterDagOver = false;

            if
               (
               treeNodeToRefreshAfterDagOver != null &&
               treeNodeToRefreshAfterDagOver.TreeView != null &&
               !treeNodeToRefreshAfterDagOver.TreeView.IsDisposed &&
               treeNodeToRefreshAfterDagOver.Bounds != Rectangle.Empty
               )
            {
                Rectangle nodeOuterRect = new Rectangle(0, treeNodeToRefreshAfterDagOver.Bounds.Top, treeNodeToRefreshAfterDagOver.TreeView.Width, treeNodeToRefreshAfterDagOver.Bounds.Height);
                nodeOuterRect.Inflate(0, 2);
                treeNodeToRefreshAfterDagOver.TreeView.Invalidate(nodeOuterRect);
            }
            treeNodeToRefreshAfterDagOver = null;
        }

        //---------------------------------------------------------------------
        internal CollapsiblePanel InsertApplicationPanel
            (
            MenuApplication anApplicationToInsert, 
            CollapsiblePanel applicationPanelBefore
            )
        {
            if (anApplicationToInsert == null)
                return null;

            Image applicationImage = null;

            if (anApplicationToInsert.Name == null || anApplicationToInsert.Name.Length == 0) // nuova applicazione non ancora inizializzata
            {
				string newApplicationName = MenuEditorStrings.ApplicationNamePrefix;
                int applicationCounter = 1;
                foreach (Control aChildControl in this.ApplicationsPanel.Controls)
                {
					CollapsiblePanel aCollapsiblePanel = aChildControl as CollapsiblePanel;

                    if
                        (
						aCollapsiblePanel != null &&
						aCollapsiblePanel.MenuApplication != null
                        )
                    {
                        if
                            (
							aCollapsiblePanel.MenuApplication.Name != null &&
							aCollapsiblePanel.MenuApplication.Name.Length > newApplicationName.Length &&
						    aCollapsiblePanel.MenuApplication.Name.StartsWith(newApplicationName) &&
							String.Compare(aCollapsiblePanel.MenuApplication.Name.Substring(newApplicationName.Length), applicationCounter.ToString(CultureInfo.InvariantCulture)) == 0
                            )
                            applicationCounter++;
                    }
                }
                newApplicationName += applicationCounter.ToString(CultureInfo.InvariantCulture);
            }

            // Se nella cartella in cui si trova il file che ho caricato trovo un file immagine
            // che si chiama <applicationName>.<ext>, dove <ext> può essere ".bmp" 
            // o ".jpg" ecc., esso va usato per rappresentare graficamente l'applicazione
			applicationImage = GetMenuItemImage(anApplicationToInsert);

            string applicationTitle = (anApplicationToInsert.Title != null) ? anApplicationToInsert.Title.ToString() : String.Empty;

            CollapsiblePanel applicationCollapsiblePanel = this.ApplicationsPanel.InsertCollapsiblePanel(applicationTitle, applicationImage, applicationPanelBefore);
            
            applicationCollapsiblePanel.MenuApplication = anApplicationToInsert;
            applicationCollapsiblePanel.DesignSupport = true;
            applicationCollapsiblePanel.BackColor = Color.Lavender;
			applicationCollapsiblePanel.Title = applicationTitle;
           
			applicationCollapsiblePanel.State = CollapsiblePanel.PanelState.Expanded;

            applicationCollapsiblePanel.DesignSelectionChanged += new EventHandler(ApplicationCollapsiblePanel_DesignSelectionChanged);
            applicationCollapsiblePanel.CollapsiblePanelLinkLabelRemoved += new ControlEventHandler(ApplicationCollapsiblePanel_CollapsiblePanelLinkLabelRemoved);
            applicationCollapsiblePanel.InitDrag += new EventHandler(ApplicationCollapsiblePanel_InitDrag);

            if (anApplicationToInsert.Groups != null && anApplicationToInsert.Groups.Count > 0)
            {
                CollapsiblePanelLinkLabel addedGroupLabel = null;
                foreach (MenuGroup aGroupToAdd in anApplicationToInsert.Groups)
                {
                    if (aGroupToAdd == null)
                        continue;

                    addedGroupLabel = InsertGroupLinkLabel(aGroupToAdd, applicationCollapsiblePanel, addedGroupLabel);
                    // validate 4 level structure for the current group.

                    bool bValidate = aGroupToAdd.ValidateStructure();                    
                    if (!bValidate)
                    {
                        // at least one child has some validation problem.
                        addedGroupLabel.BackColor = m_oErrorColor;
                    }
                }
            }

            return applicationCollapsiblePanel;
        }
       
        //---------------------------------------------------------------------
        internal CollapsiblePanelLinkLabel InsertGroupLinkLabel(MenuGroup aGroupToInsert, CollapsiblePanel applicationCollapsiblePanel, CollapsiblePanelLinkLabel groupLinkLabelBefore)
        {
			CollapsiblePanel aCollapsiblePanel = applicationCollapsiblePanel as CollapsiblePanel;
            if 
                (
                aGroupToInsert == null ||
				aCollapsiblePanel == null ||
				aCollapsiblePanel.MenuApplication == null
                )
                return null;

            Image groupImage = null;
			if (aGroupToInsert != null)
			{
				groupImage = GetMenuItemImage(aGroupToInsert);
				if (groupImage == null)
					groupImage = Resources.DefaultGroupImage;
			}

            string groupTitle = (aGroupToInsert.Title != null) ? aGroupToInsert.Title.ToString() : String.Empty;

			CollapsiblePanelLinkLabel groupLinkLabel = aCollapsiblePanel.InsertLinkLabel(groupTitle, groupImage, groupLinkLabelBefore);
            groupLinkLabel.MenuGroup = aGroupToInsert;
            groupLinkLabel.DesignSupport = true;
            groupLinkLabel.KeyUp += new KeyEventHandler(GroupLinkLabel_KeyUp);
            groupLinkLabel.DesignSelectionChanged += new EventHandler(GroupLinkLabel_DesignSelectionChanged);
            groupLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(GroupLinkLabel_LinkClicked);
            groupLinkLabel.InitDrag += new EventHandler(GroupLinkLabel_InitDrag);
            groupLinkLabel.DragEnter += new DragEventHandler(GroupLinkLabel_DragEnter);
            groupLinkLabel.DragOver += new DragEventHandler(GroupLinkLabel_DragOver);
            groupLinkLabel.DragDrop += new DragEventHandler(GroupLinkLabel_DragDrop);

            return groupLinkLabel;
        }

		//---------------------------------------------------------------------
		// Necessario affinchè i collapsible panel delle applicazioni ricalcolino
		// correttamente la larghezza.
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
				ApplicationsPanel.UpdateAllPanelsPositions();
		}

        //---------------------------------------------------------------------
        protected override void OnKeyUp(KeyEventArgs e)
        {
            
			base.OnKeyUp(e);

            // Esc ===> Selection: selects the parent item.
            if (e.KeyCode == Keys.Escape && e.Modifiers == Keys.None)
            {
                if (currentDesignSelectedItem != null && currentDesignSelectedItem.MenuItem != null)
                {
                    if
                        (
                        currentDesignSelectedItem.MenuItem is MenuApplication &&
                        currentDesignSelectedItem.Control != null &&
                        currentDesignSelectedItem.Control is CollapsiblePanel && 
                        currentDesignSelectedItem.Control.Parent == this.ApplicationsPanel 
                        )
                    {
                        ((CollapsiblePanel)currentDesignSelectedItem.Control).IsDesignSelected = false;

                        currentDesignSelectedItem = null;

						OnDesignSelectionChanged(MenuDesignUIItemEventArgs.Empty);
                    }
                    else if
                        (
                        currentDesignSelectedItem.MenuItem is MenuGroup &&
                        currentDesignSelectedItem.Control != null &&
                        currentDesignSelectedItem.Control is CollapsiblePanelLinkLabel && 
                        currentDesignSelectedItem.Control.Parent != null && 
                        currentDesignSelectedItem.Control.Parent is CollapsiblePanel && 
                        currentDesignSelectedItem.Control.Parent.Parent == this.ApplicationsPanel
                        )
                    {
                        ((CollapsiblePanelLinkLabel)currentDesignSelectedItem.Control).IsDesignSelected = false;
                        
                        currentDesignSelectedItem = new MenuDesignUIItem((CollapsiblePanel)currentDesignSelectedItem.Control.Parent);
                        ((CollapsiblePanel)currentDesignSelectedItem.Control).IsDesignSelected = true;

                        OnDesignSelectionChanged(new MenuDesignUIItemEventArgs(currentDesignSelectedItem));
                    }
                    else if
                        (
                        currentDesignSelectedItem.MenuItem is MenuBranch &&
                        currentDesignSelectedItem.Control == this.MenuBranchesTreeView
                        )
                    {
                        this.MenuBranchesTreeView.SelectedNode = null;

                        currentDesignSelectedItem = (currentSelectedGroupLabel != null) ? new MenuDesignUIItem(currentSelectedGroupLabel) : null;
                        if (currentSelectedGroupLabel != null)
                        {
                            currentSelectedGroupLabel.Refresh();
                            currentSelectedGroupLabel.Focus();
                            currentSelectedGroupLabel.IsDesignSelected = true;
                        }

                        OnDesignSelectionChanged(new MenuDesignUIItemEventArgs(currentDesignSelectedItem));
                    }
                    else if
                   (
                   currentDesignSelectedItem.MenuItem is MenuCommand &&
                   currentDesignSelectedItem.Control == this.CommandsTreeView
                   )
                    {
                        this.CommandsTreeView.SelectedNode = null;

						currentDesignSelectedItem = (this.MenuBranchesTreeView.SelectedNode != null) ? new MenuDesignUIItem(this.MenuBranchesTreeView.SelectedNode as MenuEditorTreeNode) : null;
                        
                        this.MenuBranchesTreeView.Focus();

						OnDesignSelectionChanged(new MenuDesignUIItemEventArgs(currentDesignSelectedItem));
                    }
                }
                e.Handled = true;
                return;
            }
        }

        //---------------------------------------------------------------------
        public CollapsiblePanel AddApplication(MenuApplication applicationToAdd)
        {
            if 
                (
                applicationToAdd == null ||
                this.ApplicationsPanel == null ||
                this.ApplicationsPanel.IsDisposed
                )
                return null;

            CollapsiblePanel lastApplicationPanel = null;
            if (this.ApplicationsPanel.PanelsCount > 0)
                lastApplicationPanel = this.ApplicationsPanel.Panels[this.ApplicationsPanel.PanelsCount - 1];
            
            return InsertApplicationPanel(applicationToAdd, lastApplicationPanel);
        }
        
        //---------------------------------------------------------------------
		public bool InsertNewApplication(MenuApplication aMenuApplication, CollapsiblePanel applicationPanelBefore)
        {
            if (applicationPanelBefore != null && applicationPanelBefore.Parent != this.ApplicationsPanel)
                return false;

            CollapsiblePanel newApplicationPanel = (applicationPanelBefore != null) ?
				InsertApplicationPanel(aMenuApplication, applicationPanelBefore) :
				AddApplication(aMenuApplication);

            if (newApplicationPanel == null)
                return false;

            Cursor currentCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            newApplicationPanel.IsDesignSelected = true;
            
            newApplicationPanel.Focus();

            this.Cursor = currentCursor;
            
            return true;
        }

        //---------------------------------------------------------------------
        public bool MoveApplicationPanel(CollapsiblePanel applicationPanelToMove, CollapsiblePanel applicationPanelBefore)
        { 
            if 
                (
                applicationPanelToMove == null ||
                applicationPanelToMove.Parent != this.ApplicationsPanel ||
                (applicationPanelBefore != null && applicationPanelBefore.Parent != this.ApplicationsPanel)
                )
                return false;

            this.ApplicationsPanel.Controls.Remove(applicationPanelToMove);

            CollapsiblePanel movedApplicationPanel = InsertApplicationPanel(applicationPanelToMove.MenuApplication, applicationPanelBefore);

            if (movedApplicationPanel == null)
                return false;

            Cursor currentCursor = this.Cursor;
			this.Cursor = Cursors.WaitCursor;

            movedApplicationPanel.IsDesignSelected = true;
            
            if (MenuItemMoved != null)
            {
                MenuItemMoved
                    (
                    this,
                    new MenuDesignUIItemEventArgs(
						currentDesignSelectedItem,
						null,
						(applicationPanelBefore != null) ? (applicationPanelBefore.MenuApplication) : null
						)
                    );
            }
            movedApplicationPanel.Focus();

            this.Cursor = currentCursor;

            return true;
        }

        //---------------------------------------------------------------------
        public bool InsertNewGroup(
			MenuGroup aMenuGroup,
			CollapsiblePanel applicationPanel,
			CollapsiblePanelLinkLabel groupLinkLabelBefore
			)
        {
            if (applicationPanel == null || applicationPanel.MenuApplication == null)
                return false;

            if
                (
                groupLinkLabelBefore == null &&
                applicationPanel.MenuApplication.Groups != null &&
                applicationPanel.MenuApplication.Groups.Count > 0
                )
            {
                // Calcolo qual'è l'ultima label di gruppo nel pannello dell'applicazione
                int previousLabelBottom = int.MinValue;
                foreach (Control aChildControl in applicationPanel.Controls)
                {
                    if (aChildControl != null && aChildControl is CollapsiblePanelLinkLabel)
                    {
                        if (aChildControl.Bottom > previousLabelBottom)
                        {
                            groupLinkLabelBefore = aChildControl as CollapsiblePanelLinkLabel;
                            previousLabelBottom = aChildControl.Bottom;
                        }
                    }
                }
            }

            CollapsiblePanelLinkLabel newGroupLinkLabel = InsertGroupLinkLabel(aMenuGroup, applicationPanel, groupLinkLabelBefore);

            if (newGroupLinkLabel == null)
                return false;

            Cursor currentCursor = this.Cursor;
			this.Cursor = Cursors.WaitCursor;

			if (!applicationPanel.IsExpanded)
				applicationPanel.Expand();

            newGroupLinkLabel.IsDesignSelected = true;

            newGroupLinkLabel.Focus();

			SelectGroupLabel(newGroupLinkLabel);

            this.Cursor = currentCursor;
            
            return true;
        }

        //---------------------------------------------------------------------
        public bool MoveGroupLinkLabel(CollapsiblePanelLinkLabel groupLinkLabelToMove, CollapsiblePanelLinkLabel groupLinkLabelBefore)
        {
            if (
                groupLinkLabelToMove == null ||
                groupLinkLabelToMove.Parent == null ||
                !(groupLinkLabelToMove.Parent is CollapsiblePanel) ||
                (groupLinkLabelBefore != null && groupLinkLabelBefore.Parent != groupLinkLabelToMove.Parent)
                )
                return false;

            CollapsiblePanel applicationPanel = groupLinkLabelToMove.Parent as CollapsiblePanel;

            applicationPanel.RemoveLinkLabel(groupLinkLabelToMove);

            CollapsiblePanelLinkLabel movedGroupLinkLabel = InsertGroupLinkLabel(groupLinkLabelToMove.MenuGroup, applicationPanel, groupLinkLabelBefore);

            if (movedGroupLinkLabel == null)
                return false;

            Cursor currentCursor = this.Cursor;
			this.Cursor = Cursors.WaitCursor;

            movedGroupLinkLabel.IsDesignSelected = true;

            if (MenuItemMoved != null)
            {
                MenuItemMoved
                    (
                    this,
                    new MenuDesignUIItemEventArgs(
						currentDesignSelectedItem,
						applicationPanel.MenuApplication,
						(groupLinkLabelBefore != null) ? (groupLinkLabelBefore.MenuGroup) : null
						)
                    );
            }
            movedGroupLinkLabel.Focus();

            this.Cursor = currentCursor;

            return true;
        }

        //---------------------------------------------------------------------
        public bool InsertNewMenuBranch(
			MenuBranch aMenuBranch,
			CollapsiblePanelLinkLabel groupLinkLabel,
			TreeNode nodeBefore
			)
        {
            if (groupLinkLabel != null)
            {
                if (!groupLinkLabel.IsDesignSelected)
                {
                    groupLinkLabel.IsDesignSelected = true;
                    nodeBefore = null;
                }
            }
            else
                groupLinkLabel = currentSelectedGroupLabel;

            if (groupLinkLabel == null)
                return false;

			MenuEditorTreeNode parentNode = null;
            if (nodeBefore == null || nodeBefore.TreeView != this.MenuBranchesTreeView)
            {
                if (this.MenuBranchesTreeView.Nodes != null && this.MenuBranchesTreeView.Nodes.Count > 0)
					nodeBefore = this.MenuBranchesTreeView.Nodes[this.MenuBranchesTreeView.Nodes.Count - 1] as MenuEditorTreeNode;
                else
                    nodeBefore = null;
            }
            
            if (nodeBefore != null)
				parentNode = nodeBefore.Parent as MenuEditorTreeNode;

			MenuEditorTreeNode newMenuBranchNode = InsertMenuBranch(aMenuBranch, parentNode, nodeBefore);
            if (newMenuBranchNode == null)
                return false;

            Cursor currentCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            if (newMenuBranchNode.Parent != null && !newMenuBranchNode.Parent.IsExpanded)
                newMenuBranchNode.Parent.Expand();
            newMenuBranchNode.EnsureVisible();

            this.MenuBranchesTreeView.SelectedNode = newMenuBranchNode;

            this.MenuBranchesTreeView.Focus();

            this.Cursor = currentCursor;

            return true;
        }

        public MenuEditorTreeNode GetSeletedTreeNode() 
        {
            return this.MenuBranchesTreeView.SelectedNode as MenuEditorTreeNode;
        }

        //---------------------------------------------------------------------
        public bool InsertNewMenuBranch(
			MenuBranch aMenuBranch,
			TreeNode parentMenuBranchNode,
			TreeNode nodeBefore
			)
        {
            if 
                (
                   currentSelectedGroupLabel == null ||
                   parentMenuBranchNode != null && 
                (
                parentMenuBranchNode.TreeView != this.MenuBranchesTreeView || 
                (nodeBefore != null && !parentMenuBranchNode.Nodes.Contains(nodeBefore))
                )
                )
                return false;

            TreeNodeCollection siblingNodes = (parentMenuBranchNode != null) ? parentMenuBranchNode.Nodes : this.MenuBranchesTreeView.Nodes;
            if (nodeBefore == null && siblingNodes != null && siblingNodes.Count > 0)
				nodeBefore = this.MenuBranchesTreeView.Nodes[this.MenuBranchesTreeView.Nodes.Count - 1] as MenuEditorTreeNode;

			MenuEditorTreeNode newMenuBranchNode = InsertMenuBranch(aMenuBranch, parentMenuBranchNode, nodeBefore);
            if (newMenuBranchNode == null)
                return false;

            Cursor currentCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            if (newMenuBranchNode.Parent != null && !newMenuBranchNode.Parent.IsExpanded)
                newMenuBranchNode.Parent.Expand();
            newMenuBranchNode.EnsureVisible();

            this.MenuBranchesTreeView.SelectedNode = newMenuBranchNode;

            this.MenuBranchesTreeView.Focus();

            this.Cursor = currentCursor;

            return true;
        }

        //---------------------------------------------------------------------
		public bool MoveMenuBranchNode(TreeNode aMenuBranchNodeToMove, TreeNode aParentNode, TreeNode aNodeBefore)
        {
			MenuEditorTreeNode menuBranchNodeToMove = aMenuBranchNodeToMove as MenuEditorTreeNode;
			MenuEditorTreeNode parentNode = aParentNode as MenuEditorTreeNode;
			MenuEditorTreeNode nodeBefore = aNodeBefore as MenuEditorTreeNode;

            if (
                currentSelectedGroupLabel == null ||
                menuBranchNodeToMove == null ||
                menuBranchNodeToMove.TreeView != this.MenuBranchesTreeView ||
                menuBranchNodeToMove.MenuItem == null ||
				!(menuBranchNodeToMove.MenuItem is MenuBranch) ||
                (parentNode != null && parentNode.TreeView != this.MenuBranchesTreeView) ||
                (nodeBefore != null && nodeBefore.TreeView != this.MenuBranchesTreeView)
                )
                return false;

			MenuEditorTreeNode movedMenuBranchNode = MoveMenuBranch(menuBranchNodeToMove.MenuItem as MenuBranch, parentNode, nodeBefore);

            if (movedMenuBranchNode == null)
                return false;

            Cursor currentCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            this.MenuBranchesTreeView.SelectedNode = movedMenuBranchNode;

            if (MenuItemMoved != null)
            {
                MenuItemMoved
                    (
                    this,
                    new MenuDesignUIItemEventArgs
						(
							currentDesignSelectedItem,
							(BaseMenuItem)((parentNode != null) ? parentNode.MenuItem : currentSelectedGroupLabel.MenuGroup),
							(nodeBefore != null) ? nodeBefore.MenuItem : null
						)
                    );
            }
            this.MenuBranchesTreeView.Focus();

            this.Cursor = currentCursor;

            return true;
        }

        //---------------------------------------------------------------------
		public bool InsertNewCommand(MenuCommand aMenuCommand, TreeNode aMenuBranchTreeNode, TreeNode aNodeBefore)
        {
			MenuEditorTreeNode menuBranchTreeNode = aMenuBranchTreeNode as MenuEditorTreeNode;
			MenuEditorTreeNode nodeBefore = aNodeBefore as MenuEditorTreeNode;

			if (aMenuCommand == null)
                return false;

            if (menuBranchTreeNode == null || menuBranchTreeNode.TreeView != this.MenuBranchesTreeView)
				menuBranchTreeNode = this.MenuBranchesTreeView.SelectedNode as MenuEditorTreeNode;
            else if (this.MenuBranchesTreeView.SelectedNode != menuBranchTreeNode)
                this.MenuBranchesTreeView.SelectedNode = menuBranchTreeNode;
            if
                (
                menuBranchTreeNode == null ||
				menuBranchTreeNode.MenuItem == null ||
				!(menuBranchTreeNode.MenuItem is MenuBranch)
                )
                return false;

            if (nodeBefore == null || nodeBefore.TreeView != this.CommandsTreeView)
            {
                if (this.CommandsTreeView.Nodes != null && this.CommandsTreeView.Nodes.Count > 0)
					nodeBefore = this.CommandsTreeView.Nodes[this.CommandsTreeView.Nodes.Count - 1] as MenuEditorTreeNode;
                else
                    nodeBefore = null;
            }

			MenuBranch parentBranch = null;
			MenuEditorTreeNode selectedNode = MenuBranchesTreeView.SelectedNode as MenuEditorTreeNode;
			if (selectedNode != null)
				parentBranch = selectedNode.MenuItem as MenuBranch;

			if (parentBranch == null)
				return false;

			MenuEditorTreeNode newCommandNode = InsertMenuCommand(aMenuCommand, nodeBefore);

            if (newCommandNode == null)
                return false;

            Cursor currentCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            newCommandNode.EnsureVisible();

            this.CommandsTreeView.SelectedNode = newCommandNode;
			this.CommandsTreeView.Focus();

            this.Cursor = currentCursor;

            return true;
        }

        //---------------------------------------------------------------------
		public bool MoveCommandNode(TreeNode aCommandNodeToMove, TreeNode aNodeBefore)
        {
			MenuEditorTreeNode commandNodeToMove = aCommandNodeToMove as MenuEditorTreeNode;
			MenuEditorTreeNode nodeBefore = aNodeBefore as MenuEditorTreeNode;

            if (
                this.MenuBranchesTreeView.SelectedNode == null || 
                commandNodeToMove == null ||
                commandNodeToMove.TreeView != this.CommandsTreeView ||
				commandNodeToMove.MenuItem == null ||
                !(commandNodeToMove.MenuItem is MenuCommand) ||
                (nodeBefore != null && nodeBefore.Parent != commandNodeToMove.Parent)
                )
                return false;

            commandNodeToMove.Remove();

			MenuEditorTreeNode movedCommandNode = InsertMenuCommand(commandNodeToMove.MenuItem, nodeBefore);

            if (movedCommandNode == null)
                return false;

            Cursor currentCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            this.CommandsTreeView.SelectedNode = movedCommandNode;

            if (MenuItemMoved != null)
            {
                MenuItemMoved
                    (
                    this,
                    new MenuDesignUIItemEventArgs(
						currentDesignSelectedItem,
						(this.MenuBranchesTreeView.SelectedNode as MenuEditorTreeNode).MenuItem as MenuBranch,
						(nodeBefore != null) ? nodeBefore.MenuItem: null
						)
                    );
            }
            this.CommandsTreeView.Focus();

            this.Cursor = currentCursor;

            return true;
        }

        //---------------------------------------------------------------------
        public void ClearContent()
        {
            currentSelectedGroupLabel = null;

            if (this.ApplicationsPanel != null && !this.ApplicationsPanel.IsDisposed)
            {
                if
                    (
                    currentDesignSelectedItem != null &&
                    currentDesignSelectedItem.Control != null &&
                    (
                    (currentDesignSelectedItem.Control is CollapsiblePanel && currentDesignSelectedItem.Control.Parent == this.ApplicationsPanel) ||
                    (currentDesignSelectedItem.Control is CollapsiblePanelLinkLabel && currentDesignSelectedItem.Control.Parent != null && currentDesignSelectedItem.Control.Parent is CollapsiblePanel && currentDesignSelectedItem.Control.Parent.Parent == this.ApplicationsPanel)
                    )
                    )
                {
                    currentDesignSelectedItem = null;

					OnDesignSelectionChanged(MenuDesignUIItemEventArgs.Empty);
                }

                this.ApplicationsPanel.Controls.Clear();
                if (ApplicationsPanelCleared != null)
                    ApplicationsPanelCleared(this, EventArgs.Empty);
            }

            ClearMenuBranchesTreeView();

            PerformLayout();
        }

        //---------------------------------------------------------------------
        public System.Windows.Forms.TreeView GetMenuBranchesTreeView()
        {
            return this.MenuBranchesTreeView;
        }

        //---------------------------------------------------------------------
        public System.Windows.Forms.TreeView GetCommandsTreeView()
        {
            return this.CommandsTreeView;
        }

        //---------------------------------------------------------------------
        public MenuDesignUIItem GetMenuDesignUIItemFromMousePoint(Point p)
        {
            Point clientPoint = this.PointToClient(p);

            Control childAtPoint = this.GetChildAtPoint(clientPoint);

            if (childAtPoint == null)
                return null;

            if (childAtPoint == this.ApplicationsPanel)
            {
                CollapsiblePanel collapsiblePanelAtPoint = this.ApplicationsPanel.GetChildAtPoint(this.ApplicationsPanel.PointToClient(p)) as CollapsiblePanel;
                if (collapsiblePanelAtPoint == null)
                    return null;

                CollapsiblePanelLinkLabel groupLabelAtPoint = collapsiblePanelAtPoint.GetChildAtPoint(collapsiblePanelAtPoint.PointToClient(p)) as CollapsiblePanelLinkLabel;
                if (groupLabelAtPoint == null)
                {
                    MenuApplication application = collapsiblePanelAtPoint.MenuApplication;
                    return (application != null) ? new MenuDesignUIItem(collapsiblePanelAtPoint) : null; 
                }

                MenuGroup group = groupLabelAtPoint.MenuGroup;
                return (group != null) ? new MenuDesignUIItem(groupLabelAtPoint) : null;
            }

            if (childAtPoint == this.MenuBranchesTreeView)
            {
				MenuEditorTreeNode menuTreeNodeAtPoint = this.MenuBranchesTreeView.GetNodeAt(this.MenuBranchesTreeView.PointToClient(p)) as MenuEditorTreeNode;
				if (menuTreeNodeAtPoint == null || menuTreeNodeAtPoint.MenuItem == null)
                    return null;

				MenuBranch menuBranch = menuTreeNodeAtPoint.MenuItem as MenuBranch;
                return (menuBranch != null) ? new MenuDesignUIItem(menuTreeNodeAtPoint) : null;
            }

            if (childAtPoint == this.CommandsTreeView)
            {
				MenuEditorTreeNode commandTreeNodeAtPoint = this.CommandsTreeView.GetNodeAt(this.CommandsTreeView.PointToClient(p)) as MenuEditorTreeNode;
				if (commandTreeNodeAtPoint == null || commandTreeNodeAtPoint.MenuItem == null)
                    return null;

				MenuCommand command = commandTreeNodeAtPoint.MenuItem as MenuCommand;
                return (command != null) ? new MenuDesignUIItem(commandTreeNodeAtPoint) : null;
            }

            return null;
        }

        //---------------------------------------------------------------------
        public MenuDesignUIItem SelectDesignMenuItem(object aMenuItemToSelect)
        {
            if (aMenuItemToSelect == null)
                return null;

            if (aMenuItemToSelect is MenuApplication || aMenuItemToSelect is MenuGroup)
            {
                MenuDesignUIItem menuDesignUIItem = GetMenuUIItem(aMenuItemToSelect);
                if (menuDesignUIItem != null)
                    menuDesignUIItem.IsDesignSelected = true;
                return menuDesignUIItem;
            }

            if (aMenuItemToSelect is MenuBranch)
            {
                if
                    (
                        currentSelectedGroupLabel == null ||
                        currentSelectedGroupLabel.MenuGroup != null ||
                        currentSelectedGroupLabel.MenuGroup is MenuGroup ||
                        (currentSelectedGroupLabel.MenuGroup).Menus == null ||
						!(currentSelectedGroupLabel.MenuGroup).Menus.Contains((MenuBranch)aMenuItemToSelect, true)
                    )
                {
                    CollapsiblePanelLinkLabel groupLinkLabelToSelect = null;

                    foreach (Control aChildControl in this.ApplicationsPanel.Controls)
                    {
						CollapsiblePanel aCollapsiblePanel = aChildControl as CollapsiblePanel;
                        if
                            (
                            aCollapsiblePanel != null &&
							aCollapsiblePanel.MenuApplication != null
                            )
                        {
                            foreach (Control aAppChildControl in aChildControl.Controls)
                            {
								CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = aAppChildControl as CollapsiblePanelLinkLabel;
                                if
                                    (
									aCollapsiblePanelLinkLabel != null &&
									aCollapsiblePanelLinkLabel.MenuGroup != null &&
									aCollapsiblePanelLinkLabel.MenuGroup.Menus != null
                                    )
                                {
									if (aCollapsiblePanelLinkLabel.MenuGroup.Menus.Contains((MenuBranch)aMenuItemToSelect, true))
                                    {
										groupLinkLabelToSelect = aCollapsiblePanelLinkLabel;
                                        break;
                                    }
                                }
                            }
                            if (groupLinkLabelToSelect != null)
                                break;
                        }
                    }

                    if (groupLinkLabelToSelect == null)
                        return null;

                    groupLinkLabelToSelect.IsDesignSelected = true;
                    SelectGroupLabel(groupLinkLabelToSelect);

                    MenuDesignUIItem menuDesignUIItem = GetMenuUIItem(aMenuItemToSelect);
                    if (menuDesignUIItem == null || menuDesignUIItem.TreeNode == null)
                        return null;

                    this.MenuBranchesTreeView.SelectedNode = menuDesignUIItem.TreeNode;
                    return menuDesignUIItem;
                }
            }

            if (aMenuItemToSelect is MenuCommand)
            {
                CollapsiblePanelLinkLabel groupLinkLabelToSelect = null;
                MenuBranch commandMenuBranch = null;

                foreach (Control aChildControl in this.ApplicationsPanel.Controls)
                {
					CollapsiblePanel aCollapsiblePanel = aChildControl as CollapsiblePanel;
                    if
                        (
						aCollapsiblePanel != null &&
						aCollapsiblePanel.MenuApplication != null
                        )
                    {
                        foreach (Control aAppChildControl in aChildControl.Controls)
                        {
							CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = aAppChildControl as CollapsiblePanelLinkLabel;
                            if
                                (
								aCollapsiblePanelLinkLabel != null &&
								aCollapsiblePanelLinkLabel.MenuGroup != null &&
                                aCollapsiblePanelLinkLabel.MenuGroup.Menus != null
                                )
                            {
                                commandMenuBranch = aCollapsiblePanelLinkLabel.MenuGroup.Menus.GetCommandMenuBranch((MenuCommand)aMenuItemToSelect);
                                if (commandMenuBranch != null)
                                {
									groupLinkLabelToSelect = aCollapsiblePanelLinkLabel;
                                    break;
                                }
                            }
                        }
                        if (commandMenuBranch != null)
                            break;
                    }
                }
                if (groupLinkLabelToSelect == null || commandMenuBranch == null)
                    return null;

                groupLinkLabelToSelect.IsDesignSelected = true;
                SelectGroupLabel(groupLinkLabelToSelect);

                MenuDesignUIItem menuBranchDesignUIItem = GetMenuUIItem(commandMenuBranch);
                if (menuBranchDesignUIItem == null || menuBranchDesignUIItem.TreeNode == null)
                    return null;

                this.MenuBranchesTreeView.SelectedNode = menuBranchDesignUIItem.TreeNode;

                MenuDesignUIItem commandDesignUIItem = GetMenuUIItem(aMenuItemToSelect);
                if (commandDesignUIItem == null || commandDesignUIItem.TreeNode == null)
                    return null;

                this.CommandsTreeView.SelectedNode = commandDesignUIItem.TreeNode;

                return commandDesignUIItem;
            }

            return null;
        }

        //---------------------------------------------------------------------
        public bool InsertMenuItem(object aMenuItem, object parentObject, object objectBefore)
        {
            if (aMenuItem == null)
                return false;

            MenuDesignUIItem parentObjectMenuUIItem = SelectDesignMenuItem(parentObject);

            MenuDesignUIItem objectBeforeMenuUIItem = GetMenuUIItem(objectBefore);
            if (aMenuItem is MenuApplication)
            {
                CollapsiblePanel applicationPanelBefore = (objectBeforeMenuUIItem != null) ? objectBeforeMenuUIItem.Control as CollapsiblePanel : null;

                CollapsiblePanel newApplicationPanel = InsertApplicationPanel((MenuApplication)aMenuItem, applicationPanelBefore);
                if (newApplicationPanel == null)
                    return false;
                
                Cursor currentCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                newApplicationPanel.IsDesignSelected = true;

                newApplicationPanel.Focus();

                this.Cursor = currentCursor;

                return true;
            }

            if (aMenuItem is MenuGroup)
            {
                CollapsiblePanel applicationPanel = (parentObjectMenuUIItem != null) ? parentObjectMenuUIItem.Control as CollapsiblePanel : null;
                if (applicationPanel == null)
                    return false;

                CollapsiblePanelLinkLabel groupLinkLabelBefore = (objectBeforeMenuUIItem != null) ? objectBeforeMenuUIItem.Control as CollapsiblePanelLinkLabel : null;
                CollapsiblePanelLinkLabel newGroupLinkLabel = InsertGroupLinkLabel((MenuGroup)aMenuItem, applicationPanel, groupLinkLabelBefore);

                if (newGroupLinkLabel == null)
                    return false;

                Cursor currentCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                if (applicationPanel != null && !applicationPanel.IsExpanded)
                    applicationPanel.Expand();

                newGroupLinkLabel.IsDesignSelected = true;

                newGroupLinkLabel.Focus();

                this.Cursor = currentCursor;

                return true;
            }

            if (aMenuItem is MenuBranch)
            {
				MenuEditorTreeNode parentNode = (parentObjectMenuUIItem != null) ? parentObjectMenuUIItem.TreeNode : null;
				MenuEditorTreeNode nodeBefore = (objectBeforeMenuUIItem != null) ? objectBeforeMenuUIItem.TreeNode : null;

				MenuEditorTreeNode newMenuBranchNode = InsertMenuBranch((MenuBranch)aMenuItem, parentNode, nodeBefore);
                if (newMenuBranchNode == null)
                    return false;

                Cursor currentCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                if (newMenuBranchNode.Parent != null && !newMenuBranchNode.Parent.IsExpanded)
                    newMenuBranchNode.Parent.Expand();
                newMenuBranchNode.EnsureVisible();

                this.MenuBranchesTreeView.SelectedNode = newMenuBranchNode;

                this.MenuBranchesTreeView.Focus();

                this.Cursor = currentCursor;

                return true;
            }

            if (aMenuItem is MenuCommand)
            {
				MenuEditorTreeNode nodeBefore = (objectBeforeMenuUIItem != null) ? objectBeforeMenuUIItem.TreeNode : null;
				MenuEditorTreeNode newCommandNode = InsertMenuCommand((MenuCommand)aMenuItem, nodeBefore);
                if (newCommandNode == null)
                    return false;

                Cursor currentCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                newCommandNode.EnsureVisible();

                this.CommandsTreeView.SelectedNode = newCommandNode;

                this.CommandsTreeView.Focus();

                this.Cursor = currentCursor;

                return true;
            }

            return false;
        }
       
        //---------------------------------------------------------------------
        public bool MoveMenuItem(object aMenuItem, object parentObject, object objectBefore)
        {
            if (aMenuItem == null)
                return false;

            MenuDesignUIItem parentObjectMenuUIItem = SelectDesignMenuItem(parentObject);

            MenuDesignUIItem menuUIItemToMove = GetMenuUIItem(aMenuItem);
            if (menuUIItemToMove == null)
                return false;

            MenuDesignUIItem objectBeforeMenuUIItem = GetMenuUIItem(objectBefore);
            if (aMenuItem is MenuApplication)
            {
                CollapsiblePanel panelToMove = menuUIItemToMove.Control as CollapsiblePanel;
                if (panelToMove == null)
                    return false;

                CollapsiblePanel panelBefore = (objectBeforeMenuUIItem != null) ? objectBeforeMenuUIItem.Control as CollapsiblePanel : null;
                return MoveApplicationPanel(panelToMove, panelBefore);
            }

            if (aMenuItem is MenuGroup)
            {
                CollapsiblePanelLinkLabel linkLabelToMove = menuUIItemToMove.Control as CollapsiblePanelLinkLabel;
                if (linkLabelToMove == null)
                    return false;

                CollapsiblePanelLinkLabel linkLabelBefore = (objectBeforeMenuUIItem != null) ? objectBeforeMenuUIItem.Control as CollapsiblePanelLinkLabel : null;
                return MoveGroupLinkLabel(linkLabelToMove, linkLabelBefore);
            }

            if (aMenuItem is MenuBranch)
            {
                if (menuUIItemToMove.TreeNode == null)
                    return false;

				MenuEditorTreeNode parentNode = (parentObjectMenuUIItem != null) ? parentObjectMenuUIItem.TreeNode : null;
				MenuEditorTreeNode nodeBefore = (objectBeforeMenuUIItem != null) ? objectBeforeMenuUIItem.TreeNode : null;
                
                return MoveMenuBranchNode(menuUIItemToMove.TreeNode, parentNode, nodeBefore);
            }

            if (aMenuItem is MenuCommand)
            {                
                if (menuUIItemToMove.TreeNode == null)
                    return false;

				MenuEditorTreeNode nodeBefore = (objectBeforeMenuUIItem != null) ? objectBeforeMenuUIItem.TreeNode : null;
                return MoveCommandNode(menuUIItemToMove.TreeNode, nodeBefore);
            }

            return false;
        }

        //---------------------------------------------------------------------
        public bool RemoveMenuItem(MenuDesignUIItem aMenuDesignUIItem)
        {
            if (
                aMenuDesignUIItem == null ||
                aMenuDesignUIItem.MenuItem == null ||
                aMenuDesignUIItem.Control == null
                )
                return false;

            
            if (aMenuDesignUIItem.MenuItem is MenuApplication && aMenuDesignUIItem.Control is CollapsiblePanel)
            {
                this.ApplicationsPanel.Controls.Remove(aMenuDesignUIItem.Control);

                if (currentDesignSelectedItem == null && this.ApplicationsPanel.PanelsCount > 0)
                    this.ApplicationsPanel.Panels[0].IsDesignSelected = true;

				if (currentDesignSelectedItem == aMenuDesignUIItem)
				{
					currentDesignSelectedItem = null;

					OnDesignSelectionChanged(MenuDesignUIItemEventArgs.Empty);
				}

                return true;
            }

            if 
                (
                aMenuDesignUIItem.MenuItem is MenuGroup && 
                aMenuDesignUIItem.Control is CollapsiblePanelLinkLabel && 
                aMenuDesignUIItem.Control.Parent != null && 
                aMenuDesignUIItem.Control.Parent is CollapsiblePanel
                )
            {
                if (currentSelectedGroupLabel == aMenuDesignUIItem.Control)
                    ClearMenuBranchesTreeView();

                CollapsiblePanel parentApplicationPanel = aMenuDesignUIItem.Control.Parent as CollapsiblePanel;

                parentApplicationPanel.RemoveLinkLabel((CollapsiblePanelLinkLabel)aMenuDesignUIItem.Control);

                if (currentDesignSelectedItem == null)
                    parentApplicationPanel.IsDesignSelected = true;

				if (currentDesignSelectedItem == aMenuDesignUIItem)
				{
					currentDesignSelectedItem = null;

					OnDesignSelectionChanged(MenuDesignUIItemEventArgs.Empty);
				}

				if (parentApplicationPanel.MenuApplication.Groups.Count == 0)
					OnDesignSelectionChanged(new MenuDesignUIItemEventArgs(new MenuDesignUIItem(parentApplicationPanel)));

                return true;
            }

            if
                (
                aMenuDesignUIItem.MenuItem is MenuBranch &&
                aMenuDesignUIItem.TreeNode != null &&
                aMenuDesignUIItem.Control is TreeView &&
                aMenuDesignUIItem.TreeNode.TreeView == aMenuDesignUIItem.Control
                )
            {
                if (aMenuDesignUIItem.TreeNode.TreeView.SelectedNode == aMenuDesignUIItem.TreeNode)
                    ClearCommandsTreeView();

                // remove just the current node.
                aMenuDesignUIItem.TreeNode.Remove();

                //// take care of the parent node
                //if (aMenuDesignUIItem.IsParentToBeRemoved())
                //{
                //    // remove the parent as well as it is going to have no children after the removal.                    
                //    aMenuDesignUIItem.TreeNode.Parent.Remove();
                //}
                //else
                //{
                //    // remove just the current node.
                //    aMenuDesignUIItem.TreeNode.Remove();
                //}
                //// take care of the parent node
                //if (aMenuDesignUIItem.TreeNode.Parent != null && aMenuDesignUIItem.TreeNode.Parent.Nodes.Count == 1)
                //{
                //    // remove the parent as well as it is going to have no children after the removal.
                //    aMenuDesignUIItem.TreeNode.Parent.Remove();
                //}

                if (currentDesignSelectedItem == aMenuDesignUIItem)
                {
                    currentDesignSelectedItem = null;

					OnDesignSelectionChanged(MenuDesignUIItemEventArgs.Empty);
                }

                if (currentDesignSelectedItem == null && currentSelectedGroupLabel != null)
                    currentSelectedGroupLabel.IsDesignSelected = true;

                return true;
            }

            if
                (
                aMenuDesignUIItem.MenuItem is MenuCommand &&
                aMenuDesignUIItem.TreeNode != null &&
                aMenuDesignUIItem.Control is TreeView &&
                aMenuDesignUIItem.TreeNode.TreeView == aMenuDesignUIItem.Control
                )
            {
                aMenuDesignUIItem.TreeNode.Remove();

                if (currentDesignSelectedItem == aMenuDesignUIItem)
                {
                    currentDesignSelectedItem = null;

					OnDesignSelectionChanged(MenuDesignUIItemEventArgs.Empty);
                }

                if (currentDesignSelectedItem == null && this.MenuBranchesTreeView.SelectedNode == null && currentSelectedGroupLabel != null)
                    currentSelectedGroupLabel.IsDesignSelected = true;

                return true;
            }

            return false;
        }

        //---------------------------------------------------------------------
        public MenuDesignUIItem GetMenuUIItem(object aMenuItem)
        {
            if (aMenuItem == null)
                return null;

            if (aMenuItem is MenuApplication)
            {
                CollapsiblePanel applicationPanel = FindApplicationPanel((MenuApplication)aMenuItem);
                return (applicationPanel != null) ? new MenuDesignUIItem(applicationPanel) : null;
            }

            if (aMenuItem is MenuGroup)
            { 
                 CollapsiblePanelLinkLabel groupLabel = FindGroupLinkLabel((MenuGroup)aMenuItem);
                 return (groupLabel != null) ? new MenuDesignUIItem(groupLabel) : null;
            }
            
            if (aMenuItem is MenuBranch)
            {
				MenuEditorTreeNode menuBranchTreeNode = FindMenuBranchTreeNode((MenuBranch)aMenuItem);
                return (menuBranchTreeNode != null) ? new MenuDesignUIItem(menuBranchTreeNode) : null;
            }
                
            if (aMenuItem is MenuCommand)
            {
				MenuEditorTreeNode commandTreeNode = FindAndUpdateCommandTreeNode((MenuCommand)aMenuItem);
                return (commandTreeNode != null) ? new MenuDesignUIItem(commandTreeNode) : null;
            }

            return null;
        }

        //---------------------------------------------------------------------
        public void UpdateMenuItemUI(object aMenuItem)
        {
            if (aMenuItem == null)
                return;

			MenuApplication aMenuApp = aMenuItem as MenuApplication;
			if (aMenuApp != null)
            {
				CollapsiblePanel applicationPanel = FindApplicationPanel(aMenuApp);
                if (applicationPanel != null)
                {
					applicationPanel.Title = (aMenuApp.Title != null) ? aMenuApp.Title.ToString() : null;
					applicationPanel.TitleImage = GetMenuItemImage(aMenuApp); ;
                }

                return;
            }

			MenuGroup aMenuGroup = aMenuItem as MenuGroup;
			if (aMenuGroup != null)
            {
				CollapsiblePanelLinkLabel groupLabel = FindGroupLinkLabel(aMenuGroup);
                if (groupLabel != null)
                {
                    Image groupImage = null;
					if (!String.IsNullOrEmpty((aMenuGroup).Name))
						groupImage = GetMenuItemImage((aMenuGroup));

					groupLabel.Image = groupImage != null ? groupImage : Resources.DefaultGroupImage;

					groupLabel.Text = ((aMenuGroup).Title != null) ? (aMenuGroup).Title.ToString() : null;
                }

                return;
            }

			MenuBranch aMenuBranch = aMenuItem as MenuBranch;
			if (aMenuBranch != null)
            {
				MenuEditorTreeNode menuBranchTreeNode = FindMenuBranchTreeNode(aMenuBranch);
                if (menuBranchTreeNode != null)
					menuBranchTreeNode.Text = (aMenuBranch.Title != null) ? aMenuBranch.Title.ToString() : null;
                
				return;
            }

			MenuCommand aMenuCommand = aMenuItem as MenuCommand;
			if (aMenuCommand != null)
            {
				MenuEditorTreeNode commandTreeNode = FindAndUpdateCommandTreeNode(aMenuCommand);
				if (commandTreeNode != null)
					commandTreeNode.Text = (aMenuCommand.Title != null) ? aMenuCommand.Title.ToString() : null;

				return;
            }
        }

        //---------------------------------------------------------------------
        public CollapsiblePanel GetPreviousApplicationPanel(CollapsiblePanel applicationPanel)
        {
            if (this.ApplicationsPanel == null || applicationPanel == null || !this.ApplicationsPanel.Controls.Contains(applicationPanel))
                return null;

            CollapsiblePanel previousApplicationPanel = null;
            int previousPanelBottom = int.MinValue;
            foreach (Control aChildControl in this.ApplicationsPanel.Controls)
            {
                if (aChildControl != null && aChildControl is CollapsiblePanel && aChildControl != applicationPanel)
                {
                    if (aChildControl.Bottom <= applicationPanel.Top && aChildControl.Bottom > previousPanelBottom)
                    {
                        previousApplicationPanel = aChildControl as CollapsiblePanel;
                        previousPanelBottom = aChildControl.Bottom;
                    }
                }
            }

            return previousApplicationPanel;
        }

        //---------------------------------------------------------------------
        public CollapsiblePanel GetNextApplicationPanel(CollapsiblePanel applicationPanel)
        {
            if (this.ApplicationsPanel == null || applicationPanel == null || !this.ApplicationsPanel.Controls.Contains(applicationPanel))
                return null;

            CollapsiblePanel nextApplicationPanel = null;
            int nextPanelTop = int.MaxValue;
            foreach (Control aChildControl in this.ApplicationsPanel.Controls)
            {
                if (aChildControl != null && aChildControl is CollapsiblePanel && aChildControl != applicationPanel)
                {
                    if (aChildControl.Top >= applicationPanel.Bottom && aChildControl.Top < nextPanelTop)
                    {
                        nextApplicationPanel = aChildControl as CollapsiblePanel;
                        nextPanelTop = aChildControl.Top;
                    }
                }
            }

            return nextApplicationPanel;
        }

        //---------------------------------------------------------------------
        public static CollapsiblePanelLinkLabel GetPreviousGroupLinkLabel(CollapsiblePanelLinkLabel groupLinkLabel)
        {
            if (groupLinkLabel == null || groupLinkLabel.Parent == null)
                return null;

            CollapsiblePanelLinkLabel previousGroupLinkLabel = null;
            int previousLinkLabelBottom = int.MinValue;
            foreach (Control aChildControl in groupLinkLabel.Parent.Controls)
            {
                if (aChildControl != null && aChildControl is CollapsiblePanelLinkLabel && aChildControl != groupLinkLabel)
                {
                    if (aChildControl.Bottom <= groupLinkLabel.Top && aChildControl.Bottom > previousLinkLabelBottom)
                    {
                        previousGroupLinkLabel = aChildControl as CollapsiblePanelLinkLabel;
                        previousLinkLabelBottom = aChildControl.Bottom;
                    }
                }
            }

            return previousGroupLinkLabel;
        }

        //---------------------------------------------------------------------
        public static CollapsiblePanelLinkLabel GetNextGroupLinkLabel(CollapsiblePanelLinkLabel groupLinkLabel)
        {
            if (groupLinkLabel == null || groupLinkLabel.Parent == null)
                return null;

            CollapsiblePanelLinkLabel nextGroupLinkLabel = null;
            int nextLinkLabelTop = int.MaxValue;
            foreach (Control aChildControl in groupLinkLabel.Parent.Controls)
            {
                if (aChildControl != null && aChildControl is CollapsiblePanelLinkLabel && aChildControl != groupLinkLabel)
                {
                    if (aChildControl.Top >= groupLinkLabel.Bottom && aChildControl.Top < nextLinkLabelTop)
                    {
                        nextGroupLinkLabel = aChildControl as CollapsiblePanelLinkLabel;
                        nextLinkLabelTop = aChildControl.Top;
                    }
                }
            }

            return nextGroupLinkLabel;
        }

        //---------------------------------------------------------------------
        private void MenuBranchesTreeView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
				MenuEditorTreeNode clickedNode = this.MenuBranchesTreeView.GetNodeAt(e.Location) as MenuEditorTreeNode;
				if (clickedNode != null && this.MenuBranchesTreeView.SelectedNode != clickedNode)
					this.MenuBranchesTreeView.SelectedNode = clickedNode;
            }

        }

        //---------------------------------------------------------------------
        private void MenuBranchesTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.Node != null && this.MenuBranchesTreeView.SelectedNode != e.Node)
                this.MenuBranchesTreeView.SelectedNode = e.Node;
        }

        //---------------------------------------------------------------------
        private void MenuBranchesTreeView_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        //---------------------------------------------------------------------
        private void MenuBranchesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (this.CommandsTreeView.Nodes.Count > 0)
            {
                this.CommandsTreeView.Nodes.Clear();

                if (CommandsTreeViewNodesCleared != null)
                    CommandsTreeViewNodesCleared(this, EventArgs.Empty);
            }

			if (e == null)
				return;

			MenuEditorTreeNode node = e.Node as MenuEditorTreeNode;

            if
                (
				node == null ||
				node.MenuItem == null ||
				!(node.MenuItem is MenuBranch)
                )
                return;

			AddMenuCommands((MenuBranch)node.MenuItem);

			currentDesignSelectedItem = new MenuDesignUIItem(node);

			OnDesignSelectionChanged(new MenuDesignUIItemEventArgs(new MenuDesignUIItem(node)));
        }
        
        //---------------------------------------------------------------------
        private void MenuBranchesTreeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e == null || e.Node == null)
                return;

			MenuEditorTreeNode menuEditorTreeNode = e.Node as MenuEditorTreeNode;
			if (menuEditorTreeNode == null)
				return;

			menuEditorTreeNode.Expanded = true;
			
        }

        //---------------------------------------------------------------------
        private void MenuBranchesTreeView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e == null || e.Node == null)
                return;

			MenuEditorTreeNode menuEditorTreeNode = e.Node as MenuEditorTreeNode;
			if (menuEditorTreeNode == null)
				return;

			menuEditorTreeNode.Expanded = false;
        }

        //---------------------------------------------------------------------
        private void GroupLinkLabel_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }
        
        //---------------------------------------------------------------------
        private void GroupLinkLabel_DesignSelectionChanged(object sender, EventArgs e)
        {
			CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = sender as CollapsiblePanelLinkLabel;
            if (aCollapsiblePanelLinkLabel == null || !aCollapsiblePanelLinkLabel.IsDesignSelected)
                return;

            currentDesignSelectedItem = new MenuDesignUIItem(aCollapsiblePanelLinkLabel);

            OnDesignSelectionChanged(new MenuDesignUIItemEventArgs(currentDesignSelectedItem));
        }

        //---------------------------------------------------------------------
        private void GroupLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (sender == null || !(sender is CollapsiblePanelLinkLabel))
                return;

            SelectGroupLabel((CollapsiblePanelLinkLabel)sender);
        }

        //---------------------------------------------------------------------
        private void GroupLinkLabel_InitDrag(object sender, EventArgs e)
        {
			CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = sender as CollapsiblePanelLinkLabel;
            if ( aCollapsiblePanelLinkLabel == null || 
				!(aCollapsiblePanelLinkLabel.MenuGroup is MenuGroup)
				)
                return;

            bool isControlPressed = (Control.ModifierKeys == Keys.Control);
            DragDropEffects currentEffect = isControlPressed ? DragDropEffects.Copy : DragDropEffects.Move;

            MenuGroup draggedGroup = isControlPressed
				? aCollapsiblePanelLinkLabel.MenuGroup.Clone()
				: aCollapsiblePanelLinkLabel.MenuGroup;

			if (!draggedGroup.CanBeCustomized)
				return;

            if (isControlPressed)
                draggedGroup.Name = String.Empty;

            this.ApplicationsPanel.DoDragDrop(draggedGroup, currentEffect);
        }

        //---------------------------------------------------------------------
        private void GroupLinkLabel_DragEnter(object sender, DragEventArgs e)
        {
            ClearDragFeedback();
            
            if (e == null || e.Data == null || sender == null || !(sender is CollapsiblePanelLinkLabel))
                return;

            e.Effect = GetGroupLabelDragDropEffect((CollapsiblePanelLinkLabel)sender, e.Data, e.AllowedEffect);
        }

        //---------------------------------------------------------------------
        private void GroupLinkLabel_DragOver(object sender, DragEventArgs e)
        {
            if (e == null || e.Data == null || sender == null || !(sender is CollapsiblePanelLinkLabel))
                return;

            e.Effect = GetGroupLabelDragDropEffect((CollapsiblePanelLinkLabel)sender, e.Data, e.AllowedEffect);

            if (e.Effect == DragDropEffects.None)
                ClearDragFeedback();
        }

        //---------------------------------------------------------------------
        private void GroupLinkLabel_DragDrop(object sender, DragEventArgs e)
        {
            ClearDragFeedback();

            if (e == null || e.Data == null || sender == null || !(sender is CollapsiblePanelLinkLabel))
                return;

            if (e.Data.GetDataPresent(typeof(MenuGroup)))
            {
                CollapsiblePanel parentPanel = ((CollapsiblePanelLinkLabel)sender).Parent as CollapsiblePanel;
                if (parentPanel == null)
                    return;

                parentPanel.Refresh();

                MenuGroup draggedGroup = e.Data.GetData(typeof(MenuGroup)) as MenuGroup;
                if (draggedGroup == null)
                    return;

                Cursor currentCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                // Calcolo sotto quale linklabel esistente vada inserita la nuova linklabel
                CollapsiblePanelLinkLabel groupLinkLabelBefore = null;
                int previousLabelBottom = int.MinValue;

                foreach (Control aChildControl in parentPanel.Controls)
                {
                    if (aChildControl != null && aChildControl is CollapsiblePanelLinkLabel)
                    {
                        if (aChildControl.Bottom <= ((CollapsiblePanelLinkLabel)sender).Top && aChildControl.Bottom > previousLabelBottom)
                        {
                            groupLinkLabelBefore = aChildControl as CollapsiblePanelLinkLabel;
                            previousLabelBottom = aChildControl.Bottom;
                        }
                    }
                }

                if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    CollapsiblePanelLinkLabel groupLinkLabelToRemove = FindGroupLinkLabel(draggedGroup);
                    if (groupLinkLabelToRemove != null && groupLinkLabelToRemove.Parent != null && groupLinkLabelToRemove.Parent is CollapsiblePanel)
                        ((CollapsiblePanel)groupLinkLabelToRemove.Parent).RemoveLinkLabel(groupLinkLabelToRemove);
                }

                CollapsiblePanelLinkLabel newGroupLinkLabel = InsertGroupLinkLabel(draggedGroup, parentPanel, groupLinkLabelBefore);

                if (newGroupLinkLabel != null)
                {
                    newGroupLinkLabel.IsDesignSelected = true;

                    if (MenuItemMoved != null && (e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                    {
                        MenuItemMoved
                            (
                            this,
                           new MenuDesignUIItemEventArgs(
								currentDesignSelectedItem,
								parentPanel.MenuApplication,
								(groupLinkLabelBefore != null) ? (groupLinkLabelBefore.MenuGroup) : null
								)
                            );
                    }
                    newGroupLinkLabel.Focus();
                }
                this.Cursor = currentCursor;
            }
        }

        //---------------------------------------------------------------------
        private void CommandsTreeView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
				MenuEditorTreeNode clickedNode = this.CommandsTreeView.GetNodeAt(e.Location) as MenuEditorTreeNode;
                if (clickedNode != null && this.CommandsTreeView.SelectedNode != clickedNode)
                    this.CommandsTreeView.SelectedNode = clickedNode;
            }
        }

        //---------------------------------------------------------------------
        private void CommandsTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.Node != null && this.CommandsTreeView.SelectedNode != e.Node)
                this.CommandsTreeView.SelectedNode = e.Node;
        }
        
        //---------------------------------------------------------------------
        private void CommandsTreeView_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        //---------------------------------------------------------------------
        private void CommandsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
			if (e == null)
				return;

			MenuEditorTreeNode node = e.Node as MenuEditorTreeNode;

            if
                (
				node == null ||
				node.MenuItem == null ||
				!(node.MenuItem is MenuCommand)
                )
                return;

			currentDesignSelectedItem = new MenuDesignUIItem(node);

			OnDesignSelectionChanged(new MenuDesignUIItemEventArgs(new MenuDesignUIItem(node)));
        }

        //---------------------------------------------------------------------
        private void ApplicationsPanel_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        // The DragEnter event occurs when an object is dragged into the control's bounds.
        //---------------------------------------------------------------------
        private void ApplicationsPanel_DragEnter(object sender, DragEventArgs e)
        {
            ClearDragFeedback();
            
            if (e == null || e.Data == null)
                return;

            e.Effect = GetApplicationsPanelDragDropEffect(e.Data, e.AllowedEffect);
        }

        // The DragOver event occurs when an object is dragged over the control's bounds.
        //---------------------------------------------------------------------
        private void ApplicationsPanel_DragOver(object sender, DragEventArgs e)
        {
            if (e == null || e.Data == null)
                return;

            e.Effect = GetApplicationsPanelDragDropEffect(e.Data, e.AllowedEffect);

            if (e.Effect == DragDropEffects.None)
                ClearDragFeedback();
        }


        // The DragDrop event occurs when a drag-and-drop operation is completed.
        //---------------------------------------------------------------------
        private void ApplicationsPanel_DragDrop(object sender, DragEventArgs e)
        {
            ClearDragFeedback();

            if (e == null || e.Data == null || ((e.Effect & DragDropEffects.Copy) != DragDropEffects.Copy && (e.Effect & DragDropEffects.Move) != DragDropEffects.Move))
                return;

            Point currentMousePos = Control.MousePosition;
            Point appPanelClientMousePos = this.ApplicationsPanel.PointToClient(currentMousePos);
            CollapsiblePanel collapsiblePanelAtPoint = this.ApplicationsPanel.GetChildAtPoint(appPanelClientMousePos) as CollapsiblePanel;

            if (e.Data.GetDataPresent(typeof(MenuApplication)))
            {
                
                if (collapsiblePanelAtPoint != null)
                    return;

                MenuApplication draggedApplication = e.Data.GetData(typeof(MenuApplication)) as MenuApplication;
                if (draggedApplication == null)
                    return;

                Cursor currentCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                // Calcolo sotto quale pannello di applicazione esistente vada copiata la nuova applicazione
                CollapsiblePanel applicationPanelBefore = null;
                int previousPanelBottom = int.MinValue;
                foreach (Control aChildControl in this.ApplicationsPanel.Controls)
                {
                    if (aChildControl != null && aChildControl is CollapsiblePanel)
                    {
                        if (aChildControl.Bottom <= appPanelClientMousePos.Y && aChildControl.Bottom > previousPanelBottom)
                        {
                            applicationPanelBefore = aChildControl as CollapsiblePanel;
                            previousPanelBottom = aChildControl.Bottom;
                        }
                    }
                }
                if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    CollapsiblePanel applicationPanelToRemove = FindApplicationPanel(draggedApplication);
                    if (applicationPanelToRemove != null)
                        this.ApplicationsPanel.Controls.Remove(applicationPanelToRemove);
                }

                CollapsiblePanel newApplicationPanel = InsertApplicationPanel(draggedApplication, applicationPanelBefore);

                if (newApplicationPanel != null)
                {
                    newApplicationPanel.IsDesignSelected = true;

                    if (MenuItemMoved != null && (e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                    {
                        MenuItemMoved
                            (
                            this,
                            new MenuDesignUIItemEventArgs(
								currentDesignSelectedItem,
								null,
								(applicationPanelBefore != null) ? (applicationPanelBefore.MenuApplication) : null
								)
                            );
                    }
                    newApplicationPanel.Focus();
                }

                this.Cursor = currentCursor;
            }

            if (e.Data.GetDataPresent(typeof(MenuGroup)))
            {
                if (collapsiblePanelAtPoint == null)
                    return;

                collapsiblePanelAtPoint.Refresh();

                MenuGroup draggedGroup = e.Data.GetData(typeof(MenuGroup)) as MenuGroup;
                if (draggedGroup == null)
                    return;

                Cursor currentCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                // Calcolo sotto quale linklabel esistente vada inserita la nuova linklabel
                CollapsiblePanelLinkLabel groupLinkLabelBefore = null;
                if (collapsiblePanelAtPoint.IsExpanded)
                {
                    Point panelClientMousePos = collapsiblePanelAtPoint.PointToClient(currentMousePos);

                    int previousLabelBottom = int.MinValue;

                    foreach (Control aChildControl in collapsiblePanelAtPoint.Controls)
                    {
                        if (aChildControl != null && aChildControl is CollapsiblePanelLinkLabel)
                        {
                            if (aChildControl.Bottom <= panelClientMousePos.Y && aChildControl.Bottom > previousLabelBottom)
                            {
                                groupLinkLabelBefore = aChildControl as CollapsiblePanelLinkLabel;
                                previousLabelBottom = aChildControl.Bottom;
                            }
                        }
                    }
                }
                else
                {
                    collapsiblePanelAtPoint.Expand();

                    int previousLabelBottom = int.MinValue;
                    foreach (Control aChildControl in collapsiblePanelAtPoint.Controls)
                    {
                        if (aChildControl != null && aChildControl is CollapsiblePanelLinkLabel)
                        {
                            if (aChildControl.Bottom > previousLabelBottom)
                            {
                                groupLinkLabelBefore = aChildControl as CollapsiblePanelLinkLabel;
                                previousLabelBottom = aChildControl.Bottom;
                            }
                        }
                    }
                }

                if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    CollapsiblePanelLinkLabel groupLinkLabelToRemove = FindGroupLinkLabel(draggedGroup);
                    if (groupLinkLabelToRemove != null && groupLinkLabelToRemove.Parent != null && groupLinkLabelToRemove.Parent is CollapsiblePanel)
                        ((CollapsiblePanel)groupLinkLabelToRemove.Parent).RemoveLinkLabel(groupLinkLabelToRemove);
                }

                CollapsiblePanelLinkLabel newGroupLinkLabel = InsertGroupLinkLabel(draggedGroup, collapsiblePanelAtPoint, groupLinkLabelBefore);

                if (newGroupLinkLabel != null)
                {
                    newGroupLinkLabel.IsDesignSelected = true;

					if (MenuItemMoved != null && (e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
					{
						MenuItemMoved
							(
							this,
							new MenuDesignUIItemEventArgs(
								currentDesignSelectedItem,
								collapsiblePanelAtPoint.MenuApplication,
								(groupLinkLabelBefore != null) ? (groupLinkLabelBefore.MenuGroup) : null
								)
							);
					}
                    newGroupLinkLabel.Focus();
                }
                this.Cursor = currentCursor;
            }
        }

         //---------------------------------------------------------------------
        private void ApplicationsPanel_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
        }

        //---------------------------------------------------------------------
        private void MenuBranchesTreeView_DragEnter(object sender, DragEventArgs e)
        {
            ClearDragFeedback();
            
            if (e == null || e.Data == null)
                return;

            e.Effect = GetMenuBranchesTreeViewDragDropEffect(e.Data, e.AllowedEffect);

            
        }

        //---------------------------------------------------------------------
        private void MenuBranchesTreeView_DragOver(object sender, DragEventArgs e)
        {
            
            if (e == null || e.Data == null)
                return;

            e.Effect = GetMenuBranchesTreeViewDragDropEffect(e.Data, e.AllowedEffect);

            if (e.Effect == DragDropEffects.None)
                ClearDragFeedback();
        }

        

        //---------------------------------------------------------------------
        private void MenuBranchesTreeView_DragDrop(object sender, DragEventArgs e)
        {
            ClearDragFeedback();

            this.MenuBranchesTreeView.Cursor = Cursors.Default;

            if (e == null || e.Data == null || currentSelectedGroupLabel == null)
                return;

            Point currentMouseClientPos = this.MenuBranchesTreeView.PointToClient(Control.MousePosition);
			MenuEditorTreeNode menuBranchAtPoint = this.MenuBranchesTreeView.GetNodeAt(currentMouseClientPos) as MenuEditorTreeNode;

           
			// menuBranchAtPoint è null se:
			// 1) non ci sono branch nel gruppo;
			// oppure
			// 2) l'item che è stato 'drop-ato' è stato rilasciato in una zona del controllo dove 
			// l'albero dei branch non si estende per cui non ne viene rilevata la posizione.
			//
			// Nel primo caso lascio tutto così e l'item in questione mi verrà aggiunto come primo.
			// Nel secondo caso devo andare a mano a prendermi l'ultimo item della mia collezione
			// e usarlo come menuBranchAtPoint.

			int nodesCount = 0;
			if (
				menuBranchAtPoint == null &&
				this.MenuBranchesTreeView.Nodes != null &&
				(nodesCount = this.MenuBranchesTreeView.Nodes.Count) > 0
				)
				menuBranchAtPoint = this.MenuBranchesTreeView.Nodes[nodesCount - 1] as MenuEditorTreeNode;

            if (e.Data.GetDataPresent(typeof(MenuBranch)))
            {
                MenuBranch draggedMenuBranch = e.Data.GetData(typeof(MenuBranch)) as MenuBranch;
                if (draggedMenuBranch == null || draggedMenuBranch.Menus.Count > 0)
                    return;

                MenuBranch oParentBranch = FindParentBranch(draggedMenuBranch);


                if (oParentBranch != null && oParentBranch.Menus.Count <= 1)
                {
                    // the to be moved node's parent has just that child, can not 
                    // move it or the four level structure would be altered.
                    return;
                }

                Cursor currentCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

				MenuEditorTreeNode newMenuBranchNode = null;
				MenuEditorTreeNode parentNode = null;
				MenuEditorTreeNode nodeBefore = null;
                if (menuBranchAtPoint != null)
                {
                    // Se sono nella parte alta di un nodo di menù, il nuovo branch va inserito sopra a 
                    // quello su cui mi trovo, se sono nella parte bassa sotto, altrimenti va aggiunto (o 
                    // spostato) come suo ultimo sottonodo
                    if (menuBranchAtPoint.Bounds.Top + menuBranchAtPoint.Bounds.Height / 3 > currentMouseClientPos.Y)
                    {
                        parentNode = menuBranchAtPoint.Parent as MenuEditorTreeNode ?? menuBranchAtPoint;
						nodeBefore = menuBranchAtPoint.PrevNode as MenuEditorTreeNode;
                    }
                    else if (menuBranchAtPoint.Bounds.Bottom - menuBranchAtPoint.Bounds.Height / 3 < currentMouseClientPos.Y)
                    {
                        parentNode = menuBranchAtPoint.Parent as MenuEditorTreeNode ?? menuBranchAtPoint;
                        nodeBefore = menuBranchAtPoint;
                    }
                    else
                    {
                        parentNode = menuBranchAtPoint;
                        if (parentNode != null && parentNode.Nodes.Count > 0)
							nodeBefore = parentNode.Nodes[parentNode.Nodes.Count - 1] as MenuEditorTreeNode;
                    }
                    if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy)
                        newMenuBranchNode = InsertMenuBranch(draggedMenuBranch, parentNode, nodeBefore);
                    else if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                        newMenuBranchNode = MoveMenuBranch(draggedMenuBranch, parentNode, nodeBefore);
                }
                else if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy)
                    newMenuBranchNode = AddMenuBranch(draggedMenuBranch, null);
                else if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                    newMenuBranchNode = MoveMenuBranchAsLast(draggedMenuBranch, null);

                if (newMenuBranchNode != null)
                {
                    if (newMenuBranchNode.Parent != null && !newMenuBranchNode.Parent.IsExpanded)
                        newMenuBranchNode.Parent.Expand();
                    newMenuBranchNode.EnsureVisible();

                    this.MenuBranchesTreeView.SelectedNode = newMenuBranchNode;

                    if (MenuItemMoved != null && (e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                    {
                        MenuItemMoved
                            (
                            this,
                            new MenuDesignUIItemEventArgs(
								currentDesignSelectedItem,
								((parentNode != null) ? parentNode.MenuItem : currentSelectedGroupLabel.MenuGroup),
								(nodeBefore != null) ? nodeBefore.MenuItem : null
								)
                            );
                    }
                    this.MenuBranchesTreeView.Focus();
                }
                this.Cursor = currentCursor;
            }

            if
                (
                e.Data.GetDataPresent(typeof(DocumentMenuCommand)) ||
                e.Data.GetDataPresent(typeof(BatchMenuCommand)) ||
                e.Data.GetDataPresent(typeof(ReportMenuCommand)) ||
                e.Data.GetDataPresent(typeof(FunctionMenuCommand)) ||
                e.Data.GetDataPresent(typeof(TextMenuCommand)) ||
                e.Data.GetDataPresent(typeof(ExeMenuCommand)) ||
                e.Data.GetDataPresent(typeof(OfficeItemMenuCommand))
                )
            {
                if (menuBranchAtPoint == null)
                    return;

                if (menuBranchAtPoint.Parent == null)
                {
                    // destination node is a root, drop of a command is not allowed.
                    // TODO: allow branches without branch children to be moved as root children.
                    return;
                }
                string[] dataFormats = e.Data.GetFormats();
                if (dataFormats == null || dataFormats.Length == 0)
                    return;

                MenuCommand draggedCommand = e.Data.GetData(dataFormats[0], true) as MenuCommand;
                if (draggedCommand == null)
                    return;

                Cursor currentCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                this.MenuBranchesTreeView.SelectedNode = menuBranchAtPoint;

                if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                {
					MenuEditorTreeNode treeNodeToRemove = FindAndUpdateCommandTreeNode(draggedCommand);
                    if (treeNodeToRemove != null)
                        treeNodeToRemove.Remove();
                }

				MenuEditorTreeNode newCommandNode = AddMenuCommand(draggedCommand);

                if (newCommandNode != null)
                {
                    newCommandNode.EnsureVisible();
                    
                    this.CommandsTreeView.SelectedNode = newCommandNode;

                    if (MenuItemMoved != null && (e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                    {
                        MenuItemMoved
                            (
                            this,
                            new MenuDesignUIItemEventArgs(
								currentDesignSelectedItem,
								menuBranchAtPoint.MenuItem as MenuBranch,
								null
								)
                            );
                    }

                    // update validation feedback.
                    BaseMenuItem oCommandItem = draggedCommand as BaseMenuItem;
                    MenuBranch oCommandBranch = oCommandItem.Site != null ? oCommandItem.Site.Container as MenuBranch: null;
                    if (oCommandBranch != null)
                    {
                        bool bIsValid = oCommandBranch.ValidateStructure();
                        MenuEditorTreeNode oBranchTreeNode = FindMenuBranchTreeNode(oCommandBranch);
                        if (bIsValid)
                        {
                            oBranchTreeNode.BackColor = m_oOkColor;
                        }
                        else 
                        {
                            oBranchTreeNode.BackColor = m_oErrorColor;
                        }

                        MenuDesignUIItem oGroup = FindNodeGroup(oCommandBranch);
                        if (oGroup != null)
                        {
                            MenuGroup oMenuGroup = oGroup.MenuItem as MenuGroup;
                            bool bIsGroupValid = oMenuGroup.ValidateStructure();
                            CollapsiblePanelLinkLabel oGroupLabel = FindGroupLinkLabel(oMenuGroup);
                            if (bIsGroupValid)
                            {
                                oGroupLabel.BackColor = m_oOkGroupColor;
                            }
                            else
                            {
                                oGroupLabel.BackColor = m_oErrorColor;
                            }
                        }
                    }
                    this.CommandsTreeView.Focus();
                }
                this.Cursor = currentCursor;
            }
        }

        private MenuBranch FindParentBranch(MenuBranch draggedMenuBranch)
        {
            if (draggedMenuBranch == null)            
            {
                return null;
            }

            MenuBranch oParentBranch = null;
            MenuEditorTreeNode oTreeNode = FindMenuBranchTreeNode(draggedMenuBranch);
            foreach (TreeNode oNode in this.GetMenuBranchesTreeView().Nodes)
            {
                if (oNode == oTreeNode)
                {
                    break;
                }
                // it may be a child of the current node.

                foreach (TreeNode oChildNode in oNode.Nodes)
                {
                    if (oChildNode == oTreeNode)
                    {
                        oParentBranch = (MenuBranch)((MenuEditorTreeNode)oNode).MenuItem;
                        return oParentBranch;
                    }
                }
            }
            return oParentBranch;
        }

        //---------------------------------------------------------------------
        private void MenuBranchesTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e == null)
                return;

			MenuEditorTreeNode aNodeToDrag = e.Item as MenuEditorTreeNode;
			if (aNodeToDrag == null || aNodeToDrag.MenuItem == null || !(aNodeToDrag.MenuItem is MenuBranch))
                return;

			MenuBranch aMenuBranchToDrag = aNodeToDrag.MenuItem as MenuBranch;

            bool isControlPressed = (Control.ModifierKeys == Keys.Control);
            DragDropEffects currentEffect = isControlPressed ? DragDropEffects.Copy : DragDropEffects.Move;

			MenuBranch draggedMenuBranch = isControlPressed ? aMenuBranchToDrag.Clone() : aMenuBranchToDrag;

			if (!draggedMenuBranch.CanBeCustomized)
				return;

            if (isControlPressed)
                draggedMenuBranch.Name = String.Empty;

            this.MenuBranchesTreeView.DoDragDrop(draggedMenuBranch, currentEffect);
        }
        
        //---------------------------------------------------------------------
        private void CommandsTreeView_DragEnter(object sender, DragEventArgs e)
        {
            ClearDragFeedback();
            
            if (e == null || e.Data == null)
                return;
            
            e.Effect = GetCommandsTreeViewDragDropEffect(e.Data, e.AllowedEffect);
        }

        //---------------------------------------------------------------------
        private void CommandsTreeView_DragOver(object sender, DragEventArgs e)
        {
            if (e == null || e.Data == null)
                return;

            e.Effect = GetCommandsTreeViewDragDropEffect(e.Data, e.AllowedEffect);

            if (e.Effect == DragDropEffects.None)
                ClearDragFeedback();
        }

        
        //---------------------------------------------------------------------
        private void CommandsTreeView_DragDrop(object sender, DragEventArgs e)
        {
            ClearDragFeedback();

            this.CommandsTreeView.Cursor = Cursors.Default;
     
            if (e == null || e.Data == null || this.MenuBranchesTreeView.SelectedNode == null)
                return;

            if
                (
                e.Data.GetDataPresent(typeof(DocumentMenuCommand)) ||
                e.Data.GetDataPresent(typeof(BatchMenuCommand)) ||
                e.Data.GetDataPresent(typeof(ReportMenuCommand)) ||
                e.Data.GetDataPresent(typeof(FunctionMenuCommand)) ||
                e.Data.GetDataPresent(typeof(TextMenuCommand)) ||
                e.Data.GetDataPresent(typeof(ExeMenuCommand)) ||
                e.Data.GetDataPresent(typeof(OfficeItemMenuCommand))
                )
            {
                string[] dataFormats = e.Data.GetFormats();
                if (dataFormats == null || dataFormats.Length == 0)
                    return;

                MenuCommand draggedCommand = e.Data.GetData(dataFormats[0], true) as MenuCommand;
                if (draggedCommand == null)
                    return;
     
                Cursor currentCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                Point currentMouseClientPos = this.CommandsTreeView.PointToClient(Control.MousePosition);
				MenuEditorTreeNode commandAtPoint = this.CommandsTreeView.GetNodeAt(currentMouseClientPos) as MenuEditorTreeNode;
                
                // Se sono nella parte alta di un nodo di comando, il nuovo comando va inserito sopra a 
                // quello su cui mi trovo, altrimenti sotto
				MenuEditorTreeNode nodeBefore = null;
                if (commandAtPoint == null)
                {
                    if (this.CommandsTreeView.Nodes.Count > 0)
						nodeBefore = this.CommandsTreeView.Nodes[this.CommandsTreeView.Nodes.Count - 1] as MenuEditorTreeNode;
                }
                else if (commandAtPoint.Bounds.Top + commandAtPoint.Bounds.Height / 2 > currentMouseClientPos.Y)
					nodeBefore = commandAtPoint.PrevNode as MenuEditorTreeNode;
                else
                    nodeBefore = commandAtPoint;

                if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                {
					MenuEditorTreeNode treeNodeToRemove = FindAndUpdateCommandTreeNode(draggedCommand);
                    if (treeNodeToRemove != null)
                        treeNodeToRemove.Remove();
                }
				MenuEditorTreeNode newCommandNode = InsertMenuCommand(draggedCommand, nodeBefore);

                if (newCommandNode != null)
                {
                    newCommandNode.EnsureVisible();
                    
                    this.CommandsTreeView.SelectedNode = newCommandNode;
                    
                    if (MenuItemMoved != null && (e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                    {
                        MenuItemMoved
                            (
                            this,
                            new MenuDesignUIItemEventArgs(
								currentDesignSelectedItem,
								(this.MenuBranchesTreeView.SelectedNode as MenuEditorTreeNode).MenuItem as MenuBranch,
								(nodeBefore != null) ? nodeBefore.MenuItem : null
								)
                            );
                    }
                    
                    this.CommandsTreeView.Focus();
                }
                this.Cursor = currentCursor;
            }
        }

        //---------------------------------------------------------------------
        private void CommandsTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e == null)
                return;

			MenuEditorTreeNode aNodeToDrag = e.Item as MenuEditorTreeNode;
			if (aNodeToDrag == null || aNodeToDrag.MenuItem == null || !(aNodeToDrag.MenuItem is MenuCommand))
                return;

            MenuCommand aMenuCommandToDrag = aNodeToDrag.MenuItem as MenuCommand;

			if (!aMenuCommandToDrag.CanBeCustomized)
				return;

            bool isControlPressed = (Control.ModifierKeys == Keys.Control);
            DragDropEffects currentEffect = isControlPressed ? DragDropEffects.Copy : DragDropEffects.Move;

			BaseMenuItem draggedMenuCommand = isControlPressed ? aMenuCommandToDrag.Clone() : aMenuCommandToDrag;

			if (!draggedMenuCommand.CanBeCustomized)
				return;

            this.CommandsTreeView.DoDragDrop(aMenuCommandToDrag, currentEffect);
        }

        //---------------------------------------------------------------------
        private void CommandsTreeView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
        }
        
        //---------------------------------------------------------------------
        private void ApplicationCollapsiblePanel_DesignSelectionChanged(object sender, EventArgs e)
        {
			CollapsiblePanel aCollapsiblePanel = sender as CollapsiblePanel;
			if (aCollapsiblePanel == null || aCollapsiblePanel.IsLinkLabelDesignSelected)
                return;

            ClearMenuBranchesTreeView();

            currentSelectedGroupLabel = null;

			if (!aCollapsiblePanel.IsLinkLabelDesignSelected)
				currentDesignSelectedItem = new MenuDesignUIItem(aCollapsiblePanel);
            
            OnDesignSelectionChanged(new MenuDesignUIItemEventArgs(new MenuDesignUIItem(aCollapsiblePanel)));
        }

        //---------------------------------------------------------------------
        private void ApplicationCollapsiblePanel_CollapsiblePanelLinkLabelRemoved(object sender, ControlEventArgs e)
        {
			if (e == null)
				return;

			CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = e.Control as CollapsiblePanelLinkLabel;
            if
                (
                GroupLinkLabelRemoved != null &&
			    aCollapsiblePanelLinkLabel != null &&
				aCollapsiblePanelLinkLabel.MenuGroup != null
                )
            {
                if 
                    (currentDesignSelectedItem != null &&
                    currentDesignSelectedItem.Control == e.Control
                    )
                {
                    currentDesignSelectedItem = null;

					OnDesignSelectionChanged(MenuDesignUIItemEventArgs.Empty);
                }

                if (GroupLinkLabelRemoved != null)
                    GroupLinkLabelRemoved(this, e);
            }
        }

        //---------------------------------------------------------------------
        private void ApplicationCollapsiblePanel_InitDrag(object sender, EventArgs e)
        {
			CollapsiblePanel aCollapsiblePanel = sender as CollapsiblePanel;
            if
			 (
			 aCollapsiblePanel == null || 
             aCollapsiblePanel.MenuApplication == null
             )
                return;

            bool isControlPressed = (Control.ModifierKeys == Keys.Control);
            DragDropEffects currentEffect = isControlPressed ? DragDropEffects.Copy : DragDropEffects.Move;

            MenuApplication draggedApplication = isControlPressed
				? aCollapsiblePanel.MenuApplication.Clone()
				: aCollapsiblePanel.MenuApplication;

			if (!draggedApplication.CanBeCustomized)
				return;

            if (isControlPressed)
                draggedApplication.Name = String.Empty;

            this.ApplicationsPanel.DoDragDrop(draggedApplication, currentEffect);
        }

        //---------------------------------------------------------------------
        private void ApplicationsPanel_ControlRemoved(object sender, ControlEventArgs e)
        {
			if (e == null)
				return;

			CollapsiblePanel aCollapsiblePanel = e.Control as CollapsiblePanel;
            if
                (
                ApplicationPanelRemoved != null &&
                aCollapsiblePanel != null &&
                aCollapsiblePanel.MenuApplication != null
                )
            {
                if
                   (
                   currentDesignSelectedItem != null &&
                   currentDesignSelectedItem.Control == e.Control
                   )
                {
                    currentDesignSelectedItem = null;

					OnDesignSelectionChanged(MenuDesignUIItemEventArgs.Empty);
                }

                if (ApplicationPanelRemoved != null)
                    ApplicationPanelRemoved(this, e);
            }
        }

        //---------------------------------------------------------------------
        [Browsable(false)]
        public MenuDesignUIItem DesignSelectedItem { get { return currentDesignSelectedItem;  } }

		//---------------------------------------------------------------------
        [Browsable(false)]
		internal CollapsiblePanelLinkLabel CurrentSelectedGroupLabel { get { return currentSelectedGroupLabel; } }

        //---------------------------------------------------------------------
        [DefaultValue(""), Browsable(false)]
        public string MenuFile
        {
            get { return menuFileFullPath; }
            set
            {
                if (!String.IsNullOrEmpty(value) && Path.IsPathRooted(value))
                {
                    menuFileFullPath = value;

                    string menuFileFolder = Path.GetDirectoryName(menuFileFullPath);
                    menuDirectoryInfo = (!String.IsNullOrEmpty(menuFileFolder) && Directory.Exists(menuFileFolder)) ? new DirectoryInfo(menuFileFolder) : null;
                }
                else
                {
                    menuFileFullPath = String.Empty;
                    menuDirectoryInfo = null;
                }

            }
        }

		//---------------------------------------------------------------------
		//Abbiamo dovuto intercettare anche il click sull' albero dei branch per il seguente motivo:
		//Nel caso in cui ci sia un solo branch che ha due documenti figli, se selziono il branch e poi uno dei due documenti,
		//il treeview perde il fuoco ma il nodo del branch rimane selezionato.
		//ci`o fa si che se riselezioniamo il nodo del branch, siccome era gi`a selezionato allora non
		//scatta piu' l'evento AfterSelect e cosi` non agisce pi`u la logica che abbiamo sulla seleizone degli oggetti
		//(sincronizzazione propertu grid ecc).
		private void MenuBranchesTreeView_Click(object sender, EventArgs e)
		{
			MenuEditorTreeNode selectedNode = this.MenuBranchesTreeView.GetNodeAt(this.MenuBranchesTreeView.PointToClient(MousePosition)) as MenuEditorTreeNode;
			if (selectedNode == null)
				return;

			MenuBranchesTreeView_AfterSelect(sender, new TreeViewEventArgs(selectedNode));
		}

		//---------------------------------------------------------------------
		internal static void EmptyClipboard()
		{
			Clipboard.SetDataObject(new DataObject());
		}

        internal MenuDesignUIItem FindNodeGroup(MenuBranch oBranch)
        {
            if(oBranch == null)
            {
                return null;
            }
            string[] asNames = oBranch.Name.Split('.');

            if (asNames.Length > 0)
            {
                string sGroupName = asNames[0];
                for (int i = 1; i < asNames.Length - 1; i++)
                {
                    sGroupName +=  "." + asNames[i] ;
                }
                return FindGroupByName(sGroupName);
            }
            return null;
        }
    }

	//---------------------------------------------------------------------
	internal static class StaticResources
	{
		private static Font customizableItemFont = new Font(new FontFamily("Verdana"), 10, FontStyle.Italic | FontStyle.Bold);
		private static Font nonCustomizableItemFont = new Font(new FontFamily("Verdana"), 10, FontStyle.Regular);

		/// <remarks/>
		//---------------------------------------------------------------------
		internal static Font CustomizableItemFont
		{
			get { return customizableItemFont; }
		}

		/// <remarks/>
		//---------------------------------------------------------------------
		internal static Font NonCustomizableItemFont
		{
			get { return nonCustomizableItemFont; }
		}

		/// <remarks/>
		//---------------------------------------------------------------------
		internal static Color CustomizableItemColor
		{
			get { return Color.Green; }
		}

		/// <remarks/>
		//---------------------------------------------------------------------
		internal static Color NonCustomizableItemColor
		{
			get { return Color.Gray; }
		}
	}

    //=========================================================================
	internal class MenuDesignUIItem
    {
        private Control control;
		private MenuEditorTreeNode treeNode;
		private BaseMenuItem menuItem;

        //---------------------------------------------------------------------
        public MenuDesignUIItem(Control aControl)
        {
			CollapsiblePanel aCollapsiblePanel = aControl as CollapsiblePanel;
			CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = aControl as CollapsiblePanelLinkLabel;

			if (aCollapsiblePanel != null && aCollapsiblePanel.MenuApplication != null)
			{
				this.menuItem = aCollapsiblePanel.MenuApplication;
				control = aControl;
				return;
			}

			if (aCollapsiblePanelLinkLabel != null && aCollapsiblePanelLinkLabel.MenuGroup != null)
			{
				this.menuItem = aCollapsiblePanelLinkLabel.MenuGroup;
				control = aControl;
				return;
			}
        }

        //---------------------------------------------------------------------
		public MenuDesignUIItem(TreeNode aTreeNode)
        {
			MenuEditorTreeNode treeNode = aTreeNode as MenuEditorTreeNode;
            if 
                (
				treeNode != null &&
				treeNode.TreeView != null
                )
            {
				if (treeNode.MenuItem is MenuBranch || treeNode.MenuItem is MenuCommand)
                {
					control = treeNode.TreeView;
					this.treeNode = treeNode;
					menuItem = treeNode.MenuItem;
                }
            }
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            if (menuItem != null)
                menuItem.ToString();

            return base.ToString();
        }

        //---------------------------------------------------------------------
        public override bool Equals(object obj)
        {
			MenuDesignUIItem aMenuDesignUIItem = obj as MenuDesignUIItem;
			if (aMenuDesignUIItem == null)
                return false;

            if (obj == this)
                return true;

			if (aMenuDesignUIItem.Control != control)
                return false;

			if (aMenuDesignUIItem.MenuItem == menuItem)
                return true;

			if (aMenuDesignUIItem.MenuItem == null || aMenuDesignUIItem.MenuItem.GetType() != menuItem.GetType())
                return false;

			BaseMenuItem aBaseMenuItem = menuItem as BaseMenuItem;
			if (aBaseMenuItem != null) // MenuApplication, MenuGroup e MenuBranch derivano da BaseMenuItem
				return String.Compare(aMenuDesignUIItem.MenuItem.Name, aBaseMenuItem.Name) == 0;

			MenuCommand menuItemCommand = menuItem as MenuCommand;
			if (menuItemCommand != null)
                return
					aMenuDesignUIItem.TreeNode == treeNode &&
					String.Compare(aMenuDesignUIItem.MenuItem.Title.ToString(), menuItemCommand.Title.ToString()) == 0 &&
					String.Compare(((MenuCommand)aMenuDesignUIItem.MenuItem).CommandObject, menuItemCommand.CommandObject) == 0;

            return false;
        }

        // if I override Equals, I must override GetHashCode as well... 
        //---------------------------------------------------------------------------
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        //---------------------------------------------------------------------
        public Control Control { get { return control; } }
        //---------------------------------------------------------------------
		public MenuEditorTreeNode TreeNode { get { return treeNode; } }
        //---------------------------------------------------------------------
        public BaseMenuItem MenuItem { get { return menuItem; } }
        //---------------------------------------------------------------------
        public MenuApplication Application 
        {
            get 
            {
                return (control != null && control is CollapsiblePanel) ? menuItem as MenuApplication : null; 
            }
        }

        //---------------------------------------------------------------------
        public MenuGroup Group 
        {
            get 
            { 
                return (control != null && control is CollapsiblePanelLinkLabel) ? menuItem as MenuGroup : null; 
            }
        }

        //---------------------------------------------------------------------
        public MenuBranch MenuBranch
        {
            get
            {
                return (control != null && control is TreeView && treeNode != null) ? menuItem as MenuBranch : null;
            }
        }

        //---------------------------------------------------------------------
        public MenuCommand Command
        {
            get
            {
                return (control != null && control is TreeView && treeNode != null) ? menuItem as MenuCommand : null;
            }
        }

        //---------------------------------------------------------------------
        public string CommandItemObject
        {
            get
            {
                if (control == null || !(control is TreeView) || treeNode == null)
                    return String.Empty;

                MenuCommand command = this.Command;
                if (command == null)
                    return String.Empty;

                return command.CommandObject;
            }
        }

        //---------------------------------------------------------------------
        public bool IsDesignSelected
        {
            set
            {
                if (control == null)
                    return;
                
                if (control is TreeView && treeNode != null)
                {
                    if (value)
                        ((TreeView)control).SelectedNode = treeNode;
                    else if (((TreeView)control).SelectedNode == treeNode)
                        ((TreeView)control).SelectedNode = null;
                    return;
                }

                if (control is CollapsiblePanelLinkLabel)
                {
                    ((CollapsiblePanelLinkLabel)control).IsDesignSelected = value;
                    return;
                }

                if (control is CollapsiblePanel)
                {
                    ((CollapsiblePanel)control).IsDesignSelected = value;
                    return;
                }
            }
            get
            {
                if (control == null)
                    return false;
                if (treeNode != null)
                    return (control is TreeView) ? (((TreeView)control).SelectedNode == treeNode) : false;
                if (control is CollapsiblePanelLinkLabel)
                    return ((CollapsiblePanelLinkLabel)control).IsDesignSelected;
                if (control is CollapsiblePanel)
                    return (((CollapsiblePanel)control).IsDesignSelected && !((CollapsiblePanel)control).IsLinkLabelDesignSelected);

                return false;
            }
        }

        //---------------------------------------------------------------------
        public bool IsValidClipboardDataTarget()
        {
            if (menuItem == null)
                return false;

            IDataObject data = Clipboard.GetDataObject();
            if (data == null)
                return false;

            if (!(menuItem is MenuCommand) && data.GetDataPresent(menuItem.GetType().FullName))
                return true;

            if (menuItem is MenuApplication && data.GetDataPresent(typeof(MenuGroup).FullName))
                return true;

            if (menuItem is MenuGroup && data.GetDataPresent(typeof(MenuBranch).FullName))
                return true;

            if (menuItem is MenuBranch || menuItem is MenuCommand)
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

            return false;
        }

        //---------------------------------------------------------------------
        public bool IsParentToBeRemoved()
        {
            return (TreeNode.Parent != null && TreeNode.Parent.Nodes.Count == 1);
        }
	}

	//=========================================================================
	internal class MenuDesignUIItemEventArgs : EventArgs
	{
		private static MenuDesignUIItemEventArgs empty = InitEmptyEventArgs();

		//---------------------------------------------------------------------
		internal new static MenuDesignUIItemEventArgs Empty
		{
			get { return empty; }
		}

		//---------------------------------------------------------------------
		private static MenuDesignUIItemEventArgs InitEmptyEventArgs()
		{
			return new MenuDesignUIItemEventArgs(null, null, null);
		}

		//---------------------------------------------------------------------
		public MenuDesignUIItem WorkingMenuDesignUIItem { get; set; }
		public BaseMenuItem TargetMenuItem { get; set; }
		public BaseMenuItem MenuObjectBefore { get; set; }

		//---------------------------------------------------------------------
		public MenuDesignUIItemEventArgs(
			MenuDesignUIItem workingMenuDesignUIItem,
			BaseMenuItem targetMenuItem,
			BaseMenuItem menuObjectBefore			
			)
		{
			this.WorkingMenuDesignUIItem = workingMenuDesignUIItem;
			this.TargetMenuItem = targetMenuItem;
			this.MenuObjectBefore = menuObjectBefore;
		}

		//---------------------------------------------------------------------
		public MenuDesignUIItemEventArgs(
			MenuDesignUIItem workingMenuDesignUIItem
			)
			: this(workingMenuDesignUIItem, null, null)
		{}
	}

	//=========================================================================
	/// <remarks/>
	internal class MenuEditorTreeNode : TreeNode
	{
		private BaseMenuItem menuItem;
		private bool isCut;
		private bool expanded;

		internal BaseMenuItem MenuItem { get { return menuItem; } set { menuItem = value; } }

		//---------------------------------------------------------------------
		internal bool IsCut
		{
			get
			{
				return isCut; 
			}
			set
			{
				isCut = value;
				this.ImageIndex = this.SelectedImageIndex = GetCommandImageIndex();
			}
		}

		//---------------------------------------------------------------------
		internal bool Expanded
		{
			get
			{
				return expanded; 
			}
			set
			{
				expanded = value;
				this.ImageIndex = this.SelectedImageIndex = GetCommandImageIndex();
			}
		}

		//---------------------------------------------------------------------
		public MenuEditorTreeNode(BaseMenuItem menuItem)
			: base(menuItem.Title.ToString())
		{
			this.menuItem = menuItem;
			this.Name = this.Text;
			
			this.ImageIndex = this.SelectedImageIndex = GetCommandImageIndex();

			this.NodeFont = MenuItem.CanBeCustomized
				? StaticResources.CustomizableItemFont
				: StaticResources.NonCustomizableItemFont;

			this.ForeColor = MenuItem.CanBeCustomized
				? StaticResources.CustomizableItemColor
				: StaticResources.NonCustomizableItemColor;
		}

		//---------------------------------------------------------------------
		private int GetCommandImageIndex()
		{
			if (menuItem is MenuBranch)
			{
				if (isCut)
					return expanded 
						? (int)ImageLists.MenuBranchImageIndex.OpenedFolder_Cut
						: (int)ImageLists.MenuBranchImageIndex.ClosedFolder_Cut;
				else
					return expanded
						  ? (int)ImageLists.MenuBranchImageIndex.OpenedFolder_Normal
						  : (int)ImageLists.MenuBranchImageIndex.ClosedFolder_Normal;
			}

			if (menuItem is DocumentMenuCommand)
				return isCut
					? (int)ImageLists.MenuCommandImageIndex.RunDocument_Cut
					: (int)ImageLists.MenuCommandImageIndex.RunDocument_Normal;

			if (menuItem is BatchMenuCommand)
			{
				return ((BatchMenuCommand)menuItem).IsWizardBatch
					? isCut
						? (int)ImageLists.MenuCommandImageIndex.RunWizardBatch_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunWizardBatch_Normal
					: isCut
						? (int)ImageLists.MenuCommandImageIndex.RunBatch_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunBatch_Normal;
			}
				
			if (menuItem is ReportMenuCommand)
				return isCut
					? (int)ImageLists.MenuCommandImageIndex.RunReport_Cut
					: (int)ImageLists.MenuCommandImageIndex.RunReport_Normal;

			if (menuItem is FunctionMenuCommand)
			{
				FunctionMenuCommand aFunctionMenuCommand = menuItem as FunctionMenuCommand;
				if (aFunctionMenuCommand.IsRunDocumentFunction)
					return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunDocumentFunction_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunDocumentFunction_Normal;

				if (aFunctionMenuCommand.IsRunBatchFunction)
					return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunBatchFunction_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunBatchFunction_Normal;

				if (aFunctionMenuCommand.IsRunReportFunction)
					return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunReportFunction_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunReportFunction_Normal;

				if (aFunctionMenuCommand.IsRunExecutableFunction)
					return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunExeFunction_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunExeFunction_Normal;

				if (aFunctionMenuCommand.IsRunTextFunction)
					return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunTextFunction_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunTextFunction_Normal;

				return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunFunction_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunFunction_Normal;

			}

			if (menuItem is ExeMenuCommand)
				return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunExe_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunExe_Normal;

			if (menuItem is TextMenuCommand)
				return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunText_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunText_Normal;

			if (menuItem is OfficeItemMenuCommand)
			{
				OfficeItemMenuCommand aOfficeItemMenuCommand = menuItem as OfficeItemMenuCommand;
				if (aOfficeItemMenuCommand.IsWordItem)
				{
					if (aOfficeItemMenuCommand.IsWordDocument)
						return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunWordDocument_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunWordDocument_Normal;

					if (aOfficeItemMenuCommand.IsWordTemplate)
						return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunWordTemplate_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunWordTemplate_Normal;

					return isCut
						? (int)ImageLists.MenuCommandImageIndex.Word16x16_Cut
						: (int)ImageLists.MenuCommandImageIndex.Word16x16_Normal;
				}

				if (aOfficeItemMenuCommand.IsExcelItem)
				{
					if (aOfficeItemMenuCommand.IsExcelDocument)
						return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunExcelDocument_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunExcelDocument_Normal;

					if (aOfficeItemMenuCommand.IsExcelTemplate)
						return isCut
						? (int)ImageLists.MenuCommandImageIndex.RunExcelTemplate_Cut
						: (int)ImageLists.MenuCommandImageIndex.RunExcelTemplate_Normal;

					return isCut
						? (int)ImageLists.MenuCommandImageIndex.OfficeItem16x16_Cut
						: (int)ImageLists.MenuCommandImageIndex.OfficeItem16x16_Normal;
				}
				return isCut
						? (int)ImageLists.MenuCommandImageIndex.OfficeItem16x16_Cut
						: (int)ImageLists.MenuCommandImageIndex.OfficeItem16x16_Normal;
			}

			return -1;
		}

        internal bool ValidateStructure(MenuEditorTreeNode parentTreeNode)
        {
            MenuBranch oBranch = this.menuItem as MenuBranch;
            
            if ((oBranch == null || oBranch.Menus.Count == 0 || oBranch.Commands.Count > 0) && parentTreeNode == null)            
            {
                // it is a second level, it should have one child node and no command children.
                return false;

            }
            return true;
        }
    }
}
