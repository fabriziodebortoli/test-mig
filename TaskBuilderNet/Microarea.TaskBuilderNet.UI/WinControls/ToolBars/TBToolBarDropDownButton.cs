using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	/// <summary>
	/// Summary description for TBToolBarDropDownButton.
	/// </summary>
    //==============================================================================
    internal partial class TBToolBarDropDownButton : System.Windows.Forms.Panel
	{
		#region Events
		
		public event System.EventHandler PopUp;
		
		#endregion

		private System.Drawing.Image arrowImage = null;
		private bool pressed = false;

		internal const int DropDownButtonDefaultWidth = 20;

        //---------------------------------------------------------------------------
        public TBToolBarDropDownButton()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			Stream arrowImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.ToolBars.Images.TBToolBarDropDownArrow.gif");
			if (arrowImageStream != null)
				arrowImage = Image.FromStream(arrowImageStream);
			
			this.Width = DropDownButtonDefaultWidth;
			this.Height = 32;
			//this.BackColor = Color.Transparent;
			this.ForeColor = Color.Navy;
			this.Visible = true;
//			this.FlatStyle = FlatStyle.Standard;
		}

        //---------------------------------------------------------------------------
        public GraphicsPath GetButtonBordersPath()
        {
            const int diameter = 4;
            int radius = diameter / 2;
            
            // Create a GraphicsPath with curved corners
            GraphicsPath buttonBordersPath = new GraphicsPath();
            
            buttonBordersPath.AddLine(radius, 0, this.Width - radius - 1, 0);
            buttonBordersPath.AddArc(this.Width - diameter - 1, 0, diameter, diameter, 270, 90);
            buttonBordersPath.AddLine(this.Width - 1, radius, this.Width - 1, this.Height - radius - 1);
            buttonBordersPath.AddArc(this.Width - diameter - 1, this.Height - diameter - 1, diameter, diameter, 0, 90);
            buttonBordersPath.AddLine(this.Width - radius - 1, this.Height - 1, radius, this.Height - 1);
            buttonBordersPath.AddArc(0, this.Height - diameter - 1, diameter, diameter, 90, 90);
            buttonBordersPath.AddLine(0, this.Height - radius - 1, 0, 0 + radius + 1);
            buttonBordersPath.AddArc(0, 0, diameter, diameter, 180, 90);

            return buttonBordersPath;
        }
		
		//---------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

            if (!this.Visible || e == null || e.Graphics == null)
                return;

            GraphicsPath buttonBordersPath = GetButtonBordersPath();

            if (pressed)
            {
                LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, Color.Cornsilk, Color.DarkOrange, LinearGradientMode.Vertical);

                e.Graphics.FillPath(brush, buttonBordersPath);

                brush.Dispose();

                e.Graphics.DrawPath(System.Drawing.Pens.Gold, buttonBordersPath);
            }
            else
                e.Graphics.DrawPath(System.Drawing.Pens.LightSteelBlue, buttonBordersPath);

            buttonBordersPath.Dispose();

            if (arrowImage != null)
                e.Graphics.DrawImage(arrowImage, 1, 1, arrowImage.Width, arrowImage.Height);

		}

		//---------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			this.Pressed = true;

            base.OnMouseDown(e);
            
            if (PopUp != null)
				PopUp(this, System.EventArgs.Empty);
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp (e);

			this.Pressed = false;
		}
		//---------------------------------------------------------------------------
		public bool Pressed
		{
			get { return pressed; }
			set 
			{
				if (pressed == value)
					return;
		
				pressed = value;
				if (!pressed)
				{
					this.Visible = false;
					this.BackgroundImage = null;
					//this.BackColor = Color.Transparent;
					this.Visible = true;
				}
				else
					Refresh();
			}
		}
	
    }
}
