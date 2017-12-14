using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	internal partial class CollapsiblePanelLinkLabel : UserControl
    {
        public event EventHandler DesignSelectionChanged;
        public event LinkLabelLinkClickedEventHandler LinkClicked;
        public EventHandler InitDrag;
		
        private bool designSelected;
        private bool designSupport;

		private MenuGroup menuGroup;

        private System.Windows.Forms.Timer mouseDownTimer;

        //---------------------------------------------------------------------------
        public CollapsiblePanelLinkLabel()
        {
            InitializeComponent();

            this.LinkLabel.KeyUp += new KeyEventHandler(ChildControl_KeyUp);
            this.PictureBox.KeyUp += new KeyEventHandler(ChildControl_KeyUp);
        }

        //---------------------------------------------------------------------------
        [Localizable(true)]
        public new string Text
        {
            get { return this.LinkLabel.Text; }
            set 
            {
                this.LinkLabel.Text = value;
                AdjustControls();
            }
        }

        //---------------------------------------------------------------------------
        public Image Image
        {
            get { return this.PictureBox.Image; }
            set 
            {
                this.PictureBox.Image = value;
                AdjustControls();
            }
        }

		//---------------------------------------------------------------------------
		internal MenuGroup MenuGroup 
		{
			get { return menuGroup; } 
			set 
			{
				menuGroup = value;
				
				LinkLabel.Font =
					menuGroup.CanBeCustomized ?
					StaticResources.CustomizableItemFont :
					StaticResources.NonCustomizableItemFont;

				LinkLabel.LinkColor =
					menuGroup.CanBeCustomized ?
					StaticResources.CustomizableItemColor :
					StaticResources.NonCustomizableItemColor;
			} 
		}


		//---------------------------------------------------------------------------
        public int PreferredHeight
        {
            get { return Math.Max(this.PictureBox.Height, this.LinkLabel.PreferredHeight); }
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

				designSelected = value;

				Refresh();

				if (!designSelected)
					return;

				Focus();
				if (DesignSelectionChanged != null)
					DesignSelectionChanged(this, EventArgs.Empty);
			}
		}

        //---------------------------------------------------------------------------
        private void AdjustControls()
        {
            this.PictureBox.Location = new Point(0, 0);
            if (this.PictureBox.Image == null)
                this.PictureBox.Size = new Size(0, 0);

			this.LinkLabel.Location = new Point(this.PictureBox.Right, (this.PreferredHeight - this.LinkLabel.Height) / 2);
            this.LinkLabel.Width = Math.Max(0, this.Width - this.PictureBox.Width);
        }

        //---------------------------------------------------------------------------
        private Rectangle GetDesignSelectionRect(System.Drawing.Graphics graphics)
        {
            bool disposeGraphics = false;
            if (graphics == null)
            {
                graphics = this.CreateGraphics();
                disposeGraphics = true;
            }

            StringFormat format = new StringFormat();
            format.Trimming = StringTrimming.EllipsisCharacter;
            format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;
            format.LineAlignment = StringAlignment.Center;

            System.Drawing.SizeF textSize = graphics.MeasureString(this.Text, this.LinkLabel.Font, this.LinkLabel.Width - 4, format);

            if (disposeGraphics)
                graphics.Dispose();

            return new Rectangle(this.LinkLabel.Location, new Size((int)Math.Ceiling(textSize.Width), this.LinkLabel.Height));
        }

        //---------------------------------------------------------------------------
        private void StartMouseDownTimer()
        {
            if (components != null && mouseDownTimer == null)
            {
                mouseDownTimer = new System.Windows.Forms.Timer(components);
                mouseDownTimer.Interval = 300;
                mouseDownTimer.Tick += new EventHandler(MouseDownTimer_Tick);
            }
            mouseDownTimer.Start();
        }

        //---------------------------------------------------------------------------
        private void StopMouseDownTimer()
        {
            if (mouseDownTimer != null)
            {
				mouseDownTimer.Tick -= new EventHandler(MouseDownTimer_Tick);
                mouseDownTimer.Stop();
                mouseDownTimer.Dispose();
                mouseDownTimer = null;
            }
        }

        //---------------------------------------------------------------------------
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            AdjustControls();
        }

        //---------------------------------------------------------------------------
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            this.LinkLabel.Focus();
        }

        //---------------------------------------------------------------------------
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            
            AdjustControls();
        }

        //---------------------------------------------------------------------------
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!this.IsDesignSelected)
            {
                Rectangle designSelectionRect = GetDesignSelectionRect(null);
                if (designSelectionRect.Contains(e.Location))
                    this.IsDesignSelected = true;
            }
        }

        //---------------------------------------------------------------------------
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

			if (!this.IsDesignSelected)
				return;

            Rectangle insideRect = GetDesignSelectionRect(e.Graphics);
			if (insideRect.IsEmpty)
				return;

			Rectangle outsideRect = insideRect;
			ControlPaint.DrawBorder(e.Graphics, outsideRect, Color.White, ButtonBorderStyle.Dotted);
			insideRect.Inflate(-4, -4);
			ControlPaint.DrawSelectionFrame(e.Graphics, true, outsideRect, insideRect, Color.Transparent);
        }

        //---------------------------------------------------------------------------
        private void ChildControl_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }
        
        //---------------------------------------------------------------------------
        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (!this.IsDesignSelected)
                this.IsDesignSelected = true;
        }

        //---------------------------------------------------------------------------
        private void LinkLabel_MouseDown(object sender, MouseEventArgs e)
        {
			if (e.Button == MouseButtons.Left)
                StartMouseDownTimer();
        }

        //---------------------------------------------------------------------------
        private void LinkLabel_MouseLeave(object sender, EventArgs e)
        {
            StopMouseDownTimer();
        }

        //---------------------------------------------------------------------------
        private void LinkLabel_MouseUp(object sender, MouseEventArgs e)
        {
            StopMouseDownTimer();

			if (DesignSelectionChanged != null)
				DesignSelectionChanged(this, EventArgs.Empty);
        }

	    //---------------------------------------------------------------------------
        private void LinkLabel_TextChanged(object sender, EventArgs e)
        {
            this.LinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, this.Text.Length);
        }
        
        //---------------------------------------------------------------------------
        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StopMouseDownTimer();

			if (!this.IsDesignSelected)
			    this.IsDesignSelected = true;
            
            if (LinkClicked != null)
                LinkClicked(this, e);
        }

        //---------------------------------------------------------------------------
        private void MouseDownTimer_Tick(object sender, System.EventArgs e)
        {
            if (Control.MouseButtons == MouseButtons.Left && InitDrag != null)
                InitDrag(this, EventArgs.Empty);

			StopMouseDownTimer();
        }

		//---------------------------------------------------------------------------
		private void PictureBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (DesignSelectionChanged != null)
				DesignSelectionChanged(this, EventArgs.Empty);
		}

		public bool IsCut { get; set; }
	}
}
