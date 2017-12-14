using System;
using System.ComponentModel;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls;
using Telerik.WinControls.Enumerations;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=========================================================================
	public class UIRadioButton : RadRadioButton, IUIControl, ITBBindableObject
	{
        TBWFCUIControl cui;

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }


        [Browsable(false)]
		[Obsolete("do not use ToggleStateChanged, use UIToggleStateChanged instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new event StateChangedEventHandler ToggleStateChanged;
		[Browsable(false)]
		[Obsolete("do not use ToggleStateChanging, use UIToggleStateChanging instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new event StateChangingEventHandler ToggleStateChanging;

		public event EventHandler<UIStateChangedEventArgs> UIToggleStateChanged;
		public event EventHandler<UIStateChangingEventArgs> UIToggleStateChanging;

		[Browsable(false)]
		[Obsolete("do not use RootElement")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-------------------------------------------------------------------------
		public new RootRadElement RootElement { get { return base.RootElement; } }

		//---------------------------------------------------------------------
		public UIRadioButton()
		{
            cui = new TBWFCUIControl(this, Interfaces.NameSpaceObjectType.Control);
			AttachEvents();
            ThemeClassName = typeof(RadRadioButton).ToString();
        }

		//---------------------------------------------------------------------
		private void AttachEvents()
		{
			Debug.Assert(ToggleStateChanged == null, "Se non hai capito, non devi usarlo");
			Debug.Assert(ToggleStateChanging == null, "Se non hai capito, non devi usarlo");

			base.ToggleStateChanged += new StateChangedEventHandler(UICheckBox_ToggleStateChanged);
			base.ToggleStateChanging += new StateChangingEventHandler(UICheckBox_ToggleStateChanging);
		}

		//---------------------------------------------------------------------
		private void DetachEvents()
		{
			base.ToggleStateChanged -= new StateChangedEventHandler(UICheckBox_ToggleStateChanged);
			base.ToggleStateChanging -= new StateChangingEventHandler(UICheckBox_ToggleStateChanging);
		}

		//---------------------------------------------------------------------
		[Browsable(false)]
		[Obsolete("do not use ToggleState, use UIToggleState instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ToggleState ToggleState { get { return base.ToggleState; } set { base.ToggleState = value; } }
		
		//---------------------------------------------------------------------
		public UIToggleState UIToggleState { get { return (UIToggleState)base.ToggleState; } set { base.ToggleState = (ToggleState)value; } }

		//---------------------------------------------------------------------
		void UICheckBox_ToggleStateChanging(object sender, StateChangingEventArgs args)
		{
			if (UIToggleStateChanging != null)
				UIToggleStateChanging(sender, new UIStateChangingEventArgs(args));
		}

		//---------------------------------------------------------------------
		void UICheckBox_ToggleStateChanged(object sender, StateChangedEventArgs args)
		{
			if (UIToggleStateChanged != null)
				UIToggleStateChanged(sender, new UIStateChangedEventArgs(args));

		}

        //-------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public object UIValue { get { return base.IsChecked; } }
      
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			DetachEvents();
            if (cui != null)
            {
                cui.Dispose();
                cui = null;
            }
        }

		//-------------------------------------------------------------------------
		public string DefaultBindingProperty
		{
            get { return "IsChecked"; }
		}
    }
}
