using System;
using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=========================================================================
	[ToolboxItem(true)]
    public class UIPanel : RadPanel, IUIContainer
	{
        TBCUI cui;
		
        [Browsable(false)]
		virtual public ITBCUI CUI { get { return cui; } }

        //-------------------------------------------------------------------------
		public UIPanel()
    	{
            ThemeClassName = typeof(RadPanel).ToString();
            cui = new TBCUI(this, Interfaces.NameSpaceObjectType.Control); 
		}

		[Browsable(false)]
		[Obsolete("do not use RootElement")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-------------------------------------------------------------------------
		public new RootRadElement RootElement { get { return base.RootElement; } }
       
		//-------------------------------------------------------------------------
        public System.Collections.IList ChildControls
        {
            get { return base.Controls; }
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
	}
}
