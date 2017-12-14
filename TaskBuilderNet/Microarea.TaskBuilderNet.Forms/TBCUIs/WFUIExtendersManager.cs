using System.Diagnostics;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;

namespace Microarea.TaskBuilderNet.Forms
{
	//=========================================================================
	/// <summary>
	/// WFUIExtendersManager
	/// </summary>
	internal class WFUIExtendersManager : UIExtendersManager
	{
		//---------------------------------------------------------------------
		public WFUIExtendersManager(TBCUIControl controller)
			: base(controller)
		{

		}

		//---------------------------------------------------------------------
		protected override void AddSibling(IUIControl control, IUIControl newSibling)
		{
			Control  extendee = control as Control;
			Debug.Assert((extendee != null), "OOOOPSSS! Il control da estendere non e` un System.Windows.Forms.Control");
    
			Control  control3 = newSibling as Control;
			Debug.Assert((control3 != null), "OOOOPSSS! Il control che estende non e` un System.Windows.Forms.Control");

			extendee.Parent.Controls.Add(control3);
		}

		//---------------------------------------------------------------------
		protected override void RemoveSibling(IUIControl control, IUIControl sibling)
		{
			Control  extendee = control as Control;
			Debug.Assert((extendee != null), "OOOOPSSS! Il control da estendere non e` un System.Windows.Forms.Control");
    
			Control  control3 = sibling as Control;
			Debug.Assert((control3 != null), "OOOOPSSS! Il control che estende non e` un System.Windows.Forms.Control");

			extendee.Parent.Controls.Remove(control3);
		}
	}
}
