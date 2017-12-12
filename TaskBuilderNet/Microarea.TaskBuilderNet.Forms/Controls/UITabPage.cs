using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	[ToolboxItem(false)]
    public class UITabPage : RadPageViewPage, IUIContainer
	{
        TBCUI cui;

         [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }

        //-------------------------------------------------------------------------
        public UITabPage()
        {
            cui = new TBCUI(this, Interfaces.NameSpaceObjectType.TabDialog);
 		}

        //-------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (cui != null)
            {
                cui.Dispose();
                cui = null;
            }
        }
        //-------------------------------------------------------------------------
        public System.Collections.IList ChildControls
        {
            get { return base.Controls; }
        }
    }
}
