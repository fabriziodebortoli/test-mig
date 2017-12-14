using System.Drawing;
using System.Drawing.Drawing2D;

namespace Microarea.TaskBuilderNet.UI.WinControls.Labels
{
	/// <summary>
	/// Summary description for FadeLabel.
	/// </summary>
	public partial class FadeLabel : System.Windows.Forms.UserControl
	{
		public FadeLabel()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		private string labelText = "Testo della label";
		public string LabelText
		{
			get
			{
				return labelText;
			}
			set
			{
				labelText = value;
			}
		}

		private int angle = 120;
		public int Angle
		{
			get
			{
				return angle;
			}
			set
			{
				angle = value;
			}
		}

		private Color endColor, startColor, textColor;
		public Color EndColor
		{
			get
			{
				return endColor;
			}
			set
			{
				endColor = value;
			}
		}

		public Color StartColor
		{
			get
			{
				return startColor;
			}
			set
			{
				startColor = value;
			}
		}


		public Color TextColor
		{
			get
			{
				return textColor;
			}
			set
			{
				textColor = value;
			}
		}

		private void FadeLabel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Rectangle rect = new Rectangle(2, 2, this.Width, this.Height);
			LinearGradientBrush brush = new LinearGradientBrush(rect, EndColor, StartColor, Angle);
			Pen pen = new Pen(brush, this.Width);
			e.Graphics.DrawRectangle(pen, rect);

			LinearGradientBrush drawBrush = new LinearGradientBrush(rect, TextColor, TextColor, 10);
			e.Graphics.DrawString(labelText, Font, drawBrush, 10, 10);
		}
	}
}
