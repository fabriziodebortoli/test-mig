using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;

namespace Microarea.EasyBuilder.MenuEditor
{
	//============================================================================
	/// <summary>
	/// An extended Panel that provides collapsible panels like those provided in Windows XP.
	/// </summary>
	//============================================================================
	internal partial class CollapsiblePanel : Panel
	{
		public enum PanelState	{ Undefined = -1, Expanded = 0, Collapsed = 1 }

		private PanelState state = PanelState.Collapsed;

		public event EventHandler<CollapsiblePanelEventArgs> PanelStateChanged;
        public event EventHandler DesignSelectionChanged;
        public event ControlEventHandler CollapsiblePanelLinkLabelRemoved;
        public EventHandler InitDrag;

		private int	panelHeight = 0;
		private int imageIndex = 0;
		private int exactTitleHeight = 0;
		private const int iconBorder = 2;
		private const int expandBorder = 4;
		private const int labelsXOffset = 10;
		private const int labelsYSpacing = 4;
		private const int labelsXSpacingFromPicture = 2;
		
		private MenuApplication menuApplication;

		private System.Drawing.Color					startColor = Color.White;
		private System.Drawing.Color					endColor = Color.CornflowerBlue;
		private System.Drawing.Image					titleImage;
		private System.Drawing.Color					imagesTransparentColor = Color.White;
		private System.Drawing.Imaging.ColorMatrix		grayMatrix;
		private System.Drawing.Imaging.ImageAttributes	grayAttributes;

        private bool designSelected;
        private bool designSupport;
        private System.Windows.Forms.Timer titleMouseDownTimer;
        //---------------------------------------------------------------------------
		public CollapsiblePanel()
		{
            InitializeComponent();

			PanelImageList.Images.Add(Resources.Collapse);
			PanelImageList.Images.Add(Resources.Expand);

			exactTitleHeight = this.TitleLabel.Font.Height + CollapsiblePanel.expandBorder + CollapsiblePanel.iconBorder;
            if (titleImage != null)
                exactTitleHeight = Math.Max(titleImage.Height + (2 * CollapsiblePanel.iconBorder), exactTitleHeight);
            panelHeight = exactTitleHeight;

            AdjustSizeAndLocations();

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

            UpdateDisplayedState();

            titleMouseDownTimer = new System.Windows.Forms.Timer(components);
            titleMouseDownTimer.Interval = 300;
            titleMouseDownTimer.Tick += new EventHandler(TitleMouseDownTimer_Tick);
        }

		//---------------------------------------------------------------------------
		internal MenuApplication MenuApplication
		{
			get { return menuApplication; }
			set
			{
				menuApplication = value;

				this.TitleLabel.Font =
					menuApplication.CanBeCustomized
					? new Font(this.TitleLabel.Font, FontStyle.Bold | FontStyle.Italic)
					: new Font(this.TitleLabel.Font, FontStyle.Bold);

				this.TitleLabel.ForeColor =
					menuApplication.CanBeCustomized ?
					StaticResources.CustomizableItemColor :
					StaticResources.NonCustomizableItemColor;
			}
		}

		//---------------------------------------------------------------------------
		protected override void InitLayout()
		{
			base.InitLayout();

            if (panelHeight == 0)
                panelHeight = exactTitleHeight;

            AdjustSizeAndLocations();

            UpdateDisplayedState();
		}
       
        //---------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{	
			base.OnResize(e);

            if (IsExpanded) 
				panelHeight = this.Height;

            AdjustSizeAndLocations();

			CollapsiblePanelLinkLabel pnl = null;
            foreach (Control addedControl in this.Controls)
			{
				pnl = addedControl as CollapsiblePanelLinkLabel;
				if (pnl != null)
					pnl.Width = (this.Width - 2* labelsXOffset);
			}
		}

