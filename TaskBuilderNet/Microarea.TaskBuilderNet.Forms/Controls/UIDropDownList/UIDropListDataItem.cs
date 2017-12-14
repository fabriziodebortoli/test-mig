using System.ComponentModel;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=============================================================================================
	public class UIDropListDataItem : RadListDataItem
	{
		//-----------------------------------------------------------------------------------------
		public UIDropListDataItem()
			:
			base()
		{

		}

		//-----------------------------------------------------------------------------------------
		public UIDropListDataItem(string text, object value)
			:
			base(text, value)
		{
		}

		//-----------------------------------------------------------------------------------------
		[DefaultValue(true)]
		public new bool TextWrap
		{
			get { return base.TextWrap; }
			set { base.TextWrap = value; }
		}
	}
}
