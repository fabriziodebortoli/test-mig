using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Security;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.UI.WinControls.Dock.Win32;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	//===========================================================================
	[Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
	public class DockManager : Panel
	{
		internal class MeasureContainer
		{
			public const int DragSize = 4;
			public const int MinSize = 24;
		}
		
		internal class MeasureAutoHideTab
		{
			public const int ImageHeight = 16;
			public const int ImageWidth = 16;
			public const int ImageGapTop = 2;
			public const int ImageGapLeft = 2;
			public const int ImageGapRight = 2;
			public const int ImageGapBottom = 2;
			public const int TextGapLeft = 4;
			public const int TextGapRight = 10;
			public const int TabGapTop = 3;
			public const int TabGapLeft = 2;
			public const int TabGapBetween = 10;
		}
	
		#region DockManager public constants

		public const double DefaultDockFactor = 0.25;

		#endregion

		#region DockManager private members

		private DockableFormsCollection				forms = null;
		private DockableForm						activeDocumentForm = null;	
		private DockableForm						activeAutoHideForm = null;
		private DockableFormsContainer				activeDocumentsContainer = null;
		private DockableFormsContainersCollection	containers = null;
		private FloatingFormsCollection				floatingForms = null;
		private DragHandler							dragHandler = null;
		private Matrix								identityMatrix = null;
		private DockState[]							autoHideDockStates = null;
		private StringFormat						horizontalTabStringFormat = null;
		private StringFormat						verticalTabStringFormat = null;
		private bool								redockingAllowed = true;
		private bool								mdiIntegration = true;
		private double								dockLeftFactor = DefaultDockFactor;
		private double								dockRightFactor = DefaultDockFactor;
		private double								dockTopFactor = DefaultDockFactor;
		private double								dockBottomFactor = DefaultDockFactor;
		private Color								dockPanelActiveCaptionColor = SystemColors.ActiveCaption;
		private Color								dockPanelActiveCaptionTextColor = SystemColors.ActiveCaptionText;
		private Color								dockPanelInactiveCaptionColor = SystemColors.InactiveCaption;
		private Color								dockPanelInactiveCaptionTextColor = SystemColors.InactiveCaptionText;
		private bool								animateAutoHide = true;
		private Control								autoHideControl = null;
		private bool								suspendAutoHideControlLayout = false;
		private System.Drawing.Font					defaultTabsFont = SystemInformation.MenuFont;
		private System.Drawing.Color				defaultTabsActiveTabTextColor = SystemColors.ControlText;
		private System.Drawing.Color				defaultTabsInactiveTabTextColor = SystemColors.GrayText;
		private Control								dummyControl = null;
		private bool								showTabsAlways = true;

		#endregion

		#region DockManager internal properties

		//---------------------------------------------------------------------------
		internal bool AnimateAutoHide { get { return animateAutoHide; } set	{ animateAutoHide = value; } }

		//---------------------------------------------------------------------------
		internal Control AutoHideControl { get { return autoHideControl; } }

		//---------------------------------------------------------------------------
		internal bool SuspendAutoHideControlLayout { get { return suspendAutoHideControlLayout; } set { suspendAutoHideControlLayout = value; } }

		//---------------------------------------------------------------------------
		internal Control DummyControl { get { return dummyControl; } }

		//---------------------------------------------------------------------------
		internal Rectangle DockArea
		{
			get
			{
				return new Rectangle(DockPadding.Left, DockPadding.Top,
					ClientRectangle.Width - DockPadding.Left - DockPadding.Right,
					ClientRectangle.Height - DockPadding.Top - DockPadding.Bottom);
			}
		}

		#endregion

		#region DockManager public properties

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public DockableFormsCollection Forms { get { return forms; } }

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public DockableForm[] Documents { get { return (Forms != null) ? Forms.Select(DockableForm.States.Document) : null; } }

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public DockableFormsContainersCollection Containers { get { return containers; } }

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public FloatingFormsCollection FloatingForms { get { return floatingForms; } }

		//---------------------------------------------------------------------------
		// This MdiIntegration property determines if current docking manager should
		// treat form in document state as MDI child form and merge its menu to 
		// the main form.
		[DefaultValue(true)]
		public bool MdiIntegration
		{
			get	{ return mdiIntegration; }
			set
			{
				if (value == mdiIntegration)
					return;

				mdiIntegration = value;
				RefreshMdiIntegration();
			}
		}

		//---------------------------------------------------------------------------
		[DefaultValue(true)]
		public bool IsRedockingAllowed { get { return redockingAllowed; } set { redockingAllowed = value; } }

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public DockableForm ActiveDocumentForm { get { return activeDocumentForm; } }

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public DockableFormsContainer ActiveDocumentsContainer { get { return activeDocumentsContainer; } }

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public DockableForm ActiveAutoHideForm
		{
			get	{ return activeAutoHideForm; }
			set
			{
				if (value == activeAutoHideForm)
					return;

				if (value != null)
				{
					if (!IsDockStateAutoHide(value.DockState) || value.DockManager != this)
                        throw new InvalidOperationException();
				}

				if (activeAutoHideForm != null)
				{
					DockableTabbedPanel tabbedPanel = activeAutoHideForm.TabbedPanel as DockableTabbedPanel;
					if (tabbedPanel != null)
						tabbedPanel.AnimateHide();
					else
						AutoHideControl.Hide();
				}

				DockableForm oldValue = activeAutoHideForm;
				activeAutoHideForm = value;

				if (oldValue != null)
					if (oldValue.FormContainer != null)
						oldValue.FormContainer.SetParent(null);
				if (value != null)
					if (value.FormContainer != null)
						value.FormContainer.SetParent(AutoHideControl);

				if (activeAutoHideForm != null)
					((DockableTabbedPanel)(activeAutoHideForm.TabbedPanel)).AnimateShow();

				Invalidate();
			}
		}

		//---------------------------------------------------------------------------
		public new System.Drawing.Color BackColor
		{
			get { return base.BackColor; }
			set 
			{ 
				base.BackColor = value; 

				if (Containers != null && Containers.Count > 0)
				{
					foreach(DockableFormsContainer aContainer in Containers)
					{
						if (aContainer.DocumentTabbedPanel != null)
							aContainer.DocumentTabbedPanel.BackColor= value; 
						if (aContainer.TabbedPanel != null)
							aContainer.TabbedPanel.BackColor= value; 
					}
				}
			}
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Color DockPanelActiveCaptionColor
		{
			get { return dockPanelActiveCaptionColor; }
			set { dockPanelActiveCaptionColor = value; }
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Color DockPanelActiveCaptionTextColor
		{
			get { return dockPanelActiveCaptionTextColor; }
			set { dockPanelActiveCaptionTextColor = value; }
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Color DockPanelInactiveCaptionColor
		{
			get { return dockPanelInactiveCaptionColor; }
			set { dockPanelInactiveCaptionColor = value; }
		}
		
		//---------------------------------------------------------------------------
		public System.Drawing.Color DockPanelInactiveCaptionTextColor
		{
			get { return dockPanelInactiveCaptionTextColor; }
			set { dockPanelInactiveCaptionTextColor = value; }
		}

		//---------------------------------------------------------------------------
		[DefaultValue(DefaultDockFactor)]
		public double DockLeftFactor
		{
			get	{ return dockLeftFactor; }
			set
			{
				if (value <= 0 || value >= 1)
					throw new ArgumentOutOfRangeException();

				if (value == dockLeftFactor)
					return;

				dockLeftFactor = value;

				if (dockLeftFactor + dockRightFactor > 1)
					dockRightFactor = 1 - dockLeftFactor;
				
				PerformLayout();
			}
		}

		//---------------------------------------------------------------------------
		[DefaultValue(DefaultDockFactor)]
		public double DockRightFactor
		{
			get	{ return dockRightFactor; }
			set
			{
				if (value <= 0 || value >= 1)
					throw new ArgumentOutOfRangeException();

				if (value == dockRightFactor)
					return;

				dockRightFactor = value;

				if (dockLeftFactor + dockRightFactor > 1)
					dockLeftFactor = 1 - dockRightFactor;

				PerformLayout();
			}
		}
	
		//---------------------------------------------------------------------------
		[DefaultValue(DefaultDockFactor)]
		public double DockTopFactor
		{
			get	{ return dockTopFactor;	}
			set
			{
				if (value <= 0 || value >= 1)
					throw new ArgumentOutOfRangeException();

				if (value == dockTopFactor)
					return;

				dockTopFactor = value;

				if (dockTopFactor + dockBottomFactor > 1)
					dockBottomFactor = 1 - dockTopFactor;

				PerformLayout();
			}
		}

		//---------------------------------------------------------------------------
		[DefaultValue(DefaultDockFactor)]
		public double DockBottomFactor
		{
			get	{ return dockBottomFactor; }
			set
			{
				if (value <= 0 || value >= 1)
					throw new ArgumentOutOfRangeException();

				if (value == dockBottomFactor)
					return;

				dockBottomFactor = value;

				if (dockTopFactor + dockBottomFactor > 1)
					dockTopFactor = 1 - dockBottomFactor;

				PerformLayout();
			}
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Font DefaultTabsFont
		{
			get { return defaultTabsFont; } 
			set { defaultTabsFont = value; }
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Color DefaultActiveTabTextColor 
		{	
			get { return defaultTabsActiveTabTextColor; } 
			set { defaultTabsActiveTabTextColor = value; } 
		}
		
		//---------------------------------------------------------------------------
		public System.Drawing.Color DefaultInactiveTabTextColor
		{	
			get { return defaultTabsInactiveTabTextColor; } 
			set { defaultTabsInactiveTabTextColor = value; } 
		}
		
		//---------------------------------------------------------------------------
		public bool ShowTabsAlways
		{
			get { return showTabsAlways; }

			set 
			{
				if (showTabsAlways == value)
					return;
				
				showTabsAlways = value;

				foreach (DockableFormsContainer aContainer in Containers)
					aContainer.ShowTabsAlways = showTabsAlways;
			}
		}
		
		#endregion

		#region DockManager protected properties

		//---------------------------------------------------------------------------
		[Browsable(false)]
		protected Rectangle DocumentRectangle
		{
			get
			{
				Rectangle rect = this.DockArea;
				if (GetContainer(DockState.DockLeft) != null)
				{
					rect.X += (int)(DockArea.Width * DockLeftFactor);
					rect.Width -= (int)(DockArea.Width * DockLeftFactor);
				}
				if (GetContainer(DockState.DockRight) != null)
					rect.Width -= (int)(DockArea.Width * DockRightFactor);
				if (GetContainer(DockState.DockTop) != null)
				{
					rect.Y += (int)(DockArea.Height * DockTopFactor);
					rect.Height -= (int)(DockArea.Height * DockTopFactor);
				}
				if (GetContainer(DockState.DockBottom) != null)
					rect.Height -= (int)(DockArea.Height * DockBottomFactor);

				return rect;
			}
		}

		#endregion
		
		#region DockManager constructor and dispose method

		//---------------------------------------------------------------------------
		public DockManager()
		{
			forms = new DockableFormsCollection();
			containers = new DockableFormsContainersCollection();
			floatingForms = new FloatingFormsCollection();
			dragHandler = new DragHandler(this);

			horizontalTabStringFormat = new StringFormat();
			horizontalTabStringFormat.Alignment = StringAlignment.Near;
			horizontalTabStringFormat.LineAlignment = StringAlignment.Center;
			horizontalTabStringFormat.FormatFlags = StringFormatFlags.NoWrap;

			verticalTabStringFormat = new StringFormat();
			verticalTabStringFormat.Alignment = StringAlignment.Near;
			verticalTabStringFormat.LineAlignment = StringAlignment.Center;
			verticalTabStringFormat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.DirectionVertical;

			identityMatrix = new Matrix();

			autoHideDockStates = new DockState[4];
			autoHideDockStates[0] = DockState.DockLeftAutoHide;
			autoHideDockStates[1] = DockState.DockRightAutoHide;
			autoHideDockStates[2] = DockState.DockTopAutoHide;
			autoHideDockStates[3] = DockState.DockBottomAutoHide;

			SetStyle
				(
				ControlStyles.ResizeRedraw |
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.DoubleBuffer,
				true
				);

			autoHideControl = new Control();
			autoHideControl.Visible = false;

			Controls.Add(autoHideControl);

			dummyControl = new Control();
			dummyControl.Bounds = Rectangle.Empty;
			Controls.Add(dummyControl);
		}

		//---------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				FloatingForms.Dispose();
				Containers.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region DockManager protected overriden methods

		//---------------------------------------------------------------------------
		protected override void OnLayout(LayoutEventArgs e)
		{
			CalculateDockPadding();

			int width = ClientRectangle.Width - DockPadding.Left - DockPadding.Right;
			int height = ClientRectangle.Height - DockPadding.Top - DockPadding.Bottom;
			int dockLeftSize = (int)(width * dockLeftFactor);
			int dockRightSize = (int)(width * dockRightFactor);
			int dockTopSize = (int)(height * dockTopFactor);
			int dockBottomSize = (int)(height * dockBottomFactor);

			if (dockLeftSize < MeasureContainer.MinSize)
				dockLeftSize = MeasureContainer.MinSize;
			if (dockRightSize < MeasureContainer.MinSize)
				dockRightSize = MeasureContainer.MinSize;
			if (dockTopSize < MeasureContainer.MinSize)
				dockTopSize = MeasureContainer.MinSize;
			if (dockBottomSize < MeasureContainer.MinSize)
				dockBottomSize = MeasureContainer.MinSize;

			DockableTabbedPanel dockLeft = GetDockedPanel(DockState.DockLeft);
			DockableTabbedPanel dockRight = GetDockedPanel(DockState.DockRight);
			DockableTabbedPanel dockTop = GetDockedPanel(DockState.DockTop);
			DockableTabbedPanel dockBottom = GetDockedPanel(DockState.DockBottom);

			if (dockLeft != null)
			{
				dockLeft.Dock = DockStyle.Left;
				dockLeft.Width = dockLeftSize;
			}
			if (dockRight != null)
			{
				dockRight.Dock = DockStyle.Right;
				dockRight.Width = dockRightSize;
			}
			if (dockTop != null)
			{
				dockTop.Dock = DockStyle.Top;
				dockTop.Height = dockTopSize;
			}
			if (dockBottom != null)
			{
				dockBottom.Dock = DockStyle.Bottom;
				dockBottom.Height = dockBottomSize;
			}

			// Make sure auto hide window at the top of Z-order
			if (ActiveAutoHideForm != null && !SuspendAutoHideControlLayout)
			{
				AutoHideControl.BringToFront();
				AutoHideControl.Bounds = GetAutoHideRectangle(true);
				if (ActiveAutoHideForm.TabbedPanel != null)
					ActiveAutoHideForm.TabbedPanel.Bounds = AutoHideControl.ClientRectangle;
			}

			// Make sure document window at the top of Z-order of any DockableTabbedPanel
			DockableTabbedPanel firstDockableTabbedPanel = null;
			foreach (Control aControl in Controls)
			{
				if (firstDockableTabbedPanel == null)
					firstDockableTabbedPanel = aControl as DockableTabbedPanel;
				
				DockableDocumentPanel aDocumentPanel = aControl as DockableDocumentPanel;
				if (aDocumentPanel != null)
				{
					aDocumentPanel.Dock = DockStyle.Fill;
					if (firstDockableTabbedPanel != null)
						Controls.SetChildIndex(aDocumentPanel, Controls.IndexOf(firstDockableTabbedPanel));
					break;
				}
			}

			base.OnLayout(e);
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button != MouseButtons.Left)
				return;

			DockableForm aForm = GetHitTest();
			if (aForm == null)
				return;

			if (aForm != ActiveAutoHideForm)
				ActiveAutoHideForm = aForm;

			if (aForm.FormContainer != null && !aForm.FormContainer.IsActivated)
				aForm.FormContainer.Activate();
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseHover(EventArgs e)
		{
			base.OnMouseHover(e);

			DockableForm aForm = GetHitTest();
			if (aForm != null && ActiveAutoHideForm != aForm)
			{
				ActiveAutoHideForm = aForm;
				Invalidate();
			}

			// requires further tracking of mouse hover behavior,
			// call TrackMouseEvent
			Win32.TRACKMOUSEEVENTS tme = new Win32.TRACKMOUSEEVENTS(Win32.TRACKMOUSEEVENTS.TME_HOVER, Handle, Win32.TRACKMOUSEEVENTS.HOVER_DEFAULT);
			User32.TrackMouseEvent(ref tme);
		}

		//---------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Graphics g = e.Graphics;

			Brush backBrush = new SolidBrush(this.BackColor);
			g.FillRectangle(backBrush, ClientRectangle);
			backBrush.Dispose();

			CalculateTabs(DockState.DockLeftAutoHide);
			CalculateTabs(DockState.DockRightAutoHide);
			CalculateTabs(DockState.DockTopAutoHide);
			CalculateTabs(DockState.DockBottomAutoHide);

			int leftAutoHideWindows = GetCountOfAutoHideWindows(DockState.DockLeftAutoHide);
			int rightAutoHideWindows = GetCountOfAutoHideWindows(DockState.DockRightAutoHide);
			int topAutoHideWindows = GetCountOfAutoHideWindows(DockState.DockTopAutoHide);
			int bottomAutoHideWindows = GetCountOfAutoHideWindows(DockState.DockBottomAutoHide);

			Brush brush = SystemBrushes.ControlLightLight;

			int height = GetTabStripHeight();
			if (leftAutoHideWindows != 0 && topAutoHideWindows != 0)
				g.FillRectangle(brush, 0, 0, height, height);
			if (leftAutoHideWindows != 0 && bottomAutoHideWindows != 0)
				g.FillRectangle(brush, 0, Height - height, height, height);
			if (topAutoHideWindows != 0 && rightAutoHideWindows != 0)
				g.FillRectangle(brush, Width - height, 0, height, height);
			if (rightAutoHideWindows != 0 && bottomAutoHideWindows != 0)
				g.FillRectangle(brush, Width - height, Height - height, height, height);
			
			DrawTabStrip(e.Graphics, DockState.DockLeftAutoHide);
			DrawTabStrip(e.Graphics, DockState.DockRightAutoHide);
			DrawTabStrip(e.Graphics, DockState.DockTopAutoHide);
			DrawTabStrip(e.Graphics, DockState.DockBottomAutoHide);

			PerformLayout();
		}

		#endregion

		#region DockManager internal methods

		//---------------------------------------------------------------------------
		internal DockableFormsContainer GetActiveContainer()
		{
			foreach(DockableFormsContainer aContainer in Containers)
			{
				if (aContainer.IsActivated)
					return aContainer;
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		internal void SetActiveDocumentForm(DockableForm value)
		{
			if (activeDocumentForm == value)
				return;

			activeDocumentForm = value;

			if (activeDocumentForm != null)
				if (activeDocumentForm.HiddenMdiChild != null)
					activeDocumentForm.HiddenMdiChild.Activate();

			OnActiveDocumentChanged(EventArgs.Empty);
		}

		//---------------------------------------------------------------------------
		internal void SetActiveDocument(DockableFormsContainer value)
		{
			if (activeDocumentsContainer == value)
				return;

			DockableFormsContainer lastActive = activeDocumentsContainer;
			activeDocumentsContainer = value;
			SetActiveDocumentForm(value != null ? value.ActiveForm : null);
			if (lastActive != null)
				lastActive.IsActivated = false;
		}

		//---------------------------------------------------------------------------
		internal void AddForm(DockableForm aFormToAdd)
		{
			if (aFormToAdd == null)
				throw(new ArgumentNullException());

			if (!Forms.Contains(aFormToAdd))
			{
				Forms.Add(aFormToAdd);
				OnFormAdded(new DockableFormEventArgs(aFormToAdd));
			}
		}

		//---------------------------------------------------------------------------
		internal void RemoveForm(DockableForm aFormToRemove)
		{
			if (aFormToRemove == null)
				throw(new ArgumentNullException());
			
			if (Forms.Contains(aFormToRemove))
			{
				Forms.Remove(aFormToRemove);
				OnFormRemoved(new DockableFormEventArgs(aFormToRemove));
			}
		}

		//---------------------------------------------------------------------------
		internal void AddContainer(DockableFormsContainer aContainerToAdd)
		{
			if (Containers.Contains(aContainerToAdd))
				return;

			Containers.Add(aContainerToAdd);
		}

		//---------------------------------------------------------------------------
		internal void RemoveContainer(DockableFormsContainer aContainerToRemove)
		{
			if (!Containers.Contains(aContainerToRemove))
				return;

			Containers.Remove(aContainerToRemove);
		}

		//---------------------------------------------------------------------------
		internal void AddFloatingForm(FloatingForm aFloatingFormToAdd)
		{
			if (FloatingForms.Contains(aFloatingFormToAdd))
				return;

			FloatingForms.Add(aFloatingFormToAdd);
		}

		//---------------------------------------------------------------------------
		internal void RemoveFloatingForm(FloatingForm aFloatingFormToRemove)
		{
			if (!FloatingForms.Contains(aFloatingFormToRemove))
				return;

			FloatingForms.Remove(aFloatingFormToRemove);
		}

		//---------------------------------------------------------------------------
		internal Rectangle GetAutoHideRectangle(bool final)
		{
			if (ActiveAutoHideForm == null)
				return Rectangle.Empty;

			DockState state = ActiveAutoHideForm.DockState;
			Rectangle rectDockArea = DockArea;
			int autoHideWidth = (int)(rectDockArea.Width * ActiveAutoHideForm.AutoHiddenFactor);
			int autoHideHeight = (int)(rectDockArea.Height * ActiveAutoHideForm.AutoHiddenFactor);

			Rectangle rectTabStrip = GetTabStripRectangle(state, true);
			Rectangle rect = Rectangle.Empty;
			if (state == DockState.DockLeftAutoHide)
			{
				rect.X = rectTabStrip.X + rectTabStrip.Width;
				rect.Y = rectTabStrip.Y;
				rect.Width = (final ? autoHideWidth : 0);
				rect.Height = rectTabStrip.Height;
			}
			else if (state == DockState.DockRightAutoHide)
			{
				rect.X = rectTabStrip.X - (final ? autoHideWidth : 0);
				rect.Y = rectTabStrip.Y;
				rect.Width = (final ? autoHideWidth : 0);
				rect.Height = rectTabStrip.Height;
			}
			else if (state == DockState.DockTopAutoHide)
			{
				rect.X = rectTabStrip.X;
				rect.Y = rectTabStrip.Y + rectTabStrip.Height;
				rect.Width = rectTabStrip.Width;
				rect.Height = (final ? autoHideHeight : 0);
			}
			else
			{
				rect.X = rectTabStrip.X;
				rect.Y = rectTabStrip.Y - (final ? autoHideHeight : 0);
				rect.Width = rectTabStrip.Width;
				rect.Height = (final ? autoHideHeight : 0);
			}

			return rect;
		}

		//---------------------------------------------------------------------------
		internal DockableFormsContainer GetContainer(DockState dockState)
		{
			foreach (DockableFormsContainer aContainer in Containers)
			{
				if (aContainer.DockState == dockState)
					return aContainer;
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		internal void RefreshContainers(DockState dockState)
		{
			if (dockState == DockState.Float)
				return;

			DockableFormsContainer first = GetContainer(dockState);
			if (first == null)
				return;

			if (dockState == DockState.Hidden || dockState == DockState.Unknown)
				first.SetParent(null);
			else if (IsDockStateAutoHide(dockState))
				first.SetParent((ActiveAutoHideForm == first.ActiveForm) ? AutoHideControl : null);
			else
				first.SetParent(this);

			if (first.TabbedPanel is DockableDocumentPanel)
				first.TabbedPanel.BorderWidth = 1;

			for (DockableFormsContainer aContainer = first.Next; aContainer != null; aContainer = aContainer.Next)
			{
				if (IsDockStateAutoHide(dockState))
					aContainer.SetParent(ActiveAutoHideForm == aContainer.ActiveForm ? AutoHideControl : null);
				else if (dockState == DockState.Hidden || dockState == DockState.Unknown)
					aContainer.SetParent(null);
				else if (IsDockStateAutoHide(dockState))
					aContainer.SetParent((ActiveAutoHideForm == aContainer.ActiveForm) ? AutoHideControl : null);
				else
					aContainer.SetParent(aContainer.Previous.TabbedPanel);

				if (aContainer.TabbedPanel is DockableDocumentPanel)
					aContainer.TabbedPanel.BorderWidth = 0;
			}
		}

		//---------------------------------------------------------------------------
		internal Rectangle GetTabStripRectangle(DockState dockState, bool transformed)
		{
			if (!IsDockStateAutoHide(dockState))
				return Rectangle.Empty;

			int leftAutoHideWindows = GetCountOfAutoHideWindows(DockState.DockLeftAutoHide);
			int rightAutoHideWindows = GetCountOfAutoHideWindows(DockState.DockRightAutoHide);
			int topAutoHideWindows = GetCountOfAutoHideWindows(DockState.DockTopAutoHide);
			int bottomAutoHideWindows = GetCountOfAutoHideWindows(DockState.DockBottomAutoHide);

			int x, y, width, height;

			height = GetTabStripHeight();
			if (dockState == DockState.DockLeftAutoHide)
			{
				if (leftAutoHideWindows == 0)
					return Rectangle.Empty;

				x = 0;
				y = (topAutoHideWindows == 0) ? 0 : height;
				width = Height - (topAutoHideWindows == 0 ? 0 : height) - (bottomAutoHideWindows == 0 ? 0 :height);
			}
			else if (dockState == DockState.DockRightAutoHide)
			{
				if (rightAutoHideWindows == 0)
					return Rectangle.Empty;

				x = Width - height;
				if (leftAutoHideWindows != 0 && x < height)
					x = height;
				y = (topAutoHideWindows == 0) ? 0 : height;
				width = Height - (topAutoHideWindows == 0 ? 0 : height) - (bottomAutoHideWindows == 0 ? 0 :height);
			}
			else if (dockState == DockState.DockTopAutoHide)
			{
				if (topAutoHideWindows == 0)
					return Rectangle.Empty;

				x = leftAutoHideWindows == 0 ? 0 : height;
				y = 0;
				width = Width - (leftAutoHideWindows == 0 ? 0 : height) - (rightAutoHideWindows == 0 ? 0 : height);
			}
			else
			{
				if (bottomAutoHideWindows == 0)
					return Rectangle.Empty;

				x = leftAutoHideWindows == 0 ? 0 : height;
				y = Height - height;
				if (topAutoHideWindows != 0 && y < height)
					y = height;
				width = Width - (leftAutoHideWindows == 0 ? 0 : height) - (rightAutoHideWindows == 0 ? 0 : height);
			}

			if (!transformed)
				return new Rectangle(x, y, width, height);
			else
				return TransformRectangle(dockState, new Rectangle(x, y, width, height));
		}

		//---------------------------------------------------------------------------
		internal void TestDrop(DragHandler dragHandler, Point pt)
		{
			System.Drawing.Rectangle currentDockArea = this.DockArea;
			if (currentDockArea.Width <=0 || currentDockArea.Height <= 0)
				return;

			Point ptClient = PointToClient(pt);

			int dragSize = MeasureContainer.DragSize;
			
			Rectangle rectDoc = DocumentRectangle;

			if ((ptClient.Y - rectDoc.Top) >= 0 && (ptClient.Y - rectDoc.Top) < dragSize &&
				GetContainer(DockState.DockTop) == null &&
				dragHandler.IsDockStateValid(DockState.DockTop))
				dragHandler.DropTarget.SetDropTarget(this, DockStyle.Top);
			else if ((rectDoc.Bottom - ptClient.Y) >= 0 && (rectDoc.Bottom - ptClient.Y) < dragSize &&
				GetContainer(DockState.DockBottom) == null &&
				dragHandler.IsDockStateValid(DockState.DockBottom))
				dragHandler.DropTarget.SetDropTarget(this, DockStyle.Bottom);
			else if ((rectDoc.Right - ptClient.X) >= 0 && (rectDoc.Right - ptClient.X) < dragSize &&
				GetContainer(DockState.DockRight) == null &&
				dragHandler.IsDockStateValid(DockState.DockRight))
				dragHandler.DropTarget.SetDropTarget(this, DockStyle.Right);
			else if ((ptClient.X - rectDoc.Left) >= 0 && (ptClient.X - rectDoc.Left) < dragSize &&
				GetContainer(DockState.DockLeft) == null &&
				dragHandler.IsDockStateValid(DockState.DockLeft))
				dragHandler.DropTarget.SetDropTarget(this, DockStyle.Left);
			else if ((((ptClient.Y - rectDoc.Top) >= dragSize && (ptClient.Y - rectDoc.Top) < 2 * dragSize) ||
				((rectDoc.Bottom - ptClient.Y) >= dragSize && (rectDoc.Bottom - ptClient.Y) < 2 * dragSize) ||
				((rectDoc.Right - ptClient.X) >= dragSize && (rectDoc.Right - ptClient.X) < 2 * dragSize) ||
				((ptClient.X - rectDoc.Left) >= dragSize && (ptClient.X - rectDoc.Left) < 2 * dragSize)) &&
				GetContainer(DockState.Document) == null &&
				dragHandler.IsDockStateValid(DockState.Document))
				dragHandler.DropTarget.SetDropTarget(this, DockStyle.Fill);
			else
				return;

			if (dragHandler.DropTarget.SameAsOldValue)
				return;

			if (dragHandler.DropTarget.DockStyle == DockStyle.Top || dragHandler.DropTarget.DockStyle == DockStyle.Bottom)
				currentDockArea.Height = (int)(DockArea.Height * DockTopFactor);
			else if (dragHandler.DropTarget.DockStyle == DockStyle.Left || dragHandler.DropTarget.DockStyle == DockStyle.Right)
				currentDockArea.Width = (int)(DockArea.Width * DockTopFactor);
			if (dragHandler.DropTarget.DockStyle == DockStyle.Bottom)
				currentDockArea.Y = DockArea.Bottom - currentDockArea.Height;
			else if (dragHandler.DropTarget.DockStyle == DockStyle.Right)
				currentDockArea.X = DockArea.Right - currentDockArea.Width;
			else if (dragHandler.DropTarget.DockStyle == DockStyle.Fill)
				currentDockArea = DocumentRectangle;

			currentDockArea.Location = PointToScreen(currentDockArea.Location);
			dragHandler.DragFrame = DockManager.DragDrawHelper.CreateDragFrame(currentDockArea, dragSize);
		}
		
		//---------------------------------------------------------------------------
		internal void BeginDragForm(TabbedPanel tabbedPanel, Rectangle rectTabWindow)
		{
			if (dragHandler == null)
				return;
			dragHandler.BeginDragForm(tabbedPanel, rectTabWindow);		
		}

		//---------------------------------------------------------------------------
		internal void BeginDragContainer(TabbedPanel tabbedPanel, Point captionLocation)
		{
			if (dragHandler == null)
				return;
			dragHandler.BeginDragContainer(tabbedPanel, captionLocation);		
		}

		//---------------------------------------------------------------------------
		internal void BeginDragFloatingForm(FloatingForm aFloatingForm)
		{
			if (dragHandler == null)
				return;
			dragHandler.BeginDragFloatingForm(aFloatingForm);		
		}

		//---------------------------------------------------------------------------
		internal void BeginDragTabbedPanelSplitter(TabbedPanel tabbedPanel, Point splitterLocation)
		{
			if (dragHandler == null)
				return;
			dragHandler.BeginDragTabbedPanelSplitter(tabbedPanel, splitterLocation);
		}

		#endregion

		#region DockManager private methods

		//---------------------------------------------------------------------------
		private void CalculateDockPadding()
		{
			DockPadding.All = 0;

			foreach (DockState state in autoHideDockStates)
			{
				int countAutoHideWindows = GetCountOfAutoHideWindows(state);

				if (countAutoHideWindows == 0)
					continue;

				Rectangle rectTabStrip = GetTabStripRectangle(state);

				if (state == DockState.DockLeftAutoHide)
					DockPadding.Left = rectTabStrip.Height;
				else if (state == DockState.DockRightAutoHide)
					DockPadding.Right = rectTabStrip.Height;
				else if (state == DockState.DockTopAutoHide)
					DockPadding.Top = rectTabStrip.Height;
				else if (state == DockState.DockBottomAutoHide)
					DockPadding.Bottom = rectTabStrip.Height;
			}
		}

		//---------------------------------------------------------------------------
		private void CalculateTabs(DockState dockState)
		{
			Rectangle rectTabStrip = GetTabStripRectangle(dockState);

			int imageHeight = rectTabStrip.Height - MeasureAutoHideTab.ImageGapTop -
				MeasureAutoHideTab.ImageGapBottom;
			int imageWidth = MeasureAutoHideTab.ImageWidth;
			if (imageHeight > MeasureAutoHideTab.ImageHeight)
				imageWidth = MeasureAutoHideTab.ImageWidth * (imageHeight/MeasureAutoHideTab.ImageHeight);

			int maxWidth = 0;
			
			Graphics g = this.CreateGraphics();

			foreach (DockableFormsContainer aContainer in Containers)
			{
				if (aContainer.DockState != dockState)
					continue;

				foreach (DockableForm aForm in aContainer.Forms)
				{
					if (aContainer.IsHidden)
						continue;

					int width = imageWidth + MeasureAutoHideTab.ImageGapLeft +
						MeasureAutoHideTab.ImageGapRight +
						(int)g.MeasureString(aForm.Text, aForm.TabsFont).Width + 1 +
						MeasureAutoHideTab.TextGapLeft + MeasureAutoHideTab.TextGapRight;
					if (width > maxWidth)
						maxWidth = width;
				}
			}
			
			g.Dispose();
			
			int x = MeasureAutoHideTab.TabGapLeft + rectTabStrip.X;
			foreach (DockableFormsContainer aContainer in Containers)
			{
				if (aContainer.DockState != dockState)
					continue;

				foreach (DockableForm aForm in aContainer.Forms)
				{
					if (aForm.IsHidden)
						continue;

					aForm.TabX = x;
					if (aForm == aContainer.ActiveForm)
						aForm.TabWidth = maxWidth;
					else
						aForm.TabWidth = imageWidth + MeasureAutoHideTab.ImageGapLeft + MeasureAutoHideTab.ImageGapRight;
					x += aForm.TabWidth;
				}
				x += MeasureAutoHideTab.TabGapBetween;
			}
		}

		//---------------------------------------------------------------------------
		private void DrawTab(Graphics g, DockState dockState, DockableFormsContainer aContainer, DockableForm aForm)
		{
			Rectangle rectTab = GetTabRectangle(dockState, aForm);
			if (rectTab.IsEmpty)
				return;

			g.FillRectangle(SystemBrushes.Control, rectTab);

			g.DrawLine(SystemPens.GrayText, rectTab.Left, rectTab.Top, rectTab.Left, rectTab.Bottom);
			g.DrawLine(SystemPens.GrayText, rectTab.Right, rectTab.Top, rectTab.Right, rectTab.Bottom);
			if (dockState == DockState.DockTopAutoHide || dockState == DockState.DockRightAutoHide)
				g.DrawLine(SystemPens.GrayText, rectTab.Left, rectTab.Bottom, rectTab.Right, rectTab.Bottom);
			else
				g.DrawLine(SystemPens.GrayText, rectTab.Left, rectTab.Top, rectTab.Right, rectTab.Top);


			// Set no rotate for drawing icon and text
			Matrix matrixRotate = g.Transform;
			g.Transform = identityMatrix;

			// Draw the icon
			Rectangle rectImage = rectTab;
			rectImage.X += MeasureAutoHideTab.ImageGapLeft;
			rectImage.Y += MeasureAutoHideTab.ImageGapTop;
			int imageHeight = rectTab.Height - MeasureAutoHideTab.ImageGapTop -	MeasureAutoHideTab.ImageGapBottom;
			int imageWidth = MeasureAutoHideTab.ImageWidth;
			if (imageHeight > MeasureAutoHideTab.ImageHeight)
				imageWidth = MeasureAutoHideTab.ImageWidth * (imageHeight/MeasureAutoHideTab.ImageHeight);
			rectImage.Height = imageHeight;
			rectImage.Width = imageWidth;
			rectImage = TransformRectangle(dockState, rectImage);
			g.DrawIcon(aForm.Icon, rectImage);

			// Draw the text
			if (aForm == aContainer.ActiveForm)
			{
				System.Drawing.SolidBrush tabTextBrush = new System.Drawing.SolidBrush(aContainer.TabsInactiveTabTextColor);
			
				Rectangle rectText = rectTab;
				rectText.X += MeasureAutoHideTab.ImageGapLeft + imageWidth + MeasureAutoHideTab.ImageGapRight + MeasureAutoHideTab.TextGapLeft;
				rectText.Width -= MeasureAutoHideTab.ImageGapLeft + imageWidth + MeasureAutoHideTab.ImageGapRight + MeasureAutoHideTab.TextGapLeft;
				rectText = TransformRectangle(dockState, rectText);
				if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
					g.DrawString(aForm.Text, aForm.TabsFont, tabTextBrush, rectText, verticalTabStringFormat);
				else
					g.DrawString(aForm.Text, aForm.TabsFont, tabTextBrush, rectText, horizontalTabStringFormat);

				tabTextBrush.Dispose();
			}

			// Set rotate back
			g.Transform = matrixRotate;
		}

		//---------------------------------------------------------------------------
		private void DrawTabStrip(Graphics g, DockState dockState)
		{
			Rectangle rectTabStrip = GetTabStripRectangle(dockState);

			if (rectTabStrip.IsEmpty)
				return;

			Matrix matrixIdentity = g.Transform;
			if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
			{
				Matrix matrixRotated = new Matrix();
				matrixRotated.RotateAt(90, new PointF((float)rectTabStrip.X + (float)rectTabStrip.Height / 2,
					(float)rectTabStrip.Y + (float)rectTabStrip.Height / 2));
				g.Transform = matrixRotated;
			}

			g.FillRectangle(SystemBrushes.ControlLightLight, rectTabStrip);

			foreach (DockableFormsContainer aContainer in Containers)
			{
				if (aContainer.DockState != dockState)
					continue;

				foreach (DockableForm aForm in aContainer.Forms)
				{
					if (!aForm.IsHidden)
						DrawTab(g, dockState, aContainer, aForm);
				}
			}
			g.Transform = matrixIdentity;
		}

		//---------------------------------------------------------------------------
		private int GetCountOfAutoHideWindows(DockState dockState)
		{
			int result = 0;

			foreach (DockableFormsContainer aContainer in Containers)
			{
				if (aContainer.DockState == dockState)
					result ++;
			}

			return result;
		}

		//---------------------------------------------------------------------------
		private DockableTabbedPanel GetDockedPanel(DockState dockState)
		{
			DockableFormsContainer first = GetContainer(dockState);
			return (first == null ? null : first.TabbedPanel as DockableTabbedPanel);
		}

		//---------------------------------------------------------------------------
		private DockableForm GetHitTest()
		{
			Point ptMouse = PointToClient(Control.MousePosition);

			foreach(DockState state in autoHideDockStates)
			{
				Rectangle rectTabStrip = GetTabStripRectangle(state, true);
				if (!rectTabStrip.Contains(ptMouse))
					continue;

				foreach(DockableFormsContainer aContainer in Containers)
				{
					if (aContainer.DockState != state)
						continue;

					foreach(DockableForm aForm in aContainer.Forms)
					{
						if (aForm.IsHidden)
							continue;

						Rectangle rectTab = GetTabRectangle(state, aForm, true);
						rectTab.Intersect(rectTabStrip);
						if (rectTab.Contains(ptMouse))
							return aForm;
					}
				}
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		private Rectangle GetTabRectangle(DockState dockState, DockableForm aForm)
		{
			return GetTabRectangle(dockState, aForm, false);
		}

		//---------------------------------------------------------------------------
		private Rectangle GetTabRectangle(DockState dockState, DockableForm aForm, bool transformed)
		{
			Rectangle rectTabStrip = GetTabStripRectangle(dockState);

			if (rectTabStrip.IsEmpty)
				return Rectangle.Empty;

			int x = aForm.TabX;
			int y = rectTabStrip.Y + 
				(dockState == DockState.DockTopAutoHide || dockState == DockState.DockRightAutoHide ?
				0 : MeasureAutoHideTab.TabGapTop);
			int width = aForm.TabWidth;
			int height = rectTabStrip.Height - MeasureAutoHideTab.TabGapTop;

			if (!transformed)
				return new Rectangle(x, y, width, height);
			else
				return TransformRectangle(dockState, new Rectangle(x, y, width, height));
		}

		//---------------------------------------------------------------------------
		private int GetTabStripHeight()
		{
			int maxTabsFontHeight = 0;
			foreach (DockableFormsContainer aContainer in Containers)
			{
				foreach (DockableForm aForm in aContainer.Forms)
				{
					if (aForm.TabsFont.Height > maxTabsFontHeight)
						maxTabsFontHeight = aForm.TabsFont.Height;
				}
			}

			return Math.Max(MeasureAutoHideTab.ImageGapBottom +
				MeasureAutoHideTab.ImageGapTop + MeasureAutoHideTab.ImageHeight,
				maxTabsFontHeight) + MeasureAutoHideTab.TabGapTop;
		}

		//---------------------------------------------------------------------------
		private Rectangle GetTabStripRectangle(DockState dockState)
		{
			return GetTabStripRectangle(dockState, false);
		}

		//---------------------------------------------------------------------------
		private Rectangle TransformRectangle(DockState dockState, Rectangle rect)
		{
			if (dockState != DockState.DockLeftAutoHide && dockState != DockState.DockRightAutoHide)
				return rect;

			PointF[] pts = new PointF[1];
			// the center of the rectangle
			pts[0].X = (float)rect.X + (float)rect.Width / 2;
			pts[0].Y = (float)rect.Y + (float)rect.Height / 2;
			Rectangle rectTabStrip = GetTabStripRectangle(dockState);
			Matrix matrix = new Matrix();
			matrix.RotateAt(90, new PointF((float)rectTabStrip.X + (float)rectTabStrip.Height / 2,
				(float)rectTabStrip.Y + (float)rectTabStrip.Height / 2));
			matrix.TransformPoints(pts);

			return new Rectangle((int)(pts[0].X - (float)rect.Height / 2 + .5F),
				(int)(pts[0].Y - (float)rect.Width / 2 + .5F),
				rect.Height, rect.Width);
		}

		#endregion

		#region DockManager public methods
		
		//---------------------------------------------------------------------------
		public void RefreshMdiIntegration()
		{
			foreach (DockableFormsContainer aContainer in Containers)
			{
				if (aContainer.DockState == DockState.Document)
				{
					foreach (DockableForm aForm in aContainer.Forms)
						aForm.RefreshMdiIntegration();
				}
			}
		}

		//---------------------------------------------------------------------------
		public void SetContainerIndex(DockableFormsContainer aContainer, int index)
		{
			int oldIndex = Containers.IndexOf(aContainer);

			if ((index < 0 || index > (Containers.Count - 1)) && (index != -1))
                throw new ArgumentOutOfRangeException();
				
			if (oldIndex >= 0 && oldIndex == index)
				return;
			if (oldIndex == Containers.Count - 1 && index == -1)
				return;

			if (oldIndex >= 0)
				Containers.Remove(aContainer);
			
			if (index == -1)
				Containers.Add(aContainer);
			else if (oldIndex < index)
				Containers.Insert(index - 1, aContainer);
			else
				Containers.Insert(index, aContainer);

			RefreshContainers(aContainer.DockState);
		}

		//---------------------------------------------------------------------------
		public void CloseAllForms()
		{
			if (Forms != null && Forms.Count > 0)
			{
				for (int i = Forms.Count -1; i >= 0; i--)
				{
					DockableForm formToClose = Forms[i]; 
					if (formToClose == null)
						continue;

					RemoveForm(formToClose);
					formToClose.Close();
				}
			}

			if (FloatingForms != null && FloatingForms.Count > 0)
			{
				for (int i = FloatingForms.Count -1; i >= 0; i--)
				{
					FloatingForm formToClose = FloatingForms[i]; 
					if (formToClose == null)
						continue;

					RemoveFloatingForm(formToClose);
					formToClose.Close();
				}
			}

			if (Containers != null && Containers.Count > 0)
			{
				foreach (DockableFormsContainer aContainer in Containers)
					aContainer.CloseAllForms();
				
				Containers.RemoveAll();
			}
		}

		#endregion

		#region DockManager public static methods
		
		//---------------------------------------------------------------------------
		public static bool IsDockStateAutoHide(DockState dockState)
		{
			if (dockState == DockState.DockLeftAutoHide ||
				dockState == DockState.DockRightAutoHide ||
				dockState == DockState.DockTopAutoHide ||
				dockState == DockState.DockBottomAutoHide)
				return true;
			else
				return false;
		}

		//---------------------------------------------------------------------------
		public static bool IsDockStateDocked(DockState dockState)
		{
			return (dockState == DockState.DockLeft ||
				dockState == DockState.DockRight ||
				dockState == DockState.DockTop ||
				dockState == DockState.DockBottom);
		}

		//---------------------------------------------------------------------------
		public static bool IsDockBottom(DockState dockState)
		{
			return (dockState == DockState.DockBottom || dockState == DockState.DockBottomAutoHide) ? true : false;
		}

		//---------------------------------------------------------------------------
		public static bool IsDockLeft(DockState dockState)
		{
			return (dockState == DockState.DockLeft || dockState == DockState.DockLeftAutoHide) ? true : false;
		}

		//---------------------------------------------------------------------------
		public static bool IsDockRight(DockState dockState)
		{
			return (dockState == DockState.DockRight || dockState == DockState.DockRightAutoHide) ? true : false;
		}

		//---------------------------------------------------------------------------
		public static bool IsDockTop(DockState dockState)
		{
			return (dockState == DockState.DockTop || dockState == DockState.DockTopAutoHide ) ? true : false;
		}

		//---------------------------------------------------------------------------
		public static bool IsDockStateValid(DockState dockState, DockableForm.States allowedStates)
		{
			if (((allowedStates & DockableForm.States.Float) == 0) &&
				(dockState == DockState.Float))
				return false;
			else if (((allowedStates & DockableForm.States.Document) == 0) &&
				(dockState == DockState.Document))
				return false;
			else if (((allowedStates & DockableForm.States.DockLeft) == 0) &&
				(dockState == DockState.DockLeft || dockState == DockState.DockLeftAutoHide))
				return false;
			else if (((allowedStates & DockableForm.States.DockRight) == 0) &&
				(dockState == DockState.DockRight || dockState == DockState.DockRightAutoHide))
				return false;
			else if (((allowedStates & DockableForm.States.DockTop) == 0) &&
				(dockState == DockState.DockTop || dockState == DockState.DockTopAutoHide))
				return false;
			else if (((allowedStates & DockableForm.States.DockBottom) == 0) &&
				(dockState == DockState.DockBottom || dockState == DockState.DockBottomAutoHide))
				return false;
			else
				return true;
		}

		//---------------------------------------------------------------------------
		public static bool IsValidRestoreState(DockState state)
		{
			if (state == DockState.DockLeft || state == DockState.DockRight || state == DockState.DockTop ||
				state == DockState.DockBottom || state == DockState.Document)
				return true;
			else
				return false;
		}

		//---------------------------------------------------------------------------
		public static DockState ToggleAutoHideState(DockState state)
		{
			if (state == DockState.DockLeft)
				return DockState.DockLeftAutoHide;
			else if (state == DockState.DockRight)
				return DockState.DockRightAutoHide;
			else if (state == DockState.DockTop)
				return DockState.DockTopAutoHide;
			else if (state == DockState.DockBottom)
				return DockState.DockBottomAutoHide;
			else if (state == DockState.DockLeftAutoHide)
				return DockState.DockLeft;
			else if (state == DockState.DockRightAutoHide)
				return DockState.DockRight;
			else if (state == DockState.DockTopAutoHide)
				return DockState.DockTop;
			else if (state == DockState.DockBottomAutoHide)
				return DockState.DockBottom;
			else
				return state;
		}

		#endregion

		#region DockManager events

		//---------------------------------------------------------------------------
		private static readonly object ActiveDocumentChangedEvent = new object();
		public event EventHandler ActiveDocumentChanged
		{
			add { Events.AddHandler(ActiveDocumentChangedEvent, value);	}
			remove { Events.RemoveHandler(ActiveDocumentChangedEvent, value);	}
		}
		//---------------------------------------------------------------------------
		protected virtual void OnActiveDocumentChanged(EventArgs e)
		{
			EventHandler handler = (EventHandler)Events[ActiveDocumentChangedEvent];
			if (handler != null)
				handler(this, e);
		}

		//---------------------------------------------------------------------------
		public delegate void DockManagerFormEventHandler(object sender, DockableFormEventArgs e);
		private static readonly object FormAddedEvent = new object();
		public event DockManagerFormEventHandler FormAdded
		{
			add	{ Events.AddHandler(FormAddedEvent, value); }
			remove { Events.RemoveHandler(FormAddedEvent, value); }
		}
		//---------------------------------------------------------------------------
		protected virtual void OnFormAdded(DockableFormEventArgs e)
		{
			DockManagerFormEventHandler handler = (DockManagerFormEventHandler)Events[FormAddedEvent];
			if (handler != null)
				handler(this, e);
		}

		//---------------------------------------------------------------------------
		private static readonly object FormRemovedEvent = new object();
		public event DockManagerFormEventHandler FormRemoved
		{
			add { Events.AddHandler(FormRemovedEvent, value); }
			remove { Events.RemoveHandler(FormRemovedEvent, value); }
		}
		//---------------------------------------------------------------------------
		protected virtual void OnFormRemoved(DockableFormEventArgs e)
		{
			DockManagerFormEventHandler handler = (DockManagerFormEventHandler)Events[FormRemovedEvent];
			if (handler != null)
				handler(this, e);
		}

		#endregion
	
		#region Load from xml File
		
		#region DockManager private constant strings for XML

		private const string XML_DOCK_MANAGER_ELEMENT_TAG					= "DockManager";
		private const string XML_DOCK_MANAGER_SYNTAX_VERSION				= "1.0";
		private const string XML_FORMS_TAG									= "Forms";
		private const string XML_FORM_TAG_PREFIX							= "Form";
		private const string XML_FLOATINGFORMS_TAG							= "FloatingForms";
		private const string XML_FLOATINGFORM_TAG_PREFIX					= "FloatingForm";
		private const string XML_FORM_CONTAINERS_TAG						= "FormsContainers";
		private const string XML_FORM_CONTAINER_TAG_PREFIX					= "FormsContainer";
		private const string XML_FORMAT_VERSION_ATTRIBUTE					= "FormatVersion";
		private const string XML_COUNT_ATTRIBUTE							= "count";
		private const string XML_PERSIST_STRING_ATTRIBUTE					= "persistString";
		private const string XML_DOCKLEFT_FACTOR_ATTRIBUTE					= "DockLeftFactor";
		private const string XML_DOCKRIGHT_FACTOR_ATTRIBUTE					= "DockRightFactor";
		private const string XML_DOCKTOP_FACTOR_ATTRIBUTE					= "DockTopFactor";
		private const string XML_DOCKBOTTOM_FACTOR_ATTRIBUTE				= "DockBottomFactor";
		private const string XML_AUTOHIDDEN_PERCENTAGE_ATTRIBUTE			= "autoHiddenFactor";
		private const string XML_IS_HIDDEN_ATTRIBUTE						= "hidden";
		private const string XML_FLOATINGFORM_INDEX_ATTRIBUTE				= "floatingFormIndex";
		private const string XML_BOUNDS_ATTRIBUTE							= "bounds";
		private const string XML_REDOCKING_ALLOWED_ATTRIBUTE				= "redockingAllowed";
		private const string XML_VISIBLE_STATE_ATTRIBUTE					= "visibleState";
		private const string XML_RESTORE_STATE_ATTRIBUTE					= "restoreState";
		private const string XML_DOCK_LAYOUT_ATTRIBUTE						= "dockLayout";
		private const string XML_DOCUMENT_LAYOUT_ATTRIBUTE					= "documentLayout";
		private const string XML_FLOATING_LAYOUT_ATTRIBUTE					= "floatLayout"; 
		private const string XML_DOCK_SIZE_ATTRIBUTE						= "dockSize";
		private const string XML_DOCUMENT_SIZE_ATTRIBUTE					= "documentSize";
		private const string XML_FLOATING_SIZE_ATTRIBUTE					= "floatingSize";
		private const string XML_IS_ACTIVATED_ATTRIBUTE						= "isActivated";
		private const string XML_ACTIVE_FORM_INDEX_ATTRIBUTE				= "activeFormIndex";
		private const string XML_CONTAINER_INDEX_IN_FLOATINGFORMS_ATTRIBUTE	= "containerIndexInFloatingForms";
		private const string XML_CONTAINER_FORM_REFINDEX					= "RefIndex";

		#endregion

		public delegate DockableForm GetFormCallback(string persistString);

		//---------------------------------------------------------------------------
		public void LoadFromXmlFile(string xmlFilename, GetFormCallback getFormCallback)
		{
			if (!File.Exists(xmlFilename))
				return;

			FileStream xmlFileStream = null;
			try
			{
				xmlFileStream = new FileStream(xmlFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
				LoadFromXmlFile(xmlFileStream, getFormCallback);
			}
			finally
			{
				if (xmlFileStream != null)
					xmlFileStream.Close();
			}
		}

		//---------------------------------------------------------------------------
		private void LoadFromXmlFile(FileStream stream, GetFormCallback getFormCallback)
		{
			if (stream == null)
				throw new ArgumentOutOfRangeException();

			if (Containers != null && Containers.Count > 0)
				CloseAllForms();

			try
			{
				XmlDocument xmlDocumentToLoad = new XmlDocument();
				xmlDocumentToLoad.PreserveWhitespace = true;

				xmlDocumentToLoad.Load(stream);

				if (xmlDocumentToLoad.DocumentElement == null)
					return;

				XmlElement dockManagerElement = null;
				if (String.Compare(xmlDocumentToLoad.DocumentElement.Name, XML_DOCK_MANAGER_ELEMENT_TAG) != 0)
				{
					XmlNode dockManagerNode = xmlDocumentToLoad.DocumentElement.SelectSingleNode("descendant::" + XML_DOCK_MANAGER_ELEMENT_TAG);

					if 
						(
							dockManagerNode == null || 
							dockManagerNode.NodeType != XmlNodeType.Element ||
							!dockManagerNode.HasChildNodes
						)
						return;
				
					dockManagerElement = (XmlElement)dockManagerNode;
				}
				else
					dockManagerElement = xmlDocumentToLoad.DocumentElement;
				
				string formatVersion = dockManagerElement.GetAttribute(XML_FORMAT_VERSION_ATTRIBUTE);
				if (formatVersion != XML_DOCK_MANAGER_SYNTAX_VERSION)
					throw new ArgumentException(Strings.InvalidFormatVersion);

				DockLeftFactor = Double.Parse(dockManagerElement.GetAttribute(XML_DOCKLEFT_FACTOR_ATTRIBUTE), NumberFormatInfo.InvariantInfo);
				DockRightFactor = Double.Parse(dockManagerElement.GetAttribute(XML_DOCKRIGHT_FACTOR_ATTRIBUTE), NumberFormatInfo.InvariantInfo);
				DockTopFactor = Double.Parse(dockManagerElement.GetAttribute(XML_DOCKTOP_FACTOR_ATTRIBUTE), NumberFormatInfo.InvariantInfo);
				DockBottomFactor = Double.Parse(dockManagerElement.GetAttribute(XML_DOCKBOTTOM_FACTOR_ATTRIBUTE), NumberFormatInfo.InvariantInfo);

				XmlNode formsNode = dockManagerElement.SelectSingleNode("child::" + XML_FORMS_TAG);

				if (formsNode == null || formsNode.NodeType != XmlNodeType.Element)
					throw new ArgumentException(Strings.InvalidXmlFormat);

				XmlElement formsElement = (XmlElement)formsNode;
					
				int formsCount = Convert.ToInt16(formsElement.GetAttribute(XML_COUNT_ATTRIBUTE));
				for (int i=0; i < formsCount; i++)
				{
					XmlNode formNode = formsElement.SelectSingleNode("child::" + XML_FORM_TAG_PREFIX + i.ToString());

					if (formNode == null || formNode.NodeType != XmlNodeType.Element)
						throw new ArgumentException(Strings.InvalidXmlFormat);

					XmlElement formElement = (XmlElement)formNode;

					string persistString = formElement.GetAttribute(XML_PERSIST_STRING_ATTRIBUTE);
					DockableForm aForm = (getFormCallback != null) ? getFormCallback(persistString) : null;
					if (aForm == null)
						continue;

					aForm.DockManager = this;
					aForm.AutoHiddenFactor = Double.Parse(formElement.GetAttribute(XML_AUTOHIDDEN_PERCENTAGE_ATTRIBUTE), NumberFormatInfo.InvariantInfo);
					aForm.IsHidden = Convert.ToBoolean(formElement.GetAttribute(XML_IS_HIDDEN_ATTRIBUTE));
				}

				XmlNode floatingFormsNode = dockManagerElement.SelectSingleNode("child::" + XML_FLOATINGFORMS_TAG);
				
				if (floatingFormsNode == null || floatingFormsNode.NodeType != XmlNodeType.Element)
					throw new ArgumentException(Strings.InvalidXmlFormat);

				XmlElement floatingFormsElement = (XmlElement)floatingFormsNode;
				
				int floatingFormsCount = Convert.ToInt16(floatingFormsElement.GetAttribute(XML_COUNT_ATTRIBUTE));

				RectangleConverter rectConverter = new RectangleConverter();
			
				for (int i=0; i < floatingFormsCount; i++)
				{
					XmlNode floatingFormNode = floatingFormsElement.SelectSingleNode("child::" + XML_FLOATINGFORM_TAG_PREFIX + i.ToString());

					if (floatingFormNode == null || floatingFormNode.NodeType != XmlNodeType.Element)
						throw new ArgumentException(Strings.InvalidXmlFormat);

					XmlElement floatingFormElement = (XmlElement)floatingFormNode;

					Rectangle bounds = Rectangle.Empty;
					string boundsAttribute = floatingFormElement.GetAttribute(XML_BOUNDS_ATTRIBUTE);
					if (boundsAttribute != null && boundsAttribute.Length > 0)
						bounds = (Rectangle)rectConverter.ConvertFromInvariantString(boundsAttribute);
					FloatingForm aFloatingForm = new FloatingForm(this, bounds);
					aFloatingForm.IsRedockingAllowed = Convert.ToBoolean(floatingFormElement.GetAttribute(XML_REDOCKING_ALLOWED_ATTRIBUTE));
				}
		
				XmlNode containersNode = dockManagerElement.SelectSingleNode("child::" + XML_FORM_CONTAINERS_TAG);
				
				if (containersNode == null || containersNode.NodeType != XmlNodeType.Element)
					throw new ArgumentException(Strings.InvalidXmlFormat);

				XmlElement containersElement = (XmlElement)containersNode;
				
				int containersCount = Convert.ToInt16(containersElement.GetAttribute(XML_COUNT_ATTRIBUTE));

				bool[] activatedContainers = new bool[containersCount];
				int[] floatingFormsIndexes = new int[containersCount];

				for (int i=0; i < containersCount; i++)
				{
					XmlNode containerNode = containersElement.SelectSingleNode("child::" + XML_FORM_CONTAINER_TAG_PREFIX + i.ToString());

					if (containerNode == null || containerNode.NodeType != XmlNodeType.Element)
						throw new ArgumentException(Strings.InvalidXmlFormat);

					XmlElement containerElement = (XmlElement)containerNode;

					EnumConverter dockStateConverter = new EnumConverter(typeof(DockState));
					DockState visibleState = (DockState)dockStateConverter.ConvertFrom(containerElement.GetAttribute(XML_VISIBLE_STATE_ATTRIBUTE));
					DockState restoreState = (DockState)dockStateConverter.ConvertFrom(containerElement.GetAttribute(XML_RESTORE_STATE_ATTRIBUTE));
					
					bool isHidden = Convert.ToBoolean(containerElement.GetAttribute(XML_IS_HIDDEN_ATTRIBUTE));
					
					EnumConverter layoutStylesConverter = new EnumConverter(typeof(DockableFormsContainer.LayoutStyles));

					DockableFormsContainer.LayoutStyles dockLayout = (DockableFormsContainer.LayoutStyles)layoutStylesConverter.ConvertFrom(containerElement.GetAttribute(XML_DOCK_LAYOUT_ATTRIBUTE));
					double dockSize = Double.Parse(containerElement.GetAttribute(XML_DOCK_SIZE_ATTRIBUTE), NumberFormatInfo.InvariantInfo);
					
					DockableFormsContainer.LayoutStyles documentLayout = (DockableFormsContainer.LayoutStyles)layoutStylesConverter.ConvertFrom(containerElement.GetAttribute(XML_DOCUMENT_LAYOUT_ATTRIBUTE));
					double documentSize = Double.Parse(containerElement.GetAttribute(XML_DOCUMENT_SIZE_ATTRIBUTE), NumberFormatInfo.InvariantInfo);
					
					DockableFormsContainer.LayoutStyles floatingLayout = (DockableFormsContainer.LayoutStyles)layoutStylesConverter.ConvertFrom(containerElement.GetAttribute(XML_FLOATING_LAYOUT_ATTRIBUTE));
					double floatingSize = Double.Parse(containerElement.GetAttribute(XML_FLOATING_SIZE_ATTRIBUTE), NumberFormatInfo.InvariantInfo);
					
					activatedContainers[i] = Convert.ToBoolean(containerElement.GetAttribute(XML_IS_ACTIVATED_ATTRIBUTE));
					
					floatingFormsIndexes[i] = Convert.ToInt16(containerElement.GetAttribute(XML_CONTAINER_INDEX_IN_FLOATINGFORMS_ATTRIBUTE));
					
					int floatingFormIndex = Convert.ToInt16(containerElement.GetAttribute(XML_FLOATINGFORM_INDEX_ATTRIBUTE));
					FloatingForm aFloatingForm = (floatingFormIndex == -1 ? null : FloatingForms[floatingFormIndex]);
					
					string activeFormAttribute = containerElement.GetAttribute(XML_ACTIVE_FORM_INDEX_ATTRIBUTE);
					int activeFormIndex = (activeFormAttribute != null && activeFormAttribute.Length > 0) ? (int)Convert.ToInt16(activeFormAttribute) : -1;
					DockableForm activeForm = (activeFormIndex >= 0 && activeFormIndex < Forms.Count) ? Forms[activeFormIndex] : null;

					bool allowRedocking = Convert.ToBoolean(containerElement.GetAttribute(XML_REDOCKING_ALLOWED_ATTRIBUTE));

					
					XmlNode containerFormsNode = containerNode.SelectSingleNode("child::" + XML_FORMS_TAG);

					if (containerFormsNode == null || containerFormsNode.NodeType != XmlNodeType.Element)
						throw new ArgumentException(Strings.InvalidXmlFormat);
					
					XmlElement containerFormsElement = (XmlElement)containerFormsNode;
					
					int formsInContainerCount = Convert.ToInt16(containerFormsElement.GetAttribute(XML_COUNT_ATTRIBUTE));
					
					DockableFormsContainer container = null;

					for (int j=0; j < formsInContainerCount; j++)
					{
						XmlNode formNode = containerFormsNode.SelectSingleNode("child::" + XML_FORM_TAG_PREFIX + j.ToString());

						if (formNode == null || formNode.NodeType != XmlNodeType.Element)
							throw new ArgumentException(Strings.InvalidXmlFormat);
					
						XmlElement formElement = (XmlElement)formNode;
						
						int formIndex = Convert.ToInt16(formElement.GetAttribute(XML_CONTAINER_FORM_REFINDEX));
						if (formIndex < 0 || formIndex >= Forms.Count)
							continue;

						DockableForm aForm = Forms[formIndex];
						if (j==0)
						{
							if (visibleState != DockState.Float)
								container = new DockableFormsContainer(aForm, visibleState);
							else
								container = new DockableFormsContainer(aForm, visibleState, aFloatingForm);

							if (IsValidRestoreState(restoreState))
								container.RestoreState = restoreState;

							container.IsHidden = isHidden;
							container.DockLayoutStyle = dockLayout;
							container.DocumentLayoutStyle = documentLayout;
							container.FloatingLayoutStyle = floatingLayout;
							container.DockSize = dockSize;
							container.DocumentSize = documentSize;
							container.FloatingSize = floatingSize;
							container.FloatingForm = aFloatingForm;
							container.IsRedockingAllowed = allowRedocking;
						}
						else
						{
							aForm.FormContainer = container;
						}
					}

					if (aFloatingForm != null)		// May need to adjust the sequence in the float window
					{
						for (int j=0; j<i; j++)
							if (floatingFormsIndexes[i] < floatingFormsIndexes[j])
							{
								aFloatingForm.SetContainerIndex(container, aFloatingForm.Containers.IndexOf(Containers[j]));
								break;
							}
					}

					if (activeForm != null)
						container.ActiveForm = activeForm;
				}

				for (int i=0; i < containersCount; i++)
					if (Containers[i].DockState == DockState.Document && activatedContainers[i])
						Containers[i].Activate();

				for (int i=0; i < containersCount; i++)
					if (Containers[i].DockState != DockState.Document && activatedContainers[i])
						Containers[i].Activate();
					
				for (int i = Forms.Count-1; i>=0; i--)
					if (Forms[i].IsDummy)
						Forms[i].Close();
			}
			catch(Exception exception) 
			{
				Debug.Fail("Exception raised in DockManager.LoadFromXmlFile: " + exception.Message);
			}		
		}

		#endregion
		
		#region Save to xml File
		
		//---------------------------------------------------------------------------
		public bool SaveToXmlFile(string xmlFilename)
		{
			bool setReadOnlyFlag = false;
			FileStream xmlFileStream = null;
			try
			{
				if (File.Exists(xmlFilename) && ((File.GetAttributes(xmlFilename) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly))
				{
					File.SetAttributes(xmlFilename, File.GetAttributes(xmlFilename) & ~FileAttributes.ReadOnly);
					setReadOnlyFlag = true;
				}

				// FileMode.Create specifies that the operating system should create a new file.
				// If the file already exists, it will be opened.
				xmlFileStream = new FileStream(xmlFilename, FileMode.OpenOrCreate);
				
				SaveToXmlFile(xmlFileStream);
				
				return true;
			}
			catch(SecurityException exception)
			{
				// L'utente non possiede i permessi richiesti
				Debug.Fail("DockManager.SaveToXmlFile Error: " + exception.Message);
				return false;
			}
			catch(UnauthorizedAccessException exception)
			{
				// L'utente non possiede le autorizzazioni necessarie per salvare il file
				Debug.Fail("DockManager.SaveToXmlFile Error: " + exception.Message);
				return false;
			}
			catch(ArgumentException exception) 
			{
				// Può essere fallita la File.SetAttributes
				Debug.Fail("DockManager.SaveToXmlFile Error: " + exception.Message);
				return false;
			}	
			finally
			{
				if (xmlFileStream != null)
					xmlFileStream.Close();
				
				if (setReadOnlyFlag)
					File.SetAttributes(xmlFilename, File.GetAttributes(xmlFilename) | FileAttributes.ReadOnly);			
			}
		}

		//---------------------------------------------------------------------------
		private void SaveToXmlFile(FileStream stream)
		{
			if (stream == null)
				return;

			XmlDocument xmlDocumentToSave = new XmlDocument();
			// if the PreserveWhitespace property is set to false, 
			// XmlDocument auto-indents the output.
			xmlDocumentToSave.PreserveWhitespace = false;

			if (stream.Length > 0)
				xmlDocumentToSave.Load(stream);

			string savedfilename = stream.Name;
			stream.Close();

			XmlNode dockManagerNode = null;

			if (xmlDocumentToSave.DocumentElement == null)
			{
				XmlDeclaration xmlDeclaration = xmlDocumentToSave.CreateXmlDeclaration("1.0", "UTF-8", "yes");     
				if (xmlDeclaration != null)
					xmlDocumentToSave.AppendChild(xmlDeclaration);
				
				XmlElement newRoot = xmlDocumentToSave.CreateElement(XML_DOCK_MANAGER_ELEMENT_TAG);

				if (newRoot == null)
					return;
			
				dockManagerNode = xmlDocumentToSave.AppendChild(newRoot); 
			}
			else
			{
				if (String.Compare(xmlDocumentToSave.DocumentElement.Name, XML_DOCK_MANAGER_ELEMENT_TAG) != 0)
				{
					dockManagerNode = xmlDocumentToSave.DocumentElement.SelectSingleNode("descendant::" + XML_DOCK_MANAGER_ELEMENT_TAG);
					if (dockManagerNode == null || dockManagerNode.NodeType != XmlNodeType.Element)
					{
						XmlElement newDockManagerNode = xmlDocumentToSave.CreateElement(XML_DOCK_MANAGER_ELEMENT_TAG);

						if (newDockManagerNode == null)
							return;
			
						dockManagerNode = xmlDocumentToSave.DocumentElement.AppendChild(newDockManagerNode); 
					}
				}
				else
					dockManagerNode = xmlDocumentToSave.DocumentElement;
			}

			if (dockManagerNode == null || dockManagerNode.NodeType != XmlNodeType.Element)
				return;
			
			dockManagerNode.RemoveAll();
				
			XmlElement dockManagerElement = (XmlElement)dockManagerNode;
			
			dockManagerElement.SetAttribute(XML_FORMAT_VERSION_ATTRIBUTE, XML_DOCK_MANAGER_SYNTAX_VERSION);	
			dockManagerElement.SetAttribute(XML_DOCKLEFT_FACTOR_ATTRIBUTE, DockLeftFactor.ToString(NumberFormatInfo.InvariantInfo));
			dockManagerElement.SetAttribute(XML_DOCKRIGHT_FACTOR_ATTRIBUTE, DockRightFactor.ToString(NumberFormatInfo.InvariantInfo));
			dockManagerElement.SetAttribute(XML_DOCKTOP_FACTOR_ATTRIBUTE, DockTopFactor.ToString(NumberFormatInfo.InvariantInfo));
			dockManagerElement.SetAttribute(XML_DOCKBOTTOM_FACTOR_ATTRIBUTE, DockBottomFactor.ToString(NumberFormatInfo.InvariantInfo));
			
			// Dockable Forms
			XmlElement formsElement = xmlDocumentToSave.CreateElement(XML_FORMS_TAG);
			int formsCount = 0;
			foreach (DockableForm aForm in Forms)
			{
				XmlElement formElement = xmlDocumentToSave.CreateElement(XML_FORM_TAG_PREFIX + formsCount.ToString());

				formElement.SetAttribute(XML_PERSIST_STRING_ATTRIBUTE, aForm.PersistString);
				formElement.SetAttribute(XML_AUTOHIDDEN_PERCENTAGE_ATTRIBUTE, aForm.AutoHiddenFactor.ToString(NumberFormatInfo.InvariantInfo));
				formElement.SetAttribute(XML_IS_HIDDEN_ATTRIBUTE, aForm.IsHidden.ToString());
				
				formsElement.AppendChild(formElement);

				formsCount++;
			}
			formsElement.SetAttribute(XML_COUNT_ATTRIBUTE, Forms.Count.ToString());
			
			dockManagerNode.AppendChild(formsElement);

			// FloatingForms
			XmlElement floatingFormsElement = xmlDocumentToSave.CreateElement(XML_FLOATINGFORMS_TAG);
			floatingFormsElement.SetAttribute(XML_COUNT_ATTRIBUTE, FloatingForms.Count.ToString());
			
			RectangleConverter rectConverter = new RectangleConverter();
			foreach (FloatingForm aFloatingForm in FloatingForms)
			{
				XmlElement floatingFormElement = xmlDocumentToSave.CreateElement(XML_FLOATINGFORM_TAG_PREFIX + FloatingForms.IndexOf(aFloatingForm).ToString());

				floatingFormElement.SetAttribute(XML_BOUNDS_ATTRIBUTE, rectConverter.ConvertToInvariantString(aFloatingForm.Bounds));
				floatingFormElement.SetAttribute(XML_REDOCKING_ALLOWED_ATTRIBUTE, aFloatingForm.IsRedockingAllowed.ToString());
				
				floatingFormsElement.AppendChild(floatingFormElement);
			}
			
			dockManagerNode.AppendChild(floatingFormsElement);

			// Containers
			XmlElement containersElement = xmlDocumentToSave.CreateElement(XML_FORM_CONTAINERS_TAG);
			containersElement.SetAttribute(XML_COUNT_ATTRIBUTE, Containers.Count.ToString());
			
			foreach (DockableFormsContainer container in Containers)
			{
				XmlElement containerElement = xmlDocumentToSave.CreateElement(XML_FORM_CONTAINER_TAG_PREFIX + Containers.IndexOf(container).ToString());

				containerElement.SetAttribute(XML_VISIBLE_STATE_ATTRIBUTE, container.VisibleState.ToString());
				containerElement.SetAttribute(XML_RESTORE_STATE_ATTRIBUTE, container.RestoreState.ToString());
				containerElement.SetAttribute(XML_IS_HIDDEN_ATTRIBUTE, container.IsHidden.ToString());
				containerElement.SetAttribute(XML_DOCK_LAYOUT_ATTRIBUTE, container.DockLayoutStyle.ToString());
				containerElement.SetAttribute(XML_DOCK_SIZE_ATTRIBUTE, container.DockSize.ToString(NumberFormatInfo.InvariantInfo));
				containerElement.SetAttribute(XML_DOCUMENT_LAYOUT_ATTRIBUTE, container.DocumentLayoutStyle.ToString());
				containerElement.SetAttribute(XML_DOCUMENT_SIZE_ATTRIBUTE, container.DocumentSize.ToString(NumberFormatInfo.InvariantInfo));
				containerElement.SetAttribute(XML_FLOATING_LAYOUT_ATTRIBUTE, container.FloatingLayoutStyle.ToString());
				containerElement.SetAttribute(XML_FLOATING_SIZE_ATTRIBUTE, container.FloatingSize.ToString(NumberFormatInfo.InvariantInfo));
				containerElement.SetAttribute(XML_IS_ACTIVATED_ATTRIBUTE, container.IsActivated.ToString());
				containerElement.SetAttribute(XML_ACTIVE_FORM_INDEX_ATTRIBUTE, Forms.IndexOf(container.ActiveForm).ToString());
				containerElement.SetAttribute(XML_CONTAINER_INDEX_IN_FLOATINGFORMS_ATTRIBUTE, (container.FloatingForm != null) ? container.FloatingForm.Containers.IndexOf(container).ToString() : "-1");
				containerElement.SetAttribute(XML_FLOATINGFORM_INDEX_ATTRIBUTE, FloatingForms.IndexOf(container.FloatingForm).ToString());
				containerElement.SetAttribute(XML_REDOCKING_ALLOWED_ATTRIBUTE, container.IsRedockingAllowed.ToString());
				
				XmlElement containerFormsElement = xmlDocumentToSave.CreateElement(XML_FORMS_TAG);

				int containerFormsCount = 0;
				foreach (DockableForm aForm in container.Forms)
				{
					XmlElement formElement = xmlDocumentToSave.CreateElement(XML_FORM_TAG_PREFIX + containerFormsCount.ToString());
					formElement.SetAttribute(XML_CONTAINER_FORM_REFINDEX, Forms.IndexOf(aForm).ToString());

					containerFormsElement.AppendChild(formElement);

					containerFormsCount++;
				}
				containerFormsElement.SetAttribute(XML_COUNT_ATTRIBUTE, containerFormsCount.ToString());
				containerElement.AppendChild(containerFormsElement);

				containersElement.AppendChild(containerElement);
			}
			
			dockManagerNode.AppendChild(containersElement);

			xmlDocumentToSave.Save(savedfilename);
		}

		#endregion

		#region Drag & Drop Classes

		#region DragDrawHelper Class

		//===========================================================================
		/// <summary>
		/// Summary description for DrawHelper.
		/// </summary>
		internal class DragDrawHelper
		{
			private static IntPtr halfToneBrush = IntPtr.Zero;

			#region DragDrawHelper public static methods

			//---------------------------------------------------------------------------
			public static Region CreateDragFrame(Rectangle rect, int indent)
			{
				return CreateRectangleDragFrame(rect, indent);
			}

			//---------------------------------------------------------------------------
			public static Region CreateDragFrame(Rectangle[] rects, int indent)
			{
				Region region = CreateRectangleDragFrame(rects[0], indent);

				for (int i=1; i < rects.Length; i++)
				{
					Region newRegion = CreateRectangleDragFrame(rects[i], indent);

					region.Xor(newRegion);
					
					newRegion.Dispose();
				}

				return region;
			}

			//---------------------------------------------------------------------------
			public static void DrawDragFrame(Region region)
			{
				if (region == null)
					return;

				// Get hold of the DC for the desktop
				IntPtr hDC = User32.GetDC(IntPtr.Zero);

				// Define the area we are allowed to draw into
				IntPtr hRegion = region.GetHrgn(Graphics.FromHdc(hDC)); //<--- Changed
				Gdi32.SelectClipRgn(hDC, hRegion); //<--- Changed

				Win32.RECT rectBox = new Win32.RECT();

				// Get the smallest rectangle that encloses region
				Gdi32.GetClipBox(hDC, ref rectBox);

				IntPtr brushHandler = GetHalfToneBrush();

				// Select brush into the device context
				IntPtr oldHandle = Gdi32.SelectObject(hDC, brushHandler);

				// Blit to screen using provided pattern brush and invert with existing screen contents
				Gdi32.PatBlt(hDC, 
					rectBox.left, 
					rectBox.top, 
					rectBox.right - rectBox.left, 
					rectBox.bottom - rectBox.top, 
					(uint)Win32.RasterOperations.PATINVERT);

				// Put old handle back again
				Gdi32.SelectObject(hDC, oldHandle);

				// Reset the clipping region
				Gdi32.SelectClipRgn(hDC, IntPtr.Zero);

				Gdi32.DeleteObject(hRegion); //<--- Add to release the handle

				// Must remember to release the HDC resource!
				User32.ReleaseDC(IntPtr.Zero, hDC);
			}

			#endregion

			#region DragDrawHelper private static methods

			//---------------------------------------------------------------------------
			private static Region CreateRectangleDragFrame(Rectangle rect, int indent)
			{
				// Create region for whole of the new rectangle
				Region region = new Region(rect);

				// If the rectangle is to small to make an inner object from, then just use the outer
				if ((indent <= 0) || (rect.Width <= 2 * indent) || (rect.Height <= 2 * indent))
					return region;

				rect.X += indent;
				rect.Y += indent;
				rect.Width -= 2 * indent;
				rect.Height -= 2 * indent;

				region.Xor(rect);

				return region;
			}

			//---------------------------------------------------------------------------
			private static IntPtr GetHalfToneBrush()
			{
				if (halfToneBrush == IntPtr.Zero)
				{	
					Bitmap bitmap = new Bitmap(8, 8, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

					Color white = Color.FromArgb(255,255,255,255);
					Color black = Color.FromArgb(255,0,0,0);

					bool flag=true;

					// Alternate black and white pixels across all lines
					for(int x=0; x<8; x++, flag = !flag)
						for(int y=0; y<8; y++, flag = !flag)
							bitmap.SetPixel(x, y, (flag ? white : black));

					IntPtr hBitmap = bitmap.GetHbitmap();

					Win32.LOGBRUSH brush = new Win32.LOGBRUSH();

					brush.lbStyle = (uint)Win32.BrushStyles.BS_PATTERN;
					brush.lbHatch = (uint)hBitmap;

					halfToneBrush = Gdi32.CreateBrushIndirect(ref brush);
				}

				return halfToneBrush;
			}
			
			#endregion
		}
		
		#endregion
		
		#region DropTarget Class

		//================================================================================
		internal class DropTarget
		{
			private Control		dropToControl = null;
			private DockStyle	dockStyle = DockStyle.None;
			private int			containerIndex = -1;

			private Control		oldDropToControl = null;
			private DockStyle	oldDockStyle = DockStyle.None;
			private int			oldContainerIndex = -1;

			//---------------------------------------------------------------------------
			public DropTarget()
			{
				Clear();
			}

			//---------------------------------------------------------------------------
			public Control DropToControl { get { return dropToControl; } }

			//---------------------------------------------------------------------------
			public DockStyle DockStyle { get { return dockStyle; } }

			//---------------------------------------------------------------------------
			public int ContainerIndex { get { return containerIndex; } }

			//---------------------------------------------------------------------------
			public bool SameAsOldValue
			{
				get	
				{
					return 
						(
						dropToControl != null && 
						dropToControl == oldDropToControl && 
						dockStyle == oldDockStyle && 
						containerIndex == oldContainerIndex
						);	
				}
			}

			//---------------------------------------------------------------------------
			public void Clear()
			{
				Clear(false);
			}

			//---------------------------------------------------------------------------
			public void Clear(bool saveOldValue)
			{
				if (saveOldValue)
				{
					oldDropToControl = dropToControl;
					oldDockStyle = dockStyle;
					oldContainerIndex = containerIndex;
				}
				else
				{
					oldDropToControl = null;
					oldDockStyle = DockStyle.None;
					oldContainerIndex = -1;
				}

				dropToControl = null;
				dockStyle = DockStyle.None;
				containerIndex = -1;
			}

			//---------------------------------------------------------------------------
			public void SetDropTarget(Control aControl, DockStyle dock, int aIndex)
			{
				dropToControl = aControl;
				dockStyle = dock;
				containerIndex = aIndex;
			}

			//---------------------------------------------------------------------------
			public void SetDropTarget(TabbedPanel tabbedPanel, DockStyle dock)
			{
				SetDropTarget(tabbedPanel, dock, -1);
			}

			//---------------------------------------------------------------------------
			public void SetDropTarget(TabbedPanel tabbedPanel, int aIndex)
			{
				SetDropTarget(tabbedPanel, DockStyle.Fill, -1);
			}

			//---------------------------------------------------------------------------
			public void SetDropTarget(DockManager dockManager, DockStyle dock)
			{
				SetDropTarget(dockManager, dock, -1);
			}
		}

		#endregion

		#region DragHandlerBase Class

		//================================================================================
		internal class DragHandlerBase : IMessageFilter
		{
			#region DragHandlerBase private fields

			private Control					dragControl = null;
			private IntPtr					saveFocus = IntPtr.Zero;
			private Point					startingMousePosition = Point.Empty;
			private IntPtr					hWnd;
			private User32.WndProcCallBack	windowCallbackProcedure;
			private IntPtr					prevWndFunc;

			#endregion

			#region DragHandlerBase constructor

			//---------------------------------------------------------------------------
			public DragHandlerBase()
			{
				windowCallbackProcedure = new User32.WndProcCallBack(this.WndProc);
			}

			#endregion

			#region DragHandlerBase protected properties

			//---------------------------------------------------------------------------
			protected Point StartingMousePosition { get { return startingMousePosition; }}

			#endregion

			#region DragHandlerBase public properties

			//---------------------------------------------------------------------------
			public Control DragControl { get { return dragControl; } }

			#endregion

			#region DragHandlerBase private methods

			//---------------------------------------------------------------------------
			private IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr wParam, IntPtr lParam)
			{
				if (iMsg == (int)Win32.Msgs.WM_CANCELMODE || iMsg == (int)Win32.Msgs.WM_CAPTURECHANGED)
					EndDrag(true);

				return User32.CallWindowProc(prevWndFunc, hWnd, iMsg, wParam, lParam);
			}

			//---------------------------------------------------------------------------
			private void AssignHandle(IntPtr aWindowHandle)
			{
				hWnd = aWindowHandle;
				prevWndFunc = User32.SetWindowLong(hWnd, -4, windowCallbackProcedure);	// GWL_WNDPROC = -4
			}

			//---------------------------------------------------------------------------
			private void ReleaseHandle()
			{
				if (hWnd != IntPtr.Zero)
					User32.SetWindowLong(hWnd, -4, prevWndFunc);	// GWL_WNDPROC = -4
				hWnd = IntPtr.Zero;
				prevWndFunc = IntPtr.Zero;
			}

			//---------------------------------------------------------------------------
			private void EndDrag(bool abort)
			{
				ReleaseHandle();
				Application.RemoveMessageFilter(this);
				dragControl.FindForm().Capture = false;
				User32.SetFocus(saveFocus);
				saveFocus = IntPtr.Zero;

				OnEndDrag(abort);

				dragControl = null;
			}

			#endregion

			#region DragHandlerBase protected methods

			//---------------------------------------------------------------------------
			protected bool BeginDrag(Control aControl)
			{
				// Avoid re-entrance;
				if (dragControl != null)
					return false;

				startingMousePosition = Control.MousePosition;

				if (!User32.DragDetect(aControl.Handle, StartingMousePosition))
					return false;

				dragControl = aControl;
				saveFocus = User32.GetFocus();
				aControl.FindForm().Capture = true;
				aControl.FindForm().Focus();
				AssignHandle(aControl.FindForm().Handle);
				Application.AddMessageFilter(this);
				return true;
			}

			//---------------------------------------------------------------------------
			protected virtual void OnDragging(){}

			//---------------------------------------------------------------------------
			protected virtual void OnEndDrag(bool abort){}

			#endregion

			// PreFilterMessage filters out a message before it is dispatched to a control
			// or form.  
			// in this case this method is used to perform code work that must be done
			// before the message is dispatched.
			//---------------------------------------------------------------------------
			bool IMessageFilter.PreFilterMessage(ref Message m)
			{
				if (m.Msg == (int)Win32.Msgs.WM_MOUSEMOVE)
					OnDragging();
				else if (m.Msg == (int)Win32.Msgs.WM_LBUTTONUP)
					EndDrag(false);
				else if (m.Msg == (int)Win32.Msgs.WM_CAPTURECHANGED)
					EndDrag(true);
				else if (m.Msg == (int)Win32.Msgs.WM_KEYDOWN && (int)m.WParam == (int)Keys.Escape)
					EndDrag(true);

				return true;
			}
		}
		
		#endregion
		
		#region DragHandler Class

		//==============================================================================
		/// <summary>
		/// Summary description for DragHandler.
		/// </summary>
		internal class DragHandler : DragHandlerBase
		{
			//==========================================================================
			internal enum SourceType
			{
				DockableForm,
				DockableFormsContainer,
				FloatingForm,
				TabbedPanelSplitter
			}

			#region DragHandler private fields

			private SourceType	dragSourceType;
			private Region		dragFrameRegion = null;
			private DropTarget	dropTarget = null;
			private Point		currentSplitterLocation;
			private Point		mouseOffset = Point.Empty;
			private DockManager dockManager = null;

			private const int DragBorderWidth = MeasureContainer.DragSize;

			#endregion

			#region DragHandler constructor

			//---------------------------------------------------------------------------
			public DragHandler(DockManager aDockManager)
			{
				dropTarget = new DropTarget();
				dockManager = aDockManager;
			}

			#endregion

			#region DragHandler public properties

			//---------------------------------------------------------------------------
			public DropTarget DropTarget { get { return dropTarget; } }
			//---------------------------------------------------------------------------
			public SourceType DragSourceType { get { return dragSourceType; } }

			//---------------------------------------------------------------------------
			public Region DragFrame
			{
				get	{ return dragFrameRegion; }
				set
				{
					if (dragFrameRegion != null)
					{
						DockManager.DragDrawHelper.DrawDragFrame(dragFrameRegion);
						dragFrameRegion.Dispose();
					}

					dragFrameRegion = value;
					if (dragFrameRegion != null)
						DockManager.DragDrawHelper.DrawDragFrame(dragFrameRegion);
				}
			}

			#endregion
		
			#region DragHandler protected overridden methods

			//---------------------------------------------------------------------------
			protected override void OnDragging()
			{
				DropTarget.Clear(true);
			
				if (dragSourceType == SourceType.DockableFormsContainer)
					OnDraggingContainer();
				else if (dragSourceType == SourceType.DockableForm)
					OnDraggingForm();
				else if (dragSourceType == SourceType.TabbedPanelSplitter)
					OnDraggingTabbedPanelSplitter();
				else if (dragSourceType == SourceType.FloatingForm)
					OnDraggingFloatingForm();
			}

			//---------------------------------------------------------------------------
			protected override void OnEndDrag(bool abort)
			{
				DragFrame = null;

				if (dragSourceType == SourceType.DockableFormsContainer)
					OnEndDragContainer(abort);
				else if (dragSourceType == SourceType.DockableForm)
					OnEndDragForm(abort);
				else if (dragSourceType == SourceType.TabbedPanelSplitter)
					OnEndDragTabbedPanelSplitter(abort);
				else if (dragSourceType == SourceType.FloatingForm)
					OnEndDragFloatingForm(abort);
			}

			#endregion
		
			#region DragHandler private methods

			//---------------------------------------------------------------------------
			private bool InitDrag(Control aControl, SourceType dragSource)
			{
				if (!base.BeginDrag(aControl))
					return false;

				dragSourceType = dragSource;
				DropTarget.Clear();
				return true;
			}

			//---------------------------------------------------------------------------
			private void BeginDragContainer(Point captionLocation)
			{
				Point pt = captionLocation;
				pt = DragControl.PointToScreen(pt);

				mouseOffset.X = pt.X - StartingMousePosition.X;
				mouseOffset.Y = pt.Y - StartingMousePosition.Y;
			}

			//---------------------------------------------------------------------------
			private void OnDraggingContainer()
			{
				Point ptMouse = Control.MousePosition;
				DockableFormsContainer aContainer = ((TabbedPanel)DragControl).FormContainer;

				if (!TestDrop(ptMouse))
					return;

				if (DropTarget.DropToControl == null && IsDockStateValid(DockState.Float))
				{
					Point location = new Point(ptMouse.X + mouseOffset.X, ptMouse.Y + mouseOffset.Y);
					Size size;
					if (aContainer.FloatingForm != null && aContainer.FloatingForm.Containers.Count == 1)
						size = ((TabbedPanel)DragControl).FormContainer.FloatingForm.Size;
					else
						size = aContainer.FloatingFormDefaultSize;

					if (ptMouse.X > location.X + size.Width)
						location.X += ptMouse.X - (location.X + size.Width) + DragBorderWidth;

					DragFrame = DockManager.DragDrawHelper.CreateDragFrame(new Rectangle(location, size), DragBorderWidth);
				}
				else if (DropTarget.DropToControl == null && !IsDockStateValid(DockState.Float))
					DragFrame = null;

				if (DragFrame == null)
					User32.SetCursor(System.Windows.Forms.Cursors.No.Handle);
				else
					User32.SetCursor(DragControl.Cursor.Handle);
			}

			//---------------------------------------------------------------------------
			private void OnEndDragContainer(bool abort)
			{
				User32.SetCursor(DragControl.Cursor.Handle);

				if (abort)
					return;

				DockableFormsContainer aContainer = ((TabbedPanel)DragControl).FormContainer;

				if (DropTarget.DropToControl is TabbedPanel)
				{
					TabbedPanel tabbedPanel = DropTarget.DropToControl as TabbedPanel;

					if (DropTarget.DockStyle == DockStyle.Fill)
					{
						for (int i= aContainer.Forms.Count - 1; i >= 0; i--)
						{
							DockableForm aForm = aContainer.Forms[i];
							aForm.FormContainer = tabbedPanel.FormContainer;
							if (DropTarget.ContainerIndex != -1)
								tabbedPanel.FormContainer.SetFormIndex(aForm, DropTarget.ContainerIndex);
							aForm.Activate();
						}
					}
					else
					{
						if (DropTarget.DockStyle == DockStyle.Left)
							tabbedPanel.InsertContainerBefore(aContainer, DockableFormsContainer.LayoutStyles.Horizontal, 0.5);
						else if (DropTarget.DockStyle == DockStyle.Right) 
							tabbedPanel.InsertContainerAfter(aContainer, DockableFormsContainer.LayoutStyles.Horizontal, 0.5);
						else if (DropTarget.DockStyle == DockStyle.Top)
							tabbedPanel.InsertContainerBefore(aContainer, DockableFormsContainer.LayoutStyles.Vertical, 0.5);
						else if (DropTarget.DockStyle == DockStyle.Bottom) 
							tabbedPanel.InsertContainerAfter(aContainer, DockableFormsContainer.LayoutStyles.Vertical, 0.5);

						aContainer.Activate();
					}
				}
				else if (DropTarget.DropToControl is DockManager)
				{
					DockState dockStateToSet = DockState.Unknown;

					if (DropTarget.DockStyle == DockStyle.Top)
						dockStateToSet = DockState.DockTop;
					else if (DropTarget.DockStyle == DockStyle.Bottom)
						dockStateToSet = DockState.DockBottom;
					else if (DropTarget.DockStyle == DockStyle.Left)
						dockStateToSet = DockState.DockLeft;
					else if (DropTarget.DockStyle == DockStyle.Right)
						dockStateToSet = DockState.DockRight;
					else if (DropTarget.DockStyle == DockStyle.Fill)
						dockStateToSet = DockState.Document;

					if (aContainer.IsDockStateValid(dockStateToSet))
						aContainer.VisibleState = dockStateToSet;
					
					aContainer.DockManager.SetContainerIndex(aContainer, 0);
					aContainer.Activate();
				}
				else if (IsDockStateValid(DockState.Float))
				{
					Point ptMouse = Control.MousePosition;

					Point location = new Point(ptMouse.X + mouseOffset.X, ptMouse.Y + mouseOffset.Y);
					Size size;
					bool createFloatWindow = true;
					if (aContainer.FloatingForm != null && aContainer.FloatingForm.Containers.Count == 1)
					{
						size = ((TabbedPanel)DragControl).FormContainer.FloatingForm.Size;
						createFloatWindow = false;
					}
					else
						size = aContainer.FloatingFormDefaultSize;

					if (ptMouse.X > location.X + size.Width)
						location.X += ptMouse.X - (location.X + size.Width) + DragBorderWidth;

					if (createFloatWindow)
						aContainer.FloatingForm = new FloatingForm(aContainer.DockManager, aContainer, location);
					else
						aContainer.FloatingForm.Bounds = new Rectangle(location, size);

					aContainer.VisibleState = DockState.Float;
					aContainer.Activate();
				}
			}

			//---------------------------------------------------------------------------
			private void BeginDragForm(Rectangle rectTabWindow)
			{
				TabbedPanel tabbedPanel = (TabbedPanel)DragControl;

				Point pt;
				if (tabbedPanel is DockableDocumentPanel)
					pt = new Point(rectTabWindow.Top, rectTabWindow.Left);
				else
					pt = new Point(rectTabWindow.Left, rectTabWindow.Bottom);

				pt = DragControl.PointToScreen(pt);

				mouseOffset.X = pt.X - StartingMousePosition.X;
				mouseOffset.Y = pt.Y - StartingMousePosition.Y;
			}

			//---------------------------------------------------------------------------
			private void OnDraggingForm()
			{
				Point ptMouse = Control.MousePosition;
				DockableFormsContainer aContainer = ((TabbedPanel)DragControl).FormContainer;

				if (!TestDrop(ptMouse))
					return;

				if (DropTarget.DropToControl == null && IsDockStateValid(DockState.Float))
				{
					Size size = aContainer.FloatingFormDefaultSize;
					Point location;
					if (aContainer.DockState == DockState.Document)
						location = new Point(ptMouse.X + mouseOffset.X, ptMouse.Y + mouseOffset.Y);
					else
						location = new Point(ptMouse.X + mouseOffset.X, ptMouse.Y + mouseOffset.Y - size.Height);

					if (ptMouse.X > location.X + size.Width)
						location.X += ptMouse.X - (location.X + size.Width) + DragBorderWidth;

					DragFrame = DockManager.DragDrawHelper.CreateDragFrame(new Rectangle(location, size), DragBorderWidth);
				}
				else if (DropTarget.DropToControl == null && !IsDockStateValid(DockState.Float))
					DragFrame = null;

				if (DragFrame == null)
					User32.SetCursor(System.Windows.Forms.Cursors.No.Handle);
				else
					User32.SetCursor(DragControl.Cursor.Handle);
			}
		
			//---------------------------------------------------------------------------
			private void OnEndDragForm(bool abort)
			{
				User32.SetCursor(DragControl.Cursor.Handle);

				if (abort)
					return;

				DockableForm aForm = ((TabbedPanel)DragControl).ActiveForm;

				if (DropTarget.DropToControl is TabbedPanel)
				{
					TabbedPanel tabbedPanel = DropTarget.DropToControl as TabbedPanel;

					if (DropTarget.DockStyle == DockStyle.Fill)
					{
						bool sameFormContainer = (aForm.FormContainer == tabbedPanel.FormContainer);
						if (!sameFormContainer)
							aForm.FormContainer = tabbedPanel.FormContainer;

						if (DropTarget.ContainerIndex == -1 || !sameFormContainer)
							tabbedPanel.FormContainer.SetFormIndex(aForm, DropTarget.ContainerIndex);
						else
						{
							DockableFormsCollection aFormsList = aForm.FormContainer.Forms;
							int oldIndex = aFormsList.IndexOf(aForm);
							int newIndex = DropTarget.ContainerIndex;
							if (oldIndex < newIndex)
							{
								newIndex += 1;
								if (newIndex > aFormsList.Count -1)
									newIndex = -1;
							}
							tabbedPanel.FormContainer.SetFormIndex(aForm, newIndex);
						}

						aForm.Activate();
					}
					else
					{
						DockableFormsContainer aContainer = new DockableFormsContainer(aForm, tabbedPanel.DockState);
						if (!tabbedPanel.FormContainer.IsDisposed)
						{
							if (DropTarget.DockStyle == DockStyle.Left)
								tabbedPanel.InsertContainerBefore(aContainer, DockableFormsContainer.LayoutStyles.Horizontal, 0.5);
							else if (DropTarget.DockStyle == DockStyle.Right) 
								tabbedPanel.InsertContainerAfter(aContainer, DockableFormsContainer.LayoutStyles.Horizontal, 0.5);
							else if (DropTarget.DockStyle == DockStyle.Top)
								tabbedPanel.InsertContainerBefore(aContainer, DockableFormsContainer.LayoutStyles.Vertical, 0.5);
							else if (DropTarget.DockStyle == DockStyle.Bottom) 
								tabbedPanel.InsertContainerAfter(aContainer, DockableFormsContainer.LayoutStyles.Vertical, 0.5);
						}

						aContainer.Activate();
					}
				}
				else if (DropTarget.DropToControl is DockManager)
				{
					DockableFormsContainer aContainer;
					if (DropTarget.DockStyle == DockStyle.Top)
						aContainer = new DockableFormsContainer(aForm, DockState.DockTop);
					else if (DropTarget.DockStyle == DockStyle.Bottom)
						aContainer = new DockableFormsContainer(aForm, DockState.DockBottom);
					else if (DropTarget.DockStyle == DockStyle.Left)
						aContainer = new DockableFormsContainer(aForm, DockState.DockLeft);
					else if (DropTarget.DockStyle == DockStyle.Right)
						aContainer = new DockableFormsContainer(aForm, DockState.DockRight);
					else if (DropTarget.DockStyle == DockStyle.Fill)
						aContainer = new DockableFormsContainer(aForm, DockState.Document);
					else
						return;
					
					aContainer.DockManager.SetContainerIndex(aContainer, 0);
					aContainer.Activate();
				}
				else if (IsDockStateValid(DockState.Float))
				{
					Point ptMouse = Control.MousePosition;
					DockableFormsContainer aContainer = aForm.FormContainer;

					Size size = aForm.FloatingFormDefaultSize;
					Point location;
					if (aContainer.DockState == DockState.Document)
						location = new Point(ptMouse.X + mouseOffset.X, ptMouse.Y + mouseOffset.Y);
					else
						location = new Point(ptMouse.X + mouseOffset.X, ptMouse.Y + mouseOffset.Y - size.Height);

					if (ptMouse.X > location.X + size.Width)
						location.X += ptMouse.X - (location.X + size.Width) + DragBorderWidth;

					aContainer = new DockableFormsContainer(aForm, new Rectangle(location, size));
					aContainer.Activate();
				}
			}

			//---------------------------------------------------------------------------
			private void BeginDragTabbedPanelSplitter(Point ptSplitter)
			{
				currentSplitterLocation = ptSplitter;
				currentSplitterLocation = DragControl.PointToScreen(currentSplitterLocation);
				Point ptMouse = StartingMousePosition;

				mouseOffset.X = currentSplitterLocation.X - ptMouse.X;
				mouseOffset.Y = currentSplitterLocation.Y - ptMouse.Y;

				Rectangle rect = TabWindowSplitter_GetDragRectangle();
				DragFrame = DockManager.DragDrawHelper.CreateDragFrame(rect, DragBorderWidth);
			}

			//---------------------------------------------------------------------------
			private void OnDraggingTabbedPanelSplitter()
			{
				Rectangle rect = TabWindowSplitter_GetDragRectangle();
				DragFrame = DockManager.DragDrawHelper.CreateDragFrame(rect, DragBorderWidth);
			}
	
			//---------------------------------------------------------------------------
			private void OnEndDragTabbedPanelSplitter(bool abort)
			{
				if (abort)
					return;

				Point pt = currentSplitterLocation;
				Rectangle rect = TabWindowSplitter_GetDragRectangle();
				TabbedPanel tabbedPanel = DragControl as TabbedPanel;
				DockManager dockManager = tabbedPanel.DockManager;
				DockState state = tabbedPanel.DockState;
				if (tabbedPanel.IsTopLevel)
				{
					Rectangle rectDockArea = dockManager.DockArea;
					DockableForm aForm = dockManager.ActiveAutoHideForm;
					if (state == DockState.DockLeft && rectDockArea.Width > 0)
						dockManager.DockLeftFactor += ((double)rect.X - (double)pt.X) / (double)rectDockArea.Width;
					else if (state == DockState.DockRight && rectDockArea.Width > 0)
						dockManager.DockRightFactor += ((double)pt.X - (double)rect.X) / (double)rectDockArea.Width;
					else if (state == DockState.DockBottom && rectDockArea.Height > 0)
						dockManager.DockBottomFactor += ((double)pt.Y - (double)rect.Y) / (double)rectDockArea.Height;
					else if (state == DockState.DockTop && rectDockArea.Height > 0)
						dockManager.DockTopFactor += ((double)rect.Y - (double)pt.Y) / (double)rectDockArea.Height;
					else if (state == DockState.DockLeftAutoHide && rectDockArea.Width > 0)
						aForm.AutoHiddenFactor += ((double)rect.X - (double)pt.X) / (double)rectDockArea.Width;
					else if (state == DockState.DockRightAutoHide && rectDockArea.Width > 0)
						aForm.AutoHiddenFactor += ((double)pt.X - (double)rect.X) / (double)rectDockArea.Width;
					else if (state == DockState.DockBottomAutoHide && rectDockArea.Height > 0)
						aForm.AutoHiddenFactor += ((double)pt.Y - (double)rect.Y) / (double)rectDockArea.Height;
					else if (state == DockState.DockTopAutoHide && rectDockArea.Height > 0)
						aForm.AutoHiddenFactor += ((double)rect.Y - (double)pt.Y) / (double)rectDockArea.Height;
				}
				else
				{
					TabbedPanel tabWindowPrev = tabbedPanel.Previous;
					double sizeAdjust;
					if (tabWindowPrev.LayoutStyle == DockableFormsContainer.LayoutStyles.Horizontal && tabWindowPrev.ClientRectangle.Width > 0)
						sizeAdjust = ((double)rect.X - (double)pt.X) / (double)tabWindowPrev.ClientRectangle.Width;
					else
						sizeAdjust = ((double)rect.Y - (double)pt.Y) / (double)tabWindowPrev.ClientRectangle.Height;

					tabWindowPrev.LayoutSize += sizeAdjust;
					tabWindowPrev.Invalidate();
					tabWindowPrev.PerformLayout();
				}
			}
	
			//---------------------------------------------------------------------------
			private Rectangle TabWindowSplitter_GetDragRectangle()
			{
				TabbedPanel tabbedPanel = DragControl as TabbedPanel;
				DockManager dockManager = tabbedPanel.DockManager;

				DockStyle splitterDockStyle = tabbedPanel.SplitterDockStyle;
				Rectangle rectParentClient;
				if (tabbedPanel.IsTopLevel)
				{
					rectParentClient = dockManager.ClientRectangle;
					if (splitterDockStyle == DockStyle.Left || splitterDockStyle == DockStyle.Right)
					{
						rectParentClient.X += dockManager.DockPadding.Left + MeasureContainer.MinSize;
						rectParentClient.Width -= dockManager.DockPadding.Left + dockManager.DockPadding.Right + 2 * MeasureContainer.MinSize;
					}
					else if (splitterDockStyle == DockStyle.Top || splitterDockStyle == DockStyle.Bottom)
					{
						rectParentClient.Y += dockManager.DockPadding.Top + MeasureContainer.MinSize;
						rectParentClient.Height -= dockManager.DockPadding.Top + dockManager.DockPadding.Bottom + 2 * MeasureContainer.MinSize;
					}
					rectParentClient.Location = dockManager.PointToScreen(rectParentClient.Location);
				}
				else
				{
					TabbedPanel tabWindowPrev = tabbedPanel.Previous;
					rectParentClient = tabWindowPrev.ClientRectangle;
					rectParentClient.Location = tabWindowPrev.PointToScreen(rectParentClient.Location);
					if (tabWindowPrev.LayoutStyle == DockableFormsContainer.LayoutStyles.Horizontal)
					{
						rectParentClient.X += MeasureContainer.MinSize;
						rectParentClient.Width -= 2 * MeasureContainer.MinSize;
					}
					else
					{
						rectParentClient.Y += MeasureContainer.MinSize;
						rectParentClient.Height -= 2 * MeasureContainer.MinSize;
					}
				}

				Point ptMouse = Control.MousePosition;
				Point pt = currentSplitterLocation;
				Rectangle rect = Rectangle.Empty;
				if (splitterDockStyle == DockStyle.Left || splitterDockStyle == DockStyle.Right)
				{
					rect.X = ptMouse.X + mouseOffset.X;
					rect.Y = pt.Y;
					rect.Width = DragBorderWidth;
					rect.Height = DragControl.ClientRectangle.Height;
				}
				else if (splitterDockStyle == DockStyle.Top || splitterDockStyle == DockStyle.Bottom)
				{
					rect.X = pt.X;
					rect.Y = ptMouse.Y + mouseOffset.Y;
					rect.Width = DragControl.ClientRectangle.Width;
					rect.Height = DragBorderWidth;
				}

				if (rectParentClient.Width <= 0 || rectParentClient.Height <= 0)
				{
					rect.X = pt.X;
					rect.Y = pt.Y;
					return rect;
				}

				if (rect.Left < rectParentClient.Left)
					rect.X = rectParentClient.X;
				if (rect.Top < rectParentClient.Top)
					rect.Y = rectParentClient.Y;
				if (rect.Right > rectParentClient.Right)
					rect.X -= rect.Right - rectParentClient.Right;
				if (rect.Bottom > rectParentClient.Bottom)
					rect.Y -= rect.Bottom - rectParentClient.Bottom;

				return rect;
			}

			//---------------------------------------------------------------------------
			private void BeginDragFloatingForm()
			{
				mouseOffset.X = DragControl.Bounds.X - StartingMousePosition.X;
				mouseOffset.Y = DragControl.Bounds.Y - StartingMousePosition.Y;
			}

			//---------------------------------------------------------------------------
			private void OnDraggingFloatingForm()
			{
				Point ptMouse = Control.MousePosition;

				if (!TestDrop(ptMouse))
					return;

				if (DropTarget.DropToControl == null)
				{
					Rectangle rect = DragControl.Bounds;
					rect.X = ptMouse.X + mouseOffset.X;
					rect.Y = ptMouse.Y + mouseOffset.Y;
					DragFrame = DockManager.DragDrawHelper.CreateDragFrame(rect, DragBorderWidth);
				}
			}

			//---------------------------------------------------------------------------
			private void OnEndDragFloatingForm(bool abort)
			{
				if (abort)
					return;

				FloatingForm floatingForm = (FloatingForm)DragControl;

				if (DropTarget.DropToControl == null)
				{
					Rectangle rect = DragControl.Bounds;
					rect.X = Control.MousePosition.X + mouseOffset.X;
					rect.Y = Control.MousePosition.Y + mouseOffset.Y;
					DragControl.Bounds = rect;
				}
				else if (DropTarget.DropToControl is TabbedPanel)
				{
					TabbedPanel tabbedPanel = (TabbedPanel)DropTarget.DropToControl;

					if (DropTarget.DockStyle == DockStyle.Fill)
					{
						for (int i = floatingForm.Containers.Count-1; i >= 0; i--)
						{
							DockableFormsContainer aContainer = floatingForm.Containers[i];
							for (int j = aContainer.Forms.Count - 1; j >= 0; j--)
							{
								DockableForm aForm = aContainer.Forms[j];
								aForm.FormContainer = tabbedPanel.FormContainer;
								if (DropTarget.ContainerIndex != -1)
									tabbedPanel.FormContainer.SetFormIndex(aForm, DropTarget.ContainerIndex);
								aForm.Activate();
							}
						}
					}
					else
					{
						TabbedPanel lastTabWindow = null;
						for (int i = floatingForm.Containers.Count-1; i>=0; i--)
						{
							DockableFormsContainer aContainer = floatingForm.Containers[i];
							if (i == floatingForm.Containers.Count-1 || aContainer.TabbedPanel == null)
							{
								if (DropTarget.DockStyle == DockStyle.Left)
									tabbedPanel.InsertContainerBefore(aContainer, DockableFormsContainer.LayoutStyles.Horizontal, 0.5);
								else if (DropTarget.DockStyle == DockStyle.Right) 
									tabbedPanel.InsertContainerAfter(aContainer, DockableFormsContainer.LayoutStyles.Horizontal, 0.5);
								else if (DropTarget.DockStyle == DockStyle.Top)
									tabbedPanel.InsertContainerBefore(aContainer, DockableFormsContainer.LayoutStyles.Vertical, 0.5);
								else if (DropTarget.DockStyle == DockStyle.Bottom) 
									tabbedPanel.InsertContainerAfter(aContainer, DockableFormsContainer.LayoutStyles.Vertical, 0.5);
							}
							else if (lastTabWindow != null)
								lastTabWindow.InsertContainerBefore(aContainer, aContainer.TabbedPanel.LayoutStyle, aContainer.TabbedPanel.LayoutSize);

							lastTabWindow = aContainer.TabbedPanel;
						}
					}
				}
				else if (DropTarget.DropToControl is DockManager)
				{
					for (int i = floatingForm.Containers.Count - 1; i>=0; i--)
					{
						DockableFormsContainer aContainer = floatingForm.Containers[i];
						
						DockState dockStateToSet = DockState.Unknown;

						if (DropTarget.DockStyle == DockStyle.Top)
							dockStateToSet = DockState.DockTop;
						else if (DropTarget.DockStyle == DockStyle.Bottom)
							dockStateToSet = DockState.DockBottom;
						else if (DropTarget.DockStyle == DockStyle.Left)
							dockStateToSet = DockState.DockLeft;
						else if (DropTarget.DockStyle == DockStyle.Right)
							dockStateToSet = DockState.DockRight;
						else if (DropTarget.DockStyle == DockStyle.Fill)
							dockStateToSet = DockState.Document;

						if (aContainer.IsDockStateValid(dockStateToSet))
							aContainer.VisibleState = dockStateToSet;

						if (aContainer.DockManager != null)
							aContainer.DockManager.SetContainerIndex(aContainer, 0);
						aContainer.DockLayoutStyle = aContainer.FloatingLayoutStyle;
						aContainer.DockSize = aContainer.FloatingSize;
						floatingForm.Containers[i].Activate();
					}
				}
			}

			//---------------------------------------------------------------------------
			private TabbedPanel TabbedPanelAtPoint(Point pt)
			{
				Control control;

				for (control = ControlAtPoint(pt); control != null; control = control.Parent)
				{
					if (control is TabbedPanel)
						return control as TabbedPanel;
				}

				return null;
			}

			//---------------------------------------------------------------------------
			private DockManager DockManagerAtPoint(Point pt)
			{
				return ControlAtPoint(pt) as DockManager;
			}

			//---------------------------------------------------------------------------
			private Control ControlAtPoint(Point pt)
			{
				Win32.POINT pt32;
				pt32.x = pt.X;
				pt32.y = pt.Y;

				return Control.FromChildHandle(User32.WindowFromPoint(pt32));
			}

			//---------------------------------------------------------------------------
			private bool TestDrop(Point ptMouse)
			{
				DockManager dropDockManager = DockManagerAtPoint(ptMouse);

				if (dropDockManager != null)
				{
					if (dropDockManager != dockManager)
						return false;
					dropDockManager.TestDrop(this, ptMouse);
				}

				if (DropTarget.DropToControl == null)
				{
					TabbedPanel dropTabbedPanel = TabbedPanelAtPoint(ptMouse);
					if (dropTabbedPanel != null)
						dropTabbedPanel.TestDrop(this, ptMouse);
				}

				return (!DropTarget.SameAsOldValue);
			}
		
			#endregion
		
			#region DragHandler public methods

			//---------------------------------------------------------------------------
			public void BeginDragContainer(TabbedPanel tabbedPanel, Point captionLocation)
			{
				if (!InitDrag(tabbedPanel, SourceType.DockableFormsContainer))
					return;

				BeginDragContainer(captionLocation);
			}

			//---------------------------------------------------------------------------
			public void BeginDragForm(TabbedPanel tabbedPanel, Rectangle rectTabWindow)
			{
				if (!InitDrag(tabbedPanel, SourceType.DockableForm))
					return;

				BeginDragForm(rectTabWindow);
			}

			//---------------------------------------------------------------------------
			public void BeginDragTabbedPanelSplitter(TabbedPanel tabbedPanel, Point splitterLocation)
			{
				if (!InitDrag(tabbedPanel, SourceType.TabbedPanelSplitter))
					return;

				BeginDragTabbedPanelSplitter(splitterLocation);
			}

			//---------------------------------------------------------------------------
			public void BeginDragFloatingForm(FloatingForm aFloatingForm)
			{
				if (!InitDrag(aFloatingForm, SourceType.FloatingForm))
					return;

				BeginDragFloatingForm();
			}

			//---------------------------------------------------------------------------
			public bool IsDockStateValid(DockState dockState)
			{
				if (DragSourceType == SourceType.FloatingForm)
					return ((FloatingForm)DragControl).IsDockStateValid(dockState);
				else if (DragSourceType == SourceType.DockableFormsContainer)
					return ((TabbedPanel)DragControl).FormContainer.IsDockStateValid(dockState);
				else if (DragSourceType == SourceType.DockableForm)
					return ((TabbedPanel)DragControl).ActiveForm.IsDockStateValid(dockState);
				else
					return false;
			}

			#endregion

			#endregion

		}
		
		
		#endregion
	}

	#region DockableFormEventArgs Class

	//===========================================================================
	public class DockableFormEventArgs : EventArgs
	{
		public DockableForm form = null;

		//---------------------------------------------------------------------------
		public DockableFormEventArgs(DockableForm aForm)
		{
			form = aForm;
		}

		//---------------------------------------------------------------------------
		public DockableForm Form { get { return form; } }
	}

	#endregion

}