using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
    //=========================================================================
    [ToolboxItem(true)]
    public class UISeparator : RadSeparator , IUIControl
    {
        TBCUI cui;

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }
        
        //-------------------------------------------------------------------------
        public UISeparator()
    	{
            ThemeClassName = typeof(RadSeparator).ToString();
            cui = new TBCUI(this, Interfaces.NameSpaceObjectType.Control); 
		}
    }
}
