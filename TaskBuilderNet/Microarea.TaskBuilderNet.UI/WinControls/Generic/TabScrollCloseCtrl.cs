using System;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
    //===========================================================================
    public partial class TabScrollCloseCtrl : System.Windows.Forms.UserControl
    {
        #region Public Events

        public event EventHandler TabClose;
        public event EventHandler ScrollLeft;
        public event EventHandler ScrollRight;

        #endregion

        //---------------------------------------------------------------------
        public TabScrollCloseCtrl()
            : base()
        {
            InitializeComponent();
            AdjustButtonSizes();

            SetStyle
            (
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint |
            ControlStyles.Opaque |
            ControlStyles.SupportsTransparentBackColor, true
            );
            UpdateStyles();
            this.BackColor = Color.Transparent;
        }

        //---------------------------------------------------------------------
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        #region Private Methods

        //---------------------------------------------------------------------
        private void AdjustButtonSizes(bool performLayout)
        {
            int newButtonWidth = this.Width / 3;

            CloseButton.Size = new Size(newButtonWidth, this.Height);
            CloseButton.Location = new Point(2 * newButtonWidth, 0);

            int delta = CloseButton.Visible ? 0 : newButtonWidth;
            LeftScrollCloseButton.Size = new Size(newButtonWidth, this.Height);
            LeftScrollCloseButton.Location = new Point(delta, 0);
            
            RightScrollCloseButton.Size = new Size(newButtonWidth, this.Height);
            RightScrollCloseButton.Location = new Point(newButtonWidth + delta, 0);
            

            if (performLayout)
                PerformLayout();
        }
  
        //---------------------------------------------------------------------
        private void AdjustButtonSizes()
        {
            AdjustButtonSizes(true);
        }     

        //---------------------------------------------------------------------
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            AdjustButtonSizes(false);
        }

        //---------------------------------------------------------------------
        protected override void  OnResize(EventArgs e)
        {
 	         base.OnResize(e);

             AdjustButtonSizes();
        }

        //---------------------------------------------------------------------
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        
            AdjustButtonSizes();
        }

        //---------------------------------------------------------------------
        private void LeftScrollCloseCtrl_Click(Object sender, System.EventArgs e)
        {
            if (ScrollLeft != null)
                ScrollLeft(this, EventArgs.Empty);
        }

        //---------------------------------------------------------------------
        private void RightScrollCloseCtrl_Click(Object sender, System.EventArgs e)
        {
            if (ScrollRight != null)
                ScrollRight(this, EventArgs.Empty);
        }

        //---------------------------------------------------------------------
        private void CloseButton_Click(Object sender, System.EventArgs e)
        {
            if (TabClose != null)
                TabClose(this, EventArgs.Empty);
        }
        //---------------------------------------------------------------------
        #endregion

        #region Protected Methods


        #endregion

        public bool LeftScrollEnabled { get { return this.LeftScrollCloseButton.Enabled; } set { this.LeftScrollCloseButton.Enabled = value; } }
        public bool RightScrollEnabled { get { return this.RightScrollCloseButton.Enabled; } set { this.RightScrollCloseButton.Enabled = value; } }
        public bool CloseEnabled { get { return this.CloseButton.Enabled; } set { this.CloseButton.Enabled = value; } }
        public bool CloseVisible { get { return this.CloseButton.Visible; } set { this.CloseButton.Visible = value; } }
    }

}
