﻿using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
    public class UIAutoCompleteBox : RadAutoCompleteBox, IUIControl, ITBBindableObject
	{
        TBWFCUIControl cui;

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }
	
        //-------------------------------------------------------------------------
        public UIAutoCompleteBox()
		{
			ThemeClassName = typeof(RadAutoCompleteBox).ToString();
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

        //-------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public object UIValue
        {
            get { return this.Text; }
        }

        //-------------------------------------------------------------------------
        public string DefaultBindingProperty
        {
            get { return "Text"; }
        }
    }
}
