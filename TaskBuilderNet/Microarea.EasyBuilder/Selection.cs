using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Interfaces.View;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	class Selections
	{
		List<Selection> list = new List<Selection>();
		FormEditor editor;
		private readonly int distanceFromStaticArea = 2; //pixel
        private WindowWrapperContainer parent = null;

		internal enum Action { None, AlignTop, AlignBottom, AlignLeft, AlignRight, SameSize, SameWidth, SameHeight, CenterHorizontally, CenterVertically, DistributeHorizontally, DistributeVertically, AlignToStaticArea }
		//--------------------------------------------------------------------------------
		public Selections(FormEditor editor)
		{
			this.editor = editor;
		}

        //----------------------------------------------------------------------------------
        public WindowWrapperContainer Parent
        {
            get
            {
                return parent;
            }
            set 
            {
                if (parent == null)
                    parent = value;
            }
        }

		//--------------------------------------------------------------------------------
		public FormEditor Editor { get { return editor; } }
		//--------------------------------------------------------------------------------
		public IEnumerable<Selection> Items { get { return list; } }
		//--------------------------------------------------------------------------------
		public int Count { get { return list.Count; } }
		//--------------------------------------------------------------------------------
		internal IWindowWrapper MainSelectedWindow
		{
			get
			{
				Selection sel = MainSelection;
				return sel == null ? null : sel.GetCurrentWindow();
			}
		}
		//--------------------------------------------------------------------------------
		internal Selection MainSelection
		{
			get
			{
				if (list.Count == 0)
					return null;
				return list[0];
			}
		}

		//--------------------------------------------------------------------------------
		internal Selection GetSelection(IWindowWrapper windowWrapper)
		{
			if (windowWrapper == null)
				return null;

			foreach (Selection sel in list)
				if (sel.Handle == windowWrapper.Handle)
					return sel;
			return null;
		}
		//--------------------------------------------------------------------------------
		internal void RemoveSelection(IWindowWrapper windowWrapper)
		{
			if (windowWrapper == null)
				return;

			foreach (Selection sel in list)
				if (sel.Handle == windowWrapper.Handle)
				{
					sel.HighlightingRectangle = Rectangle.Empty;//cancello il rettangolo disegnato attorno alla selezione
					sel.Dispose();
					list.Remove(sel);
					return;
				}
		}

		//--------------------------------------------------------------------------------
		internal IWindowWrapper GetWindow(Point screenPoint)
		{
			IWindowWrapper child;
			foreach (Selection sel in list)
				if ((child = sel.GetWindow(screenPoint)) != null)
					return child;
			return null;
		}

		//--------------------------------------------------------------------------------
		public IWindowWrapper[] Components
		{
			get
			{
				IWindowWrapper[] ar = new IWindowWrapper[list.Count];
				for (int i = 0; i < list.Count; i++)
					ar[i] = list[i].GetCurrentWindow();
				return ar;
			}
		}

		//--------------------------------------------------------------------------------
		internal bool AmIWorkingOnSelectedControl(IWindowWrapper child, Point screenPoint)
		{
			foreach (Selection sel in list)
				if (sel.AmIWorkingOnSelectedControl(child, screenPoint))
					return true;
			return false;
		}


		//--------------------------------------------------------------------------------
		internal EditingMode GetAcceptableEditingMode(Point screenPoint)
		{
			EditingMode mode = EditingMode.All;
			EditingMode requestedMode = EditingMode.None;
			foreach (Selection sel in list)
			{
				//la tipologia di azione richiesta è determinata dall'oggetto attivo
				if (sel.IsActive)
				{
					requestedMode = sel.GetEditingMode(screenPoint);
					mode &= requestedMode;
				}
                IWindowWrapper current = sel.GetCurrentWindow();
                if (current != null)
				    mode &= current.DesignerMovable;
			}
			return mode == requestedMode ? mode : EditingMode.None;
		}


		//--------------------------------------------------------------------------------
		internal void ClearSelections()
		{
			foreach (Selection sel in list)
			{
				sel.HighlightingRectangle = Rectangle.Empty;//cancello il rettangolo disegnato attorno alla selezione
				sel.Dispose();
			}
			list.Clear();
            //pulisci il parent
            parent = null;
		}

		//--------------------------------------------------------------------------------
		internal void AlignHighlightingRectangleToCurrentWindow()
		{
			foreach (Selection sel in list)
				sel.AlignHighlightingRectangleToCurrentWindow();
		}

		//--------------------------------------------------------------------------------
		internal void DoPaint(Graphics g, EditingMode editingMode)
		{
			foreach (Selection sel in list)
				sel.DoPaint(g, editingMode);
		}

		//--------------------------------------------------------------------------------
		internal void KeyMove(EditingMode editingMode, int xOffset, int yOffset)
		{
			Point[] screenPoints = new Point[list.Count];
			for (int i = 0; i < list.Count; i++)
				screenPoints[i] = list[i].KeyMoveInit(editingMode, xOffset, yOffset);

			Rectangle mainHighlightingRectangle = MainSelection.HighlightingRectangle;
			for (int i = 0; i < list.Count; i++)
				list[i].DoResizeOrMove(editingMode, screenPoints[i], mainHighlightingRectangle, true);
		}

		//--------------------------------------------------------------------------------
		internal void DoResizeOrMove(EditingMode editingMode, Point newScreenPosition, bool byKey)
		{
            if (MainSelection == null)
                return;

			Rectangle mainHighlightingRectangle = MainSelection.HighlightingRectangle;
			foreach (Selection sel in list)
				sel.DoResizeOrMove(editingMode, newScreenPosition, mainHighlightingRectangle, byKey);
		}

		//--------------------------------------------------------------------------------
		internal void PerformAction(Action action)
		{
			Selection mainSel = MainSelection;
			if (mainSel == null || list == null)
				return;
			IWindowWrapper active = mainSel.GetCurrentWindow();
			switch (action)
			{
				case Selections.Action.None:
					break;
				case Selections.Action.DistributeHorizontally:
				case Selections.Action.DistributeVertically:
					{
						bool horiz = action == Selections.Action.DistributeHorizontally;
						if (horiz)	list.Sort(new Comparison<Selection>(this.SortByX));
						else		list.Sort(new Comparison<Selection>(this.SortByY));
						IWindowWrapper parent = active.Parent;
						if (parent == null)
							return;
						int totalSum = 0;
						for (int i = 0; i < list.Count; i++)
						{
							Selection sel = list[i];
							IWindowWrapper w = sel.GetCurrentWindow();
							if (w.Parent != parent)
								return;
							totalSum += (horiz ? w.Size.Width : w.Size.Height);
						}
						int remaining = horiz ? (parent.Size.Width - totalSum) : (parent.Size.Height - totalSum);
						remaining = remaining <= 0 ? 0 : remaining;
						int space = remaining / (list.Count + 1);
						int start = space;
						for (int i = 0; i < list.Count; i++)
						{
							Selection sel = list[i];
							IWindowWrapper w = sel.GetCurrentWindow();
							Point oldLocation = w.Location;
							if (start + (horiz ? w.Size.Width : w.Size.Height) > (horiz ? parent.Size.Width : parent.Size.Height))
								start = (horiz ? parent.Size.Width : parent.Size.Height) - (horiz ? w.Size.Width : w.Size.Height);
							w.Location = horiz ? (new Point(start, w.Location.Y)) : (new Point(w.Location.X, start));
							editor.OnComponentPropertyChanged((IComponent)w, "Location", oldLocation, w.Location);
							sel.AlignHighlightingRectangleToCurrentWindow();
							start += space + (horiz ? w.Size.Width : w.Size.Height);
						}
						break;
					}
				case Selections.Action.CenterHorizontally:
				case Selections.Action.CenterVertically:
					{
						bool horiz = action == Selections.Action.CenterHorizontally;
						if (horiz) list.Sort(new Comparison<Selection>(this.SortByX));
						else list.Sort(new Comparison<Selection>(this.SortByY));
						IWindowWrapper parent = active.Parent;
						if (parent == null)
							return;
						int high = Int32.MaxValue, low = 0;
						for (int i = 0; i < list.Count; i++)
						{
							Selection sel = list[i];
							IWindowWrapper w = sel.GetCurrentWindow();
							if (w.Parent != parent) return;
							int location = horiz ? w.Location.X : w.Location.Y;
							int versus = horiz ? w.Size.Width : w.Size.Height;
							if (location < high) high = location;
							if (location + versus > low) low = location + versus;
						}

						int remaining = (horiz ? parent.Size.Width : parent.Size.Height) - (low - high);
						int translation = (remaining / 2) - high;
						for (int i = 0; i < list.Count; i++)
						{
							Selection sel = list[i];
							IWindowWrapper w = sel.GetCurrentWindow();
							Point oldLocation = w.Location;
							w.Location = horiz ? (new Point(w.Location.X + translation, w.Location.Y)) : (new Point(w.Location.X, w.Location.Y + translation));
							editor.OnComponentPropertyChanged((IComponent)w, "Location", oldLocation, w.Location);
							sel.AlignHighlightingRectangleToCurrentWindow();
						}
						break;
					}
				case Selections.Action.AlignTop:
				case Selections.Action.AlignBottom:
					bool top = action == Selections.Action.AlignTop;
					for (int i = 1; i < list.Count; i++)
					{
						Selection sel = list[i];
						IWindowWrapper w = sel.GetCurrentWindow();
						if (w == active)
							continue;
						editor.OnComponentPropertyChanging((IComponent)w, "Location");
						Point oldLocation = w.Location;
						int y = top ? active.Location.Y : active.Location.Y + active.Size.Height - w.Size.Height;
						w.Location = new Point(w.Location.X, y);
						editor.OnComponentPropertyChanged((IComponent)w, "Location", oldLocation, w.Location);
						sel.AlignHighlightingRectangleToCurrentWindow();
					}
					break;
				case Selections.Action.AlignToStaticArea:
					for (int i = 0; i < list.Count; i++)
					{
						Selection sel = list[i];				
						IWindowWrapper currControl = sel.GetCurrentWindow();
						if (currControl is GenericGroupBox && currControl.Id.Contains("STATIC_AREA"))
							continue;
						int distance = CalculateStaticArea(currControl);
						if (distance == -1)
							continue;
						editor.OnComponentPropertyChanging((IComponent)currControl, "Location");
						Point oldLocation = currControl.Location;
						currControl.Location = new Point(distance, currControl.Location.Y);
						editor.OnComponentPropertyChanged((IComponent)currControl, "Location", oldLocation, currControl.Location);
						sel.AlignHighlightingRectangleToCurrentWindow();
					}
					break;
				case Selections.Action.AlignLeft:
				case Selections.Action.AlignRight:
					bool left = action == Selections.Action.AlignLeft;
					for (int i = 1; i < list.Count; i++)
					{
						Selection sel = list[i];
						IWindowWrapper w = sel.GetCurrentWindow();
						if (w == active )
							continue;
						editor.OnComponentPropertyChanging((IComponent)w, "Location");
						Point oldLocation = w.Location;
						int x = left ? active.Location.X : active.Location.X + active.Size.Width - w.Size.Width;
						w.Location = new Point(x, w.Location.Y);
						editor.OnComponentPropertyChanged((IComponent)w, "Location", oldLocation, w.Location);
						sel.AlignHighlightingRectangleToCurrentWindow();
					}
					break;
				case Selections.Action.SameSize:
				case Selections.Action.SameWidth:
				case Selections.Action.SameHeight:
					for (int i = 1; i < list.Count; i++)
					{
						Selection sel = list[i];
						IWindowWrapper w = sel.GetCurrentWindow();
						if (w == active)
							continue;
						editor.OnComponentPropertyChanging((IComponent)w, "Size");
						Size oldSize = w.Size;

						int x = active.Size.Width, y = active.Size.Height;       //sameSize			
						if (action == Selections.Action.SameWidth)
							y = w.Size.Height;                                  //sameWidth
						else if (action == Selections.Action.SameHeight)
							x = w.Size.Width;                                   //sameHeight
						w.Size = new Size(x, y);

						editor.OnComponentPropertyChanged((IComponent)w, "Size", oldSize, w.Size);
					}
					break;
				default:
					break;
			}
			
		}
		
		//--------------------------------------------------------------------------------
		private int SortByY(Selection x, Selection y)
		{
			return x.GetCurrentWindow().Location.Y.CompareTo(y.GetCurrentWindow().Location.Y);
		}

		//--------------------------------------------------------------------------------
		private int SortByX(Selection x, Selection y)
		{
			return x.GetCurrentWindow().Location.X.CompareTo(y.GetCurrentWindow().Location.X);
		}

		//--------------------------------------------------------------------------------
		private int CalculateStaticArea(IWindowWrapper currControl)
		{
			if (currControl == null || currControl.Parent == null)
				return -1;

			ComponentCollection components = null;
			if (currControl.Parent is MTileDialog)
				components = ((MTileDialog)currControl.Parent).Components;
			else if (currControl.Parent is MTilePanel)
				components = ((MTilePanel)currControl.Parent).Components;
			else if (currControl.Parent is MEasyStudioPanel)
				components = ((MEasyStudioPanel)currControl.Parent).Components;

			if (components == null)
				return -1;

			List<GenericGroupBox> staticAreas = new List<GenericGroupBox>();
			foreach (var item in components)
			{
				GenericGroupBox staticArea = item as GenericGroupBox;
				if (staticArea != null && staticArea.Id.Contains("STATIC_AREA"))
					staticAreas.Add(staticArea);
			}

			if (staticAreas.Count < 1)
				return -1;

			GenericGroupBox e = null;
			int d = currControl.Parent.Size.Width;

			foreach (GenericGroupBox item in staticAreas)
			{
				int currDistance = currControl.Location.X - item.Location.X;
				//scelgo la static area a sinistra del control e con minor distanza dal control stesso
				if (item.Location.X <= currControl.Location.X && currDistance>=0 && d >= currDistance)
				{
					d = currDistance;
					e = item;
				}
			}

			GenericGroupBox sa = e != null ? staticAreas.Find(s => s.Id == e.Id) : staticAreas[0];
			return sa.Location.X + sa.Size.Width + distanceFromStaticArea;
		}

		//--------------------------------------------------------------------------------
		internal void MakeActive(Selection sel)
		{
			if (sel == null || list.Count == 0)
				return;
			if (list[0] != sel)
			{
				list.Remove(sel);
				list.Insert(0, sel);
			}
		}

		//--------------------------------------------------------------------------------
		internal void InitDrag(Point screenPoint)
		{
			foreach (Selection sel in list)
			{
				sel.InitDrag(screenPoint);
				sel.InitAllowedRegion();

			}
		}

		//-------------------------------------------------------------------------------
		internal bool SetCursor(Point p, MouseButtons mb)
		{
			foreach (Selection sel in list)
				if (sel.SetCursor(p, mb))
					return true;
			return false;
		}

		//--------------------------------------------------------------------------------
		internal void Add(IWindowWrapper current, bool clearPreviuos)
		{
			Selection sel = new Selection(editor, current, this);

            if (clearPreviuos)
                parent = null;

            //memorizza il parent
            Parent = sel.GetCurrentWindow().Parent as WindowWrapperContainer;

            //il panel di easystudio non deve essere selezionato con altri controlli
            if (current is MEasyStudioPanel || (list.Count == 1 && (list[0].GetCurrentWindow() is MEasyStudioPanel)))
				clearPreviuos = true;

            if (clearPreviuos)
                list.Clear();
            
            //gestione multiselezione: solo i figli dello stesso container (fratelli)
            if (!clearPreviuos) //multiselezione
                if (Parent != null && (current.Parent as WindowWrapperContainer) != Parent) //figli di padri diversi
                    return;

            //l'ultimo elemento è quello attivo
            list.Insert(0, sel);
		}

	}
	class Selection : IDisposable
	{
		FormEditor editor;
		//-----------------------------------------------------------------------------
		// grippers
		private Rectangle topLeft;
		private Rectangle bottomLeft;
		private Rectangle topRight;
		private Rectangle bottomRight;
		private Rectangle midTop;
		private Rectangle midBottom;
		private Rectangle midLeft;
		private Rectangle midRight;
		private bool isSelectedControlFullyVisible;
		private bool modified = false;
		List<Rectangle> magnetics = new List<Rectangle>();
		//Handle del control attualmente selezionato.
		private IntPtr handle;


		//Parent della finestra correntemente selezionata, serve solo per tradurre le coordinate per scrivere il tooltip durante lo spostamento di un control.
		private WindowWrapperContainer currentWindowParent;
		//Rectangle usato per disegnare il bordino sul controllo selezionato.
		private Rectangle highlightingRectangle;
		//Punto in cui si è afferrato il controllo durante le operazioni di move.
		private Point moveGrabbedPointOffset;
		private Region allowedRegion = null;
		private Rectangle tooltipRectangle = new Rectangle(0, 0, 50, 15);
		private Selections selections;

		public Rectangle TooltipRectangle
		{
			get { return tooltipRectangle; }
		}

		public bool Modified
		{
			get { return modified; }
		}
		public IntPtr Handle
		{
			get { return handle; }

		}
		public bool IsActive { get { return selections.MainSelection == this; } }
		//-----------------------------------------------------------------------------
		public Selection(FormEditor editor, IWindowWrapper wrapper, Selections selections)
		{
			this.editor = editor;
			this.handle = wrapper.Handle;
			this.selections = selections;

			BaseWindowWrapper bww = wrapper as BaseWindowWrapper;
			if (bww != null)
			{
				bww.SizeChanged += new EventHandler<EasyBuilderEventArgs>(SelectedComponent_SizeChanged);
				bww.LocationChanged += new EventHandler<EasyBuilderEventArgs>(SelectedComponent_SizeChanged);
			}
			currentWindowParent = wrapper.Parent as WindowWrapperContainer;
			AlignHighlightingRectangleToCurrentWindow();
		}

		//--------------------------------------------------------------------------------
		void SelectedComponent_SizeChanged(object sender, EasyBuilderEventArgs e)
		{
			e.Handled = false;//non mangio il messaggio per evitare problemi di sizing del parsed control
			BaseWindowWrapper bww = (BaseWindowWrapper)sender;
			if (handle != bww.Handle)
			{
				//se non sono più selezionato, mi deregistro ed esco
				bww.SizeChanged -= new EventHandler<EasyBuilderEventArgs>(SelectedComponent_SizeChanged);
				bww.LocationChanged -= new EventHandler<EasyBuilderEventArgs>(SelectedComponent_SizeChanged);
				return;
			}
			//altrimenti ricalcolo le dimensioni del rettangolo di selezione
			AlignHighlightingRectangleToCurrentWindow();
		}
		//--------------------------------------------------------------------------------
		internal void InitAllowedRegion()
		{
			if (allowedRegion != null)
				allowedRegion.Dispose();
			allowedRegion = new Region(CurrentParentScreenRectangle);
			if (currentWindowParent != null)
			{
				WindowWrapperContainer container = null;
				foreach (IComponent item in currentWindowParent.Components)
				{
					container = item as WindowWrapperContainer;
					if (container == null || container.Handle == handle)//posso sovrappormi a me stesso.
						continue;

					allowedRegion.Exclude(container.Rectangle);
				}
			}
		}
		/// <summary>
		/// Gets or sets the rectanlge to higlight the current selected component.
		/// </summary>
		//-----------------------------------------------------------------------------
		public Rectangle HighlightingRectangle
		{
			get { return highlightingRectangle; }
			set
			{
				if (highlightingRectangle == value)
					return;

				editor.Invalidate(true, true, false, highlightingRectangle, tooltipRectangle);
				highlightingRectangle = value;
				editor.Invalidate(true, false, true, highlightingRectangle, tooltipRectangle);
			}
		}

		//--------------------------------------------------------------------------------
		private bool IsNewRectangleFullyVisible(Rectangle newRectangle)
		{
			//Un rettangolo è completamente visibile se è interamente contenuto dal suo parent (non ne eccede i bordi)
			//e se non è sovrapposto ad un suo fratello che è container.
			return CurrentParentScreenRectangle.Contains(newRectangle) &&
				IntersectSiblingThatIsAContainer(newRectangle) == Rectangle.Empty;
		}


		//--------------------------------------------------------------------------------
		private void DrawHandle(Graphics graphics, Brush brush, Point p, ref Rectangle handleRect)
		{
			handleRect.Location = new Point(p.X, p.Y);
			handleRect.Size = new System.Drawing.Size(FormEditor.handleMidSize * 2, FormEditor.handleMidSize * 2);
			graphics.FillRectangle(brush, editor.RectangleToClient(handleRect));
		}
		//--------------------------------------------------------------------------------
		private void DrawTooltip(
			Graphics g,
			Point toolTipLocation,
			string toolTipText
			)
		{
			Rectangle workingToolTipRectangle = tooltipRectangle;
			workingToolTipRectangle.Location = toolTipLocation;

			SizeF tooltipTextSize = g.MeasureString(toolTipText, editor.Font);
			RectangleF rect = new RectangleF(
				new PointF(
					toolTipLocation.X + ((workingToolTipRectangle.Width - tooltipTextSize.Width) / 2),
					toolTipLocation.Y + ((workingToolTipRectangle.Height - tooltipTextSize.Height) / 2)
					),
				tooltipTextSize);
			g.FillRectangle(Brushes.Yellow, rect);
			g.DrawString(
				toolTipText,
				editor.Font,
				Brushes.Black,
				rect
				);
		}
		//--------------------------------------------------------------------------------
		public void DoPaint(Graphics g, EditingMode editingMode)
		{
			Rectangle rectangle = HighlightingRectangle;
			//Se non c'è alcun controllo selezionato allora non disegna il bordino.
			if (rectangle == Rectangle.Empty)
				return;

			//Disegno gli otto handle per il ridimensionamento
			int k = 2 * FormEditor.handleMidSize;
			DrawHandle(g, GetBrush(), new Point(rectangle.Left - k, rectangle.Top - k), ref topLeft);
			DrawHandle(g, GetBrush(), new Point(rectangle.Left - k, rectangle.Bottom), ref bottomLeft);
			DrawHandle(g, GetBrush(), new Point(rectangle.Right, rectangle.Top - k), ref topRight);
			DrawHandle(g, GetBrush(), new Point(rectangle.Right, rectangle.Bottom), ref bottomRight);
			DrawHandle(g, GetBrush(), new Point(rectangle.Left + rectangle.Width / 2, rectangle.Top - k), ref midTop);
			DrawHandle(g, GetBrush(), new Point(rectangle.Left + rectangle.Width / 2, rectangle.Bottom), ref midBottom);
			DrawHandle(g, GetBrush(), new Point(rectangle.Left - k, rectangle.Top + rectangle.Height / 2 - FormEditor.handleMidSize), ref midLeft);
			DrawHandle(g, GetBrush(), new Point(rectangle.Right, rectangle.Top + rectangle.Height / 2 - FormEditor.handleMidSize), ref midRight);
			Rectangle clientCurrRect = editor.RectangleToClient(rectangle);

			//Se sto trascinando l'oggetto e ho premuto il tasto sinistro del mouse
			//allora disegno le linee per facilitare l'allineamento con gli altri controlli.
			if (editor.Capture)
			{
				using (Pen p = new Pen(Brushes.Red))
				{
					p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
					foreach (Rectangle r in magnetics)
						g.DrawRectangle(p, editor.RectangleToClient(r));
				}
			}

			//Disegno del tooltip
			string tooltipText = null;
			Point toolTipLocation = editor.PointToClient(tooltipRectangle.Location);
			switch (editingMode)
			{
				case EditingMode.ResizingTopLeft:
				case EditingMode.ResizingBottomLeft:
				case EditingMode.ResizingTopRight:
				case EditingMode.ResizingBottomRight:
				case EditingMode.ResizingMidTop:
				case EditingMode.ResizingMidBottom:
				case EditingMode.ResizingMidLeft:
				case EditingMode.ResizingMidRight:
					//Tooltip con size.
					Size sz = clientCurrRect.Size;
					currentWindowParent.ToLogicalUnits(ref sz);
					tooltipText = String.Format("{0} x {1}", sz.Width, sz.Height);
					DrawTooltip(g, toolTipLocation, tooltipText);
					break;
				case EditingMode.Moving:
				default:
					//Tooltip con location solo se sposto con mouse
					if (currentWindowParent != null)
					{
						Point clientLocation = rectangle.Location;
						currentWindowParent.ScreenToClient(ref clientLocation);
						currentWindowParent.ToLogicalUnits(ref clientLocation);
						tooltipText = String.Format("{0}; {1}", clientLocation.X, clientLocation.Y);
						DrawTooltip(g, toolTipLocation, tooltipText);
					}
					break;
			}

			//Disegno i due rettangoli concentrici sul bordo del controllo con linee solid.
			g.DrawRectangle(GetPen(), clientCurrRect);
			clientCurrRect.Inflate(FormEditor.handleMidSize, FormEditor.handleMidSize);
			g.DrawRectangle(GetPen(), clientCurrRect);

		}

		private Brush GetBrush()
		{
			return IsActive ? editor.mainForeBrush : editor.foreBrush;
		}

		//-----------------------------------------------------------------------------
		private Pen GetPen()
		{
			return IsActive ? editor.mainForePen : editor.forePen;
		}


		//-----------------------------------------------------------------------------
		internal void AlignHighlightingRectangleToCurrentWindow()
		{
			//Spostiamo il rectangle che disegna il bordino per tenerlo
			//ancorato al controllo sottostante durante il resize della finestra.
			HighlightingRectangle = (handle != IntPtr.Zero)
				? GetWindowRect(GetCurrentWindow())
				: HighlightingRectangle = Rectangle.Empty;
		}
		/// <summary>
		/// Ritorna il rettangolo del controllo selezionato in coordinate FormEditor.
		/// </summary>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		private Rectangle GetWindowRect(IWindowWrapper windowWrapper)
		{
			if (windowWrapper == null)
				return Rectangle.Empty;

			return windowWrapper.Rectangle;
		}

		//--------------------------------------------------------------------------------
		private Rectangle CurrentParentScreenRectangle
		{
			get
			{
				//se la finestra corrente non è cambiata, restituisco il rettangolo della parent
				//if (parentWindowRectangleIdentifier == hCurrentWindow)
				//	return currentParentScreenRectangle;

				//altrimenti lo devo ricalcolare in funzione della nuova finestra
				IWindowWrapper window = GetCurrentWindow();
				if (window != null)
					window = window.Parent;
				return (window != null)
					? window.Rectangle
					: Rectangle.Empty;
			}
		}

		internal IWindowWrapper GetCurrentWindow()
		{
			return editor.GetCurrentWindow(handle);
		}

		internal bool SetCursor(Point p, MouseButtons mb)
		{
			if (topLeft.Contains(p))
			{ editor.Cursor = Cursors.SizeNWSE; return true; }
			else if (bottomRight.Contains(p))
			{ editor.Cursor = Cursors.SizeNWSE; return true; }
			else if (topRight.Contains(p))
			{ editor.Cursor = Cursors.SizeNESW; return true; }
			else if (bottomLeft.Contains(p))
			{ editor.Cursor = Cursors.SizeNESW; return true; }
			else if (midTop.Contains(p))
			{ editor.Cursor = Cursors.SizeNS; return true; }
			else if (midBottom.Contains(p))
			{ editor.Cursor = Cursors.SizeNS; return true; }
			else if (midLeft.Contains(p))
			{ editor.Cursor = Cursors.SizeWE; return true; }
			else if (midRight.Contains(p))
			{ editor.Cursor = Cursors.SizeWE; return true; }
			else if (HighlightingRectangle != Rectangle.Empty)
			{
				if (HighlightingRectangle.Contains(p) && mb == MouseButtons.Left)
				{
					if (editor.IsCopyActive())
						editor.Cursor = Cursors.Cross;
					else
						editor.Cursor = Cursors.SizeAll;
					return true;
				}

			}
			return false;
		}


		internal Point KeyMoveInit(EditingMode editingMode, int xOffset, int yOffset)
		{
			Point screenPosition = Point.Empty;
			Point handleLocation = Point.Empty;
			switch (editingMode)
			{
				case EditingMode.Moving:
					screenPosition = highlightingRectangle.Location;
					//Faccio finta di averlo afferrato sull'angolo in alto a sinistra
					moveGrabbedPointOffset = Point.Empty;
					break;
				case EditingMode.ResizingMidTop:
					handleLocation = midTop.Location;
					screenPosition = new Point(handleLocation.X, handleLocation.Y + 2 * FormEditor.handleMidSize);
					break;
				case EditingMode.ResizingMidBottom:
					screenPosition = midBottom.Location;
					break;
				case EditingMode.ResizingMidLeft:
					handleLocation = midLeft.Location;
					screenPosition = new Point(handleLocation.X + 2 * FormEditor.handleMidSize, handleLocation.Y);
					break;
				case EditingMode.ResizingMidRight:
					screenPosition = midRight.Location;
					break;
				default:
					break;
			}
			screenPosition.Offset(xOffset, yOffset);
			InitAllowedRegion();
			return screenPosition;
		}



		internal IWindowWrapper GetWindow(Point screenPoint)
		{
			if (
				topLeft.Contains(screenPoint) ||
				bottomLeft.Contains(screenPoint) ||
				topRight.Contains(screenPoint) ||
				bottomRight.Contains(screenPoint) ||
				midTop.Contains(screenPoint) ||
				midBottom.Contains(screenPoint) ||
				midLeft.Contains(screenPoint) ||
				midRight.Contains(screenPoint)
				)
			{
				return editor.View.GetControl(handle);

			}

			return null;
		}
		/// <summary>
		/// Se aPoint (che deve essere passato in coordinate client) è
		/// contenuto in uno dei gripper allora sto lavorando sul control selezionato per
		/// ridimensioanrlo.
		/// </summary>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		internal bool AmIWorkingOnSelectedControl(IWindowWrapper child, Point aScreenPoint)
		{
			//Se lavoro sui gripper allora ritorno true.
			//Questo if serve perchè i gripper sono disegnato fuori dell'area del contro e quindi
			//un click su di loro verrebbe interpretato come un click al di fuori dell'area del control
			//anche s ein realtà sto solo cercando, per esempio, di ridimensionarlo.
			if (
				topLeft.Contains(aScreenPoint) ||
				bottomRight.Contains(aScreenPoint) ||
				topRight.Contains(aScreenPoint) ||
				bottomLeft.Contains(aScreenPoint) ||
				midTop.Contains(aScreenPoint) ||
				midBottom.Contains(aScreenPoint) ||
				midLeft.Contains(aScreenPoint) ||
				midRight.Contains(aScreenPoint)
				)
				return true;

			//Se arrivo qui allora non ho fatto click sui gripper e
			//quindi se non ho fatto click dentro al retangolo di selezione 
			//vuol dire che ho cliccato su un altro control rispetto a quello selezionato.
			if (!HighlightingRectangle.Contains(aScreenPoint))
				return false;

			//il tabber e le tab si comportano in modo diverso: se ho cliccato su una linguetta del tabber e il controllo
			//selezionato è il tabber, in questo caso non devo paragonare l'handle del tabber con quello precedente, ma 
			//l'handle della tab selezionata con il tabber
			child = AmIWorkingOnATab(child, aScreenPoint);
			IntPtr candidateSelectedHandle = (child != null) ? child.Handle : IntPtr.Zero;

			//Se sono qui significa che non ho cliccato sui gripper ma ho fatto clic dentro al rettangolo
			//di selezione.
			//Siccome ci possono essere più controlli sovraposti allora devo confrontare l'handle del controllo attualmente
			//selezionato (hCurrentWindow) con l'handle del candidato per la seleizone calcolato:
			//Se coincidono allora sto proprio lavorando sul controllo selezionato, altrimenti sto solo cambiando la selezione.
			return candidateSelectedHandle == handle;
		}

		//--------------------------------------------------------------------------------
		internal EditingMode GetEditingMode(Point screenPoint)
		{
			if (topLeft.Contains(screenPoint))
				return EditingMode.ResizingTopLeft;
			else if (bottomRight.Contains(screenPoint))
				return EditingMode.ResizingBottomRight;
			else if (topRight.Contains(screenPoint))
				return EditingMode.ResizingTopRight;
			else if (bottomLeft.Contains(screenPoint))
				return EditingMode.ResizingBottomLeft;
			else if (midTop.Contains(screenPoint))
				return EditingMode.ResizingMidTop;
			else if (midBottom.Contains(screenPoint))
				return EditingMode.ResizingMidBottom;
			else if (midLeft.Contains(screenPoint))
				return EditingMode.ResizingMidLeft;
			else if (midRight.Contains(screenPoint))
				return EditingMode.ResizingMidRight;
			else if (HighlightingRectangle.Contains(screenPoint))
				return EditingMode.Moving;
			else
				return EditingMode.None;
		}
		//--------------------------------------------------------------------------------
		private Rectangle IntersectSiblingThatIsAContainer(Rectangle newRectangle)
		{
			Rectangle intersectingRectangle = Rectangle.Empty;

			if (currentWindowParent == null)
				return intersectingRectangle;

			WindowWrapperContainer container = null;
			IWindowWrapper aWapper = GetCurrentWindow();
			if (currentWindowParent == null && aWapper != null)
				currentWindowParent = aWapper.Parent as WindowWrapperContainer;

			if (currentWindowParent != null)
				foreach (IComponent item in currentWindowParent.Components)
				{
					container = item as WindowWrapperContainer;
					if (container == null || container.Handle == handle)//posso sovrappormi a me stesso.
						continue;

					Rectangle siblingContainerRectangle = container.Rectangle;
					if (siblingContainerRectangle.IntersectsWith(newRectangle))
					{
						intersectingRectangle = siblingContainerRectangle;
						break;
					}
				}

			return intersectingRectangle;
		}
		/// <summary>
		/// ritorna lo stesso child se ho cliccato su qualcosa di diverso da una tab o la tab desiderata se ho cliccato
		/// specificamente su una linguetta o su una tab
		/// </summary>
		//-----------------------------------------------------------------------------
		private static IWindowWrapper AmIWorkingOnATab(IWindowWrapper child, Point aScreenPoint)
		{
			MTabber tabber = child as MTabber;
			IWindowWrapper newChild = null;
			if (tabber != null)
			{
				MTab tab = tabber.GetTabByPoint(aScreenPoint);
				if (tab != null)
					newChild = tab as IWindowWrapper;
			}

			return (newChild == null) ? child : newChild;
		}

		//--------------------------------------------------------------------------------
		internal void DoResizeOrMove(EditingMode editingMode, Point newScreenPosition, Rectangle originalActiveRectangle, bool byKey)
		{
			//Le Tab sono escluse dal moving.
			IDesignerTarget target = GetCurrentWindow() as IDesignerTarget;
			if (target != null && (target.DesignerMovable == EditingMode.None))
				return;

			if (editingMode == EditingMode.None)
				return;

			Rectangle newRectangle = HighlightingRectangle;
			int deltaX = (originalActiveRectangle.X - newScreenPosition.X);
			int deltaY = (originalActiveRectangle.Y - newScreenPosition.Y);
			int deltaH = -deltaY - originalActiveRectangle.Height;
			int deltaW = -deltaX - originalActiveRectangle.Width;
			switch (editingMode)
			{
				case EditingMode.None:
					break;

				case EditingMode.Moving:
					newScreenPosition.Offset(moveGrabbedPointOffset);
					newRectangle.Location = newScreenPosition;
					// Se un controllo è completamente visibile (cioè non è parzialmente nascosto da un altro control) allora bado
					// a non permettere di spostarlo sopra altri control
					// altrimenti lo lascio spostare e ricalcolo solamente il fatto che sia diventato completamente visibile o meno.
					if (isSelectedControlFullyVisible)
						newRectangle = AdjustNewRectangleForMove(newRectangle);
					else
						isSelectedControlFullyVisible = IsNewRectangleFullyVisible(newRectangle);
					break;

		//senso antiorario partendo da in alto a sx
				case EditingMode.ResizingTopLeft:
					newRectangle.Height =	newRectangle.Height + deltaY;
					newRectangle.Width =	newRectangle.Width + deltaX;
					newRectangle.X =		newRectangle.X - deltaX;
					newRectangle.Y =		newRectangle.Y - deltaY;
					newRectangle =			AdjustNewRectangleForResize(newRectangle);
					break;

				case EditingMode.ResizingMidLeft:
					newRectangle.Width =	newRectangle.Width + deltaX;
					newRectangle.X =		newRectangle.X - deltaX;
					newRectangle =			AdjustNewRectangleForResize(newRectangle);
					break;

				case EditingMode.ResizingBottomLeft:
					newRectangle.Width =	newRectangle.Width + deltaX;
					newRectangle.Height =	newRectangle.Height + deltaH;
					newRectangle.X =		newRectangle.X - deltaX;
					newRectangle =			AdjustNewRectangleForResize(newRectangle);
					break;

				case EditingMode.ResizingMidBottom:
					newRectangle.Height =	newRectangle.Height + deltaH;
					newRectangle =			AdjustNewRectangleForResize(newRectangle, editingMode);
					break;

				case EditingMode.ResizingBottomRight:
					newRectangle.Width =	newRectangle.Width + deltaW;
					newRectangle.Height =	newRectangle.Height + deltaH;
					newRectangle =			AdjustNewRectangleForResize(newRectangle, editingMode);
					break;

				case EditingMode.ResizingMidRight:
					newRectangle.Width =	newRectangle.Width + deltaW;
					newRectangle =			AdjustNewRectangleForResize(newRectangle, editingMode);
				//	newRectangle =			AdjustNewRectangleByMinWidth(newRectangle);
					break;

				case EditingMode.ResizingTopRight:
					newRectangle.Height =	newRectangle.Height + deltaY;
					newRectangle.Width =	newRectangle.Width + deltaW;
					newRectangle.Y =		newRectangle.Y - deltaY;
					newRectangle =			AdjustNewRectangleForResize(newRectangle);
					break;

				case EditingMode.ResizingMidTop:
					newRectangle.Height =	newRectangle.Height + deltaY;
					newRectangle.Y =		newRectangle.Y - deltaY;
					newRectangle =			AdjustNewRectangleForResize(newRectangle);
					break;


				default:
					break;
			}

			modified = modified || !HighlightingRectangle.Equals(newRectangle);
			if (modified)
			{
				//se mi muovo con i tasti, non devo avere fenomeni di ancoraggio, hanno senso 
				//solo per spostamenti col mouse
				if (!byKey)
					CalculateMagnetics(ref newRectangle);
				HighlightingRectangle = newRectangle;
			}

			Point loc = newRectangle.Location;
			//loc.Offset(2, 2);
			tooltipRectangle.Location = loc;
		}

		//--------------------------------------------------------------------------------
		private void CalculateMagnetics(ref Rectangle newRectangle)
		{
			ClearMagnetics();

			if (currentWindowParent == null || !IsActive)
				return;
			const int width = 1;
			const int tolerance = 2;
			foreach (IComponent c in currentWindowParent.Components)
			{
				BaseWindowWrapper w = c as BaseWindowWrapper;
				if (w == null || w == this.GetCurrentWindow())
					continue;
				Rectangle rectangle = w.Rectangle;
				Rectangle border;
				if (rectangle.Left <= newRectangle.Left + tolerance && rectangle.Left >= newRectangle.Left - tolerance)
				{
					border = new Rectangle(rectangle.X - width, Math.Min(rectangle.Y, newRectangle.Y), width,
						Math.Max(Math.Abs(rectangle.Top - newRectangle.Bottom), Math.Abs(newRectangle.Top - rectangle.Bottom)));
					AddMagnetic(border);
					newRectangle.X = rectangle.Left;
				}
				if (rectangle.Right <= newRectangle.Left + tolerance && rectangle.Right >= newRectangle.Left - tolerance)
				{
					border = new Rectangle(rectangle.Right - width, Math.Min(rectangle.Y, newRectangle.Y), width,
						Math.Max(Math.Abs(rectangle.Top - newRectangle.Bottom), Math.Abs(newRectangle.Top - rectangle.Bottom)));
					AddMagnetic(border);
					newRectangle.X = rectangle.Right;
				}
				if (rectangle.Top <= newRectangle.Top + tolerance && rectangle.Top >= newRectangle.Top - tolerance)
				{
					border = new Rectangle(Math.Min(rectangle.X, newRectangle.X), rectangle.Y - width,
						Math.Max(Math.Abs(rectangle.Left - newRectangle.Right), Math.Abs(newRectangle.Left - rectangle.Right)),
						width);
					AddMagnetic(border);
					newRectangle.Y = rectangle.Top;
				}
				if (rectangle.Bottom <= newRectangle.Top + tolerance && rectangle.Bottom >= newRectangle.Top - tolerance)
				{
					border = new Rectangle(Math.Min(rectangle.X, newRectangle.X), rectangle.Bottom - width,
						Math.Max(Math.Abs(rectangle.Left - newRectangle.Right), Math.Abs(newRectangle.Left - rectangle.Right)),
						width);
					AddMagnetic(border);
					newRectangle.Y = rectangle.Bottom;
				}
				if (rectangle.Bottom <= newRectangle.Bottom + tolerance && rectangle.Bottom >= newRectangle.Bottom - tolerance)
				{
					border = new Rectangle(Math.Min(rectangle.X, newRectangle.X), rectangle.Bottom,
						Math.Max(Math.Abs(rectangle.Left - newRectangle.Right), Math.Abs(newRectangle.Left - rectangle.Right)),
						width);
					AddMagnetic(border);
					newRectangle.Y = rectangle.Bottom - newRectangle.Height;
				}
				if (rectangle.Top <= newRectangle.Bottom + tolerance && rectangle.Top >= newRectangle.Bottom - tolerance)
				{
					border = new Rectangle(Math.Min(rectangle.X, newRectangle.X), rectangle.Top,
						Math.Max(Math.Abs(rectangle.Left - newRectangle.Right), Math.Abs(newRectangle.Left - rectangle.Right)),
						width);
					AddMagnetic(border);
					newRectangle.Y = rectangle.Top - newRectangle.Height;
				}
				if (rectangle.Right <= newRectangle.Right + tolerance && rectangle.Right >= newRectangle.Right - tolerance)
				{
					border = new Rectangle(rectangle.Right, Math.Min(rectangle.Y, newRectangle.Y), width,
						Math.Max(Math.Abs(rectangle.Top - newRectangle.Bottom), Math.Abs(newRectangle.Top - rectangle.Bottom)));
					AddMagnetic(border);
					newRectangle.X = rectangle.Right - newRectangle.Width;
				}
				if (rectangle.Left <= newRectangle.Right + tolerance && rectangle.Left >= newRectangle.Right - tolerance)
				{
					border = new Rectangle(rectangle.X, Math.Min(rectangle.Y, newRectangle.Y), width,
						Math.Max(Math.Abs(rectangle.Top - newRectangle.Bottom), Math.Abs(newRectangle.Top - rectangle.Bottom)));
					AddMagnetic(border);
					newRectangle.X = rectangle.Left - newRectangle.Width;
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void ClearMagnetics()
		{
			foreach (Rectangle r in magnetics)
				editor.Invalidate(true, true, false, r, Rectangle.Empty);
			magnetics.Clear();
		}

		//--------------------------------------------------------------------------------
		private void AddMagnetic(Rectangle border)
		{
			editor.Invalidate(true, false, true, border, Rectangle.Empty);
			magnetics.Add(border);
		}

		//Ritorna un rettangolo modificando  il newRectangle perso in ingresso correggendolo
		//in base al fatto che un controllo non deve poter oltrepassare i bordi del suo contenitore
		//e non deve poter essere spostato all'interno di un controllo contenitore suo fratello
		//nella gerarchia dei controlli
		//Es:
		//Se una view contiene una label e un tabber, allora questa label potrà essere spostata liberamente
		//all'interno della view a patto che non esca dai confini della view stessa e che
		//non oltrepassi i confini del tabber suo fratello.
		//Se l'intersezione tra il rettangolo di area ammessa per il movimento
		//e il rettangolo del control è proprio pari al rettangolo del control
		//allora significa che il rettangolo del control è interamente contenuto
		//nel rettangolo di area ammessa, cioè non ho scontrato i bordi del mio
		//contenitore e di miei fratelli contenitori, quindi permetto il movimento.
		//Altrimenti impedisco il movimento.
		private Rectangle AdjustNewRectangleForMove(Rectangle newRectangle)
		{
			using (Graphics g = editor.CreateGraphics())
			using (Region tempAllowedRegion = new Region(allowedRegion.GetRegionData()))
			{
				Rectangle bounds = Rectangle.Ceiling(tempAllowedRegion.GetBounds(g));
				//if (!bounds.Contains(newRectangle)) //non ho spazio per muovermi
				//	return HighlightingRectangle;
				int x = newRectangle.Left;
				int y = newRectangle.Top;
				//sbordo a sinistra: sposto il retangolo a destra
				if (newRectangle.Left < bounds.Left)
					x = bounds.Left;
				//sbordo in alto: sposto il rettangolo in basso
				if (newRectangle.Top < bounds.Top)
					y = bounds.Top;
				//sbordo a destra: sposto il rettangolo a sinistra
				if (newRectangle.Right > bounds.Right)
					x = bounds.Right - newRectangle.Width;
				//sbordo in basso: sposto il rettangolo in alto
				if (newRectangle.Bottom > bounds.Bottom)
					y = bounds.Bottom - newRectangle.Height;

				newRectangle.Location = new Point(x, y);
				return newRectangle;
			}
		}

		//--------------------------------------------------------------------------------
		private Rectangle AdjustNewRectangleForResize(Rectangle newRectangle, EditingMode editingMode = EditingMode.None)
		{
			using (Graphics g = editor.CreateGraphics())
			using (Region tempAllowedRegion = new Region(allowedRegion.GetRegionData()))
			{
				Rectangle bounds = Rectangle.Ceiling(tempAllowedRegion.GetBounds(g));
				int x = newRectangle.Left;
				int y = newRectangle.Top;
				int w = newRectangle.Width;
				int h = newRectangle.Height;
				//sbordo a sinistra: ridimensiono il rettangolo tagliandolo a sinistra
				if (newRectangle.Left < bounds.Left)
				{
					x = bounds.Left;
					w = newRectangle.Right - x;
				}
				//sbordo in alto: ridimensiono il rettangolo tagliandolo in alto
				if (newRectangle.Top < bounds.Top)
				{
					y = bounds.Top;
					h = newRectangle.Bottom - y;
				}
				//sbordo a destra: ridimensiono il rettangolo tagliandolo a destra
				if (newRectangle.Right > bounds.Right)
				{
					w = bounds.Right - x;
				}
				//sbordo in basso: ridimensiono il rettangolo tagliandolo in basso
				if (newRectangle.Bottom > bounds.Bottom)
				{
					h = bounds.Bottom - y;
				}
				MTileDialog child = GetCurrentWindow() as MTileDialog;
				if (child != null)
				{
					switch (editingMode)
					{
						case EditingMode.ResizingBottomRight:
							w = Math.Max(w, child.MinSize.Width);
							h = Math.Max(h, child.MinSize.Height);
							break;
						case EditingMode.ResizingMidRight:
							w = Math.Max(w, child.MinSize.Width);
							break;
						case EditingMode.ResizingMidBottom:
							h = Math.Max(h, child.MinSize.Height);
							break;
						default:
							break;
					}
				}
				return new Rectangle(x, y, w, h);
			}
		}

		//--------------------------------------------------------------------------------
		public void Dispose()
		{
			if (allowedRegion != null)
			{
				allowedRegion.Dispose();
				allowedRegion = null;
			}

		}

		//--------------------------------------------------------------------------------
		internal void InitDrag(Point screenPoint)
		{
			Rectangle rect = HighlightingRectangle;

			isSelectedControlFullyVisible = IsNewRectangleFullyVisible(rect);
			Point currentRectLocation = rect.Location;
			moveGrabbedPointOffset = new Point(currentRectLocation.X - screenPoint.X, currentRectLocation.Y - screenPoint.Y);
			CalculateMagnetics(ref rect);

		}

		//--------------------------------------------------------------------------------
		internal void EndDrag()
		{
			modified = false;
			if (allowedRegion != null)
			{
				allowedRegion.Dispose();
				allowedRegion = null;
			}
			ClearMagnetics();
		}
	}


}
