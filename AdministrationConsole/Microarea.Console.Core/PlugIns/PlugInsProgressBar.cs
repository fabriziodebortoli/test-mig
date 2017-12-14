using System;
using System.Windows.Forms;

namespace Microarea.Console.Core.PlugIns
{
	/// <summary>
	/// PlugInsProgressBar.
	/// ProgressBar inserita nella MicroareaConsole e utilizzata da tutti i PlugIns che ne richiedono l'uso
	/// </summary>
	//=========================================================================
	public partial class PlugInsProgressBar : System.Windows.Forms.UserControl
	{
		private int		minValue		= 1;
		private int		maxValue		= 100;
		private int		stepValue		= 1;
		private int		currentValue	= 1;
		private string	textOfProgress	= string.Empty;

		public int		MaxValue		{ get { return maxValue;		} set { maxValue		= value; plugInProgressBar.Maximum	= value; }}
		public int		MinValue		{ get { return minValue;		} set { minValue		= value; plugInProgressBar.Minimum	= value; }}
		public int		CurrentValue	{ get { return currentValue;	} set { currentValue	= value; plugInProgressBar.Value	= value; }}
		public int		StepValue		{ get { return stepValue;		} set { stepValue		= value; plugInProgressBar.Step		= value; }}
		public string	TextProgressBar { get { return textOfProgress;	} set { textOfProgress	= value; textProgressBar.Text		= value; }}

		/// <summary>
		/// PlugInsProgressBar
		/// Costruttore - Inizializza la ProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		public PlugInsProgressBar()
		{
			InitializeComponent();
		}

		/// <summary>
		/// SetText
		/// Setta il messaggio accanto alla progress Bar
		/// </summary>
		//---------------------------------------------------------------------
		public void SetText(string messageToDisplay)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetText(messageToDisplay); });
				return;
			}

			TextProgressBar = messageToDisplay;
		}

		/// <summary>
		/// SetMaxValue
		/// Imposta il MaxValue della ProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		public void SetMaxValue(int maxValue)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetMaxValue(maxValue); });
				return;
			}

			MaxValue = maxValue;
		}

		/// <summary>
		/// SetMinValue
		/// Imposta il MinValue della ProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		public void SetMinValue(int minValue)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetMinValue(minValue); });
				return;
			}

			MinValue = minValue;
		}

		/// <summary>
		/// SetValue
		/// Imposta il currentValue della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		public void SetValue(int currentValue)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetValue(currentValue); });
				return;
			}

			CurrentValue = currentValue;
		}

		/// <summary>
		/// SetStep
		/// Imposta lo step della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		public void SetStep(int step)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetStep(step); });
				return;
			}

			StepValue = step;
		}

		/// <summary>
		/// SetProgressVisibility
		/// Imposta la visiblità o meno della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		public void SetProgressVisibility(bool visible)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetProgressVisibility(visible); });
				return;
			}

			plugInProgressBar.Visible = visible; 
		}

		/// <summary>
		/// SetProgressTextVisibility
		/// Imposta la visiblità o meno del messaggio 
		/// </summary>
		//---------------------------------------------------------------------
		public void SetProgressTextVisibility(bool visible)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetProgressTextVisibility(visible); });
				return;
			}

			textProgressBar.Visible = visible;
		}

        //---------------------------------------------------------------------
        public void SetProgressBarValue(int valueToSet)
        {
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetProgressBarValue(valueToSet); });
				return;
			}

			plugInProgressBar.Value = valueToSet;
        }

		/// <summary>
		/// IncrementCyclicStep
		/// Incrementa di step passi e quando raggiunge il MaxValue ricomincia
		/// </summary>
		//---------------------------------------------------------------------
		public void IncrementCyclicStep()
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { IncrementCyclicStep(); });
				return;
			}

			if (plugInProgressBar.Value == MaxValue)
			{
				SetProgressBarValue(1);
				CurrentValue = 1;
			}
			plugInProgressBar.Increment(StepValue);
        }

		/// <summary>
		/// IncrementNotCyclicStep
		/// Incrementa di step passi e quando raggiunge il MaxValue termina
		/// </summary>
		//---------------------------------------------------------------------
		public void IncrementNotCyclicStep()
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { IncrementNotCyclicStep(); });
				return;
			}

			if (CurrentValue < MaxValue)
				plugInProgressBar.Increment(StepValue);
		}

		/// <summary>
		/// PerformStepProgressBar
		/// Effettua uno step nella Progressbar
		/// </summary>
		//---------------------------------------------------------------------
		public void PerformStepProgressBar()
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { PerformStepProgressBar(); });
				return;
			}

			plugInProgressBar.PerformStep();
		}

        //---------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AdjustControlsLayout();
        }

        //---------------------------------------------------------------------
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            AdjustControlsLayout();
        }

        //---------------------------------------------------------------------
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            AdjustControlsLayout();
        }

        //---------------------------------------------------------------------
        private void AdjustControlsLayout()
        {
            if (
				textProgressBar == null || 
				plugInProgressBar == null || 
				!this.Contains(textProgressBar) || 
				!this.Contains(plugInProgressBar)
				)
                return;

            this.SuspendLayout();

            textProgressBar.Height = plugInProgressBar.Height;
            textProgressBar.Top = (this.Height - textProgressBar.Height)/2;

            textProgressBar.Width = plugInProgressBar.Left - textProgressBar.Left - 2;

            this.ResumeLayout();
        }
    }
}