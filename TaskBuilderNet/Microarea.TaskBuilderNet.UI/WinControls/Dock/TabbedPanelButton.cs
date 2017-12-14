using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	//===========================================================================
	// TabbedPanelButton class is a button control with “inert” behavior.
	// Two images can be defined for the button for either disabled or
	// enabled state.
	//===========================================================================
	public class TabbedPanelButton : Control
	{
		private IContainer components = new Container();
		
		private int								borderWidth = 1;
		private Color							borderColor = Color.Empty;
		private bool							mouseOver = false;
		private bool							mouseCapture = false;
		private bool							isPopup = false;
		private Image							enabledImage = null;
		private int								enabledImageIndex = -1;
		private Image							disabledImage = null;
		private int								disabledImageIndex = -1;
		private ImageList						imageList = null;
		private bool							monochrome = true;
		private System.Drawing.ContentAlignment	textAlign = System.Drawing.ContentAlignment.MiddleCenter;
		private ToolTip							toolTip = null;
		private string							toolTipText = String.Empty;

		//---------------------------------------------------------------------------
		public TabbedPanelButton(Image imageEnabled, Image imageDisabled, bool isMonochromatic)
		{
			Initialize(imageEnabled, imageDisabled, isMonochromatic);
		}

		//---------------------------------------------------------------------------
		public TabbedPanelButton(Image imageEnabled, Image imageDisabled) : this(imageEnabled, imageDisabled, false)
		{
		}
				
		//---------------------------------------------------------------------------
		public TabbedPanelButton(Image imageEnabled): this(imageEnabled, null, false)
		{
		}

		//---------------------------------------------------------------------------
		public TabbedPanelButton() : this(null, null, false)
		{
		}
				
		//---------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region TabbedPanelButton public properties

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		public Color BorderColor
		{
			get	{ return borderColor; }
			set
			{
				if (borderColor != value)
				{
					borderColor = value;
					Invalidate();
				}
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		[DefaultValue(1)]
		public int BorderWidth
		{
			get { return borderWidth; }
			set
			{
				if (value < 1)
					value = 1;
				if (borderWidth != value)
				{
					borderWidth = value;
					Invalidate();
				}
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		[DefaultValue(null)]
		public Image EnabledImage
		{
			get
			{ 
				if (enabledImage != null)
					return enabledImage;

				try
				{
					if (imageList == null || enabledImageIndex == -1)
						return null;
					else
						return imageList.Images[enabledImageIndex];
				}
				catch
				{
					return null;
				}
			}
			set
			{
				if (enabledImage != value)
				{
					enabledImage = value;
					Invalidate();
				}
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		[DefaultValue(null)]
		public Image DisabledImage
		{
			get
			{
				if (disabledImage != null)
					return disabledImage;

				try
				{
					if (imageList == null || disabledImageIndex == -1)
						return null;
					else
						return imageList.Images[disabledImageIndex];
				}
				catch
				{
					return null;
				}
			}
			set
			{
				if (disabledImage != value)
				{
					disabledImage = value;
					Invalidate();
				}
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public ImageList ImageList
		{
			get	{ return imageList; }
			set
			{
				if (imageList != value)
				{
					imageList = value;
					Invalidate();
				}
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		[DefaultValue(-1)]
		[Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design", "System.Drawing.Design.UITypeEditor,System.Drawing")]
		[TypeConverter(typeof(System.Windows.Forms.ImageIndexConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		public int EnabledImageIndex
		{
			get	{ return enabledImageIndex; }
			set
			{
				if (enabledImageIndex != value)
				{
					enabledImageIndex = value;
					Invalidate();
				}
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		[DefaultValue(-1)]
		[Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design", "System.Drawing.Design.UITypeEditor,System.Drawing")]
		[TypeConverter(typeof(System.Windows.Forms.ImageIndexConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		public int DisabledImageIndex
		{
			get	{ return disabledImageIndex; }
			set
			{
				if (disabledImageIndex != value)
				{
					disabledImageIndex = value;
					Invalidate();
				}
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		[DefaultValue(false)]
		public bool IsPopup
		{
			get { return isPopup; }

			set
			{
				if (isPopup != value)
				{
					isPopup = value;
					Invalidate();
				}
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		[DefaultValue(true)]
		public bool Monochrome
		{
			get	{ return monochrome; }
			set
			{
				if (value != monochrome)
				{
					monochrome = value;
					Invalidate();
				}
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		[DefaultValue(ContentAlignment.MiddleCenter)]
		public ContentAlignment TextAlign
		{
			get	{	return textAlign;	}
			set
			{
				if (textAlign != value)
				{
					textAlign = value;
					Invalidate();
				}
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		[DefaultValue("")]
		[Localizable(true)]
		public string ToolTipText
		{
			get	{	return toolTipText;	}
			set
			{
				if (toolTipText != value)
				{
					if (toolTip == null)
						toolTip = new ToolTip(this.components);
					toolTipText = value;
					toolTip.SetToolTip(this, value);
				}
			}
		}

		#endregion

		#region TabbedPanelButton protected overridden methods

		//---------------------------------------------------------------------------
		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			if (Enabled == false)
			{
				mouseOver = false;
				mouseCapture = false;
			}
			Invalidate();
		}
		
		//---------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button != MouseButtons.Left)
				return;

			if (mouseCapture == false || mouseOver == false)
			{
				mouseCapture = true;
				mouseOver = true;

				//Redraw to show button state
				Invalidate();
			}
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.Button != MouseButtons.Left)
				return;

			if (mouseOver == true || mouseCapture == true)
			{
				mouseOver = false;
				mouseCapture = false;

				// Redraw to show button state
				Invalidate();
			}

			base.OnMouseUp(e);
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			// Is mouse point inside our client rectangle
			bool over = this.ClientRectangle.Contains(new Point(e.X, e.Y));

			// If entering the button area or leaving the button area...
			if (over != mouseOver)
			{
				// Update state
				mouseOver = over;

				// Redraw to show button state
				Invalidate();
			}
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseEnter(EventArgs e)
		{
			// Update state to reflect mouse over the button area
			if (!mouseOver)
			{
				mouseOver = true;

				// Redraw to show button state
				Invalidate();
			}

			base.OnMouseEnter(e);
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseLeave(EventArgs e)
		{
			// Update state to reflect mouse not over the button area
			if (mouseOver)
			{
				mouseOver = false;

				// Redraw to show button state
				Invalidate();
			}

			base.OnMouseLeave(e);
		}

		//---------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint(e);

			SolidBrush bkgndBrush = new SolidBrush(this.BackColor);
			e.Graphics.FillRectangle(bkgndBrush, this.ClientRectangle);
			bkgndBrush.Dispose();

			DrawImage(e.Graphics);
			DrawText(e.Graphics);

			DrawBorder(e.Graphics);
		}

		#endregion

		#region TabbedPanelButton private methods

		//---------------------------------------------------------------------------
		private void Initialize(System.Drawing.Image aEnabledImage, System.Drawing.Image aDisabledImage, bool isMonochromatic)
		{
			Monochrome = isMonochromatic;
			EnabledImage = aEnabledImage;
			DisabledImage = aDisabledImage;

			// Prevent drawing flicker by blitting from memory in WM_PAINT
			SetStyle
				(
				ControlStyles.ResizeRedraw |
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.DoubleBuffer,
				true
				);

			// Prevent base class from trying to generate double click events and
			// so testing clicks against the double click time and rectangle. Getting
			// rid of this allows the user to press then release button very quickly.
			SetStyle(ControlStyles.StandardDoubleClick, false);

			// Should not be allowed to select this control
			SetStyle(ControlStyles.Selectable, false);
		}

		//---------------------------------------------------------------------------
		private void DrawImage(Graphics g)
		{
			Image image = this.Enabled ? EnabledImage : ((DisabledImage != null) ? DisabledImage : EnabledImage);

			if (image == null)
				return;

			ImageAttributes monochromaticImageAttr = null;

			if (monochrome)
			{
				monochromaticImageAttr = new ImageAttributes();

				// transform the monochrom image
				// white -> BackColor
				// black -> ForeColor
				ColorMap[] colorMap = new ColorMap[2];
				colorMap[0] = new ColorMap();
				colorMap[0].OldColor = Color.White;
				colorMap[0].NewColor = this.BackColor;
				colorMap[1] = new ColorMap();
				colorMap[1].OldColor = Color.Black;
				colorMap[1].NewColor = this.ForeColor;
				
				monochromaticImageAttr.SetRemapTable(colorMap);
			}

			Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

			if (!Enabled && DisabledImage == null)
			{
				Bitmap monochromeBitmap = new Bitmap(image, ClientRectangle.Size);
				if (monochromaticImageAttr != null)
				{
					Graphics monochromeGraphics = Graphics.FromImage(monochromeBitmap);
					monochromeGraphics.DrawImage
						(
						image, 
						new Point[3] { new Point(0, 0), new Point(image.Width - 1, 0), new Point(0, image.Height - 1) }, 
						rect, 
						GraphicsUnit.Pixel, 
						monochromaticImageAttr
						);
					monochromeGraphics.Dispose();
				}
				ControlPaint.DrawImageDisabled(g, monochromeBitmap, 0, 0, this.BackColor);

				monochromeBitmap.Dispose();
			}
			else
			{
				// Three points provided are upper-left, upper-right and 
				// lower-left of the destination parallelogram. 
				Point[] pts = new Point[3];
				pts[0].X = (Enabled && mouseOver && mouseCapture) ? 1 : 0;
				pts[0].Y = (Enabled && mouseOver && mouseCapture) ? 1 : 0;
				pts[1].X = pts[0].X + ClientRectangle.Width;
				pts[1].Y = pts[0].Y;
				pts[2].X = pts[0].X;
				pts[2].Y = pts[1].Y + ClientRectangle.Height;

				if (monochromaticImageAttr != null)
					g.DrawImage(image, pts, rect, GraphicsUnit.Pixel, monochromaticImageAttr);
				else
					g.DrawImage(image, pts, rect, GraphicsUnit.Pixel);
			}
		}	

		//---------------------------------------------------------------------------
		private void DrawText(Graphics g)
		{
			if (Text == null || Text == String.Empty)
				return;

			Rectangle rect = ClientRectangle;

			rect.X += BorderWidth;
			rect.Y += BorderWidth;
			rect.Width -= 2 * BorderWidth;
			rect.Height -= 2 * BorderWidth;

			StringFormat stringFormat = new StringFormat();

			if (TextAlign == ContentAlignment.TopLeft)
			{
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Near;
			}
			else if (TextAlign == ContentAlignment.TopCenter)
			{
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Near;
			}
			else if (TextAlign == ContentAlignment.TopRight)
			{
				stringFormat.Alignment = StringAlignment.Far;
				stringFormat.LineAlignment = StringAlignment.Near;
			}
			else if (TextAlign == ContentAlignment.MiddleLeft)
			{
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Center;
			}
			else if (TextAlign == ContentAlignment.MiddleCenter)
			{
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Center;
			}
			else if (TextAlign == ContentAlignment.MiddleRight)
			{
				stringFormat.Alignment = StringAlignment.Far;
				stringFormat.LineAlignment = StringAlignment.Center;
			}
			else if (TextAlign == ContentAlignment.BottomLeft)
			{
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Far;
			}
			else if (TextAlign == ContentAlignment.BottomCenter)
			{
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Far;
			}
			else if (TextAlign == ContentAlignment.BottomRight)
			{
				stringFormat.Alignment = StringAlignment.Far;
				stringFormat.LineAlignment = StringAlignment.Far;
			}

			Brush brush = new SolidBrush(ForeColor);
				
			g.DrawString(Text, Font, brush, rect, stringFormat);

			brush.Dispose();
		}

		//---------------------------------------------------------------------------
		private void DrawBorder(Graphics g)
		{
			if (!monochrome)
			{
				if (!Enabled || !mouseOver)
					return;

				System.Windows.Forms.Border3DStyle borderStyle  = mouseCapture ? System.Windows.Forms.Border3DStyle.Flat : System.Windows.Forms.Border3DStyle.Etched;

				ControlPaint.DrawBorder3D(g, this.ClientRectangle, borderStyle);

				return;
			}

			System.Windows.Forms.ButtonBorderStyle bs;

			// Decide on the type of border to draw around image
			if (!this.Enabled)
				bs = isPopup ? ButtonBorderStyle.Outset : ButtonBorderStyle.Solid;
			else if (mouseOver && mouseCapture)
				bs = ButtonBorderStyle.Inset;
			else if (isPopup || mouseOver)
				bs = ButtonBorderStyle.Outset;
			else
				bs = ButtonBorderStyle.Solid;

			Color colorLeftTop;
			Color colorRightBottom;
			if (bs == ButtonBorderStyle.Solid)
			{
				colorLeftTop = this.BackColor;
				colorRightBottom = this.BackColor;
			}
			else if (bs == ButtonBorderStyle.Outset)
			{
				colorLeftTop = borderColor.IsEmpty ? this.BackColor : borderColor;
				colorRightBottom = this.BackColor;
			}
			else
			{
				colorLeftTop = this.BackColor;
				colorRightBottom = borderColor.IsEmpty ? this.BackColor : borderColor;
			}

			ControlPaint.DrawBorder(g, this.ClientRectangle,
				colorLeftTop, borderWidth, bs,
				colorLeftTop, borderWidth, bs,
				colorRightBottom, borderWidth, bs,
				colorRightBottom, borderWidth, bs);
		}

		#endregion

	}
}