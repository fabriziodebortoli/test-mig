using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.EasyBuilder
{
	//================================================================================
	internal class ZIndexManager
	{
		public const int CircleRadius = 12;
		public static readonly int CircleDiameter = CircleRadius * 2;
		Point zIndexPoint;
		FormEditor editor;
		bool editing = false;
		EasyBuilderControl overControl = null, startingWindow = null, currentWindow = null;
		bool moving = false;
		
		//--------------------------------------------------------------------------------
		public bool Editing
		{
			get { return editing; }
			set { editing = value; }
		}

		//--------------------------------------------------------------------------------
		public bool ShowArrows { get; set; }
		//--------------------------------------------------------------------------------
		public EasyBuilderControl CurrentWindow
		{
			get { return currentWindow; }
			set
			{
				currentWindow = value;
				startingWindow = currentWindow == null
					? null
					: GetPrevious(currentWindow);
			}
		}

		//--------------------------------------------------------------------------------
		public ZIndexManager(FormEditor editor)
		{
			this.editor = editor;
			ShowArrows = true;
		}

		//--------------------------------------------------------------------------------
		public void DrawZIndexMarkers(Graphics g, IWindowWrapperContainer container)
		{
			using (Pen pen = new Pen(Brushes.Red, 1f))
			{
				pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
				
				List<ZIndexContainer> wrappers = new List<ZIndexContainer>();
				foreach (IWindowWrapper ctrl in container.Components)
				{
                    if (ctrl is MTileManager)
                    {
                        var currentGroup = ((MTileManager)ctrl).CurrentTileGroup;
                        if (currentGroup != null)
                        {
                            DrawZIndexMarkers(g, currentGroup);
                        }
                        continue;
                    }
                    if (ctrl is MTabber)
					{
                        var currentTab = ((MTabber)ctrl).CurrentTab;
                        if (currentTab != null)
                        {
                            DrawZIndexMarkers(g, currentTab);
                        }
						continue;
					}

                    if (ctrl is IWindowWrapperContainer)
					{
						DrawZIndexMarkers(g, (IWindowWrapperContainer)ctrl);
						continue;
					}
					EasyBuilderControl ebc = ctrl as EasyBuilderControl;
					if (ebc == null || !ebc.TabStop)
						continue;
					ZIndexContainer cnt = new ZIndexContainer(ebc, CalculateNewZIndex(ebc));
					wrappers.Add(cnt);
				}
				wrappers.Sort();
				Point ptEnd = Point.Empty, ptStart = Point.Empty;
				ZIndexContainer endingControl = null;
				for (int i = wrappers.Count - 1; i >= 0; i--)
				{
					ZIndexContainer ctrl = wrappers[i];
					BaseWindowWrapper bww = ctrl.Wrapper as BaseWindowWrapper;
					if (bww != null)
					{
						Rectangle r = editor.RectangleToClient(bww.Rectangle);
						ptStart = new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
						if (!ptEnd.IsEmpty)
							DrawMarker(g, ptStart, ptEnd, endingControl.Wrapper == currentWindow, endingControl.Wrapper == overControl, endingControl.ZIndex);
						ptEnd = ptStart;
					}
					endingControl = ctrl;
				}

				ptStart = Point.Ceiling(GetStartPoint(container));

				if (endingControl != null)
					DrawMarker(g, ptStart, ptEnd, endingControl.Wrapper == currentWindow, endingControl.Wrapper == overControl, endingControl.ZIndex);

				Rectangle circleRect = CalculateCircleRect(ptStart);
				DrawRoundRect(g, Pens.Red, circleRect, Brushes.Yellow);
				circleRect.Inflate(-2, -2);
				circleRect.Offset(1, 1);
				g.DrawImage(Resources.Start, circleRect);


				if (!zIndexPoint.IsEmpty)
				{
					PointF ptRealStart = CalculateArrowPoints(startingWindow as BaseWindowWrapper);
					DrawArrow(g, Color.Black, Color.Blue, ptRealStart, zIndexPoint);
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void DrawMarker(Graphics g, Point ptStart, Point ptEnd, bool currentWindow, bool overControl, int zIndex)
		{
			if (ShowArrows && !moving && !ptEnd.IsEmpty)
			{
				PointF pt1, pt2, ptRealStart, ptRealEnd;
				//calcolo l'intersezione fra il cerchietto attorno alla partenza, e la linea che unisce partenza e arrivo (avrò due punti di intersezione)
				FindLineCircleIntersections(ptStart.X, ptStart.Y, CircleRadius, ptStart, ptEnd, out pt1, out pt2);
				//scelgo quello che minimizza la distanza col punto di arrivo
				ptRealStart = Distance2D(pt1, ptEnd) > Distance2D(pt2, ptEnd) ? pt2 : pt1;
				//calcolo l'intersezione fra il cerchietto attorno all'arrivo, e la linea che unisce partenza e arrivo (avrò due punti di intersezione)
				FindLineCircleIntersections(ptEnd.X, ptEnd.Y, CircleRadius, ptStart, ptEnd, out pt1, out pt2);
				//scelgo quello che minimizza la distanza col punto di partenza
				ptRealEnd = Distance2D(pt1, ptStart) > Distance2D(pt2, ptStart) ? pt2 : pt1;
				DrawArrow(
					g,
					Color.Black,
					currentWindow ? Color.Snow : Color.Blue,
					ptRealStart,
					ptRealEnd);
			}
			DrawCircle(g, CalculateCircleRect(ptEnd), zIndex, overControl);
		}

		//--------------------------------------------------------------------------------
		public void InvalidateCircles(IWindowWrapperContainer container)
		{
			foreach (IWindowWrapper ctrl in container.Components)
			{
                if (ctrl is MTileManager)
                {
                    var currentGroup = ((MTileManager)ctrl).CurrentTileGroup;
                    if (currentGroup != null)
                    {
                        InvalidateCircles(currentGroup);
                    }

                    continue;
                }
                if (ctrl is MTabber)
				{
                    var currentTab = ((MTabber)ctrl).CurrentTab;
                    if (currentTab != null)
                    {
                        InvalidateCircles(currentTab);
                    }

                    continue;
				}
				if (ctrl is IWindowWrapperContainer)
				{
					InvalidateCircles((IWindowWrapperContainer)ctrl);
					continue;
				}
				BaseWindowWrapper bww = ctrl as BaseWindowWrapper;
				if (bww == null || !bww.TabStop)
					continue;

				Rectangle r = editor.RectangleToClient(bww.Rectangle);

				Point ptEnd = new Point(r.X + r.Width / 2, r.Y + r.Height / 2);

                Rectangle circleRect = CalculateCircleRect(ptEnd);
                if (bww.Parent != null)
                    bww.Parent.Invalidate(circleRect);
                editor.Invalidate(circleRect);
			}

		}
		//--------------------------------------------------------------------------------
		private EasyBuilderControl GetPrevious(EasyBuilderControl currentWindow)
		{
			foreach (IWindowWrapper ctrl in currentWindow.Parent.Components)
			{
				if (ctrl is IWindowWrapperContainer)
					continue;
				EasyBuilderControl ebc = ctrl as EasyBuilderControl;
				if (ebc == null || !ebc.TabStop)
					continue;
				if (ebc.TabOrder == currentWindow.TabOrder - 1)
					return ebc;
			}
			return null;
		}

		//--------------------------------------------------------------------------------
		private PointF CalculateArrowPoints(BaseWindowWrapper window)
		{
			if (window == null)
				return currentWindow == null ? PointF.Empty : GetStartPoint(currentWindow.Parent);
			Rectangle r = editor.RectangleToClient(window.Rectangle);
			Point ptStart = new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
			PointF pt1, pt2, ptRealStart;
			//calcolo l'intersezione fra il cerchietto attorno alla partenza, e la linea che unisce partenza e arrivo (avrò due punti di intersezione)
			FindLineCircleIntersections(ptStart.X, ptStart.Y, CircleRadius, ptStart, zIndexPoint, out pt1, out pt2);
			//scelgo quello che minimizza la distanza col punto di arrivo
			ptRealStart = Distance2D(pt1, zIndexPoint) > Distance2D(pt2, zIndexPoint) ? pt2 : pt1;

			return ptRealStart;
		}

		//--------------------------------------------------------------------------------
		private PointF GetStartPoint(IWindowWrapperContainer container)
		{
			Point p = editor.PointToClient(container.Rectangle.Location);
			return new Point(p.X + 15, p.Y + 15);
		}

		//--------------------------------------------------------------------------------
		private int CalculateNewZIndex(EasyBuilderControl bww)
		{
			if (overControl == null || overControl == currentWindow)
				return bww.TabOrder;

			int startZIndex = startingWindow == null ? -1 : startingWindow.TabOrder;
			int overControlZIndex = overControl.TabOrder;
			int currentZIndex = bww.TabOrder;
			if (startZIndex < overControlZIndex) //sposto in avanti
			{
				if (bww == overControl)
					return startZIndex + 1;
				if (currentZIndex <= startZIndex)
					return currentZIndex;
				if (currentZIndex < overControlZIndex)
					return currentZIndex + 1;
			}
			else //sposto indietro
			{
				if (bww == overControl)
					return startZIndex;
				if (currentZIndex < overControlZIndex)
					return currentZIndex;
				if (currentZIndex <= startZIndex)
					return currentZIndex - 1;

			}

			return currentZIndex;
		}
		
		//--------------------------------------------------------------------------------
		public void DrawRoundRect(Graphics g, Pen p, RectangleF rect, Brush fillBrush, float radius = 4)
		{
			using (GraphicsPath gp = new GraphicsPath())
			{
				float x = rect.X;
				float y = rect.Y;
				float width = rect.Width;
				float height = rect.Height;
				gp.AddLine(x + radius, y, x + width - (radius * 2), y); // Line
				gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90); // Corner
				gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2)); // Line
				gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90); // Corner
				gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height); // Line
				gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90); // Corner
				gp.AddLine(x, y + height - (radius * 2), x, y + radius); // Line
				gp.AddArc(x, y, radius * 2, radius * 2, 180, 90); // Corner
				gp.CloseFigure();

				if (fillBrush != null)
					g.FillPath(fillBrush, gp);
				g.DrawPath(p, gp);

				
			}
		}
		
		//--------------------------------------------------------------------------------
		private void DrawCircle(Graphics g, Rectangle circleRect, int index, bool overControl)
		{
			using (Font font = new Font("Arial", 10))
			{
				StringFormat format = StringFormat.GenericDefault;
				format.Alignment = StringAlignment.Center;
				format.LineAlignment = StringAlignment.Center;

				DrawRoundRect(g, Pens.Red, circleRect, overControl ? Brushes.Cyan : Brushes.Yellow);
				g.DrawString(index.ToString(), font, overControl ? Brushes.Black : Brushes.Red, circleRect, format);
			}
		}

		//--------------------------------------------------------------------------------
		private Rectangle CalculateCircleRect(Point ptCenter)
		{
			return new Rectangle(ptCenter.X - CircleRadius, ptCenter.Y - CircleRadius, CircleDiameter, CircleDiameter);
		}

		//--------------------------------------------------------------------------------
		private float Distance2D(PointF p1, PointF p2)
		{
			double part1 = Math.Pow((p2.X - p1.X), 2);
			double part2 = Math.Pow((p2.Y - p1.Y), 2);
			double underRadical = part1 + part2;
			return (float)Math.Sqrt(underRadical);
		}

		// Find the points of intersection.
		//--------------------------------------------------------------------------------
		private int FindLineCircleIntersections(float cx, float cy, float radius,
			PointF point1, PointF point2, out PointF intersection1, out PointF intersection2)
		{
			float dx, dy, A, B, C, det, t;

			dx = point2.X - point1.X;
			dy = point2.Y - point1.Y;

			A = dx * dx + dy * dy;
			B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
			C = (point1.X - cx) * (point1.X - cx) + (point1.Y - cy) * (point1.Y - cy) - radius * radius;

			det = B * B - 4 * A * C;
			if ((A <= 0.0000001) || (det < 0))
			{
				// No real solutions.
				intersection1 = new PointF(float.NaN, float.NaN);
				intersection2 = new PointF(float.NaN, float.NaN);
				return 0;
			}
			else if (det == 0)
			{
				// One solution.
				t = -B / (2 * A);
				intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
				intersection2 = new PointF(float.NaN, float.NaN);
				return 1;
			}
			else
			{
				// Two solutions.
				t = (float)((-B + Math.Sqrt(det)) / (2 * A));
				intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
				t = (float)((-B - Math.Sqrt(det)) / (2 * A));
				intersection2 = new PointF(point1.X + t * dx, point1.Y + t * dy);
				return 2;
			}
		}

		//--------------------------------------------------------------------------------
		internal void ProcessMoveAction(Point newScreenPosition, BaseWindowWrapper newOverControl)
		{
			if (currentWindow == null)
				return;
			if (!moving || !(startingWindow is BaseWindowWrapper))
			{
				moving = true;
			}
			PointF ptRealStart = CalculateArrowPoints(startingWindow as BaseWindowWrapper);
			using (GraphicsPath gp = new GraphicsPath())
			{
				AddArrow(gp, ptRealStart, zIndexPoint);

				zIndexPoint = editor.PointToClient(newScreenPosition);
				AddArrow(gp, ptRealStart, zIndexPoint);
				Rectangle r = Rectangle.Ceiling(gp.GetBounds());
				r.Inflate(2, 2);//bordi vari
			}

            if (newOverControl?.Parent != null)
            {
                Rectangle r = editor.RectangleToClient(newOverControl.Parent.Rectangle);
                editor.Invalidate(r);
            }

			if (newOverControl != null &&
				newOverControl != startingWindow &&
				newOverControl.TabStop &&
				newOverControl.Parent == currentWindow.Parent)
			{
				if (this.overControl != newOverControl)
				{
					InvalidateCircles(editor.View);
					this.overControl = newOverControl;
				}
			}
			else
			{
				if (this.overControl != null)
				{
					InvalidateCircles(editor.View);
					this.overControl = null;
				}
			}
		}

		//--------------------------------------------------------------------------------
		private float AddArrow(GraphicsPath gp, PointF ptStart, PointF ptEnd)
		{
			int lineWidth = 2;//meta` della larghezza del corpo freccia
			int arrowWidth = 4;//meta` della larghezza aggiuntiva della freccia (quanto esce dal corpo freccia)
			int arrowLenght = 10;//lunghezza della freccia
			Vector vecLine = new Vector(ptEnd.X - ptStart.X, ptEnd.Y - ptStart.Y);// vettore della direzione della freccia
			double lineAngle = vecLine.Alpha;//angolo della direzione della freccia
			Vector vecOrto = new Vector(-vecLine[1], vecLine[0]);// vettore ortogonale alla direzine della freccia
			double ortoAngle = vecOrto.Alpha;//angolo della perpendicolare

			//calcolo di quanto mi devo spostare da una parte e dall'altra, partendo dal punto di origine,
			//per creare la base del corpo freccia
			SizeF lineOffset = new SizeF((float)Math.Cos(ortoAngle) * lineWidth, (float)Math.Sin(ortoAngle) * lineWidth);
			PointF ptStartLeft = ptStart + lineOffset;
			PointF ptStartRight = ptStart - lineOffset;

			//calcolo quanto mi devo spostare in lunghezza per disegnare il corpo freccia fino alla base del triangolo freccia
			SizeF arrowLenghtOffset = new SizeF((float)Math.Cos(lineAngle) * (vecLine.Length - arrowLenght), (float)Math.Sin(lineAngle) * (vecLine.Length - arrowLenght));
			PointF ptEndLeft = ptStartLeft + arrowLenghtOffset;
			PointF ptEndRight = ptStartRight + arrowLenghtOffset;

			//calcolo quando mi devo spostare da una parte e dall'altra per disegnare la base della freccia
			SizeF arrowWidthOffset = new SizeF((float)Math.Cos(ortoAngle) * arrowWidth, (float)Math.Sin(ortoAngle) * arrowWidth);
			PointF ptStartArrowLeft = ptEndLeft + arrowWidthOffset;
			PointF ptStartArrowRight = ptEndRight - arrowWidthOffset;
			
			gp.AddPolygon(new PointF[] { ptStartLeft, ptEndLeft, ptStartArrowLeft, ptEnd, ptStartArrowRight, ptEndRight, ptStartRight });
			gp.CloseFigure();
			return vecLine.Alpha;
		}

		//--------------------------------------------------------------------------------
		internal void FinalizeMoveAction()
		{
			zIndexPoint = Point.Empty;
			if (this.overControl != null)
			{
				List<ZIndexContainer> wrappers = new List<ZIndexContainer>();

				//ricalcolo tutti gli zindex a seguito del trascinamento
				foreach (IWindowWrapper ctrl in overControl.Parent.Components)
				{
					if (ctrl is IWindowWrapperContainer)
						continue;
					EasyBuilderControl ebc = ctrl as EasyBuilderControl;
					if (ebc == null || !ebc.TabStop)
						continue;
					wrappers.Add(new ZIndexContainer(ebc, CalculateNewZIndex(ebc)));
				}
				//applico i cambiamenti a partire dallo zindex più basso per evitare interferenze reciproche
				wrappers.Sort();
				//non posso modificare subito TabOrder, perché il valore originario è usato per calcolare gli altri nella CalculateNewZIndex
				//quindi metto da parte i nuovi valori e poi li aggiorno in blocco alla fine
				foreach (ZIndexContainer ctrl in wrappers)
				{
					int newIndex = ctrl.ZIndex;
					int oldIndex = ctrl.Wrapper.TabOrder;
					if (oldIndex != newIndex)
					{
						ctrl.Wrapper.TabOrder = ctrl.ZIndex;
						editor.OnComponentPropertyChanged(ctrl.Wrapper, ReflectionUtils.GetPropertyName(() => ctrl.Wrapper.TabOrder), oldIndex, newIndex);
					}
				}

				this.overControl = null;
			}
			if (moving)
			{
				moving = false;
				editor.InvalidateEditor();
			}
		}
		
		//--------------------------------------------------------------------------------
		internal void SetCursor(Point p)
		{
			editor.Cursor = Cursors.Default;
		}

		//--------------------------------------------------------------------------------
		public void DrawArrow(Graphics graphics, Color border, Color fill, PointF ptStart, PointF ptEnd)
		{
			if (ptStart == ptEnd)
				return;

			// setup remaining arrow head points
			using (GraphicsPath gp = new GraphicsPath())
			{
				float alpha = AddArrow(gp, ptStart, ptEnd);
				using (PathGradientBrush brGradient = new PathGradientBrush(gp))
				{
					brGradient.CenterColor = Color.Gray;
					using (Pen p = new Pen(Color.Blue))
					{
						graphics.FillPath(brGradient, gp);
						graphics.DrawPath(p, gp);
					}
				}
			}
			
		}
	}
	//================================================================================
	class ZIndexContainer : IComparable<ZIndexContainer>
	{
		private EasyBuilderControl wrapper;
		private int zIndex;

		//--------------------------------------------------------------------------------
		public EasyBuilderControl Wrapper
		{
			get { return wrapper; }
		}

		//--------------------------------------------------------------------------------
		public int ZIndex
		{
			get { return zIndex; }
		}

		//--------------------------------------------------------------------------------
		public ZIndexContainer(EasyBuilderControl bww, int p)
		{
			this.wrapper = bww;
			this.zIndex = p;
		}

		//--------------------------------------------------------------------------------
		public int CompareTo(ZIndexContainer other)
		{
			return zIndex.CompareTo(other.zIndex);
		}

	}

	//================================================================================
	class ZIndexComparer : IComparer<IComponent>
	{
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Solamente nel caso di Oggetti di view, eseguo l'ordinamento per TabOrder, in tutti gli altri casi, lascia inalterato
		/// </summary>
		public int Compare(IComponent x, IComponent y)
		{
			EasyBuilderControl bwwX = x as EasyBuilderControl;
			EasyBuilderControl bwwY = y as EasyBuilderControl;

			if (bwwX == null || bwwY == null)
				return 0;

			int cmp = bwwX.TabOrder.CompareTo(bwwY.TabOrder);
			if (cmp == 0)
				return bwwX.Name.CompareTo(bwwY.Name);
			return cmp;
		}
	}

	
	/// <summary>
	/// Represents generalised vector
	/// </summary>
	//================================================================================
	[Serializable, DebuggerDisplay("{ToString()}, Len = {Length}")]
	[XmlRoot("Vector", Namespace = "", IsNullable = false)]
	internal class Vector : ICloneable
	{
		#region Private members and properties

		private float m_X;

		/// <summary>
		/// X Coordination of vector
		/// </summary>
		//--------------------------------------------------------------------------------
		public float X
		{
			get { return m_X; }
			set { m_X = value; }
		}

		private float m_Y;

		/// <summary>
		/// Y Coordination of vector
		/// </summary>
		//--------------------------------------------------------------------------------
		public float Y
		{
			get { return m_Y; }
			set { m_Y = value; }
		}

		private float m_Z;

		/// <summary>
		/// Z Coordination of vector
		/// </summary>
		//--------------------------------------------------------------------------------
		public float Z
		{
			get { return m_Z; }
			set { m_Z = value; }
		}

		/// <summary>
		/// Gets the length of vector.
		/// </summary>
		/// <value>The length.</value>
		//--------------------------------------------------------------------------------
		public float Length
		{
			get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); }
		}

		/// <summary>
		/// Gets the angle (in radiands) between x-axis and vector's projection to OXY plane.
		/// </summary>
		/// <value>The angle.</value>
		//--------------------------------------------------------------------------------
		public float Alpha
		{
			get { return (float)Math.Atan2(Y, X); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor. Initiate vector at the (0,0,0) location
		/// </summary>
		//--------------------------------------------------------------------------------
		public Vector()
		{
		}

		/// <summary>
		/// Initiate 2D vector with given parameters
		/// </summary>
		/// <param name="inX">X coordination of vector</param>
		/// <param name="inY">Y coordination of vector</param>
		//--------------------------------------------------------------------------------
		public Vector(float inX, float inY)
		{
			m_X = inX;
			m_Y = inY;
			m_Z = 0;
		}

		/// <summary>
		/// Initiate vector with given parameters
		/// </summary>
		/// <param name="inX">X coordination of vector</param>
		/// <param name="inY">Y coordination of vector</param>
		/// <param name="inZ">Z coordination of vector</param>
		//--------------------------------------------------------------------------------
		public Vector(float inX, float inY, float inZ)
		{
			m_X = inX;
			m_Y = inY;
			m_Z = inZ;
		}

		/// <summary>
		/// Initiate vector with given parameters
		/// </summary>
		/// <param name="coordination">Vector's coordinations as an array</param>
		//--------------------------------------------------------------------------------
		public Vector(float[] coordination)
		{
			m_X = coordination[0];
			m_Y = coordination[1];
			m_Z = coordination[2];
		}

		/// <summary>
		/// Initiate vector with same values as given Vector
		/// </summary>
		/// <param name="vector">Vector to copy coordinations</param>
		//--------------------------------------------------------------------------------
		public Vector(Vector vector)
		{
			m_X = vector.X;
			m_Y = vector.Y;
			m_Z = vector.Z;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Add 2 vectors and create a new one.
		/// </summary>
		/// <param name="vector1">First vector</param>
		/// <param name="vector2">Second vector</param>
		/// <returns>New vector that is the sum of the 2 vectors</returns>
		//--------------------------------------------------------------------------------
		public static Vector Add(Vector vector1, Vector vector2)
		{
			if (((Object)vector1 == null) || ((Object)vector2 == null))
				return null;
			return new Vector(vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z);
		}

		/// <summary>
		/// Substract 2 vectors and create a new one.
		/// </summary>
		/// <param name="vector1">First vector</param>
		/// <param name="vector2">Second vector</param>
		/// <returns>New vector that is the difference of the 2 vectors</returns>
		//--------------------------------------------------------------------------------
		public static Vector Subtract(Vector vector1, Vector vector2)
		{
			if (((Object)vector1 == null) || ((Object)vector2 == null))
				return null;
			return new Vector(vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z);
		}

		/// <summary>
		/// Return a new vector with negative values.
		/// </summary>
		/// <param name="vector">Original vector</param>
		/// <returns>New vector that is the inversion of the original vector</returns>
		//--------------------------------------------------------------------------------
		public static Vector Negate(Vector vector)
		{
			if ((Object)vector == null) return null;
			return new Vector(-vector.X, -vector.Y, -vector.Z);
		}

		/// <summary>
		/// Multiply a vector with a scalar
		/// </summary>
		/// <param name="vector">Vector to be multiplied</param>
		/// <param name="val">Scalar to multiply vector</param>
		/// <returns>New vector that is the multiplication of the vector with the scalar</returns>
		//--------------------------------------------------------------------------------
		public static Vector Multiply(Vector vector, float val)
		{
			if ((Object)vector == null)
				return null;
			return new Vector(vector.X * val, vector.Y * val, vector.Z * val);
		}

		/// <summary>
		/// Calculates dot product of n vectors.
		/// </summary>
		/// <param name="vectors">vectors array.</param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public static float DotProduct(params Vector[] vectors)
		{
			if (vectors.Length < 2)
				throw new ArgumentException("dot product can be calculated from at least two vectors");
			float dx = vectors[0].X, dy = vectors[0].Y, dz = vectors[0].Z;

			for (int i = 1; i < vectors.Length; i++)
			{
				dx *= vectors[0].X;
				dy *= vectors[0].Y;
				dz *= vectors[0].Z;
			}

			return (dx + dy + dz);
		}

		//--------------------------------------------------------------------------------
		public static Vector Contract(Vector vect, float dLength)
		{
			float length = vect.Length;
			if (length == 0) throw new ArgumentException("Vector length equals zero. Can't contract or expand.");
			return new Vector(vect.X - (vect.X * dLength / length),
							  vect.Y - (vect.Y * dLength / length),
							  vect.Z - (vect.Z * dLength / length));
		}

		//--------------------------------------------------------------------------------
		public static Vector Expand(Vector vect, float dLength)
		{
			return Contract(vect, -1 * dLength);
		}

		//--------------------------------------------------------------------------------
		public void Translate(float dx, float dy, float dz)
		{
			X += dx;
			Y += dy;
			Z += dz;
		}

		#endregion

		#region Operators

		/// <summary>
		/// Check equality of two vectors
		/// </summary>
		/// <param name="vector1">First vector</param>
		/// <param name="vector2">Second vector</param>
		/// <returns>True - if he 2 vectors are equal.
		/// False - otherwise</returns>
		//--------------------------------------------------------------------------------
		public static bool operator ==(Vector vector1, Vector vector2)
		{
			if (((Object)vector1 == null) || ((Object)vector2 == null)) return false;
			return ((vector1.X.Equals(vector2.X))
					&& (vector1.Y.Equals(vector2.Y))
					&& (vector1.Z.Equals(vector2.Z)));
		}

		/// <summary>
		/// Check inequality of two vectors
		/// </summary>
		/// <param name="vector1">First vector</param>
		/// <param name="vector2">Second vector</param>
		/// <returns>True - if he 2 vectors are not equal.
		/// False - otherwise</returns>
		//--------------------------------------------------------------------------------
		public static bool operator !=(Vector vector1, Vector vector2)
		{
			if (((Object)vector1 == null) || ((Object)vector2 == null)) return false;
			return (!(vector1 == vector2));
		}

		/// <summary>
		/// Calculate the sum of 2 vectors.
		/// </summary>
		/// <param name="vector1">First vector</param>
		/// <param name="vector2">Second vector</param>
		/// <returns>New vector that is the sum of the 2 vectors</returns>
		//--------------------------------------------------------------------------------
		public static Vector operator +(Vector vector1, Vector vector2)
		{
			if (((Object)vector1 == null) || ((Object)vector2 == null)) return null;
			return Add(vector1, vector2);
		}

		/// <summary>
		/// Calculate the substraction of 2 vectors
		/// </summary>
		/// <param name="vector1">First vector</param>
		/// <param name="vector2">Second vector</param>
		/// <returns>New vector that is the difference of the 2 vectors</returns>
		//--------------------------------------------------------------------------------
		public static Vector operator -(Vector vector1, Vector vector2)
		{
			if (((Object)vector1 == null) || ((Object)vector2 == null))
				return null;
			return Subtract(vector1, vector2);
		}

		//--------------------------------------------------------------------------------
		public static Vector operator -(Vector vector1, float dLength)
		{
			if ((Object)vector1 == null) return null;
			return Contract(vector1, dLength);
		}

		//--------------------------------------------------------------------------------
		public static Vector operator +(Vector vector1, float dLength)
		{
			if ((Object)vector1 == null) return null;
			return Expand(vector1, dLength);
		}

		/// <summary>
		/// Calculate the negative (inverted) vector
		/// </summary>
		/// <param name="vector">Original vector</param>
		/// <returns>New vector that is the invertion of the original vector</returns>
		//--------------------------------------------------------------------------------
		public static Vector operator -(Vector vector)
		{
			if ((Object)vector == null) return null;
			return Negate(vector);
		}

		/// <summary>
		/// Calculate the multiplication of a vector with a scalar
		/// </summary>
		/// <param name="vector">Vector to be multiplied</param>
		/// <param name="val">Scalar to multiply vector</param>
		/// <returns>New vector that is the multiplication of the vector and the scalar</returns>
		//--------------------------------------------------------------------------------
		public static Vector operator *(Vector vector, float val)
		{
			if ((Object)vector == null) return null;
			return Multiply(vector, val);
		}

		/// <summary>
		/// Calculate the multiplication of a vector with a scalar
		/// </summary>
		/// <param name="val">Scalar to multiply vector</param>
		/// <param name="vector">Vector to be multiplied</param>
		/// <returns>New vector that is the multiplication of the vector and the scalar</returns>
		//--------------------------------------------------------------------------------
		public static Vector operator *(float val, Vector vector)
		{
			if ((Object)vector == null) return null;
			return Multiply(vector, val);
		}

		//--------------------------------------------------------------------------------
		public float this[byte index]
		{
			get
			{
				if (index < 0 || index > 2) throw new ArgumentException("index has to be integer from interval [0, 2]");
				switch (index)
				{
					case 0:
						return X;
					case 1:
						return Y;
					case 2:
						return Z;
					default:
						return 0;
				}
			}
			set
			{
				if (index < 0 || index > 2) throw new ArgumentException("index has to be integer from interval [0, 2]");
				switch (index)
				{
					case 0:
						X = value;
						break;
					case 1:
						Y = value;
						break;
					case 2:
						Z = value;
						break;
					default:
						break;
				}
			}
		}

		#endregion

		#region Constants

		/// <summary>
		/// Standard (0,0,0) vector
		/// </summary>
		//--------------------------------------------------------------------------------
		public static Vector Zero
		{
			get { return new Vector(0.0f, 0.0f, 0.0f); }
		}

		/// <summary>
		/// Standard (1,0,0) vector
		/// </summary>
		//--------------------------------------------------------------------------------
		public static Vector XAxis
		{
			get { return new Vector(1.0f, 0.0f, 0.0f); }
		}

		/// <summary>
		/// Standard (0,1,0) vector
		/// </summary>
		//--------------------------------------------------------------------------------
		public static Vector YAxis
		{
			get { return new Vector(0.0f, 1.0f, 0.0f); }
		}

		/// <summary>
		/// Standard (0,0,1) vector
		/// </summary>
		//--------------------------------------------------------------------------------
		public static Vector ZAxis
		{
			get { return new Vector(0.0f, 0.0f, 1.0f); }
		}

		#endregion

		#region Overides

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
		/// </returns>
		//--------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			return (obj is Vector && (Vector)obj == this);
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </returns>
		//--------------------------------------------------------------------------------
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2})", m_X, m_Y, m_Z);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"></see>.
		/// </returns>
		//--------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return m_X.GetHashCode() ^ m_Y.GetHashCode() ^ m_Z.GetHashCode();
		}

		#endregion

		#region ICloneable Members

		//--------------------------------------------------------------------------------
		public object Clone()
		{
			return new Vector(this);
		}

		#endregion
	}
}
