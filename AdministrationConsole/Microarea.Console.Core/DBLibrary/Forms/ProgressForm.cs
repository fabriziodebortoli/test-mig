using System.Windows.Forms;

namespace Microarea.Console.Core.DBLibrary.Forms
{
	//=========================================================================
	public partial class ProgressForm : Form
	{
		public bool ElaborationInProgress { get; set; }

		//---------------------------------------------------------------------
		public ProgressForm()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		private void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = ElaborationInProgress;
		}

		//---------------------------------------------------------------------
		private void ProgressForm_Load(object sender, System.EventArgs e)
		{
			ElaborationInProgress = true;
		}
	}
}
