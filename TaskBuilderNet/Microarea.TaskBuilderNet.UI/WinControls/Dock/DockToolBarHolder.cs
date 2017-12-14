using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.DockToolBar
{
	//============================================================================
	public partial class DockToolBarHolder : System.Windows.Forms.UserControl
	{
		
		#region DockToolBarHolder private members

		private DockToolBarManager				dockManager = null;
		private string							toolbarTitle = String.Empty;
		private DockStyle						dockStyle = DockStyle.Top;
		private AllowedDockStyles				allowedDockStyles = AllowedDockStyles.All;
		private Point							preferredDockedLocation = new Point(0,0);		
		private DockToolBarContainer			preferredDockedContainer = null;

		private static int mininumStringSize = 0;

		private const int floatingFormCaptionHeight = 16;
		private const int closeFloatingFormButtonWidth = 16;
		private const int closeFloatingFormButtonHeight = 16;

		#endregion

		#region DockToolBarHolder public properties
		
		//----------------------------------------------------------------------------
		public ToolBar ToolBar { get { return toolBar; } }
		//----------------------------------------------------------------------------
		public Form FloatingForm {	get { return floatingForm; } }
		//----------------------------------------------------------------------------
		public System.Drawing.Point FloatingFormLocation 
		{	
			get
			{
				return (dockStyle == DockStyle.None) ? floatingForm.Location : Point.Empty; 
			} 

			set
			{
				if (dockStyle != DockStyle.None || value == Point.Empty)
					return;

				floatingForm.Location = value; 
			} 
		}
		
		//----------------------------------------------------------------------------
		public string Title
		{
			get {  return toolbarTitle; }
			set
			{ 
				if ((toolbarTitle == null && value == null) ||
					(toolbarTitle != null && String.Compare(toolbarTitle, value) == 0))
					return;

				toolbarTitle = value;
				TitleTextChanged();
			}    
		}

		//----------------------------------------------------------------------------
		public DockStyle DockStyle 
		{
			get { return dockStyle; }
			set 
			{
				dockStyle = value;
				Create();
			}
		}

		//----------------------------------------------------------------------------
		public bool ApplyVerticalLayout
		{
			get { return dockStyle == DockStyle.Left || dockStyle == DockStyle.Right; }
		}

		//----------------------------------------------------------------------------
		public bool ApplyHorizontalLayout
		{
			get { return !ApplyVerticalLayout; }
		}

		//----------------------------------------------------------------------------
		public DockToolBarManager DockManager 
		{
			get { return dockManager; }
			set { dockManager = value; }
		}

		//----------------------------------------------------------------------------
		public AllowedDockStyles AllowedDockStyles { get {  return allowedDockStyles; } set { allowedDockStyles = value; } }
		//----------------------------------------------------------------------------
		public Point PreferredDockedLocation  { get { return preferredDockedLocation; } set { preferredDockedLocation = value; } }
		//----------------------------------------------------------------------------
		public DockToolBarContainer PreferredDockedContainer { get { return preferredDockedContainer; } set { preferredDockedContainer = value; } }

		#endregion

		#region DockToolBarHolder constructor

		//----------------------------------------------------------------------------
		public DockToolBarHolder
				(
					DockToolBarManager		aDockManager, 
					ToolBar					aToolBar, 
					DockStyle				style,
					System.Drawing.Point	aPreferredLocation
				)
		{
			if (aPreferredLocation != Point.Empty)
				preferredDockedLocation = aPreferredLocation;
			
			InitializeComponent();
			
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
			this.UpdateStyles();

			panel.Controls.Add(aToolBar);

			DockManager = aDockManager;
			
			switch (style)
			{
				case DockStyle.Left:
					preferredDockedContainer = aDockManager.LeftDockContainer;
					break;
				case DockStyle.Right:
					preferredDockedContainer = aDockManager.RightDockContainer;
					break;
				case DockStyle.Top:
					preferredDockedContainer = aDockManager.TopDockContainer;
					break;
				case DockStyle.Bottom:
					preferredDockedContainer = aDockManager.BottomDockContainer;
					break;
				default:
					break;
			}
			
			toolBar = aToolBar;
			toolBar.Wrappable = false;

			floatingForm.Visible = false;
			floatingForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			floatingForm.MaximizeBox = false;
			floatingForm.MinimizeBox = false;
			floatingForm.ShowInTaskbar = false;
			floatingForm.ClientSize = new Size(10,10);

			DockManager.MainForm.AddOwnedForm(floatingForm);
 
			DockStyle = style;
			Title = aToolBar.Text;
		}

		#endregion

		#region DockToolBarHolder private methods

		//----------------------------------------------------------------------------
		private void Create() 
		{
			AdjustSize();
		}
		
		//----------------------------------------------------------------------------
		private void TitleTextChanged() 
		{
			if (floatingForm.Visible)
				this.Invalidate(false);
		}

		//----------------------------------------------------------------------------
		private void DrawFloatingFormCaptionText(Graphics g, string captionText, Rectangle area, Brush brush) 
		{
			if(mininumStringSize == 0) 
				mininumStringSize = (int)g.MeasureString("....", this.Font).Width;

			if(area.Width < mininumStringSize) 
				return;

			StringFormat drawFormat = new StringFormat();
			drawFormat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox;
			drawFormat.Trimming = StringTrimming.EllipsisCharacter;
			
			SizeF stringExtension = g.MeasureString(captionText, this.Font);
			
			if(stringExtension.Height < area.Height) 
			{
				int offset = (int)(area.Height - stringExtension.Height)/2;
				area.Y += offset;
				area.Height -= offset;
			}
			g.DrawString(captionText, this.Font, brush, area, drawFormat);
		}

		//----------------------------------------------------------------------------
		private void DrawCloseButton(Graphics g, Rectangle crossRect, Pen pen) 
		{
			crossRect.Inflate(-2, -2);

			g.DrawLine(pen, crossRect.X, crossRect.Y, crossRect.Right, crossRect.Bottom);
			g.DrawLine(pen, crossRect.X+1, crossRect.Y, crossRect.Right, crossRect.Bottom-1);
			g.DrawLine(pen, crossRect.X, crossRect.Y+1, crossRect.Right-1, crossRect.Bottom);
			g.DrawLine(pen, crossRect.Right, crossRect.Y, crossRect.Left, crossRect.Bottom);
			g.DrawLine(pen, crossRect.Right-1, crossRect.Y, crossRect.Left, crossRect.Bottom-1);
			g.DrawLine(pen, crossRect.Right, crossRect.Y+1, crossRect.Left+1, crossRect.Bottom);
		}

		#endregion

		#region DockToolBarHolder public methods

		//----------------------------------------------------------------------------
		public bool IsDockStyleAllowed (DockStyle dock)
		{
			switch (dock)
			{
				case DockStyle.Fill:
					return false;
				case DockStyle.Top:
					return (AllowedDockStyles & AllowedDockStyles.Top) == AllowedDockStyles.Top;
				case DockStyle.Left:
					return (AllowedDockStyles & AllowedDockStyles.Left) == AllowedDockStyles.Left;
				case DockStyle.Bottom:
					return (AllowedDockStyles & AllowedDockStyles.Bottom) == AllowedDockStyles.Bottom;
				case DockStyle.Right:
					return (AllowedDockStyles & AllowedDockStyles.Right) == AllowedDockStyles.Right;
				case DockStyle.None:
					return true;
			}
			return false;
		}

		//----------------------------------------------------------------------------
		public bool CanDrag(Point p) 
		{
			if (DockStyle == DockStyle.None) 
				return (p.Y < floatingFormCaptionHeight && (dockManager.CloseFloatingToolBarEnabled ? (p.X < (Width - closeFloatingFormButtonHeight)) : (p.X < Width)));

			if (ApplyHorizontalLayout)
				return (p.X < 8 && ClientRectangle.Contains(p));
		
			return (p.Y < 8 && ClientRectangle.Contains(p));
		}

		//----------------------------------------------------------------------------
		public bool IsDocked()
		{
			return
				dockManager != null &&
				this.Parent != null &&
				this.Parent is DockToolBarContainer &&
				(
					this.Parent == dockManager.TopDockContainer ||
					this.Parent == dockManager.LeftDockContainer ||
					this.Parent == dockManager.RightDockContainer ||
					this.Parent == dockManager.BottomDockContainer
				);
		}

		//----------------------------------------------------------------------------
		public DockToolBarContainer GetDockedContainer()
		{
			return IsDocked() ? (DockToolBarContainer)this.Parent : null;
		}

		//----------------------------------------------------------------------------
		public void ShowFloatingForm()
		{
			ShowFloatingForm(Point.Empty);
		}
		
		//----------------------------------------------------------------------------
		public void ShowFloatingForm(Point formLocation)
		{
			DockToolBarContainer docked = GetDockedContainer();
			
			if (docked != null)
				docked.SuspendLayout();
				
			this.Parent = this.FloatingForm;
			this.Location = new Point(0,0);
			this.DockStyle = DockStyle.None;
			
			floatingForm.Visible = true;
		
			FloatingFormLocation = formLocation;
			
			floatingForm.Size = this.Size;
			
			if (docked != null)
			{
				docked.ResumeLayout();
				docked.PerformLayout();
			}
		}
			
		//----------------------------------------------------------------------------
		public void ShowInContainer(DockToolBarContainer aContainer, System.Drawing.Point location)
		{
			if (aContainer == null)
				return;

			aContainer.SuspendLayout();
			
			this.DockStyle = aContainer.Dock;
			this.Parent = aContainer;
			this.FloatingForm.Visible = false;
			
			preferredDockedContainer = aContainer;
			preferredDockedLocation = location;

			aContainer.ResumeLayout();				
			aContainer.PerformLayout();
		}

		//----------------------------------------------------------------------------
		public void ShowInContainer(DockToolBarContainer aContainer)
		{
			if (aContainer == null)
				return;

			ShowInContainer(aContainer, aContainer.PointToClient(Control.MousePosition));
		}

		//----------------------------------------------------------------------------
		public void ShowPreferredDockedContainer(System.Drawing.Point location)
		{
			ShowInContainer(preferredDockedContainer, location);
		}
		
		//----------------------------------------------------------------------------
		public void ShowPreferredDockedContainer()
		{
			ShowInContainer(preferredDockedContainer);
		}

		//----------------------------------------------------------------------------
		public void AdjustSize() 
		{
			if (toolBar == null)
				return;

			Size controlSize = toolBar.Size;
			
			int toolBarWidth = 0;
			int toolBarHeight = 0;
			if(ApplyHorizontalLayout)
			{
				toolBar.Wrappable = false;
				toolBar.Dock = DockStyle.Top;
				
				foreach(System.Windows.Forms.ToolBarButton button in toolBar.Buttons)
					toolBarWidth += button.Rectangle.Width;

				toolBarHeight = toolBar.ButtonSize.Height + 4;
				
			}
			else
			{
				toolBar.Wrappable = true;
				toolBar.Dock = DockStyle.Left;

				foreach(System.Windows.Forms.ToolBarButton button in toolBar.Buttons)
				{
					if(button.Style == ToolBarButtonStyle.Separator) 
						toolBarHeight += (2 * button.Rectangle.Width);
					else 
						toolBarHeight += button.Rectangle.Height; 
				}
				toolBarWidth = toolBar.ButtonSize.Width + 4;
			}

			if (toolBar.BorderStyle == BorderStyle.FixedSingle)
			{
				toolBarWidth += (2 * SystemInformation.BorderSize.Width);
				toolBarHeight += (2 * SystemInformation.BorderSize.Height);
			}
			else if (toolBar.BorderStyle == BorderStyle.Fixed3D)
			{
				toolBarWidth += (2 * SystemInformation.Border3DSize.Width);
				toolBarHeight += (2 * SystemInformation.Border3DSize.Height);
			}

			controlSize = new Size(toolBarWidth, toolBarHeight);

			this.DockPadding.All = 0;

			if (DockStyle == DockStyle.None) 
			{
				toolBar.Wrappable = false;
				this.DockPadding.Left = 2;
				this.DockPadding.Bottom = 2;
				this.DockPadding.Right = 2;
				this.DockPadding.Top = 15;

				controlSize = new Size(controlSize.Width + 4, controlSize.Height + 18);
			}
			else if (ApplyHorizontalLayout) 
			{
				this.DockPadding.Left = 8;
				controlSize = new Size(controlSize.Width + 8, controlSize.Height);
			}
			else
			{
				this.DockPadding.Top = 8;
				controlSize = new Size(controlSize.Width, controlSize.Height + 8);
			}

			this.Size = controlSize;
		}

		#endregion

		#region DockToolBarHolder protected overridden methods

		//---------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{
			if (dockStyle == DockStyle.None && floatingForm != null)
				floatingForm.Visible = this.Visible;
			
			if (preferredDockedContainer != null)
			{
				if (!this.Visible)
				{
					ArrayList containedHolders = preferredDockedContainer.Holders;
					if (containedHolders.Count > 1) // non contiene solo questo holder
					{
						// Se c'è almeno un altro holder che ha la visibilità impostata
						// differentemente dall'holder corrente, cioè che è visibile, 
						// non devo nascondere il container
						foreach (DockToolBarHolder aHolder in containedHolders)
						{
							if (aHolder != this && aHolder.Visible && aHolder.DockStyle != DockStyle.None)
							{
								preferredDockedContainer.Visible = true;
								return;
							}
						}
					}
				}
				preferredDockedContainer.Visible = this.Visible;
			}
		}

		//----------------------------------------------------------------------------
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			Pen pen = null;

			if (DockStyle == DockStyle.None) 
			{
				e.Graphics.FillRectangle(SystemBrushes.ControlDarkDark, ClientRectangle);

				DrawFloatingFormCaptionText(e.Graphics, Title, new Rectangle(0,0, this.Width - closeFloatingFormButtonWidth, 14), SystemBrushes.ControlText);
				
				if (dockManager.CloseFloatingToolBarEnabled)
				{
					Rectangle closeRect = new Rectangle(this.Width - 15, 2, 10, 10);
					pen = new Pen(SystemColors.ControlText);
					DrawCloseButton(e.Graphics, closeRect, pen);

					if(closeRect.Contains(PointToClient(MousePosition)))
						e.Graphics.DrawRectangle(pen, closeRect);
				}
				
				Rectangle clientRect = ClientRectangle;
				clientRect.Width--;
				clientRect.Height--;
				e.Graphics.DrawRectangle(new Pen(SystemColors.ControlDarkDark), clientRect);

				return;
			}

			e.Graphics.FillRectangle(SystemBrushes.Control, this.Bounds);
			
			int off = 2;
			pen = new Pen(SystemColors.ControlDark);
			if (ApplyHorizontalLayout) 
			{
				for(int i=3; i < (this.Size.Height - 3); i+=off) 
					e.Graphics.DrawLine(pen, new Point(off, i), new Point(off+off, i));
			} 
			else 
			{
				for(int i=3; i < (this.Size.Width - 3); i+=off) 
					e.Graphics.DrawLine(pen, new Point(i, off), new Point(i, off+off));
			}
		}
		
		//----------------------------------------------------------------------------
		protected override void OnMouseEnter(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnMouseEnter(e);

			if(DockStyle != DockStyle.None && CanDrag(PointToClient(MousePosition)))
				this.Cursor = Cursors.SizeAll;
			else
				this.Cursor = Cursors.Default;		
			
			this.Invalidate(false);
		}

		//----------------------------------------------------------------------------
		protected override void OnMouseLeave(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnMouseLeave(e);

			this.Cursor = Cursors.Default;
			
			this.Invalidate(false);
		}

		//----------------------------------------------------------------------------
		protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
			// Invoke base class implementation
			base.OnMouseMove(e);

			if(DockStyle != DockStyle.None && CanDrag(new Point(e.X, e.Y)))
				this.Cursor = Cursors.SizeAll;
			else
				this.Cursor = Cursors.Default;
			
			this.Invalidate(false);
		}

		//----------------------------------------------------------------------------
		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			// Invoke base class implementation
			base.OnMouseUp(e);

			if (
				e.Button == MouseButtons.Right &&
				dockManager.ShowContextMenu && 
				CanDrag(new Point(e.X, e.Y))
				) 
			{				
				DockManager.CreateContextMenu(this.PointToScreen(new Point(e.X, e.Y)));
			} 
			
			// Floating Form Close Button Clicked
			if
				(
					dockManager.CloseFloatingToolBarEnabled &&
					e.Button == MouseButtons.Left &&
					DockStyle == DockStyle.None	&& 
					e.Y < floatingFormCaptionHeight && 
					e.X > (Width - closeFloatingFormButtonHeight)
				)
				floatingForm.Visible = false;
		}

		#endregion

	}

    [Flags]
    public enum AllowedDockStyles
    {
        None = 0x0000, // Only floating
        Top = 0x0001,
        Left = 0x0002,
        Bottom = 0x0004,
        Right = 0x0008,
        All = Top | Left | Bottom | Right
    }
}
