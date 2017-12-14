using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;

namespace Microarea.TaskBuilderNet.Forms.InputStrategies
{
    //==================================================================
	public class TBInputStrategy : ITBInputStrategy, IDisposable
	{
		//public enum InputTypes
		//{
		//    All = 0,
		//    Blanks = 1,
		//    Numbers = 2,
		//    Letters = 16,
		//    OtherChars = 32
		//};

		protected TBCUIControl tbControl;
		//InputTypes inputTypes;
		int maxLength;
        

		bool isUpperCase;
		bool isLowerCase;

		public int MaxLength { get { return maxLength; } set { maxLength = value; OnInputStrategyChanged(); } }
		public bool IsUpperCase { get { return isUpperCase; } set { isUpperCase = value; OnInputStrategyChanged(); } }
		public bool IsLowerCase { get { return isLowerCase; } set { isLowerCase = value; OnInputStrategyChanged(); } }
		//public InputTypes AllowedInput { get { return inputTypes; } set { inputTypes = value; OnInputStrategyChanged(); } }
  
		//------------------------------------------------------------------
		public TBInputStrategy(TBCUIControl control)
		{
			this.tbControl = control;
			this.tbControl.BeforeValidating +=new System.EventHandler(WinControl_BeforeValidating);
		}

        //------------------------------------------------------------------
        public virtual void ApplyTo()
        {
            MDataObj dataObj = tbControl.DataBinding.Data as MDataObj;

            InitializeDataFromDataType(dataObj);
            InitializeDataFromFormatter(tbControl.Formatter);
            ApplyToControl();
       }

		//------------------------------------------------------------------
		protected virtual void ApplyToControl()
		{
			TBWFCUIControl cui = tbControl as TBWFCUIControl;
			if (cui == null)
				return;

			IMaskedInput ctrl = cui.Control as IMaskedInput;
			if (ctrl == null)
			{
				Debug.Assert(false, "OOOPPS! Sembra che sia arrivato un control con una input strategy ma che non implementa l'interfaccia IMaskedInput");
				return;
			}

			string text = string.Empty;

			if (ctrl is IMaskedInput)
			{
				text = ctrl.Text;
				ApplyToEditableControl(cui, ctrl);
			}

			if (cui.Control is UIDateTimePicker)
				ApplyToUIDateTimePicker(cui);

			if (NeedMask(tbControl))
				ctrl.Text = tbControl.Formatter.UnformatData(text);

 		}
		
		//------------------------------------------------------------------
		void WinControl_BeforeValidating(object sender, System.EventArgs e)
		{
			DetachFromControl();
		}

		//------------------------------------------------------------------
		protected void InitializeDataFromDataType(MDataObj data)
		{
			if (data.DataType.Type == DataType.String.Type)
			{
				MDataStr dataStr = data as MDataStr;
				MaxLength = dataStr.MaxLength;
				IsUpperCase = dataStr.UpperCase;
			}
		}

		//------------------------------------------------------------------
		protected void InitializeDataFromFormatter(ITBFormatterProvider formatter)
		{
			if (formatter == null)
				return;

			MaxLength = formatter.MaxInputLength;
		}

		//------------------------------------------------------------------
		protected void OnInputStrategyChanged()
		{
			ApplyToControl();
		}

