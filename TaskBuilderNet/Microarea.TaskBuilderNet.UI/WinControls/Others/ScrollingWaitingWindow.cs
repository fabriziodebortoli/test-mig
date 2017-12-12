using System;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	/// <summary>
	/// Summary description for ScrollingWaitingWindow.
	/// </summary>
	//================================================================================
	public partial class ScrollingWaitingWindow : System.Windows.Forms.Form
	{
		private Delegate method;
		private object[] parameters;
		bool firstTime = true;

		public object ReturnValue = null;

		//--------------------------------------------------------------------------------
		public ScrollingWaitingWindow(Delegate method, object[] parameters, string waitMessage)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.messageBox.ScrollingText = waitMessage;
			this.method = method;
			this.parameters = parameters;
			
		}

		//--------------------------------------------------------------------------------
		private void ScrollingWaitingWindow_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			if (firstTime)
			{
				firstTime = false;
				ReturnValue = Invoke(method, parameters);
				Close();
			}
		}
	}
}
