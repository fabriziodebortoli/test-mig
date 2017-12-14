using System.Windows.Forms;

using Microarea.TaskBuilderNet.UI.WinControls;

namespace Microarea.Tools.TBLocalizer.CommonUtilities
{
	/// <summary>
	/// Summary description for WaitingWindowManager.
	/// </summary>
	public class WaitingWindowManager
	{
		private WaitingWindow window;
		private int count = 0;

		//---------------------------------------------------------------------
		public void OpenWaitingWindow(IWin32Window owner, string message)
		{
			if (count++ == 0)
			{
				window = new WaitingWindow(message);
				window.BringToFront();
				window.Capture = true;
				window.Show();
			}
		}
		
		//---------------------------------------------------------------------
		public void CloseWaitingWindow()
		{
			if (--count == 0)
			{
				window.Close();
				window = null;
			}
		}
	}
}
