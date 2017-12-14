using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	//Questa progress bar essendo usata da molti utilizzatori che non hanno altre reference (splash, clickonce....), la metto qui per comodità
    public partial class SmoothProgressBar : System.Windows.Forms.ProgressBar
    {
        private Color gradientStartColor = Color.LemonChiffon;
        private Color gradientEndColor = Color.DarkOrange;
        private const int margin = 2;

        //--------------------------------------------------------------------------------------------------------------------------------
        public SmoothProgressBar()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            this.BackColor = Color.Transparent;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public Color GradientStartColor
        {
            get { return gradientStartColor; }
            set
            {
                gradientStartColor = value;
                
                Refresh();
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public Color GradientEndColor
        {
            get { return gradientEndColor; }
            set
            {
                gradientEndColor = value;

                Refresh();
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        // new Value to force an invalidation 
        //--------------------------------------------------------------------------------------------------------------------------------
        public new int Value
        {
            get { return base.Value; }
            set
            {
                base.Value = value;

                Refresh();
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        protected override void OnPaint(PaintEventArgs pe)
        {
            // Calling the base class OnPaint
            base.OnPaint(pe);

            Graphics g = pe.Graphics;

            g.SmoothingMode = SmoothingMode.HighSpeed;

            if (Application.RenderWithVisualStyles)
                ProgressBarRenderer.DrawHorizontalBar(g, this.ClientRectangle);

            int valueLen = this.Value - this.Minimum;
            valueLen *= (this.Width - 2 * margin);

			int barLen = this.Maximum - this.Minimum;
			if (barLen != 0)
				valueLen /= barLen;
			else
				valueLen = -1;

            if (valueLen > 0)
            {
                Rectangle rect = new Rectangle(this.ClientRectangle.X + margin, this.ClientRectangle.Y + margin, valueLen, this.ClientRectangle.Height - 2 * margin);
                LinearGradientBrush progressBrush = new LinearGradientBrush(rect, gradientStartColor, gradientEndColor, LinearGradientMode.Vertical);
                progressBrush.SetBlendTriangularShape(0.5f);
                
                g.FillRectangle(progressBrush, rect);

                progressBrush.Dispose();
            }
        }

    }
}
