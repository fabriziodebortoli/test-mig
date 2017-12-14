using System;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	public interface ITBDockContent<out T> where T : Control
	{
		T HostedControl { get; }

		void Close();
	}
}