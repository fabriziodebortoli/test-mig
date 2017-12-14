using System;
using Microarea.TaskBuilderNet.Forms.Controls;
using Telerik.WinControls.UI.Design;

namespace Microarea.TaskBuilderNet.Forms.Designers
{
	public class UIGridColumnsDesigner : GridViewColumnCollectionEditor
	{

		public UIGridColumnsDesigner(Type type)
		: base(type)
		{
		}
		
		protected override Type[] CreateNewItemTypes()
		{
			Type[] types = new Type[4];
			types[0] = typeof(UITextBoxColumn);
			types[1] = typeof(UICheckBoxColumn);
			types[2] = typeof(UIDropDownColumn);
			types[3] = typeof(UIDateTimeColumn);

			return types;

		}
	}
}