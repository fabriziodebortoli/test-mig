using System;
using System.Windows.Forms;

namespace ManifestGenerator
{
	//================================================================================
	public partial class Output : Form
	{
		bool detailVisible = false;
		int detailHeight = 0;
		//--------------------------------------------------------------------------------
		public Output ()
		{
			Application.EnableVisualStyles();
			Application.DoEvents();
			InitializeComponent();
		}

		//--------------------------------------------------------------------------------
		private void Output_Load (object sender, EventArgs e)
		{
			ShowDetail(false);
			ProgressBar.MarqueeAnimationSpeed = 100;
		}

		//--------------------------------------------------------------------------------
		private void ShowDetail (bool show)
		{
			if (show)
			{
				this.LogTextBox.Visible = true;
				this.DetailButton.Text = Resource.ButtonHideDetailCaption;
				detailVisible = true;

				this.Height += detailHeight;
			}
			else
			{
				this.LogTextBox.Visible = false;
				this.DetailButton.Text = Resource.ButtonMoreDetailCaption;
				detailVisible = false;

				detailHeight = LogTextBox.Height;

				this.Height -= detailHeight;
			}
		}

		//--------------------------------------------------------------------------------
		private void DetailButton_Click (object sender, EventArgs e)
		{
			ToggleDetail();
		}

		//--------------------------------------------------------------------------------
		private void ToggleDetail ()
		{
			ShowDetail(!detailVisible);
		}

		//--------------------------------------------------------------------------------
		internal void PerformStep ()
		{
			ProgressBar.PerformStep();
		}

		//--------------------------------------------------------------------------------
		internal void SetProgressTop (int top)
		{
			int consumedSteps = ProgressBar.Value / ProgressBar.Step;

			ProgressBar.Step = 1;
			
			ProgressBar.Maximum = top;
			ProgressBar.Minimum = 0;
			ProgressBar.Value = consumedSteps;
		}
	}
}
