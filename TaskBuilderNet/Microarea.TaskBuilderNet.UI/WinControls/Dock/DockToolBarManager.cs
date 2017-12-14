using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.DockToolBar
{
	//============================================================================
	public class DockToolBarManager : IMessageFilter
	{
		#region DockToolBarManager private members

		private ScrollableControl dockStation = null;

		private Form mainForm = null;

		private ArrayList holders = new ArrayList();

		private DockToolBarContainer leftDockContainer = null; 
		private DockToolBarContainer rightDockContainer = null;
		private DockToolBarContainer topDockContainer = null;
		private DockToolBarContainer bottomDockContainer = null;

		private bool showContextMenu = false;
		private bool closeFloatingToolBarEnabled = true;

		private DockToolBarHolder draggedHolder = null;
		private Point	startPoint;
		private Point	offsetPoint;

		private bool	ctrlKeyDown = false;

		private const int WM_KEYDOWN = 0x100;
		private const int WM_KEYUP = 0x101; 

		#endregion

		#region DockToolBarManager private classes

		//============================================================================
		private class HolderMenuItem : System.Windows.Forms.MenuItem
		{
			private ToolBar holderToolbar = null;
			
			public ToolBar ToolBar { get { return holderToolbar; } }

			public HolderMenuItem(ToolBar aToolBar)
			{
				holderToolbar = aToolBar;
			}
		}

		//============================================================================
		private class HolderSorter : IComparer
		{
			public int Compare(object x, object y)
			{
				Debug.Assert(x != null && x is DockToolBarHolder);
				Debug.Assert(y != null && y is DockToolBarHolder);

				DockToolBarHolder h1 = (DockToolBarHolder)x;
				DockToolBarHolder h2 = (DockToolBarHolder)y;

				return String.Compare(h1.Title, h2.Title);
			}
		}

		#endregion

		#region DockToolBarManager public properties

		//----------------------------------------------------------------------------
		public ScrollableControl DockStation { get { return dockStation; } }
		//----------------------------------------------------------------------------
		public System.Windows.Forms.Form MainForm { get { return mainForm; } }

		//----------------------------------------------------------------------------
		public bool ShowContextMenu 
		{
			get { return showContextMenu; } 
			set
			{
				if (showContextMenu == value)
					return;
				
				showContextMenu = value;

				leftDockContainer.ShowContextMenu = showContextMenu;
				rightDockContainer.ShowContextMenu = showContextMenu;
				topDockContainer.ShowContextMenu = showContextMenu;
				bottomDockContainer.ShowContextMenu = showContextMenu;
			} 
		}

		//----------------------------------------------------------------------------
		public bool CloseFloatingToolBarEnabled		
		{
			get { return closeFloatingToolBarEnabled; } 
			set
			{
				if (closeFloatingToolBarEnabled == value)
					return;
				
				closeFloatingToolBarEnabled = value;

				foreach(DockToolBarHolder holder in holders) 
				{
					if (holder.FloatingForm != null && holder.FloatingForm.Visible)
						holder.FloatingForm.Refresh();
				}
			} 
		}


		//----------------------------------------------------------------------------
		public DockToolBarContainer LeftDockContainer { get { return leftDockContainer; } }
		//----------------------------------------------------------------------------
		public DockToolBarContainer RightDockContainer { get { return rightDockContainer; } }
		//----------------------------------------------------------------------------
		public DockToolBarContainer TopDockContainer { get { return topDockContainer; } }
		//----------------------------------------------------------------------------
		public DockToolBarContainer BottomDockContainer { get { return bottomDockContainer; } }

		#endregion

		#region DockToolBarManager constructor

		//----------------------------------------------------------------------------
		public DockToolBarManager(ScrollableControl aDockStation, Form aForm)
		{
			dockStation = aDockStation;
			mainForm = aForm;

			leftDockContainer = new DockToolBarContainer(this, DockStyle.Left);
			rightDockContainer = new DockToolBarContainer(this, DockStyle.Right);
			topDockContainer = new DockToolBarContainer(this, DockStyle.Top);
			bottomDockContainer = new DockToolBarContainer(this, DockStyle.Bottom);
			
			Application.AddMessageFilter(this);
		}

		#endregion

		#region DockToolBarManager private methods

		//----------------------------------------------------------------------------
		private bool IsDocked(DockToolBarHolder holder)
		{
			return holder != null && holder.IsDocked();
		}

		//----------------------------------------------------------------------------
		private DockToolBarContainer GetDockedContainer(DockToolBarHolder holder)
		{
			return (holder != null) ? holder.GetDockedContainer() : null;
		}

		//----------------------------------------------------------------------------
		private void DockToolBarHolder_MouseDown(object sender, MouseEventArgs e)
		{
			if (sender == null || !(sender is DockToolBarHolder))
				return;

			DockToolBarHolder holder = (DockToolBarHolder)sender;

			if
				(
				draggedHolder == null &&
				e.Button.Equals(MouseButtons.Left) &&
				e.Clicks == 1 &&
				holder.CanDrag(new Point(e.X, e.Y)) )
			{
				startPoint = Control.MousePosition;
				draggedHolder = holder;
				offsetPoint = new Point(e.X, e.Y);
			}
		}

		//----------------------------------------------------------------------------
		private void DockToolBarHolder_MouseMove(object sender, MouseEventArgs e)
		{
			if(draggedHolder == null)
				return;

			Point delta = new Point(startPoint.X - Control.MousePosition.X, startPoint.Y - Control.MousePosition.Y);

			Point newLocation = draggedHolder.PointToScreen(new Point(0,0));
			newLocation = new Point(newLocation.X - delta.X, newLocation.Y - delta.Y);

			DockToolBarContainer closestContainer = GetClosestContainer(Control.MousePosition, draggedHolder.PreferredDockedContainer);

			if(closestContainer != null && !draggedHolder.IsDockStyleAllowed(closestContainer.Dock))
				closestContainer = null;

			DockToolBarContainer docked = GetDockedContainer(draggedHolder);

			if (ctrlKeyDown)
				closestContainer = null;

			if (docked != null)
			{
				if (closestContainer == null) 
				{
					draggedHolder.ShowFloatingForm(new Point(Control.MousePosition.X - offsetPoint.X, Control.MousePosition.Y - 8));
				} 
				else if (closestContainer != docked) 
				{
					draggedHolder.ShowInContainer(closestContainer);
				} 
				else 
				{
					closestContainer.SuspendLayout();
					
					newLocation = closestContainer.PointToClient(Control.MousePosition);
					draggedHolder.PreferredDockedLocation = newLocation;
					
					closestContainer.ResumeLayout();
					closestContainer.PerformLayout();
				}
			}
			else
			{
				if(closestContainer != null) 
					draggedHolder.ShowInContainer(closestContainer);
				else
					draggedHolder.FloatingForm.Location = newLocation;
			}
			
			startPoint = Control.MousePosition;
		}

		//----------------------------------------------------------------------------
		private void DockToolBarHolder_MouseUp(object sender, MouseEventArgs e)
		{
			if(draggedHolder == null)
				return;

			draggedHolder = null;
			offsetPoint.X = 8;
			offsetPoint.Y = 8;
		}

		//----------------------------------------------------------------------------
		private void DockToolBarHolder_DoubleClick(object sender, System.EventArgs e)
		{
			if (sender == null || !(sender is DockToolBarHolder))
				return;

			DockToolBarHolder holder = (DockToolBarHolder)sender;
			
			if (IsDocked(holder))
				holder.ShowFloatingForm();
			else
				holder.ShowPreferredDockedContainer();
		}

		//----------------------------------------------------------------------------
		private DockToolBarContainer GetClosestContainer(Point ptScreen, DockToolBarContainer preferred)
		{
			if (preferred != null) 
			{
				Rectangle p = preferred.RectangleToScreen(preferred.ClientRectangle);
				p.Inflate(8,8);
				if (p.Contains(ptScreen)) 
					return preferred;
			}

			Rectangle topRect = topDockContainer.RectangleToScreen(topDockContainer.ClientRectangle);
			topRect.Inflate(8,8);
			
			if (topRect.Contains(ptScreen)) 
				return topDockContainer;

			Rectangle bottomRect = bottomDockContainer.RectangleToScreen(bottomDockContainer.ClientRectangle);
			bottomRect.Inflate(8,8);

			if (bottomRect.Contains(ptScreen)) 
				return bottomDockContainer;

			Rectangle leftRect = leftDockContainer.RectangleToScreen(leftDockContainer.ClientRectangle); 
			leftRect.Inflate(8,8);
			
			if (leftRect.Contains(ptScreen)) 
				return leftDockContainer;

			Rectangle rightRect = rightDockContainer.RectangleToScreen(rightDockContainer.ClientRectangle);
			rightRect.Inflate(8,8);
			
			if (rightRect.Contains(ptScreen)) 
				return rightDockContainer;

			return null;
		}

		//----------------------------------------------------------------------------
		private void MenuClickEventHandler(object sender, EventArgs e) 
		{
			if (sender == null || !(sender is HolderMenuItem))
				return;

			HolderMenuItem item = (HolderMenuItem)sender;
			ShowToolBarHolder(item.ToolBar, !item.ToolBar.Visible);
		}
		#endregion

		#region DockToolBarManager public methods

		//----------------------------------------------------------------------------
		public DockToolBarHolder GetHolder(ToolBar aToolBar)
		{		
			if (aToolBar == null)
				return null;

			foreach(DockToolBarHolder holder in holders) 
			{
				if(holder.ToolBar == aToolBar)
					return holder;
			}

			return null;
		}

		//----------------------------------------------------------------------------
		public DockToolBarHolder GetHolder(string title)
		{		
			if (title == null)
				return null;

			foreach(DockToolBarHolder holder in holders)
			{
				if(String.Compare(title, holder.Title) == 0)
					return holder;
			}

			return null;
		}

		//----------------------------------------------------------------------------
		public ArrayList GetToolBars()
		{
			ArrayList list = new ArrayList();			

			foreach(DockToolBarHolder holder in holders) 
				list.Add(holder.ToolBar);
			
			return list;
		}

		//----------------------------------------------------------------------------
		public bool ContainsToolBar(ToolBar aToolBar)
		{
			return GetToolBars().Contains(aToolBar);
		}

		//----------------------------------------------------------------------------
		public void ShowToolBarHolder(ToolBar aToolBar, bool show) 
		{
			DockToolBarHolder holder = GetHolder(aToolBar);

			if (holder == null) 
				return;

			if (holder.Parent != null && holder.Parent is DockToolBarContainer)
				holder.Parent.Visible = show;

			if (holder.Visible == show) 
				return;

			if(IsDocked(holder))
				holder.Visible = show;
			else
				holder.FloatingForm.Visible = show;
		}

		//----------------------------------------------------------------------------
		public bool IsToolBarHolderVisible(ToolBar aToolBar) 
		{
			DockToolBarHolder holder = GetHolder(aToolBar);

			if (holder == null) 
				return false;

			if(IsDocked(holder))
				return holder.Visible;
			
			return holder.FloatingForm.Visible;
		}

		//----------------------------------------------------------------------------
		public DockStyle GetToolBarDockStyle(ToolBar aToolBar) 
		{
			if (aToolBar == null)
				return DockStyle.None;

			DockToolBarHolder holder = GetHolder(aToolBar);
			if(holder == null) 
				return DockStyle.None;

			return holder.DockStyle;
		}

		//----------------------------------------------------------------------------
		public void SetToolBarDockStyle(ToolBar aToolBar, DockStyle aDockStyle) 
		{
			if (aToolBar == null)
				return;

			DockToolBarHolder holder = GetHolder(aToolBar);
			if(holder == null || holder.DockStyle == aDockStyle) 
				return;

			RemoveToolBar(aToolBar);
			AddToolBar(aToolBar, aDockStyle);

			ShowToolBarHolder(aToolBar, true);
		}

		//----------------------------------------------------------------------------
		public void SetToolBarFloatingFormLocation(ToolBar aToolBar, System.Drawing.Point formLocation) 
		{
			if (aToolBar == null)
				return;

			DockToolBarHolder holder = GetHolder(aToolBar);
			if(holder == null || holder.DockStyle != DockStyle.None) 
				return;

			holder.FloatingFormLocation = formLocation;
		}

		//----------------------------------------------------------------------------
		public System.Drawing.Point GetToolBarFloatingFormLocation(ToolBar aToolBar) 
		{
			if (aToolBar == null)
				return Point.Empty;

			DockToolBarHolder holder = GetHolder(aToolBar);
			if(holder == null || holder.DockStyle != DockStyle.None) 
				return Point.Empty;

			return holder.FloatingFormLocation;
		}

		//----------------------------------------------------------------------------
		public DockToolBarHolder AddToolBar(ToolBar aToolBar, DockStyle	aDockStyle, System.Drawing.Point holderPreferredLocation) 
		{
			if (aDockStyle == DockStyle.Fill) 
			{
				Debug.Fail("Invalid DockStyle.");
				aDockStyle = DockStyle.Top;
			}

			DockToolBarHolder holder = new DockToolBarHolder(this, aToolBar, aDockStyle, holderPreferredLocation);
			
			holders.Add(holder);

			if (aDockStyle != DockStyle.None) 
			{
				if (aDockStyle != holder.DockStyle) 
					holder.DockStyle = aDockStyle;

				holder.Parent = holder.PreferredDockedContainer;
			} 
			else 
				holder.ShowFloatingForm();

			holder.MouseDown += new MouseEventHandler(this.DockToolBarHolder_MouseDown);
			holder.MouseMove += new MouseEventHandler(this.DockToolBarHolder_MouseMove);
			holder.MouseUp += new MouseEventHandler(this.DockToolBarHolder_MouseUp);
			holder.DoubleClick += new EventHandler(this.DockToolBarHolder_DoubleClick);

			return holder;
		}

		//----------------------------------------------------------------------------
		public DockToolBarHolder AddToolBar(ToolBar aToolBar, DockStyle	aDockStyle) 
		{
			return AddToolBar(aToolBar, aDockStyle, Point.Empty);
		}
		
		//----------------------------------------------------------------------------
		public DockToolBarHolder AddToolBar(ToolBar aToolBar, System.Drawing.Point holderPreferredLocation) 
		{
			return AddToolBar(aToolBar, DockStyle.Top, holderPreferredLocation);
		}
		
		//----------------------------------------------------------------------------
		public DockToolBarHolder AddToolBar(ToolBar aToolBar) 
		{
			return AddToolBar(aToolBar, DockStyle.Top);
		}

		//----------------------------------------------------------------------------
		public void RemoveToolBar(ToolBar aToolBar) 
		{
			DockToolBarHolder holder = GetHolder(aToolBar);
			if (holder == null)
				return;

			holder.MouseDown -= new MouseEventHandler(this.DockToolBarHolder_MouseDown);
			holder.MouseMove -= new MouseEventHandler(this.DockToolBarHolder_MouseMove);
			holder.MouseUp -= new MouseEventHandler(this.DockToolBarHolder_MouseUp);
			holder.DoubleClick -= new EventHandler(this.DockToolBarHolder_DoubleClick);
		
			holders.Remove(holder);
			holder.Parent = null;
			holder.FloatingForm.Close();
		}

		//----------------------------------------------------------------------------
		public void AdjustHolderSize(ToolBar aToolBar) 
		{
			DockToolBarHolder holder = GetHolder(aToolBar);
			if (holder != null)
				holder.AdjustSize();
		}
		
		//----------------------------------------------------------------------------
		public virtual void CreateContextMenu(Point ptScreen) 
		{
			System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
			
			ArrayList sortedHoldersList = new ArrayList();
			sortedHoldersList.AddRange(holders);
			sortedHoldersList.Sort(new HolderSorter());

			foreach(DockToolBarHolder holder in sortedHoldersList) 
			{	
				ToolBar aToolBar = holder.ToolBar;

				HolderMenuItem item = new HolderMenuItem(aToolBar);
				item.Checked = aToolBar.Visible;
				item.Text = holder.Title;
				item.Click += new EventHandler(MenuClickEventHandler);

				contextMenu.MenuItems.Add(item);
			}
			
			contextMenu.Show(dockStation, dockStation.PointToClient(ptScreen));
		}

		//----------------------------------------------------------------------------
		public bool PreFilterMessage(ref Message message) 
		{
			switch(message.Msg)
			{
				case WM_KEYDOWN:
					if ((((Keys)(int)message.WParam) & Keys.KeyCode) == Keys.ControlKey) 
					{
						if(!ctrlKeyDown && draggedHolder != null && IsDocked(draggedHolder)) 
							draggedHolder.ShowFloatingForm(new Point(Control.MousePosition.X-offsetPoint.X, Control.MousePosition.Y-8));

						ctrlKeyDown = true;
					} 
					return false;

				case WM_KEYUP:
					if ((((Keys)(int)message.WParam) & Keys.KeyCode) == Keys.ControlKey) 
					{
						if(ctrlKeyDown && draggedHolder != null && !IsDocked(draggedHolder)) 
						{
							DockToolBarContainer closestContainer = GetClosestContainer(Control.MousePosition, draggedHolder.PreferredDockedContainer);
							if(closestContainer != null)  
								draggedHolder.ShowInContainer(closestContainer);
						}
						ctrlKeyDown = false;
					}
					return false;

				default:
					break;
			}
			return false;
		}

		#endregion

	}
}
