using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;


namespace Microarea.TaskBuilderNet.UI.WinControls
{
	//===========================================================================
	/// <summary>
	/// Summary description for TBToolBar.
	/// </summary>
	[DefaultProperty("Name"), DefaultEvent("ButtonClicked"), DesignerAttribute(typeof(TBToolBarDesigner))]
	public class TBToolBar : System.Windows.Forms.Control
	{
		public delegate void TBToolBarEventHandler(object sender, TBToolBarButton aButton);
	
		#region Events
		
		public event TBToolBarEventHandler ButtonClicked;
        public event TBToolBarEventHandler ButtonDropDown;
        public event TBToolBarEventHandler ButtonDropDownSelectedIndexChanged;
		
		#endregion

		#region Enums

		private enum ButtonState
		{
			Normal,
			Over,
			Down,
			Clicked
		}

		#endregion

		private TBToolBarButtonCollection buttons = new TBToolBarButtonCollection();
		
		internal const int XOffset = 8;
		private const int XButtonsSpace = 4;
		internal const int YOffset = 8;
		private const int YButtonsSpace = 4;
		private const int buttonBorderSize = 1;
		private const int separatorWidth = 3;
		
		private int			selectedButtonIndex = -1;
		private ButtonState	selectedButtonState = ButtonState.Normal;

		private System.Drawing.ContentAlignment contentAlignment = System.Drawing.ContentAlignment.MiddleLeft;
		private const int defaultSmallImageWidth = 24;
		private int smallImageWidth = defaultSmallImageWidth;
		private int smallImageHeight = defaultSmallImageWidth;
		private const int defaultLargeImageWidth = 48;
		private int largeImageWidth = defaultLargeImageWidth;
		private int largeImageHeight = defaultLargeImageWidth;
		private int defaultButtonWidth = defaultSmallImageWidth + buttonBorderSize * 2;
		private int defaultButtonHeight = defaultSmallImageWidth + buttonBorderSize * 2;
		
		private bool showSelectedButtonText = false;
		private Color selectedButtonTextColor = Color.DarkSlateBlue;
		private Color selectedButtonTextShadowColor = Color.Lavender;
		private Color linearGradientFirstColor = Color.Azure;
		private Color linearGradientSecondColor = Color.RoyalBlue;

		private System.Drawing.Imaging.ColorMatrix		disabledButtonColorMatrix;
		private System.Drawing.Imaging.ImageAttributes	disabledButtonImageAttributes;

		private DockStyle currentDockStyle = DockStyle.None;

		public static Color ToolBarBackgroundColor = Color.FromArgb(146, 168, 244);

		//---------------------------------------------------------------------------
		public TBToolBar()
		{
			this.SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint|System.Windows.Forms.ControlStyles.DoubleBuffer|System.Windows.Forms.ControlStyles.UserPaint|System.Windows.Forms.ControlStyles.ResizeRedraw|System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, true);
			
			this.Dock = currentDockStyle = System.Windows.Forms.DockStyle.Top;
			
			if (this.ApplyVerticalLayout)
				this.Width = defaultLargeImageWidth + (buttonBorderSize * 4);
			else
				this.Height = defaultLargeImageWidth + (buttonBorderSize * 4);

			// Setup the ColorMatrix and ImageAttributes for disabled images.
			disabledButtonColorMatrix = new ColorMatrix();
			disabledButtonColorMatrix.Matrix00 = 1/3f;
			disabledButtonColorMatrix.Matrix01 = 1/3f;
			disabledButtonColorMatrix.Matrix02 = 1/3f;
			disabledButtonColorMatrix.Matrix10 = 1/3f;
			disabledButtonColorMatrix.Matrix11 = 1/3f;
			disabledButtonColorMatrix.Matrix12 = 1/3f;
			disabledButtonColorMatrix.Matrix20 = 1/3f;
			disabledButtonColorMatrix.Matrix21 = 1/3f;
			disabledButtonColorMatrix.Matrix22 = 1/3f;
			disabledButtonColorMatrix.Matrix33 = 1/3f;
			disabledButtonColorMatrix.Matrix44 = 1/3f;
			disabledButtonImageAttributes = new ImageAttributes();
			disabledButtonImageAttributes.SetColorMatrix(disabledButtonColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			buttons.ButtonAdded += new TBToolBarButtonEventHandler(ButtonAdded);
		}

		#region TBToolBar protected overridden methods

		//---------------------------------------------------------------------------
		protected override void OnSizeChanged(EventArgs e)
		{
			if (this.ApplyVerticalLayout)
			{
				int minimumWidth = smallImageWidth + (buttonBorderSize + XOffset)* 2;
				if (buttons != null && buttons.HasDropDownMenus())
					minimumWidth += TBToolBarDropDownButton.DropDownButtonDefaultWidth + XOffset;
				if (minimumWidth > this.Width)
					this.Width = minimumWidth;
			}
			else
			{
				int minimumHeight = smallImageHeight + (buttonBorderSize + YOffset)* 2;
				if (minimumHeight > this.Height)
					this.Height = minimumHeight;
			}

			if (this.ApplyVerticalLayout)
			{
				int minimumWidth = largeImageWidth + (buttonBorderSize * 4);
				if (minimumWidth > this.Width)
					this.Width = minimumWidth;
			}
			else
			{
				int minimumHeight = largeImageHeight + (buttonBorderSize * 4);
				if (minimumHeight > this.Height)
					this.Height = minimumHeight;
			}
		
			base.OnSizeChanged (e);

			if (!this.DesignMode)
			{
				HideDropDownButtons();

				Application.DoEvents();

				RefreshDropDownsLocation(true);

				PerformLayout();
				Refresh();
			}
		}

		//---------------------------------------------------------------------------
		protected override void OnLocationChanged(EventArgs e)
		{
			base.OnLocationChanged (e);

			RefreshDropDownsLocation();
		}

		//---------------------------------------------------------------------------
		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout (levent);

			RefreshDropDownsLocation(true);
		}

		//---------------------------------------------------------------------------
		protected override void OnCreateControl()
		{
			base.OnCreateControl ();

			RefreshDropDownsLocation();
		}

		//---------------------------------------------------------------------------
		protected override void OnDockChanged(EventArgs e)
		{
			base.OnDockChanged (e);
		
			if (this.ApplyVerticalLayout)
			{
				linearGradientFirstColor = Color.RoyalBlue;
				linearGradientSecondColor = Color.Azure;
			
				int minimumWidth = smallImageWidth + (buttonBorderSize + XOffset)* 2;
				if (buttons != null && buttons.HasDropDownMenus())
					minimumWidth += TBToolBarDropDownButton.DropDownButtonDefaultWidth + XOffset;
				if (minimumWidth > this.Width || (currentDockStyle != DockStyle.Left && currentDockStyle != DockStyle.Right))
					this.Width = minimumWidth;
			}
			else
			{
				linearGradientFirstColor = Color.Azure;
				linearGradientSecondColor = Color.RoyalBlue;

				int minimumHeight = smallImageHeight + (buttonBorderSize + YOffset)* 2;
				if (minimumHeight > this.Height || (currentDockStyle == DockStyle.Left || currentDockStyle == DockStyle.Right))
					this.Height = minimumHeight;
			}

			currentDockStyle = this.Dock;
		}

		//---------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint (e);

			if (e == null || e.Graphics == null)
				return;

			System.Drawing.Rectangle bounds = this.ClientRectangle;
			if (bounds == System.Drawing.Rectangle.Empty)
				return;

