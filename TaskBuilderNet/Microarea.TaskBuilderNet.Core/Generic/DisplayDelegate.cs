using System;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Every Control has the property InvokeRequired - this property will be
	/// true if you're accessing it from any thread that's not the UI thread.
	/// If it is true, you must not directly call any other of the Control's
	/// methods but instead have to call Control.Invoke() which pushes the
	/// method call on to the UI-Thread.
	/// See: http://www.ingorammer.com/RemotingFAQ/HANDLING_EVENTS_HANGS_APPLICATION.html
	/// </summary>
	//=========================================================================
	public class DisplayHelper
	{
		delegate void DisplayDelegate(String text, Control aControl);

		//---------------------------------------------------------------------
		private static void InternalSafeDisplay(String text, Control aControl)
		{
			if (aControl.GetType() == typeof(ProgressBar))
				((ProgressBar)aControl).Increment(Int32.Parse(text));

			if (aControl.GetType() == typeof(RichTextBox))
				((RichTextBox)aControl).AppendText(text);
		}

		//---------------------------------------------------------------------
		public static void SafeDisplay(String text, Control aControl)
		{
			if (aControl.InvokeRequired)
			{
				aControl.Invoke
				(
					new DisplayDelegate(InternalSafeDisplay),
					new Object[] {text, aControl}
				);
			}
			else
			{
				if (aControl.GetType() == typeof(ProgressBar))
					((ProgressBar)aControl).Increment(Int32.Parse(text));

				if (aControl.GetType() == typeof(RichTextBox))
					((RichTextBox)aControl).AppendText(text);
			}
		}
	}
}