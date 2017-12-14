using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace WeifenLuo.WinFormsUI.Docking
{
	internal class VS2005DockPaneStrip : DockPaneStripBase
	{
		private class TabVS2005 : Tab
		{
			public TabVS2005(IDockContent content)
				: base(content)
			{
			}

			private int m_tabX;
			public int TabX
			{
				get { return m_tabX; }
				set { m_tabX = value; }
			}

			private int m_tabWidth;
			public int TabWidth
			{
				get { return m_tabWidth; }
				set { m_tabWidth = value; }
			}

			private int m_maxWidth;
			public int MaxWidth
			{
				get { return m_maxWidth; }
				set { m_maxWidth = value; }
			}

			private bool m_flag;
			protected internal bool Flag
			{
				get { return m_flag; }
				set { m_flag = value; }
			}
		}

		protected internal override DockPaneStripBase.Tab CreateTab(IDockContent content)
		{
			return new TabVS2005(content);
		}

		private sealed class InertButton : InertButtonBase
		{
			private Bitmap m_image0, m_image1;

			public InertButton(Bitmap image0, Bitmap image1)
				: base()
			{
                float dpiScale = 1;
                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    dpiScale = graphics.DpiX / 96;
                }

                m_image0 = new Bitmap(image0, new Size((int)(image0.Width * dpiScale), (int)(image0.Height * dpiScale)));
                m_image1 = new Bitmap(image1, new Size((int)(image1.Width * dpiScale), (int)(image1.Height * dpiScale)));
			}

			private int m_imageCategory = 0;
			public int ImageCategory
			{
				get { return m_imageCategory; }
				set
				{
					if (m_imageCategory == value)
						return;

					m_imageCategory = value;
					Invalidate();
				}
			}

			public override Bitmap Image
			{
				get { return ImageCategory == 0 ? m_image0 : m_image1; }
			}
		}

		#region consts
		private const int _ToolWindowStripGapTop = 0;
		private const int _ToolWindowStripGapBottom = 1;
		private const int _ToolWindowStripGapLeft = 0;
		private const int _ToolWindowStripGapRight = 0;
		private const int _ToolWindowImageHeight = 16;
		private const int _ToolWindowImageWidth = 16;
		private const int _ToolWindowImageGapTop = 3;
		private const int _ToolWindowImageGapBottom = 1;
		private const int _ToolWindowImageGapLeft = 2;
		private const int _ToolWindowImageGapRight = 0;
		private const int _ToolWindowTextGapRight = 3;
		private const int _ToolWindowTabSeperatorGapTop = 3;
		private const int _ToolWindowTabSeperatorGapBottom = 3;

		private const int _DocumentStripGapTop = 0;
		private const int _DocumentStripGapBottom = 1;
		private const int _DocumentTabMaxWidth = 500;
		private const int _DocumentButtonGapTop = 4;
		private const int _DocumentButtonGapBottom = 4;
		private const int _DocumentButtonGapBetween = 0;
		private const int _DocumentButtonGapRight = 3;
		private const int _DocumentTabGapTop = 3;
		private const int _DocumentTabGapLeft = 3;
		private const int _DocumentTabGapRight = 3;
		private const int _DocumentIconGapBottom = 1;
		private const int _DocumentIconGapLeft = 9;
		private const int _DocumentIconGapRight = 0;
		private const int _DocumentIconHeight = 16;
		private const int _DocumentIconWidth = 16;
		private const int _DocumentTextGapRight = 3;
		#endregion

		private bool drawCloseButtonActive = false;
		private bool drawCloseButton = false;
		private Tab activeMouseTab = null;

		private static Bitmap ImageButtonClose
		{
			get
			{
				return Resources.DockPane_Close.Clone() as Bitmap;
			}
		}

		private InertButton m_buttonClose;
		private InertButton ButtonClose
		{
			get
			{
				if (m_buttonClose == null)
				{
					m_buttonClose = new InertButton(ImageButtonClose, ImageButtonClose);
					//m_toolTip.SetToolTip(m_buttonClose, ToolTipClose);
					m_buttonClose.Click += new EventHandler(Close_Click);
					Controls.Add(m_buttonClose);
				}

				return m_buttonClose;
			}
		}

		private static Bitmap ImageButtonWindowList
		{
			get
			{ 
				return Resources.DockPane_Option.Clone() as Bitmap;
				
			}
		}

		private static Bitmap ImageButtonWindowListOverflow
		{
			get
			{
				return Resources.DockPane_OptionOverflow.Clone() as Bitmap;
			}
		}

		private InertButton m_buttonWindowList;
		private InertButton ButtonWindowList
		{
			get
			{
				if (m_buttonWindowList == null)
				{
					m_buttonWindowList = new InertButton(ImageButtonWindowList, ImageButtonWindowListOverflow);
					//m_toolTip.SetToolTip(m_buttonWindowList, ToolTipSelect);
					m_buttonWindowList.Click += new EventHandler(WindowList_Click);
					Controls.Add(m_buttonWindowList);
				}

				return m_buttonWindowList;
			}
		}

		private static GraphicsPath GraphicsPath
		{
			get { return VS2005AutoHideStrip.GraphicsPath; }
		}

		private IContainer m_components;
		private IContainer Components
		{
			get { return m_components; }
		}

		#region Customizable Properties
		private static int ToolWindowStripGapTop
		{
			get { return _ToolWindowStripGapTop; }
		}

		private static int ToolWindowStripGapBottom
		{
			get { return _ToolWindowStripGapBottom; }
		}

		private static int ToolWindowStripGapLeft
		{
			get { return _ToolWindowStripGapLeft; }
		}

		private static int ToolWindowStripGapRight
		{
			get { return _ToolWindowStripGapRight; }
		}

		private static int ToolWindowImageHeight
		{
			get { return _ToolWindowImageHeight; }
		}

		private static int ToolWindowImageWidth
		{
			get { return _ToolWindowImageWidth; }
		}

		private static int ToolWindowImageGapTop
		{
			get { return _ToolWindowImageGapTop; }
		}

		private static int ToolWindowImageGapBottom
		{
			get { return _ToolWindowImageGapBottom; }
		}

		private static int ToolWindowImageGapLeft
		{
			get { return _ToolWindowImageGapLeft; }
		}

		private static int ToolWindowImageGapRight
		{
			get { return _ToolWindowImageGapRight; }
		}

		private static int ToolWindowTextGapRight
		{
			get { return _ToolWindowTextGapRight; }
		}

		private static int ToolWindowTabSeperatorGapTop
		{
			get { return _ToolWindowTabSeperatorGapTop; }
		}

		private static int ToolWindowTabSeperatorGapBottom
		{
			get { return _ToolWindowTabSeperatorGapBottom; }
		}

		private static string _toolTipClose;
		private static string ToolTipClose
		{
			get
			{
				if (_toolTipClose == null)
					_toolTipClose = Strings.DockPaneStrip_ToolTipClose;
				return _toolTipClose;
			}
		}

		private static string _toolTipSelect;
		private static string ToolTipSelect
		{
			get
			{
				if (_toolTipSelect == null)
					_toolTipSelect = Strings.DockPaneStrip_ToolTipWindowList;
				return _toolTipSelect;
			}
		}

		private TextFormatFlags ToolWindowTextFormat
		{
			get
			{
				TextFormatFlags textFormat = TextFormatFlags.EndEllipsis |
					TextFormatFlags.HorizontalCenter |
					TextFormatFlags.SingleLine |
					TextFormatFlags.VerticalCenter;
				if (RightToLeft == RightToLeft.Yes)
					return textFormat | TextFormatFlags.RightToLeft | TextFormatFlags.Right;
				else
					return textFormat;
			}
		}

		private static int DocumentStripGapTop
		{
			get { return _DocumentStripGapTop; }
		}

		private static int DocumentStripGapBottom
		{
			get { return _DocumentStripGapBottom; }
		}

		private TextFormatFlags DocumentTextFormat
		{
			get
			{
				TextFormatFlags textFormat = TextFormatFlags.EndEllipsis |
					TextFormatFlags.SingleLine |
					TextFormatFlags.VerticalCenter |
					TextFormatFlags.HorizontalCenter;
				if (RightToLeft == RightToLeft.Yes)
					return textFormat | TextFormatFlags.RightToLeft;
				else
					return textFormat;
			}
		}

		private static int DocumentTabMaxWidth
		{
			get { return _DocumentTabMaxWidth; }
		}

		private static int DocumentButtonGapTop
		{
			get { return _DocumentButtonGapTop; }
		}

		private static int DocumentButtonGapBottom
		{
			get { return _DocumentButtonGapBottom; }
		}

		private static int DocumentButtonGapBetween
		{
			get { return _DocumentButtonGapBetween; }
		}

		private static int DocumentButtonGapRight
		{
			get { return _DocumentButtonGapRight; }
		}

		private static int DocumentTabGapTop
		{
			get { return _DocumentTabGapTop; }
		}

		private static int DocumentTabGapLeft
		{
			get { return _DocumentTabGapLeft; }
		}

		private static int DocumentTabGapRight
		{
			get { return _DocumentTabGapRight; }
		}

		private static int DocumentIconGapBottom
		{
			get { return _DocumentIconGapBottom; }
		}

		private static int DocumentIconGapLeft
		{
			get { return _DocumentIconGapLeft; }
		}

		private static int DocumentIconGapRight
		{
			get { return _DocumentIconGapRight; }
		}

		private static int DocumentIconWidth
		{
			get {

                float dpiScale = 1;
                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    dpiScale = graphics.DpiX / 96;
                }

                int iRet = (int) (_DocumentIconWidth * dpiScale);

                return iRet; 
            }
		}

		private static int DocumentIconHeight
		{

			get {
                float dpiScale = 1;
                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    dpiScale = graphics.DpiY / 96;
                }

                int iRet = (int)(_DocumentIconWidth * dpiScale);

                return iRet; 
            }
		}

		private static int DocumentTextGapRight
		{
			get { return _DocumentTextGapRight; }
		}

		private static Pen PenToolWindowTabBorder
		{
			get { return SystemPens.GrayText; }
		}

		private static Pen PenDocumentTabActiveBorder
		{
			get { return SystemPens.ControlDarkDark; }
		}

		private static Pen PenDocumentTabInactiveBorder
		{
			get { return SystemPens.GrayText; }
		}
		#endregion

		public VS2005DockPaneStrip(DockPane pane)
			: base(pane)
		{
			SetStyle(ControlStyles.ResizeRedraw |
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer, true);

			SuspendLayout();

			m_components = new Container();
			m_selectMenu = new ContextMenuStrip(Components);

			ResumeLayout();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Components.Dispose();
				if (m_boldFont != null)
				{
					m_boldFont.Dispose();
					m_boldFont = null;
				}

				if (m_font != null)
				{
					m_font.Dispose();
					m_font = null;
				}

				if (m_textFont != null)
				{
					m_textFont.Dispose();
					m_textFont = null;
				}
			}

			base.Dispose(disposing);
		}
			    
		private Font m_textFont;
		private Font TextFont
		{
			get
			{
				if (m_textFont == null)
					m_textFont = new Font("Segoe UI", 10);

				return m_textFont;
			}
		}

		private Font m_font;
		private Font m_boldFont;
		private Font BoldFont
		{
			get
			{
				if (IsDisposed)
					return null;

				if (m_boldFont == null)
				{
					m_font = TextFont;
					m_boldFont = new Font(TextFont, FontStyle.Regular);
				}
				else if (m_font != TextFont)
				{
					m_boldFont.Dispose();
					m_font = TextFont;
					m_boldFont = new Font(TextFont, FontStyle.Regular);
				}

				return m_boldFont;
			}
		}

		private int m_startDisplayingTab = 0;
		private int StartDisplayingTab
		{
			get { return m_startDisplayingTab; }
			set
			{
				m_startDisplayingTab = value;
				Invalidate();
			}
		}

		private int m_endDisplayingTab = 0;
		private int EndDisplayingTab
		{
			get { return m_endDisplayingTab; }
			set { m_endDisplayingTab = value; }
		}

		private int m_firstDisplayingTab = 0;
		private int FirstDisplayingTab
		{
			get { return m_firstDisplayingTab; }
			set { m_firstDisplayingTab = value; }
		}

		private bool m_documentTabsOverflow = false;
		private bool DocumentTabsOverflow
		{
			set
			{
				if (m_documentTabsOverflow == value)
					return;

				m_documentTabsOverflow = value;
				if (value)
					ButtonWindowList.ImageCategory = 1;
				else
					ButtonWindowList.ImageCategory = 0;
			}
		}
		
		protected internal override int MeasureHeight()
		{
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				return MeasureHeight_ToolWindow();
			else
				return MeasureHeight_Document();
		}

		private int MeasureHeight_ToolWindow()
		{
			if (DockPane.IsAutoHide || Tabs.Count <= 1)
				return 0;

			int height = Math.Max(TextFont.Height, ToolWindowImageHeight + ToolWindowImageGapTop + ToolWindowImageGapBottom)
				+ ToolWindowStripGapTop + ToolWindowStripGapBottom;

			return height;
		}

		private int MeasureHeight_Document()
		{
			int height = Math.Max(TextFont.Height + DocumentTabGapTop,
				ButtonClose.Height + DocumentButtonGapTop + DocumentButtonGapBottom)
				+ DocumentStripGapBottom + DocumentStripGapTop;

			return height;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				Rectangle rect = TabsRectangle;

				if (Appearance == DockPane.AppearanceStyle.Document)
				{
					rect.X -= DocumentTabGapLeft;

					// Add these values back in so that the DockStrip color is drawn
					// beneath the close button and window list button.
					rect.Width += DocumentTabGapLeft +
						DocumentTabGapRight +
						DocumentButtonGapRight +
						ButtonClose.Width +
						ButtonWindowList.Width;

					// It is possible depending on the DockPanel DocumentStyle to have
					// a Document without a DockStrip.
					if (rect.Width > 0 && rect.Height > 0)
					{
						Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.DockStripGradient.StartColor;
						Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.DockStripGradient.EndColor;
						LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.DockStripGradient.LinearGradientMode;
						using (LinearGradientBrush brush = new LinearGradientBrush(rect, startColor, endColor, gradientMode))
						{
							e.Graphics.FillRectangle(brush, rect);
						}
					}
				}
				else
				{
					Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient.StartColor;
					Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient.EndColor;
					LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient.LinearGradientMode;
					using (LinearGradientBrush brush = new LinearGradientBrush(rect, startColor, endColor, gradientMode))
					{
						e.Graphics.FillRectangle(brush, rect);
					}
				}
				base.OnPaint(e);
				CalculateTabs();
				if (Appearance == DockPane.AppearanceStyle.Document && DockPane.ActiveContent != null)
				{
					if (EnsureDocumentTabVisible(DockPane.ActiveContent, false))
						CalculateTabs();
				}

				DrawTabStrip(e.Graphics);
			}
			catch 
			{
			}
		}

		protected override void OnRefreshChanges()
		{
			SetInertButtons();
			Invalidate();
		}

		protected internal override GraphicsPath GetOutline(int index)
		{

			if (Appearance == DockPane.AppearanceStyle.Document)
				return GetOutline_Document(index);
			else
				return GetOutline_ToolWindow(index);

		}

		private GraphicsPath GetOutline_Document(int index)
		{
			Rectangle rectTab = GetTabRectangle(index);
			rectTab.X -= rectTab.Height / 2;
			rectTab.Intersect(TabsRectangle);
			rectTab = RectangleToScreen(DrawHelper.RtlTransform(this, rectTab));
			Rectangle rectPaneClient = DockPane.RectangleToScreen(DockPane.ClientRectangle);

			GraphicsPath path = new GraphicsPath();
			GraphicsPath pathTab = GetTabOutline_Document(Tabs[index], true, true, true);
			path.AddPath(pathTab, true);

			if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
			{
				path.AddLine(rectTab.Right, rectTab.Top, rectPaneClient.Right, rectTab.Top);
				path.AddLine(rectPaneClient.Right, rectTab.Top, rectPaneClient.Right, rectPaneClient.Top);
				path.AddLine(rectPaneClient.Right, rectPaneClient.Top, rectPaneClient.Left, rectPaneClient.Top);
				path.AddLine(rectPaneClient.Left, rectPaneClient.Top, rectPaneClient.Left, rectTab.Top);
				path.AddLine(rectPaneClient.Left, rectTab.Top, rectTab.Right, rectTab.Top);
			}
			else
			{
				path.AddLine(rectTab.Right, rectTab.Bottom, rectPaneClient.Right, rectTab.Bottom);
				path.AddLine(rectPaneClient.Right, rectTab.Bottom, rectPaneClient.Right, rectPaneClient.Bottom);
				path.AddLine(rectPaneClient.Right, rectPaneClient.Bottom, rectPaneClient.Left, rectPaneClient.Bottom);
				path.AddLine(rectPaneClient.Left, rectPaneClient.Bottom, rectPaneClient.Left, rectTab.Bottom);
				path.AddLine(rectPaneClient.Left, rectTab.Bottom, rectTab.Right, rectTab.Bottom);
			}
			return path;
		}

		private GraphicsPath GetOutline_ToolWindow(int index)
		{
			Rectangle rectTab = GetTabRectangle(index);
			rectTab.Intersect(TabsRectangle);
			rectTab = RectangleToScreen(DrawHelper.RtlTransform(this, rectTab));
			int y = rectTab.Top;
			Rectangle rectPaneClient = DockPane.RectangleToScreen(DockPane.ClientRectangle);

			GraphicsPath path = new GraphicsPath();
			GraphicsPath pathTab = GetTabOutline(Tabs[index], true, true);
			path.AddPath(pathTab, true);
			path.AddLine(rectTab.Left, rectTab.Top, rectPaneClient.Left, rectTab.Top);
			path.AddLine(rectPaneClient.Left, rectTab.Top, rectPaneClient.Left, rectPaneClient.Top);
			path.AddLine(rectPaneClient.Left, rectPaneClient.Top, rectPaneClient.Right, rectPaneClient.Top);
			path.AddLine(rectPaneClient.Right, rectPaneClient.Top, rectPaneClient.Right, rectTab.Top);
			path.AddLine(rectPaneClient.Right, rectTab.Top, rectTab.Right, rectTab.Top);
			return path;
		}

		private void CalculateTabs()
		{
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				CalculateTabs_ToolWindow();
			else
				CalculateTabs_Document();
		}

		private void CalculateTabs_ToolWindow()
		{
			if (Tabs.Count <= 1 || DockPane.IsAutoHide)
				return;

			Rectangle rectTabStrip = TabStripRectangle;

			// Calculate tab widths
			int countTabs = Tabs.Count;
			foreach (TabVS2005 tab in Tabs)
			{
				tab.MaxWidth = GetMaxTabWidth(Tabs.IndexOf(tab));
				tab.Flag = false;
			}

			// Set tab whose max width less than average width
			bool anyWidthWithinAverage = true;
			int totalWidth = rectTabStrip.Width - ToolWindowStripGapLeft - ToolWindowStripGapRight;
			int totalAllocatedWidth = 0;
			int averageWidth = totalWidth / countTabs;
			int remainedTabs = countTabs;
			for (anyWidthWithinAverage = true; anyWidthWithinAverage && remainedTabs > 0; )
			{
				anyWidthWithinAverage = false;
				foreach (TabVS2005 tab in Tabs)
				{
					if (tab.Flag)
						continue;

					if (tab.MaxWidth <= averageWidth)
					{
						tab.Flag = true;
						tab.TabWidth = tab.MaxWidth;
						totalAllocatedWidth += tab.TabWidth;
						anyWidthWithinAverage = true;
						remainedTabs--;
					}
				}
				if (remainedTabs != 0)
					averageWidth = (totalWidth - totalAllocatedWidth) / remainedTabs;
			}

			// If any tab width not set yet, set it to the average width
			if (remainedTabs > 0)
			{
				int roundUpWidth = (totalWidth - totalAllocatedWidth) - (averageWidth * remainedTabs);
				foreach (TabVS2005 tab in Tabs)
				{
					if (tab.Flag)
						continue;

					tab.Flag = true;
					if (roundUpWidth > 0)
					{
						tab.TabWidth = averageWidth + 1;
						roundUpWidth--;
					}
					else
						tab.TabWidth = averageWidth;
				}
			}

			// Set the X position of the tabs
			int x = rectTabStrip.X + ToolWindowStripGapLeft;
			foreach (TabVS2005 tab in Tabs)
			{
				tab.TabX = x;
				x += tab.TabWidth;
			}
		}

		private bool CalculateDocumentTab(Rectangle rectTabStrip, ref int x, int index)
		{
			bool overflow = false;

			TabVS2005 tab = Tabs[index] as TabVS2005;

			tab.MaxWidth = GetMaxTabWidth(index);
			int width = Math.Min(tab.MaxWidth, DocumentTabMaxWidth);
			if (x + width < rectTabStrip.Right || index == StartDisplayingTab || DockPane.DockPanel.FitTabs)
			{
				tab.TabX = x;
				tab.TabWidth = width;
				EndDisplayingTab = index;
			}
			else
			{
				tab.TabX = 0;
				tab.TabWidth = 0;
				overflow = true;
			}
			x += width;

			return overflow;
		}

		/// <summary>
		/// Calculate which tabs are displayed and in what order.
		/// </summary>
		private void CalculateTabs_Document()
		{
			if (m_startDisplayingTab >= Tabs.Count)
				m_startDisplayingTab = 0;

			Rectangle rectTabStrip = TabsRectangle;

			int x = rectTabStrip.X /*+ rectTabStrip.Height / 2*/;
			bool overflow = false;

			//try calculating more times until tabs fit in the available space
			if (DockPane.DockPanel.FitTabs)
			{
				//step 1: calculate total overflow
				for (int i = 0; i < Tabs.Count; i++)
					CalculateDocumentTab(rectTabStrip, ref x, i);

				int exceeded = x - rectTabStrip.Right;
				
				//step 2: if overflow is greater than zero, divide overflow among tabs
				if (exceeded > 0)
				{
					int checkSum = 0;
					float ratio = (float)rectTabStrip.Width / (float)(rectTabStrip.Width + exceeded);
					for (int i = 0; i < Tabs.Count; i++)
					{
						TabVS2005 tab = Tabs[i] as TabVS2005;
						checkSum += (tab.TabWidth = (int)(tab.TabWidth * ratio));
					}
				}
			}
			else
			{
				
				// Originally all new documents that were considered overflow
				// (not enough pane strip space to show all tabs) were added to
				// the far left (assuming not right to left) and the tabs on the
				// right were dropped from view. If StartDisplayingTab is not 0
				// then we are dealing with making sure a specific tab is kept in focus.
				if (m_startDisplayingTab > 0)
				{
					int tempX = x;

					// Add the active tab and tabs to the left
					for (int i = StartDisplayingTab; i >= 0; i--)
						CalculateDocumentTab(rectTabStrip, ref tempX, i);

					// Store which tab is the first one displayed so that it
					// will be drawn correctly (without part of the tab cut off)
					FirstDisplayingTab = EndDisplayingTab;

					tempX = x; // Reset X location because we are starting over

					// Start with the first tab displayed - name is a little misleading.
					// Loop through each tab and set its location. If there is not enough
					// room for all of them overflow will be returned.
					for (int i = EndDisplayingTab; i < Tabs.Count; i++)
						overflow = CalculateDocumentTab(rectTabStrip, ref tempX, i);

					// If not all tabs are shown then we have an overflow.
					if (FirstDisplayingTab != 0)
						overflow = true;
				}
				else
				{
					for (int i = StartDisplayingTab; i < Tabs.Count; i++)
						overflow = CalculateDocumentTab(rectTabStrip, ref x, i);
					for (int i = 0; i < StartDisplayingTab; i++)
						overflow = CalculateDocumentTab(rectTabStrip, ref x, i);

					FirstDisplayingTab = StartDisplayingTab;
				}
			}
			if (!overflow)
			{
				m_startDisplayingTab = 0;
				FirstDisplayingTab = 0;
				x = rectTabStrip.X + rectTabStrip.Height / 2;
				foreach (TabVS2005 tab in Tabs)
				{
					tab.TabX = x;
					x += tab.TabWidth;
				}
			}
			DocumentTabsOverflow = overflow;
		}

		protected internal override void EnsureTabVisible(IDockContent content)
		{
			if (Appearance != DockPane.AppearanceStyle.Document || !Tabs.Contains(content))
				return;

			CalculateTabs();
			EnsureDocumentTabVisible(content, true);
		}

		private bool EnsureDocumentTabVisible(IDockContent content, bool repaint)
		{
			int index = Tabs.IndexOf(content);
			if (index == -1)
				return false;
			TabVS2005 tab = Tabs[index] as TabVS2005;
			if (tab.TabWidth != 0)
				return false;

			StartDisplayingTab = index;
			if (repaint)
				Invalidate();

			return true;
		}

		private int GetMaxTabWidth(int index)
		{
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				return GetMaxTabWidth_ToolWindow(index);
			else
				return GetMaxTabWidth_Document(index);
		}

		private int GetMaxTabWidth_ToolWindow(int index)
		{
			IDockContent content = Tabs[index].Content;
			Size sizeString = TextRenderer.MeasureText(content.DockHandler.TabText, TextFont);
			return ToolWindowImageWidth + sizeString.Width + ToolWindowImageGapLeft
				+ ToolWindowImageGapRight + ToolWindowTextGapRight;
		}

		private int GetMaxTabWidth_Document(int index)
		{
			IDockContent content = Tabs[index].Content;

			int height = GetTabRectangle_Document(index).Height;

			Size sizeText = TextRenderer.MeasureText(content.DockHandler.TabText, BoldFont, new Size(DocumentTabMaxWidth, height), DocumentTextFormat);
			int max = DockPane.DockPanel.ShowDocumentIcon
				? sizeText.Width + DocumentIconWidth + DocumentIconGapLeft + DocumentIconGapRight + DocumentTextGapRight
				: sizeText.Width + DocumentIconGapLeft + DocumentTextGapRight;
			return max;
		}

		private void DrawTabStrip(Graphics g)
		{
			if (Appearance == DockPane.AppearanceStyle.Document)
				DrawTabStrip_Document(g);
			else
				DrawTabStrip_ToolWindow(g);
		}

		private void DrawTabStrip_Document(Graphics g)
		{
			int count = Tabs.Count;
			if (count == 0)
				return;

			Rectangle rectTabStrip = TabStripRectangle;

			// Draw the tabs
			Rectangle rectTabOnly = TabsRectangle;
			Rectangle rectTab = Rectangle.Empty;
			TabVS2005 tabActive = null;
			g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
			for (int i = 0; i < count; i++)
			{
				rectTab = GetTabRectangle(i);
				if (Tabs[i].Content == DockPane.ActiveContent)
				{
					tabActive = Tabs[i] as TabVS2005;
					continue;
				}
				if (rectTab.IntersectsWith(rectTabOnly))
					DrawTab(g, Tabs[i] as TabVS2005, rectTab);
			}

			g.SetClip(rectTabStrip);

			if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
				g.DrawLine(PenDocumentTabActiveBorder, rectTabStrip.Left, rectTabStrip.Top + 1,
					rectTabStrip.Right, rectTabStrip.Top + 1);
			else
				g.DrawLine(PenDocumentTabActiveBorder, rectTabStrip.Left, rectTabStrip.Bottom - 1,
					rectTabStrip.Right, rectTabStrip.Bottom - 1);

			g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
			if (tabActive != null)
			{
				rectTab = GetTabRectangle(Tabs.IndexOf(tabActive));
				if (rectTab.IntersectsWith(rectTabOnly))
					DrawTab(g, tabActive, rectTab);
			}
		}

		private void DrawTabStrip_ToolWindow(Graphics g)
		{
			Rectangle rectTabStrip = TabStripRectangle;

			g.DrawLine(PenToolWindowTabBorder, rectTabStrip.Left, rectTabStrip.Top,
				rectTabStrip.Right, rectTabStrip.Top);

			for (int i = 0; i < Tabs.Count; i++)
				DrawTab(g, Tabs[i] as TabVS2005, GetTabRectangle(i));
		}

		private Rectangle GetTabRectangle(int index)
		{
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				return GetTabRectangle_ToolWindow(index);
			else
				return GetTabRectangle_Document(index);
		}

		private Rectangle GetTabRectangle_ToolWindow(int index)
		{
			Rectangle rectTabStrip = TabStripRectangle;

			TabVS2005 tab = (TabVS2005)(Tabs[index]);
			return new Rectangle(tab.TabX, rectTabStrip.Y, tab.TabWidth, rectTabStrip.Height);
		}

		private Rectangle GetTabRectangle_Document(int index)
		{
			Rectangle rectTabStrip = TabStripRectangle;
			TabVS2005 tab = (TabVS2005)Tabs[index];

			Rectangle rect = new Rectangle();
			rect.X = tab.TabX;
			rect.Width = tab.TabWidth;
			rect.Height = rectTabStrip.Height - DocumentTabGapTop;

			if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
				rect.Y = rectTabStrip.Y + DocumentStripGapBottom;
			else
				rect.Y = rectTabStrip.Y + DocumentTabGapTop;

			return rect;
		}

		private void DrawTab(Graphics g, TabVS2005 tab, Rectangle rect)
		{
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				DrawTab_ToolWindow(g, tab, rect);
			else
				DrawTab_Document(g, tab, rect);
		}

		private GraphicsPath GetTabOutline(Tab tab, bool rtlTransform, bool toScreen)
		{
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				return GetTabOutline_ToolWindow(tab, rtlTransform, toScreen);
			else
				return GetTabOutline_Document(tab, rtlTransform, toScreen, false);
		}

		private GraphicsPath GetTabOutline_ToolWindow(Tab tab, bool rtlTransform, bool toScreen)
		{
			Rectangle rect = GetTabRectangle(Tabs.IndexOf(tab));
			if (rtlTransform)
				rect = DrawHelper.RtlTransform(this, rect);
			if (toScreen)
				rect = RectangleToScreen(rect);

			DrawHelper.GetRoundedCornerTab(GraphicsPath, rect, false);
			return GraphicsPath;
		}

		private GraphicsPath GetTabOutline_Document(Tab tab, bool rtlTransform, bool toScreen, bool full)
		{
			GraphicsPath.Reset();
			Rectangle rect = GetTabRectangle(Tabs.IndexOf(tab));
			GraphicsPath.AddLine(rect.Left, rect.Top, rect.Right, rect.Top);
			GraphicsPath.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom);
			GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
			GraphicsPath.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Top);
			return GraphicsPath;

			//int curveSize = 6;
			//if (rtlTransform)
			//	rect = DrawHelper.RtlTransform(this, rect);
			//if (toScreen)
			//	rect = RectangleToScreen(rect);

			//// Draws the full angle piece for active content (or first tab)
			//if (tab.Content == DockPane.ActiveContent || full || Tabs.IndexOf(tab) == FirstDisplayingTab)
			//{
			//	if (RightToLeft == RightToLeft.Yes)
			//	{
			//		if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
			//		{
			//			// For some reason the next line draws a line that is not hidden like it is when drawing the tab strip on top.
			//			// It is not needed so it has been commented out.
			//			//GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Right + rect.Height / 2, rect.Bottom);
			//			GraphicsPath.AddLine(rect.Right + rect.Height / 2, rect.Top, rect.Right - rect.Height / 2 + curveSize / 2, rect.Bottom - curveSize / 2);
			//		}
			//		else
			//		{
			//			GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Right + rect.Height / 2, rect.Bottom);
			//			GraphicsPath.AddLine(rect.Right + rect.Height / 2, rect.Bottom, rect.Right - rect.Height / 2 + curveSize / 2, rect.Top + curveSize / 2);
			//		}
			//	}
			//	else
			//	{
			//		if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
			//		{
			//			// For some reason the next line draws a line that is not hidden like it is when drawing the tab strip on top.
			//			// It is not needed so it has been commented out.
			//			//GraphicsPath.AddLine(rect.Left, rect.Top, rect.Left - rect.Height / 2, rect.Top);
			//			GraphicsPath.AddLine(rect.Left - rect.Height / 2, rect.Top, rect.Left + rect.Height / 2 - curveSize / 2, rect.Bottom - curveSize / 2);
			//		}
			//		else
			//		{
			//			GraphicsPath.AddLine(rect.Left, rect.Bottom, rect.Left - rect.Height / 2, rect.Bottom);
			//			GraphicsPath.AddLine(rect.Left - rect.Height / 2, rect.Bottom, rect.Left + rect.Height / 2 - curveSize / 2, rect.Top + curveSize / 2);
			//		}
			//	}
			//}
			//// Draws the partial angle for non-active content
			//else
			//{
			//	if (RightToLeft == RightToLeft.Yes)
			//	{
			//		if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
			//		{
			//			GraphicsPath.AddLine(rect.Right, rect.Top, rect.Right, rect.Top + rect.Height / 2);
			//			GraphicsPath.AddLine(rect.Right, rect.Top + rect.Height / 2, rect.Right - rect.Height / 2 + curveSize / 2, rect.Bottom - curveSize / 2);
			//		}
			//		else
			//		{
			//			GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom - rect.Height / 2);
			//			GraphicsPath.AddLine(rect.Right, rect.Bottom - rect.Height / 2, rect.Right - rect.Height / 2 + curveSize / 2, rect.Top + curveSize / 2);
			//		}
			//	}
			//	else
			//	{
			//		if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
			//		{
			//			GraphicsPath.AddLine(rect.Left, rect.Top, rect.Left, rect.Top + rect.Height / 2);
			//			GraphicsPath.AddLine(rect.Left, rect.Top + rect.Height / 2, rect.Left + rect.Height / 2 - curveSize / 2, rect.Bottom - curveSize / 2);
			//		}
			//		else
			//		{
			//			GraphicsPath.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Bottom - rect.Height / 2);
			//			GraphicsPath.AddLine(rect.Left, rect.Bottom - rect.Height / 2, rect.Left + rect.Height / 2 - curveSize / 2, rect.Top + curveSize / 2);
			//		}
			//	}
			//}

			//if (RightToLeft == RightToLeft.Yes)
			//{
			//	if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
			//	{
			//		// Draws the bottom horizontal line (short side)
			//		GraphicsPath.AddLine(rect.Right - rect.Height / 2 - curveSize / 2, rect.Bottom, rect.Left + curveSize / 2, rect.Bottom);

			//		// Drawing the rounded corner is not necessary. The path is automatically connected
			//		//GraphicsPath.AddArc(new Rectangle(rect.Left, rect.Top, curveSize, curveSize), 180, 90);
			//	}
			//	else
			//	{
			//		// Draws the bottom horizontal line (short side)
			//		GraphicsPath.AddLine(rect.Right - rect.Height / 2 - curveSize / 2, rect.Top, rect.Left + curveSize / 2, rect.Top);
			//		GraphicsPath.AddArc(new Rectangle(rect.Left, rect.Top, curveSize, curveSize), 180, 90);
			//	}
			//}
			//else
			//{
			//	if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
			//	{
			//		// Draws the bottom horizontal line (short side)
			//		GraphicsPath.AddLine(rect.Left + rect.Height / 2 + curveSize / 2, rect.Bottom, rect.Right - curveSize / 2, rect.Bottom);

			//		// Drawing the rounded corner is not necessary. The path is automatically connected
			//		//GraphicsPath.AddArc(new Rectangle(rect.Right - curveSize, rect.Bottom, curveSize, curveSize), 90, -90);
			//	}
			//	else
			//	{
			//		// Draws the top horizontal line (short side)
			//		GraphicsPath.AddLine(rect.Left + rect.Height / 2 + curveSize / 2, rect.Top, rect.Right - curveSize / 2, rect.Top);

			//		// Draws the rounded corner oppposite the angled side
			//		GraphicsPath.AddArc(new Rectangle(rect.Right - curveSize, rect.Top, curveSize, curveSize), -90, 90);
			//	}
			//}

			//if (Tabs.IndexOf(tab) != EndDisplayingTab &&
			//	(Tabs.IndexOf(tab) != Tabs.Count - 1 && Tabs[Tabs.IndexOf(tab) + 1].Content == DockPane.ActiveContent)
			//	&& !full)
			//{
			//	if (RightToLeft == RightToLeft.Yes)
			//	{
			//		if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
			//		{
			//			GraphicsPath.AddLine(rect.Left, rect.Bottom - curveSize / 2, rect.Left, rect.Bottom - rect.Height / 2);
			//			GraphicsPath.AddLine(rect.Left, rect.Bottom - rect.Height / 2, rect.Left + rect.Height / 2, rect.Top);
			//		}
			//		else
			//		{
			//			GraphicsPath.AddLine(rect.Left, rect.Top + curveSize / 2, rect.Left, rect.Top + rect.Height / 2);
			//			GraphicsPath.AddLine(rect.Left, rect.Top + rect.Height / 2, rect.Left + rect.Height / 2, rect.Bottom);
			//		}
			//	}
			//	else
			//	{
			//		if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
			//		{
			//			GraphicsPath.AddLine(rect.Right, rect.Bottom - curveSize / 2, rect.Right, rect.Bottom - rect.Height / 2);
			//			GraphicsPath.AddLine(rect.Right, rect.Bottom - rect.Height / 2, rect.Right - rect.Height / 2, rect.Top);
			//		}
			//		else
			//		{
			//			GraphicsPath.AddLine(rect.Right, rect.Top + curveSize / 2, rect.Right, rect.Top + rect.Height / 2);
			//			GraphicsPath.AddLine(rect.Right, rect.Top + rect.Height / 2, rect.Right - rect.Height / 2, rect.Bottom);
			//		}
			//	}
			//}
			//else
			//{
			//	// Draw the vertical line opposite the angled side
			//	if (RightToLeft == RightToLeft.Yes)
			//	{
			//		if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
			//			GraphicsPath.AddLine(rect.Left, rect.Bottom - curveSize / 2, rect.Left, rect.Top);
			//		else
			//			GraphicsPath.AddLine(rect.Left, rect.Top + curveSize / 2, rect.Left, rect.Bottom);
			//	}
			//	else
			//	{
			//		if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
			//			GraphicsPath.AddLine(rect.Right, rect.Bottom - curveSize / 2, rect.Right, rect.Top);
			//		else
			//			GraphicsPath.AddLine(rect.Right, rect.Top + curveSize / 2, rect.Right, rect.Bottom);
			//	}
			//}

			//return GraphicsPath;
		}

		private void DrawTab_ToolWindow(Graphics g, TabVS2005 tab, Rectangle rect)
		{
			Rectangle rectIcon = new Rectangle(
				rect.X + ToolWindowImageGapLeft,
				rect.Y + rect.Height - 1 - ToolWindowImageGapBottom - ToolWindowImageHeight,
				ToolWindowImageWidth, ToolWindowImageHeight);
			Rectangle rectText = rectIcon;
			rectText.X += rectIcon.Width + ToolWindowImageGapRight;
			rectText.Width = rect.Width - rectIcon.Width - ToolWindowImageGapLeft -
				ToolWindowImageGapRight - ToolWindowTextGapRight;

			Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
			rectText = DrawHelper.RtlTransform(this, rectText);
			rectIcon = DrawHelper.RtlTransform(this, rectIcon);
			GraphicsPath path = GetTabOutline(tab, true, false);
			if (DockPane.ActiveContent == tab.Content)
			{
				Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.StartColor;
				Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.EndColor;
				LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.LinearGradientMode;
				g.FillPath(new LinearGradientBrush(rectTab, startColor, endColor, gradientMode), path);
				g.DrawPath(PenToolWindowTabBorder, path);

				Color textColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.TextColor;
				TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, textColor, ToolWindowTextFormat);
			}
			else
			{
				Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.StartColor;
				Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.EndColor;
				LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.LinearGradientMode;
				g.FillPath(new LinearGradientBrush(rectTab, startColor, endColor, gradientMode), path);

				if (Tabs.IndexOf(DockPane.ActiveContent) != Tabs.IndexOf(tab) + 1)
				{
					Point pt1 = new Point(rect.Right, rect.Top + ToolWindowTabSeperatorGapTop);
					Point pt2 = new Point(rect.Right, rect.Bottom - ToolWindowTabSeperatorGapBottom);
					g.DrawLine(PenToolWindowTabBorder, DrawHelper.RtlTransform(this, pt1), DrawHelper.RtlTransform(this, pt2));
				}

				Color textColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.TextColor;
				TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, textColor, ToolWindowTextFormat);
			}

			if (rectTab.Contains(rectIcon))
				g.DrawIcon(tab.Content.DockHandler.Icon, rectIcon);
		}

		private void DrawTab_Document(Graphics g, TabVS2005 tab, Rectangle rect)
		{
			if (tab.TabWidth == 0)
				return;

			Rectangle rectIcon = CalculateIconRect(rect);
			Rectangle buttonIcon = CalculateCloseButtonRect(rect);
			Rectangle rectText = rectIcon;
			if (DockPane.DockPanel.ShowDocumentIcon)
			{
				rectText.X += rectIcon.Width + DocumentIconGapRight;
				rectText.Y = rect.Y;
				rectText.Width = rect.Width - rectIcon.Width - DocumentIconGapLeft -
					DocumentIconGapRight - DocumentTextGapRight + 1/*Small adjustment*/;
				rectText.Height = rect.Height;
			}
			else
				rectText.Width = rect.Width - DocumentIconGapLeft - DocumentTextGapRight;

			Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
			Rectangle rectBack = DrawHelper.RtlTransform(this, rect);
			rectBack.Width += rect.X;
			rectBack.X = 0;

			rectText = DrawHelper.RtlTransform(this, rectText);
			rectIcon = DrawHelper.RtlTransform(this, rectIcon);
			GraphicsPath path = GetTabOutline(tab, true, false);
			if (DockPane.ActiveContent == tab.Content)
			{
				Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.StartColor;
				Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.EndColor;
				LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.LinearGradientMode;
				g.FillPath(new LinearGradientBrush(rectBack, startColor, endColor, gradientMode), path);

				if (DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.ShowBorder)
					g.DrawPath(PenDocumentTabActiveBorder, path);

				Color textColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.TextColor;
				if (DockPane.IsActiveDocumentPane)
					TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, BoldFont, rectText, textColor, DocumentTextFormat);
				else
					TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, textColor, DocumentTextFormat);
			}
			else
			{
				Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.StartColor;
				Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.EndColor;
				LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.LinearGradientMode;
				g.FillPath(new LinearGradientBrush(rectBack, startColor, endColor, gradientMode), path);

				if (DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.ShowBorder)
					g.DrawPath(PenDocumentTabActiveBorder, path);

				Color textColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.TextColor;
				TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, textColor, DocumentTextFormat);
			}

			if (DockPane.DockPanel.ShowCloseButtonOnTab && activeMouseTab == tab)
			{
				DrawCloseButtonOnTab(g, buttonIcon);
				g.FillRectangle(new SolidBrush(Color.FromArgb(50, DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.StartColor)), rect);

			}
			else if (DockPane.DockPanel.ShowDocumentIcon && rectTab.Contains(rectIcon))
				g.DrawIcon(tab.Content.DockHandler.Icon, rectIcon);
		}

		private void DrawCloseButtonOnTab(Graphics g, Rectangle rectIcon)
		{
			//if mouse is in clicked state, simulate button press using a drawing offset
			if (drawCloseButton)
				rectIcon.Offset(1, 1);

			//draw the icon
			Rectangle r = rectIcon;
			r.Inflate(-2, -2);
			g.DrawImage(ImageButtonClose, r);

			//if mouse is over the close button, highligth it
			if (drawCloseButtonActive)
				g.DrawRectangle(Pens.Blue, r);
			else
				g.DrawRectangle(Pens.Green, r);
		}

		private static Rectangle CalculateIconRect(Rectangle tabRect)
		{
			return new Rectangle(
							tabRect.X + DocumentIconGapLeft,
							tabRect.Y + tabRect.Height - 1 - DocumentIconGapBottom - DocumentIconHeight,
							DocumentIconWidth, DocumentIconHeight);
		}

		private Rectangle CalculateCloseButtonRect(Rectangle tabRect)
		{
			return DrawHelper.RtlTransform(this, CalculateIconRect(tabRect));
			/*return  DrawHelper.RtlTransform(this, new Rectangle(
							tabRect.Right - DocumentIconGapRight - DocumentIconWidth,
							tabRect.Y + tabRect.Height - 1 - DocumentIconGapBottom - DocumentIconHeight,
							DocumentIconWidth, DocumentIconHeight));*/
		}
		private Rectangle TabStripRectangle
		{
			get
			{
				if (Appearance == DockPane.AppearanceStyle.Document)
					return TabStripRectangle_Document;
				else
					return TabStripRectangle_ToolWindow;
			}
		}

		private Rectangle TabStripRectangle_ToolWindow
		{
			get
			{
				Rectangle rect = ClientRectangle;
				return new Rectangle(rect.X, rect.Top + ToolWindowStripGapTop, rect.Width, rect.Height - ToolWindowStripGapTop - ToolWindowStripGapBottom);
			}
		}

		private Rectangle TabStripRectangle_Document
		{
			get
			{
				Rectangle rect = ClientRectangle;
				return new Rectangle(rect.X, rect.Top + DocumentStripGapTop, rect.Width, rect.Height - DocumentStripGapTop - ToolWindowStripGapBottom);
			}
		}

		private Rectangle TabsRectangle
		{
			get
			{
				if (Appearance == DockPane.AppearanceStyle.ToolWindow)
					return TabStripRectangle;

				Rectangle rectWindow = TabStripRectangle;
				int x = rectWindow.X;
				int y = rectWindow.Y;
				int width = rectWindow.Width;
				int height = rectWindow.Height;

				x += DocumentTabGapLeft;
				width -= DocumentTabGapLeft +
						DocumentTabGapRight +
						DocumentButtonGapRight +
						ButtonClose.Width +
						ButtonWindowList.Width +
						2 * DocumentButtonGapBetween;

				return new Rectangle(x, y, width, height);
			}
		}

		private ContextMenuStrip m_selectMenu;
		public override ContextMenuStrip SelectMenu
		{
			get { return m_selectMenu; }
		}

		private void WindowList_Click(object sender, EventArgs e)
		{
			int x = 0;
			int y = ButtonWindowList.Location.Y + ButtonWindowList.Height;

			SelectMenu.Items.Clear();

			if (this.DockPane.DockPanel.OnlyTabsInDropdownMenu)
			{
				foreach (TabVS2005 tab in Tabs)
				{
					IDockContent content = tab.Content;
					ToolStripItem item = SelectMenu.Items.Add(content.DockHandler.TabText, content.DockHandler.Icon.ToBitmap());
					item.Tag = tab.Content;
					item.Click += new EventHandler(ContextMenuItem_Click);
				}
			}
			else
			{
				foreach (IDockContent content in this.DockPane.Contents)
				{
					ToolStripItem item = SelectMenu.Items.Add(content.DockHandler.TabText, content.DockHandler.Icon.ToBitmap());
					item.Tag = content;
					item.Click += new EventHandler(ContextMenuItem_Click);
				}
			}
			SelectMenu.Show(ButtonWindowList, x, y);
		}

		private void ContextMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem item = sender as ToolStripMenuItem;
			if (item != null)
			{
				IDockContent content = (IDockContent)item.Tag;
				content.DockHandler.Activate();
			}
		}

		private void SetInertButtons()
		{
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
			{
				if (m_buttonClose != null)
					m_buttonClose.Left = -m_buttonClose.Width;

				if (m_buttonWindowList != null)
					m_buttonWindowList.Left = -m_buttonWindowList.Width;
			}
			else
			{
				bool showCloseButton = DockPane.ActiveContent == null ? true : DockPane.ActiveContent.DockHandler.CloseButton;
				ButtonClose.Enabled = showCloseButton;
				ButtonClose.Visible = DockPane.ActiveContent == null ? true : DockPane.ActiveContent.DockHandler.CloseButtonVisible;
				ButtonClose.RefreshChanges();
				ButtonWindowList.RefreshChanges();
			}
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (Appearance != DockPane.AppearanceStyle.Document)
			{
				base.OnLayout(levent);
				return;
			}

			Rectangle rectTabStrip = TabStripRectangle;

			// Set position and size of the buttons
			int buttonWidth = ButtonClose.Image.Width;
			int buttonHeight = ButtonClose.Image.Height;
			int height = rectTabStrip.Height - DocumentButtonGapTop - DocumentButtonGapBottom;
			if (buttonHeight < height)
			{
				buttonWidth = buttonWidth * (height / buttonHeight);
				buttonHeight = height;
			}
			Size buttonSize = new Size(buttonWidth, buttonHeight);

			int x = rectTabStrip.X + rectTabStrip.Width - DocumentTabGapLeft
				- DocumentButtonGapRight - buttonWidth;
			int y = rectTabStrip.Y + DocumentButtonGapTop;
			Point point = new Point(x, y);
			ButtonClose.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));

			// If the close button is not visible draw the window list button overtop.
			// Otherwise it is drawn to the left of the close button.
			if (ButtonClose.Visible)
				point.Offset(-(DocumentButtonGapBetween + buttonWidth), 0);

			ButtonWindowList.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));

			OnRefreshChanges();

			base.OnLayout(levent);
		}

		private void Close_Click(object sender, EventArgs e)
		{
			DockPane.CloseActiveContent();
		}

		protected internal override int HitTest(Point ptMouse)
		{
			Rectangle rectTabStrip = TabsRectangle;
			if (!TabsRectangle.Contains(ptMouse))
				return -1;

			foreach (Tab tab in Tabs)
			{
				GraphicsPath path = GetTabOutline(tab, true, false);
				if (path.IsVisible(ptMouse))
					return Tabs.IndexOf(tab);
			}
			return -1;
		}

		protected override void OnMouseHover(EventArgs e)
		{
			int index = HitTest(PointToClient(Control.MousePosition));
			string toolTip = string.Empty;

			base.OnMouseHover(e);

			if (drawCloseButtonActive)
				toolTip = ToolTipClose;
			else if (index != -1)
			{
				TabVS2005 tab = Tabs[index] as TabVS2005;
				if (!String.IsNullOrEmpty(tab.Content.DockHandler.ToolTipText))
					toolTip = tab.Content.DockHandler.ToolTipText;
				else
					toolTip = tab.Content.DockHandler.TabText;
			}

			// requires further tracking of mouse hover behavior,
			ResetMouseEventArgs();
		}

		protected override void OnRightToLeftChanged(EventArgs e)
		{
			base.OnRightToLeftChanged(e);
			PerformLayout();
		}

		//-----------------------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (DockPane.DockPanel.ShowCloseButtonOnTab)
			{
				//search fro tab under mouse
				for (int i = 0; i < Tabs.Count; i++)
				{
					Rectangle r = GetTabRectangle(i);
					if (r.Contains(e.Location))
					{
						if ((Tabs[i].Content).DockHandler.DockState != DockState.Document)
							break;
						//mouse is in tab area
						Rectangle buttonRect = CalculateCloseButtonRect(r);

						//mouse is in the close button area
						drawCloseButton = buttonRect.Contains(e.Location);
						Invalidate(r);

						break;
					}
				}
			}

			//if mouse is over the close button, don't do other actions associated to mouse
			if (!drawCloseButton)
				base.OnMouseDown(e);
			else
				Update();//needed, because not calling base method dows not paint the control
		}
		//-----------------------------------------------------------------------------------------
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			//if mouse down has not occurred in the close button, does nothing
			if (!drawCloseButton)
				return;

			for (int i = 0; i < Tabs.Count; i++)
			{
				Rectangle r = GetTabRectangle(i);
				if (r.Contains(e.Location))
				{
					Rectangle buttonRect = CalculateCloseButtonRect(r);

					//mouse up is in the close button area
					if (buttonRect.Contains(e.Location) && activeMouseTab == Tabs[i])
					{
						Tab tab = activeMouseTab; //keep my tab aside, then reset all mouse information to avoid
						//operations on a closed tab
						ResetCloseButtonInfos();
						DockPane.CloseContent(tab.Content);
					}
					break;
				}
			}
		}


		//-----------------------------------------------------------------------------------------
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			CalculateActiveTabInfo(e.Location);
		}

		//-----------------------------------------------------------------------------------------
		private void CalculateActiveTabInfo(Point point)
		{
			Tab hitTab = null;
			for (int i = 0; i < Tabs.Count; i++)
			{
				Rectangle r = GetTabRectangle(i);
				if (r.Contains(point))
				{

					//mouse is moving in the tab area
					hitTab = Tabs[i];
					if (hitTab.ContentForm != null)
					{
						this.ContextMenuStrip = hitTab.ContentForm.ContextMenuStrip;
						this.ContextMenu = hitTab.ContentForm.ContextMenu;
					}
					//if the tab is different from the previous, the previous has to be invalidated
					if (activeMouseTab != null && activeMouseTab != hitTab)
					{
						int index = Tabs.IndexOf(activeMouseTab);
						if (index >= 0)
							Invalidate(GetTabRectangle(index));
					}

					//change the active tab
					activeMouseTab = hitTab;

					Rectangle buttonRect = CalculateCloseButtonRect(r);
					drawCloseButtonActive = buttonRect.Contains(point);
					if (!drawCloseButtonActive)
						drawCloseButton = false;

					//invalidate the tab area
					Invalidate(r);
					break;
				}
			}

			//if mouse is not in a tab area, reset all the mouse information
			if (hitTab == null)
				ResetCloseButtonInfos();
		}
		//---------------------------------------------------------------------
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			if (DockPane!= null && DockPane.DockPanel != null && DockPane.DockPanel.ShowCloseButtonOnTab)
			{
				ResetCloseButtonInfos();
				Invalidate();
			}
		}

		//---------------------------------------------------------------------
		private void ResetCloseButtonInfos()
		{
			drawCloseButton = false;
			drawCloseButtonActive = false;
			activeMouseTab = null;
		}
	}
}
