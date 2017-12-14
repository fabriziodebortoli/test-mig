using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	/// <summary>
	/// Summary description for AskWindow.
	/// </summary>
	//================================================================================
	public partial class AskWindow : System.Windows.Forms.Form
	{
		private DialogResult latestResult = DialogResult.Cancel;
		
		private bool isQuestion = true;

		//--------------------------------------------------------------------------------
		public string Message { get { return txtQuestion.Text; } set { txtQuestion.Text = value; } }

		//--------------------------------------------------------------------------------
		public string Caption { get { return this.Text; } set { this.Text = value; } }

		//--------------------------------------------------------------------------------
		public string DoNotRepeatCaption { get { return ckbDoNotAsk.Text; } set { ckbDoNotAsk.Text = value; } }

		//--------------------------------------------------------------------------------
		public bool IsQuestion 
		{
			get { return isQuestion; }
			set 
			{
				isQuestion = value;				
				btnCancel.Visible = isQuestion;
				btnNo.Visible = isQuestion;
				btnYes.Visible = isQuestion;
				btnOk.Visible = !isQuestion;

				ckbDoNotAsk.Text = isQuestion
					? WinControlsStrings.DoNotAskAnymore
					: WinControlsStrings.DoNotMessageAnymore;
			}
		}

		//--------------------------------------------------------------------------------
		public AskWindow(string question, string caption)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			if (question != null)
				txtQuestion.Text = question;
			
			if (caption != null)
				Text = caption;
		}

		//--------------------------------------------------------------------------------
		public AskWindow(string question)
			: this (question, null)
		{
		}

		//--------------------------------------------------------------------------------
		public AskWindow()
			: this (null, null)
		{
		}

		//--------------------------------------------------------------------------------
		public new DialogResult ShowDialog()
		{
			return ShowDialog(null);
		}

		//--------------------------------------------------------------------------------
		public new DialogResult ShowDialog(IWin32Window owner)
		{
			if (ckbDoNotAsk.Checked) return latestResult;

			latestResult = base.ShowDialog(Owner);

			return latestResult;
		}


	}
}
