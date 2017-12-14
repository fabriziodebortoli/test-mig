using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
    //===========================================================================
    public class TBTabManager : System.Windows.Forms.TabControl
    {
        #region Private DataMembers

        [DllImport("User32", CallingConvention = CallingConvention.Cdecl)]
        private static extern int RealGetWindowClass(IntPtr hwnd, System.Text.StringBuilder pszType, int cchType);

        [DllImport("user32")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private const int WM_Create = 0x1;
        private const int WM_PARENTNOTIFY = 0x210;
        private const int WM_HSCROLL = 0x114;

        private const int IMAGEXOFFSET = 5;

        private int scrollPos = 0;
        private StringFormat stringFormat = null;

        private Rectangle lastTabRect = new Rectangle();
        private SolidBrush tabPagesBkgndBrush = null;

        private NativeUpDown UpDown = null;
        private TabScrollCloseCtrl scrollCloseCtrl = new TabScrollCloseCtrl();

        #endregion

        //---------------------------------------------------------------------
        public TBTabManager()
            : base()
        {
            scrollCloseCtrl.ScrollLeft += new EventHandler(ScrollCloseCtrl_ScrollLeft);
            scrollCloseCtrl.ScrollRight += new EventHandler(ScrollCloseCtrl_ScrollRight);
            scrollCloseCtrl.TabClose += new EventHandler(ScrollCloseCtrl_TabClose);

            this.DrawMode = TabDrawMode.OwnerDrawFixed;
          
            stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces;
            stringFormat.Trimming = StringTrimming.EllipsisCharacter;

            tabPagesBkgndBrush = new SolidBrush(backColor);
        }

        //---------------------------------------------------------------------
        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            SetStyle
            (
                //ControlStyles.OptimizedDoubleBuffer |
                //ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                //ControlStyles.Opaque |
                ControlStyles.SupportsTransparentBackColor, true);
            UpdateStyles();

            base.BackColor = Color.Transparent;
        }

        //---------------------------------------------------------------------
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT
                return cp;
            }
        }

        #region Public Properties

        [Browsable(true), Description("Get or Set Visibility of CloseButton.")]
        //---------------------------------------------------------------------
        public bool CloseButtonVisible { get { return scrollCloseCtrl.CloseVisible; } set { scrollCloseCtrl.CloseVisible = value; } }

        private Color backColor = Color.Lavender;
        //---------------------------------------------------------------------
        [Browsable(true)]
        public override Color BackColor
        {
            get
            {
                return backColor;
            }
            set 
            {
                backColor = value;

                if (tabPagesBkgndBrush != null)
                {
                    tabPagesBkgndBrush.Dispose();
                    tabPagesBkgndBrush = null;
                }

                if (!backColor.IsEmpty && backColor != Color.Transparent)
                    tabPagesBkgndBrush = new SolidBrush(backColor);

                Refresh();
            }
        }

        #endregion

        #region Protected Methods

        //---------------------------------------------------------------------
        protected void InvalidateBackground()
        {
            if (this.Parent != null)
            {
                Rectangle invalidArea = this.ClientRectangle;
                if (this.TabCount > 0)
                {
                    invalidArea.Height = this.GetTabRect(0).Height;
                }
                this.Parent.Invalidate(this.Parent.RectangleToClient(this.RectangleToScreen(invalidArea)), true);

                this.Invalidate(invalidArea, true);
            }
        }

        //---------------------------------------------------------------------
        protected virtual void OnLeftScrolled()
        { 
        }

        //---------------------------------------------------------------------
        protected virtual void OnRightScrolled()
        {
        }

        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (UpDown != null)
                    UpDown.ReleaseHandle();

                if (tabPagesBkgndBrush != null)
                    tabPagesBkgndBrush.Dispose();

            }
            base.Dispose(disposing);
        }

        //---------------------------------------------------------------------
        protected override void OnHandleCreated(System.EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!this.Multiline)
                SetParent(scrollCloseCtrl.Handle, this.Handle);

            scrollCloseCtrl.Height = this.ItemSize.Height - 2;
            scrollCloseCtrl.Width = this.scrollCloseCtrl.Height * 3;
            
            OnResize(EventArgs.Empty);
        }

        //---------------------------------------------------------------------
        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);

            InvalidateBackground();

            if (this.Multiline)
                return;

            if (this.Alignment == TabAlignment.Top)
                scrollCloseCtrl.Location = new Point(this.Width - scrollCloseCtrl.Width, 2);
            else
                scrollCloseCtrl.Location = new Point(this.Width - scrollCloseCtrl.Width, this.Height - scrollCloseCtrl.Height - 2);

        }
        
        #endregion

        #region Private Methods

        //---------------------------------------------------------------------
        private int ScrollPosition
        {
            get
            {
                int multiplier = -1;
                Rectangle tabRect;
                do
                {
                    tabRect = GetTabRect(multiplier + 1);
                    multiplier++;
                }
                while (tabRect.Left < 0 && multiplier < this.TabCount);
                return multiplier;
            }
        }

        //---------------------------------------------------------------------
        private IntPtr MakeParam(int loWord, int hiWord)
        {
            return (IntPtr)(loWord | hiWord);
        }

        //---------------------------------------------------------------------
        private void ScrollCloseCtrl_ScrollLeft(Object sender, System.EventArgs e)
        {
            if (this.TabCount == 0)
                return;


            scrollPos = Math.Max(0, (ScrollPosition - 1) * 0x10000);
            
            SendMessage(this.Handle, WM_HSCROLL, MakeParam(scrollPos, 0x4), IntPtr.Zero);
            //SendMessage(this.Handle, WM_HSCROLL, MakeParam(scrollPos, 0x8), IntPtr.Zero);
            
            InvalidateBackground();

            UpdateScrollButtons();

            OnLeftScrolled();
        }

        //---------------------------------------------------------------------
        private void ScrollCloseCtrl_ScrollRight(Object sender, System.EventArgs e)
        {
            if (this.TabCount == 0)
                return;
            if (GetTabRect(this.TabCount - 1).Right <= scrollCloseCtrl.Left)
                return;
           
            
            scrollPos = Math.Max(0, (ScrollPosition + 1) * 0x10000);
            SendMessage(this.Handle, WM_HSCROLL, MakeParam(scrollPos, 0x4), IntPtr.Zero);
            //SendMessage(this.Handle, WM_HSCROLL, MakeParam(scrollPos, 0x8), IntPtr.Zero);

            InvalidateBackground();

            UpdateScrollButtons();

            OnRightScrolled();
        }

        //---------------------------------------------------------------------
        private void ScrollCloseCtrl_TabClose(Object sender, System.EventArgs e)
        {
            if (this.SelectedTab != null)
            {
                TBTabPage tbPage = this.SelectedTab as TBTabPage;
                if (tbPage != null)
                {
                    if (!tbPage.IsDisposed)
                        tbPage.Close();

                    this.TabPages.Remove(tbPage);
                    
                    UpdateScrollButtons();

                    InvalidateBackground();
                }
            }
        }

        #endregion

        #region Public Methods

        //---------------------------------------------------------------------
        public virtual void CloseAllTabs()
        {
            this.TabPages.Clear();
        }

        #endregion

        #region Interop

        [StructLayout(LayoutKind.Sequential)]
        private struct NMHDR
        {
            public IntPtr HWND;
            public uint idFrom;
            public int code;
            public override String ToString()
            {
                return String.Format("Hwnd: {0}, ControlID: {1}, Code: {2}", HWND, idFrom, code);
            }
        }

        private const int TCN_FIRST = 0 - 550;
        private const int TCN_SELCHANGING = (TCN_FIRST - 2);

        private const int WM_USER = 0x400;
        private const int WM_NOTIFY = 0x4E;
        private const int WM_REFLECT = WM_USER + 0x1C00;

        #endregion

 
        //---------------------------------------------------------------------
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            UpdateScrollButtons();
        }

        //---------------------------------------------------------------------
        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        //---------------------------------------------------------------------
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Invalidate();
        }

        //---------------------------------------------------------------------
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
			try
			{       
                //Draw the Tabs
				DrawTabs(e);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
			}
        }

        //---------------------------------------------------------------------
        private void UpdateScrollButtons()
        {
            this.scrollCloseCtrl.LeftScrollEnabled = this.TabCount > 0 && (GetTabRect(0).Left <= 0);
            this.scrollCloseCtrl.RightScrollEnabled = this.TabCount > 0 && (GetTabRect(this.TabCount - 1).Right > this.scrollCloseCtrl.Left);
            this.scrollCloseCtrl.CloseEnabled = (this.TabCount > 0 && this.SelectedTab != null);
        }
       
        //---------------------------------------------------------------------
        protected virtual bool CanDrawItem(int index)
        {
            if (index < 0 || index > TabPages.Count - 1)
                return false;
            
            Rectangle r = GetTabRect(index);
            return scrollCloseCtrl.Left > r.Right;
        }

        //---------------------------------------------------------------------
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (!CanDrawItem(e.Index))
                return;

            TBTabPage tp = TabPages[e.Index] as TBTabPage;
            
            Rectangle r = e.Bounds;

            GraphicsPath tabPath = new GraphicsPath();

            e.Graphics.CompositingQuality = CompositingQuality.HighQuality | CompositingQuality.GammaCorrected;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            LinearGradientBrush activeTabBackBrush =
                e.State == DrawItemState.Selected
                ? new LinearGradientBrush(r, Color.White, Color.FromArgb(188, 211, 242), LinearGradientMode.Vertical)
                : new LinearGradientBrush(r, Color.White, Color.FromArgb(141, 178, 230), LinearGradientMode.Vertical);
			
			int angleRadius = 6;
			int angleDiameter = 2 * angleRadius;

            if (this.Alignment == TabAlignment.Top)
            {
                tabPath.AddLine(new Point(r.X, r.Bottom), new Point(r.X, r.Y + angleRadius));
                tabPath.AddArc(r.X, r.Y, angleDiameter, angleDiameter, 180, 90);
                tabPath.AddLine(new Point(r.X + angleRadius, r.Y), new Point(r.Right - angleRadius, r.Y));
                tabPath.AddArc(r.Right - angleDiameter, r.Y, angleDiameter, angleDiameter, 270, 90);
                tabPath.AddLine(new Point(r.X + r.Width, r.Y + angleRadius), new Point(r.Right, r.Bottom));
                tabPath.AddLine(new Point(r.Right, r.Bottom), new Point(r.X, r.Bottom));
            }
            else if (this.Alignment == TabAlignment.Bottom)
            {
                tabPath.AddLine(new Point(r.Right, r.Y), new Point(r.X + r.Width, r.Bottom - angleRadius));
                tabPath.AddArc(r.Right - angleDiameter, r.Bottom - angleDiameter, angleDiameter, angleDiameter, 0, 90);
                tabPath.AddLine(new Point(r.Right - angleRadius, r.Bottom), new Point(r.X + angleRadius, r.Bottom));
                tabPath.AddArc(r.X, r.Bottom - angleDiameter, angleDiameter, angleDiameter, 90, 90);
                tabPath.AddLine(new Point(r.X, r.Bottom - angleRadius), new Point(r.X, r.Y));
                tabPath.AddLine(new Point(r.X, r.Y), new Point(r.Right, r.Y));
            }

			e.Graphics.FillPath(activeTabBackBrush, tabPath);
			e.Graphics.DrawPath(Pens.RoyalBlue, tabPath);

            tabPath.Dispose();

			if (e.State == DrawItemState.Selected)
			{
				Pen activeTabPen = new Pen(activeTabBackBrush);
				
				e.Graphics.DrawLine(activeTabPen, new Point(r.Left, r.Bottom), new Point(r.Right, r.Bottom));
				
                activeTabPen.Dispose();
			}

            activeTabBackBrush.Dispose();

			//Rotazione Tab per alignment
			if (Alignment == TabAlignment.Left || Alignment == TabAlignment.Right)
			{
				float rotAngle = 90;
				if (Alignment == TabAlignment.Left)
                    rotAngle = 270;
				PointF cp = new PointF(r.Left + (r.Width >> 1), r.Top + (r.Height >> 1));
				e.Graphics.TranslateTransform(cp.X, cp.Y);
                e.Graphics.RotateTransform(rotAngle);
				r = new Rectangle(-(r.Height >> 1), -(r.Width >> 1), r.Height, r.Width);
			}

			//Disegna il Testo nella TabStrip
			DrawTabText(e.Graphics, tp, e.State, e.Index);

			e.Graphics.ResetTransform();

        }

        //---------------------------------------------------------------------
        private void DrawTabs(PaintEventArgs e)
        {
            if (this.TabCount == 0)
                return;

            if (this.SelectedTab != null)
            {
                Rectangle borderRect = this.SelectedTab.Bounds;
                //Disegna il bordo intorno alla TBTabPage
                borderRect.Inflate(3, 3);

                if (tabPagesBkgndBrush != null)
                    e.Graphics.FillRectangle(tabPagesBkgndBrush, borderRect);

                ControlPaint.DrawBorder3D(e.Graphics, borderRect);
                //ControlPaint.DrawBorder(e.Graphics, borderRect, Color.RoyalBlue, ButtonBorderStyle.Outset);
            }

            Rectangle tabsRect = Rectangle.Empty;
            for (int index = this.TabCount - 1; index >= 0; index--)
            {
                Rectangle tempRect = GetTabRect(index);
                if (index == this.TabCount - 1)
                    lastTabRect = tabsRect = tempRect;
                else
                {
                    tabsRect.Offset(-tempRect.Width, 0);
                    tabsRect.Width += tempRect.Width;
                }
                OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, tempRect, index, index == SelectedIndex ? DrawItemState.Selected : DrawItemState.None));
            }

            if (scrollCloseCtrl.Location.X < tabsRect.Width)
                this.scrollCloseCtrl.RightScrollCloseButton.Visible = this.scrollCloseCtrl.LeftScrollCloseButton.Visible = true;
            else
                this.scrollCloseCtrl.RightScrollCloseButton.Visible = this.scrollCloseCtrl.LeftScrollCloseButton.Visible = false;
        }

        //---------------------------------------------------------------------
        public virtual void DrawTabText(Graphics g, TBTabPage tp, DrawItemState state, int aIndex)
        {
            if (tp == null)
                return;

            SizeF measure = new SizeF(0, 0);

            Rectangle textRect = GetTabRect(aIndex);

            Font drawFont = (state == DrawItemState.Selected) 
				? new Font(this.Font, FontStyle.Italic)
				: new Font(this.Font, FontStyle.Regular);

			if (tp.Enabled)
			{
				using (Brush b = new SolidBrush(tp.TextColor))
				{
					g.DrawString(tp.Text, drawFont, b, (RectangleF)textRect, stringFormat);
				}
			}
			else
			{
				ControlPaint.DrawStringDisabled(g, tp.Text, drawFont, tp.TextColor, (RectangleF)textRect, stringFormat);
			}

            drawFont.Dispose();
        }

        //---------------------------------------------------------------------
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {

            if (m.Msg == WM_PARENTNOTIFY)
            {
                if ((ushort)(m.WParam.ToInt32() & 0xFFFF) == WM_Create)
                {
                    System.Text.StringBuilder WindowName = new System.Text.StringBuilder(16);
                    RealGetWindowClass(m.LParam, WindowName, 16);
                    if (WindowName.ToString() == "msctls_updown32")
                    {
                        if (UpDown != null)
                            UpDown.ReleaseHandle();
                        UpDown = new NativeUpDown();
                        UpDown.AssignHandle(m.LParam);
                    }
                }
            }

            if (m.Msg == (WM_REFLECT + WM_NOTIFY))
            {
                NMHDR hdr = (NMHDR)(Marshal.PtrToStructure(m.LParam, typeof(NMHDR)));
                if (hdr.code == TCN_SELCHANGING)
                {
                    TBTabPage tp = TestTab(PointToClient(Cursor.Position));
                    if (tp != null)
                    {
                        TabPageChangeEventArgs e = new TabPageChangeEventArgs(SelectedTab as TBTabPage, tp);
                        if (e.Cancel || tp.Enabled == false)
                        {
                            m.Result = new IntPtr(1);
                            return;
                        }
                    }
                }
            
            }

            base.WndProc(ref m);
        }

        //---------------------------------------------------------------------
        private TBTabPage TestTab(Point pt)
        {
            for (int index = 0; index <= TabCount - 1; index++)
            {
                if (GetTabRect(index).Contains(pt.X, pt.Y))
                    return TabPages[index] as TBTabPage;
            }
            return null;
        }
    }


    //===========================================================================
    public class TabPageChangeEventArgs : EventArgs
    {
        private TBTabPage _Selected = null;
        private TBTabPage _PreSelected = null;
        public bool Cancel = false;

        //---------------------------------------------------------------------
        public TBTabPage CurrentTab
        {
            get
            {
                return _Selected;
            }
        }

        //---------------------------------------------------------------------
        public TBTabPage NextTab
        {
            get
            {
                return _PreSelected;
            }
        }

        //---------------------------------------------------------------------
        public TabPageChangeEventArgs(TBTabPage CurrentTab, TBTabPage NextTab)
        {
            _Selected = CurrentTab;
            _PreSelected = NextTab;
        }


    }


    //===========================================================================
    public class NativeUpDown : NativeWindow
    {

        public NativeUpDown() : base() { }

        private const int WM_DESTROY = 0x2;
        private const int WM_NCDESTROY = 0x82;
        private const int WM_WINDOWPOSCHANGING = 0x46;

        private Rectangle _bounds;

        //---------------------------------------------------------------------
        internal Rectangle Bounds
        {
            get { return _bounds; }
        }

        //---------------------------------------------------------------------
        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPOS
        {
            public IntPtr hwnd, hwndInsertAfter;
            public int x, y, cx, cy, flags;
        }

        //---------------------------------------------------------------------
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_DESTROY || m.Msg == WM_NCDESTROY)
                this.ReleaseHandle();
            else if (m.Msg == WM_WINDOWPOSCHANGING)
            {
                WINDOWPOS wp = (WINDOWPOS)(m.GetLParam(typeof(WINDOWPOS)));
                wp.x += wp.cx;
                Marshal.StructureToPtr(wp, m.LParam, true);
                _bounds = new Rectangle(wp.x, wp.y, wp.cx, wp.cy);
            }
            base.WndProc(ref m);
        }



    }

}
