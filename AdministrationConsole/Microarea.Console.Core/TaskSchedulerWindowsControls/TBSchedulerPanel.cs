using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	//============================================================================
	/// <summary>
	/// An extended Panel that provides collapsible panels like those provided in Windows XP.
	/// </summary>
	//============================================================================
	public partial class TaskBuilderSchedulerPanel : System.Windows.Forms.Panel
	{
		public enum PanelState	{ Undefined = -1, Expanded = 0, Collapsed = 1 }
		
		public event PanelStateChangedEventHandler PanelStateChanged;

		private int	panelHeight;
		private int imageIndex = 0;
		private int minTitleHeight = 0;
		private const int iconBorder = 2;
		private const int expandBorder = 4;
		private const int labelsXOffset = 10;
		private const int labelsYSpacing = 4;
		private const int labelsXSpacingFromPicture = 2;
		
		private System.Drawing.Color					startColor = Color.White;
		private System.Drawing.Color					endColor = Color.LightSteelBlue;
		private System.Drawing.Image					titleImage;
		private System.Drawing.Color					imagesTransparentColor = Color.White;
		private System.Drawing.Imaging.ColorMatrix		grayMatrix;
		private System.Drawing.Imaging.ImageAttributes	grayAttributes;

		private PanelState	state = PanelState.Expanded;
		
		//---------------------------------------------------------------------------
		public TaskBuilderSchedulerPanel()
		{
			InitializeComponent();

			TitleLabel.Font = new System.Drawing.Font(Font.Name, Font.SizeInPoints + 1.5F, Font.FontFamily.IsStyleAvailable(System.Drawing.FontStyle.Bold) ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			minTitleHeight = TitleLabel.Font.Height + (2 * TaskBuilderSchedulerPanel.expandBorder);

			// Setup the ColorMatrix and ImageAttributes for grayscale images.
			grayMatrix = new ColorMatrix();
			grayMatrix.Matrix00 = 1/3f;
			grayMatrix.Matrix01 = 1/3f;
			grayMatrix.Matrix02 = 1/3f;
			grayMatrix.Matrix10 = 1/3f;
			grayMatrix.Matrix11 = 1/3f;
			grayMatrix.Matrix12 = 1/3f;
			grayMatrix.Matrix20 = 1/3f;
			grayMatrix.Matrix21 = 1/3f;
			grayMatrix.Matrix22 = 1/3f;
			grayAttributes = new ImageAttributes();
			grayAttributes.SetColorMatrix(grayMatrix, ColorMatrixFlag.Default,
				ColorAdjustType.Bitmap);
			grayAttributes.SetColorKey(ImagesTransparentColor, ImagesTransparentColor, ColorAdjustType.Default);
		}

		//---------------------------------------------------------------------------
		protected override void InitLayout()
		{
			base.InitLayout();

			UpdateDisplayedState();
		}

		//---------------------------------------------------------------------------
		public int PanelHeight { get { return panelHeight; } }
		
		//---------------------------------------------------------------------------
		public int TitleHeight { get { return TitleLabel.Height; } }
		
		//---------------------------------------------------------------------------
		public override bool AutoScroll
		{
			get { return false; }
		}
		

		/// <summary>
		/// Gets/sets the PanelState.
		/// </summary>
		//---------------------------------------------------------------------------
		public PanelState State
		{
			get
			{
				return state;
			}
			set
			{
				PanelState oldState = state;
				state = value;
				if(oldState != state)
				{
					// State has changed to update the display
					UpdateDisplayedState();
				}
			}
		}

		//---------------------------------------------------------------------------
		public bool IsCollapsed
		{
			get
			{
				return State == PanelState.Collapsed;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsExpanded
		{
			get
			{
				return State == PanelState.Expanded;
			}
		}

		/// <summary>
		/// Gets/sets the text displayed as the panel title.
		/// </summary>
		//---------------------------------------------------------------------------
		[Localizable(true)]
		public string Title
		{
			get
			{
				return TitleLabel.Text;
			}
			set
			{
				TitleLabel.Text = value;
			}
		}

		/// <summary>
		/// Gets/sets the foreground color used for the title bar.
		/// </summary>
		//---------------------------------------------------------------------------
		public Color TitleFontColor
		{
			get
			{
				return TitleLabel.ForeColor;
			}
			set
			{
				TitleLabel.ForeColor = value;
			}
		}

		/// <summary>
		/// Gets/sets the font used for the title bar text.
		/// </summary>
		//---------------------------------------------------------------------------
		public Font TitleFont
		{
			get
			{
				return TitleLabel.Font;
			}
			set
			{
				TitleLabel.Font = value;
				minTitleHeight =  Math.Max(TitleLabel.Font.Height + (2 * TaskBuilderSchedulerPanel.expandBorder), TitleLabel.Height);
				if(TitleLabel.Height < minTitleHeight)
				{
					TitleLabel.Height = minTitleHeight;
				}
			}
		}

		/// <summary>
		/// Gets/sets the image list used for the expand/collapse image.
		/// </summary>
		//---------------------------------------------------------------------------
		public ImageList ImageList
		{
			get
			{
				return PanelImageList;
			}
			set
			{
				imageIndex = -1;
				
				PanelImageList = value;
				if(null != PanelImageList)
				{
					if(PanelImageList.Images.Count > 0)
					{
						imageIndex = 0;
					}
				}
			}
		}

		/// <summary>
		/// Gets/sets the starting color for the background gradient of the header.
		/// </summary>
		//---------------------------------------------------------------------------
		public Color StartColor
		{
			get
			{
				return startColor;
			}
			set
			{
				startColor = value;
				TitleLabel.Invalidate();
			}
		}

		/// <summary>
		/// Gets/sets the ending color for the background gradient of the header.
		/// </summary>
		//---------------------------------------------------------------------------
		public Color EndColor
		{
			get
			{
				return endColor;
			}
			set
			{
				endColor = value;
				TitleLabel.Invalidate();
			}
		}

		/// <summary>
		/// Gets/sets the image displayed in the header of the title bar.
		/// </summary>
		//---------------------------------------------------------------------------
		public Image TitleImage //The image that will be displayed on the left hand side of the title bar.
		{
			get
			{
				return titleImage;
			}
			set
			{
				titleImage = value;
				if (value != null)
				{
					// Update the height of the title label
					TitleLabel.Height = Math.Max(titleImage.Height + (2 * TaskBuilderSchedulerPanel.iconBorder), TitleLabel.Height);
					if(TitleLabel.Height < minTitleHeight)
					{
						TitleLabel.Height = minTitleHeight;
					}
				}
				TitleLabel.Invalidate();
			}
		}

		/// <summary>
		/// Gets/sets the transparent color for the the image displayed in the header of the title bar.
		/// </summary>
		//---------------------------------------------------------------------------
		public Color ImagesTransparentColor
		{
			get
			{
				return imagesTransparentColor;
			}
			set
			{
				imagesTransparentColor = value;
				if (grayAttributes != null)
				{
					grayAttributes.ClearColorKey(ColorAdjustType.Default);
					grayAttributes.SetColorKey(value, value, ColorAdjustType.Default);
				}
				if (TitleImage != null)
					TitleLabel.Invalidate();
			}
		}

		// Expand/Collapse functionality updated as per Windows XP. Whole of title bar is active
		/// <summary>
		/// Helper function to determine if the mouse is currently over the title bar.
		/// </summary>
		/// <param name="xPos">The x-coordinate of the mouse position.</param>
		/// <param name="yPos">The y-coordinate of the mouse position.</param>
		/// <returns></returns>
		//---------------------------------------------------------------------------
		private bool IsOverTitle(int xPos, int yPos)
		{
			// Get the dimensions of the title label
			Rectangle rectTitle = TitleLabel.Bounds;
			// Check if the supplied coordinates are over the title label
			if(rectTitle.Contains(xPos, yPos))
			{
				return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public void Collapse()
		{
			State = PanelState.Collapsed;
		}
		
		//---------------------------------------------------------------------------
		public void Expand()
		{
			State = PanelState.Expanded;
		}

		/// <summary>
		/// Helper function to update the displayed state of the panel.
		/// </summary>
		//---------------------------------------------------------------------------
		private void UpdateDisplayedState()
		{
			switch(state)
			{
				case PanelState.Collapsed :
					// Entering collapsed state, so collapse the panel
					Height = TitleLabel.Height;
					// Update the image.
					imageIndex = 1;
					break;
				case PanelState.Expanded :
					// Entering expanded state, so expand the panel.
					Height = panelHeight;
					// Update the image.
					imageIndex = 0;
					break;
				default :
					// Ignore
					break;
			}
			TitleLabel.Invalidate();

			OnPanelStateChanged(new TaskBuilderSchedulerEventArgs(this));
		}

		//---------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{	
			// Invoke base class implementation
			base.OnResize(e);

			if (IsExpanded) 
				panelHeight = Height;

			foreach (Control addedControl in this.Controls)
			{
				if (addedControl is TaskBuilderSchedulerLinklabel)
					((TaskBuilderSchedulerLinklabel)addedControl).SetWidthToFitPanel(this);
			}
		}

		//---------------------------------------------------------------------------
		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);
			
			if (e != null && e.Control != null && e.Control is TaskBuilderSchedulerLinklabel)
				((TaskBuilderSchedulerLinklabel)e.Control).SetWidthToFitPanel(this);
		}
		
		/// <summary>
		/// Event handler for the PanelStateChanged event.
		/// </summary>
		/// <param name="e">A TaskBuilderSchedulerEventArgs that contains the event data.</param>
		//---------------------------------------------------------------------------
		protected virtual void OnPanelStateChanged(TaskBuilderSchedulerEventArgs e)
		{
			Refresh();

			if(PanelStateChanged != null)
				PanelStateChanged(this, e);
		}

		//---------------------------------------------------------------------------
		private void TitleLabel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			const int diameter = 14;
			int radius = diameter / 2;
			Rectangle bounds = TitleLabel.Bounds;
			int offsetY = 0;
			if(titleImage != null)
			{
				offsetY = TitleLabel.Height - minTitleHeight;
				if(offsetY < 0)
				{
					offsetY = 0;
				}
				bounds.Offset(0, offsetY);
				bounds.Height -= offsetY;
			}

			e.Graphics.Clear(Parent.BackColor);

			// Create a GraphicsPath with curved top corners
			GraphicsPath path = new GraphicsPath();
			path.AddLine(bounds.Left + radius, bounds.Top, bounds.Right - diameter - 1, bounds.Top);
			path.AddArc(bounds.Right - diameter - 1, bounds.Top, diameter, diameter, 270, 90);
			path.AddLine(bounds.Right, bounds.Top + radius, bounds.Right, bounds.Bottom);
			path.AddLine(bounds.Right, bounds.Bottom, bounds.Left - 1, bounds.Bottom);
			path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);

			// Create a color gradient
			// Draws the title gradient grayscale when disabled.
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if (Enabled)
			{
				LinearGradientBrush brush = new LinearGradientBrush(
					bounds, startColor, endColor, LinearGradientMode.Horizontal);

				// Paint the color gradient into the title label.
				e.Graphics.FillPath(brush, path);
			}
			else
			{
				PanelColor grayStart = new PanelColor();
				grayStart.CurrentColor = startColor;
				grayStart.Saturation = 0f;
				
				PanelColor grayEnd = new PanelColor();
				grayEnd.CurrentColor = endColor;
				grayEnd.Saturation = 0f;

				LinearGradientBrush brush = new LinearGradientBrush(
					bounds, grayStart.CurrentColor, grayEnd.CurrentColor,
					LinearGradientMode.Horizontal);

				// Paint the grayscale gradient into the title label.
				e.Graphics.FillPath(brush, path);
			}

			// Draw the header icon, if there is one
			System.Drawing.GraphicsUnit graphicsUnit = System.Drawing.GraphicsUnit.Display;
			int offsetX = TaskBuilderSchedulerPanel.iconBorder;
			if(titleImage != null)
			{
				offsetX += titleImage.Width + TaskBuilderSchedulerPanel.iconBorder;
				// Draws the title icon grayscale when the panel is disabled.
				RectangleF srcRect = titleImage.GetBounds(ref graphicsUnit);
				Rectangle destRect = new Rectangle(TaskBuilderSchedulerPanel.iconBorder,
					TaskBuilderSchedulerPanel.iconBorder, titleImage.Width, titleImage.Height);
				
				if(Enabled)
				{
					ImageAttributes imgAttrs = new ImageAttributes();
					// Set the default color key.
					imgAttrs.SetColorKey(ImagesTransparentColor, ImagesTransparentColor, ColorAdjustType.Default);
				
					e.Graphics.DrawImage
								(
									titleImage, 
									destRect, 
									(int)srcRect.Left, 
									(int)srcRect.Top,
									(int)srcRect.Width, 
									(int)srcRect.Height, 
									graphicsUnit, 
									imgAttrs
								);
				}
				else
				{
					e.Graphics.DrawImage
							(
								titleImage, 
								destRect, 
								(int)srcRect.Left, 
								(int)srcRect.Top,
								(int)srcRect.Width, 
								(int)srcRect.Height, 
								graphicsUnit, 
								grayAttributes
							);
				}
			}

			// Draw the title text.
			SolidBrush textBrush = new SolidBrush(TitleFontColor);
			// Title text truncated with an ellipsis where necessary.
			float left = (float)offsetX;
			float top = (float)offsetY + (float)TaskBuilderSchedulerPanel.iconBorder;
			float width = (float)TitleLabel.Width - left - PanelImageList.ImageSize.Width - TaskBuilderSchedulerPanel.expandBorder;
			float height = (float)minTitleHeight - (2f * (float)TaskBuilderSchedulerPanel.iconBorder);
			RectangleF textRectF = new RectangleF(left, top, width, height);
			StringFormat format = new StringFormat();
			format.LineAlignment = StringAlignment.Center;
			format.Trimming = StringTrimming.EllipsisWord;
			format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;
			// Draw title text disabled where appropriate.
			if(Enabled)
			{
				e.Graphics.DrawString(TitleLabel.Text, TitleLabel.Font, textBrush, textRectF, format);
			}
			else
			{
				Color disabled = SystemColors.GrayText;
				ControlPaint.DrawStringDisabled(e.Graphics, TitleLabel.Text, TitleLabel.Font, disabled, textRectF, format);
			}

			// Draw a white line at the bottom:
			const int lineWidth = 1;
			SolidBrush lineBrush = new SolidBrush(Color.White);
			Pen linePen = new Pen(lineBrush, lineWidth);
			path.Reset();
			path.AddLine(bounds.Left, bounds.Bottom - lineWidth, bounds.Right, 
				bounds.Bottom - lineWidth);
			e.Graphics.DrawPath(linePen, path);

			// Draw the expand/collapse image
			// Expand/Collapse image drawn grayscale when panel is disabled.
			int xPos = bounds.Right - PanelImageList.ImageSize.Width - TaskBuilderSchedulerPanel.expandBorder;
			int yPos = bounds.Top + TaskBuilderSchedulerPanel.expandBorder;
			RectangleF srcIconRectF = PanelImageList.Images[(int)state].GetBounds(ref graphicsUnit);
			Rectangle destIconRect = new Rectangle(xPos, yPos, 
				PanelImageList.ImageSize.Width, PanelImageList.ImageSize.Height);
			if(Enabled)
			{
				e.Graphics.DrawImage(PanelImageList.Images[(int)state], destIconRect,
					(int)srcIconRectF.Left, (int)srcIconRectF.Top, (int)srcIconRectF.Width,
					(int)srcIconRectF.Height, graphicsUnit);
			}
			else
			{
				e.Graphics.DrawImage(PanelImageList.Images[(int)state], destIconRect,
					(int)srcIconRectF.Left, (int)srcIconRectF.Top, (int)srcIconRectF.Width,
					(int)srcIconRectF.Height, graphicsUnit, grayAttributes);
			}
		}

		//---------------------------------------------------------------------------
		private void TitleLabel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if((e.Button == MouseButtons.Left) && (true == IsOverTitle(e.X, e.Y)))
			{
				if((PanelImageList != null) && (PanelImageList.Images.Count >= 2))
				{
					// Currently expanded, so store the current height.
					// Currently collapsed, so expand the panel.
					State = (imageIndex == 0) ? PanelState.Collapsed : PanelState.Expanded;
				}
			}
		}

		//---------------------------------------------------------------------------
		private void TitleLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if((e.Button == MouseButtons.None) && (true == IsOverTitle(e.X, e.Y)))
			{
				TitleLabel.Cursor = Cursors.Hand;
			}
			else
			{
				TitleLabel.Cursor = Cursors.Default;
			}
		}
	}

	/// <summary>
	/// Summary description for TaskBuilderSchedulerPanelCollection.
	/// </summary>
	//============================================================================
	public class TaskBuilderSchedulerPanelCollection : System.Collections.CollectionBase
	{
		/// <summary>
		/// Adds a TaskBuilderSchedulerPanel to the end of the collection.
		/// </summary>
		/// <param name="panel">The TaskBuilderSchedulerPanel to add.</param>
		public void Add(TaskBuilderSchedulerPanel panel)
		{
			List.Add(panel);
		}

		/// <summary>
		/// Removes the TaskBuilderSchedulerPanel from the collection at the specified index.
		/// </summary>
		/// <param name="index">The index of the TaskBuilderSchedulerPanel to remove.</param>
		//---------------------------------------------------------------------------
		public void Remove(int index)
		{
			// Ensure the supplied index is valid
			if((index >= Count) || (index < 0))
			{
				throw new IndexOutOfRangeException("The supplied index is out of range");
			}
			List.RemoveAt(index);
		}

		/// <summary>
		/// Gets a reference to the TaskBuilderSchedulerPanel at the specified index.
		/// </summary>
		/// <param name="index">The index of the TaskBuilderSchedulerPanel to retrieve.</param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------------------------------------------------------
		public TaskBuilderSchedulerPanel this[int index]
		{
			get 
			{
				if((index >= Count) || (index < 0))
				{
					throw new IndexOutOfRangeException("The supplied index is out of range");
				}
				return (TaskBuilderSchedulerPanel)List[index];
			}
		}

		/// <summary>
		/// Inserts a TaskBuilderSchedulerPanel at the specified position.
		/// </summary>
		/// <param name="index">The zero-based index at which <i>panel</i> should be inserted.</param>
		/// <param name="panel">The TaskBuilderSchedulerPanel to insert into the collection.</param>
		//---------------------------------------------------------------------------
		public void Insert(int index, TaskBuilderSchedulerPanel panel)
		{
			List.Insert(index, panel);
		}

		/// <summary>
		/// Copies the elements of the collection to a System.Array starting at a particular index.
		/// </summary>
		/// <param name="array">The one-dimensional System.Array that is the destination of the elements. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		//---------------------------------------------------------------------------
		public void CopyTo(System.Array array, System.Int32 index)
		{
			List.CopyTo(array, index);
		}

		/// <summary>
		/// Searches for the specified TaskBuilderSchedulerPanel and returns the zero-based index of the first occurrence.
		/// </summary>
		/// <param name="panel">The TaskBuilderSchedulerPanel to search for.</param>
		/// <returns></returns>
		//---------------------------------------------------------------------------
		public int IndexOf(TaskBuilderSchedulerPanel panel)
		{
			return List.IndexOf(panel);
		}
	}

	
	//============================================================================
	public class TaskBuilderSchedulerEventArgs : System.EventArgs
	{
		private TaskBuilderSchedulerPanel panel;

		//---------------------------------------------------------------------------
		public TaskBuilderSchedulerEventArgs(TaskBuilderSchedulerPanel sender)
		{
			panel = sender;
		}

		/// <summary>
		/// Gets the TaskBuilderSchedulerPanel that triggered the event.
		/// </summary>
		//---------------------------------------------------------------------------
		public TaskBuilderSchedulerPanel TaskBuilderSchedulerPanel
		{
			get
			{
				return panel;
			}
		}

		/// <summary>
		/// Gets the PanelState of the TaskBuilderSchedulerPanel that triggered the event.
		/// </summary>
		//---------------------------------------------------------------------------
		[Localizable(true)]
		public TaskBuilderSchedulerPanel.PanelState State
		{
			get
			{
				return panel.State;
			}
		}
	}

	public delegate void PanelStateChangedEventHandler(object sender, TaskBuilderSchedulerEventArgs e);

	/// <summary>
	/// Stores a color and provides conversion between the RGB and HLS color models
	/// </summary>
	//============================================================================
	public class PanelColor
	{
		// Constants
		public const int HUEMAX = 360;
		public const float SATMAX = 1.0f;
		public const float BRIGHTMAX = 1.0f;
		public const int RGBMAX	= 255;

		// Member variables
		private Color	currentColor = Color.Red;

		/// <summary>
		/// The current PanelColor (RGB model)
		/// </summary>
		//---------------------------------------------------------------------------
		public Color CurrentColor
		{
			get
			{
				return currentColor;
			}
			set
			{
				currentColor = value;
			}
		}


		/// <summary>
		/// The Red component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public byte Red
		{
			get
			{
				return currentColor.R;
			}
			set
			{
				currentColor = Color.FromArgb(value, Green, Blue);
			}
		}

		/// <summary>
		/// The Green component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public byte Green
		{
			get
			{
				return currentColor.G;
			}
			set
			{
				currentColor = Color.FromArgb(Red, value, Blue);
			}
		}

		/// <summary>
		/// The Blue component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public byte Blue
		{
			get
			{
				return currentColor.B;
			}
			set
			{
				currentColor = Color.FromArgb(Red, Green, value);
			}
		}

		/// <summary>
		/// The Hue component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public int Hue
		{
			get
			{
				return (int)currentColor.GetHue();
			}
			set
			{
				currentColor = HSBToRGB(value,
					currentColor.GetSaturation(),
					currentColor.GetBrightness());
			}
		}

		
		//---------------------------------------------------------------------------
		public float GetHue()
		{
			float top = ((float)(2 * Red - Green - Blue)) / (2 * 255);
			float bottom = (float)Math.Sqrt(((Red - Green) * (Red - Green) + (Red - Blue) * (Green - Blue)) / 255);
			return (float)Math.Acos(top / bottom);
		}

		/// <summary>
		/// The Saturation component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public float Saturation
		{
			get
			{
				if(0.0f == Brightness)
				{
					return 0.0f;
				}
				else
				{
					float fMax = (float)Math.Max(Red, Math.Max(Green, Blue));
					float fMin = (float)Math.Min(Red, Math.Min(Green, Blue));
					return (fMax - fMin) / fMax;
				}
			}
			set
			{
				currentColor = HSBToRGB((int)currentColor.GetHue(),
					value, currentColor.GetBrightness());
			}
		}

		//---------------------------------------------------------------------------
		public float GetSaturation()
		{
			return (255 -
				(((float)(Red + Green + Blue)) / 3) * Math.Min(Red, Math.Min(Green, Blue))) / 255;
		}

		/// <summary>
		/// The Brightness component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public float Brightness
		{
			get
			{
				//return currentColor.GetBrightness();
				return (float)Math.Max(Red, Math.Max(Green, Blue)) / (255.0f);
			}
			set
			{
				currentColor = PanelColor.HSBToRGB((int)currentColor.GetHue(),
					currentColor.GetSaturation(),
					value);
			}
		}

		//---------------------------------------------------------------------------
		public float GetBrightness()
		{
			return ((float)(Red + Green + Blue)) / (255.0f * 3.0f);
		}
		
		/// <summary>
		/// Converts HSB color components to an RGB System.Drawing.Color
		/// </summary>
		/// <param name="Hue">Hue component</param>
		/// <param name="Saturation">Saturation component</param>
		/// <param name="Brightness">Brightness component</param>
		/// <returns>Returns the RGB value as a System.Drawing.Color</returns>
		//---------------------------------------------------------------------------
		public static Color HSBToRGB(int Hue, float Saturation, float Brightness)
		{
			// TODO: CheckHSBValues(Hue, Saturation, Brightness);
			int red = 0; int green = 0; int blue = 0;
			if(Saturation == 0.0f)
			{
				// Achromatic color (black and white centre line)
				// Hue should be 0 (undefined), but we'll ignore it.
				// Set shade of grey
				red = green = blue = (int)(Brightness * 255);
			}
			else
			{
				// Chromatic color
				// Map hue from [0-255] to [0-360] to hexagonal-space [0-6]
				// (360 / 256) * hue[0-255] / 60
				float fHexHue = (6.0f / 360.0f) * Hue;
				// Determine sector in hexagonal-space (RGB cube projection) {0,1,2,3,4,5}
				float fHexSector = (float)Math.Floor((double)fHexHue);
				// Determine exact position in particular sector [0-1]
				float fHexSectorPos = fHexHue - fHexSector;

				// Convert parameters to in-formula ranges
				float fBrightness = Brightness * 255.0f;
				float fSaturation = Saturation/*(float)Saturation * (1.0f / 360.0f)*/;

				// Magic formulas (from Foley & Van Dam). Adding 0.5 performs rounding instead of truncation
				byte bWashOut = (byte)(0.5f + fBrightness * (1.0f - fSaturation));
				byte bHueModifierOddSector = (byte)(0.5f + fBrightness * (1.0f - fSaturation * fHexSectorPos));
				byte bHueModifierEvenSector = (byte)(0.5f + fBrightness * (1.0f - fSaturation * (1.0f - fHexSectorPos)));

				// Assign values to RGB components (sector dependent)
				switch((int)fHexSector)
				{
					case 0 :
						// Hue is between red & yellow
						red = (int)(Brightness * 255); green = bHueModifierEvenSector; blue = bWashOut;
						break;
					case 1 :
						// Hue is between yellow & green
						red = bHueModifierOddSector; green = (int)(Brightness * 255); blue = bWashOut;
						break;
					case 2 :
						// Hue is between green & cyan
						red = bWashOut; green = (int)(Brightness * 255); blue = bHueModifierEvenSector;
						break;
					case 3 :
						// Hue is between cyan & blue
						red = bWashOut; green = bHueModifierOddSector; blue = (int)(Brightness * 255);
						break;
					case 4 :
						// Hue is between blue & magenta
						red = bHueModifierEvenSector; green = bWashOut; blue = (int)(Brightness * 255);
						break;
					case 5 :
						// Hue is between magenta & red
						red = (int)(Brightness * 255); green = bWashOut; blue = bHueModifierOddSector;
						break;
					default :
						red = 0; green = 0; blue = 0;
						break;
				}
			}

			return Color.FromArgb(red, green, blue);
		}
	}


	/// <summary>
	/// An ExplorerBar-type extended Panel for containing TaskBuilderSchedulerPanel objects.
	/// </summary>
	//============================================================================
	public class TaskBuilderSchedulerPanelBar : System.Windows.Forms.Panel, System.ComponentModel.ISupportInitialize
	{
		private TaskBuilderSchedulerPanelCollection panels = new TaskBuilderSchedulerPanelCollection();
		private int xSpacing = 8;
		private int ySpacing = 8;
		private bool initialising = false;

		public delegate void PanelsPositionUpdatingEventHandler(object sender, int newPanelsWidth);
		public event PanelsPositionUpdatingEventHandler PanelsPositionUpdating;
		public event PanelsPositionUpdatingEventHandler PanelsPositionUpdated;
		//---------------------------------------------------------------------------
		public TaskBuilderSchedulerPanelBar()
		{
			InitializeComponent();

			BackColor = Color.CornflowerBlue;
		}

		#region Windows Form Designer generated code
		private void InitializeComponent()
		{
			// 
			// TaskBuilderSchedulerPanelBar
			// 
		}
		#endregion

		//---------------------------------------------------------------------------
		public TaskBuilderSchedulerPanelCollection Panels
		{
			get
			{
				return panels;
			}
		}

		//---------------------------------------------------------------------------
		public int YSpacing
		{
			get
			{
				return ySpacing;
			}
			set
			{
				ySpacing = value;
				UpdateAllPanelsPositions();
			}
		}

		/// <summary>
		/// Gets/sets the vertical xSpacing between adjacent panels.
		/// </summary>
		//---------------------------------------------------------------------------
		public int XSpacing
		{
			get
			{
				return xSpacing;
			}
			set
			{
				xSpacing = value;
				UpdateAllPanelsPositions();
			}
		}

		/// <summary>
		/// Signals the object that initialization is starting.
		/// </summary>
		//---------------------------------------------------------------------------
		public void BeginInit()
		{
			initialising = true;
		}

		/// <summary>
		/// Signals the object that initialization is complete.
		/// </summary>
		//---------------------------------------------------------------------------
		public void EndInit()
		{
			initialising = false;
		}
		
		//---------------------------------------------------------------------------
		public void HideHorizontalScrollBar() 
		{ 
			this.HScroll = false;
		}

		//---------------------------------------------------------------------------
		public void UpdateAllPanelsPositions()
		{
			if (panels == null || panels.Count < 1)
				return;

			UpdatePositions(panels.Count - 1);

			HideHorizontalScrollBar();
		}

		//---------------------------------------------------------------------------
		private void UpdatePositions(int index)
		{
			if (initialising || this.DisplayRectangle.Width == 0 || panels == null || index < 0 || index >= panels.Count)
				return;

			if (PanelsPositionUpdating != null)
				PanelsPositionUpdating(this, this.DisplayRectangle.Width - (2 * xSpacing));
		
			for(int i = index; i >= 0; i--)
			{
				int panelTop = 0;;
				// Update the panel locations.
				if(i == panels.Count - 1)
				{
					// Top panel.
					panelTop = this.DisplayRectangle.Top + ySpacing;
				}
				else
				{
					if (!panels[i+1].Visible)
						panelTop = panels[i+1].Top;
					else
						panelTop = panels[i+1].Bottom + ySpacing;
				}
				
				panels[i].Location = new Point(xSpacing, panelTop);
				panels[i].Size =  new Size (this.DisplayRectangle.Width - (2 * xSpacing), panels[i].Height);
			}

			this.PerformLayout();

			this.UpdateBounds();


			if (PanelsPositionUpdated != null)
				PanelsPositionUpdated(this, this.DisplayRectangle.Width - (2 * xSpacing));
		}

		/// <summary>
		/// Event handler for the <see cref="Control.ControlAdded">ControlAdded</see> event.
		/// </summary>
		/// <param name="e">A <see cref="System.Windows.Forms.ControlEventArgs">ControlEventArgs</see> that contains the event data.</param>
		//---------------------------------------------------------------------------
		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);

			if(e.Control is TaskBuilderSchedulerPanel)
			{
				// Adjust the docking property to Left | Right | Top
				e.Control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
				
				((TaskBuilderSchedulerPanel)e.Control).PanelStateChanged +=	new PanelStateChangedEventHandler(panel_StateChanged);

				if(initialising)
				{
					// In the middle of InitializeComponent call.
					// Generated code adds panels in reverse order, so add to end
					panels.Add((TaskBuilderSchedulerPanel)e.Control);
				}
				else
				{
					// Add the panel to the beginning of the internal collection.
					panels.Insert(0, (TaskBuilderSchedulerPanel)e.Control);
				}
				// Update the size and position of the panels
				UpdateAllPanelsPositions();
			}
		}

		/// <summary>
		/// Event handler for the ControlRemoved event.
		/// </summary>
		/// <param name="e">A ControlEventArgs that contains the event data.</param>
		//---------------------------------------------------------------------------
		protected override void OnControlRemoved(ControlEventArgs e)
		{
			base.OnControlRemoved(e);

			if(e.Control is TaskBuilderSchedulerPanel)
			{
				// Get the index of the panel within the collection.
				int index = panels.IndexOf((TaskBuilderSchedulerPanel)e.Control);
				if (index != -1)
				{
					// Remove this panel from the collection.
					panels.Remove(index);
					// Update the position of any remaining panels.
					UpdateAllPanelsPositions();
				}
			}
		}

		//---------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			//AutoScrollPosition = new System.Drawing.Point(0,0);

			UpdateAllPanelsPositions();
		}
		
		//---------------------------------------------------------------------------
		private void panel_StateChanged(object sender, TaskBuilderSchedulerEventArgs e)
		{
			if (panels == null)
				return;

			// Get the index of the control that just changed state.
			int index = panels.IndexOf(e.TaskBuilderSchedulerPanel);
			if (index > 0 && index < panels.Count)
			{
				// Now update the position of all subsequent panels.
				UpdatePositions(--index);
			}
		}
	}

	//============================================================================
	public class TaskBuilderSchedulerLinklabel : System.Windows.Forms.LinkLabel
	{
		private System.Windows.Forms.ToolTip LinklabelToolTip;

		private const int xBorderOffset = 6;
		private string toolTipText = String.Empty;
		private bool underline = false;

		//---------------------------------------------------------------------------
		public 	TaskBuilderSchedulerLinklabel()
		{
			this.LinklabelToolTip = new System.Windows.Forms.ToolTip();
		}

		//---------------------------------------------------------------------------
		[Localizable(true)]
		public string ToolTipText 
		{
			get
			{
				if (toolTipText == null || toolTipText == String.Empty) 
					return this.Text;
				return toolTipText;
			}
			set
			{
				toolTipText = value;
				this.LinklabelToolTip.SetToolTip(this, toolTipText);
			}
		}

		//---------------------------------------------------------------------------
		private void PaintLabel(System.Drawing.Graphics graphics)
		{
			if (graphics == null)
				return;

			// Fill background;
			graphics.FillRectangle(new SolidBrush(this.BackColor), this.ClientRectangle);

			System.Drawing.RectangleF labelRectF = new System.Drawing.RectangleF(0.0f, 0.0f, (float)this.ClientRectangle.Width, (float)this.ClientRectangle.Height);

			if (this.ImageList != null && this.ImageIndex != -1)
			{
				System.Drawing.Image image = this.ImageList.Images[this.ImageIndex];
				if (image != null)
				{
					int destWidth = Math.Min(image.Width, this.Width);
					Rectangle destIconRect = new Rectangle(0, 0, destWidth,  Math.Min(image.Height, this.Height));

					graphics.DrawImage
						(
						image, 
						destIconRect
						);

					labelRectF = new System.Drawing.RectangleF((float)destWidth, 0.0f, (float)(this.ClientRectangle.Width - destWidth), (float)this.ClientRectangle.Height);
				}
			}
			
			System.Drawing.Color textColor = this.LinkColor;
			if (!this.Enabled) 
				textColor = this.DisabledLinkColor;
			else if (this.LinkVisited) 
				textColor = this.VisitedLinkColor;

			System.Drawing.Font linkLabelFont = new System.Drawing.Font(this.Font, underline ? System.Drawing.FontStyle.Underline : System.Drawing.FontStyle.Regular);

			StringFormat format = new StringFormat();
			format.Trimming = StringTrimming.EllipsisWord;
			format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;
			format.LineAlignment = StringAlignment.Center;
	
			graphics.DrawString(this.Text, linkLabelFont, new SolidBrush(textColor), labelRectF, format);		
		
			if (this.Focused)
			{
				System.Drawing.SizeF textSize = graphics.MeasureString(this.Text, linkLabelFont);
				// Devo centrare verticalmente il rettangolo nell'area client della
				// label visto cho ho specificato LineAlignment = StringAlignment.Center
				int focusRectHeight = Math.Min(this.ClientRectangle.Height - 2,(int)Math.Ceiling(textSize.Height));
				int focusRectWidth = Math.Min(this.ClientRectangle.Width - 2,(int)Math.Ceiling(textSize.Width));
				if (focusRectWidth > 0 && focusRectHeight > 0)
				{
					System.Drawing.Rectangle textRect = new Rectangle(1,(this.ClientRectangle.Height - focusRectHeight)/2, focusRectWidth, focusRectHeight); 
					ControlPaint.DrawFocusRectangle(graphics, textRect);
				}
			}
		}

		//---------------------------------------------------------------------------
		public int GetFittingWidth(TaskBuilderSchedulerPanel aPanel)
		{
			if (aPanel == null)
				return this.Width;

			System.Drawing.Rectangle labelRect = this.RectangleToScreen(this.ClientRectangle);
			System.Drawing.Point labelPoint = aPanel.PointToClient(new System.Drawing.Point(labelRect.Left, labelRect.Top));
			return aPanel.Width - labelPoint.X - xBorderOffset;
		}
		
		//---------------------------------------------------------------------------
		public void SetWidthToFitPanel(TaskBuilderSchedulerPanel aPanel)
		{
			if (aPanel != null)
				this.Width = GetFittingWidth(aPanel);
		}
		
		//---------------------------------------------------------------------------
		protected override void InitLayout()
		{
			base.InitLayout();

			underline = (this.LinkBehavior == System.Windows.Forms.LinkBehavior.AlwaysUnderline);
			
			this.AutoSize = false;

			this.LinklabelToolTip.SetToolTip(this, this.ToolTipText);
		}
		
		//---------------------------------------------------------------------------
		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			this.LinkArea = new System.Windows.Forms.LinkArea(0, this.Text.Length);

			this.LinklabelToolTip.SetToolTip(this, this.ToolTipText);
		}
		
		//---------------------------------------------------------------------------
		protected override void OnAutoSizeChanged(EventArgs e)
		{
			this.AutoSize = false;
			base.OnAutoSizeChanged(e);
		}
		
		//---------------------------------------------------------------------------
		protected override void OnMouseHover(EventArgs e)
		{
			base.OnMouseHover(e);

			underline = (this.LinkBehavior == System.Windows.Forms.LinkBehavior.AlwaysUnderline) ||
						(this.LinkBehavior == System.Windows.Forms.LinkBehavior.HoverUnderline);

			PaintLabel(Graphics.FromHwnd(this.Handle));		
		}
		
		//---------------------------------------------------------------------------
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			underline = (this.LinkBehavior == System.Windows.Forms.LinkBehavior.AlwaysUnderline);

			PaintLabel(Graphics.FromHwnd(this.Handle));		
		}
		
		//---------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			PaintLabel(e.Graphics);		
		}
		
		//---------------------------------------------------------------------------
		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			// Devo espandere il pannello dei comandi qualora esso risulti 
			// collassato, altrimenti non vedo dove sta correntemente il fuoco
			if 
				(
				this.Parent != null && 
				this.Parent is TaskBuilderSchedulerPanel &&
				((TaskBuilderSchedulerPanel)this.Parent).IsCollapsed
				)
				((TaskBuilderSchedulerPanel)this.Parent).Expand();

			this.Refresh();		
		}
		
		//---------------------------------------------------------------------------
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);

			this.Refresh();		
		}
		
	}
	
}