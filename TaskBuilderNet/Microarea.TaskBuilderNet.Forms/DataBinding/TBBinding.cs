using System;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;

namespace Microarea.TaskBuilderNet.Forms.DataBinding
{
	//================================================================================================================
    public class TBBinding : Binding
    {
        TBCUIControl tbCUI;
        public TBCUIControl TBControl { get { return tbCUI; } set { tbCUI = value; } }

		//-------------------------------------------------------------------------
		public TBBinding(
			string propertyName,
			object dataSource,
			string fieldInfoPropertyName,
			bool formattingEnabled,
			DataSourceUpdateMode dataSourceUpdateMode,
			IFormatProvider formatProvider
			)
			:
			base(propertyName, dataSource, fieldInfoPropertyName, formattingEnabled, dataSourceUpdateMode, null, "", formatProvider)
		{
		}

        //-------------------------------------------------------------------------
        protected override void OnFormat(ConvertEventArgs cevent)
        {
            base.OnFormat(cevent);

			//Solo se e' bindato sun un campo Text deve intervenire la nostra formattazione
            if  (
                    String.Compare(PropertyName, "Text", StringComparison.Ordinal) == 0 
                )
			{
				if (tbCUI == null || tbCUI.Formatter == null || tbCUI.DataObj == null)
					return;
				
				cevent.Value = FormatData(tbCUI.DataObj.DataType, tbCUI.Formatter, cevent.Value);
			}
        }

		//-------------------------------------------------------------------------
		public static object FormatData(DataType dataType, ITBFormatterProvider formatterProvider, object value)
		{
			using (MDataObj clone = MDataObj.Create(dataType))
			{
				clone.Value = value;

				return formatterProvider != null
					? formatterProvider.FormatData(clone)
					: value;
			}
		}
    }
}
