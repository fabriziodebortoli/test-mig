using System;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.CodeEditor.Woorm
{
	public partial class OperatorPad : UserControl
	{
		public event EventHandler<OperatorSelectedEventArgs> OperatorSelected;

		public OperatorPad()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, EventArgs e)
		{
			if (OperatorSelected != null)
			{
				OperatorSelectedEventArgs args = new OperatorSelectedEventArgs();
				args.Operator = ((Button)sender).Text;
				OperatorSelected(this, args);
			}
		}
	}
	
	public class OperatorSelectedEventArgs : EventArgs
	{
		public string Operator { get; set; }
	}

	
}
