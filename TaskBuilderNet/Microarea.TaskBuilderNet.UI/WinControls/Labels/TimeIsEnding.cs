using System;

namespace Microarea.TaskBuilderNet.UI.WinControls.Labels
{
	/// <summary>
	/// Summary description for TimeIsEnding.
	/// </summary>
	public partial class TimeIsEnding : System.Windows.Forms.UserControl
	{
    	public	int	TotalTime	= 0;
        private int startTick	= Environment.TickCount;

		public TimeIsEnding()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

    	//---------------------------------------------------------------------------
		private void timer1_Tick(object sender, System.EventArgs e)
		{
			int currentTick = Environment.TickCount;
			int timeGone = (currentTick - startTick) / 1000;
			labelLeftTime.Text	= ((int)(TotalTime - timeGone)).ToString();
			labelGoneTime.Text	= timeGone.ToString();
		}

		//---------------------------------------------------------------------------
		public void Start(int TotalTime)
		{
			timer1.Start();
			this.TotalTime	= TotalTime * 60;
			startTick		= Environment.TickCount;

			labelTotalTime.Text = TotalTime.ToString();
			labelLeftTime.Text	= TotalTime.ToString();
			labelGoneTime.Text	= "0";
		}
	}
}
