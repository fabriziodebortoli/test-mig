using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Forms
{
    //================================================================================================================
    /// <summary>
    /// </summary>
    internal class TBWFCUIDateTimePicker : TBWFCUIControl
    {
        private UIDateTimePicker Picker { get { return Component as UIDateTimePicker; } }
     
        //---------------------------------------------------------------------------
        public TBWFCUIDateTimePicker(UIDateTimePicker control)
            :
            base(control, NameSpaceObjectType.Control)
        {}

		//---------------------------------------------------------------------------
		void InnerControl_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Picker.CustomFormat = Formatter == null ? null : Formatter.CustomDateFormat;
		}

        //---------------------------------------------------------------------------
        protected override void OnInitialize()
        {
            base.OnInitialize();

			UIDateTimePicker picker = Picker;

			picker.Validating += new System.ComponentModel.CancelEventHandler(InnerControl_Validating);

			picker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			picker.NullDate = MDataDate.NullDate;
			picker.NullText = "";
			picker.MinDate = MDataDate.MinDate;
			picker.MaxDate = MDataDate.MaxDate;

			picker.CustomFormat = Formatter == null ? null : Formatter.CustomDateFormat;         
        }

		//---------------------------------------------------------------------------
		protected override void Dispose(bool value)
		{
			base.Dispose(value);

			UIDateTimePicker picker = Picker;
			if (picker != null)
			{
				picker.Validating -= new System.ComponentModel.CancelEventHandler(InnerControl_Validating);
			}
		}
    }
}

