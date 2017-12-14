using System;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	/// <summary>
	/// Control that implements the Close event 
	/// </summary>
	//=========================================================================
	public partial class ClosableControl : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		public event EventHandler Close;

		//---------------------------------------------------------------------
		public ClosableControl()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		protected virtual void OnClose(object sender, EventArgs e)
		{
			if (Close!= null)
				Close(sender, e);
		}
	}
}
