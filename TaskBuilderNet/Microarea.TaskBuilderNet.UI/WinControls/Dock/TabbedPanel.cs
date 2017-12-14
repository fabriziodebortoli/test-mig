using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.UI.WinControls.Dock.Win32;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	/// <summary>
	/// Summary description for TabbedPanel.
	/// </summary>
	internal class TabbedPanel : Panel
	{
		#region TabbedPanel Enums and Structures

		//===========================================================================
		private enum HitTestArea
		{
			Splitter,
			Caption,
			TabStrip,
			DockableForm,
			None
		}

		//===========================================================================
		private class HitTestResult
		{
			public HitTestArea HitArea = HitTestArea.None;
			public int Index = -1;

			public HitTestResult(HitTestArea hitTestArea, int index)
			{
				HitArea = hitTestArea;
				Index = index;
			}

			public HitTestResult(HitTestArea hitTestArea) : this(hitTestArea, -1)
			{
			}

			public HitTestResult() : this(HitTestArea.None, -1)
			{
			}
		}

		#endregion

		#region TabbedPanel fields

		private DockableFormsContainer	container = null;
		private IContainer				components = null;
		private ToolTip					toolTip = null;
		private TabbedPanelSplitter		splitter = null;
		private System.Drawing.Font		tabsFont = SystemInformation.MenuFont;
		private System.Drawing.Color	tabsActiveTabTextColor = SystemColors.ControlText;
		private System.Drawing.Color	tabsInactiveTabTextColor = SystemColors.GrayText;
		private int						borderWidth = 0;
		private Size					frameSize;	// The size of the window including non client area. This variable
													// will be set in WM_NCCALCSIZE and used in WM_NCPAINT.
		#endregion

		#region TabbedPanel constructor and Dispose method

		//---------------------------------------------------------------------------
		public TabbedPanel(DockableFormsContainer aContainer)
		{
			container = aContainer;
			
			SetStyle
				(
				ControlStyles.ResizeRedraw |
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.DoubleBuffer |
				ControlStyles.Selectable, 
				true
				);

			if (aContainer != null)
			{
				tabsFont = aContainer.TabsFont;
				tabsActiveTabTextColor = aContainer.TabsActiveTabTextColor;
				tabsInactiveTabTextColor = aContainer.TabsInactiveTabTextColor;
			}

			components = new Container();
			toolTip = new ToolTip(components);
			
			splitter = new TabbedPanelSplitter();
			Controls.Add(splitter);
		}

		//---------------------------------------------------------------------------
		public TabbedPanel() : this(null)
		{
		}

		//---------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Components != null)
					Components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
		
		#region TabbedPanel internal properties

		//---------------------------------------------------------------------------
		internal DockableFormsContainer FormContainer { get { return container; } }

		//---------------------------------------------------------------------------
		internal DockStyle SplitterDockStyle
		{
			get
			{
				DockStyle value;
				if (IsTopLevel)
				{
					if (DockState == DockState.DockLeft || DockState == DockState.DockLeftAutoHide)
						value = DockStyle.Right;
					else if (DockState == DockState.DockRight || DockState == DockState.DockRightAutoHide)
						value = DockStyle.Left;
					else if (DockState == DockState.DockTop || DockState == DockState.DockTopAutoHide)
						value = DockStyle.Bottom;
					else if (DockState == DockState.DockBottom || DockState == DockState.DockBottomAutoHide)
						value = DockStyle.Top;
					else
						value = DockStyle.None;
				}
				else
				{
					DockableFormsContainer.LayoutStyles layoutStyle = Previous.LayoutStyle;
					if (layoutStyle == DockableFormsContainer.LayoutStyles.Horizontal)
						value = DockStyle.Left;
					else if (layoutStyle == DockableFormsContainer.LayoutStyles.Vertical)
						value = DockStyle.Top;
					else
						value = DockStyle.None;
				}

				return value;
			}
		}

		//---------------------------------------------------------------------------
		internal Point SplitterLocation
		{
			get
			{
				DockStyle splitterDockStyle = SplitterDockStyle;
			
				Point pt;
				if (splitterDockStyle == DockStyle.Left || splitterDockStyle == DockStyle.Top)
					pt = new Point(0, 0);
				if (splitterDockStyle == DockStyle.Right)
					pt = new Point(ClientRectangle.Width - DockManager.MeasureContainer.DragSize, 0);
				else if (splitterDockStyle == DockStyle.Bottom)
					pt = new Point(0, ClientRectangle.Height - DockManager.MeasureContainer.DragSize);
				else
					pt = Point.Empty;

				return pt;
			}
		}

		//---------------------------------------------------------------------------
		internal virtual bool HasCaption { get	{ return false; } set { } }

		#endregion

		#region TabbedPanel protected properties

		//---------------------------------------------------------------------------
		protected string Caption
		{
			get	{ return (FormContainer != null && FormContainer.ActiveForm != null) ? FormContainer.ActiveForm.Text : String.Empty;	}
		}

		//---------------------------------------------------------------------------
		protected IContainer Components
		{
			get	{ return components; }
		}

		//---------------------------------------------------------------------------
		protected int VisibleFormsCount
		{
			get	{ return (FormContainer != null) ? FormContainer.VisibleFormsCount : 0; }
		}

		//---------------------------------------------------------------------------
		protected Rectangle FormRectangle
		{
			get
			{
				Rectangle rectTabWindow = TabbedPanelRectangle;
				Rectangle rectCaption = CaptionRectangle;
				Rectangle rectTabStrip = GetTabStripRectangle();

				int x = rectTabWindow.X;
				int y = rectTabWindow.Y + (rectCaption.IsEmpty ? 0 : rectCaption.Height) +
					(DockState == DockState.Document ? rectTabStrip.Height : 0);
				int width = rectTabWindow.Width;
				int height = rectTabWindow.Height - rectCaption.Height - rectTabStrip.Height;

				return new Rectangle(x, y, width, height);
			}
		}

		//---------------------------------------------------------------------------
		protected Rectangle TabbedPanelRectangle
		{
			get
			{
				Rectangle rect = ClientRectangle;

				if (Contains(Next))
				{
					double sizeLayout = FormContainer.LayoutSize;

					if (LayoutStyle == DockableFormsContainer.LayoutStyles.Horizontal)
						rect.Width = (int)(ClientRectangle.Width * sizeLayout);
					else if (LayoutStyle == DockableFormsContainer.LayoutStyles.Vertical)
						rect.Height = (int)(ClientRectangle.Height * sizeLayout);
				}

				DockStyle splitterDockStyle = SplitterDockStyle;
				int splitterSize = DockManager.MeasureContainer.DragSize;
				if (splitterDockStyle == DockStyle.Left)
					rect.X += splitterSize;
				else if (splitterDockStyle == DockStyle.Top)
					rect.Y += splitterSize;
				
				if (splitterDockStyle == DockStyle.Right || splitterDockStyle == DockStyle.Left)
					rect.Width -= splitterSize;
				else if (splitterDockStyle == DockStyle.Bottom || splitterDockStyle == DockStyle.Top)
					rect.Height -= splitterSize;

				return rect;
			}
		}

		//---------------------------------------------------------------------------
		protected virtual Rectangle CaptionRectangle { get { return Rectangle.Empty; } }

		#endregion

		#region TabbedPanel private properties

		//---------------------------------------------------------------------------
		private DockableFormsCollection Forms { get { return FormContainer.Forms; } }

		//---------------------------------------------------------------------------
		private Rectangle NextTabWindowBounds
		{
			get
			{
				if (!Contains(Next))
					return Rectangle.Empty;

				Rectangle rectClient = ClientRectangle;
				Rectangle rectTabWindow = TabbedPanelRectangle;
				Rectangle rect = Rectangle.Empty;

				if (IsTopLevel && LayoutStyle == DockableFormsContainer.LayoutStyles.Horizontal &&
					(SplitterDockStyle == DockStyle.Left || SplitterDockStyle == DockStyle.Right))
					rectClient.Width -= DockManager.MeasureContainer.DragSize;

				if (IsTopLevel && LayoutStyle == DockableFormsContainer.LayoutStyles.Vertical &&
					(SplitterDockStyle == DockStyle.Top || SplitterDockStyle == DockStyle.Bottom))
					rectClient.Height -= DockManager.MeasureContainer.DragSize;

				if (LayoutStyle == DockableFormsContainer.LayoutStyles.Horizontal)
				{
					rect.X = rectTabWindow.X + rectTabWindow.Width;
					rect.Y = rectTabWindow.Y;
					rect.Width = rectClient.Width - rectTabWindow.Width;
					rect.Height = rectTabWindow.Height;
				}
				else
				{
					rect.X = rectTabWindow.X;
					rect.Y = rectTabWindow.Y + rectTabWindow.Height;
					rect.Width = rectTabWindow.Width;
					rect.Height = rectClient.Height - rectTabWindow.Height;
				}

				return rect;
			}
		}

		#endregion
		
		#region TabbedPanel public properties

		//---------------------------------------------------------------------------
		public System.Drawing.Font TabsFont { get { return tabsFont; } set { tabsFont = value; } }
		
		//---------------------------------------------------------------------------
		public System.Drawing.Color TabsActiveTabTextColor { get { return tabsActiveTabTextColor; } set { tabsActiveTabTextColor = value; } }
		
		//---------------------------------------------------------------------------
		public System.Drawing.Color TabsInactiveTabTextColor { get { return tabsInactiveTabTextColor; } set { tabsInactiveTabTextColor = value; } }
		
		//---------------------------------------------------------------------------
		public int BorderWidth
		{
			get	{ return borderWidth; }
			set
			{
				if (borderWidth == value)
					return;

				borderWidth = value;
				User32.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
					Win32.FlagsSetWindowPos.SWP_NOMOVE |
					Win32.FlagsSetWindowPos.SWP_NOSIZE |
					Win32.FlagsSetWindowPos.SWP_NOZORDER |
					Win32.FlagsSetWindowPos.SWP_DRAWFRAME);
			}
		}

		//---------------------------------------------------------------------------
		public DockableForm ActiveForm
		{
			get	{ return (FormContainer != null) ? FormContainer.ActiveForm : null; }
			set	{ FormContainer.ActiveForm = value;	}
		}

		//---------------------------------------------------------------------------
		public DockManager DockManager { get { return (FormContainer != null) ? FormContainer.DockManager : null; } }

		//---------------------------------------------------------------------------
		public DockState DockState { get { return (FormContainer != null) ? FormContainer.DockState : DockState.Unknown; } }

		//---------------------------------------------------------------------------
		public FloatingForm FloatingForm { get { return (FormContainer != null) ? FormContainer.FloatingForm : null; } }

		//---------------------------------------------------------------------------
		public bool IsActivated { get { return (FormContainer != null) ? FormContainer.IsActivated : false; } }

		//---------------------------------------------------------------------------
		public TabbedPanel First
		{
			get
			{	
				if (FormContainer == null)
					return null;
				DockableFormsContainer firstForm = FormContainer.First;
				return (firstForm != null) ? firstForm.TabbedPanel : null;
			}			
		}

		//---------------------------------------------------------------------------
		public TabbedPanel Last 
		{
			get	
			{
				if (FormContainer == null)
					return null;
				DockableFormsContainer lastForm = FormContainer.Last;
				return (lastForm != null) ? lastForm.TabbedPanel : null;
			}
		}

		//---------------------------------------------------------------------------
		public TabbedPanel Next
		{
			get
			{
				if (FormContainer == null)
					return null;
				DockableFormsContainer nextForm = FormContainer.Next;
				return (nextForm != null) ? nextForm.TabbedPanel : null;
			}
		}

		//---------------------------------------------------------------------------
		public TabbedPanel Previous
		{
			get
			{
				if (FormContainer == null)
					return null;
				DockableFormsContainer previousForm = FormContainer.Previous;
				return (previousForm != null) ? previousForm.TabbedPanel : null;
			}
		}

		//---------------------------------------------------------------------------
		public double LayoutSize
		{
			get
			{	
				if (FormContainer == null)
					return 0.0;
				
				if (DockState == DockState.Float)
					return FormContainer.FloatingSize;
				else if (DockState == DockState.Document)
					return FormContainer.DocumentSize;
				else
					return FormContainer.DockSize;
			}
			set
			{
				if (FormContainer == null)
					return;

				if (DockState == DockState.Float)
					FormContainer.FloatingSize = value;
				else if (DockState == DockState.Document)
					FormContainer.DocumentSize = value;
				else
					FormContainer.DockSize = value;
			}
		}

		//---------------------------------------------------------------------------
		public DockableFormsContainer.LayoutStyles LayoutStyle
		{
			get
			{	
				if (FormContainer == null)
					return DockableFormsContainer.LayoutStyles.None;

				if (DockState == DockState.Float)
					return FormContainer.FloatingLayoutStyle;
				else if (DockState == DockState.Document)
					return FormContainer.DocumentLayoutStyle;
				else
					return FormContainer.DockLayoutStyle;
			}
			set
			{
				if (FormContainer == null)
					return;

				if (DockState == DockState.Float)
					FormContainer.FloatingLayoutStyle = value;
				else if (DockState == DockState.Document)
					FormContainer.DocumentLayoutStyle = value;
				else
					FormContainer.DockLayoutStyle = value;
			}
		}
		
		//---------------------------------------------------------------------------
		public Rectangle TabStripRectangle { get { return GetTabStripRectangle(); } }


		#endregion

		#region TabbedPanel internal methods

		//---------------------------------------------------------------------------
		public void InsertContainerAfter(DockableFormsContainer aContainer, DockableFormsContainer.LayoutStyles layoutStyle, double layoutSize)
		{
			if (DockState == DockState.Float)
			{
				aContainer.FloatingForm = FloatingForm;
				int index = FloatingForm.Containers.IndexOf(aContainer);
				if (index == FloatingForm.Containers.Count - 1)
					FloatingForm.SetContainerIndex(aContainer, -1);
				else
					FloatingForm.SetContainerIndex(aContainer, index + 1);
			}
			else
			{
				int index = DockManager.Containers.IndexOf(aContainer);
				if (index == DockManager.Containers.Count - 1)
					DockManager.SetContainerIndex(aContainer, -1);
				else
					DockManager.SetContainerIndex(aContainer, index + 1);
			}
			aContainer.VisibleState = DockState;
			
			aContainer.TabbedPanel.LayoutStyle = LayoutStyle;
			if (aContainer.Next != null)
			{
				if (layoutStyle == aContainer.TabbedPanel.LayoutStyle &&  layoutStyle == aContainer.Next.TabbedPanel.LayoutStyle)
					aContainer.TabbedPanel.LayoutSize = (1 - LayoutSize) * layoutSize;
				else
					aContainer.TabbedPanel.LayoutSize = LayoutSize;
			}

			LayoutStyle = layoutStyle;
			if (aContainer.Next != null)
			{
				if (LayoutStyle == aContainer.TabbedPanel.LayoutStyle && LayoutStyle == aContainer.Next.TabbedPanel.LayoutStyle)
					LayoutSize *= (1 - layoutSize);
				else
					LayoutSize = 1 - layoutSize;
			}
			else
				LayoutSize = 1 - layoutSize;

			FormContainer.Refresh();
			
			aContainer.Refresh();
		}

		//---------------------------------------------------------------------------
		internal void InsertContainerBefore(DockableFormsContainer aContainer, DockableFormsContainer.LayoutStyles layoutStyle, double layoutSize)
		{
			if (DockState == DockState.Float)
			{
				aContainer.FloatingForm = FloatingForm;
				int index = FloatingForm.Containers.IndexOf(aContainer);
				FloatingForm.SetContainerIndex(aContainer, index);
			}
			else
			{
				int index = DockManager.Containers.IndexOf(aContainer);
				DockManager.SetContainerIndex(aContainer, index);
			}
			aContainer.VisibleState = DockState;

			aContainer.TabbedPanel.LayoutStyle = layoutStyle;
			if (Next != null && layoutStyle == LayoutStyle)
			{
				aContainer.TabbedPanel.LayoutSize = (1 - LayoutSize) * layoutSize;
				LayoutSize *= (1 - layoutSize);
			}
			else
				aContainer.TabbedPanel.LayoutSize = layoutSize;

			aContainer.Refresh();
			FormContainer.Refresh();
		}

		//---------------------------------------------------------------------------
		internal void SetParent(Control value)
		{
			if (Parent == value)
				return;

			Control oldParent = Parent;

			///!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			/// Workaround for .Net Framework bug: removing control from Form may cause form
			/// unclosable. Set focus to another dummy control.
			///!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			Form form = FindForm();
			if (ContainsFocus)
			{
				if (form is FloatingForm)
				{
					((FloatingForm)form).DummyControl.Focus();
					form.ActiveControl = ((FloatingForm)form).DummyControl;
				}
				else if (DockManager != null)
				{
					DockManager.DummyControl.Focus();
					if (form != null)
						form.ActiveControl = DockManager.DummyControl;
				}
			}
			//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

			Parent = value;

			if (oldParent != null)
				if (oldParent is TabbedPanel)
					oldParent.Invalidate();
			if (value != null)
				if (value is TabbedPanel)
					value.Invalidate();
		}

		//---------------------------------------------------------------------------
		internal virtual void TestDrop(DockManager.DragHandler dragHandler, Point pt)
		{
			if (DockState == DockState.Document)
				DockManager.TestDrop(dragHandler, pt);

			if (dragHandler.DropTarget.DropToControl != null)
				return;

			if (DockManager.IsDockStateAutoHide(DockState))
				return;

			if (!dragHandler.IsDockStateValid(DockState))
				return;

			if (dragHandler.DragSourceType == DockManager.DragHandler.SourceType.FloatingForm &&
				FormContainer.FloatingForm == dragHandler.DragControl)
				return;

			if (dragHandler.DragSourceType == DockManager.DragHandler.SourceType.DockableFormsContainer &&
				dragHandler.DragControl == this)
				return;

			if (dragHandler.DragSourceType == DockManager.DragHandler.SourceType.DockableForm && 
				dragHandler.DragControl == this &&
				DockState == DockState.Document &&
				VisibleFormsCount == 1)
				return;

			Point ptClient = PointToClient(pt);
			Rectangle rectTabWindow = TabbedPanelRectangle;
			
			int dragSize = DockManager.MeasureContainer.DragSize;

			if (container.DockState != DockState.Document && ptClient.Y - rectTabWindow.Top >= 0 && ptClient.Y - rectTabWindow.Top < dragSize)
				dragHandler.DropTarget.SetDropTarget(this, DockStyle.Top);
			else if (container.DockState != DockState.Document && rectTabWindow.Bottom - ptClient.Y >= 0 && rectTabWindow.Bottom - ptClient.Y < dragSize)
				dragHandler.DropTarget.SetDropTarget(this, DockStyle.Bottom);
			else if (container.DockState != DockState.Document && rectTabWindow.Right - ptClient.X >= 0 && rectTabWindow.Right - ptClient.X < dragSize)
				dragHandler.DropTarget.SetDropTarget(this, DockStyle.Right);
			else if (container.DockState != DockState.Document && ptClient.X - rectTabWindow.Left >= 0 && ptClient.X - rectTabWindow.Left < dragSize)
				dragHandler.DropTarget.SetDropTarget(this, DockStyle.Left);
			else
			{
				if (rectTabWindow.Height <= GetTabStripRectangle().Height)
					return;

				HitTestResult hitTestResult = GetHitTest(pt);
				
				if (hitTestResult.HitArea == HitTestArea.Caption)
					dragHandler.DropTarget.SetDropTarget(this, DockStyle.Fill, -1);
				else if (hitTestResult.HitArea == HitTestArea.TabStrip && hitTestResult.Index != -1)
					dragHandler.DropTarget.SetDropTarget(this, DockStyle.Fill, hitTestResult.Index);
				else if (DockState == DockState.Float && !HasCaption &&
					((ptClient.Y - rectTabWindow.Top >= dragSize && ptClient.Y - rectTabWindow.Top < 2 * dragSize) ||
					(rectTabWindow.Bottom - ptClient.Y >= dragSize && rectTabWindow.Bottom - ptClient.Y < 2 * dragSize) ||
					(rectTabWindow.Right - ptClient.X >= dragSize && rectTabWindow.Right - ptClient.X < 2 * dragSize) ||
					(ptClient.X - rectTabWindow.Left >= dragSize && ptClient.X - rectTabWindow.Left < 2 * dragSize)))
					dragHandler.DropTarget.SetDropTarget(this, DockStyle.Fill, -1);
				else
					return;
			}

			if (dragHandler.DropTarget.SameAsOldValue)
				return;

			dragHandler.DragFrame = GetTestDropDragFrame(dragHandler.DropTarget.DockStyle, dragHandler.DropTarget.ContainerIndex);
		}

		#endregion
		
		#region TabbedPanel protected overridden methods
		
		//---------------------------------------------------------------------------
		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);
			
			if (FormContainer != null) 
				FormContainer.IsActivated = false;
		}

		//---------------------------------------------------------------------------
		protected override void OnLayout(LayoutEventArgs e)
		{
			if (FormContainer == null || FormContainer.TabbedPanel != this)
				return;

			splitter.Dock = SplitterDockStyle;

			if (Contains(Next))
			{
				Next.Dock = DockStyle.None;
				Next.Bounds = NextTabWindowBounds;
			}

			Rectangle formBounds = FormRectangle;

			foreach (DockableForm aForm in Forms)
			{
				aForm.Parent = this;
				aForm.Visible = (aForm == ActiveForm);
				aForm.Bounds = formBounds;
			}
			base.OnLayout(e);
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Left)
			{
				HitTestResult hitTestResult = GetHitTest();
				if (hitTestResult.HitArea == HitTestArea.TabStrip && hitTestResult.Index != -1)
				{
					DockableForm aForm = GetVisibleForm(hitTestResult.Index);
					if (ActiveForm != aForm)
					{
						ActiveForm = aForm;
						this.Update();
					}
					if (DockManager.IsRedockingAllowed && FormContainer.IsRedockingAllowed && ActiveForm.IsRedockingAllowed)
						DockManager.BeginDragForm(this, TabbedPanelRectangle);
				}
				else if (hitTestResult.HitArea == HitTestArea.Caption && FormContainer.TabbedPanel == this &&
					DockManager.IsRedockingAllowed && FormContainer.IsRedockingAllowed &&
					(DockState == DockState.DockLeft || DockState == DockState.DockRight ||
					DockState == DockState.DockTop || DockState == DockState.DockBottom ||
					DockState == DockState.Float))
					DockManager.BeginDragContainer(this, CaptionRectangle.Location);
			}
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseMove(MouseEventArgs e)
		{
			HitTestResult hitTestResult = GetHitTest();
			HitTestArea hitArea = hitTestResult.HitArea;
			
			int index = hitTestResult.Index;
			
			string toolTipText = String.Empty;

			base.OnMouseMove(e);

			if (hitArea == HitTestArea.TabStrip && index != -1)
			{
				Rectangle rectTab = GetTabRectangle(index);
				if (rectTab.Width < GetTabOriginalWidth(index))
					toolTipText = GetVisibleForm(index).Text;
			}

			if (toolTip.GetToolTip(this) != toolTipText)
			{
				toolTip.Active = false;
				toolTip.SetToolTip(this, toolTipText);
				toolTip.Active = true;
			}
		}

		//---------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			
			if (e == null || e.Graphics == null || !this.Visible || this.VisibleFormsCount == 0)
				return;

			SolidBrush bkgndBrush = new SolidBrush(this.BackColor);
			e.Graphics.FillRectangle(bkgndBrush, this.ClientRectangle);
			bkgndBrush.Dispose();
			
			DrawCaption(e.Graphics);
			DrawTabStrip(e.Graphics);
			
			PerformLayout();
		}

		//---------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == (int)Win32.Msgs.WM_NCCALCSIZE)
			{
				Win32.RECT rect = (Win32.RECT)m.GetLParam(typeof(Win32.RECT));
				frameSize = new Size(rect.right-rect.left, rect.bottom-rect.top);
				base.WndProc(ref m);
				rect.left += BorderWidth;
				rect.right -= BorderWidth;
				rect.top += BorderWidth;
				rect.bottom -= BorderWidth;
				Marshal.StructureToPtr(rect, m.LParam, true);
				return;
			}
			else if (m.Msg == (int)Win32.Msgs.WM_NCPAINT)
			{
				if (BorderWidth <= 0)
					return;

				IntPtr hDC = User32.GetWindowDC(m.HWnd);
				if (hDC != IntPtr.Zero)
				{
					Graphics g = Graphics.FromHdc(hDC);
					if (g != null)
					{
						Rectangle rectFrame = new Rectangle(0, 0, frameSize.Width, frameSize.Height);
			
						Region regionBorder = new Region(rectFrame);

						regionBorder.Xor(Rectangle.Inflate(rectFrame, -BorderWidth, -BorderWidth));
						g.FillRegion(SystemBrushes.ControlDark, regionBorder);
					
						regionBorder.Dispose();

						User32.ReleaseDC(m.HWnd, hDC);
						return;
					}
				}
			}
			else if (m.Msg == (int)Win32.Msgs.WM_LBUTTONDBLCLK)
			{
				base.WndProc(ref m);

				HitTestResult hitTestResult = GetHitTest();
				if (hitTestResult.HitArea != HitTestArea.Caption)
					return;

				if (DockManager.IsDockStateAutoHide(DockState))
				{
					DockManager.ActiveAutoHideForm = null;
					return;
				}

				if 
					(
					DockManager.IsDockStateDocked(DockState) && 
					FormContainer.IsDockStateValid(DockState.Float)
					)
					FormContainer.VisibleState = DockState.Float;
				else if 
					(
					DockState == DockState.Float &&
					FormContainer.RestoreState != DockState.Unknown &&
					FormContainer.IsDockStateValid(FormContainer.RestoreState)
					)
					FormContainer.VisibleState = FormContainer.RestoreState;

				return;
			}

			base.WndProc(ref m);
		}

		#endregion

		#region TabbedPanel protected methods
		
		//---------------------------------------------------------------------------
		protected int GetVisibleFormIndex(DockableForm c)
		{
			return (FormContainer != null) ? FormContainer.GetVisibleFormIndex(c) : -1;
		}

		//---------------------------------------------------------------------------
		protected DockableForm GetVisibleForm(int index)
		{
			return (FormContainer != null) ? FormContainer.GetVisibleForm(index) : null;
		}


		#region TabbedPanel protected virtual methods
		
		//---------------------------------------------------------------------------
		protected virtual void Close() { Dispose(); }

		//---------------------------------------------------------------------------
		protected virtual int GetTabOriginalWidth(int index) { return 0; }

		//---------------------------------------------------------------------------
		protected virtual Rectangle GetTabRectangle(int index) { return Rectangle.Empty; }

		//---------------------------------------------------------------------------
		protected virtual void DrawCaption(Graphics g) { }

		//---------------------------------------------------------------------------
		protected virtual void DrawTabStrip(Graphics g) { }

		//---------------------------------------------------------------------------
		protected virtual Rectangle GetTabStripRectangle() { return Rectangle.Empty; }

		//---------------------------------------------------------------------------
		protected virtual Rectangle GetTabStripRectangle(bool tabOnly) { return Rectangle.Empty; }

		//---------------------------------------------------------------------------
		protected virtual Region GetTestDropDragFrame(DockStyle dockStyle, int containerIndex){ return null; }

		#endregion

		#endregion

		#region TabbedPanel private methods
		
		//---------------------------------------------------------------------------
		private HitTestResult GetHitTest()
		{
			return GetHitTest(Control.MousePosition);
		}

		//---------------------------------------------------------------------------
		private HitTestResult GetHitTest(Point ptMouse)
		{
			ptMouse = PointToClient(ptMouse);

			if (splitter.Bounds.Contains(ptMouse))
				return new HitTestResult(HitTestArea.Splitter, -1);

			Rectangle captionRect = CaptionRectangle;
			if (captionRect.Contains(ptMouse))
				return new HitTestResult(HitTestArea.Caption, -1);

			Rectangle formRect = FormRectangle;
			if (formRect.Contains(ptMouse))
				return new HitTestResult(HitTestArea.DockableForm, -1);

			Rectangle rectTabStrip = GetTabStripRectangle(true);
			if (rectTabStrip.Contains(ptMouse))
			{
				for (int i=0; i < VisibleFormsCount; i++)
				{
					Rectangle rectTab = GetTabRectangle(i);
					rectTab.Intersect(rectTabStrip);
					if (rectTab.Contains(ptMouse))
						return new HitTestResult(HitTestArea.TabStrip, i);
				}
				return new HitTestResult(HitTestArea.TabStrip);
			}

			return new HitTestResult();
		}

		#endregion

		#region TabbedPanel public methods

		//---------------------------------------------------------------------------
		public bool Contains(TabbedPanel tabWindow)
		{
			return (tabWindow == null ? false : FormContainer.Contains(tabWindow.FormContainer));
		}

		//---------------------------------------------------------------------------
		public bool IsTopLevel
		{
			get
			{
				if (Previous != null)
					if (Previous.Contains(this))
						return false;

				return true;
			}
		}

		//---------------------------------------------------------------------------
		public void BeginDrag()
		{
			DockManager.BeginDragTabbedPanelSplitter(this, SplitterLocation);
		}

		#region TabbedPanel public virtual methods
		
		//---------------------------------------------------------------------------
		public virtual void EnsureTabVisible(DockableForm container){}

		#endregion

		#endregion
	}
}
