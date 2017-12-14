using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	/// <summary>
	/// Summary description for EnhancedCommandsView.
	/// </summary>
	public partial class EnhancedCommandsView : System.Windows.Forms.UserControl
	{

		public event System.EventHandler SelectedCommandChanged;
		public event System.EventHandler ShowFlagsChanged;
		public event MenuMngCtrlEventHandler RunCommand;
		public event MenuMngCtrlEventHandler ContextMenuDisplayed;

        private System.Drawing.Point viewToolStripFloatingFormLocation = Point.Empty;

		//---------------------------------------------------------------------------
		public EnhancedCommandsView()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.BackColor = SystemColors.Control;

			this.ViewToolStrip.Text = MenuManagerWindowsControlsStrings.ViewToolBarCaptionText;

			RefreshCommandsToShowViewFlags();
		}

		//---------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{
			ShowViewToolBar = this.Visible;
		}
		
		#region EnhancedCommandsView public properties

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlParser MenuXmlParser
		{
			get { return (CommandsListBox != null) ? CommandsListBox.MenuXmlParser : null; }
			set { if (CommandsListBox != null) CommandsListBox.MenuXmlParser = value; } 
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public IPathFinder PathFinder
		{
			get { return (CommandsListBox != null) ? CommandsListBox.PathFinder : null; }
			set { if (CommandsListBox != null) CommandsListBox.PathFinder = value; } 
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlNode CurrentMenuNode
		{
			get { return (CommandsListBox != null) ? CommandsListBox.CurrentMenuNode : null; }
			set { if (CommandsListBox != null) CommandsListBox.CurrentMenuNode = value; } 
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public CommandListBoxItem SelectedCommand
		{
			get { return (CommandsListBox != null) ? CommandsListBox.SelectedCommand : null; }
			set { if (CommandsListBox != null) CommandsListBox.SelectedCommand = value; } 
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public CommandsListBox ListBox { get { return CommandsListBox; } }

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public ContextMenuStrip CommandsContextMenu { get { return (CommandsListBox != null) ? CommandsListBox.ContextMenuStrip : null; } }

		//---------------------------------------------------------------------------
		public bool ShowViewToolBar	
		{ 
			get 
			{ 
				return this.ViewToolStrip.Visible; 
			}
			set 
			{
                this.ViewToolStrip.Visible = value;
			} 
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public bool ShowCommandsDescriptions
		{
			get { return (CommandsListBox != null) ? CommandsListBox.ShowDescriptions : false; }
			set { if (CommandsListBox != null) CommandsListBox.ShowDescriptions = value; } 
		}
	
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public bool ShowReportsDates
		{
			get { return (CommandsListBox != null) ? CommandsListBox.ShowReportsDates : false; }
			set { if (CommandsListBox != null) CommandsListBox.ShowReportsDates = value; } 
		}
	
		//---------------------------------------------------------------------------
		public bool ShowStateImages	
		{ 
			get { return (CommandsListBox != null) ? CommandsListBox.ShowStateImages : false; }
			set { if (CommandsListBox != null) CommandsListBox.ShowStateImages = value; } 
		}

		//---------------------------------------------------------------------------
		public bool ParentMenuIndependent
		{ 
			get { return (CommandsListBox != null) ? CommandsListBox.ParentMenuIndependent : false; }
			set { if (CommandsListBox != null) CommandsListBox.ParentMenuIndependent = value; } 
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Color CurrentUserCommandForeColor	
		{ 
			get { return (CommandsListBox != null) ? CommandsListBox.CurrentUserCommandForeColor : CommandsListBox.DefaultCurrentUserCommandForeColor; }
			set { if (CommandsListBox != null) CommandsListBox.CurrentUserCommandForeColor = value; } 
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Color AllUsersCommandForeColor 
		{
			get { return (CommandsListBox != null) ? CommandsListBox.AllUsersCommandForeColor : CommandsListBox.DefaultAllUsersCommandForeColor; }
			set { if (CommandsListBox != null) CommandsListBox.AllUsersCommandForeColor = value; } 
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public CommandListBoxItemCollection Items
		{
			get { return (CommandsListBox != null) ? CommandsListBox.Items : null; }
		}
		
		//-------------------------------------------------------------------------------------
		public bool ShowDocuments
		{
			get
			{
                return this.ShowDocumentsToolStripButton.Checked;
			}
			set
			{
                this.ShowDocumentsToolStripButton.Checked = value;
				RefreshCommandsToShowViewFlags();
			}
		}
		
		//-------------------------------------------------------------------------------------
		public bool ShowReports
		{
			get
			{
                return this.ShowReportsToolStripButton.Checked;
			}
			set
			{
                this.ShowReportsToolStripButton.Checked = value;
				RefreshCommandsToShowViewFlags();
			}
		}
		
		//-------------------------------------------------------------------------------------
		public bool ShowBatches
		{
			get
			{
				return this.ShowBatchesToolStripButton.Checked;
			}
			set
			{
                this.ShowBatchesToolStripButton.Checked = value;
				RefreshCommandsToShowViewFlags();
			}
		}
		
		//-------------------------------------------------------------------------------------
		public bool ShowFunctions
		{
			get
			{
				return this.ShowFunctionsToolStripButton.Checked;
			}
			set
			{
                this.ShowFunctionsToolStripButton.Checked = value;
				RefreshCommandsToShowViewFlags();
			}
		}
		
		//-------------------------------------------------------------------------------------
		public bool ShowExecutables
		{
			get
			{
				return this.ShowExecutablesToolStripButton.Checked;
			}
			set
			{
                this.ShowExecutablesToolStripButton.Checked = value;
				RefreshCommandsToShowViewFlags();
			}
		}
		
		//-------------------------------------------------------------------------------------
		public bool ShowTexts
		{
			get
			{
                return this.ShowTextsToolStripButton.Checked;
			}
			set
			{
                this.ShowTextsToolStripButton.Checked = value;
				RefreshCommandsToShowViewFlags();
			}
		}
		
		//-------------------------------------------------------------------------------------
		public bool ShowOfficeItems
		{
			get
			{
                return this.ShowOfficeItemsToolStripButton.Checked;
			}
			set
			{
                this.ShowOfficeItemsToolStripButton.Checked = value;
				RefreshCommandsToShowViewFlags();
			}
		}
 
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public int ShowFlags 
		{
			get 
			{
				return (CommandsListBox != null) ? CommandsListBox.ShowFlags : -1; 
			}
		}


        //--------------------------------------------------------------------------------------------------------------------------------
        [Browsable(false)]
        public System.Drawing.Point FloatingFormLocation
        {
            get
            {
                return viewToolStripFloatingFormLocation;
            }
            set
            {
                viewToolStripFloatingFormLocation = value;

                if (this.ViewToolStrip == null && this.ViewToolStrip.Dock == DockStyle.None)
                    this.ViewToolStrip.Location = viewToolStripFloatingFormLocation;

            }
        }

		// Devo reimplementare necessariamente la proprietà Focused perchè il controllo ha
		// il fuoco se lo ha uno dei suoi figli
		//---------------------------------------------------------------------------
		[Browsable(false)]
		[Localizable(false)]
		public override bool Focused { get { return this.ContainsFocus; } }
		
		#endregion

		#region EnhancedCommandsView private methods

		//---------------------------------------------------------------------------
		private void RefreshCommandsToShowViewFlags()
		{
			if (CommandsListBox == null)
				return;

			int previousShowFlags = CommandsListBox.ShowFlags;

			CommandsListBox.SuspendRefresh();

			CommandsListBox.ShowDocuments	= this.ShowDocuments;
			CommandsListBox.ShowReports		= this.ShowReports;
			CommandsListBox.ShowBatches		= this.ShowBatches;
			CommandsListBox.ShowFunctions	= this.ShowFunctions;
			CommandsListBox.ShowExecutables	= this.ShowExecutables;
			CommandsListBox.ShowTexts		= this.ShowTexts;
			CommandsListBox.ShowOfficeItems	= this.ShowOfficeItems;

			CommandsListBox.ResumeRefresh();

			if (ShowFlagsChanged != null && previousShowFlags != CommandsListBox.ShowFlags)
				ShowFlagsChanged(this, null);
		}

		//---------------------------------------------------------------------------
		private int GetVisibleOptionsCount()
		{
            if (this.ViewToolStrip == null || this.ViewToolStrip.Items == null || this.ViewToolStrip.Items.Count == 0)
				return 0;

			int buttonsCount = 0;
            foreach (System.Windows.Forms.ToolStripItem optionButton in this.ViewToolStrip.Items)
			{
				if (optionButton != null && optionButton.Visible)
					buttonsCount++;
			}

			return buttonsCount;
		}

		//---------------------------------------------------------------------------
		private void UpdateToolBarVisibility()
		{
            if (this.ViewToolStrip != null)
                this.ViewToolStrip.Visible = (GetVisibleOptionsCount() > 1);
		}

		#endregion

		#region EnhancedCommandsView public methods

		//---------------------------------------------------------------------------
		public CommandListBoxItem GetItemAt(int x, int y)
		{
			if (CommandsListBox == null)
				return null;

			return (CommandListBoxItem)CommandsListBox.GetItemAt(x, y);
		}

		//---------------------------------------------------------------------------
		public CommandListBoxItem GetItemAt(System.Drawing.Point aPoint)
		{
			return GetItemAt(aPoint.X, aPoint.Y);
		}

		//---------------------------------------------------------------------------
		public CommandListBoxItem FindItem(MenuXmlNode aNodeToFind)
		{
			if  (CommandsListBox == null)
				return null;

			return CommandsListBox.FindItem(aNodeToFind);
		}
		
		//---------------------------------------------------------------------------
		public int AddMenuCommand(MenuXmlNode aCommandNode, object aItemTag)
		{
			if  (CommandsListBox == null)
				return -1;

			return CommandsListBox.AddMenuCommand(aCommandNode, aItemTag);
		}
		
		//---------------------------------------------------------------------------
		public int AddMenuCommand(MenuXmlNode aCommandNode)
		{
			return AddMenuCommand(aCommandNode, null);
		}

		//---------------------------------------------------------------------------
		public void SetCommandTypesToShow(int showFlags)
		{
			if (showFlags < 0)
				showFlags = (int)CommandsListBox.CommandTypesToShow.All;

			if (CommandsListBox != null)
				CommandsListBox.SuspendRefresh();

			this.ShowDocuments = ((showFlags & (int)CommandsListBox.CommandTypesToShow.Documents) == (int)CommandsListBox.CommandTypesToShow.Documents);

			this.ShowReports = ((showFlags & (int)CommandsListBox.CommandTypesToShow.Reports) == (int)CommandsListBox.CommandTypesToShow.Reports);

			this.ShowBatches = ((showFlags & (int)CommandsListBox.CommandTypesToShow.Batches) == (int)CommandsListBox.CommandTypesToShow.Batches);
			
			this.ShowFunctions = ((showFlags & (int)CommandsListBox.CommandTypesToShow.Functions) == (int)CommandsListBox.CommandTypesToShow.Functions);
			
			this.ShowExecutables = ((showFlags & (int)CommandsListBox.CommandTypesToShow.Executables) == (int)CommandsListBox.CommandTypesToShow.Executables);
			
			this.ShowTexts = ((showFlags & (int)CommandsListBox.CommandTypesToShow.Texts) == (int)CommandsListBox.CommandTypesToShow.Texts);
			
			this.ShowOfficeItems = ((showFlags & (int)CommandsListBox.CommandTypesToShow.OfficeItems) == (int)CommandsListBox.CommandTypesToShow.OfficeItems);

			if (CommandsListBox != null)
				CommandsListBox.ResumeRefresh();
		}
		
		//---------------------------------------------------------------------------
		public void EnableShowDocumentsOption(bool enableOption)
		{
			this.ShowDocumentsToolStripButton.Visible = enableOption;
            this.ShowDocumentsToolStripButton.Enabled = enableOption;

			UpdateToolBarVisibility();
		}

		//---------------------------------------------------------------------------
		public void EnableShowReportsOption(bool enableOption)
		{
            this.ShowReportsToolStripButton.Visible = enableOption;
            this.ShowReportsToolStripButton.Enabled = enableOption;

			UpdateToolBarVisibility();
		}

		//---------------------------------------------------------------------------
		public void EnableShowBatchesOption(bool enableOption)
		{
			this.ShowBatchesToolStripButton.Visible = enableOption;
            this.ShowBatchesToolStripButton.Enabled = enableOption;

			UpdateToolBarVisibility();
		}

		//---------------------------------------------------------------------------
		public void EnableShowFunctionsOption(bool enableOption)
		{
			this.ShowFunctionsToolStripButton.Visible = enableOption;
            this.ShowFunctionsToolStripButton.Enabled = enableOption;

			UpdateToolBarVisibility();
		}

		//---------------------------------------------------------------------------
		public void EnableShowExecutablesOption(bool enableOption)
		{
			this.ShowExecutablesToolStripButton.Visible = enableOption;
            this.ShowExecutablesToolStripButton.Enabled = enableOption;

			UpdateToolBarVisibility();
		}

		//---------------------------------------------------------------------------
		public void EnableShowTextsOption(bool enableOption)
		{
			this.ShowTextsToolStripButton.Visible = enableOption;
            this.ShowTextsToolStripButton.Enabled = enableOption;

			UpdateToolBarVisibility();
		}

		//---------------------------------------------------------------------------
		public void EnableShowOfficeItemsOption(bool enableOption)
		{
			this.ShowOfficeItemsToolStripButton.Visible = enableOption;
            this.ShowOfficeItemsToolStripButton.Enabled = enableOption;

			UpdateToolBarVisibility();
		}

		#endregion
		
		#region EnhancedCommandsView events handlers

		//--------------------------------------------------------------------------------------------------------------------------------
        private void ViewToolStripItem_CheckedChanged(object sender, System.EventArgs e)
        {
            RefreshCommandsToShowViewFlags();
        }
        
        //---------------------------------------------------------------------------
		private void CommandsListBox_SelectedCommandChanged(object sender, System.EventArgs e)
		{
			if (SelectedCommandChanged != null)
				SelectedCommandChanged(this, e);
		}

		//---------------------------------------------------------------------------
		private void CommandsListBox_ShowFlagsChanged(object sender, System.EventArgs e)
		{
			if (sender != CommandsListBox || CommandsListBox.IsRefreshSuspended)
				return;

			if (ShowFlagsChanged != null)
				ShowFlagsChanged(this, e);
		}

		//---------------------------------------------------------------------------
		private void CommandsListBox_DoubleClick(object sender, System.EventArgs e)
		{
			CommandListBoxItem selectedCommand = CommandsListBox.SelectedCommand;
			if (selectedCommand == null || selectedCommand.Node == null)
				return;
			
			Point ptMouse = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
			CommandListBoxItem aItemToRun = GetItemAt(CommandsListBox.PointToClient(ptMouse));
			if (aItemToRun == null || aItemToRun != selectedCommand)
				return;

			if (RunCommand != null)
				RunCommand(this, new MenuMngCtrlEventArgs(selectedCommand.Node));
		}


		//---------------------------------------------------------------------------
		private void CommandsListBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if 
				(
				(this.Parent == null || !(this.Parent is MenuMngWinCtrl) || ((MenuMngWinCtrl)this.Parent).KeyboardInputEnabled) &&
				this.CommandsListBox.Focused && 
				e.KeyCode == Keys.Enter && 
				e.Modifiers == Keys.None
				)
			{
				CommandListBoxItem selectedCommand = CommandsListBox.SelectedCommand;
				if (selectedCommand == null || selectedCommand.Node == null)
					return;

				if (RunCommand != null)
					RunCommand(this, new MenuMngCtrlEventArgs(selectedCommand.Node));
			}
		}

        //---------------------------------------------------------------------------
        private void ViewOptionsContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
			this.ShowViewToolBarToolStripMenuItem.Text = ShowViewToolBar ? MenuManagerWindowsControlsStrings.HideViewToolBarMenuItemText : MenuManagerWindowsControlsStrings.ShowViewToolBarMenuItemText;
			this.ShowCommandsDescriptionsToolStripMenuItem.Text = ShowCommandsDescriptions ? MenuManagerWindowsControlsStrings.HideCommandsDescriptionsMenuItemText : MenuManagerWindowsControlsStrings.ShowCommandsDescriptionsMenuItemText;

            this.ShowReportsDatesToolStripMenuItem.Visible = (CommandsListBox.PathFinder != null);
			this.ShowReportsDatesToolStripMenuItem.Text = ShowReportsDates ? MenuManagerWindowsControlsStrings.HideReportsDatesMenuItemText : MenuManagerWindowsControlsStrings.ShowReportsDatesMenuItemText;

			Point ptMouse = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.

			//Per mergeare il menù extended con quello normale, cancello tutto e ricreo il menù 
			//extended, e poi sulla ContextMenuDisplayed eventualmente viene "farcito"
			CommandsContextMenu.Items.Clear();
			CommandsContextMenu.Items.Add(ShowViewToolBarToolStripMenuItem);
			CommandsContextMenu.Items.Add(ShowCommandsDescriptionsToolStripMenuItem);
			CommandsContextMenu.Items.Add(ShowReportsDatesToolStripMenuItem);

			CommandListBoxItem clickedItem = GetItemAt(CommandsListBox.PointToClient(ptMouse));
			if (clickedItem != null && clickedItem.Node != null && clickedItem == CommandsListBox.SelectedCommand)
			{
				if (ContextMenuDisplayed != null)
					ContextMenuDisplayed(this, new MenuMngCtrlEventArgs(clickedItem.Node));
			}
		}

		//---------------------------------------------------------------------------
		private void ShowViewToolBarToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowViewToolBar = !ShowViewToolBar;
		}

		//---------------------------------------------------------------------------
		private void ShowCommandsDescriptionsToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowCommandsDescriptions = !ShowCommandsDescriptions;
		}
		
		//---------------------------------------------------------------------------
        private void ShowReportsDatesToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowReportsDates = !ShowReportsDates;
		}

        //---------------------------------------------------------------------------
        private void ViewToolStrip_DockChanged(object sender, EventArgs e)
        {
            if (this.ViewToolStrip == null && this.ViewToolStrip.Dock == DockStyle.None)
                this.ViewToolStrip.Location = viewToolStripFloatingFormLocation;
        }
        
        #endregion
	}
}