		//---------------------------------------------------------------------------
		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);

			CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = null;
			if (e != null && (aCollapsiblePanelLinkLabel = e.Control as CollapsiblePanelLinkLabel) != null)
            {
				aCollapsiblePanelLinkLabel.Width = (this.Width - 2 * labelsXOffset);
				aCollapsiblePanelLinkLabel.DesignSelectionChanged += new EventHandler(CollapsiblePanelLinkLabel_DesignSelectionChanged);
            }
		}

        //---------------------------------------------------------------------------
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);

            if
                (
                CollapsiblePanelLinkLabelRemoved != null &&
                e != null &&
                e.Control != null &&
                e.Control is CollapsiblePanelLinkLabel
                )
                CollapsiblePanelLinkLabelRemoved(this, e);
        }

        //---------------------------------------------------------------------------
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            // Se intercetto il Click sul pannello dell'applicazione vuole dire che
            // si è cliccato su una zona non occupata da suoi control figli e quindi 
            // non deve risultare selezionata in design nessuna CollapsiblePanelLinkLabel
            foreach (Control aControl in this.Controls)
            {
                if (aControl != null && aControl is CollapsiblePanelLinkLabel)
                    ((CollapsiblePanelLinkLabel)aControl).IsDesignSelected = false;
            }

            this.IsDesignSelected = true;
        }

        //---------------------------------------------------------------------------
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Se intercetto il Click sul pannello dell'applicazione vuole dire che
            // si è cliccato su una zona non occupata da suoi control figli e quindi 
            // non deve risultare selezionata in design nessuna CollapsiblePanelLinkLabel
            foreach (Control aControl in this.Controls)
            {
                if (aControl != null && aControl is CollapsiblePanelLinkLabel)
                    ((CollapsiblePanelLinkLabel)aControl).IsDesignSelected = false;
            }

            this.IsDesignSelected = true;
        }

        //---------------------------------------------------------------------------
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            DrawSelectionFrame(e.Graphics);
        }

		//---------------------------------------------------------------------------
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.KeyCode == Keys.Enter)
				TogglePanelState();
		}

		/// <summary>
		/// Event handler for the PanelStateChanged event.
		/// </summary>
		/// <param name="e">A CollapsiblePanelEventArgs that contains the event data.</param>
		//---------------------------------------------------------------------------
		protected virtual void OnPanelStateChanged(CollapsiblePanelEventArgs e)
		{
			Refresh();

			if(PanelStateChanged != null)
				PanelStateChanged(this, e);
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
			return this.TitleLabel.Bounds.Contains(xPos, yPos);
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
					this.Height = TitleLabel.Height;
					imageIndex = 1;
					break;
				case PanelState.Expanded :
					this.Height = panelHeight;
					imageIndex = 0;
					break;
				default :
					break;
			}
			TitleLabel.Invalidate();

			OnPanelStateChanged(new CollapsiblePanelEventArgs(this));
		}

        //---------------------------------------------------------------------------
        private void TogglePanelState()
        {
            if (IsExpanded)
                Collapse();
            else if (IsCollapsed)
                Expand();
        }

        //---------------------------------------------------------------------------
        private void AdjustSizeAndLocations()
        {
            if (this.TitleLabel.Height != exactTitleHeight)
                this.TitleLabel.Height = exactTitleHeight;

            int topPositionedLabelYLocation = int.MaxValue;
            int maxLabelBottom = exactTitleHeight;
            foreach (Control anExistingControl in this.Controls)
            {
                if (anExistingControl.Bottom > maxLabelBottom)
                    maxLabelBottom = anExistingControl.Bottom;

                if (anExistingControl is CollapsiblePanelLinkLabel && anExistingControl.Location.Y < topPositionedLabelYLocation)
                        topPositionedLabelYLocation = anExistingControl.Location.Y;
            }
            if (maxLabelBottom > exactTitleHeight)
                maxLabelBottom += labelsYSpacing;
            panelHeight = maxLabelBottom;

            int firstLabelYLocation = exactTitleHeight + labelsYSpacing;
            if (topPositionedLabelYLocation < int.MaxValue && topPositionedLabelYLocation != firstLabelYLocation)
            {
                this.SuspendLayout();

                int deltaY = topPositionedLabelYLocation - firstLabelYLocation;
                foreach (Control anExistingControl in this.Controls)
                    anExistingControl.Location = new Point(anExistingControl.Location.X, anExistingControl.Location.Y - deltaY);

                this.ResumeLayout();
            }
        }

        //---------------------------------------------------------------------------
        private void DrawSelectionFrame(Graphics g)
        {
            if (g == null || !this.IsDesignSelected)
                return;

            Rectangle outsideRect = new Rectangle(0, 0, this.Width, this.Height);
            ControlPaint.DrawBorder(g, outsideRect, Color.White, ButtonBorderStyle.Dotted);

			if (IsLinkLabelDesignSelected)
				return;

            outsideRect.Inflate(-1, -1);
            Rectangle insideRect = outsideRect;
            insideRect.Inflate(-3, -3);
            ControlPaint.DrawSelectionFrame(g, true, outsideRect, insideRect, Color.Transparent);
        }

		//---------------------------------------------------------------------------
		private void TitleLabel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			const int diameter = 14;
			int radius = diameter / 2;
			Rectangle bounds = this.TitleLabel.Bounds;
            int offsetY = 0;
            if (titleImage != null)
            {
                offsetY = titleImage.Height - this.TitleLabel.Font.Height;
                if (offsetY != 0)
                {
                    bounds.Offset(0, offsetY);
                    bounds.Height = exactTitleHeight - offsetY;
                }
            }

			e.Graphics.Clear(Parent.BackColor);

			// Create a GraphicsPath with curved top corners
			GraphicsPath path = new GraphicsPath();
			path.AddLine(bounds.Left + radius, bounds.Top, bounds.Right - radius - 1, bounds.Top);
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

			// Draw the header image, if there is one
			System.Drawing.GraphicsUnit graphicsUnit = System.Drawing.GraphicsUnit.Display;
			int offsetX = CollapsiblePanel.iconBorder;
			if(titleImage != null)
			{
				offsetX += titleImage.Width + CollapsiblePanel.iconBorder;
				// Draws the title image grayscale when the panel is disabled.
				RectangleF srcRect = titleImage.GetBounds(ref graphicsUnit);
				Rectangle destRect = new Rectangle(CollapsiblePanel.iconBorder,
					CollapsiblePanel.iconBorder, titleImage.Width, titleImage.Height);
				
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
			// Title text truncated with an ellipsis where necessary.
			float left = (float)offsetX;
			float top = (float)offsetY + (float)CollapsiblePanel.iconBorder;
			float width = (float)TitleLabel.Width - left - PanelImageList.ImageSize.Width - CollapsiblePanel.expandBorder;
            float height = (float)exactTitleHeight - offsetY - (2f * (float)CollapsiblePanel.iconBorder);
			RectangleF textRectF = new RectangleF(left, top, width, height);
			StringFormat format = new StringFormat();
			format.LineAlignment = StringAlignment.Center;
			format.Trimming = StringTrimming.EllipsisWord;
			format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;
			// Draw title text disabled where appropriate.
			if(Enabled)
			{
				SolidBrush textBrush = new SolidBrush(TitleFontColor);
				e.Graphics.DrawString(TitleLabel.Text, TitleLabel.Font, textBrush, textRectF, format);
				textBrush.Dispose();

				if (TitleLabel.Focused)
					ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle((int)Math.Floor(left), (int)Math.Floor(top), (int)Math.Ceiling(width), (int)Math.Ceiling(height)));
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

			linePen.Dispose();
			lineBrush.Dispose();
			path.Dispose();

			// Draw the expand/collapse image
			// Expand/Collapse image drawn grayscale when panel is disabled.
			int xPos = bounds.Right - PanelImageList.ImageSize.Width - CollapsiblePanel.expandBorder;
            int panelButtonImageHeight = PanelImageList.ImageSize.Height;
            if (panelButtonImageHeight > (bounds.Height - 2 * CollapsiblePanel.expandBorder))
                panelButtonImageHeight = bounds.Height - 2 * CollapsiblePanel.expandBorder;
            int yPos = bounds.Top + (bounds.Height - panelButtonImageHeight) / 2;//+ CollapsiblePanel.expandBorder;
			RectangleF srcIconRectF = PanelImageList.Images[(int)state].GetBounds(ref graphicsUnit);
			Rectangle destIconRect = new Rectangle
                (
                xPos, 
                yPos,
                PanelImageList.ImageSize.Width * panelButtonImageHeight / PanelImageList.ImageSize.Height,
                panelButtonImageHeight
                );
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

            DrawSelectionFrame(e.Graphics);
		}

        //---------------------------------------------------------------------------
        private void TitleLabel_SizeChanged(object sender, EventArgs e)
        {
            AdjustSizeAndLocations();
        }

        //---------------------------------------------------------------------------
        private void TitleLabel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if
                (
                e.Button == MouseButtons.Left &&
                IsOverTitle(e.X, e.Y)
                )
            {
                foreach (Control aControl in this.Controls)
                {
                    if (aControl != null && aControl is CollapsiblePanelLinkLabel)
                        ((CollapsiblePanelLinkLabel)aControl).IsDesignSelected = false;
                }

                this.IsDesignSelected = true;

                titleMouseDownTimer.Start();
            }
        }

        //---------------------------------------------------------------------------
        private void TitleLabel_MouseLeave(object sender, EventArgs e)
        {
            titleMouseDownTimer.Stop();
        }

        //---------------------------------------------------------------------------
        private void TitleLabel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            titleMouseDownTimer.Stop();
			
			// Currently expanded, so store the current height.
			// Currently collapsed, so expand the panel.
            if 
                (
                e.Button == MouseButtons.Left &&
                PanelImageList != null &&
                (PanelImageList.Images.Count >= 2) &&
                IsOverTitle(e.X, e.Y)
                )
                State = (imageIndex == 0) ? PanelState.Collapsed : PanelState.Expanded;
        }
        
        //---------------------------------------------------------------------------
		private void TitleLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if((e.Button == MouseButtons.None) && (true == IsOverTitle(e.X, e.Y)))
				TitleLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			else
				TitleLabel.Cursor = System.Windows.Forms.Cursors.Default;
		}
	
		//---------------------------------------------------------------------------
		private void TitleLabel_GotFocus(object sender, System.EventArgs e)
		{
			TitleLabel.Refresh();
		}

		//---------------------------------------------------------------------------
		private void TitleLabel_LostFocus(object sender, System.EventArgs e)
		{
			TitleLabel.Refresh();
		}

		//---------------------------------------------------------------------------
		private void TitleLabel_KeyUp(object sender, KeyEventArgs e)
		{
            if (TitleLabel.Focused && e.KeyCode == Keys.Enter)
                TogglePanelState();

            OnKeyUp(e);
        }

        //---------------------------------------------------------------------------
        private void TitleMouseDownTimer_Tick(object sender, System.EventArgs e)
        {
            titleMouseDownTimer.Stop();

            if (Control.MouseButtons == MouseButtons.Left && InitDrag != null)
                InitDrag(this, EventArgs.Empty);
        }

        //---------------------------------------------------------------------------
        private void CollapsiblePanelLinkLabel_DesignSelectionChanged(object sender, System.EventArgs e)
        {
			CollapsiblePanelLinkLabel aCollapsiblePanelLinkLabel = sender as CollapsiblePanelLinkLabel;
			if (aCollapsiblePanelLinkLabel == null || !aCollapsiblePanelLinkLabel.IsDesignSelected)
                return;

            foreach (Control aControl in this.Controls)
            {
				aCollapsiblePanelLinkLabel = aControl as CollapsiblePanelLinkLabel;
				if (aCollapsiblePanelLinkLabel != null && aControl != sender)
					aCollapsiblePanelLinkLabel.IsDesignSelected = false;
            }

            this.IsDesignSelected = true;
        }
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public override bool AutoScroll
		{
			get { return false; }
		}
		
		/// <summary>
		/// Gets/sets the PanelState.
		/// </summary>
		//---------------------------------------------------------------------------
		[DefaultValue(0)]
		[Browsable(false)]
		[Localizable(true)]
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
					UpdateDisplayedState();
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsCollapsed { get { return State == PanelState.Collapsed; } }

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsExpanded { get { return State == PanelState.Expanded; } }

		/// <summary>
		/// Gets/sets the text displayed as the panel title.
		/// </summary>
		//---------------------------------------------------------------------------
		[Localizable(true)]
		public string Title { get { return TitleLabel.Text; } set { TitleLabel.Text = value; } }

		/// <summary>
		/// Gets/sets the foreground color used for the title bar.
		/// </summary>
		//---------------------------------------------------------------------------
		public Color TitleFontColor { get { return TitleLabel.ForeColor; } set { TitleLabel.ForeColor = value; } }

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
                exactTitleHeight = Math.Max(TitleLabel.Font.Height + CollapsiblePanel.expandBorder + CollapsiblePanel.iconBorder, exactTitleHeight);
                
                AdjustSizeAndLocations();

                UpdateDisplayedState();
                this.TitleLabel.Invalidate();
            }
		}

		/// <summary>
		/// Gets/sets the image list used for the expand/collapse image.
		/// </summary>
		//---------------------------------------------------------------------------
		[Browsable(false)]
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
				if (PanelImageList == null)
					return;
			
				if(PanelImageList.Images.Count > 0)
					imageIndex = 0;
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

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public static int LabelsYSpacing { get { return labelsYSpacing; } }
		
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
                    exactTitleHeight = Math.Max(titleImage.Height + (2 * CollapsiblePanel.iconBorder), exactTitleHeight);
                else
                    exactTitleHeight = TitleLabel.Font.Height + CollapsiblePanel.expandBorder + CollapsiblePanel.iconBorder;

                AdjustSizeAndLocations();

                UpdateDisplayedState();
                this.TitleLabel.Invalidate();
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

        //---------------------------------------------------------------------------
        public bool DesignSupport
        {
            get { return designSupport; }
            set { designSupport = value; }
        }
        
        //---------------------------------------------------------------------------
        public bool IsDesignSelected 
        {
            get { return designSupport && designSelected; }

            set 
            {
                if (!designSupport)
                {
                    designSelected = false;
                    return;
                }
                
                if (designSelected != value)
                {
                    designSelected = value;

                    if (!designSelected)
                    {
                        foreach (Control aControl in this.Controls)
                        {
                            if (aControl != null && aControl is CollapsiblePanelLinkLabel)
                                ((CollapsiblePanelLinkLabel)aControl).IsDesignSelected = false;
                        }
                    }
                }
				Refresh();

				if (designSelected)
				{
					Focus();
					if (DesignSelectionChanged != null)
						DesignSelectionChanged(this, EventArgs.Empty);
				}
            }
        }

        //---------------------------------------------------------------------------
        public bool IsLinkLabelDesignSelected
        {
            get 
            {
                if (this.Controls == null || this.Controls.Count == 0)
                    return false;

                foreach (Control aControl in this.Controls)
                {
                    if 
                        (
                        aControl != null && 
                        aControl is CollapsiblePanelLinkLabel &&
                        ((CollapsiblePanelLinkLabel)aControl).IsDesignSelected
                        )
                        return true;
                }
                return false;
            }
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

        //---------------------------------------------------------------------------
		public CollapsiblePanelLinkLabel AddLinkLabel
			(
			string	labelText,
			Image	labelImage
			)
		{
            return InsertLinkLabel(labelText, labelImage, null);
		}

        //---------------------------------------------------------------------------
        public CollapsiblePanelLinkLabel InsertLinkLabel
            (
            string labelText,
            Image labelImage,
            CollapsiblePanelLinkLabel linkLabelBefore
            )
        {
            int newLinkLabelTop = 0;
            if (linkLabelBefore == null || linkLabelBefore.Parent != this)
                newLinkLabelTop = exactTitleHeight + labelsYSpacing;
            else
                newLinkLabelTop = linkLabelBefore.Bottom + labelsYSpacing;


            Point labelLocation = new Point(labelsXOffset, newLinkLabelTop);

            CollapsiblePanelLinkLabel newLinkLabel = new CollapsiblePanelLinkLabel();

            newLinkLabel.AutoSize = false;
            newLinkLabel.Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Regular);
            newLinkLabel.Text = labelText;
            newLinkLabel.Image = labelImage;

            newLinkLabel.Size = new Size(this.Width - 2 * labelsXOffset, newLinkLabel.PreferredHeight);

            newLinkLabel.Location = labelLocation;

            newLinkLabel.Visible = true;

            if (newLinkLabel.Top == (exactTitleHeight + labelsYSpacing))
            {
                newLinkLabel.TabStop = true;
                newLinkLabel.TabIndex = 1;
            }
            else
                newLinkLabel.TabStop = false;

            this.SuspendLayout();

            bool firstLinkLabel = true;

            foreach (Control anExistingControl in this.Controls)
            {
                anExistingControl.TabStop = false;

                if (anExistingControl is CollapsiblePanelLinkLabel)
                    firstLinkLabel = false;

                if (anExistingControl.Top >= newLinkLabelTop)
                    anExistingControl.Top += (newLinkLabel.Height + labelsYSpacing);
            }

            this.Controls.Add(newLinkLabel);

            if (firstLinkLabel)
                panelHeight = exactTitleHeight + labelsYSpacing;

            panelHeight += newLinkLabel.Height + labelsYSpacing;

            this.Height = panelHeight;

            this.ResumeLayout(true);

            Application.DoEvents();

            UpdateDisplayedState();

            return newLinkLabel;
        }

        //---------------------------------------------------------------------------
        public bool RemoveLinkLabel(CollapsiblePanelLinkLabel linkLabelToRemove, bool removePictureBox)
        {
            if (linkLabelToRemove == null || !this.Controls.Contains(linkLabelToRemove))
                return false;

            int deltaY = linkLabelToRemove.Height;
            PictureBox labelPictureBox = null;
            if (removePictureBox)
            {
                foreach (Control anExistingControl in this.Controls)
                {
                    if (anExistingControl is PictureBox && anExistingControl.Top == linkLabelToRemove.Top)
                    {
                        labelPictureBox = (PictureBox)anExistingControl;
                        if (labelPictureBox.Height > deltaY)
                            deltaY = labelPictureBox.Height;
                        break;
                    }
                }
            }
            deltaY += labelsYSpacing;

            int linkLabelOriginalTop = linkLabelToRemove.Top;
            foreach (Control anExistingControl in this.Controls)
            {
                if (anExistingControl.Top < linkLabelOriginalTop)
                    continue;

                anExistingControl.Top -= deltaY;
            }
            
            panelHeight -= deltaY;
            
            if (labelPictureBox != null)
                this.Controls.Remove(labelPictureBox);

            this.Controls.Remove(linkLabelToRemove);

            UpdateDisplayedState();

            return true;
        }

        //---------------------------------------------------------------------------
        public bool RemoveLinkLabel(CollapsiblePanelLinkLabel linkLabelToRemove)
        {
            return RemoveLinkLabel(linkLabelToRemove, true);
        }
	}

	/// <summary>
	/// Stores a color and provides conversion between the RGB and HLS color models
	/// </summary>
	//============================================================================
	internal class PanelColor
	{
		// Member variables
		private Color	currentColor = Color.Red;

		/// <summary>
		/// The current PanelColor (RGB model)
		/// </summary>
		//---------------------------------------------------------------------------
		public Color CurrentColor { get { return currentColor; } set { currentColor = value; } }

		/// <summary>
		/// The Red component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public byte Red { get { return currentColor.R; } set { currentColor = Color.FromArgb(value, Green, Blue); } }

		/// <summary>
		/// The Green component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public byte Green { get { return currentColor.G; } set { currentColor = Color.FromArgb(Red, value, Blue); } }

		/// <summary>
		/// The Blue component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public byte Blue { get { return currentColor.B; } set { currentColor = Color.FromArgb(Red, Green, value); } }

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
					return 0.0f;
				else
				{
					float fMax = (float)Math.Max(Red, Math.Max(Green, Blue));
					float fMin = (float)Math.Min(Red, Math.Min(Green, Blue));
					return (fMax - fMin) / fMax;
				}
			}
			set
			{
				currentColor = HSBToRGB((int)currentColor.GetHue(),	value, currentColor.GetBrightness());
			}
		}

		//---------------------------------------------------------------------------
		public float GetSaturation()
		{
			return (255 - (((float)(Red + Green + Blue)) / 3) * Math.Min(Red, Math.Min(Green, Blue))) / 255;
		}

		/// <summary>
		/// The Brightness component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public float Brightness
		{
			get
			{
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
	/// An ExplorerBar-type extended Panel for containing CollapsiblePanel objects.
	/// </summary>
	//============================================================================
	internal class CollapsiblePanelBar : Panel
	{
		private List<CollapsiblePanel> panels = new List<CollapsiblePanel>();
		private int xSpacing = 8;
		private int ySpacing = 8;

		public event EventHandler<PanelsPositionUpdatingEventArgs> PanelsPositionUpdating;
		public event EventHandler<PanelsPositionUpdatingEventArgs> PanelsPositionUpdated;
		//---------------------------------------------------------------------------
		public CollapsiblePanelBar()
		{
			BackColor = Color.CornflowerBlue;
		}

		//---------------------------------------------------------------------------
		public List<CollapsiblePanel> Panels { get { return panels; } }

        //---------------------------------------------------------------------------
		public int PanelsCount { get { return (panels != null) ? panels.Count : 0; } }
        
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

		//---------------------------------------------------------------------------
		public void UpdateAllPanelsPositions()
		{
			if (panels == null || panels.Count < 1)
				return;

			UpdatePositions(0);

			this.HScroll = false;
		}

		//---------------------------------------------------------------------------
		private void UpdatePositions(int startingIndex)
		{
            if (!this.Visible || panels == null || startingIndex < 0 || startingIndex >= panels.Count)
				return;

			if (PanelsPositionUpdating != null)
				PanelsPositionUpdating(this, new PanelsPositionUpdatingEventArgs(this.DisplayRectangle.Width - (2 * xSpacing)));

            for (int i = startingIndex; i < panels.Count; i++)
			{
                if (panels[i] == null || panels[i].IsDisposed)
                    continue;

				int panelTop = 0;;
				// Update the panel locations.
				if (i == 0)
				{
					// Top panel.
					panelTop = this.DisplayRectangle.Top + ySpacing;
				}
				else if (panels[i-1] != null && !panels[i-1].IsDisposed)
				{
					if (!panels[i-1].Visible)
						panelTop = panels[i-1].Top;
					else
						panelTop = panels[i-1].Bottom + ySpacing;
				}
				
				panels[i].Location = new Point(xSpacing, panelTop);
				panels[i].Size =  new Size (this.DisplayRectangle.Width - (2 * xSpacing), panels[i].Height);
			}

			UpdateBounds();

			PerformLayout();
	
			if (PanelsPositionUpdated != null)
				PanelsPositionUpdated(this, new PanelsPositionUpdatingEventArgs(this.DisplayRectangle.Width - (2 * xSpacing)));
		}

		/// <summary>
		/// Event handler for the <see cref="Control.ControlAdded">ControlAdded</see> event.
		/// </summary>
		/// <param name="e">A <see cref="System.Windows.Forms.ControlEventArgs">ControlEventArgs</see> that contains the event data.</param>
		//---------------------------------------------------------------------------
		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);

			CollapsiblePanel aCollapsiblePanel = e.Control as CollapsiblePanel;

			if (aCollapsiblePanel == null)
				return;

			// Adjust the docking property to Left | Right | Top
			e.Control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

			aCollapsiblePanel.VisibleChanged += new System.EventHandler(Panel_VisibleChanged);
			aCollapsiblePanel.PanelStateChanged += new EventHandler<CollapsiblePanelEventArgs>(Panel_StateChanged);
			aCollapsiblePanel.DesignSelectionChanged += new EventHandler(Panel_DesignSelected);
			aCollapsiblePanel.KeyUp += new KeyEventHandler(Panel_KeyUp);

            if (!panels.Contains(aCollapsiblePanel))
				panels.Add(aCollapsiblePanel);
		}

		/// <summary>
		/// Event handler for the ControlRemoved event.
		/// </summary>
		/// <param name="e">A ControlEventArgs that contains the event data.</param>
		//---------------------------------------------------------------------------
		protected override void OnControlRemoved(ControlEventArgs e)
		{
			base.OnControlRemoved(e);

			CollapsiblePanel aCollapsiblePanel = e.Control as CollapsiblePanel;

			if (aCollapsiblePanel == null)
				return;

			// Get the index of the panel within the collection.
			int index = panels.IndexOf(aCollapsiblePanel);
			if (index == -1)
				return;

			// Remove this panel from the collection.
			panels.RemoveAt(index);
			// Update the position of any remaining panels.
			UpdateAllPanelsPositions();
		}

		//---------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			UpdateAllPanelsPositions();
		}
		
        //---------------------------------------------------------------------------
		private void Panel_VisibleChanged(object sender, EventArgs e)
		{
			if (panels == null)
				return;

			CollapsiblePanel aCollapsiblePanel = sender as CollapsiblePanel;
			if (aCollapsiblePanel == null)
				return;

			// Get the index of the control that just changed state.
			int index = panels.IndexOf(aCollapsiblePanel);
		
			// Now update the position of all subsequent panels.
			if (index > 0 && index < panels.Count)
				UpdatePositions(++index);
		}

		//---------------------------------------------------------------------------
		private void Panel_StateChanged(object sender, CollapsiblePanelEventArgs e)
		{
			if (panels == null)
				return;

			// Get the index of the control that just changed state.
			int index = panels.IndexOf(e.CollapsiblePanel);
		
			// Now update the position of all subsequent panels.
			if (index >= 0 && index < panels.Count)
				UpdatePositions(++index);
		}

        //---------------------------------------------------------------------
        private void Panel_DesignSelected(object sender, EventArgs e)
        {
			CollapsiblePanel aCollapsiblePanel = sender as CollapsiblePanel;
            if 
                (
				aCollapsiblePanel == null ||
				!aCollapsiblePanel.IsDesignSelected ||
                panels == null ||
                panels.Count == 0
                )
                return;

            foreach (CollapsiblePanel pnl in panels)
            {
				if (pnl != sender)
					pnl.IsDesignSelected = false;
            }
        }

        //---------------------------------------------------------------------------
        private void Panel_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        //---------------------------------------------------------------------
        public CollapsiblePanel InsertCollapsiblePanel
                (
                string panelTitle,
                Image panelTitleImage,
                CollapsiblePanel panelBefore
                )
        {
            CollapsiblePanel newPanel = new CollapsiblePanel();
			newPanel.Title = panelTitle;
            newPanel.TitleImage = panelTitleImage;
            int newPanelIdx = (panelBefore != null && panels != null && panels.Count > 0) ? panels.IndexOf(panelBefore) + 1 : 0;

            panels.Insert(newPanelIdx, newPanel);

            this.Controls.Add(newPanel);

            // Update the size and position of the panels
            UpdateAllPanelsPositions();

            PerformLayout();

            return newPanel;
        }
    }

	//============================================================================
	internal class CollapsiblePanelEventArgs : EventArgs
	{
		private CollapsiblePanel panel;

		//---------------------------------------------------------------------------
		public CollapsiblePanelEventArgs(CollapsiblePanel sender)
		{
			panel = sender;
		}

		/// <summary>
		/// Gets the CollapsiblePanel that triggered the event.
		/// </summary>
		//---------------------------------------------------------------------------
		public CollapsiblePanel CollapsiblePanel { get { return panel; } }

		/// <summary>
		/// Gets the PanelState of the CollapsiblePanel that triggered the event.
		/// </summary>
		//---------------------------------------------------------------------------
		public CollapsiblePanel.PanelState State { get { return panel.State; } }
	}

	//============================================================================
	internal class PanelsPositionUpdatingEventArgs : EventArgs
	{
		public int NewPanelsWidth { get; set; }
		
		//---------------------------------------------------------------------
		public PanelsPositionUpdatingEventArgs(int newPanelsWidth)
		{
			this.NewPanelsWidth = newPanelsWidth;
		}
	}
}