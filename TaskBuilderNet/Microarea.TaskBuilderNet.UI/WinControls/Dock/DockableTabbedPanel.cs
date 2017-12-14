using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.UI.WinControls.Dock.Win32;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	//===========================================================================
	/// <summary>
	/// Descripiton of DockableTabbedPanel
	/// </summary>
	internal class DockableTabbedPanel : TabbedPanel
	{
		internal class CaptionMeasure
		{
			public const int TextGapTop = 2;
			public const int TextGapBottom = 0;
			public const int TextGapLeft = 3;
			public const int TextGapRight = 3;
			public const int ButtonGapTop = 2;
			public const int ButtonGapBottom = 2;
			public const int ButtonGapBetween = 2;
			public const int ButtonGapLeft = 2;
			public const int ButtonGapRight = 2;
		}
		
		internal class NormalTabMeasure
		{
			public const int StripGapLeft = 4;
			public const int StripGapRight = 3;
			public const int ImageHeight = 16;
			public const int ImageWidth = 16;
			public const int ImageGapTop = 3;
			public const int ImageGapBottom = 1;
			public const int ImageGapLeft = 3;
			public const int ImageGapRight = 2;
			public const int TextGapRight = 1;
		}


		private TabbedPanelButton	closeButton;
		private Bitmap				closeEnabledImage;
		private Bitmap				closeDisabledImage;
		private string				closeToolTip;
		private bool				autoHide = false;
		private TabbedPanelButton	autoHideButton;
		private Bitmap				autoHideYesImage;
		private Bitmap				autoHideNoImage;
		private string				autoHideToolTip;
		private bool				hasCaption = true;
		private Color				activeCaptionColor = SystemColors.ActiveCaption;
		private Color				activeCaptionTextColor = SystemColors.ActiveCaptionText;
		private Color				inactiveCaptionColor = SystemColors.InactiveCaption;
		private Color				inactiveCaptionTextColor = SystemColors.InactiveCaptionText;
		private StringFormat		captionStringFormat;
		private StringFormat		tabStringFormat;
		private Timer				mouseTrackTimer;
		private bool				applyFloatingStyle = false;
		private IContainer			components = new Container();

		//---------------------------------------------------------------------------
		public DockableTabbedPanel()
		{
			Initialize(false);
		}

		//---------------------------------------------------------------------------
		public DockableTabbedPanel(DockableFormsContainer aContainer) : base(aContainer)
		{
			Initialize(false);
		}

		//---------------------------------------------------------------------------
		public DockableTabbedPanel(DockableFormsContainer aContainer, bool isFloatStyle) : base(aContainer)
		{
			Initialize(isFloatStyle);
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

		#region DockableTabbedPanel internal overridden properties

		//---------------------------------------------------------------------------
		internal override bool HasCaption
		{
			get	{ return hasCaption; }
			set
			{
				if (hasCaption == value)
					return;

				hasCaption = value;
				autoHideButton.Visible = (value && !applyFloatingStyle);
				closeButton.Visible = value;
				Invalidate();
			}
		}

		#endregion

		#region DockableTabbedPanel public properties

		//---------------------------------------------------------------------------
		public bool AutoHide
		{
			get	{	return autoHide;	}
			set
			{
				if (autoHide == value)
					return;

				autoHide = value;
				autoHideButton.EnabledImage = value ? autoHideYesImage : autoHideNoImage;
			}
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Color ActiveCaptionColor
		{
			get { return activeCaptionColor; }
			set { activeCaptionColor = value; }
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Color ActiveCaptionTextColor
		{
			get { return activeCaptionTextColor; }
			set { activeCaptionTextColor = value; }
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Color InactiveCaptionColor
		{
			get { return inactiveCaptionColor; }
			set { inactiveCaptionColor = value; }
		}
		
		//---------------------------------------------------------------------------
		public System.Drawing.Color InactiveCaptionTextColor
		{
			get { return inactiveCaptionTextColor; }
			set { inactiveCaptionTextColor = value; }
		}

		#endregion

		#region DockableTabbedPanel private methods

		//---------------------------------------------------------------------------
		private void Initialize(bool isFloatStyle)
		{
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Dock.Images.CloseEnabled.bmp");
			if (imageStream != null)
				closeEnabledImage = new Bitmap(imageStream);

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Dock.Images.CloseDisabled.bmp");
			if (imageStream != null)
				closeDisabledImage = new Bitmap(imageStream);

			closeButton = new TabbedPanelButton(closeEnabledImage, closeDisabledImage);
			
			closeToolTip = Strings.CloseDockWindow;

			closeButton.Monochrome = false;
			closeButton.ToolTipText = closeToolTip;
			closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			closeButton.Click += new EventHandler(Close_Click);

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Dock.Images.AutoHideYes.bmp");
			if (imageStream != null)
				autoHideYesImage = new Bitmap(imageStream);

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Dock.Images.AutoHideNo.bmp");
			if (imageStream != null)
				autoHideNoImage = new Bitmap(imageStream);

			autoHideButton = new TabbedPanelButton(AutoHide ? autoHideYesImage : autoHideNoImage);

			autoHideToolTip =string.Empty;
			
			autoHideButton.Monochrome = false;
			autoHideButton.ToolTipText = autoHideToolTip;
			autoHideButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			autoHideButton.Click += new EventHandler(AutoHide_Click);

			Controls.AddRange(new Control[] { autoHideButton, closeButton } );
			
			captionStringFormat = new StringFormat();
			captionStringFormat.Trimming = StringTrimming.EllipsisCharacter;
			captionStringFormat.LineAlignment = StringAlignment.Center;
			captionStringFormat.FormatFlags = StringFormatFlags.NoWrap;

			tabStringFormat = new StringFormat(StringFormat.GenericTypographic);
			tabStringFormat.Trimming = StringTrimming.EllipsisCharacter;
			tabStringFormat.LineAlignment = StringAlignment.Center;
			tabStringFormat.FormatFlags = StringFormatFlags.NoWrap;

			applyFloatingStyle = isFloatStyle;

			mouseTrackTimer = new Timer(components);
			mouseTrackTimer.Tick += new EventHandler(TimerMouseTrack_Tick);

			if (isFloatStyle)
				autoHideButton.Visible = false;
		}

		//---------------------------------------------------------------------------
		private void AnimateWindow(bool show)
		{
			DockManager.SuspendAutoHideControlLayout = true;

			Rectangle rectSource = DockManager.GetAutoHideRectangle(!show);
			Rectangle rectTarget = DockManager.GetAutoHideRectangle(show);
			Control autoHideControl = DockManager.AutoHideControl;
			autoHideControl.Bounds = rectSource;
			autoHideControl.BringToFront();
			autoHideControl.Visible = true;

			TimeSpan time = new TimeSpan(0, 0, 0, 0, 100);
			int dxLoc, dyLoc;
			int dWidth, dHeight;
			dxLoc = dyLoc = dWidth = dHeight = 0;
			if (rectSource.Left == rectTarget.Left &&
				rectSource.Top == rectTarget.Top &&
				rectSource.Right == rectTarget.Right)
			{
				dHeight = (rectTarget.Height > rectSource.Height ? 1 : -1);
			}
			else if (rectSource.Left == rectTarget.Left &&
				rectSource.Top == rectTarget.Top &&
				rectSource.Bottom == rectTarget.Bottom)
			{
				dWidth = (rectTarget.Width > rectSource.Width ? 1 : -1);
			}
			else if (rectSource.Right == rectTarget.Right &&
				rectSource.Top == rectTarget.Top &&
				rectSource.Bottom == rectTarget.Bottom)
			{
				dxLoc = (rectTarget.Width > rectSource.Width ? -1 : 1);
				dWidth = (rectTarget.Width > rectSource.Width ? 1 : -1);
			}
			else if (rectSource.Right == rectTarget.Right &&
				rectSource.Left == rectTarget.Left &&
				rectSource.Bottom == rectTarget.Bottom)
			{
				dyLoc = (rectTarget.Height > rectSource.Height ? -1 : 1);
				dHeight = (rectTarget.Height > rectSource.Height ? 1 : -1);
			}
			else
				throw(new ArgumentException("Invalid rectTarget."));

			int speedFactor = 1;
			int totalPixels = (rectSource.Width != rectTarget.Width) ?
				Math.Abs(rectSource.Width - rectTarget.Width) :
				Math.Abs(rectSource.Height - rectTarget.Height);
			int remainPixels = totalPixels;
			DateTime startingTime = DateTime.Now;
			while (rectSource != rectTarget)
			{
				DateTime startPerMove = DateTime.Now;

				rectSource.X += dxLoc * speedFactor;
				rectSource.Y += dyLoc * speedFactor;
				rectSource.Width += dWidth * speedFactor;
				rectSource.Height += dHeight * speedFactor;
				if (Math.Sign(rectTarget.X - rectSource.X) != Math.Sign(dxLoc))
					rectSource.X = rectTarget.X;
				if (Math.Sign(rectTarget.Y - rectSource.Y) != Math.Sign(dyLoc))
					rectSource.Y = rectTarget.Y;
				if (Math.Sign(rectTarget.Width - rectSource.Width) != Math.Sign(dWidth))
					rectSource.Width = rectTarget.Width;
				if (Math.Sign(rectTarget.Height - rectSource.Height) != Math.Sign(dHeight))
					rectSource.Height = rectTarget.Height;
				
				autoHideControl.Bounds = rectSource;
				
				DockManager.Update();

				remainPixels -= speedFactor;

				while (true)
				{
					TimeSpan elapsedPerMove = DateTime.Now - startPerMove;
					TimeSpan elapsedTime = DateTime.Now - startingTime;
					if ((time - elapsedTime).TotalMilliseconds <= 0)
					{
						speedFactor = remainPixels;
						break;
					}
					else
						speedFactor = remainPixels * (int)elapsedPerMove.TotalMilliseconds / (int)((time - elapsedTime).TotalMilliseconds);
					if (speedFactor >= 1)
						break;
				}
			}

			DockManager.SuspendAutoHideControlLayout = false;
		}

		//---------------------------------------------------------------------------
		private void CalculateTabs()
		{
			if (VisibleFormsCount <= 1 || autoHide)
				return;

			Rectangle rectTabStrip = GetTabStripRectangle();

			// Calculate tab widths
			int visibleFormsCount = VisibleFormsCount;

			int[] maxWidths = new int[visibleFormsCount];
			bool[] flags = new bool[visibleFormsCount];
			for (int i=0; i < visibleFormsCount; i++)
			{
				maxWidths[i] = GetTabOriginalWidth(i);
				flags[i] = false;
			}

			// Set tab whose max width less than average width
			int totalWidth = rectTabStrip.Width - NormalTabMeasure.StripGapLeft - NormalTabMeasure.StripGapRight;
			int totalAllocatedWidth = 0;
			int averageWidth = totalWidth / visibleFormsCount;
			
			bool anyWidthWithinAverage = true;
			int remainedForms = visibleFormsCount;
			while (anyWidthWithinAverage && remainedForms > 0)
			{
				anyWidthWithinAverage = false;
				for (int i = 0; i < visibleFormsCount; i++)
				{
					if (flags[i])
						continue;

					DockableForm aForm = GetVisibleForm(i);

					if (maxWidths[i] <= averageWidth)
					{
						flags[i] = true;
						aForm.TabWidth = maxWidths[i];
						totalAllocatedWidth += aForm.TabWidth;

						anyWidthWithinAverage = true;
						remainedForms--;
					}
				}
				if (remainedForms != 0)
					averageWidth = (totalWidth - totalAllocatedWidth) / remainedForms;
			}

			// If any tab width not set yet, set it to the average width
			if (remainedForms > 0)
			{
				int roundUpWidth = (totalWidth - totalAllocatedWidth) - (averageWidth * remainedForms);
				for (int i = 0; i < visibleFormsCount; i++)
				{
					if (flags[i])
						continue;

					DockableForm aForm = GetVisibleForm(i);

					flags[i] = true;
					if (roundUpWidth > 0)
					{
						aForm.TabWidth = averageWidth + 1;
						roundUpWidth --;
					}
					else
						aForm.TabWidth = averageWidth;
				}
			}

			//////////////////////////////////////////////////////////////////////////////
			/// Set the X position of the tabs
			/////////////////////////////////////////////////////////////////////////////
			int x = rectTabStrip.X + NormalTabMeasure.StripGapLeft;
			for (int i = 0; i < visibleFormsCount; ++i)
			{
				DockableForm aForm = GetVisibleForm(i);
				aForm.TabX = x;
				x += aForm.TabWidth;
			}
		}

		//---------------------------------------------------------------------------
		private void DrawTab(Graphics g, DockableForm aForm, Rectangle rect)
		{
			Rectangle rectIcon = new Rectangle(
				rect.X + NormalTabMeasure.ImageGapLeft,
				rect.Y + rect.Height - 1 - NormalTabMeasure.ImageGapBottom - NormalTabMeasure.ImageHeight,
				NormalTabMeasure.ImageWidth, NormalTabMeasure.ImageHeight);
			Rectangle rectText = rectIcon;
			rectText.X += rectIcon.Width + NormalTabMeasure.ImageGapRight;
			rectText.Width = rect.Width - rectIcon.Width - NormalTabMeasure.ImageGapLeft - 
				NormalTabMeasure.ImageGapRight - NormalTabMeasure.TextGapRight;

			System.Drawing.SolidBrush tabTextBrush = new System.Drawing.SolidBrush((ActiveForm == aForm) ? TabsActiveTabTextColor : TabsInactiveTabTextColor);
			
			if (ActiveForm == aForm)
			{
				System.Drawing.SolidBrush activeTabBackBrush = new System.Drawing.SolidBrush(aForm.BackColor);
				g.FillRectangle(activeTabBackBrush, rect);
				activeTabBackBrush.Dispose();

				g.DrawLine(SystemPens.ControlText,
					rect.X, rect.Y + rect.Height - 1, rect.X + rect.Width - 1, rect.Y + rect.Height - 1);
				g.DrawLine(SystemPens.ControlText,
					rect.X + rect.Width - 1, rect.Y, rect.X + rect.Width - 1, rect.Y + rect.Height - 1);
				g.DrawString(aForm.Text, TabsFont, tabTextBrush, rectText, tabStringFormat);
			}
			else
			{
				if (GetVisibleFormIndex(ActiveForm) != GetVisibleFormIndex(aForm) + 1)
					g.DrawLine(SystemPens.GrayText,
						rect.X + rect.Width - 1, rect.Y + 3, rect.X + rect.Width - 1, rect.Y + rect.Height - 4);
				g.DrawString(aForm.Text, TabsFont, tabTextBrush, rectText, tabStringFormat);
			}

			tabTextBrush.Dispose();

			if (rect.Contains(rectIcon))
				g.DrawIcon(aForm.Icon, rectIcon);
		}

		#region private event handlers

		//---------------------------------------------------------------------------
		private void TimerMouseTrack_Tick(object sender, EventArgs e)
		{
			if (AutoHide == false)
			{
				mouseTrackTimer.Enabled = false;
				return;
			}

			if (DockManager.ActiveAutoHideForm == ActiveForm && !IsActivated)
			{
				Point ptMouseInDockWindow = PointToClient(Control.MousePosition);
				Point ptMouseInDockManager = DockManager.PointToClient(Control.MousePosition);

				Rectangle rectTabStrip = DockManager.GetTabStripRectangle(DockState, true);

				if (!ClientRectangle.Contains(ptMouseInDockWindow) && !rectTabStrip.Contains(ptMouseInDockManager))
				{
					DockManager.ActiveAutoHideForm = null;
					mouseTrackTimer.Enabled = false;
				}
			}
		}

		//---------------------------------------------------------------------------
		private void AutoHide_Click(object sender, EventArgs e)
		{
			this.FormContainer.VisibleState = DockManager.ToggleAutoHideState(DockState);
			if (AutoHide)
			{
				bool oldAnimateAutoHide = DockManager.AnimateAutoHide;
				DockManager.AnimateAutoHide = false;
				DockManager.ActiveAutoHideForm = ActiveForm;
				DockManager.AnimateAutoHide = oldAnimateAutoHide;
			}
			FormContainer.Activate();
		}

		//---------------------------------------------------------------------------
		private void Close_Click(object sender, EventArgs e)
		{
			FormContainer.CloseActiveForm();
		}

		#endregion

		#endregion

		#region DockableTabbedPanel internal methods

		//---------------------------------------------------------------------------
		internal void AnimateHide()
		{
			if (DockManager.ActiveAutoHideForm != ActiveForm || ActiveForm == null)
				throw(new InvalidOperationException(""));

			if (DockManager.AnimateAutoHide)
				AnimateWindow(false);
			else
				DockManager.AutoHideControl.Hide();

			OnAutoHide(false);
		}

		//---------------------------------------------------------------------------
		internal void AnimateShow()
		{
			if (DockManager.ActiveAutoHideForm == null)
				throw(new InvalidOperationException(""));

			ActiveForm = DockManager.ActiveAutoHideForm;

			// Set the size and location of DockableTabbedPanel
			foreach (Control aControl in DockManager.AutoHideControl.Controls)
			{
				if (aControl == this)
				{
					Rectangle rectTarget = DockManager.GetAutoHideRectangle(true);
					Bounds = new Rectangle(0, 0, rectTarget.Width, rectTarget.Height);
					Visible = true;
				}
				else
					aControl.Visible = false;
			}

			if (DockManager.AnimateAutoHide)
				AnimateWindow(true);
			else
				DockManager.AutoHideControl.Show();

			OnAutoHide(true);
		}

		#endregion
		
		#region DockableTabbedPanel protected overriden methods

		//---------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged (e);

			closeButton.Visible = this.Visible && hasCaption;
			autoHideButton.Visible = this.Visible && hasCaption;
		}

		//---------------------------------------------------------------------------
		protected override void DrawCaption(Graphics g)
		{
			DockableForm currentActiveForm = this.ActiveForm;
			if (currentActiveForm == null)
				return;

			Rectangle rectCaption = CaptionRectangle;

			if (rectCaption.IsEmpty)
				return;

			SolidBrush brushBackGround = new System.Drawing.SolidBrush(IsActivated ? activeCaptionColor : inactiveCaptionColor);

			g.FillRectangle(brushBackGround, rectCaption);

			brushBackGround.Dispose();

			closeButton.BackColor = (IsActivated ? activeCaptionColor : inactiveCaptionColor);
			closeButton.ForeColor = (IsActivated ? activeCaptionTextColor : SystemColors.ControlText);
			closeButton.BorderColor = (IsActivated ? activeCaptionTextColor : Color.Empty);

			autoHideButton.BackColor = (IsActivated ? activeCaptionColor : inactiveCaptionColor);
			autoHideButton.ForeColor = (IsActivated ? activeCaptionTextColor : SystemColors.ControlText);
			autoHideButton.BorderColor = (IsActivated ? activeCaptionTextColor : Color.Empty);

			//autoHideButton.BackColor = (IsActivated ? SystemColors.ActiveCaption : SystemColors.Control);
			//autoHideButton.ForeColor = (IsActivated ? SystemColors.ActiveCaptionText : SystemColors.ControlText);
			//autoHideButton.BorderColor = (IsActivated ? SystemColors.ActiveCaptionText : Color.Empty);

			if (!IsActivated)
			{
				g.DrawLine(SystemPens.GrayText, rectCaption.X + 1, rectCaption.Y, rectCaption.X + rectCaption.Width - 2, rectCaption.Y);
				g.DrawLine(SystemPens.GrayText, rectCaption.X + 1, rectCaption.Y + rectCaption.Height - 1, rectCaption.X + rectCaption.Width - 2, rectCaption.Y + rectCaption.Height - 1);
				g.DrawLine(SystemPens.GrayText, rectCaption.X, rectCaption.Y + 1, rectCaption.X, rectCaption.Y + rectCaption.Height - 2);
				g.DrawLine(SystemPens.GrayText, rectCaption.X + rectCaption.Width - 1, rectCaption.Y + 1, rectCaption.X + rectCaption.Width - 1, rectCaption.Y + rectCaption.Height - 2);
			}

			int buttonHeight = closeEnabledImage.Height;
			int height = rectCaption.Height - CaptionMeasure.ButtonGapTop - CaptionMeasure.ButtonGapBottom;
			if (buttonHeight < height)
				buttonHeight = height;
	
			closeButton.Size = new Size(buttonHeight, buttonHeight);
			autoHideButton.Size = new Size(buttonHeight, buttonHeight);

			int x = rectCaption.X + rectCaption.Width - 1 - CaptionMeasure.ButtonGapRight - closeButton.Width;
			int y = rectCaption.Y + CaptionMeasure.ButtonGapTop;
			closeButton.Location = new Point(x, y);
			Point point = closeButton.Location;
			point.Offset(-(autoHideButton.Width + CaptionMeasure.ButtonGapBetween), 0);
			autoHideButton.Location = point;
				
			Rectangle rectCaptionText = rectCaption;
			rectCaptionText.X += CaptionMeasure.TextGapLeft;
			rectCaptionText.Width = rectCaption.Width - CaptionMeasure.ButtonGapRight
				- CaptionMeasure.ButtonGapLeft
				- CaptionMeasure.ButtonGapBetween - 2 * closeButton.Width
				- CaptionMeasure.TextGapLeft - CaptionMeasure.TextGapRight;
			rectCaptionText.Y += CaptionMeasure.TextGapTop;
			rectCaptionText.Height -= CaptionMeasure.TextGapTop + CaptionMeasure.TextGapBottom;
			
			System.Drawing.SolidBrush textBrush = new System.Drawing.SolidBrush(IsActivated ? activeCaptionTextColor : inactiveCaptionTextColor);
			
			g.DrawString(Caption, Font, textBrush, rectCaptionText, captionStringFormat);

			textBrush.Dispose();

			closeButton.Enabled = currentActiveForm.EnableCloseButton;
		}

		//---------------------------------------------------------------------------
		protected override void DrawTabStrip(Graphics g)
		{
			DockableForm currentActiveForm = this.ActiveForm;
			if (currentActiveForm == null)
				return;
			
			CalculateTabs();

			Rectangle rectTabStrip = GetTabStripRectangle();

			if (rectTabStrip.IsEmpty)
				return;

			// Paint the background
			SolidBrush bkgndBrush = new SolidBrush(this.BackColor);
			g.FillRectangle(bkgndBrush, this.ClientRectangle);
			bkgndBrush.Dispose();
			
			g.DrawLine(SystemPens.ControlText, rectTabStrip.Left, rectTabStrip.Top,
				rectTabStrip.Right, rectTabStrip.Top);

			for (int i=0; i < VisibleFormsCount; i++)
				DrawTab(g, GetVisibleForm(i), GetTabRectangle(i));
		}

		//---------------------------------------------------------------------------
		protected override Rectangle CaptionRectangle
		{
			get
			{
				if (!HasCaption)
					return Rectangle.Empty;

				Rectangle rectTabWindow = TabbedPanelRectangle;
				int x, y, width;
				x = rectTabWindow.X;
				y = rectTabWindow.Y;
				width = rectTabWindow.Width;

				int height = Font.Height + CaptionMeasure.TextGapTop + CaptionMeasure.TextGapBottom;

				if (height < closeEnabledImage.Height + CaptionMeasure.ButtonGapTop + CaptionMeasure.ButtonGapBottom)
					height = closeEnabledImage.Height + CaptionMeasure.ButtonGapTop + CaptionMeasure.ButtonGapBottom;

				return new Rectangle(x, y, width, height);
			}
		}

		//---------------------------------------------------------------------------
		protected override int GetTabOriginalWidth(int index)
		{
			DockableForm aForm = GetVisibleForm(index);
			
			Graphics g = CreateGraphics();

			SizeF sizeString = g.MeasureString(aForm.Text, aForm.TabsFont);

			g.Dispose();
				
			return NormalTabMeasure.ImageWidth + (int)sizeString.Width + 1 + NormalTabMeasure.ImageGapLeft
					+ NormalTabMeasure.ImageGapRight + NormalTabMeasure.TextGapRight;
		}

		//---------------------------------------------------------------------------
		protected override Rectangle GetTabRectangle(int index)
		{
			Rectangle rectTabStrip = GetTabStripRectangle();

			DockableForm aForm = GetVisibleForm(index);
			return new Rectangle(aForm.TabX, rectTabStrip.Y, aForm.TabWidth, rectTabStrip.Height);
		}

		//---------------------------------------------------------------------------
		protected override Rectangle GetTabStripRectangle()
		{
			return GetTabStripRectangle(false);
		}

		//---------------------------------------------------------------------------
		protected override Rectangle GetTabStripRectangle(bool tabOnly)
		{
			if (VisibleFormsCount <= 1 || autoHide)
				return Rectangle.Empty;

			Rectangle rectTabWindow = TabbedPanelRectangle;

			int width = rectTabWindow.Width;
			int height = Math.Max(Font.Height, NormalTabMeasure.ImageHeight)
				+ NormalTabMeasure.ImageGapTop
				+ NormalTabMeasure.ImageGapBottom;
			int x = rectTabWindow.X;
			int y = rectTabWindow.Bottom - height;
			Rectangle rectCaption = CaptionRectangle;
			if (rectCaption.Contains(x, y))
				y = rectCaption.Y + rectCaption.Height;

			return new Rectangle(x, y, width, height);
		}

		//---------------------------------------------------------------------------
		protected override Region GetTestDropDragFrame(DockStyle dock, int containerIndex)
		{
			int dragSize = DockManager.MeasureContainer.DragSize;

			if (dock != DockStyle.Fill)
			{
				Rectangle rect = TabbedPanelRectangle;
				if (dock == DockStyle.Right)
					rect.X += rect.Width / 2;
				if (dock == DockStyle.Bottom)
					rect.Y += rect.Height / 2;
				if (dock == DockStyle.Left || dock == DockStyle.Right)
					rect.Width -= rect.Width / 2;
				if (dock == DockStyle.Top || dock == DockStyle.Bottom)
					rect.Height -= rect.Height / 2;
				rect.Location = PointToScreen(rect.Location);

				return DockManager.DragDrawHelper.CreateDragFrame(rect, dragSize);
			}
			else
			{
				Rectangle[] rects = new Rectangle[3];
				rects[2] = Rectangle.Empty;
				rects[0] = TabbedPanelRectangle;
				if (containerIndex != -1)
					rects[1] = GetTabRectangle(containerIndex);
				else
					rects[1] = Rectangle.Empty;

				if (!rects[1].IsEmpty)
				{
					rects[0].Height = rects[1].Top - rects[0].Top;
					rects[2].X = rects[1].X + dragSize;
					rects[2].Y = rects[1].Y - dragSize;
					rects[2].Width = rects[1].Width - 2 * dragSize;
					rects[2].Height = 2 * dragSize;
				}

				rects[0].Location = PointToScreen(rects[0].Location);
				rects[1].Location = PointToScreen(rects[1].Location);
				rects[2].Location = PointToScreen(rects[2].Location);
				return DockManager.DragDrawHelper.CreateDragFrame(rects, dragSize);
			}
		}

		#endregion

		#region DockableTabbedPanel public methods
		
		//---------------------------------------------------------------------------
		public void OnAutoHide(bool show)
		{
			if (show && !IsActivated)
			{
				// start the timer
				uint hovertime = 0;

				User32.SystemParametersInfo(Win32.SystemParametersInfoActions.GetMouseHoverTime,
					0, ref hovertime, 0);

				// assign a default value 400 in case of setting Timer.Interval invalid value exception
				if (((int)hovertime) <= 0)
					hovertime = 400;

				mouseTrackTimer.Interval = 2 * (int)hovertime;
				mouseTrackTimer.Enabled = true;
			}
			else
			{
				// stop the timer
				mouseTrackTimer.Enabled = false;
			}
		}

		#endregion
	}
}
