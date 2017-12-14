using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	//===========================================================================
	/// <summary>
	/// Description of DockableDocumentPanel
	/// </summary>
	internal class DockableDocumentPanel : TabbedPanel
	{
		internal class DocumentsListTabMeasure
		{
			public const int TabMaxWidth = 220;
			public const int ButtonGapTop = 4;
			public const int ButtonGapBottom = 4;
			public const int ButtonGapBetween = 2;
			public const int ButtonGapRight = 2;
			public const int TabGapTop = 3;
			public const int TabGapLeft = 6;
			public const int TabGapRight = 10;
			public const int TextExtraHeight = 6;
			public const int TextExtraWidth = 10;
		}

		#region DockableDocumentPanel private fields

		private TabbedPanelButton	closeButton = null;
		private Bitmap				closeEnabledImage = null;
		private Bitmap				closeDisabledImage = null;
		private TabbedPanelButton	scrollLeftButton = null;
		private Bitmap				scrollLeftEnabledImage = null;
		private Bitmap				scrollLeftDisabledImage = null;
		private TabbedPanelButton	scrollRightButton = null;
		private Bitmap				scrollRightEnabledImage = null;
		private Bitmap				scrollRightDisabledImage = null;
		private string				closeToolTip = String.Empty;
		private string				scrollLeftToolTip = String.Empty;
		private string				scrollRightToolTip = String.Empty;
		private StringFormat		tabStringFormat = null;
		private int					xOffset = 0;

		private const int defaultButtonHeight = 15;
		
		#endregion

		#region DockableDocumentPanel constructors

		//---------------------------------------------------------------------------
		public DockableDocumentPanel(DockableFormsContainer aContainer) : base(aContainer)
		{
			Initialize();

			if (aContainer != null && aContainer.DockManager != null)
				this.BackColor = aContainer.DockManager.BackColor;

			closeButton.BorderColor = scrollRightButton.BorderColor	= scrollLeftButton.BorderColor = SystemColors.GrayText;

		}

		//---------------------------------------------------------------------------
		public DockableDocumentPanel() : this(null)
		{
		}

		#endregion

		#region DockableDocumentPanel public properties
	
		//---------------------------------------------------------------------------
		public new System.Drawing.Color BackColor
		{
			get { return base.BackColor; }
			set 
			{ 
				base.BackColor = value; 

				closeButton.BackColor = scrollLeftButton.BackColor = scrollRightButton.BackColor = value;
			}
		}

		//---------------------------------------------------------------------------
		public new System.Drawing.Color ForeColor
		{
			get { return base.ForeColor; }
			set 
			{ 
				base.ForeColor = value; 

				closeButton.ForeColor = scrollLeftButton.ForeColor = scrollRightButton.ForeColor = value;
			}
		}

		#endregion

		#region DockableDocumentPanel private methods
		
		//---------------------------------------------------------------------------
		private void Initialize()
		{
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Dock.Images.CloseDocumentEnabled.bmp");
			if (imageStream != null)
				closeEnabledImage = new Bitmap(imageStream);

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Dock.Images.CloseDocumentDisabled.bmp");
			if (imageStream != null)
				closeDisabledImage = new Bitmap(imageStream);

			closeButton = new TabbedPanelButton(closeEnabledImage, closeDisabledImage);

			closeToolTip = Strings.CloseDocumentWindow;
			
			closeButton.Monochrome = false;
			closeButton.ToolTipText = closeToolTip;
			closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			closeButton.Click += new EventHandler(Close_Click);
			
			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Dock.Images.ScrollLeftEnabled.gif");
			if (imageStream != null)
				scrollLeftEnabledImage = new Bitmap(imageStream);

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Dock.Images.ScrollLeftDisabled.gif");
			if (imageStream != null)
				scrollLeftDisabledImage = new Bitmap(imageStream);

			scrollLeftButton = new TabbedPanelButton(scrollLeftEnabledImage, scrollLeftDisabledImage);

			scrollLeftToolTip = Strings.ScrollLeftDocumentWindow;

			scrollLeftButton.Monochrome = false;
			scrollLeftButton.Enabled = false;
			scrollLeftButton.ToolTipText = scrollLeftToolTip;
			scrollLeftButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			scrollLeftButton.Click += new EventHandler(ScrollLeft_Click);

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Dock.Images.ScrollRightEnabled.gif");
			if (imageStream != null)
				scrollRightEnabledImage = new Bitmap(imageStream);

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Dock.Images.ScrollRightDisabled.gif");
			if (imageStream != null)
				scrollRightDisabledImage = new Bitmap(imageStream);

			scrollRightButton = new TabbedPanelButton(scrollRightEnabledImage, scrollRightDisabledImage);

			scrollRightToolTip = Strings.ScrollRightDocumentWindow;

			scrollRightButton.Monochrome = false;
			scrollRightButton.Enabled = false;
			scrollRightButton.ToolTipText = scrollRightToolTip;
			scrollRightButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			scrollRightButton.Click += new EventHandler(ScrollRight_Click);

			Controls.AddRange(new Control[] {	scrollLeftButton,
												scrollRightButton,
												closeButton	});

			tabStringFormat = new StringFormat(StringFormat.GenericTypographic);
			tabStringFormat.Alignment = StringAlignment.Center;
			tabStringFormat.Trimming = StringTrimming.EllipsisCharacter;
			tabStringFormat.LineAlignment = StringAlignment.Center;
			tabStringFormat.FormatFlags = StringFormatFlags.NoWrap;
		}
		
		//---------------------------------------------------------------------------
		private void CalculateTabs()
		{
			int visibleFormsCount = VisibleFormsCount;
			if (visibleFormsCount == 0)
				return;

			Rectangle rectTabStrip = GetTabStripRectangle();
			int x = rectTabStrip.X + DocumentsListTabMeasure.TabGapLeft + xOffset;
			for (int i=0; i<visibleFormsCount; i++)
			{
				DockableForm aForm = GetVisibleForm(i);
				if (aForm == null)
					continue;
				
				aForm.TabX = x;
				aForm.TabWidth = Math.Min(GetTabOriginalWidth(i), DocumentsListTabMeasure.TabMaxWidth);
				x += aForm.TabWidth;
			}
		}

		//---------------------------------------------------------------------------
		private void DrawSingleDocumentCaption(Graphics g)
		{
			if (this.FormContainer == null || this.FormContainer.ShowTabsAlways || this.FormContainer.VisibleFormsCount > 1)
				return;

			DockableForm currentActiveForm = this.ActiveForm;
			if (currentActiveForm == null)
				return;

			Rectangle rectTabStrip = GetTabStripRectangle(true);
			if (rectTabStrip.IsEmpty)
				return;
	
			System.Drawing.SolidBrush captionTextBrush = new System.Drawing.SolidBrush(TabsActiveTabTextColor);

			StringFormat captionStringFormat = new StringFormat(StringFormat.GenericTypographic);
			captionStringFormat.Alignment = StringAlignment.Near;
			captionStringFormat.Trimming = StringTrimming.EllipsisCharacter;
			captionStringFormat.LineAlignment = StringAlignment.Center;
			captionStringFormat.FormatFlags = StringFormatFlags.NoWrap;
				
			g.DrawString(currentActiveForm.Text, this.TabsFont, captionTextBrush, rectTabStrip, captionStringFormat);
					
			captionTextBrush.Dispose();
		}
		
		//---------------------------------------------------------------------------
		private void DrawTab(Graphics g, DockableForm aForm, Rectangle rect)
		{
			if (this.FormContainer != null && !this.FormContainer.ShowTabsAlways && this.FormContainer.VisibleFormsCount <= 1)
				return;

			Rectangle rectText = rect;
			rectText.X += DocumentsListTabMeasure.TextExtraWidth / 2;
			rectText.Width -= DocumentsListTabMeasure.TextExtraWidth;
			System.Drawing.SolidBrush tabTextBrush = new System.Drawing.SolidBrush((ActiveForm == aForm && IsActivated) ? TabsActiveTabTextColor : TabsInactiveTabTextColor);
			if (ActiveForm == aForm)
			{
				System.Drawing.SolidBrush activeTabBackBrush = new System.Drawing.SolidBrush(aForm.BackColor);
				g.FillRectangle(activeTabBackBrush, rect);
				activeTabBackBrush.Dispose();

				g.DrawLine(SystemPens.ControlText,
					rect.X + rect.Width - 1, rect.Y,
					rect.X + rect.Width - 1, rect.Y + rect.Height - 1);
				
				if (IsActivated)
				{
					Font boldFont = new Font(this.TabsFont, FontStyle.Bold);
					
					g.DrawString(aForm.Text, boldFont, tabTextBrush, rectText, tabStringFormat);
					
					boldFont.Dispose();
				}
				else
					g.DrawString(aForm.Text, TabsFont, tabTextBrush, rectText, tabStringFormat);
			}
			else
			{
				if (GetVisibleFormIndex(ActiveForm) != GetVisibleFormIndex(aForm) + 1)
					g.DrawLine(SystemPens.GrayText,
						rect.X + rect.Width - 1, rect.Y,
						rect.X + rect.Width - 1, rect.Y + rect.Height - 1 - DocumentsListTabMeasure.TabGapTop);

				g.DrawString(aForm.Text, TabsFont, tabTextBrush, rectText, tabStringFormat);
			}
			tabTextBrush.Dispose();
		}

		#region DockableDocumentPanel private event handlers

		//---------------------------------------------------------------------------
		private void Close_Click(object sender, EventArgs e)
		{
			FormContainer.CloseActiveForm();
		}

		//---------------------------------------------------------------------------
		private void ScrollLeft_Click(object sender, EventArgs e)
		{
			Rectangle rectTabStrip = GetTabStripRectangle(true);

			int index;
			for (index=0; index < VisibleFormsCount; index++)
				if (GetTabRectangle(index).IntersectsWith(rectTabStrip))
					break;

			Rectangle rectTab = GetTabRectangle(index);
			if (rectTab.Left < rectTabStrip.Left)
				xOffset += rectTabStrip.Left - rectTab.Left;
			else if (index == 0)
				xOffset = 0;
			else
				xOffset += rectTabStrip.Left - GetTabRectangle(index - 1).Left;

			Invalidate();
		}
	
		//---------------------------------------------------------------------------
		private void ScrollRight_Click(object sender, EventArgs e)
		{
			Rectangle rectTabStrip = GetTabStripRectangle(true);

			int index;
			int visibleFormsCount = VisibleFormsCount;
			for (index=0; index<visibleFormsCount; index++)
				if (GetTabRectangle(index).IntersectsWith(rectTabStrip))
					break;

			if (index + 1 < visibleFormsCount)
			{
				xOffset -= GetTabRectangle(index + 1).Left - rectTabStrip.Left;
				CalculateTabs();
			}

			Rectangle rectLastTab = GetTabRectangle(visibleFormsCount - 1);
			if (rectLastTab.Right < rectTabStrip.Right)
				xOffset += rectTabStrip.Right - rectLastTab.Right;

			Invalidate();
		}

		#endregion

		#endregion

		#region DockableDocumentPanel protected overridden methods

		//---------------------------------------------------------------------------
		protected override void DrawTabStrip(Graphics g)
		{
			CalculateTabs();

			int visibleFormsCount = VisibleFormsCount;
			if (visibleFormsCount == 0)
				return;

			Rectangle rectTabStrip = GetTabStripRectangle();

			// Paint the background
			SolidBrush bkgndBrush = new SolidBrush(this.BackColor);
			g.FillRectangle(bkgndBrush, rectTabStrip);
			bkgndBrush.Dispose();

			int buttonHeight = (closeEnabledImage != null) ? closeEnabledImage.Height : defaultButtonHeight;
			int height = rectTabStrip.Height - DocumentsListTabMeasure.ButtonGapTop - DocumentsListTabMeasure.ButtonGapBottom;
			if (buttonHeight < height)
				buttonHeight = height;
	
			if (closeButton != null)
				closeButton.Size = new Size(buttonHeight, buttonHeight);
			if (scrollLeftButton != null)
				scrollLeftButton.Size = new Size(buttonHeight, buttonHeight);
			if (scrollRightButton != null)
				scrollRightButton.Size = new Size(buttonHeight, buttonHeight);
			
			int x = rectTabStrip.X + rectTabStrip.Width - DocumentsListTabMeasure.TabGapLeft
				- DocumentsListTabMeasure.ButtonGapRight - buttonHeight;
			int y = rectTabStrip.Y + DocumentsListTabMeasure.ButtonGapTop;
			
			closeButton.Location = new Point(x, y);
			Point point = closeButton.Location;
			point.Offset(-(DocumentsListTabMeasure.ButtonGapBetween + buttonHeight), 0);
			scrollRightButton.Location = point;
			point.Offset(-(DocumentsListTabMeasure.ButtonGapBetween + buttonHeight), 0);
			scrollLeftButton.Location = point;

			// Draw the tabs
			Rectangle rectTabOnly = GetTabStripRectangle(true);
			Rectangle rectTab = Rectangle.Empty;
			g.SetClip(rectTabOnly, CombineMode.Replace);

			if (visibleFormsCount > 1 || (this.FormContainer != null && this.FormContainer.ShowTabsAlways))
			{
				for (int i=0; i < visibleFormsCount; i++)
				{
					rectTab = GetTabRectangle(i);
					if (rectTab.IntersectsWith(rectTabOnly))
						DrawTab(g, GetVisibleForm(i), rectTab);
				}

				scrollLeftButton.Visible = true;
				scrollLeftButton.Enabled = (xOffset < 0);
				scrollRightButton.Visible = true;
				scrollRightButton.Enabled = (rectTab.Right > rectTabOnly.Right);
			}
			else
			{
				scrollLeftButton.Visible = false;
				scrollRightButton.Visible = false;
				DrawSingleDocumentCaption(g);
			}
			DockableForm currentActiveForm = this.ActiveForm;
			closeButton.Enabled = (currentActiveForm != null) ? currentActiveForm.EnableCloseButton : false;
		}

		//---------------------------------------------------------------------------
		protected override int GetTabOriginalWidth(int index)
		{
			DockableForm aForm = GetVisibleForm(index);

			if (aForm == null)
				return 0;

			Graphics g = CreateGraphics();

			SizeF sizeText;
			if (aForm == ActiveForm && IsActivated)
			{
				Font boldFont = new Font(this.TabsFont, FontStyle.Bold);
				sizeText = g.MeasureString(aForm.Text, boldFont, DocumentsListTabMeasure.TabMaxWidth, tabStringFormat);
				boldFont.Dispose();
			}
			else
				sizeText = g.MeasureString(aForm.Text, TabsFont, DocumentsListTabMeasure.TabMaxWidth, tabStringFormat);

			g.Dispose();

			return (int)Math.Ceiling(sizeText.Width) + 1 + DocumentsListTabMeasure.TextExtraWidth;
		}

		//---------------------------------------------------------------------------
		protected override Rectangle GetTabRectangle(int index)
		{
			DockableForm aForm = GetVisibleForm(index);

			if (aForm == null)
				return Rectangle.Empty;
			
			Rectangle rectTabStrip = GetTabStripRectangle();

			return new Rectangle
				(
				aForm.TabX, 
				rectTabStrip.Y + DocumentsListTabMeasure.TabGapTop,
				aForm.TabWidth, 
				rectTabStrip.Height - DocumentsListTabMeasure.TabGapTop
				);
		}

		//---------------------------------------------------------------------------
		protected override Rectangle GetTabStripRectangle()
		{
			return GetTabStripRectangle(false);
		}

		//---------------------------------------------------------------------------
		protected override Rectangle GetTabStripRectangle(bool tabOnly)
		{
			if (VisibleFormsCount == 0)
				return Rectangle.Empty;

			Rectangle rectTabWindow = TabbedPanelRectangle;
			int x = rectTabWindow.X;
			int y = rectTabWindow.Y;
			int width = rectTabWindow.Width;
			int buttonHeight = (closeEnabledImage != null) ? closeEnabledImage.Height : defaultButtonHeight;
			int height = Math.Max(TabsFont.Height + DocumentsListTabMeasure.TabGapTop + DocumentsListTabMeasure.TextExtraHeight,
				buttonHeight + DocumentsListTabMeasure.ButtonGapTop + DocumentsListTabMeasure.ButtonGapBottom);

			if (tabOnly)
			{
				x += DocumentsListTabMeasure.TabGapLeft;
				width -= DocumentsListTabMeasure.TabGapLeft + 
					DocumentsListTabMeasure.TabGapRight +
					DocumentsListTabMeasure.ButtonGapRight +
					closeButton.Width +
					scrollRightButton.Width +
					scrollLeftButton.Width +
					2 * DocumentsListTabMeasure.ButtonGapBetween;
			}

			return new Rectangle(x, y, width, height);
		}

		//---------------------------------------------------------------------------
		protected override Region GetTestDropDragFrame(DockStyle dock, int containerIndex)
		{
			int dragSize = DockManager.MeasureContainer.DragSize;

			Rectangle[] rects = new Rectangle[3];

			rects[0] = TabbedPanelRectangle;
			if (dock == DockStyle.Right)
				rects[0].X += rects[0].Width / 2;
			else if (dock == DockStyle.Bottom)
				rects[0].Y += rects[0].Height / 2;
			if (dock == DockStyle.Left || dock == DockStyle.Right)
				rects[0].Width -= rects[0].Width / 2;
			else if (dock == DockStyle.Top || dock == DockStyle.Bottom)
				rects[0].Height -= rects[0].Height / 2;

			if (dock != DockStyle.Fill)
				rects[1] = new Rectangle(rects[0].X + DocumentsListTabMeasure.TabGapLeft, rects[0].Y,
					rects[0].Width - 2 * DocumentsListTabMeasure.TabGapLeft, GetTabStripRectangle().Height);
			else if (containerIndex != -1)
				rects[1] = GetTabRectangle(containerIndex);
			else
				rects[1] = GetTabRectangle(0);

			rects[0].Y = rects[1].Top + rects[1].Height;
			rects[0].Height -= rects[1].Height;
			rects[2] = new Rectangle(rects[1].X + dragSize, rects[0].Y - dragSize,
				rects[1].Width - 2 * dragSize, 2 * dragSize);

			rects[0].Location = PointToScreen(rects[0].Location);
			rects[1].Location = PointToScreen(rects[1].Location);
			rects[2].Location = PointToScreen(rects[2].Location);
			
			return DockManager.DragDrawHelper.CreateDragFrame(rects, dragSize);
		}

		#endregion

		#region DockableDocumentPanel public overridden methods

		//---------------------------------------------------------------------------
		public override void EnsureTabVisible(DockableForm container)
		{
			CalculateTabs();

			Rectangle rectTabStrip = GetTabStripRectangle(true);
			Rectangle rectTab = GetTabRectangle(GetVisibleFormIndex(container));

			if (rectTab.Right > rectTabStrip.Right)
			{
				xOffset -= rectTab.Right - rectTabStrip.Right;
				rectTab.X -= rectTab.Right - rectTabStrip.Right;
			}

			if (rectTab.Left < rectTabStrip.Left)
				xOffset += rectTabStrip.Left - rectTab.Left;

			Invalidate();
		}

		#endregion
	}
}
