using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Microarea.EasyAttachment.UI.Controls
{
	// An abstract class that implements the functionality of an image button
	// except for a single abstract method to load the Normal, Hot and Disabled 
	// images that represent the icon that is displayed on the button. The loading
	// of these images is done in each derived concrete class.
	//================================================================================
	public abstract class DataGridViewImageButtonCell : DataGridViewButtonCell
	{
		private bool _enabled;                // Is the button enabled
		private PushButtonState _buttonState; // What is the button state
		protected Image _buttonImageHot;      // The hot image
		protected Image _buttonImageNormal;   // The normal image
		protected Image _buttonImageDisabled; // The disabled image
		private int _buttonImageOffset;       // The amount of offset or border around the image

		//--------------------------------------------------------------------------------
		protected DataGridViewImageButtonCell()
		{
			// In my project, buttons are enabled by default
			_enabled = true;
			_buttonState = PushButtonState.Normal;

			// Changing this value affects the appearance of the image on the button.
			_buttonImageOffset = 2;

			// Call the routine to load the images specific to a column.
			LoadImages();
		}

		// Button Enabled Property
		//--------------------------------------------------------------------------------
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;
				_buttonState = value ? PushButtonState.Normal : PushButtonState.Disabled;
			}
		}

		// PushButton State Property
		//--------------------------------------------------------------------------------
		public PushButtonState ButtonState { get { return _buttonState; } set { _buttonState = value; } }

		// Image Property
		// Returns the correct image based on the control's state.
		//--------------------------------------------------------------------------------
		public Image ButtonImage
		{
			get
			{
				switch (_buttonState)
				{
					case PushButtonState.Disabled:
						return _buttonImageDisabled;

					case PushButtonState.Hot:
						return _buttonImageHot;

					case PushButtonState.Normal:
						return _buttonImageNormal;

					case PushButtonState.Pressed:
						return _buttonImageNormal;

					case PushButtonState.Default:
						return _buttonImageNormal;

					default:
						return _buttonImageNormal;
				}
			}
		}

		//--------------------------------------------------------------------------------
		protected override void Paint(Graphics graphics,
					Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
					DataGridViewElementStates elementState, object value,
					object formattedValue, string errorText,
					DataGridViewCellStyle cellStyle,
					DataGridViewAdvancedBorderStyle advancedBorderStyle,
					DataGridViewPaintParts paintParts)
		{
			//base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
            SolidBrush cellBackground = null;

			// Draw the cell background, if specified.
			if ((paintParts & DataGridViewPaintParts.Background) ==	DataGridViewPaintParts.Background)
			{
				cellBackground = new SolidBrush(cellStyle.BackColor);
				graphics.FillRectangle(cellBackground, cellBounds);
				cellBackground.Dispose();
			}

			// Draw the cell borders, if specified.
			if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
				PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);

			// Calculate the area in which to draw the button.
			// Adjusting the following algorithm and values affects
			// how the image will appear on the button.
			Rectangle buttonArea = cellBounds;

			Rectangle buttonAdjustment = BorderWidths(advancedBorderStyle);

			buttonArea.X += buttonAdjustment.X;
			buttonArea.Y += buttonAdjustment.Y;
			buttonArea.Height -= buttonAdjustment.Height;
			buttonArea.Width -= buttonAdjustment.Width;

			Rectangle imageArea = new Rectangle(buttonArea.X + _buttonImageOffset, buttonArea.Y + _buttonImageOffset, 16, 16);

			ButtonRenderer.DrawButton(graphics, buttonArea, ButtonImage, imageArea, false, ButtonState);
            cellBackground.Dispose();
		}   

		// An abstract method that must be created in each derived class.
		// The images in the derived class will be loaded here.
		//--------------------------------------------------------------------------------
		public abstract void LoadImages();
	}
}