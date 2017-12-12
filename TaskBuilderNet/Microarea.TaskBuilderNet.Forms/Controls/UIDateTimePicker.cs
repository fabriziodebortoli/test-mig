using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls;
using Telerik.WinControls.UI;


namespace Microarea.TaskBuilderNet.Forms.Controls
{
	public class UIDateTimePicker : RadDateTimePicker, IUIControl, ITBBindableObject, IUIHostingControl, IUIGridEditorControl
	{
        TBWFCUIDateTimePicker cui;

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }

		//-------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public object UIValue { get { return base.Value; } set { base.Value = (DateTime)value; } }

		//-------------------------------------------------------------------------
		public object GetFocusableElement()
		{
			return DateTimePickerElement;
		}

		//-------------------------------------------------------------------------
		public UIDateTimePicker()
		{
            ThemeClassName = typeof(RadDateTimePicker).ToString();
            cui = new TBWFCUIDateTimePicker(this); 
		}

		//-------------------------------------------------------------------------
		public void FireValidated()
		{
			OnValidated(EventArgs.Empty);
		}

		//-------------------------------------------------------------------------
		public void FireValidating(CancelEventArgs e)
		{
			OnValidating(e);
		}

		//-------------------------------------------------------------------------
		public string DefaultBindingProperty
		{
			get { return "Value"; }
		}

		//-------------------------------------------------------------------------
		public Control HostedControl
		{
			get { return this.DateTimePickerElement.TextBoxElement.TextBoxItem.HostedControl; }
		}

		[Browsable(false)]
		[Obsolete("do not use RootElement")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-------------------------------------------------------------------------
		public new RootRadElement RootElement { get { return base.RootElement; } }

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

