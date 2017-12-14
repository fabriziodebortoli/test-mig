using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	public delegate void MenuMngCtrlEventHandler(object sender, MenuMngCtrlEventArgs e);
	public delegate void MenuMngCtrlCancelEventHandler(object sender, MenuMngCtrlCancelEventArgs e);
	public delegate void MenuMngCtrlTreeViewEventHandler(object sender, MenuMngCtrlTreeViewEventArgs e);
	public delegate void MenuMngCtrlTreeViewCancelEventHandler(object sender, MenuMngCtrlTreeViewCancelEventArgs e);
	
	/// <summary>
	/// Summary description for MenuMngWinCtrl.
	/// </summary>
	//============================================================================
	public partial class MenuMngWinCtrl : System.Windows.Forms.UserControl
	{
		private MenuXmlParser	menuXmlParser = null;
		private IPathFinder				pathFinder = null;

		private bool showEnhancedCommandsView = false;

      private MenuLoadingPanel LoadingPanel;

		private	ILoginManager	currentLoginManager = null;
		private string			currentAuthenticationToken = String.Empty;
		private bool			keyboardInputEnabled = true;
		private Image			treeBackgroundImage = null;
		
		private MenuApplicationPanel	currentApplicationPanel;
		private GroupLinkLabel			currSelectedGroupLabel;
		private MenuTreeNode			currSelectedMenuTreeNode;
		private MenuTreeNode			currSelectedCommandTreeNode;
		private CommandListBoxItem		currSelectedCommandListBoxItem = null;

		public event MenuMngCtrlEventHandler MenuXmlParserChanging;
		public event MenuMngCtrlEventHandler MenuXmlParserChanged;
		public event MenuMngCtrlCancelEventHandler SelectedApplicationChanging;
		public event MenuMngCtrlEventHandler SelectedApplicationChanged;
		public event MenuMngCtrlCancelEventHandler SelectedGroupChanging;
		public event MenuMngCtrlEventHandler SelectedGroupChanged;
		public event MenuMngCtrlTreeViewCancelEventHandler SelectedMenuChanging;
		public event MenuMngCtrlTreeViewEventHandler SelectedMenuChanged;
		public event MenuMngCtrlCancelEventHandler SelectedCommandChanging;
		public event MenuMngCtrlEventHandler SelectedCommandChanged;
		public event MenuMngCtrlEventHandler DisplayMenuItemsContextMenu;
		public event MenuMngCtrlEventHandler EnhancedCommandsViewContextMenuDisplayed;
		public event MenuMngCtrlEventHandler RunCommand;
		
		public event System.EventHandler MenuTreeViewFilled;
		public event System.EventHandler CommandsTreeViewFilled;
	
		public event System.EventHandler LoginCancelled;

		public event MenuMngCtrlTreeViewEventHandler BeforeCommandExpanded;
		public event MenuMngCtrlTreeViewEventHandler BeforeCommandCollapsed;
		public event EventHandler<KeyEventArgs> ChildKeyUp;
		public event EventHandler<KeyEventArgs> ChildKeyDown;
		ITheme theme;
		//---------------------------------------------------------------------------
		public MenuMngWinCtrl()
		{
			currentApplicationPanel = null;
			currSelectedGroupLabel = null;
			currSelectedMenuTreeNode = null;
			currSelectedCommandTreeNode = null;
			currSelectedCommandListBoxItem = null;

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			theme = DefaultTheme.GetTheme();
		}
				
		#region MenuMngWinCtrl public properties

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public Image TreeBackgroundImage
		{
			get
			{
				if (treeBackgroundImage == null && currentLoginManager != null)
				{
					string ns = "";
					if (currentLoginManager.IsDemo())
					{
						ns = "Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.DemoBkgnd.jpg";
					}
					else if (currentLoginManager.IsDeveloperActivation())
					{
						ns = "Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.DevelopmentBkgnd.jpg";
					}
					else if (currentLoginManager.IsReseller())
					{
						ns = "Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.ResellerBkgnd.jpg";
					}
					else if (currentLoginManager.IsDistributor())
					{
						ns = "Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.DistributorBkgnd.jpg";
					}
					
					treeBackgroundImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(ns));
				}
				return treeBackgroundImage;
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public string CurrentApplicationName
		{
			get
			{
				return (menuXmlParser != null) ? menuXmlParser.GetCurrentApplicationName() : String.Empty;
			}
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public string CurrentApplicationTitle
		{
			get
			{
				return (menuXmlParser != null) ? menuXmlParser.GetCurrentApplicationTitle() : String.Empty;
			}
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public string CurrentGroupName
		{
			get
			{
				return (menuXmlParser != null) ? menuXmlParser.GetCurrentGroupName() : String.Empty;
			}
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuApplicationPanel CurrentApplicationPanel
		{
			get
			{
				return currentApplicationPanel;
			}
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlNode CurrentApplicationNode
		{
			get
			{
				return (currentApplicationPanel != null) ? currentApplicationPanel.ApplicationNode : null;
			}
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public GroupLinkLabel SelectedGroupLabel
		{
			get
			{
				return currSelectedGroupLabel;
			}
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlNode CurrentGroupNode
		{
			get
			{
				return (currSelectedGroupLabel != null) ? currSelectedGroupLabel.Node : null;
			}
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public bool CurrentGroupVisited
		{
			get
			{
				return (currSelectedGroupLabel != null) ? currSelectedGroupLabel.GroupVisited : false;
			}
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuTreeNode CurrentMenuTreeNode
		{
			get
			{
				return currSelectedMenuTreeNode;
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlNode CurrentMenuNode
		{
			get
			{
				return (currSelectedMenuTreeNode != null) ? currSelectedMenuTreeNode.Node : null;
			}
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public string CurrentMenuPath
		{
			get
			{
				return (currSelectedMenuTreeNode != null) ? currSelectedMenuTreeNode.GetNamedFullPath() : String.Empty;
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public string CurrentMenuTitle
		{
			get
			{
				return (currSelectedMenuTreeNode != null) ? currSelectedMenuTreeNode.Text : String.Empty;
			}
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuTreeNode CurrentCommandTreeNode
		{
			get
			{
				return currSelectedCommandTreeNode;
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlNode CurrentCommandNode
		{
			get
			{
				return (currSelectedCommandTreeNode != null) ? currSelectedCommandTreeNode.Node : null;
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public string CurrentCommandPath
		{
			get
			{
				return (currSelectedCommandTreeNode != null && currSelectedCommandTreeNode.TreeView != null) ? currSelectedCommandTreeNode.GetNamedFullPath() : String.Empty;
			}
		}

		//---------------------------------------------------------------------------
		public string CurrentCommandTitle
		{
			get
			{
				return (currSelectedCommandTreeNode != null) ? currSelectedCommandTreeNode.Text : String.Empty;
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuApplicationsPanelBar ApplicationsPanel
		{
			get
			{
				return ApplicationsPanelBar;
			}
		}
        
        //---------------------------------------------------------------------------
		public int ApplicationsPanelWidth
		{
			get
			{
				return (ApplicationsPanel != null) ? ApplicationsPanel.Size.Width : 0;
			}
			set
			{
				if (value < 0 || ApplicationsPanel == null)
					return;
				ApplicationsPanel.Size = new System.Drawing.Size(value, ApplicationsPanel.Size.Height);
			}
		}

		//---------------------------------------------------------------------------
		public MenuTreeNodeCollection MenuTreeNodes
		{
			get
			{
				return (MenuTreeView != null) ? MenuTreeView.Nodes : null;
			}
		}

		//---------------------------------------------------------------------------
		public MenuTreeNodeCollection CommandsTreeNodes
		{
			get
			{
				return (CommandsTreeView != null) ? CommandsTreeView.Nodes : null;
			}
		}

		//---------------------------------------------------------------------------
		public int MenuTreeWidth
		{
			get
			{
				return (MenuTreeView != null) ? MenuTreeView.Size.Width : 0;
			}
			set
			{
				if (value < 0 || MenuTreeView == null)
					return;
				MenuTreeView.Size = new System.Drawing.Size(value, MenuTreeView.Size.Height);
			}
		}
		
		//---------------------------------------------------------------------------
		public System.Windows.Forms.ContextMenu ItemsContextMenu
		{
			get
			{
				return MenuItemsContextMenu;
			}
		}

		//---------------------------------------------------------------------------
		public System.Windows.Forms.ContextMenuStrip EnhancedCommandsViewContextMenu
		{
			get
			{
				return EnhancedCommandsViewEnabled ? EnhancedCommandsView.CommandsContextMenu : null;
			}
		}

		//---------------------------------------------------------------------------
		public System.Windows.Forms.ImageList CommandsImageList
		{
			get
			{
				return CommandsTreeView.ImageList;
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlParser MenuXmlParser
		{
			get
			{
				return menuXmlParser;
			}
			set
			{
				if (menuXmlParser != value)
				{
                    ApplicationsPanel.ClearControls();

					if (MenuXmlParserChanging != null)
						MenuXmlParserChanging(this,null);

					menuXmlParser = value;

					currentLoginManager = (menuXmlParser != null) ? menuXmlParser.LoginManager : null;
					currentAuthenticationToken = (currentLoginManager != null) ? currentLoginManager.AuthenticationToken : String.Empty;
					
					MenuTreeView.MenuXmlParser = menuXmlParser;
					CommandsTreeView.MenuXmlParser = menuXmlParser;
					if (EnhancedCommandsView != null)
						EnhancedCommandsView.MenuXmlParser = menuXmlParser;

					ShowApplicationMenu();

					if (MenuXmlParserChanged != null)
						MenuXmlParserChanged(this,null);
				}
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public IPathFinder PathFinder 
		{ 
			get { return pathFinder; } 
			set 
			{ 
				pathFinder = value; 
		
				MenuTreeView.PathFinder = pathFinder;
				CommandsTreeView.PathFinder = pathFinder;
				if (EnhancedCommandsView != null)
					EnhancedCommandsView.PathFinder = pathFinder;
			} 
		}

		//----------------------------------------------------------------------------
		public bool EnhancedCommandsViewEnabled
		{
			get { return (EnhancedCommandsView != null && this.Controls.Contains(EnhancedCommandsView)) && EnhancedCommandsView.Visible; }
			set
			{
				if (value)
				{
					if (EnhancedCommandsView != null && this.Controls.Contains(EnhancedCommandsView))
						return;
	
					this.SuspendLayout();

					if (EnhancedCommandsView == null)
					{
						EnhancedCommandsView = new EnhancedCommandsView();
	
						EnhancedCommandsView.Visible = false;

						EnhancedCommandsView.AllUsersCommandForeColor = System.Drawing.Color.Blue;
						EnhancedCommandsView.BackColor = System.Drawing.SystemColors.Window;
						EnhancedCommandsView.CurrentUserCommandForeColor = System.Drawing.Color.RoyalBlue;
						EnhancedCommandsView.Dock = System.Windows.Forms.DockStyle.Fill;
						EnhancedCommandsView.Font = new System.Drawing.Font(CommandsTreeView.Font.FontFamily, CommandsTreeView.Font.Size);
						EnhancedCommandsView.Location = new System.Drawing.Point(CommandsTreeView.Location.X, CommandsTreeView.Location.Y);
						EnhancedCommandsView.Name = "EnhancedCommandsView";
						EnhancedCommandsView.ShowDocuments = true;
						EnhancedCommandsView.ShowReports = true;
						EnhancedCommandsView.ShowBatches = true;
						EnhancedCommandsView.ShowFunctions = true;
						EnhancedCommandsView.ShowExecutables = true;
						EnhancedCommandsView.ShowTexts = true;
						EnhancedCommandsView.ShowOfficeItems = true;
						EnhancedCommandsView.ShowStateImages = true;
						EnhancedCommandsView.Size = new System.Drawing.Size(CommandsTreeView.Size.Width, CommandsTreeView.Size.Height);
						EnhancedCommandsView.TabIndex = CommandsTreeView.TabIndex + 1;
						EnhancedCommandsView.SelectedCommandChanged += new System.EventHandler(this.EnhancedCommandsView_SelectedCommandChanged);
						EnhancedCommandsView.RunCommand += new MenuMngCtrlEventHandler(EnhancedCommandsView_RunCommand);
						EnhancedCommandsView.ContextMenuDisplayed +=new MenuMngCtrlEventHandler(EnhancedCommandsView_ContextMenuDisplayed);
					}
					// Devo rimuovere tutti i control, altrimenti, se non vengono riaggiunti
					// nell'ordine (da destra a sinistra) non viene reso correttamente il 
					// Dock di tipo Fill del EnhancedCommandsView
					if (CommandsTreeView != null && this.Controls.Contains(CommandsTreeView))
						this.Controls.Remove(CommandsTreeView);
					if (Splitter2 != null && this.Controls.Contains(Splitter2))
						this.Controls.Remove(Splitter2);
					if (MenuTreeView != null && this.Controls.Contains(MenuTreeView))
						this.Controls.Remove(MenuTreeView);
					if (Splitter1 != null && this.Controls.Contains(Splitter1))
						this.Controls.Remove(Splitter1);
					if (ApplicationsPanelBar != null && this.Controls.Contains(ApplicationsPanelBar))
						this.Controls.Remove(ApplicationsPanelBar);

					this.Controls.Add(EnhancedCommandsView);		
					if (CommandsTreeView != null)
						this.Controls.Add(CommandsTreeView);
					if (Splitter2 != null)
						this.Controls.Add(Splitter2);
					if (MenuTreeView != null)
						this.Controls.Add(MenuTreeView);
					if (Splitter1 != null)
						this.Controls.Add(Splitter1);
					if (ApplicationsPanelBar != null)
						this.Controls.Add(ApplicationsPanelBar);
					
					this.ResumeLayout();

					EnhancedCommandsView.MenuXmlParser = CommandsTreeView.MenuXmlParser;
					EnhancedCommandsView.PathFinder = CommandsTreeView.PathFinder;

					EnhancedCommandsView.CurrentMenuNode = (currSelectedMenuTreeNode != null) ? currSelectedMenuTreeNode.Node : null;
					if (currSelectedCommandTreeNode != null && currSelectedCommandTreeNode.Node != null)
					{
						CommandListBoxItem itemToSel = EnhancedCommandsView.FindItem(currSelectedCommandTreeNode.Node.GetFirstLevelAncestorCommand());
						if (itemToSel != null)
							SetCurrentSelectedCommandListBoxItem(itemToSel, false);
					}
				}
				else
				{
					if (EnhancedCommandsView == null)
						return;

					if (this.Controls.Contains(EnhancedCommandsView))
						this.Controls.Remove(EnhancedCommandsView);
					
					currSelectedCommandListBoxItem = null;

					EnhancedCommandsView.Dispose();

					EnhancedCommandsView = null;
				}
			}
		}

		//----------------------------------------------------------------------------
		public bool ShowEnhancedCommandsView
		{
			get { return showEnhancedCommandsView; }
			set
			{
				bool isEnhancedCommandsViewToShow = value;

				if (isEnhancedCommandsViewToShow)
					EnhancedCommandsViewEnabled = true;
				
				bool setFocus = (!showEnhancedCommandsView && CommandsTreeView.Focused) ||
								(showEnhancedCommandsView && EnhancedCommandsView.Focused);

				if (EnhancedCommandsView != null)
				{
					EnhancedCommandsView.Visible = isEnhancedCommandsViewToShow;

					if (isEnhancedCommandsViewToShow)
						this.PerformLayout();
				}

				CommandsTreeView.Visible = !isEnhancedCommandsViewToShow;
				
				if (isEnhancedCommandsViewToShow)
				{
					EnhancedCommandsView.BringToFront();
					if (setFocus) 
						EnhancedCommandsView.Focus();
				}
				else
				{
					CommandsTreeView.BringToFront();
					if (setFocus)
						CommandsTreeView.Focus();
				}

				showEnhancedCommandsView = isEnhancedCommandsViewToShow;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public int CommandsShowFlags { get { return EnhancedCommandsViewEnabled ? EnhancedCommandsView.ShowFlags : -1; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public bool ShowEnhancedCommandsToolBar 
		{ 
			get { return EnhancedCommandsViewEnabled ? EnhancedCommandsView.ShowViewToolBar : false; } 
			set
			{
				if (!EnhancedCommandsViewEnabled)
					return;
				EnhancedCommandsView.ShowViewToolBar = value;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public bool ShowEnhancedCommandsDescriptions 
		{
			get { return EnhancedCommandsViewEnabled ? EnhancedCommandsView.ShowCommandsDescriptions : false; } 
			set
			{
				if (!EnhancedCommandsViewEnabled)
					return;
				EnhancedCommandsView.ShowCommandsDescriptions = value;
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public bool ShowEnhancedCommandsReportDates 
		{
			get { return EnhancedCommandsViewEnabled ? EnhancedCommandsView.ShowReportsDates : false; } 
			set
			{
				if (!EnhancedCommandsViewEnabled)
					return;
				EnhancedCommandsView.ShowReportsDates = value;
			}
		}

		//---------------------------------------------------------------------------
		public bool ShowTreeItemsStateImages
		{
			get
			{
				return
					MenuTreeView.ShowStateImages && 
					(CommandsTreeView.ShowStateImages || (EnhancedCommandsView != null && EnhancedCommandsView.ShowStateImages));
			}
			set
			{
                if (MenuTreeView != null)
                    MenuTreeView.ShowStateImages = value;
				if (CommandsTreeView != null)
					CommandsTreeView.ShowStateImages = value;
				if (EnhancedCommandsView != null)
					EnhancedCommandsView.ShowStateImages = value;
			}
		}
		
		//---------------------------------------------------------------------------
		public System.Drawing.Color CurrentUserCommandForeColor	
		{
			get 
			{
				if (ShowEnhancedCommandsView)
					return (EnhancedCommandsView != null) ? EnhancedCommandsView.CurrentUserCommandForeColor : SystemColors.ControlText;

				return (CommandsTreeView != null) ? CommandsTreeView.CurrentUserCommandForeColor : SystemColors.ControlText;
			} 
			set
			{ 
				if (CommandsTreeView != null)
					CommandsTreeView.CurrentUserCommandForeColor = value; 
				if (EnhancedCommandsView != null)
					EnhancedCommandsView.CurrentUserCommandForeColor = value;
			} 
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Color AllUsersCommandForeColor 
		{ 
			get 
			{
				if (ShowEnhancedCommandsView)
					return (EnhancedCommandsView != null) ? EnhancedCommandsView.AllUsersCommandForeColor : SystemColors.ControlText;

				return (CommandsTreeView != null) ? CommandsTreeView.AllUsersCommandForeColor : SystemColors.ControlText;
			} 
			set
			{ 
				if (CommandsTreeView != null)
					CommandsTreeView.AllUsersCommandForeColor = value; 
				if (EnhancedCommandsView != null)
					EnhancedCommandsView.AllUsersCommandForeColor = value;
			} 
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public ILoginManager LoginManager 
		{
			get { return currentLoginManager; } 
			set
			{
                currentLoginManager = value;
				if (currentLoginManager != null)
					currentAuthenticationToken = currentLoginManager.AuthenticationToken;
			}
		}
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public string AuthenticationToken { get { return currentAuthenticationToken; } }
        ////---------------------------------------------------------------------------
        //[Browsable(false)]
        //public bool ClearCachedData
        //{
        //    get { return LoginPanel == null ? false : LoginPanel.ClearCachedData; }
        //    set { if (LoginPanel != null) LoginPanel.ClearCachedData = value; }
        //}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public bool KeyboardInputEnabled
		{
			get { return keyboardInputEnabled; }
			set { keyboardInputEnabled = value; }
		}
		
		#endregion

		
		#region MenuMngWinCtrl private methods

		//---------------------------------------------------------------------------
		private void ShowApplicationMenu()
		{
			ClearApplicationItems();

			if 
				(
					ApplicationsPanel == null ||
					menuXmlParser == null || 
					menuXmlParser.Root == null || 
					menuXmlParser.Root.ApplicationsItems == null ||
					menuXmlParser.Root.ApplicationsItems.Count == 0
				)
				return;

			Point myPoint = new Point(ApplicationsPanel.XSpacing,ApplicationsPanel.YSpacing);

			ArrayList applicationsList = menuXmlParser.Root.ApplicationsItems;
			int firstAppPanelIndex = (ApplicationsPanel.Panels != null) ? ApplicationsPanel.Panels.Count : 0;
			int appPanelTabIndex = (ApplicationsPanel.Panels != null) ? (ApplicationsPanel.Panels.Count * 2) : 0;
			
			for (int i = 0; i < applicationsList.Count; i++)
			{
				if (applicationsList[i] == null || !(applicationsList[i] is MenuXmlNode))
					continue;

				MenuXmlNode appNode = (MenuXmlNode)applicationsList[i];
				if (appNode == null || !appNode.IsApplication || !appNode.HasCommandDescendantsNodes)
					continue;

                if (appNode.GetApplicationName().ToLowerInvariant() == "tbs")
                    continue;

				MenuApplicationPanel myAppPanel = new MenuApplicationPanel(this, appNode);
				
				myAppPanel.Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Regular);
				myAppPanel.Location = myPoint;
				myAppPanel.Name = "ApplicationPanel" + (firstAppPanelIndex + i).ToString();
				myAppPanel.State = MenuApplicationPanel.PanelState.Collapsed;
				myAppPanel.TabStop = true;
				myAppPanel.TabIndex = appPanelTabIndex;

				myAppPanel.InitializeGroupLabels();
				
				AddMenuApplicationPanel(myAppPanel);

				myPoint.Y += myAppPanel.Height + 2;
				appPanelTabIndex += 2;
			}
            
            ApplicationsPanel.UpdateAllPanelsPositions();
		}
	
		//---------------------------------------------------------------------------
		private bool SelectMenuNodeFromFullPath(string aMenuFullPath)
		{
			if (aMenuFullPath == null || aMenuFullPath.Length == 0 || MenuTreeView ==  null || MenuTreeView.Nodes == null)
				return false;
			
			MenuTreeNodeCollection nodes = MenuTreeView.Nodes;
			string path = aMenuFullPath;

			bool nodeFound = false;
			MenuTreeNode nodeToSel = null;
			do
			{
				nodeFound = false;
				int sepPos = path.IndexOf(MenuTreeView.PathSeparator);
				string menuItem = (sepPos >= 0 ) ? path.Substring(0, sepPos) : path;
				path = (sepPos >= 0 ) ? path.Substring(sepPos + MenuTreeView.PathSeparator.Length) : String.Empty; 
				if (menuItem.Length > 0)
				{
					foreach (MenuTreeNode node in nodes)
					{
						string nodeName = node.GetNameAttribute();
						if (
							(nodeName != null && nodeName.Length > 0 && String.Compare(menuItem, nodeName) == 0) ||
							node.Text == menuItem
							)
						{
							node.Expand();
							nodes = node.Nodes;
							nodeFound = true;
							nodeToSel = node;
							break;
						}
					}
				}
			}while (nodeFound && nodes != null && path != null && path.Length > 0);
			if (nodeToSel != null)
			{
				SetCurrentSelectedMenuTreeNode(nodeToSel);
				MenuTreeView.Focus();
			}
			return nodeFound;
		}
	
		//---------------------------------------------------------------------------
		private bool SelectCommandNodeFromFullPath(string aCommandFullpath)
		{
			if (aCommandFullpath == null || aCommandFullpath.Length == 0 || CommandsTreeView == null || CommandsTreeView.Nodes == null)
				return false;
			
			MenuTreeNodeCollection nodes = CommandsTreeView.Nodes;
			string path = aCommandFullpath;

			bool nodeFound = false;
			MenuTreeNode nodeToSel = null;
			do
			{
				nodeFound = false;
				int sepPos = path.IndexOf(CommandsTreeView.PathSeparator);
				string commandItem = (sepPos >= 0 ) ? path.Substring(0, sepPos) : path;
				path = (sepPos >= 0 ) ? path.Substring(sepPos + CommandsTreeView.PathSeparator.Length) : String.Empty; 
				if (commandItem.Length > 0)
				{
					foreach (MenuTreeNode node in nodes)
					{
						string nodeItemObject = node.GetCommandItemObject();
						if (
							(nodeItemObject != null && nodeItemObject.Length > 0 && String.Compare(commandItem, nodeItemObject) == 0) ||
							node.Text == commandItem
							)
						{
							node.Expand();
							nodes = node.Nodes;
							nodeFound = true;
							nodeToSel = node;
							break;
						}
					}
				}
			}while (nodeFound && nodes != null && path != null && path.Length > 0);
			if (nodeToSel != null)
			{
				SetCurrentSelectedCommandTreeNode(nodeToSel);

				if (ShowEnhancedCommandsView && EnhancedCommandsView != null)
					EnhancedCommandsView.Focus();
				else
					CommandsTreeView.Focus();
			}
			return nodeFound;
		}
		
		//---------------------------------------------------------------------------
		private bool SetCurrentSelectedMenuTreeNode(MenuTreeNode aTreeNode)
		{
			return SetCurrentSelectedMenuTreeNode(aTreeNode, true);
		}
		
		//---------------------------------------------------------------------------
		private bool SetCurrentSelectedMenuTreeNode(MenuTreeNode aTreeNode, bool raiseBeforeEvent)
		{
			if (MenuTreeView == null || MenuTreeView.Nodes == null || MenuTreeView.Nodes.Count == 0)
			{
				currSelectedMenuTreeNode = null;
				return true;
			}

			if (aTreeNode != null && (aTreeNode.TreeView != MenuTreeView || aTreeNode.Node == null))
			{
				throw new Exception("Invalid node selection!");
			}

			if (currSelectedMenuTreeNode != null && currSelectedMenuTreeNode.Equals(aTreeNode))
				return false;

			if (raiseBeforeEvent && SelectedMenuChanging != null)
				SelectedMenuChanging(this, new MenuMngCtrlTreeViewCancelEventArgs(aTreeNode));

			MenuTreeView.SelectedNode = currSelectedMenuTreeNode = aTreeNode;

			if (currSelectedGroupLabel != null)
				currSelectedGroupLabel.CurrentMenuPath = (aTreeNode != null) ? aTreeNode.GetNamedFullPath() : String.Empty;

			RefreshCommandsViews();
			
			return true;
		}
		
		//---------------------------------------------------------------------------
		private bool SetCurrentSelectedCommandTreeNode(MenuTreeNode aTreeNode, bool notifyToNormalCommandsView)
		{
			return SetCurrentSelectedCommandTreeNode(aTreeNode, notifyToNormalCommandsView, true);
		}
		
		//---------------------------------------------------------------------------
		private bool SetCurrentSelectedCommandTreeNode(MenuTreeNode aTreeNode)
		{
			return SetCurrentSelectedCommandTreeNode(aTreeNode, true);
		}
		
		//---------------------------------------------------------------------------
		private bool SetCurrentSelectedCommandTreeNode(MenuTreeNode aTreeNode, bool notifyToEnhancedCommandsView, bool raiseBeforeEvent)
		{
			if (CommandsTreeView == null || CommandsTreeView.Nodes == null || CommandsTreeView.Nodes.Count == 0)
			{
				currSelectedCommandTreeNode = null;
				return true;
			}

			if (aTreeNode != null && (aTreeNode.Node == null || CommandsTreeView != aTreeNode.TreeView))
			{
				throw new Exception("Invalid node selection!");
			}

			if (currSelectedCommandTreeNode != null && currSelectedCommandTreeNode.Equals(aTreeNode))
				return false;
	
			if (raiseBeforeEvent && SelectedCommandChanging != null)
				SelectedCommandChanging(this, new MenuMngCtrlCancelEventArgs((aTreeNode != null) ? aTreeNode.Node : null));

			CommandsTreeView.SelectedNode = currSelectedCommandTreeNode = aTreeNode;

			if 
				(
					notifyToEnhancedCommandsView &&
					currSelectedCommandTreeNode != null && 
					currSelectedCommandTreeNode.Node != null &&
					EnhancedCommandsView != null &&
					EnhancedCommandsView.Items != null &&
					EnhancedCommandsView.Items.Count > 0
				)
			{
				CommandListBoxItem itemToSel = EnhancedCommandsView.FindItem(currSelectedCommandTreeNode.Node.GetFirstLevelAncestorCommand());
				if (itemToSel != null)
					SetCurrentSelectedCommandListBoxItem(itemToSel, false);
			}
			
			if (currSelectedGroupLabel != null)
				currSelectedGroupLabel.CurrentCommandPath = (aTreeNode != null) ? aTreeNode.GetNamedFullPath() : String.Empty;

			if (SelectedCommandChanged != null && (currSelectedCommandTreeNode == null || currSelectedCommandTreeNode.Node != null))
				SelectedCommandChanged(this, new MenuMngCtrlEventArgs((currSelectedCommandTreeNode != null) ? currSelectedCommandTreeNode.Node : null));

			
			return true;
		}
			
		//---------------------------------------------------------------------------
		private bool SetCurrentSelectedCommandListBoxItem(CommandListBoxItem aListItem, bool notifyToNormalCommandsView)
		{
			return SetCurrentSelectedCommandListBoxItem(aListItem, notifyToNormalCommandsView, true);
		}
		
		//---------------------------------------------------------------------------
		private bool SetCurrentSelectedCommandListBoxItem(CommandListBoxItem aListItem)
		{
			return SetCurrentSelectedCommandListBoxItem(aListItem, true);
		}
		
		//---------------------------------------------------------------------------
		private bool SetCurrentSelectedCommandListBoxItem(CommandListBoxItem aListItem, bool notifyToNormalCommandsView, bool raiseBeforeEvent)
		{
			if (EnhancedCommandsView == null || EnhancedCommandsView.Items == null || EnhancedCommandsView.Items.Count == 0)
			{
				currSelectedCommandListBoxItem = null;
				return true;
			}

			if (aListItem != null && (aListItem.Node == null || EnhancedCommandsView.ListBox != aListItem.ListBox))
			{
				throw new Exception("Invalid item selection!");
			}

			if (currSelectedCommandListBoxItem != null && currSelectedCommandListBoxItem.Equals(aListItem))
				return false;
	
			if (raiseBeforeEvent && SelectedCommandChanging != null)
				SelectedCommandChanging(this, new MenuMngCtrlCancelEventArgs((aListItem != null) ? aListItem.Node : null));

			EnhancedCommandsView.SelectedCommand = currSelectedCommandListBoxItem = aListItem;

			if 
				(
				notifyToNormalCommandsView &&
				currSelectedCommandListBoxItem != null && 
				currSelectedCommandListBoxItem.Node != null &&
				CommandsTreeView != null &&
				CommandsTreeView.Nodes != null &&
				CommandsTreeView.Nodes.Count > 0
				)
			{
				MenuTreeNode nodeToSel = FindCommandTreeNode(currSelectedCommandListBoxItem.Node, false);
				if (nodeToSel != null)
					SetCurrentSelectedCommandTreeNode(nodeToSel, false);
			}

			if (currSelectedGroupLabel != null)
				currSelectedGroupLabel.CurrentCommandPath = (aListItem != null && aListItem.Node != null) ? aListItem.Node.Title : String.Empty;

			if (SelectedCommandChanged != null && (currSelectedCommandListBoxItem == null || currSelectedCommandListBoxItem.Node != null))
				SelectedCommandChanged(this, new MenuMngCtrlEventArgs((currSelectedCommandListBoxItem != null) ? currSelectedCommandListBoxItem.Node : null));
			
			return true;
		}

		//---------------------------------------------------------------------------
		private void ClearMenuViews()
		{
			SetCurrentSelectedMenuTreeNode(null);

			if (CommandsTreeView != null && CommandsTreeView.Nodes != null)
				CommandsTreeView.Nodes.Clear();
			if (EnhancedCommandsView != null && EnhancedCommandsView.Items != null)
				EnhancedCommandsView.Items.Clear();
			if (MenuTreeView != null && MenuTreeView.Nodes != null)
				MenuTreeView.Nodes.Clear();
		}
		
		//---------------------------------------------------------------------------
		private void FillMenuTreeView(MenuXmlNode aParentNode, MenuTreeNodeCollection treeNodeCollToAdd )
		{
			currSelectedMenuTreeNode = null;
			
			if (treeNodeCollToAdd == null || aParentNode == null || !aParentNode.HasChildNodes)
				return;

			ArrayList menuItems = aParentNode.MenuItems;
			if (menuItems == null)
				return;

			if (treeNodeCollToAdd == MenuTreeView.Nodes)
				MenuTreeView.BeginUpdate();
			
			foreach (MenuXmlNode menuNode in menuItems)
			{
				// se un menù non contiene alcun sotto-menù e nessun comando non devo nemmeno farlo vedere:
				if (!menuNode.HasMenuChildNodes() && !menuNode.HasCommandChildNodes())
					continue;

				MenuTreeNode newMenuTreeNode = new MenuTreeNode(menuNode);

				FillMenuTreeView(menuNode, newMenuTreeNode.Nodes);

				// Se il nodo non ha figli (cioè dei sotto-menù) e non contiene nemmeno dei 
				// comandi, allora non lo devo inserire nell'albero
				if (newMenuTreeNode.NodesCount > 0  || menuNode.HasCommandChildNodes())
				{
					treeNodeCollToAdd.Add(newMenuTreeNode);

					// Dopo aver aggiunto "fisicamente" il nodo all'albero è
					// assolutamente necessario reimpostare le immagini di stato.
					// Infatti, un nodo di menù viene aggiunto all'albero se e solo
					// se possiede effettivamente dei figli e, quindi, solo dopo 
					// aver richiamato ricorsivamente su di essi questa funzione.
					// Pertanto, nella fase di aggiunta dei suoi figli, il nodo non
					// è ancora inserito nell'albero e l'immagine di stato non può
					// venire impostata
					SetTreeNodeBranchStateImages(newMenuTreeNode);
				}
			}
			
			if (treeNodeCollToAdd == MenuTreeView.Nodes)
				MenuTreeView.EndUpdate();
		}

		//---------------------------------------------------------------------------
		private void RecursiveFillCommandTreeNode(MenuTreeNode commandTreeNode)
		{
			if (commandTreeNode == null)
				return;

			MenuXmlNode commandNode = commandTreeNode.Node;

			if (commandNode == null || !commandNode.IsCommand)
				return;

			ArrayList commandItems = commandNode.CommandItems;

			if (commandItems != null)
			{
				foreach ( MenuXmlNode cmdNode  in commandItems)
				{
					MenuTreeNode newCommandTreeNode = new MenuTreeNode(cmdNode);
					
					commandTreeNode.Nodes.Add(newCommandTreeNode);
					
					// Devo aggiungere i figli solo DOPO aver aggiunto il nodo
					// all'albero, altrimenti i figli non saprebbero dal loro
					// padre qual'è il control di appartenenza, cioè la loro 
					// proprietà TreeView risulterebbe nulla
					RecursiveFillCommandTreeNode(newCommandTreeNode);

					// Adesso che sono stati caricati ricorsivamente i figli e che 
					// l'albero dei menù è riempito occorre reimpostare comunque le 
					// immagini di stato del nodo di comando e dei suoi antenati,
					// compresi appunto i nodi di menù da cui discende
					AdjustCommandNodeStateImageIndex(newCommandTreeNode);
				}
			}
		}

		//---------------------------------------------------------------------------
		private void SetTreeNodeBranchStateImages (MenuTreeNode aMenuTreeNode)
		{
			if 
				(
				aMenuTreeNode == null || 
				aMenuTreeNode.Node == null || 
				!(aMenuTreeNode.Node.IsCommand || 
				aMenuTreeNode.Node.IsMenu)
				)
				return;

			aMenuTreeNode.SetBranchStateImages();
		}

		#endregion

		#region MenuMngWinCtrl public methods

		//---------------------------------------------------------------------------
		public void ClearApplicationItems()
		{
			currentApplicationPanel = null;
			currSelectedGroupLabel = null;

			ClearMenuViews();

			SetCurrentSelectedMenuTreeNode(null);
		}
		
		//---------------------------------------------------------------------------
		public int AddImageToCmdItemsImgList(Image imageToAdd)
		{
			return (CommandsTreeView != null) ? CommandsTreeView.AddImageToImageList(imageToAdd) : -1;
		}

		//---------------------------------------------------------------------------
		public int AddImageToCmdItemsStateImgList(Image imageToAdd)
		{
			return (CommandsTreeView != null) ? CommandsTreeView.AddImageToStateImageList(imageToAdd) : -1;
		}

		//---------------------------------------------------------------------------
		public int AddImageToMenuItemsImgList(Image imageToAdd)
		{
			return (MenuTreeView != null) ? MenuTreeView.AddImageToImageList(imageToAdd) : -1;
		}

		//---------------------------------------------------------------------------
		public int AddImageToMenuItemsStateImgList(Image imageToAdd)
		{
			return (MenuTreeView != null) ? MenuTreeView.AddImageToStateImageList(imageToAdd) : -1;
		}

		//---------------------------------------------------------------------------
		public void UpdateAllApplicationPanelsPositions()
		{
			ApplicationsPanel.UpdateAllPanelsPositions();
		}

		//---------------------------------------------------------------------------
		public MenuApplicationPanel AddMenuApplicationPanel(string appPanelTitle, Image appPanelTitleImage)
		{
			return AddMenuApplicationPanel(appPanelTitle, appPanelTitleImage, true);
		}

		//---------------------------------------------------------------------------
		public MenuApplicationPanel AddMenuApplicationPanel(string appPanelTitle, Image appPanelTitleImage, bool visible)
		{
			if (ApplicationsPanel == null)
				return null;

			Cursor.Current = Cursors.WaitCursor;

			MenuApplicationPanel newAppPanel = new MenuApplicationPanel(this, null);
				
			newAppPanel.Title = appPanelTitle;
			newAppPanel.TitleImage = appPanelTitleImage;
			int newPanelIndex = (ApplicationsPanel.Panels != null) ? ApplicationsPanel.Panels.Count : 0;
			newAppPanel.Name = "ApplicationPanel" + newPanelIndex.ToString();
			newAppPanel.State = MenuApplicationPanel.PanelState.Collapsed;
			newAppPanel.TabStop = true;
			newAppPanel.TabIndex = (ApplicationsPanel.Panels != null) ? (ApplicationsPanel.Panels.Count * 2) : 0;

			newAppPanel.Visible = visible;

			AddMenuApplicationPanel(newAppPanel);
					
			// Reset the cursor to the default for all controls
			Cursor.Current = Cursors.Default;

			return newAppPanel;
		}

		//---------------------------------------------------------------------------
		public void AddMenuApplicationPanel(MenuApplicationPanel appPanel)
		{
			if (ApplicationsPanel == null || appPanel == null || ApplicationsPanel.Controls.Contains(appPanel))
				return;

			ApplicationsPanel.Controls.Add(appPanel);

            appPanel.BringToFront();
		}

		//---------------------------------------------------------------------------
		public void RemoveMenuApplicationPanel(MenuApplicationPanel appPanel)
		{
			if (ApplicationsPanel == null || appPanel == null || !ApplicationsPanel.Controls.Contains(appPanel))
				return;

			ApplicationsPanel.Controls.Remove(appPanel);
		}
		//---------------------------------------------------------------------------
		public void RefreshAppMenu()
		{
			//metto da parte la selezione corrente
			string applicationName = CurrentApplicationName;
			string groupName = CurrentGroupName;
			string menuPath = CurrentMenuPath;
			string commandPath = CurrentCommandPath;
			
			//elimino i controlli
			ApplicationsPanel.ClearControls();
			//li ricreo
			ShowApplicationMenu();
			//ripristino la selezione
			Select(applicationName, groupName, menuPath, commandPath);
		}
		//---------------------------------------------------------------------------
		public void RefreshMenuTreeView()
		{
			SetCurrentSelectedMenuTreeNode(null);
		
			if (MenuTreeView == null || CommandsTreeView == null)
				return;

			ClearMenuViews();

			SetCurrentSelectedMenuTreeNode(null);
			
			if (currSelectedGroupLabel == null || currSelectedGroupLabel.Node == null)
				return;

			FillMenuTreeView(currSelectedGroupLabel.Node, MenuTreeView.Nodes);
		
			if (MenuTreeViewFilled != null)
				MenuTreeViewFilled(this, System.EventArgs.Empty);
		}
								
		//---------------------------------------------------------------------------
		public void RefreshCommandsViews()
		{
			RefreshCommandsTreeView();
			RefreshEnhancedCommandsListBox();
		}

		//---------------------------------------------------------------------------
		public void RefreshCommandsTreeView()
		{
			if (this.CommandsTreeView == null || this.CommandsTreeView.Disposing || this.CommandsTreeView.IsDisposed)
				return;

			CommandsTreeView.BeginUpdate();
			CommandsTreeView.SuspendLayout();
			SetCurrentSelectedCommandTreeNode(null);

			
			CommandsTreeView.Nodes.Clear();

			if (currSelectedMenuTreeNode != null && currSelectedMenuTreeNode.Node != null)
			{
				MenuXmlNode menuNode = currSelectedMenuTreeNode.Node;
				ArrayList commandItems = menuNode.CommandItems;

				if (commandItems != null && commandItems.Count > 0)
				{
					foreach ( MenuXmlNode cmdNode  in commandItems)
					{
                        if (cmdNode.ItemObject.Contains(NameSolverStrings.EasyStudio) && cmdNode.Type.IsRunDocument)
                            cmdNode.Type =  new Microarea.TaskBuilderNet.Core.MenuManagerLoader.MenuXmlNode.MenuXmlNodeType("Function");
						
                        MenuTreeNode commandTreeNode = new MenuTreeNode(cmdNode);

                        CommandsTreeView.Nodes.Add(commandTreeNode);
						
						// Devo aggiungere i figli solo DOPO aver aggiunto il nodo
						// all'albero, altrimenti i figli non saprebbero dal loro
						// padre qual'è il control di appartenenza, cioè la loro 
						// proprietà TreeView risulterebbe nulla
						RecursiveFillCommandTreeNode(commandTreeNode);
					}
				}
			}
			CommandsTreeView.ResumeLayout();	
			CommandsTreeView.EndUpdate();
			
			if (CommandsTreeViewFilled != null)
				CommandsTreeViewFilled(this, System.EventArgs.Empty);
		}

		//---------------------------------------------------------------------------
		public void RefreshEnhancedCommandsListBox()
		{
			if (!EnhancedCommandsViewEnabled)
				return;

			SetCurrentSelectedCommandListBoxItem(null);

			EnhancedCommandsView.CurrentMenuNode = (currSelectedMenuTreeNode != null) ? currSelectedMenuTreeNode.Node : null;
		}
		
		//---------------------------------------------------------------------------
		public void SelectFirstAvailableCommand()
		{
			if (CommandsTreeView == null || CommandsTreeView.Nodes == null || CommandsTreeView.Nodes.Count == 0)
				return;

			SetCurrentSelectedCommandTreeNode(CommandsTreeView.Nodes[0]);
		}
		
		//---------------------------------------------------------------------------
		public bool Select(MenuParserSelection aSelection)
		{
			if
				(
					aSelection == null ||
					aSelection.ApplicationName == null || 
					aSelection.ApplicationName.Length == 0
				)
			{
				Select(String.Empty,String.Empty,String.Empty,String.Empty);
				return false;
			}

			return Select(aSelection.ApplicationName, aSelection.GroupName, aSelection.MenuPath, aSelection.CommandPath);
		}
		
		//---------------------------------------------------------------------------
		public bool Select(string aApplicationName, string aGroupName, string aMenuFullPath, string aCommandFullPath)
		{
			if (menuXmlParser == null || ApplicationsPanel.Panels == null || ApplicationsPanel.Panels.Count <= 0)
				return false;
			
			if (aApplicationName != null)
				aApplicationName.Trim();

			if (aGroupName != null)
				aGroupName.Trim();
			
			bool appSelected = false;
			bool groupSelected = false;
			bool menuSelected = false;
			bool cmdSelected = false;

			if (aApplicationName != null && aApplicationName.Length > 0)
			{
				foreach (MenuApplicationPanel appPanel in ApplicationsPanel.Panels)
				{
					if (appPanel.ApplicationNode != null && appPanel.ApplicationNode.GetNameAttribute() == aApplicationName)
					{
						appSelected = SelectApplicationPanel(appPanel);
						break;
					}
				}
			}
			if (!appSelected)
			{
				if (ApplicationsPanel.Panels != null && ApplicationsPanel.Panels.Count > 0)
				{
					for( int panelIndex = ApplicationsPanel.Panels.Count - 1; panelIndex >= 0; panelIndex--)
					{
						MenuApplicationPanel appPanel = ApplicationsPanel.Panels[panelIndex];
						if (appPanel.ApplicationNode != null && appPanel.ApplicationNode.IsApplication)
						{
							appSelected = SelectApplicationPanel(appPanel) && (aApplicationName == null || aApplicationName.Length == 0);
							break;
						}
					}
				}
			}
			
			if (appSelected)
			{
				if (aGroupName != null && aGroupName.Length > 0 && currentApplicationPanel != null && currentApplicationPanel.GroupLabels != null)
				{
					foreach (GroupLinkLabel groupLabel in currentApplicationPanel.GroupLabels)
					{
						if (groupLabel != null && groupLabel.GroupName == aGroupName)
						{
							currentApplicationPanel.CurrentGroupLabel = groupLabel;
							groupSelected = true;
							break;
						}
					}
				}
			}
			
			if (!groupSelected)
			{
				if (currentApplicationPanel != null)
				{
					ArrayList  groupLabels = currentApplicationPanel.GroupLabels;
					if (groupLabels != null && groupLabels.Count > 0)
					{
						if (aGroupName == null || aGroupName.Length == 0)
						{
							if (currentApplicationPanel.CurrentGroupLabel != null)
								currentApplicationPanel.CurrentGroupLabel = currentApplicationPanel.CurrentGroupLabel;
							else
								currentApplicationPanel.CurrentGroupLabel = ((GroupLinkLabel)groupLabels[0]);
							groupSelected = true;
						}
					}
				}
			}
			foreach (MenuApplicationPanel appPanel in ApplicationsPanel.Panels)
			{
				if (appPanel != currentApplicationPanel && appPanel.IsExpanded)
					appPanel.Collapse();
			}
			if (groupSelected && currentApplicationPanel != null && currentApplicationPanel.IsCollapsed)
			{
				currentApplicationPanel.Expand();
			}

			if (groupSelected && aMenuFullPath != null && aMenuFullPath.Length > 0)
				menuSelected = SelectMenuNodeFromFullPath(aMenuFullPath);

			if (!menuSelected && MenuTreeView != null && MenuTreeView.Nodes.Count > 0)
			{
				if (groupSelected && currSelectedGroupLabel != null)
					menuSelected = SelectMenuNodeFromFullPath(currSelectedGroupLabel.CurrentMenuPath);
				
				if (!menuSelected)
					SetCurrentSelectedMenuTreeNode(MenuTreeView.Nodes[0]);
				
				menuSelected = (MenuTreeView.SelectedNode != null && (aMenuFullPath == null || aMenuFullPath.Length == 0));
			}

			if (menuSelected && aCommandFullPath != null && aCommandFullPath.Length > 0)
				cmdSelected = SelectCommandNodeFromFullPath(aCommandFullPath);
			else
			{
				SetCurrentSelectedCommandTreeNode(null);
				if (menuSelected)
					MenuTreeView.Focus();
			}

			return appSelected && groupSelected && menuSelected && cmdSelected;
		}

		//---------------------------------------------------------------------------
		public bool SelectCommandNode(MenuXmlNode aCommandNode)
		{
			if (aCommandNode == null || !aCommandNode.IsCommand)
				return false;

			return Select
				(
				aCommandNode.GetApplicationName(), 
				aCommandNode.GetGroupName(), 
				aCommandNode.GetMenuHierarchyTitlesString(), 
				aCommandNode.GetCommandsHierarchyTitlesString()
				);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool SelectApplicationPanel(MenuApplicationPanel appPanelToSel)
		{
			if (currentApplicationPanel != null && currentApplicationPanel.Equals(appPanelToSel))
				return true;

			if (SelectedApplicationChanging != null)
			{
				MenuMngCtrlCancelEventArgs e = new MenuMngCtrlCancelEventArgs((appPanelToSel != null) ? appPanelToSel.ApplicationNode : null);
				SelectedApplicationChanging(this, e);
				if (e.Cancel)
					return false;
			}

			ClearApplicationItems();

			currentApplicationPanel = appPanelToSel;

			if (SelectedApplicationChanged != null)
				SelectedApplicationChanged(this, new MenuMngCtrlEventArgs((appPanelToSel != null) ? (MenuXmlNode)appPanelToSel.ApplicationNode : null));
			
			if (appPanelToSel != null)
			{
				SelectGroupLinkLabel(appPanelToSel.CurrentGroupLabel);

				return menuXmlParser.SetApplication(appPanelToSel.ApplicationNode);
			}

			return true;
		}

		//---------------------------------------------------------------------------
		public bool SelectGroupLinkLabel(GroupLinkLabel linkLabelToSel)
		{
			if (linkLabelToSel == null || currentApplicationPanel == null)
				return false;
			
			if (currSelectedGroupLabel != null && currSelectedGroupLabel.Equals(linkLabelToSel))
			{
				if (currSelectedMenuTreeNode != null)
					MenuTreeView.Focus();
				return true;
			}

			if (SelectedGroupChanging != null)
			{
				MenuMngCtrlCancelEventArgs e = new MenuMngCtrlCancelEventArgs(linkLabelToSel.Node);
				SelectedGroupChanging(this, e);
				if (e.Cancel)
					return false;
			}

			GroupLinkLabel groupLabelToSel = null;
			foreach (MenuApplicationPanel appPanel in ApplicationsPanel.Panels)
			{
				if (appPanel.GroupLabels == null)
					continue;

				foreach (GroupLinkLabel groupLabel in appPanel.GroupLabels)
				{
					if (groupLabel != null)
					{
						if (linkLabelToSel.Equals(groupLabel))
						{
							groupLabel.LinkVisited = true;
							groupLabelToSel = groupLabel;
						}
						else
							groupLabel.LinkVisited = false;
					}
				}
			}

			
			Debug.Assert(groupLabelToSel == null || groupLabelToSel.Node != null, "MenuMngWinCtrl.SelectGroupLinkLabel Error: invalid group label");

			string menuPathToSel = (groupLabelToSel != null) ? groupLabelToSel.CurrentMenuPath : String.Empty;
			string commandPathToSel = (groupLabelToSel != null) ? groupLabelToSel.CurrentCommandPath : String.Empty;
			
			currSelectedGroupLabel = groupLabelToSel;

			menuXmlParser.SetGroup((groupLabelToSel != null) ? groupLabelToSel.Node : null);

			if (SelectedGroupChanged != null)
				SelectedGroupChanged(this, new MenuMngCtrlEventArgs((groupLabelToSel != null) ? groupLabelToSel.Node : null));

			RefreshMenuTreeView();

			if (groupLabelToSel != null)
				groupLabelToSel.GroupVisited = true;

			if (menuPathToSel.Length > 0)
			{
				if (SelectMenuNodeFromFullPath(menuPathToSel) && commandPathToSel.Length > 0)
					SelectCommandNodeFromFullPath(commandPathToSel);
			}

			return true;
		}
		
		//---------------------------------------------------------------------------
		public bool SelectDocumentNodeFromItemObject(string aItemObject)
		{
			if (menuXmlParser == null)
				return false;

			MenuXmlNodeCollection doumentNodes = menuXmlParser.GetDocumentNodesByObjectName(aItemObject);

			if (doumentNodes == null || doumentNodes.Count == 0)
				return false;

			MenuXmlNode documentNode = (MenuXmlNode)doumentNodes[0];
			Debug.Assert(documentNode != null && documentNode.IsRunDocument, "MenuMngWinCtrl.SelectDocumentNodeFromItemObject Error: invalid document node");
			
			return SelectCommandNode(documentNode);
		}

		//---------------------------------------------------------------------------
		public bool SelectBatchNodeFromItemObject(string aItemObject)
		{
			if (menuXmlParser == null)
				return false;

			MenuXmlNodeCollection batchNodes = menuXmlParser.GetBatchNodesByObjectName(aItemObject);

			if (batchNodes == null || batchNodes.Count == 0)
				return false;

			MenuXmlNode batchNode = (MenuXmlNode)batchNodes[0];
			if (batchNode == null || !batchNode.IsRunBatch)
			{
				Debug.Fail("Error in MenuMngWinCtrl.SelectBatchNodeFromItemObject");
				return false;
			}

			return SelectCommandNode(batchNode);
		}

		//---------------------------------------------------------------------------
		public bool SelectReportNodeFromItemObject(string aItemObject)
		{
			if (menuXmlParser == null)
				return false;

			MenuXmlNodeCollection reportNodes = menuXmlParser.GetReportNodesByObjectName(aItemObject);

			if (reportNodes == null || reportNodes.Count == 0)
				return false;

			MenuXmlNode reportNode = (MenuXmlNode)reportNodes[0];
			if (reportNode == null || !reportNode.IsRunReport)
			{
				Debug.Fail("Error in MenuMngWinCtrl.SelectReportNodeFromItemObject");
				return false;
			}
			
			return SelectCommandNode(reportNode);
		}

		//---------------------------------------------------------------------------
		public bool SelectFunctionNodeFromItemObject(string aItemObject)
		{
			if (menuXmlParser == null)
				return false;

			MenuXmlNodeCollection functionNodes = menuXmlParser.GetFunctionNodesByObjectName(aItemObject);

			if (functionNodes == null || functionNodes.Count == 0)
				return false;

			MenuXmlNode functionNode = (MenuXmlNode)functionNodes[0];
			if (functionNode == null || !functionNode.IsRunFunction)
			{
				Debug.Fail("Error in MenuMngWinCtrl.SelectFunctionNodeFromItemObject");
				return false;
			}
			
			return SelectCommandNode(functionNode);
		}

		//---------------------------------------------------------------------------
		public bool SelectExeNodeFromItemObject(string aItemObject)
		{
			if (menuXmlParser == null)
				return false;

			MenuXmlNodeCollection exeNodes = menuXmlParser.GetExeNodesByObjectName(aItemObject);

			if (exeNodes == null || exeNodes.Count == 0)
				return false;

			MenuXmlNode exeNode = (MenuXmlNode)exeNodes[0];
			if (exeNode == null || !exeNode.IsRunExecutable)
			{
				Debug.Fail("Error in MenuMngWinCtrl.SelectExeNodeFromItemObject");
				return false;
			}
			
			return SelectCommandNode(exeNode);
		}

		//---------------------------------------------------------------------------
		public MenuTreeNode FindMenuNode(MenuXmlNode aMenuNode)
		{
			if (aMenuNode == null || !aMenuNode.IsMenu)
				return null;

			return MenuTreeView.FindMenuTreeNode(aMenuNode);
		}
		
		//---------------------------------------------------------------------------
		public MenuTreeNode FindCommandTreeNode(MenuXmlNode aMenuNode)
		{
			return FindCommandTreeNode(aMenuNode, true);
		}
		
		//---------------------------------------------------------------------------
		public MenuTreeNode FindCommandTreeNode(MenuXmlNode aMenuNode, bool searchDescendants)
		{
			if (aMenuNode == null || !aMenuNode.IsCommand)
				return null;

			return CommandsTreeView.FindMenuTreeNode(aMenuNode, searchDescendants);
		}
		
		//---------------------------------------------------------------------------
		public void AdjustCommandNodeStateImageIndex(MenuTreeNode aMenuTreeNode)
		{
			if (aMenuTreeNode == null || aMenuTreeNode.Node == null || !(aMenuTreeNode.Node.IsCommand || aMenuTreeNode.Node.IsMenu))
				return;

			aMenuTreeNode.AdjustStateImageIndex();
			
			// Se il nodo rappresenta un menù allora il menù padre (se esiste) si ricava direttamente dalla
			// proprietà Parent, in quanto padre e figlio appartengono allo stesso albero. In tal caso, ci 
			// pensa già il metodo AdjustStateImageIndex ad impostare l'immagine paterna.
			// Se, invece, il nodo è un comando, il menù di appartenenza va cercato in MenuTreeView. Inoltre,
			// un comando di primo livello (cioè che è figlio diretto di un menù) viene visualizzato nell'albero
			// dei comandi se e solo se il menù di appartenenza coincide con il nodo di menù correntemente
			// selezionato nell'albero dei menù
			if (aMenuTreeNode.Node.IsCommand)
			{
				MenuXmlNode parentMenuNode = aMenuTreeNode.Node.GetParentMenu();
				if (parentMenuNode != null && parentMenuNode.IsSameMenuAs(currSelectedMenuTreeNode.Node))
					currSelectedMenuTreeNode.AdjustStateImageIndex();
			}
		}

		//---------------------------------------------------------------------------
		public void AdjustSubTreeNodeStateImageIndex (MenuTreeNode aMenuTreeNode)
		{
			if (aMenuTreeNode == null || aMenuTreeNode.Node == null || !(aMenuTreeNode.Node.IsCommand || aMenuTreeNode.Node.IsMenu))
				return;

			// Se sono sul corrente nodo di menù (che sta pertanto sull'albero dei menù) devo andare ad
			// aggiornare anche l'albero dei comandi...
			if (aMenuTreeNode == currSelectedMenuTreeNode)
				RefreshCommandsViews();

			if (aMenuTreeNode.Nodes != null && aMenuTreeNode.NodesCount > 0)
			{
				foreach(MenuTreeNode aChildMenu in aMenuTreeNode.Nodes)
					AdjustSubTreeNodeStateImageIndex(aChildMenu);
			}

			aMenuTreeNode.AdjustStateImageIndex();
		}
		
		//---------------------------------------------------------------------------
		public void DisplayLoadingPanel()
		{
			LoadingPanel = new MenuLoadingPanel();
			LoadingPanel.Location = new System.Drawing.Point(Splitter1.Location.X + Splitter1.Width, 0);
			LoadingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			LoadingPanel.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top;
			LoadingPanel.Name = "LoadingPanel";
			LoadingPanel.TabIndex = Splitter1.TabIndex + 1;
			LoadingPanel.Visible = true;


			LoadingPanel.BackColor = Color.Lavender;

			if (EnhancedCommandsView != null && this.Controls.Contains(EnhancedCommandsView))
				this.Controls.Remove(EnhancedCommandsView);
			this.Controls.Remove(CommandsTreeView);
			this.Controls.Remove(Splitter2);
			this.Controls.Remove(MenuTreeView);
			// Devo rimuovere anche ApplicationsPanelBar e Splitter1, altrimenti
			// se non vengono riaggiunti nell'ordine (da destra a sinistra) i
			// control non viene reso correttamente il Dock di tipo Fill del LoadingPanel
			this.Controls.Remove(Splitter1);
			this.Controls.Remove(ApplicationsPanelBar);

			this.Controls.Add(LoadingPanel);
			this.Controls.Add(Splitter1);
			this.Controls.Add(ApplicationsPanelBar);

			EnsureVisibleLoadingPanel();
		}

		//---------------------------------------------------------------------------
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
		}

		//---------------------------------------------------------------------------
		public void EnsureVisibleLoadingPanel()
		{
			int parentFormWidth = this.ParentForm.Size.Width;
			int parentFormHeight = this.ParentForm.Size.Height;

			if (LoadingPanel.Width < LoadingPanel.StandardWidth)
			{
				this.ApplicationsPanelWidth = this.DisplayRectangle.Width - this.Splitter1.Width - LoadingPanel.StandardWidth;
			
				int adjustedWidth = this.ApplicationsPanelWidth + this.Splitter1.Width + LoadingPanel.StandardWidth;
				if (parentFormWidth < adjustedWidth)
				{
					parentFormWidth = adjustedWidth;
				}
			}
			
			if (LoadingPanel.Height < LoadingPanel.StandardHeight)
			{
				parentFormHeight += LoadingPanel.StandardHeight - LoadingPanel.Height;
			}

			if (parentFormWidth > this.ParentForm.Size.Width || parentFormHeight > this.ParentForm.Size.Height)
				this.ParentForm.Size = new System.Drawing.Size(parentFormWidth, parentFormHeight);
		}

		//---------------------------------------------------------------------------
		public void StopLoadingPanelAnimation()
		{
			if (LoadingPanel != null)
				LoadingPanel.StopAnimation();
		}

		//---------------------------------------------------------------------------
		public void DestroyLoadingPanel()
		{
			if (LoadingPanel != null)
			{
				this.Controls.Remove(LoadingPanel);
				LoadingPanel.Dispose();
				LoadingPanel = null;

				this.Controls.Remove(Splitter1);
				this.Controls.Remove(ApplicationsPanelBar);
			}
			
			if (EnhancedCommandsView != null)
				this.Controls.Add(EnhancedCommandsView);
			this.Controls.Add(CommandsTreeView);
			this.Controls.Add(Splitter2);
			this.Controls.Add(MenuTreeView);
			this.Controls.Add(Splitter1);
			this.Controls.Add(ApplicationsPanelBar);
		}

		//-------------------------------------------------------------------------------------
		public void SetEnhancedCommandsViewShowFlags(int showFlags)
		{
			if (EnhancedCommandsView == null)
				return;
			
			EnhancedCommandsView.SetCommandTypesToShow(showFlags);
		}
		
		//---------------------------------------------------------------------------
		public void EnableShowDocumentsOption(bool enableOption)
		{
			if (EnhancedCommandsView == null)
				return;

			EnhancedCommandsView.EnableShowDocumentsOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowReportsOption(bool enableOption)
		{
			if (EnhancedCommandsView == null)
				return;

			EnhancedCommandsView.EnableShowReportsOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowBatchesOption(bool enableOption)
		{
			if (EnhancedCommandsView == null)
				return;

			EnhancedCommandsView.EnableShowBatchesOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowFunctionsOption(bool enableOption)
		{
			if (EnhancedCommandsView == null)
				return;

			EnhancedCommandsView.EnableShowFunctionsOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowExecutablesOption(bool enableOption)
		{
			if (EnhancedCommandsView == null)
				return;

			EnhancedCommandsView.EnableShowExecutablesOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowTextsOption(bool enableOption)
		{
			if (EnhancedCommandsView == null)
				return;

			EnhancedCommandsView.EnableShowTextsOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowOfficeItemsOption(bool enableOption)
		{
			if (EnhancedCommandsView == null)
				return;

			EnhancedCommandsView.EnableShowOfficeItemsOption(enableOption);
		}

		#endregion

		#region MenuMngWinCtrl event handlers
		
		//---------------------------------------------------------------------------
		protected override void OnFontChanged(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnFontChanged(e);

			ApplicationsPanelBar.Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Regular);
			MenuTreeView.Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Regular);
			CommandsTreeView.Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Regular);		
		}

		//---------------------------------------------------------------------------
		private void MenuTreeView_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			if (SelectedMenuChanging != null && e.Node != null && e.Node is MenuTreeNode)
				SelectedMenuChanging(this, new MenuMngCtrlTreeViewCancelEventArgs(e));
		}

		//---------------------------------------------------------------------------
		private void MenuTreeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			SetCurrentSelectedMenuTreeNode((MenuTreeNode)e.Node, false);
			
			if (SelectedMenuChanged != null && (currSelectedMenuTreeNode == null || currSelectedMenuTreeNode.Node != null))
				SelectedMenuChanged(this, new MenuMngCtrlTreeViewEventArgs(currSelectedMenuTreeNode));
		}
		//---------------------------------------------------------------------------
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
		}
		//---------------------------------------------------------------------------
		private void MenuTreeView_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (keyboardInputEnabled && this.MenuTreeView.Focused && e.KeyCode == Keys.Enter && e.Modifiers == Keys.None)
			{
				if (currSelectedMenuTreeNode == null || currSelectedMenuTreeNode.Node == null)
					return;

				if (!currSelectedMenuTreeNode.IsExpanded)
					currSelectedMenuTreeNode.Expand();
			}
			if (ChildKeyUp != null)
				ChildKeyUp(this, e);
		}

		//---------------------------------------------------------------------------
		private void CommandsTreeView_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (keyboardInputEnabled && this.CommandsTreeView.Focused && e.KeyCode == Keys.Enter && e.Modifiers == Keys.None)
			{
				if (currSelectedCommandTreeNode == null || currSelectedCommandTreeNode.Node == null)
					return;

				if (RunCommand != null)
					RunCommand(this, new MenuMngCtrlEventArgs(currSelectedCommandTreeNode.Node));
			}
			if (ChildKeyUp != null)
				ChildKeyUp(this, e);
		}

		//---------------------------------------------------------------------------
		private void CommandsTreeView_DoubleClick(object sender, System.EventArgs e)
		{
			if (currSelectedCommandTreeNode == null || currSelectedCommandTreeNode.Node == null)
				return;

			Point ptMouse = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
			MenuTreeNode aNodeToRun = CommandsTreeView.GetMenuTreeNodeAt(CommandsTreeView.PointToClient(ptMouse));
			if (aNodeToRun == null || aNodeToRun != currSelectedCommandTreeNode)
				return;

			if (RunCommand != null)
				RunCommand(this, new MenuMngCtrlEventArgs(currSelectedCommandTreeNode.Node));
		}

		//---------------------------------------------------------------------------
		private void EnhancedCommandsView_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (keyboardInputEnabled && this.EnhancedCommandsView.Focused && e.KeyCode == Keys.Enter && e.Modifiers == Keys.None)
			{
				if (currSelectedCommandListBoxItem == null || currSelectedCommandListBoxItem.Node == null)
					return;

				if (RunCommand != null)
					RunCommand(this, new MenuMngCtrlEventArgs(currSelectedCommandListBoxItem.Node));
			}
			if (ChildKeyUp != null)
				ChildKeyUp(this, e);
		}

		//---------------------------------------------------------------------------
		private void EnhancedCommandsView_RunCommand(object sender, MenuMngCtrlEventArgs e)
		{
			if (RunCommand != null)
				RunCommand(this, e);
		}

		//---------------------------------------------------------------------------
		private void EnhancedCommandsView_ContextMenuDisplayed(object sender, MenuMngCtrlEventArgs e)
		{
			if 
				(
				!EnhancedCommandsViewEnabled ||
				sender != EnhancedCommandsView || 
				EnhancedCommandsView.CommandsContextMenu == null ||
				EnhancedCommandsView.SelectedCommand == null ||
				EnhancedCommandsView.SelectedCommand.Node == null
				)
				return;
			
			if (EnhancedCommandsViewContextMenuDisplayed != null)
				EnhancedCommandsViewContextMenuDisplayed(this, new MenuMngCtrlEventArgs(EnhancedCommandsView.SelectedCommand.Node));
		}

		//---------------------------------------------------------------------------
		private void MenuItemsContextMenu_Popup(object sender, System.EventArgs e)
		{			
			if (!(sender is ContextMenu))
				return;

			MenuXmlNode currNode = null;
			if (((ContextMenu)sender).SourceControl == MenuTreeView)
			{
				// Nel caso del TreeView la richiesta del contextmenu (clic destro del mouse) non scatena l'evento di selezione
				// del nodo e quindi non si passa per MenuTreeView_AfterSelect che imposta currSelectedMenuTreeNode.
				// Pertanto devo ricavarmi il nodo sul quale si è cliccato e selezionarlo esplicitamente per evitare spiacevoli
				// effetti collaterali
				Point ptMouse = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
				MenuTreeNode aNodeToSel = MenuTreeView.GetMenuTreeNodeAt(MenuTreeView.PointToClient(ptMouse));
				if (aNodeToSel == null)
					return;
				SetCurrentSelectedMenuTreeNode(aNodeToSel);
				if (currSelectedMenuTreeNode != null && currSelectedMenuTreeNode.Node != null)
					currNode = currSelectedMenuTreeNode.Node;
			}
			else if (((ContextMenu)sender).SourceControl == CommandsTreeView && currSelectedCommandTreeNode != null)
			{
				Point ptMouse = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
				MenuTreeNode aNodeToSel = CommandsTreeView.GetMenuTreeNodeAt(CommandsTreeView.PointToClient(ptMouse));
				if (aNodeToSel == null)
					return;
				SetCurrentSelectedCommandTreeNode(aNodeToSel);
				if (currSelectedCommandTreeNode != null && currSelectedCommandTreeNode.Node != null)
					currNode = currSelectedCommandTreeNode.Node;
			}
			else if (((ContextMenu)sender).SourceControl is GroupLinkLabel)
			{
				if (
					SelectGroupLinkLabel((GroupLinkLabel)((ContextMenu)sender).SourceControl) &&
					currSelectedGroupLabel != null 
					)
					currNode = currSelectedGroupLabel.Node;
			}
			else if (((ContextMenu)sender).SourceControl.Parent is MenuApplicationPanel)
			{
				SelectApplicationPanel((MenuApplicationPanel)((ContextMenu)sender).SourceControl.Parent);
				if (currentApplicationPanel != null	)
					currNode = currentApplicationPanel.ApplicationNode;
			}
			if (currNode == null)
				return;

			if (DisplayMenuItemsContextMenu != null)
				DisplayMenuItemsContextMenu(this, new MenuMngCtrlEventArgs(currNode));
		}

		//---------------------------------------------------------------------------
		private void CommandsTreeView_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			if (SelectedCommandChanging != null && e.Node != null && e.Node is MenuTreeNode)
				SelectedCommandChanging(this, new MenuMngCtrlCancelEventArgs(((MenuTreeNode)e.Node).Node));
		}
		
		//---------------------------------------------------------------------------
		private void CommandsTreeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			SetCurrentSelectedCommandTreeNode((MenuTreeNode)e.Node, false);
		}

		//---------------------------------------------------------------------------
		private void CommandsTreeView_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			if (BeforeCommandExpanded != null && e.Node != null)
				BeforeCommandExpanded(this, new MenuMngCtrlTreeViewEventArgs((MenuTreeNode)e.Node));
		}

		//---------------------------------------------------------------------------
		private void CommandsTreeView_BeforeCollapse(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			if (BeforeCommandCollapsed != null && e.Node != null)
				BeforeCommandCollapsed(this, new MenuMngCtrlTreeViewEventArgs((MenuTreeNode)e.Node));
		}

		//---------------------------------------------------------------------------
		private void EnhancedCommandsView_SelectedCommandChanged(object sender, System.EventArgs e)
		{
			if (EnhancedCommandsView == null)
				return;

			SetCurrentSelectedCommandListBoxItem(EnhancedCommandsView.SelectedCommand);
		}

		//---------------------------------------------------------------------------
		private void LoginPanel_CancelButton_Click(object sender, System.EventArgs e)
		{
			if (LoginCancelled != null)
				LoginCancelled(this, null);
		}

		#endregion

		//---------------------------------------------------------------------------
		public int GetMenuTreeImageIdx(MenuXmlNode aNode)
		{
			return CommandsTreeView.GetMenuTreeImageIdx(aNode, false);
		}

		//---------------------------------------------------------------------------
		private void MenuTreeView_KeyDown(object sender, KeyEventArgs e)
		{
			if (ChildKeyDown != null)
				ChildKeyDown(this, e);
		}

		//---------------------------------------------------------------------------
		private void CommandsTreeView_KeyDown(object sender, KeyEventArgs e)
		{
			if (ChildKeyDown != null)
				ChildKeyDown(this, e);
		}


		//---------------------------------------------------------------------------
		private void ApplicationsPanelBar_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (ChildKeyDown != null)
				ChildKeyDown(this, e);
		}

	}

	//============================================================================
	public class MenuMngCtrlEventArgs : EventArgs
	{
		private MenuXmlNode.MenuXmlNodeType itemType;
		private string	title;
		private string	itemObject;
		private string	arguments;
		private bool	hasOtherTitles = false;

		private MenuXmlNode.OfficeItemApplication itemOfficeApplication = MenuXmlNode.OfficeItemApplication.Undefined;
		private MenuXmlNode.MenuXmlNodeCommandSubType itemOfficeSubType = null;
		private bool checkMagicDocumentsInstallation = false;
		
		//---------------------------------------------------------------------------
		public MenuMngCtrlEventArgs()
		{
			itemType = null;
			title = String.Empty;
			itemObject = String.Empty;
			arguments = String.Empty;
			hasOtherTitles = false;
		}

		//---------------------------------------------------------------------------
		public MenuMngCtrlEventArgs(MenuXmlNode aNode)
		{
			title = (aNode != null) ? aNode.Title : String.Empty;

			if (aNode != null && aNode.IsShortcut)
			{
				itemObject = aNode.GetShortcutCommand();
				arguments = aNode.GetShortcutArguments();
				itemType = new MenuXmlNode.MenuXmlNodeType(aNode.GetShortcutTypeXmlTag());
	
				itemOfficeApplication = aNode.IsOfficeItemShortcut ? aNode.GetOfficeApplication() : MenuXmlNode.OfficeItemApplication.Undefined;
				itemOfficeSubType = aNode.IsOfficeItemShortcut ? aNode.CommandSubType : null;
				checkMagicDocumentsInstallation = aNode.IsOfficeItemShortcut ? aNode.CheckMagicDocumentsInstallation() : false;
				}
			else
			{
				itemType = (aNode != null) ? new MenuXmlNode.MenuXmlNodeType(aNode.Type) : null;
				itemObject = (aNode != null && aNode.IsCommand) ? aNode.ItemObject : String.Empty;
				arguments = (aNode != null && aNode.IsCommand) ? aNode.ArgumentsOuterXml : String.Empty;
				
				itemOfficeApplication = (aNode != null && aNode.IsOfficeItem) ? aNode.GetOfficeApplication() : MenuXmlNode.OfficeItemApplication.Undefined;
				itemOfficeSubType = (aNode != null && aNode.IsOfficeItem) ? aNode.CommandSubType : null;
				checkMagicDocumentsInstallation = (aNode != null && aNode.IsOfficeItem) ? aNode.CheckMagicDocumentsInstallation() : false;
				
				hasOtherTitles = (aNode != null) ? aNode.HasOtherTitles : false;
			}
		}

		/// <summary>
		/// Returns the item type
		/// </summary>
		//---------------------------------------------------------------------------
		public MenuXmlNode.MenuXmlNodeType ItemType
		{
			get
			{
				return itemType;
			}
		}
		
		/// <summary>
		/// Returns the title
		/// </summary>
		//---------------------------------------------------------------------------
		public string Title
		{
			get
			{
				return title;
			}
		}

		/// <summary>
		/// Returns the item object
		/// </summary>
		//---------------------------------------------------------------------------
		public string ItemObject
		{
			get
			{
				return itemObject;
			}
		}

		/// <summary>
		/// Returns the arguments
		/// </summary>
		//---------------------------------------------------------------------------
		public string Arguments
		{
			get
			{
				return arguments;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeDocument
		{
			get
			{
				if (itemOfficeSubType != null)
					return itemOfficeSubType.IsOfficeDocument;

				return false;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeTemplate
		{
			get
			{
				if (itemOfficeSubType != null)
					return itemOfficeSubType.IsOfficeTemplate;

				return false;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeDocument2007
		{
			get
			{
				if (itemOfficeSubType != null)
					return itemOfficeSubType.IsOfficeDocument2007;

				return false;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeTemplate2007
		{
			get
			{
				if (itemOfficeSubType != null)
					return itemOfficeSubType.IsOfficeTemplate2007;
				return false;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsExcelFile
		{
			get
			{
				return (itemOfficeApplication == MenuXmlNode.OfficeItemApplication.Excel);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsExcelDocument
		{
			get
			{
				return (itemOfficeApplication == MenuXmlNode.OfficeItemApplication.Excel && itemOfficeSubType.IsOfficeDocument);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsExcelTemplate
		{
			get
			{
				return (itemOfficeApplication == MenuXmlNode.OfficeItemApplication.Excel && itemOfficeSubType.IsOfficeTemplate);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordFile
		{
			get
			{
				return (itemOfficeApplication == MenuXmlNode.OfficeItemApplication.Word);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordDocument
		{
			get
			{
				return (itemOfficeApplication == MenuXmlNode.OfficeItemApplication.Word && itemOfficeSubType.IsOfficeDocument);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordTemplate
		{
			get
			{
				return (itemOfficeApplication == MenuXmlNode.OfficeItemApplication.Word && itemOfficeSubType.IsOfficeTemplate);
			}
		}
	
		//---------------------------------------------------------------------------
		public bool HasOtherTitles
		{
			get
			{
				return hasOtherTitles;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool CheckMagicDocumentsInstallation  { get { return checkMagicDocumentsInstallation; } }
		
	}

	//============================================================================
	public class MenuMngCtrlCancelEventArgs : MenuMngCtrlEventArgs
	{
		private bool cancel = false;

		//---------------------------------------------------------------------------
		public MenuMngCtrlCancelEventArgs() : base()
		{
		}

		//---------------------------------------------------------------------------
		public MenuMngCtrlCancelEventArgs(MenuXmlNode aNode) : base(aNode)
		{
		}

		//---------------------------------------------------------------------------
		public bool Cancel { get { return cancel; } set { cancel = value; } }
	}

	//============================================================================
	public class MenuMngCtrlTreeViewEventArgs : MenuMngCtrlEventArgs
	{
		private MenuTreeNode treeNode;

		//---------------------------------------------------------------------------
		public MenuMngCtrlTreeViewEventArgs()
		{
			treeNode = null;
		}

		//---------------------------------------------------------------------------
		public MenuMngCtrlTreeViewEventArgs(MenuTreeNode aTreeNode) : base ((aTreeNode != null) ? aTreeNode.Node : null)
		{
			treeNode = aTreeNode;
		}
		
		//---------------------------------------------------------------------------
		public MenuTreeNode TreeNode
		{
			get
			{
				return treeNode;
			}
		}
	}

	//============================================================================
	public class MenuMngCtrlTreeViewCancelEventArgs : MenuMngCtrlTreeViewEventArgs
	{
		private System.Windows.Forms.TreeViewCancelEventArgs cancelEventArgs = null;

		//---------------------------------------------------------------------------
		public MenuMngCtrlTreeViewCancelEventArgs(System.Windows.Forms.TreeViewCancelEventArgs e) 
			: 
			base (((MenuTreeNode)e.Node))
		{
			cancelEventArgs = e;
		}
		
		//---------------------------------------------------------------------------
		public MenuMngCtrlTreeViewCancelEventArgs(MenuTreeNode aNode, System.Windows.Forms.TreeViewAction aAction) 
			: 
			base (aNode)
		{
			cancelEventArgs = new TreeViewCancelEventArgs(aNode, false, aAction);
		}
		
		//---------------------------------------------------------------------------
		public MenuMngCtrlTreeViewCancelEventArgs(MenuTreeNode aNode) : this(aNode, TreeViewAction.Unknown)
		{
		}
		
		//---------------------------------------------------------------------------
		public TreeViewAction Action 
		{
			get { return (cancelEventArgs != null) ? cancelEventArgs.Action : TreeViewAction.Unknown;	} 
		}

		//---------------------------------------------------------------------------
		public bool	Cancel 
		{
			get { return (cancelEventArgs != null) ? cancelEventArgs.Cancel : false; } 
			set { if (cancelEventArgs != null) cancelEventArgs.Cancel = value; }
		}
	}
}