			System.Drawing.Rectangle smallButtonsAreaRect;
			System.Drawing.Rectangle largeButtonsAreaRect;
			
			PaintBackground(e.Graphics, out smallButtonsAreaRect, out largeButtonsAreaRect);

			DrawButtons(e.Graphics, smallButtonsAreaRect, largeButtonsAreaRect);
		}
	
		//---------------------------------------------------------------------------
		protected override void OnMouseMove(MouseEventArgs e)
		{
			bool invalidate = false;

			int buttonIndex = GetButtonIndexFromPoint(new Point(e.X, e.Y));

			if(buttonIndex >= 0) // mouse over index = buttonIndex
			{
				invalidate = (buttonIndex != selectedButtonIndex);

				if (invalidate || selectedButtonState != ButtonState.Clicked)
				{
					selectedButtonState = (e.Button == System.Windows.Forms.MouseButtons.Left) ? ButtonState.Down : ButtonState.Over;
				}
				selectedButtonIndex = buttonIndex;
			}
			else // mouse over nothing
			{
				selectedButtonState = ButtonState.Normal;

				invalidate = (selectedButtonIndex != -1);
				selectedButtonIndex = -1;
			}

			base.OnMouseMove (e);

			if (invalidate)
				this.Invalidate();
		}
	
		//---------------------------------------------------------------------------
		protected override void OnMouseLeave(EventArgs e)
		{
			bool invalidate = (selectedButtonIndex >= 0 && selectedButtonIndex < buttons.Count);
			selectedButtonIndex = -1;

			base.OnMouseLeave (e);

			if(invalidate)
				this.Invalidate();
		}
		
		//---------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			bool invalidate = false;

			if (selectedButtonIndex >= 0 && selectedButtonIndex < buttons.Count)
			{
				invalidate = true;
				selectedButtonState = ButtonState.Down;
			}

			base.OnMouseDown (e);

			if(invalidate)
				this.Invalidate();
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp (e);

            if (selectedButtonIndex >= 0 && selectedButtonIndex < buttons.Count && buttons[selectedButtonIndex].Enabled)
            {
                Refresh();
            }
		}

        //---------------------------------------------------------------------------
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (selectedButtonIndex >= 0 && selectedButtonIndex < buttons.Count && buttons[selectedButtonIndex].Enabled)
            {
                if (buttons[selectedButtonIndex].Style == ToolBarButtonStyle.ToggleButton)
                    buttons[selectedButtonIndex].Pushed = !buttons[selectedButtonIndex].Pushed;

                if
                    (
                    (
                    buttons[selectedButtonIndex].Style == ToolBarButtonStyle.PushButton ||
                    buttons[selectedButtonIndex].Style == ToolBarButtonStyle.ToggleButton
                    ) &&
                    buttons[selectedButtonIndex].DropDownCombo == null
                    )
                    this.InvokeButtonClicked(buttons[selectedButtonIndex]);

                selectedButtonState = ButtonState.Clicked;
            }
        }
        
        #endregion

		#region TBToolBar private methods
		
		//---------------------------------------------------------------------------
		private void InvokeButtonClicked(TBToolBarButton aButton)
		{
			if (ButtonClicked != null)
				ButtonClicked(this, aButton);
		}
		
		//---------------------------------------------------------------------------
		private void ButtonAdded(TBToolBarButton addedButton)
		{
			if (addedButton == null || !buttons.Contains(addedButton))
				return;
			
			addedButton.InvalidateButton += new TBToolBarButtonEventHandler(InvalidateButton);
			addedButton.RefreshToolBar += new TBToolBarButtonEventHandler(RefreshToolBar);
            addedButton.DropDownComboDropDown += new TBToolBarButtonEventHandler(DropDownComboDropDown);
            addedButton.DropDownComboSelectedIndexChanged += new TBToolBarButtonEventHandler(DropDownComboSelectedIndexChanged);
		
			RefreshDropDownsLocation();
		}

		//---------------------------------------------------------------------------
        private Size GetButtonFixedTextSize(TBToolBarButton aToolBarButton)
        {
            if (aToolBarButton == null || String.IsNullOrEmpty(aToolBarButton.FixedText))
                return Size.Empty;

            Graphics g = CreateGraphics();

            SizeF buttonFixedTextSize = g.MeasureString(aToolBarButton.FixedText, (aToolBarButton.FixedTextFont != null) ? aToolBarButton.FixedTextFont : this.Font);

            g.Dispose();

            return new Size((int)Math.Ceiling(buttonFixedTextSize.Width), (int)Math.Ceiling(buttonFixedTextSize.Height));
        }

		//---------------------------------------------------------------------------
		private void InvalidateButton(TBToolBarButton aToolBarButton)
		{
			if (aToolBarButton == null || !aToolBarButton.Visible)
				return;

			int buttonIndex = buttons.IndexOf(aToolBarButton);
			if (buttonIndex == -1)
				return;

			System.Drawing.Rectangle smallButtonsAreaRect;
			System.Drawing.Rectangle largeButtonsAreaRect;
			CalculateButtonsAreas(out smallButtonsAreaRect, out largeButtonsAreaRect);
			if (smallButtonsAreaRect.Width == 0 || smallButtonsAreaRect.Height == 0)
				return;

			int nextButtonOffset = (this.ApplyVerticalLayout) ? smallButtonsAreaRect.Top : smallButtonsAreaRect.Left;
			
			for(int i = 0; i < buttonIndex ; i++)
			{
				if (!buttons[i].Visible)
					continue;

				if (this.ApplyVerticalLayout)
				{
					if (buttons[i].Style == ToolBarButtonStyle.Separator)
						nextButtonOffset += YOffset + separatorWidth;
					else
						nextButtonOffset += YOffset*2 + YButtonsSpace + defaultButtonHeight;
				}
				else
				{
					if (buttons[i].Style == ToolBarButtonStyle.Separator)
						nextButtonOffset += XOffset + separatorWidth;
					else
						nextButtonOffset += XOffset*2 + XButtonsSpace + defaultButtonWidth + buttons[i].GetAdditionalWidth();
				}

                nextButtonOffset += GetButtonFixedTextSize(buttons[i]).Width;
            }
		
			if (selectedButtonIndex == buttonIndex) // Upfront picture(magnified)
			{
				if (this.ApplyVerticalLayout)
					this.Invalidate(new System.Drawing.Rectangle(smallButtonsAreaRect.Left - largeImageHeight / 4 + 1, nextButtonOffset - largeImageHeight / 4 + 1, largeImageWidth, largeImageHeight));
				else
					this.Invalidate(new System.Drawing.Rectangle(nextButtonOffset - largeImageWidth / 4 + 1, smallButtonsAreaRect.Top - largeImageHeight / 4 + 1, largeImageWidth, largeImageHeight));
			}
			else 
			{			
				System.Drawing.Rectangle buttonRect = System.Drawing.Rectangle.Empty;
				if (ApplyVerticalLayout)
					buttonRect = new System.Drawing.Rectangle(smallButtonsAreaRect.Left, nextButtonOffset, smallImageWidth + 1, smallImageHeight + 1);
				else
					buttonRect = new System.Drawing.Rectangle(nextButtonOffset, smallButtonsAreaRect.Top, smallImageWidth + 1, smallImageHeight + 1);
				buttonRect.Inflate(2,2);
				
				Invalidate(buttonRect);
			}

			RefreshDropDownsLocation();
		}
		
		//---------------------------------------------------------------------------
		private void RefreshToolBar(TBToolBarButton aToolBarButton)
		{
			Refresh();
		}

        //---------------------------------------------------------------------------
        private void DropDownComboDropDown(TBToolBarButton aToolBarButton)
        {
            if (aToolBarButton == null || aToolBarButton.Style != ToolBarButtonStyle.DropDownButton)
                return;

            if (ButtonDropDown != null)
                ButtonDropDown(this, aToolBarButton);

        }

        //---------------------------------------------------------------------------
        private void DropDownComboSelectedIndexChanged(TBToolBarButton aToolBarButton)
        {
            if (aToolBarButton == null || aToolBarButton.Style != ToolBarButtonStyle.DropDownButton)
                return;

            if (ButtonDropDownSelectedIndexChanged != null)
                ButtonDropDownSelectedIndexChanged(this, aToolBarButton);
        }
        
        //---------------------------------------------------------------------------
		private void CalculateButtonsAreas(out System.Drawing.Rectangle smallButtonsAreaRect, out System.Drawing.Rectangle largeButtonsAreaRect)
		{
			smallButtonsAreaRect = new System.Drawing.Rectangle(0, 0, 0, 0);
			largeButtonsAreaRect = new System.Drawing.Rectangle(0, 0, 0, 0);

			if (buttons == null || buttons.Count == 0)
				return;

			int buttonsAreaWidth = 0;
			int buttonsAreaHeight = 0;

			if (ApplyVerticalLayout)
			{
				buttonsAreaWidth = smallImageWidth;

				int left = 0;
				int smallButtonTop = 0;
			
				if (buttons != null && buttons.Count > 0)
				{
					for(int i = 0; i < buttons.Count ; i++)
					{
						if (!buttons[i].Visible)
							continue;

						if (buttons[i].Style != ToolBarButtonStyle.Separator)
						{
							buttonsAreaHeight += YOffset*2 + defaultButtonHeight;
							if (i <(buttons.Count-1))
								buttonsAreaHeight += YButtonsSpace;

                            buttonsAreaHeight += GetButtonFixedTextSize(buttons[i]).Width;
                            
                            if (buttons[i].DropDownMenu != null && buttons[i].DropDownMenuButton != null)
							{
								int newButtonsAreaWidth = smallImageWidth + TBToolBarDropDownButton.DropDownButtonDefaultWidth + XOffset;
								if (newButtonsAreaWidth > buttonsAreaWidth)
									buttonsAreaWidth = newButtonsAreaWidth;
							}
						}
						else
							buttonsAreaHeight += YOffset + separatorWidth;
					}
				}

				switch(contentAlignment)
				{
					case System.Drawing.ContentAlignment.TopLeft:
						left = XOffset*2;
						smallButtonTop = 2*YOffset;
						break;

					case System.Drawing.ContentAlignment.TopCenter:
						left = Math.Max(2*XOffset, (this.ClientRectangle.Width - buttonsAreaWidth) / 2);
						smallButtonTop = 2*YOffset;
						break;

					case System.Drawing.ContentAlignment.TopRight:
						left = this.ClientRectangle.Width - buttonsAreaWidth - 2*XOffset;
						smallButtonTop = 2*YOffset;
						break;

					case System.Drawing.ContentAlignment.MiddleLeft:
						left = XOffset*2;
						smallButtonTop = (this.ClientRectangle.Height - buttonsAreaHeight)/ 2;
						break;

					case System.Drawing.ContentAlignment.MiddleCenter:
						left = Math.Max(2*XOffset, (this.ClientRectangle.Width - buttonsAreaWidth) / 2);
						smallButtonTop = (this.ClientRectangle.Height - buttonsAreaHeight)/ 2;
						break;

					case System.Drawing.ContentAlignment.MiddleRight:
						left = this.ClientRectangle.Width - buttonsAreaWidth - 2*XOffset;
						smallButtonTop = (this.ClientRectangle.Height - buttonsAreaHeight)/ 2;
						break;

					case System.Drawing.ContentAlignment.BottomLeft:
						left = XOffset*2;
						smallButtonTop = this.ClientRectangle.Height - buttonsAreaHeight - 2*YOffset;
						break;

					case System.Drawing.ContentAlignment.BottomCenter:
						left = Math.Max(2*XOffset, (this.ClientRectangle.Width - buttonsAreaWidth) / 2);
						smallButtonTop = this.ClientRectangle.Height - buttonsAreaHeight - 2*YOffset;
						break;

					case System.Drawing.ContentAlignment.BottomRight:
						left = this.ClientRectangle.Width - buttonsAreaWidth - 2*XOffset;
						smallButtonTop = this.ClientRectangle.Height - buttonsAreaHeight - 2*YOffset;
						break;
				}

				smallButtonsAreaRect = new System.Drawing.Rectangle(left, smallButtonTop, buttonsAreaWidth, buttonsAreaHeight);

				largeButtonsAreaRect = new System.Drawing.Rectangle(left - (largeImageWidth - smallImageWidth)/2, smallButtonTop, largeImageWidth, buttonsAreaHeight);
			}
			else
			{
				buttonsAreaHeight = smallImageHeight;
				
				int left = 0;
				int smallButtonTop = 0;
			
				if (buttons != null && buttons.Count > 0)
				{
					for(int i = 0; i < buttons.Count ; i++)
					{
						if (!buttons[i].Visible)
							continue;

						if (buttons[i].Style != ToolBarButtonStyle.Separator)
						{
							buttonsAreaWidth += XOffset*2 + defaultButtonWidth + buttons[i].GetAdditionalWidth();
							if (i <(buttons.Count-1))
								buttonsAreaWidth += XButtonsSpace;
                            buttonsAreaWidth += GetButtonFixedTextSize(buttons[i]).Width;
                        }
						else
							buttonsAreaWidth += XOffset + separatorWidth;
					}
					buttonsAreaWidth -= XOffset;
				}

				switch(contentAlignment)
				{
					case System.Drawing.ContentAlignment.TopLeft:
						left = XOffset*2;
						smallButtonTop = YOffset;
						break;

					case System.Drawing.ContentAlignment.TopCenter:
						left = (this.ClientRectangle.Width - buttonsAreaWidth) / 2;
						smallButtonTop = YOffset;
						break;

					case System.Drawing.ContentAlignment.TopRight:
						left = this.ClientRectangle.Width - buttonsAreaWidth - XOffset*2;
						smallButtonTop = YOffset;
						break;

					case System.Drawing.ContentAlignment.MiddleLeft:
						left = XOffset*2;
						smallButtonTop = (this.ClientRectangle.Height - smallImageHeight)/ 2;
						break;

					case System.Drawing.ContentAlignment.MiddleCenter:
						left = (this.ClientRectangle.Width - buttonsAreaWidth) / 2;
						smallButtonTop = (this.ClientRectangle.Height - smallImageHeight)/ 2;
						break;

					case System.Drawing.ContentAlignment.MiddleRight:
						left = this.ClientRectangle.Width - buttonsAreaWidth - XOffset*2;
						smallButtonTop = (this.ClientRectangle.Height - smallImageHeight)/ 2;
						break;

					case System.Drawing.ContentAlignment.BottomLeft:
						left = XOffset*2;
						smallButtonTop = this.ClientRectangle.Height - smallImageHeight - YOffset - 1;
						break;

					case System.Drawing.ContentAlignment.BottomCenter:
						left = (this.ClientRectangle.Width - buttonsAreaWidth) / 2;
						smallButtonTop = this.ClientRectangle.Height - smallImageHeight - YOffset - 1;
						break;

					case System.Drawing.ContentAlignment.BottomRight:
						left = this.ClientRectangle.Width - buttonsAreaWidth - XOffset*2;
						smallButtonTop = this.ClientRectangle.Height - smallImageHeight - YOffset - 1;
						break;
				}

				smallButtonsAreaRect = new System.Drawing.Rectangle(left, smallButtonTop, buttonsAreaWidth, smallImageHeight);

				largeButtonsAreaRect = new System.Drawing.Rectangle(left, smallButtonTop - (largeImageHeight - smallImageHeight)/2, buttonsAreaWidth, largeImageHeight);
			}
		}

		//---------------------------------------------------------------------------
		private void UpdateMinimumSize()
		{				
			if (ApplyVerticalLayout)
			{
					int minimumWidth = smallImageWidth + (buttonBorderSize + XOffset)* 2;
					if (buttons != null && buttons.HasDropDownMenus())
						minimumWidth += TBToolBarDropDownButton.DropDownButtonDefaultWidth + XOffset;
					if (minimumWidth > this.Width)
						this.Width = minimumWidth;
			}
			else
			{
					int minimumHeight = smallImageHeight + (buttonBorderSize + YOffset)* 2;
					if (minimumHeight > this.Height)
						this.Height = minimumHeight;
								
			}
			defaultButtonWidth = smallImageWidth + buttonBorderSize * 2;
			defaultButtonHeight = smallImageHeight + buttonBorderSize * 2;
		}

		//---------------------------------------------------------------------------
		private void PaintBackground()
		{
			System.Drawing.Rectangle smallButtonsAreaRect;
			System.Drawing.Rectangle largeButtonsAreaRect;

			System.Drawing.Graphics g = this.CreateGraphics();
			PaintBackground(g, out smallButtonsAreaRect, out largeButtonsAreaRect);
			g.Dispose();
		}
		
		//---------------------------------------------------------------------------
		private void PaintBackground(System.Drawing.Graphics g, out System.Drawing.Rectangle smallButtonsAreaRect, out System.Drawing.Rectangle largeButtonsAreaRect)
		{
			smallButtonsAreaRect = new System.Drawing.Rectangle(0, 0, 0, 0);
			largeButtonsAreaRect = new System.Drawing.Rectangle(0, 0, 0, 0);

			if (g == null)
				return;
		
			System.Drawing.Rectangle bounds = this.ClientRectangle;
			if (bounds == System.Drawing.Rectangle.Empty)
				return;

			CalculateButtonsAreas(out smallButtonsAreaRect, out largeButtonsAreaRect);

			//g.FillRectangle(System.Drawing.Brushes.CornflowerBlue, bounds);
			SolidBrush toolBarBackgroundBrush = new SolidBrush(ToolBarBackgroundColor);
			g.FillRectangle(toolBarBackgroundBrush, bounds);
			toolBarBackgroundBrush.Dispose();

			if (bounds.Width <= XOffset || bounds.Height <= YOffset)
				return;

			if (this.ApplyVerticalLayout)
				bounds.Inflate(-XOffset,-YOffset/2);
			else
				bounds.Inflate(-XOffset/2,-YOffset);

			const int diameter = 16;
			int radius = diameter / 2;
			// Create a GraphicsPath with curved top corners
			GraphicsPath path = new GraphicsPath();
			path.AddLine(bounds.Left + radius, bounds.Top, bounds.Right - radius -1, bounds.Top);
			path.AddArc(bounds.Right - diameter - 1, bounds.Top, diameter, diameter, 270, 90);
			path.AddLine(bounds.Right - 1, bounds.Top + radius, bounds.Right - 1, bounds.Bottom - radius -1);
			path.AddArc(bounds.Right - diameter - 1, bounds.Bottom - diameter - 1, diameter, diameter, 0, 90);
			path.AddLine(bounds.Right - radius - 1, bounds.Bottom - 1 , bounds.Left + radius, bounds.Bottom - 1);
			path.AddArc(bounds.Left, bounds.Bottom - diameter - 1, diameter, diameter, 90, 90);
			path.AddLine(bounds.Left, bounds.Bottom - radius - 1, bounds.Left, bounds.Top + radius + 1);
			path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);

			g.SmoothingMode = SmoothingMode.AntiAlias;
			
			LinearGradientBrush brush = new LinearGradientBrush(bounds, LinearGradientFirstColor, LinearGradientSecondColor, this.ApplyVerticalLayout ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical);
				
			g.FillPath(brush, path);

			brush.Dispose();

			g.DrawPath(System.Drawing.Pens.LightSteelBlue, path);

            path.Dispose();
		}

		//---------------------------------------------------------------------------
		private void DrawButtons(System.Drawing.Graphics g, System.Drawing.Rectangle smallButtonsAreaRect, System.Drawing.Rectangle largeButtonsAreaRect)
		{
			if (buttons == null || buttons.Count == 0)
				return;

			System.Drawing.Rectangle buttonsArea = this.ClientRectangle;
			buttonsArea.Inflate(-XOffset, -YOffset);

			int nextButtonOffset = ApplyVerticalLayout ? smallButtonsAreaRect.Top : smallButtonsAreaRect.Left;

			System.Drawing.Rectangle selSmallDestRect = System.Drawing.Rectangle.Empty;
			System.Drawing.Rectangle selLargeDestRect = System.Drawing.Rectangle.Empty;

			for(int i = 0; i < buttons.Count ; i++)
			{
				if (!buttons[i].Visible)
					continue;

				System.Drawing.Rectangle smalldestRect = System.Drawing.Rectangle.Empty;
				if (ApplyVerticalLayout)
					smalldestRect = new System.Drawing.Rectangle(smallButtonsAreaRect.Left, nextButtonOffset, smallImageWidth + 1, smallImageHeight + 1);
				else
					smalldestRect = new System.Drawing.Rectangle(nextButtonOffset, smallButtonsAreaRect.Top, smallImageWidth + 1, smallImageHeight + 1);

                int fixedTextWidth = 0;
                if (!String.IsNullOrEmpty(buttons[i].FixedText))
                {
                    Size fixedTextSize = GetButtonFixedTextSize(buttons[i]);
                    fixedTextWidth = fixedTextSize.Width;

                    Rectangle fixedTextRect = Rectangle.Empty;
                    if (ApplyVerticalLayout)
                        fixedTextRect = new System.Drawing.Rectangle(smallButtonsAreaRect.Left + (smallButtonsAreaRect.Width - fixedTextSize.Height) / 2, smalldestRect.Bottom, fixedTextSize.Height, fixedTextWidth);
                    else
                        fixedTextRect = new System.Drawing.Rectangle(smalldestRect.Right, smallButtonsAreaRect.Top + (smallButtonsAreaRect.Height - fixedTextSize.Height) / 2, fixedTextWidth, fixedTextSize.Height);

                    StringFormat fixedTextFormat = new StringFormat();
                    fixedTextFormat.Alignment = StringAlignment.Center;
                    fixedTextFormat.LineAlignment = StringAlignment.Center;
                    fixedTextFormat.FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap;
                    if (ApplyVerticalLayout)
                        fixedTextFormat.FormatFlags |= StringFormatFlags.DirectionVertical;

                    Color fixedTextColor = (!buttons[i].FixedTextColor.IsEmpty) ? buttons[i].FixedTextColor : this.ForeColor;
                    SolidBrush fixedTextBrush = new SolidBrush(fixedTextColor);

                    g.DrawString
                        (
                        buttons[i].FixedText,
                        (buttons[i].FixedTextFont != null) ? buttons[i].FixedTextFont : this.Font,
                        fixedTextBrush,
                        fixedTextRect,
                        fixedTextFormat
                    );
                    fixedTextBrush.Dispose();
                }
				
                if 
					(
					selectedButtonIndex == i && 
					selectedButtonState != ButtonState.Clicked &&
					buttons[i].Enabled && 
					buttons[i].Style != ToolBarButtonStyle.Separator
					) // Draw upfront picture(scaled)
				{
					selSmallDestRect = smalldestRect;
					
					if (ApplyVerticalLayout)
						selLargeDestRect = new System.Drawing.Rectangle(smallButtonsAreaRect.Left - largeImageHeight / 4 + 1, nextButtonOffset - largeImageHeight / 4 + 1, largeImageWidth, largeImageHeight);
					else
						selLargeDestRect = new System.Drawing.Rectangle(nextButtonOffset - largeImageWidth / 4 + 1, smallButtonsAreaRect.Top - largeImageHeight / 4 + 1, largeImageWidth, largeImageHeight);
					
					if (selLargeDestRect.Left < 0)
						selLargeDestRect.Offset(- selLargeDestRect.Left, 0);
					else if (selLargeDestRect.Right > this.ClientRectangle.Right)
						selLargeDestRect.Offset(- (selLargeDestRect.Right - this.ClientRectangle.Right), 0);
					
					if (ApplyVerticalLayout)
						nextButtonOffset += YOffset*2 + YButtonsSpace + defaultButtonHeight;
					else
						nextButtonOffset += XOffset*2 + XButtonsSpace + defaultButtonWidth + buttons[i].GetAdditionalWidth();

                    nextButtonOffset += fixedTextWidth;
                }
				else // Draw normally
				{                    
                    if (buttons[i].Style != ToolBarButtonStyle.Separator)
					{
						if (buttonsArea.Contains(smalldestRect))
						{
							int additionalButtonWidth = buttons[i].GetAdditionalWidth(this.ApplyHorizontalLayout);
							
                            if (additionalButtonWidth >= 0 || (smalldestRect.Right + additionalButtonWidth) <= buttonsArea.Right)
							{
								if (buttons[i].Style == ToolBarButtonStyle.ToggleButton && buttons[i].Pushed)
								{					
									// Draw border of button
									Rectangle buttonRect = smalldestRect;
									buttonRect.Inflate(2,2);
									const int diameter = 8;
									int radius = diameter / 2;
									// Create a GraphicsPath with curved top corners
									GraphicsPath buttonBordersPath = new GraphicsPath();
									buttonBordersPath.AddLine(buttonRect.Left + radius, buttonRect.Top, buttonRect.Right - radius -1, buttonRect.Top);
									buttonBordersPath.AddArc(buttonRect.Right - diameter - 1, buttonRect.Top, diameter, diameter, 270, 90);
									buttonBordersPath.AddLine(buttonRect.Right - 1, buttonRect.Top + radius, buttonRect.Right - 1, buttonRect.Bottom - radius -1);
									buttonBordersPath.AddArc(buttonRect.Right - diameter - 1, buttonRect.Bottom - diameter - 1, diameter, diameter, 0, 90);
									buttonBordersPath.AddLine(buttonRect.Right - radius - 1, buttonRect.Bottom - 1 , buttonRect.Left + radius, buttonRect.Bottom - 1);
									buttonBordersPath.AddArc(buttonRect.Left, buttonRect.Bottom - diameter - 1, diameter, diameter, 90, 90);
									buttonBordersPath.AddLine(buttonRect.Left, buttonRect.Bottom - radius - 1, buttonRect.Left, buttonRect.Top + radius + 1);
									buttonBordersPath.AddArc(buttonRect.Left, buttonRect.Top, diameter, diameter, 180, 90);

									g.SmoothingMode = SmoothingMode.AntiAlias;
									if (buttons[i].Enabled)
									{							
										LinearGradientBrush brush = new LinearGradientBrush(buttonRect, Color.Cornsilk, Color.DarkOrange, LinearGradientMode.Vertical);
				
										g.FillPath(brush, buttonBordersPath);
						
										brush.Dispose();

										g.DrawPath(System.Drawing.Pens.Gold, buttonBordersPath);				
									}
									else
									{
										LinearGradientBrush brush = new LinearGradientBrush(buttonRect, Color.Gainsboro, Color.DarkGray, LinearGradientMode.Vertical);
				
										g.FillPath(brush, buttonBordersPath);

										brush.Dispose();

										g.DrawPath(System.Drawing.Pens.LightSteelBlue, buttonBordersPath);
									}
                                    buttonBordersPath.Dispose();
								}
								// Draw image
								if (buttons[i].Icon != null)
								{
									Icon tmpIcon = new Icon(buttons[i].Icon, smallImageWidth, smallImageHeight);
									if (!buttons[i].Enabled)
									{
										Bitmap disabledImage = tmpIcon.ToBitmap();
										
										g.DrawImage
											(
											disabledImage, 
											new Rectangle(smalldestRect.Left, smalldestRect.Top, smallImageWidth, smallImageHeight), 
											0, 0, smallImageWidth, smallImageHeight, 
											GraphicsUnit.Pixel, 
											disabledButtonImageAttributes
											);
									}
									else
										g.DrawIcon(tmpIcon, smalldestRect);

									tmpIcon.Dispose();
								}	
							}
						}
						if (ApplyVerticalLayout)
							nextButtonOffset += YOffset * 2 + YButtonsSpace + defaultButtonHeight;
						else
							nextButtonOffset += XOffset * 2 + XButtonsSpace + defaultButtonWidth + buttons[i].GetAdditionalWidth();

                        nextButtonOffset += fixedTextWidth;
                    }
					else
					{
						if (ApplyVerticalLayout)
						{
							if (buttonsArea.Top < (nextButtonOffset -2) && buttonsArea.Bottom > (nextButtonOffset - 1))
							{
								Pen separatorShadowPen = new Pen(Color.RoyalBlue, (float)2);
								g.DrawLine(separatorShadowPen, new Point(smalldestRect.Left - 1, nextButtonOffset - YOffset/2 - 1), new Point(smalldestRect.Right, nextButtonOffset - YOffset/2 - 1));
								separatorShadowPen.Dispose();

								Pen separatorLightPen = new Pen(Color.LightSteelBlue, (float)2);
								g.DrawLine(separatorLightPen, new Point(smalldestRect.Left - 1, nextButtonOffset - YOffset/2 - 2), new Point(smalldestRect.Right + 1, nextButtonOffset - YOffset/2 - 2));
								separatorLightPen.Dispose();
							}
							nextButtonOffset += YOffset + separatorWidth;
						}
						else
						{
							if (buttonsArea.Left < (nextButtonOffset + XOffset + 1) && buttonsArea.Right > (nextButtonOffset + XOffset + 1))
							{
								Pen separatorShadowPen = new Pen(Color.RoyalBlue, (float)2);
								g.DrawLine(separatorShadowPen, new Point(nextButtonOffset - XOffset/2 + 1, smalldestRect.Top - 1), new Point(nextButtonOffset - XOffset/2 + 1, smalldestRect.Bottom));
								separatorShadowPen.Dispose();

								Pen separatorLightPen = new Pen(Color.LightSteelBlue, (float)2);
								g.DrawLine(separatorLightPen, new Point(nextButtonOffset - XOffset/2, smalldestRect.Top - 1), new Point(nextButtonOffset - XOffset/2, smalldestRect.Bottom + 1));
								separatorLightPen.Dispose();
							}
							nextButtonOffset += XOffset + separatorWidth;
						}
					}
				}
			}
		
			if (selectedButtonIndex >= 0 && buttons[selectedButtonIndex] != null && buttons[selectedButtonIndex].Enabled)
			{
				if (selLargeDestRect != System.Drawing.Rectangle.Empty && buttonsArea.Contains(selSmallDestRect))
				{
                    if (buttons[selectedButtonIndex].DropDownMenuButton != null)
                    {
                        GraphicsPath buttonBordersPath = buttons[selectedButtonIndex].DropDownMenuButton.GetButtonBordersPath();
                        if (buttonBordersPath != null)
                        {
                            Region regionToExclude = new Region(buttonBordersPath);
                            regionToExclude.Translate(buttons[selectedButtonIndex].DropDownMenuButton.Location.X, buttons[selectedButtonIndex].DropDownMenuButton.Location.Y);

                            g.ExcludeClip(regionToExclude);

                            regionToExclude.Dispose();
                            buttonBordersPath.Dispose();
                        }
                    }
                    if (buttons[selectedButtonIndex].Icon != null)
						g.DrawIcon(new Icon(buttons[selectedButtonIndex].Icon, largeImageWidth, largeImageWidth), selLargeDestRect);

					if (showSelectedButtonText && buttons[selectedButtonIndex].Text != null && buttons[selectedButtonIndex].Text.Length > 0)
					{
						g.TextRenderingHint = TextRenderingHint.AntiAlias;

						StringFormat buttonTextFormat = new StringFormat();
						buttonTextFormat.Alignment = StringAlignment.Far;
						buttonTextFormat.LineAlignment = StringAlignment.Center;

						buttonTextFormat.FormatFlags |= StringFormatFlags.NoWrap;
						if (ApplyVerticalLayout)
							buttonTextFormat.FormatFlags |= StringFormatFlags.DirectionVertical;

						SolidBrush textBrush = new SolidBrush(Color.FromArgb(194,selectedButtonTextColor));
						Font buttonTextFont = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold);
						
						System.Drawing.Rectangle textRect = Rectangle.Empty;
						do
						{
							System.Drawing.SizeF textSize = g.MeasureString(buttons[selectedButtonIndex].Text, buttonTextFont);
						
							if (ApplyVerticalLayout)
							{
								textRect = new Rectangle(0,0, buttonTextFont.Height, (int)Math.Ceiling(textSize.Width) + 2);
								textRect.Offset(XOffset/2, selLargeDestRect.Top + (selLargeDestRect.Height - textRect.Height)/2);
							}
							else
							{
								textRect = new Rectangle(0,0, (int)Math.Ceiling(textSize.Width) + 2, buttonTextFont.Height);
								textRect.Offset(selLargeDestRect.Left + (selLargeDestRect.Width - textRect.Width)/2, selLargeDestRect.Bottom - textRect.Height - 2);
							}

							Point textRectLocation = textRect.Location;
							if (textRect.Left < 0)
								textRectLocation.X = 0;
							if (textRect.Right > this.ClientRectangle.Right)
								textRectLocation.X = this.ClientRectangle.Right - textRect.Width;
							if (textRect.Top < 0)
								textRectLocation.Y = 0;
							if (textRect.Bottom > this.ClientRectangle.Height)
								textRectLocation.Y = this.ClientRectangle.Bottom - textRect.Height;
							if (textRect.Left != textRectLocation.X || textRect.Top != textRectLocation.Y)
								textRect.Location = textRectLocation;

							if (this.ClientRectangle.Contains(textRect) || buttonTextFont.Size <= 0)
								break;
							
                            buttonTextFont = new Font(this.Font.FontFamily, buttonTextFont.Size - 1, FontStyle.Bold);

						} while (true);
												
						if (selectedButtonTextShadowColor != Color.Empty)
						{
							textRect.Offset(1, 1);

                            SolidBrush textShadowBrush = new SolidBrush(Color.FromArgb(212, selectedButtonTextShadowColor));
							
                            g.DrawString(buttons[selectedButtonIndex].Text, buttonTextFont, textShadowBrush, textRect, buttonTextFormat);
							
                            textShadowBrush.Dispose();
							
                            textRect.Offset(-1, -1);
						}

						g.DrawString(buttons[selectedButtonIndex].Text, buttonTextFont, textBrush, textRect, buttonTextFormat);
						
						buttonTextFont.Dispose();
						textBrush.Dispose();
					}
                    g.ResetClip();

				}
			}
		}

		//---------------------------------------------------------------------------
		public void RefreshDropDownsLocation()
		{
			RefreshDropDownsLocation(false);
		}
		
		//---------------------------------------------------------------------------
		internal void RefreshDropDownsLocation(bool redrawDropDownButtons)
		{
			if (buttons == null || buttons.Count == 0)
				return;

			System.Drawing.Rectangle smallButtonsAreaRect;
			System.Drawing.Rectangle largeButtonsAreaRect;

			CalculateButtonsAreas(out smallButtonsAreaRect, out largeButtonsAreaRect);
			if (smallButtonsAreaRect.Width == 0 || smallButtonsAreaRect.Height == 0)
				return;
					
			int nextButtonOffset = (this.ApplyVerticalLayout) ? smallButtonsAreaRect.Top : smallButtonsAreaRect.Left;
			
			for(int i = 0; i < buttons.Count ; i++)
			{
				if (!buttons[i].Visible)
					continue;

				if (buttons[i].DropDownMenu != null && buttons[i].DropDownMenuButton != null)
				{
					if (!this.Controls.Contains(buttons[i].DropDownMenuButton))
						this.Controls.Add(buttons[i].DropDownMenuButton);

					int buttonXPos = 0;
					int buttonYPos = 0;
					if (this.ApplyVerticalLayout)
					{
						if (buttons[i].DropDownMenuButton.Height != smallImageHeight)
							buttons[i].DropDownMenuButton.Height = smallImageHeight;
						buttonXPos = smallButtonsAreaRect.Left + defaultButtonWidth + XOffset/2;
						buttonYPos = nextButtonOffset;
					}
					else
					{
						if (buttons[i].DropDownMenuButton.Height != smallButtonsAreaRect.Height)
							buttons[i].DropDownMenuButton.Height = smallButtonsAreaRect.Height;
						buttonXPos = nextButtonOffset + defaultButtonWidth + XOffset/2;
						buttonYPos = smallButtonsAreaRect.Top;
					}

					if (buttons[i].DropDownMenuButton.Left != buttonXPos || buttons[i].DropDownMenuButton.Top != buttonYPos)
						buttons[i].DropDownMenuButton.Location = new Point(buttonXPos, buttonYPos);
					
					bool isButtonVisible = false;
					if (this.ApplyVerticalLayout)
						isButtonVisible = (buttonXPos >= YOffset && (buttonYPos + buttons[i].DropDownMenuButton.Height + XOffset - 2) < this.ClientRectangle.Bottom);
					else				
						isButtonVisible = (buttonXPos >= XOffset && (buttonXPos + buttons[i].DropDownMenuButton.Width + XOffset - 2) < this.ClientRectangle.Right);
					if (isButtonVisible != buttons[i].DropDownMenuButton.Visible)
					{
						if (redrawDropDownButtons)
							buttons[i].DropDownMenuButton.Refresh();
						buttons[i].DropDownMenuButton.Visible = isButtonVisible;
					}
				}
				else if (buttons[i].Style == ToolBarButtonStyle.DropDownButton && buttons[i].DropDownCombo != null)
				{
					if (this.ApplyHorizontalLayout)
					{
						if (!this.Controls.Contains(buttons[i].DropDownCombo))
							this.Controls.Add(buttons[i].DropDownCombo);

                        buttons[i].DropDownCombo.Font = this.Font;
                        buttons[i].DropDownCombo.ForeColor = this.ForeColor;

                        int comboXPos = nextButtonOffset + defaultButtonWidth + GetButtonFixedTextSize(buttons[i]).Width + TBToolBarButton.DropDownXOffset;
						int comboYPos = smallButtonsAreaRect.Top + (smallButtonsAreaRect.Height - buttons[i].DropDownCombo.Height)/2;
						if (buttons[i].DropDownCombo.Left != comboXPos || buttons[i].DropDownCombo.Top != comboYPos)
							buttons[i].DropDownCombo.Location = new Point(comboXPos, comboYPos);
						
						buttons[i].DropDownCombo.Visible = (comboXPos >= XOffset && (comboXPos + buttons[i].DropDownWidth + XOffset*2) < this.ClientRectangle.Right);
					}
					else
						buttons[i].DropDownCombo.Visible = false;
				}

				if (this.ApplyVerticalLayout)
				{
					if (buttons[i].Style == ToolBarButtonStyle.Separator)
						nextButtonOffset += YOffset + separatorWidth;
					else
						nextButtonOffset += YOffset*2 + YButtonsSpace + defaultButtonHeight;
				}
				else
				{
					if (buttons[i].Style == ToolBarButtonStyle.Separator)
						nextButtonOffset += XOffset + separatorWidth;
					else
						nextButtonOffset += XOffset*2 + XButtonsSpace + defaultButtonWidth + buttons[i].GetAdditionalWidth();

                    nextButtonOffset += GetButtonFixedTextSize(buttons[i]).Width;
                }
			}
		}

		//---------------------------------------------------------------------------
		private void HideDropDownButtons()
		{
			if (buttons == null || buttons.Count == 0)
				return;

			for(int i = 0; i < buttons.Count ; i++)
			{
				if (!buttons[i].Visible)
					continue;

				if (buttons[i].DropDownMenu != null && buttons[i].DropDownMenuButton != null)
					buttons[i].DropDownMenuButton.Visible = false;
			}
		}
		
		#endregion

		#region TBToolBar public methods
		
		//---------------------------------------------------------------------------
		public System.Drawing.Rectangle GetButtonRectangle(int aButtonIndex)
		{
			if (buttons == null || buttons.Count == 0 || aButtonIndex < 0 || aButtonIndex >= buttons.Count || !buttons[aButtonIndex].Visible)
				return System.Drawing.Rectangle.Empty;

			System.Drawing.Rectangle smallButtonsAreaRect;
			System.Drawing.Rectangle largeButtonsAreaRect;
			CalculateButtonsAreas(out smallButtonsAreaRect, out largeButtonsAreaRect);
			
			if (smallButtonsAreaRect.Width == 0 || smallButtonsAreaRect.Height == 0)
				return System.Drawing.Rectangle.Empty;
			
			int nextButtonOffset = this.ApplyVerticalLayout ? smallButtonsAreaRect.Top : smallButtonsAreaRect.Left;
			
			for(int i = 0; i < aButtonIndex ; i++)
			{
				if (!buttons[i].Visible)
					continue;

				if (this.ApplyVerticalLayout)
				{
					if (buttons[i].Style == ToolBarButtonStyle.Separator)
						nextButtonOffset += YOffset + separatorWidth;
					else
						nextButtonOffset += YOffset*2 + YButtonsSpace + defaultButtonHeight;
				}
				else
				{
					if (buttons[i].Style == ToolBarButtonStyle.Separator)
						nextButtonOffset += XOffset + separatorWidth;
					else
						nextButtonOffset += XOffset*2 + XButtonsSpace + defaultButtonWidth + buttons[i].GetAdditionalWidth();

                    nextButtonOffset += GetButtonFixedTextSize(buttons[i]).Width;
                }
			}
		
			if (ApplyVerticalLayout)
				return new System.Drawing.Rectangle(smallButtonsAreaRect.Left, nextButtonOffset, smallImageWidth + 1, smallImageHeight + 1);
			
			return new System.Drawing.Rectangle(nextButtonOffset, smallButtonsAreaRect.Top, smallImageWidth + 1, smallImageHeight + 1);
		}
		
		//---------------------------------------------------------------------------
		public int GetButtonIndexFromPoint(System.Drawing.Point aPoint)
		{
			if 
				(
				buttons == null || 
				buttons.Count == 0 || 
				aPoint == Point.Empty || 
				!this.ClientRectangle.Contains(aPoint)
				)
				return -1;
			
			System.Drawing.Rectangle smallButtonsAreaRect;
			System.Drawing.Rectangle largeButtonsAreaRect;
			CalculateButtonsAreas(out smallButtonsAreaRect, out largeButtonsAreaRect);
			if (smallButtonsAreaRect.Width == 0 || smallButtonsAreaRect.Height == 0 || !smallButtonsAreaRect.Contains(aPoint))
				return -1;

			int nextButtonOffset = this.ApplyVerticalLayout ? smallButtonsAreaRect.Top : smallButtonsAreaRect.Left;
			
			for(int i = 0; i < buttons.Count ; i++)
			{
				if (!buttons[i].Visible)
					continue;

				if (this.ApplyVerticalLayout)
				{
					int currentButtonSpace = YOffset;
					if (buttons[i].Style == ToolBarButtonStyle.Separator)
						currentButtonSpace += separatorWidth;
					else
						currentButtonSpace += defaultButtonHeight;

					if (nextButtonOffset <= aPoint.Y && aPoint.Y <= (nextButtonOffset + currentButtonSpace))
						return i;
					
					if (buttons[i].Style == ToolBarButtonStyle.Separator)
						nextButtonOffset += currentButtonSpace;
					else
						nextButtonOffset += YOffset + YButtonsSpace + currentButtonSpace;

				}
				else
				{
					int currentButtonSpace = XOffset;
					if (buttons[i].Style == ToolBarButtonStyle.Separator)
						currentButtonSpace += separatorWidth;
					else
						currentButtonSpace += defaultButtonWidth;

					if (nextButtonOffset <= aPoint.X && aPoint.X <= (nextButtonOffset + currentButtonSpace))
						return i;
					
					if (buttons[i].Style == ToolBarButtonStyle.Separator)
						nextButtonOffset += currentButtonSpace;
					else
						nextButtonOffset += XOffset + XButtonsSpace + currentButtonSpace + buttons[i].GetAdditionalWidth();

                    nextButtonOffset += GetButtonFixedTextSize(buttons[i]).Width;
                }
			}

			return -1;
		}
	
		//---------------------------------------------------------------------------
		public System.Drawing.Rectangle GetButtonsRectangle()
		{
			if (buttons == null || buttons.Count == 0)
				return System.Drawing.Rectangle.Empty;

			System.Drawing.Rectangle smallButtonsAreaRect;
			System.Drawing.Rectangle largeButtonsAreaRect;
			CalculateButtonsAreas(out smallButtonsAreaRect, out largeButtonsAreaRect);
			
			if (smallButtonsAreaRect.Width == 0 || smallButtonsAreaRect.Height == 0)
				return System.Drawing.Rectangle.Empty;

			return smallButtonsAreaRect;
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Size GetMinimumSize()
		{
			System.Drawing.Rectangle buttonRect = GetButtonsRectangle();
			
			return new System.Drawing.Size(buttonRect.Width + 4*XOffset, buttonRect.Height + 4*YOffset);
		}
		
		#endregion

		#region TBToolBar public properties

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		public int SmallImageWidth 
		{
			get { return smallImageWidth; } 

			set 
			{
				if (value <= 0 || smallImageWidth == value)
					return;

				smallImageWidth = value;

				if (largeImageWidth < smallImageWidth)
					largeImageWidth = smallImageWidth;
			
				if (ApplyVerticalLayout)
				{
					int minimumWidth = smallImageWidth + (buttonBorderSize + XOffset)* 2;
					if (buttons != null && buttons.HasDropDownMenus())
						minimumWidth += TBToolBarDropDownButton.DropDownButtonDefaultWidth + XOffset;
					if (minimumWidth > this.Width)
						this.Width = minimumWidth;
				}

				defaultButtonWidth = smallImageWidth + buttonBorderSize * 2;

				Invalidate();
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		public int SmallImageHeight
		{
			get { return smallImageHeight; } 

			set 
			{
				if (value <= 0 || smallImageHeight == value)
					return;

				smallImageHeight = value;

				if (largeImageHeight < smallImageHeight)
					largeImageHeight = smallImageHeight;
			
				if (ApplyHorizontalLayout)
				{
					int minimumHeight = smallImageHeight + (buttonBorderSize + YOffset)* 2;
					if (minimumHeight > this.Height)
						this.Height = minimumHeight;
				}
		
				defaultButtonHeight = smallImageHeight + buttonBorderSize * 2;
				
				Invalidate();
			}
		}

		//---------------------------------------------------------------------------
		[Category("Appearance")]
		public int LargeImageWidth 
		{
			get { return largeImageWidth; } 

			set 
			{
				if (value <= 0 || largeImageWidth == value)
					return;

				largeImageWidth = (value > smallImageWidth)? value : smallImageWidth;
			
				if (ApplyVerticalLayout)
				{
					int minimumWidth = largeImageWidth + (buttonBorderSize * 4);
					if (minimumWidth != this.Width)
						this.Width = minimumWidth;
				}
				Invalidate();
			}
		}
		
		//---------------------------------------------------------------------------
		[Category("Appearance")]
		public int LargeImageHeight
		{
			get { return largeImageHeight; } 

			set 
			{
				if (value <= 0 || largeImageHeight == value)
					return;

				largeImageHeight = (value > smallImageHeight)? value : smallImageHeight;
			
				if (ApplyHorizontalLayout)
				{
					int minimumHeight = largeImageHeight + (buttonBorderSize * 4);
					if (minimumHeight != this.Height)
						this.Height = minimumHeight;
				}
				Invalidate();
			}
		}

		//---------------------------------------------------------------------------
		[Category("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TBToolBarButtonCollection Buttons
		{
			get { return buttons; }
			set 
			{
				buttons = value;

				if (this.ApplyVerticalLayout)
				{
					int minimumWidth = smallImageWidth + (buttonBorderSize + XOffset)* 2;
					if (buttons != null && buttons.HasDropDownMenus())
						minimumWidth += TBToolBarDropDownButton.DropDownButtonDefaultWidth + XOffset;
					if (minimumWidth > this.Width)
						this.Width = minimumWidth;
				}

				RefreshDropDownsLocation(true);

				PerformLayout();
				Refresh();
			}
		}

		//---------------------------------------------------------------------------
		[System.ComponentModel.Category("Behavior")]
		public System.Drawing.ContentAlignment ContentAlignment
		{
			get { return contentAlignment; }
			set
			{
				contentAlignment = value;

				HideDropDownButtons();

				Application.DoEvents();

				RefreshDropDownsLocation(true);

				PerformLayout();
				Refresh();
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public bool ApplyVerticalLayout
		{
			get { return (this.Dock == DockStyle.Left || this.Dock == DockStyle.Right); }
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public bool ApplyHorizontalLayout
		{
			get { return !ApplyVerticalLayout; }
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public System.Drawing.Size ButtonSize
		{
			get 
			{
				return new Size(defaultButtonWidth, defaultButtonHeight); 
			}
		}
		
		//---------------------------------------------------------------------------
		[System.ComponentModel.Category("Behavior")]
		public bool ShowSelectedButtonText
		{
			get { return showSelectedButtonText; }
			set { showSelectedButtonText = value; }
		}
		
		//---------------------------------------------------------------------------
		[System.ComponentModel.Category("Behavior")]
		public Color SelectedButtonTextColor
		{
			get { return selectedButtonTextColor; }
			set { selectedButtonTextColor = value; }
		}

		//---------------------------------------------------------------------------
		[System.ComponentModel.Category("Behavior")]
		public Color SelectedButtonTextShadowColor
		{
			get { return selectedButtonTextShadowColor; }
			set { selectedButtonTextShadowColor = value; }
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public Color LinearGradientFirstColor { get { return linearGradientFirstColor; } }
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public Color LinearGradientSecondColor { get { return linearGradientSecondColor; } }

		#endregion
	}

	internal class TBToolBarDesigner : System.Windows.Forms.Design.ControlDesigner
	{
		private TBToolBar toolBarControl = null;

		//---------------------------------------------------------------------------
		public override void Initialize(IComponent component)
		{
			base.Initialize(component);

			// Record instance of control we're designing
			toolBarControl = (TBToolBar) component;

			// Hook up events
			IComponentChangeService componentChangeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
			componentChangeService.ComponentRemoving += new ComponentEventHandler(OnComponentRemoving);
		}

		//---------------------------------------------------------------------------
		private void OnComponentRemoving(object sender, System.ComponentModel.Design.ComponentEventArgs e)
		{
			IComponentChangeService componentChangeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));

			TBToolBarButton button;
			IDesignerHost designerHost = (IDesignerHost) GetService(typeof(IDesignerHost));
			
			// If the user is removing a button
			if (e.Component is TBToolBarButton)
			{
				button = (TBToolBarButton) e.Component;
				if (toolBarControl.Buttons.Contains(button))
				{
					componentChangeService.OnComponentChanging(toolBarControl, null);
					toolBarControl.Buttons.Remove(button);
					componentChangeService.OnComponentChanged(toolBarControl, null, null, null);
					return;
				}
			}

			// If the user is removing the control itself
			if (e.Component == toolBarControl)
			{
				for (int i = toolBarControl.Buttons.Count - 1; i >= 0; i--)
				{
					button = toolBarControl.Buttons[i];
					componentChangeService.OnComponentChanging(toolBarControl, null);
					toolBarControl.Buttons.Remove(button);
					designerHost.DestroyComponent(button);
					componentChangeService.OnComponentChanged(toolBarControl, null, null, null);
				}
			}
		}

		//---------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			// Unhook events

			IComponentChangeService componentChangeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
			componentChangeService.ComponentRemoving -= new ComponentEventHandler(OnComponentRemoving);

			base.Dispose(disposing);
		}

		//---------------------------------------------------------------------------
		public override System.Collections.ICollection AssociatedComponents
		{
			get
			{
				return toolBarControl.Buttons;
			}
		}
	}
}
