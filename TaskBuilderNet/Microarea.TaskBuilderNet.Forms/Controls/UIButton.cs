using System;
using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
    public class UIButton : RadButton, IUIControl
	{
        TBWFCUIControl cui;

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }
	
        //-------------------------------------------------------------------------
		public UIButton()
		{
			ThemeClassName = typeof(RadButton).ToString();
            cui = new TBWFCUIControl(this, Interfaces.NameSpaceObjectType.Control);
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
		[Browsable(false)]
		[Obsolete("do not use RootElement")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-------------------------------------------------------------------------
		public new RootRadElement RootElement { get { return base.RootElement; } }
	}
}