		//------------------------------------------------------------------
		private void ApplyToEditableControl(TBWFCUIControl control, IMaskedInput uiEditableControl)
		{
			//Se il controll oha gia una mask, deve prevalere e quindi non sovrascritta 
			if (uiEditableControl.MaskStatus == UIMaskStatus.None)
			{
				uiEditableControl.MaskStatus = !string.IsNullOrEmpty(uiEditableControl.Mask) ? UIMaskStatus.CustomMask : UIMaskStatus.None;
			}
			if (uiEditableControl == null || uiEditableControl.MaskStatus == UIMaskStatus.CustomMask)
			{
				return;
			}

			if (IsUpperCase)
				uiEditableControl.CharacterCasing = CharacterCasing.Upper;
			else if (IsLowerCase)
				uiEditableControl.CharacterCasing = CharacterCasing.Lower;

			uiEditableControl.MaxLength = MaxLength;

			if (NeedTimeMask(control.DataBinding.DataType))
			{
				string s = tbControl.Formatter.NumberFormatString;
				uiEditableControl.UIMaskType = Microarea.TaskBuilderNet.Interfaces.View.MaskType.Numeric;
				uiEditableControl.Mask = s;
				uiEditableControl.MaskStatus = UIMaskStatus.TbMaskAdded;
			}

			if (NeedNumericMask(control.DataBinding.DataType))
			{
				//usiamo numeric anche per il money perche il currency telerik non accetta il simbolo vuoto (e noi non vogliamo mostrarlo)
				CultureInfo ci = CultureInfo.CreateSpecificCulture(Thread.CurrentThread.CurrentCulture.Name);
				ci.NumberFormat = tbControl.Formatter.NumberFormatInfo;
				uiEditableControl.Culture = ci;
				uiEditableControl.UIMaskType = Microarea.TaskBuilderNet.Interfaces.View.MaskType.Numeric;
				uiEditableControl.Mask = string.Format(CultureInfo.InvariantCulture , "n{0}", tbControl.Formatter.DecimalDigitNumber);
				uiEditableControl.MaskStatus = UIMaskStatus.TbMaskAdded;
			}

			if (NeedPercentMask(control.DataBinding.DataType))
			{
				CultureInfo ci = CultureInfo.CreateSpecificCulture(Thread.CurrentThread.CurrentCulture.Name);
				ci.NumberFormat = tbControl.Formatter.NumberFormatInfo;
				uiEditableControl.Culture = ci;
				uiEditableControl.UIMaskType = Microarea.TaskBuilderNet.Interfaces.View.MaskType.Numeric;
				uiEditableControl.Mask = string.Format(CultureInfo.InvariantCulture, "p{0}", tbControl.Formatter.DecimalDigitNumber);
				uiEditableControl.MaskStatus = UIMaskStatus.TbMaskAdded;
			}
		}

		//------------------------------------------------------------------
		private static bool NeedPercentMask(IDataType type)
		{
			return type.Type == DataType.Percent.Type;
		}

		//------------------------------------------------------------------
		private static bool NeedTimeMask(IDataType type)
		{
			return type.IsATime();
		}

		//------------------------------------------------------------------
		internal static bool NeedMask(TBCUIControl control)
		{
			return NeedNumericMask(control.DataBinding.DataType) || NeedPercentMask(control.DataBinding.DataType) || NeedTimeMask(control.DataBinding.DataType);
		}

		//------------------------------------------------------------------
		private static bool NeedNumericMask(IDataType type)
		{
			return type.Type == DataType.Double.Type
                ||
                type.Type == DataType.Money.Type
                ||
                type.Type == DataType.Quantity.Type
                ||
                type.Type == DataType.Integer.Type
                ||
                type.Type == DataType.Long.Type;
		}

		//------------------------------------------------------------------
		private void ApplyToUIDateTimePicker(TBWFCUIControl control)
		{
			UIDateTimePicker dateTimePicker = control.Control as UIDateTimePicker;
			if (dateTimePicker != null && control.DataBinding.DataType.Type == DataType.Date.Type)
			{
				dateTimePicker.CustomFormat = tbControl.Formatter.CustomDateFormatForEdit;
			}
		}

		//------------------------------------------------------------------
		protected void DetachFromControl()
		{
			TBWFCUIControl cui = tbControl as TBWFCUIControl;
			if (cui == null)
				return;
			IMaskedInput maskedControl = cui.Control as IMaskedInput;

			if (maskedControl.MaskStatus == UIMaskStatus.TbMaskAdded && maskedControl != null)
			{
				string s = maskedControl.Text;
				maskedControl.UIMaskType = Microarea.TaskBuilderNet.Interfaces.View.MaskType.None;
				maskedControl.Text = s;
				maskedControl.MaskStatus = UIMaskStatus.TbMaskRemoved;
			}
		}

		//------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (tbControl != null)
				{
					tbControl.BeforeValidating -= new System.EventHandler(WinControl_BeforeValidating);
				}
			}
		}
	}

}
