using System;
using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=========================================================================
    public class UIGroupBox : RadGroupBox, IUIContainer
    {
        TBCUI cui;

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }
        //-------------------------------------------------------------------------
        public System.Collections.IList ChildControls
        {
            get { return base.Controls; }
        }

		//---------------------------------------------------------------------
        public UIGroupBox()
        {
            ThemeClassName = typeof(RadGroupBox).ToString();
            cui = new TBCUI(this, Interfaces.NameSpaceObjectType.Control); 
        }

        //-----------------------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (cui != null)
            {
                cui.Dispose();
                cui = null;
            }
        }

		[Browsable(false)]
		[Obsolete("do not use RootElement")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-------------------------------------------------------------------------
		public new RootRadElement RootElement { get { return base.RootElement; } }
    }
}
