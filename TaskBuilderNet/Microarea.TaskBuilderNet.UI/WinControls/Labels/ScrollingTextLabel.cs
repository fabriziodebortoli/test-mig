using System;
using System.Drawing;

namespace Microarea.TaskBuilderNet.UI.WinControls.Labels
{
	/// <summary>
	/// Summary description for ScrollingTextLabel.
	/// </summary>
	//================================================================================
	public partial class ScrollingTextLabel : System.Windows.Forms.UserControl
	{
		private int speed = 3; 

		//--------------------------------------------------------------------------------
		public ScrollingTextLabel()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			timer.Start();

		}

        //--------------------------------------------------------------------------------
		private void Timer_Tick(object sender, EventArgs e)
		{
			if (DesignMode) return;

			textLabel.Location = textLabel.Location - new Size(speed, 0);
			if (textLabel.Right <= 0)
				textLabel.Location = new Point(this.Size.Width, textLabel.Location.Y);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Sets or gets the speed of the scrolling text (in pixels per 'Interval' unit of time)
		/// </summary>
		public int Speed { get { return speed; } set { speed = value; }}
		
		/// <summary>
		/// Sets or gets the interval, in milliseconds, between each adjustment of text position
		/// </summary>
		//--------------------------------------------------------------------------------
		public int Interval {get { return timer.Interval; } set { timer.Interval = value; }}

		/// <summary>
		/// Sets or gets scrolling text
		/// </summary>
		//--------------------------------------------------------------------------------
		public string ScrollingText {get { return textLabel.Text; } set { textLabel.Text = value; }}

	}
}
