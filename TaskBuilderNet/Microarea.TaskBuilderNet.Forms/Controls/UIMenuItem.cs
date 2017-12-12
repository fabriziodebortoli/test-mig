using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//================================================================================================================
	public class UIMenuItem : RadMenuItem, IUIMenuItem
	{
		//-------------------------------------------------------------------------
		public UIMenuItem()
		{ 
		
		}

		//-------------------------------------------------------------------------
		public UIMenuItem(string text) 
			: base(text)
		{

		}

		//-------------------------------------------------------------------------
		public UIMenuItem(string text, object tag)
			: base(text, tag)
		{
			this.ThemeRole = typeof(RadMenuItem).Name;
		}
	}
}
