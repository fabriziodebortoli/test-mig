using System.Windows.Forms;

namespace Microarea.Console.Core.PlugIns
{
	/// <summary>
	/// ConsoleGUIObjects
	/// Elementi grafici passati ai PlugIns quali
	/// - Tree
	/// - Menu
	/// - workingArea
	/// - workingAreabutton
	/// </summary>
	// ========================================================================
	public class ConsoleGUIObjects
	{
        private System.Windows.Forms.MenuStrip  menuConsole;
		private PlugInsTreeView                 treeConsole;
		private Panel			                wkgAreaConsole;
		private Panel			                bottomWkgAreaConsole;

		public System.Windows.Forms.MenuStrip   MenuConsole				{ get { return menuConsole;				} set { menuConsole = value;			}}
		public PlugInsTreeView								 TreeConsole				{ get { return treeConsole;				} set { treeConsole = value;			}}
		public Panel			                WkgAreaConsole			{ get { return wkgAreaConsole;			} set { wkgAreaConsole = value;			}}
		public Panel			                BottomWkgAreaConsole	{ get { return bottomWkgAreaConsole;	} set { bottomWkgAreaConsole = value;	}}

		//---------------------------------------------------------------------
		public ConsoleGUIObjects()
		{

		}
	}
}
